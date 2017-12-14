
#pragma once


#include "beginh.dex"

BOOL	CheckRectSizes(CWnd* pWnd, float fRatio, CStringArray& arWrongStrings, CStringArray& arExpected, CStringArray& arActual, CStringArray& arDeviations);
BOOL	CheckRectSize(CWnd* pWnd, float fRatio, int &expectedSize, int &actualSize);
CString EscapeString(LPCTSTR lpcstrBaseString);
CString UnescapeString(LPCTSTR lpcstrBaseString);

inline UINT GetHashCode(LPCWSTR key)
{
	UINT nHash = 0;
	while (*key)
		nHash = (nHash<<5) + nHash + *key++;
	return nHash;
}

#include "endh.dex"