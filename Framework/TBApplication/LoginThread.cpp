// LoginThread.cpp : implementation file
//

#include "stdafx.h"

#include <TBNameSolver\Templates.h>
#include <TBNameSolver\LoginContext.h>
#include <TBNameSolver\ThreadContext.h>
#include <TBNameSolver\ThreadInfo.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbWebServicesWrappers\LockManagerInterface.h>
#include <TbWebServicesWrappers\TbServicesWrapper.h>
#include <TbClientCore\ClientObjects.h>
#include <TbClientCore\ServerConnectionInfo.h>

#include <TbGeneric\NumberToLiteral.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\FormatsHelpers.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\SettingsTable.h>
#include <TBGeneric\StatusBarMessages.h>
#include <TbGeneric\OutdateObjectsInfo.h>
#include <TbGeneric\VisualStylesXp.h>
#include <TbGeneric\dib.h>


#include <TbParser\FontsParser.h>
#include <TbParser\FormatsParser.h>
#include <TbParser\XmlSettingsParser.h>
#include <TbParser\XmlNumberToLiteralParser.h>

#include <TbGenlib\Generic.h>
#include <TbGenlib\TbGenlibSettings.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\DiagnosticManager.h>
#include <TbGeneric\TBThemeManager.h>

#include <TbGenLibManaged\Main.h>
#include <TbGenLibManaged\MenuFunctions.h>

#include <TbOleDb\OleDbMng.h>
#include <TbGes\DocRecoveryManager.h>

#include "LoginThread.h"
#include "ApplicationsLoader.h"
#include "TaskBuilderApp.h"
#include "DocumentThread.h"
#include "ThreadMainWindow.h"

#define CHECK_TOKEN_TIMER_INTERVAL 300000
#define CHECK_TOKEN_TIMER_INTERVAL_SHORT 30000
// CLoginThread

IMPLEMENT_DYNCREATE(CLoginThread, CLoginContext)


//-----------------------------------------------------------------------------
CLoginThread::CLoginThread()
	:
	m_bProxy(false), 
	m_nTimeoutTimer(0),
	m_nCheckTokenTimer(0)
{
}

//-----------------------------------------------------------------------------
CLoginThread::~CLoginThread()
{
	if (m_bProxy) //siccome non è un vero thread, devo forzare le operazioni di pulizia
	{
		m_hThread = NULL;      
		m_nThreadID = 0; 
		ExitInstance();
	}

}

//-----------------------------------------------------------------------------
void CLoginThread::WaitForStartupReady()
{
	LoopUntil(&m_StartupReady);
}
//-----------------------------------------------------------------------------
BOOL CLoginThread::InitInstance()
{
	__try
	{
		return InitInstanceInternal();
	}
	__except (ExpFilter(GetExceptionInformation(), GetExceptionCode()))
	{
		AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)		
	}
	return FALSE;
}
//-----------------------------------------------------------------------------
BOOL CLoginThread::InitInstanceInternal()
{
	CThreadContext* pThread = AfxGetThreadContext();
	//in fase di partenza non visualizzo message box, perché sono causa di deadlock
	UserInteractionMode mode = pThread->GetUserInteractionMode();
	pThread->SetUserInteractionMode(UNATTENDED);
	//CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
	if (!__super::InitInstance())
		return FALSE;

	m_pMainWnd = new CThreadMainWindow(!m_bProxy); //dummy main window (MFC needs a thread main window)

#ifndef DEBUG //in debug voglio che si schianti per capire l'errore
	try
#endif
	{
		InitLoginContext();
		InitThreadContext();
		
		AfxGetDiagnostic()->StartSession();
		if (Login())
			PostLoginInitializations();

		m_bValid = m_bValid && !AfxGetDiagnostic()->ErrorFound(); 
		AfxGetDiagnostic()->EndSession();

		AfxGetDiagnostic()->AttachViewer(AfxCreateDefaultViewer());
		AfxGetDiagnostic()->EnableTraceInEventViewer(IsEventViewerTraceEnabled());
	}
#ifndef DEBUG
	catch (CException* pException)
	{
		AfxGetDiagnostic()->Add(pException); //null pointer accepted
		if (pException)
			pException->Delete();
	}
#endif

	//ogni x minuti minuto controllo lo stato di attivazione
	m_nCheckTokenTimer = ::SetTimer(NULL, m_nCheckTokenTimer, CHECK_TOKEN_TIMER_INTERVAL, NULL);

	m_StartupReady.Set();
	if (!m_bProxy)
		SetThreadPriority(THREAD_PRIORITY_LOWEST);

	pThread->SetUserInteractionMode(mode);
	return TRUE;
}

//--------------------------------------------------------------------------------
void CLoginThread::StartTimeoutTimer(UINT nTimeoutMilliseconds)
{
	m_nTimeoutTimer = ::SetTimer(NULL, m_nTimeoutTimer, nTimeoutMilliseconds, NULL);
}

//--------------------------------------------------------------------------------
CDocument* CLoginThread::OpenDocumentOnCurrentThread (
	const CSingleDocTemplate* pTemplate, 
	LPCTSTR pInfo, 
	BOOL bMakeVisible /* = TRUE*/)
{
	ASSERT_KINDOF(CSingleExtDocTemplate, pTemplate);
	CSingleExtDocTemplate *pNewTemplate = new CSingleExtDocTemplate((CSingleExtDocTemplate*)pTemplate, (DocInvocationParams*)pInfo);
	return pNewTemplate->OpenDocumentFile(pInfo, bMakeVisible);//il documento farà poi la delete
}
//----------------------------------------------------------------------------
void CLoginThread::Close()
{
	if (m_bProxy)
	{
		delete this;
		AfxGetThreadContext()->SetLoginContext(_T(""));
		AfxGetThreadContext()->ClearProxyObjectMap();
	}
	else
	{
		AfxGetApplicationContext()->CloseLoginThread(m_nThreadID, m_hThread); 
	}
	
}

//-----------------------------------------------------------------------------
void CLoginThread::PostLoginInitializations()
{
	const CLoginInfos *pInfos = AfxGetLoginInfos();
	if (!m_bProxy)
	{
		USES_CONVERSION;
		SetThreadName(T2A(cwsprintf(_T("%s - %s"), pInfos->m_strUserName, pInfos->m_strCompanyName)));
	}
	CStatusBarMsg statusBar(TRUE, TRUE, FALSE); // disabilita, hourglass

	CApplicationsLoader aLoader;
	if (!aLoader.IntergrateLoginDefinitions (&statusBar))
		m_bValid = FALSE;

	// Data Caching needs Settings Loaded to initialize
	InitDataCachingManager  ();

	LoadTaskBuilderParameters ();

	// updates default font (setting dependent)
	AfxGetWritableFontStyleTable()->LoadApplicationCulture(pInfos->m_strPreferredLanguage);

	CString strThemeFullPath = GetUserPreferredThemePath(AfxGetLoginContext(), AfxIsActivated(MAGONET_APP, _NS_ACT("IMago")));
	ChangeTheme(strThemeFullPath, AfxGetLoginContext());
	
	// disabilito il controllo sull'esecuzione della funzione (il metodo CanExecute controlla m_bCanRun che è posto a false)
	bool bOldValue = EnableSoapExecutionControl(false);
	if (pInfos->m_bSecurity && AfxIsActivated(szExtensionsApp, TBSECURITY_ACT))
		if (!AfxGetTbCmdManager()->RunFunction(_NS_WEB("Extensions.TbSecurity.TbSecurity.StartSecurity"), NULL))
		{
			AfxGetDiagnostic()->Add(_TB("The Security extension module is not loaded."), CDiagnostic::Error);
			m_bValid = FALSE;
		}

	if (pInfos->m_bAuditing && AfxIsActivated(szExtensionsApp, TBAUDITING_ACT))
	{
		if (!AfxGetTbCmdManager()->RunFunction(_NS_WEB("Extensions.TbAuditing.TbAuditing.OpenAuditing"), NULL))
		{
			AfxGetDiagnostic()->Add(_TB("The Auditing extension module is not loaded."), CDiagnostic::Error);
			m_bValid = FALSE;
		}
		else
		{
			//creates Auditingmanager for tables that have already been registered
			AfxGetDefaultSqlConnection()->RefreshTraces();
		}
	} 

	if (m_bValid) 
		FireOnDSNChanged();

	// ripristino il controllo sull'esecuzione della funzione
	EnableSoapExecutionControl(bOldValue);
}

//-----------------------------------------------------------------------------
void CLoginThread::FireOnDSNChanged()
{
	AfxGetTbCmdManager()->FireEvent(szOnDSNChanged, NULL);
}
	
//-----------------------------------------------------------------------------
void CLoginThread::GetDocumentThreadArray(CDWordArray& arThreadIds)
{
	arThreadIds.RemoveAll();
	TB_OBJECT_LOCK_FOR_READ(&m_DocumentThreads);
	POSITION pos = m_DocumentThreads.GetStartPosition();
	while (pos)
	{
		DWORD key = 0;
		CTBWinThread* pValue = NULL;
		m_DocumentThreads.GetNextAssoc(pos, key, pValue);
		arThreadIds.Add(key);
	}
}
//----------------------------------------------------------------------------
CThreadInfo* CLoginThread::AddThreadInfos(CThreadInfoArray& arInfos)
{	
	CThreadInfo* pInfo = __super::AddThreadInfos(arInfos);

	pInfo->SetCompany(m_LoginInfos.m_strCompanyName);
	pInfo->SetUser(m_LoginInfos.m_strUserName);
	pInfo->SetOperationDate(AfxGetApplicationDate().Str(FALSE));

	CDWordArray arThreads;

	BEGIN_TB_OBJECT_LOCK_FOR_READ(&m_DocumentThreads);
	POSITION pos = m_DocumentThreads.GetStartPosition();
	while (pos)
	{
		CTBWinThread* pThread = NULL;
		DWORD dwThreadId = 0;
		m_DocumentThreads.GetNextAssoc(pos, dwThreadId, pThread);
		arThreads.Add(dwThreadId);
		AfxInvokeAsyncThreadFunction<CThreadInfo*, CTBWinThread, CThreadInfoArray&>(dwThreadId, pThread, &CTBWinThread::AddThreadInfos, pInfo->m_arThreadInfos);
	}
	END_TB_OBJECT_LOCK();
	
	//aspetto che tutti i thread riempiano la struttura, scremando quelli che nel frattempo sono morti
	while (arThreads.GetCount() != pInfo->m_arThreadInfos.GetCount())
	{
		for (int i = arThreads.GetUpperBound(); i >=0; i--)
			if (!CTBWinThread::IsThreadAlive(arThreads[i]))
				arThreads.RemoveAt(i);

		PumpThreadMessages();
	}

	return pInfo;
}

// login cannot be LogOffed as it is caller duty
//-----------------------------------------------------------------------------
int CLoginThread::ExitInstance()
{
	int nResult = -1;
	__try
	{
		nResult = ExitInstanceInternal();
	}
	__except (ExpFilter(GetExceptionInformation(), GetExceptionCode()))
	{
		AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)		
	}

	return nResult;
}
//-----------------------------------------------------------------------------
int CLoginThread::ExitInstanceInternal()
{
#ifndef DEBUG
	try
#endif
	{
		USES_DIAGNOSTIC();
		CTaskBuilderApp::AdjustClosingMessageViewer();

		if (m_nCheckTokenTimer)
			::KillTimer(NULL, m_nCheckTokenTimer);
		if (m_nTimeoutTimer)
			::KillTimer(NULL, m_nTimeoutTimer);

		CDWordArray arThreadIds;
		GetDocumentThreadArray(arThreadIds);

		DestroyAllDocuments();
		
		//closes all active documents and waits them to terminate
		for (int i = 0; i < arThreadIds.GetCount(); i++)
		{
			DWORD dwThread = arThreadIds.GetAt(i);
			HANDLE hThread = OpenThread(SYNCHRONIZE, FALSE, dwThread);
			while (hThread)
			{
				DWORD ret = WaitForSingleObject(hThread, 1000); //aspetto un secondo
				CloseHandle(hThread);
				if (ret == WAIT_OBJECT_0)
					break;
				//mando il messaggio di chiusura al thread
				::PostThreadMessage(dwThread, WM_QUIT, NULL, NULL);
				//e mi preparo a ripetere l'attesa
				hThread = OpenThread(SYNCHRONIZE, FALSE, dwThread);

			}
		}

		AfxGetThreadContext()->PerformExitinstanceOperations();

		//const CLoginInfos *pInfos = AfxGetLoginInfos();
		
		AfxGetApplicationContext()->RemoveLoginContext(this);

		AfxCloseEnumsViewer();
		m_pMainWnd->DestroyWindow();

		SetProcessWorkingSetSize(GetCurrentProcess(), -1, -1);
		int nRet = __super::ExitInstance();
		//CoUninitialize();
		return nRet;

	}
#ifndef DEBUG //when debugging it is useful if the program stops when the error occurs
	catch (CException* pException)
	{
		AfxGetDiagnostic()->Add(pException); //null pointer accepted
		if (pException)
			pException->Delete(); 
		return -1;
	}
#endif
}

//-----------------------------------------------------------------------------
void CLoginThread::InitLoginContext()
{
	AfxAttachThreadToLoginContext(m_strName);

	AttachSettingsTable(new SettingsTable(*((SettingsTable*) AfxGetApplicationContext()->GetGlobalSettingsTable())));
	
	AttachLockManager (AfxCreateLockManager());	
	AttachOleDbMng(new COleDbManager());	
	AttachCultureInfo(new CCultureInfo());
	AttachWebServiceStateObjects(new CWebServiceStateObjects());
	AttachSecurityInterface(new CBaseSecurityInterface());
	AttachFormatsTable(new FormatStyleTable(*AfxGetStandardFormatStyleTable()));
	AttachThemeManager(new TBThemeManager());
	AttachFontsTable(new FontStyleTable(*AfxGetStandardFontStyleTable()));
	AttachNTLLookUpTableManager(new CNumberToLiteralLookUpTableManager());
	AttachSqlRecoveryManager(new DocRecoveryManager());

	// Date initialization
	AfxSetApplicationDate(DataDate(TodayDate()));

}

//-----------------------------------------------------------------------------
BOOL CLoginThread::Login(const CString& sAuthenticationToken, BOOL bAsync)
{
	AfxSetStatusBarText(_TB("Starting login operations..."));;

	if (sAuthenticationToken.IsEmpty())
	{
		AfxGetDiagnostic()->Add(_TB("Authentication token is empty"));
		return FALSE;
	}
	m_strName = sAuthenticationToken;
	AfxGetApplicationContext()->AddLoginContext(this);

	if (AfxMultiThreadedLogin())//se sono in modalità multithread, creo il thread
	{
		if (!CreateThread())
			m_bValid = FALSE;
	}
	else //altrimenti chiamo direttamente la initinstance che mi inizializza gli oggetto della classe, che diventa un wrapper alternativo al main thread 
	{
		m_hThread = AfxGetThread()->m_hThread;      
		m_nThreadID = AfxGetThread()->m_nThreadID; 
		m_bProxy = true;
		if (!InitInstance())
			m_bValid = FALSE;
	}
	if (!bAsync)
		WaitForStartupReady();
	
	if (m_bValid)
		AfxSetStatusBarText(_TB("Login operations completed successfully!"));
	else
		AfxSetStatusBarText(_TB("Login operations aborted !"));

	return m_bValid;
}
//-----------------------------------------------------------------------------
BOOL CLoginThread::Login()
{
	if (m_strName.IsEmpty())
		return FALSE;

	CString aOldCulture = AfxGetCulture();

	//Inizializzo i dati di Login di TBLoader
	if (!AfxGetLoginManager()->InitLogin(m_strName))
	{
		m_bValid = FALSE;
		return FALSE;
	}
	
	const CLoginInfos *pInfos = AfxGetLoginInfos();
	// this object in the application context can be set to override the culture information
	// normally loaded from the user and/or company DB and/or local PC culture, etc.
	CultureOverride *pOvr = AfxGetApplicationContext()->GetObject<CultureOverride>(false);
 
	// aggiorno i dati relativi alla culture dell'utente
	CCultureInfo *pInfo = const_cast<CCultureInfo*>(AfxGetCultureInfo());
	pInfo->SetUICulture(pOvr ? pOvr->m_strPreferredLanguage : pInfos->m_strPreferredLanguage);

	AfxGetThreadContext()->SetUICulture(pOvr ? pOvr->m_strPreferredLanguage : pInfos->m_strPreferredLanguage);

	InitThreadCulture();

	CXmlNumberToLiteralParser numberToLiteralParser; 
	if (!numberToLiteralParser.LoadLookUpFile())//valorizza i membri di un puntatore nel login context
	{
		ASSERT(FALSE);
		TRACE("Error loading the numeric conversion lookup file.");
		AfxGetDiagnostic()->Add
					(
					_TB("Error loading the numeric conversion lookup file."),
					CDiagnostic::Warning
					);
	}

	pInfo->InitLocale (pOvr ? pOvr->m_strApplicationLanguage : pInfos->m_strApplicationLanguage);

	// I have to refresh database culture to manage collate sensitivity on the base of settings
	pInfo->SetCultureLCID (pInfos->m_wDataBaseCultureLCID);
 
	COleDbManager * pOleDbMng = AfxGetOleDbMng();
	pOleDbMng->OnDSNChanged();
	// if Database Connection is not valid, I don't validate Login
	if (!pOleDbMng->IsValid())
		m_bValid = FALSE;

	return m_bValid;
}
	
//-----------------------------------------------------------------------------
void CLoginThread::SetMenuWindowHandle(HWND handle)
{
	__super::SetMenuWindowHandle(handle);
	UpdateMenuTitle();
}

//-----------------------------------------------------------------------------
void CLoginThread::UpdateMenuTitle()
{
	CString strOldTitle;
	CString strTitle(AfxGetBaseApp()->GetAppTitle());

	strTitle += " ";
	//SqlConnection* pDefaultSqlConnection = AfxGetDefaultSqlConnection();
    if (AfxGetOleDbMng() && AfxGetOleDbMng()->IsValid())
		strTitle += cwsprintf(_TB(" - Company: {0-%s}"), AfxGetLoginInfos()->m_strCompanyName);
	else
 		strTitle += _TB("Datasource not connected");

	strTitle += cwsprintf(_TB("- User: {0-%s}"), AfxGetLoginInfos()->m_strUserName);
	strTitle += cwsprintf(_TB(" - Operation date: [{0-%s}]"), AfxGetApplicationDate().Str(1));


	AfxSetMenuWindowTitle(strTitle);
}

//-----------------------------------------------------------------------------
void CLoginThread::LoadTaskBuilderParameters ()
{
	DataInt aDefault (EPSILON_DECIMAL);
	
	// double
	DataObj* pDataObj = AfxGetSettingValue (snsTbGenlib, szDataTypeEpsilon, szDataDblEpsilon, aDefault, szTbDefaultSettingFileName);
	AfxGetLoginContext()->SetEpsilonPrecision
		(
			DATADBL_EPSILON_PRECISION_POS, 
			pDataObj && pDataObj->IsKindOf(RUNTIME_CLASS(DataInt)) ? *((DataInt*) pDataObj) : aDefault
		);
	
	// money
	pDataObj = AfxGetSettingValue (snsTbGenlib, szDataTypeEpsilon, szDataMonEpsilon, aDefault, szTbDefaultSettingFileName);
	AfxGetLoginContext()->SetEpsilonPrecision
		(
			DATAMON_EPSILON_PRECISION_POS, 
			pDataObj && pDataObj->IsKindOf(RUNTIME_CLASS(DataInt)) ? *((DataInt*) pDataObj) : aDefault
		);
	
	// percentage
	pDataObj = AfxGetSettingValue (snsTbGenlib, szDataTypeEpsilon, szDataPercEpsilon, aDefault, szTbDefaultSettingFileName);
	AfxGetLoginContext()->SetEpsilonPrecision
		(
			DATAPERC_EPSILON_PRECISION_POS, 
			pDataObj && pDataObj->IsKindOf(RUNTIME_CLASS(DataInt)) ? *((DataInt*) pDataObj) : aDefault
		);
	
	// quantity
	pDataObj = AfxGetSettingValue (snsTbGenlib, szDataTypeEpsilon, szDataQuantityEpsilon, aDefault, szTbDefaultSettingFileName);
	AfxGetLoginContext()->SetEpsilonPrecision
		(
			DATAQTY_EPSILON_PRECISION_POS,
			pDataObj && pDataObj->IsKindOf(RUNTIME_CLASS(DataInt)) ? *((DataInt*) pDataObj) : aDefault
		);

	AfxGetOleDbMng()->CacheParameters ();
}

//-----------------------------------------------------------------------------
void CLoginThread::OnCloseLoginAsync(WPARAM wParam, LPARAM lParam)
{
	//if thread is in modal state, this code is in the modal loop of a dialog
	//so i re-post the message and throw an exception so che messsage loop terminates
	if (AfxIsThreadInInnerLoop())
	{
		PostThreadMessage(UM_CLOSE_LOGIN_ASYNC, NULL, NULL);
		throw (new CThreadAbortedException());
	}

	AfxPostQuitMessage(0);
}

//--------------------------------------------------------------------------------
void CLoginThread::OnThreadTimer(WPARAM wParam, LPARAM lParam)
{
	if (m_nCheckTokenTimer == wParam)//check token timer
	{
		try
		{
			ValidateDate();

			TRACE("CHECKING AUTHENTICATION TOKEN...\r\n");
			if (!AfxGetLoginManager()->IsValidToken(m_strName, m_bValid))
			{
				m_nCheckTokenTimer = ::SetTimer(NULL, m_nCheckTokenTimer, CHECK_TOKEN_TIMER_INTERVAL_SHORT, NULL);
				TRACE("Failed to check authentication token");
			}
			else
			{
				m_nCheckTokenTimer = ::SetTimer(NULL, m_nCheckTokenTimer, CHECK_TOKEN_TIMER_INTERVAL, NULL);
			}
		}
		catch (...)
		{
			TRACE("Failed to check authentication token");
			ASSERT(FALSE);

		}
	}
	else if (m_nTimeoutTimer == wParam)//session timeout timer
	{
		AfxGetLoginManager()->LogOff();
		Close();
	}

}

//--------------------------------------------------------------------------------
void CLoginThread::ValidateDate()
{
	++m_nMluValidCounter;

		if( m_nMluValidCounter >= 75)
			m_nMluValidCounter = 0;

		//OGNI TOT verifico che la data operazioni sia permessa dallo stato dell'MLU
		//se mlu è scaduto e la data corrente è successiva a tale scadenza imposto il valore false in modo che sia vietato lanciare documenti
		//fino a che non si imposti una data di applicazione corretta.
		if( m_nMluValidCounter ==  1 || 
			m_nMluValidCounter ==  3 || 
			m_nMluValidCounter ==  6 || 
			m_nMluValidCounter ==  15 || 
			m_nMluValidCounter ==  25 || 
			m_nMluValidCounter ==  55  )
		{
			DataDate maxdate;
			m_bMluValid = AfxIsValidDate(AfxGetApplicationDate(), maxdate);
			if (!m_bMluValid)
			{
				DataDate dtExpire;
				dtExpire.SetTodayDate();
				dtExpire += 1;
				CString Caption (_TB("BEWARE!")); 
				CString Text (cwsprintf(_TB("Sadly your M.L.U. service has expired the {0-%s}! You can use Mago.net by setting an operation date prior to the expiry date. Restore the full efficiency of you Mago.net:"), maxdate.Str(1)));
				CString Link (AfxGetLoginManager()->GetBrandedKey(_T("MLURenewalLink"))); //(L"www.microarea.it");
				CString LinkDescription (_TB("Click here to find out how!"));
				const CLoginInfos* infos = AfxGetLoginContext()->GetLoginInfos();
				CStringArray arRecipients; arRecipients.Add(infos->m_strUserName);
				CString sBody (cwsprintf(CString(L"<html><body>"
											L"<table cellspacing='2' cellpadding='2' border='0' style='text-align: left; width: 385px;"
												L"height: 115px;'>"
												L"<tr>"
													L"<td>"
														L"<div style='text-align: center; color: rgb(89, 89, 89);'>"
															L"<span style='font-family: Verdana; font-weight: bold;'><span style='color: rgb(255, 0, 0)'>"
																L"{0-%s}"//caption
																L"<br />"
																L"<br />"
															L"</span></span><span style='font-family: Verdana;'><span style='font-size: x-small'>{1-%s}<br />"//text
																L"<br />"
															L"</span><a href='{2-%s}' target='_blank'>{3-%s}</a> </span>"//link, linkdescription
														L"</div>"
													L"</td>"
												L"</tr>"
											L"</table>"

				L"</body></html>"),Caption,Text,Link,LinkDescription));

				AfxGetLoginManager()->AdvancedSendBalloon
				(
					sBody,	
					dtExpire,
					CLoginManagerInterface::bt_Contract,
					arRecipients,
					CLoginManagerInterface::bs_Warning ,
					TRUE,
					TRUE,
					15000
				);

			::PostMessage(AfxGetMenuWindowHandle(), UM_IMMEDIATE_BALLOON, NULL, NULL); //CUtility::ShowImmediateBalloon(); non compila
			}
		}
}

//--------------------------------------------------------------------------------
void CLoginThread::OnStopThread(WPARAM, LPARAM)
{
	if (CloseAllDocuments())
		AfxPostQuitMessage(0);
}
//----------------------------------------------------------------------------
void CLoginThread::AddDiagnostic(CDiagnostic *pDiagnostic)
{
	AfxInvokeThreadProcedure<CLoginThread, CDiagnostic*>(m_pMainWnd->m_hWnd, this, &CLoginThread::AddDiagnosticInternal, pDiagnostic);
}
//----------------------------------------------------------------------------
void CLoginThread::AddDiagnosticInternal(CDiagnostic *pDiagnostic)
{
	AfxGetDiagnostic()->Add(pDiagnostic);
}
//----------------------------------------------------------------------------
int CLoginThread::GetOpenDocuments() 
{
	LongArray handles;
	AfxGetWebServiceStateObjectsHandles(handles, RUNTIME_CLASS(CBaseDocument));
	
	return handles.GetCount();
}

//----------------------------------------------------------------------------
int CLoginThread::GetOpenDocumentsInDesignMode() 
{
	LongArray handles;
	AfxGetWebServiceStateObjectsHandles(handles, RUNTIME_CLASS(CBaseDocument));
	int count = 0;
	for (int i = 0; i < handles.GetCount(); i++)
	{
		if (((CBaseDocument*)handles[i])->IsInDesignMode())
			count++;
	}
	return count;
}

//----------------------------------------------------------------------------
BOOL CLoginThread::CanClose()
{
	if (AfxGetThread() == this)
		return InternalCanClose();

	return AfxInvokeThreadFunction<BOOL, CLoginThread>(m_nThreadID, this, & CLoginThread::InternalCanClose);
}

//----------------------------------------------------------------------------
BOOL CLoginThread::CloseAllDocuments()
{
	BOOL bAllClosed = TRUE;

	LongArray arDocs;
	AfxGetWebServiceStateObjectsHandles(arDocs, RUNTIME_CLASS(CBaseDocument));
		
	const CBaseDocument* pDocToClose;
	// poi li chiude al contrario
	for (int i = arDocs.GetUpperBound(); i >= 0; i--)
	{
		pDocToClose = (CBaseDocument*) arDocs[i];
		if (!AfxGetTbCmdManager()->CloseDocument(pDocToClose))
			bAllClosed = FALSE;
	}

	return bAllClosed;
}

//----------------------------------------------------------------------------
BOOL CLoginThread::SilentCloseLoginDocuments()
{
	BOOL bAllClosed = TRUE;

	LongArray arDocs;
	AfxGetWebServiceStateObjectsHandles(arDocs, RUNTIME_CLASS(CBaseDocument));
		
	CBaseDocument* pDocToClose;
	// poi li chiude al contrario
	for (int i = arDocs.GetUpperBound(); i >= 0; i--)
	{
		pDocToClose = (CBaseDocument*) arDocs[i];
		if (AfxGetTbCmdManager()->CanCloseDocument(pDocToClose))
		{
			if (!AfxGetTbCmdManager()->CloseDocument((const CBaseDocument*) pDocToClose))
				bAllClosed = FALSE;
		}
		else 
			bAllClosed = FALSE;
	}
	return bAllClosed;
}

//----------------------------------------------------------------------------
void CLoginThread::DestroyAllDocuments()
{
	LongArray arDocs;
	AfxGetWebServiceStateObjectsHandles(arDocs, RUNTIME_CLASS(CBaseDocument));

	const CBaseDocument* pDocToClose;
	// poi li chiude al contrario
	for (int i = arDocs.GetUpperBound(); i >= 0; i--)
	{
		pDocToClose = (CBaseDocument*) arDocs[i];
		AfxGetTbCmdManager()->DestroyDocument(pDocToClose);
	}

}

//-----------------------------------------------------------------------------                      
void CLoginThread::SetTbDevParams(HANDLE hDevMode, HANDLE hDevNames)
{
	TB_LOCK_FOR_WRITE();
	FreePrinterDevParams(); 
	m_hTbDevMode = CopyHandle(hDevMode);
	m_hTbDevNames = CopyHandle(hDevNames);
}

//-----------------------------------------------------------------------------                      
void CLoginThread::GetTbDevParams (HANDLE& hDevMode, HANDLE& hDevNames)
{
	if (m_hTbDevMode == NULL && m_hTbDevNames == NULL)
	{
		TB_LOCK_FOR_WRITE();
		if (m_hTbDevMode == NULL && m_hTbDevNames == NULL)
		{
			CPrintDialog pd(TRUE);
			pd.GetDefaults();
			CString s = pd.GetDeviceName();
			SetTbDevParams(pd.m_pd.hDevMode, pd.m_pd.hDevNames);
		}
	}

	TB_LOCK_FOR_READ();
	hDevMode = CopyHandle(m_hTbDevMode);
	hDevNames = CopyHandle(m_hTbDevNames);
}
	
//-----------------------------------------------------------------------------
BOOL CLoginThread::InternalCanClose()
{
	USES_UNATTENDED_DIAGNOSTIC();

	FailedInvokeCode aError = InvkNoError;
	FunctionDataInterface aFDI;
	if (AfxGetTbCmdManager()->FireEvent(szOnQueryCanCloseApp, &aFDI, &aError)
		&& aError == InvkNoError)
	{
		DataObj* pRetVal = aFDI.GetReturnValue();
		if (pRetVal 
			&& pRetVal->GetDataType() == DataType::Bool
			&& !(*((DataBool*)pRetVal)))
			 return FALSE;
	}

	if (GetOpenDocuments() > 0)
		return FALSE;
	
	if (GetOpenDialogs() > 0)
		return FALSE;
	
	if (!AfxGetOleDbMng()->CanCloseAllConnections())
		return FALSE;

	return TRUE;
}

BEGIN_MESSAGE_MAP(CLoginThread, CTBWinThread)
	ON_THREAD_MESSAGE (UM_CLOSE_LOGIN_ASYNC,	OnCloseLoginAsync)
	ON_THREAD_MESSAGE(UM_STOP_THREAD, OnStopThread)
	ON_THREAD_MESSAGE(WM_TIMER, OnThreadTimer)
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CWnd* CLoginThread::GetMainWnd()
{
	CWnd* pWnd = AfxGetThreadContext()->GetMenuWindow();
	return pWnd ? pWnd : __super::GetMainWnd();
}


//-----------------------------------------------------------------------------
int CLoginThread::Run()
{
	__try
	{
		return RunInternal();
	}
	__except (ExpFilter(GetExceptionInformation(), GetExceptionCode()))
	{
		AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)		
	}
	return ExitInstance();
}

//-----------------------------------------------------------------------------
int CLoginThread::RunInternal()
{
#ifndef DEBUG
	try
#endif
	{
		return __super::Run();
	}
#ifndef DEBUG //when debugging it is useful if the program stops when the error occurs
	catch (CException* pException)
	{
		return ManageException(pException);
	}
#endif
}

//--------------------------------------------------------------------------------
BOOL CLoginThread::ManageException(CException* pException)
{
	AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)

	if (pException)
		pException->Delete();
	
	return ExitInstance();

}

//-----------------------------------------------------------------------------
void CLoginThread::InternalSetCanUseNamespace(const CString&	nameSpace, int grantType, BOOL bCanUse)
{
	TB_OBJECT_LOCK(&m_NamespaceMap);
	CString strKey;
	strKey.Format(_T("%s-%d"), nameSpace, grantType);
	m_NamespaceMap[strKey] = bCanUse;
}
//-----------------------------------------------------------------------------
BOOL CLoginThread::InternalCanUseNamespace(const CString&	nameSpace, int grantType, BOOL& bCanUse)
{
	TB_OBJECT_LOCK_FOR_READ(&m_NamespaceMap);
	CString strKey;
	strKey.Format(_T("%s-%d"), nameSpace, grantType);
	return m_NamespaceMap.Lookup(strKey, bCanUse);
}

//--------------------------------------------------------------------------------
BOOL CLoginThread::CanUseNamespace(const CString&	nameSpace, int grantType)
{
	if (!AfxGetLoginManager()->IsSecurityLightEnabled())
		return TRUE;

	BOOL bCanUse = FALSE;
	if (InternalCanUseNamespace(nameSpace, grantType, bCanUse))
		return bCanUse;

	bCanUse =  AfxGetLoginManager()->CanUseNamespace(nameSpace, grantType);
	
	InternalSetCanUseNamespace(nameSpace, grantType, bCanUse);

	return bCanUse;
}

//--------------------------------------------------------------------------------
void CLoginThread::AddFormatter(Formatter* pFormatter)
{
	if (GetFormatsTable())
		((FormatStyleTable*) GetFormatsTable ())->AddFormatter(pFormatter->Clone());
}

//----------------------------------------------------------------------------------------------
void CLoginThread::InitDataCachingManager ()
{
	AttachDataCachingUpdatesListener(new CDataCachingUpdatesListener());
	
	CDataCachingSettings* pSettings = new CDataCachingSettings();
	AttachDataCachingSettings(pSettings);

	// lock is not needed as LoginThread and all LoginContext objects are initializing
	if (pSettings->IsDataCachingEnabled() && pSettings->GetCacheScope() == CDataCachingSettings::LOGIN)
		AfxGetOleDbMng()->GetDataCachingContext()->CreateCache ();
}
//-----------------------------------------------------------------------------
CXMLVariable* CLoginThread::GetVariable(const CString& sName)
{
	TB_OBJECT_LOCK_FOR_READ(&m_Variables);
	CXMLVariable* pVariable;
	for (int i = 0; i <= m_Variables.GetUpperBound(); i++)
	{
		pVariable = m_Variables.GetAt(i);
		if (pVariable && sName.CompareNoCase(pVariable->GetName()) == 0)
			return pVariable;
	}

	return NULL;
}
//-----------------------------------------------------------------------------
void CLoginThread::DeclareVariable(const CString& sName, DataObj* pDataObj, BOOL bOwnsDataObj, BOOL bReplaceExisting /*FALSE*/)
{
	TB_OBJECT_LOCK(&m_Variables);
	CXMLVariable* pXMLVar = NULL;
	if (pXMLVar = GetVariable(sName))
	{
		if (bReplaceExisting)
		{
			ASSERT(pXMLVar->GetOwnsDataObj() == bOwnsDataObj);
			pXMLVar->SetDataObj(pDataObj);
			return;
		}
		TRACE1("Variable %s has already been declared", sName);
		return;
	}

	pXMLVar = new CXMLVariable(sName, pDataObj, bOwnsDataObj);
	m_Variables.Add(pXMLVar);
}


//-----------------------------------------------------------------------------
void CLoginThread::CopyVariables(CAbstractFormDoc* pDoc)
{
	TB_OBJECT_LOCK_FOR_READ(&m_Variables);
	CXMLVariable* pVariable;
	for (int i = 0; i <= m_Variables.GetUpperBound(); i++)
	{
		pVariable = m_Variables.GetAt(i);
		pDoc->DeclareVariable(pVariable->GetName(), pVariable->GetDataObj()->Clone(), TRUE);
	}
}

/////////////////////////////////////////////////////////////////////////////
//								CultureOverride
//////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CultureOverride, CObject)
