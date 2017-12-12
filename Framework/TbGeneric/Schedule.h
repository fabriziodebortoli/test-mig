#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"



//===========================================================================
//	class Scheduler definition
//===========================================================================
class TB_EXPORT Scheduler
{
protected:
	HWND	m_hWndComunication;
    BOOL	m_bPaused;
	BOOL	m_bAborted;
	BOOL	m_bIdle;
	LONG	m_nCount;
	IntervalChecker m_IntervalChecker;

public:
		Scheduler	()	{ m_hWndComunication = NULL; Start(); }
		~Scheduler	()	{ Terminate(); }
        
	// check run activities
	BOOL	IsAborted		();
	BOOL	IsStarted		()	{ return !m_bAborted; }
	BOOL	IsPaused		()	{ return m_bPaused; }
	
	// message dispatching
	BOOL	CheckMessage	();

    // start and stop scheduler activities
	void	Terminate	();
	void	Start		();
	
	// pause and resume scheduler activities
	void	Pause		()	{ m_bPaused = TRUE; }
	void	Resume		()	{ m_bPaused = FALSE; }

	// attach window for message comunication
	virtual void Attach		(HWND hWnd);
	virtual void Detach 	();
	
	BOOL	IsAttached		() const { return m_hWndComunication != NULL; }
	HWND	GetAttachedCWnd	() const { return m_hWndComunication; }

	// comunication routines
	void	SendMessage	(UINT aCmd, WPARAM wParam = 0, LPARAM lParam = 0);
	void	PostMessage	(UINT aCmd, WPARAM wParam = 0, LPARAM lParam = 0);
};

#include "endh.dex"
