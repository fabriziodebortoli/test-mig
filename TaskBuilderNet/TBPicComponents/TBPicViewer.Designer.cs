namespace Microarea.TBPicComponents
{
    partial class TBPicViewer
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
			this.gdViewer = new GdPicture12.GdViewer();
			this.SuspendLayout();
			// 
			// gdViewer
			// 
			this.gdViewer.AllowDropFile = false;
			this.gdViewer.AnimateGIF = false;
			this.gdViewer.AnnotationDropShadow = false;
			this.gdViewer.BackColor = System.Drawing.Color.Lavender;
			this.gdViewer.BackgroundImage = null;
			this.gdViewer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.gdViewer.ContinuousViewMode = true;
			this.gdViewer.Cursor = System.Windows.Forms.Cursors.Default;
			this.gdViewer.DisplayQuality = GdPicture12.DisplayQuality.DisplayQualityBicubicHQ;
			this.gdViewer.DisplayQualityAuto = false;
			this.gdViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gdViewer.DocumentAlignment = GdPicture12.ViewerDocumentAlignment.DocumentAlignmentMiddleCenter;
			this.gdViewer.DocumentPosition = GdPicture12.ViewerDocumentPosition.DocumentPositionMiddleCenter;
			this.gdViewer.EnabledProgressBar = true;
			this.gdViewer.EnableMenu = true;
			this.gdViewer.EnableMouseWheel = true;
			this.gdViewer.ForceScrollBars = false;
			this.gdViewer.ForceTemporaryModeForImage = false;
			this.gdViewer.ForceTemporaryModeForPDF = false;
			this.gdViewer.ForeColor = System.Drawing.Color.Black;
			this.gdViewer.Gamma = 1F;
			this.gdViewer.HQAnnotationRendering = true;
			this.gdViewer.IgnoreDocumentResolution = false;
			this.gdViewer.KeepDocumentPosition = false;
			this.gdViewer.Location = new System.Drawing.Point(0, 0);
			this.gdViewer.LockViewer = false;
			this.gdViewer.MagnifierHeight = 90;
			this.gdViewer.MagnifierWidth = 160;
			this.gdViewer.MagnifierZoomX = 2F;
			this.gdViewer.MagnifierZoomY = 2F;
			this.gdViewer.MouseButtonForMouseMode = GdPicture12.MouseButton.MouseButtonLeft;
			this.gdViewer.MouseMode = GdPicture12.ViewerMouseMode.MouseModePan;
			this.gdViewer.MouseWheelMode = GdPicture12.ViewerMouseWheelMode.MouseWheelModeZoom;
			this.gdViewer.Name = "gdViewer";
			this.gdViewer.DisplayQualityAuto = false;
			this.gdViewer.PdfDisplayFormField = true;
			this.gdViewer.PdfEnableFileLinks = true;
			this.gdViewer.PdfEnableLinks = true;
			this.gdViewer.PdfShowDialogForPassword = true;
			this.gdViewer.RectBorderColor = System.Drawing.Color.Black;
			this.gdViewer.RectBorderSize = 1;
			this.gdViewer.RectIsEditable = true;
			this.gdViewer.RegionsAreEditable = true;
			this.gdViewer.ScrollBars = true;
			this.gdViewer.ScrollLargeChange = ((short)(50));
			this.gdViewer.ScrollSmallChange = ((short)(1));
			this.gdViewer.SilentMode = true;
			this.gdViewer.Size = new System.Drawing.Size(156, 166);
			this.gdViewer.TabIndex = 0;
			this.gdViewer.Zoom = 1D;
			this.gdViewer.ZoomCenterAtMousePosition = false;
			this.gdViewer.ZoomMode = GdPicture12.ViewerZoomMode.ZoomMode100;
			this.gdViewer.ZoomStep = 25;
			this.gdViewer.DropFile += new GdPicture12.GdViewer.DropFileEventHandler(this.gdViewer_DropFile);
			this.gdViewer.SavingProgress += new GdPicture12.GdViewer.SavingProgressEventHandler(this.gdViewer_SavingProgress);
			this.gdViewer.OnCustomAnnotationPaint += new GdPicture12.GdViewer.OnCustomAnnotationPaintEventHandler(this.gdViewer_OnCustomAnnotationPaint);
			this.gdViewer.AnnotationStartEditingText += new GdPicture12.GdViewer.AnnotationStartEditingTextEventHandler(this.gdViewer_AnnotationStartEditingText);
			this.gdViewer.AnnotationEndEditingText += new GdPicture12.GdViewer.AnnotationEndEditingTextEventHandler(this.gdViewer_AnnotationEndEditingText);
			this.gdViewer.AnnotationSelected += new GdPicture12.GdViewer.AnnotationSelectedEventHandler(this.gdViewer_AnnotationSelected);
			this.gdViewer.AnnotationClicked += new GdPicture12.GdViewer.AnnotationClickedEventHandler(this.gdViewer_AnnotationClicked);
			this.gdViewer.BeforeAnnotationAddedByUser += new GdPicture12.GdViewer.BeforeAnnotationAddedByUserEventHandler(this.gdViewer_BeforeAnnotationAddedByUser);
			this.gdViewer.AnnotationAddedByUser += new GdPicture12.GdViewer.AnnotationAddedByUserEventHandler(this.gdViewer_AnnotationAddedByUser);
			this.gdViewer.AnnotationRotated += new GdPicture12.GdViewer.AnnotationRotatedEventHandler(this.gdViewer_AnnotationRotated);
			this.gdViewer.AnnotationMoved += new GdPicture12.GdViewer.AnnotationMovedEventHandler(this.gdViewer_AnnotationMoved);
			this.gdViewer.AnnotationResized += new GdPicture12.GdViewer.AnnotationResizedEventHandler(this.gdViewer_AnnotationResized);
			this.gdViewer.PdfPasswordRequest += new GdPicture12.GdViewer.PdfPasswordRequestEventHandler(this.gdViewer_PdfPasswordRequest);
			this.gdViewer.PdfFileNavigation += new GdPicture12.GdViewer.PdfFileNavigationEventHandler(this.gdViewer_PdfFileNavigation);
			this.gdViewer.PdfUriNavigation += new GdPicture12.GdViewer.PdfUriNavigationEventHandler(this.gdViewer_PdfUriNavigation);
			this.gdViewer.BeforePrintPage += new GdPicture12.GdViewer.BeforePrintPageEventHandler(this.gdViewer_BeforePrintPage);
			this.gdViewer.AfterPrintPage += new GdPicture12.GdViewer.AfterPrintPageEventHandler(this.gdViewer_AfterPrintPage);
			this.gdViewer.DataReceived += new GdPicture12.GdViewer.DataReceivedEventHandler(this.gdViewer_DataReceived);
			this.gdViewer.TransferEnded += new GdPicture12.GdViewer.TransferEndedEventHandler(this.gdViewer_TransferEnded);
			this.gdViewer.AfterZoomChange += new GdPicture12.GdViewer.AfterZoomChangeEventHandler(this.gdViewer_AfterZoomChange);
			this.gdViewer.BeforeZoomChange += new GdPicture12.GdViewer.BeforeZoomChangeEventHandler(this.gdViewer_BeforeZoomChange);
			this.gdViewer.ScrollViewer += new GdPicture12.GdViewer.ScrollViewerEventHandler(this.gdViewer_ScrollViewer);
			this.gdViewer.RectEditedByUser += new GdPicture12.GdViewer.RectEditedByUserEventHandler(this.gdViewer_RectEditedByUser);
			this.gdViewer.RegionEditedByUser += new GdPicture12.GdViewer.RegionEditedByUserEventHandler(this.gdViewer_RegionEditedByUser);
			this.gdViewer.RegionSelectedByUser += new GdPicture12.GdViewer.RegionSelectedByUserEventHandler(this.gdViewer_RegionSelectedByUser);
			this.gdViewer.Rotation += new GdPicture12.GdViewer.RotationEventHandler(this.gdViewer_Rotation);
			this.gdViewer.BeforeRotation += new GdPicture12.GdViewer.BeforeRotationEventHandler(this.gdViewer_BeforeRotation);
			this.gdViewer.PageChanged += new GdPicture12.GdViewer.PageChangedEventHandler(this.gdViewer_PageChanged);
			this.gdViewer.BeforeDocumentChange += new GdPicture12.GdViewer.BeforeDocumentChangeEventHandler(this.gdViewer_BeforeDocumentChange);
			this.gdViewer.AfterDocumentChange += new GdPicture12.GdViewer.AfterDocumentChangeEventHandler(this.gdViewer_AfterZoomChange);
			this.gdViewer.DocumentClosed += new GdPicture12.GdViewer.DocumentClosedEventHandler(this.gdViewer_DocumentClosed);
			this.gdViewer.PageDisplayed += new GdPicture12.GdViewer.PageDisplayedEventHandler(this.gdViewer_PageDisplayed);
			this.gdViewer.ClickMenu += new GdPicture12.GdViewer.ClickMenuEventHandler(this.gdViewer_ClickMenu);
			this.gdViewer.AutoSizeChanged += new System.EventHandler(this.gdViewer_AutoSizeChanged);
			this.gdViewer.AutoValidateChanged += new System.EventHandler(this.gdViewer_AutoValidateChanged);
			this.gdViewer.Load += new System.EventHandler(this.gdViewer_Load);
			this.gdViewer.Scroll += new System.Windows.Forms.ScrollEventHandler(this.gdViewer_Scroll);
			this.gdViewer.BackColorChanged += new System.EventHandler(this.gdViewer_BackColorChanged);
			this.gdViewer.BackgroundImageChanged += new System.EventHandler(this.gdViewer_BackgroundImageChanged);
			this.gdViewer.BackgroundImageLayoutChanged += new System.EventHandler(this.gdViewer_BackgroundImageLayoutChanged);
			this.gdViewer.CausesValidationChanged += new System.EventHandler(this.gdViewer_CausesValidationChanged);
			this.gdViewer.ContextMenuStripChanged += new System.EventHandler(this.gdViewer_ContextMenuStripChanged);
			this.gdViewer.CursorChanged += new System.EventHandler(this.gdViewer_CursorChanged);
			this.gdViewer.DockChanged += new System.EventHandler(this.gdViewer_DockChanged);
			this.gdViewer.ForeColorChanged += new System.EventHandler(this.gdViewer_ForeColorChanged);
			this.gdViewer.LocationChanged += new System.EventHandler(this.gdViewer_LocationChanged);
			this.gdViewer.MarginChanged += new System.EventHandler(this.gdViewer_MarginChanged);
			this.gdViewer.RegionChanged += new System.EventHandler(this.gdViewer_RegionChanged);
			this.gdViewer.RightToLeftChanged += new System.EventHandler(this.gdViewer_RightToLeftChanged);
			this.gdViewer.TabIndexChanged += new System.EventHandler(this.gdViewer_TabIndexChanged);
			this.gdViewer.TabStopChanged += new System.EventHandler(this.gdViewer_TabStopChanged);
			this.gdViewer.Click += new System.EventHandler(this.gdViewer_Click);
			this.gdViewer.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.gdViewer_ControlAdded);
			this.gdViewer.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.gdViewer_ControlRemoved);
			this.gdViewer.DragDrop += new System.Windows.Forms.DragEventHandler(this.gdViewer_DragDrop);
			this.gdViewer.DragEnter += new System.Windows.Forms.DragEventHandler(this.gdViewer_DragEnter);
			this.gdViewer.DragOver += new System.Windows.Forms.DragEventHandler(this.gdViewer_DragOver);
			this.gdViewer.DragLeave += new System.EventHandler(this.gdViewer_DragLeave);
			this.gdViewer.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.gdViewer_GiveFeedback);
			this.gdViewer.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.gdViewer_HelpRequested);
			this.gdViewer.PaddingChanged += new System.EventHandler(this.gdViewer_PaddingChanged);
			this.gdViewer.Paint += new System.Windows.Forms.PaintEventHandler(this.gdViewer_Paint);
			this.gdViewer.QueryContinueDrag += new System.Windows.Forms.QueryContinueDragEventHandler(this.gdViewer_QueryContinueDrag);
			this.gdViewer.QueryAccessibilityHelp += new System.Windows.Forms.QueryAccessibilityHelpEventHandler(this.gdViewer_QueryAccessibilityHelp);
			this.gdViewer.DoubleClick += new System.EventHandler(this.gdViewer_DoubleClick);
			this.gdViewer.Enter += new System.EventHandler(this.gdViewer_Enter);
			this.gdViewer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gdViewer_KeyDown);
			this.gdViewer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.gdViewer_KeyPress);
			this.gdViewer.KeyUp += new System.Windows.Forms.KeyEventHandler(this.gdViewer_KeyUp);
			this.gdViewer.Leave += new System.EventHandler(this.gdViewer_Leave);
			this.gdViewer.MouseClick += new System.Windows.Forms.MouseEventHandler(this.gdViewer_MouseClick);
			this.gdViewer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.gdViewer_MouseDoubleClick);
			this.gdViewer.MouseCaptureChanged += new System.EventHandler(this.gdViewer_MouseCaptureChanged);
			this.gdViewer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gdViewer_MouseDown);
			this.gdViewer.MouseEnter += new System.EventHandler(this.gdViewer_MouseEnter);
			this.gdViewer.MouseLeave += new System.EventHandler(this.gdViewer_MouseLeave);
			this.gdViewer.MouseHover += new System.EventHandler(this.gdViewer_MouseHover);
			this.gdViewer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gdViewer_MouseMove);
			this.gdViewer.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gdViewer_MouseUp);
			this.gdViewer.Move += new System.EventHandler(this.gdViewer_Move);
			this.gdViewer.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.gdViewer_PreviewKeyDown);
			this.gdViewer.ChangeUICues += new System.Windows.Forms.UICuesEventHandler(this.gdViewer_ChangeUICues);
			this.gdViewer.StyleChanged += new System.EventHandler(this.gdViewer_StyleChanged);
			this.gdViewer.SystemColorsChanged += new System.EventHandler(this.gdViewer_SystemColorsChanged);
			this.gdViewer.Validating += new System.ComponentModel.CancelEventHandler(this.gdViewer_Validating);
			this.gdViewer.Validated += new System.EventHandler(this.gdViewer_Validated);
			this.gdViewer.ImeModeChanged += new System.EventHandler(this.gdViewer_ImeModeChanged);
			// 
			// TBPicViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.Controls.Add(this.gdViewer);
			this.Name = "TBPicViewer";
			this.Size = new System.Drawing.Size(156, 166);
			this.BackColorChanged += new System.EventHandler(this.TBPicViewer_BackColorChanged);
			this.ResumeLayout(false);

        }

        #endregion

		private GdPicture12.GdViewer gdViewer;
    }
}
