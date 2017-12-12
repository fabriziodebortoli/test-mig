
#include "stdafx.h"


#include <Math.h>
#include "MultiChartCtl.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//=====================================================================================
void CountPoint ( CRect & rcInside, int nAngle, CPoint & pt, BOOL bPercent = FALSE)
{
	while (nAngle <0) nAngle += 360;
	while (nAngle>359) nAngle -= 360;

	const double pi = 3.1415926535;
	double dAngle = ((double)nAngle) * pi / 180.0;

	double r;
	r = ((double) abs(rcInside.Height()) / 2.0 );
	if (bPercent) r = r * 3.0 / 5.0;

	double dOffX = r * cos(dAngle);
	double dOffY = r * sin(dAngle);

	double dX = ((double)(rcInside.right + rcInside.left))/2.0;
	double dY = ((double)(rcInside.top + rcInside.bottom))/2.0;
	
	pt.x = (int)(dX + dOffX);
	pt.y = (int)(dY + dOffY);
}
//---------------------------------------------------------------------------------------
void CMultiChartCtrl::DrawStackedBars (CDC *pdc, short nR, short nC)
{
	CRect rcQuadrante 
		( 
			m_nDCWidthRowHeader + nC * m_nDCWidthCols,
			nR * m_nDCHeightAllRows,
			m_nDCWidthRowHeader + nC * m_nDCWidthCols + m_nDCWidthCols,
			nR * m_nDCHeightAllRows + m_nDCHeightAllRows
		);

	CRect rcBar;

	if ( m_nTypeChart == Tipo_VerticaleCumulativo )
	{
		rcBar.left = rcQuadrante.left + 3;
		rcBar.right = rcQuadrante.right - m_nDCWidthCols / 2;
		rcBar.top = rcQuadrante.top;
		rcBar.bottom = rcQuadrante.top;
	}
	else if ( m_nTypeChart == Tipo_OrizzontaleCumulativo )
	{
		rcBar.left = rcQuadrante.left;
		rcBar.right = rcQuadrante.left;
		rcBar.top = rcQuadrante.top + m_nDCHeightAllRows / 4;
		rcBar.bottom = rcQuadrante.bottom - m_nDCHeightAllRows / 4;
	}
	//----

	for (int nB = 0; nB < m_nBars; nB++)
	{
		CBar * pBar = &(m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[0]);

		CBrush* pBrush = m_gdiRes.GetBrush( pBar -> m_crColor );

		pdc -> SelectObject ( pBrush );

		if ( m_nTypeChart == Tipo_VerticaleCumulativo )
		{
			rcBar.top = rcBar.bottom;
			rcBar.bottom += pBar -> m_nScaleValue ;
		}
		else if ( m_nTypeChart == Tipo_OrizzontaleCumulativo )
		{
			rcBar.left = rcBar.right;
			rcBar.right += pBar -> m_nScaleValue;
			
			if ( pBar -> m_dBase > 0.0 )
			{
				int nBaseOffset = (int) (pBar->m_dBase * m_nDCWidthCols / m_arInfoBars[nB].m_dMaxHeightPosBar);
				rcBar.left += nBaseOffset;
				rcBar.right += nBaseOffset;
			}

			pBar ->m_rcBar = rcBar;	//area sensibile al click
		}

		pdc -> Rectangle (rcBar);
		
		if ( m_bShowHeightBars && m_nTypeChart == Tipo_OrizzontaleCumulativo )
		{
			pdc -> SelectObject( &(m_gdiRes.m_fontValues) );
			
			TEXTMETRIC tmFont;
			pdc->GetTextMetrics( & tmFont );
			int nHFont = tmFont.tmHeight;
			
			CString strH;
			if (pBar->m_strText.IsEmpty())
			{
				strH.Format( m_arInfoBars[pBar -> m_nInd].m_strFormatHeightPosBar, pBar->m_dHeight);
				strH.TrimLeft ();
			}
			else
				strH = pBar->m_strText;

			int nH, nX;

			if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 1 )
			{
				nX = pBar->m_rcBar.left+2;
				nH = pBar->m_rcBar.top;
				if ( nHFont < pBar->m_rcBar.Height() )
					nH += ( pBar->m_rcBar.Height() - nHFont ) / 2;
				pdc->SetTextAlign (TA_LEFT | TA_BOTTOM);
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 2 )
			{
				nX = pBar->m_rcBar.left+2;
				nH = pBar->m_rcBar.top;
				pdc->SetTextAlign (TA_LEFT | TA_TOP);  
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 3 )
			{
				nX = (pBar->m_rcBar.left + pBar->m_rcBar.right) / 2;
				nH = pBar->m_rcBar.top;
				if ( nHFont < pBar->m_rcBar.Height() )
					nH += ( pBar->m_rcBar.Height() - nHFont ) / 2;
				pdc->SetTextAlign (TA_CENTER | TA_BOTTOM);
			}
			else // if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 0 )
			{
				nX = pBar->m_rcBar.left+2;
				nH = pBar->m_rcBar.bottom;
				pdc->SetTextAlign (TA_LEFT | TA_BOTTOM);
			}

			pdc->ExtTextOut
			(
				nX, nH,
				0 , NULL, 
				strH, strH .GetLength(), NULL
			);

		}
	}
}
//---------------------------------------------------------------------------------
void CMultiChartCtrl::DrawPies (CDC *pdc, short nR, short nC)
{
	CRect rcQuadrante 
		( 
			m_nDCWidthRowHeader + nC * m_nDCWidthCols,
			nR * m_nDCHeightAllRows,
			m_nDCWidthRowHeader + nC * m_nDCWidthCols + m_nDCWidthCols,
			nR * m_nDCHeightAllRows + m_nDCHeightAllRows
		);

	CRect rcPie;
	rcPie.left = rcQuadrante.left + 3;
	rcPie.right = rcQuadrante.right - 37;
	rcPie.top = rcQuadrante.top + 20;
	rcPie.bottom = rcQuadrante.bottom - 20;
	//----

	CPoint pt1, pt2;
	int nCurrectAngle = 0;
	double dTotAngle = 0;
	pt1.x = rcPie.right;
	pt1.y = ((rcPie.top + rcPie.bottom) / 2);
	for (int nB = 0; nB < m_nBars; nB++, pt1 = pt2)
	{
		CBar * pBar = & ( m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[0] );

		dTotAngle += pBar -> m_dHeight;
		nCurrectAngle = (int)( dTotAngle * 360 / m_arInfoBars[nB].m_dMaxHeightPosBar);
		CountPoint ( rcPie, nCurrectAngle, pt2 );

		//CPen* pen = (CPen*) (pdc->SelectStockObject(NULL_PEN) );

		if (pt2 != pt1)
		{
			CBrush* pBrush = m_gdiRes.GetBrush( pBar -> m_crColor );

			pdc -> SelectObject ( pBrush );

			pdc -> Pie (rcPie, pt1, pt2);
		}
		//Draw line
		//pdc -> SelectObject ( pen );
		pdc->MoveTo (pt1);
		pdc->LineTo ( rcPie.CenterPoint() );
		pdc->LineTo (pt2);		
	}
	pdc -> SelectStockObject(NULL_BRUSH);
	pdc -> Ellipse (rcPie);
}
//---------------------------------------------------------------------------------

void CMultiChartCtrl::DrawHeightBars (CDC *pdc, short nR, short nC)
{
	CRect rcQuadrante 
		( 
			m_nDCWidthRowHeader + nC * m_nDCWidthCols,
			nR * m_nDCHeightAllRows,
			m_nDCWidthRowHeader + nC * m_nDCWidthCols + m_nDCWidthCols,
			nR * m_nDCHeightAllRows + m_nDCHeightAllRows
		);

	for (int nB = 0; nB < m_nBars; nB++)
	{
		CBar* pBar = &(m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[0]);
		CString strTmp;

		if (pBar->m_strText.IsEmpty())
		{
			strTmp.Format ( m_arInfoBars[pBar->m_nInd].m_strFormatHeightPosBar, pBar->m_dHeight );
			strTmp.TrimLeft ();
		}
		else
			strTmp = pBar->m_strText;

		CSize sz = pdc -> GetTextExtent ( strTmp );

		CRect rcColor (rcQuadrante);
		rcColor.right -= 3;
		rcColor.top += nB * (sz.cy+3) +3;
		rcColor.left = rcColor.right - sz.cy;
		rcColor.bottom = rcColor.top + sz.cy;

		pBar -> m_rcBar = rcColor; //area sensibile al click

		CBrush* pBrush = m_gdiRes.GetBrush( pBar->m_crColor );

		pdc -> SelectObject ( pBrush );

		pdc -> Rectangle ( rcColor ); 

		//pdc -> DrawEdge( rcColor, EDGE_SUNKEN, BF_RECT);
		//pdc -> SetTextColor(GetSysColor(COLOR_WINDOWTEXT));
		rcColor.right = rcColor.left - 3;
		rcColor.left = rcQuadrante.left;

		pdc->SetTextAlign (TA_RIGHT | TA_BOTTOM);

		pdc->ExtTextOut
			(
				rcColor.right, rcColor.top,
				0 , NULL, 
				strTmp, strTmp .GetLength(), NULL
			);

	}
}
