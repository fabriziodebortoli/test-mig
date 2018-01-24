
#pragma once
#include "BeginH.dex"

#define UNC_SLASH_CHARS	_T("\\/")

const GUID NULL_GUID = {0x0, 0x0, 0x0, {0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0}};

#define NULL_SZGUID	_T("{00000000-0000-0000-0000-000000000000}")

//class TBFile;
// File management
//=============================================================================
TB_EXPORT BOOL		IsDosName		(const CString& name, BOOL bCheckFullPath = FALSE);
TB_EXPORT BOOL		IsValidObjName	(const CString& name, BOOL bCheckFullPath = FALSE);
TB_EXPORT BOOL		IsServerPath	(const CString& name);
TB_EXPORT BOOL		IsDriveName		(const CString& name);
TB_EXPORT BOOL		IsDirSeparator	(TCHAR ch);
TB_EXPORT BOOL		IsDirSeparator	(LPCTSTR s);
TB_EXPORT BOOL		IsFileName		(const CString& name, BOOL bCheckExist = TRUE, BOOL bCheckFullPath = FALSE);
TB_EXPORT BOOL		IsRelativePath	(const CString& name);
TB_EXPORT BOOL		IsPathName		(const CString& name, BOOL bCheckExist = TRUE);

//------------------------------------------------------------------------------
TB_EXPORT CString		GetDriver	(const CString& strName);
TB_EXPORT CString		GetPath		(const CString& name, BOOL bAppendBkSlash = FALSE);
TB_EXPORT CString		GetName		(const CString& name);
TB_EXPORT CString		GetExtension(const CString& name);
TB_EXPORT SYSTEMTIME	GetFileDate (const CString& sFileName, BOOL bLastWrite = TRUE);
TB_EXPORT CString		GetTempName	(LPCTSTR pszPrefix = NULL, BOOL bTempPath = TRUE);
TB_EXPORT CString		RemoveExtension (const CString& strName);

TB_EXPORT BOOL			GetStatus			(const CString& sFileName, CFileStatus& fs);
TB_EXPORT CString		GetNameWithExtension(const CString& name);
TB_EXPORT DWORD			GetTbFileAttributes (LPCTSTR  name);
TB_EXPORT LONG			GetFileSize			(LPCTSTR  name);

//------------------------------------------------------------------------------
TB_EXPORT BOOL		ExistPath	(const CString& name);
TB_EXPORT BOOL		ExistFile	(const CString& name);
TB_EXPORT BOOL		DeleteFile	(const CString& name);
TB_EXPORT BOOL		RemoveFile	(const CString& name);
//TB_EXPORT TBFile* GetTBFile(const CString& name);

//------------------------------------------------------------------------------
TB_EXPORT CString	AppendExtensionIfNeeded(const CString& name, const CString& ext);
TB_EXPORT CString	MakeFilePath(const CString& path, const CString& fileName, const CString& ext = _T(""));
TB_EXPORT CString	MakeName	(const CString& file, const CString& extent);
TB_EXPORT CString	MakeName	(const CString& file, UINT ids);

//------------------------------------------------------------------------------
TB_EXPORT BOOL		RemoveFolderTree		(const CString& path, BOOL bRemoveRoot =TRUE);
TB_EXPORT BOOL		RemoveEmptyParentFolders(const CString& path);
TB_EXPORT BOOL		RecursiveCreateFolders	(const CString& path);
TB_EXPORT BOOL		CreateDirectory			(const CString& path);
TB_EXPORT void	    GetSubFolders			(const CString sHomeDir, CStringArray* pSubFolders);
TB_EXPORT void		GetFiles				(const CString& sPathName, const CString& sFileExt, CStringArray* pFiles);

//=============================================================================
TB_EXPORT CString			GetComputerName				(BOOL bStripSpecial = TRUE);
TB_EXPORT CString			GetProcessFileName			();
TB_EXPORT CString			GetUserName					(BOOL bStripSpecial = TRUE);
TB_EXPORT BOOL				IsOSWindows2K				();
TB_EXPORT BOOL				IsOSWindowsXp				();
TB_EXPORT BOOL				IsOSWinXpOrGreater			();
TB_EXPORT BOOL				IsOSVista					();
TB_EXPORT BOOL				IsOSVistaOrGreater			();
TB_EXPORT BOOL				IsOSWin7					();
TB_EXPORT BOOL				IsWindowsVersion			(const int& wMajor, const int& wMinor);
TB_EXPORT BOOL				IsWindowsVersionGreaterThan	(const int& wMajor, const int& wMinor);
TB_EXPORT BOOL				GetOSDisplayString			(CString& strOS);
TB_EXPORT BOOL				IsMutexPresent				(const CString& mutexName);
TB_EXPORT BOOL				IsWorkStation				();

typedef void (__stdcall *ATTACHEVENT_FUNC) (const CString& path);
TB_EXPORT void	AfxAttachCustomizationContextPointer	(ATTACHEVENT_FUNC value);
TB_EXPORT void	AfxAddFileToCustomizationContext		(const CString& path);

#include "EndH.dex"
