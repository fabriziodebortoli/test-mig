using Microarea.TBPicComponents;
namespace Microarea.TaskBuilderNet.UI.PDFViewer
{
	partial class PDFViewerContainerForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PDFViewerContainerForm));
			this.ToolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.RightFormPanel = new System.Windows.Forms.Panel();
			this.SplitContainer2 = new System.Windows.Forms.SplitContainer();
			this.PDFThumbnail = new Microarea.TBPicComponents.TBThumbnailEx();
			this.PDFGdViewer = new Microarea.TBPicComponents.TBPicViewer();
			this.ToolStrip = new System.Windows.Forms.ToolStrip();
			this.printToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.upToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.downToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.pageNumberToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
			this.totToolStripLabel = new System.Windows.Forms.ToolStripLabel();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.plusTreeToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.minusToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.zoomToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.FindToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
			this.searchInPrevPToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.searchInNextPToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.MenuStrip = new System.Windows.Forms.MenuStrip();
			this.FileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PrintToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CloseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.GotoFirstPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.GotoPreviousPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.GotoNextPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.GotoLastPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gotoPageNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ZoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.ZoomINToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ZoomOUTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.FitToViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.FitToHeightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.FitToWidthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripContainer.ContentPanel.SuspendLayout();
			this.ToolStripContainer.TopToolStripPanel.SuspendLayout();
			this.ToolStripContainer.SuspendLayout();
			this.RightFormPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).BeginInit();
			this.SplitContainer2.Panel1.SuspendLayout();
			this.SplitContainer2.Panel2.SuspendLayout();
			this.SplitContainer2.SuspendLayout();
			this.ToolStrip.SuspendLayout();
			this.MenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// ToolStripContainer
			// 
			// 
			// ToolStripContainer.ContentPanel
			// 
			this.ToolStripContainer.ContentPanel.Controls.Add(this.RightFormPanel);
			resources.ApplyResources(this.ToolStripContainer.ContentPanel, "ToolStripContainer.ContentPanel");
			resources.ApplyResources(this.ToolStripContainer, "ToolStripContainer");
			this.ToolStripContainer.Name = "ToolStripContainer";
			// 
			// ToolStripContainer.TopToolStripPanel
			// 
			this.ToolStripContainer.TopToolStripPanel.Controls.Add(this.ToolStrip);
			// 
			// RightFormPanel
			// 
			resources.ApplyResources(this.RightFormPanel, "RightFormPanel");
			this.RightFormPanel.BackColor = System.Drawing.SystemColors.Control;
			this.RightFormPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.RightFormPanel.Controls.Add(this.SplitContainer2);
			this.RightFormPanel.Name = "RightFormPanel";
			// 
			// SplitContainer2
			// 
			resources.ApplyResources(this.SplitContainer2, "SplitContainer2");
			this.SplitContainer2.Name = "SplitContainer2";
			// 
			// SplitContainer2.Panel1
			// 
			this.SplitContainer2.Panel1.Controls.Add(this.PDFThumbnail);
			// 
			// SplitContainer2.Panel2
			// 
			this.SplitContainer2.Panel2.Controls.Add(this.PDFGdViewer);
			// 
			// PDFThumbnail
			// 
			this.PDFThumbnail.AllowDropFiles = false;
			this.PDFThumbnail.AllowMoveItems = false;
			resources.ApplyResources(this.PDFThumbnail, "PDFThumbnail");
			this.PDFThumbnail.BackColor = System.Drawing.Color.White;
			this.PDFThumbnail.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.PDFThumbnail.CheckBoxes = false;
			this.PDFThumbnail.CheckBoxesMarginLeft = 0;
			this.PDFThumbnail.CheckBoxesMarginTop = 0;
			this.PDFThumbnail.DisplayAnnotations = true;
			this.PDFThumbnail.HotTracking = true;
			this.PDFThumbnail.LockGdViewerEvents = false;
			this.PDFThumbnail.MultiSelect = false;
			this.PDFThumbnail.Name = "PDFThumbnail";
			this.PDFThumbnail.OwnDrop = false;
			this.PDFThumbnail.PauseThumbsLoading = false;
			this.PDFThumbnail.PreloadAllItems = true;
			this.PDFThumbnail.RotateExif = true;
			this.PDFThumbnail.SelectedThumbnailBackColor = System.Drawing.SystemColors.ActiveCaption;
			this.PDFThumbnail.ShowText = true;
			this.PDFThumbnail.TextMarginLeft = 0;
			this.PDFThumbnail.TextMarginTop = 0;
			this.PDFThumbnail.ThumbnailBackColor = System.Drawing.Color.Transparent;
			this.PDFThumbnail.ThumbnailBorder = true;
			this.PDFThumbnail.ThumbnailSize = new System.Drawing.Size(64, 64);
			this.PDFThumbnail.ThumbnailSpacing = new System.Drawing.Size(0, 0);
			// 
			// PDFGdViewer
			// 
			this.PDFGdViewer.AnimateGIF = false;
			resources.ApplyResources(this.PDFGdViewer, "PDFGdViewer");
			this.PDFGdViewer.BackColor = System.Drawing.Color.Gainsboro;
			this.PDFGdViewer.ContinuousViewMode = true;
			this.PDFGdViewer.Cursor = System.Windows.Forms.Cursors.Default;
			this.PDFGdViewer.DisplayQuality = Microarea.TBPicComponents.TBPicDisplayQuality.DisplayQualityBicubicHQ;
			this.PDFGdViewer.DisplayQualityAuto = true;
			this.PDFGdViewer.DocumentAlignment = Microarea.TBPicComponents.TBPicViewerDocumentAlignment.DocumentAlignmentTopLeft;
			this.PDFGdViewer.DocumentPosition = Microarea.TBPicComponents.TBPicViewerDocumentPosition.DocumentPositionMiddleCenter;
			this.PDFGdViewer.EnabledProgressBar = true;
			this.PDFGdViewer.EnableMenu = false;
			this.PDFGdViewer.EnableMouseWheel = true;
			this.PDFGdViewer.ForceScrollBars = true;
			this.PDFGdViewer.ForceTemporaryModeForImage = true;
			this.PDFGdViewer.ForceTemporaryModeForPDF = true;
			this.PDFGdViewer.ForeColor = System.Drawing.Color.Black;
			this.PDFGdViewer.Gamma = 1F;
			this.PDFGdViewer.HQAnnotationRendering = true;
			this.PDFGdViewer.IgnoreDocumentResolution = false;
			this.PDFGdViewer.KeepDocumentPosition = true;
			this.PDFGdViewer.LockViewer = false;
			this.PDFGdViewer.MouseButtonForMouseMode = Microarea.TBPicComponents.TBPicMouseButton.MouseButtonLeft;
			this.PDFGdViewer.MouseMode = Microarea.TBPicComponents.TBPicViewerMouseMode.MouseModePan;
			this.PDFGdViewer.MouseWheelMode = Microarea.TBPicComponents.TBPicViewerMouseWheelMode.MouseWheelModeZoom;
			this.PDFGdViewer.Name = "PDFGdViewer";
			this.PDFGdViewer.OptimizeDrawingSpeed = true;
			this.PDFGdViewer.PdfDisplayFormField = false;
			this.PDFGdViewer.PdfEnableLinks = false;
			this.PDFGdViewer.PdfShowDialogForPassword = false;
			this.PDFGdViewer.RectBorderColor = System.Drawing.Color.Black;
			this.PDFGdViewer.RectBorderSize = 1;
			this.PDFGdViewer.RectIsEditable = true;
			this.PDFGdViewer.RegionsAreEditable = false;
			this.PDFGdViewer.ScrollBars = true;
			this.PDFGdViewer.ScrollLargeChange = ((short)(10));
			this.PDFGdViewer.ScrollSmallChange = ((short)(1));
			this.PDFGdViewer.SilentMode = false;
			this.PDFGdViewer.Zoom = 0.001D;
			this.PDFGdViewer.ZoomCenterAtMousePosition = false;
			this.PDFGdViewer.ZoomMode = Microarea.TBPicComponents.TBPicViewerZoomMode.ZoomModeFitToViewer;
			this.PDFGdViewer.ZoomStep = 25;
			this.PDFGdViewer.PageChanged += new System.EventHandler(this.PDFGdViewer_PageChanged);
			// 
			// ToolStrip
			// 
			resources.ApplyResources(this.ToolStrip, "ToolStrip");
			this.ToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
			this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.printToolStripButton,
            this.toolStripSeparator1,
            this.upToolStripButton,
            this.downToolStripButton,
            this.pageNumberToolStripTextBox,
            this.totToolStripLabel,
            this.toolStripSeparator2,
            this.plusTreeToolStripButton,
            this.minusToolStripButton,
            this.zoomToolStripTextBox,
            this.toolStripSeparator3,
            this.toolStripLabel1,
            this.FindToolStripTextBox,
            this.searchInPrevPToolStripButton,
            this.searchInNextPToolStripButton});
			this.ToolStrip.Name = "ToolStrip";
			// 
			// printToolStripButton
			// 
			this.printToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.printToolStripButton, "printToolStripButton");
			this.printToolStripButton.Name = "printToolStripButton";
			this.printToolStripButton.Click += new System.EventHandler(this.printToolStripButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// upToolStripButton
			// 
			this.upToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.upToolStripButton, "upToolStripButton");
			this.upToolStripButton.Name = "upToolStripButton";
			this.upToolStripButton.Click += new System.EventHandler(this.upToolStripButton_Click);
			// 
			// downToolStripButton
			// 
			this.downToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.downToolStripButton, "downToolStripButton");
			this.downToolStripButton.Name = "downToolStripButton";
			this.downToolStripButton.Click += new System.EventHandler(this.downToolStripButton_Click);
			// 
			// pageNumberToolStripTextBox
			// 
			this.pageNumberToolStripTextBox.Name = "pageNumberToolStripTextBox";
			resources.ApplyResources(this.pageNumberToolStripTextBox, "pageNumberToolStripTextBox");
			this.pageNumberToolStripTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.pageNumberToolStripTextBox_KeyPress);
			// 
			// totToolStripLabel
			// 
			resources.ApplyResources(this.totToolStripLabel, "totToolStripLabel");
			this.totToolStripLabel.Name = "totToolStripLabel";
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			// 
			// plusTreeToolStripButton
			// 
			this.plusTreeToolStripButton.CheckOnClick = true;
			this.plusTreeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.plusTreeToolStripButton, "plusTreeToolStripButton");
			this.plusTreeToolStripButton.Name = "plusTreeToolStripButton";
			this.plusTreeToolStripButton.Click += new System.EventHandler(this.plusTreeToolStripButton_Click);
			// 
			// minusToolStripButton
			// 
			this.minusToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.minusToolStripButton, "minusToolStripButton");
			this.minusToolStripButton.Name = "minusToolStripButton";
			this.minusToolStripButton.Click += new System.EventHandler(this.minusToolStripButton_Click);
			// 
			// zoomToolStripTextBox
			// 
			resources.ApplyResources(this.zoomToolStripTextBox, "zoomToolStripTextBox");
			this.zoomToolStripTextBox.Name = "zoomToolStripTextBox";
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
			// 
			// FindToolStripTextBox
			// 
			this.FindToolStripTextBox.Name = "FindToolStripTextBox";
			resources.ApplyResources(this.FindToolStripTextBox, "FindToolStripTextBox");
			this.FindToolStripTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindToolStripTextBox_KeyPress);
			// 
			// searchInPrevPToolStripButton
			// 
			this.searchInPrevPToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.searchInPrevPToolStripButton, "searchInPrevPToolStripButton");
			this.searchInPrevPToolStripButton.Name = "searchInPrevPToolStripButton";
			this.searchInPrevPToolStripButton.Click += new System.EventHandler(this.searchInPrevPToolStripButton_Click);
			// 
			// searchInNextPToolStripButton
			// 
			this.searchInNextPToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.searchInNextPToolStripButton, "searchInNextPToolStripButton");
			this.searchInNextPToolStripButton.Name = "searchInNextPToolStripButton";
			this.searchInNextPToolStripButton.Click += new System.EventHandler(this.searchInNextPToolStripButton_Click);
			// 
			// MenuStrip
			// 
			resources.ApplyResources(this.MenuStrip, "MenuStrip");
			this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileToolStripMenuItem,
            this.PageToolStripMenuItem,
            this.ZoomToolStripMenuItem});
			this.MenuStrip.Name = "MenuStrip";
			// 
			// FileToolStripMenuItem
			// 
			this.FileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenToolStripMenuItem,
            this.PrintToolStripMenuItem,
            this.SaveToolStripMenuItem,
            this.CloseToolStripMenuItem,
            this.ExitToolStripMenuItem});
			this.FileToolStripMenuItem.Name = "FileToolStripMenuItem";
			resources.ApplyResources(this.FileToolStripMenuItem, "FileToolStripMenuItem");
			// 
			// OpenToolStripMenuItem
			// 
			this.OpenToolStripMenuItem.Name = "OpenToolStripMenuItem";
			resources.ApplyResources(this.OpenToolStripMenuItem, "OpenToolStripMenuItem");
			this.OpenToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
			// 
			// PrintToolStripMenuItem
			// 
			resources.ApplyResources(this.PrintToolStripMenuItem, "PrintToolStripMenuItem");
			this.PrintToolStripMenuItem.Name = "PrintToolStripMenuItem";
			this.PrintToolStripMenuItem.Click += new System.EventHandler(this.ToolStripMenuItem10_Click);
			// 
			// SaveToolStripMenuItem
			// 
			resources.ApplyResources(this.SaveToolStripMenuItem, "SaveToolStripMenuItem");
			this.SaveToolStripMenuItem.Name = "SaveToolStripMenuItem";
			// 
			// CloseToolStripMenuItem
			// 
			resources.ApplyResources(this.CloseToolStripMenuItem, "CloseToolStripMenuItem");
			this.CloseToolStripMenuItem.Name = "CloseToolStripMenuItem";
			this.CloseToolStripMenuItem.Click += new System.EventHandler(this.CloseToolStripMenuItem_Click);
			// 
			// ExitToolStripMenuItem
			// 
			this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
			resources.ApplyResources(this.ExitToolStripMenuItem, "ExitToolStripMenuItem");
			this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
			// 
			// PageToolStripMenuItem
			// 
			this.PageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GotoFirstPageToolStripMenuItem,
            this.GotoPreviousPageToolStripMenuItem,
            this.GotoNextPageToolStripMenuItem,
            this.GotoLastPageToolStripMenuItem,
            this.gotoPageNumberToolStripMenuItem});
			resources.ApplyResources(this.PageToolStripMenuItem, "PageToolStripMenuItem");
			this.PageToolStripMenuItem.Name = "PageToolStripMenuItem";
			// 
			// GotoFirstPageToolStripMenuItem
			// 
			this.GotoFirstPageToolStripMenuItem.Name = "GotoFirstPageToolStripMenuItem";
			resources.ApplyResources(this.GotoFirstPageToolStripMenuItem, "GotoFirstPageToolStripMenuItem");
			this.GotoFirstPageToolStripMenuItem.Click += new System.EventHandler(this.GotoFirstPageToolStripMenuItem_Click);
			// 
			// GotoPreviousPageToolStripMenuItem
			// 
			this.GotoPreviousPageToolStripMenuItem.Name = "GotoPreviousPageToolStripMenuItem";
			resources.ApplyResources(this.GotoPreviousPageToolStripMenuItem, "GotoPreviousPageToolStripMenuItem");
			this.GotoPreviousPageToolStripMenuItem.Click += new System.EventHandler(this.GotoPreviousPageToolStripMenuItem_Click);
			// 
			// GotoNextPageToolStripMenuItem
			// 
			this.GotoNextPageToolStripMenuItem.Name = "GotoNextPageToolStripMenuItem";
			resources.ApplyResources(this.GotoNextPageToolStripMenuItem, "GotoNextPageToolStripMenuItem");
			this.GotoNextPageToolStripMenuItem.Click += new System.EventHandler(this.GotoNextPageToolStripMenuItem_Click);
			// 
			// GotoLastPageToolStripMenuItem
			// 
			this.GotoLastPageToolStripMenuItem.Name = "GotoLastPageToolStripMenuItem";
			resources.ApplyResources(this.GotoLastPageToolStripMenuItem, "GotoLastPageToolStripMenuItem");
			this.GotoLastPageToolStripMenuItem.Click += new System.EventHandler(this.GotoLastPageToolStripMenuItem_Click);
			// 
			// gotoPageNumberToolStripMenuItem
			// 
			this.gotoPageNumberToolStripMenuItem.Name = "gotoPageNumberToolStripMenuItem";
			resources.ApplyResources(this.gotoPageNumberToolStripMenuItem, "gotoPageNumberToolStripMenuItem");
			this.gotoPageNumberToolStripMenuItem.Click += new System.EventHandler(this.gotoPageNumberToolStripMenuItem_Click);
			// 
			// ZoomToolStripMenuItem
			// 
			this.ZoomToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem6,
            this.ToolStripMenuItem2,
            this.ToolStripMenuItem3,
            this.ToolStripMenuItem4,
            this.ToolStripMenuItem1,
            this.ZoomINToolStripMenuItem,
            this.ZoomOUTToolStripMenuItem,
            this.ToolStripMenuItem5,
            this.FitToViewerToolStripMenuItem,
            this.FitToHeightToolStripMenuItem,
            this.FitToWidthToolStripMenuItem});
			resources.ApplyResources(this.ZoomToolStripMenuItem, "ZoomToolStripMenuItem");
			this.ZoomToolStripMenuItem.Name = "ZoomToolStripMenuItem";
			// 
			// ToolStripMenuItem6
			// 
			this.ToolStripMenuItem6.Name = "ToolStripMenuItem6";
			resources.ApplyResources(this.ToolStripMenuItem6, "ToolStripMenuItem6");
			this.ToolStripMenuItem6.Click += new System.EventHandler(this.ToolStripMenuItem6_Click);
			// 
			// ToolStripMenuItem2
			// 
			this.ToolStripMenuItem2.Name = "ToolStripMenuItem2";
			resources.ApplyResources(this.ToolStripMenuItem2, "ToolStripMenuItem2");
			this.ToolStripMenuItem2.Click += new System.EventHandler(this.ToolStripMenuItem2_Click);
			// 
			// ToolStripMenuItem3
			// 
			this.ToolStripMenuItem3.Name = "ToolStripMenuItem3";
			resources.ApplyResources(this.ToolStripMenuItem3, "ToolStripMenuItem3");
			this.ToolStripMenuItem3.Click += new System.EventHandler(this.ToolStripMenuItem3_Click);
			// 
			// ToolStripMenuItem4
			// 
			this.ToolStripMenuItem4.Name = "ToolStripMenuItem4";
			resources.ApplyResources(this.ToolStripMenuItem4, "ToolStripMenuItem4");
			this.ToolStripMenuItem4.Click += new System.EventHandler(this.ToolStripMenuItem4_Click);
			// 
			// ToolStripMenuItem1
			// 
			this.ToolStripMenuItem1.Name = "ToolStripMenuItem1";
			resources.ApplyResources(this.ToolStripMenuItem1, "ToolStripMenuItem1");
			// 
			// ZoomINToolStripMenuItem
			// 
			this.ZoomINToolStripMenuItem.Name = "ZoomINToolStripMenuItem";
			resources.ApplyResources(this.ZoomINToolStripMenuItem, "ZoomINToolStripMenuItem");
			this.ZoomINToolStripMenuItem.Click += new System.EventHandler(this.ZoomINToolStripMenuItem_Click);
			// 
			// ZoomOUTToolStripMenuItem
			// 
			this.ZoomOUTToolStripMenuItem.Name = "ZoomOUTToolStripMenuItem";
			resources.ApplyResources(this.ZoomOUTToolStripMenuItem, "ZoomOUTToolStripMenuItem");
			this.ZoomOUTToolStripMenuItem.Click += new System.EventHandler(this.ZoomOUTToolStripMenuItem_Click);
			// 
			// ToolStripMenuItem5
			// 
			this.ToolStripMenuItem5.Name = "ToolStripMenuItem5";
			resources.ApplyResources(this.ToolStripMenuItem5, "ToolStripMenuItem5");
			// 
			// FitToViewerToolStripMenuItem
			// 
			this.FitToViewerToolStripMenuItem.Name = "FitToViewerToolStripMenuItem";
			resources.ApplyResources(this.FitToViewerToolStripMenuItem, "FitToViewerToolStripMenuItem");
			this.FitToViewerToolStripMenuItem.Click += new System.EventHandler(this.FitToViewerToolStripMenuItem_Click);
			// 
			// FitToHeightToolStripMenuItem
			// 
			this.FitToHeightToolStripMenuItem.Name = "FitToHeightToolStripMenuItem";
			resources.ApplyResources(this.FitToHeightToolStripMenuItem, "FitToHeightToolStripMenuItem");
			this.FitToHeightToolStripMenuItem.Click += new System.EventHandler(this.FitToHeightToolStripMenuItem_Click);
			// 
			// FitToWidthToolStripMenuItem
			// 
			this.FitToWidthToolStripMenuItem.Name = "FitToWidthToolStripMenuItem";
			resources.ApplyResources(this.FitToWidthToolStripMenuItem, "FitToWidthToolStripMenuItem");
			this.FitToWidthToolStripMenuItem.Click += new System.EventHandler(this.FitToWidthToolStripMenuItem_Click);
			// 
			// PDFViewerContainerForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ToolStripContainer);
			this.Controls.Add(this.MenuStrip);
			this.Name = "PDFViewerContainerForm";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.ToolStripContainer.ContentPanel.ResumeLayout(false);
			this.ToolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.ToolStripContainer.TopToolStripPanel.PerformLayout();
			this.ToolStripContainer.ResumeLayout(false);
			this.ToolStripContainer.PerformLayout();
			this.RightFormPanel.ResumeLayout(false);
			this.SplitContainer2.Panel1.ResumeLayout(false);
			this.SplitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).EndInit();
			this.SplitContainer2.ResumeLayout(false);
			this.ToolStrip.ResumeLayout(false);
			this.ToolStrip.PerformLayout();
			this.MenuStrip.ResumeLayout(false);
			this.MenuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal System.Windows.Forms.MenuStrip MenuStrip;
		internal System.Windows.Forms.ToolStripMenuItem FileToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem OpenToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem PrintToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem SaveToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem CloseToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem PageToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem GotoFirstPageToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem GotoPreviousPageToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem GotoNextPageToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem GotoLastPageToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem ZoomToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem6;
		internal System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem2;
		internal System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem3;
		internal System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem4;
		internal System.Windows.Forms.ToolStripSeparator ToolStripMenuItem1;
		internal System.Windows.Forms.ToolStripMenuItem ZoomINToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem ZoomOUTToolStripMenuItem;
		internal System.Windows.Forms.ToolStripSeparator ToolStripMenuItem5;
		internal System.Windows.Forms.ToolStripMenuItem FitToViewerToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem FitToHeightToolStripMenuItem;
		internal System.Windows.Forms.ToolStripMenuItem FitToWidthToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem gotoPageNumberToolStripMenuItem;
		private System.Windows.Forms.ToolStripContainer ToolStripContainer;
		public System.Windows.Forms.Panel RightFormPanel;
		private System.Windows.Forms.ToolStrip ToolStrip;
		private System.Windows.Forms.ToolStripButton printToolStripButton;
		private System.Windows.Forms.ToolStripButton upToolStripButton;
		private System.Windows.Forms.ToolStripButton downToolStripButton;
		private System.Windows.Forms.ToolStripButton minusToolStripButton;
		private System.Windows.Forms.ToolStripButton plusTreeToolStripButton;
		private System.Windows.Forms.SplitContainer SplitContainer2;
		internal TBThumbnailEx PDFThumbnail;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripTextBox pageNumberToolStripTextBox;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripTextBox zoomToolStripTextBox;
		private TBPicViewer PDFGdViewer;
		private System.Windows.Forms.ToolStripLabel totToolStripLabel;
		private System.Windows.Forms.ToolStripTextBox FindToolStripTextBox;
		private System.Windows.Forms.ToolStripButton searchInPrevPToolStripButton;
		private System.Windows.Forms.ToolStripButton searchInNextPToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
	}
}

