using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.Console.Core.SecurityLibrary
{
	//=========================================================================
	public class DataGridImageColumn:DataGridColumnStyle 
	{
		private BitmapLoader bitmapLoader = null; 

		//---------------------------------------------------------------------
		public DataGridImageColumn(BitmapLoader bmpLoader)  
		{
			this.bitmapLoader = bmpLoader;
		}

		//---------------------------------------------------------------------
		protected override void Abort(int rowNumber) 
		{
		}

		//---------------------------------------------------------------------
		protected override bool Commit(CurrencyManager dataSource,int rowNumber) 
		{
			return true;
		}

		//---------------------------------------------------------------------
		protected override void Edit
									(
										CurrencyManager source,
										int				rowNumber,
										Rectangle		bounds, 
										bool			readOnly,
										string			instantText, 
										bool			cellIsVisible
									) 
		{
		}

		//---------------------------------------------------------------------
		protected override int GetMinimumHeight() 
		{
			return 16;
		}

		//---------------------------------------------------------------------
		protected override int GetPreferredHeight(Graphics g ,object objectValue) 
		{
			return 16;
		}

		//---------------------------------------------------------------------
		protected override Size GetPreferredSize(Graphics g, object objectValue) 
		{
			Size picSize = new Size(16,16);
			return picSize;
		}
		
		//---------------------------------------------------------------------
		protected Bitmap GetColumnImgAtRow(CurrencyManager source, int rowNumber)
		{
			try
			{
				int nImg = (int) GetColumnValueAtRow(source, rowNumber);
				return bitmapLoader.GetBitmap(nImg);
			}
			catch (Exception)
			{
				return bitmapLoader.GetBitmap(0);
			}
		}

		//---------------------------------------------------------------------
		protected override void Paint
			                        (
										Graphics		g,
										Rectangle		bounds,
										CurrencyManager	source,
										int				rowNumber
									) 
		{
			Paint(g, bounds, source, rowNumber, new SolidBrush(SystemColors.Window), null, false);
		}

		//---------------------------------------------------------------------
		protected override void Paint
									(
										Graphics		g,
										Rectangle		bounds,
										CurrencyManager	source,
										int				rowNumber,
										bool			alignToRight
									) 
		{
			Paint(g, bounds, source, rowNumber, new SolidBrush(SystemColors.Window), null, alignToRight);
		}

		//---------------------------------------------------------------------
		protected override void Paint
									(
										Graphics		g,
										Rectangle		bounds,
										CurrencyManager source,
										int				rowNumber, 
										Brush			backBrush,
										Brush			foreBrush,
										bool			alignToRight
									) 
		{
			g.FillRectangle(backBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);

			Bitmap imagePic = GetColumnImgAtRow(source, rowNumber);
			if (imagePic == null)
				return;

			g.DrawImage((Image)imagePic, 
				bounds.X + (bounds.Width - imagePic.Width)/2,
				bounds.Y + (bounds.Height - imagePic.Height)/2,
				imagePic.Width, 
				imagePic.Height);
		}
	}
}
