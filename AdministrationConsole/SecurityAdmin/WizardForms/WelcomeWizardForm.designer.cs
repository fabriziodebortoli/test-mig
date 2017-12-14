
namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
    partial class WelcomeWizardForm
    {
        private System.Windows.Forms.Label DescriptionLabel;
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.GroupBox ParametersGroupBox;
        private System.Windows.Forms.Label CompanyNameLabel;
        private System.Windows.Forms.Label UserOrRoleNameLabel;
        private System.Windows.Forms.Label UserOrRoleName;
        private System.Windows.Forms.Label ObjectNameLabel;
        private System.Windows.Forms.RichTextBox ObjectName;
        private System.Windows.Forms.Label WizardCompanyName;


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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WelcomeWizardForm));
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.ParametersGroupBox = new System.Windows.Forms.GroupBox();
            this.ObjectName = new System.Windows.Forms.RichTextBox();
            this.ObjectNameLabel = new System.Windows.Forms.Label();
            this.UserOrRoleName = new System.Windows.Forms.Label();
            this.UserOrRoleNameLabel = new System.Windows.Forms.Label();
            this.WizardCompanyName = new System.Windows.Forms.Label();
            this.CompanyNameLabel = new System.Windows.Forms.Label();
            this.ParametersGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_watermarkPicture
            // 
            this.m_watermarkPicture.Image = ((System.Drawing.Image)(resources.GetObject("m_watermarkPicture.Image")));
            this.m_watermarkPicture.Name = "m_watermarkPicture";
            // 
            // m_titleLabel
            // 
            this.m_titleLabel.Name = "m_titleLabel";
            this.m_titleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_titleLabel.Size")));
            this.m_titleLabel.Text = resources.GetString("m_titleLabel.Text");
            // 
            // DescriptionLabel
            // 
            this.DescriptionLabel.AccessibleDescription = resources.GetString("DescriptionLabel.AccessibleDescription");
            this.DescriptionLabel.AccessibleName = resources.GetString("DescriptionLabel.AccessibleName");
            this.DescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DescriptionLabel.Anchor")));
            this.DescriptionLabel.AutoSize = ((bool)(resources.GetObject("DescriptionLabel.AutoSize")));
            this.DescriptionLabel.BackColor = System.Drawing.Color.White;
            this.DescriptionLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DescriptionLabel.Dock")));
            this.DescriptionLabel.Enabled = ((bool)(resources.GetObject("DescriptionLabel.Enabled")));
            this.DescriptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DescriptionLabel.Font = ((System.Drawing.Font)(resources.GetObject("DescriptionLabel.Font")));
            this.DescriptionLabel.Image = ((System.Drawing.Image)(resources.GetObject("DescriptionLabel.Image")));
            this.DescriptionLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DescriptionLabel.ImageAlign")));
            this.DescriptionLabel.ImageIndex = ((int)(resources.GetObject("DescriptionLabel.ImageIndex")));
            this.DescriptionLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DescriptionLabel.ImeMode")));
            this.DescriptionLabel.Location = ((System.Drawing.Point)(resources.GetObject("DescriptionLabel.Location")));
            this.DescriptionLabel.Name = "DescriptionLabel";
            this.DescriptionLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DescriptionLabel.RightToLeft")));
            this.DescriptionLabel.Size = ((System.Drawing.Size)(resources.GetObject("DescriptionLabel.Size")));
            this.DescriptionLabel.TabIndex = ((int)(resources.GetObject("DescriptionLabel.TabIndex")));
            this.DescriptionLabel.Text = resources.GetString("DescriptionLabel.Text");
            this.DescriptionLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DescriptionLabel.TextAlign")));
            this.DescriptionLabel.Visible = ((bool)(resources.GetObject("DescriptionLabel.Visible")));
            // 
            // ParametersGroupBox
            // 
            this.ParametersGroupBox.AccessibleDescription = resources.GetString("ParametersGroupBox.AccessibleDescription");
            this.ParametersGroupBox.AccessibleName = resources.GetString("ParametersGroupBox.AccessibleName");
            this.ParametersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ParametersGroupBox.Anchor")));
            this.ParametersGroupBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ParametersGroupBox.BackgroundImage")));
            this.ParametersGroupBox.Controls.Add(this.ObjectName);
            this.ParametersGroupBox.Controls.Add(this.ObjectNameLabel);
            this.ParametersGroupBox.Controls.Add(this.UserOrRoleName);
            this.ParametersGroupBox.Controls.Add(this.UserOrRoleNameLabel);
            this.ParametersGroupBox.Controls.Add(this.WizardCompanyName);
            this.ParametersGroupBox.Controls.Add(this.CompanyNameLabel);
            this.ParametersGroupBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ParametersGroupBox.Dock")));
            this.ParametersGroupBox.Enabled = ((bool)(resources.GetObject("ParametersGroupBox.Enabled")));
            this.ParametersGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ParametersGroupBox.Font = ((System.Drawing.Font)(resources.GetObject("ParametersGroupBox.Font")));
            this.ParametersGroupBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ParametersGroupBox.ImeMode")));
            this.ParametersGroupBox.Location = ((System.Drawing.Point)(resources.GetObject("ParametersGroupBox.Location")));
            this.ParametersGroupBox.Name = "ParametersGroupBox";
            this.ParametersGroupBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ParametersGroupBox.RightToLeft")));
            this.ParametersGroupBox.Size = ((System.Drawing.Size)(resources.GetObject("ParametersGroupBox.Size")));
            this.ParametersGroupBox.TabIndex = ((int)(resources.GetObject("ParametersGroupBox.TabIndex")));
            this.ParametersGroupBox.TabStop = false;
            this.ParametersGroupBox.Text = resources.GetString("ParametersGroupBox.Text");
            this.ParametersGroupBox.Visible = ((bool)(resources.GetObject("ParametersGroupBox.Visible")));
            // 
            // ObjectName
            // 
            this.ObjectName.AccessibleDescription = resources.GetString("ObjectName.AccessibleDescription");
            this.ObjectName.AccessibleName = resources.GetString("ObjectName.AccessibleName");
            this.ObjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ObjectName.Anchor")));
            this.ObjectName.AutoSize = ((bool)(resources.GetObject("ObjectName.AutoSize")));
            this.ObjectName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ObjectName.BackgroundImage")));
            this.ObjectName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ObjectName.BulletIndent = ((int)(resources.GetObject("ObjectName.BulletIndent")));
            this.ObjectName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ObjectName.Dock")));
            this.ObjectName.Enabled = ((bool)(resources.GetObject("ObjectName.Enabled")));
            this.ObjectName.Font = ((System.Drawing.Font)(resources.GetObject("ObjectName.Font")));
            this.ObjectName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ObjectName.ImeMode")));
            this.ObjectName.Location = ((System.Drawing.Point)(resources.GetObject("ObjectName.Location")));
            this.ObjectName.MaxLength = ((int)(resources.GetObject("ObjectName.MaxLength")));
            this.ObjectName.Multiline = ((bool)(resources.GetObject("ObjectName.Multiline")));
            this.ObjectName.Name = "ObjectName";
            this.ObjectName.RightMargin = ((int)(resources.GetObject("ObjectName.RightMargin")));
            this.ObjectName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ObjectName.RightToLeft")));
            this.ObjectName.ScrollBars = ((System.Windows.Forms.RichTextBoxScrollBars)(resources.GetObject("ObjectName.ScrollBars")));
            this.ObjectName.Size = ((System.Drawing.Size)(resources.GetObject("ObjectName.Size")));
            this.ObjectName.TabIndex = ((int)(resources.GetObject("ObjectName.TabIndex")));
            this.ObjectName.Text = resources.GetString("ObjectName.Text");
            this.ObjectName.Visible = ((bool)(resources.GetObject("ObjectName.Visible")));
            this.ObjectName.WordWrap = ((bool)(resources.GetObject("ObjectName.WordWrap")));
            this.ObjectName.ZoomFactor = ((System.Single)(resources.GetObject("ObjectName.ZoomFactor")));
            // 
            // ObjectNameLabel
            // 
            this.ObjectNameLabel.AccessibleDescription = resources.GetString("ObjectNameLabel.AccessibleDescription");
            this.ObjectNameLabel.AccessibleName = resources.GetString("ObjectNameLabel.AccessibleName");
            this.ObjectNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ObjectNameLabel.Anchor")));
            this.ObjectNameLabel.AutoSize = ((bool)(resources.GetObject("ObjectNameLabel.AutoSize")));
            this.ObjectNameLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ObjectNameLabel.Dock")));
            this.ObjectNameLabel.Enabled = ((bool)(resources.GetObject("ObjectNameLabel.Enabled")));
            this.ObjectNameLabel.Font = ((System.Drawing.Font)(resources.GetObject("ObjectNameLabel.Font")));
            this.ObjectNameLabel.Image = ((System.Drawing.Image)(resources.GetObject("ObjectNameLabel.Image")));
            this.ObjectNameLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ObjectNameLabel.ImageAlign")));
            this.ObjectNameLabel.ImageIndex = ((int)(resources.GetObject("ObjectNameLabel.ImageIndex")));
            this.ObjectNameLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ObjectNameLabel.ImeMode")));
            this.ObjectNameLabel.Location = ((System.Drawing.Point)(resources.GetObject("ObjectNameLabel.Location")));
            this.ObjectNameLabel.Name = "ObjectNameLabel";
            this.ObjectNameLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ObjectNameLabel.RightToLeft")));
            this.ObjectNameLabel.Size = ((System.Drawing.Size)(resources.GetObject("ObjectNameLabel.Size")));
            this.ObjectNameLabel.TabIndex = ((int)(resources.GetObject("ObjectNameLabel.TabIndex")));
            this.ObjectNameLabel.Text = resources.GetString("ObjectNameLabel.Text");
            this.ObjectNameLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ObjectNameLabel.TextAlign")));
            this.ObjectNameLabel.Visible = ((bool)(resources.GetObject("ObjectNameLabel.Visible")));
            // 
            // UserOrRoleName
            // 
            this.UserOrRoleName.AccessibleDescription = resources.GetString("UserOrRoleName.AccessibleDescription");
            this.UserOrRoleName.AccessibleName = resources.GetString("UserOrRoleName.AccessibleName");
            this.UserOrRoleName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("UserOrRoleName.Anchor")));
            this.UserOrRoleName.AutoSize = ((bool)(resources.GetObject("UserOrRoleName.AutoSize")));
            this.UserOrRoleName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("UserOrRoleName.Dock")));
            this.UserOrRoleName.Enabled = ((bool)(resources.GetObject("UserOrRoleName.Enabled")));
            this.UserOrRoleName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.UserOrRoleName.Font = ((System.Drawing.Font)(resources.GetObject("UserOrRoleName.Font")));
            this.UserOrRoleName.Image = ((System.Drawing.Image)(resources.GetObject("UserOrRoleName.Image")));
            this.UserOrRoleName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UserOrRoleName.ImageAlign")));
            this.UserOrRoleName.ImageIndex = ((int)(resources.GetObject("UserOrRoleName.ImageIndex")));
            this.UserOrRoleName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("UserOrRoleName.ImeMode")));
            this.UserOrRoleName.Location = ((System.Drawing.Point)(resources.GetObject("UserOrRoleName.Location")));
            this.UserOrRoleName.Name = "UserOrRoleName";
            this.UserOrRoleName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("UserOrRoleName.RightToLeft")));
            this.UserOrRoleName.Size = ((System.Drawing.Size)(resources.GetObject("UserOrRoleName.Size")));
            this.UserOrRoleName.TabIndex = ((int)(resources.GetObject("UserOrRoleName.TabIndex")));
            this.UserOrRoleName.Text = resources.GetString("UserOrRoleName.Text");
            this.UserOrRoleName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UserOrRoleName.TextAlign")));
            this.UserOrRoleName.Visible = ((bool)(resources.GetObject("UserOrRoleName.Visible")));
            // 
            // UserOrRoleNameLabel
            // 
            this.UserOrRoleNameLabel.AccessibleDescription = resources.GetString("UserOrRoleNameLabel.AccessibleDescription");
            this.UserOrRoleNameLabel.AccessibleName = resources.GetString("UserOrRoleNameLabel.AccessibleName");
            this.UserOrRoleNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("UserOrRoleNameLabel.Anchor")));
            this.UserOrRoleNameLabel.AutoSize = ((bool)(resources.GetObject("UserOrRoleNameLabel.AutoSize")));
            this.UserOrRoleNameLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("UserOrRoleNameLabel.Dock")));
            this.UserOrRoleNameLabel.Enabled = ((bool)(resources.GetObject("UserOrRoleNameLabel.Enabled")));
            this.UserOrRoleNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.UserOrRoleNameLabel.Font = ((System.Drawing.Font)(resources.GetObject("UserOrRoleNameLabel.Font")));
            this.UserOrRoleNameLabel.Image = ((System.Drawing.Image)(resources.GetObject("UserOrRoleNameLabel.Image")));
            this.UserOrRoleNameLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UserOrRoleNameLabel.ImageAlign")));
            this.UserOrRoleNameLabel.ImageIndex = ((int)(resources.GetObject("UserOrRoleNameLabel.ImageIndex")));
            this.UserOrRoleNameLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("UserOrRoleNameLabel.ImeMode")));
            this.UserOrRoleNameLabel.Location = ((System.Drawing.Point)(resources.GetObject("UserOrRoleNameLabel.Location")));
            this.UserOrRoleNameLabel.Name = "UserOrRoleNameLabel";
            this.UserOrRoleNameLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("UserOrRoleNameLabel.RightToLeft")));
            this.UserOrRoleNameLabel.Size = ((System.Drawing.Size)(resources.GetObject("UserOrRoleNameLabel.Size")));
            this.UserOrRoleNameLabel.TabIndex = ((int)(resources.GetObject("UserOrRoleNameLabel.TabIndex")));
            this.UserOrRoleNameLabel.Text = resources.GetString("UserOrRoleNameLabel.Text");
            this.UserOrRoleNameLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UserOrRoleNameLabel.TextAlign")));
            this.UserOrRoleNameLabel.Visible = ((bool)(resources.GetObject("UserOrRoleNameLabel.Visible")));
            // 
            // WizardCompanyName
            // 
            this.WizardCompanyName.AccessibleDescription = resources.GetString("WizardCompanyName.AccessibleDescription");
            this.WizardCompanyName.AccessibleName = resources.GetString("WizardCompanyName.AccessibleName");
            this.WizardCompanyName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WizardCompanyName.Anchor")));
            this.WizardCompanyName.AutoSize = ((bool)(resources.GetObject("WizardCompanyName.AutoSize")));
            this.WizardCompanyName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WizardCompanyName.Dock")));
            this.WizardCompanyName.Enabled = ((bool)(resources.GetObject("WizardCompanyName.Enabled")));
            this.WizardCompanyName.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.WizardCompanyName.Font = ((System.Drawing.Font)(resources.GetObject("WizardCompanyName.Font")));
            this.WizardCompanyName.Image = ((System.Drawing.Image)(resources.GetObject("WizardCompanyName.Image")));
            this.WizardCompanyName.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("WizardCompanyName.ImageAlign")));
            this.WizardCompanyName.ImageIndex = ((int)(resources.GetObject("WizardCompanyName.ImageIndex")));
            this.WizardCompanyName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WizardCompanyName.ImeMode")));
            this.WizardCompanyName.Location = ((System.Drawing.Point)(resources.GetObject("WizardCompanyName.Location")));
            this.WizardCompanyName.Name = "WizardCompanyName";
            this.WizardCompanyName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WizardCompanyName.RightToLeft")));
            this.WizardCompanyName.Size = ((System.Drawing.Size)(resources.GetObject("WizardCompanyName.Size")));
            this.WizardCompanyName.TabIndex = ((int)(resources.GetObject("WizardCompanyName.TabIndex")));
            this.WizardCompanyName.Text = resources.GetString("WizardCompanyName.Text");
            this.WizardCompanyName.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("WizardCompanyName.TextAlign")));
            this.WizardCompanyName.Visible = ((bool)(resources.GetObject("WizardCompanyName.Visible")));
            // 
            // CompanyNameLabel
            // 
            this.CompanyNameLabel.AccessibleDescription = resources.GetString("CompanyNameLabel.AccessibleDescription");
            this.CompanyNameLabel.AccessibleName = resources.GetString("CompanyNameLabel.AccessibleName");
            this.CompanyNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CompanyNameLabel.Anchor")));
            this.CompanyNameLabel.AutoSize = ((bool)(resources.GetObject("CompanyNameLabel.AutoSize")));
            this.CompanyNameLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CompanyNameLabel.Dock")));
            this.CompanyNameLabel.Enabled = ((bool)(resources.GetObject("CompanyNameLabel.Enabled")));
            this.CompanyNameLabel.Font = ((System.Drawing.Font)(resources.GetObject("CompanyNameLabel.Font")));
            this.CompanyNameLabel.Image = ((System.Drawing.Image)(resources.GetObject("CompanyNameLabel.Image")));
            this.CompanyNameLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CompanyNameLabel.ImageAlign")));
            this.CompanyNameLabel.ImageIndex = ((int)(resources.GetObject("CompanyNameLabel.ImageIndex")));
            this.CompanyNameLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CompanyNameLabel.ImeMode")));
            this.CompanyNameLabel.Location = ((System.Drawing.Point)(resources.GetObject("CompanyNameLabel.Location")));
            this.CompanyNameLabel.Name = "CompanyNameLabel";
            this.CompanyNameLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CompanyNameLabel.RightToLeft")));
            this.CompanyNameLabel.Size = ((System.Drawing.Size)(resources.GetObject("CompanyNameLabel.Size")));
            this.CompanyNameLabel.TabIndex = ((int)(resources.GetObject("CompanyNameLabel.TabIndex")));
            this.CompanyNameLabel.Text = resources.GetString("CompanyNameLabel.Text");
            this.CompanyNameLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CompanyNameLabel.TextAlign")));
            this.CompanyNameLabel.Visible = ((bool)(resources.GetObject("CompanyNameLabel.Visible")));
            // 
            // WelcomeWizardForm
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.ParametersGroupBox);
            this.Controls.Add(this.DescriptionLabel);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "WelcomeWizardForm";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.Controls.SetChildIndex(this.m_watermarkPicture, 0);
            this.Controls.SetChildIndex(this.m_titleLabel, 0);
            this.Controls.SetChildIndex(this.DescriptionLabel, 0);
            this.Controls.SetChildIndex(this.ParametersGroupBox, 0);
            this.ParametersGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
