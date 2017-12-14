#pragma once

//includere alla fine degli include del .H
#include "FileSystemCache.h"

#include "beginh.dex"

class CDiagnostic;
//==============================================================================
class CFileSystemOperation : public CObject
{
	friend class CFileSystemOperations;
	friend class IFileSystemManager;

private:
	CString		m_sName;
	long		m_lTime;
	int			m_nCalls;

public:
	CFileSystemOperation ();
	CFileSystemOperation (const CString& sName);

private:
	const CString&	GetName () const;
	const long&		GetTime () const;
	const int&		GetCalls() const;

	void Set		(const CString& sName, const long& lTime, const int& nCalls);
	void Increment	(const long& lTime);
	void Decrement	(const long& lTime);
};

//=============================================================================
class  CFileSystemOperations : public CObArray, public CTBLockable
{
	friend class  CFileSystemManager;
	friend class  IFileSystemManager;

public:
	CFileSystemOperations();
	virtual ~CFileSystemOperations();

private:
	void	RemoveAt	(int nIndex, int nCount = 1);
	void	RemoveAll	();

	CFileSystemOperation* 	GetAt		(int nIndex) const;
	CFileSystemOperation* 	GetAt		(const CString& sName);
	int						GetIndex	(const CFileSystemOperation&);

	CFileSystemOperation*&	ElementAt	(int nIndex);
	
	CFileSystemOperation* 	operator[]	(int nIndex) const;
	CFileSystemOperation*&	operator[]	(int nIndex);

public:
	virtual LPCSTR  GetObjectName() const { return "CFileSystemOperations"; }

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
#endif
};


class IFileSystemDriver;
class DataBlob;
// manager for all file system access 
//==============================================================================
class TB_EXPORT IFileSystemManager : public CObject, public CTBLockable
{
	friend class CAboutBox;
	friend class CFileSystemManager;
	friend class CTaskBuilderApp;

private:
	CFileSystemCacher		m_Cacher;
	IFileSystemDriver*		m_pFileSystemDriver;
	IFileSystemDriver*		m_pAlternativeDriver;
	BOOL					m_bFileSystemDriverOwner;
	BOOL					m_bAlternativeDriverOwner;

public:
	IFileSystemManager	(BOOL bCacheEnabled = TRUE);
	IFileSystemManager	(IFileSystemDriver* pFileSystem, IFileSystemDriver* pAlternative = NULL, BOOL bCacheEnabled = TRUE);
	~IFileSystemManager	();

public:
	BOOL					IsManagedByAlternativeDriver	(const CString& sName) const;
	
	virtual LPCSTR			GetObjectName() const { return "IFileSystemManager"; }

private:

	BOOL				LoadCaches	();
	BOOL				AreCachesLoaded				() const;

	// drivers
	BOOL				IsAlternativeDriverEnabled		() const;
	BOOL				IsFileSystemDriverEnabled		() const;
	void				AttachFileSystemDriver			(IFileSystemDriver*, BOOL bReloadCache = TRUE, BOOL bDriverOwner = TRUE);
	void				AttachAlternativeDriver			(IFileSystemDriver*, BOOL bReloadCache = TRUE, BOOL bDriverOwner = TRUE);
	
	IFileSystemDriver*		GetFileSystemDriver	();
	IFileSystemDriver*		GetAlternativeDriver();

public:
	virtual CString		GetServerConnectionConfig	() = 0;
	virtual CString		GetTextFile					(const CString& sFileName) = 0;
	virtual BOOL		SetTextFile					(const CString& sFileName, const CString& sFileContent)  = 0;
	virtual DataBlob	GetBinaryFile				(const CString& sFileName) = 0;
	virtual BOOL		ExistFile					(const CString& sFileName) = 0;
	virtual BOOL		RemoveFile					(const CString& sFileName) = 0;
	virtual BOOL		RenameFile					(const CString& sOldFileName, const CString& sNewName) = 0;
	virtual BOOL		GetFileStatus				(const CString& sFileName, CFileStatus& fs) = 0;
	virtual DWORD		GetFileAttributes			(const CString& sFileName) = 0;
	virtual BOOL		CopyFile					(const CString& sOldFileName, const CString& sNewName, const BOOL& bOverWrite) = 0;

	virtual BOOL		ExistPath					(const CString& sPathName) = 0;
	virtual BOOL		CreateFolder				(const CString& sPathName, const BOOL& bRecursive) = 0;
	virtual BOOL		RemoveFolder				(const CString& sPathName, const BOOL& bRecursive,  const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents = FALSE)  = 0;
	virtual BOOL		CopyFolder					(const CString& sOldPathName, const CString& sNewPathName, const BOOL& bOverwrite, const BOOL& bRecursive)  = 0;
	virtual BOOL		GetSubFolders				(const CString& sPathName, CStringArray* pSubFolders) = 0;
	virtual BOOL		GetPathContent				(const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders,  BOOL bFiles, const CString& sFileExt, CStringArray* pFiles) = 0;
	virtual BOOL		GetFiles					(const CString& sPathName, const CString& sFileExt, CStringArray* pFiles) = 0;

	// utility
	virtual const CString	GetTemporaryBinaryFile		(const CString& sFileName) = 0;

private:

	virtual BOOL	Init	(const CString& strServer, const CString& strInstallation, const CString& strMasterSolutionName) = 0;
	virtual BOOL	Start	(BOOL bLoadCaches  = TRUE);
	virtual BOOL	Stop	(BOOL bClearCaches = TRUE);

#ifdef _DEBUG
	void Dump		(CDumpContext& dc) const;
	void AssertValid() const;
#endif //_DEBUG
};   

// General Functions
//-----------------------------------------------------------------------------
TB_EXPORT IFileSystemManager*	AFXAPI AfxGetFileSystemManager	 ();

#include "endh.dex"
