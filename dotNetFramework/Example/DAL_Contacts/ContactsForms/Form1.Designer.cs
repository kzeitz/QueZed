namespace ContactsForms {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbNewFolder = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbNotSystemOrGroupAdministrator = new System.Windows.Forms.Label();
            this.lbLoggedInName = new System.Windows.Forms.Label();
            this.btLogin = new System.Windows.Forms.Button();
            this.chbGroupAdministrator = new System.Windows.Forms.CheckBox();
            this.chbAdministrator = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbUser = new System.Windows.Forms.ComboBox();
            this.dtvFolders = new Karlton.Utility.WindowsForms.DataTreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.dgvTreeItems = new System.Windows.Forms.DataGridView();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dgvContacts = new System.Windows.Forms.DataGridView();
            this.dgvGroups = new System.Windows.Forms.DataGridView();
            this.dgvContactPermissions = new System.Windows.Forms.DataGridView();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvFolderPermissions = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTreeItems)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContacts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGroups)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContactPermissions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFolderPermissions)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Size = new System.Drawing.Size(1242, 605);
            this.splitContainer1.SplitterDistance = 501;
            this.splitContainer1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.toolStrip1, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.dtvFolders, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.dgvTreeItems, 0, 6);
            this.tableLayoutPanel2.Controls.Add(this.label9, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.label8, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label10, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 7;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(499, 603);
            this.tableLayoutPanel2.TabIndex = 14;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNewFolder,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(3, 165);
            this.toolStrip1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip1.Size = new System.Drawing.Size(493, 25);
            this.toolStrip1.TabIndex = 16;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbNewFolder
            // 
            this.tsbNewFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNewFolder.Image = ((System.Drawing.Image)(resources.GetObject("tsbNewFolder.Image")));
            this.tsbNewFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNewFolder.Name = "tsbNewFolder";
            this.tsbNewFolder.Size = new System.Drawing.Size(23, 22);
            this.tsbNewFolder.Text = "Create a new folder with currently selected permissions.";
            this.tsbNewFolder.Click += new System.EventHandler(this.tsbNewFolder_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Remove Folder";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbNotSystemOrGroupAdministrator);
            this.panel1.Controls.Add(this.lbLoggedInName);
            this.panel1.Controls.Add(this.btLogin);
            this.panel1.Controls.Add(this.chbGroupAdministrator);
            this.panel1.Controls.Add(this.chbAdministrator);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.cmbUser);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 23);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(493, 119);
            this.panel1.TabIndex = 13;
            // 
            // lbNotSystemOrGroupAdministrator
            // 
            this.lbNotSystemOrGroupAdministrator.AutoSize = true;
            this.lbNotSystemOrGroupAdministrator.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbNotSystemOrGroupAdministrator.Location = new System.Drawing.Point(37, 81);
            this.lbNotSystemOrGroupAdministrator.Name = "lbNotSystemOrGroupAdministrator";
            this.lbNotSystemOrGroupAdministrator.Size = new System.Drawing.Size(281, 13);
            this.lbNotSystemOrGroupAdministrator.TabIndex = 15;
            this.lbNotSystemOrGroupAdministrator.Text = "User can only administer as the logged in group:";
            // 
            // lbLoggedInName
            // 
            this.lbLoggedInName.AutoSize = true;
            this.lbLoggedInName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLoggedInName.Location = new System.Drawing.Point(251, 15);
            this.lbLoggedInName.Name = "lbLoggedInName";
            this.lbLoggedInName.Size = new System.Drawing.Size(84, 13);
            this.lbLoggedInName.TabIndex = 14;
            this.lbLoggedInName.Text = "Logged in as:";
            // 
            // btLogin
            // 
            this.btLogin.Location = new System.Drawing.Point(170, 10);
            this.btLogin.Name = "btLogin";
            this.btLogin.Size = new System.Drawing.Size(75, 23);
            this.btLogin.TabIndex = 13;
            this.btLogin.Text = "Login";
            this.btLogin.UseVisualStyleBackColor = true;
            this.btLogin.Click += new System.EventHandler(this.btLogin_Click);
            // 
            // chbGroupAdministrator
            // 
            this.chbGroupAdministrator.AutoCheck = false;
            this.chbGroupAdministrator.Location = new System.Drawing.Point(40, 61);
            this.chbGroupAdministrator.Name = "chbGroupAdministrator";
            this.chbGroupAdministrator.Size = new System.Drawing.Size(123, 17);
            this.chbGroupAdministrator.TabIndex = 11;
            this.chbGroupAdministrator.Text = "Group Administrator";
            this.chbGroupAdministrator.UseVisualStyleBackColor = true;
            // 
            // chbAdministrator
            // 
            this.chbAdministrator.AutoCheck = false;
            this.chbAdministrator.Location = new System.Drawing.Point(40, 38);
            this.chbAdministrator.Name = "chbAdministrator";
            this.chbAdministrator.Size = new System.Drawing.Size(123, 17);
            this.chbAdministrator.TabIndex = 11;
            this.chbAdministrator.Text = "System Administrator";
            this.chbAdministrator.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "User";
            // 
            // cmbUser
            // 
            this.cmbUser.FormattingEnabled = true;
            this.cmbUser.Location = new System.Drawing.Point(40, 11);
            this.cmbUser.Name = "cmbUser";
            this.cmbUser.Size = new System.Drawing.Size(123, 21);
            this.cmbUser.TabIndex = 0;
            // 
            // dtvFolders
            // 
            this.dtvFolders.AllowDrop = true;
            this.dtvFolders.DataMember = null;
            this.dtvFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtvFolders.FullRowSelect = true;
            this.dtvFolders.HideSelection = false;
            this.dtvFolders.HotTracking = true;
            this.dtvFolders.ImageKey = "externalNode";
            this.dtvFolders.ImageList = this.imageList;
            this.dtvFolders.IsLeafColumn = "IsLeaf";
            this.dtvFolders.KeyColumn = "IdString";
            this.dtvFolders.LabelEdit = true;
            this.dtvFolders.Location = new System.Drawing.Point(3, 193);
            this.dtvFolders.Name = "dtvFolders";
            this.dtvFolders.NameColumn = "Name";
            this.dtvFolders.ParentKeyColumn = "ParentIdString";
            this.dtvFolders.SelectedImageKey = "externalNodeSelected";
            this.dtvFolders.Size = new System.Drawing.Size(493, 256);
            this.dtvFolders.TabIndex = 0;
            this.dtvFolders.ValueColumn = null;
            this.dtvFolders.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvFolders_DragDrop);
            this.dtvFolders.DragEnter += new System.Windows.Forms.DragEventHandler(this.tvFolders_DragEnter);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "internalNodeCollaped");
            this.imageList.Images.SetKeyName(1, "internalNodeExpanded");
            this.imageList.Images.SetKeyName(2, "externalNode");
            this.imageList.Images.SetKeyName(3, "externalNodeSelected");
            // 
            // dgvTreeItems
            // 
            this.dgvTreeItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTreeItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTreeItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTreeItems.Location = new System.Drawing.Point(3, 475);
            this.dgvTreeItems.Name = "dgvTreeItems";
            this.dgvTreeItems.RowHeadersWidth = 12;
            this.dgvTreeItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvTreeItems.Size = new System.Drawing.Size(493, 125);
            this.dgvTreeItems.TabIndex = 1;
            this.dgvTreeItems.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvTreeItems_RowEnter);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(3, 452);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(493, 20);
            this.label9.TabIndex = 15;
            this.label9.Text = "Tree Data";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(493, 20);
            this.label8.TabIndex = 14;
            this.label8.Text = "Login Identity";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(3, 145);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(493, 20);
            this.label10.TabIndex = 16;
            this.label10.Text = "Folders";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel1.Controls.Add(this.dgvContacts, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.dgvGroups, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dgvContactPermissions, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.dgvFolderPermissions, 1, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 125F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(735, 603);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // dgvContacts
            // 
            this.dgvContacts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvContacts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvContacts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvContacts.Location = new System.Drawing.Point(3, 168);
            this.dgvContacts.Name = "dgvContacts";
            this.dgvContacts.RowHeadersWidth = 12;
            this.tableLayoutPanel1.SetRowSpan(this.dgvContacts, 3);
            this.dgvContacts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvContacts.Size = new System.Drawing.Size(486, 432);
            this.dgvContacts.TabIndex = 0;
            this.dgvContacts.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvContacts_CellMouseDown);
            this.dgvContacts.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvContacts_RowEnter);
            // 
            // dgvGroups
            // 
            this.dgvGroups.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvGroups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel1.SetColumnSpan(this.dgvGroups, 2);
            this.dgvGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvGroups.Location = new System.Drawing.Point(3, 23);
            this.dgvGroups.Name = "dgvGroups";
            this.dgvGroups.RowHeadersWidth = 12;
            this.dgvGroups.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvGroups.Size = new System.Drawing.Size(729, 119);
            this.dgvGroups.TabIndex = 1;
            this.dgvGroups.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvGroups_CellContentClick);
            this.dgvGroups.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.user_AuthenticationChanged);
            this.dgvGroups.EnabledChanged += new System.EventHandler(this.dgvGroups_EnabledChanged);
            // 
            // dgvContactPermissions
            // 
            this.dgvContactPermissions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvContactPermissions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvContactPermissions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvContactPermissions.Location = new System.Drawing.Point(495, 168);
            this.dgvContactPermissions.Name = "dgvContactPermissions";
            this.dgvContactPermissions.RowHeadersWidth = 12;
            this.dgvContactPermissions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvContactPermissions.Size = new System.Drawing.Size(237, 205);
            this.dgvContactPermissions.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(495, 145);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(237, 20);
            this.label5.TabIndex = 2;
            this.label5.Text = "Selected Contact Permissions";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.tableLayoutPanel1.SetColumnSpan(this.label6, 2);
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(729, 20);
            this.label6.TabIndex = 3;
            this.label6.Text = "Groups";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(3, 145);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(486, 20);
            this.label7.TabIndex = 4;
            this.label7.Text = "Contacts";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(495, 376);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(237, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Selected Folder Permissions";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dgvFolderPermissions
            // 
            this.dgvFolderPermissions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvFolderPermissions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFolderPermissions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFolderPermissions.Location = new System.Drawing.Point(495, 399);
            this.dgvFolderPermissions.Name = "dgvFolderPermissions";
            this.dgvFolderPermissions.RowHeadersWidth = 12;
            this.dgvFolderPermissions.Size = new System.Drawing.Size(237, 201);
            this.dgvFolderPermissions.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1242, 605);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Contact Folder Management";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTreeItems)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContacts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvGroups)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContactPermissions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFolderPermissions)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgvContactPermissions;
        //private System.Windows.Forms.DataGridView dgvAddresses;
        //private System.Windows.Forms.DataGridView dgvPhoneNumbers;
        //private System.Windows.Forms.DataGridView dgvEmails;
        //private System.Windows.Forms.DataGridView dgvSIPURIs;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgvContacts;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbUser;
        private System.Windows.Forms.CheckBox chbAdministrator;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dgvGroups;
        private System.Windows.Forms.CheckBox chbGroupAdministrator;
        private System.Windows.Forms.DataGridView dgvTreeItems;
        private Karlton.Utility.WindowsForms.DataTreeView dtvFolders;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button btLogin;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvFolderPermissions;
        private System.Windows.Forms.Label lbLoggedInName;
        private System.Windows.Forms.Label lbNotSystemOrGroupAdministrator;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbNewFolder;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
    }

}
