#pragma once
#include "afxwin.h"

//includere come ultimo include all'inizio del .h
#include "beginh.dex"

/// <summary>
/// Menu class directly derived from CMenu wich fires an OnBeforeMenuOpen
/// event just before the context menu is tracked by calling methods TBTrackPopupMenu
/// or TBTrackPopupMenuEx. In this way WebLook is notified about menu opening and can 
/// properly render such a menu.
/// </summary>
/// <remarks>
/// Standard menu is not able to notify the context menu is going to be opened and tracked,
/// so WebLook will know a menu has to be rendered.
/// </remarks>
class TB_EXPORT CTBWebFriendlyMenu :
	public CMenu
{
public:
	CTBWebFriendlyMenu(void);
	virtual ~CTBWebFriendlyMenu(void);

	BOOL TBTrackPopupMenu(UINT nFlags, int x, int y,
		CWnd* pWnd, LPCRECT lpRect = 0);
	BOOL TBTrackPopupMenuEx(UINT fuFlags, int x, int y, CWnd* pWnd, LPTPMPARAMS lptpm);
	static void GetTrackingMenuInfo(HMENU& hTrackingMenu, CString& sTrackingWindowID);
	static void SetTrackingMenuInfo(HMENU phTrackingMenu, CString sTrackingWindowID);
};

#include "endh.dex"