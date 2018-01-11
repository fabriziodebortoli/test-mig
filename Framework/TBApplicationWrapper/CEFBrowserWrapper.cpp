#include "StdAfx.h"
using namespace System;

#include <TbGenlib\CEFClasses.h>
#include "CEFBrowserWrapper.h"

//=================================================================================================
class CBrowserEvents : public CBrowserEventsObj
{
	gcroot<CCEFBrowserWrapper^> m_pWrapper;
public:
	//---------------------------------------------------------------------------------------------
	CBrowserEvents (CCEFBrowserWrapper^ pWrapper)
		: m_pWrapper(pWrapper)
	{
	}
	
	//---------------------------------------------------------------------------------------------
	virtual void OnAfterCreated (CBrowserObj* pBrowser)
	{
		m_pWrapper->m_pBrowser = pBrowser;
		m_pWrapper->FireBrowserReady();
	}
	
	//---------------------------------------------------------------------------------------------
	virtual void OnBeforeClose (CBrowserObj* pBrowser)
	{
		m_pWrapper->m_pBrowser = NULL;
		m_pWrapper->DoBrowserClosing();

	};

	//---------------------------------------------------------------------------------------------
	virtual void DoClose (CBrowserObj* pBrowser)
	{
		//prima della chiusura devo staccarmi dal parent, altrimenti CEF chiude anche il menumanager
		HWND hwnd = pBrowser ? pBrowser->GetMainWnd() : NULL;
		if (hwnd)
		{
			CWnd::FromHandle(hwnd)->SetParent(NULL);
		};
	};
	virtual bool OnRequest(LPCTSTR lpszUrl, CTBResponse& aResponse)
	{
		RequestEventArgs^ args = gcnew RequestEventArgs();
		args->Url = gcnew String(lpszUrl);
		m_pWrapper->FireRequest(args);
		
		if (!args->Handled)
			return false;

		aResponse.SetData(args->Response);
		aResponse.SetMimeType(CString(args->ResponseType));
		return true;
	}
};

//---------------------------------------------------------------------------------------------
CCEFBrowserWrapper::CCEFBrowserWrapper (void) 
	: m_pBrowser(NULL)
{
	m_pBrowserEvents = NULL;
}

//---------------------------------------------------------------------------------------------
CCEFBrowserWrapper::~CCEFBrowserWrapper (void)
{
	this->!CCEFBrowserWrapper();
	GC::SuppressFinalize(this);
}

//---------------------------------------------------------------------------------------------
CCEFBrowserWrapper::!CCEFBrowserWrapper (void)
{
	CloseBrowser(true);
}
void CCEFBrowserWrapper::CloseBrowser(bool forceClose)
{
	if (m_pBrowser)
		m_pBrowser->DoClose(forceClose);
}

//---------------------------------------------------------------------------------------------
void CCEFBrowserWrapper::Create (IntPtr parentWindowHandle, String^ initialUrl)
{	
	m_pBrowserEvents = new CBrowserEvents(this);
	VERIFY(CreateChildBrowser((HWND)parentWindowHandle.ToInt64(), CString(initialUrl), FALSE,  m_pBrowserEvents));
}

//---------------------------------------------------------------------------------------------
void CCEFBrowserWrapper::Navigate (String^ url)
{
	if( m_pBrowser && !String::IsNullOrEmpty(url) )
		m_pBrowser->Navigate (url);
}

//---------------------------------------------------------------------------------------------
void CCEFBrowserWrapper::SetCookie (String^ url, String^ name, String^ value)
{
	ASSERT(m_pBrowser);
	m_pBrowser->SetCookie(url, name, value);
}

//---------------------------------------------------------------------------------------------
void CCEFBrowserWrapper::AdjustPosition (int w, int h)
{
	HWND hwnd = m_pBrowser ? m_pBrowser->GetMainWnd() : NULL;
	if (w == 0 && h == 0)
		return;

	if (hwnd)
		CWnd::FromHandle(hwnd)->SetWindowPos (NULL, 0, 0, w, h, SWP_NOACTIVATE | SWP_NOZORDER);
}

void CCEFBrowserWrapper::ExecuteJavascript(System::String^ code)
{
	if (m_pBrowser)
		m_pBrowser->ExecuteJavascript(code);
}

void CCEFBrowserWrapper::Reload() 
{
	if (m_pBrowser)
		m_pBrowser->Reload();
}
