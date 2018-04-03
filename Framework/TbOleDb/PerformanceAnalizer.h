
#pragma once

#include <TbGeneric\FormatsHelpers.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\LocalizableObjs.h>

//includere alla fine degli include del .H
#include "beginh.dex"


class SqlRowSet;
class SqlSession;
class CTBContext;
class SqlPerformanceMng;

enum PerformanceType { E_PROC_TYPE, E_DB_TYPE}; 

#define START_DB_TIME(Operation)	{}//if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeDBTimeOperation(START_TIME,  Operation, this); 
#define STOP_DB_TIME(Operation)		{}//if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeDBTimeOperation(STOP_TIME,	 Operation, this); 
#define PAUSE_DB_TIME(Operation)	{}//if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeDBTimeOperation(PAUSE_TIME,  Operation, this); 
#define RESUME_DB_TIME(Operation)	{}//if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeDBTimeOperation(RESUME_TIME, Operation, this); 

#define START_PROC_TIME(Operation)	{}//if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeProcTimeOperation (START_TIME,	Operation);
#define STOP_PROC_TIME(Operation)	{}//if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeProcTimeOperation (STOP_TIME,	Operation);
#define PAUSE_PROC_TIME(Operation)	{}//if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeProcTimeOperation (PAUSE_TIME,  Operation);
#define RESUME_PROC_TIME(Operation) {}//if (m_pContext->m_pSqlPerformanceMng) m_pContext->m_pSqlPerformanceMng->MakeProcTimeOperation (RESUME_TIME, Operation);


// operazioni standard di comunicazione con il database
#define DB_OPEN_TABLE		0
#define DB_CONNECT_CMD		1
#define DB_BIND_COLUMNS		2
#define DB_BIND_PARAMS		3
#define DB_SET_PARAMS_VALUE	4
#define DB_FIXUP_COLUMNS	5
#define DB_EXECUTE_CMD		6
#define DB_EXECUTE_NOQUERY	7
#define DB_EXECUTE_SCALAR	8
#define DB_MOVE_FIRST		9	
#define DB_MOVE_PREV		10	
#define DB_MOVE_NEXT		11	
#define DB_MOVE_LAST		12
#define DB_DISCONNECT_CMD	13
#define DB_CLOSE_TABLE		14
#define DB_PREPARE			15
#define LAST_DB_OPERATION  DB_PREPARE // ultimo numero le prossime operazioni devo partire da questo + 1

// non di cursori
#define	PROC_OPEN_CONNECTION	0
#define PROC_INIT_BUFFER		1
#define PROC_COMMIT				2
#define PROC_ROLLBACK			3
#define PROC_CLOSE_CONNECTION	4


// Data Caching Performance
#define DATA_CACHE_CLEAR			5
#define DATA_CACHE_FIND				6
#define DATA_CACHE_INSERT			7
#define DATA_CACHE_RECORD_REFRESHED	8
#define DATA_CACHE_RECORD_DELETED	9

#define TOTAL_TIME		0 
#define ONOK_TIME		1 // parte di onoktransaction e lock
#define PRIMARY_TIME	2//  primary transaction
#define SECONDARY_TIME	3 // secondary transaction (OnXXXTransaction)
#define AUXILIARY_TIME	4 // auxiliary transaction (OnExtraXXXTransaction)


///////////////////////////////////////////////////////////////////////////////
// 						CCounterArray Definition
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CSqlCounterArray : public CCounterArray
{
	DECLARE_DYNAMIC(CSqlCounterArray)

public:
	virtual CString	GetFormattedTimeAt(int) const;
	void	LoadSqlOperations(PerformanceType);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " CSqlCounterArray\n"); }
	void AssertValid() const{ CObArray::AssertValid(); }
#endif //_DEBUG
};


/////////////////////////////////////////////////////////////////////////////
//						class SqlPerformanceManager
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlPerformanceManager : public PerformanceManager
{
	friend class SqlPerformanceDlg;

private:
	CPerformanceCrono	m_aTransCrono;  // to estimate the time of the single transactions (primary, secondary , auxiliary)
	CTBContext*			m_pTBContext;

public:
	DWORD m_dTotalTime;
	DWORD m_dOnOkTime;
	DWORD m_dPrimaryTime;
	DWORD m_dSecondaryTime;
	DWORD m_dAuxiliaryTime;
	DWORD m_dElapsedDBTime;	
	DWORD m_dTotalDBTime;
	DWORD m_dTotalProcTime;
	DWORD m_dOnOkDBTime;
	DWORD m_dPrimaryDBTime;		// saving time primary transaction
	DWORD m_dSecondaryDBTime;	// saving time secondary transaction
	DWORD m_dAuxiliaryDBTime;	// saving time auxiliary transaction
	DWORD m_dTotalForwardTime;	
	DWORD m_dTotalScrollTime;	

	int	  m_nReadOnlyForwardCursor;
	int	  m_nUpdatableForwardCursor;
	int	  m_nReadOnlyScrollCursor;
	int	  m_nUpdatableScrollCursor;

	CSqlCounterArray m_aForwardCounters;
	CSqlCounterArray m_aScrollCounters;
	CSqlCounterArray m_aProcCounters;
	
	CString	m_strAction;

private:
	int					m_nRefTotal;
	int					m_nRefSecond;
	int					m_nRefAux;
	BOOL				m_bSecondary;
	BOOL				m_bAuxiliary;
	BOOL				m_bStartTrans;

public:
	SqlPerformanceManager(CTBContext* pTBContext);

private:
	void InitCounters();	
	void BreakElapsedDBTime(); 
	void MakeTotalTime();
public:
	void MakeDBTimeOperation	 (TimeOperation, int, SqlRowSet*);
	void MakeProcTimeOperation(TimeOperation eTime, int nOperation);

	void StartTime	(int, LPCTSTR pszActionName = NULL);
	void StopTime	(int);
	void PauseTime	();
	void ResumeTime ();

	
	void ClearTimes();		
	BOOL IsStartAction() const { return m_bStartTime; }	

	CString GetStringDBPerc();	
	CString GetStringAppPerc();
	CString GetStringPrimaryPerc();	
	CString GetStringPrimaryDBPerc();	
	CString GetStringSecondaryPerc();
	CString GetStringSecondaryDBPerc();		
	CString GetStringAuxiliaryPerc();
	CString GetStringAuxiliaryDBPerc();		
};


//---------------------------------------------------------------------------------
//class CCounterGrid 
//---------------------------------------------------------------------------------
class TB_EXPORT CCounterGrid : public CBCGPListCtrl
{
public:
	CCounterArray*	m_pCounters;

// Construction
public:
	CCounterGrid();

public:
	void InitItems		(CCounterArray*);
	void RefreshCounters();
	
protected:
	virtual void PreSubclassWindow();
};


/////////////////////////////////////////////////////////////////////////////
// SqlPerformanceDlg dialog
/////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT SqlPerformanceDlg : public CLocalizableDialog
{
	DECLARE_DYNAMIC(SqlPerformanceDlg)

private:
	SqlPerformanceManager*	m_pSqlPerformanceMng;

	CCounterGrid		m_aCounterScrollList;
	CCounterGrid		m_aCounterForwardList;
	CCounterGrid		m_aCounterProcList;
	CCounterGrid		m_aDocumentList;

	CTBContext*			m_pTBContext;
	CTickTimeFormatter  m_aTickFormatter;

// Construction
public:
	SqlPerformanceDlg(CTBContext*, CWnd* pParent = NULL );
	~SqlPerformanceDlg();

private:
	void	RefreshTimeDlgItems();
	CString GetFormattedTime  (DWORD);

public:
	void CloseDialog		();
	void RefreshTimes		();

protected:
	virtual BOOL OnInitDialog	();
	virtual	void OnCancel		();	
	virtual	void PostNcDestroy	();

// Implementation
protected:
	//{{AFX_MSG(SqlPerformanceDlg)	
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#include "endh.dex"



