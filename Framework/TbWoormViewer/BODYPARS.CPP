
#include "stdafx.h"

#include <TbGenlib\baseapp.h>

// class definitions are in table.h 
#include "algopars.h"    
#include "table.h"
#include "viewpars.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//------------------------------------------------------------------------------
void Table::UnparseTableAttrib (ViewUnparser& ofile)
{
	BodyTable bodyTable (this);
	bodyTable.BuildUnparseInfo();
	
	if (bodyTable.GetNrColumnPenCells())	UnparseColumnPens 		(bodyTable, ofile);
	if (bodyTable.GetNrTitleCells())		UnparseColumnTitleCells (bodyTable, ofile);
	if (bodyTable.GetNrSubTotalCells())		UnparseSubTotalCells    (bodyTable, ofile);
	if (bodyTable.GetNrTotalCells())    	UnparseTotalCells       (bodyTable, ofile);
	
	UnparseBodyVariations   (bodyTable, ofile);
}       
	
//------------------------------------------------------------------------------
void Table::UnparseColumnPens (const BodyTable& bodyTable, ViewUnparser& ofile)
{   
	int nrColumnPens = bodyTable.GetNrColumnPenCells();

	if (nrColumnPens == 1)
	{
		ofile.UnparseTag (T_COLUMN_PEN, FALSE);
		UnparseColumnIndex      (bodyTable.GetNextColumnPen(0)->GetRect(), ofile);
		UnparseSingleColumnPen  (bodyTable.GetNextColumnPen(0)->GetBorderPen(), ofile);
		ofile.UnparseSep (TRUE);
	}
	else
	{
		ofile.UnparseTag (T_COLUMN_PEN, TRUE);
		ofile.UnparseBegin ();
		for (int index = 0; index < nrColumnPens; index++)
		{
			UnparseColumnIndex		(bodyTable.GetNextColumnPen(index)->GetRect(), ofile);
			UnparseSingleColumnPen	(bodyTable.GetNextColumnPen(index)->GetBorderPen(), ofile);
			ofile.UnparseSep		(TRUE);
		}                       
		ofile.UnparseEnd ();
	}
} 

//------------------------------------------------------------------------------
void Table::UnparseColumnTitleCells (const BodyTable& bodyTable, ViewUnparser& ofile)
{
	int nrColumnTitles = bodyTable.GetNrTitleCells();
	if (nrColumnTitles == 1)
	{
		ofile.UnparseTag			(T_TITLE, FALSE);
		UnparseSingleColumnTitle	(*(bodyTable.GetNextTitleCell (0)), ofile);
	}
	else
	{
		ofile.UnparseTag	(T_TITLE, TRUE);
		ofile.UnparseBegin	();
		for (int index = 0; index < nrColumnTitles; index++)
			UnparseSingleColumnTitle(*(bodyTable.GetNextTitleCell (index)), ofile);
		ofile.UnparseEnd	();
	}
}

//------------------------------------------------------------------------------
void Table::UnparseSubTotalCells (const BodyTable& bodyTable, ViewUnparser& ofile)
{
	int nrSubTotals = bodyTable.GetNrSubTotalCells();
	if (nrSubTotals == 1)
	{
		ofile.UnparseTag		(T_SUBTOTAL, FALSE);
		UnparseSingleSubTotal	(*(bodyTable.GetNextSubTotalCell (0)), ofile);
	}
	else
	{
		ofile.UnparseTag	(T_SUBTOTAL, TRUE);
		ofile.UnparseBegin	();
		for (int index = 0; index < nrSubTotals; index++)
			UnparseSingleSubTotal(*(bodyTable.GetNextSubTotalCell (index)), ofile);
		ofile.UnparseEnd	();
	}
}

//------------------------------------------------------------------------------
void Table::UnparseTotalCells (const BodyTable& bodyTable, ViewUnparser& ofile)
{
	int nrTotals = bodyTable.GetNrTotalCells();
	if (nrTotals == 1)
	{
		ofile.UnparseTag	(T_COLTOTAL, FALSE);
		UnparseSingleTotal	(*(bodyTable.GetNextTotalCell(0)), ofile);
	}
	else
	{
		ofile.UnparseTag	(T_COLTOTAL, TRUE);
		ofile.UnparseBegin	();
		for (int index = 0; index < nrTotals; index++)
			UnparseSingleTotal(*(bodyTable.GetNextTotalCell(index)), ofile);
		ofile.UnparseEnd	();
	}
} 

//------------------------------------------------------------------------------
void Table::UnparseSingleColumnPen (const BorderPen& aColumnPen, ViewUnparser& ofile)
{
	if (aColumnPen.m_rgbColor != RGB(0,0,0))
		ofile.UnparseColor (T_PEN, aColumnPen.m_rgbColor, FALSE);

	if (aColumnPen.m_nWidth != DEFAULT_PEN_WIDTH)
	{
		ofile.UnparseTag (T_SIZE, FALSE);
		ofile.UnparseInt (aColumnPen.m_nWidth, FALSE);
	}
	ofile.UnparseBlank (FALSE);
}

//------------------------------------------------------------------------------
void Table::UnparseSingleColumnTitle (CellColumnTitleDiff& aColumnTitle, ViewUnparser& ofile)
{
	COLORREF* rgbRectColors = aColumnTitle.GetColor ();
	UnparseColumnIndex (aColumnTitle.GetRect(), ofile);

	BOOL bTxt = (rgbRectColors[CColumnTitleColorsDlg::LABEL]		!= DEFAULT_COLUMN_TITLE_FOREGROUND);
	BOOL bBkg = (rgbRectColors[CColumnTitleColorsDlg::BACKGROUND]	!= DEFAULT_COLUMN_TITLE_BACKGROUND);
	BOOL bBrd = (rgbRectColors[CColumnTitleColorsDlg::BORDER]		!= RGB(0,0,0));
	
	UnparseAttribute
	(
		bTxt, bBkg, bBrd,
		rgbRectColors[CColumnTitleColorsDlg::LABEL],
		rgbRectColors[CColumnTitleColorsDlg::BACKGROUND],
		rgbRectColors[CColumnTitleColorsDlg::BORDER],
		ofile
	);

	if (aColumnTitle.GetPenWidth () != DEFAULT_PEN_WIDTH)
	{
		ofile.UnparseTag	(T_SIZE, FALSE);
		ofile.UnparseInt	(aColumnTitle.GetPenWidth(), FALSE);
		ofile.UnparseBlank	(FALSE);
	}

	BOOL bFnt = (aColumnTitle.GetFontIdx() != m_pDocument->m_pFontStyles->GetFontIdx(FNT_COLUMN_TITLE));
	BOOL bAln = (aColumnTitle.GetAlign	() != DEFAULT_COLUMN_TITLE_ALIGN);

	UnparseDifferences (bFnt, bAln, aColumnTitle.GetFontIdx(), aColumnTitle.GetAlign(), ofile);
}

//------------------------------------------------------------------------------
void Table::UnparseSingleSubTotal (CellSubTotalDiff& aSubTotal, ViewUnparser& ofile)
{
		UnparseColumnIndex (aSubTotal.GetRect(), ofile);
		COLORREF* rgbRectColors = aSubTotal.GetColor();
		
		BOOL bFnt;
		BOOL bTxt = (rgbRectColors[CSubTotalColorsDlg::VALUE]		!= DEFAULT_SUBTOTAL_FOREGROUND);
		BOOL bBkg = (rgbRectColors[CSubTotalColorsDlg::BACKGROUND]	!= DEFAULT_SUBTOTAL_BACKGROUND);

		UnparseAttribute 
		(
			bTxt, bBkg, FALSE,
			rgbRectColors[CSubTotalColorsDlg::VALUE],
			rgbRectColors[CSubTotalColorsDlg::BACKGROUND],
			RGB(0,0,0),
			ofile
		);

		if (aSubTotal.CellIsNumeric())
			bFnt = (aSubTotal.GetFontIdx () != m_pDocument->m_pFontStyles->GetFontIdx(FNT_SUBTOTAL_NUM));
		else
			bFnt = (aSubTotal.GetFontIdx () != m_pDocument->m_pFontStyles->GetFontIdx(FNT_SUBTOTAL_STRING));

		UnparseDifferences (bFnt, FALSE, aSubTotal.GetFontIdx(), aSubTotal.GetAlign(), ofile);
}

//------------------------------------------------------------------------------
void Table::UnparseSingleTotal (CellTotalDiff& aTotalDiff, ViewUnparser& ofile)
{
		UnparseColumnIndex (aTotalDiff.GetRect(), ofile);
		COLORREF* rgbRectColors = aTotalDiff.GetColor();

		BOOL bFnt, bAln;
		BOOL bTxt = (rgbRectColors[CTotalColorsDlg::VALUE]		!= DEFAULT_TOTAL_FOREGROUND);
		BOOL bBkg = (rgbRectColors[CTotalColorsDlg::BACKGROUND]	!= DEFAULT_TOTAL_BACKGROUND);
		BOOL bBrd = (rgbRectColors[CTotalColorsDlg::BORDER]		!= RGB(0,0,0));
		
		UnparseAttribute 
		(	
			bTxt, bBkg, bBrd,
			rgbRectColors[CTotalColorsDlg::VALUE],
			rgbRectColors[CTotalColorsDlg::BACKGROUND],
			rgbRectColors[CTotalColorsDlg::BORDER],
			ofile
		);

		if (aTotalDiff.GetPenWidth() != DEFAULT_PEN_WIDTH)
		{
			ofile.UnparseTag	(T_SIZE, FALSE);
			ofile.UnparseInt	(aTotalDiff.GetPenWidth(), FALSE);
			ofile.UnparseBlank	(FALSE);
		}

		if (aTotalDiff.CellIsNumeric())
		{
			bFnt = (aTotalDiff.GetFontIdx() != m_pDocument->m_pFontStyles->GetFontIdx(FNT_TOTAL_NUM));
			bAln = (aTotalDiff.GetAlign	() != DEFAULT_TOTAL_NUM_ALIGN);
		}
		else
		{
			bFnt = (aTotalDiff.GetFontIdx() != m_pDocument->m_pFontStyles->GetFontIdx(FNT_TOTAL_STRING));
			bAln = (aTotalDiff.GetAlign	() != DEFAULT_TOTAL_STRING_ALIGN);
		}

		UnparseDifferences (bFnt, bAln, aTotalDiff.GetFontIdx(), aTotalDiff.GetAlign(), ofile);
}
	
//------------------------------------------------------------------------------
void Table::UnparseBodyVariations (const BodyTable& bodyTable, ViewUnparser& ofile)
{
	COLORREF* rgbCellColors;
	COLORREF* rgbCoinedColors = bodyTable.GetCoinedCellColor ();

	BOOL	bTxt = (rgbCoinedColors[CCellColorsDlg::VALUE] != DEFAULT_CELL_FOREGROUND);
	BOOL	bBkg = (rgbCoinedColors[CCellColorsDlg::BACKGROUND] != DEFAULT_CELL_BACKGROUND);

	int		nRectIndex	= 0;
	int		nFixIndex	= -1;
	
	int nrVariations = bodyTable.GetNrCellDiff();
	if (bTxt || bBkg)
		nrVariations++;

	while ((nrVariations < 2) && (nRectIndex < bodyTable.GetNrRect()))
	{
		rgbCellColors = bodyTable.GetNextSingleRect(nRectIndex)->GetCellColor();
		if	(
			rgbCellColors[CCellColorsDlg::VALUE] != rgbCoinedColors[CCellColorsDlg::VALUE] ||
			rgbCellColors[CCellColorsDlg::BACKGROUND] != rgbCoinedColors[CCellColorsDlg::BACKGROUND]
			)
		{
			nrVariations++;
			if (nFixIndex >= 0)
				break;

			nFixIndex = nRectIndex;
		}
		nRectIndex++;
	}

	if (nrVariations == 0)
		return;

	ofile.UnparseCrLf	();
	ofile.UnparseTag	(T_BODY, (nrVariations > 1));

	if (nrVariations < 2)
		UnparseVariations (bTxt, bBkg, nFixIndex, nRectIndex,  bodyTable, ofile);
	else
	{
		ofile.UnparseBegin	();
		UnparseVariations	(bTxt, bBkg, nFixIndex, nRectIndex, bodyTable, ofile);
		ofile.UnparseEnd	();
	}
	ofile.UnparseCrLf ();
}

//------------------------------------------------------------------------------
void Table::UnparseVariations 
	(
			BOOL			bTxt, 
			BOOL			bBkg, 
			int				nFixIndex, 
			int				nRectIndex,
	const   BodyTable&		bodyTable, 
			ViewUnparser&	ofile 
	)
{     
	if (bTxt || bBkg)
		UnparseMoreCoined (bTxt, bBkg, bodyTable.GetCoinedCellColor(), ofile);
		
	if (bodyTable.GetNrRect())
		UnparseRectSet (nFixIndex, nRectIndex, bodyTable, ofile);
		
	if (bodyTable.GetNrCellDiff())
		UnparseDifferentCells (bodyTable, ofile);
}

//------------------------------------------------------------------------------
void Table::UnparseMoreCoined (BOOL bTxt, BOOL bBkg, COLORREF* colors, ViewUnparser& ofile )
{   
	ofile.UnparseTag (T_ALL, FALSE);
	UnparseAttribute 
	(
		bTxt, bBkg, FALSE,
		colors[CCellColorsDlg::VALUE], 
		colors[CCellColorsDlg::BACKGROUND], 
		RGB(0,0,0),
		ofile
	);
	ofile.UnparseSep(TRUE);
}

//------------------------------------------------------------------------------
void Table::UnparseAttribute
	(
		BOOL			bTxt,
		BOOL			bBkg,
		BOOL			bBrd,
		COLORREF		txtColor, 
		COLORREF		bkgColor, 
		COLORREF		brdColor,
		ViewUnparser&	ofile
	)       
{
	if (bTxt)	ofile.UnparseColor (T_TEXTCOLOR, txtColor, FALSE);
	if (bBkg)	ofile.UnparseColor (T_BKGCOLOR, bkgColor, FALSE);
	if (bBrd)	ofile.UnparseColor (T_PEN, brdColor, FALSE);
}


//------------------------------------------------------------------------------
void Table::UnparseRectSet 
	(
			int				nFixIndex,
			int				nRectIndex,
	const   BodyTable&		bodyTable,
			ViewUnparser&	ofile
	)
{       
	if (nFixIndex >= 0)
		UnparseSingleRect (bodyTable, nFixIndex, ofile);
		
	int nrRect = bodyTable.GetNrRect();
	for (int index = nRectIndex; index < nrRect; index++)
		UnparseSingleRect (bodyTable, index, ofile);
}

//------------------------------------------------------------------------------
void Table::UnparseSingleRect (const BodyTable& bodyTable, int index, ViewUnparser& ofile)
{   
	SingleRect* pSingleRect		= bodyTable.GetNextSingleRect	(index);
	COLORREF* rgbCoinedColors	= bodyTable.GetCoinedCellColor	();
	COLORREF* singleCellColor	= pSingleRect->GetCellColor		();
	
	BOOL rectTxt = (singleCellColor[CCellColorsDlg::VALUE]		!= rgbCoinedColors[CCellColorsDlg::VALUE]);
	BOOL rectBkg = (singleCellColor[CCellColorsDlg::BACKGROUND]	!= rgbCoinedColors[CCellColorsDlg::BACKGROUND]);

	if (rectTxt || rectBkg)
	{
		ofile.UnparseRect (T_RECT, pSingleRect->GetRect(), FALSE);

		UnparseAttribute 
		(
			rectTxt, rectBkg, FALSE,
			singleCellColor[CCellColorsDlg::VALUE],
			singleCellColor[CCellColorsDlg::BACKGROUND],
			RGB (0,0,0),
			ofile
		);
		ofile.UnparseSep(TRUE);
	}       
}

//------------------------------------------------------------------------------
void Table::UnparseDifferentCells (const BodyTable& bodyTable, ViewUnparser& ofile)
{   
	BOOL bFnt, bAln;
	int nrDifferences = bodyTable.GetNrCellDiff ();

	for (int diffIndex = 0; diffIndex < nrDifferences; diffIndex++)
	{   
		CellBodyDiff* singleDiff = bodyTable.GetNextCellDiff (diffIndex);
		UnparseCellIndex (singleDiff->GetRect(), ofile);

		if (singleDiff->CellIsNumeric())
		{
			bFnt = (singleDiff->GetFontIdx() != m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_NUM));
			bAln = (singleDiff->GetAlign() 	!= DEFAULT_CELL_NUM_ALIGN);
		}
		else
		{
			bFnt = (singleDiff->GetFontIdx() != m_pDocument->m_pFontStyles->GetFontIdx(FNT_CELL_STRING));
			bAln = (singleDiff->GetAlign() 	!= DEFAULT_CELL_STRING_ALIGN);
		}
		UnparseDifferences 
		(
			bFnt, bAln, 
			singleDiff->GetFontIdx(), 
			singleDiff->GetAlign(), 
			ofile
		);
	}
}
 
//------------------------------------------------------------------------------
void Table::UnparseCellIndex (const CRect& singleCell, ViewUnparser& ofile )              
{
	ofile.UnparseTag	(T_CELL, FALSE);
	ofile.UnparseOpen	();
	ofile.UnparseInt	(singleCell.top, FALSE);
	ofile.UnparseComma	();
	ofile.UnparseInt	(singleCell.left, FALSE);
	
	if (singleCell.top != singleCell.bottom)
	{
		ofile.UnparseComma	();
		ofile.UnparseInt	(singleCell.bottom, FALSE);
		ofile.UnparseComma	();
		ofile.UnparseInt	(singleCell.right, FALSE);
	}
	ofile.UnparseClose (FALSE);
	ofile.UnparseBlank (FALSE);
} 

//------------------------------------------------------------------------------
void Table::UnparseColumnIndex (const CRect& singleCell, ViewUnparser& ofile )              
{
	ofile.UnparseTag	(T_COLUMN, FALSE);
	ofile.UnparseOpen	();
	ofile.UnparseInt	(singleCell.left, FALSE);
	
	if (singleCell.right != singleCell.left)
	{
		ofile.UnparseComma	();
		ofile.UnparseInt	(singleCell.right, FALSE);
	}
	ofile.UnparseClose (FALSE);
	ofile.UnparseBlank (FALSE);
} 

//------------------------------------------------------------------------------
void Table::UnparseDifferences (BOOL bFnt, BOOL bAln, FontIdx font, AlignType nAlign, ViewUnparser& ofile)             
{   
	if (bFnt)        
		UnparseSingleFont (font, ofile);
	if (bAln)
		ofile.UnparseAlign (nAlign, TRUE);
	else
		ofile.UnparseSep (TRUE);
}

//------------------------------------------------------------------------------
void Table::UnparseSingleFont (FontIdx font, ViewUnparser& ofile) 
{
	ofile.UnparseTag	(T_FONTSTYLE, FALSE);
	ofile.UnparseString	(m_pDocument->m_pFontStyles->GetStyleName(font), FALSE);
	ofile.UnparseBlank	();
} 

//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------ 

BOOL Table::ParseTableAttrib (ViewParser& lex)
{   
	Token nToken;      
	
	while ((nToken = lex.LookAhead()) != T_END) 
	{
		switch (nToken)
		{
			case T_COLUMN_PEN :    
				lex.SkipToken();
				if (!ParseColumnPens (lex))
					return FALSE;
				break;       
			case T_BODY :   
				lex.SkipToken();
				if (!ParseBodyVariations (lex))
					return FALSE;
				break;				
			case T_SUBTOTAL :       
				lex.SkipToken();
				if (!ParseSubTotalCells (lex))
					return FALSE;
				break;
			case T_COLTOTAL :       
				lex.SkipToken();
				if (!ParseTotalCells (lex))
					return FALSE;
				break;
			case T_TITLE :  
				lex.SkipToken();
				if (!ParseColumnTitleCells (lex))
					return FALSE;
				break;
			case T_STR :
				{
				return lex.SetError(
					_TB("Parsing a Table an unexpected string was found.\nIt could be a title of an extra column:\nplease check table\'s columns number")
					);
				}
			default : 
				return lex.SetError(_TB("An unexpected symbol was found"));
		}                                       
	}  
	return TRUE;
}

//------------------------------------------------------------------------------                
BOOL Table::ParseColumnPens (ViewParser& lex)
{   
	if (lex.LookAhead(T_BEGIN))
		return
			lex.ParseBegin  ()		&&
			ParsePens		(lex)   &&
			lex.ParseEnd    ();

	return ParseSingleColumnPen(lex);
}   

//------------------------------------------------------------------------------                
BOOL Table::ParseSubTotalCells (ViewParser& lex)
{   
	COLORREF allColors[CSubTotalColorsDlg::MAX];
	allColors[CSubTotalColorsDlg::VALUE]		= DEFAULT_SUBTOTAL_FOREGROUND;
	allColors[CSubTotalColorsDlg::BACKGROUND]	= DEFAULT_SUBTOTAL_BACKGROUND;  
	
	if (lex.LookAhead(T_BEGIN))
		return
			lex.ParseBegin  ()					&&
			ParseSubTotals  (allColors, lex)	&&
			lex.ParseEnd    ();

	return ParseSingleSubTotal(allColors, lex);
}

//------------------------------------------------------------------------------                
BOOL Table::ParseTotalCells (ViewParser& lex)
{   
	COLORREF allColors[CTotalColorsDlg::MAX];
	allColors[CTotalColorsDlg::VALUE]		= DEFAULT_TOTAL_FOREGROUND;
	allColors[CTotalColorsDlg::BACKGROUND]	= DEFAULT_TOTAL_BACKGROUND;  
	
	if (lex.LookAhead(T_BEGIN))
		return
			lex.ParseBegin  ()					&&
			ParseTotals		(allColors, lex)	&&
			lex.ParseEnd    ();

	return ParseSingleTotal(allColors, lex);
}

//------------------------------------------------------------------------------
//
BOOL Table::ParseColumnTitleCells (ViewParser& lex)
{   
	COLORREF allColors[CColumnTitleColorsDlg::MAX];
	allColors[CColumnTitleColorsDlg::LABEL]			= DEFAULT_COLUMN_TITLE_FOREGROUND;
	allColors[CColumnTitleColorsDlg::BACKGROUND]	= DEFAULT_COLUMN_TITLE_BACKGROUND;  
	
	if (lex.LookAhead(T_BEGIN))
		return
			lex.ParseBegin  	() 					&&
			ParseColumnTitles	(allColors, lex)	&&
			lex.ParseEnd		();

	return ParseSingleColumnTitle(allColors, lex);
}

//------------------------------------------------------------------------------
BOOL Table::ParsePens (ViewParser& lex)
{
	BOOL ok;

	do { ok = ParseSingleColumnPen (lex) && !lex.Bad() && !lex.Eof();}
	while (ok && !lex.LookAhead(T_END));

	return ok;
}  
//------------------------------------------------------------------------------
BOOL Table::ParseSubTotals (COLORREF* allColors, ViewParser& lex)
{
	BOOL ok;

	do { ok = ParseSingleSubTotal(allColors, lex) && !lex.Bad() && !lex.Eof();}
	while (ok && !lex.LookAhead(T_END));

	return ok;
}  

//------------------------------------------------------------------------------
BOOL Table::ParseTotals (COLORREF* allColors, ViewParser& lex)
{
	BOOL ok;

	do { ok = ParseSingleTotal(allColors, lex) && !lex.Bad() && !lex.Eof();}
	while (ok && !lex.LookAhead(T_END));

	return ok;
}  

//------------------------------------------------------------------------------
BOOL Table::ParseColumnTitles (COLORREF* allColors, ViewParser& lex)
{
	BOOL ok;

	do { ok = ParseSingleColumnTitle(allColors, lex) && !lex.Bad() && !lex.Eof();}
	while (ok && !lex.LookAhead(T_END));

	return ok;
}  

//------------------------------------------------------------------------------
BOOL Table::ParseSingleColumnPen (ViewParser& lex)
{
	CRect rect;
	BorderPen aPen;
	if (! ParseColumnIndex (&rect, lex) || ! ParseColumnPen (&aPen, lex) )
		return FALSE;          
	
	// checks whether the columns belong to the table	
	int colSize = m_Columns.GetSize();
	if	((rect.left >= colSize) || (rect.right >= colSize))
		return lex.SetError(_TB("Cell or column number not belonging to the table"));    
		   
	for (int nCol = rect.left; nCol<= rect.right; nCol++) 
		m_Columns[nCol]->SetColumnPen(aPen);
	return TRUE;
}             

//------------------------------------------------------------------------------
BOOL Table::ParseSingleSubTotal (COLORREF* allColors, ViewParser& lex)
{
	CRect	rect;
	Token	nToken;
	int		nCol;  
	BOOL	bIsAny = FALSE;
	
	if (!ParseColumnIndex (&rect, lex)) return FALSE;
	while ( (nToken = lex.LookAhead()) != T_SEP)
	{
		switch (nToken)                
		{
			case T_TEXTCOLOR :
				if (!lex.ParseColor (T_TEXTCOLOR, allColors[CSubTotalColorsDlg::VALUE]))
					return FALSE;
				bIsAny = TRUE;
				for (nCol = rect.left; nCol <= rect.right; nCol++)
					(m_Columns[nCol])->m_SubTotal.SetTextColor (allColors[CSubTotalColorsDlg::VALUE]);
				break;
				
			case T_BKGCOLOR :
				if (!lex.ParseColor (T_BKGCOLOR, allColors[CSubTotalColorsDlg::BACKGROUND]))
					return FALSE;
				for (nCol = rect.left; nCol <= rect.right; nCol++)
					m_Columns[nCol]->m_SubTotal.SetBkgColor (allColors[CSubTotalColorsDlg::BACKGROUND]);
				bIsAny = TRUE;
				break;
				
			case T_FONTSTYLE :                                
				bIsAny = TRUE;
				return (ParseSubTotalDifferences (rect, lex));
			 
			default :                                     
				return lex.SetError(_TB("An unexpected symbol was found"));
		}
	}       
	// checks wheter there is no description
	if (!bIsAny)	return lex.SetError(_TB("There is no attribute"));
	return lex.ParseSep();
}

//------------------------------------------------------------------------------
BOOL Table::ParseSingleTotal (COLORREF* allColors, ViewParser& lex)
{
	CRect	rect;
	Token	nToken;
	int		nCol;
	BOOL	bIsAny = FALSE;
	
	if (!ParseColumnIndex (&rect, lex)) return FALSE;
	while ( (nToken = lex.LookAhead()) != T_SEP)
	{
		switch (nToken)                
		{
			case T_TEXTCOLOR :
				if (!lex.ParseColor (T_TEXTCOLOR, allColors[CTotalColorsDlg::VALUE]))
					return FALSE;
				bIsAny = TRUE;
				for (nCol = rect.left; nCol <= rect.right; nCol++)
					m_Columns[nCol]->m_pTotalCell->SetTextTotalColor(allColors[CTotalColorsDlg::VALUE]);
				break;
				
			case T_BKGCOLOR :
				if (!lex.ParseColor (T_BKGCOLOR, allColors[CTotalColorsDlg::BACKGROUND]))
					return FALSE;
				for (nCol = rect.left; nCol <= rect.right; nCol++)
					m_Columns[nCol]->m_pTotalCell->SetBkgTotalColor(allColors[CTotalColorsDlg::BACKGROUND]);
				bIsAny = TRUE;
				break;
				
			case T_PEN :
				{
					if (!lex.ParseColor (T_PEN, allColors[CTotalColorsDlg::BORDER]))
						return FALSE;
					BorderPen borderPen;
					for (nCol = rect.left; nCol<= rect.right; nCol++)
					{
						borderPen = m_Columns[nCol]->GetTotalPen();
						borderPen.m_rgbColor = allColors[CTotalColorsDlg::BORDER];
						m_Columns[nCol]->SetTotalPen(borderPen);
					}
					bIsAny = TRUE;
					break;
				}

			case T_SIZE     :
				{
					int    penWidth;
					if( !lex.ParseSize(penWidth )) 
						return FALSE;
					BorderPen borderPen;
					for (nCol = rect.left; nCol<= rect.right; nCol++)
					{
						borderPen = m_Columns[nCol]->GetTotalPen();
						borderPen.m_nWidth = penWidth;
						m_Columns[nCol]->SetTotalPen(borderPen);
					}
					bIsAny = TRUE;
					break;
				}

			case T_FONTSTYLE :                                
			case T_ALIGN :
				bIsAny = TRUE;
				return (ParseTotalDifferences (rect, nToken, lex));
			 
			default : 				
				return lex.SetError(_TB("An unexpected symbol was found"));

		}
	}  
	// checks whether there is no description
	if (!bIsAny)	return lex.SetError(_TB("There is no attribute"));
	return lex.ParseSep();
}

//------------------------------------------------------------------------------
BOOL Table::ParseSingleColumnTitle (COLORREF* allColors, ViewParser& lex)
{
	CRect	rect;
	Token	nToken;
	int		nCol;
	BOOL	bIsAny = FALSE;
	
	if (!ParseColumnIndex (&rect, lex)) return FALSE;  
	while ( (nToken = lex.LookAhead()) != T_SEP)
	{
		switch (nToken)                
		{    
			case T_TEXTCOLOR :
				if (!lex.ParseColor (T_TEXTCOLOR, allColors[CColumnTitleColorsDlg::LABEL]))
					return FALSE;
				bIsAny = TRUE;
				for (nCol = rect.left; nCol <= rect.right; nCol++)
					m_Columns[nCol]->m_Title.SetTextColor (allColors[CColumnTitleColorsDlg::LABEL]);
				break;
				
			case T_BKGCOLOR :
				if (!lex.ParseColor (T_BKGCOLOR, allColors[CColumnTitleColorsDlg::BACKGROUND]))
					return FALSE;
				for (nCol = rect.left; nCol <= rect.right; nCol++)
					m_Columns[nCol]->m_Title.SetBkgColor (allColors[CColumnTitleColorsDlg::BACKGROUND]);
				bIsAny = TRUE;
				break;
				
			case T_PEN :
				{
					if (!lex.ParseColor (T_PEN, allColors[CColumnTitleColorsDlg::BORDER]))
						return FALSE;
					BorderPen borderPen;
					for (nCol = rect.left; nCol<= rect.right; nCol++)
					{
						borderPen = m_Columns[nCol]->GetColumnTitlePen();
						borderPen.m_rgbColor = allColors[CColumnTitleColorsDlg::BORDER];
						m_Columns[nCol]->SetColumnTitlePen (borderPen);
					}
					bIsAny = TRUE;
					break;
				}

			case T_SIZE :
				{
					int penWidth;
					if (!(lex.ParseSize(penWidth)))
						return FALSE;
					BorderPen borderPen;
					for (nCol = rect.left; nCol<= rect.right; nCol++)
					{
						borderPen = m_Columns[nCol]->GetColumnTitlePen();
						borderPen.m_nWidth = penWidth;
						m_Columns[nCol]->SetColumnTitlePen (borderPen);
					}
					bIsAny = TRUE;
					break;
				}	
			case T_FONTSTYLE :                                
			case T_ALIGN :
				bIsAny = TRUE;
				return (ParseColumnTitleDifferences (rect, nToken, lex));
			 
			default : 				
				return lex.SetError(_TB("An unexpected symbol was found"));
		}
	}  
	// checks whether there is no description
	if (!bIsAny)	return lex.SetError(_TB("There is no attribute"));
	return lex.ParseSep();
}             

//------------------------------------------------------------------------------
BOOL Table::ParseColumnPen (BorderPen* pen, ViewParser& lex)
{
	Token	nToken;
	BOOL	bIsAny = FALSE;

	while( (nToken=lex.LookAhead()) != T_SEP)
	{
		switch(nToken)
		{
			case T_PEN :
				if (!lex.ParseColor(T_PEN, pen->m_rgbColor))
					return FALSE;
				bIsAny = TRUE;
				break;
			case T_SIZE :
				if (!lex.ParseSize(pen->m_nWidth))
					return FALSE;
				bIsAny = TRUE;
				break;
			default : 				
				return lex.SetError(_TB("An unexpected symbol was found"));
		}
	}
	// checks whether there is no description
	if (!bIsAny)	return lex.SetError(_TB("There is no attribute"));
	return lex.ParseSep();
}

//------------------------------------------------------------------------------
BOOL Table::ParseSubTotalDifferences (const CRect& cellRect, ViewParser& lex)
{    
	if (!ParseSubTotalFontIdx (cellRect, lex))
		return FALSE;
	return lex.ParseSep ();
}

//------------------------------------------------------------------------------
BOOL Table::ParseTotalDifferences (const CRect& cellRect, Token token, ViewParser& lex)
{    
	if (token == T_FONTSTYLE)
	{
		if (!ParseTotalFontIdx (cellRect, lex))
			return FALSE;
		token = lex.LookAhead ();
	}
	if (token == T_ALIGN)
		return ParseTotalAlign (cellRect, lex);

	return lex.ParseSep ();
}

//------------------------------------------------------------------------------
BOOL Table::ParseColumnTitleDifferences (const CRect& cellRect, Token token, ViewParser& lex)
{    
	if (token == T_FONTSTYLE)
	{
		if (!ParseColumnTitleFontIdx (cellRect, lex))
			return FALSE;
		token = lex.LookAhead ();
	}
	if (token == T_ALIGN)
		return ParseColumnTitleAlign (cellRect, lex);

	return lex.ParseSep ();
}

//------------------------------------------------------------------------------
BOOL Table::ParseSubTotalFontIdx (const CRect& cellRect, ViewParser& lex)
{
	CString string;
	
	if (ParseFontIdx(string, lex))
	{
		FontIdx  font = m_pDocument->m_pFontStyles->GetFontIdx(string, FALSE);
		if (font == FNT_ERROR) return lex.SetError(_TB("Font style unknown"), string);

		for (int nCol = cellRect.left; nCol<= cellRect.right; nCol++)   
			SetSubTotalFontIdx (font, nCol);
		return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Table::ParseTotalFontIdx (const CRect& cellRect, ViewParser& lex)
{
	CString string;
	
	if (ParseFontIdx(string, lex))
	{
		FontIdx  font = m_pDocument->m_pFontStyles->GetFontIdx(string, FALSE);
		if (font == FNT_ERROR) return lex.SetError(_TB("Font style unknown"), string);

		for (int nCol = cellRect.left; nCol<= cellRect.right; nCol++)   
			SetTotalFontIdx (font, nCol);
		return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Table::ParseColumnTitleFontIdx (const CRect& cellRect, ViewParser& lex)
{
	CString string;
	
	if (ParseFontIdx(string, lex))
	{
		FontIdx  font = m_pDocument->m_pFontStyles->GetFontIdx(string, FALSE);
		if (font == FNT_ERROR) return lex.SetError(_TB("Font style unknown"), string);

		for (int nCol = cellRect.left; nCol<= cellRect.right; nCol++)   
			SetColumnTitleFontIdx (font, nCol);
		return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------                
BOOL Table::ParseTotalAlign (const CRect& cellRect, ViewParser& lex)
{
	AlignType nAlign; 
	
	if (lex.ParseAlign (nAlign))
	{
		for (int nCol = cellRect.left; nCol<= cellRect.right; nCol++)   
			SetTotalAlign (nAlign, nCol);
		return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------                
BOOL Table::ParseColumnTitleAlign (const CRect& cellRect, ViewParser& lex)
{
	AlignType nAlign; 
	
	if (lex.ParseAlign (nAlign))
	{
		for (int nCol = cellRect.left; nCol<= cellRect.right; nCol++)   
				SetColumnTitleAlign (nAlign, nCol);
		return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------                
BOOL Table::ParseBodyVariations (ViewParser& lex)
{   
	COLORREF allColors[CCellColorsDlg::MAX];
	allColors[CCellColorsDlg::VALUE]		= DEFAULT_CELL_FOREGROUND;
	allColors[CCellColorsDlg::BACKGROUND]	= DEFAULT_CELL_BACKGROUND;

	if (lex.LookAhead(T_BEGIN))
		return
			lex.ParseBegin  ()		&&
			ParseVariations (allColors, lex)	&&
			lex.ParseEnd    ();

	return ParseVariation(allColors, lex, FALSE);
}

//------------------------------------------------------------------------------
BOOL Table::ParseVariations (COLORREF *allColors, ViewParser& lex)
{
	BOOL ok;

	do { ok = ParseVariation(allColors, lex, TRUE) && !lex.Bad() && !lex.Eof();}
	while (ok && !lex.LookAhead(T_END));

	return ok;
}  
//------------------------------------------------------------------------------
BOOL Table::ParseVariation (COLORREF *allColors, ViewParser& lex, BOOL /*blk*/)
{
	switch (lex.LookAhead())
	{
		case T_ALL	:       
		{
			BOOL ok = lex.ParseTag(T_ALL) &&
				ParseColorAttributes(allColors, lex);

			for (int nCol = 0; nCol <= LastColumn(); nCol++)
				for (int nRow = 0; nRow <= LastRow(); nRow++)
					m_Columns[nCol]->m_Cells[nRow]->SetCellColor (allColors);

			//TODO Parse FontStyle for All

			return ok && lex.ParseSep();
		}
		
		case T_RECT	: return ParseRectSet	(allColors, lex);
		
		case T_CELL	: return ParseCellDiff	(lex);

		default		: return lex.SetError(_TB("An unexpected symbol was found"));

	}
}

//------------------------------------------------------------------------------
BOOL Table::ParseColorAttributes (COLORREF* color, ViewParser& lex)
{
	Token   nToken;
	BOOL    anyDiff = FALSE;

	while ( (nToken = lex.LookAhead() )!= T_SEP)
	{
		switch (nToken)
		{
			case T_TEXTCOLOR :
				if (!lex.ParseColor(T_TEXTCOLOR, color[CCellColorsDlg::VALUE]))
					return FALSE;
				anyDiff = TRUE;
				break;
			case T_BKGCOLOR :
				if (!lex.ParseColor(T_BKGCOLOR, color[CCellColorsDlg::BACKGROUND]))
					return FALSE;
				anyDiff = TRUE;
				break;
			default :  return lex.SetError(_TB("An unexpected symbol was found"));
		}
	}
	// checks whether there is no description
	if (!anyDiff)	return lex.SetError(_TB("There is no attribute"));
	return TRUE;
} 
 
//------------------------------------------------------------------------------
BOOL Table::ParseRectSet (COLORREF *allColors, ViewParser& lex)
{   
	CRect rect;
	return
		lex.ParseRect 				(T_RECT, rect) &&
		ParseRectColorAttributes	(allColors, rect, lex) ;
}             

//------------------------------------------------------------------------------
BOOL Table::ParseRectColorAttributes (COLORREF *allColors, CRect& rect, ViewParser& lex)
{       
	COLORREF myColors[CCellColorsDlg::MAX];
	myColors[CCellColorsDlg::VALUE]			= allColors[CCellColorsDlg::VALUE];
	myColors[CCellColorsDlg::BACKGROUND]	= allColors[CCellColorsDlg::BACKGROUND];
	
	int colSize = m_Columns.GetSize();
	int rowSize = m_Columns[0]->m_Cells.GetSize();
	if	(	(rect.left >= colSize) || (rect.right >= colSize) ||
			(rect.top >= rowSize) || (rect.bottom >= rowSize) )
		return lex.SetError(_TB("Cell or column number not belonging to the table"));       

	if (!ParseColorAttributes  (myColors, lex)) 
		return FALSE; 

	for (int nCol = rect.left; nCol <= rect.right; nCol++)
		for (int nRow = rect.top; nRow <= rect.bottom; nRow++)
			m_Columns[nCol]->m_Cells[nRow]->SetCellColor (myColors);
	
	return lex.ParseSep();
}                               
 
//------------------------------------------------------------------------------
BOOL Table::ParseCellDiff (ViewParser& lex)
{  
	CRect cellRect;
	
	return
		ParseCellIndex (&cellRect, lex) &&
		ParseDifferences (cellRect, lex);
}

//------------------------------------------------------------------------------
BOOL Table::ParseCellIndex (CRect* cellRect, ViewParser& lex)
{
	BOOL valid;
	int nTop, nLeft, nBottom, nRight;

	valid = lex.ParseTag	(T_CELL) 	&&
			lex.ParseOpen	() 			&&
			lex.ParseInt	(nTop)		&&
			lex.ParseComma	() 			&& 
			lex.ParseInt	(nLeft);

	cellRect->top  = nTop;
	cellRect->left = nLeft;

	if (!valid) return valid;
	
	switch (lex.LookAhead())
	{
		case T_COMMA    :
			valid =	lex.ParseComma	()			&& 
					lex.ParseInt	(nBottom)	&&
					lex.ParseComma	()			&& 
					lex.ParseInt	(nRight)	&&
					lex.ParseClose	();

			cellRect->bottom	= nBottom;
			cellRect->right		= nRight;

			return valid;

		default			:	
			cellRect->bottom = cellRect->top;
			cellRect->right  = cellRect->left;
			return  lex.ParseClose ();
	}
}                                                       
				 
//------------------------------------------------------------------------------
BOOL Table::ParseColumnIndex (CRect* cellRect, ViewParser& lex)
{
	BOOL valid;
	int nLeft, nRight;

	valid = lex.ParseTag	(T_COLUMN)	&&
			lex.ParseOpen	() 			&&
			lex.ParseInt	(nLeft);

	cellRect->left = nLeft;
	if (!valid) return valid;
   
	switch (lex.LookAhead())
	{
		case T_COMMA    :	
			valid = 
				lex.ParseComma	() 			&& 
				lex.ParseInt	(nRight)	&&
				lex.ParseClose	();

			cellRect->right = nRight;
			return valid;

		default			:	
			cellRect->right  = cellRect->left;
			return  lex.ParseClose ();
	}
}
							
//------------------------------------------------------------------------------
BOOL Table::ParseDifferences (const CRect& cellRect, ViewParser& lex)
{
	Token       token = lex.LookAhead ();
	
	if (token == T_FONTSTYLE)
		if (!ParseCellFontIdx (cellRect, lex))
			return FALSE;
		else token = lex.LookAhead ();
	
	switch (token)
	{
		case T_SEP		: return lex.ParseSep	();
		case T_ALIGN	: return ParseCellAlign (cellRect, lex);
		default			: return lex.SetError(_TB("An unexpected symbol was found"));
	} 
	
}

//------------------------------------------------------------------------------
BOOL Table::ParseCellFontIdx (const CRect& cellRect, ViewParser& lex)
{
	CString string;
	
	if (ParseFontIdx(string, lex))
	{
		FontIdx  font = m_pDocument->m_pFontStyles->GetFontIdx(string, FALSE);
		if (font == FNT_ERROR) return lex.SetError(_TB("Font style unknown"), string);

		if (cellRect.top == 0 && cellRect.bottom == LastRow())
			for (int nCol = cellRect.left; nCol<= cellRect.right; nCol++)
				SetColumnFontIdx (font, nCol);
		else
			for (int nCol = cellRect.left; nCol<= cellRect.right; nCol++)   
				for (int nRow = cellRect.top; nRow <= cellRect.bottom; nRow++)
					SetCellFontIdx (font, nRow, nCol);

		return TRUE;
	}
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL Table::ParseFontIdx (CString& fontStr, ViewParser& lex)
{
	return  lex.ParseTag	(T_FONTSTYLE) &&
			lex.ParseString	(fontStr);
}

//------------------------------------------------------------------------------
BOOL Table::ParseCellAlign (const CRect& cellRect, ViewParser& lex)
{
	AlignType nAlign; 
	
	if (lex.ParseAlign (nAlign))
	{
		if (cellRect.top == 0 && cellRect.bottom == LastRow())
			for (int nCol = cellRect.left; nCol<= cellRect.right; nCol++)
				SetColumnAlign (nAlign, nCol);
		else
			for (int nCol = cellRect.left; nCol<= cellRect.right; nCol++)
				for (int nRow = cellRect.top; nRow <= cellRect.bottom; nRow++)
					SetCellAlign (nAlign, nRow, nCol);

			return TRUE;
	}
	return FALSE;
}
