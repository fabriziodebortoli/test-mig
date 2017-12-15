// TbGes.cpp : definisce le routine di inizializzazione per la DLL.
//

#include "stdafx.h"
#include <afxdllx.h>
#include <TbGeneric\spin.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

static AFX_EXTENSION_MODULE TbGesDLL = { NULL, NULL };

extern "C" void APIENTRY InitDynLinkLibrary()
{
	new CDynLinkLibrary(TbGesDLL);
}

extern "C" int APIENTRY
DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	// Rimuovere questa riga se si utilizza lpReserved
	UNREFERENCED_PARAMETER(lpReserved);

	if (dwReason == DLL_PROCESS_ATTACH)
	{
		TRACE0("TbGes.DLL Initializing\n");
		
		// Inizializzazione unica DLL di estensione
		if (!AfxInitExtensionModule(TbGesDLL, hInstance))
			return 0;
		
		// le due istruzioni sotto DEVONO essere messe nella DllMain
		if (!RegisterSpinCtrl(hInstance))
		{
			TRACE0("TbGenlib.dll <ItriSpinCtrl> registration failed!\n");
			return 0;
		}

		// Inserire la DLL nella catena di risorse
		// NOTA: se questa DLL di estensione viene collegata in modo implicito
		//  a una DLL regolare MFC (quale un controllo ActiveX)
		//  anzich� a un'applicazione MFC, � possibile
		//  rimuovere questa riga da DllMain e inserirla in una funzione
		//  separata esportata dalla DLL di estensione. La DLL regolare
		//  che utilizza questa DLL di estensione chiamer� quindi in modo esplicito
		//  tale funzione per inizializzare la DLL di estensione.  In caso contrario,
		//  l'oggetto CDynLinkLibrary non verr� collegato
		//  alla catena di risorse della DLL regolare, generando gravi
		//  problemi.


		new CDynLinkLibrary(TbGesDLL);

	}
	else if (dwReason == DLL_PROCESS_DETACH)
	{
		TRACE0("TbGes.DLL Terminating\n");

		// Terminare la libreria prima della chiamata ai distruttori
		AfxTermExtensionModule(TbGesDLL);
	}
	return 1;   // ok
}