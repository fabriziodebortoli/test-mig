
#pragma once


//includere alla fine degli include del .H
#include "beginh.dex"

class SqlTable;
class CBaseContext;
class SqlConnection;
class CLockManagerInterface;
class CLoginManagerInterface;

//------------------------------------------------------------------------------------------------------------
class TB_EXPORT LockRetriesMng : public RetryLockedResource
{
public:
	BOOL	m_bUseMessageBox;
	BOOL	m_bUseCustomSettings;

private:
	CBaseContext*	m_pContext; // lo effettuo su un CBaseContext che ha eventualmente 
								// il puntatore del documento che ha istanzione il contesto 

public:
	LockRetriesMng (BOOL bUseMessageBox = FALSE, BOOL bUseCustomSettings = FALSE);

public:
	BOOL AttachContext (CBaseContext* pContext);

public:
	virtual	void EnableDocumentFrame (BOOL bEnable);
};

/////////////////////////////////////////////////////////////////////////////
//						class SqlLockMng
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
class TB_EXPORT SqlLockMng
{
private:
	CString								m_strDBName;
	CLockManagerInterface*				m_pLockManagerInterface;
	CLoginManagerInterface*				m_pLoginManagerInterface;
	CString								m_strCurrentKey;
	CString								m_strTableName;
	SqlTable*							m_pCurrentTable;
	CString								m_strLockMsg;
	BOOL								m_bLocksCacheEnabled;
	CMapStringToOb						m_LocksCache;

public:
	SqlLockMng(const CString &strDBName);
	~SqlLockMng();

private:
	BOOL	GetLockEntry	(SqlTable*);
	
public:
	BOOL	LockCurrent		(SqlTable* pTable, BOOL bUseMessageBox = TRUE, LockRetriesMng* pRetriesMng = NULL);
	BOOL	UnlockCurrent	(SqlTable* pTable);
	BOOL	UnlockCurrent	(const CString& sLockContextKey, SqlTable* pTable);
	BOOL	IsCurrentLocked	(SqlTable* pTable);
	BOOL	IsMyLock		(SqlTable* pTable);

	BOOL	LockDocument	(CBaseContext* pBaseContext); 
	BOOL	UnlockDocument	(CBaseContext* pBaseContext);

	BOOL	UnlockAll			(CBaseContext* pContext);
	BOOL	UnlockAll			(CBaseContext* pContext, LPCTSTR szTableName);
	BOOL	LockTableKey		(SqlTable* pTable, SqlRecord* pRec, LockRetriesMng* pRetriesMng = NULL);
	BOOL	UnlockTableKey		(SqlTable* pTable, SqlRecord* pRec);
	BOOL	UnlockTableKey		(const CString& sLockContextKey, SqlTable* pTable, SqlRecord* pRec);
	BOOL	LockTableKey		(const CString& sLockContextKey, SqlTable* pTable, SqlRecord* pRec, LockRetriesMng* pRetriesMng);
	BOOL	IsTableKeyLocked	(const CString& sLockContextKey, SqlTable* pTable, SqlRecord* pRec);
	CString GetLockMessage		(SqlTable* pTable);


	//table lock
	BOOL	LockTable	(SqlTable* pTable, const CString& tableName, BOOL bUseMessageBox = TRUE, LockRetriesMng* pRetriesMng = NULL);
	BOOL	UnlockTable	(SqlTable* pTable, const CString& tableName);


	// Keys Cache management
	void	EnableLocksCache		(const BOOL bValue = TRUE);
	void	ClearLocksCache			(const CString sLockContextKey = _T(""));
	BOOL	UnlockAllLockContextKeys(const CString& sLockContextKey);
	BOOL	LockAllLockContextKeys	(const CString& sLockContextKey, LockRetriesMng* pRetriesMng = NULL);

	BOOL	HasRestarted			() const;
	BOOL	RemoveAllLocks	(CBaseContext* pContext);

private:
	BOOL	LockCurrent				(const CString& sLockContextKey, SqlTable* pTable, LockRetriesMng* pRetriesMng);
	void	InsertInLocksCache		(const CString& sLockContextKey, const CString& sLockKey, CObject* pAddress);
	void	RemoveFromLocksCache	(const CString& sLockContextKey, const CString& sLockKey);
	BOOL	IsCachedTableKeyLocked	(const CString& sLockContextKey, SqlTable* pTable, SqlRecord* pRec);

	CString FormatAbortedLockLogMessage (const CString& sCurrentLockMessage);

	BOOL	ExecuteLock(LockRetriesMng* pRetriesMng);
	
public:
	static	void	SetKey			(const DataObj*, CString&);
	static	void	SetKey			(const SqlRecord*, CString&);

	static	CString InitLockKey				(SqlTable*);
	static	CString GetLockKeyDescription	(SqlTable*);
};

#include "endh.dex"

