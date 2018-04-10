#include "stdafx.h"

#include "chars.h"
#include "PathFinder.h"
#include "TBNamespaces.h"

// it must be last include
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

TCHAR szReportExtension[] = _T(".wrm");
TCHAR szReportExtensionRDE[] = _T(".rde");
TCHAR szReportExtensionTemplate[] = _T(".wrmt");

TCHAR szWordDocumentExtension[] = _T(".doc");
TCHAR szWordTemplateExtension[] = _T(".dot");
TCHAR szExcelDocumentExtension[] = _T(".xls");
TCHAR szExcelTemplateExtension[] = _T(".xlt");
TCHAR szTBJsonFileExt[] = _T(".tbjson");
TCHAR szBinFileExt[] = _T(".bin");

TCHAR szWordDocumentExtension2007[] = _T(".docx");
TCHAR szWordTemplateExtension2007[] = _T(".dotx");
TCHAR szExcelDocumentExtension2007[] = _T(".xlsx");
TCHAR szExcelTemplateExtension2007[] = _T(".xltx");

TCHAR szOdtExtension[] = _T(".odt");
TCHAR szOdtTemplateExtension[] = _T(".ott");
TCHAR szOdsExtension[] = _T(".ods");
TCHAR szOdsTemplateExtension[] = _T(".ots");



static const TCHAR	szNamespaceSep[] = _T(".");
static const TCHAR	szSingleNamespaceSep = _T('.');
static const TCHAR	szSingleTerminator = _T('\0');
static const TCHAR	szEmptyTokenSyntax[] = _T("..");
static const TCHAR	szEmptyTypeToken[] = _T("-.");
static const int	nTypePos = 0;
static const int	nAppPos = 1;
static const int	nModPos = 2;
static const int	nMinValidTokens = 3;
static const int	nStringBufferSize = 25;
static const int	nMinTokens = 11;

///////////////////////////////////////////////////////////////////////////////
// 						CTBNamespaceType:
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CTBNamespaceType, CObject)

//CTBNamespaceType::CTBNamespaceType() 
//{ 
//	CTBNamespaceType(CTBNamespace::NOT_VALID, L"");
//	ASSERT(FALSE);
//}

//-----------------------------------------------------------------------------
CTBNamespaceType::CTBNamespaceType(
	const int&		nType,
	const CString&	sPublicName,
	const int		nFixedTokens		/*-1*/,
	const BOOL		bHasLibrary			/*TRUE*/,
	const BOOL		bHasExtension		/*FALSE*/,
	const CString	sDefaultExtension	/*_T("")*/,
	const BOOL		bHasPathInside		/*FALSE*/,
	const CString	sFakeLibraryName	/*_T("")*/
)
{
	m_nType = nType;
	m_sPublicName = sPublicName;
	m_nFixedTokens = nFixedTokens;
	m_bHasLibrary = bHasLibrary;
	m_bHasExtension = bHasExtension;
	m_sDefaultExtension = sDefaultExtension;
	m_bHasPathInside = bHasPathInside;
	m_sFakeLibraryName = sFakeLibraryName;
}

///////////////////////////////////////////////////////////////////////////////
// 						NSTypesTable:
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
NSTypesTable::NSTypesTable()
{
	//------------
	// ATTENTION!!!! Table must be in the same order of CTBNamespace::NSObjectType enum!!!!

		// fixed position namespaces
	Add(new CTBNamespaceType(CTBNamespace::MODULE, _T("Module"), 2));
	Add(new CTBNamespaceType(CTBNamespace::LIBRARY, _T("Library"), 3));

	Add(new CTBNamespaceType(CTBNamespace::DOCUMENT, _T("Document"), 4, TRUE, FALSE, _T(""), FALSE));
	Add(new CTBNamespaceType(CTBNamespace::REPORT, _T("Report"), 3, FALSE, TRUE, szReportExtension));
	Add(new CTBNamespaceType(CTBNamespace::DBT, _T("Dbt"), 5));
	Add(new CTBNamespaceType(CTBNamespace::HOTLINK, _T("HotKeyLink"), 4));
	Add(new CTBNamespaceType(CTBNamespace::TABLE, _T("Table"), 4, TRUE, FALSE, _T(""), FALSE));
	Add(new CTBNamespaceType(CTBNamespace::FUNCTION, _T("Function"), 4));

	// namespace identifying files that don't contain library and has a variabile path inside
	Add(new CTBNamespaceType(CTBNamespace::IMAGE, _T("Image"), -1, FALSE, TRUE, _T(""), TRUE));
	Add(new CTBNamespaceType(CTBNamespace::TEXT, _T("Text"), -1, FALSE, TRUE, _T(""), TRUE));
	Add(new CTBNamespaceType(CTBNamespace::FILE, _T("File"), -1, FALSE, TRUE, _T(""), TRUE));

	Add(new CTBNamespaceType(CTBNamespace::VIEW, _T("View"), 4, TRUE, FALSE, _T(""), FALSE, _T("Views")));

	// variable position namespaces
	Add(new CTBNamespaceType(CTBNamespace::FORM, _T("Form"), -1, TRUE));
	Add(new CTBNamespaceType(CTBNamespace::TABBER, _T("Tabber"), -1, TRUE));
	Add(new CTBNamespaceType(CTBNamespace::TABDLG, _T("TabDlg"), -1, TRUE));
	Add(new CTBNamespaceType(CTBNamespace::GRID, _T("Grid"), -1, TRUE));
	Add(new CTBNamespaceType(CTBNamespace::GRIDCOLUMN, _T("GridColumn"), -1, TRUE));
	Add(new CTBNamespaceType(CTBNamespace::CONTROL, _T("Control"), -1, TRUE));

	Add(new CTBNamespaceType(CTBNamespace::EVENTHANDLER, _T("EventHandler"), 4));
	Add(new CTBNamespaceType(CTBNamespace::PROCEDURE, _T("Procedure"), 4, TRUE, FALSE, _T(""), FALSE, _T("Procedures")));

	// namespace identifying files that don't contain library
	Add(new CTBNamespaceType(CTBNamespace::DATAFILE, _T("DataFile"), 3, FALSE, TRUE, szXmlExt));

	//TODO ALIAS: allow to extend a section of framework settings
	//(updated in MicroareaConsole - ServicesPlugIn - Settings) path prefixes
	Add(new CTBNamespaceType(CTBNamespace::ALIAS, _T("Alias"), -1, FALSE, TRUE, _T(""), TRUE));

	Add(new CTBNamespaceType(CTBNamespace::PROFILE, _T("Profile"), 5));

	Add(new CTBNamespaceType(CTBNamespace::EXCELDOCUMENT, _T("ExcelDocument"), 3, FALSE, TRUE, szExcelDocumentExtension));
	Add(new CTBNamespaceType(CTBNamespace::EXCELTEMPLATE, _T("ExcelTemplate"), 3, FALSE, TRUE, szExcelTemplateExtension));
	Add(new CTBNamespaceType(CTBNamespace::WORDDOCUMENT, _T("WordDocument"), 3, FALSE, TRUE, szWordDocumentExtension));
	Add(new CTBNamespaceType(CTBNamespace::WORDTEMPLATE, _T("WordTemplate"), 3, FALSE, TRUE, szWordTemplateExtension));

	Add(new CTBNamespaceType(CTBNamespace::TOOLBARBUTTON, _T("ToolbarButton"), -1));

	Add(new CTBNamespaceType(CTBNamespace::REPORTSCHEMA, _T("ReportSchema"), 3, FALSE, TRUE, szExcelDocumentExtension));
	Add(new CTBNamespaceType(CTBNamespace::REPORTSCHEMA, _T("DocumentSchema"), 3, FALSE, TRUE, szExcelDocumentExtension));

	Add(new CTBNamespaceType(CTBNamespace::VIRTUAL_TABLE, _T("VirtualTable"), 4, TRUE, FALSE, _T(""), FALSE, _T("VirtualTables")));
	Add(new CTBNamespaceType(CTBNamespace::TOOLBAR, _T("Toolbar"), -1));

	Add(new CTBNamespaceType(CTBNamespace::ENTITY, _T("Entity"), 4, TRUE, FALSE, _T(""), FALSE));
	Add(new CTBNamespaceType(CTBNamespace::BEHAVIOUR, _T("Behaviour"), 4, TRUE, FALSE, _T(""), FALSE));

	Add(new CTBNamespaceType(CTBNamespace::PDF, _T("Pdf"), -1, FALSE, TRUE, _T(""), TRUE));
	Add(new CTBNamespaceType(CTBNamespace::RTF, _T("Rtf"), -1, FALSE, TRUE, _T(""), TRUE));

	Add(new CTBNamespaceType(CTBNamespace::ODS, _T("Ods"), 3, FALSE, TRUE, szOdsExtension));
	Add(new CTBNamespaceType(CTBNamespace::ODT, _T("Odt"), 3, FALSE, TRUE, szOdtExtension));

	Add(new CTBNamespaceType(CTBNamespace::BARPANEL, _T("BarPanel")));	//TODO aggiungere numero di segmenti di namespace

	Add(new CTBNamespaceType(CTBNamespace::TILEPANEL, _T("TilePanel"), -1, TRUE));
	Add(new CTBNamespaceType(CTBNamespace::TILEPANELTAB, _T("TilePanelTab"), -1, TRUE));
	Add(new CTBNamespaceType(CTBNamespace::JSON, _T("Json"), -1, FALSE, TRUE, szTBJsonFileExt, TRUE));
	Add(new CTBNamespaceType(CTBNamespace::ITEMSOURCE, _T("ItemSource")));
	Add(new CTBNamespaceType(CTBNamespace::HOTFILTER, _T("HotFilter"), 5));
	Add(new CTBNamespaceType(CTBNamespace::VALIDATOR, _T("Validator")));
	Add(new CTBNamespaceType(CTBNamespace::DATA_ADAPTER, _T("DataAdapter")));
	Add(new CTBNamespaceType(CTBNamespace::CONTEXT_MENU, _T("ContextMenu")));
	Add(new CTBNamespaceType(CTBNamespace::CONTROL_BEHAVIOUR, _T("ControlBehaviour")));

	// ATTENTION!!!! Table must be in the same order of CTBNamespace::NSObjectType enum!!!!
	//------------
}

//-----------------------------------------------------------------------------
NSTypesTable::~NSTypesTable()
{
	int n = GetSize();
	CObject* pO;
	for (int i = 0; i < n; i++)
		if (pO = (CTBNamespaceType*)CObArray::GetAt(i))
		{
			ASSERT_VALID(pO);
			delete pO;
		}
	CObArray::RemoveAll();
}

//-----------------------------------------------------------------------------
const CTBNamespaceType* NSTypesTable::GetAt(const CString& sName) const
{
	if (sName.IsEmpty())
		return NULL;

	CTBNamespaceType* pType;
	LPCTSTR pName = (LPCTSTR)sName;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pType = (CTBNamespaceType*)CObArray::GetAt(i);
		if (_tcsicmp((LPCTSTR)pType->m_sPublicName, pName) == 0)
			return pType;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
const CTBNamespaceType* NSTypesTable::GetTypeAt(const int& nType) const
{
	if (nType < 1 || nType > CObArray::m_nSize)
	{
		ASSERT(FALSE);
		return NULL;
	}

	return (const CTBNamespaceType*)CObArray::GetAt(nType - 1);
}

///////////////////////////////////////////////////////////////////////////////
// 						CTBNamespace:
///////////////////////////////////////////////////////////////////////////////
NSTypesTable CTBNamespace::m_NSTypeTable;

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTBNamespace, CObject)

//-----------------------------------------------------------------------------
CTBNamespace::CTBNamespace(const NSObjectType& aType /*CTBNamespace::NOT_VALID*/)
	:
	m_pCurrentType(NULL)
{
	if (aType != NOT_VALID)
		SetType(aType);
}

//-----------------------------------------------------------------------------
CTBNamespace::CTBNamespace(const CTBNamespace& aNamespace)
	:
	m_pCurrentType(NULL)
{
	SetNamespace(aNamespace);
}

//-----------------------------------------------------------------------------
CTBNamespace::CTBNamespace(const CString& strNamespace)
	:
	m_pCurrentType(NULL)
{
	SetNamespace(strNamespace);
};

//-----------------------------------------------------------------------------
CTBNamespace::CTBNamespace(const NSObjectType& aType, const CString& strNamespace)
	:
	m_pCurrentType(NULL)
{
	AutoCompleteNamespace(aType, strNamespace, CTBNamespace());
}

//-----------------------------------------------------------------------------
void CTBNamespace::Clear()
{
	m_sNamespace.Empty();
	m_aTokens.RemoveAll();
}

#define CMP_RETURN(token, jsonToken) if (_tcscmp(szName, L#jsonToken) == 0) return token;
//-----------------------------------------------------------------------------
CTBNamespace::NSObjectType CTBNamespace::FromString(LPCTSTR szName)
{
	CMP_RETURN(NOT_VALID, NotValid);
	CMP_RETURN(MODULE, Module);
	CMP_RETURN(LIBRARY, Library);
	CMP_RETURN(DOCUMENT, Document);
	CMP_RETURN(REPORT, Report);
	CMP_RETURN(DBT, Dbt);
	CMP_RETURN(HOTLINK, HotLink);
	CMP_RETURN(TABLE, Table);
	CMP_RETURN(FUNCTION, Function);
	CMP_RETURN(IMAGE, Image);
	CMP_RETURN(TEXT, Text);
	CMP_RETURN(FILE, File);
	CMP_RETURN(VIEW, View);
	CMP_RETURN(FORM, Form);
	CMP_RETURN(TABBER, Tabber);
	CMP_RETURN(TABDLG, TabDlg);
	CMP_RETURN(GRID, Grid);
	CMP_RETURN(GRIDCOLUMN, GridColumn);
	CMP_RETURN(CONTROL, Control);
	CMP_RETURN(EVENTHANDLER, EventHandler);
	CMP_RETURN(PROCEDURE, Procedure);
	CMP_RETURN(DATAFILE, DataFile);
	CMP_RETURN(ALIAS, Alias);
	CMP_RETURN(PROFILE, Profile);
	CMP_RETURN(EXCELDOCUMENT, ExcelDocument);
	CMP_RETURN(EXCELTEMPLATE, ExcelTemplate);
	CMP_RETURN(WORDDOCUMENT, WordDocument);
	CMP_RETURN(WORDTEMPLATE, WordTemplate);
	CMP_RETURN(TOOLBARBUTTON, ToolbarButton);
	CMP_RETURN(REPORTSCHEMA, ReportSchema);
	CMP_RETURN(DOCUMENTSCHEMA, DocumentSchema);
	CMP_RETURN(VIRTUAL_TABLE, VirtualTable);
	CMP_RETURN(TOOLBAR, Toolbar);
	CMP_RETURN(ENTITY, Entity);
	CMP_RETURN(BEHAVIOUR, Behaviour);
	CMP_RETURN(PDF, PDF);
	CMP_RETURN(RTF, RTF);
	CMP_RETURN(ODS, ODS);
	CMP_RETURN(ODT, ODT);
	CMP_RETURN(BARPANEL, BarPanel);
	CMP_RETURN(TILEPANEL, TilePanel);
	CMP_RETURN(TILEPANELTAB, TilePanelTab);
	CMP_RETURN(JSON, JSON);
	CMP_RETURN(ITEMSOURCE, ItemSource);
	CMP_RETURN(HOTFILTER, HotFilter);
	CMP_RETURN(VALIDATOR, Validator);
	CMP_RETURN(DATA_ADAPTER, DataAdapter);
	CMP_RETURN(CONTEXT_MENU, ContextMenu);
	CMP_RETURN(CONTROL_BEHAVIOUR, ControlBehaviour);
	return NOT_VALID;
}
// inline functions 
//-----------------------------------------------------------------------------
inline /*static*/ CString CTBNamespace::GetNotSupportedChars() { return CString(szNamespaceSep + CString(_T("/?*\\<>:|\" "))); }
inline /*static*/ CString CTBNamespace::GetSeparator() { return szNamespaceSep; }

inline const CString	CTBNamespace::GetTypeString() const { return m_pCurrentType ? m_pCurrentType->m_sPublicName : _T(""); }
inline const CString&	CTBNamespace::ToString() const { return m_sNamespace; }
inline const CString	CTBNamespace::GetObjectName() const { return m_pCurrentType ? GetObjectName((NSObjectType)m_pCurrentType->m_nType) : _T(""); }
inline const CString	CTBNamespace::GetApplicationName() const { return m_aTokens.GetUpperBound() >= nAppPos ? m_aTokens.GetAt(nAppPos) : _T(""); }
inline const CString	CTBNamespace::GetModuleName() const { return m_aTokens.GetUpperBound() >= nModPos ? m_aTokens.GetAt(nModPos) : _T(""); }
inline const BOOL		CTBNamespace::IsEmpty() const { return m_sNamespace.IsEmpty(); }

//-----------------------------------------------------------------------------
inline BOOL	CTBNamespace::IsEqual(const CTBNamespace& aNS) const
{
	return _tcsicmp((LPCTSTR)aNS.m_sNamespace, (LPCTSTR)m_sNamespace) == 0;
}

//-----------------------------------------------------------------------------
inline const CTBNamespace::NSObjectType CTBNamespace::GetType() const
{
	return m_pCurrentType ? (NSObjectType)m_pCurrentType->m_nType : CTBNamespace::NOT_VALID;
}

//-----------------------------------------------------------------------------
const BOOL CTBNamespace::IsValid() const
{
	// empty or not valid namespace
	if (!m_pCurrentType || m_sNamespace.IsEmpty())
		return FALSE;

	// if I have a variabile length namespace,
	// I check it for the number of current tokens 
	int nrOfTokens = 0;

	TCHAR* pChar = (TCHAR*)(LPCTSTR)m_sNamespace;

	while (*pChar != szSingleTerminator)
	{
		if (*pChar == szSingleNamespaceSep)
			nrOfTokens++;

		pChar++;
	}

	if (nrOfTokens)
		nrOfTokens++;

	// min token or type not valid 
	if (nrOfTokens <= 0 || nrOfTokens < nMinValidTokens)
		return FALSE;

	int nFixedTokens = m_pCurrentType->m_nFixedTokens;
	int nrOfTokensUBound = nrOfTokens - 1;

	// is a namespace with variable len
	if (nFixedTokens <= 0)
		nFixedTokens = nrOfTokensUBound;

	// if namespace manages extension, I have to add it
	else if (nrOfTokensUBound > (nFixedTokens + m_pCurrentType->m_bHasExtension))
		return FALSE;

	// is valid if there are not empty tokens
	return _tcsstr((LPCTSTR)m_sNamespace, szEmptyTokenSyntax) == NULL;
}

//-----------------------------------------------------------------------------
const CString CTBNamespace::GetToken(const int& nToken) const
{
	int nrNsTokensUBound = m_aTokens.GetUpperBound();
	if (nToken < 0 || nrNsTokensUBound <= 0 || nToken > nrNsTokensUBound || !m_pCurrentType)
		return _T("");

	// I have to return only type
	if (nToken == nTypePos)
		return m_pCurrentType->m_sPublicName;

	// I have to be careful with variable len namespaces
	int nrOfFixedTokens = m_pCurrentType ? m_pCurrentType->m_nFixedTokens : -1;

	BOOL bHasExtension = m_pCurrentType ? m_pCurrentType->m_bHasExtension : FALSE;
	if (nrOfFixedTokens <= 0)
		nrOfFixedTokens = nrNsTokensUBound - bHasExtension;

	CString sName = m_aTokens.GetAt(nToken);

	// If the last token is requested I have to consider extensions
	if (nToken >= nMinValidTokens && nToken == nrOfFixedTokens && bHasExtension && nToken < nrNsTokensUBound + bHasExtension)
	{
		CString s;
		for (int i = nToken + 1; i <= nrNsTokensUBound; i++)
		{
			s = m_aTokens.GetAt(i);
			if (!s.IsEmpty())
				sName += szNamespaceSep + s;
		}
	}

	return sName;
}

//-----------------------------------------------------------------------------
const BOOL CTBNamespace::HasAFakeLibrary() const
{
	CString sLibrary = GetObjectName(CTBNamespace::LIBRARY);
	return m_pCurrentType &&
		m_pCurrentType->m_bHasLibrary &&
		_tcscmp((LPCTSTR)m_pCurrentType->m_sFakeLibraryName, (LPCTSTR)sLibrary) == 0;
}

//-----------------------------------------------------------------------------
BOOL CTBNamespace::IsSameModule(const CTBNamespace& ns) const
{
	return this->GetApplicationName().CompareNoCase(ns.GetApplicationName()) == 0 &&
		this->GetModuleName().CompareNoCase(ns.GetModuleName()) == 0;
}

//-----------------------------------------------------------------------------
const CString CTBNamespace::GetObjectName(const NSObjectType& aType) const
{
	const CTBNamespaceType* pType = (m_pCurrentType && m_pCurrentType->m_nType == aType ? m_pCurrentType : m_NSTypeTable.GetTypeAt(aType));
	if (!pType)
		return _T("");

	int nLastPos = pType->m_nFixedTokens;

	// variable length and extensions
	if (nLastPos <= 0)
		nLastPos = m_aTokens.GetUpperBound() - pType->m_bHasExtension;

	return GetToken(nLastPos);
}

// Gets the tag name used in X-Tech xml documents. It substitute blanks with '-' char
//-----------------------------------------------------------------------------
const CString CTBNamespace::GetObjectNameForTag() const
{
	const NSObjectType aType = GetType();
	if (aType == CTBNamespace::NOT_VALID)
		return _T("");

	CString objName = GetObjectName(aType);

	objName.Replace(_T(" "), _T("-"));

	return objName;
}

// returns the variable path inside files namespace
//-----------------------------------------------------------------------------
const CString CTBNamespace::GetPathInside() const
{
	if (!m_pCurrentType || !m_pCurrentType->m_bHasPathInside)
	{
		TRACE(_T("GetPathInside call with a not supported namespace type."));
		return _T("");
	}

	CString sPath;

	// I calculate it from the intermediate tokens between module and object name
	for (int i = nModPos + 1; i < m_aTokens.GetUpperBound() - m_pCurrentType->m_bHasExtension; i++)
		sPath += m_aTokens.GetAt(i) + szNamespaceSep;

	// last separator
	if (sPath.Right(1) == szNamespaceSep)
		sPath = sPath.Left(sPath.GetLength() - 1);

	return sPath;
}

//-----------------------------------------------------------------------------
void CTBNamespace::SetNamespace(const CTBNamespace& aNamespace)
{
	if (this == &aNamespace)
		return;

	m_sNamespace = aNamespace.m_sNamespace;
	m_pCurrentType = aNamespace.m_pCurrentType;

	m_aTokens.RemoveAll();
	m_aTokens.Copy(aNamespace.m_aTokens);
}

//-----------------------------------------------------------------------------
void CTBNamespace::SetNamespace(const CString& sNamespace)
{
	// if I have to clear it I avoid ToStringArray
	if (sNamespace.IsEmpty())
	{
		m_sNamespace.Empty();
		m_aTokens.RemoveAll();
		m_pCurrentType = NULL;
		return;
	}

	ToStringArray(sNamespace, m_aTokens);

	m_sNamespace = sNamespace;
	m_pCurrentType = m_NSTypeTable.GetAt(m_aTokens.GetAt(nTypePos));
}

//-----------------------------------------------------------------------------
void CTBNamespace::SetType(const NSObjectType& aType)
{
	// optimization
	if (m_pCurrentType && m_pCurrentType->m_nType == (int)aType)
		return;

	// type token has never been set, I have to prepare shift
	// namespace string tokens in order to create the 0 token
	if (!m_pCurrentType && !m_sNamespace.IsEmpty() && m_aTokens.GetSize())
		m_sNamespace = szEmptyTypeToken + m_sNamespace;

	// even if the type is not valid I used it clear the type token 
	m_pCurrentType = m_NSTypeTable.GetTypeAt(aType);

	SetToken(nTypePos, m_pCurrentType ? m_pCurrentType->m_sPublicName : _T(""));
}

//-----------------------------------------------------------------------------
void CTBNamespace::SetApplicationName(const CString& sAppName)
{
	SetToken(nAppPos, sAppName);
}

//-----------------------------------------------------------------------------
void CTBNamespace::SetModuleName(const CString& sModuleName)
{
	SetToken(nModPos, sModuleName);
}

//-----------------------------------------------------------------------------
void CTBNamespace::SetPathInside(const CString& sPath)
{
	if (!m_pCurrentType || !m_pCurrentType->m_bHasPathInside)
	{
		ASSERT(FALSE);
		TRACE(_T("SetPathInside call with a not supported namespace type."));
		return;
	}

	CStringArray arTokens;
	ToStringArray(sPath, arTokens);

	// I erase the existing one
	for (int i = m_aTokens.GetUpperBound() - (1 + m_pCurrentType->m_bHasExtension); i >= nModPos + 1; i--)
		m_aTokens.RemoveAt(i);

	// and the I reinsert the intermediate tokens
	int nPosFinale = m_pCurrentType->m_nFixedTokens;
	if (nPosFinale <= 0)
		nPosFinale = max(m_aTokens.GetUpperBound(), nMinValidTokens + 1) - m_pCurrentType->m_bHasExtension;

	int bAdd = m_aTokens.GetUpperBound() < nPosFinale ? TRUE : FALSE;

	for (int i = 0; i <= arTokens.GetUpperBound(); i++)
		if (bAdd)
			m_aTokens.Add(arTokens.GetAt(i));
		else
			m_aTokens.InsertAt(m_aTokens.GetUpperBound() - 1, arTokens.GetAt(i));

	// m_pCurrentType has still the correct value
	m_sNamespace.Empty();
	for (int i = 0; i < m_aTokens.GetSize(); i++)
		m_sNamespace += m_aTokens.GetAt(i) + (i < m_aTokens.GetUpperBound() ? szNamespaceSep : _T(""));
}

//-----------------------------------------------------------------------------
void CTBNamespace::SetObjectName(const CString& sName, BOOL bSubstituteLastInVariableNamespace /*FALSE*/)
{
	SetObjectName((NSObjectType)m_pCurrentType->m_nType, sName, FALSE, bSubstituteLastInVariableNamespace);
}

//-----------------------------------------------------------------------------
void CTBNamespace::SetObjectName(const NSObjectType& aType, const CString& sName, BOOL bChangeType /*FALSE*/, BOOL bSubstituteLastInVariableNamespace /*FALSE*/)
{
	const CTBNamespaceType* pType = m_NSTypeTable.GetTypeAt(aType);
	if (!pType)
	{
		ASSERT(FALSE);
		TRACE(_T("SetObjectName call with a not supported namespace type."));
		return;
	}

	int nToken = pType ? pType->m_nFixedTokens : -1;

	if (bChangeType)
		SetType(aType);

	if (nToken < 0)
	{
		if (bSubstituteLastInVariableNamespace && m_aTokens.GetSize())
			SetToken(m_aTokens.GetUpperBound(), sName);
		else
			SetChildNamespace(aType, sName, *this);
	}
	else
		SetToken(nToken, sName);
}

//-----------------------------------------------------------------------------
const void CTBNamespace::SetToken(const int& nToken, const CString& sName)
{
	CString aName = sName;
	aName.Trim();

	// extension management
	if (
		nToken >= nMinValidTokens && m_pCurrentType && m_pCurrentType->m_bHasExtension &&
		m_aTokens.GetSize() == nToken && GetType() != NOT_VALID &&
		!sName.IsEmpty() && sName.FindOneOf(szNamespaceSep) < 0
		)
	{
		const CTBNamespaceType* pItem = m_NSTypeTable.GetTypeAt(GetType());
		CString sExt = pItem ? pItem->m_sDefaultExtension : _T("");

		aName = aName + pItem->m_sDefaultExtension;
	}

	// tokens array alignment
	if (m_aTokens.GetUpperBound() >= nToken)
		m_aTokens.SetAt(nToken, aName);

	else for (int i = m_aTokens.GetSize(); i <= nToken; i++)
		m_aTokens.Add(i < nToken ? _T(" ") : aName);

	// string recalculated
	m_sNamespace.Empty();
	for (int i = 0; i < m_aTokens.GetSize(); i++)
	{
		m_sNamespace += m_aTokens.GetAt(i);
		if (i < m_aTokens.GetUpperBound())
			m_sNamespace += szNamespaceSep;
	}
}

// It add a new token to an existing namespace
//-----------------------------------------------------------------------------
BOOL CTBNamespace::SetChildNamespace(const NSObjectType& aType, const CString& sName, const CTBNamespace& aParentNamespace)
{
	if (aType == CTBNamespace::NOT_VALID)
		return FALSE;

	SetNamespace(aParentNamespace.m_sNamespace + szNamespaceSep + sName);
	SetType(aType);

	return IsValid();
}

// It returns the namespace string without the type for unparsing operations
//-----------------------------------------------------------------------------
const CString CTBNamespace::ToUnparsedString() const
{
	if (m_sNamespace.IsEmpty() || !m_pCurrentType || m_pCurrentType->m_sPublicName.IsEmpty())
		return m_sNamespace;

	// I add the separator to retrieve
	CString sType = m_pCurrentType->m_sPublicName + szNamespaceSep;

	int nTypeLen = sType.GetLength();

	return	_tcsicmp((LPCTSTR)m_sNamespace.Left(nTypeLen), (LPCTSTR)sType) == 0 ?
		m_sNamespace.Mid(nTypeLen) :
		m_sNamespace;
}

//-----------------------------------------------------------------------------
const BOOL CTBNamespace::IsFromTaskBuilder()  const
{
	return _tcsicmp(
		(LPCTSTR)AfxGetPathFinder()->GetAppContainerName(GetApplicationName()),
		(TCHAR*)_T("Taskbuilder")
	) == 0;
}

// It complete namespace unifying a source and a partial string. It is assumed
// that the partial string it the final part of a namespace and the source is
// the one to use to clone the remaining tokens to complete namespace
//-----------------------------------------------------------------------------
BOOL CTBNamespace::AutoCompleteNamespace(const NSObjectType& aType, const CString& sNamespace, const CTBNamespace& aSourceNamespace)
{
	CString sPartialNamespace = sNamespace;
	if (sPartialNamespace.IsEmpty() && aSourceNamespace.m_sNamespace.IsEmpty() && aType == NOT_VALID)
	{
		Clear();
		return TRUE;
	}

	if (sPartialNamespace.IsEmpty())
	{
		if (!aSourceNamespace.IsValid())
			return FALSE;
		SetNamespace(aSourceNamespace);
		SetType(aType);
		return IsValid();
	}

	// corretto anomalia su assegnazione tipo doppio
	const CTBNamespaceType* pType = m_NSTypeTable.GetTypeAt(aType);
	if (pType)
	{
		CString sTypeToken = pType->m_sPublicName + szNamespaceSep;
		if (sPartialNamespace.Left(sTypeToken.GetLength()).CompareNoCase(sTypeToken) == 0)
			sPartialNamespace = sPartialNamespace.Mid(sTypeToken.GetLength());
	}

	// I calculate the number of tokens
	int nHowManyTokensToApply = 0;
	TCHAR* pChar = (TCHAR*)(LPCTSTR)sPartialNamespace;
	int nPartialTypePos = 0;

	while (*pChar != szSingleTerminator)
	{
		if (*pChar == szSingleNamespaceSep)
			nHowManyTokensToApply++;

		if (nHowManyTokensToApply == 0)
			nPartialTypePos++;

		pChar++;
	}

	if (nHowManyTokensToApply)
		nHowManyTokensToApply++;

	CString sPartialType = sPartialNamespace.Left(nPartialTypePos);
	const CTBNamespaceType* pPartialType = m_NSTypeTable.GetAt(sPartialType);

	//risolve il caso in cui sPartialNamespace ha lo stesso valore di un CTBNamespaceType (vedi DBTCustSuppForm il cui nome è Form) 
	m_pCurrentType = (pType->m_nType == CTBNamespace::NOT_VALID && pPartialType) ? pPartialType : pType;
	m_sNamespace = m_pCurrentType->m_sPublicName;

	// namespace with varable tokens
	if (pType->m_nFixedTokens < 0)
		return  SetChildNamespace(aType, sPartialNamespace, aSourceNamespace);

	// namespace with fized tokens
	m_sNamespace += szNamespaceSep;
	if (!aSourceNamespace.m_sNamespace.IsEmpty() && nHowManyTokensToApply < pType->m_nFixedTokens)
	{
		// I skip type
		for (int i = 1; i <= pType->m_nFixedTokens - nHowManyTokensToApply; i++)
			if (i <= aSourceNamespace.m_aTokens.GetUpperBound())
				m_sNamespace += aSourceNamespace.m_aTokens[i] + szNamespaceSep;
	}

	if (m_pCurrentType == pPartialType)
		m_sNamespace += sPartialNamespace.Mid(nPartialTypePos + 1);
	else
		m_sNamespace += sPartialNamespace;

	ToStringArray(m_sNamespace, m_aTokens);

	return IsValid();
}

// I cannot use Tokenize due to delimiters and I cannot use GetAt() instead of
// Mid() due to XCHAR conversion. Finally I have to pay attention to avoid the 
// GetToken calls recursion.
//-----------------------------------------------------------------------------
void CTBNamespace::ToStringArray(const CString& sNamespace, CStringArray& aTokens) const
{
	if (sNamespace.IsEmpty())
	{
		aTokens.RemoveAll();
		return;
	}

	CString sToken;
	// I split namespace into tokens
	TCHAR* pChar = (TCHAR*)(LPCTSTR)sNamespace;
	TCHAR* pToken = sToken.GetBuffer(nStringBufferSize);
	int nTokens = 0, nChars = 0, nBufferSize = nStringBufferSize;
	aTokens.SetSize(nMinTokens);

	while (true)
	{
		nChars++;
		if (nChars > nBufferSize)
		{
			nBufferSize = nBufferSize + nStringBufferSize;
			pToken = sToken.GetBuffer(nBufferSize);
			pToken += nChars - 1;
		}
		// token separator
		if (*pChar == szSingleNamespaceSep)
		{
			*pToken = szSingleTerminator;
			sToken.ReleaseBuffer();
			aTokens.SetAtGrow(nTokens, sToken);
			nTokens++;
			nChars = 0;
			pToken = sToken.GetBuffer(nBufferSize);
		}
		else if (*pChar == szSingleTerminator)
		{
			*pToken = szSingleTerminator;
			sToken.ReleaseBuffer();
			aTokens.SetAtGrow(nTokens, sToken);
			nTokens++;
			nChars = 0;
			break;
		}
		else
		{
			*pToken = *pChar;
			pToken++;
		}

		pChar++;
	}
	if (nTokens < nMinTokens)
		aTokens.SetSize(nTokens);
}

//-----------------------------------------------------------------------------
const CString CTBNamespace::Left(const NSObjectType& aType, const BOOL bIncludeType /*FALSE*/) const
{
	const CTBNamespaceType* pType = m_pCurrentType->m_nType == aType ? m_pCurrentType : m_NSTypeTable.GetTypeAt(aType);

	if (pType->m_nFixedTokens <= 0)
	{
		ASSERT(FALSE);
		TRACE("Cannot identify the token as it is a variable token numer namespace!");
		return _T("");
	}

	int nPos = 0, nStart = 0, nSeps = 0;
	while (nPos < m_sNamespace.GetLength())
	{
		if (m_sNamespace[nPos] == szSingleNamespaceSep)
		{
			nSeps++;

			if (nSeps == 1 && !bIncludeType)
				nStart = nPos + 1;
		}

		if (nSeps == (pType->m_nFixedTokens + 1) || nPos == m_sNamespace.GetLength())
			return m_sNamespace.Mid(nStart, nPos - nStart);

		nPos++;
	}

	return m_sNamespace;
}

//-----------------------------------------------------------------------------
const CString CTBNamespace::Right(const NSObjectType& aType) const
{
	const CTBNamespaceType* pType = m_pCurrentType->m_nType == aType ? m_pCurrentType : m_NSTypeTable.GetTypeAt(aType);

	if (pType->m_nFixedTokens <= 0)
	{
		ASSERT(FALSE);
		TRACE("Cannot identify the token as it is a variable token numer namespace!");
		return _T("");
	}

	CString sPartial;
	for (int i = pType->m_nFixedTokens + 1; i <= m_aTokens.GetUpperBound(); i++)
		sPartial += m_aTokens.GetAt(i) + (i < m_aTokens.GetUpperBound() ? szNamespaceSep : _T(""));

	return sPartial;
}

//-----------------------------------------------------------------------------
const CString CTBNamespace::GetRightTokens(const int& nrOfTokens) const
{
	if (IsEmpty())
		return _T("");

	if (nrOfTokens > m_aTokens.GetSize())
	{
		ASSERT(FALSE);
		return _T("");
	}

	CString sPartial;
	int nStartToken = m_aTokens.GetSize() - nrOfTokens;
	for (int i = nStartToken; i <= m_aTokens.GetUpperBound(); i++)
		sPartial += m_aTokens.GetAt(i) + (i < m_aTokens.GetUpperBound() ? szNamespaceSep : _T(""));

	return sPartial;
}


#ifdef _DEBUG
// diagnostic
//-----------------------------------------------------------------------------
void CTBNamespace::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nCTBNamespace :");
	AFX_DUMP1(dc, "\n\tm_sNamespace = ", m_sNamespace);
}
#endif //_DEBUG

///////////////////////////////////////////////////////////////////////////////
// 						CTBNamespaceArray:
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CTBNamespaceArray::CTBNamespaceArray()
{
}

//-----------------------------------------------------------------------------
CTBNamespaceArray::~CTBNamespaceArray()
{
	RemoveAll();
}

//----------------------------------------------------------------------------
void CTBNamespaceArray::RemoveAt(int nIndex, int nCount)
{
	int n = GetSize();
	int j = nCount;
	for (int i = nIndex; (i < n) && (j-- > 0); i++)
		if (GetAt(i)) delete GetAt(i);

	CObArray::RemoveAt(nIndex, nCount);
}

//-----------------------------------------------------------------------------
void CTBNamespaceArray::RemoveAll()
{
	int n = GetSize();
	CObject* pO;
	for (int i = 0; i < n; i++)
		if (pO = GetAt(i))
		{
			ASSERT_VALID(pO);
			delete pO;
		}

	CObArray::RemoveAll();
}

//-----------------------------------------------------------------------------
void CTBNamespaceArray::AddIfNotExists(const CTBNamespace& aToAdd)
{
	for (int n = 0; n <= GetUpperBound(); n++)
		if (aToAdd == *GetAt(n))
			return;

	Add(new CTBNamespace(aToAdd));
}

//-----------------------------------------------------------------------------
CTBNamespaceArray& CTBNamespaceArray::operator= (const CTBNamespaceArray& aArray)
{
	for (int i = 0; i <= aArray.GetUpperBound(); i++)
		Add(new CTBNamespace(*aArray.GetAt(i)));

	return *this;
}

//-----------------------------------------------------------------------------
int	CTBNamespaceArray::GetIndex(const CTBNamespace& aNamespace)
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (aNamespace == *GetAt(i))
			return i;

	return -1;
}

#ifdef _DEBUG
// diagnostic
//-----------------------------------------------------------------------------
void CTBNamespaceArray::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nCTBNamespaceArray");
}
#endif //_DEBUG
