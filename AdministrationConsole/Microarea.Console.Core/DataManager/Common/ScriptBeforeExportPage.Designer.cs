
namespace Microarea.Console.Core.DataManager.Common
{
    partial class ScriptBeforeExportPage
    {
        private System.Windows.Forms.Button SyntaxCheckButton;
        private System.Windows.Forms.TextBox ScriptTextBox;
        private System.Windows.Forms.CheckBox ExecuteScriptCheckBox;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ScriptBeforeExportPage));
            this.SyntaxCheckButton = new System.Windows.Forms.Button();
            this.ScriptTextBox = new System.Windows.Forms.TextBox();
            this.ExecuteScriptCheckBox = new System.Windows.Forms.CheckBox();
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
            // SyntaxCheckButton
            // 
            this.SyntaxCheckButton.AccessibleDescription = resources.GetString("SyntaxCheckButton.AccessibleDescription");
            this.SyntaxCheckButton.AccessibleName = resources.GetString("SyntaxCheckButton.AccessibleName");
            this.SyntaxCheckButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("SyntaxCheckButton.Anchor")));
            this.SyntaxCheckButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SyntaxCheckButton.BackgroundImage")));
            this.SyntaxCheckButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("SyntaxCheckButton.Dock")));
            this.SyntaxCheckButton.Enabled = ((bool)(resources.GetObject("SyntaxCheckButton.Enabled")));
            this.SyntaxCheckButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("SyntaxCheckButton.FlatStyle")));
            this.SyntaxCheckButton.Font = ((System.Drawing.Font)(resources.GetObject("SyntaxCheckButton.Font")));
            this.SyntaxCheckButton.Image = ((System.Drawing.Image)(resources.GetObject("SyntaxCheckButton.Image")));
            this.SyntaxCheckButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SyntaxCheckButton.ImageAlign")));
            this.SyntaxCheckButton.ImageIndex = ((int)(resources.GetObject("SyntaxCheckButton.ImageIndex")));
            this.SyntaxCheckButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("SyntaxCheckButton.ImeMode")));
            this.SyntaxCheckButton.Location = ((System.Drawing.Point)(resources.GetObject("SyntaxCheckButton.Location")));
            this.SyntaxCheckButton.Name = "SyntaxCheckButton";
            this.SyntaxCheckButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("SyntaxCheckButton.RightToLeft")));
            this.SyntaxCheckButton.Size = ((System.Drawing.Size)(resources.GetObject("SyntaxCheckButton.Size")));
            this.SyntaxCheckButton.TabIndex = ((int)(resources.GetObject("SyntaxCheckButton.TabIndex")));
            this.SyntaxCheckButton.Text = resources.GetString("SyntaxCheckButton.Text");
            this.SyntaxCheckButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("SyntaxCheckButton.TextAlign")));
            this.SyntaxCheckButton.Visible = ((bool)(resources.GetObject("SyntaxCheckButton.Visible")));
            this.SyntaxCheckButton.Click += new System.EventHandler(this.SyntaxCheckButton_Click);
            // 
            // ScriptTextBox
            // 
            this.ScriptTextBox.AcceptsReturn = true;
            this.ScriptTextBox.AccessibleDescription = resources.GetString("ScriptTextBox.AccessibleDescription");
            this.ScriptTextBox.AccessibleName = resources.GetString("ScriptTextBox.AccessibleName");
            this.ScriptTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ScriptTextBox.Anchor")));
            this.ScriptTextBox.AutoSize = ((bool)(resources.GetObject("ScriptTextBox.AutoSize")));
            this.ScriptTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ScriptTextBox.BackgroundImage")));
            this.ScriptTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ScriptTextBox.Dock")));
            this.ScriptTextBox.Enabled = ((bool)(resources.GetObject("ScriptTextBox.Enabled")));
            this.ScriptTextBox.Font = ((System.Drawing.Font)(resources.GetObject("ScriptTextBox.Font")));
            this.ScriptTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ScriptTextBox.ImeMode")));
            this.ScriptTextBox.Location = ((System.Drawing.Point)(resources.GetObject("ScriptTextBox.Location")));
            this.ScriptTextBox.MaxLength = ((int)(resources.GetObject("ScriptTextBox.MaxLength")));
            this.ScriptTextBox.Multiline = ((bool)(resources.GetObject("ScriptTextBox.Multiline")));
            this.ScriptTextBox.Name = "ScriptTextBox";
            this.ScriptTextBox.PasswordChar = ((char)(resources.GetObject("ScriptTextBox.PasswordChar")));
            this.ScriptTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ScriptTextBox.RightToLeft")));
            this.ScriptTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ScriptTextBox.ScrollBars")));
            this.ScriptTextBox.Size = ((System.Drawing.Size)(resources.GetObject("ScriptTextBox.Size")));
            this.ScriptTextBox.TabIndex = ((int)(resources.GetObject("ScriptTextBox.TabIndex")));
            this.ScriptTextBox.Text = resources.GetString("ScriptTextBox.Text");
            this.ScriptTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ScriptTextBox.TextAlign")));
            this.ScriptTextBox.Visible = ((bool)(resources.GetObject("ScriptTextBox.Visible")));
            this.ScriptTextBox.WordWrap = ((bool)(resources.GetObject("ScriptTextBox.WordWrap")));
            this.ScriptTextBox.TextChanged += new System.EventHandler(this.ScriptTextBox_TextChanged);
            this.ScriptTextBox.Leave += new System.EventHandler(this.ScriptTextBox_Leave);
            // 
            // ExecuteScriptCheckBox
            // 
            this.ExecuteScriptCheckBox.AccessibleDescription = resources.GetString("ExecuteScriptCheckBox.AccessibleDescription");
            this.ExecuteScriptCheckBox.AccessibleName = resources.GetString("ExecuteScriptCheckBox.AccessibleName");
            this.ExecuteScriptCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExecuteScriptCheckBox.Anchor")));
            this.ExecuteScriptCheckBox.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("ExecuteScriptCheckBox.Appearance")));
            this.ExecuteScriptCheckBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ExecuteScriptCheckBox.BackgroundImage")));
            this.ExecuteScriptCheckBox.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ExecuteScriptCheckBox.CheckAlign")));
            this.ExecuteScriptCheckBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ExecuteScriptCheckBox.Dock")));
            this.ExecuteScriptCheckBox.Enabled = ((bool)(resources.GetObject("ExecuteScriptCheckBox.Enabled")));
            this.ExecuteScriptCheckBox.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ExecuteScriptCheckBox.FlatStyle")));
            this.ExecuteScriptCheckBox.Font = ((System.Drawing.Font)(resources.GetObject("ExecuteScriptCheckBox.Font")));
            this.ExecuteScriptCheckBox.Image = ((System.Drawing.Image)(resources.GetObject("ExecuteScriptCheckBox.Image")));
            this.ExecuteScriptCheckBox.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ExecuteScriptCheckBox.ImageAlign")));
            this.ExecuteScriptCheckBox.ImageIndex = ((int)(resources.GetObject("ExecuteScriptCheckBox.ImageIndex")));
            this.ExecuteScriptCheckBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ExecuteScriptCheckBox.ImeMode")));
            this.ExecuteScriptCheckBox.Location = ((System.Drawing.Point)(resources.GetObject("ExecuteScriptCheckBox.Location")));
            this.ExecuteScriptCheckBox.Name = "ExecuteScriptCheckBox";
            this.ExecuteScriptCheckBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ExecuteScriptCheckBox.RightToLeft")));
            this.ExecuteScriptCheckBox.Size = ((System.Drawing.Size)(resources.GetObject("ExecuteScriptCheckBox.Size")));
            this.ExecuteScriptCheckBox.TabIndex = ((int)(resources.GetObject("ExecuteScriptCheckBox.TabIndex")));
            this.ExecuteScriptCheckBox.Text = resources.GetString("ExecuteScriptCheckBox.Text");
            this.ExecuteScriptCheckBox.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ExecuteScriptCheckBox.TextAlign")));
            this.ExecuteScriptCheckBox.Visible = ((bool)(resources.GetObject("ExecuteScriptCheckBox.Visible")));
            this.ExecuteScriptCheckBox.CheckedChanged += new System.EventHandler(this.ExecuteScriptCheckBox_CheckedChanged);
            // 
            // ScriptBeforeExportPage
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.ExecuteScriptCheckBox);
            this.Controls.Add(this.ScriptTextBox);
            this.Controls.Add(this.SyntaxCheckButton);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "ScriptBeforeExportPage";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.Controls.SetChildIndex(this.SyntaxCheckButton, 0);
            this.Controls.SetChildIndex(this.ScriptTextBox, 0);
            this.Controls.SetChildIndex(this.ExecuteScriptCheckBox, 0);
            this.Controls.SetChildIndex(this.m_headerPanel, 0);
            this.ResumeLayout(false);

        }
        #endregion

    }
}
