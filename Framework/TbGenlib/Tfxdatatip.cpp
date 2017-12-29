#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\TBThemeManager.h>

#include "tfxdatatip.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

// border area around the text
static const short _border = 3;

// define the active border for data tips
static const short _horizontal  = 2;
static const short _vertical    = 2;

// class data members
BOOL  TFXDataTip::m_s_registered = FALSE;
short TFXDataTip::m_s_delay = 300;
short TFXDataTip::m_s_count = 0;

short TFXDataTip::m_s_maxWidth = 800;
short TFXDataTip::m_s_maxHeight = 600;

// GDI objects used for drawing operations in module
static CFont*   _font = NULL;
static CBrush* _brush = NULL;

// hook information
HHOOK TFXDataTip::m_s_hookProc = NULL;
TFXDataTip* TFXDataTip::m_s_pCurrent = NULL;

// special destructor object for clearing up GDI objects
struct _brush_destructor
{
    ~_brush_destructor( )
    {
		if (_brush)
		{
			if (_brush->Detach())
				_brush->DeleteObject();

			delete _brush;
			_brush = NULL;
		}
    }
};
static const _brush_destructor _destroyer;

static BOOL IsPointNearOtherPoint(CPoint &point, CPoint &test, CSize &distance)
{
	if( point == test )
		return TRUE;
	
	CRect	rectOk(
				point.x - distance.cx,
				point.y - distance.cy,
				point.x + distance.cx,
				point.y + distance.cy);

	return rectOk.PtInRect(test);
}
//=============================================================================
BEGIN_MESSAGE_MAP(TFXDataTip, CWnd)
	//{{AFX_MSG_MAP(TFXDataTip)
	ON_WM_PAINT()
	ON_WM_MOUSEMOVE()
	ON_WM_TIMER()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------
void TFXDataTip::RegisterWnd( )
{
	// check for prior registration
	if (m_s_registered) return;

	// initialise the basic information before registration
	HINSTANCE hInst = AfxGetInstanceHandle( );

	// initialise the window class information
	WNDCLASS wndcls;
	wndcls.style         = CS_SAVEBITS | CS_DBLCLKS;
	wndcls.lpfnWndProc   = ::DefWindowProc;
	wndcls.cbClsExtra    = 0;
	wndcls.cbWndExtra    = 0;
	wndcls.hInstance     = hInst;
	wndcls.hIcon         = NULL;
	wndcls.hCursor       = AfxGetApp()->LoadStandardCursor(IDC_ARROW);
	wndcls.hbrBackground = *_brush;
	wndcls.lpszMenuName  = NULL;
	wndcls.lpszClassName = _T("TFXDataTip");

	// register the window class
	if (!AfxRegisterClass(&wndcls))
		AfxThrowResourceException();

	m_s_registered = TRUE;
}

//-----------------------------------------------------------------------------
void TFXDataTip::Initialise( )
{
   if (m_s_registered) return;
       
   if (_brush == NULL)
		_brush = new CBrush(AfxGetThemeManager()->GetTooltipBkgColor());

	_font = AfxGetThemeManager()->GetFormFont();

    // register the window class
    RegisterWnd( );

    // install the keyboard hook procedure
    if (m_s_hookProc == NULL)
    {
        m_s_hookProc = ::SetWindowsHookEx(WH_KEYBOARD, 
                                       (HOOKPROC)KeyboardHookCallback,
                                       NULL,
                                       ::GetCurrentThreadId( ));
    }
}
//-----------------------------------------------------------------------------

/*
* DESCRIPTION:      This method provides the keyboard hook callback procedure.
*                   If there is currently an active DataTip then when a key 
*                   is pressed it will hide the tip, before passing on the
*                   message to the next procedure in the hook chain.
*
*/
LRESULT CALLBACK TFXDataTip::KeyboardHookCallback(int code, WPARAM wParam, LPARAM lParam)
{
	// if keypress has occured then hide any visible data tips
	if (code >= 0)
	{
		if (m_s_pCurrent != NULL)
		{
			m_s_pCurrent->Hide( );
		}
	}

	// chain to the next hook procedure
	return ::CallNextHookEx(m_s_hookProc, code, wParam, lParam);
}

//-----------------------------------------------------------------------------
TFXDataTip::TFXDataTip()
{
 	m_ready				= FALSE;
	m_timer				= 0;
	m_timerToHide		= 0;
	m_on				= FALSE;
	m_TipPosition		= CPoint(0, 0);
	m_MousePosition		= CPoint(0, 0);
	m_offset			= CPoint(0, 22);
	m_TipSurrounding	= CSize(1, 1);
	m_bVisible			= FALSE;
	m_TickCount			= 0;

	TB_LOCK_FOR_WRITE()

	m_s_count++;

   Initialise( );
}

//-----------------------------------------------------------------------------
TFXDataTip::~TFXDataTip()
{
	On(FALSE);

 	TB_LOCK_FOR_WRITE()
		
	m_s_count--;
	if ( (m_s_count == 0) && (m_s_hookProc != NULL) )
	{
		::UnhookWindowsHookEx(m_s_hookProc);
		m_s_hookProc = NULL;
	}
	DestroyWindow();
}

//-----------------------------------------------------------------------------
BOOL TFXDataTip::Create(CWnd* pParentWnd) 
{
    m_parent = pParentWnd;
	m_ready = CWnd::CreateEx(WS_EX_LEFT | WS_EX_TOOLWINDOW,
                             _T("TFXDataTip"), 
                             NULL,
                             WS_POPUP | WS_BORDER | WS_CLIPSIBLINGS, 
                             0, 0, 10, 10,
                             pParentWnd->m_hWnd, 
                             NULL);

    return m_ready;
}

//-----------------------------------------------------------------------------
void TFXDataTip::On(BOOL on)
{ 
	m_on = on;	
	if (!on)
	{
		Hide();
	}
 }

//-----------------------------------------------------------------------------
BOOL TFXDataTip::Hide( )
{
	if (!m_ready) 
		return FALSE;
	if (!IsVisible())
		return TRUE;

	m_bVisible = FALSE;
	BOOL isVisible = ShowWindow(SW_HIDE);

	if( m_timer ) 			KillTimer(m_timer);
	if( m_timerToHide ) 	KillTimer(m_timerToHide);
	m_timer = m_timerToHide = 0;

	::ReleaseCapture( );

	// reset the current tip window
	m_s_pCurrent = NULL;

	// success of hiding
	return isVisible;
}

//-----------------------------------------------------------------------------
void TFXDataTip::ResetText()
{
	m_TipPosition = CPoint(0, 0);
	m_TipText.Empty();
}

//-----------------------------------------------------------------------------
void TFXDataTip::AddText(LPCTSTR szText)
{
	if (!m_TipText.IsEmpty())
		m_TipText += _T("\r\n");
	
	m_TipText += szText;
}

//-----------------------------------------------------------------------------
void TFXDataTip::SetNewTip(CPoint point)
{
	// check whether tips are turned on
	if (!m_on) return;

	// determine whether the point is in the parent window
	CRect rect;
	m_parent->GetClientRect(&rect);
	if( rect.PtInRect(point) )
	{
		// change data tip if position has changed
		m_parent->ClientToScreen(&point);
		CPoint	pointCursor(0,0);
		::GetCursorPos(&pointCursor);

		m_TipPosition	= point;
		m_MousePosition	= pointCursor;
		
		// re-start the timer
		KillTimer(m_timer);
		m_timer = SetTimer(1010, m_s_delay, NULL);
	}
}

//-----------------------------------------------------------------------------
void TFXDataTip::Set(CPoint point, CString szText) 
{ 
	ResetText(); 

	if (szText.GetLength() == 0)
	{
		Hide();
		return;
	}

	m_TipText = szText;

	SetNewTip(point); 
}

//-----------------------------------------------------------------------------
CSize TFXDataTip::GetSize() 
{
	CSize sizeAll(0, 0);
    if (m_TipText.IsEmpty())
		return sizeAll;

	CClientDC dc(this);
	HGDIOBJ pOldFont = dc.SelectObject(&_font);

	CRect rect;
	dc.DrawText(m_TipText, rect, DT_LEFT | DT_TOP | DT_NOPREFIX | DT_EXPANDTABS | DT_CALCRECT);
	sizeAll.cx = rect.Width();
	sizeAll.cy = rect.Height();

	dc.SelectObject(pOldFont);
	return sizeAll;
}

//-----------------------------------------------------------------------------
void TFXDataTip::Display( )
{
	if (!m_ready) return;

    // hide any other currently visible data tips
	if (m_s_pCurrent != NULL)
	{
		m_s_pCurrent->Hide( );
	}

    if (m_TipText.IsEmpty())
		return;

	CSize sizeAll = GetSize();
	
	CRect r1, r2;
	if(this->m_parent)
	{
		// this is not enough, need to pass the parent of the tooltip
		// in order to get the proper monitor on which the tooltip
		// has to be shown.
		::GetMonitorRect(this->m_parent, r1, r2);
	}
	else
	{
		// better than nothing, the risk is to have the tooltip on the 
		// primary monitor instead of the one on which the mouse hoovered.
		::GetMonitorRect(this, r1, r2);
	}
	int x = r1.right - m_TipPosition.x;
	if (sizeAll.cx > x)
	{
		m_TipPosition.x -= (sizeAll.cx - x);
	}
	int y = r1.bottom - m_TipPosition.y;
	if (sizeAll.cy > (y-40))	//h Taskbar
	{
		m_TipPosition.y -= (sizeAll.cy + 30);	//h line
	}

	// determine the window size and position
	// displayed above and centered on the origin
	CRect wndRect(
			m_TipPosition.x + m_offset.x,
			m_TipPosition.y + m_offset.y,
			m_TipPosition.x + sizeAll.cx + m_offset.x,
			m_TipPosition.y + sizeAll.cy + m_offset.y);

    
	// adjust window for offset and borders
	wndRect.InflateRect(2 * _border, _border);

	//if (wndRect.right > dc.GetDeviceCaps(HORZRES)) 
	//	wndRect.OffsetRect(-wndRect.Width(), 0);
	//else 
	//	wndRect.OffsetRect(m_offset);

    // capture the mouse in order to be able to
	// determine when to hide the window
	SetCapture ();
	
	// update window position and display
	SetWindowPos(&wndTop,
			 wndRect.left,
			 wndRect.top,
			 wndRect.Width( ),
			 wndRect.Height( ),
			 SWP_SHOWWINDOW | SWP_NOACTIVATE);

	// set the current data tip window
	m_bVisible = TRUE;
	m_s_pCurrent = this;
}

//-----------------------------------------------------------------------------
void TFXDataTip::OnPaint()
{
	CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente

	CRect rect;
	GetClientRect(&rect);

	// initialise the device context for drawing
	CFont* pOldFont = dc.SelectObject(_font);
	dc.SetBkColor(AfxGetThemeManager()->GetTooltipBkgColor());
	dc.SetTextColor(AfxGetThemeManager()->GetTooltipForeColor());

	// draw the data tip
	dc.DrawText(m_TipText, rect, DT_LEFT | DT_TOP | DT_NOPREFIX | DT_END_ELLIPSIS | DT_EXPANDTABS | (DT_TABSTOP | 0x800));

	// restore the device context
	dc.SelectObject(pOldFont);
}

//-----------------------------------------------------------------------------
BOOL TFXDataTip::DestroyWindow() 
{
	if( m_timer ) 			KillTimer(m_timer);
	if( m_timerToHide ) 	KillTimer(m_timerToHide);
	m_timer = m_timerToHide = 0;

	return CWnd::DestroyWindow();
}

//-----------------------------------------------------------------------------
BOOL TFXDataTip::PreTranslateMessage(MSG* pMsg) 
{
	MSG		msgNew;

	memcpy(&msgNew, pMsg, sizeof(MSG));
	
	switch (pMsg->message)
    {
		case WM_LBUTTONDOWN:
			{
				DWORD dwTick = ::GetTickCount();
				TRACE(_T("dataTip Tick: %d %d %d %d \n"), dwTick , m_TickCount, abs((int)dwTick - (int)m_TickCount), GetDoubleClickTime());
				if ( (UINT)(abs((int)dwTick - (int)m_TickCount)) <= 20*GetDoubleClickTime())
				{
					msgNew.message = WM_LBUTTONDBLCLK;
				}
				m_TickCount = dwTick;
			}

		case WM_LBUTTONUP:
		case WM_LBUTTONDBLCLK:

		case WM_NCLBUTTONDOWN:
		case WM_NCLBUTTONUP:
		case WM_NCLBUTTONDBLCLK:

		case WM_NCRBUTTONDOWN:
		case WM_NCRBUTTONUP:
		case WM_NCRBUTTONDBLCLK:

		case WM_NCMBUTTONDOWN:
		case WM_NCMBUTTONUP:
		case WM_NCMBUTTONDBLCLK:


		case WM_RBUTTONDOWN:
		case WM_RBUTTONUP:
		case WM_RBUTTONDBLCLK:

		case WM_MBUTTONDOWN:
		case WM_MBUTTONUP:
		case WM_MBUTTONDBLCLK:

        case WM_CANCELMODE:
				//TRACE("\n %04x", msgNew.message);
				if (m_parent)
				{
					POINT lp;
					lp.x = (LONG)(short)LOWORD(msgNew.lParam);
					lp.y = (LONG)(short)HIWORD(msgNew.lParam);
					ClientToScreen(&lp);
					m_parent->ScreenToClient(&lp);
					msgNew.lParam = MAKELONG(lp.x, lp.y);
					m_parent->PostMessage(msgNew.message, msgNew.wParam, msgNew.lParam);
				}
				Hide();
				//if( m_timerToHide == 0 )
				//	m_timerToHide = SetTimer(1011, GetDoubleClickTime()+100, NULL);
				break;
   }

	return CWnd::PreTranslateMessage(pMsg);
}

//-----------------------------------------------------------------------------
void TFXDataTip::OnMouseMove(UINT nFlags, CPoint point) 
{
	CWnd::OnMouseMove(nFlags, point);

	// determine if a mouse button has been pressed or
	// we have moved outside of the active capture area
	ClientToScreen(&point);
	
	if( !IsPointNearOtherPoint(m_MousePosition, point, m_TipSurrounding) )
	{
		Hide( );
	}
}

//-----------------------------------------------------------------------------
void TFXDataTip::OnTimer(UINT nIDEvent) 
{
	if (!::IsWindow(m_hWnd))
		return;

    if( nIDEvent == m_timerToHide) 
	{
		KillTimer(m_timerToHide);
		m_timerToHide = 0;
		Hide();
	}
	else if( nIDEvent == m_timer)
	{
		KillTimer(m_timer);
		m_timer = 0;

		// make sure that cursor hasn't moved before displaying the data tip
		CPoint point;
		if (::GetCursorPos(&point))
		{
			if( IsPointNearOtherPoint(m_MousePosition, point, m_TipSurrounding) )
			{
				Display();
			}
		}
	}
	else
		CWnd::OnTimer(nIDEvent);
}

