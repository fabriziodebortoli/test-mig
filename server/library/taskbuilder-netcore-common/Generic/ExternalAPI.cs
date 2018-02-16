using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Microarea.Common.Generic
{
	public class ExternalAPI
	{
		#region Structures and constants

		public enum GetWindowLongIndex : int
		{
			GWL_STYLE = -16,
			GWL_EXSTYLE = -20,
			GWL_HWNDPARENT = -8
		}

		public enum WindowStyles : uint
		{
			WS_OVERLAPPED = 0x00000000,
			WS_POPUP = 0x80000000,
			WS_CHILD = 0x40000000,
			WS_MINIMIZE = 0x20000000,
			WS_VISIBLE = 0x10000000,
			WS_DISABLED = 0x08000000,
			WS_CLIPSIBLINGS = 0x04000000,
			WS_CLIPCHILDREN = 0x02000000,
			WS_MAXIMIZE = 0x01000000,
			WS_CAPTION = 0x00C00000,
			WS_BORDER = 0x00800000,
			WS_DLGFRAME = 0x00400000,
			WS_VSCROLL = 0x00200000,
			WS_HSCROLL = 0x00100000,
			WS_SYSMENU = 0x00080000,
			WS_THICKFRAME = 0x00040000,
			WS_GROUP = 0x00020000,
			WS_TABSTOP = 0x00010000,
			WS_MINIMIZEBOX = 0x00020000,
			WS_MAXIMIZEBOX = 0x00010000,
			WS_TILED = 0x00000000,
			WS_ICONIC = 0x20000000,
			WS_SIZEBOX = 0x00040000,
			WS_POPUPWINDOW = 0x80880000,
			WS_OVERLAPPEDWINDOW = 0x00CF0000,
			WS_TILEDWINDOW = 0x00CF0000,
			WS_CHILDWINDOW = 0x40000000
		}

		public enum WindowExStyles
		{
			WS_EX_DLGMODALFRAME = 0x00000001,
			WS_EX_NOPARENTNOTIFY = 0x00000004,
			WS_EX_TOPMOST = 0x00000008,
			WS_EX_ACCEPTFILES = 0x00000010,
			WS_EX_TRANSPARENT = 0x00000020,
			WS_EX_MDICHILD = 0x00000040,
			WS_EX_TOOLWINDOW = 0x00000080,
			WS_EX_WINDOWEDGE = 0x00000100,
			WS_EX_CLIENTEDGE = 0x00000200,
			WS_EX_CONTEXTHELP = 0x00000400,
			WS_EX_RIGHT = 0x00001000,
			WS_EX_LEFT = 0x00000000,
			WS_EX_RTLREADING = 0x00002000,
			WS_EX_LTRREADING = 0x00000000,
			WS_EX_LEFTSCROLLBAR = 0x00004000,
			WS_EX_RIGHTSCROLLBAR = 0x00000000,
			WS_EX_CONTROLPARENT = 0x00010000,
			WS_EX_STATICEDGE = 0x00020000,
			WS_EX_APPWINDOW = 0x00040000,
			WS_EX_OVERLAPPEDWINDOW = 0x00000300,
			WS_EX_PALETTEWINDOW = 0x00000188,
			WS_EX_LAYERED = 0x00080000
		}
		//--------------------------------------------------------------------------------------------------------------------------------
		[StructLayout(LayoutKind.Sequential)]
		public struct SECURITY_DESCRIPTOR
		{
			public byte revision;
			public byte size;
			public short control;
			public IntPtr owner;
			public IntPtr group;
			public IntPtr sacl;
			public IntPtr dacl;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
		public struct WINDOWPLACEMENT
		{
			public UInt32 length;
			public UInt32 flags;
			public UInt32 showCmd;
			public UInt32 ptMinPositionX;
			public UInt32 ptMinPositionY;
			public UInt32 ptMaxPositionX;
			public UInt32 ptMaxPositionY;
			public UInt32 rcNormalPositionLeft;
			public UInt32 rcNormalPositionTop;
			public UInt32 rcNormalPositionRight;
			public UInt32 rcNormalPositionBottom;
		}
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public class LOGFONT
		{
			public long lfHeight;
			public long lfWidth;
			public long lfEscapement;
			public long lfOrientation;
			public long lfWeight;
			public byte lfItalic;
			public byte lfUnderline;
			public byte lfStrikeOut;
			public byte lfCharSet;
			public byte lfOutPrecision;
			public byte lfClipPrecision;
			public byte lfQuality;
			public byte lfPitchAndFamily;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string lfFaceName;
			public LOGFONT()
			{ }
			public LOGFONT(LOGFONT lf)
			{
				this.lfHeight = lf.lfHeight;
				this.lfWidth = lf.lfWidth;
				this.lfEscapement = lf.lfEscapement;
				this.lfOrientation = lf.lfOrientation;
				this.lfWeight = lf.lfWeight;
				this.lfItalic = lf.lfItalic;
				this.lfUnderline = lf.lfUnderline;
				this.lfStrikeOut = lf.lfStrikeOut;
				this.lfCharSet = lf.lfCharSet;
				this.lfOutPrecision = lf.lfOutPrecision;
				this.lfClipPrecision = lf.lfClipPrecision;
				this.lfQuality = lf.lfQuality;
				this.lfPitchAndFamily = lf.lfPitchAndFamily;
				this.lfFaceName = lf.lfFaceName;

			}
			public override string ToString()
			{
				return string.Concat(new object[] { 
					"lfHeight=", this.lfHeight, ", lfWidth=", this.lfWidth, ", lfEscapement=", this.lfEscapement, ", lfOrientation=", this.lfOrientation, ", lfWeight=", this.lfWeight, ", lfItalic=", this.lfItalic, ", lfUnderline=", this.lfUnderline, ", lfStrikeOut=", this.lfStrikeOut, 
					", lfCharSet=", this.lfCharSet, ", lfOutPrecision=", this.lfOutPrecision, ", lfClipPrecision=", this.lfClipPrecision, ", lfQuality=", this.lfQuality, ", lfPitchAndFamily=", this.lfPitchAndFamily, ", lfFaceName=", this.lfFaceName
				 });

			}
		}


		[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
		public struct Rect
		{
			public int left;
			public int top;
			public int right;
			public int bottom;

			public Rect(int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}
			public Rect(Rectangle r)
			{
				this.left = r.Left;
				this.top = r.Top;
				this.right = r.Right;
				this.bottom = r.Bottom;
			}

			public static Rect Empty { get { return new Rect(0, 0, 0, 0);  } }
		}

		public const UInt32 SWP_NOSIZE = 0x0001;
		public const UInt32 SWP_NOMOVE = 0x0002;
		public const UInt32 SWP_NOZORDER = 0x0004;
		public const UInt32 SWP_NOREDRAW = 0x0008;
		public const UInt32 SWP_NOACTIVATE = 0x0010;
		public const UInt32 SWP_FRAMECHANGED = 0x0020; /* The frame changed: send WM_NCCALCSIZE */
		public const UInt32 SWP_SHOWWINDOW = 0x0040;
		public const UInt32 SWP_HIDEWINDOW = 0x0080;
		public const UInt32 SWP_NOCOPYBITS = 0x0100;
		public const UInt32 SWP_NOOWNERZORDER = 0x0200;  /* Don't do owner Z ordering */
		public const UInt32 SWP_NOSENDCHANGING = 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */
		public const UInt32 SWP_DRAWFRAME = SWP_FRAMECHANGED;
		public const UInt32 SWP_NOREPOSITION = SWP_NOOWNERZORDER;
		public const UInt32 SWP_DEFERERASE = 0x2000;
		public const UInt32 SWP_ASYNCWINDOWPOS = 0x4000;

		public const int HWND_TOP = 0;
		public const int HWND_BOTTOM = 1;
		public const int HWND_TOPMOST = -1;
		public const int HWND_NOTOPMOST = -2;

		public const UInt32 SW_HIDE = 0;
		public const UInt32 SW_SHOWNORMAL = 1;
		public const UInt32 SW_NORMAL = 1;
		public const UInt32 SW_SHOWMINIMIZED = 2;
		public const UInt32 SW_SHOWMAXIMIZED = 3;
		public const UInt32 SW_MAXIMIZE = 3;
		public const UInt32 SW_SHOWNOACTIVATE = 4;
		public const UInt32 SW_SHOW = 5;
		public const UInt32 SW_MINIMIZE = 6;
		public const UInt32 SW_SHOWMINNOACTIVE = 7;
		public const UInt32 SW_SHOWNA = 8;
		public const UInt32 SW_RESTORE = 9;
		public const UInt32 SW_SHOWDEFAULT = 10;
		public const UInt32 SW_FORCEMINIMIZE = 11;
		public const UInt32 SW_MAX = 11;


		public const int WM_DESTROY = 0x2;
		public const int WM_LBUTTONDOWN = 0x0201;
		public const int WM_LBUTTONUP = 0x0202;
		public const int WM_LBUTTONDBLCLK = 0x0203;
		public const int WM_RBUTTONDOWN = 0x0204;
		public const int WM_MBUTTONDOWN = 0x0207;
		public const int WM_MOUSEMOVE = 0x0200;
		public const int WM_CLOSE = 0x0010;
		public const int WM_USER = 0x0400;
		public const int WM_COMMAND = 0x0111;
		public const int WM_MOUSEACTIVATE = 0x0021;
		public const int WM_KEYDOWN = 0x0100;
		public const int WM_KEYFIRST = 0x0100;
		public const int WM_KEYLAST = 0x0108;
		public const int WM_ACTIVATE = 0x0006;
		public const int WM_ACTIVATEAPP = 0x001C;
		public const int WM_PAINT = 0x000F;
		public const int WM_ENABLE = 0x000A;
		public const int WM_VSCROLL = 0x0115;
		public const int WM_ENTERSIZEMOVE = 0x0231;
		public const int WM_EXITSIZEMOVE = 0x0232;
		public const int WM_SIZING = 0x0214;

		public const int GW_HWNDFIRST = 0;
		public const int GW_HWNDLAST = 1;
		public const int GW_HWNDNEXT = 2;
		public const int GW_HWNDPREV = 3;
		public const int GW_OWNER = 4;
		public const int GW_CHILD = 5;

		public const int WA_INACTIVE = 0;
		public const int WA_ACTIVE = 1;
		public const int WA_CLICKACTIVE = 2;

		public const int DACL_SECURITY_INFORMATION = 4;


		//see file Framework\TBNameSolver\UserMessages.h and verify this constants are aligned
		public const int UM_GET_SOAP_PORT = ExternalAPI.WM_USER + 904;
		public const int UM_GET_LOCALIZER_INFO = ExternalAPI.WM_USER + 906;
		public const int UM_UPDATE_FRAME_STATUS = ExternalAPI.WM_USER + 907;
		public const int UM_DOCUMENT_CREATED = ExternalAPI.WM_USER + 908;
		public const int UM_DOCUMENT_DESTROYED = ExternalAPI.WM_USER + 909;
		public const int UM_UPDATE_EXTERNAL_MENU = ExternalAPI.WM_USER + 910;
		public const int UM_GET_DOC_NAMESPACE_ICON = ExternalAPI.WM_USER + 911;
		public const int UM_FRAME_TITLE_UPDATED = ExternalAPI.WM_USER + 912;
		public const int UM_SET_STATUS_BAR_TEXT = ExternalAPI.WM_USER + 916;
		public const int UM_CLEAR_STATUS_BAR = ExternalAPI.WM_USER + 917;
		public const int UM_SET_MENU_WINDOW_TEXT = ExternalAPI.WM_USER + 918;
		public const int UM_ACTIVATE_TAB = ExternalAPI.WM_USER + 923;
		public const int UM_FRAME_ACTIVATE = ExternalAPI.WM_USER + 928;
		public const int UM_GET_PARSEDCTRL_TYPE = ExternalAPI.WM_USER + 936;
		public const int UM_RANGE_SELECTOR_CLOSED = ExternalAPI.WM_USER + 937;
		public const int UM_RANGE_SELECTOR_SELECTED = ExternalAPI.WM_USER + 938;
		public const int UM_SET_USER_PANEL_TEXT = ExternalAPI.WM_USER + 939;
		public const int UM_CLONE_DOCUMENT = ExternalAPI.WM_USER + 940;
		public const int UM_GET_DOCUMENT_TITLE_INFO = ExternalAPI.WM_USER + 942;
		public const int UM_REFRESH_USER_OBJECTS = ExternalAPI.WM_USER + 943;
		public const int UM_SWITCH_ACTIVE_TAB = ExternalAPI.WM_USER + 944;
		public const int UM_DESTROYING_DOCKABLE_FRAME = ExternalAPI.WM_USER + 945;
		public const int UM_REFRESH_USER_LOGIN =  ExternalAPI.WM_USER + 946;
		//public const int UM_CHOOSE_CUSTOMIZATION_CONTEXT_AND_EASYSTUDIO_AGAIN =  ExternalAPI.WM_USER + 947;
		public const int UM_IS_ROOT_DOCUMENT = ExternalAPI.WM_USER + 948;
        public const int UM_IMMEDIATE_BALLOON = ExternalAPI.WM_USER + 949;
		public const int UM_HAS_INVALID_VIEW = ExternalAPI.WM_USER + 950;
        public const int UM_MAGO_LINKER = ExternalAPI.WM_USER + 951;
        public const int UM_REFRESH_ORGANIZER = ExternalAPI.WM_USER + 952;
		public const int UM_MENU_CREATED = ExternalAPI.WM_USER + 955;
		public const int UM_EXTDOC_BATCH_COMPLETED = ExternalAPI.WM_USER + 956;
		public const int UM_CHANGE_LOGIN = ExternalAPI.WM_USER + 957;
		public const int UM_DEFERRED_CREATE_SLAVE = ExternalAPI.WM_USER + 958;
        public const int UM_ACTIVATE_MENU = ExternalAPI.WM_USER + 959;
        public const int UM_LOGIN_COMPLETED	= ExternalAPI.WM_USER + 961;
        public const int UM_SHOW_IN_OPEN_DOCUMENTS = ExternalAPI.WM_USER + 962;
        public const int UM_NEW_LOGIN = ExternalAPI.WM_USER + 963;
        public const int UM_ACTIVATE_INTERNET = ExternalAPI.WM_USER + 964;
        public const int UM_RELOGIN_COMPLETED = ExternalAPI.WM_USER + 965;
        public const int UM_OPEN_CUSTOMIZATIONMANAGER = ExternalAPI.WM_USER + 966;
		public const int UM_OPEN_MENUEDITOR = ExternalAPI.WM_USER + 967;
        public const int UM_LOGGING = ExternalAPI.WM_USER + 968;
        public const int UM_LOGIN_INCOMPLETED= ExternalAPI.WM_USER + 969;
		//public const int UM_CHOOSE_CUSTOMIZATION_CONTEXT = ExternalAPI.WM_USER + 970;
		public const int UM_PIN_UNPIN_ACTIONS = ExternalAPI.WM_USER + 971;
		public const int UM_LAYOUT_SUSPENDED_CHANGED = ExternalAPI.WM_USER + 972;
		public const int UM_TOOLBAR_UPDATE = ExternalAPI.WM_USER + 973;
		public const int UM_OPEN_URL = ExternalAPI.WM_USER + 974;

		//see file Framework\TBNameSolver\UserMessages.h and verify this constants are aligned
		public const int REFRESH_USER_DOCUMENT = 1;
		public const int REFRESH_USER_REPORT = 2;
		public const int RELOAD_ALL_MENUS = 3;

		#endregion

		#region User32 APIs
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern uint RegisterWindowMessage(string lpString);

		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern IntPtr SetCapture(IntPtr hWnd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool ReleaseCapture();
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool GetClientRect(IntPtr hWnd, ref Rect clientRect);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool GetWindowRect(IntPtr hWnd, ref Rect windowRect);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool ScreenToClient(IntPtr hWnd, ref Point pt);
		
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT windowPlacement);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool ShowWindow(IntPtr hWnd, UInt32 showCmd);
		[DllImport("user32")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32")]
		public static extern bool AllowSetForegroundWindow(Int32 processId);
		[DllImport("user32")]
		public static extern IntPtr GetForegroundWindow();
		[DllImport("user32")]
		public static extern bool AttachThreadInput(UInt32 aProcessId, UInt32 aToProcessId, bool attach);
		[DllImport("user32")]
		public static extern UInt32 GetWindowThreadProcessId(IntPtr hWnd, ref UInt32 processId);
		[DllImport("user32")]
		public static extern UInt32 GetCurrentThreadId();
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool ShowWindow(IntPtr hWnd, int showCmd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern IntPtr SetActiveWindow(IntPtr hWnd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, UInt32 nFlags);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern int GetWindowTextLength(IntPtr hWnd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern IntPtr GetWindow(IntPtr hWnd, UInt32 uCmd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool IsWindow(IntPtr hWnd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool IsWindowEnabled(IntPtr hWnd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool EnableWindow(IntPtr hWnd, bool enable);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern IntPtr SetFocus(IntPtr hWnd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool UpdateWindow(IntPtr hWnd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool IsWindowVisible(IntPtr hWnd);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool InvalidateRect(IntPtr hWnd, IntPtr windowRect, bool bErase);
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern bool InvalidateRect(IntPtr hWnd, ref Rect windowRect, bool bErase);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr GetFocus();
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr GetParent(IntPtr handle);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetWindowLong(IntPtr hWnd, int Index);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int SetWindowLong(IntPtr hWnd, int Index, int Value);
		[DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
		public static extern bool BitBlt(
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
		[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
		public struct POINT
		{
			public int x;
			public int y;
		}
		[DllImport("User32", CharSet = CharSet.Unicode)]
		public static extern IntPtr WindowFromPoint(POINT Point);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool SetKernelObjectSecurity(int Handle, int SecurityInformation, ref SECURITY_DESCRIPTOR SecurityDescriptor);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public extern static bool DestroyIcon(IntPtr handle);



		//-----------------------------------------------------------------------------------------
		public static string GetFullWindowText(IntPtr handle)
		{
			int bufferSize = GetWindowTextLength(handle);
			StringBuilder sbWindowText = new StringBuilder(bufferSize + 1);
			if (GetWindowText(handle, sbWindowText, bufferSize + 1) == 0)
				sbWindowText.Append("No title");

			return sbWindowText.ToString();
		}

		#endregion

		#region Kernel32 APIs
		[DllImport("kernel32")]
		public static extern int ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);
		[DllImport("kernel32")]
		public static extern long GetLastError();

        [DllImport("kernel32.dll")]
        public static extern uint GetProfileString(string lpAppName, string lpKeyName,
           string lpDefault, [Out] StringBuilder lpReturnedString, uint nSize);

		#endregion


		
	}
}

