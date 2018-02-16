 
#include "stdafx.h"

#include <atldbcli.h>
#include <atldbsch.h>


#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\CompanyContext.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\globals.h>

#include <TbGenlib\messages.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\addonmng.h>
#include <TbGenlib\TbCommandInterface.h>
#include <TbWoormEngine\QueryObject.h>


#include <TbParser\XmlDynamicDbObjectsParser.h>
#include <TbParser\XmlAddOnDatabaseObjectsParser.h>

#include <TbDatabaseManaged\SqlSchemaInfo.h>

#include "sqlrec.h"
#include "sqlcatalog.h"
#include "oledbmng.h"
#include "sqlmark.h"
#include "sqltable.h"
#include "sqlaccessor.h"
#include "sqlconnect.h"
#include "WClause.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

static const int SYNONYM_COLUMN_LENGHT	= 30;
static const int CLOB_SIZE				= 2147483647;
static const TCHAR szFKSep[]			= _T(";");
//-----------------------------------------------------------------------------
CString GetTypeString(int nType)
{
	switch (nType)
	{
		case TABLE_TYPE:
			return _TB("Table");
		case VIEW_TYPE:
			return _TB("View");
		case PROC_TYPE:	
			return _TB("Procedure");
		case VIRTUAL_TYPE:
			return _TB("Virtual table");
	}
	return _TB("Unknown object");
}
//-----------------------------------------------------------------------------
CString AddSquareWhenNeeds(LPCTSTR sz)
{
	if (!sz || !*sz)
		return sz;
	if (*sz == '[')
		return sz;

	if (_tcschr(sz, _T(' ')) != NULL)
		return ::cwsprintf(L"[%s]", sz);
	else
		return sz;
}

BOOL AddSquareWhenNeeds(CString& str, LPCTSTR sz)
{
	if (!sz || !*sz)
		return FALSE;

	if (*sz == '[')
	{
		if ((LPCTSTR)str != sz)
			str = sz;
		return TRUE;
	}

	if (_tcschr(sz, _T(' ')) != NULL)
	{
		str = ::cwsprintf(L"[%s]", sz);
		return TRUE;
	}

	if ((LPCTSTR)str != sz)
		str = sz;
	return FALSE;
}
//////////////////////////////////////////////////////////////////////////////
//						Collation OLE Db Queries functions
//////////////////////////////////////////////////////////////////////////////
static const TCHAR	szCollationForInvariant[]	= _T("Latin1_General_CI_AS");
// okkio che essendo il catalog un oggetto al di fuori del company database, i nomi degli
// oggetti devono essere ben capitalizzati nel sistema operativo turco.
static const TCHAR	szServerCollationQuery[]	= _T("select serverproperty('Collation')");
static const TCHAR	szDatabaseCollationQuery[]	= _T("select databasepropertyex(%s, 'Collation')");
static const TCHAR	szColumnCollationQuery[]	= _T("SELECT TABLE_NAME, COLUMN_NAME, COLLATION_NAME from [%s].INFORMATION_SCHEMA.COLUMNS where COLLATION_NAME IS NOT NULL");
static const TCHAR	szCollateMapSeparator[]		= _T(",");
static const int	nBindedLength				= 100;

//=============================================================================
struct DataBindedObject
{
	BYTE	pszBytes[nBindedLength];
	DWORD   dwStatus;

public:
	DataBindedObject()
	{
		memset(this, 0, sizeof(*this));
	}
};


//=============================================================================
class CCollationOleDbQuery
{
private:
	CCommand<CManualAccessor>	m_atlCommand;
	int							m_nColumns;
	BOOL						m_bOpened;
	CString						m_sQuery;
	BOOL						m_bUnicode;
	struct DataBindedObject*	m_pRow;
	
public:
	//-----------------------------------------------------------------------------
	CCollationOleDbQuery ()
		:
		m_nColumns		(0),
		m_bOpened		(FALSE),
		m_bUnicode		(FALSE),
		m_pRow			(NULL)
	{
	}

	//-----------------------------------------------------------------------------
	~CCollationOleDbQuery ()
	{
		if (m_bOpened)
			m_atlCommand.Close ();

		if (m_pRow)
		{
			delete m_pRow;
			m_pRow = NULL;
		}
	}

	//-----------------------------------------------------------------------------
	CString GetColumn (int nIndex = 0)
	{
		// checks
		if (!m_bOpened)
		{
			ASSERT (FALSE);
			TRACE ("The query command has not been called, GetColumn request cannot be performed!");
			return _T("");
		}

        if (nIndex > m_nColumns || nIndex < 0)
		{
			ASSERT (FALSE);
			TRACE ("GetColumn request with a nIndex parameter out of range!");
			return _T("");
		}

		// columns management
		DataBindedObject& pRow = m_pRow[nIndex];

		if (pRow.dwStatus == DBSTATUS_S_ISNULL)
			return _T("");
		
		if (m_bUnicode)
			return (wchar_t*) pRow.pszBytes;

		// not Unicode characters must be converted into Unicode
		char* pANSI = (char*) pRow.pszBytes;
		if (pANSI == NULL)
			return _T("");
		
		int nLen = strlen(pANSI);
		CString sUnicode (_T(""), nLen);
		int n = MultiByteToWideChar (CP_UTF8, 0, pANSI,	nLen ,(LPTSTR) sUnicode.GetBuffer(0), nLen);
		sUnicode.ReleaseBuffer();
		
		return sUnicode;
	}

	//-----------------------------------------------------------------------------
	void PrepareQuery (ATL::CSession* pSession, const BOOL bUseUnicode, const CString& sQuery, const int& nColumns)
	{
		if (m_bOpened)
		{
			ASSERT (FALSE);
			TRACE ("The Query command has been already called, the query cannot be changed!");
			return;
		}

		// settings parameters
		m_nColumns		= nColumns;
        m_sQuery		= sQuery;
		m_bUnicode		= bUseUnicode;
		m_pRow			= new DataBindedObject[nColumns];

		m_atlCommand.Create(*pSession, m_sQuery);
		m_atlCommand.Prepare(2);

		m_atlCommand.CreateAccessor(nColumns, m_pRow, sizeof(DataBindedObject));
		
		// columns binding
		DBTYPE 	dDbType	= m_bUnicode ? DBTYPE_WSTR : DBTYPE_STR;
		for (int i=0; i < m_nColumns; i++)
			m_atlCommand.AddBindEntry(i + 1, dDbType, nBindedLength, m_pRow[i].pszBytes, NULL, &m_pRow[i].dwStatus);
	}

	//-----------------------------------------------------------------------------
	BOOL Query ()
	{
		TRY
		{
			m_bOpened = m_atlCommand.Open() == S_OK;
			if (!m_bOpened)
				return FALSE;
			
			// eof is checked on the next movenext
			m_atlCommand.MoveNext();
			return TRUE;
		}
		CATCH(CException, e)
		{
			m_atlCommand.Close();
			TCHAR sErrorMessage[512];
			e->GetErrorMessage (sErrorMessage, 512);
			ASSERT(FALSE);
			TRACE2 ("Cannot read database collation name with query %s. The problem is caused by the following error %s", m_sQuery, sErrorMessage);
			return FALSE;
		}
		END_CATCH

		return TRUE;
	}

	//-----------------------------------------------------------------------------
	BOOL MoveNext ()
	{
		if (!m_bOpened)
		{
			ASSERT (FALSE);
			TRACE ("MoveNext call without a previous Query method call!");
			return FALSE;
		}

		TRY
		{
			return (m_atlCommand.MoveNext() == S_OK);
		}
		CATCH(CException, e)
		{
			m_atlCommand.Close();
			TCHAR sErrorMessage[512];
			e->GetErrorMessage (sErrorMessage, 512);
			ASSERT(FALSE);
			TRACE2 ("MoveNext error in the query %s. The problem is caused by the following error %s", m_sQuery, sErrorMessage);
			return FALSE;
		}
		END_CATCH
	}
};

//////////////////////////////////////////////////////////////////////////////
//							SqlTableInfoArray
//////////////////////////////////////////////////////////////////////////////
//

IMPLEMENT_DYNAMIC(SqlTableInfoArray, CArray<const SqlTableInfo*>) 

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlTableInfoArray::GetColumnInfo(const CString& strName) const 
{
	int nDot = strName.Find(DOT_CHAR);
	if (nDot == -1)
		return GetAt(0)->GetColumnInfo(strName);

	CString strTable = strName.Left(nDot);
	CString strColumn = strName.Right(strName.GetLength()-nDot-1);
	for (int i=0; i < GetSize(); i++)
	{
		if (strTable.CompareNoCase(GetAt(i)->GetTableName())==0)
			return GetAt(i)->GetColumnInfo(strColumn);
	}
	return NULL;
}

//-----------------------------------------------------------------------------
SqlTableInfoArray::SqlTableInfoArray (const SqlTableInfoArray& ar) //duplica solo l'array e non gli elementi
{
	*this = ar;
}

//-----------------------------------------------------------------------------
SqlTableInfoArray& SqlTableInfoArray::operator=(const SqlTableInfoArray& ar) //duplica solo l'array e non gli elementi
{
	if (this != &ar)
	{
		Copy(ar);
		m_arstrAliasTableName.Copy(ar.m_arstrAliasTableName);
	}
	return *this;
}

//-----------------------------------------------------------------------------
void SqlTableInfoArray::Unparse (Unparser& oFile)
{
	for (int i = 0; i < GetSize(); i++)
	{
		if (i > 0) oFile.UnparseTag(T_COMMA, FALSE);
		oFile.UnparseID(GetAt(i)->GetTableName(), FALSE);

		if (m_arstrAliasTableName.GetSize())
		{
			if (!m_arstrAliasTableName.GetAt(i).IsEmpty())
			{
				oFile.UnparseTag(T_ALIAS, FALSE);
				oFile.UnparseID(m_arstrAliasTableName.GetAt(i), FALSE);
			}
		}
	}
	oFile.UnparseCrLf();
}

//-----------------------------------------------------------------------------
CString	SqlTableInfoArray::Unparse()
{
	Unparser buff(TRUE);
	Unparse(buff);
	buff.Close();
	CString sFrom = buff.GetBufferString();

	return sFrom;
}

//-----------------------------------------------------------------------------
void SqlTableInfoArray::RemoveAll()
{
	m_arstrAliasTableName.RemoveAll();
	__super::RemoveAll();
}

//-----------------------------------------------------------------------------
int SqlTableInfoArray::Find(const SqlTableInfo* pTI)
{
	for (int i = 0; i < GetSize(); i++)
	{
		const SqlTableInfo* pT = GetAt(i);
		if (pT == pTI)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
int SqlTableInfoArray::Find(const CString& sTableName)
{
	for (int i = 0; i < GetSize(); i++)
	{
		const SqlTableInfo* pT = GetAt(i);

		if (sTableName.CompareNoCase(pT->GetTableName()) == 0)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
const SqlTableInfo* SqlTableInfoArray::GetTableInfo(const CString& sTableName)
{
	int idx = Find(sTableName);
	return idx < 0 ? NULL : GetAt(idx);
}

//////////////////////////////////////////////////////////////////////////////
//							SqlTableJoinInfoArray
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(SqlTableJoinInfoArray, SqlTableInfoArray)
//-----------------------------------------------------------------------------
SqlTableJoinInfoArray::SqlTableJoinInfoArray(const SqlTableJoinInfoArray& ar)
{
	Copy(ar);

	m_arstrAliasTableName.Copy(ar.m_arstrAliasTableName);
	m_arJoinType.Copy(ar.m_arJoinType);
	m_arJoinOn.Copy(ar.m_arJoinOn);
	m_arFieldLinks.Copy(ar.m_arFieldLinks);	//clone

	m_OwnJoinExpressions = FALSE;
}

SqlTableJoinInfoArray::SqlTableJoinInfoArray(const CString& sTableName, SqlConnection*		pConnection, SymTable*			pSymTable)
	: 
	m_OwnJoinExpressions(TRUE) 
{ 
	Add(pConnection, pSymTable, sTableName); 
}

//-----------------------------------------------------------------------------
SqlTableJoinInfoArray::SqlTableJoinInfoArray(const SqlTableInfoArray& ar)
{
	Copy(ar);

	m_arstrAliasTableName.Copy(ar.m_arstrAliasTableName);
	
	for (int i = 0; i < GetSize(); i++)
	{
		m_arJoinType.Add(EJoinType::CROSS);
		m_arJoinOn.Add(NULL);
		m_arFieldLinks.Add(new DataFieldLinkArray());
	}
}

//-----------------------------------------------------------------------------
SqlTableJoinInfoArray::~SqlTableJoinInfoArray()
{ 
	RemoveAll();
}

//-----------------------------------------------------------------------------
void SqlTableJoinInfoArray::RemoveAll()
{
	if (m_OwnJoinExpressions)
		for (int i = 0; i < m_arJoinOn.GetSize(); i++)
		{
			WClause* pW = m_arJoinOn[i];
			SAFE_DELETE(pW);
		}

	m_arJoinType.RemoveAll();
	m_arJoinOn.RemoveAll();
	m_arFieldLinks.RemoveAll();
	__super::RemoveAll();

	m_OwnJoinExpressions = TRUE;
}

//-----------------------------------------------------------------------------
SqlTableJoinInfoArray& SqlTableJoinInfoArray::operator=(const SqlTableJoinInfoArray& ar) //duplica solo l'array e non gli elementi
{
	if (this != &ar)
	{
		this->Copy(ar);
		this->m_arstrAliasTableName.Copy(ar.m_arstrAliasTableName);
		m_arJoinType.Copy(ar.m_arJoinType);
		m_arJoinOn.Copy(ar.m_arJoinOn);
		m_arFieldLinks.Copy(ar.m_arFieldLinks);	//clone

		m_OwnJoinExpressions = FALSE;
	}
	return *this;
}

//-----------------------------------------------------------------------------
int SqlTableJoinInfoArray::Add(SqlConnection*		pConnection,	SymTable*			pSymTable, const CString& sTableName)
{
	if (sTableName.IsEmpty())
		return -1;

	const SqlTableInfo* pTI = AfxGetDefaultSqlConnection()->GetTableInfo(sTableName);
	ASSERT_VALID(pTI);
	if (!pTI)
		return -1;

	int p = __super::Add(pTI);
	m_arJoinType.Add(EJoinType::CROSS);

	m_arFieldLinks.Add(new DataFieldLinkArray());

	WClause* pJoinOn = new WClause(pConnection, pSymTable, this);
	pJoinOn->SetJoinOnClause();
	m_arJoinOn.Add(pJoinOn);

	return p;
}

int SqlTableJoinInfoArray::Add(SqlConnection*		pConnection,	SymTable*			pSymTable, const SqlTableInfo* pTI, const CString& sAlias/* = CString()*/, EJoinType eJoinType /*= EJoinType::CROSS*/, WClause* pJoinOn/* = NULL*/)
{
	int p = __super::Add(pTI, sAlias);

	m_arJoinType.Add(eJoinType);
	m_arFieldLinks.Add(new DataFieldLinkArray());

	if (pJoinOn == NULL)
	{
		pJoinOn = new WClause(pConnection, pSymTable, this);
		pJoinOn->SetJoinOnClause();
	}

	m_arJoinOn.Add(pJoinOn);

	return p;
}

//-----------------------------------------------------------------------------
void SqlTableJoinInfoArray::Unparse(Unparser& oFile)
{
	if (GetSize() != m_arJoinOn.GetSize() || m_arJoinOn.GetSize() != m_arJoinType.GetSize())
	{
		ASSERT(FALSE);
		return __super::Unparse(oFile);
	}
	BOOL bNaturalJoin = TRUE;
	for (int i = 1; i < m_arJoinOn.GetSize(); i++)
	{
		if (m_arJoinType[i] != EJoinType::CROSS && (m_arJoinOn[i] == NULL || m_arJoinOn[i]->IsEmpty()))
		{
			m_arJoinType[i] = EJoinType::CROSS;
		}

		if (m_arJoinType[i] != EJoinType::CROSS)
		{
			bNaturalJoin = FALSE;
			break;
		}		
	}
	if (bNaturalJoin)
	{
		return __super::Unparse(oFile);
	}

	for (int i = 0; i < GetSize(); i++)
	{
		const SqlTableInfo* pT = GetAt(i);

		if (i > 0)
		{
			oFile.UnparseCrLf();
			oFile.IncTab();
			if (m_arJoinType[i] == EJoinType::INNER)
			{
				oFile.UnparseTag(T_INNER, FALSE);
				oFile.UnparseTag(T_JOIN, FALSE);
			}
			else if (m_arJoinType[i] == EJoinType::CROSS)
			{
				oFile.UnparseTag(T_CROSS, FALSE);
				oFile.UnparseTag(T_JOIN, FALSE);
			}
			else if (m_arJoinType[i] == EJoinType::LEFT_OUTER)
			{
				oFile.UnparseTag(T_LEFT, FALSE);
				oFile.UnparseTag(T_OUTER, FALSE);
				oFile.UnparseTag(T_JOIN, FALSE);
			}
			else if (m_arJoinType[i] == EJoinType::RIGHT_OUTER)
			{
				oFile.UnparseTag(T_RIGHT, FALSE);
				oFile.UnparseTag(T_OUTER, FALSE);
				oFile.UnparseTag(T_JOIN, FALSE);
			}
			else if (m_arJoinType[i] == EJoinType::FULL_OUTER)
			{
				oFile.UnparseTag(T_FULL, FALSE);
				oFile.UnparseTag(T_OUTER, FALSE);
				oFile.UnparseTag(T_JOIN, FALSE);
			}
			else
			{
				ASSERT(FALSE);
				continue;
			}
		}

		oFile.UnparseID(pT->GetTableName(), FALSE);

		if (!m_arstrAliasTableName.GetAt(i).IsEmpty())
		{
			oFile.UnparseTag(T_ALIAS, FALSE);
			oFile.UnparseID(m_arstrAliasTableName.GetAt(i), FALSE);
		}

		if (i > 0)
		{
			if (m_arJoinType[i] != EJoinType::CROSS)
			{
				oFile.UnparseCrLf();

				WClause* pW = m_arJoinOn[i];
				ASSERT_VALID(pW);

				oFile.UnparseTag(T_ON, FALSE);
				oFile.UnparseExpr(pW->ToString(), FALSE);
			}
			oFile.DecTab();
		}
	}

	oFile.UnparseCrLf();
}

//-----------------------------------------------------------------------------
void SqlTableJoinInfoArray::QualifiedLinks(int idx)
{
	ASSERT(idx < GetSize());
	ASSERT(idx < this->m_arFieldLinks.GetSize());

	CString sTableName = GetAt(idx)->GetTableName();

	DataFieldLinkArray* pTableLinks = this->m_arFieldLinks.GetAt(idx);

	pTableLinks->SetQualified(sTableName);
}

void SqlTableJoinInfoArray::QualifiedLinks()
{
	for (int i = 0; i < m_arFieldLinks.GetSize(); i++)
	{
		QualifiedLinks(i);
	}
}
////////////////////////////////////////////////////////////////////////////////
////							SqlColumnInfo
////////////////////////////////////////////////////////////////////////////////
////
//

// costruttore utile per gestire le colonne vituali (cioe' presenti nel record
// ma non nel database). Il tipo usato per il database e' uno dei possibili per
// le conversioni canoniche
//-----------------------------------------------------------------------------
SqlColumnInfo::SqlColumnInfo
(
	const	CString&		strTableName,
	const	CString&		strColumnName,
	const	DataObj&		aDataObj
)
	:
	SqlColumnInfoObject(strTableName, strColumnName, aDataObj)	
{
}


// Aggiorna i data menbri sulla base del tipo di DataObj a cui e' collegato
//-----------------------------------------------------------------------------
void SqlColumnInfo::SetDataObjInfo(DataObj* pDataObj) const
{
	pDataObj->SetAllocSize(GetColumnLength());
	//pDataObj->SetCollateCultureSensitive(m_bUseCollationCulture == TRUE);
	pDataObj->SetSqlDataType(m_nSqlDataType);
}

// Aggiorna i data menbri sulla base del tipo di DataObj a cui e' collegato
//-----------------------------------------------------------------------------
void SqlColumnInfo::UpdateDataObjType(DataObj* pDataObj)
{
	ASSERT_VALID(pDataObj);

	if (m_bDataObjInfoUpdated)
		return;

	//TB_LOCK_FOR_WRITE();
	//qualcuno nel frattempo potrebbe averlo modificato
	//if (m_bDataObjInfoUpdated)
	//	return;

	m_bDataObjInfoUpdated = true;

	m_DataObjType = pDataObj->GetDataType();

	if (m_bLoadedFromDB)
	{
		if (m_DataObjType.m_wType == DATA_ENUM_TYPE)
		{
			m_DataObjType.m_wTag = ((DataEnum*)pDataObj)->GetTagValue();

#ifdef _DEBUG
			if (m_DataObjType.m_wTag == 0)
			{
				ASSERT_TRACE2
				(
					FALSE,
					"SqlColumnInfo::UpdateDataObjInfo: the column %s.%s has an invalid DataEnum type: tag is 0\n",
					(LPCTSTR)m_strTableName, (LPCTSTR)m_strColumnName
				);
			}
#endif		
		}
		return;
	}

	//questo switch serve solo per i campi locali utilizzati per le query
	switch (m_DataObjType.m_wType)
	{
	case DATA_STR_TYPE:
		m_nSqlDataType = DBTYPE_WSTR;
		//TODO m_lLength		= nLen;
		break;

	case DATA_MON_TYPE:
	case DATA_QTA_TYPE:
	case DATA_PERC_TYPE:
	case DATA_DBL_TYPE:
		m_nSqlDataType = DBTYPE_R8;
		m_lPrecision = 15;
		m_lLength = 8;
		break;

	case DATA_DATE_TYPE:
		m_nSqlDataType = DBTYPE_DBTIMESTAMP;
		m_lPrecision = 19;
		m_lLength = 4;
		break;

	case DATA_LNG_TYPE:
		m_nSqlDataType = DBTYPE_I4;
		m_lPrecision = LONG_PRECISION;
		m_lLength = 4;
		break;

	case DATA_ENUM_TYPE:
		m_nSqlDataType = DBTYPE_I4;
		m_lPrecision = LONG_PRECISION;
		m_lLength = 4;

		m_DataObjType.m_wTag = ((DataEnum*)pDataObj)->GetTagValue();

#ifdef _DEBUG
		if (m_DataObjType.m_wTag == 0)
		{
			ASSERT_TRACE2
			(
				FALSE,
				"SqlColumnInfo::UpdateDataObjInfo: the column %s.%s has an invalid DataEnum type: tag is 0\n",
				(LPCTSTR)m_strTableName, (LPCTSTR)m_strColumnName
			);
		}
#endif
		break;

	case DATA_BOOL_TYPE:
		m_nSqlDataType = DBTYPE_STR;
		m_lPrecision = 1;
		m_lLength = 1;
		break;

	case DATA_INT_TYPE:
		m_nSqlDataType = DBTYPE_I2;
		m_lPrecision = INT_PRECISION;
		m_lLength = 2;
		break;

	case DATA_GUID_TYPE:
		m_nSqlDataType = DBTYPE_GUID;
		m_lLength = 16;
		break;

	case DATA_TXT_TYPE:
		m_nSqlDataType = DBTYPE_STR;

	case DATA_BLOB_TYPE:
		m_nSqlDataType = DBTYPE_BYTES;
		break;

		//case DATA_BLOB_TYPE:	
	default:
		ASSERT_TRACE2(FALSE,
			"SqlColumnInfo::UpdateDataObjType: the column %s.%s has an invalid datatype\n",
			(LPCTSTR)m_strTableName, (LPCTSTR)m_strColumnName
		);
		break;
	}
}
// Aggiorna i data menbri sulla base del tipo di DataObj a cui e' collegato
//-----------------------------------------------------------------------------
void SqlColumnInfo::ForceUpdateDataObjType(DataObj* pDataObj)
{
	m_DataObjType = pDataObj->GetDataType();
	m_lLength = 50;

	m_bDataObjInfoUpdated = false;
	m_bLoadedFromDB = FALSE;

	UpdateDataObjType(pDataObj);
}

 /*Questa  routine serve essenzialmente a Woorm per proporre all'utente tutte
 le conversioni in DataObj possibili a partire dal tipo SQL della colonna.
 Il valore di default e' il primo elemento del vettore (che deve essere 
 allocato fuori, ma puo' non essere inizializzato).*/

//-----------------------------------------------------------------------------
BOOL SqlColumnInfo::GetDataObjTypes(CWordArray& aDataObjTypes) const
{
	// Cosi' siamo sicuri che non rimangano precedenti conversioni
	aDataObjTypes.RemoveAll();

	// tipo base SQL (vedi SQLColumns in ODBC help)
	switch (m_nSqlDataType)
	{
	case DBTYPE_I2:
		aDataObjTypes.Add(DATA_INT_TYPE);
		return TRUE;

	case DBTYPE_I4:
		aDataObjTypes.Add(DATA_LNG_TYPE);
		aDataObjTypes.Add(DATA_ENUM_TYPE);
		aDataObjTypes.Add(DATA_INT_TYPE);
		return TRUE;

	case DBTYPE_R4:
	case DBTYPE_R8:
		aDataObjTypes.Add(DATA_DBL_TYPE);
		aDataObjTypes.Add(DATA_QTA_TYPE);
		aDataObjTypes.Add(DATA_MON_TYPE);
		aDataObjTypes.Add(DATA_PERC_TYPE);
		return TRUE;

	case DBTYPE_DBDATE:
	case DBTYPE_DBTIME:
	case DBTYPE_DBTIMESTAMP:
		aDataObjTypes.Add(DATA_DATE_TYPE);
		return TRUE;

	case DBTYPE_NUMERIC:
		if (m_lPrecision == 1 && m_nScale == 0)
			aDataObjTypes.Add(DATA_BOOL_TYPE);
		//continue on next cases
	case DBTYPE_DECIMAL:
	case DBTYPE_VARNUMERIC:
		if (m_nScale) // e' sicuramente un valore in floating point
		{
			aDataObjTypes.Add(DATA_DBL_TYPE);
			aDataObjTypes.Add(DATA_QTA_TYPE);
			aDataObjTypes.Add(DATA_MON_TYPE);
			aDataObjTypes.Add(DATA_PERC_TYPE);
			return TRUE;
		}

		// scala = 0 e superiore al max integer
		if (m_lPrecision > INT_PRECISION)
		{
			aDataObjTypes.Add(DATA_LNG_TYPE);
			aDataObjTypes.Add(DATA_ENUM_TYPE);
			aDataObjTypes.Add(DATA_DBL_TYPE);
			aDataObjTypes.Add(DATA_QTA_TYPE);
			aDataObjTypes.Add(DATA_MON_TYPE);
			aDataObjTypes.Add(DATA_PERC_TYPE);
			return TRUE;
		}

		aDataObjTypes.Add(DATA_INT_TYPE);
		aDataObjTypes.Add(DATA_LNG_TYPE);
		aDataObjTypes.Add(DATA_ENUM_TYPE);

		aDataObjTypes.Add(DATA_DBL_TYPE);
		aDataObjTypes.Add(DATA_QTA_TYPE);
		aDataObjTypes.Add(DATA_MON_TYPE);
		aDataObjTypes.Add(DATA_PERC_TYPE);
		return TRUE;

	case DBTYPE_WSTR:
	case DBTYPE_STR:
		if (m_lLength == 1)
			aDataObjTypes.Add(DATA_BOOL_TYPE);
		aDataObjTypes.Add(DATA_STR_TYPE);
		aDataObjTypes.Add(DATA_TXT_TYPE);
		aDataObjTypes.Add(DATA_GUID_TYPE); //per ORACLE
		return TRUE;

	case DBTYPE_BOOL:
		aDataObjTypes.Add(DATA_BOOL_TYPE);
		return TRUE;

	case DBTYPE_GUID:
		aDataObjTypes.Add(DATA_GUID_TYPE);
		aDataObjTypes.Add(DATA_STR_TYPE); //per ORACLE
		return TRUE;

		// conversione parzialmente supportata da MicroArea
	case DBTYPE_BYTES:
		aDataObjTypes.Add(DATA_BLOB_TYPE);	//@@TODO
		return FALSE;
	}

	return FALSE;
}

//-----------------------------------------------------------------------------------------
DataType SqlColumnInfo::GetDataObjType() const
{
	if (m_DataObjType == DataType::Null)
	{
		CWordArray wa;
		if (GetDataObjTypes(wa))
			return wa[0];
	}

	return m_DataObjType;
}

//-----------------------------------------------------------------------------------------
#ifdef _DEBUG
void SqlColumnInfo::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " SqlColumnInfo\n");
	dc << "\tTable Name = " << m_strTableName << "\n";
	dc << "\tColumn Name = " << m_strColumnName << "\n";

	__super::Dump(dc);
}
#endif




/////////////////////////////////////////////////////////////////////////////
// 								SqlTables
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlTables, Array)

//-----------------------------------------------------------------------------
SqlTables::SqlTables(SqlConnection* pSqlConnect)
:
	m_pSqlConnection(pSqlConnect)
{
	SetOwns(FALSE);
}

//-----------------------------------------------------------------------------
BOOL SqlTables::GetInfo(BOOL bTable)
{
	TRY
	{
		/*if (bTable)
			m_pSqlConnection->LoadTables(this);
		else
			m_pSqlConnection->LoadViews(this);*/
	}
	CATCH(SqlException, e)
	{
		m_pSqlConnection->m_pContext->AddMessage(e->m_strError);
		return FALSE;

	}
	END_CATCH

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlTables::GetStoredProcedures()
{
	TRY
	{
		//m_pSqlConnection->LoadProcedures(this);
	}
		CATCH(SqlException, e)
	{
		m_pSqlConnection->m_pContext->AddMessage(e->m_strError);
		return FALSE;

	}
	END_CATCH

	return TRUE;
}


//////////////////////////////////////////////////////////////////////////////
//								SqlForeignKeysReader
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
void SqlForeignKeysReader::LoadForeignKeys (CString sFromTableName, CString sToTableName, SqlSession* pSqlSession, BOOL bLoadAllToTables /*= FALSE*/)
{
	if (
			!pSqlSession || 
			(
				!m_sLastFromTableName.IsEmpty() && 
				!m_sLastToTableName.IsEmpty() &&
				sFromTableName.CompareNoCase(m_sLastFromTableName) == 0 &&
				(
					(!bLoadAllToTables && !sToTableName.IsEmpty() && sToTableName.CompareNoCase(m_sLastToTableName) == 0) ||
					(bLoadAllToTables  && sToTableName.IsEmpty()  && m_sLastToTableName.IsEmpty())
				)
				
			)
		)
		return;

	RemoveAll ();
	TRY
	{
		pSqlSession->GetSqlConnection()->LoadForeignKeys(sFromTableName, sToTableName, bLoadAllToTables, this);
		m_sLastFromTableName = sFromTableName;
		m_sLastToTableName = bLoadAllToTables ? _T("") : sToTableName;

	}
	CATCH(SqlException, e)
	{
		THROW_LAST();
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlForeignKeysReader::GetForeignKey (const int& nIdx, CString& sFromTable, CString& sFromCol, CString& sToTable, CString& sToCol)
{
	if (nIdx < 0 || nIdx > GetUpperBound())
	{
		ASSERT(FALSE);
		return;
	}

	CString strKeys = GetAt(nIdx);
	if (strKeys.IsEmpty())
		return;
	
	int nPos = 0;
	// from
	CString sToken = strKeys.Tokenize(szFKSep, nPos);
	if (sToken.IsEmpty())
		return;
	
	int nPosQualif = sToken.Find(DOT_CHAR);
	if (nPosQualif > 0)
		sFromTable = sToken.Left(nPosQualif);

	sFromCol = sToken.Mid(nPosQualif+1);
	
	// to
	sToken		= strKeys.Tokenize(szFKSep, nPos);
	if (sToken.IsEmpty())
		return;

	nPosQualif = sToken.Find(DOT_CHAR);
	if (nPosQualif > 0)
		sToTable = sToken.Left(nPosQualif);

	sToCol = sToken.Mid(nPosQualif+1);
}

//-----------------------------------------------------------------------------
CString SqlForeignKeysReader::GetForeignKeyOf (const CString& sTable, const CString& sCol, const CString& sOnTable, SqlSession* pSqlSession)
{
	if (sTable.CompareNoCase(m_sLastFromTableName) || sOnTable.CompareNoCase(m_sLastToTableName))
		LoadForeignKeys (sTable, sOnTable, pSqlSession);

	CString sTemp;
	CString sFromCol;
	CString sToCol;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		GetForeignKey (i, sTemp, sFromCol, sTemp, sToCol);
		if (sFromCol.CompareNoCase(sCol) == 0)
			return sToCol;
	}
	return _T("");
}

//////////////////////////////////////////////////////////////////////////////
//								CRTAddOnNewFields
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
CRTAddOnNewFields::CRTAddOnNewFields(CRuntimeClass* pRTNewFields, const CString& strSignature, CRuntimeClass* pRTSql, const CTBNamespace& nsOwnerLibrary)
:
	m_pRuntimeClass		(pRTNewFields),
	m_strSignature		(strSignature),
	m_pSqlRecordClass	(pRTSql),
	m_nsOwnerLibrary	(nsOwnerLibrary)
{
	ASSERT(m_pRuntimeClass);
	ASSERT(!m_strSignature.IsEmpty());
	ASSERT(m_pSqlRecordClass);
}

CRTAddOnNewFields::~CRTAddOnNewFields()
{
}

//////////////////////////////////////////////////////////////////////////////
//								CRTAddOnNewFieldsArray
//////////////////////////////////////////////////////////////////////////////
//

CRTAddOnNewFieldsArray::~CRTAddOnNewFieldsArray()
{
}

//-----------------------------------------------------------------------------
BOOL CRTAddOnNewFieldsArray::Exist(CRTAddOnNewFields* pRTNewFields) const
{
	for (int i= 0; i <= GetUpperBound(); i++)
		if (pRTNewFields->m_pRuntimeClass == GetAt(i)->m_pRuntimeClass)	return TRUE;
	
	return FALSE;
}

//-----------------------------------------------------------------------------
void CRTAddOnNewFieldsArray::Add(CRTAddOnNewFields* pRTNewFields)
{
	if (Exist(pRTNewFields))
	{
		ASSERT(FALSE);
		TRACE1("CRTAddOnNewFieldsArray::Add: the RuntTimeClass %s already exists\n", 
				pRTNewFields->m_pRuntimeClass->m_lpszClassName);
		return;
	}
	Array::Add(pRTNewFields);
}

//-----------------------------------------------------------------------------
LPCTSTR CRTAddOnNewFieldsArray::GetSignature(CRuntimeClass* pRTNewFields) const
{
	CString dummy;
	for (int i = 0; i <= GetUpperBound(); i++)
		if (pRTNewFields == GetAt(i)->m_pRuntimeClass)
			return GetAt(i)->m_strSignature;
	
	return _T("");
}

//-----------------------------------------------------------------------------
CTBNamespace CRTAddOnNewFieldsArray::GetNsOwnerLibrary (CRuntimeClass* pRTNewFields) const
{
	CString dummy;
	for (int i = 0; i <= GetUpperBound(); i++)
		if (pRTNewFields == GetAt(i)->m_pRuntimeClass)
			return GetAt(i)->m_nsOwnerLibrary;
	
	return _T("");
}

// ORACLE NOTE
// OraOLEDBrmc.dll is loaded and unloaded frequently and
// the programmer have no control over when the Oracle client will load and unload that dll while the 
// Oracle Provider for OLE DB is being used (notes from Metalinks)
// OraOLEDBrmc10.dll (the version for 10g) is used during the load of table, columns, parameters, keys info
//////////////////////////////////////////////////////////////////////////////
//								SqlTableInfo
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
SqlTableInfo::SqlTableInfo(SqlCatalogEntry* pSqlCatalogEntry, SqlConnection *pConnection, BOOL bReloadTableInfo /* = FALSE */)
	:
	m_pSqlUniqueColumns	(NULL),
	m_pProcParameters	(NULL), //lo utilizzo solo per le StoredProcedure
	m_pSqlNewFieldRT	(NULL),
	m_pSqlCatalogEntry	(pSqlCatalogEntry),
	m_pMasterRec		(NULL),
	m_bSortedWithRecord	(FALSE),
	m_bValid			(TRUE),
	m_bCheckedSqlNewFieldRT	(false),
	m_pCreatedColumn		(NULL),
	m_pModifiedColumn		(NULL),
	m_pCreatedIDColumn		(NULL),
	m_pModifiedIDColumn		(NULL),
	m_pGuidColumn			(NULL)
{
	m_strTableName = pSqlCatalogEntry->m_strTableName;

	switch (pSqlCatalogEntry->m_nType)
	{
		case TABLE_TYPE:
			m_pSqlUniqueColumns = new SqlUniqueColumns();
			LoadColumnsInfo(pConnection, bReloadTableInfo);
			m_bValid = m_arPhisycalColumns.GetSize() > 0;			
			break;
		
		case VIEW_TYPE:
			LoadColumnsInfo(pConnection, bReloadTableInfo);
			m_bValid = m_arPhisycalColumns.GetSize() > 0;
			break;
		
		case PROC_TYPE:
			LoadProcParametersInfo(pConnection, bReloadTableInfo);
			break; 
		
		case VIRTUAL_TYPE: break;
	}
}

//-----------------------------------------------------------------------------
SqlTableInfo::~SqlTableInfo() 
{
	delete m_pSqlUniqueColumns;
	delete m_pProcParameters;
	delete m_pSqlNewFieldRT;
	SAFE_DELETE(m_pMasterRec);

	m_pSqlNewFieldRT = NULL;
}

//-----------------------------------------------------------------------------
BOOL SqlTableInfo::ExistColumn (const CString& strColumnName) const
{
	return GetColumnInfoPos (strColumnName) >= 0;
}

//-----------------------------------------------------------------------------
const CInfoOSL* SqlTableInfo::GetOSLTableInfo()
{
	if (m_pSqlCatalogEntry)
		return m_pSqlCatalogEntry->GetInfoOSL();
	return NULL;
}

//-----------------------------------------------------------------------------
void SqlTableInfo::SetNamespace	(const CTBNamespace& aNamespace)
{
	if (m_pSqlCatalogEntry)
		m_pSqlCatalogEntry->SetNamespace(aNamespace);
}


//-----------------------------------------------------------------------------
void SqlTableInfo::LoadColumnsInfo(SqlConnection* pSqlConnection, BOOL bReloadTableInfo /*= FALSE*/)
{
	TRY
	{
		if (!m_pSqlCatalogEntry || !m_pSqlCatalogEntry->m_pTableItem)
			return;

		if (bReloadTableInfo)
			pSqlConnection->LoadColumnsInfo(m_strTableName, &m_pSqlCatalogEntry->m_pTableItem->m_arColumnsInfo);

		SqlColumnInfo* pColumnInfo = NULL;
		if (!m_pSqlUniqueColumns)
			m_pSqlUniqueColumns = new SqlUniqueColumns();
		for (int i = 0; i < m_pSqlCatalogEntry->m_pTableItem->m_arColumnsInfo.GetSize(); i++)
		{
			pColumnInfo = (SqlColumnInfo*)m_pSqlCatalogEntry->m_pTableItem->m_arColumnsInfo.GetAt(i);
			//vado ad inserire il titolo corretto al campo in base alla lingua scelta
			pColumnInfo->m_strColumnTitle = AfxLoadDatabaseString(pColumnInfo->m_strColumnName, pColumnInfo->m_strTableName);
			//vado a controllare se il campo è sensibile alla localizzazione (collate diversa dal database)
			//@@TODOBAUZI da farlo solo se il database lo prevede
			pColumnInfo->m_bUseCollationCulture = false; // SqlConnection->IsCollationCultureSensitive(pColumnInfo);

			//considero anche i campi di tipo chiave e li inserisco nell'array m_pSqlUniqueColumns
			if (pColumnInfo->m_bSpecial)
			{
				m_pSqlUniqueColumns->AddSpecialColumn(pColumnInfo->m_strColumnName);
			}
			else if (pColumnInfo->m_strColumnName.CompareNoCase(CREATED_COL_NAME) == 0)
			{
				m_pCreatedColumn = pColumnInfo;
			}
			else if (pColumnInfo->m_strColumnName.CompareNoCase(MODIFIED_COL_NAME) == 0)
			{
				m_pModifiedColumn = pColumnInfo;
			}
			else if (pColumnInfo->m_strColumnName.CompareNoCase(CREATED_ID_COL_NAME) == 0)
			{
				m_pCreatedIDColumn = pColumnInfo;
			}
			else if (pColumnInfo->m_strColumnName.CompareNoCase(MODIFIED_ID_COL_NAME) == 0)
			{
				m_pModifiedIDColumn = pColumnInfo;
			}
			else if (pColumnInfo->m_strColumnName.CompareNoCase(GUID_COL_NAME) == 0)
			{
				m_pGuidColumn = pColumnInfo;
			}

			m_arPhisycalColumns.Add(pColumnInfo);
		}

		//non ho bisogno più dell'array di appoggio
		//devo solo cancellare l'array ma non il suo contenuto
		m_pSqlCatalogEntry->m_pTableItem->m_arColumnsInfo.SetOwns(FALSE);
		m_pSqlCatalogEntry->m_pTableItem->m_arColumnsInfo.RemoveAll();
	}
		CATCH(SqlException, e)
	{
		pSqlConnection->m_pContext->AddMessage(e->m_strError);
	}
	END_CATCH

}

//-----------------------------------------------------------------------------
void SqlTableInfo::LoadProcParametersInfo(SqlConnection* pSqlConnection, BOOL bReloadTableInfo /*= FALSE*/)
{
	if (m_pProcParameters)
		return;

	m_pProcParameters = new SqlProcedureParameters();
	if (!m_pSqlCatalogEntry || !m_pSqlCatalogEntry->m_pTableItem)
		return;
	TRY
	{

		if (bReloadTableInfo)
			pSqlConnection->LoadProcedureParametersInfo(m_strTableName, &m_pSqlCatalogEntry->m_pTableItem->m_arProcedureParams);

		SqlProcedureParamInfo* pProcParamInfo = NULL;
		for (int i = 0; i < m_pSqlCatalogEntry->m_pTableItem->m_arProcedureParams.GetSize(); i++)
		{
			pProcParamInfo = (SqlProcedureParamInfo*)m_pSqlCatalogEntry->m_pTableItem->m_arProcedureParams.GetAt(i);
			m_pProcParameters->Add(pProcParamInfo);			
		}
		//devo solo cancellare l'array ma non il suo contenuto
		m_pSqlCatalogEntry->m_pTableItem->m_arProcedureParams.SetOwns(FALSE);
		m_pSqlCatalogEntry->m_pTableItem->m_arProcedureParams.RemoveAll();
	}
	CATCH(SqlException, e)
	{
		//e->ShowError();
		pSqlConnection->m_pContext->AddMessage(e->m_strError);
		if (m_pProcParameters)
		{
			delete m_pProcParameters;
			m_pProcParameters = NULL;
		}
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
void SqlTableInfo::RemoveDynamicColumnInfo (const CString& strColumnName)
{
	//1) prima di tutto, provo a vedere se c'e` senza locckare in scrittura
	int nFound = GetColumnInfoPos (strColumnName);
	if (nFound == -1) return;

	//2)se non c'e`, loccko in scrittura (cosi nessuno puo` aggiungere elementi)
	BEGIN_TB_OBJECT_LOCK(&m_arPhisycalColumns)

		//3)controllo se nel frattempo (fra 1 e 2) qualcuno ha tolto elementi
		nFound = GetColumnInfoPos (strColumnName);
		if (nFound == -1) return;
		
		m_arPhisycalColumns.RemoveAt(nFound);

	END_TB_OBJECT_LOCK()
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlTableInfo::AddDynamicColumnInfo(	
													const	CString&	strColumnName,
													const	DataObj&	aDataObj,
													int		nLen,
													BOOL	bIsCollateCultureSensitive,
													BOOL	bVirtual,
													BOOL	bSpecial
												)
{
	//1) prima di tutto, provo a vedere se c'e` senza locckare in scrittura
	const SqlColumnInfo* pInfo = GetColumnInfo(strColumnName);
	if (pInfo) 
		return pInfo;

	//2)se non c'e`, loccko in scrittura (cosi nessuno puo` aggiungere elementi)
	BEGIN_TB_OBJECT_LOCK(&m_arPhisycalColumns)

		//3)controllo se nel frattempo (fra 1 e 2) qualcuno ha aggiunto elementi
		pInfo = GetColumnInfo(strColumnName);
		if (pInfo)
			return pInfo;
		
		//4) infine aggiungo l'elemento
		SqlColumnInfo* pColumnInfo = new SqlColumnInfo
    		(
    			m_strTableName,
    			strColumnName,
    			aDataObj
    		);
		pColumnInfo->m_bUseCollationCulture = bIsCollateCultureSensitive;
		pColumnInfo->m_bVirtual = bVirtual;
		pColumnInfo->m_bSpecial = bSpecial == TRUE;
		pColumnInfo->m_lLength	= nLen;

		m_arPhisycalColumns.Add(pColumnInfo);

	return pColumnInfo;

	END_TB_OBJECT_LOCK()
}

//-----------------------------------------------------------------------------
void SqlCatalogEntry::RemoveDynamicColumnInfo(const CString& strColumnName)
{
	if (m_pTableInfo)
		m_pTableInfo->RemoveDynamicColumnInfo(strColumnName);
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlTableInfo::AddVirtualColumnInfo(int nPos, 
													const	CString&	strColumnName,
													const	DataObj&	aDataObj,
													int			nLen,
													BOOL bIsCollateCultureSensitive
													)
{
	//1) prima di tutto, provo a vedere se c'e` senza locckare in scrittura
	const SqlColumnInfo* pInfo = GetColumnInfo(strColumnName, nPos, TRUE);
	if (pInfo) 
		return pInfo;

	//2)se non c'e`, loccko in scrittura (cosi nessuno puo` aggiungere elementi)
	BEGIN_TB_OBJECT_LOCK(&m_arVirtualColumns)

		//3)controllo se nel frattempo (fra 1 e 2) qualcuno ha aggiunto elementi
		pInfo = GetColumnInfo(strColumnName, nPos, TRUE);
		if (pInfo)
			return pInfo;
		
		//4) infine aggiungo l'elemento
		SqlColumnInfo* pColumnInfo = new SqlColumnInfo
    										(
    											m_strTableName,
    											strColumnName,
    											aDataObj
    										);
		pColumnInfo->m_bUseCollationCulture = bIsCollateCultureSensitive;
		pColumnInfo->m_lLength				= nLen;
		
		m_arVirtualColumns.Add(pColumnInfo);

	return pColumnInfo;

	END_TB_OBJECT_LOCK()
}

//-----------------------------------------------------------------------------
int SqlTableInfo::GetSizePhisycalColumns () const
{
	return m_arPhisycalColumns.GetCount();
}

//-----------------------------------------------------------------------------
int SqlTableInfo::GetPreAllocSize () const
{
	if (m_pMasterRec)
	{
		return m_pMasterRec->GetSize();
	}

	return m_arPhisycalColumns.GetCount();
}

//-----------------------------------------------------------------------------
SqlColumnInfo* SqlTableInfo::GetAt (int nIndex) const
{
	int len = m_arPhisycalColumns.GetCount();
	if (nIndex < len)
		return (SqlColumnInfo*) m_arPhisycalColumns.GetAt(nIndex);
	
	TB_OBJECT_LOCK_FOR_READ(&m_arVirtualColumns);

	return (SqlColumnInfo*) m_arVirtualColumns.GetAt(nIndex - len); 
}
//-----------------------------------------------------------------------------
SqlColumnInfo*&	SqlTableInfo::ElementAt (int nIndex)
{
	int len = m_arPhisycalColumns.GetCount();
	if (nIndex < len)
		return (SqlColumnInfo*&) m_arPhisycalColumns.ElementAt(nIndex);
	
	TB_OBJECT_LOCK_FOR_READ(&m_arVirtualColumns);

	return (SqlColumnInfo*&) m_arVirtualColumns.ElementAt(nIndex - len); 
}


//-----------------------------------------------------------------------------
BOOL SqlTableInfo::IsMasterTable() const
{ 
	return (m_pSqlCatalogEntry) ? m_pSqlCatalogEntry->IsMasterTable() : FALSE; 
}

// mi serve per togliermi l'eventuale qualifica della colonna
//-----------------------------------------------------------------------------
CString SqlTableInfo::GetName(const CString& strColumnName) const
{
	return GetName(strColumnName, strColumnName.ReverseFind(DOT_CHAR));
}

//-----------------------------------------------------------------------------
CString SqlTableInfo::GetName(const CString& strColumnName, int nDotIndex) const
{
	//mi restituisce l'ultimo punto La qualifica potrebbe essere anche con il nome del database
	if (nDotIndex < 0) return strColumnName;

	//TODO: se fosse qualificata con un alias mi scapperebbe
	if (m_strTableName.CompareNoCase(strColumnName.Left(nDotIndex)))
		return _T("");

	return strColumnName.Mid(nDotIndex + 1);
}

//-----------------------------------------------------------------------------
int SqlTableInfo::GetColumnInfoPos (const CString& strColumnName, int nGuessedPos, BOOL bVirtual,  const SqlColumnInfo** ppColInfo /*= NULL*/)
{
	CString strClearName;
	int nDotIndex = strColumnName.ReverseFind(DOT_CHAR);
	const CString& strName = (nDotIndex == -1 ? strColumnName : (strClearName = GetName(strColumnName, nDotIndex)));
	if (strName.IsEmpty())
		return -1;

	//TODO prova comparativa alternativo alle righe sopra
	//CString strName2 (GetName(strColumnName));
	//if (strName2.IsEmpty())
	//	return -1;

	//-----
	int len = m_arPhisycalColumns.GetCount();

	const SqlColumnInfo* pColumnInfo/* = NULL*/;

	ASSERT (nGuessedPos > -1);

	//i local non sono indicizzati, perché potrei avere più SqlRecord Enhanced che insistono sulla stessa tabella
	if (bVirtual)
	{
		TB_OBJECT_LOCK_FOR_READ(&m_arVirtualColumns);

		int nVirtualLen = m_arVirtualColumns.GetCount();
		if (nGuessedPos >= len && nGuessedPos < (len + nVirtualLen))
		{
			pColumnInfo = (SqlColumnInfo*) m_arVirtualColumns.GetAt(nGuessedPos - len);
			if (pColumnInfo->m_strColumnName.CompareNoCase(strName) == 0)
			{
				if (ppColInfo) *ppColInfo = pColumnInfo;
				return nGuessedPos;
			}
		}

		//OTTIMIZZAZIONE
		//return  GetColumnInfoPos(strColumnName);
		//ne espando solo la parte che serve, evitando di cercare prima nelle colonne fisiche e rifare il lock
		for (int i = 0; i < nVirtualLen; i++)
		{
			pColumnInfo = (SqlColumnInfo*) m_arVirtualColumns.GetAt(i);
			if (pColumnInfo->m_strColumnName.CompareNoCase(strName) == 0)
			{
				if (ppColInfo) *ppColInfo = pColumnInfo;
				return i + len;
			}
		}
		return -1;
	}
	
	{//la graffa serve per dare uno scope al lock 
		//TB_OBJECT_LOCK_FOR_READ(&m_arPhisycalColumnMapping);

		int nRealPos;
		if (m_arPhisycalColumnMapping.Lookup(nGuessedPos, nRealPos))
		{
			pColumnInfo = (SqlColumnInfo*) m_arPhisycalColumns.GetAt(nRealPos);
			if (pColumnInfo->m_strColumnName.CompareNoCase(strName) == 0)
			{
				if (ppColInfo) *ppColInfo = pColumnInfo;
				return nRealPos;
			}
		}
	}

	int retVal = -1;
	if (nGuessedPos < len)
	{
		pColumnInfo = (SqlColumnInfo*) m_arPhisycalColumns.GetAt(nGuessedPos);
		if (pColumnInfo->m_strColumnName.CompareNoCase(strName) == 0)
		{
			if (ppColInfo) *ppColInfo = pColumnInfo;
			retVal = nGuessedPos;
		}
	} 

	if (retVal == -1)
		retVal = GetColumnInfoPos(strColumnName, ppColInfo);
	
	if (retVal == -1)
		return -1;

	//lock in scrittura
	//TB_OBJECT_LOCK(&m_arPhisycalColumnMapping);

	m_arPhisycalColumnMapping[nGuessedPos] = retVal;

	return retVal;
}

//-----------------------------------------------------------------------------
int SqlTableInfo::GetColumnInfoPos (const CString& strColumnName,  const SqlColumnInfo** ppColInfo /*= NULL*/) const
{
	CString strClearName;
	int nDotIndex = strColumnName.ReverseFind(DOT_CHAR);
	const CString& strName = (nDotIndex == -1 ? strColumnName : (strClearName = GetName(strColumnName, nDotIndex)));
	if (strName.IsEmpty())
		return -1;

	//TODO prova comparativa alternativo alle righe sopra
	//CString strName2 (GetName(strColumnName));
	//if (strName2.IsEmpty())
	//	return -1;
	//----

	int len = m_arPhisycalColumns.GetCount();

	const SqlColumnInfo* pColumnInfo/* = NULL*/;

	for (int i = 0; i < len; i++)
	{
		pColumnInfo = (SqlColumnInfo*) m_arPhisycalColumns.GetAt(i);
		if (pColumnInfo->m_strColumnName.CompareNoCase(strName) == 0)
		{
			if (ppColInfo) *ppColInfo = pColumnInfo;
			return i;
		}
	}

	TB_OBJECT_LOCK_FOR_READ(&m_arVirtualColumns);

	for (int i = 0; i < m_arVirtualColumns.GetCount(); i++)
	{
		pColumnInfo = (SqlColumnInfo*) m_arVirtualColumns.GetAt(i);
		if (pColumnInfo->m_strColumnName.CompareNoCase(strName) == 0)
		{
			if (ppColInfo) *ppColInfo = pColumnInfo;
			return i + len;
		}
	}

	return -1;
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlTableInfo::GetColumnInfo (const CString& strColumnName) const
{
	const SqlColumnInfo* pColInfo = NULL;
	//int nFound = GetColumnInfoPos (strColumnName, &pColInfo);
	//if (nFound == -1)
	//	return NULL;
	if (GetColumnInfoPos (strColumnName, &pColInfo) == -1)
		return NULL;

	ASSERT_VALID(pColInfo);
	//ASSERT(pColInfo == GetAt(nFound));
	return pColInfo; // GetAt(nFound); //costa per i lock
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlTableInfo::GetColumnInfo (const CString& strColumnName, int nPos, BOOL bVirtual)
{
	const SqlColumnInfo* pColInfo = NULL;
	//int nFound = GetColumnInfoPos (strColumnName, nPos, bVirtual, &pColInfo);
	//if (nFound == -1)
	//	return NULL;
	if (GetColumnInfoPos (strColumnName, nPos, bVirtual, &pColInfo) == -1)
		return NULL;

	ASSERT_VALID(pColInfo);
	//ASSERT(pColInfo == GetAt(nFound));
	return pColInfo; // GetAt(nFound);
}

//-----------------------------------------------------------------------------
DataType SqlTableInfo::GetColumnDataType(const CString& strColumnName) const
{
	const SqlColumnInfo* pColInfo = GetColumnInfo (strColumnName);
	return pColInfo ? pColInfo->GetDataObjType() : DATA_NULL_TYPE;
}

//-----------------------------------------------------------------------------
void SqlTableInfo::SetExistIndex(const CString& sColumnName)
{
	SqlColumnInfo* pColumnInfo = const_cast<SqlColumnInfo*>(GetColumnInfo(sColumnName));
	if (pColumnInfo)
		pColumnInfo->m_bIndexed = TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlTableInfo::ExistIndex(const CString& sColumnName) const 
{
	//il controllo è già fatta internamente dalla GetColumnInfo/GetColumnInfoPos
	//CString strName = GetName(szColumnName);
	//if (strName.IsEmpty())
	//	return FALSE;

	const SqlColumnInfo* pColumnInfo = GetColumnInfo(sColumnName);
	
	return pColumnInfo && pColumnInfo->m_bIndexed;
}

//-----------------------------------------------------------------------------
BOOL SqlTableInfo::ExistIndex(const SqlColumnInfo* pColumnInfo) const
{
	if (!pColumnInfo)
		return FALSE;
	
	return pColumnInfo->m_bIndexed;
}

// carico le informazioni sugli indici...
// questo mi serve poi per effettuare il controllo sull'Order By nel caso di utilizzo
// di cursori dinamici (vedi comportamento SQLServer) 
//-----------------------------------------------------------------------------
void SqlTableInfo::LoadIndexInfo(SqlConnection *pConnection)
{
 //   ASSERT_VALID(this);
	//CIndexes* pIndexes = new CIndexes;

	//TRY
	//{ 	
	//	//@@OLEDB
	//	// per Oracle (@@TODO da verificare)
	//	//	CString strTableName = m_strTableName; 
	//	//	CString strOwner = (AfxGetDefaultWorkspace()->m_pDriver->m_nDbmsType == DBMS_ORACLE)
	//	//						? AfxGetOleDbMng()->m_strOwner
	//	//						: _T("");

	//	//	if	(AfxGetDefaultWorkspace()->m_pDriver->m_bColumnsBug)
	//	//	{
	//	//		strOwner.MakeUpper();			
	//	//		strTableName.MakeUpper();
	//	//	}	

	//	if(pIndexes->Open(*((CSession*)pConnection->GetDefaultSession()), NULL, NULL, NULL, NULL,(LPCTSTR)m_strTableName) != S_OK)
	//		return;
	//			
	//	while (pIndexes->MoveNext() == S_OK)
	//		SetExistIndex((LPCTSTR)pIndexes->m_szColumnName);
	//	
	//	delete pIndexes;
	//	pIndexes = NULL;
	//}
	//CATCH(SqlException, e)
	//{
	//	if (pIndexes) 
	//	{
	//		delete pIndexes;
	//		pIndexes = NULL;
	//	}
	//	THROW_LAST();
	//}
	//END_CATCH
}


//-----------------------------------------------------------------------------
int SqlTableInfo::GetParamInfoPos (const CString& strParamName, int nPos/* = -1*/) const
{
	if (strParamName.IsEmpty() || !m_pProcParameters)
		return -1;
	
	SqlProcedureParamInfo* pParamInfo = NULL;

	if (nPos >= 0 && nPos <= m_pProcParameters->GetUpperBound())
	{
		pParamInfo = m_pProcParameters->GetAt(nPos);
		if (pParamInfo->m_strParamName.CompareNoCase(strParamName) == 0)
			return nPos;
	}
	

	for (int i = 0; i <= m_pProcParameters->GetUpperBound(); i++)
	{
		pParamInfo = m_pProcParameters->GetAt(i);
		if (pParamInfo->m_strParamName.CompareNoCase(strParamName) == 0)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
SqlProcedureParamInfo* SqlTableInfo::GetParamInfoByName(const CString& strParamName) const
{
	if (strParamName.IsEmpty() || !m_pProcParameters)
		return NULL;

	SqlProcedureParamInfo* pParamInfo = NULL;

	for (int i = 0; i <= m_pProcParameters->GetUpperBound(); i++)
	{
		pParamInfo = m_pProcParameters->GetAt(i);
		if (pParamInfo->m_strParamName.CompareNoCase(strParamName) == 0)
			return pParamInfo;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
const CRTAddOnNewFieldsArray* SqlTableInfo::GetCRTAddOnNewFields()
{
	if (m_bCheckedSqlNewFieldRT)
		return m_pSqlNewFieldRT;

	if (!m_pSqlCatalogEntry)
	{
		m_bCheckedSqlNewFieldRT = true;
		return m_pSqlNewFieldRT;
	}

	//TB_OBJECT_LOCK(m_pSqlCatalogEntry);

	//qualcuno potrebbe averlo modificato prima del lock
	if (m_bCheckedSqlNewFieldRT)
		return m_pSqlNewFieldRT;	
	
	// il campi aggiuntivi li permetto solo per le tabelle
	if (m_pSqlCatalogEntry->m_nType != TABLE_TYPE)
	{
		m_bCheckedSqlNewFieldRT = true;
		return NULL;
	}
	if (m_pSqlCatalogEntry->GetNamespace().IsEmpty())
		return NULL;

	m_bCheckedSqlNewFieldRT = true;
	m_pSqlNewFieldRT = m_pSqlCatalogEntry->CreateSqlNewFieldRT();

	return m_pSqlNewFieldRT;
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlTableInfo::GetPhisycalColumn (int pos) const
{
	//TB_OBJECT_LOCK(m_arPhisycalColumns);
#ifdef _DEBUG	
	if (pos >= m_arPhisycalColumns.GetCount())
	{
		ASSERT(FALSE);
		return NULL;
	}
#endif
	return (SqlColumnInfo*) m_arPhisycalColumns.GetAt(pos);
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlTableInfo::GetPhisycalSortedColumn (int pos) const
{
	return (SqlColumnInfo*) m_arPhisycalColumns.GetAt(pos);
}

//-----------------------------------------------------------------------------
BOOL SqlTableInfo::SortColumns(SqlRecord* pRec)
{
	//TB_OBJECT_LOCK(m_arPhisycalColumns);

	SAFE_DELETE(m_pMasterRec);
	ASSERT_VALID(pRec);

	m_pMasterRec = pRec;

	if (!pRec || pRec->IsKindOf(RUNTIME_CLASS(DynamicSqlRecord)) || pRec->IsKindOf(RUNTIME_CLASS(UnregisteredSqlRecord)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	int nTabPhisycalCol = m_arPhisycalColumns.GetCount();
	int posSorted = 0;
	int nRecPhisycalCol = 0;

	for (int i = 0; i < m_pMasterRec->GetSize() && posSorted < nTabPhisycalCol; i++)
	{
		SqlRecordItem* pRecItm = m_pMasterRec->GetAt(i);
		if (pRecItm->m_pColumnInfo == NULL || pRecItm->m_pColumnInfo->m_bVirtual)
		{
			continue;
		}

		nRecPhisycalCol++;

		int j;
		for (j = posSorted; j < nTabPhisycalCol; j++)
		{
			SqlColumnInfo* pColInfo = (SqlColumnInfo*) m_arPhisycalColumns.GetAt(j);
			ASSERT_VALID(pColInfo);

			if (pRecItm->m_pColumnInfo == pColInfo)
			{
				if (j > posSorted)
				{
					//NON cambia l'ordine delle altre
					m_arPhisycalColumns.Swap(j, posSorted) ;
				}
				posSorted++;
				break;
			}
		}
		ASSERT(j < nTabPhisycalCol);
	}

	m_bSortedWithRecord = (nRecPhisycalCol == posSorted);

	ASSERT_TRACE2(m_bSortedWithRecord, "Fails to reorder phisycal columns of table %s by sqlrecord class %s\n", 
		(LPCTSTR)m_strTableName, (LPCTSTR)CString(m_pMasterRec->GetRuntimeClass()->m_lpszClassName));

#ifdef _DEBUG
	//if (m_bSortedWithRecord)
	//{
	//	TRACE1("\nReordered table %s\n", (LPCTSTR)m_strTableName);
	//	for (int j = 0; j < m_arPhisycalColumns.GetSize(); j++)
	//	{
	//		SqlColumnInfo* pColInfo = (SqlColumnInfo*) m_arPhisycalColumns.GetAt(j);
	//		ASSERT_VALID(pColInfo);
	//		TRACE1("%s\n", (LPCTSTR)pColInfo->m_strColumnName);
	//	}
	//}
#endif
	return m_bSortedWithRecord;
}

///////////////////////////////////////////////////////////////////////////////
//								SqlCatalogEntry
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(SqlCatalogEntry,CObject)
//-----------------------------------------------------------------------------
SqlCatalogEntry::SqlCatalogEntry
	(
		const CString&	strTableName,
		int				nType,
		LPCTSTR		    pszSignature		/* = _T("")*/,
		CRuntimeClass*	pSqlRecordClass		/* = NULL */,
		BOOL			bExist				/* = TRUE */,
		BOOL			bVirtual			/* = FALSE */
	)
	:
	IOSLObjectManager(nType == TABLE_TYPE ? OSLType_Table : nType == VIEW_TYPE ? OSLType_View : OSLType_Function)
{
	m_bTraceChecked		= false;
	m_strTableName		= strTableName;
	m_strSignature		= CString(pszSignature);	
	m_pSqlRecordClass	= pSqlRecordClass;
	m_bExist 			= bExist;
	m_bVirtual 			= bVirtual == TRUE;
	m_nType 			= nType;
	m_pTableItem		= NULL;
	m_pTableInfo		= NULL;
	m_pAuditingMng		= NULL;
	m_pTableRowSecurityMng	= NULL;
	m_pDBObjectDescription = AfxGetDbObjectDescription(m_strTableName);
}

//-----------------------------------------------------------------------------
SqlCatalogEntry::~SqlCatalogEntry()
{
	SAFE_DELETE(m_pTableInfo);
	SAFE_DELETE(m_pAuditingMng);
	SAFE_DELETE(m_pTableRowSecurityMng);
	SAFE_DELETE(m_pTableItem);
}

//-----------------------------------------------------------------------------
BOOL SqlCatalogEntry::HasBeenRegistered() const
{
	//i moduli dinamici non hanno m_pSqlRecordClass
	return m_pDBObjectDescription != NULL;
}

//-----------------------------------------------------------------------------
SqlRecord* SqlCatalogEntry::CreateRecord() const
{
	//è dinamico? creo il SqlRecord generico a partire dal nome tabella
	if (m_pDBObjectDescription && m_pDBObjectDescription->GetDeclarationType() == CDbObjectDescription::Dynamic)
		return new DynamicSqlRecord(m_strTableName);

	//ho una runtime class specifica? ne creo un'istanza
	if (m_pSqlRecordClass)
		return (SqlRecord*) m_pSqlRecordClass->CreateObject();

	//non ce l'ho? potrebbe non essere venuta su la dll, ptovo a caricarla
	if (m_pDBObjectDescription)
		AfxGetTbCmdManager()->LoadNeededLibraries(m_pDBObjectDescription->GetNamespace());

	//è venuta su? la creo
	if  (m_pSqlRecordClass != NULL)
		return  (SqlRecord*) m_pSqlRecordClass->CreateObject();
	//in ultima istanza, creo un SqlRecord con dei dataobj di default in base al tipo di campo
	return new UnregisteredSqlRecord(m_strTableName);
}

//-----------------------------------------------------------------------------
CRTAddOnNewFieldsArray* SqlCatalogEntry::CreateSqlNewFieldRT() const
{	
	CRTAddOnNewFieldsArray* pSqlNewFieldRT = new CRTAddOnNewFieldsArray;
	
	// Inserisce in pSqlNewFieldRT le Runtime Class delle evenuali classi 
	// che gestiscono i campi aggiunti. Prima scatena le eventuali DLL mancanti
	
	if (m_pDBObjectDescription && m_pDBObjectDescription->GetDeclarationType() < CDbObjectDescription::Dynamic)
		AfxGetTbCmdManager()->LoadNeededLibraries(GetNamespace());
	
	const CAddColsTableDescription* pDescri =  AfxGetAddOnFieldsOnTable(GetNamespace());
	if (pDescri)
	{
		AddOnLibrary*		pAddOnLib;
		AddOnApplication*	pAddOnApp;
		for (int i=0; i < pDescri->m_arAlterTables.GetSize(); i++)
		{
			const CTBNamespace& aLibNs = pDescri->m_arAlterTables.GetAt(i)->GetNamespace();
			
			pAddOnLib = AfxGetAddOnLibrary(aLibNs);
			pAddOnApp = AfxGetAddOnApp(aLibNs.GetApplicationName());

			if (pAddOnLib && pAddOnLib->m_pAddOn && pAddOnApp)
			{
				if (AfxIsActivated (aLibNs.GetApplicationName (), aLibNs.GetModuleName()))
					pAddOnLib->m_pAddOn->AOI_AddOnNewColumns(this, pSqlNewFieldRT, pAddOnApp->GetSignature(), aLibNs);
			}
		}
	}

	if (pSqlNewFieldRT->GetSize() <= 0) 
		SAFE_DELETE(pSqlNewFieldRT);

	return pSqlNewFieldRT;
}
//-----------------------------------------------------------------------------
void SqlCatalogEntry::SetTraced()
{
	if (m_bTraceChecked || !s_pfCallTraced)
		return;

	m_bTraceChecked = s_pfCallTraced(m_strTableName, &m_pAuditingMng);
}

//-----------------------------------------------------------------------------
void SqlCatalogEntry::TraceOperation(int eType, SqlTable* pTable	) const
{
	if (m_pAuditingMng) 
		m_pAuditingMng->TraceOperation(eType, pTable);
}

//-----------------------------------------------------------------------------
void SqlCatalogEntry::BindTracedColumns(SqlTable* pTable) const
{
	if (m_pAuditingMng) 
		m_pAuditingMng->BindTracedColumns(pTable); 
}

//chiamate da XTech per l'esportazione
//-----------------------------------------------------------------------------
void SqlCatalogEntry::PrepareQuery(SqlTable* pTable, DataDate& aFrom, DataDate& aTo, int eType) const
{
	if (m_pAuditingMng)
		m_pAuditingMng->PrepareQuery(pTable, aFrom, aTo, eType); 
}

//-----------------------------------------------------------------------------
BOOL SqlCatalogEntry::PrepareDeletedQuery(SqlTable* pTable, DBTMaster* pDBTMaster, DataDate& aFrom, DataDate& aTo) const
{
	return m_pAuditingMng
		? m_pAuditingMng->PrepareDeletedQuery(pTable, pDBTMaster, aFrom, aTo) 
		: FALSE; 
} 

	//per creare al volo un report di woorm sui dati di tracciatura
	// con gli eventuali valori di fixedkey dipendenti dal documento
	//viene restituito il namespace del report creato
//-----------------------------------------------------------------------------
CTBNamespace* SqlCatalogEntry::CreateAuditingReport(CTBNamespace* pNamespace, CXMLFixedKeyArray* pFixedArray, BOOL bAllUsers, const CString& strUser) const
{
	return m_pAuditingMng
		? m_pAuditingMng->CreateAuditingReport(pNamespace, pFixedArray, bAllUsers, strUser) 
		: NULL; 
} 

//-----------------------------------------------------------------------------
void SqlCatalogEntry::SetNamespace(const CTBNamespace& strNamespace) 
{ 
	//TB_LOCK_FOR_WRITE();
	GetInfoOSL()->m_Namespace = strNamespace; 
}

//-----------------------------------------------------------------------------
void SqlCatalogEntry::SetDbDescription(CDbObjectDescription* pNewDescription)
{ 
	this->m_pDBObjectDescription = pNewDescription;
}

//-----------------------------------------------------------------------------
BOOL SqlCatalogEntry::SortTableInfoColumns() 
{ 
	if (this->m_bVirtual)
		return FALSE;
	if (m_nType != TABLE_TYPE && m_nType != VIEW_TYPE)
		return FALSE;

	SqlRecord* pRec = CreateRecord();
	if (pRec)
	{
		ASSERT_VALID(pRec);
		ASSERT_VALID(pRec->m_pTableInfo);
		ASSERT(m_pTableInfo == pRec->m_pTableInfo);
		/*if(!pRec->IsKindOf(RUNTIME_CLASS(DynamicSqlRecord)))
			pRec->ValorizeDBObjectDescription(m_pDBObjectDescription);*/

		if (m_pTableInfo && m_pTableInfo->m_bValid)
		{
			if (!pRec->IsKindOf(RUNTIME_CLASS(DynamicSqlRecord)) && !pRec->IsKindOf(RUNTIME_CLASS(UnregisteredSqlRecord)))
				VERIFY(m_pTableInfo->SortColumns(pRec));
			else
				SAFE_DELETE(pRec);
		}
		else
			delete pRec;
	}
	else
	{
		CString msg;
		if (AfxGetDbObjectDescription(m_strTableName) == NULL)
			msg = _T("Record without database object description, try to use macro REGISTER_VIRTUAL_TABLE; ");

		if (this->m_pSqlRecordClass)
			msg += cwsprintf(_T("record class name is %s"), (LPCTSTR)CString(this->m_pSqlRecordClass->m_lpszClassName));
		else
			msg += _T("record without runtime class");

		TRACE3("Cannot create record of table %s\n in %s\n%s\n", (LPCTSTR)m_strTableName, (LPCTSTR)GetInfoOSL()->m_Namespace.ToString(), (LPCTSTR)msg);

		//se non è una tabella registrata allora rimuovo le informazioni  di schema. Saranno eventualmente lette all'occorrenza
		if (m_pTableItem)
		{
			m_pTableItem->m_arColumnsInfo.RemoveAll();
			m_pTableItem->m_arProcedureParams.RemoveAll();
		}
	}
	return FALSE;
}

//TBROWSECURITY
//-----------------------------------------------------------------------------
void SqlCatalogEntry::SetProtected(CTableRowSecurityMngObj* pTableRowSecurityMng) 
{
	TB_LOCK_FOR_WRITE();
	m_pTableRowSecurityMng = pTableRowSecurityMng;
}


//-----------------------------------------------------------------------------
void SqlCatalogEntry::AddRowSecurityFilters(SqlTable* pTable, SqlTableItem* pTableItem) const
{
	 if (m_pTableRowSecurityMng)
		  m_pTableRowSecurityMng->AddRowSecurityFilters(pTable, pTableItem);
}
		
//-----------------------------------------------------------------------------
void SqlCatalogEntry::ValorizeRowSecurityParameters(SqlTable* pTable) const
{
	if (m_pTableRowSecurityMng)
		m_pTableRowSecurityMng->ValorizeRowSecurityParameters(pTable);
}

//-----------------------------------------------------------------------------
CString SqlCatalogEntry::GetSelectGrantString(SqlTable* pTable) const
{
	return (m_pTableRowSecurityMng) ? m_pTableRowSecurityMng->GetSelectGrantString(pTable) : _T("");
}

//-----------------------------------------------------------------------------
BOOL SqlCatalogEntry::CanCurrentWorkerUsesRecord(SqlRecord* pRecord, SqlTable* pTable) const
{
	return (m_pTableRowSecurityMng) ? m_pTableRowSecurityMng->CanCurrentWorkerUsesRecord(pRecord, pTable) : TRUE;
}


//-----------------------------------------------------------------------------
void SqlCatalogEntry::HideProtectedFields(SqlRecord* pRecord) const
{
	if (m_pTableRowSecurityMng)
		m_pTableRowSecurityMng->HideProtectedFields(pRecord);
}

//=============================================================================
static void ShowRegistration (const CString& strTableName)
{
	// Non fa vedere la registrazione delle tabelle di TaskBuilder (Locks e DBMark)
	if	(strTableName.CompareNoCase(SqlDBMark::GetStaticName()) == 0 )
		return;
	// mostra il nome della tabella in fase di registrazione	
	AfxSetStatusBarText(cwsprintf(_TB("Registrating {0-%s}... Please wait."), (LPCTSTR)strTableName));
}





//////////////////////////////////////////////////////////////////////////////
//								COslObArray
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(CGroupTableObArray, CObArray)

//////////////////////////////////////////////////////////////////////////////
//								SqlCatalog
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(SqlCatalog, CMapStringToOb);

//-----------------------------------------------------------------------------
SqlCatalog::SqlCatalog()
	:
	m_bValid (TRUE),
	m_bLoaded(FALSE)
{
}

//-----------------------------------------------------------------------------
void SqlCatalog::Load(SqlConnection *pConnection)
{
	TB_LOCK_FOR_WRITE();
	if (m_bLoaded)
		return;
	
	m_bLoaded = TRUE;

	//// usa il datasource di default
	//SqlTables tables(pConnection);
	//
	//m_bValid = tables.GetTables() && tables.GetStoredProcedures() && tables.GetSize() > 0;

	//// Riempie la mappa del catalog
	//int nType;
	//for (int i = 0; i <= tables.GetUpperBound(); i++)
	//{
	//	SqlTablesItem* pTable = tables[i];
	//	if (pTable->m_strType.Find(_T("BASE TABLE")) >= 0)
	//		nType = TABLE_TYPE;
	//	else
	//	{
	//		if (pTable->m_strType.Find(_T("VIEW")) >= 0)
	//			nType = VIEW_TYPE;
	//		else 
	//			nType = PROC_TYPE;
	//	}
	//	
	//	//i moduli dinamici non hanno m_pSqlRecordClass
	//	const CDbObjectDescription* pXmlDescri = AfxGetDbObjectDescription(pTable->m_strName);	
	//	AddOnModule *pModule = pXmlDescri ? AfxGetAddOnModule(pXmlDescri->GetNamespace()) : NULL;

	//	SqlCatalogEntry* pCatalogEntry = new SqlCatalogEntry(pTable->m_strName, nType);
	//	pCatalogEntry->m_pTableItem = pTable;
	//	//in caso di modulo dinamico, imposto già adesso le informazioni: non ci sarà una successiva fase di registrazione
	//	if (pModule && pCatalogEntry->m_strSignature.IsEmpty())
	//	{
	//		pCatalogEntry->SetNamespace(pXmlDescri->GetNamespace());
	//		pCatalogEntry->m_strSignature = pModule->GetModuleSignature();
	//	}
	//	SetEntry
	//		(
	//			pTable->m_strName, 
	//			pCatalogEntry
	//		);
	//}   

	
	CMapStringToOb tablesMap;

	TRY
	{
		pConnection->LoadTables(&tablesMap);
		pConnection->LoadProcedures(&tablesMap);
	}
	CATCH(SqlException, e)
	{
		pConnection->m_pContext->AddMessage(e->m_strError);
		m_bValid = FALSE;
		return;
	}
	END_CATCH

	m_bValid = tablesMap.GetSize() > 0;

	// Riempie la mappa del catalog
	int nType;

	SqlTablesItem* pTable;
	CString strKey;
	POSITION pos;

	for (pos = tablesMap.GetStartPosition(); pos != NULL;)
	{
		tablesMap.GetNextAssoc(pos, strKey, (CObject*&)pTable);
		if (!pTable)
			continue;

		if (pTable->m_strType.Find(_T("BASE TABLE")) >= 0)
			nType = TABLE_TYPE;
		else
		{
			if (pTable->m_strType.Find(_T("VIEW")) >= 0)
				nType = VIEW_TYPE;
			else
				nType = PROC_TYPE;
		}

		//i moduli dinamici non hanno m_pSqlRecordClass
		const CDbObjectDescription* pXmlDescri = AfxGetDbObjectDescription(pTable->m_strName);
		AddOnModule *pModule = pXmlDescri ? AfxGetAddOnModule(pXmlDescri->GetNamespace()) : NULL;

		SqlCatalogEntry* pCatalogEntry = new SqlCatalogEntry(pTable->m_strName, nType);
		pCatalogEntry->m_pTableItem = pTable;
		//in caso di modulo dinamico, imposto già adesso le informazioni: non ci sarà una successiva fase di registrazione
		if (pModule && pCatalogEntry->m_strSignature.IsEmpty())
		{
			pCatalogEntry->SetNamespace(pXmlDescri->GetNamespace());
			pCatalogEntry->m_strSignature = pModule->GetModuleSignature();
		}
		SetEntry
		(
			pTable->m_strName,
			pCatalogEntry
		);
	}

	if (!LoadColumnCollations(pConnection))
		AfxGetDiagnostic()->Add(_TB("Cannot load columns collation information from the catalog of the connected company database. The program will work with database default collation."), CDiagnostic::Warning);

	
}

//-----------------------------------------------------------------------------
SqlCatalog::~SqlCatalog()
{
	POSITION			pos;
	CString				key;
	SqlCatalogEntry* pCatalogEntry;
		
	for (pos = GetStartPosition(); pos != NULL;)
	{
		GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);
		if (AfxIsValidAddress(pCatalogEntry, sizeof(SqlCatalogEntry)))
			delete pCatalogEntry;
	}	
}

//-----------------------------------------------------------------------------
BOOL SqlCatalog::ExistTable(const CString& strTableName) const
{
	const SqlCatalogEntry* pCatalogEntry = GetEntry(strTableName);

	return 
		pCatalogEntry && 
		(pCatalogEntry->m_nType == TABLE_TYPE || pCatalogEntry->m_nType == VIEW_TYPE) &&
		pCatalogEntry->m_bExist;
}

//-----------------------------------------------------------------------------
BOOL SqlCatalog::RegisterCatalogEntry 
	(
		SqlConnection*		pConnection,		
		LPCTSTR				pszSignature,
		const CTBNamespace&	aNamespace,
		CRuntimeClass*		pSqlRecordClass,
		int					nType,
		CDbObjectDescription* pDbDescription /*= NULL*/
	)
{
	//TB_LOCK_FOR_WRITE();
	CString strTableName = aNamespace.GetObjectName();

 	BOOL bExist = (nType == VIRTUAL_TYPE); //di default non esiste

	SqlCatalogEntry* pCatalogEntry = GetEntry(strTableName);

	AddOnModule* pAddOnMod = AfxGetAddOnModule (aNamespace);
	ASSERT (pAddOnMod);

	if (pCatalogEntry)
	{
		// Se la tabella è gia registrata da una precedente dll allora non aggiorno l'entry
		// e visualizzo un messaggio di errore
		if (pCatalogEntry->GetSqlRecordClass())
		{
			if(aNamespace != pCatalogEntry->GetNamespace())
			{
				pConnection->AddMessage(
						cwsprintf(	_TB("Unable to register the table {0-%s} of the application {1-%s}.\r\nThe table has already been registered by the application {2-%s}."),
									(LPCTSTR) strTableName, 
									pszSignature, 
									pCatalogEntry->m_strSignature));
				return TRUE;
			}
			return TRUE;
		}
		//altrimenti aggiorno le info della tabella
		pCatalogEntry->m_strSignature = pszSignature;
		pCatalogEntry->SetSqlRecordClass(pSqlRecordClass);
		pCatalogEntry->SetNamespace(aNamespace);

		if (pDbDescription)
		{
			pCatalogEntry->SetDbDescription(pDbDescription);
		}

		bExist = TRUE;

		//devo posticiparlo poiche alcuni record fanno riferimento ad altri potenzialmente non registrati nel costruttore
		//pCatalogEntry->SortTableInfoColumns();

		//se é presente l'auditing allora controllo la traccibilitá della tabella		
		// se é sotto tracciatura allora istanzio il suo auditing manager
		if (AfxGetLoginInfos()->m_bAuditing && AfxIsActivated(TBEXT_APP, TBAUDITING_ACT))
			pCatalogEntry->SetTraced();		
	}
	else
	{
		if (nType == VIRTUAL_TYPE)
		{
			// registro la classe ma la tabella non esiste ancora nel database
			pCatalogEntry = new SqlCatalogEntry(strTableName, nType, pszSignature, pSqlRecordClass, bExist);
			pCatalogEntry->SetNamespace(aNamespace);
			if (!bExist)
				TRACE1("SqlCatalog::RegisterCatalogEntry: the table %s doesn't exist\n", aNamespace.ToString());
			SetEntry(strTableName, pCatalogEntry);	

			pCatalogEntry->SetDbDescription(pDbDescription);
		}
		else
			pConnection->AddMessage(
					cwsprintf	
					(
						_TB("Missing table {0-%s} in {1-%s} library."),	
						aNamespace.ToUnparsedString(), strTableName
					)
				);
	}

	return bExist;
}

//-----------------------------------------------------------------------------
void SqlCatalog::SortTableInfoColumns()
{
	TB_LOCK_FOR_WRITE();

	POSITION	pos;
	CString		key;
	SqlCatalogEntry* pCatalogEntry;

	for (pos = GetStartPosition(); pos != NULL;)
	{
		GetNextAssoc(pos, key, (CObject*&)pCatalogEntry);
		
		pCatalogEntry->SortTableInfoColumns();
	}

	//CXMLDatabaseObjectsParser aDbParser;
	//DatabaseObjectsTablePtr pTablePtr = AfxGetWritableDatabaseObjectsTable();
	//aDbParser.SaveDatabaseObjects(pTablePtr.GetPointer());

	//CXMLAddOnDatabaseObjectsParser aAddOnDbParse;
	//CAlterTableDescriptionArray* pAlterTable = (CAlterTableDescriptionArray*)AfxGetAddOnFieldsTable();
	//aAddOnDbParse.SaveAdddOnDatabaseObjects(pAlterTable);

}

//-----------------------------------------------------------------------------
void SqlCatalog::SetEntry(const CString& strTableName, SqlCatalogEntry* pEntry)
{
	SqlCatalogEntry* pPrevEntry;
	CString strKey(strTableName); strKey.MakeLower();

	if (!Lookup(strKey, (CObject*&) pPrevEntry))
	{
		SetAt(strKey, pEntry);
	}
	else
	{
		ASSERT_VALID(pPrevEntry);
		ASSERT_TRACE3(FALSE, "The table %s is registered again:\n\t%s\n\t%s\n", strTableName, pEntry->GetNamespace().ToString(), pPrevEntry->GetNamespace().ToString());
	}
}

//-----------------------------------------------------------------------------
SqlCatalogEntry* SqlCatalog::GetEntry(const CString& strTableName) const
{	
	SqlCatalogEntry* pCatalogEntry;
	CString strKey(strTableName); strKey.MakeLower();

	return Lookup(strKey, (CObject*&) pCatalogEntry) ? pCatalogEntry : NULL;
}

//-----------------------------------------------------------------------------
SqlCatalogEntry* SqlCatalog::GetEntry(const CTBNamespace& ns) const
{
	POSITION	pos;
	CString		key;
	SqlCatalogEntry* pCatalogEntry;

	for (pos = GetStartPosition(); pos != NULL;)
	{
		GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);
		if (pCatalogEntry && pCatalogEntry->GetInfoOSL()->m_Namespace == ns)
			return pCatalogEntry;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
CString SqlCatalog::GetTableSignature(const CString& strTableName) const
{
	const SqlCatalogEntry* pCatalogEntry = GetEntry(strTableName);
	return pCatalogEntry ? pCatalogEntry->m_strSignature : _T("");
}

//-----------------------------------------------------------------------------
const CTBNamespace* SqlCatalog::GetNamespace (const CString& strTableName) const 
{
	const SqlCatalogEntry* pE = GetEntry(strTableName); return pE ? &(pE->GetNamespace()) : NULL; 
}
//-----------------------------------------------------------------------------
CRuntimeClass* SqlCatalog::GetSqlRecordClass(const CString& strTableName) const
{
 	const SqlCatalogEntry* pCatalogEntry = GetEntry(strTableName);
	return pCatalogEntry ? pCatalogEntry->GetSqlRecordClass() : NULL;		
}

//-----------------------------------------------------------------------------
CRuntimeClass* SqlCatalog::GetSqlRecordClass(const CTBNamespace& ns) const
{
 	const SqlCatalogEntry* pCatalogEntry = GetEntry(ns);
	return pCatalogEntry ? pCatalogEntry->GetSqlRecordClass() : NULL;		
}

//-----------------------------------------------------------------------------
SqlTableInfo* SqlCatalog::GetTableInfo(const CString& strTableName, SqlConnection* pConnection)
{
 	SqlCatalogEntry* pCatalogEntry = GetEntry(strTableName);

	if (!pCatalogEntry)
		return NULL;
	
	//BEGIN_TB_LOCK_FOR_READ()	
	if (pCatalogEntry->m_pTableInfo)
		return pCatalogEntry->m_pTableInfo;
	if (!pCatalogEntry->m_bExist)
		return NULL;
	//END_TB_LOCK_FOR_READ()
	
	ShowRegistration(pCatalogEntry->m_strTableName);
	//TB_LOCK_FOR_WRITE()
	if (!pCatalogEntry->m_pTableInfo) //nel frattempo quelche altro thread potrebbe averlo valorizzato
		pCatalogEntry->m_pTableInfo = new SqlTableInfo(pCatalogEntry, pConnection);
	
	return pCatalogEntry->m_pTableInfo;
}

// controlla se c'e' almeno una tabella registrata esistente
//-----------------------------------------------------------------------------
BOOL SqlCatalog::DatabaseEmpty() const 
{
	POSITION	pos;
	CString		key;
	SqlCatalogEntry* pCatalogEntry;

	for (pos = GetStartPosition(); pos != NULL;)
	{
		GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);
		if (pCatalogEntry->m_bExist)
			return FALSE;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlCatalog::GetRegisteredTableNames(CStringArray& arTableNames) const
{
	POSITION pos;
	CString key;
	SqlCatalogEntry* pCatalogEntry;

	for (pos = GetStartPosition(); pos != NULL;)
	{
		GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);
		if (pCatalogEntry->HasBeenRegistered())
			arTableNames.Add(pCatalogEntry->m_strTableName);
	}
}

//-----------------------------------------------------------------------------
void SqlCatalog::RefreshTraces	()
{
	TB_LOCK_FOR_WRITE();

	POSITION	pos;
	CString		key;
	SqlCatalogEntry* pCatalogEntry;

	for (pos = GetStartPosition(); pos != NULL;)
	{
		GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);
		if (pCatalogEntry->HasBeenRegistered())
			pCatalogEntry->SetTraced();
	}
}

//-----------------------------------------------------------------------------
const CString& SqlCatalog::GetDatabaseCollation () const
{
	return m_sDatabaseCollation;
}

//-----------------------------------------------------------------------------
CString SqlCatalog::ReadServerCollationName(SqlConnection *pConnection)
{
	DataStr aDBCollate;

	SqlTable aTable(pConnection->GetDefaultSqlSession());
	aTable.Select(_T("DBCollation"), &aDBCollate);
	aTable.m_strSQL = szServerCollationQuery;
	TRY
	{
		aTable.Open();
		aTable.ScalarQuery();
	}
		CATCH(SqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH
		if (aTable.IsOpen())
			aTable.Close();

	return aDBCollate.Str();
}

//-----------------------------------------------------------------------------
CString SqlCatalog::ReadDatabaseCollationName(SqlConnection* pConnection)
{
	CString sDbSyntax = _T("'") + pConnection->m_strDBName + _T("'");

	if (pConnection->UseUnicode())
		sDbSyntax = _T("N") + sDbSyntax;

	SqlTable aTable(pConnection->GetDefaultSqlSession());
	DataStr aDBCollate;
	aTable.Select(_T("DBCollation"), &aDBCollate);
	aTable.m_strSQL = cwsprintf(szDatabaseCollationQuery, sDbSyntax);
	TRY
	{
		aTable.Open();
		aTable.ScalarQuery();
	}
		CATCH(SqlException, e)
	{
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH
		if (aTable.IsOpen())
			aTable.Close();

	return aDBCollate.Str();
}

//-----------------------------------------------------------------------------
BOOL SqlCatalog::LoadColumnCollations(SqlConnection *pConnection)
{
	// databasepropertyex collation ATL query
	m_sDatabaseCollation = ReadDatabaseCollationName(pConnection);

	// serverproperty collation ATL query
	if (m_sDatabaseCollation.IsEmpty())
		m_sDatabaseCollation = ReadServerCollationName(pConnection);
	
	return TRUE;
}


//-----------------------------------------------------------------------------
BOOL SqlCatalog::IsCollationCultureSensitive (SqlColumnInfo* pColumnInfo, SqlConnection *pConnection) const
{
	if (!pColumnInfo || AfxGetCultureInfo()->IsInvariantCulture())
		return FALSE;


	CString sCollationName = GetColumnCollationName (pColumnInfo->m_strTableName, pColumnInfo->m_strColumnName);
	
	// no column collation, previous invariant behaviour
	if (sCollationName.IsEmpty())
		sCollationName = GetDatabaseCollation ();
	
	if (sCollationName.IsEmpty ())
		return FALSE;

	return sCollationName.CompareNoCase(szCollationForInvariant) != 0;
}

// gets column collate from database preloaded map. It uses invariant culture
// as they are database objects names
//-----------------------------------------------------------------------------
CString SqlCatalog::GetColumnCollationName (const CString& sTableName, const CString& sColumnName) const
{
	SqlCatalogEntry *pEntry = GetEntry(sTableName);
	if (!pEntry)
	{
		ASSERT(FALSE);
		return m_sDatabaseCollation;
	}
	CString sCollation;
	if (!pEntry->m_ColumnsCollations.Lookup(sColumnName, sCollation))
		return m_sDatabaseCollation;

	return sCollation;
}

//-----------------------------------------------------------------------------
void SqlCatalog::RemoveDynamicCatalogEntry(const CString& strTableName)
{
	TB_LOCK_FOR_WRITE();

	SqlCatalogEntry *pEntry = GetEntry(strTableName);
	if (!pEntry)
	{
		ASSERT(FALSE);
		return;
	}
	CString strKey(strTableName); strKey.MakeLower();
	RemoveKey(strKey);
	delete pEntry;
}
//-----------------------------------------------------------------------------
BOOL SqlCatalog::AddDynamicCatalogEntry(SqlConnection* pConnection, const CTBNamespace& aNamespace, const int& nType, bool isVirtual, CDbObjectDescription* pDbDescription)
{
	AddOnModule* pAddOnMod = AfxGetAddOnModule(aNamespace);
	if (!pAddOnMod)
		return FALSE;

	TB_LOCK_FOR_WRITE();

	CString aName = aNamespace.GetObjectName();
	SqlCatalogEntry* pCatalogEntry = GetEntry(aName);
	if (!pCatalogEntry)
	{
		pCatalogEntry = new SqlCatalogEntry(aNamespace.GetObjectName(), nType, _T(""), NULL, TRUE, isVirtual);
		SetEntry(aName, pCatalogEntry);
	}

	BOOL bOk = RegisterCatalogEntry
					(
						pConnection,
						pAddOnMod->GetModuleSignature(), 
						aNamespace,
						NULL, 
						nType,
						pDbDescription
					);
	if (bOk)
	{
		SqlTableInfo* pTableInfo = GetTableInfo(aNamespace.GetObjectName(), pConnection);
		pTableInfo->m_bValid = TRUE;//valido anche se non ci sono colonne
	}

	return bOk;
}
//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlCatalog::AddDynamicColumnInfo(	
													const	CString&	strTableName,
													const	CString&	strColumnName,
													const	DataObj&	aDataObj,
													int		nLen,
													BOOL	bIsCollateCultureSensitive,
													BOOL	bVirtual,
													BOOL	bSpecial
												)
{
	SqlTableInfo* pTableInfo = GetTableInfo(strTableName, AfxGetDefaultSqlConnection());
	
	return pTableInfo 
		? pTableInfo->AddDynamicColumnInfo(strColumnName, aDataObj, nLen, bIsCollateCultureSensitive, bVirtual, bSpecial) 
		: NULL;
}

//-----------------------------------------------------------------------------
void SqlCatalog::RemoveDynamicColumnInfo(const CString& strTableName, const CString& strColumnName)
{
	SqlCatalogEntry* pCatalogEntry = GetEntry(strTableName);
	if (pCatalogEntry)
		pCatalogEntry->RemoveDynamicColumnInfo(strColumnName);
}

//-----------------------------------------------------------------------------
const SqlColumnInfo* SqlCatalog::AddVirtualColumnInfo(
															const CString&		strTableName,
															const	CString&	strColumnName,
															const	DataObj&	aDataObj,
															int			nLen,
															BOOL bIsCollateCultureSensitive
														)
{
	SqlTableInfo* pTableInfo = GetTableInfo(strTableName, AfxGetDefaultSqlConnection());
	
	return pTableInfo ? pTableInfo->AddVirtualColumnInfo(-1, strColumnName, aDataObj, nLen, bIsCollateCultureSensitive) : NULL;
}


//-----------------------------------------------------------------------------
BOOL SqlCatalog::ReloadTableInfo(const CString& strTableName, SqlConnection* pConnection)
{	//non viene mai chiamato ?
	SqlCatalogEntry* pCatalogEntry = GetEntry(strTableName);

	if (!pCatalogEntry)
		return FALSE;
	
	BEGIN_TB_LOCK_FOR_READ()	
		if (!pCatalogEntry->m_bExist)
			return FALSE;	
	END_TB_LOCK_FOR_READ()
	
	TB_LOCK_FOR_WRITE()

	if (pCatalogEntry->m_pTableInfo) 
		delete pCatalogEntry->m_pTableInfo;
	
	//nel frattempo quelche altro thread potrebbe averlo valorizzato
	pCatalogEntry->m_pTableInfo = new SqlTableInfo(pCatalogEntry, pConnection, TRUE);

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
//								SqlTypeInfoItem
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
SqlTypeInfoItem::SqlTypeInfoItem()
{
	m_strTypeName		= _T("");
	m_nSqlDataType		= DBTYPE_WSTR;
	m_lPrecision		= 0;
	m_strPrefix			= _T("");
	m_strSuffix			= _T("");
	m_strCreateParams	= _T("");
	m_bNullable			= FALSE;
	m_bCaseSensitive	= FALSE;
	m_nSearchable		= DB_SEARCHABLE;
	m_bUnsignedAttribute= FALSE;
	m_nMoney			= FALSE;
	m_bAutoIncrement	= FALSE;
	m_strLocalTypeName	= _T("");
}

//-----------------------------------------------------------------------------
void SqlTypeInfoItem::operator= (const SqlTypeInfoItem& src)
{
	m_strTypeName		= src.m_strTypeName;
	m_nSqlDataType		= src.m_nSqlDataType;
	m_lPrecision		= src.m_lPrecision;
	m_strPrefix			= src.m_strPrefix;
	m_strSuffix			= src.m_strSuffix;
	m_strCreateParams	= src.m_strCreateParams;
	m_bNullable			= src.m_bNullable;
	m_bCaseSensitive	= src.m_bCaseSensitive;
	m_nSearchable		= src.m_nSearchable;
	m_bUnsignedAttribute= src.m_bUnsignedAttribute;
	m_nMoney			= src.m_nMoney;
	m_bAutoIncrement	= src.m_bAutoIncrement;
	m_strLocalTypeName	= src.m_strLocalTypeName;
}


/////////////////////////////////////////////////////////////////////////////
//								SqlTypeInfo
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlTypeInfo, Array)

//-----------------------------------------------------------------------------
SqlTypeInfo::SqlTypeInfo(SqlSession* pSqlSession)
{
	if (!pSqlSession || !pSqlSession->GetSession())
		return;

    ASSERT_VALID(this);
	CProviderTypes* pProviderType = new	CProviderTypes;

	TRY
	{ 	
		if (pProviderType->Open(*(CSession*)pSqlSession->GetSession()) != S_OK)
			return;

		while(pProviderType->MoveNext() == S_OK)
		{
			SqlTypeInfoItem* pTypeInfoItem = new SqlTypeInfoItem();	
			
			pTypeInfoItem->m_strTypeName		= (LPTSTR) pProviderType->m_szTypeName;
			pTypeInfoItem->m_nSqlDataType		= pProviderType->m_nDataType;
			pTypeInfoItem->m_lPrecision			= pProviderType->m_nColumnSize;
			pTypeInfoItem->m_strPrefix			= (LPTSTR) pProviderType->m_szLiteralPrefix;
			pTypeInfoItem->m_strSuffix			= (LPTSTR) pProviderType->m_szLiteralSuffix;
			pTypeInfoItem->m_strCreateParams	= (LPTSTR) pProviderType->m_szCreateParams;
			pTypeInfoItem->m_bNullable			= pProviderType->m_bIsNullable;
			pTypeInfoItem->m_bCaseSensitive 	= pProviderType->m_bCaseSensitive;
			pTypeInfoItem->m_nSearchable		= pProviderType->m_nSearchable;
			pTypeInfoItem->m_bUnsignedAttribute = pProviderType->m_bUnsignedAttribute;
			pTypeInfoItem->m_nMoney				= pProviderType->m_bFixedPrecScale;
			pTypeInfoItem->m_bAutoIncrement 	= pProviderType->m_bAutoUniqueValue;
			pTypeInfoItem->m_strLocalTypeName	= (LPTSTR) pProviderType->m_szLocalTypeName;

			Add(pTypeInfoItem);
		}
	
		delete pProviderType;
		pProviderType = NULL;
	}

	CATCH(SqlException, e)
	{
		if (pProviderType)
		{
			delete pProviderType;
			pProviderType = NULL;
		}	
		THROW_LAST();
	}
	END_CATCH	
}

//-----------------------------------------------------------------------------
SqlCatalog* AfxGetSqlCatalog (SqlConnection* pConnection)
{
	CString strKey;
	strKey.Format(_T("%s@%s@%s"), pConnection->GetDatabaseName(), pConnection->GetDatabaseServerName(), pConnection->GetDbmsName());
	strKey.MakeLower();
	CCompanyContext *pContext = AfxGetCompanyContext(strKey);
	SqlCatalog* pCatalog = pContext->GetObject<SqlCatalog>();
	pCatalog->Load(pConnection);
	return pCatalog;
}

//============================================================================
// DataFieldLink
//============================================================================
IMPLEMENT_DYNAMIC(DataFieldLink, CObject)

DataFieldLink::DataFieldLink(LPCTSTR pszPhysicalName, LPCTSTR pszPublicName, BOOL bHiddenLink, /*BOOL bNativeExpr,*/ DataType type)
	:
	m_strPhysicalName	(pszPhysicalName),
	m_strPublicName		(pszPublicName),
	m_bHidden			(bHiddenLink),
	m_type				(type)
{
}

DataFieldLink::DataFieldLink(const DataFieldLink& src)
:
	m_strPhysicalName	(src.m_strPhysicalName),
	m_strPublicName		(src.m_strPublicName),
	m_bHidden			(src.m_bHidden),
	m_type				(src.m_type)
{
}

//============================================================================
IMPLEMENT_DYNAMIC(DataFieldLinkArray, Array)

DataFieldLink* DataFieldLinkArray::Find(const CString& sName) const
{
	int idxPoint = sName.Find('.');
 
	for (int i = 0; i < GetSize(); i++)
	{
		DataFieldLink* pLink = GetAt(i);

		CString sColName = pLink->m_strPhysicalName;

		int idx = sColName.Find('.');
		if (idx > -1 && idxPoint == -1)
			sColName = sColName.Mid(idx + 1);

		if (sName.CompareNoCase(sColName) == 0)
			return pLink;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void DataFieldLinkArray::Copy(const DataFieldLinkArray& src)
{
	for (int i = 0; i < src.GetSize(); i++)
	{
		DataFieldLink* pLink = src.GetAt(i);

		DataFieldLink* pNew = new DataFieldLink(*pLink);

		Add(pNew);
	}
}

//-----------------------------------------------------------------------------
void DataFieldLinkArray::SetQualified(const CString& sTableName)
{
	for (int i = 0; i < GetSize(); i++)
	{
		DataFieldLink* pLink = GetAt(i);

		if (pLink->m_strPhysicalName.Find('.') == -1)
			pLink->m_strPhysicalName = sTableName + '.' + pLink->m_strPhysicalName;
	}
}

//============================================================================
IMPLEMENT_DYNAMIC(DataFieldLinkArrays, Array)

DataFieldLink* DataFieldLinkArrays::Find(const CString& sName) const
{
	for (int i = 0; i < GetSize(); i++)
	{
		DataFieldLink* pLink = GetAt(i)->Find(sName);
		if (pLink)
			return pLink;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
void DataFieldLinkArrays::Copy(const DataFieldLinkArrays& src)
{
	for (int i = 0; i < src.GetSize(); i++)
	{
		DataFieldLinkArray* pLinks = src.GetAt(i);

		DataFieldLinkArray* pNew = new DataFieldLinkArray(*pLinks);

		Add(pNew);
	}
}

//-----------------------------------------------------------------------------
int DataFieldLinkArrays::GetFieldLinkCount() const
{
	int c = 0;
	for (int i = 0; i < GetSize(); i++)
	{
		DataFieldLinkArray* pLinks = GetAt(i);

		c += pLinks->GetSize();
	}
	return c;
}

//=============================================================================
int CompareSqlColumnInfo(CObject* po1, CObject* po2)
{
	SqlColumnInfo* p1 = (SqlColumnInfo*)po1;
	SqlColumnInfo* p2 = (SqlColumnInfo*)po2;

	return p1->GetColumnName().CompareNoCase(p2->GetColumnName());
}

int CompareSqlCatalogEntry(CObject* po1, CObject* po2)
{
	SqlCatalogEntry* p1 = (SqlCatalogEntry*)po1;
	SqlCatalogEntry* p2 = (SqlCatalogEntry*)po2;

	return p1->m_strTableName.CompareNoCase(p2->m_strTableName);
}

//=============================================================================
int CompareModuleTables(CObject* po1, CObject* po2)
{
	CHelperSqlCatalog::CModuleTables* p1 = (CHelperSqlCatalog::CModuleTables*)po1;
	CHelperSqlCatalog::CModuleTables* p2 = (CHelperSqlCatalog::CModuleTables*)po2;

	return p1->m_sTitle.CompareNoCase(p2->m_sTitle);
}

//-----------------------------------------------------------------------------
int CompareTableColumns(CObject* po1, CObject* po2)
{
	CHelperSqlCatalog::CTableColumns* p1 = (CHelperSqlCatalog::CTableColumns*)po1;
	CHelperSqlCatalog::CTableColumns* p2 = (CHelperSqlCatalog::CTableColumns*)po2;

	return p1->m_pCatalogEntry->m_strTableName.CompareNoCase(p2->m_pCatalogEntry->m_strTableName);
}

//-----------------------------------------------------------------------------
int CompareForeignKeyTables(CObject* po1, CObject* po2)
{
	CHelperSqlCatalog::CTableForeignTables* p1 = (CHelperSqlCatalog::CTableForeignTables*)po1;
	CHelperSqlCatalog::CTableForeignTables* p2 = (CHelperSqlCatalog::CTableForeignTables*)po2;

	return p1->m_sForeignTableName.CompareNoCase(p2->m_sForeignTableName);
}

/////////////////////////////////////////////////////////////////////////////
//						CHelperSqlCatalog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CHelperSqlCatalog, CObject)

//-----------------------------------------------------------------------------
CHelperSqlCatalog::CModuleTables::CModuleTables(AddOnModule* pModule, CString sTitle)
	: 
	m_pModule(pModule), m_sTitle(sTitle)
{
	m_arModTables.SetCompareFunction(::CompareTableColumns);
	m_arModTables.SetOwns(FALSE);
}

//-----------------------------------------------------------------------------
CHelperSqlCatalog::CTableColumns::CTableColumns(const SqlCatalogEntry* pCatalogEntry)
	: 
	m_pCatalogEntry	(pCatalogEntry)
{
	m_arSortedColumns.SetCompareFunction(::CompareSqlColumnInfo);
	m_arSortedColumns.SetOwns(FALSE);
	m_arForeignTables.SetOwns(TRUE);
	m_arForeignTables.SetCompareFunction(::CompareForeignKeyTables);

}

//-----------------------------------------------------------------------------
CHelperSqlCatalog::CHelperSqlCatalog() 
{
	m_arModules.SetCompareFunction(::CompareModuleTables);

	m_arAllTables.SetCompareFunction(::CompareTableColumns);

	m_arExternalTables.SetCompareFunction(::CompareTableColumns);
	m_arExternalTables.SetOwns(FALSE);
}

//-----------------------------------------------------------------------------
void CHelperSqlCatalog::FillTable(const SqlCatalogEntry* pCatalogEntry, CModuleTables* pModT)
{
	if (!pCatalogEntry)
		return;

	if  (pCatalogEntry->m_nType == VIRTUAL_TYPE || pCatalogEntry->m_nType == PROC_TYPE)
		return;

	ASSERT_VALID(pCatalogEntry->m_pTableInfo);
	if (!pCatalogEntry->m_pTableInfo)
		return;

	CTableColumns* pTC = new CTableColumns(pCatalogEntry);
	this->m_arAllTables.Add(pTC);

	if (pModT)
		pModT->m_arModTables.Add(pTC);
	else
		this->m_arExternalTables.Add(pTC);

	const SqlRecord* pRec = pCatalogEntry->m_pTableInfo ? pCatalogEntry->m_pTableInfo->GetMasterRec() : NULL;
	if (pRec)
	{
		for (int i = 0; i < pRec->GetSizeEx(); i++)
		{
			SqlRecordItem* pSqlRecField = pRec->GetAt(i);
			ASSERT_VALID(pSqlRecField);
			const SqlColumnInfo* pCol = pSqlRecField->GetColumnInfo();
			ASSERT_VALID(pCol);

			if (!pCol || pCol->m_bVirtual)
				continue;

			pTC->m_arSortedColumns.Add(const_cast<SqlColumnInfo*>(pCol));
		}
	}
	
	if (pCatalogEntry->m_pTableInfo)
	{
		for (int i = 0; i < pCatalogEntry->m_pTableInfo->GetSizePhisycalColumns(); i++)
		{
			const SqlColumnInfo* pCol = pCatalogEntry->m_pTableInfo->GetAt(i);
			ASSERT_VALID(pCol);
			if (!pCol || pCol->m_bVirtual)
				continue;
			if (pRec && pRec->GetIndexFromColumnName(pCol->GetColumnName()) > -1)
				continue;

			pTC->m_arSortedColumns.Add(const_cast<SqlColumnInfo*>(pCol));

			if (pRec)
				TRACE2("Colonna non 'bindata' nel sqlrecord %s : %s\n", (LPCTSTR)CString(pRec->GetRuntimeClass()->m_lpszClassName), (LPCTSTR) pCol->GetQualifiedColumnName());
			else
				TRACE1("Colonna di tabella non registrata: %s\n", (LPCTSTR)pCol->GetQualifiedColumnName());
		}
	}

	pTC->m_arSortedColumns.QuickSort();
}

//-----------------------------------------------------------------------------
void CHelperSqlCatalog::Load()
{
	CMapStringToPtr mapModules;

	for (int a = 0; a <= AfxGetAddOnAppsTable()->GetUpperBound(); a++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(a);
		if (!pAddOnApp || !pAddOnApp->m_pAddOnModules)
			continue;

		for (int m = 0; m <= pAddOnApp->m_pAddOnModules->GetUpperBound(); m++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(m);
			if (!pAddOnMod || pAddOnMod->GetDatabaseRelease() == -1)
				continue;

			CString sModTitle = pAddOnMod->GetAppModTitle();

			CModuleTables* pModT = new CModuleTables(pAddOnMod, sModTitle);
			
			this->m_arModules.Add(pModT);

			CString sModKey = pAddOnMod->GetApplicationName() + '.' + pAddOnMod->GetModuleName();
			sModKey.MakeUpper();
			mapModules.SetAt(sModKey, pModT);
		}
	}

	//---------------
	SqlCatalogConstPtr pCatalog = AfxGetDefaultSqlConnection()->GetCatalog();

	CString		key;
	const SqlCatalogEntry* pCatalogEntry;

	for (POSITION pos = pCatalog->GetStartPosition(); pos != NULL;)
	{
		pCatalog->GetNextAssoc(pos, key, (CObject*&)pCatalogEntry);
		
		CModuleTables* pModT = NULL;

		const CDbObjectDescription* pDbObjectDescr = AfxGetDbObjectDescription(pCatalogEntry->m_strTableName);
		if (pDbObjectDescr)
		{
			CString sApp = pDbObjectDescr->GetNamespace().GetApplicationName();
			CString sMod = pDbObjectDescr->GetNamespace().GetModuleName();

			CString sModKey = sApp + '.' + sMod;
			sModKey.MakeUpper();

			mapModules.Lookup(sModKey, (void*&)(pModT));
		}

		FillTable(pCatalogEntry, pModT);

	}

	for (int i = this->m_arModules.GetUpperBound(); i >= 0; i--)
	{
		CModuleTables* pModT = dynamic_cast<CModuleTables*>(this->m_arModules.GetAt(i));
		ASSERT_VALID(pModT);
		if (pModT->m_arModTables.GetCount())
		{
			pModT->m_arModTables.QuickSort();
		}
		else
			this->m_arModules.RemoveAt(i);
	}

	m_arAllTables.QuickSort();
	m_arExternalTables.QuickSort();
	m_arModules.QuickSort();
}

//-----------------------------------------------------------------------------
CHelperSqlCatalog::CTableColumns* CHelperSqlCatalog::FindEntryByName(const SqlCatalogEntry* pCatalogEntry)
{
	CTableColumns  aTC(pCatalogEntry);

	int idx = m_arAllTables.BinarySearch(&aTC);
	if (idx < 0)
		return NULL;

	return dynamic_cast<CTableColumns*>(m_arAllTables.GetAt(idx));
}

//-----------------------------------------------------------------------------
CHelperSqlCatalog::CTableColumns* CHelperSqlCatalog::FindEntryByName(const CString& sTableName)
{
	SqlCatalogEntry* pDummy = new SqlCatalogEntry(sTableName);
	CTableColumns  aTC(pDummy);

	int idx = m_arAllTables.BinarySearch(&aTC);
	delete pDummy;
	if (idx < 0)
		return NULL;

	CHelperSqlCatalog::CTableColumns* pTC = dynamic_cast<CTableColumns*>(m_arAllTables.GetAt(idx));

	return pTC;
}

//-----------------------------------------------------------------------------
CHelperSqlCatalog::CModuleTables* CHelperSqlCatalog::FindModuleByTitle(AddOnModule* pMod)
{
	CString sTitle = pMod->GetAppModTitle();

	CModuleTables  aMT(pMod, sTitle);

	int idx = m_arModules.BinarySearch(&aMT);
	if (idx < 0)
		return NULL;

	return dynamic_cast<CModuleTables*>(m_arModules.GetAt(idx));
}

//-----------------------------------------------------------------------------
Array& CHelperSqlCatalog::CTableColumns::GetForeignKeys() 
{ 
	if (!m_bForeignTablesLoaded) 
	{ 
		LoadForeignKeys(); 

		m_arForeignTables.QuickSort();

		m_bForeignTablesLoaded = TRUE; 
	} 
	return m_arForeignTables; 
}

//-----------------------------------------------------------------------------
BOOL CHelperSqlCatalog::CTableColumns::LoadForeignKeys()
{
	SqlForeignKeysReader aFKReader;
	CString str;
	m_mapForeignTables.RemoveAll();


	CString sDummyTableName, sColumnName, sForeignTableName, sForeignColumnName;//per aFKReader.GetForeignKey
	CString sUpperForeignTableName;

	aFKReader.LoadForeignKeys
	(
		m_pCatalogEntry->m_strTableName,
		_T(""),
		AfxGetDefaultSqlSession(),
		TRUE//carica tutte le tabelle foreign di pCatalogEntry->m_strTableName
	);
	if (aFKReader.GetSize())
	{
		// cicla sull'array di stringhe e crea gli oggetti 
		for (int i = 0; i < aFKReader.GetSize(); i++)
		{
			CHelperSqlCatalog::CTableForeignTables*		pTable = NULL;
			CHelperSqlCatalog::CTableForeignTablesKeys*	pTableKeys = NULL;

			// scompone la stringa ForeignTable.ForeignColumn;Table.Column nei suoi componenti
			aFKReader.GetForeignKey(i, sForeignTableName, sForeignColumnName, sDummyTableName, sColumnName);
			// maiuscolo del nome tabella foreign per la mappa

			AddSquareWhenNeeds(sForeignTableName, (LPCTSTR)sForeignTableName);
			AddSquareWhenNeeds(sForeignColumnName, (LPCTSTR)sForeignColumnName);
			AddSquareWhenNeeds(sColumnName, (LPCTSTR)sColumnName);

			sUpperForeignTableName = sForeignTableName;
			sUpperForeignTableName.MakeUpper();
			// se tabella foreign già caricata salta altrimenti crea l'oggetto e lo aggiunge in array

			if (!m_mapForeignTables.Lookup(sUpperForeignTableName, (CObject*&)pTable))
			{
				pTable = new CHelperSqlCatalog::CTableForeignTables(sForeignTableName);
				m_mapForeignTables.SetAt(sUpperForeignTableName, pTable);
				m_arForeignTables.Add(pTable);
			}
			// crea un oggetto con le due colonne chiave e lo aggiunge in array
			pTableKeys = new CHelperSqlCatalog::CTableForeignTablesKeys(pTable, sColumnName, sForeignColumnName);
			pTable->m_arForeignKeys.Add(pTableKeys);
		}
	}

	/*if (AfxGetLoginInfos()->m_strDatabaseType.Find(_T("ORACLE")) < 0)
	{*/
	SqlRecordDynamic* m_poSqlRecord = new SqlRecordDynamic();
	SqlTable* m_poSqlTable = new SqlTable(AfxGetDefaultSqlSession());

	CString sTableName(m_pCatalogEntry->m_strTableName);
	sTableName.Remove('['); sTableName.Remove(']');

	m_poSqlTable->m_strSQL = L" SELECT tab2.name AS[table],col2.name AS[column],col1.name AS[columnlocal] ";
	m_poSqlTable->m_strSQL.Append(L"FROM sys.foreign_key_columns fkc ");
	m_poSqlTable->m_strSQL.Append(L"INNER JOIN sys.objects obj ON obj.object_id = fkc.constraint_object_id ");
	m_poSqlTable->m_strSQL.Append(L"INNER JOIN sys.tables tab1 ON tab1.object_id = fkc.parent_object_id ");
	m_poSqlTable->m_strSQL.Append(L"INNER JOIN sys.schemas sch ON tab1.schema_id = sch.schema_id	");
	m_poSqlTable->m_strSQL.Append(L"INNER JOIN sys.columns col1 ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id ");
	m_poSqlTable->m_strSQL.Append(L"INNER JOIN sys.tables tab2 ON tab2.object_id = fkc.referenced_object_id ");
	m_poSqlTable->m_strSQL.Append(L"INNER JOIN sys.columns col2 ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id ");
	m_poSqlTable->m_strSQL.Append(L"where  tab1.name ='" + sTableName + L"'");

	DataStr* tbl = new DataStr(L"");
	DataStr* col = new DataStr(L"");
	DataStr* colLocal = new DataStr(L"");
	sForeignTableName.Empty();

	col->SetAllocSize(500);
	tbl->SetAllocSize(500);
	colLocal->SetAllocSize(500);

	m_poSqlTable->Select(_T("table"), tbl);
	m_poSqlTable->Select(_T("column"), col);
	m_poSqlTable->Select(_T("columnlocal"), colLocal);

	TRY
	{
		m_poSqlTable->Open();
		m_poSqlTable->SetDatabaseQuery(TRUE);
		m_poSqlTable->Query();

		while (!m_poSqlTable->IsEOF())
		{
			CHelperSqlCatalog::CTableForeignTables*		pTable = NULL;
			CHelperSqlCatalog::CTableForeignTablesKeys*	pTableKeys = NULL;

			AddSquareWhenNeeds(sForeignTableName, (LPCTSTR)tbl->GetString());
			sUpperForeignTableName = sForeignTableName;
			sUpperForeignTableName.MakeUpper();

			// se tabella foreign già caricata salta altrimenti crea l'oggetto e lo aggiunge in array
			if (!m_mapForeignTables.Lookup(sUpperForeignTableName, (CObject*&)pTable))
			{
				pTable = new CHelperSqlCatalog::CTableForeignTables(sForeignTableName);
				m_mapForeignTables.SetAt(sUpperForeignTableName, pTable);
				m_arForeignTables.Add(pTable);
			}
			// crea un oggetto con le due colonne chiave e lo aggiunge in array
			pTableKeys = new CHelperSqlCatalog::CTableForeignTablesKeys(pTable, colLocal->GetString(), col->GetString());
			AddSquareWhenNeeds(colLocal->GetString()), AddSquareWhenNeeds(col->GetString());
			pTable->m_arForeignKeys.Add(pTableKeys);

			m_poSqlTable->MoveNext();
		}

			m_poSqlTable->Close();
	}
	CATCH(SqlException, e)
	{	
	}
	END_CATCH

	SAFE_DELETE(tbl);
	SAFE_DELETE(col);
	SAFE_DELETE(colLocal);
		

	return m_arForeignTables.GetSize();
}

//-----------------------------------------------------------------------------
Array& CHelperSqlCatalog::GetForeignKeys(CHelperSqlCatalog::CTableColumns* pTC)
{
	return pTC->GetForeignKeys();
}

//-----------------------------------------------------------------------------
Array* CHelperSqlCatalog::GetForeignKeys(const CString& sTableName)
{
	CHelperSqlCatalog::CTableColumns* pTC = FindEntryByName(sTableName);
	ASSERT_VALID(pTC);
	if (!pTC)
	{
		return NULL;
	}

	return & pTC->GetForeignKeys();
}


//=============================================================================