#pragma once

#include <TbGenlib\DiagnosticManager.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlObject;
class SqlConnection;
class SqlRecoveryManagerDialog;
class CBaseDocument;

// utility statics
//===========================================================================
static const int nProgressBarRetriesMult = 3;

// Settings common related to Recovery Manager
//============================================================================
class TB_EXPORT CRecoveryManagerSettings : public CObject
{
	DECLARE_DYNCREATE(CRecoveryManagerSettings)

private:
	BOOL	m_bEnabled;
	int		m_nRetries;
	long	m_lRetriesIntervalMms;

public:
	CRecoveryManagerSettings ();

private:
	void InitDefaultSettings ();

public:
	// get
	const int&	GetRetries				() const;
	const long&	GetRetriesIntervalMms	() const;
	const long	IsEnabled				() const;
};

//=============================================================================
class TB_EXPORT SqlRecoveryManagerDiagnostic : public CDiagnosticManagerWriter
{
	friend class DocRecoveryManager;
	friend class SqlRecoveryManager;

	DECLARE_DYNAMIC(SqlRecoveryManagerDiagnostic)

private:
	BOOL	m_bSessionStarted;
	CString	m_sOriginalError;
	BOOL	m_bSnapShotSaved;
	BOOL	m_bSnapShotPlayed;
	int		m_nCurrPhase;


public:
	SqlRecoveryManagerDiagnostic ();

public:
	BOOL	StartSession		();
	void	SetOriginalError	(const CString& sError);
	CString	GetOriginalError	() const;
	CString GetNetworkAdapters	();
	CString GetLogFileName		(BOOL bOnClient = FALSE);
	
	BOOL	IsSnapshotsSaved	();
	BOOL	IsSnapshotsPlayed	();

	int		GetPhase			() const;
	void	StartPhase			(const int& nPhase);


	void AddError	(const CString& sMessage, Array* pExtInfos /*NULL*/);
	void AddWarning	(const CString& sMessage, Array* pExtInfos /*NULL*/);
	void AddInfo	(const CString& sMessage, Array* pExtInfos /*NULL*/);
	void AddMessage (const CString& sMessage, const MessageType& eType, Array* pExtInfos = NULL);
};

// current state if the recovery operation
//=============================================================================
class TB_EXPORT SqlRecoveryManagerState : public CObject
{
	friend class SqlRecoveryManager;
	friend class DocRecoveryManager;
	
private:
	enum State	{ OnIdle, Alerted, Recovering, Aborted, Recovered, NotRecovered };

	CString							m_sInitConn;
	CString							m_sInitConnDisplayable;
	State							m_eState;
	SqlRecoveryManagerDiagnostic	m_Diagnostic;
	BOOL							m_bApplicationFreezed;

public:
	SqlRecoveryManagerState ();


private:
	void	Init		();
	State	GetState	() const;
	int		GetPhase	() const;

	// status and error management
	void	StartPhase			(const int& nPhase);
	void	SetState			(State eState);
	void	FreezeApplication	();
	void	UnFreezeApplication	();
	BOOL	IsApplicationFrozen	() const;
	CString	GetOriginalError	() const;
};

// object to perform recovery operation
//=============================================================================
class TB_EXPORT SqlRecoveryManager : public CObject, public CTBLockable
{
	friend class DocRecoveryManager;

private:
	CRecoveryManagerSettings	m_Settings;
	SqlRecoveryManagerState		m_CurrState;
	SqlRecoveryManagerDialog*	m_pUI;

public:
	SqlRecoveryManager (CBaseDocument* pDocument = NULL);

public:
	BOOL IsApplicationFrozen	() const;

	// return TRUE value if the event is managed correctly, otherwise FALSE
	BOOL ON_CONNECTION_LOST	(CString sOriginalError);

private:
	void InitializeRecoveryState ();

	// recovery methods
	BOOL RecoveryConnections();
	BOOL CloseOldObjects	(SqlConnection* pConnection);
	BOOL RecoveryConnection	(SqlConnection* pConnection);
	BOOL RecoverySessions	(SqlConnection* pConnection);

	// errors management
	void AddError			(const CString& sMessage, Array* pExtInfos = NULL);
	void AddWarning			(const CString& sMessage, Array* pExtInfos = NULL);
	void AddInfo			(const CString& sMessage, Array* pExtInfos = NULL);

	// user feedback
	void ShowRecoveryUI		();
	void SetMaxProgressBarUI(const int& nSteps);
	void UpdateRecoveryUI	(LPCTSTR szMessage, CDiagnosticManagerWriter::MessageType eType, const int nSteps = 1);
	void DestroyRecoveryUI	();
	int	 AskToUser			(LPCTSTR szExplanation, LPCTSTR szQuestion);
	int	 SayToUser			(LPCTSTR szExplanation, LPCTSTR szQuestion);
	void ShowResultsToUser	();

public:
	//for lock tracer
	virtual LPCSTR	GetObjectName() const { return "SqlRecoveryManager"; }

protected:
	virtual BOOL OnPerformRecoveryActivity	() = 0;
};

//=============================================================================
TB_EXPORT SqlRecoveryManager* AFXAPI AfxGetSqlRecoveryManager ();

#include "endh.dex"
