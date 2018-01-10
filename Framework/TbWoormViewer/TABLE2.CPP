
#include "stdafx.h"     

#include <TbGeneric\minmax.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\LocalizableObjs.h>

#include <TbGenlib\reswalk.h>
#include <TbGeneric\TBThemeManager.h>


#include <TbWoormEngine\edtmng.h>

#include "export.h"           
#include "woormdoc.h"
#include "woormini.h"
#include "woormfrm.h"
#include "column.h"
#include "table.h"
#include "woormvw.h"
#include "ExpExter.h"
#include "mulselob.h"
//resources
#include "woormdoc.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "beginh.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


// column title sensibility for Resize as ratio
const int COL_SENS = 3;

// static const data member initializazion
const int   Table::NO_ACTIVE_COLUMN = -1;
const int   Table::ACTIVE_IS_TITLE  = -1;
const int   Table::ACTIVE_IS_TOTAL  = -2;
  
const int   Table::CELL_HEIGHT          = DEFAULT_TABLE_CELL_HEIGHT;
const int   Table::CELL_WIDTH           = DEFAULT_TABLE_CELL_WIDTH;
const int   Table::COLUMN_TITLE_HEIGHT  = DEFAULT_TABLE_COLUMN_TITLE_HEIGHT;
const int   Table::TITLE_HEIGHT         = DEFAULT_TABLE_TITLE_HEIGHT;

//-----------------------------------------------------------------------------
// used when user ask for create them
Table::Table
(
			CPoint      ptCurrPos,
			CWoormDocMng*  pDocument,
			int         nRows,
			int         nCols,
			WORD        wTableId,
			CWordArray& wColumnIds
)
	:
	BaseObj             (ptCurrPos, pDocument),
	
	m_bHideTableTitle	(FALSE),
	m_bHideColumnsTitle	(FALSE),
	m_bAlternateBkgColorOnRow(FALSE),
	m_crAlternateBkgColorOnRow	(AfxGetThemeManager()->GetAlternateColor()),
	m_nDropShadowHeight (0),
	m_crDropShadowColor (RGB(0,0,0)),
	m_bFiscalEnd		(FALSE),
	m_nLastTitleHeight	(MINCELL_Y * 2 + 10),
	m_nActiveColumn     (NO_ACTIVE_COLUMN),
	m_nActiveRow        (ACTIVE_IS_TITLE),
	m_TitlePen          (RGB(0,0,0)),
	m_nCurrentRow       (0),
	m_nTotalCounter     (0),
	m_nRowHeightZoom	(0),
	m_bTemplate			(FALSE),
	m_pDefault			(NULL),
	m_pHideExpr			(NULL),
	m_bTemplateOverridden	(FALSE),
	m_bAlternateEasyview	(TRUE),
	m_bAlternateBkgColorOnMultiLineRow (FALSE),
	m_nViewCurrentRow		(-1)
{
	m_wInternalID       = wTableId;

	// color pen for drawing title of table
	m_Title.SetTextColor	(DEFAULT_TABLE_TITLE_FOREGROUND);
	m_Title.SetBkgColor		(DEFAULT_TABLE_TITLE_BACKGROUND);
	m_Title.SetFontIdx		(pDocument->m_pFontStyles->GetFontIdx(FNT_TABLE_TITLE));

	// default value for cells and title
	CSize   csDefaultCell          (CELL_WIDTH, CELL_HEIGHT);
	CSize   csDefaultColumnTitle   (csDefaultCell.cx, COLUMN_TITLE_HEIGHT);

	// starting point and width of all table
	CPoint  ptOrigin  (ptCurrPos.x, ptCurrPos.y + TITLE_HEIGHT);
	int     width   = 0;

	// create columns
	for (int i =0; i < nCols; i++)
	{
		TableColumn* pColumn = new TableColumn
			(
				ptOrigin,
				csDefaultColumnTitle,
				csDefaultCell,
				this,
				nRows,
				wColumnIds[i]
			);
		m_Columns.Add (pColumn);

		// update origin and global width
		ptOrigin.x += pColumn->GetColumnRect().Width();
		width += pColumn->GetColumnRect().Width();
	}

	// create m_Interlines flags
	for (int nRow = 1; nRow <= nRows; nRow++) 
	{
		m_Interlines.Add(FALSE);
		m_LineWithTitles.Add(FALSE);
		m_LineWithCustomTitles.Add(L"");
	}

	// adjust rows number counter in editor manager if create from user.
	//
	m_pDocument->m_pEditorManager->SetTableRows (wTableId, nRows);
	m_Title.SetText(m_pDocument->m_pEditorManager->GetTablePublicName(wTableId));

	// adjust rect values (column dimension are already ok). Can use position 0
	// because all column Has one total too also if it is not showed
	//
	m_TitleRect       = CRect(ptCurrPos, CSize(width, TITLE_HEIGHT));
	m_BaseCellsRect   = CRect(ptCurrPos, CSize(width, TITLE_HEIGHT + ColumnCellsRect(0).Height()));
	m_BaseRect        = CRect(ptCurrPos, CSize(width, TITLE_HEIGHT + ColumnRect(0).Height()));
}

// used if create from parsing
//---------------------------------------------------------------------------
Table::Table
(
			CPoint      ptCurrPos,
			CWoormDocMng*  pDocument,
			int         nRows,
			int         nCols
)
	:
	BaseObj             (ptCurrPos, pDocument),

	m_bHideTableTitle	(FALSE),
	m_bHideColumnsTitle	(FALSE),
	m_bAlternateBkgColorOnRow(FALSE),
	m_crAlternateBkgColorOnRow	(AfxGetThemeManager()->GetAlternateColor()),
	m_nDropShadowHeight (0),
	m_crDropShadowColor (RGB(0,0,0)),
	m_bFiscalEnd		(FALSE),
	m_nLastTitleHeight	(MINCELL_Y * 2 + 10),
	m_nActiveColumn     (NO_ACTIVE_COLUMN),
	m_nActiveRow        (ACTIVE_IS_TITLE),
	m_TitlePen          (RGB(0,0,0)),
	m_nCurrentRow       (0),
	m_nTotalCounter     (0),
	m_nRowHeightZoom	(0),
	m_bTemplate			(FALSE),
	m_pDefault			(NULL),
	m_pHideExpr			(NULL),
	m_bTemplateOverridden	(FALSE),
	m_bAlternateEasyview	(TRUE),
	m_bAlternateBkgColorOnMultiLineRow (FALSE),
	m_nViewCurrentRow		(-1)
{
	//m_wInternalID       = 0;

	// color pen for drawing title of table
	m_Title.SetTextColor(DEFAULT_TABLE_TITLE_FOREGROUND);
	m_Title.SetBkgColor	(DEFAULT_TABLE_TITLE_BACKGROUND);
	m_Title.SetFontIdx	(pDocument->m_pFontStyles->GetFontIdx(FNT_TABLE_TITLE));

	// default value for cells and title
	CSize   csDefaultCell			(CELL_WIDTH, CELL_HEIGHT);
	CSize   csDefaultColumnTitle	(csDefaultCell.cx, COLUMN_TITLE_HEIGHT);

	// starting point and width of all table
	CPoint  ptOrigin  (ptCurrPos.x, ptCurrPos.y + TITLE_HEIGHT);
	int     width   = 0;

	// create columns
	for (int i = 1; i <= nCols; i++)
	{
		TableColumn* pColumn = new TableColumn
			(
				ptOrigin,
				csDefaultColumnTitle,
				csDefaultCell,
				this,
				nRows
			);
		m_Columns.Add (pColumn);

		// update origin and global width
		ptOrigin.x += pColumn->GetColumnRect().Width();
		width += pColumn->GetColumnRect().Width();
	}

	// create m_Interlines flags
	for (int nRow = 1; nRow <= nRows; nRow++)
	{ 
		m_Interlines.Add(FALSE);
		m_LineWithTitles.Add(FALSE);
		m_LineWithCustomTitles.Add(L"");
	}

	// adjust rect values (column dimension are already ok). Can use position 0
	// because all column Has one total too also if it is not showed
	//
	m_TitleRect       = CRect(ptCurrPos, CSize(width, TITLE_HEIGHT));
	m_BaseCellsRect   = CRect(ptCurrPos, CSize(width, TITLE_HEIGHT + ColumnCellsRect(0).Height()));
	m_BaseRect        = CRect(ptCurrPos, CSize(width, TITLE_HEIGHT + ColumnRect(0).Height()));
}

//---------------------------------------------------------------------------
Table::Table (const Table& source)
	:
	BaseObj					(source),

	m_BaseCellsRect			(source.m_BaseCellsRect),
	m_TitleRect				(source.m_TitleRect),
	m_Borders				(source.m_Borders),
	m_bHideTableTitle		(source.m_bHideTableTitle),
	m_bHideColumnsTitle		(source.m_bHideColumnsTitle),
	m_bAlternateBkgColorOnRow	(source.m_bAlternateBkgColorOnRow),
	m_crAlternateBkgColorOnRow		(source.m_crAlternateBkgColorOnRow),
	m_nDropShadowHeight		(source.m_nDropShadowHeight),
	m_crDropShadowColor		(source.m_crDropShadowColor),
	m_bFiscalEnd			(source.m_bFiscalEnd),
	m_TitlePen				(source.m_TitlePen),
	m_Title					(source.m_Title),
	m_nLastTitleHeight		(source.m_nLastTitleHeight),
	m_nActiveColumn			(source.m_nActiveColumn),
	m_nActiveRow			(source.m_nActiveRow),
	m_nCurrentRow			(source.m_nCurrentRow),
	m_ptCellOrigin			(source.m_ptCellOrigin),
	m_nTotalCounter			(source.m_nTotalCounter),
	m_nAction				(source.m_nAction),
	m_sStyleClass			(source.m_sStyleClass),
	m_bTemplate				(source.m_bTemplate),
	m_pDefault				(source.m_pDefault),
	m_pHideExpr				(source.m_pHideExpr	? new Expression(*source.m_pHideExpr) : NULL),
	m_bTemplateOverridden	(source.m_bTemplateOverridden),
	m_bAlternateEasyview	(source.m_bAlternateEasyview),
	m_bAlternateBkgColorOnMultiLineRow	(source.m_bAlternateBkgColorOnMultiLineRow),
	m_nViewCurrentRow				(source.m_nViewCurrentRow)
{
	m_wInternalID			= source.m_wInternalID;

	for (int i = 0; i < source.m_Columns.GetSize(); i++)
	{
		TableColumn* pSrcColumn = source.m_Columns[i];
		TableColumn* pColumn = new TableColumn (*pSrcColumn);
		pColumn->m_pTable = this;	//CHANGE OWNER TABLE
		m_Columns.Add (pColumn);
	}

	m_Interlines.Copy(source.m_Interlines);
	m_LineWithTitles.Copy(source.m_LineWithTitles);
	m_LineWithCustomTitles.Copy(source.m_LineWithCustomTitles);

	m_arAlternateEasyviewOnPage.Copy(source.m_arAlternateEasyviewOnPage);
}

//---------------------------------------------------------------------------
Table::~Table()
{
	int ub = m_Columns.GetUpperBound();
	for (int i = ub; i >= 0; i--)
	{
		delete m_Columns[i];
		m_Columns[i] = NULL;
	}
	m_Columns.RemoveAll();

	SAFE_DELETE (m_pHideExpr);
}

//---------------------------------------------------------------------------
CString Table::GetDescription (BOOL /*= TRUE*/) const 
{ 
	CString sName (this->GetTableTitleText());

	if (m_pDocument && m_pDocument->m_pEditorManager)
	{
			DisplayTables* pTbls = m_pDocument->m_pEditorManager->GetDispTable();
			if (pTbls)
			{
				sName = pTbls->GetName(GetInternalID());
			}
	}

	//CString s = cwsprintf(_T("%s (Id:%d)"), sName, GetInternalID());
	sName.Replace(_T("\\n"), _T(" "));
	return sName;
}

//---------------------------------------------------------------------------
void Table::VExpandRow (int nRow, int height)
{
	int nGlobalHeight = height * (LastRow() + 1);

	m_BaseCellsRect.bottom    += nGlobalHeight;
	m_BaseRect.bottom         += nGlobalHeight;

	for (int nCol = 0; nCol <= LastColumn(); nCol++)
		m_Columns[nCol]->VExpandRow(nRow, height);
}

//---------------------------------------------------------------------------
void Table::VMoveTotals (int height)
{
	for (int nCol = 0; nCol <= LastColumn(); nCol++)
		m_Columns[nCol]->VMoveTotal (height);
}

//---------------------------------------------------------------------------
void Table::HExpandColumn (int nCol, int width, BOOL bUpdateDefaultWidth)
{
	m_TitleRect.right     += width;
	m_BaseRect.right      += width;
	m_BaseCellsRect.right += width;

	m_Columns[nCol]->HExpandColumn (width);

	if (bUpdateDefaultWidth)
		m_Columns[nCol]->m_nWidth = m_Columns[nCol]->Width();

	for (int c = nCol + 1; c <= LastColumn(); c++)
		m_Columns[c]->HMoveColumn(width);
}

//---------------------------------------------------------------------------
void Table::RightShiftColumn (int nCol, int width)
{
	// adjust table title and global size and shift
	m_TitleRect.right     += width;
	m_BaseCellsRect.right += width;
	m_BaseRect.right      += width;

	for (int c = nCol + 1; c <= LastColumn(); c++)
		m_Columns[c]->HMoveColumn(width);
}

//---------------------------------------------------------------------------
void Table::LeftShiftColumn (int nCol, int width)
{
	m_TitleRect.right     -= width;
	m_BaseRect.right      -= width;
	m_BaseCellsRect.right -= width;

	for (int c = nCol; c <= LastColumn(); c++)
		m_Columns[c]->HMoveColumn(-width);
}

//---------------------------------------------------------------------------
void Table::VExpandColumnTitle (int height)
{
	m_TitleRect.OffsetRect (0, height);
	m_BaseRect.top        += height;
	m_BaseCellsRect.top   += height;

	for (int nCol = 0; nCol <= LastColumn(); nCol++)
		m_Columns[nCol]->VExpandTitle(height);
}

//---------------------------------------------------------------------------
void Table::VExpandTotal (int height)
{
	m_BaseRect.bottom += height;

	for (int nCol = 0; nCol <= LastColumn(); nCol++)
		m_Columns[nCol]->VExpandTotal(height);
}

//---------------------------------------------------------------------------
void Table::VExpandTitle (int height)
{
	m_TitleRect.top       += height;
	m_BaseRect.top        += height;
	m_BaseCellsRect.top   += height;
}

//---------------------------------------------------------------------------
void Table::MoveTable (CSize csDelta)
{
	m_TitleRect.      OffsetRect(csDelta);
	m_BaseRect.       OffsetRect(csDelta);
	m_BaseCellsRect.  OffsetRect(csDelta);

	for (int nCol = 0; nCol <= LastColumn(); nCol++)
		m_Columns[nCol]->MoveColumn(csDelta);
}

//---------------------------------------------------------------------------
int Table::NextVisibleColumn(int nCol) const
{
	for (int i = nCol + 1; i <= LastColumn(); i++)
		if (!m_Columns[i]->IsHidden())
			return i;

	return -1;
}

// se richiesto da utente allora posso disabilitare i titoli delle colonne
//---------------------------------------------------------------------------
int Table::LastVisibleColumn() const
{
	for (int nCol = LastColumn(); nCol >= 0; nCol--)
	{
		if (!m_Columns[nCol]->IsHidden())
			return nCol;
	}

	// di qui non devo uscire perchè non possi mettere Hidden tutte le colonne
	// almeno una deve essere visibile
	ASSERT(FALSE);
	return 0;
}

//---------------------------------------------------------------------------
int Table::FirstVisibleColumn() const
{
	for (int nCol = 0; nCol >= LastColumn(); nCol++)
	{
		if (!m_Columns[nCol]->IsHidden())
			return nCol;
	}

	// di qui non devo uscire perchè non possi mettere Hidden tutte le colonne
	// almeno una deve essere visibile
	ASSERT(FALSE);
	return 0;
}

//---------------------------------------------------------------------------
int Table::HiddenColumnNumber() const
{
	int nCount = 0;
	for (int nCol = LastColumn(); nCol >= 0; nCol--)
	{
		if (m_Columns[nCol]->IsHidden())
			nCount++;
	}
	return nCount;
}

//---------------------------------------------------------------------------
int Table::VisibleColumnNumber() const
{
	int nCount = 0;
	for (int nCol = LastColumn(); nCol >= 0; nCol--)
	{
		if (!m_Columns[nCol]->IsHidden())
			nCount++;
	}
	return nCount;
}

//---------------------------------------------------------------------------
BOOL Table::ExistsVisibleColumn() const
{
	for (int nCol = 0; nCol <= LastColumn(); nCol++)
	{
		if (!m_Columns[nCol]->IsHidden())
			return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL Table::UseColorEasyview (/*BOOL bOdd, */int nRow/*, BOOL bIsTail*/)
{
	if (!m_bAlternateBkgColorOnRow && !m_bAlternateBkgColorOnMultiLineRow)
		return FALSE;

	if (m_bAlternateBkgColorOnMultiLineRow)
		return m_bAlternateEasyview;

	return nRow % 2;	//!bOdd;	//regular alternate style
}

//---------------------------------------------------------------------------
BOOL Table::ExistsColumnWithDynamicAttributeOnRow () const
{
	int nLastColumn = LastVisibleColumn();
	for (int nCol = 0; nCol <= nLastColumn; nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];
		if (pColumn->IsHidden()) continue;

		if (pColumn->HasDynamicAttributeOnRow() || pColumn->m_bVMergeEqualCell)
			return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL Table::ExistsCellTail (int nRow)
{
	int nLastColumn = LastVisibleColumn();
	for (int nCol = 0; nCol <= nLastColumn; nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];
		if (pColumn->IsHidden()) continue;

		TableCell* pCell = pColumn->m_Cells[nRow];
		if (pCell->m_Value.m_RDEdata.IsValid() && pCell->m_Value.m_RDEdata.IsTailMultiLineString())
			return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL Table::ExistsCellSubTotal (int nRow)
{
	int nLastColumn = LastVisibleColumn();
	for (int nCol = 0; nCol <= nLastColumn; nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];
		if (pColumn->IsHidden()) continue;

		TableCell* pCell = pColumn->m_Cells[nRow];
		if (pCell->m_Value.m_RDEdata.IsValid() && pCell->m_Value.m_RDEdata.IsSubTotal())
			return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL Table::IsEmptyRow(int nRow)
{
	int nLastColumn = LastVisibleColumn();
	for (int nCol = 0; nCol <= nLastColumn; nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];
		if (pColumn->IsHidden()) continue;

		TableCell* pCell = pColumn->m_Cells[nRow];
		if (pCell->m_Value.m_RDEdata.IsValid())
			return FALSE;
	}
	return TRUE;
}
//---------------------------------------------------------------------------
void Table::CheckRowValue (int nRow, BOOL& bTail, BOOL& bSubTotal, BOOL& bIsEmpty)
{
	bIsEmpty = TRUE; bTail = FALSE; bSubTotal = FALSE;
	int nLastColumn = LastVisibleColumn();
	for (int nCol = 0; nCol <= nLastColumn; nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];
		if (pColumn->IsHidden()) continue;

		TableCell* pCell = pColumn->m_Cells[nRow];

		if (!pCell->m_Value.m_RDEdata.IsValid())
			continue;
		bIsEmpty = FALSE;
		if (pCell->m_Value.m_RDEdata.IsSubTotal())
			bSubTotal = TRUE;
		if (pCell->m_Value.m_RDEdata.IsTailMultiLineString())
			bTail = TRUE;
	}
}

//---------------------------------------------------------------------------
BOOL Table::isMarkExportable()
{
	return m_pDocument->m_pExportData && m_pDocument->m_pExportData->IncludeTable(m_wInternalID);
}

//---------------------------------------------------------------------------
BOOL Table::CheckIsHidden()
{
	DataBool bHidden;
	if (m_pHideExpr && m_pHideExpr->Eval(bHidden) && bHidden)
	{
		if (!m_pDocument->m_bAllowEditing)
			return TRUE;
	}
	if (m_pDocument->m_bAllowEditing &&/* m_pDocument->m_nCurrentLayer &&*/ m_nLayer != m_pDocument->m_nCurrentLayer)
		return TRUE;

	return FALSE;
}
//---------------------------------------------------------------------------

INT Table::GetColumnTitleHeight()
{
	return m_Columns[0]->m_ColumnTitleRect.Height();
}

//---------------------------------------------------------------------------
void Table::DrawTitles(CDC& DC, CRect invalid, BOOL /*bPreview*/)
{
	if (m_bHideColumnsTitle)
		return;

	BOOL bFirst = TRUE;
	int nLastColumn = LastVisibleColumn();
	for (int nCol = 0; nCol <= nLastColumn; nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];
		BOOL bLast = nCol == nLastColumn;

		if (!pColumn->IsHidden())
		{
			pColumn->DrawColumnTitle(DC, invalid, bFirst, bLast, pColumn->m_ColumnTitleRect);
			bFirst = FALSE;
		}
	}
}

//---------------------------------------------------------------------------
void Table::DrawTotals(CDC& DC, CRect invalid, BOOL /*bPreview*/)
{
	BOOL bFirst = TRUE;
	int nLastColumn = LastVisibleColumn();
	for (int nCol = 0; nCol <= nLastColumn; nCol++)
	{
		TableColumn* pColumn = m_Columns[nCol];

		if (pColumn->IsHidden())
			continue;

		//if (pColumn->m_bShowTotal)
		{
			int nNextVisibleColumn = NextVisibleColumn(nCol);
			BOOL bNextColumnHasTotal =
				(
				(nCol < nLastColumn) &&
					(nNextVisibleColumn >= 0) &&
					HasTotal(nNextVisibleColumn)
					);

			pColumn->DrawTotal
			(
				DC, invalid,
				bFirst,
				bNextColumnHasTotal,
				nNextVisibleColumn
			);
			bFirst = FALSE;
		}
		//else
		//	bFirst = TRUE;
	}
}

//---------------------------------------------------------------------------
void Table::DrawRows (CDC& DC, CRect invalid, BOOL bPreview, BOOL bMarkExportable)
{
	int nLastColumn = LastVisibleColumn();
	BOOL bCurrRowHasTail = FALSE;
	BOOL bNextRowHasTail = FALSE;

	if (m_bAlternateBkgColorOnMultiLineRow)
	{
		if (m_pDocument->m_pRDEmanager)
		{
			int page = m_pDocument->m_pRDEmanager->CurrPageRead();

			if (page < 0)
				m_bAlternateEasyview = TRUE;
			else if (page == m_arAlternateEasyviewOnPage.GetSize())
				m_arAlternateEasyviewOnPage.Add(m_bAlternateEasyview);
			else if (page < m_arAlternateEasyviewOnPage.GetSize())
				m_bAlternateEasyview = m_arAlternateEasyviewOnPage[page];
		}
	}
	BOOL bRowSep = m_Columns[0]->GetBorders().m_bRowSeparator || m_Columns[0]->GetBorders().m_bRowSeparatorDynamic;

	BOOL bThereIsDynamicAttr = ExistsColumnWithDynamicAttributeOnRow ();
	BOOL bMergeRow = m_bAlternateBkgColorOnMultiLineRow || m_Columns[0]->GetBorders().m_bRowSeparatorDynamic;
	BOOL bSearchTail = bMergeRow || bThereIsDynamicAttr;
	if (bSearchTail)
	{
		bNextRowHasTail = ExistsCellTail(0);
	}

	for (int nRow = 0; nRow <= LastRow(); nRow++)
	{
		if (bSearchTail)
		{
			bCurrRowHasTail = bNextRowHasTail;
			bNextRowHasTail = nRow < LastRow() ? ExistsCellTail(nRow + 1) : FALSE;

			if (bMergeRow && m_bAlternateBkgColorOnMultiLineRow && !bCurrRowHasTail)
				m_bAlternateEasyview = ! m_bAlternateEasyview;
		}

		BOOL bFirstCol = TRUE;
		for (int nCol = 0; nCol <= nLastColumn; nCol++)
		{
			TableColumn* pColumn = m_Columns[nCol];
			TableCell* pCell = pColumn->m_Cells[nRow];

			if (nCol == 0)
			{
				CString sCustomTitle = m_LineWithCustomTitles[nRow];
				if (!sCustomTitle.IsEmpty())
				{
					// ---- save and replace column style attributes and text value
					CString sSaveTitle = pColumn->m_Title.GetText(); pColumn->m_Title.SetText(sCustomTitle);
					Expression* pSaveTitleExpr = pColumn->m_pTitleExpr; pColumn->m_pTitleExpr = NULL;
					BasicTextStyle saveStyle (pColumn->m_Title);
					pColumn->m_Title.Assign(m_SubTitle);
					//-------------------------------------
	
					CRect rect(pCell->GetCellRect());
					rect.left = this->m_BaseRect.left;
					rect.right = this->m_BaseRect.right;

					pColumn->DrawColumnTitle(DC, rect, TRUE, TRUE, rect, nRow);

					//---- restore column title attributes------------------------
					pColumn->m_Title.SetText(sSaveTitle); 
					pColumn->m_pTitleExpr = pSaveTitleExpr;
					pColumn->m_Title.Assign(saveStyle);
					break;
				}
			}

			if (!pColumn->IsHidden())
			{
				if (nRow == 0)
					pColumn->m_PreviousValue.m_RDEdata.ResetValid();

				if (m_LineWithTitles[nRow])
				{
					pColumn->DrawColumnTitle(DC, invalid, bFirstCol, (nCol == nLastColumn), pCell->GetCellRect(), nRow);
				}
				else
				{
					pColumn->DrawCell
					(
						DC,
						invalid,
						nRow,
						bFirstCol,
						(nCol == nLastColumn),
						bPreview,
						bMarkExportable && m_pDocument->m_pExportData->IncludeColumn(nCol),
						bMarkExportable && m_pDocument->m_pExportData->IsTitlesColumn(nCol),
						pColumn->GetBorders().m_bRowSeparatorDynamic && bNextRowHasTail ? FALSE : bRowSep,
						bCurrRowHasTail
					);
				}

				bFirstCol = FALSE;

				if (pColumn->m_bVMergeEqualCell)
				{
					TableCell* pCell = pColumn->m_Cells[nRow];
					if (pCell->m_Value.m_RDEdata.IsValid() && !pCell->m_Value.m_RDEdata.IsTailMultiLineString())
					{
						if (pCell->m_Value.GetText().IsEmpty())
							pColumn->m_PreviousValue.m_RDEdata.ResetValid();
						else
							pColumn->m_PreviousValue = pCell->m_Value;
					}
				}
			}
		}
	}
}

//---------------------------------------------------------------------------
// non tutta la tabella e' piena allora scrivo una zeta
void Table::DrawFiscalEnd (CDC& DC, CRect invalid, BOOL /*bPreview*/)
{
	if (m_bFiscalEnd && !NoBorders(DC.IsPrinting()) && (m_nCurrentRow <= LastRow()))
	{
		CRect firstRow = RowRect(m_nCurrentRow);
		CRect lastRow = RowRect(LastRow());
		CRect rectFiscal(firstRow.left, firstRow.top, lastRow.right, lastRow.bottom);
		CRect rect;

		rectFiscal.InflateRect(-3, -3); //per tener conto degli angoli della penna
		if (DC.IsPrinting() || rect.IntersectRect(rectFiscal, invalid))
		{
			ScaleRect(rectFiscal, DC);

			CPen pen(PS_SOLID, 2, DEFAULT_TEXTCOLOR);
			CPen* pOldPen = DC.SelectObject(&pen);

			DC.MoveTo(rectFiscal.right,	rectFiscal.top);
			DC.LineTo(rectFiscal.left,	rectFiscal.top);
			DC.LineTo(rectFiscal.right,	rectFiscal.bottom);
			DC.LineTo(rectFiscal.left,	rectFiscal.bottom);

			DC.SelectObject(pOldPen);
		}
	}
}

//---------------------------------------------------------------------------
void Table::DrawColumns(CDC& DC, CRect invalid, BOOL bPreview)
{
	DrawTitles		(DC, invalid, bPreview);
	DrawRows		(DC, invalid, bPreview, isMarkExportable());
	DrawTotals		(DC, invalid, bPreview);
	DrawFiscalEnd	(DC, invalid, bPreview);
}

//------------------------------------------------------------------------------
void Table::Draw (CDC& DC, CRect invalid, BOOL bPreview)
{
	if (CheckIsHidden())
		return;

	CRect rect (m_BaseCellsRect);

	//if (m_bHideTableTitle)
	//{
	//	rect.top -= m_TitleRect.Height();
	//}
	//if (m_bHideColumnsTitle)
	//{
	//	CRect& r = m_Columns[0]->m_ColumnTitleRect;
	//	rect.top -= r.Height();
	//}

	__super::DrawDropShadow(DC, rect, m_nDropShadowHeight, m_crDropShadowColor);

	Borders title_borders (FALSE);
	rect = m_TitleRect;

	// don't Draw the bottom line for sourronding rectangle
	title_borders.top		= m_Borders.m_bTableTitleTop;
	title_borders.bottom	= m_Borders.m_bTableTitleBottom;
	title_borders.left		= m_Borders.m_bTableTitleLeft;
	title_borders.right		= m_Borders.m_bTableTitleRight;

	// invalid is already in device ptPoint
	if (DC.IsPrinting() || rect.IntersectRect(rect, invalid))
	{
		if (ShowTitles(DC.IsPrinting()))
		{
			DrawBorders (DC, m_TitleRect, m_TitlePen, &title_borders, NoBorders(DC.IsPrinting()));

			CRect inside = InsideRect
				(          
					DC,
					m_TitleRect,
					m_TitlePen,
					title_borders,
					NoBorders(DC.IsPrinting())
				);

			m_Title.Draw(DC, inside, m_pDocument, !m_bTransparent);
		}
	}
	DrawColumns (DC, invalid, bPreview);
}

//---------------------------------------------------------------------------
void Table::Redraw()
{
	Redraw(TRUE);
}

//------------------------------------------------------------------------------
void Table::Draw(CDC& DC, BOOL bPreview)
{
	Draw(DC, m_BaseRect, bPreview);
}

//---------------------------------------------------------------------------
CRect Table::GetActiveRect () const
{
	/*if (m_nActiveColumn    == NO_ACTIVE_COLUMN)    return m_TitleRect;
	if (m_nActiveRow       == ACTIVE_IS_TITLE)     return ColumnTitleRect(m_nActiveColumn);
	if (m_nActiveRow       == ACTIVE_IS_TOTAL)     return TotalRect(m_nActiveColumn);*/
	if (m_nActiveColumn == NO_ACTIVE_COLUMN)
	{
		CRect tableRect(m_BaseRect);
		if (m_bHideColumnsTitle)
		{
			if (m_Columns.GetSize() > 0)
				tableRect.top += ColumnTitleRect(0).Height();
			tableRect.top += TitleRect().Height();
		}
		else if (m_bHideTableTitle)
			tableRect.top += TitleRect().Height();
		if (!HasTotal() && m_Columns.GetSize() > 0)
			tableRect.bottom -= TotalRect(0).Height();
		return tableRect;
	}

	if (m_nActiveRow == ACTIVE_IS_TITLE)
	{
		/*CRect columnRect(ColumnRect(m_nActiveColumn));
		if (m_bHideColumnsTitle)
			columnRect.top += ColumnTitleRect(0).Height();
		if (!GetColumn(m_nActiveColumn)->HasTotal())
			columnRect.bottom -= TotalRect(m_nActiveColumn).Height();
		return columnRect;*/
		return ColumnRect(m_nActiveColumn);
	}

	if (m_nActiveRow == ACTIVE_IS_TOTAL && m_nActiveColumn > -1)
	{
		return TotalRect(m_nActiveColumn);
	}

	return CellRect(m_nActiveRow, m_nActiveColumn);
}

//---------------------------------------------------------------------------
Table::ActionType Table::TitleAction (const CRect& rect, CPoint ptMousePos)
{
	if (TitleActionRect(ADD_ROW).PtInRect(ptMousePos)) return ADD_ROW;
	if (!rect.PtInRect(ptMousePos)) return LEAVE;

	if (TitleActionRect(T_VSIZE).PtInRect(ptMousePos)) return T_VSIZE;
	return MOVE;
}

//---------------------------------------------------------------------------
Table::ActionType Table::ColumnAction (const CRect& rect, CPoint ptMousePos, int nCol)
{
	if (!rect.PtInRect(ptMousePos))
		return LEAVE;

	if (ColumnActionRect(CT_VSIZE,nCol).PtInRect(ptMousePos))
		return CT_VSIZE;

	if (TotalActionRect(TOT_VSIZE,nCol).PtInRect(ptMousePos))
		return HasTotal(nCol) ? TOT_VSIZE : LEAVE;

	return MOVE;
}

//---------------------------------------------------------------------------
Table::ActionType Table::CellAction (const CRect& rect, CPoint ptMousePos, int nRow, int nCol)
{
	if (!rect.PtInRect(ptMousePos)) return LEAVE;

	if (CellActionRect(CELL_HSIZE,  nRow, nCol).PtInRect(ptMousePos))  return CELL_HSIZE;
	if (CellActionRect(CELL_VSIZE,  nRow, nCol).PtInRect(ptMousePos))  return CELL_VSIZE;
	if (CellActionRect(CELL_SIZE,   nRow, nCol).PtInRect(ptMousePos))  return CELL_SIZE;

	return MOVE;
}

//---------------------------------------------------------------------------
Table::ActionType Table::TableAction (CPoint ptMousePos)
{
	Table::ActionType nAction = LEAVE;

	if (!m_BaseRect.PtInRect(ptMousePos)) return nAction;

	if ((nAction = TitleAction(m_TitleRect, ptMousePos)) != LEAVE)
	{
		m_nActiveRow = ACTIVE_IS_TITLE;
		m_nActiveColumn = NO_ACTIVE_COLUMN;
		return nAction;
	}

	for (int nCol = 0; nCol <= LastColumn(); nCol++)
	{
		if ((nAction = ColumnAction(ColumnTitleRect(nCol), ptMousePos, nCol)) != LEAVE)
		{
			m_nActiveRow = ACTIVE_IS_TITLE;
			m_nActiveColumn = nCol;
			return nAction;
		}

		if (HasTotal(nCol) && (nAction = ColumnAction(TotalRect(nCol), ptMousePos, nCol)) != LEAVE)
		{
			m_nActiveRow = ACTIVE_IS_TOTAL;
			m_nActiveColumn = nCol;
			return nAction;
		}

		for (int nRow = 0; nRow <= LastRow(); nRow++)
		{
			if ((nAction = CellAction(CellRect(nRow, nCol), ptMousePos, nRow, nCol)) != LEAVE)
			{
				m_nActiveRow = nRow;
				m_nActiveColumn = nCol;
				return nAction;
			}
		}
	}
	return nAction;
}



//---------------------------------------------------------------------------
CRect Table::TitleActionRect (ActionType nAction)
{
	switch (nAction)
	{
		case T_VSIZE :
		{
			CRect   rect(m_TitleRect);
			int     offset = Min (ACTION_SENS, rect.Height() / COL_SENS);
			rect.bottom = rect.top + offset;

			// almost 4 pixel height
			if (rect.Height() < 4)
				return m_TitleRect;
			else
				return rect;
		}

		case ADD_ROW:
		{
			CRect rect(RowRect(LastRow()));

			rect.top = rect.bottom - 5;

			return rect;
		}
	}
	return CRect();
}

//---------------------------------------------------------------------------
CRect Table::ColumnActionRect (ActionType nAction, int nCol)
{
	if (nAction != CT_VSIZE) return CRect();
	
	CRect   rect(ColumnTitleRect(nCol));
	int     offset = Min (ACTION_SENS, rect.Height() / COL_SENS);
	rect.bottom = rect.top + offset;

	// almost 4 pixel height
	return (rect.Height() < 4) ? ColumnTitleRect(nCol) : rect;
}

//---------------------------------------------------------------------------
CRect Table::TotalActionRect (ActionType nAction, int nCol)
{
	// if column don't Has total skip Action
	if ((nAction != TOT_VSIZE) || (!HasTotal(nCol))) return CRect();

	CRect   rect(TotalRect(nCol));
	int     offset = Min (ACTION_SENS, rect.Height() / COL_SENS);
	rect.top = rect.bottom - offset;

	// almost 4 pixel height
	return (rect.Height() < MINTOTAL_Y) ? TotalRect(nCol) : rect;
}

//---------------------------------------------------------------------------
CRect Table::CellActionRect (ActionType nAction, int nRow, int nCol)
{
	CRect   rect(CellRect(nRow, nCol));

	switch (nAction)
	{
		case CELL_HSIZE :
		{
			CSize sq(ACTION_SENS, rect.Height() - ACTION_SENS);
			CSize offset (ACTION_SENS, rect.Height());
			return CRect (rect.BottomRight() - offset, sq);
		}

		case CELL_SIZE :
		{
			CSize sq(ACTION_SENS, ACTION_SENS);
			return CRect (rect.BottomRight() - sq, sq);
		}

		case CELL_VSIZE :
		{
			CSize sq(rect.Width() - ACTION_SENS, ACTION_SENS);
			CSize offset (rect.Width(), ACTION_SENS);
			return CRect (rect.BottomRight() - offset, sq);
		}
	}

	return CRect();
}



//---------------------------------------------------------------------------
void Table::Resize (CPoint C, CPoint P)
{
	CSize   csDelta (C - P);
	int     nCol = NormalizedColumn();
	int     nRow = NormalizedRow();

	// limit Resize to lecit value
	switch (m_nAction)
	{
		case MOVE :
			if  (
					((m_BaseRect.top + csDelta.cy) >= 0) &&
					((m_BaseRect.left + csDelta.cx) >= 0)
				)
				MoveTable(csDelta);
			break;

		case CELL_VSIZE :
			if (CanVExpandRow (nRow, csDelta.cy))
				VExpandRow(nRow, csDelta.cy);
			break;

		case CELL_HSIZE :
			if (CanHExpandColumn(nCol, csDelta.cx))
				HExpandColumn(nCol, csDelta.cx, TRUE);
			break;

		case CELL_SIZE  :
			if (CanVExpandRow (nRow, csDelta.cy))
				VExpandRow(nRow,csDelta.cy);

			if (CanHExpandColumn(nCol, csDelta.cx))
				HExpandColumn(nCol, csDelta.cx, TRUE);
			break;

		case CT_VSIZE   :
			if  (
				((m_BaseRect.top + csDelta.cy) >= 0) &&
				(CanVExpandColumnTitle(csDelta.cy))
				)
				VExpandColumnTitle(csDelta.cy);
			break;

		case T_VSIZE    :
			if  (
				((m_BaseRect.top + csDelta.cy) >= 0) &&
				(CanVExpandTitle(csDelta.cy))
				)
				VExpandTitle(csDelta.cy);
			break;

		case TOT_VSIZE  :
			if  (
					HasTotal (nCol) &&
					CanVExpandTotal(csDelta.cy) &&
					(
						(m_BaseRect.bottom + csDelta.cy) >= 
						(TitlesHeight() + RowHeight())
					)
				)
				VExpandTotal(csDelta.cy);
			break;

		case ADD_ROW :
			if ((m_BaseCellsRect.bottom + csDelta.cy) > (TitlesHeight() + RowHeight()))
			{
				// modifiy rectangles for tacking purpose, but must
				// recalculate in AfterAction for determinate correct dimension
				// based on row number effectively present
				m_BaseCellsRect.bottom    += csDelta.cy;
				m_BaseRect.bottom         += csDelta.cy;
			}
			break;

		default : break;
	}   
	
	// some dimention or position are changed, so must set modified
	if (m_nAction != LEAVE)
		m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
int Table::CalcRowsNumber()
{
	int nCellsHeight = m_BaseCellsRect.Height() - TitlesHeight();

	// add row il almost 1 pixel over last row
	// remove row if reduced of entire last row
	return ((nCellsHeight - 1) / RowHeight() + 1);
}


//------------------------------------------------------------------------------
void Table::OnTrackingDraw(CDC& DC, CBCGPScrollView* pView, BOOL bShowRect)
{
	CRect   rectTrack;
	int     nCol = NormalizedColumn();
	int     nRow = NormalizedRow();

	switch (m_nAction)
	{

		case CELL_VSIZE :
		case CELL_HSIZE :
		case CELL_SIZE  :
		{
			CRect   cell_rect (CellRect(nRow,nCol));
			CSize   nSize (cell_rect.Width(), cell_rect.Height());
			CRect   rect (m_ptCellOrigin, nSize);

			rectTrack = rect;
			if (bShowRect)
			{
				CRect rect2 = NormalizedBaseRect();
				ScaleRect(rect, DC);
				ScaleRect(rect2, DC);

				DC.Rectangle(rect);
				DC.Rectangle(rect2);
			}
			break;
		}

		case CT_VSIZE :
		{
			rectTrack = ColumnTitleRect(nCol);
			if (bShowRect)
			{
				CRect   rect  = rectTrack;
				CRect   rect2 = m_TitleRect;

				ScaleRect(rect, DC);
				ScaleRect(rect2, DC);

				DC.Rectangle(rect);
				DC.Rectangle(rect2);
			}
			break;
		}

		case T_VSIZE :
		{
			rectTrack = m_TitleRect;
			CRect   rect = rectTrack;
			if (bShowRect)
			{
				ScaleRect(rect, DC);
				DC.Rectangle(rect);
			}
			break;
		}

		case TOT_VSIZE :
		{
			// tack only if there are totals
			if (!HasTotal(nCol)) return;
			
			// tack all nRow containing total
			CSize nSize (m_BaseRect.Width(), TotalRect(nCol).Height());
			rectTrack = CRect (TotalRect(0).TopLeft(), nSize);
			if (bShowRect)
			{
				CRect   rect = rectTrack;
				ScaleRect(rect, DC);
				DC.Rectangle(rect);
			}
			break;
		}

		case ADD_ROW :
		{
			if (CalcRowsNumber() > 1)
			{
				rectTrack = NormalizedBaseRect();
				if (bShowRect)
				{
					CRect   rect = rectTrack;
					ScaleRect(rect, DC);
					DC.Rectangle(rect);
				}
			}
			break;
		}

		default :
		{
			rectTrack = NormalizedBaseRect();
			if (bShowRect)
			{
				CRect   rect = rectTrack;
				ScaleRect(rect, DC);
				DC.Rectangle(rect);
			}
			break;
		}
	}

	// disegna i crocicchi
	TrackingCrossDraw(DC, pView);

	m_pDocument->m_TrackRect = rectTrack;
	m_pDocument->m_wTrackRows = (m_nAction == ADD_ROW) ? CalcRowsNumber() : RowsNumber();
}

//------------------------------------------------------------------------------
void Table::ModifyRowsNumber (int nNrRows)
{
	// avoid eventual call with no rows to add
	if (nNrRows != 0)
	{
		// remove cells for all columns
		int ub = m_Columns.GetUpperBound();
		for (int i = 0; i <= ub; i++)
			if (nNrRows > 0) 
					m_Columns[i]->AddCells (nNrRows);
			else    m_Columns[i]->RemoveCells (-nNrRows);

		// remove interline flags for requested row
		for (int nRow = 1; nRow <= abs(nNrRows); nRow++)
			if (nNrRows > 0)
			{
				m_Interlines.Add(FALSE);
				m_LineWithTitles.Add(FALSE);
				m_LineWithCustomTitles.Add(L"");
			}
			else    
			{
				m_Interlines.RemoveAt(m_Interlines.GetUpperBound());
				m_LineWithTitles.RemoveAt(m_LineWithTitles.GetUpperBound());
				m_LineWithCustomTitles.RemoveAt(m_LineWithCustomTitles.GetUpperBound());
			}

		// adjust row number in editor manager
		if (!m_pDocument->m_Template.m_bIsTemplate)
			m_pDocument->m_pEditorManager->SetTableRows (m_wInternalID, RowsNumber());
		
		if (nNrRows < 0)
			m_pDocument->AdjustRowsExportSelection (m_wInternalID, RowsNumber());
		// must recalculate right size of entire table rect because rows are changed.
		// m_BaseRect and m_BaseCellsRect was changed by TackingDraw procedure
		//
		m_BaseRect.bottom =
			m_BaseRect.top +
			TitlesHeight() +
			RowsNumber() * RowHeight() +
			TotalRect(0).Height();

		m_BaseCellsRect.bottom =
			m_BaseCellsRect.top +
			TitlesHeight() +
			RowsNumber() * RowHeight();
	}
}


//------------------------------------------------------------------------------
void Table::BeforeAction ()
{
	// enable indicatore
	m_pDocument->m_bTrackRectIndicatorActive = TRUE;
	m_pDocument->m_bTrackRowsIndicatorActive = TRUE;

	// store starting position of cellrect for tracking purpose
	int     nCol = NormalizedColumn();
	int     nRow = NormalizedRow();
	CRect   normalizedRect = NormalizedBaseRect();

	m_ptCellOrigin = CellRect(nRow,nCol).TopLeft();
	m_pDocument->m_wTrackRows = RowsNumber();
}

//------------------------------------------------------------------------------
void Table::AfterAction ()
{
	m_pDocument->m_bTrackRectIndicatorActive = FALSE;
	m_pDocument->m_bTrackRowsIndicatorActive = FALSE;

	// segnala all'editor manager che la size e' cambiata
	if (m_nAction == CELL_HSIZE || m_nAction == CELL_SIZE)
		m_Columns[NormalizedColumn()]->SetFieldWidth();

	if (m_nAction == ADD_ROW)
	{
		int nRequiredRow = CalcRowsNumber();
		if (nRequiredRow < 1) nRequiredRow = 1;

		// try to add or remove rows if needed
		ModifyRowsNumber (nRequiredRow - RowsNumber());
	}
}

//------------------------------------------------------------------------------
BOOL Table::CheckColumnsHiddenStatus ()
{
	BOOL bHiddenColumnChanged = FALSE;
	BOOL bUpdate = FALSE;
	for (int i = 0; i < m_Columns.GetSize(); i++)
	{
		TableColumn* pCol = m_Columns[i];

		if (pCol->m_pHideExpr && !pCol->m_pHideExpr->IsEmpty())
			bHiddenColumnChanged = CheckHiddenStatus(i) || bHiddenColumnChanged;

		if (pCol->m_pDynamicWidthExpr)
		{
			bHiddenColumnChanged = CheckDynamicWidth(i) || bHiddenColumnChanged;
		}

		if (bHiddenColumnChanged && pCol->m_arAnchoredFields.GetSize() > 0)
		{
			for (int i = 0; i < pCol->m_arAnchoredFields.GetSize(); i++)
			{
				BaseObj* pObj = pCol->m_arAnchoredFields.GetAt(i);
				m_pDocument->InvalidateRect(pObj->GetBaseRect(),TRUE);
			}
		}
	}
	return bHiddenColumnChanged;
}

//------------------------------------------------------------------------------
void Table::Paint (CDC& DC, CPrintInfo* pInfo)
{
	// suring mousemove in tracking mode, Paint is inhibit
	if (m_bInhibitPaint) 
		return;

	BOOL bHiddenColumnChanged = CheckColumnsHiddenStatus();
	if (!ExistsVisibleColumn())
		return;
	if (bHiddenColumnChanged)
	{
		m_pDocument->UpdateWindow();	//se la tabella si può essere stretta occorre pulire a destra, oltre il baserect

		Draw(DC, m_BaseRect, pInfo ? pInfo->m_bPreview : FALSE);
	}
	else
	{
		CRect rect = GetRectToInvalidate ();
		ScaleRect(rect, DC);
		CRect invalid;
		DC.GetClipBox(&invalid);
		// in stampa non gestisco l'invalidazione ma stampo comunque
		if (pInfo || rect.IntersectRect(&rect, &invalid))
			Draw(DC, invalid, pInfo ? pInfo->m_bPreview : FALSE);
	}

	// copy bitmap info into clipboard if posted from user Action
	if (m_bPostedCopyClipboard)
		CopyClipboard();
}

#define NUM_CURSORS 11
//------------------------------------------------------------------------------
static const HCURSOR* LoadCursors()
{   
	static HCURSOR NEAR rghCursors[NUM_CURSORS];	 // last defined cursor

	// initilization of static cursor array for speed optimization
	// Note: all track cursors must live in same module
	HINSTANCE hInst = AfxFindResourceHandle(
		MAKEINTRESOURCE(IDC_ARROW_MOVE), RT_GROUP_CURSOR);
			   
	// initialize the cursor array
	rghCursors [0] = ::LoadCursor(NULL,  MAKEINTRESOURCE(IDC_ARROW));
	rghCursors [1] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_MOVE));
	rghCursors [2] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_TABLE_ADD_ROW));
	rghCursors [3] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_COLUMN_SIZE));
	rghCursors [4] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_VERT));
	rghCursors [5] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_NW_SE));
	rghCursors [6] = ::LoadCursor(NULL,  MAKEINTRESOURCE(IDC_ARROW));
	rghCursors [7] = ::LoadCursor(NULL,  MAKEINTRESOURCE(IDC_ARROW));
	rghCursors [8] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_VERT));
	rghCursors [9] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_TITLE_UP));
	rghCursors[10] = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_ARROW_VERT));
	return rghCursors;
}

//------------------------------------------------------------------------------
void Table::SetObjCursor(ActionType nAction)
{            
	ASSERT(nAction < NUM_CURSORS);
	// various GDI objects we need to draw
	static const  HCURSOR* NEAR rghCursors = LoadCursors();
	SetCursor(rghCursors[nAction]);
}


//------------------------------------------------------------------------------
BOOL Table::EditorManagerCreateField(int nIDM, CWordArray& ids, TblRuleData* pTblRule/*=NULL*/)
{
	EditorManager* pEdt = m_pDocument->m_pEditorManager;
	/*if (nIDM == ID_ADD_DATA_FIELD)
		return pEdt->AddTableDataField(m_wInternalID, ids, FALSE, pTblRule);*/
	if (nIDM == ID_ADD_EXP_FIELD)
		return pEdt->AddTableExpField     (m_wInternalID, ids, FALSE);
	if (nIDM == ID_ADD_HIDDEN_FIELD)
		return pEdt->AddTableHiddenField  (m_wInternalID, ids, FALSE);
	if (nIDM == ID_ADD_VAR_FIELD)
		return pEdt->AddTableVarField     (m_wInternalID, ids, FALSE);
	return FALSE;
}

//------------------------------------------------------------------------------
void Table::AddColumn(CWordArray& ids, TblRuleData* pTblRule /*= NULL*/)
{
	if (m_Columns.GetUpperBound() == -1)
	{ 
		ASSERT(FALSE);
		return;
	}
	if (m_nActiveColumn < 0 || m_nActiveColumn > m_Columns.GetUpperBound())
		m_nActiveColumn = m_Columns.GetUpperBound();

	// position wher add before or after
	int nCol = m_nActiveColumn;

	if (m_pDocument->m_pWoormIni->m_bAddColumnBefore)
		for (int i = ids.GetUpperBound(); i >= 0; i--)
		{
			// need because active column become the last column added
			nCol = m_nActiveColumn;

			// copy active column for adding process
			TableColumn* old_column = m_Columns[nCol];
			TableColumn* new_column = new TableColumn(*old_column, ids[i]);

			m_pDocument->SetTemplateStyle(new_column);
			// add to array list and clear data from previosus column
			m_Columns.InsertAt(nCol, new_column);

			// must be done after creation of new column
			int width = ColumnRect(nCol).Width();

			// move remaining column
			RightShiftColumn(nCol, width);
		}
	else
		for (int i = 0; i <= ids.GetUpperBound(); i++)
		{
			// need if active column are changing
			nCol = m_nActiveColumn;

			// copy active column for adding process
			TableColumn* old_column = m_Columns[nCol];
			TableColumn* new_column = new TableColumn(*old_column, ids[i]);

			nCol++;
			m_pDocument->SetTemplateStyle(new_column);

			if (nCol <= m_Columns.GetUpperBound())
			{
				m_Columns.InsertAt(nCol, new_column);

				// must be done after creation of new column
				int width = ColumnRect(nCol).Width();
				int prev_col_width = ColumnRect(nCol - 1).Width();

				// move new column for width of previosus column
				m_Columns[nCol]->HMoveColumn(prev_col_width);

				// move remaining column for width of inserted column
				RightShiftColumn(nCol, width);
			}
			else
			{
				m_Columns.Add(new_column);

				// must be done after creation of new column
				int width = ColumnRect(nCol).Width();
				int prev_col_width = ColumnRect(nCol - 1).Width();

				// adjust table title and global size and shift
				m_TitleRect.right += width;
				m_BaseCellsRect.right += width;
				m_BaseRect.right += width;

				// move new column for width of previosus column
				m_Columns[nCol]->HMoveColumn(prev_col_width);
			}

			m_nActiveColumn = nCol;
		}

	m_pDocument->InvalidateRect(m_BaseRect, m_bTransparent);
	m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);

	// refresh active status if this are current
	if (m_pDocument->m_pCurrentObj == this)
		m_pDocument->m_pActiveRect->SetActive(GetActiveRect());

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

void Table::AddColumn(int nIDM, TblRuleData* pTblRule/* = NULL*/)
{
	if ((m_nActiveColumn < 0) || (m_nActiveColumn > m_Columns.GetUpperBound()))
		return;

	// add new entry if user want it. (don't add new table)
	CWordArray ids;
	if (!EditorManagerCreateField(nIDM, ids, pTblRule)) 
		return;

	AddColumn(ids, pTblRule);
}

//------------------------------------------------------------------------------
BOOL Table::MoveColumn(BOOL bLeft)
{
	if  (
			(m_nActiveColumn < 0) ||
			(m_nActiveColumn > m_Columns.GetUpperBound()) ||
			(m_Columns.GetSize() < 2) ||
			(bLeft && (m_nActiveColumn == 0)) ||
			((!bLeft) && (m_nActiveColumn == m_Columns.GetUpperBound()))
		)
		return FALSE;

	
	TableColumn* pActive = m_Columns[m_nActiveColumn];
	int prevColumn = m_nActiveColumn;
	BOOL bNearColumnHidden = FALSE;

	if (bLeft)
	{
		TableColumn* pLeft  = m_Columns[m_nActiveColumn - 1];
		bNearColumnHidden = pLeft->IsHidden();		
		
		int width_left      = pLeft->GetColumnTitleRect().Width();
		int width_active    = pActive->GetColumnTitleRect().Width();

		pActive->HMoveColumn (-width_left);
		pLeft->HMoveColumn (width_active);

		m_Columns[m_nActiveColumn] = pLeft;
		m_Columns[m_nActiveColumn - 1] = pActive;
		m_nActiveColumn--;
	}
	else
	{
		TableColumn* pRight = m_Columns[m_nActiveColumn + 1];
		bNearColumnHidden = pRight->IsHidden();		
		
		int width_right     = pRight->GetColumnTitleRect().Width();
		int width_active    = pActive->GetColumnTitleRect().Width();

		pActive->HMoveColumn (width_right);
		pRight->HMoveColumn (-width_active);

		m_Columns[m_nActiveColumn] = pRight;
		m_Columns[m_nActiveColumn + 1] = pActive;
		m_nActiveColumn++;
	}

	// totals need repaint of background because can be column without total
	m_pDocument->InvalidateRect(ColumnCellsRect(prevColumn), m_bTransparent);
	m_pDocument->InvalidateRect(ColumnCellsRect(m_nActiveColumn), m_bTransparent);

	int prevTotal = bLeft ? m_nActiveColumn : m_nActiveColumn - 1;
	if (prevTotal) m_pDocument->InvalidateRect(TotalRect(prevTotal - 1), TRUE);
	m_pDocument->InvalidateRect(TotalRect  (prevColumn), TRUE);
	m_pDocument->InvalidateRect(TotalRect  (m_nActiveColumn), TRUE);

	// refresh active status if this are current
	if (m_pDocument->m_pCurrentObj == this)
		m_pDocument->m_pActiveRect->SetActive(GetActiveRect());
	
	// Entriamo in ricorsione per scavalcare eventuali colonne nascoste
	if (bNearColumnHidden)
	{
		return MoveColumn(bLeft);
	}

	// Ad abbundanzia, visto il patchword di totali do una bella invalidata a tutti i totali
	m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);//ITRI
	
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
	return TRUE;
	
}

//------------------------------------------------------------------------------
BOOL Table::MoveColumn(int nFromAlias, int nToAlias)
{
	//check parameters
	TableColumn* fromCol = GetColumnByAlias(nFromAlias);
	TableColumn* toCol = GetColumnByAlias(nToAlias);

	if (!fromCol || !toCol)
		return FALSE;

	ASSERT_VALID(fromCol);
	ASSERT_VALID(toCol);

	int fromColIdx = GetColumnIndexByPtr(fromCol);
	int toColIdx = GetColumnIndexByPtr(toCol);

	ASSERT(fromColIdx != toColIdx);
	ASSERT(fromColIdx == m_nActiveColumn);

	if (fromColIdx == m_nActiveColumn)
	{
		if (fromColIdx < toColIdx)
			while (fromColIdx < toColIdx)
			{
				MoveColumn(FALSE);
				fromColIdx++;
			}
		else if (fromColIdx > toColIdx)
			while (fromColIdx > toColIdx)
			{
				MoveColumn(TRUE);
				fromColIdx--;
			}

		m_pDocument->RefreshRSTree(ERefreshEditor::Layouts);
		m_pDocument->RSTreeEnsureVisibleItem(ERefreshEditor::Layouts, fromCol, TRUE, FALSE, TRUE);
	}
	return TRUE;
}

//------------------------------------------------------------------------------
void Table::AddColumnTotal()
{
	// must be in columns limits 
	if  (
		(m_nActiveColumn < 0) ||
		(m_nActiveColumn > m_Columns.GetUpperBound())
		)
		return;

	// column total already exist for this column
	if (HasTotal(m_nActiveColumn)) return;

	// do real work
	m_Columns[m_nActiveColumn]->AddColumnTotal();
	m_nActiveRow = ACTIVE_IS_TOTAL;

	m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);
	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
void Table::DeleteColumnTotal (int nCol)
{
	// must be in columns limits
	if  (
			(nCol < 0) ||
			(nCol > m_Columns.GetUpperBound())
		)
		return;

	// column total already does not exist for this column
	if (!HasTotal(nCol)) 
		return;

	// do real work
	m_Columns[nCol]->DeleteColumnTotal();

	m_nActiveRow = ACTIVE_IS_TITLE;
	
	// refresh totals line
	m_pDocument->InvalidateRect(AllTotalsRect(), TRUE);
	
	// refresh active status if this are current
	if (m_pDocument->m_pCurrentObj == this)
		m_pDocument->m_pActiveRect->SetActive (GetActiveRect());

	m_pDocument->UpdateWindow();
	m_pDocument->SetModifiedFlag();
}

//------------------------------------------------------------------------------
BOOL Table::SetTotalRectActiveById(int nID)
{
	int nCol = 0;
	TotalCell* pTotalCell = GetTotalCellById(nID, nCol);
	if(!pTotalCell)//non l'ho trovato
		return FALSE;
	m_pDocument->m_pCurrentObj = this;
	m_nActiveColumn = nCol;
	m_nActiveRow = ACTIVE_IS_TOTAL;
	m_pDocument->m_pActiveRect->SetActive(GetActiveRect());
	m_pDocument->InvalidateStackedObjects();
	m_pDocument->UpdateWindow();
	return TRUE;
}

//------------------------------------------------------------------------------
TotalCell* Table::GetTotalCellById(int nID, int& nCol)
{
	WoormTable*	pSymTable = m_pDocument->GetEditorSymTable();
	CWordArray	idsColTotal;
	WORD		idColTotal;

	int ub = m_Columns.GetUpperBound();
	for (int i = 0; i <= ub; i++)
	{
		if (!m_Columns[i]->HasTotal())
			continue;

		idsColTotal.RemoveAll();

		pSymTable->GetTotalOf(m_Columns[i]->GetInternalID(), idsColTotal, WoormField::FIELD_COLTOTAL);
		if (idsColTotal.GetSize() > 0)
		{
			idColTotal = idsColTotal[0];
			if (idColTotal == nID)
			{
				nCol = i;
				return m_Columns[i]->m_pTotalCell;
			}
		}
	}
	return NULL;
}

//------------------------------------------------------------------------------
void Table::DeleteColumnComplete (int nCol, BOOL bUpdateDoc/* = TRUE*/)
{
	if ((nCol < 0) || (nCol > m_Columns.GetUpperBound()))
		return;

	if (m_nActiveRow == ACTIVE_IS_TOTAL)
		DeleteColumnTotal(nCol);
	else
	{
		if (HasTotal(nCol))
			DeleteColumnTotal(nCol);

		DeleteColumnAll(nCol);
	}	

	m_pDocument->SetModifiedFlag();

	if (bUpdateDoc)
	{
		m_pDocument->UpdateWindow();
	}
}

//------------------------------------------------------------------------------
BOOL Table::DeleteColumnBody(int nCol)
{
	if ((nCol < 0) || (nCol > m_Columns.GetUpperBound()))
		return FALSE;

	TableColumn*    pColumn  = m_Columns[nCol];
	int             width   = ColumnRect(nCol).Width();
	CRect           old_rect= m_BaseRect;

	// try to delete editor manager entry (cancel also column total if any)
	if (!m_pDocument->m_Layouts.ExistsFieldID(GetInternalID(), TRUE))
	{
		if (!pColumn->DeleteEditorEntry()) 
			return FALSE;
	}

	// decrement column counter if active column Has it
	if (HasTotal(nCol)) 
		m_nTotalCounter--;

	// remove columm from array and delete it
	m_Columns.RemoveAt(nCol);
	delete pColumn;

	// adjust size and position of left positioned columns and modify global rect
	// (i.e. m_BaseRect m_BaseCellsRect titleRec)
	LeftShiftColumn (nCol, width);
	
	//  refresh the table image
	m_pDocument->InvalidateRect(old_rect, TRUE);

	return TRUE;
}

//------------------------------------------------------------------------------
void Table::DeleteColumnAll(int nCol)
{
	if ((nCol < 0) || (nCol > m_Columns.GetUpperBound()))
		return;

	if (!DeleteColumnBody(nCol))
		return;
	
	//------
	// deleted last column so must readjust column pointer 
	if (m_Columns.GetUpperBound() < m_nActiveColumn)
	{
		m_nActiveColumn = m_Columns.GetUpperBound();
	}

	// must activate title because new active column may not Has total
	if ((m_nActiveRow == ACTIVE_IS_TOTAL) && !HasTotal(m_nActiveColumn))
		m_nActiveRow = ACTIVE_IS_TITLE;

	// refresh active status if this are current
	if (m_pDocument->m_pCurrentObj == this)
		m_pDocument->m_pActiveRect->SetActive (GetActiveRect());
}

//------------------------------------------------------------------------------
CRect Table::AllColumnsTitleRect () const
{
	CRect temp = m_BaseRect;
	temp.top    = ColumnTitleRect(0).top;
	temp.bottom = ColumnTitleRect(0).bottom;

	return temp;
}

//------------------------------------------------------------------------------
CRect Table::AllTotalsRect () const
{
	CRect temp = m_BaseRect;
	temp.top    = TotalRect(0).top;
	temp.bottom = TotalRect(0).bottom;

	return temp;
}

//------------------------------------------------------------------------------
CRect Table::RowRect (int nRow) const
{
	CRect temp = m_BaseRect;
	temp.top    = CellRect(nRow, 0).top;
	temp.bottom = CellRect(nRow, 0).bottom;

	return temp;
}


//------------------------------------------------------------------------------
CRect Table::BodyRect () const
{
	CRect temp = m_BaseCellsRect;
	temp.top = CellRect(0, 0).top;

	return temp;
}

//------------------------------------------------------------------------------
CRect Table::TitleRect () const
{
	return m_TitleRect;
}


