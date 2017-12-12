
#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>

#include <TbWoormEngine\edtmng.h>

#include "algopars.h"

//includere come ultimo include all'inizio del cpp
#include "beginh.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// Costruttore classe CellColumnBorderPen
//
// -----------------------------------------------------------------------------
CellColumnBorderPen::CellColumnBorderPen (BorderPen& aBorderPen, int nColumn)
{
	m_BorderPen		= aBorderPen;
	m_Rect.left		= nColumn;
	m_Rect.top		= 0;
	m_Rect.right	= nColumn;
	m_Rect.bottom	= 0;
}

// Controlla se la cella corrente puo' essere fusa con quella precedente
//
// -----------------------------------------------------------------------------
BOOL CellColumnBorderPen::MergeCell (BorderPen& aBorderPen, int nColumn)
{
	if	(
			m_BorderPen.m_nWidth == aBorderPen.m_nWidth &&
			m_BorderPen.m_rgbColor == aBorderPen.m_rgbColor &&
			m_Rect.right == (nColumn -1)
		)
	{
		m_Rect.right++;
		return TRUE;
	}
	return FALSE;
}

// Costruttore classe CellDifferences
//
// -----------------------------------------------------------------------------
CellDifferences::CellDifferences
	(
			AlignType	nAlign,
			FontIdx		nFontIdx,
			BOOL		bNumeric,
			int			nColumn,
			int			nStartRow
	)
{
	m_nAlign		= nAlign;
	m_nFontIdx		= nFontIdx;
	m_bIsNumeric	= bNumeric;
	m_Rect.left		= nColumn;
	m_Rect.top		= nStartRow;
	m_Rect.right	= nColumn;
	m_Rect.bottom	= nStartRow;
}

// Costruttore classe CellBodyDiff
//
// -----------------------------------------------------------------------------
CellBodyDiff::CellBodyDiff 
	(
		AlignType nAlign, FontIdx nFontIdx, 
		BOOL bNumeric, int nColumn, int nStartRow
	)
	:
	CellDifferences (nAlign, nFontIdx, bNumeric, nColumn, nStartRow)
{
}

// Controlla se la cella corrente puo' essere fusa con quella precedente
//
// -----------------------------------------------------------------------------
BOOL CellBodyDiff::MergeCell (AlignType nAlign, FontIdx nFontIdx, int nColumn, int nRow)
{
	if	(
			GetAlign()		== nAlign	&&
			GetFontIdx()	== nFontIdx	&&
	 		IsSameRectLeft	 (nColumn)		&&
	 		IsSameRectBottom (nRow - 1)
		)
		return IncRectBottom ();

	return FALSE;
}

// Costruttore classe CellSubTotalDiff
//
// -----------------------------------------------------------------------------
CellSubTotalDiff::CellSubTotalDiff
	(
			FontIdx		nFontIdx,
			BOOL		bNumeric,
			COLORREF*	rgbColor,
			int			nColumn
	)
	:
	CellDifferences (DEFAULT_ALIGN, nFontIdx, bNumeric, nColumn)
{
	m_rgbColor[CSubTotalColorsDlg::VALUE]		= rgbColor[CSubTotalColorsDlg::VALUE];
	m_rgbColor[CSubTotalColorsDlg::BACKGROUND]	= rgbColor[CSubTotalColorsDlg::BACKGROUND];
}

// Controlla se la cella corrente puo' essere fusa con quella precedente
//
// -----------------------------------------------------------------------------
BOOL CellSubTotalDiff::MergeCell (FontIdx nFontIdx, COLORREF* rgbColor, int nColumn)
{
	if  (
			GetFontIdx ()								== nFontIdx									&&
			m_rgbColor[CSubTotalColorsDlg::VALUE]		== rgbColor[CSubTotalColorsDlg::VALUE]		&&
			m_rgbColor[CSubTotalColorsDlg::BACKGROUND]	== rgbColor[CSubTotalColorsDlg::BACKGROUND]	&&
	 		IsSameRectRight (nColumn -1)
	 	)
		return IncRectRight ();

	return FALSE;
}

// Costruttore classe CellGeneralDiff
//
// -----------------------------------------------------------------------------
CellGeneralDiff::CellGeneralDiff
	(
			AlignType	nAlign,
			BOOL		bNumeric,
			FontIdx		nFontIdx,
			BorderPen	&aBorderPen,
			int			nColumn
	)
	:
	CellDifferences (nAlign, nFontIdx, bNumeric, nColumn)
{
	m_BorderPen	= aBorderPen;
}

// Controlla se la cella corrente puo' essere fusa con quella precedente
//
// -----------------------------------------------------------------------------
BOOL CellGeneralDiff::MergeCell
	(
			AlignType	nAlign,
			FontIdx		nFontIdx,
			BorderPen	&aBorderPen,
			int			nColumn
	)
{
	return
		m_BorderPen.m_nWidth	== aBorderPen.m_nWidth &&
		m_BorderPen.m_rgbColor	== aBorderPen.m_rgbColor &&
		GetAlign ()				== nAlign			&&
		GetFontIdx ()			== nFontIdx			&&
 		IsSameRectRight	(nColumn -1);
}

// Costruttore classe CellColumnTitleDiff
//
// -----------------------------------------------------------------------------
CellColumnTitleDiff::CellColumnTitleDiff
	(
			AlignType	nAlign,
			FontIdx		nFontIdx,
			BorderPen&	aBorderPen,
			COLORREF*	rgbColor,
			int			nColumn
	)
	:
	CellGeneralDiff (nAlign, FALSE, nFontIdx, aBorderPen, nColumn)
{
	m_rgbColor[CColumnTitleColorsDlg::LABEL]			= rgbColor[CColumnTitleColorsDlg::LABEL];
	m_rgbColor[CColumnTitleColorsDlg::BACKGROUND]	= rgbColor[CColumnTitleColorsDlg::BACKGROUND];
	m_rgbColor[CColumnTitleColorsDlg::BORDER]		= rgbColor[CColumnTitleColorsDlg::BORDER];
}

// Controlla se la cella corrente puo' essere fusa con quella precedente
//
// -----------------------------------------------------------------------------
BOOL CellColumnTitleDiff::MergeCell
	(
			AlignType	nAlign,
			FontIdx		nFontIdx,
			BorderPen&	aBorderPen,
			COLORREF*	rgbColor,
			int			nColumn
	)
{
	if  (   
			CellGeneralDiff::MergeCell(nAlign, nFontIdx, aBorderPen, nColumn)							&&
			m_rgbColor[CColumnTitleColorsDlg::LABEL]		== rgbColor[CColumnTitleColorsDlg::LABEL]		&&
			m_rgbColor[CColumnTitleColorsDlg::BACKGROUND]	== rgbColor[CColumnTitleColorsDlg::BACKGROUND]
	 	)
		return IncRectRight();

	return FALSE;
}

// Costruttore classe CellTotalDiff
//
// -----------------------------------------------------------------------------
CellTotalDiff::CellTotalDiff
	(
			AlignType	nAlign,
			BOOL		bNumeric,
			FontIdx		nFontIdx,
			BorderPen&	aBorderPen,
			COLORREF*	rgbColor,
			int			nColumn
	)
	:
	CellGeneralDiff (nAlign, bNumeric, nFontIdx, aBorderPen, nColumn)
{
	m_rgbColor[CTotalColorsDlg::VALUE]		= rgbColor[CTotalColorsDlg::VALUE];
	m_rgbColor[CTotalColorsDlg::BACKGROUND]	= rgbColor[CTotalColorsDlg::BACKGROUND];
	m_rgbColor[CTotalColorsDlg::BORDER]		= rgbColor[CTotalColorsDlg::BORDER];
}

// Controlla se la cella corrente puo' essere fusa con quella precedente
//
// -----------------------------------------------------------------------------
BOOL CellTotalDiff::MergeCell
	(
			AlignType	nAlign,
			FontIdx		nFontIdx,
			BorderPen&	aBorderPen,
			COLORREF*	rgbColor,
			int			nColumn
	)
{
	if  (   
			CellGeneralDiff::MergeCell(nAlign, nFontIdx, aBorderPen, nColumn)				&&
			m_rgbColor[CTotalColorsDlg::VALUE]		== rgbColor[CTotalColorsDlg::VALUE]		&&
			m_rgbColor[CTotalColorsDlg::BACKGROUND]	== rgbColor[CTotalColorsDlg::BACKGROUND]
	 	)
		return IncRectRight();

	return FALSE;
}

// Costruttore classe CounterColorElement
//
// -----------------------------------------------------------------------------
CounterColorElement::CounterColorElement (COLORREF rgbColor)
{
	m_rgbColor	= rgbColor;
	m_nCounter = 1;
}

// Costruttore classe CounterColor
//
// -----------------------------------------------------------------------------
CounterColor::CounterColor ()
{
}

// Se il colore e gia' presente nell'array lo aggiunge, altrimenti incrementa
// il relativo contatore
//
// -----------------------------------------------------------------------------
void CounterColor::AddOrIncrement (COLORREF rgbColor)
{
	for (int index = 0; index < m_Counters.GetSize (); index++)
		if (m_Counters[index]->GetColor() == rgbColor)
		{
			m_Counters[index]->IncrementCounter ();
			return;
		}
	m_Counters.Add (new CounterColorElement(rgbColor));
}

// Ritorna il colore piu' usato
//
// -----------------------------------------------------------------------------
COLORREF CounterColor::GetColorMaxCounter () const
{
	COLORREF	rgbColor = m_Counters[0]->GetColor ();
	int 		nMax 	= m_Counters[0]->GetCounter ();

	for (int index = 1; index < m_Counters.GetSize (); index++)
		if(m_Counters[index]->GetCounter () > nMax)
		{
			nMax		= m_Counters[index]->GetCounter ();
			rgbColor	= m_Counters[index]->GetColor ();
		}
	return rgbColor;
}

// Costruttore classe SingleRect
//
// -----------------------------------------------------------------------------
SingleRect::SingleRect (COLORREF* aCellColor, int nColumn, int nStartRow, int nEndRow)
{
	m_dwCellColor[CCellColorsDlg::VALUE]		= aCellColor[CCellColorsDlg::VALUE];
	m_dwCellColor[CCellColorsDlg::BACKGROUND]	= aCellColor[CCellColorsDlg::BACKGROUND];
	m_dwCellColor[CCellColorsDlg::BORDER]		= aCellColor[CCellColorsDlg::BORDER];
	m_Rect.left		= nColumn;
	m_Rect.top		= nStartRow;
	m_Rect.right	= nColumn;
	m_Rect.bottom	= nEndRow;
}

// Controlla se il rettangolo corrente puo' essere unito ad un rettangolo gia'
// presente nell'array
//
// -----------------------------------------------------------------------------
BOOL SingleRect::MergeRectColors (COLORREF* aCellColor, int nColumn, int nStartRow, int nEndRow)
{
	if	(
		m_dwCellColor[CCellColorsDlg::VALUE]		== aCellColor[CCellColorsDlg::VALUE]		&&
		m_dwCellColor[CCellColorsDlg::BACKGROUND]	== aCellColor[CCellColorsDlg::BACKGROUND]	&&
		(m_Rect.top == nStartRow && (m_Rect.right + 1)	== nColumn && m_Rect.bottom == nEndRow)
		)
	{
		m_Rect.right++;
		return TRUE;
	}
	return FALSE;
}

// Costruttore classe BodyTable
//
// -----------------------------------------------------------------------------
BodyTable::BodyTable(Table* pTable)
	:
	m_pTable (pTable)
{
}

// Controlla se la cella corrente puo' essere unita ad una cella gia' esistente, 
// altrimenti aggiunge una nuova cella nell'array
//
void BodyTable::AddBodyCellAttrib
	(TableColumn* pColumn, BOOL bIsNumeric, int nCol, int nRow /*= 0*/)
{
	BOOL 		bDifferDefault;
	AlignType	nAlign	= pColumn->GetCellAlign 	(nRow);
	FontIdx		nFontIdx = pColumn->GetCellFontIdx	(nRow);

	if (bIsNumeric)
		bDifferDefault = 
		(
			nAlign != DEFAULT_CELL_NUM_ALIGN || 
			nFontIdx != m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_NUM)
		);
	else
		bDifferDefault = 
		(
			nAlign != DEFAULT_CELL_STRING_ALIGN || 
			nFontIdx != m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_STRING)
		);
	
	if	(
			bDifferDefault	&&
			(!(
				m_BodyCells.GetSize ()	&&
				m_BodyCells[m_BodyCells.GetUpperBound ()]->MergeCell
					(nAlign, nFontIdx, nCol, nRow)
			))
		)
		m_BodyCells.Add (new CellBodyDiff (nAlign, nFontIdx, bIsNumeric, nCol, nRow));
}

// Controlla se la cella corrente puo' essere unita ad una cella gia'
// presente nell'array, altrimenti la aggiunge
//
// -----------------------------------------------------------------------------
void BodyTable::AddBodyCellColors (COLORREF* rgbColor, int nColumn, int nStartRow, int nEndRow)
{
	for (int index = 0; index < m_TableColorRects.GetSize (); index++)
		if (m_TableColorRects[index]->MergeRectColors (rgbColor, nColumn, nStartRow, nEndRow))
			return;

	m_TableColorRects.Add(new SingleRect (rgbColor, nColumn, nStartRow, nEndRow));
}

// Setta i parametri piu' gettonati
//
// -----------------------------------------------------------------------------
void BodyTable::SetMoreCoined (COLORREF rgbValue, COLORREF rgbBackground)
{
	m_dwCoinedCellColor[CCellColorsDlg::VALUE]		= rgbValue;
	m_dwCoinedCellColor[CCellColorsDlg::BACKGROUND]	= rgbBackground;
	m_dwCoinedCellColor[CCellColorsDlg::BORDER]		= rgbValue;
}

// Aggiunge nell/array del totale un elemento che differisce dai valori di default
//
// -----------------------------------------------------------------------------
void BodyTable::SetColumnPenCells (BorderPen &aBorderPen, int nColumn)
{
	if	(   
			aBorderPen.IsDefault ()			||
			(m_ColumnPenCells.GetSize ()		&&
			m_ColumnPenCells[m_ColumnPenCells.GetUpperBound ()]->MergeCell
				(aBorderPen, nColumn))
		)
		return;
	m_ColumnPenCells.Add (new CellColumnBorderPen (aBorderPen, nColumn));
}

// Aggiunge nell/array del totale un elemento che differisce dai valori di default
//
// -----------------------------------------------------------------------------
void BodyTable::SetTotalCells (TableColumn* pColumn, BOOL bIsNumeric, int nColumn)
{
	BOOL		bDifferDefault;
	COLORREF	rgbColor[CTotalColorsDlg::MAX];
	
	AlignType	nAlign		= pColumn->GetTotalAlign	();
	FontIdx		nFontIdx	= pColumn->GetTotalFontIdx	();
	BorderPen	borderPen	= pColumn->GetTotalPen		();

	pColumn->GetTotalColor (rgbColor);
	if (bIsNumeric)
		bDifferDefault =
		(
			nAlign != DEFAULT_TOTAL_NUM_ALIGN || 
			nFontIdx != m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_TOTAL_NUM)
		);
	else
		bDifferDefault = 
		(
			nAlign != DEFAULT_TOTAL_STRING_ALIGN || 
			nFontIdx != m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_TOTAL_STRING)
		);
	if	(
			bDifferDefault													||
			!borderPen.IsDefault ()											||
			rgbColor[CTotalColorsDlg::VALUE]			!= DEFAULT_TOTAL_FOREGROUND	||
			rgbColor[CTotalColorsDlg::BACKGROUND]	!= DEFAULT_TOTAL_BACKGROUND
		)
		if	(!(
				m_TotalCells.GetSize () &&
				m_TotalCells[m_TotalCells.GetUpperBound ()]->MergeCell
					(nAlign, nFontIdx, borderPen, rgbColor, nColumn)
			))
			m_TotalCells.Add (new CellTotalDiff (nAlign, bIsNumeric, nFontIdx, borderPen, rgbColor, nColumn));
}

// Aggiunge nell/array del subtotale un elemento che differisce dai valori di default
//
// -----------------------------------------------------------------------------
void BodyTable::SetSubTotalCells (TableColumn* pColumn, BOOL bIsNumeric, int nColumn)
{
	COLORREF	rgbColor[CSubTotalColorsDlg::MAX];
	FontIdx		nFontIdx = pColumn->GetSubTotalFontIdx();

	pColumn->GetSubTotalColor(rgbColor);
	if	(
			(
				(bIsNumeric) ? 
				m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_SUBTOTAL_NUM) : 
				m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_SUBTOTAL_STRING)
			) !=
			pColumn->GetSubTotalFontIdx()											||
			rgbColor[CSubTotalColorsDlg::VALUE]		!= DEFAULT_SUBTOTAL_FOREGROUND	||
			rgbColor[CSubTotalColorsDlg::BACKGROUND]	!= DEFAULT_SUBTOTAL_BACKGROUND
		)
		if	(!(
				m_SubTotalCells.GetSize () &&
				m_SubTotalCells[m_SubTotalCells.GetUpperBound ()]->MergeCell
					(nFontIdx, rgbColor, nColumn)
			))
			m_SubTotalCells.Add (new CellSubTotalDiff (nFontIdx, bIsNumeric, rgbColor, nColumn));
}

// Aggiunge nell/array del subtotale un elemento che differisce dai valori di default
//
// -----------------------------------------------------------------------------
void BodyTable::SetColumnTitleCells (TableColumn* pColumn, int nColumn)
{
	COLORREF	rgbColor[CColumnTitleColorsDlg::MAX];

	AlignType	nAlign		= pColumn->GetColumnTitleAlign		();
	FontIdx		nFontIdx 	= pColumn->GetColumnTitleFontIdx	();
	BorderPen	borderPen	= pColumn->GetColumnTitlePen		();

	pColumn->GetColumnTitleColor(rgbColor);
	if	(
			!borderPen.IsDefault ()															||
			nAlign										!= DEFAULT_COLUMN_TITLE_ALIGN		||
			nFontIdx									!= m_pTable->m_pDocument->m_pFontStyles->GetFontIdx(FNT_COLUMN_TITLE) ||
			rgbColor[CColumnTitleColorsDlg::LABEL]		!= DEFAULT_COLUMN_TITLE_FOREGROUND	||
			rgbColor[CColumnTitleColorsDlg::BACKGROUND]	!= DEFAULT_COLUMN_TITLE_BACKGROUND
		)
		if	(!(
				m_TitleCells.GetSize () &&
				m_TitleCells[m_TitleCells.GetUpperBound ()]->MergeCell
					(nAlign, nFontIdx, borderPen, rgbColor, nColumn)
			))
			m_TitleCells.Add (new CellColumnTitleDiff (nAlign, nFontIdx, borderPen, rgbColor, nColumn));
}

// Algoritmo di UnParse
//
// -----------------------------------------------------------------------------
void BodyTable::BuildUnparseInfo ()
{
	int 			nRow, nStartRow;
	BOOL			bIsNumeric;
	DataType		dataType;
	BorderPen		borderPen;
	COLORREF		dwCurrentColor[CCellColorsDlg::MAX], baseColor[CCellColorsDlg::MAX];
	TableColumn		*pColumn;
	CounterColor	counterValue, counterBack;

	for (int nCol = 0; nCol <= m_pTable->LastColumn (); nCol++)
	{
		pColumn		= m_pTable->m_Columns[nCol];
		borderPen	= pColumn->GetColumnPen	();

		dataType	= pColumn->GetDataType();
		bIsNumeric	= !IsShowedAsString(dataType);
		
		AddBodyCellAttrib	(pColumn, bIsNumeric, nCol);

		SetTotalCells		(pColumn, bIsNumeric, nCol);
		SetSubTotalCells	(pColumn, bIsNumeric, nCol);
		SetColumnTitleCells	(pColumn, nCol);
		SetColumnPenCells 	(borderPen, nCol);

		pColumn->GetCellColor		(baseColor, 0);
		counterBack.AddOrIncrement	(baseColor[CCellColorsDlg::BACKGROUND]);
		counterValue.AddOrIncrement (baseColor[CCellColorsDlg::VALUE]);

		nStartRow = 0;
		for (nRow = 1; nRow <= m_pTable->LastRow(); nRow++)
		{
			AddBodyCellAttrib (pColumn, bIsNumeric, nCol, nRow);
			pColumn->GetCellColor (dwCurrentColor, nRow);
			counterBack.AddOrIncrement (dwCurrentColor[CCellColorsDlg::BACKGROUND]);
			counterValue.AddOrIncrement (dwCurrentColor[CCellColorsDlg::VALUE]);

			if	(
					dwCurrentColor[CCellColorsDlg::VALUE]		== baseColor[CCellColorsDlg::VALUE] &&
					dwCurrentColor[CCellColorsDlg::BACKGROUND]	== baseColor[CCellColorsDlg::BACKGROUND]
				)
				continue;

			AddBodyCellColors (baseColor, nCol, nStartRow, nRow - 1);

			baseColor[CCellColorsDlg::VALUE]		= dwCurrentColor[CCellColorsDlg::VALUE];
			baseColor[CCellColorsDlg::BACKGROUND]	= dwCurrentColor[CCellColorsDlg::BACKGROUND];
			nStartRow = nRow;
		}
		AddBodyCellColors (dwCurrentColor, nCol, nStartRow, nRow - 1);
	}
	SetMoreCoined (counterValue.GetColorMaxCounter (), counterBack.GetColorMaxCounter ());
}

