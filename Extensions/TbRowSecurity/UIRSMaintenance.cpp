#include "stdafx.h"

//NOW INCLUDED IN COMMON PCH: #include <tbgenlib\parsobj.h>

#include <TbGes\BODYEDIT.H>

#include "BDRSMaintenance.h"
#include "UIRSMaintenance.h"
#include "UIRSMaintenance.hjson"

static TCHAR szP1[] = _T("P1");
static TCHAR szP2[] = _T("P2");
static TCHAR szP3[] = _T("P3");


//=============================================================================
// CRSMaintenanceFrame
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRSMaintenanceFrame, CBatchFrame)

BEGIN_MESSAGE_MAP(CRSMaintenanceFrame, CBatchFrame)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------	
CRSMaintenanceFrame::CRSMaintenanceFrame()
	:
	CBatchFrame()
{}

//-----------------------------------------------------------------------------
BDRSMaintenance*	CRSMaintenanceFrame::GetDocument()
{
	return (BDRSMaintenance*)CBatchFrame::GetDocument();
}

//-----------------------------------------------------------------------------
BOOL CRSMaintenanceFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	__super::OnCustomizeTabbedToolBar(pTabbedBar);

	CTBToolBar* pMainToolBar = pTabbedBar->FindToolBar(_T("Main"));
	if (!pMainToolBar) return FALSE;

	pMainToolBar->RemoveButtonForID(ID_EXTDOC_REPORT);
	
	return TRUE;
}

//==============================================================================
//	CBDRSMaintenanceView
//==============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CRSMaintenanceView, CMasterFormView)

//-----------------------------------------------------------------------------
CRSMaintenanceView::CRSMaintenanceView()
	:
	CMasterFormView(_NS_VIEW("RSMaintenanceView"), IDD_RS_MAINTENANCE)
{
}

//-----------------------------------------------------------------------------
BDRSMaintenance* CRSMaintenanceView::GetDocument() const 
{ 
	return (BDRSMaintenance*) m_pDocument;
}

//-----------------------------------------------------------------------------
void CRSMaintenanceView::BuildDataControlLinks()
{    
	AddTileGroup(IDC_RS_MAINTENANCE_TILEGROUP, RUNTIME_CLASS(CRSMaintenanceTileGroup), _NS_TABMNG("CRSMaintenanceTileGroup"));
}


//=============================================================================
// CTileRSMaintenanceBase
//=============================================================================
//
IMPLEMENT_DYNCREATE(CTileRSMaintenance, CTileDialog)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTileRSMaintenance, CTileDialog)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTileRSMaintenance::CTileRSMaintenance()
:
	CTileDialog(_T("RSMaintenance"), IDD_TILE_RS_MAINTENANCE)
{
}

//-----------------------------------------------------------------------------
CTileRSMaintenance::~CTileRSMaintenance()
{
	BDRSMaintenance* pDoc = GetDocument();
	pDoc->m_pPictureStatic_Step1 = NULL;
	pDoc->m_pPictureStatic_Step2 = NULL;
	pDoc->m_pPictureStatic_Step3 = NULL;
	pDoc->m_pPictureStatic_Step4 = NULL;
}

//-----------------------------------------------------------------------------
void CTileRSMaintenance::BuildDataControlLinks()
{
	BDRSMaintenance* pDoc = GetDocument();
	AddLink(IDC_RS_STEP_START_IMG, _NS_LNK("StepStartImg"), SDC(StepStartImg), RUNTIME_CLASS(CPictureStatic));
	pDoc->m_pPictureStatic_Step1 = (CPictureStatic*)AddLink(IDC_RS_STEP1_IMG, _NS_LNK("Step1Img"), SDC(Step1Img), RUNTIME_CLASS(CPictureStatic));
	pDoc->m_pPictureStatic_Step2 = (CPictureStatic*)AddLink(IDC_RS_STEP2_IMG, _NS_LNK("Step2Img"), SDC(Step2Img), RUNTIME_CLASS(CPictureStatic));
	pDoc->m_pPictureStatic_Step3 = (CPictureStatic*)AddLink(IDC_RS_STEP3_IMG, _NS_LNK("Step3Img"), SDC(Step3Img), RUNTIME_CLASS(CPictureStatic));
	pDoc->m_pPictureStatic_Step4 = (CPictureStatic*)AddLink(IDC_RS_STEP4_IMG, _NS_LNK("Step4Img"), SDC(Step4Img), RUNTIME_CLASS(CPictureStatic));
	AddLink(IDC_RS_STEP_END_IMG, _NS_LNK("StepEndImg"), SDC(StepEndImg), RUNTIME_CLASS(CPictureStatic));
}



//=============================================================================
//             class CRSEntitiesBodyEdit definition 
//=============================================================================
class CRSEntitiesBodyEdit : public CBodyEdit
{
	DECLARE_DYNCREATE(CRSEntitiesBodyEdit)

public:
	CRSEntitiesBodyEdit();
public:
	virtual void Customize();
};


//=============================================================================
//             class CRSEntitiesBodyEdit implementation
//=============================================================================
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE(CRSEntitiesBodyEdit, CBodyEdit)

//-----------------------------------------------------------------------------	
CRSEntitiesBodyEdit::CRSEntitiesBodyEdit()
	: CBodyEdit(_NS_BE("RSEntitiesBodyEdit"))
{
	BERemoveExStyle(BE_STYLE_SHOW_FOOTER_TOOLBAR);
	BERemoveExStyle(BE_STYLE_SHOW_HORIZ_SCROLLBAR);
	BERemoveExStyle(BE_STYLE_SHOW_VERT_SCROLLBAR);

	EnableDrag(FALSE);
	EnableInsertRow(FALSE);
	EnableAddRow(FALSE);
	EnableDeleteRow(FALSE);
	EnableMultipleSel(TRUE);
}

//-----------------------------------------------------------------------------	
void CRSEntitiesBodyEdit::Customize()
{	
	SetUITitlesRows(2);
	ColumnInfo* pCol = NULL;

	VRSEntities* pRec = (VRSEntities*)m_pDBT->GetRecord();

	pCol = AddColumn
	(
		_NS_CLN("Name"),
		L"Entità",
		0,
		IDC_ENTITY_BE_NAME,
		&(pRec->l_EntityName),
		RUNTIME_CLASS(CStrStatic)
	);
	pCol->SetScreenWidth(200, FALSE);

	pCol = AddColumn
	(
		_NS_CLN("Protected"),
		L"Sel",
		0,
		IDC_ENTITY_BE_PROTECTED,
		&(pRec->l_IsProtected),
		RUNTIME_CLASS(CBoolButton)
	);
	pCol->SetScreenWidth(100, FALSE);
}



//=============================================================================
// CTileRSMaintenanceBase
//=============================================================================
//
IMPLEMENT_DYNCREATE(CTileRSEntities, CTileDialog)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTileRSEntities, CTileDialog)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTileRSEntities::CTileRSEntities()
	:
	CTileDialog(_T("CTileRSEntities"), IDD_TILE_RS_ENTITIES)
{
}



//-----------------------------------------------------------------------------
void CTileRSEntities::BuildDataControlLinks()
{
	BDRSMaintenance* pDoc = GetDocument();
	AddLink
	(
		IDC_ENTITIES_BE,
		GetDocument()->m_pDBTRSEntities,
		RUNTIME_CLASS(CRSEntitiesBodyEdit),
		NULL,
		_TB("CEntitiesBE")
	);
}



/////////////////////////////////////////////////////////////////////////////
//	             class CRSMaintenanceTileGroup implementation
/////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNCREATE(CRSMaintenanceTileGroup, CTileGroup)

//-----------------------------------------------------------------------------
void CRSMaintenanceTileGroup::Customize()
{
	SetLayoutType(CLayoutContainer::HBOX);
	BDRSMaintenance* pDoc = (BDRSMaintenance*)GetDocument();

	AddTile(RUNTIME_CLASS(CTileRSEntities), IDD_TILE_RS_ENTITIES, _TB("Entitites"), TILE_STANDARD);
	pDoc->pTile = (CTileRSMaintenance*)AddTile(RUNTIME_CLASS(CTileRSMaintenance), IDD_TILE_RS_MAINTENANCE, _TB("Maintenance"), TILE_STANDARD);

}
