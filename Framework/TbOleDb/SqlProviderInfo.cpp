
#include "stdafx.h"

#include <atldbcli.h>

#include <TbNameSolver\TbNamespaces.h>
#include <TbNameSolver\LoginContext.h>

#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>
#include <TbGeneric\LocalizableObjs.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\globals.h>
#include <TbGenlib\generic.h>

#include "sqlproviderinfo.h"
#include "sqlcatalog.h"
#include "sqlconnect.h"
#include "oledbmng.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

// Varie ed eventuali
//-----------------------------------------------------------------------------
static const TCHAR szNativeSqlTimestamp[]	= _T("{ts '%04d-%02d-%02d %02d:%02d:%02d'}");
static const TCHAR szNativeSqlDate[]		= _T("{d '%04d-%02d-%02d'}");
static const TCHAR szNativeSqlTime[]		= _T("{t '%02d:%02d:%02d'}");

static const TCHAR szNativeOracleTimestamp[]	= _T("TO_DATE('%04d-%02d-%02d %02d:%02d:%02d', 'YYYY-MM-DD HH24:MI:SS')");
static const TCHAR szNativeOracleDate[]			= _T("TO_DATE('%04d-%02d-%02d', 'YYYY-MM-DD')");
static const TCHAR szNativeOracleTime[]			= _T("TO_DATE('%02d:%02d:%02d', 'HH24:MI:SS')");

// per problema stringa vuota di Oracle
static const TCHAR szBlank[]			= _T(" ");

//------------------------------------------------------------------------------
BOOL CheckTypeCompatibility(const DataType& aDataObjType, DBTYPE eDBType) 
{
	switch (aDataObjType.m_wType)
	{
		case DATA_INT_TYPE:
			return	eDBType == DBTYPE_I1 || 
					eDBType == DBTYPE_I2 || 
					eDBType == DBTYPE_I4 || 
					eDBType == DBTYPE_I8 || 
					eDBType == DBTYPE_R4 || 
					eDBType == DBTYPE_R8 || 
					eDBType == DBTYPE_CY || 
					eDBType == DBTYPE_DECIMAL || 
					eDBType == DBTYPE_NUMERIC || 
					eDBType == DBTYPE_BOOL || 
					eDBType == DBTYPE_DATE || 
					eDBType == DBTYPE_VARIANT || 
					eDBType == DBTYPE_PROPVARIANT || 
					eDBType == DBTYPE_VARNUMERIC;	

		case DATA_LNG_TYPE:
		case DATA_ENUM_TYPE:
			return	eDBType == DBTYPE_I4 || 
					eDBType == DBTYPE_I8 || 
					eDBType == DBTYPE_R4 || 
					eDBType == DBTYPE_R8 || 
					eDBType == DBTYPE_CY || 
					eDBType == DBTYPE_DECIMAL || 
					eDBType == DBTYPE_NUMERIC || 
					eDBType == DBTYPE_BOOL || 
					eDBType == DBTYPE_DATE || 
					eDBType == DBTYPE_VARIANT || 
					eDBType == DBTYPE_PROPVARIANT || 
					eDBType == DBTYPE_VARNUMERIC;		

		case DATA_DBL_TYPE:
		case DATA_QTA_TYPE:
		case DATA_MON_TYPE:
		case DATA_PERC_TYPE:
			return	eDBType == DBTYPE_I8 || 
					eDBType == DBTYPE_R8 || 
					eDBType == DBTYPE_DECIMAL || 
					eDBType == DBTYPE_NUMERIC || 
					eDBType == DBTYPE_BOOL || 
					eDBType == DBTYPE_DATE || 
					eDBType == DBTYPE_VARIANT || 
					eDBType == DBTYPE_PROPVARIANT|| 
					eDBType == DBTYPE_VARNUMERIC;		
	
		case DATA_DATE_TYPE:
			return	eDBType == DBTYPE_DBTIMESTAMP ||
					eDBType == DBTYPE_DBDATE ||
					eDBType == DBTYPE_DBTIME ||
					eDBType == DBTYPE_FILETIME ||
					eDBType == DBTYPE_VARIANT;		

		case DATA_BOOL_TYPE:
		case DATA_STR_TYPE:
			 return eDBType == DBTYPE_WSTR ||
					eDBType == DBTYPE_BSTR ||
					eDBType == DBTYPE_STR  ||
					eDBType == DBTYPE_VARIANT ||
					eDBType == DBTYPE_PROPVARIANT;
			
		case DATA_GUID_TYPE:
			return eDBType == DBTYPE_GUID ||
					eDBType == DBTYPE_WSTR ||
					eDBType == DBTYPE_BSTR ||
					eDBType == DBTYPE_STR;

		case DATA_TXT_TYPE:
			return eDBType = DBTYPE_IUNKNOWN ||
				   eDBType == DBTYPE_WSTR ||
				   eDBType == DBTYPE_BSTR ||
				   eDBType == DBTYPE_STR;  

		case DATA_BLOB_TYPE:
			ASSERT(FALSE);
			return FALSE;
	}

	return FALSE;
}

//////////////////////////////////////////////////////////////////////////////
//					SqlColumnTypeItem Implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlColumnTypeItem, CObject)

//-----------------------------------------------------------------------------
SqlColumnTypeItem::SqlColumnTypeItem
	(
		const DataType&	aDataObjType,
		SWORD			nSqlDataType
	)
	:
	m_DataObjType	(aDataObjType),
	m_nSqlDataType	(nSqlDataType)
{}	

//////////////////////////////////////////////////////////////////////////////
//					SqlColumnTypeArray implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlColumnTypeArray, Array)


//////////////////////////////////////////////////////////////////////////////
//					COLEDBProviderProperties implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
COLEDBProviderProperties::COLEDBProviderProperties()
:
	m_bTxnCapable				(FALSE), 
	m_bMultiStorageObj			(FALSE), 
	m_dwTxnIsolationOptions		(DBPROPVAL_TI_CHAOS),
	m_dwIdentifierCase			(DBPROPVAL_IC_MIXED), 
	m_dwCursorCommitBehavior	(DBPROPVAL_CB_DELETE),
	m_dwCursorAbortBehavior		(DBPROPVAL_CB_DELETE)
{
}

//-----------------------------------------------------------------------------
BOOL COLEDBProviderProperties::GetProperties(SqlConnection* pSqlConnection)
{
	USES_CONVERSION;
	if (!pSqlConnection || !pSqlConnection->GetDataSource())
		return FALSE;

	const CDataSource* pDataSource = (const CDataSource*)pSqlConnection->GetDataSource();
	CComVariant var;

	if (SUCCEEDED(pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_SUPPORTEDTXNISOLEVELS, &var)))
		m_dwTxnIsolationOptions = var.ulVal;

	/*DBPROPVAL_CB_DELETE  - Aborting/Commit a transaction deletes prepared commands. 
							 The application must reprepare commands before executing them. 
	  DBPROPAL_CB_PRESERVE ?Aborting/Commit a transaction preserves prepared commands. 
							 The application can reexecute commands without repreparing them. */

	if (SUCCEEDED(pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_PREPAREABORTBEHAVIOR, &var)))
		m_dwCursorAbortBehavior = var.ulVal;

	if (SUCCEEDED(pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_PREPARECOMMITBEHAVIOR, &var)))
		m_dwCursorCommitBehavior = var.ulVal;

	if (SUCCEEDED(pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_IDENTIFIERCASE, &var)))
		m_dwIdentifierCase = var.ulVal;

	if (SUCCEEDED(pDataSource->GetProperty(DBPROPSET_DATASOURCEINFO, DBPROP_MULTIPLESTORAGEOBJECTS, &var)))
		m_bMultiStorageObj = var.boolVal;


	// controllo se il provider gestisce le transazioni
	CSession aSession;
	aSession.Open(*pDataSource);
	ATLASSERT(aSession.m_spOpenRowset != NULL);
	CComPtr<ITransactionLocal> spTransactionLocal;
	m_bTxnCapable = (aSession.m_spOpenRowset->QueryInterface(&spTransactionLocal) == S_OK);
	aSession.Close();

	// Determine if there are any literal characters that specify
	// table and column names.
	CComQIPtr<IDBInfo> spInfo(pDataSource->m_spInit);
	if (spInfo != NULL)
	{
		DBLITERAL dbLit = DBLITERAL_QUOTE;
		ULONG ulLiteralInfo = 0;
		DBLITERALINFO* pLiteralInfo = NULL;
		OLECHAR* pChar = NULL;
		
		if (SUCCEEDED(spInfo->GetLiteralInfo(1, &dbLit, &ulLiteralInfo,	&pLiteralInfo, &pChar)))
			m_strQuote = (pLiteralInfo) ? OLE2T(pLiteralInfo[0].pwszLiteralValue) : _T("");
		
		if (pLiteralInfo != NULL)
			CoTaskMemFree(pLiteralInfo);
		if (pChar != NULL)
			CoTaskMemFree(pChar);
	}	
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//					CSysAdminProviderParams implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
CSysAdminProviderParams::CSysAdminProviderParams(DBMSType eDBMSType)
:
	m_bStripTrailingSpaces	(FALSE),
	m_bUseConstParameter	(FALSE),
	m_bUseUnicode			(FALSE),
	m_eDBMSType				(eDBMSType)
{
	InitParams();
}

//-----------------------------------------------------------------------------
void CSysAdminProviderParams::InitParams()
{
	switch (m_eDBMSType)
	{
		case DBMS_ORACLE:
			m_bStripTrailingSpaces = TRUE;
			break;
		case DBMS_SQLSERVER:
			m_bUseConstParameter = TRUE;
			m_bStripTrailingSpaces = TRUE;			
			break;

		default: break;
	}
}

//-----------------------------------------------------------------------------
BOOL CSysAdminProviderParams::ReadParams(const long& nProviderID)
{
	if (nProviderID == -1)
		return TRUE;

	CString strProviderName, strProviderDescription;
    //TODO_MARCO chiedere ad anna se serve passarlo...
	//nProviderID				= AfxGetLoginInfos()->m_nProviderId();
	strProviderName			= AfxGetLoginInfos()->m_strProviderName;
	strProviderDescription	= AfxGetLoginInfos()->m_strProviderDescription;
	m_bUseConstParameter	= AfxGetLoginInfos()->m_bUseConstParameter;
	m_bStripTrailingSpaces	= AfxGetLoginInfos()->m_bStripTrailingSpaces;
	m_bUseUnicode			= AfxGetLoginInfos()->m_bUseUnicode;

	if (m_eDBMSType == DBMS_SQLSERVER)
		m_bUseConstParameter = TRUE;

	return TRUE; 
}

//////////////////////////////////////////////////////////////////////////////
//							SqlProviderInfo class
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(SqlProviderInfo, SqlObject)

//-----------------------------------------------------------------------------
SqlProviderInfo::SqlProviderInfo(LPCTSTR pszProviderName, LPCTSTR pszProviderVersion, const long& nProviderId)
	:
	m_strProviderName			(pszProviderName),
	m_strProviderVer			(pszProviderVersion),
	m_pProviderProperties		(NULL),
	m_pSysAdminParams			(NULL),
	m_bTransactions				(FALSE),
	m_bInitialized 				(FALSE),
	m_bUseUnicode				(TRUE),
	m_nProviderId				(nProviderId)
  
{
	AssignDbmsType();

	m_pSqlColumnTypeArray = new SqlColumnTypeArray;
	m_pProviderProperties = new COLEDBProviderProperties;
	m_pSysAdminParams	  =	new CSysAdminProviderParams(m_eDbmsType);
}

//-----------------------------------------------------------------------------
SqlProviderInfo::~SqlProviderInfo()
{
	SAFE_DELETE(m_pSqlColumnTypeArray);
	SAFE_DELETE(m_pProviderProperties);
	SAFE_DELETE(m_pSysAdminParams);
}

//-----------------------------------------------------------------------------
void SqlProviderInfo::AssignDbmsType()
{
	if (m_strProviderName.CompareNoCase(_T("SqlOLEDB.dll")) == 0 || m_strProviderName.CompareNoCase(_T("MSDASQL.DLL")) == 0 )
		m_eDbmsType = DBMS_SQLSERVER;
	else
	{
		if (m_strProviderName.Find(_T("OraOLEDB")) >= 0)
			m_eDbmsType = DBMS_ORACLE;
		else
		{
			m_eDbmsType = DBMS_UNKNOWN;
			ASSERT(FALSE);
		}
	}
}

	
//-----------------------------------------------------------------------------
DBMSType SqlProviderInfo::GetDBMSType() const
{
	return m_eDbmsType;
}

//-----------------------------------------------------------------------------
void SqlProviderInfo::BuildColumnTypeArraySqlServer()
{       
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_STR_TYPE,		(m_bUseUnicode) ? DBTYPE_WSTR : DBTYPE_STR)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_ENUM_TYPE,	DBTYPE_I4)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_INT_TYPE,		DBTYPE_I2)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_LNG_TYPE,		DBTYPE_I4)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_DBL_TYPE,		DBTYPE_R8)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_MON_TYPE,		DBTYPE_R8)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_QTA_TYPE,		DBTYPE_R8)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_PERC_TYPE,	DBTYPE_R8)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_DATE_TYPE,	DBTYPE_DBTIMESTAMP)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_GUID_TYPE,	DBTYPE_GUID)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_TXT_TYPE,		DBTYPE_IUNKNOWN)));	
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_BOOL_TYPE,	(m_bUseUnicode) ? DBTYPE_WSTR : DBTYPE_STR)));	
}


//-----------------------------------------------------------------------------
void SqlProviderInfo::BuildColumnTypeArrayOracle() 
{              
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_STR_TYPE,		(m_bUseUnicode) ? DBTYPE_WSTR : DBTYPE_STR)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_ENUM_TYPE,	DBTYPE_I4)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_INT_TYPE,		DBTYPE_I2)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_LNG_TYPE,		DBTYPE_I4)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_DBL_TYPE,		DBTYPE_R8)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_MON_TYPE,		DBTYPE_R8)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_QTA_TYPE,		DBTYPE_R8)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_PERC_TYPE,	DBTYPE_R8)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_DATE_TYPE,	DBTYPE_DBTIMESTAMP)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_GUID_TYPE,	DBTYPE_WSTR)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_TXT_TYPE,		DBTYPE_IUNKNOWN)));
	VERIFY(0 <= m_pSqlColumnTypeArray->Add(new SqlColumnTypeItem(DATA_BOOL_TYPE,	(m_bUseUnicode) ? DBTYPE_WSTR : DBTYPE_STR)));	
}

// a seconda del database connesso 
//-----------------------------------------------------------------------------
void SqlProviderInfo::BuildColumnTypeArray()
{              
	switch (m_eDbmsType)
	{
		case DBMS_ORACLE:
			BuildColumnTypeArrayOracle(); 
			break;
		default:
			BuildColumnTypeArraySqlServer(); 
			break;
	}
}

//-----------------------------------------------------------------------------
LPCTSTR SqlProviderInfo::GetNativeDateFormat(DBTYPEENUM eDBType) const
{
	switch(m_eDbmsType)
	{
		case DBMS_SQLSERVER:	
			switch (eDBType)
			{
				case DBTYPE_DBDATE:
					return szNativeSqlDate;
				case DBTYPE_DBTIME:
					return szNativeSqlTime;
				case DBTYPE_DBTIMESTAMP:
					return szNativeSqlTimestamp;
				default:
					ASSERT(FALSE);
					return szNativeSqlDate;
			}
		case DBMS_ORACLE: 
			switch (eDBType)
			{
				case DBTYPE_DBDATE:
					return szNativeOracleDate;
				case DBTYPE_DBTIME:
					return szNativeOracleTime;
				case DBTYPE_DBTIMESTAMP:
					return szNativeOracleTimestamp;
				default:
					ASSERT(FALSE);
					return szNativeOracleDate;
			}
	}
	ASSERT(FALSE);
	return szNativeSqlDate;
}

//-----------------------------------------------------------------------------
CString	SqlProviderInfo::NativeConvert(const DataObj* pDataObj) const
{
	// DataText before 3.0 where dealed always as unicode
	// by default I have the same behaviour (see SqlConnection::NativeConvert)
	if (pDataObj->GetDataType().m_wType == DATA_TXT_TYPE)
		NativeConvert(pDataObj, TRUE);

	return NativeConvert(pDataObj, UseUnicode());
}

//-----------------------------------------------------------------------------
CString	SqlProviderInfo::NativeConvert(const DataObj* pDataObj,  const BOOL& bUseUnicode) const
{
	const rsize_t nLen = 75;
	TCHAR szVal[nLen];
	
	switch (pDataObj->GetDataType().m_wType)
	{
		case DATA_STR_TYPE:
		{
			if (m_eDbmsType == DBMS_ORACLE && ((DataStr*)pDataObj)->IsEmpty())
				((DataStr*)pDataObj)->Assign(szBlank);

			CString str = pDataObj->Str();
			str.Replace(_T("'"), _T("''"));
			return ((bUseUnicode) ? _T("N'") : _T("'")) + str + _T("'");
		}

		case DATA_TXT_TYPE:	
		{
			if (m_eDbmsType == DBMS_ORACLE)
			{
				if (m_eDbmsType == DBMS_ORACLE && ((DataStr*)pDataObj)->IsEmpty())
					((DataStr*)pDataObj)->Assign(szBlank);
				CString str = pDataObj->Str();
				str.Replace(_T("'"), _T("''"));			
				if (str.GetLength() > 4000)
					str = str.Left(4000);
				
				if (bUseUnicode)
					return cwsprintf(_T("TO_NCLOB('%s')"), str);
				
				return cwsprintf(_T("TO_CLOB('%s')"), str);
			}
			
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
			DataEnum* pDataEnum = (DataEnum*) pDataObj;
			_stprintf_s(szVal, nLen, _T("%ld"), pDataEnum->GetValue());
			return szVal;
		}

		case DATA_DATE_TYPE:
		{
			DataDate* pDataDate = (DataDate*) pDataObj;

			// Uso il formato ISO a
			switch (GetSqlDataType (pDataObj->GetDataType()))
			{
				case DBTYPE_DBDATE:
					// {d 'aaaa-mm-gg'}
					_stprintf_s
						(
						szVal, nLen, GetNativeDateFormat(DBTYPE_DBDATE), 
						pDataDate->Year(), pDataDate->Month(), pDataDate->Day()				
						);
					break;

				case DBTYPE_DBTIME:
					// {t 'hh:mm:ss'}
					_stprintf_s
						(
						szVal, nLen, GetNativeDateFormat(DBTYPE_DBTIME), 
						pDataDate->Hour(), pDataDate->Minute(), pDataDate->Second()
						);
					break;

				case DBTYPE_DBTIMESTAMP:
					// {ts 'aaaa-mm-gg hh:mm:ss'}
					_stprintf_s
						(
						szVal, nLen, GetNativeDateFormat(DBTYPE_DBTIMESTAMP),
						pDataDate->Year(), pDataDate->Month(), pDataDate->Day(),
						pDataDate->Hour(), pDataDate->Minute(), pDataDate->Second()
						);
					break;

				default: 
					ASSERT(FALSE);
					// {d 'aaaa-mm-gg'}
					_stprintf_s
						(
						szVal, nLen, (m_eDbmsType == DBMS_ORACLE) ? szNativeOracleDate : szNativeSqlDate, 
						pDataDate->Year(), pDataDate->Month(), pDataDate->Day()
						);
					break;
			}
			return szVal;
		}
		case DATA_BOOL_TYPE:
		{
			DataBool& aDataBool = (DataBool&) *pDataObj;
			return aDataBool ? _T("'1'") : _T("'0'");
		}

		case DATA_GUID_TYPE:
		{
			DataGuid& aDataGuid = (DataGuid&) *pDataObj;
			_stprintf_s(szVal, nLen, _T("\'%s\'"), aDataGuid.Str());
			return szVal;
		}

		case DATA_BLOB_TYPE:	
			break;		
	}

	ASSERT(FALSE);		
	return _T("BadDataType");
}

//-----------------------------------------------------------------------------
DBTYPE SqlProviderInfo::GetSqlDataType (const DataType& aDataObjType) const
{
	SqlColumnTypeItem* pItem = GetSqlColumnTypeItem (aDataObjType);
	return pItem ? pItem->m_nSqlDataType : DBTYPE_WSTR;
}

//-----------------------------------------------------------------------------
SqlColumnTypeItem* SqlProviderInfo::GetSqlColumnTypeItem (const DataType& aDataObjType) const
{
	if (m_pSqlColumnTypeArray)
	{
		for (int i = 0; i <= m_pSqlColumnTypeArray->GetUpperBound(); i++)
		{
			SqlColumnTypeItem* pItem = m_pSqlColumnTypeArray->GetAt(i);
			if (pItem && pItem->m_DataObjType.m_wType == aDataObjType.m_wType)
				return pItem;
		}
	}
	
	ASSERT(FALSE);
	TRACE("Impossile the convertion from DataType %s to SqlDataType\n", ::FromDataTypeToDescr(aDataObjType));
	return NULL;
}


//-----------------------------------------------------------------------------
BOOL SqlProviderInfo::CursorPreserveBehavior(BOOL bForCommit) const
{
	return (bForCommit) 
			? CursorCommitBehavior() == DBPROPVAL_CB_PRESERVE	
			: CursorAbortBehavior() == DBPROPVAL_CB_PRESERVE;  
}

// leggo le propriet?del provider
//-----------------------------------------------------------------------------
BOOL SqlProviderInfo::LoadProviderInfo(SqlConnection* pConnection)
{
	if (m_bInitialized)
		return TRUE;

	if (!pConnection)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (!m_pProviderProperties->GetProperties(pConnection))
	{
		AfxGetLoginContext()->Lock ();
		ThrowSqlException(cwsprintf(_TB("Error reading the properties of provider {0-%s}\r\nused for connecting to database {1-%s}"), m_strProviderName, pConnection->m_strDBName));
	}
	
	if (!m_pSysAdminParams->ReadParams(pConnection->GetProviderId()))
	{
		AfxGetLoginContext()->Lock ();
		ThrowSqlException(cwsprintf(_TB("Error reading customization parameters of provider {0-%s}\r\nused for connecting to database {1-%s}"), m_strProviderName, pConnection->m_strDBName)); 
	}

	// associo il tipo database/OLEDB/DataObj
	// for DataStr and DataBool the OLEDB type is different if the database has unicode columns. 
	// For unicode columns I use DBTYPE_WSTR instead for ansi columns DBTYPE_STR
	m_bUseUnicode = pConnection->UseUnicode();
	BuildColumnTypeArray();	

	//verifico se il provider supporta le transazione
	m_bTransactions =	m_pProviderProperties->m_bTxnCapable &&
						(((m_pProviderProperties->m_dwTxnIsolationOptions & DBPROPVAL_TI_READUNCOMMITTED) == DBPROPVAL_TI_READUNCOMMITTED) ||
						 ((m_pProviderProperties->m_dwTxnIsolationOptions & DBPROPVAL_TI_READCOMMITTED) == DBPROPVAL_TI_READCOMMITTED));

	m_bInitialized = TRUE;	

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//						SqlProviderInfoPool
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNAMIC(SqlProviderInfoPool, Array)

//-----------------------------------------------------------------------------
int	SqlProviderInfoPool::GetProviderInfoIdx(const CString& strProviderName, BOOL bUseUnicode) const
{
	if (strProviderName.IsEmpty())
		return -1;

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		SqlProviderInfo* pInfo = GetAt(i);
		if (pInfo->m_strProviderName.CompareNoCase(strProviderName) == 0 && pInfo->UseUnicode() == bUseUnicode)
			return i;
	}
	return -1;
}

//-----------------------------------------------------------------------------
SqlProviderInfo* SqlProviderInfoPool::GetProviderInfo(const CString& strProviderName, BOOL bUseUnicode) const
{
	int nIdx = GetProviderInfoIdx(strProviderName, bUseUnicode);
	return (nIdx > -1) ? GetAt(nIdx) : NULL;
}

//-----------------------------------------------------------------------------
int	SqlProviderInfoPool::Add(SqlProviderInfo* pInfo) 
{
	if (!pInfo || pInfo->m_strProviderName.IsEmpty())
		return -1;
	int nIdx = GetProviderInfoIdx(pInfo->m_strProviderName, pInfo->m_bUseUnicode);
    
	return (nIdx > -1) ? nIdx : Array::Add(pInfo);
}



