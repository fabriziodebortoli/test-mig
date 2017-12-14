using System;
using System.Windows.Forms;

using Microarea.EasyAttachment.Components;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyAttachment.UI.Forms
{
	//================================================================================
	public partial class DuplicateForm : Form
	{
		DuplicateDocumentAction action = DuplicateDocumentAction.Cancel;

		//---------------------------------------------------------------------
		public DuplicateForm()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		public DuplicateDocumentAction AskAction(AttachmentInfo oldDoc, AttachmentInfo newDoc, CheckDuplicateResult cdr, Control parent)
		{
			Populate(oldDoc, newDoc, cdr);

			//se o è null la form verrà posizionata in location 0,0 senza owner
			this.Owner = (parent != null) ? parent.FindForm() : null;
			
			if (this.Owner == null) this.TopMost = true;//se no rischia di rimanere sotto le altre finestre e  sembra che il programma sia bloccato.
			
			using (SafeThreadCallContext context = new SafeThreadCallContext())
				ShowDialog(this.Owner);
			
			return action;
		}

		//---------------------------------------------------------------------
		private void Populate(AttachmentInfo oldDoc, AttachmentInfo newDoc, CheckDuplicateResult cdr)
		{
			documentSpecificationNEW.Populate(newDoc);
			documentSpecificationOLD.Populate(oldDoc);

			string text = "({0})";
			
			switch (cdr)
			{
				case CheckDuplicateResult.MoreRecent:
					text = String.Format(text, Strings.MoreRecent);
					break;
				case CheckDuplicateResult.LessRecent:
					text = String.Format(text, Strings.LessRecent);
					break;
				case CheckDuplicateResult.DifferentPath:
					text = String.Format(text, Strings.DifferentPath);
					break;
				case CheckDuplicateResult.SameBarcode:
					text = string.Empty;
                    this.LblTitle.Text = string.Format(Strings.DuplicateBarcodeTitle, oldDoc.TBarcode.Value);
					this.LlblArchive.Text = Strings.ArchiveButIgnoreBarcode;
					this.LlblArrowSkip.Visible = false;
					this.LlblSkip.Visible = false;
					this.label1.Visible = false;
					break;
				default:
					break;
			}

			LblCheckDuplicateResult.Text = text;
		}

		//---------------------------------------------------------------------
		private void LlblArrowArchive_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			action = DuplicateDocumentAction.ArchiveAndKeepBothDocs;
			Close();
		}

		//---------------------------------------------------------------------
		private void LlblArchive_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			action = DuplicateDocumentAction.ArchiveAndKeepBothDocs;
			Close();
		}

		//---------------------------------------------------------------------
		private void LlblArrowSobstitute_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			action = DuplicateDocumentAction.ReplaceExistingDoc;
			Close();
		}

		//---------------------------------------------------------------------
		private void LlblSobstitute_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			action = DuplicateDocumentAction.ReplaceExistingDoc;
			Close();
		}

		//---------------------------------------------------------------------
		private void LlblSkip_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			action = DuplicateDocumentAction.UseExistingDoc;
			Close();
		}

		//---------------------------------------------------------------------
		private void LlblArrowSkip_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			action = DuplicateDocumentAction.UseExistingDoc;
			Close();
		}

		//---------------------------------------------------------------------
		private void btnCancel_Click(object sender, EventArgs e)
		{
			action = DuplicateDocumentAction.Cancel;
			Close();
		}
	}
}
