
#include "stdafx.h"

#include <TbGes\FormMng.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbGes\BODYEDIt.hjson>
#include "DSTables.h"
#include "DSManager.h"
#include "DSMonitor.h"
#include "UIProviders.h"
#include "UIDSMonitor.h"
#include "UIDSMonitor.hjson"

//==============================================================================
const TCHAR szTbDataSynchroClient[]			= _T("Module.Extensions.TbDataSynchroClient");
const CTBNamespace snsTbDataSynchroClient	= szTbDataSynchroClient;
//==============================================================================
const TCHAR szDataSynchronizerMonitor[]		= _T("DataSynchronizerMonitor");
const TCHAR szRefreshTimer[]				= _T("RefreshTimer");
//==============================================================================

static TCHAR szIconNamespace		[] = _T("Image.Framework.TbFrameworkImages.Images.%s.%s.png");
static TCHAR sz25S					[] = _T("25x25");

//////////////////////////////////////////////////////////////////////////////////
//						CDSMonitorFrame
//////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDSMonitorFrame, CBatchFrame)

//-----------------------------------------------------------------------------------
BOOL CDSMonitorFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	__super::OnCustomizeTabbedToolBar(pTabbedBar);

	DSMonitor*	pDoc = (DSMonitor*)GetDocument();
	CTBToolBar* pToolBar = pTabbedBar->FindToolBar(szToolbarNameMain);
	TCHAR		bufferRefresh[512];
	TCHAR		bufferRecovery[512];
	TCHAR		PauseMassiveSynchro[512];
	TCHAR		ContinueMassiveSynchro[512];
	TCHAR		AbortMassiveSynchro[512];
	int			nResult;
	
	nResult = swprintf_s(bufferRefresh,		szIconNamespace, sz25S, szIconRefresh);
	nResult = swprintf_s(bufferRecovery, szIconNamespace, sz25S, szIconRecovery);
	nResult = swprintf_s(PauseMassiveSynchro, szIconNamespace, sz25S, szIconPauseMassiveSynchro);
	nResult = swprintf_s(ContinueMassiveSynchro, szIconNamespace, sz25S, szIconContinueMassiveSynchro);
	nResult = swprintf_s(AbortMassiveSynchro, szIconNamespace, sz25S, szIconAbortMassiveSynchro);

	pToolBar->AddButton
	(
		ID_DS_MONITOR_REFRESH, 
		_NS_TOOLBARBTN("BatchFrameDSMonitorRefresh"), 
		bufferRefresh, 
		_TB("Status Refresh")
	);

	pToolBar->AddSeparator();

	pToolBar->AddButton
	(
		ID_DS_MONITOR_ERR_START_RECOVERY_BUTTON,
		_NS_TOOLBARBTN("BatchFrameDSMonitorRecoveryErrors"),
		bufferRecovery,
		_TB("Start Errors Recovery")
	);

	pToolBar->AddButton
	(
		ID_DS_MONITOR_PAUSE_BUTTON,
		_NS_TOOLBARBTN("BatchFrameDSMonitorPause"),
		PauseMassiveSynchro,
		_TB("Pause")
	);

	pToolBar->AddButton
	(
		ID_DS_MONITOR_CONTINUE_BUTTON,
		_NS_TOOLBARBTN("BatchFrameDSMonitorContinue"),
		ContinueMassiveSynchro,
		_TB("Resume")
	);

	pToolBar->AddButton
	(
		ID_DS_MONITOR_ABORT_BUTTON,
		_NS_TOOLBARBTN("BatchFrameDSMonitorAbort"),
		AbortMassiveSynchro,
		_TB("Abort")
	);

	pToolBar->RemoveButtonForID(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN);
	pToolBar->RemoveButtonForID(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN);

	CTaskBuilderDockPane* pPane = CreateDockingPane
	(
		RUNTIME_CLASS(CDSMonitorLegendView),
		IDD_DATASYNCHRO_MONITOR_LEGEND,
		_T("Legend"),
		_TB("Legend"),
		CBRS_RIGHT,
		CSize(360, 200),
		NULL
	);

	if (pPane)
		pPane->SetAutoHideMode(TRUE, CBRS_RIGHT | CBRS_HIDE_INPLACE);

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//						class CDSMonitorTileGrp implemenation
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDSMonitorTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CDSMonitorTileGrp::Customize()
{
	DSMonitor* pDoc = (DSMonitor*)GetDocument();

	SetLayoutType(CLayoutContainer::VBOX);
	SetLayoutAlign(CLayoutContainer::STRETCH);

	CTilePanel* pPanel = AddPanel(_T("DSMonitorFilterTilePanel"), _TB("Filters"), CLayoutContainer::COLUMN, CLayoutContainer::BEGIN);

	if (pPanel)
	{
		pPanel->SetTileStyle(AfxGetTileDialogStyleNormal());
		pPanel->SetTileDialogStyle(AfxGetTileDialogStyleFilter());

		pPanel->AddJsonTile(IDD_DATASYNCHRO_MONITOR_FILTER_BY_PROVIDER);

		if (!pDoc->m_bFromBatch)
		{
			pPanel->AddJsonTile(IDD_DATASYNCHRO_MONITOR_FILTER_BY_STATUS);
			pPanel->AddJsonTile(IDD_DATASYNCHRO_MONITOR_FILTER_BY_DATE);
		}

		pPanel->AddJsonTile(IDD_DATASYNCHRO_MONITOR_FILTER_BY_OTHER);

		pPanel->SetCollapsible(TRUE);
		pPanel->SetCollapsed(FALSE);
	}

	AddJsonTile(IDD_DATASYNCHRO_MONITOR_STATUS);

	pDoc->m_pResultsTilePanel = AddPanel(_T("SynchroMonitorReport"), _TB("Report"), CLayoutContainer::VBOX);
	pDoc->m_pResultsTilePanel->SetGroupCollapsible();
	pDoc->m_pResultsTilePanel->SetCollapsible();
	
	if (pDoc->m_bIsActivatedCRMInfinity) // La gestonione "Recovery Errors" è gestita solo per CRM Infinity
	{
		if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			pDoc->m_pSummaryTile =  pDoc->m_pResultsTilePanel->AddJsonTile(IDD_DATASYNCHRO_MONITOR_DETAIL_DOC_SUMMARY);
		else
			pDoc->m_pSummaryTile = pDoc->m_pResultsTilePanel->AddJsonTile(IDD_DATASYNCHRO_MONITOR_DETAIL_DOC_SUMMARY_IMS);
	}

	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		pDoc->m_pMonitorTile =  pDoc->m_pResultsTilePanel->AddJsonTile(IDD_DATASYNCHRO_MONITOR_DETAIL);
	else
		pDoc->m_pMonitorTile =  pDoc->m_pResultsTilePanel->AddJsonTile(IDD_DATASYNCHRO_MONITOR_DETAIL_IMS);
	pDoc->SetCollapsedResultsPanel();
}

/////////////////////////////////////////////////////////////////////////////
//				CDSMonitorView Implementation
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CDSMonitorView, CMasterFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CDSMonitorView, CMasterFormView)
	//{{AFX_MSG_MAP(CDSMonitorView)
		ON_WM_TIMER()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDSMonitorView::CDSMonitorView()
	:
	CMasterFormView(_NS_VIEW("SynchroMonitor"), IDD_DATASYNCHRO_MONITOR)
{
	EnableLayout(TRUE);
}

//-----------------------------------------------------------------------------
DSMonitor* CDSMonitorView::GetDocument() const
{
	return (DSMonitor*)m_pDocument;
}

//-----------------------------------------------------------------------------
void CDSMonitorView::BuildDataControlLinks()
{
	AddTileGroup
	(
		IDC_DS_MONITOR_TILE_GRP,
		RUNTIME_CLASS(CDSMonitorTileGrp),
		_NS_TILEGRP("DSMonitorTileGrp")
	);
}

//------------------------------------------------------------------------------
void CDSMonitorView::OnTimer (UINT nUI)
{   
	switch (nUI)
	{
	case CHECK_DS_MONITOR_TIMER:
		GetDocument()->DoOnTimer();
		break;

	case CHECK_DS_GAUGE_TIMER:
		GetDocument()->DoOnGaugeTimer();
		break;
	default:
		break;
	}
}


/////////////////////////////////////////////////////////////////////////////
//			Class CDSMonitorLegendTileGrp implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDSMonitorLegendTileGrp, CTileGroup)

//----------------------------------------------------------------------------
void CDSMonitorLegendTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);
	SetLayoutAlign(CLayoutContainer::STRETCH);

	AddJsonTile(IDD_DATASYNCHRO_MONITOR_LEGEND_TILE);

}

/////////////////////////////////////////////////////////////////////////////
//			Class CDSMonitorLegendView implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDSMonitorLegendView, CMasterFormView)

//-----------------------------------------------------------------------------
CDSMonitorLegendView::CDSMonitorLegendView()
	:
	CMasterFormView(_NS_VIEW("ActivityLegend"), IDD_DATASYNCHRO_MONITOR_LEGEND)
{}

//-----------------------------------------------------------------------------
void CDSMonitorLegendView::BuildDataControlLinks()
{
	AddTileGroup(IDC_DS_MONITOR_LEGEND_TILE_GRP, RUNTIME_CLASS(CDSMonitorLegendTileGrp), _NS_TILEGRP("DSMonitorLegendTileGrp"));
}


/////////////////////////////////////////////////////////////////////////////
//				CDocumentsToSynchCombo Implementation
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE (CDocumentsToSynchCombo, CStrCombo)

//-----------------------------------------------------------------------------
CDocumentsToSynchCombo::CDocumentsToSynchCombo()
	:
	CStrCombo()
{
}
	
//-----------------------------------------------------------------------------
void CDocumentsToSynchCombo::OnFillListBox()
{
	CStrCombo::OnFillListBox();

	DSMonitor* pDoc = (DSMonitor*) GetDocument();
	CSynchroDocInfo* pDocInfo;

	if (pDoc->m_bIsActivatedCRMInfinity && pDoc->m_bShowOnlyDocWithErr) // visualizzo solo i documenti che hanno errori
	{
		for (int i = 0 ; i < pDoc->m_pOnlyDocWithErr->GetSize(); i++)
		{
			pDocInfo = (CSynchroDocInfo*)pDoc->m_pOnlyDocWithErr->GetAt(i);
			AddAssociation(pDocInfo->m_docTitle, pDocInfo->m_strDocNamespace);
		}  
	}
	else
	{
		for (int i = 0 ; i < pDoc->m_pDocumentsToMonitor->GetSize(); i++)
		{
			pDocInfo = (CSynchroDocInfo*)pDoc->m_pDocumentsToMonitor->GetAt(i);
			AddAssociation(pDocInfo->m_docTitle, pDocInfo->m_strDocNamespace);
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
//				class CMonitorDetailBodyEdit Implementation		 //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
IMPLEMENT_DYNCREATE(CMonitorDetailBodyEdit, CJsonBodyEdit)

BEGIN_MESSAGE_MAP(CMonitorDetailBodyEdit, CJsonBodyEdit)
	ON_COMMAND(ID_MONITOR_BTN_NEXT, OnNextClicked)
	ON_COMMAND(ID_MONITOR_BTN_PREV, OnPrevClicked)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMonitorDetailBodyEdit::CMonitorDetailBodyEdit()
	:
	CJsonBodyEdit(),
	m_pBEBtnPrev	(NULL),
	m_pBEBtnNext	(NULL)
{ 
	n_Page=1;
	BERemoveExStyle(BE_STYLE_ALLOW_INSERT | BE_STYLE_ALLOW_DELETE | BE_STYLE_ALLOW_MULTIPLE_SEL );
	BERemoveExStyle(BE_STYLE_SHOW_ROWINDICATOR);
}

//-----------------------------------------------------------------------------
void CMonitorDetailBodyEdit::OnBeforeCustomize()
{
	if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		m_pBEBtnNext = m_FooterToolBar.AddButton
		(
			_NS_BE_TOOLBAR_BTN("PageNext"),
			ID_MONITOR_BTN_NEXT,
			TBIcon(szIconArrowDown, MINI),
			_TB("Next Page"),
			_TB("Next")
		);

		m_pBEBtnPrev = m_FooterToolBar.AddButton
		(
			_NS_BE_TOOLBAR_BTN("PageNext"),
			ID_MONITOR_BTN_PREV,
			TBIcon(szIconArrowUp, MINI),
			_TB("Previous Page"),
			_TB("Prev")
		);

		m_FooterToolBar.AddControl(RUNTIME_CLASS(CStrStatic), _NS_BE_TOOLBAR_BTN("PageBlock"), IDC_MONITOR_BTN_PAGE, _TB("Page no."), L"", 150, 1409286144UL,2,&(((DSMonitor*)GetDocument())->m_nDetailPageStr));
	}
}

//-----------------------------------------------------------------------------	
void CMonitorDetailBodyEdit::OnNextClicked()
{
	if (GetDocument()->m_nDetailPage < (GetDocument()->m_nPageTot-1))
	{
		((DSMonitor*)GetDocument())->m_nDetailPage++;
		((DSMonitor*)GetDocument())->m_nDetailPageStr = _TB("Pag.") + (((DSMonitor*)GetDocument())->m_nDetailPage+1).ToString() + _T("/") + ((DSMonitor*)GetDocument())->m_nPageTot.ToString();
		((DSMonitor*)GetDocument())->m_pDBVTDetail->ReloadData();
	}
}

//-----------------------------------------------------------------------------	
void CMonitorDetailBodyEdit::OnPrevClicked()
{
	((DSMonitor*)GetDocument())->m_nDetailPage > 0 ? ((DSMonitor*)GetDocument())->m_nDetailPage-- : 0;
	((DSMonitor*)GetDocument())->m_nDetailPageStr = _TB("Pag.") + ((DSMonitor*)GetDocument())->m_nDetailPage.ToString() + _T("/") + ((DSMonitor*)GetDocument())->m_nPageTot.ToString();
	((DSMonitor*)GetDocument())->m_pDBVTDetail->ReloadData();
}


//-----------------------------------------------------------------------------
void CMonitorDetailBodyEdit::Customize()
{
	__super::Customize();
	if (m_FooterToolBar.FindButtonIndex(IDC_BE_ROWINDICATOR) > 1)
		m_FooterToolBar.RemoveAt(m_FooterToolBar.FindButtonIndex(IDC_BE_ROWINDICATOR));
}

/////////////////////////////////////////////////////////////////////////////
//				class CMonitorSummaryDetailBodyEdit Implementation		//
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
IMPLEMENT_DYNCREATE(CMonitorSummaryDetailBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------
CMonitorSummaryDetailBodyEdit::CMonitorSummaryDetailBodyEdit() 
{ 
	BERemoveExStyle(BE_STYLE_ALLOW_INSERT | BE_STYLE_ALLOW_DELETE | BE_STYLE_ALLOW_MULTIPLE_SEL);
}

//-----------------------------------------------------------------------------	
BOOL CMonitorSummaryDetailBodyEdit::OnDblClick(UINT nFlags, CBodyEditRowSelected* pCurrentRow)
{
	DSMonitor* pDoc = (DSMonitor*)GetDocument();
	pDoc->ExpandDetail();
	return TRUE;
}