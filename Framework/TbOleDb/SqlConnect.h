
#pragma once

#include <lm.h>

#include <TbGeneric\array.h>

#include "sqlobject.h"
#include "sqlproviderinfo.h"
#include "sqlcatalog.h"


//includere alla fine degli include del .H
#include "beginh.dex"
//============================================================================

class SqlConnection;
class CRTAddOnNewFieldsArray;
class SqlRowSet;
class SqlDBMark;
class SqlTable;
class AddOnLibrary;
class AddOnApplication;
class CLockManagerInterface;

// classi di TBDatabaseManaged
class 	MSqlConnection;






//in questo file sono presenti le classi per la gestione della connessione
// e della sessione
//
// SqlCommandPool = insieme degli SqlRowSet aperti in una sessione
// SqlSession = gestisce una singola sessione
// SqlSessionPool = insieme delle sessioni aperte in una connessione
// SqlConnection = gestisce una singola connessione
// SqlConnectionPool = insieme delle connessioni aperte dall'applicativo

//////////////////////////////////////////////////////////////////////////////
//						SqlCommandPool
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT SqlCommandPool : public CObArray
{
	DECLARE_DYNAMIC(SqlCommandPool)

public:                                  
	SqlRowSet*	GetAt		(int nIndex) const	{ return (SqlRowSet*) CObArray::GetAt(nIndex); }
	SqlRowSet*&	ElementAt	(int nIndex)		{ return (SqlRowSet*&) CObArray::ElementAt(nIndex); }

	SqlRowSet*	operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	SqlRowSet*&	operator[]	(int nIndex)		{ return ElementAt(nIndex); }

public:
	void AddCommand				(SqlRowSet* pSqlCommand);
	void RemoveCommand			(SqlRowSet* pSqlCommand);

	bool ExistConnectedCommands (); //se esistono dei SqlCommand con SqlDataReader ancora connessi 
	void CloseAllCommands		();
	void ReleaseAllCommands		();


// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlCommandPool\n"); }
	void AssertValid() const{ CObArray::AssertValid(); }
#endif //_DEBUG
};

//////////////////////////////////////////////////////////////////////////////
//						SqlSession
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
class TB_EXPORT SqlSession : public SqlObject
{
	friend class SqlConnection;
	friend class SqlRowSet;
	friend class CTransactionContext;
	friend class SqlPerformanceManager;
	friend class SqlRecoveryManager;

	DECLARE_DYNAMIC(SqlSession)

private:
	MSqlConnection*		m_pSession;
	bool				m_bForUpdate; //false di default; se true vuo dire che è una sessione istanziata per la gestione dei command di tipo Update, Insert e Delete

protected:
	SqlCommandPool		m_arCommandPool;	// array di comandi attualmente attivi legati alla sessione
	
	BOOL				m_bTxnInProgress;// TRUE se é aperta la transazione é in corso
	BOOL				m_bOwnSession;	// TRUE se la sessione è stata aperta direttamente
										// FALSE se il puntatore ci viene passato da fuori
	BOOL			    m_bStayOpen;

	//serve per la transazione. 
	//In caso di SqlDataReader ancora attivi sulla sessione la transazione non andrà a buon fine
	// devo nettamente dividere i command di lettura (che utilizzano quindi i SqlDataReader) da quelli di scrittura (con ExecuteNonQuery)
	SqlSession*			m_pUpdatableSqlSession;

public:
	SqlConnection*		m_pSqlConnection;	// connessione che ha generato la sessione


public:
	//posso lavorare in un contesto differente rispetto a quello della connessione
	// vedi gestione messaggi legati al contesto del documento
	SqlSession	(SqlConnection* pConnection, CBaseContext* = NULL);
	SqlSession	(MSqlConnection*, SqlConnection*, CBaseContext* = NULL);
	~SqlSession	();

private:
	SqlSession* CreateUpdatableSqlSession();

public:
	MSqlConnection*		GetMSqlConnection() const;
	virtual ::DBMSType	GetDBMSType() const;

	void		Open			();
	BOOL		IsOpen			() const;	
	// metodi per la gestione delle transazioni da parte della session
	SqlSession*				GetUpdatableSqlSession();
	/*TBWebMethod*/void		StartTransaction	();
	/*TBWebMethod*/void		Commit				();
	/*TBWebMethod*/void		Abort				();

	// gestione commandpool
	void		AddCommand		(SqlRowSet*);
	void		RemoveCommand	(SqlRowSet*);

	BOOL		CanClose		() const;
	/*TBWebMethod*/void		Close (); 
	void ForceClose();//serve per forzare la chiusura indipendentemente dal valore di bStayOpen
	
	void		ReleaseCommands	();	

public:
	SqlConnection*		GetSqlConnection()	const { return m_pSqlConnection; }
	MSqlConnection* GetSession() const { return m_pSession; }
	BOOL				IsTxnInProgress()	const;

public:
	virtual void		GetErrorString(HRESULT nResult, CString& m_strError);




// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlSession "); SqlObject::Dump(dc);}
	void AssertValid() const{ SqlObject::AssertValid(); }
#endif //_DEBUG
};

//////////////////////////////////////////////////////////////////////////////
//						SqlSessionPool
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT SqlSessionPool : public CObArray
{
	DECLARE_DYNAMIC(SqlSessionPool)
	
public:                                  
    SqlSession*		GetAt		(int nIndex) const	{ return (SqlSession*) CObArray::GetAt(nIndex); }
	SqlSession*&	ElementAt	(int nIndex)		{ return (SqlSession*&) CObArray::ElementAt(nIndex); }

	SqlSession*		operator[]	(int nIndex) const	{ return GetAt(nIndex); }
	SqlSession*&	operator[]	(int nIndex)		{ return ElementAt(nIndex); }

public:
	void RemoveSession(SqlSession*);
	void ForceCloseSessions(); 

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlSessionPool\n"); }
	void AssertValid() const{ CObArray::AssertValid(); }
#endif //_DEBUG
};

//////////////////////////////////////////////////////////////////////////////
//						SqlConnection
//////////////////////////////////////////////////////////////////////////////////
//-------------------------------------------------------------------	----------
class TB_EXPORT SqlConnection : public SqlObject, public CTBLockable
{
	friend class COleDbManager;
	friend class SqlSession;
	friend class SqlCatalog;
	friend class SqlTables;
	friend class SqlTable;
	friend class SqlRowSet;
	//friend class SqlProviderInfo;
	friend class CBaseContext;
	friend class CTBContext;
	friend class SqlRecoveryManager;
	friend class CLoginThread;
	friend class SqlRecord;
	friend class SqlConnectionPool;

	DECLARE_DYNAMIC(SqlConnection)

private:
	// m_bCheckRegisterTable = TRUE controllo l'esistenza delle tabelle registrate dall'applicativo
	BOOL				m_bCheckRegisterTable; 
	SqlCatalog*			m_pCatalog;
	SqlSessionPool		m_arSessionPool; // array di sessioni

	BOOL				m_bValid;
	BOOL				m_bTablesPresent;
	BOOL				m_bOpen;
	long				m_nProviderID;
	BOOL				m_bAutocommit;
	BOOL				m_bUseUnicode;
	CString				m_strConnectionString;
	bool				m_bAlwaysConnected;
	int					m_nAlwaysConnectedRef; //serve per capire quante volte è stato chiamato la AlwaysConnect poichè la connessione di documentThread è condivisa tra tutti i documenti istanziati nel thread.
												// Viene incrementato e decrementato nella SetAlwaysConnected a seconda del valore del booleano

	CLockManagerInterface* m_pLockManagerInterface;
public:
	enum  ExecuteResult { EXECUTE_ERROR, EXECUTE_SUCCESS, LOCKED };

private:
	// informazioni generici sulla connessione
	CString			m_strDBName;		// nome del database a cui sono connessa
	CString			m_strServerName;	// server
	CString			m_strUserName;		// utente connesso
	CString			m_strDBOwner;		// database owner
	CString			m_strDbmsName;		// nome del DBMS
    CString			m_strDbmsVersion;	// versione del DBMS
	CString			m_strAlias;			//è possibile identificare la connessione con un alias


	//gestione dei settings. Li leggo nella conessione x non dover ogni volta in fase di istanziazione
	// di un oggetto sql dover chiedere al settings manager il valore
	BOOL m_bUsePerformanceMng; //gestione delle performance	
	BOOL m_bOptimizedHKL; //ottimizzazione query hotlink

public:
	SqlConnection() {}
	SqlConnection(const CString& strConnectionString, BOOL bCheckRegisterTable, CBaseContext* = NULL, const CString& strAlias = _T(""));
	SqlConnection(CBaseContext*);
	~SqlConnection();

public:
	virtual ::DBMSType	GetDBMSType() const;

	// restituisce la session di default come m_arSessionPool->GetAt(0) (di tipo SqlSession)
	SqlSession*	GetDefaultSqlSession();	
	
	// crea una nuova sessione e la inserisce in m_arSessionPool
	SqlSession* GetNewSqlSession(CBaseContext* pBaseContext = NULL);
	SqlSession* GetNewSqlSession(CTBContext* pTBContext);

	// rimuove la sessione dalla session pool
	void		RemoveSession(SqlSession*);

	void				SetConnectionString(const CString& strConnectionString);
	const CString&		GetConnectionString() { return m_strConnectionString; };

	// se si può chiudere o meno una connessione
	BOOL		CanClose			();
	void		Close				();
	BOOL		IsOpen				() const { return m_bOpen; }
	BOOL		IsValid				() const { return m_bOpen && m_bValid; }
	BOOL		IsAutocommit		() const { return m_bAutocommit; }
	BOOL		UseUnicode			() const { return m_bUseUnicode; }
	BOOL		IsAlive				() const;

	bool		AlwaysConnected		()		const	{ return m_bAlwaysConnected; }
	void		SetAlwaysConnected(bool bSet);

	SqlConnection* Clone();

	SqlCatalogConstPtr 		GetCatalog	();

	//adesso il lock manager interface è di proprietà della connessione essendo basato su tabella di sistema
	CLockManagerInterface* GetLockManagerInterface();

	void LoadTables(::CMapStringToOb* pTables);
	void LoadProcedures(::CMapStringToOb* pTables);

	void LoadColumnsInfo(const CString&  strTableName, ::Array* arPhisycalColumns);
	void LoadProcedureParametersInfo(const CString& strProcName, Array* pProcedureParams);

	void LoadForeignKeys(const CString& sFromTableName, const CString& sToTableName, BOOL bLoadAllToTables, CStringArray* pFKReader);


	
	BOOL					ExistTable				(const CString& strTableName);
	SqlTableInfo*			GetTableInfo			(const CString& strTableName);
	CRuntimeClass*			GetSqlRecordClass		(const CString& strTableName);
	CRuntimeClass*			GetSqlRecordClass		(const CTBNamespace& ns);
    const CRTAddOnNewFieldsArray* GetSqlNewFieldRT	(const CString& strTableName);
	const SqlCatalogEntry*	GetCatalogEntry			(const CString& strTableName);

	// chiamati per i moduli caricati ondemand
	BOOL					RegisterCatalogEntry
								(
									LPCTSTR				pszSignature, 
									const CTBNamespace&	aNamespace,
									CRuntimeClass*		pSqlRecordClass, 
									int					nType 
								);
	BOOL RegisterAddOnLibrayTables	(AddOnLibrary*, AddOnApplication* = NULL);	

	BOOL CheckAddOnModuleRelease(AddOnModule*, CDiagnostic*) { return TRUE; }	//lasciato per compatibiità di codice 
	BOOL					TablesPresent	()	const { return m_bTablesPresent; }
	
	virtual LPCSTR		GetObjectName() const	 { return "SqlConnection"; }

private:
	void Initialize		();
	BOOL MakeConnection();
	

	BOOL RegisterTables		();	
	
	// ATL queries to read collation settings
	void	RefreshTraces				();
	void	SetUseUnicode				(BOOL bSet)		{ m_bUseUnicode = bSet; }
	void	SetProviderId				(long nProvID)	{ m_nProviderID = nProvID; }

public:
	CString		NativeConvert		(const DataObj* pDataObj);

	// direct SQL execution without bind columns
	void ExecuteSQL(LPCTSTR lpszSQL, SqlSession* pSession = NULL);	

	//It's possibile lock one of the tables that is in lpszSQL
	//se bLock =  TRUE allora posso fare il lock di tabella 
	ExecuteResult ExtendedExecuteSQL(LPCTSTR lpszSQL, SqlSession* pSession, BOOL bLock, const CString& strLockKey,  BOOL bUseMessageBox = TRUE, LockRetriesMng* pRetriesMng = NULL);

	// serve per poter eseguire uno script SQL contenuto in un file memorizzato 
	// nella struttura standard di Script
	BOOL	RunScript		(LPCTSTR lpszFileName);
	void	UpdateStatistics();

	// Funzioni utili per controllare se la tabella è vuota
	BOOL	IsEmptyTable		(const CString& strTableName);
	long	RecordCountNumber	(const CString& strTableName);

public:
	CString			GetDatabaseName				() const	{ return m_strDBName; }
	CString			GetDatabaseServerName		() const	{ return m_strServerName; }
	CString			GetDatabaseOwner			() const	{ return m_strDBOwner; }
	CString			GetDbmsVersion				() const	{ return m_strDbmsVersion; }
	CString			GetDbmsName					() const	{ return m_strDbmsName; }
	CString			GetAlias					() const	{ return m_strAlias; }

	const CString&	GetDatabaseCollation		();
	BOOL			GetUsePerformanceMng		() const	{ return m_bUsePerformanceMng; }

	int		GetProviderId()			const	{ return m_nProviderID; }
	BOOL	IsUserConnectDbOwner()	const   { return m_strDBOwner.IsEmpty()|| m_strDBOwner.Compare(m_strUserName) == 0;}
	


// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlConnection "); SqlObject::Dump(dc);}
	void AssertValid() const{ SqlObject::AssertValid(); }
#endif //_DEBUG
};
					

//////////////////////////////////////////////////////////////////////////////
//						SqlConnectionPool
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class SqlConnectionPool : public Array
{
	DECLARE_DYNAMIC(SqlConnectionPool)

	int		m_nPrimaryConnection;

public:
	SqlConnectionPool ();

public:                                  
  	SqlConnection* 	GetAt		(int nIndex) const	{ return (SqlConnection*) Array::GetAt(nIndex);	}
	SqlConnection*&	ElementAt	(int nIndex)		{ return (SqlConnection*&) Array::ElementAt(nIndex); }
	
	SqlConnection* 	operator[]	(int nIndex) const	{ return GetAt(nIndex);	}
	SqlConnection*&	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}

public:
	BOOL RemovePrimaryConnection();
	BOOL RemoveConnection(SqlConnection*);	
	void SetPrimaryConnection	(SqlConnection*);
	BOOL CanCloseAll() const;
	void CloseAll();
	
public:
	SqlConnection* GetPrimaryConnection		();
	SqlConnection* GetSqlConnectionByAlias(const CString& strAlias);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlConnectionPool\n"); }
	void AssertValid() const{ Array::AssertValid(); }
#endif //_DEBUG
};

#include "endh.dex"

