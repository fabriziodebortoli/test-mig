using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Lists
{
    partial class UsersBrowserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private Label lblList;
        private ListView listUsers;
        private Button btnAdd;
        private Label lblAddUser;
        private TextBox tbSelectedUser;
        private ComboBox cbListDomains;

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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(UsersBrowserControl));
            this.lblList = new System.Windows.Forms.Label();
            this.listUsers = new System.Windows.Forms.ListView();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lblAddUser = new System.Windows.Forms.Label();
            this.tbSelectedUser = new System.Windows.Forms.TextBox();
            this.cbListDomains = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lblList
            // 
            this.lblList.AccessibleDescription = resources.GetString("lblList.AccessibleDescription");
            this.lblList.AccessibleName = resources.GetString("lblList.AccessibleName");
            this.lblList.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblList.Anchor")));
            this.lblList.AutoSize = ((bool)(resources.GetObject("lblList.AutoSize")));
            this.lblList.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblList.Dock")));
            this.lblList.Enabled = ((bool)(resources.GetObject("lblList.Enabled")));
            this.lblList.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblList.Font = ((System.Drawing.Font)(resources.GetObject("lblList.Font")));
            this.lblList.Image = ((System.Drawing.Image)(resources.GetObject("lblList.Image")));
            this.lblList.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblList.ImageAlign")));
            this.lblList.ImageIndex = ((int)(resources.GetObject("lblList.ImageIndex")));
            this.lblList.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblList.ImeMode")));
            this.lblList.Location = ((System.Drawing.Point)(resources.GetObject("lblList.Location")));
            this.lblList.Name = "lblList";
            this.lblList.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblList.RightToLeft")));
            this.lblList.Size = ((System.Drawing.Size)(resources.GetObject("lblList.Size")));
            this.lblList.TabIndex = ((int)(resources.GetObject("lblList.TabIndex")));
            this.lblList.Text = resources.GetString("lblList.Text");
            this.lblList.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblList.TextAlign")));
            this.lblList.Visible = ((bool)(resources.GetObject("lblList.Visible")));
            // 
            // listUsers
            // 
            this.listUsers.AccessibleDescription = resources.GetString("listUsers.AccessibleDescription");
            this.listUsers.AccessibleName = resources.GetString("listUsers.AccessibleName");
            this.listUsers.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("listUsers.Alignment")));
            this.listUsers.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listUsers.Anchor")));
            this.listUsers.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listUsers.BackgroundImage")));
            this.listUsers.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listUsers.Dock")));
            this.listUsers.Enabled = ((bool)(resources.GetObject("listUsers.Enabled")));
            this.listUsers.Font = ((System.Drawing.Font)(resources.GetObject("listUsers.Font")));
            this.listUsers.FullRowSelect = true;
            this.listUsers.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listUsers.ImeMode")));
            this.listUsers.LabelWrap = ((bool)(resources.GetObject("listUsers.LabelWrap")));
            this.listUsers.Location = ((System.Drawing.Point)(resources.GetObject("listUsers.Location")));
            this.listUsers.Name = "listUsers";
            this.listUsers.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listUsers.RightToLeft")));
            this.listUsers.Size = ((System.Drawing.Size)(resources.GetObject("listUsers.Size")));
            this.listUsers.TabIndex = ((int)(resources.GetObject("listUsers.TabIndex")));
            this.listUsers.Text = resources.GetString("listUsers.Text");
            this.listUsers.View = System.Windows.Forms.View.Details;
            this.listUsers.Visible = ((bool)(resources.GetObject("listUsers.Visible")));
            this.listUsers.DoubleClick += new System.EventHandler(this.listUsers_DoubleClick);
            // 
            // btnAdd
            // 
            this.btnAdd.AccessibleDescription = resources.GetString("btnAdd.AccessibleDescription");
            this.btnAdd.AccessibleName = resources.GetString("btnAdd.AccessibleName");
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnAdd.Anchor")));
            this.btnAdd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAdd.BackgroundImage")));
            this.btnAdd.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnAdd.Dock")));
            this.btnAdd.Enabled = ((bool)(resources.GetObject("btnAdd.Enabled")));
            this.btnAdd.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnAdd.FlatStyle")));
            this.btnAdd.Font = ((System.Drawing.Font)(resources.GetObject("btnAdd.Font")));
            this.btnAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnAdd.Image")));
            this.btnAdd.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnAdd.ImageAlign")));
            this.btnAdd.ImageIndex = ((int)(resources.GetObject("btnAdd.ImageIndex")));
            this.btnAdd.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnAdd.ImeMode")));
            this.btnAdd.Location = ((System.Drawing.Point)(resources.GetObject("btnAdd.Location")));
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnAdd.RightToLeft")));
            this.btnAdd.Size = ((System.Drawing.Size)(resources.GetObject("btnAdd.Size")));
            this.btnAdd.TabIndex = ((int)(resources.GetObject("btnAdd.TabIndex")));
            this.btnAdd.Text = resources.GetString("btnAdd.Text");
            this.btnAdd.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnAdd.TextAlign")));
            this.btnAdd.Visible = ((bool)(resources.GetObject("btnAdd.Visible")));
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // lblAddUser
            // 
            this.lblAddUser.AccessibleDescription = resources.GetString("lblAddUser.AccessibleDescription");
            this.lblAddUser.AccessibleName = resources.GetString("lblAddUser.AccessibleName");
            this.lblAddUser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblAddUser.Anchor")));
            this.lblAddUser.AutoSize = ((bool)(resources.GetObject("lblAddUser.AutoSize")));
            this.lblAddUser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblAddUser.Dock")));
            this.lblAddUser.Enabled = ((bool)(resources.GetObject("lblAddUser.Enabled")));
            this.lblAddUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblAddUser.Font = ((System.Drawing.Font)(resources.GetObject("lblAddUser.Font")));
            this.lblAddUser.Image = ((System.Drawing.Image)(resources.GetObject("lblAddUser.Image")));
            this.lblAddUser.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblAddUser.ImageAlign")));
            this.lblAddUser.ImageIndex = ((int)(resources.GetObject("lblAddUser.ImageIndex")));
            this.lblAddUser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblAddUser.ImeMode")));
            this.lblAddUser.Location = ((System.Drawing.Point)(resources.GetObject("lblAddUser.Location")));
            this.lblAddUser.Name = "lblAddUser";
            this.lblAddUser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblAddUser.RightToLeft")));
            this.lblAddUser.Size = ((System.Drawing.Size)(resources.GetObject("lblAddUser.Size")));
            this.lblAddUser.TabIndex = ((int)(resources.GetObject("lblAddUser.TabIndex")));
            this.lblAddUser.Text = resources.GetString("lblAddUser.Text");
            this.lblAddUser.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblAddUser.TextAlign")));
            this.lblAddUser.Visible = ((bool)(resources.GetObject("lblAddUser.Visible")));
            // 
            // tbSelectedUser
            // 
            this.tbSelectedUser.AcceptsReturn = true;
            this.tbSelectedUser.AccessibleDescription = resources.GetString("tbSelectedUser.AccessibleDescription");
            this.tbSelectedUser.AccessibleName = resources.GetString("tbSelectedUser.AccessibleName");
            this.tbSelectedUser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tbSelectedUser.Anchor")));
            this.tbSelectedUser.AutoSize = ((bool)(resources.GetObject("tbSelectedUser.AutoSize")));
            this.tbSelectedUser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tbSelectedUser.BackgroundImage")));
            this.tbSelectedUser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tbSelectedUser.Dock")));
            this.tbSelectedUser.Enabled = ((bool)(resources.GetObject("tbSelectedUser.Enabled")));
            this.tbSelectedUser.Font = ((System.Drawing.Font)(resources.GetObject("tbSelectedUser.Font")));
            this.tbSelectedUser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tbSelectedUser.ImeMode")));
            this.tbSelectedUser.Location = ((System.Drawing.Point)(resources.GetObject("tbSelectedUser.Location")));
            this.tbSelectedUser.MaxLength = ((int)(resources.GetObject("tbSelectedUser.MaxLength")));
            this.tbSelectedUser.Multiline = ((bool)(resources.GetObject("tbSelectedUser.Multiline")));
            this.tbSelectedUser.Name = "tbSelectedUser";
            this.tbSelectedUser.PasswordChar = ((char)(resources.GetObject("tbSelectedUser.PasswordChar")));
            this.tbSelectedUser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tbSelectedUser.RightToLeft")));
            this.tbSelectedUser.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tbSelectedUser.ScrollBars")));
            this.tbSelectedUser.Size = ((System.Drawing.Size)(resources.GetObject("tbSelectedUser.Size")));
            this.tbSelectedUser.TabIndex = ((int)(resources.GetObject("tbSelectedUser.TabIndex")));
            this.tbSelectedUser.Text = resources.GetString("tbSelectedUser.Text");
            this.tbSelectedUser.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tbSelectedUser.TextAlign")));
            this.tbSelectedUser.Visible = ((bool)(resources.GetObject("tbSelectedUser.Visible")));
            this.tbSelectedUser.WordWrap = ((bool)(resources.GetObject("tbSelectedUser.WordWrap")));
            // 
            // cbListDomains
            // 
            this.cbListDomains.AccessibleDescription = resources.GetString("cbListDomains.AccessibleDescription");
            this.cbListDomains.AccessibleName = resources.GetString("cbListDomains.AccessibleName");
            this.cbListDomains.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cbListDomains.Anchor")));
            this.cbListDomains.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cbListDomains.BackgroundImage")));
            this.cbListDomains.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cbListDomains.Dock")));
            this.cbListDomains.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbListDomains.Enabled = ((bool)(resources.GetObject("cbListDomains.Enabled")));
            this.cbListDomains.Font = ((System.Drawing.Font)(resources.GetObject("cbListDomains.Font")));
            this.cbListDomains.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cbListDomains.ImeMode")));
            this.cbListDomains.IntegralHeight = ((bool)(resources.GetObject("cbListDomains.IntegralHeight")));
            this.cbListDomains.ItemHeight = ((int)(resources.GetObject("cbListDomains.ItemHeight")));
            this.cbListDomains.Location = ((System.Drawing.Point)(resources.GetObject("cbListDomains.Location")));
            this.cbListDomains.MaxDropDownItems = ((int)(resources.GetObject("cbListDomains.MaxDropDownItems")));
            this.cbListDomains.MaxLength = ((int)(resources.GetObject("cbListDomains.MaxLength")));
            this.cbListDomains.Name = "cbListDomains";
            this.cbListDomains.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cbListDomains.RightToLeft")));
            this.cbListDomains.Size = ((System.Drawing.Size)(resources.GetObject("cbListDomains.Size")));
            this.cbListDomains.TabIndex = ((int)(resources.GetObject("cbListDomains.TabIndex")));
            this.cbListDomains.Text = resources.GetString("cbListDomains.Text");
            this.cbListDomains.Visible = ((bool)(resources.GetObject("cbListDomains.Visible")));
            this.cbListDomains.SelectedIndexChanged += new System.EventHandler(this.cbListDomains_SelectedIndexChanged);
            // 
            // UsersBrowserControl
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.cbListDomains);
            this.Controls.Add(this.tbSelectedUser);
            this.Controls.Add(this.lblAddUser);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.listUsers);
            this.Controls.Add(this.lblList);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "UsersBrowserControl";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.Load += new System.EventHandler(this.UsersBrowserControl_Load);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
