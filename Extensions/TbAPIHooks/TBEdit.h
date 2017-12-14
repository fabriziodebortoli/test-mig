#pragma once

class TBEdit : public TBWnd
{
	int m_nMaxLen;
	BOOL m_bModified;
	BOOL m_bCanUndo;
	int m_nStartChar;
	int m_nEndChar;
	RECT m_Rect;
public:
	TBEdit(HWND hwnd, DWORD dwThreadId) : TBWnd(hwnd, dwThreadId), m_nMaxLen(UINT_MAX), m_bModified(FALSE), m_nStartChar(-1), m_nEndChar(-1), m_bCanUndo(TRUE)
	{
		ZeroMemory(&m_Rect, sizeof(RECT));
	}
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};
