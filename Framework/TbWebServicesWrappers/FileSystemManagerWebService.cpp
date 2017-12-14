#include "StdAfx.h"

#include <TbNameSolver\ApplicationContext.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\FileSystemCache.h>
#include <TbNameSolver\FileSystemFunctions.h>

#include <TbGeneric\FileSystemCacheFileLoader.h>
#include <TBGeneric\FunctionCall.h>

#include <TbGenlibManaged\Main.h>

#include "FileSystemManagerWebService.h"
#include "LoginManagerInterface.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szNameAttribute[]		= _T("name");
static const TCHAR szFoldersQuery[]		= _T("/List/Folder");
static const TCHAR szFilesQuery[]			= _T("/List/File");

// no lock required as all methods manages local objects and just invoke soap calls
//----------------------------------------------------------------------------
CFileSystemManagerWebService* AFXAPI AfxGetFileSystemManagerWebService ()
{
	return AfxGetApplicationContext()->GetObject<CFileSystemManagerWebService>(&CApplicationContext::GetFileSystemManagerWS);
}

//-----------------------------------------------------------------------------
CFileSystemManagerWebService::CFileSystemManagerWebService (const CString& strService, const CString& strServiceNamespace, const CString& strServer, int nWebServicesPort)
	:
	m_strService(strService),
	m_strServiceNamespace(strServiceNamespace),
	m_strServer(strServer),
	m_nWebServicesPort(nWebServicesPort)

{
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::CanCache () const
{
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::LoadCache (CFileSystemCacher* pCacher)
{
	return TRUE;
}

//-----------------------------------------------------------------------------
CString	CFileSystemManagerWebService::GetDriverDescription () const
{
	return _T("FileSystemManager Web Service");
}

//-----------------------------------------------------------------------------
CString	CFileSystemManagerWebService::GetServerConnectionConfig	()
{
	CString sContent;
	if (GetServerConnectionConfig (_T(""), sContent))
		return sContent;

	return _T("");
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::IsAManagedObject	(const CString& sFileName) const
{
	// isServerPath
		return 	_tcsncmp(sFileName, CString(SLASH_CHAR, 2), 2) == 0 ||
			_tcsncmp(sFileName, CString(URL_SLASH_CHAR, 2), 2) == 0;		
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::IsAlive ()
{
	CFunctionDescription aFunctionDescription(_T("IsAlive"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	
	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetServerConnectionConfig (const CString& sAuthenticationToken, CString& sFileContent)
{
	CFunctionDescription aFunctionDescription(_T("GetServerConnectionConfig"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	DataStr		aContent;
	aFunctionDescription.AddOutParam	(_T("fileContent"),		&aContent);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = FALSE;
	DataObj* pDataObj;

	bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);
	
	sFileContent += aContent; 

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetTextFile (const CString& sAuthenticationToken, const CString& sFileName, CString& sFileContent)
{
	CFunctionDescription aFunctionDescription(_T("GetTextFile"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	DataStr aContent;

	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("fileName"),			sFileName);
	aFunctionDescription.AddOutParam(_T("fileContent"),			&aContent);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	sFileContent = aContent;

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::SetTextFile (const CString& sAuthenticationToken, const CString& sFileName, const CString& sFileContent)
{
	CFunctionDescription aFunctionDescription(_T("SetTextFile"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("fileName"), sFileName);
	aFunctionDescription.AddStrParam(_T("fileContent"), sFileContent);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetBinaryFile (const CString& sAuthenticationToken, const CString& sFileName, DataBlob* aFileContent)
{
	CFunctionDescription aFunctionDescription(_T("GetBinaryFile"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	DataBlob aContent;

	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("fileName"),			sFileName);
	aFunctionDescription.AddOutParam(_T("fileContent"),			&aContent);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	aFileContent->Assign (aContent);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::RemoveFolder (const CString& sAuthenticationToken, const CString& sFileName, const BOOL& bRecursive, const BOOL& bEmptyOnly)
{
	CFunctionDescription aFunctionDescription(_T("RemoveFolder"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	DataBool aEmptyOnly = bEmptyOnly;
	DataBool aRecursive	= bRecursive;

	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("pathName"), sFileName);
	aFunctionDescription.AddParam	(_T("recursive"), &aRecursive);
	aFunctionDescription.AddParam	(_T("emptyOnly"), &aEmptyOnly);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::CreateFolder (const CString& sAuthenticationToken, const CString& sPathName, const BOOL& bRecursive)
{
	CFunctionDescription aFunctionDescription(_T("CreateFolder"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	DataBool aRecursive	= bRecursive;

	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("pathName"), sPathName);
	aFunctionDescription.AddParam	(_T("recursive"), &aRecursive);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::CopyFolder (const CString& sAuthenticationToken, const CString& sOldPathName, const CString& sNewPathName, const BOOL& bRecursive)
{
	CFunctionDescription aFunctionDescription(_T("CopyFolder"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	DataBool aRecursive	= bRecursive;

	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("oldPathName"), sOldPathName);
	aFunctionDescription.AddStrParam(_T("newPathName"), sNewPathName);
	aFunctionDescription.AddParam	(_T("recursive"), &aRecursive);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::RemoveFile (const CString& sAuthenticationToken, const CString& sFileName)
{
	CFunctionDescription aFunctionDescription(_T("RemoveFile"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("fileName"), sFileName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::CopyFile(const CString& sAuthenticationToken, const CString& sOldFileName, const CString& sNewFileName, const BOOL& bOverWrite)
{
	CFunctionDescription aFunctionDescription(_T("CopyFile"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("oldPathName"), sOldFileName);
	aFunctionDescription.AddStrParam(_T("newPathName"), sNewFileName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::RenameFile(const CString& sAuthenticationToken, const CString& sOldFileName, const CString& sNewFileName)
{
	CFunctionDescription aFunctionDescription(_T("RenameFile"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("oldFileName"), sOldFileName);
	aFunctionDescription.AddStrParam(_T("newFileName"), sNewFileName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);
	
	return bOk;
}

//-----------------------------------------------------------------------------
DWORD CFileSystemManagerWebService::GetFileAttributes (const CString& sAuthenticationToken, const CString& sFileName)
{
	CFunctionDescription aFunctionDescription(_T("GetFileAttributes"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	DataInt nStatus;
	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("fileName"), sFileName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Long, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	DWORD attributes = FILE_ATTRIBUTE_NORMAL;

	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Long)
	{
		DataLng aReturnValue = *((DataLng*) pDataObj);
		attributes = (DWORD) (long) aReturnValue;
	}

	return attributes;
;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::ExistPath (const CString& sAuthenticationToken, const CString& sPathName)
{
	CFunctionDescription aFunctionDescription(_T("ExistPath"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("pathName"),			sPathName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);

	return bOk;
};

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::ExistFile (const CString& sAuthenticationToken, const CString& sFileName)
{
	CFunctionDescription aFunctionDescription(_T("ExistFile"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);

	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("fileName"),			sFileName);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	
	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);

	return bOk;
};

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetFileStatus (const CString& sAuthenticationToken, const CString& sFileName, CFileStatus& fs)
{
	CFunctionDescription aFunctionDescription(_T("GetFileStatus"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);

	DataDate aCreationTime;
	DataDate aLastWriteTime;
	DataDate aLastAccessTime;
	DataLng	 nLength;

	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("fileName"),	sFileName);
	
	aFunctionDescription.AddOutParam(_T("creation"),	&aCreationTime);
	aFunctionDescription.AddOutParam(_T("lastAccess"),	&aLastAccessTime);
	aFunctionDescription.AddOutParam(_T("lastWrite"),	&aLastWriteTime);
	aFunctionDescription.AddOutParam(_T("length"),		&nLength);

	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);

	if (bOk)
	{
		int i=0;
		for (i=0; i <= sFileName.GetLength (); i++)
			fs.m_szFullName[i] = sFileName.GetAt (i);
		fs.m_szFullName[i] = '\0';

		// conversione da vedere se va bene
		if (!aLastAccessTime.IsEmpty ())
		{
			CTime tAccessTime 
			(
				aLastAccessTime.Year (),
				aLastAccessTime.Month (),
				aLastAccessTime.Day (),
				aLastAccessTime.Hour(),
				aLastAccessTime.Minute(),
				aLastAccessTime.Second()
			);
			fs.m_atime = tAccessTime;
		}
		else 
			fs.m_atime = 0;

		if (!aLastWriteTime.IsEmpty ())
		{
			CTime tWriteTime 
			(
				aLastWriteTime.Year (),
				aLastWriteTime.Month (),
				aLastWriteTime.Day (),
				aLastWriteTime.Hour(),
				aLastWriteTime.Minute(),
				aLastWriteTime.Second()
			);
			fs.m_mtime = tWriteTime;
		}
        else
			fs.m_mtime = 0;


		if (!aCreationTime.IsEmpty ())
		{
			CTime tCreationTime 
			(
				aCreationTime.Year (),
				aCreationTime.Month (),
				aCreationTime.Day (),
				aCreationTime.Hour(),
				aCreationTime.Minute(),
				aCreationTime.Second()
			);
			fs.m_ctime = tCreationTime;
		}
		else
			fs.m_ctime = 0;
		
		fs.m_size = (long) nLength;
	}

	return bOk;
}


//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetPathContent (const CString& sAuthenticationToken, const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders,  BOOL bFiles, const CString& sFileExt, CStringArray* pFiles)
{
	CFunctionDescription aFunctionDescription(_T("GetPathContent"));
	
	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);

	DataStr		aList; 
	DataBool	aFiles		= bFiles;
	DataBool	aFolders	= bFolders;
    
	aFunctionDescription.AddStrParam(_T("authenticationToken"), sAuthenticationToken);
	aFunctionDescription.AddStrParam(_T("pathName"),			sPathName);
	aFunctionDescription.AddStrParam(_T("fileExtension"),		sFileExt);
	aFunctionDescription.AddOutParam(_T("returnDoc"),			&aList);
	aFunctionDescription.AddParam	(_T("folders"),				&aFolders);
	aFunctionDescription.AddParam	(_T("files"),				&aFiles);

	BOOL bOk = InvokeWCFFunction(&aFunctionDescription, FALSE);

	DataObj* pDataObj = aFunctionDescription.GetReturnValue ();
	
	if (bOk && pDataObj && pDataObj->GetDataType () == DataType::Bool)
		bOk = *((DataBool*) pDataObj);

	if (aList.IsEmpty ())
		return bOk;

	// parsing of the result document 
	CXMLDocumentObject aDoc;
	aDoc.LoadXML (aList.GetString());

	CXMLNode* pNode;
	CString sName;
	if (bFolders && pSubFolders)
	{
		CXMLNodeChildsList* pFolderNodes = aDoc.SelectNodes (szFoldersQuery);

		if (pFolderNodes)
		{
			for (int i=0; i <= pFolderNodes->GetUpperBound (); i++)
			{
				pNode = pFolderNodes->GetAt (i);
				
				if (pNode->GetAttribute (szNameAttribute, sName))
					pSubFolders->Add (sName);
			}
			
			delete pFolderNodes;
		}
	}

	if (bFiles && pFiles)
	{
		CXMLNodeChildsList* pFilesNodes = aDoc.SelectNodes (szFilesQuery);

		if (pFilesNodes)
		{
			for (int i=0; i <= pFilesNodes->GetUpperBound (); i++)
			{
				pNode = pFilesNodes->GetAt (i);
				
				if (pNode->GetAttribute (szNameAttribute, sName))
					pFiles->Add (sPathName + SLASH_CHAR + sName);
			}
		
			delete pFilesNodes;
		}
	}

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetSubFolders (const CString& sAuthenticationToken, const CString& sPathName, CStringArray* pSubFolders)
{
	return GetPathContent (sAuthenticationToken, sPathName, TRUE, pSubFolders, FALSE, _T(""), NULL);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetFiles (const CString& sAuthenticationToken, const CString& sPathName, const CString& sFileExt, CStringArray* pSubFiles)
{
	return GetPathContent (sAuthenticationToken, sPathName, FALSE, NULL, TRUE, sFileExt, pSubFiles);
}

 //-----------------------------------------------------------------------------
CString	CFileSystemManagerWebService::GetTextFile (const CString& sFileName)
{
	CString sContent;
	BOOL bOk = GetTextFile
			(
	
				AfxGetAuthenticationToken(), 
				sFileName,
				sContent
			);
	
	return sContent;
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::SetTextFile (const CString& sFileName, const CString& sFileContent)
{
	return SetTextFile 
			(
				AfxGetAuthenticationToken(), 
				sFileName, 
				sFileContent
			);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::ExistFile	(const CString& sFileName)
{
	return ExistFile
		(
			AfxGetAuthenticationToken(),
			sFileName
		); 
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::RemoveFile (const CString& sFileName)
{
	return RemoveFile
			(
				AfxGetAuthenticationToken(),
				sFileName
			);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::RenameFile (const CString& sOldFileName, const CString& sNewFileName)
{
	return RenameFile
		(
			AfxGetAuthenticationToken(),
			sOldFileName,
			sNewFileName
		);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetFileStatus	(const CString& sFileName, CFileStatus& fs)
{
	return GetFileStatus
		(
			AfxGetAuthenticationToken(),
			sFileName,
			fs
		);
}

//-----------------------------------------------------------------------------
DWORD CFileSystemManagerWebService::GetFileAttributes (const CString& sFileName)
{
	return GetFileAttributes
		(
			AfxGetAuthenticationToken(),
			sFileName
		);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::CopyFile (const CString& sOldFileName, const CString& sFileNewName, const BOOL& bOverWrite)
{
	return CopyFile
		(
			AfxGetAuthenticationToken(),
			sOldFileName,
			sFileNewName,
			bOverWrite
		);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::ExistPath	(const CString& sPathName)
{
	return ExistPath 
						(
							AfxGetAuthenticationToken(),
							sPathName
						);

}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::CreateFolder	(const CString& sPathName, const BOOL& bRecursive)
{
	return CreateFolder
			(
				AfxGetAuthenticationToken(),
				sPathName, 
				bRecursive
			);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::RemoveFolder (const CString& sPathName, const BOOL& bRecursive,  const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents /*FALSE*/)
{
	return RemoveFolder
			(
				AfxGetAuthenticationToken(),
				sPathName, 
				bRecursive,
				bAndEmptyParents
			);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::CopyFolder (const CString& sOldPathName, const CString& sNewPathName, const BOOL& bOverwrite, const BOOL& bRecursive)
{
	return CopyFolder
		(
			AfxGetAuthenticationToken(),
			sOldPathName,
			sNewPathName,
			bRecursive
		);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetSubFolders	(const CString& sPathName, CStringArray* pSubFolders)
{
	return GetSubFolders
		(
			AfxGetAuthenticationToken(),
			sPathName,
			pSubFolders
		);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetFiles (const CString& sPathName, const CString& sFileExt, CStringArray* pFiles)
{
	return GetFiles
		(
			AfxGetAuthenticationToken(),
			sPathName,
			sFileExt,
			pFiles
		);
}

//-----------------------------------------------------------------------------
BOOL CFileSystemManagerWebService::GetPathContent (const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders,  BOOL bFiles, const CString& sFileExt, CStringArray* pFiles)
{
	return GetPathContent
		(
			AfxGetAuthenticationToken(),
			sPathName,
			bFolders,
			pSubFolders,
			bFiles,
			sFileExt,
			pFiles
		);
}

 //-----------------------------------------------------------------------------
DataBlob CFileSystemManagerWebService::GetBinaryFile (const CString& sFileName)
{
	DataBlob aContent;
	BOOL bOk = GetBinaryFile
			(
				AfxGetAuthenticationToken(), 
				sFileName,
				&aContent
			);
	
	return aContent;

}
