// CEFSubProcess.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "TbCEFProcess.h"
#include <include\cef_client.h>
#include <include\cef_app.h>




class SimpleApp : public CefApp,
	public CefBrowserProcessHandler {
public:
	SimpleApp() {}

	// CefApp methods:
	virtual CefRefPtr<CefBrowserProcessHandler> GetBrowserProcessHandler()
		OVERRIDE { return this; }

	// CefBrowserProcessHandler methods:
	virtual void OnContextInitialized() OVERRIDE {}

private:
	// Include the default reference counting implementation.
	IMPLEMENT_REFCOUNTING(SimpleApp);


};


int APIENTRY _tWinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPTSTR    lpCmdLine,
                     int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

// Structure for passing command-line arguments.
  // The definition of this structure is platform-specific.
	CefMainArgs main_args(hInstance);

  // Optional implementation of the CefApp interface.
  CefRefPtr<SimpleApp> app(new SimpleApp);

  // Execute the sub-process logic. This will block until the sub-process should exit.
  return CefExecuteProcess(main_args, app.get(), NULL);
}

