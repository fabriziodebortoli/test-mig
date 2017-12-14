
#pragma once

#include <TbNameSolver\TBWinThread.h>

#include <TbGeneric\FunctionCall.h>
#include <TbGeneric\Schedule.h>

#include "beginh.dex"

class CAbstractCtrl;

///////////////////////////////////////////////////////////////////////////////

class TB_EXPORT CExternalControllerInfo : public CObject
{
	DECLARE_DYNAMIC(CExternalControllerInfo)

private:
	DataObjArray	m_arLockedValues;
	Scheduler		m_Scheduler;
	
public:

	enum ControllingMode 
	{ 
		NONE, EDITING, RUNNING
	};
	
	enum RunningTaskStatus 
	{ 
		TASK_NO_RUN, TASK_SUCCESS, TASK_USER_ABORT,
		TASK_CLOSING, TASK_SAVE_PARAMS, TASK_CANCEL_PARAMS,
		TASK_SUCCESS_WITH_INFO, TASK_FAILED, TASK_SET_ASKDLGS_ENTRIES, 
		TASK_REPORT_RUN_ENDED, TASK_REPORT_PRINT_ENDED
	};

	CTBEvent				m_Finished;	// indica che è stato terminato l'inserimento delle informazioni
	RunningTaskStatus		m_RunningStatus;// stato del task che sta runnando
	CFunctionDescription	m_Data;			// dati del task
	ControllingMode			m_ControllingMode;// indica se il documento e` stato runnato dal controllore esterno
	BOOL					m_bEditAndRun;
	DataStr					m_code;

	CExternalControllerInfo(void);
	~CExternalControllerInfo(void);

	BOOL IsRunning()	{ return m_ControllingMode == RUNNING; }
	BOOL IsEditing()	{ return m_ControllingMode == EDITING; }
	BOOL IsControlling(){ return m_ControllingMode != NONE; }

	void UnlockValues();
	void SetRunningTaskStatus (RunningTaskStatus status) { m_RunningStatus = status; }

	// Valorizza il control
	BOOL ValorizeControlData(CAbstractCtrl* pCtrl);
	
	// Recupera il valore dal control
	void RetrieveControlData(CAbstractCtrl* pCtrl);
	
	void ClearData();
	
	CString GetXmlString();
	void SetFromXmlString(const CString& strXml);

	void WaitUntilFinished();

private:
	BOOL	IsAutoExpression(CAbstractCtrl* pCtrl);

	void ValorizeControlData(const DataObj* pControlData, DataObj *pValue);
	void RetrieveControlData(const CString &strName, DataObj *pValue);
	void LockValue(DataObj *pValue);
};

#include "endh.dex"
