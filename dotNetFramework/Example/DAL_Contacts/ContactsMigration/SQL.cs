using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsMigration
{
    using System.Data;

    public partial class Migrate
    {
        private const string _SQL_CONTACT_TYPE_COUNT = @"SELECT count(GUID) FROM xSharedContactTypes WHERE name in ('Contact','PSAP','Station','Agent','Queue')";
        private const string _SQL_CONTACT_TYPE_GUID = @"SELECT GUID FROM xSharedContactTypes WHERE name = 'Contact'";
        private const string _SQL_CONTACT_TYPE_COLUMN_COUNT = @"SELECT COUNT(1) FROM sys.columns WHERE object_id = (SELECT object_ID FROM sys.Tables WHERE Name = 'xSharedContactTypes')";
        private const string _SQL_CONTACT_PERMISSION_COUNT = @"SELECT count(1) FROM xSharedContactPermissions ";
        private const string _SQL_XT911_GLOBAL_SETTINGS_COUNT = @"SELECT COUNT(*) FROM xSharedGlobalSettings WHERE settingType = 3000";
        private const string _SQL_XTRAKKER_GLOBAL_SETTINGS_COUNT = @"SELECT COUNT(*) FROM xSharedGlobalSettings WHERE settingType = 2000";

        private const string _SQL_CREATE_CONTACT_TYPES = @"
-- declare contactType Guids
DECLARE @PriorityBase INT = 10
SET @PriorityBase = (SELECT TOP(1) UIPriority FROM xSharedContactTypes ORDER BY UIPriority DESC)

DECLARE @contactGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedContactTypes WHERE name = 'Contact')
IF @contactGuid IS NULL
BEGIN
	SELECT @contactGuid = NEWID()
	INSERT INTO [xSharedContactTypes] ([GUID], [name], [icon16], [icon32], [description], [UIPriority], [lastUpdate], [syncDate])
	VALUES (@contactGuid, 'Contact', NULL, NULL, 'Contact Type', @PriorityBase + 10, GETDATE(), NULL)	
END
DECLARE @psapGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedContactTypes WHERE name = 'PSAP')
IF @psapGuid IS NULL
BEGIN
	SELECT @psapGuid = NEWID()
	INSERT INTO [xSharedContactTypes] ([GUID], [name], [icon16], [icon32], [description], [UIPriority], [lastUpdate], [syncDate])
	VALUES (@psapGuid, 'PSAP', NULL, NULL, 'PSAP Type', @PriorityBase + 20, GETDATE(), NULL)	
END

DECLARE @stationGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedContactTypes WHERE name = 'Station')
IF @stationGuid IS NULL
BEGIN
	SELECT @stationGuid = NEWID()
	INSERT INTO [xSharedContactTypes] ([GUID], [name], [icon16], [icon32], [description], [UIPriority], [lastUpdate], [syncDate])
	VALUES (@stationGuid, 'Station', NULL, NULL, 'Station Type', @PriorityBase + 30, GETDATE(), NULL)	
END

DECLARE @agentGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedContactTypes WHERE name = 'Agent')
IF @agentGuid IS NULL
BEGIN
	SELECT @agentGuid = NEWID()
	INSERT INTO [xSharedContactTypes] ([GUID], [name], [icon16], [icon32], [description], [UIPriority], [lastUpdate], [syncDate])
	VALUES (@agentGuid, 'Agent', NULL, NULL, 'Agent Type', @PriorityBase + 40, GETDATE(), NULL)	
END 

DECLARE @queueGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedContactTypes WHERE name = 'Queue')
IF @queueGuid IS NULL
BEGIN
	SELECT @queueGuid = NEWID()
	INSERT INTO [xSharedContactTypes] ([GUID], [name], [icon16], [icon32], [description], [UIPriority], [lastUpdate], [syncDate])
	VALUES (@queueGuid, 'Queue', NULL, NULL, 'Queue Type', @PriorityBase + 50, GETDATE(), NULL)	
END

-- Update existing contacts that use ESA type to new contact type
Update xSharedContacts
set contactTypeGUID=@contactGuid
Where contactTypeGUID='00000000-0000-0000-0000-000000000000'
";

        static readonly List<string> _RequiredTables = new List<string>() {
		"xSharedFolders",
		"xSharedEmails",
		"xSharedAddresses",
		"xSharedPhoneNumbers",
		"xSharedSipUris",
		"xSharedFavorites",
		"xSharedFolderLinks",
		"xSharedIContactToEmail",
		"xSharedIContactToAddress",
		"xSharedIContactToPhone",
		"xSharedIContactToSIPURI",
		"xSharedContactTypes",
		"xSharedAddressTypes",
		"xSharedPhoneNumberTypes",
	};

        static readonly string _SqlTargetTablesExist = string.Format(
    @"
SELECT 
CASE WHEN (
	SELECT COUNT(*) FROM sys.tables WHERE name IN (
	'{0}'
	)) = (SELECT ({1}))
THEN 1
ELSE 0
END AS Result
", string.Join(@"','", _RequiredTables.ToArray()), _RequiredTables.Count().ToString());


        private const string _SQL_SETTINGS = @"
SELECT settingID, settingLevel, [guid], settingValue, settingValueBinary, settingValueXML, updateUserGuid, lastUpdate, msrepl_tran_version 
FROM xViewApplicationSettings 
--WHERE settingID IN (3061)
WHERE settingID IN (3061, 3069, 3062)
--WHERE [guid] = '00000000-0000-0000-0000-000000000000' AND settingID IN (3061, 3069, 3062)
";

        private const string _SQL_SETTINGS_SP = @"xSolutionSettings_Update";

        static readonly List<Tuple<string, SqlDbType, string>> _SqlSettingsSpParams = new List<Tuple<string, SqlDbType, string>>() {
		new Tuple<string, SqlDbType, string>("@settingID", SqlDbType.SmallInt, "settingID"),
		new Tuple<string, SqlDbType, string>("@settingLevel", SqlDbType.SmallInt, "settingLevel"),
		new Tuple<string, SqlDbType, string>("@guid", SqlDbType.UniqueIdentifier, "guid"),
		new Tuple<string, SqlDbType, string>("@settingValue", SqlDbType.VarChar, "settingValue"),
		new Tuple<string, SqlDbType, string>("@settingValueBinary", SqlDbType.VarBinary, "settingValueBinary"),
		new Tuple<string, SqlDbType, string>("@settingValueXML", SqlDbType.Xml, "settingValueXML"),
		new Tuple<string, SqlDbType, string>("@updateUserGuid", SqlDbType.UniqueIdentifier, "updateUserGuid"),
		new Tuple<string, SqlDbType, string>("@lastUpdate", SqlDbType.DateTime, "lastUpdate"),
		new Tuple<string, SqlDbType, string>("@msrepl_tran_version", SqlDbType.UniqueIdentifier, "msrepl_tran_version"),
	};

        private const string _SQL_FOLDERS_SP = @"
INSERT INTO xSharedFolders(guid, name, description, image, owningUserGuid)
VALUES (@guid, @name, @description, @image, @owningUserGuid)
--INSERT INTO xSharedFolders(guid, name, description, image, owningUserGuid, lastUpdate, syncDate)
--VALUES (@guid, @name, @description, @image, @owningUserGuid, @lastUpdate, @syncDate)
";

        static readonly List<string> _SqlFoldersSpParams = new List<string>() {
		"@guid",
		"@name",
		"@description",
		"@image",
		"@owningUserGuid",
		//"@lastUpdate",
		//"@syncDate",
	};

        private const string _SQL_FOLDER_LINKS_SP = @"
INSERT INTO xSharedFolderLinks(guid, parentFolderGuid, iContactGuid, iContactType, lastUpdate)
VALUES (@guid, @parentFolderGuid, @iContactGuid, @iContactType, @lastUpdate)
";

        static readonly List<string> _SqlFolderLinksSpParams = new List<string>() {
		"@guid",
		"@parentFolderGuid",
		"@iContactGuid",
		"@iContactType",
		"@lastUpdate",
	};

        private const string _SQL_CONTACTS_SP = @"
INSERT INTO xSharedContacts(GUID, name, contactTypeGUID, agencyTypeGUID, externalID, description, phone, starCode, allowHookflashTransfers, cellPhone, alternatePhone, fax, email, pagerPhoneNum, pagerPIN, number, address, community, state, zip, contactImage, lastUpdate, syncDate, SIPURI)
VALUES(@GUID, @name, @contactTypeGUID, @agencyTypeGUID, @externalID, @description, @phone, @starCode, @allowHookflashTransfers, @cellPhone, @alternatePhone, @fax, @email, @pagerPhoneNum, @pagerPIN, @number, @address, @community, @state, @zip, @contactImage, @lastUpdate, @syncDate, @SIPURI)
";

        static readonly List<string> _SqlContactsSpParams = new List<string>() {
		"@GUID",
		"@name",
		"@contactTypeGUID",
		"@agencyTypeGUID",
		"@externalID",
		"@description",
		"@phone",
		"@starCode",
		"@allowHookflashTransfers",
		"@cellPhone",
		"@alternatePhone",
		"@fax",
		"@email",
		"@pagerPhoneNum",
		"@pagerPIN",
		"@number",
		"@address",
		"@community",
		"@state",
		"@zip",
		"@contactImage",
		"@lastUpdate",
		"@syncDate",
		"@SIPURI",
	};

        private const string _SQL_CONTACT_PERMISSIONS_SP = @"
INSERT INTO xSharedContactPermissions(GUID, ContactLinkGUID, ContactLinkType, memberGUID, memberType, ContactPermissionSetting, lastUpdate, updateUserGuid)
VALUES (@guid, @contactLinkguid, @contactlinktype, @memberguid, @membertype, @contactpermissionSetting, @lastupdate, @updateuserguid)
";

        static readonly List<string> _SqlContactPermissionsSpParams = new List<string>() {
		"@guid",
		"@contactLinkguid",
		"@contactlinktype",
		"@memberguid",
		"@membertype",
		"@contactpermissionSetting",
		"@lastupdate",
		"@updateuserguid",
	};

        private const string _SQL_SHORTCUT_KEY_SP = @"
INSERT INTO xSlnSettingValue(GUID, settingID, memberGUID, memberType, settingValue, lastUpdate)
VALUES (@guid, @settingid, @memberguid, @membertype, @settingvalue, @lastupdate)";

        static readonly List<string> _SqlShortcutKeySpParams = new List<string>() {
		"@guid",        
		"@settingid",
		"@memberguid",
		"@membertype",
		"@settingvalue",
		"@lastupdate",
	};

        private const string _SQLSHORTCUT_KEY_SETTINGS = @"SELECT  
settingID, settingLevel, [guid], settingValue, settingValueBinary, settingValueXML, 
updateUserGuid, lastUpdate, msrepl_tran_version 
FROM 
xViewApplicationSettings A
WHERE NOT EXISTS
( SELECT 1
  FROM xSlnSettingValue B
  WHERE B.memberGUID = A.guid  
  AND B.settingID = 20971598
 ) 
 AND A.settingID = 3025
";

        private const string _SQLPRE_EXISTING_CONTACTS = @"select C.Guid as [guid], 'Contact' as ContactType 
from xSharedContacts C
left join xSharedContactPermissions CP on C.Guid = CP.ContactLinkGUID
where CP.ContactLinkGUID is null
union all
select P.Guid as [guid], 'PSAP' as ContactType 
from xSharedPSAPs P
left join xSharedContactPermissions CP on P.Guid = CP.ContactLinkGUID
where CP.ContactLinkGUID is null
union all
select S.Guid as [guid], 'Station' as ContactType 
from xSharedStations S
left join xSharedContactPermissions CP on S.Guid = CP.ContactLinkGUID
where CP.ContactLinkGUID is null
union all
select U.Guid as [guid], 'Agent' as ContactType 
from xSharedUsers U
left join xSharedContactPermissions CP on U.Guid = CP.ContactLinkGUID
where CP.ContactLinkGUID is null
union all
select Q.Guid as [guid], 'Queue' as ContactType 
from xSwitchQueues Q
left join xSharedContactPermissions CP on Q.Guid = CP.ContactLinkGUID
where CP.ContactLinkGUID is null
union all
select F.Guid as [guid], 'Folder' as ContactType 
from xSharedFolders F
left join xSharedContactPermissions CP on F.Guid = CP.ContactLinkGUID
where CP.ContactLinkGUID is null
";

        private const string _SQLCONTACT_PERMISSIONS = @"SELECT GUID as [guid]
	  ,ContactLinkGUID
	  ,ContactLinkType
	  ,memberGUID
	  ,memberType
	  ,ContactPermissionSetting
  FROM xSharedContactPermissions
";

        private const string _SQL_CLEAN = @"
DELETE FROM xSharedFolderLinks
DELETE FROM xSharedFolders

DELETE FROM xSharedIContactToPhone
DELETE FROM xSharedPhoneNumbers

DELETE FROM xSharedIContactToEmail
DELETE FROM xSharedEmails

DELETE FROM xSharedIContactToAddress
DELETE FROM xSharedAddresses

DELETE FROM xSharedIContactToSipUri
DELETE FROM xSharedSipUris

DELETE FROM xSharedContactPermissions

DELETE FROM xSlnSettingValue WHERE settingID = 20971598
";

        private const string _SQL_MIGRATE_XSHARED_AGENCY_ESNRID = @"IF OBJECT_ID(N'dbo.xSharedAgencyESNRID', N'U') IS NOT NULL 
AND OBJECT_ID(N'dbo.xSharedAgencyESNRIDMapping', N'U') IS NOT NULL
AND 0 = (SELECT COUNT(*) FROM xSharedAgencyESNRIDMapping)
BEGIN
	PRINT 'CONVERT data xSharedAgencyESNRID to xSharedAgencyESNRIDMapping'
	SET NOCOUNT ON

	INSERT INTO xSharedAgencyESNRIDMapping (
		--[GUID], --let NewSequentialID() default do its work
		[agencyGUID],
		[ESNGUID],
		[lastUpdate]
	) 
	SELECT
		--[GUID], --don't use, might not be present, might not be unique
		[agencyGUID],
		[ESNGUID],
		[lastUpdate]
	FROM xSharedAgencyESNRID
	SET NOCOUNT OFF
	IF (SELECT COUNT(*) FROM xSharedAgencyESNRIDMapping) = (SELECT COUNT(*) FROM xSharedAgencyESNRID) PRINT 'All records copied' 
END
ELSE PRINT 'Skipped convert xSharedAgencyESNRID to xSharedAgencyESNRIDMapping'";

        private const string _SQL_MIGRATE = @"-- declare eIContactTypes this matches the eIContactTypes enum in code.
DECLARE @contactType TINYINT = 1
DECLARE @psapType TINYINT = 2
DECLARE @stationType TINYINT = 3
DECLARE @agentType TINYINT = 4
DECLARE @queueType TINYINT = 5

-- declare phone type priorities
DECLARE @mainPhonePriority DECIMAL  = '0.0'
DECLARE @pbxExtensionPriority DECIMAL = '10.0'
DECLARE @alternatePhonePriority DECIMAL = '20.0'
DECLARE @starCodePriority DECIMAL = '30.0'
DECLARE @cellPhonePriority DECIMAL = '40.0'
DECLARE @pagerPhoneNumPriority DECIMAL = '50.0'
DECLARE @faxPriority DECIMAL = '60.0'

-- declare default email priority (should only be one per contact during conversion)
DECLARE @emailPriority DECIMAL  = '0.0'

-- declare default address priority (should only be one per contact during conversion)
DECLARE @addressPriority DECIMAL = '0.0'

-- declare default sipUri priority (should only be one per contact during conversion)
DECLARE @sipUriPriority DECIMAL = '0.0'

-- declare phoneNumberType Guids
DECLARE @mainNumberGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedPhoneNumberTypes WHERE phoneNumberType = 'MainNumber')
IF @mainNumberGuid IS NULL
BEGIN
	SELECT @mainNumberGuid = NEWID()
	INSERT INTO xSharedPhoneNumberTypes([guid], phoneNumberType, [description], isDefault, lastUpdate) VALUES (@mainNumberGuid, 'MainNumber', 'MainNumber',1, GETDATE())
END
DECLARE @alternateNumberGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedPhoneNumberTypes WHERE phoneNumberType = 'Alternate Number')
IF @alternateNumberGuid IS NULL
BEGIN
	SELECT @alternateNumberGuid = NEWID()
	INSERT INTO xSharedPhoneNumberTypes([guid], phoneNumberType, [description], isDefault, lastUpdate) VALUES (@alternateNumberGuid, 'Alternate Number', 'Alternate Number',0, GETDATE())
END
DECLARE @faxNumberGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedPhoneNumberTypes WHERE phoneNumberType = 'Fax')
IF @faxNumberGuid IS NULL
BEGIN
	SELECT @faxNumberGuid = NEWID()
	INSERT INTO xSharedPhoneNumberTypes([guid], phoneNumberType, [description], isDefault, lastUpdate) VALUES (@faxNumberGuid, 'Fax', 'Fax',0, GETDATE())
END
DECLARE @starCodeNumberGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedPhoneNumberTypes WHERE phoneNumberType = 'Star Code')
IF @starCodeNumberGuid IS NULL
BEGIN
	SELECT @starCodeNumberGuid = NEWID()
	INSERT INTO xSharedPhoneNumberTypes([guid], phoneNumberType, [description], isDefault, lastUpdate) VALUES (@starCodeNumberGuid, 'Star Code', 'Star Code', 0, GETDATE())
END
DECLARE @cellularNumberGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedPhoneNumberTypes WHERE phoneNumberType = 'Cell')
IF @cellularNumberGuid IS NULL
BEGIN
	SELECT @cellularNumberGuid = NEWID()
	INSERT INTO xSharedPhoneNumberTypes([guid], phoneNumberType, [description], isDefault, lastUpdate) VALUES (@cellularNumberGuid, 'Cell', 'Cell', 0, GETDATE())
END
DECLARE @pagerNumberGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedPhoneNumberTypes WHERE phoneNumberType = 'Pager')
IF @pagerNumberGuid IS NULL
BEGIN
	SELECT @pagerNumberGuid = NEWID()
	INSERT INTO xSharedPhoneNumberTypes([guid], phoneNumberType, [description], isDefault, lastUpdate) VALUES (@pagerNumberGuid, 'Pager', 'Pager',0, GETDATE())
END
DECLARE @pbxExtentionNumberGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedPhoneNumberTypes WHERE phoneNumberType = 'PBXExtention')
IF @pbxExtentionNumberGuid IS NULL
BEGIN
	SELECT @pbxExtentionNumberGuid = NEWID()
	INSERT INTO xSharedPhoneNumberTypes([guid], phoneNumberType, [description], isDefault, lastUpdate) VALUES (@pbxExtentionNumberGuid, 'PBXExtention', 'PBXExtention', 1, GETDATE())
END

-- declare addressType guids
DECLARE @mainAddressGuid UNIQUEIDENTIFIER = (SELECT [guid] FROM xSharedAddressTypes WHERE addressType = 'Main')
IF @mainAddressGuid IS NULL
BEGIN
	SELECT @mainAddressGuid = NEWID()
	INSERT INTO xSharedAddressTypes([guid], addressType, [description], lastUpdate) VALUES (@mainAddressGuid, 'Main', 'Main address', GETDATE())
END

-- temp table creations
IF OBJECT_ID (N'tempdb..#tmpPhoneTable', N'U') IS NOT NULL DROP TABLE #tmpPhoneTable
CREATE TABLE #tmpPhoneTable
(
	recordId UNIQUEIDENTIFIER,
	phoneNumberTypeGuid UNIQUEIDENTIFIER,
	contactType TINYINT,	
	contactGuid UNIQUEIDENTIFIER,
	phoneGuid UNIQUEIDENTIFIER,
	phoneNumber VARCHAR(50),
	phonePriority DECIMAL(14,4)
)

IF OBJECT_ID (N'tempdb..#tmpEmailTable', N'U') IS NOT NULL DROP TABLE #tmpEmailTable
CREATE TABLE #tmpEmailTable
(
	recordId UNIQUEIDENTIFIER,
	contactType TINYINT,	
	contactGuid UNIQUEIDENTIFIER,
	emailGuid UNIQUEIDENTIFIER,
	email VARCHAR(100),
	emailPriority DECIMAL(14,4)
)

IF OBJECT_ID (N'tempdb..#tmpAddressTable', N'U') IS NOT NULL DROP TABLE #tmpAddressTable
CREATE TABLE #tmpAddressTable
(
	recordId UNIQUEIDENTIFIER,
	addressTypeGuid UNIQUEIDENTIFIER,
	contactType TINYINT,	
	contactGuid UNIQUEIDENTIFIER,
	addressGuid UNIQUEIDENTIFIER,
	street VARCHAR(100),
	city VARCHAR(100),
	[state] VARCHAR(2),
	zipCode VARCHAR(10),
	addressPriority DECIMAL(14,4)
)

IF OBJECT_ID (N'tempdb..#tmpSipUriTable', N'U') IS NOT NULL DROP TABLE #tmpSipUriTable
CREATE TABLE #tmpSipUriTable
(
	recordId UNIQUEIDENTIFIER,
	contactType TINYINT,	
	contactGuid UNIQUEIDENTIFIER,
	sipUriGuid UNIQUEIDENTIFIER,
	sipUri VARCHAR(80),
	sipUriPriority DECIMAL(14,4)
)

-- ******** add 'phone' into the xSharedPhoneNumbers table from xSharedContacts, xSharedUsers, xSharedPSAPs ********

-- xSharedContacts
PRINT 'Adding phone into xSharedPhoneNumbers from xSharedContact'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @mainNumberGuid, @contactType, [GUID], NEWID(), CAST(phone as VARCHAR(50)), @mainPhonePriority
FROM xSharedContacts sc
WHERE (phone IS NOT NULL AND DATALENGTH(phone) > 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = sc.guid AND spn.number = sc.Phone
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable
 
TRUNCATE TABLE #tmpPhoneTable
	
-- xSharedUsers	
PRINT 'Adding phone into xSharedPhoneNumbers from xSharedUsers'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @mainNumberGuid, @agentType, [GUID], NEWID(), CAST(phone as VARCHAR(50)), @mainPhonePriority
FROM xSharedUsers su
WHERE (phone IS NOT NULL AND DATALENGTH(phone) > 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = su.guid AND spn.number = su.Phone
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable
 
TRUNCATE TABLE #tmpPhoneTable

-- xSharedPSAPs	
PRINT 'Adding phone into xSharedPhoneNumbers from xSharedPSAPs'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @mainNumberGuid, @psapType, [GUID], NEWID(), CAST(phone as VARCHAR(50)), @mainPhonePriority
FROM xSharedPSAPs sp
WHERE (phone IS NOT NULL AND DATALENGTH(phone) > 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = sp.guid AND spn.number = sp.Phone
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable
 
TRUNCATE TABLE #tmpPhoneTable

	
-- ******** add 'alternatePhone' into the xSharedPhoneNumbers table from xSharedContacts ********

-- xSharedContacts
PRINT 'Adding alternatePhone into xSharedPhoneNumbers from xSharedContacts'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @alternateNumberGuid, @contactType, [GUID], NEWID(), CAST(alternatePhone as VARCHAR(50)), @alternatePhonePriority
FROM xSharedContacts sc
WHERE (alternatePhone IS NOT NULL AND DATALENGTH(alternatePhone) > 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = sc.guid AND spn.number = sc.alternatePhone
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable
 
TRUNCATE TABLE #tmpPhoneTable
	
	
-- ******** add 'fax' into the xSharedPhoneNumbers table from xSharedContacts, xSharedPSAPs ********

-- xSharedContacts
PRINT 'Adding fax into xSharedPhoneNumbers from xSharedContacts'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @faxNumberGuid, @contactType, [GUID], NEWID(), CAST(fax as VARCHAR(50)), @faxPriority
FROM xSharedContacts sc
WHERE (fax IS NOT NULL AND DATALENGTH(fax) > 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = sc.guid AND spn.number = sc.fax
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable
 
TRUNCATE TABLE #tmpPhoneTable

-- xSharedPSAPs	
PRINT 'Adding fax into xSharedPhoneNumbers from xSharedPSAPs'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @faxNumberGuid, @psapType, [GUID], NEWID(), CAST(fax as VARCHAR(50)), @faxPriority
FROM xSharedPSAPs sp
WHERE (fax IS NOT NULL AND DATALENGTH(fax) > 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = sp.guid AND spn.number = sp.fax
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable
 
TRUNCATE TABLE #tmpPhoneTable

	
-- ******** add 'starCode' into the xSharedPhoneNumbers table from xSharedContacts ********

-- xSharedContacts
PRINT 'Adding starCode into xSharedPhoneNumbers from xSharedContacts'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @starCodeNumberGuid, @contactType, [GUID], NEWID(), CAST(starCode as VARCHAR(50)), @starCodePriority
FROM xSharedContacts sc
WHERE (starCode IS NOT NULL AND DATALENGTH(starCode) > 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = sc.guid AND spn.number = sc.starCode
)  

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable
 
TRUNCATE TABLE #tmpPhoneTable
	
	
-- ******** add 'cellPhone' into the xSharedPhoneNumbers table from xSharedContacts ********

-- xSharedContacts
PRINT 'Adding cellPhone into xSharedPhoneNumbers from xSharedContacts'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @cellularNumberGuid, @contactType, [GUID], NEWID(), CAST(cellPhone as VARCHAR(50)), @cellPhonePriority
FROM xSharedContacts sc
WHERE (cellPhone IS NOT NULL AND DATALENGTH(cellPhone) > 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = sc.guid AND spn.number = sc.cellPhone
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable
 
TRUNCATE TABLE #tmpPhoneTable	
	
	
-- ******** add 'pagerPhoneNum' into the xSharedPhoneNumbers table from xSharedContacts	 ********

-- xSharedContacts
PRINT 'Adding pagerPhoneNum into xSharedPhoneNumbers from xSharedContacts'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @pagerNumberGuid, @contactType, [GUID], NEWID(), CAST(pagerPhoneNum as VARCHAR(50)), @pagerPhoneNumPriority
FROM xSharedContacts sc
WHERE (pagerPhoneNum IS NOT NULL AND DATALENGTH(pagerPhoneNum) > 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = sc.guid AND spn.number = sc.pagerPhoneNum
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable
 
TRUNCATE TABLE #tmpPhoneTable	
	
	
-- ******** add 'pbxExtension' into the xSharedPhoneNumbers table from xSharedStations, xSwitchQueues ********

-- xSharedStations
PRINT 'Adding PBXExtension into xSharedPhoneNumbers from xSharedStations'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @pbxExtentionNumberGuid, @stationType, [GUID], NEWID(), CAST(PBXExtension as VARCHAR(50)), @pbxExtensionPriority
FROM xSharedStations ss
WHERE (PBXExtension IS NOT NULL AND DATALENGTH(PBXExtension) > 0 AND PBXExtension <> 0) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = ss.guid AND spn.number = ss.PBXExtension
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable

TRUNCATE TABLE #tmpPhoneTable

-- xSwitchQueues	
PRINT 'Adding pbxExtension into xSharedPhoneNumbers from xSwitchQueues'

INSERT INTO #tmpPhoneTable (recordId, phoneNumberTypeGuid, contactType, contactGuid, phoneGuid, phoneNumber, phonePriority)
SELECT NEWID(), @pbxExtentionNumberGuid, @queueType, [GUID], NEWID(), CAST(pbxExtension as VARCHAR(50)), @pbxExtensionPriority
FROM xSwitchQueues sq
WHERE (pbxExtension IS NOT NULL AND DATALENGTH(pbxExtension) > 0 AND PBXExtension <> -1) AND
NOT EXISTS (
	SELECT sc2p.iContactGuid, spn.phoneNumberTypeGuid, spn.number 
	FROM xSharedIContactToPhone sc2p
	INNER JOIN xSharedPhoneNumbers spn ON spn.guid = sc2p.phoneGuid 
	WHERE sc2p.iContactGuid = sq.guid AND spn.number = sq.pbxExtension
)

INSERT INTO xSharedPhoneNumbers ([guid], phoneNumberTypeGuid, number)
SELECT phoneGuid, phoneNumberTypeGuid, phoneNumber
FROM #tmpPhoneTable

INSERT INTO xSharedIContactToPhone ([guid], iContactGuid, iContactType, phoneGuid, [priority])
SELECT recordId, contactGuid, contactType, phoneGuid, phonePriority
FROM #tmpPhoneTable

TRUNCATE TABLE #tmpPhoneTable


-- ******** copy the 'name' field from xSharedContacts into the 'firstName' field of xSharedContacts ********
PRINT 'Copying name field from xSharedContacts into the firstName field of xSharedContacts'
UPDATE xSharedContacts 
SET firstName = name
WHERE (name IS NOT NULL) AND (DATALENGTH(name) > 0) AND (firstName IS NULL OR (DATALENGTH(LTRIM(RTRIM(firstName))) = 0)) 

-- ******** add 'emailAddress' into the xSharedEmails table from xSharedContacts, xSharedUsers, xSharedPSAPs  ********

-- xSharedContacts
PRINT 'Adding email into xSharedEmails from xSharedContacts'

INSERT INTO #tmpEmailTable (recordId, contactType, contactGuid, emailGuid, email, emailPriority)
SELECT NEWID(), @contactType, [GUID], NEWID(), CAST(email as VARCHAR(100)), @emailPriority
FROM xSharedContacts sc
WHERE (email IS NOT NULL AND DATALENGTH(email) > 0) AND
NOT EXISTS (
	SELECT sc2e.iContactGuid, se.emailAddress
	FROM xSharedIContactToEmail sc2e
	INNER JOIN xSharedEmails se ON se.guid = sc2e.emailGuid 
	WHERE sc2e.iContactGuid = se.guid AND se.emailAddress = sc.email
)

INSERT INTO xSharedEmails ([guid], emailAddress)
SELECT emailGuid, email
FROM #tmpEmailTable

INSERT INTO xSharedIContactToEmail ([guid], iContactGuid, iContactType, emailGuid, [priority])
SELECT recordId, contactGuid, contactType, emailGuid, emailPriority
FROM #tmpEmailTable

TRUNCATE TABLE #tmpEmailTable

-- xSharedUsers
PRINT 'Adding emailAddress into xSharedEmails from xSharedUsers'

INSERT INTO #tmpEmailTable (recordId, contactType, contactGuid, emailGuid, email, emailPriority)
SELECT NEWID(), @agentType, [GUID], NEWID(), CAST(emailAddress as VARCHAR(100)), @emailPriority
FROM xSharedUsers su
WHERE (emailAddress IS NOT NULL AND DATALENGTH(emailAddress) > 0) AND
NOT EXISTS (
	SELECT sc2e.iContactGuid, se.emailAddress
	FROM xSharedIContactToEmail sc2e
	INNER JOIN xSharedEmails se ON se.guid = sc2e.emailGuid 
	WHERE sc2e.iContactGuid = su.guid AND se.emailAddress = su.emailAddress
)

INSERT INTO xSharedEmails ([guid], emailAddress)
SELECT emailGuid, email
FROM #tmpEmailTable

INSERT INTO xSharedIContactToEmail ([guid], iContactGuid, iContactType, emailGuid, [priority])
SELECT recordId, contactGuid, contactType, emailGuid, emailPriority
FROM #tmpEmailTable

TRUNCATE TABLE #tmpEmailTable
	
-- xSharedPSAPs
PRINT 'Adding email into xSharedEmails from xSharedPSAPs'

INSERT INTO #tmpEmailTable (recordId, contactType, contactGuid, emailGuid, email, emailPriority)
SELECT NEWID(), @psapType, [GUID], NEWID(), CAST(email as VARCHAR(100)), @emailPriority
FROM xSharedPSAPs sp
WHERE (email IS NOT NULL AND DATALENGTH(email) > 0) AND
NOT EXISTS (
	SELECT sc2e.iContactGuid, se.emailAddress
	FROM xSharedIContactToEmail sc2e
	INNER JOIN xSharedEmails se ON se.guid = sc2e.emailGuid 
	WHERE sc2e.iContactGuid = se.guid AND se.emailAddress = sp.email
)

INSERT INTO xSharedEmails ([guid], emailAddress)
SELECT emailGuid, email
FROM #tmpEmailTable

INSERT INTO xSharedIContactToEmail ([guid], iContactGuid, iContactType, emailGuid, [priority])
SELECT recordId, contactGuid, contactType, emailGuid, emailPriority
FROM #tmpEmailTable

TRUNCATE TABLE #tmpEmailTable


-- ******** add 'address values' into the xSharedAddresses table from xSharedContacts, xSharedUsers, xSharedPSAPs  ********

-- xSharedContacts
PRINT 'Adding address information into xSharedAddress from xSharedContacts'

INSERT INTO #tmpAddressTable (recordId, addressTypeGuid, contactType, contactGuid, addressGuid, street, city, [state], zipCode, addressPriority)
SELECT NEWID(), @mainAddressGuid, @contactType, [GUID], NEWID(), CAST([address] AS VARCHAR(100)), CAST(community AS VARCHAR(100)), CAST([state] AS VARCHAR(100)), CAST(zip AS VARCHAR(10)), @addressPriority
FROM xSharedContacts sc
WHERE (([address] IS NOT NULL AND DATALENGTH([address]) > 0)
	OR (community IS NOT NULL AND DATALENGTH(community) > 0)
	OR ([state] IS NOT NULL AND DATALENGTH([state]) > 0)
	OR (zip IS NOT NULL AND DATALENGTH(zip) > 0)) AND
NOT EXISTS (
	SELECT sc2a.iContactGuid, sa.street, sa.city, sa.state, sa.zipCode
	FROM xSharedIContactToAddress sc2a
	INNER JOIN xSharedAddresses sa ON sa.guid = sc2a.addressGuid
	WHERE sc2a.iContactGuid = sc.guid AND sa.street = sc.address AND sa.city = sc.community AND sa.state = sc.state AND sa.zipCode = sc.zip
)

INSERT INTO xSharedAddresses ([guid], addressTypeGuid, street, city, [state], zipCode)
SELECT addressGuid, addressTypeGuid, street, city, [state], zipCode
FROM #tmpAddressTable

INSERT INTO xSharedIContactToAddress ([guid], iContactGuid, iContactType, addressGuid, [priority])
SELECT recordId, contactGuid, contactType, addressGuid, addressPriority
FROM #tmpAddressTable

TRUNCATE TABLE #tmpAddressTable
	
-- xSharedPSAPs 
PRINT 'Adding address information into xSharedAddress from xSharedPSAPs'

INSERT INTO #tmpAddressTable (recordId, addressTypeGuid, contactType, contactGuid, addressGuid, street, city, [state], zipCode, addressPriority)
SELECT NEWID(), @mainAddressGuid, @psapType, [GUID], NEWID(), CAST((address1 + address2) AS VARCHAR(100)), CAST(city AS VARCHAR(100)), CAST([state] AS VARCHAR(100)), CAST(zip AS VARCHAR(10)), @addressPriority
FROM xSharedPSAPs sp
WHERE ((address1 IS NOT NULL AND DATALENGTH(address1) > 0)
	OR (address2 IS NOT NULL AND DATALENGTH(address2) > 0)
	OR (city IS NOT NULL AND DATALENGTH(city) > 0)
	OR ([state] IS NOT NULL AND DATALENGTH([state]) > 0)
	OR (zip IS NOT NULL AND DATALENGTH(zip) > 0)) AND
NOT EXISTS (
	SELECT sc2a.iContactGuid, sa.street, sa.city, sa.state, sa.zipCode
	FROM xSharedIContactToAddress sc2a
	INNER JOIN xSharedAddresses sa ON sa.guid = sc2a.addressGuid
	WHERE sc2a.iContactGuid = sp.guid AND sa.street = (sp.address1 + sp.address2) AND sa.city = sp.city AND sa.state = sp.state AND sa.zipCode = sp.zip
)

INSERT INTO xSharedAddresses ([guid], addressTypeGuid, street, city, [state], zipCode)
SELECT addressGuid, addressTypeGuid, street, city, [state], zipCode
FROM #tmpAddressTable

INSERT INTO xSharedIContactToAddress ([guid], iContactGuid, iContactType, addressGuid, [priority])
SELECT recordId, contactGuid, contactType, addressGuid, addressPriority
FROM #tmpAddressTable

TRUNCATE TABLE #tmpAddressTable

-- NOTE: combining varChar(60) address1 and varChar(60) address2 fields into a single varChar(100) address field of length 100.  
-- Potential to truncate data.
-- See Messages for records impacted.
Declare @truncatedPsapRecordCount int;
Select @truncatedPsapRecordCount=count([guid]) From xSharedPSAPs Where DATALENGTH(address1 + address2) > 100;
IF @truncatedPsapRecordCount > 0
	BEGIN
		Print 'WARNING: ' + @truncatedPsapRecordCount + ' xSharedPSAPs records had a combined address1 + address2 value greater than 100.  Truncation of data occured!';
	END

-- xSharedUsers
PRINT 'Adding address information into xSharedAddress from xSharedUsers'

INSERT INTO #tmpAddressTable (recordId, addressTypeGuid, contactType, contactGuid, addressGuid, street, city, [state], zipCode, addressPriority)
SELECT NEWID(), @mainAddressGuid, @agentType, [GUID], NEWID(), CAST([address] AS VARCHAR(100)), NULL, NULL, NULL, @addressPriority
FROM xSharedUsers su
WHERE (address IS NOT NULL AND DATALENGTH([address]) > 0) AND
NOT EXISTS (
	SELECT sc2a.iContactGuid, sa.street, sa.city, sa.state, sa.zipCode
	FROM xSharedIContactToAddress sc2a
	INNER JOIN xSharedAddresses sa ON sa.guid = sc2a.addressGuid
	WHERE sc2a.iContactGuid = su.guid AND sa.street = su.address
)

INSERT INTO xSharedAddresses ([guid], addressTypeGuid, street, city, [state], zipCode)
SELECT addressGuid, addressTypeGuid, street, city, [state], zipCode
FROM #tmpAddressTable

INSERT INTO xSharedIContactToAddress ([guid], iContactGuid, iContactType, addressGuid, [priority])
SELECT recordId, contactGuid, contactType, addressGuid, addressPriority
FROM #tmpAddressTable

TRUNCATE TABLE #tmpAddressTable

-- NOTE: single field varChar(250) address is being put into varChar(100) field.  
-- Potential to truncate data.
-- See Messages for records impacted.
Declare @truncatedUserAddressRecordCount int;
Select @truncatedUserAddressRecordCount=count([guid]) From xSharedUsers Where DATALENGTH([address]) > 100;
IF @truncatedUserAddressRecordCount > 0
	BEGIN
		Print 'WARNING: ''' + @truncatedUserAddressRecordCount + ''' xSharedUsers records had an address value greater than 100 in length.  Truncation of data occured!';
	END
		
	
-- ******** add 'sipUris' into the xSharedSipUris table from xSharedContacts, xSwitchQueues, xSharedPSAPs  ********

-- xSharedContacts
PRINT 'Adding sipUri from xSharedContacts into xSharedSipUris'

INSERT INTO #tmpSipUriTable (recordId, contactType, contactGuid, sipUriGuid, sipUri, sipUriPriority)
SELECT NEWID(), @contactType, [GUID], NEWID(), CAST(SIPURI as VARCHAR(80)), @sipUriPriority
FROM xSharedContacts sc
WHERE (SIPURI IS NOT NULL AND DATALENGTH(SIPURI) > 0) AND
NOT EXISTS (
	SELECT sc2s.iContactGuid, ss.sipUri
	FROM xSharedIContactToSIPURI sc2s
	INNER JOIN xSharedSIPURIs ss ON ss.guid = sc2s.sipUriGuid
	WHERE sc2s.iContactGuid = sc.guid AND ss.sipUri = sc.SIPURI
)

INSERT INTO xSharedSipUris ([guid], sipUri)
SELECT sipUriGuid, sipUri
FROM #tmpSipUriTable

INSERT INTO xSharedIContactToSipUri ([guid], iContactGuid, iContactType, sipUriGuid, [priority])
SELECT recordId, contactGuid, contactType, sipUriGuid, sipUriPriority
FROM #tmpSipUriTable

TRUNCATE TABLE #tmpSipUriTable

-- xSwitchQueues
PRINT 'Adding sipUri from xSwitchQueues into xSharedSipUris'

INSERT INTO #tmpSipUriTable (recordId, contactType, contactGuid, sipUriGuid, sipUri, sipUriPriority)
SELECT NEWID(), @queueType, [GUID], NEWID(), CAST(SIPURI as VARCHAR(80)), @sipUriPriority
FROM xSwitchQueues sq
WHERE (SIPURI IS NOT NULL AND DATALENGTH(SIPURI) > 0) AND
NOT EXISTS (
	SELECT sc2s.iContactGuid, ss.sipUri
	FROM xSharedIContactToSIPURI sc2s
	INNER JOIN xSharedSIPURIs ss ON ss.guid = sc2s.sipUriGuid
	WHERE sc2s.iContactGuid = sq.guid AND ss.sipUri = sq.SIPURI
)

INSERT INTO xSharedSipUris ([guid], sipUri)
SELECT sipUriGuid, sipUri
FROM #tmpSipUriTable

INSERT INTO xSharedIContactToSipUri ([guid], iContactGuid, iContactType, sipUriGuid, [priority])
SELECT recordId, contactGuid, contactType, sipUriGuid, sipUriPriority
FROM #tmpSipUriTable

TRUNCATE TABLE #tmpSipUriTable

-- xSharedPSAPs
PRINT 'Adding sipUri from xSharedPSAPs into xSharedSipUris'

INSERT INTO #tmpSipUriTable (recordId, contactType, contactGuid, sipUriGuid, sipUri, sipUriPriority)
SELECT NEWID(), @psapType, [GUID], NEWID(), CAST(SIPURI as VARCHAR(80)), @sipUriPriority
FROM xSharedPSAPs sp
WHERE (SIPURI IS NOT NULL AND DATALENGTH(SIPURI) > 0) AND
NOT EXISTS (
	SELECT sc2s.iContactGuid, ss.sipUri
	FROM xSharedIContactToSIPURI sc2s
	INNER JOIN xSharedSIPURIs ss ON ss.guid = sc2s.sipUriGuid
	WHERE sc2s.iContactGuid = sp.guid AND ss.sipUri = sp.SIPURI
)

INSERT INTO xSharedSipUris ([guid], sipUri)
SELECT sipUriGuid, sipUri
FROM #tmpSipUriTable

INSERT INTO xSharedIContactToSipUri ([guid], iContactGuid, iContactType, sipUriGuid, [priority])
SELECT recordId, contactGuid, contactType, sipUriGuid, sipUriPriority
FROM #tmpSipUriTable

TRUNCATE TABLE #tmpSipUriTable

-- remove temp tables

DROP TABLE #tmpPhoneTable
DROP TABLE #tmpEmailTable
DROP TABLE #tmpAddressTable
DROP TABLE #tmpSipUriTable
";

        private const string _INSERT_XT911_GLOBAL_SETTING = @"
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3003,3000,'XSL Style - ALI Display Format',2,'XSL Style - Ali Display Format','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3004,3000,'CSS Style',2,'CSS Style','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3005,3000,'Minimum Volume',2,'Minimum Volume','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3006,3000,'Maximum Volume',2,'Maximum Volume','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3007,3000,'Minimum Gain',2,'Minimum Gain','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3008,3000,'Maximum Gain',2,'Maximum Gain','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3012,3000,'Support Phone Number',0,'Support Phone Number','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3016,3000,'Application window Location',2,'Application Location on the monitors','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3017,3000,'Create Broadcast Messages',0,'Allow User to send and create Broadcast message.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3018,3000,'Start Map at Start UP',0,'Start Map at Start UP','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3019,3000,'Call Grid View Type',2,'Select either stacked or list view.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3021,3000,'Show Trunks Tab',1,'Show Trunks Tab on Main form Dock Control.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3022,3000,'Consult on conference',0,'Consult on conference','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3024,3000,'Allow Reverse ALI Query',0,'Allow Reverse ALI Query','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3026,3000,'Show Outbound Calls in Active Calls',0,'Show Outbound Calls in Active Calls','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3027,3000,'Activate Customer Info On New Call',0,'Activate Customer Info On New Call','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3030,3000,'Use Mini Toolbar',0,'Use Mini Toolbar for calls, stations, Contacts','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3031,3000,'Force Incident Type',0,'Force Incident Type After call is ended','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3033,3000,'Load Requeue Call Window',0,'Load Requeue Call Window','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3036,3000,'Set Hook State Relays with Recording Playback',0,'Set Hook State Relays with Recording Playback','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3037,3000,'Show The Ring All Call GRID Window',0,'Show The Ring All Call GRID Window','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3038,3000,'Characters to wait to Auto Pop TTY',0,'Characters to wait to Auto Pop TTY','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3039,3000,'Require Comments on Reverse ALI Query',0,'Require Comments on Reverse ALI Query','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3042,3000,'Play Telephony Recording Out Notification Speaker',0,'Play Telephony Recording Out Notification Speaker','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3043,3000,'Show Trouble Ticket Menu',0,'Show Trouble Ticket Menu','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3044,3000,'Application Window Maximized',2,'Application Window Maximized','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3046,3000,'Default Requeue Supervised',0,'Double-click to requeue call defaults to Supervised if True','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3047,3000,'Select Login Queues',0,'Allow user to select queues to login to.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3048,3000,'Auto Logout When Switch Unavailable',0,'Stations will be automatically logged out when xSwitch servers become unavailable','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3050,3000,'Show Call Stats on Ribbon',2,'Show Call Stats on Ribbon','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3051,3000,'Show System Info on Ribbon',0,'Show System Info on Ribbon','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3052,3000,'Agent Busy Reminder Interval',2,'Interval, in seconds, to wait after agent goes Busy before displaying reminder; set to zero to disable.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3053,3000,'Highlight Recent Calls',2,'Sets recent calls cuttoff value, in seconds, for highlighting in Calls History grid; set to zero to disable.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3054,3000,'Play Tone On Broadcast Message',2,'Play Tone On Broadcast Message','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3056,3000,'Leave ALI After Call',2,'Leave ALI After Call','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3059,3000,'Ringer Dock Location',0,'Ringer Dock Location','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3063,3000,'Show Mic on Ribbon',0,'Show Mic on Ribbon','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3064,3000,'ShowContactsExplorer',0,'Contacts with explorer functionality.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3065,3000,'Show Contacts Panel',0,'Legacy Contacts','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3067,3000,'Auto Busy After Call Completion',0,'After a calltaker answers call, set the state of calltaker to busy automatically after call.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3068,3000,'Close dial pad after dial',0,'Close dial pad after dial.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3070,3000,'Queues filtered Calls',0,'Selected queues to show calls in the calls grid','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3071,3000,'Show Admin Help',0,'Show Admin Help','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3072,3000,'Receive Calls While Holding',0,'Receive Calls While Holding','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3074,3000,'Queued Call Sound',0,'Queued Call Sound. ***These settings will only take affect if agent has permission to see queue***','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3075,3000,'Parked Call Sound',0,'Parked Call Sound. ***These settings will only take affect if agent has permission to see queue***','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3076,3000,'Instant Messaging',0,'Instant Messaging','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3077,3000,'Use Intelligent Transfer',0,'Use intelligent transfers for contacts.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3078,3000,'Hide Transfered Calls',0,'Hide calls that have been transferred out.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3080,3000,'Force application focus on ringing calls',0,'Force application focus on ringing calls','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3084,3000,'Log xRouter Messaging',0,'Show xRouter info messages in log','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3086,3000,'Enable Radio Recording',0,'Turn on/off radio recording.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3087,3000,'Minutes To Retain Radio Recordings',0,'Automatically remove radio recordings older than this many minutes.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3088,3000,'Radio Recording Vox Reset Length',0,'Number of seconds of continuous silence before starting a new radio recording.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3089,3000,'Default Recording Playback',2,'Default playback when playing IRR: Telephony or Combined (Radio and Telephony)','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3090,3000,'Allow Park/Hold of Multi-Station Calls',0,'Allow calls that include 3 or more parties to be placed on Park/hold','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3023,3000,'Auto Show TTY On Detection',0,'Show the TTY dialog automatically on character detection','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3098,3000,'Allow Hold',0,'Allow calls to be placed on hold in xT911','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3093,3000,'Auto Login to Queues',0,'Agent automatically logs into queues','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3106,3000,'Auto Busy Duration',0,'The duration (in seconds) that the agent has to wrap up a call','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3034,3000,'Active Calls GRID Layout',2,'Active Calls GRID Layout','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3300,3000,'Agent Disconnect Count down ',0,'Time to busy agent after disconnect ','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3096,3000,'Allow Transfers',0,'Allow Transfers','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3102,3000,'Allow Change Password',0,'Allow agent to Change Password ','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3100,3000,'Allow Merge Calls',0,'Allow Merging Calls ','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3101,3000,'Allow Merge 911 Calls ',0,'Allow Merge 911 Calls','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3013,3000,'Application Windows Layout',2,'Dock manager layout settings for the main form','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3035,3000,'Call History GRID Layout',2,'CallHistoryGRIDLayout','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3301,3000,'Call History Update Method',0,'Call History Update Method','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3061,3000,'Contacts - System',0,'Which contacts are set to show up on my contacts panel. This settings includes the organization.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3062,3000,'Contacts -Personal',2,'Personal rolodex of contacts.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3069,3000,'Contacts - PSAP',1,'PSAP rolodex of contacts.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3105,3000,'Enable Consult',0,'Enable Consult','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3103,3000,'Enable Contacts Import',0,'Enable Contacts Import','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3112,3000,'Enable Statistics Engine',0,'Enable Statistics Engine','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3099,3000,'Group Abandoned Calls',0,'Group abandoned calls in the active calls grid','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3097,3000,'Headset Disconnect Feature',0,'Detect when a headset is disconnected','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3111,3000,'Limit Telephony Recording and Transcript Access by Time',0,'Number of hours to limit access to IRR recordings.  ‘-1’ or ‘0’ enables 30 days of access','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3110,3000,'Limit Telephony Recording and Transcript Access to CallTaker',0,'Limit IRR recording Listen TTY access to CallTakers own calls.','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3095,3000,'Preferred Outbound Trunk Groups',0,'Preferred outbound dial trunk groups','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (3083,3000,'Ring All GRID Layout',2,'Ring All GRID Layout','00000000-0000-0000-0000-000000000000','12/31/2015 5:58:17 PM'); 
			";

        private const string _INSERT_XTRAKKER_GLOBAL_SETTING = @"
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2005,2000,'Call Handling Type',0,'Type of Call Handling. xT911, CML, xDistributor, etc.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2006,2000,'Ignore Rebids',0,'Ignore Rebids','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2007,2000,'Default Release Call Action',0,'Default Release Call Action','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2013,2000,'Pictometry Viewer Settings',0,'Pictometry Viewer Settings for add on or application.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2017,2000,'Allow Edit Layout',0,'Allow Edit Layout','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2018,2000,'Layout Version',0,'Layout Version','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2019,2000,'Application Location',0,'Application Location','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2020,2000,'Application Size',0,'Application Size','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2023,2000,'xCAD Settings',0,'xCAD Settings','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2024,2000,'xAVL Settings',0,'xAVL Settings','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2025,2000,'Use Embedded Picture Viewer',0,'Use Embedded Picture Viewer or Windows Default Picture Viewer','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2026,2000,'Callout Mode',0,'Callout Mode for map callouts','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2027,2000,'Default Map Tool',0,'Default Map Tool. e.g. Zoom Tool, Pointer Tool','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2029,2000,'Data Updates for GIS Data',0,'Data Updates for GIS Data','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2030,2000,'Can Create Enterprise Drawings',0,'Can Create Enterprise Drawings','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2031,2000,'Can Edit Enterprise Drawings',0,'Can Edit Enterprise Drawings','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2032,2000,'Show Drawing Tools And GRID',0,'Show Drawing Tools And GRID','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2034,2000,'Show All Candidates On Call Search',0,'Show All Candidates On Call Search','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2035,2000,'Leave Searches On Map Until Clear',0,'Leaves Search Icons on the Map until user presses the clear or Clear all buttons.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2036,2000,'Application Theme',0,'Application Theme. App Size, Location, GRID Layout, DockManager Layout.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2039,2000,'Use Place Locator Search 1st On Call Searches',0,'Use Place Locator Search 1st On Call Search','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2040,2000,'Auto Show Disc On NRF',0,'Auto Show Disc On NRF','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2041,2000,'Strip Incoming 0 s From Address',0,'Strip Incoming 0 s From Address','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2042,2000,'Dock Calls GRID',0,'Dock Calls GRID','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2043,2000,'Show NRF Form At Beginning Of Call',0,'Show NRF Form At Beginning Of Call, if false it shows at end. This only takes affect if Auto Show Disc is enabled.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2044,2000,'Enable Post Process Refresh',0,'Enabling Post Process Refresh forces a refresh of the map 1 second after YOUR call has been processes. Helps ensure map zooms properly.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2049,2000,'Perform Auto Pictometry Searches.',0,'Perform Auto Pictometry Searches.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2050,2000,'xAVL Refresh Interval',0,'Refresh Interval for AVL events, in seconds','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2051,2000,'xCAD Refresh Interval',0,'Refresh Interval for CAD events, in seconds','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2052,2000,'Local Port Call Handler Connection',0,'Local port call handler connection info','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2054,2000,'Enable Find Button',0,'Enable the Find button on the map toolbar.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2012,2000,'ApplicationTheme',0,'Color Theme of xTrakker. Silver, Blue, or Black.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2033,2000,'ENS Map',0,'ENS Map/this stores all the layer info, workspace info, etc.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2038,2000,'ENS Road Locator',0,'Connection String, Flavor, Locator Name for the ENS Map','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2037,2000,'ENS Site Locator',0,'Connection String, Flavor, Locator Name for the ENS Map','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2016,2000,'Map Dock Layout',0,'Map Dock Layout','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2028,2000,'Picture Viewer Size And Location',0,'Picture Viewer Size And Location','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			 Insert INTO xSharedGlobalSettings (settingID,settingType,settingName,autoOverrideLevel,[description],updateUserGuid, lastUpdate) VALUES (2009,2000,'PSAPs To Listen To',0,'PSAPs to show events for. Calls, CAD, AVL, Drawings.','00000000-0000-0000-0000-000000000000','12/31/2015 6:19:09 PM'); 
			";
    }
}
