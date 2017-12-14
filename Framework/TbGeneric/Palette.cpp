
#include "stdafx.h"

#include "palette.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


/////////////////////////////////////////////////////////////////////////////
// CPaletteBar
//---------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPaletteBar, CToolBar)
	//{{AFX_MSG_MAP(CPaletteBar)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CPaletteBar construction/destruction
//---------------------------------------------------------------------------
CPaletteBar::CPaletteBar()
{
	m_nColumns = 4;
	m_cxLeftBorder = 5;
	m_cxRightBorder = 5;
	m_cyTopBorder = 5;
	m_cyBottomBorder = 5;
}

//---------------------------------------------------------------------------
CPaletteBar::~CPaletteBar()
{
}

/////////////////////////////////////////////////////////////////////////////
// CPaletteBar diagnostics

#ifdef _DEBUG
void CPaletteBar::AssertValid() const
{
	CToolBar::AssertValid();
}

void CPaletteBar::Dump(CDumpContext& dc) const
{
	CToolBar::Dump(dc);
}

#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CPaletteBar message handlers

//---------------------------------------------------------------------------
void CPaletteBar::SetColumns(UINT nColumns)
{
	m_nColumns = nColumns;

	if (m_dwStyle & CBRS_SIZE_DYNAMIC)
		m_nMRUWidth = 23 * m_nColumns;	// vedi implementazioni di MFC (23 = 16 pixel + 7 effetti 3D)
	else
	{
		int nCount = GetToolBarCtrl().GetButtonCount();

		for(int i = 0; i < nCount; i++)
		{
			UINT nStyle = GetButtonStyle(i);
			BOOL bWrap = (((i + 1) % m_nColumns) == 0);
			if (bWrap)
				nStyle |= TBBS_WRAPPED;
			else
				nStyle &= ~TBBS_WRAPPED;
				
			SetButtonStyle(i, nStyle);
		}
	}

	Invalidate();
	GetParentFrame()->RecalcLayout();
}

