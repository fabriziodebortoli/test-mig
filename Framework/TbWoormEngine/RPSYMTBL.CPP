
#include "stdafx.h"

#include <TbNameSolver\TBNamespaces.h>

#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\DataObj.h>

#include <TbGenlib\baseapp.h>

#include <TbParser\Parser.h>

#include "ActionsRepEngin.h"
#include "disptbl.h"
#include "prgdata.h"
#include "MultiLayout.h"
#include "ExportSymbols.h"

#include "rpsymtbl.h"
#include "repfield.h"
#include "reptable.h"

#include "report.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif


//============================================================================
//		class CStaticFieldArrayCache
//============================================================================

int CStaticFieldArrayCache::CheckAndAddField (WoormField* pRF)
{
	if (pRF == NULL)
		return -1;

	if (pRF->IsReInit() || !pRF->IsInput())
		return -1;

	if (!pRF->IsStatic() && !pRF->IsAsk() && pRF->IsHidden()) 
		return -1;

	//passano gli "Input" senza prima "ReInit" 
	//e i campi "Hidden Static Input"
	//e i campi Hidden Input che compaiono nelle Ask Dialog

	return Add(pRF->GetName(), pRF->GetId(), pRF->GetRepData());
}
//-----------------------------------------------------------------------------
int CStaticFieldArrayCache::Add (const CString& strName, WORD wID, DataObj* pValue)
{
	if (pValue == NULL) 
		return -1;

	for (int i=0; i < GetSize(); i++)
		if (GetID(i) == wID)
		{
			GetData(i)->Assign(*pValue) ;
			return TRUE;
		}

	return Array::Add( new CFieldCache(strName, wID, pValue->DataObjClone()));
}

//------------------------------------------------------------------------------
DataObj* CStaticFieldArrayCache::GetFromName (const CString& strName)
{
	for (int i=0; i < GetSize(); i++)
		if (strName.CompareNoCase(GetName(i)) == 0)
			return GetData(i);
	return NULL;
}

//------------------------------------------------------------------------------
DataObj* CStaticFieldArrayCache::GetFromID (WORD wID)
{
	for (int i=0; i < GetSize(); i++)
		if (GetID(i) == wID)
			return GetData(i);
	return NULL;
}

//------------------------------------------------------------------------------
BOOL CheckField(WoormField* pRF)
{
	if (pRF == NULL)
		return FALSE;

	if (pRF->IsReInit() || !pRF->IsInput())
		return FALSE;

	if (!pRF->IsStatic() && !pRF->IsAsk() && pRF->IsHidden()) 
		return FALSE;

	//passano gli "Input" senza prima "ReInit" 
	//e i campi "Hidden Static Input"
	//e i campi Hidden Input che compaiono nelle Ask Dialog
	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CStaticFieldArrayCache::RetrieveStaticValue(WoormField* pRF)
{
	if (!CheckField(pRF))
		return FALSE;
	
	for (int i=0; i < GetSize(); i++)
		if (GetID(i) == pRF->GetId() && pRF->GetRepData())
		{
			if (pRF->IsAsk() && pRF->GetDataType() == DataType::String && pRF->IsUpperLimit())
			{
				DataStr ds(*(DataStr*)GetData(i));
				ds.TrimUpperLimit();
				pRF->Init(&ds);
			}
			else
				pRF->Init(GetData(i));
			return TRUE;
		}
	return FALSE;
}

//------------------------------------------------------------------------------
BOOL CStaticFieldArrayCache::RetrieveStaticValue(WORD wID, DataObj& dValue)
{
	for (int i=0; i < GetSize(); i++)
		if (GetID(i) == wID)
		{
			dValue.Assign(*GetData(i)) ;
			return TRUE;
		}
	return FALSE;
}

//------------------------------------------------------------------------------
void CStaticFieldArrayCache::RemoveAll()
{
/*
	for(int i=0; i < GetSize(); i++)
	{
		TRACE("%s=%s\n", m_arstrFieldName.GetAt(i), GetAt(i)->Str() );
	}
*/
	__super::RemoveAll ();
}

//------------------------------------------------------------------------------
BOOL CStaticFieldArrayCache::RetrieveArguments (WoormTable* pWT, DataStr& dsXml)
{
	ASSERT_VALID(pWT);
	CFunctionDescription fd;
	
	for (int i = 0; i < pWT->GetSize(); i++)
	{
		WoormField* pF = (WoormField*) (*pWT)[i];
		ASSERT_VALID(pF);
		ASSERT_KINDOF(WoormField, pF);

		if (!CheckField(pF))
			continue;

		fd.InternalAddParam(pF->GetName(), pF->GetData(), FALSE);
	}

	if (fd.GetParameters().GetCount() == 0)
		return FALSE;

	return fd.UnparseArguments(dsXml, XML_ARGUMENTS_TAG);
}
