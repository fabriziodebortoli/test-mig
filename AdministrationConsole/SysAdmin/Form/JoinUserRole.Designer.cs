using System.ComponentModel;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class JoinUserRole
    {
        private GroupBox groupBox1;
        private Label LabelTitle;
        private Label LblExplication;

        private RadioButton rbAllUsers;
        private RadioButton rbUsersRole;

        private ListView listViewUsersCompany;

        private IContainer components = null;

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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(JoinUserRole));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbUsersRole = new System.Windows.Forms.RadioButton();
            this.rbAllUsers = new System.Windows.Forms.RadioButton();
            this.listViewUsersCompany = new System.Windows.Forms.ListView();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LblExplication = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
            this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
            this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
            this.groupBox1.Controls.Add(this.rbUsersRole);
            this.groupBox1.Controls.Add(this.rbAllUsers);
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
            // rbUsersRole
            // 
            this.rbUsersRole.AccessibleDescription = resources.GetString("rbUsersRole.AccessibleDescription");
            this.rbUsersRole.AccessibleName = resources.GetString("rbUsersRole.AccessibleName");
            this.rbUsersRole.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rbUsersRole.Anchor")));
            this.rbUsersRole.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("rbUsersRole.Appearance")));
            this.rbUsersRole.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rbUsersRole.BackgroundImage")));
            this.rbUsersRole.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbUsersRole.CheckAlign")));
            this.rbUsersRole.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rbUsersRole.Dock")));
            this.rbUsersRole.Enabled = ((bool)(resources.GetObject("rbUsersRole.Enabled")));
            this.rbUsersRole.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("rbUsersRole.FlatStyle")));
            this.rbUsersRole.Font = ((System.Drawing.Font)(resources.GetObject("rbUsersRole.Font")));
            this.rbUsersRole.Image = ((System.Drawing.Image)(resources.GetObject("rbUsersRole.Image")));
            this.rbUsersRole.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbUsersRole.ImageAlign")));
            this.rbUsersRole.ImageIndex = ((int)(resources.GetObject("rbUsersRole.ImageIndex")));
            this.rbUsersRole.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rbUsersRole.ImeMode")));
            this.rbUsersRole.Location = ((System.Drawing.Point)(resources.GetObject("rbUsersRole.Location")));
            this.rbUsersRole.Name = "rbUsersRole";
            this.rbUsersRole.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rbUsersRole.RightToLeft")));
            this.rbUsersRole.Size = ((System.Drawing.Size)(resources.GetObject("rbUsersRole.Size")));
            this.rbUsersRole.TabIndex = ((int)(resources.GetObject("rbUsersRole.TabIndex")));
            this.rbUsersRole.TabStop = true;
            this.rbUsersRole.Text = resources.GetString("rbUsersRole.Text");
            this.rbUsersRole.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbUsersRole.TextAlign")));
            this.rbUsersRole.Visible = ((bool)(resources.GetObject("rbUsersRole.Visible")));
            this.rbUsersRole.Click += new System.EventHandler(this.rbUsersRole_Click);
            // 
            // rbAllUsers
            // 
            this.rbAllUsers.AccessibleDescription = resources.GetString("rbAllUsers.AccessibleDescription");
            this.rbAllUsers.AccessibleName = resources.GetString("rbAllUsers.AccessibleName");
            this.rbAllUsers.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rbAllUsers.Anchor")));
            this.rbAllUsers.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("rbAllUsers.Appearance")));
            this.rbAllUsers.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rbAllUsers.BackgroundImage")));
            this.rbAllUsers.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbAllUsers.CheckAlign")));
            this.rbAllUsers.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rbAllUsers.Dock")));
            this.rbAllUsers.Enabled = ((bool)(resources.GetObject("rbAllUsers.Enabled")));
            this.rbAllUsers.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("rbAllUsers.FlatStyle")));
            this.rbAllUsers.Font = ((System.Drawing.Font)(resources.GetObject("rbAllUsers.Font")));
            this.rbAllUsers.Image = ((System.Drawing.Image)(resources.GetObject("rbAllUsers.Image")));
            this.rbAllUsers.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbAllUsers.ImageAlign")));
            this.rbAllUsers.ImageIndex = ((int)(resources.GetObject("rbAllUsers.ImageIndex")));
            this.rbAllUsers.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rbAllUsers.ImeMode")));
            this.rbAllUsers.Location = ((System.Drawing.Point)(resources.GetObject("rbAllUsers.Location")));
            this.rbAllUsers.Name = "rbAllUsers";
            this.rbAllUsers.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rbAllUsers.RightToLeft")));
            this.rbAllUsers.Size = ((System.Drawing.Size)(resources.GetObject("rbAllUsers.Size")));
            this.rbAllUsers.TabIndex = ((int)(resources.GetObject("rbAllUsers.TabIndex")));
            this.rbAllUsers.TabStop = true;
            this.rbAllUsers.Text = resources.GetString("rbAllUsers.Text");
            this.rbAllUsers.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rbAllUsers.TextAlign")));
            this.rbAllUsers.Visible = ((bool)(resources.GetObject("rbAllUsers.Visible")));
            this.rbAllUsers.Click += new System.EventHandler(this.rbAllUsers_Click);
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
            this.listViewUsersCompany.HideSelection = false;
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
            this.listViewUsersCompany.ItemActivate += new System.EventHandler(this.listViewUsersCompany_ItemActivate);
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
            // JoinUserRole
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
            this.Controls.Add(this.listViewUsersCompany);
            this.Controls.Add(this.groupBox1);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "JoinUserRole";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.Closing += new System.ComponentModel.CancelEventHandler(this.JoinUserRole_Closing);
            this.VisibleChanged += new System.EventHandler(this.JoinUserRole_VisibleChanged);
            this.Deactivate += new System.EventHandler(this.JoinUserRole_Deactivate);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
