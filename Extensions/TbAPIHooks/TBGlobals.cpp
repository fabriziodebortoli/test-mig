#include "stdafx.h"
#include "HookedFunction.h"

#include <vector>

EXTERN_HOOK_LIB(HWND, CreateDialogIndirectParamW, (HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM dwInitParam))
// questa classe costruisce dinamicamente un template di risorsa, equilvalente alla build di un RC, da usare con la CreateDialogIndirect
class DialogTemplate {
public:
	LPCDLGTEMPLATE Template(int & size)
	{
		size = v.size();
		return (LPCDLGTEMPLATE)&v[0];
	}
	void AlignToDword()
	{
		if (v.size() % 4) Write(NULL, 4 - (v.size() % 4));
	}
	void Write(LPCVOID pvWrite, DWORD cbWrite) {
		v.insert(v.end(), cbWrite, 0);
		if (pvWrite) CopyMemory(&v[v.size() - cbWrite], pvWrite, cbWrite);
	}
	template<typename T> void Write(T t) { Write(&t, sizeof(T)); }
	void WriteString(LPCWSTR psz)
	{
		Write(psz, (lstrlenW(psz) + 1) * sizeof(WCHAR));
	}

private:
	std::vector<BYTE> v;
};


#define IMPLEMENT_GLOBAL_MAP(type)\
	type& Get_##type##()\
{\
	static type s;\
	return s;\
}\
	CCriticalSection* Get_##type##Section()\
{\
	static CCriticalSection s;\
	return &s;\
}\
	LONG Get_New_##type()\
{\
	static LONG s = 1000;\
	return InterlockedIncrement(&s);\
}

IMPLEMENT_GLOBAL_MAP(CWindowMap)
IMPLEMENT_GLOBAL_MAP(CDeferMap)
IMPLEMENT_GLOBAL_MAP(CQueueMap)
IMPLEMENT_GLOBAL_MAP(CImageListMap)
IMPLEMENT_GLOBAL_MAP(CHDCToTBDCMap)
IMPLEMENT_GLOBAL_MAP(CMenuMap)
IMPLEMENT_GLOBAL_MAP(CImageMap)
IMPLEMENT_GLOBAL_MAP(RequestMap)
IMPLEMENT_GLOBAL_MAP(BaseUnitsMap)


CHGDIOBJMap& GetGDIMap()
{
	static CHGDIOBJMap s;
	return s;
}

//units used to convert dialog units to screen units
//in real world they depends on the font, but in this scenario
//I think it's not worth to care about it
//the client will care of rendering
//I calculate them using the system font
int BASE_UNIT_X;
int BASE_UNIT_Y;

//API HOOKATE da TB
CHookedFunctionArray& GetHooks()
{
	static TBSpecial g_DesktopWnd(::GetCurrentThreadId());//special desktop window, owner of all other windows
	static CHookedFunctionArray g_Hooks;
	LONG l = GetDialogBaseUnits();
	BASE_UNIT_X = LOWORD(l);
	BASE_UNIT_Y = HIWORD(l);
	return g_Hooks;
}

//-----------------------------------------------------------------------------
void GetBaseUnits(int &baseUnitX, int& baseUnitY, LPCTSTR szFontFace, int wPoint)
{
	TCHAR buff[4];
	CString sKey = CString(szFontFace) + (TCHAR*)_itot_s(wPoint, buff, 10);
	CSingleLock l(Get_BaseUnitsMapSection(), TRUE);
	LONG units;
	if (Get_BaseUnitsMap().Lookup(sKey, units))
	{
		baseUnitX = LOWORD(units);
		baseUnitY = HIWORD(units);
		return;
	}
	//creo una finestra col font del tema e mi faccio trasformare le coordinate in base ad esso
	DialogTemplate tmp;
	// Write out the extended dialog template header
	tmp.Write<WORD>(1); // dialog version
	tmp.Write<WORD>(0xFFFF); // extended dialog template
	tmp.Write<DWORD>(0); // help ID
	tmp.Write<DWORD>(0); // extended style
	tmp.Write<DWORD>(DS_SETFONT);
	tmp.Write<WORD>(0); // number of controls
	tmp.Write<WORD>((WORD)0); // X
	tmp.Write<WORD>((WORD)0); // Y
	tmp.Write<WORD>(1); // width
	tmp.Write<WORD>(1); // height
	tmp.WriteString(L""); // no menu
	tmp.WriteString(L""); // default dialog class
	tmp.WriteString(L""); // title


	tmp.Write<WORD>(wPoint); // point
	tmp.Write<WORD>((WORD)0); // weight
	tmp.Write<BYTE>(0); // Italic
	tmp.Write<BYTE>(0); // CharSet
	tmp.WriteString(szFontFace);
	int size;
	LPCDLGTEMPLATE t = tmp.Template(size);
	HWND hMappingWindow = LIB_ORIGINAL(CreateDialogIndirectParamW)(NULL, t, NULL, NULL, NULL);

	RECT rc;
	rc.left = 0;
	rc.top = 0;
	rc.right = 4;
	rc.bottom = 8;
	::MapDialogRect(hMappingWindow, &rc);
	baseUnitY = rc.bottom;
	baseUnitX = rc.right;
	DestroyWindow(hMappingWindow);

	units = MAKELONG(baseUnitX, baseUnitY);
	Get_BaseUnitsMap().SetAt(sKey, units);
}
//usato er lo HOOK di SetWindowsHookExW
LONG g_LastHook = 1000;
HHOOK GetNewHOOK()
{
	return (HHOOK)InterlockedIncrement(&g_LastHook);
}

bool UseStandardAPIS()
{
	//se non sono in un thread MCF, allora non sono nel motore di TB e quindi niente API hookate
	return GetThreadContext()->m_bUseStandardApis;
}


