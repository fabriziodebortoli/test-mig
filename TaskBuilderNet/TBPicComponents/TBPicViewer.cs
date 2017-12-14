using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GdPicture12;
using GdPicture12.Annotations;

namespace Microarea.TBPicComponents
{
	//================================================================================
	public partial class TBPicViewer : UserControl
	{
		internal GdViewer InternalGdViewer { get { return gdViewer; } }

		//--------------------------------------------------------------------------------
		public event EventHandler PageChanged;
		protected void OnPageChanged()
		{
			EventHandler handler = this.PageChanged;
			if (handler != null)
				handler(this, EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		public TBPicViewer()
		{
			TBPicBaseComponents.Register();
			InitializeComponent();
			this.gdViewer.PageChanged += new GdViewer.PageChangedEventHandler(gdViewer_PageChanged);
		}

		//--------------------------------------------------------------------------------
		void gdViewer_PageChanged()
		{
			OnPageChanged();
		}

		//--------------------------------------------------------------------------------
		public bool ScrollBars { get { return gdViewer.ScrollBars; } set { gdViewer.ScrollBars = value; } }

		//--------------------------------------------------------------------------------
		public bool EnableMenu { get { return gdViewer.EnableMenu; } set { gdViewer.EnableMenu = value; } }

		//--------------------------------------------------------------------------------
		public TBPicViewerMouseWheelMode MouseWheelMode { get { return (TBPicViewerMouseWheelMode)gdViewer.MouseWheelMode; } set { gdViewer.MouseWheelMode = (ViewerMouseWheelMode)value; } }

		//--------------------------------------------------------------------------------
		public TBPicViewerMouseMode MouseMode { get { return (TBPicViewerMouseMode)gdViewer.MouseMode; } set { gdViewer.MouseMode = (ViewerMouseMode)value; } }

		//--------------------------------------------------------------------------------
		public bool AnimateGIF { get { return gdViewer.AnimateGIF; } set { gdViewer.AnimateGIF = value; } }

		//--------------------------------------------------------------------------------
		public bool ContinuousViewMode { get { return gdViewer.ContinuousViewMode; } set { gdViewer.ContinuousViewMode = value; } }

		//--------------------------------------------------------------------------------
		public TBPicDisplayQuality DisplayQuality { get { return (TBPicDisplayQuality)gdViewer.DisplayQuality; } set { gdViewer.DisplayQuality = (DisplayQuality)value; } }

		//--------------------------------------------------------------------------------
		public bool DisplayQualityAuto { get { return gdViewer.DisplayQualityAuto; } set { gdViewer.DisplayQualityAuto = value; } }

		//--------------------------------------------------------------------------------
		public TBPicViewerDocumentAlignment DocumentAlignment { get { return (TBPicViewerDocumentAlignment)gdViewer.DocumentAlignment; } set { gdViewer.DocumentAlignment = (ViewerDocumentAlignment)value; } }

		//--------------------------------------------------------------------------------
		public TBPicViewerDocumentPosition DocumentPosition { get { return (TBPicViewerDocumentPosition)gdViewer.DocumentPosition; } set { gdViewer.DocumentPosition = (ViewerDocumentPosition)value; } }

		//--------------------------------------------------------------------------------
		public bool EnableMouseWheel { get { return gdViewer.EnableMouseWheel; } set { gdViewer.EnableMouseWheel = value; } }

		//--------------------------------------------------------------------------------
		public bool ForceScrollBars { get { return gdViewer.ForceScrollBars; } set { gdViewer.ForceScrollBars = value; } }

		//--------------------------------------------------------------------------------
		public bool EnabledProgressBar { get { return gdViewer.EnabledProgressBar; } set { gdViewer.EnabledProgressBar = value; } }

		//--------------------------------------------------------------------------------
		public bool ForceTemporaryModeForImage { get { return gdViewer.ForceTemporaryModeForImage; } set { gdViewer.ForceTemporaryModeForImage = value; } }

		//--------------------------------------------------------------------------------
		public float Gamma { get { return gdViewer.Gamma; } set { gdViewer.Gamma = value; } }

		//--------------------------------------------------------------------------------
		public bool HQAnnotationRendering { get { return gdViewer.HQAnnotationRendering; } set { gdViewer.HQAnnotationRendering = value; } }

		//--------------------------------------------------------------------------------
		public bool IgnoreDocumentResolution { get { return gdViewer.IgnoreDocumentResolution; } set { gdViewer.IgnoreDocumentResolution = value; } }

		//--------------------------------------------------------------------------------
		public bool KeepDocumentPosition { get { return gdViewer.KeepDocumentPosition; } set { gdViewer.KeepDocumentPosition = value; } }

		//--------------------------------------------------------------------------------
		public bool LockViewer { get { return gdViewer.LockViewer; } set { gdViewer.LockViewer = value; } }

		//--------------------------------------------------------------------------------
		public TBPicMouseButton MouseButtonForMouseMode { get { return (TBPicMouseButton)gdViewer.MouseButtonForMouseMode; } set { gdViewer.MouseButtonForMouseMode = (MouseButton)value; } }

		//--------------------------------------------------------------------------------
		public bool OptimizeDrawingSpeed { get { return gdViewer.DisplayQualityAuto; } set { gdViewer.DisplayQualityAuto = value; } }

		//--------------------------------------------------------------------------------
		public bool PdfDisplayFormField { get { return gdViewer.PdfDisplayFormField; } set { gdViewer.PdfDisplayFormField = value; } }

		//--------------------------------------------------------------------------------
		public bool PdfEnableLinks { get { return gdViewer.PdfEnableLinks; } set { gdViewer.PdfEnableLinks = value; } }

		//--------------------------------------------------------------------------------
		public int ZoomStep { get { return gdViewer.ZoomStep; } set { gdViewer.ZoomStep = value; } }

		//--------------------------------------------------------------------------------
		public TBPicViewerZoomMode ZoomMode { get { return (TBPicViewerZoomMode)gdViewer.ZoomMode; } set { gdViewer.ZoomMode = (ViewerZoomMode)value; } }

		//--------------------------------------------------------------------------------
		public bool PdfShowDialogForPassword { get { return gdViewer.PdfShowDialogForPassword; } set { gdViewer.PdfShowDialogForPassword = value; } }

		//--------------------------------------------------------------------------------
		public Color RectBorderColor { get { return gdViewer.RectBorderColor; } set { gdViewer.RectBorderColor = value; } }

		//--------------------------------------------------------------------------------
		public int RectBorderSize { get { return gdViewer.RectBorderSize; } set { gdViewer.RectBorderSize = value; } }

		//--------------------------------------------------------------------------------
		public bool RectIsEditable { get { return gdViewer.RectIsEditable; } set { gdViewer.RectIsEditable = value; } }

		//--------------------------------------------------------------------------------
		public bool RegionsAreEditable { get { return gdViewer.RegionsAreEditable; } set { gdViewer.RegionsAreEditable = value; } }

		//--------------------------------------------------------------------------------
		public short ScrollLargeChange { get { return gdViewer.ScrollLargeChange; } set { gdViewer.ScrollLargeChange = value; } }

		//--------------------------------------------------------------------------------
		public short ScrollSmallChange { get { return gdViewer.ScrollSmallChange; } set { gdViewer.ScrollSmallChange = value; } }

		//--------------------------------------------------------------------------------
		public bool SilentMode { get { return gdViewer.SilentMode; } set { gdViewer.SilentMode = value; } }

		//--------------------------------------------------------------------------------
		public double Zoom { get { return gdViewer.Zoom; } set { gdViewer.Zoom = value; } }

		//--------------------------------------------------------------------------------
		public bool ZoomCenterAtMousePosition { get { return gdViewer.ZoomCenterAtMousePosition; } set { gdViewer.ZoomCenterAtMousePosition = value; } }

		//--------------------------------------------------------------------------------
		public bool ForceTemporaryModeForPDF { get { return gdViewer.ForceTemporaryModeForPDF; } set { gdViewer.ForceTemporaryModeForPDF = value; } }

		//--------------------------------------------------------------------------------
		public int PageCount { get { return gdViewer.PageCount; } }

		//--------------------------------------------------------------------------------
		public int CurrentPage { get { return gdViewer.CurrentPage; } }

		// metodo virtuale con la possibilita' di impostare un messaggio personalizzato per l'anteprima non disponibile
		//--------------------------------------------------------------------------------
		protected virtual void SetDisplayProperties(TBPictureStatus status, string message = "") { }

		protected virtual void ClearDisplayProperties() { }

		//--------------------------------------------------------------------------------
		public TBPictureStatus GetStat()
		{
			return (TBPictureStatus)gdViewer.GetStat();
		}
		
		//--------------------------------------------------------------------------------
		public TBPictureStatus DisplayFromByteArray(byte[] Data)
		{
			return (TBPictureStatus)gdViewer.DisplayFromByteArray(Data);
		}

		//--------------------------------------------------------------------------------
		public TBPictureStatus DisplayFromFile(string FilePath, string message = "")
		{
			TBPictureStatus status = (TBPictureStatus)gdViewer.DisplayFromFile(FilePath);
			SetDisplayProperties(status, message);
			return status;
		}

		//--------------------------------------------------------------------------------
		public TBPictureStatus DisplayFromGdPictureImage(int ImageID, string message = "")
		{
			TBPictureStatus status = (TBPictureStatus)gdViewer.DisplayFromGdPictureImage(ImageID);
			SetDisplayProperties(status, message);
			return status;
		}

		// Clears the viewer
		//--------------------------------------------------------------------------------
		public void Clear()
		{
			gdViewer.Clear();
			ClearDisplayProperties();
		}

		//--------------------------------------------------------------------------------
		public void CloseDocument()
		{
			gdViewer.CloseDocument();
			ClearDisplayProperties();
		}

		//--------------------------------------------------------------------------------
		public void SetRectCoordinatesOnDocument(int Left, int Top, int Width, int Height)
		{
			gdViewer.SetRectCoordinatesOnDocument(Left, Top, Width, Height); ;
		}

		//--------------------------------------------------------------------------------
		public void RemoveAllRegions()
		{
			gdViewer.RemoveAllRegions();
		}

		//--------------------------------------------------------------------------------
		public void PrintDialog(IWin32Window form)
		{
			gdViewer.PrintDialog(form);
		}

		//--------------------------------------------------------------------------------
		public void PrintSetDocumentName(string documentName)
		{
			gdViewer.PrintSetDocumentName(documentName);
		}

		//--------------------------------------------------------------------------------
		public TBPictureStatus DisplayPage(int page)
		{
			return (TBPictureStatus)gdViewer.DisplayPage(page);
		}

		//--------------------------------------------------------------------------------
		public TBPictureStatus DisplayFirstPage()
		{
			return (TBPictureStatus)gdViewer.DisplayFirstPage();
		}

		//--------------------------------------------------------------------------------
		public TBPictureStatus DisplayLastPage()
		{
			return (TBPictureStatus)gdViewer.DisplayLastPage();
		}

		//--------------------------------------------------------------------------------
		public TBPictureStatus DisplayPreviousPage()
		{
			return (TBPictureStatus)gdViewer.DisplayPreviousPage();
		}

		//--------------------------------------------------------------------------------
		public TBPictureStatus DisplayNextPage()
		{
			return (TBPictureStatus)gdViewer.DisplayNextPage();
		}

		//--------------------------------------------------------------------------------
		public bool IsRectDrawing()
		{
			return gdViewer.IsRectDrawing();
		}

		//--------------------------------------------------------------------------------
		public bool IsRect()
		{
			return gdViewer.IsRect();
		}

		//--------------------------------------------------------------------------------
		public void ClearRect()
		{
			gdViewer.ClearRect();
		}

		//--------------------------------------------------------------------------------
		public TBPictureStatus ZoomIN()
		{
			return (TBPictureStatus)gdViewer.ZoomIN();
		}

		//--------------------------------------------------------------------------------
		public TBPictureStatus ZoomOUT()
		{
			return (TBPictureStatus)gdViewer.ZoomOUT();
		}

		//--------------------------------------------------------------------------------
		public void GetRectCoordinatesOnDocumentInches(ref float Left, ref float Top, ref float Width, ref float Height)
		{
			gdViewer.GetRectCoordinatesOnDocumentInches(ref Left, ref Top, ref Width, ref Height);
		}

		//--------------------------------------------------------------------------------
		public void GetRectCoordinatesOnDocument(ref int Left, ref int Top, ref int Width, ref int Height)
		{
			gdViewer.GetRectCoordinatesOnDocument(ref Left, ref Top, ref Width, ref Height);
		}

		//--------------------------------------------------------------------------------
		public void GetRectCoordinatesOnViewer(ref int Left, ref int Top, ref int Width, ref int Height)
		{
			gdViewer.GetRectCoordinatesOnViewer(ref Left, ref Top, ref Width, ref Height);
		}

		//--------------------------------------------------------------------------------
		public bool PdfSearchText(string Text, int Occurrence, bool CaseSensitive)
		{
			return gdViewer.PdfSearchText(Text, Occurrence, CaseSensitive);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_Click(object sender, EventArgs e)
		{
			OnClick(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_DoubleClick(object sender, EventArgs e)
		{
			OnDoubleClick(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MouseHover(object sender, EventArgs e)
		{
			OnMouseHover(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AfterPrintPage(int Page, int PageLeft)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AfterZoomChange()
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AnnotationAddedByUser(int AnnotationIdx)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AnnotationEndEditingText(int AnnotationIdx)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AnnotationClicked(int AnnotationIdx)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AnnotationMoved(int AnnotationIdx)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AnnotationResized(int AnnotationIdx)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AnnotationRotated(int AnnotationIdx)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AnnotationSelected(int AnnotationIdx)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AnnotationStartEditingText(int AnnotationIdx)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AutoSizeChanged(object sender, EventArgs e)
		{
			OnAutoSizeChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_AutoValidateChanged(object sender, EventArgs e)
		{
			OnAutoValidateChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_BackColorChanged(object sender, EventArgs e)
		{
			OnBackColorChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_BackgroundImageChanged(object sender, EventArgs e)
		{
			OnBackgroundImageChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_BackgroundImageLayoutChanged(object sender, EventArgs e)
		{
			OnBackgroundImageLayoutChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_BeforeAnnotationAddedByUser(int AnnotationIdx)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_BeforeDocumentChange()
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_BeforePrintPage(int Page, int PageLeft)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_BeforeRotation(RotateFlipType Rotation)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_BeforeZoomChange()
		{
		}

		//STACKOVERFLOW
		//--------------------------------------------------------------------------------
		//private void gdViewer_BindingContextChanged(object sender, EventArgs e)
		//{
		//    OnBindingContextChanged(e);
		//}

		//--------------------------------------------------------------------------------
		private void gdViewer_CausesValidationChanged(object sender, EventArgs e)
		{
			OnCausesValidationChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_ChangeUICues(object sender, UICuesEventArgs e)
		{
			OnChangeUICues(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_ClickMenu(int MenuItem)
		{
		}

		//--------------------------------------------------------------------------------
		//private void gdViewer_ClientSizeChanged(object sender, EventArgs e)
		//{
		//    OnClientSizeChanged(e);
		//}

		//--------------------------------------------------------------------------------
		private void gdViewer_ContextMenuStripChanged(object sender, EventArgs e)
		{
			OnContextMenuStripChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_ControlRemoved(object sender, ControlEventArgs e)
		{
			OnControlRemoved(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_ControlAdded(object sender, ControlEventArgs e)
		{
			OnControlAdded(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_CursorChanged(object sender, EventArgs e)
		{
			OnCursorChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_DataReceived(float PercentProgress, int SizeLeft, int TotalLength)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_DocumentClosed()
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_DockChanged(object sender, EventArgs e)
		{
			OnDockChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_DragDrop(object sender, DragEventArgs e)
		{
			OnDragDrop(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_DragLeave(object sender, EventArgs e)
		{
			OnDragLeave(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_DragEnter(object sender, DragEventArgs e)
		{
			OnDragEnter(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_DragOver(object sender, DragEventArgs e)
		{
			OnDragOver(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_DropFile(string File)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_Enter(object sender, EventArgs e)
		{
			OnEnter(e);
		}

		//--------------------------------------------------------------------------------
		//private void gdViewer_EnabledChanged(object sender, EventArgs e)
		//{
		//    OnEnabledChanged(e);
		//}

		//--------------------------------------------------------------------------------
		//private void gdViewer_FontChanged(object sender, EventArgs e)
		//{
		//    OnFontChanged(e);
		//}

		//--------------------------------------------------------------------------------
		private void gdViewer_GiveFeedback(object sender, GiveFeedbackEventArgs e)
		{
			OnGiveFeedback(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_ForeColorChanged(object sender, EventArgs e)
		{
			OnForeColorChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			OnHelpRequested(hlpevent);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_ImeModeChanged(object sender, EventArgs e)
		{
			OnImeModeChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_KeyPress(object sender, KeyPressEventArgs e)
		{
			OnKeyPress(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_KeyDown(object sender, KeyEventArgs e)
		{
			OnKeyDown(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_KeyUp(object sender, KeyEventArgs e)
		{
			OnKeyUp(e);
		}

		//--------------------------------------------------------------------------------
		//private void gdViewer_Layout(object sender, LayoutEventArgs e)
		//{
		//    OnLayout(e);
		//}

		//--------------------------------------------------------------------------------
		private void gdViewer_Leave(object sender, EventArgs e)
		{
			OnLeave(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_Load(object sender, EventArgs e)
		{
			OnLoad(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_LocationChanged(object sender, EventArgs e)
		{
			OnLocationChanged(e);
		}

		//private void gdViewer_VisibleChanged(object sender, EventArgs e)
		//{
		//    OnVisibleChanged(e);
		//}

		//--------------------------------------------------------------------------------
		private void gdViewer_Validating(object sender, CancelEventArgs e)
		{
			OnValidating(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_Validated(object sender, EventArgs e)
		{
			OnValidated(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_TransferEnded(GdPictureStatus status, bool Download)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_TabStopChanged(object sender, EventArgs e)
		{
			OnTabStopChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MarginChanged(object sender, EventArgs e)
		{
			OnMarginChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MouseCaptureChanged(object sender, EventArgs e)
		{
			OnMouseCaptureChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MouseClick(object sender, MouseEventArgs e)
		{
			OnMouseClick(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			OnMouseDoubleClick(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MouseDown(object sender, MouseEventArgs e)
		{
			OnMouseDown(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MouseEnter(object sender, EventArgs e)
		{
			OnMouseEnter(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MouseLeave(object sender, EventArgs e)
		{
			OnMouseLeave(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MouseMove(object sender, MouseEventArgs e)
		{
			OnMouseMove(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_MouseUp(object sender, MouseEventArgs e)
		{
			OnMouseUp(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_Move(object sender, EventArgs e)
		{
			OnMove(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_OnCustomAnnotationPaint(AnnotationCustom Annot, Graphics g)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_PaddingChanged(object sender, EventArgs e)
		{
			OnPaddingChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_PageDisplayed()
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_Paint(object sender, PaintEventArgs e)
		{
			OnPaint(e);
		}

		//--------------------------------------------------------------------------------
		//private void gdViewer_ParentChanged(object sender, EventArgs e)
		//{
		//    OnParentChanged(e);
		//}

		//--------------------------------------------------------------------------------
		private void gdViewer_PdfFileNavigation(ref string FilePath, ref bool Cancel)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_PdfPasswordRequest(ref string PassWord)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_PdfUriNavigation(ref string URI, ref bool Cancel)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_QueryAccessibilityHelp(object sender, QueryAccessibilityHelpEventArgs e)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			OnPreviewKeyDown(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
		{
			OnQueryContinueDrag(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_TabIndexChanged(object sender, EventArgs e)
		{
			OnTabIndexChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_RectEditedByUser()
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_RegionChanged(object sender, EventArgs e)
		{
			OnRegionChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_RegionEditedByUser(int RegionID)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_RegionSelectedByUser(int RegionID)
		{
		}

		//--------------------------------------------------------------------------------
		//private void gdViewer_Resize(object sender, EventArgs e)
		//{
		//    OnResize(e);
		//}

		//--------------------------------------------------------------------------------
		private void gdViewer_RightToLeftChanged(object sender, EventArgs e)
		{
			OnRightToLeftChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_Rotation(RotateFlipType Rotation)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_SavingProgress(int PageNo, int PageCount)
		{
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_Scroll(object sender, ScrollEventArgs e)
		{
			OnScroll(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_ScrollViewer()
		{
		}

		//--------------------------------------------------------------------------------
		//private void gdViewer_SizeChanged(object sender, EventArgs e)
		//{
		//    OnSizeChanged(e);
		//}

		//--------------------------------------------------------------------------------
		private void gdViewer_StyleChanged(object sender, EventArgs e)
		{
			OnStyleChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void gdViewer_SystemColorsChanged(object sender, EventArgs e)
		{
			OnSystemColorsChanged(e);
		}

		//--------------------------------------------------------------------------------
		private void TBPicViewer_BackColorChanged(object sender, EventArgs e)
		{
			foreach (Control item in this.Controls)
				item.BackColor = BackColor;
		}
	}
}