#include "stdafx.h"

#include <Math.h>
#include <TbGenlib\Tfxdatatip.h>
#include "MultiChartCtl.h"
#include "resource.h"


#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//=====================================================================================

//---------------------------------------------------------------------------
BOOL CMultiChartCtrl::PreCreateWindow(CREATESTRUCT& cs)
{
	cs.style |= BS_OWNERDRAW | WS_BORDER | WS_HSCROLL | WS_VSCROLL ;	
	return CButton::PreCreateWindow(cs);
}
//----------------------------------------------------------------------------
int	 CMultiChartCtrl::OnCreate (LPCREATESTRUCT lpCreateStruct)
{
	return 0;	//altrimenti abortisce la creazione
}	
//----------------------------------------------------------------------------
void CMultiChartCtrl::PrepareScrollInfo (int cx, int cy)
{
	int nOldHeight = m_nHeight;
	int nWidthVScroll = 0;
	int nOldWidth = m_nWidth;
	int nHeightHScroll = 0;

	m_nHeight = m_nRows * m_nDCHeightAllRows + m_nDCHeightColHeader;
	m_nWidth = m_nCols * m_nDCWidthCols + m_nDCWidthRowHeader ;

	nWidthVScroll = GetSystemMetrics(SM_CXVSCROLL);
	nHeightHScroll = GetSystemMetrics(SM_CYHSCROLL);

    //@todoporting RICCARDO
	//if ( m_nHeight > cy && nOldHeight <= cy ) 
/*	if( m_nHeight > cy)
	{
		m_nHeight += nWidthVScroll;
	}*/
//	if ( m_nWidth > cx && nOldWidth <= cx ) 
/*	if ( m_nWidth > cx ) 
	{
		m_nWidth += nHeightHScroll;
	}
*/

	//m_nWidth += nWidthVScroll;

	m_nScrollStepX = m_nDCWidthCols;
    m_nScrollStepY = m_nDCHeightAllRows;

	m_nScrollPosX = 0;
	m_nScrollPosY = 0;

	m_bVScroll = FALSE;
	m_bHScroll = FALSE;

	// ---- ----

    if (cy < m_nHeight) {
        m_nScrollMaxY = m_nHeight;
        //m_nPageSizeY = cy - nHeightHScroll;
		m_nPageSizeY = cy;
		m_nScrollPosY = min (m_nScrollPosY, m_nHeight - m_nPageSizeY);
   }
    else
        m_nScrollMaxY = m_nScrollPosY = m_nPageSizeY = 0;

    SCROLLINFO si;
    si.fMask = SIF_PAGE | SIF_RANGE | SIF_POS | SIF_DISABLENOSCROLL;
	si.cbSize = sizeof(SCROLLINFO);
    si.nMin = 0;
    si.nMax = m_nScrollMaxY;
    si.nPos = m_nScrollPosY;
    si.nPage = m_nPageSizeY;

    SetScrollInfo (SB_VERT, &si, TRUE);

    if (cx < m_nWidth) {
        m_nScrollMaxX = m_nWidth;
        //m_nPageSizeX = cx - nWidthVScroll;
		m_nPageSizeX = cx;
		m_nScrollPosX = min (m_nScrollPosX, m_nWidth - m_nPageSizeX);
    }
    else
        m_nScrollMaxX = m_nScrollPosX = m_nPageSizeX = 0;

    si.nMin = 0;
    si.nMax = m_nScrollMaxX;
    si.nPos = m_nScrollPosX;
    si.nPage = m_nPageSizeX;

    SetScrollInfo (SB_HORZ, &si, TRUE);
}
//----------------------------------------------------------------------------
void CMultiChartCtrl::OnSize (UINT nType, int cx, int cy)
{
    CButton::OnSize (nType, cx, cy);	
	// ----
	OnHFontValuesChanged ();
	OnHFontHeadersChanged ();
/*
	if (m_State == InvDraw) 
		RedrawWindow();
	else
	*/
	PrepareScrollInfo(cx,cy);

	//m_State = InvDim;
	//Refresh();
}
//-----------------------------------------------------------------------------

void CMultiChartCtrl::OnVScroll (UINT nCode, UINT nPos, CScrollBar* pScrollBar)
{
    SCROLLINFO si;
	si.cbSize = sizeof(SCROLLINFO);
    GetScrollInfo (SB_VERT, &si);
	nPos = si.nPos;
	//=============================
    int nDelta;
    int nMaxPos = m_nHeight - m_nPageSizeY;

    switch (nCode) {

    case SB_LINEUP:
        if (m_nScrollPosY <= 0)
            return;
        nDelta = -(min (m_nScrollStepY, m_nScrollPosY));
        break;

    case SB_PAGEUP:
        if (m_nScrollPosY <= 0)
            return;
        nDelta = -(min (m_nPageSizeY, m_nScrollPosY));
        break;

    case SB_THUMBTRACK:
    case SB_THUMBPOSITION:
		nPos = si.nTrackPos;
        nDelta = (int) nPos - m_nScrollPosY;
        break;

    case SB_PAGEDOWN:
        if (m_nScrollPosY >= nMaxPos)
            return;
        nDelta = min (m_nPageSizeY, nMaxPos - m_nScrollPosY);
        break;

    case SB_LINEDOWN:
        if (m_nScrollPosY >= nMaxPos)
            return;
        nDelta = min (m_nScrollStepY, nMaxPos - m_nScrollPosY);
        break;

    default: // Ignore other scroll bar messages
        return;
    }

    m_nScrollPosY += nDelta;

	//m_bVScroll = TRUE;

	si.nPos = m_nScrollPosY;
	si.fMask = SIF_POS;
    SetScrollInfo (SB_VERT, &si, TRUE);

	m_nScrollPosRow =  -((m_nScrollPosY / m_nDCHeightAllRows) - m_nRows + 1);

	Refresh();
}
//-----------------------------------------------------------------------------

void CMultiChartCtrl::OnHScroll (UINT nCode, UINT nPos, CScrollBar* pScrollBar)
{
    SCROLLINFO si;
	si.cbSize = sizeof(SCROLLINFO);
	GetScrollInfo (SB_HORZ, &si);
	CWnd* pParentExternalScrollbar = pScrollBar && pScrollBar->GetParent() != this ? pScrollBar->GetParent() : NULL;
	if (!pParentExternalScrollbar)
		nPos = si.nPos;

    int nDelta;
    int nMaxPos = m_nWidth - m_nPageSizeX;

    switch (nCode) {

    case SB_LINEUP:
        if (m_nScrollPosX <= 0)
            return;
        nDelta = -(min (m_nScrollStepX, m_nScrollPosX));
        break;

    case SB_PAGEUP:
        if (m_nScrollPosX <= 0)
            return;
        nDelta = -(min (m_nPageSizeX - GetDCWidthRowHeader(), m_nScrollPosX));
        break;

   case SB_THUMBTRACK:
   case SB_THUMBPOSITION:
	   nPos = pParentExternalScrollbar ? nPos : si.nTrackPos;
        nDelta = (int) nPos - m_nScrollPosX;
        break;

    case SB_PAGEDOWN:
        if (m_nScrollPosX >= nMaxPos)
            return;
        nDelta = min (m_nPageSizeX - GetDCWidthRowHeader(), nMaxPos - m_nScrollPosX);
        break;

    case SB_LINEDOWN:
        if (m_nScrollPosX >= nMaxPos)
            return;
        nDelta = min (m_nScrollStepX, nMaxPos - m_nScrollPosX);
        break;

    default: // Ignore other scroll bar messages
        return;
    }

    m_nScrollPosX += nDelta;

 	//m_bHScroll = TRUE;

	si.nPos = m_nScrollPosX;
	si.fMask = SIF_POS;
	SetScrollInfo (SB_HORZ, & si);

	m_nScrollPosCol = m_nScrollPosX / m_nDCWidthCols;

	//OnEndHScroll(m_nScrollPosCol);

	Refresh();

}
//-------------------------------------------------------------------------
BOOL CMultiChartCtrl::PreTranslateMessage(LPMSG lpmsg)
{
    BOOL bHandleNow = FALSE;

    switch (lpmsg->message)
    {
    case WM_KEYDOWN:

        switch (lpmsg->wParam)
        {
			case VK_UP:
			case VK_DOWN:

				if (m_hWnd) 
				{
					CRect rcBounds;
					GetClientRect (rcBounds);

					if ( m_nHeight < rcBounds.bottom)
						break;
				}

				bHandleNow = TRUE;
				break;

			case VK_LEFT:
			case VK_RIGHT:
 
				if (m_hWnd) 
				{
					CRect rcBounds;
					GetClientRect (rcBounds);

					if ( m_nWidth < rcBounds.right )
						break;
				}

				bHandleNow = TRUE;
				break;
        }
        if (bHandleNow)
            OnKeyDown((UINT)(lpmsg->wParam), LOWORD(lpmsg->lParam), HIWORD(lpmsg->lParam));
        break;
    }
    return bHandleNow;
}
//---------------------------------------------------------------------------------
void CMultiChartCtrl::OnKeyDown (UINT nChar, UINT nRepCnt, UINT nFlags)
{
    switch (nChar) {

		case VK_UP:
			SendMessage (WM_VSCROLL, SB_LINEUP, 0);
			break;

		case VK_PRIOR:
			SendMessage (WM_VSCROLL, SB_PAGEUP, 0);
			break;

		case VK_NEXT:
			SendMessage (WM_VSCROLL, SB_PAGEDOWN, 0);
			break;

		case VK_DOWN:
			SendMessage (WM_VSCROLL, SB_LINEDOWN, 0);
			break;

		case VK_LEFT:
			SendMessage (WM_HSCROLL, SB_LINEUP, 0);
			break;

		case VK_HOME:
			SendMessage (WM_HSCROLL, SB_PAGEUP, 0);
			break;

		case VK_END:
			SendMessage (WM_HSCROLL, SB_PAGEDOWN, 0);
			break;

		case VK_RIGHT:
			SendMessage (WM_HSCROLL, SB_LINEDOWN, 0);
			break;
	}
}
//---------------------------------------------------------------------------------
void CMultiChartCtrl::OnButtonUp(UINT nFlags, CPoint point, BOOL bLeft)
{	
	m_nXPosLastClick = (short) point.x;
	m_nYPosLastClick = (short) point.y;

	if (GetCapture () == this /*&& m_bIsTracking*/)
	{
		ReleaseCapture ();
	}
	//----

	CClientDC dc(this);
	CClientDC* pdc = &dc;
	CRect rcBounds;
	GetClientRect(rcBounds);
	// ----
	pdc->SetBkMode(TRANSPARENT);
	pdc->SetMapMode(MM_ANISOTROPIC);
	pdc->SetWindowExt(1, 1);
	pdc->SetViewportExt(1, - 1);
	pdc->SetViewportOrg(0, rcBounds.bottom);
	pdc->SetWindowOrg (m_nScrollPosX, m_nHeight - rcBounds.bottom - m_nScrollPosY);
	
	// ---- ----
	
	CPoint ptMouse = point;
	pdc->DPtoLP(&point);

	int nR;
	int nC;

	if ( point.y >= 0 )
		nR = point.y / m_nDCHeightAllRows ;
	else
		nR = -1;
	nC = (point.x - m_nDCWidthRowHeader) / m_nDCWidthCols ;
	// ----
	CPoint ptCorner;
	ptCorner.x = pdc->GetWindowOrg().x + m_nDCWidthRowHeader;
	ptCorner.y = pdc->GetWindowOrg().y + rcBounds.bottom - m_nDCHeightColHeader;
	
	CRect rcClip = rcBounds;
	pdc->DPtoLP (&rcClip);
	rcClip.bottom = ptCorner.y;
	rcClip.right  = ptCorner.x;
	int r = pdc->IntersectClipRect (rcClip);
	ASSERT(r != NULLREGION && r != ERROR);

	if ( point.y > ptCorner.y && point.x < ptCorner.x )
	{	// Left-Upper Header Corner
		if ( m_nSelectedRowHeader != -1 )
		{
			m_nSelectedRowHeader = -1;
			DrawRowHeader(pdc, rcBounds);
		}
		if ( m_nSelectedColHeader != -1 )
		{
			m_nSelectedColHeader = -1;
			DrawColHeader(pdc, rcBounds);
		}
		DrawCorner(pdc, rcBounds);

		if ( bLeft )
		{
		//verificare con Riccardo
			

			if ( m_nSelectedRowHeader != -1 )
			{
				m_nSelectedRowHeader = -1;
				DrawRowHeader(pdc, rcBounds);
			}
			if ( m_nSelectedColHeader != -1 )
			{
				m_nSelectedColHeader = -1;
				DrawColHeader(pdc, rcBounds);
			}
			//verificare con Riccardo
			//duale di rcClient.top -= m_nDCHeightColHeader;
			//rcClip.bottom = rcClip.top - m_nDCHeightColHeader;

			FireCornerHeaderLButtonDown();	// Fire event !
		}
		else
			FireCornerHeaderRButtonDown();

		m_bIsTracking = false;
		return;
	}
	// ----
	if ( point.y > ptCorner.y && point.x > ptCorner.x )
	{	// Column Header 
		if ( nC < m_nCols && nC >= 0 )
		{
			if ( bLeft )
			{
				//verificare con Riccardo
				CRect rcClip = rcBounds;
				pdc->DPtoLP (&rcClip);
				int r = pdc->IntersectClipRect (rcClip);
				ASSERT(r != NULLREGION && r != ERROR);

				if ( m_nSelectedRowHeader != -1 )
				{
					m_nSelectedRowHeader = -1;
					DrawRowHeader(pdc, rcBounds);
				}
				m_nSelectedColHeader = nC;

				DrawCorner(pdc, rcBounds);
				DrawColHeader(pdc, rcBounds);

				FireColHeaderLButtonDown(nC); // Fire event !
			}
			else
				FireColHeaderRButtonDown(nC);
		}
		m_bIsTracking = false;
		return;
	}
	// ----
	if ( point.y < ptCorner.y && point.x < ptCorner.x )
	{	// Row Header 
		if ( nR < m_nRows && nR >= 0 )
		{
			if ( bLeft ) 
			{	
				
				CRect rcClip = rcBounds;
				pdc->DPtoLP (&rcClip);
				int r = pdc->IntersectClipRect (rcClip);
				ASSERT(r != NULLREGION && r != ERROR);

				if ( m_nSelectedColHeader != -1 )
				{
					m_nSelectedColHeader = -1;
					DrawColHeader(pdc, rcBounds);
				}
				m_nSelectedRowHeader = nR;

				DrawCorner(pdc, rcBounds);
				DrawRowHeader(pdc, rcBounds);

				FireRowHeaderLButtonDown(nR); // Fire event !
			}
			else
				FireRowHeaderRButtonDown(nR); 
		}
		m_bIsTracking = false;
		return;
	}
	// ---- ----
	if (!bLeft && nR < m_nRows && nC < m_nCols && nR >= 0 && nC >= 0) 
	{
		for (int b =0; b < m_nBars; b++)
		{
			CBarHeader* pHB = &(m_arRows[nR].m_arColumns[nC].m_arHeaderBars[b]);
			if (pHB->m_ptIcon.x && pHB->m_ptIcon.y && DIST(pHB->m_ptIcon.x, pHB->m_ptIcon.y, point.x, point.y) < 5)
			{
				FireBarIconRButtonDown(nR,nC,b); // Fire event !
				return;
			}
		}
	}
	// ----
	CBar *pBar;
	if ( nR < m_nRows && nC < m_nCols && nR >= 0 && nC >= 0 )
		pBar = TouchBars(nR, nC, point);
	else
		pBar = NULL;

	if(pBar != NULL)
	{
		if (m_bIsTracking)
		{
			FireBarSegmentMoved
					(
						m_nTrackRow, m_nTrackCol, 
						m_pTrackBar->m_nInd, m_pTrackBar->m_nIndSegment, 
						nR, nC, 
						pBar->m_nInd, pBar->m_nIndSegment
					);
			m_bIsTracking = false;
			return;
		}

		//Clip Region: proteggo header
		CRect rcBars;
		rcBars.SetRect
			(
				ptCorner.x + 1,
				ptCorner.y - 1,
				max(ptCorner.x - m_nDCWidthRowHeader + rcBounds.right, 0),
				pdc->GetWindowOrg().y
			);
		pdc->IntersectClipRect(rcBars);

		//de-evidenzio la barra precedente e seleziono la corrente
		if( m_pSelectedBar )
			DrawOneBar( pdc, m_pSelectedBar);
		m_pSelectedBar = pBar;
		DrawOneBar( pdc, m_pSelectedBar, TRUE);
		// ----

		if (bLeft)	// Fire event !
		{
			if (pBar->m_nIndSegment == 0)
				FireBarLButtonDown( nR, nC, pBar->m_nInd ); 
			FireBarSegmentLButtonDown( nR, nC, pBar->m_nInd, pBar->m_nIndSegment ); 
		}
		else
		{
			if (pBar->m_nIndSegment == 0)
				FireBarRButtonDown( nR, nC, pBar->m_nInd );
			FireBarSegmentRButtonDown( nR, nC, pBar->m_nInd, pBar->m_nIndSegment ); 
		}
	} 
	else if( ! bLeft )
	{
		    CMenu menu;
#ifdef _DEBUG
			menu.LoadMenu (IDR_CONTEXTMENU);
#else
			menu.LoadMenu (IDR_CONTEXTMENU2);
#endif		
			if (menu)
			{
				CMenu* pContextMenu = menu.GetSubMenu (0);
			
				ClientToScreen( &ptMouse);

				pContextMenu->TrackPopupMenu
				(
					TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
					ptMouse.x, ptMouse.y, this
				);
			}
	}

	m_bIsTracking = false;
}

//----------------------------------------------------------------------------
void CMultiChartCtrl::OnLButtonUp(UINT nFlags, CPoint point)
{	
	SetFocus();

	OnButtonUp (nFlags, point, TRUE);
}
//----------------------------------------------------------------------------
void CMultiChartCtrl::OnRButtonUp(UINT nFlags, CPoint point)
{	
	SetFocus();

	OnButtonUp (nFlags, point, FALSE);
}

//------------------------------------------------------------------------------
void CMultiChartCtrl::OnLButtonDown(UINT nFlags, CPoint point)
{	
	//----

	if ((nFlags & MK_SHIFT) == 0)
		return;

	m_nXPosLastClick = (short) point.x;
	m_nYPosLastClick = (short) point.y;
	//----

	CClientDC dc(this);
	CClientDC* pdc = &dc;
	CRect rcBounds;
	GetClientRect(rcBounds);
	// ----

	pdc->SetBkMode(TRANSPARENT);
	pdc->SetMapMode(MM_ANISOTROPIC);
	pdc->SetWindowExt(1, 1);
	pdc->SetViewportExt(1, - 1);
	pdc->SetViewportOrg(0, rcBounds.bottom);
	pdc->SetWindowOrg (m_nScrollPosX, m_nHeight - rcBounds.bottom - m_nScrollPosY);
	// ----
	
	CPoint ptMouse = point;
	pdc->DPtoLP(&point);

	int nR;
	int nC;

	if ( point.y >= 0 )
		nR = point.y / m_nDCHeightAllRows ;
	else
		nR = -1;
	nC = (point.x - m_nDCWidthRowHeader) / m_nDCWidthCols ;
	// ----
	CPoint ptCorner;
	ptCorner.x = pdc->GetWindowOrg().x + m_nDCWidthRowHeader;
	ptCorner.y = pdc->GetWindowOrg().y + rcBounds.bottom - m_nDCHeightColHeader;

	if ( point.y > ptCorner.y && point.x < ptCorner.x )
	{	// Left-Upper Header Corner
		return;
	}
	// ----
	if ( point.y > ptCorner.y && point.x > ptCorner.x )
	{	// Column Header 
		return;
	}
	// ----
	if ( point.y < ptCorner.y && point.x < ptCorner.x )
	{	// Row Header 
		return;
	}
	// ---- ----

	CBar *pBar;
	if ( nR < m_nRows && nC < m_nCols && nR >= 0 && nC >= 0 )
		pBar = TouchBars(nR, nC, point);
	else
		pBar = NULL;

	if (pBar)
	{
		m_bIsTracking = true;
		SetCursor (LoadCursor(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDC_CUR_DRAGMOVE)));
		SetCapture ();

		m_nTrackRow = nR;
		m_nTrackCol = nC;
		m_pTrackBar = pBar;
	}
}

//------------------------------------------------------------------------------
void CMultiChartCtrl::OnMouseMove( UINT nFlags, CPoint point )
{
	if (GetCapture () == this && m_bIsTracking)
	{
		SetCursor ( LoadCursor(AfxGetInstanceHandle(), MAKEINTRESOURCE(IDC_CUR_DRAGMOVE)) );
	}
	else if ( ! m_bShowToolTip ) return;

	CPoint ptMouse = point;

	CClientDC dc(this);
	CClientDC* pdc = &dc;
	CRect rcBounds;
	GetClientRect(rcBounds);

	pdc->SetBkMode(TRANSPARENT);
	pdc->SetMapMode(MM_ANISOTROPIC);
	pdc->SetWindowExt(1, 1);
	pdc->SetViewportExt(1, - 1);
	pdc->SetViewportOrg(0, rcBounds.bottom);
	pdc->SetWindowOrg (m_nScrollPosX, m_nHeight - rcBounds.bottom - m_nScrollPosY + 1);
	pdc->DPtoLP(&point);
	// ---- ----

	int nR;
	int nC;

	if ( point.y >= 0 )
		nR = point.y / m_nDCHeightAllRows ;
	else
		nR = -1;
	nC = (point.x - m_nDCWidthRowHeader) / m_nDCWidthCols ;
	// ----

	CPoint ptCorner;
	ptCorner.x = pdc->GetWindowOrg().x + m_nDCWidthRowHeader;
	ptCorner.y = pdc->GetWindowOrg().y + rcBounds.bottom - m_nDCHeightColHeader;
	
	if ( point.y > ptCorner.y && point.x < ptCorner.x ) // Left-Upper Header Corner
		return;	
	if ( point.y > ptCorner.y && point.x > ptCorner.x ) // Column Header 
		return;
	if ( point.y < ptCorner.y && point.x < ptCorner.x ) // Row Header 
		return;
	// ----

	CBar *pBar;
	if ( nR < m_nRows && nC < m_nCols && nR >= 0 && nC >= 0 )
		pBar = TouchBars(nR, nC, point);
	else
		pBar = NULL;

	if ( pBar != NULL )
	{
		CString strH, strToolTip;
		int nB = pBar -> m_nInd;

		if (!pBar->m_strToolTipText.IsEmpty())
			strToolTip = pBar->m_strToolTipText;
		else if (!pBar->m_strText.IsEmpty())
			strToolTip = pBar->m_strText;
		else
		{
			if ( ! m_arInfoBars[nB].m_strTitlePosBar.IsEmpty () ) 
				strToolTip = m_arInfoBars[nB].m_strTitlePosBar + _T(": ");

			if ( ! m_arInfoBars[nB].m_strFormatHeightPosBar.IsEmpty () ) 
			{
				strH.Format( m_arInfoBars[nB].m_strFormatHeightPosBar, pBar->m_dHeight);
				strH.TrimLeft ();
				strToolTip += strH;
			}
		}
	
		// set datatip for this point	
		if ( ! strToolTip.IsEmpty () ) 
			m_datatip->Set (ptMouse, strToolTip);
	}
}
//------------------------------------------------------------------------------------
CBar * CMultiChartCtrl::TouchBars(CPoint pt)
{
	CBar *pBar;
	int nR, nC;

	for(nR = 0; nR < m_nRows; nR += 1)
	{
		for(nC = 0; nC < m_nCols; nC += 1)
		{
			pBar = TouchBars(nR, nC, pt);
			if( pBar != NULL ) return pBar;
		}	
	}
	return NULL;
}
// -----------------------------------------------------------

CBar * CMultiChartCtrl::TouchBars(int nR, int nC, CPoint pt)
{
	if( nR < 0 || nR >= m_nAllocatedRows || 
		nC < 0 || nC >= m_nAllocatedCols )
		return NULL;
	// ----
	CBar *pBar; CRect rcNorm;

for(int nB = 0; nB < m_nBars; nB += 1)
	for(int nS = 0; nS < m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment; nS += 1)
	{
		pBar = &(m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS]);

		rcNorm = pBar->m_rcBar;
		int nDCTouchBonus = m_arInfoBars[nB].m_nDCShowMinHeightPosBar ? m_arInfoBars[nB].m_nDCShowMinHeightPosBar : 5;

		if( m_nTypeChart == Tipo_Verticale )
		{
			if ( (m_nDCHeightAllRows - nDCTouchBonus) > (abs(rcNorm.Height()) + m_nDCHFontValues) )
				rcNorm.top += nDCTouchBonus;
		}
		else if( m_nTypeChart == Tipo_Orizzontale )
		{
			if ( (m_nDCWidthCols - nDCTouchBonus) > abs(rcNorm.Width()) )
				rcNorm.right += nDCTouchBonus;
		}

		rcNorm.NormalizeRect();
		if(rcNorm.PtInRect(pt)) return pBar;
	}
	return NULL;
}
