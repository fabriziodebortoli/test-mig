#include "stdafx.h"

#include <TbNameSolver\TbNamespaces.h>

#include "parsobj.h"
#include "BaseDoc.h"
#include "OslBaseInterface.h"
#include "TBAutoHideBar.h"


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//						CTaskBuilderDockPaneTabs 
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CTBAutoHideBar, CBCGPAutoHideToolBar)

//-----------------------------------------------------------------------------
CTBAutoHideBar::CTBAutoHideBar()
{
	m_nSize = AfxGetThemeManager()->GetDockPaneAutoHideBarSize();
}

//-----------------------------------------------------------------------------
CSize CTBAutoHideBar::CalcFixedLayout(BOOL bStretch, BOOL bHorz)
{
	if (IsLayoutSuspended())
		return CSize(0, 0);

	if (m_nSize > 0)
		return m_nSize;
	
	return __super::CalcFixedLayout(bStretch, bHorz); 
}

//-----------------------------------------------------------------------------
void CTBAutoHideBar::SetSize(int nSize)
{
	m_nSize = nSize;
}

//----------------------------------------------------------------------------------------
BOOL CTBAutoHideBar::IsLayoutSuspended()
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(GetParentFrame());

	if (pFrame && pFrame->IsLayoutSuspended(TRUE))
		return TRUE;
	
	return FALSE;
}

//-----------------------------------------------------------------------------------
CSize CTBAutoHideBar::StretchControlBar(int nLength, BOOL bVert)
{
	return __super::StretchControlBar(nLength, bVert);
}
