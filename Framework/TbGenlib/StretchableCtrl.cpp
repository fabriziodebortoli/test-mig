
#include "stdafx.h"

#include "StretchableCtrl.h"


#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

//-----------------------------------------------------------------------------
//			class CStretchCtrl
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CStretchCtrl, CWnd)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP	(CStretchCtrl, CWnd)
//{{AFX_
	ON_WM_PAINT			()
    ON_WM_SETCURSOR		()
	ON_WM_LBUTTONDOWN	()
	ON_WM_LBUTTONUP		()
	ON_WM_MOUSEMOVE		()
//}}AFX_
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CStretchCtrl::CStretchCtrl()
{
	COLORREF transCol = AfxGetThemeManager()->GetStretchCtrlTransparentColor();
	RegisterWindowClass();
	LoadBitmapOrPng(&m_Grip, TBGlyph(szIconGrip));

	LoadBitmapOrPng(&m_Pushpin, TBGlyph(szIconPushpin));	

	LoadBitmapOrPng(&m_PushpinLocked, TBGlyph(szIconPushpinLock));	

	LoadBitmapOrPng(&m_PushpinMouseOver, TBGlyph(szIconPushpinMouseOver));	

	m_bStretching		= FALSE;
	m_pLinkedWnd		= NULL;
	
	m_bPushpinLocked	= FALSE;
	m_bMouseOver		= FALSE;
}

//-----------------------------------------------------------------------------
BOOL CStretchCtrl::RegisterWindowClass()
{
    WNDCLASS wndcls;
    HINSTANCE hInst = AfxGetInstanceHandle();

    if (!(::GetClassInfo(hInst, STRETCHCTRL_CLASSNAME, &wndcls)))
    {

        // otherwise we need to register a new class
        wndcls.style            = CS_DBLCLKS | CS_HREDRAW | CS_VREDRAW;
        wndcls.lpfnWndProc      = ::DefWindowProc;
        wndcls.cbClsExtra       = wndcls.cbWndExtra = 0;
        wndcls.hInstance        = hInst;
        wndcls.hIcon            = NULL;
        wndcls.hCursor          = AfxGetApp()->LoadStandardCursor(IDC_ARROW);
        wndcls.hbrBackground    = (HBRUSH) (COLOR_BTNFACE + 1);
        wndcls.lpszMenuName     = NULL;
        wndcls.lpszClassName    = STRETCHCTRL_CLASSNAME;

        if (!AfxRegisterClass(&wndcls))
        {
            AfxThrowResourceException();
            return FALSE;
        }
    }

    return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CStretchCtrl::Create(DWORD dwStyle, const RECT& rect, CWnd* pWnd, UINT nID)
{
	return CWnd::Create(STRETCHCTRL_CLASSNAME, NULL, dwStyle, rect, pWnd, nID);
}

//-----------------------------------------------------------------------------
void	CStretchCtrl::OnPaint()
{
	CPaintDC paint_dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente

	CRect	rect;
	GetClientRect(rect);
	m_GripRect.top			= rect.bottom - m_Grip.Height() - 1;
	m_GripRect.bottom		= rect.bottom;
	m_GripRect.left			= rect.right - m_Grip.Width();
	m_GripRect.right		= rect.right;

	m_PushpinRect.top		= rect.bottom - m_Pushpin.Height() - 1;
	m_PushpinRect.bottom	= rect.bottom - 1;
	m_PushpinRect.left		= rect.left + 2;
	m_PushpinRect.right		= rect.left + m_Pushpin.Width() + 2;
	
	
	CPen	pen (PS_SOLID, 1, AfxGetThemeManager()->GetEnabledControlForeColor());
	CPen*	pOldPen	= paint_dc.SelectObject(&pen);

	paint_dc.MoveTo(rect.left, rect.top);
	paint_dc.LineTo(rect.left, rect.bottom-1);
	paint_dc.LineTo(rect.right, rect.bottom-1);

	paint_dc.SelectObject(pOldPen);
	pen.DeleteObject();

	m_Grip.DrawTransparent(&paint_dc, m_GripRect.left, m_GripRect.top);
	
	if (m_bPushpinLocked)
		m_PushpinLocked.DrawTransparent(&paint_dc, m_PushpinRect.left, m_PushpinRect.top);
	else
	{
		if (m_bMouseOver)
			m_PushpinMouseOver.DrawTransparent(&paint_dc, m_PushpinRect.left, m_PushpinRect.top);
		else
			m_Pushpin.DrawTransparent(&paint_dc, m_PushpinRect.left, m_PushpinRect.top);
	}
}

//-----------------------------------------------------------------------------
BOOL	CStretchCtrl::OnSetCursor			( CWnd* pWnd, UINT nHitTest, UINT message )
{
	CPoint			mousepos;
	GetCursorPos	(&mousepos);
	ScreenToClient	(&mousepos);
	
	if (m_GripRect.PtInRect(mousepos))
	{
		SetCursor(AfxGetApp()->LoadStandardCursor(IDC_SIZENWSE));
		return TRUE;
	}

	return CWnd::OnSetCursor(pWnd, nHitTest, message);
}

//-----------------------------------------------------------------------------
void	CStretchCtrl::OnLButtonDown		(UINT nFlags, CPoint point)
{
	if (m_GripRect.PtInRect(point))
	{
		SetCapture();

		m_bStretching	= TRUE;

		GetCursorPos(&m_ptStartPos);
	}

	if (m_PushpinRect.PtInRect(point))
	{
		m_bPushpinLocked = !m_bPushpinLocked;

		Invalidate();
		UpdateWindow();
	}
}

//-----------------------------------------------------------------------------
void	CStretchCtrl::OnLButtonUp			(UINT nFlags, CPoint point)
{
	if (m_bStretching)
	{
		ReleaseCapture();

		m_bStretching = FALSE;
	}
}

//-----------------------------------------------------------------------------
void	CStretchCtrl::OnMouseMove			(UINT nFlags, CPoint point)
{
	if (m_bStretching && GetLinkedWnd())
	{
		GetCursorPos(&point);			// <----- NOTA: coordinate dello schermo
		static	BOOL	bLock = FALSE;

		if (!bLock)
		{
			bLock = TRUE;
			
			m_bPushpinLocked = FALSE;

			CRect	rect;
			GetLinkedWnd()->GetWindowRect(rect);

			CSize	delta = point - m_ptStartPos;

			rect.right	+= delta.cx; 
			rect.bottom	+= delta.cy;

			rect.right = rect.right < rect.left + 50 ? rect.left + 50 : rect.right;
			rect.bottom = rect.bottom < rect.top + 25 ? rect.top + 25 : rect.bottom;
/*
			TRACE2("DELTA %d-%d\n", delta.cx, delta.cy);
			TRACE2("POINT %d-%d\n", point.x, point.y);
			TRACE2("PTSTA %d-%d\n", m_ptStartPos.x, m_ptStartPos.y);
*/			
			m_ptStartPos	= point;

			GetParent()->ScreenToClient(rect);


			GetLinkedWnd()->SetWindowPos
			(
				NULL,
				rect.left,
				rect.top,
				rect.Width(),
				rect.Height(),
				SWP_NOZORDER | SWP_NOMOVE
			);

			Invalidate();
			UpdateWindow();
			
			GetLinkedWnd()->Invalidate();
			GetLinkedWnd()->UpdateWindow();

			bLock = FALSE;
		}
	}

	if (m_PushpinRect.PtInRect(point))
	{
/*		CPaintDC	paint_dc(this);
		m_PushpinLocked.DrawTransparent(&paint_dc, m_PushpinRect.left, m_PushpinRect.top);
*/
		m_bMouseOver = TRUE;

		Invalidate();
		UpdateWindow();
	}
	else
	{
		m_bMouseOver = FALSE;

		Invalidate();
		UpdateWindow();
	}
}

//-----------------------------------------------------------------------------
//			class CStretchableStrEdit
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CStretchableStrEdit, CStrEdit)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CStretchableStrEdit, CStrEdit)
	//{{AFX_MSG_MAP(CStretchableStrEdit)
	ON_WM_WINDOWPOSCHANGING	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CStretchableStrEdit::CStretchableStrEdit()
					 :m_pStretch(NULL)
{
}

//-----------------------------------------------------------------------------
CStretchableStrEdit::~CStretchableStrEdit()
{
	SAFE_DELETE (m_pStretch);
}

//-----------------------------------------------------------------------------
void CStretchableStrEdit::OnWindowPosChanging(WINDOWPOS FAR* wndPos)
{
	CRect rectEdit	(CPoint(wndPos->x, wndPos->y), CSize(wndPos->cx, wndPos->cy));
	//ASSERT(rectEdit.left);

	CRect rectStretch(0,0,0,0);

	if (rectEdit == rectStretch)
		return;

	CRect wndRect;
	GetWindowRect(wndRect);
	GetCtrlParent()->ScreenToClient(wndRect);

	//normalizzo le dimensioni x i casi NO_SIZE/NO_MOVE
	if (wndPos->cx == 0)
	{
		if (rectEdit.left == 0)
			rectEdit.left = wndRect.left;
		rectEdit.right = rectEdit.left + wndRect.Width();
	}
	else if (rectEdit.left == 0 && wndRect.left > 0)
	{
		rectEdit.left = wndRect.left;
		rectEdit.right = rectEdit.left + wndPos->cx;
	}

	if (wndPos->cy == 0)
	{
		if (rectEdit.top == 0)
			rectEdit.top = wndRect.top;
		rectEdit.bottom = rectEdit.top + wndRect.Height();
	}
	else if (rectEdit.top == 0 && wndRect.top > 0)
	{
		rectEdit.top = wndRect.top;
		rectEdit.bottom = rectEdit.top + +wndPos->cy;
	}

	//----

	if (m_pStretch)
	{
		if (m_pStretch->IsPushpinLocked())
		{
			//mantengo dimensioni più grandi se pinnate
			CRect r;
			GetClientRect(r); //left-top == 0: right == width, bottom == height

			if (r.right > rectEdit.Width())
			{
				rectEdit.right = rectEdit.left + r.right;
			}
			if (r.bottom > rectEdit.Height())
			{
				rectEdit.bottom = rectEdit.top + r.bottom;
			}
		}

		rectStretch.left	= rectEdit.left;
		rectStretch.right	= rectEdit.right;
		rectStretch.top		= rectEdit.bottom;
		rectStretch.bottom	= rectStretch.top + IXW_FSSTRETCHABLE_STRETCHBAR_HEIGHT;

		m_pStretch->SetWindowPos
		(
			this, 
			rectStretch.left, 
			rectStretch.top, 
			rectStretch.Width(), 
			rectStretch.Height(),
			((wndPos->flags & SWP_SHOWWINDOW) == SWP_SHOWWINDOW) ? SWP_SHOWWINDOW : 0
		);
	}

	//----

	wndPos->x			= rectEdit.left;
	wndPos->y			= rectEdit.top;
	wndPos->cx			= rectEdit.Width();
	wndPos->cy			= rectEdit.Height();

}

//-----------------------------------------------------------------------------
BOOL CStretchableStrEdit::CreateAssociatedStretch(CWnd* pParentWnd)
{	    
	if (!::IsWindow(m_hWnd))
		return FALSE;

    CRect rectE(0, 0, 0, 0);
    DWORD dwGenericStyle = (GetCtrlCWnd()->GetStyle() & (WS_VISIBLE | WS_DISABLED)) | WS_CHILD;
    
	m_pStretch = new CStretchCtrl();
	m_pStretch->AttachLinkedWnd(this);

	GetWindowRect(rectE);
	pParentWnd->ScreenToClient(rectE);

	if (!m_pStretch->Create(dwGenericStyle, CRect(CPoint(rectE.left, rectE.bottom), CSize(rectE.Width(), IXW_FSSTRETCHABLE_STRETCHBAR_HEIGHT)), pParentWnd, DUMMY_ID))
		return FALSE;

	return SetWindowPos(NULL, rectE.left, rectE.top, rectE.Width(), rectE.Height(), SWP_NOZORDER);
}

//-----------------------------------------------------------------------------
BOOL CStretchableStrEdit::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	return 
		__super::Create(dwStyle, rect, pParentWnd, nID) &&
		CreateAssociatedStretch(pParentWnd);
}

//-----------------------------------------------------------------------------
BOOL CStretchableStrEdit::SubclassEdit(UINT nID, CWnd* pParentWnd, const CString& strName)
{
	return
		__super::SubclassEdit(nID, pParentWnd, strName) &&
		CreateAssociatedStretch(pParentWnd);
}

//-----------------------------------------------------------------------------
BOOL	CStretchableStrEdit::ShowCtrl (int nCmdShow)
{
	if (__super::ShowCtrl(nCmdShow))
	{
		if (m_pStretch)
			m_pStretch->ShowWindow(nCmdShow);

		return TRUE;
	}

	return FALSE;
}
