/************************************************/
/*        Bar Code Library for Windows          */
/*   Copyright (C) 1988-1995, StrandWare, Inc.  */
/*             All Rights Reserved              */
/************************************************/
/* BCLW.H */

#ifndef __BCLW_H
#define __BCLW_H

#include "windows.h"

#ifdef __cplusplus
   extern "C" {
#endif

#define BC_NOSUPPORT    -1
// barCodeType values
#define BC_UPCA         1
#define BC_UPCE         2
#define BC_EAN13		3
#define BC_EAN8		    4
#define BC_EANJAN13		BC_EAN13
#define BC_EANJAN8	    BC_EAN8
#define BC_CODE39       5
#define BC_EXT39        6
#define BC_INT25        7
#define BC_CODE128      8
#define BC_CODABAR      9
#define BC_ZIP          10
#define BC_MSIPLESSEY   11
#define BC_CODE93       12
#define BC_EXT93        13
#define BC_UCC128       14
#define BC_HIBC         15
#define BC_PDF417       16
#define BC_UPCE0        17
#define BC_UPCE1        18
#define BC_CODE128A     19
#define BC_CODE128B     20
#define BC_CODE128C     21
//BC_EAN128			= 22;		  simulato
#define BC_DATAMATRIX	23
#define BC_MicroQR		24
#define BC_QR			25


// alignment values 
#define BA_FARTOPLEFT      0
#define BA_FARTOP          2
#define BA_FARTOPRIGHT     4
#define BA_TOPLEFT         6
#define BA_TOP             7
#define BA_TOPRIGHT        8
#define BA_FARLEFT         10
#define BA_LEFT            11
#define BA_CENTER          12
#define BA_RIGHT           13
#define BA_FARRIGHT        14
#define BA_BOTTOMLEFT      16
#define BA_BOTTOM          17
#define BA_BOTTOMRIGHT     18
#define BA_FARBOTTOMLEFT   20
#define BA_FARBOTTOM       22
#define BA_FARBOTTOMRIGHT  24

// humanWhere values
#define HR_OFF          0
#define HR_BELOWLEFT    1
#define HR_BELOW        2
#define HR_BELOWRIGHT   3
#define HR_ABOVELEFT    4
#define HR_ABOVE        5
#define HR_ABOVERIGHT   6

// humanStyle
#define HS_NORMAL       0
#define HS_BOLD         1
#define HS_ITALIC       2
#define HS_BOLDITALIC   3

// fileType
#define FF_BMP          0

// Error Codes
#define ERR_OKAY                  0
#define ERR_BADBARTYPE            -1
#define ERR_DATALIMIT             -2
#define ERR_HRLIMIT               -3
#define ERR_INVALIDHDC            -4
#define ERR_BADBARHEIGHT          -6
#define ERR_BADBARCHKSUM          -7
#define ERR_BARGENERATION         -9
#define ERR_BADROTATION           -11
#define ERR_BADHUMANWHERE         -12
#define ERR_BADRATIO              -13
#define ERR_BADSUPPLEMENT         -14
#define ERR_BADHRSTYLE            -15
#define ERR_NODATA                -16
#define ERR_BADALIGNMENT          -17
#define ERR_BARCODEBITMAP         -20
#define ERR_CLIPBOARD             -21
#define ERR_OPENFILE              -22
#define ERR_SAVEFILE              -23
#define ERR_FILETYPE              -24

#define ERR_BADSECURITY           -30
#define ERR_PDFDATALIMIT          -31
#define ERR_PDFNODATA             -32
#define ERR_PDFUNSUPPORTEDMODE    -33

// Barcode Handle
typedef long hBC;

// Structures
typedef struct LOGBARCODE
{
   int barCodeType;
   int rotation;
   COLORREF color;
   int barWidth;
   int barHeight;
   
   int humanWhere;
   char humanFaceName[LF_FACESIZE];    // Only True Type fonts may be requested
   int humanSize;                      // Does not apply to notched bar codes
   int humanStyle;                     // Only works with True Types that have a matching native style

   int supplement;
   int ratio;
   int checkSum;         // pre-defined barCodeType-dependant software-generated checkSum

   int pdfSecurity;      // for PDF417 Only

} FAR* LPLOGBARCODE;


// Functions

hBC EXPORT CALLBACK CreateBarCode(
   int barCodeType,
   int rotation,
   COLORREF color,
   int barWidth,
   int barHeight,
   
   int humanWhere,
   LPSTR humanFaceName,
   int humanSize,
   int humanStyle,

   int supplement,
   int ratio,
   int checkSum,
   int pdfSecurity
);

hBC EXPORT CALLBACK CreateBarCodeIndirect(
   LPLOGBARCODE lpbc);

int EXPORT CALLBACK DeleteBarCode(
   hBC bc);

DWORD EXPORT CALLBACK GetBarCodeSize(
   HDC hdc,                // may NOT be a metafile or bitmap
   hBC lpBarCode,          // bar code definition
   LPCSTR lpCodeText,      // text to encode
   LPCSTR lpHumanText);    // text to use in the human readable


int EXPORT CALLBACK DrawBarCodeToHDC(
   HDC hdc,                // may not be a metafile or bitmap
   int x,                  // in hdc logical units
   int y,                  // in hdc logical units
   int alignment,          // where on the bar code x and y specify

   hBC lpBarCode,          // bar code definition
   LPCSTR lpCodeText,      // text to encode
   LPCSTR lpHumanText);    // text to use in the human readable


int EXPORT CALLBACK DrawBarCodeToClipboard(
   int xdpi,               //   to override granularity for wysiwyg - !!not a scale!!
   int ydpi,

   hBC lpBarCode,          // bar code definition
   LPCSTR lpCodeText,      // text to encode
   LPCSTR lpHumanText);    // text to use in the human readable


int EXPORT CALLBACK DrawBarCodeToFile(
   LPCSTR lpFileName,
   int fileType,
   int xdpi,               //   to override granularity for wysiwyg - !!not a scale!!
   int ydpi,

   hBC lpBarCode,          // bar code definition
   LPCSTR lpCodeText,      // text to encode
   LPCSTR lpHumanText);    // text to use in the human readable


#ifdef __cplusplus
   }
#endif

#endif //__BCLW_H

