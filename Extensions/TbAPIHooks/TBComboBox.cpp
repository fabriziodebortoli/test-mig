#include "StdAfx.h"
#include "TBComboBox.h"
#include "TBListBox.h"
#include "TBEdit.h"

#define DEFAULT_ITEM_HEIGHT 21
//----------------------------------------------------------------------------
void TBComboBox::SendSelChange()
{
	GetParent()->SendMessage(WM_COMMAND, MAKEWPARAM(m_id, CBN_SELCHANGE), (LPARAM)m_hWnd);
}

//----------------------------------------------------------------------------
void TBComboBox::SendSetFocus()
{
	GetParent()->SendMessage(WM_COMMAND, MAKEWPARAM(m_id, CBN_SETFOCUS), (LPARAM)m_hWnd);
}
//----------------------------------------------------------------------------
void TBComboBox::SendKillFocus()
{
	GetParent()->SendMessage(WM_COMMAND, MAKEWPARAM(m_id, CBN_KILLFOCUS), (LPARAM)m_hWnd);
}
//----------------------------------------------------------------------------
TBComboBox::TBComboBox(HWND hwnd, DWORD dwThreadId) : TBWnd(hwnd, dwThreadId), m_bExtendedUI(FALSE)
{
	m_pEdit = new TBEdit((HWND)Get_New_CWindowMap(), dwThreadId);
	m_pEdit->SetParent(this);
	m_pEdit->SetClass(_T("Edit"));
	m_pEdit->SetWindowLong(GWL_STYLE, GetWindowLong(GWL_STYLE) & ~ES_READONLY);
	if ((m_pEdit->GetWindowLong(GWL_STYLE) & CBS_DROPDOWNLIST) == CBS_DROPDOWNLIST)
		m_pEdit->EnableWindow(FALSE);

	m_pListBox = new TBListBox((HWND)Get_New_CWindowMap(), dwThreadId);
	m_pListBox->SetParent(this);
	m_pListBox->SetClass(_T("ListBox"));
}
//----------------------------------------------------------------------------
TBComboBox::~TBComboBox()
{

}


//----------------------------------------------------------------------------
void TBComboBox::GetComboBoxInfo(PCOMBOBOXINFO pcbi)
{
	pcbi->hwndCombo = m_hWnd;
	pcbi->hwndItem = m_pEdit->GetHWND();
	pcbi->hwndList = m_pListBox->GetHWND();
}
//----------------------------------------------------------------------------
void TBComboBox::EnableWindow(BOOL bEnable)
{
	if ((m_pEdit->GetWindowLong(GWL_STYLE) & CBS_DROPDOWNLIST) != CBS_DROPDOWNLIST)
		m_pEdit->EnableWindow(bEnable);
	m_pListBox->EnableWindow(bEnable);
	__super::EnableWindow(bEnable);
}

//----------------------------------------------------------------------------
void TBComboBox::SetRectFromDialogUnits(CRect& r)
{
	__super::SetRectFromDialogUnits(r);
	m_pListBox->SetWidth(r.Width());
	m_pListBox->SetHeight(r.Height());
	m_pEdit->SetWidth(r.Width());
	m_pEdit->SetHeight(r.Height());
	
	AdjustHeight();
}


//----------------------------------------------------------------------------
void TBComboBox::AdjustHeight()
{
	//for combo, the RC height is referred to open dropdown
	//combo height depends on font and is non changeable
	SetHeight((m_pListBox->m_nItemHeight == -1 ? DEFAULT_ITEM_HEIGHT : m_pListBox->m_nItemHeight));
}
//----------------------------------------------------------------------------
LRESULT TBComboBox::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case WM_SETTEXT:
	{
		m_pEdit->SendMessage(message, wParam, lParam);
		return __super::DefWindowProc(message, wParam, lParam);
	}
	case WM_SETFOCUS:
	{
		LRESULT r = __super::DefWindowProc(message, wParam, lParam);
		SendSetFocus();
		return r;
	}
	case WM_KILLFOCUS:
	{
		LRESULT r = __super::DefWindowProc(message, wParam, lParam);
		SendKillFocus();
		return r;
	}
	case CB_GETLBTEXTLEN:
	{
		return m_pListBox->DefWindowProcW(LB_GETTEXTLEN, wParam, lParam);
	}
	case CB_GETLBTEXT:
	{
		return m_pListBox->DefWindowProcW(LB_GETTEXT, wParam, lParam);
	}
	case CB_LIMITTEXT:
		return m_pEdit->DefWindowProc(EM_LIMITTEXT, wParam, lParam);
	case CB_GETDROPPEDSTATE:
		return FALSE;
	case CB_RESETCONTENT:
		SendMessage(WM_SETTEXT, NULL, (LPARAM) _T(""));
		return m_pListBox->DefWindowProc(LB_RESETCONTENT, wParam, lParam);
	case CB_SETCURSEL:
	{
		LRESULT r = m_pListBox->DefWindowProc(LB_SETCURSEL, wParam, lParam);
		SendMessage(WM_SETTEXT, NULL, (LPARAM)m_pListBox->GetSelectedText());
		// http://msdn.microsoft.com/it-it/library/windows/desktop/bb775821%28v=vs.85%29.aspx
		// The CBN_SELCHANGE notification code is not sent when the current selection is set using the CB_SETCURSEL message.
		// SendSelChange();
		return r;
	}
	case CB_GETITEMHEIGHT:
	{
		return m_pListBox->DefWindowProc(LB_GETITEMHEIGHT, wParam, lParam);
	}
	case CB_SETITEMHEIGHT:
	{
		LRESULT l = m_pListBox->DefWindowProc(LB_SETITEMHEIGHT, wParam, lParam);
		AdjustHeight();
		return l;
	}
	case CB_GETITEMDATA:
	{
		return m_pListBox->DefWindowProc(LB_GETITEMDATA, wParam, lParam);
	}
	case CB_SETITEMDATA:
	{
		return m_pListBox->DefWindowProc(LB_SETITEMDATA, wParam, lParam);
	}
	case CB_GETCURSEL:
		return m_pListBox->DefWindowProc(LB_GETCURSEL, wParam, lParam);
	case CB_GETCOUNT:
		return m_pListBox->DefWindowProc(LB_GETCOUNT, wParam, lParam);
	case CB_GETEXTENDEDUI:
	{
		return m_bExtendedUI;
	}
	case CB_SETEXTENDEDUI:
	{
		m_bExtendedUI = wParam;
		return TRUE;
	}
	case CB_GETDROPPEDCONTROLRECT:
	{
		LPRECT lprect = (LPRECT)lParam;
		RECT r;
		GetWindowRect(&r);
		lprect->left = r.left;
		lprect->top = r.bottom;
		lprect->right = r.right;
		lprect->bottom = r.bottom + 200;
		return TRUE;
	}
	case CB_ADDSTRING:
	{
		return m_pListBox->DefWindowProc(LB_ADDSTRING, wParam, lParam);
	}
	case CB_DELETESTRING:
	{
		return m_pListBox->DefWindowProc(LB_DELETESTRING, wParam, lParam);
	}
	case CB_INSERTSTRING:
	{
		return m_pListBox->DefWindowProc(LB_INSERTSTRING, wParam, lParam);
	}
	case CB_SETEDITSEL:
	{
		return TRUE;
	}

	case CB_FINDSTRING:
	{
		return m_pListBox->DefWindowProc(LB_FINDSTRING, wParam, lParam);
	}
	case CB_FINDSTRINGEXACT: 
	{
		return m_pListBox->DefWindowProc(LB_FINDSTRINGEXACT, wParam, lParam);
	}
	default:
	{
		return __super::DefWindowProc(message, wParam, lParam);
	}
	}
}
