#include "stdafx.h"

#include <TbNameSolver\ThreadContext.h>
#include "EnumsViewerWrapper.h"

using namespace System;
using namespace Microarea::TaskBuilderNet::Core::ApplicationsWinUI::EnumsViewer;

IMPLEMENT_DYNCREATE(EnumsViewerWrapper, CObject)

//---------------------------------------------------------------------------------------
//  consente di rimuovere l'handle della finestra dal thread context
void OnClosing(System::Object^ sender, System::EventArgs^ args)
{
	HWND hWnd = (HWND)(int)EnumsViewerManager::GetWindowHandle();
	if (hWnd)
	{
		RemoveThreadWindowRef(hWnd, false);
		EnumsViewerManager::Closing -= gcnew System::EventHandler(&OnClosing);
		AfxGetApplicationContext()->SetEnumsViewerThreadOpened(FALSE);
	}
}


//---------------------------------------------------------------------------------------
bool EnumsViewerWrapper::Open(CString aCulture, CString aInstallation)
{
	// e' già istanziato
	if (AfxGetApplicationContext()->IsEnumsViewerThreadOpened())
		return true;

	String^ culture = gcnew String(aCulture);
	String^ installation = gcnew String(aInstallation);

	// semaforizzo subito
	AfxGetApplicationContext()->SetEnumsViewerThreadOpened(TRUE);

	bool ok = EnumsViewerManager::Open(culture, installation, true);
	if (ok)
	{
		HWND hWnd = (HWND)(int)EnumsViewerManager::GetWindowHandle();
		AddThreadWindowRef(hWnd, false);
		EnumsViewerManager::Closing += gcnew System::EventHandler(&OnClosing);
	}
	else
		AfxGetApplicationContext()->SetEnumsViewerThreadOpened(FALSE);
	return ok;
}

//---------------------------------------------------------------------------------------
bool EnumsViewerWrapper::Close	() 
{ 
	return EnumsViewerManager::Close(); 
}

//---------------------------------------------------------------------------------------
bool EnumsViewerWrapper::IsClosed () 
{ 
	return EnumsViewerManager::IsClosed(); 
}
