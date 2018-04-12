#include "stdafx.h"
#include <TbFrameworkImages\CommonImages.h>

#include "TileManager.h"
#include "TileDialog.h"
#include "Tabber.h"
#include "BODYEDIT.H"
#include "JsonFormEngineEx.h"
#include "FormMng.h"
//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

//////////////////////////////////////////////////////////////////////////////
//							TileGroupInfoItem definition
//////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC (TileGroupInfoItem, DlgInfoItem)


//////////////////////////////////////////////////////////////////////////////
//							CTabDialogTileGroup definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CTabDialogTileGroup, CTabDlgEmpty);

BEGIN_MESSAGE_MAP(CTabDialogTileGroup, CTabDlgEmpty)
	//{{AFX_MSG_MAP(CTabDialogTileGroup)
	ON_WM_WINDOWPOSCHANGED()
	ON_WM_DESTROY()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CTabDialogTileGroup::OnInitDialog()
{
	CRect rcTileGroup;
	GetClientRect(rcTileGroup);

	CRect rcTab(0, 0, rcTileGroup.Width(), rcTileGroup.Height());
	TileGroupInfoItem* pDlgInfoItem = (TileGroupInfoItem*)GetDlgInfoItem();
	m_pTileGroup = AddTileGroup(pDlgInfoItem->m_nTileGroupID, pDlgInfoItem->m_pTileGroupClass, pDlgInfoItem->m_sName, TRUE, pDlgInfoItem, rcTab);
	m_pTileGroup->LayoutElement::SetParentElement(m_pParentTabManager); 
	GetJsonContext()->m_pDescription->AttachTo(m_pTileGroup->m_hWnd);
	
	return  __super::OnInitDialog();
}

//-----------------------------------------------------------------------------
void CTabDialogTileGroup::OnBeforeAttachParents(CAbstractFormDoc* pDoc, CAbstractFormView* pView, CTabManager* pTabber)
{ 
	TileGroupInfoItem* pDlgInfoItem = (TileGroupInfoItem*)GetDlgInfoItem();
	m_sName = pDlgInfoItem->GetName();

	__super::OnBeforeAttachParents(pDoc, pView, pTabber);
}

//-----------------------------------------------------------------------------
void CTabDialogTileGroup::CustomizeExternal()
{
	ASSERT(m_pTileGroup);
	if (GetDocument())
	{
		GetDocument()->AddClientDocTileDialog(m_pTileGroup);
		if (GetDocument()->GetDesignMode() != CBaseDocument::DM_RUNTIME)
			m_pTileGroup->SetSuspendResizeStaticArea(FALSE);
	}
}

//-----------------------------------------------------------------------------
BOOL CTabDialogTileGroup::DestroyWindow()
{
	// during the destroy process, WM_SIZE messages are sent: 
	// avoid doing layout for a tilegroup about to die
	if (m_pTileGroup)
		m_pTileGroup->SuspendLayout();

	return __super::DestroyWindow();
}

//---------------------------------------------------------------------------------------
void CTabDialogTileGroup::OnWindowPosChanged( WINDOWPOS* lpwndpos)
{
	//@@@TODO LAYOUT RESIZABLE CTRL
	if (m_pTileGroup && IsWindow(m_pTileGroup->m_hWnd))
		m_pTileGroup->MoveWindow(0, 0, lpwndpos->cx, lpwndpos->cy);
	//m_pTileGroup->SendMessage(UM_RECALC_CTRL_SIZE);
}

//-----------------------------------------------------------------------------
void CTabDialogTileGroup::OnDestroy()
{	
	// during the destroy process, WM_SIZE messages are sent: 
	// avoid doing layout for a tilegroup about to die
	if (m_pTileGroup)
		m_pTileGroup->SuspendLayout();

	__super::OnDestroy();
}

//////////////////////////////////////////////////////////////////////////////
//							CRowTabDialogTileGroup definition
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRowTabDialogTileGroup, CRowTabDialog);

//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRowTabDialogTileGroup, CRowTabDialog)
	ON_WM_WINDOWPOSCHANGED()
	ON_WM_DESTROY()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CRowTabDialogTileGroup::OnInitDialog()
{
	CRect rcTileGroup;
	GetClientRect(rcTileGroup);
		
	CButton* pButton = new CButton();
	CRect rcTab(0, 0, rcTileGroup.Width(), rcTileGroup.Height());
	TileGroupInfoItem* pDlgInfoItem = (TileGroupInfoItem*)GetDlgInfoItem();
	pButton->Create(_T(""), WS_CHILD | WS_VISIBLE, rcTab, this, pDlgInfoItem->m_nTileGroupID);
	pButton->UnsubclassWindow();
		
	m_pTileGroup = AddTileGroup(pDlgInfoItem->m_nTileGroupID, pDlgInfoItem->m_pTileGroupClass, pDlgInfoItem->m_strTitle, TRUE, pDlgInfoItem);
	delete(pButton);

	return  __super::OnInitDialog();
}

//-----------------------------------------------------------------------------
void CRowTabDialogTileGroup::OnBeforeAttachParents(CAbstractFormDoc* pDoc, CAbstractFormView* pView, CTabManager* pTabber)
{ 
	TileGroupInfoItem* pDlgInfoItem = (TileGroupInfoItem*)GetDlgInfoItem();
	m_sName = pDlgInfoItem->GetName();

	__super::OnBeforeAttachParents(pDoc, pView, pTabber);
}

//-----------------------------------------------------------------------------
void CRowTabDialogTileGroup::OnWindowPosChanged(WINDOWPOS* lpwndpos)
{
	//@@@TODO LAYOUT RESIZABLE CTRL
	if (m_pTileGroup && IsWindow(m_pTileGroup->m_hWnd))
		m_pTileGroup->MoveWindow(0, 0, lpwndpos->cx, lpwndpos->cy);
	//m_pTileGroup->SendMessage(UM_RECALC_CTRL_SIZE);
}

//-----------------------------------------------------------------------------
void CRowTabDialogTileGroup::OnDestroy()
{
	// during the destroy process, WM_SIZE messages are sent: 
	// avoid doing layout for a tilegroup about to die
	if (m_pTileGroup)
		m_pTileGroup->SuspendLayout();

	__super::OnDestroy();
}


//-----------------------------------------------------------------------------
BOOL CRowTabDialogTileGroup::DestroyWindow()
{
	// during the destroy process, WM_SIZE messages are sent: 
	// avoid doing layout for a tilegroup about to die
	if (m_pTileGroup)
		m_pTileGroup->SuspendLayout();

	return __super::DestroyWindow();
}

/////////////////////////////////////////////////////////////////////////////
// 						class CTileGroup Implementation
/////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE(CTileGroup, CBaseTileGroup);
BEGIN_MESSAGE_MAP(CTileGroup, CBaseTileGroup)
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------
void CTileGroup::EnableViewControlLinks(BOOL bEnable, BOOL bMustSetOSLReadOnly)
{
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		pTileDialog->EnableTileDialogControlLinks(bEnable && pTileDialog->IsEnabled(), bMustSetOSLReadOnly);
	}
}
//-----------------------------------------------------------------------------
void CTileGroup::OnFindHotLinks()
{
	for (int j = 0; j < GetTileDialogs()->GetSize(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		if (!pTileDialog->m_hWnd)
			continue;
		pTileDialog->OnFindHotLinks();
	}
}
//-----------------------------------------------------------------------------
void CTileGroup::OnUpdateControls(BOOL bParentIsVisible)
{
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		if (!pTileDialog->m_hWnd)
			continue;
		pTileDialog->OnUpdateControls(bParentIsVisible);

		pTileDialog->OnUpdateTitle();
		pTileDialog->UpdateTitleView();
	}

	for (int j = 0; j <= GetTilePanels()->GetUpperBound(); j++)
	{
		CTilePanel* pTilePanel = (CTilePanel*)GetTilePanels()->GetAt(j);
		pTilePanel->OnUpdateTitle();
	}
}

//-----------------------------------------------------------------------------
LRESULT CTileGroup::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*)wParam;

	
	// Siccome il tilegroup ha sempre un inner layout cablato (sono sempre in relazione 1-1), non duplichiamo le informazioni con due "descrizioni", ma creiamo una sola 
	// CWndLayoutContainerDescription che ha tutte le informazioni necessarie.
	// come id si usa il puntatore, perche' serve per comodita in successivi confronti.

	CLayoutContainer* pEmbeddedLayoutContainer = this->GetLayoutContainer();
	CString strId = cwsprintf(_T("%d"), pEmbeddedLayoutContainer);
	CWndLayoutContainerDescription* pDesc = (CWndLayoutContainerDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndLayoutContainerDescription), strId);
	pDesc->UpdateAttributes(this);
	pDesc->UpdateLayoutAttributes(pEmbeddedLayoutContainer);
	pDesc->m_Type = CWndObjDescription::TileGroup;
	ASSERT(m_pDlgInfoItem);
	if (m_pDlgInfoItem)
	{
		if (pDesc->m_strText != m_pDlgInfoItem->m_strTitle)
		{
			pDesc->m_strText = m_pDlgInfoItem->m_strTitle;
			pDesc->SetUpdated(&pDesc->m_strText);
		}
		if (pDesc->m_strHint != m_pDlgInfoItem->GetSelectorTooltip())
		{
			pDesc->m_strHint = m_pDlgInfoItem->GetSelectorTooltip();
			pDesc->SetUpdated(&pDesc->m_strHint);
		}
		if (pDesc->m_strIcon != m_pDlgInfoItem->GetSelectorImage())
		{
			pDesc->m_strIcon = m_pDlgInfoItem->GetSelectorImage();
			pDesc->SetUpdated(&pDesc->m_strIcon);
		}
	}
	pDesc->m_bMaximizeBox = false;
	pDesc->m_bMinimizeBox = false; 
	pDesc->m_bOverlapped = false;
	pDesc->SetRect(CRect(0, 0, 0, 0), FALSE);

	return (LRESULT)pDesc;
}
//-----------------------------------------------------------------------------
void CTileGroup::OnResetDataObjs()
{
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		pTileDialog->OnResetDataObjs();
	}
}

//-----------------------------------------------------------------------------
BOOL CTileGroup::PrepareAuxData()
{
	if (!OnPrepareAuxData())
		return FALSE;

	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		if (!pTileDialog->PrepareAuxData())
			return FALSE;
	}
	return TRUE;
}

// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CBodyEdit* CTileGroup::GetBodyEdits (int* pnStartIdx)
{   
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		if (!pTileDialog->GetBodyEdits(pnStartIdx))
		{
			return FALSE;
		}
	}	
	return NULL;
}
//-----------------------------------------------------------------------------
CParsedCtrl* CTileGroup::GetLinkedParsedCtrl (DataObj* pDataObj)
{
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		CParsedCtrl* pControl = pTileDialog->GetLinkedParsedCtrl(pDataObj);
		if (pControl)
		{
			return pControl;
		}
	}	
	return NULL;
}

//-----------------------------------------------------------------------------
CParsedCtrl* CTileGroup::GetLinkedParsedCtrl (const CTBNamespace& aNS)
{
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		CParsedCtrl* pControl = pTileDialog->GetLinkedParsedCtrl(aNS);
		if (pControl)
		{
			return pControl;
		}
	}	
	return NULL;
}

//----------------------------------------------------------------------------
CParsedCtrl* CTileGroup::GetLinkedParsedCtrl (UINT nIDC)
{
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		CParsedCtrl* pControl = pTileDialog->GetLinkedParsedCtrl(nIDC);
		if (pControl)
		{
			return pControl;
		}
	}	
	return NULL;
}

// Questo metodo permette di risalire alla CWnd di un control IDC
//-----------------------------------------------------------------------------
CBodyEdit* CTileGroup::GetBodyEdits(const CTBNamespace& aNS)
{   
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		if (!pTileDialog->GetBodyEdits(aNS))
		{
			return FALSE;
		}
	}	
	return NULL;
}

//-----------------------------------------------------------------------------
void CTileGroup::RebuildLinks	(SqlRecord* pRec)
{
	for (int j = 0; j <= GetTileDialogs()->GetUpperBound(); j++)
	{
		CTileDialog* pTileDialog = (CTileDialog*)GetTileDialogs()->GetAt(j);
		if (pTileDialog)
			pTileDialog->RebuildLinks(pRec);
	}	
}

//-----------------------------------------------------------------------------
void CTileGroup::EnableTile(UINT nIDD, BOOL bEnable)
{
	for (int t = 0; t <= GetTileDialogs()->GetUpperBound(); t++)
	{
		CBaseTileDialog* pDialog = GetTileDialogs()->GetAt(t);
		if (pDialog && pDialog->GetDialogID() == nIDD)
		{
			pDialog->Enable(bEnable);

			if (pDialog->IsKindOf(RUNTIME_CLASS(CTileDialog)))
			{
				((CTileDialog*)pDialog)->EnableTileDialogControlLinks(
											bEnable &&
											(
												GetDocument() &&
												GetDocument()->IsABatchDocument() || 
												GetDocument()->GetFormMode() == CBaseDocument::EDIT  || 
												GetDocument()->GetFormMode() == CBaseDocument::NEW
											));

				((CTileDialog*)pDialog)->OnUpdateControls();
			}
		}
	}
}

//-----------------------------------------------------------------------------
CTilePanel* CTileGroup::GetTilePanel(UINT nIDD)
{
	for (int t = 0; t <= GetTilePanels()->GetUpperBound(); t++)
	{
		CTilePanel* pPanel = GetTilePanels()->GetAt(t);

		if (pPanel && pPanel->GetDlgCtrlID() == nIDD)
			return pPanel;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CTileGroup::EnableTilePanel(UINT nIDD, BOOL bEnable)
{
	for (int t = 0; t <= GetTilePanels()->GetUpperBound(); t++)
	{
		CTilePanel* pPanel = GetTilePanels()->GetAt(t);

		if (pPanel && pPanel->GetDlgCtrlID() == nIDD)
		{
			pPanel->Enable(bEnable);
			break;
		}
	}
}

//-----------------------------------------------------------------------------
CWnd* CTileGroup::GetWndCtrl(UINT nIDC)
{
	for (int t = 0; t <= GetTileDialogs()->GetUpperBound(); t++)
	{
		CBaseTileDialog* pDialog = GetTileDialogs()->GetAt(t);
		if (pDialog)
		{
			CWnd* pWnd = pDialog->GetDlgItem(nIDC);
			if (pWnd)
				return pWnd;
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void CTileGroup::Enable(BOOL bEnable)
{
	EnableWindow(bEnable);
}

//-----------------------------------------------------------------------------
void CTileGroup::Show(BOOL bShow)
{
	ShowWindow(bShow ? SW_SHOW : SW_HIDE);
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CTileGroup::GetTileDialog(UINT nIDD)
{
	for (int t = 0; t <= GetTileDialogs()->GetUpperBound(); t++)
	{
		CBaseTileDialog* pDialog = GetTileDialogs()->GetAt(t);
		if (pDialog && pDialog->GetDialogID() == nIDD)
			return pDialog;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CTileGroup::GetTileDialog(CTBNamespace aNs)
{
	for (int t = 0; t <= GetTileDialogs()->GetUpperBound(); t++)
	{
		CBaseTileDialog* pDialog = GetTileDialogs()->GetAt(t);
		if (pDialog && pDialog->GetNamespace() == aNs)
			return pDialog;
	}

	return NULL;
}


//-----------------------------------------------------------------------------
CBaseTileDialog* CTileGroup::AddJsonTile(UINT nDialogID, CLayoutContainer* pContainer)
{
	CJsonTileDialog* pTile = new CJsonTileDialog(nDialogID);
	AddTile(pContainer, pTile, nDialogID, pTile->GetTitle(), pTile->GetSize(), pTile->GetFlex());
	pTile->SetPinned(pTile->IsInitiallyPinned());
	return pTile;
}

//-----------------------------------------------------------------------------
void CTileGroup::OnBeforeCustomize()
{ 
	CAbstractFormDoc* pDoc = dynamic_cast<CAbstractFormDoc*>(GetDocument());
	if (pDoc && pDoc->m_pFormManager)
		pDoc->m_pFormManager->EnableDialogStateSave(FALSE);
}

//-----------------------------------------------------------------------------
void CTileGroup::OnAfterCustomize()
{ 
	CAbstractFormDoc* pDoc = dynamic_cast<CAbstractFormDoc*>(GetDocument());
	
	if (!pDoc)
		return;
	
	for (int i = 0; i < GetTileDialogs()->GetSize(); i++)
	{
		CBaseTileDialog* pTileDialog = GetTileDialogs()->GetAt(i);
		BOOL bPinned = FALSE;
		if (!pTileDialog->GetTileStyle()->Collapsible())
			continue;

		// suspend form manager update modified flag
		BOOL bDummy = FALSE, bCollapsed = FALSE;
		if (pDoc->m_pFormManager->HasDialogCustomized(pTileDialog->GetNamespace(), bDummy, bCollapsed))
			pTileDialog->SetCollapsed(bCollapsed);
		else
			// questo codice serve per allineare lo stato del form manager sulla base delle eventuali modifiche
			// gestionali fatti durante il codice di Customize
			pDoc->m_pFormManager->SetDialogsCollapsedState(pTileDialog->GetNamespace(), pTileDialog->IsCollapsed());
	}

	if (!pDoc->IsABatchDocument())
	{
		for (int i = 0; i < GetTilePanels()->GetSize(); i++)
		{
			CTilePanel* pTilePanel = GetTilePanels()->GetAt(i);
			BOOL bPinned = FALSE;
			if (!pTilePanel->GetTileStyle()->Collapsible())
				continue;

			BOOL bDummy = FALSE, bCollapsed = FALSE;
			if (pDoc->m_pFormManager->HasDialogCustomized(pTilePanel->GetNamespace(), bDummy, bCollapsed))
				pTilePanel->SetCollapsed(bCollapsed);
			else
				// questo codice serve per allineare lo stato del form manager sulla base delle eventuali modifiche
				// gestionali fatti durante il codice di Customize
				pDoc->m_pFormManager->SetDialogsCollapsedState(pTilePanel->GetNamespace(), pTilePanel->IsCollapsed());
		}
	}
	pDoc->m_pFormManager->EnableDialogStateSave(TRUE);
}

//-----------------------------------------------------------------------------
void CTileGroup::NotifyChildStateChanged(CTBNamespace aNs, BOOL bState)
{
	CAbstractFormDoc* pDoc = dynamic_cast<CAbstractFormDoc*>(GetDocument());
	if (pDoc)
		pDoc->m_pFormManager->SetDialogsCollapsedState(aNs, bState);

}


//==============================================================================
//
//			Class CTileManager implementation
//
//==============================================================================

IMPLEMENT_DYNCREATE(CTileManager, CTabManager)
BEGIN_MESSAGE_MAP(CTileManager, CTabManager)	
	ON_MESSAGE(UM_GET_CONTROL_DESCRIPTION, OnGetControlDescription)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTileManager::CTileManager()
{
	SetShowMode(VERTICAL_TILE);	
	GetInfoOSL()->SetType(OSLType_TileManager);
}

//------------------------------------------------------------------------------
CRect CTileManager::GetSelectorRect()
{
	CRect rect;
	GetWindowRect(rect);
	return CRect(0, 0, m_nSelectorWidth, rect.bottom);
}


//-----------------------------------------------------------------------------
CTileManager::~CTileManager()
{
}

//-----------------------------------------------------------------------------
LRESULT CTileManager::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CTabberDescription* pDesc = (CTabberDescription*)__super::OnGetControlDescription(wParam, lParam);
	pDesc->m_Type = CWndObjDescription::TileManager;
	return (LRESULT)pDesc;
}

//-----------------------------------------------------------------------------
TileGroupInfoItem* CTileManager::AddTileGroup(CRuntimeClass* pTileGroupClass, CString sNameTileGroup, CString sTitleTileGroup, CString sTileGroupImage, CString sTooltip, UINT nTileGroupID /* = 0 */)
{
	CFrameWnd* pFrame = this->GetParentFrame();
	CRuntimeClass* pClass = pFrame && pFrame->IsKindOf(RUNTIME_CLASS(CRowFormFrame)) ? RUNTIME_CLASS(CRowTabDialogTileGroup) : RUNTIME_CLASS(CTabDialogTileGroup);

	TileGroupInfoItem* pDlgInfoItem = (TileGroupInfoItem*)AddDialog(pClass, IDD_EMPTY_TAB);
	pDlgInfoItem->m_strTitle = sTitleTileGroup;
	pDlgInfoItem->m_pTileGroupClass = pTileGroupClass;
	pDlgInfoItem->SetSelectorImage(sTileGroupImage);
	pDlgInfoItem->m_sName = sNameTileGroup;
	pDlgInfoItem->SetSelectorTooltip(sTooltip);
	
	pDlgInfoItem->GetInfoOSL()->m_Namespace.SetChildNamespace(CTBNamespace::TABDLG, sNameTileGroup, GetNamespace());
	pDlgInfoItem->GetInfoOSL()->SetType(OSLType_TabDialog);

	pDlgInfoItem->m_nTileGroupID = nTileGroupID != 0 ? nTileGroupID : AfxGetTBResourcesMap()->GetTbResourceID(pDlgInfoItem->GetInfoOSL()->m_Namespace.ToString(), TbControls);
	pDlgInfoItem->SetDialogID(pDlgInfoItem->m_nTileGroupID);
	return pDlgInfoItem;
}

//-----------------------------------------------------------------------------
DlgInfoItem* CTileManager::CreateDlgInfoItem(CRuntimeClass*	pDialogClass, UINT nDialogID, int nOrdPos/*= -1*/)
{
	if (pDialogClass->IsDerivedFrom(RUNTIME_CLASS(CTabDialogTileGroup)) || pDialogClass->IsDerivedFrom(RUNTIME_CLASS(CRowTabDialogTileGroup)))
	{
		TileGroupInfoItem* pTileGroupInfoItem = new TileGroupInfoItem (pDialogClass, nDialogID, -1, nDialogID);
		return pTileGroupInfoItem;
	}

	return __super::CreateDlgInfoItem(pDialogClass, nDialogID, nOrdPos);
}

//-----------------------------------------------------------------------------
int CTileManager::InsertDlgInfoItem(int i, DlgInfoItem* pdlginfo)
{
	if (!pdlginfo->IsKindOf(RUNTIME_CLASS(TileGroupInfoItem)) || GetShowMode() == NORMAL)
	{
		return __super::InsertDlgInfoItem(i, pdlginfo);
	}

	TileGroupInfoItem* pTileGroupInfoItem = (TileGroupInfoItem*)pdlginfo;
	CreateSelector(i, pdlginfo, pTileGroupInfoItem->GetSelectorImage(), pTileGroupInfoItem->GetSelectorTooltip());
	ArrangeItems();
	return i;
}

//-----------------------------------------------------------------------------
void CTileManager::MoveTileGroup(TileGroupInfoItem* pItem, int nTo)
{
	if (nTo == -1)
		return;

	BOOL bOldOwns = m_pDlgInfoAr->IsOwnsElements();
	m_pDlgInfoAr->SetOwns(FALSE);
	int nOldPos = -1;
	for (int i = m_pDlgInfoAr->GetUpperBound(); i >= 0; i--)
	{
		TileGroupInfoItem* pTileGroupInfoItem = (TileGroupInfoItem*)m_pDlgInfoAr->GetAt(i);
		if (pTileGroupInfoItem != pItem)
			continue;
		
		nOldPos = i;
		break;
	}

	if (nOldPos < 0)
		return;

	m_pDlgInfoAr->RemoveAt(nOldPos);

	if (nTo > m_pDlgInfoAr->GetSize())
		m_pDlgInfoAr->Add(pItem);
	else
		m_pDlgInfoAr->InsertAt(nTo, pItem);

	MoveButtonSelector(nTo, nOldPos);

	if (m_pNormalTabber)
		m_pNormalTabber->MoveTab(nOldPos, nTo);
	if (GetShowMode() == VERTICAL_TILE)
		ArrangeItems();

	m_pDlgInfoAr->SetOwns(bOldOwns);

	SetActiveTab(nTo);
	TabDialogActivate(pItem->GetNamespace().ToUnparsedString());

	AdjustTabManager();
	
}

/////////////////////////////////////////////////////////////////////////////
// 							TileGroups
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TileGroups, Array)

/////////////////////////////////////////////////////////////////////////////
// TileGroups diagnostics

#ifdef _DEBUG
void TileGroups::AssertValid() const
{
	Array::AssertValid();
}

void TileGroups::Dump(CDumpContext& dc) const
{
	Array::Dump(dc);
	AFX_DUMP0(dc, "\nTileGroups");
}
#endif //_DEBUG
