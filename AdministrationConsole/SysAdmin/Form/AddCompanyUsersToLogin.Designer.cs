using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class AddCompanyUsersToLogin
    {
        private Label LabelTitle;
        private Label LblExplication;
        private ListView ListViewUsersCompany;
        private Button BtnUnselectAll;
        private Button BtnSelectAll;
        private Button BtnSave;
        private GroupBox GroupDatabaseLogin;
        private RadioButton RadioSelectLogin;
        private RadioButton RadioNewLogin;
        private TextBox TbLoginName;
        private ComboBox CbLogins;
        private GroupBox groupPassword;
        private TextBox TbLoginPassword;
        private Label LblAllUsersJoined;
        private System.Windows.Forms.Label LblGuestUser;
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AddCompanyUsersToLogin));
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LblExplication = new System.Windows.Forms.Label();
            this.ListViewUsersCompany = new System.Windows.Forms.ListView();
            this.BtnUnselectAll = new System.Windows.Forms.Button();
            this.BtnSelectAll = new System.Windows.Forms.Button();
            this.BtnSave = new System.Windows.Forms.Button();
            this.GroupDatabaseLogin = new System.Windows.Forms.GroupBox();
            this.TbLoginName = new System.Windows.Forms.TextBox();
            this.CbLogins = new System.Windows.Forms.ComboBox();
            this.RadioNewLogin = new System.Windows.Forms.RadioButton();
            this.RadioSelectLogin = new System.Windows.Forms.RadioButton();
            this.TbLoginPassword = new System.Windows.Forms.TextBox();
            this.groupPassword = new System.Windows.Forms.GroupBox();
            this.LblAllUsersJoined = new System.Windows.Forms.Label();
            this.LblGuestUser = new System.Windows.Forms.Label();
            this.GroupDatabaseLogin.SuspendLayout();
            this.groupPassword.SuspendLayout();
            this.SuspendLayout();
            // 
            // LabelTitle
            // 
            this.LabelTitle.AccessibleDescription = resources.GetString("LabelTitle.AccessibleDescription");
            this.LabelTitle.AccessibleName = resources.GetString("LabelTitle.AccessibleName");
            this.LabelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LabelTitle.Anchor")));
            this.LabelTitle.AutoSize = ((bool)(resources.GetObject("LabelTitle.AutoSize")));
            this.LabelTitle.BackColor = System.Drawing.Color.CornflowerBlue;
            this.LabelTitle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LabelTitle.Dock")));
            this.LabelTitle.Enabled = ((bool)(resources.GetObject("LabelTitle.Enabled")));
            this.LabelTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LabelTitle.Font = ((System.Drawing.Font)(resources.GetObject("LabelTitle.Font")));
            this.LabelTitle.ForeColor = System.Drawing.Color.White;
            this.LabelTitle.Image = ((System.Drawing.Image)(resources.GetObject("LabelTitle.Image")));
            this.LabelTitle.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LabelTitle.ImageAlign")));
            this.LabelTitle.ImageIndex = ((int)(resources.GetObject("LabelTitle.ImageIndex")));
            this.LabelTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LabelTitle.ImeMode")));
            this.LabelTitle.Location = ((System.Drawing.Point)(resources.GetObject("LabelTitle.Location")));
            this.LabelTitle.Name = "LabelTitle";
            this.LabelTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LabelTitle.RightToLeft")));
            this.LabelTitle.Size = ((System.Drawing.Size)(resources.GetObject("LabelTitle.Size")));
            this.LabelTitle.TabIndex = ((int)(resources.GetObject("LabelTitle.TabIndex")));
            this.LabelTitle.Text = resources.GetString("LabelTitle.Text");
            this.LabelTitle.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LabelTitle.TextAlign")));
            this.LabelTitle.Visible = ((bool)(resources.GetObject("LabelTitle.Visible")));
            // 
            // LblExplication
            // 
            this.LblExplication.AccessibleDescription = resources.GetString("LblExplication.AccessibleDescription");
            this.LblExplication.AccessibleName = resources.GetString("LblExplication.AccessibleName");
            this.LblExplication.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblExplication.Anchor")));
            this.LblExplication.AutoSize = ((bool)(resources.GetObject("LblExplication.AutoSize")));
            this.LblExplication.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblExplication.Dock")));
            this.LblExplication.Enabled = ((bool)(resources.GetObject("LblExplication.Enabled")));
            this.LblExplication.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblExplication.Font = ((System.Drawing.Font)(resources.GetObject("LblExplication.Font")));
            this.LblExplication.Image = ((System.Drawing.Image)(resources.GetObject("LblExplication.Image")));
            this.LblExplication.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblExplication.ImageAlign")));
            this.LblExplication.ImageIndex = ((int)(resources.GetObject("LblExplication.ImageIndex")));
            this.LblExplication.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblExplication.ImeMode")));
            this.LblExplication.Location = ((System.Drawing.Point)(resources.GetObject("LblExplication.Location")));
            this.LblExplication.Name = "LblExplication";
            this.LblExplication.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblExplication.RightToLeft")));
            this.LblExplication.Size = ((System.Drawing.Size)(resources.GetObject("LblExplication.Size")));
            this.LblExplication.TabIndex = ((int)(resources.GetObject("LblExplication.TabIndex")));
            this.LblExplication.Text = resources.GetString("LblExplication.Text");
            this.LblExplication.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblExplication.TextAlign")));
            this.LblExplication.Visible = ((bool)(resources.GetObject("LblExplication.Visible")));
            // 
            // ListViewUsersCompany
            // 
            this.ListViewUsersCompany.AccessibleDescription = resources.GetString("ListViewUsersCompany.AccessibleDescription");
            this.ListViewUsersCompany.AccessibleName = resources.GetString("ListViewUsersCompany.AccessibleName");
            this.ListViewUsersCompany.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("ListViewUsersCompany.Alignment")));
            this.ListViewUsersCompany.AllowDrop = true;
            this.ListViewUsersCompany.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ListViewUsersCompany.Anchor")));
            this.ListViewUsersCompany.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ListViewUsersCompany.BackgroundImage")));
            this.ListViewUsersCompany.CheckBoxes = true;
            this.ListViewUsersCompany.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ListViewUsersCompany.Dock")));
            this.ListViewUsersCompany.Enabled = ((bool)(resources.GetObject("ListViewUsersCompany.Enabled")));
            this.ListViewUsersCompany.Font = ((System.Drawing.Font)(resources.GetObject("ListViewUsersCompany.Font")));
            this.ListViewUsersCompany.FullRowSelect = true;
            this.ListViewUsersCompany.GridLines = true;
            this.ListViewUsersCompany.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ListViewUsersCompany.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ListViewUsersCompany.ImeMode")));
            this.ListViewUsersCompany.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
																								 ((System.Windows.Forms.ListViewItem)(resources.GetObject("ListViewUsersCompany.Items")))});
            this.ListViewUsersCompany.LabelWrap = ((bool)(resources.GetObject("ListViewUsersCompany.LabelWrap")));
            this.ListViewUsersCompany.Location = ((System.Drawing.Point)(resources.GetObject("ListViewUsersCompany.Location")));
            this.ListViewUsersCompany.MultiSelect = false;
            this.ListViewUsersCompany.Name = "ListViewUsersCompany";
            this.ListViewUsersCompany.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ListViewUsersCompany.RightToLeft")));
            this.ListViewUsersCompany.Size = ((System.Drawing.Size)(resources.GetObject("ListViewUsersCompany.Size")));
            this.ListViewUsersCompany.TabIndex = ((int)(resources.GetObject("ListViewUsersCompany.TabIndex")));
            this.ListViewUsersCompany.Text = resources.GetString("ListViewUsersCompany.Text");
            this.ListViewUsersCompany.View = System.Windows.Forms.View.Details;
            this.ListViewUsersCompany.Visible = ((bool)(resources.GetObject("ListViewUsersCompany.Visible")));
            this.ListViewUsersCompany.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ListViewUsersCompany_ItemCheck);
            // 
            // BtnUnselectAll
            // 
            this.BtnUnselectAll.AccessibleDescription = resources.GetString("BtnUnselectAll.AccessibleDescription");
            this.BtnUnselectAll.AccessibleName = resources.GetString("BtnUnselectAll.AccessibleName");
            this.BtnUnselectAll.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnUnselectAll.Anchor")));
            this.BtnUnselectAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnUnselectAll.BackgroundImage")));
            this.BtnUnselectAll.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnUnselectAll.Dock")));
            this.BtnUnselectAll.Enabled = ((bool)(resources.GetObject("BtnUnselectAll.Enabled")));
            this.BtnUnselectAll.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnUnselectAll.FlatStyle")));
            this.BtnUnselectAll.Font = ((System.Drawing.Font)(resources.GetObject("BtnUnselectAll.Font")));
            this.BtnUnselectAll.Image = ((System.Drawing.Image)(resources.GetObject("BtnUnselectAll.Image")));
            this.BtnUnselectAll.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnUnselectAll.ImageAlign")));
            this.BtnUnselectAll.ImageIndex = ((int)(resources.GetObject("BtnUnselectAll.ImageIndex")));
            this.BtnUnselectAll.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnUnselectAll.ImeMode")));
            this.BtnUnselectAll.Location = ((System.Drawing.Point)(resources.GetObject("BtnUnselectAll.Location")));
            this.BtnUnselectAll.Name = "BtnUnselectAll";
            this.BtnUnselectAll.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnUnselectAll.RightToLeft")));
            this.BtnUnselectAll.Size = ((System.Drawing.Size)(resources.GetObject("BtnUnselectAll.Size")));
            this.BtnUnselectAll.TabIndex = ((int)(resources.GetObject("BtnUnselectAll.TabIndex")));
            this.BtnUnselectAll.Text = resources.GetString("BtnUnselectAll.Text");
            this.BtnUnselectAll.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnUnselectAll.TextAlign")));
            this.BtnUnselectAll.Visible = ((bool)(resources.GetObject("BtnUnselectAll.Visible")));
            this.BtnUnselectAll.Click += new System.EventHandler(this.BtnUnselectAll_Click);
            // 
            // BtnSelectAll
            // 
            this.BtnSelectAll.AccessibleDescription = resources.GetString("BtnSelectAll.AccessibleDescription");
            this.BtnSelectAll.AccessibleName = resources.GetString("BtnSelectAll.AccessibleName");
            this.BtnSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnSelectAll.Anchor")));
            this.BtnSelectAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnSelectAll.BackgroundImage")));
            this.BtnSelectAll.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnSelectAll.Dock")));
            this.BtnSelectAll.Enabled = ((bool)(resources.GetObject("BtnSelectAll.Enabled")));
            this.BtnSelectAll.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnSelectAll.FlatStyle")));
            this.BtnSelectAll.Font = ((System.Drawing.Font)(resources.GetObject("BtnSelectAll.Font")));
            this.BtnSelectAll.Image = ((System.Drawing.Image)(resources.GetObject("BtnSelectAll.Image")));
            this.BtnSelectAll.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnSelectAll.ImageAlign")));
            this.BtnSelectAll.ImageIndex = ((int)(resources.GetObject("BtnSelectAll.ImageIndex")));
            this.BtnSelectAll.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnSelectAll.ImeMode")));
            this.BtnSelectAll.Location = ((System.Drawing.Point)(resources.GetObject("BtnSelectAll.Location")));
            this.BtnSelectAll.Name = "BtnSelectAll";
            this.BtnSelectAll.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnSelectAll.RightToLeft")));
            this.BtnSelectAll.Size = ((System.Drawing.Size)(resources.GetObject("BtnSelectAll.Size")));
            this.BtnSelectAll.TabIndex = ((int)(resources.GetObject("BtnSelectAll.TabIndex")));
            this.BtnSelectAll.Text = resources.GetString("BtnSelectAll.Text");
            this.BtnSelectAll.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnSelectAll.TextAlign")));
            this.BtnSelectAll.Visible = ((bool)(resources.GetObject("BtnSelectAll.Visible")));
            this.BtnSelectAll.Click += new System.EventHandler(this.BtnSelectAll_Click);
            // 
            // BtnSave
            // 
            this.BtnSave.AccessibleDescription = resources.GetString("BtnSave.AccessibleDescription");
            this.BtnSave.AccessibleName = resources.GetString("BtnSave.AccessibleName");
            this.BtnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnSave.Anchor")));
            this.BtnSave.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnSave.BackgroundImage")));
            this.BtnSave.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnSave.Dock")));
            this.BtnSave.Enabled = ((bool)(resources.GetObject("BtnSave.Enabled")));
            this.BtnSave.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnSave.FlatStyle")));
            this.BtnSave.Font = ((System.Drawing.Font)(resources.GetObject("BtnSave.Font")));
            this.BtnSave.Image = ((System.Drawing.Image)(resources.GetObject("BtnSave.Image")));
            this.BtnSave.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnSave.ImageAlign")));
            this.BtnSave.ImageIndex = ((int)(resources.GetObject("BtnSave.ImageIndex")));
            this.BtnSave.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnSave.ImeMode")));
            this.BtnSave.Location = ((System.Drawing.Point)(resources.GetObject("BtnSave.Location")));
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnSave.RightToLeft")));
            this.BtnSave.Size = ((System.Drawing.Size)(resources.GetObject("BtnSave.Size")));
            this.BtnSave.TabIndex = ((int)(resources.GetObject("BtnSave.TabIndex")));
            this.BtnSave.Text = resources.GetString("BtnSave.Text");
            this.BtnSave.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnSave.TextAlign")));
            this.BtnSave.Visible = ((bool)(resources.GetObject("BtnSave.Visible")));
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // GroupDatabaseLogin
            // 
            this.GroupDatabaseLogin.AccessibleDescription = resources.GetString("GroupDatabaseLogin.AccessibleDescription");
            this.GroupDatabaseLogin.AccessibleName = resources.GetString("GroupDatabaseLogin.AccessibleName");
            this.GroupDatabaseLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("GroupDatabaseLogin.Anchor")));
            this.GroupDatabaseLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("GroupDatabaseLogin.BackgroundImage")));
            this.GroupDatabaseLogin.Controls.Add(this.TbLoginName);
            this.GroupDatabaseLogin.Controls.Add(this.CbLogins);
            this.GroupDatabaseLogin.Controls.Add(this.RadioNewLogin);
            this.GroupDatabaseLogin.Controls.Add(this.RadioSelectLogin);
            this.GroupDatabaseLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("GroupDatabaseLogin.Dock")));
            this.GroupDatabaseLogin.Enabled = ((bool)(resources.GetObject("GroupDatabaseLogin.Enabled")));
            this.GroupDatabaseLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupDatabaseLogin.Font = ((System.Drawing.Font)(resources.GetObject("GroupDatabaseLogin.Font")));
            this.GroupDatabaseLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("GroupDatabaseLogin.ImeMode")));
            this.GroupDatabaseLogin.Location = ((System.Drawing.Point)(resources.GetObject("GroupDatabaseLogin.Location")));
            this.GroupDatabaseLogin.Name = "GroupDatabaseLogin";
            this.GroupDatabaseLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("GroupDatabaseLogin.RightToLeft")));
            this.GroupDatabaseLogin.Size = ((System.Drawing.Size)(resources.GetObject("GroupDatabaseLogin.Size")));
            this.GroupDatabaseLogin.TabIndex = ((int)(resources.GetObject("GroupDatabaseLogin.TabIndex")));
            this.GroupDatabaseLogin.TabStop = false;
            this.GroupDatabaseLogin.Text = resources.GetString("GroupDatabaseLogin.Text");
            this.GroupDatabaseLogin.Visible = ((bool)(resources.GetObject("GroupDatabaseLogin.Visible")));
            // 
            // TbLoginName
            // 
            this.TbLoginName.AccessibleDescription = resources.GetString("TbLoginName.AccessibleDescription");
            this.TbLoginName.AccessibleName = resources.GetString("TbLoginName.AccessibleName");
            this.TbLoginName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TbLoginName.Anchor")));
            this.TbLoginName.AutoSize = ((bool)(resources.GetObject("TbLoginName.AutoSize")));
            this.TbLoginName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TbLoginName.BackgroundImage")));
            this.TbLoginName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TbLoginName.Dock")));
            this.TbLoginName.Enabled = ((bool)(resources.GetObject("TbLoginName.Enabled")));
            this.TbLoginName.Font = ((System.Drawing.Font)(resources.GetObject("TbLoginName.Font")));
            this.TbLoginName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TbLoginName.ImeMode")));
            this.TbLoginName.Location = ((System.Drawing.Point)(resources.GetObject("TbLoginName.Location")));
            this.TbLoginName.MaxLength = ((int)(resources.GetObject("TbLoginName.MaxLength")));
            this.TbLoginName.Multiline = ((bool)(resources.GetObject("TbLoginName.Multiline")));
            this.TbLoginName.Name = "TbLoginName";
            this.TbLoginName.PasswordChar = ((char)(resources.GetObject("TbLoginName.PasswordChar")));
            this.TbLoginName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TbLoginName.RightToLeft")));
            this.TbLoginName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TbLoginName.ScrollBars")));
            this.TbLoginName.Size = ((System.Drawing.Size)(resources.GetObject("TbLoginName.Size")));
            this.TbLoginName.TabIndex = ((int)(resources.GetObject("TbLoginName.TabIndex")));
            this.TbLoginName.Text = resources.GetString("TbLoginName.Text");
            this.TbLoginName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TbLoginName.TextAlign")));
            this.TbLoginName.Visible = ((bool)(resources.GetObject("TbLoginName.Visible")));
            this.TbLoginName.WordWrap = ((bool)(resources.GetObject("TbLoginName.WordWrap")));
            this.TbLoginName.TextChanged += new System.EventHandler(this.TbLoginName_TextChanged);
            // 
            // CbLogins
            // 
            this.CbLogins.AccessibleDescription = resources.GetString("CbLogins.AccessibleDescription");
            this.CbLogins.AccessibleName = resources.GetString("CbLogins.AccessibleName");
            this.CbLogins.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CbLogins.Anchor")));
            this.CbLogins.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CbLogins.BackgroundImage")));
            this.CbLogins.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CbLogins.Dock")));
            this.CbLogins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CbLogins.Enabled = ((bool)(resources.GetObject("CbLogins.Enabled")));
            this.CbLogins.Font = ((System.Drawing.Font)(resources.GetObject("CbLogins.Font")));
            this.CbLogins.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CbLogins.ImeMode")));
            this.CbLogins.IntegralHeight = ((bool)(resources.GetObject("CbLogins.IntegralHeight")));
            this.CbLogins.ItemHeight = ((int)(resources.GetObject("CbLogins.ItemHeight")));
            this.CbLogins.Location = ((System.Drawing.Point)(resources.GetObject("CbLogins.Location")));
            this.CbLogins.MaxDropDownItems = ((int)(resources.GetObject("CbLogins.MaxDropDownItems")));
            this.CbLogins.MaxLength = ((int)(resources.GetObject("CbLogins.MaxLength")));
            this.CbLogins.Name = "CbLogins";
            this.CbLogins.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CbLogins.RightToLeft")));
            this.CbLogins.Size = ((System.Drawing.Size)(resources.GetObject("CbLogins.Size")));
            this.CbLogins.TabIndex = ((int)(resources.GetObject("CbLogins.TabIndex")));
            this.CbLogins.Text = resources.GetString("CbLogins.Text");
            this.CbLogins.Visible = ((bool)(resources.GetObject("CbLogins.Visible")));
            this.CbLogins.EnabledChanged += new System.EventHandler(this.CbLogins_EnabledChanged);
            this.CbLogins.DropDown += new System.EventHandler(this.CbLogins_DropDown);
            this.CbLogins.SelectedIndexChanged += new System.EventHandler(this.CbLogins_SelectedIndexChanged);
            // 
            // RadioNewLogin
            // 
            this.RadioNewLogin.AccessibleDescription = resources.GetString("RadioNewLogin.AccessibleDescription");
            this.RadioNewLogin.AccessibleName = resources.GetString("RadioNewLogin.AccessibleName");
            this.RadioNewLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RadioNewLogin.Anchor")));
            this.RadioNewLogin.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("RadioNewLogin.Appearance")));
            this.RadioNewLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RadioNewLogin.BackgroundImage")));
            this.RadioNewLogin.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioNewLogin.CheckAlign")));
            this.RadioNewLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RadioNewLogin.Dock")));
            this.RadioNewLogin.Enabled = ((bool)(resources.GetObject("RadioNewLogin.Enabled")));
            this.RadioNewLogin.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RadioNewLogin.FlatStyle")));
            this.RadioNewLogin.Font = ((System.Drawing.Font)(resources.GetObject("RadioNewLogin.Font")));
            this.RadioNewLogin.Image = ((System.Drawing.Image)(resources.GetObject("RadioNewLogin.Image")));
            this.RadioNewLogin.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioNewLogin.ImageAlign")));
            this.RadioNewLogin.ImageIndex = ((int)(resources.GetObject("RadioNewLogin.ImageIndex")));
            this.RadioNewLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RadioNewLogin.ImeMode")));
            this.RadioNewLogin.Location = ((System.Drawing.Point)(resources.GetObject("RadioNewLogin.Location")));
            this.RadioNewLogin.Name = "RadioNewLogin";
            this.RadioNewLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RadioNewLogin.RightToLeft")));
            this.RadioNewLogin.Size = ((System.Drawing.Size)(resources.GetObject("RadioNewLogin.Size")));
            this.RadioNewLogin.TabIndex = ((int)(resources.GetObject("RadioNewLogin.TabIndex")));
            this.RadioNewLogin.Text = resources.GetString("RadioNewLogin.Text");
            this.RadioNewLogin.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioNewLogin.TextAlign")));
            this.RadioNewLogin.Visible = ((bool)(resources.GetObject("RadioNewLogin.Visible")));
            this.RadioNewLogin.CheckedChanged += new System.EventHandler(this.RadioNewLogin_CheckedChanged);
            // 
            // RadioSelectLogin
            // 
            this.RadioSelectLogin.AccessibleDescription = resources.GetString("RadioSelectLogin.AccessibleDescription");
            this.RadioSelectLogin.AccessibleName = resources.GetString("RadioSelectLogin.AccessibleName");
            this.RadioSelectLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RadioSelectLogin.Anchor")));
            this.RadioSelectLogin.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("RadioSelectLogin.Appearance")));
            this.RadioSelectLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RadioSelectLogin.BackgroundImage")));
            this.RadioSelectLogin.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioSelectLogin.CheckAlign")));
            this.RadioSelectLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RadioSelectLogin.Dock")));
            this.RadioSelectLogin.Enabled = ((bool)(resources.GetObject("RadioSelectLogin.Enabled")));
            this.RadioSelectLogin.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RadioSelectLogin.FlatStyle")));
            this.RadioSelectLogin.Font = ((System.Drawing.Font)(resources.GetObject("RadioSelectLogin.Font")));
            this.RadioSelectLogin.Image = ((System.Drawing.Image)(resources.GetObject("RadioSelectLogin.Image")));
            this.RadioSelectLogin.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioSelectLogin.ImageAlign")));
            this.RadioSelectLogin.ImageIndex = ((int)(resources.GetObject("RadioSelectLogin.ImageIndex")));
            this.RadioSelectLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RadioSelectLogin.ImeMode")));
            this.RadioSelectLogin.Location = ((System.Drawing.Point)(resources.GetObject("RadioSelectLogin.Location")));
            this.RadioSelectLogin.Name = "RadioSelectLogin";
            this.RadioSelectLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RadioSelectLogin.RightToLeft")));
            this.RadioSelectLogin.Size = ((System.Drawing.Size)(resources.GetObject("RadioSelectLogin.Size")));
            this.RadioSelectLogin.TabIndex = ((int)(resources.GetObject("RadioSelectLogin.TabIndex")));
            this.RadioSelectLogin.Text = resources.GetString("RadioSelectLogin.Text");
            this.RadioSelectLogin.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioSelectLogin.TextAlign")));
            this.RadioSelectLogin.Visible = ((bool)(resources.GetObject("RadioSelectLogin.Visible")));
            // 
            // TbLoginPassword
            // 
            this.TbLoginPassword.AccessibleDescription = resources.GetString("TbLoginPassword.AccessibleDescription");
            this.TbLoginPassword.AccessibleName = resources.GetString("TbLoginPassword.AccessibleName");
            this.TbLoginPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TbLoginPassword.Anchor")));
            this.TbLoginPassword.AutoSize = ((bool)(resources.GetObject("TbLoginPassword.AutoSize")));
            this.TbLoginPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TbLoginPassword.BackgroundImage")));
            this.TbLoginPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TbLoginPassword.Dock")));
            this.TbLoginPassword.Enabled = ((bool)(resources.GetObject("TbLoginPassword.Enabled")));
            this.TbLoginPassword.Font = ((System.Drawing.Font)(resources.GetObject("TbLoginPassword.Font")));
            this.TbLoginPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TbLoginPassword.ImeMode")));
            this.TbLoginPassword.Location = ((System.Drawing.Point)(resources.GetObject("TbLoginPassword.Location")));
            this.TbLoginPassword.MaxLength = ((int)(resources.GetObject("TbLoginPassword.MaxLength")));
            this.TbLoginPassword.Multiline = ((bool)(resources.GetObject("TbLoginPassword.Multiline")));
            this.TbLoginPassword.Name = "TbLoginPassword";
            this.TbLoginPassword.PasswordChar = ((char)(resources.GetObject("TbLoginPassword.PasswordChar")));
            this.TbLoginPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TbLoginPassword.RightToLeft")));
            this.TbLoginPassword.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TbLoginPassword.ScrollBars")));
            this.TbLoginPassword.Size = ((System.Drawing.Size)(resources.GetObject("TbLoginPassword.Size")));
            this.TbLoginPassword.TabIndex = ((int)(resources.GetObject("TbLoginPassword.TabIndex")));
            this.TbLoginPassword.Text = resources.GetString("TbLoginPassword.Text");
            this.TbLoginPassword.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TbLoginPassword.TextAlign")));
            this.TbLoginPassword.Visible = ((bool)(resources.GetObject("TbLoginPassword.Visible")));
            this.TbLoginPassword.WordWrap = ((bool)(resources.GetObject("TbLoginPassword.WordWrap")));
            // 
            // groupPassword
            // 
            this.groupPassword.AccessibleDescription = resources.GetString("groupPassword.AccessibleDescription");
            this.groupPassword.AccessibleName = resources.GetString("groupPassword.AccessibleName");
            this.groupPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupPassword.Anchor")));
            this.groupPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupPassword.BackgroundImage")));
            this.groupPassword.Controls.Add(this.TbLoginPassword);
            this.groupPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupPassword.Dock")));
            this.groupPassword.Enabled = ((bool)(resources.GetObject("groupPassword.Enabled")));
            this.groupPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupPassword.Font = ((System.Drawing.Font)(resources.GetObject("groupPassword.Font")));
            this.groupPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupPassword.ImeMode")));
            this.groupPassword.Location = ((System.Drawing.Point)(resources.GetObject("groupPassword.Location")));
            this.groupPassword.Name = "groupPassword";
            this.groupPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupPassword.RightToLeft")));
            this.groupPassword.Size = ((System.Drawing.Size)(resources.GetObject("groupPassword.Size")));
            this.groupPassword.TabIndex = ((int)(resources.GetObject("groupPassword.TabIndex")));
            this.groupPassword.TabStop = false;
            this.groupPassword.Text = resources.GetString("groupPassword.Text");
            this.groupPassword.Visible = ((bool)(resources.GetObject("groupPassword.Visible")));
            // 
            // LblAllUsersJoined
            // 
            this.LblAllUsersJoined.AccessibleDescription = resources.GetString("LblAllUsersJoined.AccessibleDescription");
            this.LblAllUsersJoined.AccessibleName = resources.GetString("LblAllUsersJoined.AccessibleName");
            this.LblAllUsersJoined.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblAllUsersJoined.Anchor")));
            this.LblAllUsersJoined.AutoSize = ((bool)(resources.GetObject("LblAllUsersJoined.AutoSize")));
            this.LblAllUsersJoined.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblAllUsersJoined.Dock")));
            this.LblAllUsersJoined.Enabled = ((bool)(resources.GetObject("LblAllUsersJoined.Enabled")));
            this.LblAllUsersJoined.Font = ((System.Drawing.Font)(resources.GetObject("LblAllUsersJoined.Font")));
            this.LblAllUsersJoined.ForeColor = System.Drawing.Color.Red;
            this.LblAllUsersJoined.Image = ((System.Drawing.Image)(resources.GetObject("LblAllUsersJoined.Image")));
            this.LblAllUsersJoined.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblAllUsersJoined.ImageAlign")));
            this.LblAllUsersJoined.ImageIndex = ((int)(resources.GetObject("LblAllUsersJoined.ImageIndex")));
            this.LblAllUsersJoined.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblAllUsersJoined.ImeMode")));
            this.LblAllUsersJoined.Location = ((System.Drawing.Point)(resources.GetObject("LblAllUsersJoined.Location")));
            this.LblAllUsersJoined.Name = "LblAllUsersJoined";
            this.LblAllUsersJoined.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblAllUsersJoined.RightToLeft")));
            this.LblAllUsersJoined.Size = ((System.Drawing.Size)(resources.GetObject("LblAllUsersJoined.Size")));
            this.LblAllUsersJoined.TabIndex = ((int)(resources.GetObject("LblAllUsersJoined.TabIndex")));
            this.LblAllUsersJoined.Text = resources.GetString("LblAllUsersJoined.Text");
            this.LblAllUsersJoined.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblAllUsersJoined.TextAlign")));
            this.LblAllUsersJoined.Visible = ((bool)(resources.GetObject("LblAllUsersJoined.Visible")));
            // 
            // LblGuestUser
            // 
            this.LblGuestUser.AccessibleDescription = resources.GetString("LblGuestUser.AccessibleDescription");
            this.LblGuestUser.AccessibleName = resources.GetString("LblGuestUser.AccessibleName");
            this.LblGuestUser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblGuestUser.Anchor")));
            this.LblGuestUser.AutoSize = ((bool)(resources.GetObject("LblGuestUser.AutoSize")));
            this.LblGuestUser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblGuestUser.Dock")));
            this.LblGuestUser.Enabled = ((bool)(resources.GetObject("LblGuestUser.Enabled")));
            this.LblGuestUser.Font = ((System.Drawing.Font)(resources.GetObject("LblGuestUser.Font")));
            this.LblGuestUser.ForeColor = System.Drawing.Color.Red;
            this.LblGuestUser.Image = ((System.Drawing.Image)(resources.GetObject("LblGuestUser.Image")));
            this.LblGuestUser.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblGuestUser.ImageAlign")));
            this.LblGuestUser.ImageIndex = ((int)(resources.GetObject("LblGuestUser.ImageIndex")));
            this.LblGuestUser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblGuestUser.ImeMode")));
            this.LblGuestUser.Location = ((System.Drawing.Point)(resources.GetObject("LblGuestUser.Location")));
            this.LblGuestUser.Name = "LblGuestUser";
            this.LblGuestUser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblGuestUser.RightToLeft")));
            this.LblGuestUser.Size = ((System.Drawing.Size)(resources.GetObject("LblGuestUser.Size")));
            this.LblGuestUser.TabIndex = ((int)(resources.GetObject("LblGuestUser.TabIndex")));
            this.LblGuestUser.Text = resources.GetString("LblGuestUser.Text");
            this.LblGuestUser.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblGuestUser.TextAlign")));
            this.LblGuestUser.Visible = ((bool)(resources.GetObject("LblGuestUser.Visible")));
            // 
            // AddCompanyUsersToLogin
            // 
            this.AcceptButton = this.BtnSave;
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.LblGuestUser);
            this.Controls.Add(this.LblAllUsersJoined);
            this.Controls.Add(this.GroupDatabaseLogin);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.BtnUnselectAll);
            this.Controls.Add(this.BtnSelectAll);
            this.Controls.Add(this.ListViewUsersCompany);
            this.Controls.Add(this.LblExplication);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.groupPassword);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "AddCompanyUsersToLogin";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Closing += new System.ComponentModel.CancelEventHandler(this.AddCompanyUsersToLogin_Closing);
            this.Load += new System.EventHandler(this.AddCompanyUsersToLogin_Load);
            this.VisibleChanged += new System.EventHandler(this.AddCompanyUsersToLogin_VisibleChanged);
            this.Deactivate += new System.EventHandler(this.AddCompanyUsersToLogin_Deactivate);
            this.GroupDatabaseLogin.ResumeLayout(false);
            this.groupPassword.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
