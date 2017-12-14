#include "StdAfx.h"
#include "TbMenuHandlerGDI.h"

#include <TbNameSolver\PathFinder.h>
#include <TbGenlib\TBCommandInterface.h>
#include <TBApplication\LoginThread.h>
#include <TBApplication\TbCommandManager.h>
#include <TbGenLibManaged\MenuFunctions.h>
#include "TbGes\SoapFunctions.h"
#include "TbGenlibUI\SoapFunctions.h"

//--------------------------------------------------------------------------------
CTbMenuHandlerGDI::CTbMenuHandlerGDI()
{
	functionMap.SetAt(_T("runDocument/"), &CTbMenuHandlerGDI::RunDocumentFunction);
	functionMap.SetAt(_T("runReport/"), &CTbMenuHandlerGDI::RunReportFunction);
	functionMap.SetAt(_T("runFunction/"), &CTbMenuHandlerGDI::RunFunctionFunction);
	functionMap.SetAt(_T("runOfficeItem/"), &CTbMenuHandlerGDI::RunOfficeItemFunction);
	functionMap.SetAt(_T("closeDocument/"), &CTbMenuHandlerGDI::CloseDocumentFunction);
	functionMap.SetAt(_T("showProductInfo/"), &CTbMenuHandlerGDI::ShowProductInfoFunction);
	functionMap.SetAt(_T("openUrlLink/"), &CTbMenuHandlerGDI::OpenUrlLinkFunction);
	functionMap.SetAt(_T("newLogin/"), &CTbMenuHandlerGDI::NewLoginFunction);
	functionMap.SetAt(_T("activateViaInternet/"), &CTbMenuHandlerGDI::ActivateViaInternetFunction);
	functionMap.SetAt(_T("doLogin/"), &CTbMenuHandlerGDI::DoLoginFunction);
	functionMap.SetAt(_T("postLogin/"), &CTbMenuHandlerGDI::PostLoginFunction);
	functionMap.SetAt(_T("runUrl/"), &CTbMenuHandlerGDI::RunUrlFunction);
	functionMap.SetAt(_T("getInstallationInfo/"), &CTbMenuHandlerGDI::GetInstallationInfoFunction);
	
}

//--------------------------------------------------------------------------------
CTbMenuHandlerGDI::~CTbMenuHandlerGDI(void)
{
	
}

//--------------------------------------------------------------------------------
CLoginContext* CTbMenuHandlerGDI::Login(CString authenticationToken)
{
	CLoginContext* pContext = __super::Login(authenticationToken);

	//BOOL bLoadMailerFailed = FALSE;
	//if (AfxIsActivated(szExtensionsApp, _NS_ACT("TbMailer")))
	//	bLoadMailerFailed = (AfxGetTbCmdManager()->RunFunction(_NS_WEB("Extensions.TbMailer.TbMailer.StartMailConnector"), NULL) == NULL);

	//// MailConnector is optional 
	//if (bLoadMailerFailed)
	//	AfxGetDiagnostic()->Add(_TB("The Mail Connector extension module is not loaded."), CDiagnostic::Warning);


	AfxGetApplicationContext()->SetRemoteInterface(false);
	return pContext;
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//per minimizzare il numero di chiamate per thread e if, adesso nell'url in arrivo potrebbe arrivare la keyword "needLoginThread".
	//nel caso, viene immediatamente rigirata la chiamata sul loginthread, e rimossa la keyword dall'indirizzo
	if ( ExecutedOnLoginInfinity(path, params, response))//TODO LARA
		return;

	FUNCPTR temp;
	if (functionMap.Lookup(path, temp))
	{
		(this->*(temp))(path, params, response);
		return;
	}

	__super::ProcessRequest(path, params, response);
}

//--------------------------------------------------------------------------------
BOOL CTbMenuHandlerGDI::ExecuteOnLoginThread(FUNCPTR func, const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (!pContext)
	{
		CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
		if (authToken.IsEmpty())
		{
			response.SetData(_TB("Cannot process request on login thread, authentication token is empty."));
			response.SetMimeType(_T("text/plain"));
			response.SetStatus(401);
			return FALSE;
		}

		CLoginContext* pContext = AfxGetLoginContext(authToken);
		if (pContext)
		{
			AfxInvokeThreadProcedure<CTbMenuHandlerGDI, const CString&, const CNameValueCollection&, CTBResponse&>
				(
					pContext->m_nThreadID,
					this,
					func,
					path,
					params,
					response
					);
			return TRUE;
		}
		
	}
	return FALSE;
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::RunDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandlerGDI::RunDocumentFunction, path, params, response))
		return;

	CJSonResponse jsonResponse;
	RunDocument(params);
	jsonResponse.SetOK();
	response.SetMimeType(L"application/json");
	response.SetData(jsonResponse);
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::RunReportFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandlerGDI::RunReportFunction, path, params, response))
		return;

	
	RunReport(params);

	CJSonResponse jsonResponse;	
	jsonResponse.SetOK();
	jsonResponse.WriteBool(_T("desktop"), false);
	response.SetData(jsonResponse.GetJson());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::RunFunctionFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandlerGDI::RunFunctionFunction, path, params, response))
		return;

	RunFunction(params);
	CJSonResponse jsonResponse;
	jsonResponse.SetOK();
	response.SetMimeType(L"application/json");
	response.SetData(jsonResponse);
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::RunOfficeItemFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandlerGDI::RunOfficeItemFunction, path, params, response))
		return;

	RunOfficeItem(params);
	CJSonResponse jsonResponse;
	RunDocument(params);
	jsonResponse.SetOK();
	response.SetMimeType(L"application/json");
	response.SetData(jsonResponse);
}


//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::CloseDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandlerGDI::CloseDocumentFunction, path, params, response))
		return;

	response.SetMimeType(L"None");
	CloseDocument(params);
}


//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::ShowProductInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandlerGDI::ShowProductInfoFunction, path, params, response))
		return;

	::ShowAboutFramework();
	response.SetMimeType(L"None");
}


//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::OpenUrlLinkFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	ShellExecute(params.GetValueByName(_T("link")));
	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::NewLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	PostMessage(AfxGetMenuWindowHandle(), UM_NEW_LOGIN, NULL, NULL);
	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::ActivateViaInternetFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	PostMessage(AfxGetMenuWindowHandle(), UM_ACTIVATE_INTERNET, NULL, NULL);
	response.SetMimeType(L"None");
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::DoLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CJSonResponse jsonResponse;
	CString sUser = params.GetValueByName(_T("user"));
	CString sCompany = params.GetValueByName(_T("company"));
	CString sPassword = params.GetValueByName(_T("password"));
	bool sOverWrite = params.GetValueByName(_T("overwrite")) == _T("true");
	bool sRemember = params.GetValueByName(_T("rememberme")) == _T("true");
	bool winNT = params.GetValueByName(_T("winNT")) == _T("true");
	bool ccd = params.GetValueByName(_T("ccd")) == _T("true");
	bool relogin = params.GetValueByName(_T("relogin")) == _T("true");
	bool changeAutologinInfo = params.GetValueByName(_T("changeAutologinInfo")) == _T("true");
	CString saveAutologinInfo = params.GetValueByName(_T("saveAutologinInfo"));
	//chiama core - dologin
	CString sMessage;
	bool alreadyLogged = false, changePassword = false;
	CString sToken = DoLogin(sUser, sPassword, sCompany, sRemember, winNT, sOverWrite, ccd, relogin, sMessage, alreadyLogged, changePassword, changeAutologinInfo, saveAutologinInfo);
	if (!sToken.IsEmpty())
	{
		CLoginContext* pContext = CTBRequestHandlerObj::GetLoginContext(sToken, TRUE);//forza la creazione del login context
		if (pContext && pContext->IsValid())
		{
			response.SetCookie(AUTH_TOKEN_PARAM, sToken);
			jsonResponse.SetOK();
			jsonResponse.WriteString(L"menuPage", GetMenuPage());
		}
		else
		{
			jsonResponse.SetError();
			jsonResponse.WriteString(L"messages", _TB("Invalid LoginContext. Check if company database is up to date."));
		}
	}
	else
	{
		jsonResponse.SetError();
		if (alreadyLogged)
			jsonResponse.WriteBool(L"alreadyLogged", true);
		if (changePassword)
			jsonResponse.WriteBool(L"changePassword", true);
		if (changeAutologinInfo)
			jsonResponse.WriteBool(L"changeAutologinInfo", true);
	}
	if (!sMessage.IsEmpty())
		jsonResponse.WriteJsonFragment(L"messages", sMessage);
	response.SetData(jsonResponse.GetJson());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::PostLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandlerGDI::PostLoginFunction, path, params, response))
		return;

	CString token = params.GetValueByName(_T("token"));
	response.SetCookie(AUTH_TOKEN_PARAM, token);
	AfxGetApplicationContext()->CloseAllLogins(TRUE);
	response.SetMimeType(L"None");
}


//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::RunUrlFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbMenuHandlerGDI::RunUrlFunction, path, params, response))
		return;

	CString ns = params.GetValueByName(_T("url"));
	CString title = params.GetValueByName(_T("title"));

	SendMessage(AfxGetMenuWindowHandle(), UM_OPEN_URL, (WPARAM)(LPCTSTR)ns, (LPARAM)(LPCTSTR)title);
}

//--------------------------------------------------------------------------------
void CTbMenuHandlerGDI::GetInstallationInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CJsonSerializer ser;
	ser.OpenObject();
	ser.WriteBool(_T("desktop"), true);
	ser.CloseObject();
	response.SetData(ser.GetJson());
	response.SetMimeType(L"application/json");
}
//--------------------------------------------------------------------------------
bool CTbMenuHandlerGDI::CloseDocument(const CNameValueCollection& params)
{
	CString handleString = params.GetValueByName(_T("handle"));
	long handle = _wtoi((LPCTSTR)(handleString));

	::PostMessage((HWND)handle, WM_CLOSE, (WPARAM)handle, (LPARAM)NULL);
	return true;
}


//--------------------------------------------------------------------------------
bool CTbMenuHandlerGDI::RunDocument(const CNameValueCollection& params)
{
	CString ns = params.GetValueByName(_T("ns"));
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM); //TODO LARA
	CString args = params.GetValueByName(L"args");
	CString sKeyArgs = params.GetValueByName(L"keyArgs");
	BOOL	bForegroundArgs = params.GetValueByName(L"foreground").CompareNoCase(_T("true")) == 0;
	AfxInvokeThreadFunction
		<
		bool,
		CTbMenuHandlerGDI,
		CString,
		CString,
		CString,
		BOOL
		>
		(
			AfxGetLoginContext(authToken)->m_nThreadID,
			this,
			&CTbMenuHandlerGDI::RunDocument,
			ns,
			args,
			sKeyArgs,
			bForegroundArgs
			);
	return true;
}

//template <typename T>
//
//void SetWord(T Hi, T Low, DWORD &out)
//{
//		out = (Low & 0x0000FFFF) + (Hi << 16);
//}
//--------------------------------------------------------------------------------
bool CTbMenuHandlerGDI::RunDocument(CString ns, CString sAuxInfoArgs, const CString sKeyArgs, BOOL bForeground)
{
	CBaseDocument* pDoc = AfxGetTbCmdManager()->RunDocument(ns, szDefaultViewMode, FALSE, NULL, sAuxInfoArgs != _T("") ? (LPAUXINFO)(LPCTSTR)sAuxInfoArgs : NULL);
	if (pDoc == NULL)//TODO LARA 
		return pDoc != NULL;
	DataBool bOk;
	if (bForeground)
	{
		HWND window = pDoc->GetFrameHandle();

		RECT rect;
		::GetWindowRect(window, &rect);

		AttachThreadInput(GetWindowThreadProcessId(GetForegroundWindow(), NULL), GetCurrentThreadId(), TRUE);
		SendMessage(window, WM_LBUTTONDOWN, MK_LBUTTON, MAKELPARAM(10, 10));
		SendMessage(window, WM_LBUTTONUP, MK_LBUTTON, MAKELPARAM(10, 10));
	}

	if (!sKeyArgs.IsEmpty())	//valorizza le chiavi primarie a partire da una stringa con formato fieldName : FieldValue; fieldName1:fieldValue1
	{
		AfxInvokeThreadFunction<BOOL, CBaseDocument, const CString&>(pDoc->GetFrameHandle(), pDoc, &CBaseDocument::GoInBrowserMode, sKeyArgs);
	}
	return pDoc != NULL;
}

//--------------------------------------------------------------------------------
bool CTbMenuHandlerGDI::RunReport(const CNameValueCollection& params)
{
	CString ns = params.GetValueByName(_T("ns"));
	CString args = params.GetValueByName(_T("args"));
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	
	DataLng res = AfxInvokeThreadGlobalFunction<DataLng, DataStr, DataStr>(AfxGetLoginContext(authToken)->m_nThreadID, &::RunReport, CString(ns), CString(args));
	return TRUE;
}

//--------------------------------------------------------------------------------
bool CTbMenuHandlerGDI::RunFunction(const CNameValueCollection& params)
{
	CString ns = params.GetValueByName(_T("ns"));
	CString args = params.GetValueByName(_T("args"));

	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	
	AfxInvokeThreadGlobalProcedure<DataStr, DataStr>
		(AfxGetLoginContext(authToken)->m_nThreadID,
		&::RunFunctionInNewThread,
		CString(ns),
		CString(args));

	return TRUE;
}

//--------------------------------------------------------------------------------
bool CTbMenuHandlerGDI::RunOfficeItem(const CNameValueCollection& params)
{
	CString ns = params.GetValueByName(_T("ns"));
	CString subType = params.GetValueByName(_T("subType"));
	CString app = params.GetValueByName(_T("application"));
	
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);

	CLoginContext* pContext = AfxGetLoginContext();
	if (!pContext)
		return false;

	return ProcessOfficeFile(app, ns, subType, AfxGetLoginContext());
}

//--------------------------------------------------------------------------------
bool CTbMenuHandlerGDI::RunHistory(CString link)
{
	OpenHistoryLink(AfxGetMenuWindowHandle(), link);
	return true;
}

//--------------------------------------------------------------------------------
bool CTbMenuHandlerGDI::RunHistory(const CNameValueCollection& params)
{
	CString link = params.GetValueByName(_T("link"));
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	
	AfxInvokeAsyncThreadFunction
		<
		bool,
		CTbMenuHandlerGDI,
		CString
		>
		(
			AfxGetLoginContext(authToken)->m_nThreadID,
			this, 
			&CTbMenuHandlerGDI::RunHistory,	
			link
		);
	return true;
}
		
