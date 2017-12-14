using System;
using System.Windows.Forms;
using Microarea.TBPicComponents;

namespace TesterTBPic
{
	public partial class TesterForm : Form
	{
		//---------------------------------------------------------------------------------------
		public TesterForm()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------------------------
		private void BtnTestGetImageIdFromTypedBarcode_Click(object sender, EventArgs e)
		{
			TBPicImaging tbPicImaging = new TBPicImaging();
			
			TBPicBarcode1DWriterType bcType = TBPicBarcode1DWriterType.Barcode1DWriterCode39;
	
			if (string.IsNullOrWhiteSpace(TxtBarcode.Text))
			{
				tbPicViewerAdv1.CloseDocument();
				return;
			}

			int imageId = tbPicImaging.GetImageIdFromTypedBarcode(bcType, TxtBarcode.Text,0,0,60,330);

            TBPicBarcode2DWriterType bcType2d = TBPicBarcode2DWriterType.Barcode2DWriterPDF417;
            int imageId2D = tbPicImaging.GetImageIdFrom2DBarcode(bcType2d,TxtBarcode.Text,0,0,60,60);
            if (imageId == -1)
			{
				tbPicViewerAdv1.CloseDocument();
				return;
			}

			tbPicViewerAdv1.DisplayFromGdPictureImage(imageId);
            tbPicViewerAdv2.DisplayFromGdPictureImage(imageId2D);
        }
	}
}
