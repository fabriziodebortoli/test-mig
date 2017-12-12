//*******************************************************************************
// COPYRIGHT NOTES
// ---------------
// This is a part of BCGControlBar Library Professional Edition
// Copyright (C) 1998-2015 BCGSoft Ltd.
// All rights reserved.
//
// This source code can be used, distributed or modified
// only under terms and conditions 
// of the accompanying license agreement.
//*******************************************************************************
//
// BCGPSliderCtrl.cpp : implementation file
//

#include "stdafx.h"
#include "BCGPVisualManager.h"
#include "bcgglobals.h"
#include "BCGPSliderCtrl.h"
#include "BCGPDlgImpl.h"
#include "BCGPDrawManager.h"
#ifndef _BCGSUITE_
#include "BCGPBaseControlBar.h"
#include "BCGPToolBarImages.h"
#endif
#include "trackmouse.h"
#include "BCGPMath.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CBCGPSliderCtrl

IMPLEMENT_DYNAMIC(CBCGPSliderCtrl, CSliderCtrl)

CBCGPSliderCtrl::CBCGPSliderCtrl()
{
	m_bVisualManagerStyle = FALSE;
	m_bOnGlass = FALSE;
	m_bTracked = FALSE;
	m_nTicFreq = 1;
	m_bIsThumbHighligted = FALSE;
	m_bIsThumPressed = FALSE;
	m_clrSelection = (COLORREF)-1;
}

CBCGPSliderCtrl::~CBCGPSliderCtrl()
{
}

BEGIN_MESSAGE_MAP(CBCGPSliderCtrl, CSliderCtrl)
	//{{AFX_MSG_MAP(CBCGPSliderCtrl)
	ON_WM_ERASEBKGND()
	ON_WM_CANCELMODE()
	ON_WM_MOUSEMOVE()
	ON_WM_PAINT()
	ON_WM_LBUTTONUP()
	ON_WM_LBUTTONDOWN()
	//}}AFX_MSG_MAP
	ON_MESSAGE(WM_MOUSELEAVE, OnMouseLeave)
	ON_REGISTERED_MESSAGE(BCGM_ONSETCONTROLVMMODE, OnBCGSetControlVMMode)
	ON_REGISTERED_MESSAGE(BCGM_ONSETCONTROLAERO, OnBCGSetControlAero)
	ON_MESSAGE(WM_PRINTCLIENT, OnPrintClient)
	ON_MESSAGE(TBM_SETTICFREQ, OnSetTicFreq)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CBCGPSliderCtrl message handlers

BOOL CBCGPSliderCtrl::OnEraseBkgnd(CDC* /*pDC*/)
{
	if (!m_bVisualManagerStyle)
	{
		return (BOOL) Default ();
	}

	return TRUE;
}
//**************************************************************************
LRESULT CBCGPSliderCtrl::OnBCGSetControlVMMode (WPARAM wp, LPARAM)
{
	m_bVisualManagerStyle = (BOOL) wp;
	RedrawWindow ();
	return 0;
}
//**************************************************************************
LRESULT CBCGPSliderCtrl::OnBCGSetControlAero (WPARAM wp, LPARAM)
{
	m_bOnGlass = (BOOL) wp;
	return 0;
}
//**************************************************************************
void CBCGPSliderCtrl::OnCancelMode() 
{
	CSliderCtrl::OnCancelMode();

	m_bIsThumbHighligted = FALSE;
	m_bIsThumPressed = FALSE;

	RedrawWindow ();
}
//*****************************************************************************************
void CBCGPSliderCtrl::OnMouseMove(UINT nFlags, CPoint point) 
{
	BOOL bIsThumbHighligted = m_bIsThumbHighligted;

	if (GetStyle() & TBS_NOTHUMB)
	{
		m_bIsThumbHighligted = FALSE;
		CSliderCtrl::OnMouseMove(nFlags, point);
		return;
	}

	CRect rectThumb;
	GetThumbRect (rectThumb);

	m_bIsThumbHighligted = rectThumb.PtInRect (point);

	CSliderCtrl::OnMouseMove(nFlags, point);

	if (bIsThumbHighligted != m_bIsThumbHighligted)
	{
		RedrawWindow ();
	}

	if (!m_bTracked)
	{
		m_bTracked = TRUE;
		
		TRACKMOUSEEVENT trackmouseevent;
		trackmouseevent.cbSize = sizeof(trackmouseevent);
		trackmouseevent.dwFlags = TME_LEAVE;
		trackmouseevent.hwndTrack = GetSafeHwnd();
		trackmouseevent.dwHoverTime = HOVER_DEFAULT;
		::BCGPTrackMouse (&trackmouseevent);	
	}
}
//*****************************************************************************************
LRESULT CBCGPSliderCtrl::OnMouseLeave(WPARAM,LPARAM)
{
	m_bTracked = FALSE;

	if (m_bIsThumbHighligted)
	{
		m_bIsThumbHighligted = FALSE;
		RedrawWindow ();
	}

	return 0;
}
//*****************************************************************************************
void CBCGPSliderCtrl::OnPaint() 
{
	if (!m_bVisualManagerStyle)
	{
		Default ();
		return;
	}

	CPaintDC dc(this); // device context for painting
	OnDraw(&dc);
}
//*****************************************************************************************
void CBCGPSliderCtrl::OnDraw(CDC* pDCIn) 
{
	CBCGPMemDC memDC (*pDCIn, this);
	CDC* pDC = &memDC.GetDC ();

	if (!CBCGPVisualManager::GetInstance ()->OnFillParentBarBackground (this, pDC))
	{
		globalData.DrawParentBackground (this, pDC, NULL);
	}

	DWORD dwStyle = GetStyle ();
	BOOL bVert = (dwStyle & TBS_VERT);
	BOOL bLeftTop = (dwStyle & TBS_BOTH) || (dwStyle & TBS_LEFT);
	BOOL bRightBottom = (dwStyle & TBS_BOTH) || ((dwStyle & TBS_LEFT) == 0);
	BOOL bIsFocused = GetSafeHwnd () == ::GetFocus ();
	BOOL bIsAutoTicks = (dwStyle & TBS_AUTOTICKS);

	CRect rectChannel;
	GetChannelRect (rectChannel);

	if (bVert)
	{
		CRect rect = rectChannel;

		rectChannel.left = rect.top;
		rectChannel.right = rect.bottom;
		rectChannel.top = rect.left;
		rectChannel.bottom = rect.right;
	}

	CRect rectSel = GetSelectionRect();

	CBCGPDrawOnGlass dog(m_bOnGlass);
	OnDrawChannel(pDC, bVert, rectChannel, m_bOnGlass);

	CRect rectThumb;
	GetThumbRect (rectThumb);

	int nTicSize = max(3, (bVert ? rectThumb.Height() : rectThumb.Width()) / 3);
	int nTicOffset = 1;

	int nNumTics = GetNumTics();
	int nRange = GetRangeMax() - GetRangeMin();
	int nTicFreq = (nNumTics == 2) ? nRange : m_nTicFreq;

	for (int i = 0; i < nNumTics; i++)
	{
		int nTicPos = GetTicPos(bIsAutoTicks ? i * nTicFreq : i);

		if (nTicPos < 0)
		{
			if (i == nNumTics - 2)
			{
				if (bVert)
				{
					nTicPos = rectChannel.top + rectThumb.Height() / 2;
				}
				else
				{
					nTicPos = rectChannel.left + rectThumb.Width() / 2;
				}
			}
			else if (i == nNumTics - 1)
			{
				if (bVert)
				{
					nTicPos = rectChannel.bottom - rectThumb.Height() / 2 - 1;
				}
				else
				{
					nTicPos = rectChannel.right - rectThumb.Width() / 2 - 1;
				}
			}
		}

		if (nTicPos >= 0)
		{
			CRect rectTic1(0, 0, 0, 0);
			CRect rectTic2(0, 0, 0, 0);

			if (bVert)
			{
				if (bLeftTop)
				{
					rectTic1 = CRect(rectThumb.left - nTicOffset - nTicSize, nTicPos, rectThumb.left - nTicOffset, nTicPos + 1);
				}
				
				if (bRightBottom)
				{
					rectTic2 = CRect(rectThumb.right + nTicOffset, nTicPos, rectThumb.right + nTicOffset + nTicSize, nTicPos + 1);
				}
			}
			else
			{
				if (bLeftTop)
				{
					rectTic1 = CRect(nTicPos, rectThumb.top - nTicOffset - nTicSize, nTicPos + 1, rectThumb.top - nTicOffset);
				}

				if (bRightBottom)
				{
					rectTic2 = CRect(nTicPos, rectThumb.bottom + nTicOffset, nTicPos + 1, rectThumb.bottom + nTicOffset + nTicSize);
				}
			}

			if (!rectTic1.IsRectEmpty())
			{
				CBCGPVisualManager::GetInstance ()->OnDrawSliderTic(pDC, this, rectTic1, bVert, TRUE, m_bOnGlass);
			}

			if (!rectTic2.IsRectEmpty())
			{
				CBCGPVisualManager::GetInstance ()->OnDrawSliderTic(pDC, this, rectTic2, bVert, FALSE, m_bOnGlass);
			}
		}
	}

	if (!rectSel.IsRectEmpty() && nNumTics >= 2)
	{
		for (int i = 0; i < 2; i++)
		{
			CRect rectSelMarker1(0, 0, 0, 0);
			CRect rectSelMarker2(0, 0, 0, 0);

			int nMarkerPos = 0;
			
			if (bVert)
			{
				nMarkerPos = (i == 0) ? rectSel.top : rectSel.bottom;

				if (bLeftTop)
				{
					rectSelMarker1 = CRect(rectThumb.left - nTicOffset - nTicSize, nMarkerPos, rectThumb.left - nTicOffset, nMarkerPos + 1);
				}
				
				if (bRightBottom)
				{
					rectSelMarker2 = CRect(rectThumb.right + nTicOffset, nMarkerPos, rectThumb.right + nTicOffset + nTicSize, nMarkerPos + 1);
				}
			}
			else
			{
				nMarkerPos = (i == 0) ? rectSel.left : rectSel.right;

				if (bLeftTop)
				{
					rectSelMarker1 = CRect(nMarkerPos - nTicSize / 2 - 2, rectThumb.top - nTicOffset - nTicSize, nMarkerPos + nTicSize / 2 - 1, rectThumb.top - nTicOffset);
				}
				
				if (bRightBottom)
				{
					rectSelMarker2 = CRect(nMarkerPos - nTicSize / 2 - 2, rectThumb.bottom + nTicOffset, nMarkerPos + nTicSize / 2 - 1, rectThumb.bottom + nTicOffset + nTicSize);
				}
			}

			if (i == 1)
			{
				if (bVert)
				{
					rectSelMarker1.OffsetRect(0, nTicSize);
					rectSelMarker2.OffsetRect(0, nTicSize);
				}
				else
				{
					rectSelMarker1.OffsetRect(nTicSize, 0);
					rectSelMarker2.OffsetRect(nTicSize, 0);
				}
			}
			
			if (!rectSelMarker1.IsRectEmpty())
			{
				CBCGPVisualManager::GetInstance ()->OnDrawSliderSelectionMarker(pDC, this, rectSelMarker1, i == 0, bVert, TRUE, m_bOnGlass);
			}
			
			if (!rectSelMarker2.IsRectEmpty())
			{
				CBCGPVisualManager::GetInstance ()->OnDrawSliderSelectionMarker(pDC, this, rectSelMarker2, i == 0, bVert, FALSE, m_bOnGlass);
			}
		}
	}

	if ((GetStyle() & TBS_NOTHUMB) == 0)
	{
		OnDrawThumb(pDC, rectThumb, m_bIsThumbHighligted || bIsFocused,
			m_bIsThumPressed, !IsWindowEnabled (),
			bVert, bLeftTop, bRightBottom, m_bOnGlass);
	}

	if (bIsFocused && m_bDrawFocus)
	{
		CRect rectFocus;
		GetClientRect (rectFocus);

		if (m_bOnGlass)
		{
			CBCGPDrawManager dm (*pDC);
			dm.DrawFocusRect(rectFocus);
		}
		else
		{
			pDC->DrawFocusRect (rectFocus);
		}
	}
}
//*****************************************************************************************
void CBCGPSliderCtrl::OnLButtonUp(UINT nFlags, CPoint point) 
{
	if (m_bIsThumPressed)
	{
		m_bIsThumPressed = FALSE;
		RedrawWindow ();
	}
	
	CSliderCtrl::OnLButtonUp(nFlags, point);
}
//*****************************************************************************************
void CBCGPSliderCtrl::OnLButtonDown(UINT nFlags, CPoint point) 
{
	if ((GetStyle() & TBS_NOTHUMB) == 0)
	{
		CRect rectThumb;
		GetThumbRect (rectThumb);

		m_bIsThumPressed = rectThumb.PtInRect (point);
		
		if (m_bIsThumPressed)
		{
			RedrawWindow ();
		}
	}
	
	CSliderCtrl::OnLButtonDown(nFlags, point);
}
//*******************************************************************************
LRESULT CBCGPSliderCtrl::OnPrintClient(WPARAM wp, LPARAM lp)
{
	if (lp & PRF_CLIENT)
	{
		if (!m_bVisualManagerStyle)
		{
			return Default();
		}

		CDC* pDC = CDC::FromHandle((HDC) wp);
		ASSERT_VALID(pDC);

		OnDraw(pDC);
	}

	return 0;
}
//*******************************************************************************
LRESULT CBCGPSliderCtrl::OnSetTicFreq(WPARAM wp, LPARAM)
{
	m_nTicFreq = (int)wp;
	return Default();	
}
//*******************************************************************************
CRect CBCGPSliderCtrl::GetSelectionRect()
{
	DWORD dwStyle = GetStyle ();
	BOOL bVert = (dwStyle & TBS_VERT);
	BOOL bIsAutoTicks = (dwStyle & TBS_AUTOTICKS);
	BOOL bThumb = ((GetStyle() & TBS_NOTHUMB) == 0);

	CRect rectSel;
	rectSel.SetRectEmpty();

	if ((dwStyle & TBS_ENABLESELRANGE) == 0)
	{
		return rectSel;
	}

	int nRangeMin = GetRangeMin();
	int nRangeMax = GetRangeMax();
	
	if (nRangeMin >= nRangeMax)
	{
		return rectSel;
	}
	
	int nMin = 0;
	int nMax = 0;

	GetSelection(nMin, nMax);

	nMin = bcg_clamp(nMin, nRangeMin, nRangeMax);
	nMax = bcg_clamp(nMax, nRangeMin, nRangeMax);

	if (nMin >= nMax)
	{
		return rectSel;
	}

	CRect rectChannel;
	GetChannelRect (rectChannel);

	CRect rectThumb(0,0,0,0);
	
	if (bThumb)
	{
		GetThumbRect(rectThumb);
	}

	if (bVert)
	{
		CRect rect = rectChannel;

		rectChannel.left = rect.top;
		rectChannel.right = rect.bottom;
		rectChannel.top = rect.left;
		rectChannel.bottom = rect.right;

		double dblRatio = (double)(rectChannel.Height() - rectThumb.Height() / 2) / (nRangeMax - nRangeMin);
		
		rectSel = rectChannel;

		rectSel.DeflateRect(1, 0);
		rectSel.top += (int)(0.5 + dblRatio * (nMin - nRangeMin) + rectThumb.Height() / 2);
		rectSel.bottom = rectChannel.top + (int)(0.5 + dblRatio * (nMax - nRangeMin));
	}
	else
	{
		double dblRatio = (double)(rectChannel.Width() - rectThumb.Width() / 2) / (nRangeMax - nRangeMin);

		rectSel = rectChannel;

		rectSel.DeflateRect(0, 1);
		
		rectSel.left += (int)(0.5 + dblRatio * (nMin - nRangeMin)+ rectThumb.Width() / 2);
		rectSel.right = rectChannel.left + (int)(0.5 + dblRatio * (nMax - nRangeMin));
	}

	// Align to tick marks:

	int nNumTics = GetNumTics();
	int nTicFreq = (nNumTics == 2) ? (nRangeMax - nRangeMin) : m_nTicFreq;
	BOOL bStartIsReady = FALSE;
	int nPrevTicPos = -1;

	int nStartPos = bVert ? rectSel.top : rectSel.left;
	int nFinishPos = bVert ? rectSel.bottom : rectSel.right;
	
	for (int i = 0; i < nNumTics; i++)
	{
		int nTicPos = GetTicPos(bIsAutoTicks ? i * nTicFreq : i);
		if (nTicPos < 0)
		{
			continue;
		}

		if (!bStartIsReady)
		{
			if (nTicPos == nStartPos)
			{
				bStartIsReady = TRUE;
			}
			else
			{
				if (nTicPos > nStartPos)
				{
					if (nPrevTicPos >= 0)
					{
						if (bVert)
						{
							rectSel.top = nPrevTicPos + 1;
						}
						else
						{
							rectSel.left = nPrevTicPos + 1;
						}
					}

					bStartIsReady = TRUE;
				}
			}
		}

		if (nTicPos > nFinishPos)
		{
			if (bVert)
			{
				rectSel.bottom = nTicPos;
			}
			else
			{
				rectSel.right = nTicPos;
			}
			break;
		}

		nPrevTicPos = nTicPos;
	}

	return rectSel;
}
//*******************************************************************************
void CBCGPSliderCtrl::OnDrawThumb(CDC* pDC, CRect rectThumb, BOOL bIsHighlighted, BOOL bIsPressed, BOOL bIsDisabled, BOOL bVert, BOOL bLeftTop, BOOL bRightBottom, BOOL bDrawOnGlass)
{
	CBCGPVisualManager::GetInstance ()->OnDrawSliderThumb(
		pDC, this, rectThumb, bIsHighlighted, bIsPressed, bIsDisabled,
		bVert, bLeftTop, bRightBottom, bDrawOnGlass);
}
//*******************************************************************************
void CBCGPSliderCtrl::OnDrawChannel(CDC* pDC, BOOL bVert, CRect rect, BOOL bDrawOnGlass)
{
	CBCGPVisualManager::GetInstance ()->OnDrawSliderChannel (pDC, this, bVert, rect, bDrawOnGlass);
}
