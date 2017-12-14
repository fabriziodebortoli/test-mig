#pragma once

#include "TBWnd.h"
class CListBoxItem
{
public:
	CListBoxItem(LPCTSTR lpszText)
		: m_sText(lpszText), 
		m_bSelected(FALSE),
		m_nData(0)
	{
	}
	CString m_sText;
	long m_nData;
	BOOL m_bSelected;
};
class CItemArray : public CArray<CListBoxItem*> 
{
	void RemoveAt(INT_PTR nIndex, INT_PTR nCount = 1)
	{
		for (int i = nIndex; i < nIndex + nCount + 1; i++)
			delete this->GetAt(i);
		__super::RemoveAt(nIndex, nCount);
	}
	void RemoveAll()
	{
		for (int i = 0; i < GetCount(); i++)
			delete this->GetAt(i);
		__super::RemoveAll();
	}
};
class TBListBox : public TBWnd
{
friend class TBComboBox;
	CArray<CListBoxItem*> m_arItems;
	int m_nItemHeight;
	int m_nSelected;

	void SendSelChange();
	void SendSetFocus();
	void SendKillFocus();
	BOOL Multiple(){ return (GetStyle() & LBS_MULTIPLESEL) == LBS_MULTIPLESEL; }
public:
	LPCTSTR GetSelectedText();
	TBListBox(HWND hwnd, DWORD dwThreadId) : TBWnd(hwnd, dwThreadId),
		m_nItemHeight(-1) {}
	~TBListBox();
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
};