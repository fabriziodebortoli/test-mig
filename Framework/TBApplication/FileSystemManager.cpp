#include "stdafx.h"
#include <stdio.h>
#include <dos.h>
#include <sys\stat.h>
#include <errno.h>

#include <TbNameSolver\Diagnostic.h>
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
BOOL CFileSystemManager::Init(const CString& strServer, const CString& strInstallation, const CString& strMasterSolutionName)
{
	// No lock is required as it is performed only in InitInstance

	// FileSystemManager parsing
	m_ConfigFile.LoadFile ();

	CPathFinder aPathFinder;
	aPathFinder.Init 
		(
			strServer,
			strInstallation,
			strMasterSolutionName
		);

	//-------------------------------------------------------------
	// initializing Web Service
	/*AfxGetFileSystemManagerWebService()->InitService 
		(
			aPathFinder.GetInstallationName () + m_ConfigFile.GetWebServiceDriverService(),
			m_ConfigFile.GetWebServiceDriverNamespace(),
			pClientInfo->m_strApplicationServer,
			m_ConfigFile.GetWebServiceDriverPort ()
		);
	  bWSAvailable	= AfxGetFileSystemManagerWebService()->IsAlive ();*/
	BOOL bWSAvailable	= FALSE; 
	//-------------------------------------------------------------

	// database connection is not yet available
	BOOL bExistFileSystem	= ExistPath (aPathFinder.GetStandardPath ()) && ExistPath (aPathFinder.GetCustomPath ());

	// I cannot work without file system and web service
	if (!bExistFileSystem && !bWSAvailable)
	{
		ASSERT (FALSE);
		AfxGetDiagnostic()->Add (_T("The \\Standard and \\Custom folders are not reacheable on server structure!"));
		AfxGetDiagnostic()->Add (_T("Please, check network sharings if you are on a client/server installation."));
		return FALSE;
	}

	// autodetect operations
	if (m_ConfigFile.IsAutoDetectDriver())
	{
		if (m_ConfigFile.GetDriver() == CFileSystemManagerInfo::FileSystem && !bExistFileSystem)
			m_ConfigFile.SetDriver(CFileSystemManagerInfo::WebService);
		else if (m_ConfigFile.GetDriver() == CFileSystemManagerInfo::WebService && !bWSAvailable)
			m_ConfigFile.SetDriver(CFileSystemManagerInfo::FileSystem);
	}

	if (m_ConfigFile.GetDriver() == CFileSystemManagerInfo::WebService)
		AttachAlternativeDriver(AfxGetFileSystemManagerWebService(), FALSE, FALSE);

	//-------------------------------------------------------------
	//	else if (_ConfigFile.GetDriver() == CFileSystemManagerInfo::Database)
	//		AttachAlternativeDriver(new CFileSystemOnDbDriver(), FALSE);
	//-------------------------------------------------------------

	m_Cacher.m_bEnabled = m_ConfigFile.IsCachingEnabled();

	return TRUE;
}


// no content cached
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

// no content cached
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
BOOL CFileSystemManager::SetTextFile (const CString& sFileName, const CString& sFileContent)
{
	// no lock is required as methods invoked work only on local variables
	// an they don't locks other objects

	BOOL bOk = FALSE;

	if (IsManagedByAlternativeDriver (sFileName))
		bOk = GetAlternativeDriver()->SetTextFile (sFileName, sFileContent);
	else
		bOk = GetFileSystemDriver()->SetTextFile (sFileName, sFileContent);

	return bOk;
}

// no cached content
//-----------------------------------------------------------------------------
DataBlob CFileSystemManager::GetBinaryFile	(const CString& sFileName)
{
	// no lock is required as methods invoked work only on local variables
	// an they don't locks other objects
	DataBlob aContent;

	CString sContent; 
	if (IsManagedByAlternativeDriver (sFileName))
		aContent.Assign(GetAlternativeDriver()->GetBinaryFile (sFileName));
	else
		aContent.Assign(GetFileSystemDriver()->GetBinaryFile (sFileName));

	return aContent;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::ExistFile (const CString& sFileName)
{
	if (sFileName.IsEmpty()) 
		return FALSE;

	BOOL bOk = FALSE;


	// caching (no lock is required as demanded to the single methods if needed)
	if (m_Cacher.IsAManagedObject (sFileName))
	{
		bOk = m_Cacher.ExistFile (sFileName);
		return bOk;
	}

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
	if (m_Cacher.IsAManagedObject (sPathName))
	{
		bOk =  m_Cacher.ExistPath (sPathName);
		return bOk;
	}
    	
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
	if (bOk && m_Cacher.IsAManagedObject (sPathName))
		bOk = m_Cacher.CreateFolder (sPathName, bRecursive);

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
	if (bOk && m_Cacher.IsAManagedObject (sPathName))
		bOk =  m_Cacher.RemoveFolder (sPathName, bRecursive, bRemoveRoot, bAndEmptyParents);

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
	if (bOk && m_Cacher.IsAManagedObject (sFileName))
		bOk = m_Cacher.RemoveFile (sFileName);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::RenameFile (const CString& sOldFileName, const CString& sNewFileName)
{
	BOOL bOk = FALSE;

	BOOL bOldOnAlternative = IsManagedByAlternativeDriver (sOldFileName);
	BOOL bNewOnAlternative = IsManagedByAlternativeDriver (sNewFileName);

	if (IsManagedByAlternativeDriver (sOldFileName) || IsManagedByAlternativeDriver (sNewFileName))
		bOk = GetAlternativeDriver()->RenameFile (sOldFileName, sNewFileName);
	else  if (bOldOnAlternative && !bNewOnAlternative)
	{
		CString sContent = GetAlternativeDriver()->GetTextFile (sOldFileName);
		bOk  = sContent.IsEmpty() || GetFileSystemDriver()->SetTextFile (sNewFileName, sContent);
	}	
	else  if (!bOldOnAlternative && bNewOnAlternative)
	{
		CString sContent = GetFileSystemDriver()->GetTextFile (sOldFileName);
		bOk  = sContent.IsEmpty() || GetAlternativeDriver()->SetTextFile (sNewFileName, sContent);
	}
	else
		bOk = GetFileSystemDriver()->RenameFile (sOldFileName, sNewFileName);

	// caching (no lock is required as demanded to the single methods if needed)
	if (bOk && (m_Cacher.IsAManagedObject (sOldFileName) || m_Cacher.IsAManagedObject (sNewFileName)))
		bOk = m_Cacher.RenameFile (sOldFileName, sNewFileName);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::CopyFile (const CString& sOldFileName, const CString& sNewFileName, const BOOL& bOverWrite)
{
	BOOL bOk = FALSE;

	BOOL bOldOnAlternative = IsManagedByAlternativeDriver (sOldFileName);
	BOOL bNewOnAlternative = IsManagedByAlternativeDriver (sNewFileName);

	if (bOldOnAlternative && bOldOnAlternative)
		bOk = GetAlternativeDriver()->CopyFile (sOldFileName, sNewFileName, bOverWrite);
	else  if (bOldOnAlternative && !bNewOnAlternative)
	{
		CString sContent = GetAlternativeDriver()->GetTextFile (sOldFileName);
		bOk  = sContent.IsEmpty() || GetFileSystemDriver()->SetTextFile (sNewFileName, sContent);
	}
	else  if (!bOldOnAlternative && bNewOnAlternative)
	{
		CString sContent = GetFileSystemDriver()->GetTextFile (sOldFileName);
		bOk  = sContent.IsEmpty() || GetAlternativeDriver()->SetTextFile (sNewFileName, sContent);
	}
	else  
		bOk = GetFileSystemDriver()->CopyFile (sOldFileName, sNewFileName, bOverWrite);

	// caching (no lock is required as demanded to the single methods if needed)
	if (bOk && m_Cacher.IsAManagedObject (sNewFileName))
	{
		CString sPath = GetPath	(sNewFileName);
		CString sFile = GetNameWithExtension(sNewFileName);

		bOk = m_Cacher.AddInCache (sPath, sFile);
	}

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
	if (bOk && m_Cacher.IsAManagedObject (sNewPathName))
		bOk = m_Cacher.CreateFolder (sNewPathName, bRecursive);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManager::GetSubFolders (const CString& sPathName, CStringArray* pSubFolders)
{
	BOOL bOk = FALSE;

	// caching (no lock is required as demanded to the single methods if needed)
	if (m_Cacher.IsAManagedObject (sPathName))
	{
		bOk =  m_Cacher.GetSubFolders (sPathName, pSubFolders);
		return bOk;
	}
    	
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
	if (m_Cacher.IsAManagedObject (sPathName))
	{
		bOk =  m_Cacher.GetFiles (sPathName, sFileExt, pFiles);
		return bOk;
	}
    	
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
	DataBlob aBlob = GetBinaryFile (sFileName);
	if (aBlob.IsEmpty())
		return sFileName;

	CString sExtension		= GetExtension(sFileName);
	CString sTempFileName	= GetTempName() + _T(".") + sExtension;

	void* buff = aBlob.GetRawData();

	CFile aFile;
	CFileException exc;

	TRY
	{
		if (aFile.Open (sTempFileName, CFile::typeBinary | CFile::modeCreate | CFile::modeWrite), &exc)
		{
			aFile.Write (buff, aBlob.GetLen());
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


