#pragma once

class TBToolTip : public TBWnd
{
public:
	TBToolTip(HWND hwnd, DWORD dwThreadId) : TBWnd(hwnd, dwThreadId)
	{
	}
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};
