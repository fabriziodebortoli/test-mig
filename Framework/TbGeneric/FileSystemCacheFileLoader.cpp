#include "stdafx.h"

#include <io.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TBXmlCore\XmlDocObj.h>
#include "globals.h"

#include "FileSystemCacheFileLoader.h"
#include "dialogs.hjson" //JSON AUTOMATIC UPDATE

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

// xml grammar
static const TCHAR	szContainerKey[]			= _T("/MicroareaServer/Standard/Container");
static const TCHAR	szXmlMicroareaServerTag[]	= _T("MicroareaServer");
static const TCHAR	szXmlStandardTag[]			= _T("Standard");
static const TCHAR	szXmlContainerTag[]			= _T("Container");
static const TCHAR	szXmlPathTag[]				= _T("Path");
static const TCHAR	szXmlFileTag[]				= _T("File");
static const TCHAR	szNameAttribute[]			= _T("name");

// utility
static const CString	szManagedExtensions			= _T("*.xml;*.config;*.txt;*.ini;*.wrm;*.tbf;*.rad;*.jpg;*.bmp;*.gif;*.bin;*.xsl;*.xsd;");
static CString			szExcludedPaths					= _T(";clientnet;design tools .net;developerrefguide;tools;tools.net;library;microareaconsole;setup;webframework;obj;res;dbinfo;migration_xp;migration_net;databasescript;datamanager\\default;datamanager\\sample;");
static const TCHAR		szDisabledSuffix[]			= _T(".Disabled");
static const TCHAR		szStringSeparator[]			= _T(";");

///////////////////////////////////////////////////////////////////////////////
//				class CFileSystemCacheContent implementation
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CFileSystemCacheContent, CXMLSaxContent)

//----------------------------------------------------------------------------------------------
CFileSystemCacheContent::CFileSystemCacheContent(CFileSystemCacher*	pCacher)
	:
	m_pCacher (pCacher)
{
	ASSERT (m_pCacher);
}

//----------------------------------------------------------------------------------------------
CFileSystemCacheContent::~CFileSystemCacheContent()
{
}

//----------------------------------------------------------------------------------------------
CString CFileSystemCacheContent::OnGetRootTag	() const
{
	return szXmlMicroareaServerTag;
}

//----------------------------------------------------------------------------------------------
int	 CFileSystemCacheContent::OnStartDocument ()
{
	if (!m_pCacher)
		return CXMLSaxContent::ABORT;

	m_pCacher->ClearCaches();

	return CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------------------------
int CFileSystemCacheContent::OnStartElement (
													const CString& sKey, 
													const CString& sUri, 
													const CXMLSaxContentAttributes& arAttributes
											)
{
	m_bFromSkipChilds = FALSE;

	m_sNameAttribute = arAttributes.GetAttributeByName (szNameAttribute);
	
	if (m_sNameAttribute.IsEmpty ())
		return CXMLSaxContent::OK;

	if (sKey.CompareNoCase (szContainerKey) == 0)
	{
		CPathFinder::ApplicationType aType = AfxGetPathFinder()->GetContainerApplicationType(m_sNameAttribute);
		if (aType == CPathFinder::UNDEFINED)
		{
			m_bFromSkipChilds = TRUE;
			return SKIP_THE_CHILDS;
		}

		m_sContainer		= m_sNameAttribute;
		m_sCurrentPath		= AfxGetPathFinder()->GetStandardPath() + SLASH_CHAR + m_sContainer;
		m_pCacher->AddCache (m_sCurrentPath);

		return CXMLSaxContent::OK;
	}
	
	if (sKey.Find (szXmlFileTag) >= 0)
	{
		if (m_pCacher->IsAManagedObject (m_sCurrentPath + SLASH_CHAR + m_sNameAttribute))
			m_pCacher->AddInCache (m_sCurrentPath, m_sNameAttribute);
		
		return CXMLSaxContent::OK;
	}

	if (m_sContainer.IsEmpty())
		return  CXMLSaxContent::OK;

	if	(CFileSystemCacheFileLoader::IsExcludedPath(m_sCurrentPath, m_sNameAttribute))
	{
		m_bFromSkipChilds = TRUE;
		return SKIP_THE_CHILDS;
	}

	m_sCurrentPath = m_sCurrentPath + SLASH_CHAR + m_sNameAttribute;
	m_pCacher->AddInCache (m_sCurrentPath);	

	return CXMLSaxContent::OK;
}

//----------------------------------------------------------------------------------------------
int	 CFileSystemCacheContent::OnEndElement	(
												const CString& sKey, 
												const CString& sUri, 
												const CString& sTagValue
											)
{
	if (m_bFromSkipChilds)
	{
		m_bFromSkipChilds = FALSE;
		return CXMLSaxContent::OK;
	}

	if (sKey == szContainerKey)
	{
		m_sContainer.Empty ();
		m_sCurrentPath.Empty ();
		return CXMLSaxContent::OK;
	}

	if (sKey.Find (szXmlFileTag) >= 0)
		return CXMLSaxContent::OK;

	int nSlashPos = m_sCurrentPath.ReverseFind (SLASH_CHAR);
	if (nSlashPos >= 0)
		m_sCurrentPath = m_sCurrentPath.Left(nSlashPos);

	return CXMLSaxContent::OK;
}

///////////////////////////////////////////////////////////////////////////////
//						class CFileSystemCacheFileDialog
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC (CFileSystemCacheFileDialog, CDialog)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CFileSystemCacheFileDialog, CDialog)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CFileSystemCacheFileDialog::CFileSystemCacheFileDialog ()
	:
	CDialog(IDD_FILE_SYSTEM_CACHE, NULL)
{
}

//-----------------------------------------------------------------------------
void CFileSystemCacheFileDialog::PostNcDestroy() 
{
	delete this;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemCacheFileDialog::OnInitDialog()
{
	__super::OnInitDialog();

	CenterWindow();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CFileSystemCacheFileDialog::SetOperation (const CString& aOperation)
{
	CWnd* pWnd = GetDlgItem (IDC_FILE_SYSTEM_CACHE_OPERATION);
	if (pWnd)
	{
		pWnd->SetWindowText (aOperation);
		pWnd->UpdateWindow	();
		UpdateWindow ();
	}
}

///////////////////////////////////////////////////////////////////////////////
//				class CFileSystemCacheFileLoader implementation
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------------------------
CFileSystemCacheFileLoader::CFileSystemCacheFileLoader ()
	:
	m_bEnabled		(TRUE),
	m_pWorkingDialog(NULL)
{
	CPathFinder* pPathFinder = AfxGetPathFinder();

	m_sLocalFileName	= pPathFinder->GetClientFileSystemCacheName();
	m_sServerFileName	= pPathFinder->GetServerFileSystemCacheName();
	m_bEnabled			= !ExistFile (pPathFinder->GetServerFileSystemCacheName() + szDisabledSuffix); 
}

//----------------------------------------------------------------------------------------------
const BOOL CFileSystemCacheFileLoader::Load (CFileSystemCacher* pCacher)
{
	if (!pCacher)
		return FALSE;

	if (!m_bEnabled)
	{
		RemoveFiles (TRUE, TRUE);
		return TRUE;
	}

	// message dialog
	m_pWorkingDialog = new CFileSystemCacheFileDialog ();
	if (m_pWorkingDialog->Create(IDD_FILE_SYSTEM_CACHE))
		m_pWorkingDialog->ShowWindow (SW_SHOW);
	else
	{
		m_pWorkingDialog->DestroyWindow();
		delete m_pWorkingDialog;
		m_pWorkingDialog = NULL;
	}

	BOOL bOk = FALSE;

	// file auto-generation
	if (ExistFile (m_sServerFileName))
		bOk = IsFileUpdated () || SyncLocalFile ();
	else 
		bOk = GenerateFile () && SyncServerFile();
	
	// cache file not available
	if (!bOk)
	{
		if (m_pWorkingDialog)
			delete m_pWorkingDialog;
		return FALSE;
	}

	CXMLSaxReader aReader;
	CFileSystemCacheContent aContent (pCacher);

	aReader.AttachContent	(&aContent);
	bOk = aReader.ReadFile	(m_sLocalFileName);

	if (m_pWorkingDialog)
	{
		m_pWorkingDialog->DestroyWindow();
		m_pWorkingDialog = NULL;
	}
	return bOk;
}

//-----------------------------------------------------------------------------
const BOOL CFileSystemCacheFileLoader::Enable (const BOOL& bEnable)
{
	if (bEnable && m_bEnabled)
		return ExistFile (m_sServerFileName) || (GenerateFile () && SyncServerFile());
			
	return !bEnable && RemoveFiles (TRUE, TRUE);
}

//-----------------------------------------------------------------------------
const BOOL CFileSystemCacheFileLoader::IsFileUpdated () const
{
	CFileStatus aLocalStatus;
	GetStatus (m_sLocalFileName, aLocalStatus);
	
	CFileStatus aServerStatus;
	GetStatus (m_sServerFileName, aServerStatus);

	return	aServerStatus.m_mtime == aLocalStatus.m_mtime &&
			aServerStatus.m_size  == aLocalStatus.m_size;
}

//-----------------------------------------------------------------------------
const BOOL CFileSystemCacheFileLoader::SyncLocalFile ()
{
	m_pWorkingDialog->SetOperation (_T("Synchronizing cache file on the client workstation..."));

	BOOL bExistOnServer = ExistFile (m_sServerFileName);

	BOOL bOk = !bExistOnServer && DeleteFile(m_sLocalFileName) ||
				bExistOnServer && CopyFile	(m_sServerFileName, m_sLocalFileName, FALSE);

	return bOk;
}

//-----------------------------------------------------------------------------
const BOOL CFileSystemCacheFileLoader::SyncServerFile ()
{
	if (m_pWorkingDialog)
		m_pWorkingDialog->SetOperation (_T("Synchronizing cache file on the server workstation..."));
	
	BOOL bExistOnClient = ExistFile (m_sLocalFileName);

	BOOL bOk = !bExistOnClient && DeleteFile(m_sServerFileName) ||
				bExistOnClient && CopyFile	(m_sLocalFileName, m_sServerFileName, TRUE);

	return bOk;
}

//-----------------------------------------------------------------------------
const BOOL CFileSystemCacheFileLoader::GenerateFile ()
{
	CXMLDocumentObject aDoc;

	CXMLNode* pRoot = aDoc.CreateRoot(szXmlMicroareaServerTag);
	if (!pRoot)
	{
		ASSERT_TRACE1(FALSE,"Cannot create root tag of the Standard File System Cache file: %s",szXmlMicroareaServerTag);
		return FALSE;
	}

	// Caching
	CXMLNode* pNewNode = pRoot->CreateNewChild (szXmlStandardTag);
	
	if (GenerateFile (pNewNode,  AfxGetPathFinder()->GetStandardPath()))
		return aDoc.SaveXMLFile (m_sLocalFileName);
	
	return FALSE;
}

//-----------------------------------------------------------------------------
const BOOL CFileSystemCacheFileLoader::GenerateFile	(CXMLNode* pNode, const CString& sPath)
{
	if (sPath.IsEmpty() || !pNode)
		return TRUE;

	CString sRelativePath = sPath;
	sRelativePath.Replace (AfxGetPathFinder()->GetStandardPath (), _T(""));

	if (m_pWorkingDialog)
		m_pWorkingDialog->SetOperation (_T("Generating File System Cache file for Standard folder, please wait... ") + sRelativePath);

	CStringArray arFolders;
	CStringArray arFiles;
	AfxGetFileSystemManager ()->GetPathContent(sPath, TRUE, &arFolders, TRUE, _T("*.*"), &arFiles);

	CString sFolderName;
	CString sFolderFullPath;
	CString sCurrNodeName;

	pNode->GetName (sCurrNodeName);
	
	// subfolders
	for (int i=0; i <= arFolders.GetUpperBound(); i++) 
	{
		sFolderName = arFolders.GetAt (i);
		sFolderFullPath = sPath + SLASH_CHAR + sFolderName;
		sFolderName	= sFolderName.MakeLower ();

		if (IsExcludedPath (sRelativePath, sFolderName))
			continue;

		CXMLNode* pNewNode = NULL;
		if (sCurrNodeName.CompareNoCase (szXmlStandardTag) == 0)
			pNewNode = pNode->CreateNewChild (szXmlContainerTag);
		else
			pNewNode = pNode->CreateNewChild (szXmlPathTag);

		// when I write attributes, I use original capitalization 
		pNewNode->SetAttribute (szNameAttribute, arFolders.GetAt (i));
		GenerateFile (pNewNode, sFolderFullPath);
	}

	// files
	CString sFileName;
	CString sFileExt;
	for (int i=0; i <= arFiles.GetUpperBound(); i++) 
	{
		sFileName	= arFiles.GetAt (i);
		sFileName.Replace (sPath + SLASH_CHAR, _T(""));

		sFileExt	= GetExtension (sFileName) + szStringSeparator;
		sFileExt	= sFileExt.MakeLower ();
		
		if (szManagedExtensions.Find (sFileExt) < 0)
			continue;

		CXMLNode* pNewNode = pNode->CreateNewChild (szXmlFileTag);
		pNewNode->SetAttribute (szNameAttribute, sFileName);
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
const BOOL CFileSystemCacheFileLoader::RemoveFiles (BOOL bLocalFile, BOOL bServerFile)
{
	BOOL bOk = TRUE;
	
	if (bLocalFile && ExistFile (m_sLocalFileName))
		bOk = DeleteFile (m_sLocalFileName);

	if (bServerFile && ExistFile (m_sLocalFileName))
		bOk = DeleteFile (m_sServerFileName);

	return bOk;
}

//-----------------------------------------------------------------------------
const BOOL CFileSystemCacheFileLoader::IsExcludedPath (const CString& sRelativePath, const CString& sFolderName)
{
	// folder name immediatly found without folder tree: it is excluded
	if (szExcludedPaths.Find (szStringSeparator + sFolderName + szStringSeparator) >= 0)
		return TRUE;

	// folder name is not found I have to check if it is excluded as folder tree
	CString sPathToFind =	szStringSeparator 
							+ GetName(sRelativePath).MakeLower() + SLASH_CHAR + sFolderName 
							+ szStringSeparator;
	
	return szExcludedPaths.Find (sPathToFind) > 0;
}
