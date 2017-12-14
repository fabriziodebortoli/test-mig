#include "stdafx.h"

#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>

#include <TbOleDb\SqlRec.h>
#include <TbOleDb\OleDbMng.h>

#include "SqlCache.h"

#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//============================================================================
//	Static Objects & general functions
//============================================================================


//------------------------------------------------------------------------------
CDataCachingUpdatesListener* AFXAPI AfxGetDataCachingUpdatesListener ()
{
	return AfxGetLoginContext()->GetObject<CDataCachingUpdatesListener>(&CLoginContext::GetDataCachingUpdatesListener);
}

//------------------------------------------------------------------------------
const CDataCachingSettings* AFXAPI AfxGetDataCachingSettings ()
{
	return AfxGetLoginContext()->GetObject<CDataCachingSettings>(&CLoginContext::GetDataCachingSettings);
}

///////////////////////////////////////////////////////////////////////////////
// 		CCachedRecord implementation: 
//////////////////////////////////////////////////////////////////////////////
//
//------------------------------------------------------------------------------
CCachedRecord::~CCachedRecord ()
{
	SAFE_DELETE (m_pCachedRecord);
}

//------------------------------------------------------------------------------
CCachedRecord::CCachedRecord(const SqlRecord* pRecord)
	:
	m_pCachedRecord (NULL),
	m_lRef			(0)
{
	if (pRecord)
	{
		UpdateRecord(pRecord);
		m_lRef++;
	}
}

//------------------------------------------------------------------------------
void CCachedRecord::UpdateRecord(const SqlRecord* pRecord)
{
	if (!pRecord) 
		return;

	if (!m_pCachedRecord)
		m_pCachedRecord = pRecord->Create();
	
	*m_pCachedRecord = *pRecord; 
}

///////////////////////////////////////////////////////////////////////////////
// 						CCachingParamElem implementation
//////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
CCachingParamElem::CCachingParamElem (const CString& strParamList, CCachedRecord* pCachedRecord)
:
	m_pCachedRecord	(NULL),			
	m_strParams		(strParamList)
	
{
	m_pCachedRecord = pCachedRecord;
}

//----------------------------------------------------------------------------
CCachingParamElem::~CCachingParamElem()
{
	if (m_pCachedRecord)
		m_pCachedRecord->RemoveReference();

	m_pCachedRecord = NULL;
}

//----------------------------------------------------------------------------
long CCachingParamElem::GetObjectSize ()
{
	return sizeof (*this) + 
				(m_strParams.GetAllocLength() * sizeof(TCHAR)) + 
				(GetRecord() ? GetRecord()->GetRecordSize() : 0);
}

///////////////////////////////////////////////////////////////////////////////
// 						CCachingParamList implementation
//////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
CCachingParamList::CCachingParamList()
:
	m_pCachingParamArray	(NULL),
	m_pCachingParamMap		(NULL),
	m_pCurrentRecord		(NULL)	
{
	m_pCachingParamArray = new CObArray;
}

//------------------------------------------------------------------------------
CCachingParamList::~CCachingParamList()
{
	// elements are into an array 
	if (m_pCachingParamArray) 
	{
		// i remove elements from array
		for (int i = 0; i <= m_pCachingParamArray->GetUpperBound(); i++)
		{
			if (m_pCachingParamArray->GetAt(i))
				delete m_pCachingParamArray->GetAt(i);			
		}
		m_pCachingParamArray->RemoveAll();
		delete m_pCachingParamArray;
		m_pCachingParamArray = NULL;
	}

	// elements are into a map
	if (m_pCachingParamMap)
	{
		// i remove elements from the map
		POSITION			pos;
		CString				strKey;
		CCachingParamElem*	pElem;

		for (pos = m_pCachingParamMap->GetStartPosition(); pos != NULL;)
		{
			m_pCachingParamMap->GetNextAssoc(pos, strKey, (CObject*&)pElem);
			if (pElem)
				delete pElem;
		}
		m_pCachingParamMap->RemoveAll();
		delete m_pCachingParamMap;		
		m_pCachingParamMap = NULL;
	}

	m_pCurrentRecord = NULL;
}

//------------------------------------------------------------------------------
BOOL CCachingParamList::SwitchArrayToMap()
{
	if (!m_pCachingParamArray )
		return FALSE;

	if (!m_pCachingParamMap)
		m_pCachingParamMap = new CMapStringToOb;

	CCachingParamElem* pElem = NULL;

	for (int i = 0; i <= m_pCachingParamArray->GetUpperBound(); i++)
	{
		pElem =  (CCachingParamElem*)m_pCachingParamArray->GetAt(i);
		if (pElem)
			m_pCachingParamMap->SetAt(pElem->m_strParams, pElem);
	}

	delete m_pCachingParamArray;
	m_pCachingParamArray = NULL;

	return TRUE;
}

//----------------------------------------------------------------------------
long CCachingParamList::GetObjectSize ()
{
	long nCurrSize = 0;
	
	if (m_pCachingParamArray)
	{
		for (int i=0; i <= m_pCachingParamArray->GetUpperBound(); i++)
			nCurrSize += ((CCachingParamElem*) m_pCachingParamArray->GetAt(i))->GetObjectSize();
	}
	else if (m_pCachingParamMap)
	{
		// i remove elements from the map
		POSITION			pos;
		CString				strKey;
		CCachingParamElem*	pElem;

		for (pos = m_pCachingParamMap->GetStartPosition(); pos != NULL;)
		{
			m_pCachingParamMap->GetNextAssoc(pos, strKey, (CObject*&)pElem);
			if (pElem)
				nCurrSize += pElem->GetObjectSize();
		}
	}

	return 	sizeof(*this) + 
			nCurrSize + 
			(m_pCurrentRecord ? m_pCurrentRecord->GetRecordSize() : 0);
}

//------------------------------------------------------------------------------
BOOL CCachingParamList::InsertRecord (const CString& strParamList, CCachedRecord* pCachedRecord)
{
	if (!pCachedRecord)
		return FALSE;

	if (strParamList.IsEmpty())
	{
		ASSERT(FALSE);
		TRACE("CCachingParamList::InsertRecord: empty parameter list");
		return FALSE;
	}

	CCachingParamElem* pElem = NULL;
	pElem = new CCachingParamElem(strParamList, pCachedRecord);
	
	// i use array
	if (!m_pCachingParamMap)
	{
		if (!m_pCachingParamArray)
			m_pCachingParamArray = new CObArray;

		// When 100 elements are reached, I switch to the map and I copy all existing data 
		// Now I use the map
		if (m_pCachingParamArray->GetSize() == 100) 
		{
			if (!SwitchArrayToMap())
			{
				delete pElem;
				return FALSE;			
			}
		}
		else	
		{
			m_pCachingParamArray->Add(pElem);	
			return TRUE;
		}
	}
	
	// I'm using the map
	m_pCachingParamMap->SetAt(strParamList, pElem);	
	
	return TRUE;
}

//------------------------------------------------------------------------------
void CCachingParamList::DeleteRecord(CCachedRecord* pCachedRecord)
{
	m_pCurrentRecord = NULL;	
	CCachingParamElem*	pElem;

	if (m_pCachingParamArray)
	{
		for (int nIdx = 0; nIdx <= m_pCachingParamArray->GetUpperBound(); nIdx++)
		{
			pElem = ((CCachingParamElem*) m_pCachingParamArray->GetAt(nIdx));
			if (pElem && pElem->m_pCachedRecord == pCachedRecord)
			{
				m_pCachingParamArray->RemoveAt(nIdx);
				delete pElem;
			}
		}

		return;
	}

	if (m_pCachingParamMap)
	{
		// i remove elements from the map
		POSITION			pos;
		CString				strKey;
		
		for (pos = m_pCachingParamMap->GetStartPosition(); pos != NULL;)
		{
			m_pCachingParamMap->GetNextAssoc(pos, strKey, (CObject*&)pElem);
			if (pElem && pElem->m_pCachedRecord == pCachedRecord)
			{
				m_pCachingParamMap->RemoveKey(strKey);
				delete pElem;
			}
		}
	}
}

//------------------------------------------------------------------------------
BOOL CCachingParamList::FindRecord(const CString& strParamList, SqlRecord* pRecord)
{
	CCachingParamElem* pElem = NULL;
	m_pCurrentRecord = NULL;	

	// I'm using array
	if (m_pCachingParamArray)
	{
		for (int i = 0; i <= m_pCachingParamArray->GetUpperBound(); i++)
		{
			pElem =  (CCachingParamElem*)m_pCachingParamArray->GetAt(i);
			if (pElem && _tcsicmp(pElem->m_strParams, strParamList) == 0)
			{
				m_pCurrentRecord = pElem->GetRecord();
				*pRecord = *m_pCurrentRecord;
				return TRUE;
			}
		}
		return FALSE;
	}

	// I'm using map
	if (
			m_pCachingParamMap && 
			m_pCachingParamMap->Lookup(strParamList, (CObject*&)pElem) &&
			pElem
		)
	{
		m_pCurrentRecord = pElem->GetRecord();
		*pRecord = *m_pCurrentRecord;
		return TRUE;
	}

	return FALSE;	
}

///////////////////////////////////////////////////////////////////////////////
// 						CCachingQueryElem implement
//////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
CCachingQueryElem::CCachingQueryElem(const CString& strQuery)
:
	m_pCachingParams	(NULL),
	m_strQuery			(strQuery)
{
	m_pCachingParams = new CCachingParamList;
}

//------------------------------------------------------------------------------
CCachingQueryElem::~CCachingQueryElem()
{
	SAFE_DELETE(m_pCachingParams);
}

//----------------------------------------------------------------------------
long CCachingQueryElem::GetObjectSize ()
{
	return	sizeof(*this) +
			(m_strQuery.GetAllocLength() * sizeof(TCHAR)) + 
			(m_pCachingParams ? m_pCachingParams->GetObjectSize() : 0);
}

//-----------------------------------------------------------------------------
BOOL CCachingQueryElem::InsertRecord(const CString& strParamList, CCachedRecord* pCachedRecord)
{
	return m_pCachingParams->InsertRecord(strParamList, pCachedRecord);
}

//-----------------------------------------------------------------------------
void CCachingQueryElem::DeleteRecord(CCachedRecord* pCachedRecord)
{
	m_pCachingParams->DeleteRecord(pCachedRecord);	
}

//-----------------------------------------------------------------------------
BOOL CCachingQueryElem::FindRecord(const CString& strParamList, SqlRecord* pRecord)
{
	return m_pCachingParams->FindRecord(strParamList, pRecord);
}

///////////////////////////////////////////////////////////////////////////////
// 						CCachedTable implementation
//////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
CCachedTable::CCachedTable(const CString& strTableName)
:
	m_strTableName		(strTableName),
	m_pCachingQueryArray(NULL),
	m_pCachingQueryMap	(NULL),
	m_pCachingRecordMap (NULL),
	m_pCurrentQuery		(NULL)
{
	m_pCachingQueryArray = new CObArray;
	m_pCachingRecordMap	 = new CMapStringToOb;
}

//------------------------------------------------------------------------------
CCachedTable::~CCachedTable()
{	
	RemoveQueries();
	RemoveAllCachedRecords();
	
	if (m_pCachingQueryMap)
	{
		delete m_pCachingQueryMap;		
		m_pCachingQueryMap = NULL;
	}

	if (m_pCachingQueryArray)
	{
		delete m_pCachingQueryArray;
		m_pCachingQueryArray = NULL;
	}
	
	if (m_pCachingRecordMap)
	{
		delete m_pCachingRecordMap;		
		m_pCachingRecordMap = NULL;
	}	
	m_pCurrentQuery = NULL;
}

//------------------------------------------------------------------------------
BOOL CCachedTable::SwitchArrayToMap()
{
	if (!m_pCachingQueryArray )
		return FALSE;

	if (!m_pCachingQueryMap)
		m_pCachingQueryMap = new CMapStringToOb;

	CCachingQueryElem* pElem = NULL;

	for (int i = 0; i <= m_pCachingQueryArray->GetUpperBound(); i++)
	{
		pElem =  (CCachingQueryElem*)m_pCachingQueryArray->GetAt(i);
		if (pElem)
			m_pCachingQueryMap->SetAt(pElem->m_strQuery, pElem);
	}

	delete m_pCachingQueryArray;
	m_pCachingQueryArray = NULL;

	return TRUE;
}

//------------------------------------------------------------------------------
long CCachedTable::GetObjectSize ()
{
	long nCurrSize = 0;
	
	if (m_pCachingQueryArray)
	{
		for (int i=0; i <= m_pCachingQueryArray->GetUpperBound(); i++)
			nCurrSize += ((CCachingQueryElem*) m_pCachingQueryArray->GetAt(i))->GetObjectSize();
	}
	else if (m_pCachingQueryMap)
	{
		// i remove elements from the map
		POSITION			pos;
		CString				strKey;
		CCachingQueryElem*	pElem;

		for (pos = m_pCachingQueryMap->GetStartPosition(); pos != NULL;)
		{
			m_pCachingQueryMap->GetNextAssoc(pos, strKey, (CObject*&)pElem);
			if (pElem)
				nCurrSize += pElem->GetObjectSize();
		}
	}

	return	sizeof (*this) + 
			nCurrSize  +
			(m_strTableName.GetAllocLength() * sizeof(TCHAR)) + 
			(m_pCurrentQuery ? m_pCurrentQuery->GetObjectSize() : 0);
}

//------------------------------------------------------------------------------
BOOL CCachedTable::FindQuery (const CString& strQuery, BOOL bToRemove /*=FALSE*/)
{
	if (!bToRemove && m_pCurrentQuery && _tcsicmp(m_pCurrentQuery->m_strQuery, strQuery) == 0)
		return TRUE;

	CCachingQueryElem* pElem = NULL;

	if (m_pCachingQueryArray) 
	{
		for (int i = 0; i <= m_pCachingQueryArray->GetUpperBound(); i++)
		{
			pElem =  (CCachingQueryElem*) m_pCachingQueryArray->GetAt(i);
			if (pElem && _tcsicmp(pElem->m_strQuery, strQuery) == 0)
			{
				if (bToRemove)
				{
					m_pCachingQueryArray->RemoveAt(i);
					if (pElem == m_pCurrentQuery)
						m_pCurrentQuery = NULL;
				}
				else
					m_pCurrentQuery = pElem;
			}
		}
		return FALSE;
	}

	// map
	if (
			m_pCachingQueryMap && 
			m_pCachingQueryMap->Lookup(strQuery, (CObject*&)pElem) &&
			pElem
		)
	{
		if (bToRemove)
			m_pCachingQueryMap->RemoveKey(strQuery);
		else
			m_pCurrentQuery = pElem;
	}

	if (pElem && bToRemove)
	{
		if (pElem == m_pCurrentQuery)
			m_pCurrentQuery = NULL;
		delete pElem;
	}
		
	return pElem || bToRemove;
}

//------------------------------------------------------------------------------
BOOL CCachedTable::InsertQuery(const CString& strQuery)
{
	if (strQuery.IsEmpty())
		return FALSE;

	CCachingQueryElem* pElem = NULL;
	pElem = new CCachingQueryElem(strQuery);
	
	// array
	if (!m_pCachingQueryMap)
	{
		if (!m_pCachingQueryArray)
		{
			delete pElem;
			return FALSE;
		}

		// When 100 elements are reached, I switch to the map and I copy all existing data 
		// Now I use the map
		if (m_pCachingQueryArray->GetSize() == 100) 
		{
			if (!SwitchArrayToMap())
			{
				delete pElem;	
				return FALSE;			
			}
		}
		else	
			m_pCachingQueryArray->Add(pElem);	
	}
	else
		m_pCachingQueryMap->SetAt(strQuery, pElem);	

	m_pCurrentQuery = pElem;

	return TRUE;
}

//------------------------------------------------------------------------------
BOOL CCachedTable::CheckCurrentQuery(const CString& strQuery)
{
	BOOL bFoundQuery = FindQuery(strQuery); 
	if (!bFoundQuery)
		bFoundQuery = InsertQuery(strQuery);
	
	return bFoundQuery;
}
	
//------------------------------------------------------------------------------
BOOL CCachedTable::InsertRecord(const CString& strQuery, const CString& strParamList, SqlRecord* pRecord)
{
	if (!pRecord)
		return FALSE;

	if (strQuery.IsEmpty())
	{
		ASSERT(FALSE);
		TRACE("CCachedTable::InsertRecord: empty query");
		return FALSE;
	}
	
	if (strParamList.IsEmpty())
	{
		ASSERT(FALSE);
		TRACE("CCachedTable::InsertRecord: empty parameter list");
		return FALSE;
	}

	if (!CheckCurrentQuery(strQuery)) 
		return FALSE;

	// devo cercare il record in m_pCachingRecordMap
	// se esiste devo sostituire i valori dei campi altrimento lo devo inserire
	CString strKeyDescri = pRecord->GetPrimaryKeyDescription();
	CCachedRecord* pCachedRecord = NULL;
	if (!m_pCachingRecordMap->Lookup(strKeyDescri, (CObject*&)pCachedRecord) || !pCachedRecord)
	{
		pCachedRecord = new CCachedRecord(pRecord);
		m_pCachingRecordMap->SetAt(strKeyDescri, pCachedRecord);
	}
	else 
	{
		pCachedRecord->UpdateRecord(pRecord);
		pCachedRecord->AddReference();
	}

	return m_pCurrentQuery->InsertRecord(strParamList, pCachedRecord);
}

//------------------------------------------------------------------------------
BOOL CCachedTable::FindRecord(const CString& strQuery, const CString& strParamList, SqlRecord* pRecord)
{
	if (!CheckCurrentQuery(strQuery)) 
		return FALSE;

	return m_pCurrentQuery->FindRecord(strParamList, pRecord);
}

//------------------------------------------------------------------------------
BOOL CCachedTable::RemoveQuery(const CString& strQuery)
{
	return FindQuery(strQuery, TRUE);
}

//------------------------------------------------------------------------------
void CCachedTable::RemoveQueries()
{
	POSITION			pos;
	CString				strKey;
	CCachingQueryElem*	pElem;

	m_pCurrentQuery = NULL;

	// array
	if (m_pCachingQueryArray)
	{
		// remove elements from array
		for (int i = 0; i <= m_pCachingQueryArray->GetUpperBound(); i++)
		{
			if (m_pCachingQueryArray->GetAt(i))
				delete m_pCachingQueryArray->GetAt(i);
		}
		m_pCachingQueryArray->RemoveAll();
	}
	else
	// map
	if (m_pCachingQueryMap)
	{
		// remove elements from map
		for (pos = m_pCachingQueryMap->GetStartPosition(); pos != NULL;)
		{
			m_pCachingQueryMap->GetNextAssoc(pos, strKey, (CObject*&)pElem);
			if (pElem)
				delete pElem;			
		}
		m_pCachingQueryMap->RemoveAll();
	}
}

//------------------------------------------------------------------------------
void CCachedTable::RemoveAllCachedRecords()
{
	POSITION		pos;
	CString			strKey;
	CCachedRecord*	pCachedRec;

	m_pCurrentQuery = NULL;
	if (m_pCachingRecordMap)
	{
		for (pos = m_pCachingRecordMap->GetStartPosition(); pos != NULL;)
		{
			m_pCachingRecordMap->GetNextAssoc(pos, strKey, (CObject*&)pCachedRec);
			if (pCachedRec)
				delete pCachedRec;			
		}
		m_pCachingRecordMap->RemoveAll();
	}
}

//------------------------------------------------------------------------------
BOOL CCachedTable::RemoveFirstQuery ()
{
	CCachingQueryElem* pElem = NULL;
		
	if (m_pCachingQueryArray && m_pCachingQueryArray->GetSize())
	{
		pElem = (CCachingQueryElem*) m_pCachingQueryArray->GetAt(0);
		if (pElem)
		{
			m_pCachingQueryArray->RemoveAt (0);
			if (pElem == m_pCurrentQuery)
				m_pCurrentQuery = NULL;
			delete pElem;
		}
		return TRUE;
	}
	else
	if (m_pCachingQueryMap)
	{
		// I remove elements from the map
		POSITION	pos = m_pCachingQueryMap->GetStartPosition();
		CString		strKey;
		m_pCachingQueryMap->GetNextAssoc(pos, strKey, (CObject*&)pElem);
		if (pElem)
		{
			m_pCachingQueryMap->RemoveKey(strKey);
			if (pElem == m_pCurrentQuery)
				m_pCurrentQuery = NULL;
			delete pElem;
		}
	}
	
	return TRUE;
}

//------------------------------------------------------------------------------
void CCachedTable::Clear ()
{
	RemoveQueries();
	RemoveAllCachedRecords();
}

// items invalidation
//------------------------------------------------------------------------------
BOOL CCachedTable::UpdateRecord(SqlRecord* pRecord)
{
	if (!pRecord) return FALSE;

	// i look for record in m_pCachingRecordMap
	// if it exists, i replace fields values
	CString strKeyDescri = pRecord->GetPrimaryKeyDescription();
	CCachedRecord* pCachedRecord = NULL;

	if (m_pCachingRecordMap->Lookup(strKeyDescri, (CObject*&)pCachedRecord) && pCachedRecord)
		pCachedRecord->UpdateRecord(pRecord);

	return TRUE;	
}

//------------------------------------------------------------------------------
void CCachedTable::RemoveSingleCachedRecord(CCachedRecord* pCachedRecord)
{
	CCachingQueryElem* pElem = NULL;
	if (m_pCachingQueryArray) 
	{
		for (int nIdx = 0; nIdx <= m_pCachingQueryArray->GetUpperBound(); nIdx++)
		{
			if (pElem = (CCachingQueryElem*)m_pCachingQueryArray->GetAt(nIdx))
			{
				pElem->DeleteRecord(pCachedRecord);		
				if (pElem->IsEmpty())
				{
					m_pCachingQueryArray->RemoveAt(nIdx);
					delete pElem;
				}
			}

		}

		return;
	}

	if (m_pCachingQueryMap)
	{
		// i remove elements from the map
		POSITION	pos = 0;
		CString		strKey;
		for (pos = m_pCachingQueryMap->GetStartPosition(); pos != NULL;)
		{
			m_pCachingQueryMap->GetNextAssoc(pos, strKey, (CObject*&)pElem);
			
			if (!pElem)	
				continue;

			pElem->DeleteRecord(pCachedRecord);
			if (pElem->IsEmpty())
			{
				m_pCachingQueryMap->RemoveKey (strKey);
				delete pElem;
			}
		}		
	}
}

//------------------------------------------------------------------------------
void CCachedTable::DeleteRecord(SqlRecord* pRecord)
{
	if (!pRecord) return;

	m_pCurrentQuery = NULL;

	// i look for record in m_pCachingRecordMap. If it exists,
	// I loop into queries to in order to delete CCachedRecord reference
	CString strKeyDescri = pRecord->GetPrimaryKeyDescription();
	CCachedRecord* pCachedRecord = NULL;

	if (m_pCachingRecordMap->Lookup(strKeyDescri, (CObject*&)pCachedRecord) && pCachedRecord)
	{
		// query and query parameters loop
		RemoveSingleCachedRecord(pCachedRecord);	
		m_pCachingRecordMap->RemoveKey(strKeyDescri);
		delete pCachedRecord;
	}
}

///////////////////////////////////////////////////////////////////////////////
// 		CCachedTableArray implementation
//////////////////////////////////////////////////////////////////////////////
CCachedTableArray::CCachedTableArray()
	:
	m_pCurrentCachedTable(NULL)
{
}

//------------------------------------------------------------------------------
CCachedTableArray::~CCachedTableArray()
{
	m_pCurrentCachedTable = NULL;
}

//------------------------------------------------------------------------------
CCachedTable* CCachedTableArray::GetCachedTable (const CString& strTableName)
{
	CCachedTable* pTable = NULL;
	for(int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pTable = GetAt(nIdx);
		if (_tcsicmp(pTable->m_strTableName, strTableName)== 0)
			return pTable;
	}
	return NULL;
}

//------------------------------------------------------------------------------
int CCachedTableArray::GetCachedTableIdx (const CString& strTableName)
{
	CCachedTable* pTable = NULL;

	for(int nIdx = 0; nIdx < GetSize(); nIdx++)
	{
		pTable = GetAt(nIdx);
		if (_tcsicmp(pTable->m_strTableName, strTableName)== 0)
			return nIdx;
	}
	return -1;
}

//------------------------------------------------------------------------------
void CCachedTableArray::CheckCurrentTable(const CString& strTableName)
{
	if (m_pCurrentCachedTable && m_pCurrentCachedTable->m_strTableName.CompareNoCase(strTableName) == 0)
		return;
	
	m_pCurrentCachedTable = GetCachedTable(strTableName);
	
	if (!m_pCurrentCachedTable)
	{
		CCachedTable* pCachedTable =  new CCachedTable(strTableName);
		Add((CObject*)pCachedTable);
		m_pCurrentCachedTable = pCachedTable;
	}
}

//------------------------------------------------------------------------------
BOOL CCachedTableArray::FindRecord(const CString& strTableName, const CString& strQuery, const CString& strParamList, SqlRecord* pRecord)
{
	CheckCurrentTable(strTableName);	

	return m_pCurrentCachedTable->FindRecord(strQuery, strParamList, pRecord);
}

//------------------------------------------------------------------------------
BOOL CCachedTableArray::InsertRecord(const CString& strQuery, const CString& strParamList, SqlRecord* pRecord)
{
	if(!pRecord)
		return FALSE;
	CheckCurrentTable(pRecord->GetTableName());	
	return m_pCurrentCachedTable->InsertRecord(strQuery, strParamList, pRecord);
}

//------------------------------------------------------------------------------
void CCachedTableArray::RemoveTableQueries(const CString& strTableName)
{
	CCachedTable* pCachedTable = GetCachedTable(strTableName);
	if (pCachedTable)
	{
		pCachedTable->RemoveQueries();
		delete pCachedTable;
		m_pCurrentCachedTable = NULL;
	}
}

//------------------------------------------------------------------------------
void CCachedTableArray::RemoveFirstQuery ()
{
	if (!GetSize())
		return;

	CCachedTable* pCachedTable = GetAt(0);
	if (pCachedTable)
	{
		pCachedTable->RemoveQueries();
		delete pCachedTable;
		m_pCurrentCachedTable = NULL;
	}
}

// items invalidation
//------------------------------------------------------------------------------
BOOL CCachedTableArray::UpdateRecord(SqlRecord* pRecord)
{
	if (!pRecord)
		return TRUE;

	CCachedTable* pCachedTable = GetCachedTable(pRecord->GetTableName());

	return !pCachedTable || pCachedTable->UpdateRecord(pRecord);
}

//------------------------------------------------------------------------------
void CCachedTableArray::DeleteRecord(SqlRecord* pRecord)
{
	if (!pRecord)
		return;

	m_pCurrentCachedTable = NULL;
	int nIdx = GetCachedTableIdx(pRecord->GetTableName());
	if (nIdx < 0)
		return;

	CCachedTable* pCachedTable = GetAt(nIdx);
	if (pCachedTable)
	{
		pCachedTable->DeleteRecord(pRecord);
		if (pCachedTable->IsEmpty())
			RemoveAt(nIdx);
	}
}

//------------------------------------------------------------------------------
void CCachedTableArray::Clear ()
{
	for(int nIdx = 0; nIdx < GetSize(); nIdx++)
		if (GetAt(nIdx))
			GetAt(nIdx)->Clear();
	
	m_pCurrentCachedTable = NULL;
}

//----------------------------------------------------------------------------
long CCachedTableArray::GetObjectSize ()
{
	return	sizeof (*this) + 
			(m_pCurrentCachedTable ? m_pCurrentCachedTable->GetObjectSize() : 0);
}

///////////////////////////////////////////////////////////////////////////////
// 		CDataCachingSettings implementation
//////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CDataCachingSettings, CObject)

//------------------------------------------------------------------------------
CDataCachingSettings::CDataCachingSettings ()
	:
	m_bDataCachingEnabled		(FALSE),
	m_CacheScope				(CDataCachingSettings::DOCUMENT_CONTEXT),
	m_lCheckMilliSeconds		(0),
	m_lExpirationMilliSeconds	(0),
	m_lMaxBytesSize				(0),
	m_nReductionPerc			(0.0)
{
	LoadSettings ();
}

//------------------------------------------------------------------------------
void CDataCachingSettings::LoadSettings ()
{
	CDiagnostic* pDiagnostic = AfxGetDiagnostic();
	DataObj* pDataObj = NULL;
	pDataObj = AfxGetSettingValue 
			(
				snsTbOleDb, 
				szDataCaching, 
				szDataCachingEnable, 
				DataBool(m_bDataCachingEnabled),
				szTbDefaultSettingFileName
			);
	m_bDataCachingEnabled = pDataObj ? *((DataBool*) pDataObj) : m_bDataCachingEnabled;

	// if cache is not enabled I skip all other settings read.
	if (!m_bDataCachingEnabled)
		return;

	CString sHeaderMessage = _TB("Data Caching Management System Parameters:");
	BOOL	bHeaderMessage = FALSE;

	pDataObj = AfxGetSettingValue 
			(
				snsTbOleDb, 
				szDataCaching, 
				szDataCacheScope, 
				DataInt(1),
				szTbDefaultSettingFileName
			);
	int nCacheScope = (int) (pDataObj ? *((DataInt*) pDataObj) : 1);
	if (nCacheScope < 0 || nCacheScope > 3)
	{
		ASSERT (FALSE);
		if (pDiagnostic)
		{
			if (!bHeaderMessage)
			{
				pDiagnostic->Add (sHeaderMessage, CDiagnostic::Info);
				bHeaderMessage = TRUE;
			}

			pDiagnostic->Add (cwsprintf(_TB("- CacheScope parameter is not correct: the value is %d. "), nCacheScope), CDiagnostic::Info);
			pDiagnostic->Add (_TB("  Cache scope will be automatically set to '1=Document Life'"), CDiagnostic::Info);
		}

		TRACE1	("Data Caching Management: CacheScope parameter is not correct: the value is %d.\nCache scope will be automatically set to '1=Document Life'\n", nCacheScope);
		nCacheScope = 1;
	}
	m_CacheScope = (CacheScope) nCacheScope;

	pDataObj = AfxGetSettingValue 
			(
				snsTbOleDb, 
				szDataCaching, 
				szDataCacheCheckSeconds, 
				DataInt(60),
				szTbDefaultSettingFileName
			);
	int nSeconds = pDataObj ? *((DataInt*) pDataObj) : 60;
	if (nSeconds < 0)
	{
		ASSERT (FALSE);
		if (pDiagnostic)
		{
			if (!bHeaderMessage)
			{
				pDiagnostic->Add (sHeaderMessage, CDiagnostic::Info);
				bHeaderMessage = TRUE;
			}

			pDiagnostic->Add (cwsprintf(_TB("- CacheCheckSeconds parameter is less than zero: %d. "), nSeconds), CDiagnostic::Info);
			pDiagnostic->Add (_TB("  Data Caching will not manage automatic cache resize and expiration policies."), CDiagnostic::Info);
		}
		TRACE1	("Data Caching Management: CacheCheckSeconds parameter is less than zero: %d. Data Caching will not manage automatic cache resize and expiration policies.\n", nSeconds);
		nSeconds = 0;
	}

	m_lCheckMilliSeconds = nSeconds * 1000;

	pDataObj = AfxGetSettingValue 
			(
				snsTbOleDb, 
				szDataCaching, 
				szDataCacheExpirationSeconds, 
				DataInt(120),
				szTbDefaultSettingFileName
			);
	nSeconds = pDataObj ? *((DataInt*) pDataObj) : 120;
	if (nSeconds < 0)
	{
		ASSERT (FALSE);
		if (pDiagnostic)
		{
			if (!bHeaderMessage)
			{
				pDiagnostic->Add (sHeaderMessage, CDiagnostic::Info);
				bHeaderMessage = TRUE;
			}

			pDiagnostic->Add (cwsprintf(_TB("- CacheExpirationSeconds parameter is less than zero: %d. "), nSeconds), CDiagnostic::Info);
			pDiagnostic->Add (_TB("  CacheExpirationSeconds will be automatically set to 0 value"), CDiagnostic::Info);
		}
		TRACE1	("Data Caching Management: CacheExpirationSeconds parameter is less than zero: %d. CacheExpirationSeconds will be automatically set to 0 value.\n", nSeconds);
		nSeconds = 0;
	}
	m_lExpirationMilliSeconds = nSeconds * 1000;

	pDataObj = AfxGetSettingValue 
			(
				snsTbOleDb, 
				szDataCaching, 
				szDataCacheMaxKbSize, 
				DataInt(20480),
				szTbDefaultSettingFileName
			);
	int nKbSize = pDataObj ? *((DataInt*) pDataObj) : 20480;
	if (nKbSize < 0)
	{
		ASSERT (FALSE);
		if (pDiagnostic)
		{
			if (!bHeaderMessage)
			{
				pDiagnostic->Add (sHeaderMessage, CDiagnostic::Info);
				bHeaderMessage = TRUE;
			}

			pDiagnostic->Add (cwsprintf(_TB("- CacheMaxKbSize parameter is less than zero: %d. "), nSeconds), CDiagnostic::Info);
			pDiagnostic->Add (_TB("  CacheMaxKbSize will be automatically set to 0 value"), CDiagnostic::Info);
		}
		TRACE1	("Data Caching Management: CacheMaxKbSize parameter is less than zero: %d. CacheMaxKbSize will be automatically set to 0 value.\n", nSeconds);
		nSeconds = 0;
	}

	m_lMaxBytesSize = nKbSize * 1024;

	pDataObj = AfxGetSettingValue 
			(
				snsTbOleDb, 
				szDataCaching, 
				szDataCacheReductionPerc, 
				DataPerc(20.0),
				szTbDefaultSettingFileName
			);
	m_nReductionPerc = pDataObj ? *((DataPerc*) pDataObj) : 20.0;

	if (m_lMaxBytesSize && (m_nReductionPerc <= 0.0 || m_nReductionPerc >= 100.0))
	{
		ASSERT(FALSE);
		if (pDiagnostic)
		{
			if (!bHeaderMessage)
			{
				pDiagnostic->Add (sHeaderMessage, CDiagnostic::Info);
				bHeaderMessage = TRUE;
			}

			pDiagnostic->Add (cwsprintf(_TB("- CacheReductionPerc and CacheMaxKbSize parameters are not consistent: CacheReductionPerc value %d where CacheMaxKbSize is %d. "), m_nReductionPerc, m_lMaxBytesSize), CDiagnostic::Info);
			pDiagnostic->Add (_TB("  The cache dimensions will not be resized"), CDiagnostic::Info);
		}
		TRACE2 ("Data Caching Management: CacheReductionPerc and CacheMaxKbSize parameters are not consistent: CacheReductionPerc value %d where CacheMaxKbSize is %d. The cache dimensions will not be resized.\n", m_nReductionPerc, m_lMaxBytesSize);
		
		m_nReductionPerc = 0;
		m_lMaxBytesSize	 = 0;
	}
}

//------------------------------------------------------------------------------
const BOOL&	CDataCachingSettings::IsDataCachingEnabled () const
{
	return m_bDataCachingEnabled;
}

//------------------------------------------------------------------------------
const CDataCachingSettings::CacheScope& CDataCachingSettings::GetCacheScope () const
{
	return m_CacheScope;
}

//------------------------------------------------------------------------------
const BOOL	CDataCachingSettings::IsDocumentScope () const
{
	return (
				m_CacheScope == CDataCachingSettings::DOCUMENT_CONTEXT ||
				m_CacheScope == CDataCachingSettings::DOCUMENT_TRANSACTION
			);
}

//------------------------------------------------------------------------------
const long& CDataCachingSettings::GetCheckMilliSeconds () const
{
	return m_lCheckMilliSeconds;
}

//------------------------------------------------------------------------------
const long&	CDataCachingSettings::GetExpirationMilliSeconds () const
{
	return m_lExpirationMilliSeconds;
}

//------------------------------------------------------------------------------
const long&	CDataCachingSettings::GetMaxBytesSize () const
{
	return m_lMaxBytesSize;
}

//------------------------------------------------------------------------------
const double& CDataCachingSettings::GetReductionPerc () const
{
	return m_nReductionPerc;
}

///////////////////////////////////////////////////////////////////////////////
// CDataCachingUpdatesListener
///////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNCREATE(CDataCachingUpdatesListener, CObject)
//------------------------------------------------------------------------------
CDataCachingUpdatesListener::CDataCachingUpdatesListener ()
{
}

//-----------------------------------------------------------------------------
BOOL CDataCachingUpdatesListener::UpdateRecord (SqlRecord* pRecord)
{
	if (!pRecord)
		return FALSE;

	CDataCachingManager* pManager = NULL;
	BOOL bOk = TRUE;

	TB_LOCK_FOR_WRITE();
	for (int i=0; i <= m_pWorkingManagers.GetUpperBound(); i++)
	{
		pManager = (CDataCachingManager*) m_pWorkingManagers.GetAt(i);

		if (pManager && !pManager->UpdateRecord (pRecord))
			bOk = FALSE;
	}

	return bOk;
}

//-----------------------------------------------------------------------------
void CDataCachingUpdatesListener::DeleteRecord(SqlRecord* pRecord)
{
	if (!pRecord)
		return;

	CDataCachingManager* pManager = NULL;
	
	TB_LOCK_FOR_WRITE();
	for (int i=0; i <= m_pWorkingManagers.GetUpperBound(); i++)
	{
		pManager = (CDataCachingManager*) m_pWorkingManagers.GetAt(i);

		if (pManager)
			pManager->DeleteRecord (pRecord);
	}
}

//------------------------------------------------------------------------------
void CDataCachingUpdatesListener::RemoveManager (CDataCachingManager* pManager)
{
	if (!pManager)
		return;

	TB_LOCK_FOR_WRITE();
	for (int i= m_pWorkingManagers.GetUpperBound(); i >= 0; i--)
		if (pManager == (CDataCachingManager*) m_pWorkingManagers.GetAt(i))
			m_pWorkingManagers.RemoveAt(i);
}

// I don't check if manager ha already been added to speed performance.
//------------------------------------------------------------------------------
void CDataCachingUpdatesListener::AddManager (CDataCachingManager* pManager)
{
	if (!pManager)
		return;

	TB_LOCK_FOR_WRITE();
	m_pWorkingManagers.Add(pManager);
}

///////////////////////////////////////////////////////////////////////////////
// 		CDataCacheValidator implementation
//////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
CDataCacheValidator::CDataCacheValidator ()
	:
	m_pDataCache		(NULL),
	m_wLastCheck		(GetTickCount()),
	m_wLastExpiration	(GetTickCount())
{
}

//------------------------------------------------------------------------------
CDataCacheValidator::~CDataCacheValidator ()
{
	m_pDataCache = NULL;
}

//------------------------------------------------------------------------------
void CDataCacheValidator::AttachCache (CCachedTableArray* pDataCache)
{
	m_pDataCache  = pDataCache;
	ASSERT(m_pDataCache);
}

//------------------------------------------------------------------------------
void CDataCacheValidator::CheckCacheStatus ()
{
	if (!IsCheckTime())
		return;

	// first I check if cache must expire
	if (IsExpired ())
		Expire ();

	// if cache is not expired, I check size
	if (IsTooLarge ())
		Resize ();

	m_wLastCheck = GetTickCount ();
}

//------------------------------------------------------------------------------
BOOL CDataCacheValidator::IsCheckTime ()
{
	DWORD lCheck	= AfxGetDataCachingSettings()->GetCheckMilliSeconds();
	DWORD lElapsed	= GetTickCount ()- m_wLastCheck;

	return lCheck && lElapsed >= lCheck;
}

//------------------------------------------------------------------------------
BOOL CDataCacheValidator::IsExpired ()
{
	DWORD lExpiration = AfxGetDataCachingSettings()->GetExpirationMilliSeconds();
	DWORD lElapsed	 =  (GetTickCount ()- m_wLastExpiration);

	return lExpiration && lElapsed >= lExpiration;
}

//------------------------------------------------------------------------------
BOOL CDataCacheValidator::IsTooLarge ()
{
	if (!m_pDataCache)
		return FALSE;
	
	double	nPercReduction	= AfxGetDataCachingSettings()->GetReductionPerc();
	long	nMaxKbSize		= AfxGetDataCachingSettings()->GetMaxBytesSize();
	
	if (nMaxKbSize && (nPercReduction <= 0.0 || nPercReduction >= 100.0))
		return FALSE;

	long lCurrentSize = m_pDataCache->GetObjectSize ();
	return nMaxKbSize && lCurrentSize > nMaxKbSize;
}

//------------------------------------------------------------------------------
void CDataCacheValidator::Expire ()
{
	m_pDataCache->Clear ();

	m_wLastExpiration = GetTickCount ();
}

// this method can be enhanced with LRU, MRU and other cache clean criteria
//------------------------------------------------------------------------------
void CDataCacheValidator::Resize ()
{
	if (!m_pDataCache)
		return;

	// removes first query that is probably the first used
	long lOriginalSize = m_pDataCache->GetObjectSize ();
	long lCurrentSize  = lOriginalSize;
	
	double lSizeToObtain = lOriginalSize - (lOriginalSize * AfxGetDataCachingSettings()->GetReductionPerc() / 100.0);

	while (lCurrentSize  > ((long) lSizeToObtain))
	{
		m_pDataCache->RemoveFirstQuery();
		lCurrentSize  = m_pDataCache->GetObjectSize ();
	}
}

///////////////////////////////////////////////////////////////////////////////
// 		CDataCachingManager implementation
//////////////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------------------
CDataCachingManager::CDataCachingManager ()
:
	m_pCache	(NULL)
{
	m_pCache = new CCachedTableArray;
	m_Validator.AttachCache (m_pCache);
}

//-----------------------------------------------------------------------------
CDataCachingManager::~CDataCachingManager()
{
	SAFE_DELETE(m_pCache);
}

//-----------------------------------------------------------------------------
void CDataCachingManager::ClearCache ()
{
	START_PROC_TIME(DATA_CACHE_CLEAR);

	if (m_pCache)
	{
		TB_LOCK_FOR_WRITE();
		m_pCache->Clear();	
		m_Validator.CheckCacheStatus ();
	}

	STOP_PROC_TIME(DATA_CACHE_CLEAR);
}


//-----------------------------------------------------------------------------
BOOL CDataCachingManager::FindRecord (const CString& strTableName, const CString& strQuery, const CString& strParamList, SqlRecord* pRecord)
{
	START_PROC_TIME(DATA_CACHE_FIND);

	BOOL bFound = FALSE;

	// no lock, pointer to m_pCache never changes
	if (m_pCache && !strTableName.IsEmpty() && !strQuery.IsEmpty() && !strParamList.IsEmpty())
	{
		TB_LOCK_FOR_WRITE();
		m_Validator.CheckCacheStatus ();
		bFound = m_pCache->FindRecord(strTableName, strQuery, strParamList, pRecord);
	}

	STOP_PROC_TIME(DATA_CACHE_FIND);
	
	return bFound;
}

//-----------------------------------------------------------------------------
BOOL CDataCachingManager::InsertRecord	(
											const CString& strQuery, 
											const CString& strParamList, 
											SqlRecord* pRecord
										) 
{ 
	START_PROC_TIME(DATA_CACHE_INSERT);

	BOOL bOk = FALSE;
	// no lock, pointer to m_pCache never changes
	if (m_pCache && pRecord && !strQuery.IsEmpty() && !strParamList.IsEmpty())
	{
		TB_LOCK_FOR_WRITE();
		bOk = m_pCache->InsertRecord(strQuery, strParamList, pRecord);
		m_Validator.CheckCacheStatus ();
	}
	
	STOP_PROC_TIME(DATA_CACHE_INSERT);

	return bOk;
}

//------------------------------------------------------------------------------
BOOL CDataCachingManager::UpdateRecord (SqlRecord* pRecord)
{
	START_PROC_TIME(DATA_CACHE_RECORD_REFRESHED);

	BOOL bOk = FALSE;
	
	// no lock, pointer to m_pCache never changes
	if (m_pCache && pRecord)
	{
		TB_LOCK_FOR_WRITE();
		m_pCache->UpdateRecord(pRecord);
		m_Validator.CheckCacheStatus ();
	}
	
	STOP_PROC_TIME(DATA_CACHE_RECORD_REFRESHED);

	return bOk;
}

//------------------------------------------------------------------------------
void CDataCachingManager::DeleteRecord(SqlRecord* pRecord)
{
	START_PROC_TIME(DATA_CACHE_RECORD_DELETED);

	// no lock, pointer to m_pCache never changes
	if (m_pCache && pRecord)
	{
		TB_LOCK_FOR_WRITE();
		m_pCache->DeleteRecord(pRecord);
		m_Validator.CheckCacheStatus ();
	}

	STOP_PROC_TIME(DATA_CACHE_RECORD_DELETED);
}
