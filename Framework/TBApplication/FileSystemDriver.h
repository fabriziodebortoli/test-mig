#pragma once

#include <TbNameSolver/IFileSystemDriver.h>
//#include <TbGeneric/LineFile.h>

class TBFile;
//==============================================================================
class CFileSystemDriver : public IFileSystemDriver
{
	friend class CFileSystemManager;
	friend class IFileSystemManager;

public:
	CFileSystemDriver	();

private:
	BOOL	RemoveChildsFolders			(const CString& sPathName);
	BOOL	RemoveParentFolders			(const CString& sPathName);
	void	AddApplicationDirectories	(const CString& sAppContainerPath, CStringArray* pAppsPath);
	void	AddApplicationModules		(const CString& sApplicationPath, CStringArray* pModulesPath, bool isCustom);
public:
	virtual CString		GetDriverDescription		() const;

private:
	virtual BOOL		IsAManagedObject	(const CString& sFileName) const;

	virtual CString		GetServerConnectionConfig	();
	virtual void		GetAllApplicationInfo		(CStringArray*  pAppsPath);
	virtual void		GetAllModuleInfo			(const CString& strAppName, CStringArray* pModulesPath);
	virtual CString 	GetTextFile					(const CString& sFileName);
	virtual BOOL		SaveTextFile				(const CString& sFileName, const CString& sFileContent);
	virtual BYTE*		GetBinaryFile				(const CString& sFileName, int& nLen);
	virtual BOOL 		SaveBinaryFile				(const CString& sFileName, BYTE* pBinaryContent, int nLen);
	virtual BOOL		ExistFile					(const CString& sFileName);
	virtual BOOL		RemoveFile					(const CString& sFileName);
	virtual BOOL		RenameFile					(const CString& sOldFileName, const CString& sNewName);
	virtual BOOL		GetFileStatus				(const CString& sFileName, CFileStatus& fs);
	virtual DWORD		GetFileAttributes			(const CString& sFileName);
	virtual BOOL		CopySingleFile				(const CString& sOldFileName, const CString& sNewName, const BOOL& bOverWrite);

	virtual BOOL		ExistPath					(const CString& sPathName);
	virtual BOOL		CreateFolder				(const CString& sPathName, const BOOL& bRecursive);
	virtual BOOL		RemoveFolder				(const CString& sPathName, const BOOL& bRecursive,  const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents = FALSE);
	virtual BOOL		CopyFolder					(const CString& sOldPathName, const CString& sNewPathName, const BOOL& bOverwrite, const BOOL& bRecursive);
	virtual BOOL		GetSubFolders				(const CString& sPathName, CStringArray* pSubFolders);
	virtual BOOL		GetPathContent				(const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders,  BOOL bFiles, const CString& sFileExt, CStringArray* pFiles);
	virtual BOOL		GetFiles					(const CString& sPathName, const CString& sFileExt, CStringArray* pFiles);

	//virtual BOOL		CanCache			() const;
	//virtual BOOL		LoadCache			(CFileSystemCacher* pCacher);

#ifdef _DEBUG
	void Dump		(CDumpContext& dc) const;
	void AssertValid() const;
#endif //_DEBUG

};   
