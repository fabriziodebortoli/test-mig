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
	//Setting TBGENLIB
	//Environment
	DECLARE_SETTING(Bool, EnableAssertionsInRelease);
	DECLARE_SETTING(Bool, DumpAssertionsIfNoCrash);
	DECLARE_SETTING(Bool, LogDiagnosticInEventViewerUnattendedMode);
	//Forms
	DECLARE_SETTING(Bool, ImmediateBrowsing);
	DECLARE_SETTING(Bool, DisplayBrowsingLimits);
	DECLARE_SETTING(Str, HotlinkComboDefaultFields);
	DECLARE_SETTING(Lng, MaxComboBoxItems);
	DECLARE_SETTING(Bool, ShowZerosinInput);
	DECLARE_SETTING(Int, AllowBodyeditColumnHeaderSmallFont);
	DECLARE_SETTING(Bool, AddBodyeditColumnHeaderExtraSpace);
	DECLARE_SETTING(Int, DataTipDelay);
	DECLARE_SETTING(Int, DataTipLevelOnBodyedit);
	DECLARE_SETTING(Int, DataTipMaxWidth);
	DECLARE_SETTING(Int, DataTipMaxHeight);
	DECLARE_SETTING(Bool, EnableCenterControls);
	//Preference
	DECLARE_SETTING(Bool, ShowAdminCustomSaveDialog);
	DECLARE_SETTING(Bool, UseWoormRadar);
	DECLARE_SETTING(Bool, RepeatableNew);
	DECLARE_SETTING(Bool, UseEasyBrowsing);
	DECLARE_SETTING(Bool, EnableFindOnSlaveFields);
	//DataTypeEpsilons
	DECLARE_SETTING(Int, DoubleDecimals);
	DECLARE_SETTING(Int, MonetaryDecimals);
	DECLARE_SETTING(Int, PercentageDecimals);
	DECLARE_SETTING(Int, QuantityDecimals);
	//Report
	DECLARE_SETTING(Str,  BarCodeType);
	DECLARE_SETTING(Bool, ShowPrintSetup);
	DECLARE_SETTING(Bool, UpdateDefaultReport);
	DECLARE_SETTING(Bool, UseMultithreading);
	//Development
	DECLARE_SETTING(Int, TBLoaderDefaultSOAPPort);
	//Culture
	DECLARE_SETTING(Str, ExcelDateFormat);
	DECLARE_SETTING(Str, ExcelTimeFormat);
	DECLARE_SETTING(Str, ExcelDateTimeFormat);
	//Scheduler
	DECLARE_SETTING(Bool, TaskIsolation);
	//Setting TBOLEDB
	//LockManager
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
	//Connection
	DECLARE_SETTING(Bool, DebugSqlTrace);
	DECLARE_SETTING(Str, DebugSqlTraceActions);
	DECLARE_SETTING(Str, DebugSqlTraceTables);
	DECLARE_SETTING(Bool, EnableEventViewerLog);

private:
	BOOL	GetSaveTBGenlibSettings(BOOL bSave = FALSE);
	BOOL	GetSaveTBOleDBSettings(BOOL bSave = FALSE);

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
