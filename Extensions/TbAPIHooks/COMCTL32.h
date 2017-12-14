#pragma once


HIMAGELIST  WINAPI _TBNAME(ImageList_Create)(int cx, int cy, UINT flags, int cInitial, int cGrow);
BOOL        WINAPI _TBNAME(ImageList_Destroy)(__in_opt HIMAGELIST himl);
int         WINAPI _TBNAME(ImageList_GetImageCount)(__in HIMAGELIST himl);
BOOL        WINAPI _TBNAME(ImageList_SetImageCount)(__in HIMAGELIST himl, __in UINT uNewCount);
int         WINAPI _TBNAME(ImageList_Add)(__in HIMAGELIST himl, __in HBITMAP hbmImage, __in_opt HBITMAP hbmMask);
int         WINAPI _TBNAME(ImageList_ReplaceIcon)(__in HIMAGELIST himl, __in int i, __in HICON hicon);
COLORREF    WINAPI _TBNAME(ImageList_SetBkColor)(__in HIMAGELIST himl, __in COLORREF clrBk);
COLORREF    WINAPI _TBNAME(ImageList_GetBkColor)(__in HIMAGELIST himl);
BOOL        WINAPI _TBNAME(ImageList_GetImageInfo)(__in HIMAGELIST himl, __in int i, __out IMAGEINFO *pImageInfo);
int         WINAPI _TBNAME(ImageList_AddMasked)(__in HIMAGELIST himl, __in HBITMAP hbmImage, __in COLORREF crMask);
HICON       WINAPI _TBNAME(ImageList_GetIcon)(__in HIMAGELIST himl, __in int i, __in UINT flags);