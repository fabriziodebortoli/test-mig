
#include "stdafx.h"

// local declaration
#include "TBActivityDocument.h"
#include "TBActivityDocument.hjson"
#include "UITileDialog.hjson"

#include <TbGeneric\TileStyle.h>
#include <TbGenlib\TilePanel.h>
#include <TbGes\TileManager.h>
#include <TbGes\UnpinnedTilesPane.h>


#include <TbGes\ExtDocView.h>
#include <TbGes\HotFilterManager.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

#define MAX_RECORD		 10000
#define RECORD_TO_DELETE 1000

static const TCHAR szFilters[] = _T("filters");
static const TCHAR szActions[] = _T("actions");
static const TCHAR szFooter	[] = _T("footer");

//////////////////////////////////////////////////////////////////////////////
//									TSummaryDetail							//
//////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TSummaryDetail, SqlVirtualRecord)

//-----------------------------------------------------------------------------
TSummaryDetail::TSummaryDetail(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("TSummaryDetail"))
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TSummaryDetail::BindRecord()
{
	BEGIN_BIND_DATA();
	LOCAL_STR(_NS_LFLD("LineSummaryDescription"), l_LineSummaryDescription, 256);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TSummaryDetail::GetStaticName() {
	return _NS_TBL("TSummaryDetail");
}

//////////////////////////////////////////////////////////////////////////////
//									DBTSummaryDetail						//
//////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTSummaryDetail, DBTSlaveBuffered)

//--------------------------------------------------------------------------------
DBTSummaryDetail::DBTSummaryDetail(CRuntimeClass* pClass, CAbstractFormDoc* pDoc)
	:
	DBTSlaveBuffered(pClass, pDoc, _NS_DBT("DBTSummaryDetail"), ALLOW_EMPTY_BODY, FALSE)
{

}

/////////////////////////////////////////////////////////////////////////////
//					class CTBActivityFrame Implementation
/////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTBActivityFrame, CBatchFrame)

//------------------------------------------------------------------------------
CTBActivityFrame::CTBActivityFrame()
	:
	CBatchFrame()
{

}

//----------------------------------------------------------------------------------
BOOL CTBActivityFrame::OnCustomizeJsonToolBar()
{
	BOOL bRet = __super::OnCustomizeJsonToolBar();
	return bRet && CreateJsonToolbar(IDD_TB_ACTIVITY_TOOLBAR);
}

//-----------------------------------------------------------------------------------------------
BOOL CTBActivityFrame::CreateAuxObjects(CCreateContext*	pCreateContext)
{
	CTBActivityDocument* pDoc = (CTBActivityDocument*)GetDocument();

	if (!pDoc->m_pActivityLegend)
		return TRUE;

	pDoc->m_pActivityLegend->m_pCreateContext->m_pCurrentDoc = pCreateContext->m_pCurrentDoc;
	pDoc->m_pActivityLegend->m_pCreateContext->m_pNewViewClass = RUNTIME_CLASS(CJsonFormView);
	//manage legend
	
	pDoc->CustomizeLegend(pDoc->m_pActivityLegend);

	CTaskBuilderDockPane* pPane = CreateJsonDockingPane
	(
			pDoc->m_pActivityLegend->m_nIDD,
			pDoc->m_pActivityLegend->GetLegendName(), 
			pDoc->m_pActivityLegend->m_LegendTitle,
			pDoc->m_pActivityLegend->m_wAlignment,
			pDoc->m_pActivityLegend->m_Size,
			pDoc->m_pActivityLegend->m_pCreateContext
	);

	if (pPane)
		pPane->SetAutoHideMode(TRUE, pDoc->m_pActivityLegend->m_wAlignment | CBRS_HIDE_INPLACE);
	
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//					class CTBActivityWithoutLoadDataFrame Implementation
/////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTBActivityWithoutLoadDataFrame, CTBActivityFrame)

//------------------------------------------------------------------------------
CTBActivityWithoutLoadDataFrame::CTBActivityWithoutLoadDataFrame()
	:
	CTBActivityFrame()
{

}

//----------------------------------------------------------------------------------
BOOL CTBActivityWithoutLoadDataFrame::OnCustomizeJsonToolBar()
{
	BOOL bRet = __super::OnCustomizeJsonToolBar();
	return bRet && CreateJsonToolbar(IDD_TB_ACTIVITY_NO_LOAD_DATA_FRAME);
}
//////////////////////////////////////////////////////////////////////////////
//							CTBActivityDocument								//
//////////////////////////////////////////////////////////////////////////////
//
//-------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTBActivityDocument, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTBActivityDocument, CAbstractFormDoc)
	ON_COMMAND			(ID_TB_ACTIVITY_FRAME_LOAD_DATA,						OnLoadDataStart)
	ON_COMMAND			(ID_TB_ACTIVITY_FRAME_LOAD_PAUSE,						OnLoadPause)
	ON_COMMAND			(ID_TB_ACTIVITY_FRAME_LOAD_RESUME,						OnLoadResume)
	ON_COMMAND			(ID_TB_ACTIVITY_FRAME_LOAD_STOP,						OnLoadStop)
	ON_COMMAND			(ID_TB_ACTIVITY_FRAME_ADD_DATA,							OnAddData)
	ON_COMMAND			(ID_TB_ACTIVITY_FRAME_UNDO_EXTRACTION,					OnUndoExtraction)
	ON_COMMAND			(ID_TB_ACTIVITY_DOCUMENT_GRID_SELECT_DESELECT,			OnSelectDeselectClicked)
	//manage update cmd
	ON_UPDATE_COMMAND_UI(ID_TB_ACTIVITY_FRAME_LOAD_DATA,						OnUpdateLoadDataStart)
	ON_UPDATE_COMMAND_UI(ID_TB_ACTIVITY_FRAME_LOAD_PAUSE,						OnUpdateLoadPause)
	ON_UPDATE_COMMAND_UI(ID_TB_ACTIVITY_FRAME_LOAD_RESUME,						OnUpdateLoadResume)
	ON_UPDATE_COMMAND_UI(ID_TB_ACTIVITY_FRAME_LOAD_STOP,						OnUpdateLoadStop)
	ON_UPDATE_COMMAND_UI(ID_TB_ACTIVITY_FRAME_ADD_DATA,							OnUpdateAddData)
	ON_UPDATE_COMMAND_UI(ID_TB_ACTIVITY_FRAME_UNDO_EXTRACTION,					OnUpdateUndoExtraction)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_BATCH_STARTSTOP_TOOLBTN,						OnUpdateBatchStartStop)
	ON_UPDATE_COMMAND_UI(ID_EXTDOC_BATCH_PAUSERESUME_TOOLBTN,					OnUpdateBatchPauseResume)
	ON_CONTROL			(BEN_ROW_CHANGED, IDC_TB_ACTIVITY_DOCUMENT_GRID,		OnGridRowChanged)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------
CTBActivityDocument::CTBActivityDocument()
	:
	m_bExtractingData				(FALSE),
	m_bExtractData					(FALSE),
	m_bAddMoreData					(FALSE),
	m_bManageSelectButton			(TRUE),
	m_pActivityLegend				(NULL),
	m_eFiltersActionOnExtract		(E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE),
	m_eActionsActionOnExtract		(E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE),
	m_eFooterActionOnExtract		(E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE),
	m_bAskDeleteConfirmation		(TRUE),
	m_bActionsAlwaysEnabled			(FALSE),
	m_bActionsAsFilters				(FALSE),
	m_bFooterCollapsibile			(TRUE),
	m_bFooterAlwaysEnabled			(FALSE),
	m_eResultsType					(E_ACTIVITYTYPE::ACTIVITY_EMPTY),
	m_pGauge						(NULL),
	m_pGaugeLabel					(NULL),
	m_pDBTSummary					(NULL),
	m_pPanelFilters					(NULL),
	m_pPanelActions					(NULL),
	m_pPanelFooter					(NULL),
	m_nCurrentElement				(0),
	m_bSelect						(TRUE),
	m_HeaderTitle					(_T("")),
	m_bHeaderTitleBold				(FALSE),
	m_HeaderSubTitle				(_T("")),
	m_bHeaderSubTitleBold			(FALSE),
	m_FiltersPanelText				(_TB("Filters")),
	m_ActionsPanelText				(_TB("Actions")),
	m_FooterPanelText				(_TB("Footer")),
	m_nStep							(0),
	m_nProgressStep					(1)
{
	m_bBatch = TRUE;

	SetGaugeUpperRange();

	SetDocAccel(IDR_TB_ACTIVITY_DOCUMENT_ACCELERATOR);
	SetResourceModule(GetDllInstance(RUNTIME_CLASS(CTBActivityDocument)));
}

//---------------------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::OnAttachData()
{
	DECLARE_VAR_JSON(HeaderTitle);
	DECLARE_VAR_JSON(HeaderSubTitle);
	DECLARE_VAR_JSON(FiltersPanelText);
	DECLARE_VAR_JSON(ActionsPanelText);
	DECLARE_VAR_JSON(FooterPanelText);

	switch (m_eResultsType)
	{
	case E_ACTIVITYTYPE::ACTIVITY_GRID_SUMMARY:
		m_pDBTSummary = new DBTSummaryDetail(RUNTIME_CLASS(TSummaryDetail), this);
		break;
	case E_ACTIVITYTYPE::ACTIVITY_PROGRESS:
		//manage json var for Gauge
		DECLARE_VAR_JSON(nCurrentElement);
		DECLARE_VAR_JSON(GaugeDescription);
		break;
	default:
		break;
	}

	return TRUE;
}

//-------------------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::OnOpenDocument(LPCTSTR lpszPathName)
{
	if (!__super::OnOpenDocument(lpszPathName))
		return FALSE;

	if (IsEditingParamsFromExternalController())
	{
		m_eFiltersActionOnExtract = E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE;
		m_eActionsActionOnExtract = E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE;
		m_bActionsAlwaysEnabled = TRUE;
	}

	return TRUE;
}

//-------------------------------------------------------------------------------------------------------------
void CTBActivityDocument::CustomizeFrame
	(
				BOOL					bHasLegend	/*= FALSE*/
	)
{
	if (bHasLegend)
		m_pActivityLegend		= new CTBActivityLegend();

}

//-----------------------------------------------------------------------------------------------------------------------------------------------
void CTBActivityDocument::CustomizeFilters
	(
				E_ACTIVITY_PANELACTION eAction		/*= E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE*/
	)
{
	m_eFiltersActionOnExtract	= eAction;
}

//-----------------------------------------------------------------------------------------------------------------------------------------------
void CTBActivityDocument::CustomizeActions
	(
				E_ACTIVITY_PANELACTION	eAction					/*= E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE*/,
				BOOL					bActionsAlwaysEnabled	/*= FALSE*/,
				BOOL					bActionsAsFilters		/*= FALSE*/
	)
{
	m_eActionsActionOnExtract	= eAction;
	m_bActionsAlwaysEnabled		= bActionsAlwaysEnabled;
	m_bActionsAsFilters			= bActionsAsFilters;
}

//--------------------------------------------------------------------------------------------------------
void CTBActivityDocument::CustomizeResults
	(
				E_ACTIVITYTYPE	eResultsType			/* E_ACTIVITYTYPE::EMPTY*/,
				BOOL			bManageSelectButton		/*= TRUE*/
	)
{
	m_eResultsType				= eResultsType;
	m_bManageSelectButton		= bManageSelectButton;
}

//-----------------------------------------------------------------------------------------------------------------------------------------------
void CTBActivityDocument::CustomizeFooter
	(
				E_ACTIVITY_PANELACTION	eAction			/*= E_ACTIVITY_PANELACTION::ACTIVITY_NOT_COLLAPSE*/,
				BOOL			bFooterAlwaysEnabled	/*= FALSE*/
	)
{
	m_eFooterActionOnExtract	= eAction;
	m_bFooterAlwaysEnabled		= bFooterAlwaysEnabled;
}

//-----------------------------------------------------------------------------
CTBActivityDocument::~CTBActivityDocument()
{
	SAFE_DELETE(m_pActivityLegend)
	SAFE_DELETE(m_pDBTSummary)
}

//--------------------------------------------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::CanDoBatchExecute()
{
	if (IsEditingParamsFromExternalController())
		return TRUE;

	if (m_eResultsType == E_ACTIVITYTYPE::ACTIVITY_GRID && (!GetDBT() || GetDBT()->GetUpperBound() < 0))
		return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------------------
void CTBActivityDocument::CustomizeBodyEdit(CBodyEdit* pBodyEdit)
{
	if (m_eResultsType != E_ACTIVITYTYPE::ACTIVITY_GRID || !m_bManageSelectButton || IDC_TB_ACTIVITY_DOCUMENT_GRID != pBodyEdit->GetBodyEditID())
		return;

	pBodyEdit->m_HeaderToolBar.AddButton
	(
		_NS_BE_TOOLBAR_BTN("SelDesel"), 
		ID_TB_ACTIVITY_DOCUMENT_GRID_SELECT_DESELECT, 
		TBIcon(szIconSelect, MINI), 
		_TB("Select"), 
		_TB("Select"), 
		0
	);
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnBEEnableButton(CBodyEdit* pBodyEdit, CBEButton* pButton)
{
	UINT nIdBE		= pBodyEdit->GetBodyEditID();
	UINT nIdButton	= pButton->GetBEButtonID();

	if (nIdBE != IDC_TB_ACTIVITY_DOCUMENT_GRID)
		return;

	if (nIdButton != ID_TB_ACTIVITY_DOCUMENT_GRID_SELECT_DESELECT)
		return;

	BOOL bEnable = !m_bBatchRunning && m_bExtractData;

	pButton->EnableButton(bEnable);
}

//------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::OnClearDBT()
{
	switch (m_eResultsType)
	{
		case ACTIVITY_EMPTY:
		case ACTIVITY_PROGRESS:
			return TRUE;
		case ACTIVITY_GRID:
			if (m_bAskDeleteConfirmation && AfxMessageBox(_TB("Are you sure to clear the result grid?"), MB_YESNO | MB_ICONQUESTION) == IDNO)
				return FALSE;

			ASSERT(GetDBT());

			if (!GetDBT())
				return FALSE;

			GetDBT()->RemoveAll();
			return TRUE;
		case ACTIVITY_GRID_SUMMARY:
			if (!m_pDBTSummary)
				return FALSE;

			m_pDBTSummary->RemoveAll();
			return TRUE;
	}

	return FALSE;
}

//----------------------------------------------------------------------------------------------------
void CTBActivityDocument::SetPanelCollapsed(CTilePanel* pPanel, BOOL bSet)
{
	if (!pPanel)
		return;

	pPanel->SetCollapsed(bSet);
}

//------------------------------------------------------------------------------------------------------
void CTBActivityDocument::SetPanelEnabled(CTilePanel* pPanel, BOOL bSet)
{
	if (!pPanel)
		return;

	pPanel->SetActiveTabContentEnable(bSet);

	if (bSet)
		DispatchDisableControlsForBatch();

}

//----------------------------------------------------------------------------------------------------------
void CTBActivityDocument::ManagePanelsState(BOOL bEnabledAfterExtract)
{
	BOOL bEnableFilters = !bEnabledAfterExtract;
	BOOL bEnableActions = m_bActionsAlwaysEnabled || (m_bActionsAsFilters ? !bEnabledAfterExtract : bEnabledAfterExtract);
	BOOL bEnableFooter = m_bFooterAlwaysEnabled || bEnabledAfterExtract;

	EnsureExistancePanels();

	SetPanelEnabled(m_pPanelFilters,	bEnableFilters);
	SetPanelEnabled(m_pPanelActions,	bEnableActions);
	SetPanelEnabled(m_pPanelFooter,		bEnableFooter);
}

//--------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::BatchEnableControls()
{
	BOOL bOK = __super::BatchEnableControls();

	if (bOK && !m_bBatchRunning)
		EnableAllControlLinks(!m_bExtractData);

	return bOK;
}

//----------------------------------------------------------------------------------------------------------
CTilePanel* CTBActivityDocument::GetPanelFilters()
{
	EnsureExistancePanels();
	return m_pPanelFilters;
}

//----------------------------------------------------------------------------------------------------------
CTilePanel* CTBActivityDocument::GetPanelActions()
{
	EnsureExistancePanels();
	return m_pPanelActions;
}

//----------------------------------------------------------------------------------------------------------
CTilePanel* CTBActivityDocument::GetPanelFooter()
{
	EnsureExistancePanels();
	return m_pPanelFooter;
}

//--------------------------------------------------------------------------------------------------
void CTBActivityDocument::DoExtractData()
{
	EnsureExistancePanels();

	if (!OnFilterValidate() || !DispatchOnBeforeLoadDBT() || !DispatchOnLoadDBT() || !DispatchOnAfterLoadDBT())
	{
		m_pMessages->Show();
		ManagePanelsState(FALSE);
		m_bExtractData = FALSE;	//reset status
		return;
	}

	SetPanelCollapsed(m_pPanelFilters, m_eFiltersActionOnExtract == E_ACTIVITY_PANELACTION::ACTIVITY_COLLAPSE);
	SetPanelCollapsed(m_pPanelActions, m_bActionsAsFilters ? (m_eFiltersActionOnExtract == E_ACTIVITY_PANELACTION::ACTIVITY_COLLAPSE) : FALSE);
	SetPanelCollapsed(m_pPanelFooter, FALSE);

	UpdateBEButton(IDC_TB_ACTIVITY_DOCUMENT_GRID, ID_TB_ACTIVITY_DOCUMENT_GRID_SELECT_DESELECT, GetCaptionOnSelectButton(), GetCaptionOnSelectButton());

	m_bAddMoreData = TRUE;
	m_bExtractData = TRUE;

	//allow other actions from inherited document (added panel/s, tile/s ecc)
	ManageExtractData();

	ManagePanelsState(TRUE);

	//succede che si fa UpdateDataView lato gestionale prima che finisce il processo 
	//(prima di aggiornamento dello stato di extract per cui il flag modified del BE viene reset-ato)
	//e a questo punto salta la gestione corretta del bottone select
	if (GetDBT())
		GetDBT()->SetModified();

	UpdateDataView();
}

//---------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::DispatchOnBeforeLoadDBT()
{
	BOOL bOK = OnBeforeLoadDBT();

	return bOK && m_pClientDocs->OnBeforeLoadDBT();
}

//---------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::DispatchOnLoadDBT()
{
	BOOL bOK = OnLoadDBT();

	return bOK && m_pClientDocs->OnLoadDBT();
}

//----------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::DispatchOnAfterLoadDBT()
{
	BOOL bOK = OnAfterLoadDBT();

	return bOK && m_pClientDocs->OnAfterLoadDBT();
}

//-------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::DispatchOnBeforeUndoExtraction()
{
	BOOL bOK = BeforeUndoExtraction();
	return bOK && m_pClientDocs->OnBeforeUndoExtraction();
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnUpdateLoadDataStart(CCmdUI* pCmdUI)
{
	BOOL bEnable = DoUpdateLoadDataStart();

	pCmdUI->Enable(bEnable);
}

//----------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::DoUpdateLoadDataStart()
{
	// Se si � in editazione parameteri dello scheduler il bottone estrai � disattivo
	if (IsEditingParamsFromExternalController())
		return FALSE;		
	
	if (m_bExtractingData)
		return FALSE;

	return !m_bExtractData;
}

//---------------------------------------------------------------------------------------------
void CTBActivityDocument::OnLoadDataStart()
{
	// Se si � in editazione parameteri dello scheduler il bottone estrai � disattivo
	if (IsEditingParamsFromExternalController())
		return;

	if (GetNotValidView(TRUE))
		return;

	if (!m_bExtractData)
		LoadStart();
}

//---------------------------------------------------------------------------------------------
void CTBActivityDocument::LoadStart()
{
	m_bExtractingData = TRUE;

	if (!m_BatchRunningArea.IsLocked())
	{
		if (GetNotValidView(TRUE))
		{
			m_BatchRunningArea.Unlock();
			return;
		}

		m_BatchScheduler.Start();
		m_bExtractData = TRUE;
		DoExtractData();

		if (m_bExtractData)
		{
			m_BatchScheduler.Terminate();
			//m_bExtractData = FALSE;
			
			if (IsRunningFromExternalController())
			{
				m_pExternalControllerInfo->SetRunningTaskStatus(CExternalControllerInfo::TASK_SUCCESS);
				m_pExternalControllerInfo->m_Finished.Set();
			}
		}
		else
		{
			if (IsRunningFromExternalController())
			{
				m_pExternalControllerInfo->SetRunningTaskStatus(CExternalControllerInfo::TASK_USER_ABORT);
				m_pExternalControllerInfo->m_Finished.Set();
			}
		}

		UpdateDataView();

		m_BatchRunningArea.Unlock();
	}

	m_bExtractingData = FALSE;
}

//---------------------------------------------------------------------------------------------
void CTBActivityDocument::LoadStop()
{
	if
		(
			(IsRunningFromExternalController())
			||
			(IsInUnattendedMode())
			||
			AfxMessageBox(_TB("Loading...\r\nAre you sure you want to abort it?"), MB_YESNO) == IDYES
			)

	{
		m_BatchScheduler.Terminate();
		//m_bExtractData = FALSE;
		
		if (IsRunningFromExternalController())
		{
			m_pExternalControllerInfo->SetRunningTaskStatus(CExternalControllerInfo::TASK_USER_ABORT);
			m_pExternalControllerInfo->m_Finished.Set();
		}
		UpdateDataView();
	}
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnLoadPause()
{
	GetNotValidView(TRUE);

	if (!m_bExtractData)
		return;

	//if (m_BatchScheduler.IsPaused())
	//m_BatchScheduler.Resume();
	//else
	m_BatchScheduler.Pause();

	//SwitchPauseButtonState();

	UpdateDataView();
}

//--------------------------------------------------------------------------------------------------
void CTBActivityDocument::OnLoadResume()
{
	GetNotValidView(TRUE);

	if (!m_bExtractData)
		return;

	//if (m_BatchScheduler.IsPaused())
	m_BatchScheduler.Resume();
	//else
	//	m_BatchScheduler.Pause();

	//SwitchPauseButtonState();

	UpdateDataView();
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnLoadStop()
{
	if (IsEditingParamsFromExternalController())
		return;

	GetNotValidView(TRUE);

	if (m_bExtractData)
		LoadStop();

}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnUpdateLoadPause(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bExtractingData && !m_BatchScheduler.IsPaused());
}

//----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnUpdateLoadResume(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bExtractingData && m_BatchScheduler.IsPaused());
}

//------------------------------------------------------------------------------------------------
void CTBActivityDocument::OnUpdateLoadStop(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bExtractingData);
}

//--------------------------------------------------------------------------------------
CTilePanel* CTBActivityDocument::GetTilePanel(CString sName)
{
	CArray<CAbstractFormView*> arrViews;

	POSITION pos = GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = GetNextView(pos);
		ASSERT_VALID(pView);
		CAbstractFormView* pBaseView = dynamic_cast<CAbstractFormView*>(pView);
		if (!pBaseView)
			continue;
		arrViews.Add(pBaseView);
	}

	if (!arrViews.GetCount())
	{
		ASSERT(FALSE);
		return NULL;
	}

	for (int v = 0; v < arrViews.GetCount(); v++)
	{
		CAbstractFormView* pView = arrViews.GetAt(v);

		for (int g = 0; g < pView->m_pTileGroups->GetCount(); g++)
		{
			CBaseTileGroup* pGroup = pView->m_pTileGroups->GetAt(g);

			if (!pGroup)
				continue;

			for (int p = 0; p < pGroup->GetTilePanels()->GetCount(); p++)
			{
				CTilePanel* pPanel = pGroup->GetTilePanels()->GetAt(p);

				if (!pPanel)
					continue;

				CString name = pPanel->GetNamespace().GetRightTokens(1);

				if (!name.CompareNoCase(sName))
					return pPanel;
			}
		}
	}

	return NULL;
}

//------------------------------------------------------------------------------------
void CTBActivityDocument::AddSummaryString(const DataStr& aSummaryString)
{
	if (m_eResultsType != E_ACTIVITYTYPE::ACTIVITY_GRID_SUMMARY)
		return;

	if (!m_pDBTSummary)
		return;

	if (m_pDBTSummary->GetUpperBound() >= MAX_RECORD - 1)
		ClearSummary();

	TSummaryDetail* pRec = (TSummaryDetail*)m_pDBTSummary->AddRecord();

	if (!pRec)
		return;

	pRec->l_LineSummaryDescription = aSummaryString;

	m_pDBTSummary->SetCurrentRow(m_pDBTSummary->GetUpperBound());

}

//------------------------------------------------------------------------------------
void CTBActivityDocument::UpdateSummaryString(const DataStr& aSummaryString, int index /*= -1*/)
{
	if (m_eResultsType != E_ACTIVITYTYPE::ACTIVITY_GRID_SUMMARY)
		return;

	if (!m_pDBTSummary)
		return;

	TSummaryDetail* pRec = NULL;

	if (index == -1)
	{
		pRec = (TSummaryDetail*)m_pDBTSummary->GetCurrentRow();
		index = m_pDBTSummary->GetCurrentRowIdx(); // per allineamento della current row
	}
	else
		pRec = (TSummaryDetail*)m_pDBTSummary->GetRow(index);

	if (!pRec)
		return;

	pRec->l_LineSummaryDescription = aSummaryString;

	m_pDBTSummary->SetCurrentRow(index);

}

//---------------------------------------------------------------------------------------------
void CTBActivityDocument::ClearSummary()
{
	if (m_eResultsType != E_ACTIVITYTYPE::ACTIVITY_GRID_SUMMARY)
		return;

	if (!m_pDBTSummary)
		return;

	for (int i = RECORD_TO_DELETE; i >= 0; i--)
		m_pDBTSummary->DeleteRecord(i);
}

//---------------------------------------------------------------------------------------------
void CTBActivityDocument::RemoveAllSummary()
{
	if (m_eResultsType != E_ACTIVITYTYPE::ACTIVITY_GRID_SUMMARY)
		return;

	if (!m_pDBTSummary)
		return;

	m_pDBTSummary->RemoveAll();
}

//---------------------------------------------------------------------------------------
void CTBActivityDocument::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	UINT nIDC = (UINT)pCtrl->GetCtrlID();

	if (nIDC == IDC_TB_HEADER_TITLE || nIDC == IDC_TB_HEADER_SUBTITLE)
	{
		CStrStatic* pTitle = dynamic_cast<CStrStatic*>(pCtrl);

		if (!pTitle)
			return;

		pTitle->SetOwnFont(nIDC == IDC_TB_HEADER_TITLE ? m_bHeaderTitleBold : m_bHeaderSubTitleBold, FALSE, FALSE);
		pTitle->SetCustomDraw(FALSE);
		return;
	}

	if (m_eResultsType != E_ACTIVITYTYPE::ACTIVITY_PROGRESS)
		return;

	if (nIDC == IDC_TB_ACTIVITY_DOCUMENT_PROGRESS_STATIC)
	{
		m_pGaugeLabel = dynamic_cast<CStrStatic*>(pCtrl);

		if (!m_pGaugeLabel)
			return;

		m_pGaugeLabel->SetOwnFont(FALSE, FALSE, FALSE, 10);
		m_pGaugeLabel->SetCustomDraw(FALSE);
		m_pGaugeLabel->SetTextColor(AfxGetThemeManager()->GetTileDialogTitleForeColor());
	}

	if (nIDC == IDC_TB_ACTIVITY_DOCUMENT_PROGRESS)
	{
		if (!m_pGauge)
			m_pGauge = dynamic_cast<CTBLinearGaugeCtrl*>(pCtrl);

		if (!m_pGauge)
			return;

		m_pGauge->SetMajorTickMarkSize(0);
		m_pGauge->SetMinorTickMarkSize(0);
		m_pGauge->SetMajorTickMarkStep(0);
		m_pGauge->RemoveAllPointers();
		m_pGauge->SetTextLabelFormat(_T(""));
		m_pGauge->RemoveAllColoredRanges();
		m_pGauge->AddColoredRange(
			0,
			m_nCurrentElement,
			AfxGetThemeManager()->GetTileDialogTitleForeColor()
		);
		m_pGauge->SetGaugeRange(0, m_nGaugeRangeMax);
	}
}

//-----------------------------------------------------------------------------------------
void CTBActivityDocument::SetGaugeUpperRange(DataLng nMax /*= 100*/)
{
	if (!m_pGauge)
		return;

	m_nGaugeRangeMax = nMax;

	m_pGauge->SetGaugeRange(0, m_nGaugeRangeMax);
}

//------------------------------------------------------------------------------------
void CTBActivityDocument::UpdateGauge()
{
	if (!m_pGauge || m_nStep == 0 || m_nGaugeRangeMax == 0 || m_nProgressStep == 0)
		return;

	m_nCurrentElement += m_nProgressStep;
	
	if (m_nCurrentElement > m_nGaugeRangeMax)
	{
		ASSERT_TRACE(FALSE, "m_nCurrentElement > m_nGaugeRangeMax\n");
		return;
	}

	if (m_nCurrentElement == m_nGaugeRangeMax || ((long)m_nCurrentElement % m_nStep) == 0)
		m_pGauge->ModifyRange(0, 0, m_nCurrentElement);
	
}

//------------------------------------------------------------------------------------
void CTBActivityDocument::ClearGauge(const DataLng& aMax, int perc /*= 10*/)
{
	if (!m_pGauge)
		return;

	m_nCurrentElement.Clear();
	SetGaugeTitle(_T(""));
	m_pGauge->ModifyRange(0, 0, m_nCurrentElement);
	SetGaugeUpperRange(aMax);
	
	if (aMax % perc == 0)
		m_nStep = (long)((aMax * perc) / 100);
	else
		m_nStep = (long)((aMax * perc) / 100) + 1;
}

//----------------------------------------------------------------------------
void CTBActivityDocument::EnsureExistancePanels()
{
	if (!m_pPanelFilters)
		m_pPanelFilters = GetTilePanel(szFilters);

	if (!m_pPanelActions)
		m_pPanelActions = GetTilePanel(szActions);

	if (!m_pPanelFooter)
		m_pPanelFooter = GetTilePanel(szFooter);
}

//----------------------------------------------------------------------------
void CTBActivityDocument::SetGaugeTitle(CString title)
{
	m_GaugeDescription = title;
	if (m_pGaugeLabel)
		m_pGaugeLabel->UpdateCtrlView();
}

//-------------------------------------------------------------------------------------------------
void CTBActivityDocument::DoAddData()
{
	EnsureExistancePanels();

	m_bExtractData = TRUE;
	
	SetPanelCollapsed(m_pPanelFilters, FALSE);
	SetPanelCollapsed(m_pPanelActions, m_eFooterActionOnExtract == E_ACTIVITY_PANELACTION::ACTIVITY_COLLAPSE);
	SetPanelCollapsed(m_pPanelFooter, m_bActionsAsFilters ? FALSE : m_eFiltersActionOnExtract == E_ACTIVITY_PANELACTION::ACTIVITY_COLLAPSE);
	
	m_bAddMoreData = FALSE;
	m_bExtractData = TRUE;

	//manage your panel/s, tiles/s from outside
	ManageAddData();

	ManagePanelsState(FALSE);

	UpdateDataView();
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnAddData()
{
	GetNotValidView();

	DoAddData();
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnUpdateAddData(CCmdUI* pCmdUI)
{
	BOOL bEnable = DoUpdateAddData();

	pCmdUI->Enable(bEnable);
}

//----------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::DoUpdateAddData()
{
	return m_bAddMoreData;
}

//-------------------------------------------------------------------------------------------------
void CTBActivityDocument::Restart()
{
	BOOL bOldVal = m_bAskDeleteConfirmation;
	m_bAskDeleteConfirmation = FALSE;
	DoUndoExtraction();
	m_bAskDeleteConfirmation = bOldVal;
}

//-------------------------------------------------------------------------------------------------
void CTBActivityDocument::DoUndoExtraction()
{
	EnsureExistancePanels();

	if (!DispatchOnBeforeUndoExtraction())
		return;

	if (!OnClearDBT())
	{
		SetPanelCollapsed(m_pPanelFilters, m_eFiltersActionOnExtract == E_ACTIVITY_PANELACTION::ACTIVITY_COLLAPSE);
		SetPanelCollapsed(m_pPanelActions, m_bActionsAsFilters ? (m_eFiltersActionOnExtract == E_ACTIVITY_PANELACTION::ACTIVITY_COLLAPSE) : FALSE);
		SetPanelCollapsed(m_pPanelFooter, FALSE);
		ManagePanelsState(TRUE);
		UpdateDataView();
		return;
	}

	m_bExtractData = FALSE;
	m_bSelect = TRUE;		//reset Select button
	UpdateBEButton(IDC_TB_ACTIVITY_DOCUMENT_GRID, ID_TB_ACTIVITY_DOCUMENT_GRID_SELECT_DESELECT, GetCaptionOnSelectButton(), GetCaptionOnSelectButton());
	
	SetPanelCollapsed(m_pPanelFilters, FALSE);
	SetPanelCollapsed(m_pPanelActions, m_bActionsAsFilters ? FALSE : ((m_eActionsActionOnExtract == E_ACTIVITY_PANELACTION::ACTIVITY_COLLAPSE)));
	SetPanelCollapsed(m_pPanelFooter, (m_eFooterActionOnExtract == E_ACTIVITY_PANELACTION::ACTIVITY_COLLAPSE));
	
	m_bAddMoreData = FALSE;

	//allow other actions from inherited document (added panel/s, tile/s ecc)
	//FireAction(_T("ActivityDocumentUndoExtraction"));	//di persano per BSP ??????
	ManageUndoExtraction();

	ManagePanelsState(FALSE);

	UpdateDataView();

}

//------------------------------------------------------------------------------------------------------
void CTBActivityDocument::UpdateBEButton(UINT idBody, UINT idButton, CString text, CString tooltip)
{
	CBEButton* pButton = GetBEButton(idBody, idButton);

	if (pButton)
	{
		pButton->SetText(text);
		pButton->SetTooltip(tooltip);
	}
}

//---------------------------------------------------------------------------------------------------
void CTBActivityDocument::OnSelectDeselectClicked()
{
	m_bSelect = !m_bSelect;

	OnSelectDeselect();

	UpdateBEButton(IDC_TB_ACTIVITY_DOCUMENT_GRID, ID_TB_ACTIVITY_DOCUMENT_GRID_SELECT_DESELECT, GetCaptionOnSelectButton(), GetCaptionOnSelectButton());
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnUndoExtraction()
{
	GetNotValidView();

	DoUndoExtraction();
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnUpdateUndoExtraction(CCmdUI* pCmdUI)
{
	BOOL bEnable = DoUpdateUndoExtraction();

	pCmdUI->Enable(bEnable);
}

//---------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::DoUpdateUndoExtraction()
{
	return m_bExtractData;
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnUpdateBatchStartStop(CCmdUI* pCmdUI)
{
	BOOL bEnable = DoUpdateBatchStartStop();

	pCmdUI->Enable(bEnable);

}

//----------------------------------------------------------------------------------------------------
BOOL CTBActivityDocument::DoUpdateBatchStartStop()
{
	return (IsEditingParamsFromExternalController() || CanDoBatchExecute());
}

//-----------------------------------------------------------------------------------------------
void CTBActivityDocument::OnUpdateBatchPauseResume(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_bBatchRunning);
}











