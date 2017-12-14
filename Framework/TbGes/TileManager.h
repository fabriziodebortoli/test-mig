#pragma once

#include <TbGenLib\BaseTileManager.h>
#include "ExtDocView.h"
#include "Tabber.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//////////////////////////////////////////////////////////////////////////////
//							CTabDialogTileGroup definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT CTabDialogTileGroup : public CTabDlgEmpty, public ITabDialogTileGroup
{
	DECLARE_DYNCREATE(CTabDialogTileGroup);
public:
	
	CTabDialogTileGroup(CString sName = _T(" "), UINT nIDD = IDD_EMPTY_TAB) : CTabDlgEmpty(sName, nIDD) {}

private:
	BOOL OnInitDialog();
	virtual BOOL DestroyWindow();
	virtual void CustomizeExternal();
	virtual void OnBeforeAttachParents(CAbstractFormDoc* pDoc, CAbstractFormView* pView, CTabManager* pTabber);

	DECLARE_MESSAGE_MAP()

	//{{AFX_MSG(CTabDialogTileGroup)
	afx_msg void OnWindowPosChanged(WINDOWPOS* lpwndpos);
	afx_msg void OnDestroy();
	//}}AFX_MSG

};

//////////////////////////////////////////////////////////////////////////////
//							CTabDialogTileGroup definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT CRowTabDialogTileGroup : public CRowTabDialog, public ITabDialogTileGroup
{
	DECLARE_DYNCREATE(CRowTabDialogTileGroup);
public:

	CRowTabDialogTileGroup() : CRowTabDialog(_T(""), IDD_EMPTY_TAB){	}


private:
	BOOL OnInitDialog();
	virtual BOOL DestroyWindow();
	virtual void OnBeforeAttachParents(CAbstractFormDoc* pDoc, CAbstractFormView* pView, CTabManager* pTabber);

	DECLARE_MESSAGE_MAP()

	//{{AFX_MSG(CRowTabDialogTileGroup)
	afx_msg void OnWindowPosChanged(WINDOWPOS* lpwndpos);
	afx_msg void OnDestroy();
	//}}AFX_MSG

};


//////////////////////////////////////////////////////////////////////////////
//							TileGroupInfoItem definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT TileGroupInfoItem : public DlgInfoItem
{
	friend class CTabDialogTileGroup;

	DECLARE_DYNAMIC (TileGroupInfoItem)

	CString				m_sName;
	CRuntimeClass*		m_pTileGroupClass;
	UINT				m_nTileGroupID;

	TileGroupInfoItem(CRuntimeClass* pClass, UINT nDialogID, int nOrdPos, UINT nTileGroupID)
		:
	DlgInfoItem(pClass, nDialogID, nOrdPos)
	{
		m_nTileGroupID = nTileGroupID;
	}

	UINT GetTileGroupID () const { return m_nTileGroupID; }

	CString GetName() { return m_sName; }
};


//*****************************************************************************
// CTileGroup
//*****************************************************************************
class TB_EXPORT CTileGroup: public CBaseTileGroup
{
	DECLARE_DYNCREATE(CTileGroup)
	DECLARE_MESSAGE_MAP()
public:
	void EnableViewControlLinks(BOOL bEnable, BOOL bMustSetOSLReadOnly);
	void OnUpdateControls(BOOL bParentIsVisible = TRUE);
	void OnResetDataObjs();
	void OnFindHotLinks();
	BOOL PrepareAuxData();

	CBodyEdit* GetBodyEdits (const CTBNamespace& aNS);
	CBodyEdit* GetBodyEdits (int* pnStartIdx);

	CParsedCtrl* GetLinkedParsedCtrl (DataObj* pDataObj);
	CParsedCtrl* GetLinkedParsedCtrl (const CTBNamespace& aNS);
	CParsedCtrl* GetLinkedParsedCtrl (UINT nIDC);

	CWnd*		 GetWndCtrl(UINT nIDC);

	CBaseTileDialog* GetTileDialog(UINT nIDD);
	CBaseTileDialog* GetTileDialog(CTBNamespace aNs);
	CTilePanel*		 GetTilePanel(UINT nIDD);
	CBaseTileDialog* AddJsonTile(UINT nDialogID, CLayoutContainer* pContainer = NULL);

	void Enable				(BOOL bEnable);
	void EnableTile			(UINT nIDD, BOOL bEnable);
	void EnableTilePanel	(UINT nIDD, BOOL bEnable);
	void Show				(BOOL bShow);

	virtual void NotifyChildStateChanged(CTBNamespace aNs, BOOL bState);

	virtual void RebuildLinks	(SqlRecord*);
	virtual BOOL OnPrepareAuxData()	{ return TRUE; }
	virtual void OnBeforeCustomize();
	virtual void OnAfterCustomize();
	afx_msg	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
};

//*****************************************************************************
// CTileManager
//*****************************************************************************
class TB_EXPORT CTileManager: public CTabManager
{
public:
	CTileManager();
	//CTileManager(DWORD dwStyle, DWORD dwStyleEx);
	~CTileManager();

	TileGroupInfoItem* AddTileGroup(CRuntimeClass* pTileGroupClass, CString sNameTileGroup, CString sTitleTileGroup, CString sTileGroupImage = _T(""), CString sTooltip = _T(""), UINT nTileGroupID = 0);
	CRect	GetSelectorRect();

	void	MoveTileGroup(TileGroupInfoItem*, int nTo);

protected:
	virtual DlgInfoItem* CreateDlgInfoItem(CRuntimeClass*	pDialogClass, UINT nDialogID, int nOrdPos = -1);
	virtual int		InsertDlgInfoItem	(int, DlgInfoItem*);

	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);
	
	DECLARE_DYNCREATE(CTileManager)
	DECLARE_MESSAGE_MAP()
};


////////////////////////////////////////////////////////////////////////////////
//				class TileGroups definition
////////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT TileGroups : public Array
{
	DECLARE_DYNCREATE(TileGroups)

public:
	// overloaded operator helpers
	CTileGroup*	GetAt			(int nIndex) const	{ return (CTileGroup*) Array::GetAt(nIndex);}
	CTileGroup*	Get				(int nIDC) const	
													{ 
														for (int i =0; i < GetSize(); i++)
															if (GetAt(i)->GetDlgCtrlID() == nIDC)
																return GetAt(i);
														return NULL;
													}

#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif
};    


