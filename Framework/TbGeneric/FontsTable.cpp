#include "stdafx.h"

#include <TbNameSolver\ApplicationContext.h>
#include <TBNameSolver\LoginContext.h>
#include <TbNameSolver\PathFinder.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>

#include "ParametersSections.h"
#include "TBThemeManager.h"

#include "FontsTable.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

const TCHAR szAreaSep[] = _T(",");

//-----------------------------------------------------------------------------
const FontStyleTable* AFXAPI AfxGetStandardFontStyleTable()
{
	return AfxGetApplicationContext()->GetObject<const FontStyleTable>(&CApplicationContext::GetStandardFontsTable);
}

//------------------------------------------------------------------------------
TB_EXPORT FontStyleTableConstPtr AFXAPI AfxGetFontStyleTable () 
{
	CLoginContext* pContext = AfxGetLoginContext();
	FontStyleTable* pTable = pContext
		? pContext->GetObject<FontStyleTable>(&CLoginContext::GetFontsTable)
		: NULL;

	return FontStyleTableConstPtr(pTable, FALSE);
}

//------------------------------------------------------------------------------
TB_EXPORT FontStyleTablePtr AFXAPI AfxGetWritableFontStyleTable () 
{
	return FontStyleTablePtr(AfxGetLoginContext()->GetObject<FontStyleTable>(&CLoginContext::GetFontsTable), TRUE);
}

//------------------------------------------------------------------------------
const FontAliasTable*	FontStyleTable::GetAliasTable()	const 	{ return &m_FontAliasTable; }

TB_EXPORT const FontAliasTable* AFXAPI AfxGetFontAliasTable () { return AfxGetWritableFontStyleTable()->GetAliasTable(); }

BOOL	FontStyleTable::GetUseVCenterBottomAlignInWoormFields()	const	{ return m_bUseVCenterBottomAlignInWoormFields; }
int		FontStyleTable::GetSizeOfDescriptionFont()						{ return m_nSizeOfDescriptionFont; }

void	FontStyleTable::SetUseVCenterBottomAlignInWoormFields(BOOL b)	{ m_bUseVCenterBottomAlignInWoormFields = b; }
void	FontStyleTable::SetSizeOfDescriptionFont(int size)				{ m_nSizeOfDescriptionFont = size; }


//==============================================================================
//			Class FontStyle
//==============================================================================
//
const TCHAR FontStyle::s_szFontDefault[] = _T("<Default>"); //ghost font style used by report template

//------------------------------------------------------------------------------
FontStyle::FontStyle
	(
	const	CString&		strStyleName,
	const	CString&		strFaceName,
	const	CTBNamespace&	aNamespace,
	const	FontStyleSource& aFromTo,
			int				nHeight,
			int				nWeight,
			BOOL			bItalic,
			BOOL			bUnderline,
			BOOL			bStrikeout,
            BOOL			bChanged,
			int				nEscapementOrientation/* = 0*/
	)
	:
	m_strStyleName		(strStyleName),
	m_OwnerModule		(aNamespace),
	m_bChanged			(bChanged),
	m_rgbColor			(TBThemeManager::GetFontDefaultForeColor()),
	m_bDeleted			(FALSE),
	m_FromAndTo			(aFromTo),
	m_pStandardFont		(NULL)
{
	m_LogFont.lfHeight		= nHeight;
	m_LogFont.lfWidth		= 0;
	m_LogFont.lfEscapement	= nEscapementOrientation;
	m_LogFont.lfOrientation	= nEscapementOrientation;
	m_LogFont.lfWeight		= nWeight; 
	m_LogFont.lfItalic		= (BYTE) bItalic;
	m_LogFont.lfUnderline	= (BYTE) bUnderline;
	m_LogFont.lfStrikeOut	= (BYTE) bStrikeout;
	m_LogFont.lfCharSet		= ANSI_CHARSET;
	
	m_LogFont.lfOutPrecision	= OUT_DEFAULT_PRECIS;
	m_LogFont.lfClipPrecision	= CLIP_DEFAULT_PRECIS;
	m_LogFont.lfQuality			= DEFAULT_QUALITY;
	m_LogFont.lfPitchAndFamily	= FF_DONTCARE;
	TB_TCSCPY(m_LogFont.lfFaceName, strFaceName);
}

//------------------------------------------------------------------------------
FontStyle::FontStyle (const FontStyle& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
const FontStyle& FontStyle::operator= (const FontStyle& source)
{
	m_LogFont.lfHeight			= source.m_LogFont.lfHeight;
	m_LogFont.lfWidth			= source.m_LogFont.lfWidth;
	m_LogFont.lfEscapement		= source.m_LogFont.lfEscapement;
	m_LogFont.lfOrientation		= source.m_LogFont.lfOrientation;
	m_LogFont.lfWeight			= source.m_LogFont.lfWeight;
	m_LogFont.lfItalic			= source.m_LogFont.lfItalic;
	m_LogFont.lfUnderline		= source.m_LogFont.lfUnderline;
	m_LogFont.lfStrikeOut		= source.m_LogFont.lfStrikeOut;
	m_LogFont.lfCharSet			= source.m_LogFont.lfCharSet;
	m_LogFont.lfOutPrecision	= source.m_LogFont.lfOutPrecision;
	m_LogFont.lfClipPrecision	= source.m_LogFont.lfClipPrecision;
	m_LogFont.lfQuality			= source.m_LogFont.lfQuality;
	m_LogFont.lfPitchAndFamily	= source.m_LogFont.lfPitchAndFamily;
	TB_TCSCPY(m_LogFont.lfFaceName, source.m_LogFont.lfFaceName);

	m_rgbColor				= source.m_rgbColor;
	m_bChanged				= source.m_bChanged;
	m_strStyleName			= source.m_strStyleName;
	m_OwnerModule			= source.m_OwnerModule;
	m_FromAndTo				= source.m_FromAndTo;
	m_bDeleted				= source.m_bDeleted;
	m_pStandardFont			= source.m_pStandardFont;

	m_LimitedContextArea.RemoveAll();
	for (int i=0; i <= source.m_LimitedContextArea.GetUpperBound(); i++)
		m_LimitedContextArea.Add(source.m_LimitedContextArea.GetAt(i));

	return *this;
}

//------------------------------------------------------------------------------
FontStyle::~FontStyle ()
{
}

//------------------------------------------------------------------------------
LOGFONT FontStyle::GetLogFont () const
{
    return m_LogFont;
}

//------------------------------------------------------------------------------
BOOL FontStyle::CreateFont(CFont& aFont) const
{
	return aFont.CreateFontIndirect(&m_LogFont) != NULL;
}

//------------------------------------------------------------------------------
const CString FontStyle::GetTitle () const 
{ 
	CString sTitle = AfxLoadFontString(m_strStyleName, m_OwnerModule);
	
	// se il font è di report, la traduzione potrebbe non essere disponibile
	// quindi provo a cercare nel suo standard per vedere se fossero disponibili
	if (sTitle.CompareNoCase(m_strStyleName) || m_OwnerModule.GetType() != CTBNamespace::REPORT || !m_pStandardFont)
		return sTitle;

	return m_pStandardFont->GetTitle(); 
}

//------------------------------------------------------------------------------
const CString FontStyle::GetLimitedArea () const
{
	CString s;
	for (int i=0; i <= m_LimitedContextArea.GetUpperBound(); i++)
		s = s + m_LimitedContextArea.GetAt(i) + 
		(i == m_LimitedContextArea.GetUpperBound() ? _T("") : szAreaSep);
    
	return s;
}

//------------------------------------------------------------------------------
const CStringArray& FontStyle::GetLimitedAreas () const
{
	return m_LimitedContextArea;
}

//------------------------------------------------------------------------------
 void FontStyle::SetLimitedArea (const CString& sArea) 
{
	if (sArea.IsEmpty())
		return;
	
	m_LimitedContextArea.RemoveAll ();

	int nCurrPos	= -1;
	int nNexSepPos	= 0;
	CString s;

	do
	{
		nNexSepPos	= sArea.Find(szAreaSep, nCurrPos+1);
		s = sArea.Mid(nCurrPos+1, nNexSepPos > 0 ? nNexSepPos - nCurrPos-1 : sArea.GetLength());
		m_LimitedContextArea.Add(s);
		nCurrPos = nNexSepPos;
	}
	while (nCurrPos >= 0 && nCurrPos <= sArea.GetLength());
}

//----------------------------------------------------------------------------
void FontStyle::SetLimitedAreas	(const CStringArray& aAreas)
{
	m_LimitedContextArea.RemoveAll();

	for (int i=0; i <= aAreas.GetUpperBound(); i++)
		m_LimitedContextArea.Add(aAreas.GetAt(i));
}

//------------------------------------------------------------------------------
void FontStyle::SetLogFont (LOGFONT lf) 
{
    m_LogFont.lfHeight			= lf.lfHeight;
	m_LogFont.lfWidth			= lf.lfWidth;
	m_LogFont.lfEscapement		= lf.lfEscapement;
	m_LogFont.lfOrientation		= lf.lfOrientation;
	m_LogFont.lfWeight			= lf.lfWeight;
	m_LogFont.lfItalic			= lf.lfItalic;
	m_LogFont.lfUnderline		= lf.lfUnderline;
	m_LogFont.lfStrikeOut		= lf.lfStrikeOut;
	m_LogFont.lfCharSet			= lf.lfCharSet;
	m_LogFont.lfOutPrecision	= lf.lfOutPrecision;
	m_LogFont.lfClipPrecision	= lf.lfClipPrecision;
	m_LogFont.lfQuality			= lf.lfQuality;
	m_LogFont.lfPitchAndFamily	= lf.lfPitchAndFamily;
	TB_TCSCPY(m_LogFont.lfFaceName, lf.lfFaceName);
}

//------------------------------------------------------------------------------
void FontStyle::SetStandardFont	(FontStyle* pFont)
{
	m_pStandardFont = pFont;
}

//------------------------------------------------------------------------------
CSize FontStyle::GetStringWidth (CDC* pDC, int nLen) const
{              
    CFont font; font.CreateFontIndirect(&m_LogFont);
    CSize cs = GetTextSize(pDC, nLen, &font);
	return cs;
}

//------------------------------------------------------------------------------
CSize FontStyle::GetStringWidth (CDC* pDC, const CString& str) const
{              
    CFont font; font.CreateFontIndirect(&m_LogFont);
    CSize cs = GetTextSize(pDC, str, &font);
	return cs;
}

//------------------------------------------------------------------------------
int FontStyle::GetHeight () const
{
	return m_LogFont.lfHeight;
}

//----------------------------------------------------------------------------
void FontStyle::Assign(const FontStyle& Fnt)
{
	SetLogFont(Fnt.m_LogFont);
	
	m_strStyleName			= Fnt.m_strStyleName;
	m_rgbColor				= Fnt.m_rgbColor;
	m_bChanged				= Fnt.m_bChanged;
	m_bDeleted				= Fnt.m_bDeleted;
	m_OwnerModule			= Fnt.m_OwnerModule;
	m_FromAndTo				= Fnt.m_FromAndTo;	
	m_pStandardFont			= Fnt.m_pStandardFont;

	m_LimitedContextArea.RemoveAll();
	for (int i=0; i <= Fnt.m_LimitedContextArea.GetUpperBound(); i++)
		m_LimitedContextArea.Add(Fnt.m_LimitedContextArea.GetAt(i));
}

//----------------------------------------------------------------------------
int FontStyle::Compare(const FontStyle& Fnt) const 
{
	if (m_strStyleName	!= Fnt.m_strStyleName	)	return 1;

	if (m_LogFont.lfHeight			!= Fnt.m_LogFont.lfHeight			) return 1;
	if (m_LogFont.lfWidth			!= Fnt.m_LogFont.lfWidth			) return 1;
	if (m_LogFont.lfEscapement		!= Fnt.m_LogFont.lfEscapement		) return 1;
	if (m_LogFont.lfOrientation		!= Fnt.m_LogFont.lfOrientation		) return 1;
	if (m_LogFont.lfWeight			!= Fnt.m_LogFont.lfWeight			) return 1;
	if (m_LogFont.lfItalic			!= Fnt.m_LogFont.lfItalic			) return 1;
	if (m_LogFont.lfUnderline		!= Fnt.m_LogFont.lfUnderline		) return 1;
	if (m_LogFont.lfStrikeOut		!= Fnt.m_LogFont.lfStrikeOut		) return 1;
	if (m_LogFont.lfCharSet			!= Fnt.m_LogFont.lfCharSet			) return 1;
	if (_tcscmp(m_LogFont.lfFaceName, Fnt.m_LogFont.lfFaceName) != 0	) return 1;
	
	if (m_rgbColor				!= Fnt.m_rgbColor			) return 1;
	if (m_bChanged				!= Fnt.m_bChanged			) return 1;
	if (m_bDeleted				!= Fnt.m_bDeleted			) return 1;
	if (m_OwnerModule			!= Fnt.m_OwnerModule		) return 1;
	if (m_FromAndTo				!= Fnt.m_FromAndTo			) return 1;
	if (m_LimitedContextArea.GetSize() != Fnt.m_LimitedContextArea.GetSize() ) return 1;

	for (int i=0; i < m_LimitedContextArea.GetSize(); i++)
		if (m_LimitedContextArea.GetAt(i) != Fnt.m_LimitedContextArea.GetAt(i))
			return 1;
		
	return 0;
}

//------------------------------------------------------------------------------
BOOL FontStyle::IsNoneFont () const
{
	return m_strStyleName.CompareNoCase(s_szFontDefault) == 0;
}

//==============================================================================
//			Class FontStylesGroup
//==============================================================================
//
//------------------------------------------------------------------------------
FontStylesGroup::FontStylesGroup (const CString& sName)
	:
	m_strStyleName(sName)
{
}

//------------------------------------------------------------------------------
FontStyle* FontStylesGroup::GetFontStyle (CTBNamespace* pContext) 
{
	if (!m_FontsStyles.GetSize())
		return NULL;

	return BestFontForContext(pContext);
}

//------------------------------------------------------------------------------
FontStyle* FontStylesGroup::GetFontStyle (const FontStyle::FontStyleSource& aSource) 
{
	FontStyle* pFont;
	for (int i=0; i <= m_FontsStyles.GetUpperBound(); i++)
	{
		pFont = (FontStyle*) m_FontsStyles.GetAt(i);
		if (pFont && pFont->GetSource() == aSource)
			return pFont;
	}

	return NULL;
}

//----------------------------------------------------------------------------
FontStyle* FontStylesGroup::GetFontStyle(FontIdx idxFont) 
{
	if (idxFont >= m_FontsStyles.GetSize())
		return NULL;

	return (FontStyle*) m_FontsStyles.GetAt(idxFont);	
}

//----------------------------------------------------------------------------
const CString FontStylesGroup::GetTitle() 
{
	FontStyle *pFont = GetFontStyle(0);
	return pFont ? pFont->GetTitle() : _T("");
}


//----------------------------------------------------------------------------
const Array& FontStylesGroup::GetFontStyles () const
{
	return m_FontsStyles;
}

//----------------------------------------------------------------------------
FontIdx FontStylesGroup::GetFontIdx(FontStyle* FontFrom, const FontStyle::FontStyleSource& aFontSource) // restituisce idx del array del gruppo del formatter
{
	if (!m_FontsStyles.GetSize ())
		return -1;

		for (int n = 0; n <= m_FontsStyles.GetUpperBound(); n++)
		{
			FontStyle* FontSource = (FontStyle*) m_FontsStyles.GetAt(n);
			if (
					FontSource->GetSource()== aFontSource && 
					FontSource->GetOwner()	== FontFrom->GetOwner()
			)	
				{
					return n;
				}
		}

	return -1;
}

//----------------------------------------------------------------------------
int	FontStylesGroup::AddFont (FontStyle* pFont)
{
	if (!pFont)
		ASSERT_TRACE(pFont,"Parameter pFont cannot be null");

	FontStyle* pStandardFont = GetFontStyle(FontStyle::FROM_STANDARD);

	int nIdx = m_FontsStyles.Add(pFont);
	
	// devo aggiornare il puntatore allo standard se esiste
	for (int i=0; i <= m_FontsStyles.GetUpperBound(); i++)
	{
		FontStyle* pFont = (FontStyle*) m_FontsStyles.GetAt(i);
		if (pFont)
			pFont->SetStandardFont(pStandardFont);
	}

	return nIdx;
}

//----------------------------------------------------------------------------
void FontStylesGroup::DeleteFont(FontStyle* pFontStyleToDel)
{
	if (!pFontStyleToDel)
	{
		ASSERT_TRACE(pFontStyleToDel,"Parameter pFontStyleToDel cannot be null");
		return;
	}

	FontIdx idxGroup = GetFontIdx(pFontStyleToDel, pFontStyleToDel->GetSource());
	if (idxGroup < 0)
		return;
	m_FontsStyles.RemoveAt(idxGroup);
}

// Si occupa di scegliere il font migliore da applicare secondo contesto. La
// scaletta delle priorità è la seguente:
//	1) il font corrispondente ad uno specifico namespace 
//	2) il font corrispondente alla stessa applicazione e modulo
//	3) il font corrispondente alla stessa applicazione	(il primo trovato)
//	4) il font corrispondente di altre applicazioni		(il primo trovato)
//	5) l'ultimo caricato
//	- a parità di font, il font custom è più forte di quello standard
//------------------------------------------------------------------------------
FontStyle* FontStylesGroup::BestFontForContext (CTBNamespace* pContext) const
{
	// l'unico possibile
	if (!pContext)
		return (FontStyle*) m_FontsStyles.GetAt(m_FontsStyles.GetUpperBound());

	CTBNamespace nsModule(
							CTBNamespace::MODULE, 
							pContext->GetApplicationName() 
							+ CTBNamespace::GetSeparator() + 
							pContext->GetObjectName(CTBNamespace::MODULE)
						);

	// Cerco il mio corrispondente preciso, e mi predispongo già quello 
	// con lo stesso nome di applicazione e/o con lo stesso nome di modulo
	FontStyle* pExactFont	= NULL;
	FontStyle* pAppFont		= NULL;
	FontStyle* pModFont		= NULL;
	FontStyle* pOtherAppFont= NULL;

	for (int i=0; i <= m_FontsStyles.GetUpperBound(); i++)
	{
		FontStyle* pFont = (FontStyle*) m_FontsStyles.GetAt(i);

		if (!pFont || pFont->IsDeleted())
			continue;

		if (pFont->GetSource() == FontStyle::FROM_WOORM)
			return pFont;

		if (pFont->GetSource() == FontStyle::FROM_WOORM_TEMPLATE && !pExactFont)
		{
			pExactFont = pFont;
			continue;
		}

		// ho il corrispondente identico
		if (pFont->GetOwner() == *pContext && HasPriority(pExactFont, pFont, pContext))
			pExactFont = pFont;	//TODO perchè non fa return ?

		// il primo trovato con la stessa applicazione
		if (
				pFont->GetOwner().GetApplicationName() == pContext->GetApplicationName() && 
				HasPriority(pAppFont, pFont, pContext)
			)
			pAppFont = pFont;

		// alcuni owner potrebbero essere Library, quindi
		// devo essere sicura di comparare bene i moduli
		CTBNamespace nsOwnerModule
			(
				CTBNamespace::MODULE,
				pFont->GetOwner().GetApplicationName()
				+ CTBNamespace::GetSeparator() + 
				pFont->GetOwner().GetObjectName(CTBNamespace::MODULE)
			);

		// il primo trovato con lo stesso modulo
		if (nsOwnerModule == nsModule && HasPriority(pModFont, pFont, pContext))
			pModFont = pFont;

		// il primo trovato di altre applicazioni
		if (
				pFont->GetOwner().GetApplicationName()!= pContext->GetApplicationName() && 
				HasPriority(pOtherAppFont, pFont, pContext)
			)
			pOtherAppFont = pFont;	
	}

	if (pExactFont)		return pExactFont;		// di stesso namespace (report di woorm)
	if (pModFont)		return pModFont;		// di modulo
	if (pAppFont)		return pAppFont;		// di applicazione
	if (pOtherAppFont)	return pOtherAppFont;	// di altre applicazioni

	// l'ultimo caricato
	return NULL;
}

// devo stare all'okkio a usare il custom dello stesso namespace preso prima
//----------------------------------------------------------------------------
BOOL FontStylesGroup::HasPriority (const FontStyle* pOld, const FontStyle* pNew, const CTBNamespace* pContext) const
{
	if (!pNew)
		return FALSE;

	BOOL bOkForArea = !pNew->GetLimitedAreas().GetSize();

	// area di applicazione del font
    if (!bOkForArea)
	{
		CString sArea;
		CTBNamespace nsModule(
								CTBNamespace::MODULE, 
								pContext->GetApplicationName()
								+ CTBNamespace::GetSeparator() + 
								pContext->GetObjectName(CTBNamespace::MODULE)
							);
		for (int i=0; i <= pNew->GetLimitedAreas().GetUpperBound(); i++)
		{
			sArea = pNew->GetLimitedAreas().GetAt(i);
			
			if	(
					*pContext == CTBNamespace(sArea) || 
					nsModule == sArea ||
					pContext->GetApplicationName().CompareNoCase(sArea) == 0
				)
			{
				bOkForArea = TRUE;
				break;
			}
		}
	}
	if (!pOld)
		return bOkForArea;

	return	bOkForArea &&
			pOld->GetOwner () == pNew->GetOwner() &&
			pOld->GetSource() == FontStyle::FROM_STANDARD && 
			pNew->GetSource() == FontStyle::FROM_CUSTOM;
}

//----------------------------------------------------------------------------
void FontStylesGroup::Assign(const FontStylesGroup& Fnt)
{
	m_strStyleName = Fnt.m_strStyleName;
	
	m_FontsStyles.RemoveAll();

	for (int i=0; i <= Fnt.m_FontsStyles.GetUpperBound(); i++)
		AddFont(new FontStyle(*((FontStyle*) Fnt.m_FontsStyles.GetAt(i))));
}

//------------------------------------------------------------------------------
const FontStylesGroup& FontStylesGroup::operator= (const FontStylesGroup& group)
{
	m_FontsStyles.RemoveAll();
	m_strStyleName = group.m_strStyleName;

	FontStyle* pNewFnt;
	FontStyle* pFont;
	for (int i = 0; i <= group.m_FontsStyles.GetUpperBound(); i++)
	{
		pFont = (FontStyle*) group.m_FontsStyles.GetAt(i);
		pNewFnt = new FontStyle(*pFont);
		AddFont(pNewFnt);
	}
	return *this;
}

//==============================================================================
//			Class FontStyleFile
//==============================================================================
//------------------------------------------------------------------------------
FontStyleFile::FontStyleFile (
								const CTBNamespace& aOwner,
								const FontStyle::FontStyleSource& aSource,
								const SYSTEMTIME& aFileDate
							  )
{
	m_Owner		= aOwner;
	m_Source	= aSource;
	m_dLastWrite= aFileDate;
}

//------------------------------------------------------------------------------
FontStyleFile::FontStyleFile (const FontStyleFile& aSource)
{
	*this = aSource;
}

//------------------------------------------------------------------------------
void FontStyleFile::SetFileDate (const SYSTEMTIME& aDate)
{
	m_dLastWrite = aDate;
}

//------------------------------------------------------------------------------
const FontStyleFile& FontStyleFile::operator= (const FontStyleFile& source)
{
	m_Owner		= source.m_Owner;
	m_Source	= source.m_Source;
	m_dLastWrite= source.m_dLastWrite;

	return *this;
}

//==============================================================================
//			Class FontStyleTable
//==============================================================================

//------------------------------------------------------------------------------
FontStyleTable::FontStyleTable()
	:
	m_bModified	(FALSE)
{
	m_arLoadedFiles.RemoveAll();

	m_bUseVCenterBottomAlignInWoormFields = FALSE;
	m_nSizeOfDescriptionFont = 8;
}

//------------------------------------------------------------------------------
FontStyleTable::FontStyleTable (const FontStyleTable& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
const FontStyleTable& FontStyleTable::operator= (const FontStyleTable& source)
{
	RemoveAll();

	m_bModified	= source.m_bModified;

	FontStylesGroup* pNewGroup;
	FontStylesGroup* pGroup;
	for (int i = 0; i <= source.GetUpperBound(); i++)
	{
		pGroup = source.GetAt(i);
		pNewGroup = new FontStylesGroup(_T(""));
		*pNewGroup = *pGroup;
		Add(pNewGroup);
	}

	CopyFileLoaded(source);

	m_bUseVCenterBottomAlignInWoormFields = source.m_bUseVCenterBottomAlignInWoormFields;
	m_nSizeOfDescriptionFont = source.m_nSizeOfDescriptionFont;

	return *this;
}

//----------------------------------------------------------------------------
void FontStyleTable::CopyFileLoaded (const FontStyleTable& fromFiles)
{
	// i files
	m_arLoadedFiles.RemoveAll();

	FontStyleFile* pFile;
	for (int i=0; i <= fromFiles.m_arLoadedFiles.GetUpperBound(); i++)
	{
		pFile = (FontStyleFile*) fromFiles.m_arLoadedFiles.GetAt(i);
		m_arLoadedFiles.Add(new FontStyleFile(*pFile));
	}
}

//------------------------------------------------------------------------------
FontStylesGroup* FontStyleTable::operator[](FontIdx nIndex) const
{
	if ((nIndex < 0) || (nIndex > GetUpperBound())) 
		nIndex = GetFontIdx(FNT_DEFAULT);
	
	return (FontStylesGroup*) Array::GetAt(nIndex);
}

//------------------------------------------------------------------------------
const CString& FontStyleTable::GetStyleName (FontIdx nIndex) const
{
	if ((nIndex < 0) || (nIndex > GetUpperBound()))
		nIndex = GetFontIdx(FNT_DEFAULT);

	return GetAt(nIndex)->GetStyleName();
}

// non torna errore
//------------------------------------------------------------------------------
FontIdx FontStyleTable::GetFontIdx (const CString& strStyleName, BOOL bInitDefault /*TRUE*/) const
{
	// se non lo trovo ritorno sempre il default
	int nDefIndex = -1;
	FontStylesGroup* pGroup;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pGroup = GetAt(i);
		
		if (!pGroup)
			continue;

		if (pGroup->GetStyleName().CompareNoCase(strStyleName) == 0)
			return i;

		if (nDefIndex == -1 && pGroup->GetStyleName().CompareNoCase(FNT_DEFAULT) == 0)
			nDefIndex = i;
	}

	return bInitDefault ? nDefIndex : FNT_ERROR;
}

//------------------------------------------------------------------------------
FontIdx FontStyleTable::GetLocalizedFontIdx (const CString& sLocalizedName) const
{
	// se non lo trovo ritorno errore
	FontStylesGroup* pGroup;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pGroup = GetAt(i);
		
		if (pGroup && AfxGetCultureInfo()->IsEqual(pGroup->GetTitle(), sLocalizedName))
			return i;
	}

	return FNT_ERROR;
}

//------------------------------------------------------------------------------
CString FontStyleTable::GetStyleTitle (FontIdx nIndex) const
{
	if ((nIndex < 0) || (nIndex > GetUpperBound()))
		nIndex = GetFontIdx(FNT_DEFAULT);

	return GetAt(nIndex)->GetTitle ();
}

//----------------------------------------------------------------------------
FontIdx FontStyleTable::GetFontIdx(FontStyle* FontFrom, const FontStyle::FontStyleSource aFontSource)
{
	FontIdx nIdxGroup = GetFontIdx(FontFrom->GetStyleName(), FALSE);
	if (nIdxGroup < 0)
		return nIdxGroup;

	int nStyle = GetAt(nIdxGroup)->GetFontIdx(FontFrom, aFontSource);
		
	if (nStyle >= 0)
		return nStyle;

	return -1;
}

//------------------------------------------------------------------------------
FontStyle* FontStyleTable::GetFontStyle (FontIdx index, CTBNamespace* pContext) const 
{
	if (index < 0 || index > GetUpperBound())
		return NULL;

	FontStylesGroup* pGroup = GetAt(index);

	if (pGroup)
		return pGroup->GetFontStyle(pContext);

	return NULL;
}

//------------------------------------------------------------------------------
FontStyle* FontStyleTable::GetFontStyle (FontStyle* aFontFrom, FontStyle::FontStyleSource aFontSource) 												
{
	FontStyle* pFont = NULL;
	FontIdx nIdxFont = GetFontIdx(aFontFrom, aFontSource);
	if (nIdxFont < 0)
		return pFont;

	FontIdx nIdxGroup = GetFontIdx(aFontFrom->GetStyleName());
	return GetAt(nIdxGroup)->GetFontStyle(nIdxFont);
}

//----------------------------------------------------------------------------
FontStyle* FontStyleTable::GetFontStyle (FontIdx index, FontStyle::FontStyleSource aFontSource)
{
	if (index < 0 || index > GetUpperBound())
		return FALSE;
	FontStylesGroup* pGroup = GetAt(index);

	return pGroup->GetFontStyle(aFontSource);
}

//----------------------------------------------------------------------------
int	FontStyleTable::AddFont (FontStyle* pFont)
{
	if (!pFont)
		return -1;

	// cerca se esiste già un formattatore con lo stesso nome rappresentato dal suo gruppo
	FontStylesGroup* pFontsGroup = NULL;
	for (int i = 0; i <= GetUpperBound(); i++)
		if (pFont->GetStyleName().CompareNoCase(GetAt(i)->GetStyleName()) == 0)
		{
			pFontsGroup = GetAt(i);
			break;
		}
	
	// se non c'è ancora il gruppo lo creo
	if (!pFontsGroup)
	{
		pFontsGroup = new FontStylesGroup(pFont->GetStyleName());
		Add(pFontsGroup);
	}

	// e poi aggiungo il font
	return pFontsGroup->AddFont(pFont);
}

// sistema la tabella dei fonts modificata eliminando i deleted e ritorna
// l'elenco dei namespace di moduli/report che hanno dei fonts variati,
// di cui andrebbe eseguito il salvataggio
//----------------------------------------------------------------------------
BOOL FontStyleTable::CheckFontTable	(CTBNamespaceArray&	aNsToSave, const CTBNamespace& aNsReport)
{
	if (!IsModified())
		return FALSE;

	// in caso di report, ritorna se bisogna 
	// fare unparse della sezione FontStyles
	BOOL bToSave = FALSE; 

	// gruppi di fonts
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		FontStylesGroup* pFontGrp = GetAt(i);
		
		if (!pFontGrp)
			continue;
		
		// elenco dei fonts
		for (int n = pFontGrp->GetFontStyles().GetUpperBound(); n >= 0 ; n--)
		{
			FontStyle* pFontStyle = (FontStyle*) pFontGrp->GetFontStyle(n);

			// sistemazione del Ns in caso di rinominazione nome report in save
			if (pFontStyle->GetOwner().GetType() == CTBNamespace::REPORT)
				pFontStyle->SetOwner(aNsReport);

			if (!pFontStyle->IsChanged())
				continue;

			BOOL bInToSaveList = FALSE;
			for (int s = 0; s <= aNsToSave.GetUpperBound(); s++)
				if (*aNsToSave.GetAt(s) == pFontStyle->GetOwner())
				{
					bInToSaveList = TRUE;
					break;
				}
		
			if (!bInToSaveList)
				aNsToSave.Add(new CTBNamespace(pFontStyle->GetOwner()));

			// cancellazione degli eliminati dalla tabella
			if (pFontStyle->IsDeleted())
				pFontGrp->DeleteFont(pFontStyle);	

			// woorm va sempre scritto
			else if (!bToSave && pFontStyle->GetSource() == FontStyle::FROM_WOORM) 
				bToSave = TRUE;
		}
	}

	return bToSave;
}

// Si occupa di eliminare i fonts relativi allo specifico Owner e sorgente
//----------------------------------------------------------------------------
void FontStyleTable::ClearFontsOf (const CTBNamespace& aOwner, const FontStyle::FontStyleSource& aSource)
{
	// gruppi di fonts
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		FontStylesGroup* pGroup = GetAt(i);
		
		if (!pGroup)
			continue;
		
		// elenco dei fonts
		for (int n = pGroup->GetFontStyles().GetUpperBound(); n >= 0 ; n--)
		{
			FontStyle* pStyle = (FontStyle*) pGroup->GetFontStyle(n);
			
			if (pStyle && pStyle->GetOwner() == aOwner && pStyle->GetSource() == aSource)
				pGroup->DeleteFont(pStyle);
		}
	}
}

//----------------------------------------------------------------------------
int FontStyleTable::AddFileLoaded (const CTBNamespace& aOwner, const FontStyle::FontStyleSource& aSource, const SYSTEMTIME& aDate)
{
	FontStyleFile* pFile;
	for (int i=0; i <= m_arLoadedFiles.GetUpperBound(); i++)
	{
		pFile = (FontStyleFile*) m_arLoadedFiles.GetAt(i);
		if (pFile && pFile->GetOwner() == aOwner && pFile->GetSource() == aSource)
		{
			pFile->SetFileDate(aDate);
			return i;
		}
	}

	return m_arLoadedFiles.Add(new FontStyleFile(aOwner, aSource, aDate));
}

//----------------------------------------------------------------------------
void FontStyleTable::RemoveFileLoaded (const CTBNamespace& aOwner, const FontStyle::FontStyleSource& aSource)
{
	FontStyleFile* pFile;
	for (int i=m_arLoadedFiles.GetUpperBound(); i >= 0 ; i--)
	{
		pFile = (FontStyleFile*) m_arLoadedFiles.GetAt(i);
		if (pFile && pFile->GetOwner() == aOwner && pFile->GetSource() == aSource)
		{
			m_arLoadedFiles.RemoveAt(i);
			return;
		}
	}
}
// è ammesso personalizzare sulla singola stazione il facename e il charset del font 
// usato per i control, in caso di utilizzo di localizzazione non standard inglese o 
// italiana (es.: croazia, slovenia). Il charset di default(ANSI) non contiene tutti 
// i caratteri, bisogna usare un altro charset dipendente dalla localizzazione.
//-----------------------------------------------------------------------------
void FontStyleTable::LoadApplicationCulture(CString sLanguage/* = _T("")*/)
{
	CTBNamespace aNs (szTbGenlibNamespace);
	CString sFileName = _T("Settings.config");
	BOOL bCustomCulture = FALSE;

	CString sCultureSection;
	if (sLanguage.IsEmpty() || sLanguage.CompareNoCase(_T("en")) == 0)
		sCultureSection = szCultureSection;
	else
	{
		sCultureSection.Format(_T("%s_%s"), szCultureSection, sLanguage);
		BOOL bNotFound = AfxGetBestSection(aNs, sCultureSection) == NULL;
		if (bNotFound)
		{
			int nIdx = sLanguage.Find('-');
			if (nIdx > 0)
			{
				sCultureSection.Format(_T("%s_%s"), szCultureSection, sLanguage.Left(nIdx));
				bNotFound = AfxGetBestSection(aNs, sCultureSection) == NULL;
			}
		}

		if (bNotFound)
		{
			sCultureSection = szCultureSection;
		}
		else
			bCustomCulture = TRUE;
	}

	//----
	//GESTIONE CUSTOMIZZAZIONE PRIMO-ULTIMO
	CCultureInfo *pCultureInfo = const_cast<CCultureInfo *>(AfxGetCultureInfo());

	DataStr sChLowerLimit, sChLowerLimitDefault;
	DataObj* pSetting = AfxGetSettingValue(aNs, sCultureSection, szLowerLimit, sFileName);
	if (pSetting && !pSetting->IsEmpty())
	{
		sChLowerLimit = pSetting->Str();
	} 
	else if (bCustomCulture)
	{
		pSetting = AfxGetSettingValue(aNs, szCultureSection, szLowerLimit, sChLowerLimitDefault, sFileName);
		sChLowerLimit = pSetting && !pSetting->IsEmpty() ? pSetting->Str() : sChLowerLimitDefault;
	}	
	
	if (!sChLowerLimit.IsEmpty())
		pCultureInfo->m_cCultureLowerLimit = sChLowerLimit[0];

	DataStr sChUpperLimit; sChUpperLimit += pCultureInfo->m_cCultureUpperLimit;
	DataStr sChUpperLimitDefault; sChUpperLimitDefault += pCultureInfo->m_cCultureUpperLimit;
	pSetting = AfxGetSettingValue(aNs, sCultureSection, szUpperLimit, sFileName);
	if (pSetting && !pSetting->IsEmpty())
	{
		sChUpperLimit = pSetting->Str();
	} 
	else if (bCustomCulture)
	{
		pSetting = AfxGetSettingValue(aNs, szCultureSection, szUpperLimit, sChUpperLimitDefault, sFileName);
		sChUpperLimit = pSetting && !pSetting->IsEmpty() ? pSetting->Str() : sChUpperLimitDefault;
	}	
	
	if (!sChUpperLimit.IsEmpty())
		pCultureInfo->m_cCultureUpperLimit = sChUpperLimit[0];
	
	//----
	//GESTIONE CUSTOMIZZAZIONE Font Description e Align VCENTER-BOTTOM
	DataInt	nDefaultSizeFontDescription (8);
	int nSize = nDefaultSizeFontDescription;
	pSetting = AfxGetSettingValue(aNs, sCultureSection, szSizeOfDescriptionFont, sFileName);
	if (pSetting && !pSetting->IsEmpty())
	{
		nSize = (*((DataInt*)pSetting) * 100) / 72;
	} 
	else if (bCustomCulture)
	{
		pSetting = AfxGetSettingValue(aNs, szCultureSection, szSizeOfDescriptionFont, nDefaultSizeFontDescription, sFileName);
		nSize = pSetting ? (*((DataInt*)pSetting) * 100) / 72 : nDefaultSizeFontDescription;
	}

	SetSizeOfDescriptionFont(nSize);

	BOOL bUseVCenterBottomAlignInWoormFields = TRUE;
	pSetting = AfxGetSettingValue(aNs, sCultureSection, szUseVCenterBottomAlignInWoormFields, sFileName);
	if (pSetting && !pSetting->IsEmpty())
	{
		bUseVCenterBottomAlignInWoormFields = (BOOL)(*((DataBool*)pSetting));
	}
	else if (bCustomCulture)
	{
		pSetting = AfxGetSettingValue(aNs, sCultureSection, szUseVCenterBottomAlignInWoormFields, DataBool(TRUE), sFileName);
		nSize = pSetting ? (BOOL)(*((DataBool*)pSetting)) : TRUE;
	}

	SetUseVCenterBottomAlignInWoormFields(bUseVCenterBottomAlignInWoormFields);

	CString sCharsSet;
	pSetting = AfxGetSettingValue(aNs, sCultureSection, szCharSetSample, DataStr(), sFileName);
	if (pSetting && !pSetting->IsEmpty())
	{
		sCharsSet = pSetting->Str();
	}
	else if (bCustomCulture)
	{
		pSetting = AfxGetSettingValue(aNs, sCultureSection, szCharSetSample, DataStr(), sFileName);
		sCharsSet = pSetting ? pSetting->Str() : _T("");
	}

	if (sCharsSet.GetLength())
		pCultureInfo->SetCharSetSample(sCharsSet);
}

//----------------------------------------------------------------------------
SYSTEMTIME FontStyleTable::GetFileDate (const CTBNamespace& aOwner, const FontStyle::FontStyleSource& aSource)
{
	FontStyleFile* pFile;
	for (int i=0; i <= m_arLoadedFiles.GetUpperBound(); i++)
	{
		pFile = (FontStyleFile*) m_arLoadedFiles.GetAt(i);
		if (pFile && pFile->GetOwner() == aOwner && pFile->GetSource() == aSource)
			return pFile->GetFileDate();
	}

	// null date
	SYSTEMTIME aTime;
	aTime.wDay		= MIN_DAY;
	aTime.wMonth	= MIN_MONTH;
	aTime.wYear		= MIN_YEAR;
	aTime.wHour		= MIN_HOUR;
	aTime.wMinute	= MIN_MINUTE;
	aTime.wSecond	= MIN_SECOND;

	return aTime;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

int FontAliasTable::Add (const CString& sSrcName, const CString& sDstName, int nSrcSize /*= 0*/, int nDstSize /*= 0*/)
{
	FontAlias* fa = new FontAlias(sSrcName, sDstName, nSrcSize, nDstSize);
	for (int i=0; i < m_arFontAlias.GetSize(); i++)
	{
		FontAlias* cur = (FontAlias*)(m_arFontAlias.GetAt(i));
		int c = sSrcName.CompareNoCase(cur->m_sSrcName);

		if (c < 0) { m_arFontAlias.InsertAt(i, fa); return i; }
		if (c > 0) continue;
		if (fa->m_nSrcSize < cur->m_nSrcSize) { m_arFontAlias.InsertAt(i, fa); return i; }
		if (fa->m_nSrcSize > cur->m_nSrcSize) continue;
	}
	return m_arFontAlias.Add(fa);
}

//-----------------------------------------------------------------------------
int FontAliasTable::Find (const CString& sName, int nSize, int startPos /*= 0*/) const
{
	ASSERT_TRACE2(startPos <= m_arFontAlias.GetSize(),"Wrong position: startPos = %d, m_arFontAlias.GetSize() = %d", startPos, m_arFontAlias.GetSize());
	int sameName = -1;

	for (int i = startPos; i < m_arFontAlias.GetSize(); i++)
	{
		FontAlias* cur = (FontAlias*)(m_arFontAlias.GetAt(i));
		int c = sName.CompareNoCase(cur->m_sSrcName);

		if (c < 0) continue; //ricerca principale
		if (c > 0) return -1; //facename non trovato

		if (nSize == 0) return i; //trovato: era una ricerca per solo facename

		sameName == -1 ? i : i -1; //precedente match con facename

		if (nSize < cur->m_nSrcSize) continue; //ricerca secondaria

		if (nSize > cur->m_nSrcSize) break; // return sameName;

		return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
CString FontAliasTable::LookupFaceName (const CString& sName) const
{
	int pos = Find(sName);
	if (pos < 0) return _T("");

	return GetAt(pos)->m_sDstName;
}
//-----------------------------------------------------------------------------
