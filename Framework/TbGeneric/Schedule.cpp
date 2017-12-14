
#include "stdafx.h"

#include <TbNameSolver\ThreadContext.h>

#include "schedule.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//===========================================================================
//	class Scheduler implementation
//===========================================================================

////////////////////////////////////////////////////////////////////////
//	ORGINAL MAIN LOOP (from CWinThread::Run in file Thrdcore.cpp)
////////////////////////////////////////////////////////////////////////
/*
	BOOL bIdle = TRUE;
	LONG lIdleCount = 0;

	// acquire and dispatch messages until a WM_QUIT message is received.
	for (;;)
	{
		// phase1: check to see if we can do idle work
		while (bIdle &&
			!::PeekMessage(&m_msgCur, NULL, NULL, NULL, PM_NOREMOVE))
		{
			// call OnIdle while in bIdle state
			if (!OnIdle(lIdleCount++))
				bIdle = FALSE; // assume "no idle" state
		}

		// phase2: pump messages while available
		do
		{
			// pump message, but quit on WM_QUIT
			if (!PumpMessage())
				return ExitInstance();

			// reset "no idle" state after pumping "normal" message
			if (IsIdleMessage(&m_msgCur))
			{
				bIdle = TRUE;
				lIdleCount = 0;
			}

		} while (::PeekMessage(&m_msgCur, NULL, NULL, NULL, PM_NOREMOVE));
	}
*/    
////////////////////////////////////////////////////////////////////////
    
// Viene chiamata da un loop esterno (per esempio di parsing, running, od altro)
// tale da implememtare il main loop originale (vedi sopra).
//-----------------------------------------------------------------------
BOOL Scheduler::CheckMessage()
{
	if (!m_bPaused && m_IntervalChecker.SkipOperations())
		return m_bAborted;

	if (!m_bAborted && !CTBWinThread::PumpThreadMessages(m_bIdle, m_nCount))
		Terminate();

	return m_bAborted;
}

//-----------------------------------------------------------------------
BOOL Scheduler::IsAborted()
{
	do
	{
		if (CheckMessage())	Terminate();
	}
	while (m_bPaused);

	return (m_bAborted);
}

//-----------------------------------------------------------------------
void Scheduler::Terminate()
{
	m_bAborted	= TRUE;
	m_bPaused	= FALSE;
	m_bIdle		= TRUE;
	m_nCount	= 0L;
}

//-----------------------------------------------------------------------
void Scheduler::Start()
{
	m_bAborted 		= FALSE;
	m_bPaused		= FALSE;
	m_bIdle			= TRUE;
	m_nCount		= 0L;
}

//-----------------------------------------------------------------------
void Scheduler::SendMessage	(UINT cmd, WPARAM wParam, LPARAM lParam)	
{ 
	if (m_hWndComunication)
		::SendMessage(m_hWndComunication, cmd, wParam, lParam);
}

//-----------------------------------------------------------------------
void Scheduler::PostMessage	(UINT cmd, WPARAM wParam, LPARAM lParam)	
{ 
	if (m_hWndComunication) 
		::PostMessage(m_hWndComunication,cmd,wParam,lParam); 
}

//-----------------------------------------------------------------------
void Scheduler::Attach (HWND hWnd)	
{ 
	if (m_hWndComunication)
	{   
		AfxMessageBox(_T("Attach Error in Scheduler !!"));      
		AfxAbort();
		return;
	}                    
	
	m_hWndComunication = hWnd; 
}

//-----------------------------------------------------------------------
void Scheduler::Detach ()				
{
	m_hWndComunication = NULL;
}

