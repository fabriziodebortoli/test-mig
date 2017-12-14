
#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\LocalizableObjs.h>
#include "reswalk.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//==============================================================================
//			Class CWalkBitmap implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CWalkBitmap, CBitmap)

//-----------------------------------------------------------------------------
CWalkBitmap::CWalkBitmap(BOOL bDIB /*= FALSE*/, CDC* pDC /*= NULL*/)
	:
	m_bDIB			(bDIB),
	m_pDIBPalette	(NULL),
	m_pDC			(pDC),
	m_hBitmap		(NULL)
{
}

//-----------------------------------------------------------------------------
CWalkBitmap::~CWalkBitmap()
{
	if (m_hBitmap != NULL)
	{
		Detach();
		::DeleteObject(m_hBitmap);
	}

	if (m_pDIBPalette)
	{
		m_pDIBPalette->DeleteObject();
		delete m_pDIBPalette;
	}
}

//-----------------------------------------------------------------------------
BOOL CWalkBitmap::LoadBitmap(LPCTSTR lpszResourceName)
{
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(lpszResourceName, RT_BITMAP);
	if (m_bDIB)
	{
		m_hBitmap = LoadDIBitmap(hInst, lpszResourceName);
		return Attach(m_hBitmap);
	}
		
	m_hBitmap = ::LoadBitmap(hInst, lpszResourceName);
	return Attach(m_hBitmap);
#else
	return CBitmap::LoadBitmap(lpszResourceName);
#endif
}

//-----------------------------------------------------------------------------
BOOL CWalkBitmap::LoadBmpOrPng(CString strImageNS)
{
	TBThemeManager* pThemeManager = AfxGetThemeManager();
	HBITMAP hBmp;
	if (pThemeManager)
	{
		if (IsScale())
		{
			hBmp = LoadBitmapOrPng(strImageNS, pThemeManager->IsToAddMoreColor());
			BITMAP bmInfo;
			::GetObject(hBmp, sizeof(bmInfo), &bmInfo);
			LONG newSizeW = ScalePix(bmInfo.bmWidth);
			LONG newSizeH = ScalePix(bmInfo.bmHeight);
			m_hBitmap = ScaleBitmapInt(hBmp, newSizeW, newSizeH);
		}
		else
		{
			m_hBitmap = LoadBitmapOrPng(strImageNS, pThemeManager->IsToAddMoreColor());
		}
	}

	return Attach(m_hBitmap);
}

//-----------------------------------------------------------------------------
BOOL CWalkBitmap::LoadBmpOrPng(UINT nIDResource)
{
	m_hBitmap = LoadBitmapOrPng(nIDResource);
	return Attach(m_hBitmap);
}

//-----------------------------------------------------------------------------
BOOL CWalkBitmap::LoadBitmap(UINT nIDResource)
{
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nIDResource), RT_BITMAP);
	if (m_bDIB)
	{
		m_hBitmap = LoadDIBitmap(hInst, MAKEINTRESOURCE(nIDResource));
		return Attach(m_hBitmap);
	}

	m_hBitmap = ::LoadBitmap(hInst, MAKEINTRESOURCE(nIDResource));
	return Attach(m_hBitmap);
#else
	return CBitmap::LoadBitmap(nIDResource);
#endif
}

//-----------------------------------------------------------------------------
HBITMAP CWalkBitmap::LoadDIBitmap(HINSTANCE hInst,LPCTSTR lpszResourceName)
{
	if (!m_bDIB)
		return (HBITMAP)NULL;
	
	if (m_pDC)
	{
		int nRasterCaps =  m_pDC->GetDeviceCaps(RASTERCAPS);
		int nNumColors = m_pDC->GetDeviceCaps(SIZEPALETTE);
		if (!(nRasterCaps & RC_PALETTE) || nNumColors > 256)
			return ::LoadBitmap(hInst, lpszResourceName);

	}
	HRSRC hRsrc = ::FindResource(hInst, lpszResourceName, RT_BITMAP);
	if (hRsrc == NULL)
		return NULL;

	HGLOBAL hGlobal = LoadResource(hInst, hRsrc);
	if (hGlobal == NULL)
		return NULL;

	HBITMAP hBitmap = NULL;
	LPBITMAPINFOHEADER lpbi = (LPBITMAPINFOHEADER)LockResource(hGlobal);
	int nNumColors = CreateDIBPalette(lpbi);
	if (m_pDC && nNumColors)
	{
		ASSERT_VALID(m_pDIBPalette);

		m_pDC->SelectPalette(m_pDIBPalette,FALSE);
		m_pDC->RealizePalette();

		hBitmap = CreateDIBitmap
						(
							m_pDC->GetSafeHdc(),
							lpbi,
							(LONG)CBM_INIT,
							(LPTSTR)lpbi + lpbi->biSize + nNumColors *sizeof(RGBQUAD),
							(LPBITMAPINFO)lpbi,
							DIB_RGB_COLORS
						);
	}

	UnlockResource(hGlobal);
	FreeResource(hGlobal);
	
	return hBitmap;
}

//----------------------------------------------------------------------------
CPalette* CWalkBitmap::GetDIBPalette ()
{
	return m_pDIBPalette;
}

//----------------------------------------------------------------------------
int CWalkBitmap::CreateDIBPalette (LPBITMAPINFOHEADER lpbi)
{
	BOOL bSuccess = FALSE;
	int  nNumColors;
	
	if (m_pDIBPalette)
	{
		m_pDIBPalette->DeleteObject();
		delete m_pDIBPalette;
		m_pDIBPalette = NULL;
	}
	if (m_pDC && !(m_pDC->GetDeviceCaps(RASTERCAPS) & RC_PALETTE))
		return 0;

	if (lpbi->biBitCount <= 8)
	   nNumColors = (1 << lpbi->biBitCount);
	else
	   nNumColors = 0;  // No palette needed for 24 BPP DIB

	if (lpbi->biClrUsed > 0)
	   nNumColors = lpbi->biClrUsed;  // Use biClrUsed

	if (nNumColors)
	{
		m_pDIBPalette = new CPalette;
		
		HANDLE hLogPal = GlobalAlloc (GHND, sizeof (LOGPALETTE) +
									 sizeof (PALETTEENTRY) * nNumColors);
		LPLOGPALETTE lpPal = (LPLOGPALETTE) GlobalLock (hLogPal);
		lpPal->palVersion    = 0x300;
		lpPal->palNumEntries = nNumColors;

		for (int i = 0;  i < nNumColors;  i++)
		{
			 lpPal->palPalEntry[i].peRed   = ((LPBITMAPINFO)lpbi)->bmiColors[i].rgbRed;
			 lpPal->palPalEntry[i].peGreen = ((LPBITMAPINFO)lpbi)->bmiColors[i].rgbGreen;
			 lpPal->palPalEntry[i].peBlue  = ((LPBITMAPINFO)lpbi)->bmiColors[i].rgbBlue;
			 lpPal->palPalEntry[i].peFlags = 0;
		}
		if (!m_pDIBPalette->CreatePalette (lpPal))
		{
			nNumColors = 0;
			delete m_pDIBPalette;
			m_pDIBPalette = NULL;
		}
		GlobalUnlock (hLogPal);
		GlobalFree   (hLogPal);
	}
	return nNumColors;
}

//==============================================================================
//			Class CWalkMenu implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CWalkMenu, CLocalizableMenu)

//-----------------------------------------------------------------------------
BOOL CWalkMenu::LoadMenu(LPCTSTR lpszResourceName)
{
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(lpszResourceName, RT_MENU);
	return Attach(::LoadMenu(hInst, lpszResourceName));
#else
	return CLocalizableMenu::LoadMenu(lpszResourceName);
#endif
}

//-----------------------------------------------------------------------------
BOOL CWalkMenu::LoadMenu(UINT nIDResource)
{
#ifdef _AFXDLL
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nIDResource), RT_MENU);
	return Attach(::LoadMenu(hInst,MAKEINTRESOURCE(nIDResource)));
#else
	return CLocalizableMenu::LoadMenu(nIDResource);
#endif
}
