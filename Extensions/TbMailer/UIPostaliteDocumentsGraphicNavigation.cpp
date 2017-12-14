#include "StdAfx.h"

#include <TbGenlib\TBSplitterWnd.h>
#include "BDPostaliteDocumentsGraphicNavigation.h"
#include "BDPostaliteDocumentsGraphicNavigation.hjson" //JSON AUTOMATIC UPDATE
#include "UIPostaliteDocumentsGraphicNavigation.h"

////////////////////////////////////////////////////////////////////////////
// 				class UIPostaliteDocumentsGraphicNavigation Implementation
/////////////////////////////////////////////////////////////////////////////
//
//---------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(UIPostaliteDocumentsGraphicNavigation, CMasterFormView)

//-----------------------------------------------------------------------------
BDPostaliteDocumentsGraphicNavigation*	UIPostaliteDocumentsGraphicNavigation::GetDocument	() const 
{
	return (BDPostaliteDocumentsGraphicNavigation*) __super::GetDocument();
}

//-----------------------------------------------------------------------------
UIPostaliteDocumentsGraphicNavigation::UIPostaliteDocumentsGraphicNavigation()
	:
	CMasterFormView(_NS_VIEW("PostaliteDocumentsGraphicNavigation"), IDD_MESSAGES_MANAGEMENT)
{
}

// usata solo per la view figlia CGLJournalBlockView
//-----------------------------------------------------------------------------
UIPostaliteDocumentsGraphicNavigation::UIPostaliteDocumentsGraphicNavigation(UINT nIDD)
	:
	CMasterFormView(_NS_VIEW("PostaliteDocumentsGraphicNavigation"), nIDD)                 
{
}

//-----------------------------------------------------------------------------
void UIPostaliteDocumentsGraphicNavigation::BuildDataControlLinks()
{    
	AddLink
	(
		IDC_MESSAGES_MANAGEMENT_SHOWONLYTOSEND,
		_NS_LNK("OnlyToSend"),
		SDC(m_bOnlyToSend),
		RUNTIME_CLASS(CBoolButton)
	);
	
	GetDocument()->m_pFilterByDateCtrl = (CBoolButton*)AddLink
	(
		IDC_MESSAGES_MANAGEMENT_FILTERBYDATE_CHECK,
		_NS_LNK("FilterByDate"),
		SDC(m_bFilterByDate),
		RUNTIME_CLASS(CBoolButton)
	);

	GetDocument()->m_pFilterDateFromCtrl = (CDateEdit*)AddLink
	(
		IDC_MESSAGES_MANAGEMENT_FILTERBYDATE_FIELD_FROM,
		_NS_LNK("FilterDateFrom"),
		SDC(m_FilterDateFrom),
		RUNTIME_CLASS(CDateEdit)
	);

	GetDocument()->m_pFilterDateToCtrl = (CDateEdit*)AddLink
	(
		IDC_MESSAGES_MANAGEMENT_FILTERBYDATE_FIELD_TO,
		_NS_LNK("FilterDateTo"),
		SDC(m_FilterDateTo),
		RUNTIME_CLASS(CDateEdit)
	);

	GetDocument()->m_pFilterByWorkerCtrl = (CBoolButton*)AddLink
	(
		IDC_MESSAGES_MANAGEMENT_FILTERBYWORKER_CHECK,
		_NS_LNK("FilterByWorker"),
		SDC(m_bFilterByWorker),
		RUNTIME_CLASS(CBoolButton)
	); 


	GetDocument()->m_pFilterWorkerCtrl = (CPostaLiteWorkersCombo*)AddLink
	(
		IDC_MESSAGES_MANAGEMENT_FILTERBYWORKER,
		_NS_LNK("FilterWorker"),
		SDC(m_FilterByWorker),
		RUNTIME_CLASS(CPostaLiteWorkersCombo)
	);
	

	AddLink
	(
		IDC_MESSAGES_MANAGEMENT_CURRENT_CREDIT,
		_NS_LNK("Currentcredit"),
		SDC(m_CurrentCredit),
		RUNTIME_CLASS(CMoneyEdit)
	);

	AddLink
	(
		IDC_MESSAGES_MANAGEMENT_UNSENT_MESSAGE_COST,
		_NS_LNK("UnsentMessageEstimate"),
		SDC(m_UnsentMessageEstimate),
		RUNTIME_CLASS(CMoneyEdit)
	);
	
	GetDocument()->m_FilterByWorker = GetDocument()->GetCurrentWorkerName();

	GetDocument()->m_pTreeView = (CTreeViewAdvCtrl*) AddLink
	(
		IDC_MESSAGES_MANAGEMENT_TREECONTROL,
		_T("PostaliteDocumentsGraphicNavigationTreeView"),
		SDC(m_bTreeView),
		RUNTIME_CLASS(CTreeViewAdvCtrl)
	);

	if (GetDocument()->m_pTreeView)
		GetDocument()->m_pTreeView->SetBackColorTreeView(RGB(188, 199, 216));

	GetDocument()->LoadTree();
}

//////////////////////////////////////////////////////////////////////////////
//							CPostaliteDocumentsGraphicNavigationFrame
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CPostaliteDocumentsGraphicNavigationFrame, CBatchFrame)


//-----------------------------------------------------------------------------
BOOL CPostaliteDocumentsGraphicNavigationFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{		
	ASSERT(pTabbedBar);
	if (pTabbedBar == NULL) return FALSE;
	CTBToolBar* pToolBar = pTabbedBar->FindToolBarOrAdd(this, szToolbarNameAux);
	if (pToolBar == NULL) return FALSE;

	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_REFRESH_TREE, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationExpandTree"), IDB_MESSAGES_MANAGEMENT_REFRESH_TREE_NORMAL_LARGE, _TB("Refresh data"), FALSE);
	pToolBar->AddSeparator();
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_ALLOT_MESSAGES, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationAllotMessages"), IDB_MESSAGES_MANAGEMENT_ALLOT_MESSAGES_NORMAL_LARGE, _TB("Put unassigned documents into envelopes"), FALSE);
	pToolBar->AddSeparator(); 
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_REOPEN_ENVELOPE, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationReopenEnvelope"), IDB_MESSAGES_MANAGEMENT_CLOSE_ENVELOPE_NORMAL_LARGE, _TB("Re-open closed envelope"), FALSE);
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_CLOSE_ENVELOPE, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationCloseEnvelop"), IDB_MESSAGES_MANAGEMENT_COLLAPSE_NODES_NORMAL_LARGE, _TB("Close current envelope"), FALSE);
	pToolBar->AddSeparator();
	
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_EXPAND_NODES, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationExpandNodes"), IDB_MESSAGES_MANAGEMENT_EXPAND_NODES_NORMAL_LARGE, _TB("Expand subnodes"), FALSE );
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_COLLAPSE_NODES, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationCollapseNodes"), IDB_MESSAGES_MANAGEMENT_COLLAPSE_NODES_NORMAL_LARGE, _TB("Collapse subnodes"), FALSE);
	pToolBar->AddSeparator();
	
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_DELETE_MESSAGE, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationDeleteMessage"), IDB_MESSAGES_MANAGEMENT_DELETE_MESSAGE_NORMAL_LARGE, _TB("Delete Message"), FALSE);
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_REMOVE_FROM_ENVELOPE, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationRemoveFromEnvelope"), IDB_MESSAGES_MANAGEMENT_REMOVE_FROM_ENVELOPE_NORMAL_LARGE, _TB("Remove current document from envelope"), FALSE);
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_SEND_ENVELOPE_NOW, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationSendEnvelopeNow"), IDB_MESSAGES_MANAGEMENT_SEND_ENVELOPE_NOW_NORMAL_LARGE, _TB("Upload envelope to PostaLite now"), FALSE);
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_SEND_MESSAGE_NOW, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationSendMessageNow"), IDB_MESSAGES_MANAGEMENT_SEND_MESSAGE_NOW_NORMAL_LARGE, _TB("Upload document to PostaLite now"), FALSE);
	pToolBar->AddSeparator();
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_ENVELOPE_ESTIMATE, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationEnvelopeEstimate"), IDB_MESSAGES_MANAGEMENT_ENVELOPE_ESTIMATE_NORMAL_LARGE, _TB("Get envelope estimated costs"), FALSE);
	pToolBar->AddButton(ID_MESSAGES_MANAGEMENT_UPDATE_ENVELOPE_STATUS, _NS_TOOLBARBTN("PostaliteDocumentsGraphicNavigationUpdateEnvelopeStatus"), IDB_MESSAGES_MANAGEMENT_UPDATE_ENVELOPE_STATUS_NORMAL_LARGE, _TB("Update envelopes status"), FALSE);
	pToolBar->AddSeparator();
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CPostaliteDocumentsGraphicNavigationFrame::CreateStatusBar()
{   
	return TRUE;
}

//------------------------------------------------------------------
BOOL CPostaliteDocumentsGraphicNavigationFrame::CreateAuxObjects(CCreateContext* pCreateContext)
{
	CTaskBuilderSplitterWnd* pSplitter = CreateSplitter(RUNTIME_CLASS(CTaskBuilderSplitterWnd), 2, 1);
	if (!pSplitter)
		return FALSE;

	pSplitter->AddWindow(RUNTIME_CLASS(UIPostaliteDocumentsGraphicNavigation), pCreateContext);
	pSplitter->AddWindow(RUNTIME_CLASS(CPostaliteDocumentsGraphicNavigationBodyEditView), pCreateContext, 1);
	pSplitter->SetSplitRatio((float) 0.60);
	pSplitter->RecalcLayout();
	pSplitter->SplitHorizontally();
	return TRUE;
}

//=============================================================================
//						CPostaliteDocumentsGraphicNavigationBodyEditView
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CPostaliteDocumentsGraphicNavigationBodyEditView, CMasterFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPostaliteDocumentsGraphicNavigationBodyEditView, CMasterFormView)
	//{{AFX_MSG_MAP(CPostaliteDocumentsGraphicNavigationBodyEditView)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BDPostaliteDocumentsGraphicNavigation* CPostaliteDocumentsGraphicNavigationBodyEditView::GetDocument() const { return (BDPostaliteDocumentsGraphicNavigation*) __super::GetDocument(); }

//-----------------------------------------------------------------------------
CPostaliteDocumentsGraphicNavigationBodyEditView::CPostaliteDocumentsGraphicNavigationBodyEditView()
	:
	CMasterFormView(_NS_VIEW("PostaliteDocumentsGraphicNavigationDetail"), IDD_MESSAGES_DETAILS)
{
	SetCenterControls (FALSE);
}

//-----------------------------------------------------------------------------
void CPostaliteDocumentsGraphicNavigationBodyEditView::BuildDataControlLinks()
{
	GetDocument()->m_pBody = (CPostaliteDocumentsGraphicNavigationEdit*) AddLink
	(
		IDC_MESSAGES_BODYEDIT, 
		GetDocument()->m_pDBTPostaliteDocumentsGraphicNavigationDetail,
		RUNTIME_CLASS(CPostaliteDocumentsGraphicNavigationEdit),
		NULL,
		_TB("PostaLite Documents Graphic Navigation details")
	);
}

//-----------------------------------------------------------------------------
void CPostaliteDocumentsGraphicNavigationBodyEditView::OnInitialUpdate()
{
	CMasterFormView::OnInitialUpdate();
	GetDocument()->m_pBody->DoRecalcCtrlSize();
}


//////////////////////////////////////////////////////////////////////////////
//							CCompanyLayoutSplitter
//////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(CPostaliteDocumentsGraphicNavigationSplitter, CSplitterWnd)

BEGIN_MESSAGE_MAP(CPostaliteDocumentsGraphicNavigationSplitter, CSplitterWnd)
    ON_WM_SIZE		()
	ON_WM_LBUTTONUP	() 
END_MESSAGE_MAP()

//------------------------------------------------------------------
CPostaliteDocumentsGraphicNavigationSplitter::CPostaliteDocumentsGraphicNavigationSplitter()
{
	m_fSplitRatio	   = -1;
    m_bPanesSwapped	   = FALSE;
    m_nSplitResolution = 1;
}

//------------------------------------------------------------------
CPostaliteDocumentsGraphicNavigationSplitter::~CPostaliteDocumentsGraphicNavigationSplitter()
{
}

//------------------------------------------------------------------
CWnd* CPostaliteDocumentsGraphicNavigationSplitter::GetActivePane(int* pRow, int* pCol)
{
	ASSERT_VALID(this);

	// attempt to use active view of frame window
	CWnd* pView = NULL;
	CFrameWnd* pFrameWnd = GetParentFrame();
	ASSERT_VALID(pFrameWnd);
	pView = pFrameWnd->GetActiveView();

	// failing that, use the current focus
	if (pView == NULL)
		pView = GetFocus();

	return pView;
}

//------------------------------------------------------------------
void CPostaliteDocumentsGraphicNavigationSplitter::OnLButtonUp(UINT uFlags, CPoint point) 
{ 
	CSplitterWnd::OnLButtonUp(uFlags, point); 
    UpdateSplitRatio();
} 

//------------------------------------------------------------------
void CPostaliteDocumentsGraphicNavigationSplitter::SetSplitRatio(float fRatio)
{
    m_fSplitRatio = fRatio;
}

//------------------------------------------------------------------
BOOL CPostaliteDocumentsGraphicNavigationSplitter::IsSplitHorizontally() const
{
    ASSERT((m_nRows > 1) != (m_nCols > 1));
    ASSERT(max( m_nRows, m_nCols) == 2); 

    return (m_nCols > 1);
}

//------------------------------------------------------------------
void CPostaliteDocumentsGraphicNavigationSplitter::SplitHorizontally()
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
void CPostaliteDocumentsGraphicNavigationSplitter::SplitVertically()
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
void CPostaliteDocumentsGraphicNavigationSplitter::UpdateSplitRatio()
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

    if((pPanes[0].nCurSize != -1) && (pPanes[0].nCurSize + pPanes[1].nCurSize != 0))
        m_fSplitRatio = m_nSplitResolution * ((float)pPanes[0].nCurSize / (pPanes[0].nCurSize + pPanes[1].nCurSize + czSplitter));
}

//------------------------------------------------------------------
void CPostaliteDocumentsGraphicNavigationSplitter::UpdatePanes()
{
    CRect rcClient;
    GetClientRect(rcClient);

    UpdatePanes(rcClient.Width(), rcClient.Height());
}

//------------------------------------------------------------------
void CPostaliteDocumentsGraphicNavigationSplitter::UpdatePanes(int cx, int cy)
{
	CRowColInfo*	pPanes;
    int				cz;

    if(IsSplitHorizontally())
    {
        pPanes = m_pColInfo;
        cz     = cx;
    }
    else
    {
        pPanes = m_pRowInfo;
        cz     = cy;
    }

    if(m_fSplitRatio > 0)
       pPanes[0].nIdealSize = int( m_fSplitRatio * ((float)cz / m_nSplitResolution));
	else //caso in cui qualcosa é andato male
       pPanes[0].nIdealSize =  int(((float)60/100) * ((float)cz / m_nSplitResolution));
}

//------------------------------------------------------------------
void CPostaliteDocumentsGraphicNavigationSplitter::OnSize(UINT nType, int cx, int cy)
{
	if((nType != SIZE_MINIMIZED )&&( cx > 0 )&&( cy > 0 ))
        UpdatePanes(cx, cy);

    CSplitterWnd::OnSize(nType, cx, cy);
}


//////////////////////////////////////////////////////////////////////////////
// CCompanyLayoutEdit							
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE(CPostaliteDocumentsGraphicNavigationEdit, CBodyEdit)

BEGIN_MESSAGE_MAP(CPostaliteDocumentsGraphicNavigationEdit, CBodyEdit)
	//{{AFX_MSG_MAP(CPostaliteDocumentsGraphicNavigationEdit)
	ON_WM_WINDOWPOSCHANGED	()
	ON_WM_LBUTTONDBLCLK		()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------	
BOOL CPostaliteDocumentsGraphicNavigationEdit::OnDblClick(UINT nFlags, CBodyEditRowSelected* pCurrentRow)
{
	TEnhPostaliteDocumentsGraphicNavigationDetail*	pRec = (TEnhPostaliteDocumentsGraphicNavigationDetail*) pCurrentRow->m_pRec;
	BDPostaliteDocumentsGraphicNavigation*			pDoc = (BDPostaliteDocumentsGraphicNavigation*)GetDocument();

	if (pCurrentRow->m_nColumnIDC == IDC_MESSAGES_LAYOUT_FIELD_VALUE && pRec->l_FieldName == FIELD(DOCUMENT_FILENAME)) 
		pDoc->OpenMessagePdf();

	if (pCurrentRow->m_nColumnIDC == IDC_MESSAGES_LAYOUT_FIELD_VALUE && pRec->l_FieldName == FIELD(SUBJECT)) 
		pDoc->OpenRelatedDocument();

	if (pCurrentRow->m_nColumnIDC == IDC_MESSAGES_LAYOUT_FIELD_VALUE && pRec->l_FieldName == FIELD(ADDRESSE)) 
		pDoc->OpenAddressDocument();

	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL CPostaliteDocumentsGraphicNavigationEdit::OnGetCustomColor(CBodyEditRowSelected* pCurrentRow)
{
	TEnhPostaliteDocumentsGraphicNavigationDetail*	pRec = (TEnhPostaliteDocumentsGraphicNavigationDetail*) pCurrentRow->m_pRec;

	if (pRec->l_IsSeparator)
	{
		pCurrentRow->m_crBkgColor	= RGB(41, 57, 85);		//Blu
		pCurrentRow->m_crTextColor	= RGB(255, 255, 255);
	}
	else if (pCurrentRow->m_nColumnIDC == IDC_MESSAGES_LAYOUT_FIELD_NAME) 
	{
		pCurrentRow->m_crBkgColor	= RGB(69, 89, 124);		//Blu
		pCurrentRow->m_crTextColor	= RGB(224, 255, 255);	//Blu
	}
	else
	{
		pCurrentRow->m_crBkgColor	= RGB(188, 199, 216);	//Blu
	
		if (pRec->l_HasHyperlink)
			pCurrentRow->m_crTextColor	= RGB(0, 0, 255);
		else
			pCurrentRow->m_crTextColor	= RGB(0, 0, 0);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL CPostaliteDocumentsGraphicNavigationEdit::OnDrawCell(CBodyEditRowSelected* pCurrentRow, CDC* pDC, CRect& aRect)
{
	TEnhPostaliteDocumentsGraphicNavigationDetail*	pRec = (TEnhPostaliteDocumentsGraphicNavigationDetail*) pCurrentRow->m_pRec;
	BDPostaliteDocumentsGraphicNavigation*			pDoc = (BDPostaliteDocumentsGraphicNavigation*)GetDocument();

	if (pCurrentRow->m_nColumnIDC == IDC_MESSAGES_LAYOUT_FIELD_VALUE && pDoc->HasHyperLink(pRec)) 
		pDC->SelectObject(AfxGetThemeManager()->GetHyperlinkFont());
	else
		pDC->SelectObject(AfxGetThemeManager()->GetControlFont());

	return FALSE;
}

//-----------------------------------------------------------------------------	
void CPostaliteDocumentsGraphicNavigationEdit::Customize()
{
    ColumnInfo*					pColInfo;
    TEnhPostaliteDocumentsGraphicNavigationDetail*	pRec = (TEnhPostaliteDocumentsGraphicNavigationDetail*) m_pDBT->GetRecord();
	BDPostaliteDocumentsGraphicNavigation*			pDoc = (BDPostaliteDocumentsGraphicNavigation*)GetDocument();
	SetUITitlesRows(1);

	pColInfo = AddColumn
	(
		_NS_CLN("FieldName"),
		_TB(" "), 
		0,	
		IDC_MESSAGES_LAYOUT_FIELD_NAME,		
		&(pRec->l_FieldName),		
		RUNTIME_CLASS(CStrStatic)
	);
	pColInfo->SetCtrlSize(24,1);

	pColInfo = AddColumn
	(
		_NS_CLN("FieldValue"),
		_TB(" "), 
		0,	
		IDC_MESSAGES_LAYOUT_FIELD_VALUE,		
		&(pRec->l_FieldValue),		
		RUNTIME_CLASS(CStrStatic)
	);

	pColInfo->SetCtrlSize(32,4);

	SetMultipleLinesPerRow();

	EnableInsertRow(FALSE);
	EnableAddRow(FALSE);
	EnableDeleteRow(FALSE);
	EnableFormViewCall(FALSE);
	EnableSearch(FALSE, FALSE);
	EnableMultipleSel(FALSE);
}
