using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TBPicComponents;

namespace Microarea.EasyAttachment.UI.Controls
{
	///<summary>
	/// UserControl che contiene un GdViewer attrezzato per la lettura di campi OCR
	///</summary>
	//================================================================================
	public partial class OCRViewer : UserControl
	{
		private AttachmentInfo currentAttachment = null;
		private TBPictureStatus picStatus = TBPictureStatus.WrongState;
	
		// Properties
		//--------------------------------------------------------------------------------
		public List<string> TextExtensions { get; set; }
		public OCRManager OCRManager { get; set; }

		public AttachmentInfo CurrentAttachInfo { get { return currentAttachment; } }

		public int PageCount { get { return IsPdfFile ? OCRManager.PdfPageCount : GdViewerArea.PageCount; } }
		public int CurrentPage { get { return IsPdfFile ? OCRManager.PdfCurrentPage : GdViewerArea.CurrentPage; } }
		private bool IsPdfFile { get { return OCRManager.IsPdfFile; } }

		//--------------------------------------------------------------------------------
		public bool AllowDropInGdViewerArea { get { return GdViewerArea.AllowDrop; } set { GdViewerArea.AllowDrop = value; } }

		public bool ShowAddTagContextMenu
		{
			get { return GdViewerArea.ContextMenuStrip == OCRContextMenu; }
			set
			{
				GdViewerArea.ContextMenuStrip = (value) ? OCRContextMenu : null;
				GdViewerArea.EnableMenu = !value;
				ToolStripSeparator4.Visible = value;
				AddTagToolStripButton.Visible = value;
			} 
		}

		public bool ShowDeleteTemplateButton
		{
			get { return DeleteTemplateToolStripButton.Visible; }
			set
			{
				ToolStripSeparator4.Visible = value;
				DeleteTemplateToolStripButton.Visible = value; 
			} 
		}

		public bool EnableDeleteTemplateButton
		{
			get { return DeleteTemplateToolStripButton.Enabled; }
			set { DeleteTemplateToolStripButton.Enabled = value; }
		}

		//--------------------------------------------------------------------------------
		public bool ShowToolStrip { set { GdViewerToolStrip.Visible = value; } }

		// Events
		//--------------------------------------------------------------------------------
		public event EventHandler DeleteTemplateButtonClicked;

		public delegate void GetOCRTextDelegate(string ocrText);
		public event GetOCRTextDelegate GetOCRText;

		public delegate void ParsedControlDragDrop(MParsedControl parsedControl);
		public event ParsedControlDragDrop OnParsedControlDragDrop;

		///<summary> 
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public OCRViewer()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		public void CloseCurrentImage()
		{
			if (picStatus == TBPictureStatus.OK)
			{
				GdViewerArea.CloseDocument();
				OCRManager.PdfCloseDocument();
			}

			PbNoPreview.Visible = LblNoPreview.Visible = false;
		}

		///<summary>
		/// Carica il documento selezionato nell'area del GdViewer, con le opzioni 
		/// per consentire la lettura via OCR
		///</summary>	
		//---------------------------------------------------------------------
		public void LoadDocumentFromImage(AttachmentInfo currentAttach)
		{
			if (currentAttachment == currentAttach)
				return;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				CloseCurrentImage();

				// se currentAttach e' null non carico il documento e non procedo
				if (currentAttach == null)
					return;

				// inizializzo il picStatus
				picStatus = TBPictureStatus.WrongState;

				this.currentAttachment = currentAttach;

				GdViewerArea.SilentMode = true;
                GdViewerArea.ZoomMode = TBPicViewerZoomMode.ZoomModeFitToViewer;

				if (OCRManager.LoadFromAttachment(currentAttach))
					picStatus = GdViewerArea.DisplayFromGdPictureImage(OCRManager.ImageId);

				// se il documento caricato contiene piu' di una pagina rendo visibili i pulsantini
				ToolStripSeparator3.Visible = PageCount > 1;
				PreviousToolStripButton.Visible = PageCount > 1;
				NextToolStripButton.Visible = PageCount > 1;
                PreviousToolStripButton.Enabled = CurrentPage > 1;
				NextToolStripButton.Enabled = CurrentPage < PageCount;
			}
			catch (DllNotFoundException dllEx)
			{
				MessageBox.Show(String.Format(Strings.Error, dllEx.Message), Strings.Attachments, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (NullReferenceException nullEx)
			{
				// visulizziamo l'Assert solo in debug
				Debug.Fail("Error in OCRViewer:LoadDocumentFromImage method!", nullEx.ToString());
			}
			catch (ArgumentException argEx)
			{
				MessageBox.Show(String.Format(Strings.Error, argEx.Message), Strings.Attachments, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format(Strings.Error, ex.Message), Strings.Attachments, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				if (picStatus != TBPictureStatus.OK)
					PbNoPreview.Visible = LblNoPreview.Visible = true;

				Cursor.Current = Cursors.Default;
			}
		}

		//--------------------------------------------------------------------------------
		private void ZoomInToolStripButton_Click(object sender, EventArgs e)
		{
			GdViewerArea.ZoomIN();
		}

		//--------------------------------------------------------------------------------
		private void ZoomOutToolStripButton_Click(object sender, EventArgs e)
		{
			GdViewerArea.ZoomOUT();
		}

		//--------------------------------------------------------------------------------
		private void OpenDocToolStripButton_Click(object sender, EventArgs e)
		{
			OpenDocument();
		}

		///<summary>
		/// Apro il documento con il programma predefinito
		///</summary>
		//--------------------------------------------------------------------------------
		private void OpenDocument()
		{
			if (this.currentAttachment == null)
				return;

			try
			{
                currentAttachment.OpenDocument();
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.ToString());
			}
		}

		///<summary>
		/// Mi posiziono sulla pagina precedente
		///</summary>
		//--------------------------------------------------------------------------------
		private void PreviousToolStripButton_Click(object sender, EventArgs e)
		{
			if (IsPdfFile)
			{
				if (OCRManager.PdfSelectPreviousPage() && OCRManager.ImageId > 0)
					picStatus = GdViewerArea.DisplayFromGdPictureImage(OCRManager.ImageId);
			}
			else
				GdViewerArea.DisplayPreviousPage();

			PreviousToolStripButton.Enabled = CurrentPage > 1;
			NextToolStripButton.Enabled = CurrentPage < PageCount;
		}

		///<summary>
		/// Mi posiziono sulla pagina successiva
		///</summary>
		//--------------------------------------------------------------------------------
		private void NextToolStripButton_Click(object sender, EventArgs e)
		{
			if (IsPdfFile)
			{
				if (OCRManager.PdfSelectNextPage() && OCRManager.ImageId > 0)
					picStatus = GdViewerArea.DisplayFromGdPictureImage(OCRManager.ImageId);
			}
			else
				GdViewerArea.DisplayNextPage();

			PreviousToolStripButton.Enabled = CurrentPage > 1;
			NextToolStripButton.Enabled = CurrentPage < PageCount;
		}

		///<summary>
		/// Sull'apertura del contextmenu abilito/disabilito l'item
		///</summary>
		//--------------------------------------------------------------------------------
		private void OCRContextMenu_Opening(object sender, CancelEventArgs e)
		{
			// il menuitem AddTag e' abilitato solo se ho disegnato un rect
			OCRContextMenu.Items[0].Enabled = GdViewerArea.IsRect();
		}

		///<summary>
		/// Aggiungo nei tag liberi il testo letto via OCR nel rettangolo
		///</summary>
		//--------------------------------------------------------------------------------
		private void TSAddTag_Click(object sender, EventArgs e)
		{
			AddTag();
		}

		//--------------------------------------------------------------------------------
		private void AddTag()
		{
			string ocrText = GetOCRRectText();
			if (ocrText != string.Empty && GetOCRText != null)
				GetOCRText(ocrText);
		}

		//--------------------------------------------------------------------------------
		internal string GetOCRRectText()
		{
			string orcText = null;

			if (GdViewerArea.IsRect())
			{
				if (IsPdfFile)
				{
					float fLeftArea = 0; float fTopArea = 0; float fWidthArea = 0; float fHeightArea = 0;
					// mi faccio tornare le coordinate del rect dal GdViewer in inch
					GdViewerArea.GetRectCoordinatesOnDocumentInches(ref fLeftArea, ref fTopArea, ref fWidthArea, ref fHeightArea);
					// chiedo al manager la stringa contenuta  nel rect
					orcText = OCRManager.GetPdfOCRTextArea(CurrentPage, fLeftArea, fTopArea, fWidthArea, fHeightArea);
			
				}
				// orcText== empty per i file con estensione != da pdf 
				//oppure con estensione == da pdf ma non PDFA bensì immagine per cui la prima passata di OCR non ha dato esito.
				if (string.IsNullOrEmpty(orcText)) 
				{
					int leftArea = 0; int topArea = 0; int widthArea = 0; int heightArea = 0;
					// mi faccio tornare le coordinate del rect dal GdViewer in pixel
					GdViewerArea.GetRectCoordinatesOnDocument(ref leftArea, ref topArea, ref widthArea, ref heightArea);
					// chiedo al manager la stringa contenuta  nel rect
					orcText = OCRManager.GetOCRTextArea(CurrentPage, leftArea, topArea, widthArea, heightArea);
				}
			}

			return orcText;
		}

		//--------------------------------------------------------------------------------
		private void GdViewerArea_DragOver(object sender, DragEventArgs e)
		{
			// test per vedere se riusciamo ad abilitare il drag solo nell'area del rect
			// ma x ora con scarsi risultati - aspettiamo una risposta di Loic ;-)
			if (GdViewerArea.IsRect())
			{
				int leftArea = 0; int topArea = 0; int widthArea = 0; int heightArea = 0;

				// mi faccio tornare le coordinate del rect dal GdViewer
				GdViewerArea.GetRectCoordinatesOnDocument(ref leftArea, ref topArea, ref widthArea, ref heightArea);

				int leftAreaVw = 0; int topAreaVw = 0; int widthAreaVw = 0; int heightAreaVw = 0;

				// mi faccio tornare le coordinate del rect dal GdViewer
				GdViewerArea.GetRectCoordinatesOnViewer(ref leftAreaVw, ref topAreaVw, ref widthAreaVw, ref heightAreaVw);

				int x = e.X;
				int y = e.Y;

				System.Drawing.Point client = PointToClient(new System.Drawing.Point(e.X, e.Y));

				if (client == null)
					x += y;
			}
		}

		//--------------------------------------------------------------------------------
		private void GdViewerArea_DragDrop(object sender, DragEventArgs e)
		{
			//drag'ndrop di control da mago
			if (e.Data.GetDataPresent(typeof(DragDropData)))
			{
				DragDropData data = (DragDropData)e.Data.GetData(typeof(DragDropData));
				if (data == null)
					return;

				using (BaseWindowWrapper winWrapper = (BaseWindowWrapper)BaseWindowWrapper.Create(data.Data.Handle))
				{
					if (winWrapper != null)
					{
						MParsedControl parsedCtrl = (MParsedControl)winWrapper;
						if (parsedCtrl != null)
						{
							// se il parsed control associato e' buono allora invio sparo un evento
							// che poi verra' intercettato nella form per aggiungere il field corrispondente nel datagrid
							if (OnParsedControlDragDrop != null)
								OnParsedControlDragDrop(parsedCtrl);
							return;
						}
					}
				}
			}
			else
				e.Effect = DragDropEffects.None;
		}

		//--------------------------------------------------------------------------------
		private void GdViewerArea_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.None;

			if (e.Data.GetDataPresent(typeof(DragDropData)) && GdViewerArea.IsRect())
				e.Effect = DragDropEffects.Copy;
		}

		//--------------------------------------------------------------------------------
		private void GdViewerArea_MouseMove(object sender, MouseEventArgs e)
		{
			if (!GdViewerArea.IsRectDrawing())
				AddTagToolStripButton.Enabled = GdViewerArea.IsRect();
		}

		//--------------------------------------------------------------------------------
		private void DeleteTemplateToolStripButton_Click(object sender, EventArgs e)
		{
			if (DeleteTemplateButtonClicked != null)
				DeleteTemplateButtonClicked(sender, e);
		}

		//--------------------------------------------------------------------------------
		private void AddTagToolStripButton_Click(object sender, EventArgs e)
		{
			AddTag();
		}

		//--------------------------------------------------------------------------------
		internal void ClearRectOnGdViewerArea()
		{
			GdViewerArea.ClearRect();
		}

		////--------------------------------------------------------------------------------
		//internal void SetRectOnGdViewerArea(string pos, int page)
		//{
		//    if (!string.IsNullOrWhiteSpace(pos))
		//    {
		//        int leftArea, topArea, widthArea, heightArea;

		//        OCRManager.GetCoordinatesFromString(pos, out leftArea, out topArea, out widthArea, out heightArea);

		//        if (page != CurrentPage)
		//        {
		//            if (OCRManager.PdfGotoPage(page) && OCRManager.ImageId > 0)
		//                GdViewerArea.DisplayFromGdPictureImage(OCRManager.ImageId);
		//        }

		//        if (picStatus == TBPictureStatus.OK)
		//        {
		//            GdViewerArea.SetRectCoordinatesOnDocument(leftArea, topArea, widthArea, heightArea);
		//            GdViewerArea.CenterOnRect();
		//        }
		//    }
		//}

		////--------------------------------------------------------------------------------
		//internal void SetRectOnGdViewerArea(OCRPointEventArgs args)
		//{
		//    if (args.PageNumber != CurrentPage)
		//    {
		//        if (OCRManager.PdfGotoPage(args.PageNumber) && OCRManager.ImageId > 0)
		//            GdViewerArea.DisplayFromGdPictureImage(OCRManager.ImageId);
		//    }

		//    if (picStatus == TBPictureStatus.OK)
		//    {
		//        GdViewerArea.SetRectCoordinatesOnDocument(args.LeftArea, args.TopArea, args.WidthArea, args.HeightArea);
		//        GdViewerArea.CenterOnRect();
		//    }
		//}
	}
}
