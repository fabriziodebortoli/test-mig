#include "stdafx.h"
#include "TBMessageQueue.h"
#include "TBDC.h"
#include "USER32.h"
#include "HookedFunction.h"

EXTERN_HOOK_LIB(BOOL, PeekMessageW, (__out LPMSG lpMsg, __in_opt HWND hWnd, __in UINT wMsgFilterMin, __in UINT wMsgFilterMax, __in UINT wRemoveMsg));

MSG* CreateMessage(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	MSG* pMsg = new MSG;
	pMsg->hwnd = hwnd;
	pMsg->message = message;
	pMsg->lParam = lParam;
	pMsg->wParam = wParam;
	pMsg->time = GetTickCount();
	return pMsg;
}
CQueueMap::~CQueueMap()
{
	POSITION pos = GetStartPosition();
	DWORD key;
	CMessageQueue* pVal;
	while (pos)
	{
		GetNextAssoc(pos, key, pVal);
		delete pVal;
	}
}


CThreadContext::CThreadContext() : m_bUseStandardApis(AfxGetThread() == NULL)
{
}
CThreadContext::~CThreadContext()
{
	if (m_bUseStandardApis)
		return;

	DWORD id = ::GetCurrentThreadId();
	DestroyMessageQueue(id);

	CSingleLock l(Get_CWindowMapSection(), TRUE);
	Get_CWindowMap().CleanUpThreadWnd(id);
}

THREAD_LOCAL(CThreadContext, _threadContext);

CThreadContext* GetThreadContext() { return _threadContext; }
CMessageQueue* GetMessageQueue(DWORD dwThreadId)
{
	CSingleLock l(Get_CQueueMapSection(), TRUE);
	CMessageQueue* pQueue = NULL;
	if (!Get_CQueueMap().Lookup(dwThreadId, pQueue))
	{
		pQueue = new CMessageQueue(dwThreadId);
		Get_CQueueMap().SetAt(dwThreadId, pQueue);
	}

	return pQueue;
}
void DestroyMessageQueue(DWORD dwThreadId)
{
	CSingleLock l(Get_CQueueMapSection(), TRUE);
	CMessageQueue* pQueue = NULL;
	if (Get_CQueueMap().Lookup(dwThreadId, pQueue))
	{
		delete pQueue;
		Get_CQueueMap().RemoveKey(dwThreadId);
	}
}


//----------------------------------------------------------------------------
CMessageQueue::CMessageQueue(DWORD dwThreadId)
	:
	m_nThreadID(dwThreadId), m_pFocused(NULL), m_pActive(NULL)
{
}

//----------------------------------------------------------------------------
CMessageQueue::~CMessageQueue()
{

#ifdef DEBUG
	//remove redundant WM_QUIT messages
	for (int i = m_PostTBMessageQueue.GetUpperBound(); i >= 0; i--)
		if (m_PostTBMessageQueue[i]->message == WM_QUIT)
		{
			delete m_PostTBMessageQueue[i];
			m_PostTBMessageQueue.RemoveAt(i);
		}
#endif
	//no more messages should exist!
	ASSERT(m_PostTBMessageQueue.IsEmpty());
	ASSERT(m_SendTBMessageQueue.IsEmpty());
	ASSERT(m_SendTBMessageCallbackQueue.IsEmpty());
}

//----------------------------------------------------------------------------
TBWnd* CMessageQueue::SetFocus(TBWnd* pWnd, BOOL bSendMessages)
{
	TBWnd* pOld = m_pFocused;
	if (bSendMessages && pOld)
		SendMessage(pOld, WM_KILLFOCUS, (WPARAM)pWnd->GetHWND(), NULL);
	m_pFocused = pWnd;
	if (bSendMessages && m_pFocused)
		SendMessage(m_pFocused, WM_SETFOCUS, pOld ? (WPARAM)pOld->GetHWND() : NULL, NULL);
	return m_pFocused;
}
//----------------------------------------------------------------------------
TBWnd* CMessageQueue::GetFocus()
{
	return m_pFocused;
}

//----------------------------------------------------------------------------
TBWnd* CMessageQueue::GetActiveWindow()
{
	return m_pActive;
}
//----------------------------------------------------------------------------
TBWnd* CMessageQueue::SetActiveWindow(TBWnd* pWnd, BOOL bChangeZOrder)
{
	if (!pWnd || !pWnd->IsTopLevelWindow())
		return m_pActive;
	TBWnd* pOld = m_pActive;
	if (pOld)
		SendMessage(pOld, WM_ACTIVATE, MAKEWPARAM(WA_INACTIVE, 0), (LPARAM)pWnd->GetHWND());
	if (bChangeZOrder)
		pWnd->SetWindowPos(HWND_TOP, 0, 0, 0, 0, SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOSIZE);
	m_pActive = pWnd;
	SendMessage(m_pActive, WM_ACTIVATE, MAKEWPARAM(WA_ACTIVE, 0), (LPARAM)(pOld ? pOld->GetHWND() : NULL));
	return pOld;
}

class Hook
{
public:
	HHOOK hhk;
	int id;
	HOOKPROC lpfn;
};

//----------------------------------------------------------------------------
HHOOK CMessageQueue::AddHook(int idHook, HOOKPROC lpfn, HINSTANCE hmod)
{
	Hook* pHook = new Hook;
	pHook->id = idHook;
	pHook->lpfn = lpfn;
	pHook->hhk = GetNewHOOK();
	m_arHooks.Add(pHook);
	return pHook->hhk;
}
//----------------------------------------------------------------------------
BOOL CMessageQueue::RemoveHook(__in HHOOK hhk)
{
	for (int i = 0; i < m_arHooks.GetCount(); i++)
	{
		Hook* pHook = m_arHooks.GetAt(i);
		if (pHook->hhk == hhk)
		{
			delete pHook;
			m_arHooks.RemoveAt(i);
			return TRUE;
		}
	}
	return FALSE;
}

//----------------------------------------------------------------------------
void CMessageQueue::CallHook(HWND hwnd, CBT_CREATEWNDW* pCreateWnd)
{
	for (int i = 0; i < m_arHooks.GetCount(); i++)
	{
		Hook* pHook = m_arHooks.GetAt(i);
		if (pHook->id == WH_CBT)
		{
			pHook->lpfn(HCBT_CREATEWND, (WPARAM)hwnd, (LPARAM)pCreateWnd);
		}
	}
}

//----------------------------------------------------------------------------
void CMessageQueue::AddCallback(SEND_MSG* pMsg)
{
	m_TBCriticalSendCallback.Lock();
	m_SendTBMessageCallbackQueue.Add(pMsg);
	m_TBCriticalSendCallback.Unlock();
	m_TBMessageAvailable.SetEvent();
}


//----------------------------------------------------------------------------
void CMessageQueue::DispatchTimerMessages()
{
	for (int i = 0; i < m_arTimers.GetCount(); i++)
	{
		m_arTimers[i]->Call();
	}
}
//----------------------------------------------------------------------------
void CMessageQueue::DispatchSendMessages()
{
	///vedo se ci sono dei callback SendMessage da processare (lo faccio immediatamente)
	m_TBCriticalSendCallback.Lock();//ridondante? NO, l'evento m_TBMessageAvailable non è sufficiente, qualcuno potrebbe farmi una Send proprio ora da un altro thread
	for (int i = 0; i < m_SendTBMessageCallbackQueue.GetCount(); i++)
	{
		SEND_MSG* currMsg = m_SendTBMessageCallbackQueue[i];
		m_SendTBMessageCallbackQueue.RemoveAt(i);
		//chiamo la callback
		ASSERT(currMsg->m_lpResultCallBack);
		currMsg->m_lpResultCallBack(currMsg->hwnd, currMsg->message, currMsg->m_dwData, currMsg->m_Result);
		delete currMsg;
	}
	m_TBCriticalSendCallback.Unlock();

	CArray<SEND_MSG*> callbackMessages;
	///vedo se ci sono dei SendMessage da processare (lo faccio immediatamente)
	m_TBCriticalSend.Lock();//ridondante? NO, l'evento m_TBMessageAvailable non è sufficiente, qualcuno potrebbe farmi una Send proprio ora da un altro thread
	for (int i = 0; i < m_SendTBMessageQueue.GetCount(); i++)
	{
		SEND_MSG* currMsg = m_SendTBMessageQueue[i];
		m_SendTBMessageQueue.RemoveAt(i);

		if (!currMsg->hwnd || _TBNAME(IsWindow)(currMsg->hwnd))
		{
			//processo il messaggio chiamando la mia window procedure
			currMsg->m_Result = DispatchMessage(currMsg, currMsg->m_pWnd);
		}
		else
		{
			//la finestra nel frattempo è andata distrutta
			currMsg->m_Result = ERROR_INVALID_PARAMETER;
		}
		//quindi rilascio il chiamante che sta aspettando il ritorno della SendMessage
		currMsg->m_TBMessageProcessed.SetEvent();
		if (currMsg->m_lpResultCallBack)
		{
			//sono in fase di lock, mi metto da parte il messaggio processato e lo metto nella coda di callback dopo che ho rilasciato
			//il lock corrente, per evitare deadlocks
			callbackMessages.Add(currMsg);
		}
	}
	m_TBCriticalSend.Unlock();
	for (int i = 0; i < callbackMessages.GetCount(); i++)
	{
		SEND_MSG* currMsg = callbackMessages[i];
		GetMessageQueue(currMsg->m_dwCallingThread)->AddCallback(currMsg);
	}

}

//----------------------------------------------------------------------------
BOOL CMessageQueue::MyPeekMessage(MSG* lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg)
{
	//first check original message queue
	if (USER32_ORIGINAL(PeekMessageW)(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg))
		return TRUE;

	//processes sent messages
	DispatchSendMessages();

	CSingleLock aLock(&m_TBCriticalPost, TRUE);
	for (int i = 0; i < m_PostTBMessageQueue.GetCount(); i++)
	{
		MSG* currMsg = m_PostTBMessageQueue[i];
		if ((!hWnd || currMsg->hwnd == hWnd) && (wMsgFilterMin == NULL || currMsg->message >= wMsgFilterMin) && (wMsgFilterMax == NULL || currMsg->message <= wMsgFilterMax))
		{
			*lpMsg = *currMsg;
			if ((wRemoveMsg & PM_REMOVE) == PM_REMOVE)
			{
				delete currMsg;
				m_PostTBMessageQueue.RemoveAt(i);
			}
			return TRUE;
		}
	}
	//processes sent messages again (see MSDN documentation)
	DispatchSendMessages();
	//sends WM_TIMER if SetTimer has been called
	DispatchTimerMessages();
	return FALSE;
}
//----------------------------------------------------------------------------
BOOL CMessageQueue::GetMessage(MSG* lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax)
{
	//questa funzione deve essere chiamata solo sulla coda del thread corrente
	ASSERT(m_nThreadID == GetCurrentThreadId());
	while (true)
	{
		//prima di tutto vedo se qualcuno ha postato un messaggio, aspetto il via libera dal semaforo
		//da qui passo solo se una delle due code ha dei messaggi
		//la chiamata mi resetta l'evento allo stato di non segnalato, per cui se all'uscita da questa funzione nella coda ci sono ancora messaggi devo
		//fare attenzione a ri segnalare l'evento

		m_TBMessageAvailable.Lock(min(1000, FistTimerDueIn()));//wait until next timer has to be fired, or a message has been posted or sent

		//processes sent messages
		DispatchSendMessages();
	
		//first check original message queue; use PeekMessage with PM_REMOVE, so simulating GetMessage
		if (USER32_ORIGINAL(PeekMessageW)(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, PM_REMOVE))
			return lpMsg->message != WM_QUIT;

		//poi vado nella coda della PostMessage e ritorno il primo messaggio
		m_TBCriticalPost.Lock();		//ridondante? NO, l'evento m_TBMessageAvailable non è sufficiente, qualcuno potrebbe farmi una Post proprio ora da un altro thread
		for (int i = 0; i < m_PostTBMessageQueue.GetCount(); i++)
		{
			MSG* currMsg = m_PostTBMessageQueue[i];
			if (currMsg->message == WM_QUIT ||
				((!hWnd || currMsg->hwnd == hWnd) && (wMsgFilterMin == NULL || currMsg->message >= wMsgFilterMin) && (wMsgFilterMax == NULL || currMsg->message <= wMsgFilterMax)))
			{
				*lpMsg = *currMsg;
				m_PostTBMessageQueue.RemoveAt(i);
				//se ho trovato il messaggio ma nella coda ce ne sono ancora, segnalo nuovamente l'evento,
				//cosi la prossima GetMessage non dovrà aspettare
				if (m_PostTBMessageQueue.GetCount())
					m_TBMessageAvailable.SetEvent();
				bool bQuit = currMsg->message == WM_QUIT;
				m_TBCriticalPost.Unlock();
				delete currMsg;
				//verifico che la finestra a cui è diretto il messaggio non sia distrutta
				if (!lpMsg->hwnd || _TBNAME(IsWindow)(lpMsg->hwnd))
					return !bQuit;
			}
		}
		m_TBCriticalPost.Unlock();
		//processes sent messages again (see MSDN documentation)
		DispatchSendMessages();
		//sends WM_TIMER if SetTimer has been called
		DispatchTimerMessages();
	}
}
//----------------------------------------------------------------------------
void CMessageQueue::RunMessageLoop()
{
	MSG msg;

	while (MyPeekMessage(&msg, NULL, NULL, NULL, PM_NOREMOVE))
	{
		if (!GetMessage(&msg, NULL, NULL, NULL))
		{
			return;
		}

		if (msg.message != WM_KICKIDLE)
		{
			TranslateMessage(&msg);
			TBWnd* pWnd = GetTBWnd(msg.hwnd);
			DispatchMessage(&msg, pWnd);
		}
	}

}

//----------------------------------------------------------------------------
LRESULT CMessageQueue::SendMessage(TBWnd* pWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	SEND_MSG* pMsg = new SEND_MSG(pWnd, message, wParam, lParam);
	//se sono sul thread giusto, effettuo direttemente la chiamata
	if (m_nThreadID == ::GetCurrentThreadId())
	{
		LRESULT lr = DispatchMessage(pMsg, pWnd);
		delete pMsg;
		return lr;
	}

	//altrimenti metto il messaggio nella coda
	{//lock scope
		CSingleLock aLock(&m_TBCriticalSend, TRUE);
		m_SendTBMessageQueue.Add(pMsg);
	}

	//segnalo alla GetMessage che c'è un messaggio
	m_TBMessageAvailable.SetEvent();

	//aspetto che il messaggio sia stato processato
	pMsg->m_TBMessageProcessed.Lock();
	LRESULT res = pMsg->m_Result;
	delete pMsg;
	return res;
}
//----------------------------------------------------------------------------
BOOL CMessageQueue::SendMessageCallback(TBWnd* pWnd, UINT message, WPARAM wParam, LPARAM lParam, __in SENDASYNCPROC lpResultCallBack, __in ULONG_PTR dwData)
{
	DWORD dwcurrThread = ::GetCurrentThreadId();
	SEND_MSG* pMsg = new SEND_MSG(pWnd, message, wParam, lParam, lpResultCallBack, dwData, dwcurrThread);

	//se sono sul thread giusto, effettuo direttemente la chiamata
	if (m_nThreadID == dwcurrThread)
	{
		LRESULT lr = DispatchMessage(pMsg, pWnd);
		pMsg->m_lpResultCallBack(pMsg->hwnd, pMsg->message, pMsg->m_dwData, lr);
		delete pMsg;
		return TRUE;
	}

	//altrimenti metto il messaggio nella coda
	{//lock scope
		CSingleLock aLock(&m_TBCriticalSend, TRUE);
		m_SendTBMessageQueue.Add(pMsg);
	}

	//segnalo alla GetMessage che c'è un messaggio
	m_TBMessageAvailable.SetEvent();

	return TRUE;
}
//----------------------------------------------------------------------------
BOOL CMessageQueue::PostMessage(MSG* pMsg)
{
	ASSERT(pMsg);
	CSingleLock aLock(&m_TBCriticalPost, TRUE);
	
	m_PostTBMessageQueue.Add(pMsg);

	m_TBMessageAvailable.SetEvent(); //semaforo. Do il via libera alla GetMessage
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CMessageQueue::TranslateMessage(CONST MSG *lpMsg)
{
	switch(lpMsg->message)
	{
	case WM_KEYDOWN:
		{
			UINT nKeyCode = lpMsg->wParam;
			UINT nCharCode = MapVirtualKey(nKeyCode, MAPVK_VK_TO_CHAR);
			if (!nCharCode)
				return FALSE;
			MSG* pMsg = CreateMessage(lpMsg->hwnd, WM_CHAR, nCharCode, lpMsg->lParam);
			PostMessage(pMsg);
			return TRUE;
		}
	case WM_KEYUP:
		return TRUE;
	}
	return FALSE;
}
//----------------------------------------------------------------------------
void CMessageQueue::RegisterThreadMessageCallback(OnThreadMessageCallback callback)
{
	m_arThreadMessages.Add(callback);
}
//----------------------------------------------------------------------------
void CMessageQueue::UnregisterThreadMessageCallback(OnThreadMessageCallback callback)
{
	for (int i = 0; i < m_arThreadMessages.GetCount(); i++)
	{
		OnThreadMessageCallback p = m_arThreadMessages[i];
		if (p == callback)
		{
			m_arThreadMessages.RemoveAt(i);
			break;
		}
	}
}
//----------------------------------------------------------------------------
LRESULT CMessageQueue::DispatchMessage(CONST MSG *lpMsg, TBWnd* pWnd)
{
	if (pWnd)
		return pWnd->Dispatch(lpMsg->message, lpMsg->wParam, lpMsg->lParam);

	for (int i = 0; i < m_arThreadMessages.GetCount(); i++)
		m_arThreadMessages[i](lpMsg->message, lpMsg->wParam, lpMsg->lParam);

	switch (lpMsg->message)
	{
	case WM_TIMER:
	{
		UINT_PTR nTimerID = (UINT_PTR)lpMsg->wParam;
		TIMERPROC lpTimerFunc = (TIMERPROC)lpMsg->lParam;
		if (lpTimerFunc)
			lpTimerFunc(NULL, WM_TIMER, nTimerID, GetTickCount());
		return 0L;
	}
	default:
		return 0L;
	}

}
//-----------------------------------------------------------------------------
CTBTimer* CMessageQueue::GetTimer(HWND hWnd, UINT_PTR nIDEvent)
{
	ASSERT(m_nThreadID == GetCurrentThreadId()); //no sync needed, but function must be called on its own thread
	for (int i = 0; i < m_arTimers.GetCount(); i++)
	{
		CTBTimer* pTimer = m_arTimers.GetAt(i);
		if (pTimer->Equals(hWnd, nIDEvent))
			return pTimer;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
DWORD CMessageQueue::FistTimerDueIn()
{
	DWORD milliseconds = INFINITE;
	DWORD now = GetTickCount();
	for (int i = 0; i < m_arTimers.GetCount(); i++)
	{
		CTBTimer* pTimer = m_arTimers.GetAt(i);
		DWORD delta = pTimer->GetDueTime() - now;
		if (!pTimer->HasBeenCalled() && delta > 0)
			milliseconds = min(milliseconds, delta);
	}
	return milliseconds;
}

//-----------------------------------------------------------------------------
BOOL CMessageQueue::DestroyTimer(HWND hWnd, UINT_PTR nIDEvent)
{
	ASSERT(m_nThreadID == GetCurrentThreadId()); //no sync needed, but function must be called on its own thread
	for (int i = 0; i < m_arTimers.GetCount(); i++)
	{
		CTBTimer* pTimer = m_arTimers.GetAt(i);
		if (pTimer->Equals(hWnd, nIDEvent))
		{
			m_arTimers.RemoveAt(i);
			delete pTimer;
			return TRUE;
		}

	}
	return FALSE;
}

//-----------------------------------------------------------------------------
UINT_PTR CMessageQueue::CreateTimer(HWND hWnd, UINT_PTR nIDEvent, UINT uElapse, TIMERPROC lpTimerFunc)
{
	ASSERT(m_nThreadID == GetCurrentThreadId()); //no sync needed, but function must be called on its own thread
	CTBTimer* pTimer = GetTimer(hWnd, nIDEvent);
	if (pTimer)
	{
		pTimer->ResetTimer(uElapse, lpTimerFunc);
	}
	else
	{
		pTimer = new CTBTimer(hWnd, nIDEvent, uElapse, lpTimerFunc);
		m_arTimers.Add(pTimer);
	}
	return pTimer->GetId();
}
