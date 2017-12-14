#pragma once

#include "beginh.dex"

class CBaseContext;

// CachedRecord: a record can be shared by more that a query to optimize
// update\delete management on the table
//============================================================================
class CCachedRecord : public CObject
{
	friend class CCachingParamElem;
	friend class CCachedTable;

private:
	SqlRecord*	m_pCachedRecord;
	long		m_lRef;

public:
	CCachedRecord	(const SqlRecord* pRecord);
	~CCachedRecord	();

private:
	SqlRecord*	GetRecord		() const	{ return m_pCachedRecord; }
	void		AddReference	()			{ if (m_pCachedRecord) m_lRef++; }
	void		RemoveReference	()			{ if (m_pCachedRecord) m_lRef--; }
	void		UpdateRecord	(const SqlRecord* pRecord);
};

// Cached record that is selected with a specific a param values into a cached query.
//============================================================================
class CCachingParamElem : public CObject
{
	friend class CCachingParamList;

private:
	CString			m_strParams;
	CCachedRecord*	m_pCachedRecord;

public:
	CCachingParamElem	(const CString& sParam, CCachedRecord* pCachedRecord);
	~CCachingParamElem	();

private:
	long		GetObjectSize	();
	SqlRecord*	GetRecord		() { return (m_pCachedRecord) ? m_pCachedRecord->GetRecord() : NULL; }

};

// Array of cached CCachingParamElem.
// Cache is an array until 100 elements; over 100 is switched into a map
//============================================================================
class CCachingParamList
{
	friend class CCachingQueryElem;

private:
	SqlRecord*		m_pCurrentRecord;
	CMapStringToOb* m_pCachingParamMap;
	CObArray*		m_pCachingParamArray;

public:
	CCachingParamList	();
	~CCachingParamList	();

private:
	// switch the cache array into the map
	BOOL	SwitchArrayToMap();

private: 
	BOOL	IsEmpty			() const { 
										return (!m_pCachingParamMap && !m_pCachingParamArray) ||
											(m_pCachingParamMap && m_pCachingParamMap->GetCount() == 0) ||
											(m_pCachingParamArray && m_pCachingParamArray->GetSize() == 0);
									 }
	long	GetObjectSize	();

	BOOL	FindRecord		(const CString& strParamList, SqlRecord* pRecord);
	BOOL	InsertRecord	(const CString& strParamList, CCachedRecord* pRecord);
	void	DeleteRecord	(CCachedRecord* pCachedRecord);

};

// Single Cached query
//============================================================================
class CCachingQueryElem : public CObject
{
	friend class CCachedTable;

private:
	CString				m_strQuery;
	CCachingParamList*	m_pCachingParams;

public:
	CCachingQueryElem	(const CString& strQuery);
	~CCachingQueryElem	();

private:
	BOOL	IsEmpty		() const { return !m_pCachingParams || m_pCachingParams->IsEmpty(); }

	long GetObjectSize	();

	BOOL	FindRecord		(const CString& strParamList, SqlRecord* pRecord);
	BOOL	InsertRecord	(const CString& strParamList, CCachedRecord* pCachedRecord);
	void	DeleteRecord	(CCachedRecord* pCachedRecord);
};

// Array of cached CCachingQueryElem.
// Cache is an array until 100 elements; over 100 is switched into a map
//============================================================================
class CCachedTable : public CObject
{
	friend class CCachedTableArray;

private:
	CString			m_strTableName;
	CMapStringToOb* m_pCachingQueryMap;
	CObArray*		m_pCachingQueryArray;

	CMapStringToOb* m_pCachingRecordMap;

	CCachingQueryElem* m_pCurrentQuery;

public:
	CCachedTable	(const CString& strTableName);
	~CCachedTable	();

private:
	BOOL CheckCurrentQuery	(const CString& strQuery);
	BOOL InsertQuery		(const CString& strQuery);
	BOOL FindQuery			(const CString& strQuery, BOOL bToRemove = FALSE);
	BOOL RemoveQuery		(const CString& strQuery);

	// switch the cache array into the map
	BOOL SwitchArrayToMap			();
	void RemoveSingleCachedRecord	(CCachedRecord* pCachedRecord);

	BOOL IsEmpty				() const { 
										return	(!m_pCachingQueryMap && !m_pCachingQueryArray) ||
												(m_pCachingQueryMap && m_pCachingQueryMap->GetCount() == 0) ||
												(m_pCachingQueryArray && m_pCachingQueryArray->GetSize() == 0);
									 }

	void Clear					();
	long GetObjectSize			();

	BOOL FindRecord				(const CString& strQuery, const CString& strParamList, SqlRecord* pRecord);
	BOOL InsertRecord			(const CString& strQuery, const CString& strParamList, SqlRecord* pRecord);
	BOOL UpdateRecord			(SqlRecord* pRecord);
	void DeleteRecord			(SqlRecord* pRecord);

	void RemoveQueries			();
	BOOL RemoveFirstQuery		();
	void RemoveAllCachedRecords ();

	CCachingQueryElem*	GetCurrentQuery()	const { return m_pCurrentQuery; }
};

// array of CCachedTable
//============================================================================
class CCachedTableArray : public Array
{
	friend class CDataCachingManager;
	friend class CDataCacheValidator;

private:
	CCachedTable* m_pCurrentCachedTable;

public:
	CCachedTableArray	();
	~CCachedTableArray	();

private:
	CCachedTable*	GetAt(int nIdx)	const { return (CCachedTable*)Array::GetAt (nIdx); }		

	void CheckCurrentTable	(const CString& strTableName);

	// Lookup & Update Operations
	CCachedTable* GetCachedTable	(const CString& strTableName);
	int			  GetCachedTableIdx	(const CString& strTableName);
	
	BOOL FindRecord			(const CString& strTableName, const CString& strQuery, const CString& strParamList, SqlRecord* pRecord);
	BOOL InsertRecord		(const CString& strQuery, const CString& strParamList, SqlRecord* pRecord);

	void RemoveTableQueries	(const CString& strTableName);
	void RemoveFirstQuery	();

	// Update and delete of the cached record
	BOOL UpdateRecord		(SqlRecord* pRecord);
	void DeleteRecord		(SqlRecord* pRecord);
	
	void Clear				();
	long GetObjectSize		();
};

// no lock primitives as it is managed by CDataCachingManager class
//============================================================================
class CDataCacheValidator
{
	friend class CDataCachingManager;

private:
	CCachedTableArray*	m_pDataCache;
	DWORD				m_wLastCheck;
	DWORD				m_wLastExpiration;

private:
	CDataCacheValidator ();
	~CDataCacheValidator();

private:
	void AttachCache	(CCachedTableArray* pDataCache);

	// automatic invalidation
	void CheckCacheStatus();

	// invalidation criteria
	BOOL IsCheckTime	();
	BOOL IsExpired		();
	BOOL IsTooLarge		();

	// actions
	void Expire			();
	void Resize			();
};

// Data Cache Manager that can be owned by:
// - the application
// - a single document
// - a single table
//============================================================================
class CDataCachingManager : public CObject, CTBLockable
{
	friend class SqlTable;
	friend class CLoginThread;
	friend class COleDbManager;
	friend class CDataCachingContext;
	friend class CDataCachingUpdatesListener;

private:
	CCachedTableArray*		m_pCache;
	CDataCacheValidator		m_Validator;
	CBaseContext*			m_pContext;

	CDataCachingManager	();
	~CDataCachingManager();

	// Cleaning Operation
	void ClearCache		();
	
	// Lookup & Update Operations
	BOOL FindRecord		(const CString& strTableName, const CString& strQuery, const CString& strParamList, SqlRecord* pRecord);
	BOOL InsertRecord	(const CString& strQuery, const CString& strParamList, SqlRecord* pRecord);
	BOOL UpdateRecord	(SqlRecord* pRecord);
	void DeleteRecord	(SqlRecord* pRecord);

public:
	//for lock
	virtual LPCSTR	GetObjectName() const { return "CDataCachingManager"; }
};

// Settings related to Data Caching. No LOCK privitives as they are newed and
// loaded by loginthread and never modified
//============================================================================
class TB_EXPORT CDataCachingSettings : public CObject, CTBLockable
{
	DECLARE_DYNAMIC(CDataCachingSettings)

public:
	enum CacheScope { LOGIN, DOCUMENT_CONTEXT, DOCUMENT_TRANSACTION, TABLE};

private:
	BOOL		m_bDataCachingEnabled;
	CacheScope	m_CacheScope;
	long		m_lCheckMilliSeconds;
	long		m_lExpirationMilliSeconds;
	long		m_lMaxBytesSize;
	double		m_nReductionPerc;

public:
	CDataCachingSettings ();

public:
	const BOOL&			IsDataCachingEnabled		() const;
	const CacheScope&	GetCacheScope				() const;
	const BOOL			IsDocumentScope				() const;
	const long&			GetCheckMilliSeconds		() const;
	const long&			GetExpirationMilliSeconds	() const;
	const long&			GetMaxBytesSize				() const;
	const double&		GetReductionPerc			() const;

private:
	void LoadSettings ();

public:
	//for lock
	virtual LPCSTR	GetObjectName() const { return "CDataCachingSettings"; }
};

//============================================================================
class TB_EXPORT CDataCachingUpdatesListener : public CObject, CTBLockable
{
	friend class CDataCachingContext;
	friend class SqlTable;

	DECLARE_DYNCREATE(CDataCachingUpdatesListener)

private:
	CObArray	m_pWorkingManagers;

public:
	CDataCachingUpdatesListener ();

private:
	void AddManager		(CDataCachingManager* pManager);
	void RemoveManager	(CDataCachingManager* pManager);

	BOOL UpdateRecord	(SqlRecord* pRecord);
	void DeleteRecord	(SqlRecord* pRecord);

public:
	//for lock
	virtual LPCSTR	GetObjectName() const { return "CDataCachingUpdatesListener"; }
};

//============================================================================
//						Global Objects
//============================================================================

TB_EXPORT CDataCachingUpdatesListener* AFXAPI	AfxGetDataCachingUpdatesListener();
TB_EXPORT const CDataCachingSettings* AFXAPI	AfxGetDataCachingSettings		();

#include "endh.dex"
