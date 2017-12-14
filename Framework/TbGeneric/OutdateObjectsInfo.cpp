#include "stdafx.h"

#include <io.h>

#include "OutdateObjectsInfo.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//----------------------------------------------------------------------------------------------
//	class COutDateObjectDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(COutDateObjectDescription, CBaseDescription)

//----------------------------------------------------------------------------------------------
COutDateObjectDescription::COutDateObjectDescription()
	:
	CBaseDescription(),
	m_nRelease		(0),
	m_Operator		(EQ)
{
}

//----------------------------------------------------------------------------------------------
COutDateObjectDescription::COutDateObjectDescription(const CTBNamespace::NSObjectType aType)
	:
	CBaseDescription(aType),
	m_nRelease		(0),
	m_Operator		(ND)
{
}

//----------------------------------------------------------------------------------------------
void COutDateObjectDescription::SetRelease	(const int& nRelease)
{
	m_nRelease = nRelease;
}

//----------------------------------------------------------------------------------------------
void COutDateObjectDescription::SetOperator	(const OutDateOperator& ope)
{
	m_Operator = ope;
}

//----------------------------------------------------------------------------------------------
BOOL COutDateObjectDescription::IsOutDate (const int& nRelease) const
{
	return 
		(m_Operator == LT && nRelease < m_nRelease)  ||
		(m_Operator == LE && nRelease <= m_nRelease) ||
		(m_Operator == EQ && nRelease == m_nRelease) ||
		(m_Operator == GT && nRelease > m_nRelease)  ||
		(m_Operator == GE && nRelease >= m_nRelease);
}

//----------------------------------------------------------------------------------------------
//					class COutDateSettingsSectionDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(COutDateSettingsSectionDescription, COutDateObjectDescription)

//----------------------------------------------------------------------------------------------
COutDateSettingsSectionDescription::COutDateSettingsSectionDescription ()
{
}

//----------------------------------------------------------------------------------------------
void COutDateSettingsSectionDescription::SetOwner (const CString& sOwner)
{
	m_sOwner = sOwner;
}

//----------------------------------------------------------------------------------------------
BOOL COutDateSettingsSectionDescription::IsOutDateSetting(const CString& sSetting, const int& nRelease) const
{
	COutDateObjectDescription* pDescri;
	for (int i=0; i <= m_Settings.GetUpperBound(); i++)
	{
		pDescri = (COutDateObjectDescription*) m_Settings.GetAt(i);
		if (pDescri->GetName().CompareNoCase(sSetting) == 0)
			return pDescri->IsOutDate(nRelease);
	}
	
	return FALSE;
}

//----------------------------------------------------------------------------------------------
//					class COutDateObjectsDescription implementation
//----------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(COutDateObjectsDescription, CObject)

//----------------------------------------------------------------------------------------------
COutDateObjectsDescription::COutDateObjectsDescription ()
{
}

//----------------------------------------------------------------------------------------------
COutDateObjectDescription* COutDateObjectsDescription::GetOutDateObjectInfo (const CTBNamespace& sNamespace) const
{
	if (!sNamespace.IsValid())
		return NULL;

	COutDateObjectDescription* pInfo = NULL;

	if (sNamespace.GetType() == CTBNamespace::REPORT)
		pInfo = (COutDateObjectDescription*) m_arReportsInfo.GetInfo (sNamespace);
	
	return pInfo;
}

//----------------------------------------------------------------------------------------------
const CBaseDescriptionArray& COutDateObjectsDescription::GetReports	() const
{
	return m_arReportsInfo;
}

//----------------------------------------------------------------------------------------------
const CBaseDescriptionArray& COutDateObjectsDescription::GetSettings () const
{
	return m_arSettingsInfo;
}

//----------------------------------------------------------------------------------------------
void COutDateObjectsDescription::Clear()
{
	m_arReportsInfo.	RemoveAll();
	m_arSettingsInfo.	RemoveAll();
}

//----------------------------------------------------------------------------------------------
void COutDateObjectsDescription::ClearSettings()
{
	m_arSettingsInfo.RemoveAll();
}