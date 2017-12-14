#include "stdafx.h"

#include <TBNameSolver\ApplicationContext.h>
#include <TBNameSolver\LoginContext.h>

#include "DataObj.h"
#include "DataObjDescription.h"
#include "SettingsTable.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

#define DEFAULT_RELEASE  -1

//-----------------------------------------------------------------------------
TB_EXPORT SettingsTablePtr AFXAPI AfxGetSettingsTable()
{ 
	CLoginContext* pContext = AfxGetLoginContext();
	if (pContext)
		return SettingsTablePtr(pContext->GetObject<SettingsTable>(&CLoginContext::GetSettingsTable), TRUE);
	
	return SettingsTablePtr(AfxGetApplicationContext()->GetObject<SettingsTable>(&CApplicationContext::GetGlobalSettingsTable), TRUE);
}              

//-----------------------------------------------------------------------------
TB_EXPORT DataObj* AFXAPI AfxGetSettingValue		(
														const CTBNamespace& aNs,
														const CString& sSection, 
														const CString& sSetting,
														const DataObj& aDefault,
														const LPCTSTR sFileName /*NULL*/
													)
{
	SettingsTablePtr pTable = AfxGetSettingsTable();
	if (!pTable) return NULL;
	return pTable->GetSettingValue(aNs, sSection, sSetting, aDefault, sFileName);
}

//-----------------------------------------------------------------------------
TB_EXPORT DataObj* AFXAPI AfxGetSettingValue		(
														const CTBNamespace& aNs,
														const CString& sSection, 
														const CString& sSetting,
														const LPCTSTR sFileName /*NULL*/
													)
{
	SettingsTablePtr pTable = AfxGetSettingsTable();
	if (!pTable) return NULL;
	return pTable->GetSettingValue(aNs, sSection, sSetting, sFileName);
}

//-----------------------------------------------------------------------------
TB_EXPORT void AFXAPI AfxSetSettingValue		(
													const CTBNamespace& aNs,
													const CString& sSection, 
													const CString& sSetting,
													const DataObj& aValue,
													const LPCTSTR sFileName, /*NULL*/
													const int nRelease /*0*/
												)
{
	SettingsTablePtr pTable = AfxGetSettingsTable();
	pTable->SetSettingValue(aNs, sFileName, sSection, sSetting, aValue, nRelease);
}

//-----------------------------------------------------------------------------
TB_EXPORT SettingsSection* AFXAPI AfxCreateSettingsSection	(
																const CTBNamespace& aNamespace, 
																const CString& sFileName,
																const CString& sSection, 
																const int& nRelease /*0*/
															)
{
	SettingsTablePtr pTable = AfxGetSettingsTable();
	return pTable->AddSection (aNamespace, sFileName, sSection, nRelease);
}

//-----------------------------------------------------------------------------
TB_EXPORT void AfxRemoveSettingsSection		(
												const CTBNamespace& aNs, 
												const CString& sFileName,
												const CString& sSection
											)
{
	SettingsTablePtr pTable = AfxGetSettingsTable();
	SettingsSection* pSection = NULL;
	pSection = pTable->GetExactSection (aNs,sFileName, sSection);
	
	if (pSection)
		pTable->RemoveSection(pSection);
}

//-----------------------------------------------------------------------------
TB_EXPORT const SettingsSection* AfxGetBestSection	(
														const CTBNamespace& aNs, 
														const CString& sSection, 
														const LPCTSTR sFileName /*= NULL*/
													)
{
	SettingsTablePtr pTable = AfxGetSettingsTable();
	return pTable->GetBestSection(aNs, sSection, sFileName);
}

//============================================================================
//		class SettingObject implementation
//============================================================================

//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SettingObject, CDataObjDescription)

//----------------------------------------------------------------------------
SettingObject::SettingObject()
	:
	m_Source	(CURRENT_VALUE),
	m_bDeleted	(FALSE),
	m_nRelease	(0)
{
	SetCollateCultureSensitiveValue (TRUE);
}

//----------------------------------------------------------------------------
const int& SettingObject::GetRelease () const
{ 
	return m_nRelease; 
}

//----------------------------------------------------------------------------
const SettingObject::SettingSource& SettingObject::GetSource () const
{ 
	return m_Source; 
}

//----------------------------------------------------------------------------
const BOOL& SettingObject::IsDeleted () const
{ 
	return m_bDeleted; 
}

//----------------------------------------------------------------------------
void SettingObject::SetSource (const SettingSource& aSource)
{
	m_Source = aSource;
}

//----------------------------------------------------------------------------
void SettingObject::SetRelease (const int& nRelease)
{
	m_nRelease = nRelease;
}

//----------------------------------------------------------------------------
void SettingObject::SetDeleted (const BOOL& bValue)
{
	m_bDeleted = bValue;
}

//----------------------------------------------------------------------------
SettingObject* SettingObject::Clone () const
{
	SettingObject* so = (SettingObject*) GetRuntimeClass()->CreateObject();
	
	so->Assign(*this);
    return so;
}

//----------------------------------------------------------------------------
void SettingObject::Assign (const SettingObject& so)
{
	CDataObjDescription::Assign(so);

	m_Source	= so.m_Source;
	m_nRelease	= so.m_nRelease;
	m_bDeleted	= so.m_bDeleted;
}

//------------------------------------------------------------------------------
const SettingObject& SettingObject::operator= (const SettingObject& so)
{
	Assign(so);
	return *this;	
}

//============================================================================
//		class SettingObjects implementation
//============================================================================

IMPLEMENT_DYNCREATE(SettingObjects, Array)
//----------------------------------------------------------------------------
SettingObjects::SettingObjects ()
{
}

//----------------------------------------------------------------------------
SettingObjects::SettingObjects (const CString& sSettingName)
	:
	m_sSettingName	(sSettingName)
{
}

//------------------------------------------------------------------------------
SettingObjects::SettingObjects (const SettingObjects& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
const CString& SettingObjects::GetName () const
{ 
	return m_sSettingName; 
}

//------------------------------------------------------------------------------
SettingObject* SettingObjects::GetSetting ()
{
	return GetSetting(SettingObject::CURRENT_VALUE);
}

//------------------------------------------------------------------------------
SettingObject* SettingObjects::GetSetting (SettingObject::SettingSource aFrom)
{
	SettingObject* pObj;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		pObj = GetAt(i);
		if (pObj && pObj->GetSource() == aFrom && !pObj->IsDeleted())
			return pObj;
	}

	return NULL;
}

// ritorna il valore valido originalmente al momento della lettura
//------------------------------------------------------------------------------
SettingObject* SettingObjects::GetOriginalSetting()
{
	// scaletta di priorità 
	SettingObject* pSetting = GetSetting(SettingObject::FROM_COMPANYUSER);

	if (!pSetting) pSetting = GetSetting(SettingObject::FROM_COMPANYUSERS);
	if (!pSetting) pSetting = GetSetting(SettingObject::FROM_ALLCOMPANYUSER);
	if (!pSetting) pSetting = GetSetting(SettingObject::FROM_ALLCOMPANYUSERS);
	if (!pSetting) pSetting = GetSetting(SettingObject::FROM_STANDARD);
	if (!pSetting) pSetting = GetSetting(SettingObject::DEFAULT_VALUE);
	
	return pSetting;
}

//------------------------------------------------------------------------------
void SettingObjects::AddSetting (SettingObject* pSetting)
{
	// se esiste già un vero duplicato sovrascrivo
	if (GetUpperBound() > 0)
		for (int i=GetUpperBound(); i >= 0; i--)
			if (GetAt(i)->GetSource() == pSetting->GetSource())
			{
				RemoveAt(i);
				break;
			}

	Add(pSetting);
}

// imposta il valore correntemente valido per l'utente
//----------------------------------------------------------------------------
void SettingObjects::SetCurrentValue()
{
	// valore corrente
	SettingObject* pCurrent = GetSetting(SettingObject::CURRENT_VALUE);

	// mi faccio ritornare l'ultimo valido
	SettingObject* pSetting = GetOriginalSetting();
	if (!pSetting)
		return;

	if (!pCurrent)
	{
		pCurrent = new SettingObject ();
		Add (pCurrent);
	}

	*pCurrent = *pSetting;
	pCurrent->SetSource(SettingObject::CURRENT_VALUE);
}

//----------------------------------------------------------------------------
void SettingObjects::SetName (const CString& sName)
{
	m_sSettingName = sName;
}

//----------------------------------------------------------------------------
void SettingObjects::SetModified (const BOOL& bValue)
{
	for (int i=0; i <= GetUpperBound(); i++)
		GetAt(i)->SetDeleted(FALSE);
}

//------------------------------------------------------------------------------
void SettingObjects::SetSettingValue (const CString& sSettingName, const DataObj& aValue, const int nRelease /*0*/, BOOL bIsDefault /*FALSE*/)
{
	SettingObject* pCurrent = GetSetting();
	SettingObject* pSetting = bIsDefault ? GetSetting(SettingObject::DEFAULT_VALUE) : pCurrent;

	if (!pSetting)
	{
		pSetting = new SettingObject();
		pSetting->SetName(sSettingName);
		if (bIsDefault)
			pSetting->SetSource(SettingObject::DEFAULT_VALUE);
		AddSetting (pSetting);
	} 

	pSetting->SetRelease(nRelease);
	pSetting->SetValue(aValue);

	// se non esisteva prima un corrente lo setto anche a valore corrente
	if (!pCurrent && pSetting->GetSource() != SettingObject::CURRENT_VALUE)
		SetCurrentValue();
}

//------------------------------------------------------------------------------
const BOOL SettingObjects::IsDeleted ()
{
	SettingObject* pSetting = GetSetting(SettingObject::CURRENT_VALUE);
	
	return !pSetting ||pSetting->IsDeleted();
}

//------------------------------------------------------------------------------
void SettingObjects::SetDeleted	(const BOOL& bValue)
{
	SettingObject* pSetting = GetSetting(SettingObject::CURRENT_VALUE);
	if (pSetting)
		pSetting->SetDeleted(bValue);

	// se viene cancellato il setting, ne viene eliminato anche il default
	pSetting = GetSetting(SettingObject::DEFAULT_VALUE);
	if (pSetting)
		pSetting->SetDeleted(bValue);
}

//------------------------------------------------------------------------------
SettingObject* SettingObjects::GetAt (int nIdx) const
{
	return (SettingObject*) Array::GetAt(nIdx);	
}

//----------------------------------------------------------------------------
SettingObjects* SettingObjects::Clone() const
{
	SettingObjects* so = (SettingObjects*) GetRuntimeClass()->CreateObject();
	
	so->Assign(*this);
    return so;
}

//----------------------------------------------------------------------------
void SettingObjects::Assign(const SettingObjects& sos)
{
	m_sSettingName	= sos.m_sSettingName;

	RemoveAll();
	
	SettingObject* pSo;
	for (int i=0; i <= sos.GetUpperBound(); i++)
	{	
		pSo = ((SettingObject*) sos.GetAt(i))->Clone();
		Add(pSo);
	}
}

//------------------------------------------------------------------------------
const SettingObjects& SettingObjects::operator= (const SettingObjects& source)
{
	RemoveAll();

	for (int i = 0; i <= source.GetUpperBound(); i++)
		Add( ((SettingObject*) source[i])->Clone());     

	return *this;
}

//============================================================================
//		class SettingsSection implementation
//============================================================================
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SettingsSection, CObject)

//----------------------------------------------------------------------------
SettingsSection::SettingsSection ()
	:
	m_nRelease (0)
{
	m_LastFileDateStandard.		SetFullDate(TRUE);
	m_LastFileDateAllComUsers.	SetFullDate(TRUE);
	m_LastFileDateCompanyUsers.	SetFullDate(TRUE);
}

//----------------------------------------------------------------------------
SettingsSection::~SettingsSection ()
{
}

//----------------------------------------------------------------------------
SettingsSection* SettingsSection::Clone(BOOL bWithSettings /*TRUE*/) const
{
	SettingsSection* ss = (SettingsSection*) GetRuntimeClass()->CreateObject();
	
	ss->Assign(*this, bWithSettings);
    return ss;
}

//----------------------------------------------------------------------------
void SettingsSection::Assign(const SettingsSection& ss, BOOL bWithSettings /*TRUE*/)
{
	m_sSectionName	= ss.m_sSectionName;
	m_sFileName		= ss.m_sFileName;
	m_Owner			= ss.m_Owner;
	m_nRelease		= ss.m_nRelease;

	m_Settings.RemoveAll();
	
	if (!bWithSettings)
		return;
	
	SettingObjects* pSo;
	for (int i=0; i <= ss.m_Settings.GetUpperBound(); i++)
	{	
		pSo = ((SettingObjects*) ss.m_Settings.GetAt(i))->Clone();
		m_Settings.Add(pSo);
	}
}

//----------------------------------------------------------------------------
SettingsSection* SettingsSection::GetExactSection
											(
												const CTBNamespace& aOwner,
												const CString& sFileName,
												const CString& sSection
											)
{
	if (
			_tcsicmp(m_sSectionName, sSection) == 0 && 
			(sFileName.IsEmpty() || _tcsicmp(m_sFileName, sFileName) == 0) && 
			m_Owner == aOwner
		)
		return this;

	return NULL;
}

//----------------------------------------------------------------------------
SettingObject* SettingsSection::GetSetting(const CString& sSettingName, BOOL bOriginal /*FALSE*/)
{
	SettingObjects* pSetting;
	for (int i=0; i <= m_Settings.GetUpperBound(); i++)
	{
		pSetting = (SettingObjects*) m_Settings.GetAt(i);
		if (pSetting && _tcsicmp(pSetting->GetName(), sSettingName) == 0)
			if (bOriginal)
				return pSetting->GetOriginalSetting();
			else
				return pSetting->GetSetting();
	}

	return NULL;
}

//----------------------------------------------------------------------------
SettingObject* SettingsSection::GetSetting (const CString& sSettingName, const SettingObject::SettingSource aFrom)
{
	SettingObjects* pSetting;
	for (int i=0; i <= m_Settings.GetUpperBound(); i++)
	{
		pSetting = (SettingObjects*) m_Settings.GetAt(i);
		if (pSetting && _tcsicmp(pSetting->GetName(), sSettingName) == 0)
			return pSetting->GetSetting(aFrom);
	}

	return NULL;
}

//----------------------------------------------------------------------------
DataObj* SettingsSection::GetSettingValue (const CString& sSettingName, const DataObj& aDefault)
{
	// per prima cosa setto il default (può cambiare nel tempo)
	SetSettingValue(sSettingName, aDefault, DEFAULT_RELEASE, TRUE);

	// valore corrente
	SettingObject* pSetting = GetSetting(sSettingName);
	DataObj* pDataObj = NULL;

	if (pSetting)
		pDataObj = pSetting->GetValue();

	// verifico che sia compatibile
	if (pDataObj && pDataObj->GetDataType() != DataType::Null && pDataObj->GetDataType() == aDefault.GetDataType())
		return pDataObj;

	// valore originario
	if (!pSetting)
		pSetting = GetSetting(sSettingName, TRUE);

	return pSetting->GetValue();
}


//----------------------------------------------------------------------------
const Array& SettingsSection::GetSettings () const
{
	return m_Settings;
}

//----------------------------------------------------------------------------
const DataDate&	SettingsSection::GetLastFileDate (const SettingObject::SettingSource aFrom) const 
{
	switch (aFrom)
	{
		case SettingObject::FROM_ALLCOMPANYUSERS :	return m_LastFileDateAllComUsers;
		case SettingObject::FROM_COMPANYUSERS :		return m_LastFileDateCompanyUsers;
	}

	return m_LastFileDateStandard; 
}

//----------------------------------------------------------------------------
void SettingsSection::SetLastFileDate(const SettingObject::SettingSource aFrom, const DataDate& bDate)
{
	switch (aFrom)
	{
		case SettingObject::FROM_ALLCOMPANYUSERS :	m_LastFileDateAllComUsers	= bDate; break;
		case SettingObject::FROM_COMPANYUSERS :		m_LastFileDateCompanyUsers	= bDate; break;
		default:									m_LastFileDateStandard		= bDate; break;
	}
}

//----------------------------------------------------------------------------
void SettingsSection::SetName (const CString& sName)
{
	m_sSectionName = sName;
}

//----------------------------------------------------------------------------
void SettingsSection::SetFileName (const CString& sName)
{
	m_sFileName = sName;
}

//----------------------------------------------------------------------------
void SettingsSection::SetOwner (const CTBNamespace& aNamespace)
{
	m_Owner = aNamespace;
}

//----------------------------------------------------------------------------
void SettingsSection::SetRelease (const int& nRelease)
{
	m_nRelease = nRelease;
}

//----------------------------------------------------------------------------
void SettingsSection::SetModified	(const BOOL& bValue)
{
	// lo applico alle sue sezioni
	SettingObjects* pSo = NULL;
	for (int i=0; i <= m_Settings.GetUpperBound(); i++)
	{	
		pSo = (SettingObjects*) m_Settings.GetAt(i);
		if (pSo)
			pSo->SetModified(FALSE);
	}
}

//----------------------------------------------------------------------------
void SettingsSection::AddSetting (SettingObject* pSetting)
{
	if (!pSetting)
		return;
	
	SettingObjects* pSo = NULL;
	for (int i=0; i <= m_Settings.GetUpperBound(); i++)
	{
		pSo = (SettingObjects*) m_Settings.GetAt(i);
		if (pSo && _tcsicmp(pSo->GetName(), pSetting->GetName()) == 0)
			break;
		pSo = NULL;
	}

	if (!pSo)
	{
		pSo = new SettingObjects(pSetting->GetName());
		m_Settings.Add(pSo);
	} 

	pSo->AddSetting(pSetting);
}

//------------------------------------------------------------------------------
const BOOL SettingsSection::IsDeleted ()
{
	SettingObjects* pSo = NULL;
	for (int i=0; i <= m_Settings.GetUpperBound(); i++)
	{
		pSo = (SettingObjects*) m_Settings.GetAt(i);
		if (!pSo->IsDeleted())
			return FALSE;
	}
	
	return TRUE;
}

//------------------------------------------------------------------------------
void SettingsSection::SetDeleted (const BOOL& bValue)
{
	SettingObjects* pSo = NULL;
	for (int i=0; i <= m_Settings.GetUpperBound(); i++)
	{
		pSo = (SettingObjects*) m_Settings.GetAt(i);
		pSo->SetDeleted (bValue);
	}
}

//------------------------------------------------------------------------------
void SettingsSection::SetSettingValue (const CString& sSettingName, const DataObj& aValue, const int nRelease /*0*/, BOOL bIsDefault /*FALSE*/)
{
	SettingObjects* pSo = NULL;
	for (int i=0; i <= m_Settings.GetUpperBound(); i++)
	{
		pSo = (SettingObjects*) m_Settings.GetAt(i);
		if (pSo && _tcsicmp(pSo->GetName(), sSettingName) == 0)
			break;
		
		pSo = NULL;
	}

	if (!pSo)
	{
		pSo = new SettingObjects(sSettingName);
		m_Settings.Add(pSo);
	} 

	pSo->SetSettingValue(sSettingName, aValue, nRelease, bIsDefault);
}

//------------------------------------------------------------------------------
void SettingsSection::ClearSettings	(const BOOL& bAllCommons, const BOOL& bSingleUser)
{
    SettingObjects*	pObjects;
	SettingObject*	pSetting;
	for (int i = GetSettings().GetUpperBound(); i >= 0 ; i--)
	{
		pObjects = (SettingObjects*) GetSettings().GetAt(i);
		if (!pObjects)
			continue;

		for (int n = pObjects->GetUpperBound(); n >= 0 ; n--)
		{
			pSetting = pObjects->GetAt(n);
			if (!pSetting)
				continue;
			
			// quelli per utente
			if (bSingleUser && 
					(
						pSetting->GetSource() == SettingObject::FROM_ALLCOMPANYUSER ||
						pSetting->GetSource() == SettingObject::FROM_COMPANYUSER
					)
				)
				pObjects->RemoveAt(n);
			// quelli comuni
			else if (bAllCommons && 
					(
						pSetting->GetSource() == SettingObject::FROM_STANDARD ||
						pSetting->GetSource() == SettingObject::FROM_ALLCOMPANYUSERS ||
						pSetting->GetSource() == SettingObject::FROM_COMPANYUSERS ||
						// elimino i deleted nel caso richiesto
						pSetting->IsDeleted()
					)
				)
				pObjects->RemoveAt(n);
		}
	}
}

//------------------------------------------------------------------------------
void SettingsSection::UpdateCurrentValues ()
{
    SettingObjects*	pObjects;
	for (int i = 0; i <= GetSettings().GetUpperBound() ; i++)
	{
		pObjects = (SettingObjects*) GetSettings().GetAt(i);
		if (pObjects)
			pObjects->SetCurrentValue();
	}
}

//------------------------------------------------------------------------------
const SettingsSection& SettingsSection::operator= (const SettingsSection& source)
{
	Assign (source, TRUE);
	return *this;
}

//============================================================================
//		class SettingsGroup implementation
//============================================================================

IMPLEMENT_DYNCREATE(SettingsGroup, Array)
//----------------------------------------------------------------------------
SettingsGroup::SettingsGroup ()
{
}

//----------------------------------------------------------------------------
SettingsGroup::SettingsGroup (const CString& sGroup)
{
	m_sGroup = sGroup;
}

//------------------------------------------------------------------------------
SettingsGroup::SettingsGroup (const SettingsGroup& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
const CString& SettingsGroup::GetGroup () 
{ 
	return m_sGroup; 
}

//------------------------------------------------------------------------------
SettingsSection* SettingsGroup::GetAt (int nIdx) const
{
	return (SettingsSection*) Array::GetAt(nIdx);	
}

//----------------------------------------------------------------------------
SettingsGroup* SettingsGroup::Clone() const
{
	SettingsGroup* sg = (SettingsGroup*) GetRuntimeClass()->CreateObject();
	
	sg->Assign(*this);
    return sg;
}

//----------------------------------------------------------------------------
void SettingsGroup::Assign(const SettingsGroup& sg)
{
	RemoveAll();
	m_sGroup = sg.m_sGroup;

	
	SettingsSection* pSs;
	for (int i=0; i <= sg.GetUpperBound(); i++)
	{	
		pSs =  sg.GetAt(i)->Clone();
		Add(pSs);
	}
}

//------------------------------------------------------------------------------
SettingsSection* SettingsGroup::GetBestSection 
									(
										const CTBNamespace& aOwner, 
										const CString& sSection, 
										const LPCTSTR sFileName /*NULL*/
									)
{
	// prima verifico se ne esiste una esatta
	SettingsSection* pSection = GetExactSection(aOwner, sFileName, sSection);
	if (pSection)
		return pSection;

	// scaletta di ricerca
	SettingsSection* pSameOwner		= NULL;
	SettingsSection* pSameFilename	= NULL;
	SettingsSection* pSameName		= NULL;

	for (int i=0; i <= GetUpperBound(); i++)
	{
		pSection = GetAt(i);
		
		// come minimo deve avere lo stesso nome
		if (_tcsicmp(pSection->GetName(), sSection))
			continue;

		pSameName = pSection;

		// ne esiste una con lo stesso owner	
		if  (!pSameOwner && pSection->GetOwner() == aOwner)
			pSameOwner = pSection;

		// ne esiste una con lo stesso filename
		if  (sFileName && !pSameFilename && _tcsicmp(pSection->GetFileName(), sFileName) == 0)
			pSameOwner = pSection;
	}

	// prima quella con lo stesso nome e namespace
	if (pSameOwner)
		return pSameOwner;

	// poi ce n'è uno con lo stesso nome e filename
	if (pSameFilename)
		return pSameFilename;

	// infine se esiste la prima con stesso nome
	return pSameName;
}

//------------------------------------------------------------------------------
SettingsSection* SettingsGroup::GetExactSection 
									(
										const CTBNamespace& aOwner,
										const CString& sFileName,
										const CString& sSection
									)
{
	SettingsSection* pSection;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		pSection = GetAt(i);
		
		SettingsSection* pS = pSection->GetExactSection(aOwner, sFileName, sSection);
		if (pS)
			return pS;
	}

	return NULL;
}

//------------------------------------------------------------------------------
const SettingsGroup& SettingsGroup::operator= (const SettingsGroup& source)
{
	RemoveAll();

	for (int i = 0; i <= source.GetUpperBound(); i++)
		Add( ((SettingsGroup*) source[i])->Clone());     

	return *this;
}

//----------------------------------------------------------------------------
void SettingsGroup::GetFileSections	(const CTBNamespace& aNamespace, const CString& sFileName, Array& aSections)
{
	SettingsSection* pSection;
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		pSection = GetAt(i);
		if (pSection->GetOwner() == aNamespace && _tcsicmp(pSection->GetFileName(), sFileName) == 0)
			aSections.Add(GetAt(i)->Clone());     
	}
}

//============================================================================
//		class SettingsTable implementation
//============================================================================

//----------------------------------------------------------------------------
SettingsTable::SettingsTable ()
{
}

//------------------------------------------------------------------------------
SettingsTable::SettingsTable (const SettingsTable& source)
{
	*this = source;
}

//------------------------------------------------------------------------------
SettingsGroup* SettingsTable::GetAt (int nIdx) const
{
	return (SettingsGroup*) Array::GetAt(nIdx);	
}

//------------------------------------------------------------------------------
DataObj* SettingsTable::GetSettingValue 
								(
									const CTBNamespace& aNs, 
									const CString& sSection, 
									const CString& sSetting, 
									const DataObj& aDefault,
									const LPCTSTR sFileName /*NULL*/
								)
{
	SettingsGroup* pGroup = GetGroup(aNs);
	SettingsSection* pSection = NULL;
	if (!pGroup)
	{
		pGroup = new SettingsGroup (aNs.GetApplicationName());
		Add (pGroup);
	}
	else
	{
		CString sFile (sFileName);
		if (!sFile .IsEmpty())
			pSection = pGroup->GetExactSection(aNs, sFileName, sSection);
		else
			pSection = pGroup->GetBestSection(aNs, sSection, sFileName);
	}

	if (!pSection)
	{
		pSection = new SettingsSection ();
		pSection->SetOwner (aNs);
		pSection->SetFileName (sFileName);
		pSection->SetName (sSection);
		pGroup->Add (pSection);
	}

	return pSection ? pSection->GetSettingValue(sSetting, aDefault) : NULL;
}

//------------------------------------------------------------------------------
DataObj* SettingsTable::GetSettingValue 
								(
									const CTBNamespace& aNs, 
									const CString& sSection, 
									const CString& sSetting, 
									const LPCTSTR sFileName /*NULL*/
								)
{
	SettingsGroup* pGroup = GetGroup(aNs);
	SettingsSection* pSection = NULL;
	if (!pGroup)
	{
		pGroup = new SettingsGroup (aNs.GetApplicationName());
		Add (pGroup);
	}
	else
	{
		CString sFile (sFileName);
		if (!sFile .IsEmpty())
			pSection = pGroup->GetExactSection(aNs, sFileName, sSection);
		else
			pSection = pGroup->GetBestSection(aNs, sSection, sFileName);
	}

	if (!pSection)
	{
		pSection = new SettingsSection ();
		pSection->SetOwner (aNs);
		pSection->SetFileName (sFileName);
		pSection->SetName (sSection);
		pGroup->Add (pSection);
	}

	if (!pSection) 
		return NULL;

	SettingObject* pSetting = pSection->GetSetting(sSetting);
	return pSetting ? pSetting->GetValue() : NULL;
}

//------------------------------------------------------------------------------
const SettingsTable& SettingsTable::operator= (const SettingsTable& source)
{
	RemoveAll();

	SettingsGroup* pGroup;
	for (int i = 0; i <= source.GetUpperBound(); i++)
	{
		pGroup = source.GetAt(i);
		Add(pGroup->Clone());     
	}

	return *this;
}

//------------------------------------------------------------------------------
SettingsGroup* SettingsTable::GetGroup (const CTBNamespace& aOwner)
{
	SettingsGroup* pGroup;
	for (int i=0; i <= GetUpperBound(); i++)
	{
		pGroup = GetAt(i);
		if (pGroup && _tcsicmp(pGroup->GetGroup(), aOwner.GetApplicationName()) == 0)
			return pGroup;
	}

	return NULL;
}

//------------------------------------------------------------------------------
const SettingsSection* SettingsTable::GetBestSection 
									(
										const CTBNamespace& aOwner, 
										const CString& sSection,
										const LPCTSTR sFileName /*NULL*/
									)
{
	SettingsGroup* pGroup = GetGroup(aOwner);
	if (pGroup)
		return pGroup->GetBestSection(aOwner, sSection, sFileName);
	
	return NULL;
}

//------------------------------------------------------------------------------
SettingsSection* SettingsTable::GetExactSection (
													const CTBNamespace& aOwner, 
													const CString& sFileName,
													const CString& sSection
												)
{
	SettingsGroup* pGroup = GetGroup(aOwner);
	if (pGroup)
		return pGroup->GetExactSection(aOwner, sFileName, sSection);
	
	return NULL;
}

//------------------------------------------------------------------------------
SettingsSection* SettingsTable::AddSection	(
												const CTBNamespace& aNamespace, 
												const CString& sFileName,
												const CString& sSection, 
												const int& nRelease /*0*/
											)
{
	SettingsSection* pSection = GetExactSection(aNamespace, sFileName, sSection);
	if (!pSection)
	{
		pSection = new SettingsSection();
		pSection->SetName		(sSection);
		pSection->SetFileName	(sFileName);
		pSection->SetOwner		(aNamespace);
		pSection->SetRelease	(nRelease);
		AddSection(pSection);
	}

	return pSection;
}

//------------------------------------------------------------------------------
void SettingsTable::AddSection (SettingsSection* pSection)
{
	ASSERT_TRACE(pSection,"Parameter pSection cannot be null");
	if (!pSection)
		return;

	SettingsGroup* pGroup = GetGroup(pSection->GetOwner());
	if (!pGroup)
	{
		// se non esiste lo creo
		pGroup = new SettingsGroup(pSection->GetOwner().GetApplicationName());
		pGroup->Add (pSection);
		Add(pGroup);
		return;
	}

	SettingsSection* pExisting = pGroup->GetExactSection(pSection->GetOwner(), pSection->GetFileName(), pSection->GetName());

	// se la sezione esiste già travaso i dati, ed elimino il duplicato
	if (pExisting)
	{
		SettingObjects* pObjs = NULL;
		SettingObject*  pObj  = NULL;
		for (int i=0; i <= pSection->GetSettings().GetUpperBound(); i++)
		{
			pObjs = (SettingObjects*) pSection->GetSettings().GetAt(i);
			if (!pObjs)
				continue;

			for (int n=0; n <= pObjs->GetUpperBound(); n++)
			{
				pObj = pObjs->GetAt(n);

				if (pObj->GetSource() != SettingObject::CURRENT_VALUE)
				{
					// se è cancellato lo recupero
					if (pObj->IsDeleted())
						pObj->SetDeleted(FALSE);

					pExisting->AddSetting (pObj->Clone());
				}
			}
		}
		delete pSection;
	}
	else	
		pGroup->Add(pSection);
}

//------------------------------------------------------------------------------
void SettingsTable::RemoveSection (SettingsSection* pSection)
{
	if (!pSection)
	{
		ASSERT_TRACE(pSection,"Parameter pSection cannot be null");
		return;
	}
	
	// cerco il gruppo
	SettingsGroup* pGroup = GetGroup (pSection->GetOwner());
	if (!pGroup)
		return;

	// ora dichiaro eliminata la sezione
	for (int i=pGroup->GetUpperBound(); i >= 0; i--)
		if (pGroup->GetAt(i) == pSection)
			pGroup->GetAt(i)->SetDeleted(TRUE);
}

//------------------------------------------------------------------------------
void SettingsTable::SetSettingValue	(
										const CTBNamespace& aNamespace, 
										const CString& sFileName,
										const CString& sSection, 
										const CString& sSettingName, 
										const DataObj& aValue, 
										const int nRelease /*0*/
									)
{
	// se la sezione non esiste la creo
	SettingsSection* pSection = GetExactSection(aNamespace, sFileName, sSection);
	if (!pSection)
		pSection = AddSection(aNamespace, sFileName, sSection, nRelease);

	// non dovrebbe succedere
	if (!pSection)
	{
		ASSERT_TRACE(pSection,"Parameter pSection cannot be null");
		return;
	}

	pSection->SetSettingValue(sSettingName, aValue, nRelease);
}

// ritorna l'array delle sezioni di appartenenti ad un file
//------------------------------------------------------------------------------
void SettingsTable::GetFileSections(const CTBNamespace& aNamespace, const CString& sFileName, Array& aSections)
{
	// cerco il gruppo
	SettingsGroup* pGroup = GetGroup (aNamespace);
	if (!pGroup)
		return;

	pGroup->GetFileSections(aNamespace, sFileName, aSections);
}

// ritorna l'array dei nomi delle sezioni di appartenenti ad un file
//------------------------------------------------------------------------------
void SettingsTable::GetFileSectionsNames(const CTBNamespace& aNamespace, const CString& sFileName, CStringArray& aSectionsNames, BOOL bExcludeDeleted /*FALSE*/)
{
	Array aSections;
	GetFileSections(aNamespace, sFileName, aSections);

	SettingsSection* pSection;
	// travaso i nomi
	for (int i=0; i <= aSections.GetUpperBound(); i++)
	{
		pSection = (SettingsSection*) aSections.GetAt(i);
		if (!bExcludeDeleted || !pSection->IsDeleted())
			aSectionsNames.Add (pSection->GetName());
	}
}

//------------------------------------------------------------------------------
void SettingsTable::SetModified	(const CTBNamespace& aNamespace, const BOOL& bValue)
{
	// cerco il gruppo
	SettingsGroup* pGroup = GetGroup (aNamespace);
	if (!pGroup)
		return;

	// se mi viene richiesto l'intero file, scorro la tabella
	SettingsSection* pSection = NULL;
	for (int i=0; i <= pGroup->GetUpperBound(); i++)
	{	
		pSection = pGroup->GetAt(i);
		if (!pSection || pSection->GetOwner() != aNamespace)
			continue;
		pSection->SetModified(FALSE);
	}
}

// ritorna l'ultima data di modifica del file letto
//------------------------------------------------------------------------------
DataDate SettingsTable::GetLastFileDate	(
											const CTBNamespace& aNamespace,
											const CString& sFileName,
											SettingObject::SettingSource aFrom
										)
{	
	// cerco il gruppo
	SettingsGroup* pGroup = GetGroup (aNamespace);
	if (!pGroup)
		return DataDate();

	SettingsSection* pSection = NULL;

	BOOL bModified = FALSE;
	for (int i=0; i <= pGroup->GetUpperBound(); i++)
	{	
		pSection = pGroup->GetAt(i);
		if (!pSection)
			continue;

		if	(
				pSection->GetOwner() == aNamespace && 
				_tcsicmp(pSection->GetFileName(), sFileName) == 0 
			)
			return pSection->GetLastFileDate(aFrom);
	}

	return DataDate();
}

// si occupa di eliminare dalla tabella i dati o comuni a tutti gli utenti o
// quelli relativi al singolo utente, preservando sia i DEFAULT che i CURRENT
//------------------------------------------------------------------------------
void SettingsTable::ClearSettingsOf		(
											const CTBNamespace&	aModule, 
											const CString&		sFileName, 
											const CString&		sSection, 
											const BOOL&			bAllCommons,
											const BOOL&			bSingleUser
										)
{
	CStringArray aFileSections;
	
	// se ho una sezione specifica vado diretta
	// altrimenti lavoro su tutte le sezioni del file
	if (sSection.IsEmpty())
		GetFileSectionsNames(aModule, sFileName, aFileSections);
	else
		aFileSections.Add(sSection);

	SettingsSection* pSection;
	for (int i=aFileSections.GetUpperBound(); i >=0 ; i--)
	{
		pSection = GetExactSection(aModule, sFileName, aFileSections.GetAt(i));
		
		if (pSection)
		{
			pSection->ClearSettings(bAllCommons, bSingleUser);
		
			// li ho tolti tutti, butto la sezione
			if (!pSection->GetSettings().GetSize())
				aFileSections.RemoveAt(i);
		}
	}
}

// si occupa di fare una copia parziale di una sezione della tabella copiando
// solo i current values ed i valori di defaults
//------------------------------------------------------------------------------
void SettingsTable::CopyCurrentValuesFrom	(
												SettingsTablePtr		pFromTable,
												const CTBNamespace&	aModule, 
												const CString&		sFileName, 
												const CString&		sSection 
											)
{
	CStringArray aFileSections;
	
	// se ho una sezione specifica vado diretta
	// altrimenti lavoro su tutte le sezioni del file
	if (sSection.IsEmpty())
		pFromTable->GetFileSectionsNames(aModule, sFileName, aFileSections);
	else
		aFileSections.Add(sSection);

	SettingsSection* pFromSection;
	SettingsSection* pToSection;
	SettingObjects*	 pObjects;
	SettingObject*	 pSetting;
	for (int i=0; i <= aFileSections.GetUpperBound(); i++)
	{
		pFromSection = pFromTable->GetExactSection(aModule, sFileName, aFileSections.GetAt(i));
		if (!pFromSection)
			continue;

		// se manca la sezione la genero, ma senza settings
		pToSection = GetExactSection(aModule, sFileName, aFileSections.GetAt(i));
		if (!pToSection)
		{
			pToSection = pFromSection->Clone(FALSE);
			AddSection(pToSection);
		}

		// adesso copio i current values e i defaults
		for (int n = 0; n <= pFromSection->GetSettings().GetUpperBound() ; n++)
		{
			pObjects = (SettingObjects*) pFromSection->GetSettings().GetAt(n);
			if (!pObjects)
				continue;

			SettingObject* pOld = NULL;
			pSetting = pObjects->GetSetting(SettingObject::DEFAULT_VALUE);
			if (pSetting)
				pToSection->AddSetting(pSetting->Clone());
			// se non esiste ed esisteva prima è stato rimosso
			else 
			{
				pOld = pToSection->GetSetting(pObjects->GetName(), SettingObject::DEFAULT_VALUE);
				if (pOld)
					pOld->SetDeleted(TRUE);
			}

			pSetting = pObjects->GetSetting(SettingObject::CURRENT_VALUE);
			if (pSetting)
				pToSection->AddSetting(pSetting->Clone());
			// se non esiste ed esisteva prima è stato rimosso
			else 
			{
				pOld = pToSection->GetSetting(pObjects->GetName(), SettingObject::CURRENT_VALUE);
				if (pOld)
					pOld->SetDeleted(TRUE);
			}
		}
	}
}

// fa scattare il ricalcolo dei valori correnti dopo un caricamento
//------------------------------------------------------------------------------
void SettingsTable::UpdateCurrentValuesOf	(
												const CTBNamespace&	aModule, 
												const CString&		sFileName, 
												const CString&		sSection 
											)
{
	CStringArray aFileSections;
	
	// se ho una sezione specifica vado diretta
	// altrimenti lavoro su tutte le sezioni del file
	if (sSection.IsEmpty())
		GetFileSectionsNames(aModule, sFileName, aFileSections);
	else
		aFileSections.Add(sSection);

	SettingsSection* pSection;
	for (int i=0; i <= aFileSections.GetUpperBound(); i++)
	{
		pSection = GetExactSection(aModule, sFileName, aFileSections.GetAt(i));
		
		if (pSection)
			pSection->UpdateCurrentValues();
	}
}