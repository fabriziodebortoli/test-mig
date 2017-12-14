
#pragma once

#include <TBXMLCore\XSLTDocObj.h>

#include <TBNameSolver\TBNamespaces.h>
#include <TbNameSolver\PathFinder.h>
#include <TbStringLoader\StringLoader.h>


#include "DllMod.h"

#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='x86' publicKeyToken='6595b64144ccf1df' language='*'\"")

//includere alla fine degli include del .H
#include "beginh.dex"


class CXMLDocumentObject; 
class CWndObjDescription;

TB_EXPORT CStringLoader*			AFXAPI AfxGetStringLoader();

TB_EXPORT CString					AFXAPI AfxGetCulture();

TB_EXPORT BOOL						AFXAPI AfxIsChineseCulture();
TB_EXPORT BOOL						AFXAPI AfxIsEnglishCulture();
TB_EXPORT BOOL						AFXAPI AfxIsEnglishCulture(const CString& strCulture);

TB_EXPORT void						AFXAPI AfxGetDictionaryPathsFromSourceString(LPCTSTR lpcszString, CStringArray& paths);
TB_EXPORT void						AFXAPI AfxGetDictionaryPathsFromID(UINT nIDD, LPCTSTR lpszType, CStringArray &paths);
TB_EXPORT void						AFXAPI AfxGetDictionaryPathsFormDllInstance(HINSTANCE hDllInstance, CStringArray &paths);
TB_EXPORT CString					AFXAPI AfxGetDictionaryPathFromNamespace(const CTBNamespace& aNamespace, BOOL bStandard);
TB_EXPORT CString					AFXAPI AfxGetDictionaryPathFromTableName(const CString& strTableName);

TB_EXPORT CString					AFXAPI AfxLoadSourceString(LPCTSTR lpcszBaseString, LPCSTR lpszFile);

TB_EXPORT CString					AFXAPI AfxLoadEnumString(LPCTSTR lpcszBaseString, LPCTSTR lpszName, const CTBNamespace &aNamespace);
TB_EXPORT CString					AFXAPI AfxLoadFormatterString(LPCTSTR lpcszBaseString, const CTBNamespace &aNamespace);
TB_EXPORT CString					AFXAPI AfxLoadFontString(LPCTSTR lpcszBaseString, const CTBNamespace &aNamespace);

TB_EXPORT CString					AFXAPI AfxLoadXMLString(LPCTSTR lpcszBaseString, LPCTSTR lpszFileName, LPCTSTR lpszictionaryPath);

TB_EXPORT CString					AFXAPI AfxLoadTBString(UINT	IdString, HINSTANCE hinstance = NULL);
TB_EXPORT BOOL						AFXAPI AfxLoadTBString(CString&	strToLoad, UINT	IdString, HINSTANCE hinstance = NULL);

TB_EXPORT void						AFXAPI AfxLoadWindowStrings(CWnd* pWnd, LPCTSTR lpcszTemplate);
TB_EXPORT CString					AFXAPI AfxLoadJsonString(LPCTSTR lpcszBaseString, CWndObjDescription* pDescription);
TB_EXPORT void						AFXAPI AfxLoadMenuStrings(CMenu* pMenu, UINT nIDDREsource);

typedef CString  (*LOADDATABASESTRINGS) (LPCTSTR lpcszBaseString, LPCTSTR lpszTableName);
TB_EXPORT BOOL AfxInitLoadDatabaseStringFunction(LOADDATABASESTRINGS pFunction);

CString	AfxBaseLoadDatabaseString(LPCTSTR lpcszBaseString, LPCTSTR lpszTableName);

// BEGIN_TBLOCALIZER_SKIP 
// questa sezione di codice non deve essere parsata dal TBLocalizer

#define _TB(string)							AfxLoadSourceString(_T(string), __FILE__)

#define BEGIN_TB_STRING_MAP(className)     class TB_EXPORT className { public:	

//per le stringhe da localizzare
#define TB_LOCALIZED(varName, value)     	inline static const CString varName() { return _TB(value);}

//per le stringhe non soggette a traduzione
#define TB_STANDARD(varName, value)     	inline static const CString varName() { return _T(value);}

//per le stringhe non "wrappate" automaticamente da _T o _TB (la decisione è laciata all'utilizzatore)
#define TB_GENERIC(varName, value)     	inline static const CString varName() { return value; }

#define END_TB_STRING_MAP()     			};	

//	END_TBLOCALIZER_SKIP

#define REVERSEMAKEINTRESOURCE( templ ) ((USHORT) (ULONG_PTR) templ)
	

//*****************************************************************************
// CLocalizableXMLDocument 
//*****************************************************************************

class TB_EXPORT CLocalizableXMLDocument : public CXMLDocumentObject
{
	CTBNamespace m_Namespace;
	CPathFinder	*m_pPathFinder;

	CString GetDictionaryPath();

public:
	CLocalizableXMLDocument(const CTBNamespace& moduleNamespace, CPathFinder* pPathFinder);
	virtual BOOL		LoadXMLFile				(const CString&);
	virtual BOOL		SaveXMLFile				(const CString&, BOOL = FALSE);

	BOOL	GetLocalizableText(CXMLNode *pNode, CString &strText);
	BOOL	SetLocalizableText(CXMLNode *pNode, LPCTSTR lpszText, LPCTSTR lpszDictionary = NULL);

	BOOL	GetLocalizableAttribute(CXMLNode *pNode, CString &strText);
	BOOL	SetLocalizableAttribute(CXMLNode *pNode, LPCTSTR lpszText);

};

//*****************************************************************************
// CLocalizableXMLNode 
//*****************************************************************************

class TB_EXPORT CLocalizableXMLNode : public CXMLNode 
{
public:
	BOOL	GetLocalizableText(CString &strText);
	BOOL	SetLocalizableText(LPCTSTR lpszText, LPCTSTR lpszDictionary = NULL);
	BOOL	GetLocalizableAttribute(CString &strText);
	BOOL	SetLocalizableAttribute(LPCTSTR lpszText);

};

#include "endh.dex"
