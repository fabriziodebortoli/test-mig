
#include "stdafx.h"

#include <TbNameSolver\IFileSystemManager.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\TBThemeManager.h>

#include <TbFrameworkImages\GeneralFunctions.h>

#include <TbGeneric\TbStrings.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\WndObjDescription.h>
#include "globals.h"

#include "pictures.h"

#ifdef new 
#undef new
#endif

#include <gdiplus.h>
#include <atlimage.h>
//includere come ultimo include all'inizio del cpp

#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define GDIPLUS_IMG(img)	((Gdiplus::Bitmap*)(img))
							
//-----------------------------------------------------------------------------
CLSID FindCodecForExtension (LPCTSTR pszExtension, const Gdiplus::ImageCodecInfo* pCodecs, UINT nCodecs)
{
	CT2CW pszExtensionW (pszExtension);

	for (UINT iCodec = 0; iCodec < nCodecs; iCodec++)
	{
		CStringW strExtensions (pCodecs[iCodec].FilenameExtension);
		int iStart = 0;
		do
		{
			CStringW strExtension = ::PathFindExtensionW (strExtensions.Tokenize(L";", iStart));
			if (iStart != -1)
			{
				if (strExtension.CompareNoCase( pszExtensionW ) == 0)
				{
					return (pCodecs[iCodec].Clsid);
				}
			}
		} while (iStart != -1);
	}
	return CLSID_NULL;
}


//-----------------------------------------------------------------------------
//					Formati supportati precedentemente dalla libr. LEAD TOOLS
//-----------------------------------------------------------------------------
/*
Legenda : s = testato ok, n= testato con errore, niente = da testare

s			JPEG and LEAD Compressed (JPG and CMP)
			GIF and TIFF with LZW Compression
			TIFF Without LZW Compression
s			BMP Formats
s			Icons (ICO)
s			Cursors (CUR)
			PCX Formats (PCX)
n			PCX Formats (DCX)
			Kodak Formats (FPX)
s			Kodak Formats (PCD)
n			DICOM Format (DIC)
n			Exif Formats (TIFF)
s			Exif Formats (JPG)
s			Windows Metafile Formats (EMF)
s			Windows Metafile Formats (WMF)
n			Drawing Interchange Format (DXF)

			XPicMap (XPM)
			Interchange File Formats (IFF)
			Portable Bitmap Utilities (PBM)
			Dr. Halo (CUT)
			Microsoft Windows Clipboard (CLP)
			X WindowDump (XWD)
			Flic Animation (FLC)
s			Windows Animated Cursor (ANI)
			DRaWing (DRW)
			Computer Graphics Metafile (CGM)
			PLT (PLT)
			DGN (DGN)
s			PhotoShop 3.0 Format (PSD)
s			Portable Network Graphics Format (PNG)
			Truevision TARGA Format (TGA)
s			Encapsulated PostScript (EPS)
n			JBIG Format (JBG)
			SUN Raster Format (RAS)
s			WordPerfect Format (WPG)
s			Macintosh Pict Format (PCT)
n			Windows AVI Format (AVI)
			TIFF CCITT and Other FAX Formats
s			LEAD 1-Bit Format (CMP)
			Miscellaneous 1-Bit Formats (MAC)
s			Miscellaneous 1-Bit Formats (IMG)
s			Miscellaneous 1-Bit Formats (MSP)

*/

//=============================================================================
class GdiPlusContext
{
public:
	ULONG_PTR m_gdiplusToken;
	Gdiplus::GdiplusStartupInput m_gdiplusStartupInput;

	GdiPlusContext()
	{
		Gdiplus::GdiplusStartup(&m_gdiplusToken, &m_gdiplusStartupInput, NULL);
	}

	virtual ~GdiPlusContext()
	{
		Gdiplus::GdiplusShutdown(m_gdiplusToken);
	}
};

//-----------------------------------------------------------------------------

///////////////////////////////////////////////////////////////////////////////
/*TB_EXPORT*/ CString CTBPicture::ImageFitModeToString(ImageFitMode f)
{
	// NORMAL, BEST, HORIZONTAL, VERTICAL
	switch (f)
	{
	case ImageFitMode::NORMAL:			return _TB("Original Size");
	case ImageFitMode::BEST:			return _TB("Best");
	case ImageFitMode::HORIZONTAL:		return _TB("Horizontal");
	case ImageFitMode::VERTICAL:		return _TB("Vertical");
	case ImageFitMode::STRETCH:			return _TB("Stretch");
	default:							return _TB("Unknown");
	}
}
///////////////////////////////////////////////////////////////////////////////


void*	CTBPicture::g_pGdiPlusContext = NULL;

IMPLEMENT_DYNAMIC(CTBPicture, CObject)

//-----------------------------------------------------------------------------

/* static */void CTBPicture::InitializeGdiPlus()
{
	g_pGdiPlusContext = new GdiPlusContext();

}

/* static */void CTBPicture::TerminateGdiPlus()
{
	delete ((GdiPlusContext*)g_pGdiPlusContext);
}

//-----------------------------------------------------------------------------
/* static */BOOL CTBPicture::IsImageGdiPlusValid(CString filePath )
{
  Gdiplus::Bitmap image(filePath.AllocSysString());
  if(image.GetFlags() == Gdiplus::ImageFlagsNone) 
	  return FALSE;
  else 
	  return TRUE;
}

//-----------------------------------------------------------------------------
CTBPicture::CTBPicture(ImageFitMode imgFitMode)
	:
	m_bOk (TRUE),
	m_bUseGdiPlus (TRUE),
	m_pImage (NULL),
	m_ImageFitMode(imgFitMode)
{
}

//-----------------------------------------------------------------------------
CTBPicture::CTBPicture(const CTBPicture& source)
	: 	
	m_bOk				(source.m_bOk),
	m_bUseGdiPlus		(source.m_bUseGdiPlus),
	m_pImage			(NULL),
	m_ImageFitMode		(source.m_ImageFitMode),
	m_strFileName		(source.m_strFileName)
{
	if (m_bUseGdiPlus && source.m_pImage)
	{
		int width = source.GetWidth();
		int height = source.GetHeight();
		Gdiplus::Rect sourceRect(0, 0, width, height);
		m_pImage = GDIPLUS_IMG(source.m_pImage)->Clone(sourceRect, PixelFormatDontCare);
	}	
}

//-----------------------------------------------------------------------------
CTBPicture::~CTBPicture()
{
	if (m_bUseGdiPlus)
	{
		if (m_pImage)
		{
			delete GDIPLUS_IMG(m_pImage); m_pImage = NULL;
		}
	}
}

//-----------------------------------------------------------------------------
HBITMAP CTBPicture::GetHBitmap(COLORREF* pRgbBkgColor /* = NULL */)
{
	HBITMAP hBmp;
	if (pRgbBkgColor)
	{
		Gdiplus::Color bckColor(GetRValue(*pRgbBkgColor), GetGValue(*pRgbBkgColor), GetBValue(*pRgbBkgColor));
		m_pImage->GetHBITMAP(bckColor, &hBmp);
	}
	else
		m_pImage->GetHBITMAP(Gdiplus::Color::Transparent, &hBmp);
	ASSERT(hBmp);
	return hBmp;
}

//-----------------------------------------------------------------------------
int CTBPicture::GetWidth() const
{
	if (m_bUseGdiPlus)
	{
		if (m_pImage)
		{
			return GDIPLUS_IMG(m_pImage)->GetWidth();
		}
		else
			return 0;
	}
	else
		return 0;
}

//-----------------------------------------------------------------------------
int CTBPicture::GetHeight() const
{
	if (m_bUseGdiPlus)
	{
		if (m_pImage)
		{
			return GDIPLUS_IMG(m_pImage)->GetHeight();
		}
		else
			return 0;
	}		
	else
		return 0;
}

//-----------------------------------------------------------------------------
void CTBPicture::Clear()
{
	if (m_bUseGdiPlus)
	{
		if (m_pImage)
		{
			delete GDIPLUS_IMG(m_pImage); m_pImage = NULL;
		}
	}
	m_strFileName.Empty();
}

//-----------------------------------------------------------------------------
BOOL CTBPicture::LoadBitmapFromResource(int IDB, HDC hDC)
{
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(IDB), RT_BITMAP);
	HBITMAP hBmp = ::LoadBitmap(hInst, MAKEINTRESOURCE(IDB));
	if (hBmp == NULL)
	{
		m_bOk = FALSE;
		return FALSE;
	}

	// DPI Scaling Image
	if (IsScale())
	{
		BITMAP bmInfo;
		::GetObject(hBmp, sizeof(bmInfo), &bmInfo);
		LONG newSizeW = ScalePix(bmInfo.bmWidth);
		LONG newSizeH = ScalePix(bmInfo.bmHeight);
		hBmp = ScaleBitmapInt(hBmp, newSizeW, newSizeH);
	}

	if (m_bUseGdiPlus)
	{
		m_pImage = Gdiplus::Bitmap::FromHBITMAP(hBmp, (HPALETTE)GetSystemPaletteUse(hDC));
	
		if (m_pImage == NULL || GDIPLUS_IMG(m_pImage)->GetFlags() == Gdiplus::ImageFlagsNone) 
		{
			if (m_pImage)
			{
				delete GDIPLUS_IMG(m_pImage); m_pImage = NULL;
			}
			m_bOk = FALSE;
			return FALSE;
		}
	}
	
	m_bOk = TRUE;
	return m_bOk;
}

//-----------------------------------------------------------------------------
void CTBPicture::Reload()
{
	CString sFile = m_strFileName;

	Clear();

	ReadFile(sFile, TRUE);
}

//-----------------------------------------------------------------------------
//Il metodo è utilizzato pesantemente da Woorm/ReportStudio:
//TbGenlib/ParsEdtOther.cpp		(CPictureStatic)
//TbWoormEngine/repfield.cpp	(WoormField)
//TbWoormViewer/rectobj.cpp		(GraphRect, FieldRect)
//TbWoormViewer/baseobj.cpp		(GenericDrawObj)
//TbWoormViewer/cell.cpp		(TableCell)
//TbWoormViewer/WoormView.cpp	(CWoormView)
//----
// TODO BAUZI
//----
BOOL CTBPicture::ReadFile (const CString& sImage/*path or namespace*/, BOOL bCheckExist/* = FALSE*/)
{
	//ottimizzazione: se l'immagine tramite path è la stessa non fa nulla
	if (m_strFileName.CompareNoCase(sImage) == 0 && m_pImage != NULL)
		return TRUE;

	CString strNS;

	CString sPath(sImage);
	if (!sImage.IsEmpty() && !::ExistFile(sImage))
	{
		//ottimizzazione: se l'immagine tramite namespace è la stessa non fa nulla
		BOOL bAddExt = FALSE;
		if (GetExtension(sImage).IsEmpty())
		{
			sPath += L".BMP"; bAddExt = TRUE;
		}
		sPath = AfxGetPathFinder()->FromNs2Path(sPath, CTBNamespace::IMAGE, CTBNamespace::FILE);

		if (m_strFileName.CompareNoCase(sPath) == 0 && m_pImage != NULL)
			return TRUE;

		CTBNamespace ns(sImage + (bAddExt ? L".BMP" : L""));
		if (ns.IsValid())
			strNS = ns.ToString();
	}

	//elimina risorsa bitmap precedente
	Clear();

	//nuovo path immagine
	m_strFileName = sPath;

	if (m_strFileName.IsEmpty())
		return !bCheckExist;

	if (bCheckExist && !::ExistFile(m_strFileName))
		return FALSE;
	//-----

	if (m_bUseGdiPlus)
	{
		if (!strNS.IsEmpty())
			m_pImage = LoadGdiplusBitmapOrPng(strNS);//cmq ritorna NULL se non trova niente in cache o file system
		else
			m_pImage = LoadGdiplusBitmapOrPngFromFile(m_strFileName);//cmq ritorna NULL 
				
		if (m_pImage == NULL || GDIPLUS_IMG(m_pImage)->GetFlags() == Gdiplus::ImageFlagsNone) 
		{
			if (m_pImage)
			{
				delete GDIPLUS_IMG(m_pImage); m_pImage = NULL;
			}
			m_bOk = FALSE;
			return FALSE;
		}
		m_bOk = TRUE;
	}

	return m_bOk;
}

//-----------------------------------------------------------------------------
BOOL CTBPicture::SaveFile (const CString& strFileName)
{
	if (!m_bOk) 
		return m_bOk;

	if (m_bUseGdiPlus)
	{
		if (m_pImage)
		{
			Gdiplus::Bitmap* pBmp = GDIPLUS_IMG(m_pImage);
			UINT nEncoders;
			UINT nBytes;
			Gdiplus::Status status;

			status = Gdiplus::GetImageEncodersSize( &nEncoders, &nBytes);
			if (status != Gdiplus::Ok)
			{
				return FALSE;
			}

			USES_CONVERSION_EX;
			Gdiplus::ImageCodecInfo* pEncoders = static_cast< Gdiplus::ImageCodecInfo* >( _ATL_SAFE_ALLOCA(nBytes, _ATL_SAFE_ALLOCA_DEF_THRESHOLD) );
			if (pEncoders == NULL)
				return FALSE;

			status = Gdiplus::GetImageEncoders( nEncoders, nBytes, pEncoders );
			if (status != Gdiplus::Ok)
			{
				return FALSE;
			}

			CLSID clsidEncoder = CLSID_NULL;
			// Determine clsid from extension
			clsidEncoder = FindCodecForExtension( ::PathFindExtension( strFileName ), pEncoders, nEncoders );
			IFileSystemManager* pFileSystemManager = AfxGetFileSystemManager();
			if (pFileSystemManager && pFileSystemManager->IsManagedByAlternativeDriver(strFileName))
			{
				CImageBuffer imageBuffer;
				//Come secondo parametro non serve un Id, visto che qui il CImageBuffer viene usato solo per farsi popolare lo stream di Bytes 
				imageBuffer.Assign(m_pImage, _T("dummyID"));
				int nLen = 0;
				BYTE* pContent = NULL;
				imageBuffer.GetData(nLen, pContent);
				return pFileSystemManager->SaveBinaryFile(strFileName, pContent, nLen);

			}
			else
				return Gdiplus::Ok == pBmp->Save(strFileName, &clsidEncoder); 
		}
		return FALSE;
	}
	else
		return FALSE;
}

//-----------------------------------------------------------------------------
void CTBPicture::DrawPicture(CDC& DC, CRect dest)
{
	if (!m_bOk || m_pImage == NULL) 
		return;

	if (m_bUseGdiPlus)
	{
		Gdiplus::Graphics graphics(DC.m_hDC); 
		if (DC.IsPrinting())	
			graphics.SetPageUnit(Gdiplus::UnitPixel);
		Gdiplus::Rect r (dest.left, dest.top, dest.Width(), dest.Height());
		Gdiplus::Status st = graphics.DrawImage(GDIPLUS_IMG(m_pImage), r);
		graphics.Flush();
		ASSERT(st == Gdiplus::Ok);
	}
}

//-----------------------------------------------------------------------------
void CTBPicture::DrawPicture(CDC& DC, CRect dest, CRect rectSrc)
{
	if (!m_bOk || m_pImage == NULL) 
		return;

	if (m_bUseGdiPlus)
	{	
		Gdiplus::Graphics graphics(DC.m_hDC); 
		if (DC.IsPrinting())	
			graphics.SetPageUnit(Gdiplus::UnitPixel);

		Gdiplus::Rect r (dest.left, dest.top, dest.Width(), dest.Height());
		Gdiplus::Status st = graphics.DrawImage(GDIPLUS_IMG(m_pImage), r, rectSrc.left, rectSrc.top, rectSrc.Width(), rectSrc.Height(), Gdiplus::UnitPixel, NULL, NULL, NULL);
		graphics.Flush();
		ASSERT(st == Gdiplus::Ok);
	}
}

//-----------------------------------------------------------------------------
BOOL CTBPicture::TopLeftOrientation()
{
	if (!m_bOk) 
		return FALSE;

	if (m_bUseGdiPlus)
	{
		//TODO 
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void CTBPicture::FitImage
	(
		CRect&			rect,
		const CRect&	rectSrc
    )
{                  
	int nHBitmap;
	int nWBitmap;
	int nHInside;
	int nWInside;

	nHBitmap = this->GetHeight();
	nWBitmap = this->GetWidth();

	nHInside = rect.Height();
	nWInside = rect.Width(); 

	if (!(nHBitmap == nHInside && nWBitmap == nWInside))
	{
		ASSERT_TRACE( nHInside && nHBitmap && nWBitmap,"Height or Width cannot be zero");

		int nY = (int) ((double)nHBitmap / nWBitmap * nWInside);
		int nX = (int) ((double)nWBitmap / nHBitmap * nHInside);

		switch (m_ImageFitMode)
		{
			case BEST:
				if ((nY < nHInside && nX >= nWInside) || (nY >= nHInside && nX < nWInside))
				{ 
					if (nY < nHInside)
					{
						rect.bottom = rect.top + nY;
					}
					if (nX < nWInside)
					{
						rect.right = rect.left + nX;
					}
				}
				break;
			case HORIZONTAL:
				if (nHInside < nHBitmap)
					rect.bottom = rect.top + nY;					
				else
					rect.bottom = rect.top + (nWInside * nHBitmap / nWBitmap);
			
				break;
			case VERTICAL:
				if (nWInside < nWBitmap)
					rect.right = rect.left + nX;
				if (nX < nWInside)
				{
					rect.right = rect.left + nX;
				}
				if (nX > nWInside)
					rect.right = rect.left + nX;
				break;
		}
	}
}
//=============================================================================