#include "StdAfx.h"
#include "DataTypesFormatters.h"
#include ".\generalobjects.h"


IMPLEMENT_DYNCREATE(CManagedDllLoader, CObject)
//-----------------------------------------------------------------------------
CManagedDllLoader::CManagedDllLoader(LPCTSTR lpszDllName)
{
	hModule = ::LoadLibrary(lpszDllName);
	if (hModule != NULL)
	{
		LPF lpfn = (LPF) GetProcAddress(hModule, "?Initialize@@YAXXZ");
		if (lpfn != NULL)
			lpfn();
	}
}
//-----------------------------------------------------------------------------
CManagedDllLoader::~CManagedDllLoader()
{
	if (hModule != NULL)
	{
		LPF lpfn = (LPF) GetProcAddress(hModule, "?Terminate@@YAXXZ");
		if (lpfn != NULL)
			lpfn();
		::FreeLibrary(hModule);
	}
}


/////////////////////////////////////////////////////////////////////////////
//			CPerformanceCrono Declaration
/////////////////////////////////////////////////////////////////////////////
//
CPerformanceCrono::CPerformanceCrono()
{
	ClearTimes();
}

//----------------------------------------------------------------------------
void CPerformanceCrono::ClearTimes()
{
	m_dStartTick = 0;
	m_dElapsedTick = 0;
	m_dStopTick = 0;
	m_bRunning = FALSE;
}

//----------------------------------------------------------------------------
void CPerformanceCrono::Start()
{
	if (m_bRunning)
		return;

	m_dStartTick = GetTickCount();
	m_dElapsedTick = 0;
	m_dStopTick = 0;
	m_bRunning = TRUE;
}

//----------------------------------------------------------------------------
void CPerformanceCrono::Stop()
{
	if (!m_bRunning)
		return;

	DWORD aCurrTick = m_dStopTick = GetTickCount();
	// se ho chiamato Pause ho giá calcolato parte del tempo in m_dElapsedTick
	m_dElapsedTick += (aCurrTick >= m_dStartTick ? aCurrTick - m_dStartTick : 0);
	m_dStartTick = 0;
	m_bRunning = FALSE;
}

//----------------------------------------------------------------------------
void CPerformanceCrono::Pause()
{
	if (!m_bRunning)
		return;

	DWORD aCurrTick = GetTickCount();
	m_dElapsedTick = (aCurrTick >= m_dStartTick) ? aCurrTick - m_dStartTick : 0;
	m_dStartTick = 0;
	m_bRunning = FALSE;
}

//----------------------------------------------------------------------------
void CPerformanceCrono::Resume()
{
	if (m_bRunning)
		return;

	m_dStartTick = GetTickCount();
	m_bRunning = TRUE;
}


//----------------------------------------------------------------------------
CPerformanceCrono& CPerformanceCrono::operator =(const CPerformanceCrono& aPerformanceCrono)
{
	m_dStartTick = aPerformanceCrono.m_dStartTick;
	m_dElapsedTick = aPerformanceCrono.m_dElapsedTick;
	m_bRunning = aPerformanceCrono.m_bRunning;

	return *this;
}

///////////////////////////////////////////////////////////////////////////////
// 						CCounterElem Declaration
//////////////////////////////////////////////////////////////////////////////
//
//
//-----------------------------------------------------------------------------
CCounterElem::CCounterElem(int nOperation, LPCTSTR lpzTitle)
	:
	m_nOperation(nOperation),
	m_strTitle(lpzTitle),
	m_dTotalTime(0),
	m_lCount(0l)
{
}

//-----------------------------------------------------------------------------
void CCounterElem::Clear()
{
	m_dTotalTime = 0;
	m_lCount = 0;
}


//-----------------------------------------------------------------------------
CString CCounterElem::GetFormattedTime()
{
	CTickTimeFormatter aTickFormatter;
	return aTickFormatter.FormatTime(m_dTotalTime);
}


///////////////////////////////////////////////////////////////////////////////
// 						CCounterArray Declaration
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CCounterArray, Array)

//-----------------------------------------------------------------------------
void CCounterArray::ClearCounters()
{
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
		if (GetAt(nIdx)) GetAt(nIdx)->Clear();
}

//-----------------------------------------------------------------------------
CString CCounterArray::GetTitleAt(int nIdx) const
{
	if (nIdx < 0 || nIdx >= GetSize())
		return _T("");

	return GetAt(nIdx)->m_strTitle;
}

//-----------------------------------------------------------------------------
void CCounterArray::Pause()
{
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
		GetAt(nIdx)->Pause();
}

//-----------------------------------------------------------------------------
void CCounterArray::Resume()
{
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
		GetAt(nIdx)->Resume();
}

//-----------------------------------------------------------------------------
DWORD CCounterArray::MakeTotalTime() const
{
	DWORD dTotalTime = 0;
	for (int nIdx = 0; nIdx < GetSize(); nIdx++)
		dTotalTime += GetAt(nIdx)->m_dTotalTime;
	return dTotalTime;
}

//-----------------------------------------------------------------------------
CString CCounterArray::GetFormattedTimeAt(int nIdx) const
{
	if (nIdx < 0 || nIdx >= GetSize())
		return _T("");

	CString s;
	s.Format(_T("%d"), GetAt(nIdx)->m_dTotalTime);
	return s;
}

/////////////////////////////////////////////////////////////////////////////
//						class PerformanceManager
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
PerformanceManager::PerformanceManager()
{
	InitCounters();
}

//-----------------------------------------------------------------------------
void PerformanceManager::InitCounters()
{
	m_aTotalCrono.ClearTimes();

	m_bStartTime = FALSE;
}

//-----------------------------------------------------------------------------
void PerformanceManager::MakeTimeOperation(CCounterElem* pCounterElem, int eTime)
{
	if (!pCounterElem || !m_bStartTime)
	{
		ASSERT(FALSE);
		return;
	}

	switch (eTime)
	{
	case (START_TIME):
		pCounterElem->Start();
		pCounterElem->m_lCount++;
		break;
	case (STOP_TIME):
	{
		pCounterElem->Stop();
		break;
	}
	case (PAUSE_TIME):
		pCounterElem->Pause(); break;
	case (RESUME_TIME):
		pCounterElem->Resume(); break;
	default: ASSERT(FALSE); break;
	}
}

//-----------------------------------------------------------------------------
void PerformanceManager::MakeTimeOperation(int nCounter, int eTime)
{
	if (nCounter < 0 || nCounter >= m_aCounters.GetSize())
	{
		ASSERT(FALSE);
		return;
	}

	CCounterElem* pCounterElem = m_aCounters.GetAt(nCounter);

	if (!pCounterElem)
		MakeTimeOperation(pCounterElem, eTime);
	else
		ASSERT(FALSE);
}

//-----------------------------------------------------------------------------
CCounterElem* PerformanceManager::AddCounter(CString sName)
{
	int nIdx = m_aCounters.GetSize();
	CCounterElem* pElem = new CCounterElem(nIdx, sName);
	m_aCounters.Add(pElem);
	return pElem;
}

//-----------------------------------------------------------------------------
CCounterArray& PerformanceManager::GetCounters()
{
	return m_aCounters;
}

