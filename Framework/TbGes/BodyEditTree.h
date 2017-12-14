#pragma once


#include "bodyedit.h"
#include "dbttreeedit.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//-----------------------------------------------------------------------------
#define TBE_TREE_TEXT_INDENTATION	25
//-----------------------------------------------------------------------------

///////////////////////////////////////////////////////////////////////////////
// CTreeBodyEdit - CTBExtTreeEdit
//-----------------------------------------------------------------------------
class TB_EXPORT CTreeBodyEdit : public CBodyEdit
{
	DECLARE_DYNAMIC(CTreeBodyEdit)
//private:
//	using CBodyEdit::m_pDBT;

protected:
	// tree
	ColumnInfo*			m_pTreeEditColInfo;
	BOOL				m_bShowAlwaysTreeStruct;
	int					m_nTreeEditColInfoCellXEnd;
	int					m_nMaxLevel;

	// buttons
	CBEButton*			m_pBtnBTInsert;
	CBEButton*			m_pBtnBTInsertChild;

	CBEButton*			m_pBtnMoveLeftAll;
	CBEButton*			m_pBtnMoveRightAll;
	CBEButton*			m_pBtnMoveLeft;
	CBEButton*			m_pBtnMoveRight;
	CBEButton*			m_pBtnExpandCollapseNode;
	CBEButton*			m_pBtnExpandCollapseAll;
	CBEButton*			m_pBtnShowLevel;

	CBEButton*			m_pBtnSort;
	CBEButton*			m_pBtnBTFind;

protected:
	CTreeBodyEdit (const CString sName = _T(""));
	virtual ~CTreeBodyEdit();

protected:
	virtual BOOL	OnCreateClient();

	virtual	BOOL	OnExtTextOut		(int nXTxtStart, int nYStart, CDC* pDC, CRect& rectDraw, const CString& strCell, SqlRecord* pRec, ColumnInfo* pCol);
	virtual	BOOL	OnDrawText			(CDC* pDC, CRect& rectDraw, const CString& strCell, SqlRecord*, ColumnInfo*);
	virtual	BOOL	IsColumnSingleLine	(ColumnInfo* pCol)  { return pCol == m_pTreeEditColInfo; }
	virtual BOOL	OnShowCtrl			(SqlRecord*, ColumnInfo*, int /*xPos*/, int /*yPos*/, CRect&);
	virtual BOOL	OnDrawCurrentCell (int /*nRow*/, int /*nCol*/ ) { return TRUE; }

	virtual void	EnableButtons		();

	virtual void	TBEDrawTreeBitmap	(CDC& DCDest, const CRect& rect, const CTreeBodyEditNodeInfo& ni);

	virtual	BOOL	DoMovingKey			(UINT nChar);
	
	virtual	BOOL	OnShowContextMenu	(CTBMenu*);

	virtual void	AddCtrlOffset		(ColumnInfo*, int&);

	BOOL	PointOnButton			(const CRect& rect, const CPoint& pt, const CTreeBodyEditNodeInfo& ni);
	BOOL	PointOnBitmap			(const CRect& rect, const CPoint& pt, const CTreeBodyEditNodeInfo& ni);
	BOOL	PointOnBitmap			(const CRect& rect, const CPoint& pt, int nRow);

	void	AddButton				(const CString& sName, CBEButton*& pBtn, UINT ID, UINT IDB, const CString& strTooltip, const CString& strText = _T(""));
	void	AddButton				(const CString& sName, CBEButton*& pBtn, UINT ID, const CString& nsImage, const CString& strTooltip, const CString& strText = _T(""));

	void	OnInsertRecord			();
	void	OnInsertChildRecord		();

	void	OnSortAll				();
	void	OnSortNode				();

public:
	void	ShowMoveLeftAllBtn			();
	void	ShowMoveRightAllBtn			();
	void	ShowMoveLeftBtn				();
	void	ShowMoveRightBtn			();
	void	ShowExpandCollapseNodeBtn	();
	void	ShowExpandCollapseAllBtn	();
	void	ShowLevelMenuBtn			();
	void	ShowSortMenuBtn				();
	void	ShowInsertBtn				();
	void	ShowInsertChildBtn			();

	void	ShowAlwaysTreeStruct	(BOOL bSet = TRUE)	{ m_bShowAlwaysTreeStruct = bSet; }

	DBTTree*		GetDBT () { ASSERT_VALID(m_pDBT); ASSERT_KINDOF(DBTTree, m_pDBT); return (DBTTree*) m_pDBT; }

	virtual BOOL	CanDoInsert			() { return __super::CanInsertRowByBodyTree(); }
	virtual BOOL	CanDoInsertChild	();// { return CanDoInsert(); }

	virtual void	OnDoSearch			();
	virtual BOOL	CanHideColumn		(ColumnInfo*);

	void	OnToggleExpandNode	(int nRow);

	BOOL	IsEnableExpandCollapseAll	();
	BOOL	IsEnableExpandCollapseNode	();

protected:
	virtual BOOL InternalCanDeleteRow	();
	virtual void OnToggleExpandNode		(int nRow, CTreeBodyEditNodeInfo& ni);
	afx_msg void OnLButtonDown			(UINT nFlags,	CPoint point);

	void	OnMoveLeftAll				();
	void	OnMoveRightAll				();
	void	OnMoveLeft					();
	void	OnMoveRight					();

	void	OnExpand					();
	void	OnCollapse					();
	void	OnExpandAll					();
	void	OnCollapseAll				();

	void	OnExpandCollapse			();
	void	OnExpandCollapseAll			();
	//void	OnExpandEdge		();
	//void	OnCollapseEdge		();

	void	OnShowLevelMenu				();
	void	OnShowSortMenu				();

	DECLARE_MESSAGE_MAP()
};


//==============================================================================
#include "endh.dex"
