#include "stdafx.h" 

#include <TbXMLCore\XMLDocObj.h>
#include <TbNameSolver\PathFinder.h>
#include <TBClientCore\ClientObjects.h>
#include <TBClientCore\ServerConnectionInfo.h>
#include <TbOleDb\SqlRec.h>
#include <TbOleDb\SqlAccessor.h>
#include <TbOleDb\RIChecker.h>
#include <TbWebServicesWrappers\DataSynchronizerWrapper.h>

#include <TbFrameworkImages\CommonImages.h>
#include <TbFrameworkImages\GeneralFunctions.h>

#include "TBDataSynchroClientEnums.h"
#include "DSManager.h"
#include "UICommon.h"
#include "UINotification.h"
#include "UINotification.hjson"
#include "CDNotification.h"
#include "UIProviders.h"


static TCHAR szNamespace[] = _T("Image.Framework.TbFrameworkImages.Images.%s.%s.png");
static TCHAR szGlyphF[]    = _T("Glyph");

static TCHAR szOK[] = _T("SynchroOK");
static TCHAR szWarning[] = _T("SynchroWarning");

static TCHAR szParamProvider	[]	= _T("P1");
static TCHAR szParamTBGuid		[]	= _T("P2");
static TCHAR szParamDocNamespace[]	= _T("P3");
static TCHAR szParamTableName	[]	= _T("P4");

static int nDockPaneTimeOpen = 60;

/*
///////////////////////////////////////////////////////////////////////////////
//             class DBTActionsLog implementation
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(DBTActionsLog, DBTSlaveBuffered)

//-----------------------------------------------------------------------------
DBTActionsLog::DBTActionsLog
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("DSActionsLog"), ALLOW_EMPTY_BODY, TRUE)
{
	//è solo di consultazione
	SetOnlyForRead(TRUE);
	SetReadOnly();
	SetNoDelete(TRUE);
	m_bDBTOnView = TRUE;
}

//-----------------------------------------------------------------------------
void DBTActionsLog::OnDefineQuery	()
{
	m_pTable->SelectAll			();

	this->m_pTable->m_strFilter = cwsprintf(_T("%s <> %s AND %s = ? "), 
								TDS_ActionsLog::szDocTBGuid, 
								m_pTable->m_pSqlConnection->NativeConvert(&DataGuid()), 
								TDS_ActionsLog::szDocTBGuid);

	m_pTable->AddParam			(szParamTBGuid,	GetActionLogRec()->f_DocTBGuid);
	m_pTable->AddSortColumn		(GetActionLogRec()->f_LogId, TRUE);
}

//-----------------------------------------------------------------------------
void DBTActionsLog::OnPrepareQuery()
{
	m_pTable->SetParamValue		(szParamTBGuid,	GetMasterRecord()->f_TBGuid);
}

//-----------------------------------------------------------------------------
void DBTActionsLog::OnDisableControlsAlways()
{
	TEnhDS_ActionsLog* pActionsLogRec = GetActionLogRec();
	pActionsLogRec->f_ActionData.SetAlwaysReadOnly();
	pActionsLogRec->f_ActionType.SetAlwaysReadOnly();
	pActionsLogRec->f_LogId.SetAlwaysReadOnly();	
	
	pActionsLogRec->f_SynchStatus.SetAlwaysReadOnly();
	pActionsLogRec->f_SynchDirection.SetAlwaysReadOnly();

	pActionsLogRec->f_SynchMessage.SetAlwaysReadOnly();
	
	pActionsLogRec->f_TBCreated.SetAlwaysReadOnly();
	pActionsLogRec->f_TBCreatedID.SetAlwaysReadOnly();
	pActionsLogRec->l_WorkerDescri.SetAlwaysReadOnly();
}

//-----------------------------------------------------------------------------
void DBTActionsLog::OnPrepareAuxColumns(SqlRecord* pRecord)
{
	TEnhDS_ActionsLog* pActionsLogRec = (TEnhDS_ActionsLog*)pRecord;

	CWorker* pWorker = (pRecord && pRecord->f_TBCreatedID >= 0) ? AfxGetWorkersTable()->GetWorker(pRecord->f_TBCreatedID) : NULL;
	pActionsLogRec->l_WorkerDescri = (pWorker) ? cwsprintf(_T("%s %s"), pWorker->GetName(), pWorker->GetLastName()) : cwsprintf(_T("%s"), pActionsLogRec->f_TBCreatedID.Str());

	pActionsLogRec->l_SynchStatusBmp = OnGetSyncStatusBmp(pActionsLogRec->f_SynchStatus, pActionsLogRec->f_ProviderName);
	pActionsLogRec->l_SynchDirectionBmp = OnGetSyncDirectionBmp(pActionsLogRec->f_SynchDirection);	
}

*/

///////////////////////////////////////////////////////////////////////////////
//						CNotificationInfo definition
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
CNotificationInfo::CNotificationInfo(SqlRecord* pRecord)
	:
	m_pRecord(NULL)
{
	//clono il sqlRecord perchè quando faccio la notifica al server il sqlrecord originario potrebbe essere già distrutto
	if (pRecord)
		m_pRecord = pRecord->Clone();
}

//----------------------------------------------------------------------------
CNotificationInfo::~CNotificationInfo()
{
	SAFE_DELETE(m_pRecord);
}

///////////////////////////////////////////////////////////////////////////////
//						CDataSynchroManager definition
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CDataSynchroNotifier, CDataSynchroNotifierObj)

CDataSynchroNotifier::CDataSynchroNotifier(CAbstractFormDoc* pDocument, CDataSynchronizerWrapper* pDataSynchronizerWrapper, SqlConnection* pConnection)
:
	m_pDocument			(pDocument),
	m_pDataSynchronizer	(pDataSynchronizerWrapper),
	m_pSqlConnection	(pConnection),
	m_pSqlSession		(NULL),
//	m_pTUSynchroInfo	(NULL),
	m_pLogTable			(NULL)
{
	/*
	m_strInsertCmd = cwsprintf(_T("INSERT INTO %s (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)"),
		TDS_ActionsLog::GetStaticName(),
		TDS_ActionsLog::szDocNamespace, TDS_ActionsLog::szDocTBGuid, TDS_ActionsLog::szActionType, TDS_ActionsLog::szActionData,
		TDS_ActionsLog::szSynchDirection, TDS_ActionsLog::szSynchXMLData, TDS_ActionsLog::szSynchStatus, TDS_ActionsLog::szSynchMessage, TDS_ActionsLog::szProviderName, CREATED_ID_COL_NAME, MODIFIED_ID_COL_NAME);	
	
	m_pSqlSession = m_pSqlConnection->GetNewSqlSession();

	m_pLogTable = new SqlTable(m_pSqlSession);
	m_pLogTable->SetAutocommit();
	m_pLogTable->Open(FALSE, E_NO_CURSOR);

	m_pTUSynchroInfo = new TUDS_SynchronizationInfo(m_pDocument);
	m_pTUSynchroInfo->SetSqlSession(m_pSqlSession);
	m_pTUSynchroInfo->SetAutocommit();
	*/
}

//----------------------------------------------------------------------------
 CDataSynchroNotifier::~CDataSynchroNotifier()
{
	 if (m_pLogTable)
	 {
		 if (m_pLogTable->IsOpen())
			 m_pLogTable->Close();
		 delete m_pLogTable;
	 }
	 /*
	 if (m_pTUSynchroInfo)
		 delete m_pTUSynchroInfo;
	 */

	 if (m_pSqlSession)
	 {
		 m_pSqlSession->Close();
		 SAFE_DELETE(m_pSqlSession);
	 }

	 SAFE_DELETE(m_pDataSynchronizer);
}

//----------------------------------------------------------------------------
void CDataSynchroNotifier::AddNotificationInfo(CNotificationInfo* pNotificationInfo)
{
	m_NotificationInfoArray.Add(pNotificationInfo);
}

//-----------------------------------------------------------------------------
void CDataSynchroNotifier::RemoveNotifications()
{
	m_NotificationInfoArray.RemoveAll();
}

//----------------------------------------------------------------------------
DataLng CDataSynchroNotifier::InsertLogAction(CNotificationInfo* pNotificationInfo)
{
	DataLng logId;
	CString strInsertQuery = cwsprintf(_T(" %s VALUES ( %s,  %s,  %s,  %s,  %s,  %s,  %s,  %s, %s, %s, %s)"),
		m_strInsertCmd,
		m_pSqlConnection->NativeConvert(&DataStr(pNotificationInfo->m_strDocNamespace)),
		m_pSqlConnection->NativeConvert(&pNotificationInfo->m_DocGuid),
		m_pSqlConnection->NativeConvert(&pNotificationInfo->m_ActionType),
		m_pSqlConnection->NativeConvert(&pNotificationInfo->m_ActionData),
		m_pSqlConnection->NativeConvert(&DataEnum(E_SYNCHRODIRECTION_TYPE_OUTBOUND)),
		m_pSqlConnection->NativeConvert(&DataStr(_T(""))),
		m_pSqlConnection->NativeConvert(&DataEnum(E_SYNCHROSTATUS_TYPE_TOSYNCHRO)),
		m_pSqlConnection->NativeConvert(&DataStr(_T(""))),
		m_pSqlConnection->NativeConvert(&pNotificationInfo->m_ProviderName),
		m_pSqlConnection->NativeConvert(&DataLng(AfxGetWorkerId())),
		m_pSqlConnection->NativeConvert(&DataLng(AfxGetWorkerId())));


	SqlTable aTable(m_pSqlSession);
	TRY
	{
		m_pLogTable->ExecuteQuery(strInsertQuery);

		if (m_pSqlConnection->GetDBMSType() == DBMS_SQLSERVER)
		{
			//chiamo SELECT IDENT_CURRENT ed effettuo il binding dell'unico campo autoincremental della tabella
			aTable.Open();
			aTable.m_strSQL = cwsprintf(_T("SELECT IDENT_CURRENT('%s')"), TDS_ActionsLog::GetStaticName());	
			aTable.Select(_T("Ident"), &logId, -1);
			aTable.Query();
			aTable.Close();
		}
	}
	CATCH(SqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
		THROW(e);
	}
	END_CATCH

	return logId;
}

//----------------------------------------------------------------------------
BOOL CDataSynchroNotifier::ValidateNotification(CNotificationInfo* pNotificationInfo)
{
	CStringArray arMsg;
	BOOL bValidate = TRUE; //??

	RICheckNode* pRoot = AfxGetApplicationContext()->GetObject<RICheckNode>();
	RICheckNode* pNode = NULL;

	// se è un azione di update o insert devo prima validarla
	if (pNotificationInfo->m_ActionType == E_SYNCHROACTION_TYPE_INSERT || pNotificationInfo->m_ActionType == E_SYNCHROACTION_TYPE_UPDATE && pRoot)
	{
		if (pRoot->LookUp(DataStr(pNotificationInfo->m_strDocNamespace), pNode))
		{
			pNode->IsValid(pNotificationInfo->m_pRecord, arMsg);
			CString sSerializesErrors = _T("");

			if (arMsg.GetUpperBound() >= 0)
				sSerializesErrors = pRoot->SerializeErrors(arMsg);

			CString MessOut(_T(""));
			bValidate = m_pDataSynchronizer->ValidateDocument(pNotificationInfo->m_ProviderName, pNotificationInfo->m_strDocNamespace, pNotificationInfo->m_strTableName, pNotificationInfo->m_DocGuid.Str(), sSerializesErrors, MessOut);
		}
	}

	return bValidate;
}

//----------------------------------------------------------------------------
void CDataSynchroNotifier::NotifiyToDataSynchronizer()
{
	if (m_NotificationInfoArray.GetCount() == 0)
		return;

	BOOL bOK = TRUE;
	CNotificationInfo* pNotificationInfo = NULL;
	TableUpdater::FindResult eFindResult = TableUpdater::NONE;

	TRY
	{
		if(AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
		{
			CString username = GetUserName(0);
			CString compName = GetComputerName(0);
			m_pDataSynchronizer->UpdateUserMapping(username, compName);
		}

		for (int i = 0; i < m_NotificationInfoArray.GetSize(); i++)
		{
			CNotificationInfo* pNotificationInfo = (CNotificationInfo*)m_NotificationInfoArray.GetAt(i);
			ValidateNotification(pNotificationInfo);			
			if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			{
				CString strReqGuid;
				bOK = m_pDataSynchronizer->NotifyGuid(
														DataLng(1), 
														pNotificationInfo->m_ProviderName, 
														pNotificationInfo->m_strTableName, 
														pNotificationInfo->m_strDocNamespace, 
														pNotificationInfo->m_DocGuid.Str(), 
														pNotificationInfo->m_strOnlyForDMS, 
														pNotificationInfo->m_strIMagoConfigurations, 
														strReqGuid
														) || bOK;
				if (bOK)
					AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(pNotificationInfo->m_ProviderName)->m_strImagoRequestGuid[pNotificationInfo->m_DocGuid.Str()] = strReqGuid;
			}
		}

		//pulisco l'array
		m_NotificationInfoArray.RemoveAll();
		if (bOK)
		{
			if (!m_pDocument->IsInUnattendedMode() && !m_pDocument->IsABackgroundADM())
				m_pDocument->SendCommand(ID_DS_REFRESH_BTN); //se il documento è quello interattivo allora chiedo di ricaricare le info di sincronizzazione
		}
		else
			m_pDocument->Message(_TB("Impossibile to notify action to Synchronization service"));
		
	}	
	CATCH(SqlException, e)
	{
		m_pDocument->Message(_TB("Impossibile to notify action to Synchronization service") + " " + e->m_strError);
		m_NotificationInfoArray.RemoveAll();		
	}	
	END_CATCH	
}


//----------------------------------------------------------------------------
void CDataSynchroNotifier::SetInfo(CDataSynchronizerWrapper* pDataSynchronizer)
{
	m_pDataSynchronizer = new CDataSynchronizerWrapper(pDataSynchronizer);
}

///////////////////////////////////////////////////////////////////////////////
//						DMSSynchroManager declaration
// per la gestione della sincronizzazione degli allegati
///////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
DMSSynchroManager::DMSSynchroManager(CAbstractFormDoc* pDocument)
	:
	m_bCollectionChecked(FALSE),
	m_bNeedToSynchroCollection(FALSE),
	m_pDocument(pDocument)
{
}
	
//----------------------------------------------------------------------------
BOOL DMSSynchroManager::NeedToSynchroCollection()
{
	if (m_bCollectionChecked)
		return m_bNeedToSynchroCollection;

	if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		CString strXML = _T("");
		strXML = AfxGetDataSynchroManager()->GetMassiveSynchroLogs(DMSInfinityProvider,FALSE,FALSE);
		m_bNeedToSynchroCollection = strXML.IsEmpty();
		m_bCollectionChecked = TRUE;
	}

	return m_bNeedToSynchroCollection;	
}

//----------------------------------------------------------------------------
CString DMSSynchroManager::GetXMLSynchroData(DMSEventTypeEnum eventType, int nEventKey)
{
	DMSEventKey* pEventKey = new DMSEventKey(nEventKey);
	Array eventKeys;
	eventKeys.Add((CObject*)pEventKey);

	return GetXMLSynchroData(eventType, &eventKeys);
}

//----------------------------------------------------------------------------
CString DMSSynchroManager::GetXMLSynchroData(DMSEventTypeEnum eventType, Array* pEventKeys)
{
	CXMLDocumentObject aXMLDocument(TRUE, FALSE);
	CString notifyData;
	CXMLNode* pNode = NULL;
	DMSEventKey* pKey = NULL;

	if (!pEventKeys || pEventKeys->GetSize() < 1)
		return _T("");

	switch (eventType)
	{
		case NewDMSCollection:
		case UpdateDMSCollection:
			aXMLDocument.CreateRoot(_T("Collections"));	
			for (int i = 0; i < pEventKeys->GetSize(); i++)
			{
				pNode = aXMLDocument.CreateRootChild(_T("CollectionID"));
				pKey = (DMSEventKey*)pEventKeys->GetAt(i);
				pNode->SetText(DataInt(pKey->m_EventKey).FormatDataForXML());
			}
			aXMLDocument.GetXML(notifyData);
			break;

		case NewDMSAttachment:
		case DeleteDMSAttachment:
			aXMLDocument.CreateRoot(_T("Attachments"));	
			for (int i = 0; i < pEventKeys->GetSize(); i++)
			{
				pNode = aXMLDocument.CreateRootChild(_T("AttachmentID"));
				pKey = (DMSEventKey*)pEventKeys->GetAt(i);				
				pNode->SetText(DataInt(pKey->m_EventKey).FormatDataForXML());
			}
			aXMLDocument.GetXML(notifyData);
			break;
	}

	return notifyData;
}

///////////////////////////////////////////////////////////////////////////////
//					CDataSynchroManager declaration
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDNotification, CClientDoc)

BEGIN_MESSAGE_MAP(CDNotification, CClientDoc)
	//{{AFX_MSG_MAP(CDNotification)
	ON_COMMAND			(ID_FORCE_SYNCHRONIZATION,			 OnForceSynchronization)
	ON_COMMAND			(ID_FORCE_VALIDATION,				 OnForceValidation)
	ON_COMMAND			(ID_DS_REFRESH_BTN,					 OnReloadSynchroInfo)
	ON_BN_CLICKED		(IDC_IDC_DS_LASTSYNCH_COPYMSG,		 OnCopyMsg)
	ON_BN_CLICKED		(IDC_IDC_DS_LASTSYNCH_COPYMSG_HINTS, OnCopyMsgHints)

	ON_UPDATE_COMMAND_UI(ID_FORCE_SYNCHRONIZATION,		OnUpdateForceSynchronization)	
	ON_UPDATE_COMMAND_UI(ID_FORCE_VALIDATION,			OnUpdateForceValidation)
	ON_UPDATE_COMMAND_UI(ID_DS_REFRESH_BTN,				OnUpdateRefresh)

	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CDNotification::CDNotification()
	:
	m_pDataSynchronizer(NULL),
	m_pCDSynchroFilter(NULL),
	m_pDMSSynchroMng(NULL),
	m_pDataSynchroPane(NULL),
	m_pTRValidationInfo(NULL),
	m_pValidationTileDlg(NULL),
	m_bOpenDockPane(TRUE), 
	m_bImagoStudioRuntimeInstalled (FALSE),
	m_bDataSynchroEnabled(TRUE)
{
	CString strDataSynchronizerName = AfxGetPathFinder()->GetInstallationName() + _T("/DataSynchronizer/DataSynchronizer.asmx");
	m_pDataSynchronizer = new CDataSynchronizerWrapper(
		strDataSynchronizerName,
		_T("http://microarea.it/DataSynchronizer/"),
		AfxGetLoginManager()->GetServer(),
		AfxGetCommonClientObjects()->GetServerConnectionInfo()->m_nWebServicesPort
	);


	m_SynchDate.SetFullDate();

	m_pSqlSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();

	swprintf_s(m_strImgOk, szNamespace, szGlyphF, szOK);
	swprintf_s(m_strImgWait, szNamespace, szGlyphF, szIconWarning);
	swprintf_s(m_strImgError, szNamespace, szGlyphF, szWarning);
	swprintf_s(m_strImgExcluded, szNamespace, szGlyphF, szGlyphRemove);
}

//-----------------------------------------------------------------------------
void CDNotification::SetCollapsedValidationTile(BOOL bCollapsed)
{
	if (!m_pValidationTileDlg)
		m_pValidationTileDlg = GetServerDoc()->GetTileDialog(IDD_DATA_VALIDATION_STATUS);
	m_pValidationTileDlg->SetCollapsed(bCollapsed);
}

//-----------------------------------------------------------------------------
void CDNotification::SetCollapsedHistoryTile(BOOL bEnable)
{
/*

	if (!m_pHistoryTileDlg)
		m_pHistoryTileDlg = GetTile(IDD_DATA_SYNCHRO_HISTORY);
	m_pHistoryTileDlg->SetCollapsed(bEnable);
	*/
}

//-----------------------------------------------------------------------------
CDNotification::~CDNotification()
{
	SAFE_DELETE(m_pDataSynchronizer);
//	SAFE_DELETE(m_pTRSynchroInfo);
	SAFE_DELETE(m_pDMSSynchroMng);
//	SAFE_DELETE(m_pDataSynchroPane);

	if (m_pSqlSession)
	{
		m_pSqlSession->Close();
		SAFE_DELETE(m_pSqlSession);
	}

	SAFE_DELETE(m_pTRValidationInfo);
//	SAFE_DELETE(m_pTRSynchronizationInfo);
//	SAFE_DELETE(m_pTRSynchronizationInfoDMS);
}

//-----------------------------------------------------------------------------
BOOL CDNotification::OnAttachData()
{

	if (GetServerDoc()->IsInUnattendedMode())
		m_bDataSynchroEnabled = FALSE;

	m_strDocNamespace = m_pServerDocument->GetNamespace().ToString();
	m_pSqlConnection = m_pSqlSession->GetSqlConnection();
	//INSERT INTO DS_ActionsLog  (TBGuid , DocNamespace, ActionType, ActionData, SynchDirection, SynchXMLData, SynchStatus, SynchMessage, TBCreatedID, TBModifiedID, providerID)
	// VALUES (....)

//	m_pTRSynchroInfo = new TRDS_SynchronizationInfo(m_pServerDocument);
	
//considero il primo provider valido
	for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
	{
		CSynchroProvider* pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i);
		if (pSynchroProvider->IsValid())
		{
			if (m_CurrProviderName.IsEmpty())
				m_CurrProviderName = pSynchroProvider->m_Name;

			if (AfxGetIDataSynchroManager()->NeedMassiveSynchro(pSynchroProvider->m_Name))
				m_arNeedMassiveSynchroProviders.Add(pSynchroProvider->m_Name);

			if (m_FirstValidProvider.IsEmpty())
				m_FirstValidProvider = m_CurrProviderName;
		}
	}
	/*
	m_pDBTActionsLog = new DBTActionsLog(RUNTIME_CLASS(TEnhDS_ActionsLog), m_pServerDocument);
	m_pDBTActionsLog->InstantiateFromClientDoc(this);


	GetServerDoc()->m_pDBTMaster->Attach(m_pDBTActionsLog);
	*/
	//verifico se sul documento server esistono dei filtri relativi alla sincronizzazione dei dati
	//questi vengono gestiti da un clientdoc derivato da CDFilterManager
	CClientDoc* pCDFilter = GetServerDoc()->GetClientDoc(RUNTIME_CLASS(CDFilterManager));
	m_pCDSynchroFilter = (pCDFilter) ? (CDFilterManager*)pCDFilter : NULL;

	//manage json variables
	ManageJsonVariables();

	m_pTRValidationInfo = new TRDS_ValidationInfo(GetServerDoc());
	m_pTRValidationInfo->SetForceQuery(TRUE);
	/*
	m_pTRSynchronizationInfo = new TRDS_SynchronizationInfo(GetServerDoc());
	m_pTRSynchronizationInfo->SetForceQuery(TRUE);

	m_pTRSynchronizationInfoDMS = new TRDS_SynchronizationInfo(GetServerDoc());
	m_pTRSynchronizationInfoDMS->SetForceQuery(TRUE);
	*/

	m_bImagoStudioRuntimeInstalled = AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled();

	DECLARE_VAR_CD_JSON(bImagoStudioRuntimeInstalled);
	DECLARE_VAR_CD_JSON(bDataSynchroEnabled);

	return TRUE;
}

//--------------------------------------------------------------------------------------
void CDNotification::DoReloadSynchroInfo(BOOL bFromTimer /* = FALSE */)
{	
	SetSynchroData(bFromTimer);
	SetValidationData();
	ReloadSynchronizationInfo();
	/*
	if (!m_pServerDocument->IsInUnattendedMode() && !m_pServerDocument->IsABackgroundADM() && m_pDataSynchroPane)
		m_pDataSynchroPane->GetDataSynchroPaneView()->OnUpdateControls();
		*/
	if (!m_pServerDocument->IsInUnattendedMode() && !m_pServerDocument->IsABackgroundADM())
	{
		((CTileDialog*)GetServerDoc()->GetTileDialog(IDD_DATA_SYNCHRO_HISTORY_IMS_HINTS))->OnUpdateControls();
		((CTileDialog*)GetServerDoc()->GetTileDialog(IDD_DATA_SYNCHRO_HISTORY_IMS))->OnUpdateControls();
	}
}

//--------------------------------------------------------------------------------------
BOOL CDNotification::OnPrepareAuxData()
{
	SetSynchroData();
	SetValidationData();
	ReloadSynchronizationInfo();
	return TRUE;
}

//---------------------------------------------------------------------------------------
void CDNotification::EnableAllTileControls(CBaseTileDialog* pTile, BOOL bEnable)
{
	if (!pTile)
		return;

	((CTileDialog*)pTile)->EnableTileDialogControlLinks(bEnable);
}

//---------------------------------------------------------------------------------------
void CDNotification::ShowTile(CBaseTileDialog* pTile, BOOL bShow)
{
	if (!pTile)
		return;

	pTile->Show(bShow);
}

//---------------------------------------------------------------------------------------
CBaseTileDialog* CDNotification::GetTile(UINT nIDD)
{
	if(!m_pDataSynchroPane)
		return NULL;

	CDataSynchroClientView* pView = m_pDataSynchroPane->GetDataSynchroPaneView();
	CBaseTileDialog* pTile = NULL;

	if (!pView)
		return NULL;

	pTile = pView->GetTileDialog(nIDD);

	return pTile;
}

//---------------------------------------------------------------------------------
void CDNotification::ManageJsonVariables()
{
	GetServerDoc()->DeclareVariable(_T("SynchDate"),				m_SynchDate);
	GetServerDoc()->DeclareVariable(_T("SynchWorker"),				m_SynchWorker);
	GetServerDoc()->DeclareVariable(_T("SynchDirection"),			m_SynchDirection);
	GetServerDoc()->DeclareVariable(_T("SynchMsg"),					m_SynchMsg);
	GetServerDoc()->DeclareVariable(_T("SynchStatus"),				m_SynchStatus);
	GetServerDoc()->DeclareVariable(_T("SynchStatusPicture"),		m_SynchStatusPicture);
	GetServerDoc()->DeclareVariable(_T("CurrProviderName"),			m_CurrProviderName);
	GetServerDoc()->DeclareVariable(_T("ValidationStatus"),			m_ValidationStatus);
	GetServerDoc()->DeclareVariable(_T("ValidationStatusPicture"),	m_ValidationStatusPicture);
	GetServerDoc()->DeclareVariable(_T("SynchStatusHints"),			m_SynchStatusHints);

//	GetServerDoc()->RegisterControl(IDC_DS_ACTIONS_BE,		RUNTIME_CLASS(CActionsBodyEdit));
}

//-----------------------------------------------------------------------------
BOOL CDNotification::ProviderNeedMassiveSynchro(const CString& strProviderName) const
{
	for (int i = 0; i < m_arNeedMassiveSynchroProviders.GetSize(); i++)
		if (strProviderName.CompareNoCase(m_arNeedMassiveSynchroProviders.GetAt(i)) == 0)
			return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
void CDNotification::ReloadSynchronizationInfo()
{
/*
	if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		m_pDBTActionsLog->RemoveAll();
		m_pDBTActionsLog->LocalFindData();
	}
	*/
}

//-----------------------------------------------------------------------------
CAbstractFormDoc::LockStatus CDNotification::OnLockDocumentForEdit() 
{ 
	return CAbstractFormDoc::ALL_LOCKED; 
}

//-----------------------------------------------------------------------------
CAbstractFormDoc::LockStatus CDNotification::OnLockDocumentForDelete() 
{ 
	return CAbstractFormDoc::ALL_LOCKED; 
}

//restituisce il documento che ha dato origine all'eventuale catena di documenti annidati (vedi fatturazione ed ADM)
//se non ci sono documenti annidati allora è il ServerDoc
//-----------------------------------------------------------------------------
CAbstractFormDoc* CDNotification::GetFirsDocument(CAbstractFormDoc* pDoc) const
{
	if (!pDoc->GetMyAncestor())
		return pDoc;
	
	return GetFirsDocument(pDoc->GetMyAncestor());
}

//-----------------------------------------------------------------------------
void CDNotification::AddNotificationInfo(CNotificationInfo* pNotificationInfo)
{
	CAbstractFormDoc* pFirstDocument = GetFirsDocument(GetServerDoc());

	if (!pFirstDocument) //la vedo dura
		return;

	CDataSynchroNotifier* pDataSynchroMng = (CDataSynchroNotifier*)pFirstDocument->GetDataSynchroManager();
	if (!pDataSynchroMng)
	{
		pDataSynchroMng = new CDataSynchroNotifier(pFirstDocument, m_pDataSynchronizer, m_pSqlConnection);
		pDataSynchroMng->SetInfo(m_pDataSynchronizer);
		pFirstDocument->SetDataSynchroManager(pDataSynchroMng);
	}
	pDataSynchroMng->AddNotificationInfo(pNotificationInfo);	
	//nel caso di documento interattivo proprietario del contesto allora devo far partire subito la notifica al datasynchronizer poichè
	//la commit è già stata fatta e la OnExtraTransaction del clientdocument è stata chiamata dopo
	//stessa cosa per gli eventi del DMS che possono essere lanciati anche quando il documento è in stato di browse
	if (pFirstDocument == GetServerDoc() && !GetServerDoc()->m_pTbContext->TransactionPending())
		pDataSynchroMng->NotifiyToDataSynchronizer();
}

//-----------------------------------------------------------------------------
 DataLng CDNotification::InsertLogAction(const DataEnum& synchStatus, const DataEnum& actionType, const DataStr& actionData, const DataStr& providerName, CSynchroDocInfo* pDocInfo)
{	 
	 
	 SqlRecord* pRecord = (actionType == E_SYNCHROACTION_TYPE_DELETE) ? GetServerDoc()->m_pDBTMaster->GetOldRecord() : GetMasterRec();	
	 CNotificationInfo* pNotificationInfo = new CNotificationInfo(pRecord);
	 pNotificationInfo->m_SynchStatus = synchStatus;
	 pNotificationInfo->m_ActionType = actionType;
	 pNotificationInfo->m_ActionData = actionData;
	 pNotificationInfo->m_ProviderName = providerName;

	 CString sTableName = pRecord->GetTableName();

	 pNotificationInfo->m_strTableName = pRecord->GetTableName();
	 pNotificationInfo->m_strDocNamespace = m_strDocNamespace;
	 pNotificationInfo->m_DocGuid = pRecord->f_TBGuid; 
	 pNotificationInfo->m_strOnlyForDMS = pDocInfo->m_OnlyForDMS;
	 pNotificationInfo->m_strIMagoConfigurations = pDocInfo->m_iMagoConfigurations;

	 AddNotificationInfo(pNotificationInfo);
 
	 return TRUE;

}

 //-----------------------------------------------------------------------------
BOOL CDNotification::OnOkTransaction()
{
	if (GetServerDoc()->GetFormMode() == CAbstractFormDoc::EDIT)
	{
		if (GetMasterRec()->HasGUID() && GetMasterRec()->f_TBGuid.IsEmpty())
			GetMasterRec()->f_TBGuid.AssignNewGuid();
	}

	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CDNotification::NotifyAction(const DataEnum& actionType, const DataStr& actionData)
{	
	if (m_pServerDocument->IsImporting() && m_pServerDocument->GetXMLDataManager()->GetProfileName() == _T("InfinitySyncroConnector"))
		return FALSE;


	SqlRecord* pRecord = (actionType == E_SYNCHROACTION_TYPE_DELETE) ? GetServerDoc()->m_pDBTMaster->GetOldRecord() : GetMasterRec();
	CString sTableName = pRecord->GetTableName();

	DataGuid docGuid = pRecord->f_TBGuid;
	if (docGuid.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	
	//bisogna inserire una riga per ogni provider
	BOOL bOK = FALSE;
	for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
	{
		CSynchroProvider* pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i);
		//queste azioni le eseguo solo se il provider è valido e non è di tipo DMS
		if (!pSynchroProvider->IsValid() || pSynchroProvider->m_IsDMSProvider)
			continue;

		//non ho ancora eseguito la massiva allora non devo fare la notify
		if (ProviderNeedMassiveSynchro(pSynchroProvider->m_Name))
			continue;

		BOOL bContinue = TRUE;
		CSynchroDocInfo* pDocInfo = pSynchroProvider->GetDocumentsToSynch()->GetDocumentByNs(GetServerDoc()->GetNamespace().ToString());

		if (!pDocInfo)
			continue;

		bContinue = ((pDocInfo->m_wActionMode & (NOTIFY_DELETE | NOTIFY_UPDATE | NOTIFY_INSERT)) == (NOTIFY_DELETE | NOTIFY_UPDATE | NOTIFY_INSERT));

		if (!bContinue)
		{
			if (actionType == E_SYNCHROACTION_TYPE_INSERT)
				bContinue = ((pDocInfo->m_wActionMode & NOTIFY_INSERT) == NOTIFY_INSERT);
			if (actionType == E_SYNCHROACTION_TYPE_UPDATE)
				bContinue = ((pDocInfo->m_wActionMode & NOTIFY_UPDATE) == NOTIFY_UPDATE);
			if (actionType == E_SYNCHROACTION_TYPE_DELETE)
				bContinue = ((pDocInfo->m_wActionMode & NOTIFY_DELETE) == NOTIFY_DELETE);
		}

		if (!bContinue)
			continue;

		/*  TODO: DA VERIFICARE!
		BOOL bRecordFind = m_pTRSynchroInfo->FindRecord(docGuid, pSynchroProvider->m_Name) == TableUpdater::FOUND;

		//se sul documento ci sono dei filtri di sincronizzazione ed il record corrente non verifica questi filtri allora non devo notificare nulla al DataSynchronizer	
		if (m_pCDSynchroFilter)
		{
			//se non verifica la condizione di sincronizzazione ma il record risulta essere già sincronizzato allora devo dare la notifica di cancellazione 
			if (!m_pCDSynchroFilter->CheckFilterCondition(pRecord, pSynchroProvider->m_Name))
			{
				//il record risulta sincronizzato con il programma esterno. Lo cancello dal programma esterno e metto lo stato ad IGNORE in modo da non considerarlo 
				// il tutto solo se sono in stato di UPDATE
				if (actionType == E_SYNCHROACTION_TYPE_UPDATE && bRecordFind && m_pTRSynchroInfo->GetRecord()->f_SynchStatus != E_SYNCHROSTATUS_TYPE_EXCLUDED)
					InsertLogAction(E_SYNCHROSTATUS_TYPE_TOSYNCHRO, E_SYNCHROACTION_TYPE_EXCLUDE, _T(""), pSynchroProvider->m_Name, pDocInfo);
				else
					continue;
			}
		}
		*/

		//altrimenti è da sincronizzare
		InsertLogAction(E_SYNCHROSTATUS_TYPE_TOSYNCHRO, actionType, actionData, pSynchroProvider->m_Name, pDocInfo);

	}
	return TRUE;
}

//-----------------------------------------------------------------------------
void CDNotification::DoOpenDockPanel(BOOL bOpen /* = TRUE */)
{
	if(m_pDataSynchroPane)
		m_pDataSynchroPane->SetAutoHideMode(FALSE, CBRS_RIGHT | CBRS_HIDE_INPLACE, 0, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CDNotification::OnExtraNewTransaction()
{ 
	if (!GetMasterRec())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	// Nel caso in cui l'entità inserita fosse presente nella tabella DS_ValidationFKToFix
	// (la mancanza di questa entità creava problemi di Foreign key)
	// deve essere rimossa (perchè con questo inserimento quei problemi sono stati risolti)
	DeleteFromValidationFKToFix();
	return NotifyAction(DataEnum(E_SYNCHROACTION_TYPE_INSERT), DataStr());
}

//-----------------------------------------------------------------------------
void UnparseRow(CXMLNode* pNode, SqlRecord* pCurrentRow, const CUIntArray& aKIndexes)
{
	SqlRecordItem* pRecItem = NULL;
	CXMLNode* pRow = NULL;
		
	if (pCurrentRow)
	{
		pRow = pNode->CreateNewChild(_T("Row")); 
		for (int nKey = 0; nKey < aKIndexes.GetSize(); nKey++)
		{
			pRecItem = pCurrentRow->GetAt(nKey);
			if (pRecItem)
				pRow->SetAttribute(pRecItem->GetColumnName(), pRecItem->GetDataObj()->FormatDataForXML());
		}

		//se nel record è bindato anche il tbguid (vedi caso TCustSuppPeople) allora aggiungo il valore del TBGuid
		if (pCurrentRow->HasGUID())
			pRow->SetAttribute(GUID_COL_NAME, pCurrentRow->f_TBGuid.FormatDataForXML());			

	}
}

//nel campo f_ActionData sono inseriti i dati relativi ai soli inserimenti, modifiche o cancellazioni alle righe dei dbtslavebuffered coinvolti 
//poichè per il provider InfinityCRM è necessario indicare per ogni riga l'azione da eseguire (inserimento, modifica o cancellazione)
//nel modifica del documento
//Per quanta riguarda i DBTMaster ed i DBTSlave i dati sono sempre considerati per il processo di sincronizzazione (@@BAUZI da ottimizzare)????
//-----------------------------------------------------------------------------
BOOL CDNotification::OnExtraEditTransaction() 
{ 
	//CString editData;

	//Al momento non serve
	//DBTMaster* pDBTMaster = GetServerDoc()->m_pDBTMaster;

	//CXMLDocumentObject aXMLDocument(TRUE, FALSE);
	//aXMLDocument.CreateRoot(_T("UpdatedDbtS"));

	////loop tra i dbtslavebuffered. Per quelli modificati vado a prendere le informazioni di inserimento/modifica/cancellazione riga
	//for (int i = 0 ; i < pDBTMaster->GetDBTSlaves()->GetSize(); i++)
	//{
	//	//verifico 
	//	DBTSlave* pDBTSlave = pDBTMaster->GetDBTSlaves()->GetAt(i);
	//	if (pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTSlaveBuffered)) && pDBTSlave->GetUpdated()) 
	//	{
	//		if (pDBTSlave->IsKindOf(RUNTIME_CLASS(DBTActionsLog)))
	//			continue; 
	//		DBTSlaveBuffered* pDBTSlaveBuff = (DBTSlaveBuffered*)pDBTSlave;
	//		CXMLNode* pDBTNodeNode = aXMLDocument.CreateRootChild(_T("UpdatedDbt"));
	//		pDBTNodeNode->SetAttribute(_T("tablename"), pDBTSlaveBuff->GetTable()->GetTableName());
	//		SqlRecord* pCurrentRow = NULL;
	//		CXMLNode* pRowsNode = NULL;
	//		const CUIntArray& aKIndexes = pDBTSlaveBuff->GetRecord()->GetPrimaryKeyIndexes();		
	//		int nRow = 0;

	//		//due casi:
	//		// 1. prima della modifica il DBTSlaveBuffered era vuoto. I record presenti sono tutti in stato di new			
	//		if (pDBTSlaveBuff->GetOldSize() == 0)
	//		{
	//			pRowsNode = pDBTNodeNode->CreateNewChild(_T("NewRows"));		
	//			for (int j = 0 ; j < pDBTSlaveBuff->GetRecords()->GetSize(); j++)
	//			{
	//				pCurrentRow = pDBTSlaveBuff->GetRow(j);
	//				UnparseRow(pRowsNode, pCurrentRow, aKIndexes);			
	//			}
	//				continue;
	//		}

	//		// 2. prima della modifica DBTSlaveBuffered conteneva già delle righe
	//		//utilizzo gli array m_pwNewRows, m_pwModifiedRows, m_pwDeletedRows per indicare al provider le righe inserite, modificate o cancellate
	//		//righe aggiunte			
	//		if (pDBTSlaveBuff->m_pwNewRows)	
	//		{
	//			pRowsNode = pDBTNodeNode->CreateNewChild(_T("NewRows"));		
	//			for (int j = 0 ; j < pDBTSlaveBuff->m_pwNewRows->GetSize(); j++)
	//			{
	//				pCurrentRow = pDBTSlaveBuff->GetRow(pDBTSlaveBuff->m_pwNewRows->GetAt(j));
	//				UnparseRow(pRowsNode, pCurrentRow, aKIndexes);	
	//			}
	//		}

	//		//righe modificate
	//		if (pDBTSlaveBuff->m_pwModifiedRows)	
	//		{
	//			pRowsNode = pDBTNodeNode->CreateNewChild(_T("ModifiedRows"));		
	//			for (int j = 0 ; j < pDBTSlaveBuff->m_pwModifiedRows->GetSize(); j++)
	//			{
	//				pCurrentRow = pDBTSlaveBuff->GetRow(pDBTSlaveBuff->m_pwModifiedRows->GetAt(j));
	//				UnparseRow(pRowsNode, pCurrentRow, aKIndexes);	
	//			}
	//		}

	//		//righe cancellate
	//		if (pDBTSlaveBuff->m_pwDeletedRows)	
	//		{
	//			pRowsNode = pDBTNodeNode->CreateNewChild(_T("DeletedRows"));		
	//			for (int j = 0 ; j < pDBTSlaveBuff->m_pwDeletedRows->GetSize(); j++)
	//			{
	//				pCurrentRow = pDBTSlaveBuff->GetOldRow(pDBTSlaveBuff->m_pwDeletedRows->GetAt(j)); //vado sull'oldrecords
	//				UnparseRow(pRowsNode, pCurrentRow, aKIndexes);
	//			}
	//		}
	//	}
	//}

	//aXMLDocument.GetXML(editData);

	if (!GetMasterRec())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return NotifyAction(DataEnum(E_SYNCHROACTION_TYPE_UPDATE), DataStr());
}

//-----------------------------------------------------------------------------
BOOL CDNotification::OnExtraDeleteTransaction()
{
	NotifyDeleteRecordForValidation();
	return TRUE;
}

// Ricerca e cancellazione record per validazione puntuale in seguito ad evento di delete
/*	(nel campo f_ActionData sono inseriti i dati relativi alla cancellazione ovvero quelli che permettono al provider di fornire le informazioni giuste al programma
	esterno per poter cancellare in modo corretto il proprio documento
)*/
//-----------------------------------------------------------------------------
void CDNotification::NotifyDeleteRecordForValidation()
{
	SqlRecord* pMasterOldRec = GetServerDoc()->GetMaster()->GetOldRecord();
	// Cancello record dalla tabella DS_ValidationInfo
	TUDS_ValidationInfo m_TUDS_ValidationInfo(m_pServerDocument);
	m_TUDS_ValidationInfo.SetSqlSession(m_pSqlSession);
	m_TUDS_ValidationInfo.SetAutocommit();
	if (m_TUDS_ValidationInfo.FindRecord(pMasterOldRec->f_TBGuid, m_CurrProviderName, TRUE) == TableUpdater::FOUND)
	{
		m_TUDS_ValidationInfo.DeleteRecord();
		m_TUDS_ValidationInfo.UnlockCurrent();
	}
}

//notifica le azioni proprio di EA ai soli provider di tipo EA
//-----------------------------------------------------------------------------
BOOL CDNotification::NotifyEasyAttachmentAction(const DataEnum& actionType, const DataStr& actionData)
{
	SqlRecord* pRecord = GetMasterRec();
	CString sTableName = pRecord->GetTableName();
	DataGuid docGuid = pRecord->f_TBGuid;

	// se il TBGuid del current Record e' vuoto allora vado a vedere l'OldRecord
	// NON mi baso sull'actionType = DeleteAttachment perche' questo evento viene ruotato sia in fase
	// di cancellazione del solo allegato, che in coda alla cancellazione del documento gestionale e relativi allegati
	if (docGuid.IsEmpty())
	{
		pRecord = GetServerDoc()->m_pDBTMaster->GetOldRecord();
		if (pRecord == NULL || pRecord->f_TBGuid.IsEmpty())
		{
			ASSERT(FALSE);
			return FALSE;
		}
	}

	if (actionData.IsEmpty())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	//bisogna inserire una riga per ogni provider di tipo DMS
	BOOL bOK = FALSE;
	for (int i = 0; i < AfxGetDataSynchroManager()->GetSynchroProviders()->GetSize(); i++)
	{	
		CSynchroProvider* pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(i);
		//queste azioni le eseguo solo se il provider è valido e non è di tipo DMS
		if (!pSynchroProvider->IsValid() || !pSynchroProvider->m_IsDMSProvider)
			continue;

		CSynchroDocInfo* pDocInfo = NULL;
	/*
		if (!AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			pDocInfo = pSynchroProvider->GetDocumentsToSynch()->GetDocumentByNs(GetServerDoc()->GetNamespace().ToString());
		else
		*/
		if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			pDocInfo = pSynchroProvider->GetDocumentsToSynchImago()->GetDocumentByNs(GetServerDoc()->GetNamespace().ToString());

		if (!pDocInfo)
			continue;
		
		//se sul documento ci sono dei filtri di sincronizzazione ed il record corrente non verifica questi filtri allora non devo notificare nulla al DataSynchronizer	
		if (m_pCDSynchroFilter)			
		{
			//se non verifica la condizione di sincronizzazione ma il record risulta essere già sincronizzato allora devo dare la notifica di cancellazione 
			//prima devo verificare se il documento è sincronizzabile per l'eventuale provider padre (vedi caso CRM + DMS infinity Connector entrambi abilitati: non devo sincronizzare 
			//gli allegati di EA se il documento gestionale non è sincronizzabile per il CRM)
			if (pSynchroProvider->m_pParentSynchroProvider && !m_pCDSynchroFilter->CheckFilterCondition(pRecord, pSynchroProvider->m_pParentSynchroProvider->m_Name))
				continue;	
		}
		InsertLogAction(E_SYNCHROSTATUS_TYPE_TOSYNCHRO, actionType, actionData, pSynchroProvider->m_Name, pDocInfo);				
		
	}
	
	if (m_pDMSSynchroMng && actionType == E_SYNCHROACTION_TYPE_UPDCOLLECTION)
		m_pDMSSynchroMng->SetCollectionSynchronized();

	return TRUE;
}

//-----------------------------------------------------------------------------
void CDNotification::OnDMSEvent(DMSEventTypeEnum eventType, int eventKey)
{
	if (AfxGetDataSynchroManager()->NeedMassiveSynchro(DMSInfinityProvider))
		return;
	
	if (!m_pDMSSynchroMng)
		m_pDMSSynchroMng = new DMSSynchroManager(GetServerDoc());

	CXMLDocumentObject aXMLDocument(TRUE, FALSE);

	CString notifyData = m_pDMSSynchroMng->GetXMLSynchroData(eventType, eventKey);
	CXMLNode* pNode = NULL;

	switch (eventType)
	{
	/*	case NewCollection:	
			NotifyEasyAttachmentAction(DataEnum(E_SYNCHROACTION_TYPE_NEWCOLLECTION), DataStr(notifyData));
			break;*/

		case UpdateDMSCollection:
			NotifyEasyAttachmentAction(DataEnum(E_SYNCHROACTION_TYPE_UPDCOLLECTION), DataStr(notifyData));
			break;

		case NewDMSAttachment:
			{
				BOOL bSynchroColl = TRUE;
				if (m_pDMSSynchroMng->NeedToSynchroCollection())
				{
					CAttachmentInfo* pAttInfo = AfxGetIDMSRepositoryManager()->GetAttachmentInfo(eventKey);
					CString collNotifyData = m_pDMSSynchroMng->GetXMLSynchroData(NewDMSCollection, pAttInfo->m_CollectionId);
					bSynchroColl = NotifyEasyAttachmentAction(E_SYNCHROACTION_TYPE_UPDCOLLECTION, DataStr(collNotifyData));
					delete pAttInfo;
				}
				if (bSynchroColl)				
					NotifyEasyAttachmentAction(DataEnum(E_SYNCHROACTION_TYPE_NEWATTACHMENT), DataStr(notifyData));
				break;
			}

		case DeleteDMSAttachment:
			NotifyEasyAttachmentAction(DataEnum(E_SYNCHROACTION_TYPE_DELATTACHMENT), DataStr(notifyData));
			break;
	}
}

//----------------------------------------------------------------------------
void CDNotification::Customize()
{	/*
	CMasterFrame* pFrame = dynamic_cast<CMasterFrame*>(GetServerDoc()->GetFrame());

	if (pFrame)
	{
		m_pDataSynchroPane = new CDataSynchroPane(this);

		pFrame->CreateDockingPane
				(
					m_pDataSynchroPane,
					IDD_DATASYNCHRO_VIEW,
					_T("DataSynchroClientDetails"),
					_TB("Data Synchronizer"),
					CBRS_RIGHT,
					CSize(420, 360)
				);
		m_pDataSynchroPane->EnableToolbar(25, TRUE);
		m_pDataSynchroPane->GetToolBar()->SetAutoHideToolBarButton(FALSE);
		m_pDataSynchroPane->GetToolBar()->ShowInDialog(m_pDataSynchroPane);

		m_pAutoHideBar = m_pDataSynchroPane->SetAutoHideMode(TRUE, CBRS_RIGHT | CBRS_HIDE_INPLACE, 0, TRUE);
	}
	*/
}

//----------------------------------------------------------------------------
void CDNotification::OnForceSynchronization()
{
	if (!GetMasterRec())
	{
		ASSERT(FALSE);
		return;
	}

	if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{
		m_SynchStatusPicture = m_strImgWait;	//szSynchStatusWait;
		m_SynchMsg = _TB("The provider is synchronizing this element.");
		m_SynchStatusHints = _TB("The provider is synchronizing this element.");
		//m_pDataSynchroPane->GetDataSynchroPaneView()->OnUpdateControls();
		CTileDialog * pTile = ((CTileDialog*)GetServerDoc()->GetTileDialog(IDD_DATA_SYNCHRO_HISTORY_IMS_HINTS));
		if (pTile)
			pTile->OnUpdateControls();
		pTile = ((CTileDialog*)GetServerDoc()->GetTileDialog(IDD_DATA_SYNCHRO_HISTORY_IMS));
		if (pTile)
			pTile->OnUpdateControls();
	}

	// da verificare

//	if (m_CurrProviderName.IsEqual(DataStr(DMSInfinityProvider)))
//	{
		NotifyAction(DataEnum(E_SYNCHROACTION_TYPE_UPDATE), DataStr());

		if (!m_pDMSSynchroMng)
			m_pDMSSynchroMng = new DMSSynchroManager(GetServerDoc());			
			
		CAttachmentsArray* m_pAttachments = AfxGetIDMSRepositoryManager()->GetAttachments(GetServerDoc()->GetNamespace().ToString(), GetServerDoc()->m_pDBTMaster->GetRecord()->GetPrimaryKeyNameValue(), OnlyAttachment);
		CString notifyData;
		if (m_pAttachments && m_pAttachments->GetSize() > 0)
		{
			//prima cosa verifico se la collection è stata creata o meno
			BOOL bSynchroColl = TRUE;
			CAttachmentInfo* pAttachmentInfo = NULL;
			if (m_pDMSSynchroMng->NeedToSynchroCollection())
			{
				pAttachmentInfo = m_pAttachments->GetAt(0);
				notifyData = m_pDMSSynchroMng->GetXMLSynchroData(NewDMSCollection, pAttachmentInfo->m_CollectionId);
				bSynchroColl = NotifyEasyAttachmentAction(E_SYNCHROACTION_TYPE_UPDCOLLECTION, DataStr(notifyData));
			}
			if (bSynchroColl)
			{
				Array eventKeys;					
				for (int i =0; i <= m_pAttachments->GetUpperBound(); i++)
					eventKeys.Add(new DMSEventKey(m_pAttachments->GetAt(i)->m_attachmentID));											
				notifyData = m_pDMSSynchroMng->GetXMLSynchroData(NewDMSAttachment, &eventKeys);
				NotifyEasyAttachmentAction(DataEnum(E_SYNCHROACTION_TYPE_NEWATTACHMENT), DataStr(notifyData));
			}
		}
		
//		return;
//	}	

}

//---------------------------------------------------------------------------------------------
void CDNotification::OnReloadSynchroInfo()
{
	DoReloadSynchroInfo();
}

//--------------------------------------------------------------------------------------------------
void CDNotification::OnProviderChanged()
{
	if (m_pProviderCombo)
	{
		int nSel = m_pProviderCombo->GetCurSel();

		CString currString;
		m_pProviderCombo->GetLBText(nSel, currString);

		if (!m_CurrProviderName.IsEqual(DataStr(currString)))
		{
			m_CurrProviderName.Assign(currString);
			DoReloadSynchroInfo();
		}
	}
}

//----------------------------------------------------------------------------
void CDNotification::OnForceValidation()
{
	SqlRecord* pRecord = GetServerDoc()->GetMaster()->GetRecord();
	CNotificationInfo* pNotificationInfo = new CNotificationInfo(pRecord);
	CSynchroProvider* pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(0);

	pNotificationInfo->m_SynchStatus = E_SYNCHROSTATUS_TYPE_TOSYNCHRO;
	pNotificationInfo->m_ActionType = E_SYNCHROACTION_TYPE_UPDATE;
	pNotificationInfo->m_ActionData = _T("");
	pNotificationInfo->m_ProviderName = pSynchroProvider->m_Name;
	pNotificationInfo->m_strTableName = pRecord->GetTableName();
	pNotificationInfo->m_strDocNamespace = m_strDocNamespace;
	pNotificationInfo->m_DocGuid = pRecord->f_TBGuid; 
	
	CDataSynchroNotifier* pDataSynchroMng = (CDataSynchroNotifier*)GetServerDoc()->GetDataSynchroManager();
	if (!pDataSynchroMng)
	{
		pDataSynchroMng = new CDataSynchroNotifier(GetServerDoc(), m_pDataSynchronizer, m_pSqlConnection);
		GetServerDoc()->SetDataSynchroManager(pDataSynchroMng);
	}

	pDataSynchroMng->ValidateNotification(pNotificationInfo);

	SetValidationData(TRUE);

	/*
	if (!m_pServerDocument->IsInUnattendedMode() && !m_pServerDocument->IsABackgroundADM() && m_pDataSynchroPane)
	{
		m_pDataSynchroPane->GetDataSynchroPaneView()->OnUpdateControls();
		SetCollapsedValidationTile(FALSE);
	}
	*/
	if (!m_pServerDocument->IsInUnattendedMode() && !m_pServerDocument->IsABackgroundADM())
	{
		CTileDialog * pTile = (CTileDialog*)GetServerDoc()->GetTileDialog(IDD_DATA_VALIDATION_STATUS);
		pTile->OnUpdateControls();
		SetCollapsedValidationTile(FALSE);
	}
}

//----------------------------------------------------------------------------
CString CDNotification::GetValidationStatus()
{
	CString msg = _T("");
	CSynchroProvider* pSynchroProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetAt(0);
	if (!pSynchroProvider)
		return msg;

	if (m_pTRValidationInfo->FindRecord(GetMasterRec()->f_TBGuid, pSynchroProvider->m_Name) == TableReader::FOUND)
	{
		TDS_ValidationInfo* pRec = m_pTRValidationInfo->GetRecord();
		msg = pRec->f_MessageError;

		RICheckNode* pRoot = AfxGetApplicationContext()->GetObject<RICheckNode>();
		msg = pRoot->DisplayErrors(msg);
	}
/*	else
		msg = _T("NOT_FOUND");
		*/

	return msg;
}

//----------------------------------------------------------------------------
void CDNotification::DeleteFromValidationFKToFix()
{
	CString		strTableName	= m_pServerDocument->m_pDBTMaster->GetTable()->m_strTableName;
	SqlRecord*	pCurrentRec		= m_pServerDocument->m_pDBTMaster->GetRecord();

	if (!pCurrentRec)
		return;

	//select DocNamespace, TableName, fieldname 
	//from DS_ValidationFKtoFix 
	//Where DocNamespace = 'Document.ERP.Banks.Documents.CustSuppBanks'
	//group by  DocNamespace ,TableName, fieldname 

	TDS_ValidationFKtoFix aRecValidationFKtoFix;

	SqlTable aTbl(&aRecValidationFKtoFix);
	aTbl.SetSqlSession(m_pSqlSession);

	DataStr sFieldName  = _T("");
	DataStr sValueToFix = _T("");

	TRY
	{
		aTbl.Open();
		aTbl.Select(aRecValidationFKtoFix.f_DocNamespace);	
		aTbl.Select(aRecValidationFKtoFix.f_TableName);	
		aTbl.Select(aRecValidationFKtoFix.f_FieldName);	
		aTbl.FromTable(&aRecValidationFKtoFix);

		aTbl.AddParam		(szParamDocNamespace, aRecValidationFKtoFix.f_DocNamespace);
		aTbl.AddFilterColumn(aRecValidationFKtoFix.f_DocNamespace);
		aTbl.SetParamValue	(szParamDocNamespace, (DataStr)m_pServerDocument->GetNamespace().ToString());

		aTbl.AddParam		(szParamTableName, aRecValidationFKtoFix.f_TableName);
		aTbl.AddFilterColumn(aRecValidationFKtoFix.f_TableName);
		aTbl.SetParamValue	(szParamTableName, (DataStr)strTableName);

		aTbl.AddGroupByColumn(aRecValidationFKtoFix.f_DocNamespace);	
		aTbl.AddGroupByColumn(aRecValidationFKtoFix.f_TableName);	
		aTbl.AddGroupByColumn(aRecValidationFKtoFix.f_FieldName);	

		aTbl.Query();

		if (!aTbl.IsEOF())
			sFieldName = aRecValidationFKtoFix.f_FieldName.Str();
			
		aTbl.Close();
	}
	CATCH(SqlException, e)	
	{
		aTbl.Close();
		ASSERT(FALSE);
	}
	END_CATCH	

	if (!sFieldName.IsEmpty())
	{
		sValueToFix = *(DataStr*)pCurrentRec->GetDataObjFromColumnName(sFieldName);

		if (!sValueToFix.IsEmpty())
		{
			TUDS_ValidationFKtoFix aTUDS_ValidationFKtoFix(m_pServerDocument);
			aTUDS_ValidationFKtoFix.SetSqlSession(m_pSqlSession);
			aTUDS_ValidationFKtoFix.SetAutocommit();
			if	(
					aTUDS_ValidationFKtoFix.FindRecord	(
															m_CurrProviderName, 
															GetServerDoc()->GetNamespace().ToString(), 
															strTableName,
															sFieldName,
															sValueToFix,
															TRUE
														) == TableUpdater::FOUND
				)
			{
				aTUDS_ValidationFKtoFix.DeleteRecord();
				aTUDS_ValidationFKtoFix.UnlockCurrent();
			}
		}
	}
}

//----------------------------------------------------------------------------
void CDNotification::OnUpdateForceSynchronization(CCmdUI* pCmdUI)
{
	CAbstractFormDoc* pServerDoc = GetServerDoc();
	
	BOOL bEnable = pServerDoc && GetServerDoc()->ValidCurrentRecord() && pServerDoc->GetFormMode() != CAbstractFormDoc::NEW && !ProviderNeedMassiveSynchro(m_CurrProviderName);
	pCmdUI->Enable(bEnable);
}

//----------------------------------------------------------------------------
void CDNotification::OnUpdateForceValidation(CCmdUI* pCmdUI)
{
	CAbstractFormDoc* pServerDoc = GetServerDoc();
	
	BOOL bEnable = pServerDoc && GetServerDoc()->ValidCurrentRecord() && pServerDoc->GetFormMode() != CAbstractFormDoc::NEW;
	pCmdUI->Enable(bEnable);
}

//--------------------------------------------------------------------------------------
void CDNotification::OnUpdateRefresh(CCmdUI* pCmdUI)
{
	CAbstractFormDoc* pServerDoc = GetServerDoc();

	BOOL bEnable = pServerDoc && GetServerDoc()->ValidCurrentRecord();

	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
void CDNotification::OnUpdateOpenDataSynchroView(CCmdUI* pCmdUI)
{
	CAbstractFormDoc* pServerDoc = GetServerDoc();
	
	BOOL bEnable = pServerDoc && GetServerDoc()->ValidCurrentRecord() && pServerDoc->GetFormMode() != CAbstractFormDoc::NEW;
	pCmdUI->Enable(bEnable);
}

//-----------------------------------------------------------------------------
BOOL CDNotification::OnShowStatusBarMsg(CString& sMsg)
{ 
	sMsg += _T(" ") + m_SynchStatus;
	return TRUE;
}

//--------------------------------------------------------------------------------------
void CDNotification::ParseXMLLogs(const CString strProviderName, CString XmlLog,CString& strSynchStatusPicture, CString& strSynchStatusMsg)
{
	LPCTSTR pDummy;
	BOOL bResult = TRUE;
	CXMLDocumentObject aXMLModDoc;

	aXMLModDoc.EnableMsgMode(FALSE);

	if (XmlLog != _T(""))
	{
	TRY
		{
			BOOL bLoad = aXMLModDoc.LoadXML(XmlLog);
		if (!bLoad)
		{
			m_SynchMsg += cwsprintf(_T("{0-%s} : {1-%s}"), strProviderName, _TB("Unable to parse logs"));
			return;
		}

		CSynchroProvider * pProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(strProviderName);

		if (pProvider && pProvider->m_strImagoRequestGuid.LookupKey(GetMasterRec()->f_TBGuid.Str(), pDummy))
			pProvider->m_strImagoRequestGuid.RemoveKey(GetMasterRec()->f_TBGuid.Str());
		

		// root SynchroProfiles
		CXMLNode* pRoot = aXMLModDoc.GetRoot();
		// Provider
		CString strValue;
		BOOL	bOK = FALSE;
		CString strToken;
		DataEnum status;
		BOOL    bResult = TRUE;
		CString strErrorMsg = _T("");
		CString strFlow;
		CString strDir = _T("");
		m_SynchStatusHints = _TB("Yeah! Synchronization successfully done :) ");

		CXMLNode* pNode = pRoot->GetChildByName(_T("Flows"));

		if (pNode)
		{

			for (int i = 0; i < pNode->GetChilds()->GetSize(); i++)
			{
				CXMLNode* pFlowNode = pNode->GetChildAt(i);
				if (pFlowNode)
				{
					bOK = pFlowNode->GetAttribute(_T("name"), strFlow);
					if (bOK)
					{

						bOK = pFlowNode->GetAttribute(_T("Direction"), strDir);
						if (bOK)
							m_SynchDirection = strDir;

						CXMLNode* pSubNode = pFlowNode->GetChildByName(_T("Status"));

						if (pSubNode)
						{
							bOK = pSubNode->GetText(strToken);
							if (bOK)
							{
								status = strToken == _T("True") ? DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO) : DataEnum(E_SYNCHROSTATUS_TYPE_ERROR);
								strSynchStatusPicture = (status == DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)) ? m_strImgOk : m_strImgError;
								//m_SynchMsg += cwsprintf(_T("{0-%s} : {1-%s}"), CRMInfinityProvider, (status == DataEnum(E_SYNCHROSTATUS_TYPE_SYNCHRO)) ? _TB("This element has been synchronized.") : _TB("The following error occurred during the synchronization process:\r\n"));
								bResult &= BOOL(strToken == _T("True"));
							}
						}

						pSubNode = pFlowNode->GetChildByName(_T("Date"));
						if (pSubNode)
						{
							DataDate tmpDate;
							tmpDate.SetFullDate();
							bOK = pSubNode->GetText(strToken);
							if (bOK)
								tmpDate.AssignFromXMLString(strToken);
							if (m_SynchDate.IsEmpty() || tmpDate > m_SynchDate)
							{
								if (strProviderName == DMSInfinityProvider)
									m_SynchDirection = _TB("Outbound");
								m_SynchDate.AssignFromXMLString(strToken);
							}
						}
						pSubNode = pFlowNode->GetChildByName(_T("Message"));
						if (pSubNode)
						{
							DataStr strTmpMessage;
							BSTR bstrToConvert;
							HRESULT hr = pSubNode->GetIXMLDOMNodePtr()->get_text(&bstrToConvert);
							if SUCCEEDED(hr)
							{
								strTmpMessage.Assign(bstrToConvert);
								strSynchStatusMsg += strFlow + _T(":") + strTmpMessage.ToString() + _T("\r\n\r\n");
							}
						}
						pSubNode = pFlowNode->GetChildByName(_T("Hints"));
						if (pSubNode)
						{
							DataStr strTmpMessage;
							BSTR bstrToConvert;
							HRESULT hr = pSubNode->GetIXMLDOMNodePtr()->get_text(&bstrToConvert);
							if SUCCEEDED(hr)
							{
								strTmpMessage.Assign(bstrToConvert);
								if (status == E_SYNCHROSTATUS_TYPE_ERROR && strTmpMessage.IsEmpty())
								{
									m_SynchStatusHints += _TB("Oops...something gone wrong during the data synchronization. Please check technical details.") + _T("\r\n\r\n");
								}
								else if (status == E_SYNCHROSTATUS_TYPE_ERROR)
									m_SynchStatusHints +=  strTmpMessage.ToString() + _T("\r\n\r\n");
							}
						}
						else if (status == E_SYNCHROSTATUS_TYPE_ERROR)
							m_SynchStatusHints = _TB("Oops...something gone wrong during the data synchronization. Please check technical details.") + _T("\r\n\r\n");
					}
				}
			} 
		}
		if (!bResult)
		{
			strSynchStatusPicture = m_strImgError;
			strSynchStatusMsg = cwsprintf(_T("{0-%s} : {1-%s} {2-%s}"), strProviderName,  _TB("The following error occurred during the synchronization process:\r\n"), strSynchStatusMsg);
		}
		else
		{
			strSynchStatusMsg = cwsprintf(_T("{0-%s} : {1-%s}"), strProviderName, _TB("This element has been synchronized."));
			strSynchStatusPicture = m_strImgOk;
		}
		}
		CATCH_ALL(e)
		{
			strSynchStatusPicture = m_strImgError;
			strSynchStatusMsg += cwsprintf(_T("{0-%s} : {1-%s}"), strProviderName, _TB("Unable to parse logs"));
		}
		END_CATCH_ALL
	}
	else
	{
		if (ProviderNeedMassiveSynchro(strProviderName))
		{
			strSynchStatusMsg = cwsprintf(_T("{0-%s} : {1-%s}"), strProviderName, _TB("Attention please! In order to execute a right synchronization process,\r\n you need run the Massive Data Synchronization procedure first. "));
			strSynchStatusPicture = m_strImgExcluded; //szSynchStatusExcluded;; //szSynchStatusError;
		}
		else
		{
			if (
				(strProviderName == DMSInfinityProvider) && AfxGetOleDbMng()->EasyAttachmentEnable() &&
					(AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(DMSInfinityProvider)->GetDocumentsToSynchImago()->GetDocumentByNs(GetMasterDocument()->GetNamespace().ToString()))
			   )
			{
				strSynchStatusMsg = cwsprintf(_T("{0-%s} : {1-%s}"), DMSInfinityProvider, _TB("No attachment found to synchronize"));
		 		strSynchStatusPicture = m_strImgOk;
			}
			else
			{
				strSynchStatusMsg = cwsprintf(_T("{0-%s} : {1-%s}"), strProviderName, _TB("This document is excluded from synchronization process.") + _T("\r\n"));
				strSynchStatusPicture = m_strImgExcluded; //szSynchStatusExcluded; 
			}
		}
	}
}

//--------------------------------------------------------------------------------------
void CDNotification::SetSynchroData(BOOL bFromTimer /* = FALSE*/)
{
	/*
	TDS_SynchronizationInfo*    pSynchroInfoRec = NULL;
	TDS_SynchronizationInfo*	pSynchroInfoRecCRM = NULL;
	TDS_SynchronizationInfo*    pSynchroInfoRecDMS = NULL;
	*/

	if (GetMasterRec()->f_TBGuid.IsEmpty())
		return;

	BOOL bCollapsed = FALSE;

	if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
	{

		if (!m_pServerDocument->IsABackgroundADM())
		{
			LPCTSTR pDummy;

			CSynchroProvider * CRMProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(CRMInfinityProvider);
			CSynchroProvider * DMSProvider = AfxGetDataSynchroManager()->GetSynchroProviders()->GetProvider(DMSInfinityProvider);

			if (
				!bFromTimer ||
				(
				(CRMProvider && CRMProvider->m_strImagoRequestGuid.LookupKey(GetMasterRec()->f_TBGuid.Str(), pDummy)) ||
					(DMSProvider && DMSProvider->m_strImagoRequestGuid.LookupKey(GetMasterRec()->f_TBGuid.Str(), pDummy))
					)
				)
			{
				//init variables
			//m_SynchDate.Clear();
				m_SynchWorker.Clear();
				m_SynchDirection.Clear();
				m_SynchMsg.Clear();

				CXMLDocumentObject aXMLModDoc;
				CString XmlLog;

				aXMLModDoc.EnableMsgMode(FALSE);

				BOOL    bResultCRM = TRUE;
				BOOL    bResultDMS = TRUE;

				if (
					(
						CRMProvider && CRMProvider->m_strImagoRequestGuid.LookupKey(GetMasterRec()->f_TBGuid.Str(), pDummy)
						&& m_pDataSynchronizer->IsActionQueued(CRMProvider->m_strImagoRequestGuid[GetMasterRec()->f_TBGuid.Str()])
						) ||
						(
							DMSProvider && DMSProvider->m_strImagoRequestGuid.LookupKey(GetMasterRec()->f_TBGuid.Str(), pDummy) &&
							m_pDataSynchronizer->IsActionQueued(DMSProvider->m_strImagoRequestGuid[GetMasterRec()->f_TBGuid.Str()])
							)
					)
				{
					m_SynchStatusPicture = m_strImgWait;	//szSynchStatusWait;
					m_SynchMsg = _TB("The provider is synchronizing this element.");
					m_SynchStatusHints = _TB("The provider is synchronizing this element.");
					/*
					if (m_pDataSynchroPane)
						m_pDataSynchroPane->GetDataSynchroPaneView()->OnUpdateControls();
						*/
					((CTileDialog*)GetServerDoc()->GetTileDialog(IDD_DATA_SYNCHRO_HISTORY_IMS_HINTS))->OnUpdateControls();
					((CTileDialog*)GetServerDoc()->GetTileDialog(IDD_DATA_SYNCHRO_HISTORY_IMS))->OnUpdateControls();
				}
				else
				{

					XmlLog = AfxGetDataSynchroManager()->GetLogsByDocId(CRMInfinityProvider, GetMasterRec()->f_TBGuid.ToString());

					CString strCRMSynchStatusPicture = _T("");
					CString strCRMSynchStatusMsg = _T("");

					ParseXMLLogs(CRMInfinityProvider, XmlLog, strCRMSynchStatusPicture, strCRMSynchStatusMsg);

					m_SynchStatusPicture = strCRMSynchStatusPicture;

					if (!strCRMSynchStatusMsg.IsEmpty())
						strCRMSynchStatusMsg += _T("\r\n");

					if (DMSProvider)
					{
						CString strDMSStatusMsg = _T("");
						CString strDMSStatusPicture = _T("");
						 
						XmlLog = AfxGetDataSynchroManager()->GetLogsByDocId(DMSInfinityProvider, GetMasterRec()->f_TBGuid.ToString());

						ParseXMLLogs(DMSInfinityProvider, XmlLog, strDMSStatusPicture, strDMSStatusMsg);

						m_SynchMsg = (!strCRMSynchStatusMsg.IsEmpty() ? (strCRMSynchStatusMsg + _T("\r\n")) : _T("")) + strDMSStatusMsg;
						if (strCRMSynchStatusPicture == m_strImgOk && strDMSStatusPicture == m_strImgError)
							m_SynchStatusPicture = strDMSStatusPicture;
					}
					else
						m_SynchMsg = strCRMSynchStatusMsg;
				}
			}
		}
	}
	if (!m_pServerDocument->IsABackgroundADM())
	{
		// PER ASSOCIARE UN'IMMAGINE ALLA LINGUETTA DEL DOCKING PANEL DEL DATA SYNCRHONIZER
	/*	if (m_pDataSynchroPane && !m_SynchStatusPicture.IsEmpty())
		{	
			m_pDataSynchroPane->SetIcon(TBLoadImage(m_SynchStatusPicture), FALSE);
			m_pDataSynchroPane->GetAutoHideButton()->GetAutoHideWindow()->UpdateWindow();
			
		}*/
			
	}

	SetValidationData();

	if (!m_pServerDocument->IsInUnattendedMode() && !m_pServerDocument->IsABackgroundADM())
	{
		SetCollapsedValidationTile(m_ValidationStatus.IsEmpty());
		//SetCollapsedHistoryTile(bCollapsed);
	}
}


//--------------------------------------------------------------------------------------
void CDNotification::SetValidationData(BOOL bFromForceValidation /* = FALSE*/)
{
	m_ValidationStatus = GetValidationStatus();


	if (m_ValidationStatus == _T("")) 
	{
		if (bFromForceValidation)
		{
			m_ValidationStatus = _TB("Validation process ended successfully.");
			m_ValidationStatusPicture = m_strImgOk;
		}
		else
		{
			m_ValidationStatusPicture = _T("");
		}
	}
	else
	{
		m_ValidationStatusPicture = m_strImgError;
	}

}

//---------------------------------------------------------------------------------------------
void CDNotification::OnCopyMsg()
{
	CString aStr;

	if (OpenClipboard(NULL))
	{
		EmptyClipboard();
	
		if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			aStr = m_SynchMsg;

		HLOCAL hMem = GlobalAlloc(LHND, ((aStr.GetLength() + 1) * sizeof(TCHAR)));
		char* cptr = (char*)GlobalLock(hMem);
		memcpy(cptr, aStr, ((aStr.GetLength() + 1) * sizeof(TCHAR)));
		GlobalUnlock(hMem);
		if (SetClipboardData(CF_UNICODETEXT, hMem) == NULL)
		{
			AfxMessageBox(_TB("Impossible copy to Clipboard"));
			CloseClipboard();
			return;
		}

		CloseClipboard();
	}
}


//---------------------------------------------------------------------------------------------
void CDNotification::OnCopyMsgHints()
{
	CString aStr;

	if (OpenClipboard(NULL))
	{
		EmptyClipboard();

		if (AfxGetDataSynchroManager()->ImagoStudioRuntimeInstalled())
			aStr = m_SynchStatusHints;

		HLOCAL hMem = GlobalAlloc(LHND, ((aStr.GetLength() + 1) * sizeof(TCHAR)));
		char* cptr = (char*)GlobalLock(hMem);
		memcpy(cptr, aStr, ((aStr.GetLength() + 1) * sizeof(TCHAR)));
		GlobalUnlock(hMem);
		if (SetClipboardData(CF_UNICODETEXT, hMem) == NULL)
		{
			AfxMessageBox(_TB("Impossible copy to Clipboard"));
			CloseClipboard();
			return;
		}

		CloseClipboard();
	}
}