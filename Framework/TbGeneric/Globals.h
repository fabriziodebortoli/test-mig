#pragma once

#include <TBNameSolver\UserMessages.h>
#include <TBNameSolver\MacroToRedifine.h>
#include "TBScrollBar.h"

// utile per deletare i puntatori a vari table reader ecc.
#define SAFE_DELETE(p) \
			if (p) \
			{ \
				delete p; \
				p = NULL; \
			}

// da usare dove non si sa dare un ID specifico
#define DUMMY_ID	0xFFFFFFFF

// utile per espandere in modo corretto i paramentri di WM_COMMAND
#define DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWnd) \
	UINT nID	= LOWORD(wParam);\
	UINT nCode	= HIWORD(wParam);\
	HWND hWnd	= (HWND)lParam;

#define SEND_WM_COMMAND(nID, nCode, hWnd) \
	SendMessage(WM_COMMAND, (WPARAM)MAKELONG(nID, nCode), (LPARAM)(hWnd));

#define POST_WM_COMMAND(nID, nCode, hWnd) \
	PostMessage(WM_COMMAND, (WPARAM)MAKELONG(nID, nCode), (LPARAM)(hWnd));
	
#define GET_FIRST_DOC_TEMPLATE_POSITION()	GetFirstDocTemplatePosition()
#define GET_NEXT_DOC_TEMPLATE(pos)			(CSingleExtDocTemplate*) GetNextDocTemplate(pos)

#define MAKE_COMPATIBLE_HANDLE(lparam)	((LPARAM)lparam)
#define SET_ICON(hWnd,hIcon)	SetClassLong(hWnd, GCL_HICON, (LONG)hIcon)

// useful to avoid the annoying extra stuff shown by the VS 2013 TRACE
#define TBTRACE(sz)							OutputDebugString(_T(sz))	
#define TBTRACE0(sz)						TBTRACE(sz)
#define TBTRACE1(sz, p1)					OutputDebugString(cwsprintf(_T(sz), p1))	
#define TBTRACE2(sz, p1, p2)				OutputDebugString(cwsprintf(_T(sz), p1, p2))	
#define TBTRACE3(sz, p1, p2, p3)			OutputDebugString(cwsprintf(_T(sz), p1, p2, p3))	
#define TBTRACE4(sz, p1, p2, p3, p4)		OutputDebugString(cwsprintf(_T(sz), p1, p2, p3, p4))	
#define TBTRACE5(sz, p1, p2, p3, p4, p5)	OutputDebugString(cwsprintf(_T(sz), p1, p2, p3, p4, p5))	

#if WINVER < 0x0501
#error "The WINVER macro usually defined in stdafx.h has to be equal to 0x0501 (al least WindowsXp)"
#endif
#if _WIN32_WINNT < 0x0501
#error "The _WIN32_WINNT macro usually defined in stdafx.h has to be equal to 0x0501 (al least WindowsXp)"
#endif
#if _WIN32_WINDOWS < 0x0501
#error "The _WIN32_WINDOWS macro usually defined in stdafx.h has to be equal to 0x0501 (al least WindowsXp)"
#endif
#if _WIN32_IE < 0x0600
#error "The _WIN32_IE macro usually defined in stdafx.h has to be equal to 0x0600 (al least Internet Explorer 6)"
#endif