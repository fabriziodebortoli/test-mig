#pragma once

//includere alla fine degli include del .H
#include <io.h>

#include "beginh.dex"

class CFileSystemCacher;
class DataBlob;
// interface to all file system access
//==============================================================================
class TB_EXPORT IFileSystemDriver : public CObject
{
	friend class IFileSystemManager;
	friend class CFileSystemManager;
	friend class CFileSystemDriver;
	friend class CFileSystemManagerWebService;

private:
	BOOL m_bStarted;

public:
	IFileSystemDriver	();

public:
	virtual CString		GetDriverDescription		() const = 0;

private:
	virtual BOOL		IsAManagedObject			(const CString& sFileName) const = 0;
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

	virtual BOOL		Start						();
	virtual BOOL		Stop						();
	virtual BOOL		IsStarted					() const;
	virtual BOOL		CanCache					() const = 0;
	virtual BOOL		LoadCache					(CFileSystemCacher* aCacher) = 0;
};

#include "endh.dex"
