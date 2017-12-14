#include "stdafx.h"
#include <TBApplication\TaskBuilderApp.h>
#include <TBges\extdoc.h>

#include <TbApplication\TbMenuHandlerGDI.h>
#include <TbApplication\TbMenuHandler.h>
#include <TbApplication\TBWebHandler.h>
#include <TbApplication\TBGenericHandler.h>
#include <TbApplication\ThreadMainWindow.h>
#include <TbApplication\TBSocketHandler.h>
#include "TBLoaderRestService.h"

using namespace System;
using namespace Microarea::TaskBuilderNet::Core::SoapCall;
using namespace Microarea::TaskBuilderNet::Core::WebSockets;

public ref class SocketHandler
{
	CTBSocketHandler* m_pHandler;
public:
	SocketHandler(CTBSocketHandler* pHandler)
	{
		m_pHandler = pHandler;
	}
	~SocketHandler () {
		this->!SocketHandler();
		GC::SuppressFinalize(this);
	}
	!SocketHandler () {
		delete m_pHandler;
	}
	void SocketMessageHandler(String^ socketName, String^ message)
	{
		m_pHandler->Execute(CString(socketName), CString(message));
	}
};

class CTbLoaderApp : public CTaskBuilderApp
{
	CStatic* pLabel;

	//--------------------------------------------------------------------------
	BOOL InitInstance()
	{
		if (!__super::InitInstance())
			return FALSE;

		Microarea::TaskBuilderNet::Core::SoapCall::ServiceCache::AddRestService(gcnew CTBLoaderRestService());

		BOOL bHide = GetArgumentValue(_T("HideMainWindow")).CompareNoCase(_T("true")) == 0;
		m_pMainWnd->MoveWindow(10, 10, 310, 210);
		m_pMainWnd->SetWindowText(_T("TbLoader.exe"));
		//Aggiungo alcune informazioni sul processo in esecuzione
		pLabel = new CStatic();
		CString strMode = AfxIsInUnattendedMode() ? _TB("Unattended") : _TB("Attended");
		CString strInfo = cwsprintf(_TB("\r\n TbLoader.exe is listening upon:\r\n  - SoapPort: {0-%d}\r\n  - TCP port: {1-%d} \r\n\r\n TbLoader.exe is in {2-%s} mode\r\n\r\n Process ID: {3-%d}"),
			AfxGetTbLoaderSOAPPort(),
			AfxGetTbLoaderTCPPort(),
			strMode,
			GetCurrentProcessId());

		pLabel->Create(strInfo, WS_VISIBLE | WS_CHILD, CRect(0, 0, 300, 200), m_pMainWnd, IDC_STATIC);
		if (!bHide)
			m_pMainWnd->ShowWindow(SW_SHOW);

		AddRequestHandler(new CTbWebHandler());
		SocketHandler^ h = gcnew SocketHandler(new CTBSocketHandler());
		ServerWebSocketConnector::Start();
		ServerWebSocketConnector::SocketMessage += gcnew SocketMessageHandler(h, &SocketHandler::SocketMessageHandler);
		AfxGetTbCmdManager()->LoadNeededLibraries(CTBNamespace(), NULL, LoadAll);
		return TRUE;
	}

	//--------------------------------------------------------------------------
	BOOL ExitInstance()
	{
		if (pLabel)
		{
			pLabel->DestroyWindow();
			SAFE_DELETE(pLabel);
		}
		if (!AfxIsInUnattendedMode())
			DisposeRequestHandlers();
		return __super::ExitInstance();
	}
};

////////////////////////////////////////////////////////////////////////////
CTbLoaderApp g_theApp;	// The one and only CTaskBuilderApp object
/////////////////////////////////////////////////////////////////////////////
