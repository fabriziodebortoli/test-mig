
#include "stdafx.h"

#include <atlimage.h>

#include <TbNameSolver\IFileSystemManager.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include "TBThemeManager.h"
#include "WndObjDescription.h"
#include "generalfunctions.h"
#include "dib.h"
#include "dibitmap.h"
#include "DataObj.h"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// attention Microsof structure BITMAPINFOHEADER report width and height
// in pixel using item of LONG type
// is very difficult to immaginate same bitmap as large to necessitate of
// this wide type so all operation are "casted" to more appropriate "int" type


//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDIBitmap, CGdiObject)


//	~CDIBitmap ()
//
//	meed destructor because do not create CDIBitmap using CreateDIBitmap()
// 	globally allocating memory for them
//
//-----------------------------------------------------------------------------
CDIBitmap::~CDIBitmap ()
{
	if (m_hObject)
    {
		GlobalFree(m_hObject);
		Detach ();
	}
}

//-----------------------------------------------------------------------------
BOOL CDIBitmap::CreateBitmap (const CDIBitmap& bmp, CSize sz)
{
	if (m_hObject)
    {
		GlobalFree(m_hObject);
		Detach ();
    }

	return Attach
	(
		::CreateDib(bmp.GetSafeHandle(), sz.cx, sz.cy)
	);
}

//-----------------------------------------------------------------------------
BOOL CDIBitmap::CreateBitmap (const CDIBitmap& bmp)
{
	if (m_hObject)
    {
		GlobalFree(m_hObject);
		Detach ();
    }

	return Attach
	(
		// use original exetents
		::CreateDib(bmp.GetSafeHandle(), -1, -1)
	);
}

//-----------------------------------------------------------------------------
BOOL CDIBitmap::CreateBitmap
	(
		const	CBitmap&	bmp, 
				DWORD		biStyle, 
				WORD		biBits, 
		const	CPalette&	pal, 
				WORD		wUsage
	)
{
	if (m_hObject)
    {
		GlobalFree(m_hObject);
		Detach ();
	}

	return Attach
	(
		::DibFromBitmap
		(
			(HBITMAP) bmp.GetSafeHandle(),
			biStyle,
			biBits,
			(HPALETTE) pal.GetSafeHandle(),
			wUsage
		)
	);
}

//-----------------------------------------------------------------------------
BOOL CDIBitmap::CreateBitmap (LPCTSTR pszBitmapName)
{
	if (m_hObject)
    {
		GlobalFree(m_hObject);
		Detach ();
    }

	HFILE		hFile;
	OFSTRUCT	openBuffer;
    UINT		openMode = OF_READ | OF_SHARE_DENY_WRITE;

	USES_CONVERSION;
	// OpenFile non supporta UNICODE
	if ((hFile = ::OpenFile(T2A((LPTSTR)pszBitmapName), &openBuffer, openMode)) == HFILE_ERROR)
		return FALSE;

	HANDLE hDib = ::ReadDIB (hFile);

	if ((hDib == NULL) || !Attach (hDib))
	{
		::_lclose(hFile);
    	return FALSE;
    }
    
	return (::_lclose(hFile) != HFILE_ERROR);
}


//-----------------------------------------------------------------------------
BOOL CDIBitmap::SaveBitmap (LPCTSTR pszBitmapName)
{
	HFILE		hFile;
	OFSTRUCT	openBuffer;
    UINT		openMode = OF_CREATE | OF_SHARE_DENY_READ;

	USES_CONVERSION;
	// OpenFile non supporta UNICODE
	if ((hFile = ::OpenFile(T2A((LPTSTR)pszBitmapName), &openBuffer, openMode)) == HFILE_ERROR)
		return FALSE;

	if (!::WriteDIB (hFile, m_hObject))
	{                       
		::_lclose(hFile);
		return FALSE;
    }
	return (::_lclose(hFile) != HFILE_ERROR);
}



//-----------------------------------------------------------------------------
BOOL CDIBitmap::DibBitBlt
	(
				CDC&	dc,
		const	CRect&	dest,
				DWORD	dwRop,
				WORD	wUsage/* = DIB_RGB_COLORS*/
	)
{
	return ::DibBlt
		(
			dc.m_hDC,
			dest.left, dest.top, dest.Width(), dest.Height(),
			GetSafeHandle(),
			0, 0,
			dwRop,
            wUsage
        );
}

//-----------------------------------------------------------------------------
BOOL CDIBitmap::DibStretchBlt
	(
				CDC&	dc,
		const	CRect&	dest,
				DWORD	dwRop,
				WORD	wUsage/* = DIB_RGB_COLORS*/
	)
{
	BITMAPINFOHEADER info;

    // Get the bitmap header information.
	DibInfo(&info);

	return ::StretchDibBlt
		(
			dc.m_hDC,
			dest.left, dest.top, dest.Width(), dest.Height(),
			GetSafeHandle(),
			0, 0, (int) info.biWidth, (int) info.biHeight,
			dwRop,
            wUsage
        );
}

//-----------------------------------------------------------------------------
BOOL CDIBitmap::DibBitBlt
	(
				CDC&	dc,
		const	CRect&	dest,
		const	CPoint&	src, 
				DWORD	dwRop,
				WORD	wUsage/* = DIB_RGB_COLORS*/
	)
{
	return ::DibBlt
		(
			dc.m_hDC,
			dest.left, dest.top, dest.Width(), dest.Height(),
			GetSafeHandle(),
			src.x, src.y,
			dwRop,
            wUsage
        );
}

//-----------------------------------------------------------------------------
BOOL CDIBitmap::DibStretchBlt
	(
				CDC&	dc,
		const	CRect&	dest,
		const	CRect&	src,
				DWORD	dwRop,
				WORD	wUsage/* = DIB_RGB_COLORS*/
	)
{
	return ::StretchDibBlt
		(
			dc.m_hDC,
			dest.left, dest.top, dest.Width(), dest.Height(),
			GetSafeHandle(),
			src.left, src.top, src.Width(), src.Height(),
			dwRop,
            wUsage
        );
}


//-----------------------------------------------------------------------------
CSize CDIBitmap::GetSize () const
{
	BITMAPINFOHEADER info;

    // Get the bitmap header information.
	DibInfo(&info);
	return CSize((int) info.biWidth, (int) info.biHeight);
}


//-----------------------------------------------------------------------------
int CDIBitmap::GetWidth () const
{
	BITMAPINFOHEADER info;

    // Get the bitmap header information.
	DibInfo(&info);                      
	
	// cast to int because Microsoft Men are Crazy. Can you immaginate
	// a bitmap with over 40000 pixel width
	return (int) info.biWidth;
}


//-----------------------------------------------------------------------------
int CDIBitmap::GetHeight () const
{
	BITMAPINFOHEADER info;

    // Get the bitmap header information.
	DibInfo(&info);
	return (int) info.biHeight;
}

//-----------------------------------------------------------------------------
WORD CDIBitmap::GetPaletteSize () const
{
	BITMAPINFOHEADER info;

    // Get the bitmap header information.
	DibInfo(&info);
	return ::PaletteSize(&info);
}

//-----------------------------------------------------------------------------
WORD CDIBitmap::GetNumColors () const
{
	BITMAPINFOHEADER info;

    // Get the bitmap header information.
	DibInfo(&info);
	return ::DibNumColors(&info);
}

//-----------------------------------------------------------------------------
HBITMAP CDIBitmap::LoadBitmap(LPCTSTR sPath)
{
	CString aPath = sPath;

	IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
	if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(sPath))
		aPath = pFileSystemManager->GetTemporaryBinaryFile (sPath);

	CDIBitmap DIBmp;
	if (! DIBmp.CreateBitmap (aPath))
		return NULL;

	// Trasformazione da formato DIB e BMP
	HPALETTE	hPalette= ::CreateDibPalette(DIBmp.GetSafeHandle());
	HBITMAP		hBmp	= BitmapFromDib (DIBmp.GetSafeHandle(), hPalette, 0);
	::DeleteObject(hPalette);

	//CBitmap* pBitmap = new CBitmap;
	return hBmp;
}

//============================================================================
//					CCheckBitmap implementation
//============================================================================
IMPLEMENT_DYNAMIC(CCheckBitmap, CObject)

//-----------------------------------------------------------------------------
CCheckBitmap::CCheckBitmap(UINT IdBmp)
	:
	m_nIDB	(IdBmp),
	m_sNamespaceImage(_T(""))
{
	ASSERT_TRACE(IdBmp,"Parameter IdBmp cannot be 0");

	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(IdBmp), RT_BITMAP);
	HBITMAP hBitmap = ::LoadBitmap(hInst, MAKEINTRESOURCE(IdBmp));
	GetObject(hBitmap, sizeof(m_bmpInfo), &m_bmpInfo);
	DeleteObject(hBitmap);
}

//-----------------------------------------------------------------------------
CCheckBitmap::CCheckBitmap(CString namespaceImg)
	:
	m_nIDB(0),
	m_sNamespaceImage(namespaceImg)
{
	ASSERT_TRACE(!namespaceImg.IsEmpty(), "Parameter IdBmp cannot be null");
	HBITMAP hBitmap = LoadBitmapOrPng(namespaceImg);
	GetObject(hBitmap, sizeof(m_bmpInfo), &m_bmpInfo);
	DeleteObject(hBitmap);
}

//-----------------------------------------------------------------------------
CCheckBitmap::~CCheckBitmap	()
{	
}

//-----------------------------------------------------------------------------
BOOL CCheckBitmap::FloodDrawBitmap
	(
		HDC hdc, int x, int y,
		DWORD rop, COLORREF foreColor, COLORREF bkgrColor,
		BOOL bChecked, BOOL bDrawFarme, HBRUSH hFrameBrush
	)
{
	if (bChecked)
	{
		// Il bitmap deve avere come sfondo il colore che ha il pixel
		// alla posizione 0,0
		HBITMAP		hBitmap;
		COLORREF	refColor = 0;
		COLORREF	checkColor = 0;
		BOOL		bFoundCheckColor = FALSE;
		int			xCheck, yCheck;
		HBRUSH		hBrush;
		HBRUSH		hbrOld;

		HDC	hdcBits = CreateCompatibleDC(hdc);

		// determina il colore ed un punto di partenza del checkmark cercandolo nella matrice
		
		if (m_sNamespaceImage.IsEmpty())
		{
			HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(m_nIDB), RT_BITMAP);
			hBitmap = ::LoadBitmap(hInst, MAKEINTRESOURCE(m_nIDB));
		}
		else
		{
			hBitmap = LoadBitmapOrPng(m_sNamespaceImage);
		}


		SelectObject(hdcBits, hBitmap);

		// Il bitmap deve avere come sfondo il colore che ha il pixel
		// alla posizione 0,0
		refColor = GetPixel(hdcBits, 0, 0);

		// determina il colore ed un punto di partenza del checkmark cercandolo nella matrice
		for (xCheck = 0; xCheck < m_bmpInfo.bmWidth && !bFoundCheckColor; xCheck++)
			for (yCheck = 0; yCheck < m_bmpInfo.bmHeight && !bFoundCheckColor; yCheck++)
			{
				checkColor = GetPixel(hdcBits, xCheck, yCheck);
				if (checkColor == 0)
					bFoundCheckColor = TRUE;
			}

		if (bkgrColor != refColor)
		{
			hBrush = CreateSolidBrush(bkgrColor);
			hbrOld = (HBRUSH) SelectObject(hdcBits, hBrush);

			ExtFloodFill(hdcBits, 0, 0, refColor, FLOODFILLSURFACE);

			SelectObject(hdcBits, hbrOld);
			DeleteObject(hBrush);
		}

		// invece di fare la fillrect (vedi sotto)
		//if (!bChecked)
		//	foreColor = bkgrColor;

		if (bFoundCheckColor && foreColor != checkColor)
		{
			hBrush	= CreateSolidBrush(foreColor);
			hbrOld	= (HBRUSH) SelectObject(hdcBits, hBrush);

			ExtFloodFill(hdcBits, xCheck, yCheck, checkColor, FLOODFILLSURFACE);

			SelectObject(hdcBits, hbrOld);
			DeleteObject(hBrush);
		}

		if (IsScale())
		{
			int w = ScalePix(m_bmpInfo.bmWidth);
			int h = ScalePix(m_bmpInfo.bmHeight);
			StretchBlt(hdc, x, y, w, h, hdcBits, 0, 0, m_bmpInfo.bmWidth, m_bmpInfo.bmHeight, SRCCOPY);
		}
		else
		{
			BitBlt(hdc, x, y, m_bmpInfo.bmWidth, m_bmpInfo.bmHeight, hdcBits, 0, 0, rop);
		}
		
		DeleteDC(hdcBits);
		DeleteObject(hBitmap);
	}
	else
	{
		CRect bmpRect;
		bmpRect.left = x;
		bmpRect.top = y;
		bmpRect.right = bmpRect.left + m_bmpInfo.bmWidth;
		bmpRect.bottom = bmpRect.top + m_bmpInfo.bmHeight;
		// DPI scale rect
		bmpRect = ScaleRect(bmpRect);
		HBRUSH hBrush = CreateSolidBrush(bkgrColor);
		FillRect(hdc, bmpRect, hBrush);
		DeleteObject(hBrush);
	}

	if (bDrawFarme)
	{
		CRect bmpRect;
		bmpRect.left = x - 1;
		bmpRect.top = y + 1;
		int nSize = min(m_bmpInfo.bmWidth, m_bmpInfo.bmHeight) + 2;
		bmpRect.right = bmpRect.left + nSize;
		bmpRect.bottom = bmpRect.top + nSize;
		// DPI scale rect
		bmpRect = ScaleRect(bmpRect);
		FrameRect(hdc, &bmpRect, hFrameBrush ? hFrameBrush : (HBRUSH)AfxGetThemeManager()->GetStaticControlInBorderBkgBrush()->GetSafeHandle());
	}

	return TRUE;

}

//--------------------------------------------------------------------------
//CTransBmp gestisce il ridisegno di bitmap con sfondo trsparente
//--------------------------------------------------------------------------
CTransBmp::CTransBmp(COLORREF clrTransparent)
{
	m_clrTransparent =	clrTransparent;
    m_iWidth		=	0;
    m_iHeight		=	0;
    m_hbmMask		=	NULL;
}

//--------------------------------------------------------------------------
CTransBmp::~CTransBmp()
{
}

//--------------------------------------------------------------------------
void CTransBmp::GetMetrics()
{
    // Get the width and height.
    BITMAP bm;
    GetObject(sizeof(bm), &bm);
    m_iWidth = bm.bmWidth;
    m_iHeight = bm.bmHeight;
}


//--------------------------------------------------------------------------
int CTransBmp::GetWidth()
{
    if ((m_iWidth == 0) || (m_iHeight == 0)){
        GetMetrics();
    }
    return m_iWidth;
}

//--------------------------------------------------------------------------
int CTransBmp::GetHeight()
{
    if ((m_iWidth == 0) || (m_iHeight == 0)){
        GetMetrics();
    }
    return m_iHeight;
}

//--------------------------------------------------------------------------
void CTransBmp::Draw(HDC hDC, int x, int y)
{
	if (!m_hObject)
		return;
    
	ASSERT_TRACE(hDC,"Parameter hDC cannot be null");
    // Create a memory DC.
    HDC hdcMem = ::CreateCompatibleDC(hDC);
    // Select the bitmap into the mem DC.
    HBITMAP hbmold = 
        (HBITMAP)::SelectObject(hdcMem, (HBITMAP)(m_hObject));
    // Blt the bits.
    ::BitBlt(hDC,
             x, y,
             GetWidth(), GetHeight(),
             hdcMem,
             0, 0,
             SRCCOPY);
    ::SelectObject(hdcMem, hbmold);
    ::DeleteDC(hdcMem); 
}

//--------------------------------------------------------------------------
void CTransBmp::CreateMask(HDC hDC)
{
    // Nuke any existing mask.
    if (m_hbmMask) {
        ::DeleteObject(m_hbmMask);
		m_hbmMask = (HBITMAP)NULL;
    }
	
	if (!m_hObject)
		return;
    
	// Create memory DCs to work with.
    HDC hdcMask = ::CreateCompatibleDC(hDC);
    HDC hdcImage = ::CreateCompatibleDC(hDC);
    // Create a monochrome bitmap for the mask.
    m_hbmMask = ::CreateBitmap(GetWidth(),
                               GetHeight(),
                               1,
                               1,
                               NULL);
    // Select the mono bitmap into its DC.
    HBITMAP hbmOldMask = (HBITMAP)::SelectObject(hdcMask, m_hbmMask);
    // Select the image bitmap into its DC.
    HBITMAP hbmOldImage = (HBITMAP)::SelectObject(hdcImage, m_hObject);
    // Set the transparency color to be the top-left pixel.
    ::SetBkColor(hdcImage, ::GetPixel(hdcImage, 0, 0));
    // Make the mask.
    ::BitBlt(hdcMask,
             0, 0,
             GetWidth(), GetHeight(),
             hdcImage,
             0, 0,
             SRCCOPY);
    // Tidy up.
    ::SelectObject(hdcMask, hbmOldMask);
    ::SelectObject(hdcImage, hbmOldImage);
    ::DeleteDC(hdcMask);
    ::DeleteDC(hdcImage);
}

//--------------------------------------------------------------------------
void CTransBmp::DrawTransparent(HDC hDC, int x, int y)
{
	if (!m_hObject)
		return;

	ASSERT_TRACE(hDC,"Parameter hDC cannot be null");
    
	if (!m_hbmMask)
		CreateMask(hDC);

    ASSERT_TRACE(m_hbmMask,"Failed to create a mask");
    
	int dx = GetWidth();
    int dy = GetHeight();

    // Create a memory DC to which to draw.
    HDC hdcOffScr = ::CreateCompatibleDC(hDC);
    // Create a bitmap for the off-screen DC that is really
    // color-compatible with the destination DC.
    HBITMAP hbmOffScr = ::CreateBitmap(dx, dy, 
                             (BYTE)GetDeviceCaps(hDC, PLANES),
                             (BYTE)GetDeviceCaps(hDC, BITSPIXEL),
                             NULL);
    // Select the buffer bitmap into the off-screen DC.
    HBITMAP hbmOldOffScr = (HBITMAP)::SelectObject(hdcOffScr, hbmOffScr);

    // Copy the image of the destination rectangle to the
    // off-screen buffer DC, so we can manipulate it.
    ::BitBlt(hdcOffScr, 0, 0, dx, dy, hDC, x, y, SRCCOPY);

    // Create a memory DC for the source image.
    HDC hdcImage = ::CreateCompatibleDC(hDC); 
    HBITMAP hbmOldImage = (HBITMAP)::SelectObject(hdcImage, m_hObject);

    // Create a memory DC for the mask.
    HDC hdcMask = ::CreateCompatibleDC(hDC);
    HBITMAP hbmOldMask = (HBITMAP)::SelectObject(hdcMask, m_hbmMask);

    // XOR the image with the destination.
    ::SetBkColor(hdcOffScr, m_clrTransparent);
    ::BitBlt(hdcOffScr, 0, 0, dx, dy ,hdcImage, 0, 0, SRCINVERT);
    // AND the destination with the mask.
    ::BitBlt(hdcOffScr, 0, 0, dx, dy, hdcMask, 0,0, SRCAND);
    // XOR the destination with the image again.
    ::BitBlt(hdcOffScr, 0, 0, dx, dy, hdcImage, 0, 0, SRCINVERT);

    // Copy the resultant image back to the screen DC.
    ::BitBlt(hDC, x, y, dx, dy, hdcOffScr, 0, 0, SRCCOPY);

    // Tidy up.
    ::SelectObject(hdcOffScr, hbmOldOffScr);
    ::SelectObject(hdcImage, hbmOldImage);
    ::SelectObject(hdcMask, hbmOldMask);
    ::DeleteObject(hbmOffScr);
    ::DeleteDC(hdcOffScr);
    ::DeleteDC(hdcImage);
    ::DeleteDC(hdcMask);
}

/////////////////////////////////////////////////////////////////////////////
// CTranspBmpCtrl

CTranspBmpCtrl::CTranspBmpCtrl(int nBmpRef, BOOL bTransparent /*= TRUE*/)
{
	m_nBmpRef = nBmpRef;
	m_bTransparent = bTransparent;
}

CTranspBmpCtrl::CTranspBmpCtrl(const CString& sBmpName, BOOL bTransparent /*= TRUE*/)
{
	m_sBmpRefName = sBmpName;
	m_bTransparent = bTransparent;
}

CTranspBmpCtrl::~CTranspBmpCtrl()
{
}

//-----------------------------------------------------------------------------
BOOL CTranspBmpCtrl::InitCtrl()
{
	if (AfxGetThemeManager()->UseFlatStyle())
	{
		if ((this->GetStyle() & SS_SUNKEN) == SS_SUNKEN)
			ModifyStyle(SS_SUNKEN, WS_BORDER);

		AfxGetThemeManager()->MakeFlat(this);
	}

	//elimina i bordi delle immagini
	//if (!AfxGetThemeManager()->GetControlsUseBorders())
		ModifyStyle(WS_BORDER, 0);

	return TRUE;
}

void CTranspBmpCtrl::ChangeBitMap(int nBmpRef, BOOL bTransparent /*= TRUE*/)
{
	m_TransparentBmp.DeleteObject();

	m_nBmpRef = nBmpRef;
	m_bTransparent = bTransparent;

	m_TransparentBmp.LoadBitmap(nBmpRef);
	Invalidate();
}

void CTranspBmpCtrl::ChangeBitMap(const CString& sBmpName, BOOL bTransparent /*= TRUE*/)
{
	m_TransparentBmp.DeleteObject();

	m_sBmpRefName = sBmpName;
	m_bTransparent = bTransparent;

	m_TransparentBmp.LoadBitmap(m_sBmpRefName);

	Invalidate();
}

BEGIN_MESSAGE_MAP(CTranspBmpCtrl, CStatic)
	//{{AFX_MSG_MAP(CTranspBmpCtrl)
	ON_WM_PAINT()
	ON_MESSAGE				(UM_GET_CONTROL_DESCRIPTION,			OnGetControlDescription)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTranspBmpCtrl message handlers

//-----------------------------------------------------------------------------
void CTranspBmpCtrl::OnPaint() 
{
	CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente
	
	m_bTransparent ?
		m_TransparentBmp.DrawTransparent(dc.m_hDC, 0, 0) :
		m_TransparentBmp.Draw(dc.m_hDC, 0, 0);
}		

//-----------------------------------------------------------------------------
void CTranspBmpCtrl::PreSubclassWindow()
{
	m_TransparentBmp.LoadBitmap(m_nBmpRef);
	
	__super::PreSubclassWindow();

	InitCtrl();
}

//-----------------------------------------------------------------------------
LRESULT CTranspBmpCtrl::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndImageDescription* pDesc = (CWndImageDescription*)(pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndImageDescription), strId));
	
	pDesc->UpdateAttributes(this);
	CString sName = cwsprintf(_T("trbmp%ud.png"), (HBITMAP)m_TransparentBmp);
	
	if (pDesc->m_ImageBuffer.Assign( (HBITMAP)m_TransparentBmp, sName, this))
		pDesc->SetUpdated(&pDesc->m_ImageBuffer);

	return (LRESULT) pDesc;
}