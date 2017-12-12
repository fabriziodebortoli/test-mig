// LockTracer.cpp : implementation file
//
#include "stdafx.h"

#include "LockTracer.h"
#include "FileSystemFunctions.h"
#include "PathFinder.h"

class CLockInfo : public CObject
{
public:
	BOOL				m_bAcquire;
	BOOL				m_bForWriting;
	CStringA			m_sFunction;
	CStringA			m_sObjectName;
	CStringA			m_sThreadName;
	DWORD				m_nThreadId;
};


// CLockTracer

IMPLEMENT_DYNCREATE(CLockTracer, CTBWinThread)

//----------------------------------------------------------------------------
CLockTracer::CLockTracer()
: m_nPort(0), m_bValid(FALSE), m_nCounter(0), m_nReadLockIndex(-1), m_nWriteLockIndex(-1), m_bWorking(false)
{
	m_bAutoDelete = FALSE;
	m_pDataAvailable = new ::CSemaphore(0, LOCK_BUFFER);
	m_StartupEvent.ResetEvent();
}

//----------------------------------------------------------------------------
CLockTracer::~CLockTracer()
{
	POSITION pos = m_LockMap.GetStartPosition();
	while (pos)
	{
		CStringArray* pValue;
		DWORD dwKey;
		m_LockMap.GetNextAssoc(pos, dwKey, pValue);
		delete pValue;
	}

	delete m_pDataAvailable;
}

//----------------------------------------------------------------------------
CString CLockTracer::GetLockTraceFile()
{
	CString strPath = AfxGetPathFinder()->GetCustomPath();
	
	CString strFile;
	strFile.Format(_T("%s_%s_%d_tblocks.log"), GetComputerName(), GetName(GetProcessFileName()), m_nPort);
	PathAppend(strPath.GetBuffer(MAX_PATH), strFile);
	strPath.ReleaseBuffer();
	return strPath;
	
}
//----------------------------------------------------------------------------
CString CLockTracer::GetLockStructureFile()
{
	CString strPath = AfxGetPathFinder()->GetCustomPath();
	
	PathAppend(strPath.GetBuffer(MAX_PATH), _T("tblockstructure.bin"));
	strPath.ReleaseBuffer();
	return strPath;
}

//----------------------------------------------------------------------------
BOOL CLockTracer::InitInstance()
{
	m_bWorking = true;
	m_sTraceFile = GetLockTraceFile();
	m_sStructureFile = GetLockStructureFile();
	TRY
	{
		TRY
		{
			if (ExistFile(m_sStructureFile))
			{
				CFile file(m_sStructureFile, CFile::modeRead);
				CArchive ar(&file, CArchive::load);
				m_LockStructure.Serialize(ar);
			}
		}
		END_TRY
	

		if (!m_LogFile.Open(m_sTraceFile, CFile::shareDenyWrite | CFile::modeWrite | CFile::modeCreate))
			goto end;

		m_LogFile.SeekToEnd();
		
		SYSTEMTIME dateTime;
		::GetSystemTime(&dateTime);
		CString time;
		time.Format(_T("%u-%.2u-%.2u-%.2u-%.2u-%.2u"),
					dateTime.wYear, dateTime.wMonth, dateTime.wDay,
					dateTime.wHour, dateTime.wMinute, dateTime.wSecond);

		CString sTitle;
		sTitle.Format(
			_T("\r\n\r\n")
			_T("************************************************\r\n")
			_T("             Log started on %s                  \r\n")
			_T("************************************************\r\n\t\r\n"),
			time);

		m_LogFile.WriteString(sTitle);	
		m_LogFile.WriteString(_T("Potential Deadlock\tCounter\tThread Id\tThread Name\tFunction Name\tOperation\tType\tObject Name\tLock Sequence\r\n"));
		m_LogFile.Flush();
	}
	CATCH_ALL(e)
	{
		goto end;
	}
	END_CATCH_ALL
	SetThreadName("Lock logger");

	m_bValid = TRUE;

end:
	m_StartupEvent.SetEvent();
	return TRUE;
}

//----------------------------------------------------------------------------
int CLockTracer::ExitInstance()
{
	TRY
	{
		CFile file(m_sStructureFile, CFile::modeWrite | CFile::modeCreate);
		CArchive ar(&file, CArchive::store);
		m_LockStructure.Serialize(ar);
	}
	CATCH_ALL(e)
	{
		ASSERT(FALSE);	
	}
	END_CATCH_ALL

	return __super::ExitInstance();
}

//----------------------------------------------------------------------------
void CLockTracer::StopAndDestroy()
{
	m_WriteSection.Lock();
	
	//aspetto che il thread di lettura abbia svuotato la coda
	WaitForEmptyQueue();
	
	m_WriteSection.Unlock();

	m_bWorking = false;
	m_pDataAvailable->Unlock(1); //rilascio il thread che sta leggendo, che uscira` perche' m_bWorking e` FALSE
	
	WaitForSingleObject(m_hThread, INFINITE);
	delete this;
}

//----------------------------------------------------------------------------
int CLockTracer::Run()
{
	for (;;) 
	{
		if (!m_pDataAvailable->Lock() || !m_bWorking)
			break;

		m_nReadLockIndex++;
		if (m_nReadLockIndex == LOCK_BUFFER)
			m_nReadLockIndex = 0;
		OnTrace(m_LockInfos[m_nReadLockIndex]);
	}

	return ExitInstance();
}

//----------------------------------------------------------------------------
void CLockTracer::AddLockInfo(CLockInfo* pInfo)
{
	CSingleLock _lock(&m_WriteSection, TRUE);
	
	ASSERT(m_bWorking);

	m_nWriteLockIndex++;
	if (m_nWriteLockIndex == LOCK_BUFFER)
		m_nWriteLockIndex = 0;
	
	if (m_nWriteLockIndex == m_nReadLockIndex)
		WaitForEmptyQueue(); //aspetto che il thread che consuma abbia svuotato la coda

	m_LockInfos[m_nWriteLockIndex] = pInfo;
	m_pDataAvailable->Unlock(1);
}
//----------------------------------------------------------------------------
void CLockTracer::WaitForEmptyQueue()
{
	TRACE2("Emptying lock buffer (write index:%d; read index: %d)...\r\n", m_nWriteLockIndex, m_nReadLockIndex);
	
	//itero finche' ci sono elementi da leggere
	while (m_pDataAvailable->Lock(0L))
	{
		m_pDataAvailable->Unlock(1);
		Sleep(10);
	}

	TRACE2("After emptying: write index: %d; read index: %d)\r\n", m_nWriteLockIndex, m_nReadLockIndex);

	TRACE("DONE emptying lock buffer\r\n");
}

//----------------------------------------------------------------------------
CStringArray* CLockTracer::CheckForDeadlock(BOOL bAcquire, DWORD dwThreadId, const CString& sObjectName, const CString& strFunction)
{
	CStringArray* parLockedObjects = m_LockMap[dwThreadId];
	if (!parLockedObjects)
	{
		parLockedObjects = new CStringArray();
		m_LockMap[dwThreadId] = parLockedObjects;
	}

	int nLast = parLockedObjects->GetUpperBound();
	if (bAcquire)
	{
		if (nLast >= 0)
		{
			CString sLatestObj = parLockedObjects->GetAt(nLast);
			if (sLatestObj != sObjectName)
			{
				CStringArray arObjects;
				if (!m_LockStructure.CheckForDeadlock(sObjectName, sLatestObj, arObjects))
				{
					m_LogFile.WriteString(_T("* "));
					for (int i = 0; i < arObjects.GetCount(); i++)
					{
						m_LogFile.WriteString(arObjects.GetAt(i));
						m_LogFile.WriteString(_T(";"));
					}
					m_LogFile.WriteString(sObjectName);
					m_LogFile.WriteString(_T(";"));
				}
				else
				{
					m_LockStructure.AddRelationShip(sObjectName, sLatestObj, strFunction);
				}
			}
		}
		parLockedObjects->Add(sObjectName);
	}
	else
	{
		if (nLast >= 0)
		{
			BOOL bRemoved = FALSE;
			for (int i = nLast; i >= 0; i--)
			{
				CString sLatestObj = parLockedObjects->GetAt(i);
				if (sLatestObj == sObjectName)
				{
					parLockedObjects->RemoveAt(i);
					bRemoved = TRUE;
					break;
				}
			}
			ASSERT(bRemoved);
		}
	}
		
	m_LogFile.WriteString(_T("\t"));
	return parLockedObjects;

}
//----------------------------------------------------------------------------
void CLockTracer::TraceLock(CLockInfo* pInfo, const CString& sObjectName, CStringArray* parLockedObjs)
{
	CString s;
	s.Format(_T("%d\t"), m_nCounter++);
	m_LogFile.WriteString(s);
	
	USES_CONVERSION;

	s.Format(_T("%d"), pInfo->m_nThreadId);
	m_LogFile.WriteString(s);
	m_LogFile.WriteString(_T("\t"));
	m_LogFile.WriteString(A2T(pInfo->m_sThreadName));
	m_LogFile.WriteString(_T("\t"));
	m_LogFile.WriteString(A2T(pInfo->m_sFunction));
	m_LogFile.WriteString(_T("\t"));
	m_LogFile.WriteString(pInfo->m_bAcquire ? _T("LOCK\t") : _T("RELEASE\t"));
	m_LogFile.WriteString(pInfo->m_bForWriting ?  _T("WRITE\t") : _T("READ\t"));
	m_LogFile.WriteString(sObjectName);
	m_LogFile.WriteString(_T("\t"));
	
	for (int i = 0; i < parLockedObjs->GetCount(); i++)
	{
		m_LogFile.WriteString(parLockedObjs->GetAt(i));
		m_LogFile.WriteString(_T(";"));
	}
					
	m_LogFile.WriteString(_T("\r\n"));


	m_LogFile.Flush();
}

//----------------------------------------------------------------------------
void CLockTracer::OnTrace(CLockInfo* pInfo)
{
	try
	{
		ASSERT_VALID(pInfo);

		USES_CONVERSION;
		CString sObjectName = A2T(pInfo->m_sObjectName);
		
		CStringArray* pLockedObjs = CheckForDeadlock(pInfo->m_bAcquire, pInfo->m_nThreadId, sObjectName, A2T(pInfo->m_sFunction));
		TraceLock(pInfo, sObjectName, pLockedObjs);

		delete pInfo;
	}
	catch(...)
	{
	
	}
}


// CLockTracer message handlers

IMPLEMENT_SERIAL(CLockRelation, CObject, 1)
//----------------------------------------------------------------------------
CLockRelation::~CLockRelation() 
{
	for (int i = 0; i < m_arFunctions.GetCount(); i++)
		delete m_arFunctions[i];
}

//----------------------------------------------------------------------------
void CLockRelation::Serialize(CArchive& ar)
{
	m_strParentNames.Serialize(ar);
	
	if (ar.IsStoring())
	{
		ar << m_arFunctions.GetCount();
		for (int i = 0; i < m_arFunctions.GetCount(); i++)
		{
			CStringArray *pFunctions = m_arFunctions.GetAt(i);
			pFunctions->Serialize(ar);
		}
	}
	else
	{
		int count;
		ar >> count;
		for (int i = 0; i <count; i++)
		{
			CStringArray *pFunctions = new CStringArray;
			pFunctions->Serialize(ar);
			m_arFunctions.Add(pFunctions);
		}
	}
}

IMPLEMENT_SERIAL(CLockStructure, CObject, 1)

//----------------------------------------------------------------------------
CLockStructure::~CLockStructure()
{
	CString aKey;
	CLockRelation* pValue;
	POSITION pos = m_ObjectMap.GetStartPosition();
	while (pos)
	{
		m_ObjectMap.GetNextAssoc(pos, aKey, pValue);
		delete pValue;
	}
}
	
//----------------------------------------------------------------------------
void CLockStructure::AddRelationShip(const CString& strObject, const CString &strParent, const CString& sFunction)
{
	CLockRelation* pRelation = m_ObjectMap[strObject];
	if (pRelation == NULL)
	{
		pRelation = new CLockRelation();
		m_ObjectMap[strObject] = pRelation;
	}

	int index;
	CStringArray* pFunctions = NULL;
	if (!pRelation->Contains(strParent, index))
	{
		index = pRelation->m_strParentNames.Add(strParent);
		pFunctions = new CStringArray;
		pRelation->m_arFunctions.Add(pFunctions);
	}
	else
	{
		pFunctions = pRelation->m_arFunctions[index];
	}

	for (int i = 0; i < pFunctions->GetCount(); i++)
		if (pFunctions->GetAt(i) == sFunction)
			return;

	pFunctions->Add(sFunction);

} 

//----------------------------------------------------------------------------
BOOL CLockStructure::CheckForDeadlock(const CString& strTestObject, const CString& strLatestObject, CStringArray& arObjects)
{
	if (strTestObject == strLatestObject)
		return FALSE;

	BOOL bResult = TRUE;

	arObjects.Add(strLatestObject);
	CLockRelation* pRelation = m_ObjectMap[strLatestObject];
	if (!pRelation)
		return bResult;

	for (int i = 0; i < pRelation->m_strParentNames.GetCount(); i++)
		bResult = bResult && CheckForDeadlock(strTestObject, pRelation->m_strParentNames[i], arObjects);

	return bResult;
}

//----------------------------------------------------------------------------
void CLockStructure::Serialize(CArchive& ar)
{
	if (ar.IsStoring())
	{
		ar << m_ObjectMap.GetCount();
		CString aKey;
		CLockRelation* pValue;
		POSITION pos = m_ObjectMap.GetStartPosition();
		while (pos)
		{
			m_ObjectMap.GetNextAssoc(pos, aKey, pValue);
			if (!pValue || pValue->IsEmpty())
				continue;
			ar << aKey;
			pValue->Serialize(ar);
		}
	}
	else
	{
		int nCount;
		CString sKey;
		
		ar >> nCount;
		for (int i = 0; i < nCount; i++)
		{
			ar >> sKey;
			CLockRelation* pRel = new CLockRelation;
			pRel->Serialize(ar);
			m_ObjectMap[sKey] = pRel;
		}
	}
}

//----------------------------------------------------------------------------
void AfxTraceLock(BOOL bAcquire, BOOL bForWriting, LPCSTR lpszFunction, LPCSTR lpszObjectName)
{
	CLockTracer* pTracer = AfxGetApplicationContext()->GetLockTracer();
	if (!pTracer || !pTracer->m_hThread) return;

	CWinThread* pThread = AfxGetThread();
	if (!pThread) return;

	CLockInfo* pInfo = new CLockInfo();

	pInfo->m_bAcquire = bAcquire;
	pInfo->m_bForWriting = bForWriting;
	pInfo->m_sFunction = lpszFunction;
	pInfo->m_sObjectName = lpszObjectName;
	pInfo->m_sThreadName = 
	 (pThread->IsKindOf(RUNTIME_CLASS(CTBWinThread)))
		? ((CTBWinThread*) pThread)->GetThreadName()
		: pThread == AfxGetApp()
			? "Application thread"
			: "";
	pInfo->m_nThreadId = pThread->m_nThreadID;

	pTracer->AddLockInfo(pInfo);
}