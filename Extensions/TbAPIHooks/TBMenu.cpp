#include "stdafx.h"
#include "TBMenu.h"
#include "TBWnd.h"

//-----------------------------------------------------------------------------
TBMenu::TBMenu(UINT_PTR nId)
{
	if (nId == 0)
	{
		m_Id = (INT_PTR)Get_New_CMenuMap();
	}
	else
	{
		m_Id = nId;
	}
	CSingleLock l(Get_CMenuMapSection(), TRUE);
	Get_CMenuMap().SetAt(GetHMENU(), this);
}

//-----------------------------------------------------------------------------
TBMenu::~TBMenu()
{
	for (int i = 0; i < m_arItems.GetCount(); i++)
	{
		MENUITEMINFO* info = m_arItems[i];
		if ((info->fState | MF_STRING) == MF_STRING)
			delete info->dwTypeData;
		delete info;
	}
	Get_CMenuMap().RemoveKey(GetHMENU());
}

//-----------------------------------------------------------------------------
void TBMenu::SetMenuData(MENUITEMINFO* menuItemInfo, UINT uFlags, LPCTSTR lpData)
{
	if ((uFlags | MF_STRING) == MF_STRING)
	{
		if ((menuItemInfo->fState | MF_STRING) == MF_STRING)
			delete menuItemInfo->dwTypeData;
		int sz = _tcslen(lpData) + 1;
		menuItemInfo->dwTypeData = new TCHAR[sz];
		_tcscpy_s(menuItemInfo->dwTypeData, sz, lpData);
	}
	else
	{
		menuItemInfo->dwTypeData = (LPTSTR)lpData;
	}
	menuItemInfo->fState = uFlags;

}
//-----------------------------------------------------------------------------
BOOL TBMenu::AppendMenu(UINT uFlags, UINT_PTR uIDNewItem, LPCTSTR lpNewItem)
{
	MENUITEMINFO* menuItemInfo = new MENUITEMINFO;
	ZeroMemory(menuItemInfo, sizeof(MENUITEMINFO));
	menuItemInfo->wID = uIDNewItem;
	menuItemInfo->fState = uFlags;
	if (lpNewItem && lpNewItem[0])
	{
		SetMenuData(menuItemInfo, uFlags, lpNewItem);
	}
	m_arItems.Add(menuItemInfo);
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL TBMenu::InsertMenu(UINT uPosition, UINT uFlags, UINT_PTR uIDNewItem, LPCTSTR lpNewItem)
{
	MENUITEMINFO* menuItemInfo = new MENUITEMINFO;
	ZeroMemory(menuItemInfo, sizeof(MENUITEMINFO));
	menuItemInfo->fState = uFlags;
	if (lpNewItem && lpNewItem[0])
	{
		SetMenuData(menuItemInfo, uFlags, lpNewItem);
	}
	m_arItems.InsertAt(uPosition, menuItemInfo);
	return TRUE;
}
//-----------------------------------------------------------------------------
CMenuMap::~CMenuMap()
{
	POSITION pos = GetStartPosition();
	HMENU key;
	TBMenu* pVal;
	while (pos)
	{
		GetNextAssoc(pos, key, pVal);
		delete pVal;
	}
}

//-----------------------------------------------------------------------------
MENUITEMINFO* TBMenu::GetChild(UINT uIDItem, UINT flags)
{
	int i;
	return GetChild(uIDItem, flags, i);
}
//-----------------------------------------------------------------------------
MENUITEMINFO* TBMenu::GetChild(UINT uIDItem, UINT flags, int& index)
{
	index = -1;
	if ((flags | MF_BYPOSITION) == MF_BYPOSITION)
	{
		if (uIDItem < 0 || uIDItem >= (UINT)m_arItems.GetCount())
			return NULL;
		index = uIDItem;
		return m_arItems.ElementAt(uIDItem);
	}
	for (int i = 0; i < m_arItems.GetCount(); i++)
	{
		MENUITEMINFO* pItem = m_arItems.GetAt(i);
		if (pItem->wID == uIDItem)
		{
			index = i;
			return pItem;
		}
	}
	return NULL;

}
//-----------------------------------------------------------------------------
BOOL TBMenu::ChangeMenuW(_In_ UINT cmd, _In_opt_ LPCWSTR lpszNewItem, _In_ UINT cmdInsert, _In_ UINT flags)
{
	return FALSE;//TODOPERASSO
}
//-----------------------------------------------------------------------------
int TBMenu::GetMenuStringW(_In_ UINT uIDItem, _Out_writes_opt_(cchMax) LPWSTR lpString, _In_ int cchMax, _In_ UINT flags)
{
	MENUITEMINFO* pMenu = GetChild(uIDItem, flags);

	if (!pMenu)
		return 0;
	if ((pMenu->fState | MF_STRING) != MF_STRING)
		return 0;

	if (!lpString)
		return CString((LPCTSTR)pMenu->dwTypeData).GetLength();
	return _tcscpy_s(lpString, cchMax, pMenu->dwTypeData);
}

//-----------------------------------------------------------------------------
UINT TBMenu::GetMenuState(_In_ UINT uId, _In_ UINT uFlags)
{
	return -1;

	MENUITEMINFO* pMenu = GetChild(uId, uFlags);

	if (!pMenu)
		return -1;
	return pMenu->fState;
}
//-----------------------------------------------------------------------------
DWORD TBMenu::CheckMenuItem(_In_ UINT uIDCheckItem, _In_ UINT uCheck)
{
	MENUITEMINFO* pMenu = GetChild(uIDCheckItem, uCheck);

	if (!pMenu)
		return -1;
	BOOL bChecked = (pMenu->fState | MF_CHECKED) == MF_CHECKED;
	if ((uCheck | MF_CHECKED) == MF_CHECKED)
		pMenu->fState |= MF_CHECKED;
	else
		pMenu->fState &= ~MF_CHECKED;

	return bChecked ? MF_CHECKED : MF_UNCHECKED;

}

//-----------------------------------------------------------------------------
BOOL TBMenu::EnableMenuItem(_In_ UINT uIDEnableItem, _In_ UINT uEnable)
{
	return TRUE;
	MENUITEMINFO* pMenu = GetChild(uIDEnableItem, uEnable);

	if (!pMenu)
		return -1;
	BOOL bDisabled = (pMenu->fState | MF_DISABLED) == MF_DISABLED;
	BOOL bGrayed = (pMenu->fState | MF_GRAYED) == MF_GRAYED;

	if ((uEnable | MF_DISABLED) == MF_DISABLED)
	{
		pMenu->fState |= MF_DISABLED;
		pMenu->fState &= ~MF_ENABLED;
		pMenu->fState &= ~MF_GRAYED;
	}
	else if ((uEnable | MF_GRAYED) == MF_GRAYED)
	{
		pMenu->fState |= MF_GRAYED;
		pMenu->fState &= ~MF_ENABLED;
		pMenu->fState &= ~MF_DISABLED;
	}
	else if ((uEnable | MF_ENABLED) == MF_ENABLED)
	{
		pMenu->fState |= MF_ENABLED;
		pMenu->fState &= ~MF_GRAYED;
		pMenu->fState &= ~MF_DISABLED;
	}

	return bDisabled ? MF_DISABLED : (bGrayed ? MF_GRAYED : MF_ENABLED);
}
//-----------------------------------------------------------------------------
HMENU TBMenu::GetSubMenu(_In_ int nPos)
{
	MENUITEMINFO* pMenu = GetChild(nPos, MF_BYPOSITION);

	return pMenu ? (HMENU)pMenu->hSubMenu : NULL;
}
//-----------------------------------------------------------------------------
UINT TBMenu::GetMenuItemID(_In_ int nPos)
{
	MENUITEMINFO* pMenu = GetChild(nPos, MF_BYPOSITION);

	return pMenu ? pMenu->wID : -1;
}
//-----------------------------------------------------------------------------
int TBMenu::GetMenuItemCount()
{
	return m_arItems.GetCount();
}
//-----------------------------------------------------------------------------
BOOL TBMenu::ModifyMenuW(_In_ UINT uPosition, _In_ UINT uFlags, _In_ UINT_PTR uIDNewItem, _In_opt_ LPCWSTR lpNewItem)
{
	int index;
	MENUITEMINFO* pMenu = GetChild(uPosition, uFlags, index);
	if (!pMenu)
		return FALSE;

	SetMenuData(pMenu, uFlags, lpNewItem);
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL TBMenu::RemoveMenu(_In_ UINT uPosition, _In_ UINT uFlags)
{
	int index;
	MENUITEMINFO* pMenu = GetChild(uPosition, uFlags, index);
	if (!pMenu)
		return FALSE;
	m_arItems.RemoveAt(index);
	if (!pMenu->hSubMenu)//da documentazione: se contiene dei sottoelementi, può essere riusato
		delete pMenu;
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL TBMenu::DeleteMenu(_In_ UINT uPosition, _In_ UINT uFlags)
{
	int index;
	MENUITEMINFO* pMenu = GetChild(uPosition, uFlags, index);
	if (!pMenu)
		return FALSE;
	m_arItems.RemoveAt(index);
	delete pMenu;
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL TBMenu::SetMenuItemBitmaps(_In_ UINT uPosition, _In_ UINT uFlags, _In_opt_ HBITMAP hBitmapUnchecked, _In_opt_ HBITMAP hBitmapChecked)
{
	MENUITEMINFO* pMenu = GetChild(uPosition, uFlags);

	if (!pMenu)
		return FALSE;
	pMenu->hbmpChecked = hBitmapChecked;
	pMenu->hbmpUnchecked = hBitmapUnchecked;
	return TRUE;
}

#define ITEM_H 20
//-----------------------------------------------------------------------------
BOOL TBMenu::GetMenuItemRect(_In_opt_ HWND hWnd, _In_ UINT uItem, _Out_ LPRECT lprcItem)
{
	MENUITEMINFO* pMenu = GetChild(uItem, MF_BYPOSITION);

	if (!pMenu)
		return FALSE;
	lprcItem->left = m_x;
	lprcItem->top = m_y + ITEM_H * uItem;
	lprcItem->right = m_x + 100;
	lprcItem->bottom = m_y + ITEM_H;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL TBMenu::TrackPopupMenu(_In_ UINT uFlags, _In_ int x, _In_ int y, _Reserved_ int nReserved, _In_ HWND hWnd, _Reserved_ CONST RECT *prcRect)
{
	m_x = x;
	m_y = y;
	m_hwndOwner = hWnd;
	TBWnd* pWnd = GetTBWnd(hWnd);
	if (!pWnd)
		pWnd = GetTBWnd(HWND_TBMFC_SPECIAL);
	pWnd->m_pContextMenu = this;
	CMessageQueue* pQueue = pWnd->GetMessageQueue();
	while (pWnd->m_pContextMenu)
		pQueue->RunMessageLoop();

	return FALSE;
} 
//-----------------------------------------------------------------------------
BOOL TBMenu::TrackPopupMenuEx(_In_ UINT uFlags, _In_ int x, _In_ int y, _In_ HWND hwnd, _In_opt_ LPTPMPARAMS lptpm)
{
	m_x = x;
	m_y = y;
	m_hwndOwner = hwnd;
	TBWnd* pWnd = GetTBWnd(hwnd);
	if (!pWnd)
		pWnd = GetTBWnd(HWND_TBMFC_SPECIAL);
	pWnd->m_pContextMenu = this;

	CMessageQueue* pQueue = pWnd->GetMessageQueue();
	while (pWnd->m_pContextMenu)
		pQueue->RunMessageLoop();
		
	return FALSE;
}
//-----------------------------------------------------------------------------
BOOL TBMenu::GetMenuInfo(_Inout_ LPMENUINFO lpMenuInfo)
{
	lpMenuInfo->cbSize = menuinfo.cbSize;
	lpMenuInfo->hbrBack = menuinfo.hbrBack;
	lpMenuInfo->dwContextHelpID = menuinfo.dwContextHelpID;
	lpMenuInfo->cyMax = menuinfo.cyMax;
	lpMenuInfo->dwMenuData = menuinfo.dwMenuData;
	lpMenuInfo->dwStyle = menuinfo.dwStyle;
	lpMenuInfo->fMask = menuinfo.fMask;

	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL TBMenu::SetMenuInfo(_In_ LPCMENUINFO lpMenuInfo)
{
	menuinfo.cbSize = lpMenuInfo->cbSize;
	menuinfo.hbrBack = lpMenuInfo->hbrBack;
	menuinfo.dwContextHelpID = lpMenuInfo->dwContextHelpID;
	menuinfo.cyMax = lpMenuInfo->cyMax;
	menuinfo.dwMenuData = lpMenuInfo->dwMenuData;
	menuinfo.dwStyle = lpMenuInfo->dwStyle;
	menuinfo.fMask = lpMenuInfo->fMask;

	return TRUE;
}
//-----------------------------------------------------------------------------
void TBMenu::Assign(const MENUITEMINFO* pFrom, MENUITEMINFO* pTo)
{
	if ((pFrom->fMask | MIIM_BITMAP) == MIIM_BITMAP)
		pTo->hbmpItem = pFrom->hbmpItem;
	if ((pFrom->fMask | MIIM_CHECKMARKS) == MIIM_CHECKMARKS)
	{
		pTo->hbmpChecked = pFrom->hbmpChecked;
		pTo->hbmpUnchecked = pFrom->hbmpUnchecked;
	}
	if ((pFrom->fMask | MIIM_DATA) == MIIM_DATA)
		pTo->dwItemData = pFrom->dwItemData;
	if ((pFrom->fMask | MIIM_FTYPE) == MIIM_FTYPE || (pFrom->fMask | MIIM_TYPE) == MIIM_TYPE)
		pTo->fType = pFrom->fType;
	if ((pFrom->fMask | MIIM_ID) == MIIM_ID)
		pTo->wID = pFrom->wID;
	if ((pFrom->fMask | MIIM_STATE) == MIIM_STATE)
		pTo->fState = pFrom->fState;
	if ((pFrom->fMask | MIIM_STRING) == MIIM_STRING || (pFrom->fMask | MIIM_TYPE) == MIIM_TYPE)
		pTo->dwTypeData = pFrom->dwTypeData;
	if ((pFrom->fMask | MIIM_SUBMENU) == MIIM_SUBMENU)
		pTo->hSubMenu = pFrom->hSubMenu;
}
//-----------------------------------------------------------------------------
BOOL TBMenu::GetMenuItemInfo(_In_ UINT item, _In_ BOOL fByPosition, _Inout_ LPMENUITEMINFOW lpmii)
{
	MENUITEMINFO* pMenu = GetChild(item, fByPosition ? MF_BYPOSITION : MF_BYCOMMAND);

	if (!pMenu)
		return FALSE;

	Assign(pMenu, lpmii);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL TBMenu::SetMenuItemInfo(_In_ UINT item, _In_ BOOL fByPosition, _In_ LPCMENUITEMINFOW lpmii)
{
	MENUITEMINFO* pMenu = GetChild(item, fByPosition ? MF_BYPOSITION : MF_BYCOMMAND);

	if (!pMenu)
		return FALSE;

	Assign(lpmii, pMenu);

	return TRUE;
}

//-----------------------------------------------------------------------------
TBMenu* GetTBMenu(HMENU hMenu)
{
	CSingleLock l(Get_CMenuMapSection(), TRUE);
	TBMenu* pMenu = NULL;
	Get_CMenuMap().Lookup(hMenu, pMenu);
	return pMenu;
}

//-----------------------------------------------------------------------------
HMENU TBMenu::GetSafeHmenu() const
{
	HMENU hMenu = GetHMENU();
	ASSERT(this == NULL || hMenu == NULL || ::IsMenu(hMenu));
	return this == NULL ? NULL : hMenu;
}
