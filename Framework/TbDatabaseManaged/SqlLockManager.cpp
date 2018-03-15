#include "StdAfx.h"

#include "MSqlConnection.h"
#include "SqlLockManager.h"



/////////////////////////////////////////////////////////////////////////////
// 				class LockEntry Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
LockEntry::LockEntry(const CString& strLockKey, const CString& strContext)
{
	m_strLockKey = strLockKey;
	m_strContext = strContext;
	//LockDate = lockTime;
	m_strLockUser = _T("");
	m_strLockApp = _T("");
}

//----------------------------------------------------------------------------	
bool LockEntry::IsSameLock(LockEntry* pSqlLockEntry)
{
	return
		(
			pSqlLockEntry != nullptr &&
			m_strLockKey == pSqlLockEntry->m_strLockKey &&
			m_strContext == pSqlLockEntry->m_strContext
			);
}

//----------------------------------------------------------------------------	
bool LockEntry::IsSameLock(const CString& strContext)
{
	return m_strContext == strContext;
}


/////////////////////////////////////////////////////////////////////////////
// 				class TableLockEntry Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
TableLockEntry::TableLockEntry(const CString& strTableName)
{
	m_strTableName = strTableName;
}

//----------------------------------------------------------------------------	
LockEntry* TableLockEntry::GetLockByLockKey(const CString& strLockKey)
{
	if (strLockKey.IsEmpty())
		return NULL;

	LockEntry* pLockEntry = NULL;

	//verifico che il lock non sia stato già inserito.
	for (int i = 0; i < m_SqlLocksEntries.GetSize(); i++)
	{
		pLockEntry = (LockEntry*)m_SqlLocksEntries[i];
		//se è già stato inserito
		if (pLockEntry->m_strLockKey == strLockKey)
			return pLockEntry;
	}

	return NULL;
}


//----------------------------------------------------------------------------	
void TableLockEntry::AddLockEntry(LockEntry* pNewSqlLockEntry)
{
	m_SqlLocksEntries.Add(pNewSqlLockEntry);
}

//----------------------------------------------------------------------------	
bool TableLockEntry::RemoveLockEntry(const CString& strLockKey)
{
	if (strLockKey.IsEmpty())
		return true;

	LockEntry* pLockEntry = NULL;
	for (int i = m_SqlLocksEntries.GetUpperBound(); i >= 0; i--)
	{
		pLockEntry = (LockEntry*)m_SqlLocksEntries[i];
		//se è già stato inserito
		if (pLockEntry->m_strLockKey == strLockKey)
		{
			m_SqlLocksEntries.RemoveAt(i);
			break;
		}
	}
	return true;
}

//----------------------------------------------------------------------------	
bool TableLockEntry::RemoveEntriesForContext(const CString& strcontext)
{
	LockEntry* pLockEntry = NULL;

	for (int i = m_SqlLocksEntries.GetUpperBound(); i >= 0; i--)
	{
		pLockEntry = (LockEntry*)m_SqlLocksEntries[i];

		//se è già stato inserito
		if (pLockEntry->m_strContext == strcontext)
		{
			m_SqlLocksEntries.RemoveAt(i);
			break;
		}
	}
	return true;
}

/////////////////////////////////////////////////////////////////////////////
// 				class CacheLocksEntries Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------	
TableLockEntry* CacheLocksEntries::GetTableLockEntry(const CString& strTableName)
{
	if (strTableName.IsEmpty())
		return NULL;

	TableLockEntry* pTableLockEntry = NULL;
	for (int i = 0; i < GetSize(); i++)
	{
		pTableLockEntry = (TableLockEntry*)GetAt(i);
		if (pTableLockEntry->m_strTableName == strTableName)
			return pTableLockEntry;
	}

	return NULL;
}

//----------------------------------------------------------------------------	
bool CacheLocksEntries::ExistLockEntry(const CString& strTableName, const CString& strLockKey, const CString& strContext)
{
	TableLockEntry* tableLockEntry = GetTableLockEntry(strTableName);
	if (tableLockEntry != NULL)
	{
		LockEntry* lockEntry = tableLockEntry->GetLockByLockKey(strLockKey);
		return lockEntry != NULL && lockEntry->IsSameLock(strContext);
	}
	return FALSE;
}

//----------------------------------------------------------------------------	
LockEntry* CacheLocksEntries::GetLockEntry(const CString& strTableName, const CString& strLockKey)
{
	TableLockEntry* tableLockEntry = GetTableLockEntry(strTableName);
	return (tableLockEntry != NULL) ? tableLockEntry->GetLockByLockKey(strLockKey) : NULL;
}

//----------------------------------------------------------------------------	
void CacheLocksEntries::AddLockEntry(const CString& strTableName, const CString& strLockKey, const CString& strContext)
{
	TableLockEntry* tableLockEntry = GetTableLockEntry(strTableName);
	if (tableLockEntry == NULL)
	{
		tableLockEntry = new TableLockEntry(strTableName);
		Add(tableLockEntry);
	}
	tableLockEntry->AddLockEntry(new LockEntry(strLockKey, strContext));

}

//----------------------------------------------------------------------------	
void CacheLocksEntries::RemoveLockEntry(const CString& strTableName, const CString& strLockKey)
{
	TableLockEntry* tableLockEntry = GetTableLockEntry(strTableName);
	if (tableLockEntry != NULL)
		tableLockEntry->RemoveLockEntry(strLockKey);
}

//----------------------------------------------------------------------------	
void CacheLocksEntries::RemoveEntriesForContext(const CString& strContext)
{
	TableLockEntry* tableLockEntry = NULL;

	for (int i = 0; i < GetSize(); i++)
		((TableLockEntry*)GetAt(i))->RemoveEntriesForContext(strContext);
}

//----------------------------------------------------------------------------	
void CacheLocksEntries::RemoveEntriesForContext(const CString& strTableName, const CString& strContext)
{
	TableLockEntry* tableLockEntry = GetTableLockEntry(strTableName);
	if (tableLockEntry != NULL)
		tableLockEntry->RemoveEntriesForContext(strContext);
}

