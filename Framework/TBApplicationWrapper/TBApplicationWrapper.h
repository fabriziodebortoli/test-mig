// TBApplicationWrapper.h : main header file for the TBApplicationWrapper DLL
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include <afxmt.h>

#include <TBApplication\TaskBuilderApp.h>
#include "beginh.dex"
// CTBApplicationWrapperApp
// See TBApplicationWrapper.cpp for the implementation of this class
//

class TB_EXPORT CTBApplicationWrapperApp : public CTaskBuilderApp
{
public:
	::CEvent*	m_pStartCompleteEvent;
	BOOL		m_bValid;

	CString m_strArguments;

protected:
	CTBApplicationWrapperApp(CString strArgs);
public:
	~CTBApplicationWrapperApp(void);

// Overrides
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
	void InitLibrary(CString strLibName);
	virtual int ExitInstance();

	__event void OnExit();

public:
	static CTBApplicationWrapperApp* CreateTaskBuilderApp(CString strArgs);
	static void CloseTaskBuilderApp();
	static CTBApplicationWrapperApp* GetTaskBuilderApp();
	virtual int Run();

	void WaitForExitEvent();

	BOOL ResetStartCompleteEvent()	{ return m_pStartCompleteEvent->ResetEvent(); }
	BOOL SetStartCompleteEvent()	{ return m_pStartCompleteEvent->SetEvent(); }
	void WaitStartCompleteEvent()	{ WaitForSingleObject(*m_pStartCompleteEvent, INFINITE); }

protected:
	virtual BOOL UnattendedStart()	{ return TRUE; }


};


#include "endh.dex"

