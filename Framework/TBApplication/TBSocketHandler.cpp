#include "stdafx.h"
#include <TbWoormViewer\WOORMDOC.H>
#include "TBSocketHandler.h"
#include "DocumentThread.h"
#include <TbGes\DocumentSession.h>
#include <TbGes\ExtDocAbstract.h>

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
CTBSocketHandler::CTBSocketHandler()
{
	functionMap[_T("doClose")] = &CTBSocketHandler::DoClose;
	functionMap[_T("doCommand")] = &CTBSocketHandler::DoCommand;
	functionMap[_T("doValueChanged")] = &CTBSocketHandler::DoValueChanged;
	functionMap[_T("doCloseMessageDialog")] = &CTBSocketHandler::DoCloseMessage;
	functionMap[_T("doCloseDiagnosticDialog")] = &CTBSocketHandler::DoCloseDiagnostic;
	functionMap[_T("getOpenDocuments")] = &CTBSocketHandler::GetOpenDocuments;
	functionMap[_T("getDocumentData")] = &CTBSocketHandler::GetDocumentData;
	functionMap[_T("getActivationData")] = &CTBSocketHandler::GetActivationData;
	functionMap[_T("getWindowStrings")] = &CTBSocketHandler::GetWindowStrings;
	functionMap[_T("checkMessageDialog")] = &CTBSocketHandler::CheckMessageDialog;
	functionMap[_T("doFillListBox")] = &CTBSocketHandler::DoFillListBox;
	functionMap[_T("setReportResult")] = &CTBSocketHandler::SetReportResult;
	functionMap[_T("runDocument")] = &CTBSocketHandler::RunDocument;
	functionMap[_T("browseRecord")] = &CTBSocketHandler::BrowseRecord;
	functionMap[_T("openHyperLink")] = &CTBSocketHandler::OpenHyperLink;
	functionMap[_T("queryHyperLink")] = &CTBSocketHandler::QueryHyperLink;
	functionMap[_T("doControlCommand")] = &CTBSocketHandler::DoControlCommand;
	functionMap[_T("updateTitle")] = &CTBSocketHandler::DoUpdateTitle;
	functionMap[_T("activateContainer")] = &CTBSocketHandler::DoActivateClientContainer;
	functionMap[_T("pinUnpin")] = &CTBSocketHandler::DoPinUnpin;
	functionMap[_T("doCheckListBoxAction")] = &CTBSocketHandler::DoCheckListBoxAction;
}

//--------------------------------------------------------------------------------
CTBSocketHandler::~CTBSocketHandler()
{
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::OpenHyperLink(CJsonParser& json)
{
	CString sName = json.ReadString(_T("name"));
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromJson(json);
	if (!pDoc) return;

	HotKeyLink* pHkl = pDoc->GetHotLink(sName);
	if (pHkl)
	{
		DataObj* pData = pHkl->GetDataObj();
		if (pData) pHkl->BrowserLink(pData);
	}
}

CString ConvertJsonValue(CJsonParser& json)
{
	int type = json.ReadInt(_T("type"));
	CString sValue;
	switch (type) {
	case 1:
		sValue = json.ReadString(_T("value"));
		break;
	case 2:
		sValue = DataInt(json.ReadInt(_T("value"))).Str();
		break;
	case 3:
		sValue = DataInt(json.ReadInt(_T("value"))).Str();
		break;
	case 4:
		sValue = DataDbl(json.ReadDouble(_T("value"))).Str();
		break;
	case 5:
		sValue = DataMon(json.ReadDouble(_T("value"))).Str();
		break;
	case 6:
		sValue = DataQty(json.ReadDouble(_T("value"))).Str();
		break;
	case 7:
		sValue = DataPerc(json.ReadDouble(_T("value"))).Str();
		break;
	case 8:
		sValue = DataDate(json.ReadString(_T("value"))).Str();
		break;
	case 9:
		sValue = DataBool(json.ReadBool(_T("value"))).Str();
		break;
	case 10:
		sValue = DataEnum(json.ReadInt(_T("value"))).Str();
		break;
	default:
		sValue = json.ReadString(_T("value"));
		break;
	}

	return sValue;
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::QueryHyperLink(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (pSession)
	{
		CString sName = json.ReadString(_T("name"));
		CString sRequestID = json.ReadString(_T("requestId"));
		CString sControlId = json.ReadString(_T("controlId"));
		HWND cmpId = ReadComponentId(json);
		CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocument((int)cmpId);
		if (!pDoc) return;

		HotKeyLink* pHkl = pDoc->GetHotLink(sName);
		if (!pHkl) return;
		DataObj* pData = pHkl->GetDataObj()->Clone();
		if (!pData) return;

		//CString sValue = ConvertJsonValue(json);
		CString sValue = json.ReadString(_T("value"));
		pData->AssignFromXMLString(sValue);

		pHkl->SetWebCallinkInfo(sRequestID, cmpId);
		BOOL ok = pHkl->ExistData(pData);
		if (ok)
		{
			pSession->PushExistDataCompletedToClient(cmpId, pData,  ok == TRUE, pHkl->IsMustExistData() == TRUE, sRequestID);
			pHkl->SetWebCallinkInfo(NULL, NULL);
		}
		
		TRACE(_T("requestId: %s controlId: %s \r\n"), sRequestID, sControlId);
		SAFE_DELETE(pData);
	}
}
//--------------------------------------------------------------------------------
CBaseDocument* CTBSocketHandler::GetDocument(int cmpId)
{
	CSingleLock lock(&m_Critical, TRUE);
	CBaseDocument* pDoc = m_arDocuments[cmpId];
	if (!pDoc)
		m_arDocuments.RemoveKey(cmpId);
	return pDoc;
}

//--------------------------------------------------------------------------------
CBaseDocument* CTBSocketHandler::GetDocumentFromJson(CJsonParser& jsonParser)
{
	HWND hwnd = ReadComponentId(jsonParser);
	CBaseDocument* pDoc = GetDocument((int)hwnd);
	if (pDoc)
		return pDoc;
	pDoc = GetDocumentFromHwnd(hwnd);
	if (pDoc)
	{
		CSingleLock lock(&m_Critical, TRUE);
		m_arDocuments[(int)hwnd] = pDoc;
	}
	return pDoc;
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::RunDocumentOnThreadDocument(const CString& sNamespace, const CString& sArguments)
{
	CBaseDocument* pDoc = NULL;

	AfxGetDiagnostic()->StartSession(_TB("Document opening messages"));
	pDoc = AfxGetTbCmdManager()->RunDocument(sNamespace, szDefaultViewMode, FALSE, NULL, NULL);

	if (!pDoc)
	{
		CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
		if (pSession)
		{
			pSession->PushRunErrorToClients();
		}
		return;
	}
	
	if (!sArguments.IsEmpty())
	{
		CFunctionDescription fd;
		if (fd.ParseArguments(sArguments))
			pDoc->GoInBrowserMode(&fd);
	}

	AfxGetDiagnostic()->EndSession();
	
	CSingleLock lock(&m_Critical, TRUE);
	m_arDocuments[(int)pDoc->GetFrameHandle()] = pDoc;
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::RunDocument(CJsonParser& json)
{
	CString sNamespace = json.ReadString(_T("ns"));
	CString sArguments = json.ReadString(_T("sKeyArgs"));

	if (!AfxGetLoginContext())
	{
		return;
	}
	CDocumentThread* pThread = (CDocumentThread*)AfxGetTbCmdManager()->CreateDocumentThread();
	AfxInvokeThreadProcedure<CTBSocketHandler, const CString&, const CString&>(pThread->m_nThreadID, this, &CTBSocketHandler::RunDocumentOnThreadDocument, sNamespace, sArguments);
}

//--------------------------------------------------------------------------------
bool CTBSocketHandler::IsCancelableCommand(const CString& sCommand, CJsonParser* pParser)
{
	return sCommand == _T("browseRecord");
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::Execute(CString& sSocketName, CString& sMessage)
{
	CJsonParser* pParser = new CJsonParser;
	pParser->ReadJsonFromString(sMessage);
	CString sCommand = pParser->ReadString(_T("cmd"));
	FUNCPTR fn;
	if (functionMap.Lookup(sCommand, fn))
	{
		CAbstractFormDoc* pAbsDoc = NULL;
		DWORD pId = 0;
		DWORD tId = 0;
		HWND cmpId = ReadComponentId(*pParser);
		if (cmpId)
		{
			CBaseDocument* pDoc = GetDocument((int)cmpId);
			if (pDoc)
			{
				tId = pDoc->GetThreadId();
				pAbsDoc = (CAbstractFormDoc*)pDoc;
				//è un comando che può essere cancellato da uno successivo
				
				{
					//se ho una operazione in corso (evento cancellabile)
					if (WaitForSingleObject(pAbsDoc->m_WebOperationComplete, 0) != WAIT_OBJECT_0)
					{
						//alzo il flag di aborted, non manderò il json
						pAbsDoc->m_AbortJsonData.SetEvent();
						if (IsCancelableCommand(sCommand, pParser))
							pAbsDoc->m_AbortWebOperation.SetEvent();
						//e aspetto che termini l'operazione
						if (WaitForSingleObject(pAbsDoc->m_WebOperationComplete, 60000) != WAIT_OBJECT_0)
						{
							ASSERT(FALSE);
							return;
						}
					}

					//il flag di aborted è a false
					pAbsDoc->m_AbortJsonData.ResetEvent();
					pAbsDoc->m_AbortWebOperation.ResetEvent();
					//il flag di completed è a false
					pAbsDoc->m_WebOperationComplete.ResetEvent();
				}
			}
			else
			{
				tId = GetWindowThreadProcessId(cmpId, &pId);
			}
		}
		else
		{
			CLoginContext* pContext = AfxGetLoginContext(sSocketName);
			if (pContext)
				tId = pContext->m_nThreadID;
		}

		if (!tId)
			tId = AfxGetApp()->m_nThreadID;

		AfxInvokeAsyncThreadProcedure<CTBSocketHandler, FUNCPTR, CJsonParser*, CAbstractFormDoc*>(tId, this, &CTBSocketHandler::ExecuteFunction, fn, pParser, pAbsDoc);

	}
	else
	{
		delete pParser;
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::ExecuteFunction(FUNCPTR fn, CJsonParser* pParser, CAbstractFormDoc* pDoc)
{
	try
	{
		(this->*(fn))(*pParser);
	}
	catch (...)
	{
		delete pParser;
		if (pDoc)
			pDoc->m_WebOperationComplete.SetEvent();
		ASSERT(FALSE);
		throw;
	}
	delete pParser;
	if (pDoc)
		pDoc->m_WebOperationComplete.SetEvent();
}


//--------------------------------------------------------------------------------
void PushWindowsToClients()
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (pSession)
	{
		HWNDArray& arWnds = AfxGetThreadContext()->GetThreadWindows();
		for (int i = 0; i < arWnds.GetCount(); i++)
		{
			HWND hwnd = arWnds[i];
			pSession->OnAddThreadWindow(hwnd);
		}
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::GetOpenDocuments(CJsonParser& json)
{
	CArray<long, long> arThreadIds;
	AfxGetLoginContext()->GetDocumentThreads(arThreadIds);
	for (int i = 0; i < arThreadIds.GetSize(); i++)
	{
		AfxInvokeAsyncThreadGlobalProcedure(arThreadIds[i], PushWindowsToClients);
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::DoCommand(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}
	//non sospendo la push, perch� il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	HWND cmpId = ReadComponentId(json);
	DWORD idc = 0;
	CString controlId;
	if (json.TryReadString(_T("controlId"), controlId))
		idc = AfxGetTBResourcesMap()->GetTbResourceID(controlId, TbControls);

	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocument((int)cmpId);
	//aggiornamento del model
	pDoc->SetJson(json);
	if (idc)
	{
		CParsedCtrl* pCtrl = pDoc->GetLinkedParsedCtrl(idc);
		if (pCtrl && pCtrl->GetControlBehaviour())
		{
			pCtrl->GetControlBehaviour()->OnCmdMsg(id, 0, NULL, NULL);
		}
	}
	SendMessage(cmpId, WM_COMMAND, id, NULL);
	//pSession->ResumePushToClient();
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::DoControlCommand(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	//non sospendo la push, perch� il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	//esecuzione comando
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromJson(json);
	CUpdateDataViewLevel _upd(pDoc);
	if (pDoc)
	{
		//aggiornamento del model
		pDoc->SetJson(json);
	
		pDoc->OnCmdMsg(id, EN_CTRL_STATE_CHANGED, NULL, NULL);
		pDoc->UpdateDataView();
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::DoActivateClientContainer(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	CString sId = json.ReadString(_T("id"));
	bool active = false;
	json.TryReadBool(_T("active"), active);
	bool tileGroup = false;
	json.TryReadBool(_T("isTileGroup"), tileGroup);
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, tileGroup ? TbCommands : TbResources);
	//esecuzione comando
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromJson(json);
	if (pDoc)
	{
		//aggiornamento del model
		pDoc->SetJson(json);
	
		pDoc->ActivateWebClientContainer(id, active);
	}
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::DoUpdateTitle(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	//non sospendo la push, perch� il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	//esecuzione comando
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromJson(json);
	CUpdateDataViewLevel _upd(pDoc);
	if (pDoc)
	{
		//aggiornamento del model
		pDoc->SetJson(json);
		pDoc->DoUpdateTitle(id);
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::DoPinUnpin(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	//non sospendo la push, perch� il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	bool isPinned = json.ReadBool(_T("pinned"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	//esecuzione comando
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromJson(json);
	CUpdateDataViewLevel _upd(pDoc);
	if (pDoc)
	{
		//aggiornamento del model
		pDoc->SetJson(json);
		pDoc->DoPinUnpin(id, isPinned);
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::DoClose(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}
	HWND cmpId = ReadComponentId(json);
	SendMessage(cmpId, WM_CLOSE, NULL, NULL);

}
//--------------------------------------------------------------------------------
void CTBSocketHandler::DoValueChanged(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	//non sospendo la push, perch� il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	//esecuzione comando
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromJson(json);
	if (!pDoc)
		return;
	//aggiornamento del model
	pDoc->SetJson(json);

	CUpdateDataViewLevel _upd(pDoc);
	pDoc->OnCmdMsg(id, EN_VALUE_CHANGED, NULL, NULL);
	//pSession->ResumePushToClient();
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::DoCloseMessage(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}
	if (json.BeginReadObject(_T("result")))
	{
		pSession->CloseMessageBoxDialog(json);
		json.EndReadObject();
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::DoCloseDiagnostic(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}
	if (json.BeginReadObject(_T("result")))
	{
		pSession->CloseDiagnosticDialog(json);
		json.EndReadObject();
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::SetReportResult(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}
	CDocument* pDoc = GetDocumentFromJson(json);
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
	{
		CWoormDoc* pWoormDoc = (CWoormDoc*)pDoc;
		pWoormDoc->SetJsonResult(json);
	}
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::DoCheckListBoxAction(CJsonParser& json)
{
	enum CheckListBoxAction { CLB_QUERY_LIST = 0, CLB_SET_VALUES = 1, CLB_DBL_CLICK = 2 };

	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromJson(json);
	if (!pDoc)
		return;

	CString controlId = json.ReadString(_T("controlId"));
	if (!controlId)
		return;

	DWORD idc;

	int action = json.ReadInt(_T("action"));

	switch (action)
	{
	case CLB_QUERY_LIST:
	{
		pushCheckListBoxItemSource(json, pDoc, controlId);
		break;
	}
	case CLB_SET_VALUES:
	case CLB_DBL_CLICK:
	{
		idc = AfxGetTBResourcesMap()->GetTbResourceID(controlId, TbControls);
		if (!idc)
			return;

		CParsedCtrl* pCtrl = pDoc->GetLinkedParsedCtrl(idc);
		if (!pCtrl || !pCtrl->GetControlBehaviour())
			return;

		pCtrl->GetControlBehaviour()->ReceiveInfo(json);
		pushCheckListBoxItemSource(json, pDoc, controlId);
		break;
	}
	}

}

//--------------------------------------------------------------------------------
void CTBSocketHandler::pushCheckListBoxItemSource(CJsonParser& json, CAbstractFormDoc* pDoc, const CString& controlId)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	//if (json.BeginReadObject(_T("itemSource")))
	//{
		CString itemSourceName = json.ReadString(_T("itemSourceName"));
		CString itemSourceNamespace = json.ReadString(_T("itemSourceNamespace"));

		CItemSource* pItemSource = pDoc->GetItemSource(itemSourceName, CTBNamespace(CTBNamespace::ITEMSOURCE, itemSourceNamespace));


		if (!pItemSource)
			return;

		CJsonSerializer resp = pItemSource->GetJson(controlId);

		pSession->PushToClients(resp);
	//}
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::DoFillListBox(CJsonParser& json)
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromJson(json);
	if (!pDoc)
		return;

	DWORD idc;
	CString controlId;

	json.TryReadString(_T("controlId"), controlId);
	if (!controlId.IsEmpty())
		idc = AfxGetTBResourcesMap()->GetTbResourceID(controlId, TbControls);
	CString currentValue; //TODOLUCA
	CStringArray descriptions;
	DataArray values;
	CStringArray strArray;
	bool bDataFound = false;
	IItemSource* pItemSource = NULL;
	
	//LUCA, discrepanza tra itemsource e controlbehavior, adesso nel ts e html name e namespace sono due variabili separate, perchè  possono essere bindate a 
	//variabili di documento; mi aspetto che prima o poi esca un caso del genere anche sul behaviour
	CString itemSourceName = json.ReadString(_T("itemSourceName"));
	CString itemSourceNamespace = json.ReadString(_T("itemSourceNamespace"));
	if (!itemSourceName.IsEmpty() && !itemSourceNamespace.IsEmpty())
	{
		pItemSource = pDoc->GetItemSource(itemSourceName, CTBNamespace(CTBNamespace::ITEMSOURCE, itemSourceNamespace));
	}
	else if (json.BeginReadObject(_T("controlBehaviour")))
	{
		bool isItemSource = false;
		if (json.TryReadBool(_T("itemSource"), isItemSource) && isItemSource)
		{
			CString name = json.ReadString(_T("name"));
			CString nameSpace = json.ReadString(_T("namespace"));
			pItemSource = dynamic_cast<IItemSource*>(pDoc->GetControlBehaviour(name, CTBNamespace(CTBNamespace::CONTROL_BEHAVIOUR, nameSpace)));
		}
		json.EndReadObject();
	}

	if (pItemSource)
	{

		pItemSource->GetData(values, descriptions, currentValue);
		if (pItemSource->HasNoData())
		{
			DataEnum* pEnum = NULL;
			if (idc)
			{
				CParsedCtrl* pCtrl = pDoc->GetLinkedParsedCtrl(idc);
				if (pCtrl)
					pEnum = dynamic_cast<DataEnum*> (pCtrl->GetCtrlData());
			}
			if (!pEnum)
			{
				ASSERT(FALSE);
				return;
			}

			int tag = pEnum->GetTagValue();

			const EnumItemArray* pArray = AfxGetEnumsTable()->GetEnumItems(tag);
			if (!pArray)
			{
				ASSERT(FALSE);
				return;
			}

			for (int i = 0; i < pArray->GetSize(); i++)
			{
				EnumItem* pItem = pArray->GetAt(i);
				if (!pItem)
					continue;

				DataEnum current = DataEnum(pEnum->GetTagValue(), pItem->GetItemValue());

				if (pItemSource->IsValidItem(current))
				{
					DataInt value = current.GetValue();
					strArray.Add(value.ToString());
					CString desc = pItem->GetItemName();
					descriptions.Add(desc);
				}
			}
		}
		else
		{
			for (int i = 0; i < values.GetSize(); i++)
			{
				DataObj* pObj = values.GetAt(i);
				if (pItemSource->IsValidItem(*pObj))
					strArray.Add(pObj->Str());
			}
		}
		bDataFound = true;

	}

	if (bDataFound)
	{
		CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
		if (!pSession)
		{
			ASSERT(FALSE);
			return;
		}


		pSession->PushItemSourceToClients(controlId, descriptions, strArray);
	}

	if (json.BeginReadObject(_T("hotLink")))
	{
		CString hotLinkName = json.ReadString(_T("hotLink"));
		CString hotLinkNamespace = json.ReadString(_T("hotLinkNS"));
		json.EndReadObject();
		CStringArray descriptions;
		DataObjArray values;

		CTBNamespace hotLinkNS(CTBNamespace::HOTLINK, hotLinkNamespace);
		HotKeyLink* pHotLink = pDoc->GetHotLink(hotLinkName, hotLinkNS.ToString());
		if (pHotLink)
		{
			pHotLink->DoSearchComboQueryData(50, values, descriptions);

			CStringArray strArray;
			for (int i = 0; i < values.GetSize(); i++)
			{
				strArray.Add(values.GetAt(i)->FormatData());
			}

			CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
			if (!pSession)
			{
				ASSERT(FALSE);
				return;
			}
			pSession->PushItemSourceToClients(controlId, descriptions, strArray);
		}
	}
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::BrowseRecord(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)(GetDocumentFromJson(json));
	if (!pDoc)
	{
		ASSERT(FALSE);
		return;
	}
	pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	CString tbGuid = json.ReadString(_T("tbGuid"));


	//aggiornamento del model
	pDoc->SetJson(json);

	//esecuzione comando
	pDoc->BrowseRecordByTBGuid(tbGuid);
	pSession->ResumePushToClient();
}


//--------------------------------------------------------------------------------
void CTBSocketHandler::GetActivationData(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}
	HWND cmpId = ReadComponentId(json);
	pSession->PushActivationDataToClients(cmpId);
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::GetDocumentData(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	CBaseDocument* pDoc = GetDocumentFromJson(json);
	ASSERT(pDoc);
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		((CAbstractFormDoc*)pDoc)->ResetJsonData(json);
		pSession->PushDataToClients((CAbstractFormDoc*)pDoc);
		pSession->PushMessageMapToClients(((CAbstractFormDoc*)pDoc));
		pSession->PushButtonsStateToClients(pDoc->GetFrameHandle());
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::GetWindowStrings(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	HWND cmpId = ReadComponentId(json);
	CString sCulture = json.ReadString(_T("culture"));
	pSession->PushWindowStringsToClients(cmpId, sCulture);
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::CheckMessageDialog(CJsonParser& json)
{
	CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
	if (!pSession)
	{
		ASSERT(FALSE);
		return;
	}

	pSession->PushMessageToClients();//messagebox
	pSession->PushDiagnosticToClients();//CMessages
}