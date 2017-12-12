#include "stdafx.h"

#include <tbgeneric\globals.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

// local
#include "bodyedit.hjson" //JSON AUTOMATIC UPDATE
#include "DBTTreeEdit.h"
#include "FormMng.h"
#include "BodyEditTree.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

///////////////////////////////////////////////////////////////////////////////
// CTBExtTreeBEEditable
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC	(CTreeBodyEdit, CBodyEdit)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP	(CTreeBodyEdit, CBodyEdit)

	ON_WM_LBUTTONDOWN		()

	ON_COMMAND				(ID_BE_TREE_EXPAND_COLLAPSE,		OnExpandCollapse)
	ON_COMMAND				(ID_BE_TREE_EXPAND_COLLAPSE_ALL,	OnExpandCollapseAll)

	ON_COMMAND				(IDC_BE_INSERT,						OnInsertRecord)
	ON_COMMAND				(ID_BE_TREE_INSERT,					OnInsertRecord)

	ON_COMMAND				(ID_BE_TREE_INSERTCHILD,			OnInsertChildRecord)

	ON_COMMAND				(ID_BE_TREE_MOVELEFTALL,			OnMoveLeftAll)
	ON_COMMAND				(ID_BE_TREE_MOVERIGHTALL,			OnMoveRightAll)

	ON_COMMAND				(ID_BE_TREE_MOVELEFT,				OnMoveLeft)
	ON_COMMAND				(ID_BE_TREE_MOVERIGHT,				OnMoveRight)

	ON_COMMAND				(ID_BE_TREE_SORTALL,				OnSortAll)
	ON_COMMAND				(ID_BE_TREE_SORTNODE,				OnSortNode)

	ON_COMMAND				(ID_BE_TREE_SHOWLEVELMENU,			OnShowLevelMenu)
	ON_COMMAND				(ID_BE_TREE_SHOWSORTMENU,			OnShowSortMenu)

END_MESSAGE_MAP		()

//-----------------------------------------------------------------------------
CTreeBodyEdit::CTreeBodyEdit(const CString sName)
	:
	CBodyEdit(sName),

	m_pBtnMoveLeft				(NULL),
	m_pBtnMoveRight				(NULL),
	m_pBtnMoveLeftAll			(NULL),
	m_pBtnMoveRightAll			(NULL),
	m_pBtnExpandCollapseNode	(NULL),
	m_pBtnExpandCollapseAll		(NULL),
	m_pBtnShowLevel				(NULL),
	m_pBtnSort					(NULL),
	m_pBtnBTInsert				(NULL),
	m_pBtnBTInsertChild			(NULL),
	m_pBtnBTFind				(NULL),

	m_pTreeEditColInfo			(NULL),
	m_bShowAlwaysTreeStruct		(TRUE),
	m_nTreeEditColInfoCellXEnd	(0),
	m_nMaxLevel					(0)
{
	//TRACE3("\nsizeof CTreeBodyEdit in tb: %d, %d, %d\n", sizeof(CBodyEdit), sizeof(CTreeBodyEdit), sizeof(*this));
	BESetExStyle(BE_STYLE_SHOW_HEADER_TOOLBAR, TRUE);
}

//-----------------------------------------------------------------------------
CTreeBodyEdit::~CTreeBodyEdit()
{
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::OnCreateClient()
{
	ASSERT_VALID(this);
	ASSERT(m_pTreeEditColInfo == NULL);
	ASSERT_VALID(m_pDBT);
	if (!m_pDBT)
		return FALSE;
	DBTTree* pDBT = GetDBT();
	ASSERT_VALID(pDBT->GetTreeEditObj());

	for (int c = 0; c <= m_AllColumnsInfo.GetUpperBound(); c++)
	{
		ColumnInfo* pColInfo = m_AllColumnsInfo.GetAt(c);
		ASSERT_VALID(pColInfo);
		ASSERT(pColInfo->GetParsedCtrl());
		ASSERT_VALID(pDBT->GetTreeEditObj());
		ASSERT_VALID(pColInfo->GetParsedCtrl()->GetCtrlData());

		if (pDBT->GetTreeEditObj() == pColInfo->GetParsedCtrl()->GetCtrlData())
		{
			m_pTreeEditColInfo = pColInfo;

			m_pTreeEditColInfo->SetTextAlign(TA_LEFT);
			m_pTreeEditColInfo->SetStatus(STATUS_NOCHANGE_HIDDEN | m_pTreeEditColInfo->GetStatus());
			break;
		}
	}

	if (!m_pTreeEditColInfo)
	{
		ASSERT_TRACE1(FALSE, "CTreeBodyEdit::OnCreateClient: In the BodyEdit %s the field with the tree structure isn't linked", GetRuntimeClass()->m_lpszClassName);
		return FALSE;
	}

	BOOL bOk = __super::OnCreateClient();

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::CanHideColumn (ColumnInfo* pCol)
{
	if (pCol == m_pTreeEditColInfo)
		return FALSE;
	return __super::CanHideColumn (pCol);
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::AddCtrlOffset (ColumnInfo* pCol, int& width) 
{
	if (pCol == m_pTreeEditColInfo)
	{
		width += TBE_TREE_TEXT_INDENTATION * max(1, this->m_nMaxLevel);
		if (m_nBmpMaxWidth)
			width += m_nBmpMaxWidth + DEFAULT_COLUMN_OFFSET;
	}
}	

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::OnShowContextMenu (CTBMenu* pMenu)
{
	DBTTree* pDBT = GetDBT();

	pMenu->AppendMenu	(MF_SEPARATOR, 0, NULL);

	pMenu->AppendMenu	(MF_STRING | (IsEnableExpandCollapseAll() ? MF_ENABLED : MF_DISABLED | MF_GRAYED), ID_BE_TREE_EXPAND_COLLAPSE_ALL, (LPTSTR) (LPCTSTR) _TB("Expand/Collapse All"));
	
	pMenu->AppendMenu	(MF_STRING | (IsEnableExpandCollapseNode() ? MF_ENABLED : MF_DISABLED | MF_GRAYED), ID_BE_TREE_EXPAND_COLLAPSE, (LPTSTR) (LPCTSTR) _TB("Expand/Collapse Node"));
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::AddButton(const CString& sName, CBEButton*& pBtn, UINT nID, UINT nIDB, const CString& strTooltip, const CString& strText /* = _T("") */)
{
	ASSERT(pBtn == NULL);

	pBtn = m_HeaderToolBar.AddButton (sName, nID, nIDB, strTooltip, strText);
	ASSERT_VALID(pBtn);
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::AddButton(const CString& sName, CBEButton*& pBtn, UINT nID, const CString& nsImage, const CString& strTooltip, const CString& strText /* = _T("") */)
{
	ASSERT(pBtn == NULL);

	pBtn = m_HeaderToolBar.AddButton(sName, nID, nsImage, strTooltip, strText);

	ASSERT_VALID(pBtn);
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowMoveLeftAllBtn()
{
	AddButton(L"MoveLeftAll", m_pBtnMoveLeftAll, ID_BE_TREE_MOVELEFTALL, TBIcon(szIconBeTreeMoveLeftAll, TOOLBAR), _TB("Left shift"));
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowMoveRightAllBtn()
{
	AddButton(L"MoveRightAll", m_pBtnMoveRightAll, ID_BE_TREE_MOVERIGHTALL, TBIcon(szIconBeTreeMoveRightAll, TOOLBAR), _TB("Right shift"));
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowMoveLeftBtn()
{
	AddButton(L"MoveLeft", m_pBtnMoveLeft, ID_BE_TREE_MOVELEFT, TBIcon(szIconBeTreeMoveLeft, TOOLBAR), _TB("Left shift current row"));
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowMoveRightBtn()
{
	AddButton(L"MoveRight", m_pBtnMoveRight, ID_BE_TREE_MOVERIGHT, TBIcon(szIconBeTreeMoveRight, TOOLBAR), _TB("Right shift current row"));
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowExpandCollapseNodeBtn()
{
	AddButton(L"ExpandCollapseNode", m_pBtnExpandCollapseNode, ID_BE_TREE_EXPAND_COLLAPSE, TBIcon(szIconBeTreeExpand, TOOLBAR), _TB("Expand/Collapse Node"));
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowExpandCollapseAllBtn()
{
	AddButton(L"ExpandCollapseAll", m_pBtnExpandCollapseAll, ID_BE_TREE_EXPAND_COLLAPSE_ALL, TBIcon(szIconBeTreeExpandAll, TOOLBAR), _TB("Expand/Collapse All"));
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowLevelMenuBtn()
{
	ASSERT(m_pBtnShowLevel == NULL);		// si sta tentando di abilitare due volte lo stesso bottone

	m_pBtnShowLevel = m_HeaderToolBar.AddButton(L"ShowLevelMenu", ID_BE_TREE_SHOWLEVELMENU, TBIcon(szIconBeTree, TOOLBAR), _TB("Show"));
	ASSERT_VALID(m_pBtnShowLevel);
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowSortMenuBtn()
{
	ASSERT(m_pBtnSort == NULL);		// si sta tentando di abilitare due volte lo stesso bottone

	m_pBtnSort = m_HeaderToolBar.AddButton(L"ShowSortMenu", ID_BE_TREE_SHOWSORTMENU, TBIcon(szIconAlphSortingFilled, TOOLBAR), _TB("Sort"));
	ASSERT_VALID(m_pBtnSort);
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowInsertBtn()
{
	AddButton(L"InsertNode", m_pBtnBTInsert, ID_BE_TREE_INSERT, TBIcon(szIconBeTreeInsert, TOOLBAR), _TB("New node"));
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::ShowInsertChildBtn()
{
	AddButton(L"InsertChild", m_pBtnBTInsertChild, ID_BE_TREE_INSERTCHILD, TBIcon(szIconBeTreeInsertChild, TOOLBAR), _TB("New child node"));
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::EnableButtons()
{
	ASSERT_VALID(this);
	__super::EnableButtons();

	DBTTree* pDBT = GetDBT();

	m_HeaderToolBar.EnableButton(m_pBtnMoveLeft,				pDBT->CanDoMoveLeft			());
	m_HeaderToolBar.EnableButton(m_pBtnMoveRight,				pDBT->CanDoMoveRight		());
	m_HeaderToolBar.EnableButton(m_pBtnMoveLeftAll,				pDBT->CanDoMoveLeft			());
	m_HeaderToolBar.EnableButton(m_pBtnMoveRightAll,			pDBT->CanDoMoveRight		());

	m_HeaderToolBar.EnableButton(m_pBtnExpandCollapseNode,		IsEnableExpandCollapseNode());
	m_HeaderToolBar.EnableButton(m_pBtnExpandCollapseAll,		IsEnableExpandCollapseAll());

	m_HeaderToolBar.EnableButton(m_pBtnShowLevel,				pDBT->CanDoShowLevel		());

	m_HeaderToolBar.EnableButton(m_pBtnBTInsert,				CanDoInsert() && GetMultiSelMode() != MULTIPLE_SEL);
	m_HeaderToolBar.EnableButton(m_pBtnBTInsertChild,			CanDoInsertChild() && GetMultiSelMode() != MULTIPLE_SEL);

	m_HeaderToolBar.EnableButton(m_pBtnSort,					pDBT->CanDoSort		());
	m_HeaderToolBar.EnableButton(m_pBtnBTFind,					pDBT->CanDoFind		());
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::CanDoInsertChild	()
{ 
	if (!CanDoInsert())
		return FALSE; 

	DBTTree* pDBT = GetDBT();
	SqlRecord* pRec = pDBT->GetCurrentRow();
	if (!pRec)
		return FALSE;

	if (m_nMaxLevel == 0)
		return TRUE;

	CTreeBodyEditNodeInfo ni(pDBT->GetTreeData(pRec));

	return ni.GetLevel() < m_nMaxLevel;
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::OnExtTextOut(int nXTxtStart, int nYStart, CDC* pDC, CRect& rectDraw, const CString& strCell, SqlRecord* pRec, ColumnInfo* pCol)
{
	// se non c'è effettivamente una struttura ad albero chiamo solo la DrawRow standard
	// e non ne forzo la visualizzazione
	if (!m_bShowAlwaysTreeStruct && GetDBT()->GetMaxLevel() == 1)
		return CBodyEdit::OnExtTextOut (nXTxtStart, nYStart, pDC, rectDraw, strCell, pRec, pCol);;

	if (pCol != m_pTreeEditColInfo)
		return CBodyEdit::OnExtTextOut (nXTxtStart, nYStart, pDC, rectDraw, strCell, pRec, pCol);
	
	ASSERT_VALID(m_pTreeEditColInfo);

	CTreeBodyEditNodeInfo ni(GetDBT()->GetTreeData(pRec));
	int nLevel = ni.GetLevel();

	nXTxtStart += TBE_TREE_TEXT_INDENTATION * nLevel;
	if (m_nBmpMaxWidth)
		nXTxtStart += m_nBmpMaxWidth + DEFAULT_COLUMN_OFFSET;

	pDC->ExtTextOut	
		(
			nXTxtStart, nYStart,
			ETO_OPAQUE | ETO_CLIPPED, &rectDraw,
			strCell, strCell.GetLength(),
			NULL
		);

	m_nTreeEditColInfoCellXEnd = nXTxtStart + GetEditSize(pDC, GetFont(), strCell).cx + DEFAULT_COLUMN_OFFSET;

	if (pCol->GetScreenWidth() > 0)
		TBEDrawTreeBitmap (*pDC, rectDraw, ni);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::OnDrawText(CDC* pDC, CRect& rectDraw, const CString& strCell, SqlRecord* pRec, ColumnInfo* pCol)
{
	// se non c'è effettivamente una struttura ad albero chiamo solo la DrawRow standard
	// e non ne forzo la visualizzazione
	if (!m_bShowAlwaysTreeStruct && GetDBT()->GetMaxLevel() == 1)
		return CBodyEdit::OnDrawText (pDC, rectDraw, strCell, pRec, pCol);

	if (pCol != m_pTreeEditColInfo)
		return CBodyEdit::OnDrawText (pDC, rectDraw, strCell, pRec, pCol);
	ASSERT_VALID(m_pTreeEditColInfo);
		
	CTreeBodyEditNodeInfo ni(GetDBT()->GetTreeData(pRec));
	int nLevel = ni.GetLevel();

	rectDraw.left += TBE_TREE_TEXT_INDENTATION * nLevel;
	if (m_nBmpMaxWidth)
		rectDraw.left += m_nBmpMaxWidth + DEFAULT_COLUMN_OFFSET;

	pDC->DrawText (strCell, strCell.GetLength(), &rectDraw, DT_WORDBREAK | DT_LEFT);

	m_nTreeEditColInfoCellXEnd = rectDraw.left + GetEditSize(pDC, GetFont(), strCell).cx + DEFAULT_COLUMN_OFFSET;

	if (pCol->GetScreenWidth() > 0)
		TBEDrawTreeBitmap(*pDC, rectDraw, ni);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::TBEDrawTreeBitmap (CDC& DCDest, const CRect& rect, const CTreeBodyEditNodeInfo& ni)
{
	int	x = rect.left + ni.GetLevel() * TBE_TREE_TEXT_INDENTATION;
	CRect	square(0,0, 9,9);
	x -= 10;
	int y = ((GetEditSize(&DCDest, GetFont(), 1, 1).cy - square.Height()) / 2) + rect.top;

	if (ni.HasChild())
	{
		square.OffsetRect(x, y);
		CPen	pen(PS_SOLID, 1, DCDest.GetTextColor());

		CBrush*	pOldBrush	= (CBrush*)DCDest.SelectStockObject(NULL_BRUSH);
		CPen*	pOldPen		= DCDest.SelectObject(&pen);

		DCDest.Rectangle(square);
		DCDest.MoveTo(square.left+2, y+square.Height() / 2);
		DCDest.LineTo(square.right-2, y+square.Height() / 2);

		if (!ni.IsExpanded())
		{
			DCDest.MoveTo(square.left + square.Width() / 2, square.top+2);
			DCDest.LineTo(square.left + square.Width() / 2, square.bottom-2);
		}

		DCDest.SelectObject(pOldPen);
		DCDest.SelectObject(pOldBrush);
	}

	if (ni.GetIDB() > -1)
	{
		if (m_pImageList)
		{
			IMAGEINFO	ii;
			m_pImageList->GetImageInfo(ni.GetIDB(), &ii);
			int iHeight = abs(ii.rcImage.top - ii.rcImage.bottom);
			x += 10 + DEFAULT_COLUMN_OFFSET/2;
			y = rect.top + (GetEditSize(&DCDest, GetFont(), 1, 1).cy - iHeight) / 2;
			m_pImageList->Draw(&DCDest, ni.GetIDB(), CPoint(x, y), ILD_TRANSPARENT);
		}
		else if (m_pImages)
		{
			m_pImages->SetValue(DataInt(ni.GetIDB()));

			int h = (rect.Height() - m_nBmpMaxWidth) / 2;
			y = rect.top;
			if (h > 0)
				y += h;
			//TODO IMAGELIST aggiungere overloading per passare solo il Point
			m_pImages->DrawBitmap(DCDest, CRect(x + 10 + DEFAULT_COLUMN_OFFSET/2, y, x + 10 + DEFAULT_COLUMN_OFFSET/2 + this->m_nBmpMaxWidth, y + this->m_nBmpMaxWidth));
		}
	}
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::OnLButtonDown(UINT nFlags, CPoint point)
{
	int nCol, xColOffs, nRow, nNewCurrRec;
	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRow, nNewCurrRec);

	if ((cpa == IN_BODY) && (nNewCurrRec <= GetDBT()->GetUpperBound()))
	{
		ColumnInfo*	pThisColInfo = m_ColumnsInfo[nCol];

		if (pThisColInfo == m_pTreeEditColInfo)
		{
			SqlRecord*	pRec = GetDBT()->GetRow(nNewCurrRec);
			ASSERT_VALID(pRec);

			CRect rect;
			CalcCellRect(rect, nNewCurrRec, nCol);
			CTreeBodyEditNodeInfo ni(GetDBT()->GetTreeData(pRec).GetString());

			if (PointOnButton(rect, point, ni))
			{
				// tolgo il fuoco ai controls...
				if (!UpdateData(TRUE))
					return;

				if (IsCtrlVisible())
					SetFocusOnly();

				OnToggleExpandNode(nNewCurrRec, ni);

				InvalidateBody();

				GetDBT()->SetReadOnlyFields();
				return;
			}
		}
	}
	CBodyEdit::OnLButtonDown(nFlags, point);
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::PointOnButton(const CRect& rect, const CPoint& pt, const CTreeBodyEditNodeInfo& ni)
{
	// senza figli, nessun pulsante....
	if (!ni.HasChild())
		return FALSE;

	int	x = rect.left + ni.GetLevel() * TBE_TREE_TEXT_INDENTATION;
	CRect	square(0,0, 9,9);
	x -= 10;
	CDC* pDC = GetDC();
	int y = ((GetEditSize(pDC, GetFont(), 1, 1).cy - square.Height()) / 2) + rect.top;
	ReleaseDC(pDC);
	square.OffsetRect(x, y);
	return square.PtInRect(pt);
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::PointOnBitmap(const CRect& rect, const CPoint& pt, int nRow)
{
	SqlRecord* pRec = GetDBT()->GetRow(nRow);
	ASSERT_VALID(pRec);

	CTreeBodyEditNodeInfo ni(GetDBT()->GetTreeData(pRec).GetString());

	return PointOnBitmap(rect, pt, ni); // && (GetTBEMode() != MULTIPLE_SEL);
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::PointOnBitmap(const CRect& rect, const CPoint& pt, const CTreeBodyEditNodeInfo& ni)
{
	//TODO IMAGELIST

	if (m_pImageList == NULL)
		return FALSE;

	IMAGEINFO	ii;
	m_pImageList->GetImageInfo(ni.GetIDB(), &ii);
	int iHeight = abs(ii.rcImage.top - ii.rcImage.bottom);

	CDC* pDC = GetDC();
	int	x = rect.left + ni.GetLevel() * TBE_TREE_TEXT_INDENTATION + DEFAULT_COLUMN_OFFSET/2;
	int y = ((GetEditSize(pDC, GetFont(), 1, 1).cy - iHeight) / 2) + rect.top;
	ReleaseDC(pDC);

	CRect	bitmap(0,0, m_nBmpMaxWidth, ii.rcImage.bottom);

	bitmap.OffsetRect(x, y);
	return bitmap.PtInRect(pt);
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnMoveLeft()
{
	int nCurrIdx = GetDBT()->RemapIndexF2A(m_nCurrRecordIdx);

	BOOL bOk = GetDBT()->MoveLeft(FALSE);

	if (nCurrIdx >= 0 && bOk && nCurrIdx < GetDBT()->GetSize())
		SetCurrRecord(GetDBT()->RemapIndexA2F(nCurrIdx));
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnMoveLeftAll()
{
	int nCurrIdx = GetDBT()->RemapIndexF2A(m_nCurrRecordIdx);

	BOOL bOk = GetDBT()->MoveLeft(TRUE);

	if (nCurrIdx >= 0 && bOk && nCurrIdx < GetDBT()->GetSize())
		SetCurrRecord(GetDBT()->RemapIndexA2F(nCurrIdx));
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnMoveRight()
{
	int nCurrIdx = GetDBT()->RemapIndexF2A(m_nCurrRecordIdx);

	BOOL bOk = GetDBT()->MoveRight(FALSE);

	if (nCurrIdx >= 0 && bOk && nCurrIdx < GetDBT()->GetSize())
	{
		SetCurrRecord(GetDBT()->RemapIndexA2F(nCurrIdx));
	}
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnMoveRightAll()
{
	int nCurrIdx = GetDBT()->RemapIndexF2A(m_nCurrRecordIdx);

	BOOL bOk = GetDBT()->MoveRight(TRUE);

	if (nCurrIdx >= 0 && bOk && nCurrIdx < GetDBT()->GetSize())
	{
		SetCurrRecord(GetDBT()->RemapIndexA2F(nCurrIdx));
	}
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnExpand()
{
	if (GetMultiSelMode() == MULTIPLE_SEL)
		CBodyEdit::ClearSelections();

	GetDBT()->ExpandNode(m_nCurrRecordIdx);

	SetVScrollRange();

	InvalidateBody();
}

//Metodo per fare expand/collapse del singolo nodo passando il numero della riga 
//cui appartiene il nodo
//------------------------------------------------------------------------------
void CTreeBodyEdit::OnToggleExpandNode(int nRow)
{
	if (nRow <= GetDBT()->GetUpperBound())
	{
		SqlRecord* pRec = GetDBT()->GetRow(nRow);
		CTreeBodyEditNodeInfo ni(GetDBT()->GetTreeData(pRec));

		OnToggleExpandNode(nRow, ni);
	}
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnToggleExpandNode(int nRow, CTreeBodyEditNodeInfo& ni)
{
	if (GetMultiSelMode() == MULTIPLE_SEL)
		CBodyEdit::ClearSelections();

	GetDBT()->SetNodeExpand(nRow, ni);

	SetVScrollRange();

	SetCurrLine(nRow, m_nStartRecordIdx);
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnCollapse()
{
	if (GetMultiSelMode() == MULTIPLE_SEL)
		CBodyEdit::ClearSelections();

	GetDBT()->CollapseNode(m_nCurrRecordIdx);

	SetVScrollRange();

	InvalidateBody();
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnExpandAll()
{
	if (GetMultiSelMode() == MULTIPLE_SEL)
		CBodyEdit::ClearSelections();

	GetDBT()->ExpandAll();
	SetVScrollRange();

	//InvalidateBody();
	SetCurrLine(0);
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnCollapseAll()
{
	if (GetMultiSelMode() == MULTIPLE_SEL)
		CBodyEdit::ClearSelections();

	DoMoveToFirstRow(TRUE);
	
	GetDBT()->CollapseAll();

	SetVScrollRange();

	//InvalidateBody();
	SetCurrLine(0);
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnExpandCollapse()
{
	DBTTree* pDBTTree = GetDBT();

	if (!pDBTTree)
		return;

	SqlRecord* pRec = pDBTTree->GetRow(m_nCurrRecordIdx);
	if (!pRec)
		return;

	if (pDBTTree->IsExpanded(pRec))
		OnCollapse();
	else
		OnExpand();
}

//------------------------------------------------------------------------------
BOOL CTreeBodyEdit::IsEnableExpandCollapseNode()
{
	DBTTree* pDBTTree = GetDBT();

	if (!pDBTTree || m_nCurrRecordIdx < 0)
		return FALSE;

	SqlRecord* pRec = pDBTTree->GetRow(m_nCurrRecordIdx);
	if (!pRec)
		return FALSE;

	BOOL bEnabled = FALSE;

	if (pDBTTree->IsExpanded(pRec))
		bEnabled = pDBTTree->CanDoCollapseNode(m_nCurrRecordIdx);
	else
		bEnabled = pDBTTree->CanDoExpandNode(m_nCurrRecordIdx);

	return bEnabled;
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnExpandCollapseAll()
{
	DBTTree* pDBTTree = GetDBT();

	if (!pDBTTree || m_nCurrRecordIdx < 0)
		return;

	SqlRecord* pRec = pDBTTree->GetRow(m_nCurrRecordIdx);
	if (!pRec)
		return;

	if (pDBTTree->IsExpanded(pRec))
		OnCollapseAll();
	else
		OnExpandAll();
}

//------------------------------------------------------------------------------
BOOL CTreeBodyEdit::IsEnableExpandCollapseAll()
{
	return m_nCurrRecordIdx >= 0;
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnShowLevelMenu()
{
	if (IsCtrlVisible())
    	SetFocusOnly();

	m_ContextMenu.CreatePopupMenu();
	m_ContextMenu.SetBitmapBackground(CLR_MAGENTA);

	m_ContextMenu.AppendMenu(MF_STRING | MF_ENABLED, ID_BE_TREE_EXPAND_COLLAPSE_ALL, (LPTSTR)(LPCTSTR)_TB("Expand/Collapse All"));
		
	CRect cr = m_HeaderToolBar.GetToolBarRect();
	ClientToScreen(cr);
	int shift = 0;

	// calcolo a che punto si trova il menu a tendina
	for (int i = 0; i <= m_HeaderToolBar.GetUpperBound(); i++)
	{
		CBEButton* pBD = m_HeaderToolBar.GetAt(i);
		ASSERT_VALID(pBD);

		if (pBD != m_pBtnShowLevel)
			shift += pBD->GetWidth(TRUE);
		else
			break;
	}

	//Devo segnalare al thread incaricato di recuperare le descrizioni delle finestre
	//che sto per entrare in uno stato di "pseudo-modale"(la TrackPopupMenu ferma lésecuzione 
	//del thread)
	AfxGetThreadContext()->RaiseCallBreakEvent();
	
	m_ContextMenu.TrackPopupMenu
						(
							TPM_LEFTALIGN|TPM_RIGHTBUTTON,
							cr.left + shift + 1,
							cr.top + m_HeaderToolBar.GetHeight() + 2,
							this
						);
	
	m_ContextMenu.DestroyMenu();
	m_pBtnShowLevel->SetCheck(FALSE);
}

//------------------------------------------------------------------------------
void CTreeBodyEdit::OnShowSortMenu()
{
	if (IsCtrlVisible())
    	SetFocusOnly();

	m_ContextMenu.CreatePopupMenu();

	m_ContextMenu.AppendMenu	(MF_STRING | MF_ENABLED, ID_BE_TREE_SORTALL,  (LPTSTR) (LPCTSTR)_TB("All"));
	m_ContextMenu.AppendMenu	(MF_STRING | MF_ENABLED, ID_BE_TREE_SORTNODE, (LPTSTR) (LPCTSTR)_TB("Current Level"));

	// disabilitazione comandi
	if (GetDBT()->CanDoSortAll())
		m_ContextMenu.EnableMenuItem(ID_BE_TREE_SORTALL, MF_BYCOMMAND | MF_ENABLED);
	else
		m_ContextMenu.EnableMenuItem(ID_BE_TREE_SORTALL, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

	if (GetDBT()->CanDoSortNode())
		m_ContextMenu.EnableMenuItem(ID_BE_TREE_SORTNODE, MF_BYCOMMAND | MF_ENABLED);
	else
		m_ContextMenu.EnableMenuItem(ID_BE_TREE_SORTNODE, MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

	CRect cr = m_HeaderToolBar.GetToolBarRect();
	ClientToScreen(cr);
	int shift = 0;

	// calcolo a che punto si trova il menu a tendina
	for (int i = 0; i <= m_HeaderToolBar.GetUpperBound(); i++)
	{
		CBEButton* pBD = m_HeaderToolBar.GetAt(i);
		ASSERT_VALID(pBD);
		if (pBD != m_pBtnSort)
			shift += pBD->GetWidth(TRUE);
		else
			break;
	}

	m_ContextMenu.TrackPopupMenu
						(
							TPM_LEFTALIGN|TPM_RIGHTBUTTON,
							cr.left + shift + 1,
							cr.top + m_HeaderToolBar.GetHeight() + 2,
							this
						);

	m_ContextMenu.DestroyMenu();
	m_pBtnSort->SetCheck(FALSE);
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::OnShowCtrl(SqlRecord* pRec, ColumnInfo* pCol, int xPos, int yPos, CRect& rect)
{
	if (pCol != m_pTreeEditColInfo) 
		return FALSE;

	// se c'è effettivamente una struttura ad albero ridimensiono il Ctrl
	// o se ne forzo la visualizzazione
	if(m_bShowAlwaysTreeStruct || GetDBT()->GetMaxLevel() != 1)
	{
		CTreeBodyEditNodeInfo ni(GetDBT()->GetTreeData(pRec));
		int nLevel = ni.GetLevel();

		pCol->GetParsedCtrl()->GetCtrlCWnd()->GetWindowRect(&rect);
		rect.SetRect(xPos, yPos, xPos + rect.Width(), yPos + rect.Height());
		//InvalidateRect(&rect);

		rect.right = rect.left + pCol->GetScreenWidth() + CTRL_NOT_INSIDE;

		rect.left	+= TBE_TREE_TEXT_INDENTATION * nLevel;
		if (m_nBmpMaxWidth)
			rect.left += m_nBmpMaxWidth + DEFAULT_COLUMN_OFFSET;
		
		//rischio che non si veda il control se la colonna è troppo stretta
		if (rect.left > (rect.right-20))
			rect.right += 20;

		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::OnInsertRecord()
{
	if (IsCtrlVisible())
		SetFocusOnly();

	DBTTree* pDBT = GetDBT();

	if (!pDBT || !CanDoInsert())
		goto l_end ;

	if (!InternalCanInsertRow())
		goto l_end;

	int		nIdxNewRec = -1;
	BOOL bOk = GetDBT()->Insert(m_nCurrRecordIdx, nIdxNewRec) != NULL;

	DoMessageOnRecordChanged();

	if (!bOk)
		goto l_end ;

	SetVScrollRange();

	// inform document for changed data
	m_bBodyModified = TRUE;
	GetDocument()->SetModifiedFlag();

	SetCurrRecord(nIdxNewRec);

	ShowCtrl(ResetToVisiblePosition());
l_end:
	OnCommandDone();	
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::OnInsertChildRecord()
{
	if (IsCtrlVisible())
		SetFocusOnly();

	DBTTree* pDBT = GetDBT();

	if (!pDBT || !CanDoInsertChild())
		goto l_end;

	if (!InternalCanInsertRow())
		goto l_end;

	SqlRecord*  pRec = pDBT->GetCurrentRow();
	if (!pRec)
		goto l_end;

	if (m_nMaxLevel)
	{
		CTreeBodyEditNodeInfo ni(pDBT->GetTreeData(pRec));
		if (ni.GetLevel() == m_nMaxLevel)
		{
			pDBT->ConditionalDisplayMessage (_TB("Max tree level has been reached"), MB_OK | MB_ICONSTOP);
			goto l_end;
		}
	}

	if (pRec->IsNeverStorable())
	{
		if (pDBT->OnCheckPrimaryKey(m_nCurrRecordIdx, pRec))
		{
			pDBT->ConditionalDisplayMessage (_TB("Some extra reference data are not complete."), MB_OK | MB_ICONSTOP);
			goto l_end;
		}
		BOOL b = !pDBT->GetTreeEdit(pRec).IsValid() ||
				pDBT->GetTreeEdit(pRec).IsEmpty() ;
		if (
			b
			|| 
			!m_pTreeEditColInfo->GetParsedCtrl()->IsValid()
			)
		{
			pDBT->ConditionalDisplayMessage (_TB("Value is not valid"), MB_OK | MB_ICONSTOP);
			goto l_end;
		}
	}

	int		nIdxNewRec = -1;
	BOOL bOk = pDBT->InsertChild(m_nCurrRecordIdx, nIdxNewRec) != NULL;

	DoMessageOnRecordChanged();

	if (!bOk)
		goto l_end;

	SetVScrollRange();

	// inform document for changed data
	m_bBodyModified = TRUE;
	GetDocument()->SetModifiedFlag();

	SetCurrRecord(nIdxNewRec);

	ShowCtrl(ResetToVisiblePosition());
l_end:
	OnCommandDone();	
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::InternalCanDeleteRow()
{
	if (!__super::InternalCanDeleteRow())
		return FALSE;

	DBTTree* pDBT = GetDBT();
		
	SqlRecord*  pRec = pDBT->GetCurrentRow();
	CTreeBodyEditNodeInfo ni(pDBT->GetTreeData(pRec));
	if (ni.HasChild())
	{
		if (AfxMessageBox(_TB("Warning! It will be deleted this record and all its children, do you confirm?"), MB_YESNO) != IDYES)
			return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::OnSortAll()
{
	DBTTree* pDBT = GetDBT();
	if (!pDBT || !pDBT->CanDoSortAll())
		return;

	int	nDummyIdx = -1;
	int nDummyCurrRecordIdx = -1;
	pDBT->Sort(nDummyCurrRecordIdx, nDummyIdx);
	pDBT->BuildTree();

//	DoMessageOnRecordChanged();
	SetVScrollRange();

	// inform document for changed data
	m_bBodyModified = TRUE;
	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
void CTreeBodyEdit::OnSortNode()
{
	DBTTree* pDBT = GetDBT();
	if (!pDBT || !pDBT->CanDoSortNode())
		return;

	int	nDummyIdx = -1;
	pDBT->Sort(m_nCurrRecordIdx, nDummyIdx);
	pDBT->BuildTree();

	SetVScrollRange();

	// inform document for changed data
	m_bBodyModified = TRUE;
	GetDocument()->SetModifiedFlag();
}

//-----------------------------------------------------------------------------
BOOL CTreeBodyEdit::DoMovingKey(UINT nChar)
{
	BOOL	shiftDown	= (m_wKeyState & KEY_SHIFT_DOWN) == KEY_SHIFT_DOWN;

	DBTTree*	pDBT = GetDBT();
	SqlRecord*  pRec = pDBT->GetCurrentRow();
	if (!pRec)
		return CBodyEdit::DoMovingKey(nChar);

	CTreeBodyEditNodeInfo ni(pDBT->GetTreeData(pRec));

	if (shiftDown)
	{
		switch (nChar)
		{
			case VK_LEFT:

				if (ni.HasChild() && ni.IsExpanded())
				{
					ni.SetExpanded(FALSE);
					pDBT->GetTreeData(pRec) = ni.GetInfo();
					GetDocument()->SetModifiedFlag();
				}
				else
				{
					int nIdx = ni.GetParent() - 1;
					if (nIdx >= 0 && nIdx <= pDBT->GetUnfilteredUpperBound())
					{
						SetCurrRecord(pDBT->RemapIndexA2F(nIdx));
						ShowCtrl(ResetToVisiblePosition());
					}
				}
				
				pDBT->BuildTree();

				SetVScrollRange();
				InvalidateBody();
				return TRUE;

			case VK_RIGHT:

				if (ni.HasChild() && !ni.IsExpanded())
				{
					ni.SetExpanded(TRUE);
					pDBT->GetTreeData(pRec) = ni.GetInfo();
					GetDocument()->SetModifiedFlag();
				}
				else
				{
					DoMoveToNextRow(TRUE);
					return TRUE;
				}
				
				pDBT->BuildTree();

				SetVScrollRange();
				InvalidateBody();
				return TRUE;
		}
	}
	return CBodyEdit::DoMovingKey(nChar);
}

//-----------------------------------------------------------------------------
void	CTreeBodyEdit::OnDoSearch()
{
	OnExpandAll();
	__super::OnDoSearch();
}
