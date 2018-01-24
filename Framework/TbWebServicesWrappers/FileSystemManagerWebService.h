#pragma once

#include <TbNameSolver/IFileSystemDriver.h>

#include "Beginh.dex"

class CXMLNode;
class DataBlob;
//----------------------------------------------------------------------------
class CFileSystemManagerWebService : public IFileSystemDriver
{
	friend class CTaskBuilderApp;
	friend class IFileSystemManager;
	friend class CFileSystemManager;

private:
	const CString			m_strService;			// nome del WEB service (se esterno)
	const CString			m_strServiceNamespace;	// namespace del WEB service (se esterno)
	const CString			m_strServer;			// nome del server del WEB service (se esterno)
	const int				m_nWebServicesPort;		// numero porta dei Web Services (se esterno)

public:
	CFileSystemManagerWebService	(const CString& strService, const CString& strServiceNamespace, const CString& strServer, int nWebServicesPort);

private:
	// Web Methods
	BOOL		IsAlive						();
	BOOL		GetServerConnectionConfig	(const CString& sAuthenticationToken, CString& sFileContent);
	BOOL		GetTextFile					(const CString& sAuthenticationToken, const CString& sFileName, CString& sFileContent);
	BOOL		SetTextFile					(const CString& sAuthenticationToken, const CString& sFileName, const CString& sFileContent);
	BOOL		GetBinaryFile				(const CString& sAuthenticationToken, const CString& sFileName, DataBlob* aFileContent);
	BOOL		RemoveFolder				(const CString& sAuthenticationToken, const CString& sPathName, const BOOL& bRecursive, const BOOL& bEmptyOnly);
	BOOL		CreateFolder				(const CString& sAuthenticationToken, const CString& sPathName, const BOOL& bRecursive);
	BOOL		CopyFolder					(const CString& sAuthenticationToken, const CString& sOldPathName, const CString& sNewPathName, const BOOL& bRecursive);
	BOOL		RemoveFile					(const CString& sAuthenticationToken, const CString& sFileName);
	BOOL		RenameFile					(const CString& sAuthenticationToken, const CString& sOldFileName, const CString& sNewName);
	BOOL		CopyFile					(const CString& sAuthenticationToken, const CString& sOldFileName, const CString& sNewName, const BOOL& bOverWrite);
	DWORD		GetFileAttributes			(const CString& sAuthenticationToken, const CString& sFileName);
	BOOL		GetFileStatus				(const CString& sAuthenticationToken, const CString& sFileName, CFileStatus& fs);
	BOOL		ExistFile					(const CString& sAuthenticationToken, const CString& sFileName);
	BOOL		ExistPath					(const CString& sAuthenticationToken, const CString& sPathName);
	BOOL		GetSubFolders				(const CString& sAuthenticationToken, const CString& sPathName, CStringArray* pFiles);
	BOOL		GetPathContent				(const CString& sAuthenticationToken, const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders,  BOOL bFiles, const CString& sFileExt, CStringArray* pFiles);
	BOOL		GetFiles					(const CString& sAuthenticationToken, const CString& sPathName, const CString& sFileExt, CStringArray* pFiles);


	// IFileSystemDriver
public:
	virtual CString	GetDriverDescription		() const;

private:
	virtual BOOL	IsAManagedObject			(const CString& sFileName) const;

	virtual CString	GetServerConnectionConfig	();
	virtual CString GetTextFile					(const CString& sFileName);
	virtual BOOL	SetTextFile					(const CString& sFileName, const CString& sFileContent);
	virtual DataBlob GetBinaryFile				(const CString& sFileName);
	virtual BOOL	ExistFile					(const CString& sFileName);
	virtual BOOL	RemoveFile					(const CString& sFileName);
	virtual BOOL	RenameFile					(const CString& sOldFileName, const CString& sNewName);
	virtual BOOL	GetFileStatus				(const CString& sFileName, CFileStatus& fs);
	virtual DWORD	GetFileAttributes			(const CString& sFileName);
	virtual BOOL	CopyFile					(const CString& sOldFileName, const CString& sNewName, const BOOL& bOverWrite);

	virtual BOOL	ExistPath					(const CString& sPathName);
	virtual BOOL	CreateFolder				(const CString& sPathName, const BOOL& bRecursive);
	virtual BOOL	RemoveFolder				(const CString& sPathName, const BOOL& bRecursive,  const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents = FALSE);
	virtual BOOL	CopyFolder					(const CString& sOldPathName, const CString& sNewPathName, const BOOL& bOverwrite, const BOOL& bRecursive);
	virtual BOOL	GetSubFolders				(const CString& sPathName, CStringArray* pSubFolders);
	virtual BOOL	GetPathContent				(const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders,  BOOL bFiles, const CString& sFileExt, CStringArray* pFiles);
	virtual BOOL	GetFiles					(const CString& sPathName, const CString& sFileExt, CStringArray* pFiles);

	/*virtual BOOL	CanCache					() const;
	virtual BOOL	LoadCache					(CFileSystemCacher* pCacher);*/
};

//-----------------------------------------------------------------------------
TB_EXPORT CFileSystemManagerWebService*	AFXAPI AfxGetFileSystemManagerWebService ();

#include "Endh.dex"