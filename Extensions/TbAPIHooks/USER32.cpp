#include "stdafx.h"

#include <atlwin.h>

#include "TBWND.h"
#include "TBDialog.h"
#include "TBMessageQueue.h"
#include "TBDC.h"
#include "USER32.h"
#include "TBComboBox.h"
#include "HookedFunction.h"

struct DLGTEMPLATEEX
{
	WORD dlgVer;
	WORD signature;
	DWORD helpID;
	DWORD exStyle;
	DWORD style;
	WORD cDlgItems;
	short x;
	short y;
	short cx;
	short cy;

	// Everything else in this structure is variable length,
	// and therefore must be determined dynamically

	// sz_Or_Ord menu;			// name or ordinal of a menu resource
	// sz_Or_Ord windowClass;	// name or ordinal of a window class
	// WCHAR title[titleLen];	// title string of the dialog box
	// short pointsize;			// only if DS_SETFONT is set
	// short weight;			// only if DS_SETFONT is set
	// short bItalic;			// only if DS_SETFONT is set
	// WCHAR font[fontLen];		// typeface name, if DS_SETFONT is set
};
struct DLGITEMTEMPLATEEX
{
	DWORD helpID;
	DWORD exStyle;
	DWORD style;
	short x;
	short y;
	short cx;
	short cy;
	DWORD id;

	// Everything else in this structure is variable length,
	// and therefore must be determined dynamically

	// sz_Or_Ord windowClass;	// name or ordinal of a window class
	// sz_Or_Ord title;			// title string or ordinal of a resource
	// WORD extraCount;			// bytes following creation data
};


TBWnd* InternalCreateDialogIndirect(__in_opt HINSTANCE hInstance, __in LPCDLGTEMPLATE lpTemplate, __in_opt HWND hWndParent, __in_opt DLGPROC lpDialogFunc, __in LPARAM dwInitParam)
{
	TBDialog* pWnd = (TBDialog*)_TBCreateWindow(
		lpTemplate->dwExtendedStyle,
		L"#32770",
		L"",
		lpTemplate->style,
		0,
		0,
		0,
		0,
		0,
		hWndParent,
		NULL,
		hInstance,
		(LPVOID)dwInitParam,
		(WNDPROC)lpDialogFunc
		);
	if (pWnd == NULL)
		return NULL;
	CString strFace;
	WORD wSize = 0;
	if (CDialogTemplate::GetFont((DLGTEMPLATE*)lpTemplate, strFace, wSize))
		pWnd->CalculateBaseUnits(strFace, wSize);

	pWnd->SetRectFromDialogUnits(CRect(lpTemplate->x, lpTemplate->y, lpTemplate->x + lpTemplate->cx, lpTemplate->y + lpTemplate->cy));

	DLGITEMTEMPLATE *pItem = _DialogSplitHelper::FindFirstDlgItem(lpTemplate);
	for (int i = 0; i < _DialogSplitHelper::DlgTemplateItemCount(lpTemplate); i++)
	{
		LPWSTR pszFirstAfterEnd = (LPWSTR)(pItem + 1);
		LPWSTR pszClassName = _T(""), pszText = _T("");

		if (*pszFirstAfterEnd == 0xFFFF)//carattere speciale?
		{
			pszFirstAfterEnd++;//il successivo è il codice della classe di finestra
			switch (*pszFirstAfterEnd)
			{
			case 0x0080: pszClassName = _T("Button"); break;
			case 0x0081: pszClassName = _T("Edit"); break;
			case 0x0082: pszClassName = _T("Static"); break;
			case 0x0083: pszClassName = _T("ListBox"); break;
			case 0x0084: pszClassName = _T("ScrollBar"); break;//TODO è giusta?
			case 0x0085: pszClassName = _T("ComboBox"); break;
			}
			pszFirstAfterEnd++;//punto all'inizio del titolo
		}
		else
		{
			//non è speciale? allora contiene il nome della classe
			pszClassName = pszFirstAfterEnd;
			//mi sposto alla fine della stringa
			pszFirstAfterEnd += _tcslen(pszClassName);
		}

		if (*pszFirstAfterEnd == 0xFFFF)//carattere speciale?
		{
			//il successivo è il codice della risorsa
			pszFirstAfterEnd++;
			ASSERT(FALSE);
			pszText = _T("");//TODO leggere da risorsa
		}
		else
		{
			//non è speciale? allora contiene il testo della finestra
			pszText = pszFirstAfterEnd;
		}


		TBWnd* p = _TBCreateWindow(
			pItem->dwExtendedStyle,
			pszClassName,
			L"",
			pItem->style,
			pItem->id,
			0,
			0,
			0,
			0,
			pWnd->GetHWND(),
			NULL,
			hInstance,
			NULL,
			NULL);
		if (p == NULL)
			return NULL;

		p->SendMessage(WM_SETTEXT, NULL, (LPARAM)pszText);
		p->SetRectFromDialogUnits(CRect(pItem->x, pItem->y, pItem->x + pItem->cx, pItem->y + pItem->cy));

		pItem = _DialogSplitHelper::FindNextDlgItem(pItem, FALSE);

	}
	return pWnd;
}

TBWnd* InternalCreateDialogIndirect(HINSTANCE hInstance, DLGTEMPLATEEX* lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM dwInitParam)
{
	TBDialog* pWnd = (TBDialog*)_TBCreateWindow(
		lpTemplate->exStyle,
		L"#32770",
		L"",
		lpTemplate->style,
		0,
		0,
		0,
		0,
		0,
		hWndParent,
		NULL,
		hInstance,
		(LPVOID)dwInitParam,
		(WNDPROC)lpDialogFunc
		);
	if (pWnd == NULL)
		return NULL;

	CString strFace;
	WORD wSize = 0;
	if (CDialogTemplate::GetFont((DLGTEMPLATE*)lpTemplate, strFace, wSize))
		pWnd->CalculateBaseUnits(strFace, wSize);

	pWnd->SetRectFromDialogUnits(CRect(lpTemplate->x, lpTemplate->y, lpTemplate->x + lpTemplate->cx, lpTemplate->y + lpTemplate->cy));

	DLGITEMTEMPLATEEX *pItem = (DLGITEMTEMPLATEEX*)_DialogSplitHelper::FindFirstDlgItem((DLGTEMPLATE*)lpTemplate);
	for (int i = 0; i < _DialogSplitHelper::DlgTemplateItemCount((DLGTEMPLATE*)lpTemplate); i++)
	{
		LPWSTR pszFirstAfterEnd = (LPWSTR)(((DLGITEMTEMPLATEEX*)pItem) + 1);
		LPWSTR pszClassName = _T(""), pszText = _T("");

		if (*pszFirstAfterEnd == 0xFFFF)//carattere speciale?
		{
			pszFirstAfterEnd++;//il successivo è il codice della classe di finestra
			switch (*pszFirstAfterEnd)
			{
			case 0x0080: pszClassName = _T("Button"); break;
			case 0x0081: pszClassName = _T("Edit"); break;
			case 0x0082: pszClassName = _T("Static"); break;
			case 0x0083: pszClassName = _T("ListBox"); break;
			case 0x0084: pszClassName = _T("ScrollBar"); break;//TODO è giusta?
			case 0x0085: pszClassName = _T("ComboBox"); break;
			}
			pszFirstAfterEnd++;//punto all'inizio del titolo
		}
		else
		{
			//non è speciale? allora contiene il nome della classe
			pszClassName = pszFirstAfterEnd;
			//mi sposto alla fine della stringa
			pszFirstAfterEnd += _tcslen(pszClassName);
		}

		if (*pszFirstAfterEnd == 0xFFFF)//carattere speciale?
		{
			//il successivo è il codice della risorsa
			pszFirstAfterEnd++;
			ASSERT(FALSE);
			pszText = _T("");//TODO leggere da risorsa
		}
		else
		{
			//non è speciale? allora contiene il testo della finestra
			pszText = pszFirstAfterEnd;
		}


		TBWnd* p = _TBCreateWindow(
			pItem->exStyle,
			pszClassName,
			L"",
			pItem->style,
			pItem->id,
			pItem->x,
			pItem->y,
			pItem->cx,
			pItem->cy,
			pWnd->GetHWND(),
			NULL,
			hInstance,
			NULL,
			NULL);
		if (p == NULL)
			return NULL;

		p->SendMessage(WM_SETTEXT, NULL, (LPARAM)pszText);

		p->SetRectFromDialogUnits(CRect(pItem->x, pItem->y, pItem->x + pItem->cx, pItem->y + pItem->cy));
		pItem = (DLGITEMTEMPLATEEX*)_DialogSplitHelper::FindNextDlgItem((DLGITEMTEMPLATE*)pItem, TRUE);

	}
	return pWnd;
}


//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HHOOK, SetWindowsHookExW, (__in int idHook, __in HOOKPROC lpfn, __in_opt HINSTANCE hmod, __in DWORD dwThreadId))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(SetWindowsHookExW)(idHook, lpfn, hmod, dwThreadId);
}
CMessageQueue* pQueue = GetMessageQueue(dwThreadId);
return pQueue->AddHook(idHook, lpfn, hmod);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, UnhookWindowsHookEx, (__in HHOOK hhk))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(UnhookWindowsHookEx)(hhk);
}
CMessageQueue* pQueue = GetMessageQueue(GetCurrentThreadId());
return pQueue->RemoveHook(hhk);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(LRESULT, CallNextHookEx, (__in_opt HHOOK hhk, __in int nCode, __in WPARAM wParam, __in LPARAM lParam))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(CallNextHookEx)(hhk, nCode, wParam, lParam);
}
return 1L;
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, GetMessageW, (__out LPMSG lpMsg, __in_opt HWND hWnd, __in UINT wMsgFilterMin, __in UINT wMsgFilterMax))
if (UseStandardAPIS())//se il thread non ha una coda di messaggi custom, uso l'API originaria
{
	return USER32_ORIGINAL(GetMessageW)(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
}
CMessageQueue* pQueue = GetMessageQueue(GetCurrentThreadId());
return pQueue->GetMessage(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, TranslateMessage, (__in CONST MSG *lpMsg))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(TranslateMessage)(lpMsg);
}
CMessageQueue* pQueue = GetMessageQueue(GetCurrentThreadId());
return pQueue->TranslateMessage(lpMsg);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, PostThreadMessageW, (DWORD dwThreadId, UINT message, WPARAM wParam, LPARAM lParam))
if (UseStandardAPIS())//se il thread non ha una coda di messaggi custom, uso l'API originaria
{
	return USER32_ORIGINAL(PostThreadMessageW)(dwThreadId, message, wParam, lParam);
}
CMessageQueue* pQueue = GetMessageQueue(dwThreadId);
MSG* pMsg = CreateMessage(NULL, message, wParam, lParam);
return pQueue->PostMessage(pMsg);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(void, PostQuitMessage, (int exitCode))
PostThreadMessageW(GetCurrentThreadId(), WM_QUIT, exitCode, NULL);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, PeekMessageW, (__out LPMSG lpMsg, __in_opt HWND hWnd, __in UINT wMsgFilterMin, __in UINT wMsgFilterMax, __in UINT wRemoveMsg))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(PeekMessageW)(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
}
CMessageQueue* pQueue = GetMessageQueue(GetCurrentThreadId());
return pQueue->MyPeekMessage(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, IsDialogMessageW, (_In_  HWND hDlg, _In_  LPMSG lpMsg))
TBWnd* pWnd = GetTBWnd(hDlg);
if (pWnd)
return pWnd->IsDialogMessage(lpMsg);

return USER32_ORIGINAL(IsDialogMessageW)(hDlg, lpMsg);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(LRESULT, DispatchMessageW, (__in CONST MSG *lpMsg))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(DispatchMessageW)(lpMsg);
}
CMessageQueue* pQueue = GetMessageQueue(GetCurrentThreadId());
return pQueue->DispatchMessage(lpMsg, GetTBWnd(lpMsg->hwnd));
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, CreateDialogIndirectParamW, (HINSTANCE hInstance, LPCDLGTEMPLATE lpTemplate, HWND hWndParent, DLGPROC lpDialogFunc, LPARAM dwInitParam))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(CreateDialogIndirectParamW)(hInstance, lpTemplate, hWndParent, lpDialogFunc, dwInitParam);
}

TBWnd* pWnd = _DialogSplitHelper::IsDialogEx(lpTemplate)
? InternalCreateDialogIndirect(hInstance, (DLGTEMPLATEEX*)lpTemplate, hWndParent, lpDialogFunc, dwInitParam)
: InternalCreateDialogIndirect(hInstance, lpTemplate, hWndParent, lpDialogFunc, dwInitParam);

if (SendMessage(pWnd->GetHWND(), WM_INITDIALOG, /*TODO handle che deve ricevere il fuoco*/NULL, dwInitParam))
{
	//TODO set focus
}
return pWnd->GetHWND();
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, CreateWindowExW, (DWORD dwExStyle, LPCWSTR lpClassName, LPCWSTR lpWindowName, DWORD dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, LPVOID lpParam))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(CreateWindowExW)(dwExStyle, lpClassName, lpWindowName, dwStyle, X, Y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
}

TBWnd* pWnd = _TBCreateWindow(dwExStyle, lpClassName, lpWindowName, dwStyle, (int)hMenu, X, Y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam, NULL);

if (!pWnd)
return NULL;
return pWnd->GetHWND();
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, MessageBoxW, (HWND hWnd, LPCWSTR lpText, LPCWSTR lpCaption, UINT uType))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(MessageBoxW)(hWnd, lpText, lpCaption, uType);
}
return MB_OK;
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, MessageBoxA, (HWND hWnd, LPCSTR lpText, LPCSTR lpCaption, UINT uType))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(MessageBoxA)(hWnd, lpText, lpCaption, uType);
}
return MB_OK;
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(LRESULT, CallWindowProcW, (__in WNDPROC lpPrevWndFunc, __in HWND hWnd, __in UINT Msg, __in WPARAM wParam, __in LPARAM lParam))
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return lpPrevWndFunc(hWnd, Msg, wParam, lParam);

return USER32_ORIGINAL(CallWindowProcW)(lpPrevWndFunc, hWnd, Msg, wParam, lParam);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(LRESULT, DefWindowProcW, (HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam))
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->DefWindowProc(message, wParam, lParam);

return USER32_ORIGINAL(DefWindowProcW)(hWnd, message, wParam, lParam);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(LRESULT, DefDlgProcW, (HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam))
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->DefWindowProc(message, wParam, lParam);

return USER32_ORIGINAL(DefDlgProcW)(hWnd, message, wParam, lParam);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(LRESULT, SendMessageW, (HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam))
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->SendMessage(message, wParam, lParam);

return USER32_ORIGINAL(SendMessageW)(hWnd, message, wParam, lParam);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, SendMessageCallbackW, (__in HWND hWnd, __in UINT Msg, __in WPARAM wParam, __in LPARAM lParam, __in SENDASYNCPROC lpResultCallBack, __in ULONG_PTR dwData))
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->GetMessageQueue()->SendMessageCallback(pWnd, Msg, wParam, lParam, lpResultCallBack, dwData);

return USER32_ORIGINAL(SendMessageCallbackW)(hWnd, Msg, wParam, lParam, lpResultCallBack, dwData);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, PostMessageW, (HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam))
if (hWnd == NULL)
return PostThreadMessage(GetCurrentThreadId(), message, wParam, lParam);
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	MSG* pMsg = CreateMessage(hWnd, message, wParam, lParam);
	return pWnd->GetMessageQueue()->PostMessage(pMsg);
}

return USER32_ORIGINAL(PostMessageW)(hWnd, message, wParam, lParam);
}


//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, IsWindow, (HWND hWnd))
if (!hWnd)
return FALSE;
if (hWnd == HWND_TBMFC_SPECIAL)
return TRUE;
if (GetTBWnd(hWnd))
return TRUE;


return USER32_ORIGINAL(IsWindow)(hWnd);
}



//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(DWORD, GetWindowThreadProcessId, (__in HWND hWnd, __out_opt LPDWORD lpdwProcessId))
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	if (lpdwProcessId)
		*lpdwProcessId = GetCurrentProcessId();
	return pWnd->GetThreadID();
}
return  USER32_ORIGINAL(GetWindowThreadProcessId)(hWnd, lpdwProcessId);
}


//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, IsWindowEnabled, (HWND hWnd))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->IsWindowEnabled();

return USER32_ORIGINAL(IsWindowEnabled)(hWnd);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, EnableWindow, (HWND hWnd, BOOL bEnable))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	BOOL b = pWnd->IsWindowEnabled();
	pWnd->EnableWindow(bEnable);
	return b;
}
return USER32_ORIGINAL(EnableWindow)(hWnd, bEnable);
}


//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, SetParent, (HWND hWnd, HWND hwndParent))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	TBWnd* pOldParent = pWnd->GetParent();
	pWnd->SetParent(GetTBWnd(hwndParent));
	return pOldParent ? pOldParent->GetHWND() : NULL;
}

return USER32_ORIGINAL(SetParent)(hWnd, hwndParent);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, GetParent, (HWND hWnd))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	TBWnd* pParent = pWnd->GetParent();
	return pParent ? pParent->GetHWND() : NULL;
}

return USER32_ORIGINAL(GetParent)(hWnd);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, GetAncestor, (HWND hWnd, UINT gaFlags))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	TBWnd* pParent = pWnd->GetAncestor(gaFlags);
	return pParent ? pParent->GetHWND() : NULL;
}


return USER32_ORIGINAL(GetAncestor)(hWnd, gaFlags);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(LONG, SetWindowLongW, (__in HWND hWnd, __in int nIndex, __in LONG dwNewLong))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->SetWindowLong(nIndex, dwNewLong);

return USER32_ORIGINAL(SetWindowLongW)(hWnd, nIndex, dwNewLong);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(LONG, GetWindowLongW, (__in HWND hWnd, __in int nIndex))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->GetWindowLong(nIndex);

return USER32_ORIGINAL(GetWindowLongW)(hWnd, nIndex);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, GetWindowRgn, (HWND hWnd, HRGN hRgn))
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	return NULLREGION;
}

return USER32_ORIGINAL(GetWindowRgn)(hWnd, hRgn);

}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, SetWindowRgn, (HWND hWnd, HRGN hRgn, BOOL bRedraw))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	//pWnd->SetWindowRgn(hRgn);
	return TRUE;
}

return USER32_ORIGINAL(SetWindowRgn)(hWnd, hRgn, bRedraw);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, GetWindow, (HWND hWnd, UINT uCmd))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	TBWnd* pFound = pWnd->GetWindow(uCmd);
	return pFound ? pFound->GetHWND() : NULL;
}

return USER32_ORIGINAL(GetWindow)(hWnd, uCmd);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, GetTopWindow, (HWND hWnd))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	TBWnd* pFound = pWnd->GetTopWindow();
	return pFound ? pFound->GetHWND() : NULL;
}

return USER32_ORIGINAL(GetTopWindow)(hWnd);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, GetDesktopWindow, (VOID))
if (UseStandardAPIS())
return USER32_ORIGINAL(GetDesktopWindow)();
return HWND_TBMFC_SPECIAL;
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, DestroyWindow, (HWND hWnd))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	pWnd->Destroy();
	return TRUE;
}

return USER32_ORIGINAL(DestroyWindow)(hWnd);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HDC, GetDC, (__in_opt HWND hWnd))
if (!hWnd || UseStandardAPIS())
return USER32_ORIGINAL(GetDC)(hWnd);

return CreateTBDC(hWnd);
}


//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HDC, GetWindowDC, (__in_opt HWND hWnd))
if (!hWnd || UseStandardAPIS())
return USER32_ORIGINAL(GetWindowDC)(hWnd);

return CreateTBDC(hWnd);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, ReleaseDC, (__in_opt HWND hWnd, __in HDC hDC))
if (UseStandardAPIS())
return USER32_ORIGINAL(ReleaseDC)(hWnd, hDC);
if (DestroyTBDC(hDC))
return 1;

return USER32_ORIGINAL(ReleaseDC)(hWnd, hDC);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HMENU, GetMenu, (__in_opt HWND hWnd))
if (hWnd)
{
	TBWnd* pWnd = GetTBWnd(hWnd);
	if (pWnd)
		return pWnd->GetMenu();
}

return USER32_ORIGINAL(GetMenu)(hWnd);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HMENU, GetSystemMenu, (_In_ HWND hWnd, _In_ BOOL bRevert))
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->GetSystemMenu(bRevert);

return USER32_ORIGINAL(GetSystemMenu) (hWnd, bRevert);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, SetMenu, (__in HWND hWnd, __in_opt HMENU hMenu))
if (hWnd)
{
	TBWnd* pWnd = GetTBWnd(hWnd);
	if (pWnd)
	{
		pWnd->SetMenu(hMenu);
		return TRUE;
	}
}


return USER32_ORIGINAL(SetMenu)(hWnd, hMenu);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, ShowWindow, (__in HWND hWnd, __in int nCmdShow))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->ShowWindow(nCmdShow);

return USER32_ORIGINAL(ShowWindow)(hWnd, nCmdShow);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, ShowScrollBar, (__in HWND hWnd, __in int wBar, __in BOOL bShow))
if (UseStandardAPIS())
return USER32_ORIGINAL(ShowScrollBar)(hWnd, wBar, bShow);
return TRUE;
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, IsWindowVisible, (__in HWND hWnd))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
return pWnd->IsWindowVisible();

return USER32_ORIGINAL(IsWindowVisible)(hWnd);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, GetClientRect, (__in HWND hWnd, __out LPRECT lpRect))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	pWnd->GetClientRect(lpRect);
	return TRUE;
}

return USER32_ORIGINAL(GetClientRect)(hWnd, lpRect);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, GetWindowRect, (__in HWND hWnd, __out LPRECT lpRect))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	pWnd->GetWindowRect(lpRect);
	return TRUE;
}
return USER32_ORIGINAL(GetWindowRect)(hWnd, lpRect);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, SetWindowTextW, (__in HWND hWnd, __in_opt LPCTSTR lpString))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	pWnd->SendMessage(WM_SETTEXT, NULL, (LPARAM)lpString);
	return TRUE;
}

return USER32_ORIGINAL(SetWindowTextW)(hWnd, lpString);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, GetWindowTextW, (__in HWND hWnd, __out_ecount(nMaxCount) LPWSTR lpString, __in int nMaxCount))
if (!hWnd)
return 0;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	return pWnd->SendMessage(WM_GETTEXT, nMaxCount, (LPARAM)lpString);
}

return USER32_ORIGINAL(GetWindowTextW)(hWnd, lpString, nMaxCount);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, GetWindowTextLengthW, (__in HWND hWnd))
if (!hWnd)
return 0;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	return pWnd->SendMessage(WM_GETTEXTLENGTH, NULL, NULL);
}

return USER32_ORIGINAL(GetWindowTextLengthW)(hWnd);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, GetClassNameW, (HWND hWnd, LPWSTR lpClassName, int nMaxCount))
if (!hWnd)
return 0;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	_tcscpy_s(lpClassName, nMaxCount, pWnd->GetClass());
	return min(nMaxCount, (int)_tcslen(pWnd->GetClass()));
}

return USER32_ORIGINAL(GetClassNameW)(hWnd, lpClassName, nMaxCount);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, SetFocus, (__in HWND hWnd))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	TBWnd* pOld = pWnd->GetMessageQueue()->SetFocus(pWnd, TRUE);
	return pOld ? pOld->GetHWND() : NULL;
}

return USER32_ORIGINAL(SetFocus)(hWnd);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, GetFocus, ())
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(GetFocus)();
}
CMessageQueue* pQueue = GetMessageQueue(GetCurrentThreadId());
TBWnd* pOld = pQueue->GetFocus();
return pOld ? pOld->GetHWND() : NULL;
}


//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, SetActiveWindow, (__in HWND hWnd))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	TBWnd* pOld = pWnd->GetMessageQueue()->SetActiveWindow(pWnd, TRUE);
	return pOld ? pOld->GetHWND() : NULL;
}

return USER32_ORIGINAL(SetActiveWindow)(hWnd);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, GetActiveWindow, ())
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(GetActiveWindow)();
}
CMessageQueue* pQueue = GetMessageQueue(GetCurrentThreadId());
TBWnd* pOld = pQueue->GetActiveWindow();
return pOld ? pOld->GetHWND() : NULL;
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HWND, GetDlgItem, (__in_opt HWND hDlg, __in int nIDDlgItem))
if (!hDlg)
return NULL;
TBWnd* pWnd = GetTBWnd(hDlg);
if (pWnd)
{
	TBWnd* pItem = pWnd->GetDlgItem(nIDDlgItem);
	return pItem ? pItem->GetHWND() : NULL;
}

return USER32_ORIGINAL(GetDlgItem)(hDlg, nIDDlgItem);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, IsChild, (__in HWND hWndParent, __in HWND hWnd))
if (!hWndParent)
return NULL;
TBWnd* pWnd = GetTBWnd(hWndParent);
if (pWnd)
{
	return pWnd->IsChild(hWnd);
}

return USER32_ORIGINAL(IsChild)(hWndParent, hWnd);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(UINT, GetDlgCtrlID, (HWND hWnd))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	return pWnd->GetWindowLong(GWLP_ID);
}

return USER32_ORIGINAL(GetDlgCtrlID)(hWnd);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, ScreenToClient, (__in HWND hWnd, __inout LPPOINT lpPoint))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	pWnd->ScreenToClient(lpPoint);
	return TRUE;
}

return USER32_ORIGINAL(ScreenToClient)(hWnd, lpPoint);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, ClientToScreen, (__in HWND hWnd, __inout LPPOINT lpPoint))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	pWnd->ClientToScreen(lpPoint);
	return TRUE;
}

return USER32_ORIGINAL(ClientToScreen)(hWnd, lpPoint);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, MoveWindow, (__in HWND hWnd, __in int X, __in int Y, __in int nWidth, __in int nHeight, __in BOOL bRepaint))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	pWnd->MoveWindow(X, Y, nWidth, nHeight, bRepaint);
	return TRUE;
}

return USER32_ORIGINAL(MoveWindow)(hWnd, X, Y, nWidth, nHeight, bRepaint);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, SetWindowPos, (__in HWND hWnd, __in_opt HWND hWndInsertAfter, __in int X, __in int Y, __in int cx, __in int cy, __in UINT uFlags))
if (!hWnd)
return NULL;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	pWnd->SetWindowPos(hWndInsertAfter, X, Y, cx, cy, uFlags);
	return TRUE;
}

return USER32_ORIGINAL(SetWindowPos)(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
}


//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, MapDialogRect, (__in HWND hDlg, __inout LPRECT lpRect))
if (!hDlg)
return NULL;
TBWnd* pWnd = GetTBWnd(hDlg);
if (pWnd)
{
	return pWnd->MapDialogRect(lpRect);
}

return USER32_ORIGINAL(MapDialogRect)(hDlg, lpRect);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, GetComboBoxInfo, (__in HWND hwndCombo, __inout PCOMBOBOXINFO pcbi))
if (!hwndCombo)
return NULL;
TBComboBox* pWnd = (TBComboBox*)GetTBWnd(hwndCombo);
if (pWnd)
{
	ASSERT(_tcsicmp(pWnd->GetClass(), _T("COMBOBOX")) == 0);
	pWnd->GetComboBoxInfo(pcbi);
	return TRUE;
}

return USER32_ORIGINAL(GetComboBoxInfo)(hwndCombo, pcbi);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HDWP, BeginDeferWindowPos, (__in int nNumWindows))
if (UseStandardAPIS())
return USER32_ORIGINAL(BeginDeferWindowPos)(nNumWindows);

CDeferItemList* pList = new CDeferItemList();
HDWP hdwp = (HDWP)Get_New_CDeferMap();
CSingleLock l(Get_CDeferMapSection(), TRUE);
Get_CDeferMap().SetAt(hdwp, pList);
return hdwp;
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HDWP, DeferWindowPos, (__in HDWP hWinPosInfo, __in HWND hWnd, __in_opt HWND hWndInsertAfter, __in int x, __in int y, __in int cx, __in int cy, __in UINT uFlags))
CDeferItemList* pList = GetDeferItemList(hWinPosInfo);
if (pList)
{
	pList->Add(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);
	return hWinPosInfo;
}

return USER32_ORIGINAL(DeferWindowPos)(hWinPosInfo, hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, EndDeferWindowPos, (__in HDWP hWinPosInfo))
CDeferItemList* pList = GetDeferItemList(hWinPosInfo);
if (pList)
{
	for (int i = 0; i < pList->GetCount(); i++)
	{
		CDeferItem* item = pList->GetAt(i);
		_TBNAME(SetWindowPos)(item->m_hWnd, item->m_hWndInsertAfter, item->m_x, item->m_y, item->m_cx, item->m_cy, item->m_uFlags);
	}
	delete pList;
	CSingleLock l(Get_CDeferMapSection(), TRUE);
	Get_CDeferMap().RemoveKey(hWinPosInfo);
	return TRUE;
}

return USER32_ORIGINAL(EndDeferWindowPos)(hWinPosInfo);
}


//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(UINT_PTR, SetTimer, (_In_opt_ HWND hWnd, _In_ UINT_PTR nIDEvent, _In_ UINT uElapse, _In_opt_ TIMERPROC lpTimerFunc))

if (UseStandardAPIS())
{
	return USER32_ORIGINAL(SetTimer)(hWnd, nIDEvent, uElapse, lpTimerFunc);
}
CMessageQueue* pQueue = GetMessageQueue(GetCurrentThreadId());
return pQueue->CreateTimer(hWnd, nIDEvent, uElapse, lpTimerFunc);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, KillTimer, (_In_opt_ HWND hWnd, _In_ UINT_PTR uIDEvent))
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(KillTimer)(hWnd, uIDEvent);
}
CMessageQueue* pQueue = GetMessageQueue(GetCurrentThreadId());
return pQueue->DestroyTimer(hWnd, uIDEvent);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, SetScrollRange, (_In_ HWND hWnd, _In_ int nBar, _In_ int nMinPos, _In_ int nMaxPos, _In_ BOOL bRedraw))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	return pWnd->SetScrollRange(nBar, nMinPos, nMaxPos);
}

return USER32_ORIGINAL(SetScrollRange)(hWnd, nBar, nMinPos, nMaxPos, bRedraw);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, GetScrollRange, (_In_ HWND hWnd, _In_ int nBar, _Out_ LPINT lpMinPos, _Out_ LPINT lpMaxPos))
if (!hWnd)
return FALSE;
TBWnd* pWnd = GetTBWnd(hWnd);
if (pWnd)
{
	return pWnd->GetScrollRange(nBar, lpMinPos, lpMaxPos);
}

return USER32_ORIGINAL(GetScrollRange)(hWnd, nBar, lpMinPos, lpMaxPos);
}

#ifdef HOOK_IMAGES
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HBITMAP, LoadBitmapW, (__in_opt HINSTANCE hInstance,__in LPCWSTR lpBitmapName))
if (UseStandardAPIS())
return USER32_ORIGINAL(LoadBitmapW)(hInstance, lpBitmapName);
CString sKey;
if (HIWORD(lpBitmapName) == 0)
sKey.Format(_T("%d_%d"), hInstance, lpBitmapName);
else
sKey.Format(_T("%d_%s"), hInstance, lpBitmapName);

CSingleLock l(Get_CImageMapSection(), TRUE);
CTBImage* pImage = NULL;
if (!Get_CImageMap().Lookup(sKey, pImage))
{
	pImage = new CTBImage(USER32_ORIGINAL(LoadBitmapW)(hInstance, lpBitmapName), sKey);
	Get_CImageMap().SetAt(sKey, pImage);
	GetGDIMap().SetAt(pImage->GetHandle(), pImage);
}
pImage->AddRef();
return (HBITMAP)pImage->GetHandle();
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HANDLE, LoadImageW,(__in_opt HINSTANCE hInst,__in LPCWSTR name,__in UINT type,__in int cx,__in int cy,__in UINT fuLoad))
if (UseStandardAPIS())
return USER32_ORIGINAL(LoadImageW)(hInst,name,type,cx,cy,fuLoad);
CString sKey;
if (HIWORD(name) == 0)
sKey.Format(_T("%d_%d_%d"), hInst, name, type);
else
sKey.Format(_T("%d_%s_%d"), hInst, name, type);

CSingleLock l(Get_CImageMapSection(), TRUE);
CTBImage* pImage = NULL;
if (!Get_CImageMap().Lookup(sKey, pImage))
{
	pImage = new CTBImage(USER32_ORIGINAL(LoadImageW)(hInst,name,type,cx,cy,fuLoad), sKey);
	Get_CImageMap().SetAt(sKey, pImage);
	GetGDIMap().SetAt(pImage->GetHandle(), pImage);
}
pImage->AddRef();
return (HBITMAP)pImage->GetHandle();
}


#endif //HOOK_IMAGES


//MENU

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, IsMenu, (HMENU hmenu))
if (!hmenu)
return FALSE;
if (GetTBMenu(hmenu))
return TRUE;

return USER32_ORIGINAL(IsMenu)(hmenu);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HMENU, CreateMenu, ())
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(CreateMenu)();
}

return (new TBMenu)->GetHMENU();
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HMENU, CreatePopupMenu, ())
if (UseStandardAPIS())
{
	return USER32_ORIGINAL(CreatePopupMenu)();
}

return (new TBMenu)->GetHMENU();
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, DestroyMenu, (_In_ HMENU hMenu))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
{
	delete pMenu;
	return TRUE;
}

return USER32_ORIGINAL(DestroyMenu) (hMenu);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, AppendMenu, (_In_ HMENU hMenu, _In_ UINT uFlags, _In_ UINT_PTR uIDNewItem, _In_opt_ LPCWSTR lpNewItem))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->AppendMenu(uFlags, uIDNewItem, lpNewItem);

return USER32_ORIGINAL(AppendMenu) (hMenu, uFlags, uIDNewItem, lpNewItem);

}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, InsertMenu, (_In_ HMENU hMenu, _In_ UINT uPosition, _In_ UINT uFlags, _In_ UINT_PTR uIDNewItem, _In_opt_ LPCTSTR  lpNewItem))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->InsertMenu(uPosition, uFlags, uIDNewItem, lpNewItem);

return USER32_ORIGINAL(InsertMenu) (hMenu, uPosition, uFlags, uIDNewItem, lpNewItem);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, ChangeMenuW, (_In_ HMENU hMenu, _In_ UINT cmd, _In_opt_ LPCWSTR lpszNewItem, _In_ UINT cmdInsert, _In_ UINT flags))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->ChangeMenu(cmd, lpszNewItem, cmdInsert, flags);

return USER32_ORIGINAL(ChangeMenuW) (hMenu, cmd, lpszNewItem, cmdInsert, flags);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, GetMenuStringW, (_In_ HMENU hMenu, _In_ UINT uIDItem, _Out_writes_opt_(cchMax) LPWSTR lpString, _In_ int cchMax, _In_ UINT flags))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->GetMenuString(uIDItem, lpString, cchMax, flags);

return USER32_ORIGINAL(GetMenuStringW) (hMenu, uIDItem, lpString, cchMax, flags);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(UINT, GetMenuState, (_In_ HMENU hMenu, _In_ UINT uId, _In_ UINT uFlags))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->GetMenuState(uId, uFlags);

return USER32_ORIGINAL(GetMenuState) (hMenu, uId, uFlags);
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(DWORD, CheckMenuItem, (_In_ HMENU hMenu, _In_ UINT uIDCheckItem, _In_ UINT uCheck))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->CheckMenuItem(uIDCheckItem, uCheck);

return USER32_ORIGINAL(CheckMenuItem) (hMenu, uIDCheckItem, uCheck);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, EnableMenuItem, (_In_ HMENU hMenu, _In_ UINT uIDEnableItem, _In_ UINT uEnable))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->EnableMenuItem(uIDEnableItem, uEnable);

return USER32_ORIGINAL(EnableMenuItem) (hMenu, uIDEnableItem, uEnable);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(HMENU, GetSubMenu, (_In_ HMENU hMenu, _In_ int nPos))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->GetSubMenu(nPos);

return USER32_ORIGINAL(GetSubMenu) (hMenu, nPos);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(UINT, GetMenuItemID, (_In_ HMENU hMenu, _In_ int nPos))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->GetMenuItemID(nPos);

return USER32_ORIGINAL(GetMenuItemID) (hMenu, nPos);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(int, GetMenuItemCount, (_In_opt_ HMENU hMenu))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->GetMenuItemCount();

return USER32_ORIGINAL(GetMenuItemCount) (hMenu);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, ModifyMenuW, (_In_ HMENU hMenu, _In_ UINT uPosition, _In_ UINT uFlags, _In_ UINT_PTR uIDNewItem, _In_opt_ LPCWSTR lpNewItem))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->ModifyMenuW(uPosition, uFlags, uIDNewItem, lpNewItem);

return USER32_ORIGINAL(ModifyMenuW) (hMenu, uPosition, uFlags, uIDNewItem, lpNewItem);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL,RemoveMenu,(_In_ HMENU hMenu, _In_ UINT uPosition, _In_ UINT uFlags))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->RemoveMenu(uPosition,  uFlags);

return USER32_ORIGINAL(RemoveMenu) (hMenu, uPosition, uFlags);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, DeleteMenu, (_In_ HMENU hMenu, _In_ UINT uPosition, _In_ UINT uFlags))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->DeleteMenu(uPosition,  uFlags);

return USER32_ORIGINAL(DeleteMenu) (hMenu, uPosition, uFlags);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, SetMenuItemBitmaps,(_In_ HMENU hMenu, _In_ UINT uPosition, _In_ UINT uFlags, _In_opt_ HBITMAP hBitmapUnchecked, _In_opt_ HBITMAP hBitmapChecked))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->SetMenuItemBitmaps(uPosition, uFlags, hBitmapUnchecked, hBitmapChecked);

return USER32_ORIGINAL(SetMenuItemBitmaps) (hMenu, uPosition, uFlags, hBitmapUnchecked, hBitmapChecked);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, TrackPopupMenu, (_In_ HMENU hMenu, _In_ UINT uFlags, _In_ int x, _In_ int y, _Reserved_ int nReserved, _In_ HWND hWnd, _Reserved_ CONST RECT *prcRect))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->TrackPopupMenu(uFlags, x, y, nReserved, hWnd, prcRect);

return USER32_ORIGINAL(TrackPopupMenu) (hMenu, uFlags, x, y, nReserved, hWnd, prcRect);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, TrackPopupMenuEx, (_In_ HMENU hMenu, _In_ UINT uFlags, _In_ int x, _In_ int y, _In_ HWND hwnd, _In_opt_ LPTPMPARAMS lptpm))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->TrackPopupMenuEx(uFlags, x, y, hwnd, lptpm);

return USER32_ORIGINAL(TrackPopupMenuEx) (hMenu, uFlags, x, y, hwnd, lptpm);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, GetMenuInfo, (_In_ HMENU hMenu, _Inout_ LPMENUINFO lpMenuInfo))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->GetMenuInfo(lpMenuInfo);

return USER32_ORIGINAL(GetMenuInfo) (hMenu, lpMenuInfo);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, SetMenuInfo, (_In_ HMENU hMenu, _In_ LPCMENUINFO lpMenuInfo))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->SetMenuInfo(lpMenuInfo);

return USER32_ORIGINAL(SetMenuInfo) (hMenu, lpMenuInfo);
}
//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, EndMenu, (VOID))
return TRUE;
}

//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, GetMenuItemRect,(_In_opt_ HWND hWnd, _In_ HMENU hMenu, _In_ UINT uItem, _Out_ LPRECT lprcItem))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->GetMenuItemRect(hWnd, uItem,  lprcItem);

return USER32_ORIGINAL(GetMenuItemRect) (hWnd, hMenu, uItem, lprcItem);
}


//---------------------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, GetMenuItemInfo, (_In_ HMENU hMenu, _In_ UINT item, _In_ BOOL fByPosition, _Inout_ LPMENUITEMINFOW lpmii))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->GetMenuItemInfo(item, fByPosition, lpmii);

return USER32_ORIGINAL(GetMenuItemInfo) (hMenu, item, fByPosition, lpmii);
}
//------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, SetMenuItemInfoW, (_In_ HMENU hMenu, _In_ UINT item, _In_ BOOL fByPositon, _In_ LPCMENUITEMINFOW lpmii))
TBMenu* pMenu = GetTBMenu(hMenu);
if (pMenu)
return pMenu->SetMenuItemInfoW(item, fByPositon, lpmii);

return USER32_ORIGINAL(SetMenuItemInfoW) (hMenu, item, fByPositon, lpmii);
}


//------------------------------------------------------------------------------------------------
HOOK_USER32(HMONITOR, MonitorFromPoint, (_In_ POINT pt, _In_ DWORD dwFlags))
if (UseStandardAPIS())
	return USER32_ORIGINAL(MonitorFromPoint) (pt, dwFlags);
return THE_MONITOR;
}
//------------------------------------------------------------------------------------------------
HOOK_USER32(HMONITOR, MonitorFromRect, (_In_ LPCRECT lprc, _In_ DWORD dwFlags))
if (UseStandardAPIS())
return USER32_ORIGINAL(MonitorFromRect) (lprc, dwFlags);
return THE_MONITOR;
}
//------------------------------------------------------------------------------------------------
HOOK_USER32(HMONITOR, MonitorFromWindow, (_In_ HWND hwnd, _In_ DWORD dwFlags))
if (UseStandardAPIS())
return USER32_ORIGINAL(MonitorFromWindow) (hwnd, dwFlags);
return THE_MONITOR;
}
//------------------------------------------------------------------------------------------------
HOOK_USER32(BOOL, GetMonitorInfoW, (_In_ HMONITOR hMonitor, _Inout_ LPMONITORINFO lpmi))
if (hMonitor != THE_MONITOR)
return USER32_ORIGINAL(GetMonitorInfoW) (hMonitor, lpmi);
lpmi->rcMonitor = CRect(CPoint(0, 0), CSize(MONITOR_WIDTH, MONITOR_HEIGHT));
lpmi->rcWork = CRect(CPoint(0, 0), CSize(MONITOR_WIDTH, MONITOR_HEIGHT));
lpmi->dwFlags = MONITORINFOF_PRIMARY;

return TRUE;
}