#pragma once

#include <TbNameSolver\MacroToRedifine.h>

// Includere alla fine degli include del .H
#include "beginh.dex"


//=======================================================================
class TB_EXPORT CMicroareaVisualManager : public CBCGPVisualManager2013
{
	DECLARE_DYNCREATE(CMicroareaVisualManager)
private:
	BOOL				m_bUseTabbedDocuments;
	COLORREF			m_clrDefaultAccentText;

public:
	CMicroareaVisualManager();
	virtual ~CMicroareaVisualManager();

	virtual BOOL		UseLargeCaptionFontInDockingCaptions();

	virtual void		SetupColors();

	virtual BOOL		IsOwnerDrawCaption		();
	virtual BOOL		IsMinFrameCaptionBold	(CBCGPMiniFrameWnd* /* pFrame */);
	virtual COLORREF	BreadcrumbFillBackground(CDC& dc, CBCGPBreadcrumb* pControl, CRect rectFill);
	virtual void		OnFillBarBackground		(CDC* pDC, CBCGPBaseControlBar* pBar,	CRect rectClient, CRect rectClip,	BOOL bNCArea = FALSE);
	virtual void		OnDrawAutohideBar		(CDC* pDC, CRect rectBar, CBCGPAutoHideButton* pButton);
	virtual void		BreadcrumbDrawArrow		(CDC& dc, CBCGPBreadcrumb* pControl, BREADCRUMBITEMINFO* pItemInfo, CRect rect, UINT uState, COLORREF color);
	virtual BOOL		IsRibbonScenicLook		();
	virtual BOOL		IsRibbonBackgroundImage	();
	virtual CBrush&		GetDlgBackBrush			(CWnd* pDlg);
	virtual void		OnDrawSeparator(CDC* pDC, CBCGPBaseControlBar* pBar, CRect rect, BOOL bIsHoriz);
	virtual BOOL		DrawCheckBox(CDC *pDC, CRect rect, BOOL bHighlighted, int nState, BOOL bEnabled, BOOL bPressed);
	virtual BOOL		DrawRadioButton(CDC *pDC, CRect rect, BOOL bHighlighted, BOOL bChecked, BOOL bEnabled,BOOL bPressed);
	virtual BOOL		OnFillDialog			(CDC* pDC, CWnd* pDlg, CRect rect);

	// Property Grid
	virtual COLORREF OnFillPropertyListSelectedItem(CDC* pDC, CBCGPProp* pProp, CBCGPPropList* pWndList, const CRect& rectFill, BOOL bFocused);

	// Tab Wnd
	virtual void		OnEraseTabsArea		(CDC* pDC, CRect rect, const CBCGPBaseTabWnd* pTabWnd);
	virtual int			GetTabExtraHeight	(const CBCGPTabWnd* pTab);
	virtual void		OnDrawTab			(CDC* pDC, CRect rectTab, int iTab, BOOL bIsActive, const CBCGPBaseTabWnd* pTabWnd);
	virtual void		OnDrawTabContent	(CDC* pDC, CRect rectTab, int iTab, BOOL bIsActive, const CBCGPBaseTabWnd* pTabWnd, COLORREF clrText);
	virtual BOOL		OnDrawComboBoxText	(CDC* pDC, CBCGPComboBox* pComboBox);

	// Scrollbar
	virtual BOOL IsOwnerDrawScrollBar();
	virtual void OnScrollBarDrawThumb(CDC* /*pDC*/, CBCGPScrollBar* /*pScrollBar*/, CRect /*rect*/,	BOOL /*bHorz*/, BOOL /*bHighlighted*/, BOOL /*bPressed*/, BOOL /*bDisabled*/);
	virtual void OnScrollBarDrawButton(CDC* /*pDC*/, CBCGPScrollBar* /*pScrollBar*/, CRect /*rect*/, BOOL /*bHorz*/, BOOL /*bHighlighted*/, BOOL /*bPressed*/, BOOL /*bFirst*/, BOOL /*bDisabled*/);
	virtual void OnScrollBarFillBackground(CDC* /*pDC*/, CBCGPScrollBar* /*pScrollBar*/, CRect /*rect*/, BOOL /*bHorz*/, BOOL /*bHighlighted*/, BOOL /*bPressed*/, BOOL /*bFirst*/, BOOL /*bDisabled*/);

	// Button ToolBar
	virtual void OnDrawRibbonProgressBar(CDC* pDC, CBCGPRibbonProgressBar* pProgress, CRect rectProgress, CRect rectChunk, BOOL bInfiniteMode);
	
	virtual void OnFillButtonInterior(CDC* pDC,	CBCGPToolbarButton* pButton, CRect rect, CBCGPVisualManager::BCGBUTTON_STATE state);
	virtual void OnDrawButtonSeparator(CDC* pDC, CBCGPToolbarButton* pButton, CRect rect, CBCGPVisualManager::BCGBUTTON_STATE state, BOOL bHorz);
	virtual void OnDrawButtonBorder(CDC* pDC, CBCGPToolbarButton* pButton, CRect rect, CBCGPVisualManager::BCGBUTTON_STATE state);
	virtual COLORREF GetToolbarButtonTextColor(CBCGPToolbarButton* pButton, CBCGPVisualManager::BCGBUTTON_STATE state);
	virtual COLORREF OnDrawControlBarCaption(CDC* pDC, CBCGPDockingControlBar* pBar, BOOL bActive, CRect rectCaption, CRect rectButtons);
	virtual COLORREF GetStatusBarPaneTextColor(CBCGPStatusBar* pStatusBar, CBCGStatusBarPaneInfo* pPane);
	virtual int GetMenuImageMargin() const;
	void DrawThemedButton(HWND hWnd, HDC hDC, DWORD styles, DWORD addOnStyles, LPRECT pRect, BOOL bHighlighted);
	virtual BOOL IsOwnerDrawMenuCheck() { 
		return TRUE; 
	}

	COLORREF GetControlForeColor(CWnd* pWnd, BOOL bEnabled, COLORREF defaultColor = -1) const;
	BOOL IsToUseEnabledBorderColor(CWnd* pWnd, BOOL bEnabled) const;
	BOOL IsInADataEntryInBrowse(CWnd* pWnd) const;
};


#include "endh.dex"
