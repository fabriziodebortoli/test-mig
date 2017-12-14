
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    partial class MenuSearchForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Panel SearchCriteriaPanel;
        private System.Windows.Forms.Label SearchCriteriaLabel;
        private System.Windows.Forms.ComboBox SearchCriteriaComboBox;
        private System.Windows.Forms.Label FilteredByLabel;
        private System.Windows.Forms.ComboBox FilteredByComboBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.CheckBox SearchInTitlesOnlyCheckBox;
        private System.Windows.Forms.CheckBox SearchExactWordsCheckBox;
        private System.Windows.Forms.CheckBox MatchCaseCheckBox;
        private System.Windows.Forms.CheckBox SearchInPreviousResultsCheckBox;
        private System.Windows.Forms.Splitter SearchPanelsSplitter;
        private EnhancedCommandsView SearchResultsView;
        private System.Windows.Forms.StatusBar SearchStatusBar;
        private System.Windows.Forms.StatusBarPanel SearchStatusBarPanel;
        
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuSearchForm));
			this.SearchStatusBar = new System.Windows.Forms.StatusBar();
			this.SearchStatusBarPanel = new System.Windows.Forms.StatusBarPanel();
			this.SearchPanelsSplitter = new System.Windows.Forms.Splitter();
			this.SearchCriteriaPanel = new System.Windows.Forms.Panel();
			this.MatchCaseCheckBox = new System.Windows.Forms.CheckBox();
			this.SearchExactWordsCheckBox = new System.Windows.Forms.CheckBox();
			this.SearchInPreviousResultsCheckBox = new System.Windows.Forms.CheckBox();
			this.SearchInTitlesOnlyCheckBox = new System.Windows.Forms.CheckBox();
			this.SearchButton = new System.Windows.Forms.Button();
			this.FilteredByComboBox = new System.Windows.Forms.ComboBox();
			this.FilteredByLabel = new System.Windows.Forms.Label();
			this.SearchCriteriaComboBox = new System.Windows.Forms.ComboBox();
			this.SearchCriteriaLabel = new System.Windows.Forms.Label();
			this.SearchResultsView = new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.EnhancedCommandsView();
			((System.ComponentModel.ISupportInitialize)(this.SearchStatusBarPanel)).BeginInit();
			this.SearchCriteriaPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// SearchStatusBar
			// 
			resources.ApplyResources(this.SearchStatusBar, "SearchStatusBar");
			this.SearchStatusBar.Name = "SearchStatusBar";
			this.SearchStatusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.SearchStatusBarPanel});
			this.SearchStatusBar.ShowPanels = true;
			// 
			// SearchStatusBarPanel
			// 
			this.SearchStatusBarPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			resources.ApplyResources(this.SearchStatusBarPanel, "SearchStatusBarPanel");
			// 
			// SearchPanelsSplitter
			// 
			this.SearchPanelsSplitter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			resources.ApplyResources(this.SearchPanelsSplitter, "SearchPanelsSplitter");
			this.SearchPanelsSplitter.Name = "SearchPanelsSplitter";
			this.SearchPanelsSplitter.TabStop = false;
			// 
			// SearchCriteriaPanel
			// 
			this.SearchCriteriaPanel.BackColor = System.Drawing.Color.LightSteelBlue;
			this.SearchCriteriaPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SearchCriteriaPanel.Controls.Add(this.MatchCaseCheckBox);
			this.SearchCriteriaPanel.Controls.Add(this.SearchExactWordsCheckBox);
			this.SearchCriteriaPanel.Controls.Add(this.SearchInPreviousResultsCheckBox);
			this.SearchCriteriaPanel.Controls.Add(this.SearchInTitlesOnlyCheckBox);
			this.SearchCriteriaPanel.Controls.Add(this.SearchButton);
			this.SearchCriteriaPanel.Controls.Add(this.FilteredByComboBox);
			this.SearchCriteriaPanel.Controls.Add(this.FilteredByLabel);
			this.SearchCriteriaPanel.Controls.Add(this.SearchCriteriaComboBox);
			this.SearchCriteriaPanel.Controls.Add(this.SearchCriteriaLabel);
			resources.ApplyResources(this.SearchCriteriaPanel, "SearchCriteriaPanel");
			this.SearchCriteriaPanel.Name = "SearchCriteriaPanel";
			// 
			// MatchCaseCheckBox
			// 
			resources.ApplyResources(this.MatchCaseCheckBox, "MatchCaseCheckBox");
			this.MatchCaseCheckBox.AutoEllipsis = true;
			this.MatchCaseCheckBox.BackColor = System.Drawing.Color.LightSteelBlue;
			this.MatchCaseCheckBox.ForeColor = System.Drawing.Color.Navy;
			this.MatchCaseCheckBox.Name = "MatchCaseCheckBox";
			this.MatchCaseCheckBox.UseVisualStyleBackColor = false;
			// 
			// SearchExactWordsCheckBox
			// 
			resources.ApplyResources(this.SearchExactWordsCheckBox, "SearchExactWordsCheckBox");
			this.SearchExactWordsCheckBox.AutoEllipsis = true;
			this.SearchExactWordsCheckBox.BackColor = System.Drawing.Color.LightSteelBlue;
			this.SearchExactWordsCheckBox.ForeColor = System.Drawing.Color.Navy;
			this.SearchExactWordsCheckBox.Name = "SearchExactWordsCheckBox";
			this.SearchExactWordsCheckBox.UseVisualStyleBackColor = false;
			// 
			// SearchInPreviousResultsCheckBox
			// 
			resources.ApplyResources(this.SearchInPreviousResultsCheckBox, "SearchInPreviousResultsCheckBox");
			this.SearchInPreviousResultsCheckBox.AutoEllipsis = true;
			this.SearchInPreviousResultsCheckBox.BackColor = System.Drawing.Color.LightSteelBlue;
			this.SearchInPreviousResultsCheckBox.ForeColor = System.Drawing.Color.Navy;
			this.SearchInPreviousResultsCheckBox.Name = "SearchInPreviousResultsCheckBox";
			this.SearchInPreviousResultsCheckBox.UseVisualStyleBackColor = false;
			// 
			// SearchInTitlesOnlyCheckBox
			// 
			resources.ApplyResources(this.SearchInTitlesOnlyCheckBox, "SearchInTitlesOnlyCheckBox");
			this.SearchInTitlesOnlyCheckBox.AutoEllipsis = true;
			this.SearchInTitlesOnlyCheckBox.BackColor = System.Drawing.Color.LightSteelBlue;
			this.SearchInTitlesOnlyCheckBox.ForeColor = System.Drawing.Color.Navy;
			this.SearchInTitlesOnlyCheckBox.Name = "SearchInTitlesOnlyCheckBox";
			this.SearchInTitlesOnlyCheckBox.UseVisualStyleBackColor = false;
			this.SearchInTitlesOnlyCheckBox.CheckedChanged += new System.EventHandler(this.SearchInTitlesOnlyCheckBox_CheckedChanged);
			// 
			// SearchButton
			// 
			this.SearchButton.BackColor = System.Drawing.SystemColors.Control;
			resources.ApplyResources(this.SearchButton, "SearchButton");
			this.SearchButton.ForeColor = System.Drawing.Color.Navy;
			this.SearchButton.Name = "SearchButton";
			this.SearchButton.UseVisualStyleBackColor = false;
			this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
			// 
			// FilteredByComboBox
			// 
			resources.ApplyResources(this.FilteredByComboBox, "FilteredByComboBox");
			this.FilteredByComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.FilteredByComboBox.DropDownWidth = 270;
			this.FilteredByComboBox.Name = "FilteredByComboBox";
			// 
			// FilteredByLabel
			// 
			resources.ApplyResources(this.FilteredByLabel, "FilteredByLabel");
			this.FilteredByLabel.BackColor = System.Drawing.Color.LightSteelBlue;
			this.FilteredByLabel.ForeColor = System.Drawing.Color.Navy;
			this.FilteredByLabel.Name = "FilteredByLabel";
			// 
			// SearchCriteriaComboBox
			// 
			resources.ApplyResources(this.SearchCriteriaComboBox, "SearchCriteriaComboBox");
			this.SearchCriteriaComboBox.DropDownWidth = 270;
			this.SearchCriteriaComboBox.Name = "SearchCriteriaComboBox";
			this.SearchCriteriaComboBox.TextChanged += new System.EventHandler(this.SearchCriteriaComboBox_TextChanged);
			// 
			// SearchCriteriaLabel
			// 
			resources.ApplyResources(this.SearchCriteriaLabel, "SearchCriteriaLabel");
			this.SearchCriteriaLabel.BackColor = System.Drawing.Color.LightSteelBlue;
			this.SearchCriteriaLabel.ForeColor = System.Drawing.Color.Navy;
			this.SearchCriteriaLabel.Name = "SearchCriteriaLabel";
			// 
			// SearchResultsView
			// 
			this.SearchResultsView.AllUsersCommandForeColor = System.Drawing.Color.Blue;
			this.SearchResultsView.BackColor = System.Drawing.SystemColors.Control;
			this.SearchResultsView.CurrentMenuNode = null;
			this.SearchResultsView.CurrentUserCommandForeColor = System.Drawing.Color.RoyalBlue;
			resources.ApplyResources(this.SearchResultsView, "SearchResultsView");
			this.SearchResultsView.FloatingFormLocation = new System.Drawing.Point(0, 0);
			this.SearchResultsView.MenuXmlParser = null;
			this.SearchResultsView.Name = "SearchResultsView";
			this.SearchResultsView.ParentMenuIndependent = true;
			this.SearchResultsView.PathFinder = null;
			this.SearchResultsView.SelectedCommand = null;
			this.SearchResultsView.ShowBatches = true;
			this.SearchResultsView.ShowCommandsDescriptions = true;
			this.SearchResultsView.ShowDocuments = true;
			this.SearchResultsView.ShowExecutables = true;
			this.SearchResultsView.ShowFunctions = true;
			this.SearchResultsView.ShowOfficeItems = true;
			this.SearchResultsView.ShowReports = true;
			this.SearchResultsView.ShowReportsDates = false;
			this.SearchResultsView.ShowStateImages = false;
			this.SearchResultsView.ShowTexts = true;
			this.SearchResultsView.ShowViewToolBar = true;
			this.SearchResultsView.ShowFlagsChanged += new System.EventHandler(this.SearchResultsView_ShowFlagsChanged);
			this.SearchResultsView.RunCommand += new Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.MenuMngCtrlEventHandler(this.SearchResultsView_RunCommand);
			// 
			// MenuSearchForm
			// 
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.SearchResultsView);
			this.Controls.Add(this.SearchPanelsSplitter);
			this.Controls.Add(this.SearchCriteriaPanel);
			this.Controls.Add(this.SearchStatusBar);
			this.Name = "MenuSearchForm";
			((System.ComponentModel.ISupportInitialize)(this.SearchStatusBarPanel)).EndInit();
			this.SearchCriteriaPanel.ResumeLayout(false);
			this.SearchCriteriaPanel.PerformLayout();
			this.ResumeLayout(false);

        }
        #endregion
    }
}
