#include "stdafx.h"


//includere come ultimo include all'inizio del cpp
#include <TBNameSolver\ThreadContext.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\DockableFrame.h>
#include <TbGeneric\GeneralFunctions.h>

#include "ExtStatusControlBar.h"
#include "TBStrings.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CTaskBuilderStatusBar
/////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CTaskBuilderStatusBar, CBCGPStatusBar)
	ON_WM_ERASEBKGND()
	ON_WM_SIZE()
END_MESSAGE_MAP()

//---------------------------------------------------------------------------
CTaskBuilderStatusBar::CTaskBuilderStatusBar() :
	m_bPaneWithProges(FALSE)
{
	
}

//---------------------------------------------------------------------------
CTaskBuilderStatusBar::~CTaskBuilderStatusBar()
{
	for ( int i = 0; i < m_arrPaneControls.GetSize(); i++ )
	{
		if (
				m_arrPaneControls[i]->pWnd &&
				m_arrPaneControls[i]->pWnd->m_hWnd &&
				::IsWindow(m_arrPaneControls[i]->pWnd->m_hWnd)
			)
		{

			m_arrPaneControls[i]->pWnd->ShowWindow(SW_HIDE);

			if( m_arrPaneControls[i]->bAutoDestroy ) 
			{
				m_arrPaneControls[i]->pWnd->DestroyWindow();
			}
		}
		_STATUSBAR_PANE_CTRL_ *pPaneCtrl = m_arrPaneControls[i];
		if( pPaneCtrl )
			delete pPaneCtrl;
	}
}

//---------------------------------------------------------------------------
LRESULT CTaskBuilderStatusBar::WindowProc(UINT message, WPARAM wParam, LPARAM lParam)
{
	LRESULT lResult = __super::WindowProc(message, wParam, lParam);
	if( message == WM_SIZE )
		RepositionControls();

	return lResult;
}

//---------------------------------------------------------------------------
void CTaskBuilderStatusBar::OnSize(UINT nType, int cx, int cy)
{
	__super::OnSize(nType, cx, cy);
}

//---------------------------------------------------------------------------
int CTaskBuilderStatusBar::GetPanesCount() const
{
	return m_nCount;
}

//PERASSO: porcata, ho clonato il metodo in \BCGCBPro\BCGPStatusBar.cpp
//e sostituito la LoadString di CString con quella di TB, altrimenti con gli id dinamici fallisce la creazione della status bar
//---------------------------------------------------------------------------
BOOL CTaskBuilderStatusBar::SetIndicators(const UINT* lpIDArray, int nIDCount)
{
	ASSERT_VALID(this);
	ASSERT(nIDCount >= 1);  // must be at least one of them
	ASSERT(lpIDArray == NULL ||
		AfxIsValidAddress(lpIDArray, sizeof(UINT) * nIDCount, FALSE));

	// free strings before freeing array of elements
	for (int i = 0; i < m_nCount; i++)
	{
		VERIFY(SetPaneText(i, NULL, FALSE));    // no update
		//free Imagelist if any exist
		SetPaneIcon(i, NULL, FALSE);

	}


	// first allocate array for panes and copy initial data
	if (!AllocElements(nIDCount, sizeof(CBCGStatusBarPaneInfo)))
		return FALSE;

	ASSERT(nIDCount == m_nCount);

	HFONT hFont = GetCurrentFont();

	BOOL bOK = TRUE;
	if (lpIDArray != NULL)
	{
		ASSERT(hFont != NULL);        // must have a font !
		CString strText;
		CClientDC dcScreen(NULL);
		HGDIOBJ hOldFont = dcScreen.SelectObject(hFont);

		for (int i = 0; i < nIDCount; i++)
		{
			CBCGStatusBarPaneInfo* pSBP = _GetPanePtr(i);
			if (pSBP == NULL)
			{
				ASSERT(FALSE);
				return FALSE;
			}

			pSBP->nStyle = 0;
			pSBP->lpszText = NULL;
			pSBP->lpszToolTip = NULL;
			pSBP->clrText = AfxGetThemeManager()->GetStatusbarTextColor();
			pSBP->clrBackground = AfxGetThemeManager()->GetStatusbarBkgColor();
			pSBP->hImage = NULL;
			pSBP->cxIcon = 0;
			pSBP->cyIcon = 0;
			pSBP->rect = CRect(0, 0, 0, 0);
			pSBP->nFrameCount = 0;
			pSBP->nCurrFrame = 0;
			pSBP->nProgressCurr = 0;
			pSBP->nProgressTotal = -1;
			pSBP->clrProgressBar = (COLORREF)-1;
			pSBP->clrProgressBarDest = (COLORREF)-1;
			pSBP->clrProgressText = (COLORREF)-1;
			pSBP->bProgressText = FALSE;

			pSBP->nID = *lpIDArray++;
			if (pSBP->nID != 0)
			{
				//Uso la load di TB, anche se non la trova non invalido tutta la creazione
				strText = AfxLoadTBString(pSBP->nID);
				/*ORIGINALE rimosso (mi fallisce la creazione se non trova la stringa
				if (!strText.LoadString(pSBP->nID))
				{
					TRACE1("Warning: failed to load indicator string 0x%04X.\n",
						pSBP->nID);
					bOK = FALSE;
					break;
				}*/
				pSBP->cxText = dcScreen.GetTextExtent(strText,
					strText.GetLength()).cx;
				ASSERT(pSBP->cxText >= 0);

				if (!SetPaneText(i, strText, FALSE))
				{
					bOK = FALSE;
					break;
				}
			}
			else
			{
				// no indicator (must access via index)
				// default to 1/4 the screen width (first pane is stretchy)
				pSBP->cxText = ::GetSystemMetrics(SM_CXSCREEN) / 4;

				if (i == 0)
				{
					pSBP->nStyle |= (SBPS_STRETCH | SBPS_NOBORDERS);
				}
			}
		}

		dcScreen.SelectObject(hOldFont);
	}

	RecalcLayout();
	return bOK;
}

//---------------------------------------------------------------------------
void CTaskBuilderStatusBar::SetPaneWidth(int nIndex, int nWidth)
{
	_STATUSBAR_PANE_ pane;
	PaneInfoGet(nIndex, &pane);
	pane.cxText = ScalePix(nWidth);
	PaneInfoSet(nIndex, &pane);
}
	
//---------------------------------------------------------------------------
void CTaskBuilderStatusBar::DisableControl( int nIndex, BOOL bDisable)
{
	UINT uItemID = GetItemID(nIndex);
	for ( int i = 0; i < m_arrPaneControls.GetSize(); i++ )
	{
		if( uItemID == m_arrPaneControls[i]->nID )
		{
			if ( 
				m_arrPaneControls[i]->pWnd &&
				m_arrPaneControls[i]->pWnd->m_hWnd &&
				::IsWindow(m_arrPaneControls[i]->pWnd->m_hWnd) 
				)
			{
				m_arrPaneControls[i]->pWnd->EnableWindow(bDisable); 
			}
		}
	}
}

//---------------------------------------------------------------------------
void CTaskBuilderStatusBar::SetPaneInfo(int nIndex, UINT nID, UINT nStyle, int cxWidth)
{
	__super::SetPaneInfo(nIndex, nID, nStyle, ScalePix(cxWidth));
	BOOL bDisabled = ((nStyle&SBPS_DISABLED) == 0);
	DisableControl(nIndex, bDisabled);
}

//---------------------------------------------------------------------------
void CTaskBuilderStatusBar::SetPaneStyle(int nIndex, UINT nStyle)
{
	__super::SetPaneStyle(nIndex, nStyle);
	BOOL bDisabled = ((nStyle&SBPS_DISABLED) == 0);
	DisableControl(nIndex, bDisabled);
}

//---------------------------------------------------------------------------
CTaskBuilderStatusBar::_STATUSBAR_PANE_*  CTaskBuilderStatusBar::GetPanePtr(int nIndex) const
{
	ASSERT((nIndex >= 0 && nIndex < m_nCount) || m_nCount == 0);
	return ((_STATUSBAR_PANE_*)m_pData) + nIndex;
}

//---------------------------------------------------------------------------
void CTaskBuilderStatusBar::RepositionControls()
{
	HDWP _hDWP = ::BeginDeferWindowPos( m_arrPaneControls.GetSize() );
	
	CRect rcClient;
	GetClientRect(&rcClient);
	for (int i = 0; i < m_arrPaneControls.GetSize(); i++ )
	{
		int   iIndex  = CommandToIndex(m_arrPaneControls[i]->nID);
		HWND hWnd    = m_arrPaneControls[i]->pWnd->m_hWnd;
		
		CRect rcPane;
		GetItemRect(iIndex, &rcPane);
		
		// CStatusBar::GetItemRect() sometimes returns invalid size 
		// of the last pane - we will re-compute it
		int cx = ::GetSystemMetrics( SM_CXEDGE );
		DWORD dwPaneStyle = GetPaneStyle( iIndex );
		if( iIndex == (m_nCount-1) )
		{
			if( (dwPaneStyle & SBPS_STRETCH ) == 0 )
			{
				UINT nID, nStyle;
				int  cxWidth;
				GetPaneInfo( iIndex, nID, nStyle, cxWidth );
				rcPane.right = rcPane.left + cxWidth + cx*3;
			} // if( (dwPaneStyle & SBPS_STRETCH ) == 0 )
			else
			{
				CRect rcClient;
				GetClientRect( &rcClient );
				rcPane.right = rcClient.right;
				if( (GetStyle() & SBARS_SIZEGRIP) == SBARS_SIZEGRIP )
				{
					int cxSmIcon = ::GetSystemMetrics( SM_CXSMICON );
					rcPane.right -= cxSmIcon + cx;
				} // if( (GetStyle() & SBARS_SIZEGRIP) == SBARS_SIZEGRIP )
			} // else from if( (dwPaneStyle & SBPS_STRETCH ) == 0 )
		} // if( iIndex == (m_nCount-1) )
		
		if ((GetPaneStyle (iIndex) & SBPS_NOBORDERS) == 0){
			rcPane.DeflateRect(cx,cx);
		}else{
			rcPane.DeflateRect(cx,1,cx,1);
		}
		
		if (hWnd && ::IsWindow(hWnd)){
			_hDWP = ::DeferWindowPos(
				_hDWP, 
				hWnd, 
				NULL, 
				rcPane.left,
				rcPane.top, 
				rcPane.Width(), 
				rcPane.Height(),
				SWP_NOZORDER|SWP_NOOWNERZORDER|SWP_SHOWWINDOW
				);

			::RedrawWindow(
				hWnd,
				NULL,
				NULL,
				RDW_INVALIDATE|RDW_UPDATENOW
				|RDW_ERASE|RDW_ERASENOW
				);
			
		} // if (hWnd && ::IsWindow(hWnd)){ 
	}
	
	VERIFY( ::EndDeferWindowPos( _hDWP ) );
};


//-------------------------------------------------------------------------------------
BOOL CTaskBuilderStatusBar::OnEraseBkgnd(CDC* pDC)
{
	__super::OnEraseBkgnd(pDC);
	CRect rclientRect;
	GetClientRect(&rclientRect);
	pDC->FillRect(&rclientRect, AfxGetThemeManager()->GetStatusbarBkgBrush());
	return TRUE;
}

//---------------------------------------------------------------------------
void CTaskBuilderStatusBar::OnDrawPane(CDC* pDC, CBCGStatusBarPaneInfo* pPane)
{
	if (m_bPaneWithProges)
	{
		CRect rectPane = pPane->rect;
		// Fill pane background:
		if (pPane->clrBackground != (COLORREF)-1)
		{
			CBrush brush(pPane->clrBackground);
			CBrush* pOldBrush = pDC->SelectObject(&brush);
			pDC->PatBlt(rectPane.left, rectPane.top, rectPane.Width(), rectPane.Height(), PATCOPY);
			pDC->SelectObject(pOldBrush);
		}

		CRect rectText = rectPane;
		if (pPane->lpszText != NULL && pPane->cxText > 0)
		{
			COLORREF clrText = pDC->SetTextColor(CBCGPVisualManager::GetInstance()->GetStatusBarPaneTextColor(this, pPane));
			CSize sizeText = pDC->GetTextExtent(pPane->lpszText);		// Length of text
			rectText.left = m_nTextwidthProges - sizeText.cx;
			pDC->DrawText(pPane->lpszText, lstrlen(pPane->lpszText), rectText, DT_LEFT | DT_SINGLELINE | DT_VCENTER | DT_NOPREFIX);
			pDC->SetTextColor(clrText);
		}
		return;
	}

	__super::OnDrawPane(pDC, pPane);
}

//---------------------------------------------------------------------------
BOOL CTaskBuilderStatusBar::AddPane(
	 UINT nID,	// ID of the  pane
	 int nIndex	// index of the pane
	 )
{
	if (nIndex < 0 || nIndex > m_nCount){
		ASSERT(FALSE);
		return FALSE;
	}
	
	if (CommandToIndex(nID) != -1){
		ASSERT(FALSE);
		return FALSE;
	}
	
	CArray<_STATUSBAR_PANE_*,_STATUSBAR_PANE_*> arrPanesTmp;
	int iIndex = 0;
	for (iIndex = 0; iIndex < m_nCount+1; iIndex++)
	{
		_STATUSBAR_PANE_* pNewPane = new _STATUSBAR_PANE_;
		
		if (iIndex == nIndex){
			pNewPane->nID    = nID;
			pNewPane->nStyle = SBPS_NORMAL;
		}else{
			int idx = iIndex;
			if (iIndex > nIndex) idx--;
			
			_STATUSBAR_PANE_* pOldPane  = GetPanePtr(idx);
			pNewPane->cxText  = pOldPane->cxText;
			pNewPane->nFlags  = pOldPane->nFlags;
			pNewPane->nID     = pOldPane->nID;
			pNewPane->nStyle  = pOldPane->nStyle;
			pNewPane->strText = pOldPane->strText;
		}
		arrPanesTmp.Add(pNewPane);
	}
	
	int nPanesCount = arrPanesTmp.GetSize();
	UINT* lpIDArray = new UINT[ nPanesCount ];
	for (iIndex = 0; iIndex < nPanesCount; iIndex++) {
		lpIDArray[iIndex] = arrPanesTmp[iIndex]->nID;
	}
	
	// set the indicators 
	SetIndicators(lpIDArray, nPanesCount);
	// free memory
	for (iIndex = 0; iIndex < nPanesCount; iIndex++){
		_STATUSBAR_PANE_* pPane = arrPanesTmp[iIndex];
		if (iIndex != nIndex)
			PaneInfoSet(iIndex, pPane);
		if(pPane) 
			delete pPane;
	}
	
	arrPanesTmp.RemoveAll();
	if(lpIDArray) 
		delete []lpIDArray;
	
	RepositionControls();
	
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL CTaskBuilderStatusBar::RemovePane(
	UINT nID	// ID of the pane
	)
{
	if ( CommandToIndex(nID) == -1 || m_nCount == 1 ){
		ASSERT(FALSE);
		return FALSE;
	}
	
	CArray<_STATUSBAR_PANE_*,_STATUSBAR_PANE_*> arrPanesTmp;
	int nIndex;
	for (nIndex = 0; nIndex < m_nCount; nIndex++)
	{
		_STATUSBAR_PANE_* pOldPane = GetPanePtr(nIndex);
		
		if (pOldPane->nID == nID)
			continue;
		
		_STATUSBAR_PANE_* pNewPane = new _STATUSBAR_PANE_;
		
		pNewPane->cxText  = pOldPane->cxText;
		pNewPane->nFlags  = pOldPane->nFlags;
		pNewPane->nID     = pOldPane->nID;
		pNewPane->nStyle  = pOldPane->nStyle;
		pNewPane->strText = pOldPane->strText;
		arrPanesTmp.Add(pNewPane);
	}
	
	UINT* lpIDArray = new UINT[arrPanesTmp.GetSize()];
	for (nIndex = 0; nIndex < arrPanesTmp.GetSize(); nIndex++) {
		lpIDArray[nIndex] = arrPanesTmp[nIndex]->nID;
	}
	
	// set the indicators
	SetIndicators(lpIDArray, arrPanesTmp.GetSize());
	// free memory
	for (nIndex = 0; nIndex < arrPanesTmp.GetSize(); nIndex++)
	{
		_STATUSBAR_PANE_* pPane = arrPanesTmp[nIndex];
		PaneInfoSet(nIndex, pPane);
		if(pPane) 
			delete pPane;
	}
	
	for ( int i = 0; i < m_arrPaneControls.GetSize(); i++ ){
		if (m_arrPaneControls[i]->nID == nID)
		{
			if ( 
					m_arrPaneControls[i]->pWnd &&
					m_arrPaneControls[i]->pWnd->m_hWnd &&
					::IsWindow(m_arrPaneControls[i]->pWnd->m_hWnd) 
				)
			{
				m_arrPaneControls[i]->pWnd->ShowWindow(SW_HIDE); 
				if( m_arrPaneControls[i]->bAutoDestroy )
				{
					delete m_arrPaneControls[i]->pWnd;
				}
			}
			_STATUSBAR_PANE_CTRL_ *pPaneCtrl = m_arrPaneControls[i];
			if( pPaneCtrl )
			{

				delete pPaneCtrl;
			}
			m_arrPaneControls.RemoveAt(i);
			break;
		}
	}
	
	arrPanesTmp.RemoveAll();
	if(lpIDArray) 
		delete []lpIDArray;
	
	RepositionControls();
	
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL CTaskBuilderStatusBar::AddPaneControl(CWnd* pWnd, UINT nID, BOOL bAutoDestroy)
{
	if (CommandToIndex (nID) == -1) {
		return FALSE;
	}
	
	_STATUSBAR_PANE_CTRL_* pPaneCtrl = new _STATUSBAR_PANE_CTRL_;
	pPaneCtrl->nID         = nID;
	pPaneCtrl->pWnd        = pWnd;
	pPaneCtrl->bAutoDestroy = bAutoDestroy;
	
	m_arrPaneControls.Add(pPaneCtrl);

	RepositionControls();
	return TRUE;
}

//---------------------------------------------------------------------------
BOOL CTaskBuilderStatusBar::PaneInfoGet(int nIndex, _STATUSBAR_PANE_* pPane)
{
	if( nIndex < m_nCount  && nIndex >= 0 )
	{
		GetPaneInfo( nIndex,  pPane->nID, pPane->nStyle, pPane->cxText );
		CString strPaneText;
		GetPaneText( nIndex , strPaneText );
		pPane->strText = LPCTSTR(strPaneText);
		return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
BOOL CTaskBuilderStatusBar::PaneInfoSet(int nIndex, _STATUSBAR_PANE_* pPane)
{
	if( nIndex < m_nCount  && nIndex >= 0 ){
		SetPaneInfo( nIndex, pPane->nID, pPane->nStyle, pPane->cxText );
		SetPaneText( nIndex, LPCTSTR( pPane->strText) );
		return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
CSize CTaskBuilderStatusBar::CalcFixedLayout(BOOL bStretch, BOOL bHorz)
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());

	if (!pFrame)
		return CSize(0, 0);

	if (pFrame->IsLayoutSuspended(TRUE))
		return CSize(0, 0);
	
	CSize size = __super::CalcFixedLayout(bStretch, bHorz);
	size.cy = max(size.cy, AfxGetThemeManager()->GetStatusBarHeight());
	return size;
}