namespace Microarea.EasyAttachment.UI.Controls
{
    partial class SOSDocStatusSearchToolStrip
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SOSDocStatusSearchToolStrip));
            this.TSContainer = new System.Windows.Forms.ToolStripContainer();
            this.TSSearch = new System.Windows.Forms.ToolStrip();
            this.TSSearchOptions = new System.Windows.Forms.ToolStripDropDownButton();
            this.TSMISend = new System.Windows.Forms.ToolStripMenuItem();
            this.TSMIResend = new System.Windows.Forms.ToolStripMenuItem();
            this.TSContainer.TopToolStripPanel.SuspendLayout();
            this.TSContainer.SuspendLayout();
            this.TSSearch.SuspendLayout();
            this.SuspendLayout();
            // 
            // TSContainer
            // 
            this.TSContainer.BottomToolStripPanelVisible = false;
            // 
            // TSContainer.ContentPanel
            // 
            resources.ApplyResources(this.TSContainer.ContentPanel, "TSContainer.ContentPanel");
            resources.ApplyResources(this.TSContainer, "TSContainer");
            this.TSContainer.LeftToolStripPanelVisible = false;
            this.TSContainer.Name = "TSContainer";
            this.TSContainer.RightToolStripPanelVisible = false;
            // 
            // TSContainer.TopToolStripPanel
            // 
            this.TSContainer.TopToolStripPanel.Controls.Add(this.TSSearch);
            // 
            // TSSearch
            // 
            resources.ApplyResources(this.TSSearch, "TSSearch");
            this.TSSearch.BackColor = System.Drawing.SystemColors.Control;
            this.TSSearch.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.TSSearch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSSearchOptions});
            this.TSSearch.Name = "TSSearch";
            this.TSSearch.Stretch = true;
            // 
            // TSSearchOptions
            // 
            this.TSSearchOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TSSearchOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSMISend,
            this.TSMIResend});
            resources.ApplyResources(this.TSSearchOptions, "TSSearchOptions");
            this.TSSearchOptions.Name = "TSSearchOptions";
            // 
            // TSMISend
            // 
            this.TSMISend.BackColor = System.Drawing.SystemColors.Control;
            this.TSMISend.Checked = true;
            this.TSMISend.CheckOnClick = true;
            this.TSMISend.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.TSMISend, "TSMISend");
            this.TSMISend.Name = "TSMISend";
            // 
            // TSMIResend
            // 
            this.TSMIResend.Checked = true;
            this.TSMIResend.CheckOnClick = true;
            this.TSMIResend.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.TSMIResend, "TSMIResend");
            this.TSMIResend.Name = "TSMIResend";
            // 
            // SOSDocStatusSearchToolStrip
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TSContainer);
            this.Name = "SOSDocStatusSearchToolStrip";
            this.TSContainer.TopToolStripPanel.ResumeLayout(false);
            this.TSContainer.ResumeLayout(false);
            this.TSContainer.PerformLayout();
            this.TSSearch.ResumeLayout(false);
            this.TSSearch.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStrip TSSearch;
        private System.Windows.Forms.ToolStripContainer TSContainer;
		private System.Windows.Forms.ToolStripDropDownButton TSSearchOptions;
		private System.Windows.Forms.ToolStripMenuItem TSMISend;
        private System.Windows.Forms.ToolStripMenuItem TSMIResend;

	}
}
