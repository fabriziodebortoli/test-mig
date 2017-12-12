//==============================================================================
// description  : API for Device Independent Bitmap

#include <windows.h>
#include <memory.h>

#include "dib.h"

#include "begincpp.dex"

#define MAKEP(sel,off)  ((LPVOID)MAKELONG(off,sel))
#define ALLOCP(ulBytes) ((LPVOID)MAKELONG(0, GlobalAlloc(GPTR,(DWORD)(ulBytes))))
#define FREEP(lp)       GlobalFree((HANDLE)HIWORD((DWORD)(lp)));

HANDLE CreateLogicalDib(HBITMAP hbm, WORD biBits, HPALETTE hpal);

static DWORD NEAR PASCAL lread(HFILE fh, LPVOID pv, DWORD ul);
static DWORD NEAR PASCAL lwrite(HFILE fh, LPVOID pv, DWORD ul);

/* flags for _lseek */
#define  SEEK_CUR 1
#define  SEEK_END 2
#define  SEEK_SET 0

#ifdef WIN32
#define HUGE
#define HMEMCPY memcpy
#endif

// ReadDIB()
//
// Reads a DIB file and returns a handle to a memory DIB containing
// the BITMAPINFOHEADER, palette data, and bitmap bits.
// 
//-----------------------------------------------------------------------------
HANDLE ReadDIB(HFILE fh)               // Input file handle
{
	BITMAPINFOHEADER    bi;
	LPBITMAPINFOHEADER  lpbi;
	DWORD               dwLen;
	DWORD               dwBits;
	HANDLE              hdib;
	HANDLE              h;

	if (fh == HFILE_ERROR)
		return NULL;

	// Read the bitmap info header and palette from the file.
	hdib = ReadDibBitmapInfo(fh);

	if (!hdib)
		return NULL;

	// Get the bitmap header information.
	DibInfo(hdib,&bi);

	// Calculate memory needed to store the DIB bits, and attempt to
	// allocate the memory. 
	//
	dwBits = bi.biSizeImage;
	dwLen  = bi.biSize + PaletteSize(&bi) + dwBits;

	if (!(h = GlobalReAlloc(hdib,dwLen,0)))
	{
		GlobalFree(hdib);
		hdib = NULL;
	}
	else
	{
		hdib = h;
	}

	// If memory allocation successful, read the bitmap bits. 
	if (hdib)
	{
		lpbi = (LPVOID)GlobalLock(hdib);

		lread(fh, (LPSTR)lpbi+(WORD)lpbi->biSize+PaletteSize(lpbi), dwBits);

		GlobalUnlock(hdib);
	}

	return hdib;
}


// WriteDIB(fh, hdib)
//
// Writes a global handle in CF_DIB format to a file.
// 
//-----------------------------------------------------------------------------
BOOL WriteDIB( HFILE fh, HANDLE hdib)               
{
	BITMAPFILEHEADER    hdr;
	LPBITMAPINFOHEADER  lpbi;
	BITMAPINFOHEADER    bi;
	DWORD               dwSize;

	if (!hdib)
		return FALSE;

	if (fh == HFILE_ERROR)
		return FALSE;

	// Calculate total size of DIB. 
	DibInfo(hdib, &bi);
	dwSize = bi.biSize + PaletteSize(&bi) + bi.biSizeImage;

	lpbi = (LPVOID)GlobalLock(hdib);

	// Construct the BITMAPFILEHEADER and write the DIB.
	hdr.bfType          = BFT_BITMAP;
	hdr.bfSize          = dwSize + sizeof(BITMAPFILEHEADER);
	hdr.bfReserved1     = 0;
	hdr.bfReserved2     = 0;
	hdr.bfOffBits       = (DWORD)sizeof(BITMAPFILEHEADER) + lpbi->biSize +
						  PaletteSize(lpbi);

	_lwrite(fh, (LPVOID)&hdr, sizeof(BITMAPFILEHEADER));
	lwrite(fh, (LPVOID)lpbi, dwSize);

	GlobalUnlock(hdib);

	return TRUE;
}


// DibInfo(hbi, lpbi)
//
// Retrieves the DIB info associated with a CF_DIB format memory block.
// Works with both the BITMAPINFOHEADER and BITMAPCOREHEADER formats.
// 
//-----------------------------------------------------------------------------
BOOL DibInfo(HANDLE hbi, LPBITMAPINFOHEADER lpbi)       
{
	if(!hbi)
		return FALSE;

	*lpbi = *(LPBITMAPINFOHEADER)GlobalLock(hbi);
	GlobalUnlock(hbi);

	if(lpbi->biSize == sizeof(BITMAPCOREHEADER))
	{
		BITMAPCOREHEADER bc;

		bc = *(LPBITMAPCOREHEADER)lpbi;

		lpbi->biSize          = sizeof(BITMAPINFOHEADER);
		lpbi->biWidth         = (DWORD)bc.bcWidth;
		lpbi->biHeight        = (DWORD)bc.bcHeight;
		lpbi->biPlanes        =  (WORD)bc.bcPlanes;
		lpbi->biBitCount      =  (WORD)bc.bcBitCount;
		lpbi->biCompression   = BI_RGB;
		lpbi->biSizeImage     = 0;
		lpbi->biXPelsPerMeter = 0;
		lpbi->biYPelsPerMeter = 0;
		lpbi->biClrUsed       = 0;
		lpbi->biClrImportant  = 0;
	}

	// Fill in the default fields
	if (lpbi->biSize != sizeof(BITMAPCOREHEADER))
	{
		if (lpbi->biSizeImage == 0L)
			lpbi->biSizeImage = WIDTHBYTES(lpbi->biWidth*lpbi->biBitCount) *
								lpbi->biHeight;

		if (lpbi->biClrUsed == 0L)
			lpbi->biClrUsed = DibNumColors(lpbi);
	}
	return TRUE;
}

// CreateBIPalette()
//
// Given a Pointer to a BITMAPINFO struct will create a
// a GDI palette object from the color table.
//
// Works with "old" (BITMAPCOREHEADER) and "new" (BITMAPINFOHEADER) DIBs.
//
// 
//-----------------------------------------------------------------------------
HPALETTE CreateBIPalette(LPBITMAPINFOHEADER lpbi)        
{
	NPLOGPALETTE        pPal;
	HPALETTE            hpal = NULL;
	WORD                nNumColors;
	BYTE                red;
	BYTE                green;
	BYTE                blue;
	WORD                i;
	RGBQUAD             FAR *pRgb;
	BOOL                fCoreHeader;

	if (!lpbi)
		return NULL;

	fCoreHeader = (lpbi->biSize == sizeof(BITMAPCOREHEADER));

	pRgb = (RGBQUAD FAR *)((LPSTR)lpbi + (WORD)lpbi->biSize);
	nNumColors = DibNumColors(lpbi);

	if (nNumColors)
	{
		// Allocate a new color table, copy the palette entries to
		// it, and create the palette. 
		pPal = (NPLOGPALETTE)LocalAlloc(LPTR,sizeof(LOGPALETTE) + nNumColors * sizeof(PALETTEENTRY));

		if (!pPal)
			goto exit;

		pPal->palNumEntries = nNumColors;
		pPal->palVersion    = PALVERSION;

		for (i = 0; i < nNumColors; i++)
		{
			pPal->palPalEntry[i].peRed   = pRgb->rgbRed;
			pPal->palPalEntry[i].peGreen = pRgb->rgbGreen;
			pPal->palPalEntry[i].peBlue  = pRgb->rgbBlue;
			pPal->palPalEntry[i].peFlags = (BYTE)0;

			if (fCoreHeader)
				((LPSTR)pRgb) += sizeof(RGBTRIPLE) ;
			else
				pRgb++;
		}

		hpal = CreatePalette(pPal);
		LocalFree((HANDLE)pPal);
	}
	else if (lpbi->biBitCount == 24)
	{
		// A DIB with a bit count of 24 has no color table entries. Set
		// the number of entries to the maximum (256).
		nNumColors = MAXPALETTE;
		pPal = (NPLOGPALETTE)LocalAlloc(LPTR,
					sizeof(LOGPALETTE) + nNumColors * sizeof(PALETTEENTRY));

		if (!pPal)
			goto exit;

		pPal->palNumEntries = nNumColors;
		pPal->palVersion    = PALVERSION;

		red = green = blue = 0;

		// Generate 256 (8 * 8 * 4) RGB combinations to fill the palette
		// entries.
		for (i = 0; i < pPal->palNumEntries; i++)
		{
			pPal->palPalEntry[i].peRed   = red;
			pPal->palPalEntry[i].peGreen = green;
			pPal->palPalEntry[i].peBlue  = blue;
			pPal->palPalEntry[i].peFlags = (BYTE)0;

			if (!(red += 32))
				if (!(green += 32))
					blue += 64;
		}

		hpal = CreatePalette(pPal);
		LocalFree((HANDLE)pPal);
	}

exit:
	return hpal;
}


//  CreateDibPalette()
//
//  Given a Global HANDLE to a BITMAPINFO Struct
//  will create a GDI palette object from the color table.
//
//  works with "old" and "new" DIB's
//
//-----------------------------------------------------------------------------
HPALETTE CreateDibPalette(HANDLE hbi)
{
	HPALETTE hpal;

	if (!hbi)
		return NULL;

	hpal = CreateBIPalette((LPBITMAPINFOHEADER)GlobalLock(hbi));
	GlobalUnlock(hbi);
	return hpal;
}


// ReadDibBitmapInfo(fh)
//
// Reads a file in DIB format and returns a global HANDLE to its
// BITMAPINFO.  This function works with both "old" (BITMAPCOREHEADER)
// and "new" (BITMAPINFOHEADER) formats, but always returns a "new"
// BITMAPINFO structure.
// 
//-----------------------------------------------------------------------------
HANDLE ReadDibBitmapInfo(HFILE fh)                            
{
	DWORD     off;
	HANDLE    hbi = NULL;
	int       size;
	int       i;
	WORD      nNumColors;

	RGBQUAD FAR       *pRgb;
	BITMAPINFOHEADER   bi;
	BITMAPCOREHEADER   bc;
	LPBITMAPINFOHEADER lpbi;
	BITMAPFILEHEADER   bf;

	if (fh == HFILE_ERROR)
		return NULL;

	off = _llseek(fh,0L,SEEK_CUR);

	// Read bitmap file header.
	if (sizeof(bf) != _lread(fh,(LPSTR)&bf,sizeof(bf)))
		return FALSE;

	// Do we have a RC HEADER?
	if (!ISDIB(bf.bfType))
	{
		bf.bfOffBits = 0L;
		_llseek(fh,off,SEEK_SET);
	}

	// Read bitmap info header.
	if (sizeof(bi) != _lread(fh,(LPSTR)&bi,sizeof(bi)))
		return FALSE;

	nNumColors = DibNumColors(&bi);

	// Determine type of bitmap info (BITMAPINFOHEADER or BITMAPCOREHEADER).
	switch (size = (int)bi.biSize)
	{
		case sizeof(BITMAPINFOHEADER):
			break;

		case sizeof(BITMAPCOREHEADER):

			/* Fill in the missing fields and seek back to the start of
			 * the color table.
			 */
			bc = *(BITMAPCOREHEADER*)&bi;
			bi.biSize               = sizeof(BITMAPINFOHEADER);
			bi.biWidth              = (DWORD)bc.bcWidth;
			bi.biHeight             = (DWORD)bc.bcHeight;
			bi.biPlanes             =  (WORD)bc.bcPlanes;
			bi.biBitCount           =  (WORD)bc.bcBitCount;
			bi.biCompression        = BI_RGB;
			bi.biSizeImage          = 0;
			bi.biXPelsPerMeter      = 0;
			bi.biYPelsPerMeter      = 0;
			bi.biClrUsed            = nNumColors;
			bi.biClrImportant       = nNumColors;

			_llseek(fh, (LONG)sizeof(BITMAPCOREHEADER)-
							  sizeof(BITMAPINFOHEADER), SEEK_CUR);
			break;

		default:
			return NULL;       /* not a DIB */
	}

	// Fill in some default values.
	if(bi.biSizeImage == 0)
	{
		bi.biSizeImage = WIDTHBYTES((DWORD)bi.biWidth * bi.biBitCount) *
							bi.biHeight;
	}

	if(bi.biClrUsed == 0)
	{
		bi.biClrUsed = DibNumColors(&bi);
	}

	// Allocate space for the bitmap info header and color table.
	hbi = GlobalAlloc(GMEM_MOVEABLE,
					  (LONG)bi.biSize + nNumColors * sizeof(RGBQUAD));
	if (!hbi)
		return NULL;

	lpbi = (LPVOID)GlobalLock(hbi);

	*lpbi = bi;                     // Copy the bitmap header information.

	// Read the color table, if it exists. */
	if(nNumColors)
	{
		pRgb = (RGBQUAD FAR *)((LPSTR)lpbi + bi.biSize);
		if (size == sizeof(BITMAPCOREHEADER))
		{
			// Read an old color table (with 3-byte entries) and convert
			// to the new color table format (with 4-byte entries).
			_lread(fh,(LPSTR)pRgb,nNumColors * sizeof(RGBTRIPLE));

			for (i=nNumColors-1; i>=0; i--)
			{
				RGBQUAD rgb;

				rgb.rgbRed      = ((RGBTRIPLE FAR *)pRgb)[i].rgbtRed;
				rgb.rgbBlue     = ((RGBTRIPLE FAR *)pRgb)[i].rgbtBlue;
				rgb.rgbGreen    = ((RGBTRIPLE FAR *)pRgb)[i].rgbtGreen;
				rgb.rgbReserved = (BYTE)0;

				pRgb[i] = rgb;
			}
		}
		else
		{
			// Read a new color table.
			_lread(fh,(LPSTR)pRgb,nNumColors * sizeof(RGBQUAD));
		}
	}

	// Seek to the start of the bitmap data.
	if (bf.bfOffBits != 0L)
		_llseek(fh, off + bf.bfOffBits, SEEK_SET);

	GlobalUnlock(hbi);
	return hbi;
}



// PaletteSize(pv)
//
// Returns the size of the palette in bytes. The <pv> parameter can point
// to a BITMAPINFOHEADER or BITMAPCOREHEADER structure.
// 
//-----------------------------------------------------------------------------
WORD PaletteSize(LPVOID pv)
{
	#define lpbi ((LPBITMAPINFOHEADER)pv)

	WORD    NumColors;

	NumColors = DibNumColors(lpbi);

	if (lpbi->biSize == sizeof(BITMAPCOREHEADER))
		return NumColors * sizeof(RGBTRIPLE);
	else
		return NumColors * sizeof(RGBQUAD);

	#undef lpbi
}



// DibNumColors(pv)
//
// Returns the number of palette entries in the palette. The <pv> parameter
// can point to a BITMAPINFOHEADER or BITMAPCOREHEADER structure.
//
// Parameter: Pointer to the bitmap info header structure
//
//-----------------------------------------------------------------------------
WORD DibNumColors(LPVOID pv)
{
	#define lpbi ((LPBITMAPINFOHEADER)pv)
	#define lpbc ((LPBITMAPCOREHEADER)pv)

	int nBitCount;

	/* With a BITMAPINFOHEADER structure, the number of palette entries
	 * is in biClrUsed; otherwise, the count depends on the number of bits
	 * per pixel.
	 */
	if (lpbi->biSize != sizeof(BITMAPCOREHEADER))
	{
		if(lpbi->biClrUsed != 0)
			return (WORD)lpbi->biClrUsed;

		nBitCount = lpbi->biBitCount;
	}
	else
	{
		nBitCount = lpbc->bcBitCount;
	}

	switch (nBitCount)
	{
		case 1:
			return 2;

		case 4:
			return 16;

		case 8:
			return 256;

		default:
			return 0;
	}

	#undef lpbi
	#undef lpbc
}


// DibFromBitmap()
//
// Creates a global memory block in DIB format that represents the device-
// dependent bitmap passed in. Returns the handle to the DIB.
// 
//-----------------------------------------------------------------------------
HANDLE DibFromBitmap
	(
		HBITMAP     hbm,        // Device-dependent bitmap to copy
		DWORD       biStyle,    // New DIB: Format of bitmap bits (BI_RGB, etc.)
		WORD        biBits,     // New DIB: Bits per pixel
		HPALETTE    hpal,       // New DIB: Palette or NULL to use system palette
		WORD        wUsage      // New DIB: DIB palette usage (DIB_RGB_COLORS or
	)                           //   DIB_PAL_COLORS)
{                     
	BITMAP               bm;
	BITMAPINFOHEADER     bi;
	BITMAPINFOHEADER FAR *lpbi;
	DWORD                dwLen;
	int                  nColors;
	HANDLE               hdib;
	HDC                  hdc;

	if (!hbm)
		return NULL;

	if (wUsage == 0)
		wUsage = DIB_RGB_COLORS;

	if (biStyle == BI_RGB && wUsage == DIB_RGB_COLORS)
		return CreateLogicalDib(hbm,biBits,hpal);

	if (hpal == NULL)
		hpal = GetStockObject(DEFAULT_PALETTE);

	GetObject(hbm,sizeof(bm),(LPSTR)&bm);
	GetObject(hpal,sizeof(nColors),(LPSTR)&nColors);

	if (biBits == 0)
		biBits = bm.bmPlanes * bm.bmBitsPixel;

	bi.biSize               = sizeof(BITMAPINFOHEADER);
	bi.biWidth              = bm.bmWidth;
	bi.biHeight             = bm.bmHeight;
	bi.biPlanes             = 1;
	bi.biBitCount           = biBits;
	bi.biCompression        = biStyle;
	bi.biSizeImage          = 0;
	bi.biXPelsPerMeter      = 0;
	bi.biYPelsPerMeter      = 0;
	bi.biClrUsed            = 0;
	bi.biClrImportant       = 0;

	dwLen  = bi.biSize + PaletteSize(&bi);

	hdc = CreateCompatibleDC(NULL);
	hpal = SelectPalette(hdc,hpal,FALSE);
	RealizePalette(hdc);

	hdib = GlobalAlloc(GMEM_MOVEABLE,dwLen);

	if (!hdib)
		goto exit;

	// Calculate the biSizeImage field by calling GetDIBits with a NULL lpBits param.
	lpbi = (LPVOID)GlobalLock(hdib);

	*lpbi = bi;
	GetDIBits(hdc, hbm, 0, (WORD)bi.biHeight,
				NULL, (LPBITMAPINFO)lpbi, wUsage);
	bi = *lpbi;

	GlobalUnlock(hdib);

	// If the driver did not fill in the biSizeImage field, make one up
	if (bi.biSizeImage == 0)
	{
		bi.biSizeImage = WIDTHBYTES((DWORD)bm.bmWidth * biBits) * bm.bmHeight;

		if (biStyle != BI_RGB)
			bi.biSizeImage = (bi.biSizeImage * 3) / 2;
	}

	// Realloc the buffer big enough to hold all the bits
	dwLen = bi.biSize + PaletteSize(&bi) + bi.biSizeImage;
	hdib = GlobalReAlloc(hdib,dwLen,0);

	if (!hdib)
	{
		GlobalFree(hdib);
		hdib = NULL;
		goto exit;
	}

	// Get the bitmap bits.
	lpbi = (LPVOID)GlobalLock(hdib);

	GetDIBits(hdc, hbm, 0, (WORD)bi.biHeight,
				(LPSTR)lpbi + (WORD)lpbi->biSize + PaletteSize(lpbi),
				(LPBITMAPINFO)lpbi, wUsage);

	GlobalUnlock(hdib);

exit:
	SelectPalette(hdc,hpal,FALSE);
	DeleteDC(hdc);
	return hdib;
}




//  BitmapFromDib()
//
//  Creates a DDB (Device Dependent Bitmap) given a global handle to
//  a memory block in CF_DIB format. Returns a handle to the DDB.
// 
//-----------------------------------------------------------------------------
HBITMAP BitmapFromDib
	(
		HANDLE      hdib,   // Handle to original DIB
		HPALETTE    hpal,   // Handle to original DIB palette
		WORD        wUsage  // Original DIB palette-entry usage
	)
{
	LPBITMAPINFOHEADER lpbi;
	HPALETTE    hpalT;
	HDC         hdc;
	HBITMAP     hbm;

	if (!hdib)
		return NULL;

	if (wUsage == 0)
		wUsage = DIB_RGB_COLORS;

	lpbi = (LPVOID)GlobalLock(hdib);

	if (!lpbi)
		return NULL;

	hdc = GetDC(NULL);

	if (hpal)
	{
		hpalT = SelectPalette(hdc,hpal,FALSE);
		RealizePalette(hdc);
	}

	hbm = CreateDIBitmap(hdc,
			  (LPBITMAPINFOHEADER)lpbi,
			  (LONG)CBM_INIT,
			  (LPSTR)lpbi + lpbi->biSize + PaletteSize(lpbi),
			  (LPBITMAPINFO)lpbi,
			  wUsage );

	if (hpal && hpalT)
		SelectPalette(hdc,hpalT,FALSE);

	ReleaseDC(NULL,hdc);

	GlobalUnlock(hdib);
	return hbm;
}




//  DibFromDib()
//
//  Converts a DIB to the specified format.
//
//-----------------------------------------------------------------------------
HANDLE DibFromDib
	(
		HANDLE      hdib,       // DIB to convert
		DWORD       biStyle,    // New DIB: Compression format
		WORD        biBits,     // New DIB: Bits per pixel
		HPALETTE    hpal,       // New DIB: Palette (or NULL to use existing palette)
		WORD        wUsage      // New DIB: Palette usage
	)
{
	BITMAPINFOHEADER bi;
	HBITMAP     hbm;
	BOOL        fKillPalette=FALSE;

	if (!hdib)
		return NULL;

	// Determine whether the current format matches the requested format.
	DibInfo(hdib,&bi);

	if (bi.biCompression == biStyle && bi.biBitCount == biBits)
		return hdib;

	// If no palette is specified, use the existing DIB palette. 
	if (hpal == NULL)
	{
		hpal = CreateDibPalette(hdib);
		fKillPalette++;
	}

	// First create a DDB from the DIB. 
	hbm = BitmapFromDib(hdib,hpal,wUsage);

	if (hbm == NULL)
	{
		hdib = NULL;
	}
	else
	{
		// Then create the DIB from the DDB.
		hdib = DibFromBitmap(hbm,biStyle,biBits,hpal,wUsage);
		DeleteObject(hbm);
	}

	// Delete palette, if we created it.
	if (fKillPalette && hpal)
		DeleteObject(hpal);

	return hdib;
}


//  CreateLogicalDib(hbm, biBits, hPal)
//
//  Given a DDB and a HPALETTE create a "logical" DIB. If the HPALETTE is
//  NULL, it uses the system palette.
//
//  A "logical" DIB is a DIB where the DIB color table *exactly* matches
//  the passed logical palette.  There will be no system colors in the
//  color table, and a pixel value of <n> in the DIB corresponds to logical
//  palette index <n>.
//
//  Why create a "logical" DIB? When the DIB is written to a disk file and
//  then reloaded, the logical palette created from the DIB color table 
//  exactly matches the one used originaly to create the bitmap. It also
//  prevents GDI from doing nearest color matching on PC_RESERVED palettes.
//
//  To create the logical DIB, we call GetDIBits() with the DIB_PAL_COLORS
//  option. We then convert the palette indices returned in the color table
//  to logical RGB values.  The entire logical palette passed to <hpal> 
//  is always copied to the DIB color table.
//
//  The DIB color table will have exactly the same number of entries as
//  the logical palette.  Normally GetDIBits() sets the biClrUsed field to
//  the maximum colors supported by the device, regardless of the number of
//  colors in the logical palette. If the logical palette contains more 
//  than 256 colors, the function truncates the color table at 256 entries.
// 
//-----------------------------------------------------------------------------
HANDLE CreateLogicalDib
	(
		HBITMAP     hbm,    // DDB to copy
		WORD        biBits, // New DIB: bit count: 8, 4, or 0
		HPALETTE    hpal    // New DIB: palette
	)
{
	LPBITMAPINFOHEADER  lpbiDDB;      // Temporary pointer to DDB BITMAPINFO
	WORD FAR *          lpDDBClrTbl;  // Pointer to DIB color table

	HANDLE              hLDib;
	LPBITMAPINFOHEADER  lpLDib;       // Pointer to logical DIB header
	BYTE FAR *          lpLDibBits;   // Pointer to logical DIB bits
	RGBQUAD FAR *       lpLDibRGB;    // Pointer to logical DIB color table
	int                 nLDibColors;  // How many colors in logical DIB
	DWORD               dwLDibLen;    // Size of logical DIB

	HDC                 hdc;          // Temp stuff, working variables
	BITMAP              bm;
	BITMAPINFOHEADER    bi;
	PALETTEENTRY        peT;
	DWORD               dw;
	int                 n;
	HPALETTE            hpalT;

	if (hbm == NULL)
		return NULL;

	if (hpal == NULL)
		hpal = GetStockObject(DEFAULT_PALETTE);

	GetObject(hpal,sizeof(nLDibColors),(LPSTR)&nLDibColors);
	GetObject(hbm,sizeof(bm),(LPSTR)&bm);

	// Truncate palette entries at 256 if the logical palette has more
	// than 256 entries.
	if (nLDibColors > 256)
		nLDibColors = 256;

	// If bit count is zero, fill in bit count based on number of colors
	// in palette.
	if (biBits == 0)
		biBits = nLDibColors > 16 ? 8 : 4;

	bi.biSize               = sizeof(BITMAPINFOHEADER);
	bi.biWidth              = bm.bmWidth;
	bi.biHeight             = bm.bmHeight;
	bi.biPlanes             = 1;
	bi.biBitCount           = biBits;
	bi.biCompression        = BI_RGB;
	bi.biSizeImage          = WIDTHBYTES((DWORD)bm.bmWidth * biBits) * 
								bm.bmHeight;
	bi.biXPelsPerMeter      = 0;
	bi.biYPelsPerMeter      = 0;
	bi.biClrUsed            = nLDibColors;
	bi.biClrImportant       = 0;

	dwLDibLen = bi.biSize + PaletteSize(&bi) + bi.biSizeImage;

	hLDib = GlobalAlloc(GMEM_MOVEABLE,dwLDibLen);
									
	if (!hLDib)
		return NULL;

	lpbiDDB = MAKEP(GlobalAlloc(GMEM_FIXED,
					bi.biSize + 256 * sizeof(RGBQUAD)),0);

	if (!lpbiDDB)
	{
		GlobalFree(hLDib);
		return NULL;
	}

	hdc = GetDC(NULL);
	hpalT = SelectPalette(hdc,hpal,FALSE);
	RealizePalette(hdc);

	lpLDib = (LPVOID)GlobalLock(hLDib);

	*lpbiDDB  = bi;
	*lpLDib = bi;

	// Get the DIB bits. With DIB_PAL_COLORS flag, the color table is
	// filled with logical palette indices.
	lpLDibBits = (LPSTR)lpLDib + (WORD)lpLDib->biSize + PaletteSize(lpLDib);

	GetDIBits(hdc,                    // Device context
			  hbm,                    // Bitmap we're copying
			  0,                      // Starting scan line
			  (WORD)bi.biHeight,      // Number of scan lines to copy
			  lpLDibBits,             // Receives bitmap bits in DIB format
			  (LPBITMAPINFO)lpbiDDB,  // Receives DDB color table
			  DIB_PAL_COLORS);        // Usage--copy indices into the 
									  // currently realized logical palette

	// Convert the DIB bits from indices into the color table (which
	// contains indices into the logical palette) to direct indices
	// into the logical palette.
	//
	// lpDDBClrTbl   Points to the DIB color table, which is a WORD array of
	//               logical palette indices.
	//
	// lpLDibBits    Points to the DIB bits. Each DIB pixel is a index into
	//               the DIB color table.
	//
	lpDDBClrTbl = (WORD FAR *)((LPSTR)lpbiDDB + (WORD)lpbiDDB->biSize);

	if (biBits == 8)
	{
		for (dw = 0; dw < bi.biSizeImage; dw++, ((BYTE HUGE*)lpLDibBits)++)
			*lpLDibBits = (BYTE)lpDDBClrTbl[*lpLDibBits];
	}
	else // biBits == 4
	{
		for (dw = 0; dw < bi.biSizeImage; dw++, ((BYTE HUGE*)lpLDibBits)++)
			*lpLDibBits = (BYTE)(lpDDBClrTbl[*lpLDibBits & 0x0F] |
							 (lpDDBClrTbl[(*lpLDibBits >> 4) & 0x0F] << 4));
	}

	/* Now copy the RGBs in the logical palette to the DIB color table.
	 */
	lpLDibRGB = (RGBQUAD FAR *)((LPSTR)lpLDib + (WORD)lpLDib->biSize);

	for (n=0; n<nLDibColors; n++, lpLDibRGB++)
	{
		GetPaletteEntries(hpal,n,1,&peT);

		lpLDibRGB->rgbRed      = peT.peRed;
		lpLDibRGB->rgbGreen    = peT.peGreen;
		lpLDibRGB->rgbBlue     = peT.peBlue;
		lpLDibRGB->rgbReserved = (BYTE)0;
	}

	GlobalUnlock(hLDib);
	GlobalFree((HGLOBAL)HIWORD((DWORD)lpbiDDB));

	SelectPalette(hdc,hpalT,FALSE);
	ReleaseDC(NULL,hdc);

	return hLDib;
}

// StretchAllBitmap(hdc, x, y, dx, dy, hbm, rop)
//
// Draws All bitmap <hbm> at the specifed position in DC <hdc>, stretching
// or compressing the bitmap as necessary to fit the dimensions of the
// destination rectangle.
// 
//-----------------------------------------------------------------------------
BOOL StretchAllBitmap
	(
		HDC     hdc,                // Destination device context
		int     x,      int y,      // Upper-left corner of destination rect
		int     dx,     int dy,     // Destination rect extents
		HBITMAP hbm,                // Bitmap to display
		DWORD   rop                 // Raster operation
	)
{
	BITMAP  bm;
	GetObject(hbm,sizeof(bm),(LPSTR)&bm);

	return StretchBitmap(hdc, x, y, dx, dy, hbm, 0, 0, bm.bmWidth, bm.bmHeight, rop);
}


// StretchBitmap(hdc, x, y, dx, dy, hbm, xSrc, ySrc, dxSrc, dySrc, rop)
//
// Draws bitmap <hbm> at the specifed position in DC <hdc>, stretching
// or compressing the bitmap as necessary to fit the dimensions of the
// destination rectangle.
// 
//-----------------------------------------------------------------------------
BOOL StretchBitmap
	(
		HDC     hdc,                // Destination device context
		int     x,      int y,      // Upper-left corner of destination rect
		int     dx,     int dy,     // Destination rect extents
		HBITMAP hbm,                // Bitmap to display
		int     xSrc,   int ySrc,   // Upper-left corner of source rect
		int     dxSrc,  int dySrc,  // Source rect extents
		DWORD   rop                 // Raster operation
	)
{
	HDC hdcBits;
	HPALETTE hpal,hpalT;
	BOOL f;

	if (!hdc || !hbm)
		return FALSE;

	hpal = SelectPalette(hdc,GetStockObject(DEFAULT_PALETTE),FALSE);
	SelectPalette(hdc,hpal,FALSE);

	hdcBits = CreateCompatibleDC(hdc);
	SelectObject(hdcBits,hbm);
	hpalT = SelectPalette(hdcBits,hpal,FALSE);
	RealizePalette(hdcBits);
	f = StretchBlt(hdc,x,y,dx,dy,hdcBits,xSrc,ySrc,dxSrc,dySrc,rop);
	SelectPalette(hdcBits,hpalT,FALSE);
	DeleteDC(hdcBits);

	return f;
}

//
// FloodDrawBitmap(hdc, x, y, hbm, rop, fillColor, clrref)
//
// Draws bitmap <hbm> at the specifed position in DC <hdc>, setting as background
// color same as fillColor
// 
//-----------------------------------------------------------------------------
BOOL FloodDrawBitmap
	(
		HDC     	hdc,    	// Destination device context
		int     	x,
		int    		y,      	// Upper-left corner of destination rect
		HBITMAP 	hbm,    	// Bitmap to display
		DWORD   	rop,    	// Raster operation
		COLORREF	fillColor,	// fill color
		COLORREF	clrref		// color surface
	)
{                        
	HDC		hdcBits;
	BITMAP	bm;
	HBRUSH	hBrush1, hbrOld;
	BOOL	f;

	if (!hdc || !hbm)
		return FALSE;

	hdcBits = CreateCompatibleDC(hdc);
	hBrush1 = CreateSolidBrush(fillColor);
	hbrOld  = SelectObject(hdcBits, hBrush1);
	
	GetObject(hbm,sizeof(BITMAP),(LPSTR)&bm);
	SelectObject(hdcBits, hbm);
	if (fillColor != clrref)
	{
		HBRUSH	hBrush2, hbrOld1;

		ExtFloodFill(hdcBits, 0, 0, clrref, FLOODFILLSURFACE);
		f = BitBlt(hdc,x,y,bm.bmWidth,bm.bmHeight,hdcBits,0,0,rop);

		// restore previous bitmap background color
		hBrush2 = CreateSolidBrush(clrref);
		hbrOld1 = SelectObject(hdcBits, hBrush2);
		ExtFloodFill(hdcBits, 0, 0, fillColor, FLOODFILLSURFACE);
		SelectObject(hdcBits, hbrOld1);
		DeleteObject(hBrush2);
	}
	else
		f = BitBlt(hdc,x,y,bm.bmWidth,bm.bmHeight,hdcBits,0,0,rop);
		
	SelectObject(hdcBits, hbrOld);
	DeleteObject(hBrush1);
	DeleteDC(hdcBits);
	
	return f;
}


// DrawBitmap(hdc, x, y, hbm, DWORD)
//
// Draws bitmap <hbm> at the specifed position in DC <hdc>
//
// 
//-----------------------------------------------------------------------------
BOOL DrawBitmap
	(
		HDC     hdc,    // Destination device context
		int     x,
		int     y,      // Upper-left corner of destination rect
		HBITMAP hbm,    // Bitmap to display
		DWORD   rop     // Raster operation
	)
{
	HDC hdcBits;
	BITMAP bm;
	BOOL f;

	if (!hdc || !hbm)
		return FALSE;

	hdcBits = CreateCompatibleDC(hdc);
	GetObject(hbm,sizeof(BITMAP),(LPSTR)&bm);
	SelectObject(hdcBits,hbm);
	f = BitBlt(hdc,x,y,bm.bmWidth,bm.bmHeight,hdcBits,0,0,rop);
	DeleteDC(hdcBits);

	return f;
}


// SetDibUsage(hdib,hpal,wUsage)
//
// Modifies the color table of the passed DIB for use with the wUsage
// parameter specifed.
//
// if wUsage is DIB_PAL_COLORS the DIB color table is set to 0-256
// if wUsage is DIB_RGB_COLORS the DIB color table is set to the RGB values
//     in the passed palette
// 
//-----------------------------------------------------------------------------
BOOL SetDibUsage
	(
		HANDLE      hdib,   // DIB to modify
		HPALETTE    hpal,   // DIB palette
		WORD        wUsage  // New palette usage
	)
{
	LPBITMAPINFOHEADER lpbi;
	PALETTEENTRY       ape[MAXPALETTE];
	RGBQUAD FAR *      pRgb;
	WORD FAR *         pw;
	int                nColors;
	int                n;

	if (hpal == NULL)
		hpal = GetStockObject(DEFAULT_PALETTE);

	if (!hdib)
		return FALSE;

	lpbi = (LPVOID)GlobalLock(hdib);

	if (!lpbi)
		return FALSE;

	nColors = DibNumColors(lpbi);

	if (nColors > 0)
	{
		pRgb = (RGBQUAD FAR *)((LPSTR)lpbi + (WORD)lpbi->biSize);

		switch (wUsage)
		{
			//
			// Set the DIB color table to palette indexes
			//
			case DIB_PAL_COLORS:
				for (pw = (WORD FAR*)pRgb,n=0; n<nColors; n++,pw++)
					*pw = n;
				break;

			//
			// Set the DIB color table to RGBQUADS
			//
			default:
			case DIB_RGB_COLORS:
				nColors = min(nColors,MAXPALETTE);

				GetPaletteEntries(hpal,0,nColors,ape);

				for (n=0; n<nColors; n++)
				{
					pRgb[n].rgbRed      = ape[n].peRed;
					pRgb[n].rgbGreen    = ape[n].peGreen;
					pRgb[n].rgbBlue     = ape[n].peBlue;
					pRgb[n].rgbReserved = 0;
				}
				break;
		}
	}
	GlobalUnlock(hdib);
	return TRUE;
}


//  SetPalFlags(hpal,iIndex, cnt, wFlags)
//
//  Modifies the palette flags of all indices in the range 
//  (iIndex - nIndex+cnt) to the parameter specifed.
///
BOOL SetPaletteFlags
	(
		HPALETTE    hpal,       // Palette to modify
		int         iIndex,     // Starting index number
		int         cntEntries, // How many entries to modify
		WORD        wFlags      // New flag setting for pal entries
	)
{
	int     i;
	BOOL    f;
	HANDLE  hpe;
	PALETTEENTRY FAR *lppe;

	if (hpal == NULL)
		return FALSE;

	if (cntEntries < 0)
		GetObject(hpal,sizeof(int),(LPSTR)&cntEntries);

	hpe = GlobalAlloc(GMEM_MOVEABLE,(LONG)cntEntries * sizeof(PALETTEENTRY));

	if (!hpe)
		return FALSE;

	lppe = (LPVOID)GlobalLock(hpe);

	GetPaletteEntries(hpal, iIndex, cntEntries, lppe);

	for (i=0; i<cntEntries; i++)
	{
		lppe[i].peFlags = (BYTE)wFlags;
	}

	f = SetPaletteEntries(hpal, iIndex, cntEntries, lppe);

	GlobalUnlock(hpe);
	GlobalFree(hpe);
	return f;
}


// PaletteEq(hpal1,hpal2)
//
// Returns TRUE if the pallettes are the same
// 
//-----------------------------------------------------------------------------
BOOL PaletteEq(HPALETTE hpal1, HPALETTE hpal2)
{
	BOOL    f;
	int     i;
	int     nPal1,nPal2;
	PALETTEENTRY FAR *ppe;

	if (hpal1 == hpal2)
		return TRUE;

	if (!hpal1 || !hpal2)
		return FALSE;

	GetObject(hpal1,sizeof(int),(LPSTR)&nPal1);
	GetObject(hpal2,sizeof(int),(LPSTR)&nPal2);

	if (nPal1 != nPal2)
		return FALSE;

	ppe = ALLOCP(nPal1 * 2 * sizeof(PALETTEENTRY));

	if (!ppe)
		return FALSE;

	GetPaletteEntries(hpal1, 0, nPal1, ppe);
	GetPaletteEntries(hpal2, 0, nPal2, ppe+nPal1);

	for (f=TRUE,i=0; f && i<nPal1; i++)
	{
		f &= (ppe[i].peRed   == ppe[i+nPal1].peRed   &&
			  ppe[i].peBlue  == ppe[i+nPal1].peBlue  &&
			  ppe[i].peGreen == ppe[i+nPal1].peGreen);
	}

	FREEP(ppe);
	return f;
}


// StretchDibBlt(hdc, x, y, dx, dy, hdib, xSrc, ySrc, dxSrc, dySrc, rop, wUsage)
//
// Draws a bitmap in CF_DIB format StretchDIBits(). Uses the same
// parameters as StretchBlt(), except takes a DIB rather than a source
// device context.
// 
//-----------------------------------------------------------------------------
BOOL StretchDibBlt
	(
		HDC     hdc,    // Destination device context
		int     x,
		int     y,      // Destination rect origin
		int     dx,
		int     dy,     // Destination rect extents
		HANDLE  hdib,   // Bitmap to display
		int     xSrc,
		int     ySrc,   // Source rect origin
		int     dxSrc,
		int     dySrc,  // Source rect extents
		DWORD   rop,    // Raster operation
		WORD    wUsage  // Pal usage DIB_PAL_COLORS or DIB_RGB_COLORS
	)
{
	LPBITMAPINFOHEADER lpbi;
	LPSTR        pBuf;
	BOOL         f;

	if (!hdib)
		return PatBlt(hdc,x,y,dx,dy,rop);

	if (wUsage == 0)
		wUsage = DIB_RGB_COLORS;

	lpbi = (LPVOID)GlobalLock(hdib);

	if (!lpbi)
		return FALSE;

	if (dxSrc == -1 && dySrc == -1)
	{
		if (lpbi->biSize == sizeof(BITMAPCOREHEADER))
		{
			dxSrc = ((LPBITMAPCOREHEADER)lpbi)->bcWidth;
			dySrc = ((LPBITMAPCOREHEADER)lpbi)->bcHeight;
		}
		else
		{
			dxSrc = (int)lpbi->biWidth;
			dySrc = (int)lpbi->biHeight;
		}
	}

	if (dx == -1 && dy == -1)
	{
		dx = dxSrc;
		dy = dySrc;
	}

	pBuf = (LPSTR)lpbi + (WORD)lpbi->biSize + PaletteSize(lpbi);

	f = StretchDIBits (
		hdc,
		x,y,
		dx,dy,
		xSrc,ySrc,
		dxSrc,dySrc,
		pBuf, 
		(LPBITMAPINFO)lpbi,
		wUsage,
		rop);

	GlobalUnlock(hdib);
	return f;
}


//  DibBlt(hdc, xDest, yDest, dxDest, dyDest, hdib, xSrc, ySrc, rop, wUsage)
//
//  Draws a bitmap in CF_DIB format, using SetDIBits to device.
//
//  Takes the same parameters as BitBlt(), except takes a DIB instead of
//  a source device context.
// 
//-----------------------------------------------------------------------------
BOOL DibBlt
	(
		HDC     hdc,    // Destination device context
		int     xDest,
		int     yDest,  // Destination rect origin
		int     dxDest,
		int     dyDest, // Destination rect extents
		HANDLE  hdib,   // DIB to draw
		int     xSrc,
		int     ySrc,   // Source rect origin
		DWORD   rop,    // Raster op
		WORD    wUsage  // Pal usage (DIB_PAL_COLORS or DIB_RGB_COLORS)
	)
{
	LPBITMAPINFOHEADER lpbi;
	LPSTR       pBuf;
	BOOL        f;

	if (!hdib)
		return PatBlt(hdc,xDest,yDest,dxDest,dyDest,rop);

	if (wUsage == 0)
		wUsage = DIB_RGB_COLORS;

	lpbi = (LPVOID)GlobalLock(hdib);

	if (!lpbi)
		return FALSE;

	if (dxDest == -1 && dyDest == -1)
	{
		if (lpbi->biSize == sizeof(BITMAPCOREHEADER))
		{
			dxDest = ((LPBITMAPCOREHEADER)lpbi)->bcWidth;
			dyDest = ((LPBITMAPCOREHEADER)lpbi)->bcHeight;
		}
		else
		{
			dxDest = (int)lpbi->biWidth;
			dyDest = (int)lpbi->biHeight;
		}
	}

	pBuf = (LPSTR)lpbi + (WORD)lpbi->biSize + PaletteSize(lpbi);

	f = StretchDIBits (
		hdc,
		xDest,yDest,
		dxDest,dyDest,
		xSrc,ySrc,
		dxDest,dyDest,
		pBuf, (LPBITMAPINFO)lpbi,
		wUsage,
		rop);

	GlobalUnlock(hdib);
	return f;
}


// DibLock(hdib, x, y)
//
// Locks a DIB and returns a pointer to the bitmap bits at the specified
// coordinate.
//
// Parameters:
//      hdib : DIB to lock
//      x, y : Bit coordinates
//
//-----------------------------------------------------------------------------
LPVOID DibLock(HANDLE hdib, int x, int y)
{
	return DibXY((LPBITMAPINFOHEADER)GlobalLock(hdib),x,y);
}


// DibUnlock(hdib)
//
//-----------------------------------------------------------------------------
VOID DibUnlock(HANDLE hdib)
{
	GlobalUnlock(hdib);
}


// DibXY(lpbi, x, y)
//
// Given a BITMAPINFOHEADER and a set of coordinates, returns a pointer
// to the bitmap bits at the specified coordinates.
// 
//-----------------------------------------------------------------------------
LPVOID DibXY(LPBITMAPINFOHEADER lpbi,int x, int y)
{
	BYTE HUGE* pBits;
	DWORD ulWidthBytes;

	pBits = (LPSTR)lpbi + (WORD)lpbi->biSize + PaletteSize(lpbi);
	ulWidthBytes = WIDTHBYTES(lpbi->biWidth*lpbi->biBitCount);
	pBits += (ulWidthBytes * (long)y) + x;

	return (LPVOID)pBits;
}

// CreateDib(hdib, dx, dy)
//
// Creates a DIB with the same header and color table as the original
// DIB, with sufficient space to hold the indicated region. If the <hdib>
// is NULL, creates a DIB header and uninitialized, 256-entry color table.
// 
//-----------------------------------------------------------------------------
HANDLE CreateDib(HANDLE hdib, int dx, int dy)    
{
	HANDLE              hdibN;
	BITMAPINFOHEADER    bi;
	LPBITMAPINFOHEADER  lpbi1;
	LPBITMAPINFOHEADER  lpbi2;
	RGBQUAD FAR *       pRgb1;
	RGBQUAD FAR *       pRgb2;

	// Copy DIB header or create a new one
	if (hdib)
	{
		DibInfo(hdib,&bi);
	}
	else
	{
		bi.biSize           = sizeof(BITMAPINFOHEADER);
		bi.biPlanes         = 1;
		bi.biBitCount       = 8;
		bi.biWidth          = 0;
		bi.biHeight         = 0;
		bi.biCompression    = BI_RGB;
		bi.biSizeImage      = 0;
		bi.biXPelsPerMeter  = 0;
		bi.biYPelsPerMeter  = 0;
		bi.biClrUsed        = 256;
		bi.biClrImportant   = 0;
	}

	// Use specified measurements or original extents
	if (dx != -1)
		bi.biWidth = dx;

	if (dy != -1)
		bi.biHeight = dy;

	bi.biSizeImage = WIDTHBYTES(bi.biWidth*bi.biBitCount) * (long)dy;

	hdibN = GlobalAlloc(GMEM_MOVEABLE,sizeof(BITMAPINFOHEADER) +
				+ (long)bi.biClrUsed * sizeof(RGBQUAD) + bi.biSizeImage);

	//  Copy the color table across
	if (hdibN && hdib)
	{
		lpbi1 = (LPVOID)GlobalLock(hdibN);
		lpbi2 = (LPVOID)GlobalLock(hdib);

		*lpbi1 = bi;

		pRgb1 = (LPVOID)((LPSTR)lpbi1 + lpbi1->biSize);
		pRgb2 = (LPVOID)((LPSTR)lpbi2 + lpbi2->biSize);

		while (bi.biClrUsed-- > 0)
			*pRgb1++ = *pRgb2++;

		GlobalUnlock(hdib);
		GlobalUnlock(hdibN);
	}
	return hdibN;
}


// Private routines to read/write more than 64k

//#define MAXREAD (WORD)(32 * 1024)
WORD const MAXREAD = 32768;

//-----------------------------------------------------------------------------
static DWORD NEAR PASCAL lread(HFILE fh, LPVOID pv, DWORD ul)
{
	DWORD		ulT = ul;
	BYTE HUGE*	hp = pv;

	while (ul > MAXREAD) {
		if (_lread(fh, (LPSTR)hp, MAXREAD) != MAXREAD)
				return 0;
		ul -= MAXREAD;
		hp += MAXREAD;
	}
	if (_lread(fh, (LPSTR)hp, (WORD)ul) != (WORD)ul)
		return 0;
	return ulT;
}

//-----------------------------------------------------------------------------
static DWORD NEAR PASCAL lwrite(HFILE fh, LPVOID pv, DWORD ul)
{
	DWORD		ulT = ul;
	BYTE HUGE*	hp = pv;

	while (ul > MAXREAD) {
		if (_lwrite(fh, (LPSTR)hp, MAXREAD) != MAXREAD)
				return 0;
		ul -= MAXREAD;
		hp += MAXREAD;
	}
	if (_lwrite(fh, (LPSTR)hp, (WORD)ul) != (WORD)ul)
		return 0;
	return ulT;
}



// Copy handle aH in a new handle and returns it
//
//------------------------------------------------------------------------------
HANDLE CopyHandle(HANDLE aH)
{
	BYTE HUGE*	lpCopy;
	BYTE HUGE*	lp;
	HANDLE		hCopy;
	DWORD		dwLen;

	if (aH == NULL)  
		return NULL;

	dwLen = GlobalSize((HGLOBAL) aH);
	hCopy = (HANDLE) GlobalAlloc (GHND, dwLen);

	if (hCopy == NULL)
		return NULL;

	lpCopy = (BYTE HUGE*) GlobalLock((HGLOBAL) hCopy);
	lp     = (BYTE HUGE*) GlobalLock((HGLOBAL) aH);
    	
	HMEMCPY(lpCopy, lp, dwLen);
		
	GlobalUnlock((HGLOBAL) hCopy);
	GlobalUnlock((HGLOBAL) aH);
	
	return hCopy;
}

//------------------------------------------------------------------------------
LPSTR FindDIBBits (LPSTR lpbi)
{
	return (lpbi + *(LPDWORD)lpbi + PaletteSize (lpbi));
}

// Update the header information of a new DIB bitmap
//
//------------------------------------------------------------------------------
void InitBitmapInfoHeader
	(
		LPBITMAPINFOHEADER	lpBmInfoHdr,
		DWORD				dwWidth,
		DWORD				dwHeight,
		int					nBPP, 
		int nPixelXMeter /*= 0*/, int nPixelYMeter /*= 0*/
	)
{
	memset (lpBmInfoHdr, 0, sizeof (BITMAPINFOHEADER));

	lpBmInfoHdr->biSize		= sizeof (BITMAPINFOHEADER);
	lpBmInfoHdr->biWidth	= dwWidth;
	lpBmInfoHdr->biHeight	= dwHeight;
	lpBmInfoHdr->biPlanes	= 1;

	if (nBPP <= 1)
		nBPP = 1;
	else if (nBPP <= 4)
		nBPP = 4;
	else if (nBPP <= 8)
		nBPP = 8;
	else
		nBPP = 24;

	lpBmInfoHdr->biBitCount		= nBPP;
	lpBmInfoHdr->biSizeImage	= WIDTHBYTES (dwWidth * nBPP) * dwHeight;

	if(nPixelXMeter > 0)
		lpBmInfoHdr->biXPelsPerMeter = nPixelXMeter;
	if(nPixelYMeter > 0)
		lpBmInfoHdr->biYPelsPerMeter = nPixelYMeter;
}

// Make a coversion from DDB bitmap to DIB bitmap
//
//------------------------------------------------------------------------------
HANDLE BitmapToDIB (HBITMAP hBitmap, HPALETTE hPal,
						int					nPixelXMeter /*= 0*/,
						int					nPixelYMeter /*= 0*/)
{
	BITMAP				Bitmap;
	BITMAPINFOHEADER	bmInfoHdr;
	LPBITMAPINFOHEADER	lpbmInfoHdr;
	LPSTR				lpBits;
	HDC					hMemDC;
	HANDLE				hDIB;
	HPALETTE			hOldPal = NULL;

	// Do some setup -- make sure the Bitmap passed in is valid,
	//  get info on the bitmap (like its height, width, etc.),
	//  then setup a BITMAPINFOHEADER.
	if (!hBitmap || !GetObject (hBitmap, sizeof (BITMAP), (LPSTR) &Bitmap))
		return NULL;

	InitBitmapInfoHeader
		(
			&bmInfoHdr,
			Bitmap.bmWidth,
			Bitmap.bmHeight,
			Bitmap.bmPlanes * Bitmap.bmBitsPixel,
			nPixelXMeter, nPixelYMeter
		);

	// Now allocate memory for the DIB.  Then, set the BITMAPINFOHEADER
	//  into this memory, and find out where the bitmap bits go.
	hDIB = GlobalAlloc
		(
			GHND,
			sizeof (BITMAPINFOHEADER) + PaletteSize ((LPSTR) &bmInfoHdr) + bmInfoHdr.biSizeImage
		);

	if (!hDIB)
		return NULL;

	lpbmInfoHdr  = (LPBITMAPINFOHEADER) GlobalLock (hDIB);
	*lpbmInfoHdr = bmInfoHdr;
	lpBits       = FindDIBBits ((LPSTR) lpbmInfoHdr);

	// Now, we need a DC to hold our bitmap.  If the app passed us
	//  a palette, it should be selected into the DC.
	hMemDC       = GetDC (NULL);

	if (hPal)
	{
		hOldPal = SelectPalette (hMemDC, hPal, FALSE);
		RealizePalette (hMemDC);
	}

	// We're finally ready to get the DIB.  Call the driver and let
	//  it party on our bitmap.  It will fill in the color table,
	//  and bitmap bits of our global memory block.
	if (
		!GetDIBits
			(
				hMemDC,
				hBitmap,
				0,
				Bitmap.bmHeight,
				lpBits,
				(LPBITMAPINFO) lpbmInfoHdr,
				DIB_RGB_COLORS
			)
		)
	{
		GlobalUnlock (hDIB);
		GlobalFree (hDIB);
		hDIB = NULL;
	}
	else
		GlobalUnlock (hDIB);

   // clean up and return.
   if (hOldPal)
      SelectPalette (hMemDC, hOldPal, FALSE);

   ReleaseDC (NULL, hMemDC);

   return hDIB;
}
