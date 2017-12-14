#include "stdafx.h" 

#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\LoginContext.h>
#include <TbGenLibManaged\TreeViewAdvWrapper.h>
#include <TbGes\EXTDOC.H>
#include <TbGes\DBT.H>
#include <TbGes\NumbererService.h>

#include <ExtensionsImages\CommonImages.h>
#include "TBRowSecurityEnums.h"
#include "RSStructures.h"
#include "RSManager.h"
#include "RSTables.h"
#include "UIRSGrants.hjson" //JSON AUTOMATIC UPDATE
#include "CDRSGrants.h"
#include "UIRSGrants.h"

//////////////////////////////////////////////////////////////////////////////
//					CEntityGrantsTreeNode implementation					//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CEntityGrantsTreeNode, CObject)

//-----------------------------------------------------------------------------
CEntityGrantsTreeNode::CEntityGrantsTreeNode()
:
	m_pSubject(NULL),
	m_pSubjectsGrantsRec(NULL)
{
}

//-----------------------------------------------------------------------------
CEntityGrantsTreeNode::CEntityGrantsTreeNode(CSubjectCache*pSubjectCache, TRS_SubjectsGrants* pSubjectGrants, const CString& strParentKey)
:
	m_pSubject(pSubjectCache),
	m_pSubjectsGrantsRec(pSubjectGrants),
	m_strParentKey(strParentKey)
{
	m_strKey = cwsprintf(_T("%d"), m_pSubject->m_SubjectID);
}

static TCHAR szCompanyImage[] = _T("Company");
static TCHAR szResourceImage[] = _T("Resource");
static TCHAR szWorkerImage[] = _T("Worker");
static TCHAR szResourceDenyImage[]	= _T("ResourceDeny");
static TCHAR szResourceReadImage[]	= _T("ResourceRead");
static TCHAR szResourceFullImage[]	= _T("ResourceFull");
static TCHAR szWorkerDenyImage[] = _T("WorkerDeny");
static TCHAR szWorkerReadImage[] = _T("WorkerRead");
static TCHAR szWorkerFullImage[] = _T("WorkerFull");



const TCHAR szRootKey[] =  _T("-1");

//////////////////////////////////////////////////////////////////////////////
//					CEntityGrantsTreeViewAdv implementation					//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CEntityGrantsTreeViewAdv, CTreeViewAdvCtrl)

//---------------------------------------------------------------------------------------------------------------
CEntityGrantsTreeViewAdv::CEntityGrantsTreeViewAdv()
	:
	CTreeViewAdvCtrl(),
	m_pAllNodes(NULL),
	m_bIsProtected(FALSE),
	m_bTreeViewLoaded(FALSE)
{
}

//---------------------------------------------------------------------------------------------------------------
CEntityGrantsTreeViewAdv::~CEntityGrantsTreeViewAdv()
{
	if (m_pAllNodes)
		delete m_pAllNodes;
}


//---------------------------------------------------------------------------------------------------------------
void CEntityGrantsTreeViewAdv::OnInitControl()
{
	CTreeViewAdvCtrl::OnInitControl();

	SetAutoSizeCtrl(3);
	SetNodeStateIcon(TRUE);

	AddImage(szCompanyImage, AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconRoot), _T("")));
	AddImage(szResourceImage, AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconResource), _T(""))); //resource
	AddImage(szWorkerImage, AfxGetPathFinder()->GetFileNameFromNamespace(TBGlyph(szIconWorker), _T("")));

	AddImage(szResourceDenyImage, AfxGetPathFinder()->GetFileNameFromNamespace(ExtensionsGlyph(szGlyphResourceDeny), _T("")));
	AddImage(szResourceReadImage, AfxGetPathFinder()->GetFileNameFromNamespace(ExtensionsGlyph(szGlyphResourceRead), _T("")));
	AddImage(szResourceFullImage, AfxGetPathFinder()->GetFileNameFromNamespace(ExtensionsGlyph(szGlyphResourceFull), _T("")));

	
	AddImage(szWorkerDenyImage, AfxGetPathFinder()->GetFileNameFromNamespace(ExtensionsGlyph(szGlyphWorkerDeny), _T("")));
	AddImage(szWorkerReadImage, AfxGetPathFinder()->GetFileNameFromNamespace(ExtensionsGlyph(szGlyphWorkerRead), _T("")));
	AddImage(szWorkerFullImage, AfxGetPathFinder()->GetFileNameFromNamespace(ExtensionsGlyph(szGlyphWorkerFull), _T("")));
	AddControls();	
	SetViewContextMenu(FALSE);
}

//---------------------------------------------------------------------------------------------------------------
void CEntityGrantsTreeViewAdv::SetAllNodes(Array* pAllNodes, BOOL isProtected)
{
	if (m_pAllNodes)
		delete m_pAllNodes;
	m_pAllNodes = pAllNodes;
	m_bIsProtected = isProtected;
	Load();
}

//---------------------------------------------------------------------------------------------------------------
CEntityGrantsTreeNode* CEntityGrantsTreeViewAdv::GetNodeByKey(const CString& strKey)
{
	for (int i = 0; i <= m_pAllNodes->GetUpperBound(); i++)
	{
		CEntityGrantsTreeNode* pNode = (CEntityGrantsTreeNode*)m_pAllNodes->GetAt(i);
		if (pNode->m_strKey.CompareNoCase(strKey) == 0)
			return pNode;
	}

	return NULL;
}

//---------------------------------------------------------------------------------------------------------------
void CEntityGrantsTreeViewAdv::InsertNodeInTree(CEntityGrantsTreeNode* pNode)
{
	if (!pNode)
		return;

	CEntityGrantsTreeNode* pParentNode = NULL;
	if (!GetNode(pNode->m_strParentKey))
	{
		pParentNode = GetNodeByKey(pNode->m_strParentKey);
		InsertNodeInTree(pParentNode);	
	}

	InsertChild(pNode->m_strParentKey, pNode->m_pSubject->GetSubjectTitle(), pNode->m_strKey, GetSubjectTreeImage(pNode), RGB(41, 57, 85));	
}

//---------------------------------------------------------------------------------------------------------------
void CEntityGrantsTreeViewAdv::Load()
{
	m_bTreeViewLoaded = FALSE;
	
	CString strSelectedKey;
	CString strLoggedWorkerNodeKey;
	
	CRSGrantsClientDoc* pGrantsClientDoc = (CRSGrantsClientDoc*)((CAbstractFormDoc*)GetDocument())->GetClientDoc(RUNTIME_CLASS(CRSGrantsClientDoc));
	
	if (!pGrantsClientDoc || !m_pAllNodes)
		return;

	ClearTree();

	BOOL bShowNode = TRUE;

	AddNode(_TB("Company"), szRootKey, szCompanyImage, RGB(41, 57, 85));
		
	for (int i = 0; i <= m_pAllNodes->GetUpperBound(); i++)
	{
		CEntityGrantsTreeNode* pNode = (CEntityGrantsTreeNode*)m_pAllNodes->GetAt(i);
		//inserisco il nodo solo se verifica le condizioni di filtraggio
		switch (pNode->m_pSubjectsGrantsRec->f_GrantType.GetValue())
		{
		case E_GRANT_TYPE_DENY:
			bShowNode = pGrantsClientDoc->IsShowDeny(); break;
		case E_GRANT_TYPE_READ_ONLY:
			bShowNode = pGrantsClientDoc->IsShowRead(); break;
		case E_GRANT_TYPE_READWRITE:
			bShowNode = pGrantsClientDoc->IsShowFull(); break;
		}
		
		if (bShowNode)
		{
			InsertNodeInTree(pNode);	
			CSubjectCache* pLoggedSubject = AfxGetLoggedSubject();
			if (pLoggedSubject && pNode->m_pSubject->m_SubjectID == pLoggedSubject->m_SubjectID)
				strLoggedWorkerNodeKey = pNode->m_strKey;			
		}
	}
	
	strSelectedKey = (m_strSelectedNodeKey.IsEmpty()) ? strLoggedWorkerNodeKey : m_strSelectedNodeKey;
	if (strSelectedKey.IsEmpty() || !GetNode(strSelectedKey))
		strSelectedKey = szRootKey;
	
	m_bTreeViewLoaded = TRUE;
	ExpandAll();	
	SetFocus();
	SelectNode(strSelectedKey);
	EnsureVisible(strSelectedKey);
}


//---------------------------------------------------------------------------------------------------------------
CString CEntityGrantsTreeViewAdv::GetSubjectTreeImage(CEntityGrantsTreeNode* pNode)
{
	if (!pNode || !pNode->m_pSubjectsGrantsRec)
		return szWorkerDenyImage;

	BOOL bIsWorker = pNode->m_pSubjectsGrantsRec->f_WorkerID > -1;

	switch(pNode->m_pSubjectsGrantsRec->f_GrantType.GetValue())
	{
		case E_GRANT_TYPE_DENY: 
		default: 
			return bIsWorker ? (m_bIsProtected) ? szWorkerDenyImage : szWorkerImage : (m_bIsProtected) ? szResourceDenyImage : szResourceImage;;
		case E_GRANT_TYPE_READ_ONLY: 
			return bIsWorker ? szWorkerReadImage : szResourceReadImage;;
		case E_GRANT_TYPE_READWRITE: 
			return bIsWorker ? szWorkerFullImage : szResourceFullImage; 
	}
}

//---------------------------------------------------------------------------------------------------------------
CEntityGrantsTreeNode* CEntityGrantsTreeViewAdv::GetSelectedTreeNode()
{
	CString aKey = GetSelectedNodeKey();
	if (aKey.IsEmpty())
		return NULL;
		
	for (int i = 0; i <= m_pAllNodes->GetUpperBound(); i++)
	{
		CEntityGrantsTreeNode* pNode = (CEntityGrantsTreeNode*)m_pAllNodes->GetAt(i);

		if (pNode && (pNode->m_strKey.CompareNoCase(aKey) == 0))
			return pNode;
	}

	return NULL;
}

//---------------------------------------------------------------------------------------------------------------
void CEntityGrantsTreeViewAdv::ChangeNodeImage(CEntityGrantsTreeNode* pNode)
{
	if (!pNode)
		return;

	SetImage(pNode->m_strKey, GetSubjectTreeImage(pNode));
}

//---------------------------------------------------------------------------------------------------------------
void CEntityGrantsTreeViewAdv::OnToolBarCommand(int cmdId)
{
	/*if (
			cmdId == ID_RS_GRANTS_NOGRANT_FILTER ||
			cmdId == ID_RS_GRANTS_READ_FILTER||
			cmdId == ID_RS_GRANTS_FULL_FILTER
		)
		Load();*/
}


//---------------------------------------------------------------------------------------------------------------
void CEntityGrantsTreeViewAdv::OnSelectionChanged()
{
	if (!m_bTreeViewLoaded)
		return;

	m_strSelectedNodeKey = this->GetSelectedNodeKey();
}




/////////////////////////////////////////////////////////////////////////////
//	             class CTileGrantsTree implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CTileGrantsTree, CTileDialog)


//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTileGrantsTree, CTileDialog)
//{{AFX_MSG_MAP(CTileGrantsTree)
	ON_COMMAND			(ID_RS_LOAD_GRANTS_TREE,		OnLoadGrantsTree)
	ON_COMMAND			(ID_RS_REFRESH_CURRENT_NODE,	OnRefreshCurrentNode)
	ON_CONTROL			(UM_TREEVIEWADV_SELECTION_CHANGED, IDC_RS_GRANTS_TREE, OnSelectionNodeChanged)
//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CTileGrantsTree::CTileGrantsTree()
	:
	CTileDialog(_T("GrantsTree"), IDD_RS_GRANTS_TILE_TREE),		
	m_pTreeView(NULL),
	m_bTreeViewLoaded(FALSE)
{
}

//-----------------------------------------------------------------------------
void CTileGrantsTree::BuildDataControlLinks()
{
	TRS_SubjectsGrants* pCurrSubjectsGrantsRec = GetRSGrantsClientDoc()->m_pCurrSubjectsGrantsRec;
	DataBool* IsProtectedField = (DataBool*)GetDocument()->m_pDBTMaster->GetRecord()->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sIsProtected);

	m_pTreeView = (CEntityGrantsTreeViewAdv*)AddLink
	(
		IDC_RS_GRANTS_TREE,
		_NS_LNK("RSOfficeLayoutTree"),
		ClientDocSDC(GetRSGrantsClientDoc(), m_bTreeView),
		RUNTIME_CLASS(CEntityGrantsTreeViewAdv)
	);	

	AddLink
	(
		IDC_RS_GRANTS_PROTECTED_CHECK,
		_NS_LNK("ProtectedCheck"),
		NULL,
		IsProtectedField,
		RUNTIME_CLASS(CBoolButton)
	);

}

//---------------------------------------------------------------------------------------------------------------
BOOL CTileGrantsTree::OnPrepareAuxData()
{
	DataBool* IsProtectedField = (DataBool*)GetDocument()->m_pDBTMaster->GetRecord()->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sIsProtected);

	CBoolButton* pButton = (CBoolButton*)GetLinkedParsedCtrl(IDC_RS_GRANTS_PROTECTED_CHECK);
	pButton->SetWindowTextW((*IsProtectedField) ? _TB("This record is under protection") : _TB("Put this record under protection"));
	OnLoadGrantsTree();

	//GetFrame()->SendMessageToDescendants(WM_COMMAND, ID_RS_LOAD_GRANTS_TREE);
	return TRUE;
}


//---------------------------------------------------------------------------------------------------------------
CRSGrantsClientDoc* CTileGrantsTree::GetRSGrantsClientDoc() const
{
	return (CRSGrantsClientDoc*) GetDocument()->GetClientDoc(RUNTIME_CLASS(CRSGrantsClientDoc));
} 


//---------------------------------------------------------------------------------------------------------------
void CTileGrantsTree::OnLoadGrantsTree()
{
	DataBool* IsProtectedField = (DataBool*)GetDocument()->m_pDBTMaster->GetRecord()->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sIsProtected);

	m_bTreeViewLoaded = FALSE;

	Array* pAllNode = new Array();

	CSubjectCacheArray* pSubjectCacheArray = AfxGetRowSecurityManager()->GetSubjectCacheArray();
	if (pSubjectCacheArray && pSubjectCacheArray->GetSize() > 0)
	{
		//considero solo i nodi di primo livello ovvero quelli senza master
		for (int i = 0; i <= pSubjectCacheArray->GetUpperBound(); i++)
		{
			CSubjectCache* pSubjectCache = pSubjectCacheArray->GetAt(i);
			if (pSubjectCache && (!pSubjectCache->m_pMasterSubjects || pSubjectCache->m_pMasterSubjects->GetSize() == 0))
				AddTreeNode(pAllNode, pSubjectCache, szRootKey);
		}
	}

	m_pTreeView->SetAllNodes(pAllNode, *IsProtectedField);
	m_bTreeViewLoaded = TRUE;
	
	if (*IsProtectedField)
	{
		m_pTreeView->SetFocus();
		OnSelectionNodeChanged();
	}
	m_pTreeView->Enable(*IsProtectedField == TRUE);
}


//---------------------------------------------------------------------------------------------------------------
void CTileGrantsTree::OnRefreshCurrentNode()
{
	CEntityGrantsTreeNode* pSelectedTreeNode = (!m_bTreeViewLoaded) ? NULL : m_pTreeView->GetSelectedTreeNode();
	m_pTreeView->ChangeNodeImage(pSelectedTreeNode);
}

//---------------------------------------------------------------------------------------------------------------
void CTileGrantsTree::OnSelectionNodeChanged()
{
	CEntityGrantsTreeNode* pSelectedTreeNode = (!m_bTreeViewLoaded) ? NULL : m_pTreeView->GetSelectedTreeNode();

	CString aKey = (pSelectedTreeNode) ? pSelectedTreeNode->m_strKey : _T("");	

	GetRSGrantsClientDoc()->SetCurrSubjectsGrantsRec((!pSelectedTreeNode || aKey.IsEmpty() || aKey == szRootKey) ? NULL : pSelectedTreeNode->m_pSubjectsGrantsRec);	
	GetRSGrantsClientDoc()->DoCurrentSubjectGrantsChanged();
}

//---------------------------------------------------------------------------------------------------------------
void CTileGrantsTree::AddTreeNode(Array* pAllNode, CSubjectCache* pSubjectCache, const CString& strParentKey)
{
	CRSGrantsClientDoc* pRSClientDoc = GetRSGrantsClientDoc();

	if (!pSubjectCache) return;
	TRS_SubjectsGrants* pGrantForSubject = (pSubjectCache) ? pRSClientDoc->GetGrantRecordForSubject(pSubjectCache->m_SubjectID) : NULL;
	if (!pGrantForSubject) return;

	CEntityGrantsTreeNode* pTreeNode = new CEntityGrantsTreeNode(pSubjectCache, pGrantForSubject, strParentKey);
	pAllNode->Add(pTreeNode);

	if (pSubjectCache->m_pSlaveSubjects)
	{
		for (int i = 0; i <= pSubjectCache->m_pSlaveSubjects->GetUpperBound(); i++)
		{
			CSubjectHierarchy* pSlaveSubject = (CSubjectHierarchy*)pSubjectCache->m_pSlaveSubjects->GetAt(i);
			if (pSlaveSubject->m_nrLevel == 1)
				AddTreeNode(pAllNode, pSlaveSubject->m_pSubject, pTreeNode->m_strKey);
		}
	}
}




/////////////////////////////////////////////////////////////////////////////
//	             class CTileSingleGrant implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CTileSingleGrant, CTileDialog)


//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTileSingleGrant, CTileDialog)
	//{{AFX_MSG_MAP(CTileGrantsTree)
	ON_BN_CLICKED(IDC_RS_GRANTS_APPLY_BTN, OnGrantTypeChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CTileSingleGrant::CTileSingleGrant()
	:
	CTileDialog(_T("SingleGrant"), IDD_RS_GRANTS_TILE_GRANT)
{
}

//-----------------------------------------------------------------------------
void CTileSingleGrant::BuildDataControlLinks()
{
	TRS_SubjectsGrants* pCurrSubjectsGrantsRec = GetRSGrantsClientDoc()->m_pCurrSubjectsGrantsRec;

	AddLink
	(
		IDC_RS_GRANTS_COMBO,
		_NS_LNK("SubjectGrantCombo"),
		ClientDocSDC(GetRSGrantsClientDoc(), m_CurrSubjectGrantType),
		RUNTIME_CLASS(CEnumCombo)
	);

	AddLink
	(
		IDC_RS_GRANTS_DESCRI,
		_NS_LNK("SubjectGrantDescription"),
		ClientDocSDC(GetRSGrantsClientDoc(), m_strGrantDescription),
		RUNTIME_CLASS(CStrStatic)
	);

	AddLink
	(
		IDC_RS_GRANTS_INHERITED,
		_NS_LNK("SubjectGrantIneherited"),
		ClientDocSDC(GetRSGrantsClientDoc(), m_strGrantInherited),
		RUNTIME_CLASS(CStrStatic)
	);

	CPictureStatic* pPictureStatic = (CPictureStatic*)AddLink
	(
		IDC_RS_GRANTS_PICTURE,
		_NS_LNK("SubjectGrantPicture"),
		ClientDocSDC(GetRSGrantsClientDoc(), m_strGrantPicture),
		RUNTIME_CLASS(CPictureStatic)
	);
	pPictureStatic->OnCtrlStyleBest();
}


//---------------------------------------------------------------------------------------------------------------
CRSGrantsClientDoc* CTileSingleGrant::GetRSGrantsClientDoc() const
{
	return (CRSGrantsClientDoc*)GetDocument()->GetClientDoc(RUNTIME_CLASS(CRSGrantsClientDoc));
}


//---------------------------------------------------------------------------------------------------------------v
void CTileSingleGrant::OnGrantTypeChanged()
{
	CRSGrantsClientDoc* pRSClientDoc = GetRSGrantsClientDoc();

	GetRSGrantsClientDoc()->DoGrantTypeChanged();
}


/////////////////////////////////////////////////////////////////////////////
//	             class CGrantsTileGroup implementation
/////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNCREATE(CGrantsTileGroup, CTileGroup)

//-----------------------------------------------------------------------------
void CGrantsTileGroup::Customize()
{ 
	SetLayoutType(CLayoutContainer::HBOX);

	AddTile(RUNTIME_CLASS(CTileGrantsTree),	IDD_RS_GRANTS_TILE_TREE, _TB("Grants Tree"), TILE_STANDARD);
	AddTile(RUNTIME_CLASS(CTileSingleGrant), IDD_RS_GRANTS_TILE_GRANT, _TB("Single Grant"), TILE_STANDARD);
	
}


//////////////////////////////////////////////////////////////////////////////
//							CRSEntityGrantsView implementation				//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRSEntityGrantsView, CSlaveFormView)


//-----------------------------------------------------------------------------
CRSEntityGrantsView::CRSEntityGrantsView()
	:
	CSlaveFormView(_NS_VIEW("RSEntityGrants"), IDD_RS_GRANTS_DLG)
{	
}


//---------------------------------------------------------------------------------------------------------------
void CRSEntityGrantsView::BuildDataControlLinks()
{
	DataBool* IsProtectedField = (DataBool*)GetDocument()->m_pDBTMaster->GetRecord()->GetDataObjFromColumnName(RowSecurityAddOnFields::s_sIsProtected);	
	
	AddTileGroup(IDC_RS_GRANTS_TILEGROUP, RUNTIME_CLASS(CGrantsTileGroup),	_TB("EntityGrants"));
}



//=============================================================================
// CRSGrantsFrame
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRSEntityGrantsFrame, CSlaveFrame)

//-----------------------------------------------------------------------------	
CRSEntityGrantsFrame::CRSEntityGrantsFrame()
	:
	CSlaveFrame()
{}


//-----------------------------------------------------------------------------
BOOL CRSEntityGrantsFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	if (!__super::OnCustomizeTabbedToolBar(pTabbedBar))
		return FALSE;

	CTBToolBar* pToolbar = pTabbedBar->FindToolBar(szToolbarNameMain);
	if (!pToolbar) return FALSE;
	{
		pToolbar->AddButton(ID_RS_GRANTS_NOGRANT_SHOW, _T("DenyGrant"), ExtensionsIcon(szIconDenyGrant, TOOLBAR), _TB("Grants deny"), 0);
		pToolbar->AddButton(ID_RS_GRANTS_READ_SHOW, _T("ReadGrant"), ExtensionsIcon(szIconReadGrant, TOOLBAR), _TB("Only read"), 1);
		pToolbar->AddButton(ID_RS_GRANTS_FULL_SHOW, _T("FullGrant"), ExtensionsIcon(szIconFullGrant, TOOLBAR), _TB("Read and write"), 2);
		pToolbar->AddSeparatorAfter(ID_RS_GRANTS_FULL_SHOW);		

		pToolbar->AdjustLayout();
	}

	return TRUE;
}

