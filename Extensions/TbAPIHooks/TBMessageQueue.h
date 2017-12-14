#pragma once
#include "TBWnd.h"
#include "TBTimer.h"
class CMessageQueue;
class TBDC;
class Hook;
class CThreadContext;
class CQueueMap : public CMap<DWORD, DWORD, CMessageQueue*, CMessageQueue*>
{
public:
	~CQueueMap();
};

MSG* CreateMessage(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);

class SEND_MSG : public MSG
{
public:
	SEND_MSG(TBWnd*	pWnd, UINT msg, WPARAM w, LPARAM l, SENDASYNCPROC lpResultCallBack = NULL, ULONG_PTR dwData = 0, DWORD dwCallingThread = 0)
	{
		this->hwnd = pWnd->GetHWND();
		this->message = msg;
		this->lParam = l;
		this->wParam = w;
		this->time = GetTickCount();
		this->m_pWnd = pWnd;
		this->m_lpResultCallBack = lpResultCallBack;
		this->m_dwData = dwData;
		this->m_Result = 0;
		this->m_dwCallingThread = dwCallingThread;
	}
	CEvent	m_TBMessageProcessed;
	LRESULT m_Result;
	TBWnd*	m_pWnd;
	SENDASYNCPROC m_lpResultCallBack;
	ULONG_PTR m_dwData;
	DWORD m_dwCallingThread;
};

typedef void (*OnThreadMessageCallback)(UINT message, WPARAM, LPARAM);

//coda dei messaggi del thread 
class CMessageQueue
{
	CArray<Hook*>					m_arHooks;
	CArray<MSG*>					m_PostTBMessageQueue;	//coda della PostMessage
	CArray<SEND_MSG*>				m_SendTBMessageQueue;	//coda della SendMessage
	CArray<SEND_MSG*>				m_SendTBMessageCallbackQueue;	//coda della SendMessageCallback
	CEvent							m_TBMessageAvailable;	//evento che viene settato quando ci sono messaggi in una delle due code
	CCriticalSection				m_TBCriticalSendCallback;		//per accedere in esclusiva alla coda dei messaggi di callback Send
	CCriticalSection				m_TBCriticalSend;		//per accedere in esclusiva alla coda dei messaggi Send
	CCriticalSection				m_TBCriticalPost;		//per accedere in esclusiva alla coda dei messaggi Post
	DWORD							m_nThreadID;
	TBWnd*							m_pFocused;
	TBWnd*							m_pActive;
	CTimerArray						m_arTimers;
	CArray<OnThreadMessageCallback>	m_arThreadMessages;
	
	void DispatchSendMessages();
	void DispatchTimerMessages();
public:
	CMessageQueue(DWORD dwThreadId);
	~CMessageQueue();
	//quisti metodi vanno a sostituire quelli di windows
	BOOL MyPeekMessage(MSG* lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg);
	BOOL GetMessage(MSG* lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax);
	LRESULT SendMessage(TBWnd* pWnd, UINT message, WPARAM wParam, LPARAM lParam);
	BOOL SendMessageCallback(TBWnd* pWnd, UINT message, WPARAM wParam, LPARAM lParam, __in SENDASYNCPROC lpResultCallBack, __in ULONG_PTR dwData);
	BOOL PostMessage(MSG* pMsg);
	BOOL TranslateMessage(CONST MSG *lpMsg);
	LRESULT DispatchMessage(CONST MSG *lpMsg, TBWnd* pWnd);
	void AddCallback(SEND_MSG* pMsg);
	TBWnd* SetFocus(TBWnd* pWnd, BOOL bSendMessages);
	TBWnd* GetFocus();
	TBWnd* GetActiveWindow();
	TBWnd* SetActiveWindow(TBWnd* pWnd, BOOL bChangeZOrder);
	HHOOK AddHook(int idHook, HOOKPROC lpfn, HINSTANCE hmod);
	BOOL RemoveHook(__in HHOOK hhk);
	void CallHook(HWND hwnd, CBT_CREATEWNDW* pCreateWnd);
	UINT_PTR CreateTimer(HWND hWnd, UINT_PTR nIDEvent, UINT uElapse, TIMERPROC lpTimerFunc);
	BOOL DestroyTimer(HWND hWnd, UINT_PTR nIDEvent);
	void RegisterThreadMessageCallback(OnThreadMessageCallback callback);
	void UnregisterThreadMessageCallback(OnThreadMessageCallback callback);
	void RunMessageLoop();
private:
	CTBTimer* GetTimer(HWND hWnd, UINT_PTR nIDEvent);
	DWORD FistTimerDueIn();
};

class CThreadContext: public CNoTrackObject
{
public:
	bool	m_bUseStandardApis;
	CThreadContext();
	~CThreadContext();
};
CThreadContext* GetThreadContext();
CMessageQueue* GetMessageQueue(DWORD dwThreadId);
void DestroyMessageQueue(DWORD dwThreadId);
