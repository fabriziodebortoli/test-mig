#include "stdafx.h"
#include <stdio.h>
#include <dos.h>
#include <sys\stat.h>
#include <errno.h>

#include <TbNameSolver\Diagnostic.h>
#include <TbDatabaseManaged\TBFSDatabaseDriver.h>

#include <TbWebServicesWrappers\FileSystemManagerWebService.h>
#include <TbClientCore\ClientObjects.h>

#include "FileSystemManager.h"


#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
//				class CFileSystemManager implementation
///////////////////////////////////////////////////////////////////////////////
// No lock is required as m_ConfigFile is loaded and checked only in InitInstance
//-----------------------------------------------------------------------------
CFileSystemManager::CFileSystemManager(BOOL bCacheEnabled /*TRUE*/)
	:
	IFileSystemManager (bCacheEnabled)
{
}

//-----------------------------------------------------------------------------
CFileSystemManager::CFileSystemManager	(IFileSystemDriver* pFileSystem, IFileSystemDriver* pAlternative /*NULL*/, BOOL bCacheEnabled /*TRUE*/)
	:
	IFileSystemManager (pFileSystem, pAlternative, bCacheEnabled)
{
}

//-----------------------------------------------------------------------------
CFileSystemManager::~CFileSystemManager()
{
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::Init(const CString& strFileServer, const CString& strInstallation, const CString& strMasterSolutionName)
{
	// FileSystemManager parsing
	m_ConfigFile.LoadFile ();	
	CString strServerName = strFileServer;
	CString strInstance = strInstallation;
	CString strMaster = strMasterSolutionName;
	if (m_ConfigFile.GetDriver() == CFileSystemManagerInfo::FileSystem)
	{
		if (!m_ConfigFile.GetFSServerName().IsEmpty())
			strServerName = m_ConfigFile.GetFSServerName();
		if (!m_ConfigFile.GetFSInstanceName().IsEmpty())
			strInstance = m_ConfigFile.GetFSInstanceName();
	}
	CPathFinder* pPathFinder = AfxGetPathFinder();
	pPathFinder->Init
	(
		strServerName,
		strInstance,
		strMasterSolutionName
	);
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::DetectAndAttachAlternativeDriver()
{
	
	BOOL bWSAvailable = FALSE;
	CString strSysDBConnectionString = m_ConfigFile.GetStandardConnectionString();
	//if (strSysDBConnectionString)
	//	strSysDBConnectionString = AfxGetProvisiongDBConnectionString();

	BOOL bDBAvailable = !strSysDBConnectionString.IsEmpty(); //! AfxGetLoginManager()->GetSystemDBConnectionString().IsEmpty(); //@@TODO BAUZI fare una condizione più furba																					
	BOOL bExistFileSystem = ExistPath(AfxGetPathFinder()->GetStandardPath()) && ExistPath(AfxGetPathFinder()->GetCustomPath());

	// I cannot work without file system and web service
	if (!bExistFileSystem && !bWSAvailable && !bDBAvailable)
	{
		ASSERT(FALSE);
		AfxGetDiagnostic()->Add(_T("The \\Standard and \\Custom folders are not reacheable on server structure!"));
		AfxGetDiagnostic()->Add(_T("Please, check network sharings if you are on a client/server installation."));
		return FALSE;
	}

	if (m_ConfigFile.IsAutoDetectDriver())
	{
		if (m_ConfigFile.GetDriver() == CFileSystemManagerInfo::FileSystem && !bExistFileSystem)
			m_ConfigFile.SetDriver(CFileSystemManagerInfo::Database);
		else
		{
			if (m_ConfigFile.GetDriver() == CFileSystemManagerInfo::Database && !bDBAvailable)
				m_ConfigFile.SetDriver(CFileSystemManagerInfo::WebService);
			else
				if (m_ConfigFile.GetDriver() == CFileSystemManagerInfo::WebService && !bWSAvailable)
					m_ConfigFile.SetDriver(CFileSystemManagerInfo::FileSystem);
		}

	}
		
	if (m_ConfigFile.GetDriver() == CFileSystemManagerInfo::Database)
	{
		int nPos = strSysDBConnectionString.Find(_T("Data Source"));
		if (nPos > 0)
			strSysDBConnectionString = strSysDBConnectionString.Right(strSysDBConnectionString.GetLength() - nPos);
			
		AttachAlternativeDriver(new TBFSDatabaseDriver(strSysDBConnectionString), FALSE, TRUE);
	}
	else
		if (m_ConfigFile.GetDriver() == CFileSystemManagerInfo::WebService)
			AttachAlternativeDriver(AfxGetFileSystemManagerWebService(), FALSE, TRUE);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CFileSystemManager::GetServerConnectionConfig ()
{
	// no lock is required as methods invoked work only on local variables
	// an they don't locks other objects

	CString sContent; 
	if (IsManagedByAlternativeDriver (AfxGetPathFinder()->GetServerConnectionConfigFullName()))
		sContent = GetAlternativeDriver()->GetServerConnectionConfig ();
	else
		sContent = GetFileSystemDriver()->GetServerConnectionConfig ();

	return sContent;
}

//-----------------------------------------------------------------------------
void CFileSystemManager::GetAllApplicationInfo(CStringArray* pReturnArray)
{
	if (GetAlternativeDriver())
		GetAlternativeDriver()->GetAllApplicationInfo(pReturnArray);
	else
		GetFileSystemDriver()->GetAllApplicationInfo(pReturnArray);
}

//-----------------------------------------------------------------------------
void CFileSystemManager::GetAllModuleInfo(const CString& strAppName, CStringArray* pReturnArray)
{
	if (GetAlternativeDriver())
		GetAlternativeDriver()->GetAllModuleInfo(strAppName, pReturnArray);
	else
		GetFileSystemDriver()->GetAllModuleInfo(strAppName, pReturnArray);
}

//-----------------------------------------------------------------------------
CString CFileSystemManager::GetTextFile (const CString& sFileName)
{
	// no lock is required as methods invoked work only on local variables
	// an they don't locks other objects

	CString sContent; 
	if (IsManagedByAlternativeDriver (sFileName))
		sContent = GetAlternativeDriver()->GetTextFile (sFileName);
	else
		sContent = GetFileSystemDriver()->GetTextFile (sFileName);

	return sContent;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::SaveTextFile (const CString& sFileName, const CString& sFileContent)
{
	// no lock is required as methods invoked work only on local variables
	// an they don't locks other objects

	BOOL bOk = FALSE;

	if (IsManagedByAlternativeDriver (sFileName))
		bOk = GetAlternativeDriver()->SaveTextFile(sFileName, sFileContent);
	else
		bOk = GetFileSystemDriver()->SaveTextFile(sFileName, sFileContent);

	return bOk;
}

// no cached content
//-----------------------------------------------------------------------------
BYTE* CFileSystemManager::GetBinaryFile	(const CString& sFileName, int& nLen)
{
	BYTE* pBinaryContent;
 
	if (IsManagedByAlternativeDriver (sFileName))
		pBinaryContent = GetAlternativeDriver()->GetBinaryFile (sFileName, nLen);
	else
		pBinaryContent = GetFileSystemDriver()->GetBinaryFile (sFileName, nLen);

	return pBinaryContent;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::SaveBinaryFile(const CString& sFileName, BYTE* pBinaryContent, int nLen)
{
	return (IsManagedByAlternativeDriver(sFileName)) 
			? GetAlternativeDriver()->SaveBinaryFile(sFileName, pBinaryContent, nLen) 
			: GetFileSystemDriver()->SaveBinaryFile(sFileName, pBinaryContent, nLen);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::ExistFile (const CString& sFileName)
{
	if (sFileName.IsEmpty()) 
		return FALSE;

	BOOL bOk = FALSE;


	//// caching (no lock is required as demanded to the single methods if needed)
	//if (m_Cacher.IsAManagedObject (sFileName))
	//{
	//	bOk = m_Cacher.ExistFile (sFileName);
	//	return bOk;
	//}

	if (IsManagedByAlternativeDriver (sFileName))
		bOk = GetAlternativeDriver()->ExistFile (sFileName);
	else
		bOk = GetFileSystemDriver()->ExistFile (sFileName);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::ExistPath (const CString& sPathName)
{
	if (sPathName.IsEmpty()) 
		return FALSE;

	BOOL bOk = FALSE;

	// caching (no lock is required as demanded to the single methods if needed)
	/*if (m_Cacher.IsAManagedObject (sPathName))
	{
		bOk =  m_Cacher.ExistPath (sPathName);
		return bOk;
	}*/
    	
	if (IsManagedByAlternativeDriver (sPathName))
		bOk = GetAlternativeDriver()->ExistPath (sPathName);
	else
		bOk = GetFileSystemDriver()->ExistPath (sPathName);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::CreateFolder(const CString& sPathName, const BOOL& bRecursive)
{
	if (sPathName.IsEmpty ())
		return FALSE;

	BOOL bOk = FALSE;

	if (IsManagedByAlternativeDriver (sPathName))
		bOk = GetAlternativeDriver()->CreateFolder (sPathName, bRecursive);
	else
		bOk = GetFileSystemDriver()->CreateFolder (sPathName, bRecursive);

	// caching (no lock is required as demanded to the single methods if needed)
	/*if (bOk && m_Cacher.IsAManagedObject (sPathName))
		bOk = m_Cacher.CreateFolder (sPathName, bRecursive);*/

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::RemoveFolder(const CString& sPathName, const BOOL& bRecursive, const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents)
{
	BOOL bOk = FALSE;

	if (IsManagedByAlternativeDriver (sPathName))
		bOk = GetAlternativeDriver()->RemoveFolder (sPathName, bRecursive, bRemoveRoot, bAndEmptyParents);
	else
		bOk = GetFileSystemDriver()->RemoveFolder (sPathName, bRecursive, bRemoveRoot, bAndEmptyParents);

	// caching (no lock is required as demanded to the single methods if needed)
	/*if (bOk && m_Cacher.IsAManagedObject (sPathName))
		bOk =  m_Cacher.RemoveFolder (sPathName, bRecursive, bRemoveRoot, bAndEmptyParents);*/

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::RemoveFile (const CString& sFileName)
{
	BOOL bOk = FALSE;

	if (IsManagedByAlternativeDriver (sFileName))
		bOk = GetAlternativeDriver()->RemoveFile (sFileName);
	else
		bOk = GetFileSystemDriver()->RemoveFile (sFileName);

	// caching (no lock is required as demanded to the single methods if needed)
	/*if (bOk && m_Cacher.IsAManagedObject (sFileName))
		bOk = m_Cacher.RemoveFile (sFileName);*/

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::RenameFile (const CString& sOldFileName, const CString& sNewFileName)
{
	BOOL bOk = FALSE;

	BOOL bOldOnAlternative = IsManagedByAlternativeDriver (sOldFileName);
	BOOL bNewOnAlternative = IsManagedByAlternativeDriver (sNewFileName);

	if (bOldOnAlternative && bNewOnAlternative)
		bOk = GetAlternativeDriver()->RenameFile (sOldFileName, sNewFileName);
	else  if (bOldOnAlternative && !bNewOnAlternative)
	{
		CString sContent = GetAlternativeDriver()->GetTextFile (sOldFileName);
		bOk  = sContent.IsEmpty() || GetFileSystemDriver()->SaveTextFile (sNewFileName, sContent);
	}	
	else  if (!bOldOnAlternative && bNewOnAlternative)
	{
		CString sContent = GetFileSystemDriver()->GetTextFile (sOldFileName);
		bOk  = sContent.IsEmpty() || GetAlternativeDriver()->SaveTextFile(sNewFileName, sContent);
	}
	else
		bOk = GetFileSystemDriver()->RenameFile (sOldFileName, sNewFileName);

	// caching (no lock is required as demanded to the single methods if needed)
	/*if (bOk && (m_Cacher.IsAManagedObject (sOldFileName) || m_Cacher.IsAManagedObject (sNewFileName)))
		bOk = m_Cacher.RenameFile (sOldFileName, sNewFileName);*/

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::CopyFile (const CString& sOldFileName, const CString& sNewFileName, const BOOL& bOverWrite)
{
	BOOL bOk = FALSE;

	BOOL bOldOnAlternative = IsManagedByAlternativeDriver (sOldFileName);
	BOOL bNewOnAlternative = IsManagedByAlternativeDriver (sNewFileName);

	if (bOldOnAlternative && bNewOnAlternative)
		bOk = GetAlternativeDriver()->CopySingleFile(sOldFileName, sNewFileName, bOverWrite);
	else  if (bOldOnAlternative && !bNewOnAlternative)
	{
		CString sContent = GetAlternativeDriver()->GetTextFile (sOldFileName);
		bOk  = sContent.IsEmpty() || GetFileSystemDriver()->SaveTextFile(sNewFileName, sContent);
	}
	else  if (!bOldOnAlternative && bNewOnAlternative)
	{
		CString sContent = GetFileSystemDriver()->GetTextFile (sOldFileName);
		bOk  = sContent.IsEmpty() || GetAlternativeDriver()->SaveTextFile(sNewFileName, sContent);
	}
	else  
		bOk = GetFileSystemDriver()->CopySingleFile(sOldFileName, sNewFileName, bOverWrite);

	// caching (no lock is required as demanded to the single methods if needed)
	/*if (bOk && m_Cacher.IsAManagedObject (sNewFileName))
	{
		CString sPath = GetPath	(sNewFileName);
		CString sFile = GetNameWithExtension(sNewFileName);

		bOk = m_Cacher.AddInCache (sPath, sFile);
	}*/

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::GetFileStatus (const CString& sFileName, CFileStatus& fs)
{
	BOOL bOk = FALSE;

	if (IsManagedByAlternativeDriver (sFileName))
		bOk = GetAlternativeDriver()->GetFileStatus (sFileName, fs);
	else
		bOk = GetFileSystemDriver()->GetFileStatus (sFileName, fs);

	return bOk;
}

//-----------------------------------------------------------------------------
DWORD CFileSystemManager::GetFileAttributes (const CString& sFileName)
{
	DWORD attributes = 0;

	if (IsManagedByAlternativeDriver (sFileName))
		attributes = GetAlternativeDriver()->GetFileAttributes (sFileName);
	else
		attributes = GetFileSystemDriver()->GetFileAttributes (sFileName);

	return attributes;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::CopyFolder (const CString& sOldPathName, const CString& sNewPathName, const BOOL& bOverwrite, const BOOL& bRecursive)
{
	BOOL bOk = FALSE;

	BOOL bOldOnAlternative = IsManagedByAlternativeDriver (sOldPathName);
	BOOL bNewOnAlternative = IsManagedByAlternativeDriver (sNewPathName);

	if (bOldOnAlternative && bOldOnAlternative)
		bOk = GetAlternativeDriver()->CopyFolder (sOldPathName, sNewPathName, bOverwrite, bRecursive);
	else  if (bOldOnAlternative && !bNewOnAlternative)
		;//TODOBRUNA situazioni miste in ricorsione
	else  if (!bOldOnAlternative && bNewOnAlternative)
		;//TODOBRUNA situazioni miste in ricorsione
	else
		bOk = GetFileSystemDriver()->CopyFolder (sOldPathName, sNewPathName, bOverwrite, bRecursive);

	// caching (no lock is required as demanded to the single methods if needed)
	/*if (bOk && m_Cacher.IsAManagedObject (sNewPathName))
		bOk = m_Cacher.CreateFolder (sNewPathName, bRecursive);*/

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::GetSubFolders (const CString& sPathName, CStringArray* pSubFolders)
{
	BOOL bOk = FALSE;

	// caching (no lock is required as demanded to the single methods if needed)
	/*if (m_Cacher.IsAManagedObject (sPathName))
	{
		bOk =  m_Cacher.GetSubFolders (sPathName, pSubFolders);
		return bOk;
	}*/
    	
	if (IsManagedByAlternativeDriver (sPathName))
		bOk = GetAlternativeDriver()->GetSubFolders (sPathName, pSubFolders);
	else
		bOk = GetFileSystemDriver()->GetSubFolders (sPathName, pSubFolders);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::GetFiles (const CString& sPathName, const CString& sFileExt, CStringArray* pFiles)
{
	BOOL bOk = FALSE;

	// caching (no lock is required as demanded to the single methods if needed)
	/*if (m_Cacher.IsAManagedObject (sPathName))
	{
		bOk =  m_Cacher.GetFiles (sPathName, sFileExt, pFiles);
		return bOk;
	}*/
    	
	if (IsManagedByAlternativeDriver (sPathName))
		bOk = GetAlternativeDriver()->GetFiles (sPathName, sFileExt, pFiles);
	else
		bOk = GetFileSystemDriver()->GetFiles (sPathName, sFileExt, pFiles);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::GetPathContent	(const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders,  BOOL bFiles, const CString& sFileExt, CStringArray* pFiles)
{
	BOOL bOk = FALSE;

	if (IsManagedByAlternativeDriver (sPathName))
		bOk = GetAlternativeDriver()->GetPathContent (sPathName, bFolders, pSubFolders, bFiles, sFileExt, pFiles);
	else
		bOk = GetFileSystemDriver()->GetPathContent (sPathName, bFolders, pSubFolders, bFiles, sFileExt, pFiles);

	return bOk;
}

// gets a binary file and it stores it into the temp directory
//-----------------------------------------------------------------------------
const CString CFileSystemManager::GetTemporaryBinaryFile (const CString& sFileName)
{
	int nLen = 0;
	BYTE* pBinaryContent = GetBinaryFile (sFileName, nLen);
	if (pBinaryContent == NULL)
		return sFileName;

	CString sExtension		= GetExtension(sFileName);
	CString sTempFileName	= GetTempName() + _T(".") + sExtension;

	void* buff = (void*)pBinaryContent;

	CFile aFile;
	CFileException exc;

	TRY
	{
		if (aFile.Open (sTempFileName, CFile::typeBinary | CFile::modeCreate | CFile::modeWrite), &exc)
		{
			aFile.Write (buff, nLen);
			aFile.Flush	();
			aFile.Close ();
		}
	}
	CATCH (CException, e)
	{
		if (aFile.m_hFile != CFile::hFileNull)
			aFile.Close ();

		TCHAR szBuffer [255];
		e->GetErrorMessage (szBuffer, sizeof(szBuffer));
		AfxGetDiagnostic()->Add (szBuffer);
		
		return sFileName;
	}
	END_CATCH
	
	return sTempFileName;
}

//-----------------------------------------------------------------------------
CString CFileSystemManager::GetFormattedQueryTime()
{
	if (IsAlternativeDriverEnabled() && GetAlternativeDriver()->IsKindOf(RUNTIME_CLASS(TBFSDatabaseDriver)))
		return ((TBFSDatabaseDriver*)m_pAlternativeDriver)->GetFormattedQueryTime();

	return _T("");
}
//-----------------------------------------------------------------------------
CString CFileSystemManager::GetFormattedFetchTime()
{
	if (IsAlternativeDriverEnabled() && GetAlternativeDriver()->IsKindOf(RUNTIME_CLASS(TBFSDatabaseDriver)))
		return ((TBFSDatabaseDriver*)m_pAlternativeDriver)->GetFormattedFetchTime();

	return _T("");
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CFileSystemManager::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CFileSystemManager\n");
}

void CFileSystemManager::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG


