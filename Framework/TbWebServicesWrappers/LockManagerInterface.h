#pragma once

#include "beginh.dex"

class CFunctionDescription;
class CLoginManagerInterface;
class SqlLockManager; //classe di lock managed
class SqlSession;

//----------------------------------------------------------------------------
class TB_EXPORT CLockManagerObject
{
public:
	virtual ~CLockManagerObject() {}

public:
	virtual BOOL Init(const CString& strUserName, const CString& strProcessName, const CString& strAuthenticationToken, const CString& strProcessGuid) = 0;
	virtual BOOL LockCurrent(const CString& strTableName, const CString& strLockKey, const CString& strContext, CString& lockerUser, CString& lockerApp, DataDate& lockerDate) = 0;
	virtual BOOL UnlockCurrent(const CString& strTableName, const CString& strLockKey, const CString& strContext) = 0;
	virtual BOOL IsCurrentLocked(const CString& strTableName, const CString& strLockKey, const CString& strContext) = 0;
	virtual BOOL IsMyLock(const CString& strTableName, const CString& strLockKey, const CString& strContext) = 0;
	virtual BOOL UnlockAllForCurrentConnection() = 0;
	virtual BOOL UnlockAllContext(const CString& strContext) = 0;
	virtual BOOL UnlockAllTableContext(const CString& strTableName, const CString& strContext) = 0;
	virtual BOOL GetLockInfo(const CString& strLockKey, const CString& strTableName, CString& lockerUser, CString& processName, DataDate& lockerDate) = 0;
};


//----------------------------------------------------------------------------
class TB_EXPORT CLockManagerInterface : public CObject
{
private:
	const CString		m_strService;				// nome del WEB service (se esterno)
	const CString		m_strServiceNamespace;		// namespace del WEB service (se esterno)
	const CString		m_strServer;				// nome del server del WEB service (se esterno)
	const int			m_nWebServicesPort;			// numero di porta di IIS
	CString				m_sLockSessionID;
	
private:
	CLockManagerObject * m_pTBLockManagerObj;

public:
	CLockManagerInterface(
		const CString& strService,
		const CString& strServiceNamespace,
		const CString& strServer,
		int nWebServicesPort
		);	

	CLockManagerInterface(CLockManagerObject*);
	~CLockManagerInterface();

	
public:
	BOOL	Init(const CString& strDBName);
	BOOL	Init();
	BOOL	LockCurrent(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress, CString& strLockMsg, CString strLockKeyDescription = _T(""));
	BOOL	UnlockCurrent(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress);
	BOOL	IsCurrentLocked(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress);
	BOOL	IsMyLock(const CString& strCompanyDBName, const CString& strTableName, const CString& strLockKey, const CString& strAddress);
	BOOL	UnlockAllForCurrentConnection(const CString& strCompanyDBName);
	BOOL	UnlockAllContext(const CString& strCompanyDBName, const CString& strAddress);
	BOOL	UnlockAll(const CString& strCompanyDBName, const CString& strAddress, const CString& strTableName);
	BOOL	GetLockInfo(const CString& strCompanyDBName, const CString& strLockKey, const CString& strTableName, DataStr& lockerUser, DataDate& lockTime, DataStr& processName);
	BOOL	HasRestarted();
	void	InitializeLockSessionID();

	BOOL	LockDocument(const CString& strCompanyDBName, const CString& strDocumentNamespace, const CString& strAddress, CString& strLockMsg);
	BOOL	UnlockDocument(const CString& strCompanyDBName, const CString& strDocumentNamespace, const CString& strAddress);

private:
	void			InitFunction		(CFunctionDescription& aFunctionDescription) const;
	const CString	GetLockSessionID	() const;
};

#include "endh.dex"