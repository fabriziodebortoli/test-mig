
#include "stdafx.h"
#include <atldbcli.h>

#include <TbNameSolver\Chars.h>
#include <TBNameSolver\ThreadContext.h>

#include <TbGeneric\dataobj.h>
#include <TbGeneric\minmax.h>
#include <TbGeneric\globals.h>

#include <TbGenlib\Messages.h>
#include <TbGenlib\addonmng.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\basedoc.h>

#include "sqltable.h"
#include "sqlcatalog.h"
#include "wclause.h"
#include "oledbmng.h"
#include "sqlmark.h"
#include "sqlaccessor.h"
#include "performanceanalizer.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define STATUS_LEN				   4
#define PARAM_ALLOC_SIZE		2048 
#define PARAM_ALLOC_SIZE_UNI	1024 //per problema con database unicode.
#define	XML_ROOT			_T("<?xml version=\"1.0\"?>")\
							_T("<DataTables />")

// SQL command support
//-----------------------------------------------------------------------------
static const TCHAR szCall[] 		= _T("{call ");
static const TCHAR szSelect[] 		= _T("SELECT ");
static const TCHAR szDeleteFrom[]	= _T("DELETE FROM ");
static const TCHAR szUpdate[] 		= _T("UPDATE ");
static const TCHAR szInsertInto[] 	= _T("INSERT INTO ");
static const TCHAR szFrom[] 		= _T(" FROM ");
static const TCHAR szAs[] 			= _T(" AS ");
static const TCHAR szWhere[] 		= _T(" WHERE ");
static const TCHAR szOrderBy[] 		= _T(" ORDER BY ");
static const TCHAR szGroupBy[] 		= _T(" GROUP BY ");
static const TCHAR szUnion[] 		= _T(" UNION ");
static const TCHAR szHaving[] 		= _T(" HAVING ");
static const TCHAR szComma[] 		= _T(",");
static const TCHAR szLeftParen[] 	= _T(" (");
static const TCHAR szValuesParen[] 	= _T(" VALUES (");
static const TCHAR szSet[] 			= _T(" SET ");
static const TCHAR szTop[]          = _T(" TOP ");
static const TCHAR szPercent[]      = _T(" PERCENT  ");
static const TCHAR szWithTies[]     = _T(" WITH TIES ");
static const TCHAR szDistinct[]     = _T(" DISTINCT ");
static const TCHAR szDataTables[]	= _T("DataTables");

static const TCHAR szDefaultParam[] = _T("dp_%d");

// per problema stringa vuota di Oracle
static const TCHAR szBlank[]			= _T(" ");

// gestione caching
static const TCHAR szEmptyParameter[]	= _T("EmptyParameter");


#define TRUNCATE_SIZE		220

// arricchisto il messaggio dell'exception con ulteriori informazioni
#define CHECK_BUILD_ERROR(command)\
	TRY\
	{\
		command\
	}\
	CATCH(MSqlException, e)\
	{\
		e->UpdateErrorString(cwsprintf(SqlErrorString::SQL_ERROR_BUILD_QUERY(), m_pRecord->GetTableName()), FALSE);\
		m_bErrorFound = TRUE;\
		m_strErrorFound = e->m_strError;\
		TRACE(e->m_strError);\
	}\
	END_CATCH\


//-----------------------------------------------------------------------------
void AFXAPI TraceError(LPCTSTR szTrace)
{
	CString strTrace = szTrace;
	TRACE_SQL(strTrace, NULL);

#ifdef _DEBUG
	// Display 255 chars/line
	while (strTrace.GetLength() > TRUNCATE_SIZE)
	{
		TRACE1("%s\n", strTrace.Left(TRUNCATE_SIZE));
		strTrace = strTrace.Right(strTrace.GetLength() - TRUNCATE_SIZE);
	}
	TRACE1("%s\n", strTrace);
#endif
}

//-----------------------------------------------------------------------------
const CString GetDateFunction(DBMSType dbmsType)
{
	switch (dbmsType)
	{
		case DBMS_SQLSERVER:	return _T("getdate()");
		case DBMS_ORACLE:		return _T("sysdate");
		default:				return _T("getdate()");
	}
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlTableItem, CObject)

//-----------------------------------------------------------------------------
SqlTableItem::SqlTableItem(const CString& strTableName, const CString& strAliasName, SqlRecord* pRec /*=NULL*/, const CString& strOuterJoinClause /*= _T("")*/)
	:
	m_strTableName	(strTableName),
	m_strAliasName	(strAliasName),
	m_strOuterJoinClause (strOuterJoinClause),
	m_pRecord(pRec)
{ 
}

//-----------------------------------------------------------------------------
SqlTableItem::SqlTableItem(const SqlTableItem& sti)
	:
	m_strTableName	(sti.m_strTableName),
	m_strAliasName	(sti.m_strAliasName),
	m_pRecord		(sti.m_pRecord ? sti.m_pRecord->Clone() : NULL)
{ 
}

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlTableArray, Array)

//-----------------------------------------------------------------------------
SqlTableArray::SqlTableArray(SqlConnection* pSqlConnection)
:
	m_pSqlConnection (pSqlConnection)
{
	ASSERT(m_pSqlConnection);		
}

//-----------------------------------------------------------------------------
SqlTableArray::SqlTableArray(const SqlTableArray& sta)
	:
	m_pSqlConnection (sta.m_pSqlConnection)
{
	for (int i=0; i < GetSize(); i++)
	{
		SqlTableItem* sti = sta.GetAt(i);
		Add (new SqlTableItem(*sti));
	}
}

//-----------------------------------------------------------------------------
int SqlTableArray::GetTableIndex(const CString& strTableName, const CString& strAliasName) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		if (GetAt(i)->m_strTableName.CompareNoCase(strTableName) == 0 &&
			GetAt(i)->m_strAliasName.CompareNoCase(strAliasName) == 0)
		return i;
	}

	ASSERT(FALSE);
	return -1;
}

// pu� essere o il nome della tabella o il suo alias
//-----------------------------------------------------------------------------
int SqlTableArray::GetTableIndex(const CString& strTableAliasName) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		// � il nome della tabella e non ho nessun alias
		// attenzione perch� posso avere pi� volte la stessa tabella con
		// una senza alias e le altre con alias.
		if (
			(
				GetAt(i)->m_strTableName.CompareNoCase(strTableAliasName) == 0 &&
				(
					GetAt(i)->m_strAliasName.IsEmpty() ||
					GetAt(i)->m_strAliasName.CompareNoCase(strTableAliasName) == 0
				)
			) ||	// � il nome di un alias
			GetAt(i)->m_strAliasName.CompareNoCase(strTableAliasName) == 0)
		return i;
	}

	ASSERT(FALSE);
	return -1;
}
//-----------------------------------------------------------------------------
SqlTableItem* SqlTableArray::GetTableByName(const CString& strTableName, const CString& strAliasName) const
{
	int nIdx = GetTableIndex(strTableName, strAliasName);
	return (nIdx > -1) ? GetAt(nIdx) : NULL;
}

//-----------------------------------------------------------------------------
const SqlTableInfo* SqlTableArray::GetTableInfo(int nIdx) const
{
	if (nIdx > GetUpperBound()) 
	{
		ASSERT(FALSE);
		return NULL;
	}

	return m_pSqlConnection->GetTableInfo(GetAt(nIdx)->m_strTableName);
}

//-----------------------------------------------------------------------------
const SqlTableInfo* SqlTableArray::GetTableInfo(const CString& strTableAliasName) const
{
	int nIdx = GetTableIndex(strTableAliasName);
	return (nIdx > -1) ? GetTableInfo(nIdx) : NULL;
}

//-----------------------------------------------------------------------------
CString	SqlTableArray::ToString()
{
	CString s;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		SqlTableItem* pT = GetAt(i);

		if (i > 0)
			s += _T(", ");

		s += pT->m_strTableName;

		if (!pT->m_strAliasName.IsEmpty() && pT->m_strTableName.CompareNoCase(pT->m_strAliasName))
			s +=   _T(" AS ") + pT->m_strAliasName;
	}
	return s;
}

    

/////////////////////////////////////////////////////////////////////////////
//						class SqlRowSet
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlRowSet, SqlObject)

//-----------------------------------------------------------------------------
SqlRowSet::SqlRowSet()
:
	m_pRowSet				(NULL),
	m_pSqlConnection		(NULL),
	m_pSqlSession			(NULL),
	m_pOldSqlSession		(NULL)
{	
    Initialize();
	m_pParamArray = new SqlParamArray();  
	m_pColumnArray = new SqlColumnArray();  	
}

//-----------------------------------------------------------------------------
SqlRowSet::SqlRowSet(SqlSession* pSqlSession, CBaseDocument* pDocument)
:
	SqlObject				(NULL, pDocument),
	m_pRowSet				(NULL),
	m_pSqlConnection		(NULL),	
	m_pSqlSession			(pSqlSession),
	m_pOldSqlSession		(m_pSqlSession)
{
	if (m_pSqlSession)
	{
		m_pSqlConnection = m_pSqlSession->GetSqlConnection();	
		//eredito il contesto della sessione
		SetContext(m_pSqlSession->m_pContext);
	}

	Initialize();
	m_pParamArray = new SqlParamArray();  
	m_pColumnArray = new SqlColumnArray(); 	
}

//-----------------------------------------------------------------------------
SqlRowSet::~SqlRowSet()
{
	if (IsOpen())
		Close();

	SAFE_DELETE(m_pParamArray); 
	SAFE_DELETE(m_pColumnArray);	
}

//-----------------------------------------------------------------------------
void SqlRowSet::Initialize()
{
	m_bScrollable	= FALSE;
	m_bUpdatable	= FALSE;
	m_bInit			= FALSE;
	m_bErrorFound	= FALSE;
	m_strErrorFound.Empty();
	m_eCursorType	= E_FAST_FORWARD_ONLY;
	m_nPageSize = 0;
}

//-----------------------------------------------------------------------------
void SqlRowSet::SetSqlSession(SqlSession* pSqlSession)
{
	TRACE_SQL(_T("SetSqlSession"), this);
	if (pSqlSession)
	{
		m_pSqlSession = pSqlSession;
		m_pSqlConnection = pSqlSession->GetSqlConnection();
		SetContext(m_pSqlSession->m_pContext);

	}
	else
	{
		ASSERT(pSqlSession);
		TRACE(L"SqlRowSet::SetSqlSession: Invalid session\n");
	}
}


//-----------------------------------------------------------------------------
::DBMSType SqlRowSet::GetDBMSType() const
{
	return m_pSqlSession && m_pSqlSession->GetDBMSType() != DBMS_UNKNOWN ?
		m_pSqlSession->GetDBMSType() :
		m_pSqlConnection->GetDBMSType();
}

// Gestione dei cursori: VEDI DOCUMENTAZIONE TBOLEDB-REFGUIDE.DOC nella documentazione progetto
// E'possibile modificare il comportamento di default attraverso i due parametri booleani oppure
// passando in pDBPropSet le propriet� che deve assumere il RowSet
//-----------------------------------------------------------------------------
void SqlRowSet::Open(BOOL bUpdatable, CursorType eCursorType)
{
	TRACE_SQL(_T("OpenSqlRowSet"), this);

	m_eCursorType = eCursorType;
	SqlRowSet::Open(bUpdatable, (m_eCursorType == E_KEYSET_CURSOR || m_eCursorType == E_DYNAMIC_CURSOR));
	
}

//-----------------------------------------------------------------------------
void SqlRowSet::Open(BOOL bUpdatable /*=FALSE*/, BOOL bScrollable /*=FALSE*/)
{             
	m_bUpdatable = bUpdatable;
	m_bScrollable = bScrollable;

	START_DB_TIME(DB_OPEN_TABLE)
	if (!m_pSqlSession)
		ThrowSqlException(_TB("SqlRowSet::Open: attempt to open a rowset in an invalid session."));

	if (m_pRowSet)
		ThrowSqlException(_TB("SqlRowSet::Open: Rowset already open"));


	m_pRowSet = new MSqlCommand(m_pSqlSession->GetMSqlConnection());
	m_pRowSet->Create(bScrollable == TRUE);

	STOP_DB_TIME(DB_OPEN_TABLE)
}


//-----------------------------------------------------------------------------
BOOL SqlRowSet::IsOpen() const
{ 
	return m_pRowSet != NULL; 
}	


//-----------------------------------------------------------------------------
BOOL SqlRowSet::IsConnected() const
{
	return m_pRowSet && m_pRowSet->IsConnected();
}


//-----------------------------------------------------------------------------
void SqlRowSet::Connect()
{
	TRACE_SQL(_T("ConnectCommand"), this);
	START_DB_TIME(DB_CONNECT_CMD)
		
	if (m_pSqlSession)
		m_pSqlSession->AddCommand(this);

	STOP_DB_TIME(DB_CONNECT_CMD)
}

//-----------------------------------------------------------------------------
void SqlRowSet::Disconnect()
{
	ASSERT_VALID(this);
	TRACE_SQL(_T("DisconnectCommand"), this);
	START_DB_TIME(DB_DISCONNECT_CMD)

	if (m_pRowSet && m_pRowSet->IsConnected())
		m_pRowSet->Disconnect();
	if (m_pSqlSession)
		m_pSqlSession->RemoveCommand(this);

	STOP_DB_TIME(DB_DISCONNECT_CMD)
	
}

// deve evitare di usare la ClearQuery perche` il record potrebbe essere stato
// cancellato da fuori ma il puntatore non essere ancora NULL;
//-----------------------------------------------------------------------------
void SqlRowSet::Close()
{
	ASSERT_VALID(this);
	TRACE_SQL(_T("CloseRowSet"), this);

	ClearRowSet();
	START_DB_TIME(DB_CLOSE_TABLE)
	if (m_pRowSet)
	{
		delete m_pRowSet;
		m_pRowSet = NULL;
	}	

	Initialize();	
	STOP_DB_TIME(DB_CLOSE_TABLE)
}
//-----------------------------------------------------------------------------
void SqlRowSet::EnablePaging(int nPageSize) 
{ 
	m_nPageSize = nPageSize; 	
};


//-----------------------------------------------------------------------------
BOOL SqlRowSet::ExistParam (const CString& strParamName) const
{
	return m_pParamArray->ExistParam(strParamName); 
}

// gli array legati all'Accessor del RowSet vengono deletati onde evitare
// collisioni di memoria dopo una close e un'open successiva di un rowset
//-----------------------------------------------------------------------------
void SqlRowSet::ClearParams()	
{ 
	TRACE_SQL(_T("ClearParams"), this);

	if (m_pParamArray)
		delete m_pParamArray; 
	
	m_pParamArray = new SqlParamArray();
}

//-----------------------------------------------------------------------------
void SqlRowSet::ClearColumns()	
{ 
	TRACE_SQL(_T("ClearColumns"), this);

	if (m_pColumnArray)
		delete m_pColumnArray; 
	
	m_pColumnArray = new SqlColumnArray();
}

// Pulisce tutti i parametri, le colonne e le stringhe preposte alla query
//-----------------------------------------------------------------------------
void SqlRowSet::ClearRowSet()
{	
	TRACE_SQL(_T("ClearRowSet"), this);


	m_strSQL		.Empty();	// SQL completa
	m_strSelect		.Empty();	// Select-Update-Delete clause        
	m_strFilter		.Empty();	// WHERE clause	

	//where clause lock ottimistico chiavi + old values
	m_strFilterKeys.Empty();
	m_strFilterOldValues.Empty();

	m_strCurrentTables.RemoveAll();
	ClearColumns();
	ClearParams	();
	
	Disconnect();	
}

//-----------------------------------------------------------------------------
long SqlRowSet::GetRowSetCount() 
{
	//Remarks: The RecordsAffected property is not set until all rows are read and you close the SqlDataReader.
	//The number of rows changed, inserted, or deleted; 0 if no rows were affected or the statement failed; and -1 for SELECT statements.
	// solo nel caso di SqlDataAdapter (che simula il cursore scrollabile) ho un numero veritiero
	return (m_pRowSet) ? m_pRowSet->GetRecordsAffected() : 0;
}

// Aggiunge un nuovo parametro controllando che non sia gia` stato definito 
// un parametro con lo stesso nome e che la Table non sia ancora aperta perche`
// il numero di parametri dopo la open non puo` essere modificato
//-----------------------------------------------------------------------------
void SqlRowSet::AddParam (const CString& strParamName, const DataObj& aDataObj, SqlParamType eParamType /*= Input*/,  int nInsertPos /*=-1*/)
{                  
	DBLENGTH nLen = 0;
	if (aDataObj.GetDataType() == DATA_STR_TYPE)
	{
		nLen = aDataObj.GetColumnLen();
		if ((nLen <= 0))
			nLen = (m_pSqlConnection->m_bUseUnicode) ? PARAM_ALLOC_SIZE_UNI : PARAM_ALLOC_SIZE;  // x i parametri da woorm x cui non ho l'AllocSize				
	}
	else
		nLen = aDataObj.GetOleDBSize();

	AddParam(strParamName, aDataObj.GetDataType(), nLen, eParamType, aDataObj.GetSqlDataType(), _T(""), nInsertPos);
}

//-----------------------------------------------------------------------------
void SqlRowSet::AddDataTextParam(const CString& strParamName, const DataObj& aDataObj, const CString& strColumnName, SqlParamType eParamType /*= Input*/)
{
	DBLENGTH nLen = 0;
	if (aDataObj.GetDataType() != DATA_TXT_TYPE)
	{
		ASSERT(FALSE);
		return;
	}

	AddParam(strParamName, aDataObj.GetDataType(), nLen, eParamType, aDataObj.GetSqlDataType(), strColumnName);
}


//-----------------------------------------------------------------------------
BOOL SqlRowSet::IsNull() const
{
	return (m_pRowSet == NULL);
}

//-----------------------------------------------------------------------------
void SqlRowSet::AddParam 
		(
			const CString&  strParamName, 
			const DataType& nDataType, 
			const int		nLen, 
			SqlParamType eParamType /*= Input*/, 
			const SWORD nSqlDataType /*= DBTYPE_EMPTY*/,
			const CString&  strColumnName,
			int nInsertPos /*=-1*/
		)
{
	if (!strParamName.IsEmpty() && m_pParamArray->ExistParam(strParamName))
		ThrowSqlException(_TB("SqlRowSet::AddParam: Query parameters duplicated."));
	

	m_pParamArray->AddParam(strParamName, nDataType, nLen, eParamType, nSqlDataType, nInsertPos);
}

//-----------------------------------------------------------------------------
void SqlRowSet::AddProcParam(const CString& strParamName, short nOleDbParamType, DataObj* pDataObj)
{
	if (!strParamName.IsEmpty() && m_pParamArray->ExistParam(strParamName))
		ThrowSqlException(_TB("SqlRowSet::AddProcParam: procedure parameters duplicated."));	

	SqlParamType eParamType;
	switch (nOleDbParamType)
	{
	case DBPARAMTYPE_OUTPUT:
		eParamType = Output;
		break;
	case DBPARAMTYPE_RETURNVALUE:
		eParamType = ReturnValue;
		break;
	case DBPARAMTYPE_INPUT:
		eParamType = Input;
		break;
	case DBPARAMTYPE_INPUTOUTPUT:
		eParamType = InputOutput;
		break;
	default:
		eParamType = InputOutput;
		break;
	}
	DBLENGTH nLen = 0;
	if (pDataObj->GetDataType() == DATA_STR_TYPE && pDataObj->GetColumnLen() <= 0)
		pDataObj->SetAllocSize(PARAM_ALLOC_SIZE);  // x i parametri da woorm x cui non ho l'AllocSize

	m_pParamArray->AddParam
					(
						strParamName, 
						pDataObj, 
						eParamType,
						nEmptySqlRecIdx
					);	

}

//-----------------------------------------------------------------------------
void SqlRowSet::AddProcParam(SqlProcedureParamInfo*	pParamInfo, DataObj* pDataObj)
{
	return AddProcParam (pParamInfo->m_strParamName, pParamInfo->m_nType, pDataObj);
}

//-----------------------------------------------------------------------------
void SqlRowSet::SetParamValue (const CString& strParamName, const DataObj& aDataObj)
{
	m_pParamArray->SetParamValue(strParamName, aDataObj);
}

//-----------------------------------------------------------------------------
void SqlRowSet::SetParamLike (const CString& strParamName, const DataObj& aDataObj)
{
	DataStr strLike ((DataStr&)aDataObj + _T("%"));
	m_pParamArray->SetParamValue(strParamName, strLike);
}

//-----------------------------------------------------------------------------
void SqlRowSet::GetParamValue (const CString& strParamName, DataObj* pDataObj) const	
{
	m_pParamArray->GetParamValue(strParamName, pDataObj);
}


//-----------------------------------------------------------------------------
void SqlRowSet::GetParamValue (int nPos, DataObj* pDataObj) const
{
	m_pParamArray->GetParamValue(nPos, pDataObj);
}

//-----------------------------------------------------------------------------
DataType SqlRowSet::GetParamType (int nPos) const
{
	return m_pParamArray->GetParamDataType(nPos);
}

//-----------------------------------------------------------------------------
CString GetParamStr(SqlRowSet* pRowSet)
{
	if	(!IsTraceSQLEnabled() || !pRowSet->m_pParamArray || pRowSet->m_pParamArray->GetSize() == 0)
		return _T("");

	CString strParams;
	for (int i = 0; i <= pRowSet->m_pParamArray->GetUpperBound(); i++)
	{
		SqlBindingElem* pElem = pRowSet->m_pParamArray->GetAt(i);
		strParams += _T("'") + pElem->GetDataObj()->Str() + _T("' ");
	}
	
	return strParams;
}


//sostituisce i nomi dei parametri ai punti ? (venivano usati con ODBC e poi OLEDB)
//-----------------------------------------------------------------------------
void SqlRowSet::SubstituteQuestionMarks(CString& strSQL)
{
	if (strSQL.IsEmpty() || strSQL.Find(_T("?")) < 0)
		return;

	CString strFilter;

	int nParam = 0;
	int nStart = 0;
	int nEnd = 0;
	int nSingleQuot = 0;
	int nParamCount = 0;
	CString strParamName;

	for (nEnd = 0; nEnd < strSQL.GetLength(); nEnd++)
	{
		//ho trovato una stringa 
		if (strSQL[nEnd] == _T('\''))
		{
			nSingleQuot++;
			if (nSingleQuot == 2) //ho trovato sia l'apice di apertura stringa che quello di chiusura. Riazzero il contatore
				nSingleQuot = 0;
			continue;
		}

		if (strSQL[nEnd] == _T('?'))
		{
			if (nSingleQuot == 0) //sono in presenza di un parametro 
			{
				strFilter += strSQL.Mid(nStart, nEnd - nStart);
				strParamName = m_pParamArray->GetParamName(nParamCount);
				strFilter += (strParamName.Find(_T("@"), 0) != 0) ? _T("@") + strParamName : strParamName;
				nStart = nEnd + 1;
				nParamCount++;
			}
		}
	}
	if (nEnd > nStart)
		strFilter += strSQL.Mid(nStart, nEnd - nStart);

	strSQL = strFilter;
}


//-----------------------------------------------------------------------------
void SqlRowSet::ExecutePagingCommand(MoveType eMoveType /*= E_MOVE_NEXT*/)
{
	ASSERT(m_bScrollable && m_nPageSize > 0);

	TRY
	{
		Connect(); //comunico che ho bisogno di connettermi al db
		TRACE_SQL(cwsprintf(_T("ExecutePagingCommand %s"), (LPCTSTR)GetParamStr(this)), this);
		START_DB_TIME(DB_EXECUTE_CMD)
		m_pRowSet->EnablePaging(m_nPageSize);
		m_pRowSet->SetCommandText(m_strSQL);
		m_pRowSet->ExecutePagingCommand(eMoveType, false);
		STOP_DB_TIME(DB_EXECUTE_CMD)
		Disconnect();
	}
		CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_EXECUTE_CMD)
			ThrowSqlException(e->m_strError);
	}
	END_CATCH
}



//-----------------------------------------------------------------------------
void SqlRowSet::ExecuteCommand()
{
	TRY
	{
		Connect(); //comunico che ho bisogno di connettermi al db
		TRACE_SQL(cwsprintf(_T("ExecuteCommand %s"), (LPCTSTR)GetParamStr(this)), this);
		START_DB_TIME(DB_EXECUTE_CMD)
		m_pRowSet->SetCommandText(m_strSQL);
		m_pRowSet->ExecuteCommand(false);
		STOP_DB_TIME(DB_EXECUTE_CMD)
		if (m_bScrollable)
			Disconnect();
	}
	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_EXECUTE_CMD)
		ThrowSqlException(e->m_strError);
	}
	END_CATCH	
}



//-----------------------------------------------------------------------------
HRESULT SqlRowSet::Prepare()
{ 	
	return S_OK; 
}

//-----------------------------------------------------------------------------
int SqlRowSet::ExecuteNonQuery()
{
	TRACE_SQL(cwsprintf(_T("ExecuteNoQuery %s"), (LPCTSTR)GetParamStr(this)), this);
	int nAffectedRow = 0;
	TRY
	{
		Connect(); //comunico che ho bisogno di connettermi al db
		START_DB_TIME(DB_EXECUTE_NOQUERY)
		m_pRowSet->SetCommandText(m_strSQL);
		nAffectedRow = m_pRowSet->ExecuteNonQuery(false);
		STOP_DB_TIME(DB_EXECUTE_NOQUERY)

		Disconnect();
	}
	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_EXECUTE_NOQUERY)
		ThrowSqlException(e);
	}
	END_CATCH

	return nAffectedRow;	
}


//-----------------------------------------------------------------------------
void SqlRowSet::ExecuteScalar()
{
	TRACE_SQL(cwsprintf(_T("ExecuteScalar %s"), (LPCTSTR)GetParamStr(this)), this);	

	TRY
	{
		Connect(); //comunico che ho bisogno di connettermi al db
		m_pRowSet->SetCommandText(m_strSQL);
		START_DB_TIME(DB_EXECUTE_SCALAR)
			m_pRowSet->ExecuteScalar(false); // bPrepare == TRUE);
		STOP_DB_TIME(DB_EXECUTE_SCALAR)
	
		Disconnect();
	}
	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_EXECUTE_SCALAR)
		ThrowSqlException(e);
	}
	END_CATCH
	
}

//-----------------------------------------------------------------------------
BOOL SqlRowSet::BindColumns()
{
	TRACE_SQL(_T("BindColumns"), this);

	TRY
	{
		START_DB_TIME(DB_BIND_COLUMNS)
		m_pRowSet->BindColumns(m_pColumnArray);
		STOP_DB_TIME(DB_BIND_COLUMNS)
	}

	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_BIND_COLUMNS)
		ThrowSqlException(e);
	}
	END_CATCH

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlRowSet::BindParameters()
{
	TRACE_SQL(_T("BindParameters"), this);

	TRY
	{
		START_DB_TIME(DB_BIND_PARAMS)
		m_pRowSet->BindParameters(m_pParamArray);
		STOP_DB_TIME(DB_BIND_PARAMS)
		START_DB_TIME(DB_SET_PARAMS_VALUE)
		m_pRowSet->SetParametersValues();
		STOP_DB_TIME(DB_SET_PARAMS_VALUE)
	
	}
	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_SET_PARAMS_VALUE)
		ThrowSqlException(e);
	}
	END_CATCH
	
	return TRUE;
}	

//-----------------------------------------------------------------------------
void SqlRowSet::FixupColumns()
{
	TRACE_SQL(_T("FixupColumns"), this);

	TRY
	{ 
		START_DB_TIME(DB_FIXUP_COLUMNS)
		m_pRowSet->FixupColumns();
		STOP_DB_TIME(DB_FIXUP_COLUMNS)
	}
	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_FIXUP_COLUMNS)
		ThrowSqlException(e);
	}
	END_CATCH
	
}

//-----------------------------------------------------------------------------
void SqlRowSet::InitColumns()
{
	TRACE_SQL(_T("InitBuffer"), this);
	START_PROC_TIME(PROC_INIT_BUFFER)
	m_pColumnArray->InitColumns();
	STOP_PROC_TIME(PROC_INIT_BUFFER)
}

//-----------------------------------------------------------------------------
BOOL SqlRowSet::FixupParameters()
{
	TRACE_SQL(_T("FixupParameters"), this);

	TRY
	{ 
		START_DB_TIME(DB_SET_PARAMS_VALUE)
		m_pRowSet->SetParametersValues();
		STOP_DB_TIME(DB_SET_PARAMS_VALUE)
	}
	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_SET_PARAMS_VALUE)
		ThrowSqlException(e);
	}
	END_CATCH
	
	return TRUE;
}

// esecuzione query e script
//-----------------------------------------------------------------------------
void SqlRowSet::ExecuteQuery(LPCTSTR lpszSQL)
{
	if (!IsOpen())
	{
		ASSERT(FALSE);
		return;
	}

	m_strSQL = lpszSQL;
	TRY
	{
		ExecuteNonQuery();
	}
	CATCH(MSqlException, e)
	{
		ThrowSqlException(e);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlRowSet::ExecuteScript(LPCTSTR lpszFileName)
{
	//@@TODO
	ASSERT(FALSE);
}

//Paging
//-----------------------------------------------------------------------------
void SqlRowSet::GetFirstPage()
{
	ASSERT(m_bScrollable && m_nPageSize > 0);
}

//-----------------------------------------------------------------------------
void SqlRowSet::GetNextPage()
{
	ASSERT(m_bScrollable && m_nPageSize > 0);
	TRY
	{
		Connect(); //comunico che ho bisogno di connettermi al db
		TRACE_SQL(cwsprintf(_T("GetNextPage %s"), (LPCTSTR)GetParamStr(this)), this);
		START_DB_TIME(DB_EXECUTE_CMD)
		m_pRowSet->GetNextPage();
		STOP_DB_TIME(DB_EXECUTE_CMD)
		Disconnect();
	}
	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_EXECUTE_CMD)
		ThrowSqlException(e->m_strError);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlRowSet::GetPrevPage()
{
	ASSERT(m_bScrollable && m_nPageSize > 0);
	TRY
	{
		Connect(); //comunico che ho bisogno di connettermi al db
		TRACE_SQL(cwsprintf(_T("GetPrevPage %s"), (LPCTSTR)GetParamStr(this)), this);
		START_DB_TIME(DB_EXECUTE_CMD)
		m_pRowSet->GetPrevPage();
		STOP_DB_TIME(DB_EXECUTE_CMD)
		Disconnect();
	}
	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_EXECUTE_CMD)
		ThrowSqlException(e->m_strError);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlRowSet::GetLastPage()
{
	ASSERT(m_bScrollable && m_nPageSize > 0);
	TRY
	{
		Connect(); //comunico che ho bisogno di connettermi al db
		TRACE_SQL(cwsprintf(_T("GetLastPage %s"), (LPCTSTR)GetParamStr(this)), this);
		START_DB_TIME(DB_EXECUTE_CMD)
			m_pRowSet->GetLastPage();
		Disconnect();
	}
	CATCH(MSqlException, e)
	{
		STOP_DB_TIME(DB_EXECUTE_CMD)
		ThrowSqlException(e->m_strError);
	}
	END_CATCH
}


/////////////////////////////////////////////////////////////////////////////
//						class SqlTable
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlTable, SqlRowSet)


//-----------------------------------------------------------------------------
SqlTable::SqlTable()
	:
	IDisposingSourceImpl		(this),
	SqlRowSet					(),
	m_pRecord					(NULL),
	m_pParamsRecord				(NULL)
{
	InitDataMember();
	Initialize();
}

//-----------------------------------------------------------------------------
SqlTable::SqlTable(SqlSession* pSqlSession)
	:
	IDisposingSourceImpl	(this),
	SqlRowSet				(pSqlSession),
	m_pRecord				(NULL),
	m_pParamsRecord			(NULL)
	
{
	InitDataMember();
	Initialize();
}

//-----------------------------------------------------------------------------
SqlTable::SqlTable(SqlRecord* pRecord, SqlSession* pSqlSession, /*=NULL*/CBaseDocument* pDocument /*NULL*/)
	:
	IDisposingSourceImpl		(this),
	SqlRowSet					(pSqlSession, pDocument),
	m_pRecord					(pRecord),
	m_pParamsRecord				(NULL)
{	
	InitDataMember();

	//cambio la connessione al SqlRecord
	// questo viene fatta se quella associata al record � differente da
	// quella passata come parametro
	// Il sqlrecord nasce con le info della connessione di default
	if (m_pSqlConnection)
		m_pRecord->SetConnection(m_pSqlConnection);

	Initialize();
	//il nome della tabella lo prendo dal SqlRecord
	if (m_pRecord->IsValid())
		m_strTableName = m_pRecord->GetTableName();
}

//-----------------------------------------------------------------------------
SqlTable::SqlTable(SqlRecordProcedure* pRecordParams, SqlSession* pSqlSession, /*=NULL*/CBaseDocument* pDocument /*NULL*/)
	:
	IDisposingSourceImpl	(this),
	SqlRowSet				(pSqlSession, pDocument),
	m_pRecord				(NULL),
	m_pParamsRecord			(pRecordParams)
{
	InitDataMember();

	//cambio la connessione al SqlRecord
	// questo viene fatta se quella associata al record � differente da
	// quella passata come parametro
	// Il sqlrecord nasce con le info della connessione di default
	if (m_pSqlConnection)
		pRecordParams->SetConnection(m_pSqlConnection);

	Initialize();
	//il nome della tabella lo prendo dal SqlRecord
	if (pRecordParams->IsValid())
		m_strTableName = pRecordParams->GetTableName();
}


//-----------------------------------------------------------------------------
SqlTable::SqlTable(SqlRecordProcedure* pRecordParams, SqlRecord* pSqlRecord, SqlSession* pSqlSession /*=NULL*/, CBaseDocument* pDocument /*NULL*/)
	:
	IDisposingSourceImpl	(this),
	SqlRowSet				(pSqlSession, pDocument),
	m_pRecord				(pSqlRecord),
	m_pParamsRecord			(pRecordParams)
{
	InitDataMember();

	//cambio la connessione al SqlRecord
	// questo viene fatta se quella associata al record � differente da
	// quella passata come parametro
	// Il sqlrecord nasce con le info della connessione di default
	if (m_pSqlConnection)
	{
		m_pParamsRecord->SetConnection(m_pSqlConnection);
		m_pRecord->SetConnection(m_pSqlConnection);
	}

	Initialize();
	//il nome della tabella lo prendo dal SqlRecord
	if (m_pParamsRecord->IsValid())
		m_strTableName = pRecordParams->GetTableName();

	if (m_pRecord->IsValid())
		m_strTableName = m_pRecord->GetTableName();
}

//-----------------------------------------------------------------------------
void SqlTable::InitDataMember()
{
	m_pUpdateRowSet = NULL;
	m_bDBTMasterQuery = FALSE;
	m_bForceAutocommit = FALSE;

	m_pKeysArray = NULL;
	m_pTableArray = NULL;
	m_pExtraColumns = NULL;

	m_bForceQuery = FALSE;
	m_pSymTable = NULL;
	m_bSkipContextBagParameters = FALSE;
	m_bSkipRowSecurity = FALSE;
	m_bSelectGrantInformation = FALSE;
	m_pRSSelectWorkerID = NULL;
	m_pRSFilterWorkerID = NULL;
	m_bOnlyOneRecordExpected = FALSE;
	m_pOldRecord = NULL;
}

//-----------------------------------------------------------------------------
void SqlTable::Initialize()
{
	// Extended Fetch return state
	m_bEOFSeen		= FALSE;
	m_bDeleted		= FALSE;

	// Query related status
	m_lCurrentRecord= SQL_CURRENT_RECORD_UNDEFINED;
	m_lRecordCount	= 0;
	m_nEditMode		= noMode;
	m_bBOF			= TRUE;
	m_bEOF			= TRUE;
	m_bFirstQuery	= TRUE;
	m_bKeyChanged	= FALSE;
	m_sqlFetchResult = FetchOk;
}

//-----------------------------------------------------------------------------
SqlTable::~SqlTable()
{
	if (IsOpen())
	{
		TRACE1("SqlTable::~SqlTable(): the table %s is opened.\n", (LPCTSTR)m_strTableName);
		ASSERT(FALSE);
		Close();
	}

	m_Handler.FireDisposing(this);

	if (m_pTableArray)
		delete m_pTableArray;

	if (m_pKeysArray)
		delete m_pKeysArray;

	if (m_pExtraColumns)
		delete m_pExtraColumns;
}

//-----------------------------------------------------------------------------
void SqlTable::SetSqlSession(SqlSession* pSqlSession)
{
	TRY
	{
		SqlRowSet::SetSqlSession(pSqlSession);
		if (m_pRecord)
			m_pRecord->SetConnection(m_pSqlConnection);
	}
	CATCH(MSqlException, e)
	{
		TRACE(e->m_strError);
		ThrowSqlException(e);
	}
	END_CATCH
}
//-----------------------------------------------------------------------------
void SqlTable::CheckRecord()
{
	// nel caso di collegamento ad un SqlRecord il nome lo prendo da lui
	// Viene fatto nuovamente in quanto una eventuale Close ha pulito vari parametri
	// tra cui questo
	if (m_pRecord)
	{
		if (!m_pRecord->IsValid())
		{
			SqlRowSet::Close();

			m_bErrorFound = TRUE;
			m_strErrorFound = 
				(
					(m_pRecord->GetTableInfo())
					? cwsprintf(_TB("SqlTable::Open: table {0-%s} in database {1-%s} is invalid"), m_pRecord->GetTableName(), m_pSqlConnection->m_strDBName)
					: cwsprintf(_TB("SqlTable::Open: table {0-%s} not in database {1-%s}"), m_pRecord->GetTableName(), m_pSqlConnection->m_strDBName)
				);
			TRACE(m_strErrorFound);
			//ASSERT(FALSE);
		}
		else
			m_strTableName = m_pRecord->GetTableName();
	}
}

//-----------------------------------------------------------------------------
void SqlTable::Open(BOOL bUpdatable, CursorType eCursorType)
{
	TRY
	{
		// il cursore di update lo devo creare sulla sessione di lavoro che mi � stata passata
		// vedi DBT legati alla sessione di Update di un documento
		// il cursore di update viene sempre aperto come FAST_FORWARD_ONLY
		// creo lo statement adesso altrimenti dopo la SqlRowSet::Open � possibile che una
		// session differente nel caso di utilizzo di cursori FORWARD_ONLY (x cui � necessario creare
		// una session x ogni cursore aperto
		if (bUpdatable)
			m_pUpdateRowSet = new SqlRowSet(m_pSqlSession->GetUpdatableSqlSession(), (m_pContext) ? m_pContext->GetDocument() : NULL);

		SqlRowSet::Open(bUpdatable, eCursorType);
		CheckRecord();		
	}

	CATCH(MSqlException, e)
	{
		m_bErrorFound = TRUE;
		m_strErrorFound = e->m_strError;
		TRACE(e->m_strError);
	}
	END_CATCH
}


//-----------------------------------------------------------------------------
void SqlTable::Open(BOOL bUpdatable /*=FALSE*/, BOOL bScrollable /*=FALSE*/, BOOL bSensitivity /*= TRUE*/)
{             
	// se � una view non devo permettere la visibilit� delle modifiche
	TRY
	{
		// il cursore di update lo devo creare sulla sessione di lavoro che mi � stata passata
		// vedi DBT legati alla sessione di Update di un documento
		// il cursore di update viene sempre aperto come FAST_FORWARD_ONLY
		// creo lo statement adesso altrimenti dopo la SqlRowSet::Open � possibile che una
		// session differente nel caso di utilizzo di cursori FORWARD_ONLY (x cui � necessario creare
		// una session x ogni cursore aperto
		if (bUpdatable)
			m_pUpdateRowSet = new SqlRowSet(m_pSqlSession->GetUpdatableSqlSession());
		SqlRowSet::Open(bUpdatable, bScrollable);
		CheckRecord();		
	}

	CATCH(MSqlException, e)
	{
		TRACE(e->m_strError);
		m_bErrorFound = TRUE;
		m_strErrorFound = e->m_strError;
	}
	END_CATCH
}



//-----------------------------------------------------------------------------
void SqlTable::ClearSQL()
{
	m_strSelectKeyword.Empty();	// Function without binding column (i.e. TOP 100, DISTICT)
	m_strFrom		.Empty();	// From clause with more tables
	m_strTableName	.Empty();	// From clause with only one tables   	
	m_strAliasName	.Empty();	// AS clause (tabella)
	m_strSort		.Empty();	// Order By Clause
	m_strGroupBy	.Empty();	// Group By Clause
	m_strHaving		.Empty();	// Having Clause
}

// deve evitare di usare la ClearQuery perche` il record potrebbe essere stato
// cancellato da fuori ma il puntatore non essere ancora NULL;
//-----------------------------------------------------------------------------
void SqlTable::Close()
{
	FreeUpdateRowSet	();
	Initialize			();
	ClearTables			();
	ClearKeys			();
	ClearExtraColunms	();
	ClearSQL			();

	SqlRowSet::Close();
}

//-----------------------------------------------------------------------------
void SqlTable::Disconnect()
{
	ASSERT_VALID(this);

	if (m_pUpdateRowSet)
		m_pUpdateRowSet->Disconnect();

	SqlRowSet::Disconnect();
}


//-----------------------------------------------------------------------------
void SqlTable::ClearTables	()	{ if (m_pTableArray) m_pTableArray->RemoveAll(); }

//-----------------------------------------------------------------------------
void SqlTable::ClearKeys		()	{ if (m_pKeysArray) m_pKeysArray->RemoveAll(); }

//-----------------------------------------------------------------------------
void SqlTable::ClearExtraColunms()	{ if (m_pExtraColumns) m_pExtraColumns->RemoveAll(); }


// Pulisce tutti i parametri, le colonne e le stringhe preposte alla query
//-----------------------------------------------------------------------------
void SqlTable::ClearQuery()
{
	Initialize			();
	ClearTables			();
	ClearKeys			();
	ClearExtraColunms	();
	ClearSQL			();
	ClearRowSet			();
	// nel caso di collegamento ad un SqlRecord il nome lo prendo da lui
	if (m_pRecord && m_pRecord->IsValid())
		m_strTableName = m_pRecord->GetTableName();	
}

//-----------------------------------------------------------------------------
BOOL SqlTable::SameQuery() const
{
	return	m_pSqlConnection->m_bOptimizedHKL &&
		m_strSQL.CompareNoCase(m_strOldSQL) == 0 &&
		m_pParamArray->SameValues();
}
//-----------------------------------------------------------------------------
CString SqlTable::GetColumnName (const DataObj* pColumnDataObj)
{ 
	if (!m_pRecord)
		ThrowSqlException(_TB("SqlTable::GetColumnNam: no SqlRecord connected to the table."));
		
	CHECK_BUILD_ERROR
	(
		return m_pRecord->GetColumnName(pColumnDataObj);
	)
	return _T("");
}

//-----------------------------------------------------------------------------
CString SqlTable::GetQualifiedColumnName (const DataObj* pColumnDataObj)
{ 
	if (!m_pRecord)
	{
		m_bErrorFound = TRUE;
		m_strErrorFound  = _T("SqlTable::GetColumnName no SqlRecord connected to the table.");
		TRACE(m_strErrorFound);
		ASSERT(FALSE);
		return _T("");
	}
		
	CHECK_BUILD_ERROR
	(
		return m_pRecord->GetQualifiedColumnName(pColumnDataObj);
	)	
	return _T("");
}

// se � qualificata devo utilizzare la qualifica anche nella group by
//-----------------------------------------------------------------------------
void SqlTable::AddGroupByColumn(const DataObj& aDataObj)
{
	AddGroupByColumn(GetQualifiedColumnName(&aDataObj));
}

// per permettere la groupby in caso di selezioni da + tabella
//-----------------------------------------------------------------------------
void SqlTable::AddGroupByColumn(SqlRecord* pRecord, const DataObj& aDataObj)
{
	ASSERT(pRecord);
	CHECK_BUILD_ERROR
	(
		AddGroupByColumn(pRecord->GetQualifiedColumnName(&aDataObj));
	)
}

//-----------------------------------------------------------------------------
void SqlTable::AddGroupByColumn(const CString& strColumnName)
{
	if (!m_strGroupBy.IsEmpty()) m_strGroupBy += szComma;
	m_strGroupBy += strColumnName;

	// se esiste una group by nella select allora non posso fare
	// nessun update del database (in particolar modo se sono in KeyedUpdate)
	m_bUpdatable = FALSE;
}

// Sintassi : Articolo, Deposito DESC
// pu� essere anche qualificata x.Articolo
//-----------------------------------------------------------------------------
void SqlTable::AddSortColumn(const DataObj& aDataObj, BOOL bDescending/*= FALSE*/)
{
	AddSortColumn(GetQualifiedColumnName(&aDataObj), bDescending);
}

// per un campo di un'altro record (vedi query tra + tabelle)
//-----------------------------------------------------------------------------
void SqlTable::AddSortColumn(SqlRecord* pRecord, const DataObj& aDataObj, BOOL bDescending/*= FALSE*/)
{
	ASSERT(pRecord);
	CHECK_BUILD_ERROR
	(
		AddSortColumn(pRecord->GetQualifiedColumnName(&aDataObj), bDescending);
	)
}

//-----------------------------------------------------------------------------
void SqlTable::AddSortColumn(const CString& strColumnName, BOOL bDescending/*= FALSE*/)
{
	if (!m_strSort.IsEmpty()) m_strSort += szComma;
	m_strSort += strColumnName;
	if (bDescending) m_strSort += " DESC";
}

//-----------------------------------------------------------------------------
void SqlTable::AddFilterColumn(const DataObj& aDataObj, const CString& strOperator /*= ""*/)
{
	AddFilterColumn(GetQualifiedColumnName(&aDataObj), strOperator);
}

//-----------------------------------------------------------------------------
void SqlTable::AddFilterColumn(SqlRecord* pRecord, const DataObj& aDataObj, const CString& strOperator /*= ""*/)
{
	ASSERT(pRecord);
	CHECK_BUILD_ERROR
	(
		AddFilterColumn(pRecord->GetQualifiedColumnName(&aDataObj), strOperator);
	)
}

//-----------------------------------------------------------------------------
void SqlTable::AddFilterColumn(const CString& strColumnName, const CString& strOperator /*= ""*/)
{
	if (!m_strFilter.IsEmpty()) m_strFilter += _T(" AND ");

	CString strOper = (strOperator.IsEmpty()) ? _T("=") : strOperator;
	m_strFilter += strColumnName + _T(" ") + strOper + _T(" ?");
}

//-----------------------------------------------------------------------------
void SqlTable::AddCompareColumn(const DataObj& aDataObj, SqlRecord* pRecord, const DataObj& aCompareDataObj, const CString& strOperator /*= ""*/)
{
	ASSERT(pRecord);
	if (pRecord)
		AddCompareColumn(GetQualifiedColumnName(&aDataObj), pRecord->GetQualifiedColumnName(&aCompareDataObj), strOperator);
	}

//-----------------------------------------------------------------------------
void SqlTable::AddCompareColumn(SqlRecord* pRecord1, const DataObj& aCompareDataObj1, SqlRecord* pRecord2, const DataObj& aCompareDataObj2, const CString& strOperator /*= ""*/)
{
	ASSERT(pRecord1);
	ASSERT(pRecord2);
		if (pRecord1 && pRecord2)
		AddCompareColumn(pRecord1->GetQualifiedColumnName(&aCompareDataObj1), pRecord2->GetQualifiedColumnName(&aCompareDataObj2), strOperator);
}

//-----------------------------------------------------------------------------
void SqlTable::AddCompareColumn(const CString& strColumnName1, CString& strColumnName2, const CString& strOperator /*= ""*/)
		{
			if (!m_strFilter.IsEmpty()) m_strFilter += _T(" AND ");
			CString strOper = (strOperator.IsEmpty()) ? _T("=") : strOperator;
	m_strFilter += strColumnName1 + _T(" ") + strOper + _T(" ") + strColumnName2;	
}

//-----------------------------------------------------------------------------
void SqlTable::AddBetweenColumn(const DataObj& aDataObj)
{
	AddBetweenColumn(GetQualifiedColumnName(&aDataObj));
}

//-----------------------------------------------------------------------------
void SqlTable::AddBetweenColumn(SqlRecord* pRecord, const DataObj& aDataObj)
{
	ASSERT(pRecord);
		AddBetweenColumn(pRecord->GetQualifiedColumnName(&aDataObj));
}

//-----------------------------------------------------------------------------
void SqlTable::AddBetweenColumn(const CString& strColumnName)
{
	if (!m_strFilter.IsEmpty()) m_strFilter += _T(" AND ");
	m_strFilter += _T("( ") + strColumnName + _T(" BETWEEN ? AND ? )");
}

//-----------------------------------------------------------------------------
void SqlTable::AddFilterLike(const DataObj& aDataObj)
{
	AddFilterLike(GetQualifiedColumnName(&aDataObj));
}

//-----------------------------------------------------------------------------
void SqlTable::AddFilterLike(const CString& strColumnName)
{
	if (!m_strFilter.IsEmpty()) m_strFilter += _T(" AND ");
	m_strFilter += strColumnName + _T(" LIKE ?");
}

//-----------------------------------------------------------------------------
void SqlTable::SetFilter(UINT IDfmt, ...)
{
	CString strFmt;
	CString strBuffer;

	if (AfxLoadTBString(strFmt, IDfmt))
		return;
		
	va_list marker;
	
    va_start( marker, IDfmt );
	strBuffer = cwvsprintf(strFmt, marker);
    va_end( marker );

	m_strFilter = strBuffer;
}

//-----------------------------------------------------------------------------
void SqlTable::SetFilter(LPCTSTR strFmt, ...)
{
	va_list marker;
	
    va_start( marker, strFmt );
	CString strBuffer = cwvsprintf(strFmt, marker);
    va_end( marker );

	m_strFilter = strBuffer;
}

//-----------------------------------------------------------------------------
void SqlTable::SetForceQuery (BOOL bSet)
{ 
	m_bForceQuery = bSet; 
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingFilterColumn(const DataObj& aDataObj, const CString& strOperator /*= ""*/)
{
	AddHavingFilterColumn(GetQualifiedColumnName(&aDataObj), strOperator);
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingFilterColumn(SqlRecord* pRecord, const DataObj& aDataObj, const CString& strOperator /*= ""*/)
{
	ASSERT(pRecord);
	CHECK_BUILD_ERROR
	(
		AddHavingFilterColumn(pRecord->GetQualifiedColumnName(&aDataObj), strOperator);
	)
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingFilterColumn(const CString& strColumnName, const CString& strOperator /*= ""*/)
{
	if (!m_strHaving.IsEmpty()) m_strHaving += _T(" AND ");

	CString strOper = (strOperator.IsEmpty()) ? _T("=") : strOperator;
	m_strHaving += strColumnName + _T(" ") + strOper + _T(" ?");
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingCompareColumn(const DataObj& aDataObj, SqlRecord* pRecord, const DataObj& aCompareDataObj, const CString& strOperator /*= ""*/)
{
	ASSERT(pRecord);
	if (pRecord)
	{
		if (!m_strHaving.IsEmpty()) m_strHaving += _T(" AND ");
		CString strOper = (strOperator.IsEmpty()) ? _T("=") : strOperator;
		m_strHaving += GetQualifiedColumnName(&aDataObj) + _T(" ") + strOper + _T(" ") + pRecord->GetQualifiedColumnName(&aCompareDataObj);
	}
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingCompareColumn(SqlRecord* pRecord1, const DataObj& aCompareDataObj1, SqlRecord* pRecord2, const DataObj& aCompareDataObj2, const CString& strOperator /*= ""*/)
{
	ASSERT(pRecord1);
	ASSERT(pRecord2);
	CHECK_BUILD_ERROR
	(
		if (pRecord1 && pRecord2)
		{
			if (!m_strHaving.IsEmpty()) m_strHaving += _T(" AND ");
			CString strOper = (strOperator.IsEmpty()) ? _T("=") : strOperator;
			m_strHaving += pRecord1->GetQualifiedColumnName(&aCompareDataObj1) + _T(" ") + strOper + _T(" ") + pRecord2->GetQualifiedColumnName(&aCompareDataObj2);
		}
	)
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingBetweenColumn(const DataObj& aDataObj)
{
	AddHavingBetweenColumn(GetQualifiedColumnName(&aDataObj));
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingBetweenColumn(SqlRecord* pRecord, const DataObj& aDataObj)
{
	ASSERT(pRecord);
	CHECK_BUILD_ERROR
	(
		AddHavingBetweenColumn(pRecord->GetQualifiedColumnName(&aDataObj));
	)
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingBetweenColumn(const CString& strColumnName)
{
	if (!m_strHaving.IsEmpty()) m_strHaving += _T(" AND ");
	m_strHaving += _T("( ") + strColumnName + _T(" BETWEEN ? AND ? )");
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingFilterLike(const DataObj& aDataObj)
{
	AddHavingFilterLike(GetQualifiedColumnName(&aDataObj));
}

//-----------------------------------------------------------------------------
void SqlTable::AddHavingFilterLike(const CString& strColumnName)
{
	if (!m_strHaving.IsEmpty()) m_strHaving += _T(" AND ");
	m_strHaving += strColumnName + _T(" LIKE ?");
}

//-----------------------------------------------------------------------------
void SqlTable::SetHavingFilter(UINT IDfmt, ...)
{
	CString strFmt;
	CString strBuffer;

	if (AfxLoadTBString(strFmt, IDfmt))
		return;
		
	va_list marker;
	
    va_start( marker, IDfmt );
	strBuffer = cwvsprintf(strFmt, marker);
    va_end( marker );

	m_strHaving = strBuffer;
}

//-----------------------------------------------------------------------------
void SqlTable::SetHavingFilter(LPCTSTR strFmt, ...)
{
	va_list marker;
	
    va_start( marker, strFmt );
	CString strBuffer = cwvsprintf(strFmt, marker);
    va_end( marker );

	m_strHaving = strBuffer;
}

//=============================================================================

//-----------------------------------------------------------------------------
LPCTSTR SqlTable::GetAlias() const
{
	return (m_pSqlConnection->GetDBMSType() == DBMS_ORACLE) ? _T(" ") : szAs;
}

//-----------------------------------------------------------------------------
void SqlTable::FromTable(SqlRecord* pRec, const CString* pStrAlias /*= NULL*/)
{
	ASSERT_VALID(pRec);
	FromTable(pRec->GetTableName(), pStrAlias ? *pStrAlias : pRec->m_strQualifier, pRec);
}

//-----------------------------------------------------------------------------
void SqlTable::FromTable(const CString& strTableName, const CString& strAlias, SqlRecord* pRec /*=NULL*/)
{
	if (!m_pTableArray)
	{
		m_pTableArray = new SqlTableArray(m_pSqlConnection);
#ifdef __DEBUG
		if (pRec && m_pRecord)
		{
			ASSERT_VALID(pRec);
			ASSERT_VALID(m_pRecord);
			ASSERT(pRec->GetRuntimeClass() == m_pRecord->GetRuntimeClass());
		}
#endif
	}

	// inserisco le info che mi possono servire in futuro
	m_pTableArray->Add(new SqlTableItem(strTableName, strAlias, pRec));
}

//-----------------------------------------------------------------------------
void SqlTable::LeftOuterJoin(SqlRecord* pJoinRecord, const DataObj& onDataObj1,  const DataObj& onDataObj2)
{
	LeftOuterJoin(m_pRecord, pJoinRecord, onDataObj1, onDataObj2);
}

//-----------------------------------------------------------------------------
void SqlTable::RightOuterJoin(SqlRecord* pJoinRecord,const DataObj& onDataObj1,  const DataObj& onDataObj2)
{
	RightOuterJoin(m_pRecord, pJoinRecord, onDataObj1, onDataObj2);
}

//-----------------------------------------------------------------------------
void SqlTable::LeftOuterJoin(SqlRecord* pRecord, SqlRecord* pJoinRecord, const DataObj& onDataObj1, const DataObj& onDataObj2)
{
	CString strLeftOuterJoin = cwsprintf(
		_T(" LEFT OUTER JOIN %s ON  %s =  %s "),
		pJoinRecord->m_strQualifier,
		pRecord->GetQualifiedColumnName(&onDataObj1),
		pJoinRecord->GetQualifiedColumnName(&onDataObj2));

	if (!m_pTableArray)
	{
		m_pTableArray = new SqlTableArray(m_pSqlConnection);

#ifdef __DEBUG
		if (pJoinRecord && m_pRecord)
		{
			ASSERT_VALID(pJoinRecord);
			ASSERT_VALID(m_pRecord);
			ASSERT(pJoinRecord->GetRuntimeClass() == m_pRecord->GetRuntimeClass());
		}
#endif
	}

	// inserisco le info che mi possono servire in futuro
	m_pTableArray->Add(new SqlTableItem(pJoinRecord->GetTableName(), pJoinRecord->m_strQualifier, pJoinRecord, strLeftOuterJoin));
}

//-----------------------------------------------------------------------------
void SqlTable::RightOuterJoin(SqlRecord* pRecord, SqlRecord* pJoinRecord, const DataObj& onDataObj1,  const DataObj& onDataObj2)
{
	CString strRightOuterJoin = cwsprintf(
		_T(" RIGHT OUTER JOIN %s ON  %s =  %s "),
		pJoinRecord->m_strQualifier,
		pRecord->GetQualifiedColumnName(&onDataObj1),
		pJoinRecord->GetQualifiedColumnName(&onDataObj2));

	if (!m_pTableArray)
	{
		m_pTableArray = new SqlTableArray(m_pSqlConnection);

#ifdef __DEBUG
		if (pJoinRecord && m_pRecord)
		{
			ASSERT_VALID(pJoinRecord);
			ASSERT_VALID(m_pRecord);
			ASSERT(pJoinRecord->GetRuntimeClass() == m_pRecord->GetRuntimeClass());
		}
#endif
	}

	// inserisco le info che mi possono servire in futuro
	m_pTableArray->Add(new SqlTableItem(pJoinRecord->GetTableName(), pJoinRecord->m_strQualifier, pJoinRecord, strRightOuterJoin));
}


//-----------------------------------------------------------------------------
void SqlTable::AddUpdateKey(const CString& strColumnName)
{
	// il programmatore mi pu� quali colonne utilizzare per la KeyedUpdate e KeyedDelete
	// altrimenti vengono considerati i segmenti di chiave primaria
	if (!m_pKeysArray)
		m_pKeysArray = new CStringArray;
	m_pKeysArray->Add(strColumnName);
}


//-----------------------------------------------------------------------------
long SqlTable::GetRowSetCount() 
{
	//se ho un valore corretto restituito dal rows allora utilizzo quello
	if (m_strSQL.IsEmpty())
		return 0;

	//se � un cursore scrollabile oppure in caso di forwardonly ho terminato il ciclo di fetch
	if (m_bScrollable || m_bEOF || m_bUpdatable)
		return m_pRowSet->GetRecordsAffected();

	//eseguo una select count(*) from (m_strSQL tolta la ORDER BY)
	return m_pRowSet->GetTotalRecords();

}


// TypeMove pu� essere
// E_MOVE_FIRST
// E_MOVE_LAST
// E_MOVE_NEXT
// E_MOVE_PREV
//-----------------------------------------------------------------------------
void SqlTable::Move(MoveType eTypeMove, int lSkip /*= 0*/)
{
	m_sqlFetchResult = FetchOk;
	BOOL	 bForward = TRUE;
	
	int moveTime = DB_MOVE_NEXT;
	TRY
	{
		ASSERT(IsOpen());

		if (m_bErrorFound)
			ThrowSqlException(cwsprintf(_TB("SqlTable::Move:errors occurred in the command construction phase {0-%s}.\r\nUnable to carry out the move requested."), m_strSQL));

		// Esce da un evetuale stato di Edit o AddNew
		m_nEditMode = noMode;

		if (eTypeMove == E_MOVE_REFRESH || (m_bEOF && m_bBOF))
			return;
			
		switch (eTypeMove)
		{
		case E_MOVE_NEXT:
			if (m_bEOF)
			{
				TRACE0("Error: attempted to move past EOF.\n");
				ThrowSqlException(_TB("SqlTable::Move: attempt to position after end of table."));
			}
			TRACE_SQL(_T("MoveNext"), this);
			START_DB_TIME(DB_MOVE_NEXT)
			moveTime = DB_MOVE_NEXT;
			m_sqlFetchResult = m_pRowSet->Move(E_MOVE_NEXT, lSkip);
			STOP_DB_TIME(DB_MOVE_NEXT)				
			break;

		case E_MOVE_PREV:
			if (!m_bScrollable)
			{
				TRACE0("Error: MovePrev using a forward cursor .\n");
				ThrowSqlException(_TB("SQlTable::Move: if using a forward cursor.\r\nUnable to perform shift on previous record"));
			}
			if (m_bBOF)
			{
				TRACE0("Error: attempted to move before BOF.\n");
				ThrowSqlException(_TB("SqlTable::Move: attempt to position before start of table."));
			}

			TRACE_SQL(_T("MovePrev"), this);
			START_DB_TIME(DB_MOVE_PREV)
			moveTime = DB_MOVE_PREV;
			m_sqlFetchResult = m_pRowSet->Move(E_MOVE_PREV, lSkip);
			STOP_DB_TIME(DB_MOVE_PREV)
			bForward = FALSE;
			break;

		case E_MOVE_FIRST:
			TRACE_SQL(_T("MoveFirst"), this);
			if (!m_bScrollable)
			{
				Query();
				return;
			}
			START_DB_TIME(DB_MOVE_FIRST)
			moveTime = DB_MOVE_FIRST;
			m_sqlFetchResult = m_pRowSet->Move(E_MOVE_FIRST);
			STOP_DB_TIME(DB_MOVE_FIRST)
			break;

		case E_MOVE_LAST:
			if (!m_bScrollable)
			{
				TRACE0("Error: MoveLast using a forward cursor.\n");
				ThrowSqlException(_TB("SqlTable::Move: if using a forward cursor.\r\nUnable to perform shift on last record in the rowset."));
			}
			TRACE_SQL(_T("MoveLast"), this);
			START_DB_TIME(DB_MOVE_LAST)
			moveTime = DB_MOVE_LAST;
			m_sqlFetchResult = m_pRowSet->Move(E_MOVE_LAST);
			STOP_DB_TIME(DB_MOVE_LAST)
			bForward = FALSE;
			break;
		}

		switch (m_sqlFetchResult)
		{
			case FetchOk:
				FixupColumns();
				if (eTypeMove == E_MOVE_FIRST)
					m_lCurrentRecord = 0;
				else if (eTypeMove == E_MOVE_LAST)
				{
					if (m_bEOFSeen)
						m_lCurrentRecord = m_lRecordCount - 1;
					else
						m_lRecordCount = m_lCurrentRecord = SQL_CURRENT_RECORD_UNDEFINED;
				}
				else if (m_lCurrentRecord != SQL_CURRENT_RECORD_UNDEFINED)
				{
					if (bForward && !m_bScrollable)
						m_lCurrentRecord++;
					else
						// If past end, current record already decremented
						if (!m_bEOF && !m_bScrollable)
							m_lCurrentRecord--;
				}
				// Must not be at EOF/BOF anymore
				m_bEOF = m_bBOF = FALSE;
				// mette non dirty per ottimizzare gli update e modified per il refresh dei controls
				if (m_pRecord) m_pRecord->SetFlags(FALSE, TRUE);

				// aggiusta il contatore di righe viste (contatore annacquato)
				if (m_lCurrentRecord + 1 > m_lRecordCount && !m_bScrollable)
					m_lRecordCount = m_lCurrentRecord + 1;
				break;

			case EndOfRowSet:
				//InitColumns();
				if (m_pRecord) m_pRecord->SetFlags(FALSE, TRUE);

				// hit end of set
				m_bEOF = TRUE;
				// If current record is known
				if (m_lCurrentRecord != SQL_CURRENT_RECORD_UNDEFINED)
				{
					m_bEOFSeen = TRUE;
					if (!m_bScrollable)
						m_lRecordCount = m_lCurrentRecord + 1;
				}
				break;

			case BeginOfRowSet:
				//InitColumns();
				if (m_pRecord) m_pRecord->SetFlags(FALSE, TRUE);

				m_bBOF = TRUE;
				m_lCurrentRecord = SQL_CURRENT_RECORD_BOF;
				break;

			case Error:
				InitColumns();
				STOP_DB_TIME(moveTime)
				m_pRowSet->Dispose();
				ThrowSqlException(_TB("Error in fetch operation"), this);
				break;
		}

		STOP_DB_TIME(moveTime);
		// non ho pi� record da estrarre, mi disconnetto
		if ((!m_bScrollable && m_bEOF) || m_bOnlyOneRecordExpected)
			Disconnect();
	}
	
	CATCH(MSqlException, e)
	{
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		STOP_DB_TIME(moveTime);
		m_pRowSet->Dispose();
		ThrowSqlException(e);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
BOOL SqlTable::MoveNextCopy (SqlRecord* pRecord)
{
 	ASSERT(IsOpen()); 
 	ASSERT(pRecord);
 	ASSERT(pRecord->GetRuntimeClass() == m_pRecord->GetRuntimeClass());

	TRY
	{ 	
 		SqlTable::Move(E_MOVE_NEXT);
 	}
 	CATCH (MSqlException, e)
 	{
		AfxMessageBox(e->m_strError);
		ThrowSqlException(e);
 	}
 	END_CATCH

	// Is EOF
	if (IsEOF())
		return FALSE;
	
 	// all work fine
 	*pRecord = *m_pRecord;
 	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlTable::MovePrevCopy (SqlRecord* pRecord)
{
 	ASSERT(IsOpen()); 
 	ASSERT(pRecord);
 	ASSERT(pRecord->GetRuntimeClass() == m_pRecord->GetRuntimeClass());

	TRY
	{ 	
 		SqlTable::Move(E_MOVE_PREV);
 	}
 	CATCH (SqlException, e)
 	{
		AfxMessageBox(e->m_strError);
		ThrowSqlException(e);
 	}
 	END_CATCH

	// Is BOF
	if (IsBOF())
		return FALSE;
	
 	// all work fine
 	*pRecord = *m_pRecord;
 	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlTable::MoveCopy(MoveType eTypeMove, SqlRecord* pRecord)
{
 	ASSERT(IsOpen()); 
 	ASSERT(pRecord);
 	ASSERT(pRecord->GetRuntimeClass() == m_pRecord->GetRuntimeClass());

	TRY
	{
 		SqlTable::Move(eTypeMove);
 	}
 	CATCH (MSqlException, e)
 	{
		AfxMessageBox(e->m_strError);
		ThrowSqlException(e);
 	}
 	END_CATCH
 	
 	// all work fine
 	*pRecord = *m_pRecord;
}

//-----------------------------------------------------------------------------
void SqlTable::ClearColumns()
{
	if (!m_pColumnArray) return;

	m_pColumnArray->Clear();
}

// E' significativa se la tabella e' connessa ad un record. Altrimenti
// usare le funzioni di select indicando sia il buffer (DataObj) che il nome
// della colonna.
//-----------------------------------------------------------------------------
void SqlTable::SelectAll()
{
	if (!m_pRecord) 
		return;
	ASSERT_VALID(m_pRecord);

	m_pColumnArray->Clear();

	//CHECK_BUILD_ERROR
	TRY
	{
		for (int nIdx = 0; nIdx <= m_pRecord->GetUpperBound(); nIdx++)
		{
			if (m_pRecord->IsVirtual(nIdx))
				continue;
			Select(m_pRecord, nIdx);
		}
	}	
	CATCH(MSqlException, e)
	{
		e->UpdateErrorString(cwsprintf(SqlErrorString::SQL_ERROR_BUILD_QUERY(), m_pRecord->GetTableName()), FALSE);
		m_bErrorFound = TRUE;
		m_strErrorFound = e->m_strError;
		TRACE(e->m_strError);
	}
	END_CATCH
}

// come argomento gli passo l'istanza della classe derivata da SqlRecord su
// cui effettuare la select
//-----------------------------------------------------------------------------
void SqlTable::SelectAll(SqlRecord* pRecord)
{
	if (!pRecord) 
		return;
	ASSERT_VALID(pRecord);

	CHECK_BUILD_ERROR
	(
		for (int nIdx = 0; nIdx <= pRecord->GetUpperBound(); nIdx++)
		{
			if (pRecord->IsVirtual(nIdx))
				continue;
			Select(pRecord, nIdx);
		}
	)
}

//-----------------------------------------------------------------------------
void SqlTable::SelectAllExceptFields(CStringArray* pExceptedFieldName)
{
	if (!m_pRecord) return;

	m_pColumnArray->Clear();

	SelectAllExceptFields(m_pRecord, pExceptedFieldName);
}

//-----------------------------------------------------------------------------
void SqlTable::SelectAllExceptFields(SqlRecord* pRecord, CStringArray* pExceptedFieldName)
{
	if (!pRecord) return;
	CString colName;
	BOOL bExclude = FALSE;
	//CHECK_BUILD_ERROR
	TRY
	{
		for (int nIdx = 0; nIdx <= pRecord->GetUpperBound(); nIdx++)
		{
			if (pRecord->IsVirtual(nIdx))
				continue;

			colName = pRecord->GetColumnName(nIdx); 
			bExclude = FALSE;
			//controllo se il campo � tra gli esclusi
			for (int nExc = 0; nExc <= pExceptedFieldName->GetUpperBound(); nExc++)
			{
				if (pExceptedFieldName->GetAt(nExc).CollateNoCase(colName) == 0)
				{
					bExclude = TRUE;
					break;
				}
			}
			if (!bExclude)
				Select(pRecord, nIdx);

		}
	}
	CATCH(MSqlException, e)
	{
		e->UpdateErrorString(cwsprintf(SqlErrorString::SQL_ERROR_BUILD_QUERY(), pRecord->GetTableName()), FALSE);
		m_bErrorFound = TRUE;
		m_strErrorFound = e->m_strError;
		TRACE(e->m_strError);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlTable::SelectAllExceptFields(DataObjArray* pExceptedDataObj)
{
	if (!m_pRecord) return;

	m_pColumnArray->Clear();
	SelectAllExceptFields(m_pRecord, pExceptedDataObj);
}
//-----------------------------------------------------------------------------
void SqlTable::SelectAllExceptFields(SqlRecord* pRecord, DataObjArray* pExceptedDataObj)
{

	if (!pRecord) return;
	DataObj* pDataObj = NULL;

	BOOL bExclude = FALSE;
	//CHECK_BUILD_ERROR
	TRY
	{
		for (int nIdx = 0; nIdx <= pRecord->GetUpperBound(); nIdx++)
		{
			if (pRecord->IsVirtual(nIdx))
				continue;

			pDataObj = pRecord->GetDataObjAt(nIdx);
			bExclude = FALSE;
			//controllo se il campo � tra gli esclusi
			for (int nExc = 0; nExc <= pExceptedDataObj->GetUpperBound(); nExc++)
			{				
				if (pExceptedDataObj->GetAt(nExc) == pDataObj)
				{
					bExclude = TRUE;
					break;
				}
			}
			if (!bExclude)
				Select(pRecord, nIdx);
		}
	}
	CATCH(MSqlException, e)
	{
		e->UpdateErrorString(cwsprintf(SqlErrorString::SQL_ERROR_BUILD_QUERY(), pRecord->GetTableName()), FALSE);
		m_bErrorFound = TRUE;
		m_strErrorFound = e->m_strError;
		TRACE(e->m_strError);
	}
	END_CATCH
}


//-----------------------------------------------------------------------------
CString SqlTable::GetAllTableName () const	// usato in caso di From da + tabelle
{
	if (!m_pTableArray && (!m_pRecord || !m_pRecord->GetExtensions()))
		return GetTableName();

	CString sFrom;	
	const RecordArray* ar =	m_pRecord->GetExtensions ();

	if (m_pTableArray && (!ar || (m_pTableArray->GetSize() > 1)))
	{
		ASSERT_VALID(m_pTableArray);
		ASSERT(m_pTableArray->GetSize());

		for (int t = 0; t <= m_pTableArray->GetUpperBound(); t++)
		{
			if (t) sFrom += L", ";

			sFrom += m_pTableArray->GetTableName(t);
		}
		sFrom.TrimRight(L", ");
		return sFrom;
	}

	//if (m_strFrom.Find(L"OUTER") > 0)	//find NO CASE
	//{
	//	ASSERT(FALSE);
	//	return m_strFrom;
	//}

	sFrom = m_pRecord->GetTableName();

	for (int i = 0; i < ar->GetSize(); i++)
	{
		SqlRecord* pRecEx = ar->GetAt(i);
		ASSERT_VALID(pRecEx);

		sFrom += L", " + pRecEx->GetTableName();
	}
	sFrom.TrimRight(L", ");
	return sFrom;
}

//-----------------------------------------------------------------------------
void SqlTable::SelectFromAllTable()
{
	if (!m_pRecord) 
		return;
	ASSERT_VALID(m_pRecord);

	SelectAll();
	
	if (!m_pTableArray)
		return; 

	SqlRecord* pRec = NULL;
	CHECK_BUILD_ERROR
	(
		for (int r = 0; r <= m_pTableArray->GetUpperBound(); r++)
		{
			pRec = m_pTableArray->GetRecord(r); 
			if(pRec == m_pRecord || pRec == NULL)
				continue;

			for (int nIdx = 0; nIdx <= pRec->GetUpperBound(); nIdx++)
			{
				if (pRec->IsVirtual(nIdx))
					continue;

				Select(pRec, nIdx);
			}
		}
	)
}

// Da usarsi se connesso ad uno specifico record
//-----------------------------------------------------------------------------
void SqlTable::Select(DataObj* pDataObj)
{
	ASSERT_VALID(m_pRecord);
	Select(m_pRecord, pDataObj);		
}

//-----------------------------------------------------------------------------
void SqlTable::Select(DataObj& aDataObj)
{
	Select(&aDataObj);
}

//-----------------------------------------------------------------------------
void SqlTable::Select(SqlRecord* pRecord, int nIdx, int nInsertPos /*= -1*/)
{
	ASSERT(pRecord);
	if (!pRecord)
		return;
	CHECK_BUILD_ERROR
	(
		SqlRecordItem* pRecItem = pRecord->GetAt(nIdx);
	if (!pRecItem)
		return;
	CString sQualifiedColumnName = pRecord->GetQualifiedColumnName(pRecItem->m_pColumnInfo);
	DataObj* pDataObj = pRecItem->GetDataObj();

	if (pRecItem->m_pColumnInfo->m_bNativeColumnExpr)
	{
		//expand sQualifiedColumnName
		if (m_pSymTable)
			ExpandContentOfClause(sQualifiedColumnName, m_pSymTable, pRecord->GetConnection());
	}

	m_pColumnArray->AddColumn
	(
		sQualifiedColumnName,
		pDataObj,
		nIdx,
		pRecItem->m_pColumnInfo->m_bAutoIncrement == TRUE,
		nInsertPos
	);
	)
}


//-----------------------------------------------------------------------------
void SqlTable::Select(SqlRecord* pRecord, DataObj* pDataObj, int nInsertPos /*= -1*/)
{
	ASSERT_VALID(pRecord);
	CHECK_BUILD_ERROR
	(
		if (pRecord)
		{
			int nIdx = pRecord->Lookup(pDataObj);
			if (nIdx < 0) return;

			int nCol = m_pColumnArray->AddColumn
			(
				pRecord->GetQualifiedColumnName(nIdx),
				pDataObj,
				nIdx,
				pRecord->IsAutoIncrement(nIdx) == TRUE,
				nInsertPos
			);
		}
	)
}

//-----------------------------------------------------------------------------
void SqlTable::Select(SqlRecord* pRecord, DataObj& aDataObj, int nInsertPos /*= -1*/)
{
	Select(pRecord, &aDataObj, nInsertPos);
}

//-----------------------------------------------------------------------------
void SqlTable::Select(const CString& strColumnName, DataObj* pDataObj, int nAllocSize /*= 0*/, BOOL bAutoIncrement /*=FALSE*/, int nInsertPos /*= -1*/)
{
	ASSERT(!strColumnName.IsEmpty());

	CHECK_BUILD_ERROR
	(

		m_pColumnArray->AddColumn(strColumnName, pDataObj, nEmptySqlRecIdx, bAutoIncrement == TRUE, nInsertPos);
	)	
}

//-----------------------------------------------------------------------------
void SqlTable::Select(const CString& strColumnName, DataObj& aDataObj, int nAllocSize /*= 0*/, BOOL bAutoIncrement /*=FALSE*/, int nInsertPos /*= -1*/)
{
	Select(strColumnName, &aDataObj, nAllocSize, bAutoIncrement, nInsertPos);
}

// Si assume che l'funzione sia stata scritta all'esterno dal programmatore
// Utile per scrivere delle select come la seguente:
//		SELECT COUNT(*)
//		SELECT a+b
//		SELECT CASE col WHEN val1 THEN calc1 ... ELSE default_calc_val
//-----------------------------------------------------------------------------
void SqlTable::SelectSqlFun(LPCTSTR szFunction, DataObj* pResDataObj /*=NULL*/, int nAllocSize /*= 0*/, SqlRecord* pRec/*= NULL*/, int nInsertPos /*= -1*/)
{
	//DBTYPE eOLEType =  m_pSqlConnection->GetSqlDataType(pResDataObj->GetDataType());

	//if (pResDataObj && (eOLEType == DBTYPE_WSTR|| eOLEType == DBTYPE_STR) && nAllocSize > 0)
	//	pResDataObj->SetAllocSize(nAllocSize);

	m_pColumnArray->AddColumn(szFunction, pResDataObj, nEmptySqlRecIdx, false, nInsertPos);

	if (pResDataObj && pRec)
	{
		ASSERT_VALID(pRec);
		int idx = m_pColumnArray->GetUpperBound();
		SqlBindingElem* pSbe = m_pColumnArray->GetAt(idx);
		ASSERT_VALID(pSbe);

		idx = pRec->GetIndexFromDataObj(pResDataObj);
		if (idx > -1)
		{
			CString sName = (LPCTSTR)pRec->GetQualifiedColumnName(idx);
			ASSERT(!sName.IsEmpty());
			pSbe->m_sLocalName = sName;
		}
	}
	// se esiste una group by nella select allora non posso fare
	// nessun update del database (in paticolar modo se sono in KeyedUpdate)
	m_bUpdatable = FALSE;		
}

// Costruisce un column name del tipo :
//		SUM(Caio)
// La stringa funzione passata deve contenere un %s da sostituire con il nome
// della colonna
//
//-----------------------------------------------------------------------------
void SqlTable::SelectSqlFun(LPCTSTR szFunction, DataObj* pParamDataObj, DataObj* pResDataObj, int nAllocSize /*= 0*/, SqlRecord* pRec /*= NULL*/, int nInsertPos /*= -1*/)
{
	SqlRecord* pRecord = pRec ? pRec : m_pRecord;
	ASSERT_VALID(pRecord);
	if (!pRecord)
		return;

	CString strFunction(szFunction);

	if (strFunction.Find(_T("%s")) > 0)
		strFunction = cwsprintf(szFunction, (LPCTSTR)pRecord->GetQualifiedColumnName(pParamDataObj));
	else
		ASSERT(FALSE);

	SelectSqlFun(strFunction, pResDataObj, nAllocSize, pRecord, nInsertPos);
}

//-----------------------------------------------------------------------------
void SqlTable::SelectSqlFun(LPCTSTR szFunction, DataObj& aResDataObj, int nAllocSize /*= 0*/, SqlRecord* pRec /*= NULL*/, int nInsertPos /*= -1*/)
{
	SelectSqlFun(szFunction, &aResDataObj, nAllocSize, pRec, nInsertPos);
}

//-----------------------------------------------------------------------------
void SqlTable::SelectSqlFun(LPCTSTR szFunction, DataObj& aParamDataObj, DataObj& aResDataObj, int nAllocSize /*= 0*/, SqlRecord* pRec/*= NULL*/, int nInsertPos /*= -1*/)
{
	SelectSqlFun(szFunction, &aParamDataObj, &aResDataObj, nAllocSize, pRec, nInsertPos);
}


// setta la selezione per TOP
//-----------------------------------------------------------------------------
void SqlTable::AddSelectKeyword(SelectKeywordType aType, const int& nTopValue)
{ 
	switch (aType)
	{
		case DISTINCT:
			m_strSelectKeyword  += szDistinct;
			break;
		case TOP:
		case TOP_PERCENT:
		case TOP_WITH_TIES:
		{
			if (m_pSqlConnection->GetDBMSType() == DBMS_ORACLE)
			{
				ASSERT(FALSE);
				TRACE(L"Oracle doesn't allow the keyword TOP in a query statement");
				return;
			}
			if (nTopValue <= 0)
				return;
			m_strSelectKeyword += szTop + cwsprintf(_T("%d "), nTopValue);
			if (aType == TOP_PERCENT)
				m_strSelectKeyword += szPercent;
			else if (aType == TOP_WITH_TIES)
                m_strSelectKeyword += szWithTies;
			break;
		}
		default: break;
	}
	
	// se esiste una group by nella select allora non posso fare
	// nessun update del database (in paticolar modo se sono in KeyedUpdate)
	m_bUpdatable = FALSE;	
}

//-----------------------------------------------------------------------------
BOOL SqlTable::IsSelectEmpty() const
{
	return (m_pColumnArray && m_pColumnArray->GetSize() <= 0 && m_strSelect.IsEmpty()); 
}

//-----------------------------------------------------------------------------
void SqlTable::InitRow()
{
	TRACE_SQL(_T("InitRow"), this);
	InitColumns();
	if (m_pRecord) 
		m_pRecord->Init();
}

//-----------------------------------------------------------------------------
void SqlTable::BindKeyColumns()
{
	TRACE_SQL(_T("BindKeyColumns"), this);
	
	CUIntArray aIndexes;	
	m_pRecord->GetCopyPrimaryKeyIndexes(&aIndexes);

	//Oltre alle chiavi primarie aggiungo gli indici dei campi TBModified e TBModifiedID (se sono presenti nella struttura della tabella)
	//questo per fare in modo di aggiornare i campi anche nelle query di aggiornamento che prevedono select ridotte
	if (m_pRecord->GetTableInfo()->ExistModifiedColumn())			
	aIndexes.Add(m_pRecord->GetIndexFromDataObj(&m_pRecord->f_TBModified));
	if (m_pRecord->GetTableInfo()->ExistModifiedIDColumn())			
	aIndexes.Add(m_pRecord->GetIndexFromDataObj(&m_pRecord->f_TBModifiedID));

	for (int j = 0; j <= aIndexes.GetUpperBound(); j++)
	{
		SqlRecordItem* pItem = m_pRecord->GetAt(aIndexes.GetAt(j));
		
		BOOL bFound = FALSE;
		for (int i = 0; i <= m_pColumnArray->GetUpperBound(); i++)
		{
			// confronto tra puntatori per verificare se il campo e` selezionato
			if (m_pColumnArray->GetDataObjAt(i) == pItem->GetDataObj())
			{
				bFound = TRUE;
				break;
			}
		}

		if (!bFound)
			Select(pItem->GetDataObj());
	}	
}

// Esegue il parsing del predicato ORDER BY
//----------------------------------------------------------------------------
BOOL SqlTable::ParseOrderBy(CStringArray& items)
{
	if (m_strSort.IsEmpty())
		return TRUE;

	CString strPhysicalName;
	Parser lex(m_strSort);
	
    while (!lex.Bad() && !lex.Eof() && !lex.LookAhead(T_EOF))
	{
		if (lex.LookAhead(T_ID)) 
		{
			if (!lex.ParseID(strPhysicalName))  
        		return FALSE;

			items.Add(strPhysicalName);
		}
		else	//possibile contenuto di una ContentOf
		{
			lex.EnableAuditString(); lex.GetAuditString();
			Token tk[] = {T_DESCENDING, T_ASCENDING, T_COMMA};
			if (!lex.SkipToToken(tk, FALSE, TRUE, TRUE))
			{
				TRACE1("SqlTable::ParseOrderBy: wrong Order By clause:  %s.\n", (LPCTSTR) m_strSort);
				ASSERT(FALSE);
				return FALSE;
			}
			
		}

		switch(lex.LookAhead())
		{
			case T_DESCENDING:
			case T_ASCENDING:
			case T_COMMA :		
				lex.SkipToken(); 
				break;
			case T_EOF:
				return TRUE;
				
			default:
				TRACE1("SqlTable::ParseOrderBy: wrong Order By clause:  %s.\n", (LPCTSTR) m_strSort);
				ASSERT(FALSE);
				return FALSE;
		}
	}

	return TRUE;	
}

// devo fare il controllo solo se sto utilizzando il cursore DINAMICO
//-----------------------------------------------------------------------------
BOOL SqlTable::CheckOrderBy()
{
	CStringArray OrderByColumns;
	
	TRACE_SQL(_T("CheckOrderBy"), this);

	if (!ParseOrderBy(OrderByColumns))
	{
		m_strSort.Empty();
		return FALSE;
	}

	int nPos;
	
	BOOL bOk = TRUE;
	CString strColumnName;
	CString strTemp1;
	CString strTemp2;
	
	//scorro l'array e controllo per ogni campo nell'OrderBy, la presenza o meno dell'indice	
	for (int i = 0; i <= OrderByColumns.GetUpperBound(); i++)
	{
        strColumnName = OrderByColumns[i];
		strTemp1.Empty();
		strTemp2.Empty();
		
		//se non esiste un indice nella colonna allora la elimino dall OrderBy
		if (!m_pRecord->m_pTableInfo->ExistIndex(strColumnName))
		{
			bOk = FALSE; 
			nPos = m_strSort.Find(strColumnName);
			if (nPos < 0) 
				continue;

			//� il primo campo
			if (i == 0)
			{
				//c'� solo questo campo
				if (OrderByColumns.GetSize() == 1)
					m_strSort.Empty();
				else 	
					// il +1 � x la virgola
					m_strSort = m_strSort.Right(m_strSort.GetLength() - (strColumnName.GetLength() + 1));
			}
			else
			{
				//prendo la parte sinistra
                strTemp1 = m_strSort.Left(nPos);
				// se non � l'ultimo campo allora devo prendere anche la parte destra 
				if (i < OrderByColumns.GetUpperBound())
					strTemp2 = m_strSort.Right(m_strSort.GetLength() - (strColumnName.GetLength() + 1));
				
				m_strSort = strTemp1 + strTemp2;
			}
		}
	}

	return bOk;
}


// UPDATE <tablename> SET <TBModifiedID>=? ,<TBModified>= GetDate() WHERE KeySeg = ? AND....
//-----------------------------------------------------------------------------
void SqlTable::UpdateOnlyTBModified()
{
	if (!m_pRecord || m_pRecord->GetType() != TABLE_TYPE || !m_pRecord->m_pTableInfo)
		return;

	if (!m_pRecord->m_pTableInfo->ExistModifiedColumn() || !m_pRecord->m_pTableInfo->ExistModifiedIDColumn())
		return;

	TRY
	{
		CString strSelect;
		CString strSQL;
		//valorizzo il campo TBModifiedID con il WorkerID assegnato all'utente connesso 
		
		strSelect = szUpdate;
		strSelect += GetTableName();
		strSelect += szSet;
		strSelect +=  cwsprintf(_T("%s = %s"), MODIFIED_COL_NAME, GetDateFunction(m_pSqlConnection->GetDBMSType()));
		if (m_pRecord->f_TBModifiedID != AfxGetWorkerId())
		{
			m_pRecord->f_TBModifiedID = AfxGetWorkerId();
			strSelect += cwsprintf(_T(" , %s = %s"), MODIFIED_ID_COL_NAME, m_pSqlConnection->NativeConvert(&m_pRecord->f_TBModifiedID));
		}

		BOOL bPrepare = OpenUpdateRowSet(strSelect);

		if (m_pUpdateRowSet)
		{
			m_pUpdateRowSet->m_strCurrentTables.RemoveAll();
			m_pUpdateRowSet->m_strCurrentTables.Add(GetTableName());
		}
		//where con le chiavi
		int nParam = 0;
		BindKeyParameters(bPrepare, nParam);
		if (bPrepare)
		{
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strSelect;
			m_pUpdateRowSet->m_strSQL += szWhere;
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strFilterKeys;
			m_pUpdateRowSet->BindParameters();
			TRACE_SQL(cwsprintf(_T("UpdateOnlyTBModified %s (%s)"), (LPCTSTR)m_pUpdateRowSet->m_strSQL, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}
		else
		{
			m_pUpdateRowSet->FixupParameters();
			TRACE_SQL(cwsprintf(_T("UpdateOnlyTBModified(p) %s (%s)"), (LPCTSTR)m_pUpdateRowSet->m_strSQL, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}
		m_pUpdateRowSet->ExecuteNonQuery();
		
		m_pRecord->GetTableInfo()->GetSqlCatalogEntry()->TraceOperation(AUDIT_UPDATE_OP, this);
	}

	CATCH(SqlException, e)
	{
		if (m_pUpdateRowSet->IsOpen())
			m_pUpdateRowSet->Close();
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);

	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlTable::ReadMandatoryColumns()
{
	if (!m_pRecord || m_pRecord->GetType() != TABLE_TYPE || !m_pRecord->m_pTableInfo)
		return;

	if (!m_pRecord->m_pTableInfo->ExistCreatedColumn() && !m_pRecord->m_pTableInfo->ExistModifiedColumn())
		return;

	CString strColName;

	DataObjArray aPK;
	aPK.SetOwns(FALSE);
	SetSkipRowSecurity();
	
	TRY
	{
		m_pRecord->GetKeyStream(aPK, FALSE);
		if (aPK.GetSize() <= 0)
			ThrowSqlException(cwsprintf(_TB("Unable to read value of mandatory columns of table {0-%s}. No data value available."), m_pRecord->GetTableName()));
		
		Open();
		Select(m_pRecord->f_TBCreated);
		Select(m_pRecord->f_TBModified);
		Select(m_pRecord->f_TBCreatedID);
		Select(m_pRecord->f_TBModifiedID);
		SetAliasName(m_pRecord->GetQualifier());		

		//Add the primary key values in the WHERE clause as parameters
		for (int nIdx = 0; nIdx <= aPK.GetUpperBound(); nIdx++)
		{
			DataObj* pDataObj = aPK.GetAt(nIdx);
			if (!pDataObj)
				ThrowSqlException(cwsprintf(_TB("Unable to read value of mandatory columns of table {0-%s}. No data value available."), m_pRecord->GetTableName()));
							
			strColName = m_pRecord->GetColumnName(pDataObj);
			AddFilterColumn(strColName);
			AddParam(strColName, *pDataObj);
			SetParamValue(strColName, *pDataObj);			
		}
		Query();
		Close();
	}
	CATCH(MSqlException, e)
	{
		if (IsOpen())
			Close();
		TRACE1("SqlTable::ReadMandatoryColumns() error: ", e->m_strError);
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlTable::FixupAutoIncColumns()
{
	if (m_pSqlConnection->GetDBMSType() != DBMS_SQLSERVER)
		return;

	TRACE_SQL(_T("FixupAutoIncColumns"), this);
		
	SqlTable aTable(m_pSqlSession);		
	TRY
	{
		//chiamo SELECT IDENT_CURRENT ed effettuo il binding dell'unico campo autoincremental della tabella
		CString strAutoIncCol = m_pRecord->GetAutoIncrementColumn();
		aTable.Open();
		aTable.m_strSQL = cwsprintf( _T("SELECT IDENT_CURRENT('%s')"), m_strTableName);
		aTable.Select(m_pRecord, m_pRecord->GetDataObjFromColumnName(strAutoIncCol));		
		aTable.Query();
		aTable.Close();
	}
	CATCH(MSqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
		ThrowSqlException(e);
	}
	END_CATCH
}


// utilizzzato in caso di connessione oracle
//effettuo una SELECT MAX(AutoIncrColName) from TableName per avere l'ultimo numero disponibile		
//-----------------------------------------------------------------------------
void SqlTable::GetNextAutoincrementValue()
{
	if (m_pSqlConnection->GetDBMSType() == DBMS_SQLSERVER)
		return;

	SqlTable aTable(m_pSqlSession);		
	TRY
	{
		// effettuo prima Lock (per evitare sovrapposizione di numeri)
		//considero l'unica colonna autoincrementale del record
		CString strAutoIncCol = m_pRecord->GetAutoIncrementColumn();
		DataLng lMax = 0;
		aTable.Open();
		aTable.m_strSQL = cwsprintf( _T("SELECT MAX(%s) from %s"), strAutoIncCol, m_strTableName);
		aTable.Select(strAutoIncCol, &lMax);
		aTable.Query();
		aTable.Close();
		m_pRecord->GetDataObjFromColumnName(strAutoIncCol)->Assign(++lMax);
	}
	CATCH(MSqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
		ThrowSqlException(e);
	}
	END_CATCH
}
//-----------------------------------------------------------------------------
void SqlTable::BindExtraColumns()
{
    CStringArray OrderByColumns;
	if (!ParseOrderBy(OrderByColumns))
	{
		m_strSort.Empty();
		return;
	}
	
	for (int i = 0; i <= OrderByColumns.GetUpperBound(); i++)
	{
		CString strColumnName = OrderByColumns[i];
		if (m_pColumnArray->GetDataObj(strColumnName)) 
			continue;

		// se ho le colonne referenziate
		CString	strTableName, strPhysicalColName;
		int	nIdx = strColumnName.Find(DOT_CHAR); 
		if (nIdx > 0)
		{
			strPhysicalColName = strColumnName.Mid(nIdx + 1);
			strTableName = strColumnName.Left(nIdx);
		}
		 
		const SqlTableInfo* pTableInfo = (strTableName.IsEmpty())
									? m_pRecord->m_pTableInfo
									: m_pTableArray->GetTableInfo(strTableName);
		
		ASSERT(pTableInfo);
		if (!pTableInfo) continue;
		const SqlColumnInfo* pColumnInfo = pTableInfo->GetColumnInfo(strPhysicalColName);
		
		ASSERT(pColumnInfo);
		if (!pColumnInfo) continue;
	
		CWordArray dataTypes;
		VERIFY(pColumnInfo->GetDataObjTypes(dataTypes));
		
		DataObj* pDataObj = DataObj::DataObjCreate(dataTypes[0]);
		ASSERT(pDataObj);
		if (!pDataObj) continue;

		// Prealloca la stringa per avere lo spazio opportuno da utilizzare nella BindCol
		pDataObj->Allocate();
		
		// Aggiorna i dati relativi al dataobj creato nella relativa column info
		const_cast<SqlColumnInfo*>(pColumnInfo)->UpdateDataObjType(pDataObj);
		pColumnInfo->SetDataObjInfo(pDataObj);
		
		// Aggiunge il dataobj necessario a bufferizzare la colonna che l'utente non
		// aveva selezionato ma che era nella order by
		if (!m_pExtraColumns)
			m_pExtraColumns = new DataObjArray;
		m_pExtraColumns->Add(pDataObj);
		Select(strColumnName, pDataObj);
	}
}

//-----------------------------------------------------------------------------
void SqlTable::BindAllColumns()
{
	TRACE_SQL(_T("BindAllColumns"), this);
	for (int j = 0; j <= m_pRecord->GetUpperBound(); j++)
	{
		// i campi virtuali devono essere skippati
		if (m_pRecord->IsVirtual(j))
			continue;
		
		BOOL bFound = FALSE;
		for (int i = 0; i <= m_pColumnArray->GetUpperBound(); i++)
		{
			// confronto tra puntatori per verificare se il campo e` selezionato
			if (m_pColumnArray->GetDataObjAt(i) == m_pRecord->GetDataObjAt(j))
			{
				bFound = TRUE;
				break;
			}
		}

		if (!bFound)
			Select(m_pRecord->GetDataObjAt(j));
	}
}

//-----------------------------------------------------------------------------
void SqlTable::FixupColumns()
{
	SqlRowSet::FixupColumns();
	//se la tabella � protetta 
	if (m_bSelectGrantInformation && m_pRecord)
	{
		const SqlCatalogEntry* pConstCatalogEntry = m_pSqlConnection->GetCatalogEntry(GetRecord()->GetTableName());
		if (pConstCatalogEntry && pConstCatalogEntry->IsProtected()) //@@BAUZI TODO da ottimizzare. Portare il flag m_bIsUnderProtection in SqlRecord cos� si evita di interrogare il catalog
			pConstCatalogEntry->HideProtectedFields(m_pRecord);
	}
}

//-----------------------------------------------------------------------------
CString SqlTable::GetBuildSelect() const
{
	if (!m_strSelect.IsEmpty())
		return m_strSelect;

	if (m_strSelectKeyword.IsEmpty() && m_pColumnArray->GetSize() == 0) 
		return _T("");

	CString strSelect;
	for (int n = 0; n <= m_pColumnArray->GetUpperBound(); n++)
	{
		if (!strSelect.IsEmpty())
			strSelect += szComma;
		strSelect += m_pColumnArray->GetParamName(n);					
	}

	return strSelect;
}

//-----------------------------------------------------------------------------
void SqlTable::AddContextBagFilters()
{
	AddContextBagFilters(m_pRecord);
}

//-----------------------------------------------------------------------------
void SqlTable::AddContextBagFilters(const RecordArray& ar)
{
	for (int i = 0; i < ar.GetSize(); i++)
	{
		SqlRecord* pRecord = ar[i];
		//ASSERT_VALID(pRecord);

		if (pRecord)
			AddContextBagFilters(pRecord);
	}
}

//-----------------------------------------------------------------------------
void SqlTable::AddContextBagFilters(SqlRecord* pRecord)
{
	AddContextBagFiltersInternal(pRecord);	//m_pRecord oppure Woorm
/*
	if (m_pRecord->m_arExtensions)
	{
		ASSERT_VALID(m_pRecord->m_arExtensions);
		for (int i = 0; i < m_pRecord->m_arExtensions->GetSize(); i++)
		{
			SqlRecord* pRec = (*(m_pRecord->m_arExtensions))[i];
			ASSERT_VALID(pRec);

			AddContextBagFiltersInternal(pRec);
		}
	}	
	else if (m_pTableArray)
	{
		ASSERT_VALID(m_pTableArray);
		//parte da 1 perch� il primo dovrebbe essere della stessa classe di m_pRecord
		for (int i = 1; i < m_pTableArray->GetSize(); i++)
		{
			SqlRecord* pRec = m_pTableArray->GetRecord(i);
			if (pRec)
			{
				ASSERT_VALID(pRec);
				AddContextBagFiltersInternal(pRec);
			}
		}
	}
	*/
}

//-----------------------------------------------------------------------------
void SqlTable::AddContextBagFiltersInternal(SqlRecord* pRecord)
{
	// the developer may disable the context-based filter
	if (m_bSkipContextBagParameters)
		return;

	if (!pRecord || !pRecord->m_arContextBagElements.GetCount())
		return;

	CContextBag* pContextBag = AfxGetThreadContextBag();
	if (!pContextBag)
		return;

	SqlRecordItem* pItem = NULL;
	for(int idx = 0 ; idx < pRecord->m_arContextBagElements.GetCount(); idx++)
	{
		pItem = (SqlRecordItem*)(pRecord->m_arContextBagElements.GetAt(idx));

		//per prima cosa verifico se l'elemento � presente nel ContextBag
		//e che non sia gi� inserito tra i parametri
		if (
				pItem->m_strContextElementName.IsEmpty() ||	
				!pContextBag->LookupContextObject(pItem->m_strContextElementName) ||
				m_pParamArray->GetParamByName(pItem->m_strContextElementName)
			)
			continue;

		//aggiungo nella where e valorizzo il parametro
		AddFilterColumn(pRecord->GetQualifiedColumnName(pItem->m_pDataObj, TRUE));

		AddParam(pItem->m_strContextElementName, *pItem->m_pDataObj);
		SetParamValue(pItem->m_strContextElementName, *(DataObj*)pContextBag->LookupContextObject(pItem->m_strContextElementName));
	}
}	

//-----------------------------------------------------------------------------
void SqlTable::ValorizeContextBagParameters()
{
	ValorizeContextBagParameters(m_pRecord);
}

void SqlTable::ValorizeContextBagParameters(const RecordArray& ar)
{
	for (int i = 0; i < ar.GetSize(); i++)
	{
		SqlRecord* pRecord = ar[i];
		//ASSERT_VALID(pRecord);

		if (pRecord)
			ValorizeContextBagParameters(pRecord);
	}
}

//-----------------------------------------------------------------------------
void SqlTable::ValorizeContextBagParameters(SqlRecord* pRecord)
{
	ValorizeContextBagParametersInternal(pRecord);
/*
	if (m_pRecord->m_arExtensions)
	{
		ASSERT_VALID(m_pRecord->m_arExtensions);
		for (int i = 0; i < m_pRecord->m_arExtensions->GetSize(); i++)
		{
			SqlRecord* pRec = (*(m_pRecord->m_arExtensions))[i];
			ASSERT_VALID(pRec);

			ValorizeContextBagParametersInternal(pRec);
		}
	}
	else if (m_pTableArray)
	{
		ASSERT_VALID(m_pTableArray);
		for (int i = 0; i < m_pTableArray->GetSize(); i++)
		{
			SqlRecord* pRec = m_pTableArray->GetRecord(i);
			ASSERT_VALID(pRec);

			ValorizeContextBagParametersInternal(pRec);
		}
	}
	*/
}

//-----------------------------------------------------------------------------
void SqlTable::ValorizeContextBagParametersInternal(SqlRecord* pRecord)
{
	// the developer may disable the context-based filter
	if (m_bSkipContextBagParameters)
		return;

	if (!pRecord || !pRecord->m_arContextBagElements.GetCount())
		return;

	CContextBag* pContextBag = AfxGetThreadContextBag();
	if (!pContextBag)
		return;

	SqlRecordItem* pItem = NULL;
	for(int idx = 0 ; idx < pRecord->m_arContextBagElements.GetCount(); idx++)
	{
		pItem = (SqlRecordItem*)(pRecord->m_arContextBagElements.GetAt(idx));

		//per prima cosa verifico se l'elemento � presente nel ContextBag
		if (
				pItem->m_strContextElementName.IsEmpty() ||	
				!pContextBag->LookupContextObject(pItem->m_strContextElementName) ||
				!m_pParamArray->GetParamByName(pItem->m_strContextElementName)
				)
			continue;

		//valorizzo il parametro con il valore presente nel bag
		SetParamValue(pItem->m_strContextElementName, *(DataObj*)pContextBag->LookupContextObject(pItem->m_strContextElementName));
	}
}

//utilizzato dall'HOTLINK per cui abbiamo solo l'istanza corrente e non un SqlTableArray
//----------------------------------------------------------------------------------------------------------
CString SqlTable::GetSelectGrantString()
{
	if (!m_bSelectGrantInformation)
		return _T("");
	if (m_pRecord)
	{
		const SqlCatalogEntry* pConstCatalogEntry = m_pSqlConnection->GetCatalogEntry(GetRecord()->GetTableName());
		if (pConstCatalogEntry && pConstCatalogEntry->IsProtected())
			return pConstCatalogEntry->GetSelectGrantString(this);
	}

	return _T("");

}

//----------------------------------------------------------------------------------------------------------
void SqlTable::AddRowSecurityFilters()
{
	if (m_bSkipRowSecurity)
		return;

	if (m_pTableArray && m_pTableArray->GetSize() > 0)
	{
		for (int i = 0; i < m_pTableArray->GetSize(); i++)
		{
			SqlTableItem* pTableItem = m_pTableArray->GetAt(i);
			const SqlCatalogEntry* pConstCatalogEntry = m_pSqlConnection->GetCatalogEntry(pTableItem->m_strTableName);
			if (pConstCatalogEntry && pConstCatalogEntry->IsProtected())
				pConstCatalogEntry->AddRowSecurityFilters(this, pTableItem);
		}					
	}
	else
		if (m_pRecord)
		{
			const SqlCatalogEntry* pConstCatalogEntry = m_pSqlConnection->GetCatalogEntry(GetRecord()->GetTableName());
			if (pConstCatalogEntry && pConstCatalogEntry->IsProtected())
			{	
				SqlTableItem aTableItem(GetTableName(), m_strAliasName, GetRecord());
				pConstCatalogEntry->AddRowSecurityFilters(this, &aTableItem);
			}
		}	
}


//----------------------------------------------------------------------------------------------------------
void SqlTable::ValorizeRowSecurityParameters()
{		
	if (m_pRecord && (m_pRSSelectWorkerID || m_pRSFilterWorkerID))
	{
		const SqlCatalogEntry* pConstCatalogEntry = m_pSqlConnection->GetCatalogEntry(GetRecord()->GetTableName());
		if (pConstCatalogEntry && pConstCatalogEntry->IsProtected())
			pConstCatalogEntry->ValorizeRowSecurityParameters(this);
	}
	
}

//-----------------------------------------------------------------------------
void SqlTable::BuildSelect()
{
	TRACE_SQL(_T("BuildSelect"), this);
	if (m_strSQL.IsEmpty())
	{
		// verifica che si selezionino le colonne di chiave primaria e se del caso
		// le aggiunge subdolamente alla select per poter fare l'update al posto giusto
		// serve anche nel caso di update se il sqlrecord ha un campo di tipo autoincrement
		if (m_pRecord && m_bUpdatable)
		{
			BindKeyColumns();
			m_pRecord->GetTableInfo()->GetSqlCatalogEntry()->BindTracedColumns(this);
		}

		if (
				m_eCursorType == E_KEYSET_CURSOR &&	m_bUpdatable &&
				GetKeySegmentCount() <= 0  								
			)
			ThrowSqlException(cwsprintf(_TB("A Keyset cursor used in a table without primary key or unique index defined. Table: {0-%s}"), m_strTableName));

				
		// Se ho DISTINCT nella SELECT devo aggiungere le colonne presenti OrderBy eventualmente non presenti nella SELECT
		if (m_strSelectKeyword == szDistinct && m_pRecord && !m_strSort.IsEmpty())
			BindExtraColumns();

		//---- Select manuale gestito da fuori
		if (m_strSelect.IsEmpty())
		{
			if (m_strSelectKeyword.IsEmpty() && m_pColumnArray->GetSize() == 0) 
				return;

			for (int n = 0; n <= m_pColumnArray->GetUpperBound(); n++)
			{
				if (!m_strSelect.IsEmpty())
					m_strSelect += szComma;
				m_strSelect += m_pColumnArray->GetParamName(n);					
			}
		}

		ASSERT(!m_strSelect.IsEmpty());

		m_strSQL += szSelect;

		//TBROWSecurity. Se necessario valorizza il campo l_CurrentWorkerGrantType
		//l_CurrentWorkerGrantType = (select GrantType from RS_SubjectsGrants where WorkerID = %s and RowSecurityID = %s.RowSecurityID)
		m_strSQL += GetSelectGrantString();

		if (!m_strSelectKeyword.IsEmpty())
			m_strSQL += m_strSelectKeyword;

		m_strSQL += m_strSelect;	
		//----
		m_strSQL += szFrom;

		m_strCurrentTables.RemoveAll();
		if (m_strFrom.IsEmpty() && m_pTableArray && m_pTableArray->GetSize() > 0)
		{
			for (int i = 0; i <= m_pTableArray->GetUpperBound(); i++) 
			{
				SqlTableItem* pTableItem = m_pTableArray->GetAt(i);
				CString sTableName = (pTableItem->m_strAliasName.IsEmpty() ||
										pTableItem->m_strAliasName.CompareNoCase(pTableItem->m_strTableName) == 0)
									? pTableItem->m_strTableName
									: pTableItem->m_strTableName + GetAlias() + pTableItem->m_strAliasName;
				m_strCurrentTables.Add (sTableName);			
				if (!pTableItem->m_strOuterJoinClause.IsEmpty())
					m_strFrom += pTableItem->m_strOuterJoinClause;
				else
				{
					if (!m_strFrom.IsEmpty())
						m_strFrom += szComma;
					m_strFrom += sTableName;
				}				
			}
		}		
		if (m_strFrom.IsEmpty())
		{
			m_strCurrentTables.Add (GetTableName());
			m_strFrom = GetTableName();
		// gestisce anche il costrutto di alias, se indicato
		if (!m_strAliasName.IsEmpty())		
				m_strFrom += GetAlias() + m_strAliasName;
		}
		
		m_strSQL += m_strFrom;		
		
		if (m_pRecord) m_pRecord->OnExtraFiltering(m_strFilter);

		// WHERE ....
		AddContextBagFilters(); //eventuale filtraggio dovuto al contextBag
		
		AddRowSecurityFilters(); //eventuale filtraggio dovuto al row security layer TBROWSECURITY
		
		if (!m_strFilter.IsEmpty())
		{
			m_strSQL += szWhere;
			m_strSQL += m_strFilter;
		}
				
		// GROUP BY .....
		if (!m_strGroupBy.IsEmpty())
		{
			m_strSQL += szGroupBy;
			m_strSQL += m_strGroupBy;

			// HAVING .....
			if (!m_strHaving.IsEmpty())
			{
				m_strSQL += szHaving;
				m_strSQL += m_strHaving;
			}
			
		}

		// ORDER BY....
		//se devo effettuare il paging ho bisogno del sorting
		//considero le chiavi primarie
		if (m_nPageSize > 0 && m_strSort.IsEmpty())
		{
			int nIdxPos = 0;
			for (int i = 0; i < m_pRecord->m_aSqlPrimaryKeyIndexes.GetSize(); i++)
			{
				nIdxPos = m_pRecord->m_aSqlPrimaryKeyIndexes.GetAt(i);
				AddSortColumn(m_pRecord->GetColumnName(nIdxPos));
			}
		}
		if (!m_strSort.IsEmpty())
		{
			m_strSQL += szOrderBy;
			m_strSQL += m_strSort;
		}
		else
			//non posso fare il paging
			if (m_nPageSize > 0)
			{
				ASSERT(FALSE);
				EnablePaging(0);
			}
	}

	SubstituteQuestionMarks(m_strSQL); //sostituisce i nomi dei parametri ai punti ? (venivano usati con ODBC e poi OLEDB)

	// salva la query per controllare che non venga cambiata senza
	// chiudere e riaprire la tabella
	m_strOldSQL = m_strSQL;
}

	

//-----------------------------------------------------------------------------
BOOL SqlTable::BuildQuery()
{
	if (m_pRecord && m_pRecord->IsVirtual())
		return FALSE;

	if (!m_pRowSet)
		ThrowSqlException(cwsprintf(_TB("SqlTable::Query: before carry out query\n  table {0-%s} isn't open."), m_strTableName));

	if (m_bErrorFound)
		//ThrowSqlException(cwsprintf(_TB("SqltTable::Query: errors occurred in the command construction phase on table {0-%s}"), m_strTableName));
		return FALSE;

	TRY
	{
		// build database query
		if (m_bFirstQuery)
		{
			BuildSelect();
			TRACE_SQL(cwsprintf(_T("Query %s %s (%s)"), (LPCTSTR)GetTableName(), (LPCTSTR)m_strFilter, (LPCTSTR)GetParamStr(this)), this);
			Prepare();
			VERIFY(BindColumns());
			BindParameters();
			m_bFirstQuery = FALSE;
		}
	}
	CATCH(MSqlException, e)
	{
		m_pSqlSession->ShowMessage(
			cwsprintf
			(
				_TB("Errors on select from table {0-%s}.\r\n{1-%s}"),
				(LPCTSTR)m_strTableName,
				(LPCTSTR)e->m_strError
			)
		);

		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		m_pRowSet->Dispose();		
		ThrowSqlException(e);
	}
	END_CATCH
	return TRUE;
}

           
//-----------------------------------------------------------------------------
void SqlTable::Query(MoveType  moveType, int lSkip /*= 0*/)
{
	if (m_pRecord && m_pRecord->IsVirtual())
		return;

	if (!m_pRowSet)
		ThrowSqlException(cwsprintf(_TB("SqlTable::Query: before carry out query\n  table {0-%s} isn't open."), m_strTableName));

	if (m_bErrorFound)
		ThrowSqlException(cwsprintf(_TB("SqltTable::Query: errors occurred in the command construction phase on table {0-%s}"), m_strTableName));
	
	m_sqlFetchResult = FetchOk;

	TRY
	{
		BOOL bContinue = FALSE;
		m_bEOFSeen = m_bBOF = m_bEOF = TRUE;
		m_bDeleted = FALSE;

		// database query
		if (m_bFirstQuery)
		{
			BuildSelect();
			TRACE_SQL(cwsprintf(_T("Query first%s %s (%s)"), (LPCTSTR)GetTableName(), (LPCTSTR)m_strFilter, (LPCTSTR)GetParamStr(this)), this);
			BindColumns();
			BindParameters();
			m_bFirstQuery = FALSE;
		}
		else
		{
			// Qualcuno ha cambiato la query senza chiudere e riaprire la table?
			if (m_strOldSQL != m_strSQL)
				ThrowSqlException(_TB("SqlTable::Query(): query changed without closing or reopening the table."));

			m_pRowSet->Dispose();

			//ValorizeContextBagParameters();		
			ValorizeRowSecurityParameters();
			FixupParameters();
		}

		TRACE_SQL(cwsprintf(_T("Execute query %s with params %s"), m_strSQL, (LPCTSTR)GetParamStr(this)), this);

		if (m_nPageSize > 0)
			ExecutePagingCommand(moveType);
		else
			ExecuteCommand();

		
		// faccio la prima fetch del dato
		// gestione eccezione
		START_DB_TIME(DB_MOVE_NEXT)
		TRACE_SQL(_T("MoveNext"), this);

		if (m_pRowSet->IsNull())
		{
			TRACE_SQL(cwsprintf(_T("Query(n) %s (%s) is null"), (LPCTSTR)GetTableName(), (LPCTSTR)GetParamStr(this)), this);
			ThrowSqlException(_TB("SqlTable::Query(): Rowset is null"));
		}

		if (!m_pRowSet->HasRows())
		{
			InitRow();
			// If recordset empty, it doesn't make sense to check
			// record count and current record, but we'll set them anyway
			m_lCurrentRecord = SQL_CURRENT_RECORD_UNDEFINED;
			m_lRecordCount = 0;
			Disconnect();
			return;
		}
		
		m_bBOF = m_bEOF = FALSE;
		m_bEOFSeen = m_bScrollable;
		m_lCurrentRecord = 0;
		m_lRecordCount = (m_bScrollable) ? m_pRowSet->GetRecordsAffected(): 1;

		Move(moveType, lSkip);
		
	}
	CATCH(MSqlException, e)
	{		
		m_pSqlSession->ShowMessage(
									cwsprintf
										(
											_TB("Errors on select from table {0-%s}.\r\n{1-%s}"),
											(LPCTSTR)m_strTableName,
											(LPCTSTR)e->m_strError
										)
									);
	
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);	
		m_pRowSet->Dispose();
		Disconnect();
		ThrowSqlException(e);
	}
	END_CATCH
}


//-----------------------------------------------------------------------------
void SqlTable::ScalarQuery()
{
	if (m_strSQL.IsEmpty())
		ThrowSqlException(_TB("SqlTable::ScalarQuery: command string empty."));

	if (!m_pRowSet)
		ThrowSqlException(cwsprintf(_TB("SqlTable::ScalarQuery: the table is not opened for command {0-%s}."), m_strSQL));

	TRY
	{		
		// database query
		if (m_bFirstQuery)
		{
			m_strOldSQL = m_strSQL;
			BindColumns();
			BindParameters();
			m_bFirstQuery = FALSE;
		}
		else
		{
			// Qualcuno ha cambiato la query senza chiudere e riaprire la table?
			if (m_strOldSQL != m_strSQL)
				ThrowSqlException(_TB("SqlTable::ScalarQuery(): query changed without closing or reopening the table."));

			m_pRowSet->Dispose();
			ValorizeRowSecurityParameters();
			FixupParameters();
		}
	}

	TRACE_SQL(cwsprintf(_T("ScalarQuery %s with params %s"), m_strSQL, (LPCTSTR)GetParamStr(this)), this);

	ExecuteScalar();


	CATCH(MSqlException, e)
	{
		m_pSqlSession->ShowMessage(cwsprintf(_TB("Error during execution of scalar query %s with params %s"), m_strSQL, (LPCTSTR)GetParamStr(this)));
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		m_pRowSet->Dispose();
		ThrowSqlException(e);
	}
	END_CATCH
}

//   is used when there is no return value of any kind expected from SQL server, an example being a simple UPDATE statement.
//-----------------------------------------------------------------------------
int SqlTable::NonQuery()
{
	ThrowSqlException(_TB("SqlTable::NonQuery() not implemented"));

	return -1;
}

//-----------------------------------------------------------------------------
void SqlTable::AddNew(BOOL bInitRow/* = TRUE*/)
{
	ASSERT_VALID(this);
	ASSERT(IsOpen());

    if (bInitRow) InitRow();
	m_nEditMode = addnew;
}



//-----------------------------------------------------------------------------
void SqlTable::Edit()
{
	TRY
	{
		ASSERT_VALID(this);
		ASSERT(IsOpen());
	
		m_nEditMode = edit;
	}
	CATCH(MSqlException, e)
	{
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		ThrowSqlException(e);
	}
	END_CATCH
}            


// "DELETE <tablename> WHERE keyseg = ? AND ..."
//-----------------------------------------------------------------------------
BOOL SqlTable::KeyedDelete()
{
	ASSERT(m_pRecord);
	ASSERT(m_pRecord->m_pTableInfo);

	// l'OldRecord mi viene passato dal DBTSlaveBuffered. Negli altri casi non � + necessario
	if (m_pOldRecord && m_pRecord->GetRuntimeClass() != m_pOldRecord->GetRuntimeClass())
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	BOOL bContinue = FALSE;
	TRY
	{
		CString strSelect;

		strSelect = szDeleteFrom;
		strSelect += GetTableName();

		BOOL bPrepare = OpenUpdateRowSet(strSelect);
		
		if (m_pUpdateRowSet)
		{
			m_pUpdateRowSet->m_strCurrentTables.RemoveAll();
			m_pUpdateRowSet->m_strCurrentTables.Add(GetTableName());
		}

	
		int nParam = 0;
		BindKeyParameters(bPrepare, nParam);

        if (bPrepare)
		{	
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strSelect;
			m_pUpdateRowSet->m_strSQL += szWhere;
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strFilterKeys;
			m_pUpdateRowSet->BindParameters();				
			TRACE_SQL(cwsprintf(_T("KeyedDelete %s (%s)"), (LPCTSTR)strSelect, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);	
		}
		else
		{
			m_pUpdateRowSet->FixupParameters();
			TRACE_SQL(cwsprintf(_T("KeyedDelete(p) %s (%s)"), (LPCTSTR)strSelect, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}	

		m_pUpdateRowSet->ExecuteNonQuery();

		return TRUE;
	}
	CATCH(MSqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		ThrowSqlException(e);
		return FALSE;
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
BOOL SqlTable::OpenUpdateRowSet(const CString& strSelect)
{
	TRACE_SQL(_T("OpenUpdateRowSet"), this);
	if (!m_pUpdateRowSet)
		ThrowSqlException(cwsprintf(_TB("SqlTable::OpenUpdateRowSet: unable to open update cursor on table {0-%s}./n"), (LPCTSTR)m_strTableName));

	// devo riprepararmi lo statement  anche se la stringa di update � differente da quella precedente oppure � variato il valore del flag relativo all'optimisticlock del contesto di appartenenza
	if (
		m_pUpdateRowSet->IsOpen() && 
		m_pUpdateRowSet->m_strSelect.CompareNoCase(strSelect) == 0 &&
		(
			(*m_pContext->m_bOptimisticLock && !m_pUpdateRowSet->m_strFilterOldValues.IsEmpty()) ||
			(!*m_pContext->m_bOptimisticLock && m_pUpdateRowSet->m_strFilterOldValues.IsEmpty())
		)
	)
		return FALSE;

	if (!m_pUpdateRowSet->IsOpen())
		m_pUpdateRowSet->Open();
	
	//devo costruirmi la struttura di binding dei parametri e riprepararmi la query
	m_pUpdateRowSet->ClearRowSet();
	m_pUpdateRowSet->m_strSelect = strSelect;	

	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlTable::FreeUpdateRowSet()
{
	TRACE_SQL(_T("FreeUpdateRowSet"), this);

	if (m_pUpdateRowSet)
	{
		if (m_pUpdateRowSet->IsOpen())
			m_pUpdateRowSet->Close();
		
		delete m_pUpdateRowSet;
		m_pUpdateRowSet =NULL;
	}
}


// INSERT INTO <tablename> (<colname1>[,<colname2>]) VALUES (?[,?])
//-----------------------------------------------------------------------------
BOOL SqlTable::NativeInsert(BOOL bTraced /*=TRUE*/)
{
	TRY
	{
		//valorizzo il campo TBCreatedID con il WorkerID assegnato all'utente connesso 
		m_pRecord->f_TBCreatedID = AfxGetWorkerId();
		m_pRecord->f_TBModifiedID = AfxGetWorkerId();
	
		CString strInsert; // INSERT INTO <tablename> (<colname1>[,<colname2>])
		CString strValues; // VALUE (?, ?...)
		
		strInsert = szInsertInto;
		strInsert += GetTableName();
		strInsert += szLeftParen;
		
		strValues = szValuesParen;
		
		//CString			strColumnName;
		DataObj*		pDataObj = NULL;
		SqlBindingElem* pBindElem;
		SqlBindingElem* pSubBindElem;
		
		for (int i = 0; i <= m_pColumnArray->GetUpperBound(); i++)
		{
			pBindElem = m_pColumnArray->GetAt(i);
			if (pBindElem->IsReadOnly())
				continue;

			// if the column has the Autoincrement property 
			if (pBindElem->m_bAutoIncrement && m_pSqlConnection->GetDBMSType() == DBMS_SQLSERVER)
				continue;
			
			pDataObj = pBindElem->m_pDataObj;

			//devo escludere i campi local
			const SqlColumnInfo* pItem = m_pRecord->GetColumnInfo(pDataObj);
			if (pItem && pItem->m_bVirtual)
				continue;
			
			// is TBCreated or TBModified column and have empty values I haven't add the column in the columns list 
			// because its value is automatically set by the DBMS
			if (					
					pDataObj == &(m_pRecord->f_TBCreated) ||
					pDataObj == &(m_pRecord->f_TBModified)
				)
			{
				strInsert += pBindElem->GetBindName();
				strInsert += szComma;
				strValues += cwsprintf(_T("%s,"), GetDateFunction(m_pSqlConnection->GetDBMSType()));					
				continue;
			}				

			strInsert += pBindElem->m_strBindName;
			strInsert += szComma;
			strValues += (pBindElem->m_strBindName.Find(_T("@"), 0) != 0) ? _T("@") + pBindElem->m_strBindName : pBindElem->m_strBindName;
			strValues += _T(",");			
		}

		// overwrite last ',' with ')'
		strInsert.SetAt(strInsert.GetLength()-1, _T(')'));
		strValues.SetAt(strValues.GetLength()-1, _T(')'));

		BOOL bPrepare = OpenUpdateRowSet(strInsert + strValues);

		if (m_pUpdateRowSet)
		{
			m_pUpdateRowSet->m_strCurrentTables.RemoveAll();
			m_pUpdateRowSet->m_strCurrentTables.Add(GetTableName());
		}

		//BIND DEI PARAMETRI		
		int			nParam		= 0;
		DataObj*	pDataObjPar	= NULL;

		for (int n = 0; n <= m_pColumnArray->GetUpperBound(); n++)
		{
			pSubBindElem = m_pColumnArray->GetAt(n);
			if (pSubBindElem->IsReadOnly())
				continue;
			if (pSubBindElem->m_bAutoIncrement && m_pSqlConnection->GetDBMSType() == DBMS_SQLSERVER)
				continue;

			pDataObjPar = pSubBindElem->m_pDataObj;
			//devo escludere i campi local introdotti dalla TBRowSecurity
			const SqlColumnInfo* pItem = m_pRecord->GetColumnInfo(pDataObjPar);
			if (pItem && pItem->m_bVirtual)
				continue;						
	
			if (pSubBindElem->m_bAutoIncrement)
				GetNextAutoincrementValue();

			if (bPrepare)
			{
				m_pUpdateRowSet->AddParam(pSubBindElem->GetBindName(), *pDataObjPar);	
				m_pUpdateRowSet->m_pParamArray->GetAt(m_pUpdateRowSet->m_pParamArray->GetUpperBound())->SetParamValue(*pDataObjPar);
			}
			else
				m_pUpdateRowSet->m_pParamArray->GetAt(nParam)->SetParamValue(*pDataObjPar);

			nParam++;
		}
		
		if (bPrepare)
		{	
			m_pUpdateRowSet->m_strSQL = strInsert + strValues;
			m_pUpdateRowSet->BindParameters();
			TRACE_SQL(cwsprintf(_T("NativeInsert %s (%s)"), (LPCTSTR)strInsert, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}
		else			
		{	
			m_pUpdateRowSet->FixupParameters();
			TRACE_SQL(cwsprintf(_T("NativeInsert(p) %s (%s)"), (LPCTSTR)strInsert, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}

		m_pUpdateRowSet->ExecuteNonQuery();
	
		// devo fare un'interrogazione al database x farmi restituire
		// i campi di tipo autincremental				
		if (m_pRecord && m_pRecord->m_bAutoIncrement)
			FixupAutoIncColumns();
				
		
		if (bTraced) 
			m_pRecord->GetTableInfo()->GetSqlCatalogEntry()->TraceOperation
				(
					AUDIT_INSERT_OP, 
					this
				);		
		return TRUE;
	}
	CATCH(MSqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		m_strError = e->m_strError;
		return FALSE;
	}
	END_CATCH
}


// "DELETE <tablename> WHERE <colname> = ? AND ..."
//-----------------------------------------------------------------------------
BOOL SqlTable::NativeDelete()
{
	TRY
	{
		TRACE_SQL(_T("NativeDelete"), this);
		
		CString strSQL;
	
		strSQL	=  szDeleteFrom;
		strSQL	+= GetTableName();

		if (!m_strFilter.IsEmpty())
		{
			strSQL	+= szWhere;
			strSQL	+= m_strFilter;
		}
		
		SubstituteQuestionMarks(strSQL);


		BOOL bPrepare = OpenUpdateRowSet(strSQL);
		
		if (m_pUpdateRowSet)
		{
			m_pUpdateRowSet->m_strCurrentTables.RemoveAll();
			m_pUpdateRowSet->m_strCurrentTables.Add(GetTableName());
		}

		DataObj* pDataObj	= NULL;
		SqlBindingElem* pElem = NULL;

		// se bPrepare = TRUE
		// devo rieffettuare la prepare dello statement. Cambia il buffer di bind
		// devo riallocarmi la struttura di binding			
		// facciamo la binding dei parametri per la WHERER clause
		for (int i = 0; i <= m_pParamArray->GetUpperBound(); i++)
		{
			pElem = m_pParamArray->GetAt(i);
			pDataObj = pElem->GetDataObj();
			if (bPrepare)
			{
				m_pUpdateRowSet->AddParam(pElem->m_strBindName, *pDataObj);
				m_pUpdateRowSet->m_pParamArray->GetAt(m_pUpdateRowSet->m_pParamArray->GetUpperBound())->SetParamValue(*pDataObj);
			}
			else
				m_pUpdateRowSet->m_pParamArray->GetAt(i)->SetParamValue(*pDataObj);			
		}
		
		if (bPrepare)
		{	
			m_pUpdateRowSet->m_strSQL = m_pUpdateRowSet->m_strSelect;
			m_pUpdateRowSet->BindParameters();
		}
		else
			m_pUpdateRowSet->FixupParameters();

		m_pUpdateRowSet->ExecuteNonQuery();

		return TRUE;
	}
	CATCH(MSqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		ThrowSqlException(e);
		return FALSE;
	}
	END_CATCH
}

// UPDATE <tablename> SET <colname1>=?[,<colname2>=?] WHERE COLNAME = ? AND ...
//-----------------------------------------------------------------------------
int SqlTable::NativeUpdate(BOOL bForceTBModified /*=FALSE*/)
{
	TRACE_SQL(_T("NativeUpdate"), this);

	TRY
	{
		CString strSelect;
		CString strSQL;

		strSelect = szUpdate;
		strSelect += GetTableName();
		strSelect += szSet;

		DataObj* pDataObj = NULL;
		SqlBindingElem* pElem = NULL;


		// per prima cosa costruisco la stringa di query. Questa mi serve per confrontarla
		// con l'eventuale query di update fatta in precedenza. Due casi:
		// 1 - sono uguali devo solo modificare i parametri 
		// 2 - diverse. Devo fare tutti i passi relativi alla prepare e alla bind dei 
		// parametri. Passi eseguiti la prima volta 
		if (BuildSetClause(strSelect, FALSE) == 0 && !bForceTBModified)
			return UPDATE_NO_DATA;

		// Update need "WHERE COLUMN1 = ? AND ..."
		if (!m_strFilter.IsEmpty())
		{
			strSQL += szWhere;
			strSQL += m_strFilter;
		}

		BOOL bPrepare = OpenUpdateRowSet(strSQL);

		if (m_pUpdateRowSet)
		{
			m_pUpdateRowSet->m_strCurrentTables.RemoveAll();
			m_pUpdateRowSet->m_strCurrentTables.Add(GetTableName());
		}

		int nParam = BindSetParameters(bPrepare);
		if (nParam == 0 && !bForceTBModified)
			return UPDATE_NO_DATA;

		//Considero i parametri della WHERE clause
		nParam = 0;
		for (int i = 0; i <= m_pParamArray->GetUpperBound(); i++)
		{
			pElem = m_pParamArray->GetAt(i);
			pDataObj = pElem->GetOldDataObj();
			if (bPrepare)
			{
				m_pUpdateRowSet->AddParam(pElem->m_strBindName, *pDataObj);
				m_pUpdateRowSet->m_pParamArray->GetAt(m_pUpdateRowSet->m_pParamArray->GetUpperBound())->SetParamValue(*pDataObj);
			}
			else
			{
				m_pUpdateRowSet->m_pParamArray->GetAt(nParam)->SetParamValue(*pDataObj);
				nParam++;
			}
		}

		if (bPrepare)
		{
			m_pUpdateRowSet->m_strSQL = m_pUpdateRowSet->m_strSelect;
			m_pUpdateRowSet->BindParameters();
		}
		else
			m_pUpdateRowSet->FixupParameters();

		m_pUpdateRowSet->ExecuteNonQuery();
		return UPDATE_SUCCESS;
	}
	
	CATCH(MSqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		ThrowSqlException(e);
		return UPDATE_FAILED;
	}
	END_CATCH

}


//-----------------------------------------------------------------------------
int SqlTable::BuildSetClause(CString& strSelect, BOOL bCheckOldValues /*= TRUE*/)
{
	TRACE_SQL(_T("BuildSetClause"), this);

	int				nParam		= 0;
	SqlRecord*		pRec = GetRecord();
	DataObj*		pDataObj;
	CString			strColumnName;
	DataObj*		pOldDataObj;
	int				nPos;
	SqlBindingElem* pBindElem;
	// costruzione clausola di SET e considerando le sole colonne modificate			
	for (int i = 0; i <= m_pColumnArray->GetUpperBound(); i++)
	{
		pBindElem = m_pColumnArray->GetAt(i);
		if (pBindElem->IsReadOnly())
			continue;
		
			// non devo considerare gli autoincrement
		if (pBindElem->m_bAutoIncrement && m_pSqlConnection->GetDBMSType() == DBMS_SQLSERVER)
		{
			pBindElem->SetUpdatable(FALSE);
			continue;
		}
		
		pDataObj = pBindElem->m_pDataObj;
		CString strBindName = pBindElem->GetBindName();

		//devo escludere i campi local
		const SqlColumnInfo* pItem = pRec->GetColumnInfo(pDataObj);
		if (pItem && pItem->m_bVirtual)
			continue;

		// is TBCreated column its value must be skipped by update operations
		if (pDataObj == &(pRec->f_TBCreated))
		{
			pBindElem->SetUpdatable(FALSE);
			continue;
		}
		if (pDataObj == &(pRec->f_TBCreatedID))
		{
			pBindElem->SetUpdatable(FALSE);
			continue;
		}
		
		// is TBModified column its value is automatically set by the DBMS using the function 
		if (pDataObj == &(pRec->f_TBModified))
		{
			strSelect += strBindName + cwsprintf(_T(" = %s,"), GetDateFunction(m_pSqlConnection->GetDBMSType()));
			pBindElem->SetUpdatable(FALSE);
			continue;
		}
		
		
		//impr. 5936
		// is TBGuid column. If it's empty then I must valorize it with a new guid
		if (pDataObj == &(pRec->f_TBGuid) && pRec->f_TBGuid.IsEmpty())
		{
			pRec->f_TBGuid.AssignNewGuid();
			pBindElem->SetUpdatable();
			
			strSelect += strBindName + _T("=");
			strSelect += (strBindName.Find(_T("@"), 0) != 0) ? _T("@") + strBindName : strBindName;
			strSelect +=  _T(",");
				nParam++;
			continue;
		}

		if (bCheckOldValues)
		{
			// se ho un oldrecord (vedi DBTSlaveBuffered o TableUpdater) allora confronto i valori con quelli presenti nell'old
			if (m_pOldRecord)
			{
				nPos = (pBindElem->m_nSqlRecIdx == nEmptySqlRecIdx ? m_pRecord->Lookup(pDataObj) : pBindElem->m_nSqlRecIdx);
				pOldDataObj = m_pOldRecord->GetDataObjAt(nPos);
				if (pOldDataObj->IsEqual(*pDataObj))
				{
					pBindElem->SetUpdatable(FALSE);
					continue;
				}
			}
			else
			{
				//verifico con il vecchio valore contenuto nel buffer di binding
				if (pBindElem->SameValue())
				{
					pBindElem->SetUpdatable(FALSE);
					continue;
				}
			}
		}

		//TBModifiedID non lo inserisco nei parametri perch� potrebbe essere l'unico campo risulato cambiato
		//in questo eseguo la query di update solo se ForceTBModified = TRUE
		if (pDataObj == &(pRec->f_TBModifiedID))
		{
			strSelect += strBindName + cwsprintf(_T(" = %s,"), m_pSqlConnection->NativeConvert(pDataObj));
			pBindElem->SetUpdatable(FALSE);
			continue;
		}

		pBindElem->SetUpdatable();
		strSelect += strBindName + _T("=");
		strSelect += (strBindName.Find(_T("@"), 0) != 0) ? _T("@") + strBindName : strBindName;
		strSelect += _T(",");
		nParam++;
	}
	// overwrite last ',' with ' '
	strSelect.SetAt(strSelect.GetLength()-1, BLANK_CHAR);

	return nParam;
}

//-----------------------------------------------------------------------------
int SqlTable::BindSetParameters(BOOL bPrepare)
{
	int		nParam		= 0;
	DataObj* pDataObj = NULL;

	TRACE_SQL(_T("BindSetParameters"), this);

	// se bPrepare = TRUE
	// devo rieffettuare la prepare dello statement. Cambia il buffer di bind
	// devo riallocarmi la struttura di binding			
	// facciamo la binding dei parametri per la SET clause
	SqlBindingElem*  pBindElem = NULL;
	SqlBindingElem* pParameter = NULL;
	for (int i = 0; i <= m_pColumnArray->GetUpperBound(); i++)
	{
		pBindElem = m_pColumnArray->GetAt(i);
		pDataObj = pBindElem->m_pDataObj;
		
		if (!pBindElem->IsUpdatable())
			continue;		
		
		if (bPrepare)
			m_pUpdateRowSet->AddParam(pBindElem->m_strBindName, *pDataObj);	
		

		pParameter = m_pUpdateRowSet->m_pParamArray->GetAt(nParam);
		pParameter->SetParamValue(*pDataObj);
		pParameter->AssignOldDataObj(*pBindElem->m_pOldDataObj); //assegno il vecchio valore che uso poi nella where clause
		nParam++;
	}
	return nParam;
}

//-----------------------------------------------------------------------------
int SqlTable::GetKeySegmentCount()
{
	int nSize;
	if (m_pKeysArray && m_pKeysArray->GetSize() > 0) 
		nSize = m_pKeysArray->GetSize(); 
	else	
		nSize = (m_pRecord->m_pTableInfo->GetSqlUniqueColumns())
				? m_pRecord->m_pTableInfo->GetSqlUniqueColumns()->GetSize() 
				: 0;
	
	return nSize;
}

//-----------------------------------------------------------------------------
void SqlTable::BindKeyParameters(BOOL bPrepare, int& nParam)
{
	BOOL bMakeFilter = FALSE;
	CString strKeySegment;
	CString strKeyWhere; 
	
	DataObj* pOldDataObj = NULL;
	DataObj* pDataObj = NULL;

	SqlBindingElem* pBindElem = NULL;
	

	TRACE_SQL(_T("BindKeyParameters"), this);
	// costruzione WHERE Clause considerando i campi di chiave primaria
	// la prima volta devo costruirmi la stringa di where le volte successive
	// devo solo agire sui parametri
	if (m_pUpdateRowSet->m_strFilterKeys.IsEmpty())
		bMakeFilter = TRUE;
		

	int nSize = GetKeySegmentCount();
	if (nSize == 0)
		ThrowSqlException(cwsprintf(_TB("SqlTable::BindkeyParameters:  unable to proceed with query {0-%s}/nAccess keys not available"), (LPCTSTR)m_pUpdateRowSet->m_strSelect));

	//@@OLEDB da gestire errore di mancanza primary key o special columns 
	CString strParamName;
	for (int j = 0; j < nSize ; j++)
	{
		strKeySegment = (m_pKeysArray && m_pKeysArray->GetSize() > 0) 
						? m_pKeysArray->GetAt(j)
						: m_pRecord->m_pTableInfo->GetSqlUniqueColumns()->GetAt(j);
		
		pBindElem = m_pColumnArray->GetParamByName(strKeySegment);
		ASSERT(pBindElem);
		strParamName = pBindElem->m_strBindName + _T("_w");
		// la WHERE clause la costruisco solo la prima volta
		if (bMakeFilter)
		{
			if (!strKeyWhere.IsEmpty()) 
				strKeyWhere += _T(" AND ");
			
			strKeyWhere += strKeySegment;
			strKeyWhere += _T(" = ");
			strKeyWhere += (strParamName.Find(_T("@"), 0) != 0) ? _T("@") + strParamName : strParamName;
			
		}
		
		// verifico il valore del campo chiave 
		// se � stato modificato devo considerare quello prima della modifica
		// altrimenti quello invariato		
		pOldDataObj = GetOldDataObj(strKeySegment);
		m_bKeyChanged = pOldDataObj && !(pOldDataObj->IsEqual(*pBindElem->m_pDataObj));
		pDataObj = (m_bKeyChanged) ?  pOldDataObj : pBindElem->m_pDataObj;

		if (bPrepare)
		{
			m_pUpdateRowSet->AddParam(strParamName, *pDataObj);
			m_pUpdateRowSet->m_pParamArray->GetAt(m_pUpdateRowSet->m_pParamArray->GetUpperBound())->SetParamValue(*pDataObj);
		}
		else
			m_pUpdateRowSet->m_pParamArray->GetAt(nParam)->SetParamValue(*pDataObj);	
		nParam++;	
	} 
	if (bMakeFilter)
		m_pUpdateRowSet->m_strFilterKeys =  strKeyWhere;
}

// UPDATE <tablename> SET <colname1>=?[,<colname2>=?] WHERE KeySeg = ? AND....
//-----------------------------------------------------------------------------
int SqlTable::KeyedUpdate(BOOL bForceTBModified /*=FALSE*/)
{
	ASSERT(m_pRecord);
	ASSERT(m_pRecord->m_pTableInfo);
	
	// l'OldRecord mi viene passato dal DBTSlaveBuffered. Negli altri casi non � + necessario
	if (m_pOldRecord && m_pRecord->GetRuntimeClass() != m_pOldRecord->GetRuntimeClass())
	{
		ASSERT(FALSE);
		ThrowSqlException(_TB("SqlTable:: attempt to edit current record failed."));
	}

	TRY
	{
		CString strSelect;
		CString strSQL;
		//valorizzo il campo TBModifiedID con il WorkerID assegnato all'utente connesso 
		m_pRecord->f_TBModifiedID = AfxGetWorkerId();
		
		strSelect = szUpdate;
		strSelect += GetTableName();
		strSelect += szSet;

		// per prima cosa costruisco la stringa di query. Questa mi serve per confrontarla
		// con l'eventuale query di update fatta in precedenza. Due casi:
		// 1 - sono uguali devo solo modificare i parametri 
		// 2 - diverse. Devo fare tutti i passi relativi alla prepare e alla bind dei 
		// parametri. Passi eseguiti la prima volta 
		if (BuildSetClause(strSelect) == 0 && !bForceTBModified)
		{
			//rimetto il valore precedente
			if (m_pOldRecord)
				m_pRecord->f_TBModifiedID = m_pOldRecord->f_TBModifiedID;
			return UPDATE_NO_DATA;
		}

        BOOL bPrepare = OpenUpdateRowSet(strSelect);
				
		if (m_pUpdateRowSet)
		{
			m_pUpdateRowSet->m_strCurrentTables.RemoveAll();
			m_pUpdateRowSet->m_strCurrentTables.Add(GetTableName());
		}

		int nParam = BindSetParameters(bPrepare);
		if (nParam == 0 && !bForceTBModified)
			return UPDATE_NO_DATA; 
		
		//le chiavi all'inizio
		BindKeyParameters(bPrepare, nParam);
		if (*m_pContext->m_bOptimisticLock)
		{
			//lock ottimistico
			SqlBindingElem* pElem = NULL;
			SqlBindingElem* pParameter = NULL;
			DataObj* pDataObj = NULL;
			CString paramName;
			CString strBindName;
			for (int i = 0; i <= m_pColumnArray->GetUpperBound(); i++)
			{
				pElem = m_pColumnArray->GetAt(i);
				//devo considerare solo i cambi modificati 
				if (pElem->IsUpdatable())
				{
					strBindName = pElem->GetBindName();
					pDataObj = GetOldDataObj(strBindName);
					//nella where clause non devo considerare se cambiati gli eventuali campi chiave
					if (bPrepare)
					{
						paramName = strBindName + _T("_w");
						//� il parametro di tipo chiave aggiunto  nella BindKyeParameters
						if (m_pParamArray->ExistParam(paramName))
							continue;
						if (!m_pUpdateRowSet->m_strFilterOldValues.IsEmpty())
							m_pUpdateRowSet->m_strFilterOldValues += _T(" AND ");
						m_pUpdateRowSet->m_strFilterOldValues += strBindName + _T("=");
						m_pUpdateRowSet->m_strFilterOldValues += (paramName.Find(_T("@"), 0) != 0) ? _T("@") + paramName : paramName;
						m_pUpdateRowSet->AddParam(paramName, *pDataObj);
					}
					pParameter = m_pUpdateRowSet->m_pParamArray->GetAt(nParam);
					pParameter->SetParamValue(*pDataObj);
					nParam++;
				}
			}
		}
				
		if (bPrepare)
		{	
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strSelect;
			m_pUpdateRowSet->m_strSQL += szWhere;
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strFilterKeys;
			if (*m_pContext->m_bOptimisticLock && !m_pUpdateRowSet->m_strFilterOldValues.IsEmpty())
			{
				m_pUpdateRowSet->m_strSQL += _T(" AND ");				
				m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strFilterOldValues;
			}
			m_pUpdateRowSet->BindParameters();
			TRACE_SQL(cwsprintf(_T("KeyedUpdate %s (%s)"), (LPCTSTR)strSelect, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);	
		}
		else
		{
			m_pUpdateRowSet->FixupParameters();
			TRACE_SQL(cwsprintf(_T("KeyedUpdate(p) %s (%s)"), (LPCTSTR)strSelect, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}

		int nAffectedRow = m_pUpdateRowSet->ExecuteNonQuery();		
		//sono in presenza di un conflitto
		if (nAffectedRow == 0 && *m_pContext->m_bOptimisticLock)	
		{
			ThrowSqlException(new SqlException(cwsprintf(_TB("A conflict occured updating the record of the table {0} with keys {1}"), m_pRecord->GetTableName(), m_pRecord->GetPrimaryKeyDescription())));
			return UPDATE_FAILED;
		}		

		//FixupMandatoryColumns();
		return UPDATE_SUCCESS;
	}
	CATCH(MSqlException, e)
	{
		if (m_pUpdateRowSet->IsOpen())
			m_pUpdateRowSet->Close();
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		ThrowSqlException(e);
		return UPDATE_FAILED;
	}
	END_CATCH
}

// viene chiamata solo nel caso di database oracle, per il problema delle stringhe vuote-NULL
//-----------------------------------------------------------------------------
void SqlTable::StripRecordSpaces()
{
	for (int i = 0; i <= m_pRecord->GetUpperBound(); i++)
		if (m_pRecord->GetDataObjAt(i)->GetDataType() == DATA_STR_TYPE)
		{
			DataStr* pDataStr = (DataStr*) m_pRecord->GetDataObjAt(i);
			pDataStr->ReleaseBuffer();
			pDataStr->StripBlank();
		}
}

// Se e` stata cambiata la chiave primaria occorre passare il record contenente
// i dati precedenti (solo nel caso di Keyed....) per poter modificare il record
// giusto indicando i segmenti di chiave giusti con i dati precedenti alla modifica.
// Se i dati di chiave primaria non sono cambiati si puo` passare anche un NULL
//-----------------------------------------------------------------------------
int SqlTable::Update(SqlRecord* pOldRecord /*=NULL*/, BOOL bForceTBModified /*=FALSE*/)
{
	if (m_pRecord->IsVirtual())
		return UPDATE_SUCCESS;

	if (
			!m_pSqlConnection->IsAutocommit() &&
			!m_bForceAutocommit &&			
			!m_pSqlSession->IsTxnInProgress()
		)
	{
		ASSERT(FALSE);
		ThrowSqlException(_TB("SqlTable:: Update: Operation failed. Session used does not have the active transaction"));
	}
	
	int result = UPDATE_SUCCESS;
	TRY
	{
		ASSERT_VALID(this);
		ASSERT(IsOpen());
	
		TRACE_SQL(_T("Update"), this);

		if (!m_bUpdatable)
		{
			TRACE0("SqlTable::Update: error the cursor is read only.\n");
			ThrowSqlException(_TB("SqlTable:: Update: table open in read-only."));
		}

		// check mode
		if (m_nEditMode != addnew && m_nEditMode != edit)
		{
			TRACE0("SqlTable::Update: error must enter Edit or AddNew mode before updating.\n");
			ThrowSqlException(_TB("SqlTable::Attempt to update failed."));
		}
	
		// at least 1 field must be changed field   
		if (m_nEditMode == edit && m_pRecord && !m_pRecord->IsDirty())
		{
			m_nEditMode = noMode;
			return UPDATE_NO_DATA;
        }
				
		if (m_nEditMode == edit)
		{	
			m_pOldRecord = pOldRecord;			
			result = KeyedUpdate(bForceTBModified);	
		}
		else
		{
			// se nel SqlRecord � bindato il campo TBGuid allora assegno allo stesso 
			// un nuovo GUID
			if (m_pRecord->HasGUID())
				m_pRecord->f_TBGuid.AssignNewGuid(); 

			// l'exception viene lanciata esterna alla funzione, xch� ci sono
			// dei casi in cui viene chiamata solo la NativeInsert e non deve 
			// essere dato il messaggio
			if (!NativeInsert(FALSE))
				ThrowSqlException(cwsprintf(_TB("Attempt to add new record failed: {0-%s}"), m_strError), this);									
			result = UPDATE_SUCCESS;
		
		// gestione del numero di records presenti
			if (m_lCurrentRecord >= 0)
			{
				if (m_lRecordCount != -1)
					m_lRecordCount++;
				m_lCurrentRecord++;
			}
		}
		// se il database � Oracle devo togliere l'eventuale spazio inserito al posto
		// della stringa vuota
		//if (m_pSqlConnection->m_pProviderInfo->m_eDbmsType == DBMS_ORACLE)
		//	StripRecordSpaces();

		//@@AUDITING 
		if (result == UPDATE_SUCCESS)
		{
			m_pRecord->GetTableInfo()->GetSqlCatalogEntry()->TraceOperation
				(
					m_nEditMode == edit ? AUDIT_UPDATE_OP : AUDIT_INSERT_OP, 
					this
				);
		}
	}

	CATCH(MSqlException, e)
	{
		m_nEditMode = noMode;
		m_pOldRecord = NULL;
		m_bKeyChanged = FALSE;
		ThrowSqlException(e);
	}
	END_CATCH

	m_nEditMode = noMode;
	m_pOldRecord = NULL;
	m_bKeyChanged = FALSE;
	return result;
}           

// Per il parametro vedi il commento valido per la SqlTable::Update
//-----------------------------------------------------------------------------
void SqlTable::Delete(SqlRecord* pOldRecord /*= NULL*/)
{
	if (m_pRecord->IsVirtual())
		return;
	TRY
	{
		ASSERT(m_bUpdatable);
		ASSERT_VALID(this);
		ASSERT(IsOpen());

		if (
			!m_pSqlConnection->IsAutocommit() &&
			!m_bForceAutocommit &&			
			!m_pSqlSession->IsTxnInProgress()
		)
		{
			ASSERT(FALSE);
			ThrowSqlException(_TB("SqlTable::Delete: operation failed. used session has not actived transaction."));
		}
	
		if (m_nEditMode != noMode)
		{
			TRACE0("SqlTable::Delete: attempting to delete while still in Edit or AddNew mode.\n");
			ThrowSqlException(_TB("SqlTable::Delete:  attempt to delete failed."));
		}
	
		
		TRACE_SQL(_T("Delete"), this);

		m_pOldRecord = pOldRecord;
		if (!KeyedDelete())
			ThrowSqlException(_TB("SqlTable::attempt to delete current record failed."));
				
		// gestione del numero di records presenti
		if (m_lCurrentRecord > 0)
		{
			if (m_lRecordCount > 0)
				m_lRecordCount--;
			m_lCurrentRecord--;
		}
	
		//@@AUDITING 
		m_pRecord->GetTableInfo()->GetSqlCatalogEntry()->TraceOperation
				(
					AUDIT_DELETE_OP, 
					this					
				);

		// indicate on a deleted record
		m_bDeleted = TRUE;
		m_pOldRecord = NULL;
		m_bKeyChanged = FALSE;
		InitRow();
	}
	CATCH(MSqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		m_pOldRecord = NULL;
		m_bKeyChanged = FALSE;
		ThrowSqlException(e);
	}
	END_CATCH
}

//----------------------------------------------------------------------------------
BOOL SqlTable::MoreDataAvailable() const
{ 
	return m_sqlFetchResult != EndOfRowSet;
}

//----------------------------------------------------------------------------------
//ritorna la where in formato nativo del DB collegato convertendo tutti i parametri
CString SqlTable::GetNativeFilter() const
{
	CString strFilter;

	int nParam = 0;
	int nStart = 0;
	int nEnd = 0;

	for (nEnd = 0; nEnd < m_strFilter.GetLength(); nEnd++)
	{
		if (m_strFilter[nEnd] == _T('?'))
		{
			strFilter += m_strFilter.Mid (nStart, nEnd - nStart);

			DataType dataType = GetParamType (nParam);
			DataObj* pDataObj = DataObj::DataObjCreate (dataType);
			GetParamValue (nParam++, pDataObj);
			
			strFilter += m_pSqlConnection->NativeConvert(pDataObj);
			delete pDataObj;

			nStart = nEnd + 1;
		}
	}
	if (nEnd > nStart)
		strFilter += m_strFilter.Mid(nStart, nEnd - nStart);

	return strFilter;
}

//-----------------------------------------------------------------------------
void SqlTable::BuildCall()
{
	
}


//-----------------------------------------------------------------------------
void SqlTable::ExecuteStoredProcedure()
{
	TRACE_SQL(_T("Call"), this);
	m_sqlFetchResult = FetchOk;

	TRY
	{
		m_bEOFSeen = m_bBOF = m_bEOF = TRUE;
	

	if (!m_pColumnArray || m_pColumnArray->GetSize() == 0)
		ExecuteScalar();
	else
	{
		ExecuteCommand();
		if (m_pRowSet->IsNull())
		{
			TRACE_SQL(cwsprintf(_T("Query(n) %s (%s) is null"), (LPCTSTR)GetTableName(), (LPCTSTR)GetParamStr(this)), this);
			ThrowSqlException(_TB("SqlTable::Query(): Rowset is null"));
		}
		START_DB_TIME(DB_MOVE_NEXT)
		m_sqlFetchResult = m_pRowSet->Move();
		if (m_sqlFetchResult == Error)
		{
			TRACE_SQL(_T("Query fetch error"), this);
			STOP_DB_TIME(DB_MOVE_NEXT)
			ThrowSqlException(_T("Error in fetch operation"), this);
		}

		if (m_sqlFetchResult == FetchOk)
		{
			FixupColumns();
			// mette non dirty per ottimizzare gli update e modified per il refresh dei controls
			if (m_pRecord) 
				m_pRecord->SetFlags(FALSE, TRUE);
			m_bEOFSeen = m_bBOF = m_bEOF = FALSE;
			m_lCurrentRecord = 0;
			m_lRecordCount = 1;
		}
		else
		{
			InitRow();
			// If recordset empty, it doesn't make sense to check
			// record count and current record, but we'll set them anyway
			m_lCurrentRecord = SQL_CURRENT_RECORD_UNDEFINED;
			m_lRecordCount = 0;
		}
		STOP_DB_TIME(DB_MOVE_NEXT)
	}
	//se mi aspetto un solo record o non ci sono record da estrarre allora mi disconnetto subito
	if (m_bOnlyOneRecordExpected || m_lRecordCount == 0)
		Disconnect();
	}
	CATCH(MSqlException, e)
	{		
		ThrowSqlException(e);
	}
	END_CATCH
}



// gestione della stored procedure associata al SqlTable
//-----------------------------------------------------------------------------
void SqlTable::Call()
{
	if (
		!m_pRowSet ||
		!m_pParamsRecord ||
		!m_pParamsRecord->IsKindOf(RUNTIME_CLASS(SqlRecordProcedure)) ||
		!m_pParamsRecord->IsValid())

	{
		TRACE(L"Invalid SqlRecord in SqlTable::Call function");
		ASSERT(FALSE);
		LPCTSTR lpszName = (m_pParamsRecord && m_pParamsRecord->GetNamespace())
			? m_pParamsRecord->GetNamespace()->GetObjectName()
			: _T("");
		ThrowSqlException(cwsprintf
		(
			_TB("SqlTable::procedure identified by namespace {0-%s} not in database {1-%s}"),
			(LPCTSTR)lpszName,
			(LPCTSTR)m_pSqlConnection->m_strDBName
		)
		);
		return;
	}

	TRACE_SQL(_T("Call"), this);
	m_sqlFetchResult = FetchOk;

	TRY
	{
		m_bEOFSeen = m_bBOF = m_bEOF = TRUE;
	// database query
		if (m_bFirstQuery)
		{
			m_pRowSet->SetSqlCommandType(MSqlCommand::StoredProcedure);
			//eventuale bind delle colonne se la SP restituisce un rowset
			if (m_pRecord && m_pRecord->IsValid())
			{
				SelectAll();
				BindColumns();
			}

			//Bind dei parametri
			SqlProcedureParamInfo*	pParamInfo = NULL;
			SqlProcParamItem* pItem = NULL;
			const SqlProcedureParameters* pParameters = m_pParamsRecord->GetTableInfo()->m_pProcParameters;
			// i parametri devono essere inseriti nell'Accessor nell'esatto ordine previsto dalla stored procedure
			for (int i = 0; i <= pParameters->GetUpperBound(); i++)
			{
				pItem = NULL;
				pParamInfo = pParameters->GetAt(i);
				if (!pParamInfo)
				{
					ASSERT(FALSE);
					m_strSQL.Empty();
					return;
				}
				pItem = m_pParamsRecord->GetParamItemFromParamInfo(pParamInfo);
				if (!pItem)
				{
					ASSERT(FALSE);
					m_strSQL.Empty();
					return;
				}
				AddProcParam(pParamInfo, pItem->GetDataObj());
			}
			pItem = m_pParamsRecord->GetParamItemFromName(RETURN_VALUE);
			if (pItem)
				AddProcParam(pItem->m_pParameterInfo, pItem->GetDataObj());

			BindParameters();
			m_strSQL = m_strOldSQL = m_strTableName;
			m_bFirstQuery = FALSE;
		}
		else
		{
			m_pRowSet->Dispose();
			FixupParameters();
		}
		ExecuteStoredProcedure();
	}
	
	CATCH(SqlException, e)
	{
		m_pSqlSession->ShowMessage(
			cwsprintf
			(
				_TB("Errors on execute stored procedure {0-%s}.\r\n{1-%s}"),
				(LPCTSTR)m_strTableName,
				(LPCTSTR)e->m_strError
			)
		);
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		m_pRowSet->Dispose();
		ThrowSqlException(e);
	}
	END_CATCH
}

////-----------------------------------------------------------------------------
void SqlTable::DirectCall()
{
	TRY
	{
		BindParameters();
		BindColumns();
		ExecuteStoredProcedure();
	}
	CATCH(SqlException, e)
	{
		m_pSqlSession->ShowMessage(
			cwsprintf
			(
				_TB("Errors on execute stored procedure {0-%s}.\r\n{1-%s}"),
				(LPCTSTR)m_strTableName,
				(LPCTSTR)e->m_strError
			)
		);
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		m_pRowSet->Dispose();
		ThrowSqlException(e);
	}
	END_CATCH
}


// servono per la gestione dell'AUDITING
//-----------------------------------------------------------------------------
DataObj* SqlTable::GetOldDataObj(const CString& strColumnName) const
{
	if (m_pOldRecord)
		return m_pOldRecord->GetDataObjFromColumnName(strColumnName);

	SqlBindingElem* pElem = (m_pColumnArray) ? m_pColumnArray->GetParamByName(strColumnName) : NULL;
	return (pElem) ? pElem->m_pOldDataObj : NULL;
}

//-----------------------------------------------------------------------------
DataObj* SqlTable::GetDataObj(const CString& strColumnName) const
{
	SqlBindingElem* pElem = (m_pColumnArray) ? m_pColumnArray->GetParamByName(strColumnName) : NULL;
	return (pElem) ? pElem->m_pDataObj : NULL;
}

//-----------------------------------------------------------------------------
BOOL SqlTable::Parse(CXMLDocumentObject* pDoc, BOOL bWithAttributes /*TRUE*/)
{
	// Seleziono il nodo root
	CXMLNode* pRootNode = pDoc->SelectSingleNode(szDataTables);

	if (!pRootNode)
		return FALSE;

	// Seleziono la lista dei nodi
	CXMLNodeChildsList *pNodeList = pRootNode->GetChilds();
	
	if (!pNodeList)
		return FALSE;

	CXMLNode *pNode = NULL;

	for (int i=0; i<pNodeList->GetSize(); i++)
	{
		pNode = pNodeList->GetAt(i);
		CString strName;
		pNode->GetName(strName);

		// Verifico che il nome del nodo corrisponda con il nome della tabella
		if (strName == GetTableName())
		{
			TRY
			{
				AddNew();

				// Parso il nodo
				m_pRecord->Parse(pNode, bWithAttributes);

				Update();
			}
			CATCH(MSqlException, e)
			{
				TRACE(e->m_strError);
			}
			END_CATCH
		}
	}

	delete pRootNode;
	//delete pNodeList;

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlTable::UnParse(CXMLDocumentObject* pDoc, BOOL bWithAttributes /*TRUE*/, BOOL bSoapType /*= TRUE*/)
{
	if (!IsOpen())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// Creo la root del file xml
	pDoc->CreateInitialProcessingInstruction();
	pDoc->CreateRoot(szDataTables);
	
	CXMLNode* pNode = NULL;

	// Ciclo su tutti i record presenti
	while (!IsEOF())
	{
		// Creo il nodo per contenere il record
		pNode = pDoc->CreateRootChild(GetTableName());

		// Chiamo l'Unparse del SqlRecord
		GetRecord()->UnParse(pNode, bWithAttributes, FALSE, bSoapType);

		// Leggo i record successivo
		MoveNext();
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlTable::XmlExport(CString& strPath, BOOL bWithAttributes /*TRUE*/, BOOL bSoapType /*= TRUE*/)
{
	BOOL bOk;

	if (!IsOpen())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (IsEmpty())
		return TRUE;
	
	// Compongo il nome del file di export
	CString strFileRootName;
	strFileRootName.Format(_T("%s%s%s"), strPath, GetTableName(), szXmlExt);

	CXMLDocumentObject* pDoc = new CXMLDocumentObject(TRUE, FALSE);

	bOk = UnParse(pDoc, bWithAttributes, bSoapType);

	// Salvo l'xml
	pDoc->SaveXMLFile(strFileRootName);

	delete pDoc;

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL SqlTable::XmlImport(CString& strPath, BOOL bWithAttributes /*TRUE*/)
{
	BOOL bOk;

	if (!IsOpen())
	{
		ASSERT(FALSE);
		return FALSE;
	}

	// Compongo il nome del file di import
	CString strFileRootName;
	strFileRootName.Format(_T("%s%s%s"), strPath, GetTableName(), szXmlExt);

	// Verifico l'esistenza del file
	if (!ExistFile(strFileRootName))
		return FALSE;

	// Apro l'Xml
	CXMLDocumentObject* pDoc = new CXMLDocumentObject(FALSE, FALSE);

	if (!pDoc->LoadXMLFile(strFileRootName))
		return FALSE;

	bOk = Parse(pDoc, bWithAttributes);

	delete pDoc;

	return bOk;
}


//-----------------------------------------------------------------------------
BOOL SqlTable::IsEmpty ()
{ 
	return IsEOF() && IsBOF(); 
}

//-----------------------------------------------------------------------------
void SqlTable::BuildParamsList()
{
	m_strParamsList.Empty();

	if (m_pParamArray->GetSize() == 0)
		m_strParamsList = szEmptyParameter;
	else
		for (int i = 0; i <= m_pParamArray->GetUpperBound(); i++)
		{
			DataObj* pDataObj = m_pParamArray->GetDataObjAt(i);
			m_strParamsList += pDataObj->FormatData();
		}
}

//-----------------------------------------------------------------------------
BOOL SqlTable::LockTableKey (SqlRecord* pRec, const CString& sContextKey /*_T("")*/, LockRetriesMng* pRetriesMng /*NULL*/)
{
	if (sContextKey.IsEmpty())
		return __super::LockTableKey (this, pRec, GetTableName(), pRetriesMng);

	return __super::LockTableKey (this, pRec, sContextKey, pRetriesMng);
}

//-----------------------------------------------------------------------------
BOOL SqlTable::UnlockTableKey (SqlRecord* pRec, const CString& sContextKey /*_T("")*/)
{
	if (sContextKey.IsEmpty())
		return __super::UnlockTableKey (this, pRec,  GetTableName());

	return __super::UnlockTableKey (this, pRec, sContextKey);
}

//-----------------------------------------------------------------------------
BOOL SqlTable::IsTableKeyLocked (SqlRecord* pRec, const CString& sContextKey /*_T("")*/)
{
	return __super::IsTableKeyLocked (this, pRec, sContextKey.IsEmpty() ? GetTableName() : sContextKey);
}

//-----------------------------------------------------------------------------
void SqlTable::EnableLocksCache (const BOOL bValue /*TRUE*/)
{
	return __super::EnableLocksCache (this, bValue);
}

//-----------------------------------------------------------------------------
void SqlTable::ClearLocksCache (const CString sLockContextKey /*_T("")*/)
{
	return __super::ClearLocksCache (this, sLockContextKey);
}

//-----------------------------------------------------------------------------
void SqlTable::GetTableInfo	(SqlTableInfoArray& arTableInfo, BOOL bClear/*=TRUE*/)
{
	if (bClear) arTableInfo.RemoveAll();

	if (m_pTableArray)
	{
		for (int i = 0; i < m_pTableArray->GetSize(); i++)
		{
			SqlTableItem* pT = m_pTableArray->GetAt(i);
			//pu� essere NULL, ad esempio tramite il metodo:
			//void	FromTable (const CString& strTableName, const CString& strAlias, SqlRecord* = NULL);
			if (pT->m_pRecord)	
			{
				pT->m_pRecord->GetTableInfo(arTableInfo, FALSE);
			}
			else
			{
				const SqlCatalogEntry* pCE = 
					(m_pSqlConnection ? m_pSqlConnection : AfxGetDefaultSqlConnection())
						->GetCatalogEntry(pT->m_strTableName);
				if (pCE)
					arTableInfo.Add(pCE->m_pTableInfo);
			}
		}
	}
	else
	{
		GetRecord()->GetTableInfo(arTableInfo, FALSE);
	}
}

//-----------------------------------------------------------------------------
CString SqlTable::ParamToString(SqlBindingElem* pElem, BOOL bAddTagIn) const
{
	CString rep = GetConnection()->NativeConvert(pElem->GetDataObj());

	CString name = pElem->GetBindName();
	if (!name.IsEmpty())
	{
		int idx = ::ReverseFind(name, L"_w_"); //remove numeric postfix (Woorm wclause)
		if (idx < 0)
			idx = ::ReverseFind(name, L"_j_");
		if (idx < 0)
			idx = ::ReverseFind(name, L"_h_");
		if (idx < 0)
			idx = name.ReverseFind('_');
		if (idx > 0)
			name = name.Left(idx);
	}

	if (!name.IsEmpty() && !pElem->GetLocalName().IsEmpty())
		name += L" - ";
	name += pElem->GetLocalName();

	if (bAddTagIn)
	{
		if (name.CompareNoCase(L"Param")/* != 0*/)
			rep = L"{IN " + name + L" } ";
	}
	else
	{
		if (!name.IsEmpty() && name.CompareNoCase(L"Param")/* != 0*/)
			rep = L"/*" + name + L"*/ " + rep;
	}
#ifdef _DEBUG
	//rep = L" /*" + bindName + L"*/ " + rep;
#endif
	return rep;
}

//-----------------------------------------------------------------------------
CString SqlTable::ToString(BOOL bFormat/* = FALSE*/, BOOL bAddTagIn /*=FALSE*/, BOOL bAddTagCol /*=FALSE*/) const
{
	CString query = m_strSQL;

	if (bAddTagCol && this->m_pColumnArray && this->m_pColumnArray->GetCount())
	{
		int idx = ::FindWord(query, L"From");	/*TODO debole potrebbero esserci tab o newline*/
		if (idx > -1)
		{
			query = query.Mid(idx);
			BOOL first = true;
			CString columns = L"Select ";
			for (int i = 0; i < this->m_pColumnArray->GetCount(); i++)
			{
				SqlBindingElem* pElem = dynamic_cast<SqlBindingElem*>(this->m_pColumnArray->GetAt(i));
				if (pElem)
				{
					DataType dt = pElem->GetDataType();
					CString name = pElem->GetBindName(TRUE);

					CString title;
					int pos = name.Find('.');
					if (pos < 0)
						title = AfxLoadDatabaseString(name, this->GetTableName());
					else
					{
						CString colName = name.Left(pos);
						CString table = name.Mid(pos+1);
						title = AfxLoadDatabaseString(colName, table);
					}

					if (first)
					{
						first = false;	
					}
					else columns.Append(bFormat ? L", " : L",");

					columns.Append(name);
					columns.Append(cwsprintf (L" {COL %s", name));
					if (dt != DataType::String && dt != DataType::Text)
						columns.Append(cwsprintf(L" TYPE %s", dt.ToString()));
					if (!title.IsEmpty() && title != name)
						columns.Append(cwsprintf(L" TITLE \"%s\"", title));
					columns.Append(L"}");
				}
			}
			query = columns + ' ' + query;
		}
	}

	if (bFormat)
	{
		::ReplaceNoCase(query, L"SELECT ", L"SELECT\r\n");
		::ReplaceNoCase(query, L" FROM ", L"\r\nFROM\t");
		::ReplaceNoCase(query, L" ON ", L"\r\nON\r\n");
		::ReplaceNoCase(query, L" WHERE ", L"\r\nWHERE\r\n");
		::ReplaceNoCase(query, L" HAVING ", L"\r\nHAVING\r\n");
		::ReplaceNoCase(query, L" GROUP BY ", L"\r\nGROUP BY\t");
		::ReplaceNoCase(query, L" ORDER BY ", L"\r\nORDER BY\t");
		::ReplaceNoCase(query, L" AND ", L" AND \r\n");
		::ReplaceNoCase(query, L" OR ", L" OR \r\n");
		::ReplaceNoCase(query, L" IN ", L" IN ");
		::ReplaceNoCase(query, L" BETWEEN ", L" BETWEEN ");
		::ReplaceNoCase(query, L" OUTER ", L" OUTER ");
		::ReplaceNoCase(query, L" JOIN ", L" JOIN ");
		::ReplaceNoCase(query, L" LEFT ", L" LEFT ");
		::ReplaceNoCase(query, L" RIGHT ", L" RIGHT ");
		::ReplaceNoCase(query, L" IS ", L" IS ");
		::ReplaceNoCase(query, L" NULL ", L" NULL ");
		::ReplaceNoCase(query, L" NOT ", L" NOT ");

		query.Replace(L",", L", ");
	}

	if (!query.IsEmpty() && m_pParamArray)
	{
		ASSERT_VALID(m_pParamArray);
		if (m_pParamArray->GetSize())
		{
			int start = 0;
			for (int i = 0; i < m_pParamArray->GetSize(); i++)
			{
				SqlBindingElem* pElem = m_pParamArray->GetAt(i);
				ASSERT_VALID(pElem->GetDataObj());

				int nInx = query.Find('?', start);
				if (nInx >= 0)
				{
					query.Delete(nInx, 1);

					CString rep = ParamToString(pElem, bAddTagIn);
					
					query.Insert(nInx, rep);
					start = nInx + rep.GetLength();
				}
				else
				{
					int pos = -1; BOOL found = FALSE; CString bindName = pElem->GetBindName();
					while ((pos = query.Find(bindName, start) ) > -1)
					{
						int c = 0;
						if (pos > 0 && query[pos - 1] == '@')
						{
							pos--; c++;
						}
						query.Delete(pos, bindName.GetLength() + c);

						CString rep = ParamToString(pElem, bAddTagIn);

						query.Insert(pos, rep);
						start = pos + rep.GetLength();

						found = TRUE;
					}
					
					ASSERT_TRACE1(found, "Binded parameter placeholder %s was not found\n", bindName);
				}
			}
		}
	}
	return query;
}

//-----------------------------------------------------------------------------
CString SqlTable::GetQuery()
{
	if (m_pRecord && m_pRecord->IsVirtual())
		return _T("");

	if (!m_pRowSet)
		ThrowSqlException(cwsprintf(_TB("SqlTable::Query: before carry out query\n  table {0-%s} isn't open."), m_strTableName));

	if (m_bErrorFound)
		ThrowSqlException(cwsprintf(_TB("SqltTable::Query: errors occurred in the command construction phase on table {0-%s}"), m_strTableName));

	m_bEOFSeen = m_bBOF = m_bEOF = TRUE;
	m_bDeleted = FALSE;
	TRY
	{
		// database query
		if (m_bFirstQuery)
		{
			BuildSelect();
			TRACE_SQL(cwsprintf(_T("Query %s %s (%s)"), (LPCTSTR)GetTableName(), (LPCTSTR)m_strFilter, (LPCTSTR)GetParamStr(this)), this);
			Prepare();
			VERIFY(BindColumns());
			BindParameters();

			m_bFirstQuery = FALSE;
		}
		else
		{
			// Qualcuno ha cambiato la query senza chiudere e riaprire la table?
			if (m_strOldSQL != m_strSQL)
				ThrowSqlException(_TB("SqlTable::Query(): query changed without closing or reopening the table."));

			m_pRowSet->Dispose();

			ValorizeContextBagParameters();
			ValorizeRowSecurityParameters();
			FixupParameters();
		}
	}
		CATCH(MSqlException, e)
	{
		m_pSqlSession->ShowMessage(
			cwsprintf
			(
				_TB("Errors on select from table {0-%s}.\r\n{1-%s}"),
				(LPCTSTR)m_strTableName,
				(LPCTSTR)e->m_strError
			)
		);

		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		m_pRowSet->Dispose();
		return _T("");
	}
	END_CATCH

	return ToString(FALSE, FALSE, TRUE);
}

//-----------------------------------------------------------------------------
void SqlTable::StripBlankNearSquareBrackets()
{
	::StripBlankNearSquareBrackets(m_strSelect);
	::StripBlankNearSquareBrackets(m_strFrom);
	::StripBlankNearSquareBrackets(m_strFilter);
	::StripBlankNearSquareBrackets(m_strGroupBy);
	::StripBlankNearSquareBrackets(m_strHaving);
	::StripBlankNearSquareBrackets(m_strSort);

	::StripBlankNearSquareBrackets(m_strSQL);
}

//-----------------------------------------------------------------------------
#ifdef _DEBUG
// diagnostics
void SqlTable::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this); 
	AFX_DUMP0(dc, " SqlTable\n"); 
	dc << "\tTable Name = " << m_strTableName << "\n";
	dc << "\tSQL Statement = " << m_strSQL << "\n";

	SqlObject::Dump(dc);
}
#endif //_DEBUG
//-----------------------------------------------------------------------------

void SqlTable::SelectColumns(SqlTableStruct& sts)
{
	if (!sts.m_pColumns || sts.m_pColumns->GetSize() == 0)
		return;

	if (!m_pColumnArray)
		m_pColumnArray = new SqlColumnArray();
	else if (m_pColumnArray->GetSize())
	{
		ClearColumns();
		//m_pColumnArray = new SqlColumnArray(this);
	}

	//TRACE(_T("\nEnum columns %d, this:%x, pRec:%x\n"), sts.m_pColumns->GetSize(), this, m_pRecord);

	for (int i = 0; i < sts.m_pColumns->GetSize(); i++)
	{
		SqlBindingElem* pSbe = sts.m_pColumns->GetAt(i);
		if (!pSbe)
		{
			ASSERT(FALSE);
			continue;
		}

		//TRACE(_T("%s = %s rec:%d idx:%d, this:%x, pRec:%x\n"), pSbe->m_strBindName, pSbe->m_pDataObj->Str(), pSbe->m_nSqlRecIdx, pSbe->GetIndex(), this, m_pRecord);

		int idx = pSbe->GetIndex();
		DataObj* pObj = m_pRecord->GetDataObjAt(idx);
		if (!pObj)
		{
			ASSERT(FALSE);
			continue;
		}

		//ASSERT(i < m_pRecord->GetSize());

		Select(pSbe->GetBindName(TRUE), pObj);
	}
}

/////////////////////////////////////////////////////////////////////////////
//						class SqlTableStruct
/////////////////////////////////////////////////////////////////////////////
//
SqlTableStruct::SqlTableStruct ()
	:
	m_pSqlTableArray	(NULL),
	m_pTableInfoArray	(NULL),
	m_pRecord			(NULL),
	m_pColumns			(NULL)
{
}

//-----------------------------------------------------------------------------
SqlTableStruct::SqlTableStruct (const SqlTableStruct& t)
	:
	m_pSqlTableArray	(NULL),
	m_pTableInfoArray	(NULL),
	m_pRecord			(NULL),
	m_pColumns			(NULL)
{
	Assign (t);
}

//-----------------------------------------------------------------------------
SqlTableStruct::SqlTableStruct (const SqlTable& t)
{
	Assign (t);
}

//-----------------------------------------------------------------------------
SqlTableStruct::~SqlTableStruct ()
{
	SAFE_DELETE (m_pSqlTableArray);
	SAFE_DELETE (m_pTableInfoArray);
	SAFE_DELETE (m_pRecord);
	SAFE_DELETE (m_pColumns);
}

//-----------------------------------------------------------------------------
void SqlTableStruct::Assign (const SqlTableStruct& t)
{
	m_strSelectKeyword	= t.m_strSelectKeyword;	
	m_strSelect			= t.m_strSelect;
	m_strGroupBy		= t.m_strGroupBy;      
	m_strSort			= t.m_strSort;      	
	m_strFilter			= t.m_strFilter;
	m_strHaving			= t.m_strHaving;
	m_strFrom			= t.m_strFrom;

	SAFE_DELETE(m_pRecord);
	if (t.m_pRecord)
		m_pRecord		= t.m_pRecord->Clone();

	SAFE_DELETE(m_pSqlTableArray);
	if (t.m_pSqlTableArray)
		m_pSqlTableArray	= new SqlTableArray(*t.m_pSqlTableArray);

	SAFE_DELETE(m_pTableInfoArray);
	if (t.m_pTableInfoArray)
		m_pTableInfoArray	= new SqlTableInfoArray(*t.m_pTableInfoArray);

	SAFE_DELETE(m_pColumns);
	if (t.m_pColumns && t.m_pColumns->GetSize())
	{
		m_pColumns = new SqlBindingElemArray();

		//TRACE(_T("\nEnum columns %d\n"), t.m_pColumns->GetSize());

		for (int i = 0; i < t.m_pColumns->GetSize(); i++)
		{
			SqlBindingElem* pSbe = t.m_pColumns->GetAt(i);

			//TRACE(_T("%s = %s rec:%d idx:%d\n"), pSbe->m_strBindName, pSbe->m_pDataObj->Str(), pSbe->m_nSqlRecIdx, pSbe->GetIndex());

			m_pColumns->Add
				(
					new SqlBindingElem2 
							(
								pSbe->GetIndex(),
								pSbe->m_strBindName,
								pSbe->m_pDataObj,
								NoParam,
								pSbe->m_nSqlRecIdx
							)
				);
		}
	}
}

//-----------------------------------------------------------------------------
void SqlTableStruct::Assign (const SqlTable& t)
{
	m_strSelectKeyword	= t.m_strSelectKeyword;	

	m_strSelect			= t.GetBuildSelect();

	m_strGroupBy		= t.m_strGroupBy;      
	m_strSort			= t.m_strSort;      	

	SqlTable& rt		= const_cast<SqlTable&>(t);

	SAFE_DELETE(m_pRecord);
	if (rt.GetRecord())
		m_pRecord		= rt.GetRecord()->Clone();

	m_strFrom			= rt.m_strFrom;
	
	SAFE_DELETE(m_pSqlTableArray);
	if (rt.GetTableArray())
	{
		m_pSqlTableArray	= new SqlTableArray(*(rt.GetTableArray()));
	}

	SAFE_DELETE(m_pTableInfoArray);
	m_pTableInfoArray	= new SqlTableInfoArray();
	rt.GetTableInfo (*m_pTableInfoArray);

	WClause aWC (rt.m_pSqlConnection, NULL, *m_pTableInfoArray);
	m_strFilter = aWC.ToString(&rt);

	//TODO NON FUNZIONA: GetExpression(SqlTable*) non tiene minimamente conto del flag SetHavingClause
	//WClause aHC			(rt.m_pSqlConnection, m_pTableInfoArray); aHC.SetHavingClause();
	//m_strHaving			= aHC.GetExpression (&rt);

	SAFE_DELETE(m_pColumns);
	if (rt.m_pColumnArray && rt.m_pColumnArray->GetSize())
	{
		//TRACE(_T("\nEnum columns %d\n"), rt.m_pColumnArray->GetSize());
		m_pColumns = new SqlBindingElemArray();
		for (int i=0; i < rt.m_pColumnArray->GetSize(); i++)
		{
			SqlBindingElem* pSbe = rt.m_pColumnArray->GetAt(i);
			int nIdx = rt.GetRecord()->Lookup(pSbe->m_pDataObj);

			//TRACE(_T("%s = %s rec:%d idx:%d\n"), pSbe->m_strBindName, pSbe->m_pDataObj->Str(), pSbe->m_nSqlRecIdx, nIdx);

			m_pColumns->Add
				(
					new SqlBindingElem2 
						(
							nIdx,
							pSbe->m_strBindName,
							pSbe->m_pDataObj,
							NoParam,
							pSbe->m_nSqlRecIdx
						)
				);
		}
	}
}

//=============================================================================