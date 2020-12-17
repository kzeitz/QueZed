using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ContactsForms {
    using System.Reflection;
    using System.Configuration;
    using Karlton.Utility.WindowsForms;
    using BO;

    using TCS.Models;
    
    public partial class Form1 : Form {
        private BindingSource userBindingSource = new BindingSource();
        private BindingSource contactBindingSource = new BindingSource();

        public Form1() {
            InitializeComponent();
            lbLoggedInName.Text = lbNotSystemOrGroupAdministrator.Text = string.Empty;

            userBindingSource.DataSource = typeof(IUser);
            contactBindingSource.DataSource = typeof(IContact);
            cmbUser.DataSource = BOL.DA.Users; cmbUser.DisplayMember = "Login";

            chbAdministrator.DataBindings.Add("Checked", userBindingSource, "IsSystemAdministrator");
            chbGroupAdministrator.DataBindings.Add("Checked", userBindingSource, "IsGroupAdministrator");
            lbLoggedInName.DataBindings.Add("Text", userBindingSource, "Name");
            lbNotSystemOrGroupAdministrator.DataBindings.Add("Text", userBindingSource, "NotSystemOrGroupAdministratorMessage");

            dgvGroups.DataBindings.Add("MultiSelect", userBindingSource, "IsGroupAdministrator");
            
        }

        private void Form1_Load(object sender, EventArgs e) {
            //dtvFolders.DataSource = BOL.DA.TestFolders;
            //dgvFolders.DataSource = BOL.DA.TestFolders;
        }

        private void user_AuthenticationChanged(object sender, EventArgs e) {
            Cursor.Current = Cursors.WaitCursor;
            if (sender is DataGridView) {
                DataGridView dgv = (DataGridView)sender;

                int? trackRow = GetInstanceField(typeof(DataGridView), dgv, "trackRow") as int?;  // Naughty!
                if (0 == dgv.SelectedRows.Count || -1 == trackRow.Value) return;
                if (dgv.MultiSelect) return;  //if multi-select then user is a multi- group administrator.  No need to re-authenticate.

                IUsers users = (IUsers)cmbUser.DataSource;
                IUser selected = (IUser)cmbUser.SelectedItem;
                IGroup group = (IGroup)dgv.CurrentRow.DataBoundItem;
                users.Open(selected, group);
            }
            if (sender is Button) {
                IUsers users = (IUsers)cmbUser.DataSource;
                IUser selected = (IUser)cmbUser.SelectedItem;
                users.Open(selected);
                userBindingSource.DataSource = BOL.DA.CurrentUser;
                dgvGroups.DataSource = BOL.DA.CurrentUser.Groups;
            }
            dgvContacts.DataSource = BOL.DA.CurrentUser.Contacts;
            //dgvTreeItems.DataSource = dtvFolders.DataSource = BOL.DA.CurrentUser.Folders;
            dgvTreeItems.DataSource = dtvFolders.DataSource = BOL.DA.CurrentUser.FoldersContacts;

            Cursor.Current = Cursors.Default;
        }

        private void dgvContacts_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e) {
           DataGridView dgv = (DataGridView)sender; 
           dgv.DoDragDrop(dgv.SelectedRows, DragDropEffects.Copy);
        }

        private void tvFolders_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(DataGridViewSelectedRowCollection))) e.Effect = DragDropEffects.Copy;
            else e.Effect = DragDropEffects.None;
        }

        private void tvFolders_DragDrop(object sender, DragEventArgs e) {
            DataTreeView tv = (DataTreeView)sender;
            if (e.Data.GetDataPresent(typeof(DataGridViewSelectedRowCollection))) {
                Point clientPoint = tv.PointToClient(new Point(e.X, e.Y));
                TreeNode selectedNode = tv.GetNodeAt(clientPoint);
                ITreeItems treeItems = (ITreeItems)dgvTreeItems.DataSource;
                ITreeItem selectedItem = treeItems.FirstOrDefault(i => i.IdString == selectedNode.Tag.ToString());
                if (selectedItem is IFolder) {
                    IFolder parent = selectedItem as IFolder;
                    IContacts contacts = (IContacts)dgvContacts.DataSource;
                    foreach(DataGridViewRow dgvr in (DataGridViewSelectedRowCollection)e.Data.GetData(typeof(DataGridViewSelectedRowCollection))) {
                        IContact contact = (IContact)dgvr.DataBoundItem;
                        List<ITreeItem> items = contacts.CreateLink(parent, contact);
                    }
                }
            }   
        }

        private void rbAdministerSystem_CheckedChanged(object sender, EventArgs e) {
            RadioButton rb = (RadioButton)sender;
            dgvGroups.Enabled = !rb.Checked;
        }

        private void dgvGroups_EnabledChanged(object sender, EventArgs e) {
            DataGridView dgv = (DataGridView)sender;
            dgv.DefaultCellStyle.BackColor = dgv.Enabled ? SystemColors.Window : SystemColors.Control;
            dgv.DefaultCellStyle.ForeColor = dgv.Enabled ? SystemColors.ControlText : SystemColors.GrayText;
            dgv.CurrentCell = dgv.Enabled ? dgv.CurrentCell : null;
            dgv.ReadOnly = dgv.Enabled;
            dgv.ClearSelection();
        }

        private void dgvContacts_RowEnter(object sender, DataGridViewCellEventArgs e) {
            DataGridView dgv = (DataGridView)sender;
            if (0 == dgv.SelectedRows.Count) return;

            IContacts contacts = (IContacts)dgv.DataSource;
            IContact selected = (IContact)dgv.Rows[e.RowIndex].DataBoundItem;
            selected.Open();

            contactBindingSource.DataSource = BOL.DA.CurrentContact;
            dgvContactPermissions.DataSource = BOL.DA.CurrentContact.Permissions;
        }

        private void dgvTreeItems_RowEnter(object sender, DataGridViewCellEventArgs e) {
            DataGridView dgv = (DataGridView)sender;
            if (0 == dgv.SelectedRows.Count) return;

            //ITestFolder selected = (ITestFolder)dgv.Rows[e.RowIndex].DataBoundItem;
            //BOL.DA.TestFolders.Open(selected);
            //dgvFolderPermissions.DataSource = BOL.DA.CurrentTestFolder.Permissions;

            ITreeItems items = (ITreeItems)dgv.DataSource;
            ITreeItem selected = (ITreeItem)dgv.Rows[e.RowIndex].DataBoundItem;
            selected.Open();
            if (null != BOL.DA.CurrentFolder) dgvFolderPermissions.DataSource = BOL.DA.CurrentFolder.Permissions;
            if (null != BOL.DA.CurrentContact) dgvContactPermissions.DataSource = BOL.DA.CurrentContact.Permissions;
        }

        private void btLogin_Click(object sender, EventArgs e) {
            user_AuthenticationChanged(sender, e);
        }

        internal static object GetInstanceField(Type type, object instance, string fieldName) {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        private void dgvGroups_CellContentClick(object sender, DataGridViewCellEventArgs e) {
            DataGridView dgv = (DataGridView)sender;
            if (0 == dgv.SelectedRows.Count) return;

            // Make the three permission columns work like radio buttons
            DataGridViewCheckBoxCell dgvcbc = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
            if (null == dgvcbc || false == (bool)dgvcbc.EditedFormattedValue) return;

            IGroup selected = (IGroup)dgv.Rows[e.RowIndex].DataBoundItem;
            // Note: these strings have to match the fields in interface IGroup
            string permissionChecked = dgvcbc.OwningColumn.DataPropertyName;
            if (0 == string.Compare("FullAccess", permissionChecked, true)) { selected.FullAccess = true; selected.ReadOnlyAccess = selected.NoAccess = false; } 
            if (0 == string.Compare("ReadOnlyAccess", permissionChecked, true)) { selected.ReadOnlyAccess = true; selected.FullAccess = selected.NoAccess = false;}
            if (0 == string.Compare("NoAccess", permissionChecked, true)) { selected.NoAccess = true; selected.FullAccess = selected.ReadOnlyAccess = false;}
            dgv.Refresh();
        }

        private void tsbNewFolder_Click(object sender, EventArgs e) {
            ITreeItems folders = (ITreeItems)dgvTreeItems.DataSource;
            if (null == folders) return;
            IFolder selected = BOL.DA.CurrentFolder;
            //IFolder newFolder = folders.Create(selected);

            CurrencyManager cm = (dgvTreeItems.BindingContext[dgvTreeItems.DataSource] as CurrencyManager);
            //cm.Position = cm.List.IndexOf(newFolder);
        }

    }

}
