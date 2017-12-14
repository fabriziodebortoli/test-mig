#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"


// CRichEditControl50W
class TB_EXPORT CRichEditControl50W : public CWnd
{
	DECLARE_DYNAMIC(CRichEditControl50W)

protected:
	DECLARE_MESSAGE_MAP()
	CHARRANGE m_crRE50W;
	CHARFORMAT2 m_cfRE50W;
	SETTEXTEX m_st50W;

// Constructors
public:
	CRichEditControl50W();
	virtual BOOL Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID);

// Attributes
	HINSTANCE m_hInstRichEdit50W;      // handle to MSFTEDIT.DLL
	TEXTRANGEW m_trRE50W;	//TextRangeW structure, for Unicode
	LPSTR m_lpszChar;
	LPWSTR m_lpszWChar;
	LPCSTR m_lpcsUnicode;

	GETTEXTLENGTHEX m_gtleStatusLength;
	double m_dStatusLength;

	double GetTextLength50W();
	void SetSel50W(long nStartChar, long nEndChar);
	BOOL SetDefaultCharFormat50W(DWORD dwMask, COLORREF crTextColor, DWORD dwEffects, LPTSTR szFaceName, LONG yHeight, COLORREF crBackColor);
	void SetTextTo50WControl(CString csText, int nSTFlags, int nSTCodepage);
	void LimitText50W(int nChars);
	void SetOptions50W(WORD wOp, DWORD dwFlags);
	DWORD SetEventMask50W(DWORD dwEventMask);
	void GetTextRange50W(int ncharrMin, int ncharrMax);

	virtual ~CRichEditControl50W();
protected:
};

#include "endh.dex"




