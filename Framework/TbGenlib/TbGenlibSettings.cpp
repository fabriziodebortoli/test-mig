#include "stdafx.h"

#include <TbGeneric/SettingsTable.h>
#include <TbGeneric/DataObj.h>
#include <TbGeneric/GeneralFunctions.h>
#include <TbGeneric/ParametersSections.h>

#include <TbNameSolver/LoginContext.h>

#include "SettingsTableManager.h"


#include "tbGenlibSettings.h"

//-----------------------------------------------------------------------------
static const TCHAR szTbGenlibSettingsFile[]		= _T("Settings.config");




//-----------------------------------------------------------------------------
TbGenlibSettings::TbGenlibSettings ()
	: 
	TbBaseSettings(szTbGenlibNamespace, szTbGenlibSettingsFile)
{
}

//-----------------------------------------------------------------------------
CString TbGenlibSettings::GetMsOfficeFormatting (LPCTSTR szEntry) 
{
	DataObj* pSetting = NULL;
	CString sett;
	CString sLanguage = AfxGetLoginInfos()->m_strPreferredLanguage;
	CString sCultureSection;
	if (!sLanguage.IsEmpty() && sLanguage.CompareNoCase(_T("en")))
	{
		sCultureSection.Format(_T("%s_%s"), szCultureSection, sLanguage);
		pSetting = AfxGetSettingValue(m_Owner, sCultureSection, szEntry, DataStr(), szTbGenlibSettingsFile);
		sett = (!pSetting || pSetting->GetDataType() != DataType::String) ? _T("") : pSetting->Str();
	}
	if (sett.IsEmpty())
	{
		pSetting = AfxGetSettingValue(m_Owner, szCultureSection, szEntry, DataStr(), szTbGenlibSettingsFile);
		sett = (!pSetting || pSetting->GetDataType() != DataType::String) ? _T("") : pSetting->Str();
	}

	return sett;
}

//-----------------------------------------------------------------------------
CString TbGenlibSettings::GetExcelDateFormat () 
{
	return GetMsOfficeFormatting(szExcelDateFormat);
}

void TbGenlibSettings::SetExcelDateFormat (const CString& s) 
{
	if (s.Compare(GetExcelDateFormat()))
		AfxSetSettingValue(m_Owner, szCultureSection, szExcelDateFormat, DataStr(s), szTbGenlibSettingsFile);
}

//-----------------------------------------------------------------------------
CString TbGenlibSettings::GetExcelDateTimeFormat () 
{
	return GetMsOfficeFormatting(szExcelDateTimeFormat);
}

void TbGenlibSettings::SetExcelDateTimeFormat (const CString& s) 
{
	if (s.Compare(GetExcelDateTimeFormat()))
		AfxSetSettingValue(m_Owner, szCultureSection, szExcelDateTimeFormat, DataStr(s), szTbGenlibSettingsFile);
}

//-----------------------------------------------------------------------------
CString TbGenlibSettings::GetExcelTimeFormat () 
{
	return GetMsOfficeFormatting(szExcelTimeFormat);
}

void TbGenlibSettings::SetExcelTimeFormat (const CString& s) 
{
	if (s.Compare(GetExcelTimeFormat()))
		AfxSetSettingValue(m_Owner, szCultureSection, szExcelTimeFormat, DataStr(s), szTbGenlibSettingsFile);
}
