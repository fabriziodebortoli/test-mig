#pragma once

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\IFileSystemDriver.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\GeneralObjects.h>

//includere alla fine degli include del .H
#include "beginh.dex"


TB_EXPORT extern const TCHAR szTBStandartMetadata[];
TB_EXPORT extern const TCHAR szTBCustomMetadata[];

TB_EXPORT extern const TCHAR szQueryDB[];
TB_EXPORT extern const TCHAR szFetch[];

class TBFile;
class CFileSystemCacher;


///////////////////////////////////////////////////////////////////////////////
//								MetadataPerformanceManager
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT MetadataPerformanceManager : public PerformanceManager
{
public:
	MetadataPerformanceManager();


public:
	virtual void InitCounters();
	virtual void MakeTimeOperation(int nIdx, TimeOperation eTime) { PerformanceManager::MakeTimeOperation(nIdx, eTime); }

public:
	CString GetFormattedQueryTime();
	CString GetFormattedFetchTime();
};


///////////////////////////////////////////////////////////////////////////////
//								TBMetadataManager
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT TBMetadataArray : public ::Array
{

public:
	TBFile* GetAt(int i) { return (TBFile*)Array::GetAt(i); }
	int Add(TBFile* pMetadata) { return Array::Add((CObject*)pMetadata); }
};




//////////////////////////////////////////////////////////////////////////`/////
//								TBFSDatabaseDriver
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT TBFSDatabaseDriver : public IFileSystemDriver
{
public:
	TBFSDatabaseDriver(const CString& strStandardConnectionString, const CString& sTestCustomConnectionString);
	~TBFSDatabaseDriver();

private:
	TBFile* m_pCachedTBFile;
	MetadataPerformanceManager* m_pMetadataPerformance;

public:
	CString m_StandardConnectionString;		//connessione al database contenente i dati della standard
	CString m_TestCustomConnectionString;	//connessione al database per il test in caso di mancata connessione

private:
	void	MakeTimeOperation(int nIdx, TimeOperation eTime) { if (m_pMetadataPerformance) m_pMetadataPerformance->MakeTimeOperation(nIdx, eTime); }

	CString GetCustomConnectionString() const;
	int		GetFolder(const CString& strPathName,const BOOL& bCreate);
	int		GetFile(const CString& strFilePathName);
	void	GetStandardTBFileInfo(const CString& whereClause, TBMetadataArray* pArray);
	void	GetCustomTBFileInfo(const CString& whereClause, TBMetadataArray* pArray);

public:
	TBFile* GetTBFile(const CString& strPathFileName);
	BOOL	SaveTBFile(TBFile* pTBFile, const BOOL& bOverWrite);
	BOOL	CopyTBFile(TBFile* pTBOldFileInfo, const CString& strNewName, const BOOL& bOverWrite);
	BOOL	GetTBFolderContent(const CString& strPathFileName, TBMetadataArray* pFolderContent, BOOL bFolders, BOOL bFiles, const CString& strFileExt);

	CString GetFormattedQueryTime() { return (m_pMetadataPerformance) ? m_pMetadataPerformance->GetFormattedQueryTime() : _T(""); }
	CString GetFormattedFetchTime() { return (m_pMetadataPerformance) ? m_pMetadataPerformance->GetFormattedFetchTime() : _T(""); }

public:
	virtual BOOL		IsAManagedObject(const CString& sFileName) const;
	virtual CString		GetDriverDescription() const { return _T("TBFS Database Driver"); }

	virtual CString		GetServerConnectionConfig();
	virtual void		GetAllApplicationInfo(CStringArray*  pAppsPath);
	virtual void		GetAllModuleInfo(const CString& strAppName, CStringArray* pModulesPath);

	virtual CString 	GetTextFile(const CString& sFileName);
	virtual BOOL		SaveTextFile(const CString& sFileName, const CString& sFileContent);
	virtual BYTE*		GetBinaryFile(const CString& sFileName, int& nLen);
	virtual BOOL 		SaveBinaryFile(const CString& sFileName, BYTE* pBinaryContent, int nLen);


	virtual BOOL		ExistFile(const CString& sFileName);
	virtual BOOL		RemoveFile(const CString& sFileName);
	virtual BOOL		RenameFile(const CString& sOldFileName, const CString& sNewName);
	virtual BOOL		GetFileStatus(const CString& sFileName, CFileStatus& fs);
	virtual DWORD		GetFileAttributes(const CString& sFileName);
	virtual BOOL		CopySingleFile(const CString& sOldFileName, const CString& sNewName, const BOOL& bOverWrite);

	virtual BOOL		ExistPath(const CString& sPathName);
	virtual BOOL		CreateFolder(const CString& sPathName, const BOOL& bRecursive);
	virtual BOOL		RemoveFolder(const CString& sPathName, const BOOL& bRecursive, const BOOL& bRemoveRoot, const BOOL& bAndEmptyParents = FALSE);
	virtual BOOL		CopyFolder(const CString& sOldPathName, const CString& sNewPathName, const BOOL& bOverwrite, const BOOL& bRecursive);
	virtual BOOL		GetSubFolders(const CString& sPathName, CStringArray* pSubFolders);
	virtual BOOL		GetPathContent(const CString& sPathName, BOOL bFolders, CStringArray* pSubFolders, BOOL bFiles, const CString& sFileExt, CStringArray* pFiles);
	virtual BOOL		GetFiles(const CString& sPathName, const CString& sFileExt, CStringArray* pFiles);

#ifdef _DEBUG
	void Dump(CDumpContext& dc) const;
	void AssertValid() const;
#endif //_DEBUG
};


#include "endh.dex"

