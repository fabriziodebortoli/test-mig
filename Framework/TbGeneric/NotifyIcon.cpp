#include "StdAfx.h"
#include "globals.h"

#include ".\notifyicon.h"

//-----------------------------------------------------------------------------
CNotifyIcon::CNotifyIcon()
:
m_hOwnerWnd(NULL),
m_nIconId(0)
{
}

//-----------------------------------------------------------------------------
CNotifyIcon::~CNotifyIcon(void)
{
	Dispose();
}

//-----------------------------------------------------------------------------
BOOL CNotifyIcon::Create(HWND hOwnerWnd, const CString& strText, UINT nIconId, HICON hIcon, UINT nCallBackMessage)
{
	ASSERT_TRACE(hOwnerWnd,"Parameter hOwnerWnd cannot be null");
		
	NOTIFYICONDATA	nid;
	memset(&nid, 0, sizeof(NOTIFYICONDATA));
	nid.cbSize = sizeof(NOTIFYICONDATA);
	nid.hWnd = hOwnerWnd;
	nid.uID = nIconId;
	nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
	nid.uCallbackMessage = nCallBackMessage;
	nid.hIcon = hIcon;
	TB_TCSCPY(nid.szTip, strText);
	if (::Shell_NotifyIcon(NIM_ADD, &nid))
	{
		m_hOwnerWnd = hOwnerWnd;
		m_nIconId = nIconId;
		return TRUE;	
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CNotifyIcon::Dispose()
{
	if (!m_nIconId)
		return FALSE;
	
	NOTIFYICONDATA	nid;
	memset(&nid, 0, sizeof(NOTIFYICONDATA));
	nid.cbSize = sizeof(NOTIFYICONDATA);
	nid.hWnd = m_hOwnerWnd;
	nid.uID = m_nIconId;
	
	m_nIconId = 0;
	m_hOwnerWnd = NULL;

	return ::Shell_NotifyIcon(NIM_DELETE, &nid);
}

//-----------------------------------------------------------------------------
BOOL CNotifyIcon::SetText(const CString& strText)
{
	if (!m_nIconId)
		return FALSE;
	
	NOTIFYICONDATA	nid;
	memset(&nid, 0, sizeof(NOTIFYICONDATA));
	nid.cbSize = sizeof(NOTIFYICONDATA);
	nid.hWnd = m_hOwnerWnd;
	nid.uID = m_nIconId;
	TB_TCSCPY(nid.szTip, strText);
	nid.uFlags = NIF_TIP;
	
	return ::Shell_NotifyIcon(NIM_MODIFY, &nid);
}

