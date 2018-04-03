
#pragma once

#include <TbGeneric\dataobj.h>

#include "sqlobject.h"
#include "sqlrec.h"
#include "sqlcatalog.h"
#include "oledbmng.h"
#include "sqlconnect.h"
#include "performanceanalizer.h"

#include <TbDatabaseManaged\SqlBindingObjects.h>
#include <TbDatabaseManaged\MSqlConnection.h>

//includere alla fine degli include del .H
#include "beginh.dex"

///Improvement #4620: per un problema di performance si è deciso di porre la sensitivity == FALSE sia per la query
// di browse sia per la query personalizzata dall'utente attraverso il Barquery (anche radar).
// Ma per ovviare ai problemi relativi alla cancellazione dei record (vedi anomalia: 18257) è stato necessario creare un parametro che 
// mi permetta in alcuni casi di impostare una "sensitivity ridotta" per riuscire a marcare i record cancellati dagli altri 
// cursori (DBPROP_REMOVEDELETED = TRUE)
//Per alcuni casi (vedi View, GroupBy e funzioni di SUM, COUNT, MAX etc etc) la visibility non deve assolutamente essere impostata

/////////////////////////////////////////////////////////////////////////////
//						class 
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
// Classes declared in this file
	// Array
		class SqlTableArray;
		class SqlParamArray;
		class SqlColumnArray;
		class SqlTableInfo;
		class DataObjArray;
		class SqlBindingElemArray;
		class SqlTableStruct;

	//CException
//		class SqlException;    // abnormal return value
		class SqlException;


	//CObject
		class SqlObject;
				class SqlTable;   	// Data result sets
				class SqlSession;
		class DataObj;
		class SqlRecord;   	// Single Data Row
		class SqlBindingElem;
	// CDocument
		class CBaseDocument;

//-----------------------------------------------------------------------------
TB_EXPORT void AFXAPI TraceError			(LPCTSTR szTrace);


// Max display length in chars of timestamp (date & time) value
//-----------------------------------------------------------------------------
#define TIMESTAMP_PRECISION 23


// current record status value
#define SQL_CURRENT_RECORD_UNDEFINED (-2)
#define SQL_CURRENT_RECORD_BOF (-1)

//updating result
#define UPDATE_FAILED		0   // update fallita
#define UPDATE_SUCCESS		1	// update eseguita con successo
#define UPDATE_NO_DATA		2	// updaate non eseguita. Il record non era stato modificato




/////////////////////////////////////////////////////////////////////////////
//						class SqlRowSet
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT SqlRowSet : public SqlObject
{
	friend class SqlTable;
	friend class SqlAccessor;
	friend class SqlColumnArray;
	friend class SqlParamArray;
	friend class SqlPerformanceManager;
	friend class SqlCommandPool;
	
	DECLARE_DYNAMIC(SqlRowSet)

public:
	SqlSession*					m_pSqlSession;		
	SqlSession*					m_pOldSqlSession;
	SqlConnection*				m_pSqlConnection;

	SqlParamArray*				m_pParamArray;		// usato per bind il dei parametri
    SqlColumnArray*				m_pColumnArray;		// usato per bind delle colonne

	CString						m_strSQL;    		// SQL completa
	CString						m_strOldSQL;   	 	// SQL completa precedente
	CStringArray				m_strCurrentTables;	// current tables of the select (see SqlBindingElem::ManageUnicodeInDB method)
	CString						m_strSelect;   	 	// Select-Update-Delete clause   
	CString						m_strFilter;   	 	// Where clause

	//per la parte di update/delete con il lock ottimistico
	CString						m_strFilterKeys;
	CString						m_strFilterOldValues;

protected:
	MSqlCommand*	m_pRowSet;
	BOOL m_bScrollable;
	BOOL m_bUpdatable;
	BOOL m_bInit;
	BOOL m_bErrorFound;	// se ci sono degli errori in fase di costruzione della query
						// se TRUE non devo eseguire la query
	CString m_strErrorFound;	// descrizione dell'errore
							
	CursorType m_eCursorType;
	//paging
	int			m_nPageSize;

private:
	bool m_bRemoved;
	

public:
	SqlRowSet();
	SqlRowSet(SqlSession*, CBaseDocument* = NULL);
	virtual ~SqlRowSet();

private:
	void Initialize();

public: 
	virtual	void Open
	(
		BOOL bUpdatable = FALSE,   // é un rowset su cui verranno effettuate operazioni di insert/update/delete
		BOOL bScrollable = FALSE	// se TRUE un rowset con un cursore scrollabile altrimenti forward-only
	);
	virtual void Open(BOOL bUpdatable, CursorType eCursorType);

	virtual void Connect(); //serve solo per connettere il SqlCommand alla connessione
	virtual void Disconnect(); //serve solo per disconnettere il SqlCommand dalla connessione

	virtual void Close();
	virtual void SetSqlSession(SqlSession*);
	virtual void Invalidate() {}
	virtual void FixupColumns();


	virtual ::DBMSType GetDBMSType () const;

	//// x la gestione della performance
	void	MakeDBTimeOperation(TimeOperation eTimeOper, int nOperation)
			{ 
					if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeDBTimeOperation (eTimeOper,	nOperation, this);
			}

	void	MakeProcTimeOperation(TimeOperation eTimeOper, int nOperation)
			{ 
					if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeProcTimeOperation (eTimeOper, nOperation);
			}
	
	BOOL IsOpen() const;
	BOOL IsConnected() const;

	void ClearParams();
	void ClearColumns();
	BOOL ExistParam			(const CString& strParamName) const;
	// Params management helper functions
	void	AddParam		(const CString& strParamName, const DataObj& aDataObj, SqlParamType eParamType = Input, int nInsertPos = -1);
	void	AddParam		(const CString& strParamName, const DataType& nDataType, const int nLen, SqlParamType eParamType = Input, const SWORD nSqlDataType = DBTYPE_EMPTY, const CString& strColumnName = _T(""), int nInsertPos = -1);
	//per aggiungere un parametro su un campo DataText è necessario indicare anche il nome della colonna su cui viene effettuato il filtro
	void	AddDataTextParam(const CString& strParamName, const DataObj& aDataObj, const CString& strColumnName, SqlParamType eParamType = Input);

		//per i parametri di una stored procedure
	void	AddProcParam	(SqlProcedureParamInfo*	pParamInfo, DataObj* pDataObj);
	void	AddProcParam	(const CString& strParamName, short nOleDbParamType, DataObj* pDataObj);
	//nOleDbParamType: DBPARAMTYPE_INPUT, DBPARAMTYPE_INPUTOUTPUT, DBPARAMTYPE_OUTPUT, DBPARAMTYPE_RETURNVALUE

	void	SetParamValue	(const CString& strParamName, const DataObj& aDataObj);
	void	SetParamLike	(const CString& strParamName, const DataObj& aDataObj);

	// Routines utili per ricostruire i DataObj a partire dai parametri
	void		GetParamValue	(const CString& strParamName, DataObj* pDataObj) const;
	void		GetParamValue	(int nPos, DataObj* pDataObj) const;
	DataType	GetParamType	(int nPos) const;

	CursorType GetCursorType() const { return m_eCursorType; }

	long		GetRowSetCount();
	

protected:
	void	ClearRowSet();
	BOOL	IsNull() const;

	void SubstituteQuestionMarks(CString& strSQL); //sostituisce i vecchi ? con @nomeParam

	//gestione esecuzione query
	BOOL	BindParameters		();
	BOOL	BindColumns			();
	void	InitColumns			();
	BOOL	FixupParameters		();

	//lasciato per compatibilità (serve per far compilare ERP nella versione Desktop)
	HRESULT Prepare();

	//lasciato per compatibilità con il passato (serve per ERP Desktop)
	long	OpenRowSet() { return ExecuteNonQuery(); }
	void	ExecutePagingCommand(MoveType eMoveType = E_MOVE_FIRST);

public:
	//esecuzione del comando
	void	ExecuteCommand();
	int		ExecuteNonQuery();
	void	ExecuteScalar();
	void	ExecuteQuery(LPCTSTR lpszSQL);
	// esecuzione query e script
	void	ExecuteScript(LPCTSTR lpszFileName);

	//Paging
	void	GetFirstPage();
	void	GetNextPage();
	void	GetPrevPage();
	void	GetLastPage();
	
	BOOL HasRows() const { return m_pRowSet->HasRows(); }


	BOOL HasError() { return m_bErrorFound; }
	CString	GetError() { return m_strErrorFound; }

	//Paging
	void EnablePaging(int nPageSize);

	SqlConnection*	GetConnection() const { ASSERT_VALID(m_pSqlConnection); return	m_pSqlConnection; }
};

/////////////////////////////////////////////////////////////////////////////
//						class SqlTable
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT SqlTable : public SqlRowSet, public IDisposingSourceImpl
{
	friend class SqlException;
	friend class SqlLockMng;
	friend class SqlRecord;
	friend class DataTableRule;
	friend class TblRuleData;

	DECLARE_DYNAMIC(SqlTable);

public:
    enum SelectKeywordType	{ DISTINCT, TOP, TOP_PERCENT, TOP_WITH_TIES};

public:
	SqlRowSet*	m_pUpdateRowSet; // Utilizzato per INSERT/DELETE/UPDATE statement per la gestione del KeyeUpdate

	CString	m_strSelectKeyword;	// SELECT [ ALL | DISTINCT ][ TOP n [ PERCENT ] [ WITH TIES ] ] 
	CString m_strFrom;
	CString m_strOuterJoin;
	CString	m_strTableName;	 	// From clause        
	CString	m_strAliasName;	 	// AS clause (di tablella)
	CString	m_strGroupBy;      	// Group By Clause
	CString	m_strHaving;      	// Group By Clause
	CString	m_strSort;      	// Order By Clause
    
	BOOL m_bKeyChanged; 	// serve x l'updatekey e l'auditing 
							// (cosí si evita di controllare nuovamente se é cambiata la chiave primaria)
	SymTable* m_pSymTable;

protected:
	enum EditMode { noMode, edit, addnew };
	UINT m_nEditMode;

	SqlRecord*		m_pRecord;			// record corrente	

	SqlRecordProcedure* m_pParamsRecord; // per i paramentri di una Stored Procedure
	SqlTableArray*	m_pTableArray;		// usato in caso di From da + tabelle
	CStringArray*	m_pKeysArray;		// utilizzato se il programmatore vuole indicare le colonne da utilizzare
										// nella keyed update/delete mediante il metodo AddUpdateKey
	DataObjArray*	m_pExtraColumns;	// utilizzato per aggiungere nella Select i campi presenti nell'orderby nel caso in cui nella select sia presente la DISTINCT

	BOOL m_bFirstQuery;

	BOOL m_bBOF;
	BOOL m_bEOF;
	BOOL m_bEOFSeen;
	BOOL m_bDeleted;
	long m_lRecordCount;
	long m_lCurrentRecord;
	BOOL m_bDBTMasterQuery;
	BOOL m_bForceQuery;		 // serve per eseguire in ogni caso la query su tabella. Vedi SetForceQuery di TableReader
	BOOL m_bForceAutocommit; //se devo utilizzare x forza l'autocommit anche se la connessione
							 // prevede l'utilizzo delle transazioni
	SqlFetchResult m_sqlFetchResult;
	SqlRecord*	m_pOldRecord; // serve x l'updatekey, auditing e lock ottimistico
	
	BOOL	   m_bOnlyOneRecordExpected; //vuol dire che la query mi estrae un solo record (vedi oggetti come DBTMaster, DBTSlave, TableReader e TableUpdater)
										//serve per ottimizzare il processo di disconessione automantico
private:
	CString					m_strParamsList;

	SqlForeignKeysReader	m_FKReader;
	CCallbackHandler		m_Handler;
	BOOL					m_bSkipContextBagParameters;

	//@@ROWSECURITY
	BOOL					m_bSkipRowSecurity;
	DataLng* 				m_pRSFilterWorkerID; // worker utilizzato per i filtri della RowSecurity. Se il puntatore è NULL viene utilizzato il worker connesso
	DataLng*				m_pRSSelectWorkerID;
	BOOL					m_bSelectGrantInformation; //viene chiesto alla tabella di estrarre anche le informazioni di grant per ogni record estratto. Le informazioni di grant sono relative
													// o al worker connesso oppure al worker m_pRSWorkerID

	//BOOL					m_bOptimisticLock; //@@OPTIMISTICLOCK

public:
	// costruttori
	SqlTable();
	SqlTable(SqlSession*);
	SqlTable(SqlRecord*, SqlSession* = NULL, CBaseDocument* = NULL);
	//stored procedure e function
	SqlTable(SqlRecordProcedure* pRecordParams, SqlRecord* pSqlRecord, SqlSession* pSqlSession = NULL, CBaseDocument* pDocument = NULL);
	SqlTable(SqlRecordProcedure* pRecordParams, SqlSession* pSqlSession = NULL, CBaseDocument* pDocument = NULL);
	virtual ~SqlTable();

private:
	void InitDataMember		();
	// reset query operation
	void ClearTables		(); 
	void ClearKeys			();
	void ClearExtraColunms	();
	void ClearSQL			();

	// No SqlSetPos Support. 
	//Usa "DELETE/UPDATE ... WHERE KEYSEG1 = ?, ... KEYSEGn = ?"
	BOOL KeyedDelete			();
	int	 KeyedUpdate			(BOOL bForceTBModified = FALSE);
	int	 BuildSetClause		(CString& strSelect, BOOL bCheckOldValues = TRUE);
	int	 BindSetParameters	(BOOL bPrepare);
	void BindKeyParameters	(BOOL bPrepare, int& nParam);
	
	int  GetKeySegmentCount();	
	
	void Initialize		();
	void CheckRecord	();
	
	BOOL OpenUpdateRowSet	(const CString& strSelect);
	void FreeUpdateRowSet	();

	// Used for DBMS_ORACLE
	void StripRecordSpaces	();

	// controllo l'Order By 
	BOOL CheckOrderBy();
	BOOL ParseOrderBy(CStringArray& items);

	// x le colonne di tipo autoncrementale
	// utlizzato nel caso della gestione dei campi bindati come autoincremental (vedi property SQLSERVER) nel
	// caso di inserimento di record in connessione ORACLE
	void GetNextAutoincrementValue();
	void FixupAutoIncColumns(); 

	void ExecuteStoredProcedure();

public:
	void ValorizeContextBagParameters	();

private: //gestione contextBag 
	void AddContextBagFilters			();
	void AddContextBagFilters			(SqlRecord*);
	void AddContextBagFilters			(const RecordArray&);	//usato da Woorm
	void AddContextBagFiltersInternal	(SqlRecord*);
	void ValorizeContextBagParameters	(SqlRecord*);
	void ValorizeContextBagParameters	(const RecordArray&);	//usato da Woorm
	void ValorizeContextBagParametersInternal	(SqlRecord*);

private: //TBROWSECURITY 
	CString GetSelectGrantString();
	void AddRowSecurityFilters();		
	void ValorizeRowSecurityParameters();

public:
	void		SetSkipRowSecurity	()						{ m_bSkipRowSecurity = TRUE; } // allow the developer to disable query restrictions based on RowSecurityLayer
	BOOL		IsSkipRowSecurity	()			const		{ return m_bSkipRowSecurity; } // allow the developer to disable query restrictions based on RowSecurityLayer	
	
	void		SetRowSecurityFilterWorker(DataLng* pRSWorkerID)	{ m_pRSFilterWorkerID = pRSWorkerID; }  // worker used from RowSecurity filter. By default is the logged worker.
	DataLng*	GetRowSecurityFilterWorker()			const		{ return m_pRSFilterWorkerID; }  
	DataLng*	GetRowSecuritySelectWorker()			const		{ return m_pRSSelectWorkerID; }  
	void		SetSelectGrantInformation(DataLng* pRSWorkerID)		{ m_bSelectGrantInformation = TRUE;  m_pRSSelectWorkerID = pRSWorkerID; }
	BOOL		IsSelectGrantInformation()		const				{ return m_bSelectGrantInformation; }
	void		SetDatabaseQuery(BOOL isDBQuery)					{ m_bFirstQuery = isDBQuery; }


	//void		EnableOptimisticLock() { m_bOptimisticLock = TRUE; } //@@OPTIMISTICLOCK
	
private:
	void					BuildParamsList			();

public:
	void			Attach				(SymTable* pSymTable) {m_pSymTable = pSymTable;}
	SqlTableArray*	GetTableArray		() const { return m_pTableArray;	}	// usato in caso di From da + tabelle
	CString			GetAllTableName		() const;	// usato in caso di From da + tabelle

	//edit mode
	BOOL	IsInEditMode() const { return m_nEditMode == edit; }
	BOOL	IsInNewMode	() const { return m_nEditMode == addnew; }
	BOOL	IsInNoMode	() const { return m_nEditMode == noMode; }

	SqlForeignKeysReader& GetForeignKeyReader () { return m_FKReader; }

	BOOL	SameQuery() const;

protected:	
	void InitRow		();
	void BindExtraColumns();

	//per le stored procedure
	void BuildCall();

public: //in line fuction
	void MoveFirstCopy	(SqlRecord* pRecord)	{ MoveCopy(E_MOVE_FIRST,pRecord); }
	void MoveLastCopy	(SqlRecord* pRecord)	{ MoveCopy(E_MOVE_LAST,	pRecord); }
	// Safe Name if Exist
	const CString&	GetTableName() const { return m_pRecord ? m_pRecord->GetTableName() : m_strTableName; }
	const CString	GetTableTitle() const { return AfxLoadDatabaseString(GetTableName(), GetTableName()); }
	// position operation	
	BOOL	IsEOF		() const	{ return m_bEOF; }
	BOOL	IsBOF		() const	{ return m_bBOF; }
	BOOL	IsDeleted	() const	{ return m_bDeleted; }

	// Status operation
	BOOL	IsUpdatable		() const { return m_bUpdatable; }
	BOOL	IsValid			() const { return m_pRecord->IsValid(); }
	BOOL	NoCurrent		() const { return IsEOF() || IsBOF() || IsDeleted() || m_lCurrentRecord == SQL_CURRENT_RECORD_UNDEFINED; }
	BOOL	IsPreQueryState	() const { return m_bFirstQuery; }

	// serve solo al DBT per la AddNew
	void	ModifyRecord	(SqlRecord* pRecord)	{ *m_pRecord = *pRecord; }
	CString	 GetPrimaryKeyDescription () const { return m_pRecord->GetPrimaryKeyDescription(); }

	void SetDBTMasterQuery(BOOL bSet = TRUE) { m_bDBTMasterQuery = bSet; m_bOnlyOneRecordExpected = TRUE; }

	// obbligo il cursore in modifica a utilizzare l'autocommit anche se sono in una
	// connessione che prevede l'utilizzo delle transazioni
	// tale istruzione deve essere utilizzata con molta ATTENZIONE									
	void SetAutocommit(BOOL bSet = TRUE) { m_bForceAutocommit = bSet; }
	BOOL IsAutocommit() { return m_bForceAutocommit; }

	void SetForceQuery(BOOL bSet);

	// allow the developer to disable query restrictions based on context to perform manual queries
	void SetSkipContextBagParameters	(BOOL bSet)		{ m_bSkipContextBagParameters = bSet; }

   	// Records seen so far or -1 if unknown
   	BOOL InvalidCounter	() const	{ return m_bEOFSeen < 0 || m_lCurrentRecord < 0; }
   	long GetRecordCount	() const	{ return m_lRecordCount; }
   	long GetCurrRecordNo() const	{ return m_lCurrentRecord + 1; }

    // permette di gestire gli alias nei nomi di tabella (SELECT ... FROM nome As alias)
	void SetAliasName 	(const CString& strAliasName) { m_strAliasName = strAliasName; }

	BOOL IsOnlyOneRecordExpected() const				 { return m_bOnlyOneRecordExpected; }
	void SetOnlyOneRecordExpected(BOOL bSet = TRUE)		 { m_bOnlyOneRecordExpected = bSet; }

public: 
	//permette di eseguire la query e nel caso di cursori scrollabili di eseguire una move differente rispetto al MoveNext
	void Query(MoveType moveType, int lSkip = 0);

	// cursor operations con copia nel record passato se trovato qualcosa
	BOOL MoveNextCopy	(SqlRecord*);
	BOOL MovePrevCopy	(SqlRecord*);
	void MoveCopy		(MoveType eTypeMove, SqlRecord* pRecord);

	// Usa sintassi nativa "INSERT e DELETE/UPDATE ... WHERE COLNAME = ? AND COLNAME = ?...."
	BOOL NativeInsert	(BOOL bTraced = TRUE); //attenzione questa non l'exception controllare il valore di ritorno
	BOOL NativeDelete	();
	int	 NativeUpdate	(BOOL bForceTBModified = FALSE);

	// Xml management
	BOOL Parse			(CXMLDocumentObject* pDoc, BOOL bWithAttributes = TRUE);
	BOOL UnParse		(CXMLDocumentObject* pDoc, BOOL bWithAttributes = TRUE, BOOL bSoapType = TRUE);
	BOOL XmlExport		(CString& strPath, BOOL bWithAttributes = TRUE, BOOL bSoapType = TRUE);
	BOOL XmlImport		(CString& strPath, BOOL bWithAttributes = TRUE);

	void GetTableInfo	(SqlTableInfoArray& arTableInfo, BOOL bClear = TRUE);

public:	
	//use to select and bind key columns	
	void BindKeyColumns	();

	// Select management helper functions
	// il paramentro serve per referenziare il nome delle colonne in caso di JOIN
	// può essere nome di tabella o alias di tabella
	void	SelectAll	();
	void	SelectAll	(SqlRecord* pRecord);
	void	SelectFromAllTable	();
	void	ClearColumns		();
	
	// SELECT [ ALL | DISTINCT ][ TOP n [ PERCENT ] [ WITH TIES ] ] 
	void    AddSelectKeyword(SelectKeywordType aType, const int& nTopValue = 0);
	
	void	Select		(DataObj* pDataObj);
	void	Select		(DataObj& aDataObj);
	void	Select		(SqlRecord* pRecord, int nIdx, int nInsertPos = -1);
	void	Select		(SqlRecord* pRecord, DataObj* pDataObj, int nInsertPos = -1);
	void	Select		(SqlRecord* pRecord, DataObj& aDataObj, int nInsertPos = -1);
	void	Select		(const CString& strColumnName, DataObj* pDataObj, int nAllocSize = 0, BOOL bAutoIncrement = FALSE, int nInsertPos = -1);
	void	Select		(const CString& strColumnName, DataObj& aDataObj, int nAllocSize = 0, BOOL bAutoIncrement = FALSE, int nInsertPos = -1);
	

	//seleziono tutto tranne i campi elencati nell'array (o tramite il nome campo oppure tramite il dataobj)
	void	SelectAllExceptFields(CStringArray* pExceptedFieldName);
	void	SelectAllExceptFields(SqlRecord* pRecord, CStringArray* pExceptedFieldName);

	void	SelectAllExceptFields(DataObjArray* pExceptedDataObj);
	void	SelectAllExceptFields(SqlRecord* pRecord, DataObjArray* pExceptedDataObj);

	void	SelectSqlFun	(LPCTSTR szFunction, DataObj* pResDataObj, int nAllocSize = 0, SqlRecord* pRecord = NULL, int nInsertPos = -1);
	void	SelectSqlFun	(LPCTSTR szFunction, DataObj& aResDataObj, int nAllocSize = 0, SqlRecord* pRecord = NULL, int nInsertPos = -1);
	void	SelectSqlFun	(LPCTSTR szFunction, DataObj* pParamDataObj, DataObj* pResDataObj, int nAllocSize = 0, SqlRecord* pRecord = NULL, int nInsertPos = -1);
 	void	SelectSqlFun	(LPCTSTR szFunction, DataObj& aParamDataObj, DataObj& aResDataObj, int nAllocSize = 0, SqlRecord* pRecord = NULL, int nInsertPos = -1);
		
	// to check if select clause is empty
	BOOL	IsSelectEmpty	() const;	

	// ORDER BY related operation
	void	AddSortColumn	(const DataObj& aDataObj, BOOL bDescending = FALSE);
	void	AddSortColumn	(const CString& strColumnName, BOOL bDescending = FALSE);
	void	AddSortColumn	(SqlRecord* pRecord, const DataObj& aDataObj, BOOL bDescending = FALSE);

	// GROUP BY related operation
	void	AddGroupByColumn	(const DataObj& aDataObj);
	void	AddGroupByColumn	(const CString& strColumnName);
	void	AddGroupByColumn	(SqlRecord* pRecord, const DataObj& aDataObj);

	// WHERE related operation
	void	AddFilterColumn	(const DataObj& aDataObj, const CString& strOperator = _T(""));
	void	AddFilterColumn (SqlRecord* pRecord, const DataObj& aDataObj, const CString& strOperator = _T(""));
	void	AddFilterColumn	(const CString& strColumnName, const CString& strOperator = _T(""));

	void	AddCompareColumn(const DataObj& aDataObj, SqlRecord* pRecord, const DataObj& aCompareDataObj, const CString& strOperator = _T(""));
	void	AddCompareColumn(SqlRecord* pRecord1, const DataObj& aCompareDataObj1, SqlRecord* pRecord2, const DataObj& aCompareDataObj2, const CString& strOperator = _T(""));
	void	AddCompareColumn(const CString& strColumnName1, CString& strColumnName2, const CString& strOperator = _T(""));
	void	AddBetweenColumn(const DataObj& aDataObj);
	void	AddBetweenColumn(SqlRecord* pRecord, const DataObj& aDataObj);
	void	AddBetweenColumn(const CString& strColumnName);

	void	AddFilterLike	(const DataObj& aDataObj);
	void	AddFilterLike	(const CString& strColumnName);
	
	void	SetFilter		(LPCTSTR, ...);
	void	SetFilter		(UINT IDfmt, ...);

	// HAVING related operation
	void	AddHavingFilterColumn	(const DataObj& aDataObj, const CString& strOperator = _T(""));
	void	AddHavingFilterColumn	(SqlRecord* pRecord, const DataObj& aDataObj, const CString& strOperator = _T(""));
	void	AddHavingFilterColumn	(const CString& strColumnName, const CString& strOperator = _T(""));

	void	AddHavingCompareColumn(const DataObj& aDataObj, SqlRecord* pRecord, const DataObj& aCompareDataObj, const CString& strOperator = _T(""));
	void	AddHavingCompareColumn(SqlRecord* pRecord1, const DataObj& aCompareDataObj1, SqlRecord* pRecord2, const DataObj& aCompareDataObj2, const CString& strOperator = _T(""));

	void	AddHavingBetweenColumn(const DataObj& aDataObj);
	void	AddHavingBetweenColumn(SqlRecord* pRecord, const DataObj& aDataObj);
	void	AddHavingBetweenColumn(const CString& strColumnName);

	void	AddHavingFilterLike	(const DataObj& aDataObj);
	void	AddHavingFilterLike	(const CString& strColumnName);
	
	void	SetHavingFilter		(LPCTSTR, ...);
	void	SetHavingFilter		(UINT IDfmt, ...);

	// FROM TABLE
	// nel caso che abbia select e subquery che coinvolgono tabelle differenti
	// dal puntatore al SqlRecord ho le info riguardanti al nome della tabella ed al suo eventuale
	// Alias
	void	FromTable		(SqlRecord*, const CString* pStrAlias = NULL);
	void	FromTable       (const CString& strTableName, const CString& strAlias, SqlRecord* = NULL);

	// come prima tabella viene usata quella associata al SqlTable
	void	LeftOuterJoin(SqlRecord* pJoinRecord, const DataObj& onDataObj1, const DataObj& onDataObj2);
	void	RightOuterJoin(SqlRecord* pJoinRecord,const DataObj& onDataObj1, const DataObj& onDataObj2);

	void	LeftOuterJoin(SqlRecord* pRecord, SqlRecord* pJoinRecord, const DataObj& onDataObj1, const DataObj& onDataObj2);
	void	RightOuterJoin(SqlRecord* pRecord, SqlRecord* pJoinRecord, const DataObj& onDataObj1, const DataObj& onDataObj2);


	LPCTSTR	GetAlias() const;
	CString GetNativeFilter() const;	//ritorna la where in formato nativo del DB collegato convertendo tutti i parametri
	BOOL	MoreDataAvailable() const; 

	// il programmatore puó dire quali colonne utilizzare per la KeyedUpdate e KeyedDelete
	// altrimenti vengono considerati i segmenti di chiave primaria
	// e'sempre riferito al SqlRecord su cui é aperto il cursore
	void AddUpdateKey(const CString& strColumnName);

	// retrieve functions
	SqlRecord*	GetRecord				()	const { return m_pRecord; }
	CString		GetColumnName 			(const DataObj* pColumnDataObj);
	CString		GetQualifiedColumnName	(const DataObj* pColumnDataObj);

	// LOCK support
	void	EnableLocksCache(const BOOL bValue = TRUE); 
	void	ClearLocksCache	(const CString sLockContextKey = _T(""));
	BOOL	IsCurrentLocked	();
	BOOL	LockCurrent		(BOOL bUseMessageBox = TRUE, LockRetriesMng* pRetriesMng = NULL);
	BOOL	LockTableKey	(SqlRecord* pRec, const CString& sContextKey = _T(""), LockRetriesMng* pRetriesMng = NULL);
	BOOL	IsTableKeyLocked (SqlRecord* pRec, const CString& sContextKey = _T(""));
	BOOL	UnlockTableKey	(SqlRecord* pRec, const CString& sContextKey = _T(""));
	BOOL	UnlockCurrent	();
	BOOL	UnlockAll		();
	CString	GetLockMessage	();

	//table lock
	BOOL	LockTable	(const CString& tableName, BOOL bUseMessageBox = TRUE, LockRetriesMng* pRetriesMng = NULL);
	BOOL	UnlockTable	(const CString& tableName);

	// for Auditing Manager
	DataObj* GetOldDataObj(const CString& strColumnName) const;
	DataObj* GetDataObj(const CString& strColumnName) const;	

	// reset query operation
	void ClearQuery		();	
	void BuildSelect	();
	void BindAllColumns	();
	CString GetBuildSelect() const;

	// gestione stored procedure
	void	Call(); 
	void	DirectCall(); //used by framework, woorm engine


public: //virtual method
	// edit buffer operations
	virtual void AddNew	(BOOL bInitRow = TRUE);
	virtual void Edit	();
	virtual int  Update	(SqlRecord* pOldRecord = NULL, BOOL bForceTBModified = FALSE); 
	virtual void Delete	(SqlRecord* pOldRecord = NULL);
	virtual	void Open
					(
						BOOL bUpdatable = FALSE,   // é un rowset su cui verranno effettuate operazioni di insert/update/delete
						BOOL bScrollable = FALSE,   // se TRUE é un rowset con un cursore scrollabile altrimenti un forward-only
						BOOL bSensitivity = TRUE  // non usato ma lasciato per compatibilità con il passato
					);

	virtual void Open(BOOL bUpdatable, CursorType eCursorType);
	//virtual void Connect(); //serve solo per connettere il SqlCommand alla connessione
	virtual void Disconnect();
	virtual void Close();
	virtual void SetSqlSession(SqlSession*);
	
	virtual BOOL IsEmpty	();
	//virtual void Invalidate	()	{ m_bInvalid = TRUE; }
	
	virtual void FixupColumns();

	// cursor operations //move with data fetch
	virtual void MoveNext	(int lSkip = 0)	{ Move(E_MOVE_NEXT, lSkip); }
	virtual void MovePrev	(int lSkip = 0)	{ Move(E_MOVE_PREV, lSkip); }
	virtual void MoveFirst	()	{ Move(E_MOVE_FIRST); }
	virtual void MoveLast	()	{ Move(E_MOVE_LAST); }
	virtual void Move		(MoveType  moveType, int lSkip = 0);

	//le funzioni di query al db eseguono se necessario la BindColumns, BindParameters e le Fixup
	//esegue query che restituisce un rowset e la prima fetch
	virtual void Query() {
		Query(E_MOVE_NEXT);
	}
	
	virtual BOOL BuildQuery();	//used by ReportingStudio - convert table-rule to named-query

	//  when you call an SQL scalar function that just returns a single number i.e. Select count(*) from TableName
	// si ha il risultato nel DataObj di cui si è fatta la select
	virtual void ScalarQuery();

	//   is used when there is no return value of any kind expected from SQL server, an example being a simple UPDATE statement.
	virtual int NonQuery();

	long GetRowSetCount(); //attenzione operazione molto onerosa soprattutto con un cursore di tipo forwardonly
	
	void ReadMandatoryColumns(); //permette di leggere le mandatory columns di m_pRecord passato nel costruttore del SqlTable
	void UpdateOnlyTBModified(); //permette di aggiornare solo i campi TBModified e TB

	//aggiunge una callback da chiamare alla distruzione del documento
	//void AddDisposingHandler (CObject* pListener, ON_DISPOSING_METHOD pHandler) { m_Handler.AddDisposingHandler(pListener, pHandler); }
	//void RemoveDisposingHandlers (CObject* pListener) { m_Handler.RemoveDisposingHandlers(pListener); }

	//----
	void SelectColumns(SqlTableStruct&);

	CString GetQuery();
	CString ToString(BOOL format/* = FALSE*/, BOOL bAddTagIn /*=FALSE*/, BOOL bAddTagCol /*=FALSE*/) const;
	CString ParamToString(SqlBindingElem* pElem, BOOL bAddTagIn) const;
	void StripBlankNearSquareBrackets();

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const;
	void AssertValid() const{ SqlObject::AssertValid(); }
#endif //_DEBUG
};

// per la gestione del where clause con + tabelle
//////////////////////////////////////////////////////////////////////////////
//							SqlTableItem Definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
class TB_EXPORT SqlTableItem : public CObject
{
	friend class SqlTableArray;
	friend class SqlTable;
	DECLARE_DYNAMIC (SqlTableItem)

public:
	CString		m_strTableName;
	CString		m_strAliasName; 
	CString		m_strOuterJoinClause;
	SqlRecord*	m_pRecord;

public:
	SqlTableItem (const CString& strTableName, const CString& strAliasName, SqlRecord* pRecord = NULL, const CString& strOuterJoinClause = _T(""));
	SqlTableItem (const SqlTableItem&);
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlTableItem\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};
//////////////////////////////////////////////////////////////////////////////
//					SqlTableArray definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlTableArray : public Array
{
	DECLARE_DYNAMIC (SqlTableArray)
	
private:
	SqlConnection* m_pSqlConnection;

public:
	SqlTableArray(SqlConnection*);
	SqlTableArray(const SqlTableArray&);

public:
	SqlTableItem* 	GetAt		(int nIdx)const	{ return (SqlTableItem*) Array::GetAt(nIdx);	}
	SqlTableItem*&	ElementAt	(int nIdx)		{ return (SqlTableItem*&) Array::ElementAt(nIdx); }
	
	SqlTableItem* 	operator[]	(int nIdx)const	{ return GetAt(nIdx);	}
	SqlTableItem*&	operator[]	(int nIdx)		{ return ElementAt(nIdx);	}
	
	const CString&	GetTableName(int nIdx) const { return GetAt(nIdx)->m_strTableName; }
	const CString&	GetAlias	(int nIdx) const { return GetAt(nIdx)->m_strAliasName; }

	int 			GetTableIndex(const CString& strTableName, const CString& strAliasName) const;
	int				GetTableIndex(const CString& strTableAliasName) const;
	SqlTableItem* 	GetTableByName(const CString& strTableName, const CString& strAliasName) const;

public:
	const SqlTableInfo*	GetTableInfo(int nIdx) const;
	const SqlTableInfo*	GetTableInfo(const CString& strTableAliasName) const;
	SqlRecord*			GetRecord(int nIdx) const { return GetAt(nIdx)->m_pRecord; }
	CString				ToString();
	// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " SqlColumnArray\n"); }
	void AssertValid() const{ Array::AssertValid(); }
#endif //_DEBUG
};

/////////////////////////////////////////////////////////////////////////////
//						class SqlTable inlines
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
inline BOOL 	SqlTable::IsCurrentLocked	()	{ return m_pContext->IsCurrentLocked(this); }
inline BOOL 	SqlTable::LockCurrent		(BOOL bUseMessageBox /* = TRUE*/, LockRetriesMng* pRetriesMng /*NULL*/)	{ return m_pContext->LockCurrent (this, bUseMessageBox, pRetriesMng); }
inline CString	SqlTable::GetLockMessage	()	{ return m_pContext->GetLockMessage	(this);}

inline BOOL 	SqlTable::UnlockCurrent		()	{ return m_pContext->UnlockCurrent	(this); }
inline BOOL 	SqlTable::UnlockAll			()	{ return m_pContext->UnlockAll		(m_strTableName); }

inline BOOL		SqlTable::LockTable			(const CString& tableName, BOOL bUseMessageBox/* = TRUE*/, LockRetriesMng* pRetriesMng /*NULL*/) { return m_pContext->LockTable(this, tableName, bUseMessageBox, pRetriesMng);}
inline BOOL		SqlTable::UnlockTable		(const CString& tableName)  { return m_pContext->UnlockTable(this, tableName);}





/////////////////////////////////////////////////////////////////////////////
//						class SqlTableStruct
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlTableStruct : public CObject
{
public:
	CString	m_strSelectKeyword;	// [ ALL | DISTINCT ][ TOP n [ PERCENT ] [ WITH TIES ] ] 
	CString m_strSelect;
	CString m_strFrom;
	CString m_strFilter;
	CString	m_strGroupBy;      	// Group By Clause
	CString	m_strHaving;      	// Having Clause
	CString	m_strSort;      	// Order By Clause

	SqlTableArray*			m_pSqlTableArray;
	SqlTableInfoArray*		m_pTableInfoArray;
	SqlRecord*				m_pRecord;

	SqlBindingElemArray*	m_pColumns;
	//CStringArray			m_arParams;

	SqlTableStruct				();
	SqlTableStruct				(const SqlTableStruct&);
	SqlTableStruct				(const SqlTable&);
	virtual ~SqlTableStruct		();

	void Assign					(const SqlTableStruct&);
	void Assign					(const SqlTable&);
};
//===========================================================================
#include "endh.dex"

