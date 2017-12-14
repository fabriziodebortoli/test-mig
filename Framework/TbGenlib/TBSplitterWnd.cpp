#include "stdafx.h"

#include "TBSplitterWnd.h"
#include "PARSOBJ.H"
#include "TABCORE.H"
#include "BaseDoc.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

IMPLEMENT_DYNCREATE(CTaskBuilderSplitterWnd, CBCGPSplitterWnd)

BEGIN_MESSAGE_MAP(CTaskBuilderSplitterWnd, CBCGPSplitterWnd)
    ON_WM_SIZE		()
	ON_WM_LBUTTONUP	() 
END_MESSAGE_MAP()

//------------------------------------------------------------------
CTaskBuilderSplitterWnd::CTaskBuilderSplitterWnd()
{
	m_fSplitRatio	   = -1;
    m_bPanesSwapped	   = FALSE;
    m_nSplitResolution = 1;
}

//------------------------------------------------------------------
CTaskBuilderSplitterWnd::~CTaskBuilderSplitterWnd()
{
	/* TODOBRUNA memory leak delle finestre
	for (int r=m_nRows-1; r >=0; r--)
	{
		for (int c=m_nCols-1; c >=0; c--)
		{
			DeleteView(r, c);
		}
	}*/
}

//------------------------------------------------------------------
CWnd* CTaskBuilderSplitterWnd::GetActivePane(int* pRow, int* pCol)
{
	ASSERT_VALID(this);

	// attempt to use active view of frame window
	CFrameWnd* pFrameWnd = GetParentFrame();
	ASSERT_VALID(pFrameWnd);
	CWnd* pWnd = pFrameWnd->GetActiveWindow();

	// failing that, use the current focus
	if (pWnd == NULL)
		pWnd = GetFocus();

	return pWnd;
}

//------------------------------------------------------------------
void CTaskBuilderSplitterWnd::OnLButtonUp(UINT uFlags, CPoint point) 
{ 
	__super::OnLButtonUp(uFlags, point); 
    UpdateSplitRatio();
} 

//------------------------------------------------------------------
void CTaskBuilderSplitterWnd::SetSplitRatio(float fRatio)
{
    m_fSplitRatio = fRatio;
}

//------------------------------------------------------------------
BOOL CTaskBuilderSplitterWnd::IsSplitHorizontally() const
{
    ASSERT((m_nRows > 1) != (m_nCols > 1));
    ASSERT(max( m_nRows, m_nCols) == 2); 

    return (m_nCols > 1);
}

//------------------------------------------------------------------
void CTaskBuilderSplitterWnd::SplitHorizontally()
{
    if (IsSplitHorizontally())
        return;

    ASSERT(m_nCols = 1);
    ASSERT(m_nRows = 2);
    CWnd* pPane = GetDlgItem(IdFromRowCol(1, 0));
    ASSERT(pPane);

    // swap the H/V information
    m_nMaxCols			= m_nCols = 2;
    m_nMaxRows			= m_nRows = 1;
    CRowColInfo* pTmp	= m_pColInfo;
    m_pColInfo			= m_pRowInfo;
    m_pRowInfo			= pTmp;

   // change the last pane's ID reference
    pPane->SetDlgCtrlID(IdFromRowCol(0, 1));
    ASSERT(GetPane(0, 1)->GetSafeHwnd() == pPane->GetSafeHwnd());

	UpdatePanes();
    RecalcLayout();
}

//------------------------------------------------------------------
void CTaskBuilderSplitterWnd::SplitVertically()
{
    if( IsSplitVertically())
        return;

    ASSERT(m_nCols = 2);
    ASSERT(m_nRows = 1);
    CWnd* pPane = GetDlgItem(IdFromRowCol(0, 1));
    ASSERT(pPane);

    // swap the H/V information
    m_nMaxCols			= m_nCols = 1;
    m_nMaxRows			= m_nRows = 2;
    CRowColInfo* pTmp	= m_pColInfo;
    m_pColInfo			= m_pRowInfo;
    m_pRowInfo			= pTmp;

    // change last pane's ID reference (no need to change ID for first one)
    pPane->SetDlgCtrlID(IdFromRowCol(1, 0));
    ASSERT(GetPane(1, 0)->GetSafeHwnd() == pPane->GetSafeHwnd());

	UpdatePanes();
    RecalcLayout();
}

//------------------------------------------------------------------
void CTaskBuilderSplitterWnd::UpdateSplitRatio()
{
    CRowColInfo*	pPanes;
    int				czSplitter;

    if(IsSplitHorizontally())
    {
        pPanes     = m_pColInfo;
        czSplitter = m_cxSplitter;
    }
    else
    {
        pPanes     = m_pRowInfo;
        czSplitter = m_cySplitter;
    }

    if ((pPanes[0].nCurSize != -1) && (pPanes[0].nCurSize + pPanes[1].nCurSize != 0))
        m_fSplitRatio = m_nSplitResolution * ((float)pPanes[0].nCurSize / (pPanes[0].nCurSize + pPanes[1].nCurSize + czSplitter));
}

//------------------------------------------------------------------
void CTaskBuilderSplitterWnd::UpdatePanes()
{
    CRect rcClient;
    GetClientRect(rcClient);

    UpdatePanes(rcClient.Width(), rcClient.Height());
}

//------------------------------------------------------------------
void CTaskBuilderSplitterWnd::UpdatePanes(int cx, int cy)
{
	CRowColInfo*	pPanes;
    int				cz;

    if (IsSplitHorizontally())
    {
        pPanes = m_pColInfo;
        cz     = cx;
    }
    else
    {
        pPanes = m_pRowInfo;
        cz     = cy;
    }

    if (m_fSplitRatio > 0)
       pPanes[0].nIdealSize = int( m_fSplitRatio * ((float)cz / m_nSplitResolution));
	else //caso in cui qualcosa é andato male
       pPanes[0].nIdealSize =  int(((float)60/100) * ((float)cz / m_nSplitResolution));
}

//------------------------------------------------------------------
void CTaskBuilderSplitterWnd::OnSize(UINT nType, int cx, int cy)
{
	if ((nType != SIZE_MINIMIZED )&&( cx > 0 ) && ( cy > 0 ))
        UpdatePanes(cx, cy);

    __super::OnSize(nType, cx, cy);
}

//------------------------------------------------------------------
int CTaskBuilderSplitterWnd::AddWindow (CRuntimeClass* pWndClass, CCreateContext* pCreateContext, int nRow, int nCol)
{
	if (!CreateView(nRow, nCol, pWndClass, CSize(0, 0), pCreateContext))
	{
		ASSERT_TRACE(FALSE, "Failed to create splitter view class!\n");
		return FALSE;
	}

	// nel caso la view finisca in una dialog (tab dialog) che nasce e muore, mi
	// devo occupare di far rieseguire il codice delle OnInitialUpdate visto
	// che dopo la nascita del frame principale nessuno piu' lo invoca.
	// uso pDoc->GetFrameHandle() per sapere se sono o meno nella creazione del frame.
	CBaseDocument* pDoc = (CBaseDocument*) pCreateContext->m_pCurrentDoc;
	CView* pView = dynamic_cast<CView*>(GetPane(nRow, nCol));
	
	if (!pDoc || !pView)
		return FALSE;

	if (pView && pDoc->GetFrameHandle() != NULL)
		pView->SendMessage(WM_INITIALUPDATE);
	
	return TRUE;
}
