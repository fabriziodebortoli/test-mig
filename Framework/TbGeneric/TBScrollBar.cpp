#include "stdafx.h"
#include "TBScrollBar.h"
#include "GeneralFunctions.h"
#include "TBThemeManager.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

///////////////////////////////////////////////////////////////////////////
// CTBScrollBar

IMPLEMENT_DYNCREATE(CTBScrollBar, CBCGPScrollBar)

BEGIN_MESSAGE_MAP(CTBScrollBar, CBCGPScrollBar)
	//{{AFX_MSG_MAP(CTBScrollBar)
	ON_WM_PAINT()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------
CTBScrollBar::CTBScrollBar() :
	m_bVisible(TRUE),
	m_nWidth(0),
	m_BackGroundColor(AfxGetThemeManager()->GetScrollBarFillBkgColor()),
	m_BkgButtonNoPressedColor(AfxGetThemeManager()->GetScrollBarBkgButtonNoPressedColor()),
	m_BkgButtonPressedColor(AfxGetThemeManager()->GetScrollBarBkgButtonPressedColor()),
	m_bThumbVisible(TRUE)
{
}

//-------------------------------------------------------------------------------------
CTBScrollBar::~CTBScrollBar()
{
}

//-------------------------------------------------------------------------------------
int CTBScrollBar::GetWidth()
{
	return GetSystemMetrics(SM_CXVSCROLL);
}

int CTBScrollBar::GetHeight()
{ 
	return GetSystemMetrics(SM_CYHSCROLL);
}

//-------------------------------------------------------------------------------------
BOOL CTBScrollBar::isVisible() 
{ 
	return m_bVisible; 
}

//-------------------------------------------------------------------------------------
void CTBScrollBar::SetVisible(BOOL bVisible /*= TRUE*/) 
{ 
	m_bVisible = bVisible; 
}

//-------------------------------------------------------------------------------------
void CTBScrollBar::OnPaint()
{
	__super::OnPaint();
}
