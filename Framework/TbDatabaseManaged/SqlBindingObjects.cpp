#include "stdafx.h" 

#include "SqlBindingObjects.h"


//===========================================================================
//	SqlBindObject
//===========================================================================
//
IMPLEMENT_DYNAMIC(SqlBindObject, CObject)

//---------------------------------------------------------------------------
SqlBindObject::SqlBindObject(const CString& strBindName, DataObj* pDataObj)
	:
	m_strBindName(strBindName),
	m_pDataObj(pDataObj),
	m_pOldDataObj(NULL),
	m_bOwnData(false),
	m_eParamType(NoParam),
	m_nSqlRecIdx(-1)
{
	if (m_pDataObj)
	{
		m_pOldDataObj = DataObj::DataObjCreate(m_pDataObj->GetDataType());
		m_pOldDataObj->SetAllocSize(m_pDataObj->GetColumnLen());
	}
}

//---------------------------------------------------------------------------
SqlBindObject::~SqlBindObject()
{
	if (m_pOldDataObj)
		delete m_pOldDataObj;

	if (m_bOwnData && m_pDataObj)
		delete m_pDataObj;
}

//===========================================================================
//	SqlBindObjectArray
//===========================================================================
//
IMPLEMENT_DYNAMIC(SqlBindObjectArray, Array)
