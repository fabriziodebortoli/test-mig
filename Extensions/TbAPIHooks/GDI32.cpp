#include "stdafx.h"

#include <atlwin.h>

#include "TBWND.h"
#include "TBMessageQueue.h"
#include "TBDC.h"
#include "GDI32.h"
#include "TBComboBox.h"
#include "HookedFunction.h"

//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HBRUSH, CreateBrushIndirect, ( __in CONST LOGBRUSH *plbrush))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreateBrushIndirect)(plbrush);
	return  (HBRUSH)GetStockObject(BLACK_BRUSH);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HBRUSH, CreateHatchBrush, ( __in int iHatch, __in COLORREF color))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreateHatchBrush)(iHatch, color);
	return  (HBRUSH)GetStockObject(BLACK_BRUSH);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HBRUSH, CreatePatternBrush, ( __in HBITMAP hbm))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreatePatternBrush)(hbm);
	return  (HBRUSH)GetStockObject(BLACK_BRUSH);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HBRUSH, CreateSolidBrush, ( __in COLORREF color))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreateSolidBrush)(color);
	return  (HBRUSH)GetStockObject(BLACK_BRUSH);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HPEN, CreatePen,( __in int iStyle, __in int cWidth, __in COLORREF color))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreatePen)(iStyle, cWidth, color);
	return (HPEN)GetStockObject(BLACK_PEN);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HPEN, CreatePenIndirect,( __in CONST LOGPEN *plpen))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreatePenIndirect)(plpen);
	return  (HPEN)GetStockObject(BLACK_PEN);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HFONT, CreateFontW,( __in int cHeight, __in int cWidth, __in int cEscapement, __in int cOrientation, __in int cWeight, __in DWORD bItalic,__in DWORD bUnderline, __in DWORD bStrikeOut, __in DWORD iCharSet, __in DWORD iOutPrecision, __in DWORD iClipPrecision,__in DWORD iQuality, __in DWORD iPitchAndFamily, __in_opt LPCWSTR pszFaceName))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreateFontW)(cHeight,cWidth,cEscapement,cOrientation,cWeight, bItalic,bUnderline, bStrikeOut, iCharSet, iOutPrecision, iClipPrecision,iQuality, iPitchAndFamily, pszFaceName);
	return (HFONT)GetStockObject(SYSTEM_FONT);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HFONT, CreateFontIndirectW, ( __in CONST LOGFONTW *lplf))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreateFontIndirectW)(lplf);
	return (HFONT)GetStockObject(SYSTEM_FONT);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HFONT, CreateFontIndirectExW, ( __in CONST ENUMLOGFONTEXDVW * lp))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreateFontIndirectExW)(lp);
	return (HFONT)GetStockObject(SYSTEM_FONT);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(BOOL, GetTextExtentPoint32W,(__in HDC hdc, __in_ecount(c) LPCWSTR lpString,__in int c, __out LPSIZE psizl))
	if (hdc)
	{
		TBDC* pDC = GetTBDC(hdc);
		if (pDC)
			return pDC->GetTextExtentPoint32(lpString, c, psizl);
	}

	return GDI32_ORIGINAL(GetTextExtentPoint32W)(hdc,  lpString, c, psizl);

}

//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(BOOL, GetTextMetricsW, ( __in HDC hdc, __out LPTEXTMETRIC lptm))
	if (hdc)
	{
		TBDC* pDC = GetTBDC(hdc);
		if (pDC)
			return pDC->GetTextMetrics(lptm);
	}

	return GDI32_ORIGINAL(GetTextMetricsW)(hdc, lptm);

}

//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(HDC, CreateCompatibleDC,(__in HDC hDC))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(CreateCompatibleDC)(hDC);
	return CreateTBDC(hDC);
}



//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(BOOL, DeleteDC,(__in HDC hDC))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(DeleteDC)(hDC);
	if (DestroyTBDC(hDC))
		return TRUE;

	return GDI32_ORIGINAL(DeleteDC)(hDC);
}
#ifdef HOOK_IMAGES
//---------------------------------------------------------------------------------------------------------------
HOOK_GDI32(BOOL, DeleteObject,( __in HGDIOBJ ho))
	if (UseStandardAPIS())
		return GDI32_ORIGINAL(DeleteObject)(ho);
	CTBImage* pImage = NULL;
	CSingleLock l(Get_CImageMapSection(), TRUE);
	if (GetGDIMap().Lookup(ho, pImage))
	{
		if (pImage->Release() == 0)
		{
			GetGDIMap().RemoveKey(ho);
			Get_CImageMap().RemoveKey(pImage->GetKey());
			GDI32_ORIGINAL(DeleteObject)(ho);
			delete pImage;
		}
		return TRUE;
	}
	return GDI32_ORIGINAL(DeleteObject)(ho);
}
#endif //HOOK_IMAGES