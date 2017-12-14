#include "StdAfx.h"

#include ".\locktracer.h"
#include ".\threadcontext.h"

//-----------------------------------------------------------------------------
BOOL CTBLock::Lock_DataRead(BOOL bTry)
{
	DWORD nThread = ::GetCurrentThreadId();
	while (true)
	{
		{//lock scope
			CSingleLock _lock(&m_Locker, TRUE);
			if (!m_nWritingThread || m_nWritingThread == nThread)
			{
				m_arReadingThreads.Add(nThread);
				if (m_arReadingThreads.GetCount() == 1)//first reader blocks writers
					m_ResourceAvailable.ResetEvent();//acquire lock, semaphore is red
				ASSERT(m_nWritingThread == 0 || m_nWritingThread == nThread);
				return TRUE;
			}
		}
		if (bTry)
			return FALSE;
		//check if the free for all semaphore is green
		WaitForSingleObject(m_ResourceAvailable, INFINITE);

	}
}

//-----------------------------------------------------------------------------
void CTBLock::Unlock_DataRead()
{
	CSingleLock _lock(&m_Locker, TRUE);
	DWORD nThread = ::GetCurrentThreadId();
	for (int i = m_arReadingThreads.GetUpperBound(); i >= 0; i--)
	{
		if (m_arReadingThreads[i] == nThread)
		{
			m_arReadingThreads.RemoveAt(i);
			break;
		}
	}
	//last reader, if it is not also a writer, frees the resource
	if (m_arReadingThreads.GetCount() == 0 && m_nWritingThread == 0)//no more reading threads
		m_ResourceAvailable.SetEvent();//green semaphore: resource is available!
}


//-----------------------------------------------------------------------------
BOOL CTBLock::Lock_DataWrite(BOOL bTry)
{
	DWORD nThread = ::GetCurrentThreadId();
	while (true)
	{
		{
			CSingleLock _lock(&m_Locker, TRUE);
			//someone is reading, if it's only me and no other one, go on
			if (m_arReadingThreads.GetCount())
			{
				for (int i = m_arReadingThreads.GetUpperBound(); i >= 0; i--)
				{
					if (m_arReadingThreads[i] != nThread)
						goto waitForWrite;
				}
			}
			//no one is writing: can acquire lock
			if (m_nWritingThread == 0)
			{
				m_nWritingThread = nThread;
				m_ResourceAvailable.ResetEvent();//acquire lock, semaphore is red
			}
			//someone is writing, but it's not me! Wait for green semaphore!
			else if (m_nWritingThread != nThread)
				goto waitForWrite;


#ifdef DEBUG
			for (int i = 0; i < m_arReadingThreads.GetCount(); i++)
			{
				ASSERT(m_arReadingThreads[i] == nThread);
			}
#endif
			//otherwise increase lock count and return
			m_nWritingCount++;
			return TRUE;
		}
	waitForWrite:
		if (bTry)
			return FALSE;
		//check if the free for all semaphore is green
		WaitForSingleObject(m_ResourceAvailable, INFINITE);

	}
}
//-----------------------------------------------------------------------------
void CTBLock::Unlock_DataWrite()
{
	CSingleLock _lock(&m_Locker, TRUE);
	ASSERT(m_nWritingThread == ::GetCurrentThreadId());

	ASSERT(m_nWritingCount > 0 && m_arReadingThreads.GetCount() == 0);
	m_nWritingCount--;

	if (m_nWritingCount == 0)//no more writing threads
	{
		m_nWritingThread = 0;
		m_ResourceAvailable.SetEvent();//green semaphore: resource is available!
	}
};


//-----------------------------------------------------------------------------
CTBMultiAutoLock::CTBMultiAutoLock()
		: m_bForWriting(FALSE), m_pszFunction(NULL), m_pLockableObj(NULL), m_bLockAcquired(FALSE)
{
}
	
//-----------------------------------------------------------------------------
CTBMultiAutoLock::CTBMultiAutoLock(BOOL bForWriting, const CTBLockable* const pLockableObj, LPCSTR lpszFunction, const DWORD dwMilliseconds /*= LOCK_TIMEOUT*/)
{
	GetLock(bForWriting, pLockableObj, lpszFunction, dwMilliseconds);
}

//-----------------------------------------------------------------------------
void CTBMultiAutoLock::GetLock(BOOL bForWriting, const CTBLockable* const pLockableObj, LPCSTR lpszFunction, const DWORD dwMilliseconds /*= LOCK_TIMEOUT*/)
{
	m_bForWriting	= bForWriting;
	m_pszFunction	= lpszFunction;
	m_pLockableObj	= pLockableObj;
	
	m_bLockAcquired = m_pLockableObj->Lock(bForWriting, dwMilliseconds, m_pszFunction, FALSE);
}

//-----------------------------------------------------------------------------
BOOL CTBMultiAutoLock::TryGetLock(BOOL bForWriting, const CTBLockable* const pLockableObj, LPCSTR lpszFunction, const DWORD dwMilliseconds /*= LOCK_TIMEOUT*/)
{
	m_bForWriting	= bForWriting;
	m_pszFunction	= lpszFunction;
	m_pLockableObj	= pLockableObj;

	m_bLockAcquired = m_pLockableObj->Lock(bForWriting, dwMilliseconds, m_pszFunction, TRUE);
	return m_bLockAcquired;
}
//-----------------------------------------------------------------------------
CTBMultiAutoLock::~CTBMultiAutoLock()
{
	if (m_bLockAcquired)
		m_pLockableObj->Unlock(m_bForWriting, m_pszFunction);
}


///////////////////////////////////////////////////////////////////////////////
//		CTBLockable
///////////////////////////////////////////////////////////////////////////////

BOOL CTBLockable::m_sbLockingEnabled = FALSE;
BOOL CTBLockable::m_sbLockTraceEnabled = FALSE;

void CTBLockable::EnableLocking (BOOL bEnable) 
{
	m_sbLockingEnabled = bEnable; 
	m_sbLockTraceEnabled = AfxGetApplicationContext()->GetLockTracer() != NULL;
}

//-----------------------------------------------------------------------------
CTBLockable::CTBLockable( ) throw()
{
#ifdef DEBUG
	m_InternalCritical.m_pLockable = this;
#endif
}

//-----------------------------------------------------------------------------
CTBLockable::~CTBLockable() throw()
{
	
}

//-----------------------------------------------------------------------------
BOOL CTBLockable::Lock(const BOOL bForWriting, const DWORD dwMilliseconds, LPCSTR pszFunction, BOOL bTry) const throw()
{
	if (!m_sbLockingEnabled) return FALSE;

#ifdef _DEBUG

	if (bForWriting)
	{
		if (AfxGetThreadContext()->m_ReadLockMap[this] > 0)
		{
			USES_CONVERSION;
			TRACE1("WARNING! PROMOTING A READ LOCK TO WRITE LOCK MAY CAUSE DEADLOCKS!!! OBJECT NAME: %s\r\n", A2T(GetObjectName()));
			ASSERT(FALSE);
		}
	}

#endif

	if (bForWriting)
	{
		if (m_sbLockTraceEnabled) 
			AfxTraceLock(TRUE, bForWriting, pszFunction, GetObjectName());

		m_InternalCritical.Lock_DataWrite(bTry);
	}
	else
	{
		
		if (m_sbLockTraceEnabled) 
			AfxTraceLock(TRUE, bForWriting, pszFunction, GetObjectName());
		
			m_InternalCritical.Lock_DataRead(bTry);

	}

#ifdef _DEBUG

	if (bForWriting)
		AfxGetThreadContext()->m_WriteLockMap[this]++;
	else
		AfxGetThreadContext()->m_ReadLockMap[this]++;

#endif

	return TRUE;
}

//-----------------------------------------------------------------------------
void CTBLockable::Unlock(const BOOL bForWriting, LPCSTR pszFunction) const throw()
{
	if (!m_sbLockingEnabled) return;

	if (m_sbLockTraceEnabled) 
		AfxTraceLock(FALSE, bForWriting, pszFunction, GetObjectName());
		
	bForWriting 
		? m_InternalCritical.Unlock_DataWrite()
		: m_InternalCritical.Unlock_DataRead();

#ifdef _DEBUG

	if (bForWriting)
		AfxGetThreadContext()->m_WriteLockMap[this]--;
	else
		AfxGetThreadContext()->m_ReadLockMap[this]--;
#endif
}



//-----------------------------------------------------------------------------
BaseSmartLock::BaseSmartLock (LPSTR pszFunction)						
{
	m_pLockable = NULL; 
	m_bForWriting = FALSE;
	m_pszFunction = pszFunction;
	m_bLockAcquired = FALSE;
}

//-----------------------------------------------------------------------------
BaseSmartLock::BaseSmartLock (const CTBLockable* ptr, BOOL bForWriting, LPSTR pszFunction)
{
	m_bForWriting = bForWriting;
	m_pLockable = ptr;
	m_pszFunction = pszFunction;
	m_bLockAcquired = FALSE;

	if (m_pLockable)
	{
		m_bLockAcquired = m_pLockable->Lock(bForWriting, LOCK_TIMEOUT, m_pszFunction, FALSE);
	}
}

//-----------------------------------------------------------------------------
void BaseSmartLock::Assign(const BaseSmartLock& sPtr)
{
	m_pLockable = sPtr.m_pLockable;
	m_bForWriting = sPtr.m_bForWriting;
	m_bLockAcquired = FALSE;

	if (m_pLockable)
	{
		m_bLockAcquired = m_pLockable->Lock(m_bForWriting, LOCK_TIMEOUT, m_pszFunction, FALSE);
	}
}

//-----------------------------------------------------------------------------
BaseSmartLock::~BaseSmartLock ()						
{
	if (m_pLockable && m_bLockAcquired)
			m_pLockable->Unlock(m_bForWriting, m_pszFunction);
}