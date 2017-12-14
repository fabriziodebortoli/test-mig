#pragma once
#define SIZE 6

class CHookedFunction
{
private:
	void* m_pDetour;
	CString m_sModule;
	CStringA m_sFunction;
	BOOL m_bHooked;
public:
	void* m_pOrigFunctionAddress;
	CHookedFunction(void* pOriginFunctionAddress, LPVOID pNewFunctionAddress);
	CHookedFunction(LPCTSTR lpszModule, LPCSTR lpszFunction, LPVOID pNewFunctionAddress);
	~CHookedFunction();
	void BeginRedirect();
	
};
class CHookedFunctionArray : public CArray<CHookedFunction*>
{
};

void DebugHook();
#ifdef DEBUG
#define DEBUG_HOOK() DebugHook();
#else
#define DEBUG_HOOK() 
#endif

#define EXTERN_HOOK_LIB(theRet, theFunction, theArgs) extern CHookedFunction m_##theFunction;\
	typedef theRet (WINAPI *p##theFunction)theArgs;

#define HOOK_LIB(theRet, theFunction, args) \
	CHookedFunction m_##theFunction(theFunction, _TBNAME(theFunction));\
typedef theRet (WINAPI *p##theFunction)args;\
theRet WINAPI _TBNAME(theFunction) args { DEBUG_HOOK()




#define LIB_ORIGINAL(theFunction) 	((p##theFunction)m_##theFunction.m_pOrigFunctionAddress)


#define HOOK_USER32(theRet, theFunction, args)			HOOK_LIB(theRet, theFunction, args)
#define USER32_ORIGINAL(theFunction)					LIB_ORIGINAL(theFunction)

#define HOOK_COMCTL32(theRet, theFunction, args)		HOOK_LIB_STR(theRet, comctl32.dll, theFunction, args)
#define COMCTL32_ORIGINAL(theFunction) 					LIB_ORIGINAL(theFunction)

#define HOOK_GDI32(theRet, theFunction, args)			HOOK_LIB(theRet, theFunction, args)
#define GDI32_ORIGINAL(theFunction) 					LIB_ORIGINAL(theFunction)
