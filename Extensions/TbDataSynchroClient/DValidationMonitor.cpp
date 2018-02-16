#include "stdafx.h"

#include <TbFrameworkImages\GeneralFunctions.h>
#include <TbFrameworkImages\CommonImages.h>
#include <TbGenlib\TBLinearGauge.h>
#include <TbOleDb\RIChecker.h>

#include "DSComponents.h"
#include "DValidationMonitor.h"
#include "DSManager.h"
#include "ValidationMonitorSettings.h"
#include "UIValidationMonitor.h"
#include "UIDSMonitor.hjson"
#include "UIValidationMonitor.hjson"
#include "UIDSMonitor.hjson"
	
static TCHAR szParamProvider		 [] = _T("ParamProvider");
static TCHAR szParamDocNS			 [] = _T("ParamDocNamespace");
static TCHAR szAllDate				 [] = _T("ParamAllDate");
static TCHAR szFromDate				 [] = _T("ParamFromDate");
static TCHAR szToDate				 [] = _T("ParamToDate");
static TCHAR szParamMsgError		 [] = _T("szParamMsgError");
static TCHAR szID					 [] = _T("ID");

//==============================================================================
const TCHAR		   szTbDataSynchroClient[] = _T("Module.Extensions.TbDataSynchroClient");
const CTBNamespace snsTbDataSynchroClient   = szTbDataSynchroClient;

//==============================================================================
const TCHAR szDataSynchronizerMonitor[] = _T("DataSynchronizerMonitor");
const TCHAR szRefreshTimer			 [] = _T("RefreshTimer");

static TCHAR szNamespace[] = _T("Image.Framework.TbFrameworkImages.Images.%s.%s.png");
static TCHAR szGlyphF[] = _T("Glyph");

///////////////////////////////////////////////////////////////////////////////
//             class TEnhDS_ValidationInfoDetail implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(TEnhDS_ValidationInfoDetail, TDS_ValidationInfo)

//-----------------------------------------------------------------------------
TEnhDS_ValidationInfoDetail::TEnhDS_ValidationInfoDetail(BOOL bCallInit)
{
	BindRecord();	
	if (bCallInit) 
		Init(); 
}

//-----------------------------------------------------------------------------
void TEnhDS_ValidationInfoDetail::BindRecord()
{
	BEGIN_BIND_DATA	();
		LOCAL_STR	(_NS_LFLD("TEnhDS_Valid_Code"),				 l_Code,				21);
		LOCAL_STR	(_NS_LFLD("TEnhDS_Valid_Description"),		 l_Description,			128);
		LOCAL_STR	(_NS_LFLD("TEnhDS_Valid_FormattedMsgError"), l_FormattedMsgError,	512);
		LOCAL_STR	(_NS_LFLD("TEnhDS_WorkerDescri"),			 l_WorkerDescri,		64);
	END_BIND_DATA();
}

///////////////////////////////////////////////////////////////////////////////
//             class TEnhDS_ValidationInfoSummary implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(TEnhDS_ValidationInfoSummary, TDS_ValidationInfo)

//-----------------------------------------------------------------------------
TEnhDS_ValidationInfoSummary::TEnhDS_ValidationInfoSummary(BOOL bCallInit)
{
	BindRecord();	
	if (bCallInit) 
		Init(); 
}

//-----------------------------------------------------------------------------
void TEnhDS_ValidationInfoSummary::BindRecord()
{
	BEGIN_BIND_DATA	();
		LOCAL_STR	(_NS_LFLD("NoErrors"),	l_NoErrors,	10);
	END_BIND_DATA();
}

///////////////////////////////////////////////////////////////////////////////
//             class TEnhDS_ValidationFKToFixSummary implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(TEnhDS_ValidationFKToFixSummary, TDS_ValidationFKtoFix)

//-----------------------------------------------------------------------------
TEnhDS_ValidationFKToFixSummary::TEnhDS_ValidationFKToFixSummary(BOOL bCallInit)
{
	BindRecord();	
	if (bCallInit) 
		Init(); 
}

//-----------------------------------------------------------------------------
void TEnhDS_ValidationFKToFixSummary::BindRecord()
{
	BEGIN_BIND_DATA	();
		LOCAL_STR	(_NS_LFLD("TEnhDS_Valid_NoFKToFix"),	l_NoFKToFix,	10);
	END_BIND_DATA();
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTValidationInfoMonitor implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DBTValidationInfoMonitor, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTValidationInfoMonitor::DBTValidationInfoMonitor
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("ValidationInfoMonitor"), TRUE)
{
}

//-----------------------------------------------------------------------------
void DBTValidationInfoMonitor::OnDefineQuery()
{
	m_pTable->SelectAll();

	//se arrivo dalla batch
	//select * from DS_ValidationInfo 
	//where ProviderName = ? AND 
	//      DocNamespace = ? 

	//se invece non arrivo dalla batch
	//select * from DS_ValidationInfo 
	//where ProviderName = ?		 AND 
	//      DocNamespace = ?		 AND 
	//      (1=? OR (ValidationDate >= ? AND ValidationDate <= ?))
	
	DValidationMonitor* pDoc = GetDocument();
	TDS_ValidationInfo* pRec = GetValidationInfoRecord();
	
	if (pDoc->m_bFromBatch)
	{
		m_pTable->m_strFilter	= cwsprintf(_T("%s = ? AND %s = ? AND %s NOT LIKE ?"), TDS_ValidationInfo::szProviderName, TDS_ValidationInfo::szDocNamespace, TDS_ValidationInfo::szMessageError);

		m_pTable->AddParam	(szParamProvider,		pRec->f_ProviderName);
		m_pTable->AddParam	(szParamDocNS,			pRec->f_DocNamespace);
		m_pTable->AddParam	(szParamMsgError,		pRec->f_MessageError);
	}
	else
	{
		m_pTable->m_strFilter	 = cwsprintf(		_T("%s = ? AND %s = ? AND %s NOT LIKE ? AND ('1' = ? OR (%s >= ? AND %s <= ?))"),
													TDS_ValidationInfo::szProviderName,
													TDS_ValidationInfo::szDocNamespace,
													TDS_ValidationInfo::szMessageError,
													TDS_ValidationInfo::szValidationDate,
													TDS_ValidationInfo::szValidationDate
											);

		m_pTable->AddParam	(szParamProvider,		pRec->f_ProviderName);
		m_pTable->AddParam	(szParamDocNS,			pRec->f_DocNamespace);	
		m_pTable->AddParam	(szParamMsgError,		pRec->f_MessageError);	
		m_pTable->AddParam	(szAllDate,				DATA_BOOL_TYPE, 1);
		m_pTable->AddParam	(szFromDate,			pRec->f_ValidationDate);
		m_pTable->AddParam	(szToDate,				pRec->f_ValidationDate);
	}

	m_pTable->AddSortColumn	(pRec->f_ValidationDate, TRUE);
	m_pTable->AddSortColumn	(pRec->f_ProviderName);
}

//-----------------------------------------------------------------------------
void DBTValidationInfoMonitor::OnPrepareQuery()
{	
	DValidationMonitor* pDoc = GetDocument();

	if (GetDocument()->m_bFromBatch) 
	{
		m_pTable->SetParamValue(szParamProvider,	pDoc->m_ProviderName);
		m_pTable->SetParamValue(szParamDocNS,		pDoc->m_DocNamespace);
		m_pTable->SetParamValue(szParamMsgError,	DataText(_T("")));	
	}
	else
	{
		m_pTable->SetParamValue(szParamProvider,	pDoc->m_ProviderName);
		m_pTable->SetParamValue(szParamDocNS,		pDoc->m_DocNamespace);
		m_pTable->SetParamValue(szParamMsgError,	DataText(_T("")));	
		m_pTable->SetParamValue(szAllDate,			pDoc->m_bAllDate);
		m_pTable->SetParamValue(szFromDate,			DataDate(pDoc->m_DateFrom.Day(),	pDoc->m_DateFrom.Month(),	pDoc->m_DateFrom.Year(), 0, 0, 0));
		m_pTable->SetParamValue(szToDate,			DataDate(pDoc->m_DateTo.Day(),		pDoc->m_DateTo.Month(),		pDoc->m_DateTo.Year(), 23, 59, 59));
	}
}

//-----------------------------------------------------------------------------
void DBTValidationInfoMonitor::ReloadData()
{
	RemoveAll();
	LocalFindData(FALSE);
}

//-----------------------------------------------------------------------------
void DBTValidationInfoMonitor:: OnPrepareAuxColumns(SqlRecord* pRecord)
{
	if (!pRecord)
		return;

	TEnhDS_ValidationInfoDetail* pEnhRec = (TEnhDS_ValidationInfoDetail*)pRecord;
	GetDocument()->GetDecodingInfo(pEnhRec->f_DocTBGuid, pEnhRec->l_Code, pEnhRec->l_Description);

	CWorker* pWorker = pRecord->f_TBModifiedID >= 0 ? AfxGetWorkersTable()->GetWorker(pRecord->f_TBModifiedID) : NULL;
	pEnhRec->l_WorkerDescri = (pWorker) ? cwsprintf(_T("%s %s"), pWorker->GetName(), pWorker->GetLastName()) : cwsprintf(_T("%s"), pRecord->f_TBModifiedID.Str());

	RICheckNode* pRoot = AfxGetApplicationContext()->GetObject<RICheckNode>();
	pEnhRec->l_FormattedMsgError = pRoot->DisplayErrors(pEnhRec->f_MessageError);
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTValidationMonitorDocSummary implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DBTValidationMonitorDocSummary, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTValidationMonitorDocSummary::DBTValidationMonitorDocSummary
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("ValidationMonitorDocSummary"), TRUE)
{
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTValidationFKToFixDetail implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DBTValidationFKToFixDetail, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTValidationFKToFixDetail::DBTValidationFKToFixDetail
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("ValidationFKToFixDetail"), TRUE)
{
}

//-----------------------------------------------------------------------------
void DBTValidationFKToFixDetail::OnDefineQuery()
{
	//se arrivo dalla batch
	//select * from DS_ValidationFKToFix
	//where ProviderName = ? AND 
	//      DocNamespace = ? 

	TDS_ValidationFKtoFix*	pRec = GetValidationFKtoFixRecord();

	m_pTable->SelectAll();

	m_pTable->m_strFilter	= cwsprintf(_T("%s = ? AND %s = ?"), TDS_ValidationFKtoFix::szProviderName, TDS_ValidationFKtoFix::szDocNamespace);

	m_pTable->AddParam(szParamProvider,	pRec->f_ProviderName);
	m_pTable->AddParam(szParamDocNS,	pRec->f_DocNamespace);
	
	m_pTable->AddSortColumn	(pRec->f_ValidationDate, TRUE);
	m_pTable->AddSortColumn	(pRec->f_ProviderName);
}

//-----------------------------------------------------------------------------
void DBTValidationFKToFixDetail::OnPrepareQuery()
{	
	DValidationMonitor*		pDoc = GetDocument();

	m_pTable->SetParamValue(szParamProvider,	pDoc->m_ProviderName);
	m_pTable->SetParamValue(szParamDocNS,		pDoc->m_DocNamespaceFKToFix);
}

//-----------------------------------------------------------------------------
void DBTValidationFKToFixDetail::ReloadData()
{
	RemoveAll();
	LocalFindData(FALSE);
}

///////////////////////////////////////////////////////////////////////////////
//             class DBTValidationFKToFixSummary implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC(DBTValidationFKToFixSummary, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTValidationFKToFixSummary::DBTValidationFKToFixSummary
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("ValidationFKToFixSummary"), TRUE)
{
}

//////////////////////////////////////////////////////////////////////////////
//									DValidationMonitor	                    //
//////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(DValidationMonitor, CAbstractFormDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DValidationMonitor, CAbstractFormDoc)
	//{{AFX_MSG_MAP(DValidationMonitor)
		ON_CONTROL				(BEN_ROW_CHANGED, IDC_DATAVALIDATION_SUMMARY_BE,			OnMonitorSummaryRowChanged)
		ON_EN_VALUE_CHANGED		(IDC_DATAVALIDATION_NAMESPACE_COMBO,						OnDocNamespaceChanged)
		ON_EN_VALUE_CHANGED		(IDC_DATAVALIDATION_MONITOR_ALL_DATE,						OnSelectionDateChanged)
		ON_EN_VALUE_CHANGED		(IDC_DATAVALIDATION_MONITOR_SEL_DATE,						OnSelectionDateChanged)
		ON_EN_VALUE_CHANGED		(IDC_DATAVALIDATION_MONITOR_FROM_DATE,						OnDateFromChanged)
		ON_EN_VALUE_CHANGED		(IDC_DATAVALIDATION_MONITOR_TO_DATE,						OnDateToChanged)
		ON_COMMAND				(ID_DS_MONITOR_REFRESH,										OnMonitorRefresh)
		ON_UPDATE_COMMAND_UI	(ID_DS_MONITOR_REFRESH,										OnMonitorRefreshUpdate)
		ON_CONTROL				(BEN_ROW_CHANGED, IDC_DATAVALIDATION_SUMMARY_BE,			OnValidationMonitorSummaryRowChanged)
		ON_CONTROL				(BEN_ROW_CHANGED, IDC_DATAVALIDATION_FK_TO_FIX_SUMMARY_BE,	OnValidationFKToFixSummaryRowChanged)
		ON_COMMAND				(ID_VALIDATION_MONITOR_OPEN_DOCUMENT,						OnValidationMonitorDetailOpenDocument)
		ON_UPDATE_COMMAND_UI	(ID_VALIDATION_MONITOR_OPEN_DOCUMENT,						OnUpdateDetailOpenDocument)
		ON_COMMAND				(ID_VALIDATION_MONITOR_COPY_MSG,							OnValidationMonitorDetailCopyMessage)	
		ON_UPDATE_COMMAND_UI	(ID_VALIDATION_MONITOR_COPY_MSG,							OnUpdateDetailCopyMessage)
		ON_COMMAND				(ID_VALIDATION_MONITOR_FK_TO_FIX_COPY,						OnValidationFKToFixDetailCopyValue)	
		ON_UPDATE_COMMAND_UI	(ID_VALIDATION_MONITOR_FK_TO_FIX_COPY,						OnUpdateFKToFixDetailCopyValue)
		ON_COMMAND				(ID_VALIDATION_MONITOR_FK_TO_FIX_ADD,						OnValidationFKToFixAdd)
		ON_UPDATE_COMMAND_UI	(ID_VALIDATION_MONITOR_FK_TO_FIX_ADD,						OnUpdateValidationFKToFixAdd)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
DValidationMonitor::DValidationMonitor()
	:
	m_pDBTDetail				(NULL),
	m_pDBTDetailDocSummary		(NULL),
	m_pDBTFKToFixDetail			(NULL),
	m_pDBTFKToFixSummary		(NULL),
	m_pMassiveSynchroInfo		(NULL),
	m_pDocumentsToMonitor		(NULL),
	m_pGaugeLabel				(NULL),
	m_pMonitorTile				(NULL),
	m_pSummaryTile				(NULL),
	m_pResultsTilePanel			(NULL),
	m_pGauge					(NULL),
	m_GaugeRefresh				(300),
	m_nValueGauge				(0.0),
	m_nGaugeUpperRange			(100.0),
	m_bFromBatch				(FALSE),
	m_bAllDate					(TRUE),
	m_bSelectionDate			(FALSE),
	m_bErrorsOccurred			(FALSE),
	m_bAutoRefresh				(TRUE),
	m_pViewFKToFixPanel			(NULL),
	m_bNeedMassiveValidation	(FALSE),
	m_bValidationEnded			(FALSE),
	m_nValidationCounter		(3),
	m_bValidationStarted		(FALSE),
	m_bIsMassiveValidating		(TRUE)
{	
	m_bBatch = TRUE;

	m_pDocumentsToMonitor = new CSynchroDocInfoArray();
	
	m_DateFrom	.SetFullDate();
	m_DateTo	.SetFullDate();
	
	m_bIsActivatedCRMInfinity = AfxIsActivated(MAGONET_APP, CRMINFINITY_FUNCTIONALITY);

	m_IDTileSummary		= IDD_DATAVALIDATION_MONITOR_SUMMARY;
	m_IDTileDetail		= IDD_DATAVALIDATION_MONITOR_DETAIL;

	//refresh monitor
	int monitorSetting = (*((DataInt*)AfxGetSettingValue(snsTbDataSynchroClient, szDataSynchronizerMonitor, szRefreshTimer, DataInt(10))));
	// Per trasformarlo in secondi va' moltiplicato per 1000 (cosi vuole il SetTimer())
	m_MonitorRefresh = monitorSetting * 1000;
}

//-----------------------------------------------------------------------------
DValidationMonitor::~DValidationMonitor()
{	
	SAFE_DELETE(m_pDocumentsToMonitor);
}

//-----------------------------------------------------------------------------
BOOL DValidationMonitor::CanRunDocument()
{    
	if (
		!AfxIsActivated(TBEXT_APP, _NS_ACT("DataSynchroFunctionality")) ||
		!AfxGetLoginInfos()->m_bDataSynchro
		)
	{
		if (!IsInUnattendedMode())
			AfxMessageBox(_TB("The option \"Use exchange data connector\" is not checked.Please enable it in the Administration Console(Company Properties)"), MB_ICONINFORMATION);
		return FALSE;
	}
	
	if (!AfxGetIDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		if (!IsInUnattendedMode())
			AfxMessageBox(_TB("The exchange data connector engine is not installed. Please install it by using its setup. Refer to your System Administrator."), MB_ICONINFORMATION);

		return FALSE;
	}
	

	if (!AfxDataSynchronizeEnabled())
	{
		if (!IsInUnattendedMode())
			AfxMessageBox(_TB("The information for exchange data connector provider is not valid. Please check the synchronization provider data in TaskBuilder Framework\\Synchronization data\\Providers."));
		
		return FALSE;
	}

	CSynchroProvider* pSynchroProvider = NULL;
	BOOL canRunDocuments=FALSE;

	if (m_ProviderName.IsEmpty())
	{
		for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
		{	
			pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i);
			if (pSynchroProvider->IsValid())
			{
				canRunDocuments = TRUE;
				break;
			}
		}
	}

	if	(canRunDocuments==FALSE) 
	{
		if (!IsInUnattendedMode())
			AfxMessageBox(_TB("Connection data not found! Set Synchronization Provider information in Environment\\Data Synchronization\\Providers."));
		
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DValidationMonitor::InitDocument()
{
	m_DateFrom	.Clear();
	m_DateTo	.Clear();

	return CAbstractFormDoc::InitDocument();
}

//-------------------------------------------------------------------------------------
void DValidationMonitor::InitGauge()
{
	if (!m_pGauge)
		return;

	m_nValueGauge.Clear();
	m_pGauge->SetGaugeRange(0, m_nGaugeUpperRange);
	m_pGauge->RemoveAllColoredRanges();
	m_pGauge->UpdateCtrlView();
}

//-------------------------------------------------------------------------------------------
void DValidationMonitor::CompleteGauge()
{
	if (!m_pGauge)
		return;

	m_nValueGauge = m_nGaugeUpperRange;
	m_pGauge->SetGaugeRange(0, m_nGaugeUpperRange);

	m_pGauge->AddColoredRange(
		0,
		m_nValueGauge,
		AfxGetTileDialogStyleNormal()->GetTitleSeparatorColor()
	);
	m_pGauge->UpdateCtrlView();
}

//-------------------------------------------------------------------------------------------
void DValidationMonitor::UpdateGauge(double nStep /*= 1.0*/)
{
	UpdateGauge(nStep, FALSE);
}

//-------------------------------------------------------------------------------------------
void DValidationMonitor::UpdateGauge(double nStep /*= 1.0*/, BOOL bIsIndeterminate /* = FALSE*/)
{
	if (!m_pGauge)
		return;

	m_nValueGauge += nStep;

	if (!bIsIndeterminate && m_nValueGauge >= m_nGaugeUpperRange)
		m_nValueGauge = m_nValueGauge / 2;

	m_pGauge->RemoveAllColoredRanges();
	m_pGauge->AddColoredRange(
		0,
		m_nValueGauge, 
		AfxGetTileDialogStyleNormal()->GetTitleSeparatorColor()
	);
	m_pGauge->SetGaugeRange(0, m_nGaugeUpperRange);
	m_pGauge->UpdateCtrlView();
}

//-------------------------------------------------------------------------------------------
void DValidationMonitor::UpdateGaugeIndeterminate()
{
	if (m_nValueGauge >= m_nGaugeUpperRange)
		m_nValueGauge = 0;

	UpdateGauge(1.0, TRUE);
}

//--------------------------------------------------------------------------------
void DValidationMonitor::ManageJsonVars()
{
	DECLARE_VAR_JSON(ProviderName);
	DECLARE_VAR_JSON(DocNamespace);
	DECLARE_VAR_JSON(bAutoRefresh);
	DECLARE_VAR_JSON(bIsActivatedCRMInfinity);
	DECLARE_VAR_JSON(bAllDate);
	DECLARE_VAR_JSON(bSelectionDate);
	DECLARE_VAR_JSON(DateFrom);
	DECLARE_VAR_JSON(DateTo);
	//manage gauge status
	DECLARE_VAR_JSON(nValueGauge);
	DECLARE_VAR_JSON(GaugeDescription);
	DECLARE_VAR_JSON(PictureStatus);
	DECLARE_VAR_JSON(bFromBatch);
	RegisterControl(IDC_DS_MONITOR_BE,				RUNTIME_CLASS(CValidationMonitorDetailBodyEdit));
	RegisterControl(IDC_DATAVALIDATION_SUMMARY_BE,	RUNTIME_CLASS(CValidationSummaryDetailBodyEdit));	
}

//-----------------------------------------------------------------------------
BOOL DValidationMonitor::OnAttachData()
{
	SetFormTitle(_TB("Validation Monitor"));
	SetDocAccel(IDR_DATAVALIDATION_MONITOR);

	//manage json variables
	ManageJsonVars();
	
	m_pDBTDetail = new DBTValidationInfoMonitor(RUNTIME_CLASS(TEnhDS_ValidationInfoDetail),	this); 

	m_pViewFKToFixPanel = (CValidationMonitorFKToFixPanel*)GetMasterFrame()->CreateDockingPane
	(
		RUNTIME_CLASS(CValidationMonitorFKToFixPanel),
		IDD_DATAVALIDATION_MONITOR_FK_TO_FIX,
		_TB("Errors to Fix"),
		_TB("Errors to Fix"),
		CBRS_RIGHT,
		CSize(700, 300)
	);

	if (m_pViewFKToFixPanel)
		m_pViewFKToFixPanel->SetAutoHideMode(TRUE, CBRS_RIGHT | CBRS_HIDE_INPLACE, 0, TRUE);


	if (m_pMassiveSynchroInfo) // Ci entro solo se arrivo da batch
	{
		ValidationMonitorSettings aSettings;
		aSettings.SetNeedMassiveValidation(FALSE);
		aSettings.WriteParameters();

		m_ProviderName = m_pMassiveSynchroInfo->m_ProviderName;	

		if (m_pMassiveSynchroInfo->m_pDocumentsToSynch && m_pMassiveSynchroInfo->m_pDocumentsToSynch->GetSize() > 0)
		{
			for (int i = 0; i < m_pMassiveSynchroInfo->m_pDocumentsToSynch->GetSize(); i++)
			{
				CSynchroDocInfo* pSynchroDocInfo = new CSynchroDocInfo(m_pMassiveSynchroInfo->m_pDocumentsToSynch->GetAt(i));
				pSynchroDocInfo->SetDecodingInfo(GetReadOnlySqlSession());
				if (pSynchroDocInfo->IsValid())
					m_pDocumentsToMonitor->Add(pSynchroDocInfo);		
				else 
					delete pSynchroDocInfo;
			}			
		
			m_DocNamespace = (m_pDocumentsToMonitor->GetSize() > 0) ? m_pDocumentsToMonitor->GetAt(0)->m_strDocNamespace : _T("");
		}
	}
	
	CSynchroProvider* pSynchroProvider = NULL;

	if (m_ProviderName.IsEmpty())
	{
		for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
		{	
			pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i);
			if (pSynchroProvider->IsValid())
			{
				m_ProviderName = pSynchroProvider->m_Name;
				break;
			}
		}
	}
	
	pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(m_ProviderName);
	
	if (m_pDocumentsToMonitor->GetSize() ==  0 && pSynchroProvider)
	{
		for (int i = 0; i < pSynchroProvider->GetDocumentsToSynch()->GetSize(); i++)
		{
			CSynchroDocInfo* pSynchroDocInfo = new CSynchroDocInfo(pSynchroProvider->GetDocumentsToSynch()->GetAt(i)->m_strDocNamespace);
			pSynchroDocInfo->SetDecodingInfo(GetReadOnlySqlSession());
			if (pSynchroDocInfo->IsValid())
				m_pDocumentsToMonitor->Add(pSynchroDocInfo);	
			else
				delete pSynchroDocInfo;
		}
	}

	if (m_DocNamespace.IsEmpty() && m_pDocumentsToMonitor->GetSize() > 0)
		m_DocNamespace = m_pDocumentsToMonitor->GetAt(0)->m_strDocNamespace;

	m_pDBTDetail->Open();

	if (!m_bFromBatch)
		m_pDBTDetail->ReloadData();
	
	if (m_bIsActivatedCRMInfinity)
	{
		m_pDBTDetailDocSummary = new DBTValidationMonitorDocSummary(RUNTIME_CLASS(TEnhDS_ValidationInfoSummary), this); 
		
		m_pDBTFKToFixDetail = new DBTValidationFKToFixDetail(RUNTIME_CLASS(TDS_ValidationFKtoFix), this); 
		m_pDBTFKToFixDetail->Open();
		m_pDBTFKToFixDetail->ReloadData();
				
		m_pDBTFKToFixSummary = new DBTValidationFKToFixSummary(RUNTIME_CLASS(TEnhDS_ValidationFKToFixSummary), this); 

		m_bIsMassiveValidating = m_bFromBatch;
		m_bValidationStarted   = m_bFromBatch;

		DoGaugeManagement();	

		ValidationDocSummaryManagement();	

		ValidationMonitorSettings aSettings;
		m_bNeedMassiveValidation = !m_bFromBatch && aSettings.GetNeedMassiveValidation();
	}

	StartGaugeTimer();
	InitGauge();
	
	StartTimer();

	return TRUE;
}

//----------------------------------------------------------------------------------
void DValidationMonitor::OnCloseDocument()
{
	EndTimer();

	__super::OnCloseDocument();
}

//----------------------------------------------------------------------------------
BOOL DValidationMonitor::OnOpenDocument(LPCTSTR pParam)
{
	if (pParam)
	{
		m_bFromBatch = TRUE;
		m_bAutoRefresh = TRUE;

		m_pMassiveSynchroInfo = (MassiveSynchroInfo*) GET_AUXINFO(pParam);
	}
	
	SetFiltersEnable(!m_bFromBatch);
	return CAbstractFormDoc::OnOpenDocument(pParam);
}

//---------------------------------------------------------------------------------------------
void DValidationMonitor::StartTimer()
{
	if (m_MonitorRefresh > 0.0)
		GetFirstView()->SetTimer(CHECK_VALIDATION_MONITOR_TIMER, (UINT)m_MonitorRefresh, NULL);
}

//-----------------------------------------------------------------------------------------------
void DValidationMonitor::EndTimer()
{
	if (GetFirstView())
		GetFirstView()->KillTimer(CHECK_VALIDATION_MONITOR_TIMER);
}

//---------------------------------------------------------------------------------------------
void DValidationMonitor::StartGaugeTimer()
{
	GetFirstView()->SetTimer(CHECK_VALIDATION_GAUGE_TIMER, (UINT)m_MonitorRefresh, NULL);
}

//-----------------------------------------------------------------------------------------------
void DValidationMonitor::EndGaugeTimer()
{
	GetFirstView()->KillTimer(CHECK_VALIDATION_GAUGE_TIMER);
}

//------------------------------------------------------------------------------
void DValidationMonitor::DoOnTimer()
{
	if (m_bAutoRefresh)
	{
		DoRefresh();
		UpdateDataView();
	}
}

//------------------------------------------------------------------------------
void DValidationMonitor::DoOnGaugeTimer()
{
	DoGaugeRefresh();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnParsedControlCreated(CParsedCtrl* pCtrl)
{
	UINT nIDC = (UINT)pCtrl->GetCtrlID();

	if (nIDC == IDC_DATASYNCHRO_MONITOR_STATUS_DESCRIPTION)
	{
		m_pGaugeLabel = dynamic_cast<CLabelStatic*>(pCtrl);

		if (!m_pGaugeLabel)
			return;

		m_pGaugeLabel->SetOwnFont(FALSE, FALSE, FALSE, 10);
		m_pGaugeLabel->SetTextColor(AfxGetThemeManager()->GetTileDialogTitleForeColor());
		m_pGaugeLabel->SetBkgColor(AfxGetTileDialogStyleWizard()->GetBackgroundColor());
		m_pGaugeLabel->SetValue(m_GaugeDescription);
	}
	else if (nIDC == IDC_DATASYNCHRO_MONITOR_STATUS_GAUGE)
	{
		m_pGauge = dynamic_cast<CTBLinearGaugeCtrl*>(pCtrl);

		if (!m_pGauge)
			return;

		m_pGauge->SetMajorTickMarkSize(0);
		m_pGauge->SetMinorTickMarkSize(0);
		m_pGauge->SetMajorTickMarkStep(0);
		m_pGauge->RemovePointer(0);
		m_pGauge->SetBkgColor(AfxGetTileDialogStyleWizard()->GetBackgroundColor());
		m_pGauge->SetTextLabelFormat(_T(""));
		m_pGauge->RemoveAllColoredRanges();
		m_pGauge->SetGaugeRange(0, m_nGaugeUpperRange);
	}
	else if (pCtrl->GetCtrlID() == IDC_RW_DATAVALIDATION_MONITOR_DETAIL_MSG)
	{
		pCtrl->GetCtrlCWnd()->SendMessage(EM_SETREADONLY, TRUE, NULL);
	}
}

//-----------------------------------------------------------------------------
void DValidationMonitor::DeleteContents()
{
	SAFE_DELETE(m_pDBTDetail);
	SAFE_DELETE(m_pDBTDetailDocSummary);
	SAFE_DELETE(m_pDBTFKToFixDetail);
	SAFE_DELETE(m_pDBTFKToFixSummary);

	__super::DeleteContents();
}

//-----------------------------------------------------------------------------
void DValidationMonitor::DisableControlsForBatch()
{
	m_ProviderName.	SetReadOnly(m_bFromBatch);
	m_DateFrom.		SetReadOnly(m_bAllDate);
	m_DateTo.		SetReadOnly(m_bAllDate);
}

//----------------------------------------------------------------------------
void DValidationMonitor::CheckValidationDate()
{
	if (m_bSelectionDate && (m_DateFrom.IsEmpty() || m_DateTo.IsEmpty() || m_DateTo < m_DateFrom))
	{
		if (!IsInUnattendedMode())
		{
			m_pMessages->Add(cwsprintf(_TB("From date {0-%s} is later than To date {1-%s}"), m_DateFrom.Str(), m_DateTo.Str()), CMessages::MSG_HINT);
			m_pMessages->Show(TRUE);
		}
	}
}

//----------------------------------------------------------------------------
void DValidationMonitor::DoGaugeManagement()
{
	int		nResult;
	TCHAR	buffer[512];

	if(m_bValidationEnded || m_nValidationCounter == 0)
	{	
		if (m_bNeedMassiveValidation)
		{
			m_GaugeDescription = _TB("Massive Validation has never been performed");
			nResult = swprintf_s(buffer, szNamespace, szGlyphF, szIconWarning);
		}
		else
		{
			m_GaugeDescription = _TB("Massive validation ended");

			m_bErrorsOccurred = ErrorsOccurred();	

			if (m_bErrorsOccurred)
			{
				m_GaugeDescription += _T(" ") + _TB("with errors.");
				nResult = swprintf_s(buffer, szNamespace, szGlyphF, szIconError);
			}
			else
			{
				m_GaugeDescription += _T(" ") + _TB("successfully.");
				nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphOk);
			}
		}
	}
	else
	{
		if (m_bValidationStarted)
		{
			m_GaugeDescription = _TB("Massive validation in progress.");
			nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphWait);
		}
		else
		{
			if (m_bNeedMassiveValidation)
			{
				m_GaugeDescription = _TB("Massive validation is needed");
				nResult = swprintf_s(buffer, szNamespace, szGlyphF, szIconWarning);
			}
			else 
			{
				m_GaugeDescription = _TB("Retrieving information for current validation...");
				nResult = swprintf_s(buffer, szNamespace, szGlyphF, szGlyphWait);
			}
		}
	}
	if (m_pGaugeLabel)
	{
		m_pGaugeLabel->SetValue(m_GaugeDescription);
		m_pGaugeLabel->UpdateCtrlView();
	}
	m_PictureStatus = buffer;
}

//----------------------------------------------------------------------------
void DValidationMonitor::ValidationDocSummaryManagement()
{
	LoadDocSummaryDBT();
	LoadKFToFixSummaryDBT();
}

//----------------------------------------------------------------------------
void DValidationMonitor::LoadDocSummaryDBT()
{
	TEnhDS_ValidationInfoSummary	aRec;
	DataLng nCount = 0;
	DataStr strEmpty = _T("");
	int indexCurrentDoc = -1;

	SqlTable* pTable(NULL);
	pTable = new SqlTable(&aRec, GetReadOnlySqlSession());

	/*
		SELECT DocNamespace, COUNT(*)
		FROM DS_ValidationInfo
		WHERE ProviderName = 'CRMInfinity' AND MassiveError <> ''
		GROUP BY DocNamespace
		ORDER BY DocNamespace
	*/

	pTable->Open();
	pTable->Select(aRec.f_DocNamespace);
	pTable->SelectSqlFun(_T("COUNT(*)"), &nCount);
	pTable->m_strFilter =  cwsprintf	(	
											_T("%s = %s AND %s NOT LIKE %s GROUP BY %s"),
											(LPCTSTR) aRec.GetColumnName(&aRec.f_ProviderName),
											m_pSqlConnection->NativeConvert(&m_ProviderName),
											(LPCTSTR) aRec.GetColumnName(&aRec.f_MessageError),
											m_pSqlConnection->NativeConvert(&strEmpty),	
											(LPCTSTR) aRec.GetColumnName(&aRec.f_DocNamespace)	
										);

	pTable->AddSortColumn(aRec.f_DocNamespace);

	TRY
	{
		pTable->Query();

		TEnhDS_ValidationInfoSummary* pLine;

		m_pDBTDetailDocSummary->RemoveAll();

		DataStr sNamespace    = _T("");
		DataStr sOldNamespace = _T("");

		while (!pTable->IsEOF())
		{
			sNamespace = aRec.f_DocNamespace;

			if	(
					m_pDBTDetailDocSummary->GetUpperBound() < 0 || 
					sNamespace != sOldNamespace
				)
			{
				pLine = (TEnhDS_ValidationInfoSummary*) m_pDBTDetailDocSummary->AddRecord();
				pLine->f_DocNamespace	= aRec.f_DocNamespace;
				pLine->l_NoErrors		= nCount.Str();
				sOldNamespace			= aRec.f_DocNamespace;
				
				if (pLine->f_DocNamespace == m_DocNamespace)
					indexCurrentDoc = m_pDBTDetailDocSummary->GetCurrentRowIdx();
			}

			pTable->MoveNext();
		}
		
		if (pTable && pTable->IsOpen())
		{
			pTable->Close();
			SAFE_DELETE(pTable);
		}
	}
	CATCH(SqlException, e)	
	{
		TRACE(cwsprintf(_T("DValidationMonitor::LoadDocSummaryDBT SqlException.\n%s"),e->m_strError));
		if (pTable->IsOpen())	
			pTable->Close();
		
		SAFE_DELETE(pTable);
		return;
	}
	END_CATCH
		
		
	if (indexCurrentDoc == -1)
		indexCurrentDoc = 0;

	if (m_pDBTDetailDocSummary->GetSize() > 0)
	{
		m_DocNamespace = ((TEnhDS_ValidationInfoSummary*)m_pDBTDetailDocSummary->GetRow(indexCurrentDoc))->f_DocNamespace;
		m_pDBTDetailDocSummary->SetCurrentRow(indexCurrentDoc);
	}
	else
		m_DocNamespace = _T("");
}

//----------------------------------------------------------------------------
void DValidationMonitor::LoadKFToFixSummaryDBT()
{
	TEnhDS_ValidationFKToFixSummary	aRec;
	DataLng nCount = 0;

	SqlTable* pTable(NULL);
	pTable = new SqlTable(&aRec, GetReadOnlySqlSession());

	/*
		SELECT DocNamespace, COUNT(*)
		FROM DS_ValidationFKToFix
		WHERE ProviderName = 'CRMInfinity' 
		GROUP BY DocNamespace
		ORDER BY DocNamespace
	*/

	pTable->Open();
	pTable->Select(aRec.f_DocNamespace);
	pTable->SelectSqlFun(_T("COUNT(*)"), &nCount);
	pTable->m_strFilter =  cwsprintf	(	
											_T("%s = %s GROUP BY %s"),
											(LPCTSTR) aRec.GetColumnName(&aRec.f_ProviderName),
											m_pSqlConnection->NativeConvert(&m_ProviderName),
											(LPCTSTR) aRec.GetColumnName(&aRec.f_DocNamespace)	
										);

	pTable->AddSortColumn(aRec.f_DocNamespace);

	TRY
	{
		pTable->Query();

		TEnhDS_ValidationFKToFixSummary* pLine;

		m_pDBTFKToFixSummary->RemoveAll();

		DataStr sNamespace    = _T("");
		DataStr sOldNamespace = _T("");

		while (!pTable->IsEOF())
		{
			sNamespace = aRec.f_DocNamespace;

			if	(
					m_pDBTFKToFixSummary->GetUpperBound() < 0 || 
					sNamespace != sOldNamespace
				)
			{
				pLine = (TEnhDS_ValidationFKToFixSummary*) m_pDBTFKToFixSummary->AddRecord();
				pLine->f_DocNamespace	= aRec.f_DocNamespace;
				pLine->l_NoFKToFix		= nCount.Str();
				sOldNamespace			= aRec.f_DocNamespace;
			}

			pTable->MoveNext();
		}
		
		if (pTable && pTable->IsOpen())
		{
			pTable->Close();
			SAFE_DELETE(pTable);
		}
	}
	CATCH(SqlException, e)	
	{
		TRACE(cwsprintf(_T("DValidationMonitor::LoadKFToFixSummaryDBT SqlException.\n%s"),e->m_strError));
		if (pTable->IsOpen())	
			pTable->Close();
		
		SAFE_DELETE(pTable);
		return;
	}
	END_CATCH	
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnDocNamespaceChanged()
{
	m_pDBTDetail->ReloadData();
	m_pMonitorTile->SetCollapsed(FALSE);
	int currentRow = -1;
	for (int i = 0; i < m_pDBTDetailDocSummary->GetSize(); i++)
	{
		TEnhDS_ValidationInfoSummary* pRec = (TEnhDS_ValidationInfoSummary*) m_pDBTDetailDocSummary->GetRow(i);
		if (pRec->f_DocNamespace == m_DocNamespace)
		{
			currentRow = i;
			break;
		}
	}
	if (currentRow >= 0)
		m_pDBTDetailDocSummary->SetCurrentRow(currentRow);

	UpdateDataView();
}

//---------------------------------------------------------------------------------------
void DValidationMonitor::OnMonitorSummaryRowChanged()
{
	TEnhDS_ValidationInfoSummary* pRec = (TEnhDS_ValidationInfoSummary*)m_pDBTDetailDocSummary->GetCurrentRow();
	m_DocNamespace = pRec->f_DocNamespace;
	m_pDBTDetail->ReloadData();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnSelectionDateChanged()
{
	if (m_bAllDate)
	{
		m_DateFrom.	Clear();
		m_DateTo.	Clear();
	}

	m_DateFrom.	SetReadOnly(!m_bSelectionDate);
	m_DateTo.	SetReadOnly(!m_bSelectionDate);

	if (!m_bSelectionDate)
		m_pDBTDetail->ReloadData();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnDateFromChanged()
{
	if 
		(
			!m_DateFrom.IsEmpty() &&
			(m_DateTo.IsEmpty() || m_DateTo < m_DateFrom)
		)
		m_DateTo = m_DateFrom;

	CheckValidationDate();
	m_pDBTDetail->ReloadData();
	
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnDateToChanged()
{
	CheckValidationDate();
	m_pDBTDetail->ReloadData();

	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DValidationMonitor::DoReloadErrorToFixPanel()
{
	m_pDBTFKToFixDetail->ReloadData();
	LoadKFToFixSummaryDBT();
}

//-----------------------------------------------------------------------------
void DValidationMonitor::DoReloadData()
{
	m_pDBTDetail->ReloadData();
	DoReloadErrorToFixPanel();
}

//---------------------------------------------------------------------------------
void DValidationMonitor::OnMonitorRefresh()
{
	GetNotValidView(TRUE);

	BeginWaitCursor();
	DoRefresh();
	DoReloadErrorToFixPanel();
	EndWaitCursor();

	UpdateDataView();
}

//---------------------------------------------------------------------------------------
void DValidationMonitor::OnMonitorRefreshUpdate(CCmdUI* pCmdUI)
{
	BOOL bEnable = !m_bAutoRefresh && (m_bAllDate || m_bSelectionDate && !m_DateFrom.IsEmpty() && !m_DateTo.IsEmpty() && m_DateFrom <= m_DateTo);
	 
	pCmdUI->Enable(bEnable);
}

//---------------------------------------------------------------------------------------
void DValidationMonitor::OnValidationMonitorSummaryRowChanged()
{
	TEnhDS_ValidationInfoSummary* pRec = (TEnhDS_ValidationInfoSummary*) m_pDBTDetailDocSummary->GetCurrentRow();
	m_DocNamespace = pRec->f_DocNamespace;
	m_pDBTDetail->ReloadData();
	UpdateDataView();
}

//---------------------------------------------------------------------------------------
void DValidationMonitor::OnValidationFKToFixSummaryRowChanged()
{
	TEnhDS_ValidationFKToFixSummary* pRec = (TEnhDS_ValidationFKToFixSummary*) m_pDBTFKToFixSummary->GetCurrentRow();
	m_DocNamespaceFKToFix = pRec->f_DocNamespace;
	m_pDBTFKToFixDetail->ReloadData();
	UpdateDataView();
}

//-----------------------------------------------------------------------------
void DValidationMonitor::SetFiltersEnable(BOOL bSet)
{
	CBaseTileDialog* pTile = NULL;
	TileDialogEnable(IDC_DS_MONITOR_TILE_GRP, IDD_DATAVALIDATION_MONITOR_FILTER_BY_DATE,	bSet);
}

//----------------------------------------------------------------------------
void DValidationMonitor::DoRefresh()
{
	if (!m_bIsActivatedCRMInfinity)
		return;

	BeginWaitCursor();

	if (m_bValidationEnded)
	{
		LoadDocSummaryDBT();
		m_pDBTDetail->ReloadData();
		
		m_bErrorsOccurred = ErrorsOccurred();
	}
	else
	{
		ValidationDocSummaryManagement();	

		BOOL bCurrentIsMassiveValidating = AfxGetDataSynchroManager()->IsMassiveValidating();
		m_bValidationEnded = (m_bIsMassiveValidating && !bCurrentIsMassiveValidating) || m_nValidationCounter == 0;

		if (m_bFromBatch)
		{
			if(m_bValidationEnded && m_nValidationCounter > 0)
				m_nValidationCounter--;
		}
		else
		{
			if(!m_bValidationStarted && m_nValidationCounter > 0)
				m_nValidationCounter--;
		}

		if (bCurrentIsMassiveValidating)
			m_bValidationStarted = TRUE;

		m_bIsMassiveValidating = bCurrentIsMassiveValidating;
	}
	
	EndWaitCursor();
}

//----------------------------------------------------------------------------
void DValidationMonitor::DoGaugeRefresh()
{
	if (m_bValidationEnded)
	{
		EndGaugeTimer();
		CompleteGauge();
	}
	else
	{
		if (!m_bValidationStarted || m_nValidationCounter == 0)
			CompleteGauge();
		else
			UpdateGaugeIndeterminate();
	}
	

	DoGaugeManagement();
}

//-----------------------------------------------------------------------------
void DValidationMonitor::GetDecodingInfo(const DataGuid& tbGuid, DataStr& recordKey, DataStr& recordDescription)
{
	recordKey			= _T("");
	recordDescription	= _T("");

	// Posso visualizzare solo i documenti che hanno errori o tutti
	CSynchroDocInfo* pSynchroDocInfo = m_pDocumentsToMonitor->GetDocumentByNs(m_DocNamespace);

	if (pSynchroDocInfo)
		pSynchroDocInfo->GetDecodingInfo(tbGuid, recordKey, recordDescription);
}

//----------------------------------------------------------------------------
BOOL DValidationMonitor::ErrorsOccurred()
{
	BOOL bErrorOccurred = FALSE;

	CString			 sDocNamespace   = _T("");
	CSynchroDocInfo* pSynchroDocInfo = NULL;
	
	TEnhDS_ValidationInfoSummary* pSummaryRec = NULL;

	DataInt nIncorrectNamespace = m_pDBTDetailDocSummary->GetUpperBound();

	if (nIncorrectNamespace < 0)
		return bErrorOccurred;

	bErrorOccurred = TRUE;
	m_pDocumentsToMonitor->RemoveAll();

	m_DocNamespace.Clear();
	m_DocNamespace =  m_pDBTDetailDocSummary->GetDetail(0)->f_DocNamespace;
	
	for (int i = 0; i <= nIncorrectNamespace; i++)
	{
		 pSummaryRec = (TEnhDS_ValidationInfoSummary*)m_pDBTDetailDocSummary->GetDetail(i);
		 sDocNamespace = pSummaryRec->f_DocNamespace;

		pSynchroDocInfo = new CSynchroDocInfo(sDocNamespace);
		pSynchroDocInfo->SetDecodingInfo(GetReadOnlySqlSession());
		if (pSynchroDocInfo->IsValid())
			m_pDocumentsToMonitor->Add(pSynchroDocInfo);		
		else 
			delete pSynchroDocInfo;
	}
	
	if (!m_DocNamespace.IsEmpty())
		m_pDBTDetail->ReloadData();

	return bErrorOccurred;
}

//---------------------------------------------------------------------------------
void DValidationMonitor::OnValidationMonitorDetailOpenDocument()
{
	TEnhDS_ValidationInfoDetail* pRecDetail = (TEnhDS_ValidationInfoDetail*) m_pDBTDetail->GetCurrentRow();
	if (!pRecDetail)
		return;
	
	CString sDocNamespace = pRecDetail->f_DocNamespace;
	CAbstractFormDoc* pDoc = NULL;
	if (!sDocNamespace.IsEmpty())
	{
		sDocNamespace.Replace(_T("Document."), _T(""));
		pDoc = (CAbstractFormDoc*)AfxGetTbCmdManager()->RunDocument(sDocNamespace);
	}
	if (!pDoc)
		return;

	CBrowserByTBGuid* pBrowserByTBGuid = new CBrowserByTBGuid(pDoc, GetReadOnlySqlSession());
	if (!pBrowserByTBGuid->BrowseOnRecord(pRecDetail->f_DocTBGuid))
		AfxMessageBox(cwsprintf(_T("The document with TBGuid {%s} doesn't exist."), pRecDetail->f_DocTBGuid.FormatData()));

	SAFE_DELETE(pBrowserByTBGuid);
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnUpdateDetailOpenDocument(CCmdUI* pCmdUI)
{
	BOOL bEnabled = FALSE;
	TEnhDS_ValidationInfoDetail* pRecDetail = (TEnhDS_ValidationInfoDetail*)m_pDBTDetail->GetCurrentRow();

	if (pRecDetail)
		bEnabled = TRUE;

	pCmdUI->Enable(bEnabled);
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnValidationFKToFixAdd()
{
	TDS_ValidationFKtoFix* pRecDetail = (TDS_ValidationFKtoFix*) m_pDBTFKToFixDetail->GetCurrentRow();
	if (!pRecDetail)
		return;
	
	CString sDocNamespace = pRecDetail->f_DocNamespace;
	CAbstractFormDoc* pDoc = NULL;
	if (!sDocNamespace.IsEmpty())
	{
		sDocNamespace.Replace(_T("Document."), _T(""));
		pDoc = (CAbstractFormDoc*)AfxGetTbCmdManager()->RunDocument(sDocNamespace);
	}
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnUpdateValidationFKToFixAdd(CCmdUI* pCmdUI)
{
	pCmdUI->Enable(m_pDBTFKToFixDetail->GetUpperBound() >= 0);
}

//---------------------------------------------------------------------------------
void DValidationMonitor::OnValidationMonitorDetailCopyMessage()
{
	if (OpenClipboard(NULL))
	{
		EmptyClipboard();

		TEnhDS_ValidationInfoDetail* pRecDetail = (TEnhDS_ValidationInfoDetail*)m_pDBTDetail->GetCurrentRow();
		if (!pRecDetail)
			return;
		CString aStr = pRecDetail->l_FormattedMsgError;

		HLOCAL hMem = GlobalAlloc(LHND, ((aStr.GetLength() + 1) * sizeof(TCHAR))); 
		char* cptr = (char*) GlobalLock(hMem); 
		memcpy( cptr, aStr, ((aStr.GetLength() + 1) * sizeof(TCHAR))); 
		GlobalUnlock(hMem);
		if (SetClipboardData(CF_UNICODETEXT, hMem) == NULL)
		{
			AfxMessageBox( _TB("Impossible copy to Clipboard") );
			CloseClipboard();
			return;  
		}

		CloseClipboard();
	}
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnUpdateDetailCopyMessage(CCmdUI* pCmdUI)
{
	BOOL bEnabled = FALSE;
	TEnhDS_ValidationInfoDetail* pRecDetail = (TEnhDS_ValidationInfoDetail*)m_pDBTDetail->GetCurrentRow();
	
	if (pRecDetail)
		bEnabled = !pRecDetail->l_FormattedMsgError.IsEmpty();

	pCmdUI->Enable(bEnabled);
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnValidationFKToFixDetailCopyValue()
{
	if (OpenClipboard(NULL))
	{
		EmptyClipboard();

		TDS_ValidationFKtoFix* pRecDetail = (TDS_ValidationFKtoFix*)m_pDBTFKToFixDetail->GetCurrentRow();
		if (!pRecDetail)
			return;
		CString aStr = pRecDetail->f_ValueToFix;

		HLOCAL hMem = GlobalAlloc(LHND, ((aStr.GetLength() + 1) * sizeof(TCHAR))); 
		char* cptr = (char*) GlobalLock(hMem); 
		memcpy( cptr, aStr, ((aStr.GetLength() + 1) * sizeof(TCHAR))); 
		GlobalUnlock(hMem);
		if (SetClipboardData(CF_UNICODETEXT, hMem) == NULL)
		{
			AfxMessageBox( _TB("Impossible copy to Clipboard") );
			CloseClipboard();
			return;  
		}

		CloseClipboard();
	}
}

//-----------------------------------------------------------------------------
void DValidationMonitor::OnUpdateFKToFixDetailCopyValue(CCmdUI* pCmdUI)
{
	BOOL bEnabled = FALSE;
	TDS_ValidationFKtoFix* pRecDetail = (TDS_ValidationFKtoFix*)m_pDBTDetail->GetCurrentRow();
	
	if (pRecDetail)
		bEnabled = !pRecDetail->f_ValueToFix.IsEmpty();

	pCmdUI->Enable(bEnabled);
}

//-----------------------------------------------------------------------------
void DValidationMonitor::CustomizeBodyEdit(CBodyEdit* pBE)
{
	if (pBE->GetNamespace().GetObjectName() == _T("ValidationInfoMonitorBE"))
	{
		CBEButton* pBtn = pBE->m_HeaderToolBar.AddButton
		(
			_NS_BE_TOOLBAR_BTN("OpenDocument"),
			ID_VALIDATION_MONITOR_OPEN_DOCUMENT,
			TBIcon(szIconOpen, MINI),
			_TB("Open Document"),
			_TB("Open Document")
		);
		pBtn->m_bDisableOnReadOnly = TRUE;

		pBtn = pBE->m_HeaderToolBar.AddButton
		(
			_NS_BE_TOOLBAR_BTN("CopyMessage"),
			ID_VALIDATION_MONITOR_COPY_MSG,
			TBIcon(szIconCopy, MINI),
			_TB("Copy Message"),
			_TB("Copy Message")
		);
		pBtn->m_bDisableOnReadOnly = TRUE;
				
		pBE->EnableAddRow(FALSE);
		pBE->EnableDeleteRow(FALSE);
	}
	else if (pBE->GetNamespace().GetObjectName() == _T("MonitorDetails"))
	{
		pBE->EnableAddRow(FALSE);
		pBE->EnableDeleteRow(FALSE);
	}
	else if (pBE->GetNamespace().GetObjectName() == _T("ValidationFKToFixDetail"))
	{
		CBEButton* pBtn = pBE->m_HeaderToolBar.AddButton
		(
			_NS_BE_TOOLBAR_BTN("Add"),
			ID_VALIDATION_MONITOR_FK_TO_FIX_ADD,
			TBIcon(szIconAdd, MINI),
			_TB("Add"),
			_TB("Add")
		);
		pBtn->m_bDisableOnReadOnly = TRUE;

		pBtn = pBE->m_HeaderToolBar.AddButton
		(
			_NS_BE_TOOLBAR_BTN("CopyValue"),
			ID_VALIDATION_MONITOR_FK_TO_FIX_COPY,
			TBIcon(szIconCopy, MINI),
			_TB("Copy Code"),
			_TB("Copy Code")
		);
		pBtn->m_bDisableOnReadOnly = TRUE;
	}
}

//-----------------------------------------------------------------------------
CString	DValidationMonitor::OnGetCaption(CAbstractFormView* pView)
{
	if (pView->GetNamespace().GetObjectName() == _T("DValidationDetailRowView"))
	{
		if (!m_pDBTDetail)
			return _T("");

		TEnhDS_ValidationInfoDetail* pCurrentLine = (TEnhDS_ValidationInfoDetail*) m_pDBTDetail->GetCurrentRow();
		CString m_Header = _T("");

		if (!pCurrentLine)
			return m_Header;

		CString sDescription	= pCurrentLine->l_Description.FormatData();
		CString sCode			= pCurrentLine->l_Code;
	
		if (!sDescription.IsEmpty())
			m_Header = sDescription + _T(": ");

		if (!sCode.IsEmpty())
			m_Header += _T("[") + sCode + _T("] ");

		return m_Header;	
	}

	return _T("");
}


//----------------------------------------------------------------------------
void DValidationMonitor::SetTileStatus()
{
	if (!m_pSummaryTile || !m_pMonitorTile)
		GetResultsTileDialog();
	
	m_pResultsTilePanel->SetCollapsed(FALSE);
	m_pSummaryTile->SetCollapsed(!m_bErrorsOccurred);
	m_pMonitorTile->SetCollapsed(m_bErrorsOccurred);
}

//----------------------------------------------------------------------------
void DValidationMonitor::GetResultsTileDialog()
{
	CMasterFormView* pView = (CMasterFormView*)GetFirstView();
	if (!pView)
		return;

	m_pSummaryTile = GetTileDialog(IDD_DATAVALIDATION_MONITOR_SUMMARY);
	m_pMonitorTile = GetTileDialog(IDD_DATAVALIDATION_MONITOR_DETAIL);

/*	for (int i = 0; i < pView->m_pTileGroups->GetSize(); i++)
	{
		CTileGroup* pGroup = pView->m_pTileGroups->GetAt(i);
		m_pSummaryTile = pGroup->GetTileDialog(IDD_DATAVALIDATION_MONITOR_SUMMARY);
		m_pMonitorTile = pGroup->GetTileDialog(IDD_DATAVALIDATION_MONITOR_DETAIL);
		if (m_pSummaryTile && m_pMonitorTile)
			break;
	}
	*/
}

//----------------------------------------------------------------------------
void DValidationMonitor::ExpandDetail()
{
	if (m_pMonitorTile)
	m_pMonitorTile->SetCollapsed(!m_pMonitorTile->IsCollapsed());
}

//-----------------------------------------------------------------------------
void DValidationMonitor::DoOpenDockPanel(BOOL bOpen /* = TRUE */)
{
	m_pViewFKToFixPanel->SetAutoHideMode(FALSE, CBRS_RIGHT | CBRS_HIDE_INPLACE, 0, TRUE);
}

//----------------------------------------------------------------------------
void DValidationMonitor::OnPrepareAuxData(CTileDialog * pTileDlg)
{
	if (pTileDlg->GetNamespace().GetObjectName() == _NS_TILEDLG("Summary"))
	{
		m_pSummaryTile = pTileDlg;
	}
	else if (pTileDlg->GetNamespace().GetObjectName() == _NS_TILEDLG("MonitorDetails"))
	{
		m_pMonitorTile = pTileDlg;
	}


}