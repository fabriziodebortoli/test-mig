#include "StdAfx.h"
#include "TBDialog.h"


//----------------------------------------------------------------------------
TBDialog::TBDialog(HWND hwnd, DWORD dwThreadId) : TBWnd(hwnd, dwThreadId), m_baseUnitX(BASE_UNIT_X), m_baseUnitY(BASE_UNIT_Y)
{
}


//----------------------------------------------------------------------------
TBDialog::~TBDialog(void)
{
}


//----------------------------------------------------------------------------
LRESULT TBDialog::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case WM_CLOSE: 
	{
		//dialog proc transforms WM_CLOSE in cancel command (which exits modal loop, if any)
		return SendMessage(WM_COMMAND, IDCANCEL, 0);
	}
	default:
	{
		return __super::DefWindowProc(message, wParam, lParam);
	}
	}
}

//----------------------------------------------------------------------------
BOOL TBDialog::MapDialogRect(LPRECT lpRect)
{

	lpRect->left   = MulDiv(lpRect->left,   m_baseUnitX, 4);
	lpRect->right  = MulDiv(lpRect->right,  m_baseUnitX, 4);
	lpRect->top    = MulDiv(lpRect->top,    m_baseUnitY, 8);
	lpRect->bottom = MulDiv(lpRect->bottom, m_baseUnitY, 8);

	return TRUE;
}

//----------------------------------------------------------------------------
void TBDialog::CalculateBaseUnits(LPCTSTR szFontFace, int wPoint)
{
	GetBaseUnits(m_baseUnitX, m_baseUnitY, szFontFace, wPoint);
}

//----------------------------------------------------------------------------
BOOL TBDialog::IsDialogMessage(LPMSG lpMsg)
{
	//DefWindowProc(WM_GETDLGCODE, ?virtual code, (LPARAM)lpMsg); 
	return FALSE;
}