#pragma once

#include "TbGenlib\TBTabWnd.h"
#include <TbGeneric\TBThemeManager.h>

#include "beginh.dex"


class CSelectorButton;
class CSelectorButtonContainer;
class DlgInfoItem;

//======================================================================
// il tabber e' derivato per reimplementare la logica di SetActiveTab
// e di AddTab
class TB_EXPORT CTabSelectorTabWnd : public CTaskBuilderTabWnd
{
	DECLARE_DYNAMIC(CTabSelectorTabWnd);

	TBThemeManager* m_pThemeManager;

public:
	CTabSelectorTabWnd();
	~CTabSelectorTabWnd() {}

	BOOL	Create(CWnd* pParent);
	void	UpdateTabStates();
	void	CleanTabState(CWnd* pWnd);
	void	SetAutoDestroy(BOOL bAutoDestroy) { m_bAutoDestoyWindow = bAutoDestroy; }

	virtual BOOL		SetActiveTab(int iTab);
	virtual COLORREF	GetTabBkColor(int iTab) const;
	virtual void		CleanUp();
	virtual void		AttachDocument(CBaseDocument* pDoc) { m_pDocument = pDoc; }


protected:
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);

	DECLARE_MESSAGE_MAP();
};



/////////////////////////////////////////////////////////////////////////////
//			CTabSelector
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTabSelector : public CTabCtrl
{
public:
	enum ShowMode { NORMAL, VERTICAL_TILE, NONE };
	enum SelectorAppearance { IMAGE_ONLY, TEXT_ONLY, IMAGE_AND_TEXT };

private:
	friend class CSelectorButton;

	DECLARE_DYNAMIC(CTabSelector)

	int							m_nCurSel;
	CArray<CSelectorButton*>	m_arSelectors;
	int							m_nIconWidth;
	int							m_nIconHeight;
	int							m_nSelectorWidth;
	ShowMode					m_ShowMode;
	SelectorAppearance			m_SelectorAppearance;
	HICON						m_defaultIconTileGroup;
	CScrollView*				m_pParentScrollView;
	CSelectorButtonContainer*	m_pSelectorButtonContainer = NULL;
	CTabSelectorTabWnd*			m_pNormalTabber;
	
protected:
	int							m_nPosClickedTab;

private:
	int							GetSelectorIndex						(CSelectorButton* pSelector);
	CScrollView*				GetParentScrollView						();
	int							CalcSelectorButtonHeightForWordWrap		(CSelectorButton* pSelectorButton);
	CSelectorButtonContainer*	GetSelectorButtonContainer				();
	
public:
	CTabSelectorTabWnd*			GetNormalTabber();
	void						CreateNormalTabber();

	DECLARE_MESSAGE_MAP()
public:
	CTabSelector();
	virtual ~CTabSelector();
	
	ShowMode GetShowMode() const { return m_ShowMode; }
	void SetShowMode(ShowMode showMode) { m_ShowMode = showMode; }

	SelectorAppearance GetSelectorAppearance() const { return m_SelectorAppearance; }
	void SetSelectorAppearance(SelectorAppearance selectorAppearance) { m_SelectorAppearance = selectorAppearance; }
	
	// Determines the currently selected tab in the tab control.
	int GetCurSel() const;

	// Selects the specified tab in the tab control.
	int SetCurSel(_In_ int nItem);

	int HitTest(_In_ TCHITTESTINFO* pHitTestInfo) const;

	// Removes a tab from the tab control.
	BOOL DeleteItem(_In_ int nItem);
	// Sets some or all attributes of the specified tab in the tab control.
	BOOL SetItem(_In_ int nItem, _In_ TCITEM* pTabCtrlItem);
	// Retrieves the number of tabs in the tab control.
	int GetItemCount() const;
	
	// Generic creator allowing extended style bits
	virtual BOOL	CreateEx(_In_ DWORD dwExStyle, _In_ DWORD dwStyle, _In_ const RECT& rect, _In_ CWnd* pParentWnd, _In_ UINT nID);
	virtual	void	OnUpdateTabStates();

	void OnTabClick(CSelectorButton* pLabel);
	void DoTabClick(int newSelected);

	void SetIconSize(int nIconWidth, int nIconHeight) { m_nIconHeight = nIconHeight; m_nIconWidth = nIconWidth; }
	bool IsIconSizeSet() { return m_nIconHeight != -1 && m_nIconWidth != -1; }
	void GetTabsWindowRect(CRect& rect);

	virtual	BOOL	PreTranslateMessage	(MSG* pMsg);
	BOOL			ProcessSysKeyMessage(MSG* pMsg);
	
	afx_msg void	OnSize (UINT nType, int cx, int cy);

	void	UpdateSelector();

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);
	virtual CString GetRanorexNamespace();

protected:
	void				ArrangeItems	();
	int					GetSelectorWidth()			{ return m_nSelectorWidth; }
	int					GetTabIndex(CPoint point);
	CSelectorButton*	CreateSelector(int index, DlgInfoItem* pDlgInfoItem, CString strImagePath, CString strTooltip, CString strTooltipDescription = _T(""));
	void				MoveButtonSelector(int indexNew, int indexOld);
	void				ChangeImage(int index, CString strImagePath);

	virtual int					InsertDlgInfoItem(int, DlgInfoItem*);
	virtual void				CalculateSelectorWidth();
	virtual CBaseDocument*		GetSelectorDocument() = 0;
};

/////////////////////////////////////////////////////////////////////////////
//			CSelectorButtonToolTip
/////////////////////////////////////////////////////////////////////////////
class CSelectorButtonToolTip : public CBCGPToolTipCtrl 
{
	CBCGPToolTipParams m_TooltipParams;
	// Construction
public:
	CSelectorButtonToolTip();	
	void Init(CWnd* ownerWnd, CString strTitle, CString strDescription = _T(""));

protected:
	//{{AFX_MSG(CSelectorButtonToolTip)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_MSG
	afx_msg void OnPop(NMHDR* pNMHDR, LRESULT* pResult);
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////
//			CSelectorButtonContainer
/////////////////////////////////////////////////////////////////////////////

class TB_EXPORT CSelectorButtonContainer : public CWnd
{
	DECLARE_DYNAMIC(CSelectorButtonContainer)

private:
	CTBScrollBar * m_pVScrollBar;
	int m_nRealHeight;
	int m_nScrollPosY;
	int m_nScrollStepY;
	int m_nScrollWidth;

public:
	CSelectorButtonContainer();
	virtual ~CSelectorButtonContainer();

public:
	CTBScrollBar * GetVScrollBar() { return m_pVScrollBar; }
	void CreateAccessories();
	void ResetVScrollBar();
	void SetRealHeight(int nHeight) { m_nRealHeight = nHeight; }
	int  GetRealHeight() { return m_nRealHeight; }
	void SetScrollVisible(BOOL bSet) {
		if (m_pVScrollBar)
			m_pVScrollBar->SetVisible(bSet);
		if (bSet)
			m_pVScrollBar->EnableWindow();
	}
	BOOL GetScrollVisible() { return m_pVScrollBar && m_pVScrollBar->isVisible(); }

	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

private:
	afx_msg	BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	afx_msg void OnSize(UINT nType, int cx, int cy);

	DECLARE_MESSAGE_MAP();
};



/////////////////////////////////////////////////////////////////////////////
//			CSelectorButton
/////////////////////////////////////////////////////////////////////////////

class TB_EXPORT CSelectorButton : public CBCGPButton
{
	DECLARE_DYNAMIC(CSelectorButton)

private:

	CTabSelector*				m_pTabber;
	CSelectorButtonToolTip		m_ToolTip;
	CString						m_strIconSource;
	CSelectorButtonContainer*	m_pSelectorButtonContainer;
 

public:
	DlgInfoItem* m_pDlgInfoItem;

public:
	CSelectorButton(CTabSelector* pTabber, CSelectorButtonContainer* pSelectorButtonContainer);
	virtual ~CSelectorButton();

	virtual BOOL PreTranslateMessage(MSG* pMsg);

	int GetTabIndex();
	void SetChecked(BOOL bChecked);
	void OnBnClicked();

	void ChangeImage(CString strImagePath, HICON defaultIconTileGroup = NULL);
	void Create(DlgInfoItem* pDlgInfoItem, CString strImagePath, CString strTooltip, HICON defaultIconTileGroup = NULL , CString strTooltipDescription = _T(""));

	const CRect& GetCaptionRect() { return m_rectCaption; }
	CString GetIconSource() { return m_strIconSource; }

	void UpdateText();

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

protected:
	void DoDrawItem(CDC* pDCPaint, CRect rectClient, UINT itemState);
	virtual void OnFillBackground(CDC* pDC, const CRect& rectClient);

private:
	CRect m_rectCaption;

	afx_msg	BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnPaint();

	DECLARE_MESSAGE_MAP();
};

#include "endh.dex"
