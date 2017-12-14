#include "stdafx.h"
#include <TbNameSolver\Templates.h>
#include <TbGes\DocumentSession.h>
#include "TBWebHandler.h"
#include "TBGenericHandler.h"

CTbGenericHandler::CTbGenericHandler(LPCSTR szPath)
	: CTBRequestHandlerObj(szPath)
{
	m_strRoot = AfxGetPathFinder()->GetModulePath(CTBNamespace(_T("Module.Framework.TbApplication")), CPathFinder::STANDARD) + _T("\\") + _T("Files") + _T("\\");
	m_strStandard = AfxGetPathFinder()->GetStandardPath() + _T("\\");

}

CTbGenericHandler::~CTbGenericHandler()
{
}
//--------------------------------------------------------------------------------
bool CTbGenericHandler::PreProcessRequest(const CString& method, CMap<CString, LPCTSTR, CString, LPCTSTR>& requestHeaders, const CString& path, CTBResponse& response)
{
	CString origin;
	requestHeaders.Lookup(_T("Origin"), origin);
	if (origin.IsEmpty())
		origin = _T("http://localhost:4200");

	CString requestedHeaders;
	requestHeaders.Lookup(_T("Access-Control-Request-Headers"), requestedHeaders);
	response.SetHeader(_T("Access-Control-Allow-Origin"), origin);
	response.SetHeader(_T("Access-Control-Allow-Credentials"), _T("true"));
	response.SetHeader(_T("Access-Control-Allow-Headers"), requestedHeaders);
	if (method == "OPTIONS")
	{
		response.SetHeader(_T("Allow"), _T("POST, GET, OPTIONS"));
		response.SetHeader(_T("Access-Control-Allow-Methods"), _T("POST, GET, OPTIONS"));
		return false;//only headers, no further processing
	}
	return true;

}
//--------------------------------------------------------------------------------
void CTbGenericHandler::ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	// ulteriore controllo per tradurre la form di login e inizializzarla con la Preferred Language
	if (AfxGetCulture().IsEmpty())
		AfxGetThreadContext()->SetUICulture(AfxGetCommonClientObjects()->GetServerConnectionInfo()->m_sPreferredLanguage);

	if ((LPCTSTR)path == wcsstr(path, L"getWebSocketsPort/"))
	{
		response.SetMimeType(L"text/plain");
		CString s;
		s.Format(_T("%d"), GetWebSocketsConnectorPort());
		response.SetData(s);
		return;
	}

	CString file = m_strRoot + path;
	if (ReadFileContent(file, response))
	{
		SetMimeType(path, response);
		return;
	}

	file = m_strStandard + path;
	if (ReadFileContent(file, response))
	{
		SetMimeType(path, response);
		return;
	}
	response.SetData(cwsprintf(_TB("Invalid request: %s"), path));
	response.SetMimeType(_T("text/plain"));
	response.SetStatus(404);
}


//----------------------------------------------------------------------------
CLoginContext* CTbGenericHandler::GetLoginContext(const CNameValueCollection& params, BOOL bCreate)
{
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	if (authToken.IsEmpty())
		return NULL;

	return __super::GetLoginContext(authToken, bCreate);
}