
#include "stdafx.h" 

#include <TbGes\XMLGesInfo.h>

#include "TbDataSynchroClientEnums.h"
#include "DSTables.h"

static TCHAR szParamDocTBGuid	[] = _T("P1");
static TCHAR szParamProvider	[] = _T("P2");
static TCHAR szParamDocNS		[] = _T("P3");
static TCHAR szParamTBGuid		[] = _T("P4");
static TCHAR szParamSynchStatus	[] = _T("P5");
static TCHAR szParamValue		[] = _T("P6");
static TCHAR szParamTableName	[] = _T("P7");
static TCHAR szParamFieldName	[] = _T("P8");

//=============================================================================
//							TDS_Providers
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDS_Providers, SqlRecord)

//-----------------------------------------------------------------------------
TDS_Providers::TDS_Providers(BOOL bCallInit)
	:
	SqlRecord(GetStaticName()),
	f_Disabled(FALSE),
	f_IsEAProvider(FALSE)
{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDS_Providers::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(_NS_FLD("Name"),				f_Name);
		BIND_DATA	(_NS_FLD("Description"),		f_Description);		
		BIND_DATA	(_NS_FLD("Disabled"),			f_Disabled);		
		BIND_DATA	(_NS_FLD("ProviderUrl"),		f_ProviderUrl);
		BIND_DATA	(_NS_FLD("ProviderUser"),		f_ProviderUser);
		BIND_DATA	(_NS_FLD("ProviderPassword"),	f_ProviderPassword);
		BIND_DATA   (_NS_FLD("SkipCrtValidation"),  f_SkipCrtValidation);
		BIND_DATA	(_NS_FLD("ProviderParameters"),	f_ProviderParameters);
		//Impr. 5855: DMSInfinity Connector
		BIND_DATA	(_NS_FLD("IsEAProvider"),		f_IsEAProvider);	
		BIND_DATA	(_NS_FLD("IAFModules"),		f_IAFModules);
		BIND_TB_GUID();
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDS_Providers::GetStaticName() { return _NS_TBL("DS_Providers"); }

//=============================================================================
//							TDS_ActionsLog
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDS_ActionsLog, SqlRecord)

//-----------------------------------------------------------------------------
TDS_ActionsLog::TDS_ActionsLog(BOOL bCallInit)
	:
	SqlRecord(GetStaticName()),
	f_SynchStatus		(E_SYNCHROSTATUS_TYPE_DEFAULT),
	f_SynchDirection	(E_SYNCHRODIRECTION_TYPE_DEFAULT),
	f_ActionType		(E_SYNCHROACTION_TYPE_DEFAULT)

{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDS_ActionsLog::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_AUTOINCREMENT(szLogId,		f_LogId)
		BIND_DATA	(szProviderName,	f_ProviderName);
		BIND_DATA	(szDocNamespace,	f_DocNamespace);
		BIND_DATA	(szDocTBGuid,		f_DocTBGuid);
		BIND_DATA	(szActionType,		f_ActionType);
		BIND_DATA	(szActionData,		f_ActionData);			
		BIND_DATA	(szSynchDirection,	f_SynchDirection);
		BIND_DATA	(szSynchXMLData,	f_SynchXMLData);	
		BIND_DATA	(szSynchStatus,		f_SynchStatus);
		BIND_DATA	(szSynchMessage,	f_SynchMessage);	
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDS_ActionsLog::GetStaticName() { return _NS_TBL("DS_ActionsLog"); }

TCHAR	TDS_ActionsLog::szLogId[]			= _NS_FLD("LogId");
TCHAR	TDS_ActionsLog::szDocTBGuid[]		= _NS_FLD("DocTBGuid");
TCHAR	TDS_ActionsLog::szDocNamespace[]	= _NS_FLD("DocNamespace");	
TCHAR	TDS_ActionsLog::szActionType[]		= _NS_FLD("ActionType");	
TCHAR	TDS_ActionsLog::szActionData[]		= _NS_FLD("ActionData");	
TCHAR	TDS_ActionsLog::szSynchDirection[]	= _NS_FLD("SynchDirection");
TCHAR	TDS_ActionsLog::szSynchXMLData[]	= _NS_FLD("SynchXMLData");
TCHAR	TDS_ActionsLog::szSynchStatus[]		= _NS_FLD("SynchStatus");
TCHAR	TDS_ActionsLog::szSynchMessage[]	= _NS_FLD("SynchMessage");
TCHAR	TDS_ActionsLog::szProviderName[]	= _NS_FLD("ProviderName");


 ///////////////////////////////////////////////////////////////////////////////
//             class TEnhDS_ActionsLog implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(TEnhDS_ActionsLog, TDS_ActionsLog)

//-----------------------------------------------------------------------------
TEnhDS_ActionsLog::TEnhDS_ActionsLog(BOOL bCallInit)
{
	BindRecord();	
	if (bCallInit) 
		Init(); 
}

//-----------------------------------------------------------------------------
void TEnhDS_ActionsLog::BindRecord()
{
	BEGIN_BIND_DATA	();
		LOCAL_STR	(_NS_LFLD("TEnhDS_WorkerDescri"),		l_WorkerDescri,			64);
		LOCAL_STR  (_NS_LFLD("TEnhDS_SynchStatusBmp"),		l_SynchStatusBmp,		32);
		LOCAL_STR  (_NS_LFLD("TEnhDS_SynchDirectionBmp"),	l_SynchDirectionBmp,	32);
	END_BIND_DATA();
}

//=============================================================================
//							TDS_ActionsQueue
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDS_ActionsQueue, SqlRecord)

//-----------------------------------------------------------------------------
TDS_ActionsQueue::TDS_ActionsQueue(BOOL bCallInit)
	:
	SqlRecord(GetStaticName()),
	f_SynchStatus		(E_SYNCHROSTATUS_TYPE_DEFAULT),
	f_SynchDirection	(E_SYNCHRODIRECTION_TYPE_DEFAULT)

{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDS_ActionsQueue::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_AUTOINCREMENT(szLogId,		f_LogId)
		BIND_DATA	(szProviderName,	f_ProviderName);
		BIND_DATA	(szActionName,		f_ActionName);			
		BIND_DATA	(szSynchDirection,	f_SynchDirection);
		BIND_DATA	(szSynchXMLData,	f_SynchXMLData);	
		BIND_DATA	(szSynchStatus,		f_SynchStatus);
		BIND_DATA	(szSynchFilters,	f_SynchFilters);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDS_ActionsQueue::GetStaticName() { return _NS_TBL("DS_ActionsQueue"); }

TCHAR	TDS_ActionsQueue::szLogId[]				= _NS_FLD("LogId");
TCHAR	TDS_ActionsQueue::szProviderName[]		= _NS_FLD("ProviderName");
TCHAR	TDS_ActionsQueue::szActionName[]		= _NS_FLD("ActionName");	
TCHAR	TDS_ActionsQueue::szSynchDirection[]	= _NS_FLD("SynchDirection");
TCHAR	TDS_ActionsQueue::szSynchXMLData[]		= _NS_FLD("SynchXMLData");
TCHAR	TDS_ActionsQueue::szSynchStatus[]		= _NS_FLD("SynchStatus");
TCHAR	TDS_ActionsQueue::szSynchFilters[]		= _NS_FLD("SynchFilters");





//=============================================================================
//							TDS_SynchronizationInfo
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDS_SynchronizationInfo, SqlRecord)

//-----------------------------------------------------------------------------
TDS_SynchronizationInfo::TDS_SynchronizationInfo(BOOL bCallInit)
	:
	SqlRecord(GetStaticName()),	
	f_SynchStatus		(E_SYNCHROSTATUS_TYPE_DEFAULT),
	f_SynchDirection	(E_SYNCHRODIRECTION_TYPE_DEFAULT),
	f_LastAction		(E_SYNCHROACTION_TYPE_DEFAULT)


{
	f_SynchDate.SetFullDate();
	f_StartSynchDate.SetFullDate();
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDS_SynchronizationInfo::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(szDocTBGuid,		f_DocTBGuid);
		BIND_DATA	(szProviderName,	f_ProviderName);
		BIND_DATA	(szDocNamespace,	f_DocNamespace);
		BIND_DATA	(szSynchStatus,		f_SynchStatus);
		BIND_DATA	(szSynchDate,		f_SynchDate);
		BIND_DATA	(szSynchDirection,	f_SynchDirection);
		BIND_DATA	(szWorkerID,		f_WorkerID);		
		BIND_DATA	(szLastAction,		f_LastAction);
		BIND_DATA	(szStartSynchDate,	f_StartSynchDate);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDS_SynchronizationInfo::GetStaticName() { return _NS_TBL("DS_SynchronizationInfo"); }


 TCHAR	TDS_SynchronizationInfo::szProviderName[]	= _NS_FLD("ProviderName");
 TCHAR	TDS_SynchronizationInfo::szDocTBGuid[]		= _NS_FLD("DocTBGuid"); 	
 TCHAR	TDS_SynchronizationInfo::szDocNamespace[]	= _NS_FLD("DocNamespace");
 TCHAR	TDS_SynchronizationInfo::szSynchStatus[]	= _NS_FLD("SynchStatus");	
 TCHAR	TDS_SynchronizationInfo::szSynchDate[]		= _NS_FLD("SynchDate");
 TCHAR	TDS_SynchronizationInfo::szSynchDirection[] = _NS_FLD("SynchDirection");
 TCHAR	TDS_SynchronizationInfo::szWorkerID[]		= _NS_FLD("WorkerID");
 TCHAR	TDS_SynchronizationInfo::szLastAction[]		= _NS_FLD("LastAction");	
 TCHAR	TDS_SynchronizationInfo::szStartSynchDate[] = _NS_FLD("StartSynchDate");



 ///////////////////////////////////////////////////////////////////////////////
//             class TEnhDS_SynchronizationInfo implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(TEnhDS_SynchronizationInfo, TDS_SynchronizationInfo)

//-----------------------------------------------------------------------------
TEnhDS_SynchronizationInfo::TEnhDS_SynchronizationInfo(BOOL bCallInit)
{
	BindRecord();	
	if (bCallInit) 
		Init(); 
}

//-----------------------------------------------------------------------------
void TEnhDS_SynchronizationInfo::BindRecord()
{
	BEGIN_BIND_DATA	();
		LOCAL_STR	(_NS_LFLD("TEnhDS_Sync_Code"),			l_Code,					21);
		LOCAL_STR	(_NS_LFLD("TEnhDS_Sync_Description"),	l_Description,			128);
		LOCAL_STR	(_NS_LFLD("TEnhDS_WorkerDescri"),		l_WorkerDescri,			64);
		LOCAL_STR   (_NS_LFLD("TEnhDS_SynchStatusBmp"),		l_SynchStatusBmp,		512);
		LOCAL_STR   (_NS_LFLD("TEnhDS_SynchDirectionBmp"),	l_SynchDirectionBmp,	512);
		LOCAL_STR	(_NS_LFLD("TEnhDS_SynchMessage"),		l_SynchMessage,			1024);
	END_BIND_DATA();
}

 
//=============================================================================
//							TDS_AttachmentSynchroInfo
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDS_AttachmentSynchroInfo, SqlRecord)

//-----------------------------------------------------------------------------
TDS_AttachmentSynchroInfo::TDS_AttachmentSynchroInfo(BOOL bCallInit)
	:
	SqlRecord(GetStaticName()),	
	f_SynchStatus		(E_SYNCHROSTATUS_TYPE_DEFAULT),
	f_SynchDirection	(E_SYNCHRODIRECTION_TYPE_DEFAULT),
	f_LastAction		(E_SYNCHROACTION_TYPE_DEFAULT)

{
	f_SynchDate.SetFullDate();
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDS_AttachmentSynchroInfo::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(szDocTBGuid,		f_DocTBGuid);
		BIND_DATA	(szProviderName,	f_ProviderName);
		BIND_DATA	(szAttachmentID,	f_AttachmentID);		
		BIND_DATA	(szDocNamespace,	f_DocNamespace);
		BIND_DATA	(szSynchStatus,		f_SynchStatus);
		BIND_DATA	(szSynchDate,		f_SynchDate);
		BIND_DATA	(szSynchDirection,	f_SynchDirection);
		BIND_DATA	(szWorkerID,		f_WorkerID);		
		BIND_DATA	(szLastAction,		f_LastAction);	
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDS_AttachmentSynchroInfo::GetStaticName() { return _NS_TBL("DS_AttachmentSynchroInfo"); }


 TCHAR	TDS_AttachmentSynchroInfo::szProviderName[]	= _NS_FLD("ProviderName");
 TCHAR	TDS_AttachmentSynchroInfo::szDocTBGuid[]		= _NS_FLD("DocTBGuid"); 	
 TCHAR	TDS_AttachmentSynchroInfo::szAttachmentID[]	= _NS_FLD("AttachmentID"); 	
 TCHAR	TDS_AttachmentSynchroInfo::szDocNamespace[]	= _NS_FLD("DocNamespace");
 TCHAR	TDS_AttachmentSynchroInfo::szSynchStatus[]	= _NS_FLD("SynchStatus");	
 TCHAR	TDS_AttachmentSynchroInfo::szSynchDate[]		= _NS_FLD("SynchDate");
 TCHAR	TDS_AttachmentSynchroInfo::szSynchDirection[] = _NS_FLD("SynchDirection");
 TCHAR	TDS_AttachmentSynchroInfo::szWorkerID[]		= _NS_FLD("WorkerID");
 TCHAR	TDS_AttachmentSynchroInfo::szLastAction[]		= _NS_FLD("LastAction");	

///////////////////////////////////////////////////////////////////////////////
//             class TEnhDS_AttachmentSynchroInfo implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(TEnhDS_AttachmentSynchroInfo, TDS_AttachmentSynchroInfo)

//-----------------------------------------------------------------------------
TEnhDS_AttachmentSynchroInfo::TEnhDS_AttachmentSynchroInfo(BOOL bCallInit)
{
	BindRecord();	
	if (bCallInit) 
		Init(); 
}

//-----------------------------------------------------------------------------
void TEnhDS_AttachmentSynchroInfo::BindRecord()
{
	BEGIN_BIND_DATA	();
		LOCAL_STR	(_NS_LFLD("TEnhDS_FileName"),		l_FileName,			256);
		LOCAL_STR	(_NS_LFLD("TEnhDS_AttDescri"),		l_AttDescription,	256);
		LOCAL_STR	(_NS_LFLD("TEnhDS_WorkerDescri"),	l_WorkerDescri,		 64);		
		LOCAL_STR   (_NS_LFLD("TEnhDS_SynchStatusBmp"),	l_SynchStatusBmp,	 32);
	END_BIND_DATA();
}
//=============================================================================
//							TDS_SynchroFilter
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDS_SynchroFilter, SqlRecord)

//-----------------------------------------------------------------------------
TDS_SynchroFilter::TDS_SynchroFilter(BOOL bCallInit)
	:
	SqlRecord(GetStaticName())
{
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDS_SynchroFilter::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(_NS_FLD("DocNamespace"),		f_DocNamespace);	
		BIND_DATA	(_NS_FLD("ProviderName"),		f_ProviderName);
		BIND_DATA	(_NS_FLD("SynchroFilter"),		f_SynchroFilter);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDS_SynchroFilter::GetStaticName() { return _NS_TBL("DS_SynchroFilter"); }

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRDS_SynchronizationInfo
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRDS_SynchronizationInfo, TableReader)

//------------------------------------------------------------------------------
TRDS_SynchronizationInfo::TRDS_SynchronizationInfo(CBaseDocument* pDocument /*= NULL*/)
	: 
	TableReader(RUNTIME_CLASS(TDS_SynchronizationInfo), pDocument)
{}


//------------------------------------------------------------------------------
void TRDS_SynchronizationInfo::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn(GetRecord()->f_DocTBGuid);
	m_pTable->AddParam(szParamDocTBGuid,	GetRecord()->f_DocTBGuid);
	m_pTable->AddFilterColumn(GetRecord()->f_ProviderName);	
	m_pTable->AddParam(szParamProvider,	GetRecord()->f_ProviderName);
}

//------------------------------------------------------------------------------
void TRDS_SynchronizationInfo::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamDocTBGuid, m_DocTBGuid);
	m_pTable->SetParamValue(szParamProvider, m_ProviderName);
}

//------------------------------------------------------------------------------
BOOL TRDS_SynchronizationInfo::IsEmptyQuery()
{
	return (m_DocTBGuid.IsEmpty() || m_ProviderName.IsEmpty());
}

//------------------------------------------------------------------------------
TableReader::FindResult TRDS_SynchronizationInfo::FindRecord(const DataGuid& docTBGuid, const DataStr&  providerName)	
{
	m_DocTBGuid = docTBGuid;
	m_ProviderName = providerName;
	return TableReader::FindRecord();
}

/////////////////////////////////////////////////////////////////////////////
//				class RRDS_SynchronizationInfoByStatus Implementation
/////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNAMIC (RRDS_SynchronizationInfoByStatus, RowsetReader)

//------------------------------------------------------------------------------
RRDS_SynchronizationInfoByStatus::RRDS_SynchronizationInfoByStatus(DataBool bDelta, DataDate startSynchDate, CBaseDocument* pDocument/* NULL */)
	: 
	RowsetReader(RUNTIME_CLASS(TDS_SynchronizationInfo), pDocument)
{
	m_StartSynchDate.SetFullDate(TRUE);
	m_bDelta = bDelta;
	m_StartSynchDate = startSynchDate;
}

//------------------------------------------------------------------------------
void RRDS_SynchronizationInfoByStatus::OnDefineQuery ()
{   
	m_pTable->SelectAll	();
	m_pTable->AddFilterColumn	(GetRecord()->f_ProviderName);
	m_pTable->AddParam			(szParamProvider, GetRecord()->f_ProviderName);
	m_pTable->AddFilterColumn	(GetRecord()->f_DocNamespace);
	m_pTable->AddParam			(szParamDocNS, GetRecord()->f_DocNamespace);
	if (m_bDelta)
	{
		m_pTable->AddFilterColumn(GetRecord()->f_StartSynchDate);
		m_pTable->AddParam(TDS_SynchronizationInfo::szStartSynchDate, GetRecord()->f_StartSynchDate);
	}
	m_pTable->AddFilterColumn	(GetRecord()->f_SynchStatus);
	m_pTable->AddParam			(szParamSynchStatus, GetRecord()->f_SynchStatus);
	
}

//------------------------------------------------------------------------------
void RRDS_SynchronizationInfoByStatus::OnPrepareQuery ()
{
	m_pTable->SetParamValue(szParamProvider,	m_ProviderName);
	m_pTable->SetParamValue(szParamDocNS,		m_DocNamespace);
	if (m_bDelta)
		m_pTable->SetParamValue(TDS_SynchronizationInfo::szStartSynchDate, m_StartSynchDate);
	m_pTable->SetParamValue(szParamSynchStatus,	m_SynchStatus);
}

//------------------------------------------------------------------------------
BOOL RRDS_SynchronizationInfoByStatus::IsEmptyQuery()
{
	return (m_ProviderName.IsEmpty() || m_DocNamespace.IsEmpty());
}

//------------------------------------------------------------------------------
RowsetReader::FindResult RRDS_SynchronizationInfoByStatus::FindRecord 
			(
				const DataStr&  aProviderName,
				const DataStr&  aDocNamespace,
				const DataEnum& aSynchStatus
			)
{
	m_ProviderName	= aProviderName; 
	m_DocNamespace  = aDocNamespace;
	m_SynchStatus	= aSynchStatus;
		
	return RowsetReader::FindRecord();
}


/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRDS_ActionsLog
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRDS_ActionsLog, TableReader)

//------------------------------------------------------------------------------
TRDS_ActionsLog::TRDS_ActionsLog(CBaseDocument* pDocument /*= NULL*/)
	: 
	TableReader(RUNTIME_CLASS(TDS_ActionsLog), pDocument)
{
}

//------------------------------------------------------------------------------
void TRDS_ActionsLog::OnDefineQuery()
{
	m_pTable->SelectAll();
	m_pTable->AddFilterColumn(GetRecord()->f_DocTBGuid);
	m_pTable->AddParam(szParamDocTBGuid, GetRecord()->f_DocTBGuid);
	m_pTable->AddFilterColumn(GetRecord()->f_ProviderName);	
	m_pTable->AddParam(szParamProvider,	GetRecord()->f_ProviderName);
	m_pTable->AddSortColumn(GetRecord()->f_LogId, TRUE);
}

//------------------------------------------------------------------------------
void TRDS_ActionsLog::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamDocTBGuid, m_DocTBGuid);
	m_pTable->SetParamValue(szParamProvider, m_ProviderName);
}

//------------------------------------------------------------------------------
BOOL TRDS_ActionsLog::IsEmptyQuery()
{
	return (m_DocTBGuid.IsEmpty() || m_ProviderName.IsEmpty());
}

//------------------------------------------------------------------------------
TableReader::FindResult TRDS_ActionsLog::FindRecord(const DataGuid& docTBGuid, const DataStr&  providerName)	
{
	m_DocTBGuid = docTBGuid;
	m_ProviderName = providerName;
	return TableReader::FindRecord();
}


/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRDS_SynchronizationInfo
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TUDS_SynchronizationInfo, TableUpdater)

//------------------------------------------------------------------------------
TUDS_SynchronizationInfo::TUDS_SynchronizationInfo(CAbstractFormDoc* pDocument /*= NULL*/ , CMessages* pMessages/*= NULL*/)
	: 
	TableUpdater(RUNTIME_CLASS(TDS_SynchronizationInfo), (CBaseDocument*)pDocument, pMessages)
{}

//------------------------------------------------------------------------------
void TUDS_SynchronizationInfo::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn(GetRecord()->f_DocTBGuid);
	m_pTable->AddParam(szParamDocTBGuid,	GetRecord()->f_DocTBGuid);
	m_pTable->AddFilterColumn(GetRecord()->f_ProviderName);	
	m_pTable->AddParam(szParamProvider,	GetRecord()->f_ProviderName);
}

//------------------------------------------------------------------------------
void TUDS_SynchronizationInfo::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamDocTBGuid, m_DocTBGuid);
	m_pTable->SetParamValue(szParamProvider, m_ProviderName);
}

//------------------------------------------------------------------------------
BOOL TUDS_SynchronizationInfo::IsEmptyQuery()
{
	return (m_DocTBGuid.IsEmpty() || m_ProviderName.IsEmpty());
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUDS_SynchronizationInfo::FindRecord(const DataGuid& docTBGuid, const DataStr&  providerName, BOOL bLock)	
{
	m_DocTBGuid = docTBGuid;
	m_ProviderName = providerName;
	return TableUpdater::FindRecord(bLock);
}

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRDS_Providers
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRDS_Providers, TableReader)

//------------------------------------------------------------------------------
TRDS_Providers::TRDS_Providers(CBaseDocument* pDocument /*= NULL*/)
	:
	TableReader(RUNTIME_CLASS(TDS_Providers), pDocument)
{}


//------------------------------------------------------------------------------
void TRDS_Providers::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn(GetRecord()->f_Name);
	m_pTable->AddParam(szParamProvider, GetRecord()->f_Name);
}

//------------------------------------------------------------------------------
void TRDS_Providers::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamProvider, m_ProviderName);
}

//------------------------------------------------------------------------------
BOOL TRDS_Providers::IsEmptyQuery()
{
	return m_ProviderName.IsEmpty();
}

//------------------------------------------------------------------------------
TableReader::FindResult TRDS_Providers::FindRecord(const DataStr&  providerName)
{
	m_ProviderName = providerName;
	return TableReader::FindRecord();
}

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TUDS_Providers
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TUDS_Providers, TableUpdater)

//------------------------------------------------------------------------------
TUDS_Providers::TUDS_Providers(CAbstractFormDoc* pDocument /*= NULL*/ , CMessages* pMessages/*= NULL*/)
	: 
	TableUpdater(RUNTIME_CLASS(TDS_Providers), (CBaseDocument*)pDocument, pMessages)
{}

//------------------------------------------------------------------------------
void TUDS_Providers::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn(GetRecord()->f_Name);
	m_pTable->AddParam(szParamProvider,	GetRecord()->f_Name);
}

//------------------------------------------------------------------------------
void TUDS_Providers::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamProvider, m_ProviderName);
}

//------------------------------------------------------------------------------
BOOL TUDS_Providers::IsEmptyQuery()
{
	return (m_ProviderName.IsEmpty());
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUDS_Providers::FindRecord(const DataStr&  providerName, BOOL bLock)	
{
	m_ProviderName = providerName;
	return TableUpdater::FindRecord(bLock);
}


/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TUDR_SynchroFilter
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRDS_SynchroFilter, TableReader)

//------------------------------------------------------------------------------
TRDS_SynchroFilter::TRDS_SynchroFilter(CBaseDocument* pDocument /*= NULL*/)
	: 
	TableReader(RUNTIME_CLASS(TDS_SynchroFilter), pDocument)
{}

//------------------------------------------------------------------------------
void TRDS_SynchroFilter::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn(GetRecord()->f_ProviderName);
	m_pTable->AddParam(szParamProvider,	GetRecord()->f_ProviderName);

	m_pTable->AddFilterColumn(GetRecord()->f_DocNamespace);
	m_pTable->AddParam(szParamDocNS,	GetRecord()->f_DocNamespace);
}

//------------------------------------------------------------------------------
void TRDS_SynchroFilter::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamProvider, m_ProviderName);
	m_pTable->SetParamValue(szParamDocNS, m_DocNamespace);
}

//------------------------------------------------------------------------------
BOOL TRDS_SynchroFilter::IsEmptyQuery()
{
	return (m_ProviderName.IsEmpty() || m_DocNamespace.IsEmpty());
}

//------------------------------------------------------------------------------
TableReader::FindResult TRDS_SynchroFilter::FindRecord(const DataStr& docNamespace, const DataStr& providerName)	
{
	m_ProviderName = providerName;
	m_DocNamespace = docNamespace;
	return TableReader::FindRecord();
}


/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TUDS_SynchroFilter
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TUDS_SynchroFilter, TableUpdater)

//------------------------------------------------------------------------------
TUDS_SynchroFilter::TUDS_SynchroFilter(CAbstractFormDoc* pDocument /*= NULL*/ , CMessages* pMessages/*= NULL*/)
	: 
	TableUpdater(RUNTIME_CLASS(TDS_SynchroFilter), (CBaseDocument*)pDocument, pMessages)
{}

//------------------------------------------------------------------------------
void TUDS_SynchroFilter::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn(GetRecord()->f_ProviderName);
	m_pTable->AddParam(szParamProvider,	GetRecord()->f_ProviderName);

	m_pTable->AddFilterColumn(GetRecord()->f_DocNamespace);
	m_pTable->AddParam(szParamDocNS,	GetRecord()->f_DocNamespace);
}

//------------------------------------------------------------------------------
void TUDS_SynchroFilter::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamProvider, m_ProviderName);
	m_pTable->SetParamValue(szParamDocNS, m_DocNamespace);
}

//------------------------------------------------------------------------------
BOOL TUDS_SynchroFilter::IsEmptyQuery()
{
	return (m_ProviderName.IsEmpty() || m_DocNamespace.IsEmpty());
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUDS_SynchroFilter::FindRecord(const DataStr& docNamespace, const DataStr& providerName, BOOL bLock)	
{
	m_ProviderName = providerName;
	m_DocNamespace = docNamespace;
	return TableUpdater::FindRecord(bLock);
}




///////////////////////////////////////////////////////////////////////////////
//             class SynchroTableReader implementation
///////////////////////////////////////////////////////////////////////////////
////-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SynchroTableReader, TableReader)

//------------------------------------------------------------------------------
SynchroTableReader::SynchroTableReader(CRuntimeClass* pSqlRecordClass, SqlSession* pSqlSession)
	: 
	TableReader(pSqlRecordClass, pSqlSession),
	m_pDescriptionFields(NULL),
	m_pKeyFieldsToUse(NULL)
{
}

//------------------------------------------------------------------------------
void SynchroTableReader::SetFieldsToSelect(CStringArray* pKFields, CStringArray* pDescriptionFields)
{
	m_pDescriptionFields = pDescriptionFields;
	m_pKeyFieldsToUse = pKFields;
}

//------------------------------------------------------------------------------
BOOL SynchroTableReader::IsEmptyQuery()
{
	return (m_tbGuid.IsEmpty());
}

//------------------------------------------------------------------------------
void SynchroTableReader::OnDefineQuery()
{
	if (!m_pDescriptionFields && !m_pKeyFieldsToUse)
		m_pTable->SelectAll();
	else
	{
		if (m_pKeyFieldsToUse)
			for (int i = 0; i < m_pKeyFieldsToUse->GetSize(); i++)
				m_pTable->Select(m_pRecord->GetDataObjFromColumnName(m_pKeyFieldsToUse->GetAt(i)));

		if (m_pDescriptionFields)
		{
			CString descri;
			for (int i = 0; i < m_pDescriptionFields->GetSize(); i++)
			{
				descri = m_pDescriptionFields->GetAt(i);
				int nPos = m_pRecord->GetIndexFromColumnName(descri);
				if (nPos > -1)
					m_pTable->Select(m_pRecord->GetDataObjAt(nPos));
			}
		}
	}

	m_pTable->AddFilterColumn(GetRecord()->f_TBGuid);
	m_pTable->AddParam(szParamTBGuid,	GetRecord()->f_TBGuid);
}

//------------------------------------------------------------------------------
void SynchroTableReader::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamTBGuid, m_tbGuid);
}

//------------------------------------------------------------------------------
TableReader::FindResult SynchroTableReader::FindRecord(const DataGuid& tbGuid)	
{
	m_tbGuid = tbGuid;
	return TableReader::FindRecord();
}

///////////////////////////////////////////////////////////////////////////////
//             class CSynchroDocInfo implementation
///////////////////////////////////////////////////////////////////////////////
//
//=========================================================================================
CSynchroDocInfo::CSynchroDocInfo(const CString& docNamespace)
:
	m_pSynchroTR(NULL),
	m_pSqlSession(NULL),
	m_strDocNamespace(docNamespace),
	m_bIsValid(TRUE),
	m_wActionMode(NOTIFY_DELETE | NOTIFY_UPDATE | NOTIFY_INSERT)
{	
}

//=========================================================================================
CSynchroDocInfo::~CSynchroDocInfo()
{
	SAFE_DELETE(m_pSynchroTR);

}

//=========================================================================================
void CSynchroDocInfo::SetActionMode(const CString& strActionMode)
{
	if (strActionMode.IsEmpty() || strActionMode == _T("111"))
	{
		m_wActionMode = (NOTIFY_DELETE | NOTIFY_UPDATE | NOTIFY_INSERT);
		return;
	}

	bool notifyDelete = strActionMode[2] == _T('1');
	bool notifyUpdate = strActionMode[1] == _T('1');
	bool notifyInsert = strActionMode[0] == _T('1');

	m_wActionMode = 0x000;

	m_wActionMode = (notifyDelete) ? m_wActionMode | NOTIFY_DELETE : m_wActionMode;
	m_wActionMode = (notifyUpdate) ? m_wActionMode | NOTIFY_UPDATE : m_wActionMode;
	m_wActionMode = (notifyInsert) ? m_wActionMode | NOTIFY_INSERT : m_wActionMode;
}
	
//=========================================================================================
void CSynchroDocInfo::SetDecodingInfo(SqlSession* pSession)
{
	m_pSqlSession = pSession;
	
	//carico i metatdati del documento per avere le seguenti informazioni:
	// titolo localizzato del documento
	// elenco dei campi di tipo description da utilizzare per la decodifica del guid nel monitor
	CTBNamespace aDocNamespace(m_strDocNamespace);
	CXMLDocObjectInfo aDocDescri(aDocNamespace);
	aDocDescri.LoadAllFiles();

	Array docInfoArray;		
	CStringArray arFixedKey;
		
	if (!aDocDescri.GetDBTInfoArray() || !m_pSqlSession)
	{
		m_bIsValid = FALSE;
		return;
	}

	// prima leggo dal file DBTS.xml le informazioni necessarie :
	// namespace tabella master
	// fixedkey 
	// campi descrizione
	for (int i = 0; i < aDocDescri.GetDBTInfoArray()->GetSize(); i++)
	{
		CXMLDBTInfo* pDBTInfo = aDocDescri.GetDBTInfoArray()->GetAt(i);
		if (pDBTInfo->m_eType == CXMLDBTInfo::MASTER_TYPE)
		{
			m_docTitle = aDocDescri.GetDocumentTitle();
			m_tableNamespace = pDBTInfo->GetTableNameSpace();
			
			//FixedKey: vedi tipo cliente
			if (pDBTInfo->GetXMLFixedKeyArray())
			{
				for (int j = 0; j < pDBTInfo->GetXMLFixedKeyArray()->GetSize(); j++)
				{
					CXMLFixedKey* pFixedKey = pDBTInfo->GetXMLFixedKeyArray()->GetAt(j);
					arFixedKey.Add(pFixedKey->GetName());
				}				
			}
				
			//Description Field
			CStringArray arDescriptionFields;
		
			if (pDBTInfo->GetXMLSearchBookmarkArray())
			{
				for (int k = 0; k < pDBTInfo->GetXMLSearchBookmarkArray()->GetSize(); k++)
				{
					CXMLSearchBookmark* pFixedKey = pDBTInfo->GetXMLSearchBookmarkArray()->GetAt(k);
					if (pFixedKey && pFixedKey->ShowAsDescription())
						m_DescriptionFields.Add(pFixedKey->GetName());
				}
			}
			else
				//inserisco di default il campo Description
				m_DescriptionFields.Add(_T("Description"));			
			break;		
		}
	}
	
	if (!m_tableNamespace.IsValid())
	{
		m_bIsValid = FALSE;
		return;
	}

	//con il namespace della tabella vado a leggere le informazioni di catalog necessari a crearmi:
	// il TableReader per le decodifiche TBGUID->Chiave, descrizione record
	// il TableUpdater per l'aggiurnamento dei record privi di TBGUID
	const SqlCatalogEntry* pCatalogEntry = m_pSqlSession->GetSqlConnection()->GetCatalogEntry(m_tableNamespace.GetObjectName());
	if (pCatalogEntry)
	{
		m_pSqlRecordClass = pCatalogEntry->GetSqlRecordClass();
		if (!pCatalogEntry->m_pTableInfo)
		{
			SqlRecord* pRecDummy = pCatalogEntry->CreateRecord();
			delete pRecDummy;
		}
		const SqlUniqueColumns* pUniqueColumns = pCatalogEntry->m_pTableInfo->GetSqlUniqueColumns();

		if (pUniqueColumns)
			for (int i = 0; i < pUniqueColumns->GetSize(); i++)
			{
				CString strField = pUniqueColumns->GetAt(i);
				BOOL bNotFound = TRUE;
				//se è tra i fixedkey non lo considero
				for (int j = 0; j < arFixedKey.GetSize(); j++)
				{
					CString strFixedField = arFixedKey.GetAt(j);
					if (strFixedField.CompareNoCase(strField) == 0)
					{
						bNotFound = FALSE;
						break;
					}
				}
				if (bNotFound)
					m_KeyFieldsToUse.Add(strField);
			}
	}
	
	if (m_pSqlRecordClass && m_KeyFieldsToUse.GetSize() > 0)
	{
		m_pSynchroTR =  new SynchroTableReader(m_pSqlRecordClass, m_pSqlSession);
		m_pSynchroTR->SetFieldsToSelect(&m_KeyFieldsToUse, &m_DescriptionFields);
	}
	else
		m_bIsValid = FALSE;
}

//=========================================================================================
void CSynchroDocInfo::GetDecodingInfo(const DataGuid& tbGuid, DataStr& recordKey, DataStr& recordDescription)
{
	if (!m_pSynchroTR)
		return;
	if (m_pSynchroTR->FindRecord(tbGuid) == TableReader::FOUND)
	{
		SqlRecord* pFoundRec = m_pSynchroTR->GetRecord();
		//considero prima i segmenti di chiave primaria
		for (int i = 0; i < m_KeyFieldsToUse.GetSize(); i++)
		{
			DataObj* pValue = pFoundRec->GetDataObjFromColumnName(m_KeyFieldsToUse.GetAt(i));
			if (!recordKey.IsEmpty())
			{
				recordKey += _T(';');
				recordKey += _T(' ');

			}
			recordKey = recordKey + pValue->FormatData();
		}

		//e poi i campi di tipo descrizione
		for (int i = 0; i < m_DescriptionFields.GetSize(); i++)
		{
			int nPos = pFoundRec->GetIndexFromColumnName(m_DescriptionFields.GetAt(i));
			if (nPos > -1)	
			{
					DataObj* pValue = pFoundRec->GetDataObjAt(nPos);
					if (!recordKey.IsEmpty())
						recordDescription += _T(';');

				recordDescription = pValue->FormatData();
			}
		}
	}
}


/////////////////////////////////////////////////////////////////////////////
//			class CSynchroDocInfoArray declaration					   //
/////////////////////////////////////////////////////////////////////////////
//
CSynchroDocInfo *CSynchroDocInfoArray::GetDocumentByNs(const CString& strDocNamespace) const
{
	CSynchroDocInfo* pSynchroDocInfo = NULL;
	for(int i = 0; i < GetSize(); i++)
	{
		pSynchroDocInfo = GetAt(i);
		if (pSynchroDocInfo && pSynchroDocInfo->m_strDocNamespace.CompareNoCase(strDocNamespace) == 0 )
			return pSynchroDocInfo;
	}
	return NULL;
}


//////////////////////////////////////////////////////////////////////////////
//            			class VProviderParams implementation
//////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(VProviderParams , SqlVirtualRecord)

//-----------------------------------------------------------------------------
VProviderParams::VProviderParams(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord	(_T("VProviderParams"))
{
	BindRecord();
	if (bCallInit) Init(); 
}

//-----------------------------------------------------------------------------
void VProviderParams::BindRecord()
{
	BEGIN_BIND_DATA	();
		LOCAL_STR	(_NS_LFLD("VName"),			l_Name,			50);
		LOCAL_STR	(_NS_LFLD("VValue"),		l_Value,	    256);
		LOCAL_STR	(_NS_LFLD("VDescription"),	l_Description,  128);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VProviderParams::GetStaticName() { return _NS_TBL("VProviderParams"); }


//-----------------------------------------------------------------------------
BOOL VProviderParams::IsEmpty()
{
	return (l_Name.IsEmpty() || l_Value.IsEmpty());
}

/********************************************* TABELLE PER PROCEDURA DI VALIDAZIONE ***********************************************************/

//=============================================================================
//							TDS_ValidationInfo
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDS_ValidationInfo, SqlRecord)

//-----------------------------------------------------------------------------
TDS_ValidationInfo::TDS_ValidationInfo(BOOL bCallInit)
	:
	SqlRecord		(GetStaticName()),
	f_FKError		(FALSE),
	f_XSDError		(FALSE),
	f_UsedForFilter	(FALSE)
{
	f_ValidationDate.SetFullDate();
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDS_ValidationInfo::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_DATA	(szProviderName,		f_ProviderName);
		BIND_DATA	(szDocTBGuid,			f_DocTBGuid);
		BIND_DATA	(szActionName,			f_ActionName);
		BIND_DATA	(szDocNamespace,		f_DocNamespace);
		BIND_DATA	(szFKError,				f_FKError);
		BIND_DATA	(szXSDError,			f_XSDError);
		BIND_DATA	(szUsedForFilter,		f_UsedForFilter);
		BIND_DATA	(szMessageError,		f_MessageError);
		BIND_DATA	(szValidationDate,		f_ValidationDate);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDS_ValidationInfo::GetStaticName() { return _NS_TBL("DS_ValidationInfo"); }

//-----------------------------------------------------------------------------
 TCHAR	TDS_ValidationInfo::szProviderName		[]	= _NS_FLD("ProviderName");
 TCHAR	TDS_ValidationInfo::szDocTBGuid			[]	= _NS_FLD("DocTBGuid");
 TCHAR	TDS_ValidationInfo::szActionName		[]	= _NS_FLD("ActionName");
 TCHAR	TDS_ValidationInfo::szDocNamespace		[]	= _NS_FLD("DocNamespace");
 TCHAR	TDS_ValidationInfo::szFKError			[]	= _NS_FLD("FKError");
 TCHAR	TDS_ValidationInfo::szXSDError			[]	= _NS_FLD("XSDError");
 TCHAR	TDS_ValidationInfo::szUsedForFilter		[]	= _NS_FLD("UsedForFilter");
 TCHAR	TDS_ValidationInfo::szMessageError		[]	= _NS_FLD("MessageError");
 TCHAR	TDS_ValidationInfo::szValidationDate	[]	= _NS_FLD("ValidationDate");

//=============================================================================
//							TDS_ValidationFKtoFix
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TDS_ValidationFKtoFix, SqlRecord)

//-----------------------------------------------------------------------------
TDS_ValidationFKtoFix::TDS_ValidationFKtoFix(BOOL bCallInit)
	:
	SqlRecord	(GetStaticName())
{
	f_ValidationDate.SetFullDate();
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void TDS_ValidationFKtoFix::BindRecord()
{
	BEGIN_BIND_DATA();
		BIND_AUTOINCREMENT	(szID,					f_ID)
		BIND_DATA			(szProviderName,		f_ProviderName);
		BIND_DATA			(szDocNamespace,		f_DocNamespace);
		BIND_DATA			(szTableName,			f_TableName);
		BIND_DATA			(szFieldName,			f_FieldName);
		BIND_DATA			(szValueToFix,			f_ValueToFix);
		BIND_DATA			(szRelatedErrors,		f_RelatedErrors);
		BIND_DATA			(szValidationDate,		f_ValidationDate);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR TDS_ValidationFKtoFix::GetStaticName() { return _NS_TBL("DS_ValidationFKtoFix"); }

//-----------------------------------------------------------------------------
TCHAR	TDS_ValidationFKtoFix::szID				[]	= _NS_FLD("FKToFixID"); 
TCHAR	TDS_ValidationFKtoFix::szProviderName	[]	= _NS_FLD("ProviderName");
 TCHAR	TDS_ValidationFKtoFix::szDocNamespace	[]	= _NS_FLD("DocNamespace");
 TCHAR	TDS_ValidationFKtoFix::szTableName		[]	= _NS_FLD("TableName");
 TCHAR	TDS_ValidationFKtoFix::szFieldName		[]	= _NS_FLD("FieldName");
 TCHAR	TDS_ValidationFKtoFix::szValueToFix		[]	= _NS_FLD("ValueToFix");
 TCHAR	TDS_ValidationFKtoFix::szRelatedErrors	[]	= _NS_FLD("RelatedErrors");
 TCHAR	TDS_ValidationFKtoFix::szValidationDate	[]	= _NS_FLD("ValidationDate");

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRDS_ValidationInfo
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRDS_ValidationInfo, TableReader)

//------------------------------------------------------------------------------
TRDS_ValidationInfo::TRDS_ValidationInfo(CBaseDocument* pDocument /*= NULL*/)
	: 
	TableReader(RUNTIME_CLASS(TDS_ValidationInfo), pDocument)
{}

//------------------------------------------------------------------------------
void TRDS_ValidationInfo::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn	(GetRecord()->f_DocTBGuid);
	m_pTable->AddParam			(szParamDocTBGuid,	GetRecord()->f_DocTBGuid);

	m_pTable->AddFilterColumn	(GetRecord()->f_ProviderName);	
	m_pTable->AddParam			(szParamProvider,	GetRecord()->f_ProviderName);
}

//------------------------------------------------------------------------------
void TRDS_ValidationInfo::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamDocTBGuid, m_DocTBGuid);
	m_pTable->SetParamValue(szParamProvider, m_ProviderName);
}

//------------------------------------------------------------------------------
BOOL TRDS_ValidationInfo::IsEmptyQuery()
{
	return (m_DocTBGuid.IsEmpty() || m_ProviderName.IsEmpty());
}

//------------------------------------------------------------------------------
TableReader::FindResult TRDS_ValidationInfo::FindRecord(const DataGuid& docTBGuid, const DataStr&  providerName)
{
	m_DocTBGuid = docTBGuid;
	m_ProviderName = providerName;
	return TableReader::FindRecord();
}

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TUDS_ValidationInfo
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TUDS_ValidationInfo, TableUpdater)

//------------------------------------------------------------------------------
TUDS_ValidationInfo::TUDS_ValidationInfo(CAbstractFormDoc* pDocument /*= NULL*/ , CMessages* pMessages/*= NULL*/)
	: 
	TableUpdater(RUNTIME_CLASS(TDS_ValidationInfo), (CBaseDocument*) pDocument, pMessages)
{}

//------------------------------------------------------------------------------
void TUDS_ValidationInfo::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn		(GetRecord()->f_DocTBGuid);
	m_pTable->AddParam				(szParamDocTBGuid, GetRecord()->f_DocTBGuid);
	m_pTable->AddFilterColumn		(GetRecord()->f_ProviderName);	
	m_pTable->AddParam				(szParamProvider, GetRecord()->f_ProviderName);
}

//------------------------------------------------------------------------------
void TUDS_ValidationInfo::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamDocTBGuid,	m_DocTBGuid);
	m_pTable->SetParamValue(szParamProvider,	m_ProviderName);
}

//------------------------------------------------------------------------------
BOOL TUDS_ValidationInfo::IsEmptyQuery()
{
	return (m_DocTBGuid.IsEmpty() || m_ProviderName.IsEmpty());
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUDS_ValidationInfo::FindRecord(const DataGuid& docTBGuid, const DataStr&  providerName, BOOL bLock)
{
	m_DocTBGuid			= docTBGuid;
	m_ProviderName		= providerName;
	return TableUpdater::FindRecord(bLock);
}

/////////////////////////////////////////////////////////////////////////////
//		TableReader :	class TRDS_ValidationFKtoFix
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TRDS_ValidationFKtoFix, TableReader)

//------------------------------------------------------------------------------
TRDS_ValidationFKtoFix::TRDS_ValidationFKtoFix(CBaseDocument* pDocument /*= NULL*/)
	: 
	TableReader(RUNTIME_CLASS(TDS_ValidationFKtoFix), pDocument)
{}

//------------------------------------------------------------------------------
void TRDS_ValidationFKtoFix::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn	(GetRecord()->f_ProviderName);
	m_pTable->AddFilterColumn	(GetRecord()->f_DocNamespace);
	m_pTable->AddFilterColumn	(GetRecord()->f_TableName);
	m_pTable->AddFilterColumn	(GetRecord()->f_FieldName);
	m_pTable->AddFilterColumn	(GetRecord()->f_ValueToFix);

	m_pTable->AddParam			(szParamProvider,		GetRecord()->f_ProviderName);
	m_pTable->AddParam			(szParamDocNS,			GetRecord()->f_DocNamespace);
	m_pTable->AddParam			(szParamTableName,		GetRecord()->f_TableName);
	m_pTable->AddParam			(szParamFieldName,		GetRecord()->f_FieldName);
	m_pTable->AddParam			(szParamValue,			GetRecord()->f_ValueToFix);
}

//------------------------------------------------------------------------------
void TRDS_ValidationFKtoFix::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamProvider,		 m_ProviderName);
	m_pTable->SetParamValue(szParamDocNS,			 m_DocNamespace);
	m_pTable->SetParamValue(szParamTableName,		 m_TableName);
	m_pTable->SetParamValue(szParamFieldName,		 m_FieldName);
	m_pTable->SetParamValue(szParamValue,			 m_Value);
}

//------------------------------------------------------------------------------
BOOL TRDS_ValidationFKtoFix::IsEmptyQuery()
{
	return	(
				m_ProviderName	.IsEmpty() ||
				m_DocNamespace	.IsEmpty() ||
				m_TableName		.IsEmpty() ||
				m_FieldName		.IsEmpty() ||
				m_Value			.IsEmpty() 
			);
}

//------------------------------------------------------------------------------
TableReader::FindResult TRDS_ValidationFKtoFix::FindRecord
(
	const DataStr&  aProviderName,
	const DataStr&  aDocNamespace,
	const DataStr&  aTableName,
	const DataStr&  aFieldName,
	const DataStr&  aValue
)
{
	m_ProviderName	= aProviderName;
	m_DocNamespace	= aDocNamespace;
	m_TableName		= aTableName;
	m_FieldName		= aFieldName;
	m_Value			= aValue;

	return TableReader::FindRecord();
}

/////////////////////////////////////////////////////////////////////////////
//		TableUpdater :	class TUDS_ValidationFKtoFix
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(TUDS_ValidationFKtoFix, TableUpdater)

//------------------------------------------------------------------------------
TUDS_ValidationFKtoFix::TUDS_ValidationFKtoFix(CAbstractFormDoc* pDocument /*= NULL*/ , CMessages* pMessages/*= NULL*/)
	: 
	TableUpdater(RUNTIME_CLASS(TDS_ValidationFKtoFix), (CBaseDocument*) pDocument, pMessages)
{
}

//------------------------------------------------------------------------------
void TUDS_ValidationFKtoFix::OnDefineQuery()
{
	m_pTable->SelectAll();

	m_pTable->AddFilterColumn	(GetRecord()->f_ProviderName);
	m_pTable->AddFilterColumn	(GetRecord()->f_DocNamespace);
	m_pTable->AddFilterColumn	(GetRecord()->f_TableName);
	m_pTable->AddFilterColumn	(GetRecord()->f_FieldName);
	m_pTable->AddFilterColumn	(GetRecord()->f_ValueToFix);

	m_pTable->AddParam			(szParamProvider,		GetRecord()->f_ProviderName);
	m_pTable->AddParam			(szParamDocNS,			GetRecord()->f_DocNamespace);
	m_pTable->AddParam			(szParamTableName,		GetRecord()->f_TableName);
	m_pTable->AddParam			(szParamFieldName,		GetRecord()->f_FieldName);
	m_pTable->AddParam			(szParamValue,			GetRecord()->f_ValueToFix);
}

//------------------------------------------------------------------------------
void TUDS_ValidationFKtoFix::OnPrepareQuery()
{
	m_pTable->SetParamValue(szParamProvider,		 m_ProviderName);
	m_pTable->SetParamValue(szParamDocNS,			 m_DocNamespace);
	m_pTable->SetParamValue(szParamTableName,		 m_TableName);
	m_pTable->SetParamValue(szParamFieldName,		 m_FieldName);
	m_pTable->SetParamValue(szParamValue,			 m_Value);
}

//------------------------------------------------------------------------------
BOOL TUDS_ValidationFKtoFix::IsEmptyQuery()
{
	return	(
				m_ProviderName	.IsEmpty() ||
				m_DocNamespace	.IsEmpty() ||
				m_TableName		.IsEmpty() ||
				m_FieldName		.IsEmpty() ||
				m_Value			.IsEmpty() 
			);
}

//------------------------------------------------------------------------------
TableUpdater::FindResult TUDS_ValidationFKtoFix::FindRecord
(
	const DataStr&  aProviderName,
	const DataStr&  aDocNamespace,
	const DataStr&  aTableName,
	const DataStr&  aFieldName,
	const DataStr&  aValue, 
		  BOOL		bLock
)
{
	m_ProviderName	= aProviderName;
	m_DocNamespace	= aDocNamespace;
	m_TableName		= aTableName;
	m_FieldName		= aFieldName;
	m_Value			= aValue;

	return TableUpdater::FindRecord(bLock);
}


//=============================================================================
//							VSynchronizationInfo
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VSynchronizationInfo, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VSynchronizationInfo::VSynchronizationInfo(BOOL bCallInit)
	:
	SqlVirtualRecord(_T("VSynchronizationInfo")),
	l_SynchStatus(E_SYNCHROSTATUS_TYPE_DEFAULT),
	l_SynchDirection(E_SYNCHRODIRECTION_TYPE_DEFAULT),
	l_LastAction(E_SYNCHROACTION_TYPE_DEFAULT)

{
	l_SynchDate.SetFullDate();
	l_StartSynchDate.SetFullDate();
	BindRecord();

	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VSynchronizationInfo::BindRecord()
{
	BEGIN_BIND_DATA();
	LOCAL_DATA(_NS_LFLD("DocTBGuid"),			l_DocTBGuid);
	LOCAL_STR(_NS_LFLD("ProviderName"),		l_ProviderName, 64);
	LOCAL_STR(_NS_LFLD("DocNamespace"),		l_DocNamespace, 128);
	LOCAL_DATA(_NS_LFLD("SynchStatus"),		l_SynchStatus);
	LOCAL_DATA(_NS_LFLD("SynchDate"),			l_SynchDate);
	LOCAL_DATA(_NS_LFLD("SynchDirection"),	l_SynchDirection);
	LOCAL_DATA(_NS_LFLD("WorkerID"),			l_WorkerID);
	LOCAL_DATA(_NS_LFLD("LastAction"),		l_LastAction);
	LOCAL_DATA(_NS_LFLD("StartSynchDate"),	l_StartSynchDate);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VSynchronizationInfo::GetStaticName() { return _NS_TBL("VSynchronizationInfo"); }


TCHAR	VSynchronizationInfo::szProviderName[] = _NS_FLD("ProviderName");
TCHAR	VSynchronizationInfo::szDocTBGuid[] = _NS_FLD("DocTBGuid");
TCHAR	VSynchronizationInfo::szDocNamespace[] = _NS_FLD("DocNamespace");
TCHAR	VSynchronizationInfo::szSynchStatus[] = _NS_FLD("SynchStatus");
TCHAR	VSynchronizationInfo::szSynchDate[] = _NS_FLD("SynchDate");
TCHAR	VSynchronizationInfo::szSynchDirection[] = _NS_FLD("SynchDirection");
TCHAR	VSynchronizationInfo::szWorkerID[] = _NS_FLD("WorkerID");
TCHAR	VSynchronizationInfo::szLastAction[] = _NS_FLD("LastAction");
TCHAR	VSynchronizationInfo::szStartSynchDate[] = _NS_FLD("StartSynchDate");


///////////////////////////////////////////////////////////////////////////////
//             class TEnhDS_SynchronizationInfo implementation
///////////////////////////////////////////////////////////////////////////////
//
//=============================================================================
IMPLEMENT_DYNCREATE(VEnh_SynchronizationInfo, VSynchronizationInfo)

//-----------------------------------------------------------------------------
VEnh_SynchronizationInfo::VEnh_SynchronizationInfo(BOOL bCallInit)
{
	BindRecord();
	if (bCallInit)
		Init();
}

//-----------------------------------------------------------------------------
void VEnh_SynchronizationInfo::BindRecord()
{
	BEGIN_BIND_DATA();
	LOCAL_STR(_NS_LFLD("VEnh_Sync_Code"), l_Code, 21);
	LOCAL_STR(_NS_LFLD("VEnh_Sync_Description"), l_Description, 128);
	LOCAL_STR(_NS_LFLD("VEnh_WorkerDescri"), l_WorkerDescri, 64);
	LOCAL_STR(_NS_LFLD("VEnh_SynchStatusBmp"), l_SynchStatusBmp, 512);
	LOCAL_STR(_NS_LFLD("VEnh_SynchDirectionBmp"), l_SynchDirectionBmp, 512);
	LOCAL_STR(_NS_LFLD("VEnh_SynchMessage"), l_SynchMessage, 1024);
	END_BIND_DATA();
}


//=============================================================================
//							TDS_ActionsLog
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VActionsLog, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VActionsLog::VActionsLog(BOOL bCallInit)
	:
	SqlVirtualRecord(_T("VActionsLog")),
	f_SynchStatus(E_SYNCHROSTATUS_TYPE_DEFAULT),
	f_SynchDirection(E_SYNCHRODIRECTION_TYPE_DEFAULT),
	f_ActionType(E_SYNCHROACTION_TYPE_DEFAULT)

{
	BindRecord();

	if (bCallInit) Init();
}


//-----------------------------------------------------------------------------
void VActionsLog::BindRecord()
{
	BEGIN_BIND_DATA();	
	LOCAL_DATA(_T("LogId") ,					f_LogId);
	LOCAL_STR(_NS_LFLD("DocNamespace"),			f_DocNamespace, 64);
	LOCAL_DATA(_T("DocTBGuid"),					f_DocTBGuid);
	LOCAL_DATA(_T("ActionType"),				f_ActionType);
	LOCAL_STR(_NS_LFLD("ActionData"),			f_ActionData, 32);
	LOCAL_DATA(_T("SynchDirection"),			f_SynchDirection);
	LOCAL_DATA(_T("SynchXMLData"),				f_SynchXMLData);
	LOCAL_DATA(_T("SynchStatus"),				f_SynchStatus);
	LOCAL_STR(_NS_LFLD("SynchMessage"),			f_SynchMessage, 1024);
	LOCAL_STR(_NS_LFLD("ProviderName"),			f_ProviderName, 64);
	LOCAL_STR(_NS_LFLD("TEnhDS_WorkerDescri"), l_WorkerDescri, 64);
	LOCAL_STR(_NS_LFLD("TEnhDS_SynchStatusBmp"), l_SynchStatusBmp, 32);
	LOCAL_STR(_NS_LFLD("TEnhDS_SynchDirectionBmp"), l_SynchDirectionBmp, 32);
	END_BIND_DATA();
}