using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microarea.TBPicComponents;

namespace Microarea.TaskBuilderNet.UI.PDFViewer
{
	//============================================================================
	public partial class PDFViewerContainerForm : Form
	{
		private TBPicImaging gdPictureImaging = new TBPicImaging();
		private TBPicPDF nativePDF = new TBPicPDF();

		private string filepath = string.Empty;

        private bool MaximizedMode 
        {
            get { return !SplitContainer2.Panel1Collapsed; }
            set { 
                    SplitContainer2.Panel1Collapsed = !value; 
                    ToolStrip.Visible = true;
                    ToolStrip.Enabled  = true;
                    MenuStrip.Visible = value;
                    this.WindowState = FormWindowState.Normal;
                    PDFGdViewer.ContinuousViewMode = true;
					PDFGdViewer.MouseWheelMode = TBPicViewerMouseWheelMode.MouseWheelModeVerticalScroll;
					PDFGdViewer.MouseMode = TBPicViewerMouseMode.MouseModePan;
                }
        }
		//------------------------------------------------------------------------
		public PDFViewerContainerForm()
		{
			InitializeComponent();
		}

		//------------------------------------------------------------------------
		public PDFViewerContainerForm(bool maximizedMode, string filePath)
		{
			InitializeComponent();
			
			this.filepath = filePath;
            MaximizedMode = maximizedMode;

			LoadFile();
		}
		
		
		//------------------------------------------------------------------------
		private void LoadFile()
		{
			bool openOK = false;

			TBPicDocumentFormat docFormat = gdPictureImaging.GetDocumentFormatFromFile(filepath);

			if (docFormat == TBPicDocumentFormat.DocumentFormatPDF)
			{
				if (nativePDF.LoadFromFile(filepath, false) == TBPictureStatus.OK)
				{
					if (PDFGdViewer.DisplayFromFile(filepath) != TBPictureStatus.OK)
						MessageBox.Show(Strings.ErrorOnLoadPDF);

					PDFGdViewer.ZoomMode = TBPicViewerZoomMode.ZoomModeFitToViewer;
					openOK = true;
				}
				else
					MessageBox.Show(Strings.ErrorOnLoadPDF + nativePDF.GetStat().ToString());
			}

			if (openOK)
				OpenedFileLayout();
		}
		
		//------------------------------------------------------------------------
		private void OpenedFileLayout()
		{
			this.Text = filepath.Substring(filepath.LastIndexOf('\\') + 1);
			ToolStrip.Enabled = true;
			SaveToolStripMenuItem.Enabled = true;
			CloseToolStripMenuItem.Enabled = true;
			PageToolStripMenuItem.Enabled = true;
			ZoomToolStripMenuItem.Enabled = true;
			PrintToolStripMenuItem.Enabled = true;
			string zoom = Convert.ToString(PDFGdViewer.Zoom * 100);
			zoom = string.Format("{0:0.##}", zoom);
			zoomToolStripTextBox.Text = zoom;
			PDFGdViewer.Focus();
			PDFThumbnail.LoadFromGdViewer(PDFGdViewer);
			PDFThumbnail.Focus();
		}

		//------------------------------------------------------------------------
		private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFD = new OpenFileDialog(); 
			openFD.Filter = "PDF|*.pdf";

			if (openFD.ShowDialog(this) == DialogResult.OK)
				filepath = openFD.FileName;
			
			LoadFile();
		}

		//------------------------------------------------------------------------
		private void GotoFirstPageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (PDFGdViewer.DisplayFirstPage() != TBPictureStatus.OK)
				MessageBox.Show(Strings.Error + PDFGdViewer.GetStat().ToString()); 
		}

		//------------------------------------------------------------------------
		private void GotoPreviousPageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (PDFGdViewer.DisplayPreviousPage() != TBPictureStatus.OK)
				MessageBox.Show(Strings.Error + PDFGdViewer.GetStat().ToString());  
		}

		//------------------------------------------------------------------------
		private void GotoNextPageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (PDFGdViewer.DisplayNextPage() != TBPictureStatus.OK)
				MessageBox.Show(Strings.Error + PDFGdViewer.GetStat().ToString()); 
		}

		//------------------------------------------------------------------------
		private void GotoLastPageToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (PDFGdViewer.DisplayLastPage() != TBPictureStatus.OK)
				MessageBox.Show(Strings.Error + PDFGdViewer.GetStat().ToString()); 
		}

		//------------------------------------------------------------------------
		private void FitToViewerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PDFGdViewer.ZoomMode = TBPicViewerZoomMode.ZoomModeFitToViewer; 
		}

		//------------------------------------------------------------------------
		private void ToolStripMenuItem10_Click(object sender, EventArgs e)
		{
			PDFGdViewer.PrintSetDocumentName(this.Text);
			PDFGdViewer.PrintDialog(this); 
		}

		
		//------------------------------------------------------------------------
		private void PDFGdViewer_PageChanged(object sender, EventArgs e)
		{
			SelectPage();
		}

		//------------------------------------------------------------------------
		private void SelectPage()
		{
			totToolStripLabel.Text = "/ " + PDFGdViewer.PageCount;
			pageNumberToolStripTextBox.Text = PDFGdViewer.CurrentPage.ToString();
			PDFThumbnail.SelectItem(PDFGdViewer.CurrentPage - 1);
		}

		//------------------------------------------------------------------------
		private void upToolStripButton_Click(object sender, EventArgs e)
		{
			PDFGdViewer.DisplayPreviousPage();
			SelectPage();
		}

		//------------------------------------------------------------------------
		private void downToolStripButton_Click(object sender, EventArgs e)
		{
			PDFGdViewer.DisplayNextPage();
			SelectPage();
		}

		//------------------------------------------------------------------------
		private void gotoPageNumberToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GoToPageForm goToPageForm = new GoToPageForm();
			goToPageForm.OnGoToPage += new GoToPageForm.GoToPage(goToPageForm_OnGoToPage);
			goToPageForm.MaxPage = PDFGdViewer.PageCount;
			goToPageForm.ShowDialog();
		}

		//------------------------------------------------------------------------
		void goToPageForm_OnGoToPage(object sender, int pageNumber)
		{
			GoToPageNumber(pageNumber);
		}

		//------------------------------------------------------------------------
		public void GoToPageNumber(int number)
		{
			PDFGdViewer.DisplayPage(number);
		}

        //------------------------------------------------------------------------
        private void pageNumberToolStripTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			Match m = Regex.Match(e.KeyChar.ToString(), @"[z0-9]");

			if (!m.Success && e.KeyChar != (char)Keys.Return)
			{
				e.KeyChar = (char)Keys.Back;
				return;
			}

			if (e.KeyChar == (char)Keys.Return)
			{
				PDFGdViewer.DisplayPage(Convert.ToInt32(pageNumberToolStripTextBox.Text));
				SelectPage();
			}
		}

		//------------------------------------------------------------------------
		private void printToolStripButton_Click(object sender, EventArgs e)
		{		
			PDFGdViewer.PrintSetDocumentName(this.Text);
			PDFGdViewer.PrintDialog(this);
		}


		#region Zoom
		//------------------------------------------------------------------------
		private void ZoomIn()
		{ 
			PDFGdViewer.ZoomIN();
			string zoom =  Convert.ToString(PDFGdViewer.Zoom * 100);
			zoomToolStripTextBox.Text = string.Format("{0:0.##}", zoom);

		}

        //------------------------------------------------------------------------
        private void ZoomINToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ZoomIn();
		}
			
		//------------------------------------------------------------------------
		private void ZoomOut()
		{
			PDFGdViewer.ZoomOUT();
			string zoom = Convert.ToString(PDFGdViewer.Zoom * 100);
			zoom = String.Format("{0:0.##}", zoom);
			zoomToolStripTextBox.Text = zoom;
		}
        //------------------------------------------------------------------------
        private void ZoomOUTToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ZoomOut();
		}

        //------------------------------------------------------------------------
        private void plusTreeToolStripButton_Click(object sender, EventArgs e)
		{
			ZoomIn();
		}

        //------------------------------------------------------------------------
        private void minusToolStripButton_Click(object sender, EventArgs e)
		{
			ZoomOut();
		}

		//------------------------------------------------------------------------
		private void FitToHeightToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PDFGdViewer.ZoomMode = TBPicViewerZoomMode.ZoomModeHeightViewer;
		}

		//------------------------------------------------------------------------
		private void FitToWidthToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PDFGdViewer.ZoomMode = TBPicViewerZoomMode.ZoomModeWidthViewer;
		}

		//------------------------------------------------------------------------
		private void ToolStripMenuItem6_Click(object sender, EventArgs e)
		{
			PDFGdViewer.Zoom = 0.5;
		}

		//------------------------------------------------------------------------
		private void ToolStripMenuItem2_Click(object sender, EventArgs e)
		{
			PDFGdViewer.Zoom = 1;
		}

		//------------------------------------------------------------------------
		private void ToolStripMenuItem3_Click(object sender, EventArgs e)
		{
			PDFGdViewer.Zoom = 1.5;
		}

		//------------------------------------------------------------------------
		private void ToolStripMenuItem4_Click(object sender, EventArgs e)
		{
			PDFGdViewer.Zoom = 2;
		}

		#endregion Zoom




		#region Search
		//------------------------------------------------------------------------
		private void FindToolStripTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Return)
				SearchInCurrentPage();
		}
		//------------------------------------------------------------------------
		private void SearchInCurrentPage()
		{
			PDFGdViewer.RemoveAllRegions();
			if (!(PDFGdViewer.PdfSearchText(FindToolStripTextBox.Text, 0, false)))
				MessageBox.Show("Not found !");
		}
		//------------------------------------------------------------------------
		private void searchInPrevPToolStripButton_Click(object sender, EventArgs e)
		{
			if (PDFGdViewer.DisplayPreviousPage() == TBPictureStatus.OK)
				SearchInCurrentPage();
		}
		//------------------------------------------------------------------------
		private void searchInNextPToolStripButton_Click(object sender, EventArgs e)
		{
			if (PDFGdViewer.DisplayNextPage() == TBPictureStatus.OK)
				SearchInCurrentPage();
		}
		#endregion Search

		//------------------------------------------------------------------------
		private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PDFGdViewer.CloseDocument();
			PDFThumbnail.ClearAllItems();
		}

		//------------------------------------------------------------------------
		private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void PDFGdViewer_PageChanged()
		{

		}

	}
}
