#include "stdafx.h" 


#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>

#include "SqlSchemaInfo.h"
#include "MSqlConnection.h"

using namespace System;
using namespace System::Data;
using namespace System::Text;
using namespace System::Data::SqlClient;



//===========================================================================
//	SqlExceptionClient
//===========================================================================
//
//---------------------------------------------------------------------------
class SqlExceptionClient
{
public:
	SqlExceptionClient(System::Data::SqlClient::SqlException^ e)
		:
		mSqlException(e)
	{		
	}
	~SqlExceptionClient()
	{
		if ((SqlException^)mSqlException != nullptr)
			delete mSqlException;
	}

public:
	gcroot<System::Data::SqlClient::SqlException^> mSqlException;

};

//===========================================================================
//	MSqlException
//===========================================================================
//
IMPLEMENT_DYNAMIC(MSqlException, CException)

//---------------------------------------------------------------------------
MSqlException::MSqlException(SqlExceptionClient* pSqlException)
	:
	m_pSqlException(pSqlException) 
{
	StringBuilder^ errorMessages = gcnew StringBuilder();

	SqlException^ ex = (SqlException^)m_pSqlException->mSqlException;
	for (int i = 0; i < ex->Errors->Count; i++)
	{

		errorMessages->Append("Index #" + i + "\n" +
			"Message: " + ex->Errors[i]->Message + "\n" +
			"LineNumber: " + ex->Errors[i]->LineNumber + "\n" +
			"Source: " + ex->Errors[i]->Source + "\n" +
			"Procedure: " + ex->Errors[i]->Procedure + "\n");
	}

	m_strError = errorMessages->ToString();
}

//---------------------------------------------------------------------------
MSqlException::MSqlException(const CString& strError)
	: 
	m_pSqlException(nullptr),
	m_strError(strError)
{	
}


//---------------------------------------------------------------------------
MSqlException::~MSqlException()
{
	if (m_pSqlException)
		delete m_pSqlException;
}

//-----------------------------------------------------------------------------
void MSqlException::BuildErrorString()
{
	if (!m_pSqlException || ((SqlException^)m_pSqlException->mSqlException == nullptr))
		return;

	
}

//===========================================================================
//	SqlConnectionClient
//===========================================================================
//
//---------------------------------------------------------------------------
class SqlConnectionClient
{
public:
	SqlConnectionClient()
	:
		mSqlConnection(nullptr)
	{
		mSqlConnection = gcnew SqlConnection();
	}

	~SqlConnectionClient()
	{
		if ((SqlConnection^)mSqlConnection != nullptr)
		{
			if (mSqlConnection->State != ConnectionState::Closed)
				mSqlConnection->Close();

			delete mSqlConnection;
		}
	}


public:
	gcroot<System::Data::SqlClient::SqlConnection^> mSqlConnection;
	
};

//---------------------------------------------------------------------------
class SqlTransactionClient
{
public:
	SqlTransactionClient()
	:
		mSqlTransaction(nullptr)
	{
	}
	
	~SqlTransactionClient()
	{
		if ((SqlTransaction^)mSqlTransaction != nullptr)
			delete mSqlTransaction;
	}

public:
	gcroot<System::Data::SqlClient::SqlTransaction^> mSqlTransaction;
};

//===========================================================================
//	MSqlConnection
//===========================================================================
//
//---------------------------------------------------------------------------
MSqlConnection::MSqlConnection()
:
	m_pSqlConnectionClient(NULL),
	m_pSqlTransactionClient(NULL)
{
	m_pSqlConnectionClient = new SqlConnectionClient();
};

//
//---------------------------------------------------------------------------
MSqlConnection::~MSqlConnection()	
{
	if (m_pSqlConnectionClient)		
		delete m_pSqlConnectionClient;	
}

//---------------------------------------------------------------------------
void MSqlConnection::Open()
{
	if (String::IsNullOrEmpty(m_pSqlConnectionClient->mSqlConnection->ConnectionString))
		return throw(gcnew Exception("Connection string empty"));

	try
	{
		m_pSqlConnectionClient->mSqlConnection->Open();
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
	}
}

//---------------------------------------------------------------------------
CString MSqlConnection::GetDBName()
{
	return (m_pSqlConnectionClient && ((SqlConnection^)m_pSqlConnectionClient->mSqlConnection) != nullptr && m_pSqlConnectionClient->mSqlConnection->State == ConnectionState::Open)
		 ? m_pSqlConnectionClient->mSqlConnection->Database
		 : _T("");

}
//---------------------------------------------------------------------------
CString MSqlConnection::GetServerName()
{
	return (m_pSqlConnectionClient && ((SqlConnection^)m_pSqlConnectionClient->mSqlConnection) != nullptr && m_pSqlConnectionClient->mSqlConnection->State == ConnectionState::Open)
		? m_pSqlConnectionClient->mSqlConnection->DataSource
		: _T("");
}

//---------------------------------------------------------------------------
CString MSqlConnection::GetDBUserName()
{
	return _T("");
	/*return (m_pSqlConnectionClient && ((SqlConnection^)m_pSqlConnectionClient->mSqlConnection) != nullptr && m_pSqlConnectionClient->mSqlConnection->State == ConnectionState::Open)
		? m_pSqlConnectionClient->mSqlConnection->Credential->UserId
		: _T("");*/
}

//---------------------------------------------------------------------------
CString MSqlConnection::GetDbmsName()
{
	return _T("Sql Server");
}

//---------------------------------------------------------------------------
CString MSqlConnection::DbmsVersion()
{
	return (m_pSqlConnectionClient && ((SqlConnection^)m_pSqlConnectionClient->mSqlConnection) != nullptr && m_pSqlConnectionClient->mSqlConnection->State == ConnectionState::Open)
		? m_pSqlConnectionClient->mSqlConnection->ServerVersion
		: _T("");
}



//---------------------------------------------------------------------------
void MSqlConnection::SetConnectionString(const CString& strConnectionString)
{
	m_pSqlConnectionClient->mSqlConnection->ConnectionString = gcnew String(strConnectionString);
}

//---------------------------------------------------------------------------
void MSqlConnection::Close()
{
	if (m_pSqlConnectionClient->mSqlConnection->State != ConnectionState::Closed)
		m_pSqlConnectionClient->mSqlConnection->Close();
}

//---------------------------------------------------------------------------
int MSqlConnection::GetConnectionState() const
{
	return(int)m_pSqlConnectionClient->mSqlConnection->State;
}


//---------------------------------------------------------------------------
void MSqlConnection::BeginTransaction()
{
	SqlTransaction^ sqlTrans = nullptr;
	
	try
	{
		sqlTrans = m_pSqlConnectionClient->mSqlConnection->BeginTransaction(IsolationLevel::ReadUncommitted);
	}
	catch (SqlException^ e)
	{
		if (m_pSqlTransactionClient)
			delete m_pSqlTransactionClient;

		throw(new MSqlException(new SqlExceptionClient(e)));
	}
	
	if (m_pSqlTransactionClient)
		delete m_pSqlTransactionClient;
	m_pSqlTransactionClient = new SqlTransactionClient();
	m_pSqlTransactionClient->mSqlTransaction = sqlTrans;
}

//---------------------------------------------------------------------------
void MSqlConnection::Commit()
{
	try
	{
		m_pSqlTransactionClient->mSqlTransaction->Commit();
	}
	catch(Exception^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}

	delete m_pSqlTransactionClient;
	m_pSqlTransactionClient = NULL;
}

//---------------------------------------------------------------------------
void MSqlConnection::Rollback()
{
	try
	{
		m_pSqlTransactionClient->mSqlTransaction->Rollback();
	}
	catch (Exception^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}

	delete m_pSqlTransactionClient;
	m_pSqlTransactionClient = NULL;
}

//-----------------------------------------------------------------------------
static const TCHAR szNativeSqlTimestamp[] = _T("{ts '%04d-%02d-%02d %02d:%02d:%02d'}");
static const TCHAR szNativeSqlDate[] = _T("{d '%04d-%02d-%02d'}");
static const TCHAR szNativeSqlTime[] = _T("{t '%02d:%02d:%02d'}");


//---------------------------------------------------------------------------
CString MSqlConnection::NativeConvert(const DataObj* pDataObj, const BOOL& bUseUnicode) const
{
	const rsize_t nLen = 75;
	TCHAR szVal[nLen];

	switch (pDataObj->GetDataType().m_wType)
	{
	case DATA_STR_TYPE:
	{
		CString str = pDataObj->Str();
		str.Replace(_T("'"), _T("''"));
		return ((bUseUnicode) ? _T("N'") : _T("'")) + str + _T("'");
	}

	case DATA_TXT_TYPE:
	{
		CString str = pDataObj->Str();
		str.Replace(_T("'"), _T("''"));
		if (bUseUnicode)
			return cwsprintf(_T("N\'%s\'"), str);

		return cwsprintf(_T("\'%s\'"), str);
	}

	case DATA_INT_TYPE:
	case DATA_DBL_TYPE:
	case DATA_MON_TYPE:
	case DATA_QTA_TYPE:
	case DATA_PERC_TYPE:
		return pDataObj->Str();

	case DATA_LNG_TYPE:
		return pDataObj->Str(-1, pDataObj->IsATime() ? 0 : -1);

	case DATA_ENUM_TYPE:
	{
		DataEnum* pDataEnum = (DataEnum*)pDataObj;
		_stprintf_s(szVal, nLen, _T("%ld"), pDataEnum->GetValue());
		return szVal;
	}

	case DATA_DATE_TYPE:
	{
		DataDate* pDataDate = (DataDate*)pDataObj;

		// Uso il formato ISO a
		if (pDataDate->IsFullDate())
			// {ts 'aaaa-mm-gg hh:mm:ss'}
			_stprintf_s
			(
				szVal, nLen, szNativeSqlTimestamp,
				pDataDate->Year(), pDataDate->Month(), pDataDate->Day(),
				pDataDate->Hour(), pDataDate->Minute(), pDataDate->Second()
			);
		else
		{
			if (pDataDate->IsATime())
				// {t 'hh:mm:ss'}
				_stprintf_s
				(
					szVal, nLen, szNativeSqlTime,
					pDataDate->Hour(), pDataDate->Minute(), pDataDate->Second()
				);
			else
			{
				// {d 'aaaa-mm-gg'}
				_stprintf_s
				(
					szVal, nLen, szNativeSqlDate,
					pDataDate->Year(), pDataDate->Month(), pDataDate->Day()
				);
			}
		}

		return szVal;
	}

	case DATA_BOOL_TYPE:
	{
		DataBool& aDataBool = (DataBool&)*pDataObj;
		return aDataBool ? _T("'1'") : _T("'0'");
	}

	case DATA_GUID_TYPE:
	{
		DataGuid& aDataGuid = (DataGuid&)*pDataObj;
		CString str = aDataGuid.Str();
		str.Replace(_T("'"), _T("''"));
		return _T("'") + str + _T("'");
	}

	case DATA_BLOB_TYPE:
		break;
	}

	ASSERT(FALSE);
	return _T("BadDataType");
}

//---------------------------------------------------------------------------
void MSqlConnection::LoadSchemaInfo(const CString& strSchemaType, ::Array* pSqlTables)
{
	
	if (m_pSqlConnectionClient->mSqlConnection->State == ConnectionState::Closed)
		return;
	
	SqlTablesItem* pTableItem = NULL;
	bool isProcedure = String::Compare(gcnew String(strSchemaType), gcnew String("Procedures"), true) == 0;
	bool isTable = String::Compare(gcnew String(strSchemaType), gcnew String("Tables"), true) == 0;

	DataTable^ dataTable = m_pSqlConnectionClient->mSqlConnection->GetSchema(gcnew String(strSchemaType));
	for each(DataRow^ row in dataTable->Rows)
	{
		pTableItem = new SqlTablesItem();
		if (isProcedure)
		{
			String^ procName = row["ROUTINE_NAME"]->ToString();
			if (!String::IsNullOrEmpty(procName))
			{
				int pos = procName->IndexOf(gcnew String(";"));
				pTableItem->m_strName = (pos >= 0) ? procName->Substring(0, pos) : procName;
				pTableItem->m_strType = _T("PROCEDURE");
				pTableItem->m_strQualifier = row["SPECIFIC_CATALOG"]->ToString();
				pTableItem->m_strOwner = row["SPECIFIC_SCHEMA"]->ToString();


			}
		}
		else
		{
			//faccio lo skip delle tabelle di sistema e quelle con nome vuoto
			String^ schemaName = row["TABLE_SCHEMA"]->ToString();
			String^ tableName = row["TABLE_NAME"]->ToString();

			if (
				(String::Compare(schemaName, gcnew String("INFORMATION_SCHEMA"), true) == 0) ||
				(String::Compare(tableName, gcnew String("dtproperties"), true) == 0) ||
				(String::Compare(tableName, gcnew String("sysalternates"), true) == 0) ||
				(String::Compare(tableName, gcnew String("syssegments"), true) == 0) ||
				(String::Compare(tableName, gcnew String("sysconstraints"), true) == 0) ||
				(String::Compare(tableName, gcnew String(" "), true) == 0)
				)
				continue;

			pTableItem->m_strName = row["TABLE_NAME"]->ToString();
			pTableItem->m_strType = (isTable) ? _T("TABLE") : _T("VIEW");
			pTableItem->m_strQualifier = row["TABLE_CATALOG"]->ToString();
			pTableItem->m_strOwner = row["TABLE_SCHEMA"]->ToString();

		}
		pSqlTables->Add(pTableItem);
	}		
}

//@BAUZITODO da togliere: per rendere ancora compatibile il codice con OLEDB ma presto andrà tolto
SWORD GetOLEDBType(CString sqlType)
{
	if (sqlType == "BigInt")
		return DBTYPE_I8;

	if (sqlType == "Binary")
		return DBTYPE_BYTES;

	if (sqlType == "Bit")
		return DBTYPE_BOOL;

	if (sqlType == "Char")
		return DBTYPE_STR;

	if (sqlType == "DateTime")
		return DBTYPE_DBTIMESTAMP;

	if (sqlType == "Decimal")
		return DBTYPE_DECIMAL;
	if (sqlType == "Float")
		return DBTYPE_R8;

	if (sqlType == "Image")
		return DBTYPE_BYTES;

	if (sqlType == "Int")
		return DBTYPE_I4;

	if (sqlType == "Money")
		return DBTYPE_CY;


	if (sqlType == "NChar")
		return DBTYPE_WSTR;
	if (sqlType == "NText")
		return DBTYPE_WSTR;
	if (sqlType == "NVarChar")
		return DBTYPE_WSTR;
	if (sqlType == "Real")
		return DBTYPE_R4;
	if (sqlType == "UniqueIdentifier")
		return DBTYPE_GUID;
	if (sqlType == "SmallDateTime")
		return DBTYPE_DBTIMESTAMP;
	if (sqlType == "SmallInt")
		return DBTYPE_I2;
	if (sqlType == "SmallMoney")
		return DBTYPE_CY;
	if (sqlType == "Text")
		return DBTYPE_STR;
	if (sqlType == "Timestamp")
		return DBTYPE_DBTIMESTAMP;
	if (sqlType == "TinyInt")
		return DBTYPE_UI1;
	if (sqlType == "VarBinary")
		return DBTYPE_BYTES;
	if (sqlType == "VarChar")
		return DBTYPE_STR;
	if (sqlType == "Variant")
		return DBTYPE_VARIANT;

	if (sqlType == "Xml")
		return DBTYPE_I8;
	
	if (sqlType == "Udt")
		return DBTYPE_UDT;
	
	if (sqlType == "Structured")
		return DBTYPE_I8;
	//???????????????????
	if (sqlType == "Date")
		return DBTYPE_DBTIMESTAMP;
	if (sqlType == "Time")
		return DBTYPE_DBTIMESTAMP;
	if (sqlType == "DateTime2")
		return DBTYPE_DBTIMESTAMP;
	if (sqlType == "DateTimeOffset")
		return DBTYPE_DBTIMESTAMP;

	return DBTYPE_WSTR;

};

//---------------------------------------------------------------------------
void MSqlConnection::LoadColumnsInfo(const CString& strTableName, ::Array* pPhisycalColumns)
{	
	SqlCommand^ command = nullptr;
	SqlDataReader^ reader = nullptr;
	ConnectionState oldConnState = m_pSqlConnectionClient->mSqlConnection->State;
	if (oldConnState == ConnectionState::Closed)
		Open();
	
	
	String^ commandText = "SELECT TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,COLUMN_NAME,ORDINAL_POSITION,COLUMN_DEFAULT,IS_NULLABLE,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH,NUMERIC_PRECISION,NUMERIC_SCALE,COLLATION_NAME,";
	commandText += "KEYPOSITION = (SELECT ORDINAL_POSITION from INFORMATION_SCHEMA.KEY_COLUMN_USAGE K, INFORMATION_SCHEMA.TABLE_CONSTRAINTS T WHERE C.COLUMN_NAME = K.COLUMN_NAME AND C.TABLE_NAME = K.TABLE_NAME AND K.TABLE_NAME = T.TABLE_NAME AND T.CONSTRAINT_TYPE = \'PRIMARY KEY\' AND T.CONSTRAINT_NAME = K.CONSTRAINT_NAME)";
	commandText += " FROM INFORMATION_SCHEMA.COLUMNS C";
	commandText += " WHERE C.TABLE_NAME = N\'" + gcnew String(strTableName) + "\'";
	commandText += " ORDER BY ORDINAL_POSITION";
	
	SqlColumnInfoObject* pColumnInfo = NULL;
	try
	{
		command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
		reader = command->ExecuteReader();
		Object^ value;
		while (reader->Read())
		{
			pColumnInfo = new SqlColumnInfoObject();
			pColumnInfo->m_bLoadedFromDB = TRUE;
			pColumnInfo->m_strTableCatalog = reader["TABLE_CATALOG"]->ToString();
			pColumnInfo->m_strTableSchema = reader["TABLE_SCHEMA"]->ToString();
			pColumnInfo->m_strTableName = reader["TABLE_NAME"]->ToString();
			pColumnInfo->m_strColumnName = reader["COLUMN_NAME"]->ToString();
			pColumnInfo->m_strSqlDataType = reader["DATA_TYPE"]->ToString();
			pColumnInfo->m_nSqlDataType = GetOLEDBType(pColumnInfo->m_strSqlDataType);
			
			value = reader["NUMERIC_PRECISION"];
			pColumnInfo->m_lPrecision = (value == System::DBNull::Value) ? 0 : (Byte)value;
			
			value = reader["CHARACTER_MAXIMUM_LENGTH"];
			pColumnInfo->m_lLength = (value == System::DBNull::Value) ? 0 : (Int32)value;
			
			value = (reader["NUMERIC_SCALE"]);
			pColumnInfo->m_nScale = (value == System::DBNull::Value) ? 0 : (Int32)value;
			
			pColumnInfo->m_bNullable = (reader["IS_NULLABLE"]->ToString() == "YES");

			value = (reader["COLLATION_NAME"]);
			pColumnInfo->m_strCollatioName = (value == System::DBNull::Value) ? String::Empty : value->ToString();
			
			value = (reader["KEYPOSITION"]);
			pColumnInfo->m_bSpecial = (value != System::DBNull::Value);

			pPhisycalColumns->Add(pColumnInfo);
		}
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}
	finally
	{
		if (reader != nullptr && !reader->IsClosed)
		{
			reader->Close();
			delete reader;
		}
		
		if (command != nullptr)
			delete command;

		if (oldConnState == ConnectionState::Closed)
			Close();
	}
}

//---------------------------------------------------------------------------
void MSqlConnection::LoadProcedureParametersInfo(const CString& strProcedureName, SqlProcedureParameters* pProcedureParams)
{
	SqlCommand^ command = nullptr;
	SqlDataReader^ reader = nullptr;
	ConnectionState oldConnState = m_pSqlConnectionClient->mSqlConnection->State;
	if (oldConnState == ConnectionState::Closed)
		Open();


	String^ commandText = "SELECT SPECIFIC_NAME, ORDINAL_POSITION, PARAMETER_MODE, PARAMETER_NAME,DATA_TYPE";
	commandText += " FROM INFORMATION_SCHEMA.PARAMETERS";
	commandText += " WHERE SPECIFIC_NAMEE = N\'" + gcnew String(strProcedureName) + "\'";
	commandText += " ORDER BY ORDINAL_POSITION";

	try
	{
		command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
		reader = command->ExecuteReader();
		SqlProcedureParamInfo* pParamInfo = NULL;
		while (reader->Read())
		{
			pParamInfo = new SqlProcedureParamInfo();

			pParamInfo->m_strParamName =  reader["SPECIFIC_NAME"]->ToString();
			Int16 paraPosition = (Int16)reader["ORDINAL_POSITION"];
			if (paraPosition == 0)
				pParamInfo->m_nType = DBPARAMTYPE_RETURNVALUE;
			else
			{
				pParamInfo->m_nType = DBPARAMTYPE_INPUT;
				String^ paramDirection = reader["PARAMETER_MODE"]->ToString();
				if (paramDirection = "IN")
					pParamInfo->m_nType = DBPARAMTYPE_INPUT;
				else
					if (paramDirection = "OUT")
						pParamInfo->m_nType = DBPARAMTYPE_OUTPUT;
					else
						if (paramDirection = "INOUT ")
							pParamInfo->m_nType = DBPARAMTYPE_INPUTOUTPUT;
			}
			pProcedureParams->Add(pParamInfo);
		}
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}
	finally
	{
		if (reader != nullptr && !reader->IsClosed)
		{
			reader->Close();
			delete reader;
		}

		if (command != nullptr)
			delete command;

		if (oldConnState == ConnectionState::Closed)
			Close();
	}
}

//---------------------------------------------------------------------------
void MSqlConnection::LoadForeignKeys(const CString& sPrimaryTableName, const CString& sForeignTableName, BOOL bLoadAllToTables, CStringArray* pFKReader)
{
	SqlCommand^ command = gcnew SqlCommand();
	SqlDataReader^ reader = nullptr;
	ConnectionState oldConnState = m_pSqlConnectionClient->mSqlConnection->State;
	if (oldConnState == ConnectionState::Closed)
		Open();

	try
	{
		String^ commandText = "SELECT obj.name AS FKName, tab1.name AS FKTableName, col1.name AS FKColumnName, tab2.name AS PKTableName, col2.name AS PKColumnName";
		commandText += " FROM sys.foreign_key_columns fkc INNER JOIN sys.objects obj ON obj.object_id = fkc.constraint_object_id INNER JOIN sys.tables tab1 ON tab1.object_id = fkc.parent_object_id";
		commandText += " INNER JOIN sys.schemas sch	ON tab1.schema_id = sch.schema_id INNER JOIN sys.columns col1 ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id INNER JOIN sys.tables tab2";
		commandText += " ON tab2.object_id = fkc.referenced_object_id INNER JOIN sys.columns col2 ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id";
		if (!bLoadAllToTables && (!sPrimaryTableName.IsEmpty() || !sForeignTableName.IsEmpty()))
		{
			commandText += " WHERE ";
			if (!sPrimaryTableName.IsEmpty())
			{
				commandText += " tab2.name = @PKTablename";
				command->Parameters->AddWithValue("@PKTablename", gcnew String(sPrimaryTableName));
			}
			if (!sForeignTableName.IsEmpty())
			{
				commandText += " tab1.name = @FKTablename";
				command->Parameters->AddWithValue("@FKTablename", gcnew String(sForeignTableName));
			}
		}
		
		command->Connection = m_pSqlConnectionClient->mSqlConnection;
		command->CommandText = commandText;

		reader = command->ExecuteReader();

		while (reader->Read())
		{
									
			CString str = CString(reader["FKTableName"]->ToString()) + _T(".") + CString(reader["FKColumnName"]->ToString()) + ';';
			str += CString(reader["PKTableName"]->ToString()) + _T(".") + CString(reader["PKColumnName"]->ToString());

			pFKReader->Add(str);
			
		}
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}
	finally
	{
		if (reader != nullptr && !reader->IsClosed)
		{
			reader->Close();
			delete reader;
		}

		if (command != nullptr)
			delete command;

		if (oldConnState == ConnectionState::Closed)
			Close();
	}
}


//---------------------------------------------------------------------------
void FetchSingleColumn(Object^ value, SqlBindObject* pColumn)
{
	DataObj* pDataObj = pColumn->m_pDataObj;
	DataObj* pOldDataObj = pColumn->m_pOldDataObj;
	try
	{
		if (value == System::DBNull::Value)
		{
			pDataObj->Clear();
			return;
		}

		switch (pDataObj->GetDataType())
		{
			case DATA_INT_TYPE:
				((DataInt*)pDataObj)->Assign((Int16)value);
				break;

			case DATA_LNG_TYPE:
				((DataLng*)pDataObj)->Assign((Int32)value); break;

			case DATA_STR_TYPE:
				*(DataStr*)pDataObj = (String^)value; break;

			case DATA_BOOL_TYPE:
				pDataObj->Clear(); 
				(*(DataBool*)pDataObj) = ((String^)value == "1"); //->ToString() == "1");
				//break;
				//((DataBool*)pDataObj)->Assign(CString(value->ToString())); break;

			case DATA_ENUM_TYPE:
			{
				int nEnum = (Int32)value;
				((DataEnum*)pDataObj)->Assign((DWORD)nEnum); break;
			}

			case DATA_DBL_TYPE:
			case DATA_QTA_TYPE:
			case DATA_PERC_TYPE:
			case DATA_MON_TYPE:
				((DataDbl*)pDataObj)->Assign((double)value); break;

			case DATA_DATE_TYPE:
			{
				//m_pSqlDataReaderClient->mSqlDataReader->GetDateTime(i);
				//DataDate aDataTime(((DateTime)value).Day, ((DateTime)value).Month, ((DateTime)value).Year, ((DateTime)value).Hour, ((DateTime)value).Minute, ((DateTime)value).Second, ((DateTime)value).Millisecond);
				DataDate aDataTime(((DateTime)value).Day, ((DateTime)value).Month, ((DateTime)value).Year, ((DateTime)value).Hour, ((DateTime)value).Minute, ((DateTime)value).Second);
				*pDataObj = aDataTime;
				break;
			}

			case DATA_GUID_TYPE:
			{
				Guid^ g = (Guid^)value;//mSqlDataReader->GetGuid(i);
				array<Byte>^ guidData = g->ToByteArray();
				pin_ptr<Byte> data = &(guidData[0]);
				*(DataGuid*)pDataObj = *(GUID*)data;
				break;
			}
			case DATA_TXT_TYPE:
			*(DataText*)pDataObj = (String^)value; break;
		}
		//assegno il valore estratto nell'old. Serve solo per la parte di update
		//@@TODO
		if (pOldDataObj)
			*pOldDataObj = *pDataObj;
		
	}
	catch (Exception^ ex)
	{
		throw(ex);
	}
}



//===========================================================================
//	SqlDataReaderClient
//===========================================================================
//
class SqlDataReaderClient
{
public:
	gcroot<System::Data::SqlClient::SqlDataReader^> mSqlDataReader;
	SqlBindObjectArray* m_pColumnsArray;

public:
	SqlDataReaderClient(SqlBindObjectArray* pColumnsArray);
	~SqlDataReaderClient();

public:
	void Close();
	SqlResult Read();
	void FixupColumns();
};

//---------------------------------------------------------------------------
SqlDataReaderClient::SqlDataReaderClient(SqlBindObjectArray* pColumnsArray)
	:
	mSqlDataReader(nullptr),
	m_pColumnsArray(pColumnsArray)
{		
}

//---------------------------------------------------------------------------
SqlDataReaderClient::~SqlDataReaderClient()
{
	Close();
}

//---------------------------------------------------------------------------
void SqlDataReaderClient::Close()
{
	if ((SqlDataReader^)mSqlDataReader != nullptr && !mSqlDataReader->IsClosed)
	{
		mSqlDataReader->Close();
		delete mSqlDataReader;
	}
}


//---------------------------------------------------------------------------
SqlResult SqlDataReaderClient::Read()
{
	SqlResult result = ResOk;
	if ( !((SqlDataReader^)mSqlDataReader) || mSqlDataReader->IsClosed || !m_pColumnsArray)
		return Error;
	
	try
	{
		result = mSqlDataReader->Read() ? ResOk : End;
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
	}

	return result;
}

//---------------------------------------------------------------------------
void SqlDataReaderClient::FixupColumns()
{
	try
	{
		for (int i = 0; i < mSqlDataReader->FieldCount; i++)
		{
			Object^ value = mSqlDataReader->GetValue(i);
			FetchSingleColumn(value, (SqlBindObject*)m_pColumnsArray->GetAt(i));
		}			
	}
	catch (SqlException^ sqlE)
	{
		throw(sqlE);
	}
	catch (Exception^ e)
	{
		throw(e);
	}
}



//===========================================================================
//	SqlCommandClient
//===========================================================================
//
class SqlCommandClient
{
public:
	gcroot<System::Data::SqlClient::SqlCommand^> mSqlCommand;

public:
	SqlCommandClient();
	~SqlCommandClient();

public:

	void AddParam(const CString& paramName, DataObj* pDataObj, SqlParamType eParamType);
	void SetParamValue(const CString& paramName, DataObj* pDataObj);
	void SetParamValue(int nPos, DataObj* pDataObj);
	void SetParam(SqlParameter^ sqlParam, DataObj* pDataObj);
	void RemoveParam(int nPos);
	void FetchOutputParameters(SqlBindObjectArray* pParamArray);
};

//---------------------------------------------------------------------------
SqlCommandClient::SqlCommandClient()
	:
	mSqlCommand(nullptr)
{
	mSqlCommand = gcnew SqlCommand();
}

//---------------------------------------------------------------------------
SqlCommandClient::~SqlCommandClient()
{
	if ((SqlCommand^)mSqlCommand != nullptr)
		delete mSqlCommand;
}


//---------------------------------------------------------------------------
void SqlCommandClient::AddParam(const CString& paramName, DataObj* pDataObj, SqlParamType eParamType)
{
	ParameterDirection eDirection = ParameterDirection::InputOutput;
	switch(eParamType)
	{
		case SqlParamType::Input:
			eDirection = ParameterDirection::Input; break;
		case SqlParamType::Output:
			eDirection = ParameterDirection::Output; break;
		case SqlParamType::InputOutput:
			eDirection = ParameterDirection::InputOutput; break;
		case SqlParamType::ReturnValue:
			eDirection = ParameterDirection::ReturnValue; break;

	}
	SqlParameter^ sqlParam = nullptr;
	String^ name = gcnew String(paramName);
	try
	{
		switch (pDataObj->GetDataType())
		{
		case DATA_INT_TYPE:
			sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::SmallInt); break;

		case DATA_LNG_TYPE:
			sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Int); break;

		case DATA_STR_TYPE:
			sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::VarChar, pDataObj->GetColumnLen()); break;

		case DATA_BOOL_TYPE:
			sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Char, 1); break;

		case DATA_ENUM_TYPE:
			sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Int); break;

		case DATA_DBL_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
		case DATA_MON_TYPE:
			sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Float);	break;

		case DATA_DATE_TYPE:
			sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::DateTime);	break;

		case DATA_GUID_TYPE:
			sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::UniqueIdentifier); break;

		case DATA_TXT_TYPE:
			sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Text); break;

		default:
			ASSERT(FALSE); break;
		}
		if (sqlParam != nullptr)
			sqlParam->Direction = eDirection;
	}		
	catch (Exception^ e)
	{
		TRACE(CString(e->ToString()));
		ASSERT(FALSE);
	}
}

//---------------------------------------------------------------------------
void SqlCommandClient::SetParam(SqlParameter^ sqlParam, DataObj* pDataObj)
{
	switch (pDataObj->GetDataType())
	{
	case DATA_INT_TYPE:
		sqlParam->Value = ((DataInt*)pDataObj)->GetSoapValue(); break;

	case DATA_LNG_TYPE:
		sqlParam->Value = ((DataLng*)pDataObj)->GetSoapValue();	break;

	case DATA_STR_TYPE:
		sqlParam->Value = gcnew String(((DataStr*)pDataObj)->GetString()); break;

	case DATA_BOOL_TYPE:
		sqlParam->Value = (*((DataBool*)pDataObj) == TRUE) ? "1" : "0"; break;

	case DATA_ENUM_TYPE:
		sqlParam->Value = ((DataEnum*)pDataObj)->GetSoapValue(); break;

	case DATA_DBL_TYPE:
	case DATA_QTA_TYPE:
	case DATA_PERC_TYPE:
	case DATA_MON_TYPE:
		sqlParam->Value = ((DataDbl*)pDataObj)->GetSoapValue();	break;

	case DATA_DATE_TYPE:
	{
		//DataDate* pDate = (DataDate*)pDataObj;		
		//m_pSqlCommandClient->mSqlCommand->Parameters->AddWithValue(name, gcnew DateTime(pDate->Year(), pDate->Month(), pDate->Day(), pDate->Hour(), pDate->Minute(), pDate->Second()));

		const DBTIMESTAMP&	dBTimeStamp = ((DataDate*)pDataObj)->GetDateTime();
		sqlParam->Value = gcnew DateTime(dBTimeStamp.year, dBTimeStamp.month, dBTimeStamp.day, dBTimeStamp.hour, dBTimeStamp.minute, dBTimeStamp.second, dBTimeStamp.fraction);
		break;
	}

	case DATA_GUID_TYPE:
	{
		sqlParam->Value = *reinterpret_cast<Guid *>(const_cast<GUID *>(&((DataGuid*)pDataObj)->GetGUID())); break;
	}

	case DATA_TXT_TYPE:
		sqlParam->Value = gcnew String(((DataText*)pDataObj)->ToString()); break;

	}
}

//---------------------------------------------------------------------------
void SqlCommandClient::SetParamValue(const CString& paramName, DataObj* pDataObj)
{
	String^ name = gcnew String(paramName);
	SqlParameter^ sqlParam = mSqlCommand->Parameters[name];
	if (sqlParam != nullptr)
		SetParam(sqlParam, pDataObj);
}


//---------------------------------------------------------------------------
void SqlCommandClient::SetParamValue(int nPos, DataObj* pDataObj)
{
	SqlParameter^ sqlParam = mSqlCommand->Parameters[nPos];
	if (sqlParam != nullptr)
		SetParam(sqlParam, pDataObj);
}


//---------------------------------------------------------------------------
void SqlCommandClient::RemoveParam(int nPos)
{
	mSqlCommand->Parameters->RemoveAt(nPos); //delete dei parametri della setclause poichè sono cambiati altri valori rispetto all'aggiornamento precedente		
}

//---------------------------------------------------------------------------
void SqlCommandClient::FetchOutputParameters(SqlBindObjectArray* pParamArray)
{
	if (!mSqlCommand->Parameters)
		return;

	for (int i = 0; i < mSqlCommand->Parameters->Count; i++)
	{
		SqlParameter^ sqlParam = mSqlCommand->Parameters[i];
		if (sqlParam && (sqlParam->Direction == ParameterDirection::ReturnValue || sqlParam->Direction == ParameterDirection::InputOutput || sqlParam->Direction == ParameterDirection::Output))
		{
			Object^ value = sqlParam->Value;
			FetchSingleColumn(value, (SqlBindObject*)pParamArray->GetAt(i));
		}
	}
}



//===========================================================================
//	SqlDataTableClient
//===========================================================================
//
class SqlDataTableClient
{
public:
	gcroot<System::Data::DataTable^> mSqlDataTable;
	SqlBindObjectArray* m_pColumnsArray;
	long m_nDataTableSeek;

public:
	SqlDataTableClient(SqlBindObjectArray* pColumnsArray);
	~SqlDataTableClient();

public:
	SqlResult Move(SqlMoveType eMoveType);
	void FixupColumns();
};



//---------------------------------------------------------------------------
SqlDataTableClient::SqlDataTableClient(SqlBindObjectArray* pColumnsArray)
	:
	mSqlDataTable(nullptr),		
	m_nDataTableSeek(-1),
	m_pColumnsArray(pColumnsArray)
{
	mSqlDataTable = gcnew DataTable();
}

//---------------------------------------------------------------------------
SqlDataTableClient::~SqlDataTableClient()
{
	if ((DataTable^)mSqlDataTable != nullptr)
		delete mSqlDataTable;
}

//---------------------------------------------------------------------------
SqlResult SqlDataTableClient::Move(SqlMoveType eMoveType)
{
	SqlResult result = ResOk;

	switch (eMoveType)
	{
		case MoveFirst:
			m_nDataTableSeek = 0;
			break;
		case MovePrev:
		{
			if (m_nDataTableSeek == 0)
				result = Begin;
			else
				m_nDataTableSeek--;
			break;
		}
		case MoveNext:
		{
			if (m_nDataTableSeek == mSqlDataTable->Rows->Count - 1)
				result = End;
			else
				m_nDataTableSeek++;
			break;
		}
		case MoveLast:
			m_nDataTableSeek = mSqlDataTable->Rows->Count - 1;
			break;
	}	

	return result;
}

//---------------------------------------------------------------------------
void SqlDataTableClient::FixupColumns()
{
	//Fetch Current Row
	DataRow^  currentRow = mSqlDataTable->Rows[m_nDataTableSeek];
	if (!currentRow)
		m_pColumnsArray->RemoveAll();
	else
		try
		{
			for (int i = 0; i < m_pColumnsArray->GetCount(); i++)
			{
				Object^ value = currentRow->ItemArray->GetValue(i);
				FetchSingleColumn(value, (SqlBindObject*)m_pColumnsArray->GetAt(i));
			}
		}
		catch (SqlException^ sqlE)
		{			
			throw(sqlE);
		}
		catch (Exception^ e)
		{
			throw(e);
		}
}



//===========================================================================
//	MSqlCommand
//===========================================================================
//
//---------------------------------------------------------------------------
MSqlCommand::MSqlCommand(MSqlConnection* pSqlConnection)
	:
	m_pSqlCommandClient(NULL),
	m_pSqlConnection(pSqlConnection),
	m_pSqlDataReaderClient(NULL),
	m_pFetchedData(NULL),
	m_pUpdatableCmd(NULL),	
	m_pSqlDataTableClient(NULL),
	m_pFetchMandatoryColCmd(NULL),
	m_bUpdatable(false),
	m_bUseDataTable(false),
	m_bIsOpen(false),
	m_nCommandTimeout(30)
{
}

//
//---------------------------------------------------------------------------
MSqlCommand::~MSqlCommand()
{
	if (m_pUpdatableCmd)
		delete m_pUpdatableCmd;

	if (m_pSqlCommandClient)
		delete m_pSqlCommandClient;

	if (m_pSqlDataReaderClient)
		delete m_pSqlDataReaderClient;

	if (m_pFetchMandatoryColCmd)
		delete m_pFetchMandatoryColCmd;
}

//---------------------------------------------------------------------------
void MSqlCommand::Open(bool bUpdate, bool bScrollable)
{
	if (m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}

	m_bUpdatable = bUpdate;
	m_pSqlCommandClient = new SqlCommandClient();
	m_pSqlCommandClient->mSqlCommand->Connection = m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection;
	m_bUseDataTable = bScrollable;//(bUpdate || bScrollable); 
	m_bIsOpen = true;	
}

//---------------------------------------------------------------------------
void MSqlCommand::Close()
{
	if (m_bUseDataTable)
	{
		if (m_pSqlDataTableClient)
		{
			delete m_pSqlDataTableClient;
			m_pSqlDataTableClient = NULL;
		}
	}
	else
		if (m_pSqlDataReaderClient)
		{
			delete m_pSqlDataReaderClient;
			m_pSqlDataReaderClient = NULL;
		}


	m_bIsOpen = false;
}

//---------------------------------------------------------------------------
bool MSqlCommand::IsOpen()
{
	return m_bIsOpen;
}


//---------------------------------------------------------------------------
void MSqlCommand::SetCommandText(CString strCmdText)
{
	m_pSqlCommandClient->mSqlCommand->CommandText = gcnew String (strCmdText);
}

//---------------------------------------------------------------------------
CString MSqlCommand::GetCommandText() const
{
	return m_pSqlCommandClient->mSqlCommand->CommandText;
}

//---------------------------------------------------------------------------
void MSqlCommand::SetSqlCommandType(SqlCommandType sqlCmdType)
{
	if (!m_pSqlCommandClient)
		return;
		
	switch (sqlCmdType)
	{
	case SqlCommandType::StoredProcedure:
		m_pSqlCommandClient->mSqlCommand->CommandType = System::Data::CommandType::StoredProcedure;
		break;

	case SqlCommandType::TableDirect:
		m_pSqlCommandClient->mSqlCommand->CommandType = System::Data::CommandType::TableDirect;
		break;

	default:
		m_pSqlCommandClient->mSqlCommand->CommandType = System::Data::CommandType::Text;
		break;
	}
}

//---------------------------------------------------------------------------
void MSqlCommand::SetCommandTimeout(int nTimeout)
{
	m_pSqlCommandClient->mSqlCommand->CommandTimeout = nTimeout;
}

//---------------------------------------------------------------------------
void MSqlCommand::SetCurrentTransaction()
{
	m_pSqlCommandClient->mSqlCommand->Transaction = m_pSqlConnection->m_pSqlTransactionClient->mSqlTransaction;
}

//---------------------------------------------------------------------------
void MSqlCommand::Prepare()
{
	if (m_pSqlCommandClient)
		m_pSqlCommandClient->mSqlCommand->Prepare();
}
//---------------------------------------------------------------------------
void MSqlCommand::ExecuteCommand()
{
	if (!m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}
	try
	{
		if (m_bUseDataTable)
		{
			if (m_pSqlDataTableClient)
				delete m_pSqlDataTableClient;

			m_pSqlDataTableClient = new SqlDataTableClient(m_pFetchedData);
			//@@TODO utilizzare quella con maxrec e startrec
			SqlDataAdapter^ dataAdapter = gcnew SqlDataAdapter(m_pSqlCommandClient->mSqlCommand);
			dataAdapter->Fill(m_pSqlDataTableClient->mSqlDataTable);
			delete dataAdapter;
			return;
		}

		if (m_pSqlDataReaderClient)
			delete m_pSqlDataReaderClient;

		m_pSqlDataReaderClient = new SqlDataReaderClient(m_pFetchedData);
		m_pSqlDataReaderClient->mSqlDataReader = m_pSqlCommandClient->mSqlCommand->ExecuteReader();
	}
	catch (SqlException^ e)
	{
		if (m_pSqlDataTableClient)
			delete m_pSqlDataTableClient;

		if (m_pSqlDataReaderClient)
			delete m_pSqlDataReaderClient;

		throw(new MSqlException(new SqlExceptionClient(e)));		
	}
}

//---------------------------------------------------------------------------
bool MSqlCommand::HasRows()
{
	if (m_bUseDataTable)
		return  m_pSqlDataTableClient && m_pSqlDataTableClient->mSqlDataTable->Rows->Count;
	else
		return  m_pSqlDataReaderClient && m_pSqlDataReaderClient->mSqlDataReader->HasRows;
}


//---------------------------------------------------------------------------
SqlResult MSqlCommand::Move(SqlMoveType eMoveType)
{
	SqlResult eResult;
	if (m_pSqlDataReaderClient)
		eResult = m_pSqlDataReaderClient->Read();
	else
		if (m_pSqlDataTableClient)
			eResult = m_pSqlDataTableClient->Move(eMoveType);		
	
	return eResult;
}

//---------------------------------------------------------------------------
void MSqlCommand::FixupColumns()
{
	try
	{
		if (m_pSqlDataReaderClient)
			m_pSqlDataReaderClient->FixupColumns();
		else
			if (m_pSqlDataTableClient)
				m_pSqlDataTableClient->FixupColumns();
	}
	catch (SqlException^ sqlE)
	{
		throw(new MSqlException(new SqlExceptionClient(sqlE)));
	}
	catch (Exception^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}	
}



//---------------------------------------------------------------------------
void MSqlCommand::ExecuteScalar()
{
	if (!m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}

	try
	{
		Object^ obj = m_pSqlCommandClient->mSqlCommand->ExecuteScalar();
		if (m_pFetchedData && m_pFetchedData->GetCount() >= 1)
			FetchSingleColumn(obj, (SqlBindObject*)m_pFetchedData->GetAt(0));
	}
	catch (SqlException^ sqlE)
	{
		throw(new MSqlException(new SqlExceptionClient(sqlE)));
	}
	catch (Exception^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}
}

//---------------------------------------------------------------------------
void MSqlCommand::ExecuteNonQuery()
{
	if (!m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}

	try
	{
		m_pSqlCommandClient->mSqlCommand->ExecuteNonQuery();
	}
	catch (SqlException^ sqlE)
	{
		throw(new MSqlException(new SqlExceptionClient(sqlE)));
	}
	catch (Exception^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}
}

//---------------------------------------------------------------------------
void MSqlCommand::ExecuteCall()
{
	if (!m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}

	try
	{
		m_pSqlCommandClient->mSqlCommand->CommandType = CommandType::StoredProcedure;
		m_pSqlCommandClient->mSqlCommand->ExecuteNonQuery();
		m_pSqlCommandClient->FetchOutputParameters(m_pParameters);
	}
	catch (SqlException^ sqlE)
	{
		throw(new MSqlException(new SqlExceptionClient(sqlE)));
	}
	catch (Exception^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}
}


//---------------------------------------------------------------------------
void MSqlCommand::BindParameters(SqlBindObjectArray* pParamArray)
{
	if (!pParamArray)
		return;
	
	if (!m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}

	m_pParameters = pParamArray;
	SqlBindObject* pBindElem = NULL;
	for (int i = 0; i < m_pParameters->GetSize(); i++)
	{
		pBindElem = (SqlBindObject*)m_pParameters->GetAt(i);
		if (pBindElem)
			m_pSqlCommandClient->AddParam(pBindElem->m_strBindName, pBindElem->m_pDataObj, pBindElem->m_eParamType);
	}
}


//---------------------------------------------------------------------------
void MSqlCommand::SetParametersValues()
{
	if (m_pParameters == NULL)
		return;


	if (!m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}

	SqlBindObject* pBindElem = NULL;
	for (int i = 0; i < m_pParameters->GetSize(); i++)
	{
		pBindElem = (SqlBindObject*)m_pParameters->GetAt(i);
		if (pBindElem)
			m_pSqlCommandClient->SetParamValue(i, pBindElem->m_pDataObj);
	}
}


//-----------------------------------------------------------------------------
//void MSqlCommand::FixupMandatoryColumns(SqlBindObjectArray* pMandatoryCols)
//{
	//CString paramName;
	//CString strWhereKeyCols;
	//if (!m_pFetchMandatoryColCmd)
	//{
	//	m_pFetchMandatoryColCmd = new MSqlCommand(m_pSqlConnection);
	//	m_pFetchMandatoryColCmd->Open();
	//	m_pFetchMandatoryColCmd->m_pSqlCommandClient->mSqlCommand->Transaction = m_pSqlConnection->m_pSqlTransactionClient->mSqlTransaction;
	//	m_pFetchMandatoryColCmd->BindColumns(pMandatoryCols);	
	//	
	//	CString strSelect;
	//	for (int i = 0; i < pMandatoryCols->GetSize(); i++)
	//	{
	//		SqlBindingElem* pCol = (SqlBindingElem*)pMandatoryCols->GetAt(i);
	//		DataObj* pDataObj = pCol->m_pDataObj;
	//		if (!strSelect.IsEmpty())
	//			strSelect += _T(", ");
	//		strSelect += pCol->m_strBindName;

	//		if (pCol->m_bIsKey)
	//		{
	//			paramName = cwsprintf(_T("@PK%d"), i);
	//			if (!strWhereKeyCols.IsEmpty())
	//					strWhereKeyCols += _T(" AND ");
	//				strWhereKeyCols += cwsprintf(_T("%s = %s"), pCol->m_strBindName, paramName);				

	//			m_pFetchMandatoryColCmd->AddParam(paramName, pDataObj);
	//			m_pFetchMandatoryColCmd->SetParamValue(paramName, pDataObj);
	//		}
	//	}
	//	m_pFetchMandatoryColCmd->SetCommandText(cwsprintf(_T("SELECT %s FROM %s WHERE %s"), strSelect, m_strTableName, strWhereKeyCols));
	//}
	//else
	//	for (int i = 0; i < pMandatoryCols->GetSize(); i++)
	//	{
	//		SqlBindingElem* pCol = (SqlBindingElem*)pMandatoryCols->GetAt(i);
	//		DataObj* pDataObj = pCol->m_pDataObj;
	//		if (pCol->m_bIsKey)
	//		{
	//			paramName = cwsprintf(_T("@PK%d"), i);
	//			//per leggere il campo TBModified aggiornato dal db mediante funzione getdate()
	//			m_pFetchMandatoryColCmd->SetParamValue(paramName, pDataObj);
	//		}
	//	}


	//m_pFetchMandatoryColCmd->ExecuteCommand();
	//m_pFetchMandatoryColCmd->Move();
	//m_pFetchMandatoryColCmd->Close();
//}








