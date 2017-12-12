#include "StdAfx.h"
#include "TBEdit.h"

//----------------------------------------------------------------------------
LRESULT TBEdit::DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	switch(message)
	{
	case EM_LIMITTEXT:
		{
			m_nMaxLen = wParam ? wParam : UINT_MAX;
			return 1L;
		}
	case EM_SETMODIFY:
		{
			m_bModified = wParam;
			return 1L;
		}
	case EM_GETMODIFY:
		{
			return m_bModified;
		}
	case EM_SETSEL:
		{
			m_nStartChar = wParam;
			m_nEndChar = lParam;
			return 1L;
		}
	case EM_GETSEL:
		{
			if (wParam)
			{
				int *nStartChar = (int*)wParam;
				*nStartChar = m_nStartChar;
			}
			if (lParam)
			{
				int *nEndChar = (int*)lParam;
				*nEndChar = m_nEndChar;
			}
			
			return MAKELRESULT(m_nStartChar, m_nEndChar);
		}
	case EM_GETLINECOUNT:
		{
			int nCount = 0;
			int idx = 0;
			while (idx = m_text.Find(_T('\n'), idx))
				nCount++;
			return nCount;
		}
	case EM_SETREADONLY:
		{
			DWORD style = GetWindowLong(GWL_STYLE);
			if (wParam)
				style |= ES_READONLY;
			else
				style &= ~ES_READONLY;
			SetWindowLong(GWL_STYLE, style);
			return 1L;
		}
	case EM_GETRECT:
		{
			LPRECT lpRect = (LPRECT)lParam;
			memcpy(lpRect, &m_Rect, sizeof(RECT));
			return 1L;
		}
		case EM_SETRECT:
		case EM_SETRECTNP:
		{
			LPRECT lpRect = (LPRECT)lParam;
			memcpy(&m_Rect, lpRect, sizeof(RECT));
			return 1L;
		}
	case EM_SCROLLCARET:
	case EM_CANPASTE:
	case EM_CANREDO:
	case EM_CANUNDO:
	case EM_UNDO:
	case EM_REDO:
	case EM_PASTESPECIAL:
		{
			//TODO
			return 1L;
		}
	case WM_CHAR:
		{
			TCHAR nCharCode = wParam;
			
			if (!nCharCode)
				return 0L;
				
			if ( iswspace(nCharCode) || iswgraph(nCharCode))
			{ //alpha num char
				if (m_nStartChar != m_nEndChar)
				{//replace selected chars if any
					m_text = m_text.Left(m_nStartChar) + nCharCode + m_text.Mid(m_nEndChar);
				}
				else
				{//otherwise inserts char after caret
					m_text.Insert(m_nEndChar, nCharCode);
				}
				//moves caret one position to the right, non selection
				m_nStartChar++;
				m_nEndChar = m_nStartChar;
				return 0L;
			}
			if (nCharCode == VK_BACK)
			{//delete char at the left
				if (m_nStartChar == m_nEndChar)
				{//non selection, selects char to the left, will be erased
					if (m_nStartChar == 0)
						return 0L;//at the beginning, no effect
					m_nStartChar--;
				}

				//erases selected chars
				m_text = m_text.Left(m_nStartChar) + m_text.Mid(m_nEndChar);
				m_nEndChar = m_nStartChar;
				return 0L;
			}
			
			ASSERT(FALSE);//unknown char
			return 0L;
		}
	case WM_KEYDOWN:
		{
			UINT nKeyCode = wParam;
			if (nKeyCode == VK_DELETE)
			{//delete char at the rigth
				if (m_nStartChar == m_nEndChar)
				{//non selection, selects char to the rigth, will be erased
					if (m_nEndChar == m_text.GetLength())
						return 0L;//at the end, no effect
					m_nEndChar++;
				}
				//erases selected chars
				m_text = m_text.Left(m_nStartChar) + m_text.Mid(m_nEndChar);
				m_nEndChar = m_nStartChar;
				return 0L;
			}
			//UINT nCharCode = MapVirtualKey(nKeyCode, MAPVK_VK_TO_CHAR);
			////if (nCharCode)
			//{
			//	m_text.Insert(m_nEndChar, nCharCode);
				
			//}

			return 0L;
		}
	default:
		{
			return __super::DefWindowProc(message, wParam, lParam);
		}
	}
}