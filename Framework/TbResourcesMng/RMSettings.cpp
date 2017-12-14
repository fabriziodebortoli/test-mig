#include "stdafx.h"

#include <TbGeneric\DataObj.h>
#include <TbGeneric\SettingsTable.h>

#include "RMSettings.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// File
static TCHAR szOwnerModule[]	= _NS_MOD("Module.Framework.TbResourcesMng");
static TCHAR szFileName[]		= _SET_FILE("Resources.config");

// Section
static TCHAR szResourcesLayoutSection[]	= _SET_SECTION("ResourcesLayout");

// Resources Layout
static TCHAR szDisabledToo[]	= _SET_NAME("DisabledToo");
static TCHAR szShowDetails[]	= _SET_NAME("ShowDetails");

/////////////////////////////////////////////////////////////////////////////
//					ResourcesLayoutSettings implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
ResourcesLayoutSettings::ResourcesLayoutSettings()
	:
	CParameterInfo(CTBNamespace(szOwnerModule), szFileName, szResourcesLayoutSection)
{
}

//-----------------------------------------------------------------------------
DataBool ResourcesLayoutSettings::GetDisabledToo()
{
	return *((DataBool*) AfxGetSettingValue(m_Owner, m_sSection, szDisabledToo, DataBool(FALSE), m_sFileName));
}

//-----------------------------------------------------------------------------
void ResourcesLayoutSettings::SetDisabledToo(DataBool aValue)
{
	AfxSetSettingValue(m_Owner, m_sSection, szDisabledToo, aValue, m_sFileName); 
	AfxSaveSettings(m_Owner, m_sFileName, m_sSection);
}

//-----------------------------------------------------------------------------
DataBool ResourcesLayoutSettings::GetShowDetails()
{
	return *((DataBool*) AfxGetSettingValue(m_Owner, m_sSection, szShowDetails, DataBool(TRUE), m_sFileName));
}

//-----------------------------------------------------------------------------
void ResourcesLayoutSettings::SetShowDetails(DataBool aValue)
{
	AfxSetSettingValue(m_Owner, m_sSection, szShowDetails, aValue, m_sFileName); 
	AfxSaveSettings(m_Owner, m_sFileName, m_sSection);
}

