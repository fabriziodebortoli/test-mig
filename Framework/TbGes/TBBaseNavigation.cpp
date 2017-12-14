
#include "stdafx.h"

// local declaration
#include "TBBaseNavigation.h"
#include "TBBaseNavigation.hjson"

#include <TbGes\ExtDocView.h>


#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//////////////////////////////////////////////////////////////////////////////
//									TNodeDetail							//
//////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TNodeDetail, SqlVirtualRecord)

//-----------------------------------------------------------------------------
TNodeDetail::TNodeDetail(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("TNodeDetail"))
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TNodeDetail::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_STR(_NS_LFLD("FieldName"),			l_FieldName,	32)
		LOCAL_STR(_NS_LFLD("FieldValue"),			l_FieldValue,	32)
		LOCAL_DATA(_NS_LFLD("HasHyperLink"),		l_HasHyperlink)
		LOCAL_DATA(_NS_LFLD("IsSeparator"),			l_IsSeparator)
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TNodeDetail::GetStaticName() {
	return _NS_TBL("TNodeDetail");
}

//////////////////////////////////////////////////////////////////////////////
//									DBTNodeDetail							//
//////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTNodeDetail, DBTSlaveBuffered)

//--------------------------------------------------------------------------------
DBTNodeDetail::DBTNodeDetail(CRuntimeClass* pClass, CAbstractFormDoc* pDoc)
	:
	DBTSlaveBuffered(pClass, pDoc, _NS_DBT("DBTNodeDetail"), ALLOW_EMPTY_BODY, FALSE)
{

}

/////////////////////////////////////////////////////////////////////////////
//					class CTBBaseNavigationFrame Implementation
/////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTBBaseNavigationFrame, CTBActivityFrame)

//------------------------------------------------------------------------------
CTBBaseNavigationFrame::CTBBaseNavigationFrame()
	:
	CTBActivityFrame()
{

}

//----------------------------------------------------------------------------------
BOOL CTBBaseNavigationFrame::OnCustomizeJsonToolBar()
{
	BOOL bRet = __super::OnCustomizeJsonToolBar();
	return bRet && CreateJsonToolbar(IDD_TB_BASE_NAVIGATION_TOOLBAR);
}

//-----------------------------------------------------------------------------------------------
BOOL CTBBaseNavigationFrame::CreateAuxObjects(CCreateContext*	pCreateContext)
{
	__super::CreateAuxObjects(pCreateContext);

	CTBBaseNavigationDocument* pDoc = (CTBBaseNavigationDocument*)GetDocument();

	switch (pDoc->m_eSplitType)
	{
		case E_BASE_NAVIGATION_SPLITTYPE::BASE_NAVIGATION_HORIZONTAL:
			pDoc->m_pSplitter = CreateSplitter(RUNTIME_CLASS(CTaskBuilderSplitterWnd), 2, 1);	//TODO - generalization m,n ?!
			break;
		case E_BASE_NAVIGATION_SPLITTYPE::BASE_NAVIGATION_VERTICAL:
			pDoc->m_pSplitter = CreateSplitter(RUNTIME_CLASS(CTaskBuilderSplitterWnd), 1, 2);	//TODO - generalization m,n  ?!
			break;
	}

	if (!pDoc->m_pSplitter)
		return FALSE;

	//add windows to splitter
	pDoc->m_pSplitter->AddWindow(pDoc->m_pFirstView, pCreateContext);
	pDoc->m_pSplitter->AddWindow(pDoc->m_pSecondView, pCreateContext, 1);

	//initialization of splitter
	pDoc->m_pSplitter->SetSplitRatio((float)1.);
	pDoc->m_pSplitter->RecalcLayout();

	switch (pDoc->m_eSplitType)
	{
		case E_BASE_NAVIGATION_SPLITTYPE::BASE_NAVIGATION_HORIZONTAL:
			pDoc->m_pSplitter->SplitHorizontally();
			break;
		case E_BASE_NAVIGATION_SPLITTYPE::BASE_NAVIGATION_VERTICAL:
			pDoc->m_pSplitter->SplitVertically();
			break;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CTBBaseNavigationFrame::OnPopulatedDropDown(UINT nIDCommand)
{
	CTBToolBarMenu menu;
	menu.CreateMenu();

	if (nIDCommand != ID_TB_BASE_NAVIGATION_ACTIONS)
		return __super::OnPopulatedDropDown(nIDCommand);

	CTBBaseNavigationDocument* pDoc = (CTBBaseNavigationDocument*)GetDocument();

	menu.RemoveAll();

	pDoc->BuildActions(menu);

	if (GetTabbedToolBar())
		GetTabbedToolBar()->UpdateDropdownMenu(nIDCommand, &menu);

	return TRUE;
}


//////////////////////////////////////////////////////////////////////////////
//							CTBBaseNavigationDocument						//
//////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTBBaseNavigationDocument, CTBActivityDocument)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTBBaseNavigationDocument, CTBActivityDocument)
	ON_CONTROL				(UM_TREEVIEWADV_SELECTION_CHANGED,			IDC_TB_BASE_NAVIGATION_TREE,		OnSelectionNodeChanged)
	ON_CONTROL				(UM_TREEVIEWADV_CONTEXT_MENU_ITEM_CLICK,	IDC_TB_BASE_NAVIGATION_TREE,		OnCtxMenuItemClick)
	ON_CONTROL				(UM_TREEVIEWADV_DRAG_DROP,					IDC_TB_BASE_NAVIGATION_TREE,		OnDragDrop)
	ON_CONTROL				(UM_TREEVIEWADV_NODE_CHANGED,				IDC_TB_BASE_NAVIGATION_TREE,		OnNodeChanged)
	ON_CONTROL				(UM_TREEVIEWADV_MOUSE_DOUBLE_CLICK,			IDC_TB_BASE_NAVIGATION_TREE,		OnNodeDoubleClick)
	
	ON_COMMAND				(ID_TB_BASE_NAVIGATION_EXPAND_COLLAPSE,											OnExpandCollapse)
	ON_COMMAND				(ID_TB_BASE_NAVIGATION_SHOW_DETAILS,											OnShowHideDetails)
	ON_COMMAND				(ID_TB_BASE_NAVIGATION_FIND,													OnFind)

	ON_CONTROL				(BEN_ROW_CHANGED,							IDC_TB_BASE_NAVIGATION_GRID_DETAIL, OnDetailGridRowChanged)

	ON_UPDATE_COMMAND_UI	(ID_TB_BASE_NAVIGATION_ACTIONS,													OnEnableActions)
	ON_UPDATE_COMMAND_UI	(ID_TB_BASE_NAVIGATION_EXPAND_COLLAPSE,											OnEnableExpand)
	ON_UPDATE_COMMAND_UI	(ID_TB_BASE_NAVIGATION_SHOW_DETAILS,											OnEnableShowDetails)
	ON_UPDATE_COMMAND_UI	(ID_TB_BASE_NAVIGATION_FIND,													OnEnableFind)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------
CTBBaseNavigationDocument::CTBBaseNavigationDocument()
	:
	m_pDBTNodeDetail		(NULL),
	m_pSplitter				(NULL),
	m_eSplitType			(E_BASE_NAVIGATION_SPLITTYPE::BASE_NAVIGATION_HORIZONTAL),
	m_bTreeViewLoaded		(FALSE),
	m_bShowDetailsSettings	(TRUE),
	m_bShowDetails			(FALSE),
	m_nSplitterRatio		((float)(.6)),
	m_pFirstView			(NULL),
	m_pSecondView			(NULL),
	m_LastSelectedNode		(_T("")),
	m_pControlFieldValue	(NULL)
{
	SetResourceModule	(GetDllInstance(RUNTIME_CLASS(CTBBaseNavigationDocument)));
}

//-----------------------------------------------------------------------------
CTBBaseNavigationDocument::~CTBBaseNavigationDocument()
{
	SAFE_DELETE(m_pDBTNodeDetail);
}

//---------------------------------------------------------------------------------------------------------------
BOOL CTBBaseNavigationDocument::OnAttachData()
{
	__super::OnAttachData();

	m_pDBTNodeDetail = new DBTNodeDetail(RUNTIME_CLASS(TNodeDetail), this);

	return TRUE;
}

//----------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::RemoveNodeDetails()
{
	//comunque dovrei averlo valorizzato, pero' se uno non fa "__super::OnAttachData" e' NULL (ma cmq la __super::OnAttachData dovvrebbe essere fatta)
	if (m_pDBTNodeDetail)
		m_pDBTNodeDetail->RemoveAll();
}

//---------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::ShowEmptyDetails()
{
	
	RemoveNodeDetails();

	UpdateDataView();
}

//------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::ClearTreeAndEmptyDetails()
{
	m_bTreeViewLoaded = FALSE;
	ClearTree();

	ShowEmptyDetails();
}

//------------------------------------------------------------------------------------------------------
BOOL CTBBaseNavigationDocument::LoadTree()
{
	m_bTreeViewLoaded = FALSE;

	BeginWaitCursor();
	ClearTree();
	BOOL bOK = PopulateTree();
	m_LastSelectedNode = GetSelectedNodeKey();
	m_bTreeViewLoaded = bOK;
	EndWaitCursor();

	return bOK;
}


//------------------------------------------------------------------------------------------------------
BOOL CTBBaseNavigationDocument::OnLoadDBT()
{
	return LoadTree();
}

//---------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	UINT nIDC = (UINT)pCtrl->GetCtrlID();

	if (nIDC != IDC_TB_BASE_NAVIGATION_TREE)
		return;

	SetTreeViewObject(dynamic_cast<CTreeViewAdvCtrl*>(pCtrl));

	SetBackColorTreeView(AfxGetThemeManager()->GetTileDialogStaticAreaBkgColor());
	
	InitTree();
	
}

//------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnColumnInfoCreated(ColumnInfo* pColInfo)
{
	UINT nIDC = pColInfo->GetCtrlID();
	CParsedCtrl* pCtrl = pColInfo->GetParsedCtrl();

	if (m_pControlFieldValue || nIDC != IDC_TB_BASE_NAVIGATION_GRID_COL_VALUE)
		return;

	m_pControlFieldValue = dynamic_cast<CStrStatic*>(pColInfo->GetParsedCtrl());
}

//---------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::ManageDescriptionDetailHKL(HotKeyLinkObj* pHKLObj)
{
	ASSERT_VALID(m_pControlFieldValue);
	
	if (!m_pControlFieldValue)
		return;

	m_pControlFieldValue->ReattachHotKeyLink(pHKLObj);
}

//---------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnFrameCreated()
{
	__super::OnFrameCreated();
	m_pSplitter = GetMasterFrame()->GetSplitter();
}

//-------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnSelectionNodeChanged()
{
	if (!m_bTreeViewLoaded)
		return;

	CString aKey = GetSelectedNodeKey();

	if (aKey.IsEmpty())
	{
		ShowEmptyDetails();
		return;
	}

	RemoveNodeDetails();

	DoSelectionNodeChanged(aKey);

	m_LastSelectedNode = aKey;
}

//-----------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnCtxMenuItemClick()
{
	if (!m_bTreeViewLoaded)
		return;

	CString aKey = GetSelectedNodeKey();

	if (aKey.IsEmpty())
		return;

	DoCtxMenuItemClick(aKey);
}

//---------------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnDragDrop()
{
	DoDragDrop();
}

//------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnNodeChanged()
{
	if (!m_bTreeViewLoaded)
		return;

	CString aKey = GetLastChangedNodeKey();
	
	if (aKey.IsEmpty())
		return;

	DoNodeChanged(aKey, IsNodeChecked(aKey));
}

//------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnNodeDoubleClick()
{
	CString aKey = GetSelectedNodeKey();

	if (aKey.IsEmpty())
		return;

	DoNodeDoubleClick(aKey);
}

//---------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnDetailGridRowChanged()
{
	DoDetailGridRowChanged();
}

//------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::ManageSplitter(const BOOL& bShowDetails)
{
	m_bShowDetails = bShowDetails;

	BOOL bSplit = m_bShowDetailsSettings && m_bShowDetails;

	RecalculateHorizontalSplitter(bSplit);
}

//---------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::RecalculateHorizontalSplitter(BOOL bSplit)
{
	if (!m_pSplitter)
		return;

	float nSplitRatio = bSplit ? m_nSplitterRatio : 1;

	m_pSplitter->SetSplitRatio(nSplitRatio);

	CRect rect;
	GetFrame()->GetWindowRect(&rect);

	m_pSplitter->SetColumnInfo(0, (int)(rect.Width() * nSplitRatio), 1);
	m_pSplitter->SetColumnInfo(1, (int)(rect.Width() * (1 - nSplitRatio)), 1);

	m_pSplitter->RecalcLayout();
}

//---------------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::ManageExtractData()
{
	ManageSplitter(TRUE);
}

//---------------------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::ManageUndoExtraction()
{
	ClearTreeAndEmptyDetails();

	ManageSplitter(FALSE);
}

//-----------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::BuildActions(CTBToolBarMenu& menu)
{
	if (!m_bTreeViewLoaded)
		return;

	CString aKey = GetSelectedNodeKey();

	if (aKey.IsEmpty())
		return;

	DoBuildActions(aKey, menu);
}

//--------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnEnableActions(CCmdUI* pCmdUI)
{
	BOOL bEnable = DoEnableActions();

	pCmdUI->Enable(bEnable);
}

//------------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnExpandCollapse()
{
	GetNotValidView(TRUE);

	DoExpandCollapse();
}

//------------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::DoExpandCollapse()
{
	if (IsExpandedSelectedNode())
		CollapseAllFromSelectedNode();
	else
		ExpandAllFromSelectedNode();
}

//------------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnShowHideDetails()
{
	GetNotValidView(TRUE);

	CString aKey = GetSelectedNodeKey();

	if (aKey.IsEmpty())   //faccio lo stesso il test aKey visto che DoEnableDetails può essere reimplementato fuori
		return;

	DoShowHideDetails(aKey);

	UpdateDataView();
}

//-------------------------------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnFind()
{
	GetNotValidView(TRUE);

	DoFind();
}

//----------------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::DoShowHideDetails(CString nodeKey)
{
	m_bShowDetailsSettings = !m_bShowDetailsSettings;

	if (m_bShowDetailsSettings && m_LastSelectedNode.CompareNoCase(nodeKey))
		DoSelectionNodeChanged(nodeKey);
	
	ManageSplitter(TRUE);
}

//------------------------------------------------------------------------------------------------------------------
void  CTBBaseNavigationDocument::OnEnableExpand(CCmdUI* pCmdUI)
{
	BOOL bEnable = DoEnableExpand();

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------------------------------------------
BOOL CTBBaseNavigationDocument::DoEnableActions()
{
	return (m_bTreeViewLoaded && !GetSelectedNodeKey().IsEmpty());
}

//--------------------------------------------------------------------------------------------------------------------
BOOL CTBBaseNavigationDocument::DoEnableExpand()
{
	return (m_bTreeViewLoaded && !GetSelectedNodeKey().IsEmpty());
}

//-----------------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnEnableShowDetails(CCmdUI* pCmdUI)
{
	BOOL bEnable = DoEnableDetails();

	pCmdUI->Enable(bEnable);
	pCmdUI->SetCheck(m_bShowDetails && m_bShowDetailsSettings);
}

//-----------------------------------------------------------------------------------------------------
BOOL CTBBaseNavigationDocument::DoEnableDetails()
{
	return (m_bTreeViewLoaded && !GetSelectedNodeKey().IsEmpty());
}

//-----------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::OnEnableFind(CCmdUI* pCmdUI)
{
	BOOL bEnable = DoEnableFind();

	pCmdUI->Enable(bEnable);
}

//----------------------------------------------------------------------------------------------------
BOOL CTBBaseNavigationDocument::DoEnableFind()
{
	return (m_bTreeViewLoaded);
}

//--------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::AddDetail(const DataStr& fieldName, const DataObj& fieldValue, BOOL hasHyperLink)
{
	//comunque dovrei averlo valorizzato, pero' se uno non fa "__super::OnAttachData" e' NULL (ma cmq la __super::OnAttachData dovvrebbe essere fatta)
	if (!m_pDBTNodeDetail)
		return;

	TNodeDetail* pRec = (TNodeDetail*)m_pDBTNodeDetail->AddRecord();

	if (!pRec)
		return;

	pRec->l_FieldName		= fieldName;
	pRec->l_HasHyperlink	= hasHyperLink;
	pRec->l_IsSeparator		= FALSE;
	pRec->l_FieldValue		= fieldValue.FormatData();
}

//-----------------------------------------------------------------------------------------------------------
void CTBBaseNavigationDocument::AddSeparator(const DataStr& fieldName)
{
	//comunque dovrei averlo valorizzato, pero' se uno non fa "__super::OnAttachData" e' NULL (ma cmq la __super::OnAttachData dovvrebbe essere fatta)
	if (!m_pDBTNodeDetail)
		return;

	TNodeDetail* pRec = (TNodeDetail*)m_pDBTNodeDetail->AddRecord();

	if (!pRec)
		return;

	pRec->l_FieldName		= fieldName;
	pRec->l_HasHyperlink	= FALSE;
	pRec->l_IsSeparator		= TRUE;
	pRec->l_FieldValue		.Clear();
}









