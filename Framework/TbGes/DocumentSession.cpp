#include "StdAfx.h"

#include <TbNameSolver\CallbackHandler.h>

#include <TbGeneric\WndObjDescription.h>
#include <TbGeneric\TBWebFriendlyMenu.h>
#include <TbGeneric\WndObjDescription.h>
#include <tbgeneric\tbcmdui.h>

#include <TbGenlib\TBCommandInterface.h>
#include <TbGenlib\NumbererInfo.h>

#include <TbGenlib\CEFClasses.h>
#include <TbGenlib\TABCORE.H>
#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\PARSCBX.H>

#include <TBGenlib\hyperlink.h>


#include <TbGes\BODYEDIT.H>
#include <TbGes\SoapFunctions.h>

#include <TbWoormViewer\WOORMDOC.H>

#include <TBApplication\LoginThread.h>
#include <TbRadar\RADARVW.H>

#include "DocumentSession.h"


TCHAR* szCmpId = _T("cmpId");

//--------------------------------------------------------------------------------
HWND ReadComponentId(CJsonParser& json)
{
	CString sId;
	if (json.TryReadString(szCmpId, sId))
	{
		return (HWND)_ttoi(sId);
	}
	int id;
	if (json.TryReadInt(szCmpId, id))
	{
		return (HWND)id;
	}
	return 0;
}
//--------------------------------------------------------------------------------
CBaseDocument* GetDocumentFromHwnd(HWND hWnd)
{
	if (!IsWindow(hWnd))
		return NULL;
	CWnd* pWnd = CWnd::FromHandle(hWnd);
	if (!pWnd)
		return NULL;
	if (pWnd->IsKindOf(RUNTIME_CLASS(CDockableFrame)))
		return (CBaseDocument*)((CDockableFrame*)pWnd)->GetDocument();
	if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedDialog)))
		return ((CParsedDialog*)pWnd)->GetDocument();
	return NULL;
}

//--------------------------------------------------------------------------------
CBaseDocument* GetDocumentFromJson(CJsonParser& jsonParser)
{
	HWND hwnd = ReadComponentId(jsonParser);
	return GetDocumentFromHwnd(hwnd);
}
//-----------------------------------------------------------------------------
//legge i dati dalla struttura json e li assegna al record (per ogni campo in json, effettua la ricerca nel record)
bool AssignJSONToSqlRecord(SqlRecord* pRecord, CJsonParser& parser, bool checkTBModified, bool& conflict)
{
	CJsonIterator* pIterator = parser.BeginIteration();
	CString sColumn, sValue;
	conflict = false;
	bool modified = false;
	while (pIterator->GetNext(sColumn, sValue))
	{
		DataObj* pDataObj = pRecord->GetDataObjFromColumnName(sColumn);
		if (!pDataObj)
			continue;
		//compare TBModified column; if they are different, there is a conflict (control made only for master)
		if (checkTBModified && sColumn.Compare(MODIFIED_COL_NAME) == 0 && sValue.Compare(pDataObj->FormatDataForXML()) != 0)
		{
			conflict = true;
			return false;
		}
		pDataObj->AssignFromXMLString(sValue);
		if (pDataObj->IsModified())
			modified = true;
	}
	return true;
}

//-----------------------------------------------------------------------------
//popola i dati del sqlrecord cercando i volori nella struttura JSON
bool PopulateSqlRecordFromJSON(SqlRecord* pRecord, CJsonParser& parser, bool checkTBModified, bool onlyKeyFields)
{
	for (int i = 0; i < pRecord->GetCount(); i++)
	{
		SqlRecordItem* pItem = pRecord->GetAt(i);
		if (onlyKeyFields && !pItem->IsSpecial())
			continue;
		CString sColumn = pItem->GetColumnName();

		if (parser.Has(sColumn))
		{
			CString sValue = parser.ReadString(sColumn);

			//compare TBModified column; if they are different, there is a conflict (control made only for master)
			if (checkTBModified && sColumn.Compare(MODIFIED_COL_NAME) == 0 && sValue.Compare(pItem->GetDataObj()->FormatDataForXML()) != 0)
				return false;
			pItem->GetDataObj()->AssignFromXMLString(sValue);
		}
	}
	return true;
}
//-----------------------------------------------------------------------------
HWND GetHWND(const CString& strHandle)
{
	if (strHandle.IsEmpty())
		return 0;
	return (HWND)_ttol(strHandle);
}

//-----------------------------------------------------------------------------
BOOL IsButton(CWnd* pWnd)
{
	if (!pWnd)
		return FALSE;

	TCHAR szClassName[MAX_CLASS_NAME + 1];
	GetClassName(pWnd->m_hWnd, szClassName, MAX_CLASS_NAME);

	if (_tcsicmp(szClassName, _T("Button")) == 0)
		return TRUE;

	return FALSE;
}
//-----------------------------------------------------------------------------
void AssignOldValue(HWND hwnd, LPCTSTR lpszValue)
{

	if (!::IsWindow(hwnd))
		return;
	CWnd* pWnd = CWnd::FromHandle(hwnd);
	if (pWnd && !IsButton(pWnd))
	{
		CString sText;
		pWnd->GetWindowText(sText);
		if (sText != lpszValue)
		{
			if (pWnd->IsKindOf(RUNTIME_CLASS(CParsedCombo)))  //if it's a Combo, message has to be sent to combo's edit child 
			{
				CParsedCombo* pCombo = (CParsedCombo*)pWnd;
				pCombo->SetValue(lpszValue);
				pCombo->SetModifyFlag(TRUE);
			}
			else
			{
				pWnd->SetWindowText(lpszValue);
				pWnd->SendMessage(EM_SETMODIFY, TRUE, 0);
			}
		}
	}
	// The code above does not work with RadioButtons, which actually are buttons.
	else if (pWnd && IsButton(pWnd))
	{
		CButton* pButton = (CButton*)pWnd;
		UINT style = pButton->GetButtonStyle();
		UINT typeStyle = BS_TYPEMASK & style;
		if (typeStyle == BS_RADIOBUTTON || typeStyle == BS_AUTORADIOBUTTON ||
			typeStyle == BS_CHECKBOX || typeStyle == BS_AUTOCHECKBOX ||
			(typeStyle & BS_AUTOCHECKBOX) == BS_AUTOCHECKBOX)
		{
			if (wcscmp(lpszValue, L"1") == 0 || wcscmp(lpszValue, L"true") == 0)
			{
				pButton->SetCheck(true);
			}
			else if (wcscmp(lpszValue, L"0") == 0 || wcscmp(lpszValue, L"false") == 0)
			{
				pButton->SetCheck(false);
			}
		}
		// TODO: understand the button is a RadioButton 
		// or something else.
	}
}


//-----------------------------------------------------------------------------
CBaseDocument* GetActiveDocument(HWND hwnd)
{
	if (!::IsWindow(hwnd))
		return NULL;

	CWnd* pWnd = CWnd::FromHandle(hwnd);
	if (!pWnd)
		return NULL;

	CFrameWnd* pParentFrame = pWnd->IsKindOf(RUNTIME_CLASS(CFrameWnd)) ? (CFrameWnd*)pWnd : pWnd->GetParentFrame();
	if (!pParentFrame)
		return NULL;

	CDocument* pActiveDoc = pParentFrame->GetActiveDocument();
	if (pActiveDoc && pActiveDoc->IsKindOf(RUNTIME_CLASS(CBaseDocument)))
		return (CBaseDocument*)pActiveDoc;

	return NULL;
}
//-----------------------------------------------------------------------------
WebCommandType GetWebCommandType(HWND hWnd, int CommandId)
{
	CBaseDocument* pDoc = GetActiveDocument(hWnd);

	WebCommandType type = pDoc ? pDoc->OnGetWebCommandType((int)CommandId) : WEB_UNDEFINED;
	if (type != WEB_UNDEFINED)
		return type;
	CWnd* pWnd = CWnd::FromHandle(hWnd);

	//Search first parent which has no WS_CHILD style
	while (pWnd != NULL && (::GetWindowLong(pWnd->m_hWnd, GWL_STYLE) & WS_CHILD))
		pWnd = pWnd->GetParent();

	if (pWnd && ::IsWindow(hWnd))
		pWnd->SendMessage(UM_GET_WEB_COMMAND_TYPE, (int)CommandId, (int)&type);

	return type;
}

//================================CJSonResponse================================
//--------------------------------------------------------------------------------
void CJSonResponse::SetOK()
{
	WriteBool(_T("success"), true);
}

//--------------------------------------------------------------------------------
void CJSonResponse::SetError()
{
	WriteBool(_T("success"), false);

}

//--------------------------------------------------------------------------------
void CJSonResponse::SetResult(BOOL bResult)
{
	if (bResult)
		SetOK();
	else
		SetError();
}
//--------------------------------------------------------------------------------
void CJSonResponse::SetMessage(const CString& sMessage, CDiagnostic::MsgType type)
{
	OpenObject(_T("message"));
	WriteString(_T("text"), sMessage);
	WriteInt(_T("type"), type);
	CloseObject();
}

#define BEGIN_JSON_RESPONSE(command)\
CJsonSerializer resp;\
resp.WriteString(_T("cmd"), L#command);\
resp.OpenObject(_T("args"));

#define END_JSON_RESPONSE()\
resp.CloseObject();

//================================CDocumentSession================================
//--------------------------------------------------------------------------------
CDocumentSession::CDocumentSession(DWORD nLoginThreadID)
	:
	m_nLoginThreadID(nLoginThreadID)
{
	Init();
	m_nDocumentThreadID = AfxGetThread()->m_nThreadID;
}

//--------------------------------------------------------------------------------
CDocumentSession::~CDocumentSession()
{
}
//--------------------------------------------------------------------------------
void CDocumentSession::Init()
{
	SetUserInteractionMode(ATTENDED);
}

//----------------------------------------------------------------------------
void CDocumentSession::PushToClients(CJsonSerializer& resp)
{
	::PushToClients(AfxGetAuthenticationToken(), resp.GetJson());
}

//----------------------------------------------------------------------------
void CDocumentSession::OnAddThreadWindow(HWND hwnd)
{
	m_arWindowsToNotifyForCreation.Add(hwnd);
	PushWindowsToClients();
}
//----------------------------------------------------------------------------
void CDocumentSession::PushWindowsToClients()
{
	if (m_nSuspendPushToClient == 0 && m_arWindowsToNotifyForCreation.GetSize())
	{
		for (int i = 0; i < m_arWindowsToNotifyForCreation.GetSize(); i++)
		{
			BEGIN_JSON_RESPONSE(WindowOpen);
			HWND hwnd = m_arWindowsToNotifyForCreation[i];
			::SendMessage(hwnd, UM_GET_COMPONENT, (WPARAM)&(resp), NULL);
			resp.CloseObject();
			END_JSON_RESPONSE();
			PushToClients(resp);
		}

		m_arWindowsToNotifyForCreation.RemoveAll();
	}
}
//----------------------------------------------------------------------------
void CDocumentSession::PushWindowStringsToClients(HWND cmpId, const CString& sCulture)
{
	BEGIN_JSON_RESPONSE(WindowStrings);
	::SendMessage(cmpId, UM_GET_COMPONENT_STRINGS, (WPARAM)&(resp), (LPARAM)(LPCTSTR)sCulture);
	END_JSON_RESPONSE();
	PushToClients(resp);
}

//----------------------------------------------------------------------------
void CDocumentSession::PushExistDataCompletedToClient(HWND cmpId, DataObj* pValue, bool found, bool mustExist, CString requestId)
{
	BEGIN_JSON_RESPONSE(ExistDataCompleted);

	resp.WriteBool(_T("found"), found);
	resp.WriteBool(_T("mustExist"), mustExist);
	resp.WriteString(_T("requestId"), requestId);
	resp.WriteString(_T("value"), pValue->FormatDataForXML());
	END_JSON_RESPONSE();
	PushToClients(resp);
}

//-----------------------------------------------------------------------------
int CDocumentSession::MessageBoxDialog(LPCTSTR lpszText, UINT nType)
{
	m_nMessageType = nType;
	m_strMessage = lpszText;
	m_ModalClosed.Reset();
	PushMessageToClients();
	AfxGetTBThread()->LoopUntil(&m_ModalClosed);
	return m_nMessageType;//cambiato dalla risposta del client
}
//-----------------------------------------------------------------------------
BOOL CDocumentSession::DiagnosticDialog(CDiagnostic* pDiagnostic, BOOL bModal)
{
	m_pDiagnostic = pDiagnostic;
	if (bModal)
		m_ModalClosed.Reset();
	PushDiagnosticToClients();
	if (bModal)
		AfxGetTBThread()->LoopUntil(&m_ModalClosed);
	m_pDiagnostic = NULL;
	return m_bDiagnosticResult;//cambiato dalla risposta del client
}

//-----------------------------------------------------------------------------
void CDocumentSession::CloseMessageBoxDialog(CJsonParser& json)
{
	bool res;
	if (json.TryReadBool(_T("ok"), res) && res)
		m_nMessageType = IDOK;
	else if (json.TryReadBool(_T("cancel"), res) && res)
		m_nMessageType = IDCANCEL;
	else if (json.TryReadBool(_T("retry"), res) && res)
		m_nMessageType = IDRETRY;
	else if (json.TryReadBool(_T("continue"), res) && res)
		m_nMessageType = IDCONTINUE;
	else if (json.TryReadBool(_T("yes"), res) && res)
		m_nMessageType = IDYES;
	else if (json.TryReadBool(_T("no"), res) && res)
		m_nMessageType = IDNO;
	else if (json.TryReadBool(_T("abort"), res) && res)
		m_nMessageType = IDABORT;
	else if (json.TryReadBool(_T("ignore"), res) && res)
		m_nMessageType = IDIGNORE;
	else
	{
		ASSERT(FALSE);
		m_nMessageType = 0;
	}
	m_strMessage.Empty();
	m_ModalClosed.Set();
}
//-----------------------------------------------------------------------------
void CDocumentSession::CloseDiagnosticDialog(CJsonParser& json)
{
	json.TryReadBool(_T("ok"), m_bDiagnosticResult);
	m_pDiagnostic = NULL;
	m_ModalClosed.Set();
}
//----------------------------------------------------------------------------
void CDocumentSession::PushMessageToClients()
{
	if (m_strMessage.IsEmpty())
		return;

	BEGIN_JSON_RESPONSE(MessageDialog);

	TCHAR buff[32];
	CWnd* pMainWnd = AfxGetMainWnd();
	_itot_s((int)pMainWnd->m_hWnd, buff, 10);

	resp.WriteString(_T("cmpId"), buff);
	resp.WriteString(_T("text"), m_strMessage);

	if ((m_nMessageType & MB_CANCELTRYCONTINUE) == MB_CANCELTRYCONTINUE)
	{
		resp.WriteBool(_T("cancel"), true);
		resp.WriteBool(_T("retry"), true);
		resp.WriteBool(_T("continue"), true);
	}
	else if ((m_nMessageType & MB_RETRYCANCEL) == MB_RETRYCANCEL)
	{
		resp.WriteBool(_T("cancel"), true);
		resp.WriteBool(_T("retry"), true);
	}
	else if ((m_nMessageType & MB_YESNO) == MB_YESNO)
	{
		resp.WriteBool(_T("yes"), true);
		resp.WriteBool(_T("no"), true);
	}
	else if ((m_nMessageType & MB_YESNOCANCEL) == MB_YESNOCANCEL)
	{
		resp.WriteBool(_T("yes"), true);
		resp.WriteBool(_T("no"), true);
		resp.WriteBool(_T("cancel"), true);
	}
	else if ((m_nMessageType & MB_ABORTRETRYIGNORE) == MB_ABORTRETRYIGNORE)
	{
		resp.WriteBool(_T("abort"), true);
		resp.WriteBool(_T("retry"), true);
		resp.WriteBool(_T("ignore"), true);
	}
	else if ((m_nMessageType & MB_OKCANCEL) == MB_OKCANCEL)
	{
		resp.WriteBool(_T("ok"), true);
		resp.WriteBool(_T("cancel"), true);
	}
	else if ((m_nMessageType & MB_OK) == MB_OK)
	{
		resp.WriteBool(_T("ok"), true);
	}
	END_JSON_RESPONSE();

	PushToClients(resp);
}

//----------------------------------------------------------------------------
void CDocumentSession::PushRunErrorToClients()
{
	BEGIN_JSON_RESPONSE(RunError);
	AfxGetDiagnostic()->ToJson(resp);
	AfxGetDiagnostic()->ClearMessages(TRUE);
	END_JSON_RESPONSE();
	PushToClients(resp);
}
//----------------------------------------------------------------------------
void CDocumentSession::PushDiagnosticToClients()
{
	if (!m_pDiagnostic)
		return;

	BEGIN_JSON_RESPONSE(Diagnostic);

	TCHAR buff[32];
	CWnd* pMainWnd = AfxGetMainWnd();
	_itot_s((int)pMainWnd->m_hWnd, buff, 10);

	resp.WriteString(_T("cmpId"), buff);
	m_pDiagnostic->ToJson(resp);
	END_JSON_RESPONSE();
	PushToClients(resp);
}
//----------------------------------------------------------------------------
void CDocumentSession::PushItemSourceToClients(const CString& cmpId, const CStringArray& arDescriptions, const CStringArray& arData)
{
	BEGIN_JSON_RESPONSE(ItemSource);
	resp.WriteString(_T("cmpId"), cmpId);
	resp.OpenArray(_T("itemSource"));
	for (int i = 0; i < arData.GetSize(); i++)
	{
		resp.OpenObject(i);
		resp.WriteString(CString("code"), arData.GetAt(i));
		resp.WriteString(CString("description"), arDescriptions.GetAt(i));
		resp.CloseObject();
	}
	resp.CloseArray();
	END_JSON_RESPONSE();
	PushToClients(resp);
}
//----------------------------------------------------------------------------
void CDocumentSession::OnRemoveThreadWindow(HWND hwnd)
{
	BEGIN_JSON_RESPONSE(WindowClose);

	TCHAR buff[32];
	_itot_s((int)hwnd, buff, 10);
	resp.WriteString(_T("id"), buff);

	END_JSON_RESPONSE();
	PushToClients(resp);
	for (int i = 0; i < m_arWindowsToNotifyForCreation.GetSize(); i++)
		if (m_arWindowsToNotifyForCreation.GetAt(i) == hwnd)
		{
			m_arWindowsToNotifyForCreation.RemoveAt(i);
			break;
		}
}
//----------------------------------------------------------------------------
void CDocumentSession::PushDataToClients(IJsonModelProvider* pProvider)
{
	if (m_bIgnoreModelChanges)
		return;
	bool found = false;
	for (int i = 0; i < m_arJsonModelsToNotify.GetCount(); i++)
		if (m_arJsonModelsToNotify.GetAt(i) == pProvider)
		{
			found = true;
			break;
		}
	if (!found)
		m_arJsonModelsToNotify.Add(pProvider);
	PushDataToClients();
}
//----------------------------------------------------------------------------
void CDocumentSession::PushActivationDataToClients(HWND hwnd)
{
	BEGIN_JSON_RESPONSE(ActivationData);
	resp.OpenArray(_T("components"));
	int index = 0;
	::SendMessage(hwnd, UM_GET_ACTIVATION_DATA, (WPARAM)&(resp), (LPARAM)&index);

	END_JSON_RESPONSE();
	PushToClients(resp);
}

//----------------------------------------------------------------------------
void CDocumentSession::PushRadarInfoToClient(CAbstractFormDoc* pDoc)
{
	BEGIN_JSON_RESPONSE(RadarInfos);
	pDoc->GetJsonRadarInfos(resp, L"");
	END_JSON_RESPONSE();
	PushToClients(resp);
}

//----------------------------------------------------------------------------
void CDocumentSession::PushButtonsStateToClients(HWND hwnd)
{
	if (m_nSuspendPushToClient == 0 && m_bIgnoreModelChanges == 0)
	{
		BEGIN_JSON_RESPONSE(ButtonsState);
		CWnd* pWnd = CWnd::FromHandle(hwnd);
		if (!pWnd)
			return;

		CAbstractFrame* pFrame = dynamic_cast<CAbstractFrame*>(pWnd);
		if (!pFrame)
			return;

		resp.OpenObject(_T("response"));

		TCHAR buff[32];
		_itot_s((int)hwnd, buff, 10);
		resp.WriteString(_T("id"), buff);

		resp.OpenObject(_T("buttonsState"));


		CTBTabbedToolbar* pTabbedToolbar = pFrame->GetTabbedToolBar();
		if (pTabbedToolbar)
		{
			for (int i = 0; i < pFrame->GetTabbedToolBar()->GetToolBarsCount(); i++)
			{
				CTBToolBar*  pToolbar = pFrame->GetTabbedToolBar()->GetToolBar(i);
				int buttons = pToolbar->GetCount();
				for (int j = 0; j < buttons; j++)
				{
					CBCGPToolbarButton* pButton = pToolbar->GetButton(j);
					if (!pButton)
						continue;

					// Is a menu Button ?
					CTBToolbarMenuButton* pMenuButton = dynamic_cast<CTBToolbarMenuButton*> (pButton);
					BOOL bAllDisable = FALSE;
					if (pMenuButton)
					{
						CDocument* pDoc = pToolbar->GetParentDocument();
						pToolbar->PopulatedMenuButton(pMenuButton);
						CMenu menu;
						HMENU hMenu = pMenuButton->GetMenu();
						if (hMenu != NULL)
						{
							menu.Attach(hMenu);
							int iMenuCount = menu.GetMenuItemCount();
							if (pDoc && iMenuCount > 0)
							{
								bAllDisable = TRUE;
								for (int i = 0; i < iMenuCount; i++)
								{
									CString sMenuString;
									menu.GetMenuString(i, sMenuString, MF_BYPOSITION);
									UINT menuState = menu.GetMenuState(i, MF_BYPOSITION);
									// Is a separator
									if (menuState & MF_SEPARATOR) {
										continue;
									}

									UINT nID = menu.GetMenuItemID(i);
									CCmdUI state;
									state.m_nIndexMax = iMenuCount;
									state.m_nIndex = i;
									state.m_pMenu = &menu;
									state.m_nID = nID;
									pDoc->OnCmdMsg(nID, CN_UPDATE_COMMAND_UI, &state, NULL);

									menuState = menu.GetMenuState(nID, MF_BYCOMMAND);
									BOOL isMenuEnable = !(menuState & MF_DISABLED);
									INT isMenuCheck = (menuState & MF_CHECKED ? 1 : 0);
									
									CJsonResource menuResource = AfxGetTBResourcesMap()->DecodeID(TbControls, nID);
									CString cmpId = menuResource.m_strName;
									resp.OpenObject(menuResource.m_strName);
									resp.WriteBool(_T("enabled"), isMenuEnable == TRUE);
									resp.WriteInt(_T("checkStatus"), isMenuCheck);
									resp.CloseObject();
									
									if ((bAllDisable && isMenuEnable) || (nID == -1 && iMenuCount == 1))
									{
										bAllDisable = FALSE;
									}
								}
							}
						}
					}
					
					// ToolBar button
					CJsonResource resource = AfxGetTBResourcesMap()->DecodeID(TbControls, pButton->m_nID);
					CString cmpId = resource.m_strName;
					resp.OpenObject(resource.m_strName);
					// UINT nID, nStyle;
					//int iImage;
					CTBCmdUI ui(pButton->m_nID);
					ui.DoUpdate(pFrame, TRUE);

					BOOL isEnabled = ui.GetEnabled();
					int checkedState = ui.GetCheck();

					// If all items voice of menu is disable the button is disable
					if (bAllDisable)
						isEnabled = FALSE;

					resp.WriteBool(_T("enabled"), isEnabled == TRUE);
					resp.WriteInt(_T("checkStatus"), checkedState);
					resp.CloseObject();
				}
			}
		}

		resp.CloseObject();
		resp.CloseObject();
		END_JSON_RESPONSE();
		PushToClients(resp);
	}
}

//-----------------------------------------------------------------------------
void CDocumentSession::PushMessageMapToClients(CAbstractFormDoc* pDoc)
{

	CArray<int> arIDs;
	pDoc->GetIDsUsedInMessageMap(arIDs);
	CString idDoc = pDoc->GetComponentId();

	BEGIN_JSON_RESPONSE(ServerCommands);
	resp.WriteString(_T("id"), (LPCTSTR)idDoc);

	resp.OpenArray(_T("map"));
	CTBResourcesMap* pMap = AfxGetTBResourcesMap();
	for (int i = 0; i < arIDs.GetSize(); i++)
	{

		CJsonResource jsonRes = pMap->DecodeID(TbCommands, arIDs.GetAt(i));
		resp.WriteString(i, jsonRes.m_strName);
	}
	resp.CloseArray();
	END_JSON_RESPONSE();
	PushToClients(resp);
}

//----------------------------------------------------------------------------
void CDocumentSession::PushDataToClients()
{
	if (InterlockedDecrement16(&m_nPushDataNeedLevel) > 0)
		return;
	if (m_nSuspendPushToClient == 0 && m_arJsonModelsToNotify.GetSize())
	{
		bool bEmpty = true;
		BEGIN_JSON_RESPONSE(ModelData);
		resp.OpenArray(_T("models"));
		for (int i = 0; i < m_arJsonModelsToNotify.GetSize(); i++)
		{
			IJsonModelProvider* pProvider = m_arJsonModelsToNotify[i];
			resp.OpenObject(i);
			pProvider->GetJson(resp, m_bPushOnlyWebBoundData);
			resp.CloseObject(TRUE);
		}
		resp.CloseArray(TRUE);
		bEmpty = resp.IsCurrentEmpty();
		END_JSON_RESPONSE();
		if (!bEmpty)
			PushToClients(resp);
		m_arJsonModelsToNotify.RemoveAll();
	}
}

//----------------------------------------------------------------------------
void CDocumentSession::SuspendPushToClient()
{
	m_nSuspendPushToClient++;

}
//----------------------------------------------------------------------------
void CDocumentSession::PushDataNeeded()
{
	InterlockedIncrement16(&m_nPushDataNeedLevel);
}

//----------------------------------------------------------------------------
void CDocumentSession::ResumePushToClient()
{
	if (m_nSuspendPushToClient > 0)
	{
		m_nSuspendPushToClient--;
	}

	if (m_nSuspendPushToClient > 0 || m_bOperationId)
		return;
	//prima mando gli oggetti grafici, così vengono creati i componenti
	PushWindowsToClients();
	//poi mando i dati, che vendono assegnati ai componenti creati in precedenza
	PushDataToClients();
}

//----------------------------------------------------------------------------
void CDocumentSession::PushBehavioursToClient(IBehaviourContext* pContext)
{
	if (m_nSuspendPushToClient || m_bIgnoreModelChanges)
		return;

	CAbstractFormDoc* pCaller = dynamic_cast<CAbstractFormDoc*>(pContext);
	HWND hWnd = pCaller ? pCaller->GetFrameHandle() : NULL;

	BEGIN_JSON_RESPONSE(Behaviours);
	resp.OpenObject(_T("response"));
	

	resp.OpenObject(_T("behaviours"));

	// prima ciclo per tutti gli oggetti registrati bene con il loro binding
	for (int c = 0; c <= pContext->GetConsumers().GetCount() - 1; c++)
	{
		IBehaviourConsumer* pConsumer = pContext->GetConsumers().GetAt(c);
		if (!pConsumer)
			continue;

		for (int r = 0; r <= pConsumer->GetRequests().GetCount() - 1; r++)
		{
			IJsonModelProvider* pProvider = dynamic_cast<IJsonModelProvider*>(pConsumer->GetRequests().GetAt(r));
			if (pProvider)
				pProvider->GetJson(resp, m_bPushOnlyWebBoundData);
		}
	}

	// adesso devo aggiungere necessariamente la parte dei numeratori di Mago 
	// che e' sganciata dal meccanismo di bind standard
	for (int s = 0; s <= pContext->GetServices().GetCount() - 1; s++)
	{
		INumbererService* pService = dynamic_cast<INumbererService*>(pContext->GetServices().GetAt(s));

		// non sono un numeratore o non ho controlli collegati
		if (!pService || !pService->GetLinkedControls())
			continue;

		// istanzio la richiesta e chiedo. Con il meccanismo pulito di bind sparisce tutto
		CNumbererRequest aRequest(pCaller, NULL, _T(""));
		if (!pService->FireEvent(bhe_OnLoadRequestInfo, &aRequest))
			continue;

		// ora lo lego ad ogni control collegato
		for (int c = 0; c < pService->GetLinkedControls()->GetCount(); c++)
		{
			aRequest.SetEntity(pService->GetLinkedControls()->GetAt(c));
			aRequest.GetJson(resp, m_bPushOnlyWebBoundData);
		}
	}

	resp.CloseObject(TRUE);

	bool bEmpty = resp.IsCurrentEmpty();
	if (!bEmpty)
	{
		TCHAR buff[32];
		_itot_s((int)hWnd, buff, 10);
		resp.WriteString(_T("id"), buff);
	}
	resp.CloseObject(TRUE);
	END_JSON_RESPONSE();
	if (!bEmpty)
		PushToClients(resp);
}

//----------------------------------------------------------------------------
CRecordingDocumentSession::CRecordingDocumentSession()
	: CDocumentSession(AfxGetLoginContext()->m_nThreadID)
{
	m_bPushOnlyWebBoundData = false;
}

//----------------------------------------------------------------------------
void CRecordingDocumentSession::PushToClients(CJsonSerializer& resp)
{
	m_Serializer.WriteValue(m_nItems++, resp.GetCurrent());
}
//----------------------------------------------------------------------------
void CRecordingDocumentSession::Start()
{
	m_nItems = 0;
	m_Serializer.Clear();
	m_Serializer.OpenArray(_T("items"));
}
//----------------------------------------------------------------------------
void CRecordingDocumentSession::Stop()
{
	m_Serializer.CloseArray();
}
//----------------------------------------------------------------------------
void CRecordingDocumentSession::Save(LPCTSTR szFile)
{
	CLineFile file(szFile, CFile::modeCreate | CFile::modeWrite | CFile::typeText);
	file.WriteString(m_Serializer.GetJson());
}