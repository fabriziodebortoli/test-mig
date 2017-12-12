using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
    partial class Role
    {
        private Label lblRole;
        private Label lblDescrizione;
        private Label LabelTitle;
        private Label LblExplication;
        private TextBox tbRole;
        private TextBox tbRoleId;
        private TextBox tbDescrizione;
        private TextBox tbCompanyId;
        private CheckBox CbDisableRole;
        private System.Windows.Forms.ToolTip toolTip;
        private System.ComponentModel.IContainer components;

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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Role));
            this.lblRole = new System.Windows.Forms.Label();
            this.tbRole = new System.Windows.Forms.TextBox();
            this.tbDescrizione = new System.Windows.Forms.TextBox();
            this.lblDescrizione = new System.Windows.Forms.Label();
            this.tbRoleId = new System.Windows.Forms.TextBox();
            this.tbCompanyId = new System.Windows.Forms.TextBox();
            this.CbDisableRole = new System.Windows.Forms.CheckBox();
            this.LabelTitle = new System.Windows.Forms.Label();
            this.LblExplication = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // lblRole
            // 
            this.lblRole.AccessibleDescription = resources.GetString("lblRole.AccessibleDescription");
            this.lblRole.AccessibleName = resources.GetString("lblRole.AccessibleName");
            this.lblRole.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblRole.Anchor")));
            this.lblRole.AutoSize = ((bool)(resources.GetObject("lblRole.AutoSize")));
            this.lblRole.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblRole.Dock")));
            this.lblRole.Enabled = ((bool)(resources.GetObject("lblRole.Enabled")));
            this.lblRole.Font = ((System.Drawing.Font)(resources.GetObject("lblRole.Font")));
            this.lblRole.Image = ((System.Drawing.Image)(resources.GetObject("lblRole.Image")));
            this.lblRole.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRole.ImageAlign")));
            this.lblRole.ImageIndex = ((int)(resources.GetObject("lblRole.ImageIndex")));
            this.lblRole.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblRole.ImeMode")));
            this.lblRole.Location = ((System.Drawing.Point)(resources.GetObject("lblRole.Location")));
            this.lblRole.Name = "lblRole";
            this.lblRole.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblRole.RightToLeft")));
            this.lblRole.Size = ((System.Drawing.Size)(resources.GetObject("lblRole.Size")));
            this.lblRole.TabIndex = ((int)(resources.GetObject("lblRole.TabIndex")));
            this.lblRole.Text = resources.GetString("lblRole.Text");
            this.lblRole.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRole.TextAlign")));
            this.toolTip.SetToolTip(this.lblRole, resources.GetString("lblRole.ToolTip"));
            this.lblRole.Visible = ((bool)(resources.GetObject("lblRole.Visible")));
            // 
            // tbRole
            // 
            this.tbRole.AcceptsReturn = true;
            this.tbRole.AcceptsTab = true;
            this.tbRole.AccessibleDescription = resources.GetString("tbRole.AccessibleDescription");
            this.tbRole.AccessibleName = resources.GetString("tbRole.AccessibleName");
            this.tbRole.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tbRole.Anchor")));
            this.tbRole.AutoSize = ((bool)(resources.GetObject("tbRole.AutoSize")));
            this.tbRole.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tbRole.BackgroundImage")));
            this.tbRole.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tbRole.Dock")));
            this.tbRole.Enabled = ((bool)(resources.GetObject("tbRole.Enabled")));
            this.tbRole.Font = ((System.Drawing.Font)(resources.GetObject("tbRole.Font")));
            this.tbRole.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tbRole.ImeMode")));
            this.tbRole.Location = ((System.Drawing.Point)(resources.GetObject("tbRole.Location")));
            this.tbRole.MaxLength = ((int)(resources.GetObject("tbRole.MaxLength")));
            this.tbRole.Multiline = ((bool)(resources.GetObject("tbRole.Multiline")));
            this.tbRole.Name = "tbRole";
            this.tbRole.PasswordChar = ((char)(resources.GetObject("tbRole.PasswordChar")));
            this.tbRole.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tbRole.RightToLeft")));
            this.tbRole.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tbRole.ScrollBars")));
            this.tbRole.Size = ((System.Drawing.Size)(resources.GetObject("tbRole.Size")));
            this.tbRole.TabIndex = ((int)(resources.GetObject("tbRole.TabIndex")));
            this.tbRole.Text = resources.GetString("tbRole.Text");
            this.tbRole.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tbRole.TextAlign")));
            this.toolTip.SetToolTip(this.tbRole, resources.GetString("tbRole.ToolTip"));
            this.tbRole.Visible = ((bool)(resources.GetObject("tbRole.Visible")));
            this.tbRole.WordWrap = ((bool)(resources.GetObject("tbRole.WordWrap")));
            this.tbRole.TextChanged += new System.EventHandler(this.tbRole_TextChanged);
            // 
            // tbDescrizione
            // 
            this.tbDescrizione.AcceptsReturn = true;
            this.tbDescrizione.AcceptsTab = true;
            this.tbDescrizione.AccessibleDescription = resources.GetString("tbDescrizione.AccessibleDescription");
            this.tbDescrizione.AccessibleName = resources.GetString("tbDescrizione.AccessibleName");
            this.tbDescrizione.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tbDescrizione.Anchor")));
            this.tbDescrizione.AutoSize = ((bool)(resources.GetObject("tbDescrizione.AutoSize")));
            this.tbDescrizione.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tbDescrizione.BackgroundImage")));
            this.tbDescrizione.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tbDescrizione.Dock")));
            this.tbDescrizione.Enabled = ((bool)(resources.GetObject("tbDescrizione.Enabled")));
            this.tbDescrizione.Font = ((System.Drawing.Font)(resources.GetObject("tbDescrizione.Font")));
            this.tbDescrizione.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tbDescrizione.ImeMode")));
            this.tbDescrizione.Location = ((System.Drawing.Point)(resources.GetObject("tbDescrizione.Location")));
            this.tbDescrizione.MaxLength = ((int)(resources.GetObject("tbDescrizione.MaxLength")));
            this.tbDescrizione.Multiline = ((bool)(resources.GetObject("tbDescrizione.Multiline")));
            this.tbDescrizione.Name = "tbDescrizione";
            this.tbDescrizione.PasswordChar = ((char)(resources.GetObject("tbDescrizione.PasswordChar")));
            this.tbDescrizione.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tbDescrizione.RightToLeft")));
            this.tbDescrizione.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tbDescrizione.ScrollBars")));
            this.tbDescrizione.Size = ((System.Drawing.Size)(resources.GetObject("tbDescrizione.Size")));
            this.tbDescrizione.TabIndex = ((int)(resources.GetObject("tbDescrizione.TabIndex")));
            this.tbDescrizione.Text = resources.GetString("tbDescrizione.Text");
            this.tbDescrizione.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tbDescrizione.TextAlign")));
            this.toolTip.SetToolTip(this.tbDescrizione, resources.GetString("tbDescrizione.ToolTip"));
            this.tbDescrizione.Visible = ((bool)(resources.GetObject("tbDescrizione.Visible")));
            this.tbDescrizione.WordWrap = ((bool)(resources.GetObject("tbDescrizione.WordWrap")));
            this.tbDescrizione.TextChanged += new System.EventHandler(this.tbDescrizione_TextChanged);
            // 
            // lblDescrizione
            // 
            this.lblDescrizione.AccessibleDescription = resources.GetString("lblDescrizione.AccessibleDescription");
            this.lblDescrizione.AccessibleName = resources.GetString("lblDescrizione.AccessibleName");
            this.lblDescrizione.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblDescrizione.Anchor")));
            this.lblDescrizione.AutoSize = ((bool)(resources.GetObject("lblDescrizione.AutoSize")));
            this.lblDescrizione.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblDescrizione.Dock")));
            this.lblDescrizione.Enabled = ((bool)(resources.GetObject("lblDescrizione.Enabled")));
            this.lblDescrizione.Font = ((System.Drawing.Font)(resources.GetObject("lblDescrizione.Font")));
            this.lblDescrizione.Image = ((System.Drawing.Image)(resources.GetObject("lblDescrizione.Image")));
            this.lblDescrizione.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescrizione.ImageAlign")));
            this.lblDescrizione.ImageIndex = ((int)(resources.GetObject("lblDescrizione.ImageIndex")));
            this.lblDescrizione.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblDescrizione.ImeMode")));
            this.lblDescrizione.Location = ((System.Drawing.Point)(resources.GetObject("lblDescrizione.Location")));
            this.lblDescrizione.Name = "lblDescrizione";
            this.lblDescrizione.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblDescrizione.RightToLeft")));
            this.lblDescrizione.Size = ((System.Drawing.Size)(resources.GetObject("lblDescrizione.Size")));
            this.lblDescrizione.TabIndex = ((int)(resources.GetObject("lblDescrizione.TabIndex")));
            this.lblDescrizione.Text = resources.GetString("lblDescrizione.Text");
            this.lblDescrizione.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblDescrizione.TextAlign")));
            this.toolTip.SetToolTip(this.lblDescrizione, resources.GetString("lblDescrizione.ToolTip"));
            this.lblDescrizione.Visible = ((bool)(resources.GetObject("lblDescrizione.Visible")));
            // 
            // tbRoleId
            // 
            this.tbRoleId.AccessibleDescription = resources.GetString("tbRoleId.AccessibleDescription");
            this.tbRoleId.AccessibleName = resources.GetString("tbRoleId.AccessibleName");
            this.tbRoleId.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tbRoleId.Anchor")));
            this.tbRoleId.AutoSize = ((bool)(resources.GetObject("tbRoleId.AutoSize")));
            this.tbRoleId.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tbRoleId.BackgroundImage")));
            this.tbRoleId.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tbRoleId.Dock")));
            this.tbRoleId.Enabled = ((bool)(resources.GetObject("tbRoleId.Enabled")));
            this.tbRoleId.Font = ((System.Drawing.Font)(resources.GetObject("tbRoleId.Font")));
            this.tbRoleId.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tbRoleId.ImeMode")));
            this.tbRoleId.Location = ((System.Drawing.Point)(resources.GetObject("tbRoleId.Location")));
            this.tbRoleId.MaxLength = ((int)(resources.GetObject("tbRoleId.MaxLength")));
            this.tbRoleId.Multiline = ((bool)(resources.GetObject("tbRoleId.Multiline")));
            this.tbRoleId.Name = "tbRoleId";
            this.tbRoleId.PasswordChar = ((char)(resources.GetObject("tbRoleId.PasswordChar")));
            this.tbRoleId.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tbRoleId.RightToLeft")));
            this.tbRoleId.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tbRoleId.ScrollBars")));
            this.tbRoleId.Size = ((System.Drawing.Size)(resources.GetObject("tbRoleId.Size")));
            this.tbRoleId.TabIndex = ((int)(resources.GetObject("tbRoleId.TabIndex")));
            this.tbRoleId.TabStop = false;
            this.tbRoleId.Text = resources.GetString("tbRoleId.Text");
            this.tbRoleId.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tbRoleId.TextAlign")));
            this.toolTip.SetToolTip(this.tbRoleId, resources.GetString("tbRoleId.ToolTip"));
            this.tbRoleId.Visible = ((bool)(resources.GetObject("tbRoleId.Visible")));
            this.tbRoleId.WordWrap = ((bool)(resources.GetObject("tbRoleId.WordWrap")));
            // 
            // tbCompanyId
            // 
            this.tbCompanyId.AccessibleDescription = resources.GetString("tbCompanyId.AccessibleDescription");
            this.tbCompanyId.AccessibleName = resources.GetString("tbCompanyId.AccessibleName");
            this.tbCompanyId.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tbCompanyId.Anchor")));
            this.tbCompanyId.AutoSize = ((bool)(resources.GetObject("tbCompanyId.AutoSize")));
            this.tbCompanyId.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tbCompanyId.BackgroundImage")));
            this.tbCompanyId.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tbCompanyId.Dock")));
            this.tbCompanyId.Enabled = ((bool)(resources.GetObject("tbCompanyId.Enabled")));
            this.tbCompanyId.Font = ((System.Drawing.Font)(resources.GetObject("tbCompanyId.Font")));
            this.tbCompanyId.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tbCompanyId.ImeMode")));
            this.tbCompanyId.Location = ((System.Drawing.Point)(resources.GetObject("tbCompanyId.Location")));
            this.tbCompanyId.MaxLength = ((int)(resources.GetObject("tbCompanyId.MaxLength")));
            this.tbCompanyId.Multiline = ((bool)(resources.GetObject("tbCompanyId.Multiline")));
            this.tbCompanyId.Name = "tbCompanyId";
            this.tbCompanyId.PasswordChar = ((char)(resources.GetObject("tbCompanyId.PasswordChar")));
            this.tbCompanyId.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tbCompanyId.RightToLeft")));
            this.tbCompanyId.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tbCompanyId.ScrollBars")));
            this.tbCompanyId.Size = ((System.Drawing.Size)(resources.GetObject("tbCompanyId.Size")));
            this.tbCompanyId.TabIndex = ((int)(resources.GetObject("tbCompanyId.TabIndex")));
            this.tbCompanyId.TabStop = false;
            this.tbCompanyId.Text = resources.GetString("tbCompanyId.Text");
            this.tbCompanyId.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tbCompanyId.TextAlign")));
            this.toolTip.SetToolTip(this.tbCompanyId, resources.GetString("tbCompanyId.ToolTip"));
            this.tbCompanyId.Visible = ((bool)(resources.GetObject("tbCompanyId.Visible")));
            this.tbCompanyId.WordWrap = ((bool)(resources.GetObject("tbCompanyId.WordWrap")));
            // 
            // CbDisableRole
            // 
            this.CbDisableRole.AccessibleDescription = resources.GetString("CbDisableRole.AccessibleDescription");
            this.CbDisableRole.AccessibleName = resources.GetString("CbDisableRole.AccessibleName");
            this.CbDisableRole.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CbDisableRole.Anchor")));
            this.CbDisableRole.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CbDisableRole.Appearance")));
            this.CbDisableRole.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CbDisableRole.BackgroundImage")));
            this.CbDisableRole.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CbDisableRole.CheckAlign")));
            this.CbDisableRole.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CbDisableRole.Dock")));
            this.CbDisableRole.Enabled = ((bool)(resources.GetObject("CbDisableRole.Enabled")));
            this.CbDisableRole.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CbDisableRole.FlatStyle")));
            this.CbDisableRole.Font = ((System.Drawing.Font)(resources.GetObject("CbDisableRole.Font")));
            this.CbDisableRole.Image = ((System.Drawing.Image)(resources.GetObject("CbDisableRole.Image")));
            this.CbDisableRole.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CbDisableRole.ImageAlign")));
            this.CbDisableRole.ImageIndex = ((int)(resources.GetObject("CbDisableRole.ImageIndex")));
            this.CbDisableRole.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CbDisableRole.ImeMode")));
            this.CbDisableRole.Location = ((System.Drawing.Point)(resources.GetObject("CbDisableRole.Location")));
            this.CbDisableRole.Name = "CbDisableRole";
            this.CbDisableRole.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CbDisableRole.RightToLeft")));
            this.CbDisableRole.Size = ((System.Drawing.Size)(resources.GetObject("CbDisableRole.Size")));
            this.CbDisableRole.TabIndex = ((int)(resources.GetObject("CbDisableRole.TabIndex")));
            this.CbDisableRole.Text = resources.GetString("CbDisableRole.Text");
            this.CbDisableRole.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CbDisableRole.TextAlign")));
            this.toolTip.SetToolTip(this.CbDisableRole, resources.GetString("CbDisableRole.ToolTip"));
            this.CbDisableRole.Visible = ((bool)(resources.GetObject("CbDisableRole.Visible")));
            this.CbDisableRole.CheckedChanged += new System.EventHandler(this.CbDisableRole_CheckedChanged);
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
            this.toolTip.SetToolTip(this.LabelTitle, resources.GetString("LabelTitle.ToolTip"));
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
            this.toolTip.SetToolTip(this.LblExplication, resources.GetString("LblExplication.ToolTip"));
            this.LblExplication.Visible = ((bool)(resources.GetObject("LblExplication.Visible")));
            // 
            // toolTip
            // 
            this.toolTip.ShowAlways = true;
            // 
            // Role
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
            this.Controls.Add(this.CbDisableRole);
            this.Controls.Add(this.tbCompanyId);
            this.Controls.Add(this.tbRoleId);
            this.Controls.Add(this.tbDescrizione);
            this.Controls.Add(this.tbRole);
            this.Controls.Add(this.lblDescrizione);
            this.Controls.Add(this.lblRole);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
            this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.Name = "Role";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
            this.Text = resources.GetString("$this.Text");
            this.toolTip.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Role_Closing);
            this.Load += new System.EventHandler(this.Role_Load);
            this.VisibleChanged += new System.EventHandler(this.Role_VisibleChanged);
            this.Deactivate += new System.EventHandler(this.Role_Deactivate);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
