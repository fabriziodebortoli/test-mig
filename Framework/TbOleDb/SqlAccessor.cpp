#include "stdafx.h"
#include <oledberr.h>

#include <TbGeneric\DataObj.h>

#include "sqlaccessor.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#define STATUS_LEN			4
#define LENGTH_LEN			4
#define BUFFER_SIZE			4096

const int nEmptySqlRecIdx = -1;

//===========================================================================
//	SqlBindingElemArray
//===========================================================================
//
IMPLEMENT_DYNAMIC(SqlBindingElem, SqlBindObject)


//---------------------------------------------------------------------------
void SqlBindingElem::Init()
{
	m_pDataObj->Clear();
	m_pOldDataObj->Clear();
}


//-----------------------------------------------------------------------------
void SqlBindingElem::SetParamValue(const DataObj& aDataObj)
{
	AssignOldDataObj(*m_pDataObj);
	m_pDataObj->Assign(aDataObj);
}

//-----------------------------------------------------------------------------
DataType SqlBindingElem::GetDataType() const
{
	if (!m_pDataObj)
	{
		ASSERT(FALSE);
		return DataType::Null;
	}
	return m_pDataObj->GetDataType();
}

//-----------------------------------------------------------------------------
void SqlBindingElem::AssignOldDataObj(const DataObj& aDataObj)
{
	m_pOldDataObj->Assign(aDataObj);
}

//-----------------------------------------------------------------------------
void SqlBindingElem::ClearOldDataObj()
{
	m_pOldDataObj->Clear();
}


//-----------------------------------------------------------------------------
CString SqlBindingElem::GetBindName(BOOL bQualified /*= FALSE*/) const
{
	CString sBind = m_strBindName;
	if (!bQualified)
	{
		int idx = sBind.Find('.');
		if (idx > 0)
			sBind = sBind.Mid(idx + 1);
	}
	return sBind;
}


//===========================================================================
//	SqlBindingElemArray
//===========================================================================
//
IMPLEMENT_DYNAMIC(SqlBindingElemArray, SqlBindObjectArray)

//---------------------------------------------------------------------------
DataObj* SqlBindingElemArray::GetDataObj(const CString& columnName) const
{
	for (int i = 0; i < GetSize(); i++)
	{
		SqlBindingElem* pColumn = (SqlBindingElem*)GetAt(i);
		if (pColumn->m_strBindName.CompareNoCase(columnName) == 0)
			return pColumn->m_pDataObj;
	}
	return NULL;
}

//---------------------------------------------------------------------------
DataObj* SqlBindingElemArray::GetDataObjAt(int pos) const
{
	SqlBindingElem* pColumn = (SqlBindingElem*)GetAt(pos);
	return (pColumn) ? pColumn->m_pDataObj : NULL;
}


//---------------------------------------------------------------------------
DataObj* SqlBindingElemArray::GetOldDataObjAt(int pos) const
{
	SqlBindingElem* pColumn = (SqlBindingElem*)GetAt(pos);
	return (pColumn) ? pColumn->m_pOldDataObj : NULL;
}


//-----------------------------------------------------------------------------
const CString& SqlBindingElemArray::GetParamName(int nIdx) const
{
	return GetAt(nIdx)->m_strBindName;
}

//---------------------------------------------------------------------------
void SqlBindingElemArray::InitColumns()
{
	SqlBindingElem* pElem = NULL;
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pElem = GetAt(nIdx);
		if (pElem)
			pElem->Init();
	}
}

//---------------------------------------------------------------------------
int SqlBindingElemArray::AddParam(const CString&  strName, const DataType& nDataType, int nLen, SqlParamType eParamType /*= Input*/, const SWORD nSqlDataType /*= DBTYPE_EMPTY*/, int nInsertPos /*= -1*/)
{
	DataObj* pDataObj = DataObj::DataObjCreate(nDataType);

	// preallochiamo la dimensione del campo in base alla lunghezza definita //@@TODOBAUZI serve????
	if (nLen > 0)	
		pDataObj->SetAllocSize(nLen);

	//posso specificare anche un tipo di db differente dal default
	if (nSqlDataType != DBTYPE_EMPTY)
		pDataObj->SetSqlDataType(nSqlDataType);

	SqlBindingElem* pBindElem = new SqlBindingElem(strName, pDataObj, eParamType);
	pBindElem->m_bOwnData = TRUE; //lo devo distruggere nel distruttore

	int nIdx = -1;
	if (nInsertPos > -1 && GetSize() > 0 && nInsertPos >= 0 && nInsertPos < GetSize() - 1)
	{
		__super::InsertAt(nInsertPos, pBindElem);
		nIdx = nInsertPos;
	}
	else
		nIdx = __super::Add(pBindElem);

	return nIdx;
}


//---------------------------------------------------------------------------
int SqlBindingElemArray::AddParam(const CString&  strName, DataObj* pDataObj, SqlParamType eParamType /*= Input*/, int nInsertPos /*= -1*/)
{

	SqlBindingElem* pBindElem = new SqlBindingElem(strName, pDataObj, eParamType);

	int nIdx = -1;
	if (nInsertPos > -1 && GetSize() > 0 && nInsertPos >= 0 && nInsertPos < GetSize() - 1)
	{
		__super::InsertAt(nInsertPos, pBindElem);
		nIdx = nInsertPos;
	}
	else
		nIdx = __super::Add(pBindElem);

	return nIdx;
}

//-----------------------------------------------------------------------------
int	SqlBindingElemArray::AddColumn
(
	const CString&	strName,
	DataObj*	pDataObj,
	const int&		nSqlRecIdx, /*=-1*/
	bool		bAutoIncrement /*=false*/,
	int			nInsertPos		/*= -1*/ //se valorizzato il parametro viene inserito nell'nInsertPos posizione dell'array
)
{
	SqlBindingElem* pBindElem = new SqlBindingElem(strName, pDataObj, NoParam, nSqlRecIdx);
	pBindElem->m_bAutoIncrement = bAutoIncrement;
	pBindElem->m_nSqlRecIdx = nSqlRecIdx;

	int nIdx = -1;
	if (nInsertPos > -1 && GetSize() > 0 && nInsertPos >= 0 && nInsertPos < GetSize() - 1)
	{
		__super::InsertAt(nInsertPos, pBindElem);
		nIdx = nInsertPos;
	}
	else
		nIdx = __super::Add(pBindElem);

	return nIdx;
}


// ---------------------------------------------------------------------------- -
int SqlBindingElemArray::GetParamPosition(const CString& strParamName) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (GetAt(i) && GetAt(i)->m_strBindName.CompareNoCase(strParamName) == 0)
			return i;

	return -1;
}

//-----------------------------------------------------------------------------
void SqlBindingElemArray::SetParamValue(const CString& strParamName, const DataObj& aDataObj)
{
	int nPos = GetParamPosition(strParamName);
	if (nPos != -1)
	{
		GetAt(nPos)->SetParamValue(aDataObj);
		return;
	}

	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void SqlBindingElemArray::GetParamValue(const CString& strParamName, DataObj* pDataObj) const
{
	int nPos = GetParamPosition(strParamName);
	if (nPos != -1)
	{
		GetAt(nPos)->GetParamValue(pDataObj);
		return;
	}

	ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
void SqlBindingElemArray::GetParamValue(int nIdx, DataObj* pDataObj) const
{
	ASSERT(nIdx >= 0 && nIdx <= GetUpperBound());
	GetAt(nIdx)->GetParamValue(pDataObj);
}

//-----------------------------------------------------------------------------
DataType SqlBindingElemArray::GetParamDataType(int i) const
{
	ASSERT(i >= 0 && i <= GetUpperBound());
	return GetAt(i)->GetDataType();
}


//-----------------------------------------------------------------------------
SqlBindingElem* SqlBindingElemArray::GetParamByName(const CString& strName) const
{
	BOOL bQualified = strName.Find('.')  > 0;

	for (int i = 0; i <= GetUpperBound(); i++)
	{
		CString sBind = GetAt(i)->GetBindName(bQualified);
		if (sBind.CompareNoCase(strName) == 0)
			return GetAt(i);
	}

	return NULL;
}

//-----------------------------------------------------------------------------
bool SqlBindingElemArray::SameValues() const
{
	for (int i = 0; i <= GetUpperBound(); i++)
		if (!GetAt(i)->SameValue())
			return false;

	return true;
}



IMPLEMENT_DYNAMIC(SqlColumnArray, SqlBindingElemArray)

IMPLEMENT_DYNAMIC(SqlParamArray, SqlBindingElemArray)