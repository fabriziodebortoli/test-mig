namespace Microarea.Console.Plugin.SecurityAdmin
{
    partial class SearchSecurityObjectsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchSecurityObjectsForm));
            this.MatchCaseCheckBox = new System.Windows.Forms.CheckBox();
            this.SearchExactWordsCheckBox = new System.Windows.Forms.CheckBox();
            this.SearchInPreviousResultsCheckBox = new System.Windows.Forms.CheckBox();
            this.SearchInTitlesOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.SearchCriteriaComboBox = new System.Windows.Forms.ComboBox();
            this.SearchCriteriaLabel = new System.Windows.Forms.Label();
            this.FilteredByLabel = new System.Windows.Forms.Label();
            this.SearchCriteriaPanel = new System.Windows.Forms.Panel();
            this.SearchProtectedComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SearchInNamespaceCheckBox = new System.Windows.Forms.CheckBox();
            this.FilteredByComboBox = new System.Windows.Forms.ComboBox();
            this.SearchResultsListView = new System.Windows.Forms.ListView();
            this.TitleColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.NameSpaceColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.SearchCriteriaPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MatchCaseCheckBox
            // 
            resources.ApplyResources(this.MatchCaseCheckBox, "MatchCaseCheckBox");
            this.MatchCaseCheckBox.BackColor = System.Drawing.Color.LightSteelBlue;
            this.MatchCaseCheckBox.ForeColor = System.Drawing.Color.Navy;
            this.MatchCaseCheckBox.Name = "MatchCaseCheckBox";
            this.MatchCaseCheckBox.UseVisualStyleBackColor = false;
            // 
            // SearchExactWordsCheckBox
            // 
            resources.ApplyResources(this.SearchExactWordsCheckBox, "SearchExactWordsCheckBox");
            this.SearchExactWordsCheckBox.BackColor = System.Drawing.Color.LightSteelBlue;
            this.SearchExactWordsCheckBox.ForeColor = System.Drawing.Color.Navy;
            this.SearchExactWordsCheckBox.Name = "SearchExactWordsCheckBox";
            this.SearchExactWordsCheckBox.UseVisualStyleBackColor = false;
            // 
            // SearchInPreviousResultsCheckBox
            // 
            resources.ApplyResources(this.SearchInPreviousResultsCheckBox, "SearchInPreviousResultsCheckBox");
            this.SearchInPreviousResultsCheckBox.BackColor = System.Drawing.Color.LightSteelBlue;
            this.SearchInPreviousResultsCheckBox.ForeColor = System.Drawing.Color.Navy;
            this.SearchInPreviousResultsCheckBox.Name = "SearchInPreviousResultsCheckBox";
            this.SearchInPreviousResultsCheckBox.UseVisualStyleBackColor = false;
            // 
            // SearchInTitlesOnlyCheckBox
            // 
            resources.ApplyResources(this.SearchInTitlesOnlyCheckBox, "SearchInTitlesOnlyCheckBox");
            this.SearchInTitlesOnlyCheckBox.BackColor = System.Drawing.Color.LightSteelBlue;
            this.SearchInTitlesOnlyCheckBox.ForeColor = System.Drawing.Color.Navy;
            this.SearchInTitlesOnlyCheckBox.Name = "SearchInTitlesOnlyCheckBox";
            this.SearchInTitlesOnlyCheckBox.UseVisualStyleBackColor = false;
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
            // FilteredByLabel
            // 
            resources.ApplyResources(this.FilteredByLabel, "FilteredByLabel");
            this.FilteredByLabel.BackColor = System.Drawing.Color.LightSteelBlue;
            this.FilteredByLabel.ForeColor = System.Drawing.Color.Navy;
            this.FilteredByLabel.Name = "FilteredByLabel";
            // 
            // SearchCriteriaPanel
            // 
            this.SearchCriteriaPanel.BackColor = System.Drawing.Color.LightSteelBlue;
            this.SearchCriteriaPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.SearchCriteriaPanel.Controls.Add(this.SearchProtectedComboBox);
            this.SearchCriteriaPanel.Controls.Add(this.label1);
            this.SearchCriteriaPanel.Controls.Add(this.SearchInNamespaceCheckBox);
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
            // SearchProtectedComboBox
            // 
            resources.ApplyResources(this.SearchProtectedComboBox, "SearchProtectedComboBox");
            this.SearchProtectedComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SearchProtectedComboBox.DropDownWidth = 270;
            this.SearchProtectedComboBox.Name = "SearchProtectedComboBox";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.label1.ForeColor = System.Drawing.Color.Navy;
            this.label1.Name = "label1";
            // 
            // SearchInNamespaceCheckBox
            // 
            this.SearchInNamespaceCheckBox.AllowDrop = true;
            resources.ApplyResources(this.SearchInNamespaceCheckBox, "SearchInNamespaceCheckBox");
            this.SearchInNamespaceCheckBox.BackColor = System.Drawing.Color.LightSteelBlue;
            this.SearchInNamespaceCheckBox.ForeColor = System.Drawing.Color.Navy;
            this.SearchInNamespaceCheckBox.Name = "SearchInNamespaceCheckBox";
            this.SearchInNamespaceCheckBox.UseVisualStyleBackColor = false;
            // 
            // FilteredByComboBox
            // 
            resources.ApplyResources(this.FilteredByComboBox, "FilteredByComboBox");
            this.FilteredByComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FilteredByComboBox.DropDownWidth = 270;
            this.FilteredByComboBox.Name = "FilteredByComboBox";
            // 
            // SearchResultsListView
            // 
            this.SearchResultsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TitleColumnHeader,
            this.NameSpaceColumnHeader});
            resources.ApplyResources(this.SearchResultsListView, "SearchResultsListView");
            this.SearchResultsListView.Name = "SearchResultsListView";
            this.SearchResultsListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.SearchResultsListView.UseCompatibleStateImageBehavior = false;
            this.SearchResultsListView.View = System.Windows.Forms.View.Details;
            this.SearchResultsListView.DoubleClick += new System.EventHandler(this.SearchResultsListView_DoubleClick);
            this.SearchResultsListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.SearchResultsListView_ColumnClick);
            // 
            // TitleColumnHeader
            // 
            resources.ApplyResources(this.TitleColumnHeader, "TitleColumnHeader");
            // 
            // NameSpaceColumnHeader
            // 
            resources.ApplyResources(this.NameSpaceColumnHeader, "NameSpaceColumnHeader");
            // 
            // SearchSecurityObjectsForm
            // 
            this.AcceptButton = this.SearchButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.SearchResultsListView);
            this.Controls.Add(this.SearchCriteriaPanel);
            this.Name = "SearchSecurityObjectsForm";
            this.TopMost = true;
            this.SearchCriteriaPanel.ResumeLayout(false);
            this.SearchCriteriaPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox MatchCaseCheckBox;
        private System.Windows.Forms.CheckBox SearchExactWordsCheckBox;
        private System.Windows.Forms.CheckBox SearchInPreviousResultsCheckBox;
        private System.Windows.Forms.CheckBox SearchInTitlesOnlyCheckBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.ComboBox SearchCriteriaComboBox;
        private System.Windows.Forms.Label SearchCriteriaLabel;
        private System.Windows.Forms.Label FilteredByLabel;
        private System.Windows.Forms.Panel SearchCriteriaPanel;
        private System.Windows.Forms.ComboBox FilteredByComboBox;
        private System.Windows.Forms.ListView SearchResultsListView;
        private System.Windows.Forms.ColumnHeader TitleColumnHeader;
        private System.Windows.Forms.ColumnHeader NameSpaceColumnHeader;
        private System.Windows.Forms.CheckBox SearchInNamespaceCheckBox;
        private System.Windows.Forms.ComboBox SearchProtectedComboBox;
        private System.Windows.Forms.Label label1;
    }
}