using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// Summary description for StaticImageControl.
	/// </summary>
	public partial class ControlFreezer : System.Windows.Forms.UserControl
	{
		// per invocare i metodi Freeze e Defreeze da un altro thread tramite invoke
		public delegate void FreezerDelegate();

		//---------------------------------------------------------------------
		public ControlFreezer(Control sourceControl)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.sourceControl = sourceControl;
			
			// devo crearlo adesso in modo da poterne invocare i metodi
			// da un'altro thread usando la Invoke
			CreateControl();
		}

		//---------------------------------------------------------------------
		public Image GetControlImage()
		{
			Graphics g1 = null, g2 = null;
			IntPtr dc1 = IntPtr.Zero, dc2 = IntPtr.Zero;
			try
			{
				// libero la coda di messaggi per un eventuale ridisegno del controllo
				Application.DoEvents();

				// recupera il wrapper del device context (origine)
				g1 = sourceControl.CreateGraphics();

				// crea l'immagine vuota
				Image img = new Bitmap(sourceControl.ClientRectangle.Width, sourceControl.ClientRectangle.Height, g1);

				// ne recupera il wrapper del device context (destinazione)
				g2 = Graphics.FromImage(img);

				// recupera gli handle dei device context
				dc1 = g1.GetHdc();
				dc2 = g2.GetHdc();

				// copia il contenuto del device context di origine in quello di destinazione
				BitBlt(dc2, 0, 0, sourceControl.ClientRectangle.Width, sourceControl.ClientRectangle.Height, dc1, 0, 0, 13369376);

				return img;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Fail(ex.Message, ex.StackTrace);
				return null;
			}
			finally 
			{
				// rilascia i device context
				if (g1 != null)
				{
					if (dc1 != IntPtr.Zero)
						g1.ReleaseHdc(dc1);
					g1.Dispose();
				}

				
				if (g2 != null)
				{
					if (dc2 != IntPtr.Zero)
						g2.ReleaseHdc(dc2);
					g2.Dispose();
				}
			}
		}

		//---------------------------------------------------------------------
		public void Freeze()
		{
			Image img = GetControlImage();
			if (img == null) return;

			staticImage.Image = img;
			Height = staticImage.Image.Height;
			Width = staticImage.Image.Width;
		
			sourceControl.Controls.Add(this);
			
			// lascio che si ridisegni il controllo
			Application.DoEvents();
		}

		//---------------------------------------------------------------------
		public void Defreeze()
		{
			if (sourceControl.Controls.Contains(this))
				sourceControl.Controls.Remove(this);
		}

		//---------------------------------------------------------------------
		[System.Runtime.InteropServices.DllImportAttribute("gdi32.dll")]
		private static extern bool BitBlt(
				IntPtr hdcDest, // handle to destination DC
				int nXDest,  // x-coord of destination upper-left corner
				int nYDest,  // y-coord of destination upper-left corner
				int nWidth,  // width of destination rectangle
				int nHeight, // height of destination rectangle
				IntPtr hdcSrc,  // handle to source DC
				int nXSrc,   // x-coordinate of source upper-left corner
				int nYSrc,   // y-coordinate of source upper-left corner
				System.Int32 dwRop  // raster operation code
			);
	}
}
