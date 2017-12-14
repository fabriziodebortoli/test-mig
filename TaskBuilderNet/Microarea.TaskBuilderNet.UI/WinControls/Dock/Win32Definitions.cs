using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Microarea.TaskBuilderNet.UI.WinControls.Dock.Win32
{
	internal enum PeekMessageFlags
	{
		PM_NOREMOVE		= 0,
		PM_REMOVE		= 1,
		PM_NOYIELD		= 2
	}

	[Flags]
	internal enum FlagsSetWindowPos : uint
	{
		SWP_NOSIZE          = 0x0001,
		SWP_NOMOVE          = 0x0002,
		SWP_NOZORDER        = 0x0004,
		SWP_NOREDRAW        = 0x0008,
		SWP_NOACTIVATE      = 0x0010,
		SWP_FRAMECHANGED    = 0x0020,
		SWP_SHOWWINDOW      = 0x0040,
		SWP_HIDEWINDOW      = 0x0080,
		SWP_NOCOPYBITS      = 0x0100,
		SWP_NOOWNERZORDER   = 0x0200, 
		SWP_NOSENDCHANGING  = 0x0400,
		SWP_DRAWFRAME       = 0x0020,
		SWP_NOREPOSITION    = 0x0200,
		SWP_DEFERERASE      = 0x2000,
		SWP_ASYNCWINDOWPOS  = 0x4000
	}

	internal enum SetWindowPosZ 
	{
		HWND_TOP        = 0,
		HWND_BOTTOM     = 1,
		HWND_TOPMOST    = -1,
		HWND_NOTOPMOST  = -2
	}

	internal enum ShowWindowStyles : short
	{
		SW_HIDE             = 0,
		SW_SHOWNORMAL       = 1,
		SW_NORMAL           = 1,
		SW_SHOWMINIMIZED    = 2,
		SW_SHOWMAXIMIZED    = 3,
		SW_MAXIMIZE         = 3,
		SW_SHOWNOACTIVATE   = 4,
		SW_SHOW             = 5,
		SW_MINIMIZE         = 6,
		SW_SHOWMINNOACTIVE  = 7,
		SW_SHOWNA           = 8,
		SW_RESTORE          = 9,
		SW_SHOWDEFAULT      = 10,
		SW_FORCEMINIMIZE    = 11,
		SW_MAX              = 11
	}

	internal enum WindowStyles : uint
	{
		WS_OVERLAPPED       = 0x00000000,
		WS_POPUP            = 0x80000000,
		WS_CHILD            = 0x40000000,
		WS_MINIMIZE         = 0x20000000,
		WS_VISIBLE          = 0x10000000,
		WS_DISABLED         = 0x08000000,
		WS_CLIPSIBLINGS     = 0x04000000,
		WS_CLIPCHILDREN     = 0x02000000,
		WS_MAXIMIZE         = 0x01000000,
		WS_CAPTION          = 0x00C00000,
		WS_BORDER           = 0x00800000,
		WS_DLGFRAME         = 0x00400000,
		WS_VSCROLL          = 0x00200000,
		WS_HSCROLL          = 0x00100000,
		WS_SYSMENU          = 0x00080000,
		WS_THICKFRAME       = 0x00040000,
		WS_GROUP            = 0x00020000,
		WS_TABSTOP          = 0x00010000,
		WS_MINIMIZEBOX      = 0x00020000,
		WS_MAXIMIZEBOX      = 0x00010000,
		WS_TILED            = 0x00000000,
		WS_ICONIC           = 0x20000000,
		WS_SIZEBOX          = 0x00040000,
		WS_POPUPWINDOW      = 0x80880000,
		WS_OVERLAPPEDWINDOW = 0x00CF0000,
		WS_TILEDWINDOW      = 0x00CF0000,
		WS_CHILDWINDOW      = 0x40000000
	}

	internal enum WindowExStyles
	{
		WS_EX_DLGMODALFRAME     = 0x00000001,
		WS_EX_NOPARENTNOTIFY    = 0x00000004,
		WS_EX_TOPMOST           = 0x00000008,
		WS_EX_ACCEPTFILES       = 0x00000010,
		WS_EX_TRANSPARENT       = 0x00000020,
		WS_EX_MDICHILD          = 0x00000040,
		WS_EX_TOOLWINDOW        = 0x00000080,
		WS_EX_WINDOWEDGE        = 0x00000100,
		WS_EX_CLIENTEDGE        = 0x00000200,
		WS_EX_CONTEXTHELP       = 0x00000400,
		WS_EX_RIGHT             = 0x00001000,
		WS_EX_LEFT              = 0x00000000,
		WS_EX_RTLREADING        = 0x00002000,
		WS_EX_LTRREADING        = 0x00000000,
		WS_EX_LEFTSCROLLBAR     = 0x00004000,
		WS_EX_RIGHTSCROLLBAR    = 0x00000000,
		WS_EX_CONTROLPARENT     = 0x00010000,
		WS_EX_STATICEDGE        = 0x00020000,
		WS_EX_APPWINDOW         = 0x00040000,
		WS_EX_OVERLAPPEDWINDOW  = 0x00000300,
		WS_EX_PALETTEWINDOW     = 0x00000188,
		WS_EX_LAYERED			= 0x00080000
	}

	internal enum VirtualKeys
	{
		VK_LBUTTON		= 0x01,
		VK_CANCEL		= 0x03,
		VK_BACK			= 0x08,
		VK_TAB			= 0x09,
		VK_CLEAR		= 0x0C,
		VK_RETURN		= 0x0D,
		VK_SHIFT		= 0x10,
		VK_CONTROL		= 0x11,
		VK_MENU			= 0x12,
		VK_CAPITAL		= 0x14,
		VK_ESCAPE		= 0x1B,
		VK_SPACE		= 0x20,
		VK_PRIOR		= 0x21,
		VK_NEXT			= 0x22,
		VK_END			= 0x23,
		VK_HOME			= 0x24,
		VK_LEFT			= 0x25,
		VK_UP			= 0x26,
		VK_RIGHT		= 0x27,
		VK_DOWN			= 0x28,
		VK_SELECT		= 0x29,
		VK_EXECUTE		= 0x2B,
		VK_SNAPSHOT		= 0x2C,
		VK_HELP			= 0x2F,
		VK_0			= 0x30,
		VK_1			= 0x31,
		VK_2			= 0x32,
		VK_3			= 0x33,
		VK_4			= 0x34,
		VK_5			= 0x35,
		VK_6			= 0x36,
		VK_7			= 0x37,
		VK_8			= 0x38,
		VK_9			= 0x39,
		VK_A			= 0x41,
		VK_B			= 0x42,
		VK_C			= 0x43,
		VK_D			= 0x44,
		VK_E			= 0x45,
		VK_F			= 0x46,
		VK_G			= 0x47,
		VK_H			= 0x48,
		VK_I			= 0x49,
		VK_J			= 0x4A,
		VK_K			= 0x4B,
		VK_L			= 0x4C,
		VK_M			= 0x4D,
		VK_N			= 0x4E,
		VK_O			= 0x4F,
		VK_P			= 0x50,
		VK_Q			= 0x51,
		VK_R			= 0x52,
		VK_S			= 0x53,
		VK_T			= 0x54,
		VK_U			= 0x55,
		VK_V			= 0x56,
		VK_W			= 0x57,
		VK_X			= 0x58,
		VK_Y			= 0x59,
		VK_Z			= 0x5A,
		VK_NUMPAD0		= 0x60,
		VK_NUMPAD1		= 0x61,
		VK_NUMPAD2		= 0x62,
		VK_NUMPAD3		= 0x63,
		VK_NUMPAD4		= 0x64,
		VK_NUMPAD5		= 0x65,
		VK_NUMPAD6		= 0x66,
		VK_NUMPAD7		= 0x67,
		VK_NUMPAD8		= 0x68,
		VK_NUMPAD9		= 0x69,
		VK_MULTIPLY		= 0x6A,
		VK_ADD			= 0x6B,
		VK_SEPARATOR	= 0x6C,
		VK_SUBTRACT		= 0x6D,
		VK_DECIMAL		= 0x6E,
		VK_DIVIDE		= 0x6F,
		VK_ATTN			= 0xF6,
		VK_CRSEL		= 0xF7,
		VK_EXSEL		= 0xF8,
		VK_EREOF		= 0xF9,
		VK_PLAY			= 0xFA,  
		VK_ZOOM			= 0xFB,
		VK_NONAME		= 0xFC,
		VK_PA1			= 0xFD,
		VK_OEM_CLEAR	= 0xFE,
		VK_LWIN			= 0x5B,
		VK_RWIN			= 0x5C,
		VK_APPS			= 0x5D,   
		VK_LSHIFT		= 0xA0,   
		VK_RSHIFT		= 0xA1,   
		VK_LCONTROL		= 0xA2,   
		VK_RCONTROL		= 0xA3,   
		VK_LMENU		= 0xA4,   
		VK_RMENU		= 0xA5
	}

	internal enum Msgs
	{
		WM_NULL                   = 0x0000,
		WM_CREATE                 = 0x0001,
		WM_DESTROY                = 0x0002,
		WM_MOVE                   = 0x0003,
		WM_SIZE                   = 0x0005,
		WM_ACTIVATE               = 0x0006,
		WM_SETFOCUS               = 0x0007,
		WM_KILLFOCUS              = 0x0008,
		WM_ENABLE                 = 0x000A,
		WM_SETREDRAW              = 0x000B,
		WM_SETTEXT                = 0x000C,
		WM_GETTEXT                = 0x000D,
		WM_GETTEXTLENGTH          = 0x000E,
		WM_PAINT                  = 0x000F,
		WM_CLOSE                  = 0x0010,
		WM_QUERYENDSESSION        = 0x0011,
		WM_QUIT                   = 0x0012,
		WM_QUERYOPEN              = 0x0013,
		WM_ERASEBKGND             = 0x0014,
		WM_SYSCOLORCHANGE         = 0x0015,
		WM_ENDSESSION             = 0x0016,
		WM_SHOWWINDOW             = 0x0018,
		WM_WININICHANGE           = 0x001A,
		WM_SETTINGCHANGE          = 0x001A,
		WM_DEVMODECHANGE          = 0x001B,
		WM_ACTIVATEAPP            = 0x001C,
		WM_FONTCHANGE             = 0x001D,
		WM_TIMECHANGE             = 0x001E,
		WM_CANCELMODE             = 0x001F,
		WM_SETCURSOR              = 0x0020,
		WM_MOUSEACTIVATE          = 0x0021,
		WM_CHILDACTIVATE          = 0x0022,
		WM_QUEUESYNC              = 0x0023,
		WM_GETMINMAXINFO          = 0x0024,
		WM_PAINTICON              = 0x0026,
		WM_ICONERASEBKGND         = 0x0027,
		WM_NEXTDLGCTL             = 0x0028,
		WM_SPOOLERSTATUS          = 0x002A,
		WM_DRAWITEM               = 0x002B,
		WM_MEASUREITEM            = 0x002C,
		WM_DELETEITEM             = 0x002D,
		WM_VKEYTOITEM             = 0x002E,
		WM_CHARTOITEM             = 0x002F,
		WM_SETFONT                = 0x0030,
		WM_GETFONT                = 0x0031,
		WM_SETHOTKEY              = 0x0032,
		WM_GETHOTKEY              = 0x0033,
		WM_QUERYDRAGICON          = 0x0037,
		WM_COMPAREITEM            = 0x0039,
		WM_GETOBJECT              = 0x003D,
		WM_COMPACTING             = 0x0041,
		WM_COMMNOTIFY             = 0x0044 ,
		WM_WINDOWPOSCHANGING      = 0x0046,
		WM_WINDOWPOSCHANGED       = 0x0047,
		WM_POWER                  = 0x0048,
		WM_COPYDATA               = 0x004A,
		WM_CANCELJOURNAL          = 0x004B,
		WM_NOTIFY                 = 0x004E,
		WM_INPUTLANGCHANGEREQUEST = 0x0050,
		WM_INPUTLANGCHANGE        = 0x0051,
		WM_TCARD                  = 0x0052,
		WM_HELP                   = 0x0053,
		WM_USERCHANGED            = 0x0054,
		WM_NOTIFYFORMAT           = 0x0055,
		WM_CONTEXTMENU            = 0x007B,
		WM_STYLECHANGING          = 0x007C,
		WM_STYLECHANGED           = 0x007D,
		WM_DISPLAYCHANGE          = 0x007E,
		WM_GETICON                = 0x007F,
		WM_SETICON                = 0x0080,
		WM_NCCREATE               = 0x0081,
		WM_NCDESTROY              = 0x0082,
		WM_NCCALCSIZE             = 0x0083,
		WM_NCHITTEST              = 0x0084,
		WM_NCPAINT                = 0x0085,
		WM_NCACTIVATE             = 0x0086,
		WM_GETDLGCODE             = 0x0087,
		WM_SYNCPAINT              = 0x0088,
		WM_NCMOUSEMOVE            = 0x00A0,
		WM_NCLBUTTONDOWN          = 0x00A1,
		WM_NCLBUTTONUP            = 0x00A2,
		WM_NCLBUTTONDBLCLK        = 0x00A3,
		WM_NCRBUTTONDOWN          = 0x00A4,
		WM_NCRBUTTONUP            = 0x00A5,
		WM_NCRBUTTONDBLCLK        = 0x00A6,
		WM_NCMBUTTONDOWN          = 0x00A7,
		WM_NCMBUTTONUP            = 0x00A8,
		WM_NCMBUTTONDBLCLK        = 0x00A9,
		WM_KEYDOWN                = 0x0100,
		WM_KEYUP                  = 0x0101,
		WM_CHAR                   = 0x0102,
		WM_DEADCHAR               = 0x0103,
		WM_SYSKEYDOWN             = 0x0104,
		WM_SYSKEYUP               = 0x0105,
		WM_SYSCHAR                = 0x0106,
		WM_SYSDEADCHAR            = 0x0107,
		WM_KEYLAST                = 0x0108,
		WM_IME_STARTCOMPOSITION   = 0x010D,
		WM_IME_ENDCOMPOSITION     = 0x010E,
		WM_IME_COMPOSITION        = 0x010F,
		WM_IME_KEYLAST            = 0x010F,
		WM_INITDIALOG             = 0x0110,
		WM_COMMAND                = 0x0111,
		WM_SYSCOMMAND             = 0x0112,
		WM_TIMER                  = 0x0113,
		WM_HSCROLL                = 0x0114,
		WM_VSCROLL                = 0x0115,
		WM_INITMENU               = 0x0116,
		WM_INITMENUPOPUP          = 0x0117,
		WM_MENUSELECT             = 0x011F,
		WM_MENUCHAR               = 0x0120,
		WM_ENTERIDLE              = 0x0121,
		WM_MENURBUTTONUP          = 0x0122,
		WM_MENUDRAG               = 0x0123,
		WM_MENUGETOBJECT          = 0x0124,
		WM_UNINITMENUPOPUP        = 0x0125,
		WM_MENUCOMMAND            = 0x0126,
		WM_CTLCOLORMSGBOX         = 0x0132,
		WM_CTLCOLOREDIT           = 0x0133,
		WM_CTLCOLORLISTBOX        = 0x0134,
		WM_CTLCOLORBTN            = 0x0135,
		WM_CTLCOLORDLG            = 0x0136,
		WM_CTLCOLORSCROLLBAR      = 0x0137,
		WM_CTLCOLORSTATIC         = 0x0138,
		WM_MOUSEMOVE              = 0x0200,
		WM_LBUTTONDOWN            = 0x0201,
		WM_LBUTTONUP              = 0x0202,
		WM_LBUTTONDBLCLK          = 0x0203,
		WM_RBUTTONDOWN            = 0x0204,
		WM_RBUTTONUP              = 0x0205,
		WM_RBUTTONDBLCLK          = 0x0206,
		WM_MBUTTONDOWN            = 0x0207,
		WM_MBUTTONUP              = 0x0208,
		WM_MBUTTONDBLCLK          = 0x0209,
		WM_MOUSEWHEEL             = 0x020A,
		WM_PARENTNOTIFY           = 0x0210,
		WM_ENTERMENULOOP          = 0x0211,
		WM_EXITMENULOOP           = 0x0212,
		WM_NEXTMENU               = 0x0213,
		WM_SIZING                 = 0x0214,
		WM_CAPTURECHANGED         = 0x0215,
		WM_MOVING                 = 0x0216,
		WM_DEVICECHANGE           = 0x0219,
		WM_MDICREATE              = 0x0220,
		WM_MDIDESTROY             = 0x0221,
		WM_MDIACTIVATE            = 0x0222,
		WM_MDIRESTORE             = 0x0223,
		WM_MDINEXT                = 0x0224,
		WM_MDIMAXIMIZE            = 0x0225,
		WM_MDITILE                = 0x0226,
		WM_MDICASCADE             = 0x0227,
		WM_MDIICONARRANGE         = 0x0228,
		WM_MDIGETACTIVE           = 0x0229,
		WM_MDISETMENU             = 0x0230,
		WM_ENTERSIZEMOVE          = 0x0231,
		WM_EXITSIZEMOVE           = 0x0232,
		WM_DROPFILES              = 0x0233,
		WM_MDIREFRESHMENU         = 0x0234,
		WM_IME_SETCONTEXT         = 0x0281,
		WM_IME_NOTIFY             = 0x0282,
		WM_IME_CONTROL            = 0x0283,
		WM_IME_COMPOSITIONFULL    = 0x0284,
		WM_IME_SELECT             = 0x0285,
		WM_IME_CHAR               = 0x0286,
		WM_IME_REQUEST            = 0x0288,
		WM_IME_KEYDOWN            = 0x0290,
		WM_IME_KEYUP              = 0x0291,
		WM_MOUSEHOVER             = 0x02A1,
		WM_MOUSELEAVE             = 0x02A3,
		WM_CUT                    = 0x0300,
		WM_COPY                   = 0x0301,
		WM_PASTE                  = 0x0302,
		WM_CLEAR                  = 0x0303,
		WM_UNDO                   = 0x0304,
		WM_RENDERFORMAT           = 0x0305,
		WM_RENDERALLFORMATS       = 0x0306,
		WM_DESTROYCLIPBOARD       = 0x0307,
		WM_DRAWCLIPBOARD          = 0x0308,
		WM_PAINTCLIPBOARD         = 0x0309,
		WM_VSCROLLCLIPBOARD       = 0x030A,
		WM_SIZECLIPBOARD          = 0x030B,
		WM_ASKCBFORMATNAME        = 0x030C,
		WM_CHANGECBCHAIN          = 0x030D,
		WM_HSCROLLCLIPBOARD       = 0x030E,
		WM_QUERYNEWPALETTE        = 0x030F,
		WM_PALETTEISCHANGING      = 0x0310,
		WM_PALETTECHANGED         = 0x0311,
		WM_HOTKEY                 = 0x0312,
		WM_PRINT                  = 0x0317,
		WM_PRINTCLIENT            = 0x0318,
		WM_HANDHELDFIRST          = 0x0358,
		WM_HANDHELDLAST           = 0x035F,
		WM_AFXFIRST               = 0x0360,
		WM_AFXLAST                = 0x037F,
		WM_PENWINFIRST            = 0x0380,
		WM_PENWINLAST             = 0x038F,
		WM_APP                    = 0x8000,
		WM_USER                   = 0x0400
	}

	internal enum Cursors : uint
	{
		IDC_ARROW		= 32512U,
		IDC_IBEAM       = 32513U,
		IDC_WAIT        = 32514U,
		IDC_CROSS       = 32515U,
		IDC_UPARROW     = 32516U,
		IDC_SIZE        = 32640U,
		IDC_ICON        = 32641U,
		IDC_SIZENWSE    = 32642U,
		IDC_SIZENESW    = 32643U,
		IDC_SIZEWE      = 32644U,
		IDC_SIZENS      = 32645U,
		IDC_SIZEALL     = 32646U,
		IDC_NO          = 32648U,
		IDC_HAND        = 32649U,
		IDC_APPSTARTING = 32650U,
		IDC_HELP        = 32651U
	}

	internal enum TrackerEventFlags : uint
	{
		TME_HOVER	= 0x00000001,
		TME_LEAVE	= 0x00000002,
		TME_QUERY	= 0x40000000,
		TME_CANCEL	= 0x80000000
	}

	internal enum MouseActivateFlags
	{
		MA_ACTIVATE			= 1,
		MA_ACTIVATEANDEAT   = 2,
		MA_NOACTIVATE       = 3,
		MA_NOACTIVATEANDEAT = 4
	}

	internal enum DialogCodes
	{
		DLGC_WANTARROWS			= 0x0001,
		DLGC_WANTTAB			= 0x0002,
		DLGC_WANTALLKEYS		= 0x0004,
		DLGC_WANTMESSAGE		= 0x0004,
		DLGC_HASSETSEL			= 0x0008,
		DLGC_DEFPUSHBUTTON		= 0x0010,
		DLGC_UNDEFPUSHBUTTON	= 0x0020,
		DLGC_RADIOBUTTON		= 0x0040,
		DLGC_WANTCHARS			= 0x0080,
		DLGC_STATIC				= 0x0100,
		DLGC_BUTTON				= 0x2000
	}
	internal enum UpdateLayeredWindowsFlags
	{
		ULW_COLORKEY = 0x00000001,
		ULW_ALPHA    = 0x00000002,
		ULW_OPAQUE   = 0x00000004
	}

	internal enum AlphaFlags : byte
	{
		AC_SRC_OVER  = 0x00,
		AC_SRC_ALPHA = 0x01
	}

	internal enum RasterOperations : uint
	{
		SRCCOPY		= 0x00CC0020,
		SRCPAINT	= 0x00EE0086,
		SRCAND		= 0x008800C6,
		SRCINVERT	= 0x00660046,
		SRCERASE	= 0x00440328,
		NOTSRCCOPY	= 0x00330008,
		NOTSRCERASE = 0x001100A6,
		MERGECOPY	= 0x00C000CA,
		MERGEPAINT	= 0x00BB0226,
		PATCOPY		= 0x00F00021,
		PATPAINT	= 0x00FB0A09,
		PATINVERT	= 0x005A0049,
		DSTINVERT	= 0x00550009,
		BLACKNESS	= 0x00000042,
		WHITENESS	= 0x00FF0062
	}

	internal enum BrushStyles
	{
		BS_SOLID			= 0,
		BS_NULL             = 1,
		BS_HOLLOW           = 1,
		BS_HATCHED          = 2,
		BS_PATTERN          = 3,
		BS_INDEXED          = 4,
		BS_DIBPATTERN       = 5,
		BS_DIBPATTERNPT     = 6,
		BS_PATTERN8X8       = 7,
		BS_DIBPATTERN8X8    = 8,
		BS_MONOPATTERN      = 9
	}

	internal enum HatchStyles
	{
		HS_HORIZONTAL       = 0,
		HS_VERTICAL         = 1,
		HS_FDIAGONAL        = 2,
		HS_BDIAGONAL        = 3,
		HS_CROSS            = 4,
		HS_DIAGCROSS        = 5
	}

	internal enum CombineFlags
	{
		RGN_AND		= 1,
		RGN_OR		= 2,
		RGN_XOR		= 3,
		RGN_DIFF	= 4,
		RGN_COPY	= 5
	}

	internal enum HitTest
	{
		HTERROR			= -2,
		HTTRANSPARENT   = -1,
		HTNOWHERE		= 0,
		HTCLIENT		= 1,
		HTCAPTION		= 2,
		HTSYSMENU		= 3,
		HTGROWBOX		= 4,
		HTSIZE			= 4,
		HTMENU			= 5,
		HTHSCROLL		= 6,
		HTVSCROLL		= 7,
		HTMINBUTTON		= 8,
		HTMAXBUTTON		= 9,
		HTLEFT			= 10,
		HTRIGHT			= 11,
		HTTOP			= 12,
		HTTOPLEFT		= 13,
		HTTOPRIGHT		= 14,
		HTBOTTOM		= 15,
		HTBOTTOMLEFT	= 16,
		HTBOTTOMRIGHT	= 17,
		HTBORDER		= 18,
		HTREDUCE		= 8,
		HTZOOM			= 9 ,
		HTSIZEFIRST		= 10,
		HTSIZELAST		= 17,
		HTOBJECT		= 19,
		HTCLOSE			= 20,
		HTHELP			= 21
	}

	internal enum SystemParametersInfoActions : uint
	{
		GetBeep = 1,
		SetBeep = 2,
		GetMouse = 3,
		SetMouse = 4,
		GetBorder = 5,
		SetBorder = 6,
		GetKeyboardSpeed = 10,
		SetKeyboardSpeed = 11,
		LangDriver = 12,
		IconHorizontalSpacing = 13,
		GetScreenSaveTimeout = 14,
		SetScreenSaveTimeout = 15,
		GetScreenSaveActive = 16,
		SetScreenSaveActive = 17,
		GetGridGranularity = 18,
		SetGridGranularity = 19,
		SetDeskWallPaper = 20,
		SetDeskPattern = 21,
		GetKeyboardDelay = 22,
		SetKeyboardDelay = 23,
		IconVerticalSpacing = 24,
		GetIconTitleWrap = 25,
		SetIconTitleWrap = 26,
		GetMenuDropAlignment = 27,
		SetMenuDropAlignment = 28,
		SetDoubleClkWidth = 29,
		SetDoubleClkHeight = 30,
		GetIconTitleLogFont = 31,
		SetDoubleClickTime = 32,
		SetMouseButtonSwap = 33,
		SetIconTitleLogFont = 34,
		GetFastTaskSwitch = 35,
		SetFastTaskSwitch = 36,
		SetDragFullWindows = 37,
		GetDragFullWindows = 38,
		GetNonClientMetrics = 41,
		SetNonClientMetrics = 42,
		GetMinimizedMetrics = 43,
		SetMinimizedMetrics = 44,
		GetIconMetrics = 45,
		SetIconMetrics = 46,
		SetWorkArea = 47,
		GetWorkArea = 48,
		SetPenWindows = 49,
		GetFilterKeys = 50,
		SetFilterKeys = 51,
		GetToggleKeys = 52,
		SetToggleKeys = 53,
		GetMouseKeys = 54,
		SetMouseKeys = 55,
		GetShowSounds = 56,
		SetShowSounds = 57,
		GetStickyKeys = 58,
		SetStickyKeys = 59,
		GetAccessTimeout = 60,
		SetAccessTimeout = 61,
		GetSerialKeys = 62,
		SetSerialKeys = 63,
		GetSoundsEntry = 64,
		SetSoundsEntry = 65,
		GetHighContrast = 66,
		SetHighContrast = 67,
		GetKeyboardPref = 68,
		SetKeyboardPref = 69,
		GetScreenReader = 70,
		SetScreenReader = 71,
		GetAnimation = 72,
		SetAnimation = 73,
		GetFontSmoothing = 74,
		SetFontSmoothing = 75,
		SetDragWidth = 76,
		SetDragHeight = 77,
		SetHandHeld = 78,
		GetLowPowerTimeout = 79,
		GetPowerOffTimeout = 80,
		SetLowPowerTimeout = 81,
		SetPowerOffTimeout = 82,
		GetLowPowerActive = 83,
		GetPowerOffActive = 84,
		SetLowPowerActive = 85,
		SetPowerOffActive = 86,
		SetCursors = 87,
		SetIcons = 88,
		GetDefaultInputLang = 89,
		SetDefaultInputLang = 90,
		SetLangToggle = 91,
		GetWindwosExtension = 92,
		SetMouseTrails = 93,
		GetMouseTrails = 94,
		ScreenSaverRunning = 97,
		GetMouseHoverTime = 0x0066
	}

	[Flags]
	internal enum FlagsAnimateWindow : uint
	{
		AW_HOR_POSITIVE = 0x00000001,
		AW_HOR_NEGATIVE = 0x00000002,
		AW_VER_POSITIVE = 0x00000004,
		AW_VER_NEGATIVE = 0x00000008,
		AW_CENTER = 0x00000010,
		AW_HIDE = 0x00010000,
		AW_ACTIVATE = 0x00020000,
		AW_SLIDE = 0x00040000,
		AW_BLEND =0x00080000
	}

	[Flags]
	internal enum FlagsDCX : uint
	{
		DCX_WINDOW = 0x1,
		DCX_CACHE = 0x2,
		DCX_NORESETATTRS = 0x4,
		DCX_CLIPCHILDREN = 0x8,
		DCX_CLIPSIBLINGS = 0x10,
		DCX_PARENTCLIP = 0x20,
		DCX_EXCLUDERGN = 0x40,
		DCX_INTERSECTRGN = 0x80,
		DCX_EXCLUDEUPDATE = 0x100,
		DCX_INTERSECTUPDATE = 0x200,
		DCX_LOCKWINDOWUPDATE = 0x400,
		DCX_NORECOMPUTE = 0x100000,
		DCX_VALIDATE = 0x200000
	}
	
	[StructLayout(LayoutKind.Sequential)]
	internal struct MSG 
	{
		public IntPtr hwnd;
		public int message;
		public IntPtr wParam;
		public IntPtr lParam;
		public int time;
		public int pt_x;
		public int pt_y;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct PAINTSTRUCT
	{
		public IntPtr hdc;
		public int fErase;
		public Rectangle rcPaint;
		public int fRestore;
		public int fIncUpdate;
		public int Reserved1;
		public int Reserved2;
		public int Reserved3;
		public int Reserved4;
		public int Reserved5;
		public int Reserved6;
		public int Reserved7;
		public int Reserved8;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct RECT
	{
		public int left;
		public int top;
		public int right;
		public int bottom;

		public override string ToString()
		{
			return "{left=" + left.ToString() + ", " + "top=" + top.ToString() + ", " +
				"right=" + right.ToString() + ", " + "bottom=" + bottom.ToString() + "}";
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct POINT
	{
		public int x;
		public int y;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct SIZE
	{
		public int cx;
		public int cy;
	}

	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct BLENDFUNCTION
	{
		public byte BlendOp;
		public byte BlendFlags;
		public byte SourceConstantAlpha;
		public byte AlphaFormat;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct TRACKMOUSEEVENTS
	{
		public const uint TME_HOVER = 0x00000001;
		public const uint TME_LEAVE = 0x00000002;
		public const uint TME_NONCLIENT = 0x00000010;
		public const uint TME_QUERY = 0x40000000;
		public const uint TME_CANCEL = 0x80000000;
		public const uint HOVER_DEFAULT = 0xFFFFFFFF;

		private uint cbSize;
		private uint dwFlags;
		private IntPtr hWnd;
		private uint dwHoverTime;

		public TRACKMOUSEEVENTS(uint dwFlags, IntPtr hWnd, uint dwHoverTime)
		{
			cbSize = 16;
			this.dwFlags = dwFlags;
			this.hWnd = hWnd;
			this.dwHoverTime = dwHoverTime;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct LOGBRUSH
	{
		public uint lbStyle; 
		public uint lbColor; 
		public uint lbHatch; 
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct NCCALCSIZE_PARAMS
	{
		public RECT rgrc1;
		public RECT rgrc2;
		public RECT rgrc3;
		IntPtr lppos;
	}
}