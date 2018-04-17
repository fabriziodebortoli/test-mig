// TBWinThread.cpp : implementation file
//
#include "stdafx.h"

#include "ThreadContext.h"
#include "Templates.h"

#define CHECK_INTERVAL_TICKS	1000 //un secondo: se viene effettuata una check message prima che sia trascorso questo intervallo 
									 //viene ignorata per non appesantire la procedura

//----------------------------------------------------------------------------
bool IntervalChecker::SkipOperations()
{
	int tick = GetTickCount();
	//ottimizzazione: se la CheckMessage viene chiamata troppo frequentemente rallenta inutilmente le procedure
	if ((tick - m_nLatestTick) < CHECK_INTERVAL_TICKS)
		return true;

	m_nLatestTick = tick;
	return false;
}

//----------------------------------------------------------------------------
static AfxIsCurrentlyInUnattendedModePtr g_AfxIsCurrentlyInUnattendedModePtr = NULL; 

//----------------------------------------------------------------------------
void SetAfxIsCurrentlyInUnattendedModePtr(AfxIsCurrentlyInUnattendedModePtr ptr)
{
	g_AfxIsCurrentlyInUnattendedModePtr = ptr;
}

//----------------------------------------------------------------------------
class CFunctionWrapperDisposer
{
	CBaseFunctionWrapper* m_pWrapper;
public:
	CFunctionWrapperDisposer(CBaseFunctionWrapper* pWrapper) : m_pWrapper(pWrapper) {}
	~CFunctionWrapperDisposer() { m_pWrapper->Dispose(); }
};

//----------------------------------------------------------------------------
CBaseFunctionWrapper::CBaseFunctionWrapper(DWORD nThreadId)
	: m_nThreadId(nThreadId), m_Hwnd(NULL), m_pReady(NULL), m_nCallingThreadId(0), m_bReceived(false), m_bAsync(false), m_bSuccess(false)
{	
}
//----------------------------------------------------------------------------
CBaseFunctionWrapper::CBaseFunctionWrapper(HWND hwnd)
	: m_nThreadId(0), m_Hwnd(hwnd), m_pReady(NULL), m_nCallingThreadId(0), m_bReceived(false), m_bAsync(false), m_bSuccess(false)
{	
	m_nThreadId = (m_Hwnd != NULL) ? GetWindowThreadProcessId(m_Hwnd, NULL) : ::GetCurrentThreadId(); 
}

//----------------------------------------------------------------------------
CBaseFunctionWrapper::~CBaseFunctionWrapper()
{
	delete m_pReady;
}
//----------------------------------------------------------------------------
void CBaseFunctionWrapper::Call(bool bAsync)
{
	m_bAsync = bAsync;
	m_nCallingThreadId = ::GetCurrentThreadId();
	if (!m_bAsync && m_nCallingThreadId != m_nThreadId)
		m_pReady = new ::CEvent();

	if (!bAsync && m_nCallingThreadId == m_nThreadId)
	{
		//for automatic object deletion even when exception is thrown
		CFunctionWrapperDisposer __disposer(this);
		SetReceived();
		InternalCall();
		m_bSuccess = true;
	}
	else
	{
		if (!TBPostThreadMessage(bAsync))
		{
			CThreadCallFailedException* pExc = new CThreadCallFailedException(m_nCallingThreadId, m_nThreadId);
			if (bAsync)
				delete this;
			throw(pExc);
		}
		if (!bAsync && !m_bSuccess)
			throw (new CApplicationErrorException(m_sErrorMessage)); 
	}
}

#define RETRIALS 60
#define WAIT	1000 //un secondo
//----------------------------------------------------------------------------
BOOL CBaseFunctionWrapper::TBPostThreadMessage(bool bAsync)
{
	BOOL b = m_Hwnd
		? (::IsWindow(m_Hwnd) ? PostMessage(m_Hwnd, UM_EXECUTE_FUNCTION, (WPARAM)this, NULL) : FALSE)
		: ::TBPostThreadMessage(m_nThreadId, UM_EXECUTE_FUNCTION, (WPARAM)this, NULL);

	if (b && !bAsync)
	{
		CWinThread* pThread = AfxGetThread();
		DWORD total = RETRIALS;
		while (!m_pReady->Lock(WAIT)) //aspetto un po'
		{
			//se mi è arrivato un WM_QUIT, esco
			if (pThread && !CTBWinThread::PumpThreadMessages())
				return FALSE;

			if (m_bReceived)
			{
				//se il messaggio è stato ricevuto ma ho già apettato più volte,
				//controllo se il thread esiste
				if (total-- <= 0)
				{
					if (!CTBWinThread::IsThreadAlive(m_nThreadId))
						return FALSE;
					total = RETRIALS;
				}
			}
			else //se il messaggio non è ancora stato ricevuto, controllo se il thread esiste
			{
				if (!CTBWinThread::IsThreadAlive(m_nThreadId))
					return FALSE;
			}
		}
	}

	return b;
}
//----------------------------------------------------------------------------
BOOL CBaseFunctionWrapper::OnExecuteFunction()
{
	//for automatic object deletion even when exception is thrown
	CFunctionWrapperDisposer __disposer(this);

	try
	{
		SetReceived();
		InternalCall();
	}
	catch (CThreadAbortedException* e)
	{
		m_sErrorMessage = _T("Thread has been aborted");
		throw e;
	}
#ifndef DEBUG //when debugging it is useful if the program stops when the error occurs
	catch (CException *e)
	{
		if (e)
		{
			m_sErrorMessage = AfxGetDiagnostic()->Add(e);
			e->Delete();
		}
		return FALSE;
	}
	catch (...)
	{
		m_sErrorMessage = _T("An unexpected error occurred");
		return FALSE;
	}
#endif
	m_bSuccess = true;
	return TRUE;
}


//----------------------------------------------------------------------------
void CBaseFunctionWrapper::Dispose()
{
	DWORD nCallingThreadId = m_nCallingThreadId;
	
	try
	{
		bool bAsync = m_bAsync; //save current state, because after SetEvent object is deleted by waiting thread
		if (m_pReady)
			m_pReady->SetEvent();
		
		if (bAsync)
			delete this; //async call, callee has to destroy object
	}
	catch(...)
	{
		ASSERT(FALSE);
	}
}
// CTBEvent
//----------------------------------------------------------------------------
CTBEvent::CTBEvent()
:
m_WaitingThreadId(0), m_bSignaled(false)
{
}

//----------------------------------------------------------------------------
void CTBEvent::Set()
{
	m_bSignaled = true;
	PostThreadMessage(m_WaitingThreadId, UM_SIGNAL_TB_EVENT, (WPARAM) this, NULL);
}
//----------------------------------------------------------------------------
void CTBEvent::Reset()
{
	m_bSignaled = false;
}
//----------------------------------------------------------------------------
void CTBEvent::SetWaitingThread(CWinThread* pThread)
{
	m_WaitingThreadId = pThread->m_nThreadID;
}

//----------------------------------------------------------------------------
bool CTBEvent::IsSignaled()
{
	return m_bSignaled;
}


IMPLEMENT_DYNAMIC(CThreadAbortedException, CException)


//----------------------------------------------------------------------------
void CThreadAbortedException::Dump(CDumpContext& dc) const
{
	CException::Dump(dc);

	dc << _T("CThreadAbortedException");
}


// CTBWinThread

//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTBWinThread, CWinThread)

//----------------------------------------------------------------------------
CTBWinThread::CTBWinThread()
	: 
	m_pLoginContext(NULL),
	m_pTBThreadProc(NULL)
{
}

//----------------------------------------------------------------------------
CTBWinThread::CTBWinThread(AFX_THREADPROC pfnThreadProc, LPVOID pParam, CLoginContext* pLoginContext /*= NULL*/)
: 
CWinThread(pfnThreadProc, pParam),
	m_pLoginContext(pLoginContext),
	m_pTBThreadProc(NULL)
{
}

//----------------------------------------------------------------------------
CTBWinThread::~CTBWinThread()
{
}



//----------------------------------------------------------------------------
BOOL CTBWinThread::InitInstance()
{
	if (m_pLoginContext)
		AfxAttachThreadToLoginContext(m_pLoginContext->GetName());
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CTBWinThread::OnIdle(LONG lCount)
{
	if (IsSuspendedIdle())
		return FALSE;

	AfxGetThreadContext()->ClearOldObjects();
	AfxGetThreadContext()->GarbageUnusedSqlSession();

	BOOL bContinueIdle = AfxGetThreadContext()->OnIdle(lCount);

	return __super::OnIdle(lCount) || bContinueIdle;
}

//----------------------------------------------------------------------------
LRESULT CTBWinThread::ProcessWndProcException(CException* e, const MSG* pMsg)
{
	AfxGetDiagnostic()->AttachViewer(NULL);
	AfxGetDiagnostic()->EnableTraceInEventViewer(TRUE);
	AfxGetDiagnostic()->Add(e); //null pointer accepted

	if (AfxIsThreadInInnerLoop())
	{
		if (e && e->IsKindOf(RUNTIME_CLASS(CThreadAbortedException)))
			throw (CThreadAbortedException*)e; //without this cast che outer catch handler does not recognize CThreadAbortedException
		throw e;
	}
		
	return __super::ProcessWndProcException(e, pMsg);
}

//----------------------------------------------------------------------------
int CTBWinThread::ExitInstance()
{
#ifndef DEBUG
	try
#endif
	{
		AfxGetThreadContext()->ClearVoids();
		AfxGetThreadContext()->ClearObjects();

		AfxGetThreadContext()->RaiseCallBreakEvent();
		
		AfxGetThreadContext()->ClearObjects();

		return __super::ExitInstance();	
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

//----------------------------------------------------------------------------
BOOL CTBWinThread::PumpMessage()
{
	if (AfxGetThreadContext()->IsClosing()) //WM_QUIT received: no more PumpMessages 
		return FALSE;

	if (!CWinThread::PumpMessage())
	{
		AfxGetThreadContext()->SetClosing();
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void SetThreadName(DWORD dwThreadID, LPCSTR szThreadName)
{
	struct tagTHREADNAME_INFO
	{
		DWORD dwType; // must be 0x1000
		LPCSTR szName; // pointer to name (in user addr space)
		DWORD dwThreadID; // thread ID (-1=caller thread)
		DWORD dwFlags; // reserved for future use, must be zero
	}
	info;

	{
		info.dwType = 0x1000;
		info.szName = szThreadName;
		info.dwThreadID = dwThreadID;
		info.dwFlags = 0;
	}
	__try
	{
		RaiseException( 0x406D1388, 0, sizeof(info)/sizeof(DWORD), (DWORD*)&info );
	}
	__except (EXCEPTION_CONTINUE_EXECUTION)
	{
	}
}

//----------------------------------------------------------------------------
void CTBWinThread::SetThreadName (const CStringA& sName)
{
	::SetThreadName(m_nThreadID, sName);
	m_strName = sName;
}

//----------------------------------------------------------------------------
CStringA CTBWinThread::GetThreadName() const
{
	return m_strName;
}

//----------------------------------------------------------------------------
BOOL CTBWinThread::IsInInnerLoop()
{
	return AfxIsThreadInInnerLoop();
}
//----------------------------------------------------------------------------
CThreadInfo* CTBWinThread::AddThreadInfos(CThreadInfoArray& arInfos)
{
	USES_CONVERSION;
	CLoginContext* pContext = AfxGetLoginContext();
	CString strThreadName = pContext ? A2T(pContext->GetThreadName()) : _T("");

	CThreadInfo* pInfo = new CThreadInfo
		(
		m_pMainWnd->m_hWnd,
		IsDocumentThread(), 
		IsInInnerLoop(), 
		m_nThreadID,
		pContext ? pContext->m_nThreadID : 0,
		A2T(GetThreadName()),
		strThreadName
		);
	TB_OBJECT_LOCK(&arInfos);
	arInfos.Add(pInfo);
	return pInfo;
}
//----------------------------------------------------------------------------
BOOL CTBWinThread::IsCurrentlyInUnattendedMode()
{
	return  g_AfxIsCurrentlyInUnattendedModePtr == NULL ? TRUE : g_AfxIsCurrentlyInUnattendedModePtr();
}
//----------------------------------------------------------------------------
BOOL CTBWinThread::PumpThreadMessages(BOOL bIncreaseInnerLoopDepth /*= TRUE*/)
{
	// for tracking the idle time state
	BOOL bIdle = TRUE;
	LONG lIdleCount = 0;

	return PumpThreadMessages(bIdle, lIdleCount, bIncreaseInnerLoopDepth);
}

//----------------------------------------------------------------------------
BOOL CTBWinThread::IsThreadAlive(DWORD dwThreadId)
{
	HANDLE hThread = OpenThread(SYNCHRONIZE, FALSE, dwThreadId);
	if (!hThread)
		return FALSE;
	
	BOOL exited = WaitForSingleObject(hThread, 1) == WAIT_OBJECT_0;
	
	CloseHandle(hThread);
	return !exited;
}

//----------------------------------------------------------------------------
BOOL DoEvents(CWinThread* pThread, BOOL& bIdle, LONG& lIdleCount, BOOL bIncreaseInnerLoopDepth /*= TRUE*/)
{
	MSG msg;

	while (::PeekMessage(&msg, NULL, NULL, NULL, PM_NOREMOVE))
	{
		// pump message, but quit on WM_QUIT
		if (!pThread->PumpMessage())
		{
			AfxPostQuitMessage(0);
			return FALSE;
		}

		if (pThread->IsIdleMessage(&msg))
		{
			bIdle = TRUE;
			lIdleCount = 0L;
		}
	}

	if (bIdle)
		bIdle = pThread->OnIdle(lIdleCount++);
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CTBWinThread::DoEvents(BOOL& bIdle, LONG& lIdleCount, BOOL bIncreaseInnerLoopDepth /*= TRUE*/)
{
	return ::DoEvents(this, bIdle, lIdleCount, bIncreaseInnerLoopDepth);
}
//----------------------------------------------------------------------------
BOOL CTBWinThread::PumpThreadMessages(BOOL& bIdle, LONG& lIdleCount, BOOL bIncreaseInnerLoopDepth /*= TRUE*/)
{
	CWinThread* pThread = AfxGetThread();
	if (!pThread) 
		return TRUE; //no MFC message loop available

	CPushMessageLoopDepthMng __pushLoopDepth(THREAD_LOOP, bIncreaseInnerLoopDepth);

	if (pThread->IsKindOf(RUNTIME_CLASS(CTBWinThread)))
	{
		return ((CTBWinThread*)pThread)->DoEvents(bIdle, lIdleCount, bIncreaseInnerLoopDepth);
	}

	return ::DoEvents(pThread, bIdle, lIdleCount, bIncreaseInnerLoopDepth);
}
//----------------------------------------------------------------------------
void CTBWinThread::LoopUntil(CTBEvent* tbEvent, ActionPointer pAction /*= NULL*/, void* pArg /*= NULL*/)
{
	CArray<CTBEvent*> tbEvents;
	tbEvents.Add(tbEvent);
	return LoopUntil(tbEvents, pAction, pArg);
}

//----------------------------------------------------------------------------
void CTBWinThread::LoopUntil(CArray<CTBEvent*>& tbEvents, ActionPointer pAction /*= NULL*/, void* pArg /*= NULL*/)
{
	CWinThread* pThread = AfxGetThread();
	//this code is identical to CWinThread::Run except for the use of tbEvents
	ASSERT_VALID(pThread);
	_AFX_THREAD_STATE* pState = AfxGetThreadState();

	for (int i = 0; i < tbEvents.GetCount(); i++)
	{
		CTBEvent* evt = tbEvents[i];
		if (evt->IsSignaled())
			return;
		
		evt->SetWaitingThread(pThread);
		
		if (evt->IsSignaled()) //could have become signaled meanwhile
			return;
	}

	// for tracking the idle time state
	BOOL bIdle = TRUE;
	LONG lIdleCount = 0;

	CPushMessageLoopDepthMng __pushLoopDepth(THREAD_LOOP);

	// acquire and dispatch messages until a WM_QUIT message is received.
	for (;;)
	{
		if (pAction)
			pAction(pArg);

		// phase1: check to see if we can do idle work
		while (bIdle &&
			!::PeekMessage(&(pState->m_msgCur), NULL, NULL, NULL, PM_NOREMOVE))
		{
			// call OnIdle while in bIdle state
			if (!pThread->OnIdle(lIdleCount++))
				bIdle = FALSE; // assume "no idle" state
		}

		// phase2: pump messages while available
		do
		{
			// pump message, but quit on WM_QUIT
			if (!pThread->PumpMessage())
			{
				AfxPostQuitMessage(0);
				return;
			}
	
			for (int i = 0; i < tbEvents.GetCount(); i++)
				if (tbEvents[i]->IsSignaled())
					return;

			// reset "no idle" state after pumping "normal" message
			//if (IsIdleMessage(&m_msgCur))
			if (pThread->IsIdleMessage(&(pState->m_msgCur)))
			{
				bIdle = TRUE;
				lIdleCount = 0;
			}

		} while (::PeekMessage(&(pState->m_msgCur), NULL, NULL, NULL, PM_NOREMOVE));
	}
}

//----------------------------------------------------------------------------
CDocument* CTBWinThread::OpenDocumentOnCurrentThread(const CSingleDocTemplate* pTemplate, LPCTSTR pInfo, BOOL bMakeVisible /*= TRUE*/)
{
	ASSERT(FALSE);//non dovrebbe passare di qui, solo il login thread e i document thread che fanno l'override possono aprire un documento
	//fra l'altro creano un clone del template e se lo tengono da parte, qui non posso farlo perché non conosco la classe template derivata

	return NULL;	
}



//deve essere reimplementato nelle classi derivate CDocumentThread, CLoginThread ...
//default: un thread puo' essere stoppato
//-----------------------------------------------------------------------------
BOOL CTBWinThread::CanStopThread()
{
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL AfxCanStopThread()
{ 
	CTBWinThread* pThread = AfxGetTBThread();
	return pThread->CanStopThread();
}

//-----------------------------------------------------------------------------
UINT TBWinThreadProc( LPVOID pParam )
{
	CTBWinThread* pThread = AfxGetTBThread();

	if (pThread->m_pLoginContext)
		AfxAttachThreadToLoginContext(pThread->m_pLoginContext->GetName());

	return pThread->m_pTBThreadProc(pParam);
}

//-----------------------------------------------------------------------------
CTBWinThread* AFXAPI AfxBeginTBThread(AFX_THREADPROC pfnThreadProc, LPVOID pParam, CLoginContext* pContext /*= NULL*/, 
	int nPriority, UINT nStackSize, DWORD dwCreateFlags,
	LPSECURITY_ATTRIBUTES lpSecurityAttrs, LPCSTR name /*= NULL*/)
{

	ASSERT(pfnThreadProc != NULL);

	CTBWinThread* pThread = new CTBWinThread(&TBWinThreadProc, pParam, pContext);
	ASSERT_VALID(pThread);

	pThread->m_pTBThreadProc = pfnThreadProc;
	if (name) pThread->m_strName = name;

	if (!pThread->CreateThread(dwCreateFlags|CREATE_SUSPENDED, nStackSize,
		lpSecurityAttrs))
	{
		pThread->Delete();
		return NULL;
	}
	VERIFY(pThread->SetThreadPriority(nPriority));
	if (!(dwCreateFlags & CREATE_SUSPENDED))
		VERIFY(pThread->ResumeThread() != (DWORD)-1);

	return pThread;

}


//-----------------------------------------------------------------------------
BOOL AFXAPI AfxIsThreadInInnerLoop()
{
	return AfxGetThreadContext()->GetInnerLoopDepth() > 0;
}
//-----------------------------------------------------------------------------
BOOL AFXAPI AfxIsThreadInModalState()
{
	return AfxGetThreadContext()->IsThreadInModalState();
}

//-----------------------------------------------------------------------------
void AFXAPI AfxSetThreadInModalState(BOOL bSet, HWND hwndModalWindow)
{
	if (bSet)
	{
		AfxGetThreadContext()->AddWindowRef(hwndModalWindow, TRUE);
		AfxGetThreadContext()->RaiseCallBreakEvent();
	}
	else
	{
		AfxGetThreadContext()->RemoveWindowRef(hwndModalWindow, TRUE);
	}
}

//-----------------------------------------------------------------------------
HANDLE GetCallBreakEventHandle()
{
	return AfxGetThreadContext()->GetCallBreakEventHandle();
}
//-----------------------------------------------------------------------------
HANDLE GetCallBreakEventHandle(HWND hwnd)
{
	return AfxInvokeThreadGlobalFunction<HANDLE>(hwnd, &GetCallBreakEventHandle);

}

//-----------------------------------------------------------------------------
inline HWND GetThreadMainWnd()
{
	CWinThread* pThread = AfxGetThread();
	return pThread && pThread->m_pMainWnd ? pThread->m_pMainWnd->m_hWnd : NULL; 
}

//-----------------------------------------------------------------------------
BOOL TBPostThreadMessage(DWORD dwThreadId, UINT Msg, WPARAM wParam, LPARAM lParam)
{
	HWND hwnd = AfxGetApplicationContext()->GetThreadMainWindow(dwThreadId);
	return ::IsWindow(hwnd) 
		? PostMessage(hwnd, Msg, wParam, lParam) 
		: PostThreadMessage(dwThreadId, Msg, wParam, lParam);
}

//-----------------------------------------------------------------------------
LRESULT TBSendThreadMessage(DWORD dwThreadId, UINT Msg, WPARAM wParam, LPARAM lParam)
{
	HWND hwnd = AfxGetApplicationContext()->GetThreadMainWindow(dwThreadId);
	return ::SendMessage(hwnd, Msg, wParam, lParam);
}



