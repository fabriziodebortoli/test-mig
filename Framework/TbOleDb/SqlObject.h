
#pragma once

#include <TBGeneric\Array.h>
#include <TBGenlib\Messages.h>
#include <TBGenlib\BaseDoc.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbDatabaseManaged\SqlBindingObjects.h>
#include <TbDatabaseManaged\MSqlConnection.h>

#include "sqlcache.h"
#include "sqllock.h"
#include "performanceanalizer.h"


//includere alla fine degli include del .H
#include "beginh.dex"

class SqlRowSet;
class SqlSession;
class SqlSessionPool;
class SqlObject;
class CTBContext;

//x la tracciatura delle istruzioni x la comunicazione con il DB
// .............................................................. Debug
TB_EXPORT bool AFXAPI IsTraceSQLEnabled();

#define TRACE_SQL(a,b)		if (IsTraceSQLEnabled())	DebugTraceSQL(a,b)


// per la gestione dei tipi di cursore
// E_NO_CURSOR: il rowset non utilizza cursori
enum CursorType {E_NO_CURSOR = -1, E_FORWARD_ONLY, E_FAST_FORWARD_ONLY, E_KEYSET_CURSOR, E_DYNAMIC_CURSOR, E_STATIC_CURSOR };
TB_EXPORT CString GetCursorTypeDescription (CursorType eCursorType);


// enumerativo i tipi di DBMS
//-----------------------------------------------------------------------------
enum TB_EXPORT DBMSType { DBMS_SQLSERVER, DBMS_ORACLE, DBMS_UNKNOWN };


TB_EXPORT void AFXAPI DebugTraceSQL(LPCTSTR szTrace, SqlObject* pSqlObject);

TB_EXPORT void AFXAPI ResetSQLActionCounters(const CString& strMess);
TB_EXPORT void AFXAPI TraceSQLSeparator(const CString& strMess);
TB_EXPORT CString AFXAPI GetSQLActionCounters(const CString& strMess);

//-------------------------------------------------------------------------------------
//TB_EXPORT void AFXAPI ThrowSqlException	(LPUNKNOWN lpUnk, const IID& iid, HRESULT nHResult, SqlObject* = NULL);
TB_EXPORT void AFXAPI ThrowSqlException	(LPCTSTR pszError, SqlObject* = NULL);
TB_EXPORT void AFXAPI ThrowSqlException	(MSqlException* pMSqlException);


/////////////////////////////////////////////////////////////////////////////
//						class CSharedContext
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CSharedContext : public CObject
{
protected:
	BOOL				m_bOwnedContext;
	BOOL				m_bUnchangeable;
	CBaseDocument*		m_pDocument;	

public:
	CSharedContext(CBaseDocument* pDocument) 
	: 
	m_bOwnedContext	(TRUE), 
	m_bUnchangeable (FALSE),
	m_pDocument		(pDocument)
	{}

	CSharedContext(const CSharedContext& pSharedContex) 
	: 
	m_bOwnedContext	(FALSE)
	{
		m_bUnchangeable = pSharedContex.m_bUnchangeable;
		m_pDocument		= pSharedContex.m_pDocument;
	}

public:
	BOOL	IsOwnContext() { return m_bOwnedContext; }

};
// parte di contesto relativo per la gestione della messaggistica
/////////////////////////////////////////////////////////////////////////////
//						class CBaseContext
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CBaseContext : public CSharedContext
{	
	friend SqlLockMng;
	friend CTBContext;

private:
	SqlLockMng*		m_pLockMng;
	CDiagnostic*	m_pDiagnostic;
	CArray<CDiagnostic*, CDiagnostic*>*	m_pDiagnosticPool;
	//BOOL			m_bCanUnlock;
	BOOL			m_bCanDeleteLockMng;
		
protected:
	BOOL			m_bLocked;

public:
	bool*			m_bOptimisticLock;

public:
	//gestione performance
	SqlPerformanceManager*	m_pSqlPerformanceMng;

public:
	CBaseContext(CBaseDocument* pDocument = NULL);
	CBaseContext(const CBaseContext&);
	CBaseContext(CDiagnostic*);
	~CBaseContext();

public:
	void	AddMessage		(LPCTSTR, CDiagnostic::MsgType = CDiagnostic::Error);
	void	ShowMessage		(LPCTSTR, CDiagnostic::MsgType = CDiagnostic::Error);
	void	ShowMessage		(BOOL bClear = FALSE);
	CDiagnostic* GetDiagnostic()	const	{ return m_pDiagnostic; }
	
	BOOL	ErrorFound		()	const	{ return m_pDiagnostic->ErrorFound(); }

	void	StartNewDiagnostic(BOOL bShowMsg = TRUE);
	void	EndNewDiagnostic(BOOL bCopy =FALSE, const CString& strOpeningBanner = _T("") , const CString& strClosingBanner = _T(""));

	//void	SetCanUnlock() {m_bCanUnlock = TRUE;}

public: 	
	CBaseDocument*	GetDocument	()	const  { return m_pDocument; }

	//@@LOCK
	BOOL	IsValidDocument () const 
			{ return m_pDocument && AfxIsValidAddress((m_pDocument), sizeof(CBaseDocument));}

	BOOL	IsLocked		()	const { return m_bLocked; }

public:
	//@@OPTIMISTICLOCK Optimistic lock support
	void	EnableOptimisticLock(bool bEnable = true) { *m_bOptimisticLock = bEnable;	}

    // Pessimistic Lock support	
	BOOL 	IsCurrentLocked	(SqlTable* pTable);
	BOOL 	LockCurrent		(SqlTable* pTable, BOOL bUseMessageBox = TRUE, LockRetriesMng* pRetriesMng = NULL);
	//to lock a document so to avoid multiuser m_pDocument runs.
	BOOL	LockDocument	(); 

	BOOL 	UnlockCurrent	(SqlTable* pTable);	
	BOOL	UnlockAll();						
	BOOL	UnlockAll(LPCTSTR szTableName);
	BOOL	UnlockDocument	();


	BOOL	LockTable		(SqlTable* pTable, const CString& tableName, BOOL bUseMessageBox = TRUE, LockRetriesMng* pRetriesMng = NULL);
	BOOL	UnlockTable		(SqlTable* pTable, const CString& tableName);


	CString	GetLockMessage	(SqlTable* pTable);
	SqlLockMng*	GetLockMng	(SqlConnection* pSqlConnection);
	SqlLockMng*	GetLockMng	(const CString& strDBName);

	// locks cache management
	void	EnableLocksCache		(SqlTable* pTable, const BOOL bValue = TRUE);
	void	ClearLocksCache			(SqlTable* pTable, const CString sLockContextKey = _T(""));
	BOOL	LockTableKey			(SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey = _T(""), LockRetriesMng* pRetriesMng = NULL);
	BOOL	UnlockTableKey			(SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey = _T(""));
	BOOL	IsTableKeyLocked		(SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey = _T(""));

	BOOL	UnlockAllLockContextKeys(const CString& sLockContextKey, SqlTable* pTable);

};

/////////////////////////////////////////////////////////////////////////////
//						class CTransactionContext
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CTransactionContext : public CSharedContext
{

public:
	// ho la necessitá di avere due sessioni aperte. Una per le operazioni di lettura FORWARD l'altr
// per le operazioni transazionali. Questo a causa di SqlServer per cui non é possibile avere
// nella stessa sessione di lavoro cursori FORWARD e cursori SCROLLABLE

	SqlConnection*	m_pSqlConnection;
	CTBContext*		m_pTBContext;
	SqlSession*		m_pSqlSession;	 
	BOOL			m_bTxError;				//ci sono stati errori nel corso della transazione. Il proprietario del contesto fará roolback

private:
	int m_nRefOpenCount;

public:
	CTransactionContext(SqlConnection*, CTBContext*, CBaseDocument*);
	CTransactionContext(const CTransactionContext&);
	~CTransactionContext();

public:
	SqlSession*		GetSqlSession();

public:
	//per aprire e chiudere la connessione associata al contesto
	BOOL Connect();
	BOOL Disconnect();

	BOOL IsValid();
	SqlSession* OpenNewSqlSession();

	//transaction management
	BOOL StartTransaction();
	void Commit(SqlPerformanceManager*);
	void Rollback(SqlPerformanceManager*);
	BOOL TransactionPending();	// mi dice se la session ha una transazione ancora in esecuzione 	
};



/////////////////////////////////////////////////////////////////////////////
//						class CTBContext
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CTBContext
{
	friend class SqlPerformanceManager;
	friend class CBaseDocument;

private:
	CArray<CContextObject*,	CContextObject*>*	m_pCustomObjects;
	
	WORD m_wContextStatus;
	enum ContextStatus
	{
		TRANSACTION_STATUS	= 0x0001,
		CACHING_STATUS		= 0x0002,
		DIAGNOSTIC_STATUS	= 0x0004
	};

	BOOL m_bShareTransactionContext;
	BOOL m_bShareDataCachingContext;

public:
	CBaseContext*			m_pBaseContext;
	CTransactionContext*	m_pCurrTransactionContext;
	
public:
	//gestione performance
	SqlPerformanceManager*	m_pSqlPerformanceMng;
	SqlPerformanceDlg*		m_pSqlPerformanceDlg;

public:
	CTBContext();
	CTBContext(const CTBContext&, CBaseDocument* pDocument = NULL);
	CTBContext(SqlConnection*, CBaseDocument* pDocument = NULL);
	~CTBContext();

public:
	//Diagnostic  management
	CBaseDocument*	GetDocument() const		{ return m_pBaseContext->m_pDocument; }
	CDiagnostic*	GetDiagnostic() const	{ return m_pBaseContext->m_pDiagnostic; }

	//@@OPTIMISTICLOCK Optimistic lock support
	void	EnableOptimisticLock(bool bEnable = true);

	//Pessimistic lock support
	void UnlockAll()						{ m_pBaseContext->UnlockAll(); }		

	BOOL	LockDocument	() { return m_pBaseContext->LockDocument(); }	
	BOOL	UnlockDocument	() { return m_pBaseContext->UnlockDocument(); }	

	//transaction management 
	BOOL			Connect()					const { return m_pCurrTransactionContext->Connect(); }
	BOOL			Disconnect()				const { return m_pCurrTransactionContext->Disconnect(); }

	SqlSession*		GetReadOnlySqlSession()		 { return m_pCurrTransactionContext->GetSqlSession(); }
	SqlSession*		GetUpdatableSqlSession ()	 { return m_pCurrTransactionContext->GetSqlSession(); }

	BOOL			IsValid();
	SqlConnection*	GetSqlConnection()   const	{ return m_pCurrTransactionContext->m_pSqlConnection; }
	BOOL			TransactionPending() const	{ return m_pCurrTransactionContext->TransactionPending(); }// mi dice se la session ha una transazione ancora in esecuzione 
	SqlSession*		OpenNewSqlSession()			{ return m_pCurrTransactionContext->OpenNewSqlSession(); }
	BOOL			StartTransaction()			{ return m_pCurrTransactionContext->StartTransaction(); }
	void			Commit();
	void			Rollback();

	//gestione customobjects
	void	AttachCustomObject(CContextObject* pObject);
	void	DeattachCustomObject(CContextObject* pObject);
	void	DeattachCustomObject(LPCTSTR lpszObjectName);
	CContextObject*	GetCustomObject(LPCTSTR lpszObjectName);

	//gestione performance
	void SetSqlPerformanceDlg(SqlPerformanceDlg*);
	SqlPerformanceManager* CreateSqlPerformanceMng();
	void DestroySqlPerformanceMng();

	void StartTime	(int nTime, LPCTSTR pszActionName = NULL) { if (m_pSqlPerformanceMng) m_pSqlPerformanceMng->StartTime(nTime, pszActionName); }
	void StopTime	(int nTime)								  { if (m_pSqlPerformanceMng) m_pSqlPerformanceMng->StopTime(nTime); }
	void PauseTime	() 										  { if (m_pSqlPerformanceMng) m_pSqlPerformanceMng->PauseTime(); }
	void ResumeTime () 										  { if (m_pSqlPerformanceMng) m_pSqlPerformanceMng->ResumeTime(); }

public:
	//Transfer management
	BOOL IsOwnContext() const  { return m_pBaseContext->IsOwnContext(); }

	//the development can choose the context to transfer to called document and if the context is changeable (DEFAULT)
	void StartDiagnosticContext		(BOOL bShowMsg = TRUE, BOOL bChangeable = TRUE);
	void StartTransactionContext	(BOOL bChangeable = TRUE); //the called document istanziates an owned transaction context 
	void StartDataCachingContext	(BOOL bChangeable = TRUE);

	void EndDiagnosticContext		(BOOL bCopy = FALSE, const CString& strOpeningBanner = _T("") , const CString& strClosingBanner = _T(""));
	void EndTransactionContext		()  { m_bShareTransactionContext = TRUE; } //the calling document now shares the transaction context
	void EndDataCachingContext		()  { m_bShareDataCachingContext = TRUE; }  //the calling document now shares the caching context
	
public:
	void OnStartTransaction() ;
	void OnCommitTransaction();
	void OnRollbackTransaction();

private:
	CString FormatRollbackLogMessage ();
	void	SetContextStatus(ContextStatus eStatusFlag, BOOL bChangeable);
	BOOL	IsContextStatusChangeable(ContextStatus eStatusFlag);
};

// struct 
//-----------------------------------------------------------------------------
struct SQLERRORINFO
{
	BSTR    bstrSource;
	BSTR    bstrDescription;
	DWORD   dwHelpContext;
	GUID    guid;
	BSTR    bstrHelpFile;
};

//classe base della maggior parte delle classi definite nella libreria TBOleDb

/////////////////////////////////////////////////////////////////////////////
//						class SqlObject
/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SqlObject : public CObject
{
	friend class SqlException;
	DECLARE_DYNAMIC(SqlObject);

public:
	CString			m_strError;
	CBaseContext*	m_pContext;	

protected:
	BOOL			m_bOwnContext;

protected:
	SqlObject(CBaseContext* pContext = NULL, CBaseDocument* = NULL);
	virtual ~SqlObject();

	//x la gestione del controllo della performance
	virtual	void	LoadSqlOperations	()	{}

public:
	virtual ::DBMSType GetDBMSType() const { return DBMS_UNKNOWN; }

public:
	void		ShowMessage		(LPCTSTR lpszMessage, CDiagnostic::MsgType type = CDiagnostic::Error)
								{ m_pContext->ShowMessage(lpszMessage, type); };
		
	void		AddMessage		(LPCTSTR lpszMessage, CDiagnostic::MsgType type = CDiagnostic::Error)	
								{ m_pContext->AddMessage(lpszMessage, type); };
	void		ShowMessage		(BOOL bClear = FALSE)			{ m_pContext->ShowMessage(bClear); };

    BOOL		ErrorFound		() const	{ return m_pContext->ErrorFound(); }
	CDiagnostic* GetDiagnostic	()			{ return m_pContext->GetDiagnostic(); }

	void		SetContext	(CBaseContext*);

	
	// lock management
	void	EnableLocksCache		(SqlTable* pTable, const BOOL bValue = TRUE);
	void	ClearLocksCache			(SqlTable* pTable, const CString sLockContextKey = _T(""));
	BOOL	LockTableKey			(SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey = _T(""), LockRetriesMng* pRetriesMng = NULL);
	BOOL	UnlockTableKey			(SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey = _T(""));
	BOOL	UnlockAllLockContextKeys(const CString& sLockContextKey, SqlTable* pTable);
	BOOL	IsTableKeyLocked		(SqlTable* pTable, SqlRecord* pRec, const CString& sContextKey =_T(""));
	BOOL 	IsCurrentLocked			(SqlTable* pTable);
// Private members
private:
	SqlObject(const SqlObject&);   
	void operator=(const SqlObject&); 

		
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlObject\n"); CObject::Dump(dc);}
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};


/////////////////////////////////////////////////////////////////////////////
//						class SqlException
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT SqlException : public MSqlException
{
	DECLARE_DYNAMIC(SqlException)

public:
	SqlObject* m_pSqlObj; //l'oggetto che ha generato l'eccezione

public:
	SqlException(const CString& strError);
	SqlException(MSqlException& mSqlException);

public:

	BOOL	ShowError(LPCTSTR pszError = _T(""));
};

//@@BAUZI OLD VERSIONE
///////////////////////////////////////////////////////////////////////////////
////						class SqlException
///////////////////////////////////////////////////////////////////////////////
////
////===========================================================================
//class TB_EXPORT SqlException : public CException
//{
//	DECLARE_DYNAMIC(SqlException)
//
//
//	// Attributes
//public:
//	HRESULT			m_nHResult;
//	LPUNKNOWN		m_lpUnk;
//	IID				m_iid;
//	CString			m_strError;
//	SqlObject*		m_pSqlObj;
//	DWORD			m_wNativeErrCode;
//
//	// Implementation (use AfxThrowDBException to create)
//public:
//	SqlException();
//	SqlException(LPUNKNOWN lpUnk, const IID& iid, HRESULT nHResult = S_OK, SqlObject* pSqlObj = NULL, DWORD wNativeErrCode = 0);
//	virtual ~SqlException();
//
//public:
//	void Empty();
//	BOOL ShowError(LPCTSTR pszError = _T(""));
//	void SetDeletable(const BOOL& bValue);
//
//	const BOOL	IsLostConnectionError(const int eDBMSType) const;
//	void		GetLostConnectionErrors(CWordArray& arErrors, const int eDBMSType) const;
//
//public:
//	void	GetErrorString(HRESULT hResult, CString& strError);
//	void	UpdateErrorString(const CString& strNewError, BOOL bAppend = TRUE);
//	DWORD	GetNativeFromDescription(const CString& strNewError);
//	virtual void BuildErrorString(HRESULT nHResult, LPUNKNOWN lpUnk, const IID& iid);
//	virtual BOOL GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError, _Out_opt_ PUINT pnHelpContext = NULL) const;
//	virtual BOOL GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError, _Out_opt_ PUINT pnHelpContext = NULL);
//};


// costanti stringa
BEGIN_TB_STRING_MAP(SqlErrorString)
	TB_LOCALIZED(SQL_ERROR_BUILD_QUERY, "Error in query construction on table {0-%s}.")
END_TB_STRING_MAP()

#include "endh.dex"

