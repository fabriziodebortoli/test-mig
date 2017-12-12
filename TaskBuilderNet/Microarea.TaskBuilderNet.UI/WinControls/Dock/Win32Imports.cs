using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock.Win32
{
	internal class User32
	{
		public delegate IntPtr WndProcCallBack(IntPtr hwnd, int Msg, IntPtr wParam, IntPtr lParam);
			
		[DllImport("User32.dll")]
		public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, WndProcCallBack wndProcCallBack);
		
		[DllImport("User32.dll")]
		public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr wndFunc);
		
		[DllImport("User32.dll")]
		public static extern IntPtr CallWindowProc(IntPtr prevWndFunc, IntPtr hWnd, int iMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern UInt32 SetForegroundWindow(IntPtr hWnd);

		[DllImport("User32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool DragDetect(IntPtr hWnd, Point pt);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr GetSysColorBrush(int index);
		
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool InvalidateRect(IntPtr hWnd, ref RECT rect, bool erase);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr LoadCursor(IntPtr hInstance, uint cursor);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr SetCursor(IntPtr hCursor);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr GetFocus();

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr SetFocus(IntPtr hWnd);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool ReleaseCapture();

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool WaitMessage();

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool TranslateMessage(ref MSG msg);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool DispatchMessage(ref MSG msg);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool PostMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern uint SendMessage(IntPtr hWnd, int Msg, uint wParam, uint lParam);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern uint SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool GetMessage(ref MSG msg, int hWnd, uint wFilterMin, uint wFilterMax);
	
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool PeekMessage(ref MSG msg, int hWnd, uint wFilterMin, uint wFilterMax, uint wFlag);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr GetWindowDC(IntPtr hWnd);
		
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern int ShowWindow(IntPtr hWnd, short cmdShow);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int X, int Y, int Width, int Height, FlagsSetWindowPos flags);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool ClientToScreen(IntPtr hWnd, ref POINT pt);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool TrackMouseEvent(ref TRACKMOUSEEVENTS tme);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern ushort GetKeyState(int virtKey);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool DrawFocusRect(IntPtr hWnd, ref RECT rect);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool HideCaret(IntPtr hWnd);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool ShowCaret(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern bool SystemParametersInfo(SystemParametersInfoActions uAction, uint uParam, ref uint lpvParam, uint fuWinIni);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr WindowFromPoint(POINT point);
	}
	
	internal class Gdi32
	{
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern int CombineRgn(IntPtr dest, IntPtr src1, IntPtr src2, int flags);

		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr CreateRectRgnIndirect(ref Win32.RECT rect); 

		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern bool FillRgn(IntPtr hDC, IntPtr hrgn, IntPtr hBrush); 
		
		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern int GetClipBox(IntPtr hDC, ref Win32.RECT rectBox); 

		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern int SelectClipRgn(IntPtr hDC, IntPtr hRgn); 

		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr CreateBrushIndirect(ref LOGBRUSH brush); 

		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern bool PatBlt(IntPtr hDC, int x, int y, int width, int height, uint flags); 

		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr DeleteObject(IntPtr hObject);

		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern bool DeleteDC(IntPtr hDC);

		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

		[DllImport("gdi32.dll", CharSet=CharSet.Auto)]
		public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
	}
}