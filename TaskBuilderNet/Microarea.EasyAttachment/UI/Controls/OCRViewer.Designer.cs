namespace Microarea.EasyAttachment.UI.Controls
{
	partial class OCRViewer
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OCRViewer));
			this.TSContainer = new System.Windows.Forms.ToolStripContainer();
			this.GdPanel = new System.Windows.Forms.Panel();
			this.LblNoPreview = new System.Windows.Forms.Label();
			this.PbNoPreview = new System.Windows.Forms.PictureBox();
            this.GdViewerArea = new TBPicComponents.TBPicViewer();
			this.GdViewerToolStrip = new System.Windows.Forms.ToolStrip();
			this.ZoomInToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ZoomOutToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.OpenDocToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ToolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.PreviousToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.NextToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.ToolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.AddTagToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.DeleteTemplateToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.OCRContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.TSAddTag = new System.Windows.Forms.ToolStripMenuItem();
			this.TSContainer.ContentPanel.SuspendLayout();
			this.TSContainer.TopToolStripPanel.SuspendLayout();
			this.TSContainer.SuspendLayout();
			this.GdPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PbNoPreview)).BeginInit();
			this.GdViewerToolStrip.SuspendLayout();
			this.OCRContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// TSContainer
			// 
			this.TSContainer.BottomToolStripPanelVisible = false;
			// 
			// TSContainer.ContentPanel
			// 
			this.TSContainer.ContentPanel.Controls.Add(this.GdPanel);
			resources.ApplyResources(this.TSContainer.ContentPanel, "TSContainer.ContentPanel");
			resources.ApplyResources(this.TSContainer, "TSContainer");
			this.TSContainer.LeftToolStripPanelVisible = false;
			this.TSContainer.Name = "TSContainer";
			this.TSContainer.RightToolStripPanelVisible = false;
			// 
			// TSContainer.TopToolStripPanel
			// 
			this.TSContainer.TopToolStripPanel.Controls.Add(this.GdViewerToolStrip);
			this.TSContainer.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// GdPanel
			// 
			this.GdPanel.BackColor = System.Drawing.Color.Lavender;
			this.GdPanel.Controls.Add(this.LblNoPreview);
			this.GdPanel.Controls.Add(this.PbNoPreview);
			this.GdPanel.Controls.Add(this.GdViewerArea);
			resources.ApplyResources(this.GdPanel, "GdPanel");
			this.GdPanel.Name = "GdPanel";
			// 
			// LblNoPreview
			// 
			resources.ApplyResources(this.LblNoPreview, "LblNoPreview");
			this.LblNoPreview.BackColor = System.Drawing.Color.Lavender;
			this.LblNoPreview.Name = "LblNoPreview";
			// 
			// PbNoPreview
			// 
			this.PbNoPreview.Image = global::Microarea.EasyAttachment.Properties.Resources.Picture128x128_NoBorder;
			resources.ApplyResources(this.PbNoPreview, "PbNoPreview");
			this.PbNoPreview.Name = "PbNoPreview";
			this.PbNoPreview.TabStop = false;
			// 
			// GdViewerArea
			// 
			resources.ApplyResources(this.GdViewerArea, "GdViewerArea");
			this.GdViewerArea.AnimateGIF = false;
			this.GdViewerArea.BackColor = System.Drawing.Color.Lavender  ;
			this.GdViewerArea.BackgroundImage = null;
			this.GdViewerArea.ContinuousViewMode = true;
			this.GdViewerArea.Cursor = System.Windows.Forms.Cursors.Default;
            this.GdViewerArea.DisplayQuality = Microarea.TBPicComponents.TBPicDisplayQuality.DisplayQualityBicubicHQ;
			this.GdViewerArea.DisplayQualityAuto = true;
            this.GdViewerArea.DocumentAlignment = Microarea.TBPicComponents.TBPicViewerDocumentAlignment.DocumentAlignmentMiddleLeft;
            this.GdViewerArea.DocumentPosition = Microarea.TBPicComponents.TBPicViewerDocumentPosition.DocumentPositionMiddleLeft;
			this.GdViewerArea.EnableMenu = true;
			this.GdViewerArea.EnableMouseWheel = true;
			this.GdViewerArea.ForceScrollBars = false;
			this.GdViewerArea.ForceTemporaryModeForImage = false;
			this.GdViewerArea.ForceTemporaryModeForPDF = false;
			this.GdViewerArea.ForeColor = System.Drawing.Color.Black;
			this.GdViewerArea.Gamma = 1F;
			this.GdViewerArea.HQAnnotationRendering = true;
			this.GdViewerArea.IgnoreDocumentResolution = false;
			this.GdViewerArea.KeepDocumentPosition = false;
			this.GdViewerArea.LockViewer = false;
            this.GdViewerArea.MouseButtonForMouseMode = Microarea.TBPicComponents.TBPicMouseButton.MouseButtonLeft;
            this.GdViewerArea.MouseMode = Microarea.TBPicComponents.TBPicViewerMouseMode.MouseModeAreaSelection;
			this.GdViewerArea.MouseWheelMode = Microarea.TBPicComponents.TBPicViewerMouseWheelMode.MouseWheelModeVerticalScroll;
			this.GdViewerArea.Name = "GdViewerArea";
			this.GdViewerArea.OptimizeDrawingSpeed = true;
			this.GdViewerArea.PdfDisplayFormField = true;
			this.GdViewerArea.PdfEnableLinks = true;
			this.GdViewerArea.PdfShowDialogForPassword = true;
			this.GdViewerArea.RectBorderColor = System.Drawing.Color.Red;
			this.GdViewerArea.RectBorderSize = 2;
			this.GdViewerArea.RectIsEditable = false;
			this.GdViewerArea.RegionsAreEditable = false;
			this.GdViewerArea.ScrollBars = true;
			this.GdViewerArea.ScrollLargeChange = ((short)(50));
			this.GdViewerArea.ScrollSmallChange = ((short)(1));
			this.GdViewerArea.SilentMode = false;
			this.GdViewerArea.Zoom = 0.001D;
			this.GdViewerArea.ZoomCenterAtMousePosition = false;
            this.GdViewerArea.ZoomMode = Microarea.TBPicComponents.TBPicViewerZoomMode.ZoomModeFitToViewer;
			this.GdViewerArea.ZoomStep = 25;
			this.GdViewerArea.DragDrop += new System.Windows.Forms.DragEventHandler(this.GdViewerArea_DragDrop);
			this.GdViewerArea.DragEnter += new System.Windows.Forms.DragEventHandler(this.GdViewerArea_DragEnter);
			this.GdViewerArea.DragOver += new System.Windows.Forms.DragEventHandler(this.GdViewerArea_DragOver);
			this.GdViewerArea.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GdViewerArea_MouseMove);
			// 
			// GdViewerToolStrip
			// 
			this.GdViewerToolStrip.BackColor = System.Drawing.Color.Lavender;
			resources.ApplyResources(this.GdViewerToolStrip, "GdViewerToolStrip");
			this.GdViewerToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.GdViewerToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ZoomInToolStripButton,
            this.ZoomOutToolStripButton,
            this.ToolStripSeparator1,
            this.OpenDocToolStripButton,
            this.ToolStripSeparator3,
            this.PreviousToolStripButton,
            this.NextToolStripButton,
            this.ToolStripSeparator4,
            this.AddTagToolStripButton,
            this.DeleteTemplateToolStripButton});
			this.GdViewerToolStrip.Name = "GdViewerToolStrip";
			this.GdViewerToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// ZoomInToolStripButton
			// 
			this.ZoomInToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ZoomInToolStripButton.Image = global::Microarea.EasyAttachment.Properties.Resources.ZoomIn16x16;
			resources.ApplyResources(this.ZoomInToolStripButton, "ZoomInToolStripButton");
			this.ZoomInToolStripButton.Name = "ZoomInToolStripButton";
			this.ZoomInToolStripButton.Click += new System.EventHandler(this.ZoomInToolStripButton_Click);
			// 
			// ZoomOutToolStripButton
			// 
			this.ZoomOutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ZoomOutToolStripButton.Image = global::Microarea.EasyAttachment.Properties.Resources.ZoomOut16x16;
			resources.ApplyResources(this.ZoomOutToolStripButton, "ZoomOutToolStripButton");
			this.ZoomOutToolStripButton.Name = "ZoomOutToolStripButton";
			this.ZoomOutToolStripButton.Click += new System.EventHandler(this.ZoomOutToolStripButton_Click);
			// 
			// ToolStripSeparator1
			// 
			this.ToolStripSeparator1.Name = "ToolStripSeparator1";
			resources.ApplyResources(this.ToolStripSeparator1, "ToolStripSeparator1");
			// 
			// OpenDocToolStripButton
			// 
			this.OpenDocToolStripButton.AutoToolTip = false;
			this.OpenDocToolStripButton.Image = global::Microarea.EasyAttachment.Properties.Resources.OpenDoc16x16;
			resources.ApplyResources(this.OpenDocToolStripButton, "OpenDocToolStripButton");
			this.OpenDocToolStripButton.Name = "OpenDocToolStripButton";
			this.OpenDocToolStripButton.Click += new System.EventHandler(this.OpenDocToolStripButton_Click);
			// 
			// ToolStripSeparator3
			// 
			this.ToolStripSeparator3.Name = "ToolStripSeparator3";
			resources.ApplyResources(this.ToolStripSeparator3, "ToolStripSeparator3");
			// 
			// PreviousToolStripButton
			// 
			this.PreviousToolStripButton.AutoToolTip = false;
			this.PreviousToolStripButton.Image = global::Microarea.EasyAttachment.Properties.Resources.arrowup16x16;
			resources.ApplyResources(this.PreviousToolStripButton, "PreviousToolStripButton");
			this.PreviousToolStripButton.Name = "PreviousToolStripButton";
			this.PreviousToolStripButton.Click += new System.EventHandler(this.PreviousToolStripButton_Click);
			// 
			// NextToolStripButton
			// 
			this.NextToolStripButton.AutoToolTip = false;
			this.NextToolStripButton.Image = global::Microarea.EasyAttachment.Properties.Resources.arrowdown16x16;
			resources.ApplyResources(this.NextToolStripButton, "NextToolStripButton");
			this.NextToolStripButton.Name = "NextToolStripButton";
			this.NextToolStripButton.Click += new System.EventHandler(this.NextToolStripButton_Click);
			// 
			// ToolStripSeparator4
			// 
			this.ToolStripSeparator4.Name = "ToolStripSeparator4";
			resources.ApplyResources(this.ToolStripSeparator4, "ToolStripSeparator4");
			// 
			// AddTagToolStripButton
			// 
			this.AddTagToolStripButton.AutoToolTip = false;
			resources.ApplyResources(this.AddTagToolStripButton, "AddTagToolStripButton");
			this.AddTagToolStripButton.Image = global::Microarea.EasyAttachment.Properties.Resources.Add16x16;
			this.AddTagToolStripButton.Name = "AddTagToolStripButton";
			this.AddTagToolStripButton.Click += new System.EventHandler(this.AddTagToolStripButton_Click);
			// 
			// DeleteTemplateToolStripButton
			// 
			resources.ApplyResources(this.DeleteTemplateToolStripButton, "DeleteTemplateToolStripButton");
			this.DeleteTemplateToolStripButton.Image = global::Microarea.EasyAttachment.Properties.Resources.Delete16x16;
			this.DeleteTemplateToolStripButton.Name = "DeleteTemplateToolStripButton";
			this.DeleteTemplateToolStripButton.Click += new System.EventHandler(this.DeleteTemplateToolStripButton_Click);
			// 
			// OCRContextMenu
			// 
			this.OCRContextMenu.BackColor = System.Drawing.Color.Lavender;
			this.OCRContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSAddTag});
			this.OCRContextMenu.Name = "OCRContextMenu";
			resources.ApplyResources(this.OCRContextMenu, "OCRContextMenu");
			this.OCRContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.OCRContextMenu_Opening);
			// 
			// TSAddTag
			// 
			this.TSAddTag.Image = global::Microarea.EasyAttachment.Properties.Resources.Add16x16;
			this.TSAddTag.Name = "TSAddTag";
			resources.ApplyResources(this.TSAddTag, "TSAddTag");
			this.TSAddTag.Click += new System.EventHandler(this.TSAddTag_Click);
			// 
			// OCRViewer
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.TSContainer);
			this.Name = "OCRViewer";
			this.TSContainer.ContentPanel.ResumeLayout(false);
			this.TSContainer.TopToolStripPanel.ResumeLayout(false);
			this.TSContainer.TopToolStripPanel.PerformLayout();
			this.TSContainer.ResumeLayout(false);
			this.TSContainer.PerformLayout();
			this.GdPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PbNoPreview)).EndInit();
			this.GdViewerToolStrip.ResumeLayout(false);
			this.GdViewerToolStrip.PerformLayout();
			this.OCRContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel GdPanel;
		private System.Windows.Forms.Label LblNoPreview;
		private System.Windows.Forms.PictureBox PbNoPreview;
		internal TBPicComponents.TBPicViewer GdViewerArea;
		private System.Windows.Forms.ToolStrip GdViewerToolStrip;
		private System.Windows.Forms.ToolStripButton ZoomInToolStripButton;
		private System.Windows.Forms.ToolStripButton ZoomOutToolStripButton;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
		private System.Windows.Forms.ToolStripButton OpenDocToolStripButton;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator3;
		private System.Windows.Forms.ToolStripButton PreviousToolStripButton;
		private System.Windows.Forms.ToolStripButton NextToolStripButton;
		private System.Windows.Forms.ToolStripContainer TSContainer;
		private System.Windows.Forms.ContextMenuStrip OCRContextMenu;
		private System.Windows.Forms.ToolStripMenuItem TSAddTag;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator4;
		private System.Windows.Forms.ToolStripButton DeleteTemplateToolStripButton;
		private System.Windows.Forms.ToolStripButton AddTagToolStripButton;
	}
}
