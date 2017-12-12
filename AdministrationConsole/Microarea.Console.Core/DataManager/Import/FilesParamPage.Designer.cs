
namespace Microarea.Console.Core.DataManager.Import
{
    partial class FilesParamPage
	{
		private System.Windows.Forms.ComboBox CompaniesComboBox;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilesParamPage));
			this.CompaniesComboBox = new System.Windows.Forms.ComboBox();
			this.FindSourceGroupBox = new System.Windows.Forms.GroupBox();
			this.BrowseButton = new System.Windows.Forms.Button();
			this.PathTextBox = new System.Windows.Forms.TextBox();
			this.BrowseRadioButton = new System.Windows.Forms.RadioButton();
			this.SelectCompanyRadioButton = new System.Windows.Forms.RadioButton();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			this.FindSourceGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_headerPanel
			// 
			resources.ApplyResources(this.m_headerPanel, "m_headerPanel");
			// 
			// m_headerPicture
			// 
			resources.ApplyResources(this.m_headerPicture, "m_headerPicture");
			// 
			// m_titleLabel
			// 
			resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
			// 
			// m_subtitleLabel
			// 
			resources.ApplyResources(this.m_subtitleLabel, "m_subtitleLabel");
			// 
			// CompaniesComboBox
			// 
			this.CompaniesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.CompaniesComboBox, "CompaniesComboBox");
			this.CompaniesComboBox.Name = "CompaniesComboBox";
			this.CompaniesComboBox.Sorted = true;
			this.CompaniesComboBox.SelectedIndexChanged += new System.EventHandler(this.CompaniesComboBox_SelectedIndexChanged);
			// 
			// FindSourceGroupBox
			// 
			this.FindSourceGroupBox.Controls.Add(this.BrowseButton);
			this.FindSourceGroupBox.Controls.Add(this.PathTextBox);
			this.FindSourceGroupBox.Controls.Add(this.BrowseRadioButton);
			this.FindSourceGroupBox.Controls.Add(this.SelectCompanyRadioButton);
			this.FindSourceGroupBox.Controls.Add(this.CompaniesComboBox);
			this.FindSourceGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.FindSourceGroupBox, "FindSourceGroupBox");
			this.FindSourceGroupBox.Name = "FindSourceGroupBox";
			this.FindSourceGroupBox.TabStop = false;
			// 
			// BrowseButton
			// 
			resources.ApplyResources(this.BrowseButton, "BrowseButton");
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// PathTextBox
			// 
			resources.ApplyResources(this.PathTextBox, "PathTextBox");
			this.PathTextBox.Name = "PathTextBox";
			this.PathTextBox.Leave += new System.EventHandler(this.PathTextBox_Leave);
			// 
			// BrowseRadioButton
			// 
			resources.ApplyResources(this.BrowseRadioButton, "BrowseRadioButton");
			this.BrowseRadioButton.Name = "BrowseRadioButton";
			this.BrowseRadioButton.TabStop = true;
			this.BrowseRadioButton.UseVisualStyleBackColor = true;
			// 
			// SelectCompanyRadioButton
			// 
			resources.ApplyResources(this.SelectCompanyRadioButton, "SelectCompanyRadioButton");
			this.SelectCompanyRadioButton.Name = "SelectCompanyRadioButton";
			this.SelectCompanyRadioButton.TabStop = true;
			this.SelectCompanyRadioButton.UseVisualStyleBackColor = true;
			this.SelectCompanyRadioButton.CheckedChanged += new System.EventHandler(this.SelectCompanyRadioButton_CheckedChanged);
			// 
			// FilesParamPage
			// 
			this.Controls.Add(this.FindSourceGroupBox);
			this.Name = "FilesParamPage";
			resources.ApplyResources(this, "$this");
			this.Load += new System.EventHandler(this.FilesParamPage_Load);
			this.Controls.SetChildIndex(this.FindSourceGroupBox, 0);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			this.FindSourceGroupBox.ResumeLayout(false);
			this.FindSourceGroupBox.PerformLayout();
			this.ResumeLayout(false);

        }
        #endregion

		private System.Windows.Forms.GroupBox FindSourceGroupBox;
		private System.Windows.Forms.RadioButton BrowseRadioButton;
		private System.Windows.Forms.RadioButton SelectCompanyRadioButton;
		private System.Windows.Forms.TextBox PathTextBox;
		private System.Windows.Forms.Button BrowseButton;

    }
}
