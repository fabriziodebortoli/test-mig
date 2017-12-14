#pragma once

#include <afxhtml.h>

#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>

#include "SqlBindingObjects.h"

class SqlExceptionClient;
class SqlConnectionClient;
class SqlTransactionClient;
class SqlCommandClient;
class SqlDataReaderClient;
class SqlDataTableClient;
class SqlProcedureParameters;


#include "beginh.dex"




enum TB_EXPORT SqlMoveType { MoveFirst, MovePrev, MoveNext, MoveLast };


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
	SqlExceptionClient*	m_pSqlException;

public:
	MSqlException(SqlExceptionClient* pSqlExceptionClient);
	MSqlException(const CString& strError);
	virtual ~MSqlException();

private:
	void BuildErrorString();

public:
	//virtual BOOL GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError, _Out_opt_ PUINT pnHelpContext = NULL) const;
	//virtual BOOL GetErrorMessage(_Out_z_cap_(nMaxError) LPTSTR lpszError, _In_ UINT nMaxError, _Out_opt_ PUINT pnHelpContext = NULL);
	//BOOL ShowError();
};


//===========================================================================
class TB_EXPORT MSqlConnection 
{

public:
	SqlConnectionClient* m_pSqlConnectionClient;
	SqlTransactionClient* m_pSqlTransactionClient;

	enum SchemaInfoType { Tables, Views, Procedures };

	CString m_strCollationName;

public:
	MSqlConnection();
	~MSqlConnection();

private:
	bool IsCollationCultureSensitive(const CString& collationName);

public:
	void Open();
	void Close();

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


public:
	void LoadSchemaInfo(const CString& strSchemaType, ::Array* pSqlTables);

	void LoadProcedureParametersInfo(const CString& strProcedurerName, SqlProcedureParameters* pProcedureParams);
	void LoadColumnsInfo(const CString& strTableName, ::Array* pPhisycalColumns);
	void LoadForeignKeys(const CString& sFromTableName, const CString& sToTableName, BOOL bLoadAllToTables, CStringArray* pFKReader);
};



//===========================================================================
class TB_EXPORT MSqlCommand
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

	//update mng
	SqlCommandClient*		m_pUpdatableCmd;
	MSqlCommand*			m_pFetchMandatoryColCmd;
	CString					m_strUpdatedCols; //contiene la lista delle colonne aggiornate all'update precedente (col1;col2;col3;)
	CString					m_strWhereKeyCols; //contiene la where clause con le chiavi del record
	int						m_nKeysCount;

	bool					m_bUpdatable; //è un commmand aperto in scrittura
	bool					m_bUseDataTable;

	bool					m_bIsOpen;

public:
	enum SqlCommandType { StoredProcedure, TableDirect, Text /*default*/ };

public:
	MSqlCommand(MSqlConnection*);
	~MSqlCommand();

public:
	void Open(bool bUpdate = false, bool bScrollable = false);

	void SetCommandText(CString strCmdText);
	void SetSqlCommandType(SqlCommandType sqlCmdType);
	void SetCommandTimeout(int nTimeout);
	void SetCurrentTransaction();
	void SetTableName(const CString& strTableName) { m_strTableName = strTableName; }

	CString GetCommandText() const;

	void ExecuteCommand();
	void ExecuteScalar();
	void ExecuteNonQuery();
	void ExecuteCall(); //x le Stored Procedure

	void Close();
	bool IsOpen();
	bool HasRows();

	void Prepare();
	SqlResult Move(SqlMoveType eMoveType = MoveNext);

	//Gestione dati in fetch
	void BindColumns(SqlBindObjectArray* pFetchedData) { m_pFetchedData = pFetchedData; }
	void FixupColumns();

	//Gestione dei parametri
	void BindParameters(SqlBindObjectArray* pParamArray);
	void SetParametersValues();

	//void FixupMandatoryColumns(SqlBindObjectArray* pMandatoryCols);
};

#include "endh.dex"
