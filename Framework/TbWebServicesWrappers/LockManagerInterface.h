#pragma once



#include "beginh.dex"

class CFunctionDescription;
class CLoginManagerInterface;
class SqlLockManager; //classe di lock managed
class MSqlConnection;
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
	SqlLockManager * m_pTBLockManager;

public:
	CLockManagerInterface(
		const CString& strService,
		const CString& strServiceNamespace,
		const CString& strServer,
		int nWebServicesPort
		);	
	~CLockManagerInterface();
	
public:
	BOOL	Init(const CString& strDBName);
	BOOL	Init(MSqlConnection* pMSqlConnection);
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