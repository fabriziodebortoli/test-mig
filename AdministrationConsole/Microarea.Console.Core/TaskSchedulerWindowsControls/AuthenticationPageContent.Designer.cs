
namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
    partial class AuthenticationPageContent
    {
        private System.Windows.Forms.RadioButton WindowsUserRadioButton;
        private System.Windows.Forms.GroupBox ImpersonationGroupBox;
        private System.Windows.Forms.Label DomainLabel;
        private System.Windows.Forms.ComboBox ImpersonationDomainsComboBox;
        private System.Windows.Forms.Label UserLabel;
        private System.Windows.Forms.TextBox ImpersonationUserTextBox;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.RadioButton CurrentUserRadioButton;
        private System.Windows.Forms.TextBox ImpersonationPasswordTextBox;

        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AuthenticationPageContent));
            this.ImpersonationGroupBox = new System.Windows.Forms.GroupBox();
            this.ImpersonationDomainsComboBox = new System.Windows.Forms.ComboBox();
            this.DomainLabel = new System.Windows.Forms.Label();
            this.ImpersonationPasswordTextBox = new System.Windows.Forms.TextBox();
            this.ImpersonationUserTextBox = new System.Windows.Forms.TextBox();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.UserLabel = new System.Windows.Forms.Label();
            this.WindowsUserRadioButton = new System.Windows.Forms.RadioButton();
            this.CurrentUserRadioButton = new System.Windows.Forms.RadioButton();
            this.ImpersonationGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImpersonationGroupBox
            // 
            this.ImpersonationGroupBox.AccessibleDescription = resources.GetString("ImpersonationGroupBox.AccessibleDescription");
            this.ImpersonationGroupBox.AccessibleName = resources.GetString("ImpersonationGroupBox.AccessibleName");
            this.ImpersonationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ImpersonationGroupBox.Anchor")));
            this.ImpersonationGroupBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ImpersonationGroupBox.BackgroundImage")));
            this.ImpersonationGroupBox.Controls.Add(this.ImpersonationDomainsComboBox);
            this.ImpersonationGroupBox.Controls.Add(this.DomainLabel);
            this.ImpersonationGroupBox.Controls.Add(this.ImpersonationPasswordTextBox);
            this.ImpersonationGroupBox.Controls.Add(this.ImpersonationUserTextBox);
            this.ImpersonationGroupBox.Controls.Add(this.PasswordLabel);
            this.ImpersonationGroupBox.Controls.Add(this.UserLabel);
            this.ImpersonationGroupBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ImpersonationGroupBox.Dock")));
            this.ImpersonationGroupBox.Enabled = ((bool)(resources.GetObject("ImpersonationGroupBox.Enabled")));
            this.ImpersonationGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ImpersonationGroupBox.Font = ((System.Drawing.Font)(resources.GetObject("ImpersonationGroupBox.Font")));
            this.ImpersonationGroupBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ImpersonationGroupBox.ImeMode")));
            this.ImpersonationGroupBox.Location = ((System.Drawing.Point)(resources.GetObject("ImpersonationGroupBox.Location")));
            this.ImpersonationGroupBox.Name = "ImpersonationGroupBox";
            this.ImpersonationGroupBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ImpersonationGroupBox.RightToLeft")));
            this.ImpersonationGroupBox.Size = ((System.Drawing.Size)(resources.GetObject("ImpersonationGroupBox.Size")));
            this.ImpersonationGroupBox.TabIndex = ((int)(resources.GetObject("ImpersonationGroupBox.TabIndex")));
            this.ImpersonationGroupBox.TabStop = false;
            this.ImpersonationGroupBox.Text = resources.GetString("ImpersonationGroupBox.Text");
            this.ImpersonationGroupBox.Visible = ((bool)(resources.GetObject("ImpersonationGroupBox.Visible")));
            // 
            // ImpersonationDomainsComboBox
            // 
            this.ImpersonationDomainsComboBox.AccessibleDescription = resources.GetString("ImpersonationDomainsComboBox.AccessibleDescription");
            this.ImpersonationDomainsComboBox.AccessibleName = resources.GetString("ImpersonationDomainsComboBox.AccessibleName");
            this.ImpersonationDomainsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ImpersonationDomainsComboBox.Anchor")));
            this.ImpersonationDomainsComboBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ImpersonationDomainsComboBox.BackgroundImage")));
            this.ImpersonationDomainsComboBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ImpersonationDomainsComboBox.Dock")));
            this.ImpersonationDomainsComboBox.Enabled = ((bool)(resources.GetObject("ImpersonationDomainsComboBox.Enabled")));
            this.ImpersonationDomainsComboBox.Font = ((System.Drawing.Font)(resources.GetObject("ImpersonationDomainsComboBox.Font")));
            this.ImpersonationDomainsComboBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ImpersonationDomainsComboBox.ImeMode")));
            this.ImpersonationDomainsComboBox.IntegralHeight = ((bool)(resources.GetObject("ImpersonationDomainsComboBox.IntegralHeight")));
            this.ImpersonationDomainsComboBox.ItemHeight = ((int)(resources.GetObject("ImpersonationDomainsComboBox.ItemHeight")));
            this.ImpersonationDomainsComboBox.Location = ((System.Drawing.Point)(resources.GetObject("ImpersonationDomainsComboBox.Location")));
            this.ImpersonationDomainsComboBox.MaxDropDownItems = ((int)(resources.GetObject("ImpersonationDomainsComboBox.MaxDropDownItems")));
            this.ImpersonationDomainsComboBox.MaxLength = ((int)(resources.GetObject("ImpersonationDomainsComboBox.MaxLength")));
            this.ImpersonationDomainsComboBox.Name = "ImpersonationDomainsComboBox";
            this.ImpersonationDomainsComboBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ImpersonationDomainsComboBox.RightToLeft")));
            this.ImpersonationDomainsComboBox.Size = ((System.Drawing.Size)(resources.GetObject("ImpersonationDomainsComboBox.Size")));
            this.ImpersonationDomainsComboBox.TabIndex = ((int)(resources.GetObject("ImpersonationDomainsComboBox.TabIndex")));
            this.ImpersonationDomainsComboBox.Text = resources.GetString("ImpersonationDomainsComboBox.Text");
            this.ImpersonationDomainsComboBox.Visible = ((bool)(resources.GetObject("ImpersonationDomainsComboBox.Visible")));
            // 
            // DomainLabel
            // 
            this.DomainLabel.AccessibleDescription = resources.GetString("DomainLabel.AccessibleDescription");
            this.DomainLabel.AccessibleName = resources.GetString("DomainLabel.AccessibleName");
            this.DomainLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DomainLabel.Anchor")));
            this.DomainLabel.AutoSize = ((bool)(resources.GetObject("DomainLabel.AutoSize")));
            this.DomainLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DomainLabel.Dock")));
            this.DomainLabel.Enabled = ((bool)(resources.GetObject("DomainLabel.Enabled")));
            this.DomainLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.DomainLabel.Font = ((System.Drawing.Font)(resources.GetObject("DomainLabel.Font")));
            this.DomainLabel.Image = ((System.Drawing.Image)(resources.GetObject("DomainLabel.Image")));
            this.DomainLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DomainLabel.ImageAlign")));
            this.DomainLabel.ImageIndex = ((int)(resources.GetObject("DomainLabel.ImageIndex")));
            this.DomainLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DomainLabel.ImeMode")));
            this.DomainLabel.Location = ((System.Drawing.Point)(resources.GetObject("DomainLabel.Location")));
            this.DomainLabel.Name = "DomainLabel";
            this.DomainLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DomainLabel.RightToLeft")));
            this.DomainLabel.Size = ((System.Drawing.Size)(resources.GetObject("DomainLabel.Size")));
            this.DomainLabel.TabIndex = ((int)(resources.GetObject("DomainLabel.TabIndex")));
            this.DomainLabel.Text = resources.GetString("DomainLabel.Text");
            this.DomainLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DomainLabel.TextAlign")));
            this.DomainLabel.Visible = ((bool)(resources.GetObject("DomainLabel.Visible")));
            // 
            // ImpersonationPasswordTextBox
            // 
            this.ImpersonationPasswordTextBox.AcceptsReturn = true;
            this.ImpersonationPasswordTextBox.AccessibleDescription = resources.GetString("ImpersonationPasswordTextBox.AccessibleDescription");
            this.ImpersonationPasswordTextBox.AccessibleName = resources.GetString("ImpersonationPasswordTextBox.AccessibleName");
            this.ImpersonationPasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ImpersonationPasswordTextBox.Anchor")));
            this.ImpersonationPasswordTextBox.AutoSize = ((bool)(resources.GetObject("ImpersonationPasswordTextBox.AutoSize")));
            this.ImpersonationPasswordTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ImpersonationPasswordTextBox.BackgroundImage")));
            this.ImpersonationPasswordTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ImpersonationPasswordTextBox.Dock")));
            this.ImpersonationPasswordTextBox.Enabled = ((bool)(resources.GetObject("ImpersonationPasswordTextBox.Enabled")));
            this.ImpersonationPasswordTextBox.Font = ((System.Drawing.Font)(resources.GetObject("ImpersonationPasswordTextBox.Font")));
            this.ImpersonationPasswordTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ImpersonationPasswordTextBox.ImeMode")));
            this.ImpersonationPasswordTextBox.Location = ((System.Drawing.Point)(resources.GetObject("ImpersonationPasswordTextBox.Location")));
            this.ImpersonationPasswordTextBox.MaxLength = ((int)(resources.GetObject("ImpersonationPasswordTextBox.MaxLength")));
            this.ImpersonationPasswordTextBox.Multiline = ((bool)(resources.GetObject("ImpersonationPasswordTextBox.Multiline")));
            this.ImpersonationPasswordTextBox.Name = "ImpersonationPasswordTextBox";
            this.ImpersonationPasswordTextBox.PasswordChar = ((char)(resources.GetObject("ImpersonationPasswordTextBox.PasswordChar")));
            this.ImpersonationPasswordTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ImpersonationPasswordTextBox.RightToLeft")));
            this.ImpersonationPasswordTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ImpersonationPasswordTextBox.ScrollBars")));
            this.ImpersonationPasswordTextBox.Size = ((System.Drawing.Size)(resources.GetObject("ImpersonationPasswordTextBox.Size")));
            this.ImpersonationPasswordTextBox.TabIndex = ((int)(resources.GetObject("ImpersonationPasswordTextBox.TabIndex")));
            this.ImpersonationPasswordTextBox.Text = resources.GetString("ImpersonationPasswordTextBox.Text");
            this.ImpersonationPasswordTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ImpersonationPasswordTextBox.TextAlign")));
            this.ImpersonationPasswordTextBox.Visible = ((bool)(resources.GetObject("ImpersonationPasswordTextBox.Visible")));
            this.ImpersonationPasswordTextBox.WordWrap = ((bool)(resources.GetObject("ImpersonationPasswordTextBox.WordWrap")));
            this.ImpersonationPasswordTextBox.TextChanged += new System.EventHandler(this.ImpersonationPasswordTextBox_TextChanged);
            // 
            // ImpersonationUserTextBox
            // 
            this.ImpersonationUserTextBox.AccessibleDescription = resources.GetString("ImpersonationUserTextBox.AccessibleDescription");
            this.ImpersonationUserTextBox.AccessibleName = resources.GetString("ImpersonationUserTextBox.AccessibleName");
            this.ImpersonationUserTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ImpersonationUserTextBox.Anchor")));
            this.ImpersonationUserTextBox.AutoSize = ((bool)(resources.GetObject("ImpersonationUserTextBox.AutoSize")));
            this.ImpersonationUserTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ImpersonationUserTextBox.BackgroundImage")));
            this.ImpersonationUserTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ImpersonationUserTextBox.Dock")));
            this.ImpersonationUserTextBox.Enabled = ((bool)(resources.GetObject("ImpersonationUserTextBox.Enabled")));
            this.ImpersonationUserTextBox.Font = ((System.Drawing.Font)(resources.GetObject("ImpersonationUserTextBox.Font")));
            this.ImpersonationUserTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ImpersonationUserTextBox.ImeMode")));
            this.ImpersonationUserTextBox.Location = ((System.Drawing.Point)(resources.GetObject("ImpersonationUserTextBox.Location")));
            this.ImpersonationUserTextBox.MaxLength = ((int)(resources.GetObject("ImpersonationUserTextBox.MaxLength")));
            this.ImpersonationUserTextBox.Multiline = ((bool)(resources.GetObject("ImpersonationUserTextBox.Multiline")));
            this.ImpersonationUserTextBox.Name = "ImpersonationUserTextBox";
            this.ImpersonationUserTextBox.PasswordChar = ((char)(resources.GetObject("ImpersonationUserTextBox.PasswordChar")));
            this.ImpersonationUserTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ImpersonationUserTextBox.RightToLeft")));
            this.ImpersonationUserTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ImpersonationUserTextBox.ScrollBars")));
            this.ImpersonationUserTextBox.Size = ((System.Drawing.Size)(resources.GetObject("ImpersonationUserTextBox.Size")));
            this.ImpersonationUserTextBox.TabIndex = ((int)(resources.GetObject("ImpersonationUserTextBox.TabIndex")));
            this.ImpersonationUserTextBox.Text = resources.GetString("ImpersonationUserTextBox.Text");
            this.ImpersonationUserTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ImpersonationUserTextBox.TextAlign")));
            this.ImpersonationUserTextBox.Visible = ((bool)(resources.GetObject("ImpersonationUserTextBox.Visible")));
            this.ImpersonationUserTextBox.WordWrap = ((bool)(resources.GetObject("ImpersonationUserTextBox.WordWrap")));
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AccessibleDescription = resources.GetString("PasswordLabel.AccessibleDescription");
            this.PasswordLabel.AccessibleName = resources.GetString("PasswordLabel.AccessibleName");
            this.PasswordLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PasswordLabel.Anchor")));
            this.PasswordLabel.AutoSize = ((bool)(resources.GetObject("PasswordLabel.AutoSize")));
            this.PasswordLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PasswordLabel.Dock")));
            this.PasswordLabel.Enabled = ((bool)(resources.GetObject("PasswordLabel.Enabled")));
            this.PasswordLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.PasswordLabel.Font = ((System.Drawing.Font)(resources.GetObject("PasswordLabel.Font")));
            this.PasswordLabel.Image = ((System.Drawing.Image)(resources.GetObject("PasswordLabel.Image")));
            this.PasswordLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PasswordLabel.ImageAlign")));
            this.PasswordLabel.ImageIndex = ((int)(resources.GetObject("PasswordLabel.ImageIndex")));
            this.PasswordLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PasswordLabel.ImeMode")));
            this.PasswordLabel.Location = ((System.Drawing.Point)(resources.GetObject("PasswordLabel.Location")));
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PasswordLabel.RightToLeft")));
            this.PasswordLabel.Size = ((System.Drawing.Size)(resources.GetObject("PasswordLabel.Size")));
            this.PasswordLabel.TabIndex = ((int)(resources.GetObject("PasswordLabel.TabIndex")));
            this.PasswordLabel.Text = resources.GetString("PasswordLabel.Text");
            this.PasswordLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("PasswordLabel.TextAlign")));
            this.PasswordLabel.Visible = ((bool)(resources.GetObject("PasswordLabel.Visible")));
            // 
            // UserLabel
            // 
            this.UserLabel.AccessibleDescription = resources.GetString("UserLabel.AccessibleDescription");
            this.UserLabel.AccessibleName = resources.GetString("UserLabel.AccessibleName");
            this.UserLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("UserLabel.Anchor")));
            this.UserLabel.AutoSize = ((bool)(resources.GetObject("UserLabel.AutoSize")));
            this.UserLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("UserLabel.Dock")));
            this.UserLabel.Enabled = ((bool)(resources.GetObject("UserLabel.Enabled")));
            this.UserLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.UserLabel.Font = ((System.Drawing.Font)(resources.GetObject("UserLabel.Font")));
            this.UserLabel.Image = ((System.Drawing.Image)(resources.GetObject("UserLabel.Image")));
            this.UserLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UserLabel.ImageAlign")));
            this.UserLabel.ImageIndex = ((int)(resources.GetObject("UserLabel.ImageIndex")));
            this.UserLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("UserLabel.ImeMode")));
            this.UserLabel.Location = ((System.Drawing.Point)(resources.GetObject("UserLabel.Location")));
            this.UserLabel.Name = "UserLabel";
            this.UserLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("UserLabel.RightToLeft")));
            this.UserLabel.Size = ((System.Drawing.Size)(resources.GetObject("UserLabel.Size")));
            this.UserLabel.TabIndex = ((int)(resources.GetObject("UserLabel.TabIndex")));
            this.UserLabel.Text = resources.GetString("UserLabel.Text");
            this.UserLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UserLabel.TextAlign")));
            this.UserLabel.Visible = ((bool)(resources.GetObject("UserLabel.Visible")));
            // 
            // WindowsUserRadioButton
            // 
            this.WindowsUserRadioButton.AccessibleDescription = resources.GetString("WindowsUserRadioButton.AccessibleDescription");
            this.WindowsUserRadioButton.AccessibleName = resources.GetString("WindowsUserRadioButton.AccessibleName");
            this.WindowsUserRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WindowsUserRadioButton.Anchor")));
            this.WindowsUserRadioButton.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("WindowsUserRadioButton.Appearance")));
            this.WindowsUserRadioButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WindowsUserRadioButton.BackgroundImage")));
            this.WindowsUserRadioButton.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("WindowsUserRadioButton.CheckAlign")));
            this.WindowsUserRadioButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WindowsUserRadioButton.Dock")));
            this.WindowsUserRadioButton.Enabled = ((bool)(resources.GetObject("WindowsUserRadioButton.Enabled")));
            this.WindowsUserRadioButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("WindowsUserRadioButton.FlatStyle")));
            this.WindowsUserRadioButton.Font = ((System.Drawing.Font)(resources.GetObject("WindowsUserRadioButton.Font")));
            this.WindowsUserRadioButton.Image = ((System.Drawing.Image)(resources.GetObject("WindowsUserRadioButton.Image")));
            this.WindowsUserRadioButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("WindowsUserRadioButton.ImageAlign")));
            this.WindowsUserRadioButton.ImageIndex = ((int)(resources.GetObject("WindowsUserRadioButton.ImageIndex")));
            this.WindowsUserRadioButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WindowsUserRadioButton.ImeMode")));
            this.WindowsUserRadioButton.Location = ((System.Drawing.Point)(resources.GetObject("WindowsUserRadioButton.Location")));
            this.WindowsUserRadioButton.Name = "WindowsUserRadioButton";
            this.WindowsUserRadioButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WindowsUserRadioButton.RightToLeft")));
            this.WindowsUserRadioButton.Size = ((System.Drawing.Size)(resources.GetObject("WindowsUserRadioButton.Size")));
            this.WindowsUserRadioButton.TabIndex = ((int)(resources.GetObject("WindowsUserRadioButton.TabIndex")));
            this.WindowsUserRadioButton.Text = resources.GetString("WindowsUserRadioButton.Text");
            this.WindowsUserRadioButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("WindowsUserRadioButton.TextAlign")));
            this.WindowsUserRadioButton.Visible = ((bool)(resources.GetObject("WindowsUserRadioButton.Visible")));
            this.WindowsUserRadioButton.CheckedChanged += new System.EventHandler(this.WindowsUserRadioButton_CheckedChanged);
            // 
            // CurrentUserRadioButton
            // 
            this.CurrentUserRadioButton.AccessibleDescription = resources.GetString("CurrentUserRadioButton.AccessibleDescription");
            this.CurrentUserRadioButton.AccessibleName = resources.GetString("CurrentUserRadioButton.AccessibleName");
            this.CurrentUserRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CurrentUserRadioButton.Anchor")));
            this.CurrentUserRadioButton.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CurrentUserRadioButton.Appearance")));
            this.CurrentUserRadioButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CurrentUserRadioButton.BackgroundImage")));
            this.CurrentUserRadioButton.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CurrentUserRadioButton.CheckAlign")));
            this.CurrentUserRadioButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CurrentUserRadioButton.Dock")));
            this.CurrentUserRadioButton.Enabled = ((bool)(resources.GetObject("CurrentUserRadioButton.Enabled")));
            this.CurrentUserRadioButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CurrentUserRadioButton.FlatStyle")));
            this.CurrentUserRadioButton.Font = ((System.Drawing.Font)(resources.GetObject("CurrentUserRadioButton.Font")));
            this.CurrentUserRadioButton.Image = ((System.Drawing.Image)(resources.GetObject("CurrentUserRadioButton.Image")));
            this.CurrentUserRadioButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CurrentUserRadioButton.ImageAlign")));
            this.CurrentUserRadioButton.ImageIndex = ((int)(resources.GetObject("CurrentUserRadioButton.ImageIndex")));
            this.CurrentUserRadioButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CurrentUserRadioButton.ImeMode")));
            this.CurrentUserRadioButton.Location = ((System.Drawing.Point)(resources.GetObject("CurrentUserRadioButton.Location")));
            this.CurrentUserRadioButton.Name = "CurrentUserRadioButton";
            this.CurrentUserRadioButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CurrentUserRadioButton.RightToLeft")));
            this.CurrentUserRadioButton.Size = ((System.Drawing.Size)(resources.GetObject("CurrentUserRadioButton.Size")));
            this.CurrentUserRadioButton.TabIndex = ((int)(resources.GetObject("CurrentUserRadioButton.TabIndex")));
            this.CurrentUserRadioButton.Text = resources.GetString("CurrentUserRadioButton.Text");
            this.CurrentUserRadioButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CurrentUserRadioButton.TextAlign")));
            this.CurrentUserRadioButton.Visible = ((bool)(resources.GetObject("CurrentUserRadioButton.Visible")));
            this.CurrentUserRadioButton.CheckedChanged += new System.EventHandler(this.ASPNETUserRadioButton_CheckedChanged);
            // 
            // AuthenticationPageContent
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.ImpersonationGroupBox);
            this.Controls.Add(this.WindowsUserRadioButton);
            this.Controls.Add(this.CurrentUserRadioButton);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "AuthenticationPageContent";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.ImpersonationGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }


}
