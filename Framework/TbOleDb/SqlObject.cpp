
#include "stdafx.h" 

//classi ATL
#include <atldbcli.h>

#include <TbNameSolver\Templates.h>

#include <TBGeneric\globals.h>
#include <TBGeneric\ParametersSections.h>

#include <TBGenlib\baseapp.h>
#include <TBGenlib\messages.h>

#include "oledbmng.h"
#include "sqlconnect.h"
#include "sqltable.h"
#include "sqlproviderinfo.h"
#include "sqlRecoveryManager.h"

#include "sqlobject.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// lost connection errors for each provider type
static const DWORD SQLSERVER_CONNECTION_LOST_ERR				= 11;
static const DWORD ORACLE_CONNECTION_LOST_ERR_NOTAVAILABLE		= 1034;
static const DWORD ORACLE_CONNECTION_LOST_ERR_DISCONNECTED		= 1092;
static const DWORD ORACLE_CONNECTION_LOST_ERR_FATALDISCONNECTED = 1073;
static const DWORD ORACLE_CONNECTION_LOST_ERR_EOFONCHANNEL		= 3113;
static const DWORD ORACLE_CONNECTION_LOST_ERR_QUERYDISCONNECTED = 3114;

//-----------------------------------------------------------------------------
bool DebugTraceSQLEnabled()
{
	BOOL b = *(DataBool*)AfxGetSettingValue(snsTbOleDb, szConnectionSection, szDebugSqlTrace, DataBool(FALSE), szTbDefaultSettingFileName);
	return b == TRUE;
}

//-----------------------------------------------------------------------------
bool AFXAPI IsTraceSQLEnabled()
{
	static bool bSqlTraceEnabled = DebugTraceSQLEnabled();
	return bSqlTraceEnabled;
}
//-----------------------------------------------------------------------------
CString GetCursorTypeDescription (CursorType eCursorType)
{
	switch(eCursorType)
	{
		case E_FORWARD_ONLY:
			return _T("Forward only cursor");
		case E_FAST_FORWARD_ONLY:
			return _T("Fast forward only cursor");
		case E_KEYSET_CURSOR:
			return _T("Keyset cursor");
		case E_DYNAMIC_CURSOR:
			return _T("Dynamic cursor");
		case E_STATIC_CURSOR:
			return _T("Static cursor");
	}

	return _T("Unknown cursor");
}

//-----------------------------------------------------------------------------
void AFXAPI DebugTraceSQL(LPCTSTR szTrace, SqlObject* pSqlObject)
{
	COleDbManager *pOleDbManager = AfxGetOleDbMng();
	if (pOleDbManager)
		pOleDbManager->DebugTraceSQL(szTrace, pSqlObject);
}
//-----------------------------------------------------------------------------
void AFXAPI ResetSQLActionCounters(const CString& strMess)
{
	COleDbManager *pOleDbManager = AfxGetOleDbMng();
	if (pOleDbManager)
		pOleDbManager->ResetSQLActionCounters(strMess);
}

//-----------------------------------------------------------------------------
void AFXAPI TraceSQLSeparator(const CString& strMess)
{
	COleDbManager *pOleDbManager = AfxGetOleDbMng();
	if (pOleDbManager)
		pOleDbManager->TraceSQLSeparator(strMess);
}

//-----------------------------------------------------------------------------
CString AFXAPI GetSQLActionCounters(const CString& strMess)
{
	COleDbManager *pOleDbManager = AfxGetOleDbMng();
	if (!pOleDbManager)
		return NULL;
	return pOleDbManager->GetSQLActionCounters(strMess);
}
//-----------------------------------------------------------------------------
void AFXAPI ThrowSqlException(LPUNKNOWN lpUnk, const IID& iid, HRESULT nHResult, SqlObject* pSqlObj)
{
	SqlException* pExceptionOle = new SqlException(lpUnk, iid, nHResult, pSqlObj);
	
	pExceptionOle->BuildErrorString(nHResult, lpUnk, iid);

	// I try to manage the event. I remove the original SqlException
	// and I thow another one to skip original code and stop operations
	if (
			pSqlObj && pSqlObj->m_pContext && AfxGetSqlRecoveryManager() && 
			!AfxIsInUnattendedMode() && pExceptionOle->IsLostConnectionError (pSqlObj->GetDBMSType())
		)
	{	
		AfxInvokeAsyncThreadFunction<BOOL, SqlRecoveryManager, CString>
		(
			AfxGetLoginContext()->GetThreadId(),
			AfxGetSqlRecoveryManager(), 
			&SqlRecoveryManager::ON_CONNECTION_LOST,	
			pExceptionOle->m_strError
		);
		pExceptionOle->SetDeletable(TRUE);
		delete pExceptionOle;
		AfxThrowUserException();
	}

	THROW(pExceptionOle);
}

//-----------------------------------------------------------------------------
void AFXAPI ThrowSqlException(LPCTSTR pszError, HRESULT nHResult, SqlObject* pSqlObj)
{
	SqlException* pExceptionOle = new SqlException();
	
	pExceptionOle->m_strError = pszError;
	pExceptionOle->m_nHResult = nHResult;
	pExceptionOle->m_pSqlObj = pSqlObj;	

	// #3218 (di qui no ho l'errore di perdita di connessione. 
	// Vedere i casi in cui viene usato questo metodo)
	THROW(pExceptionOle);
}

/////////////////////////////////////////////////////////////////////////////
//						class CBaseContext
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CBaseContext::CBaseContext(CBaseDocument* pDocument /*= NULL*/)
:
	CSharedContext			(pDocument),
	m_bLocked				(FALSE),
	m_pSqlPerformanceMng	(NULL),
	m_pLockMng				(NULL),
	m_pDiagnosticPool		(NULL),
	m_bCanUnlock			(TRUE),
	m_bCanDeleteLockMng		(FALSE)
{
	if (m_pDocument)	
		m_pDiagnostic = m_pDocument->m_pMessages;
	else
		m_pDiagnostic = AfxGetDiagnostic();
	
	m_pDiagnosticPool = new CArray<CDiagnostic*, CDiagnostic*>;
	m_pDiagnosticPool->Add(m_pDiagnostic);
}

//-----------------------------------------------------------------------------
CBaseContext::CBaseContext(const CBaseContext& aBaseContext)
:
	CSharedContext			(aBaseContext),
	m_bLocked				(FALSE),
	m_pSqlPerformanceMng	(NULL),
	m_pLockMng				(NULL),
	m_pDiagnosticPool		(NULL),
	m_bCanUnlock			(FALSE),
	m_bCanDeleteLockMng		(FALSE)

{
	m_pDiagnostic		= aBaseContext.m_pDiagnostic;
	m_pDiagnosticPool	= aBaseContext.m_pDiagnosticPool;
	m_pSqlPerformanceMng = aBaseContext.m_pSqlPerformanceMng;
	m_pLockMng			= aBaseContext.m_pLockMng;
	m_bLocked			= aBaseContext.m_bLocked;
}

//-----------------------------------------------------------------------------
CBaseContext::CBaseContext(CDiagnostic* pDiagnostic)
:
	CSharedContext			(NULL),
	m_bLocked				(FALSE),
	m_pSqlPerformanceMng	(NULL),
	m_pLockMng				(NULL),
	m_pDiagnosticPool		(NULL),
	m_bCanUnlock			(FALSE),	
	m_bCanDeleteLockMng		(FALSE)

{
	m_pDiagnostic = pDiagnostic;	
	m_pDiagnosticPool = new CArray<CDiagnostic*, CDiagnostic*>;
	m_pDiagnosticPool->Add(m_pDiagnostic);
}

//-----------------------------------------------------------------------------
CBaseContext::~CBaseContext()
{
	if (m_bLocked)
		m_pLockMng->RemoveAllLocks(this);
		
	if (m_pLockMng && m_bCanDeleteLockMng)
		delete m_pLockMng;
	
	if (!m_bOwnedContext)
		return;

	if (m_pDiagnosticPool->GetCount() > 1)
		ASSERT(FALSE); // non è stata chiamata la EndShareDiagnostic
	m_pDiagnosticPool->RemoveAt(m_pDiagnosticPool->GetUpperBound());
	delete m_pDiagnosticPool;
}

//-----------------------------------------------------------------------------
void CBaseContext::AddMessage(LPCTSTR lpszMessage, CDiagnostic::MsgType eType)
{
	m_pDiagnostic->Add(lpszMessage, eType);
}

//-----------------------------------------------------------------------------
void CBaseContext::ShowMessage(LPCTSTR lpszMessage, CDiagnostic::MsgType eType)
{
	AddMessage(lpszMessage, eType);
	m_pDiagnostic->Show(TRUE);
}

//-----------------------------------------------------------------------------
void CBaseContext::ShowMessage(BOOL bClear)
{
	m_pDiagnostic->Show(bClear);
}

//-----------------------------------------------------------------------------
void CBaseContext::StartNewDiagnostic(BOOL bShowMsg /*=TRUE*/)
{
	m_pDiagnostic = new CDiagnostic();
	m_pDiagnostic->AttachViewer(AfxCreateDefaultViewer());
	if (!bShowMsg)
		m_pDiagnostic->StartSession();

	m_pDiagnosticPool->Add(m_pDiagnostic);
}

//-----------------------------------------------------------------------------
void CBaseContext::EndNewDiagnostic(BOOL bCopy /*=FALSE*/, const CString& strOpeningBanner /*= _T("")*/, const CString& strClosingBanner /*= _T("")*/)
{
	if (m_pDiagnosticPool->GetUpperBound() <= 0)
	{
		ASSERT(FALSE);
		return;
	}

	m_pDiagnosticPool->RemoveAt(m_pDiagnosticPool->GetUpperBound());
	CDiagnostic* pCurrent = m_pDiagnosticPool->GetAt(m_pDiagnosticPool->GetUpperBound());
	if (bCopy)
		pCurrent->Copy(m_pDiagnostic, TRUE, strOpeningBanner, strClosingBanner);
	delete m_pDiagnostic;
	m_pDiagnostic = pCurrent;
}

 // Lock support

//-----------------------------------------------------------------------------
BOOL CBaseContext::UnlockAll()						
{ 
	return (m_pLockMng && m_bLocked) ? m_pLockMng->UnlockAll(this) : TRUE; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::UnlockAll(LPCTSTR szTableName) 
{ 
	return (m_pLockMng && m_bLocked) ? m_pLockMng->UnlockAll(this, szTableName) : TRUE; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::IsCurrentLocked (SqlTable* pTable)							  
{ 
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->IsCurrentLocked(pTable) : TRUE; 
}
//-----------------------------------------------------------------------------
BOOL CBaseContext::LockCurrent (SqlTable* pTable, BOOL bUseMessageBox /*= TRUE*/, LockRetriesMng* pRetriesMng /*NULL*/) 
{ 
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->LockCurrent(pTable, bUseMessageBox, pRetriesMng) : TRUE; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::UnlockCurrent(SqlTable* pTable) 
{ 
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->UnlockCurrent(pTable) : TRUE; 
}


//-----------------------------------------------------------------------------
BOOL CBaseContext::LockDocument() 
{ 
	SqlLockMng* pLockMng = GetLockMng(m_pDocument->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->LockDocument(this) : TRUE; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::UnlockDocument () 
{ 
	SqlLockMng* pLockMng = GetLockMng(m_pDocument->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->UnlockDocument(this) : TRUE; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::LockTable(SqlTable* pTable, const CString& tableName, BOOL bUseMessageBox/*= TRUE*/, LockRetriesMng* pRetriesMng /*NULL*/)
{
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->LockTable(pTable, tableName, bUseMessageBox, pRetriesMng) : TRUE; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::UnlockTable(SqlTable* pTable, const CString& tableName)
{
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->UnlockTable(pTable, tableName) : TRUE; 
}


//-----------------------------------------------------------------------------
CString	CBaseContext::GetLockMessage (SqlTable* pTable) 
{ 
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->GetLockMessage(pTable) : _T(""); 
}

//-----------------------------------------------------------------------------
void CBaseContext::EnableLocksCache (SqlTable* pTable, const BOOL bValue /*TRUE*/)
{ 
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	if (pLockMng) 
		pLockMng->EnableLocksCache (bValue) ; 
}

//-----------------------------------------------------------------------------
void CBaseContext::ClearLocksCache (SqlTable* pTable, const CString sLockContextKey /* _T("")*/)
{ 
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	if (pLockMng) 
		pLockMng->ClearLocksCache (sLockContextKey) ; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::UnlockTableKey (SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey /*_T("")*/)
{ 
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->UnlockTableKey (sContextKey, pTable, pRec) : FALSE; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::LockTableKey (SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey /*_T("")*/, LockRetriesMng* pRetriesMng /*NULL*/)
{ 
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->LockTableKey (sContextKey, pTable, pRec, pRetriesMng) : FALSE; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::IsTableKeyLocked	 (SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey /*_T("")*/)
{ 
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->IsTableKeyLocked (sContextKey, pTable, pRec) : FALSE; 
}

//-----------------------------------------------------------------------------
BOOL CBaseContext::UnlockAllLockContextKeys(const CString& sLockContextKey, SqlTable* pTable)
{
	SqlLockMng* pLockMng = GetLockMng(pTable->m_pSqlConnection->m_strDBName);
	return (pLockMng) ? pLockMng->UnlockAllLockContextKeys (sLockContextKey) : FALSE; 
}

//-----------------------------------------------------------------------------
SqlLockMng* CBaseContext::GetLockMng(const CString& strDBName)
{
	if (m_pLockMng)
		return m_pLockMng;
	
	if (!AfxGetOleDbMng()->UseLockManager())
		return NULL;

	m_pLockMng = new SqlLockMng(strDBName);
	m_bCanDeleteLockMng = TRUE;
	return m_pLockMng;
}

/////////////////////////////////////////////////////////////////////////////
//						class CDataCachingContext
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CDataCachingContext::CDataCachingContext(CBaseContext* pBaseContext, CBaseDocument* pDocument /*= NULL*/)
	:	
	CSharedContext			(pDocument),
	m_pDataCachingManager	(NULL),
	m_pContext				(pBaseContext),
	m_bOwnDataCachingManager(FALSE)
{
	InheritDataCachingManager (pDocument);
}

//-----------------------------------------------------------------------------
CDataCachingContext::CDataCachingContext(const CDataCachingContext& aDataCachingContext)
	:
	CSharedContext(aDataCachingContext),
	m_pDataCachingManager	(NULL),
	m_pContext				(NULL),
	m_bOwnDataCachingManager(FALSE)
{
	m_pDataCachingManager	= aDataCachingContext.m_pDataCachingManager;
	m_pContext				= aDataCachingContext.m_pContext;
	m_bOwnDataCachingManager= aDataCachingContext.m_bOwnDataCachingManager;
}

//-----------------------------------------------------------------------------
CDataCachingContext::~CDataCachingContext ()
{
	if (m_bOwnedContext && m_bOwnDataCachingManager)
	{
		AfxGetDataCachingUpdatesListener()->RemoveManager (m_pDataCachingManager);
		SAFE_DELETE(m_pDataCachingManager);
	}
}

//----------------------------------------------------------------------------------------------
CDataCachingManager* CDataCachingContext::GetDataCachingManager ()
{
	return m_pDataCachingManager;
}

//----------------------------------------------------------------------------------------------
BOOL CDataCachingContext::IsOwnDataCachingManager()
{
	return m_bOwnDataCachingManager;
}

//----------------------------------------------------------------------------------------------
void CDataCachingContext::SetDataCachingManager (CDataCachingManager* pDataCachingManager)
{
	if (!pDataCachingManager || !AfxGetDataCachingSettings()->IsDataCachingEnabled()) 
		return;

	if (pDataCachingManager == m_pDataCachingManager)
		return;

	if (pDataCachingManager && m_bOwnDataCachingManager)
	{
		AfxGetDataCachingUpdatesListener()->RemoveManager (m_pDataCachingManager);
		SAFE_DELETE (m_pDataCachingManager);
	}

	m_pDataCachingManager	 = pDataCachingManager;
	m_bOwnDataCachingManager = FALSE;

}

//----------------------------------------------------------------------------------------------
void CDataCachingContext::CreateCache ()
{
	if (m_pDataCachingManager && m_bOwnDataCachingManager)
	{
		// add the manager for updates notifications
		AfxGetDataCachingUpdatesListener()->RemoveManager(m_pDataCachingManager);
		SAFE_DELETE(m_pDataCachingManager);
	}
	else 
		m_pDataCachingManager = NULL;

	if (!m_pDataCachingManager)
	{
		m_pDataCachingManager = new CDataCachingManager();
		m_bOwnDataCachingManager = TRUE;
		
		// add the manager for updates notifications
		AfxGetDataCachingUpdatesListener()->AddManager (m_pDataCachingManager);
	}
}

//----------------------------------------------------------------------------------------------
void CDataCachingContext::ClearCache ()
{
	if (m_pDataCachingManager)
	{
		TB_OBJECT_LOCK(m_pDataCachingManager);
		m_pDataCachingManager->ClearCache();
	}
}

//----------------------------------------------------------------------------------------------
void CDataCachingContext::InheritDataCachingManager(CBaseDocument* pDocument)
{
	if (!AfxGetDataCachingSettings() || !AfxGetDataCachingSettings()->IsDataCachingEnabled())
		return;

	switch (AfxGetDataCachingSettings()->GetCacheScope())
	{
		case CDataCachingSettings::LOGIN:
		{
			CDataCachingManager* pDataCachingManager = AfxGetOleDbMng() ? AfxGetOleDbMng()->GetDataCachingManager() : NULL;
			if (pDataCachingManager)
				SetDataCachingManager (pDataCachingManager);
			break;
		}

		case CDataCachingSettings::DOCUMENT_CONTEXT:

		case CDataCachingSettings::DOCUMENT_TRANSACTION:
		{
			if	(pDocument && pDocument->m_pTbContext)
			{
				CDataCachingContext* pDataCachingContext = (CDataCachingContext*) pDocument->m_pTbContext;
				if (pDataCachingContext->GetDataCachingManager())
					SetDataCachingManager (pDataCachingContext->GetDataCachingManager());
			}
			break;
		}
		default:
		{
			CreateCache ();
			break;
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
//						class CTBContext
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CTransactionContext::CTransactionContext(SqlConnection* pSqlConnection, CTBContext* pTBContext, CBaseDocument* pDocument)
:
	CSharedContext			(pDocument),
	m_pSqlConnection		(NULL),
	m_pReadOnlySqlSession	(NULL),
	m_pUpdateSqlSession		(NULL),
	m_pTBContext			(pTBContext),
	m_bTxError				(FALSE)
{

	if (pSqlConnection && pSqlConnection->IsValid())
	{
		m_pUpdateSqlSession   =	pSqlConnection->GetNewSqlSession(m_pTBContext->m_pBaseContext);	
		if (pSqlConnection->GetDBMSType() == DBMS_ORACLE)
			m_pReadOnlySqlSession = m_pUpdateSqlSession;
		else
			m_pReadOnlySqlSession = pSqlConnection->GetNewSqlSession(m_pTBContext->m_pBaseContext);
		m_pSqlConnection = pSqlConnection;
		m_pTBContext->m_pBaseContext->SetCanUnlock();
	}
}


//-----------------------------------------------------------------------------
CTransactionContext::CTransactionContext(const CTransactionContext& aTransactionContext)
:
	CSharedContext			(aTransactionContext),
	m_pSqlConnection		(NULL),
	m_pReadOnlySqlSession	(NULL),	
	m_pUpdateSqlSession		(NULL),
	m_pTBContext			(NULL),
	m_bTxError				(FALSE)

{
	m_pSqlConnection		= aTransactionContext.m_pSqlConnection;
	m_pReadOnlySqlSession	= aTransactionContext.m_pReadOnlySqlSession;
	m_pUpdateSqlSession		= aTransactionContext.m_pUpdateSqlSession;
	m_pTBContext			= aTransactionContext.m_pTBContext;
	m_bTxError				= aTransactionContext.m_bTxError;
}

//-----------------------------------------------------------------------------
CTransactionContext::~CTransactionContext()
{
	if (!m_bOwnedContext)
		return;

	if (m_pUpdateSqlSession)
	{
		if (m_pReadOnlySqlSession == m_pUpdateSqlSession)
			m_pReadOnlySqlSession = NULL;
		
		m_pUpdateSqlSession->Close();
		delete m_pUpdateSqlSession;			
	}			
		
	if (m_pReadOnlySqlSession)
	{
		m_pReadOnlySqlSession->Close();
		delete m_pReadOnlySqlSession;
	}
}

//-----------------------------------------------------------------------------
BOOL CTransactionContext::IsValid()
{
	return 	m_pSqlConnection && 
			m_pSqlConnection->IsValid() &&
			m_pSqlConnection->TablesPresent(); //é giusto ció?????
}

//-----------------------------------------------------------------------------
SqlSession* CTransactionContext::OpenNewSqlSession()
{
	SqlSession* pSqlSession = NULL;
	if (m_pSqlConnection && m_pSqlConnection->IsValid())
		pSqlSession = m_pSqlConnection->GetNewSqlSession(m_pReadOnlySqlSession->m_pContext);
	
	return pSqlSession;
}

//-----------------------------------------------------------------------------
BOOL CTransactionContext::TransactionPending()
{
	return m_pUpdateSqlSession && m_pUpdateSqlSession->m_bTxnInProgress;
}

//-----------------------------------------------------------------------------
BOOL CTransactionContext::StartTransaction()
{
	if (!m_pUpdateSqlSession)
		ThrowSqlException(_TB("Invalid session. Unable to perform transaction operations.")); 
	TRY
	{
		// se lo spazio transazionale non è più condiviso oppure è condiviso ma il metodo è stato chiamato dal creatore del contesto
		if (m_bOwnedContext || !m_pUpdateSqlSession->IsTxnInProgress())
		{
			m_pUpdateSqlSession->StartTransaction();
			//TraceError(cwsprintf(_T("Transaction start on session %lp, context %lp for document class %s\r\n"),	m_pUpdateSqlSession, m_pUpdateSqlSession->m_pContext, CString(m_pUpdateSqlSession->m_pContext->GetDocument()->GetRuntimeClass()->m_lpszClassName)));
			m_bTxError = FALSE;
			TRACE_SQL(cwsprintf(_T("Transaction start on session %lp, context %lp for document class %s"),
								m_pUpdateSqlSession, m_pUpdateSqlSession->m_pContext, CString(m_pUpdateSqlSession->m_pContext->GetDocument()->GetRuntimeClass()->m_lpszClassName)), 
								m_pUpdateSqlSession);
		}
	}
	CATCH(SqlException, e)
	{
		return FALSE;
	}
	END_CATCH
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void CTransactionContext::Rollback(SqlPerformanceManager* pSqlPerformanceMng)
{
	if (!m_pUpdateSqlSession)
		ThrowSqlException(_TB("Invalid session. Unable to perform transaction operations.")); 

	TRY
	{
		// il roolback viene effettuato solo dal creatore del contesto
		if (m_bOwnedContext) 
		{
			if (pSqlPerformanceMng) pSqlPerformanceMng->MakeProcTimeOperation(START_TIME, PROC_ROLLBACK);
			m_pUpdateSqlSession->Abort();
			m_pTBContext->OnRollbackTransaction();
			if (pSqlPerformanceMng) pSqlPerformanceMng->MakeProcTimeOperation(STOP_TIME, PROC_ROLLBACK);
			//TraceError(cwsprintf(_T("Transaction rollback on session %lp, context %lp for document class %s\r\n"), m_pUpdateSqlSession, m_pUpdateSqlSession->m_pContext, CString(m_pUpdateSqlSession->m_pContext->GetDocument()->GetRuntimeClass()->m_lpszClassName)));
			TRACE_SQL(cwsprintf(_T("Transaction rollback on session %lp, context %lp for document class %s"),
								m_pUpdateSqlSession, m_pUpdateSqlSession->m_pContext, CString(m_pUpdateSqlSession->m_pContext->GetDocument()->GetRuntimeClass()->m_lpszClassName)), 
								m_pUpdateSqlSession);
								
		}
		else
			//se non sono il possessore del contesto allora segnalo che c'é stato un errore
			m_bTxError = m_bTxError || TRUE;	
	}
		
	CATCH(SqlException, e)
	{
		THROW_LAST();
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void CTransactionContext::Commit(SqlPerformanceManager* pSqlPerformanceMng)
{
	if (!m_pUpdateSqlSession)
		ThrowSqlException(_TB("Invalid session. Unable to perform transaction operations.")); 
	
	TRY
	{
		if (m_bOwnedContext) 
		{
			if (m_bTxError)
			{
				m_pUpdateSqlSession->AddMessage(_TB("Transaction error. Changes effected will be not saved!"));
				if (pSqlPerformanceMng) pSqlPerformanceMng->MakeProcTimeOperation(START_TIME, PROC_ROLLBACK);
				m_pUpdateSqlSession->Abort();
				m_pTBContext->OnRollbackTransaction();

				if (m_pDocument)
					m_pDocument->RemoveNotifications();

				if (pSqlPerformanceMng) pSqlPerformanceMng->MakeProcTimeOperation(STOP_TIME, PROC_ROLLBACK);	
				/*TraceError(cwsprintf(_T("Transaction rollback due to commit with error on session %lp, context %lp for document class %s"),
									m_pUpdateSqlSession, m_pUpdateSqlSession->m_pContext, CString(m_pUpdateSqlSession->m_pContext->GetDocument()->GetRuntimeClass()->m_lpszClassName)));*/
				TRACE_SQL(cwsprintf(_T("Transaction rollback due to commit with error on session %lp, context %lp for document class %s"),
									m_pUpdateSqlSession, m_pUpdateSqlSession->m_pContext, CString(m_pUpdateSqlSession->m_pContext->GetDocument()->GetRuntimeClass()->m_lpszClassName)), 
									m_pUpdateSqlSession);
			}
			else
			{
				if (pSqlPerformanceMng) pSqlPerformanceMng->MakeProcTimeOperation(START_TIME, PROC_COMMIT);
				m_pUpdateSqlSession->Commit();		
				//fare commit degli oggetti condivisi
				m_pTBContext->OnCommitTransaction();
				/*TraceError(cwsprintf(_T("Transaction commit on session %lp, context %lp for document class %s"),
									 m_pUpdateSqlSession, m_pUpdateSqlSession->m_pContext, CString(m_pUpdateSqlSession->m_pContext->GetDocument()->GetRuntimeClass()->m_lpszClassName)));*/
				if (m_pDocument)
					m_pDocument->NotifiyToDataSynchronizer();

				if (pSqlPerformanceMng) pSqlPerformanceMng->MakeProcTimeOperation(STOP_TIME, PROC_COMMIT);			
				TRACE_SQL(cwsprintf(_T("Transaction commit on session %lp, context %lp for document class %s"),
									m_pUpdateSqlSession, m_pUpdateSqlSession->m_pContext, CString(m_pUpdateSqlSession->m_pContext->GetDocument()->GetRuntimeClass()->m_lpszClassName)), 
									m_pUpdateSqlSession);
			}		
			m_bTxError = FALSE;
		}
	}
	CATCH(SqlException, e)
	{
		THROW_LAST();
	}
	END_CATCH
}


//-----------------------------------------------------------------------------
void CTransactionContext::SetDataCachingContext(CDataCachingContext* pDataCachingContext)
{
	if (m_pReadOnlySqlSession)
		m_pReadOnlySqlSession->SetDataCachingContext(pDataCachingContext);
	if (m_pUpdateSqlSession)
		m_pUpdateSqlSession->SetDataCachingContext(pDataCachingContext);
}


/////////////////////////////////////////////////////////////////////////////
//						class CTBContext
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CTBContext::CTBContext()
:
	m_pBaseContext				(NULL),
	m_pCurrDataCachingContext	(NULL),
	m_pCurrTransactionContext	(NULL),
	m_pSqlPerformanceDlg		(NULL),
	m_bShareTransactionContext	(TRUE),
	m_bShareDataCachingContext	(TRUE)
{
	m_wContextStatus = TRANSACTION_STATUS | CACHING_STATUS | DIAGNOSTIC_STATUS;	
	m_pBaseContext = new CBaseContext();
}

//-----------------------------------------------------------------------------
CTBContext::CTBContext(SqlConnection* pSqlConnection, CBaseDocument* pDocument /*=NULL*/)
:
	m_pBaseContext				(NULL),
	m_pCurrDataCachingContext	(NULL),
	m_pCurrTransactionContext	(NULL),
	m_pCustomObjects			(NULL),
	m_pSqlPerformanceMng		(NULL),
	m_pSqlPerformanceDlg		(NULL),
	m_bShareTransactionContext	(TRUE),
	m_bShareDataCachingContext	(TRUE)
{
	m_wContextStatus = TRANSACTION_STATUS | CACHING_STATUS | DIAGNOSTIC_STATUS;	
	m_pBaseContext = new CBaseContext(pDocument);
	m_pCurrTransactionContext = new CTransactionContext((pSqlConnection) ? pSqlConnection : AfxGetDefaultSqlConnection(), this, pDocument);
	m_pCurrDataCachingContext = new CDataCachingContext(m_pBaseContext, pDocument);
	m_pCurrTransactionContext->SetDataCachingContext(m_pCurrDataCachingContext);
	m_pCustomObjects = new CArray<CContextObject*, CContextObject*>;
}

//-----------------------------------------------------------------------------
CTBContext::CTBContext(const CTBContext& aTBContext, CBaseDocument* pDocument)
:
	m_pCurrDataCachingContext	(NULL),
	m_pCurrTransactionContext	(NULL),
	m_pSqlPerformanceMng		(NULL),
	m_pSqlPerformanceDlg		(NULL),
	m_bShareTransactionContext	(TRUE),
	m_bShareDataCachingContext	(TRUE)
{
	m_wContextStatus = TRANSACTION_STATUS | CACHING_STATUS | DIAGNOSTIC_STATUS;	
	m_pSqlPerformanceMng	 = aTBContext.m_pSqlPerformanceMng;
	m_pSqlPerformanceDlg	 = aTBContext.m_pSqlPerformanceDlg;
	m_pCustomObjects		 = aTBContext.m_pCustomObjects;
	//m_bShareTransactionContext = aTBContext.m_bShareTransactionContext;
	//m_bShareDataCachingContext = aTBContext.m_bShareDataCachingContext;

	m_pBaseContext = new CBaseContext(*aTBContext.m_pBaseContext);

	if (aTBContext.m_bShareTransactionContext)
	{
		m_pCurrTransactionContext = new CTransactionContext(*aTBContext.m_pCurrTransactionContext);
		/*TraceError(cwsprintf(_T("CTBContext using same transaction context with UpdatableSession %lp (original UpdatableSqlSession : %lp)"), 
								m_pCurrTransactionContext->GetUpdatableSqlSession(), 
								aTBContext.m_pCurrTransactionContext->GetUpdatableSqlSession())); */

		TRACE_SQL(cwsprintf(_T("CTBContext using same transaction context with UpdatableSession %lp (original UpdatableSqlSession : %lp)"), 
								m_pCurrTransactionContext->GetUpdatableSqlSession(), 
								aTBContext.m_pCurrTransactionContext->GetUpdatableSqlSession()),
								m_pCurrTransactionContext->GetUpdatableSqlSession());
	}
	else
	{
		m_pCurrTransactionContext = new CTransactionContext((aTBContext.GetSqlConnection()) ? aTBContext.GetSqlConnection() : AfxGetDefaultSqlConnection(), this, pDocument);
		/*TraceError(cwsprintf(_T("CTBContext create new transaction context with UpdatableSession %lp (original UpdatableSqlSession : %lp)"), 
								m_pCurrTransactionContext->GetUpdatableSqlSession(), 
								aTBContext.m_pCurrTransactionContext->GetUpdatableSqlSession()));*/
		TRACE_SQL(cwsprintf(_T("CTBContext create new transaction context with UpdatableSession %lp (original UpdatableSqlSession : %lp)"), 
								m_pCurrTransactionContext->GetUpdatableSqlSession(), 
								aTBContext.m_pCurrTransactionContext->GetUpdatableSqlSession()),
								m_pCurrTransactionContext->GetUpdatableSqlSession());
	}


	if (aTBContext.m_bShareDataCachingContext)
		m_pCurrDataCachingContext = new CDataCachingContext(*aTBContext.m_pCurrDataCachingContext); 
	else
		m_pCurrDataCachingContext = new CDataCachingContext(m_pBaseContext, pDocument);
}



//-----------------------------------------------------------------------------
CTBContext::~CTBContext()
{
	if (IsOwnContext())
	{	
		if (m_pSqlPerformanceDlg)
			m_pSqlPerformanceDlg->CloseDialog();

		if (m_pSqlPerformanceMng)
			delete m_pSqlPerformanceMng;

		if (m_pCustomObjects)
		{
			for (int i = m_pCustomObjects->GetUpperBound(); i >= 0; i--)
			{
				CContextObject* pObject = m_pCustomObjects->GetAt(i);
				delete pObject;				
			}
			delete m_pCustomObjects;
		}
	}
	delete m_pCurrTransactionContext;		
	delete m_pCurrDataCachingContext;	
	delete m_pBaseContext;
}


//-----------------------------------------------------------------------------
BOOL CTBContext::IsValid()
{
	return m_pCurrTransactionContext->IsValid();
}
	
//-----------------------------------------------------------------------------
void CTBContext::AttachCustomObject(CContextObject* pObject)
{
	m_pCustomObjects->Add(pObject);
}

//------------------------------------------------------------------------------
void CTBContext::DeattachCustomObject(CContextObject* pObject)
{
	for (int i = m_pCustomObjects->GetUpperBound(); i >= 0; i--)
	{
		CContextObject* pObj = m_pCustomObjects->GetAt(i);
		if (pObj == pObject)
		{
			m_pCustomObjects->RemoveAt(i);
			delete pObj;
		}
	}
}

//------------------------------------------------------------------------------
void CTBContext::DeattachCustomObject(LPCTSTR lpszObjectName)
{
	for (int i = m_pCustomObjects->GetUpperBound(); i >= 0; i--)
	{
		CContextObject* pObj = m_pCustomObjects->GetAt(i);
		if (pObj->GetObjectName() == lpszObjectName)
		{
			m_pCustomObjects->RemoveAt(i);
			delete pObj;
		}
	}
}

//-----------------------------------------------------------------------------
CContextObject*	CTBContext::GetCustomObject(LPCTSTR lpszObjectName)
{
	for (int i = 0; i < m_pCustomObjects->GetCount(); i++)
	{
		CContextObject* pObject = m_pCustomObjects->GetAt(i);
		if (_tcscmp(pObject->GetObjectName(), lpszObjectName) == 0)
			return pObject;
	}
	return NULL;
}


//-----------------------------------------------------------------------------
void CTBContext::Rollback()
{
	TRY
	{
		m_pCurrTransactionContext->Rollback(m_pSqlPerformanceMng);
		// event viewer trace has been disabled
		DataObj* pSetting = AfxGetSettingValue	(
													snsTbOleDb, 
													szConnectionSection, 
													szEnableEventViewerLog, 
													DataBool(FALSE), 
													szTbDefaultSettingFileName
												);
		if (pSetting && pSetting->GetDataType() == DATA_BOOL_TYPE && *((DataBool*) pSetting))
			WriteEventViewerMessage(FormatRollbackLogMessage(), EVENTLOG_WARNING_TYPE);
	}
	CATCH(SqlException, e)
	{
		THROW_LAST();
	}
	END_CATCH

}

//-----------------------------------------------------------------------------
void CTBContext::Commit()
{
	TRY
	{
		m_pCurrTransactionContext->Commit(m_pSqlPerformanceMng);
	}
	CATCH(SqlException, e)
	{
		THROW_LAST();
	}
	END_CATCH

}


//----------------------------------------------------------------------------------------------
CString CTBContext::FormatRollbackLogMessage ()
{
	CString sMessage = cwsprintf
		(
			_T("\r\nTransaction rolled back by application in %s document.\r\nThe user connected at rollback time was %s on the client %s."), 
			GetDocument() ? GetDocument()->GetNamespace().ToUnparsedString () : _TB("Unknown"),
			AfxGetLoginInfos()->m_strUserName,
			GetComputerName(TRUE)
		);

	if  (GetDocument() && GetDocument()->GetType() != VMT_BATCH)
	{
		CString sDocMsg = GetDocument()->FormatRollbackLogMessage();
		if (!sDocMsg.IsEmpty())
			sMessage += cwsprintf (_T("\r\nThe primary key of the master data involved was: \"%s\""),  sDocMsg);
	}

	return sMessage;
}


//gestione performance
//----------------------------------------------------------------------------------------------
SqlPerformanceManager* CTBContext::CreateSqlPerformanceMng()
{
	m_pSqlPerformanceMng = new SqlPerformanceManager(this);	
	m_pBaseContext->m_pSqlPerformanceMng = m_pSqlPerformanceMng;
	return m_pSqlPerformanceMng;
}

//----------------------------------------------------------------------------------------------
void CTBContext::DestroySqlPerformanceMng()
{
	SAFE_DELETE(m_pSqlPerformanceMng);
	m_pBaseContext->m_pSqlPerformanceMng = NULL;
}

//----------------------------------------------------------------------------------------------
void CTBContext::SetSqlPerformanceDlg(SqlPerformanceDlg* pDialog)
{
	m_pSqlPerformanceDlg = pDialog;
}

//-----------------------------------------------------------------------------
void CTBContext::OnStartTransaction() 
{ 
	for (int i = 0; i < m_pCustomObjects->GetCount(); i++) 
	{
		CContextObject* pObject = m_pCustomObjects->GetAt(i);
		pObject->OnStartTransaction();
	}
}

//-----------------------------------------------------------------------------
void CTBContext::OnCommitTransaction() 
{
	for (int i = 0; i < m_pCustomObjects->GetCount(); i++) 
	{
		CContextObject* pObject = m_pCustomObjects->GetAt(i);
		pObject->OnCommitTransaction();
	}
}

//-----------------------------------------------------------------------------
void CTBContext::OnRollbackTransaction() 
{
	for (int i = 0; i < m_pCustomObjects->GetCount(); i++) 
	{
		CContextObject* pObject =m_pCustomObjects->GetAt(i);
		pObject->OnRollbackTransaction();
	}
}

//-----------------------------------------------------------------------------
void CTBContext::StartDiagnosticContext(BOOL bShowMsg /*=TRUE*/, BOOL bChangeable /*=TRUE*/)
{
	if (IsContextStatusChangeable(DIAGNOSTIC_STATUS))
	{
		m_pBaseContext->StartNewDiagnostic(bShowMsg); 
		SetContextStatus(DIAGNOSTIC_STATUS, bChangeable); 
	}
	else
		ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CTBContext::EndDiagnosticContext(BOOL bCopy /*=TRUE*/, const CString& strOpeningBanner /*= _T("")*/, const CString& strClosingBanner /*= _T("")*/)
{
	m_pBaseContext->EndNewDiagnostic(bCopy, strOpeningBanner, strClosingBanner); 
	SetContextStatus(DIAGNOSTIC_STATUS, TRUE); 
}

//-----------------------------------------------------------------------------
void CTBContext::StartTransactionContext(BOOL bChangeable /*=TRUE*/)
{
	if (IsContextStatusChangeable(TRANSACTION_STATUS))
	{
		m_bShareTransactionContext = FALSE;	
		SetContextStatus(TRANSACTION_STATUS, bChangeable);
	}
	else
		ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CTBContext::StartDataCachingContext(BOOL bChangeable /*=TRUE*/)
{
	if (IsContextStatusChangeable(CACHING_STATUS))
	{	
		m_bShareDataCachingContext = FALSE;
		SetContextStatus(CACHING_STATUS, bChangeable);
	}
	else
		ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void CTBContext::SetContextStatus(ContextStatus eStatusFlag, BOOL bChangeable) 
{ 
	m_wContextStatus = bChangeable ? 
						(WORD)(m_wContextStatus | eStatusFlag) : 
						(WORD)(m_wContextStatus & ~eStatusFlag);
}


//-----------------------------------------------------------------------------
BOOL CTBContext::IsContextStatusChangeable(ContextStatus eStatusFlag) 
{ 
	return (m_wContextStatus & eStatusFlag) == eStatusFlag;
}


/////////////////////////////////////////////////////////////////////////////
//						class ATLDBPropSet
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class ATLDBPropSet : public CDBPropSet
{
};

/////////////////////////////////////////////////////////////////////////////
//						class SqlObject
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlObject, CObject)

//-----------------------------------------------------------------------------
SqlObject::SqlObject(CBaseContext* pBaseContext/* = NULL*/, CBaseDocument* pDocument/*NULL*/)
:
	m_pDBPropSet			(NULL),
	m_hResult				(S_OK)
{

	m_bOwnContext	= (pBaseContext == NULL);
	m_pContext		= (pBaseContext) 
						? pBaseContext
						: new CBaseContext(pDocument);

	m_pDBPropSet = new ATLDBPropSet;	
}


//-----------------------------------------------------------------------------
SqlObject::~SqlObject()
{
	if (m_pDBPropSet)
		delete m_pDBPropSet;

	if (m_pContext && m_bOwnContext)
		delete m_pContext;
}

//-----------------------------------------------------------------------------
void SqlObject::SetContext(CBaseContext* pBaseContext)
{
	if (pBaseContext)
	{
		if (m_bOwnContext)
			delete m_pContext;

		m_pContext = pBaseContext;
		m_bOwnContext = FALSE;
	}
}

//-----------------------------------------------------------------------------
void SqlObject::CreatePropertySet()
{
	if (!m_pDBPropSet)
		m_pDBPropSet = new ATLDBPropSet();
}

//-----------------------------------------------------------------------------
void SqlObject::RemovePropertySet()
{
	if (m_pDBPropSet)
	{
		delete m_pDBPropSet;
		m_pDBPropSet = NULL;
	}
}
//-----------------------------------------------------------------------------
void SqlObject::SetPropGUID(const GUID& guid)
{
	if (m_pDBPropSet)
		m_pDBPropSet->SetGUID(guid);	
}

//-----------------------------------------------------------------------------
BOOL SqlObject::AddProperty(DWORD dwPropertyIDSet, const VARIANT& varValue)
{	
	return m_pDBPropSet && m_pDBPropSet->AddProperty(dwPropertyIDSet, varValue);
}

//-----------------------------------------------------------------------------
BOOL SqlObject::AddProperty(DWORD dwPropertyIDSet, bool bValue)
{	
	return m_pDBPropSet && m_pDBPropSet->AddProperty(dwPropertyIDSet, bValue);
}	

//-----------------------------------------------------------------------------
BOOL SqlObject::AddProperty(DWORD dwPropertyIDSet, long lValue)
{	
	return m_pDBPropSet && m_pDBPropSet->AddProperty(dwPropertyIDSet, lValue);
}	

//-----------------------------------------------------------------------------
HRESULT SqlObject::GetProperty(DWORD dwPropertyID, VARIANT* pVarValue)
{
	if (!m_pDBPropSet)
		return FALSE; 

	for (ULONG i = 0; i <= m_pDBPropSet->cProperties; i++)
	{
		if (m_pDBPropSet->rgProperties[i].dwPropertyID  == dwPropertyID)
			return ::VariantCopy(pVarValue, &(m_pDBPropSet->rgProperties[i].vValue));			
	}
	
	return S_FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlObject::Check(HRESULT hResult)
{
	m_hResult = hResult;
	return m_hResult == S_OK;
}

//-----------------------------------------------------------------------------
void SqlObject::EnableLocksCache (SqlTable* pTable, const BOOL bValue /*TRUE*/)
{
	if (m_pContext)
		m_pContext->EnableLocksCache(pTable, bValue);
}

//-----------------------------------------------------------------------------
void SqlObject::ClearLocksCache (SqlTable* pTable, const CString sLockContextKey /*_T("")*/)
{
	if (m_pContext)
		m_pContext->ClearLocksCache(pTable, sLockContextKey);
}

//-----------------------------------------------------------------------------
BOOL SqlObject::LockTableKey (SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey /*_T("")*/, LockRetriesMng* pRetriesMng /*NULL*/)
{
	return m_pContext ? m_pContext->LockTableKey(pTable, pRec, sContextKey, pRetriesMng) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlObject::UnlockTableKey (SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey /*_T("")*/)
{
	return m_pContext ?  m_pContext->UnlockTableKey(pTable, pRec, sContextKey) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlObject::IsTableKeyLocked (SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey /*_T("")*/)
{
	return m_pContext ?  m_pContext->IsTableKeyLocked(pTable, pRec, sContextKey) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlObject::UnlockAllLockContextKeys(const CString& sLockContextKey, SqlTable* pTable)
{
	return m_pContext ?  m_pContext->UnlockAllLockContextKeys(sLockContextKey, pTable) : FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlObject::IsCurrentLocked(SqlTable* pTable)
{
	return m_pContext ? m_pContext->IsCurrentLocked(pTable) : FALSE;
}

/////////////////////////////////////////////////////////////////////////////
//						class SqlException
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlException, CException)

//-----------------------------------------------------------------------------
SqlException::SqlException
	(
		LPUNKNOWN		lpUnk, 
		const IID&		iid, 
		HRESULT			nHResult/*= S_OK*/,
		SqlObject*		pSqlObj /*=NULL*/,
		DWORD			wNativeErrCode /*= 0*/
	)
:
	m_pSqlObj		(pSqlObj),
	m_nHResult		(nHResult),
	m_iid			(iid),
	m_lpUnk			(lpUnk),
	m_wNativeErrCode(wNativeErrCode)
{	
	if (m_lpUnk != NULL)
		m_lpUnk->AddRef();	
}

//-----------------------------------------------------------------------------
SqlException::SqlException()	
:
	m_pSqlObj		(NULL),
	m_nHResult		(S_OK),
	m_iid			(NULL_GUID),
	m_lpUnk			(NULL),
	m_wNativeErrCode(0)
{
}

//-----------------------------------------------------------------------------
SqlException::~SqlException()
{
	if (m_lpUnk != NULL)
	{
		m_lpUnk->Release();
		m_lpUnk = NULL;
	}
}

//-----------------------------------------------------------------------------
void SqlException::BuildErrorString(HRESULT hResult, LPUNKNOWN lpUnk, const IID& m_iid)
{
	ASSERT_VALID(this);
	
	//STE
	USES_CONVERSION;

	CDBErrorInfo errInfo;
	ULONG ulRecords = 0;
	HRESULT hr;
	IErrorRecords* pErrRecords = NULL;

	// non riesco ad avere il messaggio di errore allora
	// me lo costruisco 
	if (!m_lpUnk)
	{
		GetErrorString(hResult, m_strError);
		return;
	}

	hr = errInfo.GetErrorRecords(m_lpUnk, m_iid, &ulRecords);
	if (
			FAILED(hr)|| 
			hr == S_FALSE || 
			ulRecords == 0
		)
	{
		GetErrorString(hResult, m_strError);
		return;
	}

	//errInfo.QueryInterface(IID_IErrorRecords,	(void **) &pErrRecords);
	//if (pErrRecords == NULL) 
	//{
	//	GetErrorString(hResult, m_strError);
	//	return;
	//}

	LCID lcid = GetUserDefaultLCID();
	struct SQLERRORINFO* pInfo = new SQLERRORINFO;

	for (ULONG nRec = 0; nRec < ulRecords; nRec++)
	{
		// Get the error information from the source
		hr = errInfo.GetAllErrorInfo(nRec, lcid, &pInfo->bstrDescription,
			&pInfo->bstrSource, &pInfo->guid, &pInfo->dwHelpContext,
			&pInfo->bstrHelpFile);
		if (FAILED(hr))
			continue;
		//gestione della stringa....  m_strError
		m_strError +=  pInfo->bstrDescription;

		ERRORINFO aErr;
		errInfo.GetBasicErrorInfo(nRec, &aErr);
		m_wNativeErrCode =  aErr.dwMinor;
		if (!m_wNativeErrCode)
			m_wNativeErrCode = GetNativeFromDescription(m_strError);
	}	
	
	if (pInfo) delete pInfo;
}	

//-----------------------------------------------------------------------------	
DWORD SqlException::GetNativeFromDescription(const CString& strNewError)
{
	int nErrNative = 0;

	if (!m_pSqlObj || m_pSqlObj->GetDBMSType() == DBMS_ORACLE)
		return nErrNative; 

	CString sError (strNewError);
	sError = sError.MakeLower();
	if (
			sError.Find (_T("[dbnetlib]")) >= 0||
			sError.Find (_T("connection")) >= 0
		)
		nErrNative = SQLSERVER_CONNECTION_LOST_ERR;
	
	return nErrNative;
}
	
//-----------------------------------------------------------------------------	
BOOL SqlException::GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError, _Out_opt_ PUINT pnHelpContext /*= NULL*/) const 
{
	_tcscpy_s(lpszError, nMaxError, m_strError);
	return TRUE;
}

//-----------------------------------------------------------------------------	
BOOL SqlException::GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError, _Out_opt_ PUINT pnHelpContext /*= NULL*/)
{
	_tcscpy_s(lpszError, nMaxError, m_strError);
	return TRUE;
}

//-----------------------------------------------------------------------------	
void SqlException::GetErrorString(HRESULT hResult, CString& m_strError)
{
	switch (hResult)
	{
		case E_NOINTERFACE:
			m_strError = _TB("OLEDB: The method requires an interface not supported by the provider.");
			break;
		case DB_E_NOTSUPPORTED:
			m_strError = _TB("OLEDB: Method not supported by the provider.");
			break;
		case E_INVALIDARG:
			m_strError = _TB("OLEDB: Argument error for OLEDB call.");
			break;
		case E_FAIL:
			m_strError = _TB("OLEDB: Provider specific error.");
			break;
		case E_UNEXPECTED:
			m_strError = _TB("OLEDB: An interface method has been called, and the object is in zombie status.");
			break;
		case DB_E_NOTREENTRANT:
			m_strError = _TB("OLEDB: The provider calls a method in the consumer that has not yet returned any."); 
			break;
		case DB_E_ABORTLIMITREACHED:
			m_strError = _TB("OLEDB: has been aborted because a resource limit has been reached. For example, the preparation timed out.");
			break;
		case E_OUTOFMEMORY:
			m_strError = _TB("OLEDB: The provider is unable to allocate sufficient memory to handle the rowset.");
			break;
		case DB_E_NOTABLE:
			m_strError = _TB("OLEDB: The specific table or view does not exist in the data store.");
			break;
		default:
			if (m_pSqlObj)
				m_pSqlObj->GetErrorString(hResult, m_strError);
	}
	if (m_strError.IsEmpty())
		m_strError = _TB("OLEDB: Generic error while communicating with database.");

	TRACE1("%s\n", m_strError);
}

// arrichisce la stringa di errore di ulteriori informazioni, ponendo la nuova informazione
// dopo (default) o prima del vecchio messaggio
//-----------------------------------------------------------------------------
void SqlException::UpdateErrorString(const CString& strNewError, BOOL bAppend /*= TRUE*/)
{
	if (bAppend)
		m_strError += LF_CHAR + strNewError;
	else
		m_strError = strNewError + LF_CHAR + m_strError;
}


// Display Error Message and alway return FALSE
//-----------------------------------------------------------------------------	
BOOL SqlException::ShowError(LPCTSTR pszError/* = _T("")*/)
{
	CString msg = pszError;
	msg += _T("\n")+ m_strError;
			
	AfxMessageBox (msg, MB_OK | MB_ICONSTOP);
    return FALSE;
}

//-----------------------------------------------------------------------------
void SqlException::SetDeletable	(const BOOL& bValue)
{
#ifdef _DEBUG
	m_bReadyForDelete = bValue;
#endif
}

//-----------------------------------------------------------------------------
void SqlException::GetLostConnectionErrors (CWordArray& arErrors, const int eDBMSType) const
{
	arErrors.RemoveAll();

	if (!m_pSqlObj)
	{
		ASSERT(FALSE);
		TRACE ("SqlException::GetLostConnectionErrors call with null SqlObject");
		return;
	}

	// supported errors for connection lost
	switch (m_pSqlObj->GetDBMSType())
	{
	case DBMS_SQLSERVER:
		arErrors.Add (SQLSERVER_CONNECTION_LOST_ERR);
		break;
	case DBMS_ORACLE:
		arErrors.Add (ORACLE_CONNECTION_LOST_ERR_DISCONNECTED);
		arErrors.Add (ORACLE_CONNECTION_LOST_ERR_NOTAVAILABLE);
		arErrors.Add (ORACLE_CONNECTION_LOST_ERR_FATALDISCONNECTED);
		arErrors.Add (ORACLE_CONNECTION_LOST_ERR_QUERYDISCONNECTED);
		arErrors.Add (ORACLE_CONNECTION_LOST_ERR_EOFONCHANNEL);
		break;
	default:	//DBMS_UNKNOWN
		ASSERT(FALSE);
		TRACE ("SqlException::GetLostConnectionErrors call with unknown DBMS_TYPE");
	}
}

//-----------------------------------------------------------------------------
const BOOL SqlException::IsLostConnectionError (const int eDBMSType) const
{
	CWordArray arErrors;
	GetLostConnectionErrors(arErrors, eDBMSType);

	if (!arErrors.GetSize())
		return FALSE;
	
	for (int i=0; i <= arErrors.GetUpperBound(); i++)
		if (m_wNativeErrCode == arErrors.GetAt(i))
			return TRUE;
	
	return FALSE;
}

//-----------------------------------------------------------------------------
void SqlException::Empty()
{
	m_strError.Empty();
}
