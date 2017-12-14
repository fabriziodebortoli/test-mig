using System;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.TbSenderBL;
using Microarea.TaskBuilderNet.TbSenderBL.Pdf;

namespace TbSenderTestUI
{
	public partial class MessageEditor : UserControl
	{
		//-------------------------------------------------------------------------------
		public MessageEditor()
		{
			InitializeComponent();
		}

		//-------------------------------------------------------------------------------
		public TB_MsgQueue CreateEditedMessage()
		{
			TB_MsgQueue msg = TB_MsgQueue.CreateMessage();
			msg.Fax = txtFax.Text.Trim();
			msg.Addressee = txtL0Addressee.Text.Trim();
			msg.Address = txtL1Address.Text.Trim();
			msg.Zip = txtL3Zip.Text.Trim();
			msg.City = txtL3City.Text.Trim();
			msg.County = txtL3County.Text.Trim();
			msg.Country = txtL4Country.Text.Trim();
			msg.Subject = txtSubject.Text.Trim();
			msg.DocNamespace = txtDocNamespace.Text.Trim();
			string fileName = this.txtPdf.Text.Trim();
			if (string.IsNullOrEmpty(fileName) || false == File.Exists(fileName))
			{
				msg.DocImage = null;
				msg.DocSize = 0;
				msg.DocPages = 0;
				msg.DocFileName = null;
			}
			else
			{
				byte[] arr = File.ReadAllBytes(fileName);
				msg.DocImage = arr;
				msg.DocSize = arr.Length;
				msg.DocPages = PdfHelper.GetPdfPages(arr);
				msg.DocFileName = Path.GetFileName(fileName);
			}
			return msg;
		}

		//-------------------------------------------------------------------------------
		private void bntBrows4Pdf_Click(object sender, EventArgs e)
		{
			string fileName = null;
			using (OpenFileDialog form = new OpenFileDialog())
			{
				form.Filter = "PDF files (*.pdf)|*.pdf";
				if (form.ShowDialog() == DialogResult.OK)
					fileName = form.FileName;
			}
			this.txtPdf.Text = fileName;
		}
	}
}
