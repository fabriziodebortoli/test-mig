// TabSelector.cpp : implementation file
//

#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbGeneric\VisualStylesXP.h>

#include "BaseDoc.h"
#include "TabSelector.h"
#include "TBTabWnd.h"
#include "TABCORE.H"
#include "extres.hjson" //JSON AUTOMATIC UPDATE


#define wordWrapDisplacement	15
#define selectorIconSize	(25 + 8)

//======================================================================
// qui ho dovuto derivare uno ggetto  perche' tutti i membri bcg sono 
// protetti e gestiti friend delle classi originali.
class CTabSelectorTabWndInfo : public CTaskBuilderTabWndInfo
{
	DlgInfoItem* m_pDlgInfo;

public:
	CTabSelectorTabWndInfo(DlgInfoItem* pDlgInfo) : 
		CTaskBuilderTabWndInfo(pDlgInfo->m_strTitle, pDlgInfo->GetDialogID()),
		m_pDlgInfo(pDlgInfo) 

	{
		SetForeColor(AfxGetThemeManager()->GetTabberTabForeColor());
	}

	// dato che le nostre tab nascono on-demand, questo metodo si occupa di 
	// assicurare che finestra venga agganciata appena disponibile
	//------------------------------------------------------------------------------
	void SyncWidthDialogInfo()
	{
		// aggiorno i dati sulla base dell'ultimo stato gestionale del tabber
		if (m_pDlgInfo && m_pDlgInfo->m_pBaseTabDlg != m_pWnd)
			m_pWnd = m_pDlgInfo->m_pBaseTabDlg;

		m_strText = m_pDlgInfo->m_strTitle;
		m_bVisible = m_pDlgInfo->IsVisible() && m_pDlgInfo->IsEnabled();

		if (m_pWnd)
			m_pWnd->EnableWindow(m_pDlgInfo->IsEnabled());
	}

	CWnd* GetTabWnd()			{ return m_pDlgInfo->m_pBaseTabDlg; }
	void  SetTabWnd(CWnd* pWnd) { m_pWnd = pWnd; }
};

//------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTabSelectorTabWnd, CTaskBuilderTabWnd)

//------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTabSelectorTabWnd, CTaskBuilderTabWnd)
	ON_WM_ERASEBKGND()
END_MESSAGE_MAP()
//------------------------------------------------------------------------------
BOOL CTabSelectorTabWnd::Create(CWnd* pParent)
{
	BOOL bOk = __super::Create(AfxGetThemeManager()->GetTabberTabStyle(), CRect(), pParent, AfxGetTBResourcesMap()->GetTbResourceID(_T("Tabber"), TbControls), CTaskBuilderTabWnd::LOCATION_TOP);

	SetActiveTabColor(m_pThemeManager->GetTabberTabSelectedBkgColor());
	SetActiveTabTextColor(m_pThemeManager->GetTabberTabSelectedForeColor());
	return bOk;
}

//------------------------------------------------------------------------------
COLORREF CTabSelectorTabWnd::GetTabBkColor(int iTab) const
{
	return m_pThemeManager->GetTabberTabBkgColor();
}

//------------------------------------------------------------------------------
CTabSelectorTabWnd::CTabSelectorTabWnd()
	:
	CTaskBuilderTabWnd()
{
	m_pThemeManager = AfxGetThemeManager();
}

// la classe BCG non testa i puntatori alle m_pWnd pertanto non posso eseguire
// il codice di CleanUp se prima non ho eliminato io l'array delle tab che
//
//------------------------------------------------------------------------------
void CTabSelectorTabWnd::CleanUp()
{
	for (int i = m_arTabs.GetUpperBound(); i >= 0; i--)
	{
		CBCGPTabInfo* pBaseInfo = (CBCGPTabInfo*)m_arTabs.GetAt(i);
		CTabSelectorTabWndInfo* pInfo = dynamic_cast<CTabSelectorTabWndInfo*>(pBaseInfo);
		if (!pInfo->GetTabWnd())
		{
			m_arTabs.RemoveAt(i);
			delete pInfo;
			m_iTabsNum--;
		}
	}

	__super::CleanUp();
}

//------------------------------------------------------------------------------
BOOL CTabSelectorTabWnd::SetActiveTab(int iTab)
{ 
	// stessa tab
	if (GetActiveTab() == iTab)
		return FALSE;

	if (iTab < 0 || iTab >= m_arTabs.GetSize())
		return FALSE;

	// okkio: m_bUserSelectedTab mi dice che l'utente
	// ha cliccato sulla linguetta (OnLButtonDown)
	// e non sto usando invece il metodo programmativo
	if (m_bUserSelectedTab)
	{
		CTabSelector* pSelector = (CTabSelector*) GetParent();
		// chiedo di far scattare i DotabSelChanging/e al nostro tabber
		pSelector->DoTabClick(iTab);
	}

	// devo avvisare il tabber bcg che e' cambiata la
	// dialog attiva prima per poter riassegnare bene la 
	// finestra nella AssureWnd
	m_iActiveTab = iTab;

	CBCGPTabInfo* pBaseInfo = (CBCGPTabInfo*)m_arTabs.GetAt(iTab);
	CTabSelectorTabWndInfo* pInfo = dynamic_cast<CTabSelectorTabWndInfo*>(pBaseInfo);
	if (pInfo)
		pInfo->SyncWidthDialogInfo();

	InvalidateRect(m_rectTabsArea);
	// ora che e' tutto pronto posso far scattare il cambio linguetta
	return __super::SetActiveTab(iTab);
}

//------------------------------------------------------------------------------
void CTabSelectorTabWnd::UpdateTabStates()
{
	for (int i = 0; i < m_arTabs.GetSize(); i++)
	{
		CBCGPTabInfo* pBaseInfo = (CBCGPTabInfo*)m_arTabs.GetAt(i);
		CTabSelectorTabWndInfo* pInfo = dynamic_cast<CTabSelectorTabWndInfo*>(pBaseInfo);
		if (pInfo)
			pInfo->SyncWidthDialogInfo();
	}

	AdjustTabs();
	InvalidateRect(m_rectTabsArea);
	UpdateWindow();

}

//------------------------------------------------------------------------------
void CTabSelectorTabWnd::CleanTabState(CWnd* pWnd)
{
	for (int i = 0; i < m_arTabs.GetSize(); i++)
	{
		CBCGPTabInfo* pBaseInfo = (CBCGPTabInfo*)m_arTabs.GetAt(i);
		CTabSelectorTabWndInfo* pInfo = dynamic_cast<CTabSelectorTabWndInfo*>(pBaseInfo);
		if (pInfo && pInfo->GetTabWnd() == pWnd)
			pInfo->SetTabWnd(NULL);			
	}
}


//------------------------------------------------------------------------------
BOOL CTabSelectorTabWnd::OnEraseBkgnd(CDC* pDC)
{
	CRect rclientRect;
	this->GetClientRect(rclientRect);
	if (m_arTabs.GetSize() > 0)
	{
		CBrush brush; 
		brush.CreateSolidBrush(GetTabBkColor(GetActiveTab()));
		pDC->FillRect(&rclientRect, &brush);
		brush.DeleteObject();
	}
	else
		pDC->FillRect(&rclientRect, AfxGetThemeManager()->GetTabSelectorBkgColorBrush());

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//			CSelectorButtonContainer
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CSelectorButtonContainer, CWnd)

BEGIN_MESSAGE_MAP(CSelectorButtonContainer, CWnd)
	ON_WM_ERASEBKGND()
	ON_WM_VSCROLL()
	ON_WM_SIZE()
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CSelectorButtonContainer::CSelectorButtonContainer()
	:
	m_pVScrollBar(NULL),
	m_nRealHeight(0),
	m_nScrollPosY(0),
	m_nScrollStepY(0),
	m_nScrollWidth(0)
{
	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//------------------------------------------------------------------------------
CSelectorButtonContainer::~CSelectorButtonContainer()
{
	SAFE_DELETE(m_pVScrollBar);

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//-----------------------------------------------------------------------------
HRESULT CSelectorButtonContainer::get_accName(VARIANT varChild, BSTR *pszName)
{
	*pszName = ::SysAllocString(_T("CSelectorButtonContainer"));
	return S_OK;
}

//------------------------------------------------------------------------------------
void CSelectorButtonContainer::CreateAccessories()
{
	m_pVScrollBar = new CTBScrollBar();
	CRect rect(0, 0, 0, 0);

	UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(_T("SelectorButtonContainerVScroll"), TbResourceType::TbControls);
	if (!m_pVScrollBar->Create(SBS_VERT | WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS, rect, this, nIDC))
	{
		TRACE0("SelectorBodyEditContainer::CreateAccessories: failed to create vertical scroll bar");
		SAFE_DELETE(m_pVScrollBar);
	}
	else
	{
		SCROLLINFO sinfo;
		if (m_pVScrollBar->GetScrollInfo(&sinfo))
		{
			sinfo.nPage = 1;
			sinfo.nMax = GetRealHeight() - m_nScrollPosY;
			VERIFY(m_pVScrollBar->SetScrollInfo(&sinfo));
		}
	}

	m_pVScrollBar->SetThumbVisible(FALSE);
	m_pVScrollBar->SetBackGroundColor(AfxGetThemeManager()->GetTabSelectorScrollBarFillBkg());
	m_pVScrollBar->SetBkgButtonNoPressedColor(AfxGetThemeManager()->GetTabSelectorScrollBarBkgButtonNoPressedColor());
	m_pVScrollBar->SetBkgButtonPressedColor(AfxGetThemeManager()->GetTabSelectorScrollBarBkgButtonPressedColor());
	m_pVScrollBar->EnableScrollBar(ESB_ENABLE_BOTH);

	m_nScrollStepY = AfxGetThemeManager()->GetTabSelectorMinHeight();
	m_nScrollWidth = GetSystemMetrics(SM_CXVSCROLL);
	m_pVScrollBar->SetVisible(FALSE);
}

//-----------------------------------------------------------------------------------
void CSelectorButtonContainer::OnSize(UINT nType, int cx, int cy)
{
	__super::OnSize(nType, cx, cy);

	CRect aRect;
	this->GetWindowRect(aRect);
	this->ScreenToClient(aRect);

	if (m_pVScrollBar && m_pVScrollBar->m_hWnd)
		m_pVScrollBar->SetWindowPos(NULL, aRect.right - m_nScrollWidth, aRect.top, m_nScrollWidth, aRect.bottom, SWP_SHOWWINDOW | SWP_NOZORDER);
}

//------------------------------------------------------------------------------
BOOL CSelectorButtonContainer::OnEraseBkgnd(CDC* pDC)
{
	CRect rclientRect;
	this->GetClientRect(rclientRect);
	pDC->FillRect(&rclientRect, AfxGetThemeManager()->GetTabSelectorBkgColorBrush());

	return TRUE;
}

//----------------------------------------------------------------------------------------------
void CSelectorButtonContainer::OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar)
{
	int nMaxPos = m_nRealHeight - m_nScrollStepY;

	switch (nSBCode)
	{
	case SB_LINEUP:
		if (m_nScrollPosY <= 0)
			return;

		m_nScrollPosY -= m_nScrollStepY;
		SetScrollPos(SB_VERT, m_nScrollPosY);
		ScrollWindow(0, m_nScrollStepY);
		break;
	case SB_LINEDOWN:
		if (m_nScrollPosY >= nMaxPos)
			return;

		m_nScrollPosY += m_nScrollStepY;
		SetScrollPos(SB_VERT, m_nScrollPosY);
		ScrollWindow(0, -m_nScrollStepY);
		break;
	default:
		return;
	}

	CRect aRect;
	GetWindowRect(aRect);
	ScreenToClient(aRect);

	if (m_pVScrollBar && m_pVScrollBar->m_hWnd)
		m_pVScrollBar->SetWindowPos(NULL, aRect.right - m_nScrollWidth, aRect.top, m_nScrollWidth, aRect.bottom, SWP_SHOWWINDOW | SWP_NOZORDER);

	RedrawWindow();
}

/////////////////////////////////////////////////////////////////////////////
//			CTabSelector
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CTabSelector, CTabCtrl)

//------------------------------------------------------------------------------
CTabSelector::CTabSelector()
	: 
	m_nCurSel(-1),
	m_ShowMode(NORMAL),
	m_SelectorAppearance(IMAGE_ONLY),
	m_pParentScrollView(NULL),
	m_nSelectorWidth(AfxGetThemeManager()->GetTabSelectorMinWidth()),
	m_nIconHeight(-1),
	m_nIconWidth(-1),
	m_pNormalTabber(NULL),
	m_nPosClickedTab(-1)
{
	m_defaultIconTileGroup = NULL;

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//------------------------------------------------------------------------------
CTabSelector::~CTabSelector()
{
	for (int i = 0; i < m_arSelectors.GetSize(); i++)
	{
		CSelectorButton* pSelector = m_arSelectors.GetAt(i);
		SAFE_DELETE (pSelector);
	}

	if (m_defaultIconTileGroup)
		DestroyIcon(m_defaultIconTileGroup);
	
	SAFE_DELETE(m_pSelectorButtonContainer);
	if (m_pNormalTabber)
		m_pNormalTabber->DestroyWindow();

	SAFE_DELETE(m_pNormalTabber);

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

BEGIN_MESSAGE_MAP(CTabSelector, CTabCtrl)
	ON_WM_SIZE()
	ON_WM_LBUTTONDOWN()
END_MESSAGE_MAP()


//--------------------------------------------------------------------------
BOOL CTabSelector::ProcessSysKeyMessage(MSG* pMsg)
{
	ASSERT(pMsg != NULL);
	ASSERT_VALID(this);

	// gestisco solo acceleratori di tab & selettori
	if (!pMsg || pMsg->message != WM_SYSKEYDOWN || (pMsg->wParam != VK_PRIOR && pMsg->wParam != VK_NEXT && pMsg->wParam != VK_F11 && (pMsg->wParam < 0x30 || pMsg->wParam > 0x5A)))
		return FALSE;

	// Attenzione LPARAM è inutilizzabile in questo contesto (vedi CTaskBuilderTabWnd::PreProcessSysKeyMessage)
	
	CSelectorButton* pSelectorButton;
	switch ((UINT) pMsg->wParam)
	{
		case VK_PRIOR:
		{
			int nActiveTab = GetCurSel();
			do
			{
				if (nActiveTab == 0)
					return FALSE;

				nActiveTab--;
				pSelectorButton = m_arSelectors.GetAt(nActiveTab);
			}	// se si passa per una dialog disabilitata si skippa
			while (!pSelectorButton->m_pDlgInfoItem->IsEnabled() || !pSelectorButton->m_pDlgInfoItem->IsVisible());

			OnTabClick(pSelectorButton);
			return TRUE;
		}
		case VK_NEXT:
		{
			int nActiveTab = GetCurSel();
			do
			{
				if (nActiveTab == m_arSelectors.GetSize() - 1)
				 	return FALSE;
			
			 	nActiveTab++;
				pSelectorButton = m_arSelectors.GetAt(nActiveTab);
			}	// se si passa per una dialog disabilitata si skippa
			while (!pSelectorButton->m_pDlgInfoItem->IsEnabled() || !pSelectorButton->m_pDlgInfoItem->IsVisible());

			OnTabClick(pSelectorButton);
			return TRUE;
		}
		case VK_F11:
		{
			CWnd* pForm = GetParent();
			if (!pForm)
				return FALSE;
	
			CWnd* pWnd = pForm->GetNextDlgTabItem(this, FALSE);
	
			while (pWnd && pWnd->m_hWnd != m_hWnd)
			{
				if (pWnd->IsWindowEnabled()	&& !pWnd->IsKindOf(RUNTIME_CLASS(CStatic)))
				{
					pWnd->SetFocus();
					return TRUE;
				}
	
				pWnd = pForm->GetNextDlgTabItem(pWnd, FALSE);
			}
					
			return FALSE;
		}
		default:
		{
			CString strPattern(_T("&"));
			strPattern += (TCHAR) pMsg->wParam;
						
			for (int i = 0; i < m_arSelectors.GetSize(); i++)
			{
				pSelectorButton = m_arSelectors.GetAt(i); 
				if (!pSelectorButton || !pSelectorButton->m_pDlgInfoItem->IsVisible())
					continue;

				CString strCaption = pSelectorButton->m_pDlgInfoItem->m_strTitle;
				strCaption.MakeUpper();
				if (strCaption.Find(strPattern) != -1)
				{
					if (i == GetCurSel())
						return TRUE;
				
					// se si richiama una dialog disabilitata si da errore
					if (!pSelectorButton->m_pDlgInfoItem->IsEnabled())
					{
						MessageBeep(MB_ICONHAND);
						return TRUE;
					}

					OnTabClick(pSelectorButton);
					return TRUE;
				}
			}
		}
	}

	return FALSE;
}

//--------------------------------------------------------------------------
BOOL CTabSelector::PreTranslateMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	if (!GetSelectorDocument())
		return __super::PreTranslateMessage(pMsg);

	// La PTM di m_pNormalTabber (CTabSelectorTabWnd) si occupa lei di gestire la direzione del forwarding
	if (m_pNormalTabber && m_pNormalTabber->PreTranslateMessage(pMsg))
		return TRUE;
	
	if (m_pSelectorButtonContainer && ProcessSysKeyMessage(pMsg))
		return TRUE;

	if (!GetSelectorDocument()->m_bForwardingSysKeydownToChild)
		return GetParent() && GetParent()->PreTranslateMessage(pMsg) || __super::PreTranslateMessage(pMsg);

	return FALSE;

#else

	return __super::PreTranslateMessage(pMsg);

#endif
}

// Determines the currently selected tab in the tab control.
//------------------------------------------------------------------------------
int CTabSelector::GetCurSel() const
{
	return m_nCurSel;
}

// Selects the specified tab in the tab control.
//------------------------------------------------------------------------------
int CTabSelector::SetCurSel(_In_ int nItem)
{
	if (m_ShowMode == NONE || m_ShowMode == NORMAL)
	{
		m_nCurSel = nItem;
		if (m_pNormalTabber)
			m_pNormalTabber->SetActiveTab(nItem);
		return m_nCurSel;
	}

	int old = m_nCurSel;
	CSelectorButton* pSelector = NULL;
	if (m_nCurSel > -1 && m_nCurSel < m_arSelectors.GetCount())
	{
		pSelector = m_arSelectors.GetAt(m_nCurSel);
		pSelector->SetChecked(FALSE);
		pSelector->Invalidate();
	}

	m_nCurSel = nItem;
	if (m_nCurSel > -1 && m_nCurSel < m_arSelectors.GetCount())
	{
		pSelector = m_arSelectors.GetAt(m_nCurSel);
		pSelector->SetChecked(TRUE);
		pSelector->Invalidate();
	}
	
	return old;
}

//------------------------------------------------------------------------------
int CTabSelector::HitTest(_In_ TCHITTESTINFO* pHitTestInfo) const
{
	if (m_ShowMode == NORMAL) 
	{
		CTabSelectorTabWnd* tabberino = ((CTabSelector*)this)->GetNormalTabber();
		CPoint point = CPoint(pHitTestInfo->pt);
		return tabberino->GetTabFromPoint(point);
	}
	
	if (m_ShowMode == NONE)
		return -1;

	CRect r;
	for (int i = 0; i < m_arSelectors.GetSize(); i++)
	{
		CSelectorButton* pSelectorButton = m_arSelectors.GetAt(i); 
		if (!pSelectorButton)
			continue;
		
		if (!pSelectorButton->m_hWnd)
			continue;
		pSelectorButton->GetWindowRect(r);
		ScreenToClient(r);
		if (r.PtInRect(pHitTestInfo->pt))
		{
			return i;
		}
	}
	return -1;
}

// Removes a tab from the tab control.
//------------------------------------------------------------------------------
BOOL CTabSelector::DeleteItem(_In_ int nItem)
{
	if (m_ShowMode == NORMAL)
		return m_pNormalTabber->RemoveTab(nItem);

	if (m_ShowMode == NONE)
		return FALSE;

	CSelectorButton* pSelectorButton = m_arSelectors.GetAt(nItem);
	delete pSelectorButton;
	m_arSelectors.RemoveAt(nItem);
	
	ArrangeItems();
	
	return __super::DeleteItem(nItem);
}

// Sets some or all attributes of the specified tab in the tab control.
//------------------------------------------------------------------------------
BOOL CTabSelector::SetItem(_In_ int nItem, _In_ TCITEM* pTabCtrlItem)
{
	if (m_ShowMode == NONE)
		return TRUE; 
	if (m_ShowMode == NORMAL && m_pNormalTabber)
	{
		m_pNormalTabber->UpdateTabStates();
		return TRUE;
	}

	if (nItem > m_arSelectors.GetUpperBound() || nItem < 0)
		return FALSE;

	CSelectorButton* pSelectorButton = m_arSelectors.GetAt(nItem);
	pSelectorButton->EnableWindow(pSelectorButton->m_pDlgInfoItem->IsEnabled());
	return TRUE;
}
// Retrieves the number of tabs in the tab control.
//------------------------------------------------------------------------------
int CTabSelector::GetItemCount() const
{
	if (m_ShowMode == NORMAL)
		return m_pNormalTabber ? m_pNormalTabber->GetTabsNum() : 0;

	return m_arSelectors.GetCount();
}

//-----------------------------------------------------------------------------
int CTabSelector::GetSelectorIndex(CSelectorButton* pSelector)
{
	for (int i = 0; i < m_arSelectors.GetCount(); i++)
	{
		CSelectorButton* pSelectorButton = m_arSelectors.GetAt(i);
		if (pSelectorButton == pSelector)
		{
			return i;
		}
	}
	return -1;
}

// Generic creator allowing extended style bits
//------------------------------------------------------------------------------
BOOL CTabSelector::CreateEx(_In_ DWORD dwExStyle, _In_ DWORD dwStyle, _In_ const RECT& rect,
	_In_ CWnd* pParentWnd, _In_ UINT nID)
{
	if (m_ShowMode != NORMAL)
	{
		dwStyle = dwStyle& ~WS_CLIPCHILDREN ;
	}
	m_defaultIconTileGroup = NULL;

	return __super::CreateEx(dwExStyle, dwStyle, rect, pParentWnd, nID); 
}
//------------------------------------------------------------------------------
void CTabSelector::OnSize (UINT nType, int cx, int cy)
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(this->GetParentFrame());
	if (pFrame && pFrame->IsLayoutSuspended())
		return;

    __super::OnSize (nType, cx, cy);	

	if (m_pNormalTabber)
	{
		CRect aRect;
		GetClientRect(aRect);
		aRect.bottom = aRect.top + m_pNormalTabber->GetTabsHeight();
		m_pNormalTabber->MoveWindow(aRect);
	}

	ArrangeItems();
}

//-----------------------------------------------------------------------------
void CTabSelector::OnTabClick(CSelectorButton* pSelector)
{
	int currentSelected = GetCurSel();
	int newSelected = pSelector->GetTabIndex();

	if (currentSelected != -1)
	{
		CSelectorButton* pOldSelector = m_arSelectors.GetAt(currentSelected);
		pOldSelector->Invalidate();
	}
	DoTabClick(newSelected);
}

//-----------------------------------------------------------------------------
void CTabSelector::DoTabClick(int newSelected)
{
	if (m_ShowMode == NONE)
		return;

	int currentSelected = GetCurSel();

	if (currentSelected == newSelected)
	{
		return;
	}

	m_nPosClickedTab = newSelected;

	NMHDR nmh;
	nmh.code = TCN_SELCHANGING;    // Message type defined by control.
	nmh.idFrom = GetDlgCtrlID();
	nmh.hwndFrom = m_hWnd;
	GetParent()->SendMessage(WM_NOTIFY, (WPARAM)m_hWnd, (LPARAM)&nmh);

	// nel caso di selettore devo far scattare tutti
	// i refresh dei bottoni
	if (GetShowMode() == VERTICAL_TILE)
		SetCurSel(newSelected);
	else
		// mentre in caso di normal devo stare solo
		// attenta a non andare in ricorsione
		m_nCurSel = newSelected;
	nmh.code = TCN_SELCHANGE;
	GetParent()->SendMessage(WM_NOTIFY, (WPARAM)m_hWnd, (LPARAM)&nmh);

}

//------------------------------------------------------------------------------
CScrollView* CTabSelector::GetParentScrollView()
{
	if (m_pParentScrollView)
		return m_pParentScrollView;

	CWnd* pParentView = GetParent();
	//Risale fino alla scrollview
	while (pParentView != NULL && !pParentView->IsKindOf(RUNTIME_CLASS(CScrollView)))
	{
		pParentView = pParentView->GetParent();
	}
	if (pParentView != NULL)
	{
		m_pParentScrollView = (CScrollView*)pParentView;
		return m_pParentScrollView;
	}
	return NULL;
}

//------------------------------------------------------------------------------
int CTabSelector::CalcSelectorButtonHeightForWordWrap(CSelectorButton* pSelectorButton)
{
	//calcola la dimensione del testo, e se si accorge che il testo va a capo, aumenta lievemente la dimensione del bottone per non avere testo e immagine schiacciati
	CDC* pDC = pSelectorButton->GetWindowDC();
	CString strText;
	pSelectorButton->GetWindowText(strText);
	CSize size = GetTextSize(pDC, strText, pDC->GetCurrentFont());

	ReleaseDC(pDC);

	CRect textArea;
	pSelectorButton->GetTextMargin(&textArea);
	int selectorHeight;
	if (size.cx > textArea.Width())
		selectorHeight = max(selectorIconSize + pSelectorButton->GetCaptionRect().Height(), AfxGetThemeManager()->GetTabSelectorMinHeight() + wordWrapDisplacement);
	else
		selectorHeight = max(selectorIconSize + pSelectorButton->GetCaptionRect().Height(), AfxGetThemeManager()->GetTabSelectorMinHeight());
	return selectorHeight;

}

// Calcola la larghezza del selettore verticale, in base alla larghezza della caption del bottone piu largo.
//-----------------------------------------------------------------------------
void CTabSelector::CalculateSelectorWidth()
{
	m_nSelectorWidth = AfxGetThemeManager()->GetTabSelectorMinWidth();
	for (int i = 0; i < m_arSelectors.GetCount(); i++)
		m_nSelectorWidth = max(m_nSelectorWidth, min(m_arSelectors.GetAt(i)->GetCaptionRect().Width(), AfxGetThemeManager()->GetTabSelectorMaxWidth()));

	int labelY = 0;

	for (int i = 0; i < m_arSelectors.GetCount(); i++)
	{
		CSelectorButton* pSelectorButton = m_arSelectors.GetAt(i);
		if (pSelectorButton)
		{
			int selectorHeight = CalcSelectorButtonHeightForWordWrap(pSelectorButton);
			pSelectorButton->MoveWindow(0, labelY, m_nSelectorWidth, selectorHeight);
			labelY += selectorHeight;
		}
	}

	if (!m_pSelectorButtonContainer)
		return;

	m_pSelectorButtonContainer->SetRealHeight(labelY);
}

//------------------------------------------------------------------------------
void CTabSelector::ArrangeItems()
{
	CRect rect;
	GetWindowRect(rect);

	CRect rectScrolled(0, 0, 0, 0);
	if (m_ShowMode == VERTICAL_TILE)
	{
		if (m_pSelectorButtonContainer)
		{
			int nRealHeight = m_pSelectorButtonContainer->GetRealHeight();
			if (nRealHeight > 0 && rect.Height() > 0 && rect.Height() < nRealHeight && !m_pSelectorButtonContainer->GetVScrollBar())
			{
				m_nSelectorWidth += GetSystemMetrics(SM_CXVSCROLL);
				m_pSelectorButtonContainer->CreateAccessories();
				m_pSelectorButtonContainer->SetScrollVisible(TRUE);
			}
			m_pSelectorButtonContainer->MoveWindow(0, 0, m_nSelectorWidth, rect.Height());
		}
		CWnd* pChild = GetWindow(GW_CHILD);

		while (pChild)
		{
			if (pChild->IsKindOf(RUNTIME_CLASS(CBaseTabDialog))/* && ::IsTBWindowVisible(pChild)*/) //In fase di avvio di un documento "untabbed" il frame non e' acnora visibile, ma la move va fatta comunque
			{
				pChild->MoveWindow(m_nSelectorWidth, 0, rect.Width() - m_nSelectorWidth, rect.Height());
			}
		
			pChild = pChild->GetWindow(GW_HWNDNEXT);
		}
	}
	else if (m_ShowMode == NONE)
	{
		CWnd* pChild = GetWindow(GW_CHILD);
		while (pChild)
		{
			if (pChild->IsKindOf(RUNTIME_CLASS(CBaseTabDialog)))
				pChild->MoveWindow(0, 0, rect.Width(), rect.Height());
		
			pChild = pChild->GetWindow(GW_HWNDNEXT);
		}
	}
}

//-----------------------------------------------------------------------------
int CTabSelector::GetTabIndex(CPoint point)
{
	CRect r;
	for (int i = 0; i < m_arSelectors.GetSize(); i++)
	{
		CSelectorButton* pSelectorButton = m_arSelectors.GetAt(i);
		if (!pSelectorButton)
			continue;

		pSelectorButton->GetWindowRect(r);
		ScreenToClient(r);
		if (r.PtInRect(point))
		{
			return i;
		}
	}
	return -1;
}

//-----------------------------------------------------------------------------
void CTabSelector::ChangeImage(int index, CString strImagePath)
{
	if (index < 0)
		return;

	if (m_ShowMode == VERTICAL_TILE)
	{
		CSelectorButton* pButton = m_arSelectors.GetAt(index);
		pButton->ChangeImage(strImagePath);
	}
}

//-----------------------------------------------------------------------------
CSelectorButtonContainer*	CTabSelector::GetSelectorButtonContainer()
{
	CRect rect;
	GetClientRect(&rect);
	if (!m_pSelectorButtonContainer)
	{
		m_pSelectorButtonContainer = new CSelectorButtonContainer();
		UINT nIDC = AfxGetTBResourcesMap()->GetTbResourceID(_T("SelectorButtonContainer"), TbResourceType::TbControls);
		m_pSelectorButtonContainer->Create(NULL, NULL, WS_VISIBLE | WS_CHILD, CRect(0, 0, m_nSelectorWidth, rect.bottom), this, nIDC);
	}
	return m_pSelectorButtonContainer;
}

//-----------------------------------------------------------------------------
CSelectorButton*  CTabSelector::CreateSelector(int index, DlgInfoItem* pDlgInfoItem, CString strImagePath, CString strTooltip, CString strTooltipDescription /*= _T("")*/)
{
	CSelectorButton* pSelectorButton = new CSelectorButton(this, GetSelectorButtonContainer());

	if (!m_defaultIconTileGroup)
		m_defaultIconTileGroup = TBLoadImage(TBIcon(szTileGroup, IconSize::TILEMNG));

	if (strTooltip.IsEmpty())
		strTooltip = pDlgInfoItem->m_strTitle;

	pSelectorButton->Create(pDlgInfoItem, strImagePath, strTooltip, m_defaultIconTileGroup, strTooltipDescription);

	//font più piccolo
	pSelectorButton->SetFont(AfxGetThemeManager()->GetTabSelectorButtonFont());

	m_arSelectors.InsertAt(index, pSelectorButton);
	return pSelectorButton;
}

//---------------------------------------------------------------------------------------------------------------------------------
void CTabSelector::MoveButtonSelector(int indexNew, int indexOld)
{
	if (indexOld < 0 || indexOld > m_arSelectors.GetSize() - 1)
		return;

	if (indexNew < 0 || indexNew > m_arSelectors.GetSize() - 1)
		return;

	CSelectorButton* pSelectorButtonOld = m_arSelectors.GetAt(indexOld);
	
	if (!pSelectorButtonOld)
		return;
	
	m_arSelectors.RemoveAt(indexOld);

	if (indexNew < m_arSelectors.GetSize())
		m_arSelectors.InsertAt(indexNew, pSelectorButtonOld);
	else
		m_arSelectors.Add(pSelectorButtonOld);


	m_arSelectors.SetAt(indexNew, pSelectorButtonOld);

	//reset SetCheck
	int i = 0;
	for (; i < m_arSelectors.GetCount(); i++)
	{
		CSelectorButton* pButton = m_arSelectors.GetAt(i);
		if (!pButton)
			continue;
		pButton->SetChecked(FALSE);
	}
}

//-----------------------------------------------------------------------------
int CTabSelector::InsertDlgInfoItem(int i, DlgInfoItem* pdlginfo)
{
	if (m_ShowMode == NORMAL && pdlginfo->IsVisible())
	{
		CTabSelectorTabWnd* pTabWnd = GetNormalTabber();
		if (!pTabWnd)
		{
			CreateNormalTabber();
			pTabWnd = GetNormalTabber();
		}
		pTabWnd->AddTabByInfo(new CTabSelectorTabWndInfo(pdlginfo), i);
	}
	else if (m_ShowMode == VERTICAL_TILE)
		CreateSelector(i, pdlginfo, pdlginfo->GetSelectorImage(), pdlginfo->GetSelectorTooltip());

	return i;
};

//-----------------------------------------------------------------------------
void  CTabSelector::CreateNormalTabber()
{
	if (!m_pNormalTabber && m_ShowMode == NORMAL)
	{
		m_pNormalTabber = new CTabSelectorTabWnd();
		m_pNormalTabber->Create(this);
		m_pNormalTabber->AttachDocument(GetSelectorDocument());
	}
}

//-----------------------------------------------------------------------------
CTabSelectorTabWnd* CTabSelector::GetNormalTabber()
{
	return m_pNormalTabber;
}

//-----------------------------------------------------------------------------
void CTabSelector::OnUpdateTabStates()
{
	if (m_pNormalTabber)
		m_pNormalTabber->UpdateTabStates();
}

//-----------------------------------------------------------------------------
void CTabSelector::GetTabsWindowRect(CRect& rect)
{
	if (m_pNormalTabber)
		m_pNormalTabber->GetWindowRect(rect);
}

//-----------------------------------------------------------------------------
void CTabSelector::UpdateSelector()
{
	for (int i = 0; i < m_arSelectors.GetSize(); i++)
	{
		CSelectorButton* pSelectorButton = m_arSelectors.GetAt(i);
		if (pSelectorButton)
			pSelectorButton->UpdateText();

	}
	CalculateSelectorWidth();
}

//-----------------------------------------------------------------------------
HRESULT CTabSelector::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = GetRanorexNamespace();
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}

//-----------------------------------------------------------------------------
CString CTabSelector::GetRanorexNamespace()
{
	return _T("CTabSelector");
}

///////////////////////////////////////////////////////////////////////////////////////////////////
//									CSelectorButton
///////////////////////////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CSelectorButton, CBCGPButton)

BEGIN_MESSAGE_MAP(CSelectorButton, CBCGPButton)
	ON_CONTROL_REFLECT(BN_CLICKED, &CSelectorButton::OnBnClicked)
	ON_WM_SETCURSOR		()
	ON_WM_ERASEBKGND	()
	ON_WM_PAINT			()
END_MESSAGE_MAP()

CSelectorButton::CSelectorButton(CTabSelector* pTabber, CSelectorButtonContainer*	pSelectorButtonContainer)
	:
	m_pTabber(pTabber),
	m_pDlgInfoItem(NULL),
	m_strIconSource(_T("")),
	m_pSelectorButtonContainer(pSelectorButtonContainer)
{
	TBThemeManager* pManager = AfxGetThemeManager();
	m_clrFace = pManager->GetTabSelectorBkgColor();
	m_clrHover = pManager->GetTabSelectorHoveringForeColor();

	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//------------------------------------------------------------------------------
CSelectorButton::~CSelectorButton()
{
	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//------------------------------------------------------------------------------
BOOL CSelectorButton::OnEraseBkgnd(CDC* pDC)
{
	return __super::OnEraseBkgnd(pDC);
}

//------------------------------------------------------------------------------
BOOL CSelectorButton::PreTranslateMessage(MSG* pMsg)
{
	switch (pMsg->message)
	{
	case WM_KEYDOWN:
	case WM_SYSKEYDOWN:
	case WM_LBUTTONDOWN:
	case WM_RBUTTONDOWN:
	case WM_MBUTTONDOWN:
	case WM_LBUTTONUP:
	case WM_RBUTTONUP:
	case WM_MBUTTONUP:
	case WM_MOUSEMOVE:
		m_ToolTip.RelayEvent(pMsg);
		break;
	}
	
	return CBCGPButton::PreTranslateMessage(pMsg);
}
	
//------------------------------------------------------------------------------
int CSelectorButton::GetTabIndex() 
{ 
	return m_pTabber->GetSelectorIndex(this); 
}

//------------------------------------------------------------------------------
void CSelectorButton::SetChecked(BOOL bChecked)
{
	m_bChecked = bChecked;
	m_bTransparent = !m_bChecked;
	TBThemeManager* pManager = AfxGetThemeManager();
	SetFaceColor((m_bChecked) ? pManager->GetTabSelectorSelectedBkgColor() : pManager->GetTabSelectorBkgColor());
	if (!m_pDlgInfoItem || !pManager->IsToAddMoreColor() || m_pDlgInfoItem->GetSelectorImage().IsEmpty())
		return;

	CTBNamespace aNs(CTBNamespace::IMAGE, m_pDlgInfoItem->GetSelectorImage());
	if (bChecked)
	{
		CString sName = aNs.GetObjectName();
		CString sNewNamespace = aNs.ToString();
		sNewNamespace = sNewNamespace.Left(sNewNamespace.GetLength() - sName.GetLength());
		aNs.SetNamespace(sNewNamespace + GetName(sName) + _T("_c") + GetExtension(sName));
	}
		
	ChangeImage(aNs.ToString());
}

//------------------------------------------------------------------------------
void CSelectorButton::OnBnClicked()
{
	if (!m_pTabber)
		return;

	// Il bottone del selettore "ruba" il fuoco al contro che lo ha nel momento in cui si clicca
	int currentSelected = m_pTabber->GetCurSel();
	int newSelected = GetTabIndex();
	if (currentSelected == newSelected && m_pDlgInfoItem && m_pDlgInfoItem->m_pBaseTabDlg && m_pDlgInfoItem->m_pBaseTabDlg->GetLastFocusedCtrl())
	{
		m_pDlgInfoItem->m_pBaseTabDlg->GetLastFocusedCtrl()->SetFocus();
		return;
	}

	m_pTabber->OnTabClick(this);
}

//------------------------------------------------------------------------------
void CSelectorButton::ChangeImage(CString strImagePath, HICON defaultIconTileGroup /*= NULL*/)
{
	if (AfxIsInUnattendedMode())
		return;

	HICON hIcon = NULL;
	
	if (strImagePath.IsEmpty())
	{
		if (!defaultIconTileGroup)
		{
			hIcon = TBLoadImage(TBIcon(szTileGroup, IconSize::TILEMNG));
			this->SetIcon(hIcon);
		}
		else
			this->SetIcon(defaultIconTileGroup);
	}
	else
	{
		CDC*  pDC = GetDC();
		hIcon = TBLoadImage(strImagePath, pDC, 0);
		if (hIcon)
			this->SetIcon(hIcon);
		if (pDC)
			ReleaseDC(pDC);
	}
	
	if (!m_pTabber->IsIconSizeSet())
	{
		PICONINFO pIconInfo = new ICONINFO();
		GetIconInfo(hIcon, pIconInfo);
		BITMAP* pBitmap = new BITMAP();
		GetObject(pIconInfo->hbmMask, sizeof(BITMAP), pBitmap);
		m_pTabber->SetIconSize(pBitmap->bmWidth, pBitmap->bmHeight);
	
		DeleteObject(pIconInfo->hbmMask);
		DeleteObject(pIconInfo->hbmColor);
	
		delete pIconInfo;
		delete pBitmap;
	}

	DestroyIcon(hIcon);
	m_strIconSource = strImagePath;
	this->Invalidate(TRUE);
}

//------------------------------------------------------------------------------
void CSelectorButton::OnPaint()
{
	CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente

	const BOOL bIsRadio = m_bRadioButton && !m_bAutoCheck;

	if (!m_b3State && !bIsRadio)
	{
		Default();
		return;
	}
	
	CRect rectClient;
	GetClientRect(&rectClient);

	DoDrawItem(&dc, rectClient, (CWnd::GetFocus() == this) ? ODS_FOCUS : 0);
}

//------------------------------------------------------------------------------
void CSelectorButton::DoDrawItem(CDC* pDCPaint, CRect rectClient, UINT itemState)
{
	CDC* pDC = GetWindowDC();

	m_clrText = (COLORREF)-1;

	BOOL bDefaultDraw = TRUE;
	BOOL bDefaultCheckRadio = (m_bCheckButton || m_bRadioButton) && (GetStyle() & BS_PUSHLIKE) == 0;

	if (((m_bVisualManagerStyle && !m_bDontSkin) || bDefaultCheckRadio) && !m_bTransparent)
	{
		if (CBCGPVisualManager::GetInstance()->OnDrawPushButton(pDC, rectClient, this, m_clrText))
		{
			if (m_clrFace != -1 && globalData.m_nBitsPerPixel > 8)
			{
				CBCGPDrawManager dm(*pDC);

				CRect rectHighlight = rectClient;
				int nPercentage = 50;
				BOOL bIsFocused = (itemState & ODS_FOCUS);

				if (IsHighlighted() || IsPressed() || IsChecked() || bIsFocused)
				{
					rectHighlight.DeflateRect(2, 2);
					nPercentage = 20;
				}
				else if (m_bOnGlass)
				{
					rectHighlight.DeflateRect(1, 1);
				}

				dm.HighlightRect(rectHighlight, nPercentage, (COLORREF)-1, 0, m_clrFace);
			}

			rectClient.DeflateRect(2, 2);
			bDefaultDraw = FALSE;
		}
	}

	if (bDefaultDraw)
	{
		OnFillBackground(pDC, rectClient);
		OnDrawBorder(pDC, rectClient, itemState);
	}

	//---------------------
	// Draw button content:
	//---------------------
	OnDraw(pDC, rectClient, itemState);

	if ((itemState & ODS_FOCUS) && ((itemState & ODS_NOFOCUSRECT) == 0) && m_bDrawFocus)
	{
		OnDrawFocusRect(pDC, rectClient);
	}

	ReleaseDC(pDC);
}

//****************************************************************************
void CSelectorButton::OnFillBackground(CDC* pDC, const CRect& rectClient)
{
	if (m_bTransparent)
	{
		// Copy background from the parent window
		globalData.DrawParentBackground(this, pDC);
	}
	else
	{
		if (m_clrFace == (COLORREF)-1)
		{
			pDC->FillRect(rectClient, &globalData.brBtnFace);
		}
		else
		{
			pDC->FillSolidRect(rectClient, m_clrFace);
		}
	}

	if (m_bChecked && m_bHighlightChecked && !(m_bPushed && m_bHighlighted))
	{
		CBCGPDrawManager dm(*pDC);
		dm.HighlightRect(rectClient);
	}
}

//------------------------------------------------------------------------------
void CSelectorButton::Create(DlgInfoItem* pDlgInfoItem, CString strImagePath, CString strTooltip, HICON defaultIconTileGroup /*= NULL*/, CString strTooltipDescription)
{
	__super::Create(pDlgInfoItem->m_strTitle, WS_VISIBLE | WS_CHILD | BS_OWNERDRAW | BS_MULTILINE, CRect(0, 0, AfxGetThemeManager()->GetTabSelectorMinWidth(), AfxGetThemeManager()->GetTabSelectorMinHeight()), m_pSelectorButtonContainer, IDC_STATIC);
		
	m_pDlgInfoItem = pDlgInfoItem;
	SetFont(AfxGetThemeManager()->GetControlFont());
	m_nFlatStyle = (CBCGPButton::FlatStyle) AfxGetThemeManager()->GetTileSelectorButtonStyle();
	m_bDontUseWinXPTheme = TRUE;
	m_bVisualManagerStyle = TRUE;
	EnableWindow(pDlgInfoItem->IsEnabled());

	m_ToolTip.Init(this, strTooltip, strTooltipDescription);

	if (m_pTabber->GetSelectorAppearance() != CTabSelector::TEXT_ONLY)
	{
		m_bTopImage = TRUE;
		m_bTransparent = TRUE;
		
		ChangeImage(strImagePath, defaultIconTileGroup);
	}
}

//------------------------------------------------------------------------------
void CSelectorButton::UpdateText()
{
	if (m_pDlgInfoItem)
		SetWindowText(m_pDlgInfoItem->m_strTitle);
}

//-----------------------------------------------------------------------------
HRESULT CSelectorButton::get_accName(VARIANT varChild, BSTR *pszName)
{
	// the selector button do not have a separate namespace from the tab dialog
	CString sNamespace = cwsprintf(_T("{0-%s}SelectorButton"), m_pDlgInfoItem->GetNamespace().GetObjectName());
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
//			CSelectorButtonToolTip
/////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(CSelectorButtonToolTip, CBCGPToolTipCtrl)
	//{{AFX_MSG_MAP(CCustomToolTipCtrl)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
	ON_NOTIFY_REFLECT(TTN_POP, OnPop)
END_MESSAGE_MAP()

//------------------------------------------------------------------------------
CSelectorButtonToolTip::CSelectorButtonToolTip()
{
	m_TooltipParams.m_bVislManagerTheme = TRUE;
	m_TooltipParams.m_bBoldLabel = TRUE;
	m_TooltipParams.m_bDrawDescription = TRUE;
	m_TooltipParams.m_bDrawIcon = TRUE;

	this->SetParams (&m_TooltipParams);
}

//------------------------------------------------------------------------------
void CSelectorButtonToolTip::OnPop(NMHDR* pNMHDR, LRESULT* pResult)
{
	CString tempDescription = m_strDescription;
	__super::OnPop(pNMHDR, pResult);

	m_strDescription = tempDescription;
}
//------------------------------------------------------------------------------
void CSelectorButtonToolTip::Init(CWnd* ownerWnd, CString strTitle, CString strDescription)
{
	if (!AfxIsRemoteInterface())
	{
		Create(ownerWnd);
		Activate(TRUE);
		AddTool(ownerWnd, strTitle);
	}
		
	if (strTitle.CompareNoCase(strDescription))
		SetDescription(strDescription);
}

