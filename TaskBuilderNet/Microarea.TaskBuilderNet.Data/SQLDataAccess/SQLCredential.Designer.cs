using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Data.SQLDataAccess
{
    partial class SQLCredential
    {
        private Label LblLogin;
        private TextBox TbLogin;
        private Label LblPassword;
        private Button BtnOK;
        private Button BtnCancel;
        private TextBox TbPassword;
        private RadioButton RadioSQLServerAuthentication;
        private RadioButton RadioWindowsAuthentication;
        private Label LblDomain;
        private ComboBox ComboDomains;
        private GroupBox groupBoxTypeCredential;
        private RadioButton radioNewCredential;
        private RadioButton radioCurrentCredential;
        private GroupBox groupBoxTypeOfAuthentication;
        private PictureBox imageLogin;
        private ToolTip toolTip;
        private System.Windows.Forms.Button BtnHelp;
        private IContainer components;

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
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SQLCredential));
            this.LblLogin = new System.Windows.Forms.Label();
            this.TbLogin = new System.Windows.Forms.TextBox();
            this.LblPassword = new System.Windows.Forms.Label();
            this.TbPassword = new System.Windows.Forms.TextBox();
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.RadioSQLServerAuthentication = new System.Windows.Forms.RadioButton();
            this.RadioWindowsAuthentication = new System.Windows.Forms.RadioButton();
            this.LblDomain = new System.Windows.Forms.Label();
            this.ComboDomains = new System.Windows.Forms.ComboBox();
            this.groupBoxTypeCredential = new System.Windows.Forms.GroupBox();
            this.imageLogin = new System.Windows.Forms.PictureBox();
            this.groupBoxTypeOfAuthentication = new System.Windows.Forms.GroupBox();
            this.radioNewCredential = new System.Windows.Forms.RadioButton();
            this.radioCurrentCredential = new System.Windows.Forms.RadioButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.BtnHelp = new System.Windows.Forms.Button();
            this.groupBoxTypeCredential.SuspendLayout();
            this.groupBoxTypeOfAuthentication.SuspendLayout();
            this.SuspendLayout();
            // 
            // LblLogin
            // 
            this.LblLogin.AccessibleDescription = resources.GetString("LblLogin.AccessibleDescription");
            this.LblLogin.AccessibleName = resources.GetString("LblLogin.AccessibleName");
            this.LblLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblLogin.Anchor")));
            this.LblLogin.AutoSize = ((bool)(resources.GetObject("LblLogin.AutoSize")));
            this.LblLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblLogin.Dock")));
            this.LblLogin.Enabled = ((bool)(resources.GetObject("LblLogin.Enabled")));
            this.LblLogin.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblLogin.Font = ((System.Drawing.Font)(resources.GetObject("LblLogin.Font")));
            this.LblLogin.Image = ((System.Drawing.Image)(resources.GetObject("LblLogin.Image")));
            this.LblLogin.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblLogin.ImageAlign")));
            this.LblLogin.ImageIndex = ((int)(resources.GetObject("LblLogin.ImageIndex")));
            this.LblLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblLogin.ImeMode")));
            this.LblLogin.Location = ((System.Drawing.Point)(resources.GetObject("LblLogin.Location")));
            this.LblLogin.Name = "LblLogin";
            this.LblLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblLogin.RightToLeft")));
            this.LblLogin.Size = ((System.Drawing.Size)(resources.GetObject("LblLogin.Size")));
            this.LblLogin.TabIndex = ((int)(resources.GetObject("LblLogin.TabIndex")));
            this.LblLogin.Text = resources.GetString("LblLogin.Text");
            this.LblLogin.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblLogin.TextAlign")));
            this.toolTip.SetToolTip(this.LblLogin, resources.GetString("LblLogin.ToolTip"));
            this.LblLogin.Visible = ((bool)(resources.GetObject("LblLogin.Visible")));
            // 
            // TbLogin
            // 
            this.TbLogin.AccessibleDescription = resources.GetString("TbLogin.AccessibleDescription");
            this.TbLogin.AccessibleName = resources.GetString("TbLogin.AccessibleName");
            this.TbLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TbLogin.Anchor")));
            this.TbLogin.AutoSize = ((bool)(resources.GetObject("TbLogin.AutoSize")));
            this.TbLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TbLogin.BackgroundImage")));
            this.TbLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TbLogin.Dock")));
            this.TbLogin.Enabled = ((bool)(resources.GetObject("TbLogin.Enabled")));
            this.TbLogin.Font = ((System.Drawing.Font)(resources.GetObject("TbLogin.Font")));
            this.TbLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TbLogin.ImeMode")));
            this.TbLogin.Location = ((System.Drawing.Point)(resources.GetObject("TbLogin.Location")));
            this.TbLogin.MaxLength = ((int)(resources.GetObject("TbLogin.MaxLength")));
            this.TbLogin.Multiline = ((bool)(resources.GetObject("TbLogin.Multiline")));
            this.TbLogin.Name = "TbLogin";
            this.TbLogin.PasswordChar = ((char)(resources.GetObject("TbLogin.PasswordChar")));
            this.TbLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TbLogin.RightToLeft")));
            this.TbLogin.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TbLogin.ScrollBars")));
            this.TbLogin.Size = ((System.Drawing.Size)(resources.GetObject("TbLogin.Size")));
            this.TbLogin.TabIndex = ((int)(resources.GetObject("TbLogin.TabIndex")));
            this.TbLogin.Text = resources.GetString("TbLogin.Text");
            this.TbLogin.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TbLogin.TextAlign")));
            this.toolTip.SetToolTip(this.TbLogin, resources.GetString("TbLogin.ToolTip"));
            this.TbLogin.Visible = ((bool)(resources.GetObject("TbLogin.Visible")));
            this.TbLogin.WordWrap = ((bool)(resources.GetObject("TbLogin.WordWrap")));
            this.TbLogin.TextChanged += new System.EventHandler(this.TbLogin_TextChanged);
            // 
            // LblPassword
            // 
            this.LblPassword.AccessibleDescription = resources.GetString("LblPassword.AccessibleDescription");
            this.LblPassword.AccessibleName = resources.GetString("LblPassword.AccessibleName");
            this.LblPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblPassword.Anchor")));
            this.LblPassword.AutoSize = ((bool)(resources.GetObject("LblPassword.AutoSize")));
            this.LblPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblPassword.Dock")));
            this.LblPassword.Enabled = ((bool)(resources.GetObject("LblPassword.Enabled")));
            this.LblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblPassword.Font = ((System.Drawing.Font)(resources.GetObject("LblPassword.Font")));
            this.LblPassword.Image = ((System.Drawing.Image)(resources.GetObject("LblPassword.Image")));
            this.LblPassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblPassword.ImageAlign")));
            this.LblPassword.ImageIndex = ((int)(resources.GetObject("LblPassword.ImageIndex")));
            this.LblPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblPassword.ImeMode")));
            this.LblPassword.Location = ((System.Drawing.Point)(resources.GetObject("LblPassword.Location")));
            this.LblPassword.Name = "LblPassword";
            this.LblPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblPassword.RightToLeft")));
            this.LblPassword.Size = ((System.Drawing.Size)(resources.GetObject("LblPassword.Size")));
            this.LblPassword.TabIndex = ((int)(resources.GetObject("LblPassword.TabIndex")));
            this.LblPassword.Text = resources.GetString("LblPassword.Text");
            this.LblPassword.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblPassword.TextAlign")));
            this.toolTip.SetToolTip(this.LblPassword, resources.GetString("LblPassword.ToolTip"));
            this.LblPassword.Visible = ((bool)(resources.GetObject("LblPassword.Visible")));
            // 
            // TbPassword
            // 
            this.TbPassword.AcceptsReturn = true;
            this.TbPassword.AccessibleDescription = resources.GetString("TbPassword.AccessibleDescription");
            this.TbPassword.AccessibleName = resources.GetString("TbPassword.AccessibleName");
            this.TbPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TbPassword.Anchor")));
            this.TbPassword.AutoSize = ((bool)(resources.GetObject("TbPassword.AutoSize")));
            this.TbPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TbPassword.BackgroundImage")));
            this.TbPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TbPassword.Dock")));
            this.TbPassword.Enabled = ((bool)(resources.GetObject("TbPassword.Enabled")));
            this.TbPassword.Font = ((System.Drawing.Font)(resources.GetObject("TbPassword.Font")));
            this.TbPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TbPassword.ImeMode")));
            this.TbPassword.Location = ((System.Drawing.Point)(resources.GetObject("TbPassword.Location")));
            this.TbPassword.MaxLength = ((int)(resources.GetObject("TbPassword.MaxLength")));
            this.TbPassword.Multiline = ((bool)(resources.GetObject("TbPassword.Multiline")));
            this.TbPassword.Name = "TbPassword";
            this.TbPassword.PasswordChar = ((char)(resources.GetObject("TbPassword.PasswordChar")));
            this.TbPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TbPassword.RightToLeft")));
            this.TbPassword.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TbPassword.ScrollBars")));
            this.TbPassword.Size = ((System.Drawing.Size)(resources.GetObject("TbPassword.Size")));
            this.TbPassword.TabIndex = ((int)(resources.GetObject("TbPassword.TabIndex")));
            this.TbPassword.Text = resources.GetString("TbPassword.Text");
            this.TbPassword.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TbPassword.TextAlign")));
            this.toolTip.SetToolTip(this.TbPassword, resources.GetString("TbPassword.ToolTip"));
            this.TbPassword.Visible = ((bool)(resources.GetObject("TbPassword.Visible")));
            this.TbPassword.WordWrap = ((bool)(resources.GetObject("TbPassword.WordWrap")));
            // 
            // BtnOK
            // 
            this.BtnOK.AccessibleDescription = resources.GetString("BtnOK.AccessibleDescription");
            this.BtnOK.AccessibleName = resources.GetString("BtnOK.AccessibleName");
            this.BtnOK.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOK.Anchor")));
            this.BtnOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOK.BackgroundImage")));
            this.BtnOK.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOK.Dock")));
            this.BtnOK.Enabled = ((bool)(resources.GetObject("BtnOK.Enabled")));
            this.BtnOK.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOK.FlatStyle")));
            this.BtnOK.Font = ((System.Drawing.Font)(resources.GetObject("BtnOK.Font")));
            this.BtnOK.Image = ((System.Drawing.Image)(resources.GetObject("BtnOK.Image")));
            this.BtnOK.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOK.ImageAlign")));
            this.BtnOK.ImageIndex = ((int)(resources.GetObject("BtnOK.ImageIndex")));
            this.BtnOK.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOK.ImeMode")));
            this.BtnOK.Location = ((System.Drawing.Point)(resources.GetObject("BtnOK.Location")));
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOK.RightToLeft")));
            this.BtnOK.Size = ((System.Drawing.Size)(resources.GetObject("BtnOK.Size")));
            this.BtnOK.TabIndex = ((int)(resources.GetObject("BtnOK.TabIndex")));
            this.BtnOK.Text = resources.GetString("BtnOK.Text");
            this.BtnOK.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOK.TextAlign")));
            this.toolTip.SetToolTip(this.BtnOK, resources.GetString("BtnOK.ToolTip"));
            this.BtnOK.Visible = ((bool)(resources.GetObject("BtnOK.Visible")));
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
            this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
            this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
            this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCancel.Dock")));
            this.BtnCancel.Enabled = ((bool)(resources.GetObject("BtnCancel.Enabled")));
            this.BtnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCancel.FlatStyle")));
            this.BtnCancel.Font = ((System.Drawing.Font)(resources.GetObject("BtnCancel.Font")));
            this.BtnCancel.Image = ((System.Drawing.Image)(resources.GetObject("BtnCancel.Image")));
            this.BtnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.ImageAlign")));
            this.BtnCancel.ImageIndex = ((int)(resources.GetObject("BtnCancel.ImageIndex")));
            this.BtnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnCancel.ImeMode")));
            this.BtnCancel.Location = ((System.Drawing.Point)(resources.GetObject("BtnCancel.Location")));
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnCancel.RightToLeft")));
            this.BtnCancel.Size = ((System.Drawing.Size)(resources.GetObject("BtnCancel.Size")));
            this.BtnCancel.TabIndex = ((int)(resources.GetObject("BtnCancel.TabIndex")));
            this.BtnCancel.Text = resources.GetString("BtnCancel.Text");
            this.BtnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.TextAlign")));
            this.toolTip.SetToolTip(this.BtnCancel, resources.GetString("BtnCancel.ToolTip"));
            this.BtnCancel.Visible = ((bool)(resources.GetObject("BtnCancel.Visible")));
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // RadioSQLServerAuthentication
            // 
            this.RadioSQLServerAuthentication.AccessibleDescription = resources.GetString("RadioSQLServerAuthentication.AccessibleDescription");
            this.RadioSQLServerAuthentication.AccessibleName = resources.GetString("RadioSQLServerAuthentication.AccessibleName");
            this.RadioSQLServerAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RadioSQLServerAuthentication.Anchor")));
            this.RadioSQLServerAuthentication.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("RadioSQLServerAuthentication.Appearance")));
            this.RadioSQLServerAuthentication.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RadioSQLServerAuthentication.BackgroundImage")));
            this.RadioSQLServerAuthentication.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioSQLServerAuthentication.CheckAlign")));
            this.RadioSQLServerAuthentication.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RadioSQLServerAuthentication.Dock")));
            this.RadioSQLServerAuthentication.Enabled = ((bool)(resources.GetObject("RadioSQLServerAuthentication.Enabled")));
            this.RadioSQLServerAuthentication.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RadioSQLServerAuthentication.FlatStyle")));
            this.RadioSQLServerAuthentication.Font = ((System.Drawing.Font)(resources.GetObject("RadioSQLServerAuthentication.Font")));
            this.RadioSQLServerAuthentication.Image = ((System.Drawing.Image)(resources.GetObject("RadioSQLServerAuthentication.Image")));
            this.RadioSQLServerAuthentication.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioSQLServerAuthentication.ImageAlign")));
            this.RadioSQLServerAuthentication.ImageIndex = ((int)(resources.GetObject("RadioSQLServerAuthentication.ImageIndex")));
            this.RadioSQLServerAuthentication.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RadioSQLServerAuthentication.ImeMode")));
            this.RadioSQLServerAuthentication.Location = ((System.Drawing.Point)(resources.GetObject("RadioSQLServerAuthentication.Location")));
            this.RadioSQLServerAuthentication.Name = "RadioSQLServerAuthentication";
            this.RadioSQLServerAuthentication.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RadioSQLServerAuthentication.RightToLeft")));
            this.RadioSQLServerAuthentication.Size = ((System.Drawing.Size)(resources.GetObject("RadioSQLServerAuthentication.Size")));
            this.RadioSQLServerAuthentication.TabIndex = ((int)(resources.GetObject("RadioSQLServerAuthentication.TabIndex")));
            this.RadioSQLServerAuthentication.TabStop = true;
            this.RadioSQLServerAuthentication.Text = resources.GetString("RadioSQLServerAuthentication.Text");
            this.RadioSQLServerAuthentication.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioSQLServerAuthentication.TextAlign")));
            this.toolTip.SetToolTip(this.RadioSQLServerAuthentication, resources.GetString("RadioSQLServerAuthentication.ToolTip"));
            this.RadioSQLServerAuthentication.Visible = ((bool)(resources.GetObject("RadioSQLServerAuthentication.Visible")));
            this.RadioSQLServerAuthentication.CheckedChanged += new System.EventHandler(this.RadioSQLServerAuthentication_CheckedChanged);
            // 
            // RadioWindowsAuthentication
            // 
            this.RadioWindowsAuthentication.AccessibleDescription = resources.GetString("RadioWindowsAuthentication.AccessibleDescription");
            this.RadioWindowsAuthentication.AccessibleName = resources.GetString("RadioWindowsAuthentication.AccessibleName");
            this.RadioWindowsAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RadioWindowsAuthentication.Anchor")));
            this.RadioWindowsAuthentication.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("RadioWindowsAuthentication.Appearance")));
            this.RadioWindowsAuthentication.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RadioWindowsAuthentication.BackgroundImage")));
            this.RadioWindowsAuthentication.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioWindowsAuthentication.CheckAlign")));
            this.RadioWindowsAuthentication.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RadioWindowsAuthentication.Dock")));
            this.RadioWindowsAuthentication.Enabled = ((bool)(resources.GetObject("RadioWindowsAuthentication.Enabled")));
            this.RadioWindowsAuthentication.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RadioWindowsAuthentication.FlatStyle")));
            this.RadioWindowsAuthentication.Font = ((System.Drawing.Font)(resources.GetObject("RadioWindowsAuthentication.Font")));
            this.RadioWindowsAuthentication.Image = ((System.Drawing.Image)(resources.GetObject("RadioWindowsAuthentication.Image")));
            this.RadioWindowsAuthentication.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioWindowsAuthentication.ImageAlign")));
            this.RadioWindowsAuthentication.ImageIndex = ((int)(resources.GetObject("RadioWindowsAuthentication.ImageIndex")));
            this.RadioWindowsAuthentication.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RadioWindowsAuthentication.ImeMode")));
            this.RadioWindowsAuthentication.Location = ((System.Drawing.Point)(resources.GetObject("RadioWindowsAuthentication.Location")));
            this.RadioWindowsAuthentication.Name = "RadioWindowsAuthentication";
            this.RadioWindowsAuthentication.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RadioWindowsAuthentication.RightToLeft")));
            this.RadioWindowsAuthentication.Size = ((System.Drawing.Size)(resources.GetObject("RadioWindowsAuthentication.Size")));
            this.RadioWindowsAuthentication.TabIndex = ((int)(resources.GetObject("RadioWindowsAuthentication.TabIndex")));
            this.RadioWindowsAuthentication.TabStop = true;
            this.RadioWindowsAuthentication.Text = resources.GetString("RadioWindowsAuthentication.Text");
            this.RadioWindowsAuthentication.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RadioWindowsAuthentication.TextAlign")));
            this.toolTip.SetToolTip(this.RadioWindowsAuthentication, resources.GetString("RadioWindowsAuthentication.ToolTip"));
            this.RadioWindowsAuthentication.Visible = ((bool)(resources.GetObject("RadioWindowsAuthentication.Visible")));
            this.RadioWindowsAuthentication.CheckedChanged += new System.EventHandler(this.RadioWindowsAuthentication_CheckedChanged);
            // 
            // LblDomain
            // 
            this.LblDomain.AccessibleDescription = resources.GetString("LblDomain.AccessibleDescription");
            this.LblDomain.AccessibleName = resources.GetString("LblDomain.AccessibleName");
            this.LblDomain.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblDomain.Anchor")));
            this.LblDomain.AutoSize = ((bool)(resources.GetObject("LblDomain.AutoSize")));
            this.LblDomain.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblDomain.Dock")));
            this.LblDomain.Enabled = ((bool)(resources.GetObject("LblDomain.Enabled")));
            this.LblDomain.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblDomain.Font = ((System.Drawing.Font)(resources.GetObject("LblDomain.Font")));
            this.LblDomain.Image = ((System.Drawing.Image)(resources.GetObject("LblDomain.Image")));
            this.LblDomain.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblDomain.ImageAlign")));
            this.LblDomain.ImageIndex = ((int)(resources.GetObject("LblDomain.ImageIndex")));
            this.LblDomain.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblDomain.ImeMode")));
            this.LblDomain.Location = ((System.Drawing.Point)(resources.GetObject("LblDomain.Location")));
            this.LblDomain.Name = "LblDomain";
            this.LblDomain.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblDomain.RightToLeft")));
            this.LblDomain.Size = ((System.Drawing.Size)(resources.GetObject("LblDomain.Size")));
            this.LblDomain.TabIndex = ((int)(resources.GetObject("LblDomain.TabIndex")));
            this.LblDomain.Text = resources.GetString("LblDomain.Text");
            this.LblDomain.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblDomain.TextAlign")));
            this.toolTip.SetToolTip(this.LblDomain, resources.GetString("LblDomain.ToolTip"));
            this.LblDomain.Visible = ((bool)(resources.GetObject("LblDomain.Visible")));
            // 
            // ComboDomains
            // 
            this.ComboDomains.AccessibleDescription = resources.GetString("ComboDomains.AccessibleDescription");
            this.ComboDomains.AccessibleName = resources.GetString("ComboDomains.AccessibleName");
            this.ComboDomains.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ComboDomains.Anchor")));
            this.ComboDomains.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ComboDomains.BackgroundImage")));
            this.ComboDomains.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ComboDomains.Dock")));
            this.ComboDomains.Enabled = ((bool)(resources.GetObject("ComboDomains.Enabled")));
            this.ComboDomains.Font = ((System.Drawing.Font)(resources.GetObject("ComboDomains.Font")));
            this.ComboDomains.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ComboDomains.ImeMode")));
            this.ComboDomains.IntegralHeight = ((bool)(resources.GetObject("ComboDomains.IntegralHeight")));
            this.ComboDomains.ItemHeight = ((int)(resources.GetObject("ComboDomains.ItemHeight")));
            this.ComboDomains.Location = ((System.Drawing.Point)(resources.GetObject("ComboDomains.Location")));
            this.ComboDomains.MaxDropDownItems = ((int)(resources.GetObject("ComboDomains.MaxDropDownItems")));
            this.ComboDomains.MaxLength = ((int)(resources.GetObject("ComboDomains.MaxLength")));
            this.ComboDomains.Name = "ComboDomains";
            this.ComboDomains.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ComboDomains.RightToLeft")));
            this.ComboDomains.Size = ((System.Drawing.Size)(resources.GetObject("ComboDomains.Size")));
            this.ComboDomains.TabIndex = ((int)(resources.GetObject("ComboDomains.TabIndex")));
            this.ComboDomains.Text = resources.GetString("ComboDomains.Text");
            this.toolTip.SetToolTip(this.ComboDomains, resources.GetString("ComboDomains.ToolTip"));
            this.ComboDomains.Visible = ((bool)(resources.GetObject("ComboDomains.Visible")));
            this.ComboDomains.DropDown += new System.EventHandler(this.ComboDomains_DropDown);
            // 
            // groupBoxTypeCredential
            // 
            this.groupBoxTypeCredential.AccessibleDescription = resources.GetString("groupBoxTypeCredential.AccessibleDescription");
            this.groupBoxTypeCredential.AccessibleName = resources.GetString("groupBoxTypeCredential.AccessibleName");
            this.groupBoxTypeCredential.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBoxTypeCredential.Anchor")));
            this.groupBoxTypeCredential.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBoxTypeCredential.BackgroundImage")));
            this.groupBoxTypeCredential.Controls.Add(this.imageLogin);
            this.groupBoxTypeCredential.Controls.Add(this.groupBoxTypeOfAuthentication);
            this.groupBoxTypeCredential.Controls.Add(this.radioNewCredential);
            this.groupBoxTypeCredential.Controls.Add(this.radioCurrentCredential);
            this.groupBoxTypeCredential.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBoxTypeCredential.Dock")));
            this.groupBoxTypeCredential.Enabled = ((bool)(resources.GetObject("groupBoxTypeCredential.Enabled")));
            this.groupBoxTypeCredential.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxTypeCredential.Font = ((System.Drawing.Font)(resources.GetObject("groupBoxTypeCredential.Font")));
            this.groupBoxTypeCredential.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBoxTypeCredential.ImeMode")));
            this.groupBoxTypeCredential.Location = ((System.Drawing.Point)(resources.GetObject("groupBoxTypeCredential.Location")));
            this.groupBoxTypeCredential.Name = "groupBoxTypeCredential";
            this.groupBoxTypeCredential.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBoxTypeCredential.RightToLeft")));
            this.groupBoxTypeCredential.Size = ((System.Drawing.Size)(resources.GetObject("groupBoxTypeCredential.Size")));
            this.groupBoxTypeCredential.TabIndex = ((int)(resources.GetObject("groupBoxTypeCredential.TabIndex")));
            this.groupBoxTypeCredential.TabStop = false;
            this.groupBoxTypeCredential.Text = resources.GetString("groupBoxTypeCredential.Text");
            this.toolTip.SetToolTip(this.groupBoxTypeCredential, resources.GetString("groupBoxTypeCredential.ToolTip"));
            this.groupBoxTypeCredential.Visible = ((bool)(resources.GetObject("groupBoxTypeCredential.Visible")));
            // 
            // imageLogin
            // 
            this.imageLogin.AccessibleDescription = resources.GetString("imageLogin.AccessibleDescription");
            this.imageLogin.AccessibleName = resources.GetString("imageLogin.AccessibleName");
            this.imageLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("imageLogin.Anchor")));
            this.imageLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("imageLogin.BackgroundImage")));
            this.imageLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("imageLogin.Dock")));
            this.imageLogin.Enabled = ((bool)(resources.GetObject("imageLogin.Enabled")));
            this.imageLogin.Font = ((System.Drawing.Font)(resources.GetObject("imageLogin.Font")));
            this.imageLogin.Image = ((System.Drawing.Image)(resources.GetObject("imageLogin.Image")));
            this.imageLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("imageLogin.ImeMode")));
            this.imageLogin.Location = ((System.Drawing.Point)(resources.GetObject("imageLogin.Location")));
            this.imageLogin.Name = "imageLogin";
            this.imageLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("imageLogin.RightToLeft")));
            this.imageLogin.Size = ((System.Drawing.Size)(resources.GetObject("imageLogin.Size")));
            this.imageLogin.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("imageLogin.SizeMode")));
            this.imageLogin.TabIndex = ((int)(resources.GetObject("imageLogin.TabIndex")));
            this.imageLogin.TabStop = false;
            this.imageLogin.Text = resources.GetString("imageLogin.Text");
            this.toolTip.SetToolTip(this.imageLogin, resources.GetString("imageLogin.ToolTip"));
            this.imageLogin.Visible = ((bool)(resources.GetObject("imageLogin.Visible")));
            // 
            // groupBoxTypeOfAuthentication
            // 
            this.groupBoxTypeOfAuthentication.AccessibleDescription = resources.GetString("groupBoxTypeOfAuthentication.AccessibleDescription");
            this.groupBoxTypeOfAuthentication.AccessibleName = resources.GetString("groupBoxTypeOfAuthentication.AccessibleName");
            this.groupBoxTypeOfAuthentication.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBoxTypeOfAuthentication.Anchor")));
            this.groupBoxTypeOfAuthentication.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBoxTypeOfAuthentication.BackgroundImage")));
            this.groupBoxTypeOfAuthentication.Controls.Add(this.RadioWindowsAuthentication);
            this.groupBoxTypeOfAuthentication.Controls.Add(this.RadioSQLServerAuthentication);
            this.groupBoxTypeOfAuthentication.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBoxTypeOfAuthentication.Dock")));
            this.groupBoxTypeOfAuthentication.Enabled = ((bool)(resources.GetObject("groupBoxTypeOfAuthentication.Enabled")));
            this.groupBoxTypeOfAuthentication.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxTypeOfAuthentication.Font = ((System.Drawing.Font)(resources.GetObject("groupBoxTypeOfAuthentication.Font")));
            this.groupBoxTypeOfAuthentication.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBoxTypeOfAuthentication.ImeMode")));
            this.groupBoxTypeOfAuthentication.Location = ((System.Drawing.Point)(resources.GetObject("groupBoxTypeOfAuthentication.Location")));
            this.groupBoxTypeOfAuthentication.Name = "groupBoxTypeOfAuthentication";
            this.groupBoxTypeOfAuthentication.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBoxTypeOfAuthentication.RightToLeft")));
            this.groupBoxTypeOfAuthentication.Size = ((System.Drawing.Size)(resources.GetObject("groupBoxTypeOfAuthentication.Size")));
            this.groupBoxTypeOfAuthentication.TabIndex = ((int)(resources.GetObject("groupBoxTypeOfAuthentication.TabIndex")));
            this.groupBoxTypeOfAuthentication.TabStop = false;
            this.groupBoxTypeOfAuthentication.Text = resources.GetString("groupBoxTypeOfAuthentication.Text");
            this.toolTip.SetToolTip(this.groupBoxTypeOfAuthentication, resources.GetString("groupBoxTypeOfAuthentication.ToolTip"));
            this.groupBoxTypeOfAuthentication.Visible = ((bool)(resources.GetObject("groupBoxTypeOfAuthentication.Visible")));
            // 
            // radioNewCredential
            // 
            this.radioNewCredential.AccessibleDescription = resources.GetString("radioNewCredential.AccessibleDescription");
            this.radioNewCredential.AccessibleName = resources.GetString("radioNewCredential.AccessibleName");
            this.radioNewCredential.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioNewCredential.Anchor")));
            this.radioNewCredential.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioNewCredential.Appearance")));
            this.radioNewCredential.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioNewCredential.BackgroundImage")));
            this.radioNewCredential.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewCredential.CheckAlign")));
            this.radioNewCredential.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioNewCredential.Dock")));
            this.radioNewCredential.Enabled = ((bool)(resources.GetObject("radioNewCredential.Enabled")));
            this.radioNewCredential.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioNewCredential.FlatStyle")));
            this.radioNewCredential.Font = ((System.Drawing.Font)(resources.GetObject("radioNewCredential.Font")));
            this.radioNewCredential.Image = ((System.Drawing.Image)(resources.GetObject("radioNewCredential.Image")));
            this.radioNewCredential.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewCredential.ImageAlign")));
            this.radioNewCredential.ImageIndex = ((int)(resources.GetObject("radioNewCredential.ImageIndex")));
            this.radioNewCredential.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioNewCredential.ImeMode")));
            this.radioNewCredential.Location = ((System.Drawing.Point)(resources.GetObject("radioNewCredential.Location")));
            this.radioNewCredential.Name = "radioNewCredential";
            this.radioNewCredential.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioNewCredential.RightToLeft")));
            this.radioNewCredential.Size = ((System.Drawing.Size)(resources.GetObject("radioNewCredential.Size")));
            this.radioNewCredential.TabIndex = ((int)(resources.GetObject("radioNewCredential.TabIndex")));
            this.radioNewCredential.Text = resources.GetString("radioNewCredential.Text");
            this.radioNewCredential.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioNewCredential.TextAlign")));
            this.toolTip.SetToolTip(this.radioNewCredential, resources.GetString("radioNewCredential.ToolTip"));
            this.radioNewCredential.Visible = ((bool)(resources.GetObject("radioNewCredential.Visible")));
            this.radioNewCredential.CheckedChanged += new System.EventHandler(this.radioNewCredential_CheckedChanged);
            // 
            // radioCurrentCredential
            // 
            this.radioCurrentCredential.AccessibleDescription = resources.GetString("radioCurrentCredential.AccessibleDescription");
            this.radioCurrentCredential.AccessibleName = resources.GetString("radioCurrentCredential.AccessibleName");
            this.radioCurrentCredential.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("radioCurrentCredential.Anchor")));
            this.radioCurrentCredential.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("radioCurrentCredential.Appearance")));
            this.radioCurrentCredential.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("radioCurrentCredential.BackgroundImage")));
            this.radioCurrentCredential.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioCurrentCredential.CheckAlign")));
            this.radioCurrentCredential.Checked = true;
            this.radioCurrentCredential.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("radioCurrentCredential.Dock")));
            this.radioCurrentCredential.Enabled = ((bool)(resources.GetObject("radioCurrentCredential.Enabled")));
            this.radioCurrentCredential.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("radioCurrentCredential.FlatStyle")));
            this.radioCurrentCredential.Font = ((System.Drawing.Font)(resources.GetObject("radioCurrentCredential.Font")));
            this.radioCurrentCredential.Image = ((System.Drawing.Image)(resources.GetObject("radioCurrentCredential.Image")));
            this.radioCurrentCredential.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioCurrentCredential.ImageAlign")));
            this.radioCurrentCredential.ImageIndex = ((int)(resources.GetObject("radioCurrentCredential.ImageIndex")));
            this.radioCurrentCredential.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("radioCurrentCredential.ImeMode")));
            this.radioCurrentCredential.Location = ((System.Drawing.Point)(resources.GetObject("radioCurrentCredential.Location")));
            this.radioCurrentCredential.Name = "radioCurrentCredential";
            this.radioCurrentCredential.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("radioCurrentCredential.RightToLeft")));
            this.radioCurrentCredential.Size = ((System.Drawing.Size)(resources.GetObject("radioCurrentCredential.Size")));
            this.radioCurrentCredential.TabIndex = ((int)(resources.GetObject("radioCurrentCredential.TabIndex")));
            this.radioCurrentCredential.TabStop = true;
            this.radioCurrentCredential.Text = resources.GetString("radioCurrentCredential.Text");
            this.radioCurrentCredential.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("radioCurrentCredential.TextAlign")));
            this.toolTip.SetToolTip(this.radioCurrentCredential, resources.GetString("radioCurrentCredential.ToolTip"));
            this.radioCurrentCredential.Visible = ((bool)(resources.GetObject("radioCurrentCredential.Visible")));
            this.radioCurrentCredential.CheckedChanged += new System.EventHandler(this.radioCurrentCredential_CheckedChanged);
            // 
            // toolTip
            // 
            this.toolTip.ShowAlways = true;
            // 
            // BtnHelp
            // 
            this.BtnHelp.AccessibleDescription = resources.GetString("BtnHelp.AccessibleDescription");
            this.BtnHelp.AccessibleName = resources.GetString("BtnHelp.AccessibleName");
            this.BtnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnHelp.Anchor")));
            this.BtnHelp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnHelp.BackgroundImage")));
            this.BtnHelp.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnHelp.Dock")));
            this.BtnHelp.Enabled = ((bool)(resources.GetObject("BtnHelp.Enabled")));
            this.BtnHelp.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnHelp.FlatStyle")));
            this.BtnHelp.Font = ((System.Drawing.Font)(resources.GetObject("BtnHelp.Font")));
            this.BtnHelp.Image = ((System.Drawing.Image)(resources.GetObject("BtnHelp.Image")));
            this.BtnHelp.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnHelp.ImageAlign")));
            this.BtnHelp.ImageIndex = ((int)(resources.GetObject("BtnHelp.ImageIndex")));
            this.BtnHelp.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnHelp.ImeMode")));
            this.BtnHelp.Location = ((System.Drawing.Point)(resources.GetObject("BtnHelp.Location")));
            this.BtnHelp.Name = "BtnHelp";
            this.BtnHelp.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnHelp.RightToLeft")));
            this.BtnHelp.Size = ((System.Drawing.Size)(resources.GetObject("BtnHelp.Size")));
            this.BtnHelp.TabIndex = ((int)(resources.GetObject("BtnHelp.TabIndex")));
            this.BtnHelp.Text = resources.GetString("BtnHelp.Text");
            this.BtnHelp.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnHelp.TextAlign")));
            this.toolTip.SetToolTip(this.BtnHelp, resources.GetString("BtnHelp.ToolTip"));
            this.BtnHelp.Visible = ((bool)(resources.GetObject("BtnHelp.Visible")));
            this.BtnHelp.Click += new System.EventHandler(this.BtnHelp_Click);
            // 
            // Credential
            // 
            this.AcceptButton = this.BtnOK;
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.BtnHelp);
            this.Controls.Add(this.groupBoxTypeCredential);
            this.Controls.Add(this.ComboDomains);
            this.Controls.Add(this.LblDomain);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.TbPassword);
            this.Controls.Add(this.TbLogin);
            this.Controls.Add(this.LblPassword);
            this.Controls.Add(this.LblLogin);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximizeBox = false;
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimizeBox = false;
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "Credential";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.toolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.TopMost = true;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Credential_Closing);
            this.groupBoxTypeCredential.ResumeLayout(false);
            this.groupBoxTypeOfAuthentication.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
