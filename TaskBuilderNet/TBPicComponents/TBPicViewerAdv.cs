using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Microarea.TBPicComponents
{
	//================================================================================
	public partial class TBPicViewerAdv : TBPicViewer
	{
		private bool enablePreviewNotAvailable = true;

		//-------------------------------------------------------------------------------
		public TBPicViewerAdv()
		{
			InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		public delegate void GetOCRTextDelegate(string ocrText);
		public event GetOCRTextDelegate GetOCRText;

		// per impostare la visibilita' o meno dell'immagine di preview non disponibile (ad esempio per il barcode)
		//--------------------------------------------------------------------------------
		public bool EnablePreviewNotAvailable { get { return enablePreviewNotAvailable; } set { enablePreviewNotAvailable = value; } }

		///<summary>
		/// Per nascondere il ToolStrip dall'esterno
		///</summary>
		//-------------------------------------------------------------------------------
		public void SetToolStripVisibility(bool visible) 
		{ 
			GdViewerToolStrip.Visible = visible;
		}

		//-------------------------------------------------------------------------------
		protected override void ClearDisplayProperties()
		{
			if (EnablePreviewNotAvailable)
			{
				PreviewPictureBox.Visible = false;
				LblNAPreview.Visible = false;
				ZoomInToolStripButton.Visible = false;
				ZoomOutToolStripButton.Visible = false;
				ToolStripSeparator1.Visible = false;
				ToolStripSeparator2.Visible = false;
				PreviousToolStripButton.Visible = false;
				NextToolStripButton.Visible = false;
			}
		}

		///<summary>
		/// metodo virtuale esposto dal padre per settare nei figli alcune proprieta' del controllo
		/// (nel nostro caso agiamo sui pulsanti della toolbar)
		///</summary>
			//-------------------------------------------------------------------------------
		protected override void SetDisplayProperties(TBPictureStatus status, string message = "")
		{
			// se lo stato non e' ok visualizzo l'immagine di preview non disponibile
			if (EnablePreviewNotAvailable)
			{
				PreviewPictureBox.Visible = (status != TBPictureStatus.OK);
				if (!string.IsNullOrWhiteSpace(message))
				{
					LblNAPreview.Visible = (status != TBPictureStatus.OK);
					LblNAPreview.Text = message;
				}

				ZoomInToolStripButton.Visible = (status == TBPictureStatus.OK);
				ZoomOutToolStripButton.Visible = (status == TBPictureStatus.OK);
			}

			// se il documento caricato contiene piu' di una pagina rendo visibili i pulsantini
			ToolStripSeparator1.Visible = (PageCount > 1);
			ToolStripSeparator2.Visible = (PageCount > 1);
			PreviousToolStripButton.Visible = (PageCount > 1);
			NextToolStripButton.Visible = (PageCount > 1);
			PreviousToolStripButton.Enabled = (CurrentPage > 1);
			NextToolStripButton.Enabled = (CurrentPage < PageCount);
		}

		//-------------------------------------------------------------------------------
		private void ZoomInToolStripButton_Click(object sender, EventArgs e)
		{
			this.ZoomIN();
		}

		//-------------------------------------------------------------------------------
		private void ZoomOutToolStripButton_Click(object sender, EventArgs e)
		{
			this.ZoomOUT();
		}

		//-------------------------------------------------------------------------------
		private void PreviousToolStripButton_Click(object sender, EventArgs e)
		{
			this.DisplayPreviousPage();
			PreviousToolStripButton.Enabled = (this.CurrentPage > 1);
			NextToolStripButton.Enabled = (this.CurrentPage < this.PageCount);
		}

		//-------------------------------------------------------------------------------
		private void NextToolStripButton_Click(object sender, EventArgs e)
		{
			this.DisplayNextPage();
			PreviousToolStripButton.Enabled = (this.CurrentPage > 1);
			NextToolStripButton.Enabled = (this.CurrentPage < this.PageCount);
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

			//if (this.IsRect())
			//{
			//	if (IsPdfFile)
			//	{
			//		float fLeftArea = 0; float fTopArea = 0; float fWidthArea = 0; float fHeightArea = 0;
			//		// mi faccio tornare le coordinate del rect dal GdViewer in inch
			//		this.GetRectCoordinatesOnDocumentInches(ref fLeftArea, ref fTopArea, ref fWidthArea, ref fHeightArea);
			//		// chiedo al manager la stringa contenuta  nel rect
			//		orcText = OCRManager.GetPdfOCRTextArea(CurrentPage, fLeftArea, fTopArea, fWidthArea, fHeightArea);

			//	}
			//	// orcText== empty per i file con estensione != da pdf 
			//	//oppure con estensione == da pdf ma non PDFA bensì immagine per cui la prima passata di OCR non ha dato esito.
			//	if (string.IsNullOrEmpty(orcText))
			//	{
			//		int leftArea = 0; int topArea = 0; int widthArea = 0; int heightArea = 0;
			//		// mi faccio tornare le coordinate del rect dal GdViewer in pixel
			//		this.GetRectCoordinatesOnDocument(ref leftArea, ref topArea, ref widthArea, ref heightArea);
			//		// chiedo al manager la stringa contenuta  nel rect
			//		orcText = OCRManager.GetOCRTextArea(CurrentPage, leftArea, topArea, widthArea, heightArea);
			//	}
			//}

			return orcText;
		}

		//-------------------------------------------------------------------------------
		private void AddTagViaOCRToolStripButton_Click(object sender, EventArgs e)
		{
			AddTag();
		}
	}
}
