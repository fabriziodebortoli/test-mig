#include "stdafx.h"
#include "TBTimer.h"
#include "HookedFunction.h"


static long g_latestTimerId = 0;

//-----------------------------------------------------------------------------
CTBTimer::CTBTimer(HWND hwnd, UINT_PTR nIDEvent, UINT uElapse, TIMERPROC lpTimerFunc) 
	:
	m_hWnd(hwnd), 
	m_nTimerID(nIDEvent)
{
	if (m_nTimerID == 0)
		m_nTimerID = InterlockedIncrement(&g_latestTimerId);
	ResetTimer(uElapse, lpTimerFunc);
}
//-----------------------------------------------------------------------------
BOOL CTBTimer::Equals(HWND hwnd, UINT_PTR nIDEvent)
{
	return m_hWnd == hwnd && m_nTimerID == nIDEvent;
}
//-----------------------------------------------------------------------------
void CTBTimer::Call()
{
	DWORD now = GetTickCount();
	if (now > m_nDueTime && !m_bCalled)
	{
		PostMessage(m_hWnd, WM_TIMER, m_nTimerID, (LPARAM)m_lpTimerFunc);
		m_bCalled = TRUE;
	}
}
//-----------------------------------------------------------------------------
void CTBTimer::ResetTimer(UINT uElapse, TIMERPROC lpTimerFunc)
{
	m_nDueTime = ::GetTickCount() + uElapse;
	m_lpTimerFunc = lpTimerFunc;
	m_bCalled = FALSE;
}

//-----------------------------------------------------------------------------
CTBTimer::~CTBTimer()
{
}


//-----------------------------------------------------------------------------
CTimerArray::~CTimerArray()
{
	for (int i = 0; i < GetCount(); i++)
		delete GetAt(i);
}