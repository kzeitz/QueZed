using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ContactsMigration {
    using System.Drawing;
    using System.Reflection;
    using System.IO;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using ContactsClassic;
    using TCS.Models;
    using TCS.Common.Enumerations;
    using ClassicHotKeyControl;
    using System.Collections.ObjectModel;

    public static partial class ExtentionMethods {
        public static string Truncate(this string value, int maxLength, string fieldName) {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length > maxLength) Migrate.CConsole.WriteLineBicolor(ConsoleColor.Red, ConsoleColor.DarkYellow, "  Value : [{0}] for Field: [{1}] was truncated.", value, fieldName);
            return value.Substring(0, Math.Min(value.Length, maxLength));
        }
    }

    public partial class Migrate {
        private static ICConsole cConsole = null;
        public Migrate(ICConsole cConsole) { Migrate.cConsole = cConsole; }
        public static ICConsole CConsole { get { return cConsole; } }
        public static List<ContactPermissionModel> contactPermissions = new List<ContactPermissionModel>();
        public static List<ContactPermissionModel> existingcontactPermissions = new List<ContactPermissionModel>();
        enum settings { System = 3061, PSAP = 3069, Personal = 3062 }
        enum eIContactTypes { None = 0, Contact = 1, PSAP = 2, Station = 3, Agent = 4, Queue = 5 }
        enum eIContactLinkTypes { None = 0, Contact = 1, PSAP = 2, Station = 3, Agent = 4, Queue = 5, Folder = 6 }

        // nasty globals
        public static string originalAssemblyName = string.Empty;

        static string connectionString = string.Empty;
        static Guid contactTypeGuid;
        static bool saveSettingRecord = false;
        static bool clean = false;
        static bool print = false;

        public void Run(string connectionString, string option) {
            cConsole.WriteLine("Start.", ConsoleColor.Green);
            Migrate.connectionString = connectionString;
            if (null != option && option.StartsWith("clean", StringComparison.InvariantCultureIgnoreCase)) Migrate.clean = true;
            if (null != option && option.StartsWith("print", StringComparison.InvariantCultureIgnoreCase)) Migrate.print = true;

            try {
                CreateDefaultSettingRecords();
            } catch (Exception ex) {
                cConsole.WriteLine(ex.ToString(), ConsoleColor.Red);
            }


            contactPermissions.Clear();
            existingcontactPermissions.Clear();

            try {
                ReadExistingContactPermissions();
            } catch (Exception ex) {
                cConsole.WriteLine(ex.ToString(), ConsoleColor.Red);
            }

            try {
                MigrateShortcutKeys();
            } catch (Exception ex) {
                cConsole.WriteLine(ex.ToString(), ConsoleColor.Red);
            }

            try {
                PermissionsForPreExistingContacts();
            } catch (Exception ex) {
                cConsole.WriteLine(ex.ToString(), ConsoleColor.Red);
            }

            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();

                cConsole.WriteLine(string.Format("Migrate SharedAgencyESNRID for {0}.", connection.Database), ConsoleColor.Yellow);

                using (SqlCommand sharedAgencyESNRIDCommand = new SqlCommand(_SQL_MIGRATE_XSHARED_AGENCY_ESNRID, connection)) {
                    sharedAgencyESNRIDCommand.ExecuteNonQuery();
                }

                cConsole.WriteLine(string.Format("{0} Contacts for {1}.", clean ? "Clean" : "Migrate", connection.Database), ConsoleColor.Yellow);

                //Check if ContacyType table has more columns than required in 5.0 and above
                using (SqlCommand contactTypeCommand = new SqlCommand(_SQL_CONTACT_TYPE_COLUMN_COUNT, connection)) {
                    object columnCount = contactTypeCommand.ExecuteScalar();
                    if (null != columnCount && Int32.Parse(columnCount.ToString()) > 8) {
                        cConsole.WriteLine("Table xSharedContactTypes has more column then the required columns (GUID, name, icon16, icon32, description, UIPriority, lastupdate, syncDate). This may fail port if additional column has any Constraint", ConsoleColor.Yellow);
                    }

                }

                //Check if all ContactType exist in xSharedContactType table
                using (SqlCommand contactTypeCommand = new SqlCommand(_SQL_CONTACT_TYPE_COUNT, connection)) {
                    object ContactTypeCount = contactTypeCommand.ExecuteScalar();
                    if (null != ContactTypeCount && Int32.Parse(ContactTypeCount.ToString()) < 5) {
                        cConsole.WriteLine("Not all required ContactType exist. Creating...", ConsoleColor.Yellow);
                        using (SqlCommand createContactTypesCommand = new SqlCommand(_SQL_CREATE_CONTACT_TYPES, connection)) createContactTypesCommand.ExecuteNonQuery();
                    }
                }

                //Get ContactType GUID and assign to a variable
                using (SqlCommand contactTypeCommand = new SqlCommand(_SQL_CONTACT_TYPE_GUID, connection)) {
                    contactTypeGuid = Guid.Empty;
                    object tableCount = contactTypeCommand.ExecuteScalar();
                    if (null != tableCount) contactTypeGuid = Guid.Parse(tableCount.ToString());
                }

                using (SqlCommand tableCountCommand = new SqlCommand(_SqlTargetTablesExist, connection)) {
                    int requiredTables = (int)tableCountCommand.ExecuteScalar();
                    if (0 == requiredTables) { cConsole.WriteLine("Required Contact tables do not exist. Try running fixXStore first.", ConsoleColor.Red); return; }
                }

                using (SqlDataAdapter sqlda = new SqlDataAdapter(_SQL_SETTINGS, connection)) {
                    DataTable dt = new DataTable();
                    sqlda.Fill(dt);
                    foreach (DataRow dr in dt.Rows) readContactsAndFolders(dr);
                    DataTable changes = dt.GetChanges();
                    if (null != changes && changes.Rows.Count > 0) {
                        SqlCommand sqlc = new SqlCommand(_SQL_SETTINGS_SP, connection);
                        sqlc.CommandType = CommandType.StoredProcedure;
                        foreach (Tuple<string, SqlDbType, string> param in _SqlSettingsSpParams) sqlc.Parameters.Add(param.Item1, param.Item2, -1, param.Item3);
                        sqlda.UpdateCommand = sqlc;
                        int updated = sqlda.Update(changes);
                        cConsole.WriteLine(string.Format("  {0} Setting row(s) affected.", updated), ConsoleColor.Yellow);
                    }
                }
                try {
                    TryCreateContactPermissions(true);
                } catch (Exception ex) {
                    cConsole.WriteLine(ex.ToString(), ConsoleColor.Red);
                }

                cConsole.WriteLineBlank();
                cConsole.WriteLine("Running SQL step.", ConsoleColor.Cyan);
                SqlCommand migrateCommand = new SqlCommand(clean ? _SQL_CLEAN : _SQL_MIGRATE, connection);
                int rowsAffected = 0;
                try {
                    rowsAffected = migrateCommand.ExecuteNonQuery();
                } catch { cConsole.WriteLine("Required tables or columns do not exist. Try running fixXStore first.", ConsoleColor.Red); }
                cConsole.WriteLine(string.Format("  {0} row(s) affected.", rowsAffected), ConsoleColor.Yellow);

                connection.Close();
                cConsole.WriteLine("Done.", ConsoleColor.Green);
            }

        }

        static void readContactsAndFolders(DataRow dr) {
            saveSettingRecord = false;
            settings setting = (settings)dr["settingID"];
            Guid settingGuid = (Guid)dr["guid"];
            short settinglevel = (short)dr["settingLevel"];

            cConsole.WriteLineBlank();
            cConsole.WriteLine(string.Format("Reading {0} contacts and Folders.", setting), ConsoleColor.Cyan);
            clsContactContainer container = (clsContactContainer)DeserializeByteArrayToObject((Byte[])dr["settingValueBinary"]);
            readContacts(container.Contacts, Guid.Empty, settinglevel, settingGuid);
            foreach (clsFolder folder in container.Folders) readFolder(folder, Guid.Empty, settinglevel, settingGuid);
            if (saveSettingRecord) dr["settingValueBinary"] = SerializeObjectToByteArray(container);
        }

        static void readContacts(clsContacts contacts, Guid folderGuid, short settinglevel, Guid settingId) {
            foreach (clsContact contact in contacts) {
                cConsole.WriteLineBicolor("  Contact Type: [{0}] Folder: [{1}] Name: [{2}] Key: [{3}]", contact.contactType, folderGuid, contact.displayName, contact.key);
                Guid contactKeyGuid = Guid.Empty;
                if (Guid.TryParse(contact.key, out contactKeyGuid)) { cConsole.WriteLine("  Contact is a System contact in the root folder.  No action necessary.", ConsoleColor.DarkYellow); continue; }
                if (null != contact.tag) {
                    clsContactTag contactTag = (clsContactTag)contact.tag;
                    if (print) { printContact(contact, contactTag); continue; }
                    if (clean) { cleanContact(contactTag); continue; }
                    if (Guid.TryParse(contactTag.CurrentKey, out contactKeyGuid)) { cConsole.WriteLine("  Contact has already been migrated.  No action necessary.", ConsoleColor.DarkYellow); continue; }
                    Guid contactGuid = Guid.Empty;
                    if (tryCreateContactAndLink(contactTag, folderGuid, out contactGuid, settinglevel, settingId)) {
                        contactTag.CurrentKey = 0 == contactGuid.CompareTo(Guid.Empty) ? string.Empty : contactGuid.ToString();
                        saveSettingRecord = true;
                    }
                }
            }
            cConsole.WriteLineBlank();
        }

        static void printContact(clsContact contact, clsContactTag contactTag) {
            cConsole.WriteLine(string.Format("  Contact Tag Data.", contact.contactType, contact.key));
            cConsole.WriteLine(string.Format("    address: {0}", contactTag.Address));
            cConsole.WriteLine(string.Format("    alternate phone: {0}", contactTag.AlternatePhone));
            cConsole.WriteLine(string.Format("    cell phone: {0}", contactTag.CellPhone));
            cConsole.WriteLine(string.Format("    community: {0}", contactTag.Community));
            cConsole.WriteLine(string.Format("    currentKey: {0}", contactTag.CurrentKey));
            cConsole.WriteLine(string.Format("    email: {0}", contactTag.Email));
            cConsole.WriteLine(string.Format("    contactType: {0}", contactTag.ContactType.ToString()));
            cConsole.WriteLine(string.Format("    notes: {0}", contactTag.Notes));
            cConsole.WriteLine(string.Format("    phone: {0}", contactTag.Phone));
            cConsole.WriteLine(string.Format("    phone Ext: {0}", contactTag.PhoneExt));
            cConsole.WriteLine(string.Format("    sagencyTypeGUID: {0}", contactTag.agencyTypeGUID.ToString()));
            cConsole.WriteLine(string.Format("    sContactGUID: {0}", contactTag.ContactGUID.ToString()));
            cConsole.WriteLine(string.Format("    sStationGUID: {0}", contactTag.StationGUID.ToString()));
            cConsole.WriteLine(string.Format("    state: {0}", contactTag.State));
            cConsole.WriteLine(string.Format("    contactName: {0}", contactTag.ContactName));
            cConsole.WriteLine(string.Format("    zip: {0}", contactTag.ZIP));
        }

        static void cleanContact(clsContactTag contactTag) {
            Guid contactGuid = Guid.Empty;
            if (Guid.TryParse(contactTag.CurrentKey, out contactGuid)) {
                if (0 != contactTag.ContactGUID.CompareTo(contactGuid)) { // we didn't create the contact
                    using (SqlConnection connection = new SqlConnection(connectionString)) {
                        connection.Open();
                        SqlCommand command = new SqlCommand(@"DELETE FROM xSharedContacts WHERE GUID = @guid", connection);
                        command.Parameters.Add(new SqlParameter("@guid", contactGuid));
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0) cConsole.WriteLine(string.Format("  Contact {0} removed.", contactGuid), ConsoleColor.DarkRed); ;
                    }
                } else cConsole.WriteLine("  Contact already existed.", ConsoleColor.DarkYellow);
                if (!string.IsNullOrEmpty(contactTag.CurrentKey)) { contactTag.CurrentKey = string.Empty; saveSettingRecord = true; }
            } else {
                if (0 != contactTag.ContactGUID.CompareTo(Guid.Empty)) {
                    using (SqlConnection connection = new SqlConnection(connectionString)) {
                        connection.Open();
                        SqlCommand contactCountCommand = new SqlCommand(@"SELECT COUNT(*) FROM xSharedContacts WHERE [GUID] = @guid", connection);
                        contactCountCommand.Parameters.Add(new SqlParameter("@guid", contactGuid));
                        int contactCount = (int)contactCountCommand.ExecuteScalar();
                        if (0 == contactCount) {
                            cConsole.WriteLine(string.Format("  Contact {0} does not exist in contacts table.  Removing reference.", contactGuid), ConsoleColor.DarkYellow);
                            contactTag.ContactGUID = Guid.Empty;
                            saveSettingRecord = true;
                        }
                    }
                }
            }
        }

        static bool tryCreateContactAndLink(clsContactTag contactTag, Guid folderGuid, out Guid contactGuid, short settinglevel, Guid settingId) {
            contactGuid = Guid.Empty;
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();
                if (0 == contactTag.ContactGUID.CompareTo(Guid.Empty)) { // Contacts without a database entry have empty guids here
                    SqlCommand contactCountCommand = new SqlCommand(@"SELECT COUNT(*) FROM xSharedContacts WHERE [name] = @name AND phone = @phone", connection);
                    contactCountCommand.Parameters.Add(new SqlParameter("@name", contactTag.ContactName.Truncate(100, "name")));
                    contactCountCommand.Parameters.Add(new SqlParameter("@phone", contactTag.Phone.Truncate(24, "phone")));
                    int contactCount = (int)contactCountCommand.ExecuteScalar();
                    if (0 == contactCount) {
                        SqlCommand createContact = new SqlCommand(_SQL_CONTACTS_SP, connection);
                        foreach (string param in _SqlContactsSpParams) {
                            object value = null;
                            if (0 == string.Compare(param, "@GUID")) value = contactGuid = Guid.NewGuid();
                            if (0 == string.Compare(param, "@name")) value = contactTag.ContactName.Truncate(100, "name");
                            if (0 == string.Compare(param, "@contactTypeGUID")) value = contactTypeGuid;
                            if (0 == string.Compare(param, "@agencyTypeGUID")) value = contactTag.agencyTypeGUID;
                            if (0 == string.Compare(param, "@externalID")) value = string.Empty;
                            if (0 == string.Compare(param, "@description")) value = contactTag.Notes.Truncate(100, "description"); ;
                            if (0 == string.Compare(param, "@phone")) value = contactTag.Phone.Contains('*') ? string.Empty : contactTag.Phone.Truncate(24, "phone");
                            if (0 == string.Compare(param, "@starCode")) value = contactTag.Phone.Contains('*') ? contactTag.Phone.Truncate(10, "starCode") : string.Empty;
                            if (0 == string.Compare(param, "@allowHookflashTransfers")) value = 0;
                            if (0 == string.Compare(param, "@cellPhone")) value = contactTag.CellPhone.Truncate(24, "cellPhone");
                            if (0 == string.Compare(param, "@alternatePhone")) value = contactTag.AlternatePhone.Truncate(24, "alternatePhone");
                            if (0 == string.Compare(param, "@fax")) value = string.Empty;
                            if (0 == string.Compare(param, "@email")) value = contactTag.Email.Truncate(50, "email");
                            if (0 == string.Compare(param, "@pagerPhoneNum")) value = string.Empty;
                            if (0 == string.Compare(param, "@pagerPIN")) value = string.Empty;
                            if (0 == string.Compare(param, "@number")) value = string.Empty;
                            if (0 == string.Compare(param, "@address")) value = contactTag.Address.Truncate(100, "address");
                            if (0 == string.Compare(param, "@community")) value = contactTag.Community.Truncate(32, "community");
                            if (0 == string.Compare(param, "@state")) value = contactTag.State.Truncate(2, "state");
                            if (0 == string.Compare(param, "@zip")) value = contactTag.ZIP.Truncate(10, "zip");
                            if (0 == string.Compare(param, "@contactImage")) value = imageToByteArray(null);
                            if (0 == string.Compare(param, "@lastUpdate")) value = DateTime.Now;
                            if (0 == string.Compare(param, "@syncDate")) value = DateTime.Now;
                            if (0 == string.Compare(param, "@SIPURI")) value = string.Empty;
                            createContact.Parameters.Add(new SqlParameter(param, value));
                        }
                        if (!string.IsNullOrEmpty(createContact.Parameters["@name"].Value.ToString())) {
                            Guid newContactId = (Guid)createContact.Parameters["@GUID"].Value;
                            int beforeQueryExecution = rowsAffected;
                            rowsAffected += createContact.ExecuteNonQuery();
                            if (beforeQueryExecution < rowsAffected) {
                                ContactPermissionModel cm = new ContactPermissionModel(newContactId, TCS.Common.Enumerations.eIContactLinkTypes.Contact, settingId, Convertsettinglevel2MemberType(settinglevel), eContactPermissions.FullAccess);

                                if (existingcontactPermissions.Where(i => i.ContactLinkGUID == newContactId && i.ContactLinkType == TCS.Common.Enumerations.eIContactLinkTypes.Contact &&
                                    i.MemberId == settingId && i.MemberType == Convertsettinglevel2MemberType(settinglevel)).Count() == 0) {
                                    contactPermissions.Add(cm);
                                }
                            }
                        } else cConsole.WriteLine("  Contact name was null or empty, skipping contact.", ConsoleColor.DarkYellow);
                    }
                } else {
                    Guid existingFolderGuid = Guid.Empty;
                    // Contact may already exist without folder link so we test contactGuid to see if we created the contact or not, if we didn't we need to get the contact's guid
                    if (0 == contactGuid.CompareTo(Guid.Empty)) contactGuid = contactTag.ContactGUID;
                    if (!Guid.TryParse(contactTag.CurrentKey, out existingFolderGuid)) { // existing folderLinks are stored here
                        if (0 != folderGuid.CompareTo(Guid.Empty)) {
                            SqlCommand createFolderLink = new SqlCommand(_SQL_FOLDER_LINKS_SP, connection);
                            foreach (string param in _SqlFolderLinksSpParams) {
                                object value = null;
                                if (0 == string.Compare(param, "@guid")) { value = Guid.NewGuid(); contactTag.CurrentKey = value.ToString(); }
                                if (0 == string.Compare(param, "@parentFolderGuid")) value = folderGuid;
                                if (0 == string.Compare(param, "@iContactGuid")) value = contactGuid;
                                if (0 == string.Compare(param, "@iContactType")) value = eIContactLinkTypes.Contact;
                                if (0 == string.Compare(param, "@lastUpdate")) value = DateTime.Now;
                                createFolderLink.Parameters.Add(new SqlParameter(param, value));
                            }
                            Guid newFolderLinkId = (Guid)createFolderLink.Parameters["@guid"].Value;
                            rowsAffected += createFolderLink.ExecuteNonQuery();
                        } else cConsole.WriteLine("  Contact is in the root folder.  No folder link necessary.", ConsoleColor.DarkYellow);
                    } else cConsole.WriteLine("  Contact folder link already migrated.", ConsoleColor.DarkYellow);
                    cConsole.WriteLine("  Contact already existed.", ConsoleColor.DarkYellow);
                }
            }
            return rowsAffected > 0;
        }

        static void readFolder(clsFolder folder, Guid parentGuid, short settinglevel, Guid settingId) {
            cConsole.WriteLineBlank();
            cConsole.WriteLineBicolor("  Folder: {0} Subfolders: {1} Contacts: {2} Parent: {3}", folder.key, folder.Folders.Count, folder.Contacts.Count, parentGuid);
            Guid folderGuid = Guid.Empty;
            bool folderTagIsGuid = Guid.TryParse(folder.tag, out folderGuid);
            if (clean) cleanFolder(folderTagIsGuid, folderGuid, folder);
            else {
                if (!folderTagIsGuid) {
                    if (tryCreateFolderAndLink(folder, parentGuid, out folderGuid, settinglevel, settingId)) {
                        folder.tag = folderGuid.ToString();
                        saveSettingRecord = true;
                    }
                } else cConsole.WriteLine("  Folder already migrated.", ConsoleColor.DarkYellow);
            }
            readContacts(folder.Contacts, folderGuid, settinglevel, settingId);
            foreach (clsFolder subfolder in folder.Folders) readFolder(subfolder, folderGuid, settinglevel, settingId);
        }

        static void cleanFolder(bool folderTagIsGuid, Guid folderGuid, clsFolder folder) {
            if (folderTagIsGuid) {
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    connection.Open();
                    SqlCommand linksCommand = new SqlCommand(@"DELETE FROM xSharedFolderLinks WHERE parentFolderGUID = @guid", connection);
                    linksCommand.Parameters.Add(new SqlParameter("@guid", folderGuid));
                    int linksAffected = linksCommand.ExecuteNonQuery();
                    SqlCommand folderCommand = new SqlCommand(@"DELETE FROM xSharedFolders WHERE guid = @guid", connection);
                    folderCommand.Parameters.Add(new SqlParameter("@guid", folderGuid));
                    int foldersAffected = folderCommand.ExecuteNonQuery();
                    if (linksAffected > 0) cConsole.WriteLine(string.Format("  {0} folder links removed for parent folder {1}.", linksAffected, folderGuid), ConsoleColor.DarkRed);
                    if (foldersAffected > 0) cConsole.WriteLine(string.Format("  folder {0} removed.", foldersAffected), ConsoleColor.DarkRed);
                    folder.tag = "folder";
                    saveSettingRecord = true;
                }
            } else cConsole.WriteLine("  No action necessary.", ConsoleColor.DarkYellow);
        }

        static bool tryCreateFolderAndLink(clsFolder folder, Guid parentGuid, out Guid folderGuid, short settinglevel, Guid settingId) {
            folderGuid = Guid.Empty;
            int rowsAffected = 0;
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();
                SqlCommand createFolder = new SqlCommand(_SQL_FOLDERS_SP, connection);
                foreach (string param in _SqlFoldersSpParams) {
                    object value = null;
                    if (0 == string.Compare(param, "@guid")) value = folderGuid = Guid.NewGuid();
                    if (0 == string.Compare(param, "@name")) value = folder.DisplayName;
                    if (0 == string.Compare(param, "@description")) value = string.Format("System migrated {0}", DateTime.Now.ToString());
                    if (0 == string.Compare(param, "@image")) value = imageToByteArray(folder.displayImage);
                    if (0 == string.Compare(param, "@owningUserGuid")) value = DBNull.Value;
                    //if (0 == string.Compare(param, "@lastUpdate")) value = DateTime.Now;
                    //if (0 == string.Compare(param, "@syncDate")) value = DateTime.Now;
                    createFolder.Parameters.Add(new SqlParameter(param, value));
                }
                Guid newFolderId = (Guid)createFolder.Parameters["@guid"].Value;
                int beforeQueryExecution = rowsAffected;
                rowsAffected += createFolder.ExecuteNonQuery();
                if (beforeQueryExecution < rowsAffected) {
                    ContactPermissionModel cm = new ContactPermissionModel(newFolderId, TCS.Common.Enumerations.eIContactLinkTypes.Folder, settingId, Convertsettinglevel2MemberType(settinglevel), eContactPermissions.FullAccess);

                    if (existingcontactPermissions.Where(i => i.ContactLinkGUID == newFolderId && i.ContactLinkType == TCS.Common.Enumerations.eIContactLinkTypes.Folder &&
                        i.MemberId == settingId && i.MemberType == Convertsettinglevel2MemberType(settinglevel)).Count() == 0) {
                        contactPermissions.Add(cm);
                    }
                }

                if (0 != parentGuid.CompareTo(Guid.Empty)) { // root folders don't appear to need a link
                    SqlCommand createFolderLink = new SqlCommand(_SQL_FOLDER_LINKS_SP, connection);
                    foreach (string param in _SqlFolderLinksSpParams) {
                        object value = null;
                        if (0 == string.Compare(param, "@guid")) value = Guid.NewGuid();
                        if (0 == string.Compare(param, "@parentFolderGuid")) value = parentGuid;
                        if (0 == string.Compare(param, "@iContactGuid")) value = folderGuid;
                        if (0 == string.Compare(param, "@iContactType")) value = eIContactLinkTypes.Folder;
                        if (0 == string.Compare(param, "@lastUpdate")) value = DateTime.Now;
                        createFolderLink.Parameters.Add(new SqlParameter(param, value));
                    }
                    if (0 == parentGuid.CompareTo(Guid.Empty)) createFolderLink.Parameters["@parentFolderGuid"].Value = newFolderId;
                    rowsAffected += createFolderLink.ExecuteNonQuery();
                } else cConsole.WriteLine("  Folder parent is the root folder.  No folder link necessary.", ConsoleColor.DarkYellow);
            }
            return 0 != rowsAffected;
        }

        static object DeserializeByteArrayToObject(byte[] ba) {
            if (ba.Length == 0) return null;
            MemoryStream ms = new MemoryStream();
            ms.Write(ba, 0, ba.Length);
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Binder = new ContactSerializationBinder();
            return bf.Deserialize(ms);
        }

        static byte[] SerializeObjectToByteArray(object o) {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Binder = new ContactSerializationBinder();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, o);
            return ms.ToArray();
        }

        sealed class ContactSerializationBinder : SerializationBinder {
            public override void BindToName(Type serializedType, out string assemblyName, out string typeName) {
                if (serializedType.FullName.StartsWith("ContactsClassic.")) {
                    typeName = serializedType.FullName.Replace("ContactsClassic.", "Controls.");
                    if (string.IsNullOrEmpty(Migrate.originalAssemblyName)) throw new SerializationException("Migrate.originalAssemblyName is not defined.");
                    assemblyName = Migrate.originalAssemblyName;
                } else {
                    typeName = serializedType.FullName;
                    assemblyName = serializedType.Assembly.FullName;
                }
            }
            public override Type BindToType(string assemblyName, string typeName) {
                Type type = null;
                if (!typeName.StartsWith("Controls.")) type = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
                else {
                    if (string.IsNullOrEmpty(Migrate.originalAssemblyName)) Migrate.originalAssemblyName = assemblyName;
                    typeName = typeName.Replace("Controls.", "ContactsClassic.");
                    assemblyName = Assembly.GetAssembly(typeof(clsContactContainer)).FullName;
                    type = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
                }
                return type;
            }
        }

        static byte[] imageToByteArray(System.Drawing.Image image) {
            if (null == image) return new byte[0];
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            return ms.ToArray();
        }

        static xSlnSettingLevels Convertsettinglevel2MemberType(short settinglevel) {
            if (settinglevel == 0)
                return xSlnSettingLevels.System;
            else if (settinglevel == 1)
                return xSlnSettingLevels.Group;
            else
                return xSlnSettingLevels.Agent;
        }

        static bool TryCreateContactPermissions(bool createDefault) {
            int rowsAffected = 0;
            if (contactPermissions.Count < 1)
                return false;

            if (createDefault) {
                CreateDefaultContactPermissionRecords();
            }

            // Remove duplicate contact permission records that already exist in xSharedContactPermissions table
            contactPermissions.ForEach(i => {
                if (existingcontactPermissions.Where(j => j.MemberType == i.MemberType && j.MemberId == i.MemberId && j.ContactLinkType == i.ContactLinkType && j.ContactLinkGUID == i.ContactLinkGUID).Count() > 0)
                    contactPermissions.Remove(i);
            });

            cConsole.WriteLine(string.Format("Create Contact Permission records for migrated Contacts"), ConsoleColor.Yellow);

            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();
                try {
                    foreach (var cp in contactPermissions) {
                        SqlCommand createContactPermission = new SqlCommand(_SQL_CONTACT_PERMISSIONS_SP, connection);
                        foreach (string param in _SqlContactPermissionsSpParams) {
                            object value = null;
                            if (0 == string.Compare(param, "@guid")) value = cp.Id;
                            if (0 == string.Compare(param, "@contactLinkguid")) value = cp.ContactLinkGUID;
                            if (0 == string.Compare(param, "@contactlinktype")) value = (int)cp.ContactLinkType;
                            if (0 == string.Compare(param, "@memberguid")) value = cp.MemberId;
                            if (0 == string.Compare(param, "@membertype")) value = (int)cp.MemberType;
                            if (0 == string.Compare(param, "@contactpermissionSetting")) value = (int)cp.ContactPermissionSetting;
                            if (0 == string.Compare(param, "@lastupdate")) value = cp.LastUpdate;
                            if (0 == string.Compare(param, "@updateuserguid")) value = cp.UpdateUserId;
                            createContactPermission.Parameters.Add(new SqlParameter(param, value));
                        }
                        rowsAffected += createContactPermission.ExecuteNonQuery();
                    }
                } catch (Exception ex) {
                    cConsole.WriteLine(string.Format("Contact Permission records could not be successfully created for all migrated Contacts"), ConsoleColor.Red);
                    return false;
                }
                connection.Close();
            }
            cConsole.WriteLine(string.Format("Created {0} Contact Permission records for migrated Contacts", rowsAffected), ConsoleColor.Yellow);
            return 0 != rowsAffected;
        }

        static void CreateDefaultContactPermissionRecords() {
            if (contactPermissions.Count < 1)
                return;

            List<ContactPermissionModel> list = new List<ContactPermissionModel>();
            list = contactPermissions.Where(i => i.MemberType == xSlnSettingLevels.Agent).ToList();

            list.ForEach(i => {
                //If there's no default system one at agent level, then add it with Full Access.
                if (contactPermissions.Where(j => j.ContactLinkType == i.ContactLinkType && j.ContactLinkGUID == i.ContactLinkGUID && j.MemberType == xSlnSettingLevels.System && j.MemberId == Guid.Empty).Count() == 0) {
                    ContactPermissionModel cm = new ContactPermissionModel(i.ContactLinkGUID, i.ContactLinkType, Guid.Empty, xSlnSettingLevels.System, eContactPermissions.FullAccess);
                    contactPermissions.Add(cm);
                }
            });


            list = contactPermissions.Where(i => i.MemberType == xSlnSettingLevels.Group).ToList();
            list.ForEach(i => {
                //If there's no default system one at group level, then add it with No Access.
                if (contactPermissions.Where(j => j.ContactLinkType == i.ContactLinkType && j.ContactLinkGUID == i.ContactLinkGUID && j.MemberType == xSlnSettingLevels.System && j.MemberId == Guid.Empty).Count() == 0) {
                    ContactPermissionModel cm = new ContactPermissionModel(i.ContactLinkGUID, i.ContactLinkType, Guid.Empty, xSlnSettingLevels.System, eContactPermissions.NoAccess);
                    contactPermissions.Add(cm);
                }
            });

            contactPermissions = contactPermissions.Where(i => i.MemberType != xSlnSettingLevels.Agent).ToList();
        }

        static bool MigrateShortcutKeys() {
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();

                //xTrakker911Settings xt911 = new xTrakker911Settings();
                List<TCS.Models.Settings.ShortCutKey> shortcutList = new List<TCS.Models.Settings.ShortCutKey>();
                xSlnSettingLevels settingLevel = xSlnSettingLevels.System;
                int rowsAffected = 0;
                try {
                    using (SqlDataAdapter sqlda = new SqlDataAdapter(_SQLSHORTCUT_KEY_SETTINGS, connection)) {
                        cConsole.WriteLine(string.Format("Migrate xT911 Shortcut Keys for {0}.", connection.Database), ConsoleColor.Yellow);
                        DataTable dt = new DataTable();
                        sqlda.Fill(dt);
                        foreach (DataRow dr in dt.Rows) {
                            shortcutList = ShortcutKeyConvertertoNG.GetShortcutKeys(TCS.Common.MiscLib.DeserializeByteArrayToObject((byte[])dr["settingValueBinary"]));
                            ObservableCollection<TCS.Models.Settings.ShortCutKey> s_observable = new ObservableCollection<TCS.Models.Settings.ShortCutKey>();
                            foreach (var shortcut in shortcutList) {
                                if (s_observable.Where(i => i.Action == shortcut.Action).Count() == 0)
                                    s_observable.Add(shortcut);
                            }
                            if ((short)dr["settingLevel"] == 0) { settingLevel = xSlnSettingLevels.System; } else if ((short)dr["settingLevel"] == 1) { settingLevel = xSlnSettingLevels.Group; } else { settingLevel = xSlnSettingLevels.Agent; }

                            string serialized_shortcutKeys = TCS.Common.XmlSerializationHelper.Serialize(s_observable, noWhiteSpace: true);

                            SqlCommand createShortcutKeysetting = new SqlCommand(_SQL_SHORTCUT_KEY_SP, connection);
                            foreach (string param in _SqlShortcutKeySpParams) {
                                object value = null;
                                if (0 == string.Compare(param, "@guid")) { value = Guid.NewGuid(); }
                                if (0 == string.Compare(param, "@settingid")) { value = MdSettings.xT911_ShortCutKeyMappings; }
                                if (0 == string.Compare(param, "@memberguid")) { value = (Guid)dr["guid"]; }
                                if (0 == string.Compare(param, "@membertype")) { value = settingLevel; }
                                if (0 == string.Compare(param, "@settingvalue")) value = serialized_shortcutKeys;
                                if (0 == string.Compare(param, "@lastupdate")) { value = DateTime.Now; }

                                createShortcutKeysetting.Parameters.Add(new SqlParameter(param, value));
                            }
                            rowsAffected += createShortcutKeysetting.ExecuteNonQuery();
                        }
                    }
                    cConsole.WriteLine(string.Format("Migrated {0} records of xT911 for Shortcut Keys", rowsAffected), ConsoleColor.Yellow);
                } catch (Exception ex) {
                    cConsole.WriteLine(string.Format("All xT911 for Shortcut Keys could not be migrated successfully"), ConsoleColor.Red);
                    return false;
                }

                return true;
            }
        }

        static bool PermissionsForPreExistingContacts() {
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();

                xSlnSettingLevels memberType = xSlnSettingLevels.System;
                Guid memberId = Guid.Empty;
                try {
                    using (SqlDataAdapter sqlda = new SqlDataAdapter(_SQLPRE_EXISTING_CONTACTS, connection)) {
                        //cConsole.WriteLine(string.Format("Migrate xT911 Shortcut Keys for {0}.", connection.Database), ConsoleColor.Yellow);
                        DataTable dt = new DataTable();
                        sqlda.Fill(dt);
                        foreach (DataRow dr in dt.Rows) {
                            Guid contacttypeId = (Guid)dr["guid"];
                            string contactType = (string)dr["ContactType"];
                            ContactPermissionModel cm = new ContactPermissionModel(contacttypeId, GetContactLinkType(contactType), memberId, memberType, eContactPermissions.FullAccess);

                            if (existingcontactPermissions.Where(i => i.ContactLinkGUID == contacttypeId && i.ContactLinkType == GetContactLinkType(contactType)).Count() == 0) {
                                contactPermissions.Add(cm);
                            }
                        }
                    }
                    // cConsole.WriteLine(string.Format("Migrated {0} records of xT911 for Shortcut Keys", rowsAffected), ConsoleColor.Yellow);
                } catch (Exception ex) {
                    cConsole.WriteLine(string.Format("Error reading pre-existing contacts to create contact permissions"), ConsoleColor.Red);
                    return false;
                }
                return true;
            }
        }

        static bool ReadExistingContactPermissions() {
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();

                try {
                    using (SqlDataAdapter sqlda = new SqlDataAdapter(_SQLCONTACT_PERMISSIONS, connection)) {
                        //cConsole.WriteLine(string.Format("Migrate xT911 Shortcut Keys for {0}.", connection.Database), ConsoleColor.Yellow);
                        DataTable dt = new DataTable();
                        sqlda.Fill(dt);
                        foreach (DataRow dr in dt.Rows) {
                            Guid contacttypeId = (Guid)dr["ContactLinkGUID"];
                            TCS.Common.Enumerations.eIContactLinkTypes contactType = (TCS.Common.Enumerations.eIContactLinkTypes)dr["ContactLinkType"];
                            xSlnSettingLevels memberType = (xSlnSettingLevels)dr["memberType"];
                            Guid memberId = (Guid)dr["memberGUID"];
                            eContactPermissions contactpermission = (eContactPermissions)dr["ContactPermissionSetting"];

                            ContactPermissionModel cm = new ContactPermissionModel(contacttypeId, contactType, memberId, memberType, contactpermission);
                            existingcontactPermissions.Add(cm);
                        }
                    }
                    // cConsole.WriteLine(string.Format("Migrated {0} records of xT911 for Shortcut Keys", rowsAffected), ConsoleColor.Yellow);
                } catch (Exception ex) {
                    cConsole.WriteLine(string.Format("Error reading existing contact permissions"), ConsoleColor.Red);
                    return false;
                }
                return true;
            }
        }

        static TCS.Common.Enumerations.eIContactLinkTypes GetContactLinkType(string contactType) {
            switch (contactType) {
                case "Contact":
                return TCS.Common.Enumerations.eIContactLinkTypes.Contact;
                case "PSAP":
                return TCS.Common.Enumerations.eIContactLinkTypes.PSAP;
                case "Station":
                return TCS.Common.Enumerations.eIContactLinkTypes.Station;
                case "Folder":
                return TCS.Common.Enumerations.eIContactLinkTypes.Folder;
                case "Agent":
                return TCS.Common.Enumerations.eIContactLinkTypes.Agent;
                case "Queue":
                return TCS.Common.Enumerations.eIContactLinkTypes.Queue;
                default:
                return TCS.Common.Enumerations.eIContactLinkTypes.None;
            }
        }

        static bool CreateDefaultSettingRecords() {
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();

                try {
                    using (SqlCommand xT911GlobalSettingCommand = new SqlCommand(_SQL_XT911_GLOBAL_SETTINGS_COUNT, connection)) {
                        object xT911GlobalSettingRecCount = xT911GlobalSettingCommand.ExecuteScalar();
                        if (null == xT911GlobalSettingRecCount || Int32.Parse(xT911GlobalSettingRecCount.ToString()) == 0) {
                            using (SqlCommand xT911GlobalSettingInsertCommand = new SqlCommand(_INSERT_XT911_GLOBAL_SETTING, connection)) {
                                object xT911GlobalSettingInsertRecCount = xT911GlobalSettingInsertCommand.ExecuteScalar();
                            }
                        }
                    }

                    using (SqlCommand xTrakkerGlobalSettingCommand = new SqlCommand(_SQL_XTRAKKER_GLOBAL_SETTINGS_COUNT, connection)) {
                        object xTrakkerGlobalSettingRecCount = xTrakkerGlobalSettingCommand.ExecuteScalar();
                        if (null == xTrakkerGlobalSettingRecCount || Int32.Parse(xTrakkerGlobalSettingRecCount.ToString()) == 0) {
                            using (SqlCommand xTrakkerGlobalSettingInsertCommand = new SqlCommand(_INSERT_XTRAKKER_GLOBAL_SETTING, connection)) {
                                object xTrakkerGlobalSettingInsertRecCount = xTrakkerGlobalSettingInsertCommand.ExecuteScalar();
                            }
                        }
                    }
                } catch (Exception ex) {
                    cConsole.WriteLine(string.Format("Error Creating setting records in xSharedGlobalSettings table for xTrakker or xT911"), ConsoleColor.Red);
                    return false;
                }
                return true;
            }
        }

    }
}
