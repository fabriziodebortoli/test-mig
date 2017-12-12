#pragma once
class TBWnd;


class TBMenu
{
	//datamember Tb per simulare informazioni di windows a basso livello
	UINT_PTR m_Id;
	int		m_x;
	int		m_y;
	HWND	m_hwndOwner;

	//Struttura di contenimento menu -> menuitem
	CArray<MENUITEMINFO*> m_arItems;
	MENUINFO menuinfo;

	MENUITEMINFO* GetChild(UINT uIDItem, UINT uFLags);
	MENUITEMINFO* GetChild(UINT uIDItem, UINT uFLags, int& index);
	void SetMenuData(MENUITEMINFO* menuItemInfo, UINT uFlags, LPCTSTR lpData);
	void Assign(const MENUITEMINFO* pFrom, MENUITEMINFO* pTo);
public:
	TBMenu(UINT_PTR nId = 0);
	~TBMenu();
	HMENU GetHMENU () const { return (HMENU)m_Id; }
	HMENU GetSafeHmenu() const;

	BOOL AppendMenu(UINT uFlags, UINT_PTR uIDNewItem, LPCTSTR lpNewItem);
	BOOL InsertMenu(UINT uPosition, UINT uFlags, UINT_PTR uIDNewItem, LPCTSTR  lpNewItem);

	BOOL ChangeMenuW(_In_ UINT cmd, _In_opt_ LPCWSTR lpszNewItem, _In_ UINT cmdInsert, _In_ UINT flags);
	int GetMenuStringW(_In_ UINT uIDItem, _Out_writes_opt_(cchMax) LPWSTR lpString, _In_ int cchMax, _In_ UINT flags);

	UINT GetMenuState(_In_ UINT uId, _In_ UINT uFlags);
	DWORD CheckMenuItem(_In_ UINT uIDCheckItem, _In_ UINT uCheck);

	BOOL EnableMenuItem(_In_ UINT uIDEnableItem, _In_ UINT uEnable);
	HMENU GetSubMenu(_In_ int nPos);
	UINT GetMenuItemID(_In_ int nPos);
	int GetMenuItemCount();
	BOOL ModifyMenuW(_In_ UINT uPosition, _In_ UINT uFlags, _In_ UINT_PTR uIDNewItem, _In_opt_ LPCWSTR lpNewItem);
	BOOL RemoveMenu(_In_ UINT uPosition, _In_ UINT uFlags);
	BOOL DeleteMenu(_In_ UINT uPosition, _In_ UINT uFlags);
	BOOL SetMenuItemBitmaps(_In_ UINT uPosition, _In_ UINT uFlags, _In_opt_ HBITMAP hBitmapUnchecked, _In_opt_ HBITMAP hBitmapChecked);
	BOOL TrackPopupMenu(_In_ UINT uFlags, _In_ int x, _In_ int y, _Reserved_ int nReserved, _In_ HWND hWnd, _Reserved_ CONST RECT *prcRect);
	BOOL TrackPopupMenuEx(_In_ UINT uFlags, _In_ int x, _In_ int y, _In_ HWND hwnd, _In_opt_ LPTPMPARAMS lptpm);
	BOOL GetMenuInfo(_Inout_ LPMENUINFO lpMenuInfo);
	BOOL SetMenuInfo(_In_ LPCMENUINFO lpMenuInfo);
	BOOL GetMenuItemRect(_In_opt_ HWND hWnd, _In_ UINT uItem, _Out_ LPRECT lprcItem);
	BOOL GetMenuItemInfo(_In_ UINT item, _In_ BOOL fByPosition, _Inout_ LPMENUITEMINFOW lpmii);
	BOOL SetMenuItemInfo(_In_ UINT item, _In_ BOOL fByPositon, _In_ LPCMENUITEMINFOW lpmii);
};

class CMenuMap : public CMap<HMENU, HMENU, TBMenu*, TBMenu*>
{
public:
	~CMenuMap();
};

TBMenu* GetTBMenu(HMENU hMenu);