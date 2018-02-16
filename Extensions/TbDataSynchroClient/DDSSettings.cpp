#include "stdafx.h"

#include <TbGeneric\ParametersSections.h>

#include "DDSSettings.h"
#include "UIDSSettings.hjson"

const TCHAR szTbDataSynchroClient[] = _T("Module.Extensions.TbDataSynchroClient");
const CTBNamespace snsTbDataSynchroClient = szTbDataSynchroClient;
const TCHAR szDataSynchronizerMonitor[] = _T("DataSynchronizerMonitor");
const TCHAR szRefreshTimer[] = _T("RefreshTimer");
const TCHAR szWebMonitorUrl[] = _T("WebMonitorUrl");


/////////////////////////////////////////////////////////////////////////////
//							DDSSettings 
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(DDSSettings, DCompanyUserSettingsDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DDSSettings, DCompanyUserSettingsDoc)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
DDSSettings::DDSSettings()
	:
	DCompanyUserSettingsDoc()
{
}

//-----------------------------------------------------------------------------
BOOL DDSSettings::OnAttachData()
{
	if (!__super::OnAttachData())
		return FALSE;

	// json
	DeclareRegisterJson();

	return TRUE;
}

//-----------------------------------------------------------------------------
void DDSSettings::DeclareRegisterJson()
{
	DECLARE_VAR_JSON(RefreshTimer);
	DECLARE_VAR_JSON(WebMonitorUrl);
}

//-----------------------------------------------------------------------------
BOOL DDSSettings::GetSaveSettings(BOOL bSave)
{
	if (bSave)
	{
		//DataSynchronizerMonitor		
		AfxSetSettingValue(snsTbDataSynchroClient, szDataSynchronizerMonitor, szRefreshTimer, m_RefreshTimer, szTbDefaultSettingFileName);
		AfxSetSettingValue(snsTbDataSynchroClient, szDataSynchronizerMonitor, szWebMonitorUrl, m_WebMonitorUrl, szTbDefaultSettingFileName);
		
		CCustomSaveInterface aCustomSaveInterface;
		aCustomSaveInterface.m_bSaveAllFile = TRUE;
		aCustomSaveInterface.m_bSaveAllUsers = TRUE;
		aCustomSaveInterface.m_eSaveMode = CCustomSaveInterface::COMPANY_USERS;
		return AfxSaveSettingsFile(snsTbDataSynchroClient, szTbDefaultSettingFileName, TRUE, &aCustomSaveInterface);
	}
	else	
	{
		//DataSynchronizerMonitor
		m_RefreshTimer = *(DataInt*)AfxGetSettingValue(snsTbDataSynchroClient, szDataSynchronizerMonitor, szRefreshTimer, DataInt(3), szTbDefaultSettingFileName);
		m_WebMonitorUrl = *(DataStr*)AfxGetSettingValue(snsTbDataSynchroClient, szDataSynchronizerMonitor, szWebMonitorUrl, DataStr(_T("http://localhost:5015/login")), szTbDefaultSettingFileName);
	
	}
	return TRUE;
}

