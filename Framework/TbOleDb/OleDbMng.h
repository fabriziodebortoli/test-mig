
#pragma once

#include <TbGenlib\baseapp.h>

#include "sqlconnect.h"
#include "sqlproviderinfo.h"
#include "sqlobject.h"

//includere alla fine degli include del .H
#include "beginh.dex"

enum DMSStatusEnum { Valid, StorageInvalid, DbInvalid };

///////////////////////////////////////////////////////////////////////////////
//			class COleDbManager implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT COleDbManager : public SqlObject, public CTBLockable
{    
friend class SqlTable;
friend class SqlConnection;
friend class CDataCachingContext;
friend class CLoginThread;
friend class CTbCommandManager;
friend class CLibrariesLoader;
friend class SqlRecoveryManager;
friend class DocRecoveryManager;

	DECLARE_DYNAMIC(COleDbManager)
private:
	SqlConnectionPool				m_aConnectionPool;

	CString							m_strTraceSqlFile;
	CMap<CString,LPCTSTR,int,int>	m_TracedSqlActions;
	CStringArray					m_TracedSqlTables; // used to allow the user to trace only some tables
	CString							m_strTraceSqlActions;

	SqlProviderInfoPool				m_aProviderInfoPool;
	volatile bool					m_bValid;	

	// parameters caching in order to improve performance
	BOOL							m_bReadTraceColumnsAfterUpdate;
	BOOL							m_bUseLockManager;

	CString							m_strDMSConnectionString; //connection string to Easy Attachment database  @@Easy Attachment
	DMSStatusEnum					m_DMSStatus;

public:
	COleDbManager();
	~COleDbManager();

private: 
	BOOL	MakePrimaryConnection	();
	BOOL	CanCloseAllConnections	();
	BOOL	CloseAllConnections		();

	// per le librerie caricate ondemand.
	// ciclo su tutte le connessioni che prevedono la registrazione delle tabelle 
	// dell'applicativo
	BOOL 	RegisterAddOnTable		(AddOnLibrary*);
	BOOL 	CheckAddOnModuleRelease	(AddOnModule*, CDiagnostic*);
	
	SqlProviderInfo*	GetProviderInfo		(LPCTSTR pszProviderName, LPCTSTR pszProviderVersion, SqlConnection*);

	// Data Caching Management
	CDataCachingManager* GetDataCachingManager	();
	CDataCachingContext* GetDataCachingContext	();

public:
	// richiedo una connessione passando la stringa necessaria alla connessione e
	// se devo o meno controllare il contenuto della DBMark
	// se devo o meno utilizzare la gestione dei lock
	// se devo o meno effettuare la registrazione delle tabelle dell'applicativo
	// posso passare il dbOwner nel caso di connessione ad un database Oracle con utente non proprietario
	// ma utilizzatore mediante sinonimi
	SqlConnection*	MakeNewConnection	
				(
					LPCWSTR szConnectionString, 
					BOOL bCheckDBMark = FALSE, 
					BOOL bUserLockMng = FALSE, 
					BOOL bCheckRegisterTable = FALSE,
					CString strDbOwner = _T(""),
					CBaseContext* pContext = NULL
				);
	// effettua la chiusura della connessione passata come parametro e la toglie dal
	// connection pool effettuando il anche il delete del puntatore
	BOOL			CloseConnection		(SqlConnection*);

	
	SqlConnection*	GetPrimaryConnection	() 	{TB_LOCK_FOR_READ(); return m_aConnectionPool.GetPrimaryConnection(); }
	SqlSession*		GetDefaultSqlSession	();	

	// controllo nel SqlConnectionPool se esiste già una connessione con lo stesso
	// nome database e lo stesso server
	//SqlConnection* GetFirstActiveConnection(const CString&, const CString&);

	// restituisce le provider info relative al Provider passato come primo parametro
	// Se le info non sono giá state caricate in una precedente connessione utilizza il secondo
	// parametro per caricarle
	BOOL				IsValid()			{ return m_bValid;}

	//for lock tracer
	virtual LPCSTR			GetObjectName() const { return "COleDbManager"; }

	//Auxiary functions for EasyAttachment 
	CString				GetDMSConnectionString	();
	BOOL				EasyAttachmentEnable	();
	BOOL				EasyAttachmentFullEnable();	
	BOOL				DMSMassiveEnable		();
	BOOL				DMSSOSEnable();
	void				SetDMSStatus			(DMSStatusEnum status) { m_DMSStatus = status; }
	DMSStatusEnum		GetDMSStatus			() const { return m_DMSStatus; }
	
private:
	void 	OnDSNChanged	();	
	void	CheckLockManager(SqlConnection* pSqlConnection);

public:
	// gestione dell'accesso limitato ad un solo utente
	BOOL EnableMultiUserMode	(BOOL bEnable = TRUE);
	BOOL MultiUserModeEnabled	() const;


public:
	void DebugTraceSQL(LPCTSTR szTrace, SqlObject* pSqlObject);
	void ResetSQLActionCounters(const CString& strMess);
	void TraceSQLSeparator(const CString& strMess);
	CString GetSQLActionCounters(const CString& strMess);

	// parameters caching
	void  CacheParameters					();
	const BOOL&	ReadTraceColumnsAfterUpdate	() const { return m_bReadTraceColumnsAfterUpdate; }
	const BOOL&	UseLockManager				() const { return m_bUseLockManager; }
};

//============================================================================
// Global Windows state data helper functions
//============================================================================

TB_EXPORT COleDbManager* AfxGetOleDbMng ();

TB_EXPORT SqlConnection* AfxGetDefaultSqlConnection ();

/*TBWebMethod*/TB_EXPORT SqlSession*	AfxGetDefaultSqlSession ();
/*TBWebMethod*/TB_EXPORT SqlSession*	AfxOpenSqlSession (DataStr connectionString);

/*TBWebMethod*/TB_EXPORT SqlRecord*		AfxCreateRecord (DataStr tableName);

//============================================================================
#include "endh.dex"


