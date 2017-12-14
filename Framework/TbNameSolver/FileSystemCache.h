#pragma once

#include "TBResourceLocker.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//==============================================================================
class CFileSystemCachedItem : public CObject
{
	friend class CFileSystemCache;
	friend class CFileSystemCacher;

private:
	CString		m_sPathName;
	CString		m_sFileName;

public:
	CFileSystemCachedItem	();
	CFileSystemCachedItem	(
								const CString& sPathName, 
								const CString& sFileName = _T("")
							);

private:
	const CString&	GetPathName			() const;
	const CString&	GetFileName			() const;
	const CString	GetFullName			() const;
	const BOOL		IsAFolder			() const;

	void Set	(const CString& sPathName, const CString& sFileName);

#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
#endif
};

//=============================================================================
class CFileSystemCache : public CObject, public CTBLockable
{
	friend class IFileSystemManager;
	friend class CFileSystemManager;
	friend class CFileSystemCacher;

public:
	CFileSystemCache();
	CFileSystemCache(const CString& sCachedPath);

	virtual ~CFileSystemCache();

private:
	CMapStringToOb	m_Files;
	CMapStringToOb	m_Folders;
	CString			m_sCachedPath;
	CString			m_sRemovedPath;

private:
	void			RemoveAll		();
	void			Add				(CFileSystemCachedItem*);
	
	const CString	RemoveCachedPath(const CString& sFullName) const;
	const BOOL		IsCachedPath	(const CString& sFullName) const;

	// inspection methods
	BOOL	ExistPath		(const CString& sPathName);
	BOOL	ExistFile		(const CString& sPathName, const CString& sFileName);
	BOOL	GetSubFolders	(const CString& sPathName, CStringArray* pSubFolders);
	BOOL	GetFiles		(const CString& sPathName, const CString& sFileExt, CStringArray* pSubFolders);
	BOOL	RemoveFile		(const CString& sFileName);
	BOOL	CreateFolder	(const CString& sPathName, const BOOL& bRecursive);
	BOOL	RemoveFolder	(const CString& sPathName, const BOOL& bRecursive, const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents);

public:
	virtual LPCSTR  GetObjectName() const { return "CFileSystemCache"; }

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
#endif
};

//=============================================================================
class TB_EXPORT CFileSystemCacher : public CObject, public CTBLockable
{
	friend class IFileSystemManager;
	friend class CFileSystemManager;
	friend class CFileSystemCacheContent;

public:
	CFileSystemCacher();
	~CFileSystemCacher();

private:
	BOOL		m_bEnabled;
	CObArray	m_Caches;

private:

	CFileSystemCache*	GetCacheInvolved (const CString& sName) const;

	// reading caches
	BOOL	IsAManagedObject(const CString& sName) const;
	BOOL	AreCachesLoaded	() const;
	BOOL	IsEnabled		() const;

	BOOL	ExistPath		(const CString& sPathName);
	BOOL	ExistFile		(const CString& sFileName);
	BOOL	GetSubFolders	(const CString& sPathName, CStringArray* pSubFolders);
	BOOL	GetFiles		(const CString& sPathName, const CString& sFileExt, CStringArray* pFiles);
	BOOL	RemoveFile		(const CString& sFileName);
	BOOL	RenameFile		(const CString& sOldFileName, const CString& sNewFileName);
	BOOL	CreateFolder	(const CString& sPathName, const BOOL& bRecursive);
	BOOL	RemoveFolder	(const CString& sPathName, const BOOL& bRecursive, const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents);

	void	ClearCaches		();
	BOOL	AddInCache		(const CString& sPathName, const CString& sFileName);
	BOOL	AddInCache		(const CString& sPathName);
	void	AddCache		(const CString& sPathName);

public:
	virtual LPCSTR  GetObjectName() const { return "CFileSystemCacher"; }

#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
#endif
};

#include "endh.dex"