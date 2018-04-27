#include "StdAfx.h"

#include <TbDatabaseManaged\MSqlConnection.h>
#include "SqlConnect.h"
#include "SqlRec.h"
#include "SqlTable.h"
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
bool TableLockEntry::RemoveEntriesForContext(const CString& strContext)
{
	LockEntry* pLockEntry = NULL;

	for (int i = m_SqlLocksEntries.GetUpperBound(); i >= 0; i--)
	{
		pLockEntry = (LockEntry*)m_SqlLocksEntries[i];
		//se è già stato inserito
		if (pLockEntry->m_strContext == strContext)
			m_SqlLocksEntries.RemoveAt(i);
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



///////////////////////////////////////////////////////////////////////////////
//						SP_LockCurrent
//////////////////////////////////////////////////////////////////////////////
//
class SP_LockCurrent : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SP_LockCurrent)

public:
	DataStr		f_In_TableName;
	DataStr		f_In_LockKey;
	DataStr		f_In_AuthenticationToken;
	DataStr		f_In_AccountName;
	DataStr		f_In_Context;
	DataStr		f_In_ProcessName;
	DataStr		f_In_ProcessGuid;

	DataInt		f_ReturnValue;
	DataStr		f_Out_LockerAccount;
	DataStr		f_Out_LockerProcess;
	DataDate	f_Out_LockerDate;

public:
	SP_LockCurrent() : SqlRecordProcedure(GetStaticName()) { BindRecord(); }

	//-----------------------------------------------------------------------------
	void BindRecord()
	{
		BEGIN_BIND_PARAM_DATA()
			BIND_PARAM(_T("@TableName"), f_In_TableName);
			BIND_PARAM(_T("@LockKey"), f_In_LockKey);
			BIND_PARAM(_T("@AuthenticationToken"), f_In_AuthenticationToken);
			BIND_PARAM(_T("@AccountName"), f_In_AccountName);
			BIND_PARAM(_T("@Context"), f_In_Context);
			BIND_PARAM(_T("@ProcessName"), f_In_ProcessName);
			BIND_PARAM(_T("@ProcessGuid"), f_In_ProcessGuid);
			BIND_PARAM(RETURN_VALUE, f_ReturnValue);
			
			BIND_PARAM(_T("@LockerAccount"), f_Out_LockerAccount);
			BIND_PARAM(_T("@LockerProcess"), f_Out_LockerProcess);
			BIND_PARAM(_T("@LockerDate"), f_Out_LockerDate);
		END_BIND_PARAM_DATA()
	}

	//-----------------------------------------------------------------------------
	LPCTSTR GetStaticName() { return _T("sp_lockcurrent"); }
};

IMPLEMENT_DYNCREATE(SP_LockCurrent, SqlRecordProcedure)


///////////////////////////////////////////////////////////////////////////////
//						SP_IsCurrentLocked
//////////////////////////////////////////////////////////////////////////////
//
class SP_IsCurrentLocked : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SP_IsCurrentLocked)

public:
	DataStr		f_In_TableName;
	DataStr		f_In_LockKey;
	DataStr		f_In_Context;

	DataInt		f_ReturnValue;

public:
	SP_IsCurrentLocked() : SqlRecordProcedure(GetStaticName()) { BindRecord(); }
	
	//-----------------------------------------------------------------------------
	void SP_IsCurrentLocked::BindRecord()
	{
		BEGIN_BIND_PARAM_DATA()
			BIND_PARAM(_T("@TableName"), f_In_TableName);
			BIND_PARAM(_T("@LockKey"), f_In_LockKey);
			BIND_PARAM(_T("@Context"), f_In_Context);
			BIND_PARAM(RETURN_VALUE, f_ReturnValue);
		END_BIND_PARAM_DATA()
	}

	//-----------------------------------------------------------------------------
	LPCTSTR GetStaticName() { return _T("sp_iscurrentlocked"); }
};

IMPLEMENT_DYNCREATE(SP_IsCurrentLocked, SqlRecordProcedure)


///////////////////////////////////////////////////////////////////////////////
//						SP_IsMyLock
//////////////////////////////////////////////////////////////////////////////
//
class SP_IsMyLock : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SP_IsMyLock)

public:
	DataStr		f_In_TableName;
	DataStr		f_In_LockKey;
	DataStr		f_In_AuthenticationToken;
	DataStr		f_In_Context;

	DataInt		f_ReturnValue;

public:
	SP_IsMyLock() : SqlRecordProcedure(GetStaticName()) { BindRecord(); }
	
	//-----------------------------------------------------------------------------
	void BindRecord()
	{
		BEGIN_BIND_PARAM_DATA()
			BIND_PARAM(_T("@TableName"), f_In_TableName);
			BIND_PARAM(_T("@LockKey"), f_In_LockKey);
			BIND_PARAM(_T("@AuthenticationToken"), f_In_AuthenticationToken);
			BIND_PARAM(_T("@Context"), f_In_Context);
			BIND_PARAM(RETURN_VALUE, f_ReturnValue);
		END_BIND_PARAM_DATA()
	}

	//-----------------------------------------------------------------------------
	LPCTSTR GetStaticName() { return _T("sp_ismylock"); }
};

IMPLEMENT_DYNCREATE(SP_IsMyLock, SqlRecordProcedure)

///////////////////////////////////////////////////////////////////////////////
//						SP_GetLockInfo
//////////////////////////////////////////////////////////////////////////////
//
class SP_GetLockInfo : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SP_GetLockInfo)

public:
	DataStr		f_In_TableName;
	DataStr		f_In_LockKey;

	DataInt		f_ReturnValue;
	DataStr		f_Out_LockerAccount;
	DataStr		f_Out_LockerProcess;
	DataDate	f_Out_LockerDate;

public:
	SP_GetLockInfo() : SqlRecordProcedure(GetStaticName())	{ BindRecord(); }

	//-----------------------------------------------------------------------------
	void BindRecord()
	{
		BEGIN_BIND_PARAM_DATA()
			BIND_PARAM(_T("@TableName"), f_In_TableName);
			BIND_PARAM(_T("@LockKey"), f_In_LockKey);
			BIND_PARAM(RETURN_VALUE, f_ReturnValue);
			BIND_PARAM(_T("@LockerAccount"), f_Out_LockerAccount);
			BIND_PARAM(_T("@LockerProcess"), f_Out_LockerProcess);
			BIND_PARAM(_T("@LockerDate"), f_Out_LockerDate);
		END_BIND_PARAM_DATA()
	}

	//-----------------------------------------------------------------------------
	LPCTSTR GetStaticName() { return _T("sp_getlockinfo"); }
};

IMPLEMENT_DYNCREATE(SP_GetLockInfo, SqlRecordProcedure)



///////////////////////////////////////////////////////////////////////////////
//						SP_UnlockCurrent
//////////////////////////////////////////////////////////////////////////////
//
class SP_UnlockCurrent : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SP_UnlockCurrent)

public:
	DataStr		f_In_TableName;
	DataStr		f_In_LockKey;
	DataStr		f_In_AuthenticationToken;
	DataStr		f_In_Context;

	DataInt		f_ReturnValue;

public:
	SP_UnlockCurrent() : SqlRecordProcedure(GetStaticName()) { BindRecord(); }
		
	//-----------------------------------------------------------------------------
	void BindRecord()
	{
		BEGIN_BIND_PARAM_DATA()
			BIND_PARAM(_T("@TableName"), f_In_TableName);
			BIND_PARAM(_T("@LockKey"), f_In_LockKey);
			BIND_PARAM(_T("@AuthenticationToken"), f_In_AuthenticationToken);
			BIND_PARAM(_T("@Context"), f_In_Context);
			BIND_PARAM(RETURN_VALUE, f_ReturnValue);
		END_BIND_PARAM_DATA()
	}

	//-----------------------------------------------------------------------------
	LPCTSTR GetStaticName() { return _T("sp_unlockcurrent"); }

};

IMPLEMENT_DYNCREATE(SP_UnlockCurrent, SqlRecordProcedure)


///////////////////////////////////////////////////////////////////////////////
//						SP_UnlockAllTableContext
//////////////////////////////////////////////////////////////////////////////
//
class SP_UnlockAllTableContext : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SP_UnlockAllTableContext)

public:
	DataStr		f_In_TableName;
	DataStr		f_In_AuthenticationToken;
	DataStr		f_In_Context;

	DataInt		f_ReturnValue;

public:
	SP_UnlockAllTableContext() : SqlRecordProcedure(GetStaticName()) { BindRecord(); }

	//-----------------------------------------------------------------------------
	void BindRecord()
	{
		BEGIN_BIND_PARAM_DATA()
			BIND_PARAM(_T("@TableName"), f_In_TableName);
			BIND_PARAM(_T("@AuthenticationToken"), f_In_AuthenticationToken);
			BIND_PARAM(_T("@Context"), f_In_Context);
			BIND_PARAM(RETURN_VALUE, f_ReturnValue);
		END_BIND_PARAM_DATA()
	}

	//-----------------------------------------------------------------------------
	LPCTSTR GetStaticName() { return _T("sp_unlockalltablecontext"); }
};

IMPLEMENT_DYNCREATE(SP_UnlockAllTableContext, SqlRecordProcedure)

///////////////////////////////////////////////////////////////////////////////
//						SP_UnlockAllContext
//////////////////////////////////////////////////////////////////////////////
//
class SP_UnlockAllContext : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SP_UnlockAllContext)

public:
	DataStr		f_In_AuthenticationToken;
	DataStr		f_In_Context;

	DataInt		f_ReturnValue;

public:
	SP_UnlockAllContext() : SqlRecordProcedure(GetStaticName()) { BindRecord(); }

	//-----------------------------------------------------------------------------
	void BindRecord()
	{
		BEGIN_BIND_PARAM_DATA()
			BIND_PARAM(_T("@AuthenticationToken"), f_In_AuthenticationToken);
			BIND_PARAM(_T("@Context"), f_In_Context);
			BIND_PARAM(RETURN_VALUE, f_ReturnValue);
		END_BIND_PARAM_DATA()
	}

	//-----------------------------------------------------------------------------
	LPCTSTR GetStaticName() { return _T("sp_unlockallcontext"); }
};

IMPLEMENT_DYNCREATE(SP_UnlockAllContext, SqlRecordProcedure)


///////////////////////////////////////////////////////////////////////////////
//						SP_UnlockAllToken
//////////////////////////////////////////////////////////////////////////////
//
class SP_UnlockAllToken : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SP_UnlockAllToken)

public:
	DataStr		f_In_AuthenticationToken;

	DataInt		f_ReturnValue;

public:
	SP_UnlockAllToken() : SqlRecordProcedure(GetStaticName()) { BindRecord(); }

	//-----------------------------------------------------------------------------
	void BindRecord()
	{
		BEGIN_BIND_PARAM_DATA()
			BIND_PARAM(_T("@AuthenticationToken"), f_In_AuthenticationToken);
			BIND_PARAM(RETURN_VALUE, f_ReturnValue);
		END_BIND_PARAM_DATA()
	}

	//-----------------------------------------------------------------------------
	LPCTSTR GetStaticName() { return _T("sp_unlockalltoken"); }
};

IMPLEMENT_DYNCREATE(SP_UnlockAllToken, SqlRecordProcedure)

/////////////////////////////////////////////////////////////////////////////
// 				class SqlLockManager Implementation
/////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
SqlLockManager::SqlLockManager(SqlSession* pSqlSession, bool bEnableLockCache)
	:
	m_pSqlSession(pSqlSession),
	m_bEnableLockCache(bEnableLockCache)
{
}

//----------------------------------------------------------------------------------
SqlLockManager::~SqlLockManager()
{
	if (m_pCacheLocksEntries)
		delete m_pCacheLocksEntries;
}

//----------------------------------------------------------------------------------
BOOL SqlLockManager::Init(const CString& strUserName, const CString& strProcessName, const CString& strAuthenticationToken, const CString& strProcessGuid)
{
	m_strAccountName = strUserName;
	m_strProcessName = strProcessName;
	m_strAuthenticationToken = strAuthenticationToken;
	m_strProcessGuid = strProcessGuid;

	if (m_bEnableLockCache)
		m_pCacheLocksEntries = new CacheLocksEntries();

	return TRUE;
}

/// <summary>
/// Prenota un dato
/// </summary>
/// <param name="tableName">nome tabella</param>
/// <param name="lockKey">chiave primaria del dato da prenotare</param>
/// <param name="context">indirizzo in memoria del documento</param>
/// <param name="lockUser">in caso di record già in stato di lock restituisce l'account che impegna il dato</param>
/// <param name="lockApp">in caso di record già in stato di lock restituisce l'applicazione che impegna il dato</param>
/// <returns>true se il dato è stato prenotato con successo, false altrimenti</returns>
//----------------------------------------------------------------------------------
BOOL SqlLockManager::LockCurrent(const CString& strTableName, const CString& strLockKey, const CString& strContext, CString& lockerUser, CString& lockerApp, DataDate& lockerDate)
{
	BOOL bResult = TRUE;
	//per prima cosa verifico che non sia un lock già presente nella cache 
	LockEntry* lockEntry = (m_pCacheLocksEntries) ? m_pCacheLocksEntries->GetLockEntry(strTableName, strLockKey) : NULL;
	if (lockEntry != NULL)
	{
		if (!(bResult = lockEntry->IsSameLock(strContext)))
		{
			lockerUser = CString(lockEntry->m_strLockKey);
			lockerApp = CString(lockEntry->m_strLockApp);
		}
		return bResult;
	}

	SP_LockCurrent aSpLockCurr;
	aSpLockCurr.f_In_TableName = strTableName;
	aSpLockCurr.f_In_LockKey = strLockKey;
	aSpLockCurr.f_In_AuthenticationToken = m_strAuthenticationToken;
	aSpLockCurr.f_In_AccountName = m_strAccountName;
	aSpLockCurr.f_In_Context = strContext;
	aSpLockCurr.f_In_ProcessName = m_strProcessName;
	aSpLockCurr.f_In_ProcessGuid = m_strProcessGuid;

	SqlTable aTable(&aSpLockCurr, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.Call();
		bResult = (aSpLockCurr.f_ReturnValue == 1);
		if (!bResult)
		{
			lockerUser = aSpLockCurr.f_Out_LockerAccount;
			lockerApp = aSpLockCurr.f_Out_LockerProcess;
			lockerDate = aSpLockCurr.f_Out_LockerDate;
		}
		else
			//ho effettuato il lock, ne faccio il cache
			if (m_pCacheLocksEntries)
				m_pCacheLocksEntries->AddLockEntry(strTableName, strLockKey, strContext);
		aTable.Close();
	}
	CATCH (SqlException, e)
	{
		m_strErrorMessage = e->m_strError;
		if (aTable.IsOpen())
			aTable.Close();
	}	
	END_CATCH
	
	return bResult;
}

/// <summary>
/// Verifica se un dato è stato prenotato da un altro contesto/utente
/// </summary>
/// <param name="companyDBName">Nome del database aziendale</param>
/// <param name="tableName">Nome della tabella</param>
/// <param name="lockKey">Chiave primaria del lock</param>
/// <param name="context">Indirizzo in memoria del contesto che richiede se il dato è in stato di locked</param>
/// <returns>true il dato è stato prenotato da un altro contesto false se non è stato prenotato oppure è prenotato dallo stesso contesto</returns>
//----------------------------------------------------------------------------------
BOOL SqlLockManager::IsCurrentLocked(const CString& strTableName, const CString& strLockKey, const CString& strContext)
{
	//per prima cosa verifico che non sia un lock già presente nella cache con lo stesso indirizzo o meno
	LockEntry* lockEntry = (m_pCacheLocksEntries) ? m_pCacheLocksEntries->GetLockEntry(strTableName, strLockKey) : NULL;
	if (lockEntry != NULL)
		return !lockEntry->IsSameLock(strContext); //vuol dire che è locked da un altro contesto

	BOOL bResult = FALSE;
	SP_IsCurrentLocked aSpIsCurrentLocked;

	aSpIsCurrentLocked.f_In_TableName = strTableName;
	aSpIsCurrentLocked.f_In_LockKey = strLockKey;
	aSpIsCurrentLocked.f_In_Context = strContext;
	SqlTable aTable(&aSpIsCurrentLocked, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.Call();
		bResult = aSpIsCurrentLocked.f_ReturnValue == 1;
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		m_strErrorMessage = e->m_strError;
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH

	return bResult;
}

/// <summary>
/// Verifica se il record passato come chiave è stato lockato dal contesto stesso individuato da context
/// E' l'opposto dell'IsCurrentLocked
/// </summary>
/// <param name="tableName">Nome della tabella</param>
/// <param name="lockKey">Chiave primaria del lock</param>
/// <param name="context">Indirizzo in memoria del contesto che richiede se il dato è in stato di locked</param>
/// <returns>true il dato è stato prenotato dallo stesso contesto false se non è stato prenotato oppure è prenotato da altro contesto</returns>
//----------------------------------------------------------------------------------
BOOL SqlLockManager::IsMyLock(const CString& strTableName, const CString& strLockKey, const CString& strContext)
{
	//Se il mio è per forza nella cache
	if (m_pCacheLocksEntries)
		return m_pCacheLocksEntries->ExistLockEntry(strTableName, strLockKey, strContext);

	BOOL bResult = FALSE;
	SP_IsMyLock aSpIsMyLock;
	aSpIsMyLock.f_In_TableName = strTableName;
	aSpIsMyLock.f_In_LockKey = strLockKey;
	aSpIsMyLock.f_In_AuthenticationToken = m_strAuthenticationToken;
	aSpIsMyLock.f_In_Context = strContext;
	
	SqlTable aTable(&aSpIsMyLock, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.Call(); 	
		bResult = (aSpIsMyLock.f_ReturnValue == 1);
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		m_strErrorMessage = e->m_strError;
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH

	return bResult;
}

/// <summary>
/// Prende informazioni su un lock
/// </summary>
/// <param name="lockKey">Chiave primaria del lock</param>
/// <param name="tableName">Nome della tabella</param>
/// <param name="lockUser">in caso di record già in stato di lock restituisce l'account che impegna il dato</param>
/// <param name="lockTime">Istante di prenotazione del dato</param>
/// <param name="processName">Nome del processo che ha prenotato il dato</param>
/// <returns>true se la funzione ha avuto successo</returns>
//-----------------------------------------------------------------------
BOOL SqlLockManager::GetLockInfo(const CString& strLockKey, const CString& strTableName, CString& lockerUser, CString& lockerApp, DataDate& lockerDate)
{
	BOOL bResult = FALSE;

	//prima guardo nella cache
	LockEntry* lockEntry = (m_pCacheLocksEntries) ? m_pCacheLocksEntries->GetLockEntry(strTableName, strLockKey) : NULL;
	if (lockEntry != NULL)
	{
		lockerUser = lockEntry->m_strLockKey;
		lockerApp = lockEntry->m_strLockApp;
		delete lockEntry;
	}

	SP_GetLockInfo aSpGetLockInfo;
	aSpGetLockInfo.f_In_TableName = strTableName;
	aSpGetLockInfo.f_In_LockKey = strLockKey;
	SqlTable aTable(&aSpGetLockInfo, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.Call();
		bResult = (aSpGetLockInfo.f_ReturnValue == 1);
		if (bResult)
		{
			lockerUser = aSpGetLockInfo.f_Out_LockerAccount;
			lockerApp = aSpGetLockInfo.f_Out_LockerProcess;
			lockerDate = aSpGetLockInfo.f_Out_LockerDate;
		}
		aTable.Close();
	}
	CATCH(SqlException, e)
	{
		m_strErrorMessage = e->m_strError;
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH

	return bResult;
}


/// <summary>
/// Rimuove la prenotazione di un record
/// </summary>
/// <param name="tableName">Nome della tabella</param>
/// <param name="lockKey">Chiave primaria del lock</param>
/// <param name="context">Indirizzo in memoria del documento</param>
/// <returns>true se la funzione ha avuto successo</returns>
//----------------------------------------------------------------------------------		
BOOL SqlLockManager::UnlockCurrent(const CString& strTableName, const CString& strLockKey, const CString& strContext)
{
	BOOL bResult = FALSE;
	SP_UnlockCurrent aSpUnlockRec;
	aSpUnlockRec.f_In_TableName = strTableName;
	aSpUnlockRec.f_In_LockKey = strLockKey;
	aSpUnlockRec.f_In_AuthenticationToken = m_strAuthenticationToken;
	aSpUnlockRec.f_In_Context = strContext;

	SqlTable aTable(&aSpUnlockRec, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.Call();
		bResult = (aSpUnlockRec.f_ReturnValue > 0);
		aTable.Close();
		//poi rimuovo la cache
		if (bResult && m_pCacheLocksEntries)
			m_pCacheLocksEntries->RemoveLockEntry(strTableName, strLockKey);		
	}
	CATCH(SqlException, e)
	{
		m_strErrorMessage = e->m_strError;
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH

		return bResult;
}


/// <summary>
/// Rimuove tutti i lock su una tabella per un determinato contesto
/// </summary>
//----------------------------------------------------------------------------------		
BOOL SqlLockManager::UnlockAllTableContext(const CString& strTableName, const CString& strContext)
{
	BOOL bResult = FALSE;

	SP_UnlockAllTableContext aSpUnlock;
	aSpUnlock.f_In_TableName = strTableName;
	aSpUnlock.f_In_AuthenticationToken = m_strAuthenticationToken;
	aSpUnlock.f_In_Context = strContext;

	SqlTable aTable(&aSpUnlock, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.Call();
		bResult = (aSpUnlock.f_ReturnValue > 0);
		aTable.Close();
		//poi rimuovo la cache
		if (bResult && m_pCacheLocksEntries)
			m_pCacheLocksEntries->RemoveEntriesForContext(strTableName, strContext);
	}
	CATCH(SqlException, e)
	{
		m_strErrorMessage = e->m_strError;
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH

	return bResult;
}

/// <summary>
/// Rimuove tutti i lock di un determinato contesto
/// </summary>
//----------------------------------------------------------------------------------		
BOOL SqlLockManager::UnlockAllContext(const CString& strContext)
{
	BOOL bResult = FALSE;

	SP_UnlockAllContext aSpUnlock;
	aSpUnlock.f_In_AuthenticationToken = m_strAuthenticationToken;
	aSpUnlock.f_In_Context = strContext;

	SqlTable aTable(&aSpUnlock, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.Call();
		bResult = (aSpUnlock.f_ReturnValue > 0);
		aTable.Close();
		//poi rimuovo la cache
		if (bResult && m_pCacheLocksEntries)
			m_pCacheLocksEntries->RemoveEntriesForContext(strContext);
	}
	CATCH(SqlException, e)
	{
		m_strErrorMessage = e->m_strError;
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH

	return bResult;
}

/// <summary>
/// Rimuove tutti i lock per l'account corrente
/// </summary>
//----------------------------------------------------------------------------------		
BOOL SqlLockManager::UnlockAllForCurrentConnection()
{
	BOOL bResult = FALSE;
	
	SP_UnlockAllToken aSpUnlock;
	aSpUnlock.f_In_AuthenticationToken = m_strAuthenticationToken;

	SqlTable aTable(&aSpUnlock, m_pSqlSession);
	TRY
	{
		aTable.Open();
		aTable.Call();
		bResult = (aSpUnlock.f_ReturnValue > 0);
		aTable.Close();
		//poi rimuovo la cache
		if (bResult && m_pCacheLocksEntries)
			m_pCacheLocksEntries->RemoveAll();
	}
	CATCH(SqlException, e)
	{
		m_strErrorMessage = e->m_strError;
		if (aTable.IsOpen())
			aTable.Close();
	}
	END_CATCH

	return bResult;
}