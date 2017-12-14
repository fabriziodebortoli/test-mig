#pragma once

class DataObj;
#include "array.h"

#include "beginh.dex"

class TB_EXPORT CManagedDllLoader : public CObject
{
	DECLARE_DYNCREATE(CManagedDllLoader)
private:
	HMODULE hModule;

	typedef void (FAR PASCAL *LPF)();
protected:
	CManagedDllLoader() : hModule(NULL){};
public:
	CManagedDllLoader(LPCTSTR lpszDllName);
	virtual ~CManagedDllLoader(void);
};

class TB_EXPORT CAbstractCtrl
{
public:
	CAbstractCtrl()  {}

	virtual CString		GetPublicName() = 0;
	virtual CString		GetCtrlClass() = 0;
	virtual CString		GetCtrlName() = 0;
	virtual DataObj*	GetCtrlData() = 0;

	virtual BOOL		IsAutomaticExpression() const = 0;
	virtual CString		GetAutomaticExpression() const = 0;
	virtual BOOL		SetAutomaticExpression(const CString& strExpr) = 0;

	virtual BOOL		IsMultiValue() const									 { return FALSE; }
	virtual int			GetRowNumber() const									 { return 0; }
	virtual DataObj*	GetCtrlData(const CString& /*sName*/, int /*nRow*/ = 0) { return NULL; }
	virtual int			EnumColumnName(CStringArray& /*arNames*/, BOOL /*bAll*/ = TRUE) { return 0; }
	virtual void		FindHotLink() {}
};


/////////////////////////////////////////////////////////////////////////////
//			Gestione della performance
/////////////////////////////////////////////////////////////////////////////
//
enum TimeOperation { START_TIME, STOP_TIME, PAUSE_TIME, RESUME_TIME };

//===========================================================================
class TB_EXPORT CPerformanceCrono
{
private:
	DWORD	m_dStartTick;
	DWORD	m_dStopTick;
	DWORD	m_dElapsedTick;
	BOOL	m_bRunning;


public:
	CPerformanceCrono();

public:
	void  Start();
	void  Stop();
	void  Pause();
	void  Resume();

	void  ClearTimes();

	DWORD GetElapsedTime()	const { return m_dElapsedTick; }
	DWORD GetStartTick()	const { return m_dStartTick; }
	DWORD GetStopTick()		const { return m_dStopTick; }


public: //operator
	CPerformanceCrono&	operator = (const CPerformanceCrono&);
};

//===========================================================================
class TB_EXPORT CCounterElem : public CObject
{
public:
	int		m_nOperation; //x ottimizzare uso il numero dell'operazione x accedere direttamente sull'array 
						  //(se non é lo stesso elemento allora effetto lo scorrimento)
	CString m_strTitle;
	DWORD	m_dTotalTime;
	long	m_lCount;

private:
	CPerformanceCrono m_aCrono;

public:
	CCounterElem(int, LPCTSTR);

public:
	void  Start			() { m_aCrono.Start(); }
	void  Stop			() { m_aCrono.Stop();  m_dTotalTime += m_aCrono.GetElapsedTime(); }
	void  Pause			() { m_aCrono.Pause(); }
	void  Resume		() { m_aCrono.Resume(); }
	DWORD GetElapsedTime() const { return m_aCrono.GetElapsedTime(); }
	CString	GetFormattedTime();

	void Clear();
};

//===========================================================================
class TB_EXPORT CCounterArray : public Array
{
	DECLARE_DYNAMIC(CCounterArray)

public:
	CCounterElem*	GetAt(int nIndex) const { return (CCounterElem*)CObArray::GetAt(nIndex); }
	CCounterElem*&	ElementAt(int nIndex) { return (CCounterElem*&)CObArray::ElementAt(nIndex); }

	CCounterElem*	operator[]	(int nIndex) const { return GetAt(nIndex); }
	CCounterElem*&	operator[]	(int nIndex) { return ElementAt(nIndex); }

public:
	void ClearCounters();
	void Pause();
	void Resume();

	// chiamati dalla dialog
	CString GetTitleAt(int) const;
	DWORD	MakeTotalTime() const;

	virtual CString	GetFormattedTimeAt	(int) const;
	// diagnostics
#ifdef _DEBUG
public:
	void Dump(CDumpContext& dc) const { ASSERT_VALID(this); AFX_DUMP0(dc, " CCounterArray\n"); }
	void AssertValid() const { CObArray::AssertValid(); }
#endif //_DEBUG
};

//===========================================================================
class TB_EXPORT PerformanceManager
{
protected:
	CPerformanceCrono	m_aTotalCrono;  // to estimate the time of the entire document transaction
	BOOL				m_bStartTime;

private:
	CCounterArray		m_aCounters;

public:
	PerformanceManager();

protected:
	virtual void InitCounters();

	void MakeTimeOperation(CCounterElem* pCounterElem, int eTime);
	void MakeTimeOperation(int nCounter, int eTime);

public:
	CCounterElem* AddCounter(CString sName);
	CCounterArray& GetCounters();
};

#include "endh.dex"