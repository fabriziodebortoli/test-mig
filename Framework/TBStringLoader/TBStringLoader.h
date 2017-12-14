
#pragma once

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "demoview.h"	

#include "beginh.dex"


// CTBStringLoaderApp
// See TBStringLoader.cpp for the implementation of this class
//


class CStringLoader;

class CTBStringLoaderApp : public CWinApp
{
	CStringLoader	*m_pLoader;
	
	CObArray	m_WindowsArray;
	CPtrArray	m_ModulesArray;
	
public:
	CFont			m_FormFont;
	CString			m_strWindowClassName;

	CStringLoader* GetGlobalStringLoader();
	void AddWindow(CObject *pWnd);
	void RemoveWindow(CObject *pWnd);
	CDemoFrame* GetWndFromIDD(UINT nIDD);

	void AddModule(HMODULE hMod);
	BOOL ExistsModule(HMODULE hMod);

	void FreeCache();
	void SetFormFont();

	CTBStringLoaderApp();
	~CTBStringLoaderApp();

// Overrides
public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();

protected:
	void InitFont();

	DECLARE_MESSAGE_MAP()

};

/////////////////////////////////////////////////////////////////////////////
//EXPORTED GLOBAL FUNCTIONS
/////////////////////////////////////////////////////////////////////////////

extern "C" TB_EXPORT BOOL ExistDialog(LPCTSTR lpcszModule, UINT nIDD);
extern "C" TB_EXPORT BOOL ShowDemoDialog(HWND hParent, LPCTSTR lpcszXML, LPCTSTR lpcszModule, UINT nIDD);
extern "C" TB_EXPORT BOOL CloseDemoDialog(UINT nIDD);

extern "C" TB_EXPORT void FreeCache();
extern "C" TB_EXPORT void SetFormFont();


//***********************************************************************************************
// ATTENZIONE: il puntatore restituito da queste funzioni deve essere rilasciato con una chiamata 
// a CoTaskMemFree, a meno che la funzione non sia chiamata via InteropServices da .net
extern "C" TB_EXPORT LPCTSTR LoadXMLString(LPCTSTR lpcstrBaseString, LPCTSTR lpcstrFileName, LPCTSTR lpcstrDictionaryPath);
extern "C" TB_EXPORT LPCTSTR CheckDialog(LPCTSTR lpcszModule, UINT nIDD, float fRatio, LPCTSTR lpcszDictionaryPath);
//***********************************************************************************************

#include "endh.dex"

