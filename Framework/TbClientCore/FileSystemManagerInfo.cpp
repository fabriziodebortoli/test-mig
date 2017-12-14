#include "stdafx.h"

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\PathFinder.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>

#include "FileSystemManagerInfo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"


static const TCHAR szXmlFileName[]			= _T("FileSystemManager.config");

static const TCHAR szXmlRoot[]				= _T("FileSystemManager");
static const TCHAR szXmlDriverKey[]			= _T("/FileSystemManager/Driver");
static const TCHAR szXmlCachingKey[]			= _T("/FileSystemManager/Caching");
static const TCHAR szXmlPerformanceCheckKey[]	= _T("/FileSystemManager/PerformanceCheck");
static const TCHAR szXmlWebServiceDriverKey[]	= _T("/FileSystemManager/WebServiceDriver");
static const TCHAR szXmlDriverTag[]			= _T("Driver");
static const TCHAR szXmlCachingTag[]			= _T("Caching");
static const TCHAR szXmlPerformanceCheckTag[]	= _T("PerformanceCheck");
static const TCHAR szXmlWebServiceDriverTag[]	= _T("WebServiceDriver");
static const TCHAR szXmlEnabled[]				= _T("enabled");
static const TCHAR szXmlValue[]				= _T("value");
static const TCHAR szXmlAutodetect[]			= _T("autodetect");
static const TCHAR szXmlPort[]				= _T("port");
static const TCHAR szXmlService[]				= _T("service");
static const TCHAR szXmlNamespace[]			= _T("namespace");

static const TCHAR szXmlTrueValue[]			= _T("True");
static const TCHAR szXmlFalseValue[]			= _T("False");
static const TCHAR szXmlIntZeroValue[]		= _T("0");
static const TCHAR szXmlIntOneValue[]			= _T("1");
static const TCHAR szXmlIntTwoValue[]			= _T("2");

static const TCHAR szWebServiceDriverService[]	= _T("/FileSystemManager/FileSystemManager.asmx");
static const TCHAR szWebServiceDriverNamespace[]	= _T("http://microarea.it/FileSystemManager/");

//=============================================================================
//							CFileSystemManagerContent
//=============================================================================
IMPLEMENT_DYNAMIC(CFileSystemManagerContent, CXMLSaxContent)

//------------------------------------------------------------------------------
CFileSystemManagerContent::CFileSystemManagerContent (CFileSystemManagerInfo* pConfigInfo)
	:
	m_pConfigInfo (pConfigInfo)
{
}

//------------------------------------------------------------------------------
void CFileSystemManagerContent::OnBindParseFunctions ()
{
	BIND_PARSE_ATTRIBUTES	(szXmlDriverKey,			&CFileSystemManagerContent::ParseDriver);
	BIND_PARSE_ATTRIBUTES	(szXmlCachingKey,			&CFileSystemManagerContent::ParseCaching);
	BIND_PARSE_ATTRIBUTES	(szXmlPerformanceCheckKey,	&CFileSystemManagerContent::ParsePerformanceCheck);
	BIND_PARSE_ATTRIBUTES	(szXmlWebServiceDriverKey ,	&CFileSystemManagerContent::ParseWebServiceDriver);
}

//------------------------------------------------------------------------------
CString	CFileSystemManagerContent::OnGetRootTag	() const
{
	return szXmlRoot;
}

//------------------------------------------------------------------------------
int CFileSystemManagerContent::ParseDriver (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	if (!m_pConfigInfo)
		return CXMLSaxContent::ABORT;

	if (!arAttributes.GetSize ())
		return CXMLSaxContent::OK;
	
	CString sTmp = arAttributes.GetAttributeByName (szXmlValue);
		
	sTmp.Trim ();
	sTmp = sTmp.MakeLower();
	if (sTmp.CompareNoCase(szXmlIntOneValue) == 0)
		m_pConfigInfo->m_Driver = CFileSystemManagerInfo::WebService;
	else if (sTmp.CompareNoCase(szXmlIntTwoValue) == 0)
		m_pConfigInfo->m_Driver = CFileSystemManagerInfo::Database;

	sTmp = arAttributes.GetAttributeByName (szXmlAutodetect);
	m_pConfigInfo->m_bAutoDetectDriver =	sTmp.CompareNoCase(szXmlIntOneValue) == 0 || 
											sTmp.CompareNoCase(szXmlTrueValue) == 0;
		
	return CXMLSaxContent::OK;
}

//------------------------------------------------------------------------------
int CFileSystemManagerContent::ParseCaching	(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	if (!m_pConfigInfo)
		return CXMLSaxContent::ABORT;

	if (!arAttributes.GetSize ())
		return CXMLSaxContent::OK;
	
	CString sTmp = arAttributes.GetAttributeByName (szXmlEnabled);
		
	m_pConfigInfo->m_bEnableCaching =	sTmp.CompareNoCase(szXmlIntOneValue) == 0 || 
										sTmp.CompareNoCase(szXmlTrueValue) == 0;

	return CXMLSaxContent::OK;
}

//------------------------------------------------------------------------------
int CFileSystemManagerContent::ParsePerformanceCheck (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	if (!m_pConfigInfo)
		return CXMLSaxContent::ABORT;

	if (!arAttributes.GetSize ())
		return CXMLSaxContent::OK;
	
	CString sTmp = arAttributes.GetAttributeByName (szXmlEnabled);
		
	m_pConfigInfo->m_bEnablePerformanceCheck =	sTmp.CompareNoCase(szXmlIntOneValue) == 0 || 
												sTmp.CompareNoCase(szXmlTrueValue) == 0;

	return CXMLSaxContent::OK;
}

//------------------------------------------------------------------------------
int CFileSystemManagerContent::ParseWebServiceDriver (const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
{
	if (!m_pConfigInfo)
		return CXMLSaxContent::ABORT;

	if (!arAttributes.GetSize ())
		return CXMLSaxContent::OK;
	
	CString sTmp = arAttributes.GetAttributeByName (szXmlPort);
		
	if (!sTmp.IsEmpty ())
		m_pConfigInfo->m_nWebServiceDriverPort = _tstoi(sTmp);

	sTmp = arAttributes.GetAttributeByName (szXmlService);
	if (!sTmp.IsEmpty ())
		m_pConfigInfo->m_sWebServiceDriverService = sTmp.Trim();

	sTmp = arAttributes.GetAttributeByName (szXmlNamespace);
	if (!sTmp.IsEmpty ())
		m_pConfigInfo->m_sWebServiceDriverNamespace = sTmp.Trim();

	return CXMLSaxContent::OK;
}


//=============================================================================
//							CFileSystemManagerInfo
//=============================================================================

//----------------------------------------------------------------------------
CFileSystemManagerInfo::CFileSystemManagerInfo()
	:
	m_Driver					(FileSystem),
	m_bAutoDetectDriver			(TRUE),
	m_bEnableCaching			(TRUE),
	m_bEnablePerformanceCheck	(FALSE),
	m_nWebServiceDriverPort		(80),
	m_sWebServiceDriverService	(szWebServiceDriverService),
	m_sWebServiceDriverNamespace(szWebServiceDriverNamespace)
{
}

//----------------------------------------------------------------------------
CFileSystemManagerInfo::DriverType CFileSystemManagerInfo::GetDriver () const
{
	return m_Driver;
}


//----------------------------------------------------------------------------
BOOL CFileSystemManagerInfo::IsCachingEnabled () const
{
	return m_bEnableCaching;
}

//----------------------------------------------------------------------------
BOOL CFileSystemManagerInfo::IsPerformanceCheckEnabled () const
{
	return m_bEnablePerformanceCheck;
}

//----------------------------------------------------------------------------
const BOOL& CFileSystemManagerInfo::GetWebServiceDriverPort () const
{
	return m_nWebServiceDriverPort;
}

//----------------------------------------------------------------------------
const CString& CFileSystemManagerInfo::GetWebServiceDriverService () const
{
	return m_sWebServiceDriverService;
}

//----------------------------------------------------------------------------
const CString& CFileSystemManagerInfo::GetWebServiceDriverNamespace () const
{
	return m_sWebServiceDriverNamespace;
}

//----------------------------------------------------------------------------
BOOL CFileSystemManagerInfo::IsAutoDetectDriver () const
{
	return m_bAutoDetectDriver;
}

//----------------------------------------------------------------------------
void CFileSystemManagerInfo::SetDriver (DriverType aDriverType)
{
	m_Driver = aDriverType;
}

//----------------------------------------------------------------------------
CString CFileSystemManagerInfo::GetFileName () const
{
	return AfxGetPathFinder()->GetTBDllPath() +	SLASH_CHAR + CString(szXmlFileName);
}

//----------------------------------------------------------------------------
const BOOL	CFileSystemManagerInfo::LoadFile ()
{
	if (!ExistFile(GetFileName()))
		return TRUE;

	// FileSystemManager parsing with Sax
	CFileSystemManagerContent aFileContent (this);
	CXMLSaxReader aReader;

	aReader.AttachContent (&aFileContent);
	return aReader.ReadFile (GetFileName());
}

// FileSystemManager unparsing with Dom
//----------------------------------------------------------------------------
const BOOL CFileSystemManagerInfo::SaveFile ()
{
	CXMLDocumentObject aDoc;

	CXMLNode* pRoot = aDoc.CreateRoot(szXmlRoot);
	if (!pRoot)
	{
		ASSERT(FALSE);
		TRACE ("Cannot create root tag of the FileSystemManager.config file");
		return FALSE;
	}

	// Driver
	CXMLNode* pNewNode = pRoot->CreateNewChild (szXmlDriverTag);
	
	CString sTemp;
	switch (m_Driver)
	{
		case FileSystem: pNewNode->SetAttribute	(szXmlValue, (LPCTSTR) szXmlIntZeroValue);	break;
		case WebService: pNewNode->SetAttribute	(szXmlValue, (LPCTSTR) szXmlIntOneValue);	break;
		case Database:	 pNewNode->SetAttribute	(szXmlValue, (LPCTSTR) szXmlIntTwoValue);	break;
	}
	
	pNewNode->SetAttribute	(szXmlAutodetect, (LPCTSTR) m_bAutoDetectDriver ? szXmlTrueValue : szXmlFalseValue);

	// Caching
	pNewNode = pRoot->CreateNewChild (szXmlCachingTag);
	pNewNode->SetAttribute	(szXmlEnabled, (LPCTSTR) m_bEnableCaching ? szXmlTrueValue : szXmlFalseValue);

	// Performance Check
	pNewNode = pRoot->CreateNewChild (szXmlPerformanceCheckTag);
	pNewNode->SetAttribute	(szXmlEnabled, (LPCTSTR) m_bEnablePerformanceCheck ? szXmlTrueValue : szXmlFalseValue);

	const rsize_t nLen = 5;
	TCHAR szBuffer [nLen];
	_itot_s(m_nWebServiceDriverPort, szBuffer, nLen, 10);

	// Web Service Driver
	pNewNode = pRoot->CreateNewChild (szXmlWebServiceDriverTag);
	pNewNode->SetAttribute	(szXmlPort,		(LPCTSTR) CString (szBuffer));
	pNewNode->SetAttribute	(szXmlService,	(LPCTSTR) m_sWebServiceDriverService);
	pNewNode->SetAttribute	(szXmlNamespace,(LPCTSTR) m_sWebServiceDriverNamespace);

	return aDoc.SaveXMLFile (GetFileName());
}
