#pragma once
#include "tbwnd.h"

#define DEFAULT_ITEM_HEIGHT 20
#define TABULATION 15

class TBTreeView;
class TBTreeItem;
class CTBTreeItemArray : public CArray<TBTreeItem*>
{
	TVSORTCB* m_pfnTVSORTCB;
	void Swap(int i, int j);
	BOOL Sort(int nFirst, int nLast);
	int Compare(TBTreeItem* p1, TBTreeItem* p2);
public:
	void Sort(TVSORTCB* pfnTVSORTCB);
	
};

class TBTreeItem
{
public:
	TVITEMEX  m_Item;
	CTBTreeItemArray	m_arChilds;
	TBTreeItem* m_pParent;
	TBTreeView* m_pTree;
	CRect m_ItemRect;
	int m_nIndexInParent;//potrei calcolarlo ma lo tengo per motivi di efficienza

	TBTreeItem(TBTreeView* pTree, TBTreeItem* pParent);

	~TBTreeItem();
	TBTreeItem* Find(HTREEITEM item);
	HTREEITEM GetHandle() { return m_Item.hItem; }
	BOOL GetItemRect(BOOL onlyText, LPRECT lpRect);
	void SetItem(TVITEM* pItem);
	void GetItem(TVITEM* pItem);
	void DeleteChilds();
	BOOL IsExpanded() { return (m_Item.state & TVIS_EXPANDED) == TVIS_EXPANDED; }
	BOOL IsExpandedOnce() { return (m_Item.state & TVIS_EXPANDEDONCE) == TVIS_EXPANDEDONCE; }
	void Expand(BOOL bExpand);
	void Add(TBTreeItem* pItem);
	void Remove(TBTreeItem* pItem);
	void InsertAt(int index, TBTreeItem* pItem);
	int GetItemHeight(BOOL bWithChilds);
	int GetItemDepth();
	TBTreeItem* GetPrevVisible();
	int GetPrevVisibleBottomMargin();
	void OffsetChilds(int delta);
	void OffsetNextSiblings(int delta);
	int RecalcChildRect(BOOL bRecursive);
};

class TBTreeView : public TBWnd
{
	friend class TBTreeItem;
	TBTreeItem* m_pRootItem;
	HIMAGELIST m_hImageList;
	HIMAGELIST m_hStateImageList;
	int m_nItemHeight;
	TBTreeItem* m_pSelectedItem;
public:
	TBTreeView(HWND hwnd, DWORD dwThreadId) 
		:
	TBWnd(hwnd, dwThreadId),
		m_hImageList(NULL),
		m_hStateImageList(NULL),
		m_nItemHeight(DEFAULT_ITEM_HEIGHT),
		m_pSelectedItem(NULL)
	{
		m_pRootItem = new TBTreeItem(this, NULL);
		m_pRootItem->Expand(TRUE);//nodo fitizio che ospita i root nodes: per definizione nasce espans
	}
	~TBTreeView()
	{
		delete m_pRootItem;
	}
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
private:
	TBTreeItem* Find(HTREEITEM item);
};

