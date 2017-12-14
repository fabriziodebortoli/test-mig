#pragma once
#include "tbwnd.h"


class TBStatusBar : public TBWnd
{
	CArray<int> m_arParts;
	CStringArray m_arText;
public:
	TBStatusBar(HWND hwnd, DWORD dwThreadId) : TBWnd(hwnd, dwThreadId)
	{
	}
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};