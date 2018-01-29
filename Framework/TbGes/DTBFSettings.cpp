#include "stdafx.h"

//NOW INCLUDED IN COMMON PCH: #include <TbGeneric\GeneralFunctions.h>
//NOW INCLUDED IN COMMON PCH: #include <TbGenlib\LocalizableObjs.h>
//#include <TBApplication\Dbl\SalesSettings.h>
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
	DECLARE_VAR_JSON(DebugSqlTrace);
	DECLARE_VAR_JSON(DebugSqlTraceActions);
	DECLARE_VAR_JSON(DebugSqlTraceTables);
	DECLARE_VAR_JSON(EnableEventViewerLog);
}

//-----------------------------------------------------------------------------
BOOL DTBFSettings::GetSaveSettings(BOOL bSave)
{
	BOOL bOk = TRUE;
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
	
	if (bSave)
		bOk = AfxSaveSettingsFile(snsTbOleDb, szTbDefaultSettingFileName, TRUE, NULL);

	return bOk;
}
