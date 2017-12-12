#include "stdafx.h"
#include <TbNameSolver\Templates.h>
#include "RestApiHandler.h"
#include "DocumentSession.h"
#include "TbRequestHandler.h"



const TCHAR szTbJSONDocNamespace[] = _T("Document.Framework.TbGes.TbGes.TbJSONDocument");

CRestApiHandler::CRestApiHandler(CTbRequestHandler* pRequestHandler)
	:
	CTBRequestHandlerObj("rest-api"),
	m_pRequestHandler(pRequestHandler)
{
}


CRestApiHandler::~CRestApiHandler()
{
}

//--------------------------------------------------------------------------------
void CRestApiHandler::ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	CJSonResponse aResponse;
	BOOL bResult = TRUE;
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	BOOL bRunDocument = path == wcsstr(path, L"runJSONDocument/");
	//document session needs creation only for rundocument actions
	//otherwise it should exist
	CDocumentSession* pSession = m_pRequestHandler->GetDocumentSession(params, bRunDocument);

	if (!pSession)
	{
		aResponse.SetError();
		aResponse.SetMessage(_TB("Invalid session. Please login again."));
		response.SetData(aResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}
	if (bRunDocument)
	{
		CString sJSON = params.GetValueByName(_T("jsonData"));
		bResult = AfxInvokeThreadFunction<BOOL, CDocumentSession, CString, LPAUXINFO, BOOL, ObjectType>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::RunObject, szTbJSONDocNamespace, &sJSON, TRUE, DOCUMENT);
	}
	else if (path == wcsstr(path, L"getData/"))
	{
		CString sDbtName = params.GetValueByName(_T("dbtName"));
		CString sWithChilds = params.GetValueByName(_T("includeSlaves"));
		AfxInvokeThreadProcedure<CDocumentSession, const CString&, BOOL, CJSonResponse&>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::GetData, sDbtName, _tcscmp(sWithChilds, _T("true")) == 0, aResponse);
	}
	else if (path == wcsstr(path, L"setData/"))
	{
		CString sJSONData = params.GetValueByName(_T("jsonData"));

		bResult = AfxInvokeThreadFunction<BOOL, CDocumentSession, const CString&, CJSonResponse&>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::SetData, sJSONData, aResponse);
	}
	else if (path == wcsstr(path, L"getBrowserData/"))
	{
		AfxInvokeThreadProcedure<CDocumentSession, CJSonResponse&>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::GetBrowserData, aResponse);
	}
	else if (path == wcsstr(path, L"getMessages/"))
	{
		CString sClear = params.GetValueByName(_T("clear"));
		AfxInvokeThreadProcedure<CDocumentSession, CJSonResponse&, BOOL>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::GetMessages, aResponse, _tcscmp(sClear, _T("true")) == 0);
	}
	else if (path == wcsstr(path, L"browseRecord/"))
	{
		CString sJSON = params.GetValueByName(_T("jsonKey"));
		AfxInvokeThreadProcedure<CDocumentSession, const CString&>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::Browse, sJSON);
	}
	else if (path == wcsstr(path, L"moveFirst/"))
	{
		AfxInvokeThreadProcedure<CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::MoveFirst);
	}
	else if (path == wcsstr(path, L"moveLast/"))
	{
		AfxInvokeThreadProcedure<CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::MoveLast);
	}
	else if (path == wcsstr(path, L"movePrevious/"))
	{
		AfxInvokeThreadProcedure<CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::MovePrev);
	}
	else if (path == wcsstr(path, L"moveNext/"))
	{
		AfxInvokeThreadProcedure<CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::MoveNext);
	}
	else if (path == wcsstr(path, L"edit/"))
	{
		bResult = AfxInvokeThreadFunction<BOOL, CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::Edit);
	}
	else if (path == wcsstr(path, L"new/"))
	{
		bResult = AfxInvokeThreadFunction<BOOL, CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::New);
	}
	else if (path == wcsstr(path, L"delete/"))
	{
		bResult = AfxInvokeThreadFunction<BOOL, CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::Delete);
	}
	else if (path == wcsstr(path, L"save/"))
	{
		bResult = AfxInvokeThreadFunction<BOOL, CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::Save);
	}
	else if (path == wcsstr(path, L"escape/"))
	{
		AfxInvokeThreadProcedure<CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::Escape);
	}
	aResponse.SetResult(bResult);
	response.SetData(aResponse.GetJson());
	response.SetMimeType(L"text/json");
	return;
}
