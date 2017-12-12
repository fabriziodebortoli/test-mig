
#include "stdafx.h"

#include "MultiChartCtl.h"
#include "resource.h"

#include <atlimage.h>
#include <TBGeneric\WndObjDescription.h>
#include <TBGeneric\generalfunctions.h>

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------
void CMultiChartCtrl::Save (LPCTSTR sFileName)
{
	CString strF ( (LPCTSTR) sFileName );

	DoSave( & strF );
}
//-------------------------------------------------------------------
void CMultiChartCtrl::OnSave ()
{
	DoSave ( NULL );
}
//-------------------------------------------------------------------
void CMultiChartCtrl::DoSave ( CString* strFileName /*= NULL*/ )
{
	CString strFile;

	if ( strFileName == NULL ? true : strFileName -> IsEmpty () )
	{
		UINT nFlags = CFile::modeCreate | CFile::modeWrite | CFile::shareDenyRead | CFile::typeText;
		CFileDialog dlgF ( FALSE, _T(".emf") , _T("*.emf"), nFlags, _T("Enhanced Windows Metafile (*.emf)|*.emf||") );
			//( FALSE, strExt, strAny, WRITEFLAGS, strFilter );
		if ( dlgF.DoModal() != IDOK )
			return ;

		strFile = dlgF.GetPathName();
		if ( strFile.IsEmpty () ) 
			return;
	}
	else
		strFile = * strFileName;
	//----
	CClientDC dcClient(this);

	CRect rcClient;
	GetClientRect(rcClient);

	CDC dcEmf;

	HDC hdc = CreateEnhMetaFile ( dcClient, (LPCTSTR) strFile , NULL,  NULL );

	if ( hdc == 0 ) 
		return ;

	dcEmf.Attach(hdc);
	// ----

	m_bIsSaving = true;

	PrepareScrollInfo (rcClient.right, rcClient.bottom);

	OnDraw ( & dcEmf, rcClient);

	HENHMETAFILE hemf = CloseEnhMetaFile ( hdc );

	m_bIsSaving = false;

	if ( hemf ) 
		DeleteEnhMetaFile( hemf );  
}

//------------------------------------------------------------
void CMultiChartCtrl::SetDCOffsetAndWidthPosBar (short nB, short nXOffset, short nYOffset, short nWidth)
{
	if( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if (!m_bUseCustomPosBars)
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	//----

	m_arInfoBars[nB].m_nDCWidthBar = nWidth;
	m_arInfoBars[nB].m_nDCXOffsetBar = nXOffset;
	m_arInfoBars[nB].m_nDCYOffsetBar = nYOffset;

	m_State = min(m_State, InvDimBar);

	if ( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::GetPosLastClick (short * pnX, short * pnY)
{	
	*pnX = m_nXPosLastClick;
	*pnY = m_nYPosLastClick;
}
//--------------------------------------------------------------------------- 

void CMultiChartCtrl::SetHeightBar(short nR, short nC, short nB, double dH)
{
	SetDimBar(nR, nC, nB, 0.0, dH);
}
//-------------------------------------------------------------------
void CMultiChartCtrl::SetDimBar(short nR, short nC, short nB, double dBase, double dH)
{
	SetHeightBarSegment(nR, nC, nB, 0, dBase, dH);
}
//-------------------------------------------------------------------
void CMultiChartCtrl::SetHeightBarSegment(short nR, short nC, short nB, short nS, double dBase, double dH)
{
	if ( nR < 0 || nR >= m_nRows || nC < 0 || nC >= m_nCols || nB < 0 || nB >= m_nBars || nS < 0) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if ( dH < 0 || dBase < 0 || (dH * dBase) < 0 ) 
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if (m_State == InvGrid) AllocGrid();

	if (nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment)
	{
		SetBarSegments(nR,nC,nB,nS+1);	//espando struttura
	}
	//----
	double dPrecHeight = (
							nS > 0 ? 
							m_arRows [nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS-1].m_dHeight + 
							m_arRows [nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS-1].m_dBase : 
							0.0
						);
	if ( (dH * dBase) >= 0 && dPrecHeight >= 0.0 && ((dBase + dH + dPrecHeight) > m_arInfoBars[nB].m_dMaxHeightPosBar) ) 
	{ 
		m_State = min(m_State, InvDimBar);

		m_arInfoBars[nB].m_dMaxHeightPosBar = dBase + dH + dPrecHeight;
		if ( m_dMaxHeightAllBars < m_arInfoBars[nB].m_dMaxHeightPosBar)
		{
			m_dMaxHeightAllBars = m_arInfoBars[nB].m_dMaxHeightPosBar;
			for(int i = 0; i < m_nBars; i++)
			{
				m_arInfoBars[i].m_dMaxHeightPosBar = m_dMaxHeightAllBars;
				m_arInfoBars[i].m_dDefaultHeightPosBar = 0;
			}
		}
	}
	else if ( (dH * dBase) <= 0 && dPrecHeight <= 0.0 && ((dBase + dH + dPrecHeight) < m_arInfoBars[nB].m_dMinHeightPosBar) ) 
	{ 
		m_State = min(m_State, InvDimBar);

		m_arInfoBars[nB].m_dMinHeightPosBar = dBase + dH + dPrecHeight;
		if ( m_dMinHeightAllBars > m_arInfoBars[nB].m_dMinHeightPosBar)
		{
			m_dMinHeightAllBars = m_arInfoBars[nB].m_dMinHeightPosBar;
			for(int i = 0; i < m_nBars; i++)
			{
				m_arInfoBars[i].m_dMinHeightPosBar = m_dMinHeightAllBars;
				m_arInfoBars[i].m_dDefaultHeightPosBar = 0;
			}
		}
	}
	// ----

	CBar* pBar = &(m_arRows [nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS]);
	CBar* pNextBar = NULL;
	double dPrecBaseNextSegment = 0.0;

	m_arTotHeightPosBar [ nC * m_nBars + nB ] += dH - pBar -> m_dHeight;

	if (nS < (m_arRows [nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment-1))
	{
		pNextBar = &(m_arRows [nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS+1]);
		//preservo eventuale offset base propria del segmento successivo
		dPrecBaseNextSegment = pNextBar->m_dBase - pBar->m_dHeight - pBar->m_dBase;

	}
	//ATTENZIONE che le righe successive non devono essere invertite
	pBar->m_dHeight = dH;
	pBar->m_dBase = dBase + dPrecHeight;

	if (pNextBar)	//sposto i segmenti successivi
		SetHeightBarSegment(nR,nC,nB, nS+1, max(0, dPrecBaseNextSegment), pNextBar->m_dHeight);
	//----

	if (m_State <= InvDimBar) 
		return;
	// ----
	
	int nHeightBar, nBaseBar;
	
	if ( m_nTypeChart == Tipo_Verticale || m_nTypeChart == Tipo_VerticaleCumulativo )
	{
		nHeightBar = (int) (pBar->m_dHeight * m_nDCHeightAllRows / m_arInfoBars[nB].m_dMaxHeightPosBar );
		nBaseBar = (int) (pBar->m_dBase * m_nDCHeightAllRows / m_arInfoBars[nB].m_dMaxHeightPosBar );

		pBar->m_rcBar.bottom = m_nDCHeightAllRows * nR + nBaseBar+ m_arInfoBars[nB].m_nDCYOffsetBar;
		pBar->m_rcBar.top = pBar->m_rcBar.bottom + nHeightBar -1;
	}
	else if ( m_nTypeChart == Tipo_Orizzontale || m_nTypeChart == Tipo_OrizzontaleCumulativo )
	{
		nHeightBar = (int) (pBar->m_dHeight * m_nDCWidthCols / m_arInfoBars[nB].m_dMaxHeightPosBar);		
		nBaseBar = (int) (pBar->m_dBase * m_nDCWidthCols / m_arInfoBars[nB].m_dMaxHeightPosBar);

		pBar->m_rcBar.left = m_nDCWidthRowHeader + m_nDCWidthCols * nC + nBaseBar;
		pBar->m_rcBar.right = pBar->m_rcBar.left + nHeightBar -1;
	}
	else if ( m_nTypeChart == Tipo_SogliaVerticale )
	{
		nHeightBar = (int) (pBar->m_dHeight * m_nDCHeightAllRows / m_arInfoBars[nB].m_dMaxHeightPosBar);
	}
	else if ( m_nTypeChart == Tipo_SogliaOrizzontale )
	{
		nHeightBar = (int) (pBar->m_dHeight * m_nDCWidthCols / m_arInfoBars[nB].m_dMaxHeightPosBar);		
	}
	else if ( m_nTypeChart == Tipo_Torta )
	{
		nHeightBar = (int) (pBar->m_dHeight * 360.0 / m_arInfoBars[nB].m_dMaxHeightPosBar);		
	}

	pBar-> m_nScaleValue = nHeightBar;
	// ----

	m_State = min(m_State, InvDraw);
}

//---------------------------------------------------------------------------------
void CMultiChartCtrl::SetColorBar(short nR, short nC, short nB, COLORREF crColor)
{
	SetColorBarSegment(nR,nC,nB,0, crColor);
}

void CMultiChartCtrl::SetColorBarSegment(short nR, short nC, short nB, short nS, COLORREF crColor)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars ||
			nS < 0
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid ();

	if (nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment)
	{
		//ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		//return;
		SetBarSegments(nR,nC,nB,nS+1);	//espando struttura
	}
	// ----

	m_arRows[nR].m_arColumns [nC].m_arHeaderBars[nB].m_arBars[nS].m_crColor = crColor;

	m_State = min(m_State, InvDraw);
}

//------------------------------------------------------------------------------------
COLORREF CMultiChartCtrl::GetColorBarSegment(short nR, short nC, short nB, short nS)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars ||
			nS < 0 ||
			m_State == InvGrid ||
			nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return 0;
	}
	// ----

	return m_arRows[nR].m_arColumns [nC].m_arHeaderBars[nB].m_arBars[nS].m_crColor;
}
//----------------------------------------------------------------------------------------
void CMultiChartCtrl::SetHatchBrushBarSegment(short nR, short nC, short nB, short nS, short nHatchBrush)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars ||
			nS < 0
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid ();

	if (nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment)
	{
		//ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		//return;
		SetBarSegments(nR,nC,nB,nS+1);	//espando struttura
	}
	// ----

	m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS].m_nHatchBrush = nHatchBrush;

	m_State = min(m_State, InvDraw);
}

//------------------------------------------------------------------------------------
short CMultiChartCtrl::GetHatchBrushBarSegment(short nR, short nC, short nB, short nS)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars ||
			nS < 0 ||
			m_State == InvGrid ||
			nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return 0;
	}
	// ----

	return m_arRows[nR].m_arColumns [nC].m_arHeaderBars[nB].m_arBars[nS].m_nHatchBrush;
}
//-------------------------------------------------------------------
void CMultiChartCtrl::SetColorAllBars (short nB, COLORREF crColor)
{
	if( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	// ----

	int nR, nC;

	for(nR = 0; nR < m_nRows; nR += 1)
	{
		for(nC = 0; nC < m_nCols; nC += 1)
		{
			m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[0].m_crColor = crColor;				
		}	
	}
   
	m_State = min(m_State, InvDraw);
}

//------------------------------------------------------------------
/*
void CMultiChartCtrl::SetColorPosBar (short nB, COLORREF crColor)
{
	if( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	// ----

 	m_arColorPosBar [nB] = crColor;
  
	m_State = min(m_State, InvDraw);
}
*/
//------------------------------------------------------------------

void CMultiChartCtrl::SetMaxHeightPosBar (short nB, double nH)
{
	if( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if (m_State == InvGrid) AllocGrid();
	// ----

	m_arInfoBars[nB].m_dMaxHeightPosBar = nH;

	if (nH < m_arInfoBars[nB].m_dDefaultHeightPosBar)
		m_arInfoBars[nB].m_dDefaultHeightPosBar = 0;

	if (m_dMaxHeightAllBars < m_arInfoBars[nB].m_dMaxHeightPosBar)
	{
		m_dMaxHeightAllBars = m_arInfoBars[nB].m_dMaxHeightPosBar;
		for (int i = 0; i < m_nBars; i++)
		{
			m_arInfoBars[i].m_dMaxHeightPosBar = m_dMaxHeightAllBars;
			m_arInfoBars[i].m_dDefaultHeightPosBar = 0;
		}
	}

	m_State = min(m_State, InvDimBar);
}
//------------------------------------------------------------------
void CMultiChartCtrl::SetMinHeightPosBar (short nB, double nH)
{
	if( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if (m_State == InvGrid) AllocGrid();
	// ----

	m_arInfoBars[nB].m_dMinHeightPosBar = nH;

	if( nH > m_arInfoBars[nB].m_dDefaultHeightPosBar )
		m_arInfoBars[nB].m_dDefaultHeightPosBar = 0;

	if ( m_dMinHeightAllBars > m_arInfoBars[nB].m_dMinHeightPosBar)
	{
		m_dMinHeightAllBars = m_arInfoBars[nB].m_dMinHeightPosBar;
		for (int i = 0; i < m_nBars; i++)
		{
			m_arInfoBars[i].m_dMinHeightPosBar = m_dMinHeightAllBars;
			m_arInfoBars[i].m_dDefaultHeightPosBar = 0;
		}
	}
	m_State = min(m_State, InvDimBar);
}
//------------------------------------------------------------------

void CMultiChartCtrl::SetDefaultHeightPosBar (short nB, double nH)
{
	if( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	//----

	if ( nH > m_arInfoBars[nB].m_dMaxHeightPosBar )
		m_arInfoBars[nB].m_dDefaultHeightPosBar = 0;
	else if ( nH < m_arInfoBars[nB].m_dMinHeightPosBar )
		m_arInfoBars[nB].m_dDefaultHeightPosBar = 0;
	else
		m_arInfoBars[nB].m_dDefaultHeightPosBar = nH;

	m_State = min(m_State, InvDimBar);
}

//------------------------------------------------------------------

void CMultiChartCtrl::SetDCShowMinHeightPosBar (short nB, short nH)
{
	if( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if ( nH > m_nDCHeightAllRows || nH < 0)
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	//----

	m_arInfoBars[nB].m_nDCShowMinHeightPosBar = nH;

	m_State = min(m_State, InvDraw);
}
// --------------------------------------------------------------------------

void CMultiChartCtrl::SetFormatHeightPosBar	(short nB, LPCTSTR sFormat)
{
	if( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	//----

	m_arInfoBars[nB].m_strFormatHeightPosBar = (LPCTSTR) sFormat;

	m_State = min(m_State, InvDraw);

}

// ---------------------------------------------------------------------------

void CMultiChartCtrl::SetTitlePosBar (short nB, LPCTSTR sTitle)
{
	if( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	//----

	m_arInfoBars[nB].m_strTitlePosBar = (LPCTSTR) sTitle;

	m_State = min(m_State, InvDraw);

}
//---------------------------------------------------------------------------
void CMultiChartCtrl::SetWhereShowHeightPosBar (short nB, short n)
{
	if ( nB < 0 || nB >= m_nBars ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if ( n < -1 || n > 4 ) 
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	//----

	m_arInfoBars[nB].m_nWhereShowHeightPosBar = n;

	m_State = min(m_State, InvDraw);
}
// --------------------------------------------------------------------------
short CMultiChartCtrl::GetWhereShowHeightPosBar (short nB)
{
	if ( nB < 0 || nB >= m_nBars || m_State == InvGrid) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return -1;
	}
	//----
	return m_arInfoBars[nB].m_nWhereShowHeightPosBar;
}

// --------------------------------------------------------------------------

void CMultiChartCtrl::SetNumLimitPosBar (short nB, short nL)
{
	if( nB < 0 || nB >= m_nBars || nL < 0 ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if( m_nTypeChart != Tipo_SogliaVerticale && m_nTypeChart != Tipo_SogliaOrizzontale ) 
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}
	
	if(m_State == InvGrid) AllocGrid();
	// ----

	if ( m_arLimits [nB] . m_nLimits > 0) 
	{
		delete [] (m_arLimits [nB] . m_arValLimits);
		delete [] (m_arLimits [nB] . m_arColorLimits);
	}
	m_arLimits [nB] . m_nLimits = nL;

	if ( nL == 0 )
	{
		m_arLimits [nB] . m_arValLimits = NULL;
		m_arLimits [nB] . m_arColorLimits = NULL;
	}
	else
	{
		m_arLimits [nB] . m_arValLimits = new double [ nL ];
		m_arLimits [nB] . m_arColorLimits = new COLORREF [ nL ];
		for ( int i=0; i < nL; i++ )
		{
			m_arLimits [nB] . m_arValLimits [i] = i;
			COLORREF crCol;
			switch(i)
			{
			case 0:
					crCol = RGB (  0, 255,   0);  //green
					break;
			case 1:
					crCol = RGB ( 255, 255,   0); //yellow
					break;
			case 2:
					crCol = RGB ( 255,   0,   0); //red
					break;
			default:
					crCol = RGB ( 0, 0, 0 );
			}
			m_arLimits [nB] . m_arColorLimits [i] = crCol;
		}
	}
	m_State = min(m_State, InvDraw);
}
// -----------------------------------------------------------

void CMultiChartCtrl::SetLimitPosBar (short nB, short nL, double dVal, COLORREF crColor )
{
	if ( nB < 0 || nB >= m_nBars )
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if( m_nTypeChart != Tipo_SogliaVerticale && m_nTypeChart != Tipo_SogliaOrizzontale ) 
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}
	if( nL < 0 || nL >= m_arLimits [nB].m_nLimits || m_arLimits [nB].m_nLimits <= 0 ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if( nL > 0 && m_arLimits [nB] . m_arValLimits [nL-1] >= dVal ) 
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	// ----

	m_arLimits [nB] . m_arValLimits [nL] = dVal;
	m_arLimits [nB] . m_arColorLimits [nL] = crColor;

	// ----
	m_State = min(m_State, InvDraw);
}
//---------------------------------------------------------------------------------
void CMultiChartCtrl::GetPropertiesBar(short nR, short nC, short nB, COLORREF* pcrColor, double* pdBase, double* pdDim)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars || 
			m_State == InvGrid
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	*pcrColor = m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[0].m_crColor ;
	*pdBase = m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[0].m_dBase ;
	*pdDim = m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[0].m_dHeight ;
}

//-------------------------------------------------------------------------------------
long CMultiChartCtrl::GetItemData(short nR, short nC, short nB)
{
	return GetItemDataBarSegment(nR,nC,nB,0);
}
//-------------------------------------------------------------------------------------
long CMultiChartCtrl::GetItemDataBarSegment(short nR, short nC, short nB, short nS)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars ||
			nS < 0 ||
			m_State == InvGrid ||
			nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return 0;
	}
	//----

	return m_arRows [nR] .m_arColumns [nC] .m_arHeaderBars[nB].m_arBars[nS].m_dwItemData;
}

//-------------------------------------------------------------------------------------
short CMultiChartCtrl::GetBarSegments(short nR, short nC, short nB)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars ||
			m_State == InvGrid
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return 0;
	}
	//----

	return m_arRows [nR] .m_arColumns [nC] .m_arHeaderBars[nB].m_nSegment;

}
//-------------------------------------------------------------------------------------
void CMultiChartCtrl::SetBarSegments(short nR, short nC, short nB, short nSegments)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars 
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	//----
	if(m_State == InvGrid) AllocGrid();

	if (nSegments > m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment)
	{
		CBar* pOldBars = m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars;
		CBar* pNewBars = new CBar[nSegments];
		m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars = pNewBars;

		int i = 0;
		for(i = 0; i < m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment; i++)
		{
			pNewBars[i] = pOldBars[i];
		}
		for(;i < nSegments; i++)
		{
			pNewBars[i].m_nInd = nB;
			pNewBars[i].m_nIndSegment = i;
			if (i < NUM_COLOR_BAR)
				pNewBars[i].m_crColor = g_crColors[i];
		}

		delete [] pOldBars;
	}

	m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment = nSegments;

	m_State = min(m_State, InvDimBar);
}

//-------------------------------------------------------------------------------------
void CMultiChartCtrl::SetItemData(short nR, short nC, short nB, long dwItemData)
{
	SetItemDataBarSegment(nR,nC,nB,0, dwItemData);
}
//-------------------------------------------------------------------------------------
void CMultiChartCtrl::SetItemDataBarSegment(short nR, short nC, short nB, short nS, long dwItemData)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars ||
			nS <  0
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();

	if (nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment)
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	//----

	m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS].m_dwItemData = (unsigned) dwItemData;
}

// --------------------------------------------------------------------------------------
void CMultiChartCtrl::SetTextBar (short nR,short nC,short nB, LPCTSTR sText)
{
	USES_CONVERSION;

	SetTextBarSegment(nR,nC,nB,0, sText);
}

void CMultiChartCtrl::SetTextBarSegment (short nR, short nC, short nB, short nS, LPCTSTR sText)
{
	USES_CONVERSION;

	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars || 
			nS < 0 
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();

	if (nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment)
	{
		//ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		//return;
		SetBarSegments(nR,nC,nB,nS+1);	//espando struttura
	}
	//----

	m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS].m_strText = sText;


	m_State = min(m_State, InvDraw);
}

// --------------------------------------------------------------------------------------

LPCTSTR CMultiChartCtrl::GetTextBarSegment (short nR, short nC, short nB, short nS)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars || 
			nS < 0 ||
			m_State == InvGrid ||
			nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return NULL;
	}
	//----

	return m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS].m_strText;
}

// --------------------------------------------------------------------------------------

void CMultiChartCtrl::SetToolTipTextBarSegment (short nR, short nC, short nB, short nS, LPCTSTR sText)
{
	USES_CONVERSION;

	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars || 
			nS < 0 
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();

	if (nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment)
	{
		//ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		//return;
		SetBarSegments(nR,nC,nB,nS+1);	//espando struttura
	}
	//----

	m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS].m_strToolTipText = sText;

	m_State = min(m_State, InvDraw);
}

// --------------------------------------------------------------------------------------

LPCTSTR CMultiChartCtrl::GetToolTipTextBarSegment (short nR, short nC, short nB, short nS)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars || 
			nS < 0 ||
			m_State == InvGrid ||
			nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return NULL;
	}
	//----

	return m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS].m_strToolTipText;
}

// --------------------------------------------------------------------------------------

LPCTSTR CMultiChartCtrl::GetTagColorBarSegment (short nR, short nC, short nB, short nS)
{
	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars || 
			nS < 0 ||
			m_State == InvGrid ||
			nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return NULL;
	}
	//----

	return NULL;
}

//------------------------------------------------------------------------------------
void CMultiChartCtrl::SetTagColorBarSegment (short nR, short nC, short nB, short nS, LPCTSTR sTagColor)
{
	USES_CONVERSION;

	if
		( 
			nR < 0 || nR >= m_nRows ||
			nC < 0 || nC >= m_nCols ||
			nB < 0 || nB >= m_nBars || 
			nS < 0 
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();

	if (nS >= m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_nSegment)
	{
		//ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		//return;
		SetBarSegments(nR,nC,nB,nS+1);	//espando struttura
	}
	//----

	COLORREF crColor = 0;
	short nHatch = 0;

	if ( m_gdiRes.LookUpTagBrush(TRUE, sTagColor, crColor, nHatch) )
	{
		m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS].m_crColor = crColor;
		m_arRows[nR].m_arColumns[nC].m_arHeaderBars[nB].m_arBars[nS].m_nHatchBrush = nHatch;
	}
	else
	{
		ASSERT(FALSE);
	}

	m_State = min(m_State, InvDraw);
}

//------------------------------------------------------------------------------------
void CMultiChartCtrl::MoveBarSegment	(short nSrcR, short nSrcC, short nSrcB, short nSrcS, short nDstR, short nDstC, short nDstB, short nDstS)
{
	if
		( 
			nSrcR < 0 || nSrcR >= m_nRows ||
			nSrcC < 0 || nSrcC >= m_nCols ||
			nSrcB < 0 || nSrcB >= m_nBars || 
			nSrcS < 0  ||
			nDstR < 0 || nDstR >= m_nRows ||
			nDstC < 0 || nDstC >= m_nCols ||
			nDstB < 0 || nDstB >= m_nBars || 
			nDstS < 0 ||
			m_State == InvGrid
		) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	//----

	CBarHeader* pSrcBH = &(m_arRows[nSrcR].m_arColumns[nSrcC].m_arHeaderBars[nSrcB]);
	CBar* pSrc = &(pSrcBH->m_arBars[nSrcS]);
	
	SetBarSegments(nDstR, nDstC, nDstB, GetBarSegments(nDstR, nDstC, nDstB) + 1);
	CBarHeader* pDstBH = &(m_arRows[nDstR].m_arColumns[nDstC].m_arHeaderBars[nDstB]);
	CBar* pDst =  &(pDstBH->m_arBars[pDstBH->m_nSegment -1]);

	*pDst = *pSrc; 
	pDst->m_nInd = nDstB; pDst->m_nIndSegment = pDstBH->m_nSegment -1; 
	
	//calcolo base 
	double dSrcBase = 0.0;
	if (pSrc->m_nIndSegment)
		dSrcBase =  pSrcBH->m_arBars[pSrc->m_nIndSegment-1].m_dHeight + pSrcBH->m_arBars[pSrc->m_nIndSegment-1].m_dBase;
	if (pDst->m_nIndSegment)
		pDst->m_dBase = dSrcBase + pDstBH->m_arBars[pDst->m_nIndSegment-1].m_dHeight + pDstBH->m_arBars[pDst->m_nIndSegment-1].m_dBase;

	pSrc->Empty();

	m_State = min(m_State, InvDimBar);
}

//-------------------------------------------------------------------------
void CMultiChartCtrl::SetSoglia(short nSoglia, COLORREF crColor, double dHeight )
{
	if( nSoglia < 0 || nSoglia >= m_nNumeroSoglie ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	//----
	m_arSoglie [nSoglia].m_crColor = crColor;
	m_arSoglie [nSoglia].m_dHeight = dHeight;

	m_State = min(m_State, InvDraw);
}

//-------------------------------------------------------------------------
void CMultiChartCtrl::OnNumeroSoglieChanged ()
{
	if( m_nNumeroSoglie < 0 ) 
	{
		m_nNumeroSoglie = 0;
		if (m_arSoglie)
			delete [] m_arSoglie;
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	//----
	if (m_arSoglie)
		delete [] m_arSoglie;
	
	m_arSoglie = new CSoglie [m_nNumeroSoglie];

	m_State = min(m_State, InvDraw);
}

