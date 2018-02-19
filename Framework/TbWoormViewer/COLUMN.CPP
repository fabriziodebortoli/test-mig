
#include "stdafx.h"

#include <malloc.h>

#include <TbGeneric\minmax.h>
#include <TbGeneric\FontsTable.h>
#include <TbGenlib\const.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\expr.h>

#include <TbWoormEngine\edtmng.h>
#include <TbWoormEngine\ReportLink.h>

#include "export.h"
#include "cell.h"
#include "mclrdlg.h"
#include "table.h"
#include "viewpars.h"
#include "woormini.h"
#include "woormdoc.h"
#include "woormvw.h"
#include "woormfrm.h"
#include "column.h"
#include "mulselob.h"
#include "RSEditorUI.h"
#include "ExpExter.h"

// resources
#include "listdlg.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "beginh.dex"
	
#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// Non � possibile rideterminare la size corretta di una colonna hidden perch� non ho l'informazione
// del tipo della colonna in quanto in fase di parsing grafico non ho a disposizione l'EditorManager
// che non ha ancora parsato la zona di report (variables)
#define HIDDEN_DEFAULT_WIDTH	100

IMPLEMENT_DYNAMIC(TableColumn, CObject)

// used for create column when required by user
//
//------------------------------------------------------------------------------
TableColumn::TableColumn
	(
		CPoint	ptOrigin,
		CSize	csTitleSize,
		CSize	csCellSize,
		Table*	pTable,
		int		nRows,
		WORD	wID
	)
	:
	m_wInternalID		(wID),
	m_pTable			(pTable),
	m_ColumnPen			(pTable->m_pDocument->m_pDataDefaults->m_rgbColumnBorder),
	m_ColumnTitlePen	(pTable->m_pDocument->m_pDataDefaults->m_rgbColumnTitleBorder),
	m_pTotalCell		(NULL),
	m_bShowTotal		(FALSE),
	m_bMultipleRow		(FALSE),

	m_ShowAs			(FT_NORMAL),
		m_pBitmap			(NULL),
		m_pBarCode			(NULL),

	m_bIsHidden			(FALSE),
	m_nSavedWidth		(0),

	m_pHideExpr				(NULL),
	m_pDynamicWidthExpr		(NULL), m_nWidth(0),
	m_pTextColorExpr		(NULL),
	m_pBkgColorExpr			(NULL),
	m_pTitleExpr			(NULL),
	m_pTitleTextColorExpr	(NULL),
	m_pTitleBkgColorExpr	(NULL),
	m_pTotalTextColorExpr	(NULL),
	m_pTotalBkgColorExpr	(NULL),
	m_pTextFontExpr			(NULL),
	m_pCellBordersExpr		(NULL),
	m_pCellTooltipExpr		(NULL),
	m_pTitleTooltipExpr		(NULL),
	m_pCellFormatterExpr	(NULL),

	m_bPinnedColumn			(FALSE),
	m_bSplitterColumn		(FALSE),
	m_bAnchorColumnLeft		(FALSE),
	m_bAnchorColumnRight	(FALSE),

	m_bTemplate				(FALSE),
	m_bTemplateOverridden	(FALSE),
	m_pDefault				(NULL),

	m_bVMergeEmptyCell			(FALSE),
	m_bVMergeEqualCell			(FALSE),
	m_bVMergeTailCell			(FALSE),

	m_bHideWhenEmpty		(FALSE),
	m_bOptimizeWidth		(FALSE),
	m_pFormatter			(NULL),
	IDisposingSourceImpl(this)
{
	// created by a new id from editor manager if present also use default value
	DataType	dtype	= m_pTable->m_pDocument->m_pEditorManager->GetDataType(m_wInternalID);
	int			nLen	=	m_pTable->m_pDocument->m_pEditorManager->GetLen(m_wInternalID);

	// build pFormatter for this field and use correct nLen if any
	m_nFormatIdx = m_pTable->m_pDocument->m_pFormatStyles->GetFormatIdx(dtype);

	int lenF = m_pTable->m_pDocument->m_pFormatStyles->GetPaddedLen(m_nFormatIdx, &m_pTable->m_pDocument->GetNamespace());
	if (lenF && dtype != DATA_STR_TYPE) nLen = lenF;

	// color pen for write m_Title column text
	m_Title.SetTextColor	(DEFAULT_COLUMN_TITLE_FOREGROUND);
	m_Title.SetBkgColor		(DEFAULT_COLUMN_TITLE_BACKGROUND);
	m_Title.SetFontIdx		(m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_COLUMN_TITLE));
	m_Title.SetText			(m_pTable->m_pDocument->m_pEditorManager->GetPublicName(wID));

	// create calculated sizes and rect for m_Title and all m_Cells
	m_nWidth		= DefaultCellWidth (GetDefaultCellFontIdx(dtype), nLen);
	int title_cy	= DefaultColumnTitleHeight (m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_COLUMN_TITLE));
	int cell_cy		= DefaultCellHeight ();

	csTitleSize	= CSize(m_nWidth, title_cy);
	csCellSize	= CSize(m_nWidth, cell_cy);

	CRect	title_rect (ptOrigin,csTitleSize);
	CPoint	cell_origin(ptOrigin.x, ptOrigin.y + csTitleSize.cy);

	// create all m_Cells
	for (int i = 1; i <= nRows ; i++)
	{
		TableCell* cell = new TableCell(this, cell_origin, csCellSize);

		cell->SetCellFontIdx(GetDefaultCellFontIdx(dtype));
		m_Cells.Add (cell);
		cell_origin.y += csCellSize.cy;
	}

	// add total and increment total counter
	BOOL bExistTotal = m_pTable->m_pDocument->m_pEditorManager->ExistColTotalOf(m_wInternalID);

	m_pTotalCell = new TotalCell (this, cell_origin, csCellSize);
	m_pTotalCell->SetCellFontIdx(GetDefaultTotalFontIdx(dtype));
	m_bShowTotal = bExistTotal ||
		(
			m_pTable->m_pDocument->m_pWoormIni->m_bAutoTotal &&
			(
				(dtype == DATA_INT_TYPE) ||
				(dtype == DATA_LNG_TYPE) ||
				(dtype == DATA_DBL_TYPE) ||
				(dtype == DATA_MON_TYPE) ||
				(dtype == DATA_QTA_TYPE) ||
				(dtype == DATA_PERC_TYPE)
			)
		);
	if (m_bShowTotal)
	{
		m_pTable->m_nTotalCounter++;
		if (!bExistTotal)
			m_pTable->m_pDocument->m_pEditorManager->AddAutoColTotal
			(
				m_wInternalID,
				m_pTable->m_pDocument->m_pWoormIni->m_bTotalOnNewPage,
				m_pTable->m_pDocument->m_pWoormIni->m_bResetOnNewPage
			);
	}

	m_ColumnTitleRect = title_rect;
	m_ColumnCellsRect = CRect(ptOrigin, CSize(csTitleSize.cx, csTitleSize.cy + nRows * csCellSize.cy));
	m_ColumnRect		= CRect(ptOrigin, CSize(csTitleSize.cx, m_ColumnCellsRect.Height() + TotalRect().Height()));

	// Format and change alignment to all column m_Cells, total and subtotal
	// using current id from editor manager
	//
	SetDefaultAlign		(m_pTable->m_pDocument->m_pEditorManager->GetDataType(wID));
	SetDefaultFontIdx	(m_pTable->m_pDocument->m_pEditorManager->GetDataType(wID));

	// Format all column data
	Format();
}

// used fro create column when parsing from file
//
//------------------------------------------------------------------------------
TableColumn::TableColumn
	(
		CPoint	ptOrigin,
		CSize	csTitleSize,
		CSize	csCellSize,
		Table*	pTable,
		int		nRows
	)
	:
	m_wInternalID		(0),
	m_pTable			(pTable),
	m_ColumnPen			(pTable->m_pDocument->m_pDataDefaults->m_rgbColumnBorder),
	m_ColumnTitlePen	(pTable->m_pDocument->m_pDataDefaults->m_rgbColumnTitleBorder),
	m_pTotalCell		(NULL),
	m_bShowTotal		(FALSE),
	m_bMultipleRow		(FALSE),

	m_ShowAs			(FT_NORMAL),
		m_pBitmap			(NULL),
		m_pBarCode			(NULL),

	m_bIsHidden			(FALSE),
	m_nSavedWidth		(0),

	m_pHideExpr				(NULL),
	m_pDynamicWidthExpr		(NULL), m_nWidth(0),
	m_pTextColorExpr		(NULL),
	m_pBkgColorExpr			(NULL),
	m_pTitleExpr			(NULL),
	m_pTitleTextColorExpr	(NULL),
	m_pTitleBkgColorExpr	(NULL),
	m_pTotalTextColorExpr	(NULL),
	m_pTotalBkgColorExpr	(NULL),
	m_pTextFontExpr			(NULL),
	m_pCellBordersExpr		(NULL),
	m_pCellTooltipExpr		(NULL),
	m_pTitleTooltipExpr		(NULL),
	m_pCellFormatterExpr	(NULL),
	
	m_bPinnedColumn			(FALSE),
	m_bSplitterColumn		(FALSE),
	m_bAnchorColumnLeft		(FALSE),
	m_bAnchorColumnRight	(FALSE),

	m_bTemplate				(FALSE),
	m_bTemplateOverridden	(FALSE),
	m_pDefault				(NULL),

	m_bVMergeEmptyCell			(FALSE),
	m_bVMergeEqualCell			(FALSE),
	m_bVMergeTailCell			(FALSE),

	m_bHideWhenEmpty		(FALSE),
	m_bOptimizeWidth		(FALSE),
	m_pFormatter			(NULL),
	IDisposingSourceImpl(this)
{
	// created string typed because completly modified by Parse
	m_nFormatIdx = 0; //NO_FORMAT;

	// color pen for write m_Title column text
	m_Title.SetTextColor(DEFAULT_COLUMN_TITLE_FOREGROUND);
	m_Title.SetBkgColor	(DEFAULT_COLUMN_TITLE_BACKGROUND);
	m_Title.SetFontIdx	(m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_COLUMN_TITLE));
 
	m_nWidth = csTitleSize.cx;
	CRect	title_rect (ptOrigin, csTitleSize);
	CPoint	cell_origin(ptOrigin.x, ptOrigin.y + csTitleSize.cy);

	for (int i = 1; i <= nRows ; i++)
	{
		TableCell* cell = new TableCell(this, cell_origin, csCellSize);

		m_Cells.Add (cell);
		cell_origin.y += csCellSize.cy;
	}

	// add total always
	m_pTotalCell = new TotalCell (this, cell_origin, csCellSize);
	m_bShowTotal = FALSE;

	m_ColumnTitleRect = title_rect;
	m_ColumnCellsRect = CRect(ptOrigin, CSize(csTitleSize.cx, csTitleSize.cy + nRows * csCellSize.cy));
	m_ColumnRect		= CRect(ptOrigin, CSize(csTitleSize.cx, m_ColumnCellsRect.Height() + TotalRect().Height()));

	// the unitializzated variable are modified by Parse
}


// copy column constructor
//---------------------------------------------------------------------------
TableColumn::TableColumn (TableColumn& source, WORD wID)
	:
	m_wInternalID		(wID),
	m_ColumnPen			(source.m_ColumnPen),
	m_ColumnTitlePen	(source.m_ColumnTitlePen),
	m_Title				(source.m_Title),
	m_pTable			(source.m_pTable),
	m_ColumnTitleRect	(source.m_ColumnTitleRect),
	m_ColumnCellsRect	(source.m_ColumnCellsRect),
	m_ColumnRect		(source.m_ColumnRect),

	m_pTotalCell		(NULL),
	m_bShowTotal		(FALSE),

	m_bMultipleRow		(FALSE),
	m_bMiniHtml			(FALSE),

	m_ShowAs			(FT_NORMAL),
		m_pBitmap			(NULL),
		m_pBarCode			(NULL),

	m_bIsHidden			(FALSE),
	m_nSavedWidth		(0),

	m_pHideExpr				(NULL),
	m_pDynamicWidthExpr		(NULL), m_nWidth(source.m_nWidth),
	m_pTextColorExpr		(NULL),
	m_pBkgColorExpr			(NULL),
	m_pTitleExpr			(NULL),
	m_pTitleTextColorExpr	(NULL),
	m_pTitleBkgColorExpr	(NULL),
	m_pTotalTextColorExpr	(NULL),
	m_pTotalBkgColorExpr	(NULL),
	m_pTextFontExpr			(NULL),
	m_pCellBordersExpr		(NULL),
	m_pCellTooltipExpr		(NULL),
	m_pTitleTooltipExpr		(NULL),
	m_pCellFormatterExpr	(NULL),

	m_bPinnedColumn			(FALSE),
	m_bSplitterColumn		(FALSE),
	m_bAnchorColumnLeft		(FALSE),
	m_bAnchorColumnRight	(FALSE),

	m_sStyleClass			(source.m_sStyleClass),
	m_bTemplate				(source.m_bTemplate),
	m_bTemplateOverridden	(source.m_bTemplateOverridden),
	m_pDefault				(source.m_pDefault),

	m_bVMergeEmptyCell			(FALSE),
	m_bVMergeEqualCell			(FALSE),
	m_bVMergeTailCell			(FALSE),

	m_bHideWhenEmpty		(FALSE),
	m_bOptimizeWidth		(FALSE),
	m_pFormatter			(NULL),
	IDisposingSourceImpl(this)
{
	// created by a new id from editor manager. (only after copy column)
	// and wID must be different from 0.
	//
	DataType	dtype	= m_pTable->m_pDocument->m_pEditorManager->GetDataType(m_wInternalID);
	int			nLen	= m_pTable->m_pDocument->m_pEditorManager->GetLen(m_wInternalID);

	// build pFormatter for this field and use correct nLen if any
	m_nFormatIdx = m_pTable->m_pDocument->m_pFormatStyles->GetFormatIdx(dtype);
	int lenF = m_pTable->m_pDocument->m_pFormatStyles->GetPaddedLen(m_nFormatIdx, &m_pTable->m_pDocument->GetNamespace());
	if (lenF && dtype != DATA_STR_TYPE) 
		nLen = lenF;

	// m_Cells creations
	if (wID) 
		m_Title.SetText(m_pTable->m_pDocument->m_pEditorManager->GetPublicName(wID));

	// add all m_Cells copying from source (bad width... changed later)
	for (int i = 0; i <= source.m_Cells.GetUpperBound(); i++)
	{
		TableCell* source_cell = source.m_Cells[i];
		m_Cells.Add(new TableCell(*source_cell, this));
	}

	// add total
	BOOL bExistTotal = m_pTable->m_pDocument->m_pEditorManager->ExistColTotalOf(m_wInternalID);

	m_pTotalCell = new TotalCell (*(source.m_pTotalCell), this);
	m_bShowTotal = bExistTotal ||
		(
			m_pTable->m_pDocument->m_pWoormIni->m_bAutoTotal &&
			(
				(dtype == DATA_INT_TYPE) ||
				(dtype == DATA_LNG_TYPE) ||
				(dtype == DATA_DBL_TYPE) ||
				(dtype == DATA_MON_TYPE) ||
				(dtype == DATA_QTA_TYPE) ||
				(dtype == DATA_PERC_TYPE)
			)
		);
	if (m_bShowTotal)
	{
		m_pTable->m_nTotalCounter++;
		if (!bExistTotal)
			m_pTable->m_pDocument->m_pEditorManager->AddAutoColTotal
			(
				m_wInternalID,
				m_pTable->m_pDocument->m_pWoormIni->m_bTotalOnNewPage,
				m_pTable->m_pDocument->m_pWoormIni->m_bResetOnNewPage
			);
	}

	// adjust width of column using char number and charsize from font
	m_nWidth = DefaultCellWidth (GetDefaultCellFontIdx(dtype), nLen);
	HResizeColumn(m_nWidth);

	// Format and change alignment to all column m_Cells, total and subtotal
	// using current id from editor manager
	SetDefaultAlign		(m_pTable->m_pDocument->m_pEditorManager->GetDataType(wID));
	SetDefaultFontIdx	(m_pTable->m_pDocument->m_pEditorManager->GetDataType(wID));

	// empty data copyed by passed column
	DisableData ();

	Format		();
}

// copy column constructor
//---------------------------------------------------------------------------
TableColumn::TableColumn (const TableColumn& source)
	:
	m_pTable			(source.m_pTable),
	m_ColumnTitleRect	(source.m_ColumnTitleRect),
	m_ColumnCellsRect	(source.m_ColumnCellsRect),
	m_ColumnRect		(source.m_ColumnRect),
	m_wInternalID		(source.m_wInternalID),
	m_pTotalCell		(NULL),	//vedi sotto
	m_bShowTotal		(source.m_bShowTotal),

	m_ShowAs			(source.m_ShowAs),
		m_pBitmap			(source.m_pBitmap ? new CTBPicture(*(source.m_pBitmap)) : NULL),
		m_pBarCode			(source.m_pBarCode ? new CBarCode(*(source.m_pBarCode)) : NULL),

	m_nFormatIdx		(source.m_nFormatIdx),
	m_nFontIdx			(source.m_nFontIdx),

	m_nAlignType		(source.m_nAlignType),

	m_bMultipleRow		(source.m_bMultipleRow),
	m_bMiniHtml			(source.m_bMiniHtml),

	m_Title				(source.m_Title),

	m_ColumnTitlePen	(source.m_ColumnTitlePen),
	m_ColumnPen			(source.m_ColumnPen),

	m_SubTotal			(source.m_SubTotal),

	//m_bMixedColumnFont	(source.m_bMixedColumnFont),
	//m_bMixedColumnAlign	(source.m_bMixedColumnAlign),

	m_bIsHidden				(source.m_bIsHidden),
	m_nSavedWidth			(source.m_nSavedWidth),
	m_pHideExpr				(source.m_pHideExpr					? new Expression(*source.m_pHideExpr) : NULL),
	m_pDynamicWidthExpr		(source.m_pDynamicWidthExpr			? new Expression(*source.m_pDynamicWidthExpr) : NULL),
	m_nWidth				(source.m_nWidth),

	m_pTitleExpr			(source.m_pTitleExpr				? new Expression(*source.m_pTitleExpr) : NULL),
	m_pTitleTextColorExpr	(source.m_pTitleTextColorExpr		? new Expression(*source.m_pTitleTextColorExpr) : NULL),
	m_pTitleBkgColorExpr	(source.m_pTitleBkgColorExpr		? new Expression(*source.m_pTitleBkgColorExpr) : NULL),
	m_pTextColorExpr		(source.m_pTextColorExpr			? new Expression(*source.m_pTextColorExpr) : NULL),
	m_pBkgColorExpr			(source.m_pBkgColorExpr				? new Expression(*source.m_pBkgColorExpr) : NULL),
	m_pTotalTextColorExpr	(source.m_pTotalTextColorExpr		? new Expression(*source.m_pTotalTextColorExpr) : NULL),
	m_pTotalBkgColorExpr	(source.m_pTotalBkgColorExpr		? new Expression(*source.m_pTotalBkgColorExpr) : NULL),
	m_pTextFontExpr			(source.m_pTextFontExpr				? new Expression(*source.m_pTextFontExpr) : NULL),
	m_pCellBordersExpr		(source.m_pCellBordersExpr			? new Expression(*source.m_pCellBordersExpr) : NULL),
	m_pCellTooltipExpr		(source.m_pCellTooltipExpr			? new Expression(*source.m_pCellTooltipExpr) : NULL),
	m_pTitleTooltipExpr		(source.m_pTitleTooltipExpr			? new Expression(*source.m_pTitleTooltipExpr) : NULL),
	m_pCellFormatterExpr	(source.m_pCellFormatterExpr		? new Expression(*source.m_pCellFormatterExpr) : NULL),

	//Print attributes
	m_bAnchorColumnLeft		(source.m_bAnchorColumnLeft),   
	m_bAnchorColumnRight	(source.m_bAnchorColumnRight),
	m_bSplitterColumn		(source.m_bSplitterColumn),
	m_bPinnedColumn			(source.m_bPinnedColumn),
	//template style
	m_sStyleClass			(source.m_sStyleClass),
	m_bTemplate				(source.m_bTemplate),
	m_pDefault				(source.m_pDefault),
	m_bTemplateOverridden	(source.m_bTemplateOverridden),

	m_bVMergeEmptyCell			(source.m_bVMergeEmptyCell),
	m_bVMergeEqualCell			(source.m_bVMergeEqualCell),
	m_bVMergeTailCell			(source.m_bVMergeTailCell),
	//TODO
	m_bHideWhenEmpty		(source.m_bHideWhenEmpty),
	m_bOptimizeWidth		(source.m_bOptimizeWidth),

	m_pFormatter			(NULL),
	IDisposingSourceImpl(this)
{
	for (int i = 0; i <= source.m_Cells.GetUpperBound(); i++)
	{
		TableCell* source_cell = source.m_Cells[i];
		TableCell* pCell = new TableCell(*source_cell);
		pCell->m_pColumn = this; //CHANGE OWNER COLUMN
		m_Cells.Add(pCell);
	}

	if (source.m_pTotalCell)
	{
		m_pTotalCell = new TotalCell (*(source.m_pTotalCell));
		m_pTotalCell->m_pColumn = this; //CHANGE OWNER COLUMN
	}

	//non possiamo clonare m_arAnchoredFields perche non esistono ancora i baseobj cloni di quelli  ancorati
	//verranno reimpostati in seguito in analogia con il processo di parsing.
}

//---------------------------------------------------------------------------
TableColumn::~TableColumn()
{
	// delete all m_Cells for this column
	for (int i = m_Cells.GetUpperBound(); i >= 0; i--)
	{
		delete m_Cells[i];
		m_Cells[i] = NULL;
	}
	m_Cells.RemoveAll();

	// remove total if any
	SAFE_DELETE (m_pTotalCell)
	SAFE_DELETE (m_pBitmap)
	SAFE_DELETE (m_pBarCode)
	
	SAFE_DELETE (m_pHideExpr)
	SAFE_DELETE (m_pDynamicWidthExpr)
	SAFE_DELETE (m_pTextColorExpr)
	SAFE_DELETE (m_pBkgColorExpr)
	SAFE_DELETE (m_pTitleExpr)
	SAFE_DELETE (m_pTitleTextColorExpr)
	SAFE_DELETE (m_pTitleBkgColorExpr)
	SAFE_DELETE (m_pTotalTextColorExpr)
	SAFE_DELETE (m_pTotalBkgColorExpr)
	SAFE_DELETE (m_pTextFontExpr)
	SAFE_DELETE (m_pCellBordersExpr)
	SAFE_DELETE (m_pCellTooltipExpr)
	SAFE_DELETE (m_pTitleTooltipExpr)
	SAFE_DELETE (m_pCellFormatterExpr)
}

//------------------------------------------------------------------------------
BOOL TableColumn::IsHidden () const 
{
	if (m_pHideExpr && !m_pHideExpr->IsEmpty())
	{
		//TODO aggiungere la valutazione dell'espressione
	}
	return m_bIsHidden; 
}

//------------------------------------------------------------------------------
void TableColumn::SetHidden (BOOL bIsHidden)  
{ 
	m_bIsHidden = bIsHidden;
	if (bIsHidden)
	{
		int w = GetColumnTitleRect().Width();
		if (w)
		{
			if (m_nSavedWidth == 0)
				m_nSavedWidth = w;
		}

		HResizeColumn(0);
	}
	else
	{
		ASSERT(m_nSavedWidth);
		if (m_nWidth <= 0) 
			m_nWidth = (m_nSavedWidth ? m_nSavedWidth : HIDDEN_DEFAULT_WIDTH);

		HResizeColumn(m_nWidth);
	}
}

//------------------------------------------------------------------------------
// quando cambia la dimensione della cella in orizzontale lo comunica allo
// editor manager che formatta la stringa spezzandola su piu' righe. Vale solo per
// le stringhe

//------------------------------------------------------------------------------
int TableColumn::CalculateFieldWidth (const CString& strText) 
{
	const FontStyle* pFont = GetFontStyle(FALSE);

	CWoormView* pWView = m_pTable->m_pDocument->GetWoormView();
	ASSERT_VALID(pWView);
	CDC* pDC = pWView->GetDC();
	int cx = pFont ? pFont->GetStringWidth2 (pDC, strText) : 0;
	pWView->ReleaseDC(pDC);

	int nCharWidth = (int)((double) cx / strText.GetLength() + 0.5);

	int nWidth = m_ColumnRect.Width();

	if (m_pDynamicWidthExpr && m_pDynamicWidthExpr->HasFields())
	{
		DataInt diWidth;
		if (m_pDynamicWidthExpr->Eval(diWidth))
		{
			nWidth = diWidth;
		}
	}

	if (nWidth <= 0 && (m_pHideExpr || m_bIsHidden) && m_nSavedWidth >= 0)
		nWidth = m_nSavedWidth;

	return nCharWidth != 0
				? max(1, (int) ((nWidth - 2 * MARGIN_FROM_BORDER) / nCharWidth))
				: 1;
}

//------------------------------------------------------------------------------
FontStyle* TableColumn::GetFontStyle(BOOL bSubTotal)
{
	if (bSubTotal)
	{
		if (!m_pSubTotalFontStyle)
			m_pSubTotalFontStyle = m_pTable->m_pDocument->m_pFontStyles->GetFontStyle(this->m_SubTotal.m_nFontIdx, &m_pTable->m_pDocument->GetNamespace());

		return m_pSubTotalFontStyle;
	}
	else
	{
		if (!m_pFontStyle)
			m_pFontStyle = m_pTable->m_pDocument->m_pFontStyles->GetFontStyle(m_nFontIdx, &m_pTable->m_pDocument->GetNamespace());

		return m_pFontStyle;
	}
}

//------------------------------------------------------------------------------
BOOL TableColumn::CalculateFieldWidthFactors(FieldWidthFactors& fieldWidthFactors, BOOL bSubTotal/* = FALSE*/) 
{
	fieldWidthFactors.m_bMiniHtml = this->m_bMiniHtml;

	fieldWidthFactors.m_pFontStyle = GetFontStyle(bSubTotal);

	fieldWidthFactors.m_pWnd = m_pTable->m_pDocument->GetWoormView();
	if (fieldWidthFactors.m_pFontStyle == NULL || fieldWidthFactors.m_pWnd == NULL)
		return FALSE;
	fieldWidthFactors.m_pDC = m_pTable->m_pDocument->GetWoormView()->GetDC();

	fieldWidthFactors.m_pFormatStyle = GetFormatter();

	int nWidth = m_ColumnRect.Width();
	if (m_pDynamicWidthExpr && m_pDynamicWidthExpr->HasFields())
	{
		DataInt diWidth;
		if (m_pDynamicWidthExpr->Eval(diWidth))
		{
			nWidth = diWidth;
		}
	}

	if (m_pHideExpr || m_bIsHidden)
	{
		DataBool bHidden(TRUE);
		if (m_bMultipleRow && m_pHideExpr)
		{
			BOOL bOk = m_pHideExpr->Eval(bHidden);
			ASSERT(bOk);
		}
		nWidth = bHidden ? 32767 : m_nSavedWidth; 
	}
	fieldWidthFactors.m_nWidth = max(1, (nWidth - 2 * MARGIN_FROM_BORDER));
	fieldWidthFactors.m_nHeight = m_ColumnRect.Height();
	return TRUE;
}

//------------------------------------------------------------------------------
void TableColumn::SetFieldWidth()
{
	if	(
			!m_pTable->m_pDocument->m_pEditorManager ||
			!m_pTable->m_pDocument->m_pEditorManager->ExistProgram() ||
			(GetDataType() != DATA_STR_TYPE && GetDataType() != DATA_TXT_TYPE)
		)
		return;

	//The parameter string is used to estimate a default characters numbers that can be displayed into the cell
	//It use latin character set but we cannot change because all existing reports have this number stored into variables section.
	//EasyLook SplitString Algorithm adjust this value when draw multiline value into columns (Bug IT 11685)
	//If we change this behaviour, all existing report could have some problems with multilines data 
	int nChars = CalculateFieldWidth(_T("The Quick Broin fox jumpid over the cati."));
	//----

	m_pTable->m_pDocument->m_pEditorManager->SetFieldWidth(m_wInternalID, nChars, m_bMultipleRow);
}

// a causa di un eventuale cambio di tipo devo aggiornare il formattatore e
// tutti i font di celle e totali mettendoli al valore di default
//------------------------------------------------------------------------------
void TableColumn::UpdateTypeInfo()
{
	DataType dtype = m_pTable->m_pDocument->m_pEditorManager->GetDataType(m_wInternalID);
	SetColumnFormatIdx(m_pTable->m_pDocument->m_pFormatStyles->GetFormatIdx(dtype));
}

//------------------------------------------------------------------------------
CSize TableColumn::DefaultSize (FontIdx nFontIdx, int nLen)
{
	const FontStyle* pFont = m_pTable->m_pDocument->m_pFontStyles->GetFontStyle(nFontIdx, &m_pTable->m_pDocument->GetNamespace());

	CDC* pDC = m_pTable->m_pDocument->GetWoormView()->GetDC();
	CSize sz = pFont ? pFont->GetStringWidth (pDC, nLen) : CSize(0,0);
	m_pTable->m_pDocument->GetWoormView()->ReleaseDC(pDC);
	
	return sz;
}

// determinate font size for calculate default item size
// make attention on y size because must be equal also for different font height
// depending on cell data type. Use most height of both
//
//------------------------------------------------------------------------------
int TableColumn::DefaultCellWidth (FontIdx nFontIdx, int nLen)
{
	return Max (MINCELL_X, DefaultSize (nFontIdx, nLen).cx + 8 * MARGIN_FROM_BORDER);
}


// calculate m_Title height from default font and adding dimension of pen because body
// border are drawed inside column m_Title rectangle
//
//------------------------------------------------------------------------------
int TableColumn::DefaultColumnTitleHeight (FontIdx nFontIdx)
{
	return Max (MINCELL_Y, DefaultSize(nFontIdx).cy + 2 * m_ColumnTitlePen.GetWidth());
}

// determinate height based on maximum height of possible default cell font
// because can differ from differrent data type
//
//------------------------------------------------------------------------------
int TableColumn::DefaultCellHeight ()
{
	return Max (MINCELL_Y, (int) Max(DefaultSize(m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_NUM)).cy, DefaultSize(m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_STRING)).cy));
}


//------------------------------------------------------------------------------
void TableColumn::AddColumnTotal()
{
	BOOL bExistTotal = m_pTable->m_pDocument->m_pEditorManager->ExistColTotalOf(m_wInternalID);

	m_bShowTotal = TRUE;
	if (m_bShowTotal)
	{
		m_pTable->m_nTotalCounter++;
		if (!bExistTotal)
			m_pTable->m_pDocument->m_pEditorManager->AddAutoColTotal
			(
				m_wInternalID,
				m_pTable->m_pDocument->m_pWoormIni->m_bTotalOnNewPage,
				m_pTable->m_pDocument->m_pWoormIni->m_bResetOnNewPage
			);
	}
	(m_pTable->m_pDocument)->InvalidateRect(TotalRect(),TRUE);
}

//------------------------------------------------------------------------------
void TableColumn::DeleteColumnTotal()
{
	// cancel total if possible or forced by user
	if (m_pTable->m_pDocument->m_pEditorManager->DelColTotal
			(m_wInternalID/*, m_pTable->m_pDocument->m_pWoormIni->m_bAlwaysHidden*/))
	{
		m_bShowTotal = FALSE;
		m_pTable->m_nTotalCounter--;
		m_pTable->m_pDocument->InvalidateRect(TotalRect(), FALSE);
	}
}

//------------------------------------------------------------------------------
void TableColumn::SetDefaultAlign(const DataType& dtype)
{
	if (IsShowedAsString(dtype))
	{
		SetColumnAlign 				(DEFAULT_CELL_STRING_ALIGN);
		m_SubTotal.SetAlign			(DEFAULT_SUBTOTAL_STRING_ALIGN);
		m_pTotalCell->SetCellAlign	(DEFAULT_TOTAL_STRING_ALIGN);
	}
	else
	{
		SetColumnAlign 				(DEFAULT_CELL_NUM_ALIGN);
		m_SubTotal.SetAlign			(DEFAULT_SUBTOTAL_NUM_ALIGN);
		m_pTotalCell->SetCellAlign	(DEFAULT_TOTAL_NUM_ALIGN);
	}
}

//------------------------------------------------------------------------------
void TableColumn::SetDefaultFontIdx (const DataType& dtype)
{
	if (::IsShowedAsString(dtype))
	{
		SetColumnFontIdx 				(AfxGetFontStyleTable()->GetFontIdx(FNT_CELL_STRING));
		m_SubTotal.SetFontIdx			(AfxGetFontStyleTable()->GetFontIdx(FNT_SUBTOTAL_STRING));
		m_pTotalCell->SetCellFontIdx	(AfxGetFontStyleTable()->GetFontIdx(FNT_TOTAL_STRING));
	}
	else
	{
		SetColumnFontIdx	 			(AfxGetFontStyleTable()->GetFontIdx(FNT_CELL_NUM));
		m_SubTotal.SetFontIdx			(AfxGetFontStyleTable()->GetFontIdx(FNT_SUBTOTAL_NUM));
		m_pTotalCell->SetCellFontIdx	(AfxGetFontStyleTable()->GetFontIdx(FNT_TOTAL_NUM));
	}
}

//------------------------------------------------------------------------------
BOOL TableColumn::ColumnIsShowedAsString () const
{
	return ::IsShowedAsString(GetDataType());
}

//------------------------------------------------------------------------------
Formatter* TableColumn::GetFormatter()
{
	if (!m_pFormatter || m_pTable->m_pDocument->m_bAllowEditing)
		m_pFormatter = m_pTable->m_pDocument->m_pFormatStyles->GetFormatter(m_nFormatIdx, &m_pTable->m_pDocument->GetNamespace());

	ASSERT_VALID(m_pFormatter);
	return m_pFormatter;
}

//------------------------------------------------------------------------------
void TableColumn::Format()
{
	// make appropriate pFormatter
	Formatter* pFormatter = GetFormatter();

	// Format all column m_Cells
	for (int nRow = 0; nRow <= LastRow(); nRow++)
		m_Cells[nRow]->Format (m_wInternalID, pFormatter, m_pTable->m_pDocument);

	// Format eventual column total if any
	m_pTotalCell->Format (m_wInternalID, pFormatter, m_pTable->m_pDocument);

	(m_pTable->m_pDocument)->InvalidateRect(m_ColumnCellsRect, m_pTable->m_bTransparent);
	(m_pTable->m_pDocument)->InvalidateRect(TotalRect(), m_pTable->m_bTransparent);
}


//------------------------------------------------------------------------------
BOOL TableColumn::CheckTypeConsistence(CString& sError)
{
	// check consinstency between editor manager and Format manager
	// must be used AFTER editore manager end Parse.
	DataType	fmt = GetFormatter()->GetDataType();
	DataType	edt = m_pTable->m_pDocument->m_pEditorManager->GetDataType(m_wInternalID);
	BOOL		ok = fmt.m_wType == edt.m_wType;

	// bad id relation
	if (!ok)
	{
		sError = cwsprintf
									(
										_TB("Check format style consistence") + _T(" - ") +
										_TB("Id {0-%d}. Wrong data type\r\n{1-%s} expected\r\n{2-%s} found"),
										(int) m_wInternalID,
										(LPCTSTR) FromDataTypeToTokenString(fmt),
										(LPCTSTR) FromDataTypeToTokenString(edt)
									);


		m_pTable->m_pDocument->Message 
						(sError,
							MB_OK | MB_ICONSTOP
						);
	}
	return ok;
}

//---------------------------------------------------------------------------
void TableColumn::DrawColumnTitle
	(
		CDC&	DC,
		CRect	rectInvalid,
		BOOL	bFirst,
		BOOL	bLast,
		const CRect& rectColumnTitleRect,
		int nRow /*= -1*/
	)
{
	CRect	rect = rectColumnTitleRect;
	ScaleRect(rect, DC);
	// rectInvalid is already in device point
	if (DC.IsPrinting() || rect.IntersectRect(rect, rectInvalid))
	{
		Borders	title_borders
		(
			GetBorders().m_bColumnTitleTop && ShowTitles(DC.IsPrinting()),
			bFirst && (GetBorders().m_bColumnTitleLeft || GetBorders().m_bColumnTitleSeparator) && ShowTitles(DC.IsPrinting()),
			(GetBorders().m_bColumnTitleBottom && ShowTitles(DC.IsPrinting())) || GetBorders().m_bBodyTop,
			ShowTitles(DC.IsPrinting()) &&
			(
				(bLast && GetBorders().m_bColumnTitleRight) ||
				(!bLast && GetBorders().m_bColumnTitleSeparator)
			)
		);

		DrawBorders
		(
			DC,
			rectColumnTitleRect,
			m_ColumnTitlePen,
			&title_borders,
			NoBorders(DC.IsPrinting())
		);

		CRect inside = InsideRect
			(                        
				DC,
				rectColumnTitleRect,
				m_ColumnTitlePen,
				title_borders,
				NoBorders(DC.IsPrinting())
			);
		// Draw m_Title separated one pixel from Borders if required
		if (ShowTitles(DC.IsPrinting()))
		{
			if (m_pTitleExpr || m_pTitleTextColorExpr || m_pTitleBkgColorExpr)
			{
				if (nRow == -1)
					m_pTable->m_pDocument->UpdateViewSymbolTable();
				else
					m_pTable->m_pDocument->UpdateViewSymbolTable(m_pTable, nRow);
			}

			if (m_pTitleExpr)
			{
				DataStr t;
				if (m_pTitleExpr->Eval(t))
				{
					m_Title.SetText(t);
				}
			}

			COLORREF crText = m_Title.GetTextColor();
			COLORREF crBkg = m_Title.GetBkgColor();
			DataLng color;
			if (m_pTitleTextColorExpr && m_pTitleTextColorExpr->Eval(color))
			{
				m_Title.SetTextColor((COLORREF)(long)color);
			}
			if (m_pTitleBkgColorExpr && m_pTitleBkgColorExpr->Eval(color))
			{
				m_Title.SetBkgColor((COLORREF)(long)color);
			}

			m_Title.DrawGText(DC, inside, m_pTable->m_pDocument, !m_pTable->m_bTransparent, GenericText::NORMAL, TRUE);

			if (m_pTitleTextColorExpr) m_Title.SetTextColor(crText);
			if (m_pTitleBkgColorExpr) m_Title.SetBkgColor(crBkg);
		}
		
		if (m_pTable->m_pDocument->m_bAllowEditing && !DC.IsPrinting())
			DrawColumnIcons(DC, inside);
	}
}

//------------------------------------------------------------------------------
void TableColumn::DrawColumnIcons(CDC& DC, CRect& rectInside)
{		
	HDC hDC = DC.GetSafeHdc();
	
	// DPI Scaling
	ScaleRect(rectInside, DC);

	if (m_pHideExpr)
	{
		CTBPicture* pIcon =  m_pTable->m_pDocument->GetHiddenWhenIcon(DC);

		rectInside.top = rectInside.bottom - pIcon->GetHeight();
		rectInside.left = rectInside.right - pIcon->GetWidth();
		
		GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);	

		rectInside.right = rectInside.left;
	}

	if (m_bSplitterColumn)
	{
		CTBPicture* pIcon =  m_pTable->m_pDocument->GetSplitterIcon(DC);

		rectInside.top = rectInside.bottom - pIcon->GetHeight();
		rectInside.left = rectInside.right - pIcon->GetWidth();
		
		GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);	

		rectInside.right = rectInside.left;
	}

	if (m_bPinnedColumn)
	{
		CTBPicture* pIcon =  m_pTable->m_pDocument->GetPinnedIcon(DC);

		rectInside.top = rectInside.bottom - pIcon->GetHeight();
		rectInside.left = rectInside.right - pIcon->GetWidth();
		
		GenericDrawObj::DrawBitmap(DC, rectInside, pIcon);	

		rectInside.right = rectInside.left;
	}

	// DPI UnScaling
	UnScaleRect(rectInside, DC);
}

//---------------------------------------------------------------------------
CString TableColumn::GetTitle () const
{
	return m_Title.GetText();
}

//---------------------------------------------------------------------------
BOOL TableColumn::HasDynamicAttributeOnRow () const
{
	return 	m_pBkgColorExpr || 
			m_pTextColorExpr ||
			m_pTextFontExpr || 
			m_pCellBordersExpr ||
			m_pCellFormatterExpr ||
			m_SubTotal.m_pTextColorExpr ||
			m_SubTotal.m_pBkgColorExpr;
}

//---------------------------------------------------------------------------
void TableColumn::DrawCell
	(
		CDC&	DC,
		CRect	rectInvalid,
		int		nRow,
		BOOL	bFirstCol,
		BOOL	bLastCol,
		BOOL	bPreview,
		BOOL	bMarkExportable,
		BOOL	bIsExpTitles,
		BOOL	bRowSep,
		BOOL	bCurrRowHasTail
	)
{
	TableCell* pCell = m_Cells[nRow];
	TableCell* pNextCell = NULL;
	ASSERT_TRACE(pCell->m_pColumn == this, _TB("TableColumn::DrawCell: Cell has a wrong m_pColumn"));

	if 
		(
			nRow < m_Cells.GetUpperBound() &&
			(pNextCell = m_Cells[nRow + 1]) &&
			(
				(m_bVMergeEmptyCell && pNextCell->m_Value.GetText().IsEmpty())
				|| 
				(m_bVMergeTailCell && pNextCell->m_Value.m_RDEdata.IsTailMultiLineString())
				|| 
				(
					m_bVMergeEqualCell &&
					!pCell->m_Value.m_RDEdata.IsSubTotal() &&
					!pNextCell->m_Value.m_RDEdata.IsSubTotal() &&
					(
						(
						!pCell->m_Value.GetText().IsEmpty() &&
						pCell->m_Value.GetText() == pNextCell->m_Value.GetText()
						)
						||
						(
						bCurrRowHasTail &&
						!pCell->m_Value.m_RDEdata.IsValid() &&
						!pNextCell->m_Value.m_RDEdata.IsValid()
						)
						||
						(
						bCurrRowHasTail &&
						!pCell->m_Value.m_RDEdata.IsValid() &&
						pNextCell->m_Value.m_RDEdata.IsValid() &&
						m_PreviousValue.m_RDEdata.IsValid() &&
						m_PreviousValue.GetText() == pNextCell->m_Value.GetText()
						)
					)
				)
			)
		)
	{
		bRowSep = FALSE;
	}

	//top, left, bottom, right
	CellBorders cell_borders
		(
			nRow == 0 && m_pTable->m_bHideColumnsTitle && m_pTable->m_Borders.m_bBodyTop,
			bFirstCol && GetBorders().m_bBodyLeft,
			(
				((nRow != LastRow()) && (bRowSep || m_pTable->m_Interlines[nRow])) 
				||
				(nRow == LastRow() && GetBorders().m_bBodyBottom)
			),
			(
				(!bLastCol && GetBorders().m_bColumnSeparator) 
				||
				(bLastCol && GetBorders().m_bBodyRight)  
			),
			&m_ColumnPen,
			(nRow != LastRow() && bRowSep && !m_pTable->m_Interlines[nRow]  ? m_pTable->m_Borders.m_pRowSepPen : NULL)
		);

	CObject* pGraph = NULL;
	
	if (m_pBitmap)
		pGraph = m_pBitmap;
	else if (m_pBarCode)
		pGraph = m_pBarCode;

	BOOL bExport = 
		(
			m_pTable->m_pDocument->m_pDataDefaults->m_bUseAsRadar &&
			m_pTable->m_pDocument->m_arWoormLinks.GetConnectionRadar() &&
			(m_pTable->GetInternalID() == m_pTable->m_pDocument->m_arWoormLinks.GetConnectionRadar()->m_nAlias) &&
			(m_pTable->m_pDocument->m_arWoormLinks.GetConnectionRadar()->m_nCurrentRow == nRow)
		)
		||
		(
			bMarkExportable && 
			m_pTable->m_pDocument->m_pExportData->IncludeRow(nRow)
		);
	
	pCell->m_nCurrRow = nRow;

	if (HasDynamicAttributeOnRow ())
	{
		GetDocument()->UpdateViewSymbolTable (m_pTable, nRow, nRow && bCurrRowHasTail); 
	}

	pCell->Draw
		(
			DC, 
			bPreview,
			rectInvalid,
			m_pTable->m_pDocument,
			m_SubTotal,
			cell_borders,
			NoBorders(DC.IsPrinting()),
			pGraph,
			bExport,
			!m_pTable->m_bTransparent,
			bIsExpTitles,
			m_ShowAs == EFieldShowAs::FT_TEXTFILE
		);
}

//---------------------------------------------------------------------------
void TableColumn::DrawTotal
	(
		CDC&	DC,
		CRect	rectInvalid,
		BOOL	bFirst,
		BOOL	bNextColumnHasTotal,
		int		nNextVisibleColumn
	)
{
	// if must show total the Draw it otherwise hide it
	if (m_bShowTotal)
	{
		Borders total_borders
		(
			GetBorders().m_bTotalTop,
			bFirst && GetBorders().m_bTotalLeft,
			GetBorders().m_bTotalBottom,
			GetBorders().m_bTotalRight
		);

		m_pTotalCell->DrawTotal
		(
			DC, rectInvalid,
			m_pTable->m_pDocument,
			total_borders,
			NoBorders(DC.IsPrinting()),
			!m_pTable->m_bTransparent
		);
	}
	else
	{
		// Trick!! Right border of hided total is left border of next total
		Borders total_borders
		(
			FALSE,
			FALSE,
			FALSE,
			GetBorders().m_bTotalLeft // !! Don't use m_bTotalRight. See above
		);

		m_pTotalCell->HideTotal
		(
			DC, rectInvalid,
			total_borders,
			NoBorders(DC.IsPrinting()),
			bNextColumnHasTotal,
			nNextVisibleColumn
		);
	}
}

//---------------------------------------------------------------------------
void TableColumn::UpdateSplitterInfo(LONG oldValue)
{
	if (m_pTable->m_pDocument->GetNumberOfSplittedPage() == 0)
	{
		m_pTable->m_pDocument->m_PageInfo.m_arHPageSplitter.Remove(oldValue);
		m_pTable->m_pDocument->m_PageInfo.m_arHPageSplitter.AddSorted(m_ColumnTitleRect.right);
		m_pTable->m_pDocument->Invalidate(TRUE);
	}
}

//---------------------------------------------------------------------------
void TableColumn::VExpandTitle (int height)
{
	m_ColumnTitleRect.top	+= height;
	m_ColumnCellsRect.top	+= height;
	m_ColumnRect.top		+= height;
}

//---------------------------------------------------------------------------
void TableColumn::VExpandTotal (int height)
{
	m_ColumnRect.bottom	+= height;
	m_pTotalCell->VExpandCell (height);

	if ( m_arAnchoredFields.GetSize() > 0 )
		MoveAnchoredFields(0, height, TRUE);
}

//---------------------------------------------------------------------------
void TableColumn::MoveColumn(CSize csDelta)
{
	LONG oldRight = m_ColumnTitleRect.right;
	
	m_ColumnTitleRect.	OffsetRect(csDelta);
	m_ColumnCellsRect.	OffsetRect(csDelta);
	m_ColumnRect.		OffsetRect(csDelta);

	// move m_Cells
	for (int nRow = 0; nRow <= LastRow(); nRow++)
		m_Cells[nRow]->MoveCell(csDelta);

	// move total
	m_pTotalCell->MoveCell(csDelta);
	
	if ( m_bSplitterColumn )
		UpdateSplitterInfo(oldRight);
	if ( m_arAnchoredFields.GetSize() > 0 )
		MoveAnchoredFields(csDelta.cx, csDelta.cy);
}


//---------------------------------------------------------------------------
void TableColumn::HExpandColumn(int width)
{
	LONG oldRight = m_ColumnTitleRect.right;

	m_ColumnTitleRect.right	+= width;
	m_ColumnCellsRect.right	+= width;
	m_ColumnRect.right		+= width;

	// expand m_Cells horizontal
	for (int nRow = 0; nRow <= LastRow(); nRow++)
		m_Cells[nRow]->HExpandCell(width);

	// expand total horizontal
	m_pTotalCell->HExpandCell(width);

	if (m_bSplitterColumn)
		UpdateSplitterInfo(oldRight);
	if ( m_arAnchoredFields.GetSize() > 0 )
		HExpandAnchoredFields(width);
}

//---------------------------------------------------------------------------
void TableColumn::HResizeColumn(int width)
{
	LONG oldRight = m_ColumnTitleRect.right;

	m_ColumnTitleRect.right	= m_ColumnTitleRect.left + width;
	m_ColumnCellsRect.right	= m_ColumnCellsRect.left + width;
	m_ColumnRect.right		= m_ColumnRect.left + width;

	// resize cell horizontal
	for (int nRow = 0; nRow <= LastRow(); nRow++)
		m_Cells[nRow]->HResizeCell(width);

	// resize total horizontal
	m_pTotalCell->HResizeCell(width);

	if (m_bSplitterColumn)
		UpdateSplitterInfo(oldRight);
	if (m_arAnchoredFields.GetSize() > 0)
		HResizeAnchoredFields(width - (oldRight - m_ColumnCellsRect.left));
}

//---------------------------------------------------------------------------
void TableColumn::HMoveColumn(int width)
{
	LONG oldRight = m_ColumnTitleRect.right;

	m_ColumnTitleRect.	OffsetRect	(width,0);
	m_ColumnCellsRect.	OffsetRect	(width,0);
	m_ColumnRect.		OffsetRect	(width,0);

	// move m_Cells horizontal
	for (int nRow = 0; nRow <= LastRow(); nRow++)
		m_Cells[nRow]->HMoveCell(width);

	// move m_Cells horizontal
	m_pTotalCell->HMoveCell(width);

	if (m_bSplitterColumn)
		UpdateSplitterInfo(oldRight);
	if ( m_arAnchoredFields.GetSize() > 0 )
		MoveAnchoredFields(width);
}

//---------------------------------------------------------------------------
void TableColumn::VExpandRow (int, int height)
{
	int nGlobalHeight = height * (LastRow() + 1);

	m_ColumnCellsRect.bottom	+= nGlobalHeight;
	m_ColumnRect.bottom			+= nGlobalHeight;

	// expand m_Cells vertical
	int r = 0; 
	for (r = 0; r <= LastRow(); r++)
	{
		m_Cells[r]->VExpandCell(height);
		m_Cells[r]->VMoveCell(height * r);
	}

	// move total
	m_pTotalCell->VMoveCell(height * r);

	if ( m_arAnchoredFields.GetSize() > 0 )
		MoveAnchoredFields(0, nGlobalHeight, TRUE);
}

//---------------------------------------------------------------------------
void TableColumn::RemoveCells (int nr_cells)
{
	// total move height for column total
	int nTotalHeight = 0;

	for (int i =1; i <= nr_cells; i++)
	{
		TableCell*	bLast = m_Cells[LastRow()];
		int			height = bLast->GetCellRect().Height();

		m_Cells.RemoveAt(LastRow());
		delete bLast;

		m_ColumnCellsRect.bottom	-= height;
		m_ColumnRect.bottom			-= height;
		nTotalHeight				+= height;
	}

	// move total
	m_pTotalCell->VMoveCell(-nTotalHeight);

	if ( m_arAnchoredFields.GetSize() > 0 )
		MoveAnchoredFields(0, -nTotalHeight, TRUE);
}

//---------------------------------------------------------------------------
void TableColumn::AddCells (int nr_cells)
{
	// total move height for column total
	int nTotalHeight = 0;

	for (int i = 1; i <= nr_cells; i++)
	{
		int			nPos	= m_Cells.GetUpperBound();
		TableCell*	bLast	= new TableCell(*(m_Cells[nPos]), this);
		int			height	= bLast->GetCellRect().Height();

		m_Cells.Add(bLast);
		bLast->VMoveCell(height);

		m_ColumnCellsRect.bottom	+= height;
		m_ColumnRect.bottom			+= height;
		nTotalHeight				+= height;
	}

	// move total
	m_pTotalCell->VMoveCell(nTotalHeight);

	if ( m_arAnchoredFields.GetSize() > 0 )
		MoveAnchoredFields(0, nTotalHeight, TRUE);
}

//---------------------------------------------------------------------------
void TableColumn::MoveAnchoredFields(int xOffset, int yOffset /* = 0*/, BOOL bSkipTopField/*=FALSE*/)
{  
	for (int i = 0; i < m_arAnchoredFields.GetSize(); i++)
	{
		int yOff = yOffset;
		BaseObj* pObj = m_arAnchoredFields.GetAt(i);
		BaseRect* pBaseRect = NULL;
		if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			pBaseRect = (BaseRect*) pObj;

			if (pBaseRect->m_AnchorLeftColumnID != this->m_wInternalID)
				continue;
		}

		SelectionRect*	pSelRect = GetDocument()->m_pMultipleSelObj;
		BOOL bIsSelected = FALSE;
		if (pSelRect != NULL)
			bIsSelected = pSelRect->ObjectInSelection(pObj); 
		
		if (pBaseRect)
		{
			CRect baseRect = pBaseRect->GetBaseRect();
			GetDocument()->InvalidateRect(baseRect, TRUE);

			//se e' selezionato insieme alla tabella, gli consento di seguire solo 
			//il movimento sulla x, quello sulla y viene gia' effettuato per il fatto che e' selezionato
			//Gli oggetti ancorati infatti hanno libero solo il movimento sull'asse y ma non sull'asse x
			if (bIsSelected)
				yOff = 0;

			if (bSkipTopField && baseRect.top < this->m_ColumnRect.top)
				yOff = 0;

			pBaseRect->MoveBaseRect(baseRect.left + xOffset, baseRect.top + yOff , baseRect.right + xOffset, baseRect.bottom + yOff, TRUE, TRUE);
		}

		if (bIsSelected)
		{
			//recupero il selectedObj di questo baseObj e chiamo SetActive su questo per aggiornare il rettangolo rosa(ActiveRect)
			SelectedObj* pSelectedObj = pSelRect->GetSelectedObjInSelection(pObj);
			if (pSelectedObj != NULL)
				pSelectedObj->SetActive(FALSE);
		}
	}
}

//---------------------------------------------------------------------------
void TableColumn::HResizeAnchoredFields(int newWidth)
{
	for (int i = 0; i < m_arAnchoredFields.GetSize(); i++)
	{
		BaseObj* pObj = m_arAnchoredFields.GetAt(i);
		if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pBaseRect = (BaseRect*) pObj;
			CRect baseRect = pBaseRect->GetBaseRect();
			pBaseRect->MoveBaseRect(baseRect.left, baseRect.top, baseRect.right + newWidth, baseRect.bottom, TRUE, TRUE);
		}
	}
}

//---------------------------------------------------------------------------
void TableColumn::HExpandAnchoredFields(int increment)
{
	for (int i = 0; i < m_arAnchoredFields.GetSize(); i++)
	{
		BaseObj* pObj = m_arAnchoredFields.GetAt(i);
		if (pObj->IsKindOf(RUNTIME_CLASS(BaseRect)))
		{
			BaseRect* pBaseRect = (BaseRect*) pObj;
			CRect baseRect = pBaseRect->GetBaseRect();
			pBaseRect->MoveBaseRect(baseRect.left, baseRect.top, baseRect.left + baseRect.Width() + increment, baseRect.bottom, TRUE, TRUE);
		}
	}
}

//---------------------------------------------------------------------------
void TableColumn::RemoveSplitter()
{
	m_bSplitterColumn = FALSE;
	if (m_pTable->m_pDocument->m_PageInfo.m_arHPageSplitter.GetSize() > 0)
	{
		m_pTable->m_pDocument->m_PageInfo.m_arHPageSplitter.Remove(m_ColumnTitleRect.right);
		m_pTable->m_pDocument->Invalidate(TRUE);
	}
}

#define SET_ALIGN_FOR_MULTI_ROW(align) ((align & (DT_CENTER | DT_RIGHT | DT_VCENTER | DT_BOTTOM | DT_NOPREFIX | DT_EXPANDTABS)) | DT_SINGLELINE)

//------------------------------------------------------------------------------
void TableColumn::SetCellAlign (AlignType nAlign, int nRow)
{
	if (IsMultiRow())
		nAlign = SET_ALIGN_FOR_MULTI_ROW(nAlign);

	m_Cells[nRow]->SetCellAlign(nAlign);
}

//------------------------------------------------------------------------------
void TableColumn::SetColumnAlign (AlignType nAlign)
{
	m_pTotalCell->SetCellAlign (nAlign);

	if (IsMultiRow())
		nAlign = SET_ALIGN_FOR_MULTI_ROW(nAlign);

	//if (m_bMixedColumnAlign)
	//{
	//	for (int nRow = 0; nRow <= m_Cells.GetUpperBound(); nRow++)
	//	{
	//		AlignType nA = m_Cells[nRow]->GetCellAlign();
	//		if (nA == m_nAlignType)
	//			SetCellAlign (nAlign, nRow);
	//	}
	//}
	//else
	{
		for (int nRow = 0; nRow <= m_Cells.GetUpperBound(); nRow++)
			SetCellAlign (nAlign, nRow);
	}

	m_nAlignType = nAlign;
}

//void TableColumn::SetColumnAlignFromProp(AlignType /*nAlignOld*/)
//{
//	m_pTotalCell->SetCellAlign(m_nAlignType);
//
//	if (IsMultiRow())
//		m_nAlignType = SET_ALIGN_FOR_MULTI_ROW(m_nAlignType);
//
//	for (int nRow = 0; nRow <= m_Cells.GetUpperBound(); nRow++)
//		SetCellAlign(m_nAlignType, nRow);
//}

//------------------------------------------------------------------------------
void TableColumn::SetColumnFontIdx (FontIdx nIdx)
{
	m_pFontStyle = NULL;

	for (int nRow = 0; nRow <= m_Cells.GetUpperBound(); nRow++)
		m_Cells[nRow]->SetCellFontIdx (nIdx);

	m_nFontIdx = nIdx;

	SetFieldWidth();
}

//---------------------------------------------------------------------------
void TableColumn::SetColumnCellsColor (COLORREF* pColors)
{
	// set Borders for all column m_Cells
	m_ColumnPen.SetColor (pColors[CColumnTitleColorsDlg::BORDER]);

	// set foregrond and background for all column m_Cells
	for (int nRow = 0; nRow <= m_Cells.GetUpperBound(); nRow++)
	{
		m_Cells[nRow]->SetCellColor(pColors);
	}
}

//---------------------------------------------------------------------------
void TableColumn::SetColumnTitleColor (COLORREF* pColors)
{
	SetColumnTitleTextColor 		(pColors[CColumnTitleColorsDlg::LABEL]);
	SetColumnTitleBkgColor			(pColors[CColumnTitleColorsDlg::BACKGROUND]);
	SetColumnTitleBorderColor		(pColors[CColumnTitleColorsDlg::BORDER]);
}


//---------------------------------------------------------------------------
void TableColumn::GetColumnTitleColor (COLORREF* pColors) const
{
	pColors[CColumnTitleColorsDlg::LABEL]		= m_Title.GetTextColor();
	pColors[CColumnTitleColorsDlg::BACKGROUND]	= m_Title.GetBkgColor();
	pColors[CColumnTitleColorsDlg::BORDER]		= m_ColumnTitlePen.GetColor();
}

//------------------------------------------------------------------------------
void TableColumn::AssignData (WORD wID, RDEManager* RDEmanager)
{
	// make appropriate pFormatter
	Formatter* pFormatter = GetFormatter();

	// manipulate incoming column total using foretold rectangle
	if (RDEmanager->IsColTotal())
	{
		m_pTotalCell->AssignData (RDEmanager);
		m_pTotalCell->Format (wID, pFormatter, m_pTable->m_pDocument);
		return;
	}

	if (m_pTable->m_nCurrentRow > m_Cells.GetUpperBound())
	{
		ASSERT(FALSE);
		CString msg = _TB("There is a table row number overflow. (Table ID:%d, row:%d)\nCheck Table's rows equal to Table's rows in Report/Tables section too") +
						_TB(", or avoid table nexline instructions into event FormFeed.Before");
		m_pTable->m_pDocument->Message
		(	cwsprintf
			(
				msg,
				m_wInternalID, m_pTable->m_nCurrentRow
			),
			MB_OK | MB_ICONEXCLAMATION
		);
		m_pTable->m_nCurrentRow  = 0;
	}

	m_Cells[m_pTable->m_nCurrentRow]->AssignData (RDEmanager);
	if (this->IsMultiRow() && (this->GetDataType() == DataType::String || this->GetDataType() == DataType::Text))
	{
		// it is a multirow, so it can contain only strings or bool.
		// String case.
		CStringFormatter oFormatter;
		m_Cells[m_pTable->m_nCurrentRow]->Format(wID, &oFormatter, m_pTable->m_pDocument);
	}
	else
	{
		m_Cells[m_pTable->m_nCurrentRow]->Format(wID, pFormatter, m_pTable->m_pDocument);
	}
}

//------------------------------------------------------------------------------
BOOL TableColumn::ExecCommand (WORD wID, RDEManager* RDEmanager)
{
	if (m_wInternalID != wID) return FALSE;

	// command logic for current object
	RDEmanager->GetCommand();

	// ignore data
	RDEmanager->SkipData();
	return TRUE;
}

//------------------------------------------------------------------------------
void TableColumn::DisableData ()
{
	for (int nRow = 0; nRow <= m_Cells.GetUpperBound(); nRow++)
		m_Cells[nRow]->DisableData();

	m_pTotalCell->DisableData();

	// invalidate all column changed areas
	(m_pTable->m_pDocument)->InvalidateRect(m_ColumnCellsRect, FALSE);
	(m_pTable->m_pDocument)->InvalidateRect(TotalRect(), FALSE);
}

//------------------------------------------------------------------------------
BOOL TableColumn::DeleteEditorEntry()
{
	if (m_wInternalID)
	{
		// set m_AnchorColumnID = 0 for each BaseRect anchored
		for (int i = 0; i < m_arAnchoredFields.GetSize(); i++)
		{
			BaseRect* pBaseRect = m_arAnchoredFields.GetAt(i);
			if (m_wInternalID == pBaseRect->m_AnchorLeftColumnID)
				pBaseRect->m_AnchorLeftColumnID = 0;
			else if (m_wInternalID == pBaseRect->m_AnchorRightColumnID)
				pBaseRect->m_AnchorRightColumnID = 0;
		}
		
		if (m_pTable->m_pDocument->m_Layouts.ExistsFieldID(m_wInternalID, TRUE))
			return TRUE;

		return m_pTable->m_pDocument->m_pEditorManager->DeleteField(m_wInternalID);
	}	
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL TableColumn::AddAnchoredField	(BaseRect* pRect)
{
	for (int i = 0; i < m_arAnchoredFields.GetUpperBound(); i++)
	{
		BaseRect* pBase = m_arAnchoredFields.GetAt(i);
		if (*pBase == *pRect)
			return FALSE;
	}
	m_arAnchoredFields.Add(pRect);
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL TableColumn::RemoveAnchoredField	(BaseRect* pRect)
{
	for (int i = 0; i < m_arAnchoredFields.GetSize(); i++ )
	{
		if (*m_arAnchoredFields.GetAt(i) == *pRect)
		{
			m_arAnchoredFields.RemoveAt(i);
			return TRUE;
		}
	}
	return FALSE;
}

//	must rebuild:
//
//		m_ColumnTitleRect
//		m_ColumnCellsRect
//		m_ColumnRect
//
//------------------------------------------------------------------------------
void TableColumn::Unparse (ViewUnparser& ofile)
{
	FormatStyleTable* pFormatStyles = m_pTable->m_pDocument->m_pFormatStyles;
	BOOL bHidden = m_bIsHidden || m_ColumnTitleRect.Width() <= 0 || (m_pHideExpr && !m_pHideExpr->IsEmpty());

	if (m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseString	(ofile.IsLocalizableTextInCurrentLanguage() ? ofile.LoadReportString(m_Title.GetText()) : m_Title.GetText(), FALSE);
	}
	else
	{
		if (m_pTitleExpr == NULL)
			ofile.UnparseString	(ofile.IsLocalizableTextInCurrentLanguage() ? ofile.LoadReportString(m_Title.GetText()) : m_Title.GetText(), FALSE);
	
		if (
			(m_pTitleExpr			!= NULL && !m_pTitleExpr->IsEmpty()) ||
			(m_pTitleTextColorExpr	!= NULL && !m_pTitleTextColorExpr->IsEmpty()) ||
			(m_pTitleBkgColorExpr	!= NULL && !m_pTitleBkgColorExpr->IsEmpty()) ||
			(m_pTitleTooltipExpr	!= NULL && !m_pTitleTooltipExpr->IsEmpty())
		)
		{
			ofile.UnparseTag(T_TITLE, TRUE);
			ofile.UnparseBegin();

			if (m_pTitleExpr != NULL && !m_pTitleExpr->IsEmpty())
			{
				ofile.UnparseTag(T_TEXT, FALSE);
				ofile.UnparseTag(T_ASSIGN, FALSE);

				ofile.UnparseExpr(m_pTitleExpr->ToString(), FALSE);

				ofile.UnparseSep(TRUE);
			}
			if (m_pTitleTextColorExpr != NULL && !m_pTitleTextColorExpr->IsEmpty())
			{
				ofile.UnparseTag(T_TEXTCOLOR, FALSE);
				ofile.UnparseTag(T_ASSIGN, FALSE);

				ofile.UnparseExpr(m_pTitleTextColorExpr->ToString(), FALSE);

				ofile.UnparseSep(TRUE);
			}
			if (m_pTitleBkgColorExpr != NULL && !m_pTitleBkgColorExpr->IsEmpty())
			{
				ofile.UnparseTag(T_BKGCOLOR, FALSE);
				ofile.UnparseTag(T_ASSIGN, FALSE);

				ofile.UnparseExpr(m_pTitleBkgColorExpr->ToString(), FALSE);

				ofile.UnparseSep(TRUE);
			}
			if (m_pTitleTooltipExpr != NULL && !m_pTitleTooltipExpr->IsEmpty())
			{
				ofile.UnparseTag(T_TOOLTIP, FALSE);
				ofile.UnparseTag(T_ASSIGN, FALSE);

				ofile.UnparseExpr(m_pTitleTooltipExpr->ToString(), FALSE);

				ofile.UnparseSep(TRUE);
			}
			ofile.UnparseEnd(TRUE);
		}
	}
	ofile.UnparseAlias (m_wInternalID, FALSE);
	ofile.Write (_T(" /* ") + GetFieldName() +  _T(" */ "), FALSE);

	if (!m_sStyleClass.IsEmpty())
	{
		ofile.UnparseTag (T_STYLE, FALSE);
		ofile.UnparseString(m_sStyleClass, FALSE);
	}
	if (m_bTemplate)
		ofile.UnparseTag (T_TEMPLATE, FALSE);

	if (m_pTextColorExpr && !m_pTextColorExpr->IsEmpty() && !m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_TEXTCOLOR, FALSE);
		ofile.UnparseTag(T_ASSIGN, FALSE);

		ofile.UnparseExpr(m_pTextColorExpr->ToString(), FALSE);
	}
	if (m_pBkgColorExpr && !m_pBkgColorExpr->IsEmpty() && !m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_BKGCOLOR, FALSE);
		ofile.UnparseTag(T_ASSIGN, FALSE);

		ofile.UnparseExpr(m_pBkgColorExpr->ToString(), FALSE);
	}
	if (m_pTextFontExpr && !m_pTextFontExpr->IsEmpty() && !m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_FONTSTYLE, FALSE);
		ofile.UnparseTag(T_ASSIGN, FALSE);

		ofile.UnparseExpr(m_pTextFontExpr->ToString(), FALSE);
	}
	if (m_pCellBordersExpr && !m_pCellBordersExpr->IsEmpty() && !m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_BORDERS, FALSE);
		ofile.UnparseTag(T_ASSIGN, FALSE);

		ofile.UnparseExpr(m_pCellBordersExpr->ToString(), FALSE);
	}
	if (m_pCellTooltipExpr && !m_pCellTooltipExpr->IsEmpty() && !m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_TOOLTIP, FALSE);
		ofile.UnparseTag(T_ASSIGN, FALSE);

		ofile.UnparseExpr(m_pCellTooltipExpr->ToString(), FALSE);
	}

	//WIDTH
	int w = bHidden ? 0 : m_nWidth;
	if (m_pHideExpr && !m_pHideExpr->IsEmpty() && !m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
		w = max(m_nSavedWidth, m_nWidth);

	ofile.UnparseWidth	(w, FALSE);
	if (m_pDynamicWidthExpr != NULL && !m_pDynamicWidthExpr->IsEmpty() && !m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_COMMA, FALSE);

		ofile.UnparseExpr(m_pDynamicWidthExpr->ToString(), FALSE);
	}

	if (m_pCellFormatterExpr && !m_pCellFormatterExpr->IsEmpty() && !m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_FORMATSTYLE, FALSE);
		ofile.UnparseTag(T_ASSIGN, FALSE);

		ofile.UnparseExpr(m_pCellFormatterExpr->ToString(), FALSE);
	}
	else
		ofile.UnparseFormat	(pFormatStyles, m_nFormatIdx,				FALSE);

	if (m_bMultipleRow)
		ofile.UnparseTag(T_BREAK, FALSE);
	if (m_bVMergeEmptyCell)
		ofile.UnparseTag(T_VMERGE_EMPTY_CELL, FALSE);
	if (m_bVMergeEqualCell)
		ofile.UnparseTag(T_VMERGE_EQUAL_CELL, FALSE);
	if (m_bVMergeTailCell)
		ofile.UnparseTag(T_VMERGE_TAIL_CELL, FALSE);
	if (m_bMiniHtml)
		ofile.UnparseTag(T_HTML, FALSE);

	if (m_bShowTotal)
	{
		ofile.UnparseTag(T_TOTAL, FALSE);

		if ( 
			(
				(m_pTotalTextColorExpr != NULL && !m_pTotalTextColorExpr->IsEmpty())
			||
				(m_pTotalBkgColorExpr != NULL && !m_pTotalBkgColorExpr->IsEmpty())
			)  &&
			! m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
		{
			ofile.UnparseCrLf();
			ofile.UnparseBegin();
	
			if (m_pTotalTextColorExpr != NULL && !m_pTotalTextColorExpr->IsEmpty())
			{
				ofile.UnparseTag(T_TEXTCOLOR, FALSE);
				ofile.UnparseTag(T_ASSIGN, FALSE);

				ofile.UnparseExpr(m_pTotalTextColorExpr->ToString(), FALSE);
				ofile.UnparseSep(TRUE);
			}
			if (m_pTotalBkgColorExpr != NULL && !m_pTotalBkgColorExpr->IsEmpty())
			{
				ofile.UnparseTag(T_BKGCOLOR, FALSE);
				ofile.UnparseTag(T_ASSIGN, FALSE);

				ofile.UnparseExpr(m_pTotalBkgColorExpr->ToString(), FALSE);
				ofile.UnparseSep(TRUE);
			}
			ofile.UnparseEnd();
		}
	}

	if ( 
		(
			(m_SubTotal.m_pTextColorExpr != NULL && !m_SubTotal.m_pTextColorExpr->IsEmpty())
			 || 
			(m_SubTotal.m_pBkgColorExpr != NULL && !m_SubTotal.m_pBkgColorExpr->IsEmpty())
		)  && ! m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
	{
		ofile.UnparseTag(T_SUBTOTAL, TRUE);
		ofile.UnparseBegin();

		if (m_SubTotal.m_pTextColorExpr != NULL && !m_SubTotal.m_pTextColorExpr->IsEmpty())
		{
			ofile.UnparseTag(T_TEXTCOLOR, FALSE);
			ofile.UnparseTag(T_ASSIGN, FALSE);

			ofile.UnparseExpr(m_SubTotal.m_pTextColorExpr->ToString(), FALSE);
			ofile.UnparseSep(TRUE);
		}
		if (m_SubTotal.m_pBkgColorExpr != NULL && !m_SubTotal.m_pBkgColorExpr->IsEmpty())
		{
			ofile.UnparseTag(T_BKGCOLOR, FALSE);
			ofile.UnparseTag(T_ASSIGN, FALSE);

			ofile.UnparseExpr(m_SubTotal.m_pBkgColorExpr->ToString(), FALSE);
			ofile.UnparseSep(TRUE);
		}
		ofile.UnparseEnd();
	}

	if (m_ShowAs == EFieldShowAs::FT_IMAGE && m_pBitmap)
	{
		ofile.UnparseTag(T_BITMAP, FALSE);

		if ( m_pBitmap->IsBestFitSetted())
			ofile.UnparseTag(T_PROPORTIONAL, FALSE );
		else if (m_pBitmap->m_ImageFitMode == CTBPicture::ImageFitMode::NORMAL)
		{
			ofile.UnparseTag(T_NATIVE, FALSE);
		}
	}
	else if (m_ShowAs == EFieldShowAs::FT_BARCODE && m_pBarCode)
		ofile.UnparseBarCode(m_pBarCode, FALSE);
	else if (m_ShowAs == EFieldShowAs::FT_TEXTFILE)
		ofile.UnparseTag(T_FILE, FALSE);

	if (bHidden)
	{
		ofile.UnparseTag	(T_HIDDEN, FALSE);
		ASSERT(m_nSavedWidth);
		ofile.UnparseWidth	(m_nSavedWidth > 0 ? m_nSavedWidth : HIDDEN_DEFAULT_WIDTH, FALSE);
		if (m_pHideExpr && !m_pHideExpr->IsEmpty() && !m_pTable->m_pDocument->m_Template.m_bIsSavingTemplate)
		{
			ofile.UnparseTag(T_WHEN, FALSE);

			if (m_pTable->m_pDocument->m_bReplaceHiddenWhenExpr)
			{
				ofile.UnparseTag (m_bIsHidden ? T_TRUE : T_FALSE);
			}
			else
			{
				ofile.UnparseExpr(m_pHideExpr->ToString(), FALSE);
			}
		}
	} 

	//unparsing printer attributes
	if (m_bPinnedColumn)
		ofile.UnparseTag(T_COLUMN_FIXED, FALSE);

	if (m_bSplitterColumn)
		ofile.UnparseTag(T_COLUMN_SPLITTER, FALSE);
	
	if (m_bAnchorColumnLeft)
		ofile.UnparseTag(T_COLUMN_ANCHOR_LEFT, FALSE);
	
	if (m_bAnchorColumnRight)
		ofile.UnparseTag(T_COLUMN_ANCHOR_RIGHT, FALSE);
	
	if (m_bHideWhenEmpty)
		ofile.UnparseTag(T_COLUMN_HIDE_WHEN_EMPTY, FALSE);
	
	if (m_bOptimizeWidth)
		ofile.UnparseTag(T_COLUMN_OPTIMIZE_WIDTH, FALSE);

	ofile.UnparseSep	();
	ofile.UnparseCrLf	();
}

//------------------------------------------------------------------------------
BOOL TableColumn::Parse (ViewParser& lex)
{
	CString				sTitle;
	FormatStyleTable*	pFormatStyles = m_pTable->m_pDocument->m_pFormatStyles;
	
	WoormTable* pST = &m_pTable->m_pDocument->m_ViewSymbolTable;

	if (lex.LookAhead(T_STR) && !lex.ParseString (sTitle))
		return FALSE;

	if (lex.Matched(T_TITLE))
	{
		lex.Match(T_BEGIN);
		if (lex.Matched(T_TEXT))
		{
			lex.Match(T_ASSIGN);

			m_pTitleExpr = new Expression(pST);
			m_pTitleExpr->SetStopTokens(T_SEP);
			if (!m_pTitleExpr->Parse(lex, DataType::String, TRUE))
			{
				lex.SetError(_TB("Parser error in column title expression:") + _T("\r\n")/* + sTitle*/);
				return FALSE;
			}
			lex.Match(T_SEP);
		}
		if (lex.Matched(T_TEXTCOLOR))
		{
			lex.Match(T_ASSIGN);

			m_pTitleTextColorExpr = new Expression(pST);
			m_pTitleTextColorExpr->SetStopTokens(T_SEP);
			if (!m_pTitleTextColorExpr->Parse(lex, DataType::Long, TRUE))
			{
				lex.SetError(_TB("Parser error in column conditional text color expression:") + _T("\r\n")/* + sTitle*/);
				return FALSE;
			}
			lex.Match(T_SEP);
		}
		if (lex.Matched(T_BKGCOLOR))
		{
			lex.Match(T_ASSIGN);

			m_pTitleBkgColorExpr = new Expression(pST);
			m_pTitleBkgColorExpr->SetStopTokens(T_SEP);
			if (!m_pTitleBkgColorExpr->Parse(lex, DataType::Long, TRUE))
			{
				lex.SetError(_TB("Parser error in column title conditional background color expression:") + _T("\r\n")/* + sTitle*/);
				return FALSE;
			}
			lex.Match(T_SEP);
		}
		if (lex.Matched(T_TOOLTIP))
		{
			lex.Match(T_ASSIGN);

			m_pTitleTooltipExpr = new Expression(pST);
			m_pTitleTooltipExpr->SetStopTokens(T_SEP);
			if (!m_pTitleTooltipExpr->Parse(lex, DataType::String, TRUE))
			{
				lex.SetError(_TB("Parser error in column title tooltip expression:") + _T("\r\n")/* + sTitle*/);
				return FALSE;
			}
			lex.Match(T_SEP);
		}
		lex.Match(T_END);
	}

	if (!lex.ParseAlias	(m_wInternalID))
		return FALSE;

	if (lex.Matched(T_STYLE))
	{
		if (!lex.ParseString(m_sStyleClass))
			return FALSE;
		m_sStyleClass.Trim();
	}
	if (lex.Matched(T_TEMPLATE))
	{
		m_bTemplate = TRUE;
	}

	if (lex.Matched(T_TEXTCOLOR))
	{
		lex.Match(T_ASSIGN);

		m_pTextColorExpr = new Expression(pST);
		m_pTextColorExpr->SetStopTokens(T_BKGCOLOR, T_FONTSTYLE, T_BORDERS, T_TOOLTIP, T_WIDTH);
		if (!m_pTextColorExpr->Parse(lex, DataType::Long, TRUE))
		{
			lex.SetError(_TB("Parser error in column conditional text color expression:") + _T("\r\n")/* + sTitle*/);
			return FALSE;
		}
	}
	if (lex.Matched(T_BKGCOLOR))
	{
		lex.Match(T_ASSIGN);

		m_pBkgColorExpr = new Expression(pST);
		m_pBkgColorExpr->SetStopTokens(T_FONTSTYLE, T_BORDERS, T_TOOLTIP, T_WIDTH);
		if (!m_pBkgColorExpr->Parse(lex, DataType::Long, TRUE))
		{
			lex.SetError(_TB("Parser error in column conditional background color expression:") + _T("\r\n")/* + sTitle*/);
			return FALSE;
		}
	}
	if (lex.Matched(T_FONTSTYLE))
	{
		lex.Match(T_ASSIGN);

		m_pTextFontExpr = new Expression(pST);
		m_pTextFontExpr->SetStopTokens(T_BORDERS, T_TOOLTIP, T_WIDTH);
		if (!m_pTextFontExpr->Parse(lex, DataType::String, TRUE))
		{
			lex.SetError(_TB("Parser error in column conditional text font style expression:") + _T("\r\n")/* + sTitle*/);
			return FALSE;
		}
	}
	if (lex.Matched(T_BORDERS))
	{
		lex.Match(T_ASSIGN);

		m_pCellBordersExpr = new Expression(pST);
		m_pCellBordersExpr->SetStopTokens(T_TOOLTIP, T_WIDTH);
		if (!m_pCellBordersExpr->Parse(lex, DataType::String, TRUE))
		{
			lex.SetError(_TB("Parser error in cell conditional borders expression:") + _T("\r\n")/* + sTitle*/);
			return FALSE;
		}
	}
	if (lex.Matched(T_TOOLTIP))
	{
		lex.Match(T_ASSIGN);

		m_pCellTooltipExpr = new Expression(pST);
		m_pCellTooltipExpr->SetStopTokens(T_WIDTH);
		if (!m_pCellTooltipExpr->Parse(lex, DataType::String, TRUE))
		{
			lex.SetError(_TB("Parser error in cell tooltip expression:") + _T("\r\n")/* + sTitle*/);
			return FALSE;
		}
	}

	//Parse T_WIDTH
	if (!lex.ParseWidth(m_nWidth))
		return FALSE;
	if (m_nWidth < 0) m_nWidth = 0;
	int nCurWidth = m_nWidth;
	m_nSavedWidth = m_nWidth;
	if (m_nWidth == 0)
	{
		m_bIsHidden = TRUE;
		m_nWidth = m_nSavedWidth = HIDDEN_DEFAULT_WIDTH;
	}
	if (lex.Matched	(T_COMMA))
	{
		m_pDynamicWidthExpr = new Expression(pST);
		Token arStopToken[] =
		{
			T_FORMATSTYLE, T_HIDDEN, T_SEP, T_BREAK,
			T_TOTAL, T_SUBTOTAL,
			T_BITMAP, T_BARCODE_STRIP, T_FILE,
			T_VMERGE_EMPTY_CELL, T_VMERGE_EQUAL_CELL, T_VMERGE_TAIL_CELL,
			T_COLUMN_FIXED, T_COLUMN_SPLITTER,
			T_COLUMN_ANCHOR_LEFT, T_COLUMN_ANCHOR_RIGHT, T_COLUMN_HIDE_WHEN_EMPTY, T_COLUMN_OPTIMIZE_WIDTH, T_HTML
		};
		m_pDynamicWidthExpr->SetStopTokens(arStopToken, sizeof(arStopToken) / sizeof(Token));
		if (!m_pDynamicWidthExpr->Parse(lex, DataType::Integer, TRUE))
		{
			lex.SetError(_TB("Parser error in dynamic width expression:") + _T("\r\n")/* + sTitle*/);
			return FALSE;
		}
		if (!m_pDynamicWidthExpr->HasFields())
		{
			DataInt diWidth;
			if (m_pDynamicWidthExpr->Eval(diWidth))
			{
				nCurWidth = diWidth;
			}
		}
	}

	if (lex.Matched(T_FORMATSTYLE))
	{
		if (lex.Matched(T_ASSIGN))
		{
			m_pCellFormatterExpr = new Expression(pST);
			Token arStopToken[] = 
			{ 
				T_HIDDEN, T_SEP, T_BREAK, 
				T_TOTAL, T_SUBTOTAL, 
				T_BITMAP, T_BARCODE_STRIP, T_FILE, 
				T_VMERGE_EMPTY_CELL, T_VMERGE_EQUAL_CELL, T_VMERGE_TAIL_CELL,
				T_COLUMN_FIXED, T_COLUMN_SPLITTER, 
				T_COLUMN_ANCHOR_LEFT, T_COLUMN_ANCHOR_RIGHT, T_COLUMN_HIDE_WHEN_EMPTY, T_COLUMN_OPTIMIZE_WIDTH, T_HTML
			};
			m_pCellFormatterExpr->SetStopTokens(arStopToken, sizeof(arStopToken) / sizeof(Token));
			if (!m_pCellFormatterExpr->Parse(lex, DataType::String, TRUE))
			{
				lex.SetError(_TB("Parser error in column conditional formatter style expression:") + _T("\r\n")/* + sTitle*/);
				return FALSE;
			}
		}
		else if (!lex.ParseFormat (pFormatStyles, m_nFormatIdx, FALSE))
			return FALSE;
	}

	if (m_nFormatIdx == NO_FORMAT || m_pCellFormatterExpr)
	{
		SymField* pF = pST->GetFieldByID(m_wInternalID);
		if (pF)
			SetColumnFormatIdx(m_pTable->m_pDocument->m_pFormatStyles->GetFormatIdx(pF->GetDataType()));
		else
			SetColumnFormatIdx(0);
	}

	// abilita il word break su celle troppo lunghe
	if (lex.Matched(T_BREAK))
	{
		m_bMultipleRow = TRUE;
	}

	// In presenza di RowSep sulla tabella, non visualizza il bordo di separazione orizzontale se la riga successiva � vuota
	if (lex.Matched(T_VMERGE_EMPTY_CELL))
	{
		m_bVMergeEmptyCell = TRUE;
	}
	if (lex.Matched(T_VMERGE_EQUAL_CELL))
	{
		m_bVMergeEqualCell = TRUE;
	}
	if (lex.Matched(T_VMERGE_TAIL_CELL))
	{
		m_bVMergeTailCell = TRUE;
	}
	if (lex.Matched(T_HTML))
	{
		m_bMiniHtml = TRUE;
	}

	// flag total visibile or not
	if (lex.Matched(T_TOTAL))
	{
		m_bShowTotal = TRUE;
		if (lex.Matched(T_BEGIN))
		{
			if (lex.Matched(T_TEXTCOLOR))
			{
				lex.Match(T_ASSIGN);

				m_pTotalTextColorExpr = new Expression(pST);
				m_pTotalTextColorExpr->SetStopTokens(T_SEP);
				if (!m_pTotalTextColorExpr->Parse(lex, DataType::Long, TRUE))
				{
					lex.SetError(_TB("Parser error in column conditional total text color expression:") + _T("\r\n")/* + sTitle*/);
					return FALSE;
				}
				lex.Match(T_SEP);
			}
			if (lex.Matched(T_BKGCOLOR))
			{
				lex.Match(T_ASSIGN);

				m_pTotalBkgColorExpr = new Expression(pST);
				m_pTotalBkgColorExpr->SetStopTokens(T_SEP);
				if (!m_pTotalBkgColorExpr->Parse(lex, DataType::Long, TRUE))
				{
					lex.SetError(_TB("Parser error in column conditional total background color expression:") + _T("\r\n")/* + sTitle*/);
					return FALSE;
				}
				lex.Match(T_SEP);
			}
			lex.Match(T_END);
		}
	}

	// subtotal conditional coloring
	if (lex.Matched(T_SUBTOTAL))
	{
		if (lex.Matched(T_BEGIN))
		{
			if (lex.Matched(T_TEXTCOLOR))
			{
				lex.Match(T_ASSIGN);

				m_SubTotal.m_pTextColorExpr = new Expression(pST);
				m_SubTotal.m_pTextColorExpr->SetStopTokens(T_SEP);
				if (!m_SubTotal.m_pTextColorExpr->Parse(lex, DataType::Long, TRUE))
				{
					lex.SetError(_TB("Parser error in column conditional total text color expression:") + _T("\r\n")/* + sTitle*/);
					return FALSE;
				}
				lex.Match(T_SEP);
			}
			if (lex.Matched(T_BKGCOLOR))
			{
				lex.Match(T_ASSIGN);

				m_SubTotal.m_pBkgColorExpr = new Expression(pST);
				m_SubTotal.m_pBkgColorExpr->SetStopTokens(T_SEP);
				if (!m_SubTotal.m_pBkgColorExpr->Parse(lex, DataType::Long, TRUE))
				{
					lex.SetError(_TB("Parser error in column conditional total background color expression:") + _T("\r\n")/* + sTitle*/);
					return FALSE;
				}
				lex.Match(T_SEP);
			}
			lex.Match(T_END);
		}
	}

	m_ShowAs = EFieldShowAs::FT_NORMAL;

	// treat data as m_pBitmap name not as normal data
	if (lex.Matched(T_BITMAP))
	{
		m_ShowAs = EFieldShowAs::FT_IMAGE;

		m_pBitmap = new CTBPicture();

		if ( lex.Matched(T_PROPORTIONAL) )
		{
			m_pBitmap->m_ImageFitMode = CTBPicture::ImageFitMode::BEST;
		}
		else if (lex.Matched(T_NATIVE))
		{
			m_pBitmap->m_ImageFitMode = CTBPicture::ImageFitMode::NORMAL;
		}
	}

	// treat data as m_pBarCode name not as normal data
	if (lex.LookAhead(T_BARCODE_STRIP))
	{
		m_ShowAs = EFieldShowAs::FT_BARCODE;

		m_pBarCode = new CBarCode();
		
		if (!lex.ParseBarCode(m_pBarCode))
			return FALSE;
	}

	if (lex.Matched(T_FILE))
	{
		m_ShowAs = EFieldShowAs::FT_TEXTFILE;
	}

	//Boolean used to remove splitter on hidden always column 
	BOOL bRemoveSplitter = FALSE;
	BOOL bHiddenParsed = FALSE;
	if (lex.Matched(T_HIDDEN))
	{
		bHiddenParsed = TRUE;
		bRemoveSplitter = TRUE;
		m_bIsHidden = TRUE;

		if (!lex.ParseWidth(m_nSavedWidth))
			return FALSE;
		// nel caso la width salvata sia sballata o non settata setto il valore di default
		if (m_nSavedWidth <= 0) 
			m_nSavedWidth = HIDDEN_DEFAULT_WIDTH;
		//La width di default deve 
		if (m_nWidth)
			m_nWidth = m_nSavedWidth;
	
		if (lex.Matched(T_WHEN))
		{
			m_pHideExpr = new Expression(pST);
			Token arStopTokens [] = 
					{ 
						T_COLUMN_FIXED, 
						T_COLUMN_SPLITTER, 
						T_COLUMN_ANCHOR_LEFT, 
						T_COLUMN_ANCHOR_RIGHT,
						T_COLUMN_HIDE_WHEN_EMPTY,
						T_COLUMN_OPTIMIZE_WIDTH
					};
			
			m_pHideExpr->SetStopTokens(arStopTokens, sizeof(arStopTokens)/sizeof(Token));
			if (!m_pHideExpr->Parse(lex, DataType::Bool, TRUE))
			{
				lex.SetError(_TB("Parser error in control expression for column visualization:") + _T("\r\n") + sTitle);
				return FALSE;
			}

			//DataBool b(FALSE);
			//if (!m_pHideExpr->Eval(b))
			//{
			//	//TODO
			//	//lex.SetError(_TB("Evaluation error in control expression for column visualization:") + _T("\r\n") + sTitle);
			//	//return FALSE;
			//}
			//m_bIsHidden = b;
			bRemoveSplitter = FALSE;

			if (nCurWidth == 0 && !m_bIsHidden) 
				nCurWidth = m_nSavedWidth;
		}
	}
	else if (m_nWidth <= 0)
	{
		m_nWidth = 0;
		m_bIsHidden = TRUE;
		m_nSavedWidth = HIDDEN_DEFAULT_WIDTH;
	}

	//Parsing Column Printer Attributes
	if (lex.Matched(T_COLUMN_FIXED))
		m_bPinnedColumn = TRUE;

	if (lex.Matched(T_COLUMN_SPLITTER) && !bRemoveSplitter)
		m_bSplitterColumn = TRUE;

	if (lex.Matched(T_COLUMN_ANCHOR_LEFT))
		m_bAnchorColumnLeft = TRUE;
		
	if (lex.Matched(T_COLUMN_ANCHOR_RIGHT))
		m_bAnchorColumnRight = TRUE;
		
	if (lex.Matched(T_COLUMN_HIDE_WHEN_EMPTY))
		m_bHideWhenEmpty = TRUE;
		
	if (lex.Matched(T_COLUMN_OPTIMIZE_WIDTH))
		m_bOptimizeWidth = TRUE;
		
	// Format and change alignment to all column m_Cells, total and subtotal using
	// indicated style in Format styles m_Table
	//
	SetDefaultAlign		(pFormatStyles->GetDataType(m_nFormatIdx));
	SetDefaultFontIdx	(pFormatStyles->GetDataType(m_nFormatIdx));

	// must use SetText for setting real text in column
	m_Title.SetText(sTitle);  

	// update counter of bLast id for create new object with unique ID
	m_pTable->m_pDocument->m_pEditorManager->SetLastId(m_wInternalID);

	InitializeWidth(m_bIsHidden ? 0 : nCurWidth);

	// count total
	if (m_bShowTotal) 
		m_pTable->m_nTotalCounter++;

	return lex.ParseSep();
}

//------------------------------------------------------------------------------
void TableColumn::ResizeCells
	(
		CPoint			ptOrigin,
		CSize			size
	)
{
	for (int nRow = 0; nRow <= LastRow(); nRow++)
	{
		TableCell* cell= m_Cells[nRow];
		cell->SetCellRect(CRect(ptOrigin, size));
		ptOrigin.y += size.cy;
	}

}

//------------------------------------------------------------------------------
void TableColumn::ToggleBitmap ()
{
	if (m_ShowAs == EFieldShowAs::FT_IMAGE)
	{
		SAFE_DELETE(m_pBitmap);

		m_ShowAs = EFieldShowAs::FT_NORMAL;
	}
	else
	{
		SAFE_DELETE(m_pBarCode);

		m_pBitmap = new CTBPicture();

		m_ShowAs = EFieldShowAs::FT_IMAGE;
	}
	UpdateDocument();
}

//------------------------------------------------------------------------------
void TableColumn::ToggleBarCode ()
{
	if (m_ShowAs == EFieldShowAs::FT_BARCODE)
	{
		SAFE_DELETE(m_pBarCode);

		m_ShowAs = EFieldShowAs::FT_NORMAL;
	}
	else
	{
		SAFE_DELETE(m_pBitmap);

		m_pBarCode = new CBarCode();

		m_ShowAs = EFieldShowAs::FT_BARCODE;
	}

	UpdateDocument();
}

//------------------------------------------------------------------------------
void TableColumn::ToggleTextFile ()
{
	if (m_ShowAs == EFieldShowAs::FT_TEXTFILE)
	{
		m_ShowAs = EFieldShowAs::FT_NORMAL;
	}
	else
	{
		SAFE_DELETE(m_pBitmap);
		SAFE_DELETE(m_pBarCode);

		m_ShowAs = EFieldShowAs::FT_TEXTFILE;
	}

	UpdateDocument();
}


//metodo che imposta la colonna attiva come colonna splitter (cioe' spezza la tabella per
//mandarla in stampa) NOTA: Supportato solo se e' presente una sola tabella
//------------------------------------------------------------------------------
void TableColumn::ToggleSplitter()
{
	//Se ho piu di una tabella permetto di eliminare eventuali colonne splitter,
	//ma non di aggiungerne altre
	if (!m_bSplitterColumn && m_pTable->m_pDocument->GetTableCount() > 1)
	{
		AfxMessageBox(_TB("This command is not available in report containing more than one table"), MB_OK | MB_ICONEXCLAMATION);	
		return;
	}	

	m_bSplitterColumn = !m_bSplitterColumn;
	
	if (m_bSplitterColumn)  //I've just added this column as Splitter Column
		m_pTable->m_pDocument->m_PageInfo.m_arHPageSplitter.AddSorted(m_ColumnTitleRect.right);
	else                    //I've just removed this column as Splitter Column
		m_pTable->m_pDocument->m_PageInfo.m_arHPageSplitter.Remove(m_ColumnTitleRect.right);

	m_pTable->m_pDocument->Invalidate(TRUE);
}

//metodo che imposta la colonna attiva come colonna fissa (cioe' colonna che deve essere
//stampata in ogni pagina "splittata" del report) NOTA: Supportato solo se e' presente una sola tabella
//------------------------------------------------------------------------------
void TableColumn::ToggleFixed()
{
	//Se ho piu di una tabella permetto di eliminare eventuali colonne fisse,
	//ma non di aggiungerne altre
	if (!m_bPinnedColumn && m_pTable->m_pDocument->GetTableCount() > 1)
	{
		AfxMessageBox(_TB("This command is not available in report containing more than one table"), MB_OK | MB_ICONEXCLAMATION);	
		return;
	}
	m_bPinnedColumn = !m_bPinnedColumn;
}
//------------------------------------------------------------------------------
void TableColumn::ToggleAnchorLeft()
{
	m_bAnchorColumnLeft = !m_bAnchorColumnLeft;
}

//------------------------------------------------------------------------------
void TableColumn::ToggleAnchorRight()
{
	m_bAnchorColumnRight = !m_bAnchorColumnRight;
}

//------------------------------------------------------------------------------
void TableColumn::ToggleVMergeEmptyCell	()
{
	m_bVMergeEmptyCell = !m_bVMergeEmptyCell;
}

//------------------------------------------------------------------------------
void TableColumn::ToggleVMergeEqualCell	()
{
	m_bVMergeEqualCell = !m_bVMergeEqualCell;
}

//------------------------------------------------------------------------------
void TableColumn::ToggleVMergeTailCell	()
{
	m_bVMergeTailCell = !m_bVMergeTailCell;
}

//------------------------------------------------------------------------------
void TableColumn::ToggleHiddenWhenEmpy	()
{
	m_bHideWhenEmpty = !m_bHideWhenEmpty;
}

//------------------------------------------------------------------------------
void TableColumn::ToggleOptimizedWidth	()
{
	m_bOptimizeWidth = !m_bOptimizeWidth;
}

//------------------------------------------------------------------------------
int	TableColumn::SetColumnTitle	()
{
	CString strTitleCol = m_Title.GetText();
	CEditTextExprDlg dialog(strTitleCol, m_pTitleExpr, &(m_pTable->m_pDocument->m_ViewSymbolTable)); 
	if (dialog.DoModal() == IDOK)
	{
		m_Title.SetText(strTitleCol);
		return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
void TableColumn::GetSubTotalColor (COLORREF* pColors) const
{
	pColors[CSubTotalColorsDlg::VALUE]		= m_SubTotal.GetTextColor();
	pColors[CSubTotalColorsDlg::BACKGROUND]	= m_SubTotal.GetBkgColor();
}

//---------------------------------------------------------------------------
void TableColumn::GetCellColor (COLORREF* pColors, int nRow) const
{
	// read foregrond and background color from current cell
	m_Cells[nRow]->GetCellColor(pColors);

	// get color bord from current column pen
	pColors[CCellColorsDlg::BORDER] = m_ColumnPen.GetColor();
}

//---------------------------------------------------------------------------
void TableColumn::GetAllCellsTextColor(COLORREF& crTextColor) const
{
	crTextColor = m_Cells[0]->m_Value.m_rgbTextColor;//0 is default cell
}

//---------------------------------------------------------------------------
void TableColumn::GetAllCellsBkgColor(COLORREF& crBkgColor) const
{
	crBkgColor = m_Cells[0]->m_Value.m_rgbBkgColor;//0 is default cell
}

//---------------------------------------------------------------------------
COLORREF TableColumn::GetAllCellsTextColor() 
{
	return m_Cells[0]->m_Value.m_rgbTextColor;//0 is default cell
}

//---------------------------------------------------------------------------
COLORREF TableColumn::GetAllCellsBkgColor()
{
	return m_Cells[0]->m_Value.m_rgbBkgColor;//0 is default cell
}

//---------------------------------------------------------------------------
void TableColumn::SetAllCellsTextColor(COLORREF crTextColor)
{
	for (int nRow = 0; nRow < m_Cells.GetSize(); nRow++)
		m_Cells[nRow]->m_Value.m_rgbTextColor = crTextColor;

	if (m_pTable->m_pDocument->m_pWoormIni->m_bIncludeTotal)
		m_pTotalCell->m_Value.m_rgbTextColor = crTextColor;
}

//---------------------------------------------------------------------------
void TableColumn::SetAllCellsBkgColor(COLORREF crBkgColor)
{
	for (int nRow = 0; nRow < m_Cells.GetSize(); nRow++)
		m_Cells[nRow]->m_Value.m_rgbBkgColor = crBkgColor;

	if (m_pTable->m_pDocument->m_pWoormIni->m_bIncludeTotal)
		m_pTotalCell->m_Value.m_rgbBkgColor = crBkgColor;
}


//---------------------------------------------------------------------------
void TableColumn::SetSubTotalColor (COLORREF* pColors)
{
	m_SubTotal.SetTextColor	(pColors[CSubTotalColorsDlg::VALUE]);
	m_SubTotal.SetBkgColor	(pColors[CSubTotalColorsDlg::BACKGROUND]);
}

//------------------------------------------------------------------------------
void TableColumn::CopyClipboard ()
{
}
									  

//------------------------------------------------------------------------------
void TableColumn::VKReturn()
{
}

//------------------------------------------------------------------------------
void TableColumn::ModifyColumnTotal()
{
	m_pTable->m_pDocument->m_pEditorManager->ModColTotal(m_wInternalID);
}

//---------------------------------------------------------------------------
void TableColumn::ClearCellDynamicAttributes(BOOL bBrowsed /*= TRUE*/, BOOL bSelected /*= TRUE*/, BOOL bError/* = TRUE*/) 
{
	for (int i = 0; i <= m_Cells.GetUpperBound(); i++)
	{
		TableCell* pCell = m_Cells[i];

		if (pCell->ClearCellDynamicAttributes(bBrowsed, bSelected, bError))
		{
			m_pTable->m_pDocument->InvalidateRect(pCell->GetCellRect(), FALSE);
		}
	}
	if (m_pTotalCell)
	{
		if (m_pTotalCell->ClearCellDynamicAttributes(bBrowsed, bSelected, bError))
		{
			m_pTable->m_pDocument->InvalidateRect(m_pTotalCell->GetCellRect(), FALSE);
		}
	}
}

//---------------------------------------------------------------------------
void TableColumn::ResetCells() 
{
	for (int i = 0; i <= m_Cells.GetUpperBound(); i++)
	{
		m_Cells[i]->m_Value.m_RDEdata.ThisIsTailMultiLineString(FALSE);
	}
}

//------------------------------------------------------------------------------
Expression*& TableColumn::GetDynamicExpr(EnumDynamicExpr eObject)
{
	switch (eObject)	
	{
		case TextColorCell:
			return m_pTextColorExpr;

		case TextColorTitle:
			return m_pTitleTextColorExpr;

		case TextColorTotal:
			return m_pTotalTextColorExpr;

		case TextColorSubTotal:
			return m_SubTotal.m_pTextColorExpr;
		
		case BkgColorCell:
			return m_pBkgColorExpr;

		case BkgColorTitle:
			return m_pTitleBkgColorExpr;

		case BkgColorTotal:
			return m_pTotalBkgColorExpr;

		case BkgColorSubTotal:
			return m_SubTotal.m_pBkgColorExpr;

		case TitleExpr:
			return m_pTitleExpr;

		case TextFontExpr:
			return m_pTextFontExpr;

		case CellBordersExpr:
			return m_pCellBordersExpr;

		case CellTooltipExpr:
			return m_pCellTooltipExpr;

		case TitleTooltipExpr:
			return m_pTitleTooltipExpr;

		default: 
			ASSERT(FALSE);
			return m_pTextColorExpr; //NON posso tornare NULL
	}
}

//------------------------------------------------------------------------------
FontIdx TableColumn::GetCellDynamicFontIdx	() const
{
	if (!m_pTextFontExpr)
		return FNT_ERROR;

	DataStr sFontStyle;
	if (m_pTextFontExpr->Eval(sFontStyle))
	{
		ASSERT(m_pTable->m_pDocument->m_pFontStyles);
		return m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(sFontStyle);
	}

	return FNT_ERROR;
}

//------------------------------------------------------------------------------
FormatIdx TableColumn::GetCellDynamicFormatIdx	() const
{
	if (!m_pCellFormatterExpr)
		return FNT_ERROR;

	DataStr sFormatStyle;
	if (m_pCellFormatterExpr->Eval(sFormatStyle))
	{
		ASSERT(m_pTable->m_pDocument->m_pFormatStyles);
		return m_pTable->m_pDocument->m_pFormatStyles->GetFormatIdx(sFormatStyle);
	}

	return FNT_ERROR;
}

//------------------------------------------------------------------------------
Formatter* TableColumn::GetCellDynamicFormatter	() const
{
	if (!m_pCellFormatterExpr)
		return NULL;

	DataStr sFormatStyle;
	if (m_pCellFormatterExpr->Eval(sFormatStyle))
	{
		ASSERT(m_pTable->m_pDocument->m_pFormatStyles);
		Formatter* fmt = m_pTable->m_pDocument->m_pFormatStyles->GetFormatter(sFormatStyle, & m_pTable->m_pDocument->GetNamespace());
		return fmt;
	}

	return NULL;
}

//------------------------------------------------------------------------------
BOOL TableColumn::GetCellDynamicBorders	(Borders& b) const
{
	if (!m_pCellBordersExpr)
		return FALSE;

	DataStr s;
	if (m_pCellBordersExpr->Eval(s))
	{
		CString sB = s.GetString(); 

		b.Update(sB);

		return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
CString TableColumn::GetTitleTooltip () const
{
	DataStr sTip;
	if (m_pTitleTooltipExpr)
		m_pTitleTooltipExpr->Eval(sTip);

	return sTip;
}

//---------------------------------------------------------------------------
void TableColumn::InitializeWidth(int width)
{
	// IMPORTANT TRICKS
	// save width in m_Title coordinate (width is ok)
	m_ColumnTitleRect.left = 0;
	m_ColumnTitleRect.right = m_ColumnTitleRect.left + width;
}

int TableColumn::Width() const
{
	return m_ColumnTitleRect.Width();
}

//---------------------------------------------------------------------------
void TableColumn::CopyAttributes (CObject* pSource)
{
	if (pSource->IsKindOf(RUNTIME_CLASS(TextRect)))
	{
		TextRect* pSrc = (TextRect*) pSource;
		//-----
		this->SetColumnFontIdx	(pSrc->m_StaticText.m_nFontIdx);//SetColumnTitleFontIdx

		COLORREF colors[CCellColorsDlg::MAX];
			colors[CCellColorsDlg::VALUE]		=	pSrc->m_StaticText.m_rgbTextColor;
			colors[CCellColorsDlg::BACKGROUND]	=	pSrc->m_StaticText.m_rgbBkgColor;
			colors[CCellColorsDlg::BORDER]		=	pSrc->m_BorderPen.m_rgbColor != RGB(255,255,255) ?
													pSrc->m_BorderPen.m_rgbColor :
													this->m_ColumnPen.GetColor();
		this->SetColumnCellsColor (colors);
	}
	else if (pSource->IsKindOf(RUNTIME_CLASS(FieldRect)))
	{
		FieldRect* pSrc = (FieldRect*) pSource;
		//-----

		this->SetColumnFontIdx	(pSrc->m_Value.m_nFontIdx);

		COLORREF colors[CCellColorsDlg::MAX];
			colors[CCellColorsDlg::VALUE]		=	pSrc->m_Value.m_rgbTextColor;
			colors[CCellColorsDlg::BACKGROUND]	=	pSrc->m_Value.m_rgbBkgColor;
			colors[CCellColorsDlg::BORDER]		=	pSrc->m_BorderPen.m_rgbColor != RGB(255,255,255) ?
													pSrc->m_BorderPen.m_rgbColor :
													this->m_ColumnPen.GetColor();
		this->SetColumnCellsColor (colors);
		
		this->SetColumnAlign	(pSrc->m_Value.m_nAlign);

		if (this->GetDataType() == pSrc->GetDataType())
			this->SetColumnFormatIdx (pSrc->GetFormatIdx());
	}
	else if (pSource->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pSrc = (TableColumn*) pSource;
		//-----
		this->m_sStyleClass	= pSrc->m_sStyleClass;

		this->SetColumnFontIdx	(pSrc->GetColumnFontIdx());

		COLORREF colors[CCellColorsDlg::MAX];
		pSrc->GetCellColor(colors, 0);
		this->SetColumnCellsColor(colors);

		this->SetColumnAlign	(pSrc->GetColumnAlign());

		this->m_ColumnPen = pSrc->m_ColumnPen;

		if (this->GetDataType() == pSrc->GetDataType())
			this->SetColumnFormatIdx (pSrc->GetColumnFormatIdx());
	}
}

//---------------------------------------------------------------------------
void TableColumn::CopyHeaderAttributes	(CObject* pSource)
{
	if (pSource->IsKindOf(RUNTIME_CLASS(TextRect)))
	{
		TextRect* pSrc = (TextRect*) pSource;
		//-----

		this->SetColumnTitleFontIdx	(pSrc->m_StaticText.m_nFontIdx);

		COLORREF colors[CColumnTitleColorsDlg::MAX];
			colors[CColumnTitleColorsDlg::LABEL]		=	pSrc->m_StaticText.m_rgbTextColor;
			colors[CColumnTitleColorsDlg::BACKGROUND]	=	pSrc->m_StaticText.m_rgbBkgColor;
			colors[CColumnTitleColorsDlg::BORDER]		=	pSrc->m_BorderPen.m_rgbColor != RGB(255,255,255) ?
															pSrc->m_BorderPen.m_rgbColor :
															this->m_ColumnTitlePen.GetColor();
		this->SetColumnTitleColor (colors);

	}
	else if (pSource->IsKindOf(RUNTIME_CLASS(FieldRect)))
	{
		FieldRect* pSrc = (FieldRect*) pSource;
		//-----

		this->SetColumnTitleFontIdx	(pSrc->m_Value.m_nFontIdx);

		COLORREF colors[CColumnTitleColorsDlg::MAX];
			colors[CColumnTitleColorsDlg::LABEL]		=	pSrc->m_Value.m_rgbTextColor;
			colors[CColumnTitleColorsDlg::BACKGROUND]	=	pSrc->m_Value.m_rgbBkgColor;
			colors[CColumnTitleColorsDlg::BORDER]		=	pSrc->m_BorderPen.m_rgbColor != RGB(255,255,255) ?
															pSrc->m_BorderPen.m_rgbColor :
															this->m_ColumnTitlePen.GetColor();
		this->SetColumnTitleColor (colors);
		
		this->SetColumnTitleAlign	(pSrc->m_Value.m_nAlign);
	}
	else if (pSource->IsKindOf(RUNTIME_CLASS(TableColumn)))
	{
		TableColumn* pSrc = (TableColumn*) pSource;
		//-----
		this->m_sStyleClass	= pSrc->m_sStyleClass;

		this->SetColumnTitleFontIdx	(pSrc->GetColumnTitleFontIdx());

		COLORREF colors[CColumnTitleColorsDlg::MAX];
		pSrc->GetColumnTitleColor(colors);
		this->SetColumnTitleColor(colors);

		this->SetColumnTitleAlign	(pSrc->GetColumnTitleAlign());

		this->m_ColumnTitlePen = pSrc->m_ColumnTitlePen;
	}
}

//---------------------------------------------------------------------------
DataType TableColumn::GetDataType() const
{
	SymField* pField = m_pTable->m_pDocument->m_ViewSymbolTable.GetFieldByID(GetInternalID());
	if (pField)
	{
		return pField->GetDataType();
	}
	//ASSERT(FALSE);
	return DataType::Null;
}

//------------------------------------------------------------------------------
CString TableColumn::GetFieldName () const
{
	SymField* pField = m_pTable->m_pDocument->m_ViewSymbolTable.GetFieldByID(GetInternalID());
	if (pField)
	{
		return pField->GetName();
	}
	//ASSERT(FALSE); per i template che non hanno le variabili e' giusto che non trovi il field
	return _T("<UNKNOWN COLUMN FIELD>");
}

//------------------------------------------------------------------------------
CString TableColumn::GetDescription (BOOL bShowLabel/* = TRUE*/) const
{
	CString s; 
	if (bShowLabel)
	{
		s.Format(_T("[Col] %s (Id:%d) %s"), GetFieldName(), m_wInternalID, GetCaption());
		s.Replace(_T("\\n"), _T(" "));
	}
	else
		s.Format(_T("[Col] %s (Id:%d)"), GetFieldName(), m_wInternalID);
	return s;
}

//------------------------------------------------------------------------------
CString TableColumn::GetCaption() const
{
	return AfxLoadReportString(GetTitle(), m_pTable->m_pDocument);
}

//---------------------------------------------------------------------------
void	TableColumn::SetStyle (TableColumn* pTplCol)
{
	RemoveStyle ();
	if (!pTplCol) return;
	m_pDefault = pTplCol;
	m_sStyleClass = pTplCol->m_sStyleClass;
	//--- ----
	if (m_ColumnPen == BorderPen(m_pTable->m_pDocument->m_pDataDefaults->m_rgbColumnBorder))
		m_ColumnPen = m_pDefault->m_ColumnPen;

	if (m_ColumnTitlePen == BorderPen(m_pTable->m_pDocument->m_pDataDefaults->m_rgbColumnTitleBorder))
		m_ColumnTitlePen = m_pDefault->m_ColumnTitlePen;

	if (GetTotalPen() == BorderPen())
		SetTotalPen (m_pDefault->GetTotalPen());

	if (m_Title.GetTextColor() == DEFAULT_COLUMN_TITLE_FOREGROUND)
		m_Title.SetTextColor (m_pDefault->m_Title.GetTextColor());

	if (m_Title.GetBkgColor() == DEFAULT_COLUMN_TITLE_BACKGROUND)
		m_Title.SetBkgColor (m_pDefault->m_Title.GetBkgColor());

	BaseRect::SetTemplateFont (m_pTable->m_pDocument, m_Title.m_nFontIdx, m_pDefault->m_Title.m_nFontIdx, FNT_COLUMN_TITLE);

	if (m_Title.GetAlign () == DEFAULT_ALIGN)
		m_Title.SetAlign (m_pDefault->m_Title.GetAlign());

	LPCTSTR fnt_cell;
	LPCTSTR fnt_subtotal;
	LPCTSTR fnt_total;
	if (IsShowedAsString(GetDataType()))
	{
		fnt_cell = FNT_CELL_STRING;
		fnt_subtotal = FNT_SUBTOTAL_STRING;
		fnt_total = FNT_TOTAL_STRING;
	}
	else
	{
		fnt_cell = FNT_CELL_NUM;
		fnt_subtotal = FNT_SUBTOTAL_NUM;
		fnt_total = FNT_TOTAL_NUM;
	}

	FontIdx prevFontIdx (m_nFontIdx);
	BaseRect::SetTemplateFont (m_pTable->m_pDocument, m_nFontIdx,						m_pDefault->m_nFontIdx,							fnt_cell);
	if (prevFontIdx != m_nFontIdx)
		SetColumnFontIdx(m_nFontIdx);

	BaseRect::SetTemplateFont (m_pTable->m_pDocument, m_SubTotal.m_nFontIdx,				m_pDefault->m_SubTotal.m_nFontIdx,				fnt_subtotal);
		
	BaseRect::SetTemplateFont (m_pTable->m_pDocument, m_pTotalCell->m_Value.m_nFontIdx,	m_pDefault->m_pTotalCell->m_Value.m_nFontIdx,	fnt_total);

	//---- TODO/compromesso viene controllato solo il colore della prima cella
	COLORREF colors[CCellColorsDlg::MAX];
	m_Cells[0]->GetCellColor(colors);
	COLORREF defaultColors[CCellColorsDlg::MAX];
	m_pDefault->m_Cells[0]->GetCellColor(defaultColors);

	if (colors[CCellColorsDlg::VALUE] == DEFAULT_CELL_FOREGROUND)
		colors[CCellColorsDlg::VALUE] = defaultColors[CCellColorsDlg::VALUE];
	if (colors[CCellColorsDlg::BACKGROUND] == DEFAULT_CELL_BACKGROUND)
		colors[CCellColorsDlg::BACKGROUND] = defaultColors[CCellColorsDlg::BACKGROUND];
	
	colors[CCellColorsDlg::BORDER] = m_ColumnPen.m_rgbColor;
	SetColumnCellsColor(colors);
	//----
}

//---------------------------------------------------------------------------
void TableColumn::RemoveStyle ()
{
	if (!m_pDefault) return;
	//---- ----

	if (m_ColumnPen == m_pDefault->m_ColumnPen)
		m_ColumnPen = BorderPen(m_pTable->m_pDocument->m_pDataDefaults->m_rgbColumnBorder);

	if (m_ColumnTitlePen == m_pDefault->m_ColumnTitlePen)
		m_ColumnTitlePen = BorderPen(m_pTable->m_pDocument->m_pDataDefaults->m_rgbColumnTitleBorder);

	if (GetTotalPen() == m_pDefault->GetTotalPen())
		SetTotalPen (BorderPen());

	if (m_Title.GetTextColor() == m_pDefault->m_Title.GetTextColor())
		m_Title.SetTextColor (DEFAULT_COLUMN_TITLE_FOREGROUND);

	if (m_Title.GetBkgColor() == m_pDefault->m_Title.GetBkgColor())
		m_Title.SetBkgColor (DEFAULT_COLUMN_TITLE_BACKGROUND);

	BaseRect::RemoveTemplateFont (m_pTable->m_pDocument, m_Title.m_nFontIdx, m_pDefault->m_Title.m_nFontIdx, FNT_COLUMN_TITLE);

	if (m_Title.GetAlign () == m_pDefault->m_Title.GetAlign())
		m_Title.SetAlign (DEFAULT_ALIGN);

	LPCTSTR fnt_cell;
	LPCTSTR fnt_subtotal;
	LPCTSTR fnt_total;
	if (IsShowedAsString(GetDataType()))
	{
		fnt_cell = FNT_CELL_STRING;
		fnt_subtotal = FNT_SUBTOTAL_STRING;
		fnt_total = FNT_TOTAL_STRING;
	}
	else
	{
		fnt_cell = FNT_CELL_NUM;
		fnt_subtotal = FNT_SUBTOTAL_NUM;
		fnt_total = FNT_TOTAL_NUM;
	}

	BaseRect::RemoveTemplateFont (m_pTable->m_pDocument, m_nFontIdx,							m_pDefault->m_nFontIdx,							fnt_cell);

	BaseRect::RemoveTemplateFont (m_pTable->m_pDocument, m_SubTotal.m_nFontIdx,				m_pDefault->m_SubTotal.m_nFontIdx,				fnt_subtotal);
		
	BaseRect::RemoveTemplateFont (m_pTable->m_pDocument, m_pTotalCell->m_Value.m_nFontIdx,	m_pDefault->m_pTotalCell->m_Value.m_nFontIdx,	fnt_total);

	//----TODO/compromesso viene controllato solo il colore della prima cella
	COLORREF colors[CCellColorsDlg::MAX];
	m_Cells[0]->GetCellColor(colors);
	COLORREF defaultColors[CCellColorsDlg::MAX];
	m_pDefault->m_Cells[0]->GetCellColor(defaultColors);

	if (colors[CCellColorsDlg::VALUE] == defaultColors[CCellColorsDlg::VALUE])
		colors[CCellColorsDlg::VALUE] = DEFAULT_CELL_FOREGROUND;
	if (colors[CCellColorsDlg::BACKGROUND] == defaultColors[CCellColorsDlg::BACKGROUND])
		colors[CCellColorsDlg::BACKGROUND] = DEFAULT_CELL_BACKGROUND;
	
	colors[CCellColorsDlg::BORDER] = m_ColumnPen.m_rgbColor;
	SetColumnCellsColor(colors);
	//----

	//---- ----
	m_pDefault = NULL;
}

//---------------------------------------------------------------------------
void TableColumn::ClearStyle ()
{
	m_ColumnPen				= m_pDefault ? m_pDefault->m_ColumnPen			: BorderPen(m_pTable->m_pDocument->m_pDataDefaults->m_rgbColumnBorder);
	m_ColumnTitlePen		= m_pDefault ? m_pDefault->m_ColumnTitlePen		: BorderPen(m_pTable->m_pDocument->m_pDataDefaults->m_rgbColumnTitleBorder);
	SetTotalPen				(m_pDefault ? m_pDefault->GetTotalPen()	: BorderPen());

	m_Title.SetTextColor	(m_pDefault ? m_pDefault->m_Title.GetTextColor()	: DEFAULT_COLUMN_TITLE_FOREGROUND);
	m_Title.SetBkgColor		(m_pDefault ? m_pDefault->m_Title.GetBkgColor()		: DEFAULT_COLUMN_TITLE_BACKGROUND);
	
	BaseRect::ClearTemplateFont (m_pTable->m_pDocument, m_Title.m_nFontIdx, m_pDefault ? &(m_pDefault->m_Title.m_nFontIdx) : NULL, FNT_COLUMN_TITLE);
	
	m_Title.SetAlign		(m_pDefault ? m_pDefault->m_Title.GetAlign()		: DEFAULT_ALIGN);

	LPCTSTR fnt_cell;
	LPCTSTR fnt_subtotal;
	LPCTSTR fnt_total;
	if (IsShowedAsString(GetDataType()))
	{
		fnt_cell = FNT_CELL_STRING;
		fnt_subtotal = FNT_SUBTOTAL_STRING;
		fnt_total = FNT_TOTAL_STRING;
	}
	else
	{
		fnt_cell = FNT_CELL_NUM;
		fnt_subtotal = FNT_SUBTOTAL_NUM;
		fnt_total = FNT_TOTAL_NUM;
	}

	BaseRect::ClearTemplateFont (m_pTable->m_pDocument, m_nFontIdx,						 m_pDefault ? &(m_pDefault->m_nFontIdx) : NULL,							fnt_cell);

	BaseRect::ClearTemplateFont (m_pTable->m_pDocument, m_SubTotal.m_nFontIdx,			 m_pDefault ? &(m_pDefault->m_SubTotal.m_nFontIdx) : NULL,				fnt_subtotal);
		
	BaseRect::ClearTemplateFont (m_pTable->m_pDocument, m_pTotalCell->m_Value.m_nFontIdx, m_pDefault ? &(m_pDefault->m_pTotalCell->m_Value.m_nFontIdx) : NULL,	fnt_total);

	//TODO viene controllato solo il colore della prima cella
	COLORREF colors[CCellColorsDlg::MAX];
	if (m_pDefault)
	{
		m_pDefault->m_Cells[0]->GetCellColor(colors);
	}
	else
	{
		colors[CCellColorsDlg::VALUE]	= DEFAULT_CELL_FOREGROUND;
		colors[CCellColorsDlg::BACKGROUND] = DEFAULT_CELL_BACKGROUND;
	}
	colors[CCellColorsDlg::BORDER] = m_ColumnPen.m_rgbColor;
	SetColumnCellsColor(colors);
	SetSubTotalColor(colors);
	//----
}
///////////////////////////////////////////////////////////////////////////////

BOOL TableColumn::CanDeleteField	(LPCTSTR pszFieldName, CString& sLog) const
{
	sLog = this->GetFieldName() + _T(" - ");

	if (m_pHideExpr && m_pHideExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column hidden condition")), FALSE;
	if (m_pDynamicWidthExpr && m_pDynamicWidthExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column dynamic width condition")), FALSE;
	if (m_pTitleExpr && m_pTitleExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column title text condition")), FALSE;
	if (m_pTitleTextColorExpr && m_pTitleTextColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column title text color condition")), FALSE;
	if (m_pTitleBkgColorExpr && m_pTitleBkgColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column title background color condition")), FALSE;
	if (m_pTitleTooltipExpr && m_pTitleTooltipExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column title tooltip condition")), FALSE;

	if (m_pTextColorExpr && m_pTextColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column text color condition")), FALSE;
	if (m_pBkgColorExpr && m_pBkgColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column background color condition")), FALSE;

	if (m_pCellTooltipExpr && m_pCellTooltipExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column tooltip condition")), FALSE;

	if (m_pCellFormatterExpr && m_pCellFormatterExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column formatter condition")), FALSE;

	if (m_pCellBordersExpr && m_pCellBordersExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column borders condition")), FALSE;
	
	if (m_pTextFontExpr && m_pTextFontExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column tooltip condition")), FALSE;

	if (m_SubTotal.m_pTextColorExpr && m_SubTotal.m_pTextColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column subtotal text color condition")), FALSE;
	if (m_SubTotal.m_pBkgColorExpr && m_SubTotal.m_pBkgColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column subtotal background color condition")), FALSE;

	if (m_pTotalTextColorExpr && m_pTotalTextColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column total text color condition")), FALSE;
	if (m_pTotalBkgColorExpr && m_pTotalBkgColorExpr->HasMember(pszFieldName))
		return (sLog += _TB("Column total background color condition")), FALSE;

	sLog.Empty();
	return TRUE;
}

//-----------------------------------------------------------------------------
void TableColumn::UpdateDocument(BOOL bUpdateProperty)
{
	ASSERT_VALID(m_pTable);
	ASSERT_VALID(m_pTable->m_pDocument);

	m_pTable->m_pDocument->InvalidateRect(GetColumnCellsRect(), m_pTable->m_bTransparent);
	m_pTable->m_pDocument->InvalidateRect(m_pTable->AllTotalsRect(), TRUE);

	m_pTable->m_pDocument->UpdateWindow();
	m_pTable->m_pDocument->SetModifiedFlag();

	if (bUpdateProperty)
	{
		m_pTable->m_pDocument->GetWoormFrame()->SelectLayoutObject(this);	//TODO LAYOUT : aggiungere layout corrente come parametro
	}

}

//-----------------------------------------------------------------------------
void TableColumn::Redraw()
{
	UpdateDocument(FALSE);
}

//------------------------------------------------------------------------------
CRect TableColumn::GetColumnRect() const
{ 
	if(HasTotal() && !m_pTable->m_bHideColumnsTitle)
		return m_ColumnRect; 
	else 
	{
		CRect colRect(m_ColumnRect);
		if (!HasTotal())
			colRect.bottom -= TotalRect().Height();
		if (m_pTable->m_bHideColumnsTitle)
			colRect.top += m_pTable->ColumnTitleRect(0).Height();

		return colRect;
	}
}

//------------------------------------------------------------------------------
//TODO CHART skippare le righe che contengono dei tail di stringa
BOOL TableColumn::FillSeries(CBCGPChartSeries* pSeries, WORD nColumnID) const
{
	DataType dtCol = GetDataType();
	DataObj* pCellData = DataObj::DataObjCreate(dtCol);
	ASSERT_VALID(pCellData);
	int n = 0;
	for (int r = 0; r < m_Cells.GetCount(); r++)
	{
		TableCell* pCell = m_Cells[r];
		ASSERT_VALID(pCell);

		if (pCell->IsEnabledRDEData() && pCell->GetRDEValue())
		{
			double val = 0.0;
			pCellData->Assign(*pCell->GetRDEValue());

			if (pCellData->IsKindOf(RUNTIME_CLASS(DataDbl)))
			{
				val = (double) *(DataDbl*)pCellData;
				pSeries->AddDataPoint(val);
			}
			else if (pCellData->IsKindOf(RUNTIME_CLASS(DataInt)))
			{
				val = (short) *(DataInt*)pCellData;
				pSeries->AddDataPoint(val);
			}
			else if (pCellData->IsKindOf(RUNTIME_CLASS(DataLng)))
			{
				val = (long) *(DataLng*)pCellData;
				pSeries->AddDataPoint(val);
			}
			else
			{
				//ASSERT(FALSE);
			}
		}
	}
	SAFE_DELETE(pCellData);
	return TRUE;
}

DataArray* TableColumn::GetDataArrayFromId(WORD nColumnID) const
{
	DataArray* retArray = NULL;
	DataType dtCol = GetDataType();
	
	
	int n = 0;
	for (int r = 0; r < m_Cells.GetCount(); r++)
	{
		TableCell* pCell = m_Cells[r];
		ASSERT_VALID(pCell);

		if (pCell->IsEnabledRDEData() && pCell->GetRDEValue())
		{
			DataObj* pCellData = DataObj::DataObjCreate(dtCol);
			ASSERT_VALID(pCellData);
			pCellData->Assign(*pCell->GetRDEValue());
			if (!retArray)
				retArray = new DataArray();
			retArray->Add(pCellData);
		}
	}
	
	return retArray;
}

//------------------------------------------------------------------------------
//TODO CHART skippare le righe che contengono dei tail di stringa
BOOL TableColumn::FillSeries(CStringArray& pCategories, WORD nColumnID) const
{
	DataType dtCol = GetDataType();
	DataObj* pCellData = DataObj::DataObjCreate(dtCol);
	ASSERT_VALID(pCellData);
	int n = 0;
	for (int r = 0; r < m_Cells.GetCount(); r++)
	{
		TableCell* pCell = m_Cells[r];
		ASSERT_VALID(pCell);

		if (pCell->IsEnabledRDEData() && pCell->GetRDEValue())
		{
			double val = 0.0;
			pCellData->Assign(*pCell->GetRDEValue());

			pCategories.Add(pCellData->Str());
		}
	}
	SAFE_DELETE(pCellData);
	return TRUE;
}

//==============================================================================
//          Class SelectedColumn
//==============================================================================

//---------------------------------------------------------------------------
ActiveColumn::ActiveColumn(TableColumn* pCol, CWoormDocMng* pDocument)
	:
	m_pColumn(pCol),
	m_pActive(NULL),
	IDisposingSourceImpl(this)
{
	if (pDocument->m_pWoormIni->m_bEnableNewObjectSelection)
		m_pActive = new NewActiveRect(pDocument->m_pWoormIni, TRUE);
	else
		m_pActive = new OldActiveRect(FALSE);

	m_pActive->Attach(pDocument);
}

//---------------------------------------------------------------------------
ActiveColumn::~ActiveColumn()
{
	m_pActive->Clear();
	SAFE_DELETE(m_pActive);
}

//---------------------------------------------------------------------------
void ActiveColumn::SetActive()
{
	CRect colRect;
	if (m_pColumn->IsHidden())
		return;

	if (m_pColumn->HasTotal())
		colRect = m_pColumn->GetColumnRect();
	else
		colRect = m_pColumn->GetColumnCellsRect();

	if (m_pColumn->GetTable()->m_bHideColumnsTitle)
		colRect.top += m_pColumn->GetColumnTitleRect().Height();

	m_pActive->SetActive(colRect, FALSE);
}

//------------------------------------------------------------------------------
void ActiveColumn::Paint(CDC& DC, CPrintInfo* pInfo)
{
	if (pInfo || m_pColumn->IsHidden()) return;
	m_pActive->Paint(DC, pInfo);
}

//------------------------------------------------------------------------------
