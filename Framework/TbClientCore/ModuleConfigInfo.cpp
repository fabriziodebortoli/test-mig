#include "stdafx.h"

// library declaration

#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\ApplicationContext.h>

#include <TbXmlCore\XMLTags.h>

#include <TbGeneric\TbStrings.h>
#include <TbGeneric\DatabaseObjectsInfo.h>

#include "ModuleConfigInfo.h"

//includere come ultimo include all'inizio del cpp
//#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szXmlModuleInfo[]		= _T("ModuleInfo");
static const TCHAR szXmlModuleInfoKey[]		= _T("/ModuleInfo");
static const TCHAR szXmlDbObjectsInfo[]		= _T("/ModuleInfo/DbObjectsInfo");
static const TCHAR szXmlLibraryKey[]		= _T("/ModuleInfo/Components/Library");
static const TCHAR szXmlAssemblyKey[]		= _T("/ModuleInfo/Components/Assembly");

static const TCHAR szXmlName[]			= _T("name");
static const TCHAR szXmlAggregateName[]	= _T("aggregatename");
static const TCHAR szXmlLocalize[]		= _T("localize");
static const TCHAR szXmlComponents[]	= _T("Components");
static const TCHAR szXmlDeployPolicy[]	= _T("deploymentpolicy");
static const TCHAR szXmlSourceFolder[]	= _T("sourcefolder");
static const TCHAR szXmlRelease[]		= _T("release");
static const TCHAR szXmlSignature[]		= _T("signature");

//==============================================================================
//						Class CModuleConfigLibraryInfo
//==============================================================================
//
//------------------------------------------------------------------------------
CModuleConfigLibraryInfo::CModuleConfigLibraryInfo ()
	:
	m_bAssembly(FALSE)
{
}

//------------------------------------------------------------------------------
const CString CModuleConfigLibraryInfo::GetAlias () const
{
	return m_sSourceFolder.IsEmpty() ? m_sName : m_sSourceFolder;
}

//------------------------------------------------------------------------------
const BOOL& CModuleConfigLibraryInfo::IsAssembly () const
{
	return m_bAssembly;
}

//==============================================================================
//						Class CModuleConfigLibrariesInfo
//==============================================================================
//
//------------------------------------------------------------------------------
CModuleConfigLibrariesInfo::~CModuleConfigLibrariesInfo()
{
	RemoveAll();
}

//-----------------------------------------------------------------------------
void CModuleConfigLibrariesInfo::RemoveAll()
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

//==============================================================================
//						Class CModuleConfigContent
//==============================================================================
IMPLEMENT_DYNAMIC(CModuleConfigContent, CXMLSaxContent)

//------------------------------------------------------------------------------
CModuleConfigContent::CModuleConfigContent (CModuleConfigInfo* pConfigInfo)
	:
	m_pConfigInfo (pConfigInfo)
{
	if (m_pConfigInfo)
		m_pConfigInfo->Init ();
}
//------------------------------------------------------------------------------
CString	CModuleConfigContent::OnGetRootTag	() const
{
	return szXmlModuleInfo;
}

//------------------------------------------------------------------------------
void CModuleConfigContent::OnBindParseFunctions ()
{
	BIND_PARSE_ATTRIBUTES (szXmlModuleInfoKey,	&CModuleConfigContent::ParseModuleInfo);
	BIND_PARSE_ATTRIBUTES (szXmlLibraryKey,		&CModuleConfigContent::ParseDLL);
	BIND_PARSE_ATTRIBUTES (szXmlAssemblyKey,	&CModuleConfigContent::ParseAssembly);
}

//------------------------------------------------------------------------------
int CModuleConfigContent::ParseModuleInfo (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	if (!m_pConfigInfo)
		return CXMLSaxContent::ABORT;

	m_pConfigInfo->m_sName = ::GetName(GetPath(GetFileName()));
	m_pConfigInfo->m_Namespace.SetType(CTBNamespace::MODULE);
	m_pConfigInfo->m_Namespace.SetApplicationName(::GetName(GetPath(GetPath(GetFileName()))));
	m_pConfigInfo->m_Namespace.SetObjectName(m_pConfigInfo->m_sName);
	m_pConfigInfo->m_sNotLocalizedTitle = arAttributes. GetAttributeByName(szXmlLocalize);

	return CXMLSaxContent::OK;
}

//------------------------------------------------------------------------------
int CModuleConfigContent::ParseAssembly (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	return ParseLibrary(TRUE, arAttributes);
}

//------------------------------------------------------------------------------
int CModuleConfigContent::ParseDLL (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	return ParseLibrary(FALSE, arAttributes);
}

//------------------------------------------------------------------------------
int CModuleConfigContent::ParseLibrary (BOOL bIsAssembly, const CXMLSaxContentAttributes& arAttributes)
{
	BOOL bUsingAggregateNames = TRUE;
	CString sName= arAttributes.GetAttributeByName(szXmlAggregateName);
	if (sName.IsEmpty())
	{
		sName = arAttributes.GetAttributeByName(szXmlName);
		bUsingAggregateNames = FALSE;
	}


	if (sName.IsEmpty())
		return CXMLSaxContent::OK;

	CModuleConfigLibraryInfo* pNewLib = new CModuleConfigLibraryInfo();

	// tolgo l'eventuale estensione 
	pNewLib->m_sName = bIsAssembly ? sName : ::GetName(sName);
	
	pNewLib->m_sDeployPolicy = arAttributes.GetAttributeByName(szXmlDeployPolicy);
	pNewLib->m_sDeployPolicy = pNewLib->m_sDeployPolicy.Trim ();
	
	pNewLib->m_sSourceFolder = arAttributes.GetAttributeByName(szXmlSourceFolder);
	pNewLib->m_sSourceFolder.Trim ();
	pNewLib->m_bAssembly = bIsAssembly;

	if (!pNewLib->m_sName.IsEmpty())
	{
		if (m_pConfigInfo->m_sLibrarieNames.IsEmpty())
			m_pConfigInfo->m_sLibrarieNames = m_pConfigInfo->GetNamesSeparator();

		CString sAlias = pNewLib->GetAlias();
		
		if (!sAlias.IsEmpty())
			m_pConfigInfo->m_sLibrarieNames +=	pNewLib->GetAlias() + 
												m_pConfigInfo->GetNamesSeparator();

		// Compatibilty with not aggregated dlls that don't use aliases
		// If alias is different from library name, I have to resolve both	
		// Aggregate names are surely different and duplicated so I cannot add them
		if (
				!bUsingAggregateNames && 
				_tcsicmp(sAlias, pNewLib->GetLibraryName()) != 0
			)
			m_pConfigInfo->m_sLibrarieNames +=	pNewLib->GetLibraryName() +
												m_pConfigInfo->GetNamesSeparator();
	}

	m_pConfigInfo->m_sLibrarieNames.MakeLower();

	if (IsOkLibraryInfo(pNewLib))
		m_pConfigInfo->m_Libraries.Add (pNewLib);
	else
	{
		delete pNewLib;
		return CXMLSaxContent::ABORT;
	}

	return CXMLSaxContent::OK;
}

//-----------------------------------------------------------------------------
BOOL CModuleConfigContent::IsOkLibraryInfo	(CModuleConfigLibraryInfo* pInfo)
{
	if (pInfo->GetLibraryName().IsEmpty())
	{
		CString sTag = pInfo->IsAssembly() ? _T("<Assembly>") : _T("<Library>");
		AddError (_T("There is a nameless tag ") + sTag);
	}

	CString sPolicy = pInfo->GetDeployPolicy();
	if (sPolicy.IsEmpty())
	{
		AddError (_T("Attribute deploymentpolicy= missing in library") + pInfo->GetLibraryName() + _T("\n"));
		return FALSE;
	}
	
	LPCTSTR pDeployPolicy = (LPCTSTR) sPolicy;
	if (
			_tcsicmp(pDeployPolicy, XML_DEPLOYPOLICY_BASE_VALUE) && 
			_tcsicmp(pDeployPolicy, XML_DEPLOYPOLICY_FULL_VALUE) &&
			_tcsicmp(pDeployPolicy, XML_DEPLOYPOLICY_ADDON_VALUE)
		)
	{
		AddError ( _T("Attribute deploymentpolicy= must be 'base' or 'full' or 'addon' in library") + pInfo->GetLibraryName() + _T("\n"));
		return FALSE;
	}
	
	return TRUE;
}

//==============================================================================
//						Class CModuleConfigInfo
//==============================================================================
//
//------------------------------------------------------------------------------
CModuleConfigInfo::CModuleConfigInfo ()
{
}

//------------------------------------------------------------------------------
CModuleConfigInfo::~CModuleConfigInfo ()
{
	m_Libraries.RemoveAll ();
}

//------------------------------------------------------------------------------
void CModuleConfigInfo::Init ()
{
	m_Libraries.RemoveAll ();
	m_sNotLocalizedTitle.Empty ();
}


//-----------------------------------------------------------------------------
const TCHAR CModuleConfigInfo::GetNamesSeparator ()
{
	return _T(',');
}

//-----------------------------------------------------------------------------
CString	CModuleConfigInfo::GetDeployPolicyOf (const CString& sLibraryAlias) const
{
	LPCTSTR pLibraryAlias = (LPCTSTR) sLibraryAlias;
	CModuleConfigLibraryInfo* pInfo;
	for (int i=0; i <= m_Libraries.GetUpperBound (); i++)
	{
		pInfo = m_Libraries.GetAt(i);
		if (pInfo && _tcsicmp(pLibraryAlias, pInfo->GetAlias()) == 0)
			return pInfo->GetDeployPolicy ();
	}

	return _T("");
}

//-----------------------------------------------------------------------------
BOOL CModuleConfigInfo::IsFullDeployPolicy	(const CString& sLibraryAlias) const
{
	return _tcsicmp(GetDeployPolicyOf(sLibraryAlias), XML_DEPLOYPOLICY_FULL_VALUE) == 0;
}

//-----------------------------------------------------------------------------
BOOL CModuleConfigInfo::IsAddOnDeployPolicy (const CString& sLibraryAlias) const
{
	return _tcsicmp(GetDeployPolicyOf(sLibraryAlias), XML_DEPLOYPOLICY_ADDON_VALUE) == 0;
}

//-----------------------------------------------------------------------------
BOOL CModuleConfigInfo::IsBaseDeployPolicy (const CString& sLibraryAlias) const
{
	return _tcsicmp( GetDeployPolicyOf(sLibraryAlias), XML_DEPLOYPOLICY_BASE_VALUE) == 0;
}

//-----------------------------------------------------------------------------
BOOL CModuleConfigInfo::HasLibrary (const CString& sLibraryAlias) const
{
	CString name  = GetNamesSeparator() + sLibraryAlias + GetNamesSeparator();
	name.MakeLower();

	return m_sLibrarieNames.Find (name) >=0;
}

//-----------------------------------------------------------------------------
CString CModuleConfigInfo::ResolveLibrary (const CTBNamespace& nsLibrary) const
{
	CString sLibName = nsLibrary.GetObjectName(CTBNamespace::LIBRARY);

	CModuleConfigLibraryInfo* pInfo;
	for (int i=0; i <= m_Libraries.GetUpperBound (); i++)
	{
		pInfo = m_Libraries.GetAt(i);
		if	(
				pInfo && 
				!pInfo->GetSourceFolder().IsEmpty() && 
				_tcsicmp(sLibName,pInfo->GetSourceFolder()) == 0
			)
			return pInfo->GetLibraryName();
	}
	
	return sLibName;
}

// Ritorna un array di nomi di librerie da caricare in memoria per il modulo
//-----------------------------------------------------------------------------
CStringArray* CModuleConfigInfo::GetLibraries () const
{
	CStringArray* pLibNames = new CStringArray();

	CModuleConfigLibraryInfo* pInfo;
	for (int i=0; i <= m_Libraries.GetUpperBound (); i++)
	{
		pInfo = m_Libraries.GetAt(i);
		if (pInfo && !pInfo->IsAssembly())
			pLibNames->Add (pInfo->GetLibraryName());
	}

	return pLibNames;
}

// Ritorna un array di nomi di librerie da caricare in memoria per il modulo
//-----------------------------------------------------------------------------
CStringArray* CModuleConfigInfo::GetLibrariesAliases () const
{
	CStringArray* pLibNames = new CStringArray();

	CModuleConfigLibraryInfo* pInfo;
	for (int i=0; i <= m_Libraries.GetUpperBound (); i++)
	{
		pInfo = m_Libraries.GetAt(i);
		if (pInfo)
			pLibNames->Add (pInfo->GetAlias());
	}

	return pLibNames;
}

//-----------------------------------------------------------------------------
const CString CModuleConfigInfo::GetLibrariesNames () const
{
	CString sLibNames (GetNamesSeparator());

	CStringArray* pLibNames = GetLibraries();
	for (int i=0; i <= pLibNames->GetUpperBound(); i++)
		sLibNames += pLibNames->GetAt(i) + GetNamesSeparator();

	delete pLibNames;

	return sLibNames;
}

//-----------------------------------------------------------------------------
const CString CModuleConfigInfo::GetTitle() const
{
	return AfxLoadXMLString
			(
				m_sNotLocalizedTitle, 
				AfxGetPathFinder()->GetModuleConfigName(),
				AfxGetDictionaryPathFromNamespace(m_Namespace, TRUE)
			);
}

//-----------------------------------------------------------------------------
const BOOL CModuleConfigInfo::HasLibraries () const 
{ 
	return m_Libraries.GetSize() > 0; 
}