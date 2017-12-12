using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.UI.Controls;

namespace Microarea.EasyAttachment.UI.Forms
{
	public partial class AttachmentsListForm : Form
	{
		private List<AttachmentInfo> attachments = null;

		public List<AttachmentInfo>  SelectedDocuments { get { return docSlideShow.SelectedDocuments;  } }


		//------------------------------------------------------------------------------------------------
		public AttachmentsListForm(List<AttachmentInfo> attachments)
		{
			InitializeComponent();
			
			docSlideShow.DocViewMode = DocSlideShow.ViewMode.Grid;
			docSlideShow.DeleteVisible = false;
			docSlideShow.RefreshVisible = false;
			docSlideShow.BtnPaperyDocsVisible = false;
			docSlideShow.BtnSwitchViewVisible = false;
			docSlideShow.BrowseBtnsVisible = false;
		
			docSlideShow.OpenDocument += new EventHandler(docSlideShow_OpenDocument);
		
			this.attachments = attachments;			
		}

		//------------------------------------------------------------------------------------------------
		private void AttachmentsListForm_Load(object sender, EventArgs e)
		{
			docSlideShow.AttachmentInfoList = attachments;
		}

		//------------------------------------------------------------------------------------------------
		private void docSlideShow_OpenDocument(object sender, EventArgs e)
		{
			try
			{
                if (docSlideShow.CurrentDoc != null)
                    docSlideShow.CurrentDoc.OpenDocument();
			}
			catch (Exception)
			{				
			}
		}
	}
}
