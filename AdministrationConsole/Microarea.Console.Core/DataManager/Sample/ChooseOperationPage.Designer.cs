
using Microarea.TaskBuilderNet.UI.WinControls.Combo;
namespace Microarea.Console.Core.DataManager.Sample
{
    partial class ChooseOperationPage
    {
        private System.Windows.Forms.Label LanguageLabel;
        private System.Windows.Forms.Label SampleConfigLabel;
        private System.Windows.Forms.RadioButton ImportRadioButton;
        private System.Windows.Forms.RadioButton ExportRadioButton;
        private DataManagerISOCombo IsoStateComboBox;
        private System.Windows.Forms.ComboBox ConfigurationComboBox;
        private System.Windows.Forms.GroupBox ChooseGroupBox;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseOperationPage));
			this.ChooseGroupBox = new System.Windows.Forms.GroupBox();
			this.ConfigurationComboBox = new System.Windows.Forms.ComboBox();
			this.IsoStateComboBox = new Microarea.TaskBuilderNet.UI.WinControls.Combo.DataManagerISOCombo();
			this.LanguageLabel = new System.Windows.Forms.Label();
			this.SampleConfigLabel = new System.Windows.Forms.Label();
			this.ImportRadioButton = new System.Windows.Forms.RadioButton();
			this.ExportRadioButton = new System.Windows.Forms.RadioButton();
			this.m_headerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).BeginInit();
			this.ChooseGroupBox.SuspendLayout();
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
			// ChooseGroupBox
			// 
			this.ChooseGroupBox.Controls.Add(this.ConfigurationComboBox);
			this.ChooseGroupBox.Controls.Add(this.IsoStateComboBox);
			this.ChooseGroupBox.Controls.Add(this.LanguageLabel);
			this.ChooseGroupBox.Controls.Add(this.SampleConfigLabel);
			this.ChooseGroupBox.Controls.Add(this.ImportRadioButton);
			this.ChooseGroupBox.Controls.Add(this.ExportRadioButton);
			this.ChooseGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.ChooseGroupBox, "ChooseGroupBox");
			this.ChooseGroupBox.Name = "ChooseGroupBox";
			this.ChooseGroupBox.TabStop = false;
			// 
			// ConfigurationComboBox
			// 
			resources.ApplyResources(this.ConfigurationComboBox, "ConfigurationComboBox");
			this.ConfigurationComboBox.Name = "ConfigurationComboBox";
			this.ConfigurationComboBox.SelectedIndexChanged += new System.EventHandler(this.ConfigurationComboBox_SelectedIndexChanged);
			// 
			// IsoStateComboBox
			// 
			this.IsoStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.IsoStateComboBox, "IsoStateComboBox");
			this.IsoStateComboBox.Name = "IsoStateComboBox";
			this.IsoStateComboBox.SelectedIndexChanged += new System.EventHandler(this.IsoStateComboBox_SelectedIndexChanged);
			// 
			// LanguageLabel
			// 
			resources.ApplyResources(this.LanguageLabel, "LanguageLabel");
			this.LanguageLabel.Name = "LanguageLabel";
			// 
			// SampleConfigLabel
			// 
			resources.ApplyResources(this.SampleConfigLabel, "SampleConfigLabel");
			this.SampleConfigLabel.Name = "SampleConfigLabel";
			// 
			// ImportRadioButton
			// 
			resources.ApplyResources(this.ImportRadioButton, "ImportRadioButton");
			this.ImportRadioButton.Name = "ImportRadioButton";
			this.ImportRadioButton.CheckedChanged += new System.EventHandler(this.ImportRadioButton_CheckedChanged);
			// 
			// ExportRadioButton
			// 
			resources.ApplyResources(this.ExportRadioButton, "ExportRadioButton");
			this.ExportRadioButton.Name = "ExportRadioButton";
			this.ExportRadioButton.CheckedChanged += new System.EventHandler(this.ExportRadioButton_CheckedChanged);
			// 
			// ChooseOperationPage
			// 
			this.Controls.Add(this.ChooseGroupBox);
			this.Name = "ChooseOperationPage";
			resources.ApplyResources(this, "$this");
			this.Load += new System.EventHandler(this.ChooseOperationPage_Load);
			this.Controls.SetChildIndex(this.ChooseGroupBox, 0);
			this.Controls.SetChildIndex(this.m_headerPanel, 0);
			this.m_headerPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.m_headerPicture)).EndInit();
			this.ChooseGroupBox.ResumeLayout(false);
			this.ChooseGroupBox.PerformLayout();
			this.ResumeLayout(false);

        }
        #endregion		

    }
}
