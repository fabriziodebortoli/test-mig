#pragma once


HBRUSH  WINAPI _TBNAME(CreateBrushIndirect)( __in CONST LOGBRUSH *plbrush);
HBRUSH  WINAPI _TBNAME(CreateHatchBrush)( __in int iHatch, __in COLORREF color);
HBRUSH  WINAPI _TBNAME(CreatePatternBrush)( __in HBITMAP hbm);
HBRUSH  WINAPI _TBNAME(CreateSolidBrush)( __in COLORREF color);
HPEN WINAPI _TBNAME(CreatePen)( __in int iStyle, __in int cWidth, __in COLORREF color);
HPEN WINAPI _TBNAME(CreatePenIndirect)( __in CONST LOGPEN *plpen);
HFONT WINAPI _TBNAME(CreateFontW)( __in int cHeight, __in int cWidth, __in int cEscapement, __in int cOrientation, __in int cWeight, __in DWORD bItalic,__in DWORD bUnderline, __in DWORD bStrikeOut, __in DWORD iCharSet, __in DWORD iOutPrecision, __in DWORD iClipPrecision,__in DWORD iQuality, __in DWORD iPitchAndFamily, __in_opt LPCWSTR pszFaceName);
HFONT WINAPI _TBNAME(CreateFontIndirectW)( __in CONST LOGFONTW *lplf);
HFONT WINAPI _TBNAME(CreateFontIndirectExW)( __in CONST ENUMLOGFONTEXDVW *);
HDC WINAPI _TBNAME(CreateCompatibleDC)(__in HDC hDC);
int  WINAPI _TBNAME(GetDeviceCaps)( __in_opt HDC hdc, __in int index);
BOOL WINAPI _TBNAME(GetTextExtentPoint32W)(__in HDC hdc, __in_ecount(c) LPCWSTR lpString,__in int c, __out LPSIZE psizl);
BOOL WINAPI _TBNAME(GetTextMetricsW)( __in HDC hdc, __out LPTEXTMETRIC lptm);
BOOL WINAPI _TBNAME(DeleteDC)(__in HDC hDC);
BOOL WINAPI _TBNAME(DeleteObject)( __in HGDIOBJ ho);