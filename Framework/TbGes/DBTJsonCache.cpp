#include "stdafx.h"

#include <TbOleDb\SqlRec.h>

#include "DBTJsonCache.h"
#include "DBT.h"


//----------------------------------------------------------------------------
DBTJsonCache::DBTJsonCache(DBTSlaveBuffered* pDBT)
	: m_pDBT(pDBT)
{
	m_pClientRecords = new RecordArray();
}


//----------------------------------------------------------------------------
DBTJsonCache::~DBTJsonCache()
{
	delete m_pClientRecords;
}


//----------------------------------------------------------------------------
bool DBTJsonCache::IsModified()
{
	return true;
}
//----------------------------------------------------------------------------
void DBTJsonCache::SetJsonLimits(int nRowFrom, int nCount)
{
	m_nStart = nRowFrom;
	m_nCount = nCount;
}
//----------------------------------------------------------------------------
void DBTJsonCache::GetJsonPatch(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	if ((m_pDBT->m_bReadOnly && m_bReadonly != B_TRUE) || (!m_pDBT->m_bReadOnly && m_bReadonly != B_FALSE))
	{
		jsonSerializer.WriteBool(_T("enabled"), !m_pDBT->m_bReadOnly);
		m_bReadonly = m_pDBT->m_bReadOnly ? B_TRUE : B_FALSE;
	}
	if (m_pDBT->GetCurrentRowIdx() != m_nCurrentRow)
	{
		m_nCurrentRow = m_pDBT->GetCurrentRowIdx();
		jsonSerializer.WriteInt(_T("currentRowIdx"), m_nCurrentRow);
	}
	BOOL rowChanged = FALSE;
	if (m_pDBT->GetRowCount() != m_nRowCount)
	{
		m_nRowCount = m_pDBT->GetRowCount();
		jsonSerializer.WriteInt(_T("rowCount"), m_nRowCount);
	}

	jsonSerializer.OpenObject(_T("prototype"));	
	m_pDBT->GetRecord()->GetJsonPatch(jsonSerializer, NULL, bOnlyWebBound);
	jsonSerializer.CloseObject(TRUE);
	jsonSerializer.OpenArray(_T("rows"));
	int i;
	for (i = 0; i < m_nCount; i++)
	{
		int idx = m_nStart + i;
		if (idx >= m_pDBT->m_pRecords->GetCount())
		{
			break;
		}
		jsonSerializer.OpenObject(i);
		SqlRecord* pCurrent = m_pDBT->m_pRecords->GetAt(i);
		SqlRecord* pOld = NULL;
		//se non c'è, lo aggiungo
		if (i >= m_pClientRecords->GetSize())
		{
			pOld = pCurrent->Create();
			m_pClientRecords->Add(pOld);
		}
		else
		{
			pOld = m_pClientRecords->GetAt(i);
		}
		pCurrent->GetJsonPatch(jsonSerializer, pOld, bOnlyWebBound);
		
		jsonSerializer.CloseObject();
	}
	jsonSerializer.CloseArray(m_nRowsSent == i);
	m_nRowsSent = i;
}

//-----------------------------------------------------------------------------	
bool DBTJsonCache::SetJson(CJsonParser& jsonParser)
{
	bool modified = false;

	if (jsonParser.BeginReadArray(_T("rows")))
	{
		for (int i = m_nStart; i < m_nStart + m_nCount; i++)
		{
			if (i >= m_pDBT->GetSize())
				continue;
			SqlRecord *pRecord = m_pDBT->GetRow(i);
			if (jsonParser.BeginReadObject(i))
			{
				modified = pRecord->SetJson(jsonParser) || modified;
				jsonParser.EndReadObject();
				int idx = i - m_nStart;
				SqlRecord* pOld = NULL;
				if (idx >= m_pClientRecords->GetSize())
				{
					pOld = pRecord->Clone();
					m_pClientRecords->Add(pOld);
				}
				else
				{
					pOld = m_pClientRecords->GetAt(i);
					*pOld = *pRecord;
				}
				
			}
		}
		jsonParser.EndReadArray();
	}
	return modified;
}