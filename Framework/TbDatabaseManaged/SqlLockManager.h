#pragma once

#include "beginh.dex"
class MSqlConnection;

class CacheLocksEntries;
class LockEntry;



/// <summary>
/// LockEntry
/// </summary>
//=========================================================================
class LockEntry : public CObject
{
public:
	CString		m_strLockKey; // Chiave primaria composta del record del database
	CString		m_strContext; // Indirizzo in memoria dell'oggetto (il contesto) che ha eseguito il lock
							  //DateTime	LockDate; // Data ed ora in cui si è eseguito il lock
	CString		m_strLockUser; // Data ed ora in cui si è eseguito il lock
	CString		m_strLockApp; // Data ed ora in cui si è eseguito il lock

public:
	LockEntry(const CString& strLockKey, const CString& strContext);//, DateTime lockTime);

public:
	bool IsSameLock(LockEntry* pSqlLockEntry);
	bool IsSameLock(const CString& strContext);
};

/// <summary>
/// LockEntry
/// </summary>
//=========================================================================
class TableLockEntry : public CObject
{
public:
	CString m_strTableName; // Nome della tabella
	::Array m_SqlLocksEntries; // Nome della tabella

public:
	TableLockEntry(const CString& strTableName);
public:
	LockEntry * GetLockByLockKey(const CString& strLockKey);
	void AddLockEntry(LockEntry* pNewSqlLockEntry);
	bool RemoveLockEntry(const CString& strLockKey);
	bool RemoveEntriesForContext(const CString& strContext);
};

/// <summary>
/// CacheLocksEntries
/// </summary>
//=========================================================================
class CacheLocksEntries : public ::Array
{
public:
	TableLockEntry * GetTableLockEntry(const CString& strtableName);
	bool ExistLockEntry(const CString& strTableName, const CString& strLockKey, const CString& strContext);
	LockEntry* GetLockEntry(const CString& strTableName, const CString& strLockKey);
	void AddLockEntry(const CString& strTableName, const CString& strLockKey, const CString& strContext);//, DateTime lockTime);
	void RemoveLockEntry(const CString& strTableName, const CString& strLockKey);
	void RemoveEntriesForContext(const CString& strContext);
	void RemoveEntriesForContext(const CString& strTableName, const CString& strContext);
};

/// <summary>
/// SqlLockManager
/// </summary>
//===================================================================================
class TB_EXPORT SqlLockManager
{
private:
	CString						m_strAuthenticationToken;
	CString						m_strProcessGuid;
	CString						m_strAccountName;
	CString						m_strCompanyConnectionString;
	CString						m_strProcessName;
	bool						m_bOwnSqlConnection;
	MSqlConnection*				m_pSqlConnection;
	CString						m_strErrorMessage;

	//gestione cache
	bool						m_bEnableLockCache;
	CacheLocksEntries*			m_pCacheLocksEntries;

	int							m_OldConnState;

public:
	SqlLockManager();
	~SqlLockManager();

public:
	/// <summary>
	/// Gets the name for the object
	/// </summary>
	const CString&		GetErrorMessage()  const { return m_strErrorMessage; }
	void				EnableLockCache(bool bEnable) { m_bEnableLockCache = bEnable; }	

private:
	void OpenConnection();
	void CloseConnection();

public:
	BOOL	Init(MSqlConnection* pSqlConnection, const CString& strUserName, const CString& strProcessName, const CString& strAuthenticationToken, const CString& strProcessGuid);
	BOOL	Init(const CString& strConnectionString, const CString& strUserName, const CString& strProcessName, const CString& strAuthenticationToken, const CString& strProcessGuid);
	BOOL	LockCurrent(const CString& strTableName, const CString& strLockKey, const CString& strContext, CString& lockerUser, CString& lockerApp, DataDate& lockerDate);
	BOOL	UnlockCurrent(const CString& strTableName, const CString& strLockKey, const CString& strContext);
	BOOL	IsCurrentLocked(const CString& strTableName, const CString& strLockKey, const CString& strContext);
	BOOL	IsMyLock(const CString& strTableName, const CString& strLockKey, const CString& strContext);
	BOOL	UnlockAllForCurrentConnection();
	BOOL	UnlockAllContext(const CString& strContext);
	BOOL	UnlockAllTableContext(const CString& strTableName, const CString& strContext);
	BOOL	GetLockInfo(const CString& strLockKey, const CString& strTableName, CString& lockerUser, CString& processName, DataDate& lockerDate);
};


#include "endh.dex"