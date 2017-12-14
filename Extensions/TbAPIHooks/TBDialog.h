#pragma once
#include "TBWnd.h"
class TBDialog: public TBWnd
{
	
	int				m_baseUnitX;
	int				m_baseUnitY;
public:
	TBDialog(HWND hwnd, DWORD dwThreadId);
	~TBDialog(void);
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	virtual BOOL MapDialogRect(LPRECT lpRect);
	virtual BOOL IsDialogMessage(LPMSG lpMsg);

	void CalculateBaseUnits(LPCTSTR szFontFace, int wPoint);
};

