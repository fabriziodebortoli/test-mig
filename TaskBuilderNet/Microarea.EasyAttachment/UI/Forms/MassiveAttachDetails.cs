using System;
using System.Windows.Forms;
using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.UI.Controls;

namespace Microarea.EasyAttachment.UI.Forms
{
	public partial class MassiveAttachDetails : Form
	{
		private AttachmentInfoOtherData attachmentInfo;
       
		// evento da intercettare esternamente per il rendering del barcode nell'area del Gdviewer
		public delegate Barcode RenderingBarcodeDelegate(TypedBarcode barcode);
		public event RenderingBarcodeDelegate RenderingBarcode;

	
		//--------------------------------------------------------------------------------------------
		public MassiveAttachDetails(AttachmentInfoOtherData attachment)
		{
			InitializeComponent();

			attachmentInfo = attachment;
			barcodeDetails.RenderingBarcode += new BarcodeDetails.RenderingBarcodeDelegate(barcodeDetails_RenderingBarcode);
		}

		//--------------------------------------------------------------------------------------------
		private void MassiveAttachDetails_Load(object sender, EventArgs e)
        {
           	Text = string.Format(Strings.ItemDetails, attachmentInfo.Attachment.Name);
			if (attachmentInfo.Result == MassiveResult.Ignored)
            {
                erpDocumentListView.Visible = false;
                label1.Text = Strings.NoActionExecuted;
                barcodeDetails.Barcode = attachmentInfo.Attachment.TBarcode;
                imageBtn.Image = MassiveAttachImageList.GetStatusImage(attachmentInfo.BarCodeStatus);
                return;
            }


			erpDocumentListView.Visible = false;
            switch (attachmentInfo.BarCodeStatus)
            {
                case MassiveStatus.Papery:
					foreach (ERPDocumentBarcode doc in attachmentInfo.ERPDocumentsBarcode)
                        erpDocumentListView.AddItem(new DocumentListItem(doc.Namespace, doc.PK, string.Empty));
                    erpDocumentListView.Visible = true;
					break;
                case MassiveStatus.OnlyBC:
                    label1.Text = Strings.NoPapery;
                    break;
                case MassiveStatus.NoBC:
                    label1.Text = Strings.NoBarcode;
                    break;
                case MassiveStatus.ItemDuplicated:
                    label1.Text = Strings.ItemDuplicatedVerbose;
                    break;
				case MassiveStatus.BCDuplicated:
					label1.Text = Strings.BarcodeAlreadyInUse;
                    break;
				default:
                    label1.Text = Strings.BarcodeAlreadyInUse;
                    break;
            }
			
			label1.Text += Environment.NewLine;
            switch (attachmentInfo.ActionToDo)
            {
                case MassiveAction.None:
                    label1.Text += Strings.NoActionToExecute;
                    break;
                case MassiveAction.Archive:
                    label1.Text += Strings.ItemToArchive;
                    break;
				case MassiveAction.Substitute:
					label1.Text += Strings.SustituteDocDescription;
					break;

            }

            barcodeDetails.Barcode = attachmentInfo.Attachment.TBarcode;				
			imageBtn.Image = MassiveAttachImageList.GetStatusImage(attachmentInfo.BarCodeStatus);
		}
		
		//--------------------------------------------------------------------------------------------
		BusinessLogic.Barcode barcodeDetails_RenderingBarcode(TypedBarcode barcode)
		{
			return (RenderingBarcode != null) ? RenderingBarcode(barcode) : null;
		}

		//--------------------------------------------------------------------------------------------
		private void BtnOK_Click(object sender, EventArgs e)
		{
			this.Close();
		}				
	}
}
