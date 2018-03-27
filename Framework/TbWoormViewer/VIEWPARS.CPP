
#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>

#include <TbGenlib\baseapp.h>

#include "bordpen.h"
#include "woormini.h"
#include "listdlg.h"
#include "textobj.h"
#include "viewpars.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//==============================================================================
//          generic parse and deparse function
//==============================================================================

//------------------------------------------------------------------------------
BOOL ViewParser::ParseRect (Token token, CRect& rect)
{
	int top=0, left=0, right=0, bottom=0;

	BOOL bOK=
		ParseTag        (token)         &&
		ParseOpen       ()              &&
		ParseSignedInt  (top)           &&
		ParseComma      ()              &&
		ParseSignedInt  (left)          &&
		ParseComma      ()              &&
		ParseSignedInt  (bottom)        &&
		ParseComma      ()              &&
		ParseSignedInt  (right)         &&
		ParseClose      ();

	rect.top = top;
	rect.left = left;
	rect.right = right;
	rect.bottom = bottom;

	return bOK;
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseRect (CRect& rect)
{
	return ParseRect (T_RECT, rect);
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseRatio (int& nHRatio, int& nVRatio)
{
	nHRatio  = 0;
	nVRatio  = 0;

	if (LookAhead(T_RATIO))
		if (!(
				ParseTag    (T_RATIO)   &&
				ParseOpen   ()          &&
				ParseInt    (nHRatio)    &&
				ParseComma  ()          &&
				ParseInt    (nVRatio)    &&
				ParseClose  ()
			))
			return FALSE;

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParsePen (BorderPen& pen)
{                                     
	COLORREF rgbColor;
	if (!ParseColor (T_PEN, rgbColor)) return FALSE;
	
	pen.SetColor(rgbColor);
	pen.SetWidth(DEFAULT_PEN_WIDTH);

	if (LookAhead(T_SIZE))
	{
		int width = DEFAULT_PEN_WIDTH;	// warning in Nt value is long
		if (!ParseSize(width)) return FALSE;
		pen.SetWidth(width);
	}
	return ParseSep();
}


//------------------------------------------------------------------------------
BOOL ViewParser::ParseBorders (Borders& brd)
{
	BOOL bOk = 
		ParseTag    (T_BORDERS)     &&
		ParseOpen   ()              &&
		ParseInt    (brd.top)       &&
		ParseComma  ()              &&
		ParseInt    (brd.left)      &&
		ParseComma  ()              &&
		ParseInt    (brd.bottom)    &&
		ParseComma  ()              &&
		ParseInt    (brd.right)     &&
		ParseClose  () ;

	if (this->LookAhead(T_COMMA))
		return bOk;
	else  
		return ParseSep();
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseTextColor (COLORREF& rgbColor, BOOL parseSep/* = TRUE*/)
{
	return
		ParseColor  (T_TEXTCOLOR, rgbColor) &&
		(parseSep ? ParseSep() : TRUE);
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseBkgColor (COLORREF& rgbColor, BOOL parseSep/* = TRUE*/)
{
	return
		ParseColor  (T_BKGCOLOR, rgbColor) &&
		(parseSep ?  ParseSep    () : TRUE);
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseAlign (AlignType& nAlign)
{
	return
		ParseTag    (T_ALIGN)   &&
		ParseDWord   (nAlign)    &&
		ParseSep    ();
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseExtendedAlign (AlignType& nAlign)
{
	BOOL bOk =
		ParseTag    (T_ALIGN)   &&
		ParseDWord   (nAlign);
	
	if (!bOk)
		return FALSE;

	if (LookAhead(T_INT))
	{
		int nExAl = 0;
		bOk = ParseInt (nExAl);
		if (nExAl == 0)
			nAlign &= ~DT_EX_VCENTER_LABEL;
	}	

	return bOk && ParseSep ();
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseWidth (int& width)
{
	return
		ParseTag    (T_WIDTH)   &&
		ParseInt    (width);
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseSize (int& size)
{
	return
		ParseTag    (T_SIZE)    &&
		ParseInt    (size);
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseFont (FontStyleTable* pFontStyles, FontIdx& nIndex)
{
	CString strStyleName;
	if (!(
		ParseTag(T_FONTSTYLE)   &&
		ParseString(strStyleName)
		))
		return FALSE;

	// disabilito l'inizializzazione del default font
	nIndex = pFontStyles->GetFontIdx(strStyleName, FALSE);
	if (nIndex == FNT_ERROR) 
		return SetError(_TB("Font style unknown"), strStyleName);

	return ParseSep();
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseTR (Token tag, int& row, int& col)
{
	return
		ParseTag    (tag)   &&
		ParseOpen   ()          &&
		ParseInt    (row)       &&
		ParseComma  ()          &&
		ParseInt    (col)       &&
		ParseClose  ();
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseTable (int& row, int& col)
{
	return ParseTR (T_TABLE, row, col);
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseRepeater (int& row, int& col)
{
	return ParseTR (T_REPEATER, row, col);
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseColumn (CString& strTitle)
{
	return ParseString(strTitle);
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseOrigin (CPoint& ptOrigin)
{
	int x = 0;
	int y = 0;

	BOOL bOK=
		ParseTag        (T_ORIGIN)  &&
		ParseOpen       ()          &&
		ParseSignedInt  (x)         &&
		ParseComma      ()          &&
		ParseSignedInt  (y)         &&
		ParseClose      ();

	ptOrigin.x = x;
	ptOrigin.y = y;

	return bOK;
}


//------------------------------------------------------------------------------
BOOL ViewParser::ParseHeights
	(
		int&    nTitle,
		int&    nColumnTitle,
		int&    nCell,
		int&    nTotal
	)
{
	return
		ParseTag    (T_HEIGHTS)		&&
		ParseOpen   ()				&&
		ParseInt    (nTitle)		&&
		ParseComma  ()				&&
		ParseInt    (nColumnTitle)  &&
		ParseComma  ()          	&&
		ParseInt    (nCell)			&&
		ParseComma  ()          	&&
		ParseInt    (nTotal)		&&
		ParseClose  ();
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseFormat (FormatStyleTable* pFormatStyles, FormatIdx& nIndex, BOOL bCheckTag)
{
	CString strStyleName;

	if (bCheckTag && !LookAhead(T_FORMATSTYLE))
	{
		nIndex = 0; // default to TEXT //NO_FORMAT;
		return TRUE;
	}

	if (bCheckTag && !ParseTag(T_FORMATSTYLE))
		return FALSE;

	if (!ParseString(strStyleName))
		return FALSE;

	nIndex = pFormatStyles->GetFormatIdx(strStyleName);
	if (nIndex < 0) 
		return SetError(_TB("Format style unknown"), strStyleName);
	return nIndex >= 0;
}

//------------------------------------------------------------------------------
BOOL ViewParser::ParseBarCode (CBarCode* pBarCode)
{
	return pBarCode->Parse(*this);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseHeader ()
{
	UnparseTag  (T_COMMENT_SEP);
	UnparseTag  (T_COPYRIGHT);
	UnparseTag  (T_COMMENT_SEP);
	UnparseCrLf ();
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseRect (Token tag, const CRect& rect, BOOL bNewline)
{
	UnparseTag      (tag,           FALSE);
	UnparseOpen     ();
	UnparseInt      (rect.top,      FALSE);
	UnparseComma    ();
	UnparseInt      (rect.left,     FALSE);
	UnparseComma    ();
	UnparseInt      (rect.bottom,   FALSE);
	UnparseComma    ();
	UnparseInt      (rect.right,    FALSE);
	UnparseClose    (FALSE);

	if (bNewline)
		UnparseCrLf();
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseRect (const CRect& rect, BOOL bNewline)
{
	UnparseRect (T_RECT, rect, bNewline);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparsePen (const BorderPen& pen, BOOL bNewline)
{
	UnparseColor    (T_PEN, pen.GetColor(), FALSE);

	if (pen.GetWidth() != DEFAULT_PEN_WIDTH)
	{
		UnparseTag      (T_SIZE,           FALSE);
		UnparseInt      (pen.GetWidth(),   FALSE);
	}
	UnparseSep      ();

	if (bNewline)
		UnparseCrLf ();
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseRatio (const int& nHRatio, const int& nVRatio, BOOL bNewline)
{
	if ((nHRatio != 0) || (nVRatio != 0))
	{
		UnparseTag      (T_RATIO,   FALSE);
		UnparseOpen     ();
		UnparseInt      (nHRatio,    FALSE);
		UnparseComma    ();
		UnparseInt      (nVRatio,    FALSE);
		UnparseClose    (FALSE);
	}

	if (bNewline)
		UnparseCrLf ();
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseBorders (const Borders& brd, BOOL bNewline, BOOL bSep)
{
	UnparseTag  (T_BORDERS, FALSE);
	UnparseOpen ();
	UnparseInt  (brd.top,       FALSE);
	UnparseComma();
	UnparseInt  (brd.left,      FALSE);
	UnparseComma();
	UnparseInt  (brd.bottom,    FALSE);
	UnparseComma();
	UnparseInt  (brd.right,     FALSE);
	UnparseClose(FALSE);

	if (bSep)
		UnparseSep  (bNewline);
	else if (bNewline)
		UnparseCrLf();
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseTextColor (const COLORREF& rgbColor, BOOL bNewline)
{
	UnparseColor    (T_TEXTCOLOR, rgbColor, FALSE);
	UnparseSep      (bNewline);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseBkgColor (const COLORREF& rgbColor, BOOL bNewline)
{
	UnparseColor    (T_BKGCOLOR, rgbColor, FALSE);
	UnparseSep      (bNewline);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseAlign (const AlignType& nAlign, BOOL bNewline)
{
	UnparseTag      (T_ALIGN,   FALSE);
	UnparseInt      (nAlign,     FALSE);
	UnparseBlank    ();
	UnparseSep      (bNewline);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseWidth (const int& width, BOOL bNewline)
{
	UnparseTag      (T_WIDTH,   FALSE);
	UnparseWord     (width,     FALSE);
	UnparseBlank    (bNewline);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseSize (const int& size, BOOL bNewline)
{
	UnparseTag      (T_SIZE,    FALSE);
	UnparseWord     (size,      FALSE);
	UnparseBlank    (bNewline);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseFont
	(
				FontStyleTable* pFontStyles,
		const   FontIdx&        nIndex,
				BOOL            bNewline
	)
{
	UnparseTag      (T_FONTSTYLE,   	                FALSE);
	UnparseString   (pFontStyles->GetStyleName(nIndex), FALSE);
	UnparseSep      (bNewline);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseFormat
	(
				FormatStyleTable*   pFormatStyles,
		const   FormatIdx&          nIndex,
				BOOL                bNewline
	)
{
	if (nIndex)
	{
		UnparseTag      (T_FORMATSTYLE, 	                    FALSE);
		UnparseString   (pFormatStyles->GetStyleName(nIndex),   bNewline);
	}
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseTR (Token tag, const int& row, const int& col, BOOL bNewline)
{
	UnparseTag  (tag,   FALSE);
	UnparseOpen ();
	UnparseInt  (row,       FALSE);
	UnparseComma();
	UnparseInt  (col,       FALSE);
	UnparseClose(bNewline);
}

void ViewUnparser::UnparseTable (const int& row, const int& col, BOOL bNewline)
{
	UnparseTR (T_TABLE, row, col, bNewline);
}

void ViewUnparser::UnparseRepeater (const int& row, const int& col, BOOL bNewline)
{
	UnparseTR (T_REPEATER, row, col, bNewline);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseColumn (const CString& strTitle, BOOL bNewline)
{
	UnparseString   (strTitle, bNewline);
}


//------------------------------------------------------------------------------
void ViewUnparser::UnparseOrigin
	(
		const CPoint&   ptOrigin,
		BOOL            bNewline
	)
{
	UnparseTag  (T_ORIGIN,  FALSE);
	UnparseOpen ();
	UnparseInt  (ptOrigin.x,  FALSE);
	UnparseComma();
	UnparseInt  (ptOrigin.y,  FALSE);
	UnparseClose(bNewline);
}


//------------------------------------------------------------------------------
void ViewUnparser::UnparseHeights
	(
		const int&      nTitle,
		const int&      nColumnTitle,
		const int&      nCell,
		const int&      nTotal,
		BOOL            bNewline
	)
{
	UnparseTag  (T_HEIGHTS, FALSE);
	UnparseOpen ();
	UnparseInt  (nTitle,     FALSE);
	UnparseComma();
	UnparseInt  (nColumnTitle,  FALSE);
	UnparseComma();
	UnparseInt  (nCell,      FALSE);
	UnparseComma();
	UnparseInt  (nTotal,     FALSE);
	UnparseClose(bNewline);
}

//------------------------------------------------------------------------------
void ViewUnparser::UnparseBarCode (CBarCode* pBarCode, BOOL bNewline)
{
	pBarCode->Unparse  (*this, bNewline);
}

//-----------------------------------------------------------------------------
CString ViewUnparser::LoadReportString(const CString& sText)
{
	if (!m_bSaveAsWithCurrentLanguage)
		return sText;

	return AfxLoadReportString(sText, m_strOriginFileName, m_strOriginDictionaryPath);
}

//-----------------------------------------------------------------------------
void ViewUnparser::UnparseExpr (LPCTSTR szE, BOOL newline /*= TRUE*/)
{
	if (!m_bSaveAsWithCurrentLanguage)
	{
		Unparser::UnparseExpr(szE, newline);
		return;
	}

	CString ws(szE);
	BOOL bLoc = FALSE;

	Parser lex(ws);
	lex.EnableAuditString(TRUE);

	do 
	{
		CString word, locword;

		if (lex.LookAhead(T_FLOCALIZE))
		{
			CString sAudit = lex.GetAuditString();
			lex.EnableAuditString(FALSE);

			if (!lex.Match(T_FLOCALIZE) || !lex.Match(T_ROUNDOPEN) || !lex.ParseString(word))
			{
				bLoc = FALSE;
				break;
			}

			if (!word.IsEmpty())
			{
				locword = LoadReportString(word);
			}

			if (!lex.Match(T_ROUNDCLOSE))
			{
				bLoc = FALSE;
				break;
			}

			int nSingle = locword.Find('\'');
			int nDouble = locword.Find('\"');

			if (nSingle != -1 && nDouble == -1)
			{ 
				locword = cwsprintf(_T("\"%s\""), locword);
			}
			else if (nSingle == -1 && nDouble != -1)
			{ 
				locword = cwsprintf(_T("\'%s\'"), locword);
			}
			else if (nSingle != -1 && nDouble != -1)
			{
				bLoc = FALSE;
				break;
			}
			else // if (nSingle == -1 && nDouble == -1)
			{
				locword = cwsprintf(_T("\"%s\""), locword);
			}

			lex.EnableAuditString(TRUE);
			lex.ConcatAuditString(sAudit + locword);
			bLoc = TRUE;
		}
		else
			lex.SkipToken();
	}
	while (!lex.LookAhead(T_EOF));

	return Unparser::UnparseExpr(bLoc ? lex.GetAuditString() : szE, newline);
}
