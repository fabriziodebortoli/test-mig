
namespace Microarea.Console.Core.DataManager.Common
{
    partial class ConfigurationFileParamPage
    {
        private System.Windows.Forms.GroupBox ParamsGroupBox;
        private System.Windows.Forms.CheckBox SaveFileCheckBox;
        private System.Windows.Forms.TextBox PathTextBox;
        private System.Windows.Forms.Button BrowseButton;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ConfigurationFileParamPage));
            this.ParamsGroupBox = new System.Windows.Forms.GroupBox();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.PathTextBox = new System.Windows.Forms.TextBox();
            this.SaveFileCheckBox = new System.Windows.Forms.CheckBox();
            this.ParamsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_headerPanel
            // 
            this.m_headerPanel.Name = "m_headerPanel";
            this.m_headerPanel.Size = ((System.Drawing.Size)(resources.GetObject("m_headerPanel.Size")));
            // 
            // m_headerPicture
            // 
            this.m_headerPicture.Location = ((System.Drawing.Point)(resources.GetObject("m_headerPicture.Location")));
            this.m_headerPicture.Name = "m_headerPicture";
            this.m_headerPicture.Size = ((System.Drawing.Size)(resources.GetObject("m_headerPicture.Size")));
            this.m_headerPicture.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("m_headerPicture.SizeMode")));
            // 
            // m_titleLabel
            // 
            this.m_titleLabel.Font = ((System.Drawing.Font)(resources.GetObject("m_titleLabel.Font")));
            this.m_titleLabel.Name = "m_titleLabel";
            this.m_titleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_titleLabel.Size")));
            this.m_titleLabel.Text = resources.GetString("m_titleLabel.Text");
            // 
            // m_subtitleLabel
            // 
            this.m_subtitleLabel.Name = "m_subtitleLabel";
            this.m_subtitleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_subtitleLabel.Size")));
            this.m_subtitleLabel.Text = resources.GetString("m_subtitleLabel.Text");
            // 
            // ParamsGroupBox
            // 
            this.ParamsGroupBox.AccessibleDescription = resources.GetString("ParamsGroupBox.AccessibleDescription");
            this.ParamsGroupBox.AccessibleName = resources.GetString("ParamsGroupBox.AccessibleName");
            this.ParamsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ParamsGroupBox.Anchor")));
            this.ParamsGroupBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ParamsGroupBox.BackgroundImage")));
            this.ParamsGroupBox.Controls.Add(this.BrowseButton);
            this.ParamsGroupBox.Controls.Add(this.PathTextBox);
            this.ParamsGroupBox.Controls.Add(this.SaveFileCheckBox);
            this.ParamsGroupBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ParamsGroupBox.Dock")));
            this.ParamsGroupBox.Enabled = ((bool)(resources.GetObject("ParamsGroupBox.Enabled")));
            this.ParamsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ParamsGroupBox.Font = ((System.Drawing.Font)(resources.GetObject("ParamsGroupBox.Font")));
            this.ParamsGroupBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ParamsGroupBox.ImeMode")));
            this.ParamsGroupBox.Location = ((System.Drawing.Point)(resources.GetObject("ParamsGroupBox.Location")));
            this.ParamsGroupBox.Name = "ParamsGroupBox";
            this.ParamsGroupBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ParamsGroupBox.RightToLeft")));
            this.ParamsGroupBox.Size = ((System.Drawing.Size)(resources.GetObject("ParamsGroupBox.Size")));
            this.ParamsGroupBox.TabIndex = ((int)(resources.GetObject("ParamsGroupBox.TabIndex")));
            this.ParamsGroupBox.TabStop = false;
            this.ParamsGroupBox.Text = resources.GetString("ParamsGroupBox.Text");
            this.ParamsGroupBox.Visible = ((bool)(resources.GetObject("ParamsGroupBox.Visible")));
            // 
            // BrowseButton
            // 
            this.BrowseButton.AccessibleDescription = resources.GetString("BrowseButton.AccessibleDescription");
            this.BrowseButton.AccessibleName = resources.GetString("BrowseButton.AccessibleName");
            this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BrowseButton.Anchor")));
            this.BrowseButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BrowseButton.BackgroundImage")));
            this.BrowseButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BrowseButton.Dock")));
            this.BrowseButton.Enabled = ((bool)(resources.GetObject("BrowseButton.Enabled")));
            this.BrowseButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BrowseButton.FlatStyle")));
            this.BrowseButton.Font = ((System.Drawing.Font)(resources.GetObject("BrowseButton.Font")));
            this.BrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("BrowseButton.Image")));
            this.BrowseButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BrowseButton.ImageAlign")));
            this.BrowseButton.ImageIndex = ((int)(resources.GetObject("BrowseButton.ImageIndex")));
            this.BrowseButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BrowseButton.ImeMode")));
            this.BrowseButton.Location = ((System.Drawing.Point)(resources.GetObject("BrowseButton.Location")));
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BrowseButton.RightToLeft")));
            this.BrowseButton.Size = ((System.Drawing.Size)(resources.GetObject("BrowseButton.Size")));
            this.BrowseButton.TabIndex = ((int)(resources.GetObject("BrowseButton.TabIndex")));
            this.BrowseButton.Text = resources.GetString("BrowseButton.Text");
            this.BrowseButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BrowseButton.TextAlign")));
            this.BrowseButton.Visible = ((bool)(resources.GetObject("BrowseButton.Visible")));
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // PathTextBox
            // 
            this.PathTextBox.AccessibleDescription = resources.GetString("PathTextBox.AccessibleDescription");
            this.PathTextBox.AccessibleName = resources.GetString("PathTextBox.AccessibleName");
            this.PathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("PathTextBox.Anchor")));
            this.PathTextBox.AutoSize = ((bool)(resources.GetObject("PathTextBox.AutoSize")));
            this.PathTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("PathTextBox.BackgroundImage")));
            this.PathTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("PathTextBox.Dock")));
            this.PathTextBox.Enabled = ((bool)(resources.GetObject("PathTextBox.Enabled")));
            this.PathTextBox.Font = ((System.Drawing.Font)(resources.GetObject("PathTextBox.Font")));
            this.PathTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("PathTextBox.ImeMode")));
            this.PathTextBox.Location = ((System.Drawing.Point)(resources.GetObject("PathTextBox.Location")));
            this.PathTextBox.MaxLength = ((int)(resources.GetObject("PathTextBox.MaxLength")));
            this.PathTextBox.Multiline = ((bool)(resources.GetObject("PathTextBox.Multiline")));
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.PasswordChar = ((char)(resources.GetObject("PathTextBox.PasswordChar")));
            this.PathTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("PathTextBox.RightToLeft")));
            this.PathTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("PathTextBox.ScrollBars")));
            this.PathTextBox.Size = ((System.Drawing.Size)(resources.GetObject("PathTextBox.Size")));
            this.PathTextBox.TabIndex = ((int)(resources.GetObject("PathTextBox.TabIndex")));
            this.PathTextBox.Text = resources.GetString("PathTextBox.Text");
            this.PathTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("PathTextBox.TextAlign")));
            this.PathTextBox.Visible = ((bool)(resources.GetObject("PathTextBox.Visible")));
            this.PathTextBox.WordWrap = ((bool)(resources.GetObject("PathTextBox.WordWrap")));
            // 
            // SaveFileCheckBox
            // 
            this.SaveFileCheckBox.AccessibleDescription = resources.GetString("SaveFileCheckBox.AccessibleDescription");
            this.SaveFileCheckBox.AccessibleName = resources.GetString("SaveFileCheckBox.AccessibleName");
            this.SaveFileCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SaveFileCheckBox.Anchor")));
            this.SaveFileCheckBox.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("SaveFileCheckBox.Appearance")));
            this.SaveFileCheckBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveFileCheckBox.BackgroundImage")));
            this.SaveFileCheckBox.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SaveFileCheckBox.CheckAlign")));
            this.SaveFileCheckBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SaveFileCheckBox.Dock")));
            this.SaveFileCheckBox.Enabled = ((bool)(resources.GetObject("SaveFileCheckBox.Enabled")));
            this.SaveFileCheckBox.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("SaveFileCheckBox.FlatStyle")));
            this.SaveFileCheckBox.Font = ((System.Drawing.Font)(resources.GetObject("SaveFileCheckBox.Font")));
            this.SaveFileCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("SaveFileCheckBox.Image")));
            this.SaveFileCheckBox.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SaveFileCheckBox.ImageAlign")));
            this.SaveFileCheckBox.ImageIndex = ((int)(resources.GetObject("SaveFileCheckBox.ImageIndex")));
            this.SaveFileCheckBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SaveFileCheckBox.ImeMode")));
            this.SaveFileCheckBox.Location = ((System.Drawing.Point)(resources.GetObject("SaveFileCheckBox.Location")));
            this.SaveFileCheckBox.Name = "SaveFileCheckBox";
            this.SaveFileCheckBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SaveFileCheckBox.RightToLeft")));
            this.SaveFileCheckBox.Size = ((System.Drawing.Size)(resources.GetObject("SaveFileCheckBox.Size")));
            this.SaveFileCheckBox.TabIndex = ((int)(resources.GetObject("SaveFileCheckBox.TabIndex")));
            this.SaveFileCheckBox.Text = resources.GetString("SaveFileCheckBox.Text");
            this.SaveFileCheckBox.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SaveFileCheckBox.TextAlign")));
            this.SaveFileCheckBox.Visible = ((bool)(resources.GetObject("SaveFileCheckBox.Visible")));
            this.SaveFileCheckBox.CheckedChanged += new System.EventHandler(this.SaveFileCheckBox_CheckedChanged);
            // 
            // ConfigurationFileParamPage
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.ParamsGroupBox);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "ConfigurationFileParamPage";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.Controls.SetChildIndex(this.ParamsGroupBox, 0);
            this.Controls.SetChildIndex(this.m_headerPanel, 0);
            this.ParamsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
