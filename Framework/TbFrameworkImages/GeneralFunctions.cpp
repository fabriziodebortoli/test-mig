#include	"stdafx.h"

#include "windows.h"
#include "psapi.h"


#include	<TbNameSolver\TBNamespaces.h>
#include	<TbNameSolver\PathFinder.h>
#include	<TbNameSolver\LoginContext.h>
#include	<TbNameSolver\FileSystemFunctions.h>
#include	<TbNameSolver\IFileSystemManager.h>

//	local	declarations
#include	"CommonImages.h"	
#include	"GeneralFunctions.h"	

#ifdef	_DEBUG
#undef	THIS_FILE
static	char	THIS_FILE[] = __FILE__;
#endif

//----------------------------------------------------------------------------
static TCHAR szTBIconNamespace[] = _T("Image.Framework.TbFrameworkImages.Images.%s.%s.png");

static TCHAR sz16Size[] = _T("16x16");
static TCHAR sz20Size[] = _T("20x20");
static TCHAR sz25Size[] = _T("25x25");
static TCHAR szGlyphFolder[] = _T("Glyph");



//////////////////////////////////////////////////////////////////////////////////////////////
//					class CImageItem implementation
//////////////////////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CImageItem, CObject)

//-------------------------------------------------------------------------------------------------
CImageItem::CImageItem(Gdiplus::Bitmap* gdibitmap)
	:
	m_DGIBitmap			(gdibitmap),
	m_nReferenceCount	(1)
{

}

//-------------------------------------------------------------------------------------------------
CImageItem::~CImageItem()
{
	if (m_DGIBitmap)
		delete m_DGIBitmap;
}

//----------------------------------------------------------------------------------------------
Gdiplus::Bitmap* CImageItem::GetGdiBitmap()
{
	return m_DGIBitmap;
}

//----------------------------------------------------------------------------------------------
int CImageItem::GetReferenceCount()
{
	return m_nReferenceCount;
}

//----------------------------------------------------------------------------------------------
void CImageItem::IncrementReferenceCount()
{
	m_nReferenceCount++;
}

//----------------------------------------------------------------------------------------------
void CImageItem::DecrementReferenceCount()
{
	m_nReferenceCount--;
}


//////////////////////////////////////////////////////////////////////////////////////////////////
//						class CImagesCache implementation
//////////////////////////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CImagesCache, CObject)

//------------------------------------------------------------------------------------------------
CImagesCache::CImagesCache()
	:
	m_bCacheImages							(FALSE),
	m_pGlobalImagesMap						(NULL),
	m_bDirty								(FALSE),
	m_nMinReferences						(1),
	m_nMaxReferences						(0),
	m_nMaxCacheSize							(0),
	m_nPercCacheSizeFromProcessMemoryUsage	(1),
	m_nMaxSizeImageToCache					(32),
	m_nPercUsageOfMemoryFromMaxSize			(80)
{
	m_pGlobalImagesMap = new CMapStringToPtr();
	SetMaxSizeCacheImages();
}

//---------------------------------------------------------------------------------------------------
CImagesCache::~CImagesCache()
{
	ClearImageMap();
	if (m_pGlobalImagesMap)
		delete m_pGlobalImagesMap;
}

//---------------------------------------------------------------------------------------------------------
void CImagesCache::ClearImageMap()
{
	if (!m_pGlobalImagesMap)
		return;

	POSITION	pos;
	CString		nsImg;
	CImageItem*	pImageItem = NULL;

	for (pos = m_pGlobalImagesMap->GetStartPosition(); pos != NULL;)
	{
		m_pGlobalImagesMap->GetNextAssoc(pos, nsImg, (void*&)pImageItem);
		if (pImageItem)
			delete pImageItem;
	}

	m_pGlobalImagesMap->RemoveAll();

	m_bDirty = FALSE;
}

//---------------------------------------------------------------------------------------------------------------------
void  CImagesCache::FreeCacheMemory()
{
	BOOL bDeleted = FALSE;

	if (!m_pGlobalImagesMap || m_pGlobalImagesMap->GetSize() == 0)
	{
		m_bDirty = FALSE;
		return;
	}

	POSITION	pos;
	CString		nsImg;
	CImageItem*	pImageItem = NULL;

	SetOptimalValRefToClear();

	//reset dei valori min max
	m_nMinReferences = m_nMaxReferences;
	m_nMaxReferences = 0;

	for (pos = m_pGlobalImagesMap->GetStartPosition(); pos != NULL;)
	{
		m_pGlobalImagesMap->GetNextAssoc(pos, nsImg, (void*&)pImageItem);

		if (pImageItem && pImageItem->GetReferenceCount() < m_nOptimalRefToClear && m_bDirty)
		{
			delete pImageItem;
			m_pGlobalImagesMap->RemoveKey(nsImg);

			bDeleted = TRUE;

			//update flag isDirty
			long nTotalSize = sizeof(*m_pGlobalImagesMap) * m_pGlobalImagesMap->GetSize();
			double percUsedMemory = (nTotalSize * 100) / m_nMaxCacheSize;

			m_bDirty = percUsedMemory > m_nPercUsageOfMemoryFromMaxSize;
		}
		else
		{
			//manage min and max references (devo farla' qui in pulizia della memoria)
			if (pImageItem->GetReferenceCount() > m_nMaxReferences)
				m_nMaxReferences = pImageItem->GetReferenceCount();

			if (pImageItem->GetReferenceCount() < m_nMinReferences)
				m_nMinReferences = pImageItem->GetReferenceCount();
		}
	}

	SetOptimalValRefToClear();

	if (!bDeleted)
		m_bDirty = FALSE;	//forzatura per non andare in ricorsione all'infinito (non trovo piu' niente da deletare => esco)

	if (m_bDirty)
		FreeCacheMemory();
}

//------------------------------------------------------------------------------------------------
void  CImagesCache::SetOptimalValRefToClear()
{
	m_nOptimalRefToClear = (int)((m_nMinReferences + m_nMaxReferences) / 3);
}

//-------------------------------------------------------------------------------------------------------------
void CImagesCache::ManageMemoryUsage()
{
	if (!m_bDirty)
		return;

	FreeCacheMemory();
}

//---------------------------------------------------------------------------------------------------
Gdiplus::Bitmap* CImagesCache::GetImage(const CString& nsImage)		//, const CString& nsContainer /*= _T("")*/)
{
	Gdiplus::Bitmap* bitmap = NULL;
	CImageItem* pImageItem = NULL;

	if (!m_bCacheImages)
		return bitmap;

	if (m_pGlobalImagesMap->Lookup(nsImage, (void*&)pImageItem) && pImageItem)
	{
		pImageItem->IncrementReferenceCount();

		//manage min and max references
		if (pImageItem->GetReferenceCount() > m_nMaxReferences)
			m_nMaxReferences = pImageItem->GetReferenceCount();

		return pImageItem->GetGdiBitmap();
	}

	return bitmap;
}

//--------------------------------------------------------------------------------------------------------
void CImagesCache::AddImage(Gdiplus::Bitmap* dgibitmap, const CString& nsImage)
{
	long nTotalSize = 0;

	if (!m_bCacheImages || !dgibitmap || (int)dgibitmap->GetWidth() > m_nMaxSizeImageToCache || (int)dgibitmap->GetHeight() > m_nMaxSizeImageToCache || m_nMaxCacheSize == 0)
		return;

	m_nMinReferences = 1;
	CImageItem* pImageItem = new CImageItem(dgibitmap);

	if (!m_pGlobalImagesMap->Lookup(nsImage, (void*&)pImageItem))
	{
		m_pGlobalImagesMap->SetAt(nsImage, pImageItem);
		nTotalSize = sizeof(*m_pGlobalImagesMap) * m_pGlobalImagesMap->GetSize();

		if (nTotalSize > m_nMaxCacheSize)
			m_bDirty = TRUE;
	}
}

//-------------------------------------------------------------------------------------------------
void CImagesCache::SetCacheImages(BOOL bSet)
{
	m_bCacheImages = bSet;
}

//-----------------------------------------------------------------------------------
void  CImagesCache::SetMaxSizeCacheImages()
{
	PROCESS_MEMORY_COUNTERS_EX pmc;
	GetProcessMemoryInfo(GetCurrentProcess(), (PROCESS_MEMORY_COUNTERS*)&pmc, sizeof(pmc));
	m_nMaxCacheSize = (long)(pmc.PrivateUsage * m_nPercCacheSizeFromProcessMemoryUsage / 100);		//in bytes
}

//----------------------------------------------------------------------------
TB_EXPORT CString GetValidImagePath(const CString& strImage)
{
	CString s(strImage);
	if (!ExistFile(s))
	{
		CTBNamespace ns(s);
		if (!ns.IsValid())
		{
			return _T("");
		}
		s = AfxGetPathFinder()->GetFileNameFromNamespace(ns, AfxGetLoginInfos()->m_strUserName);
	}
	if (!ExistFile(s))
	{
		return _T("");
	}
	return s;
}

// Loads PNG image from resources, and return pounter to GDI+ bitmap object
// WARNING: returned pointer is owned by the caller and must be disposed
//-----------------------------------------------------------------------------
Gdiplus::Bitmap* LoadPNG(HINSTANCE hInst, HRSRC hResource)
{
	if (!hResource)
		return NULL;

	DWORD imageSize = ::SizeofResource(hInst, hResource);
	if (!imageSize)
		return NULL;

	const void* pResourceData = ::LockResource(::LoadResource(hInst, hResource));
	if (!pResourceData)
		return NULL;

	Gdiplus::Bitmap* pBitmap = NULL;
	HGLOBAL hBuffer = ::GlobalAlloc(GMEM_MOVEABLE, imageSize);
	if (hBuffer)
	{
		void* pBuffer = ::GlobalLock(hBuffer);
		if (pBuffer)
		{
			CopyMemory(pBuffer, pResourceData, imageSize);

			IStream* pStream = NULL;
			if (::CreateStreamOnHGlobal(hBuffer, FALSE, &pStream) == S_OK)
			{
				pBitmap = Gdiplus::Bitmap::FromStream(pStream);
				pStream->Release();
				if (
					pBitmap &&
					pBitmap->GetLastStatus() != Gdiplus::Ok
					)
				{
					delete pBitmap;
					pBitmap = NULL;
				}
			}
			::GlobalUnlock(hBuffer);
		}
		::GlobalFree(hBuffer);
	}

	return pBitmap;
}

//--------------------------------------------------------------------------------
Gdiplus::Bitmap* LoadPNG(CString sImgName, BOOL forceLoad)
{
	if (AfxIsRemoteInterface() && !forceLoad)
		return NULL;

	BYTE* temp = NULL;
	COleStreamFile stream;
	if (!stream.CreateMemoryStream(NULL))
		return NULL;

	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(sImgName))
	{
		int nFileLen = 0;
		temp = pFileSystemManager->GetBinaryFile(sImgName, nFileLen);
		if (temp != NULL && nFileLen > 0)
			stream.Write(temp, nFileLen);	
	}
	else
	{

		CFile imgFile;
		CFileException fileExc;
		if (!imgFile.Open(sImgName, CFile::modeRead | CFile::shareDenyNone, &fileExc))
			return NULL;

		int nFileLen = (int)imgFile.GetLength();
		temp = new BYTE[nFileLen];
		int nRead = 0;
		nRead = imgFile.Read(temp, nFileLen);
		stream.Write(temp, nRead);
		imgFile.Close();
	}
	Gdiplus::Bitmap* pImg = NULL;
	if (stream.GetLength() > 0)
	{
		stream.SeekToBegin();
		pImg = Gdiplus::Bitmap::FromStream(stream.GetStream());
	}
	
	if (temp)
		delete temp;
	
	stream.Close();	
	return pImg;

}

//--------------------------------------------------------------------------------
Gdiplus::Bitmap* LoadPNG(UINT nID)
{
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nID), _T("PNG"));
	ASSERT(hInst);
	HRSRC hResource = ::FindResource(hInst, MAKEINTRESOURCE(nID), _T("PNG"));
	ASSERT(hResource);
	return LoadPNG(hInst, hResource);
}

//--------------------------------------------------------------------------------
Gdiplus::Bitmap* LoadPNG(HINSTANCE hInst, UINT nID)
{
	ASSERT(hInst);
	HRSRC hResource = ::FindResource(hInst, MAKEINTRESOURCE(nID), _T("PNG"));
	ASSERT(hResource);
	return LoadPNG(hInst, hResource);
}

// Get size of icon form hicon
//----------------------------------------------------------------------------
CSize GetHiconSize(HICON	hIcon)
{
	ICONINFO info;
	CSize sizeRet(0,0);
	BOOL bRes = GetIconInfo(hIcon, &info);
	ASSERT(bRes);
	if (!bRes) {
		return sizeRet;
	}
	BITMAP bmp;
	GetObject(info.hbmColor, sizeof(bmp), &bmp);
	sizeRet.cx = bmp.bmWidth;
	sizeRet.cy = bmp.bmHeight;
	DeleteObject(info.hbmMask);
	DeleteObject(info.hbmColor);
	return sizeRet;
}

//----------------------------------------------------------------------------
void DrawBitmap(CBitmap* pBmp, CDC* pDC, const CRect& rect, COLORREF bkgColor, BOOL bStretch/* = FALSE*/)
{
	CDC dcMem;
	dcMem.CreateCompatibleDC(pDC);

	// fill with brush
	CBrush brush; brush.CreateSolidBrush(bkgColor);
	dcMem.FillRect(rect, &brush);

	if (pBmp && pBmp->GetSafeHandle())
	{
		CBitmap* pOldBmp = dcMem.SelectObject(pBmp);
		BITMAP bm;
		pBmp->GetObject(sizeof(BITMAP), (LPTSTR)&bm);
		if (bStretch)
		{
			pDC->TransparentBlt
				(
				rect.left, rect.top, rect.Width(), rect.Height(), &dcMem,
				0, 0, bm.bmWidth, bm.bmHeight, dcMem.GetPixel(0, 0)
				);
		}
		else
		{
			int nLeft = rect.Width() <= 0 ? 0 : (rect.Width() - bm.bmWidth) / 2;
			LONG w = bm.bmWidth > rect.Width() ? rect.Width() : bm.bmWidth;
			LONG h = bm.bmHeight > rect.Height() ? rect.Height() : bm.bmHeight;

			pDC->TransparentBlt
				(
				rect.left + nLeft, rect.top,
				w, h,
				&dcMem,
				0, 0,
				bm.bmWidth, bm.bmHeight,
				dcMem.GetPixel(0, 0)
				);
		}
		dcMem.SelectObject(pOldBmp);
	}
	dcMem.DeleteDC();
}

//-----------------------------------------------------------------------------
HBITMAP  ScaleBitmapInt(HBITMAP hBmp, LONG wNewWidth, LONG wNewHeight)
{
	BITMAP bm;
	::GetObject(hBmp, sizeof(bm), &bm);

	CBitmap* pOld2;
	CBitmap		bmpStretched;
	CDC destDC, sourceDC;

	sourceDC.CreateCompatibleDC(NULL);
	destDC.CreateCompatibleDC(NULL);

	HBITMAP hbmResult = ::CreateCompatibleBitmap(CClientDC(NULL), wNewWidth, wNewHeight);

	bmpStretched.CreateBitmap(wNewWidth, wNewHeight, 1, 32, NULL);
	pOld2 = sourceDC.SelectObject(&bmpStretched);
	sourceDC.SetStretchBltMode(HALFTONE);

	HBITMAP hbmOldSource = (HBITMAP)::SelectObject(sourceDC.m_hDC, hBmp);
	HBITMAP hbmOldDest = (HBITMAP)::SelectObject(destDC.m_hDC, hbmResult);
	BOOL bOk = destDC.StretchBlt(0, 0, wNewWidth, wNewHeight, &sourceDC, 0, 0, bm.bmWidth, bm.bmHeight, SRCCOPY);

	return hbmResult;
}

//----------------------------------------------------------------------------
COLORREF ConvertGrayscaleColor(COLORREF cl)
{
	int luma = (int)(GetRValue(cl)*0.3 + GetGValue(cl)*0.59 + GetBValue(cl)*0.11);
	return  RGB(luma, luma, luma);
}

//  CBitmapToHICON Convert CBitmap to HICON and resize image
//  Metodo unico per lìelaborazione delle immagini, resize, Mask ... eccc
//
//	pBitmap -> Bitmap image pointer, viene usato per applicare le mask
//	pDC		-> DC context
//  nWidth  -> Image resize if present the DC Context and nWidth > 0, nWidth = 0 calculate the size if pDC is not null
//  bUseMasakColorSpecifico se è a TRUE viene usato il Mask color specificato in rgbMask

//----------------------------------------------------------------------------
HICON CBitmapToHICON(CBitmap* pBitmap, CDC* pDC /*= NULL*/, UINT nWidth /*= 0*/, BOOL bUseMasakColorSpecifico /*= FALSE*/, COLORREF rgbMask /*= RGB(255,255,255)*/, BOOL bGray /*= FALSE*/)
{
	BITMAP		bm;
	CBitmap		bmpStretched;
	CImageList	imageList;
	CDC			memDC1, memDC2;
	CONST int	scalingFactor = 96;

	int iDPI = scalingFactor;
	if (pDC)
		iDPI = pDC->GetDeviceCaps(LOGPIXELSY);

	pBitmap->GetObject(sizeof(BITMAP), &bm);
	if (pDC == NULL || (nWidth == 0 && iDPI == scalingFactor && !bGray))
	{
		if (!bUseMasakColorSpecifico)
		{
			BITMAP bmBitmap;
			if (pBitmap->GetBitmap(&bmBitmap) == 0)
				return NULL;
			RGBTRIPLE* rgb = (RGBTRIPLE*)(bmBitmap.bmBits);
			if (rgb)
				rgbMask = RGB(rgb->rgbtRed, rgb->rgbtGreen, rgb->rgbtBlue);

		}

		if (!imageList.Create(bm.bmWidth, bm.bmHeight, ILC_COLOR32 | ILC_MASK, 0, 1)) return NULL;
		imageList.Add(pBitmap, rgbMask);
		return ((CImageList&)imageList).ExtractIcon(0);
	}

	if (nWidth == 0 && bm.bmWidth > 0)
		nWidth = MulDiv(bm.bmWidth, iDPI, scalingFactor);

	CBitmap* pOld1;
	memDC1.CreateCompatibleDC(pDC);
	pOld1 = memDC1.SelectObject(pBitmap);

	if (!bUseMasakColorSpecifico)
		rgbMask = memDC1.GetPixel(0, 0);

	if (bGray)
	{
		// convert a colour image to grayscale
		for (int x = 0; x < bm.bmWidth; x++)
		{
			for (int y = 0; y < bm.bmHeight; y++)
			{
				COLORREF cl = memDC1.GetPixel(x, y);
				int luma = (int)(GetRValue(cl)*0.3 + GetGValue(cl)*0.59 + GetBValue(cl)*0.11);
				cl = RGB(luma, luma, luma);
				memDC1.SetPixel(x, y, cl);
			}
		}
	}

	// Can use 32bpp bitmaps with alpha channel. In this case 'TransparentBlt' method will be never called.
	// this section convert to image to 32bpp
	//----------------------------------------------------------------------------
	CBitmap* pOld2;
	memDC2.CreateCompatibleDC(pDC);
		
	bmpStretched.CreateBitmap(nWidth, nWidth, 1, 32, NULL);
	pOld2 = memDC2.SelectObject(&bmpStretched);
	
	if (iDPI == scalingFactor)
		memDC2.SetStretchBltMode(HALFTONE);
	else
		memDC2.SetStretchBltMode(BLACKONWHITE);

	BOOL bOk = memDC2.StretchBlt(0, 0, nWidth, nWidth, &memDC1, 0, 0, bm.bmWidth, bm.bmHeight, SRCCOPY);
	// Reget mask Color. For effect of resize image the color can cange 
	if (!bUseMasakColorSpecifico)
		rgbMask = memDC2.GetPixel(0, 0);
	if (pOld2 != NULL)
		memDC2.SelectObject(pOld2);
	memDC2.DeleteDC();
	ASSERT(bmpStretched.m_hObject);
	if (!bmpStretched.GetBitmap(&bm))
		return NULL;

	//----------------------------------------------------------------------------
	if (pOld1 != NULL)
		memDC1.SelectObject(pOld1);
	memDC1.DeleteDC();

	if (!imageList.Create(nWidth, nWidth, ILC_COLOR32 | ILC_MASK, 0, 1)) return NULL;
	imageList.Add(&bmpStretched, rgbMask);
	HICON hIco = ((CImageList&)imageList).ExtractIcon(0);
	DeleteObject(bmpStretched);
	ASSERT(hIco);
	return hIco;
}

//----------------------------------------------------------------------------
void CBitmapClone(CBitmap* pBitmapSource, CBitmap* pBitmapDest)
{
	BITMAP bmpSouce;

	CDC SrcDC;
	CDC DestDC;

	SrcDC.CreateCompatibleDC(NULL);
	DestDC.CreateCompatibleDC(NULL);

	pBitmapSource->GetBitmap(&bmpSouce);

	// Moved this here to select the bitmap before creating the compatible 	destination bitmap
	CBitmap *pOldBmp1 = SrcDC.SelectObject(pBitmapSource);

	pBitmapDest->CreateCompatibleBitmap(&SrcDC, bmpSouce.bmWidth, bmpSouce.bmHeight);

	CBitmap *pOldBmp2 = DestDC.SelectObject(pBitmapDest);

	DestDC.BitBlt(0, 0, bmpSouce.bmWidth, bmpSouce.bmHeight, &SrcDC, 0, 0, SRCCOPY);

	SrcDC.SelectObject(pOldBmp1);
	DestDC.SelectObject(pOldBmp2);

	SrcDC.DeleteDC();
	DestDC.DeleteDC();
}

//----------------------------------------------------------------------------
HBITMAP LoadBitmapOrPng(UINT nIDB)
{
	// Load image is a BMP
	HMODULE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nIDB), RT_BITMAP);
	HBITMAP hBitmap = ::LoadBitmap(hInst, MAKEINTRESOURCE(nIDB));
	if (!hBitmap)
	{
		// load PNG
		hInst = AfxFindResourceHandle(MAKEINTRESOURCE(nIDB), _T("PNG"));
		Gdiplus::Bitmap* gdibitmap = LoadPNG(hInst, nIDB);
		if (gdibitmap)
		{
			gdibitmap->GetHBITMAP(Gdiplus::Color::Transparent, &hBitmap);
			if (gdibitmap)
			{
				delete gdibitmap;
				gdibitmap = NULL;
			}
		}
	}
	ASSERT(hBitmap);
	return hBitmap;
}

//-------------------------------------------------------------------------------------------------------------
Gdiplus::Bitmap* LoadGdiplusBitmapOrPngInternal(CString strImageNS, BOOL bUseColoredImage, BOOL bForceLoad)
{
	CTBNamespace aNs(CTBNamespace::IMAGE, strImageNS);
	CString	sImagePath = AfxGetPathFinder()->GetFileNameFromNamespace(aNs, AfxGetLoginInfos()->m_strUserName);
	
	if (bUseColoredImage)
		sImagePath = GetMoreColoredImage(sImagePath);

	return LoadPNG(sImagePath, bForceLoad);
}

//-------------------------------------------------------------------------------------------
Gdiplus::Bitmap* LoadGdiplusBitmapOrPng(CString strImageNS, BOOL bUseColoredImage /*= FALSE*/, BOOL bForceLoad /*= FALSE*/)
{
	Gdiplus::Bitmap* gdibitmap = NULL;
	//qui deve gestire la clone (chiamate dalle altri classi e che poi ammazzano il gdiplus::bitmap per cui se messo nella mappa => crash in ClearMap o tentativo di riutilizzo)
	Gdiplus::Bitmap* gdibitmap_clone = NULL;

	gdibitmap = m_GlobalCacheImages.GetImage(strImageNS);

	if (gdibitmap)
	{
		if (!m_GlobalCacheImages.GetCacheImages())
			return gdibitmap;
		else
		{
			Gdiplus::Rect aRect(0, 0, gdibitmap->GetWidth(), gdibitmap->GetHeight());
			gdibitmap_clone = gdibitmap->Clone(aRect, gdibitmap->GetPixelFormat());

			return gdibitmap_clone;
		}
	}

	CTBNamespace aNs(CTBNamespace::IMAGE, strImageNS);
	const CLoginInfos* pInfos = AfxGetLoginInfos();
	const CString sUser = pInfos ? AfxGetLoginInfos()->m_strUserName : _T("");
	CString	sImagePath = AfxGetPathFinder()->GetFileNameFromNamespace(aNs, sUser);

	if (bUseColoredImage)
		sImagePath = GetMoreColoredImage(sImagePath);
	gdibitmap = LoadPNG(sImagePath, bForceLoad);

	if (!gdibitmap)
		return NULL;

	m_GlobalCacheImages.AddImage(gdibitmap, strImageNS);

	if (!m_GlobalCacheImages.GetCacheImages())
		return gdibitmap;
	else
	{
		Gdiplus::Rect aRect(0, 0, gdibitmap->GetWidth(), gdibitmap->GetHeight());
		gdibitmap_clone = gdibitmap->Clone(aRect, gdibitmap->GetPixelFormat());

		return gdibitmap_clone;
	}

	return NULL;
}
//----------------------------------------------------------------------------
Gdiplus::Bitmap* LoadGdiplusBitmapOrPngFromFile(CString strFileName)
{
	if (strFileName.IsEmpty())
		return NULL;
	
	BYTE* temp = NULL;
	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(strFileName))
	{
		int nFileLen = 0;
		Gdiplus::Bitmap* pImg = NULL;
		temp = pFileSystemManager->GetBinaryFile(strFileName, nFileLen);		
		if (temp && nFileLen > 0)
		{
			COleStreamFile stream;
			if (!stream.CreateMemoryStream(NULL))
				return NULL;
			stream.Write(temp, nFileLen);
			if (stream.GetLength() > 0)
			{
				stream.SeekToBegin();
				pImg = Gdiplus::Bitmap::FromStream(stream.GetStream());
			}
		}
		if (temp)
			delete temp;
		return pImg;
	}
	else
		if (::ExistFile(strFileName))
			return  Gdiplus::Bitmap::FromFile(strFileName);

	return NULL;	
}

//----------------------------------------------------------------------------
HBITMAP	LoadBitmapOrPng(CString strImageNS, BOOL bUseColoredImage)
{
	Gdiplus::Bitmap* gdibitmap = NULL;
	HBITMAP hBmp = NULL;

	gdibitmap = m_GlobalCacheImages.GetImage(strImageNS); //GetImage(strImageNS);

	if (gdibitmap)
	{
		gdibitmap->GetHBITMAP(Gdiplus::Color::Transparent, &hBmp);
		return hBmp;
	}

	gdibitmap = LoadGdiplusBitmapOrPngInternal(strImageNS, bUseColoredImage, FALSE);

	if (!gdibitmap)
		return NULL;

	gdibitmap->GetHBITMAP(Gdiplus::Color::Transparent, &hBmp);

	if (hBmp)
		m_GlobalCacheImages.AddImage(gdibitmap, strImageNS);

	if (!m_GlobalCacheImages.GetCacheImages() && gdibitmap)
		delete gdibitmap;

	return hBmp;
}

//----------------------------------------------------------------------------
BOOL LoadBitmapOrPng(CBitmap* pBmp, CString strImageNS)
{
	HBITMAP hBmp = NULL;

	if (AfxIsRemoteInterface()) //per weblook non c'e' bisogno vengano caricate le immagini
		return FALSE;

	Gdiplus::Bitmap* gdibitmap = m_GlobalCacheImages.GetImage(strImageNS); //GetImage(strImageNS);

	if (gdibitmap)
	{
		gdibitmap->GetHBITMAP(Gdiplus::Color::Transparent, &hBmp);
		if (!hBmp)
			return FALSE;

		return pBmp->Attach(hBmp);
	}

	gdibitmap = LoadGdiplusBitmapOrPngInternal(strImageNS, FALSE, FALSE);

	if (!gdibitmap)
		return FALSE;

	gdibitmap->GetHBITMAP(Gdiplus::Color::Transparent, &hBmp);
	if (!hBmp)
		return FALSE;

	BOOL bOk = pBmp->Attach(hBmp);

	if (bOk)
		m_GlobalCacheImages.AddImage(gdibitmap, strImageNS);

	if (!m_GlobalCacheImages.GetCacheImages() && gdibitmap)
		delete gdibitmap;

	return bOk;
}

//----------------------------------------------------------------------------
BOOL LoadBitmapOrPng(CBitmap* pBmp, CString strImageNS, COLORREF bkgColor)
{
	Gdiplus::Bitmap* gdibitmap = NULL;
	HBITMAP hBmp = NULL;

	gdibitmap = m_GlobalCacheImages.GetImage(strImageNS); //GetImage(strImageNS);

	if (gdibitmap)
	{
		BYTE bkgR = GetRValue(bkgColor);
		BYTE bkgG = GetGValue(bkgColor);
		BYTE bkgB = GetBValue(bkgColor);
		gdibitmap->GetHBITMAP(Gdiplus::Color::Color(bkgR, bkgG, bkgB), &hBmp);

		ASSERT(hBmp);
		if (!hBmp)
			return FALSE;

		return pBmp->Attach(hBmp);

	}
	
	CTBNamespace aNs(CTBNamespace::IMAGE, strImageNS);
	CString	sImagePath = AfxGetPathFinder()->GetFileNameFromNamespace(aNs, AfxGetLoginInfos()->m_strUserName);
	
	gdibitmap = LoadPNG(sImagePath, FALSE);

	if (gdibitmap)
	{
		BYTE bkgR = GetRValue(bkgColor);
		BYTE bkgG = GetGValue(bkgColor);
		BYTE bkgB = GetBValue(bkgColor);
		gdibitmap->GetHBITMAP(Gdiplus::Color::Color(bkgR, bkgG, bkgB), &hBmp);
	}

	ASSERT(hBmp);
	if (!hBmp)
		return FALSE;

	BOOL bOk = pBmp->Attach(hBmp);

	if (bOk)
		m_GlobalCacheImages.AddImage(gdibitmap, strImageNS);	//AddImage(strImageNS, gdibitmap);

	return bOk;
}


//----------------------------------------------------------------------------
BOOL LoadBitmapOrPng(CBitmap* pBmp, UINT nIDB)
{
	// Load image is a BMP
	HBITMAP hBmp = LoadBitmapOrPng(nIDB);
	ASSERT(hBmp);
	if (!hBmp)
		return FALSE;
	return pBmp->Attach(hBmp);
}

//----------------------------------------------------------------------------
BOOL LoadBitmapOrPng(CBCGPButton* pButton, CString strImageNS, UINT nIDB /* = 0*/, CWnd* pParent /*= NULL*/, BOOL bCheckdImage/* = FALSE*/)
{
	ASSERT_VALID(pButton);
	ASSERT(pButton->m_hWnd);

	BOOL bImageLoaded = FALSE;
	CBitmap	bitmap;
	CBitmap	bitmapGray;

	if (!strImageNS.IsEmpty())
	{
		bImageLoaded = ::LoadBitmapOrPng(&bitmap, strImageNS);
	}
	else if (nIDB)
	{
		bImageLoaded = ::LoadBitmapOrPng(&bitmap, nIDB);
	}

	if (bImageLoaded)
	{
		CDC* pDC = pParent ? pParent->GetDC() : NULL;

		CBitmapClone(&bitmap, &bitmapGray);

		// conver in HICON and convert in Gray
		int nWidth = bitmap.GetBitmapDimension().cx;
		HICON icoGray = ::CBitmapToHICON(&bitmapGray, pDC, nWidth, FALSE, RGB(255, 255, 255), TRUE);
		HICON ico = ::CBitmapToHICON(&bitmap, pDC, nWidth, FALSE, RGB(255, 255, 255), FALSE);

		if (bCheckdImage)
			pButton->SetCheckedImage(ico, 1, NULL, icoGray);
		else
			pButton->SetImage(ico, 1, NULL, icoGray);

		if (pDC)
			pParent->ReleaseDC(pDC);
	}
	else
	{
		return FALSE;
	}

	// delete bitmap
	bitmap.DeleteObject();
	bitmapGray.DeleteObject();
	return TRUE;
}

//----------------------------------------------------------------------------
HICON TBLoadImage(UINT nIDB, CDC* pDC, UINT nWidth)
{
	CBitmap	bitmap;
	LoadBitmapOrPng(&bitmap, nIDB);
	HICON h = CBitmapToHICON(&bitmap, pDC, nWidth);
	bitmap.DeleteObject();
	return h;
}


// seza CDC non esegue il resize delle immagini.
//----------------------------------------------------------------------------
HICON TBLoadImage(CDC* pDC, HINSTANCE hImageContainer, UINT nIdImg, int btnWidth, BOOL bPNG)
{
	if (hImageContainer == NULL)
	{
		hImageContainer = AfxFindResourceHandle(MAKEINTRESOURCE(nIdImg), bPNG ? _T("PNG") : RT_BITMAP); // cerca nella corrente dll o attiva il walking
	}

	CBitmap		bmpButton;
	CBitmap*	pButton = &bmpButton;
	HANDLE hImg = NULL;
	Gdiplus::Color colorMaskPng;

	if (bPNG)
	{
		CImage image;
		Gdiplus::Bitmap* gdibitmap = LoadPNG(hImageContainer, nIdImg);
		if (!gdibitmap)
			return FALSE;

		gdibitmap->GetPixel(0, 0, &colorMaskPng);
		HBITMAP handle = NULL;
		gdibitmap->GetHBITMAP(Gdiplus::Color::Transparent, &handle);

		if (gdibitmap->GetHeight() == btnWidth || btnWidth == 0)
		{
			HICON hico;
			gdibitmap->GetHICON(&hico);
			if (gdibitmap)
			{
				delete gdibitmap;
				gdibitmap = NULL;
			}
			return hico;
		}

		hImg = (HANDLE)handle;
		if (gdibitmap)
		{
			delete gdibitmap;
			gdibitmap = NULL;
		}
	}
	else
	{
		hImg = (HBITMAP)::LoadImage(hImageContainer, MAKEINTRESOURCE(nIdImg), IMAGE_BITMAP, 0, 0, LR_DEFAULTSIZE | LR_CREATEDIBSECTION/*|LR_SHARED*/);
	}

	if (!bmpButton.Attach(hImg))
		return NULL;

	if (bPNG)
		return CBitmapToHICON(&bmpButton, pDC, btnWidth, TRUE, RGB(colorMaskPng.GetRed(), colorMaskPng.GetGreen(), colorMaskPng.GetBlue()));

	return CBitmapToHICON(&bmpButton, pDC, btnWidth);
}

// Never elaboration of Image, the image is loag fron name space and the Gdiplus return clean HICON
// used in cTree and other
//----------------------------------------------------------------------------
HICON TBLoadPng(CString strImageNS)
{
	if (AfxIsRemoteInterface())
		return NULL;

	HICON hIco = NULL;

	Gdiplus::Bitmap* bitmapIcon = m_GlobalCacheImages.GetImage(strImageNS);	//GetImage(strImageNS);

	if (bitmapIcon)
	{
		bitmapIcon->GetHICON(&hIco);
		return hIco;
	}

	CTBNamespace aNs(CTBNamespace::IMAGE, strImageNS);
	CString	sImagePath = AfxGetPathFinder()->GetFileNameFromNamespace(aNs, AfxGetLoginInfos()->m_strUserName);

	CString	sFileExtension = sImagePath.Right(4);
	if (sImagePath.Right(4).CompareNoCase(_T(".PNG")) != 0)
	{
		ASSERT(FALSE);
		return hIco;
	}

	Gdiplus::Bitmap* gdibitmap = LoadPNG(sImagePath, FALSE);
	ASSERT(gdibitmap);
	if (!gdibitmap)
		return hIco;

	gdibitmap->GetHICON(&hIco);

	m_GlobalCacheImages.AddImage(gdibitmap, strImageNS);

	return hIco;
}

//----------------------------------------------------------------------------
HICON TBLoadImage(CString strImageNS, CDC* pDC /* =NULL */, UINT nWidth /*= 32*/, COLORREF bkgColor /*= RGB(255, 255, 255)*/)
{
	if(strImageNS == _T("")) //in a json, ' icon : "" '
		return NULL;

	HICON icon = NULL;
	BOOL bAddToCache = FALSE;
	Gdiplus::Bitmap* bitmapIcon = m_GlobalCacheImages.GetImage(strImageNS); //GetImage(strImageNS);

	HBITMAP hBmp = NULL;
	CBitmap	bmpButton;
	HANDLE hImg = NULL;

	if (!bitmapIcon)
	{
		CTBNamespace aNs(CTBNamespace::IMAGE, strImageNS);
		CString	sImagePath = AfxGetPathFinder()->GetFileNameFromNamespace(aNs, AfxGetLoginInfos()->m_strUserName);
		bitmapIcon = GetGDIBitmapFromDosPath(sImagePath, hBmp, pDC, nWidth, bkgColor);

		if (!bitmapIcon)
			return NULL;

		bAddToCache = TRUE;
	}
	else
	{
		//is in cache => set color
		Gdiplus::Color colorMaskPng;
		BYTE bkgR = GetRValue(bkgColor);
		BYTE bkgG = GetGValue(bkgColor);
		BYTE bkgB = GetBValue(bkgColor);
		colorMaskPng = Gdiplus::Color::Color(bkgR, bkgG, bkgB);
		bitmapIcon->GetHBITMAP(colorMaskPng, &hBmp);
	}

	hImg = (HANDLE)hBmp;
	if (!hImg)
		return NULL;

	if (!bmpButton.Attach(hImg))
	{
		DeleteObject(hImg);
		return NULL;
	}

	if (bAddToCache)
		m_GlobalCacheImages.AddImage(bitmapIcon, strImageNS);

	icon = CBitmapToHICON(&bmpButton, pDC, nWidth, FALSE, bkgColor, FALSE);

	DeleteObject(hImg);

	if (!m_GlobalCacheImages.GetCacheImages() && bitmapIcon)
		delete bitmapIcon;

	return icon;
}

//--------------------------------------------------------------------------------------------------------------------------------------
Gdiplus::Bitmap* GetGDIBitmapFromDosPath(CString sImagePath, HBITMAP& hBmp, CDC* pDC, UINT nWidth, COLORREF bkgColor)
{
	// Get file extension
	Gdiplus::Bitmap* gdibitmap = NULL;
	CString	sTest = sImagePath.Right(4);
	CBitmap cBmp;
	Gdiplus::Color colorMaskPng;

	// Is a .PNG ?
	if (sImagePath.Right(4).CompareNoCase(_T(".PNG")) == 0)
	{
		if (AfxIsRemoteInterface())
		{
			return NULL;
		}

		// New load PNG
		gdibitmap = LoadPNG(sImagePath, FALSE);
		ASSERT(gdibitmap);
		if (!gdibitmap) return NULL;
		BYTE bkgR = GetRValue(bkgColor);
		BYTE bkgG = GetGValue(bkgColor);
		BYTE bkgB = GetBValue(bkgColor);
		colorMaskPng = Gdiplus::Color::Color(bkgR, bkgG, bkgB);
		gdibitmap->GetHBITMAP(colorMaskPng, &hBmp);
	}
	else if (sImagePath.Right(4).CompareNoCase(_T(".BMP")) == 0)
	{
		// .Bmp
		CImage image;
		image.Load(sImagePath);
		cBmp.Attach(image.Detach());
		hBmp = ((HBITMAP)cBmp.m_hObject);
		gdibitmap = &Gdiplus::Bitmap(hBmp, NULL);
	}
	else
	{
		// format image not supported
		ASSERT(FALSE);
		return NULL;
	}

	return gdibitmap;
}



//-----------------------------------------------------------------------------
CString ReplaceInfinity(CString szIconNS)
{
	szIconNS.Replace(sz25Size, sz16Size);
	return szIconNS;
}

//-----------------------------------------------------------------------------
CString ComposeIconNamespace(const CString szIconNSTemplate, const CString& szIcon, IconSize size /*= TILEMNG*/)
{
	TCHAR buffer[512];
	int nResult = 0;
	if (size == TILEMNG || size == TOOLBAR)
	{
		nResult = swprintf_s(buffer, szIconNSTemplate, sz25Size, szIcon);
		return buffer;
	}
	else if (size == MINI /*|| size == X24*/)
	{
		nResult = swprintf_s(buffer, szIconNSTemplate, sz25Size, szIcon);
		return buffer;
	}
	else if (size == CONTROL)
	{
		nResult = swprintf_s(buffer, szIconNSTemplate, sz20Size, szIcon);
		return buffer;
	}
	else
	{
		ASSERT(FALSE);
		return buffer;
	}
}

//-----------------------------------------------------------------------------
CString TBIcon(const CString& szIcon, IconSize size /*= TILEMNG*/)
{
	return szIcon.IsEmpty() ? szIcon : ComposeIconNamespace(szTBIconNamespace, szIcon, size);
}

//-----------------------------------------------------------------------------
CString TBGlyph(const CString& szGlyph)
{
	TCHAR buffer[512];
	int nResult = swprintf_s(buffer, szTBIconNamespace, szGlyphFolder, szGlyph);
	return buffer;
}

//-----------------------------------------------------------------------------
CString GetMoreColoredImage(const CString& strImageFullName)
{
	CString sName = GetName(strImageFullName);
	CString sSuffix = _T("_c");
	if (sName.Right(sSuffix.GetLength()).CompareNoCase(sSuffix) == 0)
		return strImageFullName;

	CString sPath = GetPath(strImageFullName);
	CString sColorImage = sPath + _T("\\") + sName + sSuffix + GetExtension(strImageFullName);

	if (ExistFile(sColorImage))
		return sColorImage;

	return strImageFullName;
}