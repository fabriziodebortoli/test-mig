
#include "stdafx.h"

#include <TbGeneric\DataObj.h>
#include <TbGeneric\SettingsTable.h>

#include "ValidationMonitorSettings.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// File
static TCHAR szOwnerModule				[] = _NS_MOD("Module.Extensions.TbDataSynchroClient");
static TCHAR szFileName					[] = _SET_FILE("ValidationMonitor.config");

// Section
static TCHAR szValidationMonitorSection	[] = _SET_SECTION("ValidationMonitor");

// Resources Layout
static TCHAR szNeedMassiveValidation	[] = _SET_NAME("NeedMassiveValidation");


/////////////////////////////////////////////////////////////////////////////
//			class ValidationMonitorSettings Implementation
/////////////////////////////////////////////////////////////////////////////
//
//===========================================================================
ValidationMonitorSettings::ValidationMonitorSettings()
	:
	CParameterInfo(CTBNamespace(szOwnerModule), szFileName, szValidationMonitorSection)
{
}

//-----------------------------------------------------------------------------
BOOL ValidationMonitorSettings::GetNeedMassiveValidation()
{
	return *((DataBool*) AfxGetSettingValue(m_Owner, m_sSection, szNeedMassiveValidation, DataBool(TRUE), m_sFileName));
}

//-----------------------------------------------------------------------------
void ValidationMonitorSettings::SetNeedMassiveValidation(DataBool aValue)
{
	AfxSetSettingValue(m_Owner, m_sSection, szNeedMassiveValidation, aValue, m_sFileName); 
	AfxSaveSettings(m_Owner, m_sFileName, m_sSection);
}