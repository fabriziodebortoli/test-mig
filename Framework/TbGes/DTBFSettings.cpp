#include "stdafx.h"

#include <TbGeneric\ParametersSections.h>

#include "DTBFSettings.h"
#include "UITBFSettings.hjson"

/////////////////////////////////////////////////////////////////////////////
//							DTBFSettings 
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(DTBFSettings, DCompanyUserSettingsDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DTBFSettings, DCompanyUserSettingsDoc)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
DTBFSettings::DTBFSettings()
	:
	DCompanyUserSettingsDoc()
{
}

//-----------------------------------------------------------------------------
BOOL DTBFSettings::OnAttachData()
{
	if (!__super::OnAttachData())
		return FALSE;

	// json
	DeclareRegisterJson();

	return TRUE;
}

//-----------------------------------------------------------------------------
void DTBFSettings::DeclareRegisterJson()
{
	DECLARE_VAR_JSON(EnableAssertionsInRelease);
	DECLARE_VAR_JSON(DumpAssertionsIfNoCrash);
	DECLARE_VAR_JSON(LogDiagnosticInEventViewerUnattendedMode);
	//Forms
	DECLARE_VAR_JSON(ImmediateBrowsing);
	DECLARE_VAR_JSON(DisplayBrowsingLimits);
	DECLARE_VAR_JSON(HotlinkComboDefaultFields);
	DECLARE_VAR_JSON(MaxComboBoxItems);
	DECLARE_VAR_JSON(ShowZerosinInput);
	DECLARE_VAR_JSON(AllowBodyeditColumnHeaderSmallFont);
	DECLARE_VAR_JSON(AddBodyeditColumnHeaderExtraSpace);
	DECLARE_VAR_JSON(DataTipDelay);
	DECLARE_VAR_JSON(DataTipLevelOnBodyedit);
	DECLARE_VAR_JSON(DataTipMaxWidth);
	DECLARE_VAR_JSON(DataTipMaxHeight);
	DECLARE_VAR_JSON(EnableCenterControls);
	//Preference
	DECLARE_VAR_JSON(ShowAdminCustomSaveDialog);
	DECLARE_VAR_JSON(UseWoormRadar);
	DECLARE_VAR_JSON(RepeatableNew);
	DECLARE_VAR_JSON(UseEasyBrowsing);
	DECLARE_VAR_JSON(EnableFindOnSlaveFields);
	DECLARE_VAR_JSON(TBLoaderDefaultSOAPPort);
	//DataTypeEpsilons
	DECLARE_VAR_JSON(DoubleDecimals);
	DECLARE_VAR_JSON(MonetaryDecimals);
	DECLARE_VAR_JSON(PercentageDecimals);
	DECLARE_VAR_JSON(QuantityDecimals);
	//Report
	DECLARE_VAR_JSON(ShowPrintSetup);
	DECLARE_VAR_JSON(UpdateDefaultReport);
	DECLARE_VAR_JSON(UseMultithreading);
	//Culture
	DECLARE_VAR_JSON(ExcelDateFormat);
	DECLARE_VAR_JSON(ExcelTimeFormat);
	DECLARE_VAR_JSON(ExcelDateTimeFormat);
	//Scheduler
	DECLARE_VAR_JSON(TaskIsolation);
	//TBOLEDB
	//LockManager
	DECLARE_VAR_JSON(DisableLockRetry);
	DECLARE_VAR_JSON(DisableBeep);
	DECLARE_VAR_JSON(MaxLockRetry);
	DECLARE_VAR_JSON(MaxLockTime);
	DECLARE_VAR_JSON(DisableBatchLockRetry);
	DECLARE_VAR_JSON(DisableBatchBeep);
	DECLARE_VAR_JSON(MaxBatchLockRetry);
	DECLARE_VAR_JSON(MaxBatchLockTime);
	DECLARE_VAR_JSON(MaxReportLockRetry);
	DECLARE_VAR_JSON(MaxReportLockTime);
	//Trace Sql instructions
	DECLARE_VAR_JSON(DebugSqlTrace);
	DECLARE_VAR_JSON(DebugSqlTraceActions);
	DECLARE_VAR_JSON(DebugSqlTraceTables);
	DECLARE_VAR_JSON(EnableEventViewerLog);
}

//-----------------------------------------------------------------------------
BOOL DTBFSettings::GetSaveTBGenlibSettings(BOOL bSave)
{
	if (bSave)
	{
		//Environment
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szEnableAssertionsInRelease, m_EnableAssertionsInRelease, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szDumpAssertionsIfNoCrash, m_DumpAssertionsIfNoCrash, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szLogDiagnosticInEventViewerUnattendedMode, m_LogDiagnosticInEventViewerUnattendedMode, szTbDefaultSettingFileName);
		//Forms
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szImmediateBrowsing, m_ImmediateBrowsing, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szDisplayBrowsingLimits, m_DisplayBrowsingLimits, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szHotlinkComboDefaultFields, m_HotlinkComboDefaultFields, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szMaxComboBoxItems, m_MaxComboBoxItems, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szShowZerosInInput, m_ShowZerosinInput, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szAllowBodyeditColumnHeaderSmallFont, m_AllowBodyeditColumnHeaderSmallFont, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szAddBodyeditColumnHeaderExtraSpace, m_AddBodyeditColumnHeaderExtraSpace, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szDataTipDelay, m_DataTipDelay, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szDataTipLevelOnBodyedit, m_DataTipLevelOnBodyedit, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szDataTipMaxWidth, m_DataTipMaxWidth, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szDataTipMaxHeight, m_DataTipMaxHeight, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szEnvironment, szEnableCenterControls, m_EnableCenterControls, szTbDefaultSettingFileName);
		//Preference
		AfxSetSettingValue(snsTbGenlib, szPreferenceSection, szShowAdminCustomSaveDialog, m_ShowAdminCustomSaveDialog, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szPreferenceSection, szUseWoormRadar, m_UseWoormRadar, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szPreferenceSection, szRepeatableNew, m_RepeatableNew, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szPreferenceSection, szUseEasyBrowsing, m_UseEasyBrowsing, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szPreferenceSection, szEnableFindOnSlaveFields, m_EnableFindOnSlaveFields, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szPreferenceSection, szTBLoaderDefaultSOAPPort, m_TBLoaderDefaultSOAPPort, szTbDefaultSettingFileName);

		//DataTypeEpsilons
		AfxSetSettingValue(snsTbGenlib, szDataTypeEpsilonSection, szDoubleDecimals, m_DoubleDecimals, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szDataTypeEpsilonSection, szMonetaryDecimals, m_MonetaryDecimals, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szDataTypeEpsilonSection, szPercentageDecimals, m_PercentageDecimals, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szDataTypeEpsilonSection, szQuantityDecimals, m_QuantityDecimals, szTbDefaultSettingFileName);
		//Report
		AfxSetSettingValue(snsTbGenlib, szReportSection, szShowPrintSetup, m_ShowPrintSetup, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szReportSection, szUpdateDefaultReport, m_UpdateDefaultReport, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szReportSection, szUseMultithreading, m_UseMultithreading, szTbDefaultSettingFileName);
		//Culture
		AfxSetSettingValue(snsTbGenlib, szCultureSection, szExcelDateFormat, m_ExcelDateFormat, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szCultureSection, szExcelTimeFormat, m_ExcelTimeFormat, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbGenlib, szCultureSection, szExcelDateTimeFormat, m_ExcelDateTimeFormat, szTbDefaultSettingFileName);
		//Culture
		AfxSetSettingValue(snsTbGenlib, szSchedulerSection, szTaskIsolation, m_TaskIsolation, szTbDefaultSettingFileName);

		CCustomSaveInterface aCustomSaveInterface;
		aCustomSaveInterface.m_bSaveAllFile = TRUE;
		aCustomSaveInterface.m_bSaveAllUsers = TRUE;
		aCustomSaveInterface.m_eSaveMode = CCustomSaveInterface::COMPANY_USERS;
		return AfxSaveSettingsFile(snsTbGenlib, szTbDefaultSettingFileName, TRUE, &aCustomSaveInterface);
	}
	else	
	{
		//Environment
		m_EnableAssertionsInRelease = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szEnableAssertionsInRelease, DataBool(FALSE), szTbDefaultSettingFileName);
		m_DumpAssertionsIfNoCrash = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szDumpAssertionsIfNoCrash, DataBool(FALSE), szTbDefaultSettingFileName);
		m_LogDiagnosticInEventViewerUnattendedMode = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szLogDiagnosticInEventViewerUnattendedMode, DataBool(FALSE), szTbDefaultSettingFileName);
		//Forms
		m_ImmediateBrowsing = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szImmediateBrowsing, DataBool(FALSE), szTbDefaultSettingFileName);
		m_DisplayBrowsingLimits = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szDisplayBrowsingLimits, DataBool(FALSE), szTbDefaultSettingFileName);
		m_HotlinkComboDefaultFields = *(DataStr*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szHotlinkComboDefaultFields, DataStr(_T("@dbfield,Description, CompanyName")), szTbDefaultSettingFileName);
		m_MaxComboBoxItems = *(DataLng*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szMaxComboBoxItems, DataLng(300), szTbDefaultSettingFileName);
		m_ShowZerosinInput = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szShowZerosInInput, DataBool(TRUE), szTbDefaultSettingFileName);
		m_AllowBodyeditColumnHeaderSmallFont = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szAllowBodyeditColumnHeaderSmallFont, DataInt(3), szTbDefaultSettingFileName);
		m_AddBodyeditColumnHeaderExtraSpace = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szAddBodyeditColumnHeaderExtraSpace, DataBool(TRUE), szTbDefaultSettingFileName);
		m_DataTipDelay = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szDataTipDelay, DataInt(300), szTbDefaultSettingFileName);
		m_DataTipLevelOnBodyedit = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szDataTipLevelOnBodyedit, DataInt(63), szTbDefaultSettingFileName);
		m_DataTipMaxWidth = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szDataTipMaxWidth, DataInt(800), szTbDefaultSettingFileName);
		m_DataTipMaxHeight = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szDataTipMaxHeight, DataInt(600), szTbDefaultSettingFileName);
		m_EnableCenterControls = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szEnvironment, szEnableCenterControls, DataBool(TRUE), szTbDefaultSettingFileName);
		//Preference
		m_ShowAdminCustomSaveDialog = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szShowAdminCustomSaveDialog, DataBool(TRUE), szTbDefaultSettingFileName);
		m_UseWoormRadar = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szUseWoormRadar, DataBool(FALSE), szTbDefaultSettingFileName);
		m_RepeatableNew = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szRepeatableNew, DataBool(FALSE), szTbDefaultSettingFileName);
		m_UseEasyBrowsing = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szUseEasyBrowsing, DataBool(FALSE), szTbDefaultSettingFileName);
		m_EnableFindOnSlaveFields = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szEnableFindOnSlaveFields, DataBool(TRUE), szTbDefaultSettingFileName);
		m_TBLoaderDefaultSOAPPort = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szPreferenceSection, szTBLoaderDefaultSOAPPort, DataInt(10000), szTbDefaultSettingFileName);
		//DataTypeEpsilons
		m_DoubleDecimals = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szDataTypeEpsilonSection, szDoubleDecimals, DataInt(7), szTbDefaultSettingFileName);
		m_MonetaryDecimals = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szDataTypeEpsilonSection, szMonetaryDecimals, DataInt(7), szTbDefaultSettingFileName);
		m_PercentageDecimals = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szDataTypeEpsilonSection, szPercentageDecimals, DataInt(7), szTbDefaultSettingFileName);
		m_QuantityDecimals = *(DataInt*)AfxGetSettingValue(snsTbGenlib, szDataTypeEpsilonSection, szQuantityDecimals, DataInt(7), szTbDefaultSettingFileName);		
		//Report
		m_ShowPrintSetup = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szReportSection, szShowPrintSetup, DataBool(FALSE), szTbDefaultSettingFileName);
		m_UpdateDefaultReport = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szReportSection, szUpdateDefaultReport, DataBool(TRUE), szTbDefaultSettingFileName);
		m_UseMultithreading = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szReportSection, szUseMultithreading, DataBool(TRUE), szTbDefaultSettingFileName);
			//Culture
		m_ExcelDateFormat = *(DataStr*)AfxGetSettingValue(snsTbGenlib, szCultureSection, szExcelDateFormat, DataStr(_T("MM/dd/yyyy")), szTbDefaultSettingFileName);
		m_ExcelTimeFormat = *(DataStr*)AfxGetSettingValue(snsTbGenlib, szCultureSection, szExcelTimeFormat, DataStr(_T("hh:mm")), szTbDefaultSettingFileName);
		m_ExcelDateTimeFormat = *(DataStr*)AfxGetSettingValue(snsTbGenlib, szCultureSection, szExcelDateTimeFormat, DataStr(_T("M/d/yyyy hh:mm;@")), szTbDefaultSettingFileName);
		//Culture
		m_TaskIsolation = *(DataBool*)AfxGetSettingValue(snsTbGenlib, szCultureSection, szTaskIsolation, DataBool(FALSE), szTbDefaultSettingFileName);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DTBFSettings::GetSaveTBOleDBSettings(BOOL bSave)
{
	if (bSave)
	{
		//LockManager
		AfxSetSettingValue(snsTbOleDb, szLockManager, szDisableLockRetry, m_DisableLockRetry, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szDisableBeep, m_DisableBeep, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szMaxLockRetry, m_MaxLockRetry, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szMaxLockTime, m_MaxLockTime, szTbDefaultSettingFileName);

		AfxSetSettingValue(snsTbOleDb, szLockManager, szDisableBatchLockRetry, m_DisableBatchLockRetry, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szDisableBatchBeep, m_DisableBatchBeep, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szMaxBatchLockRetry, m_MaxBatchLockRetry, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szMaxBatchLockTime, m_MaxBatchLockTime, szTbDefaultSettingFileName);

		AfxSetSettingValue(snsTbOleDb, szLockManager, szMaxReportLockRetry, m_MaxReportLockRetry, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szMaxReportLockTime, m_MaxReportLockTime, szTbDefaultSettingFileName);

		//Connection
		AfxSetSettingValue(snsTbOleDb, szLockManager, szDebugSqlTrace, m_DebugSqlTrace, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szDebugSqlTraceActions, m_DebugSqlTraceActions, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szDebugSqlTraceTables, m_DebugSqlTraceTables, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbOleDb, szLockManager, szEnableEventViewerLog, m_EnableEventViewerLog, szTbDefaultSettingFileName);

		CCustomSaveInterface aCustomSaveInterface;
		aCustomSaveInterface.m_bSaveAllFile = TRUE;
		aCustomSaveInterface.m_bSaveAllUsers = TRUE;
		aCustomSaveInterface.m_eSaveMode = CCustomSaveInterface::COMPANY_USERS;
		return AfxSaveSettingsFile(snsTbOleDb, szTbDefaultSettingFileName, TRUE, &aCustomSaveInterface);
	}
	else
	{
		//LockManager
		m_DisableLockRetry = *(DataBool*)AfxGetSettingValue(snsTbOleDb, szLockManager, szDisableLockRetry, DataBool(FALSE), szTbDefaultSettingFileName);
		m_DisableBeep = *(DataBool*)AfxGetSettingValue(snsTbOleDb, szLockManager, szDisableBeep, DataBool(FALSE), szTbDefaultSettingFileName);
		m_MaxLockRetry = *(DataInt*)AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxLockRetry, DataInt(2), szTbDefaultSettingFileName);
		m_MaxLockTime = *(DataInt*)AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxLockTime, DataInt(2000), szTbDefaultSettingFileName);

		m_DisableBatchLockRetry = *(DataBool*)AfxGetSettingValue(snsTbOleDb, szLockManager, szDisableBatchLockRetry, DataBool(FALSE), szTbDefaultSettingFileName);
		m_DisableBatchBeep = *(DataBool*)AfxGetSettingValue(snsTbOleDb, szLockManager, szDisableBatchBeep, DataBool(FALSE), szTbDefaultSettingFileName);
		m_MaxBatchLockRetry = *(DataInt*)AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxBatchLockRetry, DataInt(8), szTbDefaultSettingFileName);
		m_MaxBatchLockTime = *(DataInt*)AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxBatchLockTime, DataInt(3000), szTbDefaultSettingFileName);

		m_MaxReportLockRetry = *(DataInt*)AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxReportLockRetry, DataInt(10), szTbDefaultSettingFileName);
		m_MaxReportLockTime = *(DataInt*)AfxGetSettingValue(snsTbOleDb, szLockManager, szMaxReportLockTime, DataInt(5000), szTbDefaultSettingFileName);

		//Connection
		m_DebugSqlTrace = *(DataBool*)AfxGetSettingValue(snsTbOleDb, szLockManager, szDebugSqlTrace, DataBool(FALSE), szTbDefaultSettingFileName);
		m_DebugSqlTraceActions = *(DataStr*)AfxGetSettingValue(snsTbOleDb, szLockManager, szDebugSqlTraceActions, DataStr(), szTbDefaultSettingFileName);
		m_DebugSqlTraceTables = *(DataStr*)AfxGetSettingValue(snsTbOleDb, szLockManager, szDebugSqlTraceTables, DataStr(), szTbDefaultSettingFileName);
		m_EnableEventViewerLog = *(DataBool*)AfxGetSettingValue(snsTbOleDb, szLockManager, szEnableEventViewerLog, DataBool(FALSE), szTbDefaultSettingFileName);
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL DTBFSettings::GetSaveSettings(BOOL bSave)
{
	BOOL bOKSaveTBOleDB = GetSaveTBOleDBSettings(bSave);
	BOOL bOKSaveTBGenlib = GetSaveTBGenlibSettings(bSave);
	
	return (bSave) ? bOKSaveTBOleDB && bOKSaveTBGenlib : TRUE;
}
