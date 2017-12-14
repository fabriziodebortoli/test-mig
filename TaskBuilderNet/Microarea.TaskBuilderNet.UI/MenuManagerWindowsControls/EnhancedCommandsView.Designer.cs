
namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
    partial class EnhancedCommandsView
    {

        private System.Windows.Forms.ImageList ToolbarImageList;

        private System.ComponentModel.IContainer components;

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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnhancedCommandsView));
			this.ViewToolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.ViewOptionsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ShowViewToolBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowCommandsDescriptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ShowReportsDatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ViewToolStrip = new System.Windows.Forms.ToolStrip();
			this.ShowDocumentsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ShowReportsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ShowBatchesToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ShowFunctionsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ShowExecutablesToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ShowTextsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ShowOfficeItemsToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ToolbarImageList = new System.Windows.Forms.ImageList(this.components);
			this.CommandsListBox = new CommandsListBox();
			this.ViewToolStripContainer.ContentPanel.SuspendLayout();
			this.ViewToolStripContainer.TopToolStripPanel.SuspendLayout();
			this.ViewToolStripContainer.SuspendLayout();
			this.ViewOptionsContextMenuStrip.SuspendLayout();
			this.ViewToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// ViewToolStripContainer
			// 
			// 
			// ViewToolStripContainer.ContentPanel
			// 
			this.ViewToolStripContainer.ContentPanel.Controls.Add(this.CommandsListBox);
			resources.ApplyResources(this.ViewToolStripContainer.ContentPanel, "ViewToolStripContainer.ContentPanel");
			resources.ApplyResources(this.ViewToolStripContainer, "ViewToolStripContainer");
			this.ViewToolStripContainer.Name = "ViewToolStripContainer";
			// 
			// ViewToolStripContainer.TopToolStripPanel
			// 
			this.ViewToolStripContainer.TopToolStripPanel.Controls.Add(this.ViewToolStrip);
			this.ViewToolStripContainer.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			// 
			// ViewOptionsContextMenuStrip
			// 
			this.ViewOptionsContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowViewToolBarToolStripMenuItem,
            this.ShowCommandsDescriptionsToolStripMenuItem,
            this.ShowReportsDatesToolStripMenuItem});
			this.ViewOptionsContextMenuStrip.Name = "ViewOptionsContextMenuStrip";
			resources.ApplyResources(this.ViewOptionsContextMenuStrip, "ViewOptionsContextMenuStrip");
			this.ViewOptionsContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ViewOptionsContextMenuStrip_Opening);
			// 
			// ShowViewToolBarToolStripMenuItem
			// 
			this.ShowViewToolBarToolStripMenuItem.CheckOnClick = true;
			resources.ApplyResources(this.ShowViewToolBarToolStripMenuItem, "ShowViewToolBarToolStripMenuItem");
			this.ShowViewToolBarToolStripMenuItem.Name = "ShowViewToolBarToolStripMenuItem";
			this.ShowViewToolBarToolStripMenuItem.Click += new System.EventHandler(this.ShowViewToolBarToolStripMenuItem_Click);
			// 
			// ShowCommandsDescriptionsToolStripMenuItem
			// 
			this.ShowCommandsDescriptionsToolStripMenuItem.CheckOnClick = true;
			resources.ApplyResources(this.ShowCommandsDescriptionsToolStripMenuItem, "ShowCommandsDescriptionsToolStripMenuItem");
			this.ShowCommandsDescriptionsToolStripMenuItem.Name = "ShowCommandsDescriptionsToolStripMenuItem";
			this.ShowCommandsDescriptionsToolStripMenuItem.Click += new System.EventHandler(this.ShowCommandsDescriptionsToolStripMenuItem_Click);
			// 
			// ShowReportsDatesToolStripMenuItem
			// 
			this.ShowReportsDatesToolStripMenuItem.CheckOnClick = true;
			resources.ApplyResources(this.ShowReportsDatesToolStripMenuItem, "ShowReportsDatesToolStripMenuItem");
			this.ShowReportsDatesToolStripMenuItem.Name = "ShowReportsDatesToolStripMenuItem";
			this.ShowReportsDatesToolStripMenuItem.Click += new System.EventHandler(this.ShowReportsDatesToolStripMenuItem_Click);
			// 
			// ViewToolStrip
			// 
			this.ViewToolStrip.AllowDrop = true;
			resources.ApplyResources(this.ViewToolStrip, "ViewToolStrip");
			this.ViewToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.ViewToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowDocumentsToolStripButton,
            this.ShowReportsToolStripButton,
            this.ShowBatchesToolStripButton,
            this.ShowFunctionsToolStripButton,
            this.ShowExecutablesToolStripButton,
            this.ShowTextsToolStripButton,
            this.ShowOfficeItemsToolStripButton});
			this.ViewToolStrip.Name = "ViewToolStrip";
			this.ViewToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			// 
			// ShowDocumentsToolStripButton
			// 
			this.ShowDocumentsToolStripButton.CheckOnClick = true;
			this.ShowDocumentsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ShowDocumentsToolStripButton, "ShowDocumentsToolStripButton");
			this.ShowDocumentsToolStripButton.Name = "ShowDocumentsToolStripButton";
			this.ShowDocumentsToolStripButton.CheckedChanged += new System.EventHandler(this.ViewToolStripItem_CheckedChanged);
			// 
			// ShowReportsToolStripButton
			// 
			this.ShowReportsToolStripButton.CheckOnClick = true;
			this.ShowReportsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ShowReportsToolStripButton, "ShowReportsToolStripButton");
			this.ShowReportsToolStripButton.Name = "ShowReportsToolStripButton";
			this.ShowReportsToolStripButton.CheckedChanged += new System.EventHandler(this.ViewToolStripItem_CheckedChanged);
			// 
			// ShowBatchesToolStripButton
			// 
			this.ShowBatchesToolStripButton.CheckOnClick = true;
			this.ShowBatchesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ShowBatchesToolStripButton, "ShowBatchesToolStripButton");
			this.ShowBatchesToolStripButton.Name = "ShowBatchesToolStripButton";
			this.ShowBatchesToolStripButton.CheckedChanged += new System.EventHandler(this.ViewToolStripItem_CheckedChanged);
			// 
			// ShowFunctionsToolStripButton
			// 
			this.ShowFunctionsToolStripButton.CheckOnClick = true;
			this.ShowFunctionsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ShowFunctionsToolStripButton, "ShowFunctionsToolStripButton");
			this.ShowFunctionsToolStripButton.Name = "ShowFunctionsToolStripButton";
			this.ShowFunctionsToolStripButton.CheckedChanged += new System.EventHandler(this.ViewToolStripItem_CheckedChanged);
			// 
			// ShowExecutablesToolStripButton
			// 
			this.ShowExecutablesToolStripButton.CheckOnClick = true;
			this.ShowExecutablesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ShowExecutablesToolStripButton, "ShowExecutablesToolStripButton");
			this.ShowExecutablesToolStripButton.Name = "ShowExecutablesToolStripButton";
			this.ShowExecutablesToolStripButton.CheckedChanged += new System.EventHandler(this.ViewToolStripItem_CheckedChanged);
			// 
			// ShowTextsToolStripButton
			// 
			this.ShowTextsToolStripButton.CheckOnClick = true;
			this.ShowTextsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ShowTextsToolStripButton, "ShowTextsToolStripButton");
			this.ShowTextsToolStripButton.Name = "ShowTextsToolStripButton";
			this.ShowTextsToolStripButton.CheckedChanged += new System.EventHandler(this.ViewToolStripItem_CheckedChanged);
			// 
			// ShowOfficeItemsToolStripButton
			// 
			this.ShowOfficeItemsToolStripButton.CheckOnClick = true;
			this.ShowOfficeItemsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.ShowOfficeItemsToolStripButton, "ShowOfficeItemsToolStripButton");
			this.ShowOfficeItemsToolStripButton.Name = "ShowOfficeItemsToolStripButton";
			this.ShowOfficeItemsToolStripButton.CheckedChanged += new System.EventHandler(this.ViewToolStripItem_CheckedChanged);
			// 
			// ToolbarImageList
			// 
			this.ToolbarImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ToolbarImageList.ImageStream")));
			this.ToolbarImageList.TransparentColor = System.Drawing.Color.Magenta;
			this.ToolbarImageList.Images.SetKeyName(0, "");
			this.ToolbarImageList.Images.SetKeyName(1, "");
			this.ToolbarImageList.Images.SetKeyName(2, "");
			this.ToolbarImageList.Images.SetKeyName(3, "");
			this.ToolbarImageList.Images.SetKeyName(4, "");
			this.ToolbarImageList.Images.SetKeyName(5, "");
			this.ToolbarImageList.Images.SetKeyName(6, "");
			// 
			// CommandsListBox
			// 
			this.CommandsListBox.AllUsersCommandForeColor = System.Drawing.Color.Blue;
			this.CommandsListBox.ContextMenuStrip = this.ViewOptionsContextMenuStrip;
			this.CommandsListBox.CurrentMenuNode = null;
			this.CommandsListBox.CurrentUserCommandForeColor = System.Drawing.Color.RoyalBlue;
			this.CommandsListBox.DescriptionsForeColor = System.Drawing.Color.Navy;
			resources.ApplyResources(this.CommandsListBox, "CommandsListBox");
			this.CommandsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.CommandsListBox.ItemHeight = 28;
			this.CommandsListBox.MenuXmlParser = null;
			this.CommandsListBox.Name = "CommandsListBox";
			this.CommandsListBox.ParentMenuIndependent = false;
			this.CommandsListBox.PathFinder = null;
			this.CommandsListBox.ReportDatesForeColor = System.Drawing.Color.RoyalBlue;
			this.CommandsListBox.SelectedCommand = null;
			this.CommandsListBox.ShowBatches = false;
			this.CommandsListBox.ShowDescriptions = false;
			this.CommandsListBox.ShowDocuments = false;
			this.CommandsListBox.ShowExecutables = false;
			this.CommandsListBox.ShowFunctions = false;
			this.CommandsListBox.ShowOfficeItems = false;
			this.CommandsListBox.ShowReports = false;
			this.CommandsListBox.ShowReportsDates = false;
			this.CommandsListBox.ShowStateImages = false;
			this.CommandsListBox.ShowTexts = false;
			this.CommandsListBox.ShowFlagsChanged += new System.EventHandler(this.CommandsListBox_ShowFlagsChanged);
			this.CommandsListBox.DoubleClick += new System.EventHandler(this.CommandsListBox_DoubleClick);
			this.CommandsListBox.SelectedCommandChanged += new System.EventHandler(this.CommandsListBox_SelectedCommandChanged);
			// 
			// EnhancedCommandsView
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.ViewToolStripContainer);
			resources.ApplyResources(this, "$this");
			this.Name = "EnhancedCommandsView";
			this.ViewToolStripContainer.ContentPanel.ResumeLayout(false);
			this.ViewToolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.ViewToolStripContainer.TopToolStripPanel.PerformLayout();
			this.ViewToolStripContainer.ResumeLayout(false);
			this.ViewToolStripContainer.PerformLayout();
			this.ViewOptionsContextMenuStrip.ResumeLayout(false);
			this.ViewToolStrip.ResumeLayout(false);
			this.ViewToolStrip.PerformLayout();
			this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.ToolStripContainer ViewToolStripContainer;
        private System.Windows.Forms.ToolStrip ViewToolStrip;
        private System.Windows.Forms.ToolStripButton ShowDocumentsToolStripButton;
        private System.Windows.Forms.ToolStripButton ShowReportsToolStripButton;
        private System.Windows.Forms.ToolStripButton ShowBatchesToolStripButton;
        private System.Windows.Forms.ToolStripButton ShowFunctionsToolStripButton;
        private System.Windows.Forms.ToolStripButton ShowExecutablesToolStripButton;
        private System.Windows.Forms.ToolStripButton ShowTextsToolStripButton;
        private System.Windows.Forms.ToolStripButton ShowOfficeItemsToolStripButton;
        private CommandsListBox CommandsListBox;
        private System.Windows.Forms.ContextMenuStrip ViewOptionsContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ShowViewToolBarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ShowCommandsDescriptionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ShowReportsDatesToolStripMenuItem;
    }
}
