
#include "stdafx.h"
#include <math.h>
#include <stdlib.h>

// Extern library declarations
#include <bclw/bclw.h>

#include <TbNameSolver\Chars.h>

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\globals.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>

#include <TbGenlib\Baseapp.h>
#include <TbParser\SymTable.h>
#include <TbGes\TBHtmlParser.h>

#include <TbWoormEngine\edtmng.h>

#include "listdlg.h"
#include "woormini.h"
#include "woormdoc.h"

#include "textobj.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//------------------------------------------------------------------------------

//==============================================================================
//			Class BasicTextStyle implementation
//==============================================================================


//------------------------------------------------------------------------------
BasicTextStyle::BasicTextStyle()
	:
	m_nAlign		(DEFAULT_ALIGN),
	m_rgbTextColor	(DEFAULT_TEXTCOLOR),
	m_rgbBkgColor	(DEFAULT_BKGCOLOR),
	m_nFontIdx		(AfxGetFontStyleTable()->GetFontIdx(FNT_DEFAULT))
{
}

//------------------------------------------------------------------------------
BasicTextStyle::BasicTextStyle(const BasicTextStyle& source)
	:
	m_rgbTextColor	(source.m_rgbTextColor),
	m_rgbBkgColor	(source.m_rgbBkgColor),
	m_nAlign		(source.m_nAlign),
	m_nFontIdx		(source.m_nFontIdx)
{
}

//------------------------------------------------------------------------------
BasicTextStyle& BasicTextStyle::operator = (const BasicTextStyle& source)
{
	m_rgbTextColor	= source.m_rgbTextColor;
	m_rgbBkgColor	= source.m_rgbBkgColor;
	m_nAlign		= source.m_nAlign;
	m_nFontIdx		= source.m_nFontIdx;
	return *this;
}

void BasicTextStyle::Assign(const BasicTextStyle& source)
{
	*this = source;
}

//==============================================================================
//			Class SubTotalStyle implementation
//==============================================================================

SubTotalStyle::SubTotalStyle() 
	: 
	BasicTextStyle ()
{
	m_nAlign		= DEFAULT_SUBTOTAL_STRING_ALIGN;
	m_rgbTextColor	= DEFAULT_SUBTOTAL_FOREGROUND;
	m_rgbBkgColor	= DEFAULT_SUBTOTAL_BACKGROUND;
	m_nFontIdx		= AfxGetFontStyleTable()->GetFontIdx(FNT_SUBTOTAL_STRING);
}

//------------------------------------------------------------------------------
SubTotalStyle::SubTotalStyle(const SubTotalStyle& source) 
	: 
	BasicTextStyle(source)
{
	*this = source;
}

//------------------------------------------------------------------------------
SubTotalStyle::~SubTotalStyle()
{
	SAFE_DELETE(m_pTextColorExpr);
	SAFE_DELETE(m_pBkgColorExpr);
}

//------------------------------------------------------------------------------
SubTotalStyle& SubTotalStyle::operator = (const SubTotalStyle& source)
{
	BasicTextStyle::operator=(source);

	if (source.m_pTextColorExpr)
		m_pTextColorExpr = new Expression(*source.m_pTextColorExpr);
	if (source.m_pBkgColorExpr)
		m_pBkgColorExpr = new Expression(*source.m_pBkgColorExpr);

	return *this;
}

//==============================================================================
//			Class GenericText implementation
//==============================================================================

//------------------------------------------------------------------------------
GenericText::GenericText () : BasicTextStyle ()
{
}

//------------------------------------------------------------------------------
GenericText::GenericText (const GenericText& source)
	:
	BasicTextStyle	(source),
	m_strText		(source.m_strText),
	m_bLocalizable  (source.m_bLocalizable)
{
}

//------------------------------------------------------------------------------
void GenericText::Draw (CDC& DC, CRect& rect, CWoormDocMng* pWDoc, BOOL bOpaque/*=TRUE*/, int flags /*= EnumGenericTextFlags::NORMAL*/, CSize Ecc /*= CSize(0,0)*/, BOOL bMiniHtml /*= FALSE*/, BOOL bPreview /*= FALSE*/)
{
	if (bMiniHtml)
		DrawMiniHtml(pWDoc, DC, rect, ! bOpaque, bPreview);
	else
		DrawGText (DC, rect, pWDoc, bOpaque, flags, Ecc);
}

//------------------------------------------------------------------------------
GenericText& GenericText::operator = (const GenericText& source)
{
	BasicTextStyle::operator=(source);
	m_strText = source.m_strText;
	m_bLocalizable = source.m_bLocalizable;

	return *this;
}

//------------------------------------------------------------------------------
void GenericText::SetText (const CString& strText)
{
	m_strText = strText;
}
//------------------------------------------------------------------------------
CString	GenericText::GetLocalizedText	(CBaseDocument* pDocument) const	
{
	CString strTmp = GetText(); 

	if (!m_bLocalizable)
		return strTmp;

	strTmp.Remove(CR_CHAR);
	strTmp.Replace(_T("\\n"), _T("\n"));
	return AfxLoadReportString (strTmp, pDocument); 
}

// pDC : pointer to your device-context
// str : the text
// rect: the rectangle
// nOptions: can be a combination of ETO_CLIPPED and ETO_OPAQUE
// (see documentation of ExtTextOut for more details)
void DrawRotatedText(CDC* pDC, const CString str, CRect rect, 
                     double degreeAngle, UINT nOptions = 0)
{
   // convert angle to radian
   double pi = 3.141592654;
   double radian = pi * 2 / 360 * degreeAngle;
 
   // get the center of a not-rotated text
   CSize TextSize = pDC->GetTextExtent(str);
   CPoint center;
   center.x = TextSize.cx / 2;
   center.y = TextSize.cy / 2;
 
   // now calculate the center of the rotated text
   CPoint rcenter;
   rcenter.x = long(cos(radian) * center.x - sin(radian) * center.y);
   rcenter.y = long(sin(radian) * center.x + cos(radian) * center.y);
 
///*TODO*/pDC->Rectangle(rect);
   
   // finally draw the text and move it to the center of the rectangle
   UINT prevAl = pDC->SetTextAlign(TA_BOTTOM);
   pDC->SetBkMode(TRANSPARENT);
   pDC->ExtTextOut(rect.left + rect.Width() / 2 - rcenter.x, 
                   rect.top + rect.Height() / 2 + rcenter.y,
                   nOptions, rect, str, NULL);

   pDC->SetTextAlign(prevAl);
}
 
//------------------------------------------------------------------------------
void GenericText::InternalDrawGText
	(
		CDC&		DC,
		CFont*		pFont,
		const CString& str,
		CRect&		rect,
		BOOL		bForceHorizontalAlignOnRotation,
		BOOL		bCalcRect
	)
{
	if (m_PreDraw.m_nAlign & (DT_EX_ORIENTATION_90|DT_EX_ORIENTATION_270))
	{
		CSize csz = ::GetTextSize(&DC, str, pFont);
	
		if (bForceHorizontalAlignOnRotation)
		{
			if (m_PreDraw.m_nAlign & DT_RIGHT)
			{
				rect.left = rect.right - csz.cy;
			}
			else if (m_PreDraw.m_nAlign & DT_CENTER)
			{
				//nothing
			}
			else //if (al & DT_LEFT)
			{
				rect.right = rect.left + csz.cy;
			}
		}
		//----
		if (m_PreDraw.m_nAlign & DT_BOTTOM)
		{
			rect.top = rect.bottom - csz.cx - 2;

		}
		else if (m_PreDraw.m_nAlign & DT_VCENTER)
		{
			//nothing
		}
		else //if (al & DT_TOP)
		{
			rect.bottom = rect.top + csz.cx + 2;
		}
	}
	else
	{
		 if (m_PreDraw.m_nAlign & DT_WORDBREAK)
	{
		CRect r(rect);
			int h = DC.DrawText (str, str.GetLength(), r, (m_PreDraw.m_nAlign & 0xFFFF) | DT_CALCRECT);
	/*
		int x = rect.Width() - r.Width();
			if (x > 3)
		{
				if (m_PreDraw.m_nAlign & DT_CENTER)
			{
					rect.left += (x / 2 - 1);
				rect.right -= (x / 2 - 1);
			}
				else if (m_PreDraw.m_nAlign & DT_RIGHT)
					rect.left += (x - 2);
				else 
					rect.right -= (x - 2);
		}
*/
		int y = rect.Height() - r.Height();
			if (y > 3)
			{
				if (m_PreDraw.m_nAlign & DT_VCENTER)
				{
					rect.top += (y / 2 - 1);
					rect.bottom -= (y / 2 - 1);

					//if (bCalcRect)	//tappullo x PDF
					//{
					//	m_PreDraw.m_nAlign = m_PreDraw.m_nAlign & ~(DT_VCENTER);
					//}
				}
				else if (m_PreDraw.m_nAlign & DT_BOTTOM)
				{
					rect.top += (y - 2);
					
					//if (bCalcRect)	//tappullo x PDF
					//{
					//	m_PreDraw.m_nAlign = m_PreDraw.m_nAlign & ~(DT_BOTTOM);
					//}
				}
				else
		{
					rect.bottom -= (y - 2);
				}
			}

		}
	}

	if (bCalcRect)
		return;
	//--------------------

	if (m_PreDraw.m_nAlign & (DT_EX_ORIENTATION_90 | DT_EX_ORIENTATION_270))
		DrawRotatedText(&DC, str, rect, m_PreDraw.m_nAlign & DT_EX_ORIENTATION_90 ? 90 : 270);
	else
		DC.DrawText (str, str.GetLength(), rect, (m_PreDraw.m_nAlign & 0xFFFF));
}

//------------------------------------------------------------------------------
void GenericText::DrawGText
	(
		CDC&		DC,
		const CRect&	r,
		CWoormDocMng*	pDoc, 
		BOOL		opaque /*=TRUE*/,
		int			flags /*= EnumGenericTextFlags::NORMAL*/,
		CSize			Ecc /*= CSize(0,0)*/,
		BOOL			bCalcRect /*= FALSE*/
	)
{
	m_PreDraw.m_nAlign = m_nAlign;
	m_PreDraw.m_arTextLine.RemoveAll();
	m_PreDraw.m_arRectLine.RemoveAll();
	m_PreDraw.m_rect = r;
	m_PreDraw.m_strText.Empty();

	ScaleRect(m_PreDraw.m_rect, DC);
	
	if (!bCalcRect)
	{
	// fill with brush if opaque
	if (flags & WRONG)
	{
		COLORREF crError = (m_rgbBkgColor != RGB(255,0,0)) ? RGB(255,0,0) : RGB(255,0,255);
		CBrush brush;
		brush.CreateSolidBrush(crError);
		CBrush*	old_brush = DC.SelectObject(&brush);
		
			DC.FillRect(m_PreDraw.m_rect, &brush);
		DC.SelectObject(old_brush);
	}
	else if (opaque)
    {
		CBrush brush;

		if ((flags & GenericText::SELECTED) && !DC.IsPrinting())
		{
			brush.CreateSolidBrush((flags & GenericText::LAST_SELECTED) ? RGB(255,128,0) : RGB(255,255,0));
		}
		else
			brush.CreateSolidBrush(m_rgbBkgColor);

		CBrush*	old_brush = DC.SelectObject(&brush);
		
			DC.FillRect(m_PreDraw.m_rect, &brush);
		DC.SelectObject(old_brush);
    }
    }
    
	LOGFONT		lf;  ::memset(&lf, 0, sizeof lf);
	const FontStyle* pFontStyle = pDoc->m_pFontStyles->GetFontStyle(m_nFontIdx, &(pDoc->GetNamespace()));
	if (pFontStyle)
		lf = pFontStyle->GetLogFont();

	// choose color based on font style color or object color
	COLORREF dwColor = ((m_rgbTextColor != DEFAULT_TEXTCOLOR) || !pFontStyle)
		?
		m_rgbTextColor
		:
		pFontStyle->GetColor();

	// set color, mode and fonts
	COLORREF	old_color		= DC.SetTextColor(dwColor);
	COLORREF	old_bkgcolor	= DC.GetBkColor();
	int			old_mode		= DC.SetBkMode(TRANSPARENT);

	if (!bCalcRect && flags && !DC.IsPrinting() && pFontStyle)
	{
		BOOL bSpecific = TRUE;

		int idxFontH = pDoc->m_pFontStyles->GetFontIdx(
			pFontStyle->GetStyleName() + (flags & BROWSED ?  FNT_CUSTOM_SEGMENT_HYPERLINK_BROWSED : FNT_CUSTOM_SEGMENT_HYPERLINK),
			FALSE);
		if (idxFontH == FNT_ERROR && (flags & BROWSED))
			idxFontH = pDoc->m_pFontStyles->GetFontIdx(pFontStyle->GetStyleName() + FNT_CUSTOM_SEGMENT_HYPERLINK, FALSE);
		if (idxFontH == FNT_ERROR)
		{
			bSpecific = FALSE;
			idxFontH = pDoc->m_pFontStyles->GetFontIdx(flags & BROWSED ?  FNT_HYPERLINK_BROWSED : FNT_HYPERLINK, FALSE);
		}
		const FontStyle* pHyperLinkFont = idxFontH == FNT_ERROR ? NULL : pDoc->m_pFontStyles->GetFontStyle(idxFontH, &(pDoc->GetNamespace()));

		if (flags & WRONG_LINK)
		{
			lf.lfStrikeOut = TRUE;

			if (dwColor != RGB(255,0,0))
				DC.SetTextColor(RGB(255,0,0));
			else
				DC.SetTextColor(RGB(100,0,0));	
		}
		else if (flags & (BROWSED|LINKED))
		{
			if (bSpecific)
			{
				lf = pHyperLinkFont->GetLogFont();
			}
			else
			{
				if (pHyperLinkFont)
				{
					lf.lfUnderline = pHyperLinkFont->IsUnderline();

					DC.SetTextColor(pHyperLinkFont->GetColor());
				}
				else
				{
					lf.lfUnderline = (flags & (BROWSED|LINKED)) != 0;

					DC.SetTextColor(flags & BROWSED ? RGB(128,0,128) : RGB(0,0,255));
				}
			}
		}
	}
	// se si va in draft mode usa un font per tutti i testi eventualmente 
	// personalizzabile in tb.ini
	if (!bCalcRect && pDoc->m_pOptions->UseDraftFont(pDoc, DC.IsPrinting()))
	{
		lf.lfCharSet		= pDoc->m_pWoormIni->m_lfCharSet;
		lf.lfOutPrecision	= pDoc->m_pWoormIni->m_lfOutPrecision;
		lf.lfClipPrecision	= pDoc->m_pWoormIni->m_lfClipPrecision;
		lf.lfQuality		= pDoc->m_pWoormIni->m_lfQuality;
		lf.lfPitchAndFamily	= pDoc->m_pWoormIni->m_lfPitchAndFamily;

		int nLen = pDoc->m_pWoormIni->m_strFaceName.GetLength();
		TB_TCSCPY(lf.lfFaceName, pDoc->m_pWoormIni->m_strFaceName.GetBuffer(nLen));
		pDoc->m_pWoormIni->m_strFaceName.ReleaseBuffer();
	}
	
	if (m_nAlign & DT_EX_ORIENTATION_90)
	{
		lf.lfEscapement = lf.lfOrientation = 900;
	}
	else if (m_nAlign & DT_EX_ORIENTATION_270)
	{
		lf.lfEscapement = lf.lfOrientation = 2700;
	}

	if (pFontStyle)
		ScaleLogFont(&lf, DC);

	// setta se necessario il font opportuno
	CFont* pF = pFontStyle ? pDoc->SetCurrentFont(&lf, DC) : pDoc->m_pCurrentFont;

	// for separate strText from left border, only if text is not rotated
	BOOL isTextRotated = (m_nAlign & (DT_EX_ORIENTATION_90|DT_EX_ORIENTATION_270)) != 0;
	if (!isTextRotated)
	{
		m_PreDraw.m_rect.left	+= MARGIN_FROM_BORDER;
		m_PreDraw.m_rect.right	-= MARGIN_FROM_BORDER;
	}

	//----TODO VERIFICARE  traduce anche i valori dei field/celle ????
	m_PreDraw.m_strText = GetLocalizedText(pDoc);
	
	// nel caso il font sia arrivato invalido informo l'utente
	if (!pFontStyle || !pF)
		m_PreDraw.m_strText += '-' + _TB("Font style is not available. In substitution has been selected default style. Verify criterions use!");

	// elimina cr-lf se ci sono. Serve a non far vedere due quadratini se
	// non faccio il multiline 
	m_PreDraw.m_strText.Remove(CR_CHAR);
	m_PreDraw.m_strText.Replace(_T("\\n"), _T("\n"));
	
	if (m_PreDraw.m_nAlign & DT_SINGLELINE)
	{	
		m_PreDraw.m_strText.Replace(LF_CHAR, BLANK_CHAR);

		m_PreDraw.m_nAlign &= ~(DT_WORDBREAK);
	}
	
	if ((m_PreDraw.m_nAlign & DT_SINGLELINE) == 0 && m_PreDraw.m_strText.Find(LF_CHAR) < 0)
	{
		CSize csTitle = ::GetTextSize(&DC, m_PreDraw.m_strText, pF);

		int rectContainerTextWidthCapacity =  isTextRotated ? m_PreDraw.m_rect.Height() : m_PreDraw.m_rect.Width();
			if (csTitle.cx < rectContainerTextWidthCapacity)
			{
			m_PreDraw.m_nAlign |= DT_SINGLELINE;
			m_PreDraw.m_nAlign &= ~(DT_WORDBREAK);
			}			
		else 
			m_PreDraw.m_nAlign |= DT_WORDBREAK;
	}

	if (AfxIsChineseCulture())
	{
		if ((m_PreDraw.m_nAlign & DT_SINGLELINE) == 0 || (m_PreDraw.m_nAlign & (DT_BOTTOM|DT_VCENTER)) == 0)
		{	
			m_PreDraw.m_rect.top += 1;
		}
		if (m_PreDraw.m_nAlign & DT_BOTTOM)
		{	
			m_PreDraw.m_rect.bottom -= 1;
		}
	}
	
	//se allineo a destra, il padding a sinistra non mi serve
	//inoltre crea malfunzionamenti nella generazione del PDF
	if (m_PreDraw.m_nAlign & DT_RIGHT)
		m_PreDraw.m_strText.TrimLeft();

	if (m_PreDraw.m_nAlign & DT_EX_FIELD_SET)
	{
		CSize csz = ::GetTextSize(&DC, m_PreDraw.m_strText, pF);

		int hh = csz.cy / 2;
		int y = m_PreDraw.m_rect.top;
		m_PreDraw.m_rect.top = y - hh - 1 - Ecc.cy;
		m_PreDraw.m_rect.bottom = y + hh + 1 - Ecc.cy;

		if (m_PreDraw.m_nAlign & DT_RIGHT)
		{
			m_PreDraw.m_rect.right -= (10 + Ecc.cx);
			m_PreDraw.m_rect.left = m_PreDraw.m_rect.right - csz.cx - 1;;
		}
		else if (m_PreDraw.m_nAlign & DT_CENTER)
		{
			int halfwidth = (m_PreDraw.m_rect.right - m_PreDraw.m_rect.left) / 2 + 1;
			m_PreDraw.m_rect.left +=  halfwidth - csz.cx / 2 - 1;
			m_PreDraw.m_rect.right = m_PreDraw.m_rect.left + csz.cx + 1;;
		}
		else //if ((m_nAlign & DT_LEFT) == DT_LEFT)
		{
			m_PreDraw.m_rect.left += (10 + Ecc.cx);
			m_PreDraw.m_rect.right = m_PreDraw.m_rect.left + csz.cx + 1;
		}
		}

	if (!bCalcRect && m_PreDraw.m_nAlign & DT_EX_FIELD_SET)
	{
		CBrush brush;
		brush.CreateSolidBrush(m_rgbBkgColor);
		CBrush*	old_brush = DC.SelectObject(&brush);
		
		DC.FillRect(m_PreDraw.m_rect, &brush);
		DC.SelectObject(old_brush);
	}

	if (m_PreDraw.m_nAlign & DT_SINGLELINE)
	{	
		InternalDrawGText(DC, pF, m_PreDraw.m_strText, m_PreDraw.m_rect, TRUE, bCalcRect);
	}
	else if (m_PreDraw.m_nAlign & DT_WORDBREAK)
	{	
		if (isTextRotated)
		{
			//TODO Non va a capo:
			//splittare la stringa tipo WoormField::SplitString
			//saltare nel ramo else sottostante
		}
		else
			m_PreDraw.m_strText.Replace(_T("\n"), _T("\r\n"));

		InternalDrawGText(DC, pF, m_PreDraw.m_strText, m_PreDraw.m_rect, FALSE, bCalcRect);
	}
	else
	{
		CStringArray_Split(m_PreDraw.m_arTextLine, m_PreDraw.m_strText, _T("\n"), TRUE);

		int nl = m_PreDraw.m_arTextLine.GetCount();
		if (nl <= 1)
		{
			InternalDrawGText(DC, pF, m_PreDraw.m_strText, m_PreDraw.m_rect, TRUE, bCalcRect);
		}
		else
		{
			m_PreDraw.m_arRectLine.SetSize(nl);

			CSize csz = ::GetTextSize(&DC, m_PreDraw.m_arTextLine[0], pF);
			int h = csz.cy;
			m_PreDraw.m_nAlign |= DT_SINGLELINE;

			if (m_PreDraw.m_nAlign & (DT_EX_ORIENTATION_90|DT_EX_ORIENTATION_270))
			{
				int y = m_PreDraw.m_rect.Width() / nl;

				int left = m_PreDraw.m_rect.left;

				if (h < y)
				{
					if (m_PreDraw.m_nAlign & DT_CENTER)
						left += (y - h) * nl / 2;
					else if (m_PreDraw.m_nAlign & DT_RIGHT)
						left += (y - h) * nl;
				}
				m_PreDraw.m_nAlign = m_PreDraw.m_nAlign & ~(DT_RIGHT|DT_CENTER);

				int right = left + h;

				if (m_PreDraw.m_nAlign & DT_EX_ORIENTATION_270)
				{
					for (int i = nl - 1; i >= 0 ; i--)
					{
						CString sLine = m_PreDraw.m_arTextLine[i];

						CSize csz = ::GetTextSize(&DC, sLine, pF);
						int offset = m_PreDraw.m_rect.Height() - csz.cx;

						CRect r (m_PreDraw.m_rect);
						r.left = left;
						r.right = right;

						if (offset > 3)
						{
							if (m_PreDraw.m_nAlign & DT_VCENTER)
							{
								r.top += (offset / 2 - 1);
								r.bottom -= (offset / 2 -1);
							}
							else if (m_PreDraw.m_nAlign & DT_BOTTOM)
							{
								r.top += (offset - 2);
							}
							else
							{
								r.bottom -= (offset - 2);
							}
						}

						m_PreDraw.m_arRectLine[i] = r;

						InternalDrawGText(DC, pF, sLine, m_PreDraw.m_arRectLine[i], FALSE, bCalcRect);

						left += h;
						right += h;
					}
				}
				else
				{
					for (int i = 0; i < nl; i++)
					{
						CString sLine = m_PreDraw.m_arTextLine[i];

						CSize csz = ::GetTextSize(&DC, sLine, pF);
						int offset = m_PreDraw.m_rect.Height() - csz.cx;

						CRect r(m_PreDraw.m_rect);
						r.left = left;
						r.right = right;

						if (offset > 3)
						{
							if (m_PreDraw.m_nAlign & DT_VCENTER)
							{
								r.top += (offset / 2 - 1);
								r.bottom -= (offset / 2 - 1);
							}
							else if (m_PreDraw.m_nAlign & DT_BOTTOM)
							{
								r.top += (offset - 2);
							}
							else
							{
								r.bottom -= (offset - 2);
							}
						}

						m_PreDraw.m_arRectLine[i] = r;

						InternalDrawGText(DC, pF, sLine, m_PreDraw.m_arRectLine[i], FALSE, bCalcRect);

						left += h;
						right += h;
					}
				}
			}
			else
			{
				int y = m_PreDraw.m_rect.Height() / nl;

				int top = m_PreDraw.m_rect.top;

				if (h < y)
				{
					if (m_PreDraw.m_nAlign & DT_VCENTER)
						top += (y - h) * nl / 2;
					else if (m_PreDraw.m_nAlign & DT_BOTTOM)
						top += (y - h) * nl;
				}

				int bottom = top + h;

				for (int i = 0; i < nl; i++)
				{
					CString sLine = m_PreDraw.m_arTextLine[i];

					CSize csz = ::GetTextSize(&DC, sLine, pF);
					int offset = m_PreDraw.m_rect.Width() - csz.cx;

					CRect r(m_PreDraw.m_rect);
					r.top = top;
					r.bottom = bottom;
/*
					if (offset > 3)
					{
						if (m_PreDraw.m_nAlign & DT_CENTER)
						{
							r.left	+= (offset / 2 - 1);
							r.right -= (offset / 2 - 1);
						}
						else if (m_PreDraw.m_nAlign & DT_RIGHT)
						{
							r.left += (offset - 2);
						}
						else
						{
							r.right -= (offset - 2);
						}
					}
*/
					m_PreDraw.m_arRectLine[i] = r;

					if (!bCalcRect)
						DC.DrawText (sLine, sLine.GetLength(), r, (m_PreDraw.m_nAlign & 0xFFFF));

					top += h;
					bottom += h;
				}
				m_PreDraw.m_nAlign &= ~(DT_BOTTOM|DT_WORDBREAK);
				m_PreDraw.m_nAlign |= (DT_SINGLELINE|DT_VCENTER);
				//if (bCalcRect)
				//	m_PreDraw.m_nAlign &= ~(DT_VCENTER);
			}
		}
	}
	
	// clear device context
	DC.SetTextColor(old_color);
	DC.SetBkMode(old_mode);
}

//-----------------------------------------------------------------------------
void GenericText::DrawMiniHtml(CWoormDocMng* pWDoc, CDC& DC, CRect& rect, BOOL bTrasparent, BOOL bPreview /* = FALSE */)
{
	CFont font;
	CFont* old_pFont = NULL;

	CRgn old_rgn;
	old_rgn.CreateRectRgn(0, 0, 0, 0);
	HDC hdc = DC.GetSafeHdc();
	::GetClipRgn(hdc, (HRGN)old_rgn.GetSafeHandle());

	CRgn rgn;
	rgn.CreateRectRgnIndirect(rect);
	DC.SelectClipRgn(&rgn);

	const FontStyle* pFontStyle = pWDoc->m_pFontStyles->GetFontStyle(this->m_nFontIdx, &(pWDoc->GetNamespace()));
	if (pFontStyle)
	{
		if (pFontStyle->CreateFont(font))
			old_pFont = DC.SelectObject(&font);
	}

	// choose color based on font style color or object color
	COLORREF dwTextColor = ((m_rgbTextColor != DEFAULT_TEXTCOLOR) || !pFontStyle)
		?
		m_rgbTextColor
		:
		pFontStyle->GetColor();

	COLORREF	old_TextColor	= DC.SetTextColor(dwTextColor);
	COLORREF	old_BkgColor	= DC.SetBkColor(this->m_rgbBkgColor);
	int			old_trasparent	= DC.SetBkMode(bTrasparent ? TRANSPARENT : OPAQUE);

	
	ScaleRect(rect, DC);

	if (!bTrasparent)
	{
		DC.FillSolidRect(rect, this->m_rgbBkgColor);
	}

	TBHtmlParser htmlParser;
	htmlParser.HTMLDcRender(&DC, rect, m_strText, bPreview);

	DC.SetTextColor	(old_TextColor);
	DC.SetBkColor	(old_BkgColor);
	DC.SetBkMode	(old_trasparent);
	DC.SelectObject	(old_pFont);
	DC.SelectClipRgn(&old_rgn);
	DeleteObject(rgn);
	DeleteObject(old_rgn);
}

//==============================================================================
//			Class TableTitleText implementation
//==============================================================================
//------------------------------------------------------------------------------
TableTitleText::TableTitleText()
{
	m_nAlign = DEFAULT_TABLE_TITLE_ALIGN;
}

//------------------------------------------------------------------------------
TableTitleText::TableTitleText(const TableTitleText& source)
	:
	GenericText	(source),
	m_strTitle	(source.m_strTitle)
{
}

//------------------------------------------------------------------------------
void TableTitleText::FormatWithValue ()
{
	m_strText = m_strTitle;
}

//------------------------------------------------------------------------------
void TableTitleText::SetText (const CString& strText)
{
	m_strTitle = strText;
	
	FormatWithValue ();
}

//------------------------------------------------------------------------------
CString TableTitleText::GetText() const
{
	return m_strTitle;
}

//------------------------------------------------------------------------------
TableTitleText& TableTitleText::operator = (const TableTitleText& source)
{
    GenericText::operator=(source);
	m_strTitle = source.m_strTitle;
	return *this;
}

//------------------------------------------------------------------------------
void TableTitleText::FormatWithId (WORD wID, int nPosition)
{
	m_strText = cwsprintf(_TB("Id:{0-%d}, Pos:{1-%d}"), wID, nPosition);
}

//------------------------------------------------------------------------------
void TableTitleText::FormatWithName (WORD nID, CWoormDocMng* pDocument)
{
	m_strText = pDocument->m_pEditorManager->GetTablePublicName(nID);
}

//------------------------------------------------------------------------------
void TableTitleText::Format (WORD wID, CWoormDocMng* m_pDocument)
{
	switch (m_pDocument->m_pWoormIni->m_Show)
	{
	case WoormIni::ID:
		FormatWithId (wID, m_pDocument->GetPosition(wID));
		break;

	case WoormIni::NAME:
		FormatWithName (wID, m_pDocument);
		break;

	case WoormIni::VALUE:
		FormatWithValue ();
		break;
	}
}


//------------------------------------------------------------------------------
int TableTitleText::SetTextDlg ()
{
  CTextDlg dialog (m_strTitle);
  return dialog.DoModal();
}


//==============================================================================
//			Class Value implementation
//==============================================================================
//
// Format of RDEData
// WORD,	WORD,	nCHAR
// STATUS,	LEN(n),	DATA
//
//------------------------------------------------------------------------------


//------------------------------------------------------------------------------
Value::Value ()
	:
    GenericText	(),
	m_RDEdata	()
{
	SetAlign	(DEFAULT_VALUE_ALIGN);
	SetFontIdx	(AfxGetFontStyleTable()->GetFontIdx(FNT_TEXT));
}

//------------------------------------------------------------------------------
Value::Value (const Value& source)
	:
	GenericText	(source),
	m_RDEdata		(source.m_RDEdata)
{
}

//------------------------------------------------------------------------------
void Value::DisableData()
{
	// disable rde data comunication buffer and clear visual buffer
	m_RDEdata.DisableData();
	m_strText = "";
}

//------------------------------------------------------------------------------
Value& Value::operator = (const Value& source)
{
	GenericText::operator=(source);
	m_RDEdata = source.m_RDEdata;

    return *this;
}

//------------------------------------------------------------------------------
void Value::FormatWithId (WORD wID, int nPosition)
{
	m_strText = cwsprintf(_TB("Id:{0-%d}, Pos:{1-%d}"), wID, nPosition);
}

//------------------------------------------------------------------------------
void Value::FormatWithName (WORD nID, CWoormDocMng* pDocument)
{
	m_strText = pDocument->m_pEditorManager->GetPublicName(nID);
}

//------------------------------------------------------------------------------
void Value::FormatWithValue(Formatter* formatter, CWoormDocMng* pDocument)
{
	// Format only if enabled
	if (!m_RDEdata.IsEnabled())
	{
		// already cleaned strText buffer from DisableData call
		return;
	}

    // read new incoming data
	if (m_RDEdata.GetLen() == 0)
	{
		m_strText.Empty();		//@@TODO	far fare al formattatore LoadString(_TB("Not found"));
		return;
	}

	if (formatter)
	{
		if (pDocument->m_bCultureVariable)
		{
			CString sCulture = AfxGetThreadContext()->SetUICulture(pDocument->GetUICulture());
	
			formatter->Format((const void*) m_RDEdata.GetData(), m_strText);
	
			AfxGetThreadContext()->SetUICulture(sCulture);
		}
		else
			formatter->Format((const void*) m_RDEdata.GetData(), m_strText);
	}
	else
		m_strText = _TB("Formatter not available! Verify criterios use!");
}

//------------------------------------------------------------------------------
void Value::Format (WORD wID, Formatter* formatter, CWoormDocMng* pDocument)
{
	switch (pDocument->m_pWoormIni->m_Show)
	{
	case WoormIni::ID:
		FormatWithId (wID,pDocument->GetPosition(wID));
		break;

	case WoormIni::NAME:
		FormatWithName (wID, pDocument);
		break;

	case WoormIni::VALUE:
		FormatWithValue (formatter, pDocument);
		break;
	}
}

//==============================================================================
//			Class StaticText implementation
//==============================================================================
//------------------------------------------------------------------------------
StaticText::StaticText()
	:
	GenericText	()
{
	SetText 	(_T(""));
	SetAlign	(DEFAULT_TEXT_ALIGN);
	SetFontIdx	(AfxGetFontStyleTable()->GetFontIdx(FNT_DEFAULT));
}

//------------------------------------------------------------------------------
StaticText::StaticText(const StaticText& source)
	:
	GenericText	(source)
{
}

//------------------------------------------------------------------------------
StaticText& StaticText::operator = (const StaticText& source)
{
	GenericText::operator=(source);
	return *this;
}

//------------------------------------------------------------------------------
int StaticText::Query()
{
	CTextDlg dialog (m_strText);
	return dialog.DoModal();
}


//==============================================================================
//			Class ColumnTitleText implementation
//==============================================================================
//------------------------------------------------------------------------------
ColumnTitleText::ColumnTitleText()
	:
	StaticText	()
{
	SetText (_T(""));
	m_nAlign = DEFAULT_COLUMN_TITLE_ALIGN;
}

//------------------------------------------------------------------------------
ColumnTitleText::ColumnTitleText(const ColumnTitleText& source)
	:
	StaticText	(source)
{
}

//------------------------------------------------------------------------------
ColumnTitleText& ColumnTitleText::operator = (const ColumnTitleText& source)
{
	StaticText::operator=(source);
	return *this;
}



//==============================================================================
//			Class Label implementation
//==============================================================================
//------------------------------------------------------------------------------
Label::Label()
{
	SetText		(_T(""));
    SetAlign	(DEFAULT_LABEL_ALIGN);
	SetFontIdx	(AfxGetFontStyleTable()->GetFontIdx(FNT_LABEL));
}


//------------------------------------------------------------------------------
int Label::Query ()
{
	CTextDlg dialog (m_strText);
	return dialog.DoModal();
}


// label drawing MUST be transparent
//------------------------------------------------------------------------------
void Label::Draw (CDC& DC, CRect& rect, CWoormDocMng* pDoc, BOOL bOpaque/* = FALSE*/, int flags /*= EnumGenericTextFlags::NORMAL*/, CSize Ecc /*= CSize(0,0)*/)
{
	GenericText::Draw (DC, rect, pDoc, bOpaque, flags, Ecc);
}

//=============================================================================
BOOL CBarCode_Draw
	(
		CBarCode*		pBarCode,
		CDC& 			DC,
		CRect 			inside,
		CWoormDocMng*		pDoc,
		const GenericText&	value,
		CString&		sErr,
		int				nCurrRow/*= 0*/, 
		BOOL			bPreview/* = FALSE*/
	)
{   
	const FontStyle* pFont = pDoc->m_pFontStyles->GetFontStyle(value.GetFontIdx(), &(pDoc->GetNamespace()));
	ASSERT_VALID(pFont);

	CString sHumanText;
	int bcType = pBarCode->m_nBarCodeType == 0 ? pBarCode->m_nBCDefaultType : pBarCode->m_nBarCodeType;


	if (pBarCode->m_nHumanTextAlias && nCurrRow >= 0)
	{
		RDEData* pData = pDoc->GetRDEDataFromAlias(pBarCode->m_nHumanTextAlias, nCurrRow);
		if (pData && pData->GetData() && pData->IsValid())
		{
			DataStr aData;
			aData.Assign(*pData);
			if (!aData.IsEmpty())
				sHumanText = aData;
		}
	}
	//TODO NarrowBar thickness is contained into the associated alias
	if (pBarCode->m_nNarrowBarAlias && nCurrRow >= 0)
	{
		RDEData* pData = pDoc->GetRDEDataFromAlias(pBarCode->m_nNarrowBarAlias, nCurrRow);
		if (pData && pData->GetData() && pData->IsValid())
		{
			DataInt aData;
			aData.Assign(*pData);
			if (!aData.IsEmpty())
				pBarCode->m_nNarrowBar = aData;
		}
	}

	if (pBarCode->m_nBarCodeTypeAlias && nCurrRow >= 0)
	{
		RDEData* pData = pDoc->GetRDEDataFromAlias(pBarCode->m_nBarCodeTypeAlias, nCurrRow);
		if (pData && pData->GetData() && pData->IsValid())
		{
			SymField* pF = pDoc->m_ViewSymbolTable.GetFieldByID(pBarCode->m_nBarCodeTypeAlias);
			if (pF == NULL)
			{
				sErr = _TB("Failed to resolve dynamic barcode type");
				return FALSE;
			}

			if (pF->GetDataType() == DataType::String)
			{
				DataStr aDataStr;
				aDataStr.Assign(*pData);
				if (!aDataStr.IsEmpty())
				{
					CString sTypeName = aDataStr;
					pBarCode->m_nBarCodeType = CBarCodeTypes::BarCodeType(sTypeName, FALSE);
				}
			}
			else if (pF->GetDataType().m_wTag == E_BARCODE_TYPE)
			{
				DataEnum aData;
				aData.Assign(*pData);
				if (!aData.IsEmpty())
				{
					pBarCode->m_nBarCodeType = CBarCodeTypes::BarCodeType(aData.GetValue (), FALSE);
				}
			}
			else if (pF->GetDataType() == DataType::Integer)
			{
				DataInt aData;
				aData.Assign(*pData);
				if (aData >=0)
				{
					pBarCode->m_nBarCodeType = (int)aData;
				}
			}
			else
			{
				sErr = _TB("Wrong data type value for dynamic barcode type");
				return FALSE;
			}
			if (pBarCode->m_nBarCodeType < 0)
			{ 
				sErr = _TB("Failed to resolve dynamic barcode type");
				return FALSE;
			}
		}
		else
		{
			sErr = _TB("Missing value for dynamic barcode type");
			return FALSE;
		}
	}

	if (!pBarCode->m_sCheckEncodeFieldName.IsEmpty() && nCurrRow >= 0)
	{
		SymField* pF = pDoc->m_ViewSymbolTable.GetField(pBarCode->m_sCheckEncodeFieldName);

		if (pF == NULL)
		{
			sErr = _TB("Failed to resolve dynamic encoding mode/check digit");
			return FALSE;
		}

		DataObj* pData = pF->GetData();
		if (pData)
		{
			if (pF->GetDataType() == DataType::Integer)
			{
				DataInt aData;
				aData.Assign(*pData);
				pBarCode->m_nCheckSumType = (int)aData;

			}
			else
			{
				sErr = _TB("Wrong data type value for dynamic encoding mode/check digit");
				return FALSE;
			}
		}
		else
		{
			sErr = _TB("Missing value for dynamic encoding mode/check digit");
			return FALSE;
		}
	}

	if (!pBarCode->m_s2DVersionFieldName.IsEmpty() && nCurrRow >= 0)
	{
		SymField* pF = pDoc->m_ViewSymbolTable.GetField(pBarCode->m_s2DVersionFieldName);

		if (pF == NULL)
		{
			sErr = _TB("Failed to resolve dynamic barcode version");
			return FALSE;
		}

		DataObj* pData = pF->GetData();
		if (pData)
		{
			if (pF->GetDataType() == DataType::Integer)
			{
				DataInt aData;
				aData.Assign(*pData);
				pBarCode->m_n2DVersion = (int)aData;

			}
			else if (pF->GetDataType() == DataType::String)
			{
				DataStr aDataStr;
				aDataStr.Assign(*pData);
				if (!aDataStr.IsEmpty())
				{
					CString sVersion = aDataStr;
					if (bcType == BC_DATAMATRIX)
						pBarCode->m_n2DVersion = CBarCodeTypes::BarCodeDMVersion(sVersion, FALSE);
					else if (bcType == BC_PDF417)
					{
						pBarCode->m_n2DVersion = 0;
						if(!CBarCodeTypes::ReadPDF417Version(sVersion, pBarCode->m_nRowsNo, pBarCode->m_nColumnsNo))
						{
							sErr = _TB("Failed to resolve dynamic barcode version");
							return FALSE;
						}
					}
					else
					{
						pBarCode->m_n2DVersion = _ttoi(sVersion);
						if (errno == ERANGE || errno == EINVAL)
							pBarCode->m_n2DVersion = -1;
					}								
				}
				if (pBarCode->m_n2DVersion < 0)
				{
					sErr = _TB("Failed to resolve dynamic barcode version");
					return FALSE;
				}
			}
			else
			{
				sErr = _TB("Wrong data type value for dynamic barcode version");
				return FALSE;
			}
			
		}
		else
		{
			sErr = _TB("Missing value for dynamic barcode version");
			return FALSE;
		}
	}

	if (!pBarCode->m_sErrCorrLevelFieldName.IsEmpty() && nCurrRow >= 0)
	{
		SymField* pF = pDoc->m_ViewSymbolTable.GetField(pBarCode->m_sErrCorrLevelFieldName);

		if (pF == NULL)
		{
			sErr = _TB("Failed to resolve dynamic error correction level");
			return FALSE;
		}

		DataObj* pData = pF->GetData();
		if (pData)
		{
			if (pF->GetDataType() == DataType::Integer)
			{
				DataInt aData;
				aData.Assign(*pData);
				pBarCode->m_nErrCorrLevel = (int)aData;

			}
			else if (pF->GetDataType() == DataType::String)
			{
				pBarCode->m_nErrCorrLevel = -1;
				DataStr aDataStr;
				aDataStr.Assign(*pData);
				if (!aDataStr.IsEmpty())
				{
					CString sErrCorrectionLevel = aDataStr;
					if (bcType == BC_MicroQR || bcType == BC_QR)
					{
						if (sErrCorrectionLevel == _T("L"))
							pBarCode->m_nErrCorrLevel = 0;
						else if (sErrCorrectionLevel == _T("M"))
							pBarCode->m_nErrCorrLevel = 1;
						else if (sErrCorrectionLevel == _T("Q"))
							pBarCode->m_nErrCorrLevel = 2;
					}
					else if (bcType == BC_QR)
					{
						if (sErrCorrectionLevel == _T("H"))
							pBarCode->m_nErrCorrLevel = 3;
					}
					else
					{
						pBarCode->m_nErrCorrLevel = _ttoi(sErrCorrectionLevel);
						if (errno == ERANGE || errno == EINVAL)
							pBarCode->m_nErrCorrLevel = -1;
					}
				}
				if (pBarCode->m_nErrCorrLevel < 0)
				{
					sErr = _TB("Failed to resolve dynamic error correction level");
					return FALSE;
				}
			}
			else
			{
				sErr = _TB("Wrong data type value for dynamic error correction level");
				return FALSE;
			}
		}
		else
		{
			sErr = _TB("Missing value for dynamic error correction level");
			return FALSE;
		}
	}

/*
	int align = DT_LEFT;	
	if ((value.m_nAlign & DT_RIGHT) == DT_RIGHT)
		align = 2;
	else if ((value.m_nAlign & DT_CENTER) == DT_CENTER)
		align = 1;
*/
	int align = DT_CENTER;
	int vAlign = DT_CENTER;

	ScaleRect(inside, DC);

	BOOL bRet =  pBarCode->DrawBarCode
		(
			DC,
			inside,
			pFont->GetLogFont(),
			value.GetText(),
			value.GetTextColor(),
			value.GetBkgColor(),
			sErr,
			pDoc,
			sHumanText,
			bPreview,
			align,
			vAlign
		);

	return bRet;
}
//=============================================================================
