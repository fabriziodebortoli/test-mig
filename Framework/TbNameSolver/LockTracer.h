#pragma once

#include "TBWinThread.h"

#include "beginh.dex"

class CTBLockable;
class CLockInfo;

class TB_EXPORT CLockRelation : public CObject
{
	DECLARE_SERIAL(CLockRelation);
public:
	CStringArray m_strParentNames;
	CArray<CStringArray*, CStringArray*> m_arFunctions;

	CLockRelation() {}
	~CLockRelation();

	BOOL IsEmpty() { return m_strParentNames.IsEmpty(); }
	BOOL Contains(const CString& strParentName, int &index) 
	{
		for (index = 0; index < m_strParentNames.GetCount(); index++)
			if (m_strParentNames.GetAt(index) == strParentName)
				return TRUE;
		return FALSE;
	}
	virtual void Serialize(CArchive& ar);
};

class TB_EXPORT CLockStructure : public CObject
{
	DECLARE_SERIAL(CLockStructure);
	CMap<CString, LPCTSTR, CLockRelation*, CLockRelation*> m_ObjectMap;

	virtual ~CLockStructure();
	void AddRelationShip(const CString& strObject, const CString &strParent, const CString& sFunction);
	BOOL CheckForDeadlock(const CString& strTestObject, const CString& strLatestStartObject, CStringArray& arObjects);

public:
	virtual void Serialize(CArchive& ar);
};


// CLockTracer
#define LOCK_BUFFER 10000
#define LOCK_BUFFER_MAX 9500

class CLockTracer : public CTBWinThread
{
	LONGLONG		m_nCounter;
	::CEvent		m_StartupEvent;
	BOOL			m_bValid;
	CMap<DWORD, DWORD, CStringArray*, CStringArray*> m_LockMap;
	CLockStructure	m_LockStructure;
	UINT			m_nPort;
	CStdioFile		m_LogFile;
	CString			m_sTraceFile;
	CString			m_sStructureFile;
	CLockInfo*		m_LockInfos[LOCK_BUFFER];
	volatile LONG	m_nWriteLockIndex;
	volatile LONG	m_nReadLockIndex;
	volatile bool	m_bWorking;
	::CSemaphore*	m_pDataAvailable;
	::CCriticalSection m_WriteSection;

	DECLARE_DYNCREATE(CLockTracer)
protected:
	CLockTracer();           // protected constructor used by dynamic creation
	virtual ~CLockTracer();

public:
	const CLockStructure* GetLockStructure() { return &m_LockStructure; }
	void SetPort(UINT nPort) { m_nPort = nPort; }
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	void StopAndDestroy();
	void WaitForStartup()	{ WaitForSingleObject(m_StartupEvent, INFINITE); }
	void WaitForEmptyQueue();
	BOOL IsValid()			{ return m_bValid; }
	void AddLockInfo(CLockInfo* pInfo);
protected:
	void OnTrace(CLockInfo*);
	CStringArray* CheckForDeadlock(BOOL bAcquire, DWORD dwThreadId, const CString& strObjectName, const CString& strFunction);
	void TraceLock(CLockInfo* pInfo, const CString& sObjectName, CStringArray* parLockedObjs);
	CString GetLockTraceFile();
	CString GetLockStructureFile();
public:
	virtual int Run();
};



void AfxTraceLock(BOOL bAcquire, BOOL bForWriting, LPCSTR lpszFunction, LPCSTR lpszObjectName);

#include "endh.dex"