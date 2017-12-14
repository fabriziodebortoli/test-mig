#include "StdAfx.h"
#include "WndObjDescription.h"
#include "TBWebFriendlyMenu.h"


#include <TBNameSolver\ThreadContext.h>


//-----------------------------------------------------------------------------
CTBWebFriendlyMenu::CTBWebFriendlyMenu(): CMenu()
{
}

//-----------------------------------------------------------------------------
CTBWebFriendlyMenu::~CTBWebFriendlyMenu()
{
}

DECLARE_AND_INIT_THREAD_VARIABLE(HMENU, t_hTrackingMenu, NULL)
DECLARE_AND_INIT_THREAD_VARIABLE(CString, t_sTrackingWindowID, NULL)

//-----------------------------------------------------------------------------
void CTBWebFriendlyMenu::GetTrackingMenuInfo(HMENU& hTrackingMenu, CString& sTrackingWindowID)
{
	GET_THREAD_VARIABLE(HMENU, t_hTrackingMenu)
		hTrackingMenu = t_hTrackingMenu;
	GET_THREAD_VARIABLE(CString, t_sTrackingWindowID)
		sTrackingWindowID = t_sTrackingWindowID;
}

//-----------------------------------------------------------------------------
void CTBWebFriendlyMenu::SetTrackingMenuInfo(HMENU phTrackingMenu, CString sTrackingWindowID)
{
	GET_THREAD_VARIABLE(HMENU, t_hTrackingMenu)
	GET_THREAD_VARIABLE(CString, t_sTrackingWindowID)

	t_hTrackingMenu = phTrackingMenu;
	t_sTrackingWindowID = sTrackingWindowID;
}

//-----------------------------------------------------------------------------
BOOL CTBWebFriendlyMenu::TBTrackPopupMenu(UINT nFlags, int x, int y,
	CWnd* pWnd, LPCRECT lpRect)
{
	if (AfxIsRemoteInterface())
	{
		CString sOwnerId = CWndObjDescriptionContainer::GetCtrlID(pWnd->m_hWnd);
		SetTrackingMenuInfo(this->m_hMenu, sOwnerId);
	}

	BOOL b = __super::TrackPopupMenu(nFlags, x, y, pWnd, lpRect);

	if (AfxIsRemoteInterface())
	{
		SetTrackingMenuInfo(NULL, NULL);
	}

	return b;
}

//-----------------------------------------------------------------------------
BOOL CTBWebFriendlyMenu::TBTrackPopupMenuEx(UINT fuFlags, int x, int y, CWnd* pWnd, LPTPMPARAMS lptpm)
{
	return CMenu::TrackPopupMenuEx(fuFlags, x, y, pWnd, lptpm);
}
