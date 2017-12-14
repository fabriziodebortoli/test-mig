//*******************************************************************************
// COPYRIGHT NOTES 
// ---------------
// This is a part of the BCGControlBar Library
// Copyright (C) 1998-2015 BCGSoft Ltd.
// All rights reserved.
//
// This source code can be used, distributed or modified
// only under terms and conditions 
// of the accompanying license agreement.
//*******************************************************************************
//
// BCGPVisualManager2013.cpp: implementation of the CBCGPVisualManager2013 class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "BCGPToolbarMenuButton.h"
#include "BCGPOutlookBarPane.h"
#include "BCGPVisualManager2013.h"
#include "BCGGlobals.h"
#include "BCGPToolbar.h"
#include "BCGPToolbarButton.h"
#include "MenuImages.h"
#include "BCGPDrawManager.h"
#include "BCGPCaptionButton.h"
#include "BCGPMiniFrameWnd.h"
#include "BCGPSlider.h"
#include "BCGPPopupMenuBar.h"
#include "BCGPTabWnd.h"
#include "BCGPGridCtrl.h"
#include "BCGPToolTipCtrl.h"
#include "BCGPToolBox.h"
#include "BCGPAutoHideButton.h"
#include "BCGPReBar.h"
#include "BCGPRibbonPaletteButton.h"
#include "BCGPListBox.h"
#include "BCGPStatusBar.h"
#include "BCGPURLLinkButton.h"
#include "BCGPPropList.h"
#include "BCGPPopupWindow.h"
#include "BCGPTreeCtrl.h"
#include "BCGPMath.h"
#include "BCGPGanttItem.h"
#include "BCGPGanttChart.h"
#include "BCGPHeaderCtrl.h"
#include "BCGPEdit.h"

#ifndef BCGP_EXCLUDE_PLANNER
#include "BCGPPlannerManagerCtrl.h"
#include "BCGPPlannerViewDay.h"
#include "BCGPPlannerViewWeek.h"
#include "BCGPPlannerViewMonth.h"
#include "BCGPPlannerViewMulti.h"
#endif

#ifndef BCGP_EXCLUDE_RIBBON
#include "BCGPRibbonCategory.h"
#include "BCGPRibbonMainPanel.h"
#include "BCGPRibbonPanel.h"
#include "BCGPRibbonStatusBar.h"
#include "BCGPRibbonStatusBarPane.h"
#include "BCGPRibbonHyperlink.h"
#include "BCGPRibbonEdit.h"
#include "BCGPRibbonProgressBar.h"
#include "BCGPRibbonSlider.h"
#include "BCGPRibbonBackstageView.h"
#endif

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

static COLORREF GetPlannerTodayFill(COLORREF clr)
{
	double h, s, l;
	CBCGPDrawManager::RGBtoHSV(clr, &h, &s, &l);
	s = bcg_clamp(s + 0.13, 0.0, 1.0);
	l = bcg_clamp(l - 0.07, 0.0, 1.0);
	return CBCGPDrawManager::HSVtoRGB(h, s, l);
};

static COLORREF GetPlannerHeader(COLORREF clr)
{
	return CBCGPDrawManager::PixelAlpha(RGB(255, 255, 255), clr, 60);
};

static COLORREF CalculateHourLineColor (COLORREF clrBase, BOOL /*bWorkingHours*/, BOOL bHour)
{
	int nAlpha = bHour ? 85 : 95;
	return CBCGPDrawManager::PixelAlpha (clrBase, nAlpha);
}

//////////////////////////////////////////////////////////////////////////////
// CBCGPVisualManager2013

IMPLEMENT_DYNCREATE(CBCGPVisualManager2013, CBCGPVisualManagerVS2012)

CBCGPVisualManager2013::Style CBCGPVisualManager2013::m_Style = CBCGPVisualManager2013::Office2013_White;

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CBCGPVisualManager2013::CBCGPVisualManager2013()
{
	CBCGPVisualManagerVS2012::m_Style = VS2012_Light;

	m_bEmbossGripper = FALSE;
	m_bCheckedRibbonButtonFrame = FALSE;
}
//*********************************************************************************************************
CBCGPVisualManager2013::~CBCGPVisualManager2013()
{
	CBCGPMenuImages::SetColor (CBCGPMenuImages::ImageBlack, (COLORREF)-1);
	CBCGPMenuImages::SetColor (CBCGPMenuImages::ImageBlack2, (COLORREF)-1);
}
//**************************************************************************************************************
void CBCGPVisualManager2013::SetStyle(Style style)
{
	m_Style = style;

	CBCGPVisualManager2013* pThis = DYNAMIC_DOWNCAST(CBCGPVisualManager2013, CBCGPVisualManager::GetInstance ());
	if (pThis != NULL)
	{
		pThis->OnUpdateSystemColors();
	}
}
//*********************************************************************************************************
void CBCGPVisualManager2013::ModifyGlobalColors ()
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::ModifyGlobalColors();
		return;
	}

	if (m_Style == Office2013_White)
	{
		globalData.clrBarFace = RGB (255, 255, 255);
		globalData.clrBarText = RGB (59, 59, 59);
		globalData.clrBarShadow = RGB (238, 238, 238);
		globalData.clrBarHilite = RGB (255, 255, 255);
		globalData.clrBarDkShadow = RGB(162, 162, 162);
		globalData.clrBarLight = RGB (255, 255, 255);
	}
	else if (m_Style == Office2013_Gray)
	{
		globalData.clrBarFace = RGB (241, 241, 241);
		globalData.clrBarText = RGB (30, 30, 30);
		globalData.clrBarShadow = RGB (230, 230, 230);
		globalData.clrBarHilite = RGB (255, 255, 255);
		globalData.clrBarDkShadow = RGB(198, 198, 198);
		globalData.clrBarLight = RGB (250, 250, 250);
	}
	else  // Office2013_DarkGray
	{
		globalData.clrBarFace = RGB (229, 229, 229);
		globalData.clrBarText = RGB (30, 30, 30);
		globalData.clrBarShadow = RGB (224, 224, 224);
		globalData.clrBarHilite = RGB (255, 255, 255);
		globalData.clrBarDkShadow = RGB(171, 171, 171);
		globalData.clrBarLight = RGB (243, 243, 243);
	}

	globalData.brBarFace.DeleteObject ();
	globalData.brBarFace.CreateSolidBrush (globalData.clrBarFace);
}
//************************************************************************************
void CBCGPVisualManager2013::SetupColors()
{
	switch (m_curAccentColor)
	{
	case VS2012_Blue:
		m_clrAccent = RGB(43, 87, 154);	// Word
		break;
		
	case VS2012_Brown:
		m_clrAccent = RGB(164, 55, 58);	// Access
		break;
		
	case VS2012_Green:
		m_clrAccent = RGB(33, 115, 70);	// Excel
		break;
		
	case VS2012_Lime:
		m_clrAccent = RGB(137, 164, 48);
		break;
		
	case VS2012_Magenta:
		m_clrAccent = RGB(216,0,115);
		break;
		
	case VS2012_Orange:
		m_clrAccent = RGB(210, 71, 38);	// Power point
		break;
		
	case VS2012_Pink:
		m_clrAccent = RGB(172, 0, 112);
		break;
		
	case VS2012_Purple:
		m_clrAccent = RGB(128, 57, 123); // OneNote
		break;
		
	case VS2012_Red:
		m_clrAccent = RGB(229,20,0);
		break;
		
	case VS2012_Teal:
		m_clrAccent = RGB(7, 117, 104);	// Publisher
		break;
	}

//	m_clrAccent = RGB(0, 114, 198);	// Outlook ????

	CBCGPVisualManagerVS2012::SetupColors();

	if (m_curAccentColor == CBCGPVisualManagerVS2012::VS2012_Blue && !IsDarkTheme())
	{
		m_clrAccentText = RGB(13, 100, 200);
	}

	m_clrMenuLight = RGB(255, 255, 255);

	m_clrAccentLight = CBCGPDrawManager::ColorMakePale(m_clrAccent, .9);

	m_clrHighlight = m_clrAccentLight;
	m_clrHighlightMenuItem = m_clrAccentLight;
	m_clrHighlightDn = CBCGPDrawManager::ColorMakeDarker(m_clrAccentLight, .14);
	m_clrHighlightChecked = CBCGPDrawManager::ColorMakeDarker(m_clrAccentLight, .05);
	
	m_clrHighlightGradientLight = m_clrAccentLight;
	m_clrHighlightGradientDark = m_clrAccentLight;
	
	m_clrHighlightDnGradientDark = m_clrHighlightDn;
	m_clrHighlightDnGradientLight =  m_clrHighlightDn;

	m_clrEditBoxBorder = RGB(171, 171, 171);

	switch (m_Style)
	{
	case Office2013_White:
		m_clrSeparator = m_clrMenuSeparator = RGB(212, 212, 212);
		break;

	case Office2013_Gray:
		m_clrSeparator = m_clrMenuSeparator = RGB(198, 198, 198);
		break;

	case Office2013_DarkGray:
		m_clrSeparator = m_clrMenuSeparator = RGB(171, 171, 171);
		break;
	}

	CBCGPMenuImages::SetColor (CBCGPMenuImages::ImageBlack, RGB(119, 119, 119));
	CBCGPMenuImages::SetColor (CBCGPMenuImages::ImageBlack2, m_clrAccentText);

	m_clrRibbonTabs = RGB(102, 102, 102);

	m_clrMenuBorder = CBCGPDrawManager::ColorMakeLighter(globalData.clrBarDkShadow);

	m_clrEditCtrlSelectionBkActive = CBCGPDrawManager::ColorMakePale(m_clrAccent, .85);
	m_clrEditCtrlSelectionBkInactive = CBCGPDrawManager::ColorMakePale(m_clrAccent, .95);

	m_clrTabsBackground = globalData.clrBarFace;
	m_clrGripper = globalData.clrBarDkShadow;

	m_clrReportGroupText = globalData.clrBarText;
	m_clrHighlighDownText = globalData.clrBarText;

	m_clrRibbonCategoryFill = globalData.clrBarLight;
	m_clrCombo = globalData.clrBarHilite;
	m_clrControl = globalData.clrBarHilite;
	m_clrDlgBackground = globalData.clrBarLight;

#ifndef BCGP_EXCLUDE_PLANNER
	m_clrPlannerWork = RGB(205, 230, 247);
	m_clrPlannerHeader = RGB(225, 242, 250);
	m_clrPlannerTodayFill = RGB(153, 200, 233);

	m_clrPalennerLine     = m_clrSeparator;
	m_clrPlannerTodayLine = m_clrPlannerTodayFill;
#endif

	m_clrNcTextActive   = RGB(68, 68, 68);
	m_clrNcTextInactive = RGB(177, 177, 177);
	m_clrNcBorder       = RGB(131, 131, 131);

	if (m_Style == Office2013_DarkGray)
	{
		m_clrNcTextInactive = RGB(128, 128, 128);
	}

	m_clrButtonsArea = CBCGPDrawManager::ColorMakeDarker(m_clrDlgBackground, .02);

	m_clrFace = m_clrButtonsArea;
	m_clrHighlightNC = m_clrHighlight;
	m_clrHighlightDnNC = m_clrHighlightDn;

	m_clrSliderTic = globalData.clrBarDkShadow;

	m_clrRibbonGalleryBorder = globalData.clrBarShadow;
	m_clrRibbonGalleryBorderActive = m_clrAccent;
}
//************************************************************************************
COLORREF CBCGPVisualManager2013::OnDrawMenuLabel (CDC* pDC, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnDrawMenuLabel(pDC, rect);
	}

	ASSERT_VALID(pDC);

	CBrush brFill(globalData.clrBarShadow);
	pDC->FillRect(rect, &brFill);

	return globalData.clrBarText;
}
//**************************************************************************************
void CBCGPVisualManager2013::OnDrawMenuImageRectBorder (CDC* pDC, CBCGPToolbarButton* pButton, CRect rect, CBCGPVisualManager::BCGBUTTON_STATE state)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawMenuImageRectBorder(pDC, pButton, rect, state);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

	if ((pButton->m_nStyle & TBBS_CHECKED) && (pButton->GetImage() >= 0 || pButton->IsRibbonImage() || pButton->IsCustomImage()))
	{
		if (!CBCGPToolBar::IsCustomizeMode () && (pButton->m_nStyle & TBBS_DISABLED))
		{
			pDC->Draw3dRect(rect, globalData.clrBarShadow, globalData.clrBarShadow);
		}
		else
		{
			pDC->Draw3dRect(rect, m_clrHighlightDn, m_clrHighlightDn);
		}
	}
}
//****************************************************************************************
void CBCGPVisualManager2013::OnFillMenuImageRect (CDC* pDC, CBCGPToolbarButton* pButton, CRect rect, CBCGPVisualManager::BCGBUTTON_STATE state)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnFillMenuImageRect(pDC, pButton, rect, state);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

	if ((pButton->m_nStyle & TBBS_CHECKED) && (pButton->GetImage() >= 0 || pButton->IsRibbonImage() || pButton->IsCustomImage()))
	{
		pDC->FillRect(rect, &m_brHighlight);
	}
}
//*************************************************************************************
void CBCGPVisualManager2013::OnScrollBarDrawThumb (CDC* pDC, CBCGPScrollBar* /*pScrollBar*/, CRect rect, 
		BOOL /*bHorz*/, BOOL bHighlighted, BOOL bPressed, BOOL bDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return;
	}

	ASSERT_VALID(pDC);

	CBCGPDrawManager dm (*pDC);
	dm.DrawRect (rect, 
		bPressed || bDisabled ? globalData.clrBarShadow : globalData.clrBarLight, 
		bHighlighted ? globalData.clrBarDkShadow : (m_Style == Office2013_White ? m_clrSeparator : m_clrMenuBorder));
}
//*************************************************************************************
void CBCGPVisualManager2013::OnScrollBarDrawButton (CDC* pDC, CBCGPScrollBar* pScrollBar, CRect rect, 
		BOOL bHorz, BOOL bHighlighted, BOOL bPressed, BOOL bFirst, BOOL bDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return;
	}

	COLORREF clrFill = bPressed || bDisabled ? globalData.clrBarShadow : globalData.clrBarLight;

	CBCGPDrawManager dm (*pDC);
	dm.DrawRect (rect, 
		clrFill, 
		bDisabled ? (COLORREF)-1 : bHighlighted ? globalData.clrBarDkShadow : (m_Style == Office2013_White ? m_clrSeparator : m_clrMenuBorder));

	CBCGPMenuImages::IMAGES_IDS ids;
	if (bHorz)
	{
		if (pScrollBar->GetSafeHwnd() != NULL && (pScrollBar->GetExStyle() & WS_EX_LAYOUTRTL))
		{
			bFirst = !bFirst;
		}
		
		ids = bFirst ? CBCGPMenuImages::IdArowLeftTab3d : CBCGPMenuImages::IdArowRightTab3d;
	}
	else
	{
		ids = bFirst ? CBCGPMenuImages::IdArowUpLarge : CBCGPMenuImages::IdArowDownLarge;
	}

	CBCGPMenuImages::IMAGE_STATE state = bDisabled ? CBCGPMenuImages::ImageLtGray : CBCGPMenuImages::GetStateByColor(clrFill);

	CBCGPMenuImages::Draw (pDC, ids, rect, state);
}
//*************************************************************************************
void CBCGPVisualManager2013::OnScrollBarFillBackground (CDC* pDC, CBCGPScrollBar* /*pScrollBar*/, CRect rect, 
		BOOL /*bHorz*/, BOOL /*bHighlighted*/, BOOL /*bPressed*/, BOOL /*bFirst*/, BOOL /*bDisabled*/)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return;
	}

	CBCGPDrawManager dm (*pDC);
	dm.DrawRect (rect, globalData.clrBarShadow, (COLORREF)-1);
}
//*********************************************************************************************************
COLORREF CBCGPVisualManager2013::OnDrawControlBarCaption (CDC* pDC, CBCGPDockingControlBar* pBar, 
			BOOL bActive, CRect rectCaption, CRect rectButtons)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnDrawControlBarCaption (pDC, pBar, bActive, rectCaption, rectButtons);
	}

	ASSERT_VALID (pDC);

	rectCaption.bottom++;

	pDC->FillRect(rectCaption, &globalData.brBarFace);

	return bActive ? m_clrAccentText : globalData.clrBarText;
}
//****************************************************************************************
void CBCGPVisualManager2013::OnDrawControlBarCaptionText (CDC* pDC, CBCGPDockingControlBar* pBar, BOOL bActive, const CString& strTitle, CRect& rectCaption)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawControlBarCaptionText(pDC, pBar, bActive, strTitle, rectCaption);
		return;
	}

	ASSERT_VALID (pDC);

	CRect rectText = rectCaption;
	pDC->DrawText (strTitle, rectText, DT_LEFT | DT_SINGLELINE | DT_VCENTER | DT_END_ELLIPSIS);
}
//*********************************************************************************************************
void CBCGPVisualManager2013::OnDrawBarGripper (CDC* pDC, CRect rectGripper, BOOL bHorz, CBCGPBaseControlBar* pBar)
{
	CBCGPVisualManagerVS2010::OnDrawBarGripper(pDC, rectGripper, bHorz, pBar);
}
//**************************************************************************************
void CBCGPVisualManager2013::OnDrawCaptionButton (CDC* pDC, CBCGPCaptionButton* pButton, 
								BOOL bActive, BOOL bHorz, BOOL bMaximized, BOOL bDisabled,
								int nImageID /*= -1*/)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawCaptionButton (pDC, pButton, bActive, bHorz, bMaximized, bDisabled, nImageID);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

    CRect rc = pButton->GetRect ();

	const BOOL bHighlight = (pButton->m_bFocused || pButton->m_bDroppedDown) && !bDisabled;
	
	if (pButton->m_bPushed && bHighlight)
	{
		pDC->FillRect (rc, IsDarkTheme() ? &m_brHighlightDnNC : &m_brHighlightDn);
	}
	else if (bHighlight || pButton->m_bPushed)
	{
		pDC->FillRect (rc, IsDarkTheme() ? &m_brHighlightNC : &m_brHighlight);
	}
	
	CBCGPMenuImages::IMAGES_IDS id = (CBCGPMenuImages::IMAGES_IDS)-1;
	
	if (nImageID != -1)
	{
		id = (CBCGPMenuImages::IMAGES_IDS)nImageID;
	}
	else
	{
		id = pButton->GetIconID (bHorz, bMaximized);
	}
	
	if (id != (CBCGPMenuImages::IMAGES_IDS)-1)
	{
		CSize sizeImage = CBCGPMenuImages::Size ();
		CPoint ptImage (rc.left + (rc.Width () - sizeImage.cx) / 2,
			rc.top + (rc.Height () - sizeImage.cy) / 2);
		
		OnDrawCaptionButtonIcon (pDC, pButton, id, bActive, bDisabled, ptImage);
	}
}
//**********************************************************************************
void CBCGPVisualManager2013::OnDrawCaptionButtonIcon (CDC* pDC, 
													CBCGPCaptionButton* pButton,
													CBCGPMenuImages::IMAGES_IDS id,
													BOOL bActive, BOOL bDisabled,
													CPoint ptImage)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawCaptionButtonIcon (pDC, pButton, id, bActive, bDisabled, ptImage);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

	CBCGPMenuImages::IMAGE_STATE imageState = CBCGPMenuImages::ImageBlack;
	
	if (bDisabled)
	{
		imageState = CBCGPMenuImages::ImageGray;
	}
	else if (pButton->m_bFocused || pButton->m_bPushed || pButton->m_bDroppedDown)
	{
		imageState = CBCGPMenuImages::ImageBlack2;
	}

	CBCGPMenuImages::Draw (pDC, id, ptImage, imageState);
}
//*****************************************************************************
COLORREF CBCGPVisualManager2013::GetHighlightedMenuItemTextColor (CBCGPToolbarMenuButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManager2003::GetHighlightedMenuItemTextColor (pButton);
	}

	return globalData.clrBarText;
}
//**************************************************************************************
COLORREF CBCGPVisualManager2013::GetToolbarButtonTextColor (CBCGPToolbarButton* pButton, CBCGPVisualManager::BCGBUTTON_STATE state)
{
	ASSERT_VALID (pButton);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::GetToolbarButtonTextColor (pButton, state);
	}

	if (pButton->GetParentWnd() != NULL && state == ButtonsIsRegular)
	{
		if (DYNAMIC_DOWNCAST (CBCGPReBar, pButton->GetParentWnd()) != NULL || DYNAMIC_DOWNCAST (CBCGPReBar, pButton->GetParentWnd()->GetParent ()))
		{
			return CBCGPVisualManagerVS2010::GetToolbarButtonTextColor (pButton, state);
		}
	}

	BOOL bDisabled = (CBCGPToolBar::IsCustomizeMode () && !pButton->IsEditable ()) ||
			(!CBCGPToolBar::IsCustomizeMode () && (pButton->m_nStyle & TBBS_DISABLED));

	CBCGPControlBar* pWnd = DYNAMIC_DOWNCAST(CBCGPControlBar, pButton->GetParentWnd());
	if (pWnd != NULL && pWnd->IsDialogControl())
	{
		return bDisabled ? globalData.clrGrayedText : globalData.clrBtnText;
	}

	return bDisabled ? m_clrTextDisabled : globalData.clrBarText;
}
//*****************************************************************************
int CBCGPVisualManager2013::GetMenuDownArrowState (CBCGPToolbarMenuButton* pButton, BOOL bHightlight, BOOL bPressed, BOOL bDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::GetMenuDownArrowState (pButton, bHightlight, bPressed, bDisabled);
	}

	CBCGPWnd* pWnd = DYNAMIC_DOWNCAST(CBCGPWnd, pButton->GetParentWnd());

	if (pWnd != NULL && pWnd->IsOnGlass())
	{
		return CBCGPVisualManagerVS2010::GetMenuDownArrowState (pButton, bHightlight, bPressed, bDisabled);
	}

	if (bDisabled)
	{
		return (int)CBCGPMenuImages::ImageLtGray;
	}

	return (int)CBCGPMenuImages::ImageBlack;
}
//*********************************************************************************************************
void CBCGPVisualManager2013::OnHighlightRarelyUsedMenuItems (CDC* pDC, CRect rectRarelyUsed)
{
	ASSERT_VALID (pDC);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2010::OnHighlightRarelyUsedMenuItems (pDC, rectRarelyUsed);
		return;
	}

	rectRarelyUsed.left --;
	rectRarelyUsed.right = rectRarelyUsed.left + CBCGPToolBar::GetMenuImageSize ().cx + 
		2 * GetMenuImageMargin () + 2;

	pDC->FillSolidRect(rectRarelyUsed, globalData.clrBarShadow);
}
//************************************************************************************
COLORREF CBCGPVisualManager2013::OnFillMiniFrameCaption (CDC* pDC, 
								CRect rectCaption, 
								CBCGPMiniFrameWnd* pFrameWnd,
								BOOL bActive)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pFrameWnd);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnFillMiniFrameCaption (pDC, rectCaption, pFrameWnd, bActive);
	}

	if (DYNAMIC_DOWNCAST (CBCGPBaseToolBar, pFrameWnd->GetControlBar ()) != NULL)
	{
		bActive = FALSE;
	}

	pDC->FillRect(rectCaption, &globalData.brBarFace);

	return bActive ? m_clrAccentText : globalData.clrBarText;
}
//************************************************************************************
void CBCGPVisualManager2013::OnDrawMiniFrameBorder (
										CDC* pDC, CBCGPMiniFrameWnd* pFrameWnd,
										CRect rectBorder, CRect rectBorderSize)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawMiniFrameBorder (pDC, pFrameWnd, rectBorder, rectBorderSize);
		return;
	}

	ASSERT_VALID (pDC);

	CRect rectClip = rectBorder;
	rectClip.DeflateRect(rectClip);

	pDC->ExcludeClipRect(rectClip);

	pDC->FillRect(rectBorder, &globalData.brBarFace);

	if (pFrameWnd->GetSafeHwnd() != NULL && pFrameWnd->IsActive())
	{
		pDC->Draw3dRect(rectBorder, m_clrAccent, m_clrAccent);
	}
	else
	{
		pDC->Draw3dRect(rectBorder, m_clrMenuBorder, m_clrMenuBorder);
	}

	pDC->SelectClipRgn (NULL);
}
//*******************************************************************************
COLORREF CBCGPVisualManager2013::OnFillListBox(CDC* pDC, CBCGPListBox* pListBox, CRect rectClient)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnFillListBox(pDC, pListBox, rectClient);
	}

	ASSERT_VALID (pDC);

	pDC->FillRect(rectClient, &m_brControl);
	return globalData.clrBarText;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2013::OnFillListBoxItem (CDC* pDC, CBCGPListBox* pListBox, int nItem, CRect rect, BOOL bIsHighlihted, BOOL bIsSelected)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnFillListBoxItem (pDC, pListBox, nItem, rect, bIsHighlihted, bIsSelected);
	}

	ASSERT_VALID (pDC);

	BOOL bIsEnabled = pListBox->IsEnabled(nItem);

	if (bIsHighlihted)
	{
		if (bIsEnabled)
		{
			pDC->FillRect(rect, bIsSelected ? &m_brHighlightChecked : &m_brHighlight);
		}
		else
		{
			pDC->FillSolidRect(rect, globalData.clrBarShadow);
		}
	}
	else if (bIsSelected)
	{
		if (bIsEnabled)
		{
			pDC->FillRect(rect, &m_brHighlightDn);
		}
		else
		{
			pDC->FillSolidRect(rect, globalData.clrBarShadow);
		}
	}

	return (COLORREF)-1;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2013::GetListBoxCaptionTextColor(CBCGPListBox* pListBox, int nIndex)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || !pListBox->IsBackstagePageSelector())
	{
		return CBCGPVisualManagerVS2012::GetListBoxCaptionTextColor(pListBox, nIndex);
	}
	
#ifndef BCGP_EXCLUDE_RIBBON
	return GetRibbonBackstageInfoTextColor();
#else
	return CBCGPVisualManagerVS2012::GetListBoxCaptionTextColor(pListBox, nIndex);
#endif
}
//*******************************************************************************
int CBCGPVisualManager2013::GetListBoxItemExtraHeight(CBCGPListBox* pListBox)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || !pListBox->IsBackstagePageSelector())
	{
		return CBCGPVisualManagerVS2012::GetListBoxItemExtraHeight(pListBox);
	}

	return globalUtils.ScaleByDPI(20);
}
//*******************************************************************************
BOOL CBCGPVisualManager2013::IsUnderlineListBoxCaption(CBCGPListBox* pListBox)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || !pListBox->IsBackstagePageSelector())
	{
		return CBCGPVisualManagerVS2012::IsUnderlineListBoxCaption(pListBox);
	}

	return FALSE;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2013::OnFillComboBoxItem (CDC* pDC, CBCGPComboBox* pComboBox, int nItem, CRect rect, BOOL bIsHighlihted, BOOL bIsSelected)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnFillComboBoxItem (pDC, pComboBox, nItem, rect, bIsHighlihted, bIsSelected);
	}

	ASSERT_VALID (pDC);

	if (!bIsHighlihted && !bIsSelected)
	{
		pDC->FillRect(rect, &m_brControl);
		return globalData.clrBarText;
	}

	COLORREF clrText = (COLORREF)-1;

	if (bIsHighlihted)
	{
		pDC->FillRect(rect, &m_brHighlight);
	}
	else if (bIsSelected)
	{
		pDC->FillRect(rect, &m_brHighlightDn);
	}

	return clrText;
}
//*************************************************************************************
void CBCGPVisualManager2013::OnDrawComboBorder (CDC* pDC, CRect rect,
												BOOL bDisabled,
												BOOL bIsDropped,
												BOOL bIsHighlighted,
												CBCGPToolbarComboBoxButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawComboBorder (pDC, rect, bDisabled, bIsDropped, bIsHighlighted, pButton);
		return;
	}
	
	rect.DeflateRect (1, 1);
	
	COLORREF clrBorder = bDisabled ? m_clrEditBoxBorderDisabled : m_clrEditBoxBorder;
	
	if (CBCGPToolBarImages::m_bIsDrawOnGlass)
	{
		CBCGPDrawManager dm (*pDC);
		dm.DrawRect (rect, (COLORREF)-1, clrBorder);
	}
	else
	{
		pDC->Draw3dRect (&rect,  clrBorder, clrBorder);
	}
}
//*****************************************************************************
void CBCGPVisualManager2013::OnDrawComboDropButton (CDC* pDC, CRect rect,
												BOOL bDisabled,
												BOOL bIsDropped,
												BOOL bIsHighlighted,
												CBCGPToolbarComboBoxButton* pButton)
{
	ASSERT_VALID(pDC);
	ASSERT_VALID (this);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || CBCGPToolBarImages::m_bIsDrawOnGlass || bDisabled)
	{
		CBCGPVisualManagerVS2012::OnDrawComboDropButton (pDC, rect, bDisabled, bIsDropped, bIsHighlighted, pButton);
		return;
	}

	COLORREF clrFill = m_clrCombo;

	if (bIsDropped)
	{
		pDC->FillRect(rect, &m_brHighlightDn);
		clrFill = m_clrHighlightDn;
	}
	else if (bIsHighlighted)
	{
		pDC->FillSolidRect(rect, m_clrHighlight);
		clrFill = m_clrHighlight;
	}
	else
	{
		pDC->FillSolidRect (rect, m_clrCombo);
		clrFill = m_clrCombo;
	}

	CBCGPMenuImages::DrawByColor(pDC, CBCGPMenuImages::IdArowDown, rect, clrFill);
}
//*************************************************************************************
void CBCGPVisualManager2013::OnDrawEditBorder (CDC* pDC, CRect rect,
												BOOL bDisabled,
												BOOL bIsHighlighted,
												CBCGPToolbarEditBoxButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawEditBorder (pDC, rect, bDisabled, bIsHighlighted, pButton);
		return;
	}
	
	COLORREF clrBorder = bDisabled ? m_clrEditBoxBorderDisabled : m_clrEditBoxBorder;
	
	if (CBCGPToolBarImages::m_bIsDrawOnGlass)
	{
		CBCGPDrawManager dm (*pDC);
		dm.DrawRect (rect, (COLORREF)-1, clrBorder);
	}
	else
	{
		pDC->Draw3dRect (&rect,  clrBorder, clrBorder);
	}
}
//*************************************************************************************
void CBCGPVisualManager2013::OnDrawButtonBorder (CDC* pDC, CBCGPToolbarButton* pButton, CRect rect, BCGBUTTON_STATE state)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawButtonBorder (pDC, pButton, rect, state);
		return;
	}

	CBCGPControlBar* pWnd = DYNAMIC_DOWNCAST(CBCGPControlBar, pButton->GetParentWnd());
	if (pWnd != NULL && pWnd->IsDialogControl())
	{
		CBCGPVisualManagerVS2012::OnDrawButtonBorder (pDC, pButton, rect, state);
		return;
	}

	if (pButton->m_nStyle & TBBS_CHECKED)
	{
		COLORREF clrBorder = (!CBCGPToolBar::IsCustomizeMode () && (pButton->m_nStyle & TBBS_DISABLED)) ? globalData.clrBarShadow : m_clrHighlightChecked;
		rect.DeflateRect(1, 1);
		
		if (CBCGPToolBarImages::m_bIsDrawOnGlass)
		{
			CBCGPDrawManager dm (*pDC);
			
			dm.DrawRect (rect, (COLORREF)-1, clrBorder);
		}
		else
		{
			pDC->Draw3dRect(rect, clrBorder, clrBorder);
		}
	}
}
//***********************************************************************************
void CBCGPVisualManager2013::OnFillHighlightedArea (CDC* pDC, CRect rect, CBrush* pBrush, CBCGPToolbarButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || pButton == NULL)
	{
		CBCGPVisualManagerVS2012::OnFillHighlightedArea (pDC, rect, pBrush, pButton);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pBrush);
	ASSERT_VALID (pButton);

	BOOL bIsPopupMenu = pButton->GetParentWnd () != NULL &&
		pButton->GetParentWnd ()->IsKindOf (RUNTIME_CLASS (CBCGPPopupMenuBar));

	if (bIsPopupMenu)
	{
		CBCGPVisualManagerVS2012::OnFillHighlightedArea (pDC, rect, pBrush, pButton);
		return;
	}

	if ((pButton->m_nStyle & TBBS_CHECKED))
	{
		rect.DeflateRect(1, 1);
	}

	CBCGPDrawManager dm (*pDC);

	if (pBrush == &m_brHighlight)
	{
		dm.DrawRect(rect, m_clrHighlight, (COLORREF)-1);
		return;
	}
	else if (pBrush == &m_brHighlightDn)
	{
		dm.DrawRect(rect, m_clrHighlightDn, (COLORREF)-1);
		return;
	}
	else if (pBrush == &m_brHighlightChecked)
	{
		dm.DrawRect (rect, m_clrHighlightChecked, (COLORREF)-1);
		return;
	}
}
//*********************************************************************************************************
void CBCGPVisualManager2013::OnDrawTab (CDC* pDC, CRect rectTab,
						int iTab, BOOL bIsActive, const CBCGPBaseTabWnd* pTabWnd)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || !pTabWnd->Is3DStyle())
	{
		CBCGPVisualManagerVS2012::OnDrawTab (pDC, rectTab, iTab, bIsActive, pTabWnd);
		return;
	}

	const BOOL bIsHighlight = iTab == pTabWnd->GetHighlightedTab ();

	if (bIsActive || bIsHighlight)
	{
		CRect rectFill = rectTab;

		if (bIsActive)
		{
			if (pTabWnd->GetLocation () == CBCGPBaseTabWnd::LOCATION_BOTTOM)
			{
				rectFill.top--;
				rectTab.top--;
			}
			else
			{
				rectFill.bottom++;
				rectTab.bottom++;
			}
		}
		else
		{
			rectFill.DeflateRect(0, 1);
		}

		if (bIsActive)
		{
			pDC->FillSolidRect(rectFill, globalData.clrBarHilite);
		}
		else
		{
			pDC->FillRect (rectFill, &m_brHighlight);
		}
	}

	CRect rectTabFrame = rectTab;

	COLORREF clrBkTab = pTabWnd->GetTabBkColor(iTab);
	if (clrBkTab != (COLORREF)-1)
	{
		CRect rectColor = rectTab;
		const int nColorBarHeight = max(3, rectTab.Height() / 6);

		if (pTabWnd->GetLocation() == CBCGPBaseTabWnd::LOCATION_BOTTOM)
		{
			rectColor.top = rectColor.bottom - nColorBarHeight;
			rectTab.bottom -= nColorBarHeight;
		}
		else
		{
			rectColor.bottom = rectColor.top + nColorBarHeight;
			rectTab.top += nColorBarHeight;
		}

		rectColor.DeflateRect(bIsActive ? 1 : 2, 0);

		CBrush br(clrBkTab);
		pDC->FillRect(rectColor, &br);
	}

	if (bIsActive)
	{
		CBCGPPenSelector ps(*pDC, globalData.clrBarDkShadow);
		
		if (pTabWnd->GetLocation () == CBCGPBaseTabWnd::LOCATION_BOTTOM)
		{
			pDC->MoveTo (rectTabFrame.left, rectTabFrame.top);
			pDC->LineTo (rectTabFrame.left, rectTabFrame.bottom);
			pDC->LineTo (rectTabFrame.right - 1, rectTabFrame.bottom);
			pDC->LineTo (rectTabFrame.right - 1, rectTabFrame.top);
		}
		else
		{
			pDC->MoveTo (rectTabFrame.left, rectTabFrame.bottom - 1);
			pDC->LineTo (rectTabFrame.left, rectTabFrame.top);
			pDC->LineTo (rectTabFrame.right - 1, rectTabFrame.top);
			pDC->LineTo (rectTabFrame.right - 1, rectTabFrame.bottom);
		}
	}

	COLORREF clrTabText = pTabWnd->GetTabTextColor(iTab);
	if (clrTabText == (COLORREF)-1)
	{
		BOOL bIsActiveInMDITabGroup = pTabWnd->IsActiveInMDITabGroup();
		if (pTabWnd->IsMultipleSelection() && pTabWnd->IsMultipleSelectionSupported() && bIsActive)
		{
			if (pTabWnd->GetActiveTab() != iTab)
			{
				// Tab is selected
				bIsActiveInMDITabGroup = FALSE;
			}
		}

		BOOL bFocused = pTabWnd->IsMDIFocused() && bIsActiveInMDITabGroup;
		clrTabText = ((bIsActive && bFocused) || bIsHighlight) ? m_clrAccentText : globalData.clrBarText;
	}

	OnDrawTabContent(pDC, rectTab, iTab, bIsActive, pTabWnd, clrTabText);
}
//*********************************************************************************************************
void CBCGPVisualManager2013::GetTabFrameColors (const CBCGPBaseTabWnd* pTabWnd,
				   COLORREF& clrDark,
				   COLORREF& clrBlack,
				   COLORREF& clrHighlight,
				   COLORREF& clrFace,
				   COLORREF& clrDarkShadow,
				   COLORREF& clrLight,
				   CBrush*& pbrFace,
				   CBrush*& pbrBlack)
{
	ASSERT_VALID (pTabWnd);
	
	CBCGPVisualManagerVS2012::GetTabFrameColors (pTabWnd,
			   clrDark, clrBlack,
			   clrHighlight, clrFace,
			   clrDarkShadow, clrLight,
			   pbrFace, pbrBlack);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || 
		(pTabWnd->IsDialogControl() && !pTabWnd->IsPropertySheetTab()))
	{
		return;
	}

	clrDarkShadow = clrHighlight = clrLight = clrBlack = clrDark = IsDarkTheme() ? m_clrHighlightDn : globalData.clrBarDkShadow;
}
//**********************************************************************************
COLORREF CBCGPVisualManager2013::GetActiveTabBackColor(const CBCGPBaseTabWnd* pTabWnd) const
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || 
		(pTabWnd->IsDialogControl() && !pTabWnd->IsPropertySheetTab()))
	{
		return CBCGPVisualManagerVS2012::GetActiveTabBackColor(pTabWnd);
	}

	return IsDarkTheme() ? m_clrHighlightDn : globalData.clrBarFace;
}
//**********************************************************************************
void CBCGPVisualManager2013::OnDrawTabContent (CDC* pDC, CRect rectTab,
							int iTab, BOOL bIsActive, const CBCGPBaseTabWnd* pTabWnd,
							COLORREF clrText)
{
	BOOL bIsActiveInMDITabGroup = pTabWnd->IsActiveInMDITabGroup();
	if (pTabWnd->IsMultipleSelection() && pTabWnd->IsMultipleSelectionSupported() && bIsActive)
	{
		if (pTabWnd->GetActiveTab() != iTab)
		{
			// Tab is selected
			bIsActiveInMDITabGroup = FALSE;
		}
	}

	BOOL bFocused = pTabWnd->IsMDIFocused() && bIsActiveInMDITabGroup;

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () ||
		(pTabWnd->IsDialogControl () && !pTabWnd->IsPropertySheetTab()) ||
		(pTabWnd->GetTabBkColor(iTab) != (COLORREF)-1 && !pTabWnd->Is3DStyle()) ||
		pTabWnd->GetTabTextColor(iTab) != (COLORREF)-1)
	{
		if (pTabWnd->IsPointerStyle() && ((bIsActive && bFocused) || pTabWnd->GetHighlightedTab() == iTab))
		{
			clrText = globalData.IsHighContastMode () ? globalData.clrBarText : m_clrAccentText;
		}
	}
	else
	{
		clrText = (bIsActive && bFocused) || (pTabWnd->IsPointerStyle() && (pTabWnd->GetHighlightedTab() == iTab || bIsActive)) ? m_clrAccentText : globalData.clrBarText;
	}

	CBCGPVisualManagerXP::OnDrawTabContent (pDC, rectTab, iTab, bIsActive, pTabWnd, clrText);
}
//*********************************************************************************************************
void CBCGPVisualManager2013::OnDrawTabBorder(CDC* pDC, CBCGPTabWnd* pTabWnd, CRect rectBorder, COLORREF clrBorder,
										 CPen& penLine)
{
	CBCGPVisualManagerXP::OnDrawTabBorder(pDC, pTabWnd, rectBorder, clrBorder, penLine);
}
//**********************************************************************************
BOOL CBCGPVisualManager2013::OnEraseTabsFrame (CDC* pDC, CRect rect, const CBCGPBaseTabWnd* pTabWnd)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pTabWnd);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || (!pTabWnd->Is3DStyle() && !pTabWnd->IsVS2005Style()))
	{
		return CBCGPVisualManagerVS2012::OnEraseTabsFrame (pDC, rect, pTabWnd);
	}

	COLORREF clrActiveTab = pTabWnd->GetTabBkColor (pTabWnd->GetActiveTab ());
	CBrush br(clrActiveTab == (COLORREF)-1 ? globalData.clrBarHilite : clrActiveTab);
	pDC->FillRect(rect, &br);

	return TRUE;
}
//**********************************************************************************
void CBCGPVisualManager2013::OnDrawTabButton (CDC* pDC, CRect rect, 
											   const CBCGPBaseTabWnd* pTabWnd,
											   int nID,
											   BOOL bIsHighlighted,
											   BOOL bIsPressed,
											   BOOL bIsDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || pTabWnd->IsDialogControl ())
	{
		CBCGPVisualManagerXP::OnDrawTabButton (pDC, rect, pTabWnd, nID, bIsHighlighted, bIsPressed, bIsDisabled);
		return;
	}

	rect.DeflateRect(1, 1);

	CBCGPMenuImages::IMAGE_STATE state = CBCGPMenuImages::ImageBlack;

	if (bIsHighlighted && bIsPressed)
	{
		if (pTabWnd->GetHighlightedTab() == m_nCurrDrawedTab && pTabWnd->GetActiveTab() != m_nCurrDrawedTab)
		{
			pDC->FillRect (rect, &m_brHighlightChecked);
		}
		else
		{
			pDC->FillRect (rect, &m_brHighlightDn);
		}

		state = CBCGPMenuImages::ImageBlack2;
	}
	else if (bIsHighlighted)
	{
		if (pTabWnd->GetHighlightedTab() == m_nCurrDrawedTab && pTabWnd->GetActiveTab() != m_nCurrDrawedTab)
		{
			pDC->FillRect (rect, &m_brHighlightDn);
		}
		else
		{
			pDC->FillRect (rect, &m_brHighlight);
		}

		state = CBCGPMenuImages::ImageBlack2;
	}
	else
	{
		if (IsDarkTheme() && pTabWnd->IsPointerStyle() && !pTabWnd->IsMDITab())
		{
			state = CBCGPMenuImages::ImageLtGray;
		}
	}

	CBCGPMenuImages::Draw (pDC, (CBCGPMenuImages::IMAGES_IDS)nID, rect, state);
}
//*********************************************************************************************************
int CBCGPVisualManager2013::GetTabButtonState (CBCGPTabWnd* pTab, CBCGTabButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || pButton == NULL)
	{
		return CBCGPVisualManagerVS2012::GetTabButtonState (pTab, pButton);
	}

	return (int)((pButton->IsPressed() || pButton->IsHighlighted()) ? CBCGPMenuImages::ImageBlack2 : CBCGPMenuImages::ImageBlack);
}
//**********************************************************************************
BOOL CBCGPVisualManager2013::UseLargeCaptionFontInDockingCaptions() 
{ 
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerXP::UseLargeCaptionFontInDockingCaptions();
	}

	return TRUE; 
}
//**********************************************************************************
#ifndef BCGP_EXCLUDE_GRID_CTRL

void CBCGPVisualManager2013::OnFillGridHeaderBackground (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect)
{
	ASSERT_VALID (pDC);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerXP::OnFillGridHeaderBackground (pCtrl, pDC, rect);
		return;
	}

	pDC->FillRect(rect, &globalData.brBarFace);
}
//********************************************************************************
BOOL CBCGPVisualManager2013::OnDrawGridHeaderItemBorder (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect, BOOL bPressed)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnDrawGridHeaderItemBorder (pCtrl, pDC, rect, bPressed);
	}

	if (bPressed)
	{
		pDC->FillRect(rect, &m_brHighlightDn);
	}
	else if (pCtrl != NULL && pCtrl->IsGridHeaderItemHovered())
	{
		pDC->FillRect(rect, &m_brHighlight);
	}

	BOOL bIsVertLineReady = FALSE;

	if (pCtrl->GetSafeHwnd() != NULL)
	{
		CRect rectGrid;
		pCtrl->GetClientRect(rectGrid);

		if (rect.top - rectGrid.top > rect.Height() / 2)
		{
			CBCGPPenSelector pen(*pDC, &m_penSeparator);

			pDC->MoveTo(rect.right - 1, rect.top);
			pDC->LineTo(rect.right - 1, rect.bottom);

			bIsVertLineReady = TRUE;
		}

		if (pCtrl->IsGroupByBox())
		{
			CBCGPPenSelector pen(*pDC, &m_penSeparator);

			pDC->MoveTo(rect.left, rect.top);
			pDC->LineTo(rect.right - 1, rect.top);
		}
	}

	if (!bIsVertLineReady)
	{
		CRect rectVertLine = rect;
		rectVertLine.left = rectVertLine.right - 1;
		rectVertLine.bottom -= 2;
	
		CBCGPDrawManager dm(*pDC);
		dm.FillGradient(rectVertLine, m_clrSeparator, globalData.clrBarFace);
	}

	CBCGPPenSelector pen(*pDC, RGB(171, 171, 171));

	pDC->MoveTo(rect.left, rect.bottom - 1);
	pDC->LineTo(rect.right, rect.bottom - 1);

	return FALSE;
}
//********************************************************************************
void CBCGPVisualManager2013::OnFillGridRowHeaderBackground (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect)
{
	ASSERT_VALID (pDC);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerXP::OnFillGridRowHeaderBackground (pCtrl, pDC, rect);
		return;
	}

	pDC->FillRect(rect, &globalData.brBarFace);
}
//********************************************************************************
void CBCGPVisualManager2013::OnFillGridSelectAllAreaBackground (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect, BOOL bPressed)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pCtrl);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerXP::OnFillGridSelectAllAreaBackground (pCtrl, pDC, rect, bPressed);
		return;
	}

	pDC->FillRect(rect, &globalData.brBarFace);

	rect.DeflateRect (5, 5);
	int nMin = min (rect.Width (), rect.Height ());
	nMin = min (nMin, pCtrl->GetButtonWidth () - 5);
	rect.left = rect.right  - nMin;
	rect.top  = rect.bottom - nMin;

	POINT ptRgn [] =
	{
		{rect.right, rect.top},
		{rect.right, rect.bottom},
		{rect.left, rect.bottom}
	};

	CRgn rgn;
	rgn.CreatePolygonRgn (ptRgn, 3, WINDING);

	pDC->SelectClipRgn (&rgn, RGN_COPY);

	CBrush brFill (m_clrSeparator);
	pDC->FillRect (rect, &brFill);

	pDC->SelectClipRgn (NULL, RGN_COPY);
}
//********************************************************************************
COLORREF CBCGPVisualManager2013::OnFillGridGroupByBoxBackground (CDC* pDC, CRect rect)
{
	ASSERT_VALID (pDC);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerXP::OnFillGridGroupByBoxBackground (pDC, rect);
	}

	pDC->FillRect(rect, &globalData.brBarFace);
	return globalData.clrBarText;
}
//********************************************************************************
COLORREF CBCGPVisualManager2013::OnFillGridGroupByBoxTitleBackground (CDC* pDC, CRect rect)
{
	ASSERT_VALID (pDC);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerXP::OnFillGridGroupByBoxTitleBackground (pDC, rect);
	}

	pDC->FillRect(rect, &globalData.brBarFace);
	return globalData.clrBarText;
}
//********************************************************************************
COLORREF CBCGPVisualManager2013::GetGridGroupByBoxLineColor () const
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerXP::GetGridGroupByBoxLineColor ();
	}

	return globalData.clrBarDkShadow;
}
//********************************************************************************
void CBCGPVisualManager2013::OnDrawGridGroupByBoxItemBorder (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect)
{
	ASSERT_VALID (pDC);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerXP::OnDrawGridGroupByBoxItemBorder (pCtrl, pDC, rect);
		return;
	}

	pDC->FillRect(rect, &globalData.brBarFace);
	pDC->Draw3dRect (rect, globalData.clrBarDkShadow, globalData.clrBarDkShadow);
}
//********************************************************************************
COLORREF CBCGPVisualManager2013::GetGridLeftOffsetColor (CBCGPGridCtrl* pCtrl)
{
	return CBCGPVisualManagerXP::GetGridLeftOffsetColor (pCtrl);
}
//********************************************************************************
void CBCGPVisualManager2013::OnFillGridGroupBackground (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rectFill)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pCtrl);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerXP::OnFillGridGroupBackground (pCtrl, pDC, rectFill);
		return;
	}

	if (rectFill.Height () > pCtrl->GetBaseHeight ())
	{
		pDC->FillRect(rectFill, &globalData.brBarFace);
		rectFill.top ++;
	}

	CBrush brFill(m_clrSeparator);
	pDC->FillRect(rectFill, &brFill);
}
//********************************************************************************
BOOL CBCGPVisualManager2013::IsGridGroupUnderline () const
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerXP::IsGridGroupUnderline ();
	}

	return FALSE;
}
//********************************************************************************
void CBCGPVisualManager2013::OnDrawGridGroupUnderline (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rectFill)
{
	ASSERT_VALID (pDC);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerXP::OnDrawGridGroupUnderline (pCtrl, pDC, rectFill);
		return;
	}

	pDC->FillRect(rectFill, &globalData.brBarFace);
}
//********************************************************************************
COLORREF CBCGPVisualManager2013::OnFillGridRowBackground (CBCGPGridCtrl* pCtrl, 
												  CDC* pDC, CRect rectFill, BOOL bSelected)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pCtrl);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerXP::OnFillGridRowBackground (pCtrl, pDC, rectFill, bSelected);
	}

	// Fill area:
	if (bSelected)
	{
		if (!pCtrl->IsFocused ())
		{
			// no painting
		}
		else
		{
			pDC->FillRect (rectFill, &m_brAccentLight);
		}
	}
	else
	{
		CBrush brFill(globalData.clrBarShadow);
		pDC->FillRect (rectFill, &brFill);
	}

	// Return text color:
	if (!pCtrl->IsHighlightGroups () && bSelected)
	{
		if (!pCtrl->IsFocused ())
		{
			return m_clrAccentText;
		}
		else
		{
			COLORREF clrText;

			if (GetRValue (m_clrAccentLight) <= 128 ||
				GetGValue (m_clrAccentLight) <= 128 ||
				GetBValue (m_clrAccentLight) <= 128)
			{
				clrText = RGB(255, 255, 255);
			}
			else
			{
				clrText = RGB(0, 0, 0);
			}

			return clrText;
		}
	}

	return pCtrl->IsHighlightGroups () ? 
		(pCtrl->IsControlBarColors () ? globalData.clrBarShadow : globalData.clrBtnShadow) :
		globalData.clrWindowText;
}
//********************************************************************************
COLORREF CBCGPVisualManager2013::OnFillGridItem (CBCGPGridCtrl* pCtrl, 
											CDC* pDC, CRect rectFill,
											BOOL bSelected, BOOL bActiveItem, BOOL bSortedColumn)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pCtrl);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerXP::OnFillGridItem (pCtrl, pDC, rectFill, bSelected, bActiveItem, bSortedColumn);
	}

	if (bSelected && !bActiveItem)
	{
		if (!pCtrl->IsFocused ())
		{
			if (pCtrl->IsControlBarColors ())
			{
				pDC->FillRect (rectFill, &globalData.brBarFace);
			}
			else
			{
				pDC->FillRect (rectFill, &globalData.brBtnFace);
			}

			return m_clrAccentText;
		}
		else
		{
			pDC->FillRect (rectFill, &m_brAccentLight);

			COLORREF clrText;

			if (GetRValue (m_clrAccentLight) <= 128 ||
				GetGValue (m_clrAccentLight) <= 128 ||
				GetBValue (m_clrAccentLight) <= 128)
			{
				clrText = RGB(255, 255, 255);
			}
			else
			{
				clrText = RGB(0, 0, 0);
			}

			return clrText;
		}
	}

	if (bSelected && !pCtrl->IsFocused () && bActiveItem)
	{
		CBrush brFill(globalData.clrBarShadow);
		pDC->FillRect (rectFill, &brFill);
		return m_clrAccentText;
	}

	return CBCGPVisualManagerXP::OnFillGridItem(pCtrl, pDC, rectFill, bSelected, bActiveItem, bSortedColumn);
}
//********************************************************************************
BOOL CBCGPVisualManager2013::OnSetGridColorTheme (CBCGPGridCtrl* pCtrl, BCGP_GRID_COLOR_DATA& theme)
{
	BOOL bRes = CBCGPVisualManagerVS2012::OnSetGridColorTheme (pCtrl, theme);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return bRes;
	}

	if (m_Style != Office2013_White)
	{
		theme.m_OddColors.m_clrBackground = globalData.clrBarLight;
	}

	theme.m_HeaderSelColors.m_clrBorder = theme.m_LeftOffsetColors.m_clrBorder = theme.m_HeaderColors.m_clrBorder = RGB(171, 171, 171);
	theme.m_clrVertLine = theme.m_clrHorzLine = m_clrSeparator;
	theme.m_HeaderColors.m_clrBackground = (COLORREF)-1;

	return bRes;
}
//********************************************************************************
COLORREF CBCGPVisualManager2013::GetReportCtrlGroupBackgoundColor ()
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerXP::GetReportCtrlGroupBackgoundColor ();
	}

	return globalData.clrBarShadow;
}
//********************************************************************************
COLORREF CBCGPVisualManager2013::OnFillReportCtrlRowBackground (CBCGPGridCtrl* pCtrl, CDC* pDC,
		CRect rectFill, BOOL bSelected, BOOL bGroup)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pCtrl);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerXP::OnFillReportCtrlRowBackground (pCtrl, pDC, rectFill, bSelected, bGroup);
	}

	// Fill area:
	COLORREF clrText = (COLORREF)-1;

	clrText = RGB(255,0,0);

	if (bSelected)
	{
		if (!pCtrl->IsFocused ())
		{
			// no painting
			clrText = m_clrAccentText;
		}
		else
		{
			pDC->FillRect (rectFill, &m_brAccentLight);
			clrText = m_clrReportGroupText;
		}
	}
	else
	{
		if (bGroup)
		{
			// no painting
			clrText = m_clrReportGroupText;
		}
	}

	// Return text color:
	return clrText;
}
#endif // BCGP_EXCLUDE_GRID_CTRL

BOOL CBCGPVisualManager2013::GetToolTipParams (CBCGPToolTipParams& params, 
											   UINT /*nType*/ /*= (UINT)(-1)*/)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::GetToolTipParams (params);
	}
	
	params.m_bBoldLabel = TRUE;
	params.m_bDrawDescription = TRUE;
	params.m_bDrawIcon = TRUE;
	params.m_bRoundedCorners = FALSE;
	params.m_bDrawSeparator = FALSE;
	params.m_nPaddingX = 4;
	params.m_nPaddingY = 4;
	
	params.m_clrFill = globalData.clrBarHilite;
	params.m_clrFillGradient = (COLORREF)-1;
	params.m_clrText = m_clrRibbonTabs;
	params.m_clrBorder = globalData.clrBarShadow;
	
	return TRUE;
}

#ifndef BCGP_EXCLUDE_TOOLBOX

BOOL CBCGPVisualManager2013::OnEraseToolBoxButton (CDC* pDC, CRect rect,
											CBCGPToolBoxButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnEraseToolBoxButton(pDC, rect, pButton);
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

	CBCGPDrawManager dm (*pDC);

	if (pButton->GetCheck ())
	{
		dm.DrawRect (rect, m_clrHighlightChecked, (COLORREF)-1);
	}
	else if (pButton->IsHighlighted ())
	{
		dm.DrawRect(rect, m_clrHighlight, (COLORREF)-1);
	}

	return TRUE;
}
//**********************************************************************************
BOOL CBCGPVisualManager2013::OnDrawToolBoxButtonBorder (CDC* pDC, CRect& rect, 
												 CBCGPToolBoxButton* pButton, UINT uiState)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnDrawToolBoxButtonBorder(pDC, rect, pButton, uiState);
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

	if (pButton->GetCheck ())
	{
		CBCGPDrawManager dm (*pDC);
		dm.DrawRect(rect, (COLORREF)-1, m_clrHighlightDn);
	}

	return TRUE;
}
//**********************************************************************************
COLORREF CBCGPVisualManager2013::GetToolBoxButtonTextColor (CBCGPToolBoxButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::GetToolBoxButtonTextColor(pButton);
	}

	return globalData.clrBarText;
}

#endif	// BCGP_EXCLUDE_TOOLBOX

//*********************************************************************************************************
void CBCGPVisualManager2013::OnDrawAutohideBar(CDC* pDC, CRect rectBar, CBCGPAutoHideButton* pButton)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

	COLORREF clrFill = pButton->IsHighlighted() ? m_clrHighlightDn : m_clrSeparator;
	pDC->FillSolidRect(rectBar, clrFill);
}
//****************************************************************************************
void CBCGPVisualManager2013::OnFillOutlookPageButton (CDC* pDC, const CRect& rect,
										BOOL bIsHighlighted, BOOL bIsPressed,
										COLORREF& clrText)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnFillOutlookPageButton (pDC, rect,
										bIsHighlighted, bIsPressed,
										clrText);
		return;
	}

	ASSERT_VALID (pDC);

	clrText = globalData.clrBarText;

	CBCGPDrawManager dm (*pDC);

	if (bIsHighlighted)
	{
		dm.DrawRect(rect, m_clrHighlight, (COLORREF)-1);
	}
	else if (bIsPressed)
	{
		dm.DrawRect (rect, m_clrHighlightChecked, (COLORREF)-1);
	}
	else if (bIsHighlighted && bIsPressed)
	{
		dm.DrawRect(rect, m_clrHighlightDn, (COLORREF)-1);
	}
	else
	{
		dm.DrawRect (rect, globalData.clrBarLight, (COLORREF)-1);
	}
}
//****************************************************************************************
void CBCGPVisualManager2013::OnDrawOutlookPageButtonBorder (CDC* pDC, 
							CRect& rectBtn, BOOL bIsHighlighted, BOOL bIsPressed)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawOutlookPageButtonBorder (pDC, 
							rectBtn, bIsHighlighted, bIsPressed);
		return;
	}
}
//****************************************************************************************
void CBCGPVisualManager2013::OnFillOutlookBarCaption (CDC* pDC, CRect rectCaption, COLORREF& clrText)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnFillOutlookBarCaption (pDC, rectCaption, clrText);
		return;
	}

	pDC->FillRect(rectCaption, &globalData.brBarFace);
	clrText = globalData.clrBarText;
}


#ifndef BCGP_EXCLUDE_PLANNER
//****************************************************************************************
BOOL CBCGPVisualManager2013::CanDrawCaptionDayWithHeader() const
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::CanDrawCaptionDayWithHeader ();
	}

	return TRUE;
}
//****************************************************************************************
DWORD CBCGPVisualManager2013::GetPlannerDrawFlags () const
{
	DWORD dwFlags = CBCGPVisualManagerVS2012::GetPlannerDrawFlags ();

	if (globalData.m_nBitsPerPixel > 8 && !globalData.IsHighContastMode ())
	{
		dwFlags |= BCGP_PLANNER_DRAW_APP_NO_BORDER |
					BCGP_PLANNER_DRAW_APP_DURATION_WEEK_MONTH |
					BCGP_PLANNER_DRAW_VIEW_CAPTION_DAY_WITH_HEADER |
					BCGP_PLANNER_DRAW_VIEW_WEEK_BAR_FULL_WIDTH |
					BCGP_PLANNER_DRAW_VIEW_WEEK_BAR_FULL_HEIGHT;
	}

	return dwFlags;
}
//*******************************************************************************
void CBCGPVisualManager2013::GetPlannerAppointmentColors (CBCGPPlannerView* pView,
		BOOL bSelected, BOOL bSimple, DWORD dwDrawFlags, 
		COLORREF& clrBack1, COLORREF& clrBack2,
		COLORREF& clrFrame1, COLORREF& clrFrame2, COLORREF& clrText)
{
	CBCGPVisualManagerVS2012::GetPlannerAppointmentColors (pView,
		bSelected, bSimple, dwDrawFlags, clrBack1, clrBack2, clrFrame1, clrFrame2, clrText);

	if (globalData.m_nBitsPerPixel > 8 && !globalData.IsHighContastMode ())
	{
		COLORREF clrPlannerWork = m_clrPlannerWork;
		if (pView != NULL && pView->GetPlanner() != NULL && pView->GetPlanner()->GetBackgroundColor() != CLR_DEFAULT)
		{
			clrPlannerWork = pView->GetPlanner()->GetBackgroundColor();
		}

		clrFrame1 = GetPlannerTodayFill(clrPlannerWork);

		const BOOL bIsGradientFill = 
			dwDrawFlags & BCGP_PLANNER_DRAW_APP_GRADIENT_FILL &&
			!(globalData.m_bIsBlackHighContrast);
		if (bIsGradientFill)
		{
			clrBack2 = CBCGPDrawManager::PixelAlpha (RGB(255, 255, 255), clrBack1, 60);
		}
	}
}
//*******************************************************************************
COLORREF CBCGPVisualManager2013::GetPlannerAppointmentDurationColor(CBCGPPlannerView* pView, const CBCGPAppointment* pApp) const
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::GetPlannerAppointmentDurationColor(pView, pApp);
	}

	COLORREF clr = pApp->GetDurationColor();

	if (clr == CLR_DEFAULT)
	{
		COLORREF clrPlannerWork = m_clrPlannerWork;
		if (pView != NULL && pView->GetPlanner() != NULL && pView->GetPlanner()->GetBackgroundColor() != CLR_DEFAULT)
		{
			clrPlannerWork = pView->GetPlanner()->GetBackgroundColor();
		}

		clr = GetPlannerTodayFill(clrPlannerWork);
	}

	return clr;
}
//****************************************************************************************
UINT CBCGPVisualManager2013::GetPlannerWeekBarType() const
{
	return CBCGPPlannerView::BCGP_PLANNER_WEEKBAR_NUMBERS;
}
//****************************************************************************************
void CBCGPVisualManager2013::OnFillPlanner (CDC* pDC, CBCGPPlannerView* pView, 
		CRect rect, BOOL bWorkingArea)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pView);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnFillPlanner (pDC, pView, rect, bWorkingArea);
		return;
	}

	switch (pView->GetPlanner ()->GetType ())
	{
	case CBCGPPlannerManagerCtrl::BCGP_PLANNER_TYPE_DAY:
	case CBCGPPlannerManagerCtrl::BCGP_PLANNER_TYPE_WORK_WEEK:
		pDC->FillRect (rect, bWorkingArea ? &globalData.brWindow : &globalData.brBtnFace);
		break;

	default:
		if (m_bPlannerBackItemSelected)
		{
			CBrush br(GetPlannerSelectionColor(pView));
			pDC->FillRect (rect, &br);
		}
		else
		{
			pDC->FillRect (rect, &globalData.brWindow);
		}
	}
}
//****************************************************************************************
COLORREF CBCGPVisualManager2013::OnFillPlannerCaption (CDC* pDC,
		CBCGPPlannerView* pView, CRect rect, BOOL bIsToday, BOOL bIsSelected,
		BOOL bNoBorder/* = FALSE*/, BOOL bHorz /*= TRUE*/)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ()/* || m_Style == VS2012_LightBlue*/)
	{
		return CBCGPVisualManagerVS2012::OnFillPlannerCaption (pDC,
			pView, rect, bIsToday, bIsSelected, bNoBorder, bHorz);
	}

	const BOOL bMonth = DYNAMIC_DOWNCAST(CBCGPPlannerViewMonth, pView) != NULL ||
		DYNAMIC_DOWNCAST(CBCGPPlannerViewWeek, pView) != NULL;

	ASSERT_VALID (pDC);

	if (bMonth && !bIsToday)
	{
		rect.bottom--;
	}

	COLORREF clrPlannerWork = m_clrPlannerWork;
	if (pView != NULL && pView->GetPlanner() != NULL && pView->GetPlanner()->GetBackgroundColor() != CLR_DEFAULT)
	{
		clrPlannerWork = pView->GetPlanner()->GetBackgroundColor();
	}

	COLORREF clrPlannerTodayFill = GetPlannerTodayFill(clrPlannerWork);
	COLORREF clrPlannerHeader = GetPlannerHeader(clrPlannerWork);

	COLORREF clrText = globalData.clrBarText;

	if (m_bPlannerCaptionBackItemHeader)
	{
		COLORREF clr = bIsToday ? clrPlannerTodayFill : clrPlannerHeader;

		CBrush br(clr);
		pDC->FillRect(rect, &br);

		if (bIsToday)
		{
			clrText = globalData.clrBarHilite;
		}
	}
	else if (bMonth)
	{
		if (bIsToday)
		{
			CRect rect2(rect);
			rect2.bottom = rect2.top + 3;
			rect.top = rect2.bottom;

			CBrush br(clrPlannerTodayFill);
			pDC->FillRect(rect2, &br);

			clrText = clrPlannerTodayFill;
		}

		if (bIsSelected)
		{
			CBrush br(GetPlannerSelectionColor(pView));
			pDC->FillRect(rect, &br);

			clrText = globalData.clrBarHilite;
		}
	}
	else
	{
		if (rect.left != pView->GetAppointmentsRect().left)
		{
			rect.left++;
		}

		if (bIsSelected)
		{
			CBrush br(GetPlannerSelectionColor(pView));
			pDC->FillRect(rect, &br);

			clrText = globalData.clrBarHilite;
		}
		else
		{
			CBrush br(globalData.clrBarHilite);
			pDC->FillRect(rect, &br);

			if (bIsToday)
			{
				clrText = clrPlannerTodayFill;
			}
		}
	}

	return clrText;
}
//****************************************************************************************
void CBCGPVisualManager2013::OnDrawPlannerHeader (CDC* pDC, 
	CBCGPPlannerView* pView, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawPlannerHeader (pDC, pView, rect);
		return;
	}

	ASSERT_VALID (pDC);

	COLORREF clr = GetPlannerSeparatorColor (pView);

	if (DYNAMIC_DOWNCAST(CBCGPPlannerViewMonth, pView) != NULL)
	{
		COLORREF clrPlannerWork = m_clrPlannerWork;
		if (pView != NULL && pView->GetPlanner() != NULL && pView->GetPlanner()->GetBackgroundColor() != CLR_DEFAULT)
		{
			clrPlannerWork = pView->GetPlanner()->GetBackgroundColor();
		}

		clr = GetPlannerHeader(clrPlannerWork);
	}

	CBrush br (clr);
	pDC->FillRect (rect, &br);

	if (pView->IsKindOf (RUNTIME_CLASS (CBCGPPlannerViewDay)))
	{
		if (rect.left == pView->GetAppointmentsRect().left)
		{
			CRect rect1 (rect);
			rect1.right = rect1.left + 1;

			CBrush br(m_clrAccentLight);
			pDC->FillRect (rect1, &br);
		}
	}
}
//****************************************************************************************
void CBCGPVisualManager2013::OnDrawPlannerHeaderPane (CDC* pDC, 
	CBCGPPlannerView* pView, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawPlannerHeaderPane (pDC, pView, rect);
	}
}
//****************************************************************************************
COLORREF CBCGPVisualManager2013::GetPlannerViewWorkingColor (CBCGPPlannerView* pView)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::GetPlannerViewWorkingColor (pView);
	}

	return globalData.clrBarHilite;
}
//****************************************************************************************
COLORREF CBCGPVisualManager2013::GetPlannerViewNonWorkingColor (CBCGPPlannerView* pView)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::GetPlannerViewNonWorkingColor (pView);
	}

	return RGB(240, 240, 240);
}
//****************************************************************************************
COLORREF CBCGPVisualManager2013::OnFillPlannerTimeBar (CDC* pDC, 
		CBCGPPlannerView* pView, CRect rect, COLORREF& clrLine)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::OnFillPlannerTimeBar (pDC, pView, rect, clrLine);
	}

	ASSERT_VALID (pDC);

	CBrush br(globalData.clrBarHilite);
	pDC->FillRect (rect, &br);
	
	clrLine = m_clrPalennerLine;

	return globalData.clrBarText;
}
//****************************************************************************************
void CBCGPVisualManager2013::OnFillPlannerWeekBar (CDC* pDC, 
		CBCGPPlannerView* pView, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnFillPlannerWeekBar (pDC, pView, rect);
		return;
	}

	ASSERT_VALID (pDC);

	CBrush br(globalData.clrBarHilite);
	pDC->FillRect (rect, &br);
}
//****************************************************************************************
void CBCGPVisualManager2013::OnFillPlannerHeaderAllDay (CDC* pDC, 
		CBCGPPlannerView* pView, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnFillPlannerHeaderAllDay (pDC, pView, rect);
		return;
	}

	ASSERT_VALID (pDC);

	rect.bottom--;

	CBrush br(globalData.clrBarHilite);
	pDC->FillRect (rect, &br);

	CPen pen(PS_SOLID, 0, m_clrPalennerLine);
	CPen* pOldPen = (CPen*)pDC->SelectObject(&pen);

	pDC->MoveTo(rect.left, rect.bottom);
	pDC->LineTo(rect.right, rect.bottom);

	pDC->SelectObject(pOldPen);
}
//****************************************************************************************
void CBCGPVisualManager2013::OnDrawPlannerHeaderAllDayItem (CDC* pDC, 
		CBCGPPlannerView* pView, CRect rect, BOOL bIsToday, BOOL bIsSelected)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawPlannerHeaderAllDayItem (pDC, pView, rect, 
			bIsToday, bIsSelected);
		return;
	}

	if (!bIsSelected)
	{
		return;
	}

	if (pView->IsKindOf (RUNTIME_CLASS (CBCGPPlannerViewDay)) && rect.left != pView->GetAppointmentsRect().left)
	{
		rect.left++;
	}

	CBrush br (GetPlannerSelectionColor (pView));
	pDC->FillRect (rect, &br);
}

#endif

COLORREF CBCGPVisualManager2013::GetTreeControlFillColor(CBCGPTreeCtrl* pTreeCtrl, BOOL bIsSelected, BOOL bIsFocused, BOOL bIsDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::GetTreeControlFillColor(pTreeCtrl, bIsSelected, bIsFocused, bIsDisabled);
	}

	if (!bIsSelected)
	{
		return m_clrControl;
	}

	if (bIsDisabled)
	{
		return globalData.clrBarLight;
	}

	return bIsFocused ? m_clrAccentLight : globalData.clrBarShadow;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2013::GetTreeControlTextColor(CBCGPTreeCtrl* pTreeCtrl, BOOL bIsSelected, BOOL bIsFocused, BOOL bIsDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::GetTreeControlTextColor(pTreeCtrl, bIsSelected, bIsFocused, bIsDisabled);
	}

	if (!bIsSelected)
	{
		return globalData.clrBarText;
	}
	
	COLORREF clrText = (COLORREF)-1;
	
	if (bIsFocused)
	{
		if (GetRValue (m_clrAccentLight) <= 128 ||
			GetGValue (m_clrAccentLight) <= 128 ||
			GetBValue (m_clrAccentLight) <= 128)
		{
			clrText = RGB(255, 255, 255);
		}
		else
		{
			clrText = RGB(0, 0, 0);
		}
	}
	else
	{
		clrText = pTreeCtrl->GetSafeHwnd() != NULL
					? GetDlgTextColor(pTreeCtrl->GetParent())
					: globalData.clrBarText;
	}

	return clrText;
}
//*******************************************************************************
void CBCGPVisualManager2013::OnDrawMenuResizeBar (CDC* pDC, CRect rect, int nResizeFlags)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawMenuResizeBar(pDC, rect, nResizeFlags);
		return;
	}

	pDC->FillRect(rect, &globalData.brBarFace);
	OnDrawMenuResizeGipper(pDC, rect, nResizeFlags, m_clrMenuBorder);
}

#ifndef BCGP_EXCLUDE_RIBBON

void CBCGPVisualManager2013::OnDrawRibbonPaletteButtonIcon (
					CDC* pDC, 
					CBCGPRibbonPaletteIcon* pButton,
					int nID,
					CRect rectImage)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawRibbonPaletteButtonIcon(pDC, pButton, nID, rectImage);
		return;
	}

	ASSERT_VALID(pDC);
	ASSERT_VALID(pButton);

	CBCGPMenuImages::IMAGES_IDS id = (CBCGPMenuImages::IMAGES_IDS)nID;

	CBCGPMenuImages::Draw (pDC, id, rectImage,
		pButton->IsDisabled() ? CBCGPMenuImages::ImageLtGray : CBCGPMenuImages::ImageBlack);
}
//***********************************************************************************
void CBCGPVisualManager2013::OnDrawRibbonPaletteButton (
					CDC* pDC, 
					CBCGPRibbonPaletteIcon* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawRibbonPaletteButton(pDC, pButton);
		return;
	}

	ASSERT_VALID(pDC);
	ASSERT_VALID(pButton);

	OnFillRibbonButton (pDC, pButton);

	COLORREF clrBorder = m_clrRibbonGalleryBorder;

	if (!pButton->IsDisabled() && (pButton->IsHighlighted () || pButton->IsDroppedDown () || pButton->IsFocused()))
	{
		clrBorder = m_clrRibbonGalleryBorderActive;
	}

	pDC->Draw3dRect(pButton->GetRect(), clrBorder, clrBorder);
}
//*******************************************************************************
void CBCGPVisualManager2013::OnDrawRibbonKeyTip (CDC* pDC, 
												 CBCGPBaseRibbonElement* pElement, 
												 CRect rect, CString str)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawRibbonKeyTip (pDC, pElement, rect, str);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pElement);

	CBrush br(RGB(82, 82, 82));
	pDC->FillRect(rect, &br);

	str.MakeUpper ();

	COLORREF clrTextOld = pDC->SetTextColor(RGB(255, 255, 255));
	pDC->DrawText (str, rect, DT_SINGLELINE | DT_VCENTER | DT_CENTER);
	pDC->SetTextColor (clrTextOld);
}
//*******************************************************************************
BOOL CBCGPVisualManager2013::IsLayeredRibbonKeyTip () const
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManagerVS2012::IsLayeredRibbonKeyTip();
	}

	return TRUE;
}
//*******************************************************************************
void CBCGPVisualManager2013::OnDrawRibbonPaletteBorder(CDC* pDC, CBCGPRibbonPaletteButton* pButton, CRect rectBorder)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		CBCGPVisualManagerVS2012::OnDrawRibbonPaletteBorder(pDC, pButton, rectBorder);
		return;
	}

	pDC->Draw3dRect(rectBorder, m_clrRibbonGalleryBorder, m_clrRibbonGalleryBorder);
}

#endif

//////////////////////////////////////////////////////////////////////////////
// CBCGPVisualManager2016

IMPLEMENT_DYNCREATE(CBCGPVisualManager2016, CBCGPVisualManager2013)

CBCGPVisualManager2016::Style CBCGPVisualManager2016::m_Style = CBCGPVisualManager2016::Office2016_Colorful;

CBCGPVisualManager2016::CBCGPVisualManager2016()
{
	CBCGPVisualManager2013::m_Style = CBCGPVisualManager2013::Office2013_White;
}
//*********************************************************************************************************
CBCGPVisualManager2016::~CBCGPVisualManager2016()
{
}
//**************************************************************************************************************
void CBCGPVisualManager2016::SetStyle(Style style)
{
	m_Style = style;

	switch (m_Style)
	{
	case Office2016_Colorful:
		CBCGPVisualManager2013::SetStyle(CBCGPVisualManager2013::Office2013_White);
		break;

	case Office2016_DarkGray:
		CBCGPVisualManager2013::SetStyle(CBCGPVisualManager2013::Office2013_DarkGray);
		break;

	case Office2016_White:
		CBCGPVisualManager2013::SetStyle(CBCGPVisualManager2013::Office2013_White);
		break;
	}
}
//*********************************************************************************************************
void CBCGPVisualManager2016::ModifyGlobalColors ()
{
	CBCGPVisualManager2013::ModifyGlobalColors();

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return;
	}

	if (m_Style == Office2016_Colorful)
	{
		globalData.clrBarFace = RGB (230, 230, 230);
		globalData.clrBarText = RGB (70, 70, 70);
		globalData.clrBarShadow = RGB (205, 205, 205);
		globalData.clrBarHilite = RGB (253, 253, 253);
		globalData.clrBarDkShadow = RGB(171, 171, 171);
		globalData.clrBarLight = RGB (241, 241, 241);
	}
	else if (m_Style == Office2016_DarkGray)
	{
		globalData.clrBarFace = RGB(171, 171, 171);
		globalData.clrBarText = RGB (38, 38, 38);
		globalData.clrBarShadow = RGB(68, 68, 68);
		globalData.clrBarHilite = RGB (212, 212, 212);
		globalData.clrBarDkShadow = RGB(52, 52, 52);
		globalData.clrBarLight = RGB(153, 153, 153);
	}

	globalData.brBarFace.DeleteObject ();
	globalData.brBarFace.CreateSolidBrush (globalData.clrBarFace);
}
//************************************************************************************
void CBCGPVisualManager2016::SetupColors()
{
	CBCGPVisualManager2013::SetupColors();

	m_brCaption.DeleteObject();

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return;
	}

	COLORREF clrCaption = globalData.clrActiveCaption;

	if (m_Style == Office2016_Colorful)
	{
		m_clrMenuLight = globalData.clrBarHilite;
		m_clrRibbonTabs = globalData.clrBarHilite;
		m_clrSeparator = RGB(213, 213, 213);

		m_clrHighlightMenuItem = m_clrHighlight = CBCGPDrawManager::ColorMakeDarker(globalData.clrBarFace, .025);
		m_clrHighlightDn = CBCGPDrawManager::ColorMakeDarker(m_clrHighlight, .12);
		m_clrHighlightChecked = CBCGPDrawManager::ColorMakeDarker(m_clrHighlight, .008);

		m_clrHighlightGradientLight = m_clrHighlight;
		m_clrHighlightGradientDark = m_clrHighlight;
		
		m_clrHighlightDnGradientDark = m_clrHighlightDn;
		m_clrHighlightDnGradientLight =  m_clrHighlightDn;

		m_clrHighlightNC = m_clrAccentHilight;
		m_clrHighlightDnNC = CBCGPDrawManager::ColorMakeDarker(m_clrAccent);

		m_clrNcTextActive = globalData.clrBarHilite;
		m_clrNcTextInactive = globalData.clrBarShadow;

		m_clrToolBarGradientDark = m_clrToolBarGradientLight = CBCGPDrawManager::ColorMakeLighter(globalData.clrBarFace, .04);
		
		m_clrBorderActive = CBCGPDrawManager::ColorMakeLighter(m_clrAccentHilight);
		m_clrPressedButtonBorder = globalData.clrBarDkShadow;

		m_clrScrollButtonHot = CBCGPDrawManager::ColorMakeDarker(globalData.clrBarDkShadow);
		m_clrScrollButton = m_clrMenuBorder;

		CBCGPMenuImages::SetColor (CBCGPMenuImages::ImageBlack, RGB(119, 119, 119));

		clrCaption = m_clrAccent;
	}
	else	// Office2016_DarkGray
	{
		m_clrAccentLight = CBCGPDrawManager::ColorMakePale(m_clrAccent, .75);

		m_clrTextDisabled = CBCGPDrawManager::ColorMakeLighter(globalData.clrBarShadow, 1.1);

		m_clrMenuLight = RGB(240, 240, 240);

		m_clrRibbonCategoryFill = RGB (178, 178, 178);
		m_clrMenuSeparator = RGB(227, 227, 227);
		m_clrSeparator = globalData.clrBarLight;

		m_clrHighlightMenuItem = CBCGPDrawManager::ColorMakeLighter(globalData.clrBarFace, .2);
		m_clrHighlight = CBCGPDrawManager::ColorMakeDarker(globalData.clrBarFace, .12);
		m_clrHighlightDn = CBCGPDrawManager::ColorMakeDarker(m_clrHighlight, .12);
		m_clrHighlightChecked = CBCGPDrawManager::ColorMakeDarker(m_clrHighlight, .008);

		m_clrEditBoxBorder = RGB(106, 106, 106);
		m_clrHighlightNC = RGB(100, 100, 100);
		m_clrHighlightDnNC = globalData.clrBarDkShadow;

		m_clrNcTextActive = globalData.clrBarHilite;
		m_clrNcTextInactive = globalData.clrBarLight;

		m_clrScrollButtonHot = globalData.clrBarDkShadow;
		m_clrScrollButton = RGB(87, 87, 87);

		m_clrDlgBackground = RGB(106, 106, 106);

		clrCaption = globalData.clrBarShadow;

		m_clrMenuBorder = globalData.clrBarFace;

		m_clrBorderActive = globalData.clrBarShadow;
		m_clrPressedButtonBorder = globalData.clrBarDkShadow;

		m_clrToolBarGradientDark = m_clrToolBarGradientLight = CBCGPDrawManager::ColorMakeLighter(globalData.clrBarFace, .08);

		m_clrRibbonGalleryBorder = globalData.clrBarLight;
		m_clrRibbonGalleryBorderActive = globalData.clrBarShadow;

		CBCGPMenuImages::SetColor (CBCGPMenuImages::ImageBlack, RGB(48, 48, 48));
	}

	m_clrMenuItemBorder = m_clrHighlight;

	m_brCaption.CreateSolidBrush(clrCaption);
}
//************************************************************************************
void CBCGPVisualManager2016::OnUpdateSystemColors ()
{
	CBCGPVisualManager2013::OnUpdateSystemColors ();

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return;
	}

	m_brStatusBar.DeleteObject();
	m_brStatusBar.CreateSolidBrush((m_Style == Office2016_Colorful) ? globalData.clrBarLight : CBCGPDrawManager::ColorMakeDarker(m_clrHighlightNC, .15));

	if (m_Style == Office2016_DarkGray)
	{
		m_clrFrameShadowBaseActive = globalData.clrBarShadow;
	}
}
//*****************************************************************************
void CBCGPVisualManager2016::OnFillStatusBarFrame(CDC* pDC, CRect rectStatus, BOOL bActive)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnFillStatusBarFrame(pDC, rectStatus, bActive);
		return;
	}

	ASSERT_VALID(pDC);

	COLORREF clrBorder = GetNCBorderColor(bActive);
	pDC->FillSolidRect(rectStatus, clrBorder);

	rectStatus.DeflateRect(1, 0, 1, 1);
	pDC->FillRect(rectStatus, &m_brStatusBar);
}
//*****************************************************************************
COLORREF CBCGPVisualManager2016::GetNCBorderColor(BOOL bActive)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::GetNCBorderColor(bActive);
	}

	return bActive ? globalData.clrBarShadow : (m_clrFrame != (COLORREF)-1 ? m_clrFrame : m_clrNcBorder);
}

#ifndef BCGP_EXCLUDE_RIBBON

void CBCGPVisualManager2016::OnFillBarBackground(CDC* pDC, CBCGPBaseControlBar* pBar, CRect rectClient, CRect rectClip, BOOL bNCArea /*= FALSE*/)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnFillBarBackground(pDC, pBar, rectClient, rectClip, bNCArea);
		return;
	}

	if (pBar->IsKindOf (RUNTIME_CLASS (CBCGPOutlookBarPane)))
	{
		CBrush br(m_clrToolBarGradientLight);
		pDC->FillRect (rectClient, &br);
		return;
	}

	if (pBar->IsKindOf (RUNTIME_CLASS (CBCGPRibbonStatusBar)))
	{
		CBCGPRibbonStatusBar* pRibbonStatusBar = DYNAMIC_DOWNCAST (CBCGPRibbonStatusBar, pBar);

		pDC->FillRect(rectClient, &m_brStatusBar);

		{
			CBCGPPenSelector ps(*pDC, globalData.clrBarShadow);

			pDC->MoveTo(rectClient.left, rectClient.top);
			pDC->LineTo(rectClient.right, rectClient.top);
		}

		if (pRibbonStatusBar->IsBottomFrame())
		{
			CWnd* pWnd = pBar->GetParent ();
			ASSERT_VALID (pWnd);
			
			BOOL bActive = IsWindowActive (pWnd);
			COLORREF clrBorder = GetNCBorderColor(bActive);

			CBCGPPenSelector ps(*pDC, clrBorder);
			pDC->MoveTo(rectClient.left, rectClient.bottom - 1);
			pDC->LineTo(rectClient.right, rectClient.bottom - 1);
		}
		return;
	}

	if (pBar->IsKindOf (RUNTIME_CLASS (CBCGPRibbonBar)))
	{
		CBCGPRibbonBar* pRibbonBar = (CBCGPRibbonBar*)pBar;
		int nCaptionHeight = pRibbonBar->GetTabsHeight() + pRibbonBar->GetCaptionHeight();

		if (pRibbonBar->IsBackstageViewActive())
		{
			rectClient.bottom = rectClient.top + nCaptionHeight + 1;
			pDC->FillRect(rectClient, &GetDlgBackBrush(pRibbonBar));
		}
		else
		{
			CBCGPVisualManager2013::OnFillBarBackground(pDC, pBar, rectClient, rectClip, bNCArea);

			rectClient.bottom = rectClient.top + nCaptionHeight;
			pDC->FillRect(rectClient, &m_brCaption);
		}

		return;
	}

	CBCGPVisualManager2013::OnFillBarBackground(pDC, pBar, rectClient, rectClip, bNCArea);
}
//************************************************************************************
COLORREF CBCGPVisualManager2016::OnDrawRibbonCategoryTab(CDC* pDC, CBCGPRibbonTab* pTab, BOOL bIsActive)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnDrawRibbonCategoryTab(pDC, pTab, bIsActive);
	}

	ASSERT_VALID(pDC);
	ASSERT_VALID(pTab);

	COLORREF clrText = CBCGPVisualManager2013::OnDrawRibbonCategoryTab(pDC, pTab, bIsActive);
	
	CBCGPRibbonBar* pRibbon = pTab->GetTopLevelRibbonBar();
	if (!bIsActive || (pRibbon->GetHideFlags() != 0 && !pTab->IsDroppedDown()))
	{
		if (pTab->IsHighlighted())
		{
			CRect rect = pTab->GetRect ();
			
			rect.right -= 2;
			rect.top += 2;
			
			pDC->FillRect(rect, &m_brAccentHighlight);
			
			clrText = m_Style == Office2016_DarkGray ? RGB(255, 255, 255) : m_clrAccentLight;
		}
		else if (pRibbon->GetHideFlags() != 0 && pTab->IsFocused())
		{
			clrText = m_clrAccentText;
		}
		else
		{
			clrText = RGB(255, 255, 255);
		}
	}
	else
	{
		COLORREF clrTab = RibbonCategoryColorToRGB (pTab->GetParentCategory ()->GetTabColor ());

		if (clrTab != (COLORREF)-1)
		{
			clrText = CBCGPDrawManager::ColorMakeDarker(clrTab, .5);
		}
		else if (m_Style == Office2016_DarkGray && clrText == m_clrAccentText)
		{
			clrText = globalData.clrBarText;
		}
	}

	return clrText;
}
//*****************************************************************************
COLORREF CBCGPVisualManager2016::GetRibbonEditTextColor(
														   CBCGPRibbonEditCtrl* pEdit,
														   BOOL bIsHighlighted,
														   BOOL bIsDisabled)
{

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetRibbonEditTextColor(pEdit, bIsHighlighted, bIsDisabled);
	}

	return bIsDisabled ? m_clrTextDisabled : globalData.clrBarText;
}
//************************************************************************************
COLORREF CBCGPVisualManager2016::GetRibbonQATTextColor(BOOL bDisabled/* = FALSE*/)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetRibbonQATTextColor(bDisabled);
	}

	if (m_Style == Office2016_Colorful && !m_bQuickAccessToolbarOnTop)
	{
		return CBCGPVisualManager2013::GetRibbonQATTextColor(bDisabled);
	}

	if (!bDisabled)
	{
		return globalData.clrBtnHilite;
	}

	return m_Style == Office2016_DarkGray ? globalData.clrBarLight : m_clrHighlightDn;
}
//************************************************************************************
BOOL CBCGPVisualManager2016::IsSimplifiedRibbonQATIcon(CBCGPRibbonButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::IsSimplifiedRibbonQATIcon(pButton);
	}

	if (m_Style == Office2016_Colorful && !m_bQuickAccessToolbarOnTop)
	{
		return CBCGPVisualManager2013::IsSimplifiedRibbonQATIcon(pButton);
	}

	return TRUE;
}
//************************************************************************************
BOOL CBCGPVisualManager2016::IsSimplifiedRibbonTabControlIcon(CBCGPRibbonButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::IsSimplifiedRibbonTabControlIcon(pButton);
	}
	
	return TRUE;
}
//************************************************************************************
COLORREF CBCGPVisualManager2016::OnFillRibbonButton(CDC* pDC, CBCGPRibbonButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnFillRibbonButton(pDC, pButton);
	}

	if (pButton->IsKindOf (RUNTIME_CLASS (CBCGPRibbonEdit)))
	{
		return CBCGPVisualManager2013::OnFillRibbonButton(pDC, pButton);
	}

	CBCGPRibbonBar* pRibbonBar = pButton->GetTopLevelRibbonBar();
	if (m_Style == Office2016_Colorful && pRibbonBar != NULL && pRibbonBar->IsBackstageViewActive())
	{
		return CBCGPVisualManager2013::OnFillRibbonButton(pDC, pButton);
	}

	BOOL bIsCaptionButton = DYNAMIC_DOWNCAST(CBCGPRibbonCaptionButton, pButton) != NULL;
	if (bIsCaptionButton || pButton->IsTabElement() || (pButton->IsQATMode() && (m_bQuickAccessToolbarOnTop || m_Style == Office2016_Colorful) && !pButton->HasParentMenu()))
	{
		const BOOL bIsHighlighted = 
			((pButton->IsHighlighted () || pButton->IsDroppedDown ()) && 
			!pButton->IsDisabled ()) || pButton->IsFocused ();

		CRect rect = pButton->GetRect();
		CRect rectMenu = pButton->GetMenuRect ();
		
		CRect rectCommand (0, 0, 0, 0);
		if (!rectMenu.IsRectEmpty ())
		{
			rectCommand = pButton->GetCommandRect ();
			
			if (pButton->GetLocationInGroup () != CBCGPBaseRibbonElement::RibbonElementNotInGroup)
			{
				rectMenu.DeflateRect (0, 1, 1, 1);
				rectCommand.DeflateRect (1, 1, 0, 1);
			}
		}
		
		if (!rectMenu.IsRectEmpty () && bIsHighlighted)
		{
			if (pButton->IsCommandAreaHighlighted ())
			{
				if (pButton->IsPressed ())
				{
					pDC->FillRect(rectCommand, &m_brHighlightDnNC);
				}
				else
				{
					pDC->FillRect(rectCommand, &m_brHighlightNC);
				}

				pDC->Draw3dRect(rectMenu, m_clrHighlightNC, m_clrHighlightNC);
			}
			else
			{
				if (pButton->IsPressed () || pButton->IsDroppedDown ())
				{
					pDC->FillRect(rect, &m_brHighlightDnNC);
				}
				else
				{
					pDC->FillRect(rectMenu, &m_brHighlightNC);
					pDC->Draw3dRect(rectCommand, m_clrHighlightNC, m_clrHighlightNC);
				}
			}
		}
		else if (bIsHighlighted)
		{
			if (pButton->IsPressed () || pButton->IsDroppedDown ())
			{
				pDC->FillRect(rect, &m_brHighlightDnNC);
			}
			else
			{
				pDC->FillRect(rect, &m_brHighlightNC);
			}
		}
		else if (pButton->IsChecked())
		{
			pDC->FillRect(rect, &m_brHighlightNC);
		}

		if (bIsCaptionButton || pButton->IsTabElement())
		{
			if (!pButton->IsDisabled())
			{
				return globalData.clrBtnHilite;
			}
			
			return m_Style == Office2016_DarkGray ? globalData.clrBarLight : m_clrHighlightDn;
		}

		return GetRibbonQATTextColor(pButton->IsDisabled());
	}

	return CBCGPVisualManager2013::OnFillRibbonButton(pDC, pButton);
}
//************************************************************************************
void CBCGPVisualManager2016::OnDrawRibbonMinimizeButtonImage(CDC* pDC, CBCGPRibbonMinimizeButton* pButton, BOOL bRibbonIsMinimized)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnDrawRibbonMinimizeButtonImage(pDC, pButton, bRibbonIsMinimized);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);
	
	CBCGPMenuImages::IMAGE_STATE state = CBCGPMenuImages::ImageWhite;
	
	if (pButton->IsDisabled())
	{
		state = CBCGPMenuImages::ImageLtGray;
	}
	else if (pButton->IsPressed() && pButton->IsHighlighted())
	{
		state = CBCGPMenuImages::ImageLtGray;
	}
	
	CBCGPMenuImages::Draw(pDC, 
		bRibbonIsMinimized ? CBCGPMenuImages::IdMinimizeRibbon : CBCGPMenuImages::IdMaximizeRibbon, pButton->GetRect(), state);
}
//************************************************************************************
COLORREF CBCGPVisualManager2016::OnDrawRibbonStatusBarPane(CDC* pDC, CBCGPRibbonStatusBar* pBar, CBCGPRibbonStatusBarPane* pPane)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnDrawRibbonStatusBarPane(pDC, pBar, pPane);
	}

	CRect rect = pPane->GetRect ();
	
	if (pPane->IsHighlighted ())
	{
		if (pPane->IsPressed ())
		{
			pDC->FillSolidRect(rect, globalData.clrBarDkShadow);
		}
		else
		{
			pDC->FillSolidRect(rect, globalData.clrBarShadow);
		}
	}
	else if (pPane->IsChecked())
	{
		pDC->FillSolidRect(rect, globalData.clrBarShadow);
	}

	if (pPane->IsDisabled())
	{
		return m_clrTextDisabled;
	}
	
	return m_Style == Office2016_Colorful ? globalData.clrBarText : m_clrMenuLight;
}
//************************************************************************************
COLORREF CBCGPVisualManager2016::GetRibbonCaptionTextColor(CBCGPRibbonBar* pBar, BOOL bActive)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetRibbonCaptionTextColor(pBar, bActive);
	}

	if (pBar->IsBackstageViewActive() && m_Style == Office2016_Colorful)
	{
		return globalData.clrBarText;
	}
	
	return bActive ? RGB(255, 255, 255) : globalData.clrBarFace;
}
//****************************************************************************
COLORREF CBCGPVisualManager2016::OnFillRibbonStatusBarButton(CDC* pDC, CBCGPRibbonButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnFillRibbonStatusBarButton(pDC, pButton);
	}

	ASSERT_VALID(pDC);
	ASSERT_VALID(pButton);

	CRect rect = pButton->GetRect ();

	if (pButton->IsHighlighted ())
	{
		if (pButton->IsPressed ())
		{
			pDC->FillSolidRect(rect, globalData.clrBarDkShadow);
		}
		else
		{
			pDC->FillSolidRect(rect, globalData.clrBarShadow);
		}
	}
	else if (pButton->IsChecked())
	{
		pDC->FillSolidRect(rect, globalData.clrBarShadow);
	}
	
	return m_Style == Office2016_Colorful ? globalData.clrBarText : m_clrMenuLight;
}
//**************************************************************************************
COLORREF CBCGPVisualManager2016::GetRibbonStatusBarTextColor (CBCGPRibbonStatusBar* pStatusBar)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetRibbonStatusBarTextColor(pStatusBar);
	}

	return m_Style == Office2016_Colorful ? globalData.clrBarText : m_clrMenuLight;
}
//********************************************************************************
COLORREF CBCGPVisualManager2016::GetRibbonHyperlinkTextColor (CBCGPRibbonHyperlink* pHyperLink)
{
	ASSERT_VALID (pHyperLink);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || pHyperLink->IsDisabled () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetRibbonHyperlinkTextColor (pHyperLink);
	}

	if (DYNAMIC_DOWNCAST(CBCGPRibbonStatusBar, pHyperLink->GetParentRibbonBar()) != NULL)
	{
		if (m_Style == Office2016_DarkGray)
		{
			return pHyperLink->IsHighlighted () ? CBCGPDrawManager::ColorMakeDarker(m_clrAccentLight) : m_clrAccentLight;
		}

		return pHyperLink->IsHighlighted () ? m_clrAccentHilight : m_clrAccentText;
	}

	return CBCGPVisualManager2013::GetRibbonHyperlinkTextColor (pHyperLink);
}
//********************************************************************************
COLORREF CBCGPVisualManager2016::GetRibbonFrameColor(CBCGPRibbonBar* pWndRibbonBar)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetRibbonFrameColor(pWndRibbonBar);
	}

	return m_Style == Office2016_Colorful ? m_clrAccent : globalData.clrBarShadow;
}
//********************************************************************************
void CBCGPVisualManager2016::OnFillRibbonBackstageForm(CDC* pDC, CWnd* pDlg, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnFillRibbonBackstageForm(pDC, pDlg, rect);
		return;
	}

	ASSERT_VALID(pDC);

	pDC->FillRect(rect, &GetDlgBackBrush(pDlg));
}
//********************************************************************************
void CBCGPVisualManager2016::PrepareRibbonBackgroundDark(CBCGPToolBarImages& src, CBCGPToolBarImages& dst)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_Colorful)
	{
		return;
	}

	src.CopyTo(dst);
	
	COLORREF clrDest = (m_Style == Office2016_Colorful) ? CBCGPDrawManager::ColorMakeLighter(m_clrAccent, .05) : globalData.clrBarShadow;
	dst.AddaptColors(RGB(255, 255, 255), clrDest, FALSE);
}
//********************************************************************************
void CBCGPVisualManager2016::GetRibbonSliderColors (CBCGPRibbonSlider* pSlider, 
					BOOL bIsHighlighted, 
					BOOL bIsPressed,
					BOOL bIsDisabled,
					COLORREF& clrLine,
					COLORREF& clrFill)
{
	CBCGPVisualManager2013::GetRibbonSliderColors(pSlider, bIsHighlighted, bIsPressed, bIsDisabled, clrLine, clrFill);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return;
	}

	ASSERT_VALID(pSlider);

	if (bIsHighlighted || bIsPressed)
	{
		clrFill = m_clrHighlight;
	}

	if (pSlider->IsQATMode() && m_bQuickAccessToolbarOnTop)
	{
		if (m_Style == Office2016_Colorful)
		{
			clrLine = (bIsHighlighted || bIsPressed) ? globalData.clrBtnHilite : globalData.clrBarShadow;
		}
		else
		{
			clrFill = clrLine = (bIsHighlighted || bIsPressed) ? m_clrAccentLight : globalData.clrBarFace;
		}
		return;
	}

	if (m_Style == Office2016_Colorful)
	{
		clrLine = (bIsHighlighted || bIsPressed) ? globalData.clrBarText : globalData.clrBarDkShadow;
	}
	else
	{
		if (pSlider->IsStatusBarMode())
		{
			clrFill = clrLine = (bIsHighlighted || bIsPressed) ? m_clrAccentLight : globalData.clrBarFace;
		}
		else
		{
			clrFill = clrLine = (bIsHighlighted || bIsPressed) ? m_clrAccent : globalData.clrBarDkShadow;
		}
	}
}
//********************************************************************************
void CBCGPVisualManager2016::OnDrawRibbonSliderChannel (
			CDC* pDC, CBCGPRibbonSlider* pSlider, 
			CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		CBCGPVisualManager2013::OnDrawRibbonSliderChannel(pDC, pSlider, rect);
		return;
	}

	ASSERT_VALID (pDC);

	BOOL bIsVert = FALSE;
	COLORREF clrLine = globalData.clrBarDkShadow;

	if (pSlider != NULL)
	{
		ASSERT_VALID (pSlider);

		bIsVert = pSlider->IsVert ();

		if (pSlider->IsStatusBarMode() || (pSlider->IsQATMode() && m_bQuickAccessToolbarOnTop))
		{
			clrLine = globalData.clrBarFace;
		}
	}

	CBCGPDrawManager dm(*pDC);
	
	if (bIsVert)
	{
		dm.DrawLine(rect.CenterPoint().x, rect.top, rect.CenterPoint().x, rect.bottom, clrLine);
	}
	else
	{
		dm.DrawLine(rect.left, rect.CenterPoint().y, rect.right, rect.CenterPoint().y, clrLine);
	}
}
//****************************************************************************
void CBCGPVisualManager2016::OnDrawRibbonDefaultPaneButton (
					CDC* pDC, 
					CBCGPRibbonButton* pButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnDrawRibbonDefaultPaneButton (pDC, pButton);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

	if (pButton->IsQATMode () || pButton->IsSearchResultMode ())
	{
		OnFillRibbonButton (pDC, pButton);
	}
	else
	{
		CRect rect = pButton->GetRect();
		CRect rectImage = rect;

		rect.right--;

		COLORREF clrHighlight = m_Style == Office2016_Colorful ? globalData.clrBarShadow : globalData.clrBarLight;

		if (pButton->IsDroppedDown())
		{
			pDC->FillSolidRect(rect, clrHighlight);
		}
		else if (pButton->IsHighlighted() || pButton->IsFocused())
		{
			pDC->FillSolidRect(rect, CBCGPDrawManager::PixelAlpha(clrHighlight, 103));
		}

		CSize sizeImage = pButton->GetImageSize (CBCGPRibbonButton::RibbonImageSmall);

		rectImage.top += 10;
		rectImage.bottom = rectImage.top + sizeImage.cy;

		rectImage.left = rectImage.CenterPoint().x - sizeImage.cx / 2;
		rectImage.right = rectImage.left + sizeImage.cx;

		rectImage.InflateRect(5, 5);

		COLORREF clrIcon;
		BOOL bHightlightIcon = pButton->IsHighlighted() || pButton->IsDroppedDown();

		if (m_Style == Office2016_Colorful)
		{
			clrIcon = bHightlightIcon ? globalData.clrBarLight : globalData.clrBarHilite;
		}
		else
		{
			clrIcon = bHightlightIcon ? CBCGPDrawManager::ColorMakeLighter(globalData.clrBarLight) : globalData.clrBarHilite;
		}

		COLORREF clrBorder = m_Style == Office2016_DarkGray ? globalData.clrBarShadow : m_clrMenuBorder;

		pDC->FillSolidRect(rectImage, clrIcon);
		pDC->Draw3dRect(rectImage, clrBorder, clrBorder);
	}

	OnDrawRibbonDefaultPaneButtonContext (pDC, pButton);
}
//*****************************************************************************
COLORREF CBCGPVisualManager2016::OnFillRibbonPanelCaption (
										   CDC* pDC,
										   CBCGPRibbonPanel* pPanel, 
										   CRect rectCaption)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_Colorful)
	{
		return CBCGPVisualManager2013::OnFillRibbonPanelCaption(pDC, pPanel, rectCaption);
	}

	return RGB(102, 102, 102);
}
//*****************************************************************************
COLORREF CBCGPVisualManager2016::OnFillRibbonBackstageLeftPane(CDC* pDC, CRect rectPanel)
{
	ASSERT_VALID(pDC);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::OnFillRibbonBackstageLeftPane(pDC, rectPanel);
	}

	CBrush brFill(globalData.clrBarText);
	pDC->FillRect(rectPanel, &brFill);
	
	return OnGetRibbonBackstageLeftPaneTextColor();
}
//**************************************************************************************
COLORREF CBCGPVisualManager2016::GetRibbonBackstageTextColor()
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::GetRibbonBackstageTextColor();
	}

	return RGB(240, 240, 240);
}
//**************************************************************************************
COLORREF CBCGPVisualManager2016::GetRibbonBackstageInfoTextColor()
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::GetRibbonBackstageInfoTextColor();
	}

	return RGB(240, 240, 240);
}
//********************************************************************************
void CBCGPVisualManager2016::OnDrawRibbonMenuArrow(CDC* pDC, CBCGPRibbonButton* pButton, UINT idIn, const CRect& rectMenuArrow)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnDrawRibbonMenuArrow(pDC, pButton, idIn, rectMenuArrow);
		return;
	}

	ASSERT_VALID(pDC);
	ASSERT_VALID(pButton);

	if (pButton->IsDrawingOnList())
	{
		CBCGPVisualManager2013::OnDrawRibbonMenuArrow(pDC, pButton, idIn, rectMenuArrow);
		return;
	}

	CBCGPMenuImages::IMAGES_IDS id = (CBCGPMenuImages::IMAGES_IDS)idIn;
	CBCGPMenuImages::IMAGE_STATE state = CBCGPMenuImages::ImageDkGray;

	BOOL bIsDarkBackgroundQAT = (m_bQuickAccessToolbarOnTop || m_Style == Office2016_DarkGray);

	if (pButton->IsDisabled())
	{
		state = m_Style == Office2016_Colorful ? CBCGPMenuImages::ImageLtGray : CBCGPMenuImages::ImageGray;
	}
	else if (pButton->IsDrawSimplifiedIcon() || (pButton->IsQATMode() && bIsDarkBackgroundQAT) || pButton->IsTabElement())
	{
		state = CBCGPMenuImages::ImageWhite;
	}

	CBCGPMenuImages::Draw (pDC, id, rectMenuArrow, state);
}
//*******************************************************************************
void CBCGPVisualManager2016::OnDrawRibbonPaletteBorder(CDC* pDC, CBCGPRibbonPaletteButton* pButton, CRect rectBorder)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnDrawRibbonPaletteBorder(pDC, pButton, rectBorder);
		return;
	}

	pDC->FillRect(rectBorder, &m_brControl);
	pDC->Draw3dRect(rectBorder, m_clrRibbonGalleryBorder, m_clrRibbonGalleryBorder);
}
//***********************************************************************************
COLORREF CBCGPVisualManager2016::OnDrawRibbonCategoryCaption(CDC* pDC, CBCGPRibbonContextCaption* pContextCaption)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pContextCaption);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnDrawRibbonCategoryCaption(pDC, pContextCaption);
	}

	COLORREF clr = RibbonCategoryColorToRGB (pContextCaption->GetColor ());
	CRect rect = pContextCaption->GetRect ();
	
	if (clr == (COLORREF)-1)
	{
		return globalData.clrBarText;
	}

	COLORREF clrTopLine = CBCGPDrawManager::ColorMakeLighter(clr);

	CBCGPRibbonBar* pRibbon = pContextCaption->GetParentRibbonBar();
	ASSERT_VALID(pRibbon);

	rect.bottom += pRibbon->GetTabsHeight();

	LOGBRUSH lbr;
	m_brCaption.GetLogBrush(&lbr);

	COLORREF clrFill = CBCGPDrawManager::ColorMakeDarker(lbr.lbColor);

	pDC->FillSolidRect(rect, clrFill);

	CRect rectTop = rect;
	rectTop.bottom = rectTop.top + globalUtils.ScaleByDPI(4);

	pDC->FillSolidRect(rectTop, clrTopLine);

	return CBCGPDrawManager::ColorMakeLighter(clr);
}
//***********************************************************************************
void CBCGPVisualManager2016::OnFillRibbonQAT(CDC* pDC, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray || m_bQuickAccessToolbarOnTop)
	{
		CBCGPVisualManager2013::OnFillRibbonQAT(pDC, rect);
		return;
	}

	ASSERT_VALID(pDC);
	pDC->FillRect(rect, &m_brDlgBackground);
}
//***********************************************************************************
void CBCGPVisualManager2016::OnFillRibbonQATPopup(CDC* pDC, CBCGPRibbonPanelMenuBar* pMenuBar, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_Colorful)
	{
		CBCGPVisualManager2013::OnFillRibbonQATPopup(pDC, pMenuBar, rect);
		return;
	}

	ASSERT_VALID(pDC);
	pDC->FillSolidRect(rect, m_clrMenuLight);
}
//***********************************************************************************
void CBCGPVisualManager2016::OnDrawRibbonDropDownPanel(CDC* pDC, CBCGPRibbonPanel* pPanel, CRect rectFill)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		CBCGPVisualManager2013::OnDrawRibbonDropDownPanel(pDC, pPanel, rectFill);
		return;
	}
	
	ASSERT_VALID(pDC);
	pDC->FillSolidRect(rectFill, m_clrMenuLight);
}

#endif

BOOL CBCGPVisualManager2016::GetToolTipParams (CBCGPToolTipParams& params, UINT nType)
{
	if (!CBCGPVisualManager2013::GetToolTipParams(params, nType))
	{
		return FALSE;
	}

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return TRUE;
	}

	params.m_clrText = globalData.clrBarText;

	if (m_Style == Office2016_DarkGray)
	{
		params.m_clrFill = m_clrMenuLight;
		params.m_clrFillGradient = (COLORREF)-1;
		params.m_clrBorder = RGB(171, 171, 171);
	}

	return TRUE;
}
//*****************************************************************************
COLORREF CBCGPVisualManager2016::GetStatusBarPaneTextColor (CBCGPStatusBar* pStatusBar, CBCGStatusBarPaneInfo* pPane)
{
	ASSERT (pPane != NULL);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || pPane->clrText != (COLORREF)-1 || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetStatusBarPaneTextColor (pStatusBar, pPane);
	}

	return (pPane->nStyle & SBPS_DISABLED) ? m_clrTextDisabled : GetDlgTextColor(pStatusBar);
}
//*****************************************************************************
CBrush* CBCGPVisualManager2016::GetNcCaptionBrush(BOOL bActive, BOOL bSmallDlgCaption)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White || bSmallDlgCaption)
	{
		return CBCGPVisualManager2013::GetNcCaptionBrush(bActive, bSmallDlgCaption);
	}

	return &m_brCaption;
}
//*****************************************************************************
CBCGPMenuImages::IMAGE_STATE CBCGPVisualManager2016::GetNcBtnState(BCGBUTTON_STATE state, BOOL bActive, BOOL bEnabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetNcBtnState(state, bActive, bEnabled);
	}

	return (bActive && bEnabled) ? CBCGPMenuImages::ImageWhite : CBCGPMenuImages::ImageLtGray;
}
//*************************************************************************************
void CBCGPVisualManager2016::OnScrollBarDrawThumb (CDC* pDC, CBCGPScrollBar* pScrollBar, CRect rect, 
		BOOL bHorz, BOOL bHighlighted, BOOL bPressed, BOOL bDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnScrollBarDrawThumb(pDC, pScrollBar, rect, bHorz, bHighlighted, bPressed, bDisabled);
		return;
	}

	ASSERT_VALID(pDC);

	CBCGPDrawManager dm (*pDC);
	dm.DrawRect (rect, 
		bPressed || bDisabled ? 
			(m_Style == Office2016_Colorful) ? globalData.clrBarLight : RGB(196, 196, 196) :
			(m_Style == Office2016_Colorful) ? globalData.clrBarHilite : (bHighlighted ? m_clrRibbonCategoryFill : globalData.clrBarLight), 
		bHighlighted ? m_clrScrollButtonHot : m_clrScrollButton);
}
//*************************************************************************************
void CBCGPVisualManager2016::OnScrollBarDrawButton (CDC* pDC, CBCGPScrollBar* pScrollBar, CRect rect, 
		BOOL bHorz, BOOL bHighlighted, BOOL bPressed, BOOL bFirst, BOOL bDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnScrollBarDrawButton(pDC, pScrollBar, rect, bHorz, bHighlighted, bPressed, bFirst, bDisabled);
		return;
	}

	COLORREF clrFill = bPressed ? 
		(m_Style == Office2016_Colorful) ? globalData.clrBarLight : RGB(196, 196, 196) :
		(m_Style == Office2016_Colorful) ? globalData.clrBarHilite : (bHighlighted ? m_clrRibbonCategoryFill : globalData.clrBarLight);

	CBCGPDrawManager dm (*pDC);
	dm.DrawRect (rect, 
		clrFill, 
		bDisabled ? (COLORREF)-1 : bHighlighted ? m_clrScrollButtonHot : m_clrScrollButton);

	CBCGPMenuImages::IMAGES_IDS ids;
	if (bHorz)
	{
		if (pScrollBar->GetSafeHwnd() != NULL && (pScrollBar->GetExStyle() & WS_EX_LAYOUTRTL))
		{
			bFirst = !bFirst;
		}
		
		ids = bFirst ? CBCGPMenuImages::IdArowLeftTab3d : CBCGPMenuImages::IdArowRightTab3d;
	}
	else
	{
		ids = bFirst ? CBCGPMenuImages::IdArowUpLarge : CBCGPMenuImages::IdArowDownLarge;
	}

	CBCGPMenuImages::IMAGE_STATE state = bDisabled ? (m_Style == Office2016_Colorful ? CBCGPMenuImages::ImageLtGray : CBCGPMenuImages::ImageGray) : CBCGPMenuImages::ImageBlack;

	CBCGPMenuImages::Draw (pDC, ids, rect, state);
}
//*************************************************************************************
void CBCGPVisualManager2016::OnScrollBarFillBackground (CDC* pDC, CBCGPScrollBar* pScrollBar, CRect rect, 
		BOOL bHorz, BOOL bHighlighted, BOOL bPressed, BOOL bFirst, BOOL bDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnScrollBarFillBackground(pDC, pScrollBar, rect, bHorz, bHighlighted, bPressed, bFirst, bDisabled);
		return;
	}

	CBCGPDrawManager dm (*pDC);
	dm.DrawRect (rect, m_Style == Office2016_Colorful ? m_clrHighlight : m_clrScrollButton, (COLORREF)-1);
}
//*******************************************************************************
void CBCGPVisualManager2016::OnDrawMenuResizeBar (CDC* pDC, CRect rect, int nResizeFlags)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnDrawMenuResizeBar(pDC, rect, nResizeFlags);
		return;
	}

	pDC->FillSolidRect(rect, globalData.clrBarLight);
	OnDrawMenuResizeGipper(pDC, rect, nResizeFlags, globalData.clrBarText);
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::GetPrintPreviewFrameColor(BOOL bIsActive)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return CBCGPVisualManager2013::GetPrintPreviewFrameColor(bIsActive);
	}

	return bIsActive ? globalData.clrBarDkShadow : globalData.clrBarShadow;
}
//************************************************************************************
COLORREF CBCGPVisualManager2016::OnDrawMenuLabel (CDC* pDC, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnDrawMenuLabel(pDC, rect);
	}

	ASSERT_VALID(pDC);

	CBrush br(m_Style == Office2016_Colorful ? RGB(235, 235, 235) : RGB(225, 225, 225));
	pDC->FillRect(rect, &br);

	return globalData.clrBarText;
}
//*****************************************************************************
void CBCGPVisualManager2016::OnDrawSeparator (CDC* pDC, CBCGPBaseControlBar* pBar, CRect rect, BOOL bIsHoriz)
{
#ifndef BCGP_EXCLUDE_RIBBON
	if (DYNAMIC_DOWNCAST(CBCGPRibbonBackstageView, pBar) != NULL && globalData.m_nBitsPerPixel > 8 && !globalData.IsHighContastMode() && m_Style == Office2016_DarkGray)
	{
		CBCGPPenSelector ps(*pDC, m_clrEditBoxBorder);

		int y = rect.CenterPoint().y;

		pDC->MoveTo (rect.left, y);
		pDC->LineTo (rect.right, y);

		return;
	}

#endif

	CBCGPVisualManager2013::OnDrawSeparator(pDC, pBar, rect, bIsHoriz);
}
//*********************************************************************************************************
COLORREF CBCGPVisualManager2016::OnDrawControlBarCaption (CDC* pDC, CBCGPDockingControlBar* pBar, 
			BOOL bActive, CRect rectCaption, CRect rectButtons)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::OnDrawControlBarCaption (pDC, pBar, bActive, rectCaption, rectButtons);
	}

	ASSERT_VALID (pDC);

	rectCaption.bottom++;

	pDC->FillRect(rectCaption, &globalData.brBarFace);

	return bActive ? m_clrAccent : globalData.clrBarShadow;
}
//************************************************************************************
COLORREF CBCGPVisualManager2016::OnFillMiniFrameCaption (CDC* pDC, 
								CRect rectCaption, 
								CBCGPMiniFrameWnd* pFrameWnd,
								BOOL bActive)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pFrameWnd);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::OnFillMiniFrameCaption (pDC, rectCaption, pFrameWnd, bActive);
	}

	if (DYNAMIC_DOWNCAST (CBCGPBaseToolBar, pFrameWnd->GetControlBar ()) != NULL)
	{
		bActive = FALSE;
	}

	pDC->FillRect(rectCaption, &globalData.brBarFace);
	
	return bActive ? m_clrAccent : globalData.clrBarShadow;
}
//************************************************************************************
void CBCGPVisualManager2016::OnDrawMiniFrameBorder (
										CDC* pDC, CBCGPMiniFrameWnd* pFrameWnd,
										CRect rectBorder, CRect rectBorderSize)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		CBCGPVisualManager2013::OnDrawMiniFrameBorder (pDC, pFrameWnd, rectBorder, rectBorderSize);
		return;
	}

	ASSERT_VALID (pDC);

	CRect rectClip = rectBorder;
	rectClip.DeflateRect(rectClip);

	pDC->ExcludeClipRect(rectClip);

	pDC->FillRect(rectBorder, &globalData.brBarFace);

	if (pFrameWnd->GetSafeHwnd() != NULL && pFrameWnd->IsActive())
	{
		pDC->Draw3dRect(rectBorder, m_clrAccent, m_clrAccent);
	}
	else
	{
		pDC->Draw3dRect(rectBorder, m_clrMenuBorder, m_clrMenuBorder);
	}

	pDC->SelectClipRgn (NULL);
}
//**********************************************************************************
void CBCGPVisualManager2016::OnDrawCaptionButton (CDC* pDC, CBCGPCaptionButton* pButton, 
								BOOL bActive, BOOL bHorz, BOOL bMaximized, BOOL bDisabled,
								int nImageID /*= -1*/)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnDrawCaptionButton (pDC, pButton, bActive, bHorz, bMaximized, bDisabled, nImageID);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

    CRect rc = pButton->GetRect ();

	const BOOL bHighlight = (pButton->m_bFocused || pButton->m_bDroppedDown) && !bDisabled;
	
	if (pButton->m_bPushed && bHighlight)
	{
		CBrush br(CBCGPDrawManager::ColorMakeDarker(m_clrAccentLight));
		pDC->FillRect (rc, m_Style == Office2016_DarkGray ? &m_brAccent : &br);
	}
	else if (bHighlight || pButton->m_bPushed)
	{
		pDC->FillRect (rc, m_Style == Office2016_DarkGray ? &m_brAccentHighlight : &m_brAccentLight);
	}
	
	CBCGPMenuImages::IMAGES_IDS id = (CBCGPMenuImages::IMAGES_IDS)-1;
	
	if (nImageID != -1)
	{
		id = (CBCGPMenuImages::IMAGES_IDS)nImageID;
	}
	else
	{
		id = pButton->GetIconID (bHorz, bMaximized);
	}
	
	if (id != (CBCGPMenuImages::IMAGES_IDS)-1)
	{
		CSize sizeImage = CBCGPMenuImages::Size ();
		CPoint ptImage (rc.left + (rc.Width () - sizeImage.cx) / 2,
			rc.top + (rc.Height () - sizeImage.cy) / 2);
		
		OnDrawCaptionButtonIcon (pDC, pButton, id, bActive, bDisabled, ptImage);
	}
}
//**************************************************************************************
void CBCGPVisualManager2016::OnDrawCaptionButtonIcon (CDC* pDC, 
													CBCGPCaptionButton* pButton,
													CBCGPMenuImages::IMAGES_IDS id,
													BOOL bActive, BOOL bDisabled,
													CPoint ptImage)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		CBCGPVisualManager2013::OnDrawCaptionButtonIcon (pDC, pButton, id, bActive, bDisabled, ptImage);
		return;
	}

	ASSERT_VALID (pDC);
	ASSERT_VALID (pButton);

	CBCGPMenuImages::IMAGE_STATE imageState = CBCGPMenuImages::ImageBlack;
	
	if (bDisabled)
	{
		imageState = CBCGPMenuImages::ImageLtGray;
	}
	else if (pButton->m_bFocused || pButton->m_bPushed || pButton->m_bDroppedDown)
	{
		imageState = CBCGPMenuImages::ImageWhite;
	}

	CBCGPMenuImages::Draw (pDC, id, ptImage, imageState);
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::GetDlgTextColor(CWnd* pDlg)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::GetDlgTextColor(pDlg);
	}

	return RGB(240, 240, 240);
}
//****************************************************************************************
COLORREF CBCGPVisualManager2016::GetURLLinkColor (CBCGPURLLinkButton* pButton, BOOL bHover)
{
	ASSERT_VALID (pButton);

	if (!pButton->m_bVisualManagerStyle || pButton->m_bOnGlass  || globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::GetURLLinkColor (pButton, bHover);
	}

	if (pButton->GetSafeHwnd() != NULL && !pButton->IsWindowEnabled())
	{
		return m_clrTextDisabled;
	}
	
	return bHover ? CBCGPDrawManager::ColorMakeDarker(m_clrAccentLight) : m_clrAccentLight;
}

#ifndef BCGP_EXCLUDE_PROP_LIST

void CBCGPVisualManager2016::OnFillPropListToolbarArea(CDC* pDC, CBCGPPropList* pPropList, const CRect& rectToolBar)
{
	if ((pPropList != NULL && !pPropList->DrawControlBarColors()) || globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnFillPropListToolbarArea(pDC, pPropList, rectToolBar);
		return;
	}

	pDC->FillSolidRect(rectToolBar, m_clrToolBarGradientLight);
}
//****************************************************************************************
COLORREF CBCGPVisualManager2016::OnFillPropertyListSelectedItem(CDC* pDC, CBCGPProp* pProp, CBCGPPropList* pWndList, const CRect& rectFill, BOOL bFocused)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID(pProp);

	if ((pWndList != NULL && !pWndList->DrawControlBarColors()) || globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnFillPropertyListSelectedItem(pDC, pProp, pWndList, rectFill, bFocused);
	}

	COLORREF clrText = (COLORREF)-1;

	if (bFocused)
	{
		pDC->FillRect (rectFill, &m_brHighlight);
		
		if (pProp->IsEnabled())
		{
			clrText = globalData.clrBarText;
		}
		else
		{
			clrText = m_clrTextDisabled;
		}
	}
	else
	{
		pDC->FillSolidRect(rectFill, m_clrToolBarGradientLight);
		clrText = pProp->IsEnabled() ? globalData.clrBarText : m_clrTextDisabled;
	}

	return clrText;
}
//****************************************************************************************
COLORREF CBCGPVisualManager2016::GetPropListGroupTextColor(CBCGPPropList* pPropList)
{
	ASSERT_VALID (pPropList);

	if (!pPropList->DrawControlBarColors () || globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetPropListGroupTextColor(pPropList);
	}

	return globalData.clrBarText;
}
//***********************************************************************************
COLORREF CBCGPVisualManager2016::OnFillPropListDescriptionArea(CDC* pDC, CBCGPPropList* pList, const CRect& rect)
{
	ASSERT_VALID(pDC);
	ASSERT_VALID(pList);

	if (!pList->DrawControlBarColors () || globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnFillPropListDescriptionArea(pDC, pList, rect);
	}

	pDC->FillSolidRect(rect, m_clrToolBarGradientLight);
	return m_clrMenuBorder;
}
//***********************************************************************************
COLORREF CBCGPVisualManager2016::OnFillPropListCommandsArea(CDC* pDC, CBCGPPropList* pList, const CRect& rect)
{
	ASSERT_VALID(pDC);
	ASSERT_VALID(pList);

	if (!pList->DrawControlBarColors () || globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnFillPropListCommandsArea(pDC, pList, rect);
	}

	pDC->FillSolidRect(rect, m_clrToolBarGradientLight);
	return m_clrMenuBorder;
}
//***********************************************************************************
COLORREF CBCGPVisualManager2016::GetPropListCommandTextColor (CBCGPPropList* pPropList)
{	
	ASSERT_VALID (pPropList);

	if (!pPropList->DrawControlBarColors () || globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetPropListCommandTextColor (pPropList);
	}

	return m_clrAccentText;
}
//********************************************************************************
COLORREF CBCGPVisualManager2016::GetPropListGroupColor(CBCGPPropList* pPropList)
{
	ASSERT_VALID (pPropList);
	
	if (!pPropList->DrawControlBarColors () || globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetPropListGroupColor (pPropList);
	}
	
	return m_clrToolBarGradientLight;
}
//********************************************************************************
COLORREF CBCGPVisualManager2016::GetPropListDisabledTextColor(CBCGPPropList* pPropList)
{
	if (!pPropList->DrawControlBarColors () || globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetPropListDisabledTextColor(pPropList);
	}
	
	return m_clrTextDisabled;
}

#endif

void CBCGPVisualManager2016::OnDrawHeaderCtrlBorder (CBCGPHeaderCtrl* pCtrl, CDC* pDC, CRect& rect, BOOL bIsPressed, BOOL bIsHighlighted)
{
	BOOL bIsVMStyle = (pCtrl == NULL || pCtrl->m_bVisualManagerStyle);
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || !bIsVMStyle || m_Style != Office2016_DarkGray)
	{
		CBCGPVisualManager2013::OnDrawHeaderCtrlBorder (pCtrl, pDC, rect, bIsPressed, bIsHighlighted);
		return;
	}

	if (bIsPressed)
	{
		pDC->FillRect(rect, &m_brHighlightDn);
	}
	else if (bIsHighlighted)
	{
		pDC->FillRect(rect, &m_brHighlight);
	}
	else
	{
		pDC->FillRect(rect, &globalData.brBarFace);
	}

	pDC->Draw3dRect(rect, m_clrMenuBorder, m_clrMenuBorder);
}
//*******************************************************************************
static BOOL IsSpecialBackStageListBoxLook(CBCGPListBox* pListBox)
{
	return pListBox->IsBackstagePageSelector() || (pListBox->IsBackstageMode() && pListBox->IsRecentFilesListBox());
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::OnFillListBox(CDC* pDC, CBCGPListBox* pListBox, CRect rectClient)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White || !IsSpecialBackStageListBoxLook(pListBox))
	{
		return CBCGPVisualManager2013::OnFillListBox(pDC, pListBox, rectClient);
	}

	ASSERT_VALID (pDC);

	pDC->FillRect(rectClient, &GetDlgBackBrush(pListBox->GetParent()));
#ifndef BCGP_EXCLUDE_RIBBON
	return GetRibbonBackstageTextColor();
#else
	return CBCGPVisualManager2013::OnFillListBox(pDC, pListBox, rectClient);
#endif
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::OnFillListBoxItem(CDC* pDC, CBCGPListBox* pListBox, int nItem, CRect rect, BOOL bIsHighlihted, BOOL bIsSelected)
{
	ASSERT_VALID(pListBox);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnFillListBoxItem (pDC, pListBox, nItem, rect, bIsHighlihted, bIsSelected);
	}

	ASSERT_VALID (pDC);

	BOOL bIsEnabled = pListBox->IsEnabled(nItem);
	COLORREF clrFill = (COLORREF)-1;
	COLORREF clrText = (COLORREF)-1;

	if (!IsSpecialBackStageListBoxLook(pListBox) && bIsEnabled)
	{
		return CBCGPVisualManager2013::OnFillListBoxItem (pDC, pListBox, nItem, rect, bIsHighlihted, bIsSelected);
	}

	if (!bIsEnabled)
	{
		if (bIsHighlihted || bIsSelected)
		{
			clrFill = CBCGPDrawManager::ColorMakeDarker(m_clrControl);
		}
	}
	else
	{
		if (bIsHighlihted)
		{
			clrFill = bIsSelected ? m_clrAccentHilight : m_clrAccentText;
			clrText = RGB(255, 255, 255);
		}
		else if (bIsSelected)
		{
			clrFill = m_clrAccentHilight;
			clrText = RGB(255, 255, 255);
		}
	}

	if (clrFill != (COLORREF)-1)
	{
		CBrush br(clrFill);

		rect.DeflateRect(1, 0);
		pDC->FillRect(rect, &br);
	}

	return clrText;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::GetEditCtrlSelectionBkColor(CBCGPEditCtrl* pEdit, BOOL bIsFocused)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetEditCtrlSelectionBkColor (pEdit, bIsFocused);
	}

	return bIsFocused ? m_clrHighlight : (IsDarkTheme() ? globalData.clrBarLight : globalData.clrBarShadow);
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::GetEditCtrlSelectionTextColor(CBCGPEditCtrl* pEdit, BOOL bIsFocused)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetEditCtrlSelectionTextColor (pEdit, bIsFocused);
	}

	return globalData.clrBarText;
}
//*******************************************************************************
HBRUSH CBCGPVisualManager2016::GetEditBackgroundBrush(CBCGPEditCtrl* pEdit)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetEditBackgroundBrush(pEdit);
	}

	return m_brControl;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::GetEditTextColor(CBCGPEditCtrl* pEdit)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetEditTextColor(pEdit);
	}

	return globalData.clrBarText;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::GetEditLineNumbersBarTextColor(CBCGPEditCtrl* pEdit)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetEditLineNumbersBarTextColor (pEdit);
	}

	return m_Style == Office2016_DarkGray ? globalData.clrBarShadow : globalData.clrBarText;
}

#ifndef BCGP_EXCLUDE_GRID_CTRL

COLORREF CBCGPVisualManager2016::OnFillReportCtrlRowBackground (CBCGPGridCtrl* pCtrl, CDC* pDC,
		CRect rectFill, BOOL bSelected, BOOL bGroup)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pCtrl);
	
	COLORREF clrText = CBCGPVisualManager2013::OnFillReportCtrlRowBackground(pCtrl, pDC, rectFill, bSelected, bGroup);
	
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode())
	{
		return clrText;
	}
	
	if (m_Style != Office2016_White)
	{
		rectFill.bottom ++;
		if (bSelected)
		{
			pDC->FillSolidRect(rectFill, m_clrHighlightMenuItem);
		}
		else
		{
			pDC->FillRect(rectFill, &GetGridCaptionBrush(pCtrl));
		}
		clrText = globalData.clrBarText;
	}
	
	return clrText;
}
//*******************************************************************************
void CBCGPVisualManager2016::OnFillGridGroupBackground (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rectFill)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode())
	{
		CBCGPVisualManager2013::OnFillGridGroupBackground(pCtrl, pDC, rectFill);
		return;
	}
	
	ASSERT_VALID(pCtrl);
	ASSERT_VALID (pDC);

	if (m_Style != Office2016_White)
	{
		rectFill.bottom ++;
		pDC->FillRect(rectFill, pCtrl->IsVisualManagerStyle() ? &m_brBarBkgnd : &GetGridCaptionBrush(pCtrl));
	}
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::OnFillGridGroupByBoxBackground (CDC* pDC, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::OnFillGridGroupByBoxBackground(pDC, rect);
	}

	pDC->FillSolidRect(rect, IsDarkTheme() ? m_clrControl : m_clrButtonsArea);
	return globalData.clrBarText;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::GetGridGroupByBoxLineColor () const
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetGridGroupByBoxLineColor();
	}

	return globalData.clrBarShadow;
}
//*******************************************************************************
void CBCGPVisualManager2016::OnDrawGridGroupByBoxItemBorder (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnDrawGridGroupByBoxItemBorder(pCtrl, pDC, rect);
		return;
	}

	ASSERT_VALID (pDC);

	if (IsDarkTheme())
	{
		pDC->FillSolidRect(rect, CBCGPDrawManager::ColorMakeLighter(m_clrControl));
		pDC->Draw3dRect (rect, globalData.clrBarFace, globalData.clrBarFace);
	}
	else
	{
		pDC->FillRect(rect, &globalData.brBarFace);
		pDC->Draw3dRect (rect, globalData.clrBarShadow, globalData.clrBarShadow);
	}
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::OnFillGridItem (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rectFill,
								 BOOL bSelected, BOOL bActiveItem, BOOL bSortedColumn)
{
	ASSERT_VALID (pDC);
	ASSERT_VALID (pCtrl);

	COLORREF clrText = CBCGPVisualManager2013::OnFillGridItem (pCtrl, pDC, rectFill, bSelected, bActiveItem, bSortedColumn);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode())
	{
		return clrText;
	}

	if (m_Style != Office2016_White)
	{
		// Fill area:
		if (bSelected && !bActiveItem)
		{
			pDC->FillSolidRect(rectFill, m_clrHighlightMenuItem);
			clrText = globalData.clrBarText;
		}
		else if (!pCtrl->IsFocused() && bSelected)
		{
			pDC->FillSolidRect(rectFill, globalData.clrBarFace);
			clrText = globalData.clrBarText;
		}

	}
	
	return clrText;
}
//*******************************************************************************
void CBCGPVisualManager2016::OnFillGridHeaderBackground (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect)
{
	ASSERT_VALID(pCtrl);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || !(pCtrl->IsVisualManagerStyle() && IsDarkTheme()))
	{
		CBCGPVisualManager2013::OnFillGridHeaderBackground(pCtrl, pDC, rect);
		return;
	}

	pDC->FillRect (rect, &globalData.brBarFace);
}
//*******************************************************************************
BOOL CBCGPVisualManager2016::OnDrawGridHeaderItemBorder (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect, BOOL bPressed)
{
	ASSERT_VALID(pCtrl);

	BOOL bRes = CBCGPVisualManager2013::OnDrawGridHeaderItemBorder(pCtrl, pDC, rect, bPressed);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || ! IsDarkTheme())
	{
		return bRes;
	}

	if (bPressed && pCtrl->IsVisualManagerStyle())
	{
		bRes = FALSE;

		CBrush br;
		br.CreateSolidBrush(globalData.clrBarDkShadow);

		pDC->FillRect (rect, &br);
	}
	else
	{
		CBCGPPenSelector pen(*pDC, IsDarkTheme() ? globalData.clrBarShadow : globalData.clrBarDkShadow);
		
		pDC->MoveTo(rect.left, rect.bottom - 1);
		pDC->LineTo(rect.right, rect.bottom - 1);
	}

	return bRes;
}
//*******************************************************************************
void CBCGPVisualManager2016::OnFillGridRowHeaderBackground (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect)
{
	ASSERT_VALID(pCtrl);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || !(pCtrl->IsVisualManagerStyle() && IsDarkTheme()))
	{
		CBCGPVisualManager2013::OnFillGridRowHeaderBackground(pCtrl, pDC, rect);
		return;
	}

	pDC->FillRect(rect, &globalData.brBarFace);
}
//*******************************************************************************
BOOL CBCGPVisualManager2016::OnDrawGridRowHeaderItemBorder (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect, BOOL bPressed)
{
	ASSERT_VALID(pCtrl);

	BOOL bRes = CBCGPVisualManager2013::OnDrawGridRowHeaderItemBorder(pCtrl, pDC, rect, bPressed);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || !(pCtrl->IsVisualManagerStyle() && IsDarkTheme()))
	{
		return bRes;
	}

	if (bPressed)
	{
		CBrush br;
		br.CreateSolidBrush(globalData.clrBarDkShadow);
		
		pDC->FillRect (rect, &br);
	}

	return FALSE;
}
//*******************************************************************************
void CBCGPVisualManager2016::OnFillGridSelectAllAreaBackground (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect, BOOL bPressed)
{
	ASSERT_VALID(pCtrl);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || !pCtrl->IsVisualManagerStyle())
	{
		CBCGPVisualManager2013::OnFillGridSelectAllAreaBackground(pCtrl, pDC, rect, bPressed);
		return;
	}

	if (!bPressed)
	{
		pDC->FillRect (rect, &globalData.brBarFace);
	}
	else
	{
		CBrush br;
		br.CreateSolidBrush(globalData.clrBarDkShadow);
		
		pDC->FillRect (rect, &br);
	}
}
//*******************************************************************************
void CBCGPVisualManager2016::OnDrawGridSelectAllMarker(CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect, int nPadding, BOOL bPressed)
{
	if (m_Style != Office2016_White && bPressed)
	{
		CBCGPVisualManager::OnDrawGridSelectAllMarker(pCtrl, pDC, rect, nPadding, bPressed);	
	}
	else
	{
		CBCGPVisualManager2013::OnDrawGridSelectAllMarker(pCtrl, pDC, rect, nPadding, bPressed);
	}
}
//*******************************************************************************
BOOL CBCGPVisualManager2016::OnDrawGridSelectAllAreaBorder (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect, BOOL bPressed)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode() || !bPressed)
	{
		return CBCGPVisualManager2013::OnDrawGridSelectAllAreaBorder(pCtrl, pDC, rect, bPressed);
	}

	return TRUE;
}
//*******************************************************************************
void CBCGPVisualManager2016::OnDrawGridSelectionBorder (CBCGPGridCtrl* pCtrl, CDC* pDC, CRect rect)
{
	ASSERT_VALID (pCtrl);
	ASSERT_VALID (pDC);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode())
	{
		CBCGPVisualManager2013::OnDrawGridSelectionBorder(pCtrl, pDC, rect);
		return;
	}

	COLORREF clrBorder = (pCtrl->IsVisualManagerStyle() || m_Style != Office2016_DarkGray) ? m_clrAccentHilight : m_clrHighlightDn;

	pDC->Draw3dRect (rect, clrBorder, clrBorder);
	rect.DeflateRect (1, 1);
	pDC->Draw3dRect (rect, clrBorder, clrBorder);
}
//*******************************************************************************
BOOL CBCGPVisualManager2016::OnSetGridColorTheme (CBCGPGridCtrl* pCtrl, BCGP_GRID_COLOR_DATA& theme)
{
	BOOL bRes = CBCGPVisualManager2013::OnSetGridColorTheme (pCtrl, theme);
	
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode ())
	{
		return bRes;
	}

	theme.m_clrBackground = m_clrControl;
	if (m_Style != Office2016_White)
	{
		theme.m_clrHeader = IsDarkTheme() ? globalData.clrBarLight : globalData.clrBarFace;
		theme.m_HeaderColors.m_clrBackground = IsDarkTheme() ? globalData.clrBarLight : globalData.clrBarFace;
		theme.m_HeaderColors.m_clrText = IsDarkTheme() ? RGB(255, 255, 255) : globalData.clrBarText;
		theme.m_HeaderColors.InitColors();
		theme.m_HeaderSelColors.m_clrBackground = IsDarkTheme() ?  globalData.clrBarShadow : globalData.clrBarShadow;
		theme.m_HeaderSelColors.m_clrText = IsDarkTheme() ? RGB(255, 255, 255) : globalData.clrBarText;
		theme.m_SelColors.m_clrBackground = m_clrHighlight;
		theme.m_SelColors.m_clrText = globalData.clrBarText;

		theme.m_GroupColors.m_clrBackground = IsDarkTheme() ? 
			CBCGPDrawManager::ColorMakeLighter(globalData.clrBarLight, .2) : 
			CBCGPDrawManager::ColorMakeDarker(globalData.clrBarLight, .1);

		theme.m_GroupColors.m_clrGradient = (COLORREF)-1;
		theme.m_GroupColors.m_clrBorder = (COLORREF)-1;
		theme.m_GroupColors.m_clrText = globalData.clrBarText;
		theme.m_GroupSelColors.m_clrBackground = theme.m_GroupColors.m_clrBackground;
		theme.m_GroupSelColors.m_clrGradient = theme.m_GroupColors.m_clrGradient;
		theme.m_GroupSelColors.m_clrBorder = (COLORREF)-1;
		theme.m_GroupSelColors.m_clrText = theme.m_HeaderColors.m_clrText;
	}

	if (IsDarkTheme())
	{
		theme.m_HeaderColors.m_clrBorder = (COLORREF)-1;
		theme.m_HeaderSelColors.m_clrBorder = (COLORREF)-1;

		theme.m_LeftOffsetColors.m_clrBackground = CBCGPDrawManager::ColorMakeLighter(globalData.clrBarLight, .15);
	}

 	theme.m_EvenColors.InitColors();
 	theme.m_OddColors.m_clrBackground = CBCGPDrawManager::ColorMakeDarker(theme.m_clrBackground, .04);

	return bRes;
}

#endif // BCGP_EXCLUDE_GRID_CTRL

#ifndef BCGP_EXCLUDE_POPUP_WINDOW

COLORREF CBCGPVisualManager2016::GetPopupWindowLinkTextColor(CBCGPPopupWindow* pPopupWnd, BOOL bIsHover)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::GetPopupWindowLinkTextColor(pPopupWnd, bIsHover);
	}

	return bIsHover ? CBCGPDrawManager::ColorMakeDarker(m_clrAccentLight) : m_clrAccentLight;
}
//**********************************************************************************
COLORREF CBCGPVisualManager2016::OnDrawPopupWindowCaption (CDC* pDC, CRect rectCaption, CBCGPPopupWindow* pPopupWnd)
{
	COLORREF clrText = CBCGPVisualManager2013::OnDrawPopupWindowCaption (pDC, rectCaption, pPopupWnd);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return clrText;
	}

	return GetDlgTextColor(pPopupWnd);
}
//**********************************************************************************
COLORREF CBCGPVisualManager2016::GetPopupWindowCaptionTextColor(CBCGPPopupWindow* pPopupWnd, BOOL bButton)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetPopupWindowCaptionTextColor(pPopupWnd, bButton);
	}

	return GetDlgTextColor(pPopupWnd);
}
//**********************************************************************************
COLORREF CBCGPVisualManager2016::GetPopupWindowTextColor(CBCGPPopupWindow* pPopupWnd)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetPopupWindowTextColor(pPopupWnd);
	}

	return GetDlgTextColor(pPopupWnd);
}

#endif	// BCGP_EXCLUDE_POPUP_WINDOW

CBrush& CBCGPVisualManager2016::GetDlgButtonsAreaBrush(CWnd* pDlg, COLORREF* pclrLine)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetDlgButtonsAreaBrush (pDlg, pclrLine);
	}

	if (pclrLine != NULL)
	{
		*pclrLine = globalData.clrBarShadow;
	}

	return m_brDlgButtonsArea;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::GetDlgHeaderTextColor(CWnd* pDlg)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray)
	{
		return CBCGPVisualManager2013::GetDlgHeaderTextColor(pDlg);
	}
	
	return m_clrAccentLight;
}
//*****************************************************************************
void CBCGPVisualManager2016::OnDrawComboDropButton (CDC* pDC, CRect rect,
												BOOL bDisabled,
												BOOL bIsDropped,
												BOOL bIsHighlighted,
												CBCGPToolbarComboBoxButton* pButton)
{
	ASSERT_VALID(pDC);
	ASSERT_VALID (this);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || CBCGPToolBarImages::m_bIsDrawOnGlass || bDisabled || m_Style != Office2016_DarkGray)
	{
		CBCGPVisualManager2013::OnDrawComboDropButton (pDC, rect, bDisabled, bIsDropped, bIsHighlighted, pButton);
		return;
	}

	if (bIsDropped)
	{
		pDC->FillRect(rect, &m_brHighlightDn);
	}
	else if (bIsHighlighted)
	{
		pDC->FillSolidRect(rect, m_clrHighlight);
	}
	else
	{
		pDC->FillSolidRect (rect, m_clrCombo);
	}

	if (bIsHighlighted || bIsDropped)
	{
		CBCGPPenSelector ps(*pDC, m_clrEditBoxBorder);
		
		int x = rect.left;
		
		pDC->MoveTo (x, rect.top);
		pDC->LineTo (x, rect.bottom);
	}

	CBCGPMenuImages::Draw(pDC, CBCGPMenuImages::IdArowDown, rect);
}
//*************************************************************************************
void CBCGPVisualManager2016::OnDrawSpinButtons (CDC* pDC, CRect rectSpin, 
											  int nState, BOOL bOrientation, CBCGPSpinButtonCtrl* pSpinCtrl)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::OnDrawSpinButtons (pDC, rectSpin, nState, bOrientation, pSpinCtrl);
		return;
	}

	ASSERT_VALID(pDC);
	ASSERT_VALID (this);
	
	BOOL bDisabled = (nState & SPIN_DISABLED);

	CRect rect [2];
	rect[0] = rect[1] = rectSpin;
	
	if (!bOrientation) 
	{
		rect[0].DeflateRect(0, 0, 0, rect[0].Height() / 2);
		rect[1].top = rect[0].bottom ;
	}
	else
	{
		rect[1].DeflateRect(0, 0, rect[0].Width() / 2, 0);
		rect[0].left = rect[1].right;
	}
	
	CBCGPMenuImages::IMAGES_IDS id[2][2] = {{CBCGPMenuImages::IdArowUp, CBCGPMenuImages::IdArowDown}, {CBCGPMenuImages::IdArowRight, CBCGPMenuImages::IdArowLeft}};
	
	int idxPressed = bDisabled ? -1 : (nState & (SPIN_PRESSEDUP | SPIN_PRESSEDDOWN)) - 1;
	
	int idxHighlighted = -1;
	if (!bDisabled)
	{
		if (nState & SPIN_HIGHLIGHTEDUP)
		{
			idxHighlighted = 0;
		}
		else if (nState & SPIN_HIGHLIGHTEDDOWN)
		{
			idxHighlighted = 1;
		}
	}
	
	for (int i = 0; i < 2; i ++)
	{
		CBCGPMenuImages::IMAGE_STATE state = bDisabled ? CBCGPMenuImages::ImageLtGray : CBCGPMenuImages::ImageDkGray;

		COLORREF clrBorder = m_clrMenuItemBorder;

		if (idxPressed == i || idxHighlighted == i)
		{
			OnFillHighlightedArea (pDC, rect [i], (idxPressed == i) ? &m_brHighlightDn : &m_brHighlight, NULL);

			state = CBCGPMenuImages::ImageBlack;
			clrBorder = m_clrBorderActive;
		}
		else
		{
			OnFillSpinButton (pDC, pSpinCtrl, rect [i], bDisabled);
		}

		CBCGPMenuImages::Draw (pDC, id[bOrientation ? 1 : 0][i], rect[i], state);
	}
}
//*********************************************************************************
CBrush& CBCGPVisualManager2016::GetEditCtrlBackgroundBrush(CBCGPEdit* pEdit)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetEditCtrlBackgroundBrush(pEdit);
	}

	if (pEdit->GetSafeHwnd() != NULL && ((pEdit->GetStyle() & ES_READONLY) == ES_READONLY || !pEdit->IsWindowEnabled()))
	{
		return m_brHighlight;
	}
	
	return m_brControl;
}

#if !defined (BCGP_EXCLUDE_GRID_CTRL) && !defined (BCGP_EXCLUDE_GANTT)

void CBCGPVisualManager2016::GetGanttColors (const CBCGPGanttChart* pChart, BCGP_GANTT_CHART_COLORS& colors, COLORREF clrBack) const
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		CBCGPVisualManager2013::GetGanttColors(pChart, colors, clrBack);
		return;
	}

	if (pChart->GetSafeHwnd() != NULL && !pChart->IsVisualManagerStyle())
	{
		CBCGPVisualManager2013::GetGanttColors(pChart, colors, clrBack);
		return;
	}

	BOOL bIsDefault = FALSE;

	if (clrBack == CLR_DEFAULT)
	{
		clrBack = globalData.clrBarHilite;
		bIsDefault = TRUE;
	}	

    colors.clrBackground      = globalData.clrBarHilite;
	colors.clrShadows         = m_clrMenuShadowBase;

	colors.clrRowBackground   = colors.clrBackground;

	colors.clrBarFill         = m_clrAccentLight;
	colors.clrBarComplete     = m_clrAccent;

	colors.clrRowBackground  = CBCGPDrawManager::SmartMixColors(globalData.clrBarLight, globalData.clrBarHilite);

	if (bIsDefault || IsDarkTheme())
	{
		colors.clrRowDayOff = CBCGPDrawManager::ColorMakeDarker(colors.clrRowBackground, .08);
	}
	else
	{
		colors.clrRowDayOff = CBCGPDrawManager::ColorMakePale(clrBack, .9);
	}

	colors.clrConnectorLines = globalData.clrBarDkShadow;

	colors.clrGridLine0 = IsDarkTheme() ? m_clrSeparator : CalculateHourLineColor (colors.clrRowDayOff, TRUE, TRUE);
	colors.clrGridLine1 = IsDarkTheme() ? m_clrSeparator : CalculateHourLineColor (colors.clrRowDayOff, TRUE, !bIsDefault);

	colors.clrSelection = m_clrAccentLight;
	colors.clrSelectionBorder = m_clrAccentText;
}

#endif // !defined (BCGP_EXCLUDE_GRID_CTRL) && !defined (BCGP_EXCLUDE_GANTT)

COLORREF CBCGPVisualManager2016::GetTreeControlFillColor(CBCGPTreeCtrl* pTreeCtrl, BOOL bIsSelected, BOOL bIsFocused, BOOL bIsDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White || !bIsSelected || bIsDisabled)
	{
		return CBCGPVisualManager2013::GetTreeControlFillColor(pTreeCtrl, bIsSelected, bIsFocused, bIsDisabled);
	}

	if (m_Style == Office2016_Colorful)
	{
		return bIsFocused ? m_clrHighlight : globalData.clrBarLight;
	}

	return bIsFocused ? m_clrHighlight : m_clrToolBarGradientLight;
}
//*******************************************************************************
COLORREF CBCGPVisualManager2016::GetTreeControlTextColor(CBCGPTreeCtrl* pTreeCtrl, BOOL bIsSelected, BOOL bIsFocused, BOOL bIsDisabled)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White || !bIsSelected || bIsDisabled)
	{
		return CBCGPVisualManager2013::GetTreeControlTextColor(pTreeCtrl, bIsSelected, bIsFocused, bIsDisabled);
	}

	return globalData.clrBarText;
}
//*********************************************************************************
COLORREF CBCGPVisualManager2016::GetIntelliSenseFillColor(CBCGPBaseIntelliSenseLB* pCtrl, BOOL bIsSelected)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetIntelliSenseFillColor(pCtrl, bIsSelected);
	}
	
	return bIsSelected ? m_clrHighlight : m_clrControl;
}
//*********************************************************************************
COLORREF CBCGPVisualManager2016::GetIntelliSenseTextColor(CBCGPBaseIntelliSenseLB* pCtrl, BOOL bIsSelected)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style == Office2016_White)
	{
		return CBCGPVisualManager2013::GetIntelliSenseTextColor(pCtrl, bIsSelected);
	}

	return globalData.clrBarText;
}
//**********************************************************************************
void CBCGPVisualManager2016::OnDrawTabContent (CDC* pDC, CRect rectTab,
							int iTab, BOOL bIsActive, const CBCGPBaseTabWnd* pTabWnd,
							COLORREF clrText)
{
	ASSERT_VALID(pTabWnd);

	BOOL bIsHighlighted = pTabWnd->GetHighlightedTab() == iTab;
	COLORREF clrTextSaved = clrText;

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode () || m_Style != Office2016_DarkGray ||
		pTabWnd->GetTabTextColor(iTab) != (COLORREF)-1 || pTabWnd->IsOneNoteStyle () || pTabWnd->IsVS2005Style ())
	{
		CBCGPVisualManager2013::OnDrawTabContent (pDC, rectTab, iTab, bIsActive, pTabWnd, clrText);
		return;
	}

	if (pTabWnd->IsPointerStyle() && !pTabWnd->IsMDITab())
	{
		clrText = (bIsActive || bIsHighlighted) ? m_clrAccentLight : GetDlgTextColor(pTabWnd->GetParent());
	}
	else if (pTabWnd->Is3DStyle() && !bIsActive && !bIsHighlighted && pTabWnd->IsDialogControl())
	{
		clrText = GetDlgTextColor(pTabWnd->GetParent());
	}

	if (clrTextSaved != clrText)
	{
		CBCGPVisualManagerXP::OnDrawTabContent (pDC, rectTab, iTab, bIsActive, pTabWnd, clrText);
	}
	else
	{
		CBCGPVisualManager2013::OnDrawTabContent (pDC, rectTab, iTab, bIsActive, pTabWnd, clrText);
	}
}
