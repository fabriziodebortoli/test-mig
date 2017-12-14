#include "stdafx.h"
#include "TBHtmlParser.h"

#include <TbGeneric\GeneralFunctions.h>

//-----------------------------------------------------------------------------
TBHtmlParser::TBHtmlParser()
{
	m_bPreview = FALSE;
}

//-----------------------------------------------------------------------------
TBHtmlParser::~TBHtmlParser() 
{
}

//-----------------------------------------------------------------------------
CString TBHtmlParser::GetSubAttributeValue(CString stTag, CString StAtt)
{
	stTag = stTag.MakeUpper();
	INT nPosStart = stTag.Find(StAtt);
	if (nPosStart < 0) return _T("");
	INT nPosColum = stTag.Find(HtmlColon, nPosStart);
	if (nPosColum < 0) return _T("");
	nPosColum++;
	INT nPosSemiCol = stTag.Find(HtmlSemicolon, nPosStart);
	if (nPosSemiCol > nPosColum)
	{
		INT nLen = nPosSemiCol - nPosColum;
		if (nLen <= 0) return _T("");
		return stTag.Mid(nPosColum, nLen).Trim().MakeUpper();
	}
	INT nPosA = stTag.Find(_T("'"), nPosStart);
	if (nPosA < 0) return _T("");
	INT nLen = nPosA- nPosColum;
	return stTag.Mid(nPosColum, nLen).Trim().MakeUpper();
}

//-----------------------------------------------------------------------------
CString TBHtmlParser::GetAttributeValue(CString stTag, CString StAtt)
{
	stTag = stTag.MakeUpper();
	INT nPosStart = stTag.Find(StAtt);
	if (nPosStart < 0) return _T("");
	INT nPosA = stTag.Find(_T("'"), nPosStart);
	if (nPosA < 0) return _T("");
	INT nPosB = stTag.Find(_T("'"), nPosA + 1);
	if (nPosB < 0) return _T("");
	nPosA ++;
	if (nPosA > nPosB) return _T("");

	INT nLen = nPosB - nPosA;
	if (nPosB <= 0) return _T("");
	return stTag.Mid(nPosA,  nLen).Trim().MakeUpper();
}

//-----------------------------------------------------------------------------
CString TBHtmlParser::GetTag(CString stTag)
{
	INT iS = stTag.Find(HtmlStartTag);
	INT iT = stTag.Find(HtmlStopTag);
	if (iS < 0 || iT < 0 || iT < iS) return _T("");
	CString stRet = stTag.Mid(iS + 1, iT - iS -1);
	return stRet.MakeUpper();
}

//-----------------------------------------------------------------------------
HtmlAlign TBHtmlParser::GetAlign(CString val)
{
	val = val.Trim();
	if (val.CompareNoCase(HtmlPosLeft) == 0)
	{
		return HtmlAlign::LEFT;
	}
	else if (val.CompareNoCase(HtmlPosRight) == 0)
	{
		return HtmlAlign::RIGHT;
	}
	else if (val.CompareNoCase(HtmlPosCenter) == 0)
	{
		return HtmlAlign::CENTER;
	}
	return HtmlAlign::NONEALIGN;
}

//-----------------------------------------------------------------------------
HtmlOverflow TBHtmlParser::GetOverflow(CString val)
{
	val = val.Trim();
	if (val.CompareNoCase(HtmlVisible) == 0)
	{
		return HtmlOverflow::VISIBLE;
	} 
	else if (val.CompareNoCase(HtmlHidden) == 0)
	{
		return HtmlOverflow::HIDDEN;
	}
	else if (val.CompareNoCase(HtmlScroll) == 0)
	{
		return HtmlOverflow::SCROLL;
	}
	else if (val.CompareNoCase(HtmlAuto) == 0)
	{
		return HtmlOverflow::AUTO;
	}
	return HtmlOverflow::NONEOVERFLOW;
}

//-----------------------------------------------------------------------------
COLORREF TBHtmlParser::ConvertHexColorToColorRef(CString val)
{
	LPCTSTR pszTmp = val;
	pszTmp++; // cut the #

	LPTSTR pStop;
	INT nTmp = _tcstol(pszTmp, &pStop, 16);
	INT nR = (nTmp & 0xFF0000) >> 16;
	INT nG = (nTmp & 0xFF00) >> 8;
	INT nB = (nTmp & 0xFF);
	return RGB(nR, nG, nB);
}

// http://www.w3schools.com/colors/colors_names.asp
//-----------------------------------------------------------------------------
BOOL TBHtmlParser::ConvertColorToColorRef(CString sFontColor, COLORREF& colorRef)
{
	CMap<CString, LPCTSTR, COLORREF, COLORREF> mapColor;
	mapColor.SetAt(_T("RED"), RGB(255, 0, 0));
	mapColor.SetAt(_T("GREEN"), RGB(0, 255, 0));
	mapColor.SetAt(_T("BLUE"), RGB(0, 0, 255));
	mapColor.SetAt(_T("CYAN"), RGB(0, 255, 255));
	mapColor.SetAt(_T("YELLOW"), RGB(255, 255, 0));
	mapColor.SetAt(_T("BLACK"), RGB(0, 0, 0));
	mapColor.SetAt(_T("WHITE"), RGB(255, 255, 255));
	mapColor.SetAt(_T("BISQUE"), RGB(0xFF, 0xE4, 0xC4));
	mapColor.SetAt(_T("AQUA"), RGB(0x00, 0xFF, 0xFF));
	mapColor.SetAt(_T("AQUAMARINE"), RGB(0x7F, 0xFF, 0xD4));
	
	if (!sFontColor.IsEmpty())
	{
		if (sFontColor.Left(1).Compare(HtmlHash) == 0)
		{
			colorRef = ConvertHexColorToColorRef(sFontColor);
			return TRUE;
		}
		else if (mapColor.Lookup(sFontColor, colorRef))
		{
			return TRUE;
		}
	}

	// Defoult color Black
	return FALSE;
}

//-----------------------------------------------------------------------------
CString	TBHtmlParser::GetHref(CRect inRect)
{
	POSITION pos = m_mapURL.GetStartPosition();
	CRect rectUrl;
	CString strUrl;
	while (pos != NULL)
	{
		m_mapURL.GetNextAssoc(pos, strUrl, rectUrl);

		POINT a;POINT b;
		a.x = inRect.left;
		a.y = inRect.top;
		b.x = inRect.right;
		b.y = inRect.bottom;

		if (inRect.PtInRect(a) && inRect.PtInRect(b))
			return strUrl;
	}
	return _T("");
}

//-----------------------------------------------------------------------------
CRect TBHtmlParser::RenderText(HtmlNode node, CDC* pDc, CRect rect, stHtmlFont stFont, BOOL bCalc /*FALSE*/)
{
	m_mapURL.RemoveAll();
	stFont.rectOrig = rect;
	stFont.rectNext = CRect(0,0,0,0);
	stHtmlCalc stRet = RenderTextRecursive(node, pDc, rect, stFont, bCalc);
	CRect retRect = stRet.rectCalc;
	retRect.left = rect.left;
	return retRect;
}

//-----------------------------------------------------------------------------
stHtmlCalc TBHtmlParser::RenderTextRecursive(HtmlNode node, CDC* pDc, CRect rect, stHtmlFont stFont, BOOL bCalc)
{
	int iElements = node.Count;
	CFont* pPrevFont = NULL;
	CRect rectCalculate(rect);
	CRect rectNext(stFont.rectNext); // last text calculate

	// Html List
	INT			iListLavel	= stFont.iListLavel;
	CString		sListMarker	= stFont.sListMarker;
	INT			iListCount	= stFont.iListCounter;
	BOOL		bDiv		= stFont.bDiv;
	INT			iDivWidth	= stFont.iDivWidth;

 	for (int n = 0;n < iElements;n++) 
	{
		BOOL			bFontSize = stFont.bFontSize;
		BOOL			bFontColor = stFont.bFontColor;
		BOOL			bBackground = stFont.bBackground;
		BOOL			bBold = stFont.bBold;
		BOOL			bItalic = stFont.bItalic;
		BOOL			bUnderline = stFont.bUnderline;
		BOOL			bStrikethrough = stFont.bStrikethrough;
		int				iFontSize = stFont.iFontSize;
		COLORREF		cText = stFont.cText;
		COLORREF		cBackground = stFont.cBackground;
		CString			sFace = stFont.sFace;
		CRect			rectOrig = stFont.rectOrig;
		HtmlAlign		iDivFloat = stFont.iDivFloat;
		HtmlAlign		iDivTextAlign = stFont.iDivTextAlign;
		HtmlOverflow	iOverflow = stFont.iOverflow;
		CString			sHref = stFont.sHref;

		HtmlNode e = node.Nodes[n];
		CString tagName = (e->szName).MakeUpper();
		CString tagLeft = e->GetTagLeft();
		// <font size = ”n” color = ”color”>
		if (tagName.Compare(HtmlTagFont) == 0)
		{
			if (!tagLeft.IsEmpty())
			{
				CString sFontSize = GetAttributeValue(tagLeft, HtmlAttSize);
				if (!sFontSize.IsEmpty())
				{
					int iSize = _wtoi(sFontSize);
					iFontSize = iSize;
					bFontSize = TRUE;
				}

				CString sFontFace = GetAttributeValue(tagLeft, HtmlAttInFace);
				if (!sFontFace.IsEmpty())
				{
					sFace = sFontFace.Trim();
				}

				CString sFontColor = GetAttributeValue(tagLeft, HtmlAttColor);
				if (!sFontColor.IsEmpty())
					bFontColor = ConvertColorToColorRef(sFontColor, cText);
			}
		}
		else  if ((tagName.Compare(HtmlTagSPAN) == 0) || (tagName.Compare(HtmlTagA) == 0)) // SPAN <span style="color:#FF0000;background-color:#80BFFF;">tre</span>
		{																				   // Attribute <a style=”color:#80BFFF; background-color: #ffffff"”>: - <a href="http://www.w3schools.com">Visit W3Schools.com!</a>
			INT nHtmlColon = -1;
			INT nHtmlSemicolon = -1;
			INT iFind = 0;

			if ((tagName.Compare(HtmlTagA) == 0))
			{
				CString tmpHref = GetAttributeValue(tagLeft, HtmlAttHref);
				if (!tmpHref.IsEmpty())
					sHref = tmpHref;
			}

			CString sStyle = GetAttributeValue(tagLeft, HtmlAttStyle);
			if (!sStyle.IsEmpty())
			{
				do
				{
					nHtmlSemicolon = sStyle.Find(HtmlSemicolon, iFind);
					nHtmlColon = sStyle.Find(HtmlColon, iFind);
					if (nHtmlColon > 0)
					{
						CString sTag = sStyle.Mid(iFind, nHtmlColon - iFind).Trim();
						if (sTag.Compare(HtmlAttInBackground) == 0)
						{
							CString sBackground = GetSubAttributeValue(tagLeft, HtmlAttInBackground);
							if (!sBackground.IsEmpty())
								bBackground = ConvertColorToColorRef(sBackground, cBackground);
						}
						else if (sTag.Compare(HtmlAttInColor) == 0)
						{
							CString sColor = GetSubAttributeValue(tagLeft, HtmlAttInColor);
							if (!sColor.IsEmpty())
								bFontColor = ConvertColorToColorRef(sColor, cText);
						}
						else
							ASSERT(FALSE);
					}
					iFind = nHtmlSemicolon + 1;
				} while (nHtmlSemicolon > 0);
			}
		}
		else if (tagName.Compare(HtmlTagB) == 0) // Bold 
		{
			bBold = TRUE;
		}
		else if (tagName.Compare(HtmlTagI) == 0) // Italic 
		{
			bItalic = TRUE;
		}
		else if (tagName.Compare(HtmlTagU) == 0) // Underline 
		{
			bUnderline = TRUE;
		}
		else if (tagName.Compare(HtmlTagS) == 0) // Strikethrough 
		{
			bStrikethrough = TRUE;
		}
		else if (tagName.Compare(HtmlTagUL) == 0) // Unordered List
		{
			iListLavel++;
			sListMarker.Empty();
			iListCount = 0;
			CString sStyle = GetAttributeValue(tagLeft, HtmlAttStyle);
			if (!sStyle.IsEmpty())
			{
				int nHtmlColon = sStyle.Find(HtmlColon);
				if (nHtmlColon > 0)
					sListMarker = sStyle.Mid(nHtmlColon+1).Trim().MakeUpper();
				
			}
		}
		else if (tagName.Compare(HtmlTagOL) == 0) // Ordered List
		{
			iListLavel++;
			sListMarker.Empty();
			iListCount = 0;
			CString sStyle = GetAttributeValue(tagLeft, HtmlListType);
			if (!sStyle.IsEmpty())
			{
				sListMarker = sStyle.Trim().MakeUpper();
			}
		}
		else if (tagName.Compare(HtmlTag) == 0) // HTML
		{ // skip
		}
		else if ((tagName.Compare(HtmlTagBR) == 0) || (tagName.Compare(HtmlTagLI) == 0))
		{
			INT iBot = rectCalculate.top + rectNext.Height() + 1;
			if (rectCalculate.bottom < iBot)
				rectCalculate.bottom = iBot;
			rectCalculate.top = rectCalculate.top + rectNext.Height() + 1;
			rectCalculate.right = rectOrig.right;
			rectCalculate.left = rectOrig.left;

			if (tagName.Compare(HtmlTagLI) == 0) 
				iListCount++;
		}
		else  if (tagName.Compare(HtmlTagDIV) == 0) // DIV <div style="width:140px;float:left;text-align:center;background-color:bisque;"> center < / div>
		{
			INT nHtmlColon = -1;
			INT nHtmlSemicolon = -1;
			INT iFind = 0;
			bDiv = TRUE;
			CString sStyle = GetAttributeValue(tagLeft, HtmlAttStyle);
			iDivWidth = 0;
			do
			{
				nHtmlSemicolon = sStyle.Find(HtmlSemicolon, iFind);
				nHtmlColon = sStyle.Find(HtmlColon, iFind);
				if (nHtmlColon > 0)
				{
					CString sTag = sStyle.Mid(iFind, nHtmlColon - iFind).Trim();
					if (sTag.Compare(HtmlAttInBackground) == 0)
					{
						CString sFontColor = GetSubAttributeValue(tagLeft, HtmlAttInBackground);
						if (!sFontColor.IsEmpty())
							bBackground = ConvertColorToColorRef(sFontColor, cBackground);
					}
					else if (sTag.Compare(HtmlAttInColor) == 0)
					{
						CString sColor = GetSubAttributeValue(tagLeft, HtmlAttInColor);
						if (!sColor.IsEmpty())
							bFontColor = ConvertColorToColorRef(sColor, cText);
					}
					else if (sTag.Compare(HtmlAttInWidth) == 0)
					{
						CString sDivWidth = GetSubAttributeValue(tagLeft, HtmlAttInWidth).MakeUpper();
						if (!sDivWidth.IsEmpty())
						{
							sDivWidth.Replace(HtmlUnitPx, _T(""));
							iDivWidth = max(0, _ttoi(sDivWidth));
							if (pDc->IsPrinting())
							{
								iDivWidth = MulDiv(iDivWidth, pDc->GetDeviceCaps(LOGPIXELSX), SCALING_FACTOR);
							}
						}
					}
					else if (sTag.Compare(HtmlAttInFloat) == 0)
					{
						CString sFloat = GetSubAttributeValue(tagLeft, HtmlAttInFloat).MakeUpper();
						if (!sFloat.IsEmpty())
						{
							iDivFloat = GetAlign(sFloat);
						}
					}
					else if (sTag.Compare(HtmlAttInTextAlign) == 0)
					{
						CString sTextAlign = GetSubAttributeValue(tagLeft, HtmlAttInTextAlign).MakeUpper();
						if (!sTextAlign.IsEmpty())
						{
							iDivTextAlign = GetAlign(sTextAlign);
						}
					}
					else if (sTag.Compare(HtmlAttInOverflow) == 0)
					{
						CString sOverflow = GetSubAttributeValue(tagLeft, HtmlAttInOverflow).MakeUpper();
						if (!sOverflow.IsEmpty())
						{
							iOverflow = GetOverflow(sOverflow);
						}
					}
					else
						ASSERT(FALSE);
				}
				iFind = nHtmlSemicolon + 1;
			} while (nHtmlSemicolon > 0);
		}
		else
		{
			// Tag not present
			ASSERT(FALSE);
		}

		// Get string to print 
		CString stStart = e->lpszStartStart;
		CString stStop  = e->lpszStartStop;
		{
			INT iC = stStop.Find(HtmlStartTag);
			if (iC > 0)
			{
				CString StToPrint = stStop.Left(iC);
				CString StTmp = StToPrint;
				if (!StTmp.Trim().IsEmpty())
				{
					COLORREF colPrevText = pDc->GetTextColor();
					COLORREF colPrevBck	 = pDc->GetBkColor();
					LOGFONT lf;
					CFont *currentFont = pDc->GetCurrentFont();
					currentFont->GetLogFont(&lf);
					
					// Font scaling in more face Print, print preview & screen
					if (m_bPreview)
					{
						if (bFontSize)
							lf.lfHeight = -MulDiv(iFontSize, GetDeviceCaps(pDc->m_hDC, LOGPIXELSY), 72);
						if (pDc->IsPrinting())
							ScaleLogFont(&lf, *pDc);
					}
					else
					{
						if (bFontSize)
						{
							if (pDc->IsPrinting())
							{
								lf.lfHeight = -MulDiv(iFontSize, GetDeviceCaps(pDc->m_hDC, LOGPIXELSY), SCALING_FACTOR);
							}
							else
							{
								lf.lfHeight = -MulDiv(iFontSize, GetDeviceCaps(pDc->m_hDC, LOGPIXELSY), 72);
							}
						}
						else
						{
							if (pDc->IsPrinting())
								ScaleLogFont(&lf, *pDc);
						}
					}

					if (bBold)			lf.lfWeight = FW_BOLD;
					if (bItalic)		lf.lfItalic = TRUE;
					if (bUnderline)		lf.lfUnderline = TRUE;
					if (bFontColor)		pDc->SetTextColor(cText);
					if (bBackground)	pDc->SetBkColor(cBackground);
					if (!sFace.IsEmpty()) 
					{
						_tcsncpy_s(lf.lfFaceName, LF_FACESIZE, sFace, sFace.GetLength());
					}

					CFont newFont;
					newFont.CreateFontIndirect(&lf);
					pPrevFont = pDc->SelectObject(&newFont);
					// Print in DC contet the string
					UINT uiTextFormat = DT_LEFT | DT_SINGLELINE /*| DT_WORDBREAK*/;
					CRect rectText(rectCalculate);
					// Calculate for print mode in screen
					pDc->DrawText(StToPrint, rectText, uiTextFormat | DT_CALCRECT);
					// Different font in more line
					if (rectNext.Height() > 0 && rectNext.Height() != rectText.Height() && rectCalculate.left > rectOrig.left)
						rectCalculate.top = rectNext.top  + (rectNext.Height() - rectText.Height());
					if (iListLavel > 0)
					{
						CRect rectCalcSpace(rectCalculate);
						pDc->DrawText(_T("XX"), rectCalcSpace, uiTextFormat | DT_CALCRECT);
						rectCalculate.left += (iListLavel * rectCalcSpace.Width());

						// Market
						if (sListMarker.Compare(HtmlListMarkedCircle) == 0 || sListMarker.Compare(HtmlListMarkedDisc) == 0) // Disk or Circle
						{
							CPen shapePen(PS_SOLID, 2, pDc->GetTextColor());
							CPen* oldPen = pDc->SelectObject(&shapePen);
							CBrush* oldBrush;
							CBrush brush;
							if (sListMarker.Compare(HtmlListMarkedDisc) == 0)
							{	
								brush.CreateStockObject(NULL_BRUSH);
								oldBrush = (CBrush*)pDc->SelectObject(&brush);
							}
							else
							{
								brush.CreateSolidBrush(pDc->GetTextColor());
								oldBrush = (CBrush*)pDc->SelectObject(&brush);
							}

							INT c = (rectCalcSpace.Height() / 3);
							CRect rMark(rectCalculate.left + c, rectCalcSpace.top + c,
								rectCalculate.left + rectCalcSpace.Height() - c, rectCalcSpace.top + rectCalcSpace.Height() - c);
							pDc->Ellipse(rMark);
							rectCalculate.left += rectCalcSpace.Height();
							pDc->SelectObject(oldBrush);
							pDc->SelectObject(oldPen);
						}
						else if (sListMarker.Compare(HtmlListMarkedSquare) == 0) // Square
						{
							INT c = (rectCalcSpace.Height() / 3);
							CRect rMark(rectCalculate.left + c,	rectCalcSpace.top  + c,
										rectCalculate.left + rectCalcSpace.Height() - c, rectCalcSpace.top  + rectCalcSpace.Height() - c);

							CBrush BrushMarker(pDc->GetTextColor());
							pDc->FillRect(rMark, &BrushMarker);
							rectCalculate.left += rectCalcSpace.Height();
						}
						else if (sListMarker.Compare(HtmlListMarkedNone) == 0) // None
						{
						}
						else if (sListMarker.Compare(HtmlListMarkedNumber) == 0) // 1.2.3..
						{
							CString sNum;
							sNum.Format(_T("%d."), iListCount);
							sNum.Append(StToPrint);
							StToPrint = sNum;
						}
						else if (sListMarker.Compare(HtmlListMarkedUppercaseLetters) == 0) // A.B.C...
						{
							CString sNum;
							sNum.Append(_T("     "));
							sNum.SetAt(0, 0x40 + iListCount);
							sNum.Trim();
							sNum.Append(_T("."));
							sNum.MakeUpper();
							sNum.Append(StToPrint);
							StToPrint = sNum;
							if (iListCount >= 0x5A /*Z*/) iListCount = 1;
						}
						else if (sListMarker.Compare(HtmlListMarkedLowercaseLetters) == 0) // a.b.c...
						{
							CString sNum;
							sNum.Append(_T("     "));
							sNum.SetAt(0, 0x40 + iListCount);
							sNum.Trim();
							sNum.Append(_T("."));
							sNum.MakeLower();
							sNum.Append(StToPrint);
							StToPrint = sNum;
							if (iListCount >= 0x5A /*Z*/) iListCount = 1;
						}
					}

					// Print string in DC
					if (!bCalc)
					{
						CRect cRectText(rectCalculate);

						if (bDiv)
						{
							if (iDivWidth <= 0)
							{
								iDivWidth = cRectText.Width();
							}

							CBrush brush;
							brush.CreateSolidBrush(cBackground);
							cRectText.top		= cRectText.top;
							cRectText.bottom	= cRectText.top + rectText.Height();
							cRectText.left		= cRectText.left;
							cRectText.right		= cRectText.left + iDivWidth;
							pDc->FillRect(cRectText, &brush);

							if (iDivTextAlign == HtmlAlign::CENTER)
							{
								uiTextFormat = DT_CENTER | DT_SINGLELINE;
							}
							else if (iDivTextAlign == HtmlAlign::RIGHT)
							{
								uiTextFormat = DT_RIGHT | DT_SINGLELINE;
							}
						}

						if (!sHref.IsEmpty())
						{
							m_mapURL.SetAt(sHref, rectText);
						}

						pDc->DrawText(StToPrint, cRectText, uiTextFormat);
						// Draw Line if Strikethrough
						if (bStrikethrough)
						{
							int x0 = cRectText.left;
							int x1 = x0 + rectText.Width();
							int y0 = cRectText.top + (rectText.Height() / 2);

							if (uiTextFormat & DT_CENTER)
							{
								x0 = cRectText.left + (cRectText.Width() / 2)  - (rectText.Width() /2);
								x1 = x0  + rectText.Width();
							}
							else if (uiTextFormat & DT_RIGHT)
							{
								x0 = cRectText.right - rectText.Width();
								x1 = cRectText.right;
							}

							for (int yk = y0; yk < y0 + 2; yk++ )
							{
								pDc->MoveTo(x0, yk);
								pDc->LineTo(x1, yk);
							}
						}
					}
					
					// Replace old value
					pDc->SetBkColor(colPrevBck);
					pDc->SetTextColor(colPrevText);
					pDc->SelectObject(pPrevFont);
					rectNext = rectText;
					
					INT iRi = rectText.right + 2;
					if (rectCalculate.right < iRi)
						rectCalculate.right = iRi;
					rectCalculate.left = rectText.right + 1;
				}
			}
		}

		if (!node.Nodes[n].IsLeaf())
		{
			stHtmlFont stH;
			stH.bFontSize = bFontSize;
			stH.bFontColor = bFontColor;
			stH.bBackground = bBackground;
			stH.bBold = bBold;
			stH.bItalic = bItalic;
			stH.bUnderline = bUnderline;
			stH.bStrikethrough = bStrikethrough;
			stH.iFontSize = iFontSize,
			stH.cText = cText;
			stH.cBackground = cBackground;
			stH.rectOrig = rectOrig;
			stH.rectNext = rectNext;
			stH.sFace = sFace;

			stH.bDiv = bDiv;
			stH.iDivWidth = iDivWidth;
			stH.iDivFloat = iDivFloat;
			stH.iDivTextAlign = iDivTextAlign;
			stH.iOverflow = iOverflow;

			// Html List
			stH.iListLavel = iListLavel;
			stH.sListMarker = sListMarker;
			stH.iListCounter = iListCount;

			stH.sHref		= sHref;

			stHtmlCalc stRetx = RenderTextRecursive(node.Nodes[n], pDc, rectCalculate, stH, bCalc);
			rectCalculate = stRetx.rectCalc;
			rectNext = stRetx.rectNext;

			if ((tagName.Compare(HtmlTagUL) == 0) || (tagName.Compare(HtmlTagOL) == 0))
			{
				if (iListLavel <= 1)
				{
					INT iBot = rectCalculate.top + rectNext.Height() + 1;
					if (rectCalculate.bottom < iBot)
						rectCalculate.bottom = iBot;
					rectCalculate.top = rectCalculate.top + rectNext.Height() + 1;
					rectCalculate.right = rectOrig.right;
					rectCalculate.left = rectOrig.left;
				}
				if (iListLavel > 0) iListLavel--;
			}
		}
	} // End For


	// DIV post
	if (bDiv && iDivWidth > 0)
	{
		bDiv = FALSE;
		rectNext.right = rectNext.left + iDivWidth;
		rectCalculate.left = rectNext.right + 1;
	}

	stHtmlCalc stRet;
	stRet.rectCalc = rectCalculate;
	stRet.rectNext = rectNext;
	return stRet;
}

//-----------------------------------------------------------------------------
void TBHtmlParser::HtmlToArray(CString stHtml, CStringArray* pStrArraySplit, CUIntArray* pUiArray)
{
	if (pStrArraySplit == NULL || pUiArray == NULL)
		return;

	INT iSearch = 0;
	INT itagStart = 0;
	INT itagStop = 0;

	// Organize the string HTML
	do
	{
		itagStart = stHtml.Find(HtmlStartTag, iSearch);
		if (itagStart >= 0)
		{
			// Get text 
			if (itagStop > 0 && itagStop < itagStart)
			{
				CString stText = stHtml.Mid(itagStop + 1, itagStart - itagStop - 1);
				CString stTest = stText;
				if (!stText.IsEmpty() && !stTest.TrimLeft().IsEmpty())
				{
					pStrArraySplit->Add(stText);
					pUiArray->Add(stText.GetLength());
				}
			}

			itagStop = stHtml.Find(HtmlStopTag, itagStart + 1);
			if (itagStop > itagStart)
			{
				CString stTag = stHtml.Mid(itagStart, itagStop - itagStart + 1);
				pStrArraySplit->Add(stTag);
				pUiArray->Add(0);
				iSearch = itagStop + 1;
			}
			else
				iSearch = itagStart + 1;
		}
	} while (itagStart >= 0);
}

//-----------------------------------------------------------------------------
void TBHtmlParser::SplitHtmlText(CString stHtml, UINT nCar, CString* split1, CString* split2)
{	
	ASSERT(split1);
	ASSERT(split2);

	CStringArray strArraySplit;
	CUIntArray   uiArray;
	HtmlToArray(stHtml, &strArraySplit, &uiArray);
	
	int nElement = strArraySplit.GetCount()-1;
	if (nElement < 0) return;
	BOOL bRemove = true;
	UINT  nCarToSplit = nCar;

	for (INT k = nElement; k >= 0; k--)
	{
		CString stIn = strArraySplit[k];
		UINT    uiVal = uiArray[k];
		if (uiVal > 0)
		{
			if (bRemove)
			{
				if (uiVal > nCarToSplit)
				{
					INT j = uiVal - nCarToSplit;
					CString stp1 = stIn.Mid(0, j);
					CString stp2 = stIn.Mid(j);

					split1->Insert(0, stp1);
					split2->Insert(0, stp2);
					bRemove = FALSE;
				}
				else if (uiVal == nCarToSplit)
				{
					split2->Insert(0, stIn);
					bRemove = FALSE;
				}
				else
				{
					ASSERT(nCarToSplit > uiVal);
					nCarToSplit -= uiVal;
					split2->Insert(0, stIn);
				}
			}
			else
			{
				split1->Insert(0, stIn);
			}
			
		}
		else
		{
			split1->Insert(0, stIn);
			split2->Insert(0, stIn);
		}
	}
} 

//-----------------------------------------------------------------------------
HtmlTree TBHtmlParser::GetHtmlhTree(CString tHTML)
{
	HtmlTree hTree;
	if (tHTML.IsEmpty()) return hTree;
	CLiteHTMLReader theReader;
	CHtmlElementCollection theElementCollectionHandler;
	theReader.setEventHandler(&theElementCollectionHandler);
	theElementCollectionHandler.InitWantedTag(_T(""));
	if (theReader.Read(tHTML))
	{
		hTree = theElementCollectionHandler.GetTree();
	}
	return hTree;
}

// Split For tag (Br - LI)
//-----------------------------------------------------------------------------
void TBHtmlParser::HTMLSplitter(CString stHtml, CStringArray* pStrArrayHTML)
{
	CString splitL;
	CString stToHtmlSplitter = stHtml;
	BOOL bSlit = FALSE;

	CStringArray strArraySplit;
	CUIntArray   uiArray;
	HtmlToArray(stToHtmlSplitter, &strArraySplit, &uiArray);
	int nElement = strArraySplit.GetCount();
	int nStartEl = 0;
	do
	{
		if (nElement < 0) return;
		splitL.Empty();
		bSlit = FALSE;
		for (INT k = 0; k < nElement; k++)
		{
			CString stIn = strArraySplit[k];
			UINT    uiVal = uiArray[k];

			if (uiVal > 0)
			{
				if (k >= nStartEl && !bSlit)
					splitL.Append(stIn);
			}
			else
			{
				if (GetTag(stIn).Compare(HtmlTagBR) == 0)
				{
					if (k >= nStartEl)
					{
						if (!bSlit) nStartEl = k + 1;
						bSlit = TRUE;
					}
				}
				else if (GetTag(stIn).Compare(HtmlTagLIClose) == 0)
				{
					if (k >= nStartEl)
					{
						if (!bSlit) nStartEl = k + 1;
						bSlit = TRUE;
					}
					splitL.Append(stIn);
				}
				else
				{
					splitL.Append(stIn);
				}
			}
		}
		pStrArrayHTML->Add(splitL);
	} while (bSlit);
}

//-----------------------------------------------------------------------------
CString TBHtmlParser::HtmlCorrection(CString inHTML)
{
	CStringArray strArraySplit;
	CUIntArray   uiArray;
	CString stHTMLElab;

	inHTML.Replace(_T("\n"), _T(""));
	inHTML = HtmlTagOpen + inHTML + HtmlTagClose;
	HtmlToArray(inHTML, &strArraySplit, &uiArray);
	int nElement = strArraySplit.GetCount();
	if (nElement < 0) return inHTML;

	for (INT k = 0; k < nElement; k++)
	{
		CString stIn = strArraySplit[k];
		UINT    uiVal = uiArray[k];

		if (uiVal > 0)
		{
			stHTMLElab.Append(_T("<A>"));
			stHTMLElab.Append(stIn);
			stHTMLElab.Append(_T("</A>"));
		}
		else
			stHTMLElab.Append(stIn);
	}
	return stHTMLElab;
}

//-----------------------------------------------------------------------------
stHtmlFont TBHtmlParser::HtmlFontInit(CDC* pDc)
{
	stHtmlFont stH;
	stH.bFontSize = FALSE;
	stH.bFontColor = FALSE;
	stH.bBackground = FALSE;
	stH.bBold = FALSE;
	stH.bItalic = FALSE;
	stH.bUnderline = FALSE;
	stH.bStrikethrough = FALSE;
	stH.iFontSize = -1;
	stH.cText = pDc->GetTextColor();
	stH.cBackground = pDc->GetBkColor();
	stH.sFace.Empty();

	stH.bDiv = FALSE;
	stH.iDivWidth = -1;
	stH.iDivFloat = HtmlAlign::NONEALIGN;
	stH.iDivTextAlign = HtmlAlign::NONEALIGN;
	stH.iOverflow = HtmlOverflow::NONEOVERFLOW;
	
	// Htmnl list
	stH.iListLavel = 0;
	stH.sListMarker.Empty();
	stH.iListCounter = 0;

	stH.sHref.Empty();

	return stH;
}

//-----------------------------------------------------------------------------
BOOL TBHtmlParser::HTMLDcStringSplitter(CDC* pDc, CRect rect, CString stHtml, CStringArray* pStrArrayHTML)
{
	ASSERT(pDc);
	ASSERT(pStrArrayHTML);

	if (pStrArrayHTML == NULL || pDc == NULL || stHtml.Trim().IsEmpty())
		return FALSE;

	// Correction & ajust Html in input 
	stHtml = HtmlCorrection(stHtml);

	CStringArray arHtml;
	HTMLSplitter(stHtml, &arHtml);
	for (INT k = 0; k < arHtml.GetCount(); k++)
	{
		CString stToSplit = arHtml[k];
		CRect newRect(rect);
		CString retSplt1 = stToSplit;
		CString retSplt2;
		int nCut = 0;
		do
		{
			HtmlTree hTree = GetHtmlhTree(retSplt1);
			if (hTree.Count <= 0)
				return FALSE;

			CRect recRet = RenderText(hTree, pDc, rect, HtmlFontInit(pDc), TRUE);
			int w = recRet.Width();

			if (w > rect.Width())
			{
				nCut++;
				retSplt1.Empty();
				retSplt2.Empty();
				SplitHtmlText(stToSplit, nCut, &retSplt1, &retSplt2);
			}
			else
			{
				CString strToAdd = retSplt1;
				strToAdd.Replace(HtmlTagOpen, _T(""));
				strToAdd.Replace(HtmlTagClose, _T(""));
				pStrArrayHTML->Add(retSplt1);
				if (retSplt2.IsEmpty()) break;
				stToSplit = retSplt2;
				retSplt1 = stToSplit;
				retSplt2.Empty();
				nCut = 0;
			}

		} while (TRUE);
		ASSERT(retSplt2.IsEmpty());
	}

	if (pStrArrayHTML->GetCount() <= 1)
		return FALSE;

	return TRUE;
}

// return string length
//-----------------------------------------------------------------------------
void TBHtmlParser::HTMLDcRender(CDC* pDc, CRect rect, CString stHTML, BOOL bPreview)
{
	m_bPreview = bPreview;

	ASSERT(pDc);
	if (pDc == NULL || stHTML.Trim().IsEmpty()) return;
	
	// Correction & ajust Html in input 
	CString stHTMLElab = HtmlCorrection(stHTML);

	HtmlTree hTree = GetHtmlhTree(stHTMLElab);
	if (hTree.Count <= 0)
		return;

	RenderText(hTree, pDc, rect, HtmlFontInit(pDc));
}

