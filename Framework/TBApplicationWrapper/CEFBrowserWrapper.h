#pragma once

class CBrowserEventsObj;
class CBrowserObj;
public ref class RequestEventArgs : public EventArgs
{
public:
	bool Handled;
	String^ Url;
	String^ Response;
	String^ ResponseType;
public:
};
public ref class CCEFBrowserWrapper
{
public:
	virtual event EventHandler^ BrowserReady;
	virtual event EventHandler^ BrowserClosing;
	virtual event EventHandler<RequestEventArgs^>^ Request;
	CBrowserEventsObj* m_pBrowserEvents;
	CBrowserObj* m_pBrowser;
	CCEFBrowserWrapper(void);
	virtual ~CCEFBrowserWrapper(void);
	!CCEFBrowserWrapper();
	property bool BrowserAlive { bool get() { return m_pBrowser != NULL; }}
	void Navigate			(System::String^ url);
	void SetCookie			(System::String^ url, System::String^ name, System::String^ value);
	void AdjustPosition		(int w, int h);
	void Create				(IntPtr parentWindowHandle, String^ initialUrl);
	void FireBrowserReady	(){ BrowserReady(this, EventArgs::Empty); }
	void FireRequest(RequestEventArgs^ args)		{ Request(this, args); }
	void DoBrowserClosing	(){ delete m_pBrowserEvents; BrowserClosing(this, EventArgs::Empty); }
	void ExecuteJavascript	(System::String^ code);
	void CloseBrowser		(bool forceClose);
	void Reload				();
};

