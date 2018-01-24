#include "stdafx.h"
#include <stdio.h>
#include <dos.h>
#include <sys\stat.h>
#include <errno.h>

#include <TbGeneric/TbStrings.h>

#include <TbNameSolver/chars.h>
#include <TbNameSolver/FileSystemFunctions.h>
#include <TbNameSolver/FileSystemCache.h>
#include <TbNameSolver/PathFinder.h>
#include <TbNameSolver/Diagnostic.h>

#include <TbXmlCore/XMLSaxReader.h>

#include <TbGeneric/DataObj.h>
#include <TbGeneric/GeneralFunctions.h>
#include <TbGeneric/FileSystemCacheFileLoader.h>

#include "FileSystemDriver.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////
//				class CFileSystemDriver implementation
///////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
CFileSystemDriver::CFileSystemDriver()
	:
	IFileSystemDriver	()
{
}

////-----------------------------------------------------------------------------
//BOOL CFileSystemDriver::CanCache () const
//{
//	return FALSE;
//}
//
////-----------------------------------------------------------------------------
//BOOL CFileSystemDriver::LoadCache (CFileSystemCacher* pCacher)
//{
//	return TRUE;
//}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::IsAManagedObject (const CString& sFileName) const
{
	return TRUE;
}

//-----------------------------------------------------------------------------
CString	CFileSystemDriver::GetDriverDescription () const
{
	return _TB("File System from LAN Sharings");	
}

// see use of CPiture, CImage, etc...
//-----------------------------------------------------------------------------
BYTE* CFileSystemDriver::GetBinaryFile (const CString& sFileName, int& nLen)
{
	CFile f;
	f.Open(sFileName, CFile::modeRead | CFile::typeBinary);
	nLen = (int)f.GetLength();
	BYTE* buff = new BYTE[nLen];
	f.Read(buff, nLen);
	return buff;
}

// see use of CPiture, CImage, etc...
//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::SaveBinaryFile(const CString& sFileName, BYTE* sBinaryContent, int nLen)
{
	ASSERT(FALSE);
	return TRUE;
}

// see use of CLineFile, CXmlSaxReader, CXmlDocObj
//-----------------------------------------------------------------------------
CString CFileSystemDriver::GetTextFile (const CString& sFileName)
{
	ASSERT (FALSE);
	return _T("");
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::SaveTextFile(const CString& sFileName, const CString& sFileContent)
{
	ASSERT (FALSE);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::ExistFile (const CString& sFileName)
{
	CString strTmp (sFileName);

	if (IsDriveName (sFileName))
		strTmp = sFileName + CString(SLASH_CHAR);
	else
		if (IsDirSeparator(sFileName[sFileName.GetLength() - 1]))
		{
			strTmp = sFileName.Left(sFileName.GetLength() - 1);
			if (IsDriveName (strTmp)) strTmp = sFileName;
		}

	struct _stat statbuf;
	return (_tstat((TCHAR*)(LPCTSTR)strTmp, &statbuf) == 0) && ((statbuf.st_mode & _S_IFREG) == _S_IFREG);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::ExistPath (const CString& sPathName)
{
	if (sPathName.IsEmpty()) 
		return FALSE;
    	
	CString strTmp (sPathName);

	if (IsDriveName (sPathName))
		strTmp = sPathName + CString(SLASH_CHAR);
	else
		if (IsDirSeparator(sPathName[sPathName.GetLength() - 1]))
		{
			strTmp = sPathName.Left(sPathName.GetLength() - 1);
			if (IsDriveName (strTmp)) strTmp = sPathName;
		}

	struct _stat statbuf;

	return (_tstat((TCHAR*)(LPCTSTR)strTmp, &statbuf) == 0) && ((statbuf.st_mode & _S_IFDIR) == _S_IFDIR);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::CreateFolder(const CString& sPathName, const BOOL& bRecursive)
{
	if (sPathName.IsEmpty ())
		return FALSE;

	if (ExistPath(sPathName))
		return TRUE;

	// file system
	if (!bRecursive)
		return  ::CreateDirectory (sPathName, NULL);

	BOOL bOk = FALSE;
	CString strParentPath;

	if (IsDirSeparator(sPathName.Right(1)))
		strParentPath = GetPath(sPathName.Left(sPathName.GetLength() - 1));
	else
		strParentPath = GetPath(sPathName);

	if (!CreateFolder(strParentPath, bRecursive))
		bOk = FALSE;
	else
		bOk =  ::CreateDirectory(sPathName, NULL);
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::RemoveFolder(const CString& sPathName, const BOOL& bRecursive, const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents)
{
	BOOL bOk = FALSE;

	if (bRecursive)
		bOk = RemoveChildsFolders (sPathName);

	if (bOk && bRemoveRoot)
		bOk = ::RemoveDirectory (sPathName);

	if (bOk && bAndEmptyParents)
		bOk = RemoveParentFolders (sPathName);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::RemoveChildsFolders (const CString& sPathName)
{
	if (_taccess(sPathName, 02) == -1) 		// accesso al file
		return FALSE;

	BOOL bOk = TRUE;

	CFileFind finder;  
	BOOL bWorking =  finder.FindFile(sPathName + URL_SLASH_CHAR + _T("*.*"));   

	while (bWorking)
	{     
		bWorking = finder.FindNextFile();

		// evito "." e ".." per evitare ricorsione
		if (finder.IsDots())
			continue;

		if (finder.IsDirectory())
		{
			CString sFolderPath = finder.GetFilePath();
			if (!RemoveChildsFolders(sFolderPath))
				bOk = FALSE;
			
			if (!::RemoveDirectory(sFolderPath))
				bOk = FALSE;
		}
		else
		{
			if (!::DeleteFile(finder.GetFilePath()))
				bOk = FALSE;
		}
	}
	
	finder.Close ();
		
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::RemoveParentFolders (const CString& sPathName)
{
	CString strFilePath;
	if (IsDirSeparator(sPathName.Right(1)))
		strFilePath = sPathName.Left(sPathName.GetLength() - 1);
	else
		strFilePath = sPathName;

	// if doesn't exists on db, I always check file system
	BOOL bOk = FALSE;

	CFileFind finder;  
	BOOL bWorking =  finder.FindFile(sPathName + URL_SLASH_CHAR + _T("*.*"));   
	BOOL bToRemove = TRUE;
	while (bWorking)
	{     
		bWorking = finder.FindNextFile();

		// ho trovato un file o una directory quindi non rimuoverò la dir
		if (!finder.IsDots())
		{
			bToRemove = FALSE;
			break;
		}
	}
	
	finder.Close ();
	
	// se è vuota e la riesco a rimuovere saldo alla parent
	if (bToRemove && ::RemoveDirectory (strFilePath))
		RemoveParentFolders(GetPath(strFilePath));

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::RemoveFile (const CString& sFileName)
{
	BOOL bOk = FALSE;

	if (_taccess(sFileName, 02) == -1)
		_tchmod (sFileName, _S_IREAD | _S_IWRITE);

	return _tremove ((LPCTSTR) sFileName) == 0;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::RenameFile (const CString& sOldFileName, const CString& sNewFileName)
{
	BOOL bOk = FALSE;

	if (IsDosName(sOldFileName) && ExistPath(::GetPath(sOldFileName, FALSE)))
	{
		int nResult = ::_trename(sOldFileName, sNewFileName);
		if (nResult == 0)
			bOk = TRUE;

		if (!bOk)
		{
			const rsize_t nLen = 512;
			TCHAR szBuffer[nLen];
			switch (errno)
			{
				case EACCES:
					_stprintf_s(szBuffer, nLen, _T("RenameFilePath:%s already exists or cannot be created.  "),LPCTSTR(sNewFileName));
				case ENOENT: 
					_stprintf_s (szBuffer, nLen, _T("RenameFilePath:%s not found"), LPCTSTR(sOldFileName));
				case EINVAL:
					_stprintf_s (szBuffer, nLen, _T("RenameFilePath: invalid characters: /n%s /n%s in name"), LPCTSTR(sOldFileName), LPCTSTR(sNewFileName));
				default:
					_stprintf_s (szBuffer, nLen, _T("RenameFilePath: error renaming %s into %s"), LPCTSTR(sOldFileName), LPCTSTR(sNewFileName)); 
			}
			TRACE(szBuffer);
		}
	}
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::CopySingleFile(const CString& sOldFileName, const CString& sNewFileName, const BOOL& bOverWrite)
{
	BOOL bSetReadOnly = FALSE;
	if (bOverWrite && ExistFile(sNewFileName))
	{
		DWORD dwAttr = GetTbFileAttributes((LPCTSTR) sNewFileName);
		if (FILE_ATTRIBUTE_READONLY & dwAttr)
		{
			if (!SetFileAttributes((LPCTSTR) sNewFileName, dwAttr & !FILE_ATTRIBUTE_READONLY))
			{
				AfxGetDiagnostic()->Add (cwsprintf(_TB("Copy of the file %s has failed due to the following errors: error removing readonly attribute on destination file!"), sNewFileName), CDiagnostic::Warning);
				return FALSE;
			}
			bSetReadOnly = TRUE;
		}
	}
	if (!::CopyFile ( (LPCTSTR) sOldFileName, (LPCTSTR) sNewFileName, !bOverWrite))
	{
		LPVOID	lpMsgBuf;
		FormatMessage
		( 
			FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
			NULL,
			GetLastError(),
			MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
			(LPTSTR) &lpMsgBuf,
			0,
			NULL
		);
		AfxGetDiagnostic()->Add (cwsprintf(_TB("Copy of the file %s has failed due to the following errors %s"), sNewFileName, (LPCTSTR) lpMsgBuf), CDiagnostic::Warning);
		return FALSE;
	}
	if (bSetReadOnly)
	{
		DWORD dwAttr = GetTbFileAttributes((LPCTSTR) sNewFileName);
		if (!SetFileAttributes((LPCTSTR) sNewFileName, dwAttr & FILE_ATTRIBUTE_READONLY))
			AfxGetDiagnostic()->Add (cwsprintf(_TB("Copy of the file %s with success, but cannot restore readonly attribute on destination file!"), sNewFileName), CDiagnostic::Warning);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::GetFileStatus (const CString& sFileName, CFileStatus& fs)
{
	return CFile::GetStatus(sFileName, fs);
}

//-----------------------------------------------------------------------------
DWORD CFileSystemDriver::GetFileAttributes (const CString& sFileName)
{
	return ::GetFileAttributes (sFileName);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::CopyFolder (const CString& sOldPathName, const CString& sNewPathName, const BOOL& bOverwrite, const BOOL& bRecursive)
{
	BOOL bOk = FALSE;

	CString strOriginPath, strDestinationPath;
	if (IsDirSeparator(sOldPathName.Right(1)))
		strOriginPath = sOldPathName.Left(sOldPathName.GetLength() - 1);
	else
		strOriginPath = sOldPathName;

	if (IsDirSeparator(sNewPathName.Right(1))) 
		strDestinationPath = sNewPathName.Left(sNewPathName.GetLength() - 1);
	else
		strDestinationPath = sNewPathName;
		
	BOOL bExistDestination = ExistPath (strDestinationPath);

	if (!bOverwrite && bExistDestination)
		return FALSE;

	if (!bExistDestination && !CreateFolder (strDestinationPath, FALSE))
		return FALSE;

	CFileFind finder;  
	BOOL bWorking =  finder.FindFile(strOriginPath + SLASH_CHAR + _T("*.*"));   
	
	BOOL bResult = TRUE;
	while (bWorking)
	{     
		bWorking = finder.FindNextFile();

		// evito "." e ".." per evitare ricorsione
		if (finder.IsDots())
			continue;

		if (finder.IsDirectory())
		{
			if(!CopyFolder (strOriginPath + SLASH_CHAR + finder.GetFileName(), 
								strDestinationPath + SLASH_CHAR + finder.GetFileName(),
								bOverwrite, bRecursive))
				bResult = FALSE;
		}
		else
		{
			if (!CopyFile(strOriginPath + SLASH_CHAR + finder.GetFileName(), 
								strDestinationPath + SLASH_CHAR + finder.GetFileName(), bOverwrite))
				bResult = FALSE;
		}
	}
	
	finder.Close ();
	bOk = bResult;
	return bOk;

}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::GetSubFolders (const CString& sPathName, CStringArray* pSubFolders)
{
	CFileFind aFinder;

	BOOL bWorking = aFinder.FindFile(sPathName + _T("\\*.*"));
	CString sSubDirName;
	while (bWorking)
	{    
		bWorking = aFinder.FindNextFile();

		// salto ., .., e i files
		if (aFinder.IsDots () || !aFinder.IsDirectory())
			continue;
		
		sSubDirName = aFinder.GetFileName();
		if (!sSubDirName.IsEmpty())
			pSubFolders->Add (sSubDirName);
	}

	aFinder.Close();	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::GetFiles (const CString& sPathName, const CString& sFileExt, CStringArray* pFiles)
{
	CFileFind aFinder;
	DWORD dw = 0;
	BOOL bWorking = aFinder.FindFile(sPathName + _T("\\") + sFileExt);
	if (bWorking == 0)
	{
		dw = GetLastError();

	}
	CString sName;
	while (bWorking)
	{    
		bWorking = aFinder.FindNextFile();

		// salto ., .., e i files
		if (aFinder.IsDots () || aFinder.IsDirectory())
			continue;
		
		sName = aFinder.GetFilePath();
		pFiles->Add (sName);
	}

	aFinder.Close();	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemDriver::GetPathContent (const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders,  BOOL bFiles, const CString& sFileExt, CStringArray* pFiles)
{
	BOOL bOk = TRUE;
	if (bFolders)
		bOk = GetSubFolders (sPathName, pSubFolders);

	if (bFiles)
		bOk = GetFiles (sPathName, sFileExt, pFiles) && bOk ;

	return bOk;
}

//-----------------------------------------------------------------------------
CString	CFileSystemDriver::GetServerConnectionConfig ()
{
	// no lock is required
	CXMLDocumentObject doc;

	if (!doc.LoadXMLFile(AfxGetPathFinder()->GetServerConnectionConfigFullName()))
		return _T("");

	CString sInnerText;
	doc.GetXML (sInnerText);
	
	return sInnerText;
}

//-------------------------------------------------------------------------------------
void CFileSystemDriver::AddApplicationDirectories(const CString& sAppContainerPath, CStringArray* pAppsPath)
{
	BOOL bIsCustom = AfxGetPathFinder()->IsCustomPath(sAppContainerPath);
	CStringArray arFolders;
	TBFile* pMetadataObj = NULL;
	//CString strFileName;
	CString strFolder;
	CString strPath;
	GetSubFolders(sAppContainerPath, &arFolders);
	for (int i = 0; i <= arFolders.GetUpperBound(); i++)
	{
		strFolder = arFolders.GetAt(i);
		strPath = sAppContainerPath + SLASH_CHAR + strFolder;
		strPath.MakeLower();
		if (ExistFile(AfxGetPathFinder()->GetApplicationConfigFullNameFromPath(strPath)))
			pAppsPath->Add(strPath);
	}
}
//-----------------------------------------------------------------------------
void CFileSystemDriver::GetAllApplicationInfo(CStringArray* pAppsPath)
{
	if (!pAppsPath)
		pAppsPath = new CStringArray();

	const CString sAppContainerPath = AfxGetPathFinder()->GetContainerPath(CPathFinder::TB);
	CString strFolder = szTaskBuilderApp;
	CString strPath = sAppContainerPath + SLASH_CHAR + strFolder;
	strPath.MakeLower();
	pAppsPath->Add(strPath); 

	strFolder = szExtensionsApp;
	strPath = sAppContainerPath + SLASH_CHAR + strFolder;
	strPath.MakeLower();	
	pAppsPath->Add(strPath);

	AddApplicationDirectories(AfxGetPathFinder()->GetContainerPath(CPathFinder::TB_APPLICATION), pAppsPath);
	AddApplicationDirectories(AfxGetPathFinder()->GetCustomApplicationsPath(), pAppsPath);
}



//-------------------------------------------------------------------------------------
void CFileSystemDriver::AddApplicationModules(const CString& sApplicationPath, CStringArray* pModulesPath, bool isCustom)
{
	CStringArray arFolders;
	GetSubFolders(sApplicationPath, &arFolders);

	CString strModulePath;
	CString strModuleName;

	for (int i = 0; i <= arFolders.GetUpperBound(); i++)
	{
		strModuleName = arFolders.GetAt(i);
		strModulePath = sApplicationPath + SLASH_CHAR + strModuleName;
		if (!strModuleName.IsEmpty() && ExistFile(strModulePath + SLASH_CHAR + AfxGetPathFinder()->GetModuleConfigName()))
			pModulesPath->Add(strModulePath);
	}
}

//-----------------------------------------------------------------------------
void CFileSystemDriver::GetAllModuleInfo(const CString& strAppName, CStringArray* pModulesPath)
{
	ASSERT(pModulesPath);
	// load modules namespaces into map form file system
	AddApplicationModules(AfxGetPathFinder()->GetApplicationPath(strAppName, CPathFinder::STANDARD), pModulesPath, false);
	if (pModulesPath->GetSize() == 0) //non ho moduli: si tratta di un'applicazione nella custom?
		AddApplicationModules(AfxGetPathFinder()->GetApplicationPath(strAppName, CPathFinder::CUSTOM, FALSE), pModulesPath, true);
}







/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CFileSystemDriver::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CFileSystemDriver\n");
}

void CFileSystemDriver::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG
