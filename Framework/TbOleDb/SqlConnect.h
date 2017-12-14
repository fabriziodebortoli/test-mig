
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
class SqlConnection;
class SqlCatalog;
class SqlCatalogEntry;
class SqlTableInfo;
class SqlDBMark;
class SqlTable;
class AddOnLibrary;
class AddOnModule;
class AddOnApplication;
class SqlColumnInfo;

// classi di ATL
class ATLDataSource;
class ATLSession;

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

protected:
	ATLSession*			m_pSession;
	SqlCommandPool		m_arCommandPool;	// array di comandi attualmente attivi legati alla sessione
	
	BOOL				m_bTxnInProgress;// TRUE se é aperta la transazione é in corso
	BOOL				m_bOwnSession;	// TRUE se la sessione è stata aperta direttamente
										// FALSE se il puntatore ci viene passato da fuori
public:
	SqlConnection*		m_pSqlConnection;	// connessione che ha generato la sessione

public:
	//posso lavorare in un contesto differente rispetto a quello della connessione
	// vedi gestione messaggi legati al contesto del documento
	SqlSession	(SqlConnection* pConnection, CBaseContext* = NULL);
	SqlSession	(ATLSession*, SqlConnection*, CBaseContext* = NULL);
	~SqlSession	();

public:
	void		Open			();
	// metodi per la gestione delle transazioni da parte della session
	/*TBWebMethod*/void		StartTransaction	();
	/*TBWebMethod*/void		Commit				();
	/*TBWebMethod*/void		Abort				();

	// gestione commandpool
	void		AddCommand		(SqlRowSet*);
	void		RemoveCommand	(SqlRowSet*);

	BOOL		CanClose		() const;
	/*TBWebMethod*/void		Close			();
	void		ReleaseCommands	();
	
	//DataCaching management
	void SetDataCachingContext(CDataCachingContext* pDataCachingContext);

public:
	SqlConnection*		GetSqlConnection()	const { return m_pSqlConnection; }
	BOOL				IsTxnInProgress()	const { return m_bTxnInProgress; }

public:
	virtual ::DBMSType	GetDBMSType			() const;
	virtual void		GetErrorString(HRESULT nResult, CString& m_strError);

public:
	ATLSession* GetSession() const { return m_pSession; }

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
	friend class SqlProviderInfo;
	friend class CBaseContext;
	friend class CTBContext;
	friend class SqlRecoveryManager;
	friend class CLoginThread;
	friend class SqlRecord;

	DECLARE_DYNAMIC(SqlConnection)

private:
	// m_bCheckRegisterTable = TRUE controllo l'esistenza delle tabelle registrate dall'applicativo
	// m_bUseLockMng = TRUE uso il lock manager via socket
	// m_bCheckDBMark = TRUE controllo la tabella TB_DBMARK 
    // attenzione il controllo della TB_DBMARK in caso di primary connection sono fatti esternamente dopo
	// aver creato la connessione di default
	BOOL m_bCheckRegisterTable; 
	BOOL m_bCheckDBMark;

	// mi tengo aperto il cursore nel caso devo effettuare il controllo del database
	SqlTable*	m_pCheckTable;
	SqlDBMark*	m_pSqlMarkRec;
	
	SqlCatalog*			m_pCatalog;
	ATLDataSource*		m_pDataSource;
	
	SqlSessionPool		m_arSessionPool; // array di sessioni

	BOOL				m_bValid;
	BOOL				m_bTablesPresent;

	// START ************ da SqlDatabase *********************
	BOOL				m_bOpen;
	long				m_nProviderID;
	BOOL				m_bAutocommit;
	BOOL				m_bUseUnicode;

public:
	const SqlProviderInfo*	m_pProviderInfo;
	enum  ExecuteResult { EXECUTE_ERROR, EXECUTE_SUCCESS, LOCKED };

private:
	// informazioni generici sulla connessione
	CString			m_strDBName;		// nome del database a cui sono connessa
	CString			m_strServerName;	// server
	CString			m_strUserName;		// utente connesso
	CString			m_strDBOwner;		// database owner
	CString			m_strDbmsName;		// nome del DBMS
    CString			m_strDbmsVersion;	// versione del DBMS


	//gestione dei settings. Li leggo nella conessione x non dover ogni volta in fase di istanziazione
	// di un oggetto sql dover chiedere al settings manager il valore
	BOOL m_bUsePerformanceMng; //gestione delle performance	
	BOOL m_bOptimizedHKL; //ottimizzazione query hotlink

	CursorType m_eHKLTRCursor; //Cursor Type for TableReader and HotLink
	CursorType m_eROForwardCursor; //Cursor Type for read only forward SqlTable

	BOOL	   m_bRemoveDeletedRows;

public:
	SqlConnection	(CBaseContext* = NULL, CString dbOwner= _T(""));
    SqlConnection	(BOOL bCheckRegisterTable, BOOL bUseLockMng, BOOL bCheckDBMark, CBaseContext* = NULL, CString dbOwner = _T(""));
	~SqlConnection	();

public:
	// restituisce la session di default come m_arSessionPool->GetAt(0)->m_pSession 
	ATLSession*	GetDefaultSession	();	
	
	// crea una nuova sessione e la inserisce in m_arSessionPool
	ATLSession*	GetNewSession		(CBaseContext* pContext = NULL); 

	// restituisce la session di default come m_arSessionPool->GetAt(0) (di tipo SqlSession)
	SqlSession*	GetDefaultSqlSession();	
	
	// crea una nuova sessione e la inserisce in m_arSessionPool
	SqlSession* GetNewSqlSession(CBaseContext* pBaseContext = NULL);
	SqlSession* GetNewSqlSession(CTBContext* pTBContext);

	// rimuove la sessione dalla session pool
	void		RemoveSession(SqlSession*);

	// restituisce un m_pDataSource castato a ATLDataSource
	const ATLDataSource*	GetDataSource		();
	
	// se si può chiudere o meno una connessione
	BOOL		CanClose			();
	void		Close				();
	BOOL		IsOpen				() const { return m_bOpen; }
	BOOL		IsValid				() const { return m_bOpen && m_bValid; }
	BOOL		IsAutocommit		() const { return m_bAutocommit; }
	BOOL		UseUnicode			() const { return m_bUseUnicode; }
	BOOL		IsAlive				() const;
	
	SqlCatalogConstPtr 		GetCatalog		();
	
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
	BOOL CheckAddOnModuleRelease	(AddOnModule*,  CDiagnostic*);
	void CleanupCheckStructures		();

	// retituisce le infomazioni sul Provider
	const SqlProviderInfo*	GetProviderInfo	()	const { return m_pProviderInfo; }
	BOOL					TablesPresent	()	const { return m_bTablesPresent; }
	
	virtual LPCSTR		GetObjectName() const	 { return "SqlConnection"; }

private:
	void Initialize			();
	BOOL MakeConnection		(LPCOLESTR szInitString);
	BOOL Open				(LPCWSTR szInitString);
	BOOL InitConnectionInfo	();
	void InitQuoteChar		();
	BOOL InitSysAdminParams	();

	BOOL RegisterTables		();
	void DefineCheckQuery	();
	BOOL CheckRelease		(AddOnModule*, AddOnApplication* = NULL);

	BOOL CheckDatabase		();

	// ATL queries to read collation settings
	void	RefreshTraces				();
	void	SetUseUnicode				(BOOL bSet)		{ m_bUseUnicode = bSet; }
	void	SetProviderId				(long nProvID)	{ m_nProviderID = nProvID; }

public:
	// ProviderInfo dependent function
	DBTYPE					GetSqlDataType		(const DataType&);
	SqlColumnTypeItem*		GetSqlColumnTypeItem(const DataType&);
	CString					NativeConvert		(const DataObj* pDataObj, SqlRowSet* pRowSet = NULL);
	virtual ::DBMSType		GetDBMSType			() const;

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
	BOOL			GetUsePerformanceMng		() const	{ return m_bUsePerformanceMng; }
	CursorType		GetROForwardCursor			() const	{ return m_eROForwardCursor; }
	CursorType		GetHKLTRCursor				() const	{ return m_eHKLTRCursor; }
	BOOL			RemoveDeletedRows			() const	{ return m_bRemoveDeletedRows; }
	const CString&	GetDatabaseCollation		();
	BOOL			IsCollationCultureSensitive	(SqlColumnInfo* pColumnInfo){ return m_pCatalog->IsCollationCultureSensitive(pColumnInfo, this); }


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
	//SqlConnection* GetFirstActiveConnection	(const CString&, const CString&);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlConnectionPool\n"); }
	void AssertValid() const{ Array::AssertValid(); }
#endif //_DEBUG
};

#include "endh.dex"

