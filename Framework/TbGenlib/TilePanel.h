#pragma once

#include <TbGenlib\OslInfo.h>
#include <TbGenlib\TBToolBar.h>
#include <TbGenlib\Tile.h>
#include <TbGenlib\TBTabWnd.h>

#include "beginh.dex"

class CBaseTileGroup;
class CTilePanel;
class CBaseStatusTile;

//=============================================================================
class TB_EXPORT CTilePanelTab : public CWnd, public ILayoutObject, public IOSLObjectManager 
{
	friend class CTilePanel;

	DECLARE_DYNAMIC (CTilePanelTab)

public:
	CTilePanelTab(CTilePanel* pOwner);
	~CTilePanelTab();

private:
	CLayoutContainer*	m_pLayoutContainer;
	CTilePanel*			m_pOwner;
	// serve per tenere traccia del fatto che il 
	// programmatore non vuole abilitare la finestra
	// indipendentemente dallo stato di browse mode e
	// quindi di Enablewindow dell'oggetto CWnd relativo
	BOOL				m_bEnabled;
	CString				m_Title;

private:
	void				SetContentEnable	(const LayoutElementArray* pElements, BOOL bEnable);

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

public:
	void				Enable				(BOOL bEnable = TRUE);
	void				EnableContent		(BOOL bEnable);
	TileStyle*			GetTileDialogStyle	();
	CBaseTileGroup*		GetParentTileGroup	();
	CTBNamespace&		GetNamespace		()		{ return GetInfoOSL()->m_Namespace; }
	CString				GetTitle			()		{ return m_Title;}
	void				SetTitle			(CString title)		{m_Title = title; }

	virtual	BOOL		PreTranslateMessage	(MSG* pMsg);

	CTilePanel*			GetTilePanel		()	{ return m_pOwner; }
	BOOL				IsEnabled			()	{ return m_bEnabled; }
	virtual CWnd*		GetWndLinkedCtrl	(const CTBNamespace& aNS);
	virtual CWnd*		GetWndLinkedCtrl	(UINT nIDC);
public:
	// ILayoutObject interface implementation
	virtual const	LayoutElementArray*	GetContainedElements()	{ return m_pLayoutContainer->GetContainedElements();}
	virtual const	CString				GetElementNameSpace() { return GetInfoOSL()->m_Namespace.ToString(); }
	virtual const	CString				GetElementName() { return GetInfoOSL()->m_Namespace.GetObjectName(); }

	CLayoutContainer*	GetLayoutContainer() { return m_pLayoutContainer;  }

	void	Relayout(CRect& rect) { m_pLayoutContainer->Relayout(rect); }

//protected:
	afx_msg void	OnWindowPosChanged	(WINDOWPOS* lpwndpos);
	afx_msg void	OnWindowPosChanging	(WINDOWPOS* lpwndpos);
	virtual	BOOL	OnCommand(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnValueChanged		(WPARAM wParam, LPARAM lParam);
	afx_msg BOOL	OnEraseBkgnd		(CDC* pDC);
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////
//						CTilePanelTabber 
/////////////////////////////////////////////////////////////////////////////
class CTilePanelTabber : public CTaskBuilderTabWnd
{
	friend class CTilePanel;
	friend class CTilePanelTab;

private:
	CTilePanel*	m_pOwner;

public:
	CTilePanelTabber(CTilePanel* pOwner);

	virtual BOOL SetActiveTab(int iTab);
	virtual void AttachDocument(CBaseDocument* pDoc) { m_pDocument = pDoc; }
	virtual void CleanUp();

	void OnTabEnableChanged(CTilePanelTab* pTab, BOOL bEnable);

	BOOL OnCommand(WPARAM wParam, LPARAM lParam);

	CTilePanelTab*		GetActiveTilePanelTab();

protected:
	virtual void		DrawTabDot(CDC* pDC, CBCGPTabInfo* pTab, BOOL bActive);

public:
	afx_msg LRESULT OnValueChanged(WPARAM wParam, LPARAM lParam);
	afx_msg BOOL	OnEraseBkgnd(CDC* pDC);
	afx_msg void	OnWindowPosChanged(WINDOWPOS* lpwndpos);
	DECLARE_MESSAGE_MAP()
};


//======================================================================
class TB_EXPORT CTilePanel : public CBCGPWnd, public Tile, public LayoutElement, public	IDisposingSourceImpl, public IOSLObjectManager 
{
	friend class CTilePanelTab;
	template <class T> friend class TBJsonTileGroupWrapper;
	friend class CJsonHeaderStrip;
		
	DECLARE_DYNCREATE(CTilePanel);

private:
	// componenti visivi
	CBaseTileGroup*					m_pParent;
	CTilePanelTabber*				m_pTabber;

	// gestione del layout
	TileStyle*						m_pTileDialogStyle;	// the style of the contained dialogs
	CLayoutContainer::LayoutType	m_DefaultLayoutType;
	CLayoutContainer::LayoutAlign	m_DefaultLayoutAlign;

	BOOL							m_bIsShowAsTile = false;
public:
	CTilePanel();
	virtual ~CTilePanel();

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);

public:
	BOOL				Create	(CBaseTileGroup* pParent, const CString& strTitle, UINT nIDC = -1);
	CTBNamespace&		GetNamespace()				{ return GetInfoOSL()->m_Namespace; }

	BOOL				GetIsShowAsTile()			{ return m_bIsShowAsTile; }
	void				SetIsShowAsTile(BOOL value) { m_bIsShowAsTile = value; }

	// no tabs required
	CBaseTileDialog*	AddTile(CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, BOOL bInitiallyUnpinned = FALSE, CObject* pOwner = NULL);
	CBaseTileDialog*	AddTile(CLayoutContainer* pContainer, CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, CObject* pOwner = NULL);
	CBaseTileDialog*	AddJsonTile(UINT nDialogID, CLayoutContainer* pContainer = NULL);

	// tabs management
	CTilePanelTab*		AddTab		(LPCTSTR szName, LPCTSTR szTitle, UINT nImage = -1);
	CTilePanelTab*		AddTab		(LPCTSTR szName, LPCTSTR szTitle, CLayoutContainer::LayoutType	aLayoutType, CLayoutContainer::LayoutAlign aLayoutAlign, UINT nImage = -1);
	BOOL				RemoveTab	(int tab, BOOL recalcLayout);
	BOOL				RemoveTab	(CTilePanelTab* pTab, BOOL recalcLayout);
	void				RemoveAllTabs();

	CBaseTileDialog*	AddTile(CTilePanelTab* pTab, CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, BOOL bInitiallyUnpinned = FALSE, CObject* pOwner = NULL);
	CBaseTileDialog*	AddTile(CTilePanelTab* pTab, CLayoutContainer* pContainer, CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, BOOL bInitiallyUnpinned = FALSE, CObject* pOwner = NULL);
	CBaseTileDialog*	AddTile(CTilePanelTab* pTab, CLayoutContainer* pContainer, CBaseTileDialog* pTile, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, BOOL bInitiallyUnpinned = FALSE, CObject* pOwner = NULL);
	CBaseTileDialog*	AddJsonTile(CTilePanelTab* pTab, UINT nDialogID, CLayoutContainer* pContainer = NULL);

	void                AddStatusTile(CBaseStatusTile*	pStatusTile, UINT nTileID = 0, int flex = 0);    //TODO togliere default nTileID
	CBaseStatusTile*	AddStatusTile(CRuntimeClass* pStatusTileClass, UINT nTileID = 0, int flex = 0);  //TODO togliere default nTileID

	CLayoutContainer*	AddContainer
						(
							CTilePanelTab*					pTab,
							CLayoutContainer::LayoutType	aLayoutType, 
							int								flex = AUTO,
							CLayoutContainer::LayoutAlign	aLayoutAlign = CLayoutContainer::STRETCH
						);
	CLayoutContainer*	AddContainer		
						(
							CLayoutContainer*				pParentContainer,
							CLayoutContainer::LayoutType	aLayoutType, 
							int								flex = AUTO,
							CLayoutContainer::LayoutAlign	aLayoutAlign = CLayoutContainer::STRETCH
						);
	CLayoutContainer*	AddContainer(CLayoutContainer::LayoutType aLayoutType, int flex = AUTO, CLayoutContainer::LayoutAlign aLayoutAlign = CLayoutContainer::STRETCH);

	// affect the style of contained dialogs
	// set default layout inherited both in case of tabs and no tabs 
	void				SetLayoutType(CLayoutContainer::LayoutType aLayoutType);
	void				SetLayoutAlign(CLayoutContainer::LayoutAlign aLayoutAlign);

	virtual Tile*	AsATile() { return this; }

	void		ModifyTabberStyle		(int style) { m_pTabber->ModifyTabberStyle(style); }
	int			GetTabberDefaultHeight	()			{ return m_pTabber->GetDefaultHeight(); }
	TileStyle*	GetTileDialogStyle		();
	void		SetTileDialogStyle		(TileStyle* pStyle);
	void		SetTileDialogHasTitle	(BOOL bSet = TRUE)		{ m_pTileDialogStyle->SetHasTitle(bSet); }
	void		SetTileDialogCollapsible(BOOL bSet = TRUE)		{ m_pTileDialogStyle->SetCollapsible(bSet); }

	// short way to set the panel aspect as big tile (panel get style from the parent, contained tiles without title)
	void		ShowAsTile				();
	void		SetCollapsedDescription(CString strDescription);
	void		SetActiveTabContentEnable(BOOL bSet);

	// metodi usati da EasyBuilder
	int					GetTabFromPoint	(CPoint pt);
	CTilePanelTab*		GetTab			(int nTab);
	CTilePanelTab*		GetActiveTab	();
	void				SetShowTab		(int nTab, BOOL bVisible);
	BOOL				IsTabVisible	(int nTab);
	int					GetActiveTabNum	();
	int					GetTabIdx		(CTilePanelTab* pTab);
	void				SetActiveTab	(int nTab);
	int					GetTabsNum		();
	virtual CWnd*		GetWndLinkedCtrl(const CTBNamespace& aNS);
	virtual CWnd*		GetWndLinkedCtrl(UINT nIDC);


	// LayoutElement interface implementation
	virtual const	CString				GetElementNameSpace	()										{ return GetInfoOSL()->m_Namespace.ToString(); }
	virtual const	LayoutElementArray*	GetContainedElements()										{ return NULL;}
	virtual			BOOL				IsVisible			()										{ return m_bVisible; }
	virtual			int					GetRequiredHeight	(CRect &rectAvail);
	virtual			int					GetRequiredWidth	(CRect &rectAvail);
	virtual			void				GetAvailableRect	(CRect &rectAvail);
	virtual			void				Relayout			(CRect &rectNew, HDWP hDWP = NULL);
	virtual			void				GetUsedRect			(CRect &rectUsed);

	virtual void	SetMinWidth(int nWidth);
	virtual void	SetMaxWidth(int nWidth);

	virtual	BOOL	IsGroupCollapsible	();
	virtual	void	SetCollapsed		(BOOL bCollapsed = TRUE);
	virtual	void	SetGroupCollapsible	(BOOL bSet = TRUE);

	virtual void OnUpdateTitle();
	virtual void Enable(BOOL bEnable);
	virtual	BOOL PreTranslateMessage(MSG* pMsg);

protected:
	virtual int		GetFlex					(FlexDim dim);
	virtual int		GetMinHeight			(CRect& rect = CRect(0, 0, 0, 0));
	virtual	BOOL	CanDoLastFlex			(FlexDim fd);

	// Tile interface implementation
	virtual CWnd*				GetTileCWnd			()	{ return this; }
	virtual CBaseTileGroup*		GetParentTileGroup	()	{ return m_pParent; }
	virtual void				DoCollapseExpand	();
	virtual void				DoPinUnpin			();
	virtual LayoutElement*		AsALayoutElement	()	{ return this; }
	virtual	BOOL				OnCommand			(WPARAM wParam, LPARAM lParam);

private:
	void	EnsureTabExistance	();
	int		GetTabsHeight		();

protected:
	//{{AFX_MSG(CBaseTileDialog)
	afx_msg void	OnPaint				();
	afx_msg	void	OnLButtonDown		(UINT nFlags, CPoint point);
	afx_msg LRESULT OnValueChanged		(WPARAM, LPARAM);
	afx_msg void	OnWindowPosChanged(WINDOWPOS* lpwndpos);
	afx_msg	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnChangeActiveTab		(WPARAM, LPARAM);
	afx_msg LRESULT OnChangingActiveTab		(WPARAM, LPARAM);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"
