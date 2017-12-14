using System;
using System.Diagnostics;
using System.Windows.Forms;

using Microarea.EasyAttachment.Components;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyAttachment.UI.Forms
{
	///<summary>
	/// Form che visualizza le azioni possibili dopo aver scelto di inserire un papery
	///</summary>
	//================================================================================
	public partial class DuplicateBarcodeForm : Form
	{
		public enum DuplicateBarcodeType { BarcodeExistsInArchDoc, BarcodeExistsInPapery };

		//--------------------------------------------------------------------------------
		private DuplicateBarcodeType actionType = DuplicateBarcodeType.BarcodeExistsInArchDoc;

		private bool canAction = true;
		private AttachmentInfo attachmentInfo = null;

		///<summary>
		/// Constructor
		///</summary>		
		//--------------------------------------------------------------------------------
		public DuplicateBarcodeForm()
		{
			InitializeComponent();
			this.Text = CommonStrings.EasyAttachmentTitle;
		}

		///<summary>
		/// Metodo da richiamare per inizializzare il layout della form
		///</summary>
		//--------------------------------------------------------------------------------
		public void InitForm(DuplicateBarcodeType actionType)
		{
			this.actionType = actionType;

			// il pulsante di preview e' visibile solo se si tratta di un barcode agganciato ad un doc archiviato
			BtnPreview.Visible = (this.actionType == DuplicateBarcodeType.BarcodeExistsInArchDoc);
		}

		///<summary>
		/// Entry-point per richiamare la form e farsi ritornare il tipo di azione scelta dall'utente
		///</summary>
		//---------------------------------------------------------------------
		public bool CanCreateAttachment(AttachmentInfo ai, Control parent)
		{
			attachmentInfo = ai;

			LblDescriAttach.Text = (this.actionType == DuplicateBarcodeType.BarcodeExistsInArchDoc)
                ? (string.Format(Strings.CanCreateAttachFromArchDoc, (ai != null) ? ai.TBarcode.Value : ""))
				: (string.Format(Strings.CanCreateAttachFromPapery, (ai != null) ? ai.TBarcode.Value : ""));

			//se o è null la form verrà posizionata in location 0,0 senza owner
			this.Owner = (parent != null) ? parent.FindForm() : null;

			if (this.Owner == null)
				this.TopMost = true;//se no rischia di rimanere sotto le altre finestre e sembra che il programma sia bloccato.

			using (SafeThreadCallContext context = new SafeThreadCallContext())
				ShowDialog(this.Owner);

			return canAction;
		}

		///<summary>
		/// Entry-point per richiamare la form in caso di duplicazione barcode
		///</summary>
		//---------------------------------------------------------------------
		public bool CanSubstituteDocument(AttachmentInfo ai, Control parent)
		{
			attachmentInfo = ai;

			LblAttach.Text = Strings.SubstituteDocument;

            LblDescriAttach.Text = (string.Format(Strings.CanSubstituteDocumentBarcode, (ai != null) ? ai.TBarcode.Value : ""));
			
			//se o è null la form verrà posizionata in location 0,0 senza owner
			this.Owner = (parent != null) ? parent.FindForm() : null;

			if (this.Owner == null)
				this.TopMost = true;//se no rischia di rimanere sotto le altre finestre e sembra che il programma sia bloccato.

			using (SafeThreadCallContext context = new SafeThreadCallContext())
				ShowDialog(this.Owner);

			return canAction;
		}


		//--------------------------------------------------------------------------------
		private void BtnYes_Click(object sender, EventArgs e)
		{
			canAction = true;
			Close();
		}

		//--------------------------------------------------------------------------------
		private void BtnNo_Click(object sender, EventArgs e)
		{
			canAction = false;
			Close();
		}

		//--------------------------------------------------------------------------------
		private void BtnPreview_Click(object sender, EventArgs e)
		{
			// visualizzo il documento archiviato corrispondente al barcode appena inserito
			OpenDocument();
		}

		///<summary>
		/// Apro il documento con il programma predefinito
		///</summary>
		//--------------------------------------------------------------------------------
		private void OpenDocument()
		{
			if (this.attachmentInfo == null)
				return;

			try
			{
                attachmentInfo.OpenDocument();

            }
			catch (Exception exc)
			{
				Debug.WriteLine(exc.ToString());
			}
		}
	}
}
