
#include "stdafx.h"

#include <TbGes\FormMng.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "DSTables.h"
#include "DSManager.h"
#include "DValidationMonitor.h"
#include "UIProviders.h"
#include "UIValidationMonitor.h"
#include "UIValidationMonitor.hjson"
#include "UIDSMonitor.h"
#include "UIDSMonitor.hjson"

//==============================================================================
static TCHAR szIconNamespace[] = _T("Image.Framework.TbFrameworkImages.Images.%s.%s.png");
static TCHAR sz25S[] = _T("25x25");

/*
//////////////////////////////////////////////////////////////////////////////////
//						CValidationFrame
//////////////////////////////////////////////////////////////////////////////////
//---------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CValidationFrame, CBatchFrame)

//-----------------------------------------------------------------------------------
BOOL CValidationFrame::OnCustomizeTabbedToolBar(CTBTabbedToolbar* pTabbedBar)
{
	__super::OnCustomizeTabbedToolBar(pTabbedBar);

	CTBToolBar* pToolBar = pTabbedBar->FindToolBar(szToolbarNameMain);
	if (!pToolBar)
		return FALSE;

	TCHAR		bufferRefresh[512];
	int			nResult = swprintf_s(bufferRefresh, szIconNamespace, sz25S, szIconRefresh);
	pToolBar->AddButton
	(
		ID_DS_MONITOR_REFRESH, 
		_NS_TOOLBARBTN("ValidationMonitorRefresh"), 
		bufferRefresh,
		_TB("Status Refresh")
	);
	
	pToolBar->RemoveButtonForID(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN);
	pToolBar->RemoveButtonForID(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN);

	DValidationMonitor*	pDoc = (DValidationMonitor*) GetDocument();

	if (!pDoc)
		return FALSE;

	pDoc->m_pViewFKToFixPanel = (CValidationMonitorFKToFixPanel*)CreateDockingPane
		(
			RUNTIME_CLASS(CValidationMonitorFKToFixPanel),
			IDD_DATAVALIDATION_MONITOR_FK_TO_FIX,
			_TB("Errors to Fix"),
			_TB("Errors to Fix"),
			CBRS_RIGHT,
			CSize(700, 300)
		);

	if (pDoc->m_pViewFKToFixPanel)
		pDoc->m_pViewFKToFixPanel->SetAutoHideMode(TRUE, CBRS_RIGHT | CBRS_HIDE_INPLACE, 0, TRUE);

	return TRUE;
}*/

/////////////////////////////////////////////////////////////////////////////
//				CValidationMonitorView Implementation
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(CValidationMonitorView, CJsonFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CValidationMonitorView, CJsonFormView)
	//{{AFX_MSG_MAP(CValidationMonitorView)
		ON_WM_TIMER()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CValidationMonitorView::CValidationMonitorView()
	:
	CJsonFormView(IDD_DATASYNCHRO_MONITOR)
{
//	EnableLayout(TRUE);
}

//-----------------------------------------------------------------------------
DValidationMonitor* CValidationMonitorView::GetDocument() const
{
	return (DValidationMonitor*) m_pDocument;
}

/*
//-----------------------------------------------------------------------------
void CValidationMonitorView::BuildDataControlLinks()
{
	AddTileGroup
	(
		IDC_DS_MONITOR_TILE_GRP,
		RUNTIME_CLASS(CValidationMonitorTileGrp),
		_NS_TILEGRP("CValidationMonitorTileGrp")
	);

	GetDocument()->SetTileStatus();
}
*/
//------------------------------------------------------------------------------
void CValidationMonitorView::OnTimer(UINT nUI)
{
	switch (nUI)
	{
	case CHECK_VALIDATION_MONITOR_TIMER:
		GetDocument()->DoOnTimer();
		break;

	case CHECK_VALIDATION_GAUGE_TIMER:
		GetDocument()->DoOnGaugeTimer();
		break;
	default:
		break;
	}
}
/*
//////////////////////////////////////////////////////////////////////////////
//						class CValidationMonitorTileGrp implemenation
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CValidationMonitorTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CValidationMonitorTileGrp::Customize()
{
	DValidationMonitor* pDoc = (DValidationMonitor*) GetDocument();

	SetLayoutType(CLayoutContainer::VBOX);

	CTilePanel* pPanel = AddPanel(_T("DValidationMonitorTilePanel"), _TB("Filters"), CLayoutContainer::COLUMN, CLayoutContainer::BEGIN);

	if (pPanel)
	{
		pPanel->SetTileStyle(AfxGetTileDialogStyleNormal());
		pPanel->SetTileDialogStyle(AfxGetTileDialogStyleFilter());

		pPanel->AddJsonTile(IDD_DATAVALIDATION_MONITOR_FILTER_BY_PROVIDER);

		if (!pDoc->m_bFromBatch) 
		{
			pPanel->AddJsonTile(IDD_DATAVALIDATION_MONITOR_FILTER_BY_DATE);
		}

		pPanel->AddJsonTile(IDD_DATAVALIDATION_MONITOR_FILTER_BY_OTHER);
		
		pPanel->SetCollapsible(TRUE);
		pPanel->SetCollapsed(FALSE);
	}

	AddJsonTile(IDD_DATASYNCHRO_MONITOR_STATUS);
	
	pDoc->m_pResultsTilePanel = AddPanel(_T("ValidationMonitorReportErrors"), _TB("Report errors"), CLayoutContainer::VBOX);
	pDoc->m_pResultsTilePanel->SetGroupCollapsible();
	pDoc->m_pResultsTilePanel->SetCollapsible();
	pDoc->m_pResultsTilePanel->AddJsonTile(IDD_DATAVALIDATION_MONITOR_SUMMARY);
	pDoc->m_pResultsTilePanel->AddJsonTile(IDD_DATAVALIDATION_MONITOR_DETAIL);
}
*/

/////////////////////////////////////////////////////////////////////////////
//				CDocumentsToValidateCombo Implementation
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE (CDocumentsToValidateCombo, CStrCombo)

//-----------------------------------------------------------------------------
CDocumentsToValidateCombo::CDocumentsToValidateCombo()
	:
	CStrCombo()
{
}
	
//-----------------------------------------------------------------------------
void CDocumentsToValidateCombo::OnFillListBox()
{
	CStrCombo::OnFillListBox();

	DValidationMonitor* pDoc = (DValidationMonitor*) GetDocument();
	CSynchroDocInfo* pDocInfo = NULL;

	for (int i = 0 ; i < pDoc->m_pDocumentsToMonitor->GetSize(); i++)
	{
		pDocInfo = (CSynchroDocInfo*)pDoc->m_pDocumentsToMonitor->GetAt(i);
		AddAssociation(pDocInfo->m_docTitle, pDocInfo->m_strDocNamespace);
	}
	
}

/////////////////////////////////////////////////////////////////////////////
//				class CValidationMonitorDetailBodyEdit Implementation		//
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
IMPLEMENT_DYNCREATE(CValidationMonitorDetailBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------
CValidationMonitorDetailBodyEdit::CValidationMonitorDetailBodyEdit()
{ 
	BERemoveExStyle(BE_STYLE_ALLOW_INSERT | BE_STYLE_ALLOW_DELETE | BE_STYLE_ALLOW_MULTIPLE_SEL);
}

/////////////////////////////////////////////////////////////////////////////
//				class CValidationSummaryDetailBodyEdit Implementation		//
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
IMPLEMENT_DYNCREATE(CValidationSummaryDetailBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------
CValidationSummaryDetailBodyEdit::CValidationSummaryDetailBodyEdit()
{
	BERemoveExStyle(BE_STYLE_ALLOW_INSERT | BE_STYLE_ALLOW_DELETE | BE_STYLE_ALLOW_MULTIPLE_SEL);
}

//-----------------------------------------------------------------------------	
BOOL CValidationSummaryDetailBodyEdit::OnDblClick(UINT nFlags, CBodyEditRowSelected* pCurrentRow)
{
	DValidationMonitor* pDoc = (DValidationMonitor*)GetDocument();
	pDoc->ExpandDetail();
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//				class CValidationMonitorFKToFixPanel Implementation		   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
IMPLEMENT_DYNCREATE(CValidationMonitorFKToFixPanel, CTaskBuilderDockPane)

//-----------------------------------------------------------------------------
CValidationMonitorFKToFixPanel::CValidationMonitorFKToFixPanel()
:
CTaskBuilderDockPane(RUNTIME_CLASS(CValidationMonitorFKToFixView))
{
}

/////////////////////////////////////////////////////////////////////////////
//				class CValidationMonitorFKToFixView Implementation		   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
IMPLEMENT_DYNCREATE(CValidationMonitorFKToFixView, CJsonFormView)

//-----------------------------------------------------------------------------
CValidationMonitorFKToFixView::CValidationMonitorFKToFixView()
	:
	CJsonFormView(IDD_DATAVALIDATION_MONITOR_FK_TO_FIX)
{ 
}

/*
//-----------------------------------------------------------------------------
void CValidationMonitorFKToFixView::BuildDataControlLinks()
{
	AddTileGroup
	(
		IDC_TG_DATAVALIDATION_MONITOR_FK_TO_FIX,
		RUNTIME_CLASS(CValidationMonitorFKToFixTileGrp),
		_NS_TILEGRP("CValidationMonitorFKToFix")
	);
}

/////////////////////////////////////////////////////////////////////////////
//				class CValidationMonitorFKToFixTileGrp Implementation	   //
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
IMPLEMENT_DYNCREATE(CValidationMonitorFKToFixTileGrp, CTileGroup)

//-----------------------------------------------------------------------------
void CValidationMonitorFKToFixTileGrp::Customize()
{
	SetLayoutType(CLayoutContainer::VBOX);
	SetLayoutAlign(CLayoutContainer::STRETCH);

	AddJsonTile(IDD_DATAVALIDATION_MONITOR_FK_TO_FIX_SUMMARY);
	AddJsonTile(IDD_DATAVALIDATION_MONITOR_FK_TO_FIX_DETAIL);
}*/