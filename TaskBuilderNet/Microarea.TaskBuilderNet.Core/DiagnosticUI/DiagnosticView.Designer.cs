
namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
    partial class DiagnosticView
    {
        private System.ComponentModel.IContainer components = null;

        #region Dispose
        /// <summary>
        /// Dispose
        /// Clean up any resources being used.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiagnosticView));
			this.DiagnosticViewerToolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.MessagesList = new Microarea.TaskBuilderNet.Core.DiagnosticUI.MessagesListBox(this.components);
			this.FilterToolStrip = new System.Windows.Forms.ToolStrip();
			this.ViewErrorsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ViewWarningsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ViewInfoToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.SortToolStrip = new System.Windows.Forms.ToolStrip();
			this.SortByDateTimeToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.SortByTypeToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.NavigationToolStrip = new System.Windows.Forms.ToolStrip();
			this.PreviousToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.NextToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ActionToolStrip = new System.Windows.Forms.ToolStrip();
			this.DetailsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.FilterToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.SortToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.SaveToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.OpenToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.BtnClose = new System.Windows.Forms.Button();
			this.LblFileName = new System.Windows.Forms.Label();
			this.BottomPanel = new System.Windows.Forms.Panel();
			this.DiagnosticViewerToolStripContainer.ContentPanel.SuspendLayout();
			this.DiagnosticViewerToolStripContainer.TopToolStripPanel.SuspendLayout();
			this.DiagnosticViewerToolStripContainer.SuspendLayout();
			this.FilterToolStrip.SuspendLayout();
			this.SortToolStrip.SuspendLayout();
			this.NavigationToolStrip.SuspendLayout();
			this.ActionToolStrip.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// DiagnosticViewerToolStripContainer
			// 
			resources.ApplyResources(this.DiagnosticViewerToolStripContainer, "DiagnosticViewerToolStripContainer");
			// 
			// DiagnosticViewerToolStripContainer.ContentPanel
			// 
			this.DiagnosticViewerToolStripContainer.ContentPanel.Controls.Add(this.MessagesList);
			resources.ApplyResources(this.DiagnosticViewerToolStripContainer.ContentPanel, "DiagnosticViewerToolStripContainer.ContentPanel");
			this.DiagnosticViewerToolStripContainer.Name = "DiagnosticViewerToolStripContainer";
			// 
			// DiagnosticViewerToolStripContainer.TopToolStripPanel
			// 
			this.DiagnosticViewerToolStripContainer.TopToolStripPanel.Controls.Add(this.NavigationToolStrip);
			this.DiagnosticViewerToolStripContainer.TopToolStripPanel.Controls.Add(this.ActionToolStrip);
			this.DiagnosticViewerToolStripContainer.TopToolStripPanel.Controls.Add(this.SortToolStrip);
			this.DiagnosticViewerToolStripContainer.TopToolStripPanel.Controls.Add(this.FilterToolStrip);
			// 
			// MessagesList
			// 
			this.MessagesList.AllowDrop = true;
			resources.ApplyResources(this.MessagesList, "MessagesList");
			this.MessagesList.BackColor = System.Drawing.Color.White;
			this.MessagesList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.MessagesList.Name = "MessagesList";
			this.MessagesList.SelectedIndex = -1;
			this.MessagesList.SelectedItem = null;
			this.MessagesList.SelectedIndexChanged += new System.EventHandler(this.MessagesList_SelectedIndexChanged);
			// 
			// FilterToolStrip
			// 
			resources.ApplyResources(this.FilterToolStrip, "FilterToolStrip");
			this.FilterToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.FilterToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewErrorsToolStripButton,
            this.ViewWarningsToolStripButton,
            this.ViewInfoToolStripButton});
			this.FilterToolStrip.Name = "FilterToolStrip";
			// 
			// ViewErrorsToolStripButton
			// 
			this.ViewErrorsToolStripButton.Checked = true;
			this.ViewErrorsToolStripButton.CheckOnClick = true;
			this.ViewErrorsToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ViewErrorsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ViewErrorsToolStripButton, "ViewErrorsToolStripButton");
			this.ViewErrorsToolStripButton.Name = "ViewErrorsToolStripButton";
			this.ViewErrorsToolStripButton.CheckedChanged += new System.EventHandler(this.FilteredView_CheckedChanged);
			// 
			// ViewWarningsToolStripButton
			// 
			this.ViewWarningsToolStripButton.Checked = true;
			this.ViewWarningsToolStripButton.CheckOnClick = true;
			this.ViewWarningsToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ViewWarningsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ViewWarningsToolStripButton, "ViewWarningsToolStripButton");
			this.ViewWarningsToolStripButton.Name = "ViewWarningsToolStripButton";
			this.ViewWarningsToolStripButton.CheckedChanged += new System.EventHandler(this.FilteredView_CheckedChanged);
			// 
			// ViewInfoToolStripButton
			// 
			this.ViewInfoToolStripButton.Checked = true;
			this.ViewInfoToolStripButton.CheckOnClick = true;
			this.ViewInfoToolStripButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ViewInfoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ViewInfoToolStripButton, "ViewInfoToolStripButton");
			this.ViewInfoToolStripButton.Name = "ViewInfoToolStripButton";
			this.ViewInfoToolStripButton.CheckedChanged += new System.EventHandler(this.FilteredView_CheckedChanged);
			// 
			// SortToolStrip
			// 
			resources.ApplyResources(this.SortToolStrip, "SortToolStrip");
			this.SortToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.SortToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SortByDateTimeToolStripButton,
            this.SortByTypeToolStripButton});
			this.SortToolStrip.Name = "SortToolStrip";
			// 
			// SortByDateTimeToolStripButton
			// 
			this.SortByDateTimeToolStripButton.CheckOnClick = true;
			this.SortByDateTimeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.SortByDateTimeToolStripButton, "SortByDateTimeToolStripButton");
			this.SortByDateTimeToolStripButton.Name = "SortByDateTimeToolStripButton";
			this.SortByDateTimeToolStripButton.CheckedChanged += new System.EventHandler(this.FilteredView_CheckedChanged);
			// 
			// SortByTypeToolStripButton
			// 
			this.SortByTypeToolStripButton.CheckOnClick = true;
			this.SortByTypeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.SortByTypeToolStripButton, "SortByTypeToolStripButton");
			this.SortByTypeToolStripButton.Name = "SortByTypeToolStripButton";
			this.SortByTypeToolStripButton.CheckedChanged += new System.EventHandler(this.FilteredView_CheckedChanged);
			// 
			// NavigationToolStrip
			// 
			resources.ApplyResources(this.NavigationToolStrip, "NavigationToolStrip");
			this.NavigationToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.NavigationToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.PreviousToolStripButton,
            this.NextToolStripButton});
			this.NavigationToolStrip.Name = "NavigationToolStrip";
			// 
			// PreviousToolStripButton
			// 
			this.PreviousToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.PreviousToolStripButton, "PreviousToolStripButton");
			this.PreviousToolStripButton.Name = "PreviousToolStripButton";
			this.PreviousToolStripButton.Click += new System.EventHandler(this.PreviousToolStripButton_Click);
			// 
			// NextToolStripButton
			// 
			this.NextToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.NextToolStripButton, "NextToolStripButton");
			this.NextToolStripButton.Name = "NextToolStripButton";
			this.NextToolStripButton.Click += new System.EventHandler(this.NextToolStripButton_Click);
			// 
			// ActionToolStrip
			// 
			resources.ApplyResources(this.ActionToolStrip, "ActionToolStrip");
			this.ActionToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.ActionToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DetailsToolStripButton,
            this.FilterToolStripButton,
            this.SortToolStripButton,
            this.SaveToolStripButton,
            this.OpenToolStripButton});
			this.ActionToolStrip.Name = "ActionToolStrip";
			// 
			// DetailsToolStripButton
			// 
			this.DetailsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.DetailsToolStripButton, "DetailsToolStripButton");
			this.DetailsToolStripButton.Name = "DetailsToolStripButton";
			this.DetailsToolStripButton.Click += new System.EventHandler(this.DetailsToolStripButton_Click);
			// 
			// FilterToolStripButton
			// 
			this.FilterToolStripButton.CheckOnClick = true;
			this.FilterToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.FilterToolStripButton, "FilterToolStripButton");
			this.FilterToolStripButton.Name = "FilterToolStripButton";
			this.FilterToolStripButton.CheckedChanged += new System.EventHandler(this.FilterToolStripButton_CheckedChanged);
			// 
			// SortToolStripButton
			// 
			this.SortToolStripButton.CheckOnClick = true;
			this.SortToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.SortToolStripButton, "SortToolStripButton");
			this.SortToolStripButton.Name = "SortToolStripButton";
			this.SortToolStripButton.CheckedChanged += new System.EventHandler(this.SortToolStripButton_CheckedChanged);
			// 
			// SaveToolStripButton
			// 
			this.SaveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.SaveToolStripButton, "SaveToolStripButton");
			this.SaveToolStripButton.Name = "SaveToolStripButton";
			this.SaveToolStripButton.Click += new System.EventHandler(this.SaveToolStripButton_Click);
			// 
			// OpenToolStripButton
			// 
			this.OpenToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.OpenToolStripButton, "OpenToolStripButton");
			this.OpenToolStripButton.Name = "OpenToolStripButton";
			this.OpenToolStripButton.Click += new System.EventHandler(this.OpenToolStripButton_Click);
			// 
			// BtnClose
			// 
			resources.ApplyResources(this.BtnClose, "BtnClose");
			this.BtnClose.Name = "BtnClose";
			this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
			// 
			// LblFileName
			// 
			resources.ApplyResources(this.LblFileName, "LblFileName");
			this.LblFileName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblFileName.ForeColor = System.Drawing.Color.Blue;
			this.LblFileName.Name = "LblFileName";
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.BtnClose);
			this.BottomPanel.Controls.Add(this.LblFileName);
			resources.ApplyResources(this.BottomPanel, "BottomPanel");
			this.BottomPanel.Name = "BottomPanel";
			// 
			// DiagnosticView
			// 
			this.AcceptButton = this.BtnClose;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.DiagnosticViewerToolStripContainer);
			this.MinimizeBox = false;
			this.Name = "DiagnosticView";
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.DiagnosticViewerToolStripContainer.ContentPanel.ResumeLayout(false);
			this.DiagnosticViewerToolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.DiagnosticViewerToolStripContainer.TopToolStripPanel.PerformLayout();
			this.DiagnosticViewerToolStripContainer.ResumeLayout(false);
			this.DiagnosticViewerToolStripContainer.PerformLayout();
			this.FilterToolStrip.ResumeLayout(false);
			this.FilterToolStrip.PerformLayout();
			this.SortToolStrip.ResumeLayout(false);
			this.SortToolStrip.PerformLayout();
			this.NavigationToolStrip.ResumeLayout(false);
			this.NavigationToolStrip.PerformLayout();
			this.ActionToolStrip.ResumeLayout(false);
			this.ActionToolStrip.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.ToolStripContainer DiagnosticViewerToolStripContainer;
        private System.Windows.Forms.Button BtnClose;
        private MessagesListBox MessagesList;
        private System.Windows.Forms.Label LblFileName;
        private System.Windows.Forms.ToolStrip NavigationToolStrip;
        private System.Windows.Forms.ToolStripButton PreviousToolStripButton;
        private System.Windows.Forms.ToolStripButton NextToolStripButton;
        private System.Windows.Forms.ToolStrip ActionToolStrip;
        private System.Windows.Forms.ToolStripButton DetailsToolStripButton;
        private System.Windows.Forms.ToolStripButton FilterToolStripButton;
        private System.Windows.Forms.ToolStripButton SortToolStripButton;
        private System.Windows.Forms.ToolStripButton SaveToolStripButton;
        private System.Windows.Forms.ToolStripButton OpenToolStripButton;
        private System.Windows.Forms.ToolStrip FilterToolStrip;
        private System.Windows.Forms.ToolStripButton ViewErrorsToolStripButton;
        private System.Windows.Forms.ToolStripButton ViewWarningsToolStripButton;
        private System.Windows.Forms.ToolStripButton ViewInfoToolStripButton;
        private System.Windows.Forms.ToolStrip SortToolStrip;
        private System.Windows.Forms.ToolStripButton SortByDateTimeToolStripButton;
        private System.Windows.Forms.ToolStripButton SortByTypeToolStripButton;
		private System.Windows.Forms.Panel BottomPanel;
    }
}
