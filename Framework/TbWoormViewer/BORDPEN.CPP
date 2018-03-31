
#include "stdafx.h"

#include <TbParser\TokensTable.h>

#include "woormini.h"

#include "bordpen.h"

#include "viewpars.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//=============================================================================
//	Class BorderPen implementation
//=============================================================================

//---------------------------------------------------------------------------
BorderPen::BorderPen ()
{
	m_nWidth = DEFAULT_PEN_WIDTH;
	m_rgbColor = RGB(0, 0, 0);
}

//---------------------------------------------------------------------------
BorderPen::BorderPen (COLORREF rgbColor)
{
	m_nWidth = DEFAULT_PEN_WIDTH;
	m_rgbColor = rgbColor;
}

//---------------------------------------------------------------------------
BorderPen::BorderPen (int nWidth, COLORREF rgbColor)
{
	m_nWidth = nWidth;
	m_rgbColor = rgbColor;
}


//------------------------------------------------------------------------------
BorderPen::BorderPen(const BorderPen& source)
{
	m_nWidth = source.m_nWidth;
	m_rgbColor = source.m_rgbColor;
}

//---------------------------------------------------------------------------
BOOL BorderPen::IsDefault () const
{
	return
		(m_nWidth	== DEFAULT_PEN_WIDTH) &&
		(m_rgbColor	== RGB(0,0,0));
}

//------------------------------------------------------------------------------
BorderPen& BorderPen::operator = (const BorderPen& source)
{
	m_nWidth = source.m_nWidth;
	m_rgbColor = source.m_rgbColor;

	return *this;
}

//------------------------------------------------------------------------------
BOOL  BorderPen::operator != (const BorderPen& source) const
{
	return m_nWidth != source.m_nWidth ||
			m_rgbColor != source.m_rgbColor;
}

//------------------------------------------------------------------------------
BOOL  BorderPen::operator == (const BorderPen& source) const
{
	return m_nWidth == source.m_nWidth &&
			m_rgbColor == source.m_rgbColor;
}
//==============================================================================
//          Class Borders implementation
//==============================================================================

//------------------------------------------------------------------------------
Borders::Borders(BOOL bEnabled)
{
	top     = bEnabled;
	left    = bEnabled;
	bottom  = bEnabled;
	right   = bEnabled;
}

//------------------------------------------------------------------------------
Borders::Borders(BOOL aTop, BOOL aLeft, BOOL aBottom, BOOL aRight)
{
	top     = aTop;
	left    = aLeft;
	bottom  = aBottom;
	right   = aRight;
}

//------------------------------------------------------------------------------
Borders::Borders(const Borders& source)
{
	top     = source.top;
	left    = source.left;
	bottom  = source.bottom;
	right   = source.right;
}

//------------------------------------------------------------------------------
BOOL Borders::operator == (const Borders& brd) const
{
	return
	(
		(top    == brd.top)     &&
		(left   == brd.left)    &&
		(bottom == brd.bottom)  &&
		(right  == brd.right)
	);
}

//------------------------------------------------------------------------------
BOOL Borders::operator != (const Borders& brd) const
{
	return !(*this == brd);
}

//------------------------------------------------------------------------------
Borders& Borders::operator = (const Borders& source)
{
	top     = source.top;
	left    = source.left;
	bottom  = source.bottom;
	right   = source.right;

	return *this;
}

//------------------------------------------------------------------------------
void Borders::Update(CString sB)
{
	sB.MakeUpper();
	sB.Remove(' ');

	int idx;
	if ((idx = sB.Find(_T("LEFT"))) >= 0)
		left = idx == 0 ? TRUE : sB[idx - 1] != '-';
	if ((idx = sB.Find(_T("RIGHT"))) >= 0)
		right = idx == 0 ? TRUE : sB[idx - 1] != '-';
	if ((idx = sB.Find(_T("TOP"))) >= 0)
		top = idx == 0 ? TRUE : sB[idx - 1] != '-';
	if ((idx = sB.Find(_T("BOTTOM"))) >= 0)
		bottom = idx == 0 ? TRUE : sB[idx - 1] != '-';
}

//==============================================================================
//          Class TableBorders implementation
//==============================================================================
//------------------------------------------------------------------------------
TableBorders::TableBorders()
{
	Init(TRUE);

	m_bRowSeparator = FALSE;
	m_bRowSeparatorDynamic = FALSE;
	m_bTableTitleBottom = m_bColumnTitleBottom = m_bTotalTop = FALSE;
}

//------------------------------------------------------------------------------
TableBorders::TableBorders(BOOL bEnabled)
{
	Init(bEnabled);

	m_bRowSeparator = FALSE;
	m_bRowSeparatorDynamic = FALSE;
	m_bTableTitleBottom = m_bColumnTitleBottom = m_bTotalTop = FALSE;
}

//------------------------------------------------------------------------------
TableBorders::TableBorders (const TableBorders& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
BOOL TableBorders::operator == (const TableBorders& source) const
{
	return
		(
			m_bTableTitleTop		==	source.m_bTableTitleTop &&
			m_bTableTitleBottom		==	source.m_bTableTitleBottom &&
			m_bTableTitleLeft		==	source.m_bTableTitleLeft &&
			m_bTableTitleRight		==	source.m_bTableTitleRight &&

			m_bColumnTitleTop		==	source.m_bColumnTitleTop &&
			m_bColumnTitleBottom	==	source.m_bColumnTitleBottom &&
			m_bColumnTitleLeft		==	source.m_bColumnTitleLeft &&
			m_bColumnTitleRight		==	source.m_bColumnTitleRight &&

			m_bColumnTitleSeparator	==	source.m_bColumnTitleSeparator &&
			m_bColumnSeparator		==	source.m_bColumnSeparator &&

			m_bRowSeparator			==	source.m_bRowSeparator &&
			m_bRowSeparatorDynamic	==	source.m_bRowSeparatorDynamic &&

			m_bBodyTop				==	source.m_bBodyTop &&
			m_bBodyLeft				==	source.m_bBodyLeft &&
			m_bBodyBottom			==	source.m_bBodyBottom &&
			m_bBodyRight			==	source.m_bBodyRight &&

			m_bTotalTop				==	source.m_bTotalTop &&
			m_bTotalBottom			==	source.m_bTotalBottom &&
			m_bTotalLeft			==	source.m_bTotalLeft &&
			m_bTotalRight			==	source.m_bTotalRight &&

			(m_pRowSepPen && source.m_pRowSepPen ?
				*m_pRowSepPen == *source.m_pRowSepPen :
				m_pRowSepPen == source.m_pRowSepPen)	&&	
	
			(m_pInterlinePen && source.m_pInterlinePen ?
				*m_pInterlinePen == *source.m_pInterlinePen :
				m_pInterlinePen == source.m_pInterlinePen) &&

			(m_pColumnSepPen && source.m_pColumnSepPen ?
			*m_pColumnSepPen == *source.m_pColumnSepPen :
			m_pColumnSepPen == source.m_pColumnSepPen) &&

			(m_pColumnTitleSepPen && source.m_pColumnTitleSepPen ?
				*m_pColumnTitleSepPen == *source.m_pColumnTitleSepPen :
				m_pColumnTitleSepPen == source.m_pColumnTitleSepPen)
			);
}

//------------------------------------------------------------------------------
BOOL TableBorders::operator != (const TableBorders& brd) const
{
	return !(*this == brd);
}

//------------------------------------------------------------------------------
TableBorders& TableBorders::operator = (const TableBorders& source)
{
	m_bTableTitleTop		=	source.m_bTableTitleTop;
	m_bTableTitleBottom		=	source.m_bTableTitleBottom;
	m_bTableTitleLeft		=	source.m_bTableTitleLeft;
	m_bTableTitleRight		=	source.m_bTableTitleRight;

	m_bColumnTitleTop		=	source.m_bColumnTitleTop;
	m_bColumnTitleBottom	=	source.m_bColumnTitleBottom;
	m_bColumnTitleLeft		=	source.m_bColumnTitleLeft;
	m_bColumnTitleRight		=	source.m_bColumnTitleRight;

	m_bColumnTitleSeparator	=	source.m_bColumnTitleSeparator;
	m_bColumnSeparator		=	source.m_bColumnSeparator;

	m_bRowSeparator			=	source.m_bRowSeparator;
	m_bRowSeparatorDynamic	=	source.m_bRowSeparatorDynamic;

	m_bBodyTop				=	source.m_bBodyTop;
	m_bBodyBottom			=	source.m_bBodyBottom;
	m_bBodyLeft				=	source.m_bBodyLeft;
	m_bBodyRight			=	source.m_bBodyRight;

	m_bTotalTop				=	source.m_bTotalTop;
	m_bTotalBottom			=	source.m_bTotalBottom;
	m_bTotalLeft			=	source.m_bTotalLeft;
	m_bTotalRight			=	source.m_bTotalRight;

	SAFE_DELETE(m_pRowSepPen);
	if (source.m_pRowSepPen)
		m_pRowSepPen = new BorderPen(*source.m_pRowSepPen);

	SAFE_DELETE(m_pInterlinePen);
	if (source.m_pInterlinePen)
		m_pInterlinePen = new BorderPen(*source.m_pInterlinePen);

	SAFE_DELETE(m_pColumnSepPen);
	if (source.m_pColumnSepPen)
		m_pColumnSepPen = new BorderPen(*source.m_pColumnSepPen);

	SAFE_DELETE(m_pColumnTitleSepPen);
	if (source.m_pColumnTitleSepPen)
		m_pColumnTitleSepPen = new BorderPen(*source.m_pColumnTitleSepPen);

	return *this;
}

//------------------------------------------------------------------------------
void TableBorders::Init(BOOL bEnabled)
{
	m_bTableTitleTop   = bEnabled;
	m_bTableTitleLeft  = bEnabled;
	m_bTableTitleRight = bEnabled;

	m_bColumnTitleTop      = bEnabled;
	m_bColumnTitleLeft     = bEnabled;
	m_bColumnTitleRight    = bEnabled;

	m_bColumnTitleSeparator= bEnabled;
	m_bColumnSeparator		= bEnabled;

	m_bBodyTop     = bEnabled;
	m_bBodyBottom  = bEnabled;
	m_bBodyLeft    = bEnabled;
	m_bBodyRight   = bEnabled;

	m_bTotalLeft   = bEnabled;
	m_bTotalBottom = bEnabled;
	m_bTotalRight  = bEnabled;
}

//------------------------------------------------------------------------------
BOOL TableBorders::IsDefault() const
{
	return
		m_bTableTitleTop       &&
		!m_bTableTitleBottom   &&
		m_bTableTitleLeft      &&
		m_bTableTitleRight     &&

		m_bColumnTitleTop      &&
		!m_bColumnTitleBottom  &&
		m_bColumnTitleLeft     &&
		m_bColumnTitleRight    &&

		m_bColumnTitleSeparator &&
		m_bColumnSeparator     &&

		!m_bRowSeparator       &&
		!m_bRowSeparatorDynamic &&
		!m_pRowSepPen			&&

		m_bBodyTop             &&
		m_bBodyBottom          &&
		m_bBodyLeft            &&
		m_bBodyRight           &&

		!m_bTotalTop	       &&
		m_bTotalBottom         &&
		m_bTotalLeft           &&
		m_bTotalRight;
}

//------------------------------------------------------------------------------
int TableBorders::Changed() const
{
	int counter = 0;

	if (!m_bTableTitleTop)     counter++;
	if (m_bTableTitleBottom)   counter++;
	if (!m_bTableTitleLeft)    counter++;
	if (!m_bTableTitleRight)   counter++;

	if (!m_bColumnTitleTop)        counter++;
	if (m_bColumnTitleBottom)      counter++;
	if (!m_bColumnTitleLeft)       counter++;
	if (!m_bColumnTitleRight)      counter++;

	if (!m_bColumnTitleSeparator)  counter++;
	if (!m_bColumnSeparator)   counter++;

	if (m_bRowSeparator || m_bRowSeparatorDynamic) counter++;

	if (!m_bBodyTop)       counter++;
	if (!m_bBodyBottom)    counter++;
	if (!m_bBodyLeft)      counter++;
	if (!m_bBodyRight)     counter++;

	if (m_bTotalTop)	   counter++;
	if (!m_bTotalBottom)   counter++;
	if (!m_bTotalLeft)     counter++;
	if (!m_bTotalRight)    counter++;

	return counter;
}


//------------------------------------------------------------------------------
void TableBorders::Unparse (ViewUnparser& ofile)
{
	if (IsDefault()) return;

	BOOL blk = (Changed() > 1);

	ofile.UnparseTag    (T_BORDERS, FALSE);
	ofile.UnparseBlank  (blk);

	if (blk)                    ofile.UnparseBegin  ();
	if (!m_bTableTitleTop)         ofile.UnparseTag    (T_NO_TTT);
	if (m_bTableTitleBottom)       ofile.UnparseTag    (T_TITLE_BOTTOM);
	if (!m_bTableTitleLeft)        ofile.UnparseTag    (T_NO_TTL);
	if (!m_bTableTitleRight)       ofile.UnparseTag    (T_NO_TTR);

	if (!m_bColumnTitleTop)        ofile.UnparseTag    (T_NO_CTT);
	if (m_bColumnTitleBottom)      ofile.UnparseTag    (T_COLTITLE_BOTTOM);
	if (!m_bColumnTitleLeft)       ofile.UnparseTag    (T_NO_CTL);
	if (!m_bColumnTitleRight)      ofile.UnparseTag    (T_NO_CTR);

	if (!m_bColumnTitleSeparator)  ofile.UnparseTag    (T_NO_CTS);
	if (!m_bColumnSeparator)       ofile.UnparseTag    (T_NO_CSE);

	if (m_bRowSeparator || m_bRowSeparatorDynamic)   
	{
		ofile.UnparseTag    (T_ROWSEP, FALSE);
		if (m_bRowSeparatorDynamic)
			ofile.UnparseTag (T_DYNAMIC, FALSE);
		if (m_pRowSepPen)
		{
			ofile.UnparsePen(*m_pRowSepPen, FALSE);
		}
		ofile.UnparseCrLf();
	}

	if (!m_bBodyTop)               ofile.UnparseTag    (T_NO_BOT);
	if (!m_bBodyBottom)            ofile.UnparseTag    (T_NO_BOB);
	if (!m_bBodyLeft)              ofile.UnparseTag    (T_NO_BOL);
	if (!m_bBodyRight)             ofile.UnparseTag    (T_NO_BOR);

	if (m_bTotalTop)			ofile.UnparseTag    (T_TOTAL_TOP);
	if (!m_bTotalBottom)        ofile.UnparseTag    (T_NO_TOB);
	if (!m_bTotalLeft)			ofile.UnparseTag    (T_NO_TOL);
	if (!m_bTotalRight)         ofile.UnparseTag    (T_NO_TOR);
	if (blk)					ofile.UnparseEnd    ();
}

//------------------------------------------------------------------------------
BOOL TableBorders::ParseBorder(ViewParser& lex, BOOL blk)
{
	BOOL ok = TRUE;

	do
	{
		switch (lex.LookAhead())
		{
			case T_EOF      : lex.SetError (_TB("Unexpected end file")); 	ok  = FALSE;    break;
			case T_NO_TTT   : ok = lex.SkipToken(); m_bTableTitleTop       = FALSE;    break;
			case T_TITLE_BOTTOM   : ok = lex.SkipToken(); m_bTableTitleBottom      = TRUE;    break;
			case T_NO_TTR   : ok = lex.SkipToken(); m_bTableTitleRight     = FALSE;    break;
			case T_NO_TTL   : ok = lex.SkipToken(); m_bTableTitleLeft      = FALSE;    break;

			case T_NO_CTT   : ok = lex.SkipToken(); m_bColumnTitleTop      = FALSE;    break;
			case T_COLTITLE_BOTTOM   : ok = lex.SkipToken(); m_bColumnTitleBottom      = TRUE;    break;
			case T_NO_CTL   : ok = lex.SkipToken(); m_bColumnTitleLeft     = FALSE;    break;
			case T_NO_CTR   : ok = lex.SkipToken(); m_bColumnTitleRight    = FALSE;    break;

			case T_NO_CTS   : 
			{
				lex.SkipToken();
				m_bColumnTitleSeparator = FALSE;

				if (lex.LookAhead(T_PEN))
				{
					m_pColumnTitleSepPen = new BorderPen();
					ok = lex.ParsePen(*m_pColumnTitleSepPen);
					if (ok && m_pColumnTitleSepPen->m_nWidth > 0)
						m_bColumnTitleSeparator = TRUE;
				}
				break;
			}

			case T_NO_CSE   : 
			{
				lex.SkipToken();
				m_bColumnSeparator = FALSE;

				if (lex.LookAhead(T_PEN))
				{
					m_pColumnSepPen = new BorderPen();
					ok = lex.ParsePen(*m_pColumnSepPen);
					if (ok && m_pColumnSepPen->m_nWidth > 0)
						m_bColumnSeparator = TRUE;
				}
				break;
			}

			case T_ROWSEP   : 
			{
				lex.SkipToken(); 
				if (lex.Matched(T_DYNAMIC))
					m_bRowSeparatorDynamic = TRUE;  
				else
					m_bRowSeparator        = TRUE;  
					
				if (lex.LookAhead(T_PEN))
				{
					m_pRowSepPen = new BorderPen();
					ok = lex.ParsePen(*m_pRowSepPen);
				}

				break;
			}

			case T_INTERLINE:
			{
				lex.SkipToken();
				m_pInterlinePen = new BorderPen();
				ok = lex.ParsePen(*m_pInterlinePen);
				break;
			}

			case T_NO_BOT   : ok = lex.SkipToken(); m_bBodyTop             = FALSE;    break;
			case T_NO_BOB   : ok = lex.SkipToken(); m_bBodyBottom          = FALSE;    break;
			case T_NO_BOL   : ok = lex.SkipToken(); m_bBodyLeft            = FALSE;    break;
			case T_NO_BOR   : ok = lex.SkipToken(); m_bBodyRight           = FALSE;    break;

			case T_TOTAL_TOP   : ok = lex.SkipToken(); m_bTotalTop         = TRUE;    break;
			case T_NO_TOB   : ok = lex.SkipToken(); m_bTotalBottom         = FALSE;    break;
			case T_NO_TOL   : ok = lex.SkipToken(); m_bTotalLeft           = FALSE;    break;
			case T_NO_TOR   : ok = lex.SkipToken(); m_bTotalRight          = FALSE;    break;

			case T_END :
				if (blk) return ok;
				lex.SetError(_TB("Unexpected END"));
				return FALSE;

			default :
				if (blk)
				{
					lex.SetError(_TB("There is no final END after \r\n more options on the table borders"));
					ok = FALSE;
				}
		}
	}
	while (ok && blk);

	return ok;
}

//------------------------------------------------------------------------------
BOOL TableBorders::ParseBorders (ViewParser& lex)
{
	BOOL ok = TRUE;

	do { ok = ParseBorder(lex) && !lex.Bad() && !lex.Eof(); }
		while (ok && !lex.LookAhead(T_END));

	return ok;
}


//------------------------------------------------------------------------------
BOOL TableBorders::ParseBlock(ViewParser& lex)
{
	if (lex.LookAhead(T_BEGIN))
		return
			lex.ParseBegin  ()      &&
			ParseBorders    (lex)   &&
			lex.ParseEnd    ();

	return ParseBorder(lex, FALSE);
}

//------------------------------------------------------------------------------
BOOL TableBorders::Parse (ViewParser& lex)
{
	// don't exist option section so standrad value
	BOOL ok =TRUE;

	// try if all m_Borders are off
	if (lex.LookAhead(T_NO_BORDERS))
	{
		ok = lex.ParseTag (T_NO_BORDERS);
		if (ok) Init (FALSE);
		return ok;
	}

	// parse eventual m_Borders flags
	if (lex.LookAhead(T_BORDERS))
		ok =
		(
			lex.ParseTag(T_BORDERS) &&
			ParseBlock  (lex)
		);

	return ok;
}


//=============================================================================
