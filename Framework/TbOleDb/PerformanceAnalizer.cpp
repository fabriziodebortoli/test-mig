
#include "stdafx.h"

#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\LocalizableObjs.h>

#include "sqltable.h"
#include "sqlobject.h"
#include "sqlconnect.h"

#include "performanceanalizer.hjson" //JSON AUTOMATIC UPDATE
#include "PerformanceAnalizer.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

static const TCHAR szSeparator		[] = _T(".");


//----------------------------------------------------------------------------
CString GetFormattedCount(int nCount)
{
	const rsize_t nLen = 10;
	TCHAR buff[nLen];

	_itot_s(nCount, buff, nLen, 10);
	FormatHelpers::InsertThousandSeparator(buff, szSeparator);

	return CString(buff);
}

///////////////////////////////////////////////////////////////////////////////
// 						CSqlCounterArray Declaration
//////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CSqlCounterArray, CCounterArray)


//-----------------------------------------------------------------------------
CString CSqlCounterArray::GetFormattedTimeAt(int nIdx) const
{
	if (nIdx < 0 || nIdx >= GetSize())
		return _T("");

	CTickTimeFormatter aTickFormatter;
	return aTickFormatter.FormatTime(GetAt(nIdx)->m_dTotalTime);
}



//-----------------------------------------------------------------------------
void CSqlCounterArray::LoadSqlOperations(PerformanceType ePerfType)
{
	switch (ePerfType)
	{
		case E_DB_TYPE:
		{
			Add(new CCounterElem(DB_ADD_PROPERTIES, _T("AddProperties")));
			Add(new CCounterElem(DB_PREPARE,		_T("Prepare")));
			Add(new CCounterElem(DB_BIND_COLUMNS,	_T("BindColumns")));
			Add(new CCounterElem(DB_BIND_PARAMETERS,_T("BindParameters")));
			Add(new CCounterElem(DB_OPEN_ROWSET,	_T("OpenRowset")));
			Add(new CCounterElem(DB_MOVE_FIRST,		_T("MoveFirst")));
			Add(new CCounterElem(DB_MOVE_PREV,		_T("MovePrev")));
			Add(new CCounterElem(DB_MOVE_NEXT,		_T("MoveNext")));
			Add(new CCounterElem(DB_MOVE_LAST,		_T("MoveLast")));
			/*Add(new CCounterElem(DB_UPDATE,		_T("Update")));
			Add(new CCounterElem(DB_INSERT,			_T("Insert")));
		*/	Add(new CCounterElem(DB_CLOSE_ROWSET,	_T("CloseRowset")));
			break;
		}
		case E_PROC_TYPE:
		{
			Add(new CCounterElem(PROC_STATEMENT_PREPARE, _T("StmPrepare")));
			Add(new CCounterElem(PROC_INIT_BUFFER,		 _T("InitBuffer")));
			Add(new CCounterElem(PROC_FIXUP_OPERATION,   _T("Fixup")));
			Add(new CCounterElem(PROC_ADD_BIND_ELEMENT,  _T("AddBindElem")));
			Add(new CCounterElem(PROC_COMMIT,			 _T("Commit")));
			Add(new CCounterElem(PROC_ROLLBACK,			 _T("Rollback")));			
			
			// data caching
			Add(new CCounterElem(DATA_CACHE_CLEAR,				_T("Data Cache Clear")));
			Add(new CCounterElem(DATA_CACHE_FIND,				_T("Data Cache Find")));
			Add(new CCounterElem(DATA_CACHE_INSERT,				_T("Data Cache Insert")));
			Add(new CCounterElem(DATA_CACHE_RECORD_REFRESHED,	_T("Data Cache Update Record")));
			Add(new CCounterElem(DATA_CACHE_RECORD_DELETED,		_T("Data Cache Delete Record")));
			
			break;
		}
		default: break;
	}
}

/////////////////////////////////////////////////////////////////////////////
//						class SqlPerformanceManager
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
SqlPerformanceManager::SqlPerformanceManager(CTBContext* pTBContext)
:
	m_pTBContext(pTBContext)
{
	InitCounters();
	m_aScrollCounters.LoadSqlOperations	(E_DB_TYPE);
	m_aForwardCounters.LoadSqlOperations(E_DB_TYPE);
	m_aProcCounters.LoadSqlOperations	(E_PROC_TYPE);
}

//-----------------------------------------------------------------------------
void SqlPerformanceManager::MakeDBTimeOperation(TimeOperation eTime, int nOperation, SqlRowSet* pSqlRowSet)
{
	if (!m_bStartTime && !pSqlRowSet) return;

	CCounterElem* pCounterElem = (pSqlRowSet->m_bScrollable) 
								? m_aScrollCounters.GetAt(nOperation) 
								: m_aForwardCounters.GetAt(nOperation);
	if (!pCounterElem)
	{
		ASSERT(FALSE);
		return;
	}

	switch (eTime)
	{
		case (START_TIME):
			//m_aDBCrono.Start(); 
			pCounterElem->m_lCount++;	
			pCounterElem->Start();
			if (nOperation == DB_OPEN_ROWSET)
			{
				if (pSqlRowSet->m_bScrollable)
					(pSqlRowSet->m_bUpdatable) ? m_nUpdatableScrollCursor++ : m_nReadOnlyScrollCursor++;
				else
					(pSqlRowSet->m_bUpdatable) ? m_nUpdatableForwardCursor++ : m_nReadOnlyForwardCursor++;
			}
			break;
		case (STOP_TIME):
			{ 
				pCounterElem->Stop();
				DWORD dElapsed = pCounterElem->GetElapsedTime();				
				m_dElapsedDBTime += dElapsed;
				m_dTotalDBTime += dElapsed;			
				break;
			}
		case (PAUSE_TIME):
			pCounterElem->Pause();break;
		case (RESUME_TIME):
			pCounterElem->Resume();break;
			default: ASSERT(FALSE);  break;
	}
}

//-----------------------------------------------------------------------------
void SqlPerformanceManager::MakeProcTimeOperation(TimeOperation eTime, int nOperation)
{
	if (!m_bStartTime) return;

	CCounterElem* pCounterElem = m_aProcCounters.GetAt(nOperation);

	if (!pCounterElem)
	{
		ASSERT(FALSE);
		return;
	}

	MakeTimeOperation(pCounterElem, eTime);
	if (eTime == STOP_TIME)
	{
		DWORD dElapsed = pCounterElem->GetElapsedTime();				
		m_dElapsedDBTime += dElapsed;
		m_dTotalDBTime += dElapsed;		
	}
}

//-----------------------------------------------------------------------------
void SqlPerformanceManager::InitCounters()
{
	__super::InitCounters();

	m_aTransCrono.ClearTimes();

	m_nRefTotal			= 0;
	m_nRefSecond		= 0;
	m_nRefAux			= 0;
	m_dTotalTime		= 0;
	m_dTotalDBTime		= 0;
	m_dOnOkTime			= 0;
	m_dOnOkDBTime		= 0;
	m_dPrimaryTime		= 0;
	m_dPrimaryDBTime	= 0;
	m_dSecondaryTime	= 0;
	m_dSecondaryDBTime	= 0;
	m_dAuxiliaryTime	= 0;
	m_dAuxiliaryDBTime	= 0;
	m_bSecondary		= FALSE;
	m_bAuxiliary		= FALSE;
	m_bStartTrans		= FALSE;

	m_dElapsedDBTime	= 0;
	m_dTotalForwardTime	= 0;
	m_dTotalScrollTime	= 0;
	m_dTotalProcTime	= 0;

	m_nReadOnlyForwardCursor  = 0;
	m_nUpdatableForwardCursor = 0;	
	m_nReadOnlyScrollCursor   = 0;
	m_nUpdatableScrollCursor  = 0;

	// fa la somma (x ogni tipo di operazione) dei tempi suddividendo per cursori forward e cursori scrollabili
	m_aForwardCounters.ClearCounters();
	m_aScrollCounters.ClearCounters();
	m_aProcCounters.ClearCounters();
	m_strAction.Empty();
}


//-----------------------------------------------------------------------------
void SqlPerformanceManager::BreakElapsedDBTime() 
{
	m_dElapsedDBTime = 0;
}

//----------------------------------------------------------------------------
void SqlPerformanceManager::StartTime(int nType, LPCTSTR pszActionName)
{
	switch (nType)
	{ 
		case TOTAL_TIME:
		{
			if (m_nRefTotal == 0)
			{
				InitCounters();
				m_bStartTime = TRUE;
				m_aTotalCrono.Start();
				m_strAction =  (pszActionName) ? pszActionName : _T("");
			}
			m_nRefTotal++;
			break;
		}

		case ONOK_TIME:
		case PRIMARY_TIME:
			if (!m_bSecondary && !m_bAuxiliary)
			{
				BreakElapsedDBTime();
				m_bStartTrans = TRUE;	
				m_aTransCrono.Start();			
			}
			break;

				
		case SECONDARY_TIME:
			if (!m_bAuxiliary)
			{
				if (m_nRefSecond == 0)
				{
					BreakElapsedDBTime();
					m_bSecondary = TRUE;				
					m_aTransCrono.Start();				
				}
				m_nRefSecond++;
			}
			break;

		case AUXILIARY_TIME:
			if (!m_bSecondary)
			{
				if (m_nRefAux == 0)
				{
					BreakElapsedDBTime();
					m_bAuxiliary = TRUE;				
					m_aTransCrono.Start();				
				}
				m_nRefAux++;
			}
			break;	
	}
}

//----------------------------------------------------------------------------
void SqlPerformanceManager::StopTime(int nType)
{
	switch (nType)
	{ 
		case TOTAL_TIME:
		{
			if (m_nRefTotal > 0)
				m_nRefTotal--;
			if (m_nRefTotal == 0)
			{
				m_aTotalCrono.Stop();
				m_bStartTime = FALSE;
				m_dTotalTime += m_aTotalCrono.GetElapsedTime();
				MakeTotalTime();
				if (m_pTBContext->m_pSqlPerformanceDlg)
					m_pTBContext->m_pSqlPerformanceDlg->RefreshTimes();

				//temporaneo. Da togliere
				CCounterElem* pCounterElem;
				CTickTimeFormatter  aTickFormatter;
				//prima faccio vedere i tempi dei forward
				TRACE_SQL(_T("PA ##################################################################"), NULL);
				TRACE_SQL(cwsprintf(_T("PA ######   FORWARD CURSORS ReadOnlyCount: %d UpdatableCount: %d TIME: %s  ######"), m_nReadOnlyForwardCursor, m_nUpdatableForwardCursor, aTickFormatter.FormatTime(m_dTotalForwardTime)), NULL);
				for (int nPos = 0; nPos < this->m_aForwardCounters.GetSize(); nPos++)
				{
					pCounterElem = m_aForwardCounters.GetAt(nPos);
					TRACE_SQL(cwsprintf(_T("PA %s			count: %d		time: %s"), pCounterElem->m_strTitle, pCounterElem->m_lCount, pCounterElem->GetFormattedTime()), NULL);
				}

				//poi faccio vedere i tempi dei scrollable
				TRACE_SQL(cwsprintf(_T("PA ######   SCROLLABLE CURSORS ReadOnlyCount: %d UpdatableCount: %d TIME: %s  ######"), m_nReadOnlyScrollCursor, m_nUpdatableScrollCursor, aTickFormatter.FormatTime(m_dTotalScrollTime)), NULL);

				for (int nPos = 0; nPos < this->m_aScrollCounters.GetSize(); nPos++)
				{
					pCounterElem = m_aScrollCounters.GetAt(nPos);
					TRACE_SQL(cwsprintf(_T("PA %s			count: %d		time: %s"), pCounterElem->m_strTitle, pCounterElem->m_lCount, pCounterElem->GetFormattedTime()), NULL);
				}

				//poi faccio vedere i tempi dei scrollable
				TRACE_SQL(cwsprintf(_T("PA ######   FUNCTIONALITY time: %s	######"), aTickFormatter.FormatTime(m_dTotalProcTime)), NULL);
				for (int nPos = 0; nPos < this->m_aProcCounters.GetSize(); nPos++)
				{
					pCounterElem = m_aProcCounters.GetAt(nPos);
					TRACE_SQL(cwsprintf(_T("PA %s			count: %d		time: %s"), pCounterElem->m_strTitle, pCounterElem->m_lCount, pCounterElem->GetFormattedTime()), NULL);
				}

				//poi i totali
				TRACE_SQL(cwsprintf(_T("PA ######   TOTAL TIME:			%s	######"), aTickFormatter.FormatTime(m_dTotalTime)), NULL);
				TRACE_SQL(cwsprintf(_T("PA OnOk Trans : %s		Primary Trans: %s	Secondary Trans: %s		Auxiliary Trans: %s    ######"), aTickFormatter.FormatTime(m_dOnOkTime), aTickFormatter.FormatTime(m_dPrimaryTime), aTickFormatter.FormatTime(m_dSecondaryTime), aTickFormatter.FormatTime(m_dAuxiliaryTime)), NULL);
				TRACE_SQL(cwsprintf(_T("PA ######   TOTAL DATABASETIME:	%s	######"), aTickFormatter.FormatTime(m_dTotalDBTime)), NULL);
				TRACE_SQL(cwsprintf(_T("PA OnOk Trans: %s		Primary Trans: %s	Secondary Trans: %s		Auxiliary Trans: %s    ######"), aTickFormatter.FormatTime(m_dOnOkDBTime), aTickFormatter.FormatTime(m_dPrimaryDBTime), aTickFormatter.FormatTime(m_dSecondaryDBTime), aTickFormatter.FormatTime(m_dAuxiliaryDBTime)), NULL);
				TRACE_SQL(_T("PA ##################################################################"), NULL);

			}
			break;
		}

		case ONOK_TIME:
		{
			if (!m_bSecondary && !m_bAuxiliary)
			{
				m_aTransCrono.Stop();
				m_dOnOkTime += m_aTransCrono.GetElapsedTime();
				m_dOnOkDBTime += m_dElapsedDBTime;
				m_bStartTrans = FALSE;
			}
			break;
		}
		
		case PRIMARY_TIME:
		{
			if (!m_bSecondary && !m_bAuxiliary)
			{
				m_aTransCrono.Stop();
				m_dPrimaryTime += m_aTransCrono.GetElapsedTime();		
				m_dPrimaryDBTime += m_dElapsedDBTime;
				m_bStartTrans = FALSE;
			}
			break;
		}
	
		case SECONDARY_TIME:
		{
			if (!m_bAuxiliary)
			{
				if (m_nRefSecond > 0)
					m_nRefSecond--;
				if (m_nRefSecond == 0)
				{	
					m_aTransCrono.Stop();
					m_dSecondaryTime += m_aTransCrono.GetElapsedTime();
					m_dSecondaryDBTime += m_dElapsedDBTime;
					m_bSecondary = FALSE;
				}
			}
			break;
		}

		case AUXILIARY_TIME:
		{
			if (!m_bSecondary)
			{
				if (m_nRefAux > 0)
					m_nRefAux--;
				if (m_nRefAux == 0)
				{	
					m_aTransCrono.Stop();
					m_dAuxiliaryTime += m_aTransCrono.GetElapsedTime();
					m_dAuxiliaryDBTime += m_dElapsedDBTime;
					m_bAuxiliary = FALSE;
				}
			}
			break;
		}
	}	
}

//----------------------------------------------------------------------------
void SqlPerformanceManager::PauseTime() 
{ 
	m_aForwardCounters.Pause();
	m_aScrollCounters.Pause();
	m_aProcCounters.Pause();

	m_aTotalCrono.Pause();
	m_aTransCrono.Pause();
}	

//----------------------------------------------------------------------------
void SqlPerformanceManager::ResumeTime() 
{ 
	m_aForwardCounters.Resume();
	m_aScrollCounters.Resume();
	m_aProcCounters.Resume();
	
	m_aTotalCrono.Resume();
	m_aTransCrono.Resume();
}

//----------------------------------------------------------------------------
void SqlPerformanceManager::MakeTotalTime()
{
	m_dTotalForwardTime	= m_aForwardCounters.MakeTotalTime();
	m_dTotalScrollTime	= m_aScrollCounters.MakeTotalTime();
	m_dTotalProcTime	= m_aProcCounters.MakeTotalTime();
	BreakElapsedDBTime();				
}

//----------------------------------------------------------------------------
CString SqlPerformanceManager::GetStringAppPerc()		
{
	DWORD aTotalAppTime = (m_dTotalTime > m_dTotalDBTime) ? m_dTotalTime - m_dTotalDBTime : 0;

	
	if (m_dTotalTime == 0 || aTotalAppTime == 0 || m_dTotalTime < aTotalAppTime)
		return _T("");

	double dPerc = double(aTotalAppTime * 100.0 / m_dTotalTime);
	round(dPerc, 7);
	return cwsprintf(_T("%.2f"), dPerc);
}

//----------------------------------------------------------------------------
CString SqlPerformanceManager::GetStringDBPerc()		
{
	if (m_dTotalTime == 0 || m_dTotalDBTime == 0 || m_dTotalTime < m_dTotalDBTime)
		return _T("");

	double dPerc = double(m_dTotalDBTime * 100.0 / m_dTotalTime);
	round(dPerc, 7);
	return cwsprintf(_T("%.2f"), dPerc);
}

//----------------------------------------------------------------------------
CString SqlPerformanceManager::GetStringPrimaryPerc()		
{
	if (m_dTotalTime == 0 || m_dPrimaryTime == 0 || m_dTotalTime < m_dPrimaryTime)
		return _T("");

	double dPerc = double(m_dPrimaryTime * 100.0 / m_dTotalTime);
	round(dPerc, 7);
	return cwsprintf(_T("%.2f"), dPerc);
}


//----------------------------------------------------------------------------
CString SqlPerformanceManager::GetStringPrimaryDBPerc()
{
	if (m_dPrimaryTime == 0 || m_dPrimaryDBTime == 0 || m_dPrimaryTime < m_dPrimaryDBTime)
		return _T("");

	double dPerc = double(m_dPrimaryDBTime * 100.0 / m_dPrimaryTime);
	round(dPerc, 7);
	return cwsprintf(_T("%.2f"), dPerc);
}


//----------------------------------------------------------------------------
CString SqlPerformanceManager::GetStringSecondaryPerc()		
{
	if (m_dTotalTime == 0 || m_dSecondaryTime == 0 || m_dTotalTime < m_dSecondaryTime)
		return _T("");

	double dPerc = double(m_dSecondaryTime * 100.0 / m_dTotalTime);
	round(dPerc, 7);
	return cwsprintf(_T("%.2f"), dPerc);
}

//----------------------------------------------------------------------------
CString SqlPerformanceManager::GetStringSecondaryDBPerc()
{
	if (m_dSecondaryTime == 0 || m_dSecondaryDBTime == 0 || m_dSecondaryTime < m_dSecondaryDBTime)
		return _T("");

	double dPerc = double(m_dSecondaryDBTime * 100.0 / m_dSecondaryTime);
	round(dPerc, 7);
	return cwsprintf(_T("%.2f"), dPerc);
}

//----------------------------------------------------------------------------
CString SqlPerformanceManager::GetStringAuxiliaryPerc()		
{
	if (m_dTotalTime == 0 || m_dAuxiliaryTime == 0 || m_dTotalTime < m_dAuxiliaryTime)
		return _T("");

	double dPerc = double(m_dAuxiliaryTime * 100.0 / m_dTotalTime);
	round(dPerc, 7);		
	return cwsprintf(_T("%.2f"), dPerc);
}

//----------------------------------------------------------------------------
CString SqlPerformanceManager::GetStringAuxiliaryDBPerc()
{
	if (m_dAuxiliaryTime == 0 || m_dAuxiliaryDBTime == 0 || m_dAuxiliaryTime < m_dAuxiliaryDBTime)
		return _T("");

	double dPerc = double(m_dAuxiliaryDBTime * 100.0 / m_dAuxiliaryTime);
	round(dPerc, 7);
	return cwsprintf(_T("%.2f"), dPerc);
}

//---------------------------------------------------------------------------
// CCounterGrid
//---------------------------------------------------------------------------
//---------------------------------------------------------------------------
CCounterGrid::CCounterGrid()
	:
	CBCGPListCtrl		(),
	m_pCounters		(NULL)
{
}

//---------------------------------------------------------------------------
void CCounterGrid::PreSubclassWindow() 
{
	SetExtendedStyle(LVS_REPORT);

	CRect rcGrid;
	GetClientRect(&rcGrid);
	int nColWidth = rcGrid.Width() / 3;

	InsertColumn(0, _T("Operation"), LVCFMT_RIGHT, nColWidth + 10);
	InsertColumn(1, _T("hh:mm:ss,ms"), LVCFMT_RIGHT, nColWidth + 23);
	InsertColumn(2, _T("Count"), LVCFMT_RIGHT, nColWidth - 33);

	SetBkColor(AfxGetThemeManager()->GetPerformanceAnalyzerBkgColor());

	SetTextBkColor(AfxGetThemeManager()->GetPerformanceAnalyzerForeColor());

	CBCGPListCtrl::PreSubclassWindow();
}

//----------------------------------------------------------------------------
void CCounterGrid::InitItems(CCounterArray* pCounterArray)
{
	if (!pCounterArray)
		return;
	
	m_pCounters = pCounterArray;

	int nPos = -1;

	for (int nIdx = 0; nIdx < m_pCounters->GetSize(); nIdx++)
	{
		nPos = InsertItem(nIdx, _T(""));
		SetItemText(nPos, 0, m_pCounters->GetTitleAt(nIdx));
		SetItemText(nPos, 1, _T(""));
		SetItemText(nPos, 2, _T(""));
	}
}

//----------------------------------------------------------------------------
/*void CCounterGrid::ViewCounter()
{
	TCHAR buff[10];
	long nTotValue = 0;
	for (int nPos = 0; nPos < GetItemCount(); nPos++)
	{
		_ltot(m_pCounters.GetCurrAt(nPos), buff, 10);
		FormatHelpers::InsertThousandSeparator(buff, szSeparator);
		SetItemText(nPos, 1, (LPCTSTR)buff);
		
		_ltot(m_aLocalCounters.GetPrevAt(nPos), buff, 10);
		FormatHelpers::InsertThousandSeparator(buff, szSeparator);
		SetItemText(nPos, 2, (LPCTSTR)buff);

		nTotValue = m_aLocalCounters.GetTotAt(nPos);
		_ltot(nTotValue, buff, 10);
		FormatHelpers::InsertThousandSeparator(buff, szSeparator);
		SetItemText(nPos, 3, (LPCTSTR)buff);
	}
}*/

//----------------------------------------------------------------------------
void CCounterGrid::RefreshCounters()
{
	for (int nPos = 0; nPos < GetItemCount(); nPos++)
	{
		SetItemText(nPos, 1, (LPCTSTR)m_pCounters->GetFormattedTimeAt(nPos));
		SetItemText(nPos, 2, (LPCTSTR) GetFormattedCount(m_pCounters->GetAt(nPos)->m_lCount));
	}
}

/////////////////////////////////////////////////////////////////////////////
// SqlPerformanceDlg dialog
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC(SqlPerformanceDlg, CLocalizableDialog);

BEGIN_MESSAGE_MAP(SqlPerformanceDlg, CLocalizableDialog)
	//{{AFX_MSG_MAP(SqlPerformanceDlg)
	ON_WM_CLOSE		()
	ON_WM_NCDESTROY ()
	//ON_BN_CLICKED ( ID_PERFORMANCE_REFRESH,	OnRefreshButton)	
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-------------------------------------------------------------------------
SqlPerformanceDlg::SqlPerformanceDlg(CTBContext* pTBContext, CWnd* pParent) 	
	: 
	CLocalizableDialog		(IDD_PERFORMANCE_DLG, pParent),
	m_pSqlPerformanceMng	(NULL),
	m_pTBContext			(pTBContext)
{
	ASSERT(m_pTBContext);
	m_bInOpenDlgCounter = FALSE;

	m_pSqlPerformanceMng = m_pTBContext->CreateSqlPerformanceMng();
	m_pTBContext->SetSqlPerformanceDlg(this);

	Create (IDD_PERFORMANCE_DLG, pParent);
}

//-------------------------------------------------------------------------
SqlPerformanceDlg::~SqlPerformanceDlg() 	
{
	m_pTBContext->DestroySqlPerformanceMng();
}

//---------------------------------------------------------------------------------
BOOL SqlPerformanceDlg::OnInitDialog()
{
	CLocalizableDialog::OnInitDialog();

	m_aCounterScrollList.SubclassDlgItem(IDC_PERFORMANCE_SCROLLABLE_LIST, this);
	m_aCounterForwardList.SubclassDlgItem(IDC_PERFORMANCE_FORWARD_LIST, this);
	m_aCounterProcList.SubclassDlgItem(IDC_PERFORMANCE_PROC_LIST, this);
	//m_aDocumentList.SubclassDlgItem(IDC_PERFORMANCE_DOC_LIST, this);

	m_aCounterScrollList.InitItems(&m_pSqlPerformanceMng->m_aScrollCounters);
	m_aCounterForwardList.InitItems(&m_pSqlPerformanceMng->m_aForwardCounters);
	m_aCounterProcList.InitItems(&m_pSqlPerformanceMng->m_aProcCounters);
//	m_aDocumentList.InitItems(m_pTBContext->GetDocument()->GetCounters());

	return TRUE;
}

//----------------------------------------------------------------------------
CString SqlPerformanceDlg::GetFormattedTime(DWORD dTime)
{
	return m_aTickFormatter.FormatTime(dTime);	
}

//----------------------------------------------------------------------------
void SqlPerformanceDlg::RefreshTimeDlgItems()
{	
	GetDlgItem(IDC_PERFORMANCE_TOT_TIME_SCROLL)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dTotalScrollTime));
	GetDlgItem(IDC_PERFORMANCE_TOT_TIME_FORWARD)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dTotalForwardTime));
	GetDlgItem(IDC_PERFORMANCE_TOT_TIME_PROC)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dTotalProcTime));

	GetDlgItem(IDC_PERFORMANCE_ACTION_NAME)->SetWindowText(m_pSqlPerformanceMng->m_strAction);

	GetDlgItem(IDC_PERFORMANCE_NUMB_FORWARD_RO)->SetWindowText(GetFormattedCount(m_pSqlPerformanceMng->m_nReadOnlyForwardCursor));
	GetDlgItem(IDC_PERFORMANCE_NUMB_FORWARD_UP)->SetWindowText(GetFormattedCount(m_pSqlPerformanceMng->m_nUpdatableForwardCursor));

	GetDlgItem(IDC_PERFORMANCE_NUMB_SCROLL_RO)->SetWindowText(GetFormattedCount(m_pSqlPerformanceMng->m_nReadOnlyScrollCursor));
	GetDlgItem(IDC_PERFORMANCE_NUMB_SCROLL_UP)->SetWindowText(GetFormattedCount(m_pSqlPerformanceMng->m_nUpdatableScrollCursor));


	GetDlgItem(IDC_PERFORMANCE_OPERATION_TIME)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dTotalTime));
	GetDlgItem(IDC_PERFORMANCE_DATABASE_TIME)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dTotalDBTime));
	GetDlgItem(IDC_PERFORMANCE_DATABASE_TIME_PERC)->SetWindowText(m_pSqlPerformanceMng->GetStringDBPerc());

	DWORD dAppTime = (m_pSqlPerformanceMng->m_dTotalTime > m_pSqlPerformanceMng->m_dTotalDBTime) 
					? m_pSqlPerformanceMng->m_dTotalTime - m_pSqlPerformanceMng->m_dTotalDBTime 
					: 0;

	GetDlgItem(IDC_PERFORMANCE_APP_TIME)->SetWindowText(GetFormattedTime(dAppTime));
	GetDlgItem(IDC_PERFORMANCE_APP_TIME_PERC)->SetWindowText(m_pSqlPerformanceMng->GetStringAppPerc());

	GetDlgItem(IDC_PERFORMANCE_ONOK_TRANS)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dOnOkTime));
	GetDlgItem(IDC_PERFORMANCE_ONOK_TRANS_DB)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dOnOkDBTime));

	GetDlgItem(IDC_PERFORMANCE_PRIMARY_TIME)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dPrimaryTime));
	GetDlgItem(IDC_PERFORMANCE_PRIMARY_TIME_PERC)->SetWindowText(m_pSqlPerformanceMng->GetStringPrimaryPerc());
	GetDlgItem(IDC_PERFORMANCE_PRIMARY_TIME_DB)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dPrimaryDBTime));
	GetDlgItem(IDC_PERFORMANCE_PRIMARY_TIME_DB_PERC)->SetWindowText(m_pSqlPerformanceMng->GetStringPrimaryDBPerc());

	GetDlgItem(IDC_PERFORMANCE_SECOND_TIME)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dSecondaryTime));
	GetDlgItem(IDC_PERFORMANCE_SECOND_TIME_PERC)->SetWindowText(m_pSqlPerformanceMng->GetStringSecondaryPerc());
	GetDlgItem(IDC_PERFORMANCE_SECOND_TIME_DB)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dSecondaryDBTime));
	GetDlgItem(IDC_PERFORMANCE_SECOND_TIME_DB_PERC)->SetWindowText(m_pSqlPerformanceMng->GetStringSecondaryDBPerc());	
	
	GetDlgItem(IDC_PERFORMANCE_AUX_TIME)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dAuxiliaryTime));
	GetDlgItem(IDC_PERFORMANCE_AUX_TIME_PERC)->SetWindowText(m_pSqlPerformanceMng->GetStringAuxiliaryPerc());
	GetDlgItem(IDC_PERFORMANCE_AUX_TIME_DB)->SetWindowText(GetFormattedTime(m_pSqlPerformanceMng->m_dAuxiliaryDBTime));
	GetDlgItem(IDC_PERFORMANCE_AUX_TIME_DB_PERC)->SetWindowText(m_pSqlPerformanceMng->GetStringAuxiliaryDBPerc());	
}

//----------------------------------------------------------------------------
void SqlPerformanceDlg::RefreshTimes()
{
	m_aCounterScrollList.RefreshCounters();
	m_aCounterForwardList.RefreshCounters();
	m_aCounterProcList.RefreshCounters();
	RefreshTimeDlgItems();
}

//------------------------------------------------------------------------------
void SqlPerformanceDlg::PostNcDestroy ()
{
	m_pTBContext->SetSqlPerformanceDlg(NULL);
	delete this;
}

//------------------------------------------------------------------------------
//void SqlPerformanceDlg::OnClose()
//{
//	DestroyWindow();
//}

//------------------------------------------------------------------------------
void SqlPerformanceDlg::OnCancel()
{
	CLocalizableDialog::OnCancel();
	//DestroyWindow();
}

//------------------------------------------------------------------------------
void SqlPerformanceDlg::CloseDialog()
{
	OnCancel();
}

