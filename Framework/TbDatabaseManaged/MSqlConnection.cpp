#include "stdafx.h" 

#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\Array.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataTypesFormatters.h>

#include <TbNameSolver\Chars.h>

#include "SqlSchemaInfo.h"
#include "MSqlConnection.h"
#include "SqlLockManager.h"

using namespace System;
using namespace System::Data;
using namespace System::Text;
using namespace System::Data::SqlClient;
using namespace System::Runtime::InteropServices;

//---------------------------------------------------------------------------
CString AddSquareWhenNeeds(CString name)
{
	if (name.Find(_T(' ')) < 0)
		return name;

	if (name.IsEmpty() || name.FindOneOf(_T("[")) == 0)
		return name;

	return ::cwsprintf(L"[%s]", name);
}

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
	m_nHResult(S_OK),
	m_wNativeErrCode(0)
{
	if (pSqlException)
	{
		StringBuilder^ errorMessages = gcnew StringBuilder();

		SqlException^ ex = (SqlException^)pSqlException->mSqlException;
		m_nHResult = ex->HResult;
		m_wNativeErrCode = ex->Number;
		for (int i = 0; i < ex->Errors->Count; i++)
		{

			errorMessages->Append("Index #" + i + "\n" +
				"Message: " + ex->Errors[i]->Message + "\n" +
				"LineNumber: " + ex->Errors[i]->LineNumber + "\n" +
				"Source: " + ex->Errors[i]->Source + "\n" +
				"Procedure: " + ex->Errors[i]->Procedure + "\n" +
				"Number: " + ex->Errors[i]->Number + "\n");
		}

		m_strError = errorMessages->ToString();
	}
}
//---------------------------------------------------------------------------
MSqlException::MSqlException(const MSqlException& mSqlException)
{
	m_strError = mSqlException.m_strError;
	m_nHResult = mSqlException.m_nHResult;
	m_wNativeErrCode = mSqlException.m_wNativeErrCode;
}

//---------------------------------------------------------------------------
MSqlException::MSqlException(const CString& strError)
	:
	m_nHResult(E_FAIL),
	m_wNativeErrCode(0),
	m_strError(strError)
{	
}


//-----------------------------------------------------------------------------
void MSqlException::BuildErrorString()
{
}

// arrichisce la stringa di errore di ulteriori informazioni, ponEndOfRowSeto la nuova informazione
// dopo (default) o prima del vecchio messaggio
//-----------------------------------------------------------------------------
void MSqlException::UpdateErrorString(const CString& strNewError, BOOL bAppEndOfRowSet /*= TRUE*/)
{
	if (bAppEndOfRowSet)
		m_strError += LF_CHAR + strNewError;
	else
		m_strError = strNewError + LF_CHAR + m_strError;
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
	m_pSqlTransactionClient(NULL),
	m_bKeepOpen (false),
	m_QuerySchemaTimeCounter(0, _T("QuerySchema")),
	m_FetchSchemaTimeCounter(1, _T("FetchSchema")),
	m_PrimaryKeyTimeCounter(2, _T("PrimaryKey"))
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
void MSqlConnection::Open(bool keepOpen)
{
	if (String::IsNullOrEmpty(m_pSqlConnectionClient->mSqlConnection->ConnectionString))
		return throw(gcnew Exception("Connection string empty"));
	
	SqlCommand^ command = nullptr;
	try
	{
		m_pSqlConnectionClient->mSqlConnection->Open();
		SqlCommand^ command = gcnew SqlCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED", m_pSqlConnectionClient->mSqlConnection);
		command->ExecuteNonQuery();
		m_bKeepOpen = m_bKeepOpen || keepOpen;
		delete command;
	}
	catch (SqlException^ e)
	{
		if (command != nullptr)
			delete command;
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
CString MSqlConnection::GetConnectionString()
{
	return m_pSqlConnectionClient->mSqlConnection->ConnectionString;
}


//---------------------------------------------------------------------------
bool MSqlConnection::IsOpen() const
{
	return m_pSqlConnectionClient && ((SqlConnection^)m_pSqlConnectionClient->mSqlConnection) != nullptr && m_pSqlConnectionClient->mSqlConnection->State == ConnectionState::Open;
}

//---------------------------------------------------------------------------
void MSqlConnection::Close()
{
	if (m_pSqlConnectionClient->mSqlConnection->State != ConnectionState::Closed)
	{
		if (m_pSqlTransactionClient)
		{
			delete m_pSqlTransactionClient;
			m_pSqlTransactionClient = NULL;
		}
		m_pSqlConnectionClient->mSqlConnection->Close();
	}
	m_bKeepOpen = false;
}


//---------------------------------------------------------------------------
int MSqlConnection::GetConnectionState() const
{
	return (int)m_pSqlConnectionClient->mSqlConnection->State;
}


//---------------------------------------------------------------------------
void MSqlConnection::BeginTransaction()
{	
	SqlTransaction^ sqlTrans = nullptr;
	
	if (m_pSqlTransactionClient)
		return;

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

//@BAUZITODO da togliere: per rendere ancora compatibile il codice con OLEDB ma presto andrà tolto
SWORD GetOLEDBType(CString sqlType)
{
	if (!sqlType.CompareNoCase(_T("BigInt")))
		return DBTYPE_I8;

	if (!sqlType.CompareNoCase(_T("Binary")))
		return DBTYPE_BYTES;

	if (!sqlType.CompareNoCase(_T("Bit")))
		return DBTYPE_BOOL;

	if (!sqlType.CompareNoCase(_T("Char")))
		return DBTYPE_STR;

	if (!sqlType.CompareNoCase(_T("DateTime")))
		return DBTYPE_DBTIMESTAMP;

	if (!sqlType.CompareNoCase(_T("Decimal")))
		return DBTYPE_DECIMAL;
	if (!sqlType.CompareNoCase(_T("Float")))
		return DBTYPE_R8;

	if (!sqlType.CompareNoCase(_T("Image")))
		return DBTYPE_BYTES;

	if (!sqlType.CompareNoCase(_T("Int")))
		return DBTYPE_I4;

	if (!sqlType.CompareNoCase(_T("Money")))
		return DBTYPE_CY;


	if (!sqlType.CompareNoCase(_T("NChar")))
		return DBTYPE_WSTR;
	if (!sqlType.CompareNoCase(_T("NText")))
		return DBTYPE_WSTR;
	if (!sqlType.CompareNoCase(_T("NVarChar")))
		return DBTYPE_WSTR;
	if (!sqlType.CompareNoCase(_T("Real")))
		return DBTYPE_R4;
	if (!sqlType.CompareNoCase(_T("UniqueIdentifier")))
		return DBTYPE_GUID;

	if (!sqlType.CompareNoCase(_T("SmallDateTime")))
		return DBTYPE_DBTIMESTAMP;
	if (!sqlType.CompareNoCase(_T("SmallInt")))
		return DBTYPE_I2;
	if (!sqlType.CompareNoCase(_T("SmallMoney")))
		return DBTYPE_CY;
	if (!sqlType.CompareNoCase(_T("Text")))
		return DBTYPE_STR;
	if (!sqlType.CompareNoCase(_T("Timestamp")))
		return DBTYPE_DBTIMESTAMP;
	if (!sqlType.CompareNoCase(_T("TinyInt")))
		return DBTYPE_UI1;
	if (!sqlType.CompareNoCase(_T("VarBinary")))
		return DBTYPE_BYTES;
	if (!sqlType.CompareNoCase(_T("VarChar")))
		return DBTYPE_STR;
	if (!sqlType.CompareNoCase(_T("Variant")))
		return DBTYPE_VARIANT;

	if (!sqlType.CompareNoCase(_T("Xml")))
		return DBTYPE_I8;

	if (!sqlType.CompareNoCase(_T("Udt")))
		return DBTYPE_UDT;

	if (!sqlType.CompareNoCase(_T("Structured")))
		return DBTYPE_I8;
	//???????????????????
	if (!sqlType.CompareNoCase(_T("Date")))
		return DBTYPE_DBDATE;
	if (!sqlType.CompareNoCase(_T("Time")))
		return DBTYPE_DBTIME;


	if (!sqlType.CompareNoCase(_T("DateTime2")))
		return DBTYPE_DBTIMESTAMP;
	if (!sqlType.CompareNoCase(_T("DateTimeOffset")))
		return DBTYPE_DBTIMESTAMP;

	return DBTYPE_WSTR;

};
//---------------------------------------------------------------------------
void MSqlConnection::LoadSchemaInfo(const CString& strSchemaType, ::CMapStringToOb* pSqlTables)
{
	ConnectionState oldConnState = m_pSqlConnectionClient->mSqlConnection->State;
	if (oldConnState == ConnectionState::Closed)
		Open();
	
	SqlCommand^ command = nullptr;
	SqlDataReader^ reader = nullptr;
	SqlTablesItem* pTableItem = NULL;
	bool isProcedure = String::Compare(gcnew String(strSchemaType), gcnew String("Procedures"), true) == 0;
	String^ commandText;
	CTickTimeFormatter aTickFormatter;
	DWORD dStartTick = 0;
	DWORD dElapsedTick = 0;
	DWORD dStopTick = 0;

	if (!pSqlTables)
		pSqlTables = new CMapStringToOb();
	try
	{
		dStartTick = GetTickCount();
		if (isProcedure)
			commandText = "SELECT ROUTINE_NAME, SPECIFIC_CATALOG, SPECIFIC_SCHEMA FROM INFORMATION_SCHEMA.ROUTINES";
		else
			commandText = "SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES";

		command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
		command->CommandTimeout = 0;
		if (m_pSqlTransactionClient)
			command->Transaction = m_pSqlTransactionClient->mSqlTransaction;

	
		reader = command->ExecuteReader();

		dStopTick = GetTickCount();
		dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
		if (isProcedure) 
			TRACE1("Query Procedures Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));
		else
			TRACE1("Query Tables Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));
		
		dStartTick = GetTickCount();
		while (reader->Read())
		{
			pTableItem = new SqlTablesItem();
			if (isProcedure)
			{
				String^ procName = (String^)reader["ROUTINE_NAME"];
				if (!String::IsNullOrEmpty(procName))
				{
					int pos = procName->IndexOf(gcnew String(";"));
					pTableItem->m_strName = AddSquareWhenNeeds((pos >= 0) ? procName->Substring(0, pos) : procName);
					pTableItem->m_strType = _T("PROCEDURE");
					pTableItem->m_strQualifier = (String^)reader["SPECIFIC_CATALOG"];
					pTableItem->m_strOwner = (String^)reader["SPECIFIC_SCHEMA"];


				}
			}
			else
			{
				 
				CString strTableName = AddSquareWhenNeeds((String^)reader["TABLE_NAME"]);			
				pTableItem->m_strName = strTableName;
				pTableItem->m_strType = (String^)reader["TABLE_TYPE"]; //BASE TABLE or VIEW
				pTableItem->m_strQualifier = (String^)reader["TABLE_CATALOG"];
				pTableItem->m_strOwner = (String^)reader["TABLE_SCHEMA"];
			}

			pSqlTables->SetAt(pTableItem->m_strName, pTableItem);
			
		}
		dStopTick = GetTickCount();
		dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
		
		if (isProcedure)
			TRACE1("Fetch Procedures Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));
		else
			TRACE1("Fetch Tables Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));

	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
	}
	catch (Exception^ ex)
	{
		throw(new MSqlException(CString(ex->ToString())));
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
	}


	if (pSqlTables->GetCount() == 0)
	{
		if (oldConnState == ConnectionState::Closed)
			Close();
		return;
	}

	//carico le colonne
	try
	{
		dStartTick = GetTickCount();

		if (isProcedure)
		{
			commandText = "SELECT SPECIFIC_NAME, ORDINAL_POSITION, PARAMETER_MODE, PARAMETER_NAME,DATA_TYPE";
			commandText += " FROM INFORMATION_SCHEMA.PARAMETERS";
			commandText += " ORDER BY SPECIFIC_NAME, ORDINAL_POSITION";

			command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
			command->CommandTimeout = 0;
			reader = command->ExecuteReader();

			dStopTick = GetTickCount();
			dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
			TRACE1("Query Parameters Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));
			
			SqlProcedureParamInfo* pParamInfo = NULL;
			CString strTableName;
			SqlTablesItem* pCurrTable = NULL;

			dStartTick = GetTickCount();
			while (reader->Read())
			{
				strTableName = AddSquareWhenNeeds((String^)reader["SPECIFIC_NAME"]);
				if (!pCurrTable || pCurrTable->m_strName.CompareNoCase(strTableName) != 0)
				{
					//cambio tabella
					pCurrTable = NULL;
					pSqlTables->Lookup(strTableName, (CObject*&)pCurrTable);
				}

				if (pCurrTable)
				{
					pParamInfo = new SqlProcedureParamInfo();

					//per prima cosa aggiungo un parametro di ritorno posticcio
					pParamInfo->m_strProcName = strTableName;
					pParamInfo->m_strParamName = (String^)reader["PARAMETER_NAME"];
					pParamInfo->m_nOrdinalPosition = (Int32)reader["ORDINAL_POSITION"];				
					if (pParamInfo->m_nOrdinalPosition == 0)
					{
						pParamInfo->m_nType = DBPARAMTYPE_RETURNVALUE;
						pParamInfo->m_strParamName = _T("@RETURN_VALUE");
					}
					else
					{
						pParamInfo->m_nType = DBPARAMTYPE_INPUT;
						String^ paramDirection = (String^)reader["PARAMETER_MODE"];
						if (paramDirection == "IN")
							pParamInfo->m_nType = DBPARAMTYPE_INPUT;
						else
							if (paramDirection == "OUT")
								pParamInfo->m_nType = DBPARAMTYPE_OUTPUT;
							else
								if (paramDirection == "INOUT")
									pParamInfo->m_nType = DBPARAMTYPE_INPUTOUTPUT;
					}

					pParamInfo->m_strSqlDataType = (String^)reader["DATA_TYPE"];
					pParamInfo->m_nSqlDataType = GetOLEDBType(pParamInfo->m_strSqlDataType);

					pCurrTable->m_arProcedureParams.Add(pParamInfo);
				}
			}
			dStopTick = GetTickCount();
			dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
			TRACE1("Fetch Parameters Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));
		}
		else
		{

			dStartTick = GetTickCount();

			commandText = "SELECT TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME,COLUMN_NAME,ORDINAL_POSITION,COLUMN_DEFAULT,IS_NULLABLE,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH,NUMERIC_PRECISION,NUMERIC_SCALE,COLLATION_NAME";
			//commandText += "KEYPOSITION = (SELECT ORDINAL_POSITION from INFORMATION_SCHEMA.KEY_COLUMN_USAGE K, INFORMATION_SCHEMA.TABLE_CONSTRAINTS T WHERE C.COLUMN_NAME = K.COLUMN_NAME AND C.TABLE_NAME = K.TABLE_NAME AND K.TABLE_NAME = T.TABLE_NAME AND T.CONSTRAINT_TYPE = \'PRIMARY KEY\' AND T.CONSTRAINT_NAME = K.CONSTRAINT_NAME)";
			commandText += " FROM INFORMATION_SCHEMA.COLUMNS C";
			commandText += " ORDER BY TABLE_NAME, ORDINAL_POSITION";

			SqlColumnInfoObject* pColumnInfo = NULL;
			command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
			command->CommandTimeout = 0;
			reader = command->ExecuteReader();
			dStopTick = GetTickCount();
			dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
			TRACE1("Query Columns Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));

			Object^ value;
			CString strTableName;
			SqlTablesItem* pCurrTable = NULL;

			dStartTick = GetTickCount();
			while (reader->Read())
			{
				strTableName = AddSquareWhenNeeds((String^)reader["TABLE_NAME"]);

				if (!pCurrTable || pCurrTable->m_strName.CompareNoCase(strTableName) != 0)
				{
					//cambio tabella
					pCurrTable = NULL;
					pSqlTables->Lookup(strTableName, (CObject*&)pCurrTable);					
				}

				if (pCurrTable)
				{
					pColumnInfo = new SqlColumnInfoObject();
					pColumnInfo->m_bLoadedFromDB = TRUE;
					pColumnInfo->m_strTableCatalog = (String^)reader["TABLE_CATALOG"];
					pColumnInfo->m_strTableSchema = (String^)reader["TABLE_SCHEMA"];
					pColumnInfo->m_strTableName = strTableName;
					pColumnInfo->m_strColumnName = (String^)reader["COLUMN_NAME"];
					pColumnInfo->m_strSqlDataType = (String^)reader["DATA_TYPE"];
					pColumnInfo->m_nSqlDataType = GetOLEDBType(pColumnInfo->m_strSqlDataType);

					value = reader["NUMERIC_PRECISION"];
					pColumnInfo->m_lPrecision = (value == System::DBNull::Value) ? 0 : (Byte)value;

					value = reader["CHARACTER_MAXIMUM_LENGTH"];
					pColumnInfo->m_lLength = (value == System::DBNull::Value) ? 0 : (Int32)value;

					value = (reader["NUMERIC_SCALE"]);
					pColumnInfo->m_nScale = (value == System::DBNull::Value) ? 0 : (Int32)value;

					pColumnInfo->m_bNullable = ((String^)reader["IS_NULLABLE"] == "YES");

					value = (reader["COLLATION_NAME"]);
					pColumnInfo->m_strCollatioName = (value == System::DBNull::Value) ? String::Empty : (String^)value;

					//value = (reader["KEYPOSITION"]);
					pColumnInfo->m_bSpecial = false; // (value != System::DBNull::Value);

					pCurrTable->m_arColumnsInfo.Add(pColumnInfo);
				}
			}
			if (reader)
				reader->Close();
			if (command)
				delete command;
			//Check the primarykey columns
			dStopTick = GetTickCount();
			dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
			TRACE1("Fetch Columns Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));

			dStartTick = GetTickCount();

			commandText = "SELECT K.TABLE_NAME, K.COLUMN_NAME from INFORMATION_SCHEMA.KEY_COLUMN_USAGE K, INFORMATION_SCHEMA.TABLE_CONSTRAINTS T ";
			commandText += " WHERE K.TABLE_NAME = T.TABLE_NAME AND T.CONSTRAINT_TYPE = 'PRIMARY KEY' AND T.CONSTRAINT_NAME = K.CONSTRAINT_NAME";
			commandText += " ORDER BY K.TABLE_NAME, ORDINAL_POSITION";
			
			pColumnInfo = NULL;
			CString strColumnName;
			command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
			command->CommandTimeout = 0;
			reader = command->ExecuteReader();

			dStopTick = GetTickCount();
			dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
			TRACE1("Query PrimaryKey Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));
			while (reader->Read())
			{
				strTableName = AddSquareWhenNeeds((String^)reader["TABLE_NAME"]);
				strColumnName = AddSquareWhenNeeds((String^)reader["COLUMN_NAME"]);
				if (!pCurrTable || pCurrTable->m_strName.CompareNoCase(strTableName) != 0)
				{
					//cambio tabella
					pCurrTable = NULL;
					pSqlTables->Lookup(strTableName, (CObject*&)pCurrTable);
				}

				if (pCurrTable)
				{
					for (int i = 0; i < pCurrTable->m_arColumnsInfo.GetCount(); i++)
					{
						SqlColumnInfoObject* pCurrColInfo = (SqlColumnInfoObject*)pCurrTable->m_arColumnsInfo.GetAt(i);
						if (pCurrColInfo->m_strColumnName.CompareNoCase(strColumnName) == 0)
							pCurrColInfo->m_bSpecial = true;
					}
				}
			}
			dStopTick = GetTickCount();
			dElapsedTick = (dStopTick >= dStartTick) ? dStopTick - dStartTick : 0;
			TRACE1("Fetch PrimaryKey Elapsed Time: %s\n\r", (LPCTSTR)aTickFormatter.FormatTime(dElapsedTick));
		}
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
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
void MSqlConnection::LoadColumnsInfo(const CString& strTableName, ::Array* pPhisycalColumns)
{	
	SqlCommand^ command = nullptr;
	SqlDataReader^ reader = nullptr;
	ConnectionState oldConnState = m_pSqlConnectionClient->mSqlConnection->State;
	
	m_QuerySchemaTimeCounter.Start();		
	if (oldConnState == ConnectionState::Closed)
		Open();
	
	ASSERT(pPhisycalColumns);
	
	pPhisycalColumns->RemoveAll();	

	String^ commandText = "SELECT TABLE_CATALOG, TABLE_SCHEMA, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE";
	commandText += " FROM INFORMATION_SCHEMA.COLUMNS";
	commandText += " WHERE TABLE_NAME = @TableName";
	commandText += " ORDER BY ORDINAL_POSITION";
	
	SqlColumnInfoObject* pColumnInfo = NULL;
	try
	{
		command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
		command->Parameters->AddWithValue("@TableName", gcnew String(strTableName));
		if (m_pSqlTransactionClient)
			command->Transaction = m_pSqlTransactionClient->mSqlTransaction;
		reader = command->ExecuteReader();
		m_QuerySchemaTimeCounter.Stop();

		m_FetchSchemaTimeCounter.Start();
		Object^ value;
		while (reader->Read())
		{
			pColumnInfo = new SqlColumnInfoObject();
			pColumnInfo->m_bLoadedFromDB = TRUE;
			pColumnInfo->m_strTableCatalog = (String^)reader["TABLE_CATALOG"];
			pColumnInfo->m_strTableSchema = (String^)reader["TABLE_SCHEMA"];
			pColumnInfo->m_strTableName = strTableName;
			pColumnInfo->m_strColumnName = AddSquareWhenNeeds((String^)reader["COLUMN_NAME"]);
			pColumnInfo->m_strSqlDataType = (String^)reader["DATA_TYPE"];
			pColumnInfo->m_nSqlDataType = GetOLEDBType(pColumnInfo->m_strSqlDataType);
			
			value = reader["NUMERIC_PRECISION"];
			pColumnInfo->m_lPrecision = (value == System::DBNull::Value) ? 0 : (Byte)value;
			
			value = reader["CHARACTER_MAXIMUM_LENGTH"];
			pColumnInfo->m_lLength = (value == System::DBNull::Value) ? 0 : (Int32)value;
			
			value = (reader["NUMERIC_SCALE"]);
			pColumnInfo->m_nScale = (value == System::DBNull::Value) ? 0 : (Int32)value;
			
			/*pColumnInfo->m_bNullable = ((String^)reader["IS_NULLABLE"] == "YES");

			value = (reader["COLLATION_NAME"]);
			pColumnInfo->m_strCollatioName = (value == System::DBNull::Value) ? String::Empty : (String^)value;*/
			
			//value = (reader["KEYPOSITION"]);
			pColumnInfo->m_bSpecial = false; // (value != System::DBNull::Value);

			pPhisycalColumns->Add(pColumnInfo);
		}
		m_FetchSchemaTimeCounter.Stop();

		//Check the primarykey columns
		if (pPhisycalColumns && pPhisycalColumns->GetCount() > 0)
		{
			reader->Close();
			m_QuerySchemaTimeCounter.Start();

			commandText = "SELECT K.TABLE_NAME, K.COLUMN_NAME from INFORMATION_SCHEMA.KEY_COLUMN_USAGE K, INFORMATION_SCHEMA.TABLE_CONSTRAINTS T ";
			commandText += " WHERE K.TABLE_NAME = @TableName AND K.TABLE_NAME = T.TABLE_NAME AND T.CONSTRAINT_TYPE = 'PRIMARY KEY' AND T.CONSTRAINT_NAME = K.CONSTRAINT_NAME";
			commandText += " ORDER BY K.TABLE_NAME, ORDINAL_POSITION";

			pColumnInfo = NULL;
			CString strColumnName;
			command->CommandText = commandText;
			reader = command->ExecuteReader();
			m_QuerySchemaTimeCounter.Stop();
			m_FetchSchemaTimeCounter.Start();
			while (reader->Read())
			{
				strColumnName = AddSquareWhenNeeds((String^)reader["COLUMN_NAME"]);
				for (int i = 0; i < pPhisycalColumns->GetCount(); i++)
				{
					SqlColumnInfoObject* pCurrColInfo = (SqlColumnInfoObject*)pPhisycalColumns->GetAt(i);
					if (pCurrColInfo->m_strColumnName.CompareNoCase(strColumnName) == 0)
						pCurrColInfo->m_bSpecial = true;
				}
			}
			m_FetchSchemaTimeCounter.Stop();
		}
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
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
void MSqlConnection::LoadPrimaryKey(const CString& strTableName, CStringArray* pPKColumnsName) 
{
	SqlCommand^ command = nullptr;
	SqlDataReader^ reader = nullptr;
	ConnectionState oldConnState = m_pSqlConnectionClient->mSqlConnection->State;

	m_PrimaryKeyTimeCounter.Start();
	if (oldConnState == ConnectionState::Closed)
		Open();

	ASSERT(pPKColumnsName);

	pPKColumnsName->RemoveAll();

	String^ commandText = "SELECT K.COLUMN_NAME from INFORMATION_SCHEMA.KEY_COLUMN_USAGE K, INFORMATION_SCHEMA.TABLE_CONSTRAINTS T WHERE K.TABLE_NAME = @TableName AND K.TABLE_NAME = T.TABLE_NAME AND T.CONSTRAINT_TYPE = 'PRIMARY KEY' AND T.CONSTRAINT_NAME = K.CONSTRAINT_NAME ORDER BY K.ORDINAL_POSITION";
	try
	{
		command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
		command->Parameters->AddWithValue("@TableName", gcnew String(strTableName));
		if (m_pSqlTransactionClient)
			command->Transaction = m_pSqlTransactionClient->mSqlTransaction;
		reader = command->ExecuteReader();

		while (reader->Read())
			pPKColumnsName->Add((String^)reader["COLUMN_NAME"]);

		m_PrimaryKeyTimeCounter.Stop();
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
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
void MSqlConnection::LoadUniqueFields(const CString& strTableName, CStringArray* pUniqueFields)
{
	SqlCommand^ command = nullptr;
	SqlDataReader^ reader = nullptr;
	ConnectionState oldConnState = m_pSqlConnectionClient->mSqlConnection->State;

	m_PrimaryKeyTimeCounter.Start();
	if (oldConnState == ConnectionState::Closed)
		Open();

	ASSERT(pUniqueFields);

	pUniqueFields->RemoveAll();

	String^ commandText = "SELECT K.COLUMN_NAME from INFORMATION_SCHEMA.KEY_COLUMN_USAGE K, INFORMATION_SCHEMA.TABLE_CONSTRAINTS T WHERE K.TABLE_NAME = @TableName AND K.TABLE_NAME = T.TABLE_NAME AND T.CONSTRAINT_TYPE = 'UNIQUE' AND T.CONSTRAINT_NAME = K.CONSTRAINT_NAME ORDER BY K.ORDINAL_POSITION";
	try
	{
		command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
		command->Parameters->AddWithValue("@TableName", gcnew String(strTableName));
		if (m_pSqlTransactionClient)
			command->Transaction = m_pSqlTransactionClient->mSqlTransaction;
		reader = command->ExecuteReader();

		while (reader->Read())
			pUniqueFields->Add((String^)reader["COLUMN_NAME"]);

		m_PrimaryKeyTimeCounter.Stop();
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
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
void MSqlConnection::LoadProcedureParametersInfo(const CString& strProcedureName, ::Array* pProcedureParams)
{
	SqlCommand^ command = nullptr;
	SqlDataReader^ reader = nullptr;
	ConnectionState oldConnState = m_pSqlConnectionClient->mSqlConnection->State;
	if (oldConnState == ConnectionState::Closed)
		Open();


	String^ commandText = "SELECT SPECIFIC_NAME, ORDINAL_POSITION, PARAMETER_MODE, PARAMETER_NAME,DATA_TYPE";
	commandText += " FROM INFORMATION_SCHEMA.PARAMETERS";
	commandText += " WHERE SPECIFIC_NAME = N\'" + gcnew String(strProcedureName) + "\'";
	commandText += " ORDER BY ORDINAL_POSITION";

	try
	{
		command = gcnew SqlCommand(commandText, m_pSqlConnectionClient->mSqlConnection);
		if (m_pSqlTransactionClient)
			command->Transaction = m_pSqlTransactionClient->mSqlTransaction;
		reader = command->ExecuteReader();
		SqlProcedureParamInfo* pParamInfo = NULL;
		while (reader->Read())
		{
			pParamInfo = new SqlProcedureParamInfo();

			pParamInfo->m_strParamName = (String^)reader["SPECIFIC_NAME"];
			pParamInfo->m_strProcName = strProcedureName;
			Int16 paraPosition = (Int16)reader["ORDINAL_POSITION"];
			if (pParamInfo->m_nOrdinalPosition == 0)
			{
				pParamInfo->m_nType = DBPARAMTYPE_RETURNVALUE;
				pParamInfo->m_strParamName = _T("@RETURN_VALUE");
			}
			else
			{
				pParamInfo->m_nType = DBPARAMTYPE_INPUT;
				String^ paramDirection = (String^)reader["PARAMETER_MODE"];
				if (paramDirection == "IN")
					pParamInfo->m_nType = DBPARAMTYPE_INPUT;
				else
					if (paramDirection == "OUT")
						pParamInfo->m_nType = DBPARAMTYPE_OUTPUT;
					else
						if (paramDirection == "INOUT")
							pParamInfo->m_nType = DBPARAMTYPE_INPUTOUTPUT;
			}
			pProcedureParams->Add(pParamInfo);
		}
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
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
	CString sFromTableName = sPrimaryTableName;
	CString sToTableName = sForeignTableName;

	sFromTableName.Remove('['); sFromTableName.Remove(']');
	sToTableName.Remove('['); sToTableName.Remove(']');

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
				command->Parameters->AddWithValue("@PKTablename", gcnew String(sFromTableName));
			}
			if (!sForeignTableName.IsEmpty())
			{
				if (!sPrimaryTableName.IsEmpty())
					commandText += " AND ";
				commandText += " tab1.name = @FKTablename";
				command->Parameters->AddWithValue("@FKTablename", gcnew String(sToTableName));
			}
		}
		
		command->Connection = m_pSqlConnectionClient->mSqlConnection;
		
		if (m_pSqlTransactionClient)
			command->Transaction = m_pSqlTransactionClient->mSqlTransaction;
		command->CommandText = commandText;

		reader = command->ExecuteReader();

		while (reader->Read())
		{
									
			CString str = AddSquareWhenNeeds(((String^)reader["FKTableName"])) + _T(".") + AddSquareWhenNeeds(((String^)reader["FKColumnName"])) + ';';
			str += AddSquareWhenNeeds(((String^)reader["PKTableName"])) + _T(".") + AddSquareWhenNeeds(((String^)reader["PKColumnName"]));

			pFKReader->Add(str);
			
		}
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
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
				if (value->GetType() == Int32::typeid)
					((DataInt*)pDataObj)->Assign((Int32)value);
				else
					((DataInt*)pDataObj)->Assign((Int16)value);
				break;

			case DATA_LNG_TYPE:
				if (value->GetType() == Int32::typeid)
					((DataLng*)pDataObj)->Assign((Int32)value);
				else
				{
					if (value->GetType() == Int16::typeid)
						((DataLng*)pDataObj)->Assign((Int16)value);
					else
						if (value->GetType() == Decimal::typeid)
							((DataLng*)pDataObj)->Assign(Convert::ToInt32(value));
				}
				break;


			case DATA_STR_TYPE:
				*(DataStr*)pDataObj = (String^)value; break;

			case DATA_BOOL_TYPE:
				//pDataObj->Clear(); 
				(*(DataBool*)pDataObj) = ((String^)value == "1");	break;

			case DATA_ENUM_TYPE:
			{
				int nEnum = (Int32)value;
				((DataEnum*)pDataObj)->Assign((DWORD)nEnum); break;
			}

			case DATA_DBL_TYPE:
			case DATA_QTA_TYPE:
			case DATA_PERC_TYPE:
			case DATA_MON_TYPE:
				((DataDbl*)pDataObj)->Assign((Double)value); break;

			case DATA_DATE_TYPE:
			{
				DateTime dateTime = (DateTime)value;
				((DataDate*)pDataObj)->Assign(dateTime.Day, dateTime.Month, dateTime.Year, dateTime.Hour, dateTime.Minute, dateTime.Second);

				break;
			}

			case DATA_GUID_TYPE:
			{
				Guid^ g = (Guid^)value;
				array<Byte>^ guidData = g->ToByteArray();
				pin_ptr<Byte> data = &(guidData[0]);
				*(DataGuid*)pDataObj = *(GUID*)data;
				break;
			}
			case DATA_TXT_TYPE:
			*(DataText*)pDataObj = (String^)value; break;

			case DATA_BLOB_TYPE:
			{	
				//Byte^ byteData = value;
				array<Byte>^ byteData = ((array<Byte>^)value);
				pin_ptr<Byte> data = &byteData[0];
				((DataBlob*)pDataObj)->Assign(data, byteData->Length);
				break;
			}
			

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
	SqlFetchResult Read(int lSkip = 0);
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
SqlFetchResult SqlDataReaderClient::Read(int lSkip /*= 0*/)
{
	SqlFetchResult result = FetchOk;
	if ( !((SqlDataReader^)mSqlDataReader) || mSqlDataReader->IsClosed || !m_pColumnsArray)
		return Error;
	
	try
	{
		if (lSkip == 0)
			result = mSqlDataReader->Read() ? FetchOk : EndOfRowSet;
		else
		{
			int count = lSkip;
			while (count > 0 && result == FetchOk)
			{
				result = mSqlDataReader->Read() ? FetchOk : EndOfRowSet;
				count--;
			}
		}
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
	//void SetParamValue(const CString& paramName, DataObj* pDataObj);
	void SetParamValue(int nPos, DataObj* pDataObj);
	void SetParam(SqlParameter^ sqlParam, DataObj* pDataObj);
	void RemoveParam(int nPos);
	void RemoveAllParameters();
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
	int nLen = pDataObj->GetColumnLen();
	try
	{
		

		switch (pDataObj->GetSqlDataType())
		{
			case DBTYPE_I2:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::SmallInt, 0);	break;
			case DBTYPE_I4:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Int, 0);	break;
			case DBTYPE_R4:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Real, 0);	break;
			case DBTYPE_R8:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Float, 0);	break;
			case DBTYPE_DBTIMESTAMP:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::DateTime, 0);	break;
			case DBTYPE_DBDATE:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Date, 0);	break;
			case DBTYPE_DBTIME:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Time, 0);	break;
			case DBTYPE_DECIMAL:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Decimal, 0);	break;
			case DBTYPE_BYTES:
			{
				ASSERT(pDataObj->GetDataType() == DATA_BLOB_TYPE);
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Binary, 0); 
				break;
			}
			case DBTYPE_BOOL:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Bit, 0); break;
			case DBTYPE_STR:
			{
				if (pDataObj->GetDataType() == DATA_STR_TYPE)
				{
					ASSERT(nLen > 0);
					sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::VarChar, nLen);
				}
				else
				{
					if (pDataObj->GetDataType() == DATA_BOOL_TYPE)
						sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Char, 1);
					else
					{
						if (pDataObj->GetDataType() == DATA_TXT_TYPE)
						{
							sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::Text, -1);
						}
					}
				}
				break;
			}	
			case DBTYPE_WSTR:
			{
				if (pDataObj->GetDataType() == DATA_STR_TYPE)
				{
					ASSERT(nLen > 0);
					sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::NVarChar, nLen);
				}
				else
				{
					if (pDataObj->GetDataType() == DATA_BOOL_TYPE)
						sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::NChar, 1);
					else
					{
						if (pDataObj->GetDataType() == DATA_TXT_TYPE)
						{
							sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::NText, -1);
						}
					}
				}
				break;
			}

			case DBTYPE_GUID:
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::UniqueIdentifier, 0);	break;

			default:
			{
				ASSERT(FALSE);
				sqlParam = mSqlCommand->Parameters->Add(name, SqlDbType::NVarChar, nLen > 0 ? nLen : 0);
			}

		}
		if (sqlParam != nullptr)
			sqlParam->Direction = eDirection;
	}
	catch (SqlException^ sqle)
	{
		throw(sqle);
	}
	catch (Exception^ e)
	{
		throw(e);
	}
	
}

//---------------------------------------------------------------------------
void SqlCommandClient::SetParam(SqlParameter^ sqlParam, DataObj* pDataObj)
{
	
}

//---------------------------------------------------------------------------
void SqlCommandClient::SetParamValue(int nPos, DataObj* pDataObj)
{	
}


//---------------------------------------------------------------------------
void SqlCommandClient::RemoveParam(int nPos)
{
	mSqlCommand->Parameters->RemoveAt(nPos); //delete dei parametri della setclause poichè sono cambiati altri valori rispetto all'aggiornamento precedente		
}


//---------------------------------------------------------------------------
void SqlCommandClient::RemoveAllParameters()
{
	mSqlCommand->Parameters->Clear();
}

//---------------------------------------------------------------------------
void SqlCommandClient::FetchOutputParameters(SqlBindObjectArray* pParamArray)
{
	if (!mSqlCommand->Parameters || mSqlCommand->Parameters->Count == 0)
		return;
	try
	{
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
	SqlFetchResult Move(MoveType eMoveType, int lSkip /*= 0*/);
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
SqlFetchResult SqlDataTableClient::Move(MoveType eMoveType, int lSkip /*= 0*/)
{
	SqlFetchResult result = FetchOk;
	long nCurrentRow = m_nDataTableSeek + 1;

	switch (eMoveType)
	{
		case E_MOVE_FIRST:
		{
			m_nDataTableSeek = 0;
			break;
		}
		case E_MOVE_PREV:
		{
			if (m_nDataTableSeek == 0)
				result = BeginOfRowSet;
			else
			{
				if (lSkip == 0)
					m_nDataTableSeek--;
				else
				{
					if (nCurrentRow - lSkip >= 0)
						m_nDataTableSeek = nCurrentRow - lSkip;
					else
					{
						m_nDataTableSeek = 0;
						result = BeginOfRowSet;
					}
				}
			}
			break;
		}
		case E_MOVE_NEXT:
		{
			result = FetchOk;
			if (nCurrentRow == mSqlDataTable->Rows->Count)
				result = EndOfRowSet;
			else
			{
				if (lSkip == 0)
					m_nDataTableSeek++;
				else
				{
					if (nCurrentRow + lSkip < mSqlDataTable->Rows->Count)
						m_nDataTableSeek = nCurrentRow + lSkip;
					else
					{
						m_nDataTableSeek = mSqlDataTable->Rows->Count - 1;
						result = EndOfRowSet;
					}
				}
			}
			break;
		}
		case E_MOVE_LAST:
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
	m_pSqlCommandClient		(NULL),
	m_pSqlConnection		(pSqlConnection),
	m_pSqlDataReaderClient	(NULL),
	m_pFetchedData			(NULL),
	m_pSqlDataTableClient	(NULL),
	m_bUseDataTable			(false)
{
	Init();
}

//
//---------------------------------------------------------------------------
MSqlCommand::~MSqlCommand()
{	
	Dispose();

	if (m_pSqlCommandClient)
		delete m_pSqlCommandClient;

	if (m_pSqlDataReaderClient)
		delete m_pSqlDataReaderClient;
}


//---------------------------------------------------------------------------
void MSqlCommand::Init()
{
	m_lRecordCounts = -1;
	m_nCommandTimeout = 0;
	m_nPageSize = 0;
	m_nCurrentPage = 0;
	m_nLastPage = 0;
	m_ePagingResult = FistPage;
}

//---------------------------------------------------------------------------
void MSqlCommand::Create(bool bScrollable)
{
	if (m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}

	m_pSqlCommandClient = new SqlCommandClient();	
	m_bUseDataTable = bScrollable;	
}


//---------------------------------------------------------------------------
void MSqlCommand::Dispose()
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
	
	Init();
}

//disconnetto solo i SqlDataReader 
//---------------------------------------------------------------------------
void MSqlCommand::Disconnect()
{	
	if (m_pSqlDataReaderClient)
	{
		m_lRecordCounts = m_pSqlDataReaderClient->mSqlDataReader->RecordsAffected;
		delete m_pSqlDataReaderClient;
		m_pSqlDataReaderClient = NULL;
	}
}


//solo il SqlDataReader può essere connesso. Il DataTable è in locale
//---------------------------------------------------------------------------
bool MSqlCommand::IsConnected() const
{
	return m_pSqlDataReaderClient != NULL;
}

//---------------------------------------------------------------------------
bool MSqlCommand::IsNull() const
{
	return m_pSqlCommandClient == NULL || ((SqlCommand^)m_pSqlCommandClient->mSqlCommand == nullptr);
}


//---------------------------------------------------------------------------
void MSqlCommand::SetCommandText(CString strCmdText)
{
	m_strCommandText = strCmdText;
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
void MSqlCommand::SetCurrentTransaction(bool setTransaction)
{
	if (!setTransaction && m_pSqlCommandClient && (SqlCommand^)m_pSqlCommandClient->mSqlCommand != nullptr)
		m_pSqlCommandClient->mSqlCommand->Transaction = nullptr;
}

//---------------------------------------------------------------------------
void MSqlCommand::EnablePaging(int nPageSize)
{
	m_bUseDataTable = true;
	m_nPageSize = nPageSize;
}


//---------------------------------------------------------------------------
long  MSqlCommand::GetTotalRecords()
{
	if (m_lRecordCounts > -1)
		return m_lRecordCounts;


	SqlCommand^ pCommand = gcnew SqlCommand();
	try
	{
		String^ strCommandText = gcnew String(m_strCommandText);
		//faccio la select del solo primo campo x evitare problemi con le join
		int nPos = strCommandText->IndexOf(" FROM ", StringComparison::InvariantCultureIgnoreCase);
		if (nPos > 0)
		{
			//cerco la prima virgola
			int nFirstComma = strCommandText->IndexOf(",");
			if (nFirstComma > 0)
				//String^ fieldsToRemove = strCommandText->Substring(nFirstComma, nPos);
				strCommandText = strCommandText->Remove(nFirstComma, nPos - nFirstComma);
		}

		//devo togliere l'eventuale order by (la sintassi Select COUNT(*) FROM (...) AS CT non l'ammette)
		nPos = strCommandText->LastIndexOf("ORDER BY", StringComparison::InvariantCultureIgnoreCase) ;
		if ( nPos > 0 )
			strCommandText = strCommandText->Substring(0, nPos);
		 
		pCommand->Connection = m_pSqlCommandClient->mSqlCommand->Connection;
		pCommand->Transaction = m_pSqlCommandClient->mSqlCommand->Transaction;
		pCommand->CommandText = String::Format("SELECT COUNT(*) FROM ( {0} ) AS CT", strCommandText);
		for each (SqlParameter^ param in m_pSqlCommandClient->mSqlCommand->Parameters)
		{
			SqlParameter^ newParam = gcnew SqlParameter(param->ParameterName, param->Value);
			pCommand->Parameters->Add(newParam);
		}

		m_lRecordCounts = (long)pCommand->ExecuteScalar();
		return m_lRecordCounts;
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
	}
}

//---------------------------------------------------------------------------
void MSqlCommand::GetCurrentPage()
{
	int nRowsNumb =  (m_nCurrentPage > 0) ? (m_nCurrentPage - 1) * m_nPageSize : 0;
	String^ offsetClause = String::Format(" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", nRowsNumb, m_nPageSize);
	try
	{
		m_pSqlCommandClient->mSqlCommand->CommandText = gcnew String(m_strCommandText + offsetClause);
		if (m_pSqlDataTableClient)
			delete m_pSqlDataTableClient;

		m_pSqlDataTableClient = new SqlDataTableClient(m_pFetchedData);

		SqlDataAdapter^ dataAdapter = gcnew SqlDataAdapter(m_pSqlCommandClient->mSqlCommand);
		dataAdapter->Fill(m_pSqlDataTableClient->mSqlDataTable);
		m_pSqlDataTableClient->m_nDataTableSeek = -1;	
		if (m_pSqlDataTableClient->mSqlDataTable->Rows->Count == 0)
		{
			m_ePagingResult = (m_nCurrentPage == 1) ? SqlPagingResult::NoData : SqlPagingResult::LastPage;
			if (m_nLastPage == -1)
			{
				m_nCurrentPage = (m_nCurrentPage == 1) ? m_nCurrentPage - 1 : 1;
				m_nLastPage = m_nCurrentPage;
			}
		}
		else
			if (m_pSqlDataTableClient->mSqlDataTable->Rows->Count < m_nPageSize)
				if (m_nLastPage == -1)
					m_nLastPage = m_nCurrentPage;

		delete dataAdapter;
	}
	catch (SqlException^ e)
	{
		SAFE_DELETE(m_pSqlDataTableClient);

		m_ePagingResult = SqlPagingResult::ErrorPage;

		throw(e);
	}
}

//---------------------------------------------------------------------------
void MSqlCommand::GetFirstPage()
{
	m_ePagingResult = SqlPagingResult::FistPage;
	
//sono già all'inizio, non devo fare nulla
	if (m_nCurrentPage == 1 && m_pSqlDataTableClient)
	{
		m_pSqlDataTableClient->m_nDataTableSeek = -1;
		return;
	}

	m_nCurrentPage = 1;
	try
	{
		GetCurrentPage();	
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
	}
}

//---------------------------------------------------------------------------
void MSqlCommand::GetNextPage()
{
	//sono già in fondo, non devo fare nulla
	if (m_nLastPage != -1 && m_nCurrentPage == m_nLastPage)
	{
		if (m_pSqlDataTableClient)
			m_pSqlDataTableClient->m_nDataTableSeek = -1;
		m_ePagingResult = SqlPagingResult::LastPage;
		return;
	}

	m_nCurrentPage++;
	try
	{
		GetCurrentPage();		
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
	}
}

//---------------------------------------------------------------------------
void MSqlCommand::GetPrevPage()
{
	//sono già in fondo, non devo fare nulla
	if (m_nCurrentPage == 1)
	{
		if (m_pSqlDataTableClient)
			m_pSqlDataTableClient->m_nDataTableSeek = -1;
		m_ePagingResult = SqlPagingResult::FistPage;
		return;
	}	

	m_nCurrentPage--;
	try
	{
		GetCurrentPage();
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
	}
}

//---------------------------------------------------------------------------
void MSqlCommand::GetLastPage()
{	
	//ho già trovato l'ultima pagina	
	if (m_nLastPage == -1)
	{
		//devo andare a blocchi finchè non trovo l'ultima pagina
		while (m_ePagingResult != SqlPagingResult::LastPage)
		{
			m_nCurrentPage++;
			GetCurrentPage();			
		}
		return;
	}
	else
		//sono già in fondo, non devo fare nulla
		if (m_nCurrentPage == m_nLastPage)
		{
			if (m_pSqlDataTableClient)
				m_pSqlDataTableClient->m_nDataTableSeek = -1;
			m_ePagingResult = SqlPagingResult::LastPage;
			return;
		}
	
	m_nCurrentPage = m_nLastPage;
	try
	{
		GetCurrentPage();
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
	}
}


//---------------------------------------------------------------------------
void MSqlCommand::ExecutePagingCommand(MoveType eMoveType, bool bPrepared /*= false*/)
{
	if (!m_pSqlConnection || !m_pSqlCommandClient || m_nPageSize <= 0)
	{
		ASSERT(FALSE);
		return;
	}
	try
	{
		m_pSqlCommandClient->mSqlCommand->Connection = m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection;

		if (m_pSqlConnection->m_pSqlTransactionClient)
			m_pSqlCommandClient->mSqlCommand->Transaction = m_pSqlConnection->m_pSqlTransactionClient->mSqlTransaction;
		m_pSqlCommandClient->mSqlCommand->CommandText = gcnew String(m_strCommandText);

		if (bPrepared)
			m_pSqlCommandClient->mSqlCommand->Prepare();


		GetTotalRecords();
		//se ci sono dei record
		if (m_lRecordCounts > 0)
		{
			int mod = m_lRecordCounts % m_nPageSize;
			int div = m_lRecordCounts / m_nPageSize;
			m_nLastPage = (mod > 0) ? ++div : div;
		}
		else
		{
			m_nCurrentPage = m_nLastPage = 0; //non ci sono dati
			return;
		}
		if (m_lRecordCounts < m_nPageSize)
			m_nPageSize = m_lRecordCounts;

		(eMoveType == E_MOVE_LAST) ? GetLastPage() : GetFirstPage();
	}
	catch (SqlException^ e)
	{
		SAFE_DELETE(m_pSqlDataTableClient);		

		throw(new MSqlException(new SqlExceptionClient(e)));
	}

	catch (MSqlException* ex)
	{
		SAFE_DELETE(m_pSqlDataTableClient);

		throw(ex);
	}
	
}
	
	
	
//---------------------------------------------------------------------------
void MSqlCommand::ExecuteCommand(bool bPrepared /*= false*/)
{
	if (!m_pSqlConnection || !m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}	
	
	try
	{	
		m_lRecordCounts = -1;
		m_pSqlCommandClient->mSqlCommand->Connection = m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection;

		if (m_pSqlConnection->m_pSqlTransactionClient)
			m_pSqlCommandClient->mSqlCommand->Transaction = m_pSqlConnection->m_pSqlTransactionClient->mSqlTransaction;
		m_pSqlCommandClient->mSqlCommand->CommandText = gcnew String(m_strCommandText);
		
		if (bPrepared)
			m_pSqlCommandClient->mSqlCommand->Prepare();


		if (m_bUseDataTable)
		{			
			SAFE_DELETE(m_pSqlDataTableClient);

			m_pSqlDataTableClient = new SqlDataTableClient(m_pFetchedData);

			SqlDataAdapter^ dataAdapter = gcnew SqlDataAdapter(m_pSqlCommandClient->mSqlCommand);
			dataAdapter->Fill(m_pSqlDataTableClient->mSqlDataTable);
			delete dataAdapter;
			return;			
		}

		if (m_pSqlDataReaderClient)
			delete m_pSqlDataReaderClient;


		m_pSqlDataReaderClient = new SqlDataReaderClient(m_pFetchedData);
		m_pSqlDataReaderClient->mSqlDataReader = m_pSqlCommandClient->mSqlCommand->ExecuteReader();

		//se si tratta di un SP con solo valori scalari allora devo fare la fixup degli eventuali parametri di tipo output e del result value
		if (m_pSqlCommandClient->mSqlCommand->CommandType == CommandType::StoredProcedure)
			m_pSqlCommandClient->FetchOutputParameters(m_pParameters);

	}
	catch (SqlException^ e)
	{
		SAFE_DELETE(m_pSqlDataTableClient);
		SAFE_DELETE(m_pSqlDataReaderClient);
	
		throw(new MSqlException(new SqlExceptionClient(e)));		
	}
}


//---------------------------------------------------------------------------
long MSqlCommand::GetRecordsAffected() const
{
	if (m_bUseDataTable)
		return  (m_nPageSize > 0)
		? m_lRecordCounts
		: (m_pSqlDataTableClient && ((DataTable^)m_pSqlDataTableClient->mSqlDataTable) != nullptr) ? m_pSqlDataTableClient->mSqlDataTable->Rows->Count : 0;
	else
		return (m_pSqlDataReaderClient && ((SqlDataReader^)m_pSqlDataReaderClient->mSqlDataReader) != nullptr) ? m_pSqlDataReaderClient->mSqlDataReader->RecordsAffected : m_lRecordCounts;
}

//---------------------------------------------------------------------------
bool MSqlCommand::HasRows() const
{
	if (m_bUseDataTable)
		return  (m_nPageSize > 0)
			? m_lRecordCounts > 0
			: m_pSqlDataTableClient && ((DataTable^)m_pSqlDataTableClient->mSqlDataTable) != nullptr && m_pSqlDataTableClient->mSqlDataTable->Rows->Count > 0 ;	
	else
		return  (m_pSqlDataReaderClient && ((SqlDataReader^)m_pSqlDataReaderClient->mSqlDataReader) != nullptr) ?  m_pSqlDataReaderClient->mSqlDataReader->HasRows : m_lRecordCounts > 0;
}

//---------------------------------------------------------------------------
SqlFetchResult MSqlCommand::MoveOnPaging(MoveType eMoveType, int lSkip /*= 0*/)
{
	SqlFetchResult result = FetchOk;
	try
	{
		switch (eMoveType)
		{
			case E_MOVE_FIRST:
			{
				if (m_nCurrentPage != 1)
					GetFirstPage();
				result = m_pSqlDataTableClient->Move(E_MOVE_FIRST, 0);
				break;
			}
			case E_MOVE_PREV:
			{
				result = m_pSqlDataTableClient->Move(E_MOVE_PREV, lSkip);
				//sono all'inzio del rowset	e sono in una pagina diversa dalla prima
				//devo andare a leggere il blocco precedente ed andare sull'ultimo record
				if (result == BeginOfRowSet && m_nCurrentPage != 1)
				{
					GetPrevPage();
					result = m_pSqlDataTableClient->Move(E_MOVE_LAST, lSkip);
					if (lSkip > 1)
						result = m_pSqlDataTableClient->Move(E_MOVE_PREV, lSkip - 1);
				}
				break;
			}
			case E_MOVE_NEXT:
			{
				result = m_pSqlDataTableClient->Move(E_MOVE_NEXT, lSkip);
				//sono all'inzio del rowset	e sono in una pagina diversa dalla prima
				//devo andare a leggere il blocco precedente ed andare sull'ultimo record
				if (result == EndOfRowSet && m_nCurrentPage != m_nLastPage)
				{
					GetNextPage();
					result = m_pSqlDataTableClient->Move(E_MOVE_NEXT, lSkip);
				}
				break;
			}
			case E_MOVE_LAST:
			{
				if (m_nCurrentPage != m_nLastPage)
					GetLastPage();
				result = m_pSqlDataTableClient->Move(E_MOVE_LAST, 0);
				break;
			}
		}
	}
	catch (MSqlException* e)
	{
		throw(e);
	}
	return result;
}

//---------------------------------------------------------------------------
SqlFetchResult MSqlCommand::Move(MoveType eMoveType, int lSkip /*= 0*/)
{
	SqlFetchResult eResult = Error;

	if (!m_pSqlDataReaderClient && !m_pSqlDataTableClient)
		throw(new MSqlException(_TB("No rowset to move in.")));
	try
	{
		if (m_pSqlDataReaderClient)
			eResult = m_pSqlDataReaderClient->Read(lSkip);
		else
			if (m_pSqlDataTableClient)
			{
				//se effettuo il paging
				if (m_nPageSize > 0)
					eResult = MoveOnPaging(eMoveType, lSkip); 
				else
					eResult = m_pSqlDataTableClient->Move(eMoveType, lSkip);
			}
	}
	catch (SqlException^ e)
	{
		throw(new MSqlException(new SqlExceptionClient(e)));
	}
	catch (MSqlException* me)
	{
		throw(me);
	}

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
void MSqlCommand::ExecuteScalar(bool bPrepared /*=false*/)
{
	m_lRecordCounts = -1;
	if (!m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return;
	}

	try
	{
		m_pSqlCommandClient->mSqlCommand->Connection = m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection;
		m_pSqlCommandClient->mSqlCommand->CommandText = gcnew String(m_strCommandText);

		if (m_pSqlConnection->m_pSqlTransactionClient)
			m_pSqlCommandClient->mSqlCommand->Transaction = m_pSqlConnection->m_pSqlTransactionClient->mSqlTransaction;
		
		if (bPrepared)
			m_pSqlCommandClient->mSqlCommand->Prepare();

		Object^ obj = m_pSqlCommandClient->mSqlCommand->ExecuteScalar();

		if (obj != nullptr)
			m_lRecordCounts = 1;

		//se si tratta di un SP con solo valori scalari allora devo fare la fixup dei parametri di tipo output e del result value
		if (m_pSqlCommandClient->mSqlCommand->CommandType == CommandType::StoredProcedure)
			m_pSqlCommandClient->FetchOutputParameters(m_pParameters);
		else
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
int MSqlCommand::ExecuteNonQuery(bool bPrepared /*=false*/)
{
	m_lRecordCounts = -1;

	if (!m_pSqlCommandClient)
	{
		ASSERT(FALSE);
		return 0;
	}

	try
	{
		
		m_pSqlCommandClient->mSqlCommand->Connection = m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection;
		m_pSqlCommandClient->mSqlCommand->CommandText = gcnew String(m_strCommandText);

		if (m_pSqlConnection->m_pSqlTransactionClient)
			m_pSqlCommandClient->mSqlCommand->Transaction = m_pSqlConnection->m_pSqlTransactionClient->mSqlTransaction;
		
		if (bPrepared)
			m_pSqlCommandClient->mSqlCommand->Prepare();

		m_lRecordCounts = m_pSqlCommandClient->mSqlCommand->ExecuteNonQuery();
		return m_lRecordCounts;
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
	m_pSqlCommandClient->RemoveAllParameters();
	m_pParameters = pParamArray;
	SqlBindObject* pBindElem = NULL;
	
	try
	{
		for (int i = 0; i < m_pParameters->GetSize(); i++)
		{
			pBindElem = (SqlBindObject*)m_pParameters->GetAt(i);
			if (pBindElem)
				m_pSqlCommandClient->AddParam(pBindElem->m_strParamName, pBindElem->m_pDataObj, pBindElem->m_eParamType);
		}
	}
	catch (SqlException^ sqlE)
	{
		throw(new MSqlException((new SqlExceptionClient(sqlE))));
	}
	catch (Exception^ e)
	{
		throw(new MSqlException(CString(e->ToString())));
	}	
}


//---------------------------------------------------------------------------
void MSqlCommand::SetParametersValues()
{
	if (m_pParameters == NULL)
		return;


	if (!m_pSqlCommandClient || ((SqlCommand^)(m_pSqlCommandClient->mSqlCommand)) == nullptr)
	{
		ASSERT(FALSE);
		return;
	}

	SqlBindObject* pBindElem = NULL;
	SqlParameter^ sqlParam = nullptr;
	DataObj* pDataObj = NULL;

	for (int nPos = 0; nPos < m_pParameters->GetSize(); nPos++)
	{
		pBindElem = (SqlBindObject*)m_pParameters->GetAt(nPos);
		sqlParam = m_pSqlCommandClient->mSqlCommand->Parameters[nPos];
		if (pBindElem && sqlParam != nullptr && pBindElem->m_pDataObj)
		{
			pDataObj = pBindElem->m_pDataObj;
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
					sqlParam->Value = gcnew String(((DataText*)pDataObj)->GetString()); break;


				case DATA_BLOB_TYPE:
					DataBlob* pDataBlob = (DataBlob*)pDataObj;
					int size = pDataBlob->GetAllocSize();					
					array<Byte>^ arr = gcnew array<Byte>(size);					
					Marshal::Copy((IntPtr)pDataBlob->GetOleDBDataPtr(), arr, 0,  size);
					sqlParam->Value = arr;

			}
		}
	}
}





/////////////////////////////////////////////////////////////////////////////
// 				class SqlLockManager Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
SqlLockManager::SqlLockManager()
	:
	m_pSqlConnection(NULL),
	m_bOwnSqlConnection(false),
	m_OldConnState(0)
{
}

//----------------------------------------------------------------------------------
SqlLockManager::~SqlLockManager()
{
	if (m_bOwnSqlConnection && m_pSqlConnection->IsOpen())
	{
		m_pSqlConnection->Close();
		delete m_pSqlConnection;
	}
	
	if (m_pCacheLocksEntries)
		delete m_pCacheLocksEntries;
}

//----------------------------------------------------------------------------------
BOOL SqlLockManager::Init(MSqlConnection* pMSqlConnection, const CString& strUserName, const CString& strProcessName, const CString& strAuthenticationToken, const CString& strProcessGuid)
{
	m_strAccountName = strUserName;
	m_strProcessName = strProcessName;
	m_strAuthenticationToken = strAuthenticationToken;
	m_strProcessGuid = strProcessGuid;

	m_pSqlConnection = pMSqlConnection;
	m_bOwnSqlConnection = false;
	if (m_bEnableLockCache)
		m_pCacheLocksEntries = new CacheLocksEntries();

	return TRUE;
}

//----------------------------------------------------------------------------------
BOOL SqlLockManager::Init(const CString& strConnectionString, const CString& strUserName, const CString& strProcessName, const CString& strAuthenticationToken, const CString& strProcessGuid)
{
	m_strAccountName = strUserName;
	m_strProcessName = strProcessName;
	m_strAuthenticationToken = strAuthenticationToken;
	m_strProcessGuid = strProcessGuid;

	m_pSqlConnection = new MSqlConnection();
	m_pSqlConnection->SetConnectionString(strConnectionString);
	m_bOwnSqlConnection = true;

	if (m_bEnableLockCache)
		m_pCacheLocksEntries = new CacheLocksEntries();

	return TRUE;
}


//----------------------------------------------------------------------------------
void SqlLockManager::OpenConnection()
{
	m_OldConnState = m_pSqlConnection->GetConnectionState();
	if ((ConnectionState)m_OldConnState == ConnectionState::Closed)
		m_pSqlConnection->Open();	
}

//----------------------------------------------------------------------------------
void SqlLockManager::CloseConnection()
{
	if (m_pSqlConnection->IsOpen() && (ConnectionState)m_OldConnState == ConnectionState::Closed && !m_pSqlConnection->GetKeepOpen())
		m_pSqlConnection->Close();
}

/// <summary>
/// Prenota un dato
/// </summary>
/// <param name="tableName">nome tabella</param>
/// <param name="lockKey">chiave primaria del dato da prenotare</param>
/// <param name="context">indirizzo in memoria del documento</param>
/// <param name="lockUser">in caso di record già in stato di lock restituisce l'account che impegna il dato</param>
/// <param name="lockApp">in caso di record già in stato di lock restituisce l'applicazione che impegna il dato</param>
/// <returns>true se il dato è stato prenotato con successo, false altrimenti</returns>
//----------------------------------------------------------------------------------
BOOL SqlLockManager::LockCurrent(const CString& strTableName, const CString& strLockKey, const CString& strContext, CString& lockerUser, CString& lockerApp, DataDate& lockerDate)
{
	bool result = true;
	//per prima cosa verifico che non sia un lock già presente nella cache 
	LockEntry* lockEntry = (m_pCacheLocksEntries) ? m_pCacheLocksEntries->GetLockEntry(strTableName, strLockKey) : NULL;
	if (lockEntry != NULL)
	{
		if (!(result = lockEntry->IsSameLock(strContext)))
		{
			lockerUser = CString(lockEntry->m_strLockKey);
			lockerApp = CString(lockEntry->m_strLockApp);
		}
		return result;
	}
	////performance: costruisco secca la commandstring senza passare dai parametri e senza la string.Format
	//CString fieldsValue = _T("\'" + strTableName + "\', '" + strLockKey + "\', \'" + m_strAccountName + "\', \'" + strContext + "\', \'" + m_strProcessName + "\'");
	//CString insertText = _T("INSERT INTO TB_Locks (TableName, LockKey, AccountName, Context, ProcessName) VALUES " + "(" + fieldsValue + ")");

	SqlCommand^ sqlCommand = gcnew SqlCommand("sp_lockcurrent", m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection);

	try
	{	
		sqlCommand->CommandType = CommandType::StoredProcedure;
		sqlCommand->Parameters->AddWithValue("@TableName", gcnew String(strTableName));
		sqlCommand->Parameters->AddWithValue("@LockKey", gcnew String(strLockKey));
		sqlCommand->Parameters->AddWithValue("@AuthenticationToken", gcnew String(m_strAuthenticationToken));
		sqlCommand->Parameters->AddWithValue("@AccountName", gcnew String(m_strAccountName));
		sqlCommand->Parameters->AddWithValue("@Context", gcnew String(strContext));
		sqlCommand->Parameters->AddWithValue("@ProcessName", gcnew String(m_strProcessName));
		sqlCommand->Parameters->AddWithValue("@ProcessGuid", gcnew String(m_strProcessGuid));

		SqlParameter^ param = sqlCommand->Parameters->Add("@Locked", SqlDbType::Int);
		param->Direction = ParameterDirection::ReturnValue;
		param = sqlCommand->Parameters->Add("@LockerAccount", SqlDbType::VarChar, 128);
		param->Direction = ParameterDirection::Output;
		param = sqlCommand->Parameters->Add("@LockerProcess", SqlDbType::VarChar, 256);
		param->Direction = ParameterDirection::Output;
		param = sqlCommand->Parameters->Add("@LockerDate", SqlDbType::DateTime);
		param->Direction = ParameterDirection::Output;

		OpenConnection();
		sqlCommand->ExecuteNonQuery();
		result = (int)sqlCommand->Parameters["@Locked"]->Value == 1;
		if (!result)
		{
			lockerUser = (String^)sqlCommand->Parameters["@LockerAccount"]->Value;
			lockerApp = (String^)sqlCommand->Parameters["@LockerProcess"]->Value;
			DateTime dateTime = (DateTime)sqlCommand->Parameters["@LockerDate"]->Value;
			lockerDate.Assign(dateTime.Day, dateTime.Month, dateTime.Year, dateTime.Hour, dateTime.Minute, dateTime.Second);			
		}
		else
		//ho effettuato il lock, ne faccio il cache
		if (m_pCacheLocksEntries)
			m_pCacheLocksEntries->AddLockEntry(strTableName, strLockKey, strContext);

	}
	catch (SqlException^ e)
	{
		m_strErrorMessage = e->Message;
	}
	catch (Exception^ ex)
	{
		m_strErrorMessage = ex->Message;
	}
	finally
	{
		if (sqlCommand != nullptr)
			delete sqlCommand;

		CloseConnection();
	}
	return result;
}

/// <summary>
/// Verifica se un dato è stato prenotato da un altro contesto/utente
/// </summary>
/// <param name="companyDBName">Nome del database aziendale</param>
/// <param name="tableName">Nome della tabella</param>
/// <param name="lockKey">Chiave primaria del lock</param>
/// <param name="context">Indirizzo in memoria del contesto che richiede se il dato è in stato di locked</param>
/// <returns>true il dato è stato prenotato da un altro contesto false se non è stato prenotato oppure è prenotato dallo stesso contesto</returns>
//----------------------------------------------------------------------------------
BOOL SqlLockManager::IsCurrentLocked(const CString& strTableName, const CString& strLockKey, const CString& strContext)
{
	//per prima cosa verifico che non sia un lock già presente nella cache con lo stesso indirizzo o meno
	LockEntry* lockEntry = (m_pCacheLocksEntries) ? m_pCacheLocksEntries->GetLockEntry(strTableName, strLockKey) : NULL;
	if (lockEntry != NULL)
		return !lockEntry->IsSameLock(strContext); //vuol dire che è locked da un altro contesto

	bool result = false;
	SqlCommand^ sqlCommand = gcnew SqlCommand("sp_iscurrentlocked", m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection);
	try
	{
		sqlCommand->CommandType = CommandType::StoredProcedure;
		sqlCommand->Parameters->AddWithValue("@TableName", gcnew String(strTableName));
		sqlCommand->Parameters->AddWithValue("@LockKey", gcnew String(strLockKey));
		sqlCommand->Parameters->AddWithValue("@Context", gcnew String(strContext));
		SqlParameter^ param = sqlCommand->Parameters->Add("@Locked", SqlDbType::Int);
		param->Direction = ParameterDirection::ReturnValue;
		OpenConnection();
		sqlCommand->ExecuteNonQuery();
		result = (int)sqlCommand->Parameters["@Locked"]->Value == 1;		
	}
	catch (SqlException^ e)
	{
		m_strErrorMessage = e->Message;
	}
	finally
	{
		if (sqlCommand != nullptr)
			delete sqlCommand;

		CloseConnection();
	}
	return result;
}

/// <summary>
/// Verifica se il record passato come chiave è stato lockato dal contesto stesso individuato da context
/// E' l'opposto dell'IsCurrentLocked
/// </summary>
/// <param name="tableName">Nome della tabella</param>
/// <param name="lockKey">Chiave primaria del lock</param>
/// <param name="context">Indirizzo in memoria del contesto che richiede se il dato è in stato di locked</param>
/// <returns>true il dato è stato prenotato dallo stesso contesto false se non è stato prenotato oppure è prenotato da altro contesto</returns>
//----------------------------------------------------------------------------------
BOOL SqlLockManager::IsMyLock(const CString& strTableName, const CString& strLockKey, const CString& strContext)
{
	//Se il mio è per forza nella cache
	if (m_pCacheLocksEntries)
		return m_pCacheLocksEntries->ExistLockEntry(strTableName, strLockKey, strContext);

	bool result = false;
	SqlCommand^ sqlCommand = gcnew SqlCommand("sp_ismylock", m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection);
	try
	{
		sqlCommand->CommandType = CommandType::StoredProcedure;
		sqlCommand->Parameters->AddWithValue("@TableName", gcnew String(strTableName));
		sqlCommand->Parameters->AddWithValue("@LockKey", gcnew String(strLockKey));
		sqlCommand->Parameters->AddWithValue("@AuthenticationToken", gcnew String(m_strAuthenticationToken));
		sqlCommand->Parameters->AddWithValue("@Context", gcnew String(strContext));
		SqlParameter^ param = sqlCommand->Parameters->Add("@Locked", SqlDbType::Int);
		param->Direction = ParameterDirection::ReturnValue;
		OpenConnection();
		sqlCommand->ExecuteNonQuery();
		result = (int)sqlCommand->Parameters["@Locked"]->Value == 1;
	}
	catch (SqlException^ e)
	{
		m_strErrorMessage = e->Message;
	}
	finally
	{
		if (sqlCommand != nullptr)
			delete sqlCommand;

		CloseConnection();
	}
	return result;
}

/// <summary>
/// Prende informazioni su un lock
/// </summary>
/// <param name="lockKey">Chiave primaria del lock</param>
/// <param name="tableName">Nome della tabella</param>
/// <param name="lockUser">in caso di record già in stato di lock restituisce l'account che impegna il dato</param>
/// <param name="lockTime">Istante di prenotazione del dato</param>
/// <param name="processName">Nome del processo che ha prenotato il dato</param>
/// <returns>true se la funzione ha avuto successo</returns>
//-----------------------------------------------------------------------
BOOL SqlLockManager::GetLockInfo(const CString& strLockKey, const CString& strTableName, CString& lockerUser, CString& lockerApp, DataDate& lockerDate)
{
	bool result = false;

	//prima guardo nella cache
	LockEntry* lockEntry = (m_pCacheLocksEntries) ? m_pCacheLocksEntries->GetLockEntry(strTableName, strLockKey) : NULL;
	if (lockEntry != NULL)
	{
		lockerUser = lockEntry->m_strLockKey;
		lockerApp = lockEntry->m_strLockApp;
		delete lockEntry;
	}
	
	SqlCommand^ sqlCommand = gcnew SqlCommand("sp_getlockinfo", m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection);

	try
	{
		CString processGuid = _T(""); //@@TODO da valorizzare quando avro' il guid della singola istanza di TBLoader
		sqlCommand->CommandType = CommandType::StoredProcedure;
		sqlCommand->Parameters->AddWithValue("@TableName", gcnew String(strTableName));
		sqlCommand->Parameters->AddWithValue("@LockKey", gcnew String(strLockKey));

		SqlParameter^ param = sqlCommand->Parameters->Add("@Result", SqlDbType::Int);
		param->Direction = ParameterDirection::ReturnValue;
		param = sqlCommand->Parameters->Add("@LockerAccount", SqlDbType::VarChar, 128);
		param->Direction = ParameterDirection::Output;
		param = sqlCommand->Parameters->Add("@LockerProcess", SqlDbType::VarChar, 256);
		param->Direction = ParameterDirection::Output;
		param = sqlCommand->Parameters->Add("@LockerDate", SqlDbType::DateTime);
		param->Direction = ParameterDirection::Output;

		OpenConnection();
		sqlCommand->ExecuteNonQuery();
		result = (int)sqlCommand->Parameters["@Result"]->Value == 1;
		if (result)
		{
			lockerUser = (String^)sqlCommand->Parameters["@LockerAccount"]->Value;
			lockerApp = (String^)sqlCommand->Parameters["@LockerProcess"]->Value;
			DateTime dateTime = (DateTime)sqlCommand->Parameters["@LockerDate"]->Value;
			lockerDate.Assign(dateTime.Day, dateTime.Month, dateTime.Year, dateTime.Hour, dateTime.Minute, dateTime.Second);
		}		
	}
	catch (SqlException^ e)
	{
		m_strErrorMessage = e->Message;
	}
	finally
	{
		if (sqlCommand != nullptr)
			delete sqlCommand;

		CloseConnection();
	}
	return result;
}


/// <summary>
/// Rimuove la prenotazione di un record
/// </summary>
/// <param name="tableName">Nome della tabella</param>
/// <param name="lockKey">Chiave primaria del lock</param>
/// <param name="context">Indirizzo in memoria del documento</param>
/// <returns>true se la funzione ha avuto successo</returns>
//----------------------------------------------------------------------------------		
BOOL SqlLockManager::UnlockCurrent(const CString& strTableName, const CString& strLockKey, const CString& strContext)
{
	bool result = false;
	SqlCommand^ sqlCommand = gcnew SqlCommand("sp_unlockcurrent", m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection);
	try
	{
		sqlCommand->CommandType = CommandType::StoredProcedure;
		sqlCommand->Parameters->AddWithValue("@TableName", gcnew String(strTableName));
		sqlCommand->Parameters->AddWithValue("@LockKey", gcnew String(strLockKey));
		sqlCommand->Parameters->AddWithValue("@AuthenticationToken", gcnew String(m_strAuthenticationToken));
		sqlCommand->Parameters->AddWithValue("@Context", gcnew String(strContext));
		SqlParameter^ param = sqlCommand->Parameters->Add("@Result", SqlDbType::Int);
		param->Direction = ParameterDirection::ReturnValue;
		OpenConnection();
		sqlCommand->ExecuteNonQuery();
		result = (int)sqlCommand->Parameters["@Result"]->Value > 0;
		//poi rimuovo la cache
		if (result && m_pCacheLocksEntries)
			m_pCacheLocksEntries->RemoveLockEntry(strTableName, strLockKey);
	}
	catch (SqlException^ e)
	{
		m_strErrorMessage = e->Message;
	}
	finally
	{
		if (sqlCommand != nullptr)
			delete sqlCommand;

		CloseConnection();
	}
	return result;
}


/// <summary>
/// Rimuove tutti i lock su una tabella per un determinato contesto
/// </summary>
//----------------------------------------------------------------------------------		
BOOL SqlLockManager::UnlockAllTableContext(const CString& strTableName, const CString& strContext)
{
	bool result = false;
	SqlCommand^ sqlCommand = gcnew SqlCommand("sp_unlockalltablecontext", m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection);
	try
	{
		sqlCommand->CommandType = CommandType::StoredProcedure;
		sqlCommand->Parameters->AddWithValue("@TableName", gcnew String(strTableName));
		sqlCommand->Parameters->AddWithValue("@Context", gcnew String(strContext));
		sqlCommand->Parameters->AddWithValue("@AuthenticationToken", gcnew String(m_strAuthenticationToken));
		SqlParameter^ param = sqlCommand->Parameters->Add("@Result", SqlDbType::Int);
		param->Direction = ParameterDirection::ReturnValue;
		OpenConnection();
		sqlCommand->ExecuteNonQuery();
		result = (int)sqlCommand->Parameters["@Result"]->Value > 0;
		if (result && m_pCacheLocksEntries)
			m_pCacheLocksEntries->RemoveEntriesForContext(strTableName, strContext);
	}
	catch (SqlException^ e)
	{
		m_strErrorMessage = e->Message;
	}
	finally
	{
		if (sqlCommand != nullptr)
			delete sqlCommand;

		CloseConnection();
	}
	return result;
}

/// <summary>
/// Rimuove tutti i lock di un determinato contesto
/// </summary>
//----------------------------------------------------------------------------------		
BOOL SqlLockManager::UnlockAllContext(const CString& strContext)
{
	bool result = false;
	SqlCommand^ sqlCommand = gcnew SqlCommand("sp_unlockallcontext", m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection);
	try
	{
		sqlCommand->CommandType = CommandType::StoredProcedure;
		sqlCommand->Parameters->AddWithValue("@Context", gcnew String(strContext));
		sqlCommand->Parameters->AddWithValue("@AuthenticationToken", gcnew String(m_strAuthenticationToken));
		SqlParameter^ param = sqlCommand->Parameters->Add("@Result", SqlDbType::Int);
		param->Direction = ParameterDirection::ReturnValue;
		OpenConnection();
		sqlCommand->ExecuteNonQuery();
		result = (int)sqlCommand->Parameters["@Result"]->Value > 0;
		//dopo cancello la cache
		if (result && m_pCacheLocksEntries)
			m_pCacheLocksEntries->RemoveEntriesForContext(strContext);
	}
	catch (SqlException^ e)
	{
		m_strErrorMessage = e->Message;
	}
	finally
	{
		if (sqlCommand != nullptr)
			delete sqlCommand;

		CloseConnection();
	}
	return result;
}

/// <summary>
/// Rimuove tutti i lock per l'account corrente
/// </summary>
//----------------------------------------------------------------------------------		
BOOL SqlLockManager::UnlockAllForCurrentConnection()
{
	bool result = false;
	SqlCommand^ sqlCommand = gcnew SqlCommand("sp_unlockalltoken", m_pSqlConnection->m_pSqlConnectionClient->mSqlConnection);
	try
	{
		sqlCommand->CommandType = CommandType::StoredProcedure;
		sqlCommand->Parameters->AddWithValue("@AuthenticationToken", gcnew String(m_strAuthenticationToken));
		SqlParameter^ param = sqlCommand->Parameters->Add("@Result", SqlDbType::Int);
		param->Direction = ParameterDirection::ReturnValue;
		OpenConnection();
		sqlCommand->ExecuteNonQuery();
		result = (int)sqlCommand->Parameters["@Result"]->Value > 0;
		//poi rimuovo la cache
		if (result && m_pCacheLocksEntries)
			m_pCacheLocksEntries->RemoveAll();
	}
	catch (SqlException^ e)
	{
		m_strErrorMessage = e->Message;
	}
	finally
	{
		if (sqlCommand != nullptr)
			delete sqlCommand;

		CloseConnection();
	}
	return result;
}