#pragma once

#include "beginh.dex"

class TB_EXPORT CNotifyIcon
{
	HWND	m_hOwnerWnd;
	UINT	m_nIconId;
public:
	CNotifyIcon();
	~CNotifyIcon(void);

	BOOL Create(HWND hOwnerWnd, const CString& strText, UINT nIconId, HICON hIcon, UINT nCallBackMessage);
	BOOL Dispose();
	BOOL SetText(const CString& strText);
};

#include "endh.dex"