
#include "stdafx.h"

#include <TbGeneric\Array.h>
#include "parsobj.h"
#include "baseapp.h"
#include "AutoExpressionMng.h"
#include "AutoExprDlg.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

//-------------------------------------------------------------------------------------
//CAutoExpressionData
//-------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CAutoExpressionData, CObject)

//-------------------------------------------------------------------------------------
CAutoExpressionData::CAutoExpressionData()
	:
	m_pDataObj	(NULL)
{

}

//-------------------------------------------------------------------------------------
CAutoExpressionData::CAutoExpressionData(const CString& strExpression, const CString& strVarName, DataObj* pDataObj)
{
	m_strExpression = strExpression;
	m_strVarName	= strVarName;
	m_pDataObj		= pDataObj;
}

//-------------------------------------------------------------------------------------
CAutoExpressionData::CAutoExpressionData(const CAutoExpressionData& aAutoExpressionData)
{
	Assign(aAutoExpressionData);
}

//-------------------------------------------------------------------------------------
void CAutoExpressionData::Assign(const CAutoExpressionData& aAutoExpressionData)
{
	m_strExpression = aAutoExpressionData.m_strExpression;
	m_strVarName	= aAutoExpressionData.m_strVarName;
	m_pDataObj		= aAutoExpressionData.m_pDataObj;
}

//-------------------------------------------------------------------------------------
void CAutoExpressionData::SetExpression(const CString& strExpression)
{
	m_strExpression = strExpression;
}

//-------------------------------------------------------------------------------------
CString CAutoExpressionData::GetExpression()
{
	return m_strExpression;
}

//-------------------------------------------------------------------------------------
void CAutoExpressionData::SetVarName(const CString& strVarName)
{
	m_strVarName = strVarName;
}

//-------------------------------------------------------------------------------------
CString	CAutoExpressionData::GetVarName()
{
	return m_strVarName;
}

//-------------------------------------------------------------------------------------
void CAutoExpressionData::SetDataObj(DataObj* pDataObj)
{
	m_pDataObj = pDataObj;
}

//-------------------------------------------------------------------------------------
DataObj* CAutoExpressionData::GetDataObj()
{
	return m_pDataObj;
}

//-------------------------------------------------------------------------------------
CAutoExpressionData& CAutoExpressionData::operator = (const CAutoExpressionData& aAutoExpressionData)
{
	if (this == &aAutoExpressionData)
		return *this;
	
	m_strVarName	= aAutoExpressionData.m_strVarName;
	m_strExpression	= aAutoExpressionData.m_strExpression;
	m_pDataObj		= aAutoExpressionData.m_pDataObj;

	return *this;
}

//-------------------------------------------------------------------------------------
BOOL CAutoExpressionData::IsEqual(const CAutoExpressionData& aAutoExpressionData) const
{
	if (this == &aAutoExpressionData)
		return TRUE;

	return 	(
		m_strVarName	== aAutoExpressionData.m_strVarName		&&
		m_strExpression	== aAutoExpressionData.m_strExpression	&&
		m_pDataObj		== aAutoExpressionData.m_pDataObj
		);
}

//-------------------------------------------------------------------------------------
//CAutoExpressionDataArray
//-------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CAutoExpressionDataArray, Array);

//-------------------------------------------------------------------------------------
CAutoExpressionDataArray::CAutoExpressionDataArray()
{

}

//-------------------------------------------------------------------------------------
CString	CAutoExpressionDataArray::GetVarNameByDataObj(DataObj* pDataObj)
{
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if(GetAt(i)->m_pDataObj == pDataObj)
			return GetAt(i)->m_strVarName;
	}

	return _T("");
}

//-------------------------------------------------------------------------------------
CString	CAutoExpressionDataArray::GetExpressionByDataObj(DataObj* pDataObj)
{
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if(GetAt(i)->m_pDataObj == pDataObj)
			return GetAt(i)->m_strExpression;
	}

	return _T("");
}

//-------------------------------------------------------------------------------------
DataObj* CAutoExpressionDataArray::GetDataObjByVarName(const CString& strVarName)
{
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if(GetAt(i)->m_strVarName == strVarName)
			return GetAt(i)->m_pDataObj;
	}

	return NULL;
}

//-------------------------------------------------------------------------------------
CString	CAutoExpressionDataArray::GetExpressionByVarName(const CString& strVarName)
{
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if(GetAt(i)->m_strVarName == strVarName)
			return GetAt(i)->GetExpression();
	}

	return _T("");
}


//-------------------------------------------------------------------------------------
int	CAutoExpressionDataArray::Add(const CString& strExpression, const CString& strVarName, DataObj* pDataObj)
{
	return Array::Add(new CAutoExpressionData(strExpression, strVarName, pDataObj));
}

//----------------------------------------------------------------------------------------------
BOOL CAutoExpressionDataArray::IsEqual(const CAutoExpressionDataArray& aAutoArray) const
{
	if (this == &aAutoArray)
		return TRUE;
	
	if(aAutoArray.GetSize() != GetSize())
		return FALSE;
	
	for(int i = 0 ; i < GetSize() ; i++)
	{
		if (!GetAt(i) && aAutoArray.GetAt(i))
			return FALSE;
		
		if(!GetAt(i)->IsEqual(*aAutoArray.GetAt(i)))
			return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
CAutoExpressionDataArray& CAutoExpressionDataArray::operator= (const CAutoExpressionDataArray& aAutoArray)
{
	if (this != &aAutoArray)
	{
		RemoveAll();
		for (int i=0; i < aAutoArray.GetSize(); i++)
			Add(new CAutoExpressionData(*aAutoArray.GetAt(i)));
	}
	return *this;
}

//----------------------------------------------------------------------------
//CAutoExpressionMng
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CAutoExpressionMng, CObject);

//----------------------------------------------------------------------------
CAutoExpressionMng::CAutoExpressionMng()
{

}

//----------------------------------------------------------------------------
void CAutoExpressionMng::Add(CAutoExpressionData* pAutoExpressionData)
{
	if(!pAutoExpressionData)
	{
		ASSERT(FALSE);
		return;
	}

	m_AutoExpressionDataArray.Add(pAutoExpressionData);
}

//----------------------------------------------------------------------------
void CAutoExpressionMng::Add(const CString& strExpression, const CString& strVarName, DataObj* pDataObj)
{
	m_AutoExpressionDataArray.Add(new CAutoExpressionData(strExpression, strVarName, pDataObj));
}

//----------------------------------------------------------------------------
BOOL CAutoExpressionMng::EvaluateExpression(const CString& strExpression, DataObj* pDataObj)
{
	if(strExpression.IsEmpty() || !pDataObj)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return GetAutoExpressionValue(RemoveAutoExpressionPrefix(strExpression), pDataObj);
}

//----------------------------------------------------------------------------
CString	CAutoExpressionMng::GetVarNameByDataObj(DataObj* pDataObj)
{
	if(!pDataObj)
	{
		ASSERT(FALSE);
		return _T("");
	}

	return m_AutoExpressionDataArray.GetVarNameByDataObj(pDataObj);
}

//----------------------------------------------------------------------------
CString	CAutoExpressionMng::GetExpressionByDataObj(DataObj* pDataObj)
{
	if(!pDataObj)
	{
		ASSERT(FALSE);
		return _T("");
	}

	return m_AutoExpressionDataArray.GetExpressionByDataObj(pDataObj);
}

//----------------------------------------------------------------------------
CString	CAutoExpressionMng::GetExpressionByVarName(const CString& strVarName)
{
	if(strVarName.IsEmpty())
	{
		ASSERT(FALSE);
		return _T("");
	}

	return m_AutoExpressionDataArray.GetExpressionByVarName(strVarName);
}

//----------------------------------------------------------------------------
DataObj* CAutoExpressionMng::GetDataObjByVarName(const CString& strVarName)
{
	if(strVarName.IsEmpty())
	{
		ASSERT(FALSE);
		return NULL;
	}

	return m_AutoExpressionDataArray.GetDataObjByVarName(strVarName);
}

//----------------------------------------------------------------------------
void CAutoExpressionMng::RemoveAll()
{
	m_AutoExpressionDataArray.RemoveAll();
}