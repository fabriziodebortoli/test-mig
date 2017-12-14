#include "stdafx.h"
#include "TBDC.h"
#include "HookedFunction.h"

EXTERN_HOOK_LIB(HDC, CreateCompatibleDC, (__in HDC hDC));
EXTERN_HOOK_LIB(BOOL, DeleteDC, (__in HDC hDC));
EXTERN_HOOK_LIB(BOOL, GetTextExtentPoint32W,(__in HDC hdc, __in_ecount(c) LPCWSTR lpString,__in int c, __out LPSIZE psizl))
EXTERN_HOOK_LIB(BOOL, GetTextMetricsW, ( __in HDC hdc, __out LPTEXTMETRIC lptm))
TBDC::TBDC(HDC hdc) 
{
	m_hdc = GDI32_ORIGINAL(CreateCompatibleDC)(hdc);
	CSingleLock l(Get_CHDCToTBDCMapSection(), TRUE); 
	Get_CHDCToTBDCMap()[m_hdc] = this;
	m_hOld = NULL;
	//TRACE1("%d\n", Get_CHDCToTBDCMap().GetCount());
}

TBDC::TBDC(HWND hwnd)
{
	m_hdc = GDI32_ORIGINAL(CreateCompatibleDC)(NULL);
	
	TBWnd* pWnd = GetTBWnd(hwnd);
	m_hOld = pWnd ? SelectObject(m_hdc, pWnd->GetDCBitmap()) : NULL;
	CSingleLock l(Get_CHDCToTBDCMapSection(), TRUE);
	Get_CHDCToTBDCMap()[m_hdc] = this;
	//TRACE1("%d\n", Get_CHDCToTBDCMap().GetCount());
}
TBDC::~TBDC()
{
	GDI32_ORIGINAL(DeleteDC)(m_hdc);
	CSingleLock l(Get_CHDCToTBDCMapSection(), TRUE);
	Get_CHDCToTBDCMap().RemoveKey(m_hdc);
	if (m_hOld)
		SelectObject(m_hdc, m_hOld);
	//TRACE1("%d\n", Get_CHDCToTBDCMap().GetCount());
}




BOOL TBDC::GetTextExtentPoint32(LPCWSTR lpString, int c, LPSIZE psizl)
{
	return GDI32_ORIGINAL(GetTextExtentPoint32)(m_hdc, lpString, c, psizl);
}


BOOL TBDC::GetTextMetrics(LPTEXTMETRIC lptm)
{
	return GDI32_ORIGINAL(GetTextMetrics)(m_hdc, lptm);
}


TBDC* GetTBDC(HDC hdc)
{
	TBDC* pValue = NULL;
	CSingleLock l(Get_CHDCToTBDCMapSection(), TRUE);
	Get_CHDCToTBDCMap().Lookup(hdc, pValue);
	return pValue;
}

HDC CreateTBDC(HDC hdc)
{
	 return (new TBDC(hdc))->GetHDC();
}

HDC CreateTBDC(HWND hwnd)
{
	return (new TBDC(hwnd))->GetHDC();
}

BOOL DestroyTBDC(HDC dc)
{
	TBDC* pDC = GetTBDC(dc);
	if (!pDC)
		return FALSE;
	delete pDC;
	return TRUE;

}