
#include "stdafx.h"

#include <string.h>
#include <stdio.h>
#include <limits.h>
#include <float.h>
#include <atlenc.h>
#include <atlsafe.h>

#include <ctype.h>

#include <TbXmlCore\XmlGeneric.h>

#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\Chars.h>
#include <TbNameSolver\FileSystemFunctions.h>
#include <TbNameSolver\ThreadContext.h>

#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\MinMax.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\FormatsHelpers.h>

#include "ISqlRecord.h"
#include "DataObj.h"
#include "RdeProtocol.h"
#include "FormatsTable.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

const TCHAR* szValue = _T("value");
const TCHAR* szEnabled = _T("enabled");
const TCHAR* szTag = _T("tag");
const TCHAR* szType = _T("type");

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

const DataType  DataType::Null(DATA_NULL_TYPE, 0);
const DataType  DataType::Void(DATA_NULL_TYPE, DataObj::TB_VOID);
const DataType	DataType::String(DATA_STR_TYPE, 0);
const DataType  DataType::Integer(DATA_INT_TYPE, 0);
const DataType  DataType::Long(DATA_LNG_TYPE, 0);
const DataType  DataType::Double(DATA_DBL_TYPE, 0);
const DataType  DataType::Money(DATA_MON_TYPE, 0);
const DataType  DataType::Quantity(DATA_QTA_TYPE, 0);
const DataType  DataType::Percent(DATA_PERC_TYPE, 0);
const DataType  DataType::Date(DATA_DATE_TYPE, 0);
const DataType  DataType::DateTime(DATA_DATE_TYPE, DataObj::FULLDATE);
const DataType  DataType::Time(DATA_DATE_TYPE, DataObj::FULLDATE | DataObj::TIME);
const DataType  DataType::ElapsedTime(DATA_LNG_TYPE, DataObj::TIME);	//@@ElapsedTime
const DataType  DataType::Bool(DATA_BOOL_TYPE, 0);
const DataType  DataType::Enum(DATA_ENUM_TYPE, 0);
const DataType  DataType::Guid(DATA_GUID_TYPE, 0);
const DataType  DataType::Text(DATA_TXT_TYPE, 0);
const DataType  DataType::Blob(DATA_BLOB_TYPE, 0);
const DataType  DataType::Array(DATA_ARRAY_TYPE, 0);
const DataType  DataType::Variant(DATA_VARIANT_TYPE, 0);
const DataType  DataType::Object(DATA_LNG_TYPE, DataObj::TB_HANDLE);
const DataType  DataType::Record(DATA_RECORD_TYPE, 0);

CString DataType::ToString() const
{
	return ::FromDataTypeToScriptType(*this);
}

BOOL DataType::IsNumeric() const
{
	switch (m_wType)
	{
	case DATA_LNG_TYPE:
	{
		if (m_wTag == DataObj::TB_HANDLE)
			return FALSE;
		return TRUE;
	}
	case DATA_INT_TYPE:
	case DATA_DBL_TYPE:
	case DATA_MON_TYPE:
	case DATA_QTA_TYPE:
	case DATA_PERC_TYPE:
		return TRUE;
	}

	return FALSE;
}

BOOL DataType::IsReal() const
{
	switch (m_wType)
	{
	case DATA_DBL_TYPE:
	case DATA_MON_TYPE:
	case DATA_QTA_TYPE:
	case DATA_PERC_TYPE:
		return TRUE;
	}

	return FALSE;
}

const DataLng   DataLng::MINVALUE(DATA_LNG_MINVALUE);
const DataLng   DataLng::MAXVALUE(DATA_LNG_MAXVALUE);

const DataDate  DataDate::NULLDATE(MIN_DAY, MIN_MONTH, MIN_YEAR);
const DataDate  DataDate::MINVALUE(MIN_GIULIAN_DATE);
const DataDate  DataDate::MAXVALUE(MAX_GIULIAN_DATE);

DataInt   DataInt::MINVALUE(DATA_INT_MINVALUE);
DataInt   DataInt::MAXVALUE(DATA_INT_MAXVALUE);

const DataDbl   DataDbl::MINVALUE(DATA_DBL_MINVALUE);
const DataDbl   DataDbl::MAXVALUE(DATA_DBL_MAXVALUE);

//----------------------------------------------------------------------------
static const TCHAR szDataStr[] = _T("DataStr");
static const TCHAR szDataLng[] = _T("DataLng");
static const TCHAR szDataDate[] = _T("DataDate");
static const TCHAR szDataInt[] = _T("DataInt");
static const TCHAR szDataBool[] = _T("DataBool");
static const TCHAR szDataDbl[] = _T("DataDbl");
static const TCHAR szDataMon[] = _T("DataMon");
static const TCHAR szDataQta[] = _T("DataQty"); //DataQta
static const TCHAR szDataPerc[] = _T("DataPerc");
static const TCHAR szDataEnum[] = _T("DataEnum");
static const TCHAR szDataGuid[] = _T("DataGuid");
static const TCHAR szDataTxt[] = _T("DataTxt");
static const TCHAR szDataBlob[] = _T("DataBlob");
static const TCHAR szSeparator[] = _T("::");
static const TCHAR szDataArray[] = _T("DataArray");
static const TCHAR szDataRecord[] = _T("DataRecord");

//////////////////////////////////////////////////////////////////////////////
//  COMMON FUNCTION 
//
//////////////////////////////////////////////////////////////////////////////

#ifdef _DEBUG
#undef DataObjCreate
DataObj* DataObj::DataObjCreate(const DataType& aDataType, LPCSTR pszFile/*=NULL*/, int nLine/*=0*/)
#define DataObjCreate(dt) DataObjCreate(dt, THIS_FILE, __LINE__)
#else
DataObj* DataObj::DataObjCreate(const DataType& aDataType/*, LPCSTR pszFile, int nLine*/)
#endif
{
	DataObj* pObj = NULL;
	switch (aDataType.m_wType)
	{
	case DATA_STR_TYPE: pObj = new DataStr(); break;
	case DATA_INT_TYPE: pObj = new DataInt(); break;
	case DATA_LNG_TYPE:
	{
		DataLng* pLng = new DataLng();
		pLng->SetAsTime(aDataType.IsATime());	//@@ElapsedTime
		if (aDataType.IsAHandle())
			pLng->SetAsHandle(TRUE);
		pObj = pLng;  break;
	}
	case DATA_DBL_TYPE: pObj = new DataDbl(); break;
	case DATA_MON_TYPE: pObj = new DataMon(); break;
	case DATA_QTA_TYPE: pObj = new DataQty(); break;
	case DATA_PERC_TYPE: pObj = new DataPerc(); break;
	case DATA_DATE_TYPE:
	{
		DataDate* pDate = new DataDate();
		if (aDataType.IsFullDate())
			if (aDataType.IsATime())
				pDate->SetAsTime();
			else
				pDate->SetFullDate();
		pObj = pDate;
		break;
	}
	case DATA_BOOL_TYPE: pObj = new DataBool(); break;
	case DATA_ENUM_TYPE: pObj = new DataEnum(aDataType.m_wTag, 0); break;
	case DATA_GUID_TYPE: pObj = new DataGuid(); break;
	case DATA_TXT_TYPE: pObj = new DataText(); break;
	case DATA_BLOB_TYPE: pObj = new DataBlob(); break;
	case DATA_ARRAY_TYPE: pObj = new DataArray(); break;
	case DATA_RECORD_TYPE: pObj = new DataRecord(); break;
	case DATA_VARIANT_TYPE: return NULL;
	default: ASSERT_TRACE1(FALSE, "Bad type: %d", aDataType.m_wType); return NULL;
	}

#ifdef _DEBUG
	if (pszFile)
	{
		pObj->m_pNewed = new MemoryLeakTrackNew(pszFile, nLine);
	}
#endif

	return pObj;
}

//-----------------------------------------------------------------------------
DataObj*	DataObj::Clone() const
{
#ifdef _DEBUG
#undef DataObjClone
	return DataObjClone();
#define DataObjClone() DataObjClone(THIS_FILE, __LINE__)
#else
	return DataObjClone();
#endif
}

//-----------------------------------------------------------------------------
#ifdef _DEBUG
#undef DataObjClone
DataObj* DataObj::DataObjClone(LPCSTR pszFile/*=NULL*/, int nLine/*=0*/) const
#define DataObjClone() DataObjClone(THIS_FILE, __LINE__)
#else
DataObj* DataObj::DataObjClone(/*LPCSTR pszFile, int nLine*/) const
#endif
{
	DataObj* pObj = NULL;
	switch (this->GetDataType().m_wType)
	{
	case DATA_STR_TYPE: pObj = new DataStr(*(DataStr*)this); break;
	case DATA_INT_TYPE: pObj = new DataInt(*(DataInt*)this); break;
	case DATA_LNG_TYPE: pObj = new DataLng(*(DataLng*)this);
		if (this->IsHandle())
			pObj->SetAsHandle();
		else if (this->IsATime())
			pObj->SetAsTime();
		break;
	case DATA_DBL_TYPE: pObj = new DataDbl(*(DataDbl*)this); break;
	case DATA_MON_TYPE: pObj = new DataMon(*(DataMon*)this); break;
	case DATA_QTA_TYPE: pObj = new DataQty(*(DataQty*)this); break;
	case DATA_PERC_TYPE: pObj = new DataPerc(*(DataPerc*)this); break;
	case DATA_DATE_TYPE: pObj = new DataDate(*(DataDate*)this);
		if (this->IsFullDate())
			pObj->SetFullDate();
		else if (this->IsATime())
			pObj->SetAsTime();
		break;
	case DATA_BOOL_TYPE: pObj = new DataBool(*(DataBool*)this); break;
	case DATA_ENUM_TYPE: pObj = new DataEnum(*(DataEnum*)this); break;
	case DATA_GUID_TYPE: pObj = new DataGuid(*(DataGuid*)this); break;
	case DATA_TXT_TYPE: pObj = new DataText(*(DataText*)this); break;
	case DATA_BLOB_TYPE: pObj = new DataBlob(*(DataBlob*)this); break;
	case DATA_ARRAY_TYPE: pObj = new DataArray(*(DataArray*)this); break;
	case DATA_RECORD_TYPE: pObj = new DataRecord(*(DataRecord*)this); break;
	default: return NULL;
	}
#ifdef _DEBUG
	if (pszFile)
	{
		pObj->m_pNewed = new MemoryLeakTrackNew(pszFile, nLine);
	}
#endif
	return pObj;
}

//-----------------------------------------------------------------------------
CString FromTBTypeToNetType(const DataType& aDataType)
{
	switch (aDataType.m_wType)
	{
	case DATA_STR_TYPE: return _T("String");
	case DATA_INT_TYPE: return _T("Int32");
	case DATA_LNG_TYPE: return _T("Int64");
	case DATA_DBL_TYPE: return _T("Double");
	case DATA_PERC_TYPE: return _T("Double");
	case DATA_QTA_TYPE: return _T("Double");
	case DATA_MON_TYPE: return _T("Double");
	case DATA_GUID_TYPE: return _T("String");
	case DATA_DATE_TYPE: return _T("DateTime");
	case DATA_BOOL_TYPE: return _T("Boolean");
	case DATA_ENUM_TYPE: return _T("DataEnum");
	case DATA_TXT_TYPE: return _T("String");
	case DATA_ARRAY_TYPE: { /*ASSERT(FALSE);*/ return _T("Array"); }
	case DATA_RECORD_TYPE: { /*ASSERT(FALSE);*/ return _T("Record"); }
	case DATA_VARIANT_TYPE: { /*ASSERT(FALSE);*/ return _T("Variant"); }
							//case DATA_OBJECT_TYPE	: { /*ASSERT(FALSE);*/ return _T("Object");}
	case DATA_BLOB_TYPE: { ASSERT_TRACE(FALSE, "Blob type not allowed here"); return _T("Blob"); }
	default: return _T("String");
	}
	return _T("String");
}

//-----------------------------------------------------------------------------
CString FromDataTypeToDescr(const DataType& aDataType)
{
	switch (aDataType.m_wType)
	{
	case DATA_STR_TYPE:	return _TB("String");
	case DATA_INT_TYPE:	return _TB("Integer");
	case DATA_LNG_TYPE:	return						//@@ElapsedTime
		(
			aDataType.IsATime()
			? _TB("ElapsedTime")
			: _TB("Extended")
			);
	case DATA_DBL_TYPE:	return _TB("Real");
	case DATA_MON_TYPE:	return _TB("Monetary");
	case DATA_QTA_TYPE:	return _TB("Quantity");
	case DATA_PERC_TYPE:	return _TB("Percentage");
	case DATA_DATE_TYPE:	return
		(
			!aDataType.IsFullDate()
			? _TB("Date")
			: aDataType.IsATime()
			? _TB("Time")
			: _TB("DateTime")
			);
	case DATA_BOOL_TYPE:	return _TB("Logic");
	case DATA_ENUM_TYPE:	return AfxGetEnumsTable()->GetEnumTagTitle(aDataType.m_wTag);
	case DATA_GUID_TYPE:	return _TB("Guid");
	case DATA_TXT_TYPE:	return _TB("Text");
	case DATA_ARRAY_TYPE:	return _TB("Array");
	case DATA_RECORD_TYPE:	return _TB("Record");
		//case DATA_BLOB_TYPE	:	return _TB("Blob");
	case DATA_VARIANT_TYPE:	return _TB("Variant");
		//case DATA_OBJECT_TYPE	:	return _TB("Object");
	default: { return CString(""); }
	}
}

//-----------------------------------------------------------------------------
CString FromDataTypeToScriptType(const DataType& aDataType)
{
	switch (aDataType.m_wType)
	{
	case DATA_STR_TYPE:	return _T("String");
	case DATA_INT_TYPE:	return _T("Integer");
	case DATA_LNG_TYPE:	return
		(
			aDataType.IsATime()
			? _T("ElapsedTime")
			: (aDataType.IsAHandle() ? L"Handle" : _T("Long"))
			);
	case DATA_DBL_TYPE:	return _T("Double");
	case DATA_MON_TYPE:	return _T("Money");
	case DATA_QTA_TYPE:	return _T("Quantity");
	case DATA_PERC_TYPE:	return _T("Percent");
	case DATA_DATE_TYPE:	return
		(
			!aDataType.IsFullDate()
			? _T("Date")
			: aDataType.IsATime()
			? _T("Time")
			: _T("DateTime")
			);
	case DATA_BOOL_TYPE:	return _TB("Bool");

	case DATA_ENUM_TYPE:
	{
		return cwsprintf(__T("Enum[%d] /* %s */"), aDataType.m_wTag, AfxGetEnumsTable()->GetEnumTagTitle(aDataType.m_wTag));
	}

	case DATA_GUID_TYPE:		return _T("Uuid");
	case DATA_TXT_TYPE:		return _T("Text");
	case DATA_ARRAY_TYPE:	return _T("Array");
	case DATA_RECORD_TYPE:	return _T("Record");

	case DATA_VARIANT_TYPE:	return _T("Var");

	case DATA_NULL_TYPE:
	{
		if ((aDataType.m_wTag & DataObj::TB_VOID) == DataObj::TB_VOID)
			return _T("Void");
	}
	default:	return CString("");
	}
}

//-----------------------------------------------------------------------------
CString	DataType::FormatDefaultValue() const
{
	switch (m_wType)
	{

	case DATA_MON_TYPE:
	case DATA_QTA_TYPE:
	case DATA_PERC_TYPE:
	case DATA_DBL_TYPE:
		return _T("0");

	case DATA_INT_TYPE:	return _T("0");

	case DATA_LNG_TYPE:	return
		(
			IsATime()
			? _T("0")
			: (IsAHandle() ? L"NULL" : _T("0"))
			);

	case DATA_DATE_TYPE:	return
		(
			!IsFullDate()
			? _T("{d\"\"}")
			: IsATime()
			? _T("{t\"\"}")
			: _T("{dt\"\"}")
			);
	case DATA_BOOL_TYPE:	return _T("FALSE");

	case DATA_ENUM_TYPE:
	{
		WORD wDefaultItemValue = AfxGetEnumsTable()->GetEnumDefaultItemValue(m_wTag);
		return cwsprintf(_T("{%d:%d}"), m_wTag, wDefaultItemValue);
	}

	case DATA_GUID_TYPE:        return L"\"{00000000-0000-0000-0000-000000000000}\"";

	case DATA_STR_TYPE:
	case DATA_TXT_TYPE:			return _T("\"\"");

	case DATA_ARRAY_TYPE:
		return _T("[]");
		//case DATA_RECORD_TYPE:	
		//case DATA_VARIANT_TYPE:	
		//case DATA_NULL_TYPE:

	default:	return _T("");
	}
}

//-----------------------------------------------------------------------------
int ConvertStringToDateTime(DataType aType, LPCTSTR lpszInput, DWORD* pValue1, DWORD* pValue2 /* = NULL */)
{
	if (aType == DataType::Date)
	{
		WORD wDay, wMonth, wYear;
		if (
			!::GetDD_MM_YYYY(wDay, wMonth, wYear, lpszInput) &&
			!::GetYYYYMMDD(wDay, wMonth, wYear, lpszInput)
			)
			return CONVERT_DATATIME_SYNTAX_ERROR;

		*pValue1 = ::GetGiulianDate(wDay, wMonth, wYear);
		if (pValue2) *pValue2 = 0;
		return CONVERT_DATATIME_SUCCEEDED;
	}

	if (aType == DataType::DateTime)	// use fixed format DD/MM/YYYY HH:MM:SS or YYYY-MM-DDTHH:MM:SS (standard ISO 8601 date/time format)
	{
		if (pValue2 == NULL)
		{
			ASSERT_TRACE(pValue2 != NULL, "Conversion to DateTime requires also pValue2 parameter");
			return CONVERT_DATATIME_FAILED;
		}

		DBTIMESTAMP aDateTime;

		// try DD/MM/YYYY HH:MM:SS
		if (
			!GetTimeStamp
			(
				aDateTime, lpszInput,
				CDateFormatHelper::DATE_DMY, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999, _T("/"), _T("/"),
				CDateFormatHelper::TIME_HF99, _T(":"), NULL, NULL
			)
			)
		{
			// try YYYY-MM-DDTHH:MM:SS (standard ISO 8601 date/time format)
			CString strDate(lpszInput);
			strDate.Replace(_T("T"), _T(" "));
			if (
				!GetTimeStamp
				(
					aDateTime, (LPCTSTR)strDate,
					CDateFormatHelper::DATE_YMD, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999, _T("-"), _T("-"),
					CDateFormatHelper::TIME_HF99, _T(":"), NULL, NULL
				)
				)

			{
				// try YYYY-MM-DD HH:MM:SS (T-SQL timestamp format)
				if (
					!GetTimeStamp
					(
						aDateTime, lpszInput,
						CDateFormatHelper::DATE_YMD, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999, _T("-"), _T("-"),
						CDateFormatHelper::TIME_HF99, _T(":"), NULL, NULL
					)
					)
					return CONVERT_DATATIME_SYNTAX_ERROR;
			}
		}

		*pValue1 = ::GetGiulianDate(aDateTime);
		*pValue2 = ::GetTotalSeconds(aDateTime);

		return CONVERT_DATATIME_SUCCEEDED;
	}

	if (aType == DataType::Time)	// use fixed format HH:MM:SS
	{
		if (pValue2 == NULL)
		{
			ASSERT_TRACE(pValue2 != NULL, "Conversion to Time requires also pValue2 parameter");
			return CONVERT_DATATIME_FAILED;
		}

		DBTIMESTAMP aDateTime;

		if (
			!GetTimeStamp
			(
				aDateTime, lpszInput,
				0, 0, 0, 0, _T(""), _T(""),
				CDateFormatHelper::TIME_HF99 | CDateFormatHelper::TIME_ONLY, _T(":"), NULL, NULL
			)
			)
			return CONVERT_DATATIME_SYNTAX_ERROR;

		*pValue1 = ::GetGiulianDate(MIN_TIME_DAY, MIN_TIME_MONTH, MIN_TIME_YEAR);
		*pValue2 = ::GetTotalSeconds(aDateTime);

		return CONVERT_DATATIME_SUCCEEDED;
	}

	//ElapsedTime
	if (aType == DataType::ElapsedTime)	// use fixed format D:H:M:S:F
	{
		DataLng anElapsedTime;
		anElapsedTime.SetAsTime();

		if (!GetElapsedTime
		(
			anElapsedTime,
			lpszInput,
			CElapsedTimeFormatHelper::TIME_DHMSF,
			_T(":"),
			NULL
		)
			)
			return CONVERT_DATATIME_SYNTAX_ERROR;

		*pValue1 = (long)anElapsedTime;
	}

	return CONVERT_DATATIME_SUCCEEDED;
}

//-----------------------------------------------------------------------------
double GetEpsilonForDataType(const DataType& aDataType)
{
	// login context not yet instanced, double by default 
	if (!AfxGetLoginContext())
		return pow(10.0, -EPSILON_DECIMAL);

	// Epsilon for noise in double operations that could be customized by user
	switch (aDataType.m_wType)
	{
	case DATA_MON_TYPE: return AfxGetLoginContext()->GetDataMonEpsilon();
	case DATA_QTA_TYPE: return AfxGetLoginContext()->GetDataQtyEpsilon();
	case DATA_PERC_TYPE: return AfxGetLoginContext()->GetDataPercEpsilon();
	}

	return AfxGetLoginContext()->GetDataDblEpsilon();
}

//-----------------------------------------------------------------------------
void FillCompareType(CComboBox* pCbx, const DataType& dt)
{
	int idx;
	pCbx->ResetContent();

	idx = pCbx->AddString(_TB("Equals..."));
	pCbx->SetItemData(idx, ECompareType::CMP_EQUAL);

	idx = pCbx->AddString(_TB("Does not equal..."));
	pCbx->SetItemData(idx, ECompareType::CMP_NOT_EQUAL);

	if (dt == DataType::Void)
	{
		pCbx->SetCurSel(0);
		return;
	}

	if (
		dt == DataType::Date ||
		dt == DataType::DateTime ||

		dt == DataType::Double ||
		dt == DataType::Money ||
		dt == DataType::Percent ||
		dt == DataType::Quantity ||

		dt == DataType::Integer ||
		dt == DataType::Long
		)
	{
		idx = pCbx->AddString(_TB("Is lesser then..."));
		pCbx->SetItemData(idx, ECompareType::CMP_LESSER_THEN);
		idx = pCbx->AddString(_TB("Is lesser then or equal to..."));
		pCbx->SetItemData(idx, ECompareType::CMP_LESSER_OR_EQUAL);
		idx = pCbx->AddString(_TB("Is greater then..."));
		pCbx->SetItemData(idx, ECompareType::CMP_GREATER_THEN);
		idx = pCbx->AddString(_TB("Is grater then or equal to..."));
		pCbx->SetItemData(idx, ECompareType::CMP_GREATER_OR_EQUAL);
	}

	if (dt == DataType::String)
	{
		idx = pCbx->AddString(_TB("Begins with..."));
		pCbx->SetItemData(idx, ECompareType::CMP_BEGIN_WITH);
		idx = pCbx->AddString(_TB("Ends with..."));
		pCbx->SetItemData(idx, ECompareType::CMP_END_WITH);
		idx = pCbx->AddString(_TB("Contains..."));
		pCbx->SetItemData(idx, ECompareType::CMP_CONTAINS);
		idx = pCbx->AddString(_TB("Does not contain..."));
		pCbx->SetItemData(idx, ECompareType::CMP_NOT_CONTAINS);
	}

	pCbx->SetCurSel(0);
}

int SelectCompareType(CComboBox* pCbx, ECompareType cmd)
{
	for (int i = 0; i < pCbx->GetCount(); i++)
	{
		DWORD dw = pCbx->GetItemData(i);
		if (dw == (DWORD)cmd)
		{
			pCbx->SetCurSel(i);
			return i;
		}
	}
	//if (pCbx->GetCount())
	//{
	//	pCbx->SetCurSel(0);
	//	return 0;
	//}
	return -1;
}

//////////////////////////////////////////////////////////////////////////////
//					DataType Implementation
//////////////////////////////////////////////////////////////////////////////
//
DataType::DataType(const CString& st)
{
	LONG l = _tstol(st);

	m_wType = WORD(l & 0xFFFF);
	m_wTag = WORD(l >> 16);
}

//-----------------------------------------------------------------------------
BOOL 	DataType::IsFullDate()		const
{
	return
		(m_wType == DATA_DATE_TYPE) &&
		((m_wTag & DataObj::FULLDATE) == DataObj::FULLDATE);
}

BOOL 	DataType::IsATime()		const
{
	return
		(m_wType == DATA_DATE_TYPE || m_wType == DATA_LNG_TYPE) &&	//@@ElapsedTime
		((m_wTag & DataObj::TIME) == DataObj::TIME);
}

BOOL 	DataType::IsAVoid()		const
{
	return
		(m_wType == DATA_NULL_TYPE) &&
		((m_wTag & DataObj::TB_VOID) == DataObj::TB_VOID);
}

BOOL 	DataType::IsAHandle()		const
{
	return
		(m_wType == DATA_LNG_TYPE) &&
		((m_wTag & DataObj::TB_HANDLE) == DataObj::TB_HANDLE);
}

//-----------------------------------------------------------------------------
void DataType::SetFullDate(BOOL bFullDate)
{
	ASSERT_TRACE1(m_wType == DATA_DATE_TYPE, "Wrong data type: %d", ((int)m_wType));

	m_wTag = bFullDate
		? WORD(m_wTag | DataObj::FULLDATE)
		: WORD(m_wTag & ~DataObj::FULLDATE);
}

//-----------------------------------------------------------------------------
void DataType::SetAsTime(BOOL bIsTime)
{
	ASSERT_TRACE1(m_wType == DATA_DATE_TYPE || m_wType == DATA_LNG_TYPE, "Wrong data type: %d", ((int)m_wType));	//@@ElapsedTime

	m_wTag = bIsTime
		? WORD(m_wTag | DataObj::TIME)
		: WORD(m_wTag & ~DataObj::TIME);

	if (bIsTime && m_wType == DATA_DATE_TYPE)
		SetFullDate(TRUE);
}

//----------------------------------------------------------------------------

/*static*/ BOOL DataType::IsCompatible(const DataType& fromType, const DataType& toType)
{
	if (fromType == toType)
		return TRUE;
	if (fromType == DataType::Variant)
		return TRUE;

	switch (toType.m_wType)
	{
	case DATA_LNG_TYPE:
		if (fromType == DATA_INT_TYPE)	return TRUE;
		if (fromType == DATA_LNG_TYPE)	return TRUE;	// il tag puo` contenere sottotipologie
		if (fromType == DATA_ENUM_TYPE)	return TRUE;
		break;

	case DATA_DBL_TYPE:
		if (fromType == DATA_BOOL_TYPE)	return TRUE;
		if (fromType == DATA_INT_TYPE)	return TRUE;
		if (fromType == DATA_LNG_TYPE)	return TRUE;	// il tag puo` contenere sottotipologie
		if (fromType == DATA_DBL_TYPE)	return TRUE;
		if (fromType == DATA_MON_TYPE)	return TRUE;
		if (fromType == DATA_QTA_TYPE)	return TRUE;
		if (fromType == DATA_PERC_TYPE)	return TRUE;
		break;

	case DATA_MON_TYPE:
		if (fromType == DATA_INT_TYPE)	return TRUE;
		if (fromType == DATA_LNG_TYPE)	return TRUE;	// il tag puo` contenere sottotipologie
		if (fromType == DATA_DBL_TYPE)	return TRUE;
		if (fromType == DATA_MON_TYPE)	return TRUE;
		break;

	case DATA_QTA_TYPE:
		if (fromType == DATA_INT_TYPE)	return TRUE;
		if (fromType == DATA_LNG_TYPE)	return TRUE;	// il tag puo` contenere sottotipologie
		if (fromType == DATA_DBL_TYPE)	return TRUE;
		if (fromType == DATA_MON_TYPE)	return TRUE;
		if (fromType == DATA_QTA_TYPE)	return TRUE;
		break;

	case DATA_PERC_TYPE:
		if (fromType == DATA_INT_TYPE)	return TRUE;
		if (fromType == DATA_LNG_TYPE)	return TRUE;	// il tag puo` contenere sottotipologie
		if (fromType == DATA_DBL_TYPE)	return TRUE;
		if (fromType == DATA_PERC_TYPE)	return TRUE;
		break;

	case DATA_BOOL_TYPE:
		if (fromType == DATA_INT_TYPE)	return TRUE;
		if (fromType == DATA_BOOL_TYPE)	return TRUE;
		break;

	case DATA_DATE_TYPE:
		if (fromType.m_wType == DATA_DATE_TYPE)	return TRUE;	// il tag puo` contenere sottotipologie
		break;

	case DATA_ENUM_TYPE:
		if (fromType.m_wType == DATA_ENUM_TYPE)	return TRUE;	// il tag contiene sottotipologie
		if (fromType.m_wType == DATA_LNG_TYPE)	return TRUE;	// da validare
		break;

	case DATA_STR_TYPE:
		if (fromType == DATA_TXT_TYPE)	return TRUE;
		break;
	case DATA_TXT_TYPE:
		if (fromType == DATA_STR_TYPE)	return TRUE;
		break;
	case DATA_GUID_TYPE:
		if (fromType == DATA_STR_TYPE)	return TRUE;
		break;

	case DATA_VARIANT_TYPE:	//DataArray indexer, Array_GetAt, Array_SetAt, DefaultTttribute
		return TRUE;
	} // switch

	return FALSE;
}

//------------------------------------------------------------------------------
/* static */ BOOL DataType::IsCompatible(VARTYPE vtFromType, const DataType& toType)
{
	switch (toType.m_wType)
	{
	case DATA_LNG_TYPE:
		if (
			vtFromType &
			(
				VT_UI1 | VT_UI2 | VT_UI4 | VT_UINT |
				VT_I1 | VT_I2 | VT_I4 | VT_INT
				)
			)
			return TRUE;
		break;

	case DATA_INT_TYPE:
		if (
			vtFromType &
			(VT_UI1 | VT_UI2 | VT_I1 | VT_I2)
			)
			return TRUE;
		break;

	case DATA_DBL_TYPE:
	case DATA_MON_TYPE:
	case DATA_QTA_TYPE:
	case DATA_PERC_TYPE:
		if (vtFromType & (VT_R4 | VT_R8))
			return TRUE;
		break;

	case DATA_BOOL_TYPE:
		if (vtFromType & (VT_BOOL))
			return TRUE;
		break;

	case DATA_DATE_TYPE:
		if (vtFromType & (VT_DATE))
			return TRUE;
		break;

	case DATA_STR_TYPE:
		if (vtFromType & (VT_BSTR))
			return TRUE;
		break;

	case DATA_ENUM_TYPE:
		if (vtFromType & (VT_UINT | VT_UI4))
			return TRUE;
		break;
	} // switch

	return FALSE;
}

//////////////////////////////////////////////////////////////////////////////
//					DataObjArray Implementation
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(DataObjArray, Array)

//-----------------------------------------------------------------------------
DataObjArray* DataObjArray::Clone(LPCSTR pszFile /*= NULL*/, int nLine /*= 0*/) const
{
	DataObjArray* pClone = new DataObjArray;

	for (int i = 0; i < GetSize(); i++)
		pClone->Add(GetAt(i)->DataObjClone());

	return pClone;
}

//-----------------------------------------------------------------------------
BOOL DataObjArray::IsElementEqual(CObject* po1, CObject* po2) const
{
	if (po1 == NULL && po2 == NULL)
		return TRUE;
	if (po1 == NULL || po2 == NULL)
		return FALSE;

	DataObj* pDataObj1 = (DataObj*)po1;
	DataObj* pDataObj2 = (DataObj*)po2;

	return *pDataObj1 == *pDataObj2;
}

//-----------------------------------------------------------------------------
BOOL DataObjArray::LessThen(CObject* po1, CObject* po2) const
{
	if (po1 == NULL && po2 == NULL)
	{
		return FALSE;
	}
	if (po1 == NULL || po2 == NULL)
	{
		if (m_bSortDescending)
			return po1 == NULL ? FALSE : TRUE;
		else
			return po1 == NULL ? TRUE : FALSE;
	}

	DataObj* pDataObj1 = (DataObj*)po1;
	DataObj* pDataObj2 = (DataObj*)po2;

	return m_bSortDescending ? pDataObj1->IsGreaterThan(*pDataObj2) : pDataObj1->IsLessThan(*pDataObj2);
}

//-----------------------------------------------------------------------------
int DataObjArray::Compare(CObject* po1, CObject* po2) const
{
	if (po1 == NULL && po2 == NULL)
	{
		return 0;
	}

	if (po1 == NULL || po2 == NULL)
	{
		if (m_bSortDescending)
			return po1 == NULL ? 1 : -1;
		else
			return po1 == NULL ? -1 : 1;
	}

	DataObj* pDataObj1 = (DataObj*)po1;
	DataObj* pDataObj2 = (DataObj*)po2;

	if (m_bSortDescending)
	{
		if (pDataObj1->IsGreaterThan(*pDataObj2))
			return -1;
		if (pDataObj1->IsLessThan(*pDataObj2))
			return 1;
	}
	else
	{
		if (pDataObj1->IsLessThan(*pDataObj2))
			return -1;
		if (pDataObj1->IsGreaterThan(*pDataObj2))
			return 1;
	}
	return 0;
}

//-----------------------------------------------------------------------------
DataObjArray& DataObjArray::operator= (const DataObjArray& a)
{
	if (this != &a)
	{
		RemoveAll();
		for (int i = 0; i < a.GetSize(); i++)
			Add((a.GetAt(i))->DataObjClone());
	}
	return *this;
}

//-----------------------------------------------------------------------------
BOOL DataObjArray::IsEqual(const DataObjArray& a) const
{
	if (a.GetSize() != GetSize())
		return FALSE;

	for (int i = 0; i < a.GetSize(); i++)
	{
		DataObj* o1 = GetAt(i);
		DataObj* o2 = a.GetAt(i);
		if (
			(o1 == NULL && o2 != NULL)
			||
			(o1 != NULL && o2 == NULL)
			||
			!o1->IsEqual(*o2)
			)
			return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DataObjArray::IsLessThan(const DataObjArray& a) const
{
	if (a.GetSize() != GetSize())
		return FALSE;

	for (int i = 0; i < a.GetSize(); i++)
		if (!GetAt(i) || !GetAt(i)->IsLessThan(*(a.GetAt(i))))
			return FALSE;

	return TRUE;
}

//-----------------------------------------------------------------------------
void DataObjArray::ToStringArray(CStringArray& ars) const
{
	for (int i = 0; i < GetSize(); i++)
		ars.Add(GetAt(i)->Str());
}

//-----------------------------------------------------------------------------
CString DataObjArray::ToString() const
{
	CString s('[');
	for (int i = 0; i < GetSize(); i++)
	{
		if (i) s += ',';

		s += GetAt(i)->ToString();
	}
	s += ']';
	return s;
}

//-----------------------------------------------------------------------------
void DataObjArray::Assign(CString str, DataType dt)
{
	if (str.GetLength() < 2)
		return;

	if (
		(str[0] != '(' && str[0] != '[')
		||
		(str.Right(1) != ')' && str.Right(1) != ']')
		)
		return;

	RemoveAll();

	BOOL bRound = str[0] == '(';
	str = str.Mid(1, str.GetLength() - 2);

	CStringArray ar;
	CStringArray_Split(ar, str, L",");

	for (int i = 0; i < ar.GetCount(); i++)
	{
		DataObj* pObj = DataObj::DataObjCreate(dt);

		CString s = ar[i];
		if (dt == DataType::String)
		{
			if (bRound)
				s.Trim(L"'");
			else
				s.Trim(L"\"");
		}

		pObj->Assign(s);

		Add(pObj);
	}
}

//-----------------------------------------------------------------------------
//non riesco a chiamare la 
//AfxGetDefaultSqlConnection()->NativeConvert(pData);
//CString DataObjArray::ToSqlString() const
//{
//	CString s('[');
//	for (int i = 0 ; i < GetSize() ; i++)
//	{
//		if (i) s += ',';
//
//		s += GetAt(i)->Str();
//	}
//	s += ']';
//	return s;
//}

//-----------------------------------------------------------------------------
INT_PTR DataObjArray::Append(const DataObjArray& src)
{
	INT_PTR nOldSize = m_nSize;
	for (int i = 0; i < src.GetSize(); i++)
	{
		DataObj* pObj = src[i];
		Add(pObj->Clone());
	}
	return nOldSize;
}

void DataObjArray::Copy(const DataObjArray& src)
{
	RemoveAll();

	Append(src);
}

//-----------------------------------------------------------------------------
int DataObjArray::Find(const DataObj* pObj, int nStartPos/* = 0*/, BOOL noCase/* = FALSE*/) const
{
	ASSERT_VALID(pObj);
	if (!pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
		noCase = FALSE;

	for (int i = nStartPos; i < GetSize(); i++)
	{
		DataObj* pO = GetAt(i);
		ASSERT_VALID(pO);
		if (!pO)
			break;

		if (pObj == pO)
			return i;

		if (noCase)
		{
			ASSERT_KINDOF(DataStr, pO);
			if (_tcsicmp(((DataStr*)pObj)->GetPtrString(), ((DataStr*)pO)->GetPtrString()) == 0)
				return i;
		}
		else
		{
			if (pObj->IsEqual(*pO))
				return i;
		}
	}
	return -1;
}

//-----------------------------------------------------------------------------
template <class T> void DataObjArray::CalcSum(DataObj& pValue) const
{
	T& aVal = (T&)pValue;

	aVal.Clear();
	for (int i = 0; i < GetSize(); i++)
	{
		aVal += *(T*)GetAt(i);
	}
}

//-----------------------------------------------------------------------------
template <class T> void	DataObjArray::CalcPercentages(DataObjArray& arPercentages) const
{
	arPercentages.RemoveAll();
	if (GetSize() == 0) return;
	if (GetSize() == 1) 
	{
		arPercentages.Add(new DataPerc(100.0));
		return;
	}

	arPercentages.SetSize(GetSize());

	T aSum;
	CalcSum<T>(aSum);

	double dbl = ((double) aSum ) / 100.0;

	for (int i = 0; i < GetSize(); i++)
	{
		double d = double(*(T*)GetAt(i)) * dbl;

		DataPerc* pVal = new DataPerc(d);
		arPercentages[i] = pVal;
	}
}

//-----------------------------------------------------------------------------
void DataObjArray::CalcSum(DataObj& aSum) const
{
	DataType dt = aSum.GetDataType();
	if (dt == DataType::Money)
	{
		CalcSum<DataMon>(aSum);
		return;
	}
	if (dt == DataType::Quantity)
	{
		CalcSum<DataQty>(aSum);
		return;
	}
	if (dt == DataType::Percent)
	{
		CalcSum<DataPerc>(aSum);
		return;
	}
	if (dt == DataType::Double)
	{
		CalcSum<DataDbl>(aSum);
		return;
	}
	if (dt == DataType::ElapsedTime)
	{
		CalcSum<DataLng>(aSum);
		return;
	}
	if (dt == DataType::Integer)
	{
		CalcSum<DataInt>(aSum);
		return;
	}
	if (dt == DataType::Long)
	{
		CalcSum<DataLng>(aSum);
		return;
	}

	ASSERT_TRACE1(FALSE, "Function DataArray::CalcSum not implemented on type %s\n", dt.ToString());
}

//-----------------------------------------------------------------------------
DataObj* DataObjArray::GetMinElem() const
{
	if (GetSize() == 0)
		return NULL;

	DataObj* pMin = GetAt(0);
	for (int i = 1; i < GetSize(); i++)
	{
		if (pMin > GetAt(i))
			pMin = GetAt(i);
	}
	return pMin;
}

DataObj* DataObjArray::GetMaxElem() const
{
	if (GetSize() == 0)
		return NULL;

	DataObj* pMax = GetAt(0);
	for (int i = 1; i < GetSize(); i++)
	{
		if (pMax < GetAt(i))
			pMax = GetAt(i);
	}
	return pMax;
}

//-----------------------------------------------------------------------------
#ifdef _DEBUG

void DataObjArray::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataObjArray:");
	AFX_DUMP0(dc, "\n\tContent= ");

	for (int i = 0; i < this->GetCount(); i++)
		AFX_DUMP1(dc, "\n%s", this->GetAt(i)->Str());
}

void DataObjArray::AssertValid() const
{
	__super::AssertValid();
}
#endif //_DEBUG
//-----------------------------------------------------------------------------


//////////////////////////////////////////////////////////////////////////////
//					DataObjArrayOfArray Implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(DataObjArrayOfArray, Array)

DataObjArrayOfArray& DataObjArrayOfArray::operator= (const DataObjArrayOfArray& a)
{
	if (this != &a)
	{
		RemoveAll();
		for (int i = 0; i < a.GetSize(); i++)
			Add((a.GetAt(i))->Clone());
	}
	return *this;
}

//-----------------------------------------------------------------------------
BOOL DataObjArrayOfArray::IsEqual(const DataObjArrayOfArray& a) const
{
	if (a.GetSize() != GetSize())
		return FALSE;

	for (int i = 0; i < a.GetSize(); i++)
		if (!GetAt(i) || !GetAt(i)->IsEqual(*(a.GetAt(i))))
			return FALSE;

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//					DataStrArray Implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(DataStrArray, DataObjArray)

//-----------------------------------------------------------------------------
void DataStrArray::operator= (const DataStrArray& aSelBox)
{
	if (GetSize() > 0)
		RemoveAll();

	for (int i = 0; i < aSelBox.GetSize(); i++)
	{
		ASSERT_TRACE1(aSelBox.GetAt(i), "Failed selecting element %d", i);
		Add(new DataStr(*(aSelBox.GetAt(i))));
	}
}

//-----------------------------------------------------------------------------
void DataStrArray::operator= (const CStringArray& arStrings)
{
	for (int i = 0; i < arStrings.GetSize(); i++)
	{
		ASSERT_TRACE1(arStrings.GetAt(i), "Failed selecting element %d", i);
		Add(new DataStr(arStrings.GetAt(i)));
	}
}


//////////////////////////////////////////////////////////////////////////////
//					DataTypeNamedArray Implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(DataTypeNamedArray, Array);

//////////////////////////////////////////////////////////////////////////////
//					MemoryLeakTrackNew Implementation
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(MemoryLeakTrackNew, CObject);

#ifdef _DEBUG
void MemoryLeakTrackNew::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nMemoryLeakTrackNew :");
	dc << _T("\n\t") << m_sFile << _T(": line ") << m_nLine;
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////
//          		DataObj class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DataObj, CContextObject)

//-----------------------------------------------------------------------------
DataObj::DataObj()
	:
	m_wDataStatus(0)
{
#ifdef _DEBUG
	m_pNewed = NULL;
#endif
	Clear();
}

//-----------------------------------------------------------------------------
DataObj::~DataObj()
{
#ifdef _DEBUG
	SAFE_DELETE(m_pNewed);
#endif

}

//-----------------------------------------------------------------------------
void DataObj::Assign(BYTE* pBuffer, DBLENGTH lLen, BOOL /*bUnicode = TRUE*/)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;


	void* pData = GetOleDBDataPtr();
	memcpy(pData, pBuffer, lLen);

	SetValid();
	SetModified();
	SetDirty();

	SignalOnChanged();
}

// ignora ReadOnly e Selected status mantenendo i valori originali, cioe` non si 
// ereditano per copia nelle assegnazioni e nelle copie le caratteristiche estetiche
// di sola lettura e di selezionato
//-----------------------------------------------------------------------------
void DataObj::AssignStatus(const DataObj& aDataObj)
{
	SetValid(aDataObj.IsValid());
	if (aDataObj.IsCollateCultureSensitive())
		SetCollateCultureSensitive();
	if (aDataObj.IsPrivate()) //@@BAUZI lo stato di PRIVATE è legato al singolo valore del dato 
		SetPrivate();
}
//-----------------------------------------------------------------------------
bool DataObj::AlignHKL(HotKeyLink* pHKL)
{
	//se il puntatore è presente in lista, vuol dire che la find per questo valore è stata fatta
	for (int i = 0; i < m_arAlignedHKLs.GetCount(); i++)
	{
		if (m_arAlignedHKLs[i] == pHKL)
			return false;
	}
	m_arAlignedHKLs.Add(pHKL);
	return true;
}

//-----------------------------------------------------------------------------
void DataObj::SerializeToJson(CJsonSerializer& jsonSerializer)
{
	jsonSerializer.WriteBool(szEnabled, !IsReadOnly());
	jsonSerializer.WriteInt(szType, GetDataType().m_wType);
	SerializeJsonValue(jsonSerializer);
}

//-----------------------------------------------------------------------------
void DataObj::SerializeJsonValue(CJsonSerializer& jsonSerializer)
{
	jsonSerializer.WriteString(szValue, FormatDataForXML());
}
//-----------------------------------------------------------------------------
void DataObj::AssignJsonValue(CJsonParser& jsonParser)
{
	CString value = jsonParser.ReadString(szValue);
	AssignFromXMLString(value);
}
//-----------------------------------------------------------------------------
void DataObj::SignalOnChanged()
{
	Signal(ON_CHANGED);
	m_arAlignedHKLs.RemoveAll();//tutti gli evedntuali hotlink dovranno rieffettuare la find, perché il valore è cambiato
}
//-----------------------------------------------------------------------------
void DataObj::AssignFromJson(CJsonParser& jsonParser)
{
	AssignJsonValue(jsonParser);
	bool enabled = jsonParser.ReadBool(szEnabled);
	SetReadOnly(!enabled);
}

//@@ rivedere
//-----------------------------------------------------------------------------
void DataObj::Clear(BOOL bValid)
{
	if (IsValueLocked())
		return;

	SetValid(bValid);
	SetModified();
	SetDirty();

	SetValueChanged(FALSE);

	if (IsPrivate()) //@@BAUZI lo stato di PRIVATE è legato al singolo valore del dato
		SetPrivate(FALSE);

	// non va chiamata per evitare duplicazioni, chiamarlo nel figlio!
	//SignalOnChanged();
}

//-----------------------------------------------------------------------------
CString DataObj::FormatData(LPCTSTR pszFormatName /* = NULL */) const
{
	// Viene formattato il dato
	Formatter* pFormatter = NULL;

	if (pszFormatName)
	{
		pFormatter = AfxGetFormatStyleTable()->GetFormatter(pszFormatName, NULL);
		ASSERT_TRACE1(pFormatter, "Formatter not found %s", pszFormatName);
	}

	if (pFormatter == NULL)
	{
		pFormatter = AfxGetFormatStyleTable()->GetFormatter(GetDataType(), NULL);
		if (pFormatter == NULL)
		{
			ASSERT_TRACE(FALSE, "Base formatter for the type not found");
			return Str();
		}
	}

	CString strCell; pFormatter->FormatDataObj(*this, strCell);

	return strCell;
}

//-----------------------------------------------------------------------------
//CString DataObj::ToJson(BOOL bracket/* = FALSE*/, BOOL escape /*= FALSE*/, BOOL quoted /*= TRUE*/) const
//{
//	return Str();
//}

//-----------------------------------------------------------------------------
void DataObj::SetAsTime(BOOL bIsTime)
{
	ASSERT_TRACE(GetDataType() == DATA_DATE_TYPE || GetDataType() == DATA_LNG_TYPE, "Wrong data type");	//@@ElapsedTime

	SetStatus(bIsTime, TIME);

	if (bIsTime && GetDataType() == DATA_DATE_TYPE)
		SetFullDate(TRUE);
}

//-----------------------------------------------------------------------------
void DataObj::SetAsHandle(BOOL bIsHandle)
{
	ASSERT_TRACE(GetDataType() == DATA_LNG_TYPE, "Wrong data type");

	SetStatus(bIsHandle, TB_HANDLE);
}

//-----------------------------------------------------------------------------
//void DataObj::SetAsVoid(BOOL bIsVoid)
//{
//	ASSERT(GetDataType() == DATA_NULL_TYPE);	
//
//	SetStatus(bIsHandle,	TB_VOID);
//}

//-----------------------------------------------------------------------------
void DataObj::SetReadOnly(BOOL bReadOnly)
{
	SetStatus(bReadOnly, READONLY);

	if (bReadOnly)
		SetStatus(FALSE, FINDABLE);

	SetModified();
}

//-----------------------------------------------------------------------------
void DataObj::SetOSLReadOnly(BOOL bReadOnly)
{
	SetStatus(bReadOnly, OSL_READONLY);

	if (bReadOnly)
		SetStatus(FALSE, FINDABLE);

	SetModified();
}

//-----------------------------------------------------------------------------
void DataObj::SetBPMReadOnly(BOOL bReadOnly)
{
	SetStatus(bReadOnly, BPM_READONLY);

	if (bReadOnly)
		SetStatus(FALSE, FINDABLE);

	SetModified();
}

//-----------------------------------------------------------------------------
void DataObj::SetAlwaysReadOnly(BOOL bReadOnly)
{
	SetStatus(bReadOnly, ALWAYS_READONLY);

	if (bReadOnly)
		SetStatus(FALSE, FINDABLE);

	SetModified();
}

//-----------------------------------------------------------------------------
void DataObj::SetAlwaysEditable(BOOL bEditable)
{
	SetStatus(bEditable, ALWAYS_EDITABLE);

	SetModified();
}

//-----------------------------------------------------------------------------
void DataObj::SetFindable(BOOL bFindable)
{
	SetStatus(bFindable, FINDABLE);

	if (bFindable)
	{
		SetStatus(FALSE, READONLY);
		SetStatus(FALSE, ALWAYS_READONLY);
	}

	SetModified();
}

//-----------------------------------------------------------------------------
void DataObj::SetValueLocked(BOOL bValueLocked)
{
	SetStatus(bValueLocked, VALUE_LOCKED);

	SetModified();
}

//-----------------------------------------------------------------------------
BOOL DataObj::CompareBy(ECompareType cmp, DataObj* pObj, BOOL bCompareNoCase/* = TRUE*/)
{
	ASSERT_VALID(pObj);

	if (!DataType::IsCompatible(GetDataType(), pObj->GetDataType()))
	{
		if (pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
		{
			DataStr dst(FormatData());
			if (bCompareNoCase)
			{
				dst.MakeUpper();
				DataStr dsv(*(DataStr*)pObj); dsv.MakeUpper();
				return dst.CompareBy(cmp, &dsv, FALSE);
			}
			return dst.CompareBy(cmp, pObj, FALSE);
		}
		return FALSE;
	}

	if (bCompareNoCase && IsKindOf(RUNTIME_CLASS(DataStr)) && pObj->IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		DataStr dst(*(DataStr*)this); dst.MakeUpper();
		DataStr dsv(*(DataStr*)pObj); dsv.MakeUpper();
		return dst.CompareBy(cmp, &dsv, FALSE);
	}

	switch (cmp)
	{
	case CMP_EQUAL:
		return *this == *pObj;
	case CMP_NOT_EQUAL:
		return *this != *pObj;

	case CMP_LESSER_THEN:
		return *this < *pObj;
	case CMP_LESSER_OR_EQUAL:
		return *this <= *pObj;

	case CMP_GREATER_THEN:
		return *this > *pObj;
	case CMP_GREATER_OR_EQUAL:
		return *this >= *pObj;

	case CMP_BEGIN_WITH:
		return ::WildcardMatch(this->FormatData(), pObj->FormatData() + '*');
	case CMP_END_WITH:
		return ::WildcardMatch(this->FormatData(), CString('*' + pObj->FormatData()));

	case CMP_CONTAINS:
		return ::WildcardMatch(this->FormatData(), CString('*' + pObj->FormatData()) + '*');
	case CMP_NOT_CONTAINS:
		return !::WildcardMatch(this->FormatData(), CString('*' + pObj->FormatData()) + '*');

	case CMP_MATCH:
		return ::WildcardMatch(this->FormatData(), pObj->FormatData());
		break;

	}
	ASSERT(FALSE);
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL DataObj::CompareBy(ECompareType cmp, const CStringArray& arCmpValues, DataObj* pPreAllocatedObj/* = NULL*/, BOOL bCompareNoCase/* = TRUE*/)
{
	DataObj* pdoObj = pPreAllocatedObj ? pPreAllocatedObj : DataObjClone();
	int n = 0;
	for (int j = 0; j < arCmpValues.GetSize(); j++)
	{
		pdoObj->Clear();
		pdoObj->Assign(arCmpValues.GetAt(j));

		if (pdoObj->IsValid())
		{
			BOOL b = CompareBy(cmp, pdoObj);

			if (cmp != CMP_NOT_EQUAL)
			{
				if (b)
				{
					if (!pPreAllocatedObj) delete pdoObj;
					return TRUE;
				}
			}
			else
			{
				if (!b)
				{
					break;
				}
				else n++;
			}
		}
	}
	if (!pPreAllocatedObj) delete pdoObj;

	if (cmp == CMP_NOT_EQUAL && n == arCmpValues.GetSize())
	{
		return TRUE;
	}

	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataObj::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataObj :");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
}

void DataObj::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//============================================================================
//          DataStr class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataStr, DataObj)

//-----------------------------------------------------------------------------
DataStr::DataStr()
	:
	m_strText(""),
	m_nAllocSize(0),
	m_bCollateCultureSensitive(false)
{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataStr::DataStr(LPCTSTR pszString)
	:
	m_strText(pszString),
	m_nAllocSize(0),
	m_bCollateCultureSensitive(false)
{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataStr::DataStr(const DataStr& aDataStr)
	:
	m_strText(aDataStr.m_strText),
	m_nAllocSize(0),
	m_bCollateCultureSensitive(false)
{
	AssignStatus(aDataStr);
	SetUpperCase(aDataStr.IsUpperCase());
	SetModified();
	SetDirty();

	// l'assign status evaluates single cases,
	// copy contructor assign status always
	if (aDataStr.IsCollateCultureSensitive())
		SetCollateCultureSensitive(TRUE);
}

//-----------------------------------------------------------------------------
DataStr::DataStr(const CString& aString)
	:
	m_strText(aString),
	m_nAllocSize(0),
	m_bCollateCultureSensitive(false)
{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataStr::~DataStr()
{
}

// Prealloca la stringa essenzialmente per gestire bene il bind delle colonne
//-----------------------------------------------------------------------------
void DataStr::Allocate()
{
	if (m_nAllocSize <= 0)
		return;

	if (m_nAllocSize >= 0x3FFFFFFF)
		m_nAllocSize = 255;
	m_strText.GetBufferSetLength(m_nAllocSize);
	m_strText.ReleaseBuffer();
}

//-----------------------------------------------------------------------------
int DataStr::CompareNoCase(const DataStr& aDataStr) const
{
	return _tcsicmp(m_strText, aDataStr.m_strText);
}

//-----------------------------------------------------------------------------
int DataStr::IsEqual(const DataObj& aDataObj) const
{
#ifdef _DEBUG
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)), "Parameter aDataObj must be of DataStr type");
		return FALSE;
	}
#endif

	DataStr& aDataStr = (DataStr&)aDataObj;
	BOOL bUpperCase = ((m_wDataStatus & UPPERCASE) == UPPERCASE);

	//se l'impostazione globale dice che sono sensibile alla culture e almeno uno
	//dei dataobj e' sensibile alla culture, mi comporto di conseguenza
	if (AfxIsThreadCollateCultureSensitive() &&
		(IsCollateCultureSensitive() || aDataObj.IsCollateCultureSensitive()))
		return AfxGetCultureInfo()->IsEqual(m_strText, aDataStr.m_strText, !bUpperCase || !aDataStr.IsUpperCase());

	BOOL bOk = bUpperCase
		? (_tcsicmp(m_strText, aDataStr.m_strText) == 0)
		: (m_strText.Compare(aDataStr.m_strText) == 0);

	return bOk;
}

//-----------------------------------------------------------------------------
int DataStr::IsLessThan(const DataObj& aDataObj) const
{
#ifdef _DEBUG
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)), "Parameter aDataObj must be of DataStr type");
		return FALSE;
	}
#endif

	DataStr& aDataStr = (DataStr&)aDataObj;

	BOOL bUpperCase = ((m_wDataStatus & UPPERCASE) == UPPERCASE);

	if (IsCollateCultureSensitive() || aDataObj.IsCollateCultureSensitive())
		return AfxGetCultureInfo()->IsLessThan(m_strText, aDataStr.m_strText, !bUpperCase);

	BOOL bOk = bUpperCase
		? (_tcsicmp(m_strText, aDataStr.m_strText) < 0)
		: (m_strText.Compare(aDataStr.m_strText) < 0);

	return bOk;
}

//-----------------------------------------------------------------------------
void DataStr::MakeUpper(BOOL bSignal)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	CString sOldValue;
	if (bSignal)
		sOldValue = m_strText;

	if (IsCollateCultureSensitive())
		AfxGetCultureInfo()->MakeUpper(m_strText);
	else
		m_strText.MakeUpper();

	if (bSignal && sOldValue != m_strText)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataStr::MakeLower(BOOL bSignal)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	CString sOldValue;
	if (bSignal)
		sOldValue = m_strText;

	if (IsCollateCultureSensitive())
		AfxGetCultureInfo()->MakeLower(m_strText);
	else
		m_strText.MakeLower();

	if (bSignal && sOldValue != m_strText)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
CString	DataStr::GetUpperCase()
{
	if (IsCollateCultureSensitive())
		return AfxGetCultureInfo()->GetUpperCase(m_strText);

	return Str().MakeUpper();

}

//-----------------------------------------------------------------------------
CString	DataStr::GetLowerCase()
{
	if (IsCollateCultureSensitive())
		return AfxGetCultureInfo()->GetLowerCase(m_strText);

	return Str().MakeLower();
}

//-----------------------------------------------------------------------------
void DataStr::StripBlank()
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	//CString sOldValue = m_strText;
	//::StripBlank(m_strText); 
	int len = m_strText.GetLength();
	m_strText.TrimRight(BLANK_CHAR); //bugfix 24701 (When used with no parameters, TrimRight removes trailing newline, space, and tab characters from the string.) 
									// invece è necessario eliminare solo il ' '

	//SetModified(); 
	//SetDirty();  

	//if (sOldValue != m_strText)
	if (len != m_strText.GetLength())
	{
		SetModified();
		SetDirty();

		SignalOnChanged();
	}
}

//-----------------------------------------------------------------------------
void* DataStr::GetRawData(DataSize* pDataSize) const
{
	// TRICS to optimize speed on RDEManager
	if (pDataSize)
		*pDataSize = (m_strText.GetLength() + 1) * sizeof(TCHAR); //Gestione UNICODE

	return (void*)((LPCTSTR)m_strText);
}

//-----------------------------------------------------------------------------
CString DataStr::Str(int nLen, int) const
{
	if (nLen <= 0)
		return m_strText;

	return cwsprintf(_T("%-*.*s"), nLen, nLen, (LPCTSTR)m_strText);
}

//-----------------------------------------------------------------------------
CString DataStr::ToString() const
{
	return cwsprintf(_T("\"%s\""), (LPCTSTR)m_strText);
}

//-----------------------------------------------------------------------------
void DataStr::Assign(BYTE* pBuffer, DBLENGTH len, BOOL bUseUnicode /*= TRUE*/)
{
	USES_CONVERSION;
	LPCTSTR pTBuffer = NULL;
	pTBuffer = (bUseUnicode) ? (LPCTSTR)pBuffer : A2T((LPCSTR)pBuffer);
	if (len > 0)
	{
		TCHAR p = NULL_CHAR;
		memcpy(pBuffer + len, &p, sizeof(TCHAR));
	}
	Assign(pTBuffer);
}

//-----------------------------------------------------------------------------
void DataStr::Assign(LPCTSTR pszString)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear();

	if (pszString)
	{
		if (m_strText == pszString)
			return;

		m_strText = pszString;

		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SignalOnChanged();
}


//-----------------------------------------------------------------------------
void DataStr::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(v.vt == VT_BSTR, "Parameter v must be of VT_BSTR type");
	USES_CONVERSION;
	Assign(W2T(v.bstrVal));
}

//-----------------------------------------------------------------------------
void DataStr::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	Assign((LPCTSTR)aRDEData.GetData());
}

//-----------------------------------------------------------------------------
void DataStr::Assign(const DataObj& aDataObj)
{
	Signal(ON_CHANGING);

	if (IsValueLocked())
		return;

	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)), "Parameter aDataObj must be of DataStr type");
		return;
	}

	DataStr& aDataStr = (DataStr&)aDataObj;

	CString sOldValue = m_strText;

	m_strText = aDataStr.m_strText;

	AssignStatus(aDataStr);
	SetModified();
	SetDirty();

	if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
		this->MakeUpper(FALSE);

	if (sOldValue != m_strText)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataStr::Assign(TCHAR ch)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear();

	if (m_strText == ch)
		return;

	m_strText = ch;

	if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
		this->MakeUpper(FALSE);

	SignalOnChanged();
}

// Non bisogna chiamare Empty() perche` non si deve riallocare il 
// buffer interno della stringa. ODBC richiede che il puntatore della colonna bindata
// non cambi, mentre nel caso della stringa se si rialloca se si usa Empty il puntatore
// ai dati cambia (nel caso di Empty diventa NIL)
//-----------------------------------------------------------------------------
void DataStr::Clear(BOOL bValid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);

	if (m_strText.IsEmpty())
		return;

	m_strText.Empty();

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataStr::SetLowerValue(int nLen)
{
	if (IsValueLocked())
		return;

	if (nLen <= 0) return;

	if (!IsEmpty()) return;

	m_nAllocSize = nLen;
	Allocate();

	Assign(_T(""));
}

//-----------------------------------------------------------------------------
void DataStr::SetUpperValue(int nLen)
{
	if (IsValueLocked())
		return;

	if (nLen <= 0) return;

	// viene presa in considerazione la stringa inserita dall'utente
	// ed eventualmente paddata di 'z'
	int nCurrSize = m_strText.GetLength();
	if (nCurrSize > 0)
	{
		int nPadLen = nLen - nCurrSize;
		if (nPadLen > 0)
			m_strText += AfxGetCultureInfo()->PadUpperLimitString(nPadLen);

		m_nAllocSize = m_strText.GetLength();
		return;
	}

	m_nAllocSize = nLen;
	Assign(AfxGetCultureInfo()->PadUpperLimitString(nLen));
}

//-----------------------------------------------------------------------------
BOOL DataStr::IsLowerValue() const
{
	if (m_nAllocSize == 0)
		return FALSE;

	return m_strText.IsEmpty();
}

//-----------------------------------------------------------------------------
BOOL DataStr::IsUpperValue() const
{
	return m_nAllocSize && m_strText == AfxGetCultureInfo()->PadUpperLimitString(m_nAllocSize);
}

//-----------------------------------------------------------------------------
CString DataStr::Trim() const
{
	CString strTmp;

	int nLen = m_strText.GetLength();
	for (int i = 0; i < nLen; i++)
		if (m_strText[i] != BLANK_CHAR)
			strTmp += m_strText[i];

	return strTmp;
}

//-----------------------------------------------------------------------------
CString DataStr::Ltrim() const
{
	int nLen = m_strText.GetLength();
	for (int i = 0; i < nLen; i++)
		if (m_strText[i] != BLANK_CHAR)
			return m_strText.Right(nLen - i);

	return CString("");
}

//-----------------------------------------------------------------------------
CString DataStr::Rtrim() const
{
	int nLen = m_strText.GetLength();
	for (int i = nLen - 1; i >= 0; i--)
		if (m_strText[i] != BLANK_CHAR)
			return m_strText.Left(i + 1);

	return CString("");
}

//-----------------------------------------------------------------------------
void DataStr::TrimUpperLimit()
{
	m_strText = AfxGetCultureInfo()->TrimUpperLimitString(m_strText);
}

//-----------------------------------------------------------------------------
CString DataStr::FormatDataForXML(BOOL) const
{
	return FormatStringForXML((LPCTSTR)m_strText);
}

//-----------------------------------------------------------------------------
void DataStr::AssignFromXMLString(LPCTSTR lpszValue)
{
	CString str(lpszValue);
	str.Replace(_T("\n"), _T("\r\n")); //altrimenti non importa correttamente il carattere di a capo
	Assign(str);
}

//-----------------------------------------------------------------------------
void DataStr::SetAt(int i, TCHAR c)
{
	Signal(ON_CHANGING);
	if (IsValueLocked()) return;

	BOOL bChanged = m_strText.GetAt(i) != c;
	m_strText.SetAt(i, c);
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}

// -----------------------------------------------------------------------------
void DataStr::Append(TCHAR c, BOOL duplicate /*= FALSE*/)
{
	Signal(ON_CHANGING);
	if (IsValueLocked()) return;

	BOOL bChanged = duplicate || m_strText.IsEmpty();
	if (bChanged)
	{
		m_strText += c;
	}
	else
	{
		if (m_strText[m_strText.GetLength() - 1] != c)
		{
			m_strText += c;
			bChanged = TRUE;
		}
	}

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
const DataStr& DataStr::operator+=(const DataStr& datastr)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return *this;

	BOOL bChanged = !datastr.m_strText.IsEmpty();

	if (bChanged)
	{
		m_strText += datastr.m_strText;
		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();

	return *this;
}

//-----------------------------------------------------------------------------
const DataStr& DataStr::operator+=(const CString& string)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return *this;

	BOOL bChanged = !string.IsEmpty();
	if (bChanged)
	{
		m_strText += string;
		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();

	return *this;
}

//-----------------------------------------------------------------------------
const DataStr& DataStr::operator+=(TCHAR ch)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return *this;

	BOOL bChanged = ch != NULL;
	if (bChanged)
	{
		m_strText += ch;
		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();

	return *this;
}

//-----------------------------------------------------------------------------
const DataStr& DataStr::operator+=(LPCTSTR psz)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return *this;

	BOOL bChanged = psz != NULL && psz[0] != NULL;
	if (bChanged)
	{
		m_strText += psz;
		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();

	return *this;
}

//-----------------------------------------------------------------------------
DataStr operator+(const DataStr& datastr1, const DataStr& datastr2)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataStr dest(datastr1.m_strText);
	dest.m_strText += datastr2.m_strText;

	return dest;
}

//-----------------------------------------------------------------------------
DataStr operator+(const DataStr& datastr, const CString& string)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataStr dest(datastr.m_strText);
	dest += string;

	return dest;
}

//-----------------------------------------------------------------------------
DataStr operator+(const CString& string, const DataStr& datastr)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataStr dest(string);
	dest += datastr;

	return dest;
}

//-----------------------------------------------------------------------------
DataStr operator+(const DataStr& datastr, TCHAR ch)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataStr dest(datastr.m_strText);
	dest += ch;

	return dest;
}

//-----------------------------------------------------------------------------
DataStr operator+(TCHAR ch, const DataStr& datastr)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	return DataStr(ch + datastr.m_strText);
}

//-----------------------------------------------------------------------------
DataStr operator+(const DataStr& datastr, LPCTSTR psz)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataStr dest(datastr.m_strText);
	dest += psz;

	return dest;
}

//-----------------------------------------------------------------------------
DataStr operator+(LPCTSTR psz, const DataStr& datastr)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	return DataStr(psz + datastr.m_strText);
}

//-----------------------------------------------------------------------------
BOOL operator==(const DataStr& s1, const DataStr& s2)
{
	return s1.IsEqual(s2);
}

//-----------------------------------------------------------------------------
BOOL operator==(const DataStr& s1, const CString& s2)
{
	return s1.IsEqual(DataStr(s2));
}

//-----------------------------------------------------------------------------
BOOL operator==(const CString& s1, const DataStr& s2)
{
	return s2 == s1;
}

//-----------------------------------------------------------------------------
BOOL operator==(const DataStr& s1, LPCTSTR s2)
{
	return s1.IsEqual(DataStr(s2));
}

//-----------------------------------------------------------------------------
BOOL operator==(LPCTSTR s1, const DataStr& s2)
{
	return s2 == s1;
}

//-----------------------------------------------------------------------------
BOOL operator<(const DataStr& s1, const DataStr& s2)
{
	return s1.IsLessThan(s2);
}

//-----------------------------------------------------------------------------
BOOL operator<(const DataStr& s1, const CString& s2)
{
	return s1.IsLessThan(DataStr(s2));
}

//-----------------------------------------------------------------------------
BOOL operator<(const CString& s1, const DataStr& s2)
{
	return !s2.IsLessEqualThan(DataStr(s1));
}

//-----------------------------------------------------------------------------
BOOL operator<(const DataStr& s1, LPCTSTR s2)
{
	return s1.IsLessThan(DataStr(s2));
}

//-----------------------------------------------------------------------------
BOOL operator<(LPCTSTR s1, const DataStr& s2)
{
	return !s2.IsLessEqualThan(DataStr(s1));
}

//-----------------------------------------------------------------------------
BOOL operator>(const DataStr& s1, const DataStr& s2)
{
	return !s1.IsLessEqualThan(s2);
}

//-----------------------------------------------------------------------------
BOOL operator>(const DataStr& s1, const CString& s2)
{
	return !s1.IsLessEqualThan(DataStr(s2));
}

//-----------------------------------------------------------------------------
BOOL operator>(const CString& s1, const DataStr& s2)
{
	return s2.IsLessThan(DataStr(s1));
}

//-----------------------------------------------------------------------------
BOOL operator>(const DataStr& s1, LPCTSTR s2)
{
	return !s1.IsLessEqualThan(DataStr(s2));
}

//-----------------------------------------------------------------------------
BOOL operator>(LPCTSTR s1, const DataStr& s2)
{
	return s2.IsLessThan(DataStr(s1));
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataStr::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataStr:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_strText= ", m_strText);
}

void DataStr::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG


//////////////////////////////////////////////////////////////////////////////
//          DataBool class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataBool, DataObj)

//-----------------------------------------------------------------------------
DataBool::DataBool(const BOOL bValue)
	:
	m_bValue(bValue)
{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataBool::DataBool(const DataBool& aDataBool)
	:
	m_bValue(aDataBool.m_bValue)
{
	AssignStatus(aDataBool);
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
VARIANT DataBool::ToVariant() const
{
	//Il seguente operatore provoca un warning di livello 4 dovuto alla costante VARIANT_TRUE: (short)1
	VARIANT v;
	v.vt = VT_BOOL;
	v.boolVal = (m_bValue ? VARIANT_TRUE : VARIANT_FALSE);
	return v;
}

//-----------------------------------------------------------------------------
int DataBool::IsEqual(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataBool)))
		return m_bValue == ((DataBool&)aDataObj).m_bValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		return m_bValue == (int)((DataInt&)aDataObj);

	ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataBool or DataInt type");
	return FALSE;
}

//-----------------------------------------------------------------------------
int DataBool::IsLessThan(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataBool)))
		return m_bValue < ((DataBool&)aDataObj).m_bValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		return m_bValue < (int)((DataInt&)aDataObj);

	ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataBool or DataInt type");
	return FALSE;
}

//-----------------------------------------------------------------------------
void* DataBool::GetRawData(DataSize* pDataSize) const
{
	// TRICS to optimize speed on RDEManager
	if (pDataSize)
		*pDataSize = sizeof(m_bValue);

	return (void*)&m_bValue;
}

//-----------------------------------------------------------------------------
CString DataBool::Str(int nLen, int) const
{
	if (nLen <= 0)
		return m_bValue ? _T("TRUE") : _T("FALSE");

	return m_bValue ? Strings::YES() : Strings::NO();
}

//-----------------------------------------------------------------------------
void DataBool::Assign(BYTE* pBuffer, DBLENGTH, BOOL bUseUnicode /*= TRUE*/)
{
	USES_CONVERSION;
	LPCTSTR pTBuffer = NULL;
	pTBuffer = (bUseUnicode) ? (LPCTSTR)pBuffer : A2T((LPCSTR)pBuffer);
	Assign((*pTBuffer == _T('1')) ? TRUE : FALSE);
}

//-----------------------------------------------------------------------------
void DataBool::Assign(LPCTSTR pszValue)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	BOOL bOldValue = m_bValue;

	m_bValue = FALSE;

	if (pszValue && pszValue[0])
	{
		if (
			AfxGetCultureInfo()->IsEqual(pszValue, Strings::YES())
			||
			AfxGetCultureInfo()->IsEqual(pszValue, _TB("True"))
			||
			AfxGetCultureInfo()->IsEqual(pszValue, _T("True"))
			)
		{
			m_bValue = TRUE;
			SetValid();
		}
		else if (
			AfxGetCultureInfo()->IsEqual(pszValue, Strings::NO())
			||
			AfxGetCultureInfo()->IsEqual(pszValue, _TB("False"))
			||
			AfxGetCultureInfo()->IsEqual(pszValue, _T("False"))
			)
		{
			SetValid();
		}
		else
		{
			//ASSERT_TRACE1(FALSE, "DataBool assigned with wrong value %s\n", pszValue);
			SetValid(FALSE);
		}
	}
	else
		SetValid();

	SetModified();
	SetDirty();

	if (bOldValue != m_bValue)
		SignalOnChanged();
}
//-----------------------------------------------------------------------------
void DataBool::SerializeJsonValue(CJsonSerializer& jsonSerializer)
{
	jsonSerializer.WriteBool(szValue, m_bValue == TRUE);
}
//-----------------------------------------------------------------------------
void DataBool::AssignJsonValue(CJsonParser& jsonParser)
{
	m_bValue = jsonParser.ReadBool(szValue);
}
//-----------------------------------------------------------------------------
void DataBool::Assign(const BOOL bValue)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	BOOL bChanged = bValue != m_bValue;
	if (bChanged)
		m_bValue = bValue;

	SetValid();
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}
//-----------------------------------------------------------------------------
void DataBool::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(v.vt == VT_BOOL, "Parameter v must be of VT_BOOL type");
	Assign(v.boolVal == VARIANT_TRUE);
}
//-----------------------------------------------------------------------------
void DataBool::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	Assign(*(BOOL*)aRDEData.GetData());
}

//-----------------------------------------------------------------------------
void DataBool::Assign(const DataObj& aDataObj)
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		Assign(((DataStr&)aDataObj).GetString());
		return;
	}

	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	BOOL bOldValue = m_bValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataBool)))
		m_bValue = (BOOL)((DataBool&)aDataObj);
	else if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		m_bValue = (int)((DataInt&)aDataObj);
	else
	{
		ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataBool or DataInt type");
		return;
	}

	AssignStatus(aDataObj);
	SetModified();
	SetDirty();

	if (bOldValue != m_bValue)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataBool::Clear(BOOL bValid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);
	if (!m_bValue)
		return;

	m_bValue = FALSE;

	SignalOnChanged();
}


//-----------------------------------------------------------------------------
//============================================================================================
// XML Data Type: boolean
// 0 or 1, where 0 == "false" and 1 =="true".
//============================================================================================
CString DataBool::FormatDataForXML(BOOL bSoapMode) const
{
	return FormatBoolForXML(m_bValue, bSoapMode);
}

//-----------------------------------------------------------------------------
void DataBool::AssignFromXMLString(LPCTSTR lpszValue)
{
	Assign(GetBoolFromXML(lpszValue));
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataBool::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataBool:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_bValue= ", m_bValue);
}

void DataBool::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG



//////////////////////////////////////////////////////////////////////////////
//          DataInt class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataInt, DataObj)

//-----------------------------------------------------------------------------
DataInt::DataInt(const int nValue)
	:
	m_nValue(nValue)
{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataInt::DataInt(const DataInt& aDataInt)
	:
	m_nValue(aDataInt.m_nValue)
{
	AssignStatus(aDataInt);
	SetModified();
	SetDirty();
}

// Identico codice per DataDbl, DataLng, DataInt per compatibilita` di tipo
//-----------------------------------------------------------------------------
int DataInt::IsEqual(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		return m_nValue == ((DataInt&)aDataObj).m_nValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataLng)))
		return m_nValue == ((DataLng&)aDataObj).m_nValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataDbl)))
		return m_nValue == ((DataDbl&)aDataObj).m_nValue;

	ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataInt, DataLng or DataDbl type");
	return FALSE;
}

// Identico codice per DataDbl, DataLng, DataInt per compatibilita` di tipo
//-----------------------------------------------------------------------------
int DataInt::IsLessThan(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		return m_nValue < ((DataInt&)aDataObj).m_nValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataLng)))
		return m_nValue < ((DataLng&)aDataObj).m_nValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataDbl)))
		return m_nValue < ((DataDbl&)aDataObj).m_nValue;

	ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataInt, DataLng or DataDbl type");
	return FALSE;
}

//-----------------------------------------------------------------------------
void* DataInt::GetRawData(DataSize* pDataSize) const
{
	// TRICS to optimize speed on RDEManager
	if (pDataSize)
		*pDataSize = sizeof(m_nValue);

	return (void*)&m_nValue;
}

//-----------------------------------------------------------------------------
CString DataInt::Str(int nLen, int nZeroPad) const
{
	if (nLen <= 0)
		return cwsprintf(_T("%d"), m_nValue);

	if (nZeroPad != 0)
		return cwsprintf(_T("%*d"), nLen, m_nValue);

	return cwsprintf(_T("%0*d"), nLen, m_nValue);
}

//-----------------------------------------------------------------------------
void DataInt::Assign(LPCTSTR pszIntStr)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	if (!IsIntNumber(pszIntStr))
	{
		Clear(FALSE);
		return;
	}

	int newValue = _tstoi(pszIntStr);
	BOOL bChanged = newValue != m_nValue;
	if (bChanged)
		m_nValue = newValue;

	SetValid();
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}
//-----------------------------------------------------------------------------
void DataInt::SerializeJsonValue(CJsonSerializer& jsonSerializer)
{
	jsonSerializer.WriteInt(szValue, m_nValue);
}

//-----------------------------------------------------------------------------
void DataInt::AssignJsonValue(CJsonParser& jsonParser)
{
	m_nValue = jsonParser.ReadInt(szValue);
}
//-----------------------------------------------------------------------------
void DataInt::Assign(const short nValue)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	BOOL bChanged = m_nValue != nValue;
	if (bChanged)
		m_nValue = nValue;

	SetValid();
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataInt::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(v.vt == VT_I2, "Parameter v must be of VT_I2 type");
	Assign(v.iVal);
}

//-----------------------------------------------------------------------------
void DataInt::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	Assign(*(short*)aRDEData.GetData());
}

//-----------------------------------------------------------------------------
void DataInt::Assign(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
	{
		if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
			Assign(((DataStr&)aDataObj).GetString());
		else
			ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)), "Parameter aDataObj must be of DataInt type");

		return;
	}

	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataInt& aDataInt = (DataInt&)aDataObj;
	BOOL bChanged = m_nValue != aDataInt.m_nValue;
	if (bChanged)
		m_nValue = aDataInt.m_nValue;

	AssignStatus(aDataInt);
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataInt::Clear(BOOL bValid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);

	if (m_nValue == 0)
		return;

	m_nValue = 0;

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataInt::SetLowerValue(int)
{
	if (IsValueLocked())
		return;

	if (!IsEmpty()) return;

	Assign(DataInt::MINVALUE);
}

//-----------------------------------------------------------------------------
void DataInt::SetUpperValue(int)
{
	if (IsValueLocked())
		return;

	if (!IsEmpty()) return;

	Assign(DataInt::MAXVALUE);
}

//-----------------------------------------------------------------------------
BOOL DataInt::IsLowerValue() const
{
	return (DataInt)*this == DataInt::MINVALUE;
}

//-----------------------------------------------------------------------------
BOOL DataInt::IsUpperValue() const
{
	return (DataInt)*this == DataInt::MAXVALUE;
}

//-----------------------------------------------------------------------------
//============================================================================================
// XML Data Type: int
// Number, with optional sign, no fractions, and no exponent.
//============================================================================================
CString DataInt::FormatDataForXML(BOOL) const
{
	return Str();
}

//-----------------------------------------------------------------------------
void DataInt::AssignFromXMLString(LPCTSTR lpszValue)
{
	Assign(lpszValue);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataInt::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataInt:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_nValue= ", m_nValue);
}

void DataInt::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////
//          DataLng class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataLng, DataObj)

//-----------------------------------------------------------------------------
DataLng::DataLng(const long nVal)
	:
	m_nValue(nVal)
{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataLng::DataLng(const DataLng& aDataLng)
	:
	m_nValue(aDataLng.m_nValue)
{
	AssignStatus(aDataLng);
	SetAsTime(aDataLng.IsATime());	//@@ElapsedTime
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataLng::DataLng(void* ptr, BOOL)
	:
	m_nValue((long)ptr)
{
	SetModified();
	SetDirty();

	ASSERT(ptr);
	if (ptr)
		SetAsHandle();
}

//-----------------------------------------------------------------------------
DataLng::DataLng(long lDays, long nHours, long nMins, long nSecs)	//@@ElapsedTime
{
	SetAsTime();

	Assign(lDays, nHours, nMins, nSecs);
}

// Identico codice per DataDbl, DataLng, DataInt per compatibilita` di tipo
//-----------------------------------------------------------------------------
int DataLng::IsEqual(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		return m_nValue == ((DataInt&)aDataObj).m_nValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataLng)))
		return m_nValue == ((DataLng&)aDataObj).m_nValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataDbl)))
		return m_nValue == ((DataDbl&)aDataObj).m_nValue;

	ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataInt, DataLng or DataDbl type");
	return FALSE;
}

// Identico codice per DataDbl, DataLng, DataInt per compatibilita` di tipo
//-----------------------------------------------------------------------------
int DataLng::IsLessThan(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		return m_nValue < ((DataInt&)aDataObj).m_nValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataLng)))
		return m_nValue < ((DataLng&)aDataObj).m_nValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataDbl)))
		return m_nValue < ((DataDbl&)aDataObj).m_nValue;

	ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataInt, DataLng or DataDbl type");
	return FALSE;
}

//-----------------------------------------------------------------------------
void DataLng::SetElapsedTimePrecision(int nPrec)
{
	AfxGetLoginContext()->SetElapsedTimePrecision(nPrec);
}
//-----------------------------------------------------------------------------
int DataLng::GetElapsedTimePrecision()
{
	CLoginContext *pContext = AfxGetLoginContext();
	return pContext ? pContext->GetElapsedTimePrecision() : 1;
}
//-----------------------------------------------------------------------------
void DataLng::SerializeJsonValue(CJsonSerializer& jsonSerializer)
{
	jsonSerializer.WriteInt(szValue, m_nValue);
}
//-----------------------------------------------------------------------------
void DataLng::AssignJsonValue(CJsonParser& jsonParser)
{
	m_nValue = jsonParser.ReadInt(szValue);
}
//-----------------------------------------------------------------------------
void* DataLng::GetRawData(DataSize* pDataSize) const
{
	// TRICS to optimize speed on RDEManager
	if (pDataSize)
		*pDataSize = sizeof(m_nValue);

	return (void*)&m_nValue;
}

//-----------------------------------------------------------------------------
CString DataLng::Str(int nParam, int nFormatIdx) const
{
	if (IsATime())	//@@ElapsedTime
	{
		if (nParam > 0)
		{
			if (nFormatIdx < 0)		// usa il formattatore di default
				nFormatIdx = AfxGetFormatStyleTable()->GetFormatIdx(GetDataType());

			Formatter* pFormatter = AfxGetFormatStyleTable()->GetFormatter(nFormatIdx, NULL);

			if (pFormatter)
			{
				CString str; pFormatter->Format(&m_nValue, str);
				return str;
			}

			ASSERT_TRACE1(FALSE, "Formatter not found: %d", nFormatIdx);
		}

		if (nParam == 0 && nFormatIdx == 0)
			return cwsprintf
			(
				_T("E%05d%02d%02d%#.2f"),		 //@@ElapsedTime
				GetDays(), GetHours(), GetMinutes(), GetSeconds()
			);

		if (nParam < 0 && nFormatIdx == 0) //@@ElapsedTime lo scrivo come secondi
			return cwsprintf(_T("%ld"), m_nValue);

		// Compatibilita` con la Parser::ParseComplexData(...) (cfr. parser.cpp)
		return cwsprintf
		(
			_T("%d:%02d:%02d:%#.2f"),		 //@@ElapsedTime
			GetDays(), GetHours(), GetMinutes(), GetSeconds()
		);
	}

	if (nParam <= 0)
		return cwsprintf(_T("%ld"), m_nValue);

	if (nFormatIdx != 0)
		return cwsprintf(_T("%*ld"), nParam, m_nValue);

	return cwsprintf(_T("%0*ld"), nParam, m_nValue);
}

//-----------------------------------------------------------------------------
CString DataLng::ToString() const
{
	if (IsATime())	//@@ElapsedTime
	{
		return CString(_T("{et\"")) + Str() + _T("\"}");
	}
	else
	{
		return Str();
	}
}

//-----------------------------------------------------------------------------
void DataLng::Assign(LPCTSTR pszLongStr)
{
	if (IsValueLocked())
		return;

	Assign(pszLongStr, UNDEF_FORMAT);
}

//-----------------------------------------------------------------------------
void DataLng::Assign(LPCTSTR pszLongStr, int nFormatIdx)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;
	long oldValue = m_nValue;

	if (IsATime() && !IsIntNumber(pszLongStr))	//@@ElapsedTime
	{
		BOOL bValid = FALSE;	// potenzialmente incompatibile con la Str(-1)
		if (pszLongStr && *pszLongStr == _T('E'))
			bValid = ::GetElapsedTime	// conforme a quello che scrive la Str(0, 0)
			(
				*this, &pszLongStr[1],
				CElapsedTimeFormatHelper::TIME_DHMS,
				NULL,
				NULL
			);
		else
			bValid = ::GetElapsedTime	// conforme a quello che scrive la Str(1)
			(
				*this, pszLongStr,
				(nFormatIdx == UNDEF_FORMAT) ? AfxGetFormatStyleTable()->GetFormatIdx(GetDataType()) : nFormatIdx,
				NULL
			);

		SetValid(bValid);
		SetModified();
		SetDirty();

		if (oldValue != m_nValue)
			SignalOnChanged();
		return;
	}

	if (!IsIntNumber(pszLongStr))
	{
		Clear(FALSE);
		return;
	}

	m_nValue = _tstol(pszLongStr);

	SetValid();
	SetModified();
	SetDirty();

	if (oldValue != m_nValue)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataLng::Assign(const long nVal)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;
	BOOL bChanged = m_nValue != nVal;

	if (bChanged)
		m_nValue = nVal;

	SetValid();
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataLng::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(v.vt == VT_I4, "Parameter v must be of type VT_I4");
	Assign(v.lVal);
}

//-----------------------------------------------------------------------------
void DataLng::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	Assign(*(long*)aRDEData.GetData());
}

//-----------------------------------------------------------------------------
void DataLng::Assign(const DataObj& aDataObj)
{
	if (IsValueLocked())
		return;

	long oldValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataLng)))
	{
		Signal(ON_CHANGING);

		oldValue = m_nValue;
		m_nValue = ((DataLng&)aDataObj).m_nValue;
	}
	else if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
	{
		Signal(ON_CHANGING);

		oldValue = m_nValue;
		m_nValue = (int)((DataInt&)aDataObj);
	}
	else if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		Assign(((DataStr&)aDataObj).GetString());
		return;
	}
	else
	{
		CString sErr(L"DataLng::Assign called with wrong datatype: " + aDataObj.GetDataType().ToString());
		ASSERT_TRACE(FALSE, sErr);
		return;
	}

	AssignStatus(aDataObj);
	SetModified();
	SetDirty();

	if (oldValue != m_nValue)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
/*
void DataLng::Assign(const DataObj& aDataObj)
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		Assign(((DataStr&) aDataObj).GetString());
		return;

	Signal(ON_CHANGING);

	if (IsValueLocked ())
		return;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataLng)))
	{
		Signal(ON_CHANGING);

		oldValue = m_nValue;
		m_nValue = ((DataLng&) aDataObj).m_nValue;
	}
	else if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
	{
		Signal(ON_CHANGING);

		oldValue = m_nValue;
		m_nValue = (int)((DataInt&) aDataObj);
	}
	else if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		Assign(((DataStr&) aDataObj).GetString());
		return;
	}
	else
	{
		CString sErr(L"DataLng::Assign called with wrong datatype: " + CString(aDataObj.GetRuntimeClass()->m_lpszClassName));
		ASSERT_TRACE(FALSE,sErr);
		return;
	}

	AssignStatus	(aDataObj);
	SetModified		();
	SetDirty		();

	if (oldValue != m_nValue)
		SignalOnChanged();
}
*/
//-----------------------------------------------------------------------------
void DataLng::SetElapsedTime(const DataDate& date1, const DataDate& date2)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	ASSERT_TRACE(IsATime(), "DataObj \"this\" is not a time");
	ASSERT_TRACE(date1.IsFullDate() == date2.IsFullDate(), "Either date1 or date 2 is not a full date");
	ASSERT_TRACE(date1.IsATime() == date2.IsATime(), "Either date1 or date 2 is not a time");

	long newValue = (date2 - date1) * 24L * 3600L + (date2.TotalSeconds() - date1.TotalSeconds());
	BOOL bChanged = m_nValue != newValue;
	if (bChanged)
		m_nValue = newValue;

	SetValid();
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataLng::Assign(long lDays, long nHours, long nMins, double nSecs)	//@@ElapsedTime
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	ASSERT_TRACE(IsATime(), "DataObj \"this\" is not a time");

	int nPrecision = GetElapsedTimePrecision();
	long newValue = (long)(nSecs * nPrecision + 0.5) +
		60 * (nMins  * nPrecision +
			60 * (nHours * nPrecision +
				24 * lDays * nPrecision));

	BOOL bChanged = m_nValue != newValue;
	if (bChanged)
		m_nValue = newValue;

	SetValid();
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
VARIANT DataLng::ToVariant() const
{
	VARIANT v;

	if (IsATime())
	{
		v.vt = VT_R8; // VT_TIME;

		COleDateTimeSpan span = COleDateTimeSpan
		(
			GetDays(),
			GetHours(),
			GetMinutes(),
			(long)GetSeconds()
		);

		if (span.m_status == COleDateTimeSpan::valid)
		{
			v.dblVal = span.m_span;
		}
		else
			v.vt = VT_NULL; //VT_ERROR
	}
	else
	{
		v.vt = VT_I4;
		v.lVal = m_nValue;
	}
	return v;
}

//-----------------------------------------------------------------------------
void DataLng::Clear(BOOL bValid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);

	if (m_nValue == 0L)
		return;

	m_nValue = 0L;

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataLng::SetLowerValue(int)
{
	if (IsValueLocked())
		return;

	if (!IsEmpty()) return;

	Assign(DataLng::MINVALUE);
}

//-----------------------------------------------------------------------------
void DataLng::SetUpperValue(int)
{
	if (IsValueLocked())
		return;

	if (!IsEmpty()) return;

	Assign(DataLng::MAXVALUE);
}

//-----------------------------------------------------------------------------
BOOL DataLng::IsLowerValue() const
{
	return (DataLng)*this == DataLng::MINVALUE;
}

//-----------------------------------------------------------------------------
BOOL DataLng::IsUpperValue() const
{
	return (DataLng)*this == DataLng::MAXVALUE;
}

//-----------------------------------------------------------------------------
//============================================================================================
// XML Data Type: int
// Number, with optional sign, no fractions, and no exponent.
//============================================================================================
CString DataLng::FormatDataForXML(BOOL) const
{
	return Str(-1, 0); // in XML lo scrivo come numero ovvero in secondi
}

//-----------------------------------------------------------------------------
void DataLng::AssignFromXMLString(LPCTSTR lpszValue)
{
	Assign(lpszValue);
}


/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataLng::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataLng:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_nValue = ", m_nValue);
}

void DataLng::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//          DataDbl class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataDbl, DataObj)
IMPLEMENT_DYNCREATE(DataMon, DataDbl)
IMPLEMENT_DYNCREATE(DataQty, DataDbl)
IMPLEMENT_DYNCREATE(DataPerc, DataDbl)

//-----------------------------------------------------------------------------
DataDbl::DataDbl(const double aDouble)
	:
	m_nValue(aDouble)
{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataDbl::DataDbl(const DataDbl& aDataDbl)
	:
	m_nValue(aDataDbl.m_nValue)
{
	AssignStatus(aDataDbl);
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
int DataDbl::IsEmpty() const
{
	return fabs(m_nValue) < GetEpsilon();
}

// Identico codice per DataDbl, DataLng, DataInt per compatibilita` di tipo
//-----------------------------------------------------------------------------
int DataDbl::IsEqual(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		return fabs(m_nValue - (double)(((DataInt&)aDataObj).m_nValue)) < GetEpsilon();

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataLng)))
		return fabs(m_nValue - (double)(((DataLng&)aDataObj).m_nValue)) < GetEpsilon();

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataDbl)))
		return fabs(m_nValue - (double)(((DataDbl&)aDataObj).m_nValue)) < GetEpsilon();

	ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataInt, DataLng or DataDbl type");
	return FALSE;
}

// Identico codice per DataDbl, DataLng, DataInt per compatibilita` di tipo
//-----------------------------------------------------------------------------
int DataDbl::IsLessThan(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		return m_nValue < (double)(((DataInt&)aDataObj).m_nValue) && fabs(m_nValue - (double)(((DataInt&)aDataObj).m_nValue)) >= GetEpsilon();

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataLng)))
		return m_nValue < (double)(((DataLng&)aDataObj).m_nValue) && fabs(m_nValue - (double)(((DataLng&)aDataObj).m_nValue)) >= GetEpsilon();

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataDbl)))
		return m_nValue < (double)(((DataDbl&)aDataObj).m_nValue) && fabs(m_nValue - (double)(((DataDbl&)aDataObj).m_nValue)) >= GetEpsilon();

	ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataInt, DataLng or DataDbl type");
	return FALSE;
}

//-----------------------------------------------------------------------------
void DataDbl::Round(int nDec)
{
	round(m_nValue, nDec);
}

//-----------------------------------------------------------------------------
void* DataDbl::GetRawData(DataSize* pDataSize) const
{
	// Trucco per ottimizzare la comunicazione con RDEManager
	if (pDataSize)
		*pDataSize = sizeof(m_nValue);

	return (void*)&m_nValue;
}

//-----------------------------------------------------------------------------
CString DataDbl::Str(int nLen, int nDecimal) const
{
	if (nLen <= 0)
		return cwsprintf(_T("%f"), m_nValue);

	if (nDecimal < 0)
		return cwsprintf(_T("%*f"), nLen, m_nValue);

	return cwsprintf(_T("%*.*f"), nLen, nDecimal, m_nValue);
}

//-----------------------------------------------------------------------------
void DataDbl::Assign(LPCTSTR pszDoubleStr)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	if (!IsFloatNumber(pszDoubleStr))
	{
		Clear(FALSE);
		return;
	}

	double dOldValue = m_nValue;

	m_nValue = _tstof(pszDoubleStr);

	SetValid();
	SetModified();
	SetDirty();

	if (dOldValue != m_nValue)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataDbl::Assign(const double aDouble)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	double oldValue = m_nValue;

	m_nValue = aDouble;

	SetValid();
	SetModified();
	SetDirty();

	if (oldValue != m_nValue)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataDbl::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(v.vt == VT_R8, "Parameter v must be of VT_R8 type");
	Assign(v.dblVal);
}

//-----------------------------------------------------------------------------
void DataDbl::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	Assign(*(double*)aRDEData.GetData());
}

//-----------------------------------------------------------------------------
void DataDbl::Assign(const DataObj& aDataObj)
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		Assign(((DataStr&)aDataObj).GetString());
		return;
	}

	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	double oldValue = m_nValue;

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataInt)))
		m_nValue = ((DataInt&)aDataObj).m_nValue;
	else if (aDataObj.IsKindOf(RUNTIME_CLASS(DataLng)))
		m_nValue = ((DataLng&)aDataObj).m_nValue;
	else if (aDataObj.IsKindOf(RUNTIME_CLASS(DataDbl)))
		m_nValue = ((DataDbl&)aDataObj).m_nValue;
	else
	{
		ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataInt, DataLng or DataDbl type");
		return;
	}

	AssignStatus(aDataObj);
	SetModified();
	SetDirty();

	if (oldValue != m_nValue)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataDbl::Clear(BOOL bValid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);

	double oldValue = m_nValue;

	m_nValue = 0.0;

	if (oldValue != m_nValue)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataDbl::SetLowerValue(int)
{
	if (!IsEmpty() || IsValueLocked())
		return;

	Assign(DataDbl::MINVALUE);
}

//-----------------------------------------------------------------------------
void DataDbl::SetUpperValue(int)
{
	if (!IsEmpty() || IsValueLocked())
		return;

	Assign(DataDbl::MAXVALUE);
}

//-----------------------------------------------------------------------------
BOOL DataDbl::IsLowerValue() const
{
	return *this == DataDbl::MINVALUE;
}

//-----------------------------------------------------------------------------
BOOL DataDbl::IsUpperValue() const
{
	return *this == DataDbl::MAXVALUE;
}
//-----------------------------------------------------------------------------
void DataDbl::SerializeJsonValue(CJsonSerializer& jsonSerializer)
{
	jsonSerializer.WriteDouble(szValue, m_nValue);
}
//-----------------------------------------------------------------------------
void DataDbl::AssignJsonValue(CJsonParser& jsonParser)
{
	m_nValue = jsonParser.ReadDouble(szValue);
}

//============================================================================================
// XML Data Type: float
// Real number, with no limit on digits; can potentially have a leading sign, 
// fractional digits, and optionally an exponent. Punctuation as in U.S. English. 
// Values range from 1.7976931348623157E+308 to 2.2250738585072014E-308.
//============================================================================================
CString DataDbl::FormatDataForXML(BOOL bSoap) const
{
	//devo usare l'approssimazione
	if (!bSoap)
	{
		int nDec = GetNumDecimals(GetEpsilon(), EPSILON_DECIMAL);
		double dDummy = m_nValue;
		round(dDummy, nDec);
		return cwsprintf(_T("%.*f"), nDec, dDummy);
	}
	return cwsprintf(_T("%.*f"), 15, m_nValue);
}

//-----------------------------------------------------------------------------
void DataDbl::AssignFromXMLString(LPCTSTR lpszValue)
{
	Assign(lpszValue);
}

//-----------------------------------------------------------------------------
double DataDbl::GetEpsilon()
{
	return GetEpsilonForDataType(DataType::Double);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataDbl::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataDbl:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_nValue= ", m_nValue);
}

void DataDbl::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG


//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//          DataMon class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
DataMon::DataMon(const double nDouble /*0.0*/)
	:
	DataDbl(nDouble)
{
	SetAccountable(TRUE);
}

//-----------------------------------------------------------------------------
DataMon::DataMon(const DataDbl& nDouble)
	:
	DataDbl(nDouble)
{
	SetAccountable(TRUE);
}

//-----------------------------------------------------------------------------
DataMon::DataMon(const DataMon& nMoney)
	:
	DataDbl(nMoney)
{
	SetAccountable(nMoney.IsAccountable());
}

//-----------------------------------------------------------------------------
CString	DataMon::FormatData(LPCTSTR pszFormatName /*= NULL*/) const
{
	return __super::FormatData(pszFormatName == NULL ? AfxGetMoneyFormatterName(IsAccountable()) : pszFormatName);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics
//-----------------------------------------------------------------------------
double DataMon::GetEpsilon()
{
	return GetEpsilonForDataType(DataType::Money);
}

#ifdef _DEBUG
void DataMon::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataMon:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_nValue= ", m_nValue);
}

void DataMon::AssertValid() const
{
	DataDbl::AssertValid();
}
#endif //_DEBUG



//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//          DataQty class implementations
//////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////
// Diagnostics

//-----------------------------------------------------------------------------
double DataQty::GetEpsilon()
{
	return GetEpsilonForDataType(DataType::Quantity);
}

#ifdef _DEBUG
void DataQty::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataQta:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_nValue= ", m_nValue);
}

void DataQty::AssertValid() const
{
	DataDbl::AssertValid();
}
#endif //_DEBUG




//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
//          DataPerc class implementations
//////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////
// Diagnostics
//-----------------------------------------------------------------------------
double DataPerc::GetEpsilon()
{
	return GetEpsilonForDataType(DataType::Percent);
}

#ifdef _DEBUG
void DataPerc::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataPerc:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_nValue= ", m_nValue);
}

void DataPerc::AssertValid() const
{
	DataDbl::AssertValid();
}
#endif //_DEBUG

//////////////////////////////////////////////////////////////////////////////
//          DataDate class implementations
//////////////////////////////////////////////////////////////////////////////
//
static void SetDateTimeTypeAndValue(DBTIMESTAMP& aDateTime, DataDate& aDataDate)
{
	// "Prova" a preimpostare la tipologia sulla base dei valori
	//
	if (
		aDateTime.hour != MIN_HOUR ||
		aDateTime.minute != MIN_MINUTE ||
		aDateTime.second != MIN_SECOND
		)
		if (
			(
				aDateTime.day == MIN_TIME_DAY		&&
				aDateTime.month == MIN_TIME_MONTH	&&
				aDateTime.year == MIN_TIME_YEAR
				) ||
				(
					aDateTime.day == MIN_DAY		&&
					aDateTime.month == MIN_MONTH	&&
					aDateTime.year == MIN_YEAR
					)
			)
		{
			// Devo forzare la data "speciale" per indicare solo il tempo:
			aDateTime.day = MIN_TIME_DAY;
			aDateTime.month = MIN_TIME_MONTH;
			aDateTime.year = MIN_TIME_YEAR;

			aDataDate.SetAsTime();
		}
		else
			aDataDate.SetFullDate();

	aDataDate.Assign(aDateTime);
}
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataDate, DataObj)

//-----------------------------------------------------------------------------
DataDate::DataDate()
{
	Clear();
}

//-----------------------------------------------------------------------------
DataDate::DataDate(const DataDate& aDataDate)
{
	Clear();

	if (aDataDate.IsFullDate())
		if (aDataDate.IsATime())
			SetAsTime();
		else
			SetFullDate();

	Assign(aDataDate);
}

//-----------------------------------------------------------------------------
DataDate::DataDate(const DBTIMESTAMP& aDateTimeParam)
{
	DBTIMESTAMP aDateTime = aDateTimeParam;

	Clear();
	SetDateTimeTypeAndValue(aDateTime, *this);
}

//-----------------------------------------------------------------------------
DataDate::DataDate(const CTime& aTime)
{
	Clear();

	DBTIMESTAMP aDateTime;

	aDateTime.day = (SWORD)aTime.GetDay();
	aDateTime.month = (SWORD)aTime.GetMonth();
	aDateTime.year = (UWORD)aTime.GetYear();

	aDateTime.hour = (SWORD)aTime.GetHour();
	aDateTime.minute = (SWORD)aTime.GetMinute();
	aDateTime.second = (UWORD)aTime.GetSecond();
	aDateTime.fraction = 0;

	SetDateTimeTypeAndValue(aDateTime, *this);
}

//-----------------------------------------------------------------------------
DataDate::DataDate
(
	const UWORD wDay, const UWORD wMonth, const SWORD wYear,
	const UWORD nHour, const UWORD nMinute, const UWORD nSecond
)
{
	Clear();

	DBTIMESTAMP aDateTime;

	aDateTime.day = wDay;
	aDateTime.month = wMonth;
	aDateTime.year = wYear;

	aDateTime.hour = nHour;
	aDateTime.minute = nMinute;
	aDateTime.second = nSecond;
	aDateTime.fraction = 0;

	SetDateTimeTypeAndValue(aDateTime, *this);
}

//-----------------------------------------------------------------------------
DataDate::DataDate(const long nLongDate, const long nLongTime)
{
	Clear();

	// prima si cerca di preimpostare la tipologia sulla base dei valori
	//
	if (nLongTime)
	{
		long nNullDateForTime = ::GetGiulianDate(MIN_TIME_DAY, MIN_TIME_MONTH, MIN_TIME_YEAR);

		if (nLongDate == 0 || nLongDate == nNullDateForTime)
		{
			SetAsTime();

			// forza la data "speciale" per indicare solo il tempo
			Assign(nLongDate, nNullDateForTime);
			return;
		}

		SetFullDate();
	}

	Assign(nLongDate, nLongTime);
}

//-----------------------------------------------------------------------------
DataDate::DataDate(LPCTSTR pszDateStr, BOOL bFixFormat /* = FALSE */)
{
	DBTIMESTAMP aDateTime;

	BOOL bOk = bFixFormat
		? GetTimeStamp	// compatibile con la DataDate::Str(-1, -1)
		(
			aDateTime, pszDateStr,
			CDateFormatHelper::DATE_YMD, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999, NULL, NULL,
			CDateFormatHelper::TIME_HF99, NULL, NULL, NULL
		)
		: GetTimeStamp
		(
			aDateTime, pszDateStr,
			AfxGetFormatStyleTable()->GetFormatIdx(GetDataType())
		);

	if (bOk)
		SetDateTimeTypeAndValue(aDateTime, *this);
	else
		Clear(FALSE);
}

//-----------------------------------------------------------------------------
int DataDate::IsEqual(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataDate)))
	{
		DataDate& aDataDate = (DataDate&)aDataObj;
		return (*this == aDataDate);
	}

	ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataDate)), "Parameter aDataObj must be of DataDate type");
	return FALSE;
}

//-----------------------------------------------------------------------------
int DataDate::IsLessThan(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataDate)))
	{
		DataDate& aDataDate = (DataDate&)aDataObj;
		return (*this < aDataDate);
	}

	ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataDate)), "Parameter aDataObj must be of DataDate type");
	return FALSE;
}

//-----------------------------------------------------------------------------
const tm DataDate::GetTMDateTime() const
{
	tm aTime;
	aTime.tm_year = Year() - 1900;		//1900 based
	aTime.tm_mon = Month() - 1;		//zero based
	aTime.tm_mday = Day();
	aTime.tm_hour = Hour();
	aTime.tm_min = Minute();
	aTime.tm_sec = Second();
	aTime.tm_wday = DayOfWeek() + 1;	//zero based sunday
	if (aTime.tm_wday == 7)
		aTime.tm_wday = 0;
	aTime.tm_yday = DayOfYear();
	aTime.tm_isdst = -1;
	return aTime;
}

// operatori friend di confronto
//
//-----------------------------------------------------------------------------
BOOL operator==(const DataDate& d1, const DataDate&	d2)
{
	ASSERT_TRACE(d1.IsATime() == d2.IsATime(), "Either parameter d1 or d2 is not a time");

	if (d1.m_DateStruct.day != d2.m_DateStruct.day ||
		d1.m_DateStruct.month != d2.m_DateStruct.month ||
		d1.m_DateStruct.year != d2.m_DateStruct.year
		)
		return FALSE;

	if (d1.IsFullDate() && d2.IsFullDate())
	{
		if (d1.m_DateStruct.second != d2.m_DateStruct.second ||
			d1.m_DateStruct.minute != d2.m_DateStruct.minute ||
			d1.m_DateStruct.hour != d2.m_DateStruct.hour
			)
			return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL operator!=(const DataDate& d1, const DataDate&	d2)
{
	return !(d1 == d2);
}

//-----------------------------------------------------------------------------
BOOL operator< (const DataDate& d1, const DataDate&	d2)
{
	ASSERT_TRACE(d1.IsATime() == d2.IsATime(), "Either parameter d1 or d2 is not a time");

	long gd1 = d1.GiulianDate();
	long gd2 = d2.GiulianDate();

	BOOL bLess = gd1 < gd2;

	if (d1.IsFullDate() && d2.IsFullDate())
	{
		long ts1 = d1.TotalSeconds();
		long ts2 = d2.TotalSeconds();

		bLess = bLess || (gd1 == gd2 && ts1 < ts2);
	}

	return bLess;
}

//-----------------------------------------------------------------------------
BOOL operator> (const DataDate& d1, const DataDate&	d2)
{
	ASSERT_TRACE(d1.IsATime() == d2.IsATime(), "Either parameter d1 or d2 is not a time");

	long gd1 = d1.GiulianDate();
	long gd2 = d2.GiulianDate();

	BOOL bLess = gd1 > gd2;

	if (d1.IsFullDate() && d2.IsFullDate())
	{
		long ts1 = d1.TotalSeconds();
		long ts2 = d2.TotalSeconds();

		bLess = bLess || (gd1 == gd2 && ts1 > ts2);
	}

	return bLess;
}

//-----------------------------------------------------------------------------
BOOL DataDate::IsEmpty() const
{
	// Dato che Time e` identificato univocamente dall'avere
	// la data valorizzata a MIN_TIME_DAY, MIN_TIME_MONTH, MIN_TIME_YEAR
	// siha che NullDate == NullDateTime == NullTime ==
	// MIN_DAY, MIN_MONTH, MIN_YEAR, MIN_HOUR, MIN_MINUTE, MIN_SECOND
	// (vedi IsNullDate - generic.cpp)
	return ::IsNullDate(m_DateStruct, IsATime());
}

//-----------------------------------------------------------------------------
void* DataDate::GetRawData(DataSize* pDataSize) const
{
	// TRICS to optimize speed on RDEManager
	if (pDataSize)
		*pDataSize = sizeof(m_DateStruct);

	return (void*)&m_DateStruct;
}

//-----------------------------------------------------------------------------
CString DataDate::Str(int nUseFormatter, int nFormatIdx) const
{
	if (IsEmpty())
		return _T("");

	if (nUseFormatter > 0)
	{
		if (nFormatIdx < 0)		// usa il formattatore di default
			nFormatIdx = AfxGetFormatStyleTable()->GetFormatIdx(GetDataType());

		if (nFormatIdx >= 0)
		{
			Formatter* pFormatter = AfxGetFormatStyleTable()->GetFormatter(nFormatIdx, NULL);

			if (pFormatter)
			{
				CString str; pFormatter->Format(&m_DateStruct, str);
				return str;
			}
		}
	}

	// Compatibilita` con la Parser::ParseComplexData(...) (cfr. parser.cpp)
	BOOL bNoSeparator = nFormatIdx < 0 || (nFormatIdx == 0 && nUseFormatter == 0);

	if (IsFullDate())
		if (IsATime())
			if (bNoSeparator)
				return cwsprintf
				(
					_T("%s%02d%02d%02d"), nFormatIdx == 0 ? _T("T") : _T(""),
					m_DateStruct.hour, m_DateStruct.minute, m_DateStruct.second
				);
			else
				return cwsprintf
				(
					_T("%02d:%02d:%02d"),
					m_DateStruct.hour, m_DateStruct.minute, m_DateStruct.second
				);
		else
			if (bNoSeparator)
				return cwsprintf
				(
					_T("%s%04d%02d%02d%02d%02d%02d"), nFormatIdx == 0 ? _T("F") : _T(""),
					m_DateStruct.year, m_DateStruct.month, m_DateStruct.day,
					m_DateStruct.hour, m_DateStruct.minute, m_DateStruct.second
				);
			else
				return cwsprintf
				(
					_T("%02d/%02d/%04d %02d:%02d:%02d"),
					m_DateStruct.day, m_DateStruct.month, m_DateStruct.year,
					m_DateStruct.hour, m_DateStruct.minute, m_DateStruct.second
				);
	else
		if (bNoSeparator)
			return cwsprintf
			(
				_T("%s%04d%02d%02d"), nFormatIdx == 0 ? _T("D") : _T(""),
				m_DateStruct.year, m_DateStruct.month, m_DateStruct.day
			);
		else
			return cwsprintf
			(
				_T("%02d/%02d/%04d"),
				m_DateStruct.day, m_DateStruct.month, m_DateStruct.year
			);
}

//-----------------------------------------------------------------------------
CString DataDate::ToString() const
{
	CString str;
	if (this->IsFullDate())
	{
		str = L"{dt\"";
		ASSERT(this->GetDataType() == DataType::DateTime);
	}
	else if (this->IsATime())
	{
		str = L"{t\"";
		ASSERT(this->GetDataType() == DataType::Time);
	}
	else
	{
		str = L"{d\"";
		ASSERT(this->GetDataType() == DataType::Date);
	}

	// per la data usa il formato GG/MM/AAAA
	return	str + Str(-1, 0) + _T("\"}");
}

//-----------------------------------------------------------------------------
void DataDate::Assign(LPCTSTR pszDateStr)
{
	if (IsValueLocked())
		return;

	Assign(pszDateStr, UNDEF_FORMAT);
}


//-----------------------------------------------------------------------------
void DataDate::Assign(LPCTSTR pszDateStr, int nFormatIdx)
{
	if (IsValueLocked())
		return;

	if (!pszDateStr || !*pszDateStr)
	{
		Clear(TRUE);
		return;
	}

	BOOL bOk;
	DBTIMESTAMP aDateTime;
	switch (*pszDateStr)
	{
	case _T('D'):	// Date : compatibile con la DataDate::Str(0, 0)
		bOk = GetTimeStamp
		(
			aDateTime, &pszDateStr[1],
			CDateFormatHelper::DATE_YMD, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999, NULL, NULL,
			CDateFormatHelper::TIME_NONE, NULL, NULL, NULL
		);
		break;
	case _T('F'):	// DateTime : compatibile con la DataDate::Str(0, 0)
		bOk = GetTimeStamp
		(
			aDateTime, &pszDateStr[1],
			CDateFormatHelper::DATE_YMD, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999, NULL, NULL,
			CDateFormatHelper::TIME_HF99, NULL, NULL, NULL
		);
		break;
	case _T('T'):	// Time : compatibile con la DataDate::Str(0, 0)
		bOk = GetTimeStamp
		(
			aDateTime, &pszDateStr[1],
			0, 0, 0, 0, NULL, NULL,
			CDateFormatHelper::TIME_HF99 | CDateFormatHelper::TIME_ONLY, NULL, NULL, NULL
		);
		break;
	default:	// compatibile con la DataDate::Str(1)
		bOk = GetTimeStamp
		(
			aDateTime, pszDateStr,
			(nFormatIdx == UNDEF_FORMAT) ? AfxGetFormatStyleTable()->GetFormatIdx(GetDataType()) : nFormatIdx
		);
		break;
	}

	if (!bOk)
	{
		Clear(FALSE);
		return;
	}

	Assign(aDateTime);
}

//-----------------------------------------------------------------------------
void DataDate::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	Assign(*(DBTIMESTAMP*)aRDEData.GetData());
}

//-----------------------------------------------------------------------------
void DataDate::Assign(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataDate)))
	{
		if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
			Assign(((DataStr&)aDataObj).GetString());
		else
			ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataDate)), "Parameter aDataObj must be of type DataDate");

		return;
	}

	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataDate& aDataDate = (DataDate&)aDataObj;

	// illegali: Time ::= Date e 	Date ::= Time
	if (IsATime() && !aDataDate.IsFullDate() || !IsFullDate() && aDataDate.IsATime())
	{
		ASSERT_TRACE(FALSE, "Assigning a date to a time or vice versa is illegal");
		return;
	}

	if (!aDataDate.IsEmpty())
	{
		DBTIMESTAMP oldStruct;
		memcpy(&oldStruct, &m_DateStruct, sizeof(m_DateStruct));

		if (!IsATime())	// Date or DateTime
		{
			if (!aDataDate.IsATime())	// Date or DateTime ::= Date or DateTime
			{
				m_DateStruct.day = aDataDate.m_DateStruct.day;
				m_DateStruct.month = aDataDate.m_DateStruct.month;
				m_DateStruct.year = aDataDate.m_DateStruct.year;
			}

			if (!IsFullDate())	// Date ::= Date or DateTime
			{
				// annulla la parte ora
				m_DateStruct.hour = MIN_HOUR;
				m_DateStruct.minute = MIN_MINUTE;
				m_DateStruct.second = MIN_SECOND;
			}
			else	// DateTime
				if (aDataDate.IsFullDate())	// DateTime ::= DateTime Or Time
				{
					m_DateStruct.hour = aDataDate.m_DateStruct.hour;
					m_DateStruct.minute = aDataDate.m_DateStruct.minute;
					m_DateStruct.second = aDataDate.m_DateStruct.second;
				}
		}
		else	// Time
		{
			if (aDataDate.IsATime() || aDataDate.IsEmpty())	// Time ::= Time or NullDateTime
			{
				m_DateStruct.day = aDataDate.m_DateStruct.day;
				m_DateStruct.month = aDataDate.m_DateStruct.month;
				m_DateStruct.year = aDataDate.m_DateStruct.year;
			}
			else	// Time ::= DateTime
			{
				// annulla la parte data
				m_DateStruct.day = MIN_TIME_DAY;
				m_DateStruct.month = MIN_TIME_MONTH;
				m_DateStruct.year = MIN_TIME_YEAR;
			}

			m_DateStruct.hour = aDataDate.m_DateStruct.hour;
			m_DateStruct.minute = aDataDate.m_DateStruct.minute;
			m_DateStruct.second = aDataDate.m_DateStruct.second;
		}

		m_DateStruct.fraction = 0;

		if (memcmp(&oldStruct, &m_DateStruct, sizeof(m_DateStruct) != 0))
			SignalOnChanged();
	}
	else
		Clear();

	AssignStatus(aDataObj);
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
void DataDate::Assign(const long nLongDate)
{
	if (IsValueLocked())
		return;

	SetDate(nLongDate);
}

//-----------------------------------------------------------------------------
void DataDate::Assign(const long nLongDate, const long nTotalSeconds)
{
	if (IsValueLocked())
		return;

	WORD wDay, wMonth, wYear;
	WORD wHour, wMinute, wSecond;

	if (
		!GetShortDate(wDay, wMonth, wYear, nLongDate) ||
		!GetShortTime(wHour, wMinute, wSecond, nTotalSeconds)
		)
	{
		Clear(FALSE);
		return;
	}

	DBTIMESTAMP aDateTime;

	aDateTime.day = wDay;
	aDateTime.month = wMonth;
	aDateTime.year = wYear;

	aDateTime.hour = wHour;
	aDateTime.minute = wMinute;
	aDateTime.second = wSecond;
	aDateTime.fraction = 0;

	Assign(aDateTime);
}

//-----------------------------------------------------------------------------
void DataDate::Assign(BYTE* pBuffer, DBLENGTH lLen)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DBTIMESTAMP oldStruct;
	memcpy(&oldStruct, &m_DateStruct, sizeof(m_DateStruct));

	memcpy(GetOleDBDataPtr(), pBuffer, lLen);
	BOOL bOk = CheckDate(m_DateStruct);

	if (!bOk || IsNullDate(m_DateStruct))
	{
		Clear(bOk);
		return;
	}

	SetValid();
	SetModified();
	SetDirty();

	if (memcmp(&oldStruct, &m_DateStruct, sizeof(m_DateStruct) != 0))
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataDate::Assign(const DBTIMESTAMP& aDateTime)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	ASSERT_TRACE(aDateTime.day && aDateTime.month && aDateTime.year, "Either day, month or year of the date to assign are empty");

	BOOL bOk = CheckDate(aDateTime);

	if (!bOk || IsNullDate(aDateTime))
	{
		Clear(bOk);
		return;
	}

	DBTIMESTAMP oldStruct;
	memcpy(&oldStruct, &m_DateStruct, sizeof(m_DateStruct));

	m_DateStruct.fraction = 0;
	if (IsFullDate())
	{
		m_DateStruct.hour = aDateTime.hour;
		m_DateStruct.minute = aDateTime.minute;
		m_DateStruct.second = aDateTime.second;
	}
	else
	{
		// annulla la parte ora
		m_DateStruct.hour = MIN_HOUR;
		m_DateStruct.minute = MIN_MINUTE;
		m_DateStruct.second = MIN_SECOND;
	}

	if (IsATime())
	{
		// annulla la parte data
		m_DateStruct.day = MIN_TIME_DAY;
		m_DateStruct.month = MIN_TIME_MONTH;
		m_DateStruct.year = MIN_TIME_YEAR;
	}
	else
		if (
			!IsFullDate() ||
			!(
				aDateTime.day == MIN_DAY		&&
				aDateTime.month == MIN_MONTH	&&
				aDateTime.year == MIN_YEAR
				)	// Se la data e` nulla il tempo e` non nullo la data viene ignorata
			)
		{
			m_DateStruct.day = aDateTime.day;
			m_DateStruct.month = aDateTime.month;
			m_DateStruct.year = aDateTime.year;
		}

	SetValid(TRUE);
	SetModified();
	SetDirty();

	if (memcmp(&oldStruct, &m_DateStruct, sizeof(m_DateStruct) != 0))
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataDate::Assign(const CTime& aTime)
{
	if (IsValueLocked())
		return;

	DBTIMESTAMP aDateTime;

	aDateTime.day = (SWORD)aTime.GetDay();
	aDateTime.month = (SWORD)aTime.GetMonth();
	aDateTime.year = (UWORD)aTime.GetYear();

	aDateTime.hour = (SWORD)aTime.GetHour();
	aDateTime.minute = (SWORD)aTime.GetMinute();
	aDateTime.second = (UWORD)aTime.GetSecond();
	aDateTime.fraction = 0;

	Assign(aDateTime);
}

//-----------------------------------------------------------------------------
void DataDate::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(v.vt == VT_DATE, "Parameter v must be of VT_DATE type");
	Assign(COleDateTime(v.date));
}

//-----------------------------------------------------------------------------
void DataDate::Assign(const COleDateTime& dt)
{
	if (IsValueLocked())
		return;

	DBTIMESTAMP aDateTime;

	aDateTime.day = (SWORD)dt.GetDay();
	aDateTime.month = (SWORD)dt.GetMonth();
	aDateTime.year = (UWORD)dt.GetYear();

	aDateTime.hour = (SWORD)dt.GetHour();
	aDateTime.minute = (SWORD)dt.GetMinute();
	aDateTime.second = (UWORD)dt.GetSecond();
	aDateTime.fraction = 0;

	Assign(aDateTime);
}

//-----------------------------------------------------------------------------
void DataDate::AssignFromOwnToken(LPCTSTR pszDateStr)
{
	if (IsValueLocked()) return;

	DWORD dwValue1 = 0;
	DWORD dwValue2 = 0;

	if (ConvertStringToDateTime
	(
		GetDataType(),
		pszDateStr,
		&dwValue1,
		&dwValue2
	) == CONVERT_DATATIME_SUCCEEDED)
		Assign(dwValue1, dwValue2);

	else
		Clear(TRUE);
}


//-----------------------------------------------------------------------------
void DataDate::SetSoapValue(BSTR bstrValue)
{
	CString strValue(bstrValue);

	AssignFromXMLString(strValue);
}

//-----------------------------------------------------------------------------
void DataDate::Clear(BOOL bValid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);

	if (IsNullDate(m_DateStruct))
		return;

	m_DateStruct.day = MIN_DAY;
	m_DateStruct.month = MIN_MONTH;
	m_DateStruct.year = MIN_YEAR;

	m_DateStruct.hour = MIN_HOUR;
	m_DateStruct.minute = MIN_MINUTE;
	m_DateStruct.second = MIN_SECOND;
	m_DateStruct.fraction = 0;

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataDate::SetLowerValue(int)
{
	if (IsValueLocked()) return;

	if (!IsEmpty()) return;

	Assign(DataDate::MINVALUE);
}

//-----------------------------------------------------------------------------
void DataDate::SetUpperValue(int)
{
	if (IsValueLocked()) return;

	if (!IsEmpty()) return;

	Assign(DataDate::MAXVALUE);
}

//-----------------------------------------------------------------------------
BOOL DataDate::IsLowerValue() const
{
	DataDate aDate(DataDate::MINVALUE);

	if (this->IsATime())
		aDate.SetAsTime();

	return *this == aDate;
}

//-----------------------------------------------------------------------------
BOOL DataDate::IsUpperValue() const
{
	DataDate aDate(DataDate::MAXVALUE);

	if (this->IsATime())
		aDate.SetAsTime();

	return *this == aDate;
}

//-----------------------------------------------------------------------------
void DataDate::SetTodayDate()
{
	if (IsValueLocked()) return;

	ASSERT_TRACE(!IsATime(), "The DataObj cannot be a time to allow this operation");

	SetDate(::TodayDate());
}

//-----------------------------------------------------------------------------
void DataDate::SetTodayTime()
{
	if (IsValueLocked()) return;

	ASSERT_TRACE(IsFullDate(), "The DataObj must be a full date to allow this operation");

	SetTime(::TodayTime());
}

//-----------------------------------------------------------------------------
void DataDate::SetTodayDateTime()
{
	if (IsValueLocked()) return;

	ASSERT_TRACE(IsFullDate() && !IsATime(), "The DataObj must be a full date and not a time to allow this operation");

	Assign(::TodayDate(), ::TodayTime());
}

//-----------------------------------------------------------------------------
void DataDate::SetWeekStartDate()
{
	int year = m_DateStruct.year;
	int week = WeekOfYear();

	SetWeekStartDate(year, week);
}

void DataDate::SetWeekStartDate(int year, int week)
{
	SetDate(1, 1, year);

	int nDay = DayOfWeek();

	if (nDay > 3)
		*this += (7 - nDay);
	else
		*this -= nDay;

	*this += (week - 1) * 7;
}

//-----------------------------------------------------------------------------
BOOL DataDate::SetDate(const long nLongDate)
{
	if (IsValueLocked()) return FALSE;

	ASSERT_TRACE(!IsATime(), "The DataObj cannot be a time to allow this operation");

	WORD day, month, year;
	if (GetShortDate(day, month, year, nLongDate))
		return SetDate(day, month, year);

	Clear(FALSE);
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL DataDate::SetDate(const UWORD wDay, const UWORD wMonth, const SWORD wYear)
{
	Signal(ON_CHANGING);
	if (IsValueLocked()) return FALSE;

	ASSERT_TRACE(!IsATime(), "The DataObj must not be a time to allow this operation");

	if (
		(
			wDay == MIN_DAY		&&
			wMonth == MIN_MONTH	&&
			wYear == MIN_YEAR
			) ||
			(
				wYear > MIN_YEAR && wYear <= MAX_YEAR &&
				wMonth >= 1 && wMonth <= 12 &&
				wDay >= 1 &&
				wDay <= ::MonthDays(wMonth, wYear)
				)
		)
	{
		DBTIMESTAMP oldStruct;
		memcpy(&oldStruct, &m_DateStruct, sizeof(m_DateStruct));

		m_DateStruct.day = wDay;
		m_DateStruct.month = wMonth;
		m_DateStruct.year = wYear;

		if (
			wDay == MIN_DAY		&&
			wMonth == MIN_MONTH	&&
			wYear == MIN_YEAR
			)
		{
			m_DateStruct.hour = MIN_HOUR;
			m_DateStruct.minute = MIN_MINUTE;
			m_DateStruct.second = MIN_SECOND;
		}

		SetValid(TRUE);
		SetModified();
		SetDirty();

		if (memcmp(&oldStruct, &m_DateStruct, sizeof(m_DateStruct) != 0))
			SignalOnChanged();
		return TRUE;
	}

	Clear(FALSE);
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL DataDate::SetTime(const long nTotalSeconds)
{
	if (IsValueLocked()) return FALSE;

	ASSERT_TRACE(IsFullDate(), "The DataObj must be a full date to allow this operation");

	WORD hour, minute, second;
	if (GetShortTime(hour, minute, second, nTotalSeconds))
		return SetTime(hour, minute, second);

	Clear(FALSE);
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL DataDate::SetTime(const UWORD wHour, const UWORD wMin, const UWORD wSec)
{
	Signal(ON_CHANGING);
	if (IsValueLocked()) return FALSE;

	ASSERT_TRACE(IsFullDate(), "The DataObj must be a full date to allow this operation");

	if (wHour >= 0 && wHour <= 23 && wMin >= 0 && wMin <= 59 && wSec >= 0 && wSec <= 59)
	{
		DBTIMESTAMP oldStruct;
		memcpy(&oldStruct, &m_DateStruct, sizeof(m_DateStruct));

		m_DateStruct.fraction = 0;

		if (IsATime())
		{
			// "annulla" la parte data
			m_DateStruct.day = MIN_TIME_DAY;
			m_DateStruct.month = MIN_TIME_MONTH;
			m_DateStruct.year = MIN_TIME_YEAR;
		}

		// a questo punto si puo` valorizzare la parte ora
		// questo codice impedisce l'assegnazione di un hh/mm/ss se la
		// data è nulldate. Non sappiamo perchè, il controllo risale a . 
		// i tempi di MagoXp
		if (IsNullDate(m_DateStruct))
			return FALSE;

		m_DateStruct.hour = wHour;
		m_DateStruct.minute = wMin;
		m_DateStruct.second = wSec;

		SetValid(TRUE);
		SetModified();
		SetDirty();

		if (memcmp(&oldStruct, &m_DateStruct, sizeof(m_DateStruct) != 0))
			SignalOnChanged();

		return TRUE;
	}

	Clear(FALSE);
	return FALSE;
}

//-----------------------------------------------------------------------------
DataDate DataDate::AddTime(const long wDays, const long wMonths, const long wYears, const long wHours, const long wMinutes, const long wSeconds) const
{
	long day = m_DateStruct.day;
	long month = m_DateStruct.month;
	long year = m_DateStruct.year;
	long hour = m_DateStruct.hour;
	long minute = m_DateStruct.minute;
	long second = m_DateStruct.second;

	long gDay = GiulianDate();

	// Add Seconds
	second += wSeconds;

	minute += second / 60;

	if (abs(second) >= 60)
		second = second % 60;
	if (second < 0)
	{
		second += 60;
		minute--;
	}

	// Add Minutes
	minute += wMinutes;
	hour += minute / 60;

	if (abs(minute) >= 60)
		minute = minute % 60;
	if (minute < 0)
	{
		minute += 60;
		hour--;
	}

	// Add Hours
	hour += wHours;
	gDay += hour / 24;
	if (abs(hour) >= 24)
		hour = hour % 24;
	if (hour < 0)
	{
		hour += 24;
		gDay--;
	}

	// Add Days
	gDay += wDays;
	DataDate tmp = DataDate(gDay);
	day = tmp.Day();
	month = tmp.Month();
	year = tmp.Year();

	// Add Month
	month += wMonths;
	if (abs(month) > 12)
	{
		year += month / 12;
		month = month % 12;
	}
	if (month <= 0)
	{
		month += 12;
		year--;
	}

	year += wYears;

	if (day > ::MonthDays((short)month, (short)year))
		day = ::MonthDays((short)month, (short)year);
	//----  ----

	DataDate dateTmp((short)day, (short)month, (short)year, (short)hour, (short)minute, (short)second);

	if (IsFullDate())
		dateTmp.SetFullDate(TRUE);
	else if (IsATime())
		dateTmp.SetAsTime(TRUE);
	else if (hour || minute || second)
		dateTmp.SetFullDate(TRUE);

	return dateTmp;
}

//-----------------------------------------------------------------------------
DataDate  DataDate::AddSeconds(const long wSec)  const
{
	return AddTime(0, 0, 0, 0, 0, wSec);
}

//-----------------------------------------------------------------------------
DataDate  DataDate::AddMinutes(const long wMin)  const
{
	return AddTime(0, 0, 0, 0, wMin, 0);
}

//-----------------------------------------------------------------------------
DataDate  DataDate::AddHours(const long wHour)  const
{
	return AddTime(0, 0, 0, wHour, 0, 0);
}

//-----------------------------------------------------------------------------
DataDate  DataDate::AddDays(const long nDay)  const
{
	return AddTime(nDay, 0, 0, 0, 0, 0);
}

//-----------------------------------------------------------------------------
DataDate  DataDate::AddMonths(const long nMonth)  const
{
	return AddTime(0, nMonth, 0, 0, 0, 0);
}

//-----------------------------------------------------------------------------
DataDate  DataDate::AddYears(const long nYear) const
{
	return AddTime(0, 0, nYear, 0, 0, 0);
}

//-----------------------------------------------------------------------------
/*static */DataDate DataDate::EasterSunday(SWORD year)
{
	int day = 0;
	int month = 0;

	int g = year % 19;
	int c = year / 100;
	int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
	int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

	day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
	month = 3;

	if (day > 31)
	{
		month++;
		day -= 31;
	}
	return DataDate(day, month, year);
}

//-----------------------------------------------------------------------------
int DataDate::WeekOfMonth(int alg /*= 0*/) const
{
	if (alg == 1)
		return ::WeekOfMonthISO(*this);
	return ::WeekOfMonth(m_DateStruct);
}

//============================================================================================
// XML Data Type: dateTime
// Date in a subset of ISO 8601 format, with optional time and no optional zone. 
// Fractional seconds can be as precise as nanoseconds. 
// For example, "1988-04-07T18:39:09".
//
// ISO 8601 "Data elements and interchange formats - Information interchange - Representation of dates and times"
// The ISO 8601 date/time format is YYYYMMDDTHHMMSSZ, where the T indicates the 
// split between date and time, and the optional Z indicates that the event uses the 
// Universal Coordinated Time (UTC) zone, or Greenwich Mean Time. 
//============================================================================================
//============================================================================================
// XML Data Type: date
// Date in a subset ISO 8601 format, without the time data. 
// For example: "1994-11-05"
//============================================================================================
//============================================================================================
// XML Data Type: time
// Time in a subset ISO 8601 format, with no date and no time zone.
// For example: "08:15:27".

//============================================================================================
CString DataDate::GetXMLType(BOOL bSoapType) const
{
	if (bSoapType)
		return SCHEMA_XSD_DATATYPE_DATETIME_VALUE;

	if (IsFullDate())
	{
		if (IsATime())
			return SCHEMA_XSD_DATATYPE_TIME_VALUE;

		return SCHEMA_XSD_DATATYPE_DATETIME_VALUE;
	}
	return SCHEMA_XSD_DATATYPE_DATE_VALUE;
}

//============================================================================================
CString DataDate::FormatDataForXML(BOOL bSoapType) const
{
	// XML Data Type: dateTime compatibile con Soap
	if (bSoapType)
		return FormatDateTimeForXML(m_DateStruct, bSoapType);

	if (IsFullDate())
	{
		if (IsATime()) // XML Data Type: time
			return FormatTimeForXML(m_DateStruct);

		// XML Data Type: dateTime
		return FormatDateTimeForXML(m_DateStruct, bSoapType);
	}
	// XML Data Type: date
	return FormatDateForXML(m_DateStruct);
}

//-----------------------------------------------------------------------------
void DataDate::AssignFromXMLString(LPCTSTR lpszValue)
{
	AssignFromOwnToken(lpszValue);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataDate::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataDate:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_wDay= ", m_DateStruct.day);
	AFX_DUMP1(dc, "\n\tm_wMonth= ", m_DateStruct.month);
	AFX_DUMP1(dc, "\n\tm_wYear= ", m_DateStruct.year);
}

void DataDate::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG




//////////////////////////////////////////////////////////////////////////////
//          DataEnum class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataEnum, DataObj)

//-----------------------------------------------------------------------------
DataEnum::DataEnum(WORD wTagValue, WORD wItemValue)
	:
	m_dwValue(GET_TI_VALUE(wItemValue, wTagValue))
{
	if
		(
		(
			(wTagValue == 0 && wItemValue != 0)
#ifdef _DEBUG
			||
			!AfxGetEnumsTable()->ExistEnumValue(m_dwValue)
#endif
			)
			)
	{
		ASSERT_TRACE2(FALSE, "value with wrong value: {%d:%d}", wTagValue, wItemValue);

		if (AfxGetEnumsTable()->ExistEnumTagValue(wTagValue))
		{
			WORD wItem = AfxGetEnumsTable()->GetEnumDefaultItemValue(wTagValue);
			m_dwValue = GET_TI_VALUE(wItem, wTagValue);
		}
		else
			SetValid(FALSE);
	}

	m_wBirthTag = wTagValue;

	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataEnum::DataEnum(DWORD dwValue)
	:
	m_dwValue(dwValue)
{

	if
		(
			m_dwValue != 0 &&
			(
				GET_TAG_VALUE(m_dwValue) == 0
#ifdef _DEBUG
				||
				!AfxGetEnumsTable()->ExistEnumValue(m_dwValue)
#endif
				)
			)
	{
		ASSERT_TRACE1(FALSE, "value with wrong value: %d", m_dwValue);

		WORD wTag = GET_TAG_VALUE(dwValue) != 0 ? GET_TAG_VALUE(dwValue) : GET_ITEM_VALUE(dwValue);
		m_dwValue = GET_TI_VALUE(0, wTag);
		if
			(
				wTag != 0 &&
				!AfxGetEnumsTable()->ExistEnumValue(m_dwValue) &&
				AfxGetEnumsTable()->ExistEnumTagValue(wTag)
				)
		{
			WORD wItem = AfxGetEnumsTable()->GetEnumDefaultItemValue(wTag);
			dwValue = GET_TI_VALUE(wItem, wTag);
		}
		SetValid(AfxGetEnumsTable()->ExistEnumValue(m_dwValue));
	}

	m_wBirthTag = GetTagValue();
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataEnum::DataEnum(const DataEnum& aDataEnum)
	:
	m_dwValue(aDataEnum.m_dwValue)
{
	m_wBirthTag = aDataEnum.m_wBirthTag;

	AssignStatus(aDataEnum);
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
int DataEnum::IsEqual(const DataObj& aDataObj) const
{
	if (
		aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)) &&
		GetTagValue() == ((DataEnum&)aDataObj).GetTagValue()
		)
		return GetItemValue() == ((DataEnum&)aDataObj).GetItemValue();

	ASSERT_TRACE1(FALSE, "Either the parameter DataObj is not of enum type or its tag value is not %d", GetTagValue());
	return FALSE;
}

//-----------------------------------------------------------------------------
int DataEnum::IsLessThan(const DataObj& aDataObj) const
{
	if (
		aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)) &&
		GetTagValue() == ((DataEnum&)aDataObj).GetTagValue()
		)
		return GetItemValue() < ((DataEnum&)aDataObj).GetItemValue();

	ASSERT_TRACE1(FALSE, "Either the parameter DataObj is not of enum type or its tag value is not %d", GetTagValue());
	return FALSE;
}

//-----------------------------------------------------------------------------
void* DataEnum::GetRawData(DataSize* pDataSize) const
{
	// TRICS to optimize speed on RDEManager

	if (pDataSize)
		*pDataSize = sizeof(m_dwValue);

	return (void*)&m_dwValue;
}

//-----------------------------------------------------------------------------
CString DataEnum::Str(int nLen, int nPrec) const
{
	CString strName(AfxGetEnumsTable()->GetEnumItems(GetTagValue())->GetEnumItemName(GetItemValue()));

	if (nLen < 0)
		return strName;

	if (nLen == 0)
		if (nPrec == 0)
			return cwsprintf(_T("%08X"), m_dwValue);
		else
			return cwsprintf(_T("%04X"), GET_ITEM_VALUE(m_dwValue));

	return cwsprintf(_T("%-*.*s"), nLen, nLen, (LPCTSTR)strName);
}

//-----------------------------------------------------------------------------
CString DataEnum::ToString() const
{
	return cwsprintf(_T("{%d:%d}"), GET_TAG_VALUE(m_dwValue), GET_ITEM_VALUE(m_dwValue));
}
//-----------------------------------------------------------------------------
void DataEnum::SerializeJsonValue(CJsonSerializer& jsonSerializer)
{
	jsonSerializer.WriteString(szValue, FormatDataForXML());
	jsonSerializer.WriteInt(szTag, GetDataType().m_wTag);
}
//-----------------------------------------------------------------------------
void DataEnum::AssignJsonValue(CJsonParser& jsonParser)
{
	CString value = jsonParser.ReadString(szValue);
	AssignFromXMLString(value);
}
//-----------------------------------------------------------------------------
void DataEnum::Assign(LPCTSTR pszEnumStr)
{
	if (IsValueLocked())
		return;

	if (!pszEnumStr || !*pszEnumStr)
	{
		ASSERT_TRACE(pszEnumStr && *pszEnumStr, "Parameter pszEnumStr either null or empty");
		return;
	}

	DWORD dwValue;
	int ret = _stscanf_s(pszEnumStr, _T("%X"), &dwValue);

	if (ret == 1)
	{
		if (_tcslen(pszEnumStr) == 8)
			Assign(dwValue);
		else
			Assign(GET_TAG_VALUE(m_dwValue), GET_ITEM_VALUE(dwValue));
		return;
	}
	/*
		CString s(pszEnumStr);
		s.Trim();
		if (s[0] == '{')
		{
			//TODO { tag : iem }

			ASSERT(FALSE);
			return;
		}
	*/
}

//-----------------------------------------------------------------------------
void DataEnum::AssignByTitle(CString sEnumStr)
{
	if (IsValueLocked())
		return;

	sEnumStr.Trim();
	if (sEnumStr.IsEmpty())
	{
		ASSERT_TRACE(FALSE, "Parameter sEnumStr is empty");
		return;
	}

	ASSERT(this->GetTagValue());
	const EnumItemArray* pEnumItemArray = GetEnumItems();
	ASSERT_VALID(pEnumItemArray);
	if (!pEnumItemArray)
		return;
	for (int i = 0; i <= pEnumItemArray->GetUpperBound(); i++)
	{
		EnumItem* pItem = pEnumItemArray->GetAt(i);
		if (sEnumStr.CompareNoCase(pItem->GetTitle()) == 0)
		{
			Assign(this->GetTagValue(), pItem->GetItemValue());
			return;
		}
	}
}

//-----------------------------------------------------------------------------
void DataEnum::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	Assign(*(DWORD*)aRDEData.GetData());
}

//-----------------------------------------------------------------------------
void DataEnum::Assign(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)))
	{
		if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
			Assign(((DataStr&)aDataObj).GetString());
		else
			ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)), _T("Parameter aDataObj must be of DataEnum type"));

		return;
	}

	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataEnum& aDataEnum = (DataEnum&)aDataObj;

#ifdef _DEBUG
	if (GetTagValue() != 0 && GetTagValue() != aDataEnum.GetTagValue())
	{
		ASSERT_TRACE(GetTagValue() == 0 || GetTagValue() == aDataEnum.GetTagValue(),
			cwsprintf(_T("\nDifferent TagValue. Trying to assign (Value = %s, TagName = %s) to (Value = %s, TagName = %s)\n"),
				aDataEnum.FormatData(), aDataEnum.GetTagName(), FormatData(), GetTagName()));
	}
#endif

	BOOL bChanged = m_dwValue != aDataEnum.m_dwValue;

	if (bChanged)
		m_dwValue = aDataEnum.m_dwValue;

	AssignStatus(aDataEnum);
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataEnum::Assign(DWORD dwValue)
{
	if (IsValueLocked())
		return;

	Assign(GET_TAG_VALUE(dwValue), GET_ITEM_VALUE(dwValue));
}

//-----------------------------------------------------------------------------
void DataEnum::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(v.vt == VT_UI4, "Parameter v must be of VT_UI4 type");
	Assign(v.ulVal);
}

//-----------------------------------------------------------------------------
void DataEnum::Assign(WORD wTagValue, WORD wItemValue)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	ASSERT_TRACE(GetTagValue() == 0 || GetTagValue() == wTagValue, cwsprintf(_T("Different TagValue. Trying to assign (Value = %d, TagValue = %d) to (Value = %s, TagName = %s"), ((int)wItemValue), ((int)wTagValue), FormatData(), GetTagName()));
	ASSERT_TRACE2(wItemValue != 0 ? wTagValue != 0 : TRUE, "Parameter wTagValue cannot be zero if wItemValue is nonzero: wTagValue = %d, wItemValue = %d", wTagValue, wItemValue);

	DWORD dNewValue = GET_TI_VALUE(wItemValue, wTagValue);

	SetValid();

	if
		(
			dNewValue != 0 &&
			(
				wTagValue == 0
#ifdef _DEBUG
				||
				!AfxGetEnumsTable()->ExistEnumValue(dNewValue)
#endif
				)
			)
	{
		//ASSERT_TRACE2(FALSE,"Value with wrong value: {%d:%d}", wTagValue, wItemValue);

		if (wTagValue != 0 && AfxGetEnumsTable()->ExistEnumTagValue(wTagValue))
		{
			WORD wItem = AfxGetEnumsTable()->GetEnumDefaultItemValue(wTagValue);
			dNewValue = GET_TI_VALUE(wItem, wTagValue);
		}
		else
			SetValid(FALSE);
	}

	BOOL bChanged = dNewValue != m_dwValue;
	if (bChanged)
		m_dwValue = dNewValue;

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataEnum::Clear(BOOL bValid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);

	// MUST don't inizialize TagValue;
	WORD wTagValue = GetTagValue();
	WORD wItemValue = AfxGetEnumsTable()->GetEnumDefaultItemValue(wTagValue);

	DWORD dNewValue = GET_TI_VALUE(wItemValue, wTagValue);
	if (dNewValue == m_dwValue)
		return;

	m_dwValue = dNewValue;

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
CString DataEnum::GetTagName() const
{
	return AfxGetEnumsTable()->GetEnumTagName(GetTagValue());
}

//-----------------------------------------------------------------------------
const EnumItemArray* DataEnum::GetEnumItems() const
{
	return AfxGetEnumsTable()->GetEnumItems(GetTagValue());
}

//-----------------------------------------------------------------------------
BOOL DataEnum::IsEmpty() const
{
	return GetItemValue() == AfxGetEnumsTable()->GetEnumDefaultItemValue(GetTagValue());
}

//-----------------------------------------------------------------------------
CString DataEnum::GetXMLType(BOOL bSoapType) const
{
	if (bSoapType)
		return SCHEMA_XSD_DATATYPE_UINT_VALUE;

	return SCHEMA_XSD_DATATYPE_STRING_VALUE;
}

// occorre considerarlo unsigned per compatibilitá con TBC#
//-----------------------------------------------------------------------------
CString DataEnum::FormatDataForXML(BOOL bSoapType) const
{
	if (bSoapType)
		return cwsprintf(_T("%u"), GetValue());

	return  GetTagName() + szSeparator + GetEnumItems()->GetEnumItemName(GetItemValue());
}

//-----------------------------------------------------------------------------
void DataEnum::AssignFromXMLString(LPCTSTR lpszValue)
{
	CString strToParse = lpszValue;
	if (strToParse.IsEmpty())
	{
		Clear();
		return;
	}

	int pos = strToParse.Find(szSeparator);
	if (pos == -1)
		Assign((DWORD)_tstol(lpszValue));
	else
	{
		CString strTagValue, strItemValue;
		strTagValue = strToParse.Left(pos);
		strItemValue = strToParse.Mid(pos + 2);//sizeof szSeperator

		WORD wTagValue = AfxGetEnumsTable()->GetEnumTagValue(strTagValue);
		WORD wItemValue = AfxGetEnumsTable()->GetEnumItemValue(strTagValue, strItemValue);
		Assign(wTagValue, wItemValue);
	}
}

//-----------------------------------------------------------------------------
/*static*/ WORD DataEnum::GetTagValue(CString sValue)
{
	if (sValue.IsEmpty())
		return 0;

	DWORD dwValue = 0;
	TRY{ dwValue = (DWORD)_tstol(sValue.GetBuffer(0));	sValue.ReleaseBuffer(); }

		CATCH(CException, e)
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		TRACE2("DataEnum::GetTagValue called with %s value has thrown the following exception: %s", sValue, szError);
	}
	END_CATCH

		return GET_TAG_VALUE(dwValue);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataEnum::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataEnum:");
	dc << _T("\n\tm_wDataStatus = ") << m_wDataStatus << _T(", m_dwValue = ") << m_dwValue << _T(", { ") << (m_dwValue & 0xFFFF) << _T(" : ") << (m_dwValue >> 16) << _T(" }");
}

void DataEnum::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG

//============================================================================
//          DataGuid class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataGuid, DataObj)
//-----------------------------------------------------------------------------
DataGuid::DataGuid()
	:
	m_guid(NULL_GUID)
{
}

//-----------------------------------------------------------------------------
DataGuid::DataGuid(const GUID& guid)
	:
	m_guid(guid)

{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataGuid::DataGuid(LPCTSTR lpszGUID)
{
	Assign(lpszGUID);
}

//-----------------------------------------------------------------------------
DataGuid::DataGuid(const DataGuid& aDataGuid)
	:
	m_guid(aDataGuid.m_guid)

{
	AssignStatus(aDataGuid);
	SetModified();
	SetDirty();
}



//-----------------------------------------------------------------------------
void DataGuid::Clear(BOOL bValid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);
	if (IsEmpty())
		return;

	m_guid = NULL_GUID;

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
BOOL DataGuid::IsEmpty()  const
{
	return ::IsNullGUID(m_guid);
}

// confronto anche con datastr
//-----------------------------------------------------------------------------
int DataGuid::IsEqual(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataGuid)))
		return IsEqualGUID(m_guid, ((DataGuid&)aDataObj).m_guid);

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		GUID aGuid = NULL_GUID;
		if (IsGUIDStringValid(((DataStr&)aDataObj).GetString(), FALSE, &aGuid))
			return IsEqualGUID(m_guid, aGuid);
	}

	ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataGuid or DataStr type");
	return FALSE;
}

//-----------------------------------------------------------------------------
int DataGuid::IsEqual(const GUID& guid)	const
{
	return m_guid == guid;
}

//-----------------------------------------------------------------------------
int DataGuid::IsEqual(LPCTSTR lpszGuid)	const
{
	GUID aGuid = NULL_GUID;
	if (IsGUIDStringValid(lpszGuid, FALSE, &aGuid))
		return IsEqualGUID(m_guid, aGuid);

	return FALSE;
}


//-----------------------------------------------------------------------------
int DataGuid::IsLessThan(const DataObj& aDataObj) const
{
	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataGuid)))
		return _tcsicmp(Str(), aDataObj.Str()) < 0;

	return FALSE;
}

//-----------------------------------------------------------------------------
void* DataGuid::GetRawData(DataSize* pDataSize)	const
{
	// TRICS to optimize speed on RDEManager
	if (pDataSize)
		*pDataSize = sizeof(m_guid);

	return (void*)&m_guid;
}

//-----------------------------------------------------------------------------
CString DataGuid::Str(int bWithoutCurlyBraces, int /*nParam2*/) const
{
	return GetGUIDString(m_guid, (bWithoutCurlyBraces > 0) ? TRUE : FALSE);
}

//-----------------------------------------------------------------------------
CString DataGuid::ToString() const
{
	return L"\"" + Str() + '\"';
}

//-----------------------------------------------------------------------------
void DataGuid::Assign(LPCTSTR lpszGUID)
{
	if (IsValueLocked())
		return;

	if (!IsGUIDStringValid(lpszGUID, FALSE, &m_guid))
	{
		Clear(FALSE);
		return;
	}

	SetValid();
	SetModified();
	SetDirty();
}

//@@TODO da vedere con Germano
//-----------------------------------------------------------------------------
void DataGuid::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	Assign(*(GUID*)aRDEData.GetData());
}

//-----------------------------------------------------------------------------
void DataGuid::Assign(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataGuid)))
	{
		if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
			Assign(((DataStr&)aDataObj).GetString());
		else
			ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataGuid)), "Parameter aDataObj must be of DataGuid type");

		return;
	}

	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataGuid& aDataGuid = (DataGuid&)aDataObj;
	BOOL bChanged = !IsEqualGUID(m_guid, aDataGuid.m_guid);

	if (bChanged)
		m_guid = aDataGuid.m_guid;

	AssignStatus(aDataGuid);
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}


//-----------------------------------------------------------------------------
void DataGuid::Assign(const GUID& aGuid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	BOOL bChanged = !IsEqualGUID(m_guid, aGuid);
	if (bChanged)
		m_guid = aGuid;

	SetValid();
	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataGuid::AssignNewGuid()
{
	if (IsValueLocked()) return;

	GUID aGuid;
	::CoCreateGuid(&aGuid);
	Assign(aGuid);
}

//-----------------------------------------------------------------------------
//============================================================================================
// XML Data Type: string
// with CurlyBraces
//============================================================================================
CString DataGuid::FormatDataForXML(BOOL) const
{
	return Str();
}

//-----------------------------------------------------------------------------
void DataGuid::AssignFromXMLString(LPCTSTR lpszValue)
{
	Assign(lpszValue);
}


/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataGuid::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataGuid:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tm_guid= ", GetGUIDString(m_guid));
}

void DataGuid::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG



//============================================================================
//          DataText class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataText, DataStr)

//-----------------------------------------------------------------------------
DataText::DataText()
{
}

//-----------------------------------------------------------------------------
DataText::DataText(LPCTSTR pszString)
	:
	DataStr(pszString)
{
}

//-----------------------------------------------------------------------------
DataText::DataText(const DataText& aDataText)
{
	Assign(aDataText);
}

//-----------------------------------------------------------------------------
void DataText::AppendFromSStream(BYTE* pBuf, int nSize, BOOL bUnicode, BOOL bSet /*=FALSE*/)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	BOOL bChanged = pBuf != NULL && nSize > 0;

	pBuf[nSize] = _T('\0');

	if (bChanged)
	{
		if (bUnicode)
			m_strText += (TCHAR*)pBuf;
		else
			m_strText += (char*)pBuf;
	}

	if (bSet)
	{
		if (bChanged && ((m_wDataStatus & UPPERCASE) == UPPERCASE))
			this->MakeUpper(FALSE);

		SetValid();
		SetModified();
		SetDirty();
	}

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataText::Assign(const DataObj& aDataObj)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataText)) && !aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		ASSERT_TRACE(FALSE, "Parameter aDataObj must be of DataText or DataStr type");
		return;
	}

	BOOL bChanged = aDataObj.Str() != m_strText;
	if (bChanged)
	{
		m_strText = aDataObj.Str();
	}

	AssignStatus(aDataObj);
	SetModified();
	SetDirty();

	if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
		this->MakeUpper(!bChanged);	//devo segnalare se la makeupper modifica la stringa

	if (bChanged)
		SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataText::Assign(LPCTSTR pszString)
{
	DataStr::Assign(pszString);
}

//-----------------------------------------------------------------------------
void DataText::Assign(TCHAR ch)
{
	DataStr::Assign(ch);
}

//-----------------------------------------------------------------------------
const DataText& DataText::operator+=(const DataText& datatxt)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return *this;

	BOOL bChanged = !datatxt.m_strText.IsEmpty();

	if (bChanged)
	{
		m_strText += datatxt.m_strText;
		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();

	return *this;
}

//-----------------------------------------------------------------------------
const DataText& DataText::operator+=(const DataStr& datastr)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return *this;

	BOOL bChanged = !datastr.IsEmpty();

	if (bChanged)
	{
		m_strText += datastr.GetString();
		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();

	return *this;
}

//-----------------------------------------------------------------------------
const DataText& DataText::operator+=(const CString& string)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return *this;

	BOOL bChanged = !string.IsEmpty();

	if (bChanged)
	{
		m_strText += string;
		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();

	return *this;
}

//-----------------------------------------------------------------------------
const DataText& DataText::operator+=(TCHAR ch)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return *this;

	BOOL bChanged = ch != NULL;
	if (bChanged)
	{
		m_strText += ch;
		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();

	return *this;
}

//-----------------------------------------------------------------------------
const DataText& DataText::operator+=(LPCTSTR psz)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return *this;

	BOOL bChanged = psz != NULL && psz[0] != NULL;

	if (bChanged)
	{
		m_strText += psz;

		if ((m_wDataStatus & UPPERCASE) == UPPERCASE)
			this->MakeUpper(FALSE);
	}

	SetModified();
	SetDirty();

	if (bChanged)
		SignalOnChanged();
	return *this;
}


//-----------------------------------------------------------------------------
DataText operator+(const DataText& datatxt1, const DataText& datatxt2)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataText dest(datatxt1.m_strText);
	dest.m_strText += datatxt2.m_strText;

	return dest;
}

//-----------------------------------------------------------------------------
DataText operator+(const DataText& datatxt, const DataStr& datastr)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataText dest(datatxt.m_strText);
	dest.m_strText += datastr.GetString();

	return dest;
}
//-----------------------------------------------------------------------------
DataText operator+(const DataStr& datastr, const DataText& datatxt)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataText dest(datastr.GetString());
	dest.m_strText += datatxt.m_strText;

	return dest;
}

//-----------------------------------------------------------------------------
DataText operator+(const DataText& datatxt, const CString& string)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataText dest(datatxt.m_strText);
	dest += string;

	return dest;
}

//-----------------------------------------------------------------------------
DataText operator+(const CString& string, const DataText& datatxt)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataText dest(string);
	dest += datatxt;

	return dest;
}

//-----------------------------------------------------------------------------
DataText operator+(const DataText& datatxt, TCHAR ch)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataText dest(datatxt.m_strText);
	dest += ch;

	return dest;
}

//-----------------------------------------------------------------------------
DataText operator+(TCHAR ch, const DataText& datatxt)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	return DataText(ch + datatxt.m_strText);
}

//-----------------------------------------------------------------------------
DataText operator+(const DataText& datatxt, LPCTSTR psz)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	DataText dest(datatxt.m_strText);
	dest += psz;

	return dest;
}

//-----------------------------------------------------------------------------
DataText operator+(LPCTSTR psz, const DataText& datatxt)
{
	// Non deve ereditare l'attributo di uppercase da nessuno dei due operandi
	//
	return DataText(psz + datatxt.m_strText);
}

//-----------------------------------------------------------------------------
BOOL operator==(const DataText& s1, const DataText& s2)
{
	return s1.IsEqual(s2);
}

//-----------------------------------------------------------------------------
BOOL operator==(const DataText& s1, const DataStr& s2)
{
	return s1.IsEqual(s2);
}

//-----------------------------------------------------------------------------
BOOL operator==(const DataStr& s1, const DataText& s2)
{
	return s1.IsEqual(s2);
}

//-----------------------------------------------------------------------------
BOOL operator==(const DataText& s1, const CString& s2)
{
	return s1.IsEqual(DataText(s2));
}

//-----------------------------------------------------------------------------
BOOL operator==(const CString& s1, const DataText& s2)
{
	return s2 == s1;
}

//-----------------------------------------------------------------------------
BOOL operator==(const DataText& s1, LPCTSTR s2)
{
	return s1.IsEqual(DataText(s2));
}

//-----------------------------------------------------------------------------
BOOL operator==(LPCTSTR s1, const DataText& s2)
{
	return s2 == s1;
}

//-----------------------------------------------------------------------------
BOOL operator<(const DataText& s1, const DataText& s2)
{
	return s1.IsLessThan(s2);
}

//-----------------------------------------------------------------------------
BOOL operator<(const DataText& s1, const DataStr& s2)
{
	return s1.IsLessThan(s2);
}

//-----------------------------------------------------------------------------
BOOL operator<(const DataStr& s1, const DataText& s2)
{
	return s1.IsLessThan(s2);
}

//-----------------------------------------------------------------------------
BOOL operator<(const DataText& s1, const CString& s2)
{
	return s1.IsLessThan(DataText(s2));
}

//-----------------------------------------------------------------------------
BOOL operator<(const CString& s1, const DataText& s2)
{
	return !s2.IsLessEqualThan(DataText(s1));
}

//-----------------------------------------------------------------------------
BOOL operator<(const DataText& s1, LPCTSTR s2)
{
	return s1.IsLessThan(DataText(s2));
}

//-----------------------------------------------------------------------------
BOOL operator<(LPCTSTR s1, const DataText& s2)
{
	return !s2.IsLessEqualThan(DataText(s1));
}

//-----------------------------------------------------------------------------
BOOL operator>(const DataText& s1, const DataText& s2)
{
	return !s1.IsLessEqualThan(s2);
}

//-----------------------------------------------------------------------------
BOOL operator>(const DataText& s1, const DataStr& s2)
{
	return !s1.IsLessEqualThan(s2);
}

//-----------------------------------------------------------------------------
BOOL operator>(const DataStr& s1, const DataText& s2)
{
	return !s1.IsLessEqualThan(s2);
}

//-----------------------------------------------------------------------------
BOOL operator>(const DataText& s1, const CString& s2)
{
	return !s1.IsLessEqualThan(DataText(s2));
}

//-----------------------------------------------------------------------------
BOOL operator>(const CString& s1, const DataText& s2)
{
	return s2.IsLessThan(DataText(s1));
}

//-----------------------------------------------------------------------------
BOOL operator>(const DataText& s1, LPCTSTR s2)
{
	return !s1.IsLessEqualThan(DataText(s2));
}

//-----------------------------------------------------------------------------
BOOL operator>(LPCTSTR s1, const DataText& s2)
{
	return s2.IsLessThan(DataText(s1));
}

//============================================================================
//          DataBlob class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataBlob, DataObj)

//-----------------------------------------------------------------------------
DataBlob::DataBlob()
	:
	m_pBuffer(NULL),
	m_nAllocSize(0),
	m_nUsedLen(0)
{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
DataBlob::DataBlob(void* pBuf, int nSize)
	:
	m_pBuffer(NULL),
	m_nAllocSize(nSize),
	m_nUsedLen(nSize)
{
	SetModified();
	SetDirty();

	m_pBuffer = new BYTE[nSize];
	memcpy(m_pBuffer, pBuf, nSize);
}

//-----------------------------------------------------------------------------
DataBlob::DataBlob(const DataBlob& aDataBlob)
	:
	m_pBuffer(NULL),
	m_nAllocSize(0),
	m_nUsedLen(0)
{
	Assign(aDataBlob);
}

//-----------------------------------------------------------------------------
DataBlob::~DataBlob()
{
	if (m_pBuffer)
	{
		delete[] m_pBuffer;
		m_pBuffer = NULL;
	}
}

// Prealloca per gestire bene il bind delle colonne
//-----------------------------------------------------------------------------
void DataBlob::Allocate()
{
	if (m_pBuffer == NULL && m_nAllocSize)
	{
		m_pBuffer = new BYTE[m_nAllocSize];
		memset(m_pBuffer, 0, m_nAllocSize);

		m_nUsedLen = 0;
	}
}
//-----------------------------------------------------------------------------

void DataBlob::SetAllocSize(int nSize)
{
	if (m_pBuffer)
	{
		delete[] m_pBuffer;
		m_pBuffer = NULL;
	}
	m_nUsedLen = 0;
	m_nAllocSize = nSize;
	m_pBuffer = new BYTE[m_nAllocSize];
	memset(m_pBuffer, 0, m_nAllocSize);
}

//-----------------------------------------------------------------------------
void DataBlob::SetBuffer(BYTE* pBuffer, int nUsedLen)
{
	delete m_pBuffer;
	m_pBuffer = pBuffer;
	m_nUsedLen = nUsedLen;
	m_nAllocSize = nUsedLen;
}

//-----------------------------------------------------------------------------
int DataBlob::IsEqual(const DataObj& aDataObj) const
{
#ifdef _DEBUG
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataBlob)))
	{
		ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataBlob)), "Parameter aDataObj must be of DataBlob type");
		return FALSE;
	}
#endif

	ASSERT_TRACE(FALSE, "This feature is not implemented");

	return FALSE;
}

//-----------------------------------------------------------------------------
void* DataBlob::GetRawData(DataSize* pDataSize) const
{
	// TRICS to optimize speed on RDEManager
	if (pDataSize)
		*pDataSize = m_nUsedLen;

	return (void*)(m_pBuffer);
}

//-----------------------------------------------------------------------------
CString DataBlob::Str(int nLen, int) const
{
	ASSERT_TRACE(FALSE, "This feature is not implemented");

	return _T("");
}
//-----------------------------------------------------------------------------
void DataBlob::Assign(LPCTSTR)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(FALSE, "This feature is not implemented");
}

//-----------------------------------------------------------------------------
void DataBlob::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(FALSE, "This feature is not implemented");
}

//-----------------------------------------------------------------------------
CString DataBlob::FormatDataForXML(BOOL) const
{
	int nLength = Base64EncodeGetRequiredLength(m_nUsedLen, ATL_BASE64_FLAG_NONE);
	CStringA s;
	Base64Encode((const BYTE*)m_pBuffer, m_nUsedLen, s.GetBuffer(nLength), &nLength, ATL_BASE64_FLAG_NONE);
	s.ReleaseBuffer();
	return UTF8ToUnicode(s);
}

//-----------------------------------------------------------------------------
void DataBlob::AssignFromXMLString(LPCTSTR lpszValue)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	CStringA sValue = UnicodeToUTF8(lpszValue);
	int nLen = sValue.GetLength();
	int nSize = Base64DecodeGetRequiredLength(nLen);

	BYTE* pBuffer = new BYTE[nSize];
	VERIFY(Base64Decode(sValue, nSize, (BYTE*)pBuffer, &nSize));
	SetBuffer(pBuffer, nSize);

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
SAFEARRAY* DataBlob::GetSoapValue() const
{
	CComSafeArray<BYTE> ar;
	ar.Create(m_nUsedLen);
	memcpy(ar.m_psa->pvData, m_pBuffer, m_nUsedLen);
	return ar.Detach();
}
//-----------------------------------------------------------------------------
void DataBlob::SetSoapValue(SAFEARRAY* pValue)
{
	if (pValue == NULL)
		return;

	CComSafeArray<BYTE> ar;
	ar.Attach(pValue);

	int count = ar.GetCount();
	Assign(pValue->pvData, count);
	ar.Detach();
}

//-----------------------------------------------------------------------------
void DataBlob::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(FALSE, "This feature is not implemented");
}

//-----------------------------------------------------------------------------
void DataBlob::Assign(void* pBuf, int nSize)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	SetModified();
	SetDirty();

	if (m_pBuffer && m_nAllocSize < nSize)
	{
		delete[] m_pBuffer;
		m_pBuffer = NULL;
	}
	if (m_pBuffer == NULL)
	{
		m_pBuffer = new BYTE[nSize];
		m_nAllocSize = nSize;
	}
	m_nUsedLen = nSize;
	memcpy(m_pBuffer, pBuf, nSize);

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataBlob::Assign(const DataObj& aDataObj)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataBlob)))
	{
		ASSERT_TRACE(aDataObj.IsKindOf(RUNTIME_CLASS(DataBlob)), "Parameter aDataObj must be of type DataBlob");
		return;
	}

	AssignStatus(aDataObj);
	SetModified();
	SetDirty();

	const DataBlob& aDataBlob = (DataBlob&)aDataObj;

	if (m_pBuffer && m_nAllocSize < aDataBlob.m_nAllocSize)
	{
		delete[] m_pBuffer;
		m_pBuffer = NULL;
	}
	if (m_pBuffer == NULL)
	{
		m_nAllocSize = aDataBlob.m_nAllocSize;
		m_pBuffer = new BYTE[m_nAllocSize];
		memset(m_pBuffer, 0, m_nAllocSize);
	}
	m_nUsedLen = aDataBlob.m_nAllocSize;
	if (m_nAllocSize)
	{
		memcpy(m_pBuffer, aDataBlob.m_pBuffer, m_nUsedLen);
	}

	SignalOnChanged();
}

// Non bisogna chiamare Empty() perche` non si deve riallocare il 
// buffer interno della stringa. ODBC richiede che il puntatore della colonna bindata
// non cambi, mentre nel caso della stringa se si rialloca se si usa Empty il puntatore
// ai dati cambia (nel caso di Empty diventa NIL)
//-----------------------------------------------------------------------------
void DataBlob::Clear(BOOL bValid)
{
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);

	m_nUsedLen = 0;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataBlob::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\n DataBlob:");
	AFX_DUMP1(dc, "\n\t m_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\t m_AllocSize= ", m_nAllocSize);
	AFX_DUMP1(dc, "\n\t m_UsedLen= ", m_nUsedLen);
}

void DataBlob::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG

//============================================================================
//          DataArray class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataArray, DataObj)

//-----------------------------------------------------------------------------
DataArray::DataArray()
	:
	m_BaseDataType(DataType::Null)
{
	SetModified();
	SetDirty();
}

//-----------------------------------------------------------------------------
void DataArray::SetAt(int nIndex, DataObj* pObj)
{
	if (IsValueLocked())
		return;
	Signal(ON_CHANGING);

	if (nIndex < m_arData.GetSize() && nIndex >= 0)
	{
		if (m_arData.GetAt(nIndex) != NULL)
			delete m_arData.GetAt(nIndex);
	}

	m_arData.SetAt(nIndex, pObj);

	SetModified();
	SetDirty();

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataArray::SetAtGrow(int nIndex, DataObj* pObj)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	if (nIndex < m_arData.GetSize() && nIndex >= 0)
	{
		if (m_arData.GetAt(nIndex) != NULL)
			delete m_arData.GetAt(nIndex);
	}

	//An. 15744
	//riempo le posizioni intermedie con dataobj che avranno il valore di default
	for (int i = m_arData.GetSize(); i < nIndex; i++)
		m_arData.Add(DataObj::DataObjCreate(GetBaseDataType()));

	m_arData.SetAtGrow(nIndex, pObj);

	SetModified();
	SetDirty();

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
//DataObj 	DataArray::operator[]	(int nIndex) const				
//{ 
//	if (nIndex < m_arData.GetSize() && nIndex >= 0)
//	{
//		if (m_arData.GetAt(nIndex) == NULL) 
//			SetAt(nIndex, DataObj::DataObjCreate(m_DataType));
//	}
//	else
//		SetAtGrow(nIndex, DataObj::DataObjCreate(m_DataType));
//
//	return *(m_arData.GetAt(nIndex));	
//}
//
////-----------------------------------------------------------------------------
//DataObj& 	DataArray::operator[]	(int nIndex)					
//{ 
//	if (nIndex < m_arData.GetSize() && nIndex >= 0)
//	{
//		if (m_arData.GetAt(nIndex) == NULL) 
//			SetAt(nIndex, DataObj::DataObjCreate(m_arData.m_DataType));
//	}
//	else
//		SetAtGrow(nIndex, DataObj::DataObjCreate(m_DataType));
//
//	return *(m_arData.ElementAt(nIndex));	
//}

//-----------------------------------------------------------------------------
CString DataArray::Str(int, int) const
{
	return m_arData.ToString(); //.Str ???
}

//-----------------------------------------------------------------------------
CString DataArray::ToString() const
{
	return m_arData.ToString();
}

//-----------------------------------------------------------------------------
void DataArray::Assign(const DataObj& aDataObj)
{
	if (IsValueLocked())
		return;

	Signal(ON_CHANGING);

	if (aDataObj.IsKindOf(RUNTIME_CLASS(DataArray)))
	{
		DataArray* pAr = (DataArray*)&aDataObj;

		m_BaseDataType = pAr->m_BaseDataType;
		m_arData = pAr->m_arData;
	}
	else if (aDataObj.IsKindOf(RUNTIME_CLASS(DataStr)))
	{
		DataStr* pDs = (DataStr*)&aDataObj;
		CString str = pDs->GetString();
		m_arData.Assign(str, m_BaseDataType);
	}
	else return;

	AssignStatus(aDataObj);
	SetModified();
	SetDirty();

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataArray::Assign(LPCTSTR psz)
{
	if (IsValueLocked())
		return;

	m_arData.Assign(psz, m_BaseDataType);

	SetModified();
	SetDirty();

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataArray::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(FALSE, "This feature is not implemented");
}

//-----------------------------------------------------------------------------
void DataArray::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(FALSE, "This feature is not implemented");
}

//-----------------------------------------------------------------------------
void DataArray::Clear(BOOL bValid)
{
	if (IsValueLocked())
		return;
	Signal(ON_CHANGING);

	DataObj::Clear(bValid);

	if (!m_arData.GetSize())
		return;

	m_arData.RemoveAll();
	SignalOnChanged();
}

//-----------------------------------------------------------------------------
int DataArray::IsEqual(const DataObj& o)	const
{
	DataArray* par = (DataArray*)(const_cast<DataObj*>(&o));
	return m_BaseDataType == par->m_BaseDataType && m_arData.IsEqual(par->m_arData);
}

int DataArray::IsLessThan(const DataObj& o)	const
{
	DataArray* par = (DataArray*)(const_cast<DataObj*>(&o));
	return m_BaseDataType == par->m_BaseDataType && m_arData.IsLessThan(par->m_arData) > 0;
}

//-----------------------------------------------------------------------------
CString DataArray::FormatDataForXML(BOOL b) const
{
	//TODO
	ASSERT_TRACE(FALSE, "This feature is not implemented");
	/*
	CString str;
	for (int i = 0; i < GetSize(); i++)
	{
	DataObj* pObj = GetAt(i);

	CString sTmpOne = pObj->FormatDataForXML(b);
	}
	*/
	return _T("");
}

//-----------------------------------------------------------------------------
void DataArray::SerializeJsonValue(CJsonSerializer& jsonSerializer)
{
	jsonSerializer.WriteString(szValue, FormatDataForXML());
//  TODO qui sotto implementazione futura per quando anche la functionDescription sara' capace di gestire json
/*	jsonSerializer.OpenArray(_T("values"));

	for (int i = 0; i < GetSize(); i++)
	{
		DataObj* pObj = GetAt(i);
		jsonSerializer.OpenObject(i);
		jsonSerializer.WriteString(_T("value"), pObj->FormatDataForXML());
		jsonSerializer.CloseObject();
	}

	jsonSerializer.CloseArray();*/
}

//-----------------------------------------------------------------------------
void DataArray::AssignFromXMLString(LPCTSTR lpszValue)
{
	Assign(lpszValue);
}

//-----------------------------------------------------------------------------
BOOL DataArray::Attach(CObArray* par)
{
	return m_arData.AddAlignArray(par);
}

//-----------------------------------------------------------------------------
BOOL DataArray::Attach(DataArray* ar)
{
	return m_arData.AddAlignArray(&(ar->GetData()));
}

//-----------------------------------------------------------------------------
void DataArray::Detach()
{
	m_arData.RemoveAllAlignArray();
}

//-----------------------------------------------------------------------------
BOOL DataArray::Append(const CStringArray& ar)
{
	if (GetBaseDataType() != DataType::String && GetBaseDataType() != DataType::Text)
		return FALSE;

	//Signal(ON_CHANGING);

	for (int i = 0; i < ar.GetCount(); i++)
		Add(new DataStr(ar[i]));

	//SignalOnChanged();
	return TRUE;
}

//-----------------------------------------------------------------------------
void DataArray::CalcSum(DataObj& aSum) const
{
	ASSERT(this->m_BaseDataType == aSum.GetDataType());
	m_arData.CalcSum(aSum);
}

DataObj* DataArray::GetMinElem() const
{
	return m_arData.GetMinElem();
}

DataObj* DataArray::GetMaxElem() const
{
	return m_arData.GetMaxElem();
}

void DataArray::CalcPercentages(DataObjArray& arPercentages) const
{
	if (m_BaseDataType == DataType::Money)
	{
		m_arData.CalcPercentages<DataMon>(arPercentages);
		return;
	}
	if (m_BaseDataType == DataType::Quantity)
	{
		m_arData.CalcPercentages<DataQty>(arPercentages);
		return;
	}
	if (m_BaseDataType == DataType::Percent)
	{
		m_arData.CalcPercentages<DataPerc>(arPercentages);
		return;
	}
	if (m_BaseDataType == DataType::Double)
	{
		m_arData.CalcPercentages<DataDbl>(arPercentages);
		return;
	}
	if (m_BaseDataType == DataType::ElapsedTime)
	{
		m_arData.CalcPercentages<DataLng>(arPercentages);
		return;
	}
	if (m_BaseDataType == DataType::Integer)
	{
		m_arData.CalcPercentages<DataInt>(arPercentages);
		return;
	}
	if (m_BaseDataType == DataType::Long)
	{
		m_arData.CalcPercentages<DataLng>(arPercentages);
		return;
	}

	ASSERT_TRACE1(FALSE, "Function DataArray::CalcPercentages not implemented on array basetype %s\n", m_BaseDataType.ToString());
}

//-----------------------------------------------------------------------------
BOOL DataArray::FixDataType(DataType newType)
{
	for (int i = 0; i < m_arData.GetCount(); i++)
	{
		DataObj* pE = GetAt(i);
		ASSERT_VALID(pE);
		DataType tv = pE->GetDataType();
		if (!DataType::IsCompatible(newType, tv))
		{
			DataObj* pVal = DataObj::DataObjCreate(newType);
			pVal->Assign(*pE);

			SetAt(i, pVal);	//la delete di pE è interna
		}
	}
	this->m_BaseDataType = newType;
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataArray::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataArray:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tContent= ", Str());
}

void DataArray::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG

//============================================================================
//          DataRecord class implementations
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DataRecord, DataObj)

//-----------------------------------------------------------------------------
DataRecord::DataRecord()
	:
	m_pRecord(NULL),
	m_bOwnRecord(FALSE)
{
	SetModified();
	SetDirty();
}

DataRecord::DataRecord(const DataRecord& ar)
	:
	m_pRecord(NULL),
	m_bOwnRecord(FALSE)
{
	Assign(ar);
}

DataRecord::DataRecord(ISqlRecord* pRec, BOOL bOwnRecord)
	:
	m_pRecord(pRec),
	m_bOwnRecord(bOwnRecord)
{}

DataRecord::~DataRecord()
{
	if (m_bOwnRecord && m_pRecord)
		m_pRecord->Dispose();
}

void DataRecord::SetIRecord(ISqlRecord* rec, BOOL bOwnRecord)
{
	if (m_pRecord && m_bOwnRecord)
		m_pRecord->Dispose();

	m_bOwnRecord = bOwnRecord;
	m_pRecord = rec;
}

//-----------------------------------------------------------------------------
CString DataRecord::Str(int, int) const
{
	return m_pRecord ? m_pRecord->ToString() : L"";
}

//-----------------------------------------------------------------------------
CString DataRecord::ToString() const
{
	return m_pRecord ? m_pRecord->ToString() : L"";
}

//-----------------------------------------------------------------------------
void DataRecord::Assign(const DataObj& aDataObj)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataRecord)))
	{
		ASSERT(FALSE);
		return;
	}

	DataRecord* pDRSrc = dynamic_cast<DataRecord*>(const_cast<DataObj*>(&aDataObj));

	if (m_pRecord)
	{
		if (m_pRecord->GetTableName().CompareNoCase(pDRSrc->GetIRecord()->GetTableName()))
		{
			ASSERT(FALSE);
			return;
		}

		m_pRecord->Assign(pDRSrc->GetIRecord());
	}
	else
	{
		m_bOwnRecord = TRUE;
		m_pRecord = pDRSrc->GetIRecord()->IClone();
	}

	AssignStatus(aDataObj);
	SetModified();
	SetDirty();

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
void DataRecord::Assign(LPCTSTR)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(FALSE, "This feature is not implemented");
}

//-----------------------------------------------------------------------------
void DataRecord::Assign(const VARIANT& v)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(FALSE, "This feature is not implemented");
}

//-----------------------------------------------------------------------------
void DataRecord::Assign(const RDEData& aRDEData)
{
	if (IsValueLocked())
		return;

	ASSERT_TRACE(FALSE, "This feature is not implemented");
}

//-----------------------------------------------------------------------------
void DataRecord::Clear(BOOL bValid)
{
	Signal(ON_CHANGING);
	if (IsValueLocked())
		return;

	DataObj::Clear(bValid);

	if (!m_pRecord)
		return;

	m_pRecord->Init();

	SignalOnChanged();
}

//-----------------------------------------------------------------------------
int DataRecord::IsEqual(const DataObj& o)	const
{
	DataRecord* par = (DataRecord*)(const_cast<DataObj*>(&o));
	return m_pRecord->IIsEqual(*(par->m_pRecord));
}

int DataRecord::IsLessThan(const DataObj& o)	const
{
	ASSERT_TRACE(FALSE, "This feature is not implemented");

	return FALSE;
}

//-----------------------------------------------------------------------------
CString DataRecord::FormatDataForXML(BOOL b) const
{
	//TODO
	ASSERT_TRACE(FALSE, "This feature is not implemented");
	/*
	CString str;
	for (int i = 0; i < GetSize(); i++)
	{
	DataObj* pObj = GetAt(i);

	CString sTmpOne = pObj->FormatDataForXML(b);
	}
	*/
	return _T("");
}

//-----------------------------------------------------------------------------
void DataRecord::AssignFromXMLString(LPCTSTR lpszValue)
{
	Assign(lpszValue);
}

/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void DataRecord::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "\nDataRecord:");
	AFX_DUMP1(dc, "\n\tm_wDataStatus = ", m_wDataStatus);
	AFX_DUMP1(dc, "\n\tContent= ", Str());
}

void DataRecord::AssertValid() const
{
	DataObj::AssertValid();
}
#endif //_DEBUG
// CGuid implementation
//////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE(CGuid, CObject)

// Construction

CGuid::CGuid()
{
	m_guid = GUID_NULL;
}

CGuid::CGuid(const CGuid& guid)
{
	m_guid = guid.GetValue();
}

CGuid::CGuid(const GUID& guid)
{
	m_guid = guid;
}

CGuid::CGuid(LPCTSTR lpszGuid)
{
	GuidFromString(lpszGuid, &m_guid);
}

// Attributes

GUID CGuid::GetValue() const
{
	return m_guid;
}

void CGuid::SetValue(const GUID& newValue)
{
	m_guid = newValue;
}

// Operations

void CGuid::GenerateGuid()
{
	HRESULT hResult = ::CoCreateGuid(&m_guid);
	if (FAILED(hResult))
		AfxThrowOleException(hResult);
}

CGuid& CGuid::operator=(const CGuid& guid)
{
	m_guid = guid.GetValue();
	return *this;
}

CGuid& CGuid::operator=(const GUID& guid)
{
	m_guid = guid;
	return *this;
}

CGuid& CGuid::operator=(LPCTSTR lpszGuid)
{
	GuidFromString(lpszGuid, &m_guid);
	return *this;
}

CGuid::operator LPGUID()
{
	return &m_guid;
}

CGuid::operator CString() const
{
	USES_CONVERSION;
	CString strGuid;
	LPOLESTR lpolestrGuid;
	HRESULT hResult = ::StringFromCLSID(m_guid, &lpolestrGuid);
	if (FAILED(hResult))
		AfxThrowOleException(hResult);

	strGuid = CString(W2T(lpolestrGuid));
	::CoTaskMemFree(lpolestrGuid);
	return strGuid;
}

BOOL CGuid::operator==(const CGuid& guid) const
{
	return m_guid == guid.m_guid;
}

BOOL CGuid::operator !=(const CGuid& guid) const
{
	return m_guid != guid.m_guid;
}

BOOL CGuid::operator <(const CGuid& guid) const
{
	if (m_guid.Data1 < guid.m_guid.Data1)
		return TRUE;
	if (m_guid.Data1 > guid.m_guid.Data1)
		return FALSE;

	if (m_guid.Data2 < guid.m_guid.Data2)
		return TRUE;
	if (m_guid.Data2 > guid.m_guid.Data2)
		return FALSE;

	if (m_guid.Data3 < guid.m_guid.Data3)
		return TRUE;
	if (m_guid.Data3 > guid.m_guid.Data3)
		return FALSE;

	for (int i = 0; i < sizeof(m_guid.Data4) / sizeof(m_guid.Data4[0]); i++)
	{
		if (m_guid.Data4[i] < guid.m_guid.Data4[i])
			return TRUE;

		if (m_guid.Data4[i] > guid.m_guid.Data4[i])
			return FALSE;
	}
	return FALSE;
}

BOOL CGuid::operator >(const CGuid& guid) const
{
	if (m_guid.Data1 > guid.m_guid.Data1)
		return TRUE;
	if (m_guid.Data1 < guid.m_guid.Data1)
		return FALSE;

	if (m_guid.Data2 > guid.m_guid.Data2)
		return TRUE;
	if (m_guid.Data2 < guid.m_guid.Data2)
		return FALSE;

	if (m_guid.Data3 > guid.m_guid.Data3)
		return TRUE;
	if (m_guid.Data3 < guid.m_guid.Data3)
		return FALSE;

	for (int i = 0; i < sizeof(m_guid.Data4) / sizeof(m_guid.Data4[0]); i++)
	{
		if (m_guid.Data4[i] > guid.m_guid.Data4[i])
			return TRUE;
		if (m_guid.Data4[i] < guid.m_guid.Data4[i])
			return FALSE;
	}
	return FALSE;
}

// Implementation


void CGuid::GuidFromString(LPCTSTR lpsz, GUID* pGuid)
{
	if (_tcslen(lpsz))
	{
		USES_CONVERSION;

		HRESULT hResult = CLSIDFromString(T2W((LPTSTR)lpsz), pGuid);
		if (FAILED(hResult))
			AfxThrowOleException(hResult);
	}
	else
		*pGuid = GUID_NULL;
}

// CDiagnostic support

#ifdef _DEBUG
void CGuid::AssertValid() const
{
	CObject::AssertValid();
}

void CGuid::Dump(CDumpContext& dc) const
{
	CString strTabber(TAB_CHAR, dc.GetDepth());
	dc << strTabber;
	CObject::Dump(dc);
	dc << strTabber << CString() << _T("\n");
}
#endif


//==============================================================================
//          Class RDEData implementation
//==============================================================================

//-----------------------------------------------------------------------------
RDEData::RDEData()
	:
	m_Status(0),
	m_Len(0),
	m_Size(0),
	m_pData(NULL),
	m_bValid(FALSE),
	m_bIsDataOwner(FALSE),
	m_bIsTailMultiLineString(FALSE)
{
}

//-----------------------------------------------------------------------------
RDEData::RDEData(const RDEData& RdeData)
	:
	m_Status(0),
	m_Len(0),
	m_Size(0),
	m_pData(NULL),
	m_bValid(FALSE),
	m_bIsDataOwner(FALSE),
	m_bIsTailMultiLineString(FALSE)
{
	*this = RdeData;
}

//-----------------------------------------------------------------------------
RDEData::~RDEData()
{
	if (m_pData && m_bIsDataOwner) delete m_pData;
}

//-----------------------------------------------------------------------------
RDEData::RDEData(RDEcmd Status, DataSize Size, void* pData, BOOL bKeepPropr)
	:
	m_Status(Status),
	m_Len(Size),
	m_Size(Size),
	m_pData(pData),
	m_bValid(TRUE),
	m_bIsDataOwner(bKeepPropr),
	m_bIsTailMultiLineString(FALSE)
{
}

//-----------------------------------------------------------------------------
RDEData::RDEData(RDEcmd Status, const CString& str)
	:
	m_Status(Status),
	m_bValid(TRUE),
	m_bIsDataOwner(TRUE),
	m_bIsTailMultiLineString(FALSE)
{
	m_Size = m_Len = (str.GetLength() + 1) * sizeof(TCHAR);

	m_pData = (void*)_tcsdup(str.GetString());	// RDEData keep property of data area
}

//-----------------------------------------------------------------------------
const RDEData& RDEData::operator = (const RDEData& RdeData)
{
	if (!RdeData.m_bIsDataOwner)
	{
		if (m_pData && m_bIsDataOwner)
			delete m_pData;

		m_pData = RdeData.m_pData;
		m_Size = RdeData.m_Size;
		m_Len = RdeData.m_Len;
		m_Status = RdeData.m_Status;
		m_bValid = RdeData.m_bValid;
		m_bIsDataOwner = RdeData.m_bIsDataOwner;
		m_bIsTailMultiLineString = RdeData.m_bIsTailMultiLineString;

		return *this;
	}

	// source is owner of allocated data then we alloc o realloc (as needed)
	// data memory for destination data
	if (
		NewData(RdeData.m_Len, RdeData.m_Status, RdeData.m_bValid, RdeData.m_bIsDataOwner) &&
		m_Len > 0	// modified by NewData()
		)
		memcpy(m_pData, RdeData.m_pData, (size_t)m_Len);

	return *this;
}

//-----------------------------------------------------------------------------
const RDEData& RDEData::CloneData(const RDEData& RdeData)
{
	// "this" will be owner of allocated data then we alloc o realloc (as needed)
	// data memory for destination data
	if (
		NewData(RdeData.m_Len, RdeData.m_Status, RdeData.m_bValid, TRUE) &&
		m_Len > 0	// modified by NewData()
		)
		memcpy(m_pData, RdeData.m_pData, (size_t)m_Len);

	return *this;
}

//-----------------------------------------------------------------------------
void RDEData::SetData(RDEcmd Status, const DataObj* pDataObj, BOOL bIsTailMultiLineString /* = FALSE */)
{
	m_Status = Status;

	if (pDataObj)
	{
		m_pData = pDataObj->GetRawData(&m_Len);  // GetRawData also update m_Len

		if (m_Len > 0x7FFF)
		{
			//ASSERT_TRACE2(FALSE, "Overflow RDE string value with Len = %d must be less than or equal to max field size is %d", m_Len, 0x7FFF / 2);
			//m_Len = 0x7FFF;

			m_dsOverflowError = cwsprintf(_TB("*** The string value is too long: {0-%d}, the max allowed length is {1-%d} ***"), m_Len / 2 - 3, (0x7FFF / 2)); // /2 è per l'unicode
			m_pData = m_dsOverflowError.GetRawData(&m_Len);
		}

		m_Size = m_Len;

		if (bIsTailMultiLineString)
			m_Len |= 0x8000;
	}
	else
	{
		m_pData = NULL;
		m_Size = m_Len = 0;
	}

	m_bValid = TRUE;
	m_bIsDataOwner = FALSE;                // the m_pData is property of pDataObj
}

//-----------------------------------------------------------------------------
void* RDEData::NewData(DataSize Len, RDEcmd Status, BOOL bValid, BOOL bIsOwner)
{
	if (Len > 0x7FFF && Status != IRDEManager::Command::ARRAY_DATA)
	{
		ASSERT_TRACE2(FALSE, "RDE new data with size = %d must be less than or equal to max field size %d ***", Len, 0x7FFF);

		//Len = 0x7FFF;
	}

	if (m_pData && m_bIsDataOwner && (Len > m_Size || Len == 0))
	{
		delete m_pData;
		m_pData = NULL;
		m_Size = m_Len = 0;
	}

	if (!m_pData && bIsOwner && Len)
	{
		m_pData = new BYTE[Len];
		m_Size = Len;
	}

	m_Len = Len;

	ASSERT_TRACE2(m_Len <= m_Size, "parameter Len = %d must be less than or equal to m_Size = %d", Len, m_Size);

	m_Status = Status;
	m_bValid = bValid;
	m_bIsDataOwner = bIsOwner;

	return m_pData;
}

//-----------------------------------------------------------------------------
RDEcmd	RDEData::GetStatus() const { return m_Status; }

//-----------------------------------------------------------------------------
DataSize RDEData::GetLen() const { return m_Len; }

//-----------------------------------------------------------------------------
void* RDEData::GetData() const { return m_pData; }

//-----------------------------------------------------------------------------
BOOL RDEData::IsEnabled() const { return m_bValid; }

//-----------------------------------------------------------------------------
void RDEData::DisableData() { m_bValid = FALSE; }

//-----------------------------------------------------------------------------
BOOL RDEData::IsColTotal() const
{
	return IsColTotalKind(m_Status);
}

//-----------------------------------------------------------------------------
BOOL RDEData::IsSubTotal() const
{
	return IsSubTotalKind(m_Status);
}

//-----------------------------------------------------------------------------
WORD RDEData::GetColTotalId() const
{
	return (WORD)(m_Status & MAX_ID_FOR_TOTAL);
}

//-----------------------------------------------------------------------------
WORD RDEData::GetSubTotalId() const
{
	return (WORD)(m_Status & MAX_ID_FOR_TOTAL);
}

//-----------------------------------------------------------------------------
RDEcmd RDEData::GetSubTotalKind(WORD id)
{
	return (WORD)(id | IRDEManager::SUB_TOTAL);
}

//-----------------------------------------------------------------------------
RDEcmd RDEData::GetColTotalKind(WORD id)
{
	return (WORD)(id | IRDEManager::COL_TOTAL);
}

//-----------------------------------------------------------------------------
BOOL RDEData::IsColTotalKind(RDEcmd value)
{
	return
		(value & MAX_ID_FOR_TOTAL) &&
		(value & ~MAX_ID_FOR_TOTAL) == IRDEManager::COL_TOTAL;
}

//-----------------------------------------------------------------------------
BOOL RDEData::IsSubTotalKind(RDEcmd value)
{
	return
		(value & MAX_ID_FOR_TOTAL) &&
		(value & ~MAX_ID_FOR_TOTAL) == IRDEManager::SUB_TOTAL;
}

//=============================================================================

SpecialReportField::SRFname SpecialReportField::NAME;
SpecialReportField::SRFid	SpecialReportField::ID;

//=============================================================================
