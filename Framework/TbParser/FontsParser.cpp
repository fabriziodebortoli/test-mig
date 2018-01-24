#include "stdafx.h"

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\PathFinder.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\TBThemeManager.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include "Parser.h"
#include "FontsParser.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define FONT_STYLES_RELEASE		3

////////////////////////////////////////////////////////////////////////////////
//						class	FontsParser
////////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
FontsParser::FontsParser()
	:
	m_nRelease	(FONT_STYLES_RELEASE),
	m_Source	(FontStyle::FROM_STANDARD)
{
}

// I fonts hanno una politica di customizzazione per cui i files trovati sono
// un'integrazione dei precedenti. Per questo motivo carico in ordine,  prima
// gli Standard e poi i Custom. Nei Custom, inoltre, prima carico quelli degli
// AllUsers e poi quelli del singolo utente. Dopo la revisione dell'InitInstance
// ora gli standard vengono caricati in InitInstance mentre i Custom alla Login
//-----------------------------------------------------------------------------
BOOL FontsParser::LoadFonts 
						(
							FontStyleTable*			pTable,
							const CTBNamespace&		aModule, 
							const CPathFinder*		pPathFinder,
							CStatusBarMsg*			pStatusBar,
							CPathFinder::PosType	posType
						)
{
	if (!pPathFinder || !aModule.IsValid())
		return FALSE;

	CString strFileName = pPathFinder->GetFontsFullName(aModule, posType);
	if (!ExistFile(strFileName))
		return FALSE;


	AfxGetApp()->BeginWaitCursor();
	pStatusBar->Show(cwsprintf (_TB("Loading Font Styles for the %s ..."),aModule.ToString()));

	Parser lex;
	lex.Attach(NULL);
	if (!lex.Open(strFileName))
	{
		AfxGetApp()->EndWaitCursor();
		return FALSE;
	}

	if (posType == CPathFinder::STANDARD)
		Parse(aModule, FontStyle::FROM_STANDARD, pTable, lex);
	else
	{
		pTable->AddFileLoaded(aModule, FontStyle::FROM_CUSTOM, ::GetFileDate(strFileName));
		Parse(aModule, FontStyle::FROM_CUSTOM, pTable, lex);
	}

	AfxGetApp()->EndWaitCursor();

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL FontsParser::LoadFontAlias(FontStyleTable* pTable)
{
	CTBNamespace aNs (_NS_MOD("Module.Framework.TbWoormViewer"));	
	CString sFileName = AfxGetPathFinder()->GetModuleXmlPathCulture(aNs, CPathFinder::STANDARD, AfxGetLoginInfos()->m_strPreferredLanguage) + SLASH_CHAR + _T("FontsAlias.xml");
	if (!ExistFile (sFileName))
		return FALSE;

	CLocalizableXMLDocument aDocument(aNs, AfxGetPathFinder());
	if (!aDocument.LoadXMLFile(sFileName))
	{
		//aErrors.Add (_TB("Error formatting file FontsAlias.xml ") + aModule.ToString());
		return FALSE;
	}

	CXMLNode* pRoot = aDocument.GetRoot();
	CString sRootName;
	if (!pRoot || !pRoot->GetName(sRootName) || _tcsicmp(sRootName, _T("Fonts")) != 0) 
	{
		//aErrors.Add(_TB("The file FontsAlias.xml has no root element. File not loaded.") + (LPCTSTR) aModule.ToString());
		return FALSE;
	}

	CXMLNodeChildsList* pTagsNodes = aDocument.SelectNodes (_T("/Fonts/Font"));

	// se è vuoto lo salto
	if (!pTagsNodes || !pTagsNodes->GetSize())
	{
		if (pTagsNodes)
			delete pTagsNodes;
		return TRUE;
	}

	BOOL bOk = TRUE;
	
	// anche se un TAG è errato non mi fermo a caricare il file
	// così fornisco una diagnostica completa di tutto
	for (int i = 0; i <= pTagsNodes->GetUpperBound(); i++)
	{
		CXMLNode* pNode = pTagsNodes->GetAt(i);

		CString	sSrcName;
		if (!pNode->GetAttribute(_T("sourceName"), sSrcName) || sSrcName.IsEmpty())
		{
			continue;
		}
		CString	sDstName;
		if (!pNode->GetAttribute(_T("targetName"), sDstName) || sDstName.IsEmpty())
		{
			continue;
		}

		CString strTmp;
		int nSrcSize = 0;
		if (pNode->GetAttribute(_T("sourceSize"), strTmp) && !strTmp.IsEmpty())
		{
			nSrcSize = _tstoi((LPCTSTR)strTmp);
		}

		int nDstSize = 0;
		if (pNode->GetAttribute(_T("targetSize"), strTmp) && !strTmp.IsEmpty())
		{
			nDstSize = _tstoi((LPCTSTR)strTmp);
		}

		pTable->AddAlias(sSrcName, sDstName, nSrcSize, nDstSize);
	}
	delete pTagsNodes;
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL FontsParser::SaveFonts	(
								const CTBNamespace&	aModule, 
								const CPathFinder*	pPathFinder
							)	
{ 
	if (!pPathFinder || !aModule.IsValid())
		return FALSE;

	// non sono stati aggiunti stili e quelli base non sono cambiati
	if (!AfxGetFontStyleTable()->IsModified())
		return TRUE;

	Unparser		oFile;

	CStatusBarMsg	msgBar(TRUE, TRUE); 

	msgBar.Show(_TB("Saving Font Styles..."));

	AfxGetApp()->BeginWaitCursor();

	// salvo nella Custom
	CString sFileName = pPathFinder->GetFontsFullName(aModule, CPathFinder::CUSTOM, TRUE);

	// bisogna controllare che il file non sia read only
	DWORD dwAttr = GetTbFileAttributes ((LPCTSTR) sFileName);
	if (FILE_ATTRIBUTE_READONLY & dwAttr)
		SetFileAttributes((LPCTSTR) sFileName, dwAttr & !FILE_ATTRIBUTE_READONLY);

	CFileStatus stat;
	if (oFile.Open(sFileName) )
	{
		oFile.SetFormat(CLineFile::UTF8);
		Unparse(aModule, FontStyle::FROM_CUSTOM, AfxGetWritableFontStyleTable(), oFile);
	}

	AfxGetApp()->EndWaitCursor();

	return TRUE ;
}

// Se parsa i font style di applicazione richiede la release altrimenti (wrmeng)
// non bisogna parsare la release ma solo la tabella di stili
//------------------------------------------------------------------------------
BOOL FontsParser::Parse 
	(
		const	CTBNamespace&				aModule, 
		const	FontStyle::FontStyleSource	aSource,
				FontStyleTable*				pFontTable, 
				Parser&						lex
	)
{
    BOOL bOk = TRUE;

	m_Source = aSource;
	// parsa la release solo per gli stili di applicazione controllando
	// il tipo di parse da effettuare
	if (m_Source != FontStyle::FROM_WOORM)
	{
		int	nRelease;
		if (!lex.ParseTag (T_RELEASE) || !lex.ParseInt (nRelease))
			return FALSE;
	
		if (nRelease > m_nRelease)
			return lex.SetError(_TB("Incompatible Font Styles release.\r\nTable not loaded."));
    }

	// fonts style section exist
	if (lex.LookAhead(T_FONTSTYLES))
		return lex.ParseTag(T_FONTSTYLES) && ParseBlock	(aModule, pFontTable, lex);				

	return bOk;
}

// Se unparsa i font style di applicazione richiede la release altrimenti
//------------------------------------------------------------------------------
void FontsParser::Unparse	(
								const	CTBNamespace&				aModule, 
								const	FontStyle::FontStyleSource	aSource,
										FontStyleTablePtr			pFontTable, 
										Unparser&					ofile 
							)
{
	// se chiamata da wrmeng deve sempre salvare
	if (!pFontTable->IsModified())
		return;
 
	m_Source = aSource;

	// scrive  la release supportata
	if (m_Source != FontStyle::FROM_WOORM && m_Source != FontStyle::FROM_WOORM_TEMPLATE)
	{
		ofile.UnparseTag    (T_RELEASE,	FALSE);
		ofile.UnparseInt    (m_nRelease);
    }

	ofile.UnparseTag	(T_FONTSTYLES, FALSE);
	ofile.UnparseBlank	(TRUE);

	ofile.UnparseBegin	();
	BOOL bNotEmpty = UnparseStyles(aModule, pFontTable, ofile);
	ofile.UnparseEnd	();
	ofile.UnparseCrLf	();

	// avviso che è stata cambiata la data del file
	if (m_Source != FontStyle::FROM_WOORM && m_Source != FontStyle::FROM_WOORM_TEMPLATE)
		pFontTable->AddFileLoaded(aModule, m_Source, ::GetFileDate(ofile.GetFilePath()));

	if (bNotEmpty)
		return;

	// se è vuoto viene eliminato
	if (m_Source != FontStyle::FROM_WOORM && m_Source != FontStyle::FROM_WOORM_TEMPLATE)
	{
		CString strPath = ofile.GetFilePath();
		ofile.Close();
		DeleteFile((LPCTSTR) strPath);
		pFontTable->RemoveFileLoaded(aModule, m_Source);
	}
}

//------------------------------------------------------------------------------
BOOL FontsParser::ParseBlock(
								const	CTBNamespace&				aModule, 
										FontStyleTable*				pFontTable, 
										Parser&						lex
							)
{
	if (!lex.LookAhead(T_BEGIN))
		return ParseStyle(aModule, pFontTable, lex);
	
	return	lex.ParseBegin () && 
			ParseStyles	(aModule, pFontTable, lex) && 
			lex.ParseEnd ();
}

//------------------------------------------------------------------------------
BOOL FontsParser::ParseStyles	(
									const CTBNamespace&					aModule, 
									FontStyleTable*						pFontTable,
									Parser&								lex
								)
{
	BOOL ok = TRUE;

	do 
	{
		ok = ParseStyle(aModule, pFontTable, lex) && 
			!lex.Bad() 
			&& !lex.Eof();
	}
	while (ok && !lex.LookAhead(T_END));

    return ok;
}

//------------------------------------------------------------------------------
BOOL FontsParser::UnparseStyles (
									CTBNamespace						Ns,
									FontStyleTablePtr					pFontTable, 
									Unparser&							oFile
								) const
{
	BOOL bExist = FALSE;
    for (int i=0; i <= pFontTable->GetUpperBound (); i++)
	{
		FontStylesGroup* pGroup = pFontTable->GetAt(i);
		if (!pGroup)
			continue;

		for (int n = 0; n <= pGroup->GetFontStyles().GetUpperBound (); n++)
		{
			FontStyle* pStyle = (FontStyle*) pGroup->GetFontStyles().GetAt(n);
			if (pStyle->IsChanged() && pStyle->GetSource() == m_Source && pStyle->GetOwner() == Ns ) 
			{
				UnparseStyle (oFile, pStyle);
				bExist = TRUE;
			}
		}
	}
	return bExist;
}

//------------------------------------------------------------------------------
BOOL FontsParser::ParseStyle(
								const CTBNamespace&		aModule, 
								FontStyleTable*			pFontTable, 
								Parser&					lex
							)
{
	CString strStyleName;
	CString strFaceName;
	int		nSize;

	if (lex.LookAhead(T_ID))
	{
		CString sId;
		lex.ParseID(sId);
		if (_tcsicmp(sId, _T("name")))
			return FALSE;
	}

    // try to parse
    BOOL ok = 
			lex.ParseString		(strStyleName)	&&
			lex.ParseTag (T_FACENAME)	&&	lex.ParseString		(strFaceName) &&
			lex.ParseTag (T_SIZE)		&&  lex.ParseSignedInt	(nSize);
			
	// syntax error. abort parse
	if (!ok) return FALSE;

	// must change sign because unparse write nPositive height (see font documentation)
	// but LOGFONT need negative value for ignorate tmExternalLeading. Make also
	// conversion from Tipographic point to logical inch point (sett pag 664 of Petzold book)
	nSize = -((nSize * 100) / 72);

	FontStyle* pNewStyle = new FontStyle (strStyleName, strFaceName, aModule, m_Source, nSize, 0, FALSE, FALSE);
	if (!ParseStyleOption(lex, pNewStyle))
	{
		delete pNewStyle;
		return FALSE;
	}

	// l'area di applicazione è comune a tutti ed è in fondo
	if (lex.LookAhead(T_STR))
	{
		CString sArea;
		lex.ParseString (sArea);

		if (!sArea.IsEmpty())
			pNewStyle->SetLimitedArea(sArea);
	}

	if (!lex.ParseSep ())
	{
		delete pNewStyle;
		return FALSE;
	}

	// se il font esiste già da un messaggio di warning
	int nGroupIdx = pFontTable->GetFontIdx(pNewStyle->GetStyleName(), FALSE);
	FontStylesGroup* pGroup = nGroupIdx >= 0 ? pFontTable->GetAt(nGroupIdx) : NULL;
	FontStyle* pExisting;
	if (pGroup)
		for (int i=0; i <= pGroup->GetFontStyles().GetUpperBound(); i++)
		{
			pExisting = (FontStyle*) pGroup->GetFontStyles().GetAt(i);
			if (pExisting && pExisting->GetSource() == pNewStyle->GetSource() && pExisting->GetOwner() == pNewStyle->GetOwner())
				if (pNewStyle->GetOwner().GetType() == CTBNamespace::REPORT)
					lex.SetError (cwsprintf(_TB("Font style {0-%s} is twice defined in report {1-%s}."), (LPCTSTR) pNewStyle->GetStyleName(), (LPCTSTR) pNewStyle->GetOwner().ToUnparsedString()));
				else
					lex.SetError (cwsprintf(_TB("Font style {0-%s} is twice defined in file Fonts.ini of module {1-%s}."), (LPCTSTR) pNewStyle->GetStyleName(), (LPCTSTR) pNewStyle->GetOwner().ToUnparsedString()));
		}


	// add new font
	pFontTable->AddFont(pNewStyle);

	// set change new Font only if current is a WOORM FontStyleTable
	if (pFontTable != AfxGetFontStyleTable().GetPointer())
	{
		pNewStyle->SetChanged(TRUE);
		pFontTable->SetModified(TRUE);
	}
	else
		pNewStyle->SetChanged(m_Source == FontStyle::FROM_CUSTOM);

	return TRUE;
}

// write height with minus because for LOGFONT negative value indicate
// heigth of font size without tmExternalLeading (nPosistive value include it)
//------------------------------------------------------------------------------
void FontsParser::UnparseStyle(Unparser& ofile, FontStyle* pStyle) const
{
	// Make conversion from Tipographic point to logical inch point 
	// (see pag 664 of Petzold book). Invert sign for not include External Leading
	int nFontHeight = pStyle->GetLogFont().lfHeight;
	if (nFontHeight < 0)
		nFontHeight = nFontHeight * (-1);
	int nFontPitch = ((nFontHeight * 72) / 100) + 1;
	
	ofile.UnparseString	(pStyle->GetStyleName(),			FALSE);
	ofile.UnparseBlank	();
	ofile.UnparseTag	(T_FACENAME,						FALSE);
	ofile.UnparseString	(pStyle->GetLogFont().lfFaceName,	FALSE);
	ofile.UnparseBlank	();
	ofile.UnparseTag	(T_SIZE,							FALSE);
	ofile.UnparseInt	(nFontPitch,						FALSE);
	ofile.UnparseBlank	();

	UnparseStyleOption(ofile, pStyle);

	// area di applicazione
	if (!pStyle->GetLimitedArea().IsEmpty() && m_Source != FontStyle::FROM_WOORM && m_Source != FontStyle::FROM_WOORM_TEMPLATE)
		ofile.UnparseString(pStyle->GetLimitedArea(), FALSE);

	ofile.UnparseSep	();
	ofile.UnparseCrLf	();
}

//------------------------------------------------------------------------------
BOOL FontsParser::ParseStyleOption (Parser& lex, FontStyle* pStyle)
{
	LOGFONT aLogFont = pStyle->GetLogFont();

	// default starting value
	aLogFont.lfItalic		= FALSE;
	aLogFont.lfWeight		= FW_NORMAL;
	aLogFont.lfUnderline	= FALSE;
	aLogFont.lfStrikeOut	= FALSE;

	pStyle->SetColor	(TBThemeManager::GetFontDefaultForeColor());
	pStyle->SetLogFont	(aLogFont);

	// if style absent use default values
	if (!lex.LookAhead(T_STYLE)) 
		return TRUE;

	BOOL ok = lex.ParseTag(T_STYLE);

	do
	{
		switch (lex.LookAhead())
		{
			case T_ITALIC	: 
				ok = lex.SkipToken(); aLogFont.lfItalic = TRUE; 		pStyle->SetLogFont	(aLogFont); break;
			case T_BOLD		: 
				ok = lex.SkipToken(); aLogFont.lfWeight = FW_BOLD; 	pStyle->SetLogFont	(aLogFont); break;
			case T_UNDERLINE: 
				ok = lex.SkipToken(); aLogFont.lfUnderline = TRUE;	pStyle->SetLogFont	(aLogFont); break;
			case T_STRIKEOUT: 
				ok = lex.SkipToken(); aLogFont.lfStrikeOut = TRUE;	pStyle->SetLogFont	(aLogFont); break;
			case T_TEXTCOLOR:	
				{
					COLORREF aColor;
					ok = lex.ParseColor(T_TEXTCOLOR, aColor);				
					pStyle->SetColor(aColor);						
					break;
				}
			case T_ROUNDOPEN: 
				ok = ParseLogFont(lex, pStyle);					break;
			case T_EOF		: 
				ok = lex.SetError (_TB("Font: unexpected EOF"));	break;
			case T_END		: 
				ok = lex.SetError (_TB("Font: unexpected END"));	break;
			default			:  return ok;
		}
	}
	while (ok);

    return ok;
}

// Metodo di parsing con tutta la struttura del LOGFONT.
//------------------------------------------------------------------------------
BOOL FontsParser::ParseLogFont (Parser& lex, FontStyle* pStyle)
{
	COLORREF aColorRef = pStyle->GetColor();
	LOGFONT	 aLogFont  = pStyle->GetLogFont();
	BOOL bOk = 
		lex.ParseOpen	() &&
		lex.ParseLong	(aLogFont.lfWidth) &&			lex.ParseComma() &&
		lex.ParseLong	(aLogFont.lfEscapement) &&		lex.ParseComma() &&
		lex.ParseLong	(aLogFont.lfOrientation) &&		lex.ParseComma() &&
		lex.ParseLong	(aLogFont.lfWeight) &&			lex.ParseComma() &&
		lex.ParseByte	(aLogFont.lfItalic) &&			lex.ParseComma() &&
		lex.ParseByte	(aLogFont.lfUnderline) &&		lex.ParseComma() &&
		lex.ParseByte	(aLogFont.lfStrikeOut) &&		lex.ParseComma() &&
		lex.ParseByte	(aLogFont.lfCharSet) &&			lex.ParseComma() &&
		lex.ParseByte	(aLogFont.lfOutPrecision) &&	lex.ParseComma() &&
		lex.ParseByte	(aLogFont.lfClipPrecision) &&	lex.ParseComma() &&
		lex.ParseByte	(aLogFont.lfQuality) &&			lex.ParseComma() &&
		lex.ParseByte	(aLogFont.lfPitchAndFamily) &&	lex.ParseComma() &&
		lex.ParseColor	(T_TEXTCOLOR, aColorRef) &&
		lex.ParseClose	();
	
	pStyle->SetColor	(aColorRef);
	pStyle->SetLogFont	(aLogFont);

	return bOk;
}

//------------------------------------------------------------------------------
void FontsParser::UnparseStyleOption(Unparser& ofile, FontStyle* pStyle) const
{
	if	(
			pStyle->GetLogFont().lfWidth			!= 0 ||
			pStyle->GetLogFont().lfEscapement		!= 0 ||
			pStyle->GetLogFont().lfOrientation		!= 0 ||
			pStyle->GetLogFont().lfWeight			!= FW_NORMAL ||
			pStyle->GetLogFont().lfItalic			||
			pStyle->GetLogFont().lfUnderline		||
			pStyle->GetLogFont().lfStrikeOut		||
			pStyle->GetLogFont().lfCharSet			!= ANSI_CHARSET ||
			pStyle->GetLogFont().lfOutPrecision		!= OUT_DEFAULT_PRECIS ||
			pStyle->GetLogFont().lfClipPrecision	!= CLIP_DEFAULT_PRECIS ||
			pStyle->GetLogFont().lfQuality			!= DEFAULT_QUALITY ||
			pStyle->GetLogFont().lfPitchAndFamily	!= FF_DONTCARE ||
			pStyle->GetColor() != TBThemeManager::GetFontDefaultForeColor()
		)
	{   
		ofile.UnparseTag	(T_STYLE, FALSE);
		ofile.UnparseOpen	(FALSE);
		ofile.UnparseLong	(pStyle->GetLogFont().lfWidth,			FALSE);	ofile.UnparseComma();
		ofile.UnparseLong	(pStyle->GetLogFont().lfEscapement,		FALSE);	ofile.UnparseComma();
		ofile.UnparseLong	(pStyle->GetLogFont().lfOrientation,	FALSE);	ofile.UnparseComma();
		ofile.UnparseLong	(pStyle->GetLogFont().lfWeight,			FALSE);	ofile.UnparseComma();
		ofile.UnparseByte   (pStyle->GetLogFont().lfItalic,			FALSE);	ofile.UnparseComma();
		ofile.UnparseByte   (pStyle->GetLogFont().lfUnderline,		FALSE);	ofile.UnparseComma();
		ofile.UnparseByte   (pStyle->GetLogFont().lfStrikeOut,		FALSE);	ofile.UnparseComma();
		ofile.UnparseByte   (pStyle->GetLogFont().lfCharSet,		FALSE);	ofile.UnparseComma();
		ofile.UnparseByte   (pStyle->GetLogFont().lfOutPrecision,	FALSE);	ofile.UnparseComma();
		ofile.UnparseByte   (pStyle->GetLogFont().lfClipPrecision,	FALSE);	ofile.UnparseComma();
		ofile.UnparseByte   (pStyle->GetLogFont().lfQuality,		FALSE);	ofile.UnparseComma();
		ofile.UnparseByte   (pStyle->GetLogFont().lfPitchAndFamily,	FALSE);	ofile.UnparseComma();
		ofile.UnparseColor	(T_TEXTCOLOR, pStyle->GetColor(),		FALSE);
		ofile.UnparseClose	(FALSE);
	}                           
}

//-----------------------------------------------------------------------------
BOOL FontsParser::RefreshFonts	(
										FontStyleTablePtr	pTable,
										const CTBNamespace&	aModule, 
										const CPathFinder*	pPathFinder
									)
{
	if (!MustBeRefreshed(pTable, aModule, pPathFinder))
		return FALSE;

	CStatusBarMsg	msgBar(TRUE, TRUE); 
	// lo comunico subito anche se non dovesse funzionare il
	// LoadFont, comunque la Clean mi ha toccato la tabella.
	pTable->ClearFontsOf(aModule, FontStyle::FROM_CUSTOM);
	LoadFonts(pTable.GetPointer(), aModule, AfxGetPathFinder(), &msgBar, CPathFinder::CUSTOM);
	
	return TRUE;
}

// Verifica se le date dei files .INI sono state modifiche dall'ultimo load
// Gli standard non sono soggetti a modifiche e non devono!!
//----------------------------------------------------------------------------
BOOL FontsParser::MustBeRefreshed	(
										FontStyleTablePtr	pTable,
										const CTBNamespace&	aModule, 
										const CPathFinder*	pPathFinder
									)
{
	if (aModule.IsEmpty() || aModule.GetType() != CTBNamespace::MODULE || !pPathFinder)
		return FALSE;

	CString sFileName	= pPathFinder->GetFontsFullName(aModule, CPathFinder::CUSTOM);
	SYSTEMTIME aFileDate;
	aFileDate.wDay		= MIN_DAY;
	aFileDate.wMonth	= MIN_MONTH;
	aFileDate.wYear		= MIN_YEAR;
	aFileDate.wHour		= MIN_HOUR;
	aFileDate.wMinute	= MIN_MINUTE;
	aFileDate.wSecond	= MIN_SECOND;

	if (ExistFile(sFileName))
		aFileDate = GetFileDate(sFileName);

	SYSTEMTIME aTableDate = pTable->GetFileDate(aModule, FontStyle::FROM_CUSTOM);

	return DataDate(aTableDate) != DataDate(aFileDate);
}

