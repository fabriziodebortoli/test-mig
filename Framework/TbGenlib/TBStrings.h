
#pragma once

#include <TbGeneric\TBStrings.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CBaseDocument;

class TB_EXPORT CDictionaryPathFinder : public CDictionaryPathFinderObj
{
	virtual void	GetDictionaryPathsFromString(LPCTSTR lpcszString, CStringArray &paths);
	virtual void	GetDictionaryPathsFromID(UINT nIDD, LPCTSTR lpszType, CStringArray &paths);
	virtual CString	GetDictionaryPathFromNamespace(const CTBNamespace &aNamespace, BOOL bStandard);
	virtual CString	GetDictionaryPathFromTableName(const CString& strTableName);
	virtual CString	GetDllNameFromNamespace(const CTBNamespace& aNamespace);
	
	void GetDictionaryPathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths);
	void GetModulePathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths);
};

TB_EXPORT CString		AFXAPI AfxLoadReportString(LPCTSTR lpcszBaseString, CBaseDocument* pDoc);
TB_EXPORT CString		AFXAPI AfxLoadReportString(LPCTSTR lpcszBaseString, LPCTSTR lpcstrFileName, LPCTSTR lpcstrDictionaryPath);
TB_EXPORT CString		AFXAPI AfxLoadReportString(LPCTSTR lpcszBaseString, LPCTSTR lpcstrReportPath);

TB_EXPORT CString		AfxLoadDatabaseString(LPCTSTR lpcszBaseString, LPCTSTR lpszTableName);

#include "endh.dex"
