#pragma once

#include "beginh.dex"

class CInternalManagedWindowWrapper;

class TB_EXPORT ManagedWindowWrapper
{
	CInternalManagedWindowWrapper* m_pWrapper;
public:
	HWND m_hwndFromControl;
	HWND m_hwndManagedForm;

public:
	ManagedWindowWrapper	(HWND hwndControl);
	~ManagedWindowWrapper();

	void ShowRangeSelector(HWND hwndParent, int anchorDateDay, int anchorDateMonth, int anchorDateYear, CString sFilePath);
	void ShowMonthCalendar(HWND hwndParent, int anchorDateDay, int anchorDateMonth, int anchorDateYear, int selectedDateDay, int selectedDateMonth, int selectedDateYear);
};

#include "endh.dex"