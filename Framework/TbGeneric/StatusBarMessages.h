
#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

/////////////////////////////////////////////////////////////////////////////
//          	Class CStatusBarMsg definition
/////////////////////////////////////////////////////////////////////////////
//
//---------------------------------------------------------------------------
class TB_EXPORT CStatusBarMsg : public CObject
{
private:	
	bool m_bWait;
	bool m_bDisable;
	bool m_bEnabled;
    bool m_bIdleMessage;
	bool m_bUnattended;
	bool m_bDisposed;
    HWND m_hwndMainWnd;
public:
	CStatusBarMsg (bool bDisable = false, bool bWait = false, bool bIdleMessage = true);
	void Dispose();
	virtual ~CStatusBarMsg ();

public:
	void Show 		(UINT nIDS, double nPerc = -1.0);
	void Show 		(const CString& strText, double nPerc = -1.0);
	void ShowIdle	();
};

#include "endh.dex"
