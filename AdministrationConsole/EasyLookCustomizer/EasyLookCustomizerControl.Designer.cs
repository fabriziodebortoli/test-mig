
namespace Microarea.Console.Plugin.EasyLookCustomizer
{
    partial class EasyLookCustomizerControl
    {
        private System.Windows.Forms.Panel CaptionPanel;
        private System.Windows.Forms.PictureBox EasyLookCustomizerPictureBox;
        private System.Windows.Forms.Label CaptionLabel;
        private System.Windows.Forms.Label DescriptionLabel;
        private System.Windows.Forms.TabControl CustomizerTabControl;
        private System.Windows.Forms.TabPage LayoutSettingsTabPage;
        private System.Windows.Forms.Label LogoImageURLLabel;
        private System.Windows.Forms.RichTextBox LogoImageURLRichTextBox;
        private System.Windows.Forms.Label AppPanelBkgndColorLabel;
		private Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker AppPanelBkgndColorWebColorPicker;
        private System.Windows.Forms.GroupBox GroupsPanelBkgndSettingsBox;
        private System.Windows.Forms.RadioButton GroupsPanelBkgndImageRadioButton;
        private System.Windows.Forms.RichTextBox GroupsPanelBkgndImageURLRichTextBox;
        private System.Windows.Forms.RadioButton GroupsPanelBkgndColorRadioButton;
		private Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker GroupsPanelBkgndColorWebColorPicker;
        private System.Windows.Forms.Label MenuTreeBkgndColorLabel;
		private Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker MenuTreeBkgndColorWebColorPicker;
        private System.Windows.Forms.Label CommandListBkgndColorLabel;
		private Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker CommandListBkgndColorWebColorPicker;
        private System.Windows.Forms.Label FontFamilyLabel;
        private Microarea.Console.Plugin.EasyLookCustomizer.FontSelectionComboBox FontFamilyComboBox;
        private System.Windows.Forms.GroupBox CustomReportsGroupBox;
        private System.Windows.Forms.Label AllUsersTitleColorLabel;
		private Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker AllUsersTitleColorWebColorPicker;
        private System.Windows.Forms.Label CurrentUserTitleColorLabel;
		private Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker CurrentUserTitleColorWebColorPicker;
        private System.Windows.Forms.Button ResetLayoutDefaultsButton;
        private System.Windows.Forms.TabPage ManageReporHistoryTabPage;
        private System.Windows.Forms.Label SnapshotHelpLabel;
        private System.Windows.Forms.PictureBox SnapshotPictureBox;
        private System.Windows.Forms.Label MaxWrmHistoryNumLabel;
        private System.Windows.Forms.NumericUpDown MaxWrmHistoryNumNumericUpDown;
        private System.Windows.Forms.Label WrmHistoryAutoDelCommentLabel;
        private System.Windows.Forms.CheckBox WrmHistoryAutoDelEnabledCheckBox;
        private System.Windows.Forms.Label WrmHistoryAutoDelOlderThanLabel;
        private System.Windows.Forms.NumericUpDown WrmHistoryAutoDelPeriodNumericUpDown;
        private System.Windows.Forms.ComboBox WrmHistoryAutoDelTypeComboBox;
        private System.Windows.Forms.GroupBox WrmHistoryAutoDelGroupBox;
        private System.Windows.Forms.Panel WrmHistoryPanel;
        private System.Windows.Forms.Panel WrmHistoryDataGridPanel;
        private Microarea.Console.Plugin.EasyLookCustomizer.EasyLookCustomizerControl.RunnedReportsDataGrid CurrentRunnedReportsDataGrid;
        private System.Windows.Forms.Button DelRunnedReportsButton;
        private System.Windows.Forms.Button LogoImageOpenFileButton;
        private System.Windows.Forms.Button ApplicationImageOpenFileButton;
        private System.Windows.Forms.OpenFileDialog LogoImageOpenFileDialog;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EasyLookCustomizerControl));
            this.CaptionPanel = new System.Windows.Forms.Panel();
            this.CaptionLabel = new System.Windows.Forms.Label();
            this.EasyLookCustomizerPictureBox = new System.Windows.Forms.PictureBox();
            this.DescriptionLabel = new System.Windows.Forms.Label();
            this.CustomizerTabControl = new System.Windows.Forms.TabControl();
            this.LayoutSettingsTabPage = new System.Windows.Forms.TabPage();
            this.LogoImageOpenFileButton = new System.Windows.Forms.Button();
            this.CustomReportsGroupBox = new System.Windows.Forms.GroupBox();
			this.CurrentUserTitleColorWebColorPicker = new Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker();
			this.AllUsersTitleColorWebColorPicker = new Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker();
            this.CurrentUserTitleColorLabel = new System.Windows.Forms.Label();
            this.AllUsersTitleColorLabel = new System.Windows.Forms.Label();
            this.ResetLayoutDefaultsButton = new System.Windows.Forms.Button();
            this.FontFamilyComboBox = new Microarea.Console.Plugin.EasyLookCustomizer.FontSelectionComboBox();
			this.CommandListBkgndColorWebColorPicker = new Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker();
			this.MenuTreeBkgndColorWebColorPicker = new Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker();
            this.GroupsPanelBkgndSettingsBox = new System.Windows.Forms.GroupBox();
            this.ApplicationImageOpenFileButton = new System.Windows.Forms.Button();
			this.GroupsPanelBkgndColorWebColorPicker = new Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker();
            this.GroupsPanelBkgndImageURLRichTextBox = new System.Windows.Forms.RichTextBox();
            this.GroupsPanelBkgndImageRadioButton = new System.Windows.Forms.RadioButton();
            this.GroupsPanelBkgndColorRadioButton = new System.Windows.Forms.RadioButton();
			this.AppPanelBkgndColorWebColorPicker = new Microarea.TaskBuilderNet.UI.WinControls.Combo.WebColorPicker();
            this.AppPanelBkgndColorLabel = new System.Windows.Forms.Label();
            this.LogoImageURLRichTextBox = new System.Windows.Forms.RichTextBox();
            this.LogoImageURLLabel = new System.Windows.Forms.Label();
            this.FontFamilyLabel = new System.Windows.Forms.Label();
            this.CommandListBkgndColorLabel = new System.Windows.Forms.Label();
            this.MenuTreeBkgndColorLabel = new System.Windows.Forms.Label();
            this.ManageReporHistoryTabPage = new System.Windows.Forms.TabPage();
            this.MaxWrmHistoryNumNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.MaxWrmHistoryNumLabel = new System.Windows.Forms.Label();
            this.WrmHistoryAutoDelGroupBox = new System.Windows.Forms.GroupBox();
            this.WrmHistoryAutoDelTypeComboBox = new System.Windows.Forms.ComboBox();
            this.WrmHistoryAutoDelPeriodNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.WrmHistoryAutoDelOlderThanLabel = new System.Windows.Forms.Label();
            this.WrmHistoryAutoDelEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.WrmHistoryAutoDelCommentLabel = new System.Windows.Forms.Label();
            this.WrmHistoryPanel = new System.Windows.Forms.Panel();
            this.WrmHistoryDataGridPanel = new System.Windows.Forms.Panel();
            this.DelRunnedReportsButton = new System.Windows.Forms.Button();
            this.CurrentRunnedReportsDataGrid = new Microarea.Console.Plugin.EasyLookCustomizer.EasyLookCustomizerControl.RunnedReportsDataGrid();
            this.SnapshotPictureBox = new System.Windows.Forms.PictureBox();
            this.SnapshotHelpLabel = new System.Windows.Forms.Label();
            this.LogoImageOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.CaptionPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EasyLookCustomizerPictureBox)).BeginInit();
            this.CustomizerTabControl.SuspendLayout();
            this.LayoutSettingsTabPage.SuspendLayout();
            this.CustomReportsGroupBox.SuspendLayout();
            this.GroupsPanelBkgndSettingsBox.SuspendLayout();
            this.ManageReporHistoryTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxWrmHistoryNumNumericUpDown)).BeginInit();
            this.WrmHistoryAutoDelGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WrmHistoryAutoDelPeriodNumericUpDown)).BeginInit();
            this.WrmHistoryPanel.SuspendLayout();
            this.WrmHistoryDataGridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CurrentRunnedReportsDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SnapshotPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // CaptionPanel
            // 
            this.CaptionPanel.BackColor = System.Drawing.Color.Lavender;
            this.CaptionPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.CaptionPanel.Controls.Add(this.CaptionLabel);
            this.CaptionPanel.Controls.Add(this.EasyLookCustomizerPictureBox);
            this.CaptionPanel.Controls.Add(this.DescriptionLabel);
            resources.ApplyResources(this.CaptionPanel, "CaptionPanel");
            this.CaptionPanel.Name = "CaptionPanel";
            // 
            // CaptionLabel
            // 
            this.CaptionLabel.CausesValidation = false;
            resources.ApplyResources(this.CaptionLabel, "CaptionLabel");
            this.CaptionLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CaptionLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.CaptionLabel.Name = "CaptionLabel";
            // 
            // EasyLookCustomizerPictureBox
            // 
            resources.ApplyResources(this.EasyLookCustomizerPictureBox, "EasyLookCustomizerPictureBox");
            this.EasyLookCustomizerPictureBox.Name = "EasyLookCustomizerPictureBox";
            this.EasyLookCustomizerPictureBox.TabStop = false;
            // 
            // DescriptionLabel
            // 
            resources.ApplyResources(this.DescriptionLabel, "DescriptionLabel");
            this.DescriptionLabel.Name = "DescriptionLabel";
            // 
            // CustomizerTabControl
            // 
            resources.ApplyResources(this.CustomizerTabControl, "CustomizerTabControl");
            this.CustomizerTabControl.Controls.Add(this.LayoutSettingsTabPage);
            this.CustomizerTabControl.Controls.Add(this.ManageReporHistoryTabPage);
            this.CustomizerTabControl.ForeColor = System.Drawing.Color.Navy;
            this.CustomizerTabControl.Name = "CustomizerTabControl";
            this.CustomizerTabControl.SelectedIndex = 0;
            // 
            // LayoutSettingsTabPage
            // 
            resources.ApplyResources(this.LayoutSettingsTabPage, "LayoutSettingsTabPage");
            this.LayoutSettingsTabPage.BackColor = System.Drawing.Color.Lavender;
            this.LayoutSettingsTabPage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LayoutSettingsTabPage.Controls.Add(this.LogoImageOpenFileButton);
            this.LayoutSettingsTabPage.Controls.Add(this.CustomReportsGroupBox);
            this.LayoutSettingsTabPage.Controls.Add(this.ResetLayoutDefaultsButton);
            this.LayoutSettingsTabPage.Controls.Add(this.FontFamilyComboBox);
            this.LayoutSettingsTabPage.Controls.Add(this.CommandListBkgndColorWebColorPicker);
            this.LayoutSettingsTabPage.Controls.Add(this.MenuTreeBkgndColorWebColorPicker);
            this.LayoutSettingsTabPage.Controls.Add(this.GroupsPanelBkgndSettingsBox);
            this.LayoutSettingsTabPage.Controls.Add(this.AppPanelBkgndColorWebColorPicker);
            this.LayoutSettingsTabPage.Controls.Add(this.AppPanelBkgndColorLabel);
            this.LayoutSettingsTabPage.Controls.Add(this.LogoImageURLRichTextBox);
            this.LayoutSettingsTabPage.Controls.Add(this.LogoImageURLLabel);
            this.LayoutSettingsTabPage.Controls.Add(this.FontFamilyLabel);
            this.LayoutSettingsTabPage.Controls.Add(this.CommandListBkgndColorLabel);
            this.LayoutSettingsTabPage.Controls.Add(this.MenuTreeBkgndColorLabel);
            this.LayoutSettingsTabPage.ForeColor = System.Drawing.Color.Navy;
            this.LayoutSettingsTabPage.Name = "LayoutSettingsTabPage";
            // 
            // LogoImageOpenFileButton
            // 
            resources.ApplyResources(this.LogoImageOpenFileButton, "LogoImageOpenFileButton");
            this.LogoImageOpenFileButton.Name = "LogoImageOpenFileButton";
            this.LogoImageOpenFileButton.Click += new System.EventHandler(this.LogoImageOpenFileButton_Click);
            // 
            // CustomReportsGroupBox
            // 
            resources.ApplyResources(this.CustomReportsGroupBox, "CustomReportsGroupBox");
            this.CustomReportsGroupBox.Controls.Add(this.CurrentUserTitleColorWebColorPicker);
            this.CustomReportsGroupBox.Controls.Add(this.AllUsersTitleColorWebColorPicker);
            this.CustomReportsGroupBox.Controls.Add(this.CurrentUserTitleColorLabel);
            this.CustomReportsGroupBox.Controls.Add(this.AllUsersTitleColorLabel);
            this.CustomReportsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CustomReportsGroupBox.Name = "CustomReportsGroupBox";
            this.CustomReportsGroupBox.TabStop = false;
            // 
            // CurrentUserTitleColorWebColorPicker
            // 
            resources.ApplyResources(this.CurrentUserTitleColorWebColorPicker, "CurrentUserTitleColorWebColorPicker");
            this.CurrentUserTitleColorWebColorPicker.BackColor = System.Drawing.Color.Lavender;
            this.CurrentUserTitleColorWebColorPicker.Color = System.Drawing.Color.Empty;
            this.CurrentUserTitleColorWebColorPicker.ComboBoxBackColor = System.Drawing.SystemColors.Window;
            this.CurrentUserTitleColorWebColorPicker.ForeColor = System.Drawing.Color.Navy;
            this.CurrentUserTitleColorWebColorPicker.Name = "CurrentUserTitleColorWebColorPicker";
            this.CurrentUserTitleColorWebColorPicker.SelectedColorChanged += new System.EventHandler(this.CurrentUserTitleColorWebColorPicker_SelectedColorChanged);
            // 
            // AllUsersTitleColorWebColorPicker
            // 
            resources.ApplyResources(this.AllUsersTitleColorWebColorPicker, "AllUsersTitleColorWebColorPicker");
            this.AllUsersTitleColorWebColorPicker.BackColor = System.Drawing.Color.Lavender;
            this.AllUsersTitleColorWebColorPicker.Color = System.Drawing.Color.Empty;
            this.AllUsersTitleColorWebColorPicker.ComboBoxBackColor = System.Drawing.SystemColors.Window;
            this.AllUsersTitleColorWebColorPicker.ForeColor = System.Drawing.Color.Navy;
            this.AllUsersTitleColorWebColorPicker.Name = "AllUsersTitleColorWebColorPicker";
            this.AllUsersTitleColorWebColorPicker.SelectedColorChanged += new System.EventHandler(this.AllUsersTitleColorWebColorPicker_SelectedColorChanged);
            // 
            // CurrentUserTitleColorLabel
            // 
            resources.ApplyResources(this.CurrentUserTitleColorLabel, "CurrentUserTitleColorLabel");
            this.CurrentUserTitleColorLabel.BackColor = System.Drawing.Color.Lavender;
            this.CurrentUserTitleColorLabel.ForeColor = System.Drawing.Color.Navy;
            this.CurrentUserTitleColorLabel.Name = "CurrentUserTitleColorLabel";
            // 
            // AllUsersTitleColorLabel
            // 
            resources.ApplyResources(this.AllUsersTitleColorLabel, "AllUsersTitleColorLabel");
            this.AllUsersTitleColorLabel.BackColor = System.Drawing.Color.Lavender;
            this.AllUsersTitleColorLabel.ForeColor = System.Drawing.Color.Navy;
            this.AllUsersTitleColorLabel.Name = "AllUsersTitleColorLabel";
            // 
            // ResetLayoutDefaultsButton
            // 
            resources.ApplyResources(this.ResetLayoutDefaultsButton, "ResetLayoutDefaultsButton");
            this.ResetLayoutDefaultsButton.Name = "ResetLayoutDefaultsButton";
            this.ResetLayoutDefaultsButton.Click += new System.EventHandler(this.ResetDefaultsButton_Click);
            // 
            // FontFamilyComboBox
            // 
            resources.ApplyResources(this.FontFamilyComboBox, "FontFamilyComboBox");
            this.FontFamilyComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.FontFamilyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FontFamilyComboBox.ForeColor = System.Drawing.Color.Navy;
            this.FontFamilyComboBox.Name = "FontFamilyComboBox";
            this.FontFamilyComboBox.SelectedFontName = "";
            this.FontFamilyComboBox.Sorted = true;
            this.FontFamilyComboBox.SelectedIndexChanged += new System.EventHandler(this.FontFamilyComboBox_SelectedIndexChanged);
            // 
            // CommandListBkgndColorWebColorPicker
            // 
            resources.ApplyResources(this.CommandListBkgndColorWebColorPicker, "CommandListBkgndColorWebColorPicker");
            this.CommandListBkgndColorWebColorPicker.BackColor = System.Drawing.Color.Lavender;
            this.CommandListBkgndColorWebColorPicker.Color = System.Drawing.Color.Empty;
            this.CommandListBkgndColorWebColorPicker.ComboBoxBackColor = System.Drawing.SystemColors.Window;
            this.CommandListBkgndColorWebColorPicker.ForeColor = System.Drawing.Color.Navy;
            this.CommandListBkgndColorWebColorPicker.Name = "CommandListBkgndColorWebColorPicker";
            this.CommandListBkgndColorWebColorPicker.SelectedColorChanged += new System.EventHandler(this.CommandListBkgndColorWebColorPicker_SelectedColorChanged);
            // 
            // MenuTreeBkgndColorWebColorPicker
            // 
            resources.ApplyResources(this.MenuTreeBkgndColorWebColorPicker, "MenuTreeBkgndColorWebColorPicker");
            this.MenuTreeBkgndColorWebColorPicker.BackColor = System.Drawing.Color.Lavender;
            this.MenuTreeBkgndColorWebColorPicker.Color = System.Drawing.Color.Empty;
            this.MenuTreeBkgndColorWebColorPicker.ComboBoxBackColor = System.Drawing.SystemColors.Window;
            this.MenuTreeBkgndColorWebColorPicker.ForeColor = System.Drawing.Color.Navy;
            this.MenuTreeBkgndColorWebColorPicker.Name = "MenuTreeBkgndColorWebColorPicker";
            this.MenuTreeBkgndColorWebColorPicker.SelectedColorChanged += new System.EventHandler(this.MenuTreeBkgndColorWebColorPicker_SelectedColorChanged);
            // 
            // GroupsPanelBkgndSettingsBox
            // 
            resources.ApplyResources(this.GroupsPanelBkgndSettingsBox, "GroupsPanelBkgndSettingsBox");
            this.GroupsPanelBkgndSettingsBox.BackColor = System.Drawing.Color.Lavender;
            this.GroupsPanelBkgndSettingsBox.Controls.Add(this.ApplicationImageOpenFileButton);
            this.GroupsPanelBkgndSettingsBox.Controls.Add(this.GroupsPanelBkgndColorWebColorPicker);
            this.GroupsPanelBkgndSettingsBox.Controls.Add(this.GroupsPanelBkgndImageURLRichTextBox);
            this.GroupsPanelBkgndSettingsBox.Controls.Add(this.GroupsPanelBkgndImageRadioButton);
            this.GroupsPanelBkgndSettingsBox.Controls.Add(this.GroupsPanelBkgndColorRadioButton);
            this.GroupsPanelBkgndSettingsBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GroupsPanelBkgndSettingsBox.ForeColor = System.Drawing.Color.Navy;
            this.GroupsPanelBkgndSettingsBox.Name = "GroupsPanelBkgndSettingsBox";
            this.GroupsPanelBkgndSettingsBox.TabStop = false;
            // 
            // ApplicationImageOpenFileButton
            // 
            resources.ApplyResources(this.ApplicationImageOpenFileButton, "ApplicationImageOpenFileButton");
            this.ApplicationImageOpenFileButton.Name = "ApplicationImageOpenFileButton";
            this.ApplicationImageOpenFileButton.Click += new System.EventHandler(this.ApplicationImageOpenFileButton_Click);
            // 
            // GroupsPanelBkgndColorWebColorPicker
            // 
            resources.ApplyResources(this.GroupsPanelBkgndColorWebColorPicker, "GroupsPanelBkgndColorWebColorPicker");
            this.GroupsPanelBkgndColorWebColorPicker.BackColor = System.Drawing.Color.Lavender;
            this.GroupsPanelBkgndColorWebColorPicker.Color = System.Drawing.Color.Empty;
            this.GroupsPanelBkgndColorWebColorPicker.ComboBoxBackColor = System.Drawing.SystemColors.Window;
            this.GroupsPanelBkgndColorWebColorPicker.ForeColor = System.Drawing.Color.Navy;
            this.GroupsPanelBkgndColorWebColorPicker.Name = "GroupsPanelBkgndColorWebColorPicker";
            this.GroupsPanelBkgndColorWebColorPicker.SelectedColorChanged += new System.EventHandler(this.GroupsPanelBkgndColorWebColorPicker_SelectedColorChanged);
            // 
            // GroupsPanelBkgndImageURLRichTextBox
            // 
            resources.ApplyResources(this.GroupsPanelBkgndImageURLRichTextBox, "GroupsPanelBkgndImageURLRichTextBox");
            this.GroupsPanelBkgndImageURLRichTextBox.ForeColor = System.Drawing.Color.Navy;
            this.GroupsPanelBkgndImageURLRichTextBox.Name = "GroupsPanelBkgndImageURLRichTextBox";
            this.GroupsPanelBkgndImageURLRichTextBox.Validated += new System.EventHandler(this.GroupsPanelBkgndImageURLRichTextBox_Validated);
            this.GroupsPanelBkgndImageURLRichTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.GroupsPanelBkgndImageURLRichTextBox_Validating);
            // 
            // GroupsPanelBkgndImageRadioButton
            // 
            this.GroupsPanelBkgndImageRadioButton.BackColor = System.Drawing.Color.Lavender;
            this.GroupsPanelBkgndImageRadioButton.ForeColor = System.Drawing.Color.Navy;
            resources.ApplyResources(this.GroupsPanelBkgndImageRadioButton, "GroupsPanelBkgndImageRadioButton");
            this.GroupsPanelBkgndImageRadioButton.Name = "GroupsPanelBkgndImageRadioButton";
            this.GroupsPanelBkgndImageRadioButton.UseVisualStyleBackColor = false;
            this.GroupsPanelBkgndImageRadioButton.CheckedChanged += new System.EventHandler(this.GroupsPanelBkgndRadioButton_CheckedChanged);
            // 
            // GroupsPanelBkgndColorRadioButton
            // 
            this.GroupsPanelBkgndColorRadioButton.BackColor = System.Drawing.Color.Lavender;
            this.GroupsPanelBkgndColorRadioButton.ForeColor = System.Drawing.Color.Navy;
            resources.ApplyResources(this.GroupsPanelBkgndColorRadioButton, "GroupsPanelBkgndColorRadioButton");
            this.GroupsPanelBkgndColorRadioButton.Name = "GroupsPanelBkgndColorRadioButton";
            this.GroupsPanelBkgndColorRadioButton.UseVisualStyleBackColor = false;
            this.GroupsPanelBkgndColorRadioButton.CheckedChanged += new System.EventHandler(this.GroupsPanelBkgndRadioButton_CheckedChanged);
            // 
            // AppPanelBkgndColorWebColorPicker
            // 
            resources.ApplyResources(this.AppPanelBkgndColorWebColorPicker, "AppPanelBkgndColorWebColorPicker");
            this.AppPanelBkgndColorWebColorPicker.BackColor = System.Drawing.Color.Lavender;
            this.AppPanelBkgndColorWebColorPicker.Color = System.Drawing.Color.Empty;
            this.AppPanelBkgndColorWebColorPicker.ComboBoxBackColor = System.Drawing.SystemColors.Window;
            this.AppPanelBkgndColorWebColorPicker.ForeColor = System.Drawing.Color.Navy;
            this.AppPanelBkgndColorWebColorPicker.Name = "AppPanelBkgndColorWebColorPicker";
            this.AppPanelBkgndColorWebColorPicker.SelectedColorChanged += new System.EventHandler(this.AppPanelBkgndColorWebColorPicker_SelectedColorChanged);
            // 
            // AppPanelBkgndColorLabel
            // 
            resources.ApplyResources(this.AppPanelBkgndColorLabel, "AppPanelBkgndColorLabel");
            this.AppPanelBkgndColorLabel.BackColor = System.Drawing.Color.Lavender;
            this.AppPanelBkgndColorLabel.ForeColor = System.Drawing.Color.Navy;
            this.AppPanelBkgndColorLabel.Name = "AppPanelBkgndColorLabel";
            // 
            // LogoImageURLRichTextBox
            // 
            resources.ApplyResources(this.LogoImageURLRichTextBox, "LogoImageURLRichTextBox");
            this.LogoImageURLRichTextBox.ForeColor = System.Drawing.Color.Navy;
            this.LogoImageURLRichTextBox.Name = "LogoImageURLRichTextBox";
            this.LogoImageURLRichTextBox.Validated += new System.EventHandler(this.LogoImageURLRichTextBox_Validated);
            // 
            // LogoImageURLLabel
            // 
            resources.ApplyResources(this.LogoImageURLLabel, "LogoImageURLLabel");
            this.LogoImageURLLabel.BackColor = System.Drawing.Color.Lavender;
            this.LogoImageURLLabel.ForeColor = System.Drawing.Color.Navy;
            this.LogoImageURLLabel.Name = "LogoImageURLLabel";
            // 
            // FontFamilyLabel
            // 
            resources.ApplyResources(this.FontFamilyLabel, "FontFamilyLabel");
            this.FontFamilyLabel.BackColor = System.Drawing.Color.Lavender;
            this.FontFamilyLabel.ForeColor = System.Drawing.Color.Navy;
            this.FontFamilyLabel.Name = "FontFamilyLabel";
            // 
            // CommandListBkgndColorLabel
            // 
            resources.ApplyResources(this.CommandListBkgndColorLabel, "CommandListBkgndColorLabel");
            this.CommandListBkgndColorLabel.BackColor = System.Drawing.Color.Lavender;
            this.CommandListBkgndColorLabel.ForeColor = System.Drawing.Color.Navy;
            this.CommandListBkgndColorLabel.Name = "CommandListBkgndColorLabel";
            // 
            // MenuTreeBkgndColorLabel
            // 
            resources.ApplyResources(this.MenuTreeBkgndColorLabel, "MenuTreeBkgndColorLabel");
            this.MenuTreeBkgndColorLabel.BackColor = System.Drawing.Color.Lavender;
            this.MenuTreeBkgndColorLabel.ForeColor = System.Drawing.Color.Navy;
            this.MenuTreeBkgndColorLabel.Name = "MenuTreeBkgndColorLabel";
            // 
            // ManageReporHistoryTabPage
            // 
            resources.ApplyResources(this.ManageReporHistoryTabPage, "ManageReporHistoryTabPage");
            this.ManageReporHistoryTabPage.BackColor = System.Drawing.Color.Lavender;
            this.ManageReporHistoryTabPage.Controls.Add(this.MaxWrmHistoryNumNumericUpDown);
            this.ManageReporHistoryTabPage.Controls.Add(this.MaxWrmHistoryNumLabel);
            this.ManageReporHistoryTabPage.Controls.Add(this.WrmHistoryAutoDelGroupBox);
            this.ManageReporHistoryTabPage.Controls.Add(this.WrmHistoryPanel);
            this.ManageReporHistoryTabPage.Name = "ManageReporHistoryTabPage";
            // 
            // MaxWrmHistoryNumNumericUpDown
            // 
            resources.ApplyResources(this.MaxWrmHistoryNumNumericUpDown, "MaxWrmHistoryNumNumericUpDown");
            this.MaxWrmHistoryNumNumericUpDown.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.MaxWrmHistoryNumNumericUpDown.Name = "MaxWrmHistoryNumNumericUpDown";
            // 
            // MaxWrmHistoryNumLabel
            // 
            resources.ApplyResources(this.MaxWrmHistoryNumLabel, "MaxWrmHistoryNumLabel");
            this.MaxWrmHistoryNumLabel.Name = "MaxWrmHistoryNumLabel";
            // 
            // WrmHistoryAutoDelGroupBox
            // 
            resources.ApplyResources(this.WrmHistoryAutoDelGroupBox, "WrmHistoryAutoDelGroupBox");
            this.WrmHistoryAutoDelGroupBox.Controls.Add(this.WrmHistoryAutoDelTypeComboBox);
            this.WrmHistoryAutoDelGroupBox.Controls.Add(this.WrmHistoryAutoDelPeriodNumericUpDown);
            this.WrmHistoryAutoDelGroupBox.Controls.Add(this.WrmHistoryAutoDelOlderThanLabel);
            this.WrmHistoryAutoDelGroupBox.Controls.Add(this.WrmHistoryAutoDelEnabledCheckBox);
            this.WrmHistoryAutoDelGroupBox.Controls.Add(this.WrmHistoryAutoDelCommentLabel);
            this.WrmHistoryAutoDelGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.WrmHistoryAutoDelGroupBox.Name = "WrmHistoryAutoDelGroupBox";
            this.WrmHistoryAutoDelGroupBox.TabStop = false;
            // 
            // WrmHistoryAutoDelTypeComboBox
            // 
            resources.ApplyResources(this.WrmHistoryAutoDelTypeComboBox, "WrmHistoryAutoDelTypeComboBox");
            this.WrmHistoryAutoDelTypeComboBox.Items.AddRange(new object[] {
            resources.GetString("WrmHistoryAutoDelTypeComboBox.Items"),
            resources.GetString("WrmHistoryAutoDelTypeComboBox.Items1"),
            resources.GetString("WrmHistoryAutoDelTypeComboBox.Items2"),
            resources.GetString("WrmHistoryAutoDelTypeComboBox.Items3")});
            this.WrmHistoryAutoDelTypeComboBox.Name = "WrmHistoryAutoDelTypeComboBox";
            // 
            // WrmHistoryAutoDelPeriodNumericUpDown
            // 
            resources.ApplyResources(this.WrmHistoryAutoDelPeriodNumericUpDown, "WrmHistoryAutoDelPeriodNumericUpDown");
            this.WrmHistoryAutoDelPeriodNumericUpDown.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.WrmHistoryAutoDelPeriodNumericUpDown.Name = "WrmHistoryAutoDelPeriodNumericUpDown";
            // 
            // WrmHistoryAutoDelOlderThanLabel
            // 
            resources.ApplyResources(this.WrmHistoryAutoDelOlderThanLabel, "WrmHistoryAutoDelOlderThanLabel");
            this.WrmHistoryAutoDelOlderThanLabel.Name = "WrmHistoryAutoDelOlderThanLabel";
            // 
            // WrmHistoryAutoDelEnabledCheckBox
            // 
            resources.ApplyResources(this.WrmHistoryAutoDelEnabledCheckBox, "WrmHistoryAutoDelEnabledCheckBox");
            this.WrmHistoryAutoDelEnabledCheckBox.Name = "WrmHistoryAutoDelEnabledCheckBox";
            this.WrmHistoryAutoDelEnabledCheckBox.CheckedChanged += new System.EventHandler(this.WrmHistoryAutoDelEnabledCheckBox_CheckedChanged);
            // 
            // WrmHistoryAutoDelCommentLabel
            // 
            resources.ApplyResources(this.WrmHistoryAutoDelCommentLabel, "WrmHistoryAutoDelCommentLabel");
            this.WrmHistoryAutoDelCommentLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.WrmHistoryAutoDelCommentLabel.Name = "WrmHistoryAutoDelCommentLabel";
            // 
            // WrmHistoryPanel
            // 
            resources.ApplyResources(this.WrmHistoryPanel, "WrmHistoryPanel");
            this.WrmHistoryPanel.Controls.Add(this.WrmHistoryDataGridPanel);
            this.WrmHistoryPanel.Controls.Add(this.SnapshotPictureBox);
            this.WrmHistoryPanel.Controls.Add(this.SnapshotHelpLabel);
            this.WrmHistoryPanel.Name = "WrmHistoryPanel";
            // 
            // WrmHistoryDataGridPanel
            // 
            resources.ApplyResources(this.WrmHistoryDataGridPanel, "WrmHistoryDataGridPanel");
            this.WrmHistoryDataGridPanel.Controls.Add(this.DelRunnedReportsButton);
            this.WrmHistoryDataGridPanel.Controls.Add(this.CurrentRunnedReportsDataGrid);
            this.WrmHistoryDataGridPanel.Name = "WrmHistoryDataGridPanel";
            // 
            // DelRunnedReportsButton
            // 
            resources.ApplyResources(this.DelRunnedReportsButton, "DelRunnedReportsButton");
            this.DelRunnedReportsButton.Name = "DelRunnedReportsButton";
            this.DelRunnedReportsButton.Click += new System.EventHandler(this.DelRunnedReportsButton_Click);
            // 
            // CurrentRunnedReportsDataGrid
            // 
            resources.ApplyResources(this.CurrentRunnedReportsDataGrid, "CurrentRunnedReportsDataGrid");
            this.CurrentRunnedReportsDataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
            this.CurrentRunnedReportsDataGrid.CaptionBackColor = System.Drawing.Color.CornflowerBlue;
            this.CurrentRunnedReportsDataGrid.CaptionForeColor = System.Drawing.Color.Navy;
            this.CurrentRunnedReportsDataGrid.Connection = null;
            this.CurrentRunnedReportsDataGrid.DataMember = "";
            this.CurrentRunnedReportsDataGrid.ForeColor = System.Drawing.Color.Navy;
            this.CurrentRunnedReportsDataGrid.GridLineColor = System.Drawing.Color.LightSeaGreen;
            this.CurrentRunnedReportsDataGrid.HeaderBackColor = System.Drawing.Color.LightSteelBlue;
            this.CurrentRunnedReportsDataGrid.HeaderForeColor = System.Drawing.Color.Navy;
            this.CurrentRunnedReportsDataGrid.Name = "CurrentRunnedReportsDataGrid";
            this.CurrentRunnedReportsDataGrid.ParentRowsBackColor = System.Drawing.Color.Lavender;
            this.CurrentRunnedReportsDataGrid.ParentRowsForeColor = System.Drawing.Color.Navy;
            this.CurrentRunnedReportsDataGrid.ReadOnly = true;
            this.CurrentRunnedReportsDataGrid.VisibleChanged += new System.EventHandler(this.CurrentRunnedReportsDataGrid_VisibleChanged);
            this.CurrentRunnedReportsDataGrid.RowsSelectionChanged += new System.EventHandler(this.CurrentRunnedReportsDataGrid_RowsSelectionChanged);
            // 
            // SnapshotPictureBox
            // 
            resources.ApplyResources(this.SnapshotPictureBox, "SnapshotPictureBox");
            this.SnapshotPictureBox.Name = "SnapshotPictureBox";
            this.SnapshotPictureBox.TabStop = false;
            // 
            // SnapshotHelpLabel
            // 
            resources.ApplyResources(this.SnapshotHelpLabel, "SnapshotHelpLabel");
            this.SnapshotHelpLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.SnapshotHelpLabel.Name = "SnapshotHelpLabel";
            // 
            // EasyLookCustomizerControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.CustomizerTabControl);
            this.Controls.Add(this.CaptionPanel);
            this.ForeColor = System.Drawing.Color.Navy;
            this.Name = "EasyLookCustomizerControl";
            this.CaptionPanel.ResumeLayout(false);
            this.CaptionPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EasyLookCustomizerPictureBox)).EndInit();
            this.CustomizerTabControl.ResumeLayout(false);
            this.LayoutSettingsTabPage.ResumeLayout(false);
            this.LayoutSettingsTabPage.PerformLayout();
            this.CustomReportsGroupBox.ResumeLayout(false);
            this.CustomReportsGroupBox.PerformLayout();
            this.GroupsPanelBkgndSettingsBox.ResumeLayout(false);
            this.ManageReporHistoryTabPage.ResumeLayout(false);
            this.ManageReporHistoryTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxWrmHistoryNumNumericUpDown)).EndInit();
            this.WrmHistoryAutoDelGroupBox.ResumeLayout(false);
            this.WrmHistoryAutoDelGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WrmHistoryAutoDelPeriodNumericUpDown)).EndInit();
            this.WrmHistoryPanel.ResumeLayout(false);
            this.WrmHistoryPanel.PerformLayout();
            this.WrmHistoryDataGridPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CurrentRunnedReportsDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SnapshotPictureBox)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

    }
}
