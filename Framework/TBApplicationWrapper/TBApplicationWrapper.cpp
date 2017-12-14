// TBApplicationWrapper.cpp : Defines the initialization routines for the DLL.
//

#include "stdafx.h"
#include <TbGenlib\CEFClasses.h>
#include <TbWoormEngine\EDTCMM.H>
#include <TbWoormViewer\WOORMDOC.H>

#include "MExpression.h"
#include "MWoormInfo.h"
#include "TBApplicationWrapper.h"
#include "TBProxy.h"
#include "GlobalContext.h"
#include <TbApplication\TbMenuHandlerGDI.h>
#include <TbApplication\TbMenuHandler.h>
#include <TbApplication\TBWebHandler.h>
#include <TbApplication\TBGenericHandler.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif
using namespace  System;
using namespace  System::IO;
using namespace  System::Reflection;
using namespace  Microarea::Framework::TBApplicationWrapper;
using namespace  Microarea::TaskBuilderNet::Woorm::WoormController;
using namespace  Microarea::TaskBuilderNet::Core::WebSockets;

CTBApplicationWrapperApp *m_gTBApplicationWrapperApp = NULL;

//	THIS LIBRARY IS A BRIDGE BETWEEN MANAGED AND UNMANAGED CODE.
//	IT ENCAPSULATES ALL THE NECESSARY MECHANISM TO CREATE A TaskBuilderApp
//	INSTANCE LIVING IN ITS OWN THREAD, DIFFERENT FROM THE ONE OF THE 
//	HOSTING MANAGED APPLICATION. TO PERMIT THIS, SOME ADJUSTMENTS HAVE BEEN PERFORMED
//	IN THE MFC LOADING MECHANISM:
//	1) IN THE CONSTRUCTOR, VARIABLES m_hThread  AND m_nThreadID HAVE BEEN RESTORED TO 0
//		BECAUSE THE OBJECT IS CREATED FIRST AND THE THREAD LATER (THIS IS AN IMPORTANT DIFFERENCE
//		FROM THE STANDARD MFC PROCEDURE)
//	2)	AfxWinInit IS EXPLICITLY CALLED IN THE InitInstance METHOD, BECAUSE THE APPLICATION OBJECT IS
//		INITIALIZED BY US, NOT BY THE MFC FRAMEWORK
//	3)	ALL CLOSING OPERATIONS ARE TRIGGERED BY A UM_EXIT_THREAD CUSTOM MESSAGE; THE CLOSING OPERATIONS
//		CONSIST OF THE CLOSURE OF THE MAIN WINDOW AND THE SENDING OF THE WM_QUIT MESSAGE TO THE THREAD
//	4)	THE APPLICATION OBJECT OVERRIDES THE RUN METHOD, SO IT CAN EXECUTE CODE WHEN THE PARENT'S MESSAGE LOOP
//		ENDS (OBJECT DELETION IS AUTO MANAGED TO AVOID MFC ASSERTS DUE TO THE COINCIDENCE OF CURRENT THREAD 
//		AND CURRENT APPLICATION OBJECT (WHICH MFC DOES NOT EXPECT); THE ROW:
//			pState->m_pCurrentWinThread = NULL;
//		AVOIDS THREAD DELETION BY MFC, AND THE SUBSEQUENT ROW:
//			delete this;
//		PERFORMS DELETION



//
//TODO: If this DLL is dynamically linked against the MFC DLLs,
//		any functions exported from this DLL which call into
//		MFC must have the AFX_MANAGE_STATE macro added at the
//		very beginning of the function.
//
//		For example:
//
//		extern "C" BOOL PASCAL EXPORT ExportedFunction()
//		{
//			AFX_MANAGE_STATE(AfxGetStaticModuleState());
//			// normal function body here
//		}
//
//		It is very important that this macro appear in each
//		function, prior to any calls into MFC.  This means that
//		it must appear as the first statement within the 
//		function, even before any object variable declarations
//		as their constructors may generate calls into the MFC
//		DLL.
//
//		Please see MFC Technical Notes 33 and 58 for additional
//		details.
//



// CTBApplicationWrapperApp

BEGIN_MESSAGE_MAP(CTBApplicationWrapperApp, CTaskBuilderApp)
END_MESSAGE_MAP()
	
//HANDLE CTBApplicationWrapperApp::s_hMainThread = NULL;


// CTBApplicationWrapperApp construction
//-----------------------------------------------------------------------------
CTBApplicationWrapperApp::CTBApplicationWrapperApp(CString strArgs)
{
	m_pStartCompleteEvent = new ::CEvent(FALSE, TRUE);
	m_hThread = 0;
	m_nThreadID = 0;
	m_strArguments = strArgs;
	m_bValid = FALSE;
}

//-----------------------------------------------------------------------------
CTBApplicationWrapperApp::~CTBApplicationWrapperApp(void)
{
	delete m_pStartCompleteEvent;			 
}

//-----------------------------------------------------------------------------
BOOL CTBApplicationWrapperApp::InitInstance()
{
	AFX_MODULE_STATE *pState = AfxGetStaticModuleState();
	AFX_MANAGE_STATE(pState);
	// AFX internal initialization
	if (!AfxWinInit(pState->m_hCurrentInstanceHandle, NULL, (LPTSTR)(LPCTSTR)m_strArguments, SW_SHOW))
	{
		m_bValid = FALSE;
		goto end;
	}

	if (!InitApplication())
	{
		m_bValid = FALSE;
		goto end;
	}

	m_bValid = __super::InitInstance();

	ServerWebSocketConnector::Start();

	CTBRequestHandlerObj* handler = AfxGetApplicationContext()->IsIISModule() ? new CTbMenuHandler() : new CTbMenuHandlerGDI();
	AddRequestHandler(handler);
	AddRequestHandler(new CTbWebHandler());

	SetExpressionEditorFunction(&DoExpressionEditor);
end:
	SetStartCompleteEvent();

	return TRUE;
}

//-----------------------------------------------------------------------------
int CTBApplicationWrapperApp::Run()
{
	int nRes = __super::Run();

	AFX_MODULE_THREAD_STATE* pState = AfxGetModuleThreadState();
	pState->m_pCurrentWinThread = NULL;

	delete this;

	return nRes; 
}

//-----------------------------------------------------------------------------
int CTBApplicationWrapperApp::ExitInstance()
{
	ServerWebSocketConnector::Stop();
	DisposeRequestHandlers();
	return __super::ExitInstance();
}
//-----------------------------------------------------------------------------
void CTBApplicationWrapperApp::WaitForExitEvent()	
{
	if (!this)
		return;

	HANDLE hThread = OpenThread(SYNCHRONIZE, FALSE, m_nThreadID);
	WaitForSingleObject(hThread, INFINITE); 
	CloseHandle(hThread);
}

//-----------------------------------------------------------------------------
CTBApplicationWrapperApp* CTBApplicationWrapperApp::CreateTaskBuilderApp(CString strArgs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	ASSERT(m_gTBApplicationWrapperApp == NULL);//singleton
	
	m_gTBApplicationWrapperApp = new CTBApplicationWrapperApp(strArgs);
	m_gTBApplicationWrapperApp->ResetStartCompleteEvent();
	m_gTBApplicationWrapperApp->CreateThread();
	m_gTBApplicationWrapperApp->WaitStartCompleteEvent();
	return m_gTBApplicationWrapperApp;
}

//-----------------------------------------------------------------------------
void CTBApplicationWrapperApp::CloseTaskBuilderApp()
{
	if (!m_gTBApplicationWrapperApp) return;
	
	m_gTBApplicationWrapperApp->WaitStartCompleteEvent();

	m_gTBApplicationWrapperApp->PostThreadMessage(WM_QUIT, 0, NULL);
	m_gTBApplicationWrapperApp->WaitForExitEvent();
	m_gTBApplicationWrapperApp = NULL;
}

//-----------------------------------------------------------------------------
CTBApplicationWrapperApp* CTBApplicationWrapperApp::GetTaskBuilderApp()
{
	return m_gTBApplicationWrapperApp;
}

//-----------------------------------------------------------------------------
typedef void *InitDynLinkLibrary(void);
void CTBApplicationWrapperApp::InitLibrary(CString strLibName)
{
	HMODULE hMod = LoadLibrary(strLibName);
	if (!hMod)
	{
		ASSERT(FALSE);
		return;
	}
	InitDynLinkLibrary* pFunc = (InitDynLinkLibrary*) GetProcAddress(hMod, "InitDynLinkLibrary");
	if (!pFunc)
	{
		ASSERT(FALSE);
		return;
	}
	pFunc();
}


