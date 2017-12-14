#include "stdafx.h"

#include <TbGeneric/SettingsTable.h>
#include <TbGeneric/DataObj.h>
#include <TbGeneric/GeneralFunctions.h>

#include <TbNameSolver/LoginContext.h>

#include "SettingsTableManager.h"

#include "tbGenlibSettings.h"

//-----------------------------------------------------------------------------
static const TCHAR szTbGenlibSettingsFile[]		= _T("Settings.config");

//static const TCHAR szTbGenlib_Section_Preference[]			= _T("Preference");
static const TCHAR szTbGenlib_Section_MsOffice[]		= _T("MsOffice");
static const TCHAR szTbGenlib_Section_Culture[]			= _T("Culture");

static const TCHAR szExcelDateFormat[]				= _T("ExcelDateFormat");
static const TCHAR szExcelDateTimeFormat[]			= _T("ExcelDateTimeFormat");
static const TCHAR szExcelTimeFormat[]				= _T("ExcelTimeFormat");

//-----------------------------------------------------------------------------
TbGenlibSettings::TbGenlibSettings ()
	: 
	TbBaseSettings(_T("Module.Framework.TbGenlib"), szTbGenlibSettingsFile)
{
}

CString TbGenlibSettings::GetMsOfficeSectionName()
{
	return szTbGenlib_Section_MsOffice;
}

//-----------------------------------------------------------------------------
CString TbGenlibSettings::GetMsOfficeFormatting (LPCTSTR szEntry) 
{
	DataObj* pSetting = AfxGetSettingValue(m_Owner, szTbGenlib_Section_MsOffice, szEntry, DataStr(), szTbGenlibSettingsFile);

	CString sett = (!pSetting || pSetting->GetDataType() != DataType::String) ? _T("") : pSetting->Str();
	if (sett.IsEmpty())
	{
		CString sLanguage = AfxGetLoginInfos()->m_strPreferredLanguage;

		CString sCultureSection;
		if (!sLanguage.IsEmpty() && sLanguage.CompareNoCase(_T("en")))
		{
			sCultureSection.Format(_T("%s_%s"), szTbGenlib_Section_Culture, sLanguage);

			pSetting = AfxGetSettingValue(m_Owner, sCultureSection, szEntry, DataStr(), szTbGenlibSettingsFile);
			sett = (!pSetting || pSetting->GetDataType() != DataType::String) ? _T("") : pSetting->Str();
		}
	}
	if (sett.IsEmpty())
	{
		pSetting = AfxGetSettingValue(m_Owner, szTbGenlib_Section_Culture, szEntry, DataStr(), szTbGenlibSettingsFile);
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
		AfxSetSettingValue(m_Owner, szTbGenlib_Section_MsOffice, szExcelDateFormat, DataStr(s), szTbGenlibSettingsFile);
}

//-----------------------------------------------------------------------------
CString TbGenlibSettings::GetExcelDateTimeFormat () 
{
	return GetMsOfficeFormatting(szExcelDateTimeFormat);
}

void TbGenlibSettings::SetExcelDateTimeFormat (const CString& s) 
{
	if (s.Compare(GetExcelDateTimeFormat()))
		AfxSetSettingValue(m_Owner, szTbGenlib_Section_MsOffice, szExcelDateTimeFormat, DataStr(s), szTbGenlibSettingsFile);
}

//-----------------------------------------------------------------------------
CString TbGenlibSettings::GetExcelTimeFormat () 
{
	return GetMsOfficeFormatting(szExcelTimeFormat);
}

void TbGenlibSettings::SetExcelTimeFormat (const CString& s) 
{
	if (s.Compare(GetExcelTimeFormat()))
		AfxSetSettingValue(m_Owner, szTbGenlib_Section_MsOffice, szExcelTimeFormat, DataStr(s), szTbGenlibSettingsFile);
}
