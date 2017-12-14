using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.PDFViewer
{
	public partial class GoToPageForm : Form
	{
		private int maxPage = 0;
		public int MaxPage { get { return maxPage; } set { maxPage = value; }}

		public delegate void GoToPage(object sender, int pageNumber);
		public event GoToPage OnGoToPage;

		//---------------------------------------------------------------------------
		public GoToPageForm()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------------
		private void pageNumberTextBox_Validating(object sender, CancelEventArgs e)
		{
			if (Convert.ToInt32(pageNumberTextBox.Text)> maxPage)
				e.Cancel = true;
		}

		//---------------------------------------------------------------------------
		private void goButton_Click(object sender, EventArgs e)
		{
			GoToPageOfPDF();
		}

		//---------------------------------------------------------------------------
		private void GoToPageOfPDF()
		{
			if (OnGoToPage != null)
				OnGoToPage(this, Convert.ToInt32(pageNumberTextBox.Text));

			this.Close();
		}

		//---------------------------------------------------------------------------
		private void pageNumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			Match m = Regex.Match(e.KeyChar.ToString(), @"[z0-9]");

			if (!m.Success && e.KeyChar != (char)Keys.Return)
			{
				e.KeyChar = (char)Keys.Back;
				return;
			}

			if (e.KeyChar == (char)Keys.Return)
				GoToPageOfPDF();
		}

	}
}
