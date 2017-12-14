#pragma once

#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\FontsTable.h>
#include <TbParser\Parser.h>

#include "WoormIni.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//===========================================================================
class FormatStyleTable;
class FontStyleTable;
class Borders;
class BorderPen;

class CBarCode;

//===========================================================================
class TB_EXPORT ViewParser : public Parser
{
public:
	// constructors
	ViewParser	(const TToken* userTokenTable = NULL)
		: Parser (userTokenTable) {}

	ViewParser	(LPCTSTR aString, BOOL bAllowOpenWhenEmpty)
		: Parser (aString, NULL, -1, bAllowOpenWhenEmpty) {}

    // useful parse functions
	BOOL ParseRect			(Token, CRect&);
	BOOL ParseRect			(Token, CRect&, int&, int&);
	BOOL ParseRect			(CRect&);
	BOOL ParseRatio			(int&, int&);
	BOOL ParsePen			(BorderPen&);
	BOOL ParseBorders		(Borders&);
	BOOL ParseTextColor		(COLORREF&);
	BOOL ParseBkgColor		(COLORREF&);
	BOOL ParseAlign			(AlignType&);
	BOOL ParseExtendedAlign (AlignType&);
	BOOL ParseWidth			(int&);
	BOOL ParseSize			(int&);
	BOOL ParseFont			(FontStyleTable*, FontIdx&);
	BOOL ParseFormat		(FormatStyleTable*, FormatIdx&, BOOL bCheckTag = TRUE);
	BOOL ParseTR			(Token tag, int& row, int& col);
	BOOL ParseTable			(int&, int&);
	BOOL ParseRepeater		(int&, int&);
	BOOL ParseOrigin		(CPoint&);
	BOOL ParseHeights		(int&, int&, int&, int&);
	BOOL ParseColumn		(CString&);
	BOOL ParseBarCode		(CBarCode*);
};

//===========================================================================
class TB_EXPORT ViewUnparser : public Unparser
{
public:
	BOOL		m_bSaveAsWithCurrentLanguage;
	CString		m_strOriginFileName;
	CString		m_strOriginDictionaryPath;

public:
	
	// Constructors
	ViewUnparser(BOOL bUseMemFile = FALSE) 
		: 
		Unparser (bUseMemFile), 
		m_bSaveAsWithCurrentLanguage (FALSE) 			
		{
			SetFormat(UTF8);
		}

	ViewUnparser(LPCTSTR pszFileName)	
		: 
		Unparser (pszFileName), 
		m_bSaveAsWithCurrentLanguage (FALSE) 
		{
			SetFormat(UTF8);
		}

    // useful unparse functions
	void UnparseHeader		();
	void UnparseRect		(Token tag, const CRect&,				BOOL newline = TRUE);
	void UnparseRect		(const CRect&,							BOOL newline = TRUE);
	void UnparseRatio		(const int&, const int&,				BOOL newline = TRUE);
	void UnparsePen			(const BorderPen&,						BOOL newline = TRUE);
	void UnparseBorders		(const Borders&,						BOOL newline = TRUE, BOOL sep = TRUE);
	void UnparseColumn		(const CString&, 						BOOL newline = TRUE);
	void UnparseTextColor	(const COLORREF&,						BOOL newline = TRUE);
	void UnparseBkgColor	(const COLORREF&,						BOOL newline = TRUE);
	void UnparseAlign		(const AlignType&,						BOOL newline = TRUE);
	//void UnparseExtendedAlign (const AlignType&,	const int& nExAl,	 BOOL newline = TRUE);
	void UnparseWidth		(const int&,							BOOL newline = TRUE);
	void UnparseSize		(const int&,							BOOL newline = TRUE);
	void UnparseFont		(FontStyleTable*, const FontIdx&,		BOOL newline = TRUE);
	void UnparseFormat		(FormatStyleTable*, const FormatIdx&,	BOOL newline = TRUE);
	void UnparseTR			(Token tag, const int&, const int&,				BOOL newline = TRUE);
	void UnparseTable		(const int&, const int&,				BOOL newline = TRUE);
	void UnparseRepeater	(const int&, const int&,				BOOL newline = TRUE);
	void UnparseOrigin		(const CPoint&,							BOOL newline = TRUE);
	void UnparseHeights		(const int&, const int&, const int&, const int&,BOOL newline = TRUE);
	void UnparseBarCode 	(CBarCode*, BOOL newline = TRUE);

	virtual void UnparseExpr (LPCTSTR, BOOL newline = TRUE);
	virtual BOOL IsLocalizableTextInCurrentLanguage() { return m_bSaveAsWithCurrentLanguage; }
	virtual CString LoadReportString(const CString& sText);
};

#include "endh.dex"
