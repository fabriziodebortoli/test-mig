#include "StdAfx.h"
#include "TbRequestHandler.h"

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGenlib\TBCommandInterface.h>
#include <TbGenlib\PARSCTRL.H>
#include <TbGenLibManaged\GlobalFunctions.h>
#include <TbGenLibManaged\Main.h>
#include <TBApplication\LoginThread.h>
#include <TBApplication\TaskBuilderApp.h>
#include <TBApplication\TbCommandManager.h>
#include <TbGes\BODYEDIT.H>
#include <TbGenLibManaged\MenuFunctions.h>
#include "DocumentThread.h"

#define SESSION_TIMEOUT 1200000 //20 minutes
const TCHAR szNeedLoginThreadUrl[] = _T("needLoginThread/");
const int nNeedLoginThreadUrlLen = _tcslen(szNeedLoginThreadUrl);

const TCHAR szNeedLoginInfinityUrl[] = _T("needLoginInfinity/");
const int szNeedLoginInfinityUrllLen = _tcslen(szNeedLoginInfinityUrl);

class AddOnModule;

#define ENSURE_SESSION() \
	if (!pSession)\
				{\
		aResponse.SetMessage(_TB("Invalid session. Please login again."));\
		return;\
				} 

#define ADD_LOCALIZED_CONTENT(key, value) \
	jsonSerializer.OpenObject(elements);\
	jsonSerializer.WriteString(_T("key"), key);\
	jsonSerializer.WriteString(_T("value"), value);\
	jsonSerializer.CloseObject();\
	elements++;\

//----------------------------------------------------------------------------
CLoginContext* CTbRequestHandler::Login(CString authenticationToken)
{
	CLoginContext* pContext = AfxGetLoginContext(authenticationToken);
	if (!pContext)
	{ 

		AfxGetTbCmdManager()->Login(authenticationToken);
		pContext = AfxGetLoginContext(authenticationToken);
	}
	
	AfxGetApplicationContext()->SetRemoteInterface(TRUE);
	
	return pContext;
}

//----------------------------------------------------------------------------
///restituisce il login context associato al token; se non esiste, lo crea
//resetta il timer di timeout di sessione
CLoginContext* CTbRequestHandler::GetLoginContext(const CString& authToken, BOOL bCreate)
{
	TB_LOCK_FOR_WRITE();
	CLoginContext* pContext = AfxGetLoginContext(authToken);
	if (!pContext && bCreate)
	{
		pContext = AfxGetLoginContext(authToken);
		if (!pContext)
		{
			pContext = AfxInvokeThreadFunction<CLoginContext*, CTbRequestHandler, CString>(AfxGetApplicationContext()->GetAppMainWnd(), this, &CTbRequestHandler::Login, authToken);
		}
	}

	if (!pContext || !pContext->IsValid())
	{
		return NULL;
	}
	//if we are in IIS context, login thread has a timeout
	if (m_bIsIISModule)
		AfxInvokeAsyncThreadProcedure<CLoginThread, UINT>(pContext->m_nThreadID, (CLoginThread*)pContext, &CLoginThread::StartTimeoutTimer, SESSION_TIMEOUT);
	return pContext;
}


//----------------------------------------------------------------------------
CDocumentSession* CTbRequestHandler::GetDocumentSession(const CNameValueCollection& params, BOOL bCreate, BOOL bNotApiHooked)
{
	CString sessionId = params.GetValueByName(_T("session"));
	if (sessionId.IsEmpty())
		return NULL;
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	if (authToken.IsEmpty())
		return NULL;

	CLoginContext* pContext = GetLoginContext(authToken, bCreate);
	if (!pContext)
		return NULL;
	return (CDocumentSession*)pContext->GetDocumentSession(sessionId, bCreate, bNotApiHooked);
}
//----------------------------------------------------------------------------
void CTbRequestHandler::RunObject(const CNameValueCollection& params, CJSonResponse& aResponse, ObjectType type)
{
	CString sNamespace = params.GetValueByName(_T("ns"));
	CString sNotHooked = params.GetValueByName(_T("notHooked"));

	BOOL bNotHooked = sNotHooked == _T("true");
	CDocumentSession* pSession = GetDocumentSession(params, TRUE, bNotHooked);
	if (!pSession)
	{
		CString sessionId = params.GetValueByName(_T("session"));
		if (sessionId.IsEmpty())
		{
			DataGuid dg;
			dg.AssignNewGuid();
			aResponse.WriteString(L"windowUrl", L"document.html?type=" + cwsprintf(L"%d", type) + L"&ns=" + sNamespace + L"&session=" + dg.Str(FALSE) + (bNotHooked ? L"&notHooked=true" : L""));
			aResponse.SetOK();
		}
		else
		{
			aResponse.SetError();
			aResponse.SetMessage(L"Invalid Session");
		}
		return;
	}

	AfxInvokeAsyncThreadFunction<BOOL, CDocumentSession, CString, LPAUXINFO, BOOL, ObjectType>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::RunObject, sNamespace, NULL, FALSE, type);
	aResponse.SetOK();
}

//----------------------------------------------------------------------------
void CTbRequestHandler::CloseDocument(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);

	CDocumentSession* pSession = GetDocumentSession(params);

	ENSURE_SESSION();

	AfxInvokeAsyncThreadProcedure<CDocumentSession>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::CloseDocument);
	aResponse.SetOK();
}

//----------------------------------------------------------------------------
void CTbRequestHandler::CloseDialog(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);

	CDocumentSession* pSession = GetDocumentSession(params);

	CString sControlId = GetControlId(params);

	HWND hWndFrom = GetHWND(sControlId);

	ENSURE_SESSION();

	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::CloseDialog, hWndFrom);
	aResponse.SetOK();
}
//--------------------------------------------------------------------------------
void CTbRequestHandler::HKLClick(const CNameValueCollection& params, bool bLower, CJSonResponse& aResponse)
{
	// TODO refactor this, it changes in just the invoked procedure parameters.
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = GetControlId(params);

	int nPos = sControlId.FindOneOf(_T("_"));
	long nObjectId = _ttol(nPos > 0 ? sControlId.Left(nPos) : sControlId);
	long nSubElemId = _ttol(nPos > 0 ? sControlId.Mid(nPos + 1) : 0);

	const CString sFromWindow = params.GetValueByName(_T("fromControl"));
	HWND hWndFrom = GetHWND(sFromWindow);
	const CString sValue = params.GetValueByName(_T("value"));

	if (!nObjectId)
	{
		aResponse.SetMessage(_TB("Missing document nObjectId id!"));
		return;
	}


	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, CString, HWND, UINT>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::ButtonClick, hWndFrom, sValue, (HWND)nObjectId, (UINT)bLower);
	aResponse.SetOK();
}

//--------------------------------------------------------------------------------
BOOL CTbRequestHandler::MenuInfinity(CLoginContext* pContext)
{
	CString file = AfxGetPathFinder()->GetUserLogPath(AfxGetLoginInfos()->m_strUserName, AfxGetLoginInfos()->m_strCompanyName) + SLASH_CHAR + _T("IMAGO.log");
	try {
		CString xml = CheckMenuReload(pContext, AfxGetLoginManager()->GetSystemDBConnectionString(), file);

		if (!xml.IsEmpty())
			return ParseMenuInfinity(pContext, AfxGetLoginManager()->GetSystemDBConnectionString(), xml, file);
	}
	catch (CApplicationErrorException* e)
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		return false;
	}
	return true;
}


//--------------------------------------------------------------------------------
void CTbRequestHandler::ButtonClick(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = GetControlId(params);

	int nPos = sControlId.FindOneOf(_T("_"));
	long nObjectId = _ttol(nPos > 0 ? sControlId.Left(nPos) : sControlId);
	long nSubElemId = _ttol(nPos > 0 ? sControlId.Mid(nPos + 1) : 0);

	const CString sFromWindow = params.GetValueByName(_T("fromControl"));
	HWND hWndFrom = GetHWND(sFromWindow);
	const CString sValue = params.GetValueByName(_T("value"));

	if (!nObjectId)
	{
		aResponse.SetMessage(_TB("Missing document nObjectId id!"));
		return;
	}


	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, CString, HWND, UINT>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::ButtonClick, hWndFrom, sValue, (HWND)nObjectId, (UINT)nSubElemId);
	aResponse.SetOK();

}

//--------------------------------------------------------------------------------
void CTbRequestHandler::RadarDblClick(const CNameValueCollection& params, CJSonResponse& aResponse)
{

	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = GetControlId(params);

	int iIndex = _ttoi(params.GetValueByName(_T("index")));
	int nPos = sControlId.FindOneOf(_T("_"));
	long nObjectId = _ttol(nPos > 0 ? sControlId.Left(nPos) : sControlId);
	/*long nSubElemId = _ttol(nPos > 0 ? sControlId.Mid(nPos + 1) : 0);*/

	// TODO: I guess we do not need this. Focus is on radar, but 
	// user can not change anything from the radar, right?
	const CString sFromWindow = params.GetValueByName(_T("fromControl"));
	HWND hWndFrom = GetHWND(sFromWindow);
	const CString sValue = params.GetValueByName(_T("value"));

	if (!nObjectId)
	{
		aResponse.SetMessage(_TB("Missing document nObjectId id!"));
		return;
	}


	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, CString, HWND, UINT>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::RadarDblClick, hWndFrom, sValue, (HWND)nObjectId, (UINT)iIndex);
	aResponse.SetOK();
}

//--------------------------------------------------------------------------------
void CTbRequestHandler::RadarScroll(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	// var params = { "session": window.session, "controlId" : sID }; params.direction = 'down';
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = GetControlId(params);

	long nObjectId = _ttol(sControlId);

	// TODO: I guess we do not need this. Focus is on radar, but 
	// user can not change anything from the radar, right?
	const CString sFromWindow = params.GetValueByName(_T("fromControl"));
	HWND hWndFrom = GetHWND(sFromWindow);
	const CString sValue = params.GetValueByName(_T("value"));

	int iDirection = _ttoi(params.GetValueByName(_T("direction")));

	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, CString, HWND, INT>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::RadarScroll, hWndFrom, sValue, (HWND)nObjectId, iDirection);

	aResponse.SetOK();
}

//--------------------------------------------------------------------------------
void CTbRequestHandler::RadarSelect(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	// var params = { "session": window.session, "controlId" : sID, "selected" : index };
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = GetControlId(params);

	long nObjectId = _ttol(sControlId);

	CString sSelectedId = params.GetValueByName(_T("selected"));
	int iSelectedId = _ttoi(sSelectedId);

	// TODO: I guess we do not need this. Focus is on radar, but 
	// user can not change anything from the radar, right?
	const CString sFromWindow = params.GetValueByName(_T("fromControl"));
	HWND hWndFrom = GetHWND(sFromWindow);
	const CString sValue = params.GetValueByName(_T("value"));

	int iDirection = _ttoi(params.GetValueByName(_T("direction")));

	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, CString, HWND, INT>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::RadarSelect, hWndFrom, sValue, (HWND)nObjectId, iSelectedId);

	aResponse.SetOK();
}

//--------------------------------------------------------------------------------
void CTbRequestHandler::ButtonArrowClick(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = GetControlId(params);

	int nPos = sControlId.FindOneOf(_T("_"));
	long nObjectId = _ttol(nPos > 0 ? sControlId.Left(nPos) : sControlId);
	long nSubElemId = _ttol(nPos > 0 ? sControlId.Mid(nPos + 1) : 0);

	const CString sFromWindow = params.GetValueByName(_T("fromControl"));
	HWND hWndFrom = GetHWND(sFromWindow);
	const CString sValue = params.GetValueByName(_T("value"));

	if (!nObjectId)
	{
		aResponse.SetMessage(_TB("Missing document nObjectId id!"));
		return;
	}


	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, CString, HWND, UINT>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::ButtonArrowClick, hWndFrom, sValue, (HWND)nObjectId, (UINT)nSubElemId);
	aResponse.SetOK();
}
//--------------------------------------------------------------------------------
void CTbRequestHandler::MenuItemClick(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	// sendRequest("menuItemClick/", { "controlId": sender.command, "session": window.session, "fromControl": focusedControlInfo.id, "value": focusedControlInfo.val }, function (data) {
	CString sControlId = GetControlId(params);
	//	

	HWND hMenuWnd = GetHWND(sControlId);
	CString sCommandId = params.GetValueByName(_T("command"));
	UINT uCommand = _tstoi(sCommandId);
	//
	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, UINT>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::MenuItemClick, hMenuWnd, uCommand);

	aResponse.SetOK();
}
///eseguita nel thread della richiesta
//--------------------------------------------------------------------------------
bool CTbRequestHandler::GetImage(const CNameValueCollection& params, LPCTSTR id, CTBResponse& response)
{
	CString sCacheId = params.GetValueByName(_T("cacheId"));
	// 30days image cache validity (60sec * 60min * 24hours * 30days)
	response.SetHeader(L"Cache-Control", L"max-age=2592000");
	CachedFile* pCachedFile = NULL;
	if (!sCacheId.IsEmpty())
	{
		TB_OBJECT_LOCK_FOR_READ(&m_Cache);
		//first check in cache
		if (m_Cache.Lookup(sCacheId, pCachedFile))
		{
			//clone the buffer for the response
			//(will be deleted when response has been sent)
			BYTE * pData = new BYTE[pCachedFile->m_len];
			memcpy_s(pData, pCachedFile->m_len, pCachedFile->m_data, pCachedFile->m_len);
			response.SetData(pData, pCachedFile->m_len);
			return true;
		}
	}

	//then ask document objects for bitmap bytes
	CDocumentSession* pSession = GetDocumentSession(params);
	if (!pSession)
		return false;

	CString sIconSource = params.GetValueByName(_T("iconSource"));

	BYTE* buffer = NULL;
	int nLen = 0;
	CImageBuffer iBuffer;
	if (!sIconSource.IsEmpty())
	{
		if (!AfxInvokeThreadFunction<bool, CDocumentSession, LPCTSTR, CImageBuffer&>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::GetImageBytes, sIconSource, iBuffer))
			return false;
		iBuffer.GetData(nLen, buffer);
	}
	else //todo: da eliminare? Era solo per il caso immagine embedded nelle risorse?
	{
		if (!AfxInvokeThreadFunction<bool, CDocumentSession, LPCTSTR, BYTE*&, int&>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::GetImageBytes, id, buffer, nLen))
			return false;

	}
	if (!buffer)
		return false;

	if (!sCacheId.IsEmpty())
	{
		TB_OBJECT_LOCK(&m_Cache);
		if (!m_Cache.Lookup(sCacheId, pCachedFile))//retry reading, perhaps someone added meanwhile
		{
			//clone another buffer for cache
			BYTE * pData = new BYTE[nLen];
			memcpy_s(pData, nLen, buffer, nLen);
			m_Cache.SetAt(sCacheId, new CachedFile(pData, nLen));
		}
	}
	BYTE * pData = new BYTE[nLen];
	memcpy_s(pData, nLen, buffer, nLen);
	response.SetData(pData, nLen);
	return true;
}

//----------------------------------------------------------------------------
CString CTbRequestHandler::GetControlId(const CNameValueCollection& params, CString sProp /* = "controlId"*/)
{

	CString sOuterId = params.GetValueByName(sProp);
	int idxUnderscore = sOuterId.Find(_T('_'), 0);
	return sOuterId.Mid(idxUnderscore + 1);
}

//----------------------------------------------------------------------------
void CTbRequestHandler::ActivateTab(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();


	CString sControlId = GetControlId(params);


	if (sControlId.IsEmpty())
	{
		aResponse.SetMessage(_T("Missing control id!"));
		return;
	}

	//il tabId ha una parte iniziale che e' l'handle della finestra,
	//(e' costruito cosi: cwsprintf(_T("%d_%d"), pDialog->m_hWnd, pItem->GetDialogID());)
	//perche' possono coesistere piu tabber con una tab ciascuno che ha lo stesso Id sullo stesso thread.
	//depuro dalla parte dell'handle prima di usarlo 
	int idxUnderscore = sControlId.Find(_T('_'), 0);
	CString strTabHWND = sControlId.Left(idxUnderscore);
	CString strTabId = sControlId.Mid(idxUnderscore + 1);
	int nTabId = _ttoi(strTabId);
	HWND hTabberWnd = (HWND)_ttoi(strTabHWND);

	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, UINT>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::ActivateTab, hTabberWnd, (UINT)nTabId);
	aResponse.SetOK();
	return;
}

//-----------------------------------------------------------------------------
void CTbRequestHandler::GetComboItems(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = GetControlId(params);
	CString sQuery = params.GetValueByName(_T("query"));
	CString sStart = params.GetValueByName(_T("start"));
	CString sLimit = params.GetValueByName(_T("limit"));

	CStringArray items;

	int total = AfxInvokeThreadFunction<int, CDocumentSession, const CString&, const CString&, int, int, CStringArray&>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::GetComboItems, sControlId, sQuery, _ttoi(sStart), _ttoi(sLimit), items);
	aResponse.WriteInt(_T("num"), total);
	aResponse.OpenArray(_T("data"));
	for (int i = 0; i < items.GetSize(); i++)
	{
		aResponse.OpenObject(i);
		aResponse.WriteString(_T("val"), items.ElementAt(i));
		aResponse.CloseObject();

	}

	aResponse.CloseArray();

}
//-----------------------------------------------------------------------------
void CTbRequestHandler::GetBodyItems(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = params.GetValueByName(_T("controlId")).TrimLeft(L"id_t_");
	CString sStart = params.GetValueByName(_T("start"));
	CString sLimit = params.GetValueByName(_T("limit"));

	CStringArray items;

	Array descri;
	descri.SetOwns(TRUE);
	int total = AfxInvokeThreadFunction<int, CDocumentSession, const CString&, int, int, Array&>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::GetBodyItems, sControlId, _ttoi(sStart), _ttoi(sLimit), descri);
	aResponse.WriteInt(_T("num"), total);
	aResponse.OpenArray(_T("data"));
	for (int i = 0; i < descri.GetCount(); i++)
	{
		aResponse.OpenObject(i);

		CNameValueCollection* pRow = (CNameValueCollection*)descri.GetAt(i);
		for (int j = 0; j < pRow->GetCount(); j++)
		{
			CNameValuePair* pair = pRow->GetAt(j);
			aResponse.WriteString(pair->GetName(), pair->GetValue());
		}
		aResponse.CloseObject();
	}

	aResponse.CloseArray();
}

//-----------------------------------------------------------------------------
void CTbRequestHandler::SelectBodyEditRows(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = params.GetValueByName(_T("controlId")).TrimLeft(L"t_");
	CString sSelectedIds = params.GetValueByName(_T("selectedId"));
	int nTokenPos = 0;
	// split the selected ids string on "_" char.
	CString strToken = sSelectedIds.Tokenize(_T("_"), nTokenPos);
	CArray<int> aiSelectedIds;
	while (!strToken.IsEmpty())
	{
		aiSelectedIds.Add(_ttoi(strToken));

		strToken = sSelectedIds.Tokenize(_T("_"), nTokenPos);
	}

	AfxInvokeAsyncThreadProcedure<CDocumentSession, const CString, CString>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::SelectBodyEditRows, sControlId, sSelectedIds);

	aResponse.SetOK();
}
//-----------------------------------------------------------------------------
void CTbRequestHandler::MoveTo(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	const CString sFromWindow = GetControlId(params, _T("fromControl"));
	HWND hWndFrom = GetHWND(sFromWindow);
	const CString sValue = params.GetValueByName(_T("value"));
	const CString sToWindow = GetControlId(params, _T("toControl"));
	HWND hWndTo = GetHWND(sToWindow);
	const CString sCol = params.GetValueByName(_T("col"));
	int nCol = _ttoi(sCol);
	const CString sRow = params.GetValueByName(_T("row"));
	int nRow = _ttoi(sRow);
	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, CString, HWND, int, int>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::MoveTo, hWndFrom, sValue, hWndTo, nRow, nCol);
	aResponse.SetOK();
}

//-----------------------------------------------------------------------------
void CTbRequestHandler::Blur(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControl = GetControlId(params, _T("control"));
	CString sValue = params.GetValueByName(_T("value"));
	
	AfxInvokeAsyncThreadProcedure<CDocumentSession, CString, CString>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::Blur, sControl, sValue);
	aResponse.SetOK();
}
//-----------------------------------------------------------------------------
void CTbRequestHandler::HideMenu(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	CString sControlId = GetControlId(params);
	HWND hMenuWnd = GetHWND(sControlId);

	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::HideMenu, hMenuWnd);

	aResponse.SetOK();
}

//-----------------------------------------------------------------------------
void CTbRequestHandler::SelectedItem(const CNameValueCollection& params, CJSonResponse& aResponse) {
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();
	// ComboBox ID.
	const CString sWindow = GetControlId(params);
	// ComboBox window handle.
	HWND hWnd = GetHWND(sWindow);

	const CString sFromWindow = params.GetValueByName(_T("fromControl"));
	HWND hWndFrom = GetHWND(sFromWindow);
	const CString sValue = params.GetValueByName(_T("value"));
	std::vector<int> indexes;
	params.GetValuesByName(_T("selectedIndexes"), indexes);
	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, CString, HWND, std::vector<int>>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::SelectedItems, hWndFrom, sValue, hWnd, indexes);
	aResponse.SetOK();
}

//-----------------------------------------------------------------------------
void CTbRequestHandler::DoHyperLink(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();
	// ComboBox ID.
	const CString sWindow = GetControlId(params);
	// ComboBox window handle.
	HWND hWnd = GetHWND(sWindow);

	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::DoHyperLink, hWnd);
	aResponse.SetOK();
}
//-----------------------------------------------------------------------------
void CTbRequestHandler::AddItem(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	const CString sParentControl = params.GetValueByName(_T("parentControl"));
	const CString sType = params.GetValueByName(_T("type"));
	CWndObjDescription::WndObjType type = (CWndObjDescription::WndObjType)_ttoi(sType);
	AfxInvokeAsyncThreadProcedure<CDocumentSession, CString, CWndObjDescription::WndObjType>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::AddItem, sParentControl, type);
	aResponse.SetOK();
}

//-----------------------------------------------------------------------------
void CTbRequestHandler::CheckListChanged(const CNameValueCollection& params, CJSonResponse& aResponse)
{
	CDocumentSession* pSession = GetDocumentSession(params);
	ENSURE_SESSION();

	const CString sWindow = GetControlId(params);
	// ComboBox window handle.
	HWND hWnd = GetHWND(sWindow);

	const CString sRow = params.GetValueByName(_T("rowIndex"));
	int nRow = _ttoi(sRow);

	const CString sChecked = params.GetValueByName(_T("checked"));

	AfxInvokeAsyncThreadProcedure<CDocumentSession, HWND, int, CString>
		(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::CheckListChanged, hWnd, nRow, sChecked);
	aResponse.SetOK();
}

//----------------------------------------------------------------------------
CTbRequestHandler::CTbRequestHandler()
	:
	CTBRequestHandlerObj("tbloader")
{
	m_strRoot = AfxGetPathFinder()->GetModulePath(CTBNamespace(_T("Module.Framework.TbApplication")), CPathFinder::STANDARD) + _T("\\") + _T("Files") + _T("\\");
	m_strStandardInstallationPath = AfxGetPathFinder()->GetStandardPath();
	m_bIsIISModule = TRUE == AfxGetApplicationContext()->IsIISModule();
}

//----------------------------------------------------------------------------
CTbRequestHandler::~CTbRequestHandler(void)
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
void CTbRequestHandler::ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	// ulteriore controllo per tradurre la form di login e inizializzarla con la Preferred Language
	if (AfxGetCulture().IsEmpty())
		AfxGetThreadContext()->SetUICulture(AfxGetCommonClientObjects()->GetServerConnectionInfo()->m_sPreferredLanguage);

	//per minimizzare il numero di chiamate per thread e if, adesso nell'url in arrivo potrebbe arrivare la keyword "needLoginThread".
	//nel caso, viene immediatamente rigirata la chiamata sul loginthread, e rimossa la keyword dall'indirizzo
	if (ExecutedOnLoginThread(path, params, response))
		return;

	//confronto il puntatore di inizio stringa col puntatore alla prima occorrenza della stringa cercata
	//perché la stringa deve essere all'inizio
	if ((LPCTSTR)path == wcsstr(path, L"getUserObjects/"))
	{
		CJSonResponse jsonResponse;
		jsonResponse.WriteInt(L"User Objects", GetGuiResources(GetCurrentProcess(), GR_USEROBJECTS));
		jsonResponse.WriteInt(L"GDI Objects", GetGuiResources(GetCurrentProcess(), GR_GDIOBJECTS));
		response.SetData(jsonResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getloginCompanies/"))
	{
		CString user = params.GetValueByName(_T("user"));
		response.SetData(GetJsonCompaniesForUser(user));
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"doLogin/"))
	{
		CString sUser = params.GetValueByName(_T("user"));
		CString sCompany = params.GetValueByName(_T("company"));
		CString sPassword = params.GetValueByName(_T("password"));
		bool bOverWrite = params.GetValueByName(_T("overwrite")) == _T("true");
		//bool sRemember = params.GetValueByName(_T("rememberme")) == _T("true");
		bool winNT = params.GetValueByName(_T("winNT")) == _T("true");
		//bool ccd = params.GetValueByName(_T("ccd")) == _T("true");
		bool relogin = params.GetValueByName(_T("relogin")) == _T("true");
		bool changeAutologinInfo = params.GetValueByName(_T("changeAutologinInfo")) == _T("true");
		CString saveAutologinInfo = params.GetValueByName(_T("saveAutologinInfo")) ;
		CString sMessage;
		bool alreadyLogged = false, changePassword = false;
		CString sToken = DoLoginWeb(sUser, sPassword, sCompany, winNT, bOverWrite, relogin, sMessage, alreadyLogged, changePassword, changeAutologinInfo, saveAutologinInfo);

		CJSonResponse jsonResponse;
		if (!sToken.IsEmpty())
		{
			CLoginContext* pContext = GetLoginContext(sToken, TRUE);//forza la creazione del login context
			if (pContext && pContext->IsValid())
			{
				response.SetCookie(AUTH_TOKEN_PARAM, sToken);
				jsonResponse.SetOK();
				jsonResponse.WriteString(L"menuPage", L"newMenu.html");
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

			if (!sMessage.IsEmpty())
				jsonResponse.WriteJsonFragment(L"messages", sMessage);

		}
		response.SetData(jsonResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"ssologoff"))
	{
		CString cryptedtoken = params.GetValueByName(_T("tk"));
		DoSSOLogOff(cryptedtoken);
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"ssologin"))
	{
		CString cryptedtoken = params.GetValueByName(_T("tk"));
 		CString tblink = params.GetValueByName(_T("tblink"));
		CString user = params.GetValueByName(_T("user"));
		CString company = params.GetValueByName(_T("company"));
		CString pwd = params.GetValueByName(_T("password"));
		bool bOverWrite = params.GetValueByName(_T("overwrite")) == _T("true");
		bool winNT = params.GetValueByName(_T("winNT")) == _T("true");
		bool relogin = params.GetValueByName(_T("relogin")) == _T("true");
		bool changeAutologinInfo = params.GetValueByName(_T("changeAutologinInfo")) == _T("true");
		CString saveAutologinInfo = params.GetValueByName(_T("saveAutologinInfo"));
		CString sMessage;
		bool alreadyLogged = false;
		bool changePassword = false;
		int res = 0;
		CString sToken = DoSSOLoginWeb(cryptedtoken, user, pwd, company, winNT, bOverWrite, relogin, sMessage, alreadyLogged, changePassword, changeAutologinInfo, res, saveAutologinInfo);

		CJSonResponse jsonResponse;
		if (!sToken.IsEmpty())
		{
			CLoginContext* pContext = GetLoginContext(sToken, TRUE);//forza la creazione del login context
			CString file;
			response.SetCookie(AUTH_TOKEN_PARAM, sToken);

			
				BOOL bParsedMenu = AfxInvokeThreadFunction<BOOL, CTbRequestHandler, CLoginContext*>(pContext->m_nThreadID, this, &CTbRequestHandler::MenuInfinity, pContext);
				if (bParsedMenu)
					file = m_strRoot + L"newMenu.html";
				else
					file = m_strRoot + L"noMenu.html";

				if (ReadFileContent(file, response))
				{
					SetMimeType(L"newMenu.html", response);
					return;
				}
			
		}

		else
		{ 
			if (!cryptedtoken.IsEmpty()) 
			{
				if (!sMessage.IsEmpty() && res != 0) {
					jsonResponse.WriteJsonFragment(L"messages", sMessage);
					//if (res== 58)//(int)LoginReturnCodes.ImagoUserAlreadyAssociated = 58,
					//{
					response.SetData(jsonResponse.GetJson());
					response.SetMimeType(L"text/json");
					return;
				}
				//}
				//if (res == 13)
				//{
				//	jsonResponse.SetError();
				//	jsonResponse.WriteBool(L"passworderror", true);
				//	response.SetData(jsonResponse.GetJson());
				//	response.SetMimeType(L"text/json");
				//	return;
				//}
				else {
					CString file = m_strRoot + L"loginhost.html";
					if (ReadFileContent(file, response))
					{
						SetMimeType(L"loginhost.html", response);
						return;
					}
				}
			}
			else {
				jsonResponse.SetError();
				if (alreadyLogged)
					jsonResponse.WriteBool(L"alreadyLogged", true);
				if (changePassword)
					jsonResponse.WriteBool(L"changePassword", true);
				if (changeAutologinInfo)
					jsonResponse.WriteBool(L"changeAutologinInfo", true);

				if (!sMessage.IsEmpty())
					jsonResponse.WriteJsonFragment(L"messages", sMessage);
			}

		}
		response.SetData(jsonResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;


	}

	if ((LPCTSTR)path == wcsstr(path, L"doLogoff/"))
	{
		response.SetMimeType(L"text/json");
		CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);

		CLoginContext* pContext = GetLoginContext(authToken, FALSE);
		pContext->Close();
		CJSonResponse jsonResponse;
		jsonResponse.SetOK();

		response.SetData(jsonResponse.GetJson());
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"changePassword/"))
	{
		CString spassword = params.GetValueByName(_T("password"));

		//chiama core - ChangePassword
		CString res = ChangePassword(spassword);

		response.SetData(res);
		response.SetMimeType(L"text/json");
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"getLoginActiveThreads/"))
	{
		CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
		CLoginContext* pContext = GetLoginContext(authToken, FALSE);
		if (pContext)
		{
			CString sThreadinfo = AfxInvokeThreadFunction<CString, CLoginContext>(pContext->m_nThreadID, pContext, &CLoginContext::GetThreadInfosJSON);
			CJSonResponse jsonResponse;
			response.SetData(sThreadinfo);
		}
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getIDsUsedInMessageMap/"))
	{
		CJSonResponse aResponse;
		CDocumentSession* pSession = GetDocumentSession(params);

		ENSURE_SESSION();
		AfxInvokeThreadProcedure<CDocumentSession, CJSonResponse&>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::GetIDsUsedInMessageMap, aResponse);
		response.SetData(aResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getData/"))
	{
		CJSonResponse aResponse;
		CDocumentSession* pSession = GetDocumentSession(params);

		ENSURE_SESSION();
		AfxInvokeThreadProcedure<CDocumentSession, CJSonResponse&>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::GetData, aResponse);
		response.SetData(aResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"setData/"))
	{
		CJSonResponse aResponse;
		CDocumentSession* pSession = GetDocumentSession(params);

		ENSURE_SESSION();
		CString sJSONData = params.GetValueByName(_T("jsonData"));
		CJsonParser parser;
		if (!parser.ReadJsonFromString(sJSONData))
		{
			return;
		}
		AfxInvokeThreadFunction<SetDataResult, CDocumentSession, CJsonParser&, BOOL>(pSession->m_nDocumentThreadID, pSession, &CDocumentSession::SetData, parser, FALSE);
		response.SetData(aResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"getWindowData/"))
	{
		response.SetMimeType(L"application/octet-stream");//send a byte array
		//prima calcolo le dimensioni
		LRESULT size = SendMessage(HWND_TBMFC_SPECIAL, UM_REQUEST_INFO, NULL, NULL);
		if (!size)
		{
			return;
		}
		else
		{
			//poi alloco il buffer e lo faccio riempire
			BYTE* buff = new BYTE[size];
			SendMessage(HWND_TBMFC_SPECIAL, UM_REQUEST_INFO, size, (LPARAM)buff);
			response.SetData(buff, size);
		}
		
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"chageWindowProperties/"))
	{
		BYTE* buff;
		int nSize;
		if (params.GetValueByName(BYTE_DATA_PARAM_NAME, buff, nSize))
		{
			SendMessage(HWND_TBMFC_SPECIAL, UM_CHANGE_WINDOW_PROPERTIES, nSize, (LPARAM)buff);
		}
		
		return;
	}
	//confronto il puntatore di inizio stringa col puntatore alla prima occorrenza della stringa cercata
	//perché la stringa deve esserege all'inizio
	if ((LPCTSTR)path == wcsstr(path, L"runObject/"))
	{
		CJSonResponse jsonResponse;
		CString sType = params.GetValueByName(_T("type"));
		ObjectType type = (ObjectType)_ttoi(sType);
		RunObject(params, jsonResponse, type);
		response.SetData(jsonResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}
	//confronto il puntatore di inizio stringa col puntatore alla prima occorrenza della stringa cercata
	//perché la stringa deve essere all'inizio
	if ((LPCTSTR)path == wcsstr(path, L"runDocument/"))
	{
		CJSonResponse jsonResponse;
		RunObject(params, jsonResponse, DOCUMENT);
		response.SetData(jsonResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}
	//confronto il puntatore di inizio stringa col puntatore alla prima occorrenza della stringa cercata
	//perché la stringa deve essere all'inizio
	if ((LPCTSTR)path == wcsstr(path, L"runReport/"))
	{
		CJSonResponse jsonResponse;
		RunObject(params, jsonResponse, REPORT);
		response.SetData(jsonResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"closeDocument/"))
	{
		CJSonResponse jsonResponse;
		CloseDocument(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"closeDialog/"))
	{
		CJSonResponse jsonResponse;
		CloseDialog(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		response.SetMimeType(L"text/json");
		return;
	}

	//confronto il puntatore di inizio stringa col puntatore alla prima occorrenza della stringa cercata
	//perché la stringa deve essere all'inizio
	if ((LPCTSTR)path == wcsstr(path, L"image/"))
	{
		SetMimeType(path, response);
		GetImage(params, ((LPCTSTR)path) + 6 /*salto image/*/, response);
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"getWebSocketsPort/"))
	{
		response.SetMimeType(L"text/plain");
		CString s;
		s.Format(_T("%d"), GetWebSocketsConnectorPort());
		response.SetData(s);
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"login/"))
	{
		CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
		CLoginContext* pContext = GetLoginContext(authToken, TRUE);
		if (pContext)
		{
			//TODO scrivere OK in json
		}
		else
		{
			//TODO scrivere l'errore in json
		}

		response.SetMimeType(L"text/json");
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"canChangeLogin/"))
	{

		response.SetMimeType(L"text/json");
		CJSonResponse jsonResponse;
		CLoginThread* pThread = AfxGetLoginThread();
		if (pThread && pThread->GetOpenDocuments() > 0)
		{
			jsonResponse.SetError();
			jsonResponse.SetMessage(_TB("It is not possible to change login at this point.\r\nPlease, check whether there is any opened document in the application framework."));

		}
		else
		{
			jsonResponse.SetOK();
		}

		response.SetData(jsonResponse.GetJson());
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"activateTab/"))
	{
		CJSonResponse jsonResponse;

		ActivateTab(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"buttonClick/"))
	{
		CJSonResponse jsonResponse;
		ButtonClick(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"command/"))
	{
		CJSonResponse aResponse;
		CDocumentSession* pSession = GetDocumentSession(params);
		ENSURE_SESSION();
		pSession->SuspendPushToClient();
		CString sId = params.GetValueByName(_T("id"));
		DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
		if (pSession->m_DocumentPtr)
			SendMessage(pSession->m_DocumentPtr->GetFrameHandle(), WM_COMMAND, id, NULL);
		pSession->ResumePushToClient();
		aResponse.SetOK();
		response.SetData(aResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"radarDblClick/"))
	{
		CJSonResponse jsonResponse;
		RadarDblClick(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"radarSelect/"))
	{
		CJSonResponse jsonResponse;
		RadarSelect(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"radarScroll/"))
	{
		CJSonResponse jsonResponse;
		RadarScroll(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"buttonArrowClick/"))
	{
		CJSonResponse jsonResponse;
		ButtonArrowClick(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"hklUpClick/"))
	{
		CJSonResponse jsonResponse;
		HKLClick(params, 0, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"hklLowClick/"))
	{
		CJSonResponse jsonResponse;
		HKLClick(params, 1, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"menuItemClick/"))
	{
		CJSonResponse jsonResponse;
		MenuItemClick(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"menuHide/"))
	{
		CJSonResponse jsonResponse;
		HideMenu(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"fillCombo/"))
	{
		CJSonResponse jsonResponse;

		GetComboItems(params, jsonResponse);

		response.SetData(jsonResponse.GetJson());
		return;
	}
	// body edit requestes.
	if ((LPCTSTR)path == wcsstr(path, L"fillBodyEdit/"))
	{
		CJSonResponse jsonResponse;

		GetBodyItems(params, jsonResponse);

		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"gridSelectionChanged/"))
	{
		CJSonResponse jsonResponse;

		SelectBodyEditRows(params, jsonResponse);

		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"moveTo/"))
	{
		CJSonResponse jsonResponse;
		MoveTo(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"blur/"))
	{
		CJSonResponse jsonResponse;
		Blur(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"selectedItem/"))
	{
		CJSonResponse jsonResponse;
		SelectedItem(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		// TODO
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"checkListChanged/"))
	{
		CJSonResponse jsonResponse;
		// TODO
		CheckListChanged(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		// TODO
		return;
	}
	//doHyperLink
	if ((LPCTSTR)path == wcsstr(path, L"doHyperLink/"))
	{
		CJSonResponse jsonResponse;
		DoHyperLink(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		// TODO
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"addItem/"))
	{
		CJSonResponse jsonResponse;
		AddItem(params, jsonResponse);
		response.SetData(jsonResponse.GetJson());
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"setRememberMe/"))
	{
		CString  checkedVal = params.GetValueByName(_T("checked"));
		SetRememberMe(checkedVal);
		response.SetMimeType(L"None");
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"getRememberMe/"))
	{
		response.SetData(GetRememberMe());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"isAutoLoginable/"))
	{
		response.SetData(IsAutoLoginable());
		response.SetMimeType(L"text/json");
		return;
	}
	if ((LPCTSTR)path == wcsstr(path, L"getLoginInitInformation/"))
	{
		response.SetData(GetLoginInitInformation());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getLoginInitImage/"))
	{
		response.SetData(GetLoginInitImage());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getLoginBackgroundImage/"))
	{
		response.SetData(GetLoginBackgroundImage());
		response.SetMimeType(L"text/json");
		return;
	}

	//una staticimage é un'immagine presa da file system
	if ((LPCTSTR)path == wcsstr(path, L"staticimage/"))
	{
		CString file = m_strStandardInstallationPath + SLASH_CHAR + (((LPCTSTR)path) + 12) /*salto staticimage/*/;
		if (ReadFileContent(file, response))
		{
			SetMimeType(path, response);
		}
		else
		{
			response.SetData(cwsprintf(_TB("Error reading file %s"), file));
			response.SetMimeType(_T("text/plain"));
		}

		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"staticThumbnail/"))
	{
		CString ns = params.GetValueByName(_T("ns"));

		CString file = AfxGetPathFinder()->GetMenuThumbnailsFolderPath() + _T("\\") + ns + _T(".jpg");
		if (ReadFileContent(file, response))
		{
			SetMimeType(file, response);
		}
		else
		{
			response.SetData(cwsprintf(_TB("Error reading file %s"), file));
			response.SetMimeType(_T("text/plain"));
		}
		return;
	}


	if ((LPCTSTR)path == wcsstr(path, L"LoadCustomTheme/"))
	{
		CString file = AfxGetPathFinder()->GetThemeFullName((((LPCTSTR)path) + 15)) /*salto LoadCustomTheme/*/;
		if (ReadFileContent(file, response))
		{
			SetMimeType(path, response);
		}
		else
		{
			response.SetData(cwsprintf(_TB("Error reading file %s"), file));
			response.SetMimeType(_T("text/plain"));
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getLocalizedElements/"))
	{
		CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
		response.SetData(GetJsonLocalizedContent(authToken));
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"LoadDynamicTheme/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetData(GetDynamicCssTheme());
			response.SetMimeType(L"text/css");
		}
		return;
	}


	if ((LPCTSTR)path == wcsstr(path, L"getMenuElements/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetData(LoadMenuWithFavoritesAsJson(pContext));
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getThemedSettings/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetData(GetJsonMenuSettings());
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getDocumentHistory/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetData(LoadHistoryAsJson(pContext));
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"addToMostUsed/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString target = params.GetValueByName(_T("target"));
			CString objectType = params.GetValueByName(_T("objectType"));

			AddToMostUsed(target, objectType, pContext);
			response.SetMimeType(L"None");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"removeFromMostUsed/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString target = params.GetValueByName(_T("target"));
			CString objectType = params.GetValueByName(_T("objectType"));

			RemoveFromMostUsed(target, objectType, pContext);
			response.SetMimeType(L"None");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"updateMostUsedShowNr/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
			CString nrElement = params.GetValueByName(_T("nr"));
			response.SetMimeType(L"None");

			UpdateMostUsedShowNrElements(nrElement, pContext);
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getCulture/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString culture = AfxGetCultureInfo()->GetUICulture();
			if (culture.IsEmpty())
				culture = _T("en");

			response.SetData(culture);
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"updateHistoryShowNr/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString nrElement = params.GetValueByName(_T("nrElements"));
			response.SetMimeType(L"None");

			UpdateHistoryShowNrElements(nrElement, pContext);
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getMostUsedShowNr/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
			response.SetData(GetMostUsedShowNrElements(pContext));
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getHistoryShowNr/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString nrElements = GetHistoryShowNrElements(pContext);
			response.SetData(nrElements);
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"removeFromHistory/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString target = params.GetValueByName(_T("target"));
			CString objectType = params.GetValueByName(_T("objectType"));
			CString record = params.GetValueByName(_T("record"));

			response.SetMimeType(L"None");
			RemoveFromHistory(pContext, target, objectType, record);
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getUserInfo/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{

			CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
			response.SetData(GetJsonWorkerInfos(authToken));
		}
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getBrandInfo/"))
	{
		response.SetData(LoadBrandInfoAsJson());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"unFavoriteObject/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetMimeType(L"None");
			CString target = params.GetValueByName(_T("target"));
			CString objectType = params.GetValueByName(_T("objectType"));
			RemoveFromFavorites(target, objectType, pContext);
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"favoriteObject/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetMimeType(L"None");
			CString target = params.GetValueByName(_T("target"));
			CString objectType = params.GetValueByName(_T("objectType"));
			AddToFavorites(target, objectType, pContext);
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getOpenDocuments/"))
	{
		response.SetData(GetJsonOpenDocuments());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getNewMessages/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetData(GetNewMessages(pContext));
			response.SetMimeType(L"text/json");
		}
		return;
	}


	if ((LPCTSTR)path == wcsstr(path, L"updateFavoritesPosition/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString target1 = params.GetValueByName(_T("target1"));
			CString objectType1 = params.GetValueByName(_T("objectType1"));
			CString target2 = params.GetValueByName(_T("target2"));
			CString objectType2 = params.GetValueByName(_T("objectType2"));
			UpdateFavoritesPosition(target1, objectType1, target2, objectType2, pContext);
			response.SetMimeType(L"None");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"setPreference/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString preferenceName = params.GetValueByName(_T("name"));
			CString preferenceValue = params.GetValueByName(_T("value"));

			SetPreference(preferenceName, preferenceValue, pContext);

			response.SetMimeType(L"None");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getPreferences/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetData(GetPreferencesAsJson(pContext));
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"clearAllHistory/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			ClearHistory(pContext);
			response.SetMimeType(L"None");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"clearAllMostUsed/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			ClearMostUsed(pContext);
			response.SetMimeType(L"None");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"setLeftGroupVisibility/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString groupName = params.GetValueByName(_T("name"));
			CString visible = params.GetValueByName(_T("visible"));

			SetLeftGroupVisibility(groupName, visible, pContext);
			response.SetMimeType(L"None");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"changeApplicationDate/"))
	{
		response.SetMimeType(L"text/json");
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
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getApplicationDateFormat/"))
	{
		DataDate pDate = AfxGetApplicationDate();
		CDateFormatter* pFormatter = (CDateFormatter*)AfxGetFormatStyleTable()->GetFormatter(pDate.GetDataType(), NULL);
		if (pFormatter)
		{

			CDateFormatHelper::FormatTag  format = pFormatter->GetFormat();
			CString formatString = _T("");
			switch (format)
			{
			case	CDateFormatHelper::FormatTag::DATE_DMY:
				formatString = _T("dd/mm/yyyy");
				break;
			case	CDateFormatHelper::FormatTag::DATE_MDY:
				formatString = _T("mm/dd/yyyy");
				break;
			case	CDateFormatHelper::FormatTag::DATE_YMD:
				formatString = _T("yyyy/mm/dd");
				break;
			}

			response.SetData(formatString);
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getApplicationDate/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetData(AfxGetApplicationDate().FormatData());
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"addToHiddenTiles/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString appName = params.GetValueByName(_T("application"));
			CString groupName = params.GetValueByName(_T("group"));
			CString menuName = params.GetValueByName(_T("menu"));
			CString tileName = params.GetValueByName(_T("tile"));

			AddToHiddenTiles(appName, groupName, menuName, tileName, pContext);
			response.SetMimeType(L"None");
		}
		return;
	}


	if ((LPCTSTR)path == wcsstr(path, L"removeFromHiddenTiles/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			CString appName = params.GetValueByName(_T("application"));
			CString groupName = params.GetValueByName(_T("group"));
			CString menuName = params.GetValueByName(_T("menu"));
			CString tileName = params.GetValueByName(_T("tile"));

			RemoveFromHiddenTiles(appName, groupName, menuName, tileName, pContext);
			response.SetMimeType(L"None");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getThemes/"))
	{
		response.SetData(GetJsonThemesList());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"clearCachedData/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			ClearCachedData(pContext);
			response.SetMimeType(L"None");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"changeThemes/"))
	{
		response.SetMimeType(L"text/json");
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
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getProductInfo/"))
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetData(GetJsonProductInfo(pContext));
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getCustomizationsForDocument/"))
	{
		CString docNs = params.GetValueByName(_T("ns"));
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext)
		{
			response.SetData(GetEasyBuilderAppAssembliesPathsAsJson(docNs, pContext));
			response.SetMimeType(L"text/json");
		}
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getCustomizationContextAppAndModule/"))
	{
		response.SetData(GetAndFormatCustomizationContextApplicationAndModule());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"closeCustomizationContext/"))
	{
		CJSonResponse jsonResponse;
		CLoginThread* pThread = AfxGetLoginThread();
		if (pThread && pThread->GetOpenDocuments() > 0)
		{
			jsonResponse.SetError();
			jsonResponse.SetMessage(_TB("It is not possible to close customization context with opened documents.\r\nPlease, close all documents."));
		}
		else
		{
			CloseCustomizationContext();
			jsonResponse.SetOK();
		}
		response.SetData(jsonResponse.GetJson());
		response.SetMimeType(L"text/json");

		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"getConnectionInfo/"))
	{
		response.SetData(GetConnectionInformation());
		response.SetMimeType(L"text/json");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"activateViaSMS/"))
	{
		PingViaSMS();
		response.SetMimeType(L"None");
		return;
	}

	if ((LPCTSTR)path == wcsstr(path, L"producerSite/"))
	{
		OpenProducerSite();
		response.SetMimeType(L"None");
		return;
	}

	CString file = m_strRoot + path;
	if (ReadFileContent(file, response))
	{
		SetMimeType(path, response);
		return;
	}

	response.SetData(cwsprintf(_TB("Invalid request: %s"), path));
	response.SetMimeType(_T("text/plain"));
	response.SetStatus(404);
}

//--------------------------------------------------------------------------------
bool CTbRequestHandler::ExecutedOnLoginInfinity(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{

	if ((LPCTSTR)path != wcsstr(path, szNeedLoginInfinityUrl))
		return false; //non ho necessità di girare nel login thread

	CString cryptedtoken = params.GetValueByName(_T("tk"));
	CString user = params.GetValueByName(_T("user"));
	CString company = params.GetValueByName(_T("company"));
	CString pwd = params.GetValueByName(_T("password"));
	bool bOverWrite1 = params.GetValueByName(_T("overwrite")) == _T("true");
	bool winNT1 = params.GetValueByName(_T("winNT")) == _T("true");
	bool relogin1 = params.GetValueByName(_T("relogin")) == _T("true");
	bool changeAutologinInfo1 = params.GetValueByName(_T("changeAutologinInfo")) == _T("true");
	CString saveAutologinInfo1 = params.GetValueByName(_T("saveAutologinInfo"));
	CString sMessage1;
	bool alreadyLogged1 = false, changePassword1 = false;
	CString sTokenMicroarea = params.GetValueByName(AUTH_TOKEN_PARAM);
	CJSonResponse jsonResponse1;
	int res = 0;
	if (sTokenMicroarea.IsEmpty())
	{
		sTokenMicroarea = DoSSOLoginWeb(cryptedtoken, user, pwd, company, winNT1, bOverWrite1, relogin1, sMessage1, alreadyLogged1, changePassword1, changeAutologinInfo1, res, saveAutologinInfo1);
		response.SetCookie(AUTH_TOKEN_PARAM, sTokenMicroarea);
	}

	if (!sTokenMicroarea.IsEmpty())
	{
		CLoginContext* pContext = GetLoginContext(sTokenMicroarea, TRUE);//forza la creazione del login context

																		 /*CString file = m_strRoot + L"newMenu.html";
																		 if (ReadFileContent(file, response))
																		 {
																		 SetMimeType(L"newMenu.html", response);
																		 return;
																		 }*/

		CNameValueCollection * pParams = (CNameValueCollection*)&params;
		pParams->Add(AUTH_TOKEN_PARAM, sTokenMicroarea);

		CString newPath = (LPCTSTR)path + nNeedLoginThreadUrlLen + 2; //TODO LARA
		try
		{
			AfxInvokeThreadProcedure
				<
				CTbRequestHandler,
				const CString&,
				const CNameValueCollection&,
				CTBResponse&
				>
				(
					pContext->m_nThreadID,
					this,
					&CTbRequestHandler::ProcessRequest,
					newPath,
					params,
					response
					);
		}
		catch (CApplicationErrorException*)
		{
			response.SetData(_TB("Error processing request on login thread, thread communication error."));
			response.SetMimeType(_T("text/plain"));
		}

		return false;
	}

	else
	{
		if (!cryptedtoken.IsEmpty()) {
			CString file = m_strRoot + L"loginhost.html";
			if (ReadFileContent(file, response))
			{
				SetMimeType(L"loginhost.html", response);
				return true;
			}
		}
		else {

			jsonResponse1.SetError();
			if (alreadyLogged1)
				jsonResponse1.WriteBool(L"alreadyLogged", true);
			if (changePassword1)
				jsonResponse1.WriteBool(L"changePassword", true);
			if (changeAutologinInfo1)
				jsonResponse1.WriteBool(L"changeAutologinInfo", true);

			if (!sMessage1.IsEmpty())
				jsonResponse1.WriteJsonFragment(L"messages", sMessage1);
		}

	}
	response.SetData(jsonResponse1.GetJson());
	response.SetMimeType(L"text/json");
	return true;
}

//--------------------------------------------------------------------------------
CString CTbRequestHandler::GetAndFormatCustomizationContextApplicationAndModule()
{
	CString appName, modName;
	GetEasyBuilderAppAndModule(appName, modName);
	if (appName.IsEmpty() || modName.IsEmpty())
		return NULL;

	CString res(appName + _T(";") + modName);
	return res;
}

//--------------------------------------------------------------------------------
CString CTbRequestHandler::GetDynamicCssTheme()
{
	 CString strCssFullPath = AfxGetThemeManager()->GetThemeCssFullPath();
	 if (!ExistFile(strCssFullPath))
		 return _T("");
	
	 CLineFile file;
	 if (!file.Open(strCssFullPath, CFile::modeRead | CFile::typeText))
		 return _T("");


	 CString cssResult = file.ReadToEnd();
	 file.Close();

	 return cssResult;
}

//--------------------------------------------------------------------------------
CString CTbRequestHandler::GetDocumentType(CBaseDocument* pDoc)
{
	if (pDoc->IsABatchDocument())
		return _T("Batch");

	return pDoc->GetNamespace().GetTypeString();
}

//--------------------------------------------------------------------------------
void CTbRequestHandler::FindHwndNotInOpenDocuments(CArray<HWND>& arNotInOpenDocuments)
{
	CBaseDocument* pDoc = NULL;
	LongArray arDocs;
	AfxGetWebServiceStateObjectsHandles(arDocs, RUNTIME_CLASS(CBaseDocument));

	CArray<HWND>& ar = AfxGetLoginContext()->GetAllHwndArray();

	for (int i = 0; i <= ar.GetUpperBound(); i++)
	{
		bool found = false;
		HWND hWnd = ar.GetAt(i);
		for (int j = 0; j <= arDocs.GetUpperBound(); j++)
		{
			pDoc = (CBaseDocument*)arDocs[j];
			if (!pDoc->CanShowInOpenDocuments())
				continue;

			if (pDoc->GetFrameHandle() == hWnd)
			{
				found = true;
				continue;
			}
		}

		if (!found)
			arNotInOpenDocuments.Add(hWnd);
	}
}



//--------------------------------------------------------------------------------
CString CTbRequestHandler::GetJsonProductInfo(CLoginContext* pContext)
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

//--------------------------------------------------------------------------------
CString CTbRequestHandler::GetJsonThemesList()
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
CString CTbRequestHandler::GetJsonOpenDocuments()
{
	LongArray arDocs;
	AfxGetWebServiceStateObjectsHandles(arDocs, RUNTIME_CLASS(CBaseDocument));

	CBaseDocument* pDoc = NULL;

	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("OpenDocuments"));
	jsonSerializer.OpenArray(_T("Document"));

	int arrayIndex = 0;
	for (int i = 0; i <= arDocs.GetUpperBound(); i++)
	{
		pDoc = (CBaseDocument*)arDocs[i];
		if (!pDoc->CanShowInOpenDocuments())
			continue;

		jsonSerializer.OpenObject(arrayIndex);

		jsonSerializer.WriteString(_T("title"), pDoc->GetTitle());
		jsonSerializer.WriteString(_T("target"), pDoc->GetNamespace().ToUnparsedString());
		jsonSerializer.WriteString(_T("handle"), cwsprintf(_T("{0-%d}"), pDoc->GetFrameHandle()));
		jsonSerializer.WriteString(_T("objectType"), GetDocumentType(pDoc));
		jsonSerializer.WriteString(_T("defaultDescription"), pDoc->GetDefaultMenuDescription());

		jsonSerializer.WriteString(_T("creationTime"), cwsprintf(_T("{0-%d}"), pDoc->GetCreationTime()));

		if (pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		{
			CAbstractFormDoc* tempDoc = (CAbstractFormDoc*)pDoc;
			if (tempDoc && tempDoc->GetMaster() && tempDoc->GetMaster()->GetRecord())
				jsonSerializer.WriteString(_T("record"), tempDoc->GetMaster()->GetRecord()->GetPrimaryKeyNameValue());
		}

		jsonSerializer.CloseObject();
		arrayIndex++;
	}

	CArray<HWND> arNotInOpenDocuments;
	FindHwndNotInOpenDocuments(arNotInOpenDocuments);
	for (int i = 0; i <= arNotInOpenDocuments.GetUpperBound(); i++)
	{
		HWND tempHwnd = arNotInOpenDocuments.GetAt(i);
		BOOL isValid = ::IsWindow(tempHwnd);
		if (!isValid)
			continue;

		CWnd* pWnd = CWnd::FromHandle(tempHwnd);
		if (!pWnd)
			continue;

		int bShow = pWnd->SendMessage(UM_SHOW_IN_OPEN_DOCUMENTS, NULL, NULL);
		if (!bShow)
			continue;

		CString str;
		pWnd->GetWindowText(str);

		jsonSerializer.OpenObject(arrayIndex);  //qua non è i, è l'ultimo slot disponibile
		jsonSerializer.WriteString(_T("title"), str);
		jsonSerializer.WriteString(_T("handle"), cwsprintf(_T("{0-%d}"), tempHwnd));
		jsonSerializer.WriteString(_T("target"), _T(""));
		jsonSerializer.WriteString(_T("objectType"), _T("Document"));
		jsonSerializer.WriteString(_T("defaultDescription"), _T(""));

		jsonSerializer.CloseObject();
		arrayIndex++;
	}

	jsonSerializer.CloseArray();
	jsonSerializer.CloseObject();
	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
CString CTbRequestHandler::GetJsonCompaniesForUser(CString strUser)
{
	CStringArray companies;
	AfxGetLoginManager()->EnumCompanies(strUser, companies);

	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("Companies"));
	jsonSerializer.OpenArray(_T("Company"));

	for (int i = 0; i <= companies.GetUpperBound(); i++)
	{
		jsonSerializer.OpenObject(i);

		jsonSerializer.WriteString(_T("name"), companies[i]);

		jsonSerializer.CloseObject();
	}
	jsonSerializer.CloseArray();
	jsonSerializer.CloseObject();

	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
CString CTbRequestHandler::GetJsonWorkerInfos(CString authToken)
{
	CLoginContext* pContext = GetLoginContext(authToken, FALSE);
	if (!pContext)
		return _T("");

	CString workerName, userName, workerLastName, company;
	CWorkersTableObj* workersTable = (CWorkersTableObj*)pContext->GetWorkersTable();
	CString imagePath;
	if (workersTable)
	{
		CWorker* worker = workersTable->GetWorker(pContext->GetWorkerId());
		if (worker)
		{
			workerName = worker->GetName();
			workerLastName = worker->GetLastName();
			imagePath = (worker && !worker->GetImagePath().IsEmpty()) ? worker->GetImagePath() : _T("");
		}
	}

	company = AfxGetLoginInfos()->m_strCompanyName;
	userName = AfxGetLoginInfos()->m_strUserName;

	CString encodedImage = _T("");
	CString file = _T("");

	file = AfxGetPathFinder()->GetFileNameFromNamespace(imagePath, AfxGetLoginInfos()->m_strUserName);
	file.Replace(m_strStandardInstallationPath, _T(""));

	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("UserInfos"));

	if (!workerName.IsEmpty())
		jsonSerializer.WriteString(_T("workerName"), workerName);

	if (!userName.IsEmpty())
		jsonSerializer.WriteString(_T("userName"), userName);
	if (!workerLastName.IsEmpty())
		jsonSerializer.WriteString(_T("workerLastName"), workerLastName);
	if (!file.IsEmpty())
		jsonSerializer.WriteString(_T("image_file"), file);
	if (!company.IsEmpty())
		jsonSerializer.WriteString(_T("company"), company);

	jsonSerializer.CloseObject();

	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
CString CTbRequestHandler::GetJsonLocalizedContent(const CString& authToken)
{
	//CLoginContext* pContext = AfxGetLoginContext(authToken);
	//if (!pContext)
	//	return _T("");

	//TB://Document.ERP.CustomersSuppliers.Documents.Customers?custsupptype:00310000%3bCustSupp:1%3b
	CJsonSerializer jsonSerializer;

	jsonSerializer.OpenObject(_T("LocalizedElements"));
	jsonSerializer.OpenArray(_T("LocalizedElement"));

	int elements = 0;
	ADD_LOCALIZED_CONTENT(_T("PwdChanged"), _TB("Password changed"));
	ADD_LOCALIZED_CONTENT(_T("ErrUserAlreadyLogged"), _TB("The user is already connected to the system. Do you want to close previous working session and open new one?"));
	ADD_LOCALIZED_CONTENT(_T("changeAutologinInfo"), _TB("Exists information of another user, do you want to replace them with your own? "));
	ADD_LOCALIZED_CONTENT(_T("passwordError"), _TB("Please make sure you entered the password correctly."));
	ADD_LOCALIZED_CONTENT(_T("FullMenuTitle"), _TB("Full Menu"));
	ADD_LOCALIZED_CONTENT(_T("EnvironmentMenuTitle"), _TB("Environment Menu"));
	ADD_LOCALIZED_CONTENT(_T("WorkerLabel"), _TB("Worker"));
	ADD_LOCALIZED_CONTENT(_T("CompanyLabel"), _TB("Company"));
	ADD_LOCALIZED_CONTENT(_T("OptionsLabel"), _TB("Options"));
	ADD_LOCALIZED_CONTENT(_T("ShowAllLabel"), _TB("Show all"));
	ADD_LOCALIZED_CONTENT(_T("ChangeLogin"), _TB("Change Login"));
	ADD_LOCALIZED_CONTENT(_T("GenericOptionsLabel"), _TB("Generic options"));
	ADD_LOCALIZED_CONTENT(_T("ShowHistoryLabel"), _TB("Show history"));
	ADD_LOCALIZED_CONTENT(_T("ShowMostUsedLabel"), _TB("Show most used"));
	ADD_LOCALIZED_CONTENT(_T("ShowThumbnailsLabel"), _TB("Show thumbnails"));
	ADD_LOCALIZED_CONTENT(_T("CloseLabel"), _TB("Close"));
	ADD_LOCALIZED_CONTENT(_T("ReleaseInfoLabel"), _TB("Release Info"));
	ADD_LOCALIZED_CONTENT(_T("FavoritesLabel"), _TB("Favorites"));
	ADD_LOCALIZED_CONTENT(_T("HistoryLabel"), _TB("History"));
	ADD_LOCALIZED_CONTENT(_T("MaximumNumberOfElements"), _TB("Maximum number of elements"));
	ADD_LOCALIZED_CONTENT(_T("MaximumNumberOfRecords"), _TB("Maximum number of records"));
	ADD_LOCALIZED_CONTENT(_T("MostUsedLabel"), _TB("Recently used"));
	ADD_LOCALIZED_CONTENT(_T("AdvancedSearchLabel"), _TB("Advanced Search"));
	ADD_LOCALIZED_CONTENT(_T("SearchInDocuments"), _TB("Search in documents"));
	ADD_LOCALIZED_CONTENT(_T("SearchInReports"), _TB("Search in reports"));
	ADD_LOCALIZED_CONTENT(_T("SearchInBatches"), _TB("Search in batches"));
	ADD_LOCALIZED_CONTENT(_T("LoginLabel"), _TB("Login"));
	ADD_LOCALIZED_CONTENT(_T("ProductLicencedTo"), _TB("Product licenced to"));
	ADD_LOCALIZED_CONTENT(_T("ProductVersion"), _TB("Product version"));
	ADD_LOCALIZED_CONTENT(_T("ShowFullMenu"), _TB("Show Full Menu"));
	ADD_LOCALIZED_CONTENT(_T("ChangeApplicationDate"), _TB("Change operation date"));
	ADD_LOCALIZED_CONTENT(_T("CurrentApplicationDate"), _TB("Operation date"));
	ADD_LOCALIZED_CONTENT(_T("Open"), _TB("Open"));
	ADD_LOCALIZED_CONTENT(_T("UnpinElement"), _TB("Unpin element"));
	ADD_LOCALIZED_CONTENT(_T("PinElement"), _TB("Pin element"));
	ADD_LOCALIZED_CONTENT(_T("RemoveFromFavorites"), _TB("Remove from favorites"));
	ADD_LOCALIZED_CONTENT(_T("AddToFavorites"), _TB("Add to favorites"));
	ADD_LOCALIZED_CONTENT(_T("SetAsStartupMenu"), _TB("Set as startup menu"));
	ADD_LOCALIZED_CONTENT(_T("StartupElement"), _TB("Startup element"));
	ADD_LOCALIZED_CONTENT(_T("MenuOptions"), _TB("Menu options"));
	ADD_LOCALIZED_CONTENT(_T("Gadgets"), _TB("Gadgets"));
	ADD_LOCALIZED_CONTENT(_T("ShowSearchBox"), _TB("Show search box"));
	ADD_LOCALIZED_CONTENT(_T("ShowFilterBox"), _TB("Show filter box"));
	ADD_LOCALIZED_CONTENT(_T("NewFavoriteElement"), _TB("New favorite element"));
	ADD_LOCALIZED_CONTENT(_T("SearchOptions"), _TB("Search options"));
	ADD_LOCALIZED_CONTENT(_T("DeleteAllElements"), _TB("Clear list"));
	ADD_LOCALIZED_CONTENT(_T("DeleteSelectedElements"), _TB("Delete selected elements"));
	ADD_LOCALIZED_CONTENT(_T("ExpandAll"), _TB("Expand all"));
	ADD_LOCALIZED_CONTENT(_T("CollapseAll"), _TB("Collapse all"));
	ADD_LOCALIZED_CONTENT(_T("SearchLabel"), _TB("Search"));
	ADD_LOCALIZED_CONTENT(_T("FilterLabel"), _TB("Filter"));
	ADD_LOCALIZED_CONTENT(_T("ProductInfo"), _TB("Product info"));
	ADD_LOCALIZED_CONTENT(_T("ShowNotifications"), _TB("Show Notifications"));
	ADD_LOCALIZED_CONTENT(_T("ShowHideLeftGroups"), _TB("Show/hide groups"));
	ADD_LOCALIZED_CONTENT(_T("NrMaxItemsSearch"), _TB("Max number of items in search"));
	ADD_LOCALIZED_CONTENT(_T("StartsWith"), _TB("Starts with"));
	ADD_LOCALIZED_CONTENT(_T("ShowAsTile"), _TB("Show tiles"));
	ADD_LOCALIZED_CONTENT(_T("ShowAsTree"), _TB("Show tree"));
	ADD_LOCALIZED_CONTENT(_T("ChangePassword"), _TB("Change Password"));
	ADD_LOCALIZED_CONTENT(_T("PwdMatchError"), _TB("Password doesn't match!"));
	ADD_LOCALIZED_CONTENT(_T("Password"), _TB("Password"));
	ADD_LOCALIZED_CONTENT(_T("Username"), _TB("Username"));
	ADD_LOCALIZED_CONTENT(_T("RememberMe"), _TB("Remember Me"));
	ADD_LOCALIZED_CONTENT(_T("ActivateViaInternet"), _TB("Activate via Internet"));
	ADD_LOCALIZED_CONTENT(_T("ActivateViaSMS"), _TB("Activate via SMS"));
	ADD_LOCALIZED_CONTENT(_T("Activation"), _TB("Activation"));
	ADD_LOCALIZED_CONTENT(_T("Login"), _TB("Login"));
	ADD_LOCALIZED_CONTENT(_T("NewLogin"), _TB("Open New Login"));
	ADD_LOCALIZED_CONTENT(_T("NewPassword"), _TB("New Password"));
	ADD_LOCALIZED_CONTENT(_T("ConfirmNewPassword"), _TB("Confirm New Password"));
	ADD_LOCALIZED_CONTENT(_T("ClearCachedData"), _TB("Clear cached data"));
	ADD_LOCALIZED_CONTENT(_T("WindowsAuthentication"), _TB("Windows Authentication"));
	ADD_LOCALIZED_CONTENT(_T("OpenDocuments"), _TB("Open documents"));
	ADD_LOCALIZED_CONTENT(_T("ChangeTheme"), _TB("Change theme"));
	ADD_LOCALIZED_CONTENT(_T("HideTile"), _TB("Hide element"));
	ADD_LOCALIZED_CONTENT(_T("HiddenTiles"), _TB("Hidden elements"));
	ADD_LOCALIZED_CONTENT(_T("RestoreTile"), _TB("Restore element"));
	ADD_LOCALIZED_CONTENT(_T("ApplicationLabel"), _TB("Application"));
	ADD_LOCALIZED_CONTENT(_T("MenuLabel"), _TB("Menu"));
	ADD_LOCALIZED_CONTENT(_T("ModuleLabel"), _TB("Module"));
	ADD_LOCALIZED_CONTENT(_T("viewOldMsg"), _TB("View old messages"));
	ADD_LOCALIZED_CONTENT(_T("ViewOptions"), _TB("View options"));
	ADD_LOCALIZED_CONTENT(_T("NoCompany"), _TB("This user is not associated to any company."));
	ADD_LOCALIZED_CONTENT(_T("OtherElements"), _TB("Other elements"));
	ADD_LOCALIZED_CONTENT(_T("InstallationVersion"), _TB("Installation Version"));
	ADD_LOCALIZED_CONTENT(_T("Provider"), _TB("Provider"));
	ADD_LOCALIZED_CONTENT(_T("Edition"), _TB("Edition"));
	ADD_LOCALIZED_CONTENT(_T("InstallationName"), _TB("Installation Name"));
	ADD_LOCALIZED_CONTENT(_T("ActivationState"), _TB("Activation State"));
	ADD_LOCALIZED_CONTENT(_T("OK"), _TB("OK"));
	ADD_LOCALIZED_CONTENT(_T("Cancel"), _TB("Cancel"));
	ADD_LOCALIZED_CONTENT(_T("ChangeOpDate"), _TB("Change Operation Date"));
	ADD_LOCALIZED_CONTENT(_T("GotoProducerSite"), _TB("Producer Site"));
	ADD_LOCALIZED_CONTENT(_T("ViewProductInfo"), _TB("Product Info"));

	ADD_LOCALIZED_CONTENT(_T("ConnectionInfo"), _TB("Connection Info"));
	ADD_LOCALIZED_CONTENT(_T("FreeSpace"), _TB("Free space"));
	ADD_LOCALIZED_CONTENT(_T("UsedSpace"), _TB("Used space"));
	ADD_LOCALIZED_CONTENT(_T("DBServer"), _TB("Server"));
	ADD_LOCALIZED_CONTENT(_T("DBUser"), _TB("User"));
	ADD_LOCALIZED_CONTENT(_T("DBName"), _TB("Database"));
	ADD_LOCALIZED_CONTENT(_T("AuditingEnabled"), _TB("Auditing enabled"));
	ADD_LOCALIZED_CONTENT(_T("SecurityEnabled"), _TB("Security enabled"));
	ADD_LOCALIZED_CONTENT(_T("WebApplicationServer"), _TB("Web Application Server"));
	ADD_LOCALIZED_CONTENT(_T("FileApplicationServer"), _TB("File Application Server"));
	ADD_LOCALIZED_CONTENT(_T("Instance"), _TB("Instance"));
	ADD_LOCALIZED_CONTENT(_T("EasyStudioDeveloper"), _TB("Easy Studio Developer"));
	ADD_LOCALIZED_CONTENT(_T("Administrator"), _TB("Administrator"));
	ADD_LOCALIZED_CONTENT(_T("NA"), _TB("Not available"));
	ADD_LOCALIZED_CONTENT(_T("Yes"), _TB("Yes"));
	ADD_LOCALIZED_CONTENT(_T("No"), _TB("No"));
	ADD_LOCALIZED_CONTENT(_T("Error"), _TB("Error"));
	ADD_LOCALIZED_CONTENT(_T("Messages"), _TB("Messages"));
	ADD_LOCALIZED_CONTENT(_T("LoginExpired"), _TB("Working session expired, please login again."));

	ADD_LOCALIZED_CONTENT(_T("EasyStudioOptions"), _TB("EasyStudio options"));
	ADD_LOCALIZED_CONTENT(_T("OpenEasyStudioNewCustomization"), _TB("New EasyStudio customization"));
	ADD_LOCALIZED_CONTENT(_T("OpenEasyStudioWithCustomization"), _TB("Edit EasyStudio Customization"));
	ADD_LOCALIZED_CONTENT(_T("CurrentApplication"), _TB("Current application"));
	ADD_LOCALIZED_CONTENT(_T("CurrentModule"), _TB("Current module"));
	ADD_LOCALIZED_CONTENT(_T("ChangeCustomizationContext"), _TB("Change customization context"));
	ADD_LOCALIZED_CONTENT(_T("CloseCustomizationContext"), _TB("Close customization context"));
	ADD_LOCALIZED_CONTENT(_T("NotDesignableDocument"), _TB("This document/batch is not designable"));
	ADD_LOCALIZED_CONTENT(_T("CustomizationNotActive"), _TB("Customization valid only for context"));	
	
	jsonSerializer.CloseArray();
	jsonSerializer.CloseObject();

	return jsonSerializer.GetJson();
}

//--------------------------------------------------------------------------------
bool CTbRequestHandler::ExecutedOnLoginThread(const CString& path, const CNameValueCollection& params, CTBResponse& response)
{
	if ((LPCTSTR)path != wcsstr(path, szNeedLoginThreadUrl))
		return false; //non ho necessità di girare nel login thread

	//ho bisogno del login thread: devo avere il modo di recuperarlo, altrimenti segnalo errore
	//in ogni caso ritorno true, come se avessi fatto la chiamata, perché non posso comunque proseguire 
	CString authToken = params.GetValueByName(AUTH_TOKEN_PARAM);
	if (authToken.IsEmpty())
	{
		response.SetData(_TB("Cannot process request on login thread, authentication token is empty."));
		response.SetMimeType(_T("text/plain"));
		response.SetStatus(401);
		return true;
	}

	CLoginContext* pContext = AfxGetLoginContext(authToken);
	if (!pContext)
	{
		response.SetData(_TB("Cannot process request on login thread, authentication token not valid."));
		response.SetMimeType(_T("text/plain"));
		response.SetStatus(401);
		return true;
	}
	CString newPath = (LPCTSTR)path + nNeedLoginThreadUrlLen;
	try
	{
		AfxInvokeThreadProcedure
			<
			CTbRequestHandler,
			const CString&,
			const CNameValueCollection&,
			CTBResponse&
			>
			(
				pContext->m_nThreadID,
				this,
				&CTbRequestHandler::ProcessRequest,
				newPath,
				params,
				response
				);
	}
	catch (CApplicationErrorException*)
	{response.SetData(_TB("Error processing request on login thread, thread communication error."));
		response.SetMimeType(_T("text/plain"));
	}
	return true;
}

