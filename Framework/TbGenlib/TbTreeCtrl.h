#pragma once

#include <BCGCBPro\BCGPTreeCtrl.h>

#include <TbGeneric\array.h>
#include <TbGeneric\WndObjDescription.h>

#include "ParsObj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//-----------------------------------------------------------------------
class TB_EXPORT CHTreeItem : public CObject
{
public:
	CHTreeItem(HTREEITEM hItem){ m_hItem = hItem; };

public:
	HTREEITEM m_hItem;
};

//-----------------------------------------------------------------------
class TB_EXPORT CHTreeItemsArray : public Array
{
public:
	int				Add			(CHTreeItem* pEl)	{ return Array::Add(pEl); }
	CHTreeItem* 	GetAt		(int nIdx) const	{ return (CHTreeItem*)Array::GetAt(nIdx); }
	BOOL			Contains	(HTREEITEM item);
	void			Remove		(HTREEITEM item);
};

//-----------------------------------------------------------------------
class  TB_EXPORT CLevelGrantItem : public CObject
{
public:
	CLevelGrantItem(HTREEITEM hItem){ m_hItemLevel = hItem; };

public:
	HTREEITEM m_hItemLevel;
};

//-----------------------------------------------------------------------
class  TB_EXPORT CLevelGrantArray : public Array
{
public:
	int					Add		(CLevelGrantItem* pEl)	{ return Array::Add(pEl); }
	CLevelGrantItem* 	GetAt	(int nIdx) const		{ return (CLevelGrantItem*)Array::GetAt(nIdx); }
	BOOL				Contains(CLevelGrantItem* item);
	void				Remove	(CLevelGrantItem* item);
};

//=============================================================================
//			Class CTBTreeCtrl
//=============================================================================
class TB_EXPORT CTBTreeCtrl : public CBCGPTreeCtrl, public ResizableCtrl
{
	DECLARE_DYNAMIC(CTBTreeCtrl)

public:
	CTBTreeCtrl();
	virtual ~CTBTreeCtrl();

protected:
	CImageList			m_ImageList;
	int					m_MaxLevel;

	HTREEITEM			m_hOldItem;
	DWORD				m_dwOldTime;

	BOOL				m_bMultiSelect;
	

	CLevelGrantArray	m_HItemLevel;

	CList<HTREEITEM, HTREEITEM>* pHitemList;

public:
	HTREEITEM	m_hDragItem = NULL;
	BOOL		m_bDeleting = FALSE;
	BOOL		m_bMultiSelectCustom;
public:
	virtual void	RemoveTreeChilds	(HTREEITEM);
	virtual BOOL	DeleteItem			(_In_ HTREEITEM hItem);

	HTREEITEM		InsertItem//(CString, HTREEITEM = TVI_ROOT, HTREEITEM = TVI_LAST, int = 0, int = 0);
						(
							const CString&	strText,
							int				nImage			= 0,
							int				nSelectedImage	= 0,
							HTREEITEM		hParentItem		= TVI_ROOT,
							HTREEITEM		hInsertAfter	= TVI_LAST
						);

	void			GetSelectedStrings(CStringArray*, HTREEITEM);
	void			GetSelectedHItems(CHTreeItemsArray*, HTREEITEM = NULL);
	int				GetItemLevel(HTREEITEM);

	BOOL			CanSelectLevel(HTREEITEM hItem);
	void			ClearSelection();

protected:
	void			DeselectLevel(int);
	virtual BOOL	OnMultiSelect() { return FALSE; }
	BOOL			SelectItems(HTREEITEM hItemFrom, HTREEITEM hItemTo, BOOL bRemovesOtherSelection = TRUE);
	HTREEITEM		TbGetNextItem(HTREEITEM hItem);

public:
	virtual	void	InitializeImageList();
	virtual void	SelectAll(HTREEITEM, UINT);
	virtual void	ExpandAll(HTREEITEM, UINT);
	virtual void	ExpandAll(UINT);
	virtual BOOL	CanSelect(HTREEITEM hItem){ return TRUE; }
	virtual BOOL	CanSelect(CHTreeItemsArray*){ return TRUE; }
	virtual void	OnContextMenu(CWnd* pWnd, CPoint mousePos);
	virtual void	OnRename();

	virtual HTREEITEM	FindItemData(DWORD, HTREEITEM hParentItem = TVI_ROOT, BOOL bFindNextSibling = FALSE);
	virtual BOOL		IsEqualItemData(DWORD dwItemData, DWORD dwExternalData);

	virtual HTREEITEM	FindItemText(const CString&, HTREEITEM hItem = NULL);

	HTREEITEM GetLastChild(HTREEITEM htItem);

	void	ToggleItemSelect(HTREEITEM hItem);
	void	DeselectItem	(HTREEITEM hItem);
protected:
	virtual HTREEITEM	FindItemText(const CString&, const HTREEITEM hItem, BOOL bFindNextSibling, BOOL bFindParentNextSibling, BOOL bSkipCurrent);
	virtual BOOL			OnFindItemTextOnFirstChild(HTREEITEM htItem) { return TRUE; }
	virtual BOOL			OnFindItemTextOnChild(HTREEITEM htItem, const CString&) { return FALSE; }
	virtual void			OnFindItemTextOnLastChild(HTREEITEM htItem) {}
		
	//virtual HTREEITEM	Move(HTREEITEM hItem, BOOL bNext);

protected:
	afx_msg void OnLButtonDown	(UINT nFlags, CPoint point);
	afx_msg void OnRButtonDown	(UINT nFlags, CPoint point);
	
	LRESULT OnGetControlDescription(WPARAM wParam, LPARAM lParam);

public:
	virtual	BOOL	SubclassDlgItem(UINT, CWnd*);

protected:
	afx_msg	LRESULT	OnRecalcCtrlSize(WPARAM, LPARAM);

	DECLARE_MESSAGE_MAP()

	// Diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext&)	const;
	void AssertValid()			const { __super::AssertValid(); }
#endif // _DEBUG
};

//=============================================================================
//			Class CWndTreeCtrlDescription
//=============================================================================

class TB_EXPORT CWndTreeCtrlDescription : public CWndImageDescription
{
	DECLARE_DYNCREATE(CWndTreeCtrlDescription);
	//gestione stili

	bool m_bCheckBoxes = false;
	bool m_bDisableDragDrop = false;
	bool m_bEditLabels = false;
	bool m_bHasButtons = false;
	bool m_bHasLines = false;
	bool m_bInfoTip = false;
	bool m_bLinesAtRoot = false;
	bool m_bNoTooltips = true;
	bool m_bAlwaysShowSelection = false;

protected:
	CWndTreeCtrlDescription()
	{
		m_Type = CWndObjDescription::Tree;
		m_bBorder = GetDefaultBorder();
	}

public:
	int m_nIconHeight = 0;
	int m_nIcons = 0;

	CWndTreeCtrlDescription(CWndObjDescription* pParent);
	~CWndTreeCtrlDescription() {}
	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
	virtual void UpdatePropertiesFromStyle(DWORD dwStyle, DWORD dwExStyle);
	virtual void ApplyStyleFromProperties(DWORD& dwStyle, DWORD& dwExStyle);

	virtual bool GetDefaultBorder() { return true; }
};

//=============================================================================
//			Class CWndTreeNodeDescription
//=============================================================================

class TB_EXPORT CWndTreeNodeDescription : public CWndImageDescription
{
	DECLARE_DYNCREATE(CWndTreeNodeDescription);

	bool	m_bExpanded = false;
	bool	m_bSelected = false;
	bool	m_bHasChild = false;
	bool	m_bHasStateImg = false;
	int		m_nIcon = -1;
	int		m_nSelectedIcon = -1;

private:
	CWndTreeNodeDescription();

public:
	CWndTreeNodeDescription(CWndObjDescription* pParent);
	~CWndTreeNodeDescription();

	void GetItemDescription(HTREEITEM hItem, CTBTreeCtrl* pTreeCtrl, CWndTreeCtrlDescription* pTreeDesc, CPoint ptScrollOffset);

	virtual void Assign(CWndObjDescription* pDesc);

	virtual void SerializeJson(CJsonSerializer& strJson);
	virtual void ParseJson(CJsonFormParser& parser);
};

//-----------------------------------------------------------------------------
#include "endh.dex"

