
//special window to identify a sort of desktow virtual window 
#define HWND_TBMFC_SPECIAL ((HWND) -20)
#define UM_REQUEST_INFO WM_USER + 1
#define UM_CHANGE_WINDOW_PROPERTIES WM_USER + 2

//-----------------------------------------------------------------------------
//carica e attiva la dll di hooking delle api di windows
inline void ActivateApiHooking()
{
	HINSTANCE h = AfxLoadLibrary(_T("TbApiHooks.dll"));
	if (h)
	{
		typedef void (*fptr)();
		fptr pFun = (fptr)GetProcAddress(h, "BeginRedirect");
		ASSERT(pFun);
		pFun();
	}
}


//-----------------------------------------------------------------------------
//abilita o disabilita il thread corrente all'utilizzo dei meccanismi di hooking
inline void ActivateThreadHooks(BOOL bActivate)
{
	HINSTANCE h = GetModuleHandle(_T("TbApiHooks.dll"));
	if (h)
	{
		typedef void (*fptr)(bool);
		fptr pFun = (fptr)GetProcAddress(h, "ActivateThreadHooks");
		ASSERT(pFun);
		pFun(bActivate==TRUE);
	}
}

enum ThreadHooksState { DEFAULT, ACTIVE, INACTIVE };
