#include "stdafx.h"

#include "RichEditControl50W.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////

// CRichEditControl50W
IMPLEMENT_DYNAMIC(CRichEditControl50W, CWnd)
CRichEditControl50W::CRichEditControl50W()
{
}

CRichEditControl50W::~CRichEditControl50W()
{
	//Free the MSFTEDIT.DLL library
	if(m_hInstRichEdit50W)
		FreeLibrary(m_hInstRichEdit50W);
}

BEGIN_MESSAGE_MAP(CRichEditControl50W, CWnd)
END_MESSAGE_MAP()

BOOL CRichEditControl50W::Create(DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID)
{
	//Load the MSFTEDIT.DLL library
	m_hInstRichEdit50W = LoadLibrary(_T("MSFTEDIT.DLL"));
	if (!m_hInstRichEdit50W)
	{
		AfxMessageBox(_T("MSFTEDIT.DLL Didn't Load"));
		return(0);
	}

	CWnd* pWnd = this;
	return pWnd->Create(_T("RichEdit50W"), NULL, dwStyle, rect, pParentWnd, nID);
}

void CRichEditControl50W::SetSel50W(long nStartChar, long nEndChar)
{
	m_crRE50W.cpMin = nStartChar;
	m_crRE50W.cpMax = nEndChar;
	SendMessage(EM_EXSETSEL, 0, (LPARAM)&m_crRE50W);
}

BOOL CRichEditControl50W::SetDefaultCharFormat50W(DWORD dwMask, COLORREF crTextColor, DWORD dwEffects, LPTSTR  szFaceName, LONG yHeight, COLORREF crBackColor)
{	//Set the text defaults.  CHARFORMAT2 m_cfStatus is declared in RichEditControl50W.h
	m_cfRE50W.cbSize = sizeof(CHARFORMAT2);
	m_cfRE50W.dwMask = dwMask ;
	m_cfRE50W.crTextColor = crTextColor;
	m_cfRE50W.dwEffects = dwEffects;
	::lstrcpy(m_cfRE50W.szFaceName, szFaceName);
	m_cfRE50W.yHeight = yHeight;
	m_cfRE50W.crBackColor = crBackColor;

	return (BOOL) SendMessage(EM_SETCHARFORMAT, 0, (LPARAM)&m_cfRE50W);
}

void CRichEditControl50W::SetTextTo50WControl(CString csText, int nSTFlags, int nSTCodepage)
{
	//Set the options. SETTEXTEX m_st50W declared in RichEditControl50W.h
	m_st50W.codepage = nSTCodepage;	
	m_st50W.flags = nSTFlags;

	#ifdef _UNICODE
		//UNICODE
	    USES_CONVERSION;
	    m_lpcsUnicode = W2A( csText.LockBuffer( ) );
	    csText.UnlockBuffer( );
		SendMessage(EM_SETTEXTEX,(WPARAM)&m_st50W,(LPARAM)(LPCTSTR)m_lpcsUnicode);
	#else
		//MBCS or NOT SET
		SendMessage(EM_SETTEXTEX, (WPARAM)&m_st50W,(LPARAM)(LPCTSTR)csText);
	#endif
}
void CRichEditControl50W::LimitText50W(int nChars)
{
	SendMessage(EM_LIMITTEXT, nChars, 0);
}

void CRichEditControl50W::SetOptions50W(WORD wOp, DWORD dwFlags)
{
	SendMessage(EM_SETOPTIONS, (WPARAM)wOp, (LPARAM)dwFlags);
}

double CRichEditControl50W::GetTextLength50W()
{
	//Returns the number of megabytes of text in the control
	m_gtleStatusLength.flags = GTL_NUMBYTES;
	m_gtleStatusLength.codepage = 1200;

	return ((float)(SendMessage(EM_GETTEXTLENGTHEX, (WPARAM)&m_gtleStatusLength, (LPARAM)NULL))/1000000);
}

DWORD CRichEditControl50W::SetEventMask50W(DWORD dwEventMask)
{
	return (DWORD)SendMessage(EM_SETEVENTMASK, 0, dwEventMask);
}

void CRichEditControl50W::GetTextRange50W(int ncharrMin, int ncharrMax)
{
	//Set the CHARRANGE for the trRE50W = the characters sent by ENLINK 
	m_trRE50W.chrg.cpMin = ncharrMin;
	m_trRE50W.chrg.cpMax = ncharrMax;

	//Set the size of the character buffers, + 1 for null character
	int nLength = int((m_trRE50W.chrg.cpMax - m_trRE50W.chrg.cpMin +1));

	#ifdef _UNICODE
		//UNICODE
		m_lpszWChar = new WCHAR[nLength];
		//Set the trRE50W LPWSTR character buffer = Unicode buffer
		m_trRE50W.lpstrText = m_lpszWChar;
		//Get the Unicode text
		SendMessage(EM_GETTEXTRANGE, 0,  (LPARAM) &m_trRE50W);  
	#else
		//MBCS or NOT SET
		//create an ANSI buffer and a Unicode (Wide Character) buffer
		m_lpszChar = new CHAR[nLength];
		m_lpszWChar = new WCHAR[nLength];

		//Set the trRE50W LPWSTR character buffer = Unicode buffer
		m_trRE50W.lpstrText = m_lpszWChar;

		//Get the Unicode text
		SendMessage(EM_GETTEXTRANGE, 0,  (LPARAM) &m_trRE50W);  

		//Convert the Unicode RTF text to ANSI.
		WideCharToMultiByte(CP_ACP, 0, m_lpszWChar, -1, m_lpszChar, nLength, NULL, NULL);
	#endif

	return;
}

