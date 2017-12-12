using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyAttachment.Components
{
	///<summary>
	/// ParsedControlWrapper
	/// Wrapper di un parsed control di una form di Mago che mi consente di portarmi dietro
	/// le info del controllo durante l'operazione di Drag&Drop nel DataGridView dei CollectionFields
	/// (ovvero i campi di ricerca) per un documento.
	///</summary>
	//================================================================================
	public class ParsedControlWrapper : NativeWindow, IDisposable
	{
		//private bool dragEnabled = false;
		private Control owner = null;
		private IntPtr hwnd;
		private Rectangle rectangle; //rettangolo della client area della finestra wrappata
		
		//--------------------------------------------------------------------------------
		public void Activate(Control owner)
		{
			this.owner = owner;
			ExternalAPI.Rect rect = new ExternalAPI.Rect(0, 0, 0, 0);
			ExternalAPI.GetClientRect(hwnd, ref rect);

			rectangle = new Rectangle(0, 0, rect.right - rect.left, rect.bottom - rect.top);
			AssignHandle(hwnd);

			ExternalAPI.EnableWindow(hwnd, true);
			UpdateView();
		}

		//--------------------------------------------------------------------------------
		public void Deactivate()
		{
			owner = null;
			ReleaseHandle();
			UpdateView();
		}
		
		//--------------------------------------------------------------------------------
		public ParsedControlWrapper(IntPtr hwnd)
		{
			this.hwnd = hwnd;
		}
		
		//--------------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			if (m.Msg >= ExternalAPI.WM_KEYFIRST && m.Msg <= ExternalAPI.WM_KEYLAST)
				return;
			
			switch (m.Msg)
			{
				case ExternalAPI.WM_PAINT:
					{
						base.WndProc(ref m);

						//using (Graphics g = Graphics.FromHwnd(Handle))
						//{
							// visualizzazione bitmap in fondo al control
							/*using (Bitmap bmp = Microarea.EasyAttachment.Properties.Resources.Pin16x16)
							{
								int x = rectangle.Right - 16;
								g.DrawImage(bmp, x, 0, 16, 16);
							}*/
							
							// disegna uno sfondo azzurrino sul rettangolo del control (peccato che sovrascriva troppo e non va bene)
							//g.FillRectangle(Brushes.AliceBlue, rectangle);
							
							// disegna una linea azzurra in alto e a sx del control
							/*using (Pen myPen = new Pen(Brushes.CadetBlue, 3))
							{
								// linea verticale a sx
								g.DrawLine(myPen, new Point(0, 0), new Point(0, rectangle.Height));
								// linea orizzonatale sopra
								g.DrawLine(myPen, new Point(0, 0), new Point(rectangle.Width, 0));
							}*/
						//}
						
						return;
					}
				case ExternalAPI.WM_ENABLE:
					{
						//mantengo la finestra in stato di abilitato
						if (m.WParam == IntPtr.Zero)
							ExternalAPI.EnableWindow(Handle, true);
						return;
					}
				case ExternalAPI.WM_LBUTTONDOWN:
					{
						//base.WndProc(ref m);
						//dragEnabled = true;
						return;
					}
				case ExternalAPI.WM_LBUTTONUP:
					{
						//base.WndProc(ref m);
						//dragEnabled = false;
						return;
					}

				case 0x02A3: //ExternalAPI.WM_MOUSELEAVE:
					{
						break;
					}

				case ExternalAPI.WM_MOUSEMOVE:
					{
						base.WndProc(ref m);
						using (Graphics g = Graphics.FromHwnd(Handle))
						{
							using (Pen myPen = new Pen(Brushes.CadetBlue, 3))
							{
								// linea verticale a sx
								g.DrawLine(myPen, new Point(0, 0), new Point(0, rectangle.Height));
								// linea orizzonatale sopra
								g.DrawLine(myPen, new Point(0, 0), new Point(rectangle.Width, 0));
							}							
						}

						//the use has the mouse left button clicked
						if (m.WParam.ToInt32() == 0x0001) //MK_LBUTTON
						{
							if (owner != null)
								owner.DoDragDrop(new DragDropData(this), DragDropEffects.Copy);
							return;
						}
						break;
					}

				case ExternalAPI.WM_KEYFIRST:
					{					
						return;
					}
				// al momento il doppio click non riceve l'evento perche' viene mangiato dal mousemove che scatena poi il Drag&Drop
				/*case ExternalAPI.WM_LBUTTONDBLCLK:
					{
						base.WndProc(ref m);
						archiveDocForm.AddFieldWithDoubleClick(this);
						return;
					}
					*/
			}
			base.WndProc(ref m);
		}

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			Deactivate();
		}

		//--------------------------------------------------------------------------------
		public virtual void UpdateView()
		{ 
			//overridato dalla classe figlia
		}

		//--------------------------------------------------------------------------------
		internal bool HasHandle(IntPtr handle)
		{
			return hwnd == handle;
		}
		//--------------------------------------------------------------------------------
		internal void DrawRectangle()
		{
			//int cxBorder = GetSystemMetrics(SM_CXBORDER);
			//CDC* pDC;
			//CRect rectWnd;
			//pDC = pWndToTrack->GetWindowDC();
			//pWndToTrack->GetWindowRect(&rectWnd);
			//pDC->SetROP2(R2_NOT);
			//COLORREF cr = RGB(0, 0, 0);
			//CPen* pPen = new CPen(PS_INSIDEFRAME, 3 * cxBorder, cr);
			//CPen* pOldPen = pDC->SelectObject(pPen);
			//CGdiObject* pOldBrush = pDC->SelectStockObject(NULL_BRUSH);
			//pDC->Rectangle
			//    (
			//        rectToTrack.left - rectWnd.left,
			//        rectToTrack.top - rectWnd.top,
			//        rectToTrack.right - rectWnd.left,
			//        rectToTrack.bottom - rectWnd.top
			//    );

			//pDC->SelectObject(pOldPen);
			//pDC->SelectObject(pOldBrush);
			//pWndToTrack->ReleaseDC(pDC);
			//delete pPen;

			using (Graphics g = Graphics.FromHwnd(Handle))
			{
				//Rectangle rect = new Rectangle();
				using (Pen myPen = new Pen(Brushes.CadetBlue, 3))
					g.DrawRectangle(myPen, rectangle);
					
			}		

		}
	}
}
