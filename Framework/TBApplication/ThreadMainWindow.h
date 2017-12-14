#pragma once

#include "beginh.dex"

//=============================================================================================
class TB_EXPORT CThreadMainWindow : public CTBLockedFrame
{
	BOOL m_bRegisterInMap;
	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(CThreadMainWindow)
public:
	CThreadMainWindow(BOOL bRegisterInMap)	;
	~CThreadMainWindow();
	afx_msg LRESULT OnGetSoapPort		(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnExecuteFunction	(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnIsUnattendedWindow(WPARAM wParam, LPARAM lParam);
	virtual BOOL DestroyWindow();
};

LONG WINAPI ExpFilter(EXCEPTION_POINTERS* pExp, DWORD dwExpCode);
void DumpAssertions();

#include "endh.dex"