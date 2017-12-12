
#include "stdafx.h" 

#include <TBNamesolver\LoginContext.h>
#include <TBNamesolver\ThreadContext.h>


#include "Globals.h"
#include "StatusBarMessages.h"
#include "GeneralFunctions.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//ATTENZIONE:	Le SendMessage presenti non possono essere trasformate in
//				PostMessages senza prima rendere statici i parametri passati
//				in quanto tutti contenuti in variabili temporanee.
/////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////////////////////////////////////////////
//          Class CStatusBarMsg implementation
/////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
CStatusBarMsg::CStatusBarMsg
	(
		bool bDisable		/* = FALSE*/, 
		bool bWait			/* = FALSE*/,
		bool bIdleMessage	/* = TRUE*/
	)
	:
	m_bWait	 		(bWait),
	m_bDisable		(bDisable),
	m_bIdleMessage	(bIdleMessage),
	m_bEnabled		(FALSE), 
	m_hwndMainWnd	(NULL), 
	m_bDisposed		(false)
{
	m_bUnattended = TRUE == AfxIsInUnattendedMode();
	if (m_bUnattended)
		return;

	if (m_bWait)
		AfxGetThread()->BeginWaitCursor();
		
	CWnd* pWnd = AfxGetMainWnd();
	if (pWnd)
	{
		m_hwndMainWnd = pWnd->m_hWnd;
		if (m_bDisable)
			m_bEnabled = !pWnd->EnableWindow(FALSE);
	}
}

//------------------------------------------------------------------------------
CStatusBarMsg::~CStatusBarMsg()
{
	Dispose();
}

//------------------------------------------------------------------------------
void CStatusBarMsg::Dispose()
{
	if (m_bDisposed)
		return;
	
	m_bDisposed = true;

	if (m_bUnattended)
		return;
	
	if (m_bWait)
	    AfxGetThread()->EndWaitCursor();

	if (m_hwndMainWnd)
	{
		if (m_bDisable && m_bEnabled)
		{
			CWnd* pWnd = CWnd::FromHandle(m_hwndMainWnd);
			if (pWnd)
				pWnd->EnableWindow(TRUE);
		}
		
		if (m_bIdleMessage)
			ShowIdle();
	}
}


//------------------------------------------------------------------------------
void CStatusBarMsg::Show (UINT nIDS, double nPerc /*= -1.0*/)
{
	if (m_bUnattended)
		return;

	CString s;
	s.LoadString(nIDS);

	return Show(s, nPerc);
}

//------------------------------------------------------------------------------
void CStatusBarMsg::Show (const CString& strText, double nPerc /*= -1.0*/)
{
	if (m_bUnattended)
		return;

	AfxSetStatusBarText(strText);

	if (nPerc != -1.0)
	{
		WORD wPerc = (WORD)nPerc;
		::SendMessage(m_hwndMainWnd, UM_PARSE_TIC, (WPARAM) wPerc, NULL);
	}
}

//------------------------------------------------------------------------------
void CStatusBarMsg::ShowIdle ()
{
	if (m_bUnattended)
		return;

	AfxClearStatusBar();
}
