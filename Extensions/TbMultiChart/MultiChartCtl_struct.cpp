
#include "stdafx.h"

#include "MultiChartCtl.h"
#include "resource.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
//----------------------------------------------------------------

void CMultiChartCtrl::AllocGrid()
{
	if(m_arRows) FreeGrid();
	if(m_nRows == 0 || m_nCols == 0 || m_nBars == 0) return;
	// ----

	CBar *pBar;
	int nR, nC, nB;

	try
	{
		//uno per ogni header
		m_arInfoRowHeader = new CHeaderInfo [m_nRows];
		m_arInfoColHeader = new CHeaderInfo [m_nCols];

		//comuni a tutti i quadranti
		m_arInfoBars = new CBarInfo [m_nBars];

		m_arTotHeightPosBar = new double [m_nBars * m_nCols];

		for (nB = 0; nB < m_nBars; nB++)
		{
			m_arInfoBars[nB].m_dDefaultHeightPosBar = m_dDefaultHeightBars;
			m_arInfoBars[nB].m_dMaxHeightPosBar		= m_dMaxHeightAllBars;
			m_arInfoBars[nB].m_dMinHeightPosBar		= m_dMinHeightAllBars;
			m_arInfoBars[nB].m_strFormatHeightPosBar = m_strFormatHeightBars;
			m_arInfoBars[nB].m_nDCShowMinHeightPosBar = 0;
			m_arInfoBars[nB].m_nSegmentPosBar = 1;
			m_arInfoBars[nB].m_nWhereShowHeightPosBar = m_nWhereShowValueBars;
		}
		for (int nT = 0 ; nT < (m_nCols * m_nBars); nT++)
			m_arTotHeightPosBar [ nT ] = m_dDefaultHeightBars * m_nRows;

		if ( m_nTypeChart == Tipo_SogliaVerticale || m_nTypeChart == Tipo_SogliaOrizzontale )
		{
			m_arLimits = new CLimitBar [ m_nBars ];
			for (int nB = 0; nB < m_nBars; nB++ )
			{
				m_arLimits [nB] . m_nLimits = 0;
				m_arLimits [nB] . m_arValLimits = NULL;
				m_arLimits [nB] . m_arColorLimits = NULL;
			}
		}
		//----

		if (m_nNumLabelRowHeader)
			for(nR = 0; nR < m_nRows; nR++)
			{
				m_arInfoRowHeader[nR].m_arstrTitle.SetSize (m_nNumLabelRowHeader);
//				m_arInfoRowHeader[nR].m_nAlign = -1;
				m_arInfoRowHeader[nR].m_arstrTitle[0].Format (TEXT("%s %d"), m_szRowLabel, nR + 1);
			}

		if (m_nNumLabelColHeader)
			for(nC = 0; nC < m_nCols; nC++)
			{
				m_arInfoColHeader[nC].m_arstrTitle.SetSize (m_nNumLabelColHeader);
//				m_arInfoColHeader[nR].m_nAlign = 0;
				m_arInfoColHeader[nC].m_arstrTitle[0].Format (TEXT("%s %d"), m_szColumnLabel, nC + 1);
			}
		// ----

		m_arRows = new CRowHeader [ m_nRows ];
		if ( m_arRows == NULL )
		{
			MessageBox(TEXT("Manca Memoria"));
			return;
		}

		for(nR = 0; nR < m_nRows; nR++)
		{
			m_arRows[nR].m_arColumns = new CColumnHeader[m_nCols];

			if(m_arRows[nR].m_arColumns == NULL)
			{
				MessageBox(TEXT("Manca Memoria"));
				return;
			}
			
			for(nC = 0; nC < m_nCols; nC++)
			{
				m_arRows[nR].m_arColumns[nC].m_arHeaderBars = new CBarHeader[m_nBars];

				if(m_arRows[nR].m_arColumns[nC].m_arHeaderBars == NULL)
				{
					MessageBox(TEXT("Manca Memoria"));
					return;
				}

				for(int nB = 0; nB < m_nBars; nB++)
				{	
					CBarHeader* pBH = &(m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB]);

					pBH->m_nSegment = m_arInfoBars[nB].m_nSegmentPosBar;
					pBH->m_arBars = new CBar[pBH->m_nSegment];
					for (int nS=0; nS < pBH->m_nSegment; nS++)
					{
						pBar = &(pBH->m_arBars[nS]);

						pBar->m_nInd = nB; 
						pBar->m_nIndSegment = nS;
						pBar->m_crColor = g_crColors [ min(NUM_COLOR_BAR -1, nB) ];
						pBar->m_dHeight = m_arInfoBars[nB].m_dDefaultHeightPosBar;
					}
				}
			}
		}
		m_nAllocatedRows = m_nRows;
		m_nAllocatedCols = m_nCols;
		m_nAllocatedBars = m_nBars;

		m_gdiRes.ClearMapTagBrushes();

		m_State = InvDim;
	}
	catch( CMemoryException* e )
	{
		e->Delete();
		ThrowError ( CTL_E_OUTOFMEMORY, 0 );
	}
}
//------------------------------------------------------------------------------
void CMultiChartCtrl::FreeGrid()
{
	if(m_arRows == NULL) return;
	// ----
	int nR, nC;
	
	//Laber Header
	for(nR = 0; nR < m_nAllocatedRows; nR++)
		m_arInfoRowHeader [ nR ].m_arstrTitle.RemoveAll();
	for(nC = 0; nC < m_nAllocatedCols; nC++)
		m_arInfoColHeader [ nC ].m_arstrTitle.RemoveAll();

	delete [] m_arInfoRowHeader; m_arInfoRowHeader = NULL;
	delete [] m_arInfoColHeader; m_arInfoColHeader = NULL;

	delete [] m_arInfoBars; m_arInfoBars = NULL;

	delete [] m_arTotHeightPosBar; m_arTotHeightPosBar = NULL;
	// ----

	if (m_arLimits != NULL)
	{
		for ( int nB = 0; nB < m_nAllocatedBars; nB++ )
		{
			if (m_arLimits [nB] . m_arValLimits != NULL )
				delete (m_arLimits [nB] . m_arValLimits);
			if (m_arLimits [nB] . m_arColorLimits != NULL ) 
				delete (m_arLimits [nB] . m_arColorLimits);
		}
		delete m_arLimits;
		m_arLimits = NULL;
	}
	// ----
	for(nR = 0; nR < m_nAllocatedRows; nR++)
	{
		for(nC = 0; nC < m_nAllocatedCols; nC++)
		{
			for(int nB = 0; nB < m_nAllocatedBars; nB++)
				delete [] (m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars);

			delete [] (m_arRows[nR].m_arColumns[nC].m_arHeaderBars);
		}
		delete [] (m_arRows[nR].m_arColumns);
	}
	delete [] (m_arRows);
	m_arRows = NULL;

	m_State = InvGrid;
}
//-----------------------------------------------------------------------------------------
void CMultiChartCtrl::ReAllocBars ()
{
	int nR, nC, nB;
	CBarInfo *pBarInfo = new CBarInfo [m_nBars];

	for (nB = 0; nB < m_nAllocatedBars; nB++)
	{
		pBarInfo[nB] = m_arInfoBars[nB];
	}
	for (nB = m_nAllocatedBars; nB < m_nBars; nB++)
	{
		pBarInfo[nB].m_dDefaultHeightPosBar = m_dDefaultHeightBars;
		pBarInfo[nB].m_dMaxHeightPosBar		= m_dMaxHeightAllBars;
		pBarInfo[nB].m_dMinHeightPosBar		= m_dMinHeightAllBars;
		pBarInfo[nB].m_strFormatHeightPosBar = m_strFormatHeightBars;
		pBarInfo[nB].m_nDCShowMinHeightPosBar = 0;
		pBarInfo[nB].m_nSegmentPosBar = 1;
	}
	delete [] m_arInfoBars; 
	m_arInfoBars = pBarInfo;
	// ----

	double* pOldTotHeight = m_arTotHeightPosBar;
	m_arTotHeightPosBar = new double [m_nBars * m_nCols];
	memcpy(m_arTotHeightPosBar, pOldTotHeight, m_nAllocatedBars * m_nCols * sizeof(double));
	delete [] pOldTotHeight;
	for (int nT = m_nAllocatedBars * m_nCols ; nT < (m_nCols * m_nBars); nT++)
		m_arTotHeightPosBar [ nT ] = m_dDefaultHeightBars * m_nRows;
	//----

	for (nR = 0; nR < m_nRows; nR++)
	{
		for (nC = 0; nC < m_nCols; nC++)
		{
			CBarHeader *pOldBarHs = m_arRows[nR].m_arColumns[nC].m_arHeaderBars;
			CBarHeader *pBarHs = m_arRows[nR].m_arColumns[nC].m_arHeaderBars = new CBarHeader[m_nBars];

			for (nB = 0; nB < m_nAllocatedBars; nB++)
			{	
				pBarHs[nB] = pOldBarHs[nB];
			}
			for (nB = m_nAllocatedBars; nB < m_nBars; nB++)
			{	
				pBarHs[nB].m_nSegment = 1;
				pBarHs[nB].m_arBars = new CBar [1];

				pBarHs[nB].m_arBars[0].m_nInd = nB;
				pBarHs[nB].m_arBars[0].m_nIndSegment = 0;

				pBarHs[nB].m_arBars[0].m_crColor = g_crColors[min(NUM_COLOR_BAR-1, nB)];		
				pBarHs[nB].m_arBars[0].m_dHeight = m_arInfoBars[nB].m_dDefaultHeightPosBar;
			}
			delete [] pOldBarHs;
		}
	}
	//----

	m_bUseCustomWidthBars = m_bUseCustomPosBars = FALSE;
	m_nAllocatedBars = m_nBars;
	m_State = InvDraw;

}
//------------------------------------------------------------------------------------------
void CMultiChartCtrl::DoCalcWidthBars ()
{	
	if(	m_bUseCustomPosBars)
		return;

	double dDCSep =0;
	double dRealWidth = m_nDCWidthBars;

	switch(m_nTypeChart)
	{
		case Tipo_Verticale:
		case Tipo_SogliaVerticale:
			
			if( ! m_bUseCustomWidthBars )
			{
				dRealWidth = ( (m_nDCWidthCols / (m_nBars*2.0+m_nBars+1.0)) * 2.0);
				m_nDCWidthBars = (short) dRealWidth;
			}

			if ( (m_nDCWidthBars * m_nBars) > m_nDCWidthCols )
			{
				m_nDCWidthBars = (short) (dRealWidth = (double)m_nDCWidthCols / m_nBars);
			}
			dDCSep = max(m_nDCWidthCols - (dRealWidth * m_nBars), 0.0);
			break;

		case Tipo_Orizzontale:
		case Tipo_SogliaOrizzontale:

			if( ! m_bUseCustomWidthBars )
			{
				dRealWidth = ( (m_nDCHeightAllRows / (m_nBars*2.0+m_nBars+1.0)) * 2.0);
				m_nDCWidthBars = (short) dRealWidth;
			}

			if ( (m_nDCWidthBars * m_nBars) > m_nDCHeightAllRows )
			{
				m_nDCWidthBars = (short) (dRealWidth = (double)m_nDCHeightAllRows / m_nBars);
			}
			dDCSep = max(m_nDCHeightAllRows - (dRealWidth * m_nBars), 0.0);
			break;

		case Tipo_OrizzontaleCumulativo:
		case Tipo_VerticaleCumulativo:
		case Tipo_Torta:
		default:
			m_bUseCustomWidthBars = FALSE;
			m_nDCWidthBars = 1;
			return;
	}

	dDCSep = dDCSep / (m_nBars+1.0);
	for (int nB = 0; nB < m_nBars; nB++)
	{
		if (m_nTypeChart == Tipo_Verticale || m_nTypeChart == Tipo_SogliaVerticale)
		{
			m_arInfoBars[nB].m_nDCXOffsetBar = (short) (dDCSep * (nB+1) + dRealWidth * nB);
			m_arInfoBars[nB].m_nDCYOffsetBar = 0;
		}
		else if (m_nTypeChart == Tipo_Orizzontale || m_nTypeChart == Tipo_SogliaOrizzontale)
		{
			m_arInfoBars[nB].m_nDCYOffsetBar = (short) (dDCSep * (nB+1) + dRealWidth * nB);
			m_arInfoBars[nB].m_nDCXOffsetBar = 0;
		}
		m_arInfoBars[nB].m_nDCWidthBar = m_nDCWidthBars;
	}
}

//---------------------------------------------------------------------------------
void CMultiChartCtrl::DimBars()
{
	int nMustRedim = 0;

l_MustRedim:

	DoCalcWidthBars();

	CRect rcBar;
	int nB, nR, nC;
	int nQx, nQy;
	int nHeightBar, nBaseBar;
	CBar* pBar;

	for (nQy = 0, nR = 0; nR < m_nRows; nQy += m_nDCHeightAllRows, nR += 1)
	{
		for (nQx = m_nDCWidthRowHeader, nC = 0; nC < m_nCols; nQx += m_nDCWidthCols, nC += 1)
		{
			for (nB = 0; nB < m_nBars; nB++)
			{
				CBarHeader* pBH = & ( m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB]);

				for (int nS = 0; nS < pBH->m_nSegment; nS++)
				{
					pBar = &(pBH->m_arBars[nS]);
					//if (nS > 0)	pBar->m_dBase = (pBH->m_arBars[nS-1].m_dHeight+pBH->m_arBars[nS-1].m_dBase);
					
					if ( (pBar->m_dHeight+pBar->m_dBase) > m_arInfoBars[nB].m_dMaxHeightPosBar )
					{
						//TO DO: segnalazione ? colore? flag?
						if (nMustRedim < 0)
						{
							pBar->m_dHeight = m_arInfoBars[nB].m_dMaxHeightPosBar;
							pBar->m_crColor = RGB(0,0,0);
						}
						else
						{
							m_arInfoBars[nB].m_dMaxHeightPosBar = (pBar->m_dHeight + pBar->m_dBase);
							m_dMaxHeightAllBars = max(m_dMaxHeightAllBars, m_arInfoBars[nB].m_dMaxHeightPosBar);

							nMustRedim++;
						}
					}
					else if ( (pBar->m_dHeight+pBar->m_dBase) < m_arInfoBars[nB].m_dMinHeightPosBar )
					{
						//TO DO: segnalazione ? colore? flag?
						pBar->m_dHeight = m_arInfoBars[nB].m_dMinHeightPosBar;
						pBar->m_crColor = RGB(0,0,0);
					}

					if 
						( 
							m_nTypeChart == Tipo_Verticale || 
							m_nTypeChart == Tipo_SogliaVerticale ||
							m_nTypeChart == Tipo_VerticaleCumulativo
						)
					{	
						//ASSERT( (pBar->m_dHeight+pBar->m_dBase) <= m_arInfoBars[nB].m_dMaxHeightPosBar);

						nHeightBar = (int) (pBar->m_dHeight * m_nDCHeightAllRows / m_arInfoBars[nB].m_dMaxHeightPosBar );
						nBaseBar = (int) (pBar->m_dBase * m_nDCHeightAllRows / m_arInfoBars[nB].m_dMaxHeightPosBar );
						
						//ASSERT( (nHeightBar+nBaseBar) <= m_nDCHeightAllRows);

						pBar->m_rcBar.left = nQx + m_arInfoBars[nB].m_nDCXOffsetBar;
						pBar->m_rcBar.right = pBar->m_rcBar.left + m_arInfoBars[nB].m_nDCWidthBar;

						pBar->m_rcBar.bottom = nQy + nBaseBar + m_arInfoBars[nB].m_nDCYOffsetBar;
						pBar->m_rcBar.top = pBar->m_rcBar.bottom + nHeightBar-1;

						if ( m_nTypeChart == Tipo_SogliaVerticale )
							pBar -> m_rcBar.top = pBar -> m_rcBar.bottom + m_nDCHeightAllRows -1;
					}
					else if 
						( 
							m_nTypeChart == Tipo_Orizzontale ||
							m_nTypeChart == Tipo_SogliaOrizzontale ||
							m_nTypeChart == Tipo_OrizzontaleCumulativo
						)
					{
						nHeightBar = (int) (pBar->m_dHeight * m_nDCWidthCols / m_arInfoBars[nB].m_dMaxHeightPosBar );
						nBaseBar = (int) (pBar->m_dBase * m_nDCWidthCols / m_arInfoBars[nB].m_dMaxHeightPosBar );
						
						pBar->m_rcBar.left = nQx + nBaseBar + m_arInfoBars[nB].m_nDCXOffsetBar;
						pBar->m_rcBar.right = pBar->m_rcBar.left + nHeightBar;
						pBar->m_rcBar.bottom = nQy + m_arInfoBars[nB].m_nDCYOffsetBar;
						pBar->m_rcBar.top = pBar->m_rcBar.bottom + m_arInfoBars[nB].m_nDCWidthBar;

						if ( m_nTypeChart == Tipo_SogliaOrizzontale )
							pBar -> m_rcBar.right = pBar -> m_rcBar.left + m_nDCWidthCols -1;
					}
					else if ( m_nTypeChart == Tipo_Torta )
					{
						nHeightBar = (int) (pBar->m_dHeight * 360 / m_arInfoBars[nB].m_dMaxHeightPosBar );
						pBar->m_rcBar.SetRect( 0,0,0,0 );
					}

					pBar->m_nScaleValue = nHeightBar;
				}
			}
		}	
	}
	if (nMustRedim > 0)
	{
		nMustRedim = -1;
		goto l_MustRedim;
	}

	m_State = InvDraw;
}

//-------------------------------------------------------------------------------------
void CMultiChartCtrl::Refresh()
{
	RefreshAndSetPos();
}

//-------------------------------------------------------------------------------------
void CMultiChartCtrl::RefreshAndSetPos(short nR/*=-1*/, short nC/*=-1*/)
{
	if (nR < -1 || nR >= m_nRows || nC < -1 || nC >= m_nCols) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	// ----

	CRect rcBounds;
	if (m_State < InvDimBar) 
	{
		GetClientRect (rcBounds);
		PrepareScrollInfo (rcBounds.right, rcBounds.bottom);
	}
	// ----

	switch (m_State)
	{	// no "break": operazioni volutamente in sequenza
		case InvGrid:
			
			AllocGrid();

		case InvDim:

			m_nScrollPosRow = m_nScrollPosCol = -1;

		case InvDimBar:

			DimBars();

		case InvDraw:
		default:
			m_pSelectedBar = NULL;

			if (m_hWnd)
			{
				if (nC >= 0)
				{
					SCROLLINFO si;
					si.fMask = SIF_POS;
					si.cbSize = sizeof(SCROLLINFO);
/*
					m_nScrollPosX = min
						( 
							nC * m_nDCWidthCols,
							m_nWidth - (rcBounds.right-rcBounds.left) +1
						);
*/	
					m_nScrollPosX = nC * m_nDCWidthCols;

					si.nPos = m_nScrollPosX;

					//m_bHScroll = TRUE;

					SetScrollInfo (SB_HORZ, &si, TRUE);
					//RedrawWindow();
				}
				if(nR >= 0)
				{
					SCROLLINFO si;
					si.fMask = SIF_POS;
					si.cbSize = sizeof(SCROLLINFO);
					
				//	ASSERT(rcBounds.bottom > rcBounds.top);

					m_nScrollPosY = (m_nRows - nR - 1) * m_nDCHeightAllRows;

					si.nPos = m_nScrollPosY;

					//m_bVScroll = TRUE;

					SetScrollInfo (SB_VERT, &si, TRUE);
					//RedrawWindow();
				}			
			}

		InvalidateControl();
			
	}
}
//-------------------------------------------------------------------------------------
BOOL CMultiChartCtrl::GetScrollPosRowAndCol (short& nR, short& nC)
{

	nR = m_nScrollPosRow;
	nC = m_nScrollPosCol;

	return TRUE;
}