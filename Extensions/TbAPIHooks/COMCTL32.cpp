#include "stdafx.h"

#include <atlwin.h>

#include "TBWND.h"
#include "TBMessageQueue.h"
#include "TBDC.h"
#include "COMCTL32.h"
#include "TBComboBox.h"
#include "HookedFunction.h"

#ifdef HOOK_IMAGES

HOOK_COMCTL32 (HIMAGELIST, ImageList_Create, (int cx, int cy, UINT flags, int cInitial, int cGrow))
{
	if (UseStandardAPIS())
	{
		return COMCTL32_ORIGINAL(ImageList_Create)(cx, cy, flags, cInitial, cGrow);
	}
	return (new TBImageList(cx, cy, flags, cInitial, cGrow))->GetHandle();
}

HOOK_COMCTL32(BOOL, ImageList_Destroy, (__in_opt HIMAGELIST himl))
{
	TBImageList* pImageList = GetTBImageList(himl);
	if (pImageList)
	{
		delete pImageList;
		return TRUE;
	}
	return COMCTL32_ORIGINAL(ImageList_Destroy)(himl);
}
HOOK_COMCTL32(int, ImageList_GetImageCount, (__in HIMAGELIST himl))
{
	TBImageList* pImageList = GetTBImageList(himl);
	if (pImageList)
	{
		return pImageList->GetCount();
	}
	return COMCTL32_ORIGINAL(ImageList_GetImageCount)(himl);
}
HOOK_COMCTL32(BOOL, ImageList_SetImageCount, (__in HIMAGELIST himl, __in UINT uNewCount))
{
	TBImageList* pImageList = GetTBImageList(himl);
	if (pImageList)
	{
		pImageList->SetCount(uNewCount);
		return TRUE;
	}
	return COMCTL32_ORIGINAL(ImageList_SetImageCount)(himl, uNewCount);
}

HOOK_COMCTL32(BOOL, ImageList_GetImageInfo, (__in HIMAGELIST himl, __in int i, __out IMAGEINFO *pImageInfo))
{
	TBImageList* pImageList = GetTBImageList(himl);
	if (pImageList)
	{
		return pImageList->GetImageInfo(i, pImageInfo);
	}
	return COMCTL32_ORIGINAL(ImageList_GetImageInfo)(himl, i, pImageInfo);
}

HOOK_COMCTL32(int, ImageList_AddMasked,(__in HIMAGELIST himl, __in HBITMAP hbmImage, __in COLORREF crMask))
{
	TBImageList* pImageList = GetTBImageList(himl);
	if (pImageList)
	{
		return pImageList->AddMasked( hbmImage, crMask);
	}
	return COMCTL32_ORIGINAL(ImageList_AddMasked)(himl, hbmImage, crMask);
}
HOOK_COMCTL32(HICON, ImageList_GetIcon, (__in HIMAGELIST himl, __in int i, __in UINT flags))
{
	TBImageList* pImageList = GetTBImageList(himl);
	if (pImageList)
	{
		return pImageList->GetIcon(i, flags);
	}
	return COMCTL32_ORIGINAL(ImageList_GetIcon)(himl, i, flags);
}

#endif //HOOK_IMAGES