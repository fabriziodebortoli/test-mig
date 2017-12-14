#include "stdafx.h"

#include <TbGeneric\GeneralFunctions.h>

#include "MiniHtmlStatic.h"


//=============================================================================
// some common character entities
CMiniHtmlDraw::MiniHtml_CHAR_ENTITIES CMiniHtmlDraw::m_aCharEntities[] = 
{
	{ _T("&amp;"),		0,	_T('&') },		// ampersand
	{ _T("&bull;"),		0,	_T('\x95') },	// bullet      NOT IN MS SANS SERIF
	{ _T("&cent;"),		0,	_T('\xA2') },	// cent sign
	{ _T("&copy;"),		0,	_T('\xA9') },	// copyright
	{ _T("&deg;"),		0,	_T('\xB0') },	// degree sign
	{ _T("&euro;"),		0,	_T('\x80') },	// euro sign
	{ _T("&frac12;"),	0,	_T('\xBD') },	// fraction one half
	{ _T("&frac14;"),	0,	_T('\xBC') },	// fraction one quarter
	{ _T("&gt;"),		0,	_T('>') },		// greater than
	{ _T("&iquest;"),	0,	_T('\xBF') },	// inverted question mark
	{ _T("&lt;"),		0,	_T('<') },		// less than
	{ _T("&micro;"),	0,	_T('\xB5') },	// micro sign
	{ _T("&middot;"),	0,	_T('\xB7') },	// middle dot = Georgian comma
	{ _T("&nbsp;"),		0,	_T(' ') },		// nonbreaking space
	{ _T("&para;"),		0,	_T('\xB6') },	// pilcrow sign = paragraph sign
	{ _T("&plusmn;"),	0,	_T('\xB1') },	// plus-minus sign
	{ _T("&pound;"),	0,	_T('\xA3') },	// pound sign
	{ _T("&quot;"),		0,	_T('"') },		// quotation mark
	{ _T("&reg;"),		0,	_T('\xAE') },	// registered trademark
	{ _T("&sect;"),		0,	_T('\xA7') },	// section sign
	{ _T("&sup1;"),		0,	_T('\xB9') },	// superscript one
	{ _T("&sup2;"),		0,	_T('\xB2') },	// superscript two
	{ _T("&times;"),	0,	_T('\xD7') },	// multiplication sign
	{ _T("&trade;"),	0,	_T('\x99') },	// trademark   NOT IN MS SANS SERIF
	{ NULL,				0,	0 }				// MUST BE LAST
};

//=============================================================================
CMiniHtmlDraw::CMiniHtmlDraw()
{
	m_nLeftMargin       = 0;
	m_nRightMargin      = 0;
	
	InitCharEntities();

	ResetAll();
}

//=============================================================================
CMiniHtmlDraw::~CMiniHtmlDraw()
{
	ResetAll();
}

//=============================================================================
void CMiniHtmlDraw::InitCharEntities()
{
	for (int i = 0; m_aCharEntities[i].pszName != NULL; i++)
	{
		m_aCharEntities[i].cCode = (TCHAR) (i + 2);	// don't use 0 or 1
	}
}

//=============================================================================
TCHAR CMiniHtmlDraw::GetCharEntity(TCHAR cCode)
{
	TCHAR c = _T(' ');

	for (int i = 0; m_aCharEntities[i].pszName != NULL; i++)
	{
		if (cCode == m_aCharEntities[i].cCode)
		{
			c = m_aCharEntities[i].cSymbol;
			break;
		}
	}

	return c;
}

//=============================================================================
void CMiniHtmlDraw::ResetAll()
{
	Reset();

	m_nLeftMargin	= 0;
	m_nRightMargin	= 0;
}

//=============================================================================
// Reset    (now called by SetWindowText() instead of ResetAll())
void CMiniHtmlDraw::Reset()
{
	m_bUnderline			= FALSE;
	m_bBold					= FALSE;
	m_bItalic				= FALSE;
	m_bCenter				= FALSE;
	m_bStrikeThrough		= FALSE;
	m_bSubscript			= FALSE;
	m_bSuperscript			= FALSE;
	m_bHorizontalRule		= FALSE;
	m_nHorizontalRuleSize	= 2;
	m_bGeneratedText		= FALSE;
}

//=============================================================================
void CMiniHtmlDraw::Draw(HDC hDC, CStatic* pWnd, COLORREF crText, COLORREF crBackground)
{
	ASSERT(hDC);
	if (hDC == NULL)
		return;

	CRect rect;
	pWnd->GetClientRect(&rect);

	// get text from control
	CString strText;
	pWnd->GetWindowText(strText);

	// replace character entity names with codes
	TCHAR ent[3] = { 0 };
	ent[0] = _T('\001');	// each entity name is replaced with a two-character
							// code that begins with \001

	for (int i = 0; m_aCharEntities[i].pszName != NULL; i++)
	{
		ent[1] = m_aCharEntities[i].cCode;
		strText.Replace(m_aCharEntities[i].pszName, ent);
	}

	CString str1;
	int index = 0;

	// ---- set text and background colors
	//COLORREF crText = m_crMHText;
	COLORREF prev_crText = crText;

	//COLORREF crBackground = m_crMHBackGround;
	COLORREF prev_crBackground = crBackground;

	//if (!(pWnd->GetExStyle() & WS_EX_TRANSPARENT))
	//{
	//	HBRUSH hbrush = CreateSolidBrush(crBackground); 
	//	ASSERT(hbrush);
	//	FillRect(hDC, &rect, hbrush);
	//	if (hbrush)
	//		DeleteObject(hbrush);
	//}

	// nothing to do if no text or not visible
	if (strText.IsEmpty() || !pWnd->IsWindowVisible())
		return;

	int n = strText.GetLength();

	// allow for margins
	rect.left += m_nLeftMargin;
	rect.right -= m_nRightMargin;

	int nInitialXOffset = 0;//m_nLeftMargin;
	m_yStart = rect.top;

	LOGFONT lf = { 0 };
	LOGFONT prev_lf = { 0 };


	CFont* cf = pWnd->GetFont();
	if (cf != NULL)
	{
		VERIFY(cf->GetObject(sizeof(lf), &lf));
	}
	else
	{
		HFONT hFont = (HFONT)::GetCurrentObject(hDC, OBJ_FONT);	//+++1.1
		if (hFont)
			GetObject(hFont, sizeof(lf), &lf);
		else
			GetObject(GetStockObject(SYSTEM_FONT), sizeof(lf), &lf);
	}
	
	lf.lfWeight    = m_bBold ? FW_BOLD : FW_NORMAL;
	lf.lfUnderline = (BYTE) m_bUnderline;
	lf.lfItalic    = (BYTE) m_bItalic;
	lf.lfStrikeOut = (BYTE) m_bStrikeThrough;

	memcpy(&prev_lf, &lf, sizeof(lf));

	// create initial font
	HFONT hNewFont = CreateFontIndirect(&lf);
	ASSERT(hNewFont);
	HFONT hOldFont = (HFONT) SelectObject(hDC, hNewFont);

	CString strAnchorText = _T("");

	BOOL bSizeChange = FALSE;
	TEXTMETRIC tm = { 0 };
	GetTextMetrics(hDC, &tm);

	while (n > 0)
	{
		///////////////////////////////////////////////////////////////////////
		if (_tcsnicmp(strText, _T("<B>"), 3) == 0)	// check for <b> or <B>
		{
			n -= 3;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			m_bBold++;// = TRUE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</B>"), 4) == 0)	// check for </B>
		{
			n -= 4;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (m_bBold)
				m_bBold--;// = FALSE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<I>"), 3) == 0)	// check for <I>
		{
			n -= 3;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			m_bItalic++;// = TRUE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</I>"), 4) == 0)	// check for </I>
		{
			n -= 4;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (m_bItalic)
				m_bItalic--;// = FALSE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<U>"), 3) == 0)		// check for <U>
		{
			n -= 3;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			m_bUnderline++;// = TRUE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</U>"), 4) == 0)	// check for </U>
		{
			n -= 4;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (m_bUnderline)
				m_bUnderline--;// = FALSE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<CENTER>"), 8) == 0)	// check for <CENTER>
		{
			n -= 8;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			m_bCenter++;// = TRUE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</CENTER>"), 9) == 0)	// check for </CENTER>
		{
			n -= 9;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (m_bCenter)
				m_bCenter--;// = FALSE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<STRIKE>"), 8) == 0)	// check for <STRIKE>
		{
			n -= 8;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			m_bStrikeThrough++;// = TRUE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</STRIKE>"), 9) == 0)	// check for </STRIKE>
		{
			n -= 9;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (m_bStrikeThrough)
				m_bStrikeThrough--;// = FALSE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<BIG>"), 5) == 0)	// check for <BIG>
		{
			n -= 5;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (lf.lfHeight > 0)
				lf.lfHeight++;
			else
				lf.lfHeight--;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</BIG>"), 6) == 0)	// check for </BIG>
		{
			n -= 6;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (lf.lfHeight > 0)
				lf.lfHeight--;
			else
				lf.lfHeight++;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<SMALL>"), 7) == 0)	// check for <SMALL>
		{
			n -= 7;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (lf.lfHeight > 0)
				lf.lfHeight--;
			else
				lf.lfHeight++;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</SMALL>"), 8) == 0)	// check for </SMALL>
		{
			n -= 8;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (lf.lfHeight > 0)
				lf.lfHeight++;
			else
				lf.lfHeight--;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<SUB>"), 5) == 0)	// check for <SUB>
		{
			n -= 5;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			m_bSubscript++;// = TRUE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</SUB>"), 6) == 0)	// check for </SUB>
		{
			n -= 6;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (m_bSubscript)
				m_bSubscript--;// = FALSE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<SUP>"), 5) == 0)	// check for <SUP>
		{
			n -= 5;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			m_bSuperscript++;// = TRUE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</SUP>"), 6) == 0)	// check for </SUP>
		{
			n -= 6;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			if (m_bSuperscript)
				m_bSuperscript--;// = FALSE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<FONT"), 5) == 0)	// check for <FONT
		{
			index = strText.Find(_T('>'));
			if (index != -1)
			{
				CString strAttributes = strText.Mid(5, index-5);
				int m = strAttributes.GetLength();
				strText = strText.Mid(index+1);

				// loop to parse FONT attributes
				while (m > 0)
				{
					// trim left whitespace
					if ((strAttributes.GetLength() > 0) && 
						(strAttributes[0] == _T(' ')))
					{
						m--;
						strAttributes = strAttributes.Mid(1);
						continue;
					}

					///////////////////////////////////////////////////////////
					if (_tcsnicmp(strAttributes, _T("COLOR"), 5) == 0)
					{
						int index2 = strAttributes.Find(_T('"'));
						if (index2 != -1)
						{
							m -= index2 + 1;
							strAttributes = strAttributes.Mid(index2+1);

							index2 = strAttributes.Find(_T('"'));
							if (index2 != -1)
							{
								CString strColor = strAttributes.Left(index2);
								crText = GetColorFromString(strColor);

								strAttributes = strAttributes.Mid(index2+1);
								m = strAttributes.GetLength();
							}
						}
						else
							break;
					}
					///////////////////////////////////////////////////////////
					else if (_tcsnicmp(strAttributes, _T("BGCOLOR"), 7) == 0)
					{
						int index2 = strAttributes.Find(_T('"'));
						if (index2 != -1)
						{
							m -= index2 + 1;
							strAttributes = strAttributes.Mid(index2+1);

							index2 = strAttributes.Find(_T('"'));
							if (index2 != -1)
							{
								CString strBgColor = strAttributes.Left(index2);
								crBackground = GetColorFromString(strBgColor);

								strAttributes = strAttributes.Mid(index2+1);
								m = strAttributes.GetLength();
							}
						}
						else
							break;
					}
					///////////////////////////////////////////////////////////
					else if (_tcsnicmp(strAttributes, _T("FACE"), 4) == 0)
					{
						int index2 = strAttributes.Find(_T('"'));
						if (index2 != -1)
						{
							m -= index2 + 1;
							strAttributes = strAttributes.Mid(index2+1);
							index2 = strAttributes.Find(_T('"'));
							if (index2 != -1)
							{
								memset(lf.lfFaceName, 0, sizeof(lf.lfFaceName));
								_tcsncpy_s(lf.lfFaceName, 32, strAttributes, index2);

								m -= index2 + 1;
								if (m > 0)
									strAttributes = strAttributes.Mid(index2+1);
								else
									strAttributes = _T("");
								m = strAttributes.GetLength();
							}
						}
						else
							break;
					}
					///////////////////////////////////////////////////////////
					else if (_tcsnicmp(strAttributes, _T("SIZE"), 4) == 0)
					{
						int index2 = strAttributes.Find(_T('"'));
						if (index2 != -1)
						{
							m -= index2 + 1;
							strAttributes = strAttributes.Mid(index2+1);
							index2 = strAttributes.Find(_T('"'));
							if (index2 != -1)
							{
								int nSize = 0;
								nSize = _ttoi(strAttributes);
								lf.lfHeight -= nSize;
								bSizeChange = TRUE;

								m -= index2 + 1;
								if (m > 0)
									strAttributes = strAttributes.Mid(index2+1);
								else
									strAttributes = _T("");
								m = strAttributes.GetLength();
							}
						}
						else
							break;
					}
					else
					{
						while ((strAttributes.GetLength() > 0) && 
							   (strAttributes[0] != _T(' ')))
						{
							m--;
							strAttributes = strAttributes.Mid(1);
						}
					}
				}
				n -= index + 1;
			}
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("</FONT>"), 7) == 0)	// check for </FONT>
		{
			n -= 7;
			index = strText.Find(_T('>'));
			if (index != -1)
				strText = strText.Mid(index+1);
			crText = prev_crText;
			crBackground = prev_crBackground;
			memcpy(&lf, &prev_lf, sizeof(lf));
			if (bSizeChange)
				m_yStart += tm.tmDescent;
			bSizeChange = FALSE;
			continue;
		}
		///////////////////////////////////////////////////////////////////////
		else if (_tcsnicmp(strText, _T("<HR"), 3) == 0)	// check for <HR>
		{
			index = strText.Find(_T('>'));
			if (index != -1)
			{
				CString strAttributes = strText.Mid(3); //, index-3);
				int m = index; //strAttributes.GetLength();
				strText = strText.Mid(index+1);

				// loop to parse attributes
				while (m > 0)
				{
					// trim left whitespace
					if ((strAttributes.GetLength() > 0) && 
						(strAttributes[0] == _T(' ')))
					{
						m--;
						strAttributes = strAttributes.Mid(1);
						continue;
					}

					///////////////////////////////////////////////////////////
					if (_tcsnicmp(strAttributes, _T("SIZE"), 4) == 0)
					{
						int index2 = strAttributes.Find(_T('='));
						if (index2 != -1)
						{
							m -= index2 + 1;
							strAttributes = strAttributes.Mid(index2+1);

							index2 = strAttributes.Find(_T('>'));
							if (index2 != -1)
							{
								CString strSize = strAttributes.Left(index2);
								strSize.Remove('"');
								m_nHorizontalRuleSize = _ttoi(strSize);
								strAttributes = strAttributes.Mid(index2+1);
								m = strAttributes.GetLength();
							}
						}
						else
							break;
					}
					else
					{
						while ((strAttributes.GetLength() > 0) && 
							   (strAttributes[0] != _T(' ')))
						{
							m--;
							strAttributes = strAttributes.Mid(1);
						}
					}
				}
				n -= index + 1;
			}

			m_bHorizontalRule++;// = TRUE;
			str1 = _T("\r\n");
			m_bGeneratedText = TRUE;
		}
		///////////////////////////////////////////////////////////////////////
		// <br> or \r\n or plain text
		else
		{
			str1 = strText;
			index = str1.Find(_T('<'));
			if (index != -1)
			{
				if (_tcsnicmp(strText, _T("<BR>"), 4) == 0)	// check for <BR>
				{
					n -= 4;
					str1 = _T("\r\n");
					m_bGeneratedText = TRUE;
					strText = strText.Mid(4);
				}
				else
				{
					str1 = strText.Left(index);
					if (str1.GetLength() <= 0)
					{
						if (strText.GetLength() != 0)
						{
							str1 = strText[0];
							index = 1;
							n -= 1;
						}
					}
					strText = strText.Mid(index);
				}
			}
			else
			{
				str1 = strText;
				strText = _T("");
			}
		}

		lf.lfWeight    = m_bBold ? FW_BOLD : FW_NORMAL;
		lf.lfUnderline = (BYTE) m_bUnderline;
		lf.lfItalic    = (BYTE) m_bItalic;
		lf.lfStrikeOut = (BYTE) m_bStrikeThrough;

		// update font

		if (hOldFont)
			SelectObject(hDC, hOldFont);
		if (hNewFont)
			DeleteObject(hNewFont);

		hNewFont = CreateFontIndirect(&lf);
		ASSERT(hNewFont);

		hOldFont = (HFONT) SelectObject(hDC, hNewFont);

		::SetTextColor(hDC, crText);
//TODO
		if (pWnd->GetExStyle() & WS_EX_TRANSPARENT)
			::SetBkMode(hDC, TRANSPARENT);
		else
			::SetBkColor(hDC, crBackground);

		GetTextMetrics(hDC, &tm);
		int nBaselineAdjust = tm.tmAscent / 2;

		if (m_bSubscript)
		{
			rect.top += nBaselineAdjust;
			rect.bottom += nBaselineAdjust;
			m_yStart += nBaselineAdjust;
		}
		if (m_bSuperscript)
		{
			rect.top -= nBaselineAdjust;
			rect.bottom -= nBaselineAdjust;
			m_yStart -= nBaselineAdjust;
		}
		int saved_left = rect.left;
		if (m_bCenter)
		{
			SIZE size;
			GetTextExtentPoint32(hDC, str1, (int)_tcslen(str1), &size);
			int w = rect.Width();
			rect.left = max(0, (w - size.cx) / 2);
		}

		nInitialXOffset = FormatText(hDC, str1, &rect, nInitialXOffset);
		rect.left = saved_left;
		if (str1 == _T("\r\n"))
		{
			nInitialXOffset = 0;
		}

		if (m_bSubscript)
		{
			rect.top -= nBaselineAdjust;
			rect.bottom -= nBaselineAdjust;
			m_yStart -= nBaselineAdjust;
		}
		if (m_bSuperscript)
		{
			rect.top += nBaselineAdjust;
			rect.bottom += nBaselineAdjust;
			m_yStart += nBaselineAdjust;
		}

		// draw horizontal rule 
		if (m_bHorizontalRule)
		{
			int nPenWidth = m_nHorizontalRuleSize;
			HPEN hPen = CreatePen(PS_SOLID, nPenWidth, crText);
			ASSERT(hPen);

			if (hPen)
			{
				HPEN hOldPen = (HPEN) SelectObject(hDC, hPen);

				::MoveToEx	(hDC, rect.left		+ m_nLeftMargin, rect.top, NULL);
				::LineTo	(hDC, rect.right	- m_nRightMargin, rect.top);	//- al posto di +

				if (hOldPen)
					SelectObject(hDC, hOldPen);

				DeleteObject(hPen);
			}

			m_yStart += nPenWidth;
			rect.top += nPenWidth;
			rect.bottom += nPenWidth;
			nInitialXOffset = 0;

			m_bHorizontalRule--;
		}

		if (!m_bGeneratedText)
			n -= str1.GetLength();
		m_bGeneratedText = FALSE;
	}


	// clean up font
	if (hOldFont)
		SelectObject(hDC, hOldFont);
	if (hNewFont)
		DeleteObject(hNewFont);

	// Do not call CStatic::OnPaint() for painting messages
}

//=============================================================================
BOOL CMiniHtmlDraw::IsBlank(LPCTSTR lpszText)
{
	TCHAR c;
	while ((c = *lpszText++) != _T('\0'))
		if (c != _T(' ') && c != _T('\t'))
			return FALSE;
	return TRUE;
}

//=============================================================================
int CMiniHtmlDraw::FormatText(HDC hdc, 
							 LPCTSTR lpszText, 
							 RECT * pRect, 
							 int nInitialXOffset)
{
	//TRACE(_T("in CMiniHtmlDraw::FormatText:  nInitialXOffset=%d  <%-20.20s>\n"), 
		//nInitialXOffset, lpszText);
	//TRACERECT(*pRect);

	int		xStart, nWord, xNext, xLast, nLeftMargin;
	TCHAR	*pText = (TCHAR *) lpszText;
	SIZE	size;

	xNext = nInitialXOffset;
	nLeftMargin = nInitialXOffset;
	xLast = 0;

	if (pRect->top >= (pRect->bottom-1))
		return 0;

	// set initial size
	TCHAR * szTest = _T("abcdefgABCDEFG");

	//GetTextExtentPoint32(hdc, pText, (int)_tcslen(pText), &size);
	//if (size.cx > (pRect->right - pRect->left))
		GetTextExtentPoint32(hdc, szTest, (int)_tcslen(szTest), &size);

	// prepare for next line - clear out the error term
	SetTextJustification(hdc, 0, 0);

	CString strOut;

	BOOL bReturnSeen = FALSE;

	TEXTMETRIC tm = { 0 };
	::GetTextMetrics(hdc, &tm);

	do									// for each text line
	{
		nWord = 0;						// initialize number of spaces in line

		// skip to first non-space in line
		while (/**pText != _T('\0') && */*pText == _T(' '))
		{
			if (xNext)
				strOut += *pText;
			pText++;
		}

		for(;;)							// process each word
		{
			CString strWord;
			TCHAR *saved_pText = pText;
			strWord = GetNextWord(&pText, &bReturnSeen);

			CString strTrial;
			strTrial = strOut + strWord;

			// after each word, calculate extents
			nWord++;
			CString s(strTrial); s.Replace(' ', '=');
			GetTextExtentPoint32(hdc, s, strTrial.GetLength(), &size);

			BOOL bOverflow = (size.cx >= (pRect->right - xNext - 2));	
											// don't get too close to margin,
											// in case of italic text

			if (bOverflow)
			{
				if (strOut.IsEmpty())
				{
					bOverflow = FALSE;
					strOut = strWord;

					// FOLLOWING CHANGE SUGGESTED BY DAVID PRITCHARD
					if (xNext > 0) 
					{
						m_yStart += size.cy;
						xStart = 0;
						xNext = 0;
					}
					// --------  END CHANGE  --------
				}
			}
			else
			{
				strOut += strWord;
			}

			if (bReturnSeen || bOverflow || (*pText == _T('\0')))
			{
				if (strOut.IsEmpty())
					break;

				if (bOverflow)
					pText = saved_pText;
				nWord--;               // discount last space at end of line

				// if end of text and no space characters, set pEnd to end
				CString s(strOut); s.Replace(' ', '=');
				GetTextExtentPoint32(hdc, s, strOut.GetLength(), &size);

				xStart = pRect->left;
				xStart += xNext;
				xNext = 0;
				xLast = xStart + size.cx;

				// display the text

				if ((m_yStart <= (pRect->bottom-size.cy)))// && (!IsBlank(strOut)))
				{
					TextOut(hdc, xStart, m_yStart, strOut, strOut.GetLength());
					if (*pText || bReturnSeen)
						m_yStart += size.cy;
				}

				// prepare for next line - clear out the error term
				SetTextJustification(hdc, 0, 0);

				strOut.Empty();
			}
			else	// new word will fit
			{

			}
		}

		nWord--;               // discount last space at end of line

		// if end of text and no space characters, set pEnd to end

		// prepare for next line - clear out the error term
		SetTextJustification(hdc, 0, 0);

		strOut.Empty();

	} while (*pText && (m_yStart < pRect->bottom));

	if (m_yStart > (pRect->bottom-size.cy))
		pRect->top = pRect->bottom;
	else
		pRect->top = m_yStart;

	return xLast + 1;
}

//=============================================================================
CString CMiniHtmlDraw::GetNextWord(TCHAR **ppText, BOOL * pbReturnSeen)
{
	CString strWord;
	strWord = _T("");
	TCHAR *pText = *ppText;

	*pbReturnSeen = FALSE;

	// skip to next word

	for(;;)
	{
		if (*pText == _T('\0'))
			break;

		// skip \r
		if (*pText == _T('\r'))
			pText++;

		// \n = new line
		if (*pText == _T('\n'))
		{
			strWord += _T(' ');
			pText++;
			*pbReturnSeen = TRUE;
			break;
		}

		TCHAR c = *pText;

		// process character entities
		if (c == _T('\001'))
		{
			c = *++pText;

			c = GetCharEntity(c);
		}

		strWord += c;

		if (*pText++ == _T(' '))
			break;
	}

	*ppText = pText;

	return strWord;
}
