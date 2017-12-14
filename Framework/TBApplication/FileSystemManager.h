#pragma once

#include <TbNameSolver/IFileSystemManager.h>
#include <TbNameSolver/FileSystemCache.h>
#include <TbClientCore/FileSystemManagerInfo.h>

#include <TbGenlib/Messages.h>
#include <TbOleDb/SqlRec.h>

//includere alla fine degli include del .H
//==============================================================================
class CFileSystemManager : public IFileSystemManager
{
private:
	CFileSystemManagerInfo	m_ConfigFile;

public:
	CFileSystemManager	(BOOL bCacheEnabled = TRUE);
	CFileSystemManager	(IFileSystemDriver* pFileSystem, IFileSystemDriver* pAlternative = NULL, BOOL bCacheEnabled = TRUE);
	~CFileSystemManager	();
	
public:
	virtual CString		GetServerConnectionConfig	();
	virtual DataBlob	GetBinaryFile				(const CString& sFileName);
	virtual CString		GetTextFile					(const CString& sFileName);
	virtual BOOL		SetTextFile					(const CString& sFileName, const CString& sFileContent);
	virtual BOOL		ExistFile					(const CString& sFileName);
	virtual BOOL		RemoveFile					(const CString& sFileName);
	virtual BOOL		RenameFile					(const CString& sOldFileName, const CString& sNewName);
	virtual BOOL		GetFileStatus				(const CString& sFileName, CFileStatus& fs);
	virtual DWORD		GetFileAttributes			(const CString& sFileName);
	virtual BOOL		CopyFile					(const CString& sOldFileName, const CString& sNewName, const BOOL& bOverWrite);

	virtual BOOL		ExistPath					(const CString& sPathName);
	virtual BOOL		CreateFolder				(const CString& sPathName, const BOOL& bRecursive);
	virtual BOOL		RemoveFolder				(const CString& sPathName, const BOOL& bRecursive,  const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents = FALSE);
	virtual BOOL		CopyFolder					(const CString& sOldPathName, const CString& sNewPathName, const BOOL& bOverwrite, const BOOL& bRecursive);
	virtual BOOL		GetSubFolders				(const CString& sPathName, CStringArray* pSubFolders);
	virtual BOOL		GetPathContent				(const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders,  BOOL bFiles, const CString& sFileExt, CStringArray* pFiles);
	virtual BOOL		GetFiles					(const CString& sPathName, const CString& sFileExt, CStringArray* pFiles);

	// utility
	virtual const CString	GetTemporaryBinaryFile		(const CString& sFileName);

private:
	virtual BOOL		Init				(const CString& strServer, const CString& strInstallation, const CString& strMasterSolutionName);

#ifdef _DEBUG
	void Dump		(CDumpContext& dc) const;
	void AssertValid() const;
#endif //_DEBUG
};   
