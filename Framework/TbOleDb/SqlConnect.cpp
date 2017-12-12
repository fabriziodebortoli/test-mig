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

#include "sqlconnect.h"
#include "sqlmark.h"
#include "sqltable.h"
#include "sqlobject.h"
#include "sqllock.h"
#include "sqlcatalog.h"
#include "sqlaccessor.h"
#include "sqlRecoveryManager.h"
#include "ATLSession.h"

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
IMPLEMENT_DYNAMIC(SqlCommandPool, CObArray)

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
void SqlCommandPool::ReleaseAllCommands ()
{
	SqlRowSet* pRowSet;
	SqlRowSet* pUpdRowSet;
	CCommand<CManualAccessor>* pCommand;
	SqlTable* pTable;
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		// readonly rowsets
		pRowSet = GetAt(nIdx);
		if (pRowSet->IsOpen())
		{
			pCommand = (CCommand<CManualAccessor>*) pRowSet->m_pRowSet;
			pCommand->Close();
			pCommand->ReleaseCommand();
			pRowSet->Invalidate();
		}

		// updatable rowsets are not contained into commands array
		if (pRowSet->IsKindOf(RUNTIME_CLASS(SqlTable)))
		{
			pTable = (SqlTable*) pRowSet;
			pUpdRowSet = pTable->m_pUpdateRowSet;
			if (pUpdRowSet && pUpdRowSet->IsOpen())
			{
				pCommand = (CCommand<CManualAccessor>*) pUpdRowSet->m_pRowSet;
				pCommand->Close();
				pCommand->ReleaseCommand();
				pUpdRowSet->Invalidate();
			}
		}
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
	SqlObject		(pContext ? pContext : pConnection->m_pContext),
	m_pSqlConnection(pConnection),
	m_bTxnInProgress(FALSE),
	m_bOwnSession	(TRUE)
{
	m_pSession = new ATLSession;	
}

//-----------------------------------------------------------------------------
SqlSession::SqlSession(ATLSession* pSession, SqlConnection* pConnection, CBaseContext* pContext/*=NULL*/)
	:
	SqlObject		(pContext ? pContext : pConnection->m_pContext),
	m_pSqlConnection(pConnection),
	m_bTxnInProgress(FALSE),
	m_bOwnSession	(FALSE)
{
	m_pSession = pSession;
}
//-----------------------------------------------------------------------------
SqlSession::~SqlSession()
{
	if (m_pSession && m_bOwnSession)
		delete m_pSession;
}

//-----------------------------------------------------------------------------	
BOOL SqlSession::CanClose() const
{
	return !m_bTxnInProgress;
}

//-----------------------------------------------------------------------------	
void SqlSession::ReleaseCommands()
{
	if (m_bTxnInProgress)
		Commit();

	m_arCommandPool.ReleaseAllCommands();
	m_pSession->Close ();
}

//-----------------------------------------------------------------------------	
///<summary>
///Close sql connection
///</summary>
//[TBWebMethod(name = Connection_Close, thiscall_method=true)]
void SqlSession::Close()
{
	if (m_bTxnInProgress)
		Commit();

	m_arCommandPool.CloseAllCommands();
	m_pSqlConnection->RemoveSession(this);
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
DBMSType SqlSession::GetDBMSType() const
{
	return m_pSqlConnection->GetDBMSType();
}
//-----------------------------------------------------------------------------
void SqlSession::Open()
{
	SetPropGUID(DBPROPSET_SESSION);

	if (m_pSqlConnection->GetDBMSType() == DBMS_ORACLE)
		AddProperty(DBPROP_SESS_AUTOCOMMITISOLEVELS, DBPROPVAL_TI_READCOMMITTED); 
	else
		AddProperty(DBPROP_SESS_AUTOCOMMITISOLEVELS, DBPROPVAL_TI_READUNCOMMITTED); 
		
	if (!Check(m_pSession->Open(*(const CDataSource*)m_pSqlConnection->GetDataSource(), (CDBPropSet*)m_pDBPropSet)))
	{
		if (m_pSession->m_spOpenRowset)
			ThrowSqlException(((CSession*)m_pSession)->m_spOpenRowset, IID_IOpenRowset, m_hResult);	
		else
			ThrowSqlException(((const CDataSource*)m_pSqlConnection->GetDataSource())->m_spInit, IID_IDBCreateSession, m_hResult);	
	}
}	

//-----------------------------------------------------------------------------
///<summary>
///Begin sql transaction
///</summary>
//[TBWebMethod(name = Connection_BeginTransaction, thiscall_method=true)]
void SqlSession::StartTransaction()
{
	if (m_pSqlConnection->m_bAutocommit || m_bTxnInProgress)
		return;

	if (m_pSqlConnection->GetDBMSType() == DBMS_ORACLE)
	{
		if (!Check(m_pSession->StartTransaction(ISOLATIONLEVEL_READCOMMITTED)))
			ThrowSqlException(m_pSession->m_spOpenRowset, IID_ITransactionLocal, m_hResult);	
	}
	else
		if (!Check(m_pSession->StartTransaction(ISOLATIONLEVEL_READUNCOMMITTED)))
			ThrowSqlException(m_pSession->m_spOpenRowset, IID_ITransactionLocal, m_hResult);	

	m_bTxnInProgress = TRUE;
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
							m_pContext->GetLockMng(m_pSqlConnection->GetDatabaseName()) 
							: NULL;
	
	if (pLockMng && pLockMng->HasRestarted())
		ThrowSqlException(_TB("Lock Manager Web Services has been restarted. The current transaction will be aborted!\r\tIt is recommended to Logout e re-Login the application!"));	

	if (m_pSqlConnection->m_bAutocommit || !m_bTxnInProgress)
		return;

	if (!Check(m_pSession->Commit()))
	{
		m_bTxnInProgress = FALSE;
		ThrowSqlException(m_pSession->m_spOpenRowset, IID_ITransactionLocal, m_hResult);	
	}

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
	if (m_pSqlConnection->m_bAutocommit || !m_bTxnInProgress)
		return;

	if (!Check(m_pSession->Abort()))
	{
		m_bTxnInProgress = FALSE;
		ThrowSqlException(m_pSession->m_spOpenRowset, IID_ITransactionLocal, m_hResult);	
	}

	//InvalidateCommands(FALSE);
	m_bTxnInProgress = FALSE; // ho terminato la transazione
}

//-----------------------------------------------------------------------------
void SqlSession::AddCommand(SqlRowSet* pSqlRowSet)
{
	m_arCommandPool.Add(pSqlRowSet); 
}

//-----------------------------------------------------------------------------
void SqlSession::RemoveCommand(SqlRowSet* pSqlRowSet)
{
	SqlRowSet* pCurrRowSet;
	for (int i = 0; i < m_arCommandPool.GetSize(); i++)
	{
		pCurrRowSet = m_arCommandPool.GetAt(i); 
		if (pCurrRowSet && pCurrRowSet == pSqlRowSet)
		{
			m_arCommandPool.RemoveAt(i);
			return;
		}
	}
}

//-----------------------------------------------------------------------------
void SqlSession:: SetDataCachingContext(CDataCachingContext* pDataCachingContext)
{
	SqlRowSet* pCurrRowSet;
	for (int i = 0; i < m_arCommandPool.GetSize(); i++)
	{
		pCurrRowSet = m_arCommandPool.GetAt(i); 
		if (pCurrRowSet && pCurrRowSet->IsKindOf(RUNTIME_CLASS(SqlTable)))
			((SqlTable*)pCurrRowSet)->SetDataCachingContext(pDataCachingContext);
	}
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
//					ATLDataSource Implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class ATLDataSource : public CDataSource
{};

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

//only document thread has to destroy default session, because login
//thread is cleaned when destroying OleDbManager
DECLARE_THREAD_VARIABLE
	(	
	CDefaultSqlSessions,
	m_DefaultSqlSessions
	);
// costruttore di default Lo utilizzo solo per la Primary Connection
//-----------------------------------------------------------------------------
SqlConnection::SqlConnection(CBaseContext* pContext /*= NULL*/, CString dbOwner /*= _T("")*/)
:
	SqlObject				(pContext),
	m_bCheckRegisterTable	(TRUE),
	m_bCheckDBMark			(FALSE),
	m_strDBOwner			(dbOwner)
{
	Initialize();	
}

// costruttore utilizzato per tutte le altre connection
//-----------------------------------------------------------------------------
SqlConnection::SqlConnection
					(
						BOOL bCheckRegisterTable, 
						BOOL bUseLockMng, 
						BOOL bCheckDBMark, 
						CBaseContext* pContext /*= NULL*/, 
						CString dbOwner /*= _T("")*/
					)
:
	SqlObject				(pContext),
	m_bCheckRegisterTable	(bCheckRegisterTable),
	m_bCheckDBMark			(bCheckDBMark),
	m_strDBOwner			(dbOwner)
{
	Initialize();
}

//-----------------------------------------------------------------------------
SqlConnection::~SqlConnection()
{
	if (m_pCheckTable)
		delete m_pCheckTable;
	
	if (m_pSqlMarkRec)
		delete m_pSqlMarkRec;

	if (m_pDataSource)
	{
		m_pDataSource->Close();
		delete m_pDataSource;
	}

	if (m_arSessionPool.GetSize() > 0)
	{
		ASSERT(FALSE);
		TRACE1("SqlConnection::~SqlConnection(): sessions of the database %s aren't closed", (LPCTSTR)m_strDBName);
		Close();
	}
}

//-----------------------------------------------------------------------------
void SqlConnection::Initialize()
{
	m_bValid				= FALSE;
	m_bOpen					= FALSE;
	m_bTablesPresent		= FALSE;
	m_bAutocommit			= FALSE;
	m_bUseUnicode			= TRUE;
	m_bOptimizedHKL			= TRUE;
	m_pSqlMarkRec  			= NULL;
	m_pCheckTable			= NULL;	
	m_pProviderInfo			= NULL;
	m_pCatalog				= NULL;
	m_nProviderID			= -1;
	m_bRemoveDeletedRows    = TRUE;

	DataObj* pDataObj = AfxGetSettingValue(snsTbOleDb, szConnectionSection, szHKLTableReaderCursor, DataInt(m_eHKLTRCursor));
	if (pDataObj)
		m_eHKLTRCursor = (CursorType)(int) *((DataInt*) pDataObj) ;

	pDataObj =  AfxGetSettingValue(snsTbOleDb, szConnectionSection, szROForwardTableCursor, DataInt(m_eROForwardCursor));
	if (pDataObj)
		m_eROForwardCursor = (CursorType)(int) *((DataInt*) pDataObj) ;

	pDataObj =  AfxGetSettingValue(snsTbOleDb, szPerformanceAnalizer, szAnalizeDocPerformance, DataBool(m_bUsePerformanceMng));
	if (pDataObj)
		m_bUsePerformanceMng = *((DataBool*) pDataObj) ;
	
	pDataObj = AfxGetSettingValue(snsTbOleDb, szDataCaching, szOptimizeHotLinkQuery, DataBool(m_bOptimizedHKL), szTbDefaultSettingFileName);
	if (pDataObj)
		m_bOptimizedHKL	=  *((DataBool*) pDataObj) ;
	
	pDataObj = AfxGetSettingValue(snsTbOleDb, szConnectionSection, _T("RemoveDeletedRows"), DataBool(m_bRemoveDeletedRows));
	if (pDataObj)
		m_bRemoveDeletedRows = *((DataBool*) pDataObj) ;

	
	m_pDataSource = new ATLDataSource;
}




// restituisce la sessione di default
//-----------------------------------------------------------------------------
ATLSession* SqlConnection::GetDefaultSession()	
{
	return GetDefaultSqlSession()->m_pSession;
}



// restituisce la sessione di default di tipo SqlSession
//-----------------------------------------------------------------------------
SqlSession* SqlConnection::GetDefaultSqlSession()	
{
	GET_THREAD_VARIABLE(CDefaultSqlSessions, m_DefaultSqlSessions);
	SqlSession *pSession = m_DefaultSqlSessions[this];
	if (!pSession)
	{
		pSession = GetNewSqlSession();
		m_DefaultSqlSessions[this] = pSession;
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
		pSession->Open();
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

// crea una nuova sessione e la inserisce in m_arSessionPool e ritorna un ATLSession
//-----------------------------------------------------------------------------
ATLSession* SqlConnection::GetNewSession(CBaseContext* pContext /*=NULL*/)
{
	SqlSession* pSession = NULL;
		
	TRY
	{
		pSession = GetNewSqlSession(pContext);
	}
	return pSession->m_pSession;

	CATCH(SqlException, e)	
	{
		TRACE("%s\n", (LPCTSTR)e->m_strError);
		THROW_LAST();
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlConnection::RemoveSession(SqlSession* pSession)
{
	TB_LOCK_FOR_WRITE();
	m_arSessionPool.RemoveSession(pSession);
}

//-----------------------------------------------------------------------------
const ATLDataSource* SqlConnection::GetDataSource()
{
	return m_pDataSource;	
}

//-----------------------------------------------------------------------------
SqlCatalogConstPtr SqlConnection::GetCatalog()
{
	return SqlCatalogConstPtr(m_pCatalog, FALSE); //READ ONLY LOCK
}

//-----------------------------------------------------------------------------
BOOL SqlConnection::Open(LPCWSTR szInitString)
{ 
	if (!Check(m_pDataSource->OpenFromInitializationString(szInitString)))
		ThrowSqlException(GetDataSource()->m_spInit, IID_IDBInitialize, m_hResult);	

	// carico le properties
	if (!InitConnectionInfo())
	{
		AddMessage(_TB("I's impossible to read OLE DB Provider information"));
		return FALSE;
	}

	return TRUE;
}

// chiudere una connessione: questo vuole dire che non ci devono essere appesi
// command, transazioni, ecc.
//-----------------------------------------------------------------------------
BOOL SqlConnection::CanClose()
{ 	
	TB_LOCK_FOR_READ();
	if (!m_bValid) return TRUE;

	for (int nIdx = 0; nIdx < m_arSessionPool.GetUpperBound(); nIdx++) 	
		if (!m_arSessionPool.GetAt(nIdx)->CanClose())
			return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlConnection::Close()
{ 
	TB_LOCK_FOR_WRITE();
	if (m_pCheckTable && m_pCheckTable->IsOpen())
		m_pCheckTable->Close();

	
	//default sqlsession
	GET_THREAD_VARIABLE(CDefaultSqlSessions, m_DefaultSqlSessions);
	m_DefaultSqlSessions[this] = NULL;
	
	SqlSession* pSession;	
	for (int nIdx = m_arSessionPool.GetUpperBound(); nIdx >= 0; nIdx--) 	
	{
		pSession = m_arSessionPool.GetAt(nIdx);
		if (pSession)
			pSession->Close();
		delete pSession;
	}
	m_arSessionPool.RemoveAll();

}

//-----------------------------------------------------------------------------
BOOL SqlConnection::InitSysAdminParams()
{
	m_bAutocommit = !(m_pProviderInfo->m_bTransactions && AfxGetLoginInfos()->m_bTransactionUse);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlConnection::InitConnectionInfo()
{
	USES_CONVERSION;
	CComVariant var1, var2;		

	if (SUCCEEDED(m_pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_PROVIDERNAME, &var1)) &&
		SUCCEEDED(m_pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_PROVIDERVER, &var2)))
		m_pProviderInfo = AfxGetOleDbMng()->GetProviderInfo(OLE2T(var1.bstrVal), OLE2T(var2.bstrVal), this); 
	// nome del server
	if (SUCCEEDED(m_pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_DATASOURCENAME, &var1)))
		m_strServerName = OLE2T(var1.bstrVal);

	// nome del DBMS (Sql Server / Oracle)
	if (SUCCEEDED(m_pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_DBMSNAME, &var1)))
		m_strDbmsName = OLE2T(var1.bstrVal);

	// versione del DBMS
	if (SUCCEEDED(m_pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_DBMSVER, &var1)))
		m_strDbmsVersion = OLE2T(var1.bstrVal);

	// nome del database
	if (m_pProviderInfo->GetDBMSType() == DBMS_ORACLE)
	{
		if (SUCCEEDED(m_pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_USERNAME, &var1)))
			m_strDBName = m_strUserName = OLE2T(var1.bstrVal);
		if (m_strDBName.IsEmpty()) // sono in sicurezza integrata
			m_strDBName = m_strUserName = AfxGetLoginInfos()->m_strUserName;
	}
	else
	{
		if (SUCCEEDED(m_pDataSource->GetProperty(DBPROPSET_DATASOURCE, DBPROP_CURRENTCATALOG, &var1)))
			m_strDBName = OLE2T(var1.bstrVal);

		// nome utente connesso
		if (SUCCEEDED(m_pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_USERNAME, &var1)))
			m_strUserName = OLE2T(var1.bstrVal);
		if (m_strUserName.IsEmpty()) // sono in sicurezza integrata
			m_strUserName = AfxGetLoginInfos()->m_strUserName;
	}

	if (m_pProviderInfo)
		InitSysAdminParams();

	return m_pProviderInfo != NULL;
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
BOOL SqlConnection::MakeConnection(LPCWSTR szInitString)
{ 
	if (m_bOpen)
	{
		TRACE("SqlConnection::MakeConnection: the connection is already opened\n");
		AddMessage(cwsprintf(_TB("The connection {0-%s} it is already open"), szInitString));
		return FALSE;
	}

	m_bValid = TRUE;
	
	if (!Open(szInitString))
		return FALSE;
	
	m_bOpen = TRUE;

	// carico il catalog
	m_pCatalog = AfxGetSqlCatalog(this);
	
	if (m_pCatalog->DatabaseEmpty())
	{
		AddMessage(_TB("The database has not been created.\r\nPlease contact the program administrator."));
		return FALSE;
	}
	//se é una connessione che utilizza i meccanismi di TaskBuilder
	// allora devo gestire anche i Lock
	// e registrare le tabelle
	if (m_bCheckRegisterTable)
	{
		m_bTablesPresent = RegisterTables();
		if (!m_bTablesPresent)
		{
			AddMessage(_TB("Some tables required by the application are missing.\r\nPlease contact the program administrator."));
			return FALSE;
		}
	}

	m_bValid = !m_bCheckDBMark || CheckDatabase();
	
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
void SqlConnection::DefineCheckQuery()
{ 
	m_pCheckTable->Open();
	m_pCheckTable->SelectAll		();
	m_pCheckTable->AddFilterColumn	(m_pSqlMarkRec->f_Signature);
	m_pCheckTable->AddFilterColumn	(m_pSqlMarkRec->f_Module);
	m_pCheckTable->AddParam			(szParamSignature,	m_pSqlMarkRec->f_Signature);
	m_pCheckTable->AddParam			(szParamModule,		m_pSqlMarkRec->f_Module);		
}


//-----------------------------------------------------------------------------
BOOL SqlConnection::CheckRelease(AddOnModule* pAddOnMod, AddOnApplication* pAddOnApp /*=NULL*/)
{
	// se non prevedere database release non faccio nessun controllo
	if (!pAddOnMod || pAddOnMod->GetDatabaseRelease() == -1)
		return TRUE;

	AddOnApplication* pApp = (pAddOnApp) ? pAddOnApp : AfxGetAddOnApp(pAddOnMod->GetApplicationName()); 

	m_pCheckTable->SetParamValue	(szParamSignature,	DataStr(pApp->GetSignature()));
	m_pCheckTable->SetParamValue	(szParamModule,		DataStr(pAddOnMod->GetModuleSignature()));			
	m_pCheckTable->Query();

	return (m_pSqlMarkRec->f_Status);
}

//-----------------------------------------------------------------------------
void SqlConnection::CleanupCheckStructures()
{
	if (m_pCheckTable && m_pCheckTable->IsOpen())
		m_pCheckTable->Close();

	SAFE_DELETE(m_pCheckTable);
	SAFE_DELETE(m_pSqlMarkRec);
}

// devo confrontare le versioni attuali dei moduli con quelli presenti nella TB_DBMark 
// controllo il singolo modulo. Questo serve per il caricamento ondemand
//-----------------------------------------------------------------------------
BOOL SqlConnection::CheckAddOnModuleRelease(AddOnModule* pAddOnMod, CDiagnostic* pDiagnostic)
{
	TB_LOCK_FOR_WRITE();
	// se non prevedere database release non faccio nessun controllo
	if (!pAddOnMod || pAddOnMod->GetDatabaseRelease() == -1)
		return TRUE;
	
	if (!m_pSqlMarkRec && !m_pCheckTable)
	{
		m_pSqlMarkRec  = new SqlDBMark;
		m_pCheckTable  = new SqlTable(m_pSqlMarkRec, GetDefaultSqlSession());
	}

	ASSERT(m_pSqlMarkRec && m_pCheckTable);

	if (!m_pCheckTable->IsOpen())
		DefineCheckQuery();

	BOOL bOk = FALSE;
	TRY
	{
		bOk = CheckRelease(pAddOnMod);
		if (!bOk)
		{
			pDiagnostic->Add(
							cwsprintf
								(
									_TB("Signature: {0-%s}\tModule: {1-%s}\tExpected release: {2-%d}\tRelease on database: {3-%d}\tStatus: {4-%s}"),
									(LPCTSTR) pAddOnMod->GetApplicationName(), 
									(LPCTSTR) pAddOnMod->GetModuleName(), 
											  pAddOnMod->GetDatabaseRelease(),
									(int)	  m_pSqlMarkRec->f_DBRelease,
									(m_pSqlMarkRec->f_Status) ? _TB("Consistent") : _TB("Inconsistent")
								)
							);
			AfxGetLoginContext()->Lock ();
		}
	}
	CATCH(SqlException, e)
	{
		pDiagnostic->Add(e->m_strError);
		AfxGetLoginContext()->Lock ();
		return FALSE;
	}
	END_CATCH	
	
	return bOk;
}

// controllo di tutti i moduli caricati in memoria al momento della connessione
//-----------------------------------------------------------------------------
BOOL SqlConnection::CheckDatabase()
{ 
	// mi serve per la primaria che chiama in maniera esplicita il checkdatabase 
	m_bCheckDBMark = TRUE;

	//se non esiste il DBMark allora segnalo l'errore e mi fermo
	if (!ExistTable(SqlDBMark::GetStaticName()))
	{
		AddMessage(_TB("Not exist the table TB_DBMark. Please contact the administrator."));
		AfxGetLoginContext()->Lock ();
		return FALSE;
	}

	if (!m_pSqlMarkRec && !m_pCheckTable)
	{
		m_pSqlMarkRec  = new SqlDBMark;
		m_pCheckTable  = new SqlTable(m_pSqlMarkRec, GetDefaultSqlSession());
	}

	ASSERT(m_pSqlMarkRec && m_pCheckTable);

	if (!m_pCheckTable->IsOpen())
		DefineCheckQuery();

	//faccio il controllo solo per i moduli caricati in memoria
	BOOL			bOk = TRUE;
	BOOL			bFirstMsg = TRUE;

	AddOnApplication*	pAddOnApp;
	AddOnModule*		pAddOnMod;

	TRY
	{
		// scorro tutte le addonapplication
		for (int nApp = 0; nApp < AfxGetAddOnAppsTable()->GetSize(); nApp++)
		{
			pAddOnApp = AfxGetAddOnAppsTable()->GetAt(nApp);
			
			//scorro tutti i moduli caricati
			for (int nMod = 0; nMod < pAddOnApp->m_pAddOnModules->GetSize(); nMod++)
			{
				pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(nMod);

				if (!CheckRelease(pAddOnMod, pAddOnApp))
				{
					if (bFirstMsg)
					{
						AddMessage(_TB("Warning! The following database incompatibilities have been detected when connecting:"), CDiagnostic::Info);
						AddMessage(_T("==================================================="), CDiagnostic::Info);
						bFirstMsg = FALSE;
					}

					AddMessage(cwsprintf(
						_TB("Signature: {0-%s}\tModule: {1-%s}\tExpected release: {2-%d}\tRelease on database: {3-%d}\tStatus: {4-%s}"),
									(LPCTSTR) pAddOnMod->GetApplicationName(), 
									(LPCTSTR) pAddOnMod->GetModuleName(), 
											  pAddOnMod->GetDatabaseRelease(),
									(int)	  m_pSqlMarkRec->f_DBRelease,
									(m_pSqlMarkRec->f_Status) ? _TB("Consistent") : _TB("Inconsistent")
									));
					bOk = FALSE;
					AfxGetLoginContext()->Lock ();
				}
			}
		}
		if (!bOk)
			ShowMessage(TRUE);
	}
	CATCH(SqlException, e)	
	{
		AddMessage(e->m_strError);
		AfxGetLoginContext()->Lock ();
		return FALSE;
	}
	END_CATCH	

	return bOk;	
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

// da vedere per OLEDB



//-----------------------------------------------------------------------------
DBTYPE SqlConnection::GetSqlDataType (const DataType& aDataObjType)
{
	return m_pProviderInfo->GetSqlDataType(aDataObjType);
}

//-----------------------------------------------------------------------------
SqlColumnTypeItem* SqlConnection::GetSqlColumnTypeItem (const DataType& aDataObjType)
{
	return m_pProviderInfo->GetSqlColumnTypeItem(aDataObjType);
}

//
//-----------------------------------------------------------------------------
CString SqlConnection::NativeConvert (const DataObj* pDataObj, SqlRowSet* pRowSet /*NULL*/)
{
	// DataText before 3.0 where dealed always as unicode so by default NativeConvert 
	// behaves at the old way. In order to NativeConvert correctly DataText basing on
	// single column TEXT/NTEXT declaration the pRowSet is needed to inspect catalog 
	// information. 
	if (pDataObj->GetDataType() == DATA_TXT_TYPE && pRowSet)
	{
		SqlBindingElem* pBindElem = pRowSet->m_pColumnArray->GetElemByDataObj(pDataObj);
		if (pBindElem)
			return m_pProviderInfo->NativeConvert(pDataObj, pBindElem->IsUnicodeInDB());
	}

	// default behaviour
	return m_pProviderInfo->NativeConvert(pDataObj);
}


//-----------------------------------------------------------------------------
DBMSType SqlConnection::GetDBMSType() const
{
	return m_pProviderInfo->m_eDbmsType;
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
	aTable.m_pColumnArray->Add(_T("COUNT(*)"), &aCount, GetSqlDataType(DATA_LNG_TYPE), nEmptySqlRecIdx);
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
	USES_CONVERSION;
	CComVariant var;		

	if (SUCCEEDED(m_pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_CONNECTIONSTATUS, &var)))
		return var.lVal == DBPROPVAL_CS_INITIALIZED;

	// if GetProperty fails, default is ok
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
	
	m_nPrimaryConnection = Add(pConnection);
}

//-----------------------------------------------------------------------------
SqlConnection* SqlConnectionPool::GetPrimaryConnection() 
{
	if (GetSize() >= 1 && m_nPrimaryConnection != nNoPrimaryConnection)
		return GetAt(m_nPrimaryConnection);

	return NULL;
}

////-----------------------------------------------------------------------------
//SqlConnection* SqlConnectionPool::GetFirstActiveConnection (const CString& strServerName,
//																  const CString& strDBName)
//{
//	for (int i = 0; i < GetSize(); i++)
//	{
//		if ((GetAt(i)->m_strDBName.CompareNoCase(strDBName) == 0) &&
//			(GetAt(i)->m_strServerName.CompareNoCase(strServerName) == 0))
//			return GetAt(i);
//	}
//
//	return NULL;
//}
