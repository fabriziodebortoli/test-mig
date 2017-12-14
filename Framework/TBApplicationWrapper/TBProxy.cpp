#include "StdAfx.h"

#include <TBNameSolver\Diagnostic.h>
#include <TBNameSolver\LoginContext.h>
#include <TBNameSolver\Templates.h>
#include <TBGeneric\GeneralObjects.h>
#include <TBGeneric\Critical.h>
#include <TBGes\SoapFunctions.h>
#include <TBGenlib\BaseApp.h>
#include <TBGenlib\TbCommandInterface.h>
#include <TBGenlibUI\SoapFunctions.h>
#include <TbWebServicesWrappers\LockManagerInterface.h>
#include <TBApplication\ThreadMainWindow.h>
#include <TbGes\ExtDocAbstract.h>

#include "TBProxy.h"

#include "TBApplicationWrapper.h"
#include "TBToolStripMenuItem.h"
#include "GlobalContext.h"

#include "Utility.h"
#include "CLockStructureViewer.h"

using namespace System::Threading;
using namespace System;
using namespace System::Windows::Forms;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace Microarea::Framework::TBApplicationWrapper;



class CTBApplicationProxy
{
public:
	static void SetUnattendedMode(bool set)
	{
		AfxGetApplicationContext()->SetInUnattendedMode(set);
	}
};

//-----------------------------------------------------------------------------
TBApplicationProxy::TBApplicationProxy(System::String^ tbApplicationPath, System::String^ arguments)
{
	Monitor::Enter(TBApplicationProxy::typeid);
	try
	{
		if (!CTBApplicationWrapperApp::GetTaskBuilderApp())
			Create(tbApplicationPath, arguments);
		authenticationToken = gcnew String(AfxGetAuthenticationToken());
	}
	finally
	{
		Monitor::Exit(TBApplicationProxy::typeid);
	}
}

//-----------------------------------------------------------------------------
TBApplicationProxy::~TBApplicationProxy(void)
{
	/*if (!httpDocumentHandler) 
	{
		delete httpDocumentHandler;
		httpDocumentHandler = nullptr;
	}*/
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::SetUnattendedMode(bool set)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CTBApplicationProxy::SetUnattendedMode(set);
}
//-----------------------------------------------------------------------------
ITBApplicationProxy^ TBApplicationProxy::AttachProxyToTbApplication()
{
	Monitor::Enter(TBApplicationProxy::typeid);
	try
	{
		if (!CTBApplicationWrapperApp::GetTaskBuilderApp()) return nullptr;
		return gcnew TBApplicationProxy(nullptr, nullptr);
	}
	finally
	{
		Monitor::Exit(TBApplicationProxy::typeid);
	}
}
/*
//-----------------------------------------------------------------------------
IHttpDocumentHandler^ TBApplicationProxy::HttpDocumentHandler::get()
{
	if (!httpDocumentHandler)
		httpDocumentHandler = gcnew Microarea::Framework::TBApplicationWrapper::HttpDocumentHandler();
	return httpDocumentHandler;
}*/

//-----------------------------------------------------------------------------
void TBApplicationProxy::CloseTbApplication()
{
	Monitor::Enter(TBApplicationProxy::typeid);
	try
	{
		CloseEnumsViewer();
		CTBApplicationWrapperApp::CloseTaskBuilderApp();
	}
	finally
	{
		Monitor::Exit(TBApplicationProxy::typeid);
	}
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::ShowLockStructure()
{
	CLockStructureViewer^ viewer = gcnew CLockStructureViewer();
	viewer->ShowDialog();			
}

//-----------------------------------------------------------------------------
int TBApplicationProxy::SoapPort::get() 
{
	return AfxGetTbLoaderSOAPPort();
}
//-----------------------------------------------------------------------------
bool TBApplicationProxy::IsEasyBuilderDeveloper::get() 
{
	CLoginContext *pContext = GetLoginContext();
	return pContext && pContext->GetLoginInfos()->m_bEasyBuilderDeveloper == TRUE;
}

//-----------------------------------------------------------------------------
int TBApplicationProxy::TcpPort::get() 
{
	return AfxGetTbLoaderSOAPPort();
}
//-----------------------------------------------------------------------------
bool TBApplicationProxy::Valid::get() 
{
	Monitor::Enter(TBApplicationProxy::typeid);
	try
	{
		return (CTBApplicationWrapperApp::GetTaskBuilderApp() != NULL) && CTBApplicationWrapperApp::GetTaskBuilderApp()->m_bValid;
	}
	finally
	{
		Monitor::Exit(TBApplicationProxy::typeid);
	}
}
//-----------------------------------------------------------------------------
bool TBApplicationProxy::CanShowLockStructure::get() 
{
	return CLockStructureViewer::CanShowLockStructure::get();
}
//-----------------------------------------------------------------------------
cli::array<System::String^>^ TBApplicationProxy::DocumentHistory::get() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	const CStringArray& ar = GetLoginContext()->GetDocumentLinks();
	cli::array<System::String^>^ list = gcnew cli::array<System::String^>(ar.GetCount());
	for (int i = 0; i < ar.GetCount(); i++)
		list[i] = gcnew System::String(ar.GetAt(i));
	return list;
}

//-----------------------------------------------------------------------------
bool TBApplicationProxy::Logged::get() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return Valid && GetLoginContext() && GetLoginContext()->IsValid();
}
//--------------------------------------------------------------------------------------------------------
void TBApplicationProxy::OnApplicationExit(Object^ sender, EventArgs^ e)
{
	TBApplicationProxy::CloseTbApplication();
}


//-----------------------------------------------------------------------------
void TBApplicationProxy::Create(System::String^ tbApplicationPath, System::String^ arguments)
{
	if (!GlobalContext::Valid)
		return;

	Monitor::Enter(TBApplicationProxy::typeid);
	try
	{
		CUtility::AddEnvironmentVariable("PATH", tbApplicationPath);
		
		CTBApplicationWrapperApp::CreateTaskBuilderApp(arguments);
		
	}
	catch (Exception^ ex) 
	{
		GlobalContext::Valid = false;
		GlobalContext::GlobalDiagnostic->Set(DiagnosticType::Error, "Error creating Task Builder application, please verify all components are installed");
		GlobalContext::GlobalDiagnostic->Set(DiagnosticType::Error, ex->Message);
	}
	finally
	{
		Monitor::Exit(TBApplicationProxy::typeid);
	}
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::Destroy(void)
{
	CloseTbApplication();	
}


//-----------------------------------------------------------------------------
bool TBApplicationProxy::HasExited(void)
{
	Monitor::Enter(TBApplicationProxy::typeid);
	try
	{
		return CTBApplicationWrapperApp::GetTaskBuilderApp() == NULL;
	}
	finally
	{
		Monitor::Exit(TBApplicationProxy::typeid);
	}
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::WaitForExit(void)
{
	CTBApplicationWrapperApp::GetTaskBuilderApp()->WaitForExitEvent();
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::SetMenuWindowHandle(IntPtr hMenuWindow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CLoginContext* pContext = GetLoginContext();
	if (pContext)
	{
		//warning! use this dispatcher function instead of calling the method
		//directly so as to put che call into the right thread execution context
		AfxInvokeThreadProcedure<CLoginContext, HWND>
			(
			pContext->m_nThreadID, 
			pContext, 
			&CLoginContext::SetMenuWindowHandle, 
			(HWND)hMenuWindow.ToInt32()
			);
	}
	else
	{
		//se non e' mai stato impostato, lo imposto ora
		if (!AfxGetApplicationContext()->GetMenuWindowHandle())
			AfxGetApplicationContext()->SetMenuWindowHandle((HWND)hMenuWindow.ToInt32());
	}
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::SetDocked (Boolean isDocked)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CLoginContext* pContext = GetLoginContext();
	if (!pContext) return;
	//warning! use this dispatcher function instead of calling the method
	//directly so as to put che call into the right thread execution context
	AfxInvokeThreadProcedure<CLoginContext, BOOL>
		(
		pContext->m_nThreadID, 
		pContext, 
		&CLoginContext::SetDocked, 
		isDocked
		);
}

//-----------------------------------------------------------------------------
bool TBApplicationProxy::Login (System::String ^autheticationToken)
{
	this->authenticationToken = autheticationToken;
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	return AfxInvokeThreadFunction<BOOL, CTbCommandInterface, const CString&>(AfxGetApp()->m_nThreadID, AfxGetTbCmdManager(), &CTbCommandInterface::Login, CString(authenticationToken)) != 0;

}
//-----------------------------------------------------------------------------
void TBApplicationProxy::CloseLogin ()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext* pContext = GetLoginContext();
	if (pContext)
		AfxInvokeThreadProcedure<CLoginContext>(pContext->m_nThreadID, pContext, &CLoginContext::Close);
}	

//-----------------------------------------------------------------------------
bool TBApplicationProxy::CloseAllDocuments ()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext *pLogin =  GetLoginContext();
	if (!pLogin)
		return false;
	return AfxInvokeThreadFunction<BOOL, CLoginContext>(pLogin->m_nThreadID, pLogin, &CLoginContext::CloseAllDocuments) == TRUE;
}

//-----------------------------------------------------------------------------
bool TBApplicationProxy::SilentCloseLoginDocuments()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext *pLogin =  GetLoginContext();
	if (!pLogin)
		return false;
	return AfxInvokeThreadFunction<BOOL, CLoginContext>(pLogin->m_nThreadID, pLogin, &CLoginContext::SilentCloseLoginDocuments) == TRUE;
}

void TBApplicationProxy::FireAction(int documentHandle, System::String^ action)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CObject* pObj = (CObject*)documentHandle;
	if (!pObj || ! pObj->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		return;
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*) pObj;
	AfxInvokeThreadFunction<int, CAbstractFormDoc, const CString&>
		(
		pDoc->GetFrameHandle(), 
		pDoc,
		&CAbstractFormDoc::FireAction, 
		CString(action)
		);
}

//-----------------------------------------------------------------------------
int TBApplicationProxy::RunDocument(System::String^ command, System::String^ arguments)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);

	return AfxInvokeThreadGlobalFunction<DataLng, DataStr, DataStr>
		(pContext->m_nThreadID, 
		&::RunDocument, 
		CString(command),
		CString(arguments));
}
//-----------------------------------------------------------------------------
int TBApplicationProxy::RunReport(System::String^ command, System::String^ arguments)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);

	return AfxInvokeThreadGlobalFunction<DataLng, DataStr, DataStr>
		(pContext->m_nThreadID, 
		&::RunReport, 
		CString(command),
		CString(arguments));

}
//-----------------------------------------------------------------------------
bool TBApplicationProxy::RunFunction(System::String^ command, System::String^ arguments)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);

	return (bool)AfxInvokeThreadGlobalFunction<DataBool, DataStr, DataStr>
		(pContext->m_nThreadID, 
		&::RunFunction, 
		CString(command),
		CString(arguments)) != 0;

}

//-----------------------------------------------------------------------------
void TBApplicationProxy::RunFunctionInNewThread(System::String^ command, System::String^ arguments)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);

	AfxInvokeThreadGlobalProcedure<DataStr, DataStr>
		(pContext->m_nThreadID, 
		&::RunFunctionInNewThread, 
		CString(command),
		CString(arguments));

}
//-----------------------------------------------------------------------------
bool TBApplicationProxy::RunTextEditor(System::String^ command)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);

	return (bool)AfxInvokeThreadGlobalFunction<DataBool, DataStr>
		(pContext->m_nThreadID, 
		&::RunEditor, 
		CString(command)) != 0;

}
//-----------------------------------------------------------------------------
IDiagnostic^ TBApplicationProxy::GetGlobalDiagnostic(bool clear)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CDiagnostic* pDiagnostic = AfxInvokeThreadGlobalFunction<CDiagnostic*, BOOL>(AfxGetApp()->m_nThreadID, &CloneDiagnostic, clear);

	Diagnostic ^diagnostic = GetDiagnostic(pDiagnostic, nullptr);
	
	CLoginContext* pContext = GetLoginContext();
	if (pContext)
	{
		pDiagnostic = AfxInvokeThreadGlobalFunction<CDiagnostic*, BOOL>(pContext->m_nThreadID, &CloneDiagnostic, clear);
		diagnostic = GetDiagnostic(pDiagnostic, diagnostic);
	}

	return diagnostic;
}

//-----------------------------------------------------------------------------
IDiagnostic^ TBApplicationProxy::GetApplicationContextDiagnostic(bool clear)
{
	if (!CTBApplicationWrapperApp::GetTaskBuilderApp())
		return GlobalContext::GlobalDiagnostic;
	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CDiagnostic* pDiagnostic = AfxInvokeThreadGlobalFunction<CDiagnostic*, BOOL>(AfxGetApp()->m_nThreadID, &CloneDiagnostic, clear);

	return GetDiagnostic(pDiagnostic, nullptr);
}

//-----------------------------------------------------------------------------
IDiagnostic^ TBApplicationProxy::GetLoginContextDiagnostic(bool clear)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CLoginContext* pContext = GetLoginContext();
	if (!pContext)
		return gcnew Diagnostic("");

	CDiagnostic* pDiagnostic = AfxInvokeThreadGlobalFunction<CDiagnostic*, BOOL>(pContext->m_nThreadID, &CloneDiagnostic, clear);

	return GetDiagnostic(pDiagnostic, nullptr);
}

//-----------------------------------------------------------------------------
Diagnostic^ TBApplicationProxy::GetDiagnostic(CDiagnostic* pDiagnostic, Diagnostic^ diagnostic)
{
	if (diagnostic == nullptr)
		diagnostic = gcnew Diagnostic("");

	CObArray arMessages;
	pDiagnostic->ToArray(arMessages);
	for (int i = 0; i <= arMessages.GetUpperBound(); i++)
	{
		CDiagnosticItem* pItem = (CDiagnosticItem*) arMessages.GetAt(i);
		// priority level manage diagnostic type
		DiagnosticType type;
            switch (pItem->GetType())
            {
			case CDiagnostic::Error: type = DiagnosticType::Error; break;
			case CDiagnostic::Warning: type = DiagnosticType::Warning; break;
			case CDiagnostic::Info: type = DiagnosticType::Information; break;
            }
		CString sMessage = pItem->GetMessageText();
		diagnostic->Set(type, gcnew System::String(sMessage));
	}
	delete pDiagnostic;
	return diagnostic;
}

//-----------------------------------------------------------------------------
CLoginContext* TBApplicationProxy::GetLoginContext()
{
	return AfxGetLoginContext(CString(authenticationToken));
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::ShowAboutFramework()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);
	
	AfxInvokeAsyncThreadGlobalProcedure(pContext->m_nThreadID, &::ShowAboutFramework);
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::ChangeOperationsDate()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);
	
	AfxInvokeAsyncThreadGlobalProcedure(pContext->m_nThreadID, &::ChangeOperationsDate);
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::SetApplicationDate(DateTime date)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);
	DataDate aDate(date.Day, date.Month, date.Year);
	AfxInvokeThreadGlobalProcedure<DataDate>
		(pContext->m_nThreadID, 
		&::SetApplicationDate,
		aDate);
}

//-----------------------------------------------------------------------------
bool TBApplicationProxy::OpenEnumsViewer(System::String^ culture, System::String^ installation)
{
	CString sCulture (culture);
	CString sInstallation (installation);
	return AfxRunEnumsViewer(sCulture, sInstallation) == TRUE;
}

//-----------------------------------------------------------------------------
bool TBApplicationProxy::CloseEnumsViewer()
{
	return AfxCloseEnumsViewer() == TRUE;
}

//-----------------------------------------------------------------------------
DateTime TBApplicationProxy::GetApplicationDate()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CLoginContext* pContext = GetLoginContext();
	if (!pContext)
		return DateTime::Now;

	DataDate aDate = AfxInvokeThreadGlobalFunction<DataDate>
		(pContext->m_nThreadID, 
		&::GetApplicationDate);

	return DateTime(aDate.Year(), aDate.Month(), aDate.Day());
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::ClearCache()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	AfxInvokeThreadGlobalProcedure
		(AfxGetApp()->m_nThreadID, 
		&::ClearCache);
}

//-----------------------------------------------------------------------------
bool TBApplicationProxy::OnBeforeCanCloseTB()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	CWinApp* pApp = AfxGetApp();
	if (!pApp) //when application fails to start
		return TRUE;

	CLoginContext* pContext = GetLoginContext();
	if (pContext == NULL)
		return TRUE;
	
	return (bool)AfxInvokeThreadGlobalFunction<DataBool>(pContext->m_nThreadID, &::OnBeforeCanCloseTB) != 0;
}

//-----------------------------------------------------------------------------
bool TBApplicationProxy::CanCloseTB()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CWinApp* pApp = AfxGetApp();
	if (!pApp) //when application fails to start
		return TRUE;

	return (bool)AfxInvokeThreadGlobalFunction<DataBool>
		(pApp->m_nThreadID, 
		&::CanCloseTB) != 0;
}
//-----------------------------------------------------------------------------
bool TBApplicationProxy::CanCloseLogin()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	if (!AfxGetApp()) //when application fails to start
		return TRUE;

	CLoginContext* pContext = GetLoginContext();
	if (pContext == NULL)
		return TRUE;

	return (bool)AfxInvokeThreadGlobalFunction<DataBool>
		(pContext->m_nThreadID, 
		&::CanCloseLogin) != 0;
}
//-----------------------------------------------------------------------------
bool TBApplicationProxy::CanChangeLogin(bool lockTB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CLoginContext* pContext = GetLoginContext();
	if (!pContext)
		return true;
	
	return (bool)AfxInvokeThreadGlobalFunction<DataBool, DataBool>
		(pContext->m_nThreadID, 
		&::CanChangeLogin,
		lockTB) != 0;
}

//-----------------------------------------------------------------------------
int TBApplicationProxy::GetOpenDocuments()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);
	
	int result = AfxInvokeThreadFunction<int, CLoginContext>
		(
			pContext->m_nThreadID, 
			pContext,
			&CLoginContext::GetOpenDocuments) ;
	return result;
}

//-----------------------------------------------------------------------------
int TBApplicationProxy::GetOpenDocumentsInDesignMode() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);
	
	int result = AfxInvokeThreadFunction<int, CLoginContext>
		(
			pContext->m_nThreadID, 
			pContext,
			&CLoginContext::GetOpenDocumentsInDesignMode) ;
	return result;
}

//-----------------------------------------------------------------------------
int TBApplicationProxy::ChangeLogin
		(
		System::String^ oldAuthenticationToken, 
		System::String^ newAuthenticationToken,
		bool unlock
		)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	CWinApp* pApp = AfxGetApp();
	ASSERT(pApp);
	int result = AfxInvokeThreadGlobalFunction<DataInt, DataStr, DataStr, DataBool>
		(pApp->m_nThreadID, 
		&::ChangeLogin,
		CString(oldAuthenticationToken),
		CString(newAuthenticationToken),
		unlock);

	if (result == 0)
		authenticationToken = newAuthenticationToken;

	return result;
}

//-----------------------------------------------------------------------------
MenuStrip^ TBApplicationProxy::CreateMenuStrip(System::IntPtr Handle, System::IntPtr MenuHandle)
{
	return TBMenuStrip::FromMenuHandle(Handle, MenuHandle);
}

//-----------------------------------------------------------------------------
bool TBApplicationProxy::RunBatchInUnattendedMode(System::String^ documentNamespace, System::String^ xmlParams, int% documentHandle, cli::array<System::String^>^% messages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);
	DataStr documentNamespaceParam = documentNamespace;
	DataStr xmlParamsParam = xmlParams;
	DataLng documentHandleParam = documentHandle;
	DataObjArray messagesParam;

	DataBool b = AfxInvokeThreadGlobalFunction<DataBool, DataStr, DataStr, DataLng&, DataObjArray&>
		(
		pContext->m_nThreadID,
		&::RunBatchInUnattendedMode, 
		documentNamespaceParam,
		xmlParamsParam,
		documentHandleParam,
		messagesParam
		);
	
	ArrayList ^list = gcnew ArrayList();
	for (int i = 0; i < messagesParam.GetCount(); i++)
		list->Add(gcnew System::String((messagesParam[i]->Str())));
	
	messages = (cli::array<System::String^>^)list->ToArray(System::String::typeid);
	documentHandle = documentHandleParam;
	return TRUE == (BOOL)b;
}

//-----------------------------------------------------------------------------
bool TBApplicationProxy::RunReportInUnattendedMode(int woormInfo, System::String^ xmlParams, int% reportHandle, cli::array<System::String^>^% messages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLoginContext* pContext = GetLoginContext();
	ASSERT(pContext);
	
	DataLng woormInfoParam = woormInfo;
	DataStr xmlParamsParam = xmlParams;
	DataLng reportHandleParam = reportHandle;
	DataObjArray messagesParam;

	DataBool b = AfxInvokeThreadGlobalFunction<DataBool, DataLng, DataStr, DataLng&, DataObjArray&>
		(
		pContext->m_nThreadID,
		&::RunReportInUnattendedMode, 
		woormInfoParam,
		xmlParamsParam,
		reportHandleParam,
		messagesParam
		);
	
	ArrayList ^list = gcnew ArrayList();
	for (int i = 0; i < messagesParam.GetCount(); i++)
		list->Add(gcnew System::String((messagesParam[i]->Str())));
	
	messages = (cli::array<System::String^>^)list->ToArray(System::String::typeid);
	reportHandle = reportHandleParam;
	return TRUE == (BOOL)b;
}

//-----------------------------------------------------------------------------
void TBApplicationProxy::InitLockManager()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	CLockManagerInterface* pLockManager = AfxCreateLockManager();
	pLockManager->Init(_T(""));
	delete pLockManager;
}

