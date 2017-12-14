using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class JoinRoleUser
    {
        private ListView listViewUsersCompany;
        private GroupBox groupBox1;
        private RadioButton rbRolesUser;
        private RadioButton rbAllRoles;
        private Label LblInfoColoredUsers;
        private Label LabelTitle;
        private Label LblExplication;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(JoinRoleUser));
            this.listViewUsersCompany = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbRolesUser = new System.Windows.Forms.RadioButton();
            this.rbAllRoles = new System.Windows.Forms.RadioButton();
            this.LblInfoColoredUsers = new System.Windows.Forms.Label();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LblExplication = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewUsersCompany
            // 
            this.listViewUsersCompany.AccessibleDescription = resources.GetString("listViewUsersCompany.AccessibleDescription");
            this.listViewUsersCompany.AccessibleName = resources.GetString("listViewUsersCompany.AccessibleName");
            this.listViewUsersCompany.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("listViewUsersCompany.Alignment")));
            this.listViewUsersCompany.AllowDrop = true;
            this.listViewUsersCompany.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listViewUsersCompany.Anchor")));
            this.listViewUsersCompany.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listViewUsersCompany.BackgroundImage")));
            this.listViewUsersCompany.CheckBoxes = true;
            this.listViewUsersCompany.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listViewUsersCompany.Dock")));
            this.listViewUsersCompany.Enabled = ((bool)(resources.GetObject("listViewUsersCompany.Enabled")));
            this.listViewUsersCompany.Font = ((System.Drawing.Font)(resources.GetObject("listViewUsersCompany.Font")));
            this.listViewUsersCompany.FullRowSelect = true;
            this.listViewUsersCompany.GridLines = true;
            this.listViewUsersCompany.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewUsersCompany.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listViewUsersCompany.ImeMode")));
            this.listViewUsersCompany.LabelWrap = ((bool)(resources.GetObject("listViewUsersCompany.LabelWrap")));
            this.listViewUsersCompany.Location = ((System.Drawing.Point)(resources.GetObject("listViewUsersCompany.Location")));
            this.listViewUsersCompany.MultiSelect = false;
            this.listViewUsersCompany.Name = "listViewUsersCompany";
            this.listViewUsersCompany.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listViewUsersCompany.RightToLeft")));
            this.listViewUsersCompany.Size = ((System.Drawing.Size)(resources.GetObject("listViewUsersCompany.Size")));
            this.listViewUsersCompany.TabIndex = ((int)(resources.GetObject("listViewUsersCompany.TabIndex")));
            this.listViewUsersCompany.Text = resources.GetString("listViewUsersCompany.Text");
            this.listViewUsersCompany.View = System.Windows.Forms.View.Details;
            this.listViewUsersCompany.Visible = ((bool)(resources.GetObject("listViewUsersCompany.Visible")));
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
            this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
            this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
            this.groupBox1.Controls.Add(this.rbRolesUser);
            this.groupBox1.Controls.Add(this.rbAllRoles);
            this.groupBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox1.Dock")));
            this.groupBox1.Enabled = ((bool)(resources.GetObject("groupBox1.Enabled")));
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Font = ((System.Drawing.Font)(resources.GetObject("groupBox1.Font")));
            this.groupBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox1.ImeMode")));
            this.groupBox1.Location = ((System.Drawing.Point)(resources.GetObject("groupBox1.Location")));
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox1.RightToLeft")));
            this.groupBox1.Size = ((System.Drawing.Size)(resources.GetObject("groupBox1.Size")));
            this.groupBox1.TabIndex = ((int)(resources.GetObject("groupBox1.TabIndex")));
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = resources.GetString("groupBox1.Text");
            this.groupBox1.Visible = ((bool)(resources.GetObject("groupBox1.Visible")));
            // 
            // rbRolesUser
            // 
            this.rbRolesUser.AccessibleDescription = resources.GetString("rbRolesUser.AccessibleDescription");
            this.rbRolesUser.AccessibleName = resources.GetString("rbRolesUser.AccessibleName");
            this.rbRolesUser.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rbRolesUser.Anchor")));
            this.rbRolesUser.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("rbRolesUser.Appearance")));
            this.rbRolesUser.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rbRolesUser.BackgroundImage")));
            this.rbRolesUser.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbRolesUser.CheckAlign")));
            this.rbRolesUser.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rbRolesUser.Dock")));
            this.rbRolesUser.Enabled = ((bool)(resources.GetObject("rbRolesUser.Enabled")));
            this.rbRolesUser.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("rbRolesUser.FlatStyle")));
            this.rbRolesUser.Font = ((System.Drawing.Font)(resources.GetObject("rbRolesUser.Font")));
            this.rbRolesUser.Image = ((System.Drawing.Image)(resources.GetObject("rbRolesUser.Image")));
            this.rbRolesUser.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbRolesUser.ImageAlign")));
            this.rbRolesUser.ImageIndex = ((int)(resources.GetObject("rbRolesUser.ImageIndex")));
            this.rbRolesUser.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rbRolesUser.ImeMode")));
            this.rbRolesUser.Location = ((System.Drawing.Point)(resources.GetObject("rbRolesUser.Location")));
            this.rbRolesUser.Name = "rbRolesUser";
            this.rbRolesUser.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rbRolesUser.RightToLeft")));
            this.rbRolesUser.Size = ((System.Drawing.Size)(resources.GetObject("rbRolesUser.Size")));
            this.rbRolesUser.TabIndex = ((int)(resources.GetObject("rbRolesUser.TabIndex")));
            this.rbRolesUser.TabStop = true;
            this.rbRolesUser.Text = resources.GetString("rbRolesUser.Text");
            this.rbRolesUser.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbRolesUser.TextAlign")));
            this.rbRolesUser.Visible = ((bool)(resources.GetObject("rbRolesUser.Visible")));
            this.rbRolesUser.Click += new System.EventHandler(this.rbRolesUser_Click);
            // 
            // rbAllRoles
            // 
            this.rbAllRoles.AccessibleDescription = resources.GetString("rbAllRoles.AccessibleDescription");
            this.rbAllRoles.AccessibleName = resources.GetString("rbAllRoles.AccessibleName");
            this.rbAllRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rbAllRoles.Anchor")));
            this.rbAllRoles.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("rbAllRoles.Appearance")));
            this.rbAllRoles.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rbAllRoles.BackgroundImage")));
            this.rbAllRoles.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbAllRoles.CheckAlign")));
            this.rbAllRoles.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rbAllRoles.Dock")));
            this.rbAllRoles.Enabled = ((bool)(resources.GetObject("rbAllRoles.Enabled")));
            this.rbAllRoles.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("rbAllRoles.FlatStyle")));
            this.rbAllRoles.Font = ((System.Drawing.Font)(resources.GetObject("rbAllRoles.Font")));
            this.rbAllRoles.Image = ((System.Drawing.Image)(resources.GetObject("rbAllRoles.Image")));
            this.rbAllRoles.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbAllRoles.ImageAlign")));
            this.rbAllRoles.ImageIndex = ((int)(resources.GetObject("rbAllRoles.ImageIndex")));
            this.rbAllRoles.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rbAllRoles.ImeMode")));
            this.rbAllRoles.Location = ((System.Drawing.Point)(resources.GetObject("rbAllRoles.Location")));
            this.rbAllRoles.Name = "rbAllRoles";
            this.rbAllRoles.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rbAllRoles.RightToLeft")));
            this.rbAllRoles.Size = ((System.Drawing.Size)(resources.GetObject("rbAllRoles.Size")));
            this.rbAllRoles.TabIndex = ((int)(resources.GetObject("rbAllRoles.TabIndex")));
            this.rbAllRoles.TabStop = true;
            this.rbAllRoles.Text = resources.GetString("rbAllRoles.Text");
            this.rbAllRoles.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbAllRoles.TextAlign")));
            this.rbAllRoles.Visible = ((bool)(resources.GetObject("rbAllRoles.Visible")));
            this.rbAllRoles.Click += new System.EventHandler(this.rbAllRoles_Click);
            // 
            // LblInfoColoredUsers
            // 
            this.LblInfoColoredUsers.AccessibleDescription = resources.GetString("LblInfoColoredUsers.AccessibleDescription");
            this.LblInfoColoredUsers.AccessibleName = resources.GetString("LblInfoColoredUsers.AccessibleName");
            this.LblInfoColoredUsers.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblInfoColoredUsers.Anchor")));
            this.LblInfoColoredUsers.AutoSize = ((bool)(resources.GetObject("LblInfoColoredUsers.AutoSize")));
            this.LblInfoColoredUsers.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblInfoColoredUsers.Dock")));
            this.LblInfoColoredUsers.Enabled = ((bool)(resources.GetObject("LblInfoColoredUsers.Enabled")));
            this.LblInfoColoredUsers.Font = ((System.Drawing.Font)(resources.GetObject("LblInfoColoredUsers.Font")));
            this.LblInfoColoredUsers.Image = ((System.Drawing.Image)(resources.GetObject("LblInfoColoredUsers.Image")));
            this.LblInfoColoredUsers.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblInfoColoredUsers.ImageAlign")));
            this.LblInfoColoredUsers.ImageIndex = ((int)(resources.GetObject("LblInfoColoredUsers.ImageIndex")));
            this.LblInfoColoredUsers.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblInfoColoredUsers.ImeMode")));
            this.LblInfoColoredUsers.Location = ((System.Drawing.Point)(resources.GetObject("LblInfoColoredUsers.Location")));
            this.LblInfoColoredUsers.Name = "LblInfoColoredUsers";
            this.LblInfoColoredUsers.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblInfoColoredUsers.RightToLeft")));
            this.LblInfoColoredUsers.Size = ((System.Drawing.Size)(resources.GetObject("LblInfoColoredUsers.Size")));
            this.LblInfoColoredUsers.TabIndex = ((int)(resources.GetObject("LblInfoColoredUsers.TabIndex")));
            this.LblInfoColoredUsers.Text = resources.GetString("LblInfoColoredUsers.Text");
            this.LblInfoColoredUsers.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblInfoColoredUsers.TextAlign")));
            this.LblInfoColoredUsers.Visible = ((bool)(resources.GetObject("LblInfoColoredUsers.Visible")));
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
            // JoinRoleUser
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.Controls.Add(this.LblExplication);
            this.Controls.Add(this.LabelTitle);
            this.Controls.Add(this.LblInfoColoredUsers);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listViewUsersCompany);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "JoinRoleUser";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Closing += new System.ComponentModel.CancelEventHandler(this.JoinRoleUser_Closing);
            this.VisibleChanged += new System.EventHandler(this.JoinRoleUser_VisibleChanged);
            this.Deactivate += new System.EventHandler(this.JoinRoleUser_Deactivate);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
