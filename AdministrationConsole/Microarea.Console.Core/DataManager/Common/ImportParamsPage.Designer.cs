
namespace Microarea.Console.Core.DataManager.Common
{
    partial class ImportParamsPage
    {
        private System.Windows.Forms.GroupBox ParamDetailsGroupBox;
        private System.Windows.Forms.CheckBox InsertExtraFieldsCheckBox;
        private System.Windows.Forms.CheckBox DeleteTableContextCheckBox;
        private System.Windows.Forms.CheckBox DisableCheckFKCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton SkipRowRadioButton;
        private System.Windows.Forms.RadioButton UpdateRowRadioButton;
        private System.Windows.Forms.RadioButton ErrorRowRadioButton;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ImportParamsPage));
            this.InsertExtraFieldsCheckBox = new System.Windows.Forms.CheckBox();
            this.ParamDetailsGroupBox = new System.Windows.Forms.GroupBox();
            this.ErrorRowRadioButton = new System.Windows.Forms.RadioButton();
            this.UpdateRowRadioButton = new System.Windows.Forms.RadioButton();
            this.SkipRowRadioButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.DisableCheckFKCheckBox = new System.Windows.Forms.CheckBox();
            this.DeleteTableContextCheckBox = new System.Windows.Forms.CheckBox();
            this.ParamDetailsGroupBox.SuspendLayout();
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
            // InsertExtraFieldsCheckBox
            // 
            this.InsertExtraFieldsCheckBox.AccessibleDescription = resources.GetString("InsertExtraFieldsCheckBox.AccessibleDescription");
            this.InsertExtraFieldsCheckBox.AccessibleName = resources.GetString("InsertExtraFieldsCheckBox.AccessibleName");
            this.InsertExtraFieldsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("InsertExtraFieldsCheckBox.Anchor")));
            this.InsertExtraFieldsCheckBox.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("InsertExtraFieldsCheckBox.Appearance")));
            this.InsertExtraFieldsCheckBox.BackColor = System.Drawing.SystemColors.Control;
            this.InsertExtraFieldsCheckBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("InsertExtraFieldsCheckBox.BackgroundImage")));
            this.InsertExtraFieldsCheckBox.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("InsertExtraFieldsCheckBox.CheckAlign")));
            this.InsertExtraFieldsCheckBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("InsertExtraFieldsCheckBox.Dock")));
            this.InsertExtraFieldsCheckBox.Enabled = ((bool)(resources.GetObject("InsertExtraFieldsCheckBox.Enabled")));
            this.InsertExtraFieldsCheckBox.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("InsertExtraFieldsCheckBox.FlatStyle")));
            this.InsertExtraFieldsCheckBox.Font = ((System.Drawing.Font)(resources.GetObject("InsertExtraFieldsCheckBox.Font")));
            this.InsertExtraFieldsCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("InsertExtraFieldsCheckBox.Image")));
            this.InsertExtraFieldsCheckBox.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("InsertExtraFieldsCheckBox.ImageAlign")));
            this.InsertExtraFieldsCheckBox.ImageIndex = ((int)(resources.GetObject("InsertExtraFieldsCheckBox.ImageIndex")));
            this.InsertExtraFieldsCheckBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("InsertExtraFieldsCheckBox.ImeMode")));
            this.InsertExtraFieldsCheckBox.Location = ((System.Drawing.Point)(resources.GetObject("InsertExtraFieldsCheckBox.Location")));
            this.InsertExtraFieldsCheckBox.Name = "InsertExtraFieldsCheckBox";
            this.InsertExtraFieldsCheckBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("InsertExtraFieldsCheckBox.RightToLeft")));
            this.InsertExtraFieldsCheckBox.Size = ((System.Drawing.Size)(resources.GetObject("InsertExtraFieldsCheckBox.Size")));
            this.InsertExtraFieldsCheckBox.TabIndex = ((int)(resources.GetObject("InsertExtraFieldsCheckBox.TabIndex")));
            this.InsertExtraFieldsCheckBox.Text = resources.GetString("InsertExtraFieldsCheckBox.Text");
            this.InsertExtraFieldsCheckBox.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("InsertExtraFieldsCheckBox.TextAlign")));
            this.InsertExtraFieldsCheckBox.Visible = ((bool)(resources.GetObject("InsertExtraFieldsCheckBox.Visible")));
            // 
            // ParamDetailsGroupBox
            // 
            this.ParamDetailsGroupBox.AccessibleDescription = resources.GetString("ParamDetailsGroupBox.AccessibleDescription");
            this.ParamDetailsGroupBox.AccessibleName = resources.GetString("ParamDetailsGroupBox.AccessibleName");
            this.ParamDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ParamDetailsGroupBox.Anchor")));
            this.ParamDetailsGroupBox.BackColor = System.Drawing.SystemColors.Control;
            this.ParamDetailsGroupBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ParamDetailsGroupBox.BackgroundImage")));
            this.ParamDetailsGroupBox.Controls.Add(this.ErrorRowRadioButton);
            this.ParamDetailsGroupBox.Controls.Add(this.UpdateRowRadioButton);
            this.ParamDetailsGroupBox.Controls.Add(this.SkipRowRadioButton);
            this.ParamDetailsGroupBox.Controls.Add(this.label1);
            this.ParamDetailsGroupBox.Controls.Add(this.DisableCheckFKCheckBox);
            this.ParamDetailsGroupBox.Controls.Add(this.DeleteTableContextCheckBox);
            this.ParamDetailsGroupBox.Controls.Add(this.InsertExtraFieldsCheckBox);
            this.ParamDetailsGroupBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ParamDetailsGroupBox.Dock")));
            this.ParamDetailsGroupBox.Enabled = ((bool)(resources.GetObject("ParamDetailsGroupBox.Enabled")));
            this.ParamDetailsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ParamDetailsGroupBox.Font = ((System.Drawing.Font)(resources.GetObject("ParamDetailsGroupBox.Font")));
            this.ParamDetailsGroupBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ParamDetailsGroupBox.ImeMode")));
            this.ParamDetailsGroupBox.Location = ((System.Drawing.Point)(resources.GetObject("ParamDetailsGroupBox.Location")));
            this.ParamDetailsGroupBox.Name = "ParamDetailsGroupBox";
            this.ParamDetailsGroupBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ParamDetailsGroupBox.RightToLeft")));
            this.ParamDetailsGroupBox.Size = ((System.Drawing.Size)(resources.GetObject("ParamDetailsGroupBox.Size")));
            this.ParamDetailsGroupBox.TabIndex = ((int)(resources.GetObject("ParamDetailsGroupBox.TabIndex")));
            this.ParamDetailsGroupBox.TabStop = false;
            this.ParamDetailsGroupBox.Text = resources.GetString("ParamDetailsGroupBox.Text");
            this.ParamDetailsGroupBox.Visible = ((bool)(resources.GetObject("ParamDetailsGroupBox.Visible")));
            // 
            // ErrorRowRadioButton
            // 
            this.ErrorRowRadioButton.AccessibleDescription = resources.GetString("ErrorRowRadioButton.AccessibleDescription");
            this.ErrorRowRadioButton.AccessibleName = resources.GetString("ErrorRowRadioButton.AccessibleName");
            this.ErrorRowRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ErrorRowRadioButton.Anchor")));
            this.ErrorRowRadioButton.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("ErrorRowRadioButton.Appearance")));
            this.ErrorRowRadioButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ErrorRowRadioButton.BackgroundImage")));
            this.ErrorRowRadioButton.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ErrorRowRadioButton.CheckAlign")));
            this.ErrorRowRadioButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ErrorRowRadioButton.Dock")));
            this.ErrorRowRadioButton.Enabled = ((bool)(resources.GetObject("ErrorRowRadioButton.Enabled")));
            this.ErrorRowRadioButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ErrorRowRadioButton.FlatStyle")));
            this.ErrorRowRadioButton.Font = ((System.Drawing.Font)(resources.GetObject("ErrorRowRadioButton.Font")));
            this.ErrorRowRadioButton.Image = ((System.Drawing.Image)(resources.GetObject("ErrorRowRadioButton.Image")));
            this.ErrorRowRadioButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ErrorRowRadioButton.ImageAlign")));
            this.ErrorRowRadioButton.ImageIndex = ((int)(resources.GetObject("ErrorRowRadioButton.ImageIndex")));
            this.ErrorRowRadioButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ErrorRowRadioButton.ImeMode")));
            this.ErrorRowRadioButton.Location = ((System.Drawing.Point)(resources.GetObject("ErrorRowRadioButton.Location")));
            this.ErrorRowRadioButton.Name = "ErrorRowRadioButton";
            this.ErrorRowRadioButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ErrorRowRadioButton.RightToLeft")));
            this.ErrorRowRadioButton.Size = ((System.Drawing.Size)(resources.GetObject("ErrorRowRadioButton.Size")));
            this.ErrorRowRadioButton.TabIndex = ((int)(resources.GetObject("ErrorRowRadioButton.TabIndex")));
            this.ErrorRowRadioButton.Text = resources.GetString("ErrorRowRadioButton.Text");
            this.ErrorRowRadioButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ErrorRowRadioButton.TextAlign")));
            this.ErrorRowRadioButton.Visible = ((bool)(resources.GetObject("ErrorRowRadioButton.Visible")));
            // 
            // UpdateRowRadioButton
            // 
            this.UpdateRowRadioButton.AccessibleDescription = resources.GetString("UpdateRowRadioButton.AccessibleDescription");
            this.UpdateRowRadioButton.AccessibleName = resources.GetString("UpdateRowRadioButton.AccessibleName");
            this.UpdateRowRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("UpdateRowRadioButton.Anchor")));
            this.UpdateRowRadioButton.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("UpdateRowRadioButton.Appearance")));
            this.UpdateRowRadioButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("UpdateRowRadioButton.BackgroundImage")));
            this.UpdateRowRadioButton.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UpdateRowRadioButton.CheckAlign")));
            this.UpdateRowRadioButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("UpdateRowRadioButton.Dock")));
            this.UpdateRowRadioButton.Enabled = ((bool)(resources.GetObject("UpdateRowRadioButton.Enabled")));
            this.UpdateRowRadioButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("UpdateRowRadioButton.FlatStyle")));
            this.UpdateRowRadioButton.Font = ((System.Drawing.Font)(resources.GetObject("UpdateRowRadioButton.Font")));
            this.UpdateRowRadioButton.Image = ((System.Drawing.Image)(resources.GetObject("UpdateRowRadioButton.Image")));
            this.UpdateRowRadioButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UpdateRowRadioButton.ImageAlign")));
            this.UpdateRowRadioButton.ImageIndex = ((int)(resources.GetObject("UpdateRowRadioButton.ImageIndex")));
            this.UpdateRowRadioButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("UpdateRowRadioButton.ImeMode")));
            this.UpdateRowRadioButton.Location = ((System.Drawing.Point)(resources.GetObject("UpdateRowRadioButton.Location")));
            this.UpdateRowRadioButton.Name = "UpdateRowRadioButton";
            this.UpdateRowRadioButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("UpdateRowRadioButton.RightToLeft")));
            this.UpdateRowRadioButton.Size = ((System.Drawing.Size)(resources.GetObject("UpdateRowRadioButton.Size")));
            this.UpdateRowRadioButton.TabIndex = ((int)(resources.GetObject("UpdateRowRadioButton.TabIndex")));
            this.UpdateRowRadioButton.Text = resources.GetString("UpdateRowRadioButton.Text");
            this.UpdateRowRadioButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("UpdateRowRadioButton.TextAlign")));
            this.UpdateRowRadioButton.Visible = ((bool)(resources.GetObject("UpdateRowRadioButton.Visible")));
            // 
            // SkipRowRadioButton
            // 
            this.SkipRowRadioButton.AccessibleDescription = resources.GetString("SkipRowRadioButton.AccessibleDescription");
            this.SkipRowRadioButton.AccessibleName = resources.GetString("SkipRowRadioButton.AccessibleName");
            this.SkipRowRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SkipRowRadioButton.Anchor")));
            this.SkipRowRadioButton.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("SkipRowRadioButton.Appearance")));
            this.SkipRowRadioButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SkipRowRadioButton.BackgroundImage")));
            this.SkipRowRadioButton.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SkipRowRadioButton.CheckAlign")));
            this.SkipRowRadioButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SkipRowRadioButton.Dock")));
            this.SkipRowRadioButton.Enabled = ((bool)(resources.GetObject("SkipRowRadioButton.Enabled")));
            this.SkipRowRadioButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("SkipRowRadioButton.FlatStyle")));
            this.SkipRowRadioButton.Font = ((System.Drawing.Font)(resources.GetObject("SkipRowRadioButton.Font")));
            this.SkipRowRadioButton.Image = ((System.Drawing.Image)(resources.GetObject("SkipRowRadioButton.Image")));
            this.SkipRowRadioButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SkipRowRadioButton.ImageAlign")));
            this.SkipRowRadioButton.ImageIndex = ((int)(resources.GetObject("SkipRowRadioButton.ImageIndex")));
            this.SkipRowRadioButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SkipRowRadioButton.ImeMode")));
            this.SkipRowRadioButton.Location = ((System.Drawing.Point)(resources.GetObject("SkipRowRadioButton.Location")));
            this.SkipRowRadioButton.Name = "SkipRowRadioButton";
            this.SkipRowRadioButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SkipRowRadioButton.RightToLeft")));
            this.SkipRowRadioButton.Size = ((System.Drawing.Size)(resources.GetObject("SkipRowRadioButton.Size")));
            this.SkipRowRadioButton.TabIndex = ((int)(resources.GetObject("SkipRowRadioButton.TabIndex")));
            this.SkipRowRadioButton.Text = resources.GetString("SkipRowRadioButton.Text");
            this.SkipRowRadioButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SkipRowRadioButton.TextAlign")));
            this.SkipRowRadioButton.Visible = ((bool)(resources.GetObject("SkipRowRadioButton.Visible")));
            // 
            // label1
            // 
            this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
            this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
            this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
            this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
            this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
            this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
            this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
            this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
            this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
            this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
            this.label1.Name = "label1";
            this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
            this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
            this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
            this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
            // 
            // DisableCheckFKCheckBox
            // 
            this.DisableCheckFKCheckBox.AccessibleDescription = resources.GetString("DisableCheckFKCheckBox.AccessibleDescription");
            this.DisableCheckFKCheckBox.AccessibleName = resources.GetString("DisableCheckFKCheckBox.AccessibleName");
            this.DisableCheckFKCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DisableCheckFKCheckBox.Anchor")));
            this.DisableCheckFKCheckBox.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("DisableCheckFKCheckBox.Appearance")));
            this.DisableCheckFKCheckBox.BackColor = System.Drawing.SystemColors.Control;
            this.DisableCheckFKCheckBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DisableCheckFKCheckBox.BackgroundImage")));
            this.DisableCheckFKCheckBox.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DisableCheckFKCheckBox.CheckAlign")));
            this.DisableCheckFKCheckBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DisableCheckFKCheckBox.Dock")));
            this.DisableCheckFKCheckBox.Enabled = ((bool)(resources.GetObject("DisableCheckFKCheckBox.Enabled")));
            this.DisableCheckFKCheckBox.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("DisableCheckFKCheckBox.FlatStyle")));
            this.DisableCheckFKCheckBox.Font = ((System.Drawing.Font)(resources.GetObject("DisableCheckFKCheckBox.Font")));
            this.DisableCheckFKCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("DisableCheckFKCheckBox.Image")));
            this.DisableCheckFKCheckBox.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DisableCheckFKCheckBox.ImageAlign")));
            this.DisableCheckFKCheckBox.ImageIndex = ((int)(resources.GetObject("DisableCheckFKCheckBox.ImageIndex")));
            this.DisableCheckFKCheckBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DisableCheckFKCheckBox.ImeMode")));
            this.DisableCheckFKCheckBox.Location = ((System.Drawing.Point)(resources.GetObject("DisableCheckFKCheckBox.Location")));
            this.DisableCheckFKCheckBox.Name = "DisableCheckFKCheckBox";
            this.DisableCheckFKCheckBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DisableCheckFKCheckBox.RightToLeft")));
            this.DisableCheckFKCheckBox.Size = ((System.Drawing.Size)(resources.GetObject("DisableCheckFKCheckBox.Size")));
            this.DisableCheckFKCheckBox.TabIndex = ((int)(resources.GetObject("DisableCheckFKCheckBox.TabIndex")));
            this.DisableCheckFKCheckBox.Text = resources.GetString("DisableCheckFKCheckBox.Text");
            this.DisableCheckFKCheckBox.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DisableCheckFKCheckBox.TextAlign")));
            this.DisableCheckFKCheckBox.Visible = ((bool)(resources.GetObject("DisableCheckFKCheckBox.Visible")));
            // 
            // DeleteTableContextCheckBox
            // 
            this.DeleteTableContextCheckBox.AccessibleDescription = resources.GetString("DeleteTableContextCheckBox.AccessibleDescription");
            this.DeleteTableContextCheckBox.AccessibleName = resources.GetString("DeleteTableContextCheckBox.AccessibleName");
            this.DeleteTableContextCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DeleteTableContextCheckBox.Anchor")));
            this.DeleteTableContextCheckBox.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("DeleteTableContextCheckBox.Appearance")));
            this.DeleteTableContextCheckBox.BackColor = System.Drawing.SystemColors.Control;
            this.DeleteTableContextCheckBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DeleteTableContextCheckBox.BackgroundImage")));
            this.DeleteTableContextCheckBox.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteTableContextCheckBox.CheckAlign")));
            this.DeleteTableContextCheckBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DeleteTableContextCheckBox.Dock")));
            this.DeleteTableContextCheckBox.Enabled = ((bool)(resources.GetObject("DeleteTableContextCheckBox.Enabled")));
            this.DeleteTableContextCheckBox.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("DeleteTableContextCheckBox.FlatStyle")));
            this.DeleteTableContextCheckBox.Font = ((System.Drawing.Font)(resources.GetObject("DeleteTableContextCheckBox.Font")));
            this.DeleteTableContextCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("DeleteTableContextCheckBox.Image")));
            this.DeleteTableContextCheckBox.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteTableContextCheckBox.ImageAlign")));
            this.DeleteTableContextCheckBox.ImageIndex = ((int)(resources.GetObject("DeleteTableContextCheckBox.ImageIndex")));
            this.DeleteTableContextCheckBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DeleteTableContextCheckBox.ImeMode")));
            this.DeleteTableContextCheckBox.Location = ((System.Drawing.Point)(resources.GetObject("DeleteTableContextCheckBox.Location")));
            this.DeleteTableContextCheckBox.Name = "DeleteTableContextCheckBox";
            this.DeleteTableContextCheckBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DeleteTableContextCheckBox.RightToLeft")));
            this.DeleteTableContextCheckBox.Size = ((System.Drawing.Size)(resources.GetObject("DeleteTableContextCheckBox.Size")));
            this.DeleteTableContextCheckBox.TabIndex = ((int)(resources.GetObject("DeleteTableContextCheckBox.TabIndex")));
            this.DeleteTableContextCheckBox.Text = resources.GetString("DeleteTableContextCheckBox.Text");
            this.DeleteTableContextCheckBox.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("DeleteTableContextCheckBox.TextAlign")));
            this.DeleteTableContextCheckBox.Visible = ((bool)(resources.GetObject("DeleteTableContextCheckBox.Visible")));
            this.DeleteTableContextCheckBox.CheckedChanged += new System.EventHandler(this.DeleteTableContextCheckBox_CheckedChanged);
            // 
            // ImportParamsPage
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.ParamDetailsGroupBox);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "ImportParamsPage";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.Controls.SetChildIndex(this.ParamDetailsGroupBox, 0);
            this.Controls.SetChildIndex(this.m_headerPanel, 0);
            this.ParamDetailsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
