
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
	CATCH(SqlException, e)\
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

// può essere o il nome della tabella o il suo alias
//-----------------------------------------------------------------------------
int SqlTableArray::GetTableIndex(const CString& strTableAliasName) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		// è il nome della tabella e non ho nessun alias
		// attenzione perchè posso avere più volte la stessa tabella con
		// una senza alias e le altre con alias.
		if (
			(
				GetAt(i)->m_strTableName.CompareNoCase(strTableAliasName) == 0 &&
				(
					GetAt(i)->m_strAliasName.IsEmpty() ||
					GetAt(i)->m_strAliasName.CompareNoCase(strTableAliasName) == 0
				)
			) ||	// è il nome di un alias
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
//						class ATLCommand
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
//===========================================================================
class ATLCommand : public CCommand<CManualAccessor>
{};
    

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
	m_pOldSqlSession		(NULL),
	m_bOwnSqlSession		(FALSE)
{	
    Initialize();
	m_pParamArray = new SqlParamArray(this);  
	m_pColumnArray = new SqlColumnArray(this);  	
}

//-----------------------------------------------------------------------------
SqlRowSet::SqlRowSet(SqlSession* pSqlSession, CBaseDocument* pDocument)
:
	SqlObject				(NULL, pDocument),
	m_pRowSet				(NULL),
	m_pSqlConnection		(NULL),	
	m_pSqlSession			(pSqlSession),
	m_pOldSqlSession		(m_pSqlSession),
	m_bOwnSqlSession		(FALSE)
{
	if (m_pSqlSession)
	{
		m_pSqlConnection = m_pSqlSession->GetSqlConnection();	
		//eredito il contesto della sessione
		SetContext(m_pSqlSession->m_pContext);
	}

	Initialize();
	m_pParamArray = new SqlParamArray(this);  
	m_pColumnArray = new SqlColumnArray(this); 	
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
	m_bSensitivity	= TRUE;
	m_bRemoveDeletedRow = FALSE;
	m_bUpdatable	= FALSE;
	m_bInit			= FALSE;
	m_bErrorFound	= FALSE;
	m_strErrorFound.Empty();
	m_eCursorType	= E_KEYSET_CURSOR;
	m_lRowCount		= 0;
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
		m_bOwnSqlSession = FALSE;

	}
	else
	{
		ASSERT(pSqlSession);
		TRACE(L"SqlRowSet::SetSqlSession: Invalid session\n");
	}
}


//sostituisce i nomi dei parametri ai punti ? (venivano usati con ODBC e poi OLEDB)
//-----------------------------------------------------------------------------
void SqlRowSet::SubstituteQuestionMarks(CString& strSQL)
{
	if (strSQL.IsEmpty() || strSQL.Find(_T('?')) < 0)
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
				DataObj* pObj = m_pParamArray->GetDataObjAt(nParamCount);
				CString strValue = m_pSqlConnection->NativeConvert(pObj);

				//strParamName = m_pParamArray->GetParamName(nParamCount);
				//strFilter += (strParamName.Find(_T("@"), 0) != 0) ? _T("@") + strParamName : strParamName;
				strFilter += strValue;

				nStart = nEnd + 1;
				nParamCount++;
			}
		}
	}
	if (nEnd > nStart)
		strFilter += strSQL.Mid(nStart, nEnd - nStart);

	strSQL = strFilter;
}



// Gestione dei cursori: VEDI DOCUMENTAZIONE TBOLEDB-REFGUIDE.DOC nella documentazione progetto
// E'possibile modificare il comportamento di default attraverso i due parametri booleani oppure
// passando in pDBPropSet le proprietá che deve assumere il RowSet
//-----------------------------------------------------------------------------
void SqlRowSet::Open(BOOL bUpdatable, CursorType eCursorType, BOOL bSensitivity /*= TRUE*/ )
{
	TRACE_SQL(_T("Open"), this);

	m_bUpdatable   = bUpdatable;
	m_eCursorType  = eCursorType;

	// m_bSensitivity può essere stato cambiato dai metodi SelectSqlFun e AddSelectKeyword poichè con funzioni di aggregazione
	// o  keyword (TOP, DISTINCT,..) nella SELECT il cursore non può aggiornare il rowset con le modifiche\cancellazioni fatte da altri cursori 
	m_bSensitivity	= m_bSensitivity && bSensitivity;
	m_bScrollable = (m_eCursorType == E_KEYSET_CURSOR || m_eCursorType == E_DYNAMIC_CURSOR);
	TRY
	{
		if (m_eCursorType == E_FORWARD_ONLY)
		{
			if (!m_bUpdatable && !m_bOwnSqlSession && m_pSqlConnection->GetDBMSType() == DBMS_SQLSERVER)
			{
				if (m_pSqlSession)
					m_pOldSqlSession = m_pSqlSession;

				m_pSqlSession = m_pSqlConnection->GetNewSqlSession(m_pSqlConnection->m_pContext);
				SetSqlSession(m_pSqlSession);
				m_bOwnSqlSession = TRUE;
			}
			else
				m_eCursorType = E_FAST_FORWARD_ONLY;

		}		
	}
	CATCH(SqlException, e)
	{
		TRACE(e->m_strError);
	}
	END_CATCH

	if (!m_pSqlSession)
		ThrowSqlException(_TB("SqlRowSet::Open: attempt to open a rowset in an invalid session."));

	if (m_pRowSet)
		ThrowSqlException(_TB("SqlRowSet::Open: Rowset already open"));


	m_pRowSet = new ATLCommand;
	
	// se utilizzo le transazioni non posso aprire un cursore updatable sulla sessione
	// di default. Questa la posso usare solo in caso di lettura di parametri globali
	// quando non ho il contesto
	if (
			m_bUpdatable && 
			!m_pSqlConnection->IsAutocommit() && 
			m_pSqlSession == AfxGetDefaultSqlSession()
		)
	{
		delete m_pRowSet;
		ThrowSqlException(_TB("SplRowSet:: Open: unable to use the primary session to open tables in write mode\r\n"));
	}

	m_pSqlSession->AddCommand(this);
}

//-----------------------------------------------------------------------------
void SqlRowSet::Open(
						BOOL bUpdatable /*=FALSE*/, 
						BOOL bScrollable /*=FALSE*/, 
						BOOL bSensitivity /*= TRUE*/
					 )
{             
	return SqlRowSet::Open(bUpdatable, (bScrollable) ? E_KEYSET_CURSOR : m_pSqlConnection->m_eROForwardCursor, bSensitivity);	
}

//-----------------------------------------------------------------------------
void SqlRowSet::SetProperties() 
{
	if (m_bInit)
		return;

	TRACE_SQL(_T("SetProperties"), this);

	m_bInit = TRUE;
	
	START_DB_TIME(DB_ADD_PROPERTIES)
	
	// nel caso in cui il cursore venga chiuso e riaperto
	CreatePropertySet();
	SetPropGUID(DBPROPSET_ROWSET);	

	if (m_eCursorType == E_NO_CURSOR) //non utilizzo cursori
	{
		STOP_DB_TIME(DB_ADD_PROPERTIES)
		return;
	}
		
	//va bene anche per il FAST-FORWARDONLY (verificare x ORACLE)
	// questo vale solo se nella tabella della query non sono presenti campi
	// di tipo text-image, nel caso si trasforma in un cursore dinamico
	if (m_eCursorType != E_FORWARD_ONLY)
		AddProperty(DBPROP_SERVERCURSOR, true);	
		
	switch (m_eCursorType)
	{
		case E_DYNAMIC_CURSOR:
				AddProperty(DBPROP_CANFETCHBACKWARDS, true); 
				AddProperty(DBPROP_CANSCROLLBACKWARDS, true);
				if (m_bSensitivity)
				{
					AddProperty(DBPROP_OTHERINSERT, true);
					AddProperty(DBPROP_OTHERUPDATEDELETE, true);	
					if (m_pSqlConnection->GetDBMSType() == DBMS_ORACLE) //Bug fix 13772
						AddProperty(DBPROP_REMOVEDELETED, true);						
				}
				else
					if (m_bRemoveDeletedRow)
						AddProperty(DBPROP_REMOVEDELETED, true);
				break;

		case E_KEYSET_CURSOR: 
				AddProperty(DBPROP_CANFETCHBACKWARDS, true); 
				AddProperty(DBPROP_CANSCROLLBACKWARDS, true);
				if (m_bSensitivity)
				{
					AddProperty(DBPROP_OTHERUPDATEDELETE, true);	
					if (m_pSqlConnection->GetDBMSType() == DBMS_ORACLE) //Bug fix 13772
						AddProperty(DBPROP_REMOVEDELETED, true);	
				}
				else
					if (m_bRemoveDeletedRow)
						AddProperty(DBPROP_REMOVEDELETED, true);	
				break;
		default: break;
	}			

	STOP_DB_TIME(DB_ADD_PROPERTIES)
}


//-----------------------------------------------------------------------------
BOOL SqlRowSet::IsOpen() const
{ 
	return m_pRowSet != NULL; 
}	


// deve evitare di usare la ClearQuery perche` il record potrebbe essere stato
// cancellato da fuori ma il puntatore non essere ancora NULL;
//-----------------------------------------------------------------------------
void SqlRowSet::Close()
{
	ASSERT_VALID(this);
	TRACE_SQL(_T("Close"), this);

	ClearRowSet();
	if (m_pRowSet)
	{
		delete m_pRowSet;
		m_pRowSet = NULL;
	}
	
	RemovePropertySet();
	Initialize();

	if (m_pSqlSession)
	{
		if (m_bOwnSqlSession)
		{
			m_pSqlSession->Close();
			delete m_pSqlSession;
			m_pSqlSession = NULL;
			m_bOwnSqlSession = FALSE;
			m_pSqlSession = m_pOldSqlSession;
		}
		else
			m_pSqlSession->RemoveCommand(this);				
	}
}
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
	
	m_pParamArray = new SqlParamArray(this);
}

//-----------------------------------------------------------------------------
void SqlRowSet::ClearColumns()	
{ 
	TRACE_SQL(_T("ClearColumns"), this);

	if (m_pColumnArray)
		delete m_pColumnArray; 
	
	m_pColumnArray = new SqlColumnArray(this);
}

// Pulisce tutti i parametri, le colonne e le stringhe preposte alla query
//-----------------------------------------------------------------------------
void SqlRowSet::ClearRowSet()
{	
	TRACE_SQL(_T("ClearRowSet"), this);

	m_lRowCount = 0; 

	m_strSQL		.Empty();	// SQL completa
	m_strSelect		.Empty();	// Select-Update-Delete clause        
	m_strFilter		.Empty();	// WHERE clause	
	m_strCurrentTables.RemoveAll();

	if (m_pRowSet)
	{
		START_DB_TIME(DB_CLOSE_ROWSET)
		m_pRowSet->Close();
		m_pRowSet->ReleaseCommand();
		STOP_DB_TIME(DB_CLOSE_ROWSET)
	}
	ClearColumns();
	ClearParams	();
}


// Aggiunge un nuovo parametro controllando che non sia gia` stato definito 
// un parametro con lo stesso nome e che la Table non sia ancora aperta perche`
// il numero di parametri dopo la open non puo` essere modificato
//-----------------------------------------------------------------------------
void SqlRowSet::AddParam (const CString& strParamName, const DataObj& aDataObj, const DBPARAMIO& eType /*= DBPARAMIO_INPUT*/, int nInsertPos /*=-1*/)
{                  
	DBLENGTH nLen = 0;
	if (aDataObj.GetDataType() == DATA_STR_TYPE)
	{
		if ((aDataObj.GetColumnLen() <= 0))
			nLen = (m_pSqlConnection->m_bUseUnicode) ? PARAM_ALLOC_SIZE_UNI : PARAM_ALLOC_SIZE;  // x i parametri da woorm x cui non ho l'AllocSize		
		else
			nLen = aDataObj.GetColumnLen();
	}
	else
		nLen = aDataObj.GetOleDBSize();

	AddParam(strParamName, aDataObj.GetDataType(), nLen, eType, _T(""), nInsertPos);
}

//-----------------------------------------------------------------------------
void SqlRowSet::AddDataTextParam(const CString& strParamName, const DataObj& aDataObj, const CString& strColumnName, const DBPARAMIO& eType /*= DBPARAMIO_INPUT*/)
{
	DBLENGTH nLen = 0;
	if (aDataObj.GetDataType() != DATA_TXT_TYPE)
	{
		ASSERT(FALSE);
		return;
	}

	AddParam(strParamName, aDataObj.GetDataType(), nLen, eType, strColumnName);
}


//-----------------------------------------------------------------------------
BOOL SqlRowSet::IsNull() const
{
	return (m_pRowSet == NULL);
}

//-----------------------------------------------------------------------------
HRESULT SqlRowSet::CreateAccessor(int nBindEntries, void* pBuffer, ULONG nBufferSize)
{
	 return m_pRowSet->CreateAccessor(nBindEntries, pBuffer, nBufferSize);
}

//-----------------------------------------------------------------------------
HRESULT SqlRowSet::CreateParameterAccessor(int nBindEntries, void* pBuffer, ULONG nBufferSize)
{
	 return m_pRowSet->CreateParameterAccessor(nBindEntries, pBuffer, nBufferSize);
}

//-----------------------------------------------------------------------------
void SqlRowSet::AddParameterEntry(ULONG nOrdinal, WORD wType, ULONG nColumnSize, void* pData, void* pLength, void* pStatus, DWORD eParamIO)
{
	m_pRowSet->AddParameterEntry(nOrdinal, wType, nColumnSize, pData, pLength, pStatus, eParamIO);
}

//-----------------------------------------------------------------------------
void SqlRowSet::AddBindEntry(ULONG nOrdinal, WORD wType, ULONG nColumnSize, void* pData, void* pLength, void* pStatus)
{
	m_pRowSet->AddBindEntry(nOrdinal, wType, nColumnSize, pData, pLength, pStatus);
}

//-----------------------------------------------------------------------------
HRESULT SqlRowSet::SetParameterInfo(ULONG ulParams, const ULONG* pOrdinals, void* pParamInfo)
{
	return m_pRowSet->SetParameterInfo(ulParams, pOrdinals, (DBPARAMBINDINFO*)pParamInfo);
}

//-----------------------------------------------------------------------------
void SqlRowSet::CreateStreamObject(int nBindEntry)
{
	DBOBJECT* pObject = new DBOBJECT;
	pObject->dwFlags = STGM_READ;
	pObject->iid = IID_ISequentialStream;

	m_pRowSet->m_pEntry[nBindEntry].pObject = pObject;			
}

//-----------------------------------------------------------------------------
void SqlRowSet::CreateParamStreamObject(int nBindEntry)
{
	DBOBJECT* pObject = new DBOBJECT;
	pObject->dwFlags = STGM_READ;
	pObject->iid = IID_ISequentialStream;

	m_pRowSet->m_pParameterEntry[nBindEntry].pObject = pObject;			
}

//-----------------------------------------------------------------------------
void SqlRowSet::AddParam 
		(
			const CString&  strParamName, 
			const DataType& nDataType, 
			const DBLENGTH& nLen, 
			const DBPARAMIO& eType, /*= DBPARAMIO_INPUT*/
			const CString&  strColumnName,
			int nInsertPos /*=-1*/
		)
{
	if (!strParamName.IsEmpty() && m_pParamArray->ExistParam(strParamName))
		ThrowSqlException(_TB("SqlRowSet::AddParam: Query parameters duplicated."));
	
	m_pParamArray->Add(strParamName, nDataType, m_pSqlConnection->GetSqlDataType(nDataType), nLen, nEmptySqlRecIdx, eType, strColumnName, nInsertPos);	
}

//-----------------------------------------------------------------------------
void SqlRowSet::AddProcParam(const CString& strParamName, short nOleDbParamType, DataObj* pDataObj)
{
	if (!strParamName.IsEmpty() && m_pParamArray->ExistParam(strParamName))
		ThrowSqlException(_TB("SqlRowSet::AddProcParam: procedure parameters duplicated."));	

	DBPARAMIO eParamType; 
	switch (nOleDbParamType)
	{
		case DBPARAMTYPE_OUTPUT:
		case DBPARAMTYPE_RETURNVALUE:
			eParamType = DBPARAMIO_OUTPUT;
			break;
		case DBPARAMTYPE_INPUT:
			eParamType = DBPARAMIO_INPUT;
			break;
		case DBPARAMTYPE_INPUTOUTPUT:
			eParamType = DBPARAMIO_INPUT | DBPARAMIO_OUTPUT;
			break;
		default:
			eParamType = DBPARAMIO_INPUT | DBPARAMIO_OUTPUT;
			break;
	}	

	DBLENGTH nLen = 0;
	if (pDataObj->GetDataType() == DATA_STR_TYPE && pDataObj->GetColumnLen() <= 0)
		pDataObj->SetAllocSize(PARAM_ALLOC_SIZE);  // x i parametri da woorm x cui non ho l'AllocSize

	m_pParamArray->Add
					(
						strParamName, 
						pDataObj, 
						m_pSqlConnection->GetSqlDataType(pDataObj->GetDataType()), 
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
DataType SqlRowSet::GetParamType (const CString& strParamName) const
{
	return m_pParamArray->GetParamDataType(strParamName);
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

// I need to call Setparameterinfo method after the command creating for the following subquery problem
// see  MSDN: Q235053
// PRB: E_FAIL Returned from Prepare() When SQL Statement Contains a Parameter in a Subquery
//-----------------------------------------------------------------------------
HRESULT SqlRowSet::Prepare()
{
	TRACE_SQL(cwsprintf(_T("Prepare %s"), m_strSQL), this);

	START_DB_TIME(DB_PREPARE)
	if 	(
			!(
				Check(m_pRowSet->Create(*((CSession*)m_pSqlSession->GetSession()), m_strSQL)) &&
				Check(m_pParamArray->SetParameterInfo()) &&
				Check(m_pRowSet->Prepare())
			  )
		)
	{
		STOP_DB_TIME(DB_PREPARE)
		if (m_pRowSet->m_spRowset)
			ThrowSqlException(m_pRowSet->m_spRowset, IID_IRowset, m_hResult, this);	
		else
			ThrowSqlException(m_pRowSet->m_spCommand , IID_ICommand, m_hResult, this);
	}
	STOP_DB_TIME(DB_PREPARE)

	return m_hResult;	
};

//-----------------------------------------------------------------------------
CString GetParamStr(SqlRowSet* pRowSet)
{
	if	(!IsTraceSQLEnabled() || !pRowSet->m_pParamArray)
		return _T("");

	CString strParams;
	for (int i = 0; i <= pRowSet->m_pParamArray->GetUpperBound(); i++)
	{
		SqlBindingElem* pElem = pRowSet->m_pParamArray->GetAt(i);
		strParams += _T("'") + pElem->GetDataObj()->Str() + _T("' ");
	}
	
	return strParams;
}

//-----------------------------------------------------------------------------
void SqlRowSet::OpenRowSet(DBROWCOUNT* pRowsAffected /*= NULL*/)
{
	TRACE_SQL(cwsprintf(_T("OpenRowSet %s"), (LPCTSTR)GetParamStr(this)), this);

	SetProperties();

	START_DB_TIME(DB_OPEN_ROWSET)
	m_lRowCount = 0;

	if (!Check(m_pRowSet->Open((CDBPropSet*)m_pDBPropSet, &m_lRowCount)))
	{
		if (m_hResult == DB_E_ERRORSOCCURRED)
		{
			if (m_pColumnArray && !m_pColumnArray->CheckStatus(m_pContext->GetDiagnostic()))
				ShowMessage(TRUE); 
			else
			{
				ASSERT_TRACE1(FALSE,
					    "SqlTable::OpenRowSet: query %s \nit's possible that there are some parameters added with AddParam method\n and not set with the SetParamValue\n",
						(LPCTSTR)m_strSQL
					   );
			}
		}
		STOP_DB_TIME(DB_OPEN_ROWSET)

		if (m_pRowSet->m_spRowset)
			ThrowSqlException(m_pRowSet->m_spRowset , IID_IRowset, m_hResult, this);
		else
			ThrowSqlException(m_pRowSet->m_spCommand , IID_ICommand, m_hResult, this);
	}
	if (pRowsAffected)
		*pRowsAffected = m_lRowCount;

	STOP_DB_TIME(DB_OPEN_ROWSET)
}

//-----------------------------------------------------------------------------
BOOL SqlRowSet::BindColumns()
{
	TRACE_SQL(_T("BindColumns"), this);

	return m_pColumnArray->BindColumns();
}

//-----------------------------------------------------------------------------
BOOL SqlRowSet::BindParameters()
{
	TRACE_SQL(_T("BindParameters"), this);

	return m_pParamArray->BindParameters();
}	

//-----------------------------------------------------------------------------
BOOL SqlRowSet::FixupColumns()
{
	TRACE_SQL(_T("FixupColumns"), this);

	return m_pColumnArray->FixupColumns();
}

//-----------------------------------------------------------------------------
BOOL SqlRowSet::FixupBuffer()
{
	TRACE_SQL(_T("FixupBuffer"), this);

	return m_pColumnArray->FixupBuffer(FALSE);		
}


//-----------------------------------------------------------------------------
BOOL SqlRowSet::InitBuffer()
{
	TRACE_SQL(_T("InitBuffer"), this);

	return m_pColumnArray->InitBuffer();
}

//-----------------------------------------------------------------------------
BOOL SqlRowSet::FixupParameters()
{
	TRACE_SQL(_T("FixupParameters"), this);

	return	m_pColumnArray->RibindColumns() && m_pParamArray->FixupParameters();
}

// esecuzione query e script
//-----------------------------------------------------------------------------
void SqlRowSet::ExecuteQuery(LPCTSTR lpszSQL)
{
	m_lRowCount = 0;

	if (!IsOpen())
	{
		ASSERT(FALSE);
		return;
	}
	if (!
			(
				Check(m_pRowSet->Create(*((CSession*)m_pSqlSession->GetSession()), lpszSQL)) && 
				Check(m_pRowSet->Open((CDBPropSet*)m_pDBPropSet, &m_lRowCount))
			)
		)
	{
		if (m_pRowSet->m_spRowset)
			ThrowSqlException(m_pRowSet->m_spRowset , IID_IRowset, m_hResult, this);
		else
			ThrowSqlException(m_pRowSet->m_spCommand , IID_ICommand, m_hResult, this);
	}
}

//-----------------------------------------------------------------------------
void SqlRowSet::ExecuteScript(LPCTSTR lpszFileName)
{
	//@@TODO
	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
BOOL SqlRowSet::Check(HRESULT hResult)
{
	return SqlObject::Check(hResult) ||
			hResult == DB_S_ENDOFROWSET ||
			hResult == DB_E_DELETEDROW;			
}

//-----------------------------------------------------------------------------	
void SqlRowSet::GetErrorString(HRESULT nResult, CString& m_strError)
{
	if (!m_strSQL.IsEmpty())
		m_strError = cwsprintf(_TB("The following error occurred while executing query {0-%s}:"), m_strSQL);
	switch (nResult)
	{
		case DB_S_ENDOFROWSET:			
			m_strError += _TB("Attempt positioning beyond rowset beginning or end.");
			break;
		case DB_S_ERRORSOCCURRED:
		case DB_E_ERRORSOCCURRED:
			m_strError += _TB("Error executing query");
			break;			
		case DB_E_PARAMNOTOPTIONAL:
			m_strError += _TB("Parameters in query not extended\r\n");
			break;
		case DB_E_ROWSNOTRELEASED:
			m_strError += _TB("Row set must be regenerated. Release rows.");
			break;
		case DB_SEC_E_PERMISSIONDENIED:
			m_strError += _TB("Consumer does not have sufficient permission to reset the next set position."); 
			break;
		case DB_S_ROWLIMITEXCEEDED:
			m_strError = _TB("The number of rows specified in cRow is greater than tha maximum authorized by the rowset");
			break;
		case DB_S_STOPLIMITREACHED:
			m_strError = _TB("The fetching operation requested a subsequent command execution.  The execution was stopped because a resource limit has been reached");				
			break;
		case DB_E_BADBINDINFO:
			m_strError = _TB("The accessor has binding information for more than one column.");
			break;
		case DB_E_CANTFETCHBACKWARDS:
			m_strError = _TB("Unable to fetch backwards in the rowset.");
			break;
		case DB_E_CANTSCROLLBACKWARDS:
			m_strError = _TB("Unable to scroll backwards in the rowset.");
			break;
		case DB_E_BADACCESSORTYPE:
			m_strError = _TB("Wrong accessor type");
			break;
		case DB_E_BADROWHANDLE:
			m_strError = _TB("Invalid Handle Row");
			break;
		case DB_E_NEWLYINSERTED:
			m_strError = _TB("The provider is unable to identify the row for which the insertion was requested.");
			break;
		case DB_S_COLUMNSCHANGED:
			m_strError = _TB("Column arrangement not specified in object that created the rowset.");
			break;
		case DB_S_COMMANDREEXECUTED:
			m_strError = _TB("Command associated to rowset rerun.");
			break;
		case DB_E_CANCELED:
			m_strError = _TB("Irowset::RestartPosition erased during notice.  Fetch position remained unchanged");
			break;
		case DB_E_CANNOTRESTART:
			m_strError = _TB("The rowset is built over an active datastream and the position cannot be re-initialized.");
			break;		
		case DB_E_CANTTRANSLATE:
			m_strError = _TB("OLEDB: The last command was set using ICommandStream::SetCommandStream, not ICommandText::SetCommandText.");
			break;
		case DB_E_NOCOMMAND:
			m_strError = _TB("No query text associated to command.");
			break;
		case DB_E_NOTPREPARED:
			m_strError = _TB("Provider unable to obtain information on the parameters.  The command goes to unprepared status, and no information on the parameters is specified with ICommandWithParameters::SetParameterInfo.");
			break;
		case DB_E_PARAMUNAVAILABLE:
			m_strError = _TB("ICommandWithParameters::SetParametersInfo not invoked to insert information on parameters.");
			break;
		case DB_E_ERRORSINCOMMAND:
			m_strError = _TB("Command text contains syntax errors.");
			break;
		case DB_E_NOTABLE:
			m_strError = _TB("Nonexistent table or view in database");
			break;
		case DB_E_OBJECTOPEN:
			m_strError = _TB("Rowset already open in command.");
			break;
		case DB_E_BADORDINAL:
			m_strError = _TB("Parameter with invalid ordinal number.");
			break;
		case DB_E_BADPARAMETERNAME:
			m_strError = _TB("Parameter name invalid.");
			break;
		case DB_E_BADTYPENAME :
			m_strError = _TB("Parameter name type invalid.");
			break;	
	}
}

//-----------------------------------------------------------------------------
DBMSType SqlRowSet::GetDBMSType () const
{
	return m_pSqlSession && m_pSqlSession->GetDBMSType() != DBMS_UNKNOWN ? 
				m_pSqlSession->GetDBMSType() : 
				m_pSqlConnection->GetDBMSType();
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

	m_pUpdateRowSet				(NULL),
	m_bDBTMasterQuery			(FALSE),
	m_bForceAutocommit			(FALSE),
	m_pKeysArray				(NULL),
	m_pTableArray				(NULL),
	m_pExtraColumns				(NULL),
	m_pOldRecord				(NULL),
	m_pMandatoryColTable		(NULL),
	m_pDataCachingContext		(NULL),
	m_bCanUseDataCaching		(FALSE),
	m_bCurrentReadFromDatabase	(FALSE),
	m_bForceQuery				(FALSE),
	m_pSymTable					(NULL),
	m_bSkipContextBagParameters	(FALSE),
	m_bSkipRowSecurity			(FALSE),
	m_bSelectGrantInformation	(FALSE),
	m_pRSSelectWorkerID			(NULL),
	m_pRSFilterWorkerID			(NULL)	
{
	m_pRecord 	= NULL;
	Initialize();
}

//-----------------------------------------------------------------------------
SqlTable::SqlTable(SqlSession* pSqlSession)
	:
	SqlRowSet					(pSqlSession),
	IDisposingSourceImpl		(this),

	m_pUpdateRowSet				(NULL),
	m_bDBTMasterQuery			(FALSE),
	m_bForceAutocommit			(FALSE),
	m_pKeysArray				(NULL),
	m_pTableArray				(NULL),
	m_pExtraColumns				(NULL),
	m_pOldRecord				(NULL),
	m_pMandatoryColTable		(NULL),
	m_pDataCachingContext		(NULL),
	m_bCanUseDataCaching		(FALSE),
	m_bCurrentReadFromDatabase	(FALSE),
	m_bForceQuery				(FALSE),
	m_pSymTable					(NULL),
	m_bSkipContextBagParameters	(FALSE),
	m_bSkipRowSecurity			(FALSE),
	m_bSelectGrantInformation	(FALSE),
	m_pRSSelectWorkerID			(NULL),
	m_pRSFilterWorkerID			(NULL)
{
	m_pRecord 	= NULL;	
	Initialize();
}

//-----------------------------------------------------------------------------
SqlTable::SqlTable(SqlRecord* pRecord, SqlSession* pSqlSession, /*=NULL*/CBaseDocument* pDocument /*NULL*/)
	:
	SqlRowSet					(pSqlSession, pDocument),
	IDisposingSourceImpl		(this),

	m_pUpdateRowSet				(NULL),
	m_bDBTMasterQuery			(FALSE),
	m_bForceAutocommit			(FALSE),
	m_pKeysArray				(NULL),
	m_pTableArray				(NULL),
	m_pExtraColumns				(NULL),
	m_pOldRecord				(NULL),
	m_pMandatoryColTable		(NULL),
	m_pDataCachingContext		(NULL),
	m_bCanUseDataCaching		(FALSE),
	m_bCurrentReadFromDatabase	(FALSE),	
	m_bForceQuery				(FALSE),
	m_pSymTable					(NULL),
	m_bSkipContextBagParameters	(FALSE),
	m_bSkipRowSecurity			(FALSE),
	m_bSelectGrantInformation	(FALSE),
	m_pRSSelectWorkerID			(NULL),
	m_pRSFilterWorkerID			(NULL)
{
	m_pRecord	= pRecord;
	
	//cambio la connessione al SqlRecord
	// questo viene fatta se quella associata al record é differente da
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
void SqlTable::Initialize()
{
	m_bCachingSettingStatus = AfxGetDataCachingSettings() && AfxGetDataCachingSettings()->IsDataCachingEnabled();

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
	m_bInvalid		= FALSE;
	m_bKeyChanged	= FALSE;
	m_bIsEmpty		= TRUE;
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

	if (m_pMandatoryColTable)
	{
		if (m_pMandatoryColTable->IsOpen())
			m_pMandatoryColTable->Close();

		delete m_pMandatoryColTable;
	}
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
	CATCH(SqlException, e)
	{
		TRACE(e->m_strError);
		THROW_LAST();
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
		// il cursore di update lo devo creare sulla sessione di lavoro che mi é stata passata
		// vedi DBT legati alla sessione di Update di un documento
		// il cursore di update viene sempre aperto come FAST_FORWARD_ONLY
		// creo lo statement adesso altrimenti dopo la SqlRowSet::Open é possibile che una
		// session differente nel caso di utilizzo di cursori FORWARD_ONLY (x cui é necessario creare
		// una session x ogni cursore aperto
		if (bUpdatable)
			m_pUpdateRowSet = new SqlRowSet(m_pSqlSession);

		SqlRowSet::Open(bUpdatable, eCursorType, (!m_pRecord || !m_pRecord->IsAView()));
		CheckRecord();		
	}

	CATCH(SqlException, e)
	{
		m_bErrorFound = TRUE;
		m_strErrorFound = e->m_strError;
		TRACE(e->m_strError);
	}
	END_CATCH
}


//-----------------------------------------------------------------------------
void SqlTable::Open
				(
					BOOL bUpdatable /*=FALSE*/, 
					BOOL bScrollable /*=TRUE*/, 
					BOOL bSensitivity /*=TRUE*/
				)
{             
	// se é una view non devo permettere la visibilitá delle modifiche
	TRY
	{
		// il cursore di update lo devo creare sulla sessione di lavoro che mi é stata passata
		// vedi DBT legati alla sessione di Update di un documento
		// il cursore di update viene sempre aperto come FAST_FORWARD_ONLY
		// creo lo statement adesso altrimenti dopo la SqlRowSet::Open é possibile che una
		// session differente nel caso di utilizzo di cursori FORWARD_ONLY (x cui é necessario creare
		// una session x ogni cursore aperto
		if (bUpdatable)
			m_pUpdateRowSet = new SqlRowSet(m_pSqlSession);
		SqlRowSet::Open(bUpdatable, bScrollable, bSensitivity && (!m_pRecord || !m_pRecord->IsAView()));
		CheckRecord();		
	}

	CATCH(SqlException, e)
	{
		TRACE(e->m_strError);
		m_bErrorFound = TRUE;
		m_strErrorFound = e->m_strError;
	}
	END_CATCH
}

//dal row set verranno eliminate le righe cancellate
//-----------------------------------------------------------------------------
void SqlTable::EnableRemoveDeletedRow()
{ 
	m_bRemoveDeletedRow = m_pSqlConnection->RemoveDeletedRows() && (!m_pRecord || !m_pRecord->IsAView()); 
} 

//-----------------------------------------------------------------------------
void SqlTable::SetProperties()
{
	if (m_bInit)
		return; 

	SqlRowSet::SetProperties();
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

	if (m_pMandatoryColTable && m_pMandatoryColTable->IsOpen())
		m_pMandatoryColTable->Close();

	SqlRowSet::Close();
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

	if (m_pMandatoryColTable)
		m_pMandatoryColTable->ClearQuery();
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

// se è qualificata devo utilizzare la qualifica anche nella group by
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
// puè essere anche qualificata x.Articolo
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
	// il programmatore mi puó quali colonne utilizzare per la KeyedUpdate e KeyedDelete
	// altrimenti vengono considerati i segmenti di chiave primaria
	if (!m_pKeysArray)
		m_pKeysArray = new CStringArray;
	m_pKeysArray->Add(strColumnName);
}

//-----------------------------------------------------------------------------
long SqlTable::GetExtractedRows()
{
	//se ho un valore corretto restituito dal rows allora utilizzo quello
	if (m_lRowCount > 0)
		return m_lRowCount;

	//vuol dire che il programmatore ha utilizzato la m_strSql...in questo caso dubito di poter riuscire a fare qualcosa
	if (m_strFrom.IsEmpty())
		return 0;

	DataLng aRowCount = 0;		
	SqlTable aCountTbl(this->m_pSqlSession);
		//dalla query di sqltable devo costruirmi una query con il count eliminando le colonne della select e l'eventuale order by della query principale
	TRY
	{
		aCountTbl.Open(FALSE, E_NO_CURSOR);
		aCountTbl.SelectSqlFun(_T("COUNT(*)"), aRowCount);
		//due casi:
		//Caso 1: query costruita con i costrutti standard di SqlTable per cui avente i vari pezzi separati
		CString strCountText;
		aCountTbl.m_strFrom += m_strFrom;
		if (!m_strFilter.IsEmpty())
		{
			aCountTbl.m_strFilter = m_strFilter;
			if (m_pParamArray && m_pParamArray->GetSize() > 0)
			{
				for (int i = 0; i < m_pParamArray->GetSize(); i++)
				{
					SqlBindingElem* pParam = m_pParamArray->GetAt(i);
					aCountTbl.AddParam(pParam->GetBindName(), *pParam->GetDataObj());
					aCountTbl.SetParamValue(pParam->GetBindName(), *pParam->GetDataObj());
				}
			}
			aCountTbl.m_strGroupBy = m_strGroupBy;
			aCountTbl.m_strHaving = m_strHaving;
		}
		aCountTbl.Query();
		aCountTbl.Close();
	}

	CATCH(SqlException, e)
	{
		if (aCountTbl.IsOpen())
			aCountTbl.Close();
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		THROW_LAST();
	}
	END_CATCH

	return aRowCount;
}


// TypeMove può essere
// E_MOVE_FIRST
// E_MOVE_LAST
// E_MOVE_NEXT
// E_MOVE_PREV
// lRows il numero di righe di skip nel caso di MoveNext e MovePrev
//-----------------------------------------------------------------------------
void SqlTable::Move(MoveType eTypeMove, DBROWOFFSET lSkip /*= 0*/)
{
	TRY
	{
		ASSERT(IsOpen());

		if (m_bErrorFound)
			ThrowSqlException(cwsprintf(_TB("SqlTable::Move:errors occurred in the command construction phase {0-%s}.\r\nUnable to carry out the move requested."), m_strSQL));
		    
	    // Esce da un evetuale stato di Edit o AddNew
		m_nEditMode = noMode;
		
		if (eTypeMove == E_MOVE_REFRESH || IsEmpty())
			return;
		
		// Data Caching
		// if the first query has not been executed (the record has been extracted from cache area)
		// I have to execute the query
		if (CanUseDataCaching() && !m_bCurrentReadFromDatabase)
		{
			Query();
			m_bCurrentReadFromDatabase = TRUE;
			return;
		}

		if (CanUseDataCaching())
			SetUseDataCaching(FALSE);

		BOOL	 bForward	= TRUE;
		MoveType eNextMove	= eTypeMove;
		m_hResult = S_OK;

		// Skip deleted rows
		while (Check(m_hResult) && m_hResult != DB_S_ENDOFROWSET && lSkip >= 0)
		{
			switch (eNextMove)
			{
				case E_MOVE_NEXT :	 
					if (m_bEOF)
						{
							TRACE0("Error: attempted to move past EOF.\n");
							ThrowSqlException(_TB("SqlTable::Move: attempt to position after end of table."));
						}
						TRACE_SQL(_T("MoveNext"), this);
						START_DB_TIME(DB_MOVE_NEXT)
						m_hResult = m_pRowSet->MoveNext(lSkip);
						STOP_DB_TIME(DB_MOVE_NEXT)						
						if (lSkip > 0)
							lSkip = 0;								
					break;

				case E_MOVE_FIRST : 
						TRACE_SQL(_T("MoveFirst"), this);
						if (!m_bScrollable)
						{
							Query();
							return;
						}
						START_DB_TIME(DB_MOVE_FIRST)
						m_hResult = m_pRowSet->MoveFirst(); 
						STOP_DB_TIME(DB_MOVE_FIRST)						
					break;

				case E_MOVE_LAST :			
						if (!m_bScrollable)
						{
							TRACE0("Error: MoveLast using a forward cursor.\n");
							ThrowSqlException(_TB("SqlTable::Move: if using a forward cursor.\r\nUnable to perform shift on last record in the rowset."));
						}
						TRACE_SQL(_T("MoveLast"), this);
						START_DB_TIME(DB_MOVE_LAST)
						m_hResult = m_pRowSet->MoveLast();						  
						STOP_DB_TIME(DB_MOVE_LAST)			
						bForward = FALSE;						
					break;
					
				case E_MOVE_PREV :	
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
						if (lSkip > 0)
						{
							TRACE_SQL(_T("MovePrev"), this);
							START_DB_TIME(DB_MOVE_PREV)
							m_hResult = m_pRowSet->MoveNext(-lSkip);  
							STOP_DB_TIME(DB_MOVE_PREV)			
							lSkip = 0;
						}
						else
						{
							// problema ORACLE, dopo aver fatto il MoveFirst, se effettuo il MovePrev viene effettuato
							// un movimento del cursore in avanti, posizionandosi sul secondo record
							if (m_pSqlConnection->GetDBMSType() == DBMS_ORACLE && m_lCurrentRecord == 0)
								return; //rimango sullo stesso
							TRACE_SQL(_T("MovePrev"), this);
							START_DB_TIME(DB_MOVE_PREV)
							m_hResult = m_pRowSet->MovePrev(); 
							STOP_DB_TIME(DB_MOVE_PREV)				
						}
						bForward = FALSE;
										
					break;

			}

			if (!Check(m_hResult))
				ThrowSqlException(m_pRowSet->m_spRowset, IID_IRowset, m_hResult, this);

			BOOL bFixupCol = TRUE;
			if (m_hResult == S_OK)
				bFixupCol = FixupColumns();	
			else
				InitBuffer();

			if (!bFixupCol && (eTypeMove == E_MOVE_FIRST || eTypeMove == E_MOVE_LAST)) //Bug fix 13772
			{
				Query();
				eNextMove = eTypeMove;
				continue;
			}
			
			// If doing MoveFirst/Last and first/last record is deleted, 
			// must do MoveNext/Prev
			if (eTypeMove == E_MOVE_FIRST || eTypeMove == E_MOVE_LAST)
				eNextMove = (bForward) ? E_MOVE_NEXT : E_MOVE_PREV;
	        
	        // gestione delle righe deletate
			m_bDeleted = (m_hResult == DB_E_DELETEDROW);  //Bug fix 13772 
			
			if (!m_bDeleted)
			{
				lSkip--;
				if (m_hResult != DB_S_ENDOFROWSET)
				{
					if (eTypeMove == E_MOVE_FIRST)
						m_lCurrentRecord = 0;
					else if (eTypeMove == E_MOVE_LAST)
					{
						if (m_bEOFSeen)
							m_lCurrentRecord = m_lRecordCount-1;
						else
							m_lRecordCount = m_lCurrentRecord = SQL_CURRENT_RECORD_UNDEFINED;
					}
					else if (m_lCurrentRecord != SQL_CURRENT_RECORD_UNDEFINED)
					{
						if (bForward)
							m_lCurrentRecord++;
						else
							// If past end, current record already decremented
							if (!m_bEOF)
								m_lCurrentRecord--;
					}
	
					// Must not be at EOF/BOF anymore
					m_bEOF = m_bBOF = FALSE;
				}
			}
		}

		// mette non dirty per ottimizzare gli update e modified per il refresh dei controls
		if (m_pRecord) m_pRecord->SetFlags(FALSE, TRUE);
		
		if (m_hResult != DB_S_ENDOFROWSET)
		{
			// aggiusta il contatore di righe viste (contatore annacquato)
			ASSERT(m_hResult != DB_E_BADROWHANDLE);
			if (m_lCurrentRecord + 1 > m_lRecordCount)
				m_lRecordCount = m_lCurrentRecord + 1;
	
			m_bBOF = FALSE;
			m_bEOF = FALSE;
			
			return;
		}
	
		// Only deleted records are left in set
		if (m_bDeleted)
		{
			m_bEOF = m_bBOF = m_bEOFSeen = TRUE;
			return;
		}
	
		// DB_S_ENDOFROWSET
		if (bForward)
		{
			// hit end of set
			m_bEOF = TRUE;
	
			// If current record is known
			if (m_lCurrentRecord != SQL_CURRENT_RECORD_UNDEFINED)
			{
				m_bEOFSeen = TRUE;
				m_lRecordCount = m_lCurrentRecord+1;
			}
		}
		else
		{
			m_bBOF = TRUE;
			m_lCurrentRecord = SQL_CURRENT_RECORD_BOF;
		}
	}

	CATCH(SqlException, e)	
	{
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		m_pRowSet->Close();
		THROW_LAST();
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
 	CATCH (SqlException, e)
 	{
		AfxMessageBox(e->m_strError);
		THROW_LAST();
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
		THROW_LAST();
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
void SqlTable::MoveCopy(MoveType eTypeMove, SqlRecord* pRecord, DBROWOFFSET lSkip /*= 0*/)
{
 	ASSERT(IsOpen()); 
 	ASSERT(pRecord);
 	ASSERT(pRecord->GetRuntimeClass() == m_pRecord->GetRuntimeClass());

	TRY
	{
 		SqlTable::Move(eTypeMove, lSkip);
 	}
 	CATCH (SqlException, e)
 	{
		AfxMessageBox(e->m_strError);
		THROW_LAST();
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
			m_pColumnArray->Add(m_pRecord, nIdx);
		}
	}	
	CATCH(SqlException, e)
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
			m_pColumnArray->Add(pRecord, nIdx);
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
			//controllo se il campo è tra gli esclusi
			for (int nExc = 0; nExc <= pExceptedFieldName->GetUpperBound(); nExc++)
			{
				if (pExceptedFieldName->GetAt(nExc).CollateNoCase(colName) == 0)
				{
					bExclude = TRUE;
					break;
				}
			}
			if (!bExclude)
				m_pColumnArray->Add(m_pRecord, nIdx);

		}
	}
		CATCH(SqlException, e)
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
			//controllo se il campo è tra gli esclusi
			for (int nExc = 0; nExc <= pExceptedDataObj->GetUpperBound(); nExc++)
			{				
				if (pExceptedDataObj->GetAt(nExc) == pDataObj)
				{
					bExclude = TRUE;
					break;
				}
			}
			if (!bExclude)
				m_pColumnArray->Add(m_pRecord, nIdx);

		}
	}
		CATCH(SqlException, e)
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

				m_pColumnArray->Add(pRec, nIdx);
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
void SqlTable::Select(SqlRecord* pRecord, DataObj* pDataObj)
{
	ASSERT_VALID(pRecord);
	CHECK_BUILD_ERROR
	(
		if (pRecord)
		{
			int idx = m_pColumnArray->Add(pRecord, pDataObj);
			if (idx >= 0 && pRecord != m_pRecord)
			{
				m_pColumnArray->GetAt(idx)->SetReadOnly();
			}
		}
	)
}

//-----------------------------------------------------------------------------
void SqlTable::Select(SqlRecord* pRecord, DataObj& aDataObj)
{
	Select(pRecord, &aDataObj);
}

//-----------------------------------------------------------------------------
void SqlTable::Select(const CString& strColumnName, DataObj* pDataObj, int nAllocSize /*= 0*/, BOOL bAutoIncrement /*=FALSE*/)
{
	ASSERT(!strColumnName.IsEmpty());

	DBTYPE eOLEType =  m_pSqlConnection->GetSqlDataType(pDataObj->GetDataType());
	
	if ((eOLEType == DBTYPE_WSTR || eOLEType == DBTYPE_STR) && nAllocSize > 0)
		pDataObj->SetAllocSize(nAllocSize);

	CHECK_BUILD_ERROR
	(
		m_pColumnArray->Add(strColumnName, pDataObj, eOLEType, nEmptySqlRecIdx, bAutoIncrement);
	)	
}

//-----------------------------------------------------------------------------
void SqlTable::Select(const CString& strColumnName, DataObj& aDataObj, int nAllocSize /*= 0*/, BOOL bAutoIncrement /*=FALSE*/)
{
	Select(strColumnName, &aDataObj, nAllocSize, bAutoIncrement);
}

// Si assume che l'funzione sia stata scritta all'esterno dal programmatore
// Utile per scrivere delle select come la seguente:
//		SELECT COUNT(*)
//		SELECT a+b
//		SELECT CASE col WHEN val1 THEN calc1 ... ELSE default_calc_val
//-----------------------------------------------------------------------------
void SqlTable::SelectSqlFun(LPCTSTR szFunction, DataObj* pResDataObj /*=NULL*/, int nAllocSize /*= 0*/, SqlRecord* pRec/*= NULL*/)
{
	DBTYPE eOLEType =  m_pSqlConnection->GetSqlDataType(pResDataObj->GetDataType());

	if (pResDataObj && (eOLEType == DBTYPE_WSTR|| eOLEType == DBTYPE_STR) && nAllocSize > 0)
		pResDataObj->SetAllocSize(nAllocSize);

	m_pColumnArray->Add(szFunction, pResDataObj, eOLEType, nEmptySqlRecIdx);

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
	
	// se nella query ho un funzione non posso settare le proprietá relative alla visibilitá
	m_bSensitivity = FALSE;			
	m_bRemoveDeletedRow = FALSE;			
}

// Costruisce un column name del tipo :
//		SUM(Caio)
// La stringa funzione passata deve contenere un %s da sostituire con il nome
// della colonna
//
//-----------------------------------------------------------------------------
void SqlTable::SelectSqlFun(LPCTSTR szFunction, DataObj* pParamDataObj, DataObj* pResDataObj, int nAllocSize /*= 0*/, SqlRecord* pRec /*= NULL*/)
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

	SelectSqlFun(strFunction, pResDataObj, nAllocSize, pRecord);
}

//-----------------------------------------------------------------------------
void SqlTable::SelectSqlFun(LPCTSTR szFunction, DataObj& aResDataObj, int nAllocSize /*= 0*/, SqlRecord* pRec /*= NULL*/)
{
	SelectSqlFun(szFunction, &aResDataObj, nAllocSize, pRec);
}

//-----------------------------------------------------------------------------
void SqlTable::SelectSqlFun(LPCTSTR szFunction, DataObj& aParamDataObj, DataObj& aResDataObj, int nAllocSize /*= 0*/, SqlRecord* pRec/*= NULL*/)
{
	SelectSqlFun(szFunction, &aParamDataObj, &aResDataObj, nAllocSize, pRec);
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
	
	// se nella query ho un funzione non posso settare le proprietá relative alla visibilitá
	m_bSensitivity = FALSE;			
	m_bRemoveDeletedRow = FALSE;	
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
	//OnInitRow();
	InitBuffer();
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

			//TODO non posso aggiungerlo perchè la BindExtraColumns NON è attrezzata
			//strPhysicalName = lex.GetAuditString();
			//strPhysicalName.Trim();
			//items.Add(strPhysicalName);
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

			//é il primo campo
			if (i == 0)
			{
				//c'é solo questo campo
				if (OrderByColumns.GetSize() == 1)
					m_strSort.Empty();
				else 	
					// il +1 è x la virgola
					m_strSort = m_strSort.Right(m_strSort.GetLength() - (strColumnName.GetLength() + 1));
			}
			else
			{
				//prendo la parte sinistra
                strTemp1 = m_strSort.Left(nPos);
				// se non é l'ultimo campo allora devo prendere anche la parte destra 
				if (i < OrderByColumns.GetUpperBound())
					strTemp2 = m_strSort.Right(m_strSort.GetLength() - (strColumnName.GetLength() + 1));
				
				m_strSort = strTemp1 + strTemp2;
			}
		}
	}

	return bOk;
}

//-----------------------------------------------------------------------------
void SqlTable::FixupMandatoryColumns()
{
	if (!m_pRecord || !ReadTraceColumns() || m_pRecord->GetType() != TABLE_TYPE || !m_pRecord->m_pTableInfo)
		return;

	if (!m_pRecord->m_pTableInfo->ExistCreatedColumn() && !m_pRecord->m_pTableInfo->ExistModifiedColumn())
		return;


	BOOL bFirst = FALSE;

	DataObjArray aPK;
	aPK.SetOwns(FALSE);

	if (!m_pMandatoryColTable)
	{
		m_pMandatoryColTable = new SqlTable(m_pRecord, m_pSqlSession);	
		m_pMandatoryColTable->SetSkipRowSecurity();
	}
	
	TRY
	{
		m_pRecord->GetKeyStream(aPK, FALSE);
		if (aPK.GetSize() <= 0)
			ThrowSqlException(cwsprintf(_TB("Unable to read value of mandatory columns of table {0-%s}. No data value available."), m_pRecord->GetTableName()));
		
		if (!m_pMandatoryColTable->IsOpen())
		{
			bFirst = TRUE;
			m_pMandatoryColTable->Open();
			m_pMandatoryColTable->Select(m_pRecord->f_TBCreated);
			m_pMandatoryColTable->Select(m_pRecord->f_TBModified);
			m_pMandatoryColTable->Select(m_pRecord->f_TBCreatedID);
			m_pMandatoryColTable->Select(m_pRecord->f_TBModifiedID);
			m_pMandatoryColTable->SetAliasName(m_pRecord->GetQualifier());
		}

		//Add the primary key values in the WHERE clause as parameters
		for (int nIdx = 0; nIdx <= aPK.GetUpperBound(); nIdx++)
		{
			DataObj* pDataObj = aPK.GetAt(nIdx);
			if (!pDataObj)
				ThrowSqlException(cwsprintf(_TB("Unable to read value of mandatory columns of table {0-%s}. No data value available."), m_pRecord->GetTableName()));
							
			if (bFirst)
			{
				m_pMandatoryColTable->AddFilterColumn(m_pRecord->GetQualifiedColumnName(pDataObj));
				m_pMandatoryColTable->AddParam(_T(""), *pDataObj);
			}
			SqlBindingElem* pElem = m_pMandatoryColTable->m_pParamArray->GetAt(nIdx);
			if (!pElem)
				ThrowSqlException(cwsprintf(_TB("Unable to read value of mandatory columns of table {0-%s}. No data value available."), m_pRecord->GetTableName()));

			pElem->SetParamValue(*pDataObj);
		}
		m_pMandatoryColTable->Query();
	}
	CATCH(SqlException, e)
	{
		if (m_pMandatoryColTable->IsOpen())
			m_pMandatoryColTable->Close();
		THROW_LAST();
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
		aTable.m_pColumnArray->Add(m_pRecord, m_pRecord->GetDataObjFromColumnName(strAutoIncCol));		
		aTable.Query();
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
		THROW_LAST();
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
		aTable.m_pColumnArray->Add(strAutoIncCol, &lMax, m_pSqlConnection->GetSqlDataType(DATA_LNG_TYPE), nEmptySqlRecIdx);
		aTable.Query();
		aTable.Close();
		m_pRecord->GetDataObjFromColumnName(strAutoIncCol)->Assign(++lMax);
	}
	CATCH(SqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
		THROW_LAST();
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
		m_pColumnArray->Add(strColumnName, pDataObj, m_pSqlConnection->GetSqlDataType(pDataObj->GetDataType()), nEmptySqlRecIdx);
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
BOOL SqlTable::FixupColumns()
{
	BOOL bResult = SqlRowSet::FixupColumns();
	if (bResult)
	{
		//se la tabella è protetta 
		if (m_bSelectGrantInformation && m_pRecord)
		{
			const SqlCatalogEntry* pConstCatalogEntry = m_pSqlConnection->GetCatalogEntry(GetRecord()->GetTableName());
			if (pConstCatalogEntry && pConstCatalogEntry->IsProtected()) //@@BAUZI TODO da ottimizzare. Portare il flag m_bIsUnderProtection in SqlRecord così si evita di interrogare il catalog
				pConstCatalogEntry->HideProtectedFields(m_pRecord);
		}
	}
	return bResult;

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
		//parte da 1 perchè il primo dovrebbe essere della stessa classe di m_pRecord
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

		//per prima cosa verifico se l'elemento è presente nel ContextBag
		//e che non sia già inserito tra i parametri
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

		//per prima cosa verifico se l'elemento è presente nel ContextBag
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
			// se nella query ho un GROUP BY non posso settare le proprietá relative alla visibilitá
			m_bSensitivity = FALSE;			
			m_bRemoveDeletedRow = FALSE;	
		}

		// Alcuni database non ammettono "order by" se la select e' "for update of"
		// ORDER BY....
		if	(!m_strSort.IsEmpty())
		{
			if (
					m_eCursorType == E_DYNAMIC_CURSOR &&
					!CheckOrderBy()
				)
			{
				ASSERT(FALSE);
				m_pContext->ShowMessage(_TB("Warning: query changed.\r\nScheduled arrangement not carried out owing to fields without indexes and use of dynamic cursors"));
			}
			if (!m_strSort.IsEmpty())
			{
				m_strSQL += szOrderBy;
				m_strSQL += m_strSort;
			}
		}


	}
		
	// salva la query per controllare che non venga cambiata senza
	// chiudere e riaprire la tabella
	m_strOldSQL = m_strSQL;
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


	BOOL bContinue = FALSE;
	m_bEOFSeen = m_bBOF = m_bEOF = TRUE;
	m_bDeleted = FALSE;

	// Data Caching Management
	if (CanUseDataCaching() && ReadCache())
		return _T("");

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

		m_pRowSet->Close();

		//la query é stata invalidata
		if (m_bInvalid)
		{
			TRACE_SQL(cwsprintf(_T("Query(i) %s %s (%s)"), (LPCTSTR)GetTableName(), (LPCTSTR)m_strFilter, (LPCTSTR)GetParamStr(this)), this);
			Prepare();
			m_bInvalid = FALSE;
		}
		else
			TRACE_SQL(cwsprintf(_T("Query(p) %s (%s)"), (LPCTSTR)GetTableName(), (LPCTSTR)GetParamStr(this)), this);

		ValorizeContextBagParameters();
		ValorizeRowSecurityParameters();
		FixupParameters();
	}
	CString sqlTemp = m_strSQL;
	SubstituteQuestionMarks(sqlTemp);
	return sqlTemp;
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
	CATCH(SqlException, e)
	{
		// verifico se l'errore non é dovuto alla mancanza di chiave primaria / indice univoco
		// sulla tabella della query
		if (
			m_hResult == DB_E_ERRORSOCCURRED &&
			m_pRecord &&
			m_pRecord->m_pTableInfo &&
			(!m_pRecord->m_pTableInfo->GetSqlUniqueColumns() ||
				m_pRecord->m_pTableInfo->GetSqlUniqueColumns()->GetSize() <= 0)
			)
		{
			m_pSqlSession->ShowMessage(
				cwsprintf
				(
					_TB("{0-%s}.\r\nMake sure that the primary key or unique index is in the table {1-%s}"),
					(LPCTSTR)e->m_strError,
					(LPCTSTR)m_strTableName
				)
			);
		}
		else if (m_hResult == DB_E_ERRORSINCOMMAND)
		{
			m_pSqlSession->ShowMessage(
				cwsprintf
				(
					_TB("Errors on select from table {0-%s}.\r\n{1-%s}"),
					(LPCTSTR)m_strTableName,
					(LPCTSTR)e->m_strError
				)
			);
		}
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		m_pRowSet->Close();

		ASSERT(FALSE);
		return FALSE;
	}
	END_CATCH
	return TRUE;
 }
           
//-----------------------------------------------------------------------------
void SqlTable::Query()
{
	if (m_pRecord && m_pRecord->IsVirtual())
		return;

	if (!m_pRowSet)
		ThrowSqlException(cwsprintf(_TB("SqlTable::Query: before carry out query\n  table {0-%s} isn't open."), m_strTableName));

	if (m_bErrorFound)
		ThrowSqlException(cwsprintf(_TB("SqltTable::Query: errors occurred in the command construction phase on table {0-%s}"), m_strTableName));
	
	TRY
	{
		BOOL bContinue = FALSE;
		m_bEOFSeen = m_bBOF = m_bEOF = TRUE;
		m_bDeleted = FALSE;

		// Data Caching Management
		if (CanUseDataCaching() && ReadCache())
			return;

		// database query
		if (m_bFirstQuery)
		{
			BuildSelect		();
			TRACE_SQL(cwsprintf(_T("Query %s %s (%s)"), (LPCTSTR)GetTableName(), (LPCTSTR)m_strFilter, (LPCTSTR)GetParamStr(this)), this);
			Prepare			();					
			VERIFY(BindColumns ());				
			BindParameters	();
			m_bFirstQuery = FALSE;
		}
		else 
		{
			// Qualcuno ha cambiato la query senza chiudere e riaprire la table?
			if (m_strOldSQL != m_strSQL)
				ThrowSqlException(_TB("SqlTable::Query(): query changed without closing or reopening the table."));	
			
			m_pRowSet->Close(); 

			//la query é stata invalidata
			if (m_bInvalid)
			{
				TRACE_SQL(cwsprintf(_T("Query(i) %s %s (%s)"), (LPCTSTR)GetTableName(), (LPCTSTR)m_strFilter, (LPCTSTR)GetParamStr(this)), this);
				Prepare();
				m_bInvalid = FALSE;
			}
			else
				TRACE_SQL(cwsprintf(_T("Query(p) %s (%s)"), (LPCTSTR)GetTableName(), (LPCTSTR)GetParamStr(this)), this);

			ValorizeContextBagParameters();		
			ValorizeRowSecurityParameters();
			FixupParameters();			
		}
	
		OpenRowSet();

		// non devo effettuare la fetch
		if (m_pColumnArray->GetCount() == 0)
			return;

		// faccio la prima fetch del dato
		// gestione eccezione
		START_DB_TIME(DB_MOVE_NEXT)
		TRACE_SQL(_T("MoveNext"), this);

		if (!m_pRowSet->m_spRowset)
		{
			TRACE_SQL(cwsprintf(_T("Query(n) %s (%s)"), (LPCTSTR)GetTableName(), (LPCTSTR)GetParamStr(this)), this);
			ThrowSqlException(_TB("SqlTable::Query(): Rowset is null"));	
		}

		if (!Check(m_pRowSet->MoveNext()))  
		{
			STOP_DB_TIME(DB_MOVE_NEXT)

			if (m_pColumnArray && !m_pColumnArray->CheckStatus(m_pContext->GetDiagnostic()))
				ShowMessage(TRUE); 

			ThrowSqlException(m_pRowSet->m_spRowset, IID_IRowset, m_hResult, this);
		}
				
		if (m_hResult != DB_S_ENDOFROWSET)
		{
			FixupColumns();
			// mette non dirty per ottimizzare gli update e modified per il refresh dei controls
			if (m_pRecord) m_pRecord->SetFlags(FALSE, TRUE);
			m_bEOFSeen = m_bBOF = m_bEOF = FALSE;
			m_lCurrentRecord = 0;
			m_lRecordCount = 1;

			// Data Caching Management
			if (CanUseDataCaching())
				WriteCache();
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
	CATCH(SqlException, e)	
	{
		// verifico se l'errore non é dovuto alla mancanza di chiave primaria / indice univoco
		// sulla tabella della query
		if (
				m_hResult == DB_E_ERRORSOCCURRED && 
				m_pRecord &&
				m_pRecord->m_pTableInfo &&
				( !m_pRecord->m_pTableInfo->GetSqlUniqueColumns() ||
				  m_pRecord->m_pTableInfo->GetSqlUniqueColumns()->GetSize() <= 0)
			)
		{
			m_pSqlSession->ShowMessage(
										cwsprintf
											(
												_TB("{0-%s}.\r\nMake sure that the primary key or unique index is in the table {1-%s}"),
												(LPCTSTR)e->m_strError,
												(LPCTSTR)m_strTableName
											)
									   );
		}
		else if (m_hResult == DB_E_ERRORSINCOMMAND) 
		{
			m_pSqlSession->ShowMessage(
										cwsprintf
											(
												_TB("Errors on select from table {0-%s}.\r\n{1-%s}"),
												(LPCTSTR)m_strTableName,
												(LPCTSTR)e->m_strError
											)
									   );
		}
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);	
		m_pRowSet->Close(); 
		THROW_LAST();
	}
	END_CATCH
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
	CATCH(SqlException, e)	
	{
		TRACE(L"%s\n", (LPCTSTR)e->m_strError);
		THROW_LAST();
	}
	END_CATCH
}            


// "DELETE <tablename> WHERE keyseg = ? AND ..."
//-----------------------------------------------------------------------------
BOOL SqlTable::KeyedDelete()
{
	ASSERT(m_pRecord);
	ASSERT(m_pRecord->m_pTableInfo);

	// l'OldRecord mi viene passato dal DBTSlaveBuffered. Negli altri casi non é + necessario
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

		if (!bPrepare)		
			m_pUpdateRowSet->m_pRowSet->Close();

		BindKeyParameters(bPrepare, 0);		

        if (bPrepare)
		{	
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strSelect;
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strFilter;
			m_pUpdateRowSet->Prepare();
			bContinue = m_pUpdateRowSet->BindParameters();
			TRACE_SQL(cwsprintf(_T("KeyedDelete %s (%s)"), (LPCTSTR)strSelect, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);	
		}
		else
		{
			bContinue = m_pUpdateRowSet->FixupParameters();
			TRACE_SQL(cwsprintf(_T("KeyedDelete(p) %s (%s)"), (LPCTSTR)strSelect, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}	

		if (bContinue)
			m_pUpdateRowSet->OpenRowSet();

		return TRUE;
	}
	CATCH(SqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		THROW_LAST();
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

	// devo riprepararmi lo statement  anche se la stringa di update é differente da quella precedente 
	if (m_pUpdateRowSet->IsOpen() && m_pUpdateRowSet->m_strSelect.CompareNoCase(strSelect) == 0)
		return FALSE;

	if (!m_pUpdateRowSet->IsOpen())
		// non ha senso utilizzare un cursore per lo statement di update/insert/delete. Il rowset è composto da un solo record
		m_pUpdateRowSet->Open(FALSE, E_NO_CURSOR);
	
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
		BOOL bContinue = FALSE;

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
					//strColumnName.CompareNoCase(CREATED_COL_NAME) == 0 || 
					//strColumnName.CompareNoCase(MODIFIED_COL_NAME) == 0
					pDataObj == &(m_pRecord->f_TBCreated) ||
					pDataObj == &(m_pRecord->f_TBModified)
				)
			{
				strInsert += pBindElem->GetBindName();
				strInsert += szComma;
				strValues += cwsprintf(_T("%s,"), GetDateFunction(m_pSqlConnection->GetDBMSType()));					
				continue;
			}				

			strInsert += pBindElem->GetBindName();
			strInsert += szComma;
			strValues += "?,";			
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
		BOOL		bFixupMandatory = FALSE;

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

						
			// is TBCreated or TBModified column and have empty values I haven't add the column in the columns list 
			// because its value is automatically set by the DBMS
			if (
					//(!strColumnName.CompareNoCase(CREATED_COL_NAME)  || 
					//!strColumnName.CompareNoCase(MODIFIED_COL_NAME))
					pDataObjPar == &(m_pRecord->f_TBCreated) ||
					pDataObjPar == &(m_pRecord->f_TBModified)
				)
			{
				bFixupMandatory = TRUE;
				continue;
			}

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
			m_pUpdateRowSet->Prepare();
			bContinue = m_pUpdateRowSet->BindParameters();
			TRACE_SQL(cwsprintf(_T("NativeInsert %s (%s)"), (LPCTSTR)strInsert, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}
		else			
		{	
			bContinue = m_pUpdateRowSet->FixupParameters();
			TRACE_SQL(cwsprintf(_T("NativeInsert(p) %s (%s)"), (LPCTSTR)strInsert, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}

		if (bContinue)
		{
			m_pUpdateRowSet->OpenRowSet();
	
			// devo fare un'interrogazione al database x farmi restituire
			// i campi di tipo autincremental				
			if (m_pRecord && m_pRecord->m_bAutoIncrement)
				FixupAutoIncColumns();

			//I have to read the TBCreated and TBModified column value
			if (bFixupMandatory)
				FixupMandatoryColumns();
		}
		
		if (bTraced) 
			m_pRecord->GetTableInfo()->GetSqlCatalogEntry()->TraceOperation
				(
					AUDIT_INSERT_OP, 
					this
				);		
		return TRUE;
	}
	CATCH(SqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		m_strError = e->m_strError;
		m_hResult = e->m_nHResult;
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
		BOOL bContinue = FALSE;

		strSQL	=  szDeleteFrom;
		strSQL	+= GetTableName();

		if (!m_strFilter.IsEmpty())
		{
			strSQL	+= szWhere;
			strSQL	+= m_strFilter;
		}

		BOOL bPrepare = OpenUpdateRowSet(strSQL);
		
		if (m_pUpdateRowSet)
		{
			m_pUpdateRowSet->m_strCurrentTables.RemoveAll();
			m_pUpdateRowSet->m_strCurrentTables.Add(GetTableName());
		}

		DataObj* pDataObj	= NULL;

		// se bPrepare = TRUE
		// devo rieffettuare la prepare dello statement. Cambia il buffer di bind
		// devo riallocarmi la struttura di binding			
		// facciamo la binding dei parametri per la WHERER clause
		for (int i = 0; i <= m_pParamArray->GetUpperBound(); i++)
		{
			pDataObj = m_pParamArray->GetDataObjAt(i);
			if (bPrepare)
			{
				m_pUpdateRowSet->AddParam(_T(""), *pDataObj);	
				m_pUpdateRowSet->m_pParamArray->GetAt(m_pUpdateRowSet->m_pParamArray->GetUpperBound())->SetParamValue(*pDataObj);
			}
			else
				m_pUpdateRowSet->m_pParamArray->GetAt(i)->SetParamValue(*pDataObj);			
		}
		
		if (bPrepare)
		{	
			m_pUpdateRowSet->m_strSQL = m_pUpdateRowSet->m_strSelect;
			m_pUpdateRowSet->Prepare();
			bContinue = m_pUpdateRowSet->BindParameters();
		}
		else
			bContinue = m_pUpdateRowSet->FixupParameters();

		if (bContinue)
			m_pUpdateRowSet->OpenRowSet();

		return TRUE;
	}
	CATCH(SqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		THROW_LAST();
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
		BOOL bContinue = FALSE;

		strSelect = szUpdate;
		strSelect += GetTableName();
		strSelect += szSet;

		DataObj* pDataObj = NULL;

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
			pDataObj = m_pParamArray->GetDataObjAt(i);
			if (bPrepare)
			{
				m_pUpdateRowSet->AddParam(_T(""), *pDataObj);	
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
			m_pUpdateRowSet->Prepare();
			bContinue = m_pUpdateRowSet->BindParameters();
		}
		else
			bContinue = m_pUpdateRowSet->FixupParameters();

		if (bContinue)
			m_pUpdateRowSet->OpenRowSet();


		return UPDATE_SUCCESS;
	}
	CATCH(SqlException, e)
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		THROW_LAST();
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
	// costruzione clausola di SET considerando le sole colonne modificate			
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

		//devo escludere i campi local
		const SqlColumnInfo* pItem = pRec->GetColumnInfo(pDataObj);
		if (pItem && pItem->m_bVirtual)
			continue;

		// is TBCreated column its value must be skipped by update operations
		//if (!pBindElem->GetBindName().CompareNoCase(CREATED_COL_NAME))
		if (pDataObj == &(pRec->f_TBCreated))
		{
			pBindElem->SetUpdatable(FALSE);
			continue;
		}
		//if (!pBindElem->GetBindName().CompareNoCase(CREATED_ID_COL_NAME))
		if (pDataObj == &(pRec->f_TBCreatedID))
		{
			pBindElem->SetUpdatable(FALSE);
			continue;
		}
		
		// is TBModified column its value is automatically set by the DBMS using the function 
		//if (!pBindElem->GetBindName().CompareNoCase(MODIFIED_COL_NAME))
		if (pDataObj == &(pRec->f_TBModified))
		{
			strSelect += pBindElem->GetBindName() + cwsprintf(_T(" = %s,"), GetDateFunction(m_pSqlConnection->GetDBMSType()));
			pBindElem->SetUpdatable(FALSE);
			continue;
		}

		//impr. 5936
		// is TBGuid column. If it's empty then I must valorize it with a new guid
		if (pDataObj == &(pRec->f_TBGuid) && pRec->f_TBGuid.IsEmpty())
		{
			pRec->f_TBGuid.AssignNewGuid();
			pBindElem->SetUpdatable();
			strSelect += pBindElem->GetBindName() + _T("=?,");
			nParam++;
			continue;
		}

		if (bCheckOldValues)
		{
			// se ho un oldrecord (vedi DBTSlaveBuffered) allora confronto i valori con quelli presenti nell'old
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

		pBindElem->SetUpdatable();
		strSelect += pBindElem->GetBindName() + _T("=?,");
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
	SqlBindingElem*  pBindElem;
	for (int i = 0; i <= m_pColumnArray->GetUpperBound(); i++)
	{
		pBindElem = m_pColumnArray->GetAt(i);
		pDataObj = pBindElem->m_pDataObj;
		
		if (!pBindElem->IsUpdatable())
			continue;		
		
		if (bPrepare)
		{
			m_pUpdateRowSet->AddParam(pBindElem->m_strBindName, *pDataObj);	
			m_pUpdateRowSet->m_pParamArray->GetAt(m_pUpdateRowSet->m_pParamArray->GetUpperBound())->SetParamValue(*pDataObj);
		}
		else
			m_pUpdateRowSet->m_pParamArray->GetAt(nParam)->SetParamValue(*pDataObj);			
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
void SqlTable::BindKeyParameters(BOOL bPrepare, int nStart)
{
	BOOL bMakeFilter = FALSE;
	CString strKeySegment;
	CString strKeyWhere; 
	
	DataObj* pOldDataObj = NULL;
	DataObj* pDataObj = NULL;

	SqlBindingElem* pBindElem = NULL;
	int nParam = nStart;

	TRACE_SQL(_T("BindKeyParameters"), this);
	// costruzione WHERE Clause considerando i campi di chiave primaria
	// la prima volta devo costruirmi la stringa di where le volte successive
	// devo solo agire sui parametri
	if (m_pUpdateRowSet->m_strFilter.IsEmpty())
	{
		m_pUpdateRowSet->m_strFilter += szWhere;
		bMakeFilter = TRUE;
	}	

	int nSize = GetKeySegmentCount();
	if (nSize == 0)
		ThrowSqlException(cwsprintf(_TB("SqlTable::BindkeyParameters:  unable to proceed with query {0-%s}/nAccess keys not available"), (LPCTSTR)m_pUpdateRowSet->m_strSelect));

	//@@OLEDB da gestire errore di mancanza primary key o special columns 
	for (int j = 0; j < nSize ; j++)
	{
		strKeySegment = (m_pKeysArray && m_pKeysArray->GetSize() > 0) 
						? m_pKeysArray->GetAt(j)
						: m_pRecord->m_pTableInfo->GetSqlUniqueColumns()->GetAt(j);

		// la WHERE clause la costruisco solo la prima volta
		if (bMakeFilter)
		{
			if (!strKeyWhere.IsEmpty()) 
				strKeyWhere += _T(" AND ");
			strKeyWhere += strKeySegment + _T(" = ?");
		}
		
		pBindElem = m_pColumnArray->GetParamByName(strKeySegment);
		
		ASSERT(pBindElem);
		// verifico il valore del campo chiave 
		// se é stato modificato devo considerare quello prima della modifica
		// altrimenti quello invariato		
		pOldDataObj =GetOldDataObj(strKeySegment);
		m_bKeyChanged = pOldDataObj && !(pOldDataObj->IsEqual(*pBindElem->m_pDataObj));
		pDataObj = (m_bKeyChanged) ?  pOldDataObj : pBindElem->m_pDataObj;

		if (bPrepare)
		{
			m_pUpdateRowSet->AddParam(_T(""), *pDataObj);			
			m_pUpdateRowSet->m_pParamArray->GetAt(m_pUpdateRowSet->m_pParamArray->GetUpperBound())->SetParamValue(*pDataObj);
		}
		else
			m_pUpdateRowSet->m_pParamArray->GetAt(nParam)->SetParamValue(*pDataObj);	
		nParam++;	
	} 
	m_pUpdateRowSet->m_strFilter +=  strKeyWhere;
}

// UPDATE <tablename> SET <colname1>=?[,<colname2>=?] WHERE KeySeg = ? AND....
//-----------------------------------------------------------------------------
int SqlTable::KeyedUpdate(BOOL bForceTBModified /*=FALSE*/)
{
	ASSERT(m_pRecord);
	ASSERT(m_pRecord->m_pTableInfo);
	
	// l'OldRecord mi viene passato dal DBTSlaveBuffered. Negli altri casi non é + necessario
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
		BOOL bContinue = FALSE;

		strSelect = szUpdate;
		strSelect += GetTableName();
		strSelect += szSet;

		// per prima cosa costruisco la stringa di query. Questa mi serve per confrontarla
		// con l'eventuale query di update fatta in precedenza. Due casi:
		// 1 - sono uguali devo solo modificare i parametri 
		// 2 - diverse. Devo fare tutti i passi relativi alla prepare e alla bind dei 
		// parametri. Passi eseguiti la prima volta 
		if (BuildSetClause(strSelect) == 0 && !bForceTBModified)
			return UPDATE_NO_DATA; 

        BOOL bPrepare = OpenUpdateRowSet(strSelect);
				
		if (m_pUpdateRowSet)
		{
			m_pUpdateRowSet->m_strCurrentTables.RemoveAll();
			m_pUpdateRowSet->m_strCurrentTables.Add(GetTableName());
		}

		int nParam = BindSetParameters(bPrepare);
		if (nParam == 0 && !bForceTBModified)
			return UPDATE_NO_DATA; 
		
		BindKeyParameters(bPrepare, nParam);
		
		if (bPrepare)
		{	
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strSelect;
			m_pUpdateRowSet->m_strSQL += m_pUpdateRowSet->m_strFilter;
			m_pUpdateRowSet->Prepare();
			bContinue = m_pUpdateRowSet->BindParameters();
			TRACE_SQL(cwsprintf(_T("KeyedUpdate %s (%s)"), (LPCTSTR)strSelect, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);	
		}
		else
		{
			bContinue = m_pUpdateRowSet->FixupParameters();
			TRACE_SQL(cwsprintf(_T("KeyedUpdate(p) %s (%s)"), (LPCTSTR)strSelect, (LPCTSTR)GetParamStr(m_pUpdateRowSet)), this);
		}

		if (bContinue)
			m_pUpdateRowSet->OpenRowSet();

		FixupMandatoryColumns();
		return UPDATE_SUCCESS;
	}
	CATCH(SqlException, e)
	{
		if (m_pUpdateRowSet->IsOpen())
			m_pUpdateRowSet->Close();
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		THROW_LAST();
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
			// se nel SqlRecord é bindato il campo TBGuid allora assegno allo stesso 
			// un nuovo GUID
			if (m_pRecord->HasGUID())
				m_pRecord->f_TBGuid.AssignNewGuid(); 

			// l'exception viene lanciata esterna alla funzione, xché ci sono
			// dei casi in cui viene chiamata solo la NativeInsert e non deve 
			// essere dato il messaggio
			if (!NativeInsert(FALSE))
				ThrowSqlException(cwsprintf(_TB("Attempt to add new record failed: {0-%s}"), m_strError), m_hResult, this);									
			result = UPDATE_SUCCESS;
		
		// gestione del numero di records presenti
			if (m_lCurrentRecord >= 0)
			{
				if (m_lRecordCount != -1)
					m_lRecordCount++;
				m_lCurrentRecord++;
			}
		}
		// se il database è Oracle devo togliere l'eventuale spazio inserito al posto
		// della stringa vuota
		if (m_pSqlConnection->m_pProviderInfo->m_eDbmsType == DBMS_ORACLE)
			StripRecordSpaces();

		//@@AUDITING 
		if (result == UPDATE_SUCCESS)
		{
			m_pRecord->GetTableInfo()->GetSqlCatalogEntry()->TraceOperation
				(
					m_nEditMode == edit ? AUDIT_UPDATE_OP : AUDIT_INSERT_OP, 
					this
				);

			// Data Caching Managemnt
			if (m_bCachingSettingStatus)
				AfxGetDataCachingUpdatesListener()->UpdateRecord (m_pRecord);
		}
	}

	CATCH(SqlException, e)	
	{
		m_nEditMode = noMode;
		m_pOldRecord = NULL;
		m_bKeyChanged = FALSE;
		THROW_LAST();
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

		// Data Caching Managemnt
		if (m_bCachingSettingStatus)
			AfxGetDataCachingUpdatesListener()->DeleteRecord (m_pRecord);

		// indicate on a deleted record
		m_bDeleted = TRUE;
		m_pOldRecord = NULL;
		m_bKeyChanged = FALSE;
		InitRow();
	}
	CATCH(SqlException, e)	
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		m_pOldRecord = NULL;
		m_bKeyChanged = FALSE;
		THROW_LAST();
	}
	END_CATCH
}

//----------------------------------------------------------------------------------
BOOL SqlTable::MoreDataAvailable() const
{ 
	return m_hResult != DB_S_ENDOFROWSET; 
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
			
			strFilter += m_pSqlConnection->m_pProviderInfo->NativeConvert(pDataObj);
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
	CString strTemp("? = ");
	CString strProc;
	BOOL bFirst = TRUE;

	TRACE_SQL(_T("BuildCall"), this);

	SqlRecordProcedure* pRecProc = (SqlRecordProcedure*)m_pRecord;
	
	SqlProcedureParamInfo*	pParamInfo = NULL;
	SqlProcParamItem* pItem;
	
	const SqlProcedureParameters* pParameters =  pRecProc->GetTableInfo()->m_pProcParameters;

	strProc.Format(_T("Call %s "), m_pRecord->GetTableName());

	// costruisco la stringa di call e intanto effettuo il bind dei parametri
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
		pItem = pRecProc->GetParamItemFromParamInfo(pParamInfo);
		if (!pItem)
		{
			ASSERT(FALSE);
			m_strSQL.Empty();
			return;
		}

		if (pParamInfo->m_nType == DBPARAMTYPE_RETURNVALUE)
		{
			strTemp += strProc;
			strProc = strTemp;
		}
		else
		{
			if (bFirst)
			{
				strProc += "(?";
				bFirst = FALSE;
			}
			else
				strProc += ", ?";
		}
		// i parametri devono essere inseriti nell'Accessor nell'esatto ordine previsto
		// dalla stored procedure
		AddProcParam(pParamInfo, pItem->GetDataObj());
	}
	if (!bFirst)
		strProc += ")";
	
	m_strSQL.Format(_T("{ %s }"), strProc);
}

// gestione della stored procedure associata al SqlTable
//-----------------------------------------------------------------------------
void SqlTable::Call()
{
	if (
			!m_pRecord || 
			!m_pRecord->IsKindOf(RUNTIME_CLASS(SqlRecordProcedure)) ||
			!m_pRecord->IsValid())

	{
		TRACE(L"Invalid SqlRecord in SqlTable::Call function");
		ASSERT(FALSE);
		LPCTSTR lpszName = (m_pRecord && m_pRecord->GetNamespace())
			? m_pRecord->GetNamespace()->GetObjectName()
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

	BuildCall();

	if (m_strSQL.IsEmpty())
	{
		ASSERT(FALSE);
		return;
	}

	DirectCall();
}

//-----------------------------------------------------------------------------
void SqlTable::DirectCall()
{
	TRY
	{
		BindParameters();

		HRESULT hr = m_pRowSet->Open(*((CSession*)m_pSqlSession->GetSession()), m_strSQL);
		if (!Check(hr))
		{
			if (m_pRowSet->m_spRowset)
				ThrowSqlException(m_pRowSet->m_spRowset , IID_IRowset, m_hResult, this);
			else
				ThrowSqlException(m_pRowSet->m_spCommand , IID_ICommand, m_hResult, this);
		}
		m_pParamArray->FixupOutParams();
	}
	CATCH(SqlException, e)	
	{
		TRACE(_T("%s\n"), (LPCTSTR)e->m_strError);
		m_pRowSet->Close(); 
		THROW_LAST();
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
			CATCH(SqlException, e)
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
// Data Caching
//-----------------------------------------------------------------------------
BOOL SqlTable::CanUseDataCaching ()
{
	return	m_bCachingSettingStatus &&
			m_bCanUseDataCaching &&
			m_pDataCachingContext;
}

//-----------------------------------------------------------------------------
void SqlTable::SetUseDataCaching (BOOL bEnable)
{
	m_bCanUseDataCaching = bEnable;
}

//-----------------------------------------------------------------------------
void SqlTable::SetDataCachingContext(CDataCachingContext* pDataCachingContext)
{
	m_pDataCachingContext = pDataCachingContext;
}


//-----------------------------------------------------------------------------
BOOL SqlTable::IsEmpty ()
{ 
	// Data Caching
	if (!CanUseDataCaching() || m_bCurrentReadFromDatabase) 
		return IsEOF() && IsBOF(); 

	return m_bIsEmpty;
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
BOOL SqlTable::ReadCache ()
{
	CDataCachingManager* pDataCachingManager = m_pDataCachingContext ? m_pDataCachingContext->GetDataCachingManager() : NULL;

	// if it is the first time i read query and parameters list
	if (m_bFirstQuery)
		BuildSelect	();

	BuildParamsList();			
	
	if (!m_bForceQuery && pDataCachingManager)
	{
		if (!pDataCachingManager->FindRecord(m_strTableName, m_strSQL, m_strParamsList, m_pRecord))
			return FALSE;

		m_pRecord->SetFlags(FALSE, TRUE);
		
		m_bIsEmpty = FALSE;
		m_bCurrentReadFromDatabase = FALSE;
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
void SqlTable::WriteCache ()
{
	CDataCachingManager* pDataCachingManager = m_pDataCachingContext ? m_pDataCachingContext->GetDataCachingManager() : NULL;
	
	m_bCurrentReadFromDatabase = TRUE;

	if (!IsEmpty() && pDataCachingManager) // it is the first record
		pDataCachingManager->InsertRecord(m_strSQL, m_strParamsList, m_pRecord);
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
			//può essere NULL, ad esempio tramite il metodo:
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
					CString name = pElem->GetBindName();

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
					CString rep = GetConnection()->NativeConvert(pElem->GetDataObj());
					
					CString name = pElem->GetBindName();
					if (!name.IsEmpty())
					{
						int idx = name.ReverseFind('_');	//remove numeric postfix (Woorm table rule and named query)
						name = name.Left(idx);
					}

					if (!name.IsEmpty() && !pElem->GetLocalName().IsEmpty())
						name += L" - ";
					name += pElem->GetLocalName();

					rep = bAddTagIn ?
								L"{IN " + name + L" } "
							:
								name.IsEmpty() ?
												 rep 
											   :  
												 L"/*" + name + L"*/ " + rep;
					
					query.Insert(nInx, rep);
					start = nInx + rep.GetLength();
				}
				else
				{
					ASSERT_TRACE(FALSE, "Binded parameter placeholder was not found\n");
				}
			}
		}
	}
	return query;
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
		m_pColumnArray = new SqlColumnArray(this);
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

		Select (pSbe->GetBindName(TRUE), pObj);
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
								pSbe->m_nDBType,
								pSbe->m_pDataObj,
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
							pSbe->m_nDBType,
							pSbe->m_pDataObj,
							pSbe->m_nSqlRecIdx
						)
				);
		}
	}
}

//=============================================================================