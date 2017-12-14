#include "StdAfx.h"
#include "HookedFunction.h"
#include "detours.h"


//function used to set a breakpoint in each hooked function for debugging purposes
//eventually conditioned by thread id
void DebugHook()
{
//	DWORD id = GetCurrentThreadId();
//	BOOL b = UseStandardAPIS();
}

CHookedFunction::CHookedFunction(void* pOriginFunctionAddress, LPVOID pNewFunctionAddress) 
{
	m_pOrigFunctionAddress = pOriginFunctionAddress;
	m_pDetour = pNewFunctionAddress;
	m_bHooked = FALSE;
	GetHooks().Add(this);

}

CHookedFunction::CHookedFunction(LPCTSTR lpszModule, LPCSTR lpszFunction, LPVOID pNewFunctionAddress)
{
	m_sModule = lpszModule;
	m_sFunction = lpszFunction;
	m_pDetour = pNewFunctionAddress;
	m_pOrigFunctionAddress = NULL;
	m_bHooked = FALSE;
	GetHooks().Add(this);
}
CHookedFunction::~CHookedFunction()
{
	if (m_bHooked)
	{
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourDetach(&m_pOrigFunctionAddress, m_pDetour);
		VERIFY(NO_ERROR == DetourTransactionCommit());
	}
}
void CHookedFunction::BeginRedirect()
{
	if (!m_pOrigFunctionAddress)
	{
		m_pOrigFunctionAddress = GetProcAddress(GetModuleHandle(m_sModule), m_sFunction);
	}
	if (!m_pOrigFunctionAddress)
	{
		ASSERT(FALSE);
		return;
	}
	DetourAttach(&m_pOrigFunctionAddress, m_pDetour);
	m_bHooked = TRUE;
}

int MyReportHook( int reportType, char *message, int *returnValue )
{
	if (reportType != _CRT_ASSERT)
		return FALSE;
	AtlTrace(message);
	*returnValue = TRUE;
	return TRUE;
}
extern "C" __declspec(dllexport) void BeginRedirect()
{
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());

	for (int i = 0; i < GetHooks().GetCount(); i++)
	{
		GetHooks()[i]->BeginRedirect();
	}
	VERIFY(NO_ERROR == DetourTransactionCommit());

	_CrtSetReportHook(MyReportHook);

}
extern "C" __declspec(dllexport) void HookFunction(void** ppOrigFunctionAddress, void* pNewFunctionAddress)
{
	DetourTransactionBegin();
	DetourUpdateThread(GetCurrentThread());

	DetourAttach(ppOrigFunctionAddress, pNewFunctionAddress);
	VERIFY(NO_ERROR == DetourTransactionCommit());
}
extern "C" __declspec(dllexport) void  ActivateThreadHooks(bool bActivate)
{
	GetThreadContext()->m_bUseStandardApis = !bActivate;
}