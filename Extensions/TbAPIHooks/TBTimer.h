#pragma once


class CTBTimer
{
	HWND m_hWnd;
	UINT_PTR m_nTimerID;
	DWORD m_nDueTime;
	TIMERPROC m_lpTimerFunc;
	BOOL m_bCalled;
public:
	CTBTimer(HWND hwnd, UINT_PTR nIDEvent, UINT uElapse, TIMERPROC lpTimerFunc);
	~CTBTimer();
	BOOL Equals(HWND hwnd, UINT_PTR nIDEvent);
	void Call();
	UINT_PTR GetId(){ return m_nTimerID; }
	BOOL HasBeenCalled() { return m_bCalled; }
	void ResetTimer(UINT uElapse, TIMERPROC lpTimerFunc);
	DWORD GetDueTime(){ return m_nDueTime; }
};


class CTimerArray : public CArray<CTBTimer*, CTBTimer*>
{
public:
	~CTimerArray();
};


