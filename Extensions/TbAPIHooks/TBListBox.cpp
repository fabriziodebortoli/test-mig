#include "StdAfx.h"
#include "TBListBox.h"

//----------------------------------------------------------------------------
TBListBox::~TBListBox()
{
	for (int i = 0; i < m_arItems.GetCount(); i++)
		delete m_arItems[i];
}
//----------------------------------------------------------------------------
void TBListBox::SendSelChange()
{
	if ((GetStyle() & LBS_NOTIFY) == LBS_NOTIFY)
		GetParent()->SendMessage(WM_COMMAND, MAKEWPARAM(m_id, LBN_SELCHANGE), (LPARAM)m_hWnd);
}

//----------------------------------------------------------------------------
void TBListBox::SendSetFocus()
{
	GetParent()->SendMessage(WM_COMMAND, MAKEWPARAM(m_id, LBN_SETFOCUS), (LPARAM)m_hWnd);
}
//----------------------------------------------------------------------------
void TBListBox::SendKillFocus()
{
	GetParent()->SendMessage(WM_COMMAND, MAKEWPARAM(m_id, LBN_KILLFOCUS), (LPARAM)m_hWnd);
}

//----------------------------------------------------------------------------
LPCTSTR TBListBox::GetSelectedText()
{
	if (m_nSelected == -1)
		return _T("");
	return m_arItems[m_nSelected]->m_sText;
}
	
//----------------------------------------------------------------------------
LRESULT TBListBox::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{	
	switch(message)
	{
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
	case LB_GETITEMHEIGHT:
	{
		return m_nItemHeight;
	}
	case LB_SETITEMHEIGHT:
	{
		m_nItemHeight = lParam;
		return TRUE;
	}
	case LB_GETCOUNT:
		return m_arItems.GetCount();
	case LB_RESETCONTENT:
		{
			for (int i = 0; i < m_arItems.GetCount(); i++)
				delete m_arItems[i];
			m_arItems.RemoveAll();
			m_nSelected = -1;
			return 1L;
		}
	case LB_GETCURSEL:
		{
			return m_nSelected;
		}
	case LB_SETCURSEL:
		{
			int nIndex = wParam;
			if (nIndex >= m_arItems.GetCount())
			{
				return LB_ERR;
			}
			m_nSelected = nIndex;
			SendSelChange();
			return nIndex;
		}
	case LB_SETSEL:
	{
		BOOL bSet = wParam;
		int nIndex = lParam;
		if (nIndex >= m_arItems.GetCount())
		{
			return LB_ERR;
		}
		if (nIndex < 0)
		{
			for (int i = 0; i < m_arItems.GetCount(); i++)
				m_arItems[nIndex]->m_bSelected = bSet;
			SendSelChange();
			return nIndex;
		}
		else
		{
			m_arItems[nIndex]->m_bSelected = bSet;
			SendSelChange();
			return nIndex;
		}
	}
	case LB_GETSEL:
	{
		int nIndex = wParam;
		if (nIndex >= m_arItems.GetCount())
		{
			return LB_ERR;
		}
		if (nIndex < 0)
		{
			return LB_ERR;
		}
		else
		{
			return m_arItems[nIndex]->m_bSelected;
		}
	}
	case LB_ADDSTRING:
		{
			return m_arItems.Add(new CListBoxItem((LPCTSTR)lParam));
		}
	case LB_DELETESTRING:
		{
			int nIndex = wParam;
			if (nIndex < 0 || nIndex >= m_arItems.GetCount())
				return LB_ERR;
			delete m_arItems[nIndex];
			m_arItems.RemoveAt(nIndex);
			return m_arItems.GetCount();
		}
	case LB_INSERTSTRING:
		{
			int nIndex = wParam;
			m_arItems.InsertAt(nIndex, new CListBoxItem((LPCTSTR)wParam));
			return nIndex;
		}
	case LB_GETTEXT:
		{
			int nIndex = wParam;
			if (nIndex < 0 || nIndex >= m_arItems.GetCount())
				return LB_ERR;
			LPTSTR lpszBuffer = (LPTSTR)lParam;
			CString s = m_arItems[nIndex]->m_sText;
			int l = s.GetLength() + 1;
			_tcscpy_s(lpszBuffer, l, s);
			return l;
		}
	case LB_GETTEXTLEN:
		{
			int nIndex = wParam;
			if (nIndex < 0 || nIndex >= m_arItems.GetCount())
				return LB_ERR;
			CString s = m_arItems[nIndex]->m_sText;
			return s.GetLength();
		}
	case LB_FINDSTRING:
		{
			int nStart = wParam;
			LPCTSTR lpszSearch = LPCTSTR(lParam);
			int nLen = _tcslen(lpszSearch);
			for (int i = nStart + 1; i < m_arItems.GetCount(); i++)
			{
				CListBoxItem* pItem = m_arItems[i];
				if (pItem->m_sText.GetLength() >= nLen && pItem->m_sText.Left(nLen).CompareNoCase(lpszSearch) == 0)
					return i;
			}
			for (int i = 0; i <= nStart; i++)
			{
				CListBoxItem* pItem = m_arItems[i];
				if (pItem->m_sText.GetLength() >= nLen && pItem->m_sText.Left(nLen).CompareNoCase(lpszSearch) == 0)
					return i;
			}
			return -1;
		}
		case LB_FINDSTRINGEXACT:
		{
			int nStart = wParam;
			LPCTSTR lpszSearch = LPCTSTR(lParam);
			for (int i = nStart + 1; i < m_arItems.GetCount(); i++)
			{
				CListBoxItem* pItem = m_arItems[i];
				if (pItem->m_sText.CompareNoCase(lpszSearch) == 0)
					return i;
			}
			for (int i = 0; i <= nStart; i++)
			{
				CListBoxItem* pItem = m_arItems[i];
				if (pItem->m_sText.CompareNoCase(lpszSearch) == 0)
					return i;
			}
			return -1;
		}
		case LB_SETITEMDATA:
		{
			int idx = wParam;
			if (idx == -1)
			{
				for (int i = 0; i < m_arItems.GetCount(); i++)
				{
					m_arItems.GetAt(i)->m_nData = lParam;
				}
				return TRUE;
			}
			else if (idx > -1 && idx < m_arItems.GetCount())
			{
				m_arItems.GetAt(idx)->m_nData = lParam;
				// m_arItems.GetAt(idx)->m_nData = wParam;
				return TRUE;
			}
			return LB_ERR;
		}
		case LB_GETITEMDATA:
		{
			int idx = wParam;
			if (idx > -1 && idx < m_arItems.GetCount())
			{
				return m_arItems.GetAt(idx)->m_nData;
			}
			return LB_ERR;
		}
		case LB_GETITEMRECT:
			return true;
	default:
		{
			return __super::DefWindowProc(message, wParam, lParam);
		}
	}
}