#pragma once

#include "TbResourceLocker.h"
#include "beginh.dex"

class CThreadInfoArray;
class CThreadInfo;
class CLoginContext;

//===========================================================================
//	class IntervalChecker definition
//	tiene traccia dell'intervallo di tempo trascorso fra una chiamata e l'altra
//	e da il via libera solo se e' trascorso un intervallo apprezzabile
//	serve per evitare che vengano chiamate migliaia di istruzioni in un intervallo
//	breve, ad esempio feedback utente, che appesantiscono e non danno vantaggi reali
//===========================================================================
class TB_EXPORT IntervalChecker
{
	DWORD	m_nLatestTick;
public:
	IntervalChecker() : m_nLatestTick(0) {}
	bool SkipOperations();
};

//classes derived from CBaseFunctionWrapper are used to call a function on a different
//thread: instead of invoking directly the funcion, you create a wrapper of the function call context
//(pointer to the object, pointer to the function, params) and then post a message to the
//target thread passing this wrapper as WPARAM (this thread uses the wrapper to perform the call, then
//sets an event to signal the function has been called and return value (if any) is ready

//AfxInvokeThreadFunction function family wraps all details for you
//==================================================================================
class TB_EXPORT CBaseFunctionWrapper 
{
	friend class CFunctionWrapperDisposer;
protected:

	DWORD			m_nCallingThreadId;
	DWORD			m_nThreadId;
	HWND			m_Hwnd;
	::CEvent*		m_pReady;
	volatile bool	m_bReceived;
	bool			m_bAsync;
	bool			m_bSuccess;
	CString			m_sErrorMessage;

public:
	CBaseFunctionWrapper(DWORD nThreadId);
	CBaseFunctionWrapper(HWND hwnd);
	virtual ~CBaseFunctionWrapper();
	BOOL OnExecuteFunction();
	void Call(bool bAsync);
	
private:
	BOOL TBPostThreadMessage(bool bAsync);
	virtual void InternalCall() = 0;
	void Dispose();
	void SetReceived() { m_bReceived = true; }
};

// CTBEvent
//==================================================================================
class TB_EXPORT CTBEvent
{
friend class CTBWinThread;

	volatile bool	m_bSignaled;
	DWORD	m_WaitingThreadId;

	void SetWaitingThread(CWinThread* pThread);
public:
	CTBEvent();
	void Set();
	void Reset();
	bool IsSignaled();

};


enum UserInteractionMode {UNDEFINED = 0, ATTENDED = 1, UNATTENDED = 2};

typedef BOOL (AFXAPI *AfxIsCurrentlyInUnattendedModePtr) (void);

TB_EXPORT void SetAfxIsCurrentlyInUnattendedModePtr(AfxIsCurrentlyInUnattendedModePtr ptr);

//==================================================================================
class TB_EXPORT CThreadAbortedException : public CException
{
public:
	DECLARE_DYNAMIC(CThreadAbortedException)
	~CThreadAbortedException() {}

	virtual void Dump(CDumpContext& dc) const;
};

typedef void (*ActionPointer) (void*);




// CTBWinThread

//==================================================================================
class TB_EXPORT CTBWinThread : public CWinThread
{ 
friend class CTaskBuilderApp;
friend class CApplicationContext;
	
DECLARE_DYNCREATE(CTBWinThread)

	CStringA m_strName;
	CLoginContext*		m_pLoginContext;
	AFX_THREADPROC		m_pTBThreadProc;
public:
	// constructor used by implementation of AfxBeginTBThread
	CTBWinThread(AFX_THREADPROC pfnThreadProc, LPVOID pParam, CLoginContext* pLoginContext = NULL);

protected:
	CTBWinThread();           // protected constructor used by dynamic creation
	virtual ~CTBWinThread();

public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	virtual CDocument* OpenDocumentOnCurrentThread(const CSingleDocTemplate* pTemplate, LPCTSTR pInfo, BOOL bMakeVisible = TRUE);
	virtual BOOL IsDocumentThread() { return FALSE; }
	
	BOOL IsInInnerLoop();
	void SetThreadName		(const CStringA& sName);
	
	CStringA GetThreadName	() const;
	static BOOL PumpThreadMessages(BOOL bIncreaseInnerLoopDepth = TRUE);
	static BOOL PumpThreadMessages(BOOL& bIdle, LONG& lIdleCount, BOOL bIncreaseInnerLoopDepth = TRUE);
	static void LoopUntil(CTBEvent* tbEvent, ActionPointer pAction = NULL, void* pArg = NULL);
	static void LoopUntil(CArray<CTBEvent*>& tbEvents, ActionPointer pAction = NULL, void* pArg = NULL);
	static BOOL IsThreadAlive(DWORD dwThreadId);
	static BOOL IsCurrentlyInUnattendedMode();
	virtual BOOL CanStopThread();
	virtual bool IsManaged(){ return false; }

protected:
	int m_nSuspendIdle = 0;
	BOOL m_bSuspendIdleEnabled = TRUE;

public:
	void GarbageUnusedSqlSession();
	void IncSuspendIdle() { m_nSuspendIdle++; }
	void DecSuspendIdle() { if (m_nSuspendIdle) m_nSuspendIdle--; }
	void ResetSuspendIdle() { m_nSuspendIdle = 0; }
	void SetEnableSuspendIdle(BOOL bEnable) { m_bSuspendIdleEnabled = bEnable; }
	BOOL IsSuspendedIdle() { return m_bSuspendIdleEnabled && m_nSuspendIdle > 0; }
	virtual BOOL OnIdle(LONG lCount);

	virtual BOOL PumpMessage();

	virtual LRESULT ProcessWndProcException(CException* e, const MSG* pMsg);

protected:
	virtual CThreadInfo* AddThreadInfos(CThreadInfoArray& arInfos);
	virtual BOOL DoEvents(BOOL& bIdle, LONG& lIdleCount, BOOL bIncreaseInnerLoopDepth /*= TRUE*/);
};

//-----------------------------------------------------------------------------
TB_EXPORT BOOL	AfxCanStopThread();

//-----------------------------------------------------------------------------
TB_EXPORT CTBWinThread* AFXAPI AfxBeginTBThread(AFX_THREADPROC pfnThreadProc, LPVOID pParam, CLoginContext* pContext = NULL,
	int nPriority = THREAD_PRIORITY_NORMAL, UINT nStackSize = 0,
	DWORD dwCreateFlags = 0, LPSECURITY_ATTRIBUTES lpSecurityAttrs = NULL, LPCSTR name = NULL);

//-----------------------------------------------------------------------------
inline CTBWinThread* AfxGetTBThread() 
{
	CWinThread *pThread = AfxGetThread();
	if (!pThread || !pThread->IsKindOf(RUNTIME_CLASS(CTBWinThread)))
		return NULL;

	return (CTBWinThread*)pThread;
}

TB_EXPORT BOOL AFXAPI AfxIsThreadInInnerLoop();
TB_EXPORT BOOL AFXAPI AfxIsThreadInModalState();
TB_EXPORT void AFXAPI AfxSetThreadInModalState(BOOL bSet, HWND hwndModalWindow);


TB_EXPORT HANDLE GetCallBreakEventHandle(HWND hwnd);
inline TB_EXPORT HWND GetThreadMainWnd();

TB_EXPORT BOOL TBPostThreadMessage(DWORD dwThreadId, UINT Msg, WPARAM wParam, LPARAM lParam);
TB_EXPORT LRESULT TBSendThreadMessage(DWORD dwThreadId, UINT Msg, WPARAM wParam, LPARAM lParam);
TB_EXPORT void SetThreadName(DWORD dwThreadID, LPCSTR szThreadName);

#include "endh.dex"
