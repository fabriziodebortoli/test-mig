using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	#region strutture utili
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int left;
		public int top;
		public int right;
		public int bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BUTTON_IMAGELIST
	{
		public IntPtr himl;
		public RECT margin;
		public int uAlign;
	}

	
	[StructLayout(LayoutKind.Sequential)]
	public struct DLLVERSIONINFO
	{
		public int cbSize;
		public int dwMajorVersion;
		public int dwMinorVersion;
		public int dwBuildNumber;
		public int dwPlatformID;
	}

	//----------------------------------------------------------------------------
	public enum Alignment { Left, Right, Top, Bottom, Center };
	
	#endregion
	
	[Designer(typeof (ButtonDesigner))]
	//----------------------------------------------------------------------------
	public class ImageButton: Button
	{
		private System.Drawing.Imaging.ColorMatrix		grayMatrix;
		private System.Drawing.Imaging.ImageAttributes	grayAttributes;

		#region costruttori
		
		//----------------------------------------------------------------------------
		public ImageButton()
		{
			//è quello che usa il manifest
			FlatStyle = FlatStyle.System;
		
			// Setup the ColorMatrix and ImageAttributes for grayscale images.
			grayMatrix = new ColorMatrix();
			grayMatrix.Matrix00 = 1/3f;
			grayMatrix.Matrix01 = 1/3f;
			grayMatrix.Matrix02 = 1/3f;
			grayMatrix.Matrix10 = 1/3f;
			grayMatrix.Matrix11 = 1/3f;
			grayMatrix.Matrix12 = 1/3f;
			grayMatrix.Matrix20 = 1/3f;
			grayMatrix.Matrix21 = 1/3f;
			grayMatrix.Matrix22 = 1/3f;
			grayAttributes = new ImageAttributes();
			grayAttributes.SetColorMatrix(grayMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
		}
		
		#endregion

		#region membri privati e proprietà
		private const int BCM_SETIMAGELIST = 0x1600 + 2;
		private int ComCtlMajorVersion = -1;
	
		[DllImport("comctl32.dll", EntryPoint="DllGetVersion")]
		private static extern int GetCommonControlDLLVersion(ref DLLVERSIONINFO dvi);
	
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref BUTTON_IMAGELIST lParam);

        private Image image = null;
		private Alignment alignment = Alignment.Left;
		
		//----------------------------------------------------------------------------
        [Category("Appearance"), Description("Button Image"), DefaultValue(null)]
        public new Image Image
        {
            get { return image; }
            set { image = value; Refresh(); }
        }
        
        //----------------------------------------------------------------------------
        [Category("Appearance"), Description("Image Alignment"), DefaultValue(Alignment.Center)]
		public Alignment AlignImage
		{
			get 
			{
				
				return alignment;
			}

			set 
			{
				switch(value)
				{
					case Alignment.Left:
						this.ImageAlign = ContentAlignment.MiddleLeft;
						break;
					case Alignment.Right:
						this.ImageAlign = ContentAlignment.MiddleRight;
						break;
					case Alignment.Center:
						this.ImageAlign = ContentAlignment.MiddleCenter;
						break;
					case Alignment.Top:
						this.ImageAlign = ContentAlignment.TopCenter;
						break;
					case Alignment.Bottom:
						this.ImageAlign = ContentAlignment.BottomCenter;
						break;
				}
				
				alignment = value;

                Refresh();
			}
		}

		#endregion

		#region overload di setimage
		//---------------------------------------------------------------------------
		public void SetImage(Bitmap image)
		{
			SetImage(new Bitmap[] { image }, Alignment.Center, 0, 0, 0, 0);
		}
		
		//---------------------------------------------------------------------------
		public void SetImage(Bitmap image, Alignment align)
		{
			SetImage(new Bitmap[] { image, image, image, GetDisabledImage(image), image }, align, 0, 0, 0, 0);
		}

		//---------------------------------------------------------------------------
		public void SetImage(Bitmap image, Alignment align, int leftMargin, int topMargin, int rightMargin,
			int bottomMargin)
		{
			SetImage(new Bitmap[] { image }, align, leftMargin, topMargin, rightMargin, bottomMargin);
		}

		//---------------------------------------------------------------------------
		public void SetImage(Bitmap normalImage, Bitmap hoverImage, Bitmap pressedImage,
			Bitmap disabledImage, Bitmap focusedImage)
		{
			SetImage(new Bitmap[] { normalImage, hoverImage, pressedImage,
									  disabledImage, focusedImage },
				Alignment.Center, 0, 0, 0, 0);
		}
		
		//---------------------------------------------------------------------------
		public void SetImage(Bitmap normalImage, Bitmap hoverImage, Bitmap pressedImage,
			Bitmap disabledImage, Bitmap focusedImage,
			Alignment align)
		{
			SetImage(new Bitmap[] { normalImage, hoverImage, pressedImage,
									  disabledImage, focusedImage },
				align, 0, 0, 0, 0);
		}

		//---------------------------------------------------------------------------
		public void SetImage(Bitmap normalImage, Bitmap hoverImage, Bitmap pressedImage,
			Bitmap disabledImage, Bitmap focusedImage,
			Alignment align, int leftMargin, int topMargin, int rightMargin,
			int bottomMargin)
		{
			SetImage(new Bitmap[] { normalImage, hoverImage, pressedImage,
									  disabledImage, focusedImage },
				align, leftMargin, topMargin, rightMargin, bottomMargin);
		}
		#endregion

		//---------------------------------------------------------------------------
		private Bitmap GetDisabledImage(Bitmap normalBitmap)
		{
			if (normalBitmap == null)
				return null;
		
			Graphics buttonGraphics = this.CreateGraphics();
			Bitmap bkgndBmp = new Bitmap(normalBitmap.Width, normalBitmap.Height, buttonGraphics);
			
			Graphics bmpGraphics = Graphics.FromImage(bkgndBmp);

			bmpGraphics.DrawImage
				(
				normalBitmap, 
				new Rectangle(0,0, normalBitmap.Width, normalBitmap.Height),
				0, 
				0,
				normalBitmap.Width, 
				normalBitmap.Height, 
				GraphicsUnit.Pixel, 
				grayAttributes
				);

			buttonGraphics.Dispose();
			bmpGraphics.Dispose();

			return bkgndBmp;

		}

		#region funzioni serie
		//---------------------------------------------------------------------------
		protected override void OnCreateControl()
		{
			if (Image != null)
				SetImage((Bitmap)Image, alignment);
		}

		//---------------------------------------------------------------------------
		public void SetImage
			(
			Bitmap[] images,
			Alignment align, 
			int leftMargin, int topMargin, int rightMargin, int bottomMargin
			)
		{
			if (ComCtlMajorVersion < 0)
			{
				DLLVERSIONINFO dllVersion = new DLLVERSIONINFO();
				dllVersion.cbSize = Marshal.SizeOf(typeof(DLLVERSIONINFO));
				GetCommonControlDLLVersion(ref dllVersion);
				ComCtlMajorVersion = dllVersion.dwMajorVersion;
			}

			if (ComCtlMajorVersion >= 6 && FlatStyle == FlatStyle.System)
			{
				RECT rect = new RECT();
				rect.left = leftMargin;
				rect.top = topMargin;
				rect.right = rightMargin;
				rect.bottom = bottomMargin;

				BUTTON_IMAGELIST buttonImageList = new BUTTON_IMAGELIST();
				buttonImageList.margin = rect;
				buttonImageList.uAlign = (int)align;

				ImageList = GenerateImageList(images);
				buttonImageList.himl = ImageList.Handle;

				SendMessage(this.Handle, BCM_SETIMAGELIST, 0, ref buttonImageList);
			}
			else
			{
				FlatStyle = FlatStyle.Standard;

				if (images.Length > 0)
				{
					this.Image = images[0];
				}
			}
            Refresh();

		}

		//---------------------------------------------------------------------------
		private ImageList GenerateImageList(Bitmap[] images)
		{
			ImageList il = new ImageList();
			il.ColorDepth = ColorDepth.Depth32Bit;

			if (images.Length > 0)
			{
				il.ImageSize = new Size(images[0].Width, images[0].Height);

				foreach (Bitmap image in images)
				{
					il.Images.Add(image);
					Bitmap bm = (Bitmap)il.Images[il.Images.Count - 1];

					// copy pixel data from original Bitmap into ImageList
					// to work around a bug in ImageList:
					// adding an image to an ImageList destroys the alpha channel
					for (int x = 0; x < bm.Width; x++)
					{
						for (int y = 0; y < bm.Height; y++)
						{
							bm.SetPixel(x, y, image.GetPixel(x, y));
						}
					}
				}
			}

			return il;
		}
		#endregion
	}
}
