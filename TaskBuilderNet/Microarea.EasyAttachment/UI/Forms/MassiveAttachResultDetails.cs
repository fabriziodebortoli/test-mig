using System;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.UI.Controls;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyAttachment.UI.Forms
{
    //--------------------------------------------------------------------------------
    public partial class MassiveAttachResultDetails : Form
	{
		private AttachmentInfoOtherData attachmentInfo;

        //--------------------------------------------------------------------------------
        public MassiveAttachResultDetails(AttachmentInfoOtherData attachment)
		{
			InitializeComponent();

			attachmentInfo = attachment;
		}

        //--------------------------------------------------------------------------------
        private void MassiveAttachResultDetails_Load(object sender, EventArgs e)
		{

            Text = string.Format(Strings.ItemDetails, attachmentInfo.Attachment.Name); 
            if (attachmentInfo.ERPDocumentsBarcode == null || attachmentInfo.ERPDocumentsBarcode.Count == 0)
			{
				resultListView.Visible = false;
                detailLabel.Text = (attachmentInfo.Diagnostic.Error) ? Strings.ErrorArchivingItem 
					:  ((attachmentInfo.ActionToDo == MassiveAction.Substitute) ? Strings.SuccessfullySubstitutedDocument : Strings.SuccessfullyArchivedItem);
				showErrorBtn.Visible = attachmentInfo.Diagnostic.Error;
				errorLabel.Visible = attachmentInfo.Diagnostic.Error;
			}

            if (attachmentInfo.Result == MassiveResult.Ignored)
            {
                detailLabel.Text = Strings.NoActionExecuted;
                resultListView.Visible = false;
            }

            else//se l'item aveva come azione none non mostro la lista di docuemtni
                foreach (ERPDocumentBarcode doc in attachmentInfo.ERPDocumentsBarcode)
                    resultListView.AddItem(new MassiveAttachDetailsListItem(doc));

			resultPictureBox.Image = MassiveAttachImageList.GetResultImage(attachmentInfo.Result);
		}

		//-----------------------------------------------------------------------
		private void showErrorBtn_Click(object sender, EventArgs e)
		{
		 using (SafeThreadCallContext context = new SafeThreadCallContext())
			 DiagnosticViewer.ShowDiagnostic(attachmentInfo.Diagnostic);	
		}

		//-----------------------------------------------------------------------
		private void BtnOK_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
