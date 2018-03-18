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
void DBTJsonCache::ResetJsonData()
{
	m_nCurrentRow = -2;
}

//----------------------------------------------------------------------------
void DBTJsonCache::SetJsonLimits(int nRowFrom, int nCount, int currentRow)
{
	m_nStart = nRowFrom;
	m_nCount = nCount;
	m_nCurrentRow = currentRow;
}

//----------------------------------------------------------------------------
void DBTJsonCache::GetJson(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound)
{
	BOOL bPatch = m_nCurrentRow == -2;
	if (!bPatch || ((m_pDBT->m_bReadOnly && m_bReadonly != B_TRUE) || (!m_pDBT->m_bReadOnly && m_bReadonly != B_FALSE)))
	{
		jsonSerializer.WriteBool(_T("enabled"), !m_pDBT->m_bReadOnly);
		m_bReadonly = m_pDBT->m_bReadOnly ? B_TRUE : B_FALSE;
	}
	if (!bPatch || m_pDBT->GetCurrentRowIdx() != m_nCurrentRow)
	{
		m_nCurrentRow = m_pDBT->GetCurrentRowIdx();
		jsonSerializer.WriteInt(_T("currentRowIdx"), m_nCurrentRow);
	}
	BOOL rowChanged = FALSE;
	if (!bPatch || m_pDBT->GetRowCount() != m_nRowCount)
	{
		m_nRowCount = m_pDBT->GetRowCount();
		jsonSerializer.WriteInt(_T("rowCount"), m_nRowCount);
	}

	jsonSerializer.OpenObject(_T("prototype"));
	if (bPatch)
		m_pDBT->GetRecord()->GetJsonPatch(jsonSerializer, NULL, bOnlyWebBound);
	else
		m_pDBT->GetRecord()->GetJson(jsonSerializer, bOnlyWebBound);

	jsonSerializer.CloseObject(TRUE);
	jsonSerializer.OpenArray(_T("rows"));
	int i = 0;
	for (int j = m_nStart; j < m_nStart + m_nCount; j++)
	{
		if (j >= m_pDBT->m_pRecords->GetCount())
		{
			break;
		}
		jsonSerializer.OpenObject(i);
		SqlRecord* pCurrent = m_pDBT->m_pRecords->GetAt(j);
		SqlRecord* pOld = NULL;
		//se non c'è, lo aggiungo
		if (j >= m_pClientRecords->GetSize())
		{
			pOld = pCurrent->Create();
			m_pClientRecords->Add(pOld);
		}
		else
		{
			pOld = m_pClientRecords->GetAt(j);
		}
		if (bPatch)
			pCurrent->GetJsonPatch(jsonSerializer, pOld, bOnlyWebBound);
		else
			pCurrent->GetJson(jsonSerializer, bOnlyWebBound);


		jsonSerializer.CloseObject();
		i++;
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
				break;
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