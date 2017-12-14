
namespace Microarea.Console.Core.RegressionTestLibrary.WizardPages
{
    partial class FoldersSelectionPage
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox RepositoryGroupBox;
        private System.Windows.Forms.GroupBox WinZipGroupBox;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.TextBox RepositoryPathTextBox;
        private System.Windows.Forms.TextBox WinZipPathTextBox;
        private System.Windows.Forms.Button RepositoryPathButton;
        private System.Windows.Forms.Button WinZipPathButton;
        private System.Windows.Forms.GroupBox ExtraUpdateGroupBox;
        private System.Windows.Forms.Button ExtraUpdatePathButton;
        private System.Windows.Forms.TextBox ExtraUpdatePathTextBox;
        private System.Windows.Forms.OpenFileDialog ExtraUpdateFileDialog;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------------	
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FoldersSelectionPage));
            this.RepositoryGroupBox = new System.Windows.Forms.GroupBox();
            this.RepositoryPathButton = new System.Windows.Forms.Button();
            this.RepositoryPathTextBox = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.WinZipGroupBox = new System.Windows.Forms.GroupBox();
            this.WinZipPathButton = new System.Windows.Forms.Button();
            this.WinZipPathTextBox = new System.Windows.Forms.TextBox();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.ExtraUpdateGroupBox = new System.Windows.Forms.GroupBox();
            this.ExtraUpdatePathButton = new System.Windows.Forms.Button();
            this.ExtraUpdatePathTextBox = new System.Windows.Forms.TextBox();
            this.ExtraUpdateFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.m_headerPanel.SuspendLayout();
            this.RepositoryGroupBox.SuspendLayout();
            this.WinZipGroupBox.SuspendLayout();
            this.ExtraUpdateGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_headerPanel
            // 
            this.m_headerPanel.Location = ((System.Drawing.Point)(resources.GetObject("m_headerPanel.Location")));
            this.m_headerPanel.Name = "m_headerPanel";
            this.m_headerPanel.Size = ((System.Drawing.Size)(resources.GetObject("m_headerPanel.Size")));
            // 
            // m_headerPicture
            // 
            this.m_headerPicture.Location = ((System.Drawing.Point)(resources.GetObject("m_headerPicture.Location")));
            this.m_headerPicture.Name = "m_headerPicture";
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
            this.m_subtitleLabel.Location = ((System.Drawing.Point)(resources.GetObject("m_subtitleLabel.Location")));
            this.m_subtitleLabel.Name = "m_subtitleLabel";
            this.m_subtitleLabel.Size = ((System.Drawing.Size)(resources.GetObject("m_subtitleLabel.Size")));
            this.m_subtitleLabel.Text = resources.GetString("m_subtitleLabel.Text");
            // 
            // RepositoryGroupBox
            // 
            this.RepositoryGroupBox.AccessibleDescription = resources.GetString("RepositoryGroupBox.AccessibleDescription");
            this.RepositoryGroupBox.AccessibleName = resources.GetString("RepositoryGroupBox.AccessibleName");
            this.RepositoryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RepositoryGroupBox.Anchor")));
            this.RepositoryGroupBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RepositoryGroupBox.BackgroundImage")));
            this.RepositoryGroupBox.Controls.Add(this.RepositoryPathButton);
            this.RepositoryGroupBox.Controls.Add(this.RepositoryPathTextBox);
            this.RepositoryGroupBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RepositoryGroupBox.Dock")));
            this.RepositoryGroupBox.Enabled = ((bool)(resources.GetObject("RepositoryGroupBox.Enabled")));
            this.RepositoryGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RepositoryGroupBox.Font = ((System.Drawing.Font)(resources.GetObject("RepositoryGroupBox.Font")));
            this.RepositoryGroupBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RepositoryGroupBox.ImeMode")));
            this.RepositoryGroupBox.Location = ((System.Drawing.Point)(resources.GetObject("RepositoryGroupBox.Location")));
            this.RepositoryGroupBox.Name = "RepositoryGroupBox";
            this.RepositoryGroupBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RepositoryGroupBox.RightToLeft")));
            this.RepositoryGroupBox.Size = ((System.Drawing.Size)(resources.GetObject("RepositoryGroupBox.Size")));
            this.RepositoryGroupBox.TabIndex = ((int)(resources.GetObject("RepositoryGroupBox.TabIndex")));
            this.RepositoryGroupBox.TabStop = false;
            this.RepositoryGroupBox.Text = resources.GetString("RepositoryGroupBox.Text");
            this.RepositoryGroupBox.Visible = ((bool)(resources.GetObject("RepositoryGroupBox.Visible")));
            // 
            // RepositoryPathButton
            // 
            this.RepositoryPathButton.AccessibleDescription = resources.GetString("RepositoryPathButton.AccessibleDescription");
            this.RepositoryPathButton.AccessibleName = resources.GetString("RepositoryPathButton.AccessibleName");
            this.RepositoryPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RepositoryPathButton.Anchor")));
            this.RepositoryPathButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RepositoryPathButton.BackgroundImage")));
            this.RepositoryPathButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RepositoryPathButton.Dock")));
            this.RepositoryPathButton.Enabled = ((bool)(resources.GetObject("RepositoryPathButton.Enabled")));
            this.RepositoryPathButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("RepositoryPathButton.FlatStyle")));
            this.RepositoryPathButton.Font = ((System.Drawing.Font)(resources.GetObject("RepositoryPathButton.Font")));
            this.RepositoryPathButton.Image = ((System.Drawing.Image)(resources.GetObject("RepositoryPathButton.Image")));
            this.RepositoryPathButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RepositoryPathButton.ImageAlign")));
            this.RepositoryPathButton.ImageIndex = ((int)(resources.GetObject("RepositoryPathButton.ImageIndex")));
            this.RepositoryPathButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RepositoryPathButton.ImeMode")));
            this.RepositoryPathButton.Location = ((System.Drawing.Point)(resources.GetObject("RepositoryPathButton.Location")));
            this.RepositoryPathButton.Name = "RepositoryPathButton";
            this.RepositoryPathButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RepositoryPathButton.RightToLeft")));
            this.RepositoryPathButton.Size = ((System.Drawing.Size)(resources.GetObject("RepositoryPathButton.Size")));
            this.RepositoryPathButton.TabIndex = ((int)(resources.GetObject("RepositoryPathButton.TabIndex")));
            this.RepositoryPathButton.Text = resources.GetString("RepositoryPathButton.Text");
            this.RepositoryPathButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("RepositoryPathButton.TextAlign")));
            this.RepositoryPathButton.Visible = ((bool)(resources.GetObject("RepositoryPathButton.Visible")));
            this.RepositoryPathButton.Click += new System.EventHandler(this.RepositoryPathButton_Click);
            // 
            // RepositoryPathTextBox
            // 
            this.RepositoryPathTextBox.AccessibleDescription = resources.GetString("RepositoryPathTextBox.AccessibleDescription");
            this.RepositoryPathTextBox.AccessibleName = resources.GetString("RepositoryPathTextBox.AccessibleName");
            this.RepositoryPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RepositoryPathTextBox.Anchor")));
            this.RepositoryPathTextBox.AutoSize = ((bool)(resources.GetObject("RepositoryPathTextBox.AutoSize")));
            this.RepositoryPathTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RepositoryPathTextBox.BackgroundImage")));
            this.RepositoryPathTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RepositoryPathTextBox.Dock")));
            this.RepositoryPathTextBox.Enabled = ((bool)(resources.GetObject("RepositoryPathTextBox.Enabled")));
            this.RepositoryPathTextBox.Font = ((System.Drawing.Font)(resources.GetObject("RepositoryPathTextBox.Font")));
            this.RepositoryPathTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RepositoryPathTextBox.ImeMode")));
            this.RepositoryPathTextBox.Location = ((System.Drawing.Point)(resources.GetObject("RepositoryPathTextBox.Location")));
            this.RepositoryPathTextBox.MaxLength = ((int)(resources.GetObject("RepositoryPathTextBox.MaxLength")));
            this.RepositoryPathTextBox.Multiline = ((bool)(resources.GetObject("RepositoryPathTextBox.Multiline")));
            this.RepositoryPathTextBox.Name = "RepositoryPathTextBox";
            this.RepositoryPathTextBox.PasswordChar = ((char)(resources.GetObject("RepositoryPathTextBox.PasswordChar")));
            this.RepositoryPathTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RepositoryPathTextBox.RightToLeft")));
            this.RepositoryPathTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("RepositoryPathTextBox.ScrollBars")));
            this.RepositoryPathTextBox.Size = ((System.Drawing.Size)(resources.GetObject("RepositoryPathTextBox.Size")));
            this.RepositoryPathTextBox.TabIndex = ((int)(resources.GetObject("RepositoryPathTextBox.TabIndex")));
            this.RepositoryPathTextBox.Text = resources.GetString("RepositoryPathTextBox.Text");
            this.RepositoryPathTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("RepositoryPathTextBox.TextAlign")));
            this.RepositoryPathTextBox.Visible = ((bool)(resources.GetObject("RepositoryPathTextBox.Visible")));
            this.RepositoryPathTextBox.WordWrap = ((bool)(resources.GetObject("RepositoryPathTextBox.WordWrap")));
            // 
            // pictureBox1
            // 
            this.pictureBox1.AccessibleDescription = resources.GetString("pictureBox1.AccessibleDescription");
            this.pictureBox1.AccessibleName = resources.GetString("pictureBox1.AccessibleName");
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureBox1.Anchor")));
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureBox1.Dock")));
            this.pictureBox1.Enabled = ((bool)(resources.GetObject("pictureBox1.Enabled")));
            this.pictureBox1.Font = ((System.Drawing.Font)(resources.GetObject("pictureBox1.Font")));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureBox1.ImeMode")));
            this.pictureBox1.Location = ((System.Drawing.Point)(resources.GetObject("pictureBox1.Location")));
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureBox1.RightToLeft")));
            this.pictureBox1.Size = ((System.Drawing.Size)(resources.GetObject("pictureBox1.Size")));
            this.pictureBox1.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureBox1.SizeMode")));
            this.pictureBox1.TabIndex = ((int)(resources.GetObject("pictureBox1.TabIndex")));
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Text = resources.GetString("pictureBox1.Text");
            this.pictureBox1.Visible = ((bool)(resources.GetObject("pictureBox1.Visible")));
            // 
            // WinZipGroupBox
            // 
            this.WinZipGroupBox.AccessibleDescription = resources.GetString("WinZipGroupBox.AccessibleDescription");
            this.WinZipGroupBox.AccessibleName = resources.GetString("WinZipGroupBox.AccessibleName");
            this.WinZipGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WinZipGroupBox.Anchor")));
            this.WinZipGroupBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WinZipGroupBox.BackgroundImage")));
            this.WinZipGroupBox.Controls.Add(this.WinZipPathButton);
            this.WinZipGroupBox.Controls.Add(this.WinZipPathTextBox);
            this.WinZipGroupBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WinZipGroupBox.Dock")));
            this.WinZipGroupBox.Enabled = ((bool)(resources.GetObject("WinZipGroupBox.Enabled")));
            this.WinZipGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.WinZipGroupBox.Font = ((System.Drawing.Font)(resources.GetObject("WinZipGroupBox.Font")));
            this.WinZipGroupBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WinZipGroupBox.ImeMode")));
            this.WinZipGroupBox.Location = ((System.Drawing.Point)(resources.GetObject("WinZipGroupBox.Location")));
            this.WinZipGroupBox.Name = "WinZipGroupBox";
            this.WinZipGroupBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WinZipGroupBox.RightToLeft")));
            this.WinZipGroupBox.Size = ((System.Drawing.Size)(resources.GetObject("WinZipGroupBox.Size")));
            this.WinZipGroupBox.TabIndex = ((int)(resources.GetObject("WinZipGroupBox.TabIndex")));
            this.WinZipGroupBox.TabStop = false;
            this.WinZipGroupBox.Text = resources.GetString("WinZipGroupBox.Text");
            this.WinZipGroupBox.Visible = ((bool)(resources.GetObject("WinZipGroupBox.Visible")));
            // 
            // WinZipPathButton
            // 
            this.WinZipPathButton.AccessibleDescription = resources.GetString("WinZipPathButton.AccessibleDescription");
            this.WinZipPathButton.AccessibleName = resources.GetString("WinZipPathButton.AccessibleName");
            this.WinZipPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WinZipPathButton.Anchor")));
            this.WinZipPathButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WinZipPathButton.BackgroundImage")));
            this.WinZipPathButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WinZipPathButton.Dock")));
            this.WinZipPathButton.Enabled = ((bool)(resources.GetObject("WinZipPathButton.Enabled")));
            this.WinZipPathButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("WinZipPathButton.FlatStyle")));
            this.WinZipPathButton.Font = ((System.Drawing.Font)(resources.GetObject("WinZipPathButton.Font")));
            this.WinZipPathButton.Image = ((System.Drawing.Image)(resources.GetObject("WinZipPathButton.Image")));
            this.WinZipPathButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("WinZipPathButton.ImageAlign")));
            this.WinZipPathButton.ImageIndex = ((int)(resources.GetObject("WinZipPathButton.ImageIndex")));
            this.WinZipPathButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WinZipPathButton.ImeMode")));
            this.WinZipPathButton.Location = ((System.Drawing.Point)(resources.GetObject("WinZipPathButton.Location")));
            this.WinZipPathButton.Name = "WinZipPathButton";
            this.WinZipPathButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WinZipPathButton.RightToLeft")));
            this.WinZipPathButton.Size = ((System.Drawing.Size)(resources.GetObject("WinZipPathButton.Size")));
            this.WinZipPathButton.TabIndex = ((int)(resources.GetObject("WinZipPathButton.TabIndex")));
            this.WinZipPathButton.Text = resources.GetString("WinZipPathButton.Text");
            this.WinZipPathButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("WinZipPathButton.TextAlign")));
            this.WinZipPathButton.Visible = ((bool)(resources.GetObject("WinZipPathButton.Visible")));
            this.WinZipPathButton.Click += new System.EventHandler(this.WinZipPathButton_Click);
            // 
            // WinZipPathTextBox
            // 
            this.WinZipPathTextBox.AccessibleDescription = resources.GetString("WinZipPathTextBox.AccessibleDescription");
            this.WinZipPathTextBox.AccessibleName = resources.GetString("WinZipPathTextBox.AccessibleName");
            this.WinZipPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("WinZipPathTextBox.Anchor")));
            this.WinZipPathTextBox.AutoSize = ((bool)(resources.GetObject("WinZipPathTextBox.AutoSize")));
            this.WinZipPathTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("WinZipPathTextBox.BackgroundImage")));
            this.WinZipPathTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("WinZipPathTextBox.Dock")));
            this.WinZipPathTextBox.Enabled = ((bool)(resources.GetObject("WinZipPathTextBox.Enabled")));
            this.WinZipPathTextBox.Font = ((System.Drawing.Font)(resources.GetObject("WinZipPathTextBox.Font")));
            this.WinZipPathTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("WinZipPathTextBox.ImeMode")));
            this.WinZipPathTextBox.Location = ((System.Drawing.Point)(resources.GetObject("WinZipPathTextBox.Location")));
            this.WinZipPathTextBox.MaxLength = ((int)(resources.GetObject("WinZipPathTextBox.MaxLength")));
            this.WinZipPathTextBox.Multiline = ((bool)(resources.GetObject("WinZipPathTextBox.Multiline")));
            this.WinZipPathTextBox.Name = "WinZipPathTextBox";
            this.WinZipPathTextBox.PasswordChar = ((char)(resources.GetObject("WinZipPathTextBox.PasswordChar")));
            this.WinZipPathTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("WinZipPathTextBox.RightToLeft")));
            this.WinZipPathTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("WinZipPathTextBox.ScrollBars")));
            this.WinZipPathTextBox.Size = ((System.Drawing.Size)(resources.GetObject("WinZipPathTextBox.Size")));
            this.WinZipPathTextBox.TabIndex = ((int)(resources.GetObject("WinZipPathTextBox.TabIndex")));
            this.WinZipPathTextBox.Text = resources.GetString("WinZipPathTextBox.Text");
            this.WinZipPathTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("WinZipPathTextBox.TextAlign")));
            this.WinZipPathTextBox.Visible = ((bool)(resources.GetObject("WinZipPathTextBox.Visible")));
            this.WinZipPathTextBox.WordWrap = ((bool)(resources.GetObject("WinZipPathTextBox.WordWrap")));
            // 
            // FolderBrowserDialog
            // 
            this.FolderBrowserDialog.Description = resources.GetString("FolderBrowserDialog.Description");
            this.FolderBrowserDialog.SelectedPath = resources.GetString("FolderBrowserDialog.SelectedPath");
            // 
            // ExtraUpdateGroupBox
            // 
            this.ExtraUpdateGroupBox.AccessibleDescription = resources.GetString("ExtraUpdateGroupBox.AccessibleDescription");
            this.ExtraUpdateGroupBox.AccessibleName = resources.GetString("ExtraUpdateGroupBox.AccessibleName");
            this.ExtraUpdateGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExtraUpdateGroupBox.Anchor")));
            this.ExtraUpdateGroupBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ExtraUpdateGroupBox.BackgroundImage")));
            this.ExtraUpdateGroupBox.Controls.Add(this.ExtraUpdatePathButton);
            this.ExtraUpdateGroupBox.Controls.Add(this.ExtraUpdatePathTextBox);
            this.ExtraUpdateGroupBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ExtraUpdateGroupBox.Dock")));
            this.ExtraUpdateGroupBox.Enabled = ((bool)(resources.GetObject("ExtraUpdateGroupBox.Enabled")));
            this.ExtraUpdateGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ExtraUpdateGroupBox.Font = ((System.Drawing.Font)(resources.GetObject("ExtraUpdateGroupBox.Font")));
            this.ExtraUpdateGroupBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ExtraUpdateGroupBox.ImeMode")));
            this.ExtraUpdateGroupBox.Location = ((System.Drawing.Point)(resources.GetObject("ExtraUpdateGroupBox.Location")));
            this.ExtraUpdateGroupBox.Name = "ExtraUpdateGroupBox";
            this.ExtraUpdateGroupBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ExtraUpdateGroupBox.RightToLeft")));
            this.ExtraUpdateGroupBox.Size = ((System.Drawing.Size)(resources.GetObject("ExtraUpdateGroupBox.Size")));
            this.ExtraUpdateGroupBox.TabIndex = ((int)(resources.GetObject("ExtraUpdateGroupBox.TabIndex")));
            this.ExtraUpdateGroupBox.TabStop = false;
            this.ExtraUpdateGroupBox.Text = resources.GetString("ExtraUpdateGroupBox.Text");
            this.ExtraUpdateGroupBox.Visible = ((bool)(resources.GetObject("ExtraUpdateGroupBox.Visible")));
            // 
            // ExtraUpdatePathButton
            // 
            this.ExtraUpdatePathButton.AccessibleDescription = resources.GetString("ExtraUpdatePathButton.AccessibleDescription");
            this.ExtraUpdatePathButton.AccessibleName = resources.GetString("ExtraUpdatePathButton.AccessibleName");
            this.ExtraUpdatePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExtraUpdatePathButton.Anchor")));
            this.ExtraUpdatePathButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ExtraUpdatePathButton.BackgroundImage")));
            this.ExtraUpdatePathButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ExtraUpdatePathButton.Dock")));
            this.ExtraUpdatePathButton.Enabled = ((bool)(resources.GetObject("ExtraUpdatePathButton.Enabled")));
            this.ExtraUpdatePathButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ExtraUpdatePathButton.FlatStyle")));
            this.ExtraUpdatePathButton.Font = ((System.Drawing.Font)(resources.GetObject("ExtraUpdatePathButton.Font")));
            this.ExtraUpdatePathButton.Image = ((System.Drawing.Image)(resources.GetObject("ExtraUpdatePathButton.Image")));
            this.ExtraUpdatePathButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ExtraUpdatePathButton.ImageAlign")));
            this.ExtraUpdatePathButton.ImageIndex = ((int)(resources.GetObject("ExtraUpdatePathButton.ImageIndex")));
            this.ExtraUpdatePathButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ExtraUpdatePathButton.ImeMode")));
            this.ExtraUpdatePathButton.Location = ((System.Drawing.Point)(resources.GetObject("ExtraUpdatePathButton.Location")));
            this.ExtraUpdatePathButton.Name = "ExtraUpdatePathButton";
            this.ExtraUpdatePathButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ExtraUpdatePathButton.RightToLeft")));
            this.ExtraUpdatePathButton.Size = ((System.Drawing.Size)(resources.GetObject("ExtraUpdatePathButton.Size")));
            this.ExtraUpdatePathButton.TabIndex = ((int)(resources.GetObject("ExtraUpdatePathButton.TabIndex")));
            this.ExtraUpdatePathButton.Text = resources.GetString("ExtraUpdatePathButton.Text");
            this.ExtraUpdatePathButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ExtraUpdatePathButton.TextAlign")));
            this.ExtraUpdatePathButton.Visible = ((bool)(resources.GetObject("ExtraUpdatePathButton.Visible")));
            this.ExtraUpdatePathButton.Click += new System.EventHandler(this.ExtraUpdatePathButton_Click);
            // 
            // ExtraUpdatePathTextBox
            // 
            this.ExtraUpdatePathTextBox.AccessibleDescription = resources.GetString("ExtraUpdatePathTextBox.AccessibleDescription");
            this.ExtraUpdatePathTextBox.AccessibleName = resources.GetString("ExtraUpdatePathTextBox.AccessibleName");
            this.ExtraUpdatePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ExtraUpdatePathTextBox.Anchor")));
            this.ExtraUpdatePathTextBox.AutoSize = ((bool)(resources.GetObject("ExtraUpdatePathTextBox.AutoSize")));
            this.ExtraUpdatePathTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ExtraUpdatePathTextBox.BackgroundImage")));
            this.ExtraUpdatePathTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ExtraUpdatePathTextBox.Dock")));
            this.ExtraUpdatePathTextBox.Enabled = ((bool)(resources.GetObject("ExtraUpdatePathTextBox.Enabled")));
            this.ExtraUpdatePathTextBox.Font = ((System.Drawing.Font)(resources.GetObject("ExtraUpdatePathTextBox.Font")));
            this.ExtraUpdatePathTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ExtraUpdatePathTextBox.ImeMode")));
            this.ExtraUpdatePathTextBox.Location = ((System.Drawing.Point)(resources.GetObject("ExtraUpdatePathTextBox.Location")));
            this.ExtraUpdatePathTextBox.MaxLength = ((int)(resources.GetObject("ExtraUpdatePathTextBox.MaxLength")));
            this.ExtraUpdatePathTextBox.Multiline = ((bool)(resources.GetObject("ExtraUpdatePathTextBox.Multiline")));
            this.ExtraUpdatePathTextBox.Name = "ExtraUpdatePathTextBox";
            this.ExtraUpdatePathTextBox.PasswordChar = ((char)(resources.GetObject("ExtraUpdatePathTextBox.PasswordChar")));
            this.ExtraUpdatePathTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ExtraUpdatePathTextBox.RightToLeft")));
            this.ExtraUpdatePathTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("ExtraUpdatePathTextBox.ScrollBars")));
            this.ExtraUpdatePathTextBox.Size = ((System.Drawing.Size)(resources.GetObject("ExtraUpdatePathTextBox.Size")));
            this.ExtraUpdatePathTextBox.TabIndex = ((int)(resources.GetObject("ExtraUpdatePathTextBox.TabIndex")));
            this.ExtraUpdatePathTextBox.Text = resources.GetString("ExtraUpdatePathTextBox.Text");
            this.ExtraUpdatePathTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("ExtraUpdatePathTextBox.TextAlign")));
            this.ExtraUpdatePathTextBox.Visible = ((bool)(resources.GetObject("ExtraUpdatePathTextBox.Visible")));
            this.ExtraUpdatePathTextBox.WordWrap = ((bool)(resources.GetObject("ExtraUpdatePathTextBox.WordWrap")));
            // 
            // ExtraUpdateFileDialog
            // 
            this.ExtraUpdateFileDialog.DefaultExt = "sql";
            this.ExtraUpdateFileDialog.Filter = resources.GetString("ExtraUpdateFileDialog.Filter");
            this.ExtraUpdateFileDialog.Title = resources.GetString("ExtraUpdateFileDialog.Title");
            // 
            // FoldersSelectionPage
            // 
            this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
            this.AccessibleName = resources.GetString("$this.AccessibleName");
            this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
            this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
            this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.Controls.Add(this.WinZipGroupBox);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.RepositoryGroupBox);
            this.Controls.Add(this.ExtraUpdateGroupBox);
            this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
            this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
            this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
            this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
            this.Name = "FoldersSelectionPage";
            this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
            this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
            this.Controls.SetChildIndex(this.ExtraUpdateGroupBox, 0);
            this.Controls.SetChildIndex(this.RepositoryGroupBox, 0);
            this.Controls.SetChildIndex(this.pictureBox1, 0);
            this.Controls.SetChildIndex(this.m_headerPanel, 0);
            this.Controls.SetChildIndex(this.WinZipGroupBox, 0);
            this.m_headerPanel.ResumeLayout(false);
            this.RepositoryGroupBox.ResumeLayout(false);
            this.WinZipGroupBox.ResumeLayout(false);
            this.ExtraUpdateGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
