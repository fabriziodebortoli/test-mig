#include "stdafx.h"
#include "TbDataSynchroClientFunctions.h"
#include <TBNameSolver\LoginContext.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbGeneric\SettingsTable.h>

const TCHAR szTbDataSynchroClient[] = _T("Module.Extensions.TbDataSynchroClient");
const CTBNamespace snsTbDataSynchroClient = szTbDataSynchroClient;
const TCHAR szDataSynchronizerMonitor[] = _T("DataSynchronizerMonitor");
const TCHAR szWebMonitorUrl[] = _T("WebMonitorUrl");

//----------------------------------------------------------------------------
///<summary>
///Allow to open the Monitor Web
///</summary>
//[TBWebMethod(securityhidden=true, woorm_method=false)]
void OpenWebSynchroMonitor()
{
	DataStr webMonitorUrl = (*((DataStr*)AfxGetSettingValue(snsTbDataSynchroClient, szDataSynchronizerMonitor, szWebMonitorUrl, DataStr())));
	CString sTitle(_TB("Synchronization Web Monitor"));
	CString sLanguage = AfxGetLoginInfos()->m_strPreferredLanguage;
    DataInt nCompanyId = AfxGetLoginInfos()->m_nCompanyId;
	DataInt nLoginId = AfxGetLoginInfos()->m_nLoginId;
	CString SAuthToken = AfxGetAuthenticationToken();

	/*
	DataStr lan = _TB("/en");
	if (sLanguage.CompareNoCase(_TB("it")) == 0)
		lan = _TB("/it");
	DataStr Url = webMonitorUrl.Str() + lan.Str();
	*/

	DataStr Url = webMonitorUrl.Str() + _T("?userid=") + nLoginId.Str() + _T("&companyid=") + nCompanyId.Str() + _T("&auth=") + SAuthToken;

	SendMessage(AfxGetMenuWindowHandle(), UM_OPEN_URL, (WPARAM)(LPCTSTR)Url.Str(), (LPARAM)(LPCTSTR)sTitle);
}