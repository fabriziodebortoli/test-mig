#include "stdafx.h"

#include "MicroareaVisualManager.h"
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\TBThemeManager.h>
#include "TBToolBar.h"
#include "TBTabWnd.h"
#include "TBDockPane.h"
#include "TBAutoHideBar.h"
#include "TBPropertyGrid.h"
#include "PARSOBJ.H"
#include "PARSEDT.H"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define SCROLLBARSPACE	2

/////////////////////////////////////////////////////////////////////////////
//						BCGP Visual Manager
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CMicroareaVisualManager, CBCGPVisualManager2013)
//-----------------------------------------------------------------------------
CMicroareaVisualManager::CMicroareaVisualManager()
{
	SetStyle(CBCGPVisualManager2013::Office2013_Gray);

	DataObj* pDataObj = AfxGetSettingValue(snsTbGenlib, szFormsSection, szTabbedDocuments, DataBool(TRUE), szTbDefaultSettingFileName);
	m_bUseTabbedDocuments = pDataObj ? (*((DataBool*)(pDataObj))) : FALSE;

	CBCGPDockManager::m_pAutoHideToolbarRTC = RUNTIME_CLASS(CTBAutoHideBar);
	globalData.m_nCoveredMainWndClientAreaPercent = 100;
	
	//disabilito le funzionalità grafiche nel caso di IIS, nessuno le vedrà
	if (AfxGetApplicationContext()->IsIISModule())
		globalData.m_bShowFrameLayeredShadows = FALSE;
}

//-----------------------------------------------------------------------------
CMicroareaVisualManager::~CMicroareaVisualManager()
{

}

//--------------------------------------------------------------------------------
void CMicroareaVisualManager::SetupColors()
{
	__super::SetupColors();

	m_clrDefaultAccentText = m_clrAccentText;
}

//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::IsOwnerDrawCaption()
{
	// questo overload serve altrimenti il tema 2013 
	// toglie il child window e sdocca la CMasterView!
	return !m_bUseTabbedDocuments;
}

//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::IsMinFrameCaptionBold(CBCGPMiniFrameWnd* pFrame)
{
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::UseLargeCaptionFontInDockingCaptions()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
COLORREF CMicroareaVisualManager::BreadcrumbFillBackground(CDC& dc, CBCGPBreadcrumb* pControl, CRect rectFill)
{
	COLORREF color = pControl->GetBackColor();
	CBrush br(color);
	dc.FillRect(&rectFill, &br);

	return pControl->GetDefaultTextColor();

}

//----------------------------------------------------------------------------------------------------------
void CMicroareaVisualManager::OnDrawAutohideBar(CDC* pDC, CRect rectBar, CBCGPAutoHideButton* pButton)
{
	COLORREF oldColor = m_clrHighlightDn;

	if (AfxGetThemeManager()->GetAutoHideBarAccentColor() != -1)
		m_clrHighlightDn = AfxGetThemeManager()->GetAutoHideBarAccentColor();

	__super::OnDrawAutohideBar(pDC, rectBar, pButton);

	m_clrHighlightDn = oldColor;
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnFillBarBackground(CDC* pDC, CBCGPBaseControlBar* pBar, CRect rectClient, CRect rectClip, BOOL bNCArea /*= FALSE*/)
{
	const CBCGPStatusBar* pStatusBar = dynamic_cast<const CBCGPStatusBar*>(pBar);
	if (pStatusBar)
	{
		pDC->FillRect(rectClient, AfxGetThemeManager()->GetStatusbarBkgBrush());
		return;
	}
	
	CTBAutoHideBar* pAutoHideBar = dynamic_cast<CTBAutoHideBar*>(pBar);

	if (pAutoHideBar && AfxGetThemeManager()->GetAutoHideBarAccentColor() != -1)
		m_clrAccentText = AfxGetThemeManager()->GetAutoHideBarAccentColor();
	else
		m_clrAccentText = m_clrDefaultAccentText;

	__super::OnFillBarBackground(pDC, pBar, rectClient, rectClip, bNCArea);

}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::BreadcrumbDrawArrow(CDC& dc, CBCGPBreadcrumb* pControl, BREADCRUMBITEMINFO* pItemInfo, CRect rect, UINT uState, COLORREF color)
{
	__super::BreadcrumbDrawArrow(dc, pControl, pItemInfo, rect, uState, pControl->GetDefaultTextColor());
}

//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::IsRibbonScenicLook()
{
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::IsRibbonBackgroundImage()
{
	return FALSE;
}

//-----------------------------------------------------------------------------
CBrush& CMicroareaVisualManager::GetDlgBackBrush(CWnd* pDlg)
{
	CParsedForm* pParsedForm = GetParsedForm(pDlg);
	if (pParsedForm)
		return pParsedForm->GetDlgBackBrush();

	return  __super::GetDlgBackBrush(pDlg);
}

//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::OnFillDialog(CDC* pDC, CWnd* pDlg, CRect rect)
{
	CParsedForm* pParsedForm = GetParsedForm(pDlg);
	if (pParsedForm && pParsedForm->IsTransparent())
		return TRUE;

	return __super::OnFillDialog(pDC, pDlg, rect);
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnDrawSeparator(CDC* pDC, CBCGPBaseControlBar* pBar, CRect rect, BOOL bIsHoriz)
{
	if (!bIsHoriz)
	{
		rect.left = - (CBCGPToolBar::GetMenuImageSize().cx + GetMenuImageMargin()) + 5;
	}

	__super::OnDrawSeparator(pDC, pBar, rect, bIsHoriz);
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::DrawThemedButton(HWND hWnd, HDC hDC, DWORD styles, DWORD addOnStyles, LPRECT pRect, BOOL bHighlighted)
{
	TBThemeManager* pThemeManager = AfxGetThemeManager();

	if (pThemeManager->UseFlatStyle() && styles != BP_PUSHBUTTON)
	{
		CMicroareaVisualManager* pVisualManager = dynamic_cast<CMicroareaVisualManager*>(CBCGPVisualManager::GetInstance());
		CDC* pDC = CDC::FromHandle(hDC);
		CWnd* pWnd = CWnd::FromHandle(hWnd);
		//BOOL bTreatAsEnabled = pWnd ? pWnd->IsWindowEnabled() && IsInADataEntryInBrowse(pWnd) : FALSE;

		BOOL bTreatAsEnabled = pWnd ? pWnd->IsWindowEnabled() : FALSE;
		if (pVisualManager && pVisualManager->IsToUseEnabledBorderColor(pWnd, bTreatAsEnabled))
			bTreatAsEnabled = TRUE;
		
		if (styles == BP_RADIOBUTTON)
		{
			BOOL bCheck = ((addOnStyles & RBS_CHECKEDHOT) == RBS_CHECKEDHOT) || ((addOnStyles & RBS_CHECKEDNORMAL) == RBS_CHECKEDNORMAL) || ((addOnStyles & RBS_CHECKEDDISABLED) == RBS_CHECKEDDISABLED);

			pVisualManager->DrawRadioButton(pDC, pRect, bHighlighted, bCheck,
				bTreatAsEnabled, FALSE);
		}
		else
		{
			int bCheck = ((addOnStyles & CBS_CHECKEDHOT) == CBS_CHECKEDHOT) || ((addOnStyles & CBS_CHECKEDNORMAL) == CBS_CHECKEDNORMAL) || ((addOnStyles & CBS_CHECKEDDISABLED) == CBS_CHECKEDDISABLED) ? 1 : 0;
			int bFocused = ((addOnStyles & CBS_CHECKEDHOT) == CBS_CHECKEDHOT) || ((addOnStyles & CBS_UNCHECKEDHOT) == CBS_UNCHECKEDHOT) ? 2 : 0;
			pVisualManager->DrawCheckBox(pDC,
				pRect, bHighlighted, bCheck | bFocused,
				bTreatAsEnabled, FALSE);
		}
	}
	else
		pThemeManager->DrawOldXPButton(hWnd, hDC, styles, addOnStyles, pRect);
}

// OKKIO che purtroppo questo codice e' duplicato da CBCGPVisualManagerVS2012::DrawCheckBox
// e cambia solo per la gestione dell'enabled
// nState = 2 usato per indicare che è focused mentre nella versione originale vuol dire indeterminato
//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::DrawCheckBox(CDC *pDC, CRect rect, BOOL bHighlighted, int nState, BOOL bEnabled, BOOL bPressed)
{		
//	return __super::DrawCheckBox(pDC, rect, bHighlighted, nState, bEnabled, bPressed);

	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode())
	{
		return CBCGPVisualManagerVS2010::DrawCheckBox(pDC, rect, bHighlighted, (nState & 1), bEnabled, bPressed);
	}

	rect.DeflateRect(1, 1);

	if (rect.Width() <= 0 || rect.Height() <= 0)
	{
		return TRUE;
	}

	if (bPressed)
	{
		pDC->FillRect(rect, &m_brHighlightDn);
	}
	else if (bHighlighted || bPressed)
	{
		pDC->FillRect(rect, AfxGetThemeManager()->GetParsedButtonHoveringBrush());
	}
	else if (!IsDarkTheme())
	{
		if (bEnabled)
			pDC->FillRect(rect, &m_brWhite);
		else
			pDC->FillRect(rect, AfxGetThemeManager()->GetBackgroundColorBrush());
	}

	COLORREF clrBorder = (bHighlighted || bPressed) ?
		(m_Style == VS2012_LightBlue ? m_clrMenuItemBorder : m_clrHighlightDn) : AfxGetThemeManager()->GetEnabledControlBorderForeColor();
	
	
	if (!bEnabled)
	{
		clrBorder = AfxGetThemeManager()->GetDisabledControlForeColor();
	}

	if ((nState & 2) == 2)	//focused
		clrBorder = AfxGetThemeManager()->GetParsedButtonCheckedForeColor();

	pDC->Draw3dRect(&rect, clrBorder, clrBorder);

	if ((nState & 1) == 1)
	{
		CSize sizeDest(0, 0);

		if (CBCGPMenuImages::Size().cx > rect.Width() || CBCGPMenuImages::Size().cy > rect.Height())
		{
			sizeDest = rect.Size();
		}
		else
		{
			rect.bottom--;
		}

		CBCGPMenuImages::Draw(pDC, CBCGPMenuImages::IdCheck, rect, bEnabled ? CBCGPMenuImages::ImageBlack2 : CBCGPMenuImages::ImageLtGray, sizeDest);
	}

	return TRUE;
}

// OKKIO che purtroppo questo codice e' duplicato da CBCGPVisualManagerVS2012::DrawCheckBox
// e cambia solo per la gestione dell'enabled nella DrawEllipsed
//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::DrawRadioButton(CDC *pDC, CRect rect, BOOL bHighlighted, BOOL bChecked, BOOL bEnabled, BOOL bPressed)
{
	if (globalData.m_nBitsPerPixel <= 8 || globalData.IsHighContastMode())
	{
		return CBCGPVisualManagerVS2010::DrawRadioButton(pDC, rect, bHighlighted, bChecked, bEnabled, bPressed);
	}

	rect.DeflateRect(1, 1);

	CBCGPDrawManager dm(*pDC);

	if (bPressed)
	{
		dm.DrawEllipse(rect, m_clrHighlightDn, (COLORREF)-1);
	}
	else if (bHighlighted || bPressed)
	{
		dm.DrawEllipse(rect, AfxGetThemeManager()->GetParsedButtonHoveringColor(), (COLORREF)-1);
	}
	else if (!IsDarkTheme())
	{
		if (bEnabled)
			dm.DrawEllipse(rect, RGB(255, 255, 255), (COLORREF)-1);
		else
			dm.DrawEllipse(rect, AfxGetThemeManager()->GetBackgroundColor(), (COLORREF)-1);
	}

	COLORREF disabledColor = AfxGetThemeManager()->GetDisabledControlForeColor();
	COLORREF clrBorder = !bEnabled ? disabledColor :
		(
		(bHighlighted || bPressed) ?
			(m_Style == VS2012_LightBlue ? m_clrMenuItemBorder : AfxGetThemeManager()->GetButtonFaceHighLightColor()) : AfxGetThemeManager()->GetEnabledControlBorderForeColor()
		);

	if (bChecked && bEnabled)
		clrBorder = AfxGetThemeManager()->GetParsedButtonCheckedForeColor();

	dm.DrawEllipse(rect, (COLORREF)-1, clrBorder);

	rect.DeflateRect(rect.Width() / 3, rect.Width() / 3);
	if (bChecked)
	{
		rect.left -= 1;
		rect.right += 1;
		rect.top -= 1;
		rect.bottom += 1;

		dm.DrawEllipse(rect, bEnabled ? AfxGetThemeManager()->GetParsedButtonCheckedForeColor() : disabledColor, (COLORREF)-1);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
COLORREF CMicroareaVisualManager::OnFillPropertyListSelectedItem(CDC* pDC, CBCGPProp* pProp, CBCGPPropList* pWndList, const CRect& rectFill, BOOL bFocused)
{
	CTBProperty* pProperty = dynamic_cast<CTBProperty*>(pProp);
	if (!bFocused || !pProperty)
		return __super::OnFillPropertyListSelectedItem(pDC, pProp, pWndList, rectFill, bFocused);

	TBThemeManager* pThemeManager = AfxGetThemeManager();
	// personalizzazione del colore di fuoco
	CBrush aBrush;
	aBrush.CreateSolidBrush(pThemeManager->GetBERowSelectedBkgColor());

	pDC->FillRect(rectFill, &aBrush);
	return IsToUseEnabledBorderColor(pWndList, pProp->IsEnabled()) ? pThemeManager->GetEnabledControlForeColor() : globalData.clrGrayedText;
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnDrawTabContent(CDC* pDC, CRect rectTab, int iTab, BOOL bIsActive, const CBCGPBaseTabWnd* pTabWnd, COLORREF clrText)
{
	COLORREF foreColor = clrText;
	const CTaskBuilderTabWnd* pTBTabWnd = dynamic_cast<const CTaskBuilderTabWnd*>(pTabWnd);
	if (pTBTabWnd)
		foreColor = pTBTabWnd->GetBestTabForeColor(iTab, bIsActive);
	
	__super::OnDrawTabContent(pDC, rectTab, iTab, bIsActive, pTabWnd, foreColor);
}

// disegna la linguetta vera e propria
//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnDrawTab(CDC* pDC, CRect rectTab, int iTab, BOOL bIsActive, const CBCGPBaseTabWnd* pTabWnd)
{
	const CTaskBuilderTabWnd* pTBTabWnd = dynamic_cast<const CTaskBuilderTabWnd*>(pTabWnd);
	if (pTBTabWnd && pTBTabWnd->GetTbStyle() == CTaskBuilderTabWnd::TB_STRIP)
	{
		CBrush aBrush(pTBTabWnd->GetBestTabBkgColor(iTab, bIsActive));
		pTBTabWnd->AdjustTabTopBorder(rectTab);
		pDC->FillRect(rectTab, &aBrush);
		OnDrawTabContent(pDC, rectTab, iTab, bIsActive, pTabWnd, -1);
		return;
	}

	__super::OnDrawTab(pDC, rectTab, iTab, bIsActive, pTabWnd);
}

// rappresenta l'area di background dove si appoggiano le linguette delle tab
//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnEraseTabsArea(CDC* pDC, CRect rect, const CBCGPBaseTabWnd* pTabWnd)
{
	const CTaskBuilderTabWnd* pTBTabWnd = dynamic_cast<const CTaskBuilderTabWnd*>(pTabWnd);
	if (pTBTabWnd && pTBTabWnd->GetTbStyle() == CTaskBuilderTabWnd::TB_STRIP)
	{
		COLORREF bkgColor = pTBTabWnd->GetTabBkColor(pTBTabWnd->GetActiveTab());
		pDC->FillSolidRect(rect, bkgColor);
	}
	else
		__super::OnEraseTabsArea(pDC, rect, pTabWnd);
}

//-----------------------------------------------------------------------------
int	CMicroareaVisualManager::GetTabExtraHeight(const CBCGPTabWnd* pTab)
{
	const CTaskBuilderTabWnd* pTBTabWnd = dynamic_cast<const CTaskBuilderTabWnd*>(pTab);
	if (pTBTabWnd && pTBTabWnd->GetTbStyle() == CTaskBuilderTabWnd::TB_STRIP)
	{
		return pTBTabWnd->GetDefaultHeight();
	}
	return __super::GetTabExtraHeight(pTab);
}

//*******************************************************************************
BOOL CMicroareaVisualManager::OnDrawComboBoxText(CDC* pDC, CBCGPComboBox* pComboBox)
{
	// per gestire il caso delle ParsedCombo nei PropertyGrid (SetFont maffo)
	if (dynamic_cast<CParsedCombo*>(pComboBox))
		return TRUE;

	return __super::OnDrawComboBoxText(pDC, pComboBox);
}

//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::IsOwnerDrawScrollBar()
{
	return __super::IsOwnerDrawScrollBar();
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnScrollBarDrawThumb(CDC* pDC, CBCGPScrollBar* pScrollBar, CRect rect, BOOL bHorz, BOOL bHighlighted, BOOL bPressed, BOOL bDisabled)
{
	CBCGPDrawManager dm(*pDC);

	int nThumbSize = AfxGetThemeManager()->GetScrollBarThumbSize();
	int nThumbSpace = SCROLLBARSPACE;

	if (bHorz) {
		if (nThumbSize + nThumbSpace > rect.Height()) nThumbSize = -1;
	} 
	else {
		if (nThumbSize + nThumbSpace > rect.Width()) nThumbSize = -1;
	}

	if (nThumbSize <= 0)
	{
		dm.DrawRect(rect,
			bPressed || bDisabled ? AfxGetThemeManager()->GetScrollBarThumbPressedColor() : AfxGetThemeManager()->GetScrollBarThumbNoPressedColor(),
			bHighlighted ? globalData.clrBarDkShadow : (m_Style == Office2013_White ? m_clrSeparator : m_clrMenuBorder));
	}
	else
	{
		dm.DrawRect(rect, AfxGetThemeManager()->GetScrollBarFillBkgColor(), (COLORREF)-1);
		CRect ThumbRect = rect;

		if (bHorz)
		{
			ThumbRect.top = rect.bottom - (nThumbSize + nThumbSpace);
			ThumbRect.bottom = rect.bottom - nThumbSpace;
		}
		else
		{
			ThumbRect.left = rect.right - (nThumbSize + nThumbSpace);
			ThumbRect.right = rect.right - nThumbSpace;
		}

		dm.DrawRect(ThumbRect,
			bPressed || bDisabled ? AfxGetThemeManager()->GetScrollBarThumbPressedColor() : AfxGetThemeManager()->GetScrollBarThumbNoPressedColor(),
			bHighlighted ? globalData.clrBarDkShadow : (m_Style == Office2013_White ? m_clrSeparator : m_clrMenuBorder));
	}
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnScrollBarDrawButton(CDC* pDC, CBCGPScrollBar* pScrollBar, CRect rect, BOOL bHorz, BOOL bHighlighted, BOOL bPressed, BOOL bFirst, BOOL bDisabled)
{
	COLORREF clrFill = bPressed || bDisabled ? AfxGetThemeManager()->GetScrollBarBkgButtonPressedColor() : AfxGetThemeManager()->GetScrollBarBkgButtonNoPressedColor();
	COLORREF clrLine = bPressed || bDisabled ? AfxGetThemeManager()->GetScrollBarThumbPressedColor() : AfxGetThemeManager()->GetScrollBarThumbNoPressedColor();

	if (bDisabled) {
		clrLine = AfxGetThemeManager()->GetScrollBarThumbDisableColor();
	}

	CBCGPDrawManager dm(*pDC);
	
	// Button without border
	dm.DrawRect(rect, clrFill, (COLORREF)-1);
	int nThumbSize = AfxGetThemeManager()->GetScrollBarThumbSize();
	int nThumbSpace = SCROLLBARSPACE;
	
	if (bHorz)
	{
		if (pScrollBar->GetSafeHwnd() != NULL && (pScrollBar->GetExStyle() & WS_EX_LAYOUTRTL)) { 
			bFirst = !bFirst; 
		}
		// bFirst ? ArowLeft : ArowRight
		int iYT = rect.bottom - (nThumbSize + nThumbSpace);
		int iYB = rect.bottom - (nThumbSpace + 1);
		int iX = bFirst ? rect.right - 1: rect.left + 1;
		for (int i = 0; i < nThumbSize; i++)
		{
			dm.DrawLine(iX, iYT, iX, iYB, clrLine);
			iYT++;
			iYB--;
			bFirst ? iX-- : iX++;
			if (iYT > iYB)
				break;
		}
	}
	else
	{
		// bFirst ? ArowUp : ArowDown
		int iXL = rect.right - (nThumbSize + nThumbSpace);
		int iXR = rect.right - (nThumbSpace + 1);
		int iY = bFirst ? rect.bottom - 1 : rect.top;
		for (int i = 0; i < nThumbSize; i++)
		{
			dm.DrawLine( iXL, iY, iXR, iY, clrLine);
			iXL++;
			iXR--;
			bFirst ? iY-- : iY++;
			if (iXL > iXR)
				break;
		}
	}
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnScrollBarFillBackground(CDC* pDC, CBCGPScrollBar* pScrollBar, CRect rect, BOOL bHorz, BOOL bHighlighted, BOOL bPressed, BOOL bFirst, BOOL bDisabled)
{
	CBCGPDrawManager dm(*pDC);
	dm.DrawRect(rect, AfxGetThemeManager()->GetScrollBarFillBkgColor(), (COLORREF)-1);
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnDrawRibbonProgressBar(CDC* pDC, CBCGPRibbonProgressBar* pProgress, CRect rectProgress, CRect rectChunk, BOOL bInfiniteMode)
{
	ASSERT_VALID(pDC);

	COLORREF clrBorder = globalData.clrBtnDkShadow;
	// pDC->FillRect(rectProgress, &m_brAccentLight);
	
	if (!rectChunk.IsRectEmpty())
	{
		rectChunk.DeflateRect(1, 1);
		pDC->FillRect(rectChunk, AfxGetThemeManager()->GetProgressBarColorBrush());
	}

	pDC->Draw3dRect(rectProgress, clrBorder, clrBorder);
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnFillButtonInterior(CDC* pDC, CBCGPToolbarButton* pButton, CRect rect, CBCGPVisualManager::BCGBUTTON_STATE state)
{
	CBCGPVisualManagerXP::OnFillButtonInterior(pDC, pButton, rect, CBCGPVisualManager::BCGBUTTON_STATE::ButtonsIsRegular);
	
	CTBToolBar* pToolbar = DYNAMIC_DOWNCAST(CTBToolBar, pButton->GetParentWnd());
	BOOL bDraw = FALSE;
	COLORREF clrBorder;
	ASSERT_VALID(pDC);
	
	if (state == CBCGPVisualManager::BCGBUTTON_STATE::ButtonsIsHighlighted)
	{
		clrBorder = AfxGetThemeManager()->GetToolbarHighlightedColor();
		if (pToolbar) {
			clrBorder = pToolbar->GetHighlightedColor();
		}
		bDraw = TRUE;
	}
	else if (state == CBCGPVisualManager::BCGBUTTON_STATE::ButtonsIsPressed && pToolbar && pToolbar->IsEnableHighlightedColorClick())
	{
		clrBorder = pToolbar->GetHighlightedColorClick();
		bDraw = TRUE;
	}

	if (!bDraw) return;

	rect.top = rect.bottom - AfxGetThemeManager()->GetToolbarHighlightedHeight();
	pDC->FillSolidRect(rect, clrBorder);
}

//-----------------------------------------------------------------------------
int CMicroareaVisualManager::GetMenuImageMargin() const
{
	return max(__super::GetMenuImageMargin(), AfxGetThemeManager()->GetMenuImageMargin());
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnDrawButtonBorder(CDC* pDC, CBCGPToolbarButton* pButton, CRect rect, CBCGPVisualManager::BCGBUTTON_STATE state)
{
	if (pButton->m_nStyle & TBBS_CHECKED)
	{
		ASSERT_VALID(pDC);
		COLORREF clrBorder = AfxGetThemeManager()->GetToolbarButtonCheckedColor();
		rect.top = rect.bottom - AfxGetThemeManager()->GetToolbarHighlightedHeight();
		pDC->FillSolidRect(rect, clrBorder);
	}
}

//-----------------------------------------------------------------------------
void CMicroareaVisualManager::OnDrawButtonSeparator(CDC* pDC, CBCGPToolbarButton* pButton, CRect rect, CBCGPVisualManager::BCGBUTTON_STATE state, BOOL bHorz)
{
	// Draw of separetor in internal button es.: DropDownMenu disabled
}

//-----------------------------------------------------------------------------
COLORREF CMicroareaVisualManager::GetToolbarButtonTextColor(CBCGPToolbarButton* pButton, CBCGPVisualManager::BCGBUTTON_STATE state)
{
	ASSERT_VALID(pButton);
	CTBToolBar* pToolbar = DYNAMIC_DOWNCAST(CTBToolBar, pButton->GetParentWnd());
	ASSERT_VALID(pToolbar);
	if (pToolbar == NULL)
		return __super::GetToolbarButtonTextColor(pButton, state);

	COLORREF clText				= pToolbar->GetTextColor();
	COLORREF clTextHighlighted  = pToolbar->GetTextColorHighlighted();
	
	if (pButton->m_nStyle & TBBS_DISABLED)
	{
		clText = __super::GetToolbarButtonTextColor(pButton, state);
		clTextHighlighted = clText;
	}

	CTBToolbarButton* pTButton = DYNAMIC_DOWNCAST(CTBToolbarButton, pButton);
	// Secondary Fill Color enable firsth button
	if (pTButton && pTButton->IsSecondaryFillColorButton())
	{
		if (state == CBCGPVisualManager::BCGBUTTON_STATE::ButtonsIsHighlighted)
		{
			return clText;
		}
		return clTextHighlighted;
	}

	// Normal color button 
	if (state == CBCGPVisualManager::BCGBUTTON_STATE::ButtonsIsHighlighted)
	{
		// button is selected
		return clTextHighlighted;
	}
	return clText;
}

//-----------------------------------------------------------------------------
COLORREF CMicroareaVisualManager::GetStatusBarPaneTextColor(CBCGPStatusBar* pStatusBar, CBCGStatusBarPaneInfo* pPane)
{
	if (pPane)
		return pPane->clrText;

	return __super::GetStatusBarPaneTextColor(pStatusBar, pPane);
}

//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::IsToUseEnabledBorderColor(CWnd* pWnd, BOOL bEnabled) const
{
	return bEnabled || IsInADataEntryInBrowse(pWnd);
}

//-----------------------------------------------------------------------------
BOOL CMicroareaVisualManager::IsInADataEntryInBrowse(CWnd* pWnd) const
{
	CBaseDocument* pDoc = NULL;
	CParsedCtrl* pCtrl = dynamic_cast<CParsedCtrl*>(pWnd);
	if (pCtrl)
		pDoc = pCtrl->GetDocument();
	else
	{
		CGridControlObj* pGrid = dynamic_cast<CGridControlObj*>(pWnd);
		if (pGrid)
			pDoc = pGrid->GetDocument();
	}
	
	return pDoc && pDoc->GetFormMode() == CBaseDocument::BROWSE && !pDoc->IsABatchDocument();
}

//-----------------------------------------------------------------------------
COLORREF CMicroareaVisualManager::GetControlForeColor(CWnd* pWnd, BOOL bEnabled, COLORREF defaultColor /*-1*/) const
{
	COLORREF clrBrowse = globalData.clrGrayedText;
	CParsedStatic* pStatic = dynamic_cast<CParsedStatic*>(pWnd);
	if (pStatic)
		if (defaultColor == -1)
			return clrBrowse;
		else
			return defaultColor;

	if (IsInADataEntryInBrowse(pWnd))
		return clrBrowse;

	if (bEnabled)
	{
		if (defaultColor == -1)
			return clrBrowse;
		return defaultColor;
	}

	return pWnd->IsKindOf(RUNTIME_CLASS(CGridControl))  ? clrBrowse : AfxGetThemeManager()->GetDisabledControlForeColor();
}

//-----------------------------------------------------------------------------
COLORREF CMicroareaVisualManager::OnDrawControlBarCaption(CDC* pDC, CBCGPDockingControlBar* pBar, BOOL bActive, CRect rectCaption, CRect rectButtons)
{

	CTaskBuilderDockPane* pPane = dynamic_cast<CTaskBuilderDockPane*>(pBar);
	if (pPane)
	{
		// codice del metodo originale
		rectCaption.bottom++;
		COLORREF c = pPane->GetTitleBkgColor();
		pDC->FillSolidRect(rectCaption, c);
		return bActive ? pPane->GetTitleHoveringForeColor() : pPane->GetTitleForeColor();

	}
	
	return __super::OnDrawControlBarCaption(pDC, pBar, bActive, rectCaption, rectButtons);
}