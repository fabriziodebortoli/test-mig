using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Microarea.TaskBuilderNet.Core.CoreTypes
{
	

	/// <summary>
	/// Summary description for GraphicUtility.
	/// </summary>
	public class HtmlUtility
	{
		//------------------------------------------------------------------------------
		public static string ToHtml(Color aColor)
		{
			if (aColor == Color.Empty || aColor == Color.Transparent)
				return "Transparent";

			return String.Format("#{0:X2}{1:X2}{2:X2}", aColor.R, aColor.G, aColor.B);
		}
	}

	/// <summary>
	/// Summary description for GraphicUtility.
	/// </summary>
	public class GraphicUtility
	{
		//---------------------------------------------------------------------
		public static bool ClipText 
			(
			ref string				text, 
			ref System.Drawing.Font textFont, 
			System.Drawing.Size		clippingSize,
			bool					allowOverflow
			)
		{
			bool inputChanged= false;

			if (text == null || text.Trim() == String.Empty)
				return inputChanged;

			text = text.Trim();

			Bitmap tmpBmp = new System.Drawing.Bitmap(clippingSize.Width, clippingSize.Height, PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(tmpBmp);

			System.Drawing.SizeF textSizeF = g.MeasureString(text, textFont);

			if (clippingSize.Height < (int)Math.Ceiling(textSizeF.Height))
			{
				textFont = new Font(textFont.FontFamily.Name, clippingSize.Height, textFont.Style, System.Drawing.GraphicsUnit.Pixel);
				textSizeF = g.MeasureString(text, textFont);

				inputChanged = true;
			}

			while (text.Length > 0 && clippingSize.Width < (int)Math.Ceiling(textSizeF.Width))
			{
				if (allowOverflow)
				{
					text = text.Substring(0, text.Length -1);
					text = text.Trim();
					textSizeF = g.MeasureString(text + "...", textFont);
				}
				else
				{
					text = new String('*', text.Length -1);
					textSizeF = g.MeasureString(text, textFont);
				}
				inputChanged = true;
			}

			g.Dispose();
			tmpBmp.Dispose();

			return inputChanged;
		}
	}
}
