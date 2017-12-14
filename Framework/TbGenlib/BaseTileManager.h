#pragma once

#include "ParsObj.h"

#include "BaseTileDialog.h"
#include "LayoutContainer.h"
#include "TilePanel.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class TileDialogArray;
class CLayoutContainer;

typedef CArray<CTilePanel*, CTilePanel*&>	TilePanelArray;

//*****************************************************************************
// CBaseTileGroup
//*****************************************************************************
class TB_EXPORT CBaseTileGroup : public CWnd, public ResizableCtrl, public LayoutElement, public IDisposingSourceImpl, public IOSLObjectManager 
{
	friend class CLayoutContainer;
	friend class CTilePanel;
	friend class CTilePanelTab;
	friend class CBaseTileDialog;

	DECLARE_DYNAMIC(CBaseTileGroup)
private:
	CBaseDocument*		m_pDocument;

	TileDialogArray		m_TileDialogArray;
	TilePanelArray		m_TilePanelArray;

	TileStyle*			m_pTileDialogStyle;
	BOOL				m_bTransparent;
	BOOL				m_bFillEmptySpace;

	BOOL					m_bSuspendedLayout;
	CLayoutContainer*		m_pLayoutContainer;	
	LayoutContainerArray	m_OwnedLayoutContainers;  //Elementi di cui la classe ha fatto la new, e quindi deve gestire la delete (e' un sottoinsieme di m_Elements)
	BOOL					m_bSuspendedResizeStaticArea;
public:
	TileGroupInfoItem*		m_pDlgInfoItem = NULL;

public:
	CBaseTileGroup();
	virtual ~CBaseTileGroup();

public:
	// Accessibility - Method used to uniquely identify an object by Ranorex Spy
	virtual HRESULT get_accName(VARIANT varChild, BSTR *pszName);
	
	virtual void SetSuspendResizeStaticArea(BOOL bValue);
public:

	void	EnableLayout(BOOL bEnable = TRUE);

	void				OnInitialUpdate(UINT nIDD, CWnd* pParentWnd, CRect rectWnd = CRect(0, 0, 0, 0));

	CBaseTileDialog*	AddTile(CLayoutContainer* pContainer, CBaseTileDialog* pTileDialog, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, CObject* pOwner = NULL);
			
	virtual	CBaseTileDialog*	AddTile(CLayoutContainer* pContainer,	CRuntimeClass*,	UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, CObject* pOwner = NULL);
	virtual	CBaseTileDialog*	AddTile(								CRuntimeClass*,	UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex = AUTO, CObject* pOwner = NULL);

	CTilePanel*					AddStatusPanel(int nColumns = 2);
	
	CLayoutContainer*	AddContainer		
						(
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


	CTilePanel*			AddPanel
		(
		CString strName,
		int		flex = AUTO,
		UINT	nPanelID = -1
		);

	CTilePanel*			AddPanel
		(
		CLayoutContainer*	pParentContainer,
		CString strName,
		int		flex = AUTO,
		UINT	nPanelID = -1
		);

	CTilePanel*			AddPanel
		(
		CString strName,
		const	CString&				strTitle,
		CLayoutContainer::LayoutType	aLayoutType = CLayoutContainer::HBOX,
		CLayoutContainer::LayoutAlign	aLayoutAlign = CLayoutContainer::STRETCH,
		int								flex = AUTO,
		UINT							nPanelID = -1
		);

	CTilePanel*			AddPanel
		(
		CLayoutContainer*				pParentContainer,
		CString strName,
		const	CString&				strTitle,
		CLayoutContainer::LayoutType	aLayoutType = CLayoutContainer::HBOX,
		CLayoutContainer::LayoutAlign	aLayoutAlign = CLayoutContainer::STRETCH,
		int								flex = AUTO,
		UINT							nPanelID = -1
		);

	void				MoveTileByIDD(CBaseTileDialog* pTile, UINT nIDD, bool bAfter);
	void				MoveTile(CBaseTileDialog* pTile, CBaseTileDialog* pBeforeTile);

	void				SuspendLayout(BOOL bSuspend = TRUE)	{ m_bSuspendedLayout = bSuspend; }
	virtual 	void	SetFlex(int flex, BOOL bInContainerToo = TRUE);

	CBaseDocument*	GetDocument		()							{ return m_pDocument; }
	void			AttachDocument	(CBaseDocument* pDocument)	{ m_pDocument = pDocument; }

	TileDialogArray* GetTileDialogs()	{ return &m_TileDialogArray; }
	TilePanelArray* GetTilePanels()		{ return &m_TilePanelArray; }

	const LayoutElementArray*		GetLayoutElementArray() { return m_pLayoutContainer->GetContainedElements(); }

	void  SetTileVisible(CString sTileName, BOOL bVisible);
	BOOL  SetNextControlFocus(CBaseTileDialog* pTileDialog, BOOL bBackward, CBaseTileDialog* pCurrTileDialog);
	bool  SetDefaultFocus();

	void  ResizeStaticArea();

	virtual void OnUpdateControls(BOOL bParentIsVisible = TRUE) {}

	virtual CTBNamespace&	GetNamespace		()				{ return GetInfoOSL()->m_Namespace; }


	virtual CWnd*	GetWndLinkedCtrl(const CTBNamespace& aNS);

	TileStyle*			GetTileDialogStyle	()					{ return m_pTileDialogStyle; }
	void				SetTileHasTitle		(BOOL bSet = TRUE)	{ m_pTileDialogStyle->SetHasTitle(bSet); }
	void				SetTileCollapsible	(BOOL bSet = TRUE)	{ m_pTileDialogStyle->SetCollapsible(bSet); }
	void				SetTransparent		(BOOL bTransparent);
	const BOOL&			IsTransparent		() const;

	virtual	int		GetMinHeight		(CRect& rect = CRect(0, 0, 0, 0));
	virtual	BOOL	CanDoLastFlex		(FlexDim fd);
	virtual	BOOL	PreTranslateMessage	(MSG* pMsg);

public:
	// LayoutElement interface implementation
	virtual 		CLayoutContainer*	GetLayoutContainer	() { return m_pLayoutContainer; }
	virtual const	CString				GetElementNameSpace	() { return GetInfoOSL()->m_Namespace.ToString(); }
	virtual const	LayoutElementArray*	GetContainedElements()								{ return m_pLayoutContainer->GetContainedElements();}
	virtual			BOOL				IsVisible			()								{ return TRUE; }
	virtual			int					GetRequiredHeight	(CRect &rectAvail);
	virtual			int					GetRequiredWidth	(CRect &rectAvail);
	virtual			void				GetAvailableRect	(CRect &rectAvail);
	virtual			void				Relayout			(CRect &rectNew, HDWP hDWP = NULL);
	virtual			void				GetUsedRect			(CRect &rectUsed);
	virtual			void				NotifyChildStateChanged(CTBNamespace aNs, BOOL bState) { /* default do nothing */ }

private:
	// backward compatibility for STRIPES layout only
	virtual CWnd*	GetCWnd				()					{ return this; }
	virtual BOOL	IsFillEmptySpaceMode()					{ return m_bFillEmptySpace; }
	
	CTileDesignModeParamsObj* GetTileDesignModeParams();

protected:
	virtual void	AttachTile(CBaseTileDialog* pTileDialog, CObject* pOwner);

	void	SetFillEmptySpace(BOOL bFillEmptySpace = TRUE) { m_bFillEmptySpace = bFillEmptySpace; }
	
			void	SetLayoutType		(CLayoutContainer::LayoutType aLayoutType);
			void	SetLayoutAlign		(CLayoutContainer::LayoutAlign aLayoutAlign);
	virtual void	OnBeforeCustomize() { /* do nothing	*/ }
	virtual	void	Customize			()				 							{ /* do nothing	*/ }
	virtual void	OnAfterCustomize	() { /* do nothing	*/ }
	virtual	BOOL	OnCommand			(WPARAM wParam, LPARAM lParam);
	virtual	BOOL	SkipRecalcCtrlSize	();
			void	SetTileDialogStyle	(TileStyle* pStyle);

			void	RemoveAllTiles		();
	virtual void	OnTileDialogUnpin	(CBaseTileDialog* pDlg)		{ /* default do nothing */}
	virtual void	OnTileDialogPin		(CBaseTileDialog* pDlg)		{ /* default do nothing */}
			
	//{{AFX_MSG(CBaseTileGroup)
	afx_msg LRESULT OnValueChanged		(WPARAM, LPARAM);
	afx_msg	LRESULT	OnRecalcCtrlSize	(WPARAM, LPARAM);
	afx_msg BOOL	OnEraseBkgnd		(CDC* pDC);
	afx_msg LRESULT	OnCtrlFocused		(WPARAM, LPARAM);
	afx_msg	LRESULT	OnInitializeLayout	(WPARAM, LPARAM);
	afx_msg	void	OnWindowPosChanged	(WINDOWPOS FAR* lpwndpos);
	afx_msg	void 	OnSetFocus			(CWnd*);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
