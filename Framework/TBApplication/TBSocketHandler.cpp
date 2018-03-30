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
	functionMap[_T("openNewHyperLink")] = &CTBSocketHandler::OpenNewHyperLink;
	functionMap[_T("doControlCommand")] = &CTBSocketHandler::DoControlCommand;
	functionMap[_T("updateTitle")] = &CTBSocketHandler::DoUpdateTitle;
	functionMap[_T("activateContainer")] = &CTBSocketHandler::DoActivateClientContainer;
	functionMap[_T("pinUnpin")] = &CTBSocketHandler::DoPinUnpin;

}

//--------------------------------------------------------------------------------
CTBSocketHandler::~CTBSocketHandler()
{
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::OpenHyperLink(CJsonParser& json)
{
	CString sName = json.ReadString(_T("name"));
	HWND cmpId = ReadComponentId(json);
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromHwnd(cmpId);
	
	if (!pDoc) return;

	HotKeyLink* pHkl = pDoc->GetHotLink(sName);
	if (pHkl)
	{
		DataObj* pData = pHkl->GetDataObj();
		if(pData) pHkl->BrowserLink(pData);
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
	CString sName = json.ReadString(_T("name"));
	HWND cmpId = ReadComponentId(json);
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromHwnd(cmpId);
	if (!pDoc) return;

	HotKeyLink* pHkl = pDoc->GetHotLink(sName);
	if (!pHkl) return;
	DataObj* pData = pHkl->GetDataObj()->Clone();
	if (!pData) return;

	CString sValue = ConvertJsonValue(json);
	pData->AssignFromXMLString(sValue);
	pHkl->ExistData(pData);

	SAFE_DELETE(pData);
}

//--------------------------------------------------------------------------------
void CTBSocketHandler::OpenNewHyperLink(CJsonParser& json)
{
	CString sName = json.ReadString(_T("name"));
	HWND cmpId = ReadComponentId(json);
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromHwnd(cmpId);

	if (!pDoc) return;

	HotKeyLink* pHkl = pDoc->GetHotLink(sName);
	if (!pHkl) return;
	DataObj* pData = pHkl->GetDataObj();
	if (!pData) return;
	CString sValue = ConvertJsonValue(json);
	pData->AssignFromXMLString(sValue);
	pHkl->DoCallLink(); 
}

//--------------------------------------------------------------------------------
void RunDocumentOnThreadDocument(const CString& sNamespace, const CString& sArguments)
{
	CBaseDocument* pDoc = NULL;
	{	//LASCIARE LA GRAFFA, E' LO SCOPE DI SwitchTemporarilyMode tmp
		//in questa fase non vanno segnalati messaggi con dialog modali
		SwitchTemporarilyMode tmp(UNATTENDED);

		AfxGetDiagnostic()->StartSession(_TB("Document opening messages"));
		pDoc = AfxGetTbCmdManager()->RunDocument(sNamespace, szDefaultViewMode, FALSE, NULL, NULL);

		if (!pDoc)
		{
			CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
			if (pSession)
			{
				pSession->PushRunErrorToClients();
			}
		}
		AfxGetDiagnostic()->EndSession();
		return;
	}
	
	if (!sArguments.IsEmpty())
	{
		CFunctionDescription fd;
		if (fd.ParseArguments(sArguments))
			pDoc->GoInBrowserMode(&fd);
	}
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
	CDocumentThread* pThread = (CDocumentThread*) AfxGetTbCmdManager()->CreateDocumentThread();
	AfxInvokeThreadGlobalProcedure<const CString&, const CString&>(pThread->m_nThreadID, &RunDocumentOnThreadDocument, sNamespace, sArguments);
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
		DWORD pId = 0;
		DWORD tId = 0;
		HWND cmpId = ReadComponentId(*pParser);
		if (cmpId)
		{
			tId = GetWindowThreadProcessId(cmpId, &pId);
		}
		else
		{
			CLoginContext* pContext = AfxGetLoginContext(sSocketName);
			if (pContext)
				tId = pContext->m_nThreadID;
		}

		if (!tId)
			tId = AfxGetApp()->m_nThreadID;
		
		AfxInvokeAsyncThreadProcedure<CTBSocketHandler, FUNCPTR, CJsonParser*>(tId, this, &CTBSocketHandler::ExecuteFunction, fn, pParser);

	}
	else
	{
		delete pParser;
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::ExecuteFunction(FUNCPTR fn, CJsonParser* pParser)
{
	(this->*(fn))(*pParser);
	delete pParser;
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
	//non sospendo la push, perché il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	HWND cmpId = ReadComponentId(json);
	//aggiornamento del model
	pSession->SetJsonModel(json, cmpId);
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

	//non sospendo la push, perché il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	HWND cmpId = ReadComponentId(json);
	//aggiornamento del model
	pSession->SetJsonModel(json, cmpId);
	//esecuzione comando
	CBaseDocument* pDoc = GetDocumentFromHwnd(cmpId);
	CUpdateDataViewLevel _upd(pDoc);
	if (pDoc)
	{
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
	HWND cmpId = ReadComponentId(json);
	//aggiornamento del model
	pSession->SetJsonModel(json, cmpId);
	//esecuzione comando
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromHwnd(cmpId);
	if (pDoc)
	{
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

	//non sospendo la push, perché il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	HWND cmpId = ReadComponentId(json);
	//aggiornamento del model
	pSession->SetJsonModel(json, cmpId);
	//esecuzione comando
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromHwnd(cmpId);
	CUpdateDataViewLevel _upd(pDoc);
	if (pDoc)
	{
		pDoc->OnUpdateTitle(id);
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

	//non sospendo la push, perché il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	bool isPinned = json.ReadBool(_T("pinned"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	HWND cmpId = ReadComponentId(json);
	//aggiornamento del model
	pSession->SetJsonModel(json, cmpId);
	//esecuzione comando
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromHwnd(cmpId);
	CUpdateDataViewLevel _upd(pDoc);
	if (pDoc)
	{
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

	//non sospendo la push, perché il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	HWND cmpId = ReadComponentId(json);
	//aggiornamento del model
	pSession->SetJsonModel(json, cmpId);
	//esecuzione comando
	CBaseDocument* pDoc = GetDocumentFromHwnd(cmpId);
	if (!pDoc)
		return;

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
	HWND cmpId = ReadComponentId(json);
	CDocument* pDoc = GetDocumentFromHwnd(cmpId);
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
	{
		CWoormDoc* pWoormDoc = (CWoormDoc*)pDoc;
		pWoormDoc->SetJsonResult(json);
	}
}
//--------------------------------------------------------------------------------
void CTBSocketHandler::DoFillListBox(CJsonParser& json)
{
	HWND cmpId = ReadComponentId(json);
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)GetDocumentFromHwnd(cmpId);
	if (!pDoc)
		return;

	DWORD idc;
	CString controlId;

	json.TryReadString(_T("controlId"), controlId);
	if (!controlId.IsEmpty())
		idc = AfxGetTBResourcesMap()->GetTbResourceID(controlId, TbControls);

	if (json.BeginReadObject(_T("itemSource")))
       {
             CString itemSourceName = json.ReadString(_T("name"));
             CString itemSourceNamespace = json.ReadString(_T("namespace"));
             CString itemSourceParameter = json.ReadString(_T("parameter"));
             bool itemSourceUseProductLanguage = json.ReadBool(_T("useProductLanguage"));
             bool itemSourceAllowChanges = json.ReadBool(_T("allowChanges"));             
			 json.EndReadObject();

             CString currentValue; //TODOLUCA
             CItemSource* pItemSource = pDoc->GetItemSource(itemSourceName, CTBNamespace(CTBNamespace::ITEMSOURCE, itemSourceNamespace));
             if (pItemSource)
             {
                    CStringArray descriptions;
                    DataArray values;
					CStringArray strArray;

                    pItemSource->GetData(values, descriptions, currentValue);
                    if (pItemSource->GetNoData())
                    {
							DataEnum* pEnum;   
							if (idc)
								pEnum = dynamic_cast<DataEnum*> (pDoc->GetLinkedParsedCtrl(idc)->GetCtrlData());
							
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
									  strArray.Add(values.GetAt(i)->ToString());
						}
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

	//non sospendo la push, perché il comando potrebbe bloccarmi con una dialog modale, 
	//e non potrei chiudere con la ResumePushToClient
	//pSession->SuspendPushToClient();
	CString sId = json.ReadString(_T("id"));
	DWORD id = AfxGetTBResourcesMap()->GetTbResourceID(sId, TbCommands);
	CString tbGuid = json.ReadString(_T("tbGuid"));
	HWND cmpId = ReadComponentId(json);

	//aggiornamento del model
	pSession->SetJsonModel(json, cmpId);
	//esecuzione comando
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)(GetDocumentFromHwnd(cmpId));
	
	ASSERT(pDoc);
	pDoc->BrowseRecordByTBGuid(tbGuid);
	//pSession->ResumePushToClient();
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

	HWND cmpId = ReadComponentId(json);
	CBaseDocument* pDoc = GetDocumentFromHwnd(cmpId);
	ASSERT(pDoc);
	if (pDoc && pDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{ 
		((CAbstractFormDoc*)pDoc)->ResetJsonData(json);
		pSession->PushDataToClients((CAbstractFormDoc*)pDoc);
		pSession->PushMessageMapToClients(((CAbstractFormDoc*)pDoc));
		pSession->PushButtonsStateToClients(cmpId);
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