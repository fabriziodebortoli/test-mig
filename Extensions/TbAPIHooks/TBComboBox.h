#pragma once
#include "TBWnd.h"
class TBComboBox : public TBWnd
{
	TBEdit*		m_pEdit;
	TBListBox*	m_pListBox;
	BOOL		m_bExtendedUI;

	void SendSelChange();
	void SendSetFocus();
	void SendKillFocus();
	void AdjustHeight();
public:
	TBComboBox(HWND hwnd, DWORD dwThreadId);
	~TBComboBox();
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	virtual void EnableWindow(BOOL bEnable);
	virtual void SetRectFromDialogUnits(CRect& rect);
	void GetComboBoxInfo(PCOMBOBOXINFO pcbi);

};