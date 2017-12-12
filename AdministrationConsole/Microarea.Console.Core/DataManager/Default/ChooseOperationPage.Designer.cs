
using Microarea.TaskBuilderNet.UI.WinControls.Combo;
namespace Microarea.Console.Core.DataManager.Default
{
    partial class ChooseOperationPage
    {
        private DataManagerISOCombo IsoStateComboBox;
        private System.Windows.Forms.GroupBox ChooseGroupBox;
        private System.Windows.Forms.RadioButton ExportRadioButton;
        private System.Windows.Forms.Label SampleConfigLabel;
        private System.Windows.Forms.RadioButton ImportOptionalRadioButton;
        private System.Windows.Forms.RadioButton ImportRadioButton;
        private System.Windows.Forms.Label IsoStateLabel;
        private System.Windows.Forms.CheckBox LoadFromFileCheckBox;
        private System.Windows.Forms.TextBox PathConfigFileTextBox;
        private System.Windows.Forms.Button LoadConfigFileButton;
        private System.Windows.Forms.ComboBox ConfigurationComboBox;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseOperationPage));
			this.ChooseGroupBox = new System.Windows.Forms.GroupBox();
			this.LoadConfigFileButton = new System.Windows.Forms.Button();
			this.PathConfigFileTextBox = new System.Windows.Forms.TextBox();
			this.LoadFromFileCheckBox = new System.Windows.Forms.CheckBox();
			this.ConfigurationComboBox = new System.Windows.Forms.ComboBox();
			this.IsoStateLabel = new System.Windows.Forms.Label();
			this.IsoStateComboBox = new Microarea.TaskBuilderNet.UI.WinControls.Combo.DataManagerISOCombo();
			this.ExportRadioButton = new System.Windows.Forms.RadioButton();
			this.SampleConfigLabel = new System.Windows.Forms.Label();
			this.ImportOptionalRadioButton = new System.Windows.Forms.RadioButton();
			this.ImportRadioButton = new System.Windows.Forms.RadioButton();
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
			this.ChooseGroupBox.Controls.Add(this.LoadConfigFileButton);
			this.ChooseGroupBox.Controls.Add(this.PathConfigFileTextBox);
			this.ChooseGroupBox.Controls.Add(this.LoadFromFileCheckBox);
			this.ChooseGroupBox.Controls.Add(this.ConfigurationComboBox);
			this.ChooseGroupBox.Controls.Add(this.IsoStateLabel);
			this.ChooseGroupBox.Controls.Add(this.IsoStateComboBox);
			this.ChooseGroupBox.Controls.Add(this.ExportRadioButton);
			this.ChooseGroupBox.Controls.Add(this.SampleConfigLabel);
			this.ChooseGroupBox.Controls.Add(this.ImportOptionalRadioButton);
			this.ChooseGroupBox.Controls.Add(this.ImportRadioButton);
			this.ChooseGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			resources.ApplyResources(this.ChooseGroupBox, "ChooseGroupBox");
			this.ChooseGroupBox.Name = "ChooseGroupBox";
			this.ChooseGroupBox.TabStop = false;
			// 
			// LoadConfigFileButton
			// 
			resources.ApplyResources(this.LoadConfigFileButton, "LoadConfigFileButton");
			this.LoadConfigFileButton.Name = "LoadConfigFileButton";
			this.LoadConfigFileButton.Click += new System.EventHandler(this.LoadConfigFileButton_Click);
			// 
			// PathConfigFileTextBox
			// 
			resources.ApplyResources(this.PathConfigFileTextBox, "PathConfigFileTextBox");
			this.PathConfigFileTextBox.Name = "PathConfigFileTextBox";
			this.PathConfigFileTextBox.Leave += new System.EventHandler(this.PathConfigFileTextBox_Leave);
			// 
			// LoadFromFileCheckBox
			// 
			resources.ApplyResources(this.LoadFromFileCheckBox, "LoadFromFileCheckBox");
			this.LoadFromFileCheckBox.Name = "LoadFromFileCheckBox";
			this.LoadFromFileCheckBox.CheckedChanged += new System.EventHandler(this.LoadFromFileCheckBox_CheckedChanged);
			// 
			// ConfigurationComboBox
			// 
			this.ConfigurationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.ConfigurationComboBox, "ConfigurationComboBox");
			this.ConfigurationComboBox.Name = "ConfigurationComboBox";
			this.ConfigurationComboBox.Sorted = true;
			// 
			// IsoStateLabel
			// 
			resources.ApplyResources(this.IsoStateLabel, "IsoStateLabel");
			this.IsoStateLabel.Name = "IsoStateLabel";
			// 
			// IsoStateComboBox
			// 
			this.IsoStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			resources.ApplyResources(this.IsoStateComboBox, "IsoStateComboBox");
			this.IsoStateComboBox.Name = "IsoStateComboBox";
			this.IsoStateComboBox.SelectedIndexChanged += new System.EventHandler(this.IsoStateComboBox_SelectedIndexChanged);
			// 
			// ExportRadioButton
			// 
			resources.ApplyResources(this.ExportRadioButton, "ExportRadioButton");
			this.ExportRadioButton.Name = "ExportRadioButton";
			this.ExportRadioButton.CheckedChanged += new System.EventHandler(this.ExportRadioButton_CheckedChanged);
			// 
			// SampleConfigLabel
			// 
			resources.ApplyResources(this.SampleConfigLabel, "SampleConfigLabel");
			this.SampleConfigLabel.Name = "SampleConfigLabel";
			// 
			// ImportOptionalRadioButton
			// 
			resources.ApplyResources(this.ImportOptionalRadioButton, "ImportOptionalRadioButton");
			this.ImportOptionalRadioButton.Name = "ImportOptionalRadioButton";
			this.ImportOptionalRadioButton.CheckedChanged += new System.EventHandler(this.ImportOptionalRadioButton_CheckedChanged);
			// 
			// ImportRadioButton
			// 
			resources.ApplyResources(this.ImportRadioButton, "ImportRadioButton");
			this.ImportRadioButton.Name = "ImportRadioButton";
			this.ImportRadioButton.CheckedChanged += new System.EventHandler(this.ImportRadioButton_CheckedChanged);
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
