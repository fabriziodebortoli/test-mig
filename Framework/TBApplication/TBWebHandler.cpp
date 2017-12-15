#include "stdafx.h"
#include <TbNameSolver\Templates.h>
#include <TbNameSolver\LoginContext.h>
#include <TbGenlibManaged\GlobalFunctions.h>
#include <TbGenlibManaged\MenuFunctions.h>
#include <TbGenlibManaged\HelpManager.h>
#include <TbWoormEngine\REPORT.H>
#include <tbges\DocumentSession.h>
#include "TBWebHandler.h"


CTbWebHandler::CTbWebHandler()
	:
	CTbGenericHandler("tb/document")
{
	functionMap.SetAt(_T("command/"),						&CTbWebHandler::CommandFunction);
	functionMap.SetAt(_T("getImage/"),						&CTbWebHandler::GetImageFunction);
	functionMap.SetAt(_T("runFunction/"),					&CTbWebHandler::RunFunction);
	functionMap.SetAt(_T("getHotlinkQuery/"),				&CTbWebHandler::GetHotlinkQuery);
	functionMap.SetAt(_T("initTBLogin/"),					&CTbWebHandler::InitTBLoginFunction);
	functionMap.SetAt(_T("doLogoff/"),						&CTbWebHandler::DoLogoffFunction);
	functionMap.SetAt(_T("canLogoff/"),						&CTbWebHandler::CanLogoffFunction);
	functionMap.SetAt(_T("getProductInfo/"),				&CTbWebHandler::GetProductInfoFunction);
	functionMap.SetAt(_T("getWebSocketsPort/"),				&CTbWebHandler::GetWebSocketsPortFunction);
	functionMap.SetAt(_T("getApplicationDate/"),			&CTbWebHandler::GetApplicationDateFunction);
	functionMap.SetAt(_T("changeApplicationDate/"),			&CTbWebHandler::ChangeApplicationDateFunction);
	functionMap.SetAt(_T("getOnlineHelpUrl/"),				&CTbWebHandler::GetOnlineHelpFunction);
	functionMap.SetAt(_T("getThemes/"),						&CTbWebHandler::GetThemesFunction);
	functionMap.SetAt(_T("changeThemes/"),					&CTbWebHandler::ChangeThemesFunction);
	
	functionMap.SetAt(_T("getAllAppsAndModules/"),		&CTbWebHandler::GetAllAppsAndModules);
	functionMap.SetAt(_T("setAppAndModule/"),			&CTbWebHandler::SetAppAndModule);
	functionMap.SetAt(_T("createNewContext/"),			&CTbWebHandler::CreateNewContext);
	functionMap.SetAt(_T("getDefaultContext/"),			&CTbWebHandler::GetDefaultContextXml);
	functionMap.SetAt(_T("runEasyStudio/"),				&CTbWebHandler::RunEasyStudioFunction);
	functionMap.SetAt(_T("refreshEasyBuilderApps/"),	&CTbWebHandler::RefreshEasyBuilderApps);
	functionMap.SetAt(_T("getCurrentContext/"),			&CTbWebHandler::GetCurrentContext);
	functionMap.SetAt(_T("isEasyStudioDocument/"),		&CTbWebHandler::IsEasyStudioDocumentFunction);
	functionMap.SetAt(_T("getCustomizationsForDocument/"), &CTbWebHandler::GetCustomizationsForDocumentFunction);
	functionMap.SetAt(_T("assignWoormParameters/"),			&CTbWebHandler::AssignWoormParameters);
	functionMap.SetAt(_T("cloneEasyStudioDocument/"),	&CTbWebHandler::CloneEasyStudioDocumentFunction);
	functionMap.SetAt(_T("closeCustomizationContext/"), &CTbWebHandler::CloseCustomizationContextFunction);
	functionMap.SetAt(_T("canModifyContext/"), &CTbWebHandler::CanModifyContextFunction);
	
}

CTbWebHandler::~CTbWebHandler()
{
	TB_OBJECT_LOCK(&m_Cache);

	POSITION pos = m_Cache.GetStartPosition();
	while (pos)
	{
		CString sKey;
		CachedFile* pCachedFile = NULL;
		m_Cache.GetNextAssoc(pos, sKey, pCachedFile);
		delete pCachedFile;
	}
}

//--------------------------------------------------------------------------------
BOOL CTbWebHandler::ExecuteOnLoginThread(FUNCPTR func, const CString& path, const CNameValueCollection& params, CTBResponse& response)
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
			return TRUE;
		}

		CLoginContext* pContext = AfxGetLoginContext(authToken);
		if (pContext)
		{
			AfxInvokeThreadProcedure<CTbWebHandler, const CString&, const CNameValueCollection&, CTBResponse&>
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
void CTbWebHandler::SetAppAndModule(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString appName = params.GetValueByName(_T("app"));
	CString modName = params.GetValueByName(_T("mod"));
	CString isThisPairDefault = params.GetValueByName(_T("def"));
	SetApplicAndModule(appName, modName);
	if (isThisPairDefault == "true")
		SetDefaultContextMF(appName, modName);
}
//--------------------------------------------------------------------------------
void CTbWebHandler::CreateNewContext(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString appName = params.GetValueByName(_T("app"));
	CString modName = params.GetValueByName(_T("mod"));
	CString type = params.GetValueByName(_T("type"));
	CreateNewContextNF(appName, modName, type);
}

//--------------------------------------------------------------------------------
void CTbWebHandler::CanModifyContextFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response) {
	bool r = !ThereAreOpenedDocumentsWithES();
	CString res(r ? "true" : "false");
	response.SetData(res);
	response.SetMimeType(L"text/plain");
}

//--------------------------------------------------------------------------------
void CTbWebHandler::GetAllAppsAndModules(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	response.SetData(GetEasyBuilderApps());
	response.SetMimeType(L"application/json");
}
//--------------------------------------------------------------------------------
void CTbWebHandler::GetDefaultContextXml(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString appName, modName;
	CString resp;

	GetDefaultContextMF(appName, modName);

	CString res(appName + _T(";") + modName);
	if (appName == "" || modName == "")
		res = "";

	response.SetData(res);
	response.SetMimeType(L"application/json");
}
//--------------------------------------------------------------------------------
void CTbWebHandler::CloseCustomizationContextFunction(const CString & path, const CNameValueCollection & params, CTBResponse & response)
{
	CJSonResponse jsonResponse;
	if (ThereAreOpenedDocumentsWithES())
		jsonResponse.SetError();
	else
	{
		CloseCustomizationContext();
		jsonResponse.SetOK();
	}
	response.SetData(jsonResponse.GetJson());
	response.SetMimeType(L"application/json");
}
//--------------------------------------------------------------------------------
void CTbWebHandler::RunEasyStudioFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	RunEasyStudio(path, params);
	response.SetMimeType(L"None");
}
//--------------------------------------------------------------------------------
void CTbWebHandler::RunEasyStudio(const CString& path, const CNameValueCollection& params)
{
	CString ns = params.GetValueByName(_T("ns"));
	if (!ns || ns.IsEmpty())
		return;
	CString customizationName = params.GetValueByName(_T("customization"));
	CRestartDocumentInvocationInfo* pInfo = new CRestartDocumentInvocationInfo(CBaseDocument::DM_RUNTIME);

	if (!customizationName.IsEmpty() && customizationName!="undefined")
		pInfo->SetCustomizationName(customizationName);

	CBaseDocument* pDoc = AfxGetTbCmdManager()->RunDocument(CString(ns), szDefaultViewMode, FALSE, NULL, NULL, NULL, NULL, NULL, FALSE, FALSE, pInfo);

}
//--------------------------------------------------------------------------------
void CTbWebHandler::RefreshEasyBuilderApps(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	RefreshESApps(path, params);
	response.SetData(GetEasyBuilderApps());
	response.SetMimeType(L"application/json");
}
//--------------------------------------------------------------------------------
void CTbWebHandler::GetCurrentContext(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString appName, modName;
	GetEasyBuilderAppAndModule(appName, modName);
	if (appName.IsEmpty() || modName.IsEmpty())
		return;

	CString res(appName + _T(";") + modName);
	response.SetData(res);
	response.SetMimeType(L"application/json");
}
//--------------------------------------------------------------------------------
void CTbWebHandler::IsEasyStudioDocumentFunction(const CString & path, const CNameValueCollection & params, CTBResponse & response)
{
	CString ns = params.GetValueByName(_T("ns"));
	CTBNamespace aNs(CTBNamespace::DOCUMENT, ns);
	AddOnModule* pAddOnMod = AfxGetAddOnModule(aNs);

	CJSonResponse jsonResponse;
	jsonResponse.SetMessage(pAddOnMod && pAddOnMod->m_bIsCustom ? _T("true") : _T("false"));
	response.SetData(jsonResponse.GetJson());
	response.SetMimeType(L"application/json");
}

//--------------------------------------------------------------------------------
void CTbWebHandler::GetCustomizationsForDocumentFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString docNs = params.GetValueByName(_T("ns"));
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(GetEasyBuilderAppAssembliesPathsAsJson(docNs, pContext));
		response.SetMimeType(L"application/json");
	}
}

//--------------------------------------------------------------------------------
void CTbWebHandler::CloneEasyStudioDocumentFunction(const CString & path, const CNameValueCollection & params, CTBResponse & response)
{
	CString newNs = params.GetValueByName(_T("newNamespace"));
	response.SetMimeType(L"None");
	const CDocumentDescription* pDescription = AfxGetDocumentDescription(newNs);
	if (pDescription)
	{
		CJSonResponse jsonResponse;
		jsonResponse.SetMessage
		(
			cwsprintf
			(
				_TB("Document {0-%s} is already existing.\r\nPlease, change your name."),
				pDescription->GetName()
			)
		);
		response.SetData(jsonResponse.GetJson());
		return;
	}

	CloneEasyStudioDocument(params);
}
//--------------------------------------------------------------------------------
bool CTbWebHandler::CloneEasyStudioDocument(const CNameValueCollection & params)
{
	CString originalNs = params.GetValueByName(_T("ns"));
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	CString newNs = params.GetValueByName(_T("newNamespace"));
	CString newTitle = params.GetValueByName(_T("newTitle"));

	AfxInvokeAsyncThreadFunction
		<
		bool,
		CTbWebHandler,
		CString,
		CString,
		CString
		>
		(
			AfxGetLoginContext(authToken)->m_nThreadID,
			this,
			&CTbWebHandler::CloneEasyStudioDocument,
			originalNs,
			newNs,
			newTitle
			);
	return true;
}
//--------------------------------------------------------------------------------
bool CTbWebHandler::CloneEasyStudioDocument(CString strOriginalNs, CString strNewNs, CString strNewTitle)
{
	CTBNamespace newNs(CTBNamespace::DOCUMENT, strNewNs);

	// already existing
	if (AfxGetDocumentDescription(newNs))
		return false;
	return ::CloneAsEasyStudioDocument(strOriginalNs, strNewNs, strNewTitle) == TRUE;
}

//--------------------------------------------------------------------------------
void CTbWebHandler::GetProductInfoFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(GetJsonProductInfo());
		response.SetMimeType(L"application/json");
	}
}
//--------------------------------------------------------------------------------
void CTbWebHandler::GetWebSocketsPortFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CJsonSerializer ser;
	ser.WriteInt(_T("port"), GetWebSocketsConnectorPort());
	response.SetData(ser);
}

//--------------------------------------------------------------------------------
void CTbWebHandler::GetThemesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbWebHandler::GetThemesFunction, path, params, response))
		return;

	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		response.SetData(GetJsonThemesList());
		response.SetMimeType(L"application/json");
	}
}

//--------------------------------------------------------------------------------
CString CTbWebHandler::GetJsonThemesList()
{
	CStringArray themes;

	AfxGetPathFinder()->GetAvailableThemesFullNames(themes);

	CString strFullPath = AfxGetThemeManager()->GetThemeFullPath();

	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("Themes"));
	jsonSerializer.OpenArray(_T("Theme"));

	for (int i = 0; i <= themes.GetUpperBound(); i++)
	{
		jsonSerializer.OpenObject(i);

		jsonSerializer.WriteString(_T("path"), themes[i]);

		if (strFullPath.CompareNoCase(themes[i]) == 0)
			jsonSerializer.WriteBool(_T("isFavoriteTheme"), true);

		jsonSerializer.CloseObject();
	}
	jsonSerializer.CloseArray();
	jsonSerializer.CloseObject();

	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
void CTbWebHandler::ChangeThemesFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if (ExecuteOnLoginThread(&CTbWebHandler::ChangeThemesFunction, path, params, response))
		return;

	response.SetMimeType(L"application/json");
	CLoginContext* pContext = AfxGetLoginContext();
	CJSonResponse jsonResponse;
	if (pContext)
	{
		if (pContext->GetOpenDocuments() > 0)
		{
			jsonResponse.SetError();
			jsonResponse.SetMessage(_TB("It is not possible to change the theme at this point.\r\nPlease, check whether there is any opened document in the application framework."));
			response.SetData(jsonResponse.GetJson());
		}
		else
		{
			CString themePath = params.GetValueByName(_T("theme"));
			ChangeTheme(themePath, pContext);
			jsonResponse.SetOK();
		}
	}
	response.SetData(jsonResponse.GetJson());
}

//--------------------------------------------------------------------------------
void CTbWebHandler::CanLogoffFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CJSonResponse jsonResponse;
	CLoginThread* pThread = AfxGetLoginThread();
	if (pThread && pThread->GetOpenDocuments() > 0)
	{
		jsonResponse.SetError();
		jsonResponse.SetMessage(_TB("It is not possible to log out at this point.\r\nPlease, check whether there is any opened document in the application framework."));
	}
	else
	{
		jsonResponse.SetOK();
	}

	response.SetData(jsonResponse);
}

//--------------------------------------------------------------------------------
void CTbWebHandler::DoLogoffFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);

	CLoginContext* pContext = CTBRequestHandlerObj::GetLoginContext(authToken, FALSE);
	if (pContext)
		pContext->Close();
	CJSonResponse jsonResponse;
	jsonResponse.SetOK();

	response.SetData(jsonResponse);
}

//--------------------------------------------------------------------------------
void CTbWebHandler::GetDiagnosticMessages(CLoginContext* pContext, CString& messages)
{
	//TODOLUCA
	if (!pContext || !pContext->m_nThreadID)
	{
		//_TB("Invalid LoginContext. Check if company database is up to date."));
		messages.Append(_TB("Invalid LoginContext. Check if company database is up to date."));
		return;
	}

	CStringArray messageArray;
	CDiagnostic* pDiagnostic = NULL;
	pDiagnostic = AfxInvokeThreadGlobalFunction<CDiagnostic*, BOOL>(pContext->m_nThreadID, &CloneDiagnostic, false);
	pDiagnostic->ToStringArray(messageArray);
	for (int i = 0; i < messageArray.GetSize(); i++)
	{
		const CString  error = messageArray.GetAt(i);
		messages.Append(error + _T("\r\n"));
	}
	delete pDiagnostic;
}

//--------------------------------------------------------------------------------
void CTbWebHandler::InitTBLoginFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	CString isDesktop = params.GetValueByName(_T("isDesktop"));
	CJSonResponse jsonResponse;

	CString messages;
	bool ok = DbCheck(authToken, messages);
	if (!ok)
	{
		jsonResponse.SetError();
		jsonResponse.SetMessage(messages);
		response.SetData(jsonResponse);
		return;
	}

	//TODOLUCA, comunicazione che vengo da desktop, da rivedere
	BOOL isRemoteInterface = isDesktop != _T("true");

	//se siamo in desktop, comunico al tbappmanager il token, cos� se qualcuno fa logout dal tbappmanager sa quale token sloggare
	if (!isRemoteInterface)
		SendCurrentToken(AfxGetMenuWindowHandle(), authToken);

	CLoginContext* pContext = CTBRequestHandlerObj::GetLoginContext(authToken, TRUE);//forza la creazione del login context
	if (pContext == NULL || !pContext->IsValid())
	{
		//_TB("Invalid LoginContext. Check if company database is up to date."));
		jsonResponse.SetError();
		CString message;
		GetDiagnosticMessages(pContext, message);
		jsonResponse.SetMessage(message);
		response.SetData(jsonResponse);
		return;
	}

	response.SetCookie(AUTH_TOKEN_PARAM, authToken);
	
	AfxGetApplicationContext()->SetRemoteInterface(isRemoteInterface);
	if (!isRemoteInterface)
	{
		//TODOLUCA, primo step per gestione menu web in desktop
		const CLoginInfos* pInfo = pContext->GetLoginInfos();
		if (pInfo) 
		{
			CString messages;
			CString userName = pInfo->m_strUserName;
			CString applicationLanguage = pInfo->m_strApplicationLanguage;
			InitTBPostLogin(AfxGetMenuWindowHandle(), authToken, userName, false, applicationLanguage, applicationLanguage, messages);
		}
	}
	jsonResponse.SetOK();
	response.SetData(jsonResponse);
}

//--------------------------------------------------------------------------------
void CTbWebHandler::GetApplicationDateFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	//date di applicazione
	CString date = AfxGetApplicationDate().FormatDataForXML();
	CLoginContext* pContext = GetLoginContext(params);
	if (!pContext)
	{
		response.SetMimeType(L"None");
		return;
	}
		
	//culture corrente
	CString culture = AfxGetCultureInfo()->GetUICulture();
	if (culture.IsEmpty())
		culture = _T("en");

	//formattazione della data
	DataDate pDate = AfxGetApplicationDate();
	CString formatString = _T("");
	CDateFormatter* pFormatter = (CDateFormatter*)AfxGetFormatStyleTable()->GetFormatter(pDate.GetDataType(), NULL);
	if (pFormatter)
	{
		CDateFormatHelper::FormatTag  format = pFormatter->GetFormat();
		switch (format)
		{
		case	CDateFormatHelper::FormatTag::DATE_DMY:
			formatString = _T("dd/MM/yyyy");
			break;
		case	CDateFormatHelper::FormatTag::DATE_MDY:
			formatString = _T("MM/dd/yyyy");
			break;
		case	CDateFormatHelper::FormatTag::DATE_YMD:
			formatString = _T("yyyy/MM/dd");
			break;
		}
	}
	
	CJSonResponse jsonResponse;
	jsonResponse.OpenObject(_T("dateInfo"));

	jsonResponse.WriteString(L"applicationDate", date);
	jsonResponse.WriteString(L"culture", culture);
	jsonResponse.WriteString(L"formatDate", formatString);

	jsonResponse.CloseObject();

	jsonResponse.CloseObject();

	response.SetData(jsonResponse);
	response.SetMimeType(L"application/json");

}

//--------------------------------------------------------------------------------
void CTbWebHandler::ChangeApplicationDateFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	response.SetMimeType(L"application/json");
	CJSonResponse jsonResponse;
	if (AfxGetLoginThread()->GetOpenDocuments() > 0)
	{
		jsonResponse.SetError();
		jsonResponse.SetMessage(_TB("It is not possible to change the operation date at this point.\r\nPlease, check whether there is any opened document in the application framework."));
		response.SetData(jsonResponse.GetJson());
		return;
	}

	CString day = params.GetValueByName(_T("day"));
	CString month = params.GetValueByName(_T("month"));
	CString year = params.GetValueByName(_T("year"));

	int d = _ttoi(day);
	int m = _ttoi(month);
	int y = _ttoi(year);

	DataDate date(d, m, y);
	AfxSetApplicationDate(date);
	//ChangeOperationsDate();
	jsonResponse.SetOK();
	response.SetData(jsonResponse.GetJson());
}

//--------------------------------------------------------------------------------
void CTbWebHandler::GetOnlineHelpFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	CString nameSpace = params.GetValueByName(_T("nameSpace"));

	ShowHelp(nameSpace);
	response.SetMimeType(L"application/json");
	CJSonResponse jsonResponse;
	jsonResponse.SetOK();
	response.SetData(jsonResponse.GetJson());
}

//--------------------------------------------------------------------------------
void CTbWebHandler::ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	FUNCPTR fn;
	if (functionMap.Lookup(path, fn))
	{
		CString cmpId = params.GetValueByName(_T("cmpId"));
		if (!cmpId.IsEmpty())
		{
			DWORD pId = 0;
			DWORD tId = GetWindowThreadProcessId((HWND)_ttoi(cmpId), &pId);
			AfxInvokeThreadProcedure<CTbWebHandler, const CString&, const CNameValueCollection&, CTBResponse&>
				(tId, this, fn, path, params, response);
			return;
		}
		CString autToken = params.GetValueByName(AUTH_TOKEN_PARAM);
		if (!autToken.IsEmpty())
		{
			CLoginContext* pContext = AfxGetLoginContext(autToken);
			if (pContext)
			{
				// ulteriore controllo per tradurre la form di login e inizializzarla con la Preferred Language
				if (pContext->GetLoginInfos() && AfxGetCulture().IsEmpty())
					AfxGetThreadContext()->SetUICulture(pContext->GetLoginInfos()->m_strPreferredLanguage);

				AfxInvokeThreadProcedure<CTbWebHandler, const CString&, const CNameValueCollection&, CTBResponse&>
					(pContext->m_nThreadID, this, fn, path, params, response);
				return;
			}
		}

		CWinApp* pApp = AfxGetApp();
		if (pApp)
		{
			AfxInvokeThreadProcedure<CTbWebHandler, const CString&, const CNameValueCollection&, CTBResponse&>
				(pApp->m_nThreadID, this, fn, path, params, response);
			return;
		}
		(this->*(fn))(path, params, response);
		
		return;
	}

	//confronto il puntatore di inizio stringa col puntatore alla prima occorrenza della stringa cercata
	//perch� la stringa deve essere all'inizio
	/*if ((LPCTSTR)path == wcsstr(path, L"image/"))
	{
		SetMimeType(path, response);
		GetImage(params, ((LPCTSTR)path) + 6 , response);
		return;
	}*/

	__super::ProcessRequest(path, params, response);
}

//--------------------------------------------------------------------------------
void CTbWebHandler::CommandFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CJSonResponse aResponse;
	CDocumentSession* pSession = (CDocumentSession*) AfxGetThreadContext()->m_pDocSession;
	ENSURE_SESSION();
	pSession->SuspendPushToClient();
	CString sId = params.GetValueByName(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);

	CString cmpId = params.GetValueByName(_T("cmpId"));
	SendMessage((HWND)_ttoi(cmpId), WM_COMMAND, id, NULL);
	pSession->ResumePushToClient();
	aResponse.SetOK();
	response.SetData(aResponse.GetJson());
	
}

//--------------------------------------------------------------------------------
void CTbWebHandler::RunFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString nsFunction = params.GetValueByName(_T("ns"));
	CString args = params.GetValueByName(_T("args"));

	CFunctionDescription aFunc;
	CJSonResponse jsonResponse;

	if (!AfxGetTbCmdManager()->GetFunctionDescription((LPCTSTR)nsFunction, aFunc))
	{
		jsonResponse.SetMessage(_TB("Function not found"));
		jsonResponse.SetError();
	}
	else if (!aFunc.ParseArguments(args))
	{
		jsonResponse.SetMessage(_TB("Invalid function arguments"));
		jsonResponse.SetError();
	}
	else if (!AfxGetTbCmdManager()->RunFunction(&aFunc))
	{
		jsonResponse.SetMessage(_TB("Cannot execute function"));
		jsonResponse.SetError();
	}
	else
	{
		CDataObjDescription pRet = aFunc.GetReturnValueDescription();
		//unparse dei parametri di output
		DataStr dsArgs;
		BOOL bOk = aFunc.UnparseArguments(dsArgs, L"Arguments");	//TODO filtrare solo i parametri ref/out  e unparsare un oggetto json
		ASSERT_TRACE2(bOk, "Unknown type: %s(%s)", (LPCTSTR)nsFunction, (LPCTSTR)args);
		if (bOk)
			args = dsArgs;
		else args.Empty();

		if (bOk)
			jsonResponse.SetOK();

		DataStr dsReturnValue; 
		pRet.Unparse(dsReturnValue);
		CString sRetValue = dsReturnValue;
		jsonResponse.WriteString(L"returnValue", sRetValue);
		
		//pRet->SerializeToJson(jsonResponse);   // da ripristinare quando anche gli args saranno in json
		jsonResponse.WriteString(L"args", args);
		jsonResponse.CloseObject();
	}
	response.SetData(jsonResponse);
}

//--------------------------------------------------------------------------------
void CTbWebHandler::GetHotlinkQuery(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString sDocumentID = params.GetValueByName(_T("cmpId"));
	CJSonResponse aResponse;
	HotKeyLink* pHkl = NULL;
	if (!sDocumentID.IsEmpty())
	{
		CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
		ENSURE_SESSION();
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromHwnd((HWND)_ttoi(sDocumentID));
		if (!pDoc)
		{
			aResponse.SetMessage(_TB("Invalid document ID.")); 
			return;
		}
		CString hklName = params.GetValueByName(_T("hklName"));
		if (hklName.IsEmpty())
		{
			aResponse.SetMessage(_TB("Hotlink istance name seems missing...."));
			return;
		}
		pHkl = pDoc->GetHotLink(hklName);
	}
	CString nsHkl = params.GetValueByName(_T("ns"));
	CString args = params.GetValueByName(_T("args"));
	DataInt nAction; nAction.Assign(params.GetValueByName(_T("action")));
	CString sFilter = params.GetValueByName(_T("filter"));
	DataStr ds = AfxGetTbCmdManager()->GetHotlinkQuery(nsHkl, args, nAction, sFilter, pHkl);


	aResponse.SetOK();
	aResponse.WriteString(_T("query"), ds.GetString());
	response.SetData(aResponse);
}

//--------------------------------------------------------------------------------
void CTbWebHandler::AssignWoormParameters(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString autToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	if (autToken.IsEmpty())
	{
		return;
	}

	CString cmpId = params.GetValueByName(_T("woormdocProxyId"));
	HWND hWndWoormDoc = NULL;
	if (cmpId.IsEmpty())
	{
		return;		
	}
	hWndWoormDoc = (HWND)_ttoi(cmpId);

	CString reportOutParameters = params.GetValueByName(_T("args"));
	
	CWnd* pWnd = CWnd::FromHandlePermanent(hWndWoormDoc);

	
	AfxInvokeAsyncThreadProcedure<CTbWebHandler, HWND,  CString>(hWndWoormDoc, this, &CTbWebHandler::AssignWoormParametersInternal, hWndWoormDoc, reportOutParameters);
}

 
//--------------------------------------------------------------------------------
void CTbWebHandler::AssignWoormParametersInternal(HWND hWndWoormDoc, CString reportOutParameters)
{
	CAbstractWoormFrame* pAbstractWoormFrame = dynamic_cast<CAbstractWoormFrame*>(CWnd::FromHandlePermanent(hWndWoormDoc));
	if (pAbstractWoormFrame)
	{
		pAbstractWoormFrame->AssignWoormParameters(reportOutParameters);
	}
}

//--------------------------------------------------------------------------------
void CTbWebHandler::GetImageFunction(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CString sSrc = params.GetValueByName(_T("src"));
	if (sSrc.IsEmpty())
	{
		return;
	}
	// 30days image cache validity (60sec * 60min * 24hours * 30days)
	response.SetHeader(L"Cache-Control", L"max-age=2592000");
	CachedFile* pCachedFile = NULL;
	{
		TB_OBJECT_LOCK_FOR_READ(&m_Cache);
		//first check in cache
		if (m_Cache.Lookup(sSrc, pCachedFile))
		{
			//clone the buffer for the response
			//(will be deleted when response has been sent)
			BYTE * pData = new BYTE[pCachedFile->m_len];
			memcpy_s(pData, pCachedFile->m_len, pCachedFile->m_data, pCachedFile->m_len);
			response.SetData(pData, pCachedFile->m_len);
			return;
		}
	}


	BYTE* buffer = NULL;
	int nLen = 0;
	CImageBuffer iBuffer;
	GetImageBytes(sSrc, iBuffer);
	iBuffer.GetData(nLen, buffer);
	
	if (!buffer)
		return;

	{
		TB_OBJECT_LOCK(&m_Cache);
		if (!m_Cache.Lookup(sSrc, pCachedFile))//retry reading, perhaps someone added meanwhile
		{
			//clone another buffer for cache
			BYTE * pData = new BYTE[nLen];
			memcpy_s(pData, nLen, buffer, nLen);
			m_Cache.SetAt(sSrc, new CachedFile(pData, nLen));
		}
	}
	BYTE * pData = new BYTE[nLen];
	memcpy_s(pData, nLen, buffer, nLen);
	response.SetData(pData, nLen);

}

//--------------------------------------------------------------------------------
CString GetJsonProductInfo()
{
	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("ProductInfos"));

	jsonSerializer.WriteString(_T("installationVersion"), AfxGetLoginManager()->GetInstallationVersion());
	jsonSerializer.WriteString(_T("providerDescription"), AfxGetLoginInfos()->m_strProviderDescription);
	jsonSerializer.WriteString(_T("edition"), AfxGetLoginManager()->GetEditionType());
	jsonSerializer.WriteString(_T("installationName"), AfxGetPathFinder()->GetInstallationName());
	jsonSerializer.WriteString(_T("activationState"), AfxGetCommonClientObjects() ? AfxGetCommonClientObjects()->GetActivationStateInfo() : _T(""));

	CString debugState;
	// aggiunge la release di TB
#ifdef _DEBUG
	debugState += _TB("(Debug version)");
#endif

	jsonSerializer.WriteString(_T("debugState"), debugState);

	jsonSerializer.OpenArray(_T("Applications"));



	// accoda i nomi e le relase di tutte le add-on application
	CString sActive;
	CString sAppName;
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOn = AfxGetAddOnAppsTable()->GetAt(i);
		ASSERT(pAddOn);

		sAppName = pAddOn->GetTitle();
		if (sAppName.IsEmpty())
			sAppName = pAddOn->m_strAddOnAppName;

		CString strVersions;
		sActive = AfxIsAppActivated(pAddOn->m_strAddOnAppName) ? _TB("Licensed") : _TB("Not Licensed");
		strVersions += cwsprintf(_TB("{0-%s} rel. {1-%s} "), sAppName, pAddOn->GetAppVersion());

		jsonSerializer.OpenObject(i);

		jsonSerializer.WriteString(_T("application"), strVersions);
		jsonSerializer.WriteString(_T("licensed"), sActive);

		jsonSerializer.CloseObject();
	}

	jsonSerializer.CloseArray();
	jsonSerializer.CloseObject();

	return jsonSerializer.GetJson();
}

