#pragma once
#include <TbGes\DParametersDoc.h>
#include <TbGenlibUI\SettingsTableManager.h>
#include "beginh.dex"


/////////////////////////////////////////////////////////////////////////////
//							DTBFSettings 
/////////////////////////////////////////////////////////////////////////////
class DTBFSettings : public DCompanyUserSettingsDoc
{
	DECLARE_DYNCREATE(DTBFSettings)

	// Construction
public:
	DTBFSettings();


public:
	DECLARE_SETTING(Bool, DisableLockRetry);
	DECLARE_SETTING(Bool, DisableBeep);
	DECLARE_SETTING(Int, MaxLockRetry);
	DECLARE_SETTING(Int, MaxLockTime);
	DECLARE_SETTING(Bool, DisableBatchLockRetry);
	DECLARE_SETTING(Bool, DisableBatchBeep);
	DECLARE_SETTING(Int, MaxBatchLockRetry);
	DECLARE_SETTING(Int, MaxBatchLockTime);
	DECLARE_SETTING(Int, MaxReportLockRetry);
	DECLARE_SETTING(Int, MaxReportLockTime);
	DECLARE_SETTING(Bool, DebugSqlTrace);
	DECLARE_SETTING(Str, DebugSqlTraceActions);
	DECLARE_SETTING(Str, DebugSqlTraceTables);
	DECLARE_SETTING(Bool, EnableEventViewerLog);

protected:
	virtual BOOL	OnAttachData();
	virtual BOOL	GetSaveSettings(BOOL bSave = FALSE);
	// json
	void	DeclareRegisterJson();


protected:
	//{{AFX_MSG(DTBFAccountingSettings)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

#include "endh.dex"
