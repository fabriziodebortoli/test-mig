
#include "stdafx.h"

#include <TbGenlib\Tfxdatatip.h>
#include "MultiChartCtl.h"
#include "resource.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// Fire Event
/////////////////////////////////////////////////////////////////////////////
void CMultiChartCtrl::FireRowHeaderLButtonDown(short nR)
{ 
	m_MCEventArguments.m_nR = nR ;
	
    GetParent()->SendMessage(UM_Multichart_RowHeaderLeftClick,0,(LPARAM)&m_MCEventArguments);
	OnRowHeaderLButtonDown(0, (LPARAM)&m_MCEventArguments);
}
//-------------------------------------------------------------------
void CMultiChartCtrl::FireColHeaderLButtonDown(short nC)
{ 
	m_MCEventArguments.m_nC = nC ; 
	
	GetParent()->SendMessage(UM_Multichart_ColHeaderLeftClick,0,(LPARAM)&m_MCEventArguments);
	OnColHeaderLButtonDown(0, (LPARAM)&m_MCEventArguments);
}
//-------------------------------------------------------------------
void CMultiChartCtrl::FireCornerHeaderLButtonDown()
{ 
	GetParent()->SendMessage(UM_Multichart_CornerHeaderLeftClick);
	OnCornerHeaderLButtonDown(0, 0);
}
//--------------------------------------------------------------
void CMultiChartCtrl::FireRowHeaderRButtonDown(short nR)
{ 
	m_MCEventArguments.m_nR = nR ;

	GetParent()->SendMessage(UM_Multichart_RowHeaderRightClick,0,(LPARAM)&m_MCEventArguments);
	OnRowHeaderRButtonDown(0, (LPARAM)&m_MCEventArguments);
}
//-------------------------------------------------------------------
void CMultiChartCtrl::FireColHeaderRButtonDown(short nC)
{	
	m_MCEventArguments.m_nC = nC ; 

	GetParent()->SendMessage(UM_Multichart_ColHeaderRightClick,0,(LPARAM)&m_MCEventArguments);
	OnColHeaderRButtonDown(0, (LPARAM)&m_MCEventArguments);
}
//-------------------------------------------------------------------
void CMultiChartCtrl::FireCornerHeaderRButtonDown()
{ 
	GetParent()->SendMessage(UM_Multichart_CornerHeaderRightClick);
}
//-------------------------------------------------------------------
void CMultiChartCtrl::FireBarLButtonDown( short nR, short nC, short nB)
{ 	
	m_MCEventArguments.m_nR = nR;
	m_MCEventArguments.m_nC = nC;
	m_MCEventArguments.m_nB = nB;

	GetParent()->SendMessage(UM_Multichart_BarLeftClick, 0, (LPARAM)&m_MCEventArguments);
	OnBarLButtonDown(0, (LPARAM)&m_MCEventArguments);
}
//-------------------------------------------------------------------
void CMultiChartCtrl::FireBarRButtonDown(short nR, short nC, short nB)
{ 
	m_MCEventArguments.m_nR = nR;
	m_MCEventArguments.m_nC = nC;
	m_MCEventArguments.m_nB = nB;

	GetParent()->SendMessage(UM_Multichart_BarRightClick, 0, (LPARAM)&m_MCEventArguments);
	OnBarRButtonDown(0, (LPARAM)&m_MCEventArguments);
}
//-------------------------------------------------------------------
void CMultiChartCtrl::FireBarIconRButtonDown(short nR, short nC, short nB)
{ 
	m_MCEventArguments.m_nR = nR;
	m_MCEventArguments.m_nC = nC;
	m_MCEventArguments.m_nB = nB;

	GetParent()->SendMessage(UM_Multichart_BarIconRightClick, 0, (LPARAM)&m_MCEventArguments);
	OnIconRButtonDown(0, (LPARAM)&m_MCEventArguments);
}
//-----------------------------------------------------------------------------
void CMultiChartCtrl::FireBarSegmentLButtonDown( short nR, short nC, short nB, short nS)
{ 
	m_MCEventArguments.m_nR = nR;
	m_MCEventArguments.m_nC = nC;
	m_MCEventArguments.m_nB = nB;
	m_MCEventArguments.m_nS = nS;

	GetParent()->SendMessage(UM_Multichart_BarSegmentLeftClick, 0, (LPARAM)&m_MCEventArguments);
	OnSegmentLButtonDown(0, (LPARAM)&m_MCEventArguments);

}
//-----------------------------------------------------------------------------
void CMultiChartCtrl::FireBarSegmentRButtonDown(short nR, short nC, short nB, short nS)
{ 

	m_MCEventArguments.m_nR = nR;
	m_MCEventArguments.m_nC = nC;
	m_MCEventArguments.m_nB = nB;
	m_MCEventArguments.m_nS = nS;

	GetParent()->SendMessage(UM_Multichart_BarSegmentRightClick, 0, (LPARAM)&m_MCEventArguments);
	OnSegmentRButtonDown(0, (LPARAM)&m_MCEventArguments);
}

//-------------------------------------------------------------------
void CMultiChartCtrl::FireBarSegmentMoved( short nR, short nC, short nB, short nS, short nDstR, short nDstC, short nDstB, short nDstS)
{ 
	m_MCEventArguments.m_nR    = nR;
	m_MCEventArguments.m_nC    = nC;
	m_MCEventArguments.m_nB    = nB;
	m_MCEventArguments.m_nS    = nS;
	m_MCEventArguments.m_nDstR = nDstR;
	m_MCEventArguments.m_nDstC = nDstC;
	m_MCEventArguments.m_nDstB = nDstB;
	m_MCEventArguments.m_nDstS = nDstS;


	GetParent()->SendMessage(UM_Multichart_BarSegmentMoved, 0, (LPARAM)&m_MCEventArguments);
	OnSegmentMoved(0, (LPARAM)&m_MCEventArguments);
}

/////////////////////////////////////////////////////////////////////////////
// CMultiChartCtrl::DoPropExchange - Persistence support
/*
void CMultiChartCtrl::DoPropExchange(CPropExchange* pPX)
{
	ExchangeVersion(pPX, MAKELONG(_wVerMinor, _wVerMajor));
	COleControl::DoPropExchange(pPX);
	// TODO: Call PX_ functions for each persistent custom property.

	try
	{
		PX_BOOL(pPX, _T("ShowGrid"),			m_bShowGrid,			TRUE );

		PX_Short(pPX, _T("Rows"),				m_nRows,				3 );
		PX_Short(pPX, _T("Cols"),				m_nCols,				4 );
		PX_Short(pPX, _T("Bars"),				m_nBars,				3 );

		PX_Short(pPX, _T("DCHeightAllRows"),	m_nDCHeightAllRows,		100 );
		PX_Short(pPX, _T("DCWidthCols"),		m_nDCWidthCols,			100 );
		PX_Short(pPX, _T("DCHeightColHeader"),	m_nDCHeightColHeader,	20 );
		PX_Short(pPX, _T("DCWidthRowHeader"),	m_nDCWidthRowHeader,	100 );

		PX_Double(pPX, _T("MaxHeightAllBars"),	m_dMaxHeightAllBars,	100.0 );
		PX_Double(pPX, _T("StepHeightGrid"),	m_dStepHeightGrid,		10.0 );
		PX_Double(pPX, _T("DefaultHeightBars"),	m_dDefaultHeightBars,	20.0 );

		PX_Short(pPX, _T("NumLabelRowHeader"),	m_nNumLabelRowHeader,	1 );
		PX_Short(pPX, _T("NumLabelColHeader"),	m_nNumLabelColHeader,	1 );

		PX_BOOL(pPX,	_T("ShowHeightBars"),	m_bShowHeightBars,		TRUE );
		PX_String(pPX,	_T("FormatHeightBars"),	m_strFormatHeightBars,	_T("%7.2f")	 );
		PX_BOOL(pPX,	_T("ShowTitleBars"),	m_bShowTitleBars,		FALSE );
		PX_BOOL(pPX,	_T("ShowTotBars"),		m_bShowTotBars,			FALSE );
		PX_Short(pPX,	_T("TypeChart"),		m_nTypeChart,			0 );
		PX_String(pPX,	_T("TitleBars"),		m_strTitleBars,			_T("unused") );

		//---- > 24/11/98
		PX_BOOL(pPX,	_T("ShowTrueValue"),	m_bShowTrueValue,		TRUE );
		//---- > 04/12/98
		PX_BOOL(pPX,	_T("ShowToolTip"),		m_bShowToolTip,			FALSE );
		PX_Short(pPX,	_T("DCHeightFont"),		m_nDCHFontValues,				6 );
		//---- > 10/12/98
		PX_Short(pPX,	_T("WhereShowValueBars"),	m_nWhereShowValueBars,	0 );
		PX_BOOL(pPX,	_T("ShowPercentValues"),	m_bShowPercentValues, FALSE );
		//---- > 07/07/99
		PX_BOOL(pPX,	_T("UseCustomWidthBars"),	m_bUseCustomWidthBars, FALSE );
		PX_Short(pPX,	_T("DCCustomWidthBars"),	m_nDCWidthBars,  ( (m_nDCWidthCols / (m_nBars*2+m_nBars)) * 2) );
		//---- > 15/09/99
		PX_BOOL(pPX,	_T("HideBoxBars"),			m_bHideBoxBars, FALSE );
		PX_Color(pPX,	_T("GridColor"),			m_crColorGrid, RGB(180,180,180) );
		PX_Color(pPX,	_T("ZoneSeparatorColor"),	m_crColorZoneSeparator, RGB(0,0,0) );
		//---- > 16/07/2000
		PX_Double(pPX, _T("MinHeightAllBars"),	m_dMinHeightAllBars,	0 );
		PX_BOOL(pPX,	_T("UseCustomPosBars"),	m_bUseCustomPosBars, FALSE );
		//---- > 15/10/2000
		PX_Short(pPX,	_T("ShowGridScale"),	m_nShowGridScale, 0 );
		//---- > 12/15/2000 1.6.9
		PX_BOOL(pPX,	_T("ShowIconInfo"),		m_bShowIconInfo, FALSE );
		PX_Short(pPX,	_T("DCHeightFontHeaders"),		m_nDCHFontHeaders,				8 );
		//---- > 04/02/2002 ???
		PX_Short(pPX,	_T("NumeroSoglie"),		m_nNumeroSoglie,				0 );

	}
	catch(CArchiveException* e)
	{
		e->Delete();
		// evito exception bloccante al caricamento di risorse dialog
		// che contengono il controllo inserito a design-time avente un numero inferiore
		// di proprieta' persistenti
		m_bErrorOnLoadResProp = true ;
	
		m_nDCHFontValues = 6;
		m_nDCHFontHeaders = 8;
	}
	catch(...)
	{
		// evito exception bloccante al caricamento di risorse dialog
		// che contengono il controllo inserito a design-time avente un numero inferiore
		// di proprieta' persistenti
		m_bErrorOnLoadResProp = true ;
	}
}

/////////////////////////////////////////////////////////////////////////////
// CMultiChartCtrl::OnResetState - Reset control to default state

void CMultiChartCtrl::OnResetState()
{
	COleControl::OnResetState();  // Resets defaults found in DoPropExchange
	// TODO: Reset any other control state here.
		SetBackColor( RGB(255,255,255) );
}
*/
/////////////////////////////////////////////////////////////////////////////
// CMultiChartCtrl::AboutBox - Display an "About" box to the user

void CMultiChartCtrl::OnAboutBox()
{
	CDialog dlgAbout(IDD_ABOUTBOX_MULTICHART);
	dlgAbout.DoModal();
}


///////////////////////////////////////////////////////////////////////////
// modify property

void CMultiChartCtrl::OnRowsChanged()
{
	m_State = InvGrid;

	if ( m_nRows <= 0 )
	{
		m_nRows = m_nAllocatedRows;

		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if( ! this->m_hWnd) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnColsChanged()
{
	m_State = InvGrid;

	if ( m_nCols <= 0 )
	{
		m_nCols = m_nAllocatedCols;

		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}
	
	if( ! this->m_hWnd ) InvalidateControl();
}
//------------------------------------------------------------
void CMultiChartCtrl::OnBarsChanged()
{
	if ( m_nBars <= 0 )
	{
		m_nBars = m_nAllocatedBars;

		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}
	//----

	if (
			m_State > InvGrid && 
			m_nTypeChart != Tipo_SogliaVerticale &&
			m_nTypeChart != Tipo_SogliaOrizzontale
		)
	{
		if (m_nBars > m_nAllocatedBars) 
			ReAllocBars();

		m_State = InvDim;
	}
	else
		m_State = InvGrid;

	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------
void CMultiChartCtrl::OnCheckShowGridChanged()
{
	m_State = min(m_State, InvDraw);

	/*if( ! this->m_hWnd ) */ InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnStepHeightGridChanged()
{
	m_State = min(m_State, InvDraw);

	if( ! this->m_hWnd ) InvalidateControl();
}

//------------------------------------------------------------
void CMultiChartCtrl::OnWhereShowValueBarsChanged ()
{
	m_State = min(m_State, InvDraw);

	if ( m_nWhereShowValueBars < -1 || m_nWhereShowValueBars > 4 ) 
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if (m_nWhereShowValueBars == -1 )
		this->m_bShowHeightBars = FALSE;

	if(m_State > InvGrid)
		for(int nB=0; nB < m_nBars; nB++)
			m_arInfoBars[nB].m_nWhereShowHeightPosBar = m_nWhereShowValueBars;

	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnMaxHeightAllBarsChanged()
{
	m_State = min(m_State, InvDimBar);

	if ( m_dMaxHeightAllBars <= 0) 
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if ( m_dDefaultHeightBars > m_dMaxHeightAllBars ) 
		m_dDefaultHeightBars = 0;

	if(m_State > InvGrid) 
		for(int nB = 0; nB < m_nBars; nB++)
		{
			m_arInfoBars[ nB ]. m_dDefaultHeightPosBar = m_dDefaultHeightBars;
			m_arInfoBars[ nB ]. m_dMaxHeightPosBar = m_dMaxHeightAllBars;
		}

	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------
void CMultiChartCtrl::OnMinHeightAllBarsChanged()
{
	m_State = min(m_State, InvDimBar);

	if ( m_dMinHeightAllBars > 0) 
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if ( m_dDefaultHeightBars < m_dMinHeightAllBars ) 
		m_dDefaultHeightBars = 0;

	if(m_State > InvGrid) 
		for(int nB = 0; nB < m_nBars; nB++)
		{
			m_arInfoBars[ nB ]. m_dDefaultHeightPosBar = m_dDefaultHeightBars;
			m_arInfoBars[ nB ]. m_dMinHeightPosBar = m_dMinHeightAllBars;
		}

	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnDefaultHeightBarsChanged()
{
	//serve solo in fase di creazione della struttura
	//m_State = min(m_State, InvDimBar);

	if ( m_dDefaultHeightBars < m_dMinHeightAllBars || m_dDefaultHeightBars > m_dMaxHeightAllBars ) 
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}
	
	if(m_State > InvGrid)
		for(int nB = 0; nB < m_nBars; nB++)
		{
			m_arInfoBars[ nB ]. m_dDefaultHeightPosBar = m_dDefaultHeightBars;
			m_arInfoBars[ nB ]. m_dMaxHeightPosBar  = m_dMaxHeightAllBars;
			m_arInfoBars[ nB ]. m_dMinHeightPosBar  = m_dMinHeightAllBars;
		}

	//if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnDCHeightAllRowsChanged()
{
	m_State = min(m_State, InvDim);
	
	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnDCWidthColsChanged()
{
	m_State = min(m_State, InvDim);

	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnDCWidthRowHeaderChanged()
{
	m_State = min(m_State, InvDim);
	
	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnDCHeightColHeaderChanged()
{
	m_State = min(m_State, InvDim);

	if( ! this->m_hWnd ) InvalidateControl();
}

// -----------------------------------------------------------
void CMultiChartCtrl::OnUseCustomWidthBarsChanged ()
{	
	m_State = min(m_State, InvDimBar);

	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------
void CMultiChartCtrl::OnUseCustomPosBarsChanged ()
{	
	m_State = min(m_State, InvDimBar);

	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------
void CMultiChartCtrl::OnDCCustomWidthBarsChanged ()
{
	if( m_nDCWidthBars <= 0 || ! m_bUseCustomWidthBars)
	{
		m_bUseCustomWidthBars = FALSE;
		m_nDCWidthBars = 1;
		m_State = min(m_State, InvDim);
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
	}

	switch(m_nTypeChart)
	{
		case Tipo_Verticale:
		case Tipo_SogliaVerticale:
			if ( (m_nDCWidthBars * m_nBars) > m_nDCWidthCols )
			{
				m_nDCWidthBars = m_nDCWidthCols / m_nBars;
				m_State = min(m_State, InvDim);
				m_bUseCustomWidthBars = FALSE;
				ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
				return;
			}
			break;

		case Tipo_Orizzontale:
		case Tipo_SogliaOrizzontale:
			if ( (m_nDCWidthBars * m_nBars) > m_nDCHeightAllRows )
			{
				m_bUseCustomWidthBars = FALSE;
				m_nDCWidthBars = m_nDCHeightAllRows / m_nBars;
				m_State = min(m_State, InvDim);
				ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
				return;
			}
			break;

		case Tipo_OrizzontaleCumulativo:
		case Tipo_VerticaleCumulativo:
		case Tipo_Torta:
		default:
			m_bUseCustomWidthBars = FALSE;
			m_nDCWidthBars = 1;
			m_State = min(m_State, InvDim);

			ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
			return;
	}

	m_State = min(m_State, InvDimBar);
	
	if(m_State > InvGrid)
		for(int nB=0; nB < m_nBars; nB++)
			m_arInfoBars[nB].m_nDCWidthBar = m_nDCWidthBars;

	if( ! this->m_hWnd ) InvalidateControl();
}

// -----------------------------------------------------------
void CMultiChartCtrl::OnShowHeightBarsChanged ()
{	
	m_State = min(m_State, InvDraw);

	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------
void CMultiChartCtrl::OnHFontValuesChanged ()
{
	//if (m_nDCHFontValues < 4 )
	CClientDC dc(this);
	CClientDC* pdc = &dc;

	m_gdiRes.m_fontValues.DeleteObject();

	LOGFONT lf;
	memset(&lf, 0, sizeof(LOGFONT));

	lf.lfHeight = m_nDCHFontValues*10;
	
	lf.lfWeight = FW_NORMAL;
	lf.lfCharSet = DEFAULT_CHARSET;
	LPCTSTR lpszFaceName = _T("Arial");
	lstrcpyn(lf.lfFaceName, lpszFaceName, sizeof(lpszFaceName) / sizeof(lpszFaceName[0]));
//	lf.lfEscapement = lf.lfOrientation = 900;	// decimi di grado

	m_gdiRes.m_fontValues.CreatePointFontIndirect(&lf, pdc);

	m_State = min(m_State, InvDraw);
	if( ! this->m_hWnd) InvalidateControl();
}

void CMultiChartCtrl::OnHFontHeadersChanged ()
{
	CClientDC dc(this);
	CClientDC* pdc = &dc;

	m_gdiRes.m_fontHeaders.DeleteObject();

	LOGFONT lf;
	memset(&lf, 0, sizeof(LOGFONT));

	lf.lfHeight = m_nDCHFontHeaders*10;
	
	lf.lfWeight = FW_NORMAL;
	lf.lfCharSet = DEFAULT_CHARSET;
	LPCTSTR lpszFaceName = _T("Arial");
	lstrcpyn(lf.lfFaceName, lpszFaceName, sizeof(lpszFaceName) / sizeof(lpszFaceName[0]));
//	lf.lfEscapement = lf.lfOrientation = 900;	// decimi di grado

	m_gdiRes.m_fontHeaders.CreatePointFontIndirect(&lf, pdc);

	m_State = min(m_State, InvDraw);
	if (! this->m_hWnd) InvalidateControl();
}

//------------------------------------------------------------
void CMultiChartCtrl::OnGenericInvDraw ()
{	
	m_State = min(m_State, InvDraw);
	
	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnTypeChartChanged ()
{	
//#ifndef _DEBUG
	if
		(
			m_nTypeChart == Tipo_VerticaleCumulativo || 
			m_nTypeChart == Tipo_OrizzontaleCumulativo || 
			m_nTypeChart == Tipo_Torta 
		)
	{
		AfxMessageBox(_T("ATTENZIONE: modalità di visualizzazione non più supportate. Contattare l\'assistenza del prodotto") , MB_OK|MB_ICONINFORMATION);
		m_nTypeChart = 0;
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}
//#endif

	switch ( m_nTypeChart )
	{
		case Tipo_Verticale:
		case Tipo_VerticaleCumulativo:
			m_State = min(m_State, InvDim);
			break;

		case Tipo_Orizzontale:
		case Tipo_OrizzontaleCumulativo:
			m_State = min(m_State, InvDim);

			m_bShowTotBars = false;
			m_bShowTitleBars = false;
			break;

		case Tipo_SogliaOrizzontale:
			m_State = min(m_State, InvGrid);

			m_bShowTotBars = false;
			m_bShowTitleBars = false;
			break;	

		case Tipo_SogliaVerticale:
			m_State = min(m_State, InvGrid);
			break;	
				
		case Tipo_Torta:
			m_State = min(m_State, InvDim);

			m_bShowGrid = false;
			m_bShowTotBars = false;
			m_bShowTitleBars = false;
			break;

		default:
			m_nTypeChart = Tipo_Verticale;
			m_State = min(m_State, InvGrid);

			ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
			return;
	}

	m_nWhereShowValueBars = 0;
	if(m_State > InvGrid)
		for(int nB=0; nB < m_nBars; nB++)
			m_arInfoBars[nB].m_nWhereShowHeightPosBar = m_nWhereShowValueBars;

	m_bUseCustomWidthBars = FALSE;
	m_bUseCustomPosBars = FALSE;

	if( ! this->m_hWnd ) InvalidateControl();
}

//------------------------------------------------------
void CMultiChartCtrl::OnFormatHeightBarsChanged ()
{
	if(m_State > InvGrid)
		for(int nB = 0; nB < m_nBars; nB++)
		{
			m_arInfoBars[nB].m_strFormatHeightPosBar = m_strFormatHeightBars;
		}
	
	m_State = min(m_State, InvDraw);

	if( ! this->m_hWnd ) InvalidateControl();
}

// -----------------------------------------------------------

void CMultiChartCtrl::OnNumLabelRowHeaderChanged()
{
	if ( m_nNumLabelRowHeader < 0 )
	{
		m_nNumLabelRowHeader = 1;
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
	}

	if ( m_State == InvGrid ) AllocGrid ();
	// ----

	for( int nR = 0; nR < m_nRows; nR++ )
		if ( m_arInfoRowHeader [nR].m_arstrTitle.GetSize() < m_nNumLabelRowHeader )
			m_arInfoRowHeader [nR].m_arstrTitle.SetSize (m_nNumLabelRowHeader); 	

	m_State = min(m_State, InvDraw);

	if( ! this->m_hWnd ) InvalidateControl();
}
// -----------------------------------------------------------

void CMultiChartCtrl::OnNumLabelColHeaderChanged()
{	
	if ( m_nNumLabelColHeader < 0 )
	{
		m_nNumLabelColHeader = 1;
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
	}

	if ( m_State == InvGrid ) AllocGrid ();
	// ----

	for( int nC = 0; nC < m_nCols; nC++ )
		if ( m_arInfoColHeader [nC].m_arstrTitle.GetSize() < m_nNumLabelColHeader )
			m_arInfoColHeader [nC].m_arstrTitle.SetSize (m_nNumLabelColHeader); 

	m_State = min(m_State, InvDraw);

	if( ! this->m_hWnd ) InvalidateControl();
}

// ---------------------------------------------------------------------------------
void CMultiChartCtrl::OnShowToolTip()
{
	m_datatip->On(m_bShowToolTip);
}

