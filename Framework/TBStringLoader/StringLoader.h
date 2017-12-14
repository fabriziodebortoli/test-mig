
#pragma once

#define DIALOG_TYPE			_T("dialog") 
#define JSON_FORMS_TYPE		_T("jsonforms") 
#define STRINGTABLE_TYPE	_T("stringtable")
#define MENU_TYPE			_T("menu")
#define STRING_TYPE			_T("string")
#define FILE_TYPE			_T("source")
#define ENUM_TYPE			_T("enum")
#define FORMATTER_TYPE		_T("format")
#define FONT_TYPE			_T("font")
#define XML_TYPE			_T("xml")
#define DATABASE_TYPE		_T("script")
#define REPORT_TYPE			_T("report")

#include "DictionaryClasses.h"	

#include <afx.h>
#include <afxmt.h>

#include "beginh.dex"

//===========================================================================
// CModuleStrings
//===========================================================================

class CModuleStrings : public CObject
{

	DECLARE_SERIAL(CModuleStrings)
	
	::CCriticalSection			m_CriticalSection;
	CUsedStringBlockContainer	m_Dictionary;

public:
	CString						m_strDictionaryPath;
	CModuleStrings() {}
	CModuleStrings(const CString &strDictionaryPath);
	virtual ~CModuleStrings();


	CStringBlock*			GetDialogStrings(CStringLoader *pLoader, UINT nIDD);
	CStringBlock*			GetJsonFormStrings(CStringLoader *pLoader, const CString& sId, const CString& sName);
	CStringBlock*			GetStringTableStrings(CStringLoader *pLoader, UINT nIDD);
	CStringBlock*			GetMenuStrings(CStringLoader *pLoader, UINT nIDD);
	CStringBlock*			GetSourceStrings(CStringLoader *pLoader, const CString &strFileName);
	CStringBlock*			GetEnumStrings(CStringLoader *pLoader, const CString &strTagName);
	CStringBlock*			GetFormatterStrings(CStringLoader *pLoader);
	CStringBlock*			GetFontStrings(CStringLoader *pLoader);
	CStringBlock*			GetXMLStrings(CStringLoader *pLoader, const CString &strFileName);
	CStringBlock*			GetDatabaseStrings(CStringLoader *pLoader, const CString &strTableName);
	CStringBlock*			GetReportStrings(CStringLoader *pLoader, const CString &strFileName);

	void FreeStrings();
	
	void Serialize(CArchive& ar);

public:
	CString AdjustName(const CString& strFileName);
};



//===========================================================================
// CStringLoader
//===========================================================================

class TB_EXPORT CStringLoader : public CObject
{
	friend		class CDemoDialog;
	
	::CCriticalSection					m_CriticalSection;

	CString								m_strLocalCacheId;
	CString								m_strCacheFilePath;
	CSerializableArray<CModuleStrings>	m_Modules;
	CObArray							m_Fonts;
	CDictionaryBinaryFileLRU			m_BinaryFileLRU;
	CStringLoader(const CString &sCachePath);

public:
	CStringLoader(const CString& strLocalCacheId, const CString &sCachePath);
	virtual ~CStringLoader();

	// pulisce la struttura dati (corrisponde ad un 'delete this')
	void FreeCache();
	void FreeModules();

	CStringBlockContainer* GetDictionary(const CString& strFilePath);
	
	BOOL LoadWindowStrings(CWnd* pWnd, const CStringArray& strDictionaryPaths, UINT IDD);
	BOOL LoadJsonFormString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrId, LPCTSTR lpcstrName, LPCTSTR lpcstrDictionaryPath, void*& lpPrivateData, CString& sTarget);
	BOOL LoadMenuStrings(CMenu* pMenu, const CStringArray& strDictionaryPaths, UINT nIDD);
	CString LoadResourceString(LPCTSTR lpcstrBaseString, UINT IDD, const CStringArray& strDictionaryPaths);
	
	CString LoadSourceString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrFileName, const CStringArray& strDictionaryPaths);
	CString LoadEnumString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrTagName, LPCTSTR lpcstrDictionaryPath);
	CString LoadFormatterString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrDictionaryPath);
	CString LoadFontString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrDictionaryPath);
	CString LoadReportString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrFileName, LPCTSTR lpcstrDictionaryPath);
	CString LoadDatabaseString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrTableName, LPCTSTR lpcstrDictionaryPath);

	CString LoadXMLString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrFileName, LPCTSTR lpcstrDictionaryPath);
	void ClearCache();
	
private:
	void LoadCache(const CString &sCachePath);
	void SaveCache();
	void InitCacheFilePath(const CString &sCachePath);
	BOOL LoadMenuStrings(CMenu* pMenu, CStringBlock *pBlock);
	BOOL LoadWindowStrings(CWnd* pWnd, CStringBlock *pBlock, BOOL bDemo);

	CStringItem* FindStringItem(LPCTSTR lpcstrBaseString, CStringBlock* pStringBlock, BOOL bEscape);
	
	BOOL FindString(LPCTSTR lpcstrBaseString, CStringBlock* pStringBlock, CString &strTarget);
	
	BOOL ReplaceString(CWnd* pwndChild, CStringBlock* pStringBlock);
	CFont* CStringLoader::GetFont(const LOGFONT &lf);
	CModuleStrings* GetModule(const CString &strDictionaryPath);
	
	CStringBlock* 			GetMenuStrings(const CStringArray &strRootPaths, UINT nIDD);
	CStringBlock* 			GetDialogStrings(const CStringArray &strRootPaths, UINT nIDD);
	CStringBlock* 			GetJsonFormStrings(const CString &strRootPath, const CString& sId, const CString& sName);
	CStringBlock* 			GetStringTableStrings(const CStringArray &strRootPaths, UINT nIDD);
	
	CStringBlock* 			GetSourceStrings(const CStringArray &strRootPaths, const CString &strFileName);
	CStringBlock* 			GetEnumStrings(const CString &strRootPath, const CString &strTagName);
	CStringBlock* 			GetFormatterStrings(const CString &strRootPath);
	CStringBlock* 			GetFontStrings(const CString &strRootPath);
	CStringBlock* 			GetXMLStrings(const CString &strRootPath, const CString &strFileName);
	CStringBlock* 			GetDatabaseStrings(const CString &strRootPath, const CString &strTableName);
	CStringBlock*			GetReportStrings(const CString &strRootPath, const CString &strFileName);

	void	FreeStrings(const CString &strRootPath, const CString& strType, const CString& strFileName);
};

#include "endh.dex"