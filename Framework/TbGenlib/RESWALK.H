
#pragma once

#include "TBStrings.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//==============================================================================
//			Class CWalkBitmap definition
//==============================================================================
class TB_EXPORT CWalkBitmap : public CBitmap
{
	DECLARE_DYNAMIC(CWalkBitmap)

protected:
	BOOL		m_bDIB;
	CPalette*	m_pDIBPalette;
	CDC*		m_pDC;
	HBITMAP		m_hBitmap;

public:
	CWalkBitmap(BOOL bDIB = FALSE, CDC* pDC = NULL);
	virtual ~CWalkBitmap();
	
	BOOL LoadBitmap(LPCTSTR lpszResourceName);
	BOOL LoadBitmap(UINT nIDResource);

	BOOL LoadBmpOrPng(CString strImageNS);
	BOOL LoadBmpOrPng(UINT nIDResource);

	HBITMAP		LoadDIBitmap(HINSTANCE,LPCTSTR);
	int			CreateDIBPalette (LPBITMAPINFOHEADER);
	CPalette*	GetDIBPalette ();
};


//==============================================================================
//			Class CWalkMen definition
//==============================================================================
class TB_EXPORT CWalkMenu : public CLocalizableMenu
{
	DECLARE_DYNAMIC(CWalkMenu)

public:
	BOOL LoadMenu(LPCTSTR lpszResourceName);
	BOOL LoadMenu(UINT nIDResource);
};


inline HCURSOR LoadWalkCursor(LPCTSTR lpszResourceName)
{ 
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(lpszResourceName, RT_GROUP_CURSOR);
#else
	HINSTANCE hInst = AfxGetResourceHandle();
#endif
	return ::LoadCursor(hInst, lpszResourceName); 
}   

inline HCURSOR LoadWalkCursor(UINT nIDResource)
{ 
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nIDResource), RT_GROUP_CURSOR);
#else
	HINSTANCE hInst = AfxGetResourceHandle();
#endif
	return ::LoadCursor(hInst, MAKEINTRESOURCE(nIDResource)); 
}

inline HICON LoadWalkIcon(LPCTSTR lpszResourceName)
{ 
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(lpszResourceName, RT_GROUP_ICON);
#else
	HINSTANCE hInst = AfxGetResourceHandle();
#endif
	return ::LoadIcon(hInst, lpszResourceName); 
}

inline HICON LoadWalkIcon(UINT nIDResource)
{ 
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nIDResource), RT_GROUP_ICON);
#else
	HINSTANCE hInst = AfxGetResourceHandle();
#endif
	return ::LoadIcon(hInst, MAKEINTRESOURCE(nIDResource)); 
}

#include "endh.dex"
