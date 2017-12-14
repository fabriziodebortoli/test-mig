#pragma once

#include "beginh.dex"

#ifdef __cplusplus
extern "C" {
#endif

	//includere alla fine degli include del .H

//=============================================================================
#define BFT_ICON   0x4349   /* 'IC' */
#define BFT_BITMAP 0x4d42   /* 'BM' */
#define BFT_CURSOR 0x5450   /* 'PT' */

#define ISDIB(bft) ((bft) == BFT_BITMAP)
#define ALIGNULONG(i)   ((i+3)/4*4)        /* ULONG aligned ! */
#define WIDTHBYTES(i)   ((i+31)/32*4)      /* ULONG aligned ! */

#define PALVERSION      0x300
#define MAXPALETTE      256


TB_EXPORT extern HANDLE   ReadDIB         (HFILE fh);
TB_EXPORT extern BOOL     WriteDIB        (HFILE fh,HANDLE hdib);
TB_EXPORT extern WORD     PaletteSize     (LPVOID pv);
TB_EXPORT extern WORD     DibNumColors    (LPVOID pv);
TB_EXPORT extern HPALETTE CreateDibPalette(HANDLE hdib);
TB_EXPORT extern HPALETTE CreateBIPalette (LPBITMAPINFOHEADER lpbi);
TB_EXPORT extern HANDLE   DibFromBitmap   (HBITMAP hbm, DWORD biStyle, WORD biBits, HPALETTE hpal, WORD wUsage);
TB_EXPORT extern HANDLE   DibFromDib      (HANDLE hdib, DWORD biStyle, WORD biBits, HPALETTE hpal, WORD wUsage);
TB_EXPORT extern HBITMAP  BitmapFromDib   (HANDLE hdib, HPALETTE hpal, WORD wUsage);
TB_EXPORT extern BOOL     SetDibUsage     (HANDLE hdib, HPALETTE hpal,WORD wUsage);
TB_EXPORT extern BOOL     DibInfo         (HANDLE hdib,LPBITMAPINFOHEADER lpbi);
TB_EXPORT extern HANDLE   ReadDibBitmapInfo(HFILE fh);
TB_EXPORT extern BOOL     SetPaletteFlags (HPALETTE hpal, int iIndex, int cntEntries, WORD wFlags);
TB_EXPORT extern BOOL     PaletteEq       (HPALETTE hpal1, HPALETTE hpal2);

TB_EXPORT extern BOOL     DrawBitmap      (HDC hdc, int x, int y, HBITMAP hbm, DWORD rop);
TB_EXPORT extern BOOL     StretchAllBitmap(HDC hdc, int x, int y, int dx, int dy, HBITMAP hbm, DWORD rop);
TB_EXPORT extern BOOL     StretchBitmap   (HDC hdc, int x, int y, int dx, int dy, HBITMAP hbm, int x0, int y0, int dx0, int dy0, DWORD rop);

TB_EXPORT extern BOOL     DibBlt          (HDC hdc, int x0, int y0, int dx, int dy, HANDLE hdib, int x1, int y1, DWORD rop, WORD wUsage);
TB_EXPORT extern BOOL     StretchDibBlt   (HDC hdc, int x, int y, int dx, int dy, HANDLE hdib, int x0, int y0, int dx0, int dy0, DWORD rop, WORD wUsage);
TB_EXPORT extern BOOL 	FloodDrawBitmap	(HDC hdc, int x, int y, HBITMAP hbm, DWORD rop, COLORREF fillColor, COLORREF clrref);

TB_EXPORT extern LPVOID   DibLock         (HANDLE hdib,int x, int y);
TB_EXPORT extern VOID     DibUnlock       (HANDLE hdib);
TB_EXPORT extern LPVOID   DibXY           (LPBITMAPINFOHEADER lpbi,int x, int y);
TB_EXPORT extern HANDLE   CreateDib       (HANDLE hdib, int dx, int dy);

TB_EXPORT extern	HANDLE	CopyHandle(HANDLE aH);
TB_EXPORT extern	LPSTR	FindDIBBits (LPSTR lpbi);
TB_EXPORT extern	HANDLE	BitmapToDIB (HBITMAP hBitmap, HPALETTE hPal, int nPixelXMeter, int nPixelYMeter);
TB_EXPORT extern	void	InitBitmapInfoHeader
					(
						LPBITMAPINFOHEADER	lpBmInfoHdr,
						DWORD				dwWidth,
						DWORD				dwHeight,
						int					nBPP,
						int					nPixelXMeter,
						int					nPixelYMeter
					);


#ifdef __cplusplus
}
#endif

#include "endh.dex"
