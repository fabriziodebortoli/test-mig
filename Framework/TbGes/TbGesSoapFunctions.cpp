
#include "StdAfx.h"

#include <TBNameSolver\LoginContext.h>
#include <TBNameSolver\Templates.h>
#include <TBNameSolver\Diagnostic.h>
#include <TBNameSolver\ThreadInfo.h>

#include <TbGeneric\WebServiceStateObjects.h>

#include <TbGenlib\BaseApp.h>
#include <TbGenlib\BaseDoc.h>
#include <TbGenlib\FunProto.h>
#include <TbGenlib\ExternalControllerInfo.h>
#include <TbGenlib\DiagnosticManager.h>

#include <TbGenLibManaged\Main.h>

#include <TbWoormEngine\report.h>

#include <TBApplication\DocumentThread.h>


#include "Formmng.h"
#include "Formmngdlg.h"

#include "ExtDoc.h"

#include "XMLDocGenerator.h"

#include "soapfunctions.h"

// costanti stringa
BEGIN_TB_STRING_MAP(Messages)
	TB_LOCALIZED(INVALID_DOCUMENT, "Document not valid")
	TB_LOCALIZED(INVALID_WOORM_INFO, "The itentifier of the object CWoormInfo is not valid.")
}; //chiusura senza utilizzare la macro, perche con la macro si ha un intellisense error che pregiudica la creazione dei webmethod da parte dell'addin (viene cancellato il primo dopo questa riga)

//-----------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// Get the number of connected users
///</summary>
DataInt GetLogins()
{
	return AfxGetApplicationContext()->GetLoginContextNumber();
}

//-----------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// Sets user interaction mode for the login thread: 0 = Undefined, 1 = Attended, 2 = Unattended
///</summary>
void SetUserInteractionMode(DataInt mode)
{
	AfxGetThreadContext()->SetUserInteractionMode((UserInteractionMode)(int)mode);
}
//-----------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// Informs that application is remote controlled (for example by a web application)
///</summary>
void UseRemoteInterface(DataBool set)
{
	AfxGetApplicationContext()->SetRemoteInterface(set);
}



//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Effect connection to database
///</summary>
//---------------------------------------------------------------------------
DataBool Login (DataStr/*[ciString]*/ authenticationToken)
{
	USES_UNATTENDED_DIAGNOSTIC();

	return AfxGetTbCmdManager()->Login(authenticationToken.GetString());			
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Get numeric ID of process in progress
///</summary>
//---------------------------------------------------------------------------
DataLng GetProcessID()
{
	return AfxGetTbCmdManager()->GetProcessID();
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Kill the thread
///</summary>
//---------------------------------------------------------------------------
void KillThread(DataLng ThreadId)
{
	DWORD dwThread = (DWORD)(long)ThreadId;
	if (!::PostThreadMessage(dwThread, WM_QUIT, 0, NULL))
		return; //il thread non esiste, tutto OK (fingo che sia stoppato)

	BOOL bExited = TRUE;
	HANDLE hThread = OpenThread(SYNCHRONIZE, FALSE, dwThread);
	if (hThread)
	{
		//waits 15 seconds, than fails! (perhaps, thread has to be killed)
		bExited = (WAIT_OBJECT_0 == WaitForSingleObject(hThread, 15000));
		CloseHandle(hThread);
	}

	return;
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Stop the thread
///</summary>
//---------------------------------------------------------------------------
DataBool StopThread(DataLng ThreadId)
{
	DWORD dwThread = (DWORD)(long)ThreadId;
	if (!::PostThreadMessage(dwThread, UM_STOP_THREAD, 0, NULL))
		return TRUE; //il thread non esiste, tutto OK (fingo che sia stoppato)

	BOOL bExited = TRUE;
	HANDLE hThread = OpenThread(SYNCHRONIZE, FALSE, dwThread);
	if (hThread)
	{
		//waits 15 seconds, than fails! (perhaps, thread has to be killed)
		bExited = (WAIT_OBJECT_0 == WaitForSingleObject(hThread, 15000));
		CloseHandle(hThread);
	}

	return bExited;
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///return true if thread can be stopped in safe mode
///</summary>
//---------------------------------------------------------------------------
DataBool CanStopThread(DataLng ThreadId)
{
	return   AfxInvokeThreadGlobalFunction<BOOL>((DWORD)(long)ThreadId, &::AfxCanStopThread);
}
//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Get active threads information (all threads of the application)
///</summary>
//---------------------------------------------------------------------------
DataStr GetActiveThreads()
{
	return AfxGetApplicationContext()->GetThreadInfos();
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Get active threads information all threads of current login)
///</summary>
//---------------------------------------------------------------------------
DataStr GetLoginActiveThreads()
{
	return AfxGetLoginContext()->GetThreadInfos();
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Get the list of thread identifiers for current login
///</summary>
//---------------------------------------------------------------------------
DataArray/*long*/ GetDocumentThreads()
{
	DataArray arInfos;
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
	{
		CArray<long, long> arThreads;
		pContext->GetDocumentThreads(arThreads);

		for (int i = 0; i < arThreads.GetCount(); i++)
			arInfos.Add(new DataLng(arThreads[i]));
	}
	return arInfos;
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Transmit to Tb the menu handle 
///</summary>
//---------------------------------------------------------------------------
void SetMenuHandle (DataLng menuWindowHandle)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext) //se ho fatto la login, imposto la poroprieta` a livello di login context
		pContext->SetMenuWindowHandle((HWND)(long)menuWindowHandle);
	else //altrimenti a livello globale (utile per avere feedback dal processo di login)
		AfxGetApplicationContext()->SetMenuWindowHandle((HWND)(long)menuWindowHandle);
}


//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Puts the document's frame into the foreground 
///</summary>
//---------------------------------------------------------------------------
DataBool SetDocumentInForeground(DataLng documentHandle)
{
	if (!AfxExistWebServiceStateObject((CObject*)(long)documentHandle))
		return FALSE;

	CBaseDocument* pDocument = (CBaseDocument*) (long)documentHandle;
	return pDocument->SetInForeground(); 
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Get user and company in use by this TB
///</summary>
//---------------------------------------------------------------------------
DataBool GetCurrentUser(DataStr&/*[ciString]*/ strUser, DataStr&/*[ciString]*/ strCompany)
{
	CString u, c;

	if (!AfxGetTbCmdManager()->GetCurrentUser(u, c))
		return FALSE;

	strUser = u;
	strCompany = c;

	return TRUE;
}

//---------------------------------------------------------------------------
void AssignParameters(CBaseDocument* pDocument, const DataStr& arguments)
{ 
	BOOL bOk (FALSE); 
	if (arguments[0] == '<')	//multi tag allowed: <Arguments, Parameters, Function, etc
	{
		CFunctionDescription fd;
		bOk = fd.ParseArguments(arguments);
		if (bOk)
			pDocument->GoInBrowserMode (&fd);
	}
		if (!bOk)
			pDocument->GoInBrowserMode (arguments);
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Open a document
///</summary>
//---------------------------------------------------------------------------
DataLng RunDocument(DataStr/*[ciString]*/ documentNamespace, DataStr arguments)
{ 
	CBaseDocument* pDocument = AfxGetTbCmdManager()->RunDocument(documentNamespace.GetString());

	if (pDocument && !arguments.IsEmpty() && pDocument->IsADataEntry())
		AfxInvokeThreadGlobalProcedure<CBaseDocument*, const DataStr&>(pDocument->GetFrameHandle(), &AssignParameters, pDocument, arguments);
	
	return (long)pDocument;
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Open a report
///</summary>
//---------------------------------------------------------------------------
DataLng RunReport(DataStr/*[ciString]*/ reportNamespace, DataStr arguments)
{ 
	CTBWinThread* pThread = AfxGetTBThread();
	BOOL isDocumentThread =  pThread && pThread->IsDocumentThread();
	//se non mi trovo nel document thread (normalmente è così: sono nel LoginThread)
	//ne creo uno e poi invoco la RunReport su quel thread
	if ( !isDocumentThread && AfxMultiThreadedDocument()) //multi threaded
	{
		//creo il thread
		CWinThread* pThread = AfxGetTbCmdManager()->CreateDocumentThread();
		//invoco me stesso nel thread di documenti
		return AfxInvokeThreadGlobalFunction
				<
				DataLng,
				DataStr,
				DataStr
				>
				(
				pThread->m_nThreadID,
				&::RunReport,	
				reportNamespace,
				arguments
				);
	}
	//----

	CWoormInfo* pFDI = new CWoormInfo();
	pFDI->m_ReportNames.Add(reportNamespace.GetString());
	if (!arguments.IsEmpty())
		pFDI->ParseArguments(arguments);

	CBaseDocument* pDocument = AfxGetTbCmdManager()->RunWoormReport(pFDI);
	//Se il report usciva per errore di parsing si aveva un crash perchè veniva deletato 2 volte il pFDI
	//if (!pDocument)	delete pFDI;

	return (long)pDocument; 
}

//-----------------------------------------------------------------------------
///<summary>
///Open a report using a WoormInfo object
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataLng RunReportFromWoormInfo(DataLng woormInfoHandle, DataStr arguments)
{
	long handle = (long)woormInfoHandle;
	
	if (!AfxExistWebServiceStateObject((CObject*)handle))
		return FALSE;

	CObject* pObj = ((CObject*)handle);

	if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
		return FALSE;

	if (!arguments.IsEmpty())
		((CWoormInfo*)pObj)->ParseArguments(arguments);

	CBaseDocument* pDocument = AfxGetTbCmdManager()->RunWoormReport((CWoormInfo*)pObj);
	return (long)pDocument; 
}


//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Run function
///</summary>
//---------------------------------------------------------------------------
DataBool RunFunction(DataStr/*[ciString]*/ functionNamespace, DataStr arguments)
{ 
	CFunctionDescription aFunction;
	if (!AfxGetTbCmdManager()->GetFunctionDescription((LPCTSTR)functionNamespace.GetString(), aFunction))
		return FALSE;

	if (!arguments.IsEmpty())
		aFunction.ParseArguments(arguments);

	return AfxGetTbCmdManager()->RunFunction(&aFunction);
}

//---------------------------------------------------------------------------
//ad uso e consumo della RunFunctionInNewThread
void RunFunctionProcedure(DataStr functionNamespace, DataStr arguments)
{
	DataBool b = RunFunction(functionNamespace, arguments);
	//se la funzione non mi ha lasciato aperte finestre, chiudo il thread
	//altrimenti lo chiudera poi la finestra in uscita
	if (AfxGetThreadContext()->GetThreadWindows().GetCount() == 0)
		AfxPostQuitMessage(b ? 0 : -1);
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Run function in a new thread
///</summary>
//---------------------------------------------------------------------------
void RunFunctionInNewThread(DataStr/*[ciString]*/ functionNamespace, DataStr arguments)
{ 
	//TODO PERASSO: la funzione dell'organizer deve avere una coda di messaggi managed,
	//altrimenti non funziona il drag&drop. Per ora cablo la funzione, in attesa di capire perche' non
	//sono compatibili gli oggetti c# con le code di messaggi c++
	//il problema prima o poi verra' risolto, allora capiremo cosa fare qui
	bool bManaged = TRUE == (functionNamespace == _T("OFM.Core.Components.ExecOpenOrganizer"));

	//creo il thread di documento
	CWinThread* pThread = AfxGetTbCmdManager()->CreateDocumentThread(bManaged);
	//dico al thread creato di eseguire la funzione ed esco
	AfxInvokeAsyncThreadGlobalProcedure<DataStr, DataStr>
		(
		pThread->m_nThreadID, 
		&RunFunctionProcedure, 
		functionNamespace, 
		arguments);
}
//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Run Manage file text application
///</summary>
//---------------------------------------------------------------------------
DataBool RunEditor(DataStr/*[ciString]*/ functionNamespace)
{ 
	return (AfxGetTbCmdManager()->RunEditor(functionNamespace.Str()) != NULL);
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Close document
///</summary>
//---------------------------------------------------------------------------
DataBool CloseDocument(DataLng documentHandle)
{ 
	CBaseDocument* pDocument = (CBaseDocument*) (long)documentHandle;
	if (!AfxExistWebServiceStateObject(pDocument))
		return FALSE;

	AfxGetTbCmdManager()->CloseDocument((const CBaseDocument*)pDocument);

	return TRUE; 
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Run Iconized Document
///</summary>
//---------------------------------------------------------------------------
DataBool RunIconizedDocument(DataLng documentHandle)
{
	CBaseDocument* pDocument = (CBaseDocument*)(long)documentHandle;
	if (!AfxExistWebServiceStateObject(pDocument))
		return FALSE;

	pDocument->PostMessage(WM_SYSCOMMAND, SC_MINIMIZE, 0);
	return TRUE;
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Close document
///</summary>
//---------------------------------------------------------------------------
DataBool CanCloseDocument(DataLng documentHandle)
{ 
	CBaseDocument* pDocument = (CBaseDocument*) (long)documentHandle;
	if (!AfxExistWebServiceStateObject(pDocument))
		return FALSE;

	return AfxGetTbCmdManager()->CanCloseDocument(pDocument);
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Verify document existence
///</summary>
//---------------------------------------------------------------------------
DataBool ExistDocument(DataLng documentHandle)
{ 
	CBaseDocument* pDocument = (CBaseDocument*) (long)documentHandle;
	return AfxExistWebServiceStateObject(pDocument);
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Destroy document
///</summary>
//---------------------------------------------------------------------------
DataBool DestroyDocument(DataLng documentHandle)
{ 
	const CBaseDocument* pDocument = (CBaseDocument*) (long)documentHandle;

	return AfxGetTbCmdManager()->DestroyDocument(pDocument);
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Close all docuements
///</summary>
//---------------------------------------------------------------------------
DataBool CloseAllDocuments()
{ 
	return AfxGetLoginContext()->CloseAllDocuments();
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Return ID list of all open documents
///</summary>
//---------------------------------------------------------------------------
DataObjArray/*[long]*/ GetOpenDocuments() 
{ 
	LongArray handles;
	AfxGetWebServiceStateObjectsHandles(handles, RUNTIME_CLASS(CBaseDocument));
	
	DataObjArray ar;
	ar.SetOwns(TRUE);
	for (int i = 0; i < handles.GetCount(); i++)
		ar.Add(new DataLng(handles[i]));
	return ar;
}
//---------------------------------------------------------------------------
void ExtractMessages(CDiagnostic* pDiagnostic, DataArray& messages, DataArray& types)
{
	CObArray arMessages;
	pDiagnostic->ToArray(arMessages);
	for (int i = 0; i <= arMessages.GetUpperBound(); i++)
	{
		CDiagnosticItem* pItem = (CDiagnosticItem*) arMessages.GetAt(i);
		types.Add(new DataInt(pItem->GetType()));
		CString sMessage = pItem->GetMessageText();
		messages.Add(new DataStr(sMessage));
	}
}
//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Return application context messages list
///</summary>
//---------------------------------------------------------------------------
void GetApplicationContextMessages(DataBool clearMessages, DataArray&/*[string]*/ messages, DataArray&/*integer*/ types)
{
	CDiagnostic* pDiagnostic = AfxInvokeThreadGlobalFunction<CDiagnostic*, BOOL>(AfxGetApp()->m_nThreadID, &CloneDiagnostic, clearMessages);
	ExtractMessages(pDiagnostic, messages, types);
	delete pDiagnostic;
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Return login context messages list
///</summary>
void GetLoginContextMessages(DataBool clearMessages, DataArray&/*[string]*/ messages, DataArray&/*integer*/ types)
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (!pContext)
		return;

	CDiagnostic* pDiagnostic = AfxInvokeThreadGlobalFunction<CDiagnostic*, BOOL>(pContext->m_nThreadID, &CloneDiagnostic, clearMessages);
	ExtractMessages(pDiagnostic, messages, types);
	delete pDiagnostic;
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///It allows to make actions before closing Task Builder
///</summary>
//---------------------------------------------------------------------------
DataBool OnBeforeCanCloseTB()
{
	FunctionDataInterface aParams;
	BOOL bOk = AfxGetTbCmdManager()->FireEvent(szOnBeforeCanCloseTB, &aParams);
	DataObj* pDataObj = aParams.GetReturnValue();
	if (pDataObj->GetDataType() == DATA_BOOL_TYPE)
		return *((DataBool*) pDataObj);
	
	return bOk;
}


//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Verify if it is possible to close Task Builder
///</summary>
//---------------------------------------------------------------------------
DataBool CanCloseTB	()
{
	return AfxGetTbCmdManager()->CanCloseTB(); 
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Verify if it is possible to close Login
///</summary>
//---------------------------------------------------------------------------
DataBool CanCloseLogin	()
{
	CLoginContext *pContext = AfxGetLoginContext();
	if (!pContext) return TRUE;

	return pContext->CanClose();
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Close current Login context
///</summary>
//---------------------------------------------------------------------------
void CloseLogin	()
{
	CLoginContext *pContext = AfxGetLoginContext();
	if (!pContext) return;

	pContext->Close();
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Verify current login context existence
///</summary>
//---------------------------------------------------------------------------
DataBool IsLoginValid()
{
	return AfxGetLoginContext() != NULL;
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Close Task Builder
///</summary>
//---------------------------------------------------------------------------
void CloseTB ()
{
	AfxGetTbCmdManager()->CloseTB(); 
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Close Task Builder destroying all open documents
///</summary>
//---------------------------------------------------------------------------
void DestroyTB ()
{
	::PostThreadMessage(AfxGetApp()->m_nThreadID, WM_QUIT, 0, NULL);
}

//[TBWebMethod(defaultsecurityroles="aUnprotected Report Manager", woorm_method=false)]
///<summary>
///Open Manage Woorm Report application
///</summary>
//----------------------------------------------------------------------------
void AdminDocumentReports ()
{
	if (!AfxGetLoginInfos()->m_bAdmin)
	{
		AfxMessageBox(_TB("Warning, this option is available only for administrator."));
		return;
	}

	CAdminToolDocReportDlg* pDialog = new CAdminToolDocReportDlg();
	VERIFY(pDialog->DoModeless());
}

//[TBWebMethod(defaultsecurityroles="Configuration, aXtech Parameter Manager", woorm_method=false, securityhidden=true)]
///<summary>
///Open the document profiles management tool
///</summary>
//----------------------------------------------------------------------------
void AdminDocumentProfiles ()
{
	if (
		AfxIsActivated(TBEXT_APP, INTERACTIVE_FUNCTIONALITY) &&
		!AfxGetLoginInfos()->m_bAdmin
		)
	{
		AfxMessageBox(_TB("Warning, this option is available only for administrator."));
		return;
	}

	CAdminToolDocProfileDlg* pDialog = new CAdminToolDocProfileDlg();
	VERIFY(pDialog->DoModeless());
}




//[TBWebMethod(woorm_method=false, defaultsecurityroles = "Configuration,aXtech Parameter Manager")]
///<summary>
///Open the document xml description management tool
///</summary>
//----------------------------------------------------------------------------
void XMLDocumentDescription()
{
	if (!AfxGetBaseApp()->IsDevelopment())
	{
		AfxMessageBox(_TB("Warning, this option is available only in a development activation."));
		return;
	}
	CDocDescrMngPage*  pDocDescri = new CDocDescrMngPage();
	VERIFY(pDocDescri->DoModeless());
}


//-----------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Return extraction query of HotLink
///</summary>
DataStr GetHotlinkQuery	(DataStr/*[ciString]*/ hotLinkNamespace, DataStr arguments, DataInt action)
{
	return AfxGetTbCmdManager()->GetHotlinkQuery(hotLinkNamespace.Str(), arguments.Str(), action);
}


//-----------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Return parameters for run batch in not interactive modality
///</summary>
DataBool GetDocumentParameters(DataStr/*[ciString]*/ documentNamespace, DataStr& xmlParams, DataStr code)
{
	CExternalControllerInfo info;
	info.m_ControllingMode = CExternalControllerInfo::EDITING;
	info.m_code = code;
	if (!xmlParams.IsEmpty())
		info.SetFromXmlString(xmlParams.GetString());

	const CBaseDocument* pBaseDoc = AfxGetTbCmdManager()->RunDocument
														(
															documentNamespace.GetString(), 
															szDefaultViewMode, 
															FALSE, 
															NULL, 
															NULL, 
															NULL, 
															NULL, 
															&info
														);

	if (!pBaseDoc) 
		return FALSE;

	if (!pBaseDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		AfxGetTbCmdManager()->CloseDocument(pBaseDoc);
		return FALSE;
	}

	CAbstractFormDoc *pDocument = (CAbstractFormDoc*)pBaseDoc;
	if (pDocument->GetType() != VMT_BATCH)
	{
		AfxGetTbCmdManager()->CloseDocument(pDocument);
		return FALSE;
	}
	
	// imposta correttamente i bottoni della toolbar (se necessario, fa il dispatch sul thread del documento)
	if (pDocument->InvokeRequired())
		AfxInvokeThreadProcedure<CAbstractFormDoc>(pDocument->GetThreadId(), pDocument, &CAbstractFormDoc::SetBatchButtonState);
	else
		pDocument->SetBatchButtonState(); 
	
	info.WaitUntilFinished();
	
	info.m_ControllingMode = CExternalControllerInfo::NONE; 
	// quindi lo chiudo
	AfxGetTbCmdManager()->CloseDocument(pBaseDoc);

	if (info.m_RunningStatus != CExternalControllerInfo::TASK_SAVE_PARAMS)
		return FALSE;

	xmlParams = info.GetXmlString();
		
	return TRUE;
}

//-----------------------------------------------------------------------------
///<summary>
///Return parameters for run report in not interactive modality
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool GetReportParameters(DataStr/*[ciString]*/ reportNamespace, DataStr& xmlParams, DataStr/*[ciString]*/ code)
{
	CExternalControllerInfo info;
	info.m_ControllingMode = CExternalControllerInfo::EDITING;
	info.m_code = code;

	if (!xmlParams.IsEmpty())
		info.SetFromXmlString(xmlParams.GetString()); 

	CWoormInfo *pInfo = new CWoormInfo();
	pInfo->m_ReportNames.Add(reportNamespace.GetString());
	pInfo->m_bHideFrame = TRUE; //Questo flag implica che il documento si chiuda in automatico al termine dell'esecuzione
	
	const CBaseDocument* pBaseDoc = AfxGetTbCmdManager()->RunWoormReport(pInfo, NULL, &info);

	if (!pBaseDoc) 
		return FALSE;

	if (!pBaseDoc->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
	{
		AfxGetTbCmdManager()->CloseDocument(pBaseDoc);
		return FALSE;
	}

	CWoormDoc *pDocument = (CWoormDoc*)pBaseDoc;

	info.WaitUntilFinished();

	// mi stacco dal documento
	info.m_ControllingMode = CExternalControllerInfo::NONE;

	if (info.m_RunningStatus != CExternalControllerInfo::TASK_SAVE_PARAMS)
	{
		xmlParams.Clear();
		return FALSE;
	}

	xmlParams = info.GetXmlString();
		
	return TRUE;
}

//-----------------------------------------------------------------------------
//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Run bacth in not interactive modality
///</summary>
DataBool RunBatchInUnattendedMode(DataStr/*[ciString]*/ documentNamespace, DataStr xmlParams, DataLng& documentHandle, DataObjArray&/*[string]*/ messages )
{
	CExternalControllerInfo info;
	info.SetFromXmlString(xmlParams.GetString());
	info.m_ControllingMode = CExternalControllerInfo::RUNNING;

	AfxGetDiagnostic()->StartSession();
	CBaseDocument* pBaseDoc = AfxGetTbCmdManager()->RunDocument
														(
															documentNamespace.GetString(), 
															szDefaultViewMode, 
															FALSE, 
															NULL, 
															NULL, 
															NULL, 
															NULL, 
															&info
														);
	AfxGetDiagnostic()->EndSession();
	
	if (!pBaseDoc)
	{
		CStringArray arMessages;
		AfxGetDiagnostic()->ToStringArray(arMessages);
		((DataStrArray&) messages) = arMessages;
		messages.Add(new DataStr(Messages::INVALID_DOCUMENT()));
		return FALSE;
	}

	if (!pBaseDoc->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
	{
		messages.Add(new DataStr(Messages::INVALID_DOCUMENT()));
		// mi stacco dal documento
		info.m_ControllingMode = CExternalControllerInfo::NONE;
		pBaseDoc->OnCloseDocument();
		return FALSE;
	}
	
	CAbstractFormDoc* pDocument = (CAbstractFormDoc*)pBaseDoc;
	if (pDocument->GetType() != VMT_BATCH)
	{
		messages.Add(new DataStr(Messages::INVALID_DOCUMENT()));
		// mi stacco dal documento
		info.m_ControllingMode = CExternalControllerInfo::NONE;
		pBaseDoc->OnCloseDocument();
		return FALSE;
	}
		
	documentHandle = (long)pBaseDoc; 
	//disabilita la messaggistica

	// avvia la batch (se necessario, fa il dispatch sul thread del documento)
	if (pDocument->InvokeRequired())
		AfxInvokeThreadProcedure<CAbstractFormDoc>(pDocument->GetThreadId(), pDocument, &CAbstractFormDoc::ExecuteBatchFromExternalController);
	else
		pDocument->ExecuteBatchFromExternalController();

	info.WaitUntilFinished();

	//Scheduler aScheduler;
	//// aspetta che termini il report di documento
	//for (int i = 0; i < pDocument->m_arReportDoc.GetSize(); i++)
	//{
	//	WoormDocPtr pReportDoc = pDocument->m_arReportDoc.GetAt(i);
	//	if (!pReportDoc)
	//		continue;
	//	while (pReportDoc)
	//		aScheduler.CheckMessage();
	//}
	pDocument->WaitReportEnd();

	// travaso i messaggi del documento
	pDocument->m_pMessages->ToArray(messages);
	
	// ripristino il documento nello stato interattivo
	pDocument->m_pExternalControllerInfo = NULL;
	
	if (info.m_RunningStatus != CExternalControllerInfo::TASK_SUCCESS &&
		info.m_RunningStatus != CExternalControllerInfo::TASK_SUCCESS_WITH_INFO)
		return FALSE;
		
	return TRUE;
}

//-----------------------------------------------------------------------------
///<summary>
///Run report in not interactive modality
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool RunReportInUnattendedMode(DataLng woormInfoHandle, DataStr xmlParams, DataLng& reportHandle, DataObjArray&/*[string]*/ messages)
{
	long handle = (long)woormInfoHandle;

	if (!AfxExistWebServiceStateObject((CObject*)handle))
	{
		messages.Add(new DataStr(Messages::INVALID_WOORM_INFO()));
		return FALSE;
	}

	CObject* pObj = ((CObject*)handle);

	if (!pObj->IsKindOf(RUNTIME_CLASS(CWoormInfo)))
	{
		messages.Add(new DataStr(Messages::INVALID_WOORM_INFO()));
		return FALSE;
	}

	CExternalControllerInfo info;
	info.m_ControllingMode = CExternalControllerInfo::RUNNING;
	if (!xmlParams.IsEmpty())
		info.SetFromXmlString(xmlParams.GetString());

	AfxGetDiagnostic()->StartSession();
	CBaseDocument* pBaseDoc = AfxGetTbCmdManager()->RunWoormReport((CWoormInfo*)pObj, NULL, &info);
	AfxGetDiagnostic()->EndSession();

	if (!pBaseDoc) 
	{
		CStringArray arMessages;
		AfxGetDiagnostic()->ToStringArray(arMessages);
		((DataStrArray&) messages) = arMessages;
		messages.Add(new DataStr(Messages::INVALID_DOCUMENT()));
		return FALSE;
	}

	if (!pBaseDoc->IsKindOf(RUNTIME_CLASS(CWoormDoc)))
	{
		pBaseDoc->OnCloseDocument();
		messages.Add(new DataStr(Messages::INVALID_DOCUMENT()));
		return FALSE;
	}

	CWoormDoc *pDocument = (CWoormDoc*)pBaseDoc;
	
	//disabilita la messaggistica
	pDocument->m_pMessages->StartSession();

	info.WaitUntilFinished();

	// travaso i messaggi del documento
	pDocument->m_pMessages->ToArray(messages);

	//ripristina la messaggistica
	pDocument->m_pMessages->EndSession();

	reportHandle = (long)pDocument;

	// ripristino il documento nello stato interattivo
	pDocument->m_pExternalControllerInfo = NULL;

	return (info.m_RunningStatus == CExternalControllerInfo::TASK_SUCCESS) ||
		(info.m_RunningStatus == CExternalControllerInfo::TASK_SUCCESS_WITH_INFO);
}


//-----------------------------------------------------------------------------
///<summary>
///Return application date
///</summary>
//[TBWebMethod(securityhidden=true)]
DataDate GetApplicationDate()
{
	return AfxGetApplicationDate();
}

//-----------------------------------------------------------------------------
///<summary>
///Return year of application date
///</summary>
//[TBWebMethod(securityhidden=true)]
DataInt GetApplicationYear()
{
	return AfxGetApplicationYear();
}

//-----------------------------------------------------------------------------
///<summary>
///Return month of application date
///</summary>
//[TBWebMethod(securityhidden=true)]
DataInt GetApplicationMonth()
{
	return AfxGetApplicationMonth();
}

//-----------------------------------------------------------------------------
///<summary>
///Return day of application date
///</summary>
//[TBWebMethod(securityhidden=true)]
DataInt GetApplicationDay()
{
	return AfxGetApplicationDay();
}

//-----------------------------------------------------------------------------
///<summary>
/// Enables or disables control over SOAP function calls; SOAP function calls are normally restricted when TBLoader is in some particular states (i.e. modal state)
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
DataBool EnableSoapFunctionExecutionControl(DataBool enable)
{
	return EnableSoapExecutionControl((BOOL)enable == TRUE);
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
///Verify if possible change Task Builder login
///</summary>
//---------------------------------------------------------------------------
DataBool CanChangeLogin	(DataBool bLock)
{
	return AfxGetTbCmdManager()->CanChangeLogin(bLock); 
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// Changes Task Builder Login
///</summary>
//---------------------------------------------------------------------------
DataInt ChangeLogin (DataStr/*[ciString]*/ oldAuthenticationToken, DataStr/*[ciString]*/ newAuthenticationToken, DataBool bUnLock)
{
	USES_UNATTENDED_DIAGNOSTIC();
	return AfxGetTbCmdManager()->ChangeLogin
		(
			oldAuthenticationToken.GetString(), 
			newAuthenticationToken.GetString(), 
			bUnLock
		); 
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// if Task Builder functionalities are locked
///</summary>
//---------------------------------------------------------------------------
DataBool IsTBLocked	 ()
{
	return AfxGetTbCmdManager()->IsTBLocked ();
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// Lock Task Builder functionalities
///</summary>
//---------------------------------------------------------------------------
DataBool LockTB	(DataStr/*[ciString]*/ authenticationToken)
{
	return AfxGetTbCmdManager()->LockTB (authenticationToken.Str());
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// UnLock Task Builder functionalities
///</summary>
//---------------------------------------------------------------------------
DataBool UnLockTB (DataStr/*[ciString]*/ authenticationToken)
{
	return AfxGetTbCmdManager()->UnLockTB (authenticationToken.Str());
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// Clears cached objects from client folder
///</summary>
//---------------------------------------------------------------------------
void ClearCache ()
{
	CStringLoader *pLoader = AfxGetStringLoader();
	if (pLoader)
		pLoader->FreeModules();
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// disconnect the current company
///</summary>
//---------------------------------------------------------------------------
DataInt DisconnectCompany (DataStr authenticationToken)
{
	AfxGetLoginContext()->DestroyAllDocuments();

	return AfxGetTbCmdManager()->DisconnectCompany (authenticationToken.Str());
}

//[TBWebMethod(securityhidden=true, woorm_method=false)]
///<summary>
/// reconnect the current company 
///</summary>
//---------------------------------------------------------------------------
DataInt ReconnectCompany (DataStr authenticationToken)
{
	return AfxGetTbCmdManager()->ReconnectCompany (authenticationToken.Str());
}
