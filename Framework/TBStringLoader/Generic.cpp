
#include "StdAfx.h"

#include <math.h>
#include <shlwapi.h>
#include "generic.h"

//------------------------------------------------------------------------------
CSize GetTextSize(CFont* pFont, const CString& str)
{              
	CDC		dc; dc.CreateIC(_T("DISPLAY"), NULL, NULL, NULL);
	CFont*	pOldFont = dc.SelectObject(pFont);
	CSize cs = dc.GetTextExtent(str, str.GetLength());

	TEXTMETRIC tm;
	dc.GetTextMetrics(&tm);
	cs.cy = tm.tmHeight + tm.tmExternalLeading;

	dc.SelectObject(pOldFont);
	return cs;
}

//------------------------------------------------------------------------------
CSize GetMultilineTextSize(CFont* pFont, CString strT)
{              
	if (strT.Find(_T('\n')) >= 0)
	{
		TCHAR* nextTK;
		TCHAR* pT = strT.GetBuffer(strT.GetLength());
		TCHAR* pRow = _tcstok_s(pT, _T("\n") ,&nextTK);

		int nMaxMultilineTitleWidth = 0;
		CSize cs;
			
		while (pRow)
		{
			cs = GetTextSize(pFont, pRow);

			if (cs.cx > nMaxMultilineTitleWidth)
				nMaxMultilineTitleWidth = cs.cx;

			pRow = _tcstok_s(NULL, _T("\n"),&nextTK);
		} 
		strT.ReleaseBuffer();

		return CSize(nMaxMultilineTitleWidth, cs.cy);
	}
	else
		return GetTextSize(pFont, strT);
}

//------------------------------------------------------------------------------
BOOL CheckRectSizes(CWnd* pWnd, float fRatio, CStringArray& arWrongStrings, CStringArray& arExpected, CStringArray& arActual, CStringArray& arDeviations)
{
	if (!pWnd) return FALSE;

	BOOL bResult = TRUE;
	CWnd* pwndChild = pWnd->GetWindow(GW_CHILD);
	while (pwndChild)
	{
		float deviation = 0;
		int expectedSize = 0, actualSize = 0;
		if (!CheckRectSize(pwndChild, fRatio, expectedSize, actualSize))
		{
			CString strText;
			pwndChild->GetWindowText(strText);
			arWrongStrings.Add(strText);
			CString strDeviation, strExpected, strActual;
			
			strActual.Format(_T("%d"), actualSize);
			arActual.Add(strActual);

			strExpected.Format(_T("%d"), expectedSize);
			arExpected.Add(strExpected);

			deviation = ((float)(expectedSize-actualSize))/((float)actualSize);
			strDeviation.Format(_T("%3.2f%%"),deviation*100 );

			arDeviations.Add(strDeviation);
			bResult = FALSE;
		}

		pwndChild = pwndChild->GetNextWindow();
	}

	return bResult;
}

//------------------------------------------------------------------------------
BOOL CheckRectSize(CWnd* pWnd, float fRatio, int &expectedSize, int &actualSize)
{
	CString strText; 
	pWnd->GetWindowText(strText);
	
	if (strText.IsEmpty()) return TRUE;

	CSize tmpSize = GetTextSize(pWnd->GetFont(), _T("O"));

	CSize theoricSize = GetMultilineTextSize(pWnd->GetFont(), strText);
	theoricSize = CSize
						(
							(int) ceil((float)theoricSize.cx*fRatio), 
							theoricSize.cy 
						);
	

	CRect r;
	pWnd->GetWindowRect(r); 
	
	CSize size = r.Size();
	if (theoricSize.cy > size.cy) 
	{
		expectedSize = theoricSize.cy;
		actualSize = size.cy;	
		return FALSE;
	}
	
	if (theoricSize.cx < size.cx) return TRUE;
	
	BOOL bMultiline = (pWnd->GetStyle() & SS_LEFTNOWORDWRAP) != SS_LEFTNOWORDWRAP;
	if (!bMultiline) 
	{
		expectedSize = theoricSize.cx;
		actualSize = size.cx;
		return FALSE;
	}

	int nRows = (int)floor((float)size.cy/(float)tmpSize.cy);

	if (theoricSize.cx/nRows > size.cx) 
	{
		expectedSize = theoricSize.cx/nRows;
		actualSize = size.cx;
		return FALSE;
	}
	
	return TRUE;
}

//------------------------------------------------------------------------------
CString EscapeString(LPCTSTR lpcstrString)
{
	ASSERT(lpcstrString);

	CString strNew;
	TCHAR ch;
	while (ch = lpcstrString[0])
	{
		switch (ch)
		{
			case _T('\n'): strNew += _T("\\n"); break;
			case _T('\r'): strNew += _T("\\r"); break;
			case _T('\t'): strNew += _T("\\t"); break;
			case _T('\\'): strNew += _T("\\\\"); break;
			case _T('\a'): strNew += _T("\\a"); break;
			case _T('\b'): strNew += _T("\\b"); break;
			case _T('\f'): strNew += _T("\\f"); break;
			case _T('\v'): strNew += _T("\\v"); break;
			case _T('\"'): strNew += _T("\\\""); break;
			default: strNew += ch;
		}

		lpcstrString++;
	}

	return strNew;
}

//------------------------------------------------------------------------------
CString UnescapeString(LPCTSTR lpcstrString)
{
	ASSERT(lpcstrString); 

	CString strNew;
	TCHAR ch;
	while (ch = lpcstrString[0])
	{
		if (ch == '\\')
		{
			switch (lpcstrString[1])
			{
				case _T('n'): strNew += _T('\n'); lpcstrString++; break;
				case _T('r'): strNew += _T('\r'); lpcstrString++; break;
				case _T('t'): strNew += _T('\t'); lpcstrString++; break;
				case _T('\\'): strNew += _T('\\'); lpcstrString++; break;
				case _T('a'): strNew += _T('\a'); lpcstrString++; break;
				case _T('b'): strNew += _T('\b'); lpcstrString++; break;
				case _T('f'): strNew += _T('\f'); lpcstrString++; break;
				case _T('v'): strNew += _T('\v'); lpcstrString++; break;
				case _T('?'): strNew += _T('\?'); lpcstrString++; break;
				case _T('"'): strNew += _T('\"'); lpcstrString++; break;
				default: strNew += ch;
			}
		}
		else
			strNew += ch;

		lpcstrString++;
	}

	return strNew;
}