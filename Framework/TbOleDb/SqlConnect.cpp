#include "stdafx.h"

#include <TbClientCore\ClientObjects.h>

#include <TbNameSolver\TbNamespaces.h>
#include <TbNameSolver\LoginContext.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>
#include <TbGeneric\ParametersSections.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbWebServicesWrappers\LockManagerInterface.h>

#include <TbGenlib\messages.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlibUI\SettingsTableManager.h>
#include <TbDatabaseManaged\MSqlConnection.h>

#include "sqlconnect.h"
#include "sqlmark.h"
#include "sqltable.h"
#include "sqlobject.h"
#include "sqllock.h"
#include "sqlcatalog.h"
#include "sqlaccessor.h"
#include "sqlRecoveryManager.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

static const TCHAR szParamSignature[]	= _T("pSignature");
static const TCHAR szParamModule[]		= _T("pModule");
static const int nNoPrimaryConnection	= -1;

#define DB_TB_REOPEN_TRANSACTION 1

//////////////////////////////////////////////////////////////////////////////
//						SqlCommandPool Implementation
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
//
IMPLEMENT_DYNAMIC(SqlCommandPool, CObArray)

//-----------------------------------------------------------------------------
void SqlCommandPool::AddCommand(SqlRowSet* pSqlCommand)
{
	//se non c'è lo aggiungo
	SqlRowSet* pCurrCommand;
	bool bFound = false;
	for (int i = 0; i < GetSize(); i++)
	{
		pCurrCommand = GetAt(i);
		if (pCurrCommand && pCurrCommand == pSqlCommand)
		{
			bFound = true;
			break;
		}
	}

	if (!bFound)
		Add(pSqlCommand);
}

//-----------------------------------------------------------------------------
void SqlCommandPool::RemoveCommand(SqlRowSet* pSqlCommand)
{

	SqlRowSet* pCurrCommand;
	for (int i = 0; i < GetSize(); i++)
	{
		pCurrCommand = GetAt(i);
		if (pCurrCommand && pCurrCommand == pSqlCommand)
		{
			RemoveAt(i);
			break;
		}
	}
}


//-----------------------------------------------------------------------------
void SqlCommandPool::CloseAllCommands()
{
	for (int nIdx = GetUpperBound(); nIdx >= 0; nIdx--)
	{
		SqlRowSet* pRowSet = GetAt(nIdx);
		if (pRowSet->IsOpen())
			pRowSet->Close();
		//delete pRowSet; //non viene cancellato il puntatore per evitare sequenze di distruzione obbligatorie(prima tabella e poi sessione) 
						  // non sempre sotto controllo del programmatore. Sarà il distruttore del SqlTable che si preoccuperà di distruggere
						  // gli oggetti di RowSet (BAUZI)
	}
	
	RemoveAll();
}


//-----------------------------------------------------------------------------
bool SqlCommandPool::ExistConnectedCommands()
{
	SqlRowSet* pRowSet;
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pRowSet = GetAt(nIdx);
		if (pRowSet && pRowSet->m_pRowSet && pRowSet->m_pRowSet->IsConnected())
			return true;
	}
	return false;
}

//-----------------------------------------------------------------------------
void SqlCommandPool::ReleaseAllCommands ()
{
	SqlRowSet* pRowSet;
	for (int nIdx = GetSize() - 1; nIdx >= 0 ; nIdx--)
	{
		pRowSet = GetAt(nIdx);
	
		if (pRowSet && pRowSet->m_pRowSet && pRowSet->m_pRowSet->IsConnected())
		{
			//START_DB_TIME(DB_CLOSE_ROWSET)
				pRowSet->m_pRowSet->Disconnect();
			//STOP_DB_TIME(DB_CLOSE_ROWSET)
		}
		//pRowSet->Disconnect();
		RemoveAt(nIdx);
	}
}	

void SqlSessionPool::ForceCloseSessions()
{
	SqlSession* pSession;
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pSession = GetAt(nIdx);
		pSession->ForceClose();
	}
}


//////////////////////////////////////////////////////////////////////////////
//					SqlSession Implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlSession, SqlObject)

//-----------------------------------------------------------------------------
SqlSession::SqlSession(SqlConnection* pConnection, CBaseContext* pContext /*=NULL*/)
	:
	SqlObject(pContext ? pContext : pConnection->m_pContext),
	m_pSqlConnection		(pConnection),
	m_pUpdatableSqlSession	(NULL),
	m_bForUpdate			(false),
	m_bTxnInProgress		(FALSE),
	m_bOwnSession			(TRUE),
	m_bStayOpen				(FALSE)
{
	m_pSession = new MSqlConnection();
	m_pSession->SetConnectionString(pConnection->m_strConnectionString);
}

//-----------------------------------------------------------------------------
SqlSession::SqlSession(MSqlConnection* pSession, SqlConnection* pConnection, CBaseContext* pContext/*=NULL*/)
	:
	SqlObject				(pContext ? pContext : pConnection->m_pContext),
	m_pSqlConnection		(pConnection),
	m_pUpdatableSqlSession	(NULL),
	m_bForUpdate			(false),
	m_bTxnInProgress		(FALSE),
	m_bOwnSession			(FALSE),
	m_bStayOpen				(FALSE)
{
	m_pSession = pSession;	

}
//-----------------------------------------------------------------------------
SqlSession::~SqlSession()
{
	Close();	
	m_pSqlConnection->RemoveSession(this);

	if (m_pUpdatableSqlSession)
	{
		delete m_pUpdatableSqlSession;
		m_pUpdatableSqlSession = NULL;
	}

	if (m_pSession && m_bOwnSession)
		delete m_pSession;
}

//-----------------------------------------------------------------------------
MSqlConnection*	SqlSession::GetMSqlConnection() const
{
	return m_pSession;
}

//-----------------------------------------------------------------------------
::DBMSType SqlSession::GetDBMSType() const
{
	return m_pSqlConnection->GetDBMSType();
}

//-----------------------------------------------------------------------------	
BOOL SqlSession::IsOpen() const
{
	return m_pSession && m_pSession->IsOpen();
}

//-----------------------------------------------------------------------------	
BOOL SqlSession::CanClose() const
{
	return !IsTxnInProgress();
}

//-----------------------------------------------------------------------------	
void SqlSession::ReleaseCommands()
{
	if (IsTxnInProgress())
		Commit();

	m_arCommandPool.ReleaseAllCommands();

	if (m_pUpdatableSqlSession)
		m_pUpdatableSqlSession->ReleaseCommands();

	m_pSession->Close ();
}

//-----------------------------------------------------------------------------
SqlSession* SqlSession::CreateUpdatableSqlSession()
{
	SqlSession* pUpdateableSqlSession = m_pSqlConnection->GetNewSqlSession(m_pContext);
	pUpdateableSqlSession->m_bForUpdate = true;
	return pUpdateableSqlSession;
}


//-----------------------------------------------------------------------------
SqlSession* SqlSession::GetUpdatableSqlSession()
{
	if (!m_pUpdatableSqlSession)
		m_pUpdatableSqlSession = CreateUpdatableSqlSession();

	return m_pUpdatableSqlSession;
}

//-----------------------------------------------------------------------------
void SqlSession::Open()
{
	TRY
	{

		if (!m_pSession->IsOpen())
		{
			//se non è la DefaultSqlSession allora guardo lo stato di open della DefaultSqlSession
			START_PROC_TIME(PROC_OPEN_CONNECTION)
			m_pSession->Open(m_pSqlConnection->AlwaysConnected());
			STOP_PROC_TIME(PROC_OPEN_CONNECTION)
			TRACE_SQL(_T("Open session"), this);
			if (m_pUpdatableSqlSession)
				m_pUpdatableSqlSession->Open();
		}
	}
	
		
	CATCH(MSqlException, e)
	{
		ThrowSqlException(cwsprintf(_TB("Unable to open the connection {0-%s} for the following error: \n\r {1-%s}"), m_pSession->GetDBName(), e->m_strError));

	}
	END_CATCH
}

//-----------------------------------------------------------------------------	
///<summary>
///Close sql connection
/// bForceClose = TRUE if you need to close the session with bStayOpen = TRUE
///</summary>
//[TBWebMethod(name = Connection_Close, thiscall_method=true)]
void SqlSession::Close()
{
	//non faccio niente 
	if (m_pSqlConnection->AlwaysConnected() || !m_pContext->IsOwnContext())
		return;
	
	if (IsTxnInProgress())
		Commit();

	m_arCommandPool.ReleaseAllCommands();

	if (m_pUpdatableSqlSession)
		m_pUpdatableSqlSession->Close();
	
	START_PROC_TIME(PROC_CLOSE_CONNECTION)
	m_pSession->Close();
	STOP_PROC_TIME(PROC_CLOSE_CONNECTION)
	TRACE_SQL(_T("Close session"), this);
}

//-----------------------------------------------------------------------------	
void SqlSession::ForceClose()
{
	if (!m_pContext->IsOwnContext()) // || m_arCommandPool.ExistConnectedCommands())
		return; 
	
	if (IsTxnInProgress())
		Commit();

	m_arCommandPool.ReleaseAllCommands();
			
	/*if (m_pUpdatableSqlSession)
		m_pUpdatableSqlSession->ForceClose();*/

	START_PROC_TIME(PROC_CLOSE_CONNECTION)
	m_pSession->Close();
	STOP_PROC_TIME(PROC_CLOSE_CONNECTION)
	TRACE_SQL(_T("ForceClose session"), this);
}

//-----------------------------------------------------------------------------	
void SqlSession::GetErrorString(HRESULT nResult, CString& m_strError)
{
	switch (nResult)
	{
		case DB_TB_REOPEN_TRANSACTION:			
			m_strError = _TB("Transaction running");
		default:
			m_strError.Empty();
	}
}

//-----------------------------------------------------------------------------
BOOL SqlSession::IsTxnInProgress()	const
{
	return	m_bTxnInProgress || (m_pUpdatableSqlSession && m_pUpdatableSqlSession->m_bTxnInProgress);
}

//-----------------------------------------------------------------------------
///<summary>
///Begin sql transaction
///</summary>
//[TBWebMethod(name = Connection_BeginTransaction, thiscall_method=true)]
void SqlSession::StartTransaction()
{
	if (m_pSqlConnection->m_bAutocommit || IsTxnInProgress())
		return;

	TRY
	{
		if (!m_pUpdatableSqlSession)
			m_pUpdatableSqlSession = CreateUpdatableSqlSession();

		if (!m_pUpdatableSqlSession->IsOpen())
			 m_pUpdatableSqlSession->Open();

		m_pUpdatableSqlSession->m_pSession->BeginTransaction();
		m_pUpdatableSqlSession->m_bTxnInProgress = TRUE;
		m_bTxnInProgress = TRUE;
	}
		CATCH(MSqlException, e)
	{
		m_pUpdatableSqlSession->AddMessage(cwsprintf(_TB("Unable to open the transaction for the following error: \n\r {0-%s}"), e->m_strError));
		//ThrowSqlException(cwsprintf(_TB("Unable to open the transaction for the following error: \n\r {0-%s}"), e->m_strError));
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
///<summary>
///Commit sql transaction
///</summary>
//[TBWebMethod(name = Connection_Commit, thiscall_method=true)]
void SqlSession::Commit()
{
	// lock manager restart error
	SqlLockMng* pLockMng =	m_pContext ? 
							m_pContext->GetLockMng(m_pSqlConnection) 
							: NULL;
	
	if (pLockMng && pLockMng->HasRestarted())
		ThrowSqlException(_TB("Lock Manager Web Services has been restarted. The current transaction will be aborted!\r\tIt is recommended to Logout e re-Login the application!"));	

	if (m_pSqlConnection->m_bAutocommit || !m_pUpdatableSqlSession || !IsTxnInProgress())
		return;

	TRY
	{
		m_pUpdatableSqlSession->m_pSession->Commit();
		m_pUpdatableSqlSession->m_bTxnInProgress = FALSE;
		m_pUpdatableSqlSession->Close();
	}
	
	CATCH(MSqlException, e)
	{
		m_pUpdatableSqlSession->AddMessage(cwsprintf(_TB("Unable to commit the transaction for the following error: \n\r {0-%s}"), e->m_strError));
		//ThrowSqlException(cwsprintf(_TB("Unable to commit the transaction for the following error: \n\r {0-%s}"), e->m_strError));
	}
	END_CATCH

	//InvalidateCommands(TRUE);
	m_bTxnInProgress = FALSE; // ho terminato la transazione
}

//-----------------------------------------------------------------------------
///<summary>
///Rollback sql transaction
///</summary>
//[TBWebMethod(name = Connection_Rollback, thiscall_method=true)]
void SqlSession::Abort()
{
	if (m_pSqlConnection->m_bAutocommit || !m_pUpdatableSqlSession || !IsTxnInProgress())
		return;

	TRY
	{
		m_pUpdatableSqlSession->m_pSession->Rollback();
		m_pUpdatableSqlSession->m_bTxnInProgress = FALSE;
		m_pUpdatableSqlSession->Close();
		m_bTxnInProgress = FALSE; // ho terminato la transazione
	}
		CATCH(MSqlException, e)
	{
		m_bTxnInProgress = FALSE;
		m_pUpdatableSqlSession->AddMessage(cwsprintf(_TB("Unable to abort the transaction for the following error: \n\r {0-%s}"), e->m_strError));

		//ThrowSqlException(cwsprintf(_TB("Unable to abort the transaction for the following error: \n\r {0-%s}"), e->m_strError));
	}

	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlSession::AddCommand(SqlRowSet* pSqlCommand)
{
	m_arCommandPool.AddCommand(pSqlCommand);
	Open(); //se la connessione è chiusa viene riaperta
}

//-----------------------------------------------------------------------------
void SqlSession::RemoveCommand(SqlRowSet* pSqlCommand)
{
	m_arCommandPool.RemoveCommand(pSqlCommand);

	//chiudo la sessione nel caso in cui non ci sia più nessun sqlrowset aperto e nel caso in cui la connessione non sia forzata
	if (!m_pSqlConnection->AlwaysConnected() && m_arCommandPool.GetSize() == 0 && !IsTxnInProgress())
		Close();
}





//////////////////////////////////////////////////////////////////////////////
//					SqlSessionPool Implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlSessionPool, CObArray)

//-----------------------------------------------------------------------------
void SqlSessionPool::RemoveSession(SqlSession* pSession)
{
	for (int i = 0; i < GetSize(); i++)
	{
		if (GetAt(i) == pSession)
		{
			RemoveAt(i);
			return;
		}
	}
}


//////////////////////////////////////////////////////////////////////////////
//					CDefaultSqlSessions Implementation
//////////////////////////////////////////////////////////////////////////////
class CDefaultSqlSessions : public CMap<SqlConnection*, SqlConnection*, SqlSession*, SqlSession*>
{
	BOOL m_bNeedDestruction;
public:
	CDefaultSqlSessions() 
	{
		m_bNeedDestruction = AfxGetTBThread() && AfxGetTBThread()->IsDocumentThread();
	};

	~CDefaultSqlSessions()
	{
		if (!m_bNeedDestruction)
			return;

		POSITION pos = GetStartPosition();
		SqlSession* pObject;
		SqlConnection* pKey;
		while (pos)
		{
			GetNextAssoc(pos, pKey, pObject);
			if (pObject && pKey == AfxGetDefaultSqlConnection())
			{
				pObject->Close();
				delete pObject;
			}
		}
	}
};

//////////////////////////////////////////////////////////////////////////////
//					SqlConnection Implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlConnection, SqlObject)

// costruttore di default Lo utilizzo solo per la Primary Connection
//-----------------------------------------------------------------------------
SqlConnection::SqlConnection(CBaseContext* pContext /*= NULL*/)
:
	SqlObject				(pContext),
	m_bCheckRegisterTable	(TRUE),
	m_nAlwaysConnectedRef	(0)
{
	Initialize();	
}

// costruttore utilizzato per tutte le altre connection
//-----------------------------------------------------------------------------
SqlConnection::SqlConnection
					(
						const CString& strConnectionString, 
						BOOL bCheckRegisterTable,
						CBaseContext* pContext /*= NULL*/,
						const CString& strAlias /*= _T("")*/
					)
:
	SqlObject				(pContext),
	m_bCheckRegisterTable	(bCheckRegisterTable),
	m_nAlwaysConnectedRef	(0),
	m_strAlias				(strAlias),
	m_pLockManagerInterface (NULL)
{
	SetConnectionString(strConnectionString);
	Initialize();
}

//-----------------------------------------------------------------------------
SqlConnection::~SqlConnection()
{	
	if (m_pLockManagerInterface)
		delete m_pLockManagerInterface;

	if (m_arSessionPool.GetSize() > 0)
	{
		ASSERT(FALSE);
		TRACE1("SqlConnection::~SqlConnection(): sessions of the database %s aren't closed", (LPCTSTR)m_strDBName);
		Close();
	}	
}
//-----------------------------------------------------------------------------
void SqlConnection::SetConnectionString(const CString& strConnectionString)
{
	m_strConnectionString = strConnectionString;
	if (m_strConnectionString.Find(_T("MultipleActiveResultSets=True"), 0) == -1)
		m_strConnectionString += _T(";MultipleActiveResultSets=True;"); 
}

//-----------------------------------------------------------------------------
void SqlConnection::Initialize()
{
	m_bValid				= FALSE;
	m_bOpen					= FALSE;
	m_bTablesPresent		= FALSE;
	m_bAutocommit			= FALSE;
	m_bUseUnicode			= TRUE;
	m_pCatalog				= NULL;
	m_nProviderID			= -1;
	m_bAlwaysConnected		= false;

	
	DataObj* pDataObj =  AfxGetSettingValue(snsTbOleDb, szPerformanceAnalizer, szAnalizeDocPerformance, DataBool(FALSE));
	if (pDataObj)
		m_bUsePerformanceMng = *((DataBool*) pDataObj) ;

	pDataObj = AfxGetSettingValue(snsTbOleDb, szDataCaching, szOptimizeHotLinkQuery, DataBool(FALSE), szTbDefaultSettingFileName);
	if (pDataObj)
		m_bOptimizedHKL = *((DataBool*)pDataObj);

}

//-----------------------------------------------------------------------------
SqlConnection* SqlConnection::Clone()
{
	SqlConnection* pConnection = new SqlConnection(m_strConnectionString, FALSE);

	pConnection->m_strDBName = this->m_strDBName;
	pConnection->m_strServerName = this->m_strServerName;
	pConnection->m_strDbmsName = this->m_strDbmsName;
	pConnection->m_strDbmsVersion = this->m_strDbmsVersion;
	pConnection->m_strAlias = this->m_strAlias;
	pConnection->m_bOpen = this->m_bOpen;
	pConnection->m_pCatalog = this->m_pCatalog;
	pConnection->m_bValid = this->m_bValid;
	pConnection->m_bTablesPresent = this->m_bTablesPresent;	
	pConnection->GetDefaultSqlSession(); //faccio creare subito la SqlSession di Default così è sicuramente nella posizione 0
	return pConnection;
}

//-----------------------------------------------------------------------------
CLockManagerInterface* SqlConnection::GetLockManagerInterface()
{
	DataObj* pDataObj = AfxGetSettingValue
	(
		snsTbOleDb,
		szLockManager,
		szUseNewSqlLockManager,
		DataBool(FALSE),
		szTbDefaultSettingFileName
	);
	if (!pDataObj || *((DataBool*)pDataObj) == FALSE)
		return  (CLockManagerInterface*)AfxGetLoginContext()->GetLockManager();

	if (!m_pLockManagerInterface)
	{
		m_pLockManagerInterface = new CLockManagerInterface
		(
			AfxGetPathFinder()->GetLockServiceName(),
			_T("http://microarea.it/LockManager/"),
			AfxGetLoginManager()->GetServer(),
			AfxGetCommonClientObjects()->GetServerConnectionInfo()->m_nWebServicesPort
		);

		m_pLockManagerInterface->Init(this->GetNewSqlSession()->GetMSqlConnection());
	}
	return m_pLockManagerInterface;
 }

//-----------------------------------------------------------------------------
::DBMSType SqlConnection::GetDBMSType() const
{
	return DBMSType::DBMS_SQLSERVER;
}

// restituisce la sessione di default di tipo SqlSession
//-----------------------------------------------------------------------------
SqlSession* SqlConnection::GetDefaultSqlSession()	
{
	SqlSession* pSession = NULL;
	if (m_arSessionPool.GetSize() > 0)
		pSession = m_arSessionPool[0];	
	else
	{
		pSession = GetNewSqlSession();
		m_arSessionPool[0] = pSession;
	}
	return pSession;

}

// crea una nuova sessione e la inserisce in m_arSessionPool e ritorna un SqlSession
// posso fare nascere la sessione in un contesto messaggisto differente rispetto
// a quello della connessione
//-----------------------------------------------------------------------------
SqlSession* SqlConnection::GetNewSqlSession(CTBContext* pTBContext)
{
 	TRY
	{
		return GetNewSqlSession(pTBContext ? pTBContext->m_pBaseContext : NULL);
	}
	CATCH(SqlException, e)	
	{
		THROW_LAST();
	}	
	END_CATCH
}

//-----------------------------------------------------------------------------
SqlSession* SqlConnection::GetNewSqlSession(CBaseContext* pContext /*=NULL*/)
{
    TB_LOCK_FOR_WRITE();
	SqlSession* pSession = new SqlSession(this, pContext);

	TRY
	{
		pSession->GetMSqlConnection()->SetKeepOpen(m_bAlwaysConnected);
		m_arSessionPool.Add(pSession);
	}
	CATCH(SqlException, e)	
	{
		if (pSession) 
			delete pSession;
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		THROW_LAST();
	}	
	END_CATCH

	return pSession;	
}

//-----------------------------------------------------------------------------
void SqlConnection::RemoveSession(SqlSession* pSession)
{
	TB_LOCK_FOR_WRITE();
	m_arSessionPool.RemoveSession(pSession);
}

//-----------------------------------------------------------------------------
SqlCatalogConstPtr SqlConnection::GetCatalog()
{
	return SqlCatalogConstPtr(m_pCatalog, FALSE); //READ ONLY LOCK
}



// chiudere una connessione: questo vuole dire che non ci devono essere appesi
// command, transazioni, ecc.
//-----------------------------------------------------------------------------
BOOL SqlConnection::CanClose()
{ 	
	TB_LOCK_FOR_READ();
	if (!m_bValid) return TRUE;

	for (int nIdx = 0; nIdx < m_arSessionPool.GetSize(); nIdx++) 	
		if (!m_arSessionPool.GetAt(nIdx)->CanClose())
			return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlConnection::Close()
{ 
	TB_LOCK_FOR_WRITE();
	
	//default sqlsession
	//GET_THREAD_VARIABLE(CDefaultSqlSessions, m_DefaultSqlSessions);
	//m_DefaultSqlSessions[this] = NULL;
	SqlSession* pSession;	
	for (int nIdx = m_arSessionPool.GetUpperBound(); nIdx >= 0; nIdx--) 	
	{
		pSession = m_arSessionPool.GetAt(nIdx);
		if (pSession && !pSession->m_bForUpdate) //nel caso di pSession->m_bForUpdate = true sarà la sessione che l'ha istanziata che si preoccuperà di distruggerla
		{
			pSession->Close();
			delete pSession;			
		}
	}
	m_arSessionPool.RemoveAll();

}

//-----------------------------------------------------------------------------
void SqlConnection::SetAlwaysConnected(bool bSet)
{ 
	if (bSet)
	{
		if (m_nAlwaysConnectedRef == 0)
		{
			m_bAlwaysConnected = TRUE;
			for (int nIdx = 0; nIdx < m_arSessionPool.GetSize(); nIdx++)
				m_arSessionPool.GetAt(nIdx)->GetMSqlConnection()->SetKeepOpen(true);
		}
		m_nAlwaysConnectedRef++;
	}
	else
	{
		ASSERT(m_nAlwaysConnectedRef > 0);
		m_nAlwaysConnectedRef = (m_nAlwaysConnectedRef > 0) ? m_nAlwaysConnectedRef - 1 : 0;
		if (m_nAlwaysConnectedRef == 0)
		{
			m_bAlwaysConnected = FALSE;		
			m_arSessionPool.ForceCloseSessions();
			for (int nIdx = 0; nIdx < m_arSessionPool.GetSize(); nIdx++)
				m_arSessionPool.GetAt(nIdx)->GetMSqlConnection()->SetKeepOpen(false);
		}
	}
}



// stringa di connessione
// Provider SqlServer
// con DB-Autentication	L"Provider=SQLOLEDB;Data Source=USR-BAUZONEANN;Initial Catalog='TestAppOleDb';User ID='sa';Password='';Connect Timeout=30;";
// con Win-Autentication = L"Provider=SQLOLEDB;Data Source=USR-BAUZONEANN;Initial Catalog='TestAppOleDb';Integrated Security='SSPI';Connect Timeout=30;";
// Provider Oracle: 
// con DB-Autentication	= L"Provider=OraOLEDB.Oracle;Data Source=ORCL;User ID='TestAppOleDb';Password='pwd';";
// con Win-Autentication = L"Provider=OraOLEDB.Oracle;Data Source=ORCL;OSAuthent=1;";
// Provider MySql: provider ODBC con driver MySQL ODBC 5.1
// con DB-Authentication =  L"Provider=MSDASQL;Driver=MySQL ODBC 5.1 Driver;Database=MySql_Prova;Server=localhost;Uid=root;Pwd=anna;Port=3306;";
	
//per la stringa di connessione al database di prova ORACLE
// da inserire temporaneamente nel file LoginManagerInterface
//	m_nProviderId								= 2;
//	m_strProviderCompanyConnectionString		= _T("Provider=OraOLEDB.Oracle;Data Source=ORA920;User ID='ANNA';Password='a';");



//-----------------------------------------------------------------------------
BOOL SqlConnection::MakeConnection()
{
	if (m_bOpen)
	{
		TRACE("SqlConnection::MakeConnection: the connection is already opened\n");
		AddMessage(cwsprintf(_TB("The connection {0-%s} it is already open"), m_strConnectionString));
		return FALSE;
	}

	m_bValid = FALSE;

	TRY
	{
		
		SqlSession* pDefaultSqlSession = GetDefaultSqlSession();

		SetAlwaysConnected(true);
		//apro la connessione
		pDefaultSqlSession->Open(); //true = sono io che decido quando chiudere la connessione e non mi affido agli automatismi
		m_strDBName = pDefaultSqlSession->m_pSession->GetDBName();
		m_strServerName = pDefaultSqlSession->m_pSession->GetServerName();
		m_strUserName = pDefaultSqlSession->m_pSession->GetDBUserName();
		m_strDbmsName = pDefaultSqlSession->m_pSession->GetDbmsName();
		m_strDbmsVersion = pDefaultSqlSession->m_pSession->DbmsVersion();

		m_bOpen = TRUE;	
		
		CTickTimeFormatter aTickFormatter;		
		DWORD dTotStartTick = GetTickCount();
		DWORD dElapsedTick = 0;
		// carico il catalog
		DWORD dStartTick = GetTickCount();
		m_pCatalog = AfxGetSqlCatalog(this);
		DWORD dStopTick = GetTickCount();
		dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
		TRACE1("AfxGetSqlCatalog Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));
		if (m_pCatalog->DatabaseEmpty())
		{
			AddMessage(_TB("The database has not been created.\r\nPlease contact the program administrator."));
			return FALSE;
		}

		m_bValid = TRUE;
		//se é una connessione che utilizza i meccanismi di TaskBuilder
		// allora devo gestire anche i Lock
		// e registrare le tabelle
		if (m_bCheckRegisterTable)
		{
			dStartTick = GetTickCount();
			m_bTablesPresent = RegisterTables();
			dStopTick = GetTickCount();
			dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
			TRACE1("RegisterTables Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));
			if (!m_bTablesPresent)
			{
				AddMessage(_TB("Some tables required by the application are missing.\r\nPlease contact the program administrator."));
				m_bValid = FALSE;
			}
		}
		
		//chiudo la connessione
		SetAlwaysConnected(false);
		//pDefaultSqlSession->ForceClose();

		dStopTick = GetTickCount();
		dElapsedTick = (dStopTick >= dTotStartTick) ? dStopTick - dTotStartTick : 0;
		TRACE1("MakeConnection Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));

	}
	CATCH(SqlException, e)
	{
		AddMessage(e->m_strError);
		m_bValid = FALSE;
		return FALSE;
	}
	END_CATCH
		
	
	return m_bValid;		
}

// viene chiamato in fase di connessione al database per i soli moduli giá caricati
// in memoria
//-----------------------------------------------------------------------------
BOOL SqlConnection::RegisterTables()
{
	BOOL bOk = TRUE;
	AddOnApplication* pAddOnApp = NULL;
	for (int nApp = 0; nApp <= AfxGetAddOnAppsTable()->GetUpperBound(); nApp++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(nApp);
		ASSERT(pAddOnApp);
		if (pAddOnApp->m_pAddOnModules)
		{
			for (int nMod = 0; nMod <= pAddOnApp->m_pAddOnModules->GetUpperBound(); nMod++)
			{
				AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(nMod);
				ASSERT(pAddOnMod);
				if (pAddOnMod->m_pAddOnLibs)
				{
					for (int nLib = 0; nLib <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); nLib++)
					{
						AddOnLibrary* pAddOnLib = pAddOnMod->m_pAddOnLibs->GetAt(nLib);
						bOk = RegisterAddOnLibrayTables(pAddOnLib, pAddOnApp);
					}
				}
			}
		}
	}
	//-----
	if (m_pCatalog)
	{
		ASSERT_VALID(m_pCatalog);
		m_pCatalog->SortTableInfoColumns();
	}
	return bOk;
}
//-----------------------------------------------------------------------------
BOOL SqlConnection::RegisterCatalogEntry
						(
							LPCTSTR				pszSignature, 
							const CTBNamespace&	aNamespace,
							CRuntimeClass*		pSqlRecordClass, 
							int					nType 
						)
{
	return m_pCatalog->RegisterCatalogEntry(this, pszSignature, aNamespace, pSqlRecordClass, nType);
}
// questo metodo viene chiamato anche per i moduli caricati successivamente
//-----------------------------------------------------------------------------
BOOL SqlConnection::RegisterAddOnLibrayTables(AddOnLibrary* pAddOnLib, AddOnApplication* pAddOnApp /*=NULL*/)
{
	AddOnApplication* pApp = (pAddOnApp) ? pAddOnApp : AfxGetAddOnApp(pAddOnLib->GetApplicationName());
	if (!pApp || !pAddOnLib->m_pAddOn)
		return TRUE;

	return pAddOnLib->m_pAddOn->AOI_RegisterTables(this, pApp->GetSignature(), &pAddOnLib->m_Namespace);
}


//-----------------------------------------------------------------------------
SqlTableInfo* SqlConnection::GetTableInfo(const CString& strTableName)
{
	return  m_pCatalog ? m_pCatalog->GetTableInfo(strTableName, this) : NULL;
}

//-----------------------------------------------------------------------------
const CRTAddOnNewFieldsArray* SqlConnection::GetSqlNewFieldRT(const CString& strTableName)
{
	SqlTableInfo* pInfo = GetTableInfo(strTableName);

	return pInfo ? pInfo->GetCRTAddOnNewFields() : NULL;
}

//-----------------------------------------------------------------------------
const SqlCatalogEntry* SqlConnection::GetCatalogEntry(const CString& strTableName)
{
	return m_pCatalog ? m_pCatalog->GetEntry(strTableName) : NULL;
}

//-----------------------------------------------------------------------------
CRuntimeClass* SqlConnection::GetSqlRecordClass(const CString& strTableName)
{
	return m_pCatalog ? m_pCatalog->GetSqlRecordClass(strTableName) : NULL;
}

//-----------------------------------------------------------------------------
CRuntimeClass* SqlConnection::GetSqlRecordClass(const CTBNamespace& ns)
{
	return m_pCatalog ? m_pCatalog->GetSqlRecordClass(ns) : NULL;
}

//-----------------------------------------------------------------------------
BOOL SqlConnection::ExistTable(const CString& strTableName)
{
	if (!m_bValid || !m_pCatalog)
		return FALSE;

	if (m_pCatalog->ExistTable(strTableName))
		return TRUE;

	return FALSE;
}

//-----------------------------------------------------------------------------
void SqlConnection::UpdateStatistics()
{
	CString strSP_UpdStat= cwsprintf (_T("{call sp_updatestats }"));
	
	ExecuteSQL(strSP_UpdStat);
//	AfxMessageBox(_TB("Successful database performance improvement."));
}

//-----------------------------------------------------------------------------
SqlConnection::ExecuteResult SqlConnection::ExtendedExecuteSQL(LPCTSTR lpszSQL, SqlSession* pSession, BOOL bLock, const CString& strLockKey,  BOOL bUseMessageBox /*= TRUE*/, LockRetriesMng* pRetriesMng /*= NULL*/)
{
	ASSERT_VALID(this);
	ASSERT(AfxIsValidString(lpszSQL));
	
	//OLEDB
	SqlTable table(pSession ?  pSession : GetDefaultSqlSession());
	TRY
	{	
		if (bLock && !table.LockTable(strLockKey, bUseMessageBox, pRetriesMng))
		{
			if (!bUseMessageBox)
			{
				table.m_pSqlSession->AddMessage(table.GetLockMessage());
				return LOCKED;
			}
		}

		table.Open(FALSE, E_NO_CURSOR);
		table.ExecuteQuery(lpszSQL);

		if (bLock)
			table.UnlockTable(strLockKey);
		
		table.Close();

		return EXECUTE_SUCCESS;
	}
	
	CATCH(SqlException, e)	
	{
		if (table.IsOpen())
			table.Close();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		THROW_LAST();
	}
	END_CATCH

}

//-----------------------------------------------------------------------------
void SqlConnection::ExecuteSQL(LPCTSTR lpszSQL, SqlSession* pSession /*= NULL*/)
{
	ASSERT_VALID(this);
	ASSERT(AfxIsValidString(lpszSQL));
	
	//OLEDB
	SqlTable table(pSession ?  pSession : GetDefaultSqlSession());
	TRY
	{	
		table.Open(FALSE, E_NO_CURSOR);
		table.ExecuteQuery(lpszSQL);
		table.Close();

	}
	
	CATCH(SqlException, e)	
	{
		if (table.IsOpen())
			table.Close();
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		THROW_LAST();
	}
	END_CATCH

}

//@@OLEDB
// serve per poter eseguire uno script SQL contenuto in un file memorizzato 
// nella struttura standard
//	Script
//		All
//		SqlServer
//		Oracle
// Il file viene prima cercato nella directory specifica relativa al provider
// che si sta utilizzando. Se non trovato si cerca nella directory All
//-----------------------------------------------------------------------------
BOOL SqlConnection::RunScript(LPCTSTR lpszFileName)
{
	ASSERT(FALSE);
	return TRUE;
}


//-----------------------------------------------------------------------------
CString SqlConnection::NativeConvert(const DataObj* pDataObj) 
{
	return  GetDefaultSqlSession()->m_pSession->NativeConvert(pDataObj, m_bUseUnicode);
}

//-----------------------------------------------------------------------------
void SqlConnection::LoadTables(::CMapStringToOb* pTables)
{
	TRY
	{ 
		GetDefaultSqlSession()->m_pSession->LoadSchemaInfo(_T("Tables"), pTables);
	}
	CATCH(MSqlException, e)
	{
		ThrowSqlException(e);
	}
	END_CATCH
}


//-----------------------------------------------------------------------------
void SqlConnection::LoadProcedures(::CMapStringToOb* pTables)
{
	TRY
	{ 
		GetDefaultSqlSession()->m_pSession->LoadSchemaInfo(_T("Procedures"), pTables);
	}
	CATCH(MSqlException, e)
	{
		ThrowSqlException(e);
	}
	END_CATCH
}


//-----------------------------------------------------------------------------
void SqlConnection::LoadProcedureParametersInfo(const CString& strProcName, ::Array* pProcedureParams)
{
	TRY
	{ 
		GetDefaultSqlSession()->m_pSession->LoadProcedureParametersInfo(strProcName, pProcedureParams);
	}
	CATCH(MSqlException, e)
	{
		ThrowSqlException(e);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlConnection::LoadColumnsInfo(const CString&  strTableName, ::Array* arPhisycalColumns)
{
	TRY
	{ 
		GetDefaultSqlSession()->m_pSession->LoadColumnsInfo(strTableName, arPhisycalColumns);
	}
	CATCH(MSqlException, e)
	{
		ThrowSqlException(e);
	}
	END_CATCH
}


//-----------------------------------------------------------------------------
void SqlConnection::LoadForeignKeys(const CString& sFromTableName, const CString& sToTableName, BOOL bLoadAllToTables, CStringArray* pFKReader)
{
	TRY
	{
		GetDefaultSqlSession()->m_pSession->LoadForeignKeys(sFromTableName, sToTableName, bLoadAllToTables, pFKReader);
	}
	CATCH(MSqlException, e)
	{
		ThrowSqlException(e);
	}
	END_CATCH
}



// dato il nome fisico della tabella verifica se la tabella è vuota o meno
//-----------------------------------------------------------------------------
BOOL SqlConnection::IsEmptyTable(const CString& strTableName)
{
	return RecordCountNumber(strTableName) <= 0;
}

//-----------------------------------------------------------------------------
long SqlConnection::RecordCountNumber(const CString& strTableName)
{
	if (strTableName.IsEmpty()) return -1;

	DataLng aCount;
	SqlTable aTable(GetDefaultSqlSession());

	aTable.m_strSQL = cwsprintf (_T("SELECT COUNT(*) FROM %s"), (LPCTSTR)strTableName);
	aTable.Select(_T("COUNT(*)"), &aCount);
	TRY
	{
		aTable.Open();
		aTable.Query();
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		GetDefaultSqlSession()->ShowMessage(e->m_strError);
		return 0;
	}
	END_CATCH
	return (long)aCount;
}

//-----------------------------------------------------------------------------
const CString& SqlConnection::GetDatabaseCollation ()
{
	return m_pCatalog->GetDatabaseCollation();
}

//-----------------------------------------------------------------------------
void SqlConnection::RefreshTraces () 
{
	if (m_pCatalog)
		m_pCatalog->RefreshTraces(); 
}

//-----------------------------------------------------------------------------
BOOL SqlConnection::IsAlive () const 	
{
	ThrowSqlException(_T(" SqlConnection::IsAlive method to implements"));
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//					SqlConnectionPool Implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlConnectionPool, Array)

//-----------------------------------------------------------------------------
SqlConnectionPool::SqlConnectionPool ()
	:
	m_nPrimaryConnection (nNoPrimaryConnection)
{
}

//-----------------------------------------------------------------------------
BOOL SqlConnectionPool::CanCloseAll() const
{
	for (int i = 0; i < GetSize(); i++)
		if (!GetAt(i)->CanClose()) 
			return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlConnectionPool::CloseAll()
{
	for (int nIdx = GetUpperBound(); nIdx >= 0; nIdx--)
	{
		SqlConnection* pConnect = GetAt(nIdx);
		if (pConnect->IsOpen())
			pConnect->Close();
	}
	RemoveAll();

	m_nPrimaryConnection = nNoPrimaryConnection;
}

//-----------------------------------------------------------------------------
BOOL SqlConnectionPool::RemovePrimaryConnection()
{
	if (GetSize() >= 1 && m_nPrimaryConnection != nNoPrimaryConnection)
	{
		if (GetAt(m_nPrimaryConnection) && GetAt(m_nPrimaryConnection)->CanClose())
		{
			GetAt(m_nPrimaryConnection)->Close();
			RemoveAt(m_nPrimaryConnection);
			m_nPrimaryConnection = nNoPrimaryConnection;
			return TRUE;
		}
	}

	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlConnectionPool::RemoveConnection(SqlConnection* sqlConnection)
{
	for (int i = 0; i < GetSize(); i++)
	{
		if (GetAt(i) == sqlConnection && sqlConnection->CanClose())
		{
			sqlConnection->Close();
			if (i == m_nPrimaryConnection)
				m_nPrimaryConnection = nNoPrimaryConnection;
			RemoveAt(i);
			sqlConnection = NULL;
			return TRUE;
		}
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void SqlConnectionPool::SetPrimaryConnection(SqlConnection* pConnection)
{
	if (!pConnection)
		return;

	pConnection->m_strAlias = _T("PRIMARY");
	m_nPrimaryConnection = Add(pConnection);
}

//-----------------------------------------------------------------------------
SqlConnection* SqlConnectionPool::GetPrimaryConnection() 
{
	if (GetSize() >= 1 && m_nPrimaryConnection != nNoPrimaryConnection)
		return GetAt(m_nPrimaryConnection);

	return NULL;
}

//-----------------------------------------------------------------------------
SqlConnection* SqlConnectionPool::GetSqlConnectionByAlias(const CString& strAlias)
{
	SqlConnection* pConnection = NULL;
	for (int i = 0; i < GetSize(); i++)
	{
		pConnection = GetAt(i);
		if (pConnection && pConnection->GetAlias().CompareNoCase(strAlias) == 0)
			return pConnection;
	}

	return NULL;
}

