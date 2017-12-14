#include "stdafx.h"

#include <float.h>

#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\PathFinder.h>

#include <TbGeneric\FormatsHelpers.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\GeneralFunctions.h>

#include <TbParser\Parser.h>

#include "FormatsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define FORMATS_STYLES_RELEASE	2

////////////////////////////////////////////////////////////////////////////////
//						class	FormatsParser
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
FormatsParser::FormatsParser()
	:
	m_nRelease (FORMATS_STYLES_RELEASE),
	m_bParsing (FALSE)
{
};

// I fonts hanno una politica di customizzazione per cui i files trovati sono
// un'integrazione dei precedenti. Per questo motivo carico in ordine,  prima
// gli Standard e poi i Custom. Dopo la revisione dell'InitInstance ora gli 
// standard vengono caricati in InitInstance mentre i Custom alla Login.
//-----------------------------------------------------------------------------
BOOL FormatsParser::LoadFormats (
									FormatStyleTable*		pTable,
									const CTBNamespace&		aModule, 
									const CPathFinder*		pPathFinder, 
									CStatusBarMsg*			pStatusBar,
									CPathFinder::PosType	posType
								)
{
	if (!pPathFinder || !aModule.IsValid())
		return FALSE;

	CString strFileName = pPathFinder->GetFormatsFullName(aModule, posType);
	if (!ExistFile(strFileName))
		return FALSE;

	AfxGetApp()->BeginWaitCursor();
	pStatusBar->Show(cwsprintf(_TB("Loading Format Styles for the %s..."), aModule.ToString()));

	Parser lex;
	lex.Attach(NULL);
	if (!lex.Open(strFileName))
	{
		AfxGetApp()->EndWaitCursor();
		return FALSE;
	}

	m_bParsing = TRUE;

	if (posType == CPathFinder::STANDARD)
		Parse (aModule, Formatter::FROM_STANDARD, pTable, lex);
	else
	{
		pTable->AddFileLoaded(aModule, Formatter::FROM_CUSTOM, ::GetFileDate(strFileName));
		Parse (aModule, Formatter::FROM_CUSTOM, pTable, lex);
	}

	m_bParsing = FALSE;

	AfxGetApp()->EndWaitCursor();
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL FormatsParser::SaveFormats	(
									const CTBNamespace&	aModule, 
									const CPathFinder*	pPathFinder, 
									const BOOL&			bSaveStandards
								)	
{ 
	if (!pPathFinder || !aModule.IsValid())
		return FALSE;

	// non sono stati aggiunti stili e quelli base non sono cambiati
	if (!AfxGetFormatStyleTable()->IsModified())
		return TRUE;

	Unparser		oFile;

	CStatusBarMsg	msgBar(TRUE, TRUE); 

	msgBar.Show(_TB("Saving Format Styles..."));

	AfxGetApp()->BeginWaitCursor();

	CFileStatus stat;
	CString sFileName;

	// se devo salvo prima gli standard
	if (bSaveStandards)
	{
		sFileName = pPathFinder->GetFormatsFullName(aModule, CPathFinder::STANDARD);
		if (oFile.Open(sFileName) && CLineFile::GetStatus(sFileName, stat))
		{
			Unparse(aModule, Formatter::FROM_STANDARD, AfxGetWritableFormatStyleTable(), oFile);
		}
	}

	// salvo nella Custom
	sFileName = pPathFinder->GetFormatsFullName(aModule, CPathFinder::CUSTOM, TRUE);
	// bisogna controllare che il file non sia read only
	DWORD dwAttr = GetTbFileAttributes((LPCTSTR) sFileName);
	if (FILE_ATTRIBUTE_READONLY & dwAttr)
		SetFileAttributes((LPCTSTR) sFileName, dwAttr & !FILE_ATTRIBUTE_READONLY);

	if (oFile.Open(sFileName))
	{  
		oFile.SetFormat(CLineFile::UTF8);
		Unparse(aModule, Formatter::FROM_CUSTOM, AfxGetWritableFormatStyleTable(), oFile);
	}
	
	AfxGetApp()->EndWaitCursor();

	return TRUE ;
}

// Se parsa styli di formattazione di applicazione richiede la release altrimenti
// (wrmeng) non bisogna parsare la release ma solo la tabella di stili.
//------------------------------------------------------------------------------
BOOL FormatsParser::Parse 
		(
			const CTBNamespace&					aModule, 
			const Formatter::FormatStyleSource	aSource,
			FormatStyleTable*					pTable, 
			Parser&								lex
		)
{
	if (!aModule.IsValid() || !pTable)
		return FALSE;

	// prima aggancio i dati obbligatori
	m_Namespace = aModule;
	m_Source = aSource;
	m_pTable = pTable;

    BOOL bOk = TRUE;

	// parsa la release solo per gli stili di applicazione
	if (aSource != Formatter::FROM_WOORM)
	{
		int	nRelease;
		if (!lex.ParseTag (T_RELEASE) || !lex.ParseInt (nRelease))
			return FALSE;
	
		if (nRelease != FORMATS_STYLES_RELEASE)
			return lex.SetError(_TB("Incompatible Format Styles release.\r\nTable not loaded."));
    }

	// fonts style section exist
	if (lex.LookAhead(T_FORMATSTYLES))
		return lex.ParseTag(T_FORMATSTYLES) && ParseBlock (lex);

	return bOk;
}

// Se unparsa styli di formattazione di applicazione richiede la release altrimenti
// (wrmeng) non bisogna unparsare la release ma solo la tabella di stili
//------------------------------------------------------------------------------
void FormatsParser::Unparse 
	(
		const CTBNamespace&					aModule, 
		const Formatter::FormatStyleSource	aSource,
		FormatStyleTablePtr					pTable, 
		Unparser&							ofile
	)
{
	if (!aModule.IsValid() || !pTable)
		return;

	// prima aggancio i dati obbligatori
	m_Namespace = aModule;
	m_Source	= aSource;
	m_pTable	= pTable.GetPointer();

	// se chiamata da wrmeng deve sempre salvare
	if (m_Source != Formatter::FROM_WOORM && !pTable->IsModified())
		return;

	// scrive  la release supportata
	if (m_Source != Formatter::FROM_WOORM)
	{
		ofile.UnparseTag    (T_RELEASE,	FALSE);
		ofile.UnparseInt    (FORMATS_STYLES_RELEASE);
	}
	
	ofile.UnparseTag	(T_FORMATSTYLES, FALSE);
	ofile.UnparseBlank	(TRUE);

	ofile.UnparseBegin	();
	BOOL bNotEmpty = UnparseFormatsStyles(ofile);
	ofile.UnparseEnd	();

	ofile.UnparseCrLf	();

	// avviso che è stata cambiata la data del file
	if (m_Source != Formatter::FROM_WOORM)
		pTable->AddFileLoaded(aModule, m_Source, ::GetFileDate(ofile.GetFilePath()));

	if (bNotEmpty)
		return;

	// se è vuoto viene eliminato
	if (aSource != Formatter::FROM_WOORM)
	{
		CString strPath = ofile.GetFilePath();
		ofile.Close();
		DeleteFile((LPCTSTR) strPath);
		pTable->RemoveFileLoaded(aModule, m_Source);
	}
}

//------------------------------------------------------------------------------
void FormatsParser::UnparseFormatStyle (Unparser&	ofile, Formatter* pFormatter)
{
	ofile.UnparseTag		(FromDataTypeToToken(pFormatter->GetDataType()), FALSE);
	ofile.UnparseString		(pFormatter->GetName(),	 FALSE);
	ofile.UnparseBlank		();
	
	UnparseFormatter (ofile, pFormatter);

	// ending newline
    ofile.UnparseSep	();
	ofile.UnparseCrLf	();
}

//------------------------------------------------------------------------------
BOOL FormatsParser::UnparseFormatsStyles(Unparser& ofile)
{
	BOOL bExist = FALSE;
	for (int i = 0; i <= m_pTable->GetUpperBound(); i++)
	{
		FormatterGroup* pFormatterGroup = (FormatterGroup*) m_pTable->GetAt(i);
		
		if (!pFormatterGroup)
			continue;

		for (int n = 0; n <= pFormatterGroup->GetFormatters().GetUpperBound(); n++)
		{
			Formatter* pFormatter = (Formatter*) pFormatterGroup->GetFormatters().GetAt(n);

			// devo tenere conto anche dei namespace Library
			CTBNamespace aNs(
								CTBNamespace::MODULE, 
								pFormatter->GetOwner().GetApplicationName()
								+ CTBNamespace::GetSeparator() +
								pFormatter->GetOwner().GetObjectName(CTBNamespace::MODULE)
							);
			// ma anche del report
			if (pFormatter->GetOwner().GetType() == CTBNamespace::REPORT)
				aNs = pFormatter->GetOwner();

			if (pFormatter->GetSource() == m_Source && aNs == m_Namespace)
			{
				UnparseFormatStyle (ofile, pFormatter);
				bExist = TRUE;
			}
		}
	}
	return bExist;
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtType(Parser& lex, DataType& type, CString& stylename)
{
	type = FromTokenToDataType(lex.LookAhead());
	
	if (type == DATA_NULL_TYPE)
	{
		lex.SetError (_TB("Wrong format style."));
    	return FALSE;
	}

    lex.SkipToken();
	return lex.ParseString (stylename);
}

//------------------------------------------------------------------------------
BOOL FormatsParser::ParseStyle (Parser&	lex)
{
	CString	stylename;
	DataType type;

    // try to parse
	if (!ParseFmtType (lex, type, stylename)) 
		return FALSE;

	Formatter* pNewFmt = PolyNewFormatter(type, m_Source, m_Namespace, (LPCTSTR) stylename);

	if (!pNewFmt)
		return FALSE;

	pNewFmt->RecalcWidths();

	if (!ParseFormatter(lex, pNewFmt))
	{
		delete pNewFmt;
		return FALSE;
	}

	// se il formattatore esiste già da un messaggio di warning
	int nGroupIdx = m_pTable->GetFormatIdx(pNewFmt->GetName());
	FormatterGroup* pGroup = nGroupIdx >= 0 ? m_pTable->GetFormatterGroup(nGroupIdx) : NULL;
	Formatter* pExisting;
	if (pGroup)
		for (int i=0; i <= pGroup->GetFormatters().GetUpperBound(); i++)
		{
			pExisting = (Formatter*) pGroup->GetFormatters().GetAt(i);
			if (pExisting && pExisting->GetSource() == pNewFmt->GetSource() && pExisting->GetOwner() == pNewFmt->GetOwner())
				if (pNewFmt->GetOwner().GetType() == CTBNamespace::REPORT)
					lex.SetError (cwsprintf(_TB("Format style {0-%s} is twice defined in report {1-%s}."), (LPCTSTR) pNewFmt->GetName(),  (LPCTSTR) pNewFmt->GetOwner().ToUnparsedString()));
				else
					lex.SetError (cwsprintf(_TB("Format style {0-%s} is twice defined in file Formats.ini of module {1-%s}."), (LPCTSTR) pNewFmt->GetName(),  (LPCTSTR) pNewFmt->GetOwner().ToUnparsedString()));
		}

	// cerca se esiste già un formattatore con lo stesso nome rappresentato dal suo gruppo
	if (AfxGetCultureInfo() && AfxGetCultureInfo()->GetFormatStyleLocale().IsLoaded())
	{
		pNewFmt->SetToLocale();
	}

	pNewFmt->RecalcWidths();
	m_pTable->AddFormatter(pNewFmt);

		// set change new Font only if current is a WOORM FontStyleTable
	if (AfxGetFormatStyleTable() != m_pTable)
	{
		pNewFmt->SetChanged(TRUE);

		m_pTable->SetModified(TRUE);
	}
	else
		pNewFmt->SetChanged(FALSE);

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL FormatsParser::ParseBlock (Parser&	lex)
{
	if (lex.LookAhead(T_BEGIN))
       	return
			lex.ParseBegin	()		&&
			ParseStyles		(lex)	&&
			lex.ParseEnd	();

    return ParseStyle(lex);
}

//------------------------------------------------------------------------------
BOOL FormatsParser::ParseStyles (Parser& lex)
{
	//se la tabella è vuota non segnalo errore ma dò semplicemente un ASSERT
	if (lex.LookAhead(T_END)) 
	{
		ASSERT(FALSE);
		return TRUE;
	}

	BOOL ok = TRUE;
	do { ok = ParseStyle(lex) && !lex.Bad() && !lex.Eof(); }
	while (ok && !lex.LookAhead(T_END));

    return ok;
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseFormatter(Unparser& ofile, Formatter* pFormatter)
{
	UnparseFmtCommon	(ofile, pFormatter);
	UnparseFmtVariable	(ofile, pFormatter);

	// è un formattatore programmativo derivante da libreria
	if (pFormatter->GetOwner().GetType() == CTBNamespace::LIBRARY)
	{
		ofile.UnparseTag	(T_FROM, FALSE);
		ofile.UnparseString	(pFormatter->GetOwner().ToString(), FALSE);
	}

	// area di applicazione
	if (!pFormatter->GetLimitedArea().IsEmpty())
		ofile.UnparseString(pFormatter->GetLimitedArea(), FALSE);
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFormatter (Parser& lex, Formatter* pFormatter)
{
	BOOL bOk =	ParseFmtCommon	(lex, pFormatter) &&
				ParseFmtVariable(lex, pFormatter);

	CString s;
	// programmative formatters are at the end before application criteria
	if (bOk && lex.LookAhead(T_FROM	))
	{
		lex.SkipToken(); 
		bOk = lex.ParseString (s);
		if (bOk)
		{
			CTBNamespace ns(s);
			if (ns.IsValid())
			{
				bOk = ns.GetType() == CTBNamespace::LIBRARY;
				if (bOk)
					pFormatter->SetOwner(ns);
			}
		}
	}

	// application criteria is at the end 
	if (bOk && lex.LookAhead(T_STR))
	{
		bOk = lex.ParseString (s);

		if (!s.IsEmpty())
			pFormatter->SetLimitedArea(s);
	}

	bOk = bOk && lex.ParseSep	();
	
	if (bOk)
		pFormatter->RecalcWidths();

	return bOk;
}

//------------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtAlign(Parser& lex, Formatter* pFormatter)
{
	BOOL ok = TRUE;

	lex.SkipToken();

	switch (lex.LookAhead())
	{
		case T_FLEFT: 
			ok = lex.SkipToken(); 
			pFormatter->SetAlign(Formatter::LEFT);
			break;
		case T_FRIGHT: 
			ok = lex.SkipToken(); 
			pFormatter->SetAlign(Formatter::RIGHT);
			break;
		case T_EOF: 
			ok = lex.SetError (_TB("Unexpected EOF while reading format styles."));	
			break;
		default: 
			ok = lex.SetError (_TB("Wrong alignment type."));
			break;
	}

    return ok;
}

//------------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtCommon (Parser& lex, Formatter* pFormatter)
{
	BOOL ok = TRUE;
	CString s;
	int n;
	do
	{
		switch (lex.LookAhead())
		{
			case T_PREFIX	:	lex.SkipToken(); ok = lex.ParseString (s); pFormatter->SetHead(s);		break;
			case T_POSTFIX	:	lex.SkipToken(); ok = lex.ParseString (s); pFormatter->SetTail(s);		break;
			case T_FLEN		:	lex.SkipToken(); ok = lex.ParseInt	(n); pFormatter->SetPaddedLen(n);	break;
			case T_ALIGN	: ok = ParseFmtAlign(lex, pFormatter);										break;
			case T_EOF		: ok = lex.SetError (_TB("Unexpected EOF while reading format styles."));		break;
			case T_END		: ok = lex.SetError (_TB("Unexpected END while reading format styles.")); 		break;
			default			: return ok;
		}
	}
	while (ok);

    return ok;
}

//------------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtVariable (Parser& lex, Formatter* pFormatter)
{
	if	(
			pFormatter->GetDataType() == DataType::Integer ||
			pFormatter->GetDataType() == DataType::Long 
		)	
		return ParseLongData(lex, pFormatter);

	if	(
			pFormatter->GetDataType() == DataType::String ||
			pFormatter->GetDataType() == DataType::Text	  ||
			pFormatter->GetDataType() == DataType::Guid
		)	
		return ParseStringData (lex, pFormatter);

	if (
			pFormatter->GetDataType() == DataType::Double ||
			pFormatter->GetDataType() == DataType::Money ||
			pFormatter->GetDataType() == DataType::Quantity ||
			pFormatter->GetDataType() == DataType::Percent 
		)	
		return ParseDoubleData (lex, pFormatter);

	if	(
			pFormatter->GetDataType() == DataType::Date ||
			pFormatter->GetDataType() == DataType::DateTime ||
			pFormatter->GetDataType() == DataType::Time
		)	
		return ParseDateData (lex, pFormatter);

	if	(pFormatter->GetDataType() == DataType::ElapsedTime)
		return ParseElapsedTimeData (lex, pFormatter);

	if	(pFormatter->GetDataType() == DataType::Bool)
		return ParseBoolData (lex, pFormatter);

	if	(pFormatter->GetDataType() == DataType::Enum)
		return ParseEnumData (lex, pFormatter);

	return FALSE;
}

//------------------------------------------------------------------------------
void FormatsParser::UnparseFmtVariable	(Unparser& ofile, Formatter* pFormatter)
{
	if	(
			pFormatter->GetDataType() == DataType::Integer ||
			pFormatter->GetDataType() == DataType::Long 
		)	
		return UnparseLongData (ofile, pFormatter);

	if	(
			pFormatter->GetDataType() == DataType::String ||
			pFormatter->GetDataType() == DataType::Guid
		)	
		return UnparseStringData(ofile, pFormatter);

	if	(
			pFormatter->GetDataType() == DataType::Double ||
			pFormatter->GetDataType() == DataType::Money ||
			pFormatter->GetDataType() == DataType::Quantity ||
			pFormatter->GetDataType() == DataType::Percent 
		)	
		return UnparseDoubleData (ofile, pFormatter);

	if	(
			pFormatter->GetDataType() == DataType::Date ||
			pFormatter->GetDataType() == DataType::DateTime ||
			pFormatter->GetDataType() == DataType::Time
		)	
		return UnparseDateData (ofile, pFormatter);

	if	(pFormatter->GetDataType() == DataType::ElapsedTime)
		return UnparseElapsedTimeData (ofile, pFormatter);

	if	(pFormatter->GetDataType() == DataType::Bool)
		return UnparseBoolData (ofile, pFormatter);

	if	(pFormatter->GetDataType() == DataType::Enum)
		return UnparseEnumData (ofile, pFormatter);
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtAlign(Unparser& ofile, int default_align, Formatter* pFormatter)
{
	if (pFormatter->GetAlign() == default_align || pFormatter->GetAlign() == Formatter::NONE) 
		return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_ALIGN, FALSE);

	switch (pFormatter->GetAlign())
	{
		case Formatter::LEFT : ofile.UnparseTag(T_FLEFT,	FALSE); break;
		case Formatter::RIGHT: ofile.UnparseTag(T_FRIGHT,FALSE);	break;
	}
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtCommon(Unparser& ofile, Formatter* pFormatter)
{
	if (!pFormatter->GetHead().IsEmpty())
	{
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseTag 	(T_PREFIX,	FALSE);
		ofile.UnparseString (pFormatter->GetHead(),	FALSE);
	}

	if (!pFormatter->GetTail().IsEmpty())
	{
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseTag 	(T_POSTFIX,	FALSE);
		ofile.UnparseString (pFormatter->GetTail(),	FALSE);
	}

	if (pFormatter->GetPaddedLen())
	{
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseTag 	(T_FLEN,		FALSE);
		ofile.UnparseInt	(pFormatter->GetPaddedLen(),	FALSE);
	}
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseDoubleData(Parser& lex, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt = (CDblFormatter*) pFormatter;

	BOOL ok = TRUE;

	pDblFmt->m_bShowMSZero = TRUE;
	pDblFmt->m_bShowLSZero = TRUE;

	do switch (lex.LookAhead())
	{
		case T_ALIGN	: ok = ParseFmtAlign	(lex, pFormatter);	break;
		case T_STYLE	: ok = ParseDoubleDataStyle(lex, pFormatter);	break;
		case T_FROUND	: ok = ParseFmtRound	(lex, pFormatter);	break;
		case T_THOUSAND	: ok = ParseFmtThousand	(lex, pFormatter);	pDblFmt->m_bIs1000SeparatorDefault = !ok; break;
		case T_SEPARATOR: ok = ParseDoubleFmtSep(lex, pFormatter);	pDblFmt->m_bIsDecSeparatorDefault = !ok; break;
		case T_PRECISION: ok = ParseFmtDecimal	(lex, pFormatter);	break;
		case T_FSIGN	: ok = ParseFmtSign		(lex, pFormatter);	break;
		case T_TABLE	: ok = ParseFmtTable	(lex, pFormatter);	break;

		case T_HIDE_MS0	: pDblFmt->m_bShowMSZero = FALSE; ok = lex.ParseTag(T_HIDE_MS0);	break;
		case T_HIDE_LS0	: pDblFmt->m_bShowLSZero = FALSE; ok = lex.ParseTag(T_HIDE_LS0);	break;

		case T_EOF	: ok = lex.SetError (_TB("Unexpected EOF while reading format styles."));	break;
		case T_END	: ok = lex.SetError (_TB("Unexpected END while reading format styles.")); 	break;
		default		: return ok;
	}
	while (ok);

	if (pDblFmt->m_str1000Separator == pDblFmt->m_strDecSeparator)
		ok = lex.SetError(_TB("The thousands separator must be different from the decimal separator."));
		
    return ok;
}

// specifica per i Double
//----------------------------------------------------------------------------
void FormatsParser::UnparseDoubleData (Unparser& ofile, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt = (CDblFormatter*) pFormatter;

	if (pDblFmt->m_nPaddedLen > 0) 
		UnparseFmtAlign (ofile, pDblFmt->GetDefaultAlign(), pFormatter);

	UnparseDoubleDataStyle	(ofile, pFormatter);
	UnparseFmtRound			(ofile, pFormatter);
	UnparseFmtSign			(ofile, pFormatter);
	UnparseFmtTable			(ofile, pFormatter);
	UnparseFmtThousand		(ofile, pFormatter);
	UnparseDoubleFmtSep		(ofile, pFormatter);
	UnparseFmtDecimal		(ofile, pFormatter);

	if (!pDblFmt->m_bShowMSZero)
	{
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseTag(T_HIDE_MS0, FALSE);
	}

	if (!pDblFmt->m_bShowLSZero)
	{
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseTag(T_HIDE_LS0, FALSE);
	}
}

// specifica per i Double
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtRound (Parser& lex, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt = (CDblFormatter*) pFormatter;

	return
		lex.ParseTag	(T_FROUND)		&&
		lex.ParseInt	((int&) pDblFmt->m_Rounding) && 
		lex.ParseDouble	(pDblFmt->m_nQuantum);
}

// specifica per i Double
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtRound (Unparser& ofile, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt = (CDblFormatter*) pFormatter;

	if (pDblFmt->GetRounding() == CDblFormatter::ROUND_NONE) 
		return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_FROUND, FALSE);
	ofile.UnparseInt	(pDblFmt->GetRounding(), FALSE); 
	ofile.UnparseBlank	(FALSE);
	ofile.UnparseDouble	(pDblFmt->m_nQuantum, FALSE);
}

// specifica per i Double
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtDecimal (Parser& lex, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt = (CDblFormatter*) pFormatter;

	BOOL ok = lex.ParseTag (T_PRECISION) &&	lex.ParseInt (pDblFmt->m_nDecNumber);

	if ((pDblFmt->GetDecNumber() < 0) || (pDblFmt->GetDecNumber() > DBL_DIG))
    {
		lex.SetError(_TB("Wrong precision value. It must be in the range from 0 to 18."));
		return FALSE;
	}
    return ok;
}

// specifica per i Double
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtDecimal (Unparser& ofile, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt = (CDblFormatter*) pFormatter;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_PRECISION, FALSE);
	ofile.UnparseInt	(pDblFmt->GetDecNumber(), FALSE);
}

// specifica per i Double
//----------------------------------------------------------------------------
void FormatsParser::UnparseDoubleFmtSep (Unparser& ofile, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt	= (CDblFormatter*) pFormatter;
	CString sDecSeparator	= pDblFmt->m_strDecSeparator;
	CString sLocaleSep		= AfxGetCultureInfo()->GetFormatStyleLocale().m_sDecSeparator;
	
	if (sDecSeparator.IsEmpty() || _tcsicmp(sDecSeparator, sLocaleSep) == 0) 
		return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_SEPARATOR, FALSE);
	ofile.UnparseString	(sDecSeparator, FALSE);
}

// specifica per i Double
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseDoubleFmtSep (Parser& lex, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt = (CDblFormatter*) pFormatter;

	return lex.ParseTag	(T_SEPARATOR) && lex.ParseString (pDblFmt->m_strDecSeparator);
}

// per i double
//----------------------------------------------------------------------------
void FormatsParser::UnparseDoubleDataStyle (Unparser& ofile, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt = (CDblFormatter*) pFormatter;

	if (pDblFmt->GetFormat () == CDblFormatter::FIXED) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_STYLE, FALSE);
	ofile.UnparseInt 	(pDblFmt->GetFormat (), FALSE);

	if	(
			pDblFmt->GetFormat () != CDblFormatter::ZERO_AS_DASH ||
			pDblFmt->m_strAsZeroValue == szZeroAsDash
		)
		return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_NULL, FALSE);
	ofile.UnparseString	(pDblFmt->m_strAsZeroValue, FALSE);
}

// per i double
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseDoubleDataStyle (Parser& lex, Formatter* pFormatter)
{
	CDblFormatter* pDblFmt = (CDblFormatter*) pFormatter;

	if (!lex.ParseTag (T_STYLE) || !lex.ParseInt ((int&) pDblFmt->m_FormatType))
		return FALSE;

	if	(
			pDblFmt->m_FormatType != CDblFormatter::ZERO_AS_DASH ||
			lex.LookAhead() != T_NULL
		)
		return TRUE;

	return lex.ParseTag (T_NULL) && lex.ParseString (pDblFmt->m_strAsZeroValue);
}

// specifica per i Bool
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseBoolData(Parser& lex, Formatter* pFormatter)
{
	CBoolFormatter* pBoolFmt = (CBoolFormatter*) pFormatter;

	// inital value
	pBoolFmt->m_FormatType = CBoolFormatter::AS_ZERO;

	BOOL ok = TRUE;
	do switch (lex.LookAhead())
	{
		case T_BITMAP	: pBoolFmt->m_FormatType = CBoolFormatter::AS_CHAR; lex.SkipToken(); break;
		case T_ALIGN	: ok = ParseFmtAlign		(lex, pFormatter);	break;
		case T_LOGIC	: ok = ParseFmtBoolString	(lex, pFormatter);	break;

		case T_EOF		: ok = lex.SetError(_TB("Unexpected EOF while reading format styles."));	break;
		case T_END		: ok = lex.SetError(_TB("Unexpected END while reading format styles."));	break;
		default			: return ok;
	}
	while (ok);

    return ok;
}

// specifica per i Bool
//----------------------------------------------------------------------------
void FormatsParser::UnparseBoolData (Unparser& ofile, Formatter* pFormatter)
{
	CBoolFormatter* pBoolFmt = (CBoolFormatter*) pFormatter;

	UnparseFmtAlign (ofile, pBoolFmt->GetDefaultAlign(), pFormatter);

	if (pBoolFmt->m_FormatType) ofile.UnparseTag (T_BITMAP,	FALSE);

	UnparseFmtBoolString (ofile, pFormatter);
}

// specifica per i Bool
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtBoolString(Unparser& ofile, Formatter* pFormatter)
{
	CBoolFormatter* pBoolFmt = (CBoolFormatter*) pFormatter;

	if	(
			pBoolFmt->m_strTrueTag == DataObj::Strings::YES() &&
			pBoolFmt->m_strFalseTag == DataObj::Strings::NO()
		)
		return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_LOGIC, FALSE);
	ofile.UnparseString	(pBoolFmt->m_strTrueTag, FALSE);
	ofile.UnparseString	(pBoolFmt->m_strFalseTag, FALSE);
}

// specifica per i Bool
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtBoolString(Parser& lex, Formatter* pFormatter)
{
	CBoolFormatter* pBoolFmt = (CBoolFormatter*) pFormatter;

	return
		lex.ParseTag 	(T_LOGIC)		&&
		lex.ParseString	(pBoolFmt->m_strTrueTag)	&&
		lex.ParseString	(pBoolFmt->m_strFalseTag);		
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseDateData(Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	BOOL ok = TRUE;

	do switch (lex.LookAhead())
	{
		case T_ALIGN	: ok = ParseFmtAlign	(lex, pFormatter);	break;
		case T_ORDER	: ok = ParseFmtOrder		(lex, pFormatter);  pDateFmt->m_bIsFormatTypeDefault = !ok; break;
		case T_WEEKDAY	: ok = ParseFmtWeekDay	(lex, pFormatter);	break;
		case T_FDAY		: ok = ParseFmtDayFmt		(lex, pFormatter);	break;
		case T_FMONTH	: ok = ParseFmtMonthFmt	(lex, pFormatter);	break;
		case T_FYEAR	: ok = ParseFmtYearFmt	(lex, pFormatter);	pDateFmt->m_bIsFormatYearDefault = !ok; break;
		case T_BEFORE	: ok = ParseFmtFirstSep	(lex, pFormatter);	break;
		case T_AFTER	: ok = ParseFmtSecondSep	(lex, pFormatter);	break;
		case T_STYLE	:
		case T_TTIME	: ok = ParseDateTimeFmt	(lex, pFormatter);	break;

		case T_EOF	: ok = lex.SetError (_TB("Unexpected EOF while reading format styles."));	break;
		case T_END	: ok = lex.SetError (_TB("Unexpected END while reading format styles.")); 	break;
		default		: return ok;
	}
	while (ok);

    return ok;
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseDateData (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	// must Unparsed in variable part because align default change
	// from type to type. Parse must be done in common part
    if (pDateFmt->m_nPaddedLen > 0) 
		UnparseFmtAlign (ofile, pDateFmt->GetDefaultAlign(), pFormatter);

	UnparseFmtOrder		(ofile, pFormatter);	
	UnparseFmtWeekDay	(ofile, pFormatter);
	UnparseFmtDayFmt	(ofile, pFormatter);
	UnparseFmtMonthFmt	(ofile, pFormatter);
	UnparseFmtYearFmt	(ofile, pFormatter);
	UnparseFmtFirstSep	(ofile, pFormatter);
	UnparseFmtSecondSep	(ofile, pFormatter);

	UnparseDateTimeFmt	(ofile, pFormatter);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtOrder (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (pDateFmt->GetFormat() == AfxGetCultureInfo()->GetFormatStyleLocale().m_ShortDateFormat) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_ORDER,	FALSE);
	ofile.UnparseInt 	(pDateFmt->GetFormat(), FALSE);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtWeekDay (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (pDateFmt->GetWeekdayFormat() == CDateFormatHelper::NOWEEKDAY) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_WEEKDAY, FALSE);
	ofile.UnparseInt 	(pDateFmt->GetWeekdayFormat(), FALSE);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtDayFmt (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (pDateFmt->GetDayFormat() == AfxGetCultureInfo()->GetFormatStyleLocale().m_ShortDateDayFormat) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_FDAY, FALSE);
	ofile.UnparseInt 	(pDateFmt->GetDayFormat(), FALSE);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtMonthFmt (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (pDateFmt->GetMonthFormat() == AfxGetCultureInfo()->GetFormatStyleLocale().m_ShortDateMonthFormat) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_FMONTH,	FALSE);
	ofile.UnparseInt 	(pDateFmt->GetMonthFormat(), FALSE);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtYearFmt (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (pDateFmt->GetYearFormat() == AfxGetCultureInfo()->GetFormatStyleLocale().m_ShortDateYearFormat) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_FYEAR, FALSE);
	ofile.UnparseInt 	(pDateFmt->GetYearFormat(), FALSE);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtFirstSep (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (_tcsicmp(pDateFmt->GetFirstSeparator(), AfxGetCultureInfo()->GetFormatStyleLocale().m_sDateSeparator) == 0) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_BEFORE, FALSE);
	ofile.UnparseTag	(T_FMONTH, FALSE);
	ofile.UnparseString	(pDateFmt->GetFirstSeparator(), FALSE);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtSecondSep (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (_tcsicmp(pDateFmt->GetSecondSeparator(), AfxGetCultureInfo()->GetFormatStyleLocale().m_sDateSeparator) == 0) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_AFTER, FALSE);
	ofile.UnparseTag	(T_FMONTH, FALSE);
	ofile.UnparseString	(pDateFmt->GetSecondSeparator(), FALSE);
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseDateTimeFmt (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (!pDateFmt->IsFullDateTimeFormat()) return;

	ofile.UnparseBlank	(FALSE);

	if (pDateFmt->m_TimeFormat != AfxGetCultureInfo()->GetFormatStyleLocale().m_TimeFormat)
	{
		// se e` un DataTime la sintassi e`
		// ... TIME STYLE .....
		// altrimenti e`
		// STYLE .....
		if (!pDateFmt->m_OwnType.IsATime())
			ofile.UnparseTag 	(T_TTIME, FALSE);

		ofile.UnparseTag 	(T_STYLE, FALSE);
		ofile.UnparseInt 	((int)pDateFmt->GetTimeFormat(), FALSE);
	}

	UnparseFmtTimeSep	(ofile, pFormatter);
	UnparseFmtTimeAMPM	(ofile, pFormatter);
}

// specifica per date e tempi ed elapsedtimes
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtTimeSep (Unparser& ofile, Formatter* pFormatter)
{
	CString sTimeSeparator 
		(
			pFormatter->IsKindOf(RUNTIME_CLASS(CDateFormatter)) ?
			((CDateFormatter*) pFormatter)->GetTimeSeparator() :
			((CElapsedTimeFormatter*) pFormatter)->GetTimeSeparator()
		);

	if (_tcsicmp(sTimeSeparator, AfxGetCultureInfo()->GetFormatStyleLocale().m_sTimeSeparator) == 0) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_SEPARATOR, FALSE);
	ofile.UnparseString	(sTimeSeparator, FALSE);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtTimeAMPM (Unparser& ofile, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (!pDateFmt->IsTimeAMPMFormat()) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_POSTFIX, FALSE);
	ofile.UnparseString	(pDateFmt->GetTimeAMString(), FALSE);
	ofile.UnparseString	(pDateFmt->GetTimePMString(), FALSE);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtOrder (Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	return
		lex.ParseTag (T_ORDER) &&
		lex.ParseInt ((int&)pDateFmt->m_FormatType); 
}

// specifica per date e tempi
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtWeekDay (Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	return
		lex.ParseTag (T_WEEKDAY) &&
		lex.ParseInt ((int&)pDateFmt->m_WeekdayFormat); 
}

// specifica per date e tempi
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtDayFmt (Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	return
		lex.ParseTag (T_FDAY) &&
		lex.ParseInt ((int&)pDateFmt->m_DayFormat); 
}

// specifica per date e tempi
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtMonthFmt (Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	return 
		lex.ParseTag (T_FMONTH) &&
		lex.ParseInt ((int&)pDateFmt->m_MonthFormat); 
}

// specifica per date e tempi
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtYearFmt (Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	return
		lex.ParseTag (T_FYEAR) &&
		lex.ParseInt ((int&)pDateFmt->m_YearFormat); 
}

// specifica per date e tempi
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtFirstSep (Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	return
		lex.ParseTag	(T_BEFORE)	&&
		lex.ParseTag	(T_FMONTH)	&&
		lex.ParseString	(pDateFmt->m_strFirstSeparator);
}

// specifica per date e tempi
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtSecondSep (Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	return
		lex.ParseTag	(T_AFTER)	&&
		lex.ParseTag	(T_FMONTH)	&&
		lex.ParseString	(pDateFmt->m_strSecondSeparator);
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseDateTimeFmt (Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	// se e` un DataTime la sintassi e`
	// ... TIME STYLE .....
	// altrimenti e`
	// STYLE .....
	if (!pDateFmt->m_OwnType.IsATime() && !lex.ParseTag(T_TTIME))
		return FALSE;

	pDateFmt->m_bIsTimeFormatDefault = FALSE;
	return
		lex.ParseTag 	(T_STYLE)		&&
		lex.ParseInt 	((int&)pDateFmt->m_TimeFormat)	&& 
		ParseFmtTimeSep	(lex, pFormatter) &&
		ParseFmtTimeAMPM(lex, pFormatter);
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtTimeSep (Parser& lex, Formatter* pFormatter)
{
	if (lex.LookAhead() != T_SEPARATOR) return TRUE;

	CString sTimeSeparator;
	BOOL bOk = lex.ParseTag	(T_SEPARATOR) && lex.ParseString (sTimeSeparator);
	
	if (bOk)
		if (pFormatter->IsKindOf(RUNTIME_CLASS(CDateFormatter)))
			((CDateFormatter*) pFormatter)->m_strTimeSeparator = sTimeSeparator;
		else
			((CElapsedTimeFormatter*) pFormatter)->m_strTimeSeparator = sTimeSeparator;

	return bOk;
}

// specifica per date e tempi
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtTimeAMPM (Parser& lex, Formatter* pFormatter)
{
	CDateFormatter* pDateFmt = (CDateFormatter*) pFormatter;

	if (lex.LookAhead() != T_POSTFIX) return TRUE;

	return
		lex.ParseTag	(T_POSTFIX)		&&
		lex.ParseString	(pDateFmt->m_strTimeAM)	&&
		lex.ParseString	(pDateFmt->m_strTimePM);
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseElapsedTimeData(Parser& lex, Formatter* pFormatter)
{
	BOOL ok = TRUE;

	do switch (lex.LookAhead())
	{
		case T_PROMPT		:	ok =	ParseFmtTimePrompt	(lex, pFormatter);	break;
		case T_ALIGN		:	ok =	ParseFmtAlign		(lex, pFormatter);	break;
		case T_STYLE		:	ok =	ParseElapsedTimeFmt		(lex, pFormatter)	&&
									ParseFmtTimeSep		(lex, pFormatter)	&&
									ParseFmtTimeDecimal	(lex, pFormatter);
							break;
		case T_SEPARATOR	: ok = ParseFmtTimeSep (lex, pFormatter);	break;
		case T_EOF			: ok = lex.SetError (_TB("Unexpected EOF while reading format styles."));	break;
		case T_END			: ok = lex.SetError (_TB("Unexpected END while reading format styles.")); 	break;
		default				: return ok;
	}
	while (ok);

    return ok;
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseElapsedTimeData(Unparser& ofile, Formatter* pFormatter)
{
	CElapsedTimeFormatter* pTimeFmt = (CElapsedTimeFormatter*) pFormatter;

	// must Unparsed in variable part because align default change
	// from type to type. Parse must be done in common part
    if (pTimeFmt->m_nPaddedLen > 0) 
		UnparseFmtAlign (ofile, pTimeFmt->GetDefaultAlign(), pFormatter);

	UnparseElapsedTimeFmt	(ofile, pFormatter);
	UnparseFmtTimePrompt	(ofile, pFormatter);
	UnparseFmtTimeSep		(ofile, pFormatter);
	UnparseFmtTimeDecimal	(ofile, pFormatter);
}

// per gli elapsed times
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtTimeDecimal (Unparser& ofile, Formatter* pFormatter)
{
	CElapsedTimeFormatter* pTimeFmt = (CElapsedTimeFormatter*) pFormatter;

	if ((pTimeFmt->GetFormat() & CElapsedTimeFormatHelper::TIME_DEC) == 0) return;

	if (pTimeFmt->GetDecSeparator() == szDefDecSeparator && pTimeFmt->GetDecNumber() == nDefDecimals) 
		return;
	
	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_PRECISION, FALSE);

	if (pTimeFmt->GetDecSeparator() != szDefDecSeparator)
		ofile.UnparseString	(pTimeFmt->GetDecSeparator(), FALSE);

	if (pTimeFmt->GetDecNumber() != nDefDecimals)
		ofile.UnparseInt(pTimeFmt->GetDecNumber(), FALSE);
}

// per gli elapsed times
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtTimePrompt (Parser& lex, Formatter* pFormatter)
{
	CElapsedTimeFormatter* pTimeFmt = (CElapsedTimeFormatter*) pFormatter;

	if (!lex.ParseTag(T_PROMPT))
		return FALSE;

	if (lex.Matched(T_RIGHT))
		pTimeFmt->m_nCaptionPos = T_RIGHT;
	else
		if (!lex.ParseTag(T_LEFT))
			return FALSE;
		else
			pTimeFmt->m_nCaptionPos = T_LEFT;

	return TRUE;
}

// per gli elapsed times
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtTimeDecimal (Parser& lex, Formatter* pFormatter)
{
	CElapsedTimeFormatter* pTimeFmt = (CElapsedTimeFormatter*) pFormatter;

	if ((pTimeFmt->GetFormat() & CElapsedTimeFormatHelper::TIME_DEC) != 0)
	{
		if (lex.LookAhead() != T_PRECISION) return TRUE;
		
		if (!lex.ParseTag (T_PRECISION))			
			return FALSE;

		if (lex.LookAhead(T_STR) && !lex.ParseString(pTimeFmt->m_strDecSeparator))
			return FALSE;

		if (pTimeFmt->GetTimeSeparator() == pTimeFmt->GetDecSeparator())
			return lex.SetError(_TB("The thousands separator must be different from the decimal separator."));

		if (lex.LookAhead(T_INT) && !lex.ParseInt (pTimeFmt->m_nDecNumber))
			return FALSE;

		if ((pTimeFmt->GetDecNumber() < 1) || (pTimeFmt->GetDecNumber() > DBL_DIG))
			return lex.SetError(_TB("Wrong precision value. It must be in the range from 0 to 18."));
	}

	return TRUE;
}

// per gli elapsed times
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtTimePrompt (Unparser& ofile, Formatter* pFormatter)
{
	CElapsedTimeFormatter* pDateFmt = (CElapsedTimeFormatter*) pFormatter;

	if (pDateFmt->GetCaptionPos() == 0) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_PROMPT, FALSE);
	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	((Token)pDateFmt->GetCaptionPos(), FALSE);
}

// per gli elapsed times
//----------------------------------------------------------------------------
void FormatsParser::UnparseElapsedTimeFmt (Unparser& ofile, Formatter* pFormatter)
{
	CElapsedTimeFormatter* pTimeFmt = (CElapsedTimeFormatter*) pFormatter;

	if (pTimeFmt->GetFormat() == CElapsedTimeFormatHelper::TIME_HM) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_STYLE, FALSE);
	ofile.UnparseInt 	(pTimeFmt->GetFormat(), FALSE);
}

// per gli elapsed times
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseElapsedTimeFmt (Parser& lex, Formatter* pFormatter)
{
	CElapsedTimeFormatter* pTimeFmt = (CElapsedTimeFormatter*) pFormatter;

	return
		lex.ParseTag		(T_STYLE)		&&
		lex.ParseInt 		((int&) pTimeFmt->m_FormatType); 
}

// per i numerici in generale
//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtSign (Unparser& ofile, Formatter* pFormatter)
{
	int nSignValue = (int)
		(
			pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)) ?
			((CLongFormatter*) pFormatter)->m_Sign :
			((CDblFormatter*) pFormatter)->m_Sign
		);

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_FSIGN, FALSE);
	ofile.UnparseInt 	(nSignValue, FALSE);
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtSign (Parser& lex, Formatter* pFormatter)
{
	BOOL bOk = FALSE;

	if (lex.ParseTag (T_FSIGN))
		if (pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)))
			bOk = lex.ParseInt ((int&)((CLongFormatter*) pFormatter)->m_Sign);
		else
			bOk = lex.ParseInt ((int&) ((CDblFormatter*) pFormatter)->m_Sign);

	return bOk;
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtTable (Unparser& ofile, Formatter* pFormatter)
{
	CString sTable = 
		(
			pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)) ?
			((CLongFormatter*) pFormatter)->m_strXTable :
			((CDblFormatter*) pFormatter)->m_strXTable
		);
	if (sTable.IsEmpty()) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_TABLE, FALSE);
	ofile.UnparseString	(sTable, FALSE);
}


//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtTable (Parser& lex, Formatter* pFormatter)
{
	BOOL bOk = FALSE;

	if (lex.ParseTag (T_TABLE))
		if (pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)))
			bOk = lex.ParseString (((CLongFormatter*) pFormatter)->m_strXTable);
		else
			bOk = lex.ParseString (((CDblFormatter*) pFormatter)->m_strXTable);

	return bOk;
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseFmtThousand (Unparser& ofile, Formatter* pFormatter)
{
	CString sDefaultSeparator 
			(
				pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)) ?
				AfxGetCultureInfo()->GetFormatStyleLocale().m_s1000LongSeparator :
				AfxGetCultureInfo()->GetFormatStyleLocale().m_s1000DoubleSeparator
			);
	CString sSeparator 
		(
			pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)) ?
			((CLongFormatter*) pFormatter)->m_str1000Separator :
			((CDblFormatter*) pFormatter)->m_str1000Separator
		);

	if (sSeparator == sDefaultSeparator) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag	(T_THOUSAND, FALSE);
	ofile.UnparseString	(sSeparator, FALSE);
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseFmtThousand (Parser& lex, Formatter* pFormatter)
{
	BOOL bOk = FALSE;

	if (lex.ParseTag (T_THOUSAND))
		if (pFormatter->IsKindOf(RUNTIME_CLASS(CLongFormatter)))
			bOk = lex.ParseString (((CLongFormatter*) pFormatter)->m_str1000Separator);
		else
			bOk = lex.ParseString (((CDblFormatter*) pFormatter)->m_str1000Separator);

	return bOk;
}

//----------------------------------------------------------------------------
BOOL FormatsParser::ParseLongData(Parser& lex, Formatter* pFormatter)
{
	CLongFormatter* pLongFmt = (CLongFormatter*) pFormatter;

	BOOL ok = TRUE;
	pLongFmt->m_bZeroPadded = FALSE;

	do switch (lex.LookAhead())
	{
		case T_PADDED	: ok = pLongFmt->m_bZeroPadded = lex.ParseTag(T_PADDED);	break;
		case T_ALIGN	: ok = ParseFmtAlign		(lex, pFormatter);			break;
		case T_FSIGN	: ok = ParseFmtSign			(lex, pFormatter);			break;
		case T_STYLE	: ok = ParseLongDataStyle	(lex, pFormatter);			break;
		case T_THOUSAND	: ok = ParseFmtThousand		(lex, pFormatter);	pLongFmt->m_bIs1000SeparatorDefault = !ok;		break;
		case T_TABLE	: ok = ParseFmtTable		(lex, pFormatter);			break;

		case T_EOF	: ok = lex.SetError(_TB("Unexpected EOF while reading format styles."));	break;
		case T_END	: ok = lex.SetError(_TB("Unexpected END while reading format styles.")); break;
		default		: return ok;
	}
	while (ok);

    return ok;
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseLongData(Unparser& ofile, Formatter* pFormatter)
{
	CLongFormatter* pLongFmt = (CLongFormatter*) pFormatter;

	// must Unparsed in variable part because align default change
	// from type to type. Parse must be done in common part
	if (pLongFmt->m_nPaddedLen > 0)	UnparseFmtAlign (ofile, pLongFmt->GetDefaultAlign(), pFormatter);

	if (pLongFmt->m_bZeroPadded)
	{
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseTag	(T_PADDED, FALSE);
	}

	UnparseFmtSign		(ofile, pFormatter);
	UnparseLongDataStyle(ofile, pFormatter);
	UnparseFmtThousand	(ofile, pFormatter);
	UnparseFmtTable		(ofile, pFormatter);
}

// per i long
//----------------------------------------------------------------------------
void FormatsParser::UnparseLongDataStyle (Unparser& ofile, Formatter* pFormatter)
{
	CLongFormatter* pLongFmt = (CLongFormatter*) pFormatter;

	if (pLongFmt->GetFormat() == CLongFormatter::NUMERIC) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_STYLE,	FALSE);
	ofile.UnparseInt 	(pLongFmt->GetFormat(), FALSE);

	if	(
			pLongFmt->GetFormat() != CLongFormatter::ZERO_AS_DASH ||
			pLongFmt->m_strAsZeroValue == szZeroAsDash
		)
		return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_NULL, FALSE);
	ofile.UnparseString	(pLongFmt->m_strAsZeroValue, FALSE);
}

// per i long
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseLongDataStyle (Parser& lex, Formatter* pFormatter)
{
	CLongFormatter* pLongFmt = (CLongFormatter*) pFormatter;
	
	if (!lex.ParseTag (T_STYLE) || !lex.ParseInt ((int&) pLongFmt->m_FormatType))
		return FALSE;

	if	(
			pLongFmt->m_FormatType != CLongFormatter::ZERO_AS_DASH ||
			lex.LookAhead() != T_NULL
		)
		return TRUE;

	return lex.ParseTag (T_NULL) && lex.ParseString (pLongFmt->m_strAsZeroValue);
}

// per le stringhe
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseStringData(Parser& lex, Formatter* pFormatter)
{
	BOOL ok = TRUE;

	do switch (lex.LookAhead())
	{
		case T_ALIGN: ok = ParseFmtAlign	(lex, pFormatter); break;
		case T_STYLE: ok = ParseStringDataStyle(lex, pFormatter); break;

		case T_EOF	: ok = lex.SetError (_TB("Unexpected EOF while reading format styles.")); break;
		case T_END	: ok = lex.SetError (_TB("Unexpected END while reading format styles.")); break;
		default		: return ok;
	}
	while (ok);
    return ok;
}

//----------------------------------------------------------------------------
void FormatsParser::UnparseStringData (Unparser& ofile, Formatter* pFormatter)
{
	CStringFormatter* pStringFmt = (CStringFormatter*) pFormatter;

	if (pStringFmt->m_nPaddedLen > 0) 
		UnparseFmtAlign (ofile, pStringFmt->GetDefaultAlign(), pFormatter);

	UnparseStringDataStyle (ofile, pFormatter);
}

// per le stringhe
//----------------------------------------------------------------------------
void FormatsParser::UnparseStringDataStyle (Unparser& ofile, Formatter* pFormatter)
{
	CStringFormatter* pStringFmt = (CStringFormatter*) pFormatter;

	if (pStringFmt->GetFormat() == CStringFormatter::ASIS) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_STYLE,	FALSE);
	ofile.UnparseInt 	(pStringFmt->GetFormat(),	FALSE);

	if (pStringFmt->GetFormat() == CStringFormatter::EXPANDED)
	{
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseString	(pStringFmt->m_strInterChars, FALSE);
	}
	else if (pStringFmt->GetFormat() == CStringFormatter::MASKED)
	{
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseString	(pStringFmt->GetFormatMask().GetMask(), FALSE);
	}
	
	if (pStringFmt->m_bZeroPadded)
	{
		ofile.UnparseBlank	(FALSE);
		ofile.UnparseTag	(T_PADDED, FALSE);
	}
}

// per le stringhe
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseStringDataStyle (Parser& lex, Formatter* pFormatter)
{
	CStringFormatter* pStringFmt = (CStringFormatter*) pFormatter;

	if (!lex.ParseTag (T_STYLE) || !lex.ParseInt ((int&)pStringFmt->m_FormatType))	
		return FALSE; 

	if (pStringFmt->m_FormatType == CStringFormatter::EXPANDED)
		return lex.ParseString	(pStringFmt->m_strInterChars);
	else if (pStringFmt->m_FormatType == CStringFormatter::MASKED)
	{
		CString strMask;
		if (!lex.ParseString	(strMask))
			return FALSE;
		pStringFmt->SetMask(strMask);

		if (lex.LookAhead(T_PADDED))
			pFormatter->SetZeroPadded(lex.ParseTag(T_PADDED));

	}
	return TRUE;
}

// per gli enums
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseEnumData (Parser& lex, Formatter* pFormatter)
{
	BOOL ok = TRUE;

	do switch (lex.LookAhead())
	{
		case T_ALIGN: ok = ParseFmtAlign	 (lex, pFormatter); break;
		case T_STYLE: ok = ParseEnumDataStyle(lex, pFormatter); break;

		case T_EOF	: ok = lex.SetError (_TB("Unexpected EOF while reading format styles.")); break;
		case T_END	: ok = lex.SetError (_TB("Unexpected END while reading format styles.")); break;
		default		: return ok;
	}
	while (ok);
    return ok;
}

// per gli enums
//----------------------------------------------------------------------------
void FormatsParser::UnparseEnumData (Unparser& ofile, Formatter* pFormatter)
{
	CEnumFormatter* pEnumFmt = (CEnumFormatter*) pFormatter;

	if (pEnumFmt->m_nPaddedLen > 0) 
    	UnparseFmtAlign (ofile, pEnumFmt->GetDefaultAlign(), pFormatter);

	UnparseEnumDataStyle (ofile, pFormatter);
}

// per gli enums
//----------------------------------------------------------------------------
void FormatsParser::UnparseEnumDataStyle(Unparser& ofile, Formatter* pFormatter)
{
	CEnumFormatter* pEnumFmt = (CEnumFormatter*) pFormatter;

	if (pEnumFmt->GetFormat() == CEnumFormatter::ASIS) return;

	ofile.UnparseBlank	(FALSE);
	ofile.UnparseTag 	(T_STYLE,	FALSE);
	ofile.UnparseInt 	(pEnumFmt->GetFormat(),	FALSE);
}

// per gli enums
//----------------------------------------------------------------------------
BOOL FormatsParser::ParseEnumDataStyle(Parser& lex, Formatter* pFormatter)
{
	CEnumFormatter* pEnumFmt = (CEnumFormatter*) pFormatter;

	return lex.ParseTag (T_STYLE) && lex.ParseInt ((int&) pEnumFmt->m_FormatType); 
}

//-----------------------------------------------------------------------------
BOOL FormatsParser::RefreshFormats (
										FormatStyleTablePtr	pTable,
										const CTBNamespace&	aModule, 
										const CPathFinder*	pPathFinder
									)
{
	if (!MustBeRefreshed(pTable, aModule, pPathFinder))
		return FALSE;

	CStatusBarMsg	msgBar(TRUE, TRUE); 
	// lo comunico subito anche se non dovesse funzionare il
	// LoadFont, comunque la Clean mi ha toccato la tabella.
	pTable->ClearFormatsOf(aModule, Formatter::FROM_CUSTOM);
	LoadFormats(pTable.GetPointer(), aModule, AfxGetPathFinder(), &msgBar, CPathFinder::CUSTOM);
	
	return TRUE;
}

// Verifica se le date dei files .INI sono state modifiche dall'ultimo load
// Gli standard non sono soggetti a modifiche e non devono!!
//----------------------------------------------------------------------------
BOOL FormatsParser::MustBeRefreshed	(
										FormatStyleTablePtr	pTable,
										const CTBNamespace&	aModule, 
										const CPathFinder*	pPathFinder
									)
{
	if (aModule.IsEmpty() || aModule.GetType() != CTBNamespace::MODULE || !pPathFinder)
		return FALSE;

	CString sFileName	= pPathFinder->GetFormatsFullName(aModule, CPathFinder::CUSTOM);
	SYSTEMTIME aFileDate;
	aFileDate.wDay		= MIN_DAY;
	aFileDate.wMonth	= MIN_MONTH;
	aFileDate.wYear		= MIN_YEAR;
	aFileDate.wHour		= MIN_HOUR;
	aFileDate.wMinute	= MIN_MINUTE;
	aFileDate.wSecond	= MIN_SECOND;

	if (ExistFile(sFileName))
		aFileDate = GetFileDate(sFileName);
	
	SYSTEMTIME aTableDate = pTable->GetFileDate(aModule, Formatter::FROM_CUSTOM);

	return DataDate(aTableDate) != DataDate(aFileDate);
}
