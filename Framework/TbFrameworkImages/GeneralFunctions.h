#pragma once

#include "beginh.dex"

////////////////////////////////////////////////////////////////////////////////////////////////
//					class CImageItem definition
////////////////////////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CImageItem : public CObject
{
	DECLARE_DYNAMIC(CImageItem)

private:
	Gdiplus::Bitmap*	m_DGIBitmap;
	int					m_nReferenceCount;

public:
	CImageItem(Gdiplus::Bitmap* gdibitmap);
	~CImageItem();

public:
	Gdiplus::Bitmap*	GetGdiBitmap();
	int					GetReferenceCount();
	void				IncrementReferenceCount();
	void				DecrementReferenceCount();
};

//////////////////////////////////////////////////////////////////////////////////////////////////
//						class CImagesCache definition
//////////////////////////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CImagesCache : public CObject
{
	DECLARE_DYNCREATE(CImagesCache);

public:
	CImagesCache();
	~CImagesCache();

private:
	long						m_nMaxCacheSize;
	int							m_nPercCacheSizeFromProcessMemoryUsage;
	int							m_nMaxSizeImageToCache;
	int							m_nPercUsageOfMemoryFromMaxSize;
	CMapStringToPtr*			m_pGlobalImagesMap;
	BOOL						m_bCacheImages;
	BOOL						m_bDirty;
	int							m_nMinReferences;
	int							m_nMaxReferences;
	int							m_nOptimalRefToClear;

private:
	void  FreeCacheMemory();
	void  SetOptimalValRefToClear();

public:
	Gdiplus::Bitmap* GetImage(const CString& nsImage/*, const CString& nsContainer = _T("")*/);
	void			 AddImage(Gdiplus::Bitmap* dgibitmap, const CString& nsImage/*, const CString& nsContainer = _T("")*/);
	void			 SetCacheImages(BOOL bSet);
	void			 SetMaxSizeCacheImages();
	BOOL			 GetCacheImages() { return m_bCacheImages; }
	void			 ClearImageMap();
	void			 ManageMemoryUsage();

};

TB_EXPORT CImagesCache m_GlobalCacheImages;

Gdiplus::Bitmap* LoadPNG(CString sImageFile, BOOL forceLoad);

//-----------------------------------------------------------------------------
TB_EXPORT Gdiplus::Bitmap* LoadPNG(UINT nID);

TB_EXPORT Gdiplus::Bitmap* LoadPNG(HINSTANCE hInst, UINT nID);

//-----------------------------------------------------------------------------
TB_EXPORT CString GetValidImagePath(const CString& strImage);

//-----------------------------------------------------------------------------
TB_EXPORT COLORREF ConvertGrayscaleColor(COLORREF cl);
TB_EXPORT HBITMAP  ScaleBitmapInt(HBITMAP hBmp, LONG wNewWidth, LONG wNewHeight);

//-----------------------------------------------------------------------------
TB_EXPORT HICON		CBitmapToHICON(CBitmap* pBitmap, CDC* pDC = NULL, UINT nWidth = 0, BOOL bUseMasakColorSpecifico = FALSE, COLORREF rgbMask = RGB(255, 255, 255), BOOL bGray = FALSE);
TB_EXPORT void		CBitmapClone(CBitmap* pBitmapSource, CBitmap* pBitmapDest);

TB_EXPORT void		DrawBitmap(CBitmap* pBmp, CDC* pDC, const CRect& rect, COLORREF bkgColor, BOOL bStretch = FALSE);

//-----------------------------------------------------------------------------
TB_EXPORT HBITMAP	LoadBitmapOrPng(UINT nIDB);
TB_EXPORT HBITMAP	LoadBitmapOrPng(CString strImageNS, BOOL bUseColoredImage = FALSE);
TB_EXPORT Gdiplus::Bitmap* LoadGdiplusBitmapOrPngInternal	(CString strImageNS, BOOL bUseColoredImage = FALSE, BOOL bForceLoad = FALSE);
TB_EXPORT Gdiplus::Bitmap* LoadGdiplusBitmapOrPng			    (CString strImageNS, BOOL bUseColoredImage = FALSE, BOOL bForceLoad = FALSE);
TB_EXPORT Gdiplus::Bitmap* LoadGdiplusBitmapOrPngFromFile (CString strFileName);

TB_EXPORT BOOL		LoadBitmapOrPng(CBitmap* pBmp, UINT nIDB);
TB_EXPORT BOOL		LoadBitmapOrPng(CBitmap* pBmp, CString strImageNS);
TB_EXPORT BOOL		LoadBitmapOrPng(CBitmap* pBmp, CString strImageNS, COLORREF bkgColor);

TB_EXPORT BOOL		LoadBitmapOrPng(CBCGPButton* pButton, CString strImageNS, UINT nIDB = 0, CWnd* pParent = NULL, BOOL bCheckdImage = FALSE);

//-----------------------------------------------------------------------------
		  Gdiplus::Bitmap* GetGDIBitmapFromDosPath(CString sPath, HBITMAP& hBmp, CDC* pDC, UINT nWidth, COLORREF bkgColor);

TB_EXPORT HICON		TBLoadImage(UINT nIDB, CDC* pDC = NULL, UINT nWidth = 0);
TB_EXPORT HICON		TBLoadImage(CString strImageNS, CDC* pDC = NULL, UINT nWidth = 32, COLORREF bkgColor = RGB(255, 255, 255));
TB_EXPORT HICON		TBLoadImage(CDC* pDC, HINSTANCE hImageContainer, UINT nIdImg, int btnWidth, BOOL bPNG);
TB_EXPORT HICON		TBLoadPng(CString strImageNS);
TB_EXPORT CSize		GetHiconSize(HICON	hIcon);

TB_EXPORT CString GetMoreColoredImage(const CString& strImageFullName);
//-----------------------------------------------------------------------------
enum IconSize { TILEMNG, TOOLBAR, MINI, CONTROL };

TB_EXPORT CString ComposeIconNamespace(const CString szIconNSTemplate, const CString& szIconFileName, IconSize size = TILEMNG);
TB_EXPORT CString ReplaceInfinity(CString szIconNS);

TB_EXPORT CString TBIcon(const CString& szIcon, IconSize size);
TB_EXPORT CString TBGlyph(const CString& szIcon);

#include "endh.dex"
