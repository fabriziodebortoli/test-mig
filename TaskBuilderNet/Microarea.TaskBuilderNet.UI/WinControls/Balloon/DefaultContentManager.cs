using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// DefaultContentManager.
	/// </summary>
	//=========================================================================
	public class DefaultContentManager : IContentManager
	{
		protected const int		DefaultWidth		= 320;
		protected const int		XOffset				= 36;
		protected const int		YOffset				= 36;
		protected const int		MinimumArrowHeight	= 52;

		private Color startColor = Color.White;
		private Color endColor = Color.Lavender;

		//---------------------------------------------------------------------
		public Color StartColor
		{
			get { return startColor; }
			set { startColor = value; }
		}

		//---------------------------------------------------------------------
		public Color EndColor
		{
			get { return endColor; }
			set { endColor = value; }
		}

		#region IContentManager Members

		//---------------------------------------------------------------------
		protected virtual Size GetContentArea(ref int titleYOffset, Balloon container)
		{
			Graphics g = container.CreateGraphics();

			Size contentArea = new Size(0,0);

			Image titleImage = container.TitleImage;
			string titleText = container.TitleText;
			string contentText = container.ContentText;

			if (titleImage != null)
			{
				contentArea.Width += titleImage.Size.Width;
				contentArea.Height += titleImage.Size.Height;
			}

			contentArea.Width += DefaultWidth;

			if (titleText != null && titleText.Length > 0)
			{
				Font titleFont = new Font(container.Font.FontFamily, container.Font.Size + 1, FontStyle.Bold);
				SizeF titleSize = g.MeasureString(titleText, titleFont, new SizeF(DefaultWidth, int.MaxValue));
				titleFont.Dispose();
				titleFont = null;

				if (titleSize.Height > contentArea.Height)
					contentArea.Height = (int)Math.Ceiling(titleSize.Height);
			}
			contentArea.Width += 2;
			contentArea.Height += 10;
			titleYOffset = contentArea.Height;

			if (contentText != null && contentText.Length > 0)
			{
				SizeF helpContentSize = g.MeasureString(contentText, container.Font, new SizeF(contentArea.Width, int.MaxValue));

				contentArea.Height += (int)Math.Ceiling(helpContentSize.Height);
			}

			g.Dispose();
			g = null;

			return contentArea;
		}

		//---------------------------------------------------------------------
		public virtual void SetBackgroundImage(Balloon container)
		{
			if (container == null)
				throw new ArgumentNullException("container");
			int titleYOffset = 0;
			
			Size contentArea = GetContentArea(ref titleYOffset, container);

			Color borderColor = container.BorderColor;
			string contentText = container.ContentText;
			string titleText = container.TitleText;
			Image titleImage = container.TitleImage;
			Point startPosition = container.BaloonStartPosition;

			Balloon.Position p = Balloon.Position.None;

			Rectangle screenRect = Screen.GetWorkingArea(container.AssociatedControl);

			p |= (startPosition.X < screenRect.Left + screenRect.Width / 2) ? Balloon.Position.Right : Balloon.Position.Left;
			p |= (startPosition.Y < screenRect.Top + screenRect.Height / 2) ? Balloon.Position.Down : Balloon.Position.Up;

			if ((p & (Balloon.Position.Right | Balloon.Position.Down)) == (Balloon.Position.Right | Balloon.Position.Down))
			{
				startPosition.X += container.AssociatedControl.Width / 2;
				container.Location = new Point(startPosition.X, startPosition.Y );
			}
			else if ((p & (Balloon.Position.Right | Balloon.Position.Up)) == (Balloon.Position.Right | Balloon.Position.Up))
			{
				startPosition.X += container.AssociatedControl.Width / 2;
				container.Location = new Point(startPosition.X, startPosition.Y - (contentArea.Height + 2 * YOffset + MinimumArrowHeight));
			}
			else if ((p & (Balloon.Position.Left | Balloon.Position.Up)) == (Balloon.Position.Left | Balloon.Position.Up))
			{
				startPosition.X -= container.AssociatedControl.Width / 2;
				container.Location = new Point(startPosition.X - (contentArea.Width + 2 * XOffset), startPosition.Y - (contentArea.Height + 2 * YOffset + MinimumArrowHeight));
			}
			else if ((p & (Balloon.Position.Left | Balloon.Position.Down)) == (Balloon.Position.Left | Balloon.Position.Down))
			{
				startPosition.X -= container.AssociatedControl.Width / 2;
				container.Location = new Point(startPosition.X - (contentArea.Width + 2 * XOffset), startPosition.Y);
			}

			Point arrowPoint = container.PointToClient(startPosition);
	
			container.Size = new Size(2 * XOffset + contentArea.Width, contentArea.Height + 2 * YOffset + MinimumArrowHeight);

			int controlTop = 0;
			
			GraphicsPath path = new GraphicsPath();

			if ((p & (Balloon.Position.Left | Balloon.Position.Down)) == (Balloon.Position.Left | Balloon.Position.Down))
			{
				controlTop = MinimumArrowHeight;
				
				path.AddLine(container.Width, arrowPoint.Y, contentArea.Width + XOffset - 2, controlTop);
				path.AddArc(contentArea.Width - 2, controlTop, 2 * XOffset, 2 * YOffset, 270, 90);
				path.AddLine(2 * XOffset + contentArea.Width - 2, controlTop + YOffset, 2 * XOffset + contentArea.Width - 2, contentArea.Height + controlTop+YOffset - 2);
				path.AddArc(contentArea.Width, controlTop + contentArea.Height - 2, 2 * XOffset - 2, 2 * YOffset, 0, 90);
				path.AddLine(contentArea.Width + XOffset, controlTop + contentArea.Height + 2 * YOffset - 2, XOffset, controlTop + contentArea.Height + 2 * YOffset - 2);
				path.AddArc(0, controlTop + contentArea.Height - 2, 2 * XOffset, 2 * YOffset, 90, 90);
				path.AddLine(0, controlTop + contentArea.Height + YOffset - 2, 0, controlTop + YOffset);
				path.AddArc(0, controlTop, 2 * XOffset, 2 * YOffset, 180, 90);
				path.AddLine(XOffset, controlTop, contentArea.Width, controlTop);
				path.AddLine(contentArea.Width, controlTop, container.Width, arrowPoint.Y);
			}
			else if ((p & (Balloon.Position.Right | Balloon.Position.Down)) == (Balloon.Position.Right | Balloon.Position.Down))
			{
				controlTop = MinimumArrowHeight;
				
				path.AddLine(0, 0, 2 * XOffset, controlTop);
				path.AddLine(2 * XOffset, controlTop, contentArea.Width - 2, controlTop);
				path.AddArc(contentArea.Width - 2, controlTop, 2 * XOffset, 2 * YOffset, 270, 90);
				path.AddLine(2 * XOffset + contentArea.Width - 2, controlTop + YOffset, 2 * XOffset + contentArea.Width - 2, controlTop + YOffset + contentArea.Height - 2);
				path.AddArc(contentArea.Width - 2, controlTop + contentArea.Height - 2, 2 * XOffset, 2 * YOffset, 0, 90);
				path.AddLine(contentArea.Width + XOffset - 2, controlTop + contentArea.Height + 2 * YOffset - 2, XOffset - 1, controlTop + contentArea.Height + 2 * YOffset - 2);
				path.AddArc(0, controlTop + contentArea.Height - 2, 2 * XOffset, 2 * YOffset, 90, 90);
				path.AddLine(0, controlTop + contentArea.Height - 2, 0, controlTop + YOffset);
				path.AddArc(0, controlTop, 2 * XOffset, 2 * YOffset, 180, 90);
				path.AddLine(XOffset, controlTop, 0, 0);
				
			}
			else if ((p & (Balloon.Position.Left | Balloon.Position.Up)) == (Balloon.Position.Left | Balloon.Position.Up))
			{
				controlTop = container.Height - MinimumArrowHeight;

				path.AddLine(container.Width - 2, arrowPoint.Y, contentArea.Width , controlTop);
				path.AddLine(contentArea.Width, controlTop, XOffset - 1, controlTop );
				path.AddArc(0, contentArea.Height,2 * XOffset, 2 * YOffset, 90, 90);
				path.AddLine(0, contentArea.Height + YOffset, 0, YOffset);
				path.AddArc(0, 0, 2 * XOffset, 2 * YOffset, 180, 90);
				path.AddLine(XOffset, 0, contentArea.Width - 2, 0);
				path.AddArc(contentArea.Width - 2, 0, 2 * XOffset, 2 * YOffset, 270, 90);
				path.AddLine(2 * XOffset + contentArea.Width - 2, YOffset, 2 * XOffset + contentArea.Width - 2, controlTop - YOffset);
				path.AddArc(contentArea.Width - 2, contentArea.Height, 2 * XOffset, 2 * YOffset, 0, 90);
				path.AddLine(contentArea.Width + XOffset, controlTop, container.Width - 2, arrowPoint.Y);
			}
			else if ((p & (Balloon.Position.Right | Balloon.Position.Up)) == (Balloon.Position.Right | Balloon.Position.Up))
			{
				controlTop = container.Height - MinimumArrowHeight;

				path.AddLine(0, arrowPoint.Y, XOffset , controlTop);
				path.AddArc(0, contentArea.Height, 2 * XOffset, 2 * YOffset, 90, 90);
				
				path.AddLine(0, contentArea.Height + YOffset, 0, YOffset);
				path.AddArc(0, 0, 2 * XOffset, 2 * YOffset, 180, 90);
				
				path.AddLine(XOffset, 0, contentArea.Width - 2, 0);
				path.AddArc(contentArea.Width - 2, 0, 2 * XOffset, 2 * YOffset, 270, 90);
				path.AddLine(2 * XOffset + contentArea.Width - 2, YOffset, 2 * XOffset + contentArea.Width - 2, controlTop - YOffset);
				path.AddArc(contentArea.Width - 2, controlTop - 2 * YOffset, 2 * XOffset, 2 * YOffset, 0, 90);
				path.AddLine(XOffset + contentArea.Width - 2, controlTop, 2 * XOffset, controlTop);
				path.AddLine(2 * XOffset, controlTop, 0, arrowPoint.Y);
			}
            path.CloseFigure();
			
			// translate the graphicspath by 1 pixel downwards and to the right
			path.Transform(new Matrix(1,0,0,1,1 ,1));

			Graphics containerGraphics = container.CreateGraphics();
			Bitmap bkgndBmp = new Bitmap(container.Width, container.Height, containerGraphics);
			
			Graphics bmpGraphics = Graphics.FromImage(bkgndBmp);

			Bitmap desktopBmp = GetDesktopImage();
			if (desktopBmp != null)
			{
				Rectangle srcRect = container.RectangleToScreen(container.ClientRectangle);
				Screen containerScreen = Screen.FromControl(container);
				int minX = int.MaxValue;
				int minY = int.MaxValue;
				foreach (Screen aScreen in Screen.AllScreens)
				{
					if (aScreen.Bounds.Left < minX)
						minX = aScreen.Bounds.Left;
					if (aScreen.Bounds.Top < minY)
						minY = aScreen.Bounds.Top;
				}
				srcRect.Offset(-minX, -minY);
				bmpGraphics.DrawImage(
					desktopBmp, 
					container.ClientRectangle,
					srcRect,
					GraphicsUnit.Pixel
					);
			}
			
			bmpGraphics.SmoothingMode = SmoothingMode.HighQuality ;

			PathGradientBrush brush = new PathGradientBrush(path);
			brush.WrapMode = WrapMode.Clamp;
			brush.SurroundColors= new Color[]{ endColor };
			brush.CenterColor = startColor;
			brush.FocusScales = new PointF(0.84f, 0.84f);
	
			bmpGraphics.FillPath (brush, path);

			Pen tmpBorderPen = new Pen 
				(
					Color.FromArgb(
					(endColor.R > 12) ? (endColor.R - 12) : 0,
					(endColor.G > 12) ? (endColor.G - 12) : 0,
					(endColor.B > 12) ? (endColor.B - 12) : 0
					),
				(float)1.0
				); 
			bmpGraphics.DrawPath (tmpBorderPen, path);
			tmpBorderPen.Dispose();

			brush.Dispose();

			if (borderColor != Color.Transparent && container.BorderSize > 0)
			{
				Pen myPen = new Pen(borderColor, container.BorderSize);
						
				bmpGraphics.DrawPath (myPen, path);
				myPen.Dispose();
			}
			
			Region pathRegion = new Region(path);
			// When a Region is defined from a GraphicsPath object, the region 
			// is defined as the inner area of the path. This code below includes
			// adjustment to include the path border.
			float inflateBy = 1.0f + 2.0f/(float)pathRegion.GetBounds(bmpGraphics).Width;
			Matrix matrix = new Matrix();
			matrix.Scale(inflateBy, inflateBy);
			matrix.Translate(-1, -1);
			pathRegion.Transform(matrix);
						
			container.Region = pathRegion;

			AddContent(p, bmpGraphics, contentArea, titleYOffset, container);

			container.Size = bkgndBmp.Size;
			//container.BackColor = Color.DimGray;
			container.BackgroundImage = bkgndBmp;

			bmpGraphics.Dispose();
			bmpGraphics = null;

			path.Dispose();
			path = null;

			titleImage.Dispose();
			titleImage = null;

			containerGraphics.Dispose();
		}

		//---------------------------------------------------------------------
		protected virtual void AddContent(
			Balloon.Position p,
			Graphics bmpGraphics,
			Size contentArea,
			int titleYOffset,
			Balloon container
			)
		{
			string contentText = container.ContentText;
			string titleText = container.TitleText;
			Image titleImage = container.TitleImage;
			int controlTop = 0;

			if (contentText != null && contentText.Length > 0)
			{
				Font font = new Font(container.Font.FontFamily, container.Font.Size + 2, FontStyle.Bold );

				if ((p & Balloon.Position.Down) == Balloon.Position.Down)
				{
					controlTop = MinimumArrowHeight;
					bmpGraphics.DrawImage(titleImage, XOffset, controlTop + YOffset );
					bmpGraphics.DrawString(titleText, font, Brushes.Black, new Rectangle(XOffset + titleImage.Width + 10, controlTop + YOffset, contentArea.Width, contentArea.Height));
					bmpGraphics.DrawString(contentText, container.Font, Brushes.Black, new Rectangle(XOffset, controlTop + YOffset + titleYOffset, contentArea.Width, contentArea.Height - titleYOffset));
				}
				else
				{
					controlTop = container.Height - MinimumArrowHeight;
					bmpGraphics.DrawImage(titleImage, XOffset, YOffset);
					bmpGraphics.DrawString(titleText, font, Brushes.Black, new Rectangle(XOffset + titleImage.Width + 10, YOffset, contentArea.Width, contentArea.Height));
					bmpGraphics.DrawString(contentText, container.Font, Brushes.Black, new Rectangle(XOffset, YOffset + titleYOffset, contentArea.Width, contentArea.Height - titleYOffset));
				}
			}
		}

		#endregion

		#region Capturing the Screen Image 

		#region constants and DllImports 

		private const int SM_CXSCREEN=0;
		private const int SM_CYSCREEN=1;
		private const int SM_XVIRTUALSCREEN = 76;
		private const int SM_YVIRTUALSCREEN = 77;
		private const int SM_CXVIRTUALSCREEN = 78;
		private const int SM_CYVIRTUALSCREEN = 79;
		private const int SRCCOPY = 13369376;
		
		[DllImport("user32.dll", EntryPoint="GetDesktopWindow")]
		private static extern IntPtr GetDesktopWindow();
		
		[DllImport("user32.dll",EntryPoint="GetDC")]
		private static extern IntPtr GetDC(IntPtr ptr);
		
		[DllImport("user32.dll",EntryPoint="GetSystemMetrics")]
		private static extern int GetSystemMetrics(int abc);
		
		[DllImport("user32.dll",EntryPoint="GetWindowDC")]
		private static extern IntPtr GetWindowDC(Int32 ptr);
		[DllImport("user32.dll",EntryPoint="ReleaseDC")]
		private static extern IntPtr ReleaseDC(IntPtr hWnd,IntPtr hDc);
		
		[DllImport("gdi32.dll",EntryPoint="DeleteDC")]
		private static extern IntPtr DeleteDC(IntPtr hDc);
		
		[DllImport("gdi32.dll",EntryPoint="DeleteObject")]
		private static extern IntPtr DeleteObject(IntPtr hDc);
		
		[DllImport("gdi32.dll",EntryPoint="BitBlt")]
		private static extern bool BitBlt(IntPtr hdcDest,int xDest,
			int yDest,int wDest,int hDest,IntPtr hdcSource,
			int xSrc,int ySrc,int RasterOp);
		
		[DllImport ("gdi32.dll",EntryPoint="CreateCompatibleBitmap")]
		private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc,	int nWidth, int nHeight);
		
		[DllImport ("gdi32.dll",EntryPoint="CreateCompatibleDC")]
		private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
		
		[DllImport ("gdi32.dll",EntryPoint="SelectObject")]
		private static extern IntPtr SelectObject(IntPtr hdc,IntPtr bmp);

		#endregion // constants and DllImports 
		
		//---------------------------------------------------------------------
		public static Bitmap GetDesktopImage()
		{
			IntPtr  hwndDesktop = GetDesktopWindow();
			if (hwndDesktop == IntPtr.Zero)
				return null;

			//bool isPrimaryScreen = Screen.FromHandle(hwndDesktop).Primary;

			IntPtr  hDC = GetDC(hwndDesktop);
			IntPtr	hMemDC = CreateCompatibleDC(hDC);

			int leftPos = GetSystemMetrics(SM_XVIRTUALSCREEN);
			int topPos = GetSystemMetrics(SM_YVIRTUALSCREEN);
			int imageWidth = GetSystemMetrics(SM_CXVIRTUALSCREEN);
			int imageHeight = GetSystemMetrics(SM_CYVIRTUALSCREEN);

			IntPtr hBitmap = CreateCompatibleBitmap(hDC, imageWidth, imageHeight);

			if (hBitmap != IntPtr.Zero)
			{
				IntPtr hOld = (IntPtr) SelectObject	(hMemDC, hBitmap);
				
				// Copy the Bitmap to the memory device context.
				BitBlt(hMemDC, 0, 0, imageWidth, imageHeight, hDC, leftPos, topPos, SRCCOPY);

				SelectObject(hMemDC, hOld);

				DeleteDC(hMemDC);
				ReleaseDC(hwndDesktop, hDC);

				Bitmap bmp = System.Drawing.Image.FromHbitmap(hBitmap); 

				DeleteObject(hBitmap);

				//Return the bitmap 
				return bmp;
			}
			return null;
		}

		#endregion // Capturing the Screen Image 
	}

}
