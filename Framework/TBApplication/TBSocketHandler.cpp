#include "stdafx.h"
#include <TbWoormViewer\WOORMDOC.H>
#include "TBSocketHandler.h"
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
	functionMap[_T("getWindowStrings")] = &CTBSocketHandler::GetWindowStrings;
	functionMap[_T("checkMessageDialog")] = &CTBSocketHandler::CheckMessageDialog;
	functionMap[_T("doFillListBox")] = &CTBSocketHandler::DoFillListBox;
	functionMap[_T("setReportResult")] = &CTBSocketHandler::SetReportResult; 
	functionMap[_T("runDocument")] = &CTBSocketHandler::RunDocument;
	functionMap[_T("getRadarQuery")] = &CTBSocketHandler::GetRadarQuery;
	functionMap[_T("browseRecord")] = &CTBSocketHandler::BrowseRecord;
}

//--------------------------------------------------------------------------------
CTBSocketHandler::~CTBSocketHandler()
{
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
	CBaseDocument* pDoc = AfxGetTbCmdManager()->RunDocument(sNamespace, szDefaultViewMode, FALSE, NULL, NULL);

	if (!pDoc)
	{
		return;
	}
	else if (!sArguments.IsEmpty())
	{
		//ASSERT_VALID(pDoc); non si puo' fare perch� fa un AssertValid anche della view
		CFunctionDescription fd;
		if (fd.ParseArguments(sArguments))
			pDoc->GoInBrowserMode(&fd);
	}
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
	//non sospendo la push, perch� il comando potrebbe bloccarmi con una dialog modale, 
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
	HWND cmpId = ReadComponentId(json);
	//aggiornamento del model
	pSession->SetJsonModel(json, cmpId);
	//esecuzione comando
	CDocument* pDoc = GetDocumentFromHwnd(cmpId);
	ASSERT(pDoc);
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

	if (json.BeginReadObject(_T("itemSource")))
	{
		CString itemSourceName = json.ReadString(_T("name"));
		CString itemSourceNamespace = json.ReadString(_T("namespace"));
		CString itemSourceParameter = json.ReadString(_T("parameter"));
		bool itemSourceUseProductLanguage = json.ReadBool(_T("useProductLanguage"));
		bool itemSourceAllowChanges = json.ReadBool(_T("allowChanges"));
		json.EndReadObject();

		CString currentValue; //TODOLUCA
		CItemSource* pItemSource = pDoc->GetItemSource(itemSourceName, itemSourceNamespace);
		if (pItemSource)
		{
			CStringArray descriptions;
			DataArray values;
			pItemSource->GetData(values, descriptions, currentValue);

			CDocumentSession* pSession = (CDocumentSession*)AfxGetThreadContext()->m_pDocSession;
			if (!pSession)
			{
				ASSERT(FALSE);
				return;
			}

			CStringArray strArray;
			for (int i = 0; i < values.GetSize(); i++)
			{
				strArray.Add(values.GetAt(i)->FormatData());
			}
			pSession->PushItemSourceToClients(descriptions, strArray);
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
			pSession->PushItemSourceToClients(descriptions, strArray);
		}
	}
}


//--------------------------------------------------------------------------------
void CTBSocketHandler::GetRadarQuery(CJsonParser& json)
{
	CJSonResponse aResponse;
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
		pSession->PushRadarInfoToClient((CAbstractFormDoc*)pDoc);
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

	//non sospendo la push, perch� il comando potrebbe bloccarmi con una dialog modale, 
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
		pSession->PushActivationDataToClients();
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