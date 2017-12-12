#include "stdafx.h"

#include "OslBaseInterface.h"
#include "TBTabWnd.h"
#include <TbGenlib\BaseTileManager.h>
#include <TbGes\BODYEDIT.H>
#include <TbGes\JsonFormEngineEx.h>

#include "TilePanel.h"


#include <TbGes\StatusTile.hjson>

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
BOOL DefaultOnEraseBkgnd(CWnd* pWnd, CDC* pDC)
{
	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(pWnd->GetParentFrame());

	CRect rclientRect;
	pWnd->GetClientRect(rclientRect);

	if (!pFrame)
	{
		pDC->FillRect(&rclientRect, AfxGetThemeManager()->GetTileGroupBkgColorBrush());
		return FALSE;
	}

	BOOL bClipped = FALSE;
	if (pFrame && pFrame->IsLayoutSuspended())
	{
		CWnd* pCtrl = pWnd->GetWindow(GW_CHILD);
		for (; pCtrl; pCtrl = pCtrl->GetNextWindow())
		{
			CRect screen;
			pCtrl->GetWindowRect(&screen);
			pWnd->ScreenToClient(&screen);
			pDC->ExcludeClipRect(&screen);
		}

		bClipped = TRUE;
	}

	pDC->FillRect(&rclientRect, AfxGetThemeManager()->GetTileGroupBkgColorBrush());

	return bClipped;
}


//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CTilePanelTab, CWnd)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTilePanelTab, CWnd)
	ON_WM_WINDOWPOSCHANGED	()
	ON_WM_WINDOWPOSCHANGING	()
	ON_WM_ERASEBKGND()
	ON_MESSAGE				(UM_VALUE_CHANGED,	OnValueChanged)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTilePanelTab::CTilePanelTab(CTilePanel* pOwner)
	:
	m_pOwner	(pOwner),
	m_bEnabled	(TRUE)
{
	m_pLayoutContainer = new CLayoutContainer(m_pOwner, m_pOwner->m_pTileDialogStyle);
	m_pLayoutContainer->SetRequestedLastFlex(1);
}

//-----------------------------------------------------------------------------
CTilePanelTab::~CTilePanelTab()
{
	SAFE_DELETE (m_pLayoutContainer);
}

//-----------------------------------------------------------------------------
HRESULT CTilePanelTab::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), GetNamespace().GetObjectName(), GetNamespace().GetTypeString());
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}

//--------------------------------------------------------------------------
BOOL CTilePanelTab::PreTranslateMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	if (!GetParentTileGroup() || !GetParentTileGroup()->m_pDocument)
		return __super::PreTranslateMessage(pMsg);

	if (GetParentTileGroup()->m_pDocument->m_bForwardingSysKeydownToChild)
		return FALSE;

	return CTaskBuilderTabWnd::PreProcessSysKeyMessage(pMsg, GetParentTileGroup()->m_pDocument, this) || __super::PreTranslateMessage(pMsg);

#else

	return __super::PreTranslateMessage(pMsg);

#endif
}

//-----------------------------------------------------------------------------
BOOL CTilePanelTab::OnEraseBkgnd(CDC* pDC)
{
	if (!DefaultOnEraseBkgnd(this, pDC))
	{
		CWnd* pWndChild = GetWindow(GW_CHILD);
		while (pWndChild != NULL)
		{
			CBaseTileDialog* pDialog = dynamic_cast<CBaseTileDialog*>(pWndChild);
			if (pDialog)
			{
				BOOL bSkip = FALSE;
				ControlLinks* pControlLinks = pDialog->GetControlLinks();
				if (pControlLinks)
				{
					for (int i = 0; i < pControlLinks->GetSize(); i++)
					{
						CBodyEdit* pwndChild = dynamic_cast<CBodyEdit*>(pControlLinks->GetAt(i));
						if (pwndChild)
							bSkip = TRUE;
					}
				}

				if (!bSkip)
					pDialog->Invalidate();

			}
			pWndChild = pWndChild->GetNextWindow();
		}
	}

	return TRUE;

}

//-----------------------------------------------------------------------------
void CTilePanelTab::Enable(BOOL bEnable /*= TRUE*/)
{ 
	m_bEnabled = bEnable;
	CTilePanelTabber* pParent = (CTilePanelTabber*) GetParent();
	pParent->OnTabEnableChanged(this, bEnable);
}

//---------------------------------------------------------------------------------------
void CTilePanelTab::SetContentEnable(const LayoutElementArray* pElements, BOOL bEnable)
{
	for (int i = 0; i < pElements->GetCount(); i++)
	{
		LayoutElement* pElem = pElements->GetAt(i);

		if (!pElem)
			continue;

		if (pElem->GetContainedElements() && pElem->GetContainedElements()->GetSize())
			SetContentEnable(pElem->GetContainedElements(), bEnable);
		else
		{
			CTileDialog* pTile = dynamic_cast<CTileDialog*>(pElem);
			
			if (!pTile)
				continue;

			pTile->EnableTileDialogControlLinks(bEnable);
		}
	}
}

//----------------------------------------------------------------------------
void CTilePanelTab::EnableContent(BOOL bEnable)
{
	for (int i = 0; i < GetContainedElements()->GetCount(); i++)
	{
		LayoutElement* pElem = GetContainedElements()->GetAt(i);

		if (!pElem)
			continue;

		if (pElem->GetContainedElements() && pElem->GetContainedElements()->GetSize())
			SetContentEnable(pElem->GetContainedElements(), bEnable);
		else
		{
			CTileDialog* pTile = dynamic_cast<CTileDialog*>(pElem);
			if (!pTile)
				continue;

			pTile->EnableTileDialogControlLinks(bEnable);
		}
	}

	CAbstractFormDoc* pDoc = NULL;

	CTileGroup* pGroup = dynamic_cast<CTileGroup*>(m_pOwner->GetParent());

	if (!pGroup)
		return;

	pDoc = dynamic_cast<CAbstractFormDoc*>(pGroup->GetDocument());

	if (!pDoc)
		return;

	if (bEnable)
		pDoc->DispatchDisableControlsForBatch();
}

//-----------------------------------------------------------------------------
TileStyle* CTilePanelTab::GetTileDialogStyle()
{ 
	return m_pOwner->GetTileDialogStyle(); 
}

//-----------------------------------------------------------------------------
CBaseTileGroup* CTilePanelTab::GetParentTileGroup()
{ 
	return m_pOwner->m_pParent; 
}

//------------------------------------------------------------------------------
BOOL CTilePanelTab::OnCommand(WPARAM wParam, LPARAM lParam)
{
	if (__super::OnCommand(wParam, lParam))
		return TRUE;

	CWnd* pParent = GetParent();
	return pParent ? pParent->SendMessage(WM_COMMAND, wParam, lParam) : FALSE;
}

//-----------------------------------------------------------------------------
LRESULT CTilePanelTab::OnValueChanged	(WPARAM wParam, LPARAM lParam)
{ 
	return GetParent()->SendMessage(UM_VALUE_CHANGED, wParam, lParam);	
}

//-----------------------------------------------------------------------------
void CTilePanelTab::OnWindowPosChanging(WINDOWPOS* lpwndpos)
{
	// questo flag consente alle MoveWindow e SetWindowPos
	// inviate da sotto dai BCG e non pilotate da noi di forzare
	// la chiamata al WindowPosChanged che fa la Relayout
	lpwndpos->flags |= SWP_FRAMECHANGED;
}

//-----------------------------------------------------------------------------
void CTilePanelTab::OnWindowPosChanged(WINDOWPOS* lpwndpos)
{
	__super::OnWindowPosChanged(lpwndpos);
	CRect aRect(0, 0, lpwndpos->cx, lpwndpos->cy);
	Relayout(aRect);
}

//-----------------------------------------------------------------------------
CWnd* CTilePanelTab::GetWndLinkedCtrl(const CTBNamespace& aNS)
{
	CWnd* pWndChild = GetWindow(GW_CHILD);
	while (pWndChild != NULL)
	{
		CBaseTileDialog* pDialog = dynamic_cast<CBaseTileDialog*>(pWndChild);
		if (pDialog && pDialog->GetNamespace().GetObjectName() == aNS.GetObjectName())
			return pDialog;
		
		pWndChild = pWndChild->GetNextWindow();
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CWnd* CTilePanelTab::GetWndLinkedCtrl(UINT nIDC)
{
	CWnd* pWndChild = GetWindow(GW_CHILD);
	while (pWndChild != NULL)
	{
		CBaseTileDialog* pDialog = dynamic_cast<CBaseTileDialog*>(pWndChild);
		if (pDialog && pDialog->GetDlgCtrlID() == nIDC)
			return pDialog;

		pWndChild = pWndChild->GetNextWindow();
	}
	return NULL;
}


/////////////////////////////////////////////////////////////////////////////
//						CTilePanelTabber 
/////////////////////////////////////////////////////////////////////////////
////-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTilePanelTabber, CTaskBuilderTabWnd)
	ON_MESSAGE		(UM_VALUE_CHANGED,	OnValueChanged)
	ON_WM_WINDOWPOSCHANGED()
	ON_WM_ERASEBKGND()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
void CTilePanelTabber::OnWindowPosChanged(WINDOWPOS* lpwndpos)
{
	__super::OnWindowPosChanged(lpwndpos);
	CRect aRect(0, 0, lpwndpos->cx, lpwndpos->cy - GetTabsHeight());
	CTilePanelTab* pActiveTab = GetActiveTilePanelTab();
	if (pActiveTab)
		pActiveTab->Relayout(aRect);
}

//-----------------------------------------------------------------------------
CTilePanelTab* CTilePanelTabber::GetActiveTilePanelTab()
{
	int nTab = GetActiveTab();
	if (nTab < 0)
		return NULL;

	return (CTilePanelTab*)GetTabWnd(nTab);
}

//-------------------------------------------------------------------------------
void CTilePanelTabber::DrawTabDot(CDC* pDC, CBCGPTabInfo* pTab, BOOL bActive)
{
	int			oldHighlighted	= m_iHighlighted;
	CTilePanel* pPanel			= dynamic_cast<CTilePanel*>(GetParent());

	if (pPanel)
	{
		CTilePanelTab* pTab = pPanel->GetTab(m_iHighlighted);

		if (!pTab || !pTab->IsEnabled())
			m_iHighlighted = -1;
	}
	
	__super::DrawTabDot(pDC, pTab, bActive);

	m_iHighlighted = oldHighlighted;
}

//-----------------------------------------------------------------------------
CTilePanelTabber::CTilePanelTabber(CTilePanel* pOwner)
{ 
	m_pOwner = pOwner; 
	m_bActivateLastActiveTab = TRUE;
}

//-----------------------------------------------------------------------------
void CTilePanelTabber::CleanUp()
{
	for (int t = GetTabsNum() - 1; t >= 0; t--)
	{
		CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(GetTabWnd(t));
		if (pTab)
		{
			RemoveTab(t, FALSE);
			delete pTab;

		}
	}
	__super::CleanUp();
}

//-----------------------------------------------------------------------------
BOOL CTilePanelTabber::OnEraseBkgnd(CDC* pDC)
{
	return DefaultOnEraseBkgnd(this, pDC);
}

//-----------------------------------------------------------------------------
LRESULT CTilePanelTabber::OnValueChanged	(WPARAM wParam, LPARAM lParam)
{ 
	return GetParent()->SendMessage(UM_VALUE_CHANGED, wParam, lParam);	
}

//------------------------------------------------------------------------------
BOOL CTilePanelTabber::OnCommand(WPARAM wParam, LPARAM lParam)
{
	if (__super::OnCommand(wParam, lParam))
		return TRUE;

	CWnd* pParent = GetParent();
	return pParent ? pParent->SendMessage(WM_COMMAND, wParam, lParam) : FALSE;
}

//-----------------------------------------------------------------------------
void CTilePanelTabber::OnTabEnableChanged(CTilePanelTab* pTab, BOOL bEnable)
{
	int iTab = GetTabFromHwnd(pTab->GetSafeHwnd());
	SetTabTextColor(bEnable ? (COLORREF)-1 : AfxGetThemeManager()->GetDisabledControlForeColor());
	if (!bEnable && GetActiveTab() == iTab)
		SetActiveTab(iTab);
}

//-----------------------------------------------------------------------------
BOOL CTilePanelTabber::SetActiveTab(int iTab)
{ 
	CTilePanelTab* pWnd = dynamic_cast<CTilePanelTab*>(GetTabWnd(iTab));
	// percorso di tab abilitata normale
	if (!pWnd || pWnd->IsEnabled())
		return __super::SetActiveTab(iTab);

	// se m_bUserSelectedTab vuol dire che l'utente sta cliccando
	// la linguetta del tabber e non e' una richiesta programmativa
	if (GetTabsNum() > 1 && !m_bUserSelectedTab)
	{
		for (int i = 1; i <= GetTabsNum() - 1; i++)
		{
			int t = (iTab + i) % GetTabsNum();
			CTilePanelTab* pCurrTab = dynamic_cast<CTilePanelTab*>(GetTabWnd(t));
			if (pCurrTab && pCurrTab->IsEnabled())
				return __super::SetActiveTab(t);
		}
	}
	// if no other tabs enabled to activate, the current one remains visible and inactive
	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
//						CTilePanel 
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE (CTilePanel, CBCGPWnd)

BEGIN_MESSAGE_MAP(CTilePanel, CBCGPWnd)
	ON_WM_PAINT				()
	ON_WM_WINDOWPOSCHANGED	()
	ON_WM_LBUTTONDOWN		()	
	ON_MESSAGE		(UM_VALUE_CHANGED,	OnValueChanged)
	ON_MESSAGE		(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
	ON_REGISTERED_MESSAGE(BCGM_CHANGE_ACTIVE_TAB, OnChangeActiveTab)
	ON_REGISTERED_MESSAGE(BCGM_CHANGING_ACTIVE_TAB, OnChangingActiveTab)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTilePanel::CTilePanel()
	:
	Tile				(AfxGetTilePanelStyleNormal()),
	IDisposingSourceImpl(this),
	IOSLObjectManager	(OSLType_TilePanel),
	m_pParent			(NULL),
	m_pTileDialogStyle	(NULL),
	m_pTabber			(NULL),
	m_DefaultLayoutType	(CLayoutContainer::HBOX), 
	m_DefaultLayoutAlign(CLayoutContainer::STRETCH)
{
	if (AfxGetApplicationContext()->IsActiveAccessibilityEnabled())
		EnableActiveAccessibility();
}

//-----------------------------------------------------------------------------
CTilePanel::~CTilePanel()
{
	SAFE_DELETE(m_pTabber);
	SAFE_DELETE(m_pTileDialogStyle);

	if (m_pProxy != NULL)
	{
		//force disconnect accessibility clients
		::CoDisconnectObject((IAccessible*)m_pProxy, NULL);
		m_pProxy = NULL;
	}
}

//--------------------------------------------------------------------------
LRESULT CTilePanel::OnChangeActiveTab(WPARAM wParam, LPARAM lParam)
{
	return 	GetParent()->GetParent()->SendMessage (BCGM_CHANGE_ACTIVE_TAB, wParam, lParam);
}

//--------------------------------------------------------------------------
LRESULT CTilePanel::OnChangingActiveTab(WPARAM wParam, LPARAM lParam)
{
	return 	GetParent()->GetParent()->SendMessage (BCGM_CHANGING_ACTIVE_TAB, wParam, lParam);
}

//-----------------------------------------------------------------------------
HRESULT CTilePanel::get_accName(VARIANT varChild, BSTR *pszName)
{
	CString sNamespace = cwsprintf(_T("{0-%s}{1-%s}"), GetNamespace().GetObjectName(), GetNamespace().GetTypeString());
	*pszName = ::SysAllocString(sNamespace);
	return S_OK;
}

//--------------------------------------------------------------------------
BOOL CTilePanel::PreTranslateMessage(MSG* pMsg)
{
#ifndef _OLD_PTM

	if (!m_pParent || !m_pParent->m_pDocument)
		return __super::PreTranslateMessage(pMsg);

	if (m_pParent->m_pDocument->m_bForwardingSysKeydownToChild)
		return m_pTabber && m_pTabber->PreTranslateMessage(pMsg);

	return m_pParent && CTaskBuilderTabWnd::PreProcessSysKeyMessage(pMsg, m_pParent->m_pDocument, this) || __super::PreTranslateMessage(pMsg);

#else

	return __super::PreTranslateMessage(pMsg);

#endif
}

//-----------------------------------------------------------------------------
BOOL CTilePanel::Create(CBaseTileGroup* pParent, const CString& strTitle, UINT nIDC /*-1*/)
{
	CTBNamespace aNs;

	aNs.SetChildNamespace(CTBNamespace::FORM, _T("Tabber"), pParent->GetInfoOSL()->m_Namespace);

	UINT nID = nIDC > 0 ? nIDC : AfxGetTBResourcesMap()->GetTbResourceID(aNs.ToString(), TbControls);

	if (!__super::Create(NULL, _T(""), WS_CHILD | WS_VISIBLE, CRect(), pParent, nID))
	{
		ASSERT_TRACE(FALSE, "CTilePanel failed to create!\n");
		return FALSE;
	}
	Tile::TileCreate(strTitle);

	m_pTabber = new CTilePanelTabber(this);
	if (!m_pTabber->Create(CTaskBuilderTabWnd::STYLE_3D, CRect(), this, AfxGetTBResourcesMap()->GetTbResourceID(aNs.ToString(), TbControls)))
	{
		ASSERT_TRACE(FALSE, "CTilePanel failed to create tabber!\n");
		return FALSE;
	}
	m_pTabber->AttachDocument(pParent->GetDocument());
	m_pTabber->HideNoTabs(TRUE);
	m_pTabber->HideSingleTab(TRUE);
	m_pTabber->SetTabBorderSize(0);

	m_pParent = pParent;

	m_pTileDialogStyle = TileStyle::Inherit(m_pParent->GetTileDialogStyle());

	return TRUE;
}

//-----------------------------------------------------------------------------
int CTilePanel::GetTabsNum()
{
	return m_pTabber ? m_pTabber->GetTabsNum() : 0;
}

//-----------------------------------------------------------------------------
CTilePanelTab* CTilePanel::GetTab(int nTab)
{
	return (CTilePanelTab*) (m_pTabber ? m_pTabber->GetTabWnd(nTab) : NULL);
}

//-----------------------------------------------------------------------------
void CTilePanel::SetCollapsedDescription(CString strDescription)
{ 
	Tile::SetCollapsedDescription(strDescription);
}

//------------------------------------------------------------------------------
void CTilePanel::SetActiveTabContentEnable(BOOL bSet)
{
	if (!GetActiveTab())
		return;

	GetActiveTab()->EnableContent(bSet);
}

//-----------------------------------------------------------------------------
TileStyle* CTilePanel::GetTileDialogStyle()
{ 
	return m_pTileDialogStyle;
}

//-----------------------------------------------------------------------------
void CTilePanel::SetTileDialogStyle(TileStyle* pStyle)
{
	m_pTileDialogStyle ->Assign(pStyle);
}

//-----------------------------------------------------------------------------
void CTilePanel::ShowAsTile()
{
	BOOL bCollapsible = m_pTileStyle->Collapsible();
	// must forget any customized attribute and restart from parent's style
	ResetTileStyle(m_pTileDialogStyle);
	// contained tiles will retain their style, but will have no title, no top separator 
	// and spacing will be set to zero
	m_pTileDialogStyle->SetHasTitle(FALSE);
	m_pTileDialogStyle->SetTitleTopSeparatorWidth(0);
	m_pTileDialogStyle->SetTitleBottomSeparatorWidth(0);
	m_pTileDialogStyle->SetTileSpacing(m_pTileStyle->GetTileSpacing());
	SetCollapsible(bCollapsible);
	SetIsShowAsTile(true);
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CTilePanel::AddTile(CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex /*= AUTO*/, BOOL bInitiallyUnpinned/* FALSE*/, CObject* pOwner /*= NULL*/)
{
	EnsureTabExistance();
	return AddTile(GetActiveTab(), GetActiveTab()->m_pLayoutContainer, pClass, nDialogID, sTileTitle, tileSize, flex, bInitiallyUnpinned, pOwner);
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CTilePanel::AddTile(CLayoutContainer* pContainer, CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex /*= AUTO*/, CObject* pOwner /*= NULL*/)
{
	EnsureTabExistance();
	return AddTile(GetActiveTab(), pContainer, pClass, nDialogID, sTileTitle, tileSize, flex, FALSE, pOwner);
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CTilePanel::AddJsonTile(CTilePanelTab* pTab, UINT nDialogID, CLayoutContainer* pContainer)
{
	ASSERT(pTab);
	CJsonTileDialog* pTile = new CJsonTileDialog(nDialogID);
	return AddTile(pTab, pContainer, pTile, nDialogID, pTile->GetTitle(), pTile->GetSize(), pTile->GetFlex(), !pTile->IsInitiallyPinned());
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CTilePanel::AddJsonTile(UINT nDialogID, CLayoutContainer* pContainer)
{
	EnsureTabExistance();
	return AddJsonTile(GetActiveTab(), nDialogID, pContainer);
}

//-----------------------------------------------------------------------------
void CTilePanel::SetLayoutType	(CLayoutContainer::LayoutType aLayoutType)
{
	m_DefaultLayoutType = aLayoutType;
}

//-----------------------------------------------------------------------------
void CTilePanel::SetLayoutAlign	(CLayoutContainer::LayoutAlign aLayoutAlign)
{
	m_DefaultLayoutAlign = aLayoutAlign;
}

//-----------------------------------------------------------------------------
CTilePanelTab*	CTilePanel::AddTab
								(
									LPCTSTR							szName,
									LPCTSTR							szTitle, 
									UINT							nImage /*= -1*/
								)
{
	return AddTab(szName, szTitle, m_DefaultLayoutType, m_DefaultLayoutAlign, nImage);
}

//-----------------------------------------------------------------------------
BOOL CTilePanel::RemoveTab(int tab, BOOL recalcLayout)
{
	if (m_pTabber)
		return m_pTabber->RemoveTab(tab, recalcLayout);

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTilePanel::RemoveTab(CTilePanelTab* pTab, BOOL recalcLayout)
{
	int pos = -1;
	for (int i = 0; i < GetTabsNum(); i++)
	{
		if (GetTab(i) != pTab)
			continue;

		pos = i;
		break;
	}

	if (pos >= 0)
		return RemoveTab(pos, FALSE);

	return FALSE;
}

//---------------------------------------------------------------------------------
void CTilePanel::RemoveAllTabs()
{
	if (m_pTabber)
		m_pTabber->RemoveAllTabs();
}

//-----------------------------------------------------------------------------
CTilePanelTab*	CTilePanel::AddTab
(
	LPCTSTR							szName,
	LPCTSTR							szTitle, 
	CLayoutContainer::LayoutType	aLayoutType,
	CLayoutContainer::LayoutAlign	aLayoutAlign,
	UINT							nImage /*= -1*/
)
{
	CTilePanelTab* pTab = new CTilePanelTab(this);
	pTab->m_pLayoutContainer->SetLayoutType(aLayoutType);
	pTab->m_pLayoutContainer->SetLayoutAlign(aLayoutAlign);
	pTab->m_pLayoutContainer->SetMinWidth(m_nMinWidth);
	pTab->m_pLayoutContainer->SetMaxWidth(m_nMaxWidth);
	pTab->m_pLayoutContainer->SetGroupCollapsible(IsGroupCollapsible());

	pTab->SetTitle(CString(szTitle));

	pTab->Create(NULL, _T(""), WS_CHILD | WS_VISIBLE, CRect(), m_pTabber, AfxGetTBResourcesMap()->GetTbResourceID(m_pParent->GetInfoOSL()->m_Namespace.ToString(), TbControls));

	m_pTabber->AddTab(pTab, szTitle, nImage, FALSE);

	pTab->GetInfoOSL()->m_pParent = GetInfoOSL();
	pTab->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::TILEPANELTAB, szName, GetInfoOSL()->m_Namespace);
	
	int iTab = m_pTabber->GetTabFromHwnd(pTab->GetSafeHwnd());
	SetActiveTab(iTab);
	return pTab;
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CTilePanel::AddTile(CTilePanelTab* pTab, CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex /*= 1*/, BOOL bInitiallyUnpinned/* FALSE*/, CObject* pOwner /*= NULL*/)
{
	return AddTile(pTab, pTab->m_pLayoutContainer, pClass, nDialogID, sTileTitle, tileSize, flex, bInitiallyUnpinned, pOwner);
}

//-----------------------------------------------------------------------------
LRESULT CTilePanel::OnValueChanged	(WPARAM wParam, LPARAM lParam)
{ 
	return GetParent()->SendMessage(UM_VALUE_CHANGED, wParam, lParam);	
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CTilePanel::AddTile
(
	CTilePanelTab*		pTab,
	CLayoutContainer*	pCntner,
	CRuntimeClass*		pClass,
	UINT				nDialogID,
	CString				sTileTitle,
	TileDialogSize		tileSize,
	int					flex /*= -1*/,
	BOOL				bInitiallyUnpinned, /* FALSE*/
	CObject*			pOwner	/*= NULL*/
)
{

	if (!pClass->IsDerivedFrom(RUNTIME_CLASS(CBaseTileDialog)))
	{
		ASSERT(FALSE);
		return NULL;
	}

	CBaseTileDialog* pTile = (CBaseTileDialog*)pClass->CreateObject();
	return AddTile(pTab, pCntner, pTile, nDialogID, sTileTitle, tileSize, flex /*= -1*/, bInitiallyUnpinned, /* FALSE*/ pOwner	/*= NULL*/);
}
//-----------------------------------------------------------------------------
CBaseTileDialog* CTilePanel::AddTile
	(
	CTilePanelTab*		pTab,
	CLayoutContainer*	pCntner,
	CBaseTileDialog*	pTile,
	UINT				nDialogID,
	CString				sTileTitle,
	TileDialogSize		tileSize,
	int					flex /*= -1*/,
	BOOL				bInitiallyUnpinned, /* FALSE*/
	CObject*			pOwner	/*= NULL*/
	)
{
	m_pParent->AttachTile(pTile, pOwner);
	
	pTile->Create(nDialogID, sTileTitle, pTab, tileSize);

	// force a flex only if not auto
	if (flex != AUTO)
		pTile->SetFlex(flex, FALSE);

	if (bInitiallyUnpinned)
		pTile->SetPinned(FALSE);

	CLayoutContainer*	pContainer = pCntner ? pCntner : pTab->m_pLayoutContainer;
	pContainer->AddChildElement(pTile);

	return pTile;
}

//-----------------------------------------------------------------------------
CBaseStatusTile* CTilePanel::AddStatusTile(CRuntimeClass* pStatusTileClass, UINT nTileID, int flex)
{
	CBaseStatusTile* pTile = NULL;

	pTile = (CBaseStatusTile*)AddTile(pStatusTileClass, nTileID, _T(""), TILE_MICRO, flex);

	return pTile;
}

//-----------------------------------------------------------------------------
void CTilePanel::AddStatusTile(CBaseStatusTile* pStatusTile, UINT nTileID, int flex)
{
	CTilePanelTab* pTilePanelTab = GetActiveTab();
	if (!pTilePanelTab)
	{
		ASSERT(FALSE);
		return;
	}

	AddTile(pTilePanelTab, pTilePanelTab->m_pLayoutContainer, (CBaseTileDialog*)pStatusTile, nTileID, _T(""), TILE_MICRO, flex);
}

//-----------------------------------------------------------------------------
CLayoutContainer* CTilePanel::AddContainer		
(
	CTilePanelTab*					pTab,
	CLayoutContainer::LayoutType	aLayoutType, 
	int								flex /*= -1*/,
	CLayoutContainer::LayoutAlign	aLayoutAlign /*= CLayoutContainer::STRETCH*/
)
{
	return AddContainer(pTab->m_pLayoutContainer, aLayoutType, flex, aLayoutAlign);
}

//-----------------------------------------------------------------------------
CLayoutContainer* CTilePanel::AddContainer(CLayoutContainer::LayoutType	aLayoutType, int flex /*= -1*/, CLayoutContainer::LayoutAlign aLayoutAlign /*= CLayoutContainer::STRETCH*/)
{
	EnsureTabExistance();
	return AddContainer(GetActiveTab()->m_pLayoutContainer, aLayoutType, flex, aLayoutAlign);
}

//-----------------------------------------------------------------------------
CLayoutContainer* CTilePanel::AddContainer		
(
	CLayoutContainer*				pParentContainer,
	CLayoutContainer::LayoutType	aLayoutType, 
	int								flex /*= -1*/,
	CLayoutContainer::LayoutAlign	aLayoutAlign /*= CLayoutContainer::STRETCH*/
)
{
	CLayoutContainer* pContainer = new CLayoutContainer(pParentContainer, m_pTileDialogStyle);

	pContainer->SetLayoutType(aLayoutType);
	pContainer->SetLayoutAlign(aLayoutAlign);
	pContainer->SetMinWidth(m_nMinWidth);
	pContainer->SetMaxWidth(m_nMaxWidth);
	pContainer->SetGroupCollapsible(IsGroupCollapsible());
	pParentContainer->AddOwnedContainer(pContainer);
	pContainer->SetFlex(flex);

	return pContainer;
}

//------------------------------------------------------------------------------
void CTilePanel::SetActiveTab(int nTab)
{
	if (m_pTabber)
		m_pTabber->SetActiveTab(nTab);
}

//------------------------------------------------------------------------------
void CTilePanel::Enable(BOOL bEnable)
{ 
	__super::Enable(bEnable); 
	EnableWindow(bEnable); 
}

//------------------------------------------------------------------------------
void CTilePanel::SetGroupCollapsible(BOOL bSet /*= TRUE*/)		
{ 
	LayoutElement::SetGroupCollapsible(bSet); 
	for (int t = 0; t < m_pTabber->GetTabsNum(); t++)
	{
		CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(m_pTabber->GetTabWnd(t));
		if (pTab)
			pTab->m_pLayoutContainer->SetGroupCollapsible(bSet);
	}
}

//-----------------------------------------------------------------------------
void CTilePanel::OnUpdateTitle()
{
	if (!GetTileStyle()->HasTitle())
		return;
	CRect rectTitle;
	GetWindowRect(&rectTitle);
	ScreenToClient(&rectTitle);
	rectTitle.bottom = m_nTitleHeight;
	InvalidateRect(rectTitle);
}

//-----------------------------------------------------------------------------
void CTilePanel::OnPaint()
{
	{ // Scope del CPaintDC
		CPaintDC dc(this);  //le OnPaint DEVONO obbligatoriamente avere un CPaintDC al loro interno , altrimenti non funziona niente
		Tile::OnTilePaint(&dc);
	}

	__super::OnPaint();
}

//-----------------------------------------------------------------------------
void CTilePanel::OnLButtonDown(UINT nFlags, CPoint point)
{
	// click sulla zona del titolo
	if (point.y < m_nTitleHeight)
	{
		Tile::CollapseExpand();
		return;
	}

	__super::OnLButtonDown(nFlags, point);
}

//------------------------------------------------------------------------------
void CTilePanel::DoCollapseExpand()
{
	GetParentTileGroup()->NotifyChildStateChanged(GetNamespace(), IsCollapsed());
	RequestRelayout();
}

//------------------------------------------------------------------------------
void CTilePanel::DoPinUnpin()
{
	m_bVisible = IsPinned();
	ShowWindow(m_bVisible ? SW_SHOW : SW_HIDE);
	RequestRelayout();
}

//-----------------------------------------------------------------------------
int CTilePanel::GetRequiredHeight(CRect &rectAvail)
{
	if (IsCollapsed() && GetTileStyle()->HasTitle())
		return m_nTitleHeight;

	int maxHeight = 0;
	for (int t = 0; t < m_pTabber->GetTabsNum(); t++)
	{
		CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(m_pTabber->GetTabWnd(t));
		if (!pTab)
			continue;

		int nHeight = pTab->m_pLayoutContainer->GetRequiredHeight(rectAvail);
		maxHeight = max(maxHeight, nHeight);
	}

	return maxHeight + GetTabsHeight() + m_nTitleHeight;
}

//-----------------------------------------------------------------------------
int CTilePanel::GetRequiredWidth(CRect &rectAvail)
{
	int maxWidth = 0;
	for (int t = 0; t < m_pTabber->GetTabsNum(); t++)
	{
		CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(m_pTabber->GetTabWnd(t));
		if (!pTab)
			continue;

		int nWidth = pTab->m_pLayoutContainer->GetRequiredWidth(rectAvail);
		maxWidth = max(maxWidth, nWidth);
	}

	return maxWidth;
}

//------------------------------------------------------------------------------
BOOL CTilePanel::CanDoLastFlex(FlexDim  fd)
{
	if (fd == HEIGHT && IsCollapsed())
		return FALSE;

	if (IsFlexAuto())
	{
		CTilePanelTab* pTab = GetActiveTab();
		if (pTab)
			return pTab->m_pLayoutContainer->CanDoLastFlex(fd);
	}

	return IsFlex(fd);
}

//------------------------------------------------------------------------------
int CTilePanel::GetFlex(FlexDim dim)
{
	if (dim == HEIGHT && IsCollapsed())
		return 0;

	if (IsFlexAuto())
	{
		if (GetRequestedLastFlex() != AUTO)
			return GetRequestedLastFlex();
		else 
			return 0;
	}

	return __super::GetFlex(dim);
}

//------------------------------------------------------------------------------
int CTilePanel::GetTabsHeight()
{
	// 1 tab only, no tabs selector
	return m_pTabber->GetTabsNum() > 1 ? m_pTabber->GetTabsHeight() : 0;
}

//------------------------------------------------------------------------------
int CTilePanel::GetMinHeight(CRect& rect /*= CRect(0, 0, 0, 0)*/)
{ 
	switch (m_nMinHeight)
	{
		case ORIGINAL	:
		case FREE		:
		case AUTO		:
		{
			// solo il titolo
			if (IsCollapsed())
				return m_nTitleHeight;
			
			// la massima tra le altezze minime di tutte le tab
			int maxHeight = 0;
			for (int t = 0; t < m_pTabber->GetTabsNum(); t++)
			{
				CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(m_pTabber->GetTabWnd(t));
				if (pTab)
					maxHeight = max(maxHeight, pTab->m_pLayoutContainer->GetMinHeight(rect));
			}
			// altezza delle tab + tabberino se c'e' + titolo se c'e'
			return maxHeight + GetTabsHeight() + m_nTitleHeight;
		}
		// altrimenti altezza fissa
		default: return m_nTitleHeight  + m_nMinHeight;
	}
}

//------------------------------------------------------------------------------
void CTilePanel::GetAvailableRect(CRect &rectAvail)
{
	GetClientRect(rectAvail);
	if (IsCollapsed() && rectAvail.Height() > m_nTitleHeight)
		rectAvail.bottom = rectAvail.top + m_nTitleHeight;
}

//------------------------------------------------------------------------------
void CTilePanel::SetMinWidth(int nWidth)
{
	__super::SetMinWidth(nWidth);

	for (int t = 0; t < m_pTabber->GetTabsNum(); t++)
	{
		CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(m_pTabber->GetTabWnd(t));
		if (pTab)
			pTab->m_pLayoutContainer->SetMinWidth(nWidth);
	}
}

//------------------------------------------------------------------------------
void CTilePanel::SetMaxWidth(int nWidth)
{
	__super::SetMaxWidth(nWidth);

	for (int t = 0; t < m_pTabber->GetTabsNum(); t++)
	{
		CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(m_pTabber->GetTabWnd(t));
		if (pTab)
			pTab->m_pLayoutContainer->SetMaxWidth(nWidth);
	}
}

//-----------------------------------------------------------------------------
void CTilePanel::Relayout(CRect &rectNew, HDWP hDWP /*= NULL*/)
{
	if (!IsVisible())
		return;

	CDockableFrame* pFrame = dynamic_cast<CDockableFrame*>(m_pParent->GetParentFrame());
	if ((pFrame && pFrame->IsLayoutSuspended()))
		return;

	if (IsCollapsed() && GetTileStyle()->HasTitle())
		rectNew.bottom = rectNew.top + m_nTitleHeight;

	SetWindowPos(NULL, rectNew.left, rectNew.top, rectNew.Width(), rectNew.Height(), SWP_NOZORDER | SWP_FRAMECHANGED);
	Invalidate();
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void CTilePanel::GetUsedRect(CRect &rectUsed)
{
	if (!IsVisible())
		return;

	int nTitleHeight = 0;
	GetWindowRect(rectUsed);
	if (GetTileStyle()->HasTitle())
		rectUsed.bottom = rectUsed.top + m_nTitleHeight;

	if (IsCollapsed())
		return;

	int tabs = m_pTabber->GetTabsNum();
	if (tabs <= 0)
		return;

	CRect internalRectUsed;
	CTilePanelTab* pTab = GetActiveTab();
	if (pTab)
	{
		pTab->m_pLayoutContainer->GetUsedRect(internalRectUsed);
		rectUsed.bottom = internalRectUsed.bottom + m_nTitleHeight;
	}
}

//-----------------------------------------------------------------------------
int CTilePanel::GetTabFromPoint(CPoint pt)
{
	if (!m_pTabber)
		return NULL;

	return m_pTabber->CTaskBuilderTabWnd::GetTabFromPoint(pt);
}

//-----------------------------------------------------------------------------
CTilePanelTab* CTilePanel::GetActiveTab()
{
	if (!m_pTabber)
		return NULL;

	return m_pTabber->GetActiveTilePanelTab();
}

//------------------------------------------------------------------------------
void CTilePanel::SetShowTab(int nTab, BOOL bVisible)
{
	if (!m_pTabber)
		return;

	m_pTabber->ShowTab(nTab, bVisible);
}

//----------------------------------------------------------------------------------
BOOL CTilePanel::IsTabVisible(int nTab)
{
	if (!m_pTabber)
		return FALSE;

	return m_pTabber->IsTabVisible(nTab);
}

//---------------------------------------------------------------------------------
int CTilePanel::GetActiveTabNum()
{
	if (!m_pTabber)
		return -1;

	return m_pTabber->GetActiveTab();
}

//-------------------------------------------------------------------------------------
int CTilePanel::GetTabIdx(CTilePanelTab* pTab)
{
	if (!pTab || !m_pTabber)
		return -1;

	return m_pTabber->GetTabFromHwnd(pTab->GetSafeHwnd());
}

//-----------------------------------------------------------------------------
void CTilePanel::EnsureTabExistance()
{
	if (m_pTabber->GetTabsNum() == 0)
		AddTab(_T("Main"), _TB("Main"), m_DefaultLayoutType, m_DefaultLayoutAlign);
}

//-----------------------------------------------------------------------------
void CTilePanel::OnWindowPosChanged(WINDOWPOS* lpwndpos)
{
	__super::OnWindowPosChanged(lpwndpos);

	Tile::OnTilePosChanged();
	OnUpdateTitle();

	CRect aRect(0, m_nTitleHeight, lpwndpos->cx, lpwndpos->cy);
	if (m_pTabber)
	{
		ASSERT_VALID(m_pTabber);
		m_pTabber->SetWindowPos(NULL, aRect.left, aRect.top, aRect.Width(), aRect.Height(), SWP_NOZORDER | SWP_FRAMECHANGED);
	
		int nTab = m_pTabber->GetActiveTab();
		if (nTab >= 0)
			m_pTabber->InvalidateTab(nTab);
	}
}

//------------------------------------------------------------------------------
BOOL CTilePanel::OnCommand(WPARAM wParam, LPARAM lParam)
{
	if (__super::OnCommand(wParam, lParam))
		return TRUE;

	CWnd* pParent = GetParent();
	return pParent ? pParent->SendMessage(WM_COMMAND, wParam, lParam) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL CTilePanel::IsGroupCollapsible()
{
	return LayoutElement::IsGroupCollapsible();
}

//-----------------------------------------------------------------------------
void CTilePanel::SetCollapsed(BOOL bCollapsed /*TRUE*/)
{
	if (bCollapsed && !IsGroupCollapsible() && m_pTileStyle && !m_pTileStyle->Collapsible())
	{
		SetGroupCollapsible();
		SetCollapsible();
	}

	__super::SetCollapsed(bCollapsed);;

}

//-----------------------------------------------------------------------------
CWnd* CTilePanel::GetWndLinkedCtrl(const CTBNamespace& aNS)
{
	for (int t = 0; t < m_pTabber->GetTabsNum(); t++)
	{
		CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(m_pTabber->GetTabWnd(t));
		if (pTab && pTab->GetNamespace() == aNS)
			return pTab;

		CWnd* pWnd = pTab->GetWndLinkedCtrl(aNS);
		if (pWnd)
			return pWnd;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CWnd* CTilePanel::GetWndLinkedCtrl(UINT nIDC)
{
	for (int t = 0; t < m_pTabber->GetTabsNum(); t++)
	{
		CTilePanelTab* pTab = dynamic_cast<CTilePanelTab*>(m_pTabber->GetTabWnd(t));
		if (pTab && pTab->GetDlgCtrlID() == nIDC)
			return pTab;

		CWnd* pWnd = pTab->GetWndLinkedCtrl(nIDC);
		if (pWnd)
			return pWnd;
	}
	return NULL;
}
//-----------------------------------------------------------------------------
LRESULT CTilePanel::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit).
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndObjDescription* pDesc = pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndObjDescription), strId);
	pDesc->UpdateAttributes(this);
	pDesc->m_Type = CWndObjDescription::Panel;

	pDesc->AddChildWindows(this);

	return (LRESULT)pDesc;
}
