#include "stdafx.h"

#include "SqlSchemaInfo.h"


// Default Precision e Scale per i tipi SQL_DECIMAL e SQL_NUMERIC
//-----------------------------------------------------------------------------
#define LONG_PRECISION	10
#define LONG_SCALE		0
#define INT_PRECISION	6
#define INT_SCALE		0
#define ENUM_PRECISION	10
#define ENUM_SCALE		0
#define BIT_PRECISION	1
#define BIT_SCALE		0

#define DBL_PRECISION	19
#define DBL_SCALE		4
#define MONEY_PRECISION	19
#define MONEY_SCALE		4
#define QTA_PRECISION	10
#define QTA_SCALE		3
#define PERC_PRECISION	7
#define PERC_SCALE		2




//////////////////////////////////////////////////////////////////////////////
//							SqlTablesItem
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
SqlTablesItem::SqlTablesItem()
{
	m_strQualifier = _T("");
	m_strOwner = _T("");
	m_strName = _T("");
	m_strType = _T("");
	m_strRemarks = _T("");

	m_arColumnsInfo.SetOwns(FALSE);
	m_arProcedureParams.SetOwns(FALSE);
}

////-----------------------------------------------------------------------------
//SqlTablesItem::SqlTablesItem(const SqlTablesItem& cf)
//{
//	m_strQualifier = cf.m_strQualifier;
//	m_strOwner = cf.m_strOwner;
//	m_strName = cf.m_strName;
//	m_strType = cf.m_strType;
//	m_strRemarks = cf.m_strRemarks;	
//	
//}

//////////////////////////////////////////////////////////////////////////////
//							SqlColumnInfoObject
//////////////////////////////////////////////////////////////////////////////
//

//-----------------------------------------------------------------------------
SqlColumnInfoObject::SqlColumnInfoObject()
	:	
	m_bVirtual(FALSE),
	m_bVisible(true),
	m_DataObjType(DATA_NULL_TYPE),
	m_bUseCollationCulture(FALSE),

	m_bDataObjInfoUpdated(false),
	m_bLoadedFromDB(FALSE),

	m_nSqlDataType(0),
	m_lPrecision(0),
	m_lLength(0),
	m_nScale(0),
	m_nDecimal(0),
	m_nRadix(0),

	m_bSpecial(false),
	m_bIndexed(FALSE),
	m_bAutoIncrement(false),
	m_bNullable(FALSE),
	m_bNativeColumnExpr(FALSE),

	m_RuntimeClass(NULL)
#ifdef _DEBUG
	, m_pOwnerSqlRecordClass(NULL)
#endif
{
}


// costruttore utile per gestire le colonne vituali (cioe' presenti nel record
// ma non nel database). Il tipo usato per il database e' uno dei possibili per
// le conversioni canoniche
//-----------------------------------------------------------------------------
SqlColumnInfoObject::SqlColumnInfoObject
(
	const	CString&		strTableName,
	const	CString&		strColumnName,
	const	DataObj&		aDataObj
)
	:
	m_bVirtual(TRUE),
	m_bVisible(false),
	m_DataObjType(aDataObj.GetDataType()),
	m_bUseCollationCulture(aDataObj.IsCollateCultureSensitive()),

	m_strTableName(strTableName),
	m_strColumnName(strColumnName),

	m_bDataObjInfoUpdated(false),
	m_bLoadedFromDB(FALSE),

	m_nSqlDataType(0),
	m_lPrecision(0),
	m_lLength(0),
	m_nScale(0),
	m_nDecimal(0),
	m_nRadix(0),

	m_bSpecial(false),
	m_bIndexed(FALSE),
	m_bAutoIncrement(false),
	m_bNullable(FALSE),
	m_bNativeColumnExpr(FALSE),

	m_RuntimeClass(NULL)
#ifdef _DEBUG
	, m_pOwnerSqlRecordClass(NULL)
#endif
{
}

//-----------------------------------------------------------------------------
SqlColumnInfoObject::SqlColumnInfoObject(const SqlColumnInfoObject& cf)
{
	m_strTableCatalog = cf.m_strTableCatalog;
	m_strTableSchema = cf.m_strTableSchema;
	m_strTableName = cf.m_strTableName;
	m_strColumnName = cf.m_strColumnName;
	m_strRemarks = cf.m_strRemarks;

	m_nSqlDataType = cf.m_nSqlDataType;
	m_strSqlDataType = cf.m_strSqlDataType;
	m_lPrecision = cf.m_lPrecision;
	m_lLength = cf.m_lLength;
	m_nScale = cf.m_nScale;
	m_nDecimal = cf.m_nDecimal;
	m_nRadix = cf.m_nRadix;
	m_bLoadedFromDB = cf.m_bLoadedFromDB;
	m_bNullable = cf.m_bNullable;
	m_bSpecial = cf.m_bSpecial;

	m_DataObjType = cf.m_DataObjType;
	m_bUseCollationCulture = cf.m_bUseCollationCulture;
	m_bVirtual = cf.m_bVirtual;
	m_bVisible = cf.m_bVisible;
	m_bIndexed = cf.m_bIndexed;
	m_bAutoIncrement = cf.m_bAutoIncrement;
	m_bNativeColumnExpr = cf.m_bNativeColumnExpr;
	m_bDataObjInfoUpdated = cf.m_bDataObjInfoUpdated;
}


//-----------------------------------------------------------------------------
BOOL SqlColumnInfoObject::IsEqual(const SqlColumnInfoObject& cf) const
{
	return
		m_strTableCatalog.CompareNoCase(cf.m_strTableCatalog) == 0 &&
		m_strTableSchema.CompareNoCase(cf.m_strTableSchema) == 0 &&
		m_strTableName.CompareNoCase(cf.m_strTableName) == 0 &&
		m_strColumnName.CompareNoCase(cf.m_strColumnName) == 0 &&
		m_strRemarks.CompareNoCase(cf.m_strRemarks) == 0 &&
		m_strSqlDataType.CompareNoCase(cf.m_strSqlDataType) == 0 &&	

		m_nSqlDataType == cf.m_nSqlDataType &&
		m_lPrecision == cf.m_lPrecision &&
		m_lLength == cf.m_lLength &&
		m_nScale == cf.m_nScale &&
		m_nDecimal == cf.m_nDecimal &&
		m_nRadix == cf.m_nRadix &&

		m_DataObjType == cf.m_DataObjType &&
		m_bUseCollationCulture == cf.m_bUseCollationCulture &&

		m_bVirtual == cf.m_bVirtual &&
		m_bVisible == cf.m_bVisible &&
		m_bSpecial == cf.m_bSpecial &&
		m_bIndexed == cf.m_bIndexed &&
		m_bAutoIncrement == cf.m_bAutoIncrement &&
		m_bNativeColumnExpr == cf.m_bNativeColumnExpr &&

		m_bDataObjInfoUpdated == cf.m_bDataObjInfoUpdated &&
		m_bLoadedFromDB == cf.m_bLoadedFromDB;
}


//-----------------------------------------------------------------------------------------
#ifdef _DEBUG
void SqlColumnInfoObject::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " SqlColumnInfo\n");
	dc << "\tTable Name = " << m_strTableName << "\n";
	dc << "\tColumn Name = " << m_strColumnName << "\n";

	__super::Dump(dc);
}
#endif

//////////////////////////////////////////////////////////////////////////////
//							SqlProcedureParamInfo Definition
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
SqlProcedureParamInfo::SqlProcedureParamInfo()
	:
	m_bDataObjInfoUpdated(false)
{
}

//-----------------------------------------------------------------------------
SqlProcedureParamInfo::SqlProcedureParamInfo(const SqlProcedureParamInfo& aProcParamInfo)
{
	m_strProcCatalog = aProcParamInfo.m_strProcCatalog;
	m_strProcSchema = aProcParamInfo.m_strProcSchema;
	m_strProcName = aProcParamInfo.m_strProcName;
	m_strParamName = aProcParamInfo.m_strParamName;
	m_nOrdinalPosition = aProcParamInfo.m_nOrdinalPosition;
	m_nType = aProcParamInfo.m_nType;
	m_bHasDefault = aProcParamInfo.m_bHasDefault;
	m_strDefault = aProcParamInfo.m_strDefault;
	m_bIsNullable = aProcParamInfo.m_bIsNullable;
	m_nSqlDataType = aProcParamInfo.m_nSqlDataType;
	m_strSqlDataType = aProcParamInfo.m_strSqlDataType;
	m_lLength = aProcParamInfo.m_lLength;
	m_lPrecision = aProcParamInfo.m_lPrecision;
	m_nScale = aProcParamInfo.m_nScale;
	m_strDescription = aProcParamInfo.m_strDescription;
	m_bDataObjInfoUpdated = aProcParamInfo.m_bDataObjInfoUpdated;
}

// Aggiorna i data menbri sulla base del tipo di DataObj a cui e' collegato
//-----------------------------------------------------------------------------
void SqlProcedureParamInfo::UpdateDataObjInfo(DataObj* pDataObj)
{
	pDataObj->SetAllocSize(m_lLength);
}


// Aggiorna i data menbri sulla base del tipo di DataObj a cui e' collegato
//-----------------------------------------------------------------------------
void SqlProcedureParamInfo::UpdateResultValueType(DataObj* pDataObj)
{
	ASSERT_VALID(pDataObj);

	if (m_bDataObjInfoUpdated)
		return;

	m_bDataObjInfoUpdated = true;

	DataType resDataObjType = pDataObj->GetDataType();

	//questo switch serve solo per i campi locali utilizzati per le query
	switch (resDataObjType.m_wType)
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

			resDataObjType.m_wTag = ((DataEnum*)pDataObj)->GetTagValue();

	#ifdef _DEBUG
			if (resDataObjType.m_wTag == 0)
			{
				ASSERT_TRACE2
				(
					FALSE,
					"SqlProcedureParamInfo::UpdateResultValueType: the column %s.%s has an invalid DataEnum type : tag is 0\n",
					(LPCTSTR)m_strParamName, (LPCTSTR)m_strParamName
				);
			}
	#endif
			break;

		case DATA_BOOL_TYPE:
			m_nSqlDataType = DBTYPE_STR;
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

		default:
			ASSERT_TRACE2(FALSE,
				"SqlProcedureParamInfo::UpdateResultValueType: the column %s.%s has an invalid datatype\n",
				(LPCTSTR)m_strParamName, (LPCTSTR)m_strParamName
			);
			break;
		}
}
#ifdef _DEBUG
void SqlProcedureParamInfo::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, " SqlProcedureParamInfo\n");
	dc << "\tProcedure Name = " << m_strProcName << "\n";
	dc << "\tParameter Name = " << m_strParamName << "\n";

	CObject::Dump(dc);
}
#endif