
#include "stdafx.h"

#include "MultiChartCtl.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


//-----------------------------------------------------------------------

void CMultiChartCtrl::DrawBmpIconInfo(CDC* pDC, int nR, int nC, int nB)
{
	CDC memDC;
	memDC.CreateCompatibleDC(pDC);
	memDC.SelectObject( &(m_gdiRes.m_bmpIconInfo) );

	int nWidth, nHeight, nTop, nLeft;
	nWidth = 13;
	nHeight = 13;

	if (m_nTypeChart == Tipo_Verticale)
	{
		nTop = m_nDCHeightAllRows * (nR+1) - 13;
		nLeft = m_nDCWidthRowHeader + 
					m_nDCWidthCols * nC +
					m_arInfoBars[nB].m_nDCXOffsetBar +
					m_arInfoBars[nB].m_nDCWidthBar / 2 -6;
	}
	else if (m_nTypeChart == Tipo_Orizzontale)
	{
		nTop = m_nDCHeightAllRows * (nR)+
				m_arInfoBars[nB].m_nDCYOffsetBar +
				m_arInfoBars[nB].m_nDCWidthBar / 2 -6; 
		nLeft = m_nDCWidthRowHeader + 
				m_nDCWidthCols * (nC+1) -14 ;
	}
	else
	{
		return;
	}

	pDC->BitBlt(nLeft, nTop, nWidth, nHeight,&memDC, 0, 0, SRCCOPY);

	m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_ptIcon.x = nLeft + nWidth/2;
	m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_ptIcon.y = nTop + nHeight/2;
}

//-----------------------------------------------------------------------
void CMultiChartCtrl::DrawItem(LPDRAWITEMSTRUCT lpDIS)
{
	CDC* pDC = CDC::FromHandle(lpDIS->hDC);

	OnDraw(pDC, lpDIS->rcItem);	

}
//-----------------------------------------------------------------------------
void CMultiChartCtrl::DrawSepLine(CDC* pDC, int nXStart, int nYStart, int nXEnd, int nYEnd)
{
	// draws the dot line	
	pDC->MoveTo (nXStart, nYStart);
	pDC->LineTo (nXEnd, nYEnd);
}
//-----------------------------------------------------------------------
void CMultiChartCtrl::OnDraw(CDC* pdc, const CRect& rcBounds)
{
	if (m_State < InvDraw) 
	{
		Refresh();
		return;
	}

	// ----
	pdc->SetBkMode(TRANSPARENT);

	pdc->SetMapMode(MM_ANISOTROPIC);
	pdc->SetWindowExt(1, 1);
	pdc->SetViewportExt(1, - 1);
	pdc->SetViewportOrg(0, rcBounds.bottom);
	pdc->SetWindowOrg (m_nScrollPosX, m_nHeight - rcBounds.bottom - m_nScrollPosY);
	
	CRect rcClient = rcBounds;
	pdc->DPtoLP((CPoint *) & rcClient, 2);

	// ----
	
	if(m_bVScroll)
	{
		rcClient.top -= m_nDCHeightColHeader;
	}
	if(m_bHScroll)
	{
		rcClient.left += m_nDCWidthRowHeader;
	}

	//Imposto il colore di sfondo
	CBrush* pbkBrush = m_gdiRes.GetBrush(m_crBackColor); //RGB(255,255,255)
	//se sto salvando devo colorare anche l'area non visibile a video
	if (m_bIsSaving)
	{
		CRect totalRect(rcClient.left, rcClient.top + m_nHeight, rcClient.left + m_nWidth, 0);
		pdc->FillRect (totalRect, pbkBrush);
	}
	else
	{	//altrimenti lavoro solo sull'area visibile a video
		pdc->IntersectClipRect (rcClient);
		pdc->FillRect (rcClient, pbkBrush);
	}
	//----
	m_nDCStepHeightGrid=0;
	BOOL bVert = false;
	int nProp = 0;

	if( m_nTypeChart == Tipo_Verticale || m_nTypeChart == Tipo_SogliaVerticale || m_nTypeChart == Tipo_VerticaleCumulativo )
	{
		m_nDCStepHeightGrid = (short) (m_dStepHeightGrid * m_nDCHeightAllRows / m_dMaxHeightAllBars);
		bVert = true;
		nProp = m_nDCHeightAllRows;
	}
	else if( m_nTypeChart == Tipo_Orizzontale || m_nTypeChart == Tipo_SogliaOrizzontale || m_nTypeChart == Tipo_OrizzontaleCumulativo )
	{
		m_nDCStepHeightGrid = (short) (m_dStepHeightGrid * m_nDCWidthCols / m_dMaxHeightAllBars);
		bVert = false;
		nProp = m_nDCWidthCols;
	}

	CPen* penGrid = m_gdiRes.GetPen(m_crColorGrid );
	CPen* penSep = m_gdiRes.GetPen(m_crColorZoneSeparator );
	CPen* penOld = pdc->SelectObject(penSep);

	// Visualizza i Quadranti
	int n;
	int nStepY = int(m_dStepHeightGrid * nProp / m_dMaxHeightAllBars);
	for(n = 0; n < (m_nHeight - m_nDCHeightColHeader); n += m_nDCHeightAllRows)
	{	// linee orizzontali
		pdc->SelectObject(penSep);

		pdc->MoveTo(m_nDCWidthRowHeader  /*0*/, n);
		pdc->LineTo(m_nWidth, n);

		if (m_bShowGrid && bVert && m_nDCStepHeightGrid > 0)
		{ //griglia
			pdc->SelectObject(penGrid);

			int nY = n;
			int g;
			for (g = m_nDCStepHeightGrid; g < (m_nDCHeightAllRows); g += m_nDCStepHeightGrid)
			{
				nY += nStepY;

				pdc->MoveTo(m_nScrollPosX + m_nDCWidthRowHeader, nY);
				pdc->LineTo(m_nWidth, nY);
			}
		}
		//soglie
		if (m_nNumeroSoglie > 0 && bVert)
		{ 
			int nY = n;
			for (int i = 0; i < m_nNumeroSoglie; i++)
			{
				int nDCSoglia = (short) (m_arSoglie[i].m_dHeight * m_nDCHeightAllRows / m_dMaxHeightAllBars);
				nY += nDCSoglia;

				pdc->SelectObject(m_gdiRes.GetPen(m_arSoglie[i].m_crColor));

				pdc->MoveTo(m_nScrollPosX + m_nDCWidthRowHeader, nY);
				pdc->LineTo(m_nWidth, nY);
				
			}
		}

	}

	for (n = m_nDCWidthRowHeader + m_nDCWidthCols; n <= m_nWidth; n += m_nDCWidthCols)
	{	// linee verticali
		pdc->SelectObject(penSep);

		pdc->MoveTo(n, 0);
		pdc->LineTo(n, m_nHeight - m_nDCHeightColHeader);

		if( n < m_nWidth && m_bShowGrid && ! bVert && m_nDCStepHeightGrid)
		{	//griglia
			pdc->SelectObject(penGrid);

			int g, i;
			for(g = m_nDCStepHeightGrid, i=1; g < (m_nDCWidthCols); g += m_nDCStepHeightGrid, i++)
			{
				int nX = int(n + (m_dStepHeightGrid * i * nProp / m_dMaxHeightAllBars));

				pdc->MoveTo(nX, 0 );
				pdc->LineTo(nX, m_nHeight - m_nDCHeightColHeader );
			}
		}
	}

	pdc->SelectObject(penSep);
	pdc->MoveTo(0, 0);
	pdc->LineTo(m_nWidth, 0);
	pdc->LineTo(m_nWidth, m_nHeight);

	pdc->SelectObject(penOld);
	// ----
	
	DrawCorner(pdc, rcBounds);
	
	pdc->SelectObject(penOld);

	if (!m_bVScroll) DrawColHeader(pdc, rcBounds);

	pdc->SelectObject(penOld);

	if (!m_bHScroll) DrawRowHeader(pdc, rcBounds);
	
	DrawBars(pdc, rcBounds);

	// ----
	m_bHScroll = FALSE;
	m_bVScroll = FALSE;

//	pdc->SelectObject(pOldFont);
}
//--------------------------------------------------------------------------------

void CMultiChartCtrl::PrintHeaderText 
	( 
		CDC* pdc, CRect rcBounds, 
		CHeaderInfo& infoHeader, 
		short nNumLabel,
		short nC,
		BOOL bIsSelected
	)
{
	if ( ! nNumLabel ) return;

	COLORREF crOld = pdc->SetTextColor(infoHeader.m_crTextColor);
	COLORREF crOldBk = pdc->SetBkColor(infoHeader.m_crBackColor);

	int nAlign;
	int nX = rcBounds.left;
	switch (infoHeader.m_nAlign)
	{
	case -1:
		nAlign = TA_LEFT;
		nX = rcBounds.left;
		break;
	case 0:
		nAlign = TA_CENTER;
		nX = (rcBounds.left + rcBounds.right) / 2;
		break;
	case 1:
		nAlign = TA_RIGHT;
		nX = rcBounds.right;
		break;
	}
	
	pdc->SetTextAlign (nAlign | TA_TOP);

	TEXTMETRIC tm;
	pdc->GetTextMetrics ( &tm ); 
	rcBounds.top -= tm.tmHeight / 4;	// interlinea: 1/4

	for(int nInd = 0; nInd < nNumLabel; nInd++)
	{	
		pdc->ExtTextOut
			(
				nX,
				rcBounds.top,
				ETO_CLIPPED,
				rcBounds, 
				infoHeader.m_arstrTitle[nInd],
				infoHeader.m_arstrTitle[nInd].GetLength(),
				NULL
			);

		rcBounds.top -= (int) (tm.tmHeight * 1.25);	// una riga + interlinea 1/4
		if( rcBounds.top < rcBounds.bottom ) break;
	}

	if ( (m_bShowTotBars || m_bShowTitleBars) && nC >= 0 && ! (rcBounds.top < rcBounds.bottom) )
	{
		CString strT;
		//double dBarWidth = ( (2.0 * m_nDCWidthCols) / (m_nBars + m_nBars * 2 + 1) );		
		int nB;

		if ( m_bShowTitleBars)
		{
			for ( nB = 0; nB < m_nBars; nB++ )
			{
				strT = m_arInfoBars[ nB ].m_strTitlePosBar ;
				
				//int nLeft = rcBounds.left + (int) ( (nB+1) * dBarWidth / 2.0 + nB * dBarWidth );
				int nLeft = rcBounds.left + m_arInfoBars[nB].m_nDCXOffsetBar;
				int nX = nLeft + m_arInfoBars[nB].m_nDCWidthBar / 2; // nLeft+dBarWidth / 2;
				
				pdc->ExtTextOut
					(
						nX,
						rcBounds.top,
						ETO_CLIPPED , rcBounds, 
						strT , strT.GetLength (), NULL
					);
			}
			rcBounds.top -= (int) (tm.tmHeight * 1.25);	// una riga + interlinea 1/4
			//if( rcBounds.top < rcBounds.bottom ) break;
		}
		// ----
		if ( m_bShowTotBars  && ! (rcBounds.top < rcBounds.bottom) )
		{
			for ( nB = 0; nB < m_nBars; nB++ )
			{
				strT.Format ( m_arInfoBars[nB].m_strFormatHeightPosBar, m_arTotHeightPosBar [ nC * m_nBars + nB ] );
				
				//int nLeft = rcBounds.left + (int) ( (nB+1) * dBarWidth / 2.0 + nB * dBarWidth );
				//int nX = nLeft + (int)(dBarWidth / 2);
				int nLeft = rcBounds.left + m_arInfoBars[nB].m_nDCXOffsetBar;
				int nX = nLeft + m_arInfoBars[nB].m_nDCWidthBar / 2;
				
				pdc->ExtTextOut
					(
						nX,
						rcBounds.top,
						ETO_CLIPPED , rcBounds, 
						strT , strT.GetLength (), NULL
					);
			}
		}
		// ----
	}
	pdc->SetTextColor(crOld);
	pdc->SetBkColor(crOldBk);
}
//--------------------------------------------------------------------------------

/*void CMultiChartCtrl::DrawRowHeader(CDC* pdc, const CRect& rcBounds)
{
	if ( m_nDCWidthRowHeader == 0 ) return;

	CPoint ptWinOrg = pdc->GetWindowOrg();
	CPen* penOld = NULL;
	CRect rcHeaderBox;
	int nR;
	int nWidth = m_nDCWidthRowHeader;

	COLORREF crBackgnd = GetSysColor(COLOR_BTNFACE);
	COLORREF crForegnd = GetSysColor(COLOR_BTNTEXT);

	CPen		pen (PS_SOLID, 1, crForegnd);
	CPen*		pOldPen		= pdc->SelectObject	(&pen);
	int			nOldBkMode	= pdc->SetBkMode	(TRANSPARENT);
	COLORREF	crOldBackgnd= pdc->SetBkColor	(crBackgnd);
	COLORREF	crOldForegnd= pdc->SetTextColor	(crForegnd);
	UINT		nOldAlign	= pdc->SetTextAlign	(TA_CENTER);
	CPen		pShdw(PS_SOLID, 1, GetSysColor(COLOR_BTNSHADOW));
	
	CPen		pLght(PS_SOLID, 1, GetSysColor(COLOR_BTNHIGHLIGHT));
	CPen		pShdwLight(PS_SOLID, 1, GetSysColor(COLOR_ACTIVEBORDER));
	CPen*		pTmpPen;
	CPen		pGray1(PS_SOLID, 1, RGB(226,222,205));
	CPen		pGray2(PS_SOLID, 1, RGB(214,210,194));
	CPen		pGray3(PS_SOLID, 1, RGB(203,199,184));
	

	int nYHScrollSize = GetSystemMetrics(SM_CYHSCROLL);

	CRect rcClipRect;
	rcClipRect = rcBounds;	
	pdc->DPtoLP((CPoint *) &rcClipRect, 2);
	rcClipRect.top  -= m_nDCHeightColHeader;
	//rcClipRect.bottom -= nYHScrollSize;

	if ( ! m_bIsSaving ) 
		pdc->IntersectClipRect (rcClipRect);

	if (m_nShowGridScale && m_nTypeChart == Tipo_Verticale)
	{
		pdc->SelectObject( &(this->m_gdiRes.m_fontValues) );
		
		CString str;
		str.Format(m_strFormatHeightBars, m_dMaxHeightAllBars);
		CSize size = pdc->GetTextExtent(str);
		nWidth = max(0, m_nDCWidthRowHeader - size.cx -2);

		CPen* penSep = m_gdiRes.GetPen(m_crColorZoneSeparator );
		CPen* penOld = pdc->SelectObject(penSep);

		pdc->MoveTo(ptWinOrg.x + nWidth, 0);
		pdc->LineTo(ptWinOrg.x + nWidth, m_nHeight - m_nDCHeightColHeader);

		pdc->MoveTo(ptWinOrg.x + m_nDCWidthRowHeader-1, 0);
		pdc->LineTo(ptWinOrg.x + m_nDCWidthRowHeader-1, m_nHeight - m_nDCHeightColHeader);

		pdc->SetTextAlign (TA_RIGHT);
		TEXTMETRIC tmFont;
		pdc->GetTextMetrics( & tmFont );
		int nHalfHFont = tmFont.tmHeight /2;
				
		CPen* penGrid = m_gdiRes.GetPen(m_crColorGrid );
		pdc->SelectObject(penGrid);

		int nX = ptWinOrg.x + m_nDCWidthRowHeader -10;

		for (int n = 0; n < (m_nHeight - m_nDCHeightColHeader); n += m_nDCHeightAllRows)
		{	
			int i = 1; int nXX =0;
			for(int gg = m_nDCStepHeightGrid; gg < (m_nDCHeightAllRows); gg += m_nDCStepHeightGrid, i++)
			{
				if (m_nShowGridScale == 1)
					nXX = ptWinOrg.x + m_nDCWidthRowHeader - (i & 1 ? 5 : 10);
				else if (m_nShowGridScale == 2)
					nXX = ptWinOrg.x + m_nDCWidthRowHeader - (i & 1 ? 10 : 5);

				int nY =  n + (short) (m_dStepHeightGrid * i * m_nDCHeightAllRows / m_dMaxHeightAllBars);

				if ( (i & 1) && m_nShowGridScale == 1)
				{	//dispari
					nXX = ptWinOrg.x + m_nDCWidthRowHeader - (i & 1 ? 5 : 10);

					CString strH;
					strH.Format( (LPCTSTR)m_strFormatHeightBars, m_dStepHeightGrid * i);
					pdc->ExtTextOut
							(
								nXX,
								nY + nHalfHFont,
								0,
								NULL, 
								strH,
								strH.GetLength(),
								NULL
							);
				}
				else if ( (i & 1) == 0 && m_nShowGridScale == 2)
				{	//pari
					nX = ptWinOrg.x + m_nDCWidthRowHeader - (i & 1 ? 10 : 5);

					CString strH;
					strH.Format( (LPCTSTR)m_strFormatHeightBars, m_dStepHeightGrid * i);
					pdc->ExtTextOut
							(
								nXX,
								nY + nHalfHFont,
								0,
								NULL, 
								strH,
								strH.GetLength(),
								NULL
							);
				}

				pdc->MoveTo(nXX, nY);
				pdc->LineTo(ptWinOrg.x + m_nDCWidthRowHeader, nY);
			}
		}
		pdc->SelectObject(penOld);
	}

	pdc->SelectObject( &(this->m_gdiRes.m_fontHeaders) );
	
	for(nR = 0; nR < m_nRows; ++nR)
	{
		rcHeaderBox.SetRect(
			ptWinOrg.x,
			(nR + 1) * m_nDCHeightAllRows ,
			ptWinOrg.x + nWidth,
			nR * m_nDCHeightAllRows
		);

		CBrush* pBrush = m_gdiRes.GetBrush(crBackgnd);
		pdc->SelectObject(pBrush);
	    pdc->FillRect(rcHeaderBox,pBrush);
		

		if (nR == m_nSelectedRowHeader)
		{
		//Ombreggiatura row header
		// linee scure sotto 
		pTmpPen	= pdc->SelectObject(&pGray3);
		DrawSepLine	(pdc,rcHeaderBox.left ,rcHeaderBox.bottom + 2,rcHeaderBox.right ,rcHeaderBox.bottom + 2);

		pdc->SelectObject(&pGray2);
		DrawSepLine	(pdc,rcHeaderBox.left ,rcHeaderBox.bottom + 3,rcHeaderBox.right ,rcHeaderBox.bottom + 3);
		
		pdc->SelectObject(&pGray1);
		DrawSepLine	(pdc,rcHeaderBox.left ,rcHeaderBox.bottom + 4,rcHeaderBox.right ,rcHeaderBox.bottom + 4);
		
		}

		// linea chiara e scura a destra
		pTmpPen	= pdc->SelectObject(&pShdw);
		DrawSepLine	(pdc,rcHeaderBox.left ,rcHeaderBox.bottom + 1,rcHeaderBox.right ,rcHeaderBox.bottom + 1);

		pTmpPen	= pdc->SelectObject(&pLght);
		DrawSepLine	(pdc,rcHeaderBox.left ,rcHeaderBox.bottom ,rcHeaderBox.right ,rcHeaderBox.bottom );
	
		
		rcHeaderBox.left += 3;
		rcHeaderBox.top -= 3;

		if ( m_nNumLabelRowHeader )
		{
			PrintHeaderText(pdc, rcHeaderBox, m_arInfoRowHeader[nR], m_nNumLabelRowHeader, -1, nR == m_nSelectedRowHeader);
		}

		pdc->MoveTo(ptWinOrg.x + m_nWidth, 0);
		pdc->LineTo(ptWinOrg.x + m_nWidth, m_nHeight);

	}

}
//-----------------------------------------------------------------------------
void CMultiChartCtrl::DrawColHeader(CDC * pdc, const CRect& rcBounds)
{	
	if(m_nDCHeightColHeader == 0) return;

	pdc->SelectObject( &(this->m_gdiRes.m_fontHeaders) );

	CPoint ptWinOrg = pdc->GetWindowOrg();
	CRect rcHeaderBox;	
	CPen* penOld = NULL;
	
	COLORREF crBackgnd = GetSysColor(COLOR_BTNFACE);
	COLORREF crForegnd = GetSysColor(COLOR_BTNTEXT);

	CPen		pen (PS_SOLID, 1, crForegnd);
	CPen*		pOldPen		= pdc->SelectObject	(&pen);
	int			nOldBkMode	= pdc->SetBkMode	(TRANSPARENT);
	COLORREF	crOldBackgnd= pdc->SetBkColor	(crBackgnd);
	COLORREF	crOldForegnd= pdc->SetTextColor	(crForegnd);
	UINT		nOldAlign	= pdc->SetTextAlign	(TA_CENTER);
	CPen		pShdw(PS_SOLID, 1, GetSysColor(COLOR_BTNSHADOW));
	

	CPen		pLght(PS_SOLID, 1, GetSysColor(COLOR_BTNHIGHLIGHT));
	CPen		pShdwLight(PS_SOLID, 1, GetSysColor(COLOR_ACTIVEBORDER));
	CPen*		pTmpPen;
	CPen		pGray1(PS_SOLID, 1, RGB(226,222,205));
	CPen		pGray2(PS_SOLID, 1, RGB(214,210,194));
	CPen		pGray3(PS_SOLID, 1, RGB(203,199,184));

	int nX, nC;
	int nYVScrollSize = GetSystemMetrics(SM_CYVSCROLL);

	CRect rcClipRect;
	rcClipRect = rcBounds;	
	pdc->DPtoLP((CPoint *) &rcClipRect, 2);
	rcClipRect.left  += m_nDCWidthRowHeader;
	//rcClipRect.right -= nYVScrollSize;

	if ( ! m_bIsSaving ) 
		pdc->IntersectClipRect (rcClipRect);

	
	int nTop = ptWinOrg.y + rcBounds.bottom;
	int nBottom = ptWinOrg.y + rcBounds.bottom - m_nDCHeightColHeader;

	for(nX = ptWinOrg.x, nC = 0; nC < m_nCols; ++nC, nX += m_nDCWidthCols)
	{
		rcHeaderBox.SetRect(
			m_nDCWidthRowHeader + nC * m_nDCWidthCols,
			nTop,
			m_nDCWidthRowHeader + (nC+1) * m_nDCWidthCols,
			nBottom
		);
	
		CBrush* pBrush = m_gdiRes.GetBrush(crBackgnd);
		/*CBrush* brushOld = *//*pdc->SelectObject( pBrush );
		pdc->FillRect(rcHeaderBox,pBrush);
		
		if (nC == m_nSelectedColHeader)
		{
			//Ombreggiatura column header
			// linee scure sotto 
			pTmpPen	= pdc->SelectObject(&pGray3);
			DrawSepLine	(pdc, rcHeaderBox.left, rcHeaderBox.bottom + 2, rcHeaderBox.right , rcHeaderBox.bottom + 2);

			pdc->SelectObject(&pGray2);
			DrawSepLine	(pdc, rcHeaderBox.left, rcHeaderBox.bottom +3, rcHeaderBox.right , rcHeaderBox.bottom + 3);

			pdc->SelectObject(&pGray1);
			DrawSepLine	(pdc, rcHeaderBox.left, rcHeaderBox.bottom +4, rcHeaderBox.right , rcHeaderBox.bottom +4);
		}
		
		// linea chiara e scura a destra
		pTmpPen	= pdc->SelectObject(&pShdw);
		DrawSepLine	(pdc, rcHeaderBox.right, rcHeaderBox.top + 5, rcHeaderBox.right - 1 , rcHeaderBox.bottom - 5);
		pTmpPen	= pdc->SelectObject(&pLght);
		DrawSepLine	(pdc, rcHeaderBox.right, rcHeaderBox.top + 5, rcHeaderBox.right - 1, rcHeaderBox.bottom - 5);

		pdc->SelectObject(pTmpPen);
	

	//---- 3D

	/*	if (nC == m_nSelectedColHeader)
			pdc->SelectStockObject(BLACK_PEN);
		else
			pdc->SelectStockObject(WHITE_PEN);

		pdc->MoveTo(rcHeaderBox.left +1, rcHeaderBox.top -1 );
		pdc->LineTo(rcHeaderBox.right -1, rcHeaderBox.top -1 );

		pdc->MoveTo(rcHeaderBox.left +1, rcHeaderBox.top -1 );
		pdc->LineTo(rcHeaderBox.left +1, rcHeaderBox.bottom +1 );

		if (nC == m_nSelectedColHeader)
		{
			pdc->SelectStockObject(WHITE_PEN);

			pdc->MoveTo(rcHeaderBox.left +1, rcHeaderBox.bottom+1 );
			pdc->LineTo(rcHeaderBox.right -1, rcHeaderBox.bottom+1 );

			pdc->MoveTo(rcHeaderBox.right -1, rcHeaderBox.top -1);
			pdc->LineTo(rcHeaderBox.right -1, rcHeaderBox.bottom +1);
		}

		pdc->SelectStockObject(BLACK_PEN);*/
		//----
		
	/*	if ( m_nNumLabelColHeader )
		{
			PrintHeaderText(pdc, rcHeaderBox, m_arInfoColHeader[nC], m_nNumLabelColHeader, nC, nC == m_nSelectedColHeader );
		}

		pdc->MoveTo(0, nBottom);
		pdc->LineTo(m_nWidth, nBottom);
	}	

	pdc->SelectObject(penOld);
}
//----------------------------------------------------------------------------------
void CMultiChartCtrl::DrawCorner(CDC* pdc, const CRect& rcBounds)
{
	// angolo superiore-sinistro
	CPoint ptWinOrg = pdc->GetWindowOrg();
	CRect rcHeaderBox;
	rcHeaderBox.SetRect(
			ptWinOrg.x,
			ptWinOrg.y + rcBounds.bottom ,
			ptWinOrg.x + m_nDCWidthRowHeader,
			ptWinOrg.y + rcBounds.bottom - m_nDCHeightColHeader
	);

	pdc->SelectStockObject( GRAY_BRUSH );
	pdc->Rectangle(rcHeaderBox);
	//---- 3D
	pdc->SelectStockObject(WHITE_PEN);
	pdc->MoveTo(rcHeaderBox.left + 1, rcHeaderBox.top - 1 );
	pdc->LineTo(rcHeaderBox.right - 1, rcHeaderBox.top - 1 );

	pdc->MoveTo(rcHeaderBox.left + 1, rcHeaderBox.top - 1 );
	pdc->LineTo(rcHeaderBox.left + 1, rcHeaderBox.bottom + 1 );

	pdc->SelectStockObject(BLACK_PEN);
	//----

	//pdc->ExcludeClipRect(rcHeaderBox);
}*/
//----------------------------------------------------------------------------------
void CMultiChartCtrl::DrawRowHeader(CDC* pdc, const CRect& rcBounds)
{
	if ( m_nDCWidthRowHeader == 0 ) return;


	CPoint ptWinOrg = pdc->GetWindowOrg();
	CPen* penOld = NULL;
	CRect rcHeaderBox;
	int nR;
	int nWidth = m_nDCWidthRowHeader;

	if (m_nShowGridScale && m_nTypeChart == Tipo_Verticale)
	{
		pdc->SelectObject( &(this->m_gdiRes.m_fontValues) );
		
		CString str;
		str.Format(m_strFormatHeightBars, m_dMaxHeightAllBars);
		CSize size = pdc->GetTextExtent(str);
		nWidth = max(0, m_nDCWidthRowHeader - size.cx -2);

		CPen* penSep = m_gdiRes.GetPen(m_crColorZoneSeparator);
		CPen* penOld = pdc->SelectObject(penSep);

		pdc->MoveTo(ptWinOrg.x + nWidth, 0);
		pdc->LineTo(ptWinOrg.x + nWidth, m_nHeight - m_nDCHeightColHeader);

		pdc->MoveTo(ptWinOrg.x + m_nDCWidthRowHeader-1, 0);
		pdc->LineTo(ptWinOrg.x + m_nDCWidthRowHeader-1, m_nHeight - m_nDCHeightColHeader);

		pdc->SetTextAlign (TA_RIGHT);
		TEXTMETRIC tmFont;
		pdc->GetTextMetrics( & tmFont );
		int nHalfHFont = tmFont.tmHeight /2;
				
		CPen* penGrid = m_gdiRes.GetPen(m_crColorGrid );
		pdc->SelectObject(penGrid);

		int nX = ptWinOrg.x + m_nDCWidthRowHeader -10;

		for (int n = 0; n < (m_nHeight - m_nDCHeightColHeader); n += m_nDCHeightAllRows)
		{	
			int i = 1; int nXX =0;
			for(int gg = m_nDCStepHeightGrid; gg < (m_nDCHeightAllRows); gg += m_nDCStepHeightGrid, i++)
			{
				if (m_nShowGridScale == 1)
					nXX = ptWinOrg.x + m_nDCWidthRowHeader - (i & 1 ? 5 : 10);
				else if (m_nShowGridScale == 2)
					nXX = ptWinOrg.x + m_nDCWidthRowHeader - (i & 1 ? 10 : 5);

				int nY =  n + (short) (m_dStepHeightGrid * i * m_nDCHeightAllRows / m_dMaxHeightAllBars);

				if ( (i & 1) && m_nShowGridScale == 1)
				{	//dispari
					nXX = ptWinOrg.x + m_nDCWidthRowHeader - (i & 1 ? 5 : 10);

					CString strH;
					strH.Format( (LPCTSTR)m_strFormatHeightBars, m_dStepHeightGrid * i);
					pdc->ExtTextOut
							(
								nXX,
								nY + nHalfHFont,
								0,
								NULL, 
								strH,
								strH.GetLength(),
								NULL
							);
				}
				else if ( (i & 1) == 0 && m_nShowGridScale == 2)
				{	//pari
					nX = ptWinOrg.x + m_nDCWidthRowHeader - (i & 1 ? 10 : 5);

					CString strH;
					strH.Format( (LPCTSTR)m_strFormatHeightBars, m_dStepHeightGrid * i);
					pdc->ExtTextOut
							(
								nXX,
								nY + nHalfHFont,
								0,
								NULL, 
								strH,
								strH.GetLength(),
								NULL
							);
				}

				pdc->MoveTo(nXX, nY);
				pdc->LineTo(ptWinOrg.x + m_nDCWidthRowHeader, nY);
			}
		}
		pdc->SelectObject(penOld);
	}

	pdc->SelectObject( &(this->m_gdiRes.m_fontHeaders) );
	
	for(nR = 0; nR < m_nRows; ++nR)
	{
		rcHeaderBox.SetRect(
			ptWinOrg.x,
			(nR + 1) * m_nDCHeightAllRows - 1 ,
			ptWinOrg.x + nWidth,
			nR * m_nDCHeightAllRows
		);

		//CBrush* pBrush = m_gdiRes.GetBrush(m_arInfoRowHeader[nR].m_crBackColor);
		//pdc->SelectObject(pBrush);
		//pdc->Rectangle(rcHeaderBox);
		pdc->FillSolidRect(rcHeaderBox, m_arInfoRowHeader[nR].m_crBackColor);

		rcHeaderBox.left += 3;
		rcHeaderBox.top -= 3;

		if ( m_nNumLabelRowHeader )
		{
			PrintHeaderText(pdc, rcHeaderBox, m_arInfoRowHeader[nR], m_nNumLabelRowHeader, -1, nR == m_nSelectedRowHeader);
		}

		pdc->MoveTo(ptWinOrg.x + m_nWidth, 0);
		pdc->LineTo(ptWinOrg.x + m_nWidth, m_nHeight);
	}

}
//-----------------------------------------------------------------------------
void CMultiChartCtrl::DrawColHeader(CDC * pdc, const CRect& rcBounds)
{
	if(m_nDCHeightColHeader == 0) return;

	pdc->SelectObject( &(this->m_gdiRes.m_fontHeaders) );

	CPoint ptWinOrg = pdc->GetWindowOrg();
	CRect rcHeaderBox;	
	CPen* penOld = NULL;

	int nX, nC;

	int nTop = ptWinOrg.y + rcBounds.bottom;
	int nBottom = ptWinOrg.y + rcBounds.bottom - m_nDCHeightColHeader;

	for(nX = ptWinOrg.x, nC = 0; nC < m_nCols; ++nC, nX += m_nDCWidthCols)
	{
		rcHeaderBox.SetRect(
			m_nDCWidthRowHeader + nC * m_nDCWidthCols + 1,
			nTop,
			m_nDCWidthRowHeader + (nC+1) * m_nDCWidthCols,
			nBottom
		);
	
		//CBrush* pBrush = m_gdiRes.GetBrush(m_arInfoColHeader[nC].m_crBackColor);
		//pdc->SelectObject( pBrush );
		//pdc->Rectangle(rcHeaderBox);
		pdc->FillSolidRect(rcHeaderBox, m_arInfoColHeader[nC].m_crBackColor);

		if ( m_nNumLabelColHeader )
		{
			PrintHeaderText(pdc, rcHeaderBox, m_arInfoColHeader[nC], m_nNumLabelColHeader, nC, nC == m_nSelectedColHeader );
		}

		//pdc->MoveTo(0, nBottom);
		//pdc->LineTo(m_nWidth, nBottom);
	}	
}
//----------------------------------------------------------------------------------
void CMultiChartCtrl::DrawCorner(CDC* pdc, const CRect& rcBounds)
{
	// angolo superiore-sinistro
	CPoint ptWinOrg = pdc->GetWindowOrg();
	CRect rcHeaderBox;
	rcHeaderBox.SetRect(
			ptWinOrg.x,
			ptWinOrg.y + rcBounds.bottom ,
			ptWinOrg.x + m_nDCWidthRowHeader,
			ptWinOrg.y + rcBounds.bottom - m_nDCHeightColHeader
	);

	//pdc->SelectStockObject( GRAY_BRUSH );
	//pdc->Rectangle(rcHeaderBox);

	pdc->FillSolidRect(rcHeaderBox, m_crLeftUpperCornerBackColor);

	////---- 3D
	//pdc->SelectStockObject(WHITE_PEN);
	//pdc->MoveTo(rcHeaderBox.left + 1, rcHeaderBox.top - 1 );
	//pdc->LineTo(rcHeaderBox.right - 1, rcHeaderBox.top - 1 );

	//pdc->MoveTo(rcHeaderBox.left + 1, rcHeaderBox.top - 1 );
	//pdc->LineTo(rcHeaderBox.left + 1, rcHeaderBox.bottom + 1 );

	//pdc->SelectStockObject(BLACK_PEN);
	////----

	//pdc->ExcludeClipRect(rcHeaderBox);
}
//---------------------------------------------------------------------------
void CMultiChartCtrl::DrawOneBar(CDC *pdc, CBar *pBar, BOOL bSelected /*= FALSE*/)
{
	COLORREF crColor;
	int nHatch = 0;
	int nBase = 0;

	if 
		( 
			( m_nTypeChart == Tipo_SogliaVerticale || 
			  m_nTypeChart == Tipo_SogliaOrizzontale ) 
			?
			m_arLimits [ pBar->m_nInd ]. m_nLimits 
			: false
		)
	{
		int nL;
		for 
			( 
				nL = 0; 
				nL < m_arLimits [ pBar->m_nInd ]. m_nLimits && 
				pBar -> m_dHeight > m_arLimits [ pBar->m_nInd ]. m_arValLimits [nL]; 
				nL++
			) ; //no body OK

		if ( nL == m_arLimits [ pBar->m_nInd ]. m_nLimits ) 
			crColor = RGB(0,0,0) ;
		else
			crColor =  m_arLimits [ pBar->m_nInd ]. m_arColorLimits [ nL ];
	}
	else
	{
		crColor = pBar->m_crColor;
		nHatch = pBar->m_nHatchBrush;
	}

	CPen* penOld = NULL;
	if( m_bHideBoxBars )
	{
		CPen* pen = m_gdiRes.GetPen( crColor );
		penOld = pdc->SelectObject ( pen );
	}
	
	CBrush* pBrush = m_gdiRes.GetBrush(crColor);
	pdc->SelectObject ( pBrush );

	if 
		(
			m_nTypeChart == Tipo_Orizzontale &&
			m_arInfoBars[pBar->m_nInd].m_nDCShowMinHeightPosBar != 0 && 
			pBar->m_dHeight &&
			(pBar->m_rcBar.right - pBar->m_rcBar.left +1) < m_arInfoBars[pBar->m_nInd].m_nDCShowMinHeightPosBar
		)
	{
		CPoint pts[3];
		pts[0].x = pBar->m_rcBar.left;
		pts[0].y = pBar->m_rcBar.top;
		pts[1].x = pBar->m_rcBar.left;
		pts[1].y = pBar->m_rcBar.bottom;
		pts[2].x = pBar->m_rcBar.left + m_arInfoBars[pBar->m_nInd].m_nDCShowMinHeightPosBar;
		pts[2].y = pBar->m_rcBar.bottom + (pBar->m_rcBar.top - pBar->m_rcBar.bottom)/2;
		pdc->Polygon(pts,3);
		
		if (pBar->m_nHatchBrush)
		{
			int nOldMode = pdc->SetBkMode(TRANSPARENT);
			pdc->SelectObject ( m_gdiRes.GetBrush(0, pBar->m_nHatchBrush) );
			pdc->Polygon(pts,3);
			pdc->SetBkMode(nOldMode);
		}
	}
	else if 
		(
			m_nTypeChart == Tipo_Verticale &&
			m_arInfoBars[pBar->m_nInd].m_nDCShowMinHeightPosBar != 0 && 
			pBar->m_dHeight &&
			(pBar->m_rcBar.top - pBar->m_rcBar.bottom +1) < m_arInfoBars[pBar->m_nInd].m_nDCShowMinHeightPosBar
		)
	{
		CPoint pts[3];
		pts[0].x = pBar->m_rcBar.left;
		pts[0].y = pBar->m_rcBar.bottom;
		pts[1].x = pBar->m_rcBar.right;
		pts[1].y = pBar->m_rcBar.bottom;
		pts[2].x = pBar->m_rcBar.left + (pBar->m_rcBar.right - pBar->m_rcBar.left)/2;
		pts[2].y = pBar->m_rcBar.bottom + m_arInfoBars[pBar->m_nInd].m_nDCShowMinHeightPosBar;
	
		pdc->Polygon(pts,3);
		if (pBar->m_nHatchBrush)
		{
			int nOldMode = pdc->SetBkMode(TRANSPARENT);
			pdc->SelectObject ( m_gdiRes.GetBrush(0, pBar->m_nHatchBrush) );
			pdc->Polygon(pts,3);
			pdc->SetBkMode(nOldMode);
		}
	}
	else 
	{
		pdc->Rectangle (pBar->m_rcBar);
		if (pBar->m_nHatchBrush)
		{
			int nOldMode = pdc->SetBkMode(TRANSPARENT);
			pdc->SelectObject ( m_gdiRes.GetBrush(0, pBar->m_nHatchBrush) );
			pdc->Rectangle ( pBar->m_rcBar );
			pdc->SetBkMode(nOldMode);
		}
	}

	if ( pBar->m_dBase )
	{	
		if ( m_nTypeChart == Tipo_SogliaVerticale || m_nTypeChart == Tipo_Verticale )
			nBase = (int) (pBar->m_dBase * m_nDCHeightAllRows / m_arInfoBars [pBar->m_nInd].m_dMaxHeightPosBar );
		else if ( m_nTypeChart == Tipo_SogliaOrizzontale || m_nTypeChart == Tipo_Orizzontale || m_nTypeChart == Tipo_OrizzontaleCumulativo )	
			nBase = (int) ( pBar->m_dBase * m_nDCWidthCols / m_arInfoBars[pBar->m_nInd].m_dMaxHeightPosBar  );
	}

	if ( 
			m_bShowTrueValue && 
			( m_nTypeChart == Tipo_SogliaVerticale || 
			  m_nTypeChart == Tipo_SogliaOrizzontale )
		)
	{
		CRect rcTrueH ( pBar ->m_rcBar );
		int nW;
		if ( m_nTypeChart == Tipo_SogliaVerticale )
		{
			nW = rcTrueH.Width() / 5;
			rcTrueH.top = rcTrueH.bottom + pBar -> m_nScaleValue;

			rcTrueH.bottom += nBase;
			rcTrueH.top += nBase;
			rcTrueH.left += 2*nW;
			rcTrueH.right -= 2*nW;
		}
		else if ( m_nTypeChart == Tipo_SogliaOrizzontale )
		{
			nW = abs( rcTrueH.Height() ) / 5;	
			rcTrueH.right = rcTrueH.left + pBar -> m_nScaleValue;

			rcTrueH.left += nBase;
			rcTrueH.right += nBase;
			rcTrueH.top -= 2*nW;
			rcTrueH.bottom += 2*nW;
		}
		CBrush* pBrushH = m_gdiRes.GetBrush( RGB(0,0,0) );
		pdc->SelectObject ( pBrushH );
		pdc->Rectangle (rcTrueH);
	}

	if (penOld)
	{
		pdc->SelectObject(penOld);
	}

	// ----	
	if ( m_bShowHeightBars && m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar >= 0 )
	{
		CFont* fontOld = pdc->SelectObject(&(m_gdiRes.m_fontValues));

		TEXTMETRIC tmFont;
		pdc->GetTextMetrics(&tmFont);
		int nHFont = tmFont.tmHeight;
		CString strH;

		if (pBar->m_strText.IsEmpty())
		{
			strH.Format( m_arInfoBars[pBar->m_nInd].m_strFormatHeightPosBar, pBar->m_dHeight);
			strH.TrimLeft ();
		}
		else
			strH = pBar->m_strText;

		int nH, nX;
		if( m_nTypeChart == Tipo_Verticale || m_nTypeChart == Tipo_SogliaVerticale )
		{
			if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 1 )
			{	//alla base centrato
				nX = (pBar->m_rcBar.left + pBar->m_rcBar.right) / 2;
				nH = pBar->m_rcBar.bottom;
				pdc->SetTextAlign (TA_CENTER | TA_BOTTOM);
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 0 )
			{	//sopra centrato
				nX = (pBar->m_rcBar.left + pBar->m_rcBar.right) / 2;
				nH = ( 
						(max(pBar->m_rcBar.top - pBar->m_rcBar.bottom+1, m_arInfoBars[pBar->m_nInd].m_nDCShowMinHeightPosBar) + nHFont + nBase) < m_nDCHeightAllRows 
						? 
						pBar->m_rcBar.bottom +max(pBar->m_rcBar.top - pBar->m_rcBar.bottom, m_arInfoBars[pBar->m_nInd].m_nDCShowMinHeightPosBar) 
						: 
						pBar->m_rcBar.top - nHFont
					);
				pdc->SetTextAlign (TA_CENTER | TA_BOTTOM);
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 2 )
			{	//a destra
				nX = pBar->m_rcBar.right +1;
				nH = pBar->m_rcBar.bottom + (pBar->m_rcBar.top - pBar->m_rcBar.bottom - nHFont)/2;
				pdc->SetTextAlign (TA_LEFT | TA_BOTTOM);
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 3 )
			{	//a sinistra
				nX = pBar->m_rcBar.left -1;
				nH = pBar->m_rcBar.bottom + (pBar->m_rcBar.top - pBar->m_rcBar.bottom - nHFont)/2;
				pdc->SetTextAlign (TA_RIGHT | TA_BOTTOM);
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 4 )
			{	//centrale 
				nX = (pBar->m_rcBar.left + pBar->m_rcBar.right) / 2;
				nH = pBar->m_rcBar.bottom + (pBar->m_rcBar.top - pBar->m_rcBar.bottom - nHFont)/2;
				pdc->SetTextAlign (TA_CENTER | TA_BOTTOM);
			}
		}
		else if( m_nTypeChart == Tipo_Orizzontale || m_nTypeChart == Tipo_SogliaOrizzontale || m_nTypeChart == Tipo_OrizzontaleCumulativo )
		{
			if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 3 )
			{	//sopra a sinistra
				nX = pBar->m_rcBar.left+2;
				nH = pBar->m_rcBar.top;
				if ( nHFont < pBar->m_rcBar.Height() )
					nH += ( pBar->m_rcBar.Height() - nHFont ) / 2;

				pdc->SetTextAlign (TA_LEFT | TA_BOTTOM);
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 0 ) 
			{	//centrale a destra
				CSize csH = pdc->GetOutputTextExtent( strH, strH.GetLength() ) ;

				nX = min ( 
							max(pBar->m_rcBar.right +2, pBar->m_rcBar.left + m_arInfoBars[pBar->m_nInd].m_nDCShowMinHeightPosBar), 
							pBar->m_rcBar.left -2 - nBase + m_nDCWidthCols - csH.cx 
						); 
				nH = pBar->m_rcBar.bottom + (pBar->m_rcBar.top - pBar->m_rcBar.bottom - nHFont)/2;
				pdc->SetTextAlign (TA_LEFT | TA_BOTTOM);
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 1 )
			{	//centrale a sinistra
				nX = pBar->m_rcBar.left+2;
				nH = pBar->m_rcBar.bottom + (pBar->m_rcBar.top - pBar->m_rcBar.bottom - nHFont)/2;
				pdc->SetTextAlign (TA_LEFT | TA_BOTTOM);
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 4 )
			{	//centrale 
				nX = (pBar->m_rcBar.left + pBar->m_rcBar.right) / 2;
				nH = pBar->m_rcBar.bottom + (pBar->m_rcBar.top - pBar->m_rcBar.bottom - nHFont)/2;
				pdc->SetTextAlign (TA_CENTER | TA_BOTTOM);
			}
			else if ( m_arInfoBars[pBar->m_nInd].m_nWhereShowHeightPosBar == 2 )
			{	//sotto a sinistra
				nX = pBar->m_rcBar.left+1;
				nH = pBar->m_rcBar.bottom;
				pdc->SetTextAlign (TA_LEFT | TA_TOP);
			}
		}

		pdc->ExtTextOut
			(
				nX, nH,
				0 , NULL, 
				strH, strH .GetLength(), NULL
			);
	}
}

//-------------------------------------------------------------------
void CMultiChartCtrl::DrawBars(CDC *pdc, const CRect& rcBounds)
{
	int nB, nR, nC;
	CBar *pBar;

	pdc->SelectObject(&(m_gdiRes.m_fontValues));

	CRect rcClipRect;
	rcClipRect = rcBounds;	
	pdc->DPtoLP((CPoint *) &rcClipRect, 2);
	rcClipRect.left += m_nDCWidthRowHeader;
	rcClipRect.top -= m_nDCHeightColHeader;

	if ( ! m_bIsSaving ) 
		pdc->IntersectClipRect (rcClipRect);

	for(nR = 0; nR < m_nRows; nR += 1)
	{
		for(nC = 0; nC < m_nCols; nC += 1)
		{
			if 
				( 
					m_nTypeChart == Tipo_Verticale || 
					m_nTypeChart == Tipo_Orizzontale ||
					m_nTypeChart == Tipo_SogliaVerticale || 
					m_nTypeChart == Tipo_SogliaOrizzontale 
				)
			{
				for(nB = 0; nB < m_nBars; nB += 1)
				{
					BOOL bMinBar =  FALSE;
					CBarHeader* pHB = &(m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB]);

					pHB->m_ptIcon.x = pHB->m_ptIcon.y = 0;

					for(int nS = 0; nS < pHB->m_nSegment; nS += 1)
					{
						pBar = &(pHB->m_arBars[nS]);
						DrawOneBar (pdc, pBar);

						if (pBar->m_nScaleValue < 4 && pBar->m_dHeight > 0.0)
							bMinBar = TRUE;
					}
					if (bMinBar && m_bShowIconInfo)
					{
						DrawBmpIconInfo(pdc, nR, nC, nB);
					}
				}

			}
			else if ( m_nTypeChart == Tipo_Torta )
			{
				DrawPies ( pdc, nR, nC );
				DrawHeightBars ( pdc, nR, nC );
			}
		}	
	}

	m_State = Ok;
}
