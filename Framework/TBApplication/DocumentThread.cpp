#include "stdafx.h"

#include <sys/timeb.h>

#include <TbFrameworkImages\GeneralFunctions.h>
#include <tbnamesolver\threadcontext.h>
#include <tbnamesolver\ThreadInfo.h>
#include <TbNameSolver\PathFinder.h>
#include <tbgeneric\localizableobjs.h>
#include <tbgeneric\webservicestateobjects.h>
#include <TbGenlib\diagnosticmanager.h>

#include "DocumentThread.h"
#include "LoginThread.h"
#include "TaskBuilderApp.h"
#include "ThreadMainWindow.h"


//////////////////////////////////////////////////////////////////////////////////	
//								CDocumentThread class implementation
//////////////////////////////////////////////////////////////////////////////////	
//--------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDocumentThread, CTBWinThread)

//--------------------------------------------------------------------------------
CDocumentThread::CDocumentThread()
{
	m_pMainWnd = NULL;	//from CWinThread

}

//--------------------------------------------------------------------------------
CDocumentThread::~CDocumentThread()
{
	m_pLoginThread->RemoveDocumentThread(this);
}

//--------------------------------------------------------------------------------
BOOL CDocumentThread::InitInstance()
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

//--------------------------------------------------------------------------------
BOOL CDocumentThread::InitInstanceInternal()
{
	if (m_nThreadHooksState != DEFAULT)
		ActivateThreadHooks(m_nThreadHooksState == ACTIVE);
	class CSetReady
	{
		CTBEvent& m_Ready;
	public:
		CSetReady(CTBEvent& ready) : m_Ready(ready) {  }
		~CSetReady() { m_Ready.Set(); }
	} setReady(m_Ready);

	VERIFY(SUCCEEDED(OleInitialize(NULL)));
	if (!__super::InitInstance())
		return FALSE;

#ifndef DEBUG
	try
#endif
	{
		m_pMainWnd = new CThreadMainWindow(AfxMultiThreadedDocument()); //dummy main window (MFC needs a thread main window)
		if (AfxGetDiagnostic())
		{
			AfxGetDiagnostic()->AttachViewer(AfxCreateDefaultViewer());
			AfxGetDiagnostic()->EnableTraceInEventViewer(IsEventViewerTraceEnabled());
		}
		AfxAttachThreadToLoginContext(m_strContextName);

		m_pLoginThread = (CLoginThread*)AfxGetLoginContext();
		m_pLoginThread->AddDocumentThread(this);
		
		if (AfxIsRemoteInterface())
			AfxGetThreadContext()->m_pDocSession = new CDocumentSession(m_pLoginThread->m_nThreadID);

		InitThreadContext();
		InitThreadCulture();

		//manage images cache size
		m_GlobalCacheImages.ManageMemoryUsage();

		if (m_pOriginalTemplate)
			OpenDocument();

		//inizializza con l'istante attuale i tempi che gesticono l'interazione con il thread 
		//e l'istante della richiesta della descrizione della finestra
		AfxGetThreadContext()->SetThreadInteractionTime();
		AfxGetThreadContext()->SetThreadGetDescriptionTime();


	}
#ifndef DEBUG //when debugging it is useful if the program stops when the error occurs
	catch (CException* pException)
	{
		ManageInitException(pException);
		return FALSE;
	}
#endif
	return TRUE;
}

//--------------------------------------------------------------------------------
int CDocumentThread::ExitInstance()
{
	__try
	{
		return ExitInstanceInternal();
	}
	__except (ExpFilter(GetExceptionInformation(), GetExceptionCode()))
	{
		AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)
	}
	return -1;
}
//--------------------------------------------------------------------------------
int CDocumentThread::ExitInstanceInternal()
{
#ifndef DEBUG
	try
#endif
	{
		USES_DIAGNOSTIC();
		CTaskBuilderApp::AdjustClosingMessageViewer();

		DestroyAllDocuments();

		AfxGetThreadContext()->PerformExitinstanceOperations();
		if (m_pMainWnd)
			m_pMainWnd->DestroyWindow();

		CThreadContext* pContext = AfxGetThreadContext();
		for (int i = 0; i < pContext->GetWebServiceStateObjects().GetCount(); i++)
			AfxRemoveWebServiceStateObject(pContext->GetWebServiceStateObjects()[i], TRUE);

		int nRet = __super::ExitInstance();
		OleUninitialize();
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

//--------------------------------------------------------------------------------
void CDocumentThread::DestroyAllDocuments()
{
	const CThreadContext::DocumentArray& docs = AfxGetThreadContext()->GetDocuments();
	int i = 0;
	while ((i = docs.GetUpperBound()) >= 0)
	{
		try
		{
			AfxGetTbCmdManager()->DestroyDocument((const CBaseDocument*)docs[i]);
		}
		catch (...) //however a catch could be useful...
		{
			ASSERT(FALSE);
			break;
		}
	}
}

//--------------------------------------------------------------------------------
BOOL CDocumentThread::DoEvents(BOOL& bIdle, LONG& lIdleCount, BOOL bIncreaseInnerLoopDepth /*= TRUE*/)
{
	if (m_bManagedMessagePump)
	{
		::ApplicationDoEvents();
		MSG msg;
		if (!::PeekMessage(&msg, NULL, NULL, NULL, PM_NOREMOVE) || IsIdleMessage(&msg))
		{
			::ApplicationRaiseIdle();
		}
		return TRUE;
	}
	else
	{
		return __super::DoEvents(bIdle, lIdleCount, bIncreaseInnerLoopDepth);
	}
}

//--------------------------------------------------------------------------------
int CDocumentThread::Run()
{
	__try
	{
		if (m_bManagedMessagePump)
		{
			ApplicationRun(this);
			return ExitInstance();
		}
		else
		{
			return __super::Run();
		}
	}
	__except (ExpFilter(GetExceptionInformation(), GetExceptionCode()))
	{
		AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)
		return -1;
	}

}

//--------------------------------------------------------------------------------
CDocument* CDocumentThread::OpenDocument()
{
	ASSERT_KINDOF(CSingleExtDocTemplate, m_pOriginalTemplate);
	if (!m_pMainDocument)
	{
		USES_CONVERSION;
		SetThreadName(T2A(((CSingleExtDocTemplate*)m_pOriginalTemplate)->GetNamespace().ToString()));
	}

	CSingleExtDocTemplate *pNewTemplate = new CSingleExtDocTemplate((CSingleExtDocTemplate*)m_pOriginalTemplate, m_pInfo);
	m_nOpenStack++;
	CDocument* pDocument = pNewTemplate->OpenDocumentFile((LPCTSTR)m_pInfo, m_bMakeVisible);//il documento farà poi la delete
	m_nOpenStack--;
	if (pDocument)
	{
		if (!m_pMainDocument && m_nOpenStack == 0)//solo la prima OpenDocumentFile mi deve valorizzare il main document
		{
			m_pMainDocument = pDocument;
			//reimposto il nome del thread, potrebbe essere cambiato (caso di WOORM)
			USES_CONVERSION;
			SetThreadName(T2A(((CBaseDocument*)pDocument)->GetNamespace().ToString()));
		}

		CBaseDocument* pBaseDoc = (CBaseDocument*)pDocument;
		pBaseDoc->DispatchDocumentCreated();
		pBaseDoc->OnOpenCompleted();

	}
	return pDocument;
}

//-----------------------------------------------------------------------------
CWnd* CDocumentThread::GetMainWnd()
{
	CWnd* pWnd = __super::GetMainWnd();

	if (pWnd->IsKindOf(RUNTIME_CLASS(CThreadMainWindow)))
	{
		CWnd* pMenuWnd = AfxGetThreadContext()->GetMenuWindow();
		if (pMenuWnd)
			return pMenuWnd;
	}

	return pWnd;

}

//--------------------------------------------------------------------------------
void CDocumentThread::Start(const CString &strContextName)
{
	m_strContextName = strContextName;
	CreateThread();
	LoopUntil(&m_Ready);
}

//--------------------------------------------------------------------------------
CDocument* CDocumentThread::OpenDocumentOnNewThread(
	const CSingleDocTemplate* pTemplate,
	DocInvocationParams* pInfo,
	const CString &strContextName,
	BOOL bMakeVisible /* = TRUE*/)
{
	m_pInfo = pInfo;
	m_pOriginalTemplate = pTemplate;
	m_bMakeVisible = bMakeVisible;
	m_strContextName = strContextName;


	m_bAutoDelete = FALSE; //prendo il controllo della gestione della cancellazione dell'oggetto

	ASSERT(m_hThread == NULL);
	ASSERT(m_nThreadID == 0);

	CreateThread();

	LoopUntil(&m_Ready);

	if (m_pMainDocument)
	{
		//il documento è stato creato, il thread può gestire autonomamente la cancellazione dell'oggetto
		m_bAutoDelete = TRUE;
		return m_pMainDocument;
	}

	//il documento non è stato creato, dico al thread di morire, aspetto che lo faccia e poi distruggo l'oggetto

	PostThreadMessage(WM_QUIT, 0, NULL);
	WaitForSingleObject(m_hThread, INFINITE);
	delete this;
	return NULL;
}

//--------------------------------------------------------------------------------
CDocument* CDocumentThread::OpenDocumentOnCurrentThread(
	const CSingleDocTemplate* pTemplate,
	LPCTSTR pInfo,
	BOOL bMakeVisible /* = TRUE*/)
{
	if (m_nThreadID != AfxGetThread()->m_nThreadID)
		return AfxInvokeThreadFunction<CDocument*, CDocumentThread, const CSingleDocTemplate*, LPCTSTR, BOOL>
		(
		m_nThreadID,
		this,
		&CDocumentThread::OpenDocumentOnCurrentThread,
		pTemplate,
		pInfo,
		bMakeVisible
		);

	m_pInfo = (DocInvocationParams*)pInfo;
	m_pOriginalTemplate = pTemplate;
	m_bMakeVisible = bMakeVisible;

	//ci sono casi in cui il documento viene impostato "no interface", che però è un view mode finto. Adesso se il documento è invisibile, è anche unattended
	CDocument* pDoc = OpenDocument();
	CBaseDocument* pBaseDoc = (CBaseDocument*)pDoc;
	if (pBaseDoc && !bMakeVisible)
		pBaseDoc->SetInUnattendedMode(TRUE);

	return pDoc;
}


//--------------------------------------------------------------------------------
void CDocumentThread::ManageRunException(CException* pException)
{
	AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)

	if (!AfxGetThreadContext()->IsClosing())
	{
		USES_DIAGNOSTIC();
		AfxGetDiagnostic()->Add(pException); //null pointer accepted
	}
	if (pException)
		pException->Delete();
}
//--------------------------------------------------------------------------------
void CDocumentThread::ManageInitException(CException* pException)
{
	AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)

	CDiagnostic* pDiagnostic = AfxGetDiagnostic();
	pDiagnostic->Add(_TB("Error initializing document thread"));
	pDiagnostic->Add(pException); //null pointer accepted
	if (pException)
		pException->Delete();

	AfxGetLoginContext()->AddDiagnostic(pDiagnostic);
}
//--------------------------------------------------------------------------------
LRESULT CDocumentThread::ProcessWndProcException(CException* e, const MSG* pMsg)
{
	USES_DIAGNOSTIC();
	AfxGetDiagnostic()->Add(e); //null pointer accepted
	return __super::ProcessWndProcException(e, pMsg);
}

//--------------------------------------------------------------------------------
void CDocumentThread::OnStopThread(WPARAM, LPARAM)
{
	if (m_pMainDocument)
		AfxGetTbCmdManager()->CloseDocument((const CBaseDocument*)m_pMainDocument);
	else
		AfxPostQuitMessage(0);
}


//Reimplementa il metodo di CTBWinThread mettendo in aggiunta informazioni specifiche del thread di documento
//(il titolo del documento principale + un bool che dice se il thread puo' essere stoppato in modo safe)
//----------------------------------------------------------------------------
CThreadInfo* CDocumentThread::AddThreadInfos(CThreadInfoArray& arInfos)
{
	CThreadInfo* pInfo = __super::AddThreadInfos(arInfos);
	if (!pInfo)
		return NULL;

	//recupero informazioni su titolo - data operazioni - utente - company
	// dal documento principale del thread document
	if (m_pMainDocument)
		pInfo->SetTitle(m_pMainDocument->GetTitle());

	pInfo->SetCanBeStopped(CanStopThread());

	//info di login;
	pInfo->SetCompany(m_pLoginThread->GetLoginInfos()->m_strCompanyName);
	pInfo->SetUser(m_pLoginThread->GetLoginInfos()->m_strUserName);
	pInfo->SetOperationDate(AfxGetApplicationDate().Str(FALSE));


	struct _timeb timebuffer;
	if (_ftime_s(&timebuffer) == 0) //returns Zero if successful
	{
		time_t inactivityInteractionTime = timebuffer.time - AfxGetThreadContext()->GetLastInteractionTime();
		if (inactivityInteractionTime > 0)
			pInfo->SetInactivityTime(inactivityInteractionTime);

		time_t inactivityGetDescrTime = timebuffer.time - AfxGetThreadContext()->GetLastGetDescriptionTime();
		if (inactivityGetDescrTime > 0)
			pInfo->SetRemoteUserInterfaceAttached(inactivityGetDescrTime < THREAD_INTERACTION_THRESHOLD_TIME);
	}

	return pInfo;
}

//Metodo che dice se un thread puo essere stoppato in modo safe.
//Cicla sull'array dei documenti di DocumentThread e chiede a ognuno se puo essere chiuso
//--------------------------------------------------------------------------------
BOOL CDocumentThread::CanStopThread()
{
	const CThreadContext::DocumentArray& docs = AfxGetThreadContext()->GetDocuments();

	for (int i = 0; i < docs.GetCount(); i++)
	{
		CBaseDocument* pDoc = (CBaseDocument*)docs[i];
		if (!pDoc->CanCloseDocument())
			return FALSE;
	}

	return TRUE;
}

//--------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDocumentThread, CTBWinThread)
	//{{AFX_MSG_MAP(CDocumentThread)
	ON_THREAD_MESSAGE(UM_STOP_THREAD, OnStopThread)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


