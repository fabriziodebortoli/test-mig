
#include "stdafx.h"

#include "MultiChartCtl.h"
#include "resource.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//------------------------------------------------------------------------
void CMultiChartCtrl::SetLabelRowHeader(short nR, LPCTSTR sLabel, short nL)
{
	if ( nR < 0 || nR >= m_nRows || nL < 0 || nL >= m_nNumLabelRowHeader ) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	// ----

	m_arInfoRowHeader [nR].m_arstrTitle [nL] = (LPCTSTR) sLabel;

	m_State = min(m_State, InvDraw);
}

// --------------------------------------------------------------------------
void CMultiChartCtrl::SetLabelColHeader(short nC, LPCTSTR sLabel, short nL)
{
	if ( nC < 0 || nC >= m_nCols || nL < 0 || nL >= m_nNumLabelColHeader )
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	// ----

	m_arInfoColHeader [nC].m_arstrTitle [nL] = (LPCTSTR) sLabel;

	m_State = min(m_State, InvDraw);

	if( ! this->m_hWnd ) InvalidateControl();
}

// --------------------------------------------------------------------------
void CMultiChartCtrl::SetAlignLabelRowHeader (short nR, short nA /*, short nL*/)
{
	if ( nR < 0 || nR >= m_nRows /*|| nL < 0 || nL >= m_nNumLabelRowHeader*/) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if ( nA > 1 || nA < -1)
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	// ----

	m_arInfoRowHeader [nR].m_nAlign = nA;

	m_State = min(m_State, InvDraw);
}

// --------------------------------------------------------------------------
void CMultiChartCtrl::SetAlignLabelColHeader (short nC, short nA /*, short nL*/)
{
	if ( nC < 0 || nC >= m_nCols /*|| nL < 0 || nL >= m_nNumLabelColHeader */) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if ( nA > 1 || nA < -1)
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	// ----

	m_arInfoColHeader [nC].m_nAlign = nA;

	m_State = min(m_State, InvDraw);
}

//--------------------------------------------------------------------------
void CMultiChartCtrl::SetAlignLabelAllRowHeader (short nA)
{
	if ( nA > 1 || nA < -1)
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	// ----
	for (int i=0; i < m_nRows; i++)
		m_arInfoRowHeader [i].m_nAlign = nA;

	m_State = min(m_State, InvDraw);
}

//--------------------------------------------------------------------------
void CMultiChartCtrl::SetAlignLabelAllColHeader (short nA)
{
	if ( nA > 1 || nA < -1)
	{
		ThrowError ( CTL_E_INVALIDPROPERTYVALUE, IDS_E_INVALIDPROPERTYVALUE );
		return;
	}

	if(m_State == InvGrid) AllocGrid();
	// ----
	for (int i=0; i < m_nCols; i++)
		m_arInfoColHeader [i].m_nAlign = nA;

	m_State = min(m_State, InvDraw);
}

//--------------------------------------------------------------
COLORREF CMultiChartCtrl::GetTextColorRowHeaderCell(short nR)
{
	if (nR < 0 || nR >= m_nRows || m_State == InvGrid) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return RGB(0,0,0);
	}

	return m_arInfoRowHeader [nR].m_crTextColor;
}

//---------------------------------------------------------------------------
COLORREF CMultiChartCtrl::GetBackColorRowHeaderCell(short nR)
{
	if (nR < 0 || nR >= m_nRows || m_State == InvGrid) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return RGB(0,0,0);
	}

	return m_arInfoRowHeader [nR].m_crBackColor;
}

//---------------------------------------------------------------------------
COLORREF CMultiChartCtrl::GetTextColorColHeaderCell(short nC)
{
	if (nC < 0 || nC >= m_nCols || m_State == InvGrid) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return RGB(0,0,0);
	}

	return m_arInfoColHeader [nC].m_crTextColor;
}

//--------------------------------------------------------------------------
COLORREF CMultiChartCtrl::GetBackColorColHeaderCell(short nC)
{
	if (nC < 0 || nC >= m_nCols || m_State == InvGrid) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return RGB(0,0,0);
	}

	return m_arInfoColHeader [nC].m_crBackColor;
}

//--------------------------------------------------------------------------
void CMultiChartCtrl::SetTextColorRowHeaderCell(short nR, COLORREF crColor )
{
	if (nR < 0 || nR >= m_nRows) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if(m_State == InvGrid) AllocGrid();

	m_arInfoRowHeader [nR].m_crTextColor = crColor;
	m_State = min(m_State, InvDraw);
}

//-------------------------------------------------------------------------
void CMultiChartCtrl::SetBackColorRowHeaderCell(short nR, COLORREF crColor )
{
	if (nR < 0 || nR >= m_nRows) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if(m_State == InvGrid) AllocGrid();

	m_arInfoRowHeader [nR].m_crBackColor = crColor;
	m_State = min(m_State, InvDraw);
}

//--------------------------------------------------------------------------
void CMultiChartCtrl::SetTextColorColHeaderCell(short nC, COLORREF crColor )
{
	if (nC < 0 || nC >= m_nCols) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if(m_State == InvGrid) AllocGrid();

	m_arInfoColHeader [nC].m_crTextColor = crColor;
	m_State = min(m_State, InvDraw);
}

//--------------------------------------------------------------------------
void CMultiChartCtrl::SetBackColorColHeaderCell(short nC, COLORREF crColor )
{
	if (nC < 0 || nC >= m_nCols) 
	{
		ThrowError ( CTL_E_BADRECORDNUMBER, IDS_E_BADRECORDNUMBER );
		return;
	}
	if(m_State == InvGrid) AllocGrid();

	m_arInfoColHeader [nC].m_crBackColor = crColor;
	m_State = min(m_State, InvDraw);
}

//===========================================================================