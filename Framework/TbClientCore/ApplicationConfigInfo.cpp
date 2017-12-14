
#include "stdafx.h"

// library declaration
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\Chars.h>

#include <TbGeneric\TbStrings.h>

#include "ApplicationConfigInfo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

// attributes
static const TCHAR szXmlName[]			= _T("name");
static const TCHAR szXmlLocalize[]		= _T("localize");

// xPath queries 
static const TCHAR szXmlApplicationInfo[]		= _T("ApplicationInfo");
static const TCHAR szXmlApplicationInfoTag[]	= _T("/ApplicationInfo");
static const TCHAR szTypeKey[]					= _T("/ApplicationInfo/Type");
static const TCHAR szDbSignatureKey[]			= _T("/ApplicationInfo/DbSignature");
static const TCHAR szVersionKey[]				= _T("/ApplicationInfo/Version");

//==============================================================================
//						Class CApplicationConfigContent
//==============================================================================
IMPLEMENT_DYNAMIC(CApplicationConfigContent, CXMLSaxContent)

//------------------------------------------------------------------------------
CApplicationConfigContent::CApplicationConfigContent (CApplicationConfigInfo* pConfigInfo)
	:
	m_pConfigInfo (pConfigInfo)
{
}

//------------------------------------------------------------------------------
void CApplicationConfigContent::OnBindParseFunctions ()
{
	BIND_PARSE_TAG	(szTypeKey,			&CApplicationConfigContent::ParseType);
	BIND_PARSE_TAG	(szDbSignatureKey,	&CApplicationConfigContent::ParseDbSignature);
	BIND_PARSE_TAG	(szVersionKey,		&CApplicationConfigContent::ParseVersion);
}


//------------------------------------------------------------------------------
CString	CApplicationConfigContent::OnGetRootTag	() const
{
	if (m_pConfigInfo)
		m_pConfigInfo->m_sName = GetName (::GetPath(GetFileName()));

	return szXmlApplicationInfo;
}

//------------------------------------------------------------------------------
int CApplicationConfigContent::ParseType (const CString& sUri, const CString& sTagValue)
{
	if (!m_pConfigInfo)
		return CXMLSaxContent::ABORT;

	CPathFinder::ApplicationType aType = AfxGetPathFinder()->StringToApplicationType (sTagValue);

	m_pConfigInfo->m_bTbApplication = (aType != CPathFinder::UNDEFINED);
	
	return CXMLSaxContent::OK;
}

//------------------------------------------------------------------------------
int CApplicationConfigContent::ParseDbSignature (const CString& sUri, const CString& sTagValue)
{
	if (!m_pConfigInfo)
		return CXMLSaxContent::ABORT;

	if (sTagValue.IsEmpty())
	{
		AddError (_T("Empty Db Signature found "));
		return CXMLSaxContent::ABORT;
	}

	m_pConfigInfo->m_sDbSignature = sTagValue;
	
	return CXMLSaxContent::OK;
}

//------------------------------------------------------------------------------
int CApplicationConfigContent::ParseVersion (const CString& sUri, const CString& sTagValue)
{
	if (!m_pConfigInfo)
		return CXMLSaxContent::ABORT;

	if (sTagValue.IsEmpty() && m_pConfigInfo->m_sVersion.IsEmpty())
		m_pConfigInfo->m_sVersion = _T("1.0.0.0");
	else
		m_pConfigInfo->m_sVersion = sTagValue;

	return CXMLSaxContent::OK;
}

//==============================================================================
//						Class CApplicationConfigInfo
//==============================================================================
//
//------------------------------------------------------------------------------
CApplicationConfigInfo::CApplicationConfigInfo ()
{
	Init ();
}

//------------------------------------------------------------------------------
CApplicationConfigInfo::~CApplicationConfigInfo ()
{
}

//------------------------------------------------------------------------------
void CApplicationConfigInfo::Init ()
{	
	m_bTbApplication = FALSE;

	m_sDbSignature = _T("DEFAULT");
}

//==============================================================================
//						Class CLocalizableApplicationConfigContent
//==============================================================================
IMPLEMENT_DYNAMIC(CLocalizableApplicationConfigContent, CXMLSaxContent)

//------------------------------------------------------------------------------
CLocalizableApplicationConfigContent::CLocalizableApplicationConfigContent (CLocalizableApplicationConfigInfo* pConfigInfo)
	:
	m_pConfigInfo (pConfigInfo)
{
}

//------------------------------------------------------------------------------
CString	CLocalizableApplicationConfigContent::OnGetRootTag	() const
{
	if (m_pConfigInfo)
	{
		CString sFilePath = ::GetPath(GetFileName());
		CString sAppName = GetName (GetPath(sFilePath));
		CString sModName = GetName(sFilePath);

		m_pConfigInfo->m_OwnerModule.SetType (CTBNamespace::MODULE);
		m_pConfigInfo->m_OwnerModule.SetApplicationName (sAppName);
		m_pConfigInfo->m_OwnerModule.SetObjectName (CTBNamespace::MODULE, sModName);
	}

	return szXmlApplicationInfo;
}

//------------------------------------------------------------------------------
void CLocalizableApplicationConfigContent::OnBindParseFunctions ()
{
	BIND_PARSE_ATTRIBUTES (szXmlApplicationInfoTag,	&CLocalizableApplicationConfigContent::ParseTitle);
}

//------------------------------------------------------------------------------
int CLocalizableApplicationConfigContent::ParseTitle (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	if (!m_pConfigInfo)
		return CXMLSaxContent::ABORT;

	if (!arAttributes.GetSize ())
		return CXMLSaxContent::OK;
	
	CString sNotLocalizedTile = arAttributes.GetAttributeByName (szXmlLocalize);

	if (sNotLocalizedTile.IsEmpty ())
		sNotLocalizedTile = m_pConfigInfo->GetOwnerModule ().GetApplicationName();

	m_pConfigInfo->SetNotLocalizedTitle	(sNotLocalizedTile);

	return CXMLSaxContent::OK;
}

//==============================================================================
//						Class CLocalizableApplicationConfigInfo
//==============================================================================
//
//------------------------------------------------------------------------------
CLocalizableApplicationConfigInfo::CLocalizableApplicationConfigInfo ()
{
}

const CString CLocalizableApplicationConfigInfo::GetTitle () const
{
	return AfxLoadXMLString
			(
				m_sNotLocalizedTitle, 
				AfxGetPathFinder()->GetLocalAppConfigName(),
				AfxGetDictionaryPathFromNamespace(m_OwnerModule, TRUE)
			);
}