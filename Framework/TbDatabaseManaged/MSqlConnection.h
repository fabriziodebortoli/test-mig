#pragma once

#include <afxhtml.h>

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>

#include <TbGeneric\GeneralObjects.h>

#include "SqlBindingObjects.h"

class SqlExceptionClient;
class SqlConnectionClient;
class SqlTransactionClient;
class SqlCommandClient;
class SqlDataReaderClient;
class SqlDataTableClient;
class SqlProcedureParameters; 


#include "beginh.dex"


//enum TB_EXPORT SqlMoveType { MoveFirst, MovePrev, MoveNext, MoveLast };
enum TB_EXPORT MoveType { E_MOVE_REFRESH, E_MOVE_FIRST, E_MOVE_PREV, E_MOVE_NEXT, E_MOVE_LAST };
enum TB_EXPORT SqlPagingResult { FistPage, MiddlePage, LastPage, SamePage, ErrorPage, NoData };


/////////////////////////////////////////////////////////////////////////////
//						class MSqlException
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT MSqlException : public CException
{
	DECLARE_DYNAMIC(MSqlException)
	
	// Attributes
public:
	CString				m_strError;	
	HRESULT				m_nHResult;
	DWORD				m_wNativeErrCode;

public:
	MSqlException(SqlExceptionClient* pSqlExceptionClient);
	MSqlException(const CString& strError);
	MSqlException(const MSqlException& mSqlException);

public:
	void UpdateErrorString(const CString& strNewError, BOOL bAppend = TRUE);

private:
	void BuildErrorString();


	//virtual BOOL GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError, _Out_opt_ PUINT pnHelpContext = NULL) const;
	//virtual BOOL GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError, _Out_opt_ PUINT pnHelpContext = NULL);
	//BOOL ShowError();
};


//===========================================================================
class TB_EXPORT MSqlConnection 
{
	friend class MSqlCommand;


public:
	SqlConnectionClient* m_pSqlConnectionClient;
	SqlTransactionClient* m_pSqlTransactionClient;

	enum SchemaInfoType { Tables, Views, Procedures };

	CString m_strCollationName;

private:
	bool m_bKeepOpen;
	CCounterElem		m_QuerySchemaTimeCounter;
	CCounterElem		m_FetchSchemaTimeCounter;
	CCounterElem		m_PrimaryKeyTimeCounter;


public:
	MSqlConnection();
	~MSqlConnection();


public:
	bool KeepOpen() const { return m_bKeepOpen; }

public:
	void Open(bool stayOpen = false);
	void Close();
	bool IsOpen() const;

	CString GetDBName();
	CString GetServerName();
	CString GetDBUserName();
	CString GetDbmsName();
	CString DbmsVersion();

	int	 GetConnectionState() const;
	void SetConnectionString(const CString& strConnectionString);

	void BeginTransaction();
	void Commit();
	void Rollback();

	CString NativeConvert(const DataObj* pDataObj, const BOOL& bUseUnicode) const;

	CString GetQuerySchemaFormattedTime() {	return m_QuerySchemaTimeCounter.GetFormattedTime();	};
	CString GetFetchSchemaFormattedTime() { return m_FetchSchemaTimeCounter.GetFormattedTime(); };
	CString GetPrimaryKeyFormattedTime()  { return m_PrimaryKeyTimeCounter.GetFormattedTime(); };
	

public:
	void LoadSchemaInfo(const CString& strSchemaType, ::CMapStringToOb* pSqlTables);

	void LoadProcedureParametersInfo(const CString& strProcedurerName, ::Array* pProcedureParams);
	void LoadColumnsInfo(const CString& strTableName, ::Array* pPhisycalColumns);
	void LoadPrimaryKey(const CString& strTableName, CStringArray* pPKColumnsName);
	void LoadUniqueFields(const CString& strTableName, CStringArray* pUniqueFields);
	void LoadForeignKeys(const CString& sFromTableName, const CString& sToTableName, BOOL bLoadAllToTables, CStringArray* pFKReader);
};


//===========================================================================
class TB_EXPORT MSqlCommand : public CObject
{
private:
	SqlCommandClient*		m_pSqlCommandClient;
	SqlDataReaderClient*	m_pSqlDataReaderClient;
	SqlDataTableClient*		m_pSqlDataTableClient;

	MSqlConnection*			m_pSqlConnection;
	
	SqlBindObjectArray*		m_pFetchedData;
	SqlBindObjectArray*		m_pParameters;

	CString					m_strCommandText;
	int						m_nCommandTimeout;
	CString					m_strTableName;

	long					m_lRecordCounts;
	bool					m_bUpdatable; //è un commmand aperto in scrittura

	//uso un DataTable per simulare un cursore scrollabile
	//se PageSize > 0 uso la paginazione
	bool					m_bUseDataTable;
	
	//gestione paging
	SqlPagingResult	m_ePagingResult;
	long		m_nPageSize; //dimensione della pagina
	long		m_nCurrentPage; //pagina corrente
	long		m_nLastPage; //numero ultima pagina
	//serve per individuare l'ultima pagina mediante la query Select count(*) as c from (m_strNoOrderByCommandText) as A 
	// deve essere prima della ORDER BY (altrimenti SQL mi da errore)
	CString		m_strNoOrderByCommandText; 


public:
	enum SqlCommandType { StoredProcedure, TableDirect, Text /*default*/ };

public:
	MSqlCommand(MSqlConnection*);
	~MSqlCommand();

private:
	void GetCurrentPage();
	SqlFetchResult MoveOnPaging(MoveType eMoveType, int lSkip = 0);

public:
	void Create(bool bScrollable = false);
	void Init();
	void Disconnect();
	void Dispose();

	void SetCommandText(CString strCmdText);
	void SetSqlCommandType(SqlCommandType sqlCmdType);
	void SetCommandTimeout(int nTimeout);
	void SetCurrentTransaction(bool setTransaction);
	void SetTableName(const CString& strTableName) { m_strTableName = strTableName; }
	
	//Gestione paging	
	void EnablePaging(int nPageSize = 25);
	void GetFirstPage();
	void GetNextPage();
	void GetPrevPage();
	void GetLastPage();
	

	CString GetCommandText() const;

	//SqlMoveType serve nel caso di paging per evitare di eseguire più volte query inutili (vedi SqlBrowser)
	void ExecutePagingCommand(MoveType eMoveType = E_MOVE_NEXT, bool bPrepared = false);

	void ExecuteCommand(bool bPrepared = false);
	void ExecuteScalar(bool bPrepared = false);
	int  ExecuteNonQuery(bool bPrepared = false);

	bool IsConnected() const;
	bool IsNull() const;
	bool HasRows() const;
	long GetRecordsAffected() const;

	SqlFetchResult Move(MoveType eMoveType = E_MOVE_NEXT, int lSkip = 0);

	//Gestione dati in fetch
	void BindColumns(SqlBindObjectArray* pFetchedData) { m_pFetchedData = pFetchedData; }
	void FixupColumns();

	//Gestione dei parametri
	void BindParameters(SqlBindObjectArray* pParamArray);
	void SetParametersValues();

	//restituisce il numero totale di record restituite da una query (usato anche dal paging per trovare l'ultima pagina)
	long GetTotalRecords();

};

#include "endh.dex"
