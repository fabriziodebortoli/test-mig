#pragma once

class CWnd;
//===========================================================================
//							ManagedEventsHandler
// ref class that manages communication between Event Handlers and C++ CWnd
//===========================================================================
public ref class ManagedEventsHandlerObj : public System::Object
{	
private:
	CWnd*	m_pControlWnd;
	CWnd*	m_pParentToNotify;

public:
	//---------------------------------------------------------------------------------------
	ManagedEventsHandlerObj () 
	:
	m_pControlWnd		(NULL),
	m_pParentToNotify	(NULL)
	{
	}

	~ManagedEventsHandlerObj ()
	{
		m_pControlWnd		= NULL;
		m_pParentToNotify	=  NULL;
	}
public:
	CWnd* GetControlWnd () { return m_pControlWnd; }


	void AttachWindow (CWnd* pWnd)
	{
		m_pControlWnd = pWnd; 
		m_pParentToNotify = NULL;
	}

	void AttachParent (CWnd* pWnd)
	{
		m_pControlWnd = NULL; 
		m_pParentToNotify = pWnd;
	}

	virtual void MapEvents (System::Object^ pControl) {};

protected:
	CWnd* GetParentToNotify ()
	{
		if (m_pParentToNotify)
			return m_pParentToNotify;

		if (!m_pControlWnd)
			return NULL;
		m_pParentToNotify = m_pControlWnd->GetParent();

		return m_pParentToNotify;
	}

	void PostAsCommand (UINT nMessageID)
	{
		CWnd* pParent = GetParentToNotify();
		if (!pParent)
			return;

		pParent->PostMessage(WM_COMMAND, nMessageID);
	}

	void SendAsCommand (UINT nMessageID)
	{
		CWnd* pParent = GetParentToNotify();
		if (!pParent)
			return;

		pParent->SendMessage(WM_COMMAND, nMessageID);
	}

	void PostAsControl (UINT nMessageID)
	{
		CWnd* pParent = GetParentToNotify();
		if (!pParent)
			return;

		pParent->PostMessage(WM_COMMAND, (WPARAM)MAKELONG(m_pControlWnd->GetDlgCtrlID(), nMessageID), (LPARAM)(pParent->m_hWnd));
	}

	void SendAsControl (UINT nMessageID)
	{
		CWnd* pParent = GetParentToNotify();
		if (!pParent)
			return;

		pParent->SendMessage(WM_COMMAND, (WPARAM)MAKELONG(m_pControlWnd->GetDlgCtrlID(), nMessageID), (LPARAM)(pParent->m_hWnd));
	}

	void PostAsMessage (UINT nMessageID, UnmanagedEventsArgs& eventArgs)
	{
		CWnd* pParent = GetParentToNotify();
		if (!pParent)
			return;

		pParent->PostMessage(nMessageID, m_pControlWnd->GetDlgCtrlID(), (LPARAM) &eventArgs);
	}

	void SendAsMessage (UINT nMessageID, UnmanagedEventsArgs& eventArgs)
	{
		CWnd* pParent = GetParentToNotify();
		if (!pParent)
			return;

		pParent->SendMessage(nMessageID, m_pControlWnd->GetDlgCtrlID(), (LPARAM) &eventArgs);
	}

public:
	void HandleDestroyed (System::Object^ sender, System::EventArgs^ eventArgs)
	{
		m_pControlWnd->Detach();
	}
};

