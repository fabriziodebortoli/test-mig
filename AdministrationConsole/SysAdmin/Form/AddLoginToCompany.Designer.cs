using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class AddLoginToCompany
    {
        private Label LblExplication;
        private Label LabelTitle;
        private Label LblLoginName;
        private Label LblLoginPassword;
        private TextBox TextLoginName;
        private TextBox TextLoginPassword;
        private Button BtnSave;
        private ListView ListViewLogins;
        private Label LblTitleList;
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AddLoginToCompany));
            this.LblExplication = new System.Windows.Forms.Label();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.TextLoginPassword = new System.Windows.Forms.TextBox();
            this.LblLoginPassword = new System.Windows.Forms.Label();
            this.LblLoginName = new System.Windows.Forms.Label();
            this.TextLoginName = new System.Windows.Forms.TextBox();
            this.BtnSave = new System.Windows.Forms.Button();
            this.ListViewLogins = new System.Windows.Forms.ListView();
            this.LblTitleList = new System.Windows.Forms.Label();
            this.SuspendLayout();
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
            // TextLoginPassword
            // 
            this.TextLoginPassword.AcceptsReturn = true;
            this.TextLoginPassword.AccessibleDescription = resources.GetString("TextLoginPassword.AccessibleDescription");
            this.TextLoginPassword.AccessibleName = resources.GetString("TextLoginPassword.AccessibleName");
            this.TextLoginPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TextLoginPassword.Anchor")));
            this.TextLoginPassword.AutoSize = ((bool)(resources.GetObject("TextLoginPassword.AutoSize")));
            this.TextLoginPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TextLoginPassword.BackgroundImage")));
            this.TextLoginPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TextLoginPassword.Dock")));
            this.TextLoginPassword.Enabled = ((bool)(resources.GetObject("TextLoginPassword.Enabled")));
            this.TextLoginPassword.Font = ((System.Drawing.Font)(resources.GetObject("TextLoginPassword.Font")));
            this.TextLoginPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TextLoginPassword.ImeMode")));
            this.TextLoginPassword.Location = ((System.Drawing.Point)(resources.GetObject("TextLoginPassword.Location")));
            this.TextLoginPassword.MaxLength = ((int)(resources.GetObject("TextLoginPassword.MaxLength")));
            this.TextLoginPassword.Multiline = ((bool)(resources.GetObject("TextLoginPassword.Multiline")));
            this.TextLoginPassword.Name = "TextLoginPassword";
            this.TextLoginPassword.PasswordChar = ((char)(resources.GetObject("TextLoginPassword.PasswordChar")));
            this.TextLoginPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TextLoginPassword.RightToLeft")));
            this.TextLoginPassword.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TextLoginPassword.ScrollBars")));
            this.TextLoginPassword.Size = ((System.Drawing.Size)(resources.GetObject("TextLoginPassword.Size")));
            this.TextLoginPassword.TabIndex = ((int)(resources.GetObject("TextLoginPassword.TabIndex")));
            this.TextLoginPassword.Text = resources.GetString("TextLoginPassword.Text");
            this.TextLoginPassword.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TextLoginPassword.TextAlign")));
            this.TextLoginPassword.Visible = ((bool)(resources.GetObject("TextLoginPassword.Visible")));
            this.TextLoginPassword.WordWrap = ((bool)(resources.GetObject("TextLoginPassword.WordWrap")));
            this.TextLoginPassword.TextChanged += new System.EventHandler(this.TextLoginPassword_TextChanged);
            // 
            // LblLoginPassword
            // 
            this.LblLoginPassword.AccessibleDescription = resources.GetString("LblLoginPassword.AccessibleDescription");
            this.LblLoginPassword.AccessibleName = resources.GetString("LblLoginPassword.AccessibleName");
            this.LblLoginPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblLoginPassword.Anchor")));
            this.LblLoginPassword.AutoSize = ((bool)(resources.GetObject("LblLoginPassword.AutoSize")));
            this.LblLoginPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblLoginPassword.Dock")));
            this.LblLoginPassword.Enabled = ((bool)(resources.GetObject("LblLoginPassword.Enabled")));
            this.LblLoginPassword.Font = ((System.Drawing.Font)(resources.GetObject("LblLoginPassword.Font")));
            this.LblLoginPassword.Image = ((System.Drawing.Image)(resources.GetObject("LblLoginPassword.Image")));
            this.LblLoginPassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblLoginPassword.ImageAlign")));
            this.LblLoginPassword.ImageIndex = ((int)(resources.GetObject("LblLoginPassword.ImageIndex")));
            this.LblLoginPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblLoginPassword.ImeMode")));
            this.LblLoginPassword.Location = ((System.Drawing.Point)(resources.GetObject("LblLoginPassword.Location")));
            this.LblLoginPassword.Name = "LblLoginPassword";
            this.LblLoginPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblLoginPassword.RightToLeft")));
            this.LblLoginPassword.Size = ((System.Drawing.Size)(resources.GetObject("LblLoginPassword.Size")));
            this.LblLoginPassword.TabIndex = ((int)(resources.GetObject("LblLoginPassword.TabIndex")));
            this.LblLoginPassword.Text = resources.GetString("LblLoginPassword.Text");
            this.LblLoginPassword.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblLoginPassword.TextAlign")));
            this.LblLoginPassword.Visible = ((bool)(resources.GetObject("LblLoginPassword.Visible")));
            // 
            // LblLoginName
            // 
            this.LblLoginName.AccessibleDescription = resources.GetString("LblLoginName.AccessibleDescription");
            this.LblLoginName.AccessibleName = resources.GetString("LblLoginName.AccessibleName");
            this.LblLoginName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblLoginName.Anchor")));
            this.LblLoginName.AutoSize = ((bool)(resources.GetObject("LblLoginName.AutoSize")));
            this.LblLoginName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblLoginName.Dock")));
            this.LblLoginName.Enabled = ((bool)(resources.GetObject("LblLoginName.Enabled")));
            this.LblLoginName.Font = ((System.Drawing.Font)(resources.GetObject("LblLoginName.Font")));
            this.LblLoginName.Image = ((System.Drawing.Image)(resources.GetObject("LblLoginName.Image")));
            this.LblLoginName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblLoginName.ImageAlign")));
            this.LblLoginName.ImageIndex = ((int)(resources.GetObject("LblLoginName.ImageIndex")));
            this.LblLoginName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblLoginName.ImeMode")));
            this.LblLoginName.Location = ((System.Drawing.Point)(resources.GetObject("LblLoginName.Location")));
            this.LblLoginName.Name = "LblLoginName";
            this.LblLoginName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblLoginName.RightToLeft")));
            this.LblLoginName.Size = ((System.Drawing.Size)(resources.GetObject("LblLoginName.Size")));
            this.LblLoginName.TabIndex = ((int)(resources.GetObject("LblLoginName.TabIndex")));
            this.LblLoginName.Text = resources.GetString("LblLoginName.Text");
            this.LblLoginName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblLoginName.TextAlign")));
            this.LblLoginName.Visible = ((bool)(resources.GetObject("LblLoginName.Visible")));
            // 
            // TextLoginName
            // 
            this.TextLoginName.AcceptsReturn = true;
            this.TextLoginName.AccessibleDescription = resources.GetString("TextLoginName.AccessibleDescription");
            this.TextLoginName.AccessibleName = resources.GetString("TextLoginName.AccessibleName");
            this.TextLoginName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("TextLoginName.Anchor")));
            this.TextLoginName.AutoSize = ((bool)(resources.GetObject("TextLoginName.AutoSize")));
            this.TextLoginName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TextLoginName.BackgroundImage")));
            this.TextLoginName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("TextLoginName.Dock")));
            this.TextLoginName.Enabled = ((bool)(resources.GetObject("TextLoginName.Enabled")));
            this.TextLoginName.Font = ((System.Drawing.Font)(resources.GetObject("TextLoginName.Font")));
            this.TextLoginName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("TextLoginName.ImeMode")));
            this.TextLoginName.Location = ((System.Drawing.Point)(resources.GetObject("TextLoginName.Location")));
            this.TextLoginName.MaxLength = ((int)(resources.GetObject("TextLoginName.MaxLength")));
            this.TextLoginName.Multiline = ((bool)(resources.GetObject("TextLoginName.Multiline")));
            this.TextLoginName.Name = "TextLoginName";
            this.TextLoginName.PasswordChar = ((char)(resources.GetObject("TextLoginName.PasswordChar")));
            this.TextLoginName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("TextLoginName.RightToLeft")));
            this.TextLoginName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("TextLoginName.ScrollBars")));
            this.TextLoginName.Size = ((System.Drawing.Size)(resources.GetObject("TextLoginName.Size")));
            this.TextLoginName.TabIndex = ((int)(resources.GetObject("TextLoginName.TabIndex")));
            this.TextLoginName.Text = resources.GetString("TextLoginName.Text");
            this.TextLoginName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("TextLoginName.TextAlign")));
            this.TextLoginName.Visible = ((bool)(resources.GetObject("TextLoginName.Visible")));
            this.TextLoginName.WordWrap = ((bool)(resources.GetObject("TextLoginName.WordWrap")));
            this.TextLoginName.TextChanged += new System.EventHandler(this.TextLoginName_TextChanged);
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
            // ListViewLogins
            // 
            this.ListViewLogins.AccessibleDescription = resources.GetString("ListViewLogins.AccessibleDescription");
            this.ListViewLogins.AccessibleName = resources.GetString("ListViewLogins.AccessibleName");
            this.ListViewLogins.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("ListViewLogins.Alignment")));
            this.ListViewLogins.AllowDrop = true;
            this.ListViewLogins.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ListViewLogins.Anchor")));
            this.ListViewLogins.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ListViewLogins.BackgroundImage")));
            this.ListViewLogins.CheckBoxes = true;
            this.ListViewLogins.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ListViewLogins.Dock")));
            this.ListViewLogins.Enabled = ((bool)(resources.GetObject("ListViewLogins.Enabled")));
            this.ListViewLogins.Font = ((System.Drawing.Font)(resources.GetObject("ListViewLogins.Font")));
            this.ListViewLogins.FullRowSelect = true;
            this.ListViewLogins.GridLines = true;
            this.ListViewLogins.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.ListViewLogins.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ListViewLogins.ImeMode")));
            this.ListViewLogins.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
																						   ((System.Windows.Forms.ListViewItem)(resources.GetObject("ListViewLogins.Items")))});
            this.ListViewLogins.LabelWrap = ((bool)(resources.GetObject("ListViewLogins.LabelWrap")));
            this.ListViewLogins.Location = ((System.Drawing.Point)(resources.GetObject("ListViewLogins.Location")));
            this.ListViewLogins.MultiSelect = false;
            this.ListViewLogins.Name = "ListViewLogins";
            this.ListViewLogins.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ListViewLogins.RightToLeft")));
            this.ListViewLogins.Size = ((System.Drawing.Size)(resources.GetObject("ListViewLogins.Size")));
            this.ListViewLogins.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.ListViewLogins.TabIndex = ((int)(resources.GetObject("ListViewLogins.TabIndex")));
            this.ListViewLogins.Text = resources.GetString("ListViewLogins.Text");
            this.ListViewLogins.View = System.Windows.Forms.View.Details;
            this.ListViewLogins.Visible = ((bool)(resources.GetObject("ListViewLogins.Visible")));
            // 
            // LblTitleList
            // 
            this.LblTitleList.AccessibleDescription = resources.GetString("LblTitleList.AccessibleDescription");
            this.LblTitleList.AccessibleName = resources.GetString("LblTitleList.AccessibleName");
            this.LblTitleList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblTitleList.Anchor")));
            this.LblTitleList.AutoSize = ((bool)(resources.GetObject("LblTitleList.AutoSize")));
            this.LblTitleList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblTitleList.Dock")));
            this.LblTitleList.Enabled = ((bool)(resources.GetObject("LblTitleList.Enabled")));
            this.LblTitleList.Font = ((System.Drawing.Font)(resources.GetObject("LblTitleList.Font")));
            this.LblTitleList.Image = ((System.Drawing.Image)(resources.GetObject("LblTitleList.Image")));
            this.LblTitleList.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitleList.ImageAlign")));
            this.LblTitleList.ImageIndex = ((int)(resources.GetObject("LblTitleList.ImageIndex")));
            this.LblTitleList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblTitleList.ImeMode")));
            this.LblTitleList.Location = ((System.Drawing.Point)(resources.GetObject("LblTitleList.Location")));
            this.LblTitleList.Name = "LblTitleList";
            this.LblTitleList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblTitleList.RightToLeft")));
            this.LblTitleList.Size = ((System.Drawing.Size)(resources.GetObject("LblTitleList.Size")));
            this.LblTitleList.TabIndex = ((int)(resources.GetObject("LblTitleList.TabIndex")));
            this.LblTitleList.Text = resources.GetString("LblTitleList.Text");
            this.LblTitleList.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitleList.TextAlign")));
            this.LblTitleList.Visible = ((bool)(resources.GetObject("LblTitleList.Visible")));
            // 
            // AddLoginToCompany
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
            this.Controls.Add(this.LblTitleList);
            this.Controls.Add(this.TextLoginName);
            this.Controls.Add(this.TextLoginPassword);
            this.Controls.Add(this.LblLoginName);
            this.Controls.Add(this.LblLoginPassword);
            this.Controls.Add(this.ListViewLogins);
            this.Controls.Add(this.LblExplication);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.BtnSave);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "AddLoginToCompany";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.ShowInTaskbar = false;
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Closing += new System.ComponentModel.CancelEventHandler(this.AddLoginToCompany_Closing);
            this.VisibleChanged += new System.EventHandler(this.AddLoginToCompany_VisibleChanged);
            this.Deactivate += new System.EventHandler(this.AddLoginToCompany_Deactivate);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
