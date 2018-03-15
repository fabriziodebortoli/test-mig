#include "stdafx.h"

#include <TbGes\TileDialog.h>
#include <TbGes\TileManager.h>
#include <TbGes\FormMng.h>

#include <TbGes\BODYEDIT.H>
#include <TbGes\JsonFormEngineEx.h>

#include "UnpinnedTilesPane.h"
#include "HotFilterManager.h"
#include "UnpinnedTilesPane.hjson"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
//			class CUnpinnedPaneEventParam definition
/////////////////////////////////////////////////////////////////////////////
class CUnpinnedPaneEventParam : public CObject
{
	DECLARE_DYNAMIC(CUnpinnedPaneEventParam)

public:
	enum Action { LOAD, CLEAR, PINSTATE_CHANGED };

	Action			m_nAction;
	CTileGroup*		m_pSourceTileGroup;
	CTBNamespace	m_Namespace;

public:
	CUnpinnedPaneEventParam(CUnpinnedPaneEventParam::Action eAction, CTileGroup* pSourceGroup = NULL, CTBNamespace aNamespace = CTBNamespace());

	Action			GetAction() const;
	CTileGroup*		GetSourceTileGroup() const;
	CTBNamespace	GetNamespace() const;
};

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CUnpinnedPaneEventParam, CObject)

//-----------------------------------------------------------------------------
CUnpinnedPaneEventParam::CUnpinnedPaneEventParam(Action eAction, CTileGroup* pSourceGroup /*NULL*/, CTBNamespace aNamespace /*""*/)
	:
	m_nAction(eAction),
	m_pSourceTileGroup(pSourceGroup),
	m_Namespace(aNamespace)
{
}

//-----------------------------------------------------------------------------
CUnpinnedPaneEventParam::Action CUnpinnedPaneEventParam::GetAction() const
{
	return m_nAction;
}

//-----------------------------------------------------------------------------
CTileGroup* CUnpinnedPaneEventParam::GetSourceTileGroup() const
{
	return m_pSourceTileGroup;
}

//-----------------------------------------------------------------------------
CTBNamespace CUnpinnedPaneEventParam::GetNamespace() const
{
	return m_Namespace;
}

/////////////////////////////////////////////////////////////////////////////
//				Class CUnpinnedTilesTileGroup Declaration 
/////////////////////////////////////////////////////////////////////////////
class CUnpinnedTilesTileGroup : public CTileGroup
{
	DECLARE_DYNCREATE(CUnpinnedTilesTileGroup)

	CUnpinnedTilesTileGroup();

protected:
	virtual void	Customize();

public:
	void DoAction(CUnpinnedPaneEventParam& aParam);

private:
	void Clear();
	void LoadFrom(CTileGroup* pTileGroup);
	void OnTilePinUnpin(CTBNamespace aNamespace);

	LRESULT	OnAction(WPARAM wParam, LPARAM lParam);
	LRESULT	OnSuspendLayoutChanged(WPARAM wParam, LPARAM lParam);
	DECLARE_MESSAGE_MAP();
};

struct OriginalCtrl
{
	OriginalCtrl(UINT nID, DataObj* pValue, BOOL bIsRO)
		:
		m_nID(nID),
		m_pOriginalDataObj(NULL),
		m_bIsReadOnly(bIsRO)
	{
		m_pOriginalDataObj = pValue->Clone();
	}

	~OriginalCtrl()
	{
		SAFE_DELETE(m_pOriginalDataObj);
	}

public:
	UINT		m_nID;
	DataObj*	m_pOriginalDataObj;
	BOOL		m_bIsReadOnly;
};

/////////////////////////////////////////////////////////////////////////////
//			Class CUnpinnedTile Declaration & Implementation
///////////////////////////////////////////////////////////////////////////////
class CUnpinnedTile : public CJsonTileDialog
{
	DECLARE_DYNCREATE(CUnpinnedTile)

	CBaseTileDialog*	m_pOriginalTile;

public:
	CUnpinnedTile() : CJsonTileDialog() {}
	CUnpinnedTile(UINT nIDD);
	~CUnpinnedTile()
	{
		if (!m_pOriginalControls)
			return;

		for (int i = 0; i < m_pOriginalControls->GetCount(); i++)
			SAFE_DELETE(m_pOriginalControls->GetAt(i));

		m_pOriginalControls->RemoveAll();

		SAFE_DELETE(m_pOriginalControls)
	}
private:
	CArray<OriginalCtrl*>* m_pOriginalControls = NULL;

public:
	void AttachOriginalTile(CBaseTileDialog* pTile);
	void SaveOriginalValues();
	void RestoreToOriginalValues();
	void ClearControls();

	virtual BOOL IsVisible();
	virtual void SetPinned(BOOL bPinned = TRUE);
	void AdjustVisibility();

protected:
	virtual void OnUpdateControls(BOOL bParentIsVisible = TRUE);
	virtual void DoPinUnpin();
	//virtual void BuildJsonControlLinks() { /*DOES NOTHING*/ }
};

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CUnpinnedTile, CJsonTileDialog)

//-----------------------------------------------------------------------------
CUnpinnedTile::CUnpinnedTile(UINT nIDD)
	:
	CJsonTileDialog(/*_T("Filter"), 0*/),
	m_pOriginalTile(NULL)
{

	SetMinHeight(ORIGINAL);
	// original dialog may be larger than "mini" size, setting min size to zero will allow
	// the layout to shrink it
	SetMinWidth(0);
	SetFlex(0);
	SetPinnable();
	SetPinned(FALSE);
	SetCollapsible();
}

//-----------------------------------------------------------------------------
void CUnpinnedTile::AttachOriginalTile(CBaseTileDialog* pTile)
{
	m_pOriginalTile = pTile;
	if (m_pOriginalTile)
	{
		GetInfoOSL()->m_Namespace.SetNamespace(m_pOriginalTile->GetNamespace());
		SetResourceModule(m_pOriginalTile->GetResourceModule());
	}
}

// non posso usare la Enable della tile perche' non ci sono ControlLinks e
// quindi non avrebbe effetto. Si tratta tutti di controlli BCG puri.
//-----------------------------------------------------------------------------
void CUnpinnedTile::OnUpdateControls(BOOL bParentIsVisible /*TRUE*/)
{
	__super::OnUpdateControls(bParentIsVisible);
	for (CWnd* pChild = GetWindow(GW_CHILD); pChild != NULL; pChild = pChild->GetWindow(GW_HWNDNEXT))
	{
		if (pChild->IsKindOf(RUNTIME_CLASS(CCollapseButton)) || pChild->IsKindOf(RUNTIME_CLASS(CPinButton)))
			continue;
		
		pChild->EnableWindow(FALSE);
	}
}

//-----------------------------------------------------------------------------
BOOL CUnpinnedTile::IsVisible()
{
	return m_pOriginalTile ? !m_pOriginalTile->IsPinned() : m_bVisible;
}

//-----------------------------------------------------------------------------
void CUnpinnedTile::SetPinned(BOOL bPinned /*= TRUE*/)
{ 
	// I cannot call super 
	m_bPinned = FALSE; 
	RefreshPinButton();
}

//------------------------------------------------------------------------------
void CUnpinnedTile::SaveOriginalValues()
{
	if (!m_pOriginalTile || !m_pOriginalTile->IsPinnable())
		return;

	if (!m_pOriginalControls)
		m_pOriginalControls = new CArray<OriginalCtrl*>();
	else
	{
		for (int i = 0; i < m_pOriginalControls->GetCount(); i++)
			SAFE_DELETE(m_pOriginalControls->GetAt(i));

		m_pOriginalControls->RemoveAll();
	}

	for (int i = 0; i < m_pOriginalTile->GetControlLinks()->GetCount(); i++)
	{
		CWnd* pWnd = m_pOriginalTile->GetControlLinks()->GetAt(i);

		if (!pWnd)
			continue;

		UINT nID = pWnd->GetDlgCtrlID();

		CParsedCtrl* pCtrl = GetParsedCtrl(pWnd);

		if (!pCtrl)
			continue;

		DataObj* pDataObj = pCtrl->GetCtrlData();

		if (pDataObj)
		{
			BOOL bIsReadOnly = pDataObj->IsReadOnly();
			m_pOriginalControls->Add(new OriginalCtrl(nID, pDataObj, bIsReadOnly));
		}

	}
}

//----------------------------------------------------------------------------------
void CUnpinnedTile::RestoreToOriginalValues()
{
	if (!m_pOriginalTile || !m_pOriginalControls)
		return;

	//reset dataobj's
	for (int i = 0; i < m_pOriginalControls->GetCount(); i++)
	{
		OriginalCtrl* pStructCtrl = m_pOriginalControls->GetAt(i);

		if (!pStructCtrl)
			continue;

		UINT nID = pStructCtrl->m_nID;

		CWnd* pOriginalWnd = m_pOriginalTile->GetDlgItem(nID);

		if (!pOriginalWnd)
			continue;

		CParsedCtrl* pOriginalCtrl = GetParsedCtrl(pOriginalWnd);

		if (!pOriginalCtrl)
			continue;

		DataObj* pOriginalDataObj = pOriginalCtrl->GetCtrlData();

		pOriginalDataObj->Assign(*pStructCtrl->m_pOriginalDataObj);
		pOriginalCtrl->SetDataReadOnly(pStructCtrl->m_bIsReadOnly);
		pOriginalCtrl->UpdateCtrlView();
		pOriginalCtrl->UpdateCtrlStatus();
	}

	
}

//--------------------------------------------------------------------------------
void CUnpinnedTile::ClearControls()
{
	for (int i = 0; i < GetControlLinks()->GetCount(); i++)
	{
		CWnd* pWnd = GetControlLinks()->GetAt(i);

		if (!pWnd)
			continue;

		CParsedCtrl* pCtrl = GetParsedCtrl(pWnd);

		if (!pCtrl)
			continue;

		if (pCtrl->GetCtrlData())
			pCtrl->GetCtrlData()->Clear();
		pCtrl->UpdateCtrlView();
	}
}

//-----------------------------------------------------------------------------
void CUnpinnedTile::DoPinUnpin()
{
	if (!m_pOriginalTile)
		return;

	m_pOriginalTile->SetPinned(!m_pOriginalTile->IsPinned());

	CUnpinnedTilesTileGroup* pTileGroup = dynamic_cast<CUnpinnedTilesTileGroup*>(GetParentTileGroup());
	if (pTileGroup)
	{
		pTileGroup->DoAction(CUnpinnedPaneEventParam(CUnpinnedPaneEventParam::PINSTATE_CHANGED, NULL, GetNamespace()));
	}
}

//-----------------------------------------------------------------------------
void CUnpinnedTile::AdjustVisibility()
{
	if (!m_pOriginalTile)
		return;

	SetCollapsed();
	// pin button image adjustment
	SetPinned();
	// show and hide tile from the list
	ShowWindow(IsVisible() ? SW_SHOW : SW_HIDE);
}

/////////////////////////////////////////////////////////////////////////////
//				Class CUnpinnedTilesTileGroup Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CUnpinnedTilesTileGroup, CTileGroup)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CUnpinnedTilesTileGroup, CTileGroup)
	ON_MESSAGE(UM_PIN_UNPIN_ACTIONS, OnAction)
	ON_MESSAGE(UM_LAYOUT_SUSPENDED_CHANGED, OnSuspendLayoutChanged)
END_MESSAGE_MAP()
//-----------------------------------------------------------------------------
CUnpinnedTilesTileGroup::CUnpinnedTilesTileGroup()
{
}

//-----------------------------------------------------------------------------
void CUnpinnedTilesTileGroup::Customize()
{
	SetLayoutType(CLayoutContainer::COLUMN);
	SetLayoutAlign(CLayoutContainer::BEGIN);
	SetTileDialogStyle(AfxGetTileDialogStyleFilter());
	SetTileCollapsible();
}

//---------------------------------------------------------------------------------------------------------------
void CUnpinnedTilesTileGroup::DoAction(CUnpinnedPaneEventParam& aParam)
{
	CTileGroup* pSourceTileGroup = aParam.GetSourceTileGroup();

	switch (aParam.GetAction())
	{
	case CUnpinnedPaneEventParam::LOAD:
		LoadFrom(pSourceTileGroup);
		break;
	case CUnpinnedPaneEventParam::CLEAR:
		Clear();
		break;
	default:
		if (GetTileDialogs()->GetSize())
			OnTilePinUnpin(aParam.GetNamespace());
		break;
	}

	// aggiornamento 
	RequestRelayout();

	if (pSourceTileGroup)
		pSourceTileGroup->RequestRelayout();
}

//---------------------------------------------------------------------------------------------------------------
LRESULT	CUnpinnedTilesTileGroup::OnAction(WPARAM wParam, LPARAM lParam)
{
	if (wParam)
	{
		CUnpinnedPaneEventParam* pParam = (CUnpinnedPaneEventParam*)wParam;
		DoAction(*pParam);
	}

	return 0L;
}

// Questo refresh e' necessario poiche' nei wizard la action di Load avviene
// ancora durante il tab sel changed che ha il layout sospeso. Se non fa un
// relayout dopo la ResumeLayout non mi si disegnano bene le tile nel pannello
//-----------------------------------------------------------------------------
LRESULT	CUnpinnedTilesTileGroup::OnSuspendLayoutChanged(WPARAM wParam, LPARAM lParam)
{
	CDockableFrame* pFrame = (CDockableFrame*)wParam;

	if (pFrame && !pFrame->IsLayoutSuspended())
		RequestRelayout();
	return 0L;
}

//-----------------------------------------------------------------------------
void CUnpinnedTilesTileGroup::OnTilePinUnpin(CTBNamespace aNamespace)
{
	CUnpinnedTile* pUnpinned = dynamic_cast<CUnpinnedTile*>(GetTileDialog(aNamespace));
	if (!pUnpinned)
		return;

	if (!pUnpinned->m_pOriginalTile->IsPinned() && !pUnpinned->m_pOriginalTile->GetResetValuesAfterUnpin())
		pUnpinned->SaveOriginalValues();

	if (!pUnpinned->m_pOriginalTile->IsPinned())
		pUnpinned->ClearControls();

	//manage original tile
	if (!pUnpinned->m_pOriginalTile->IsPinned())
		pUnpinned->RestoreToOriginalValues();

	pUnpinned->AdjustVisibility();

	// Form Manager Flags Update 
	CAbstractFormDoc* pDoc = dynamic_cast<CAbstractFormDoc*>(GetDocument());
	ASSERT_VALID(pDoc);
	if (pDoc && pDoc->m_pFormManager)
		pDoc->m_pFormManager->SetDialogsPinState(aNamespace, pUnpinned->m_pOriginalTile->IsPinned());
}

//-----------------------------------------------------------------------------
void CUnpinnedTilesTileGroup::Clear()
{

	
	for (int i = GetTileDialogs()->GetUpperBound(); i >= 0; i--)
	{
		CUnpinnedTile* pUnpinned = dynamic_cast<CUnpinnedTile*>(GetTileDialogs()->GetAt(i));
		if (pUnpinned && GetLayoutContainer())
		{
			GetLayoutContainer()->RemoveChildElement(pUnpinned);
			pUnpinned->DestroyWindow();
		}
	}
	
	GetTileDialogs()->RemoveAll();

}

//-----------------------------------------------------------------------------
void CUnpinnedTilesTileGroup::LoadFrom(CTileGroup* pGroup)
{
	Clear();

	for (int i = 0; i < pGroup->GetTileDialogs()->GetSize(); i++)
	{
		CBaseTileDialog* pDialog = pGroup->GetTileDialogs()->GetAt(i);

		if (!pDialog->IsPinnable())
			continue;

		CUnpinnedTile* pUnpinned = NULL;	

		CWndObjDescription* pWndDescription = pDialog->GetJsonContext()->m_pDescription->DeepClone();

		CWndTileDescription* pTileDesc = dynamic_cast<CWndTileDescription*>(pWndDescription);
		if (pTileDesc)
		{
			pTileDesc->m_Size = TILE_MINI;
			pTileDesc->m_nMinWidth = -2 /*ORIGINAL*/;
		}

		UINT nTileID = CJsonFormEngineObj::GetID(pWndDescription);
		if (!pDialog->GetJsonContext()->CanCreateControl(pWndDescription, nTileID))
			continue;
		CObject* pTileOwner = NULL;
		if (pWndDescription->m_Type == CWndObjDescription::HotFilter)
			pTileOwner = ((CJsonContext*)pDialog->GetJsonContext())->CreateHotFilter(nTileID, pWndDescription);
		CBaseDocument* pDoc = GetDocument();
		CRuntimeClass* pClass = pWndDescription->m_Type == CWndObjDescription::HotFilter
			? NULL
			: pDoc ? pDoc->GetControlClass(nTileID) : NULL;
		if (!pClass)
			pClass = RUNTIME_CLASS(CUnpinnedTile);
		ASSERT(pClass->IsDerivedFrom(RUNTIME_CLASS(CUnpinnedTile)));

		pUnpinned = (CUnpinnedTile*)pClass->CreateObject();
		CJsonContext* pNewContext = pWndDescription->m_Type == CWndObjDescription::HotFilter
			? new CHotFilterJsonContext(pWndDescription->m_strControlClass)
			: (CJsonContext*)CJsonContext::Create();

		pNewContext->Assign((CJsonContext*)pDialog->GetJsonContext());
		pNewContext->m_pDescription = pWndDescription;
		pNewContext->m_bOwnDescription = true;
		pUnpinned->AssignContext(pNewContext);
		pUnpinned->SetTileSize(TILE_MINI);

		__super::AddTile(GetLayoutContainer(), pUnpinned, nTileID, pDialog->GetTitle(), TILE_MINI, AUTO, pTileOwner);
		
		pUnpinned->SetCollapsed();
		pUnpinned->SetCollapsible();

		pUnpinned->AttachOriginalTile(pDialog);
		pUnpinned->SaveOriginalValues();
		
		pUnpinned->InitializeLayout();
		pUnpinned->AdjustVisibility();

	}
}

/////////////////////////////////////////////////////////////////////////////
//			class CUnpinnedTilesView Declaration and Implementation
/////////////////////////////////////////////////////////////////////////////
class CUnpinnedTilesView : public CAbstractFormView
{
	DECLARE_DYNCREATE(CUnpinnedTilesView)

public:
	CUnpinnedTilesView();

public:
	virtual	void BuildDataControlLinks();
};

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CUnpinnedTilesView, CAbstractFormView)

//-----------------------------------------------------------------------------
CUnpinnedTilesView::CUnpinnedTilesView()
	:
	CAbstractFormView(_NS_VIEW("UnpinnedTilesView"), IDD_UNPINNED_TILES_PANE)
{
}

//-----------------------------------------------------------------------------
void CUnpinnedTilesView::BuildDataControlLinks()
{
	AddTileGroup
	(
		IDC_UNPINNED_TILES_TILEGROUP,
		RUNTIME_CLASS(CUnpinnedTilesTileGroup),
		_NS_TILEGRP("UnpinnedTilesGroup")
	);
}

//////////////////////////////////////////////////////////////////////////////
//             class CUnpinnedTilesPane implementation			//
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CUnpinnedTilesPane, CTaskBuilderDockPane)

//-----------------------------------------------------------------------------
CUnpinnedTilesPane::CUnpinnedTilesPane()
	:
	CTaskBuilderDockPane(RUNTIME_CLASS(CUnpinnedTilesView))
{
	SetMinWidth(0);
}

//---------------------------------------------------------------------------------------------------------------
CUnpinnedTilesPane* CUnpinnedTilesPane::Create(CMasterFrame* pFrame, BOOL bForceInitialUpdate /*FALSE*/, CRuntimeClass* pPaneClass /*NULL*/)
{
	CRuntimeClass* pUnpinnedClass = pPaneClass;
	if (!pUnpinnedClass)
		pUnpinnedClass = RUNTIME_CLASS(CUnpinnedTilesPane);

	// create the pane large enough to display a MINI tile
	CSize size
	(
		UnScalePix(BaseWidthPX),
		UnScalePix(MulDiv(AfxGetThemeManager()->GetTileMaxHeightUnit(), AfxGetThemeManager()->GetBaseUnitsHeight(), 100))
	);
	
	CUnpinnedTilesPane* pPane = (CUnpinnedTilesPane*) pFrame->CreateDockingPane
		(
			pUnpinnedClass,
			IDD_UNPINNED_TILES_PANE,
			_T("UnpinnedTiles"),
			_TB("Add Filters"),
			CBRS_LEFT | CBRS_HIDE_INPLACE,
			size
		);
	if (pPane)
	{
		pPane->SetAutoHideMode(TRUE, CBRS_LEFT | CBRS_HIDE_INPLACE, NULL, FALSE);
		if (pFrame->GetDockPane()->IsInCreateFrame() || bForceInitialUpdate)
			pPane->SendMessageToDescendants(WM_INITIALUPDATE, 0, 0);
	}

	return pPane;
}

/////////////////////////////////////////////////////////////////////////////
//				Class CPinnedTilesTileGroup Implementation
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPinnedTilesTileGroup, CTileGroup)
BEGIN_MESSAGE_MAP(CPinnedTilesTileGroup, CTileGroup)
	ON_WM_DESTROY()
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CPinnedTilesTileGroup::CPinnedTilesTileGroup()
	:
	m_pUnpinnedTilesPane(NULL),
	m_bOwnsPane(FALSE)
{
}

//-----------------------------------------------------------------------------
void CPinnedTilesTileGroup::OnDestroy()
{
	ClearUnpinnedTilesPane();
	if (m_bOwnsPane)
		DestroyUnpinnedTilePane();
}

//-----------------------------------------------------------------------------
void CPinnedTilesTileGroup::ClearUnpinnedTilesPane()
{
	if (!m_pUnpinnedTilesPane)
		return;

	// prima lo metto in autohide
	if (!m_pUnpinnedTilesPane->IsAutoHideMode())
		m_pUnpinnedTilesPane->SetAutoHideMode(TRUE, CBRS_LEFT | CBRS_HIDE_INPLACE);
	if (!m_bOwnsPane)
		m_pUnpinnedTilesPane->EnablePane(FALSE);

	CUnpinnedPaneEventParam aParam(CUnpinnedPaneEventParam::CLEAR);
	m_pUnpinnedTilesPane->SendMessageToDescendants(UM_PIN_UNPIN_ACTIONS, (WPARAM)&aParam);
}

//-----------------------------------------------------------------------------
void CPinnedTilesTileGroup::LoadUnpinnedTilesPane()
{
	if (!m_pUnpinnedTilesPane)
		return;

	CUnpinnedPaneEventParam aParam(CUnpinnedPaneEventParam::LOAD, this);
	m_pUnpinnedTilesPane->SendMessageToDescendants(UM_PIN_UNPIN_ACTIONS, (WPARAM)&aParam);

	// prima lo metto in autohide
	if (!m_pUnpinnedTilesPane->IsAutoHideMode())
		m_pUnpinnedTilesPane->SetAutoHideMode(TRUE, CBRS_LEFT | CBRS_HIDE_INPLACE);

	if (!m_bOwnsPane)
		m_pUnpinnedTilesPane->EnablePane(TRUE);
}

//-----------------------------------------------------------------------------
void CPinnedTilesTileGroup::OnTileDialogUnpin(CBaseTileDialog* pTileDlg)
{
	if (!pTileDlg->IsPinnable() || !m_pUnpinnedTilesPane)
		return;

	CUnpinnedPaneEventParam aParam(CUnpinnedPaneEventParam::PINSTATE_CHANGED, this, pTileDlg->GetNamespace());
	m_pUnpinnedTilesPane->SendMessageToDescendants(UM_PIN_UNPIN_ACTIONS, (WPARAM)&aParam);
}

//-----------------------------------------------------------------------------
void CPinnedTilesTileGroup::AttachUnpinnedTilesPane(CUnpinnedTilesPane* pPane)
{
	ASSERT(!m_pUnpinnedTilesPane);
	m_pUnpinnedTilesPane = pPane;
}

//-----------------------------------------------------------------------------
void CPinnedTilesTileGroup::CreateUnpinnedTilePane()
{
	CMasterFrame* pFrame = dynamic_cast<CMasterFrame*>(GetParentFrame());
	if (pFrame)
		m_pUnpinnedTilesPane = CUnpinnedTilesPane::Create(pFrame, !m_bOwnsPane);
}

//-----------------------------------------------------------------------------
void CPinnedTilesTileGroup::DestroyUnpinnedTilePane()
{
	if (!m_pUnpinnedTilesPane)
		return;

	CMasterFrame* pFrame = dynamic_cast<CMasterFrame*>(GetParentFrame());
	if (pFrame)
		pFrame->DestroyPane(m_pUnpinnedTilesPane);
}

//-----------------------------------------------------------------------------
void CPinnedTilesTileGroup::OnAfterCustomize()
{
	__super::OnAfterCustomize();
	CAbstractFormDoc* pDoc = dynamic_cast<CAbstractFormDoc*>(GetDocument());
	if (!pDoc)
		return;

	if (!m_pUnpinnedTilesPane && m_bOwnsPane)
		CreateUnpinnedTilePane();

	// applies form manager customizations
	if (pDoc->m_pFormManager)
	{
		for (int i = 0; i < GetTileDialogs()->GetSize(); i++)
		{
			CBaseTileDialog* pTileDialog = GetTileDialogs()->GetAt(i);
			BOOL bPinned = FALSE, bDummy = FALSE;
			if (pTileDialog->IsPinnable())
			{
				if (pDoc->m_pFormManager->HasDialogCustomized(pTileDialog->GetNamespace(), bPinned, bDummy))
					pTileDialog->SetPinned(bPinned);
				else // questo codice serve per allineare lo stato del form manager sulla base delle eventuali modifiche
					 // gestionali fatti durante il codice di Customize
					pDoc->m_pFormManager->SetDialogsPinState(pTileDialog->GetNamespace(), pTileDialog->IsPinned());
			}
		}
	}

	// load tiles in pane
	LoadUnpinnedTilesPane();
	// form manager modification must be reenabled after Load
	if (pDoc->m_pFormManager)
		pDoc->m_pFormManager->EnableDialogStateSave();
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CPinnedTilesTileGroup::AddJsonTile(UINT nDialogID, CLayoutContainer* pContainer)
{
	CJsonTileDialog* pDlg = dynamic_cast<CJsonTileDialog*>(__super::AddJsonTile(nDialogID, pContainer));

	// il metodo IsInitiallyPinned() funziona al contrario di bInitiallyUnpinned
	if (!pDlg->IsInitiallyPinned())
		pDlg->ForceSetPinned(pDlg->IsInitiallyPinned());

	return pDlg;
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CPinnedTilesTileGroup::AddTile(CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex, BOOL bInitiallyUnpinned, CObject* pOwner /*= NULL*/)
{
	CBaseTileDialog* pDlg = __super::AddTile(pClass, nDialogID, sTileTitle, tileSize, flex, pOwner);

	if (bInitiallyUnpinned)
		pDlg->ForceSetPinned(!bInitiallyUnpinned);

	return pDlg;
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CPinnedTilesTileGroup::AddTile(CLayoutContainer* pContainer, CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex, BOOL bInitiallyUnpinned, CObject* pOwner /*= NULL*/)
{
	CBaseTileDialog* pDlg = __super::AddTile(pContainer, pClass, nDialogID, sTileTitle, tileSize, flex, pOwner);

	if (bInitiallyUnpinned)
		pDlg->ForceSetPinned(!bInitiallyUnpinned);


	return pDlg;
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------
CBaseTileDialog* CPinnedTilesTileGroup::AddTile(CLayoutContainer* pContainer, CBaseTileDialog* pTileDialog, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex /*= AUTO*/, CObject* pOwner /*= NULL*/)
{
	CBaseTileDialog* pDlg = __super::AddTile(pContainer, pTileDialog, nDialogID, sTileTitle, tileSize, flex, pOwner);

	return pDlg;
}

//-----------------------------------------------------------------------------
CBaseTileDialog* CPinnedTilesTileGroup::AddTile(CTilePanel* pPanel, CRuntimeClass* pClass, UINT nDialogID, CString sTileTitle, TileDialogSize tileSize, int flex, BOOL bInitiallyUnpinned, CObject* pOwner /*= NULL*/)
{
	CBaseTileDialog* pDlg = pPanel->AddTile(pClass, nDialogID, sTileTitle, tileSize, flex, bInitiallyUnpinned, pOwner);

	if (bInitiallyUnpinned)
		pDlg->ForceSetPinned(!bInitiallyUnpinned);

	return pDlg;
}

//-----------------------------------------------------------------------------
void CPinnedTilesTileGroup::SetOwnsPane(BOOL bOwns)
{
	m_bOwnsPane = bOwns;
}