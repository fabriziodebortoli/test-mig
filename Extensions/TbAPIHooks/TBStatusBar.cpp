#include "StdAfx.h"
#include "TBStatusBar.h"



//----------------------------------------------------------------------------
LRESULT TBStatusBar::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case WM_SETTEXT:
		{
			m_arText.SetAtGrow(0, (LPCTSTR)lParam); 
			return __super::DefWindowProc(message, wParam, lParam);
		}
	case SB_GETBORDERS:
		{
			if (wParam != 0)
				return FALSE;

			int* rgBorders = (int*)lParam;

			rgBorders[0] = 0;
			rgBorders[1] = 2;
			rgBorders[2] = 2;
			return 1L;
		}
	case SB_SETPARTS:
		{
			m_arParts.RemoveAll();
			int* pIndex = (int*)lParam;
			int nCount = (int) wParam;
			for (int i = 0; i < nCount; i++)
				m_arParts.Add(pIndex[i]);
			return TRUE;
		}
	case SB_GETPARTS:
		{
			int nCount = (int) wParam;
			int* pIndex = (int*)lParam;
			for (int i = 0; i < min (nCount, m_arParts.GetCount()); i++)
				pIndex[i] = m_arParts[i];
			return m_arParts.GetCount();
		}
	case SB_GETTEXT:
		{
			return 1L;
		}
	case SB_SETTEXT:
		{
			int nIdx = LOBYTE(LOWORD(wParam));
			LPCTSTR szText = (LPCTSTR)lParam;
			if (nIdx == SB_SIMPLEID)
				m_text = szText;
			else
				m_arText.SetAtGrow(nIdx, szText);
			return TRUE;
		}
	case SB_GETRECT:
		{
			int idx = (int) wParam;
			if (idx < - 1 || idx >= m_arParts.GetCount())
				return FALSE;
			LPRECT lpRect = (LPRECT)lParam;
			lpRect->left  = (idx == 0) ? 0 : m_arParts[idx-1];
			lpRect->top = 0;
			lpRect->bottom = m_cs.cy;
			lpRect->right = m_arParts[idx];

			return TRUE;
		}
	default:
		{
			return __super::DefWindowProc(message, wParam, lParam);
		}
	}
}
