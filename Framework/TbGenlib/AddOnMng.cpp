
#include "stdafx.h"

#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\TBNamespaces.h>

#include <TbClientCore\ModuleConfigInfo.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TBGeneric\globals.h>
#include <TBGenlib\Parscbx.h>

// System Libraries
#include "generic.h"
#include "HLinkObj.h"
#include "addonmng.h"
#include "Baseapp.h"
#include "Messages.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
static const TCHAR szAliasSep[] = _T(";");

///////////////////////////////////////////////////////////////////////////////
// 						AddOnInterfaceObj:
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC (AddOnInterfaceObj, CObject)

//-----------------------------------------------------------------------------
AddOnInterfaceObj::AddOnInterfaceObj ()
	:
	m_State (AddOnInterfaceObj::NotRegistered)
{
}

//-----------------------------------------------------------------------------
AddOnInterfaceObj::~AddOnInterfaceObj ()
{
	UnloadRegistrationInfo ();
}

//-----------------------------------------------------------------------------
// metodo per identificare l'avvenuta registrazione

void AddOnInterfaceObj::SetRegisterState (AddOnInterfaceObj::RegisterState value)  
{ m_State = value; }

AddOnInterfaceObj::RegisterState AddOnInterfaceObj::GetRegisterState () 
{ return m_State; }

//-----------------------------------------------------------------------------
void AddOnInterfaceObj::UnloadRegistrationInfo ()
{
	m_arHotLinks.			RemoveAll();
	m_arFunctions.			RemoveAll();
	m_arDocTemplates.		RemoveAll();
	m_arItemSources.		RemoveAll();
	m_arValidators.			RemoveAll();
}

//-----------------------------------------------------------------------------
void AddOnInterfaceObj::DeclareFunction (FunctionDataInterface* pFn)
{
	if (!pFn || !pFn->GetNamespace().IsValid())
	{
		if (pFn)
			AfxGetDiagnostic()->Add(cwsprintf(_TB("{0-%s} object registration: invalid namespace. Registration ignored"), 
					pFn->GetNamespace().ToString()));
		delete pFn;
		return;
	}

	AddOnModule* pAddOnMod = AfxGetAddOnModule (pFn->GetNamespace());
	if (!pAddOnMod)
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("{0-%s} object registration: no module to be associated to the component. Registration ignored"), 
				pFn->GetNamespace().ToString()));
		delete pFn;
		return;
	}

	CFunctionDescription* pXmlDescri = pAddOnMod->m_XmlDescription.GetParamObjectInfo(pFn->GetNamespace());
	if (!pXmlDescri)
	{
		AfxGetDiagnostic()->Add(cwsprintf(_TB("{0-%s} object registration: no XML description available for the component. Registration ignored. "), 
					pFn->GetNamespace().ToString()));
		TRACE (pFn->GetNamespace().ToString());
		delete pFn;
		return;
	}

	*((CFunctionDescription*) pFn) = *pXmlDescri;

	if (pFn->GetNamespace().GetType() == CTBNamespace::HOTLINK)
	{
		pFn->GetInfoOSL()->SetType(OSLType_HotLink);
		m_arHotLinks.Add(pFn);
	}
	else
	{
		pFn->GetInfoOSL()->SetType(OSLType_Function);
		m_arFunctions.Add(pFn);
	}
}

//-----------------------------------------------------------------------------
void AddOnInterfaceObj::SetSourceFolder (char* cFolder)
{
	CString sFolder (cFolder);
	m_SourceFolder = GetPath(sFolder);
}

///////////////////////////////////////////////////////////////////////////////
// 						class AddOnDll
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(AddOnDll, CObject)

//-----------------------------------------------------------------------------
AddOnDll::AddOnDll (HINSTANCE hInstance, const CTBNamespace& aDllNamespace)
	:
	m_hInstance		(hInstance),
	m_Namespace		(aDllNamespace),
	m_bIsRegistered	(false)
{
	m_arAddOnLibs.SetOwns(FALSE);
}

///////////////////////////////////////////////////////////////////////////////
// 						class AddOnDllArray
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// 						class AddOnLibrary
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(AddOnLibrary, CObject);

//-----------------------------------------------------------------------------
AddOnLibrary::AddOnLibrary (AddOnInterfaceObj* pAddOn, AddOnDll* pAddOnDll, AddOnModule* pAddOnModule)
	:
	m_pAddOn		(pAddOn),
	m_pAddOnDll		(pAddOnDll),
	m_pAddOnModule	(pAddOnModule)
{
	m_pAddOnDll->AddAddOnLibrary(this);
}

//-----------------------------------------------------------------------------
AddOnLibrary::~AddOnLibrary()
{
	delete m_pAddOn;

	// instance deleted as last operation by ~AddOnApplication
	m_pAddOnDll = NULL;
}

//-----------------------------------------------------------------------------
void AddOnLibrary::InitLibraryIdentity (const CTBNamespace& aLibNamespace)
{	
	// inizializzo il namespace 
	m_Namespace	= aLibNamespace;
}

//-----------------------------------------------------------------------------
void AddOnLibrary::SetItemSource(const CTBNamespace& aNamespace, CRuntimeClass* pClass)
{
	CString s = aNamespace.ToString();
	s.MakeLower();
	m_pAddOn->m_arItemSources[s] = pClass;
}

//-----------------------------------------------------------------------------
CRuntimeClass* AddOnLibrary::GetItemSource(const CTBNamespace& aNamespace)
{
	CRuntimeClass* pClass = NULL;
	CString s = aNamespace.ToString();
	s.MakeLower();
	m_pAddOn->m_arItemSources.Lookup(s, pClass);
	return pClass;
}

//-----------------------------------------------------------------------------
void AddOnLibrary::GetControlBehaviours(CTBNamespaceArray& ar)
{
	if (!m_pAddOn)
		return;
	POSITION pos = m_pAddOn->m_arControlBehaviours.GetStartPosition();
	while (pos)
	{
		CString sNs;
		CRuntimeClass* pClass;
		m_pAddOn->m_arControlBehaviours.GetNextAssoc(pos, sNs, pClass);
		ar.Add(new CTBNamespace(sNs));
	}
}

//-----------------------------------------------------------------------------
CRuntimeClass* AddOnLibrary::GetControlBehaviour(const CTBNamespace& aNamespace)
{
	CRuntimeClass* pClass = NULL;
	CString s = aNamespace.ToString();
	s.MakeLower();
	m_pAddOn->m_arControlBehaviours.Lookup(s, pClass);
	return pClass;
}

//-----------------------------------------------------------------------------
void AddOnLibrary::SetControlBehaviour(const CTBNamespace& aNamespace, CRuntimeClass* pClass)
{
	CString s = aNamespace.ToString();
	s.MakeLower();
	m_pAddOn->m_arControlBehaviours[s] = pClass;
}


//-----------------------------------------------------------------------------
void AddOnLibrary::SetValidator(const CTBNamespace& aNamespace, CRuntimeClass* pClass)
{
	CString s = aNamespace.ToString();
	s.MakeLower();
	m_pAddOn->m_arValidators[s] = pClass;
}

//-----------------------------------------------------------------------------
CRuntimeClass* AddOnLibrary::GetValidator(const CTBNamespace& aNamespace)
{
	CRuntimeClass* pClass = NULL;
	CString s = aNamespace.ToString();
	s.MakeLower();
	m_pAddOn->m_arValidators.Lookup(s, pClass);
	return pClass;
}

//-----------------------------------------------------------------------------
void AddOnLibrary::SetDataAdapter(const CTBNamespace& aNamespace, CRuntimeClass* pClass)
{
	CString s = aNamespace.ToString();
	s.MakeLower();
	m_pAddOn->m_arDataAdapters[s] = pClass;
}

//-----------------------------------------------------------------------------
CRuntimeClass* AddOnLibrary::GetDataAdapter(const CTBNamespace& aNamespace)
{
	CRuntimeClass* pClass = NULL;
	CString s = aNamespace.ToString();
	s.MakeLower();
	m_pAddOn->m_arDataAdapters.Lookup(s, pClass);
	return pClass;
}

//-----------------------------------------------------------------------------
void AddOnLibrary::GetItemSources(CTBNamespaceArray& ar)
{
	if (!m_pAddOn)
		return;
	POSITION pos = m_pAddOn->m_arItemSources.GetStartPosition();
	while (pos)
	{
		CString sNs;
		CRuntimeClass* pClass;
		m_pAddOn->m_arItemSources.GetNextAssoc(pos, sNs , pClass);
		ar.Add(new CTBNamespace(sNs));
	}
}


//-----------------------------------------------------------------------------
void AddOnLibrary::GetValidators(CTBNamespaceArray& ar)
{
	if (!m_pAddOn)
		return;
	POSITION pos = m_pAddOn->m_arValidators.GetStartPosition();
	while (pos)
	{
		CString sNs;
		CRuntimeClass* pClass;
		m_pAddOn->m_arValidators.GetNextAssoc(pos, sNs, pClass);
		ar.Add(new CTBNamespace(sNs));
	}
}
//-----------------------------------------------------------------------------
CBaseDescription* AddOnLibrary::GetRegisteredObjectInfo (const CTBNamespace& aNamespace)
{
	if (!aNamespace.IsValid() || !m_pAddOn)
		return NULL;

	CObArray* pObjectsArray = NULL;
	switch (aNamespace.GetType())
	{
		case CTBNamespace::HOTLINK:
			pObjectsArray = &m_pAddOn->m_arHotLinks;			break;
		case CTBNamespace::FUNCTION:
		case CTBNamespace::EVENTHANDLER:
			pObjectsArray = &m_pAddOn->m_arFunctions;			break;
		default:
			ASSERT (FALSE);
			return NULL;
	}

	CBaseDescription* pEntry;
	for (int i=0; i <= pObjectsArray->GetUpperBound(); i++)
	{
		pEntry = (CBaseDescription*) pObjectsArray->GetAt(i);
		if (pEntry && pEntry->GetNamespace().IsValid() && (aNamespace == pEntry->GetNamespace()))
			return pEntry;
	}

	return NULL;
}

// Le funzioni non hanno una CRuntimeClass per la ricerca
//-----------------------------------------------------------------------------
CBaseDescription* AddOnLibrary::GetRegisteredObjectInfo 
	(
		CTBNamespace::NSObjectType aType, 
		const CString& sClassName
	)
{
	if (sClassName.IsEmpty() || !m_pAddOn)
		return NULL;
	
	CObArray* pObjectsArray = NULL;
	switch (aType)
	{
		case CTBNamespace::HOTLINK:
			pObjectsArray = &m_pAddOn->m_arHotLinks;			break;
		case CTBNamespace::FUNCTION:
		case CTBNamespace::EVENTHANDLER:
			pObjectsArray = &m_pAddOn->m_arFunctions;			break;
		default:
			ASSERT (FALSE);
			return NULL;
	}

	CBaseDescription* pEntry;
	CString sName;
	for (int i=0; i <= pObjectsArray->GetUpperBound(); i++)
	{
		pEntry = (CBaseDescription*) pObjectsArray->GetAt(i);
		if (!pEntry)
			continue;

		ASSERT(pEntry->IsKindOf(RUNTIME_CLASS(FunctionDataInterface)));
		sName = ((FunctionDataInterface*) pEntry)->m_pComponentClass->m_lpszClassName;

		if (_tcsicmp((LPCTSTR) sClassName, sName) == 0)
			return pEntry;
	}

	return NULL;
}

// Le funzioni non hanno una CRuntimeClass per la ricerca
//-----------------------------------------------------------------------------
CBaseDescription* AddOnLibrary::GetRegisteredObjectInfo (CRuntimeClass* pObjectClass)
{
	if (pObjectClass == NULL || !m_pAddOn)
		return NULL;

	CString sClassName (pObjectClass->m_lpszClassName);

	if (pObjectClass->IsDerivedFrom(RUNTIME_CLASS(HotKeyLinkObj)))
		return GetRegisteredObjectInfo(CTBNamespace::HOTLINK, sClassName);

	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
// 									AddOnModule:
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (AddOnModule, CObject)

//-----------------------------------------------------------------------------
AddOnModule::AddOnModule(AddOnApplication* pApp)
{
	m_nDatabaseRel = -1;
	m_pAddOnLibs = new AddOnLibsArray();
	m_bIsValid = TRUE;
	m_bIsACustomization = FALSE;
	m_pApplication = pApp;
}

//-----------------------------------------------------------------------------
AddOnModule::~AddOnModule()
{
	if (m_pAddOnLibs)
		delete m_pAddOnLibs;
}

//-----------------------------------------------------------------------------
void AddOnModule::InitModuleIdentity
	(
		const CString& strModName, 
		const CString& strAddOnApp
	) 
{	
	m_Namespace.SetType				(CTBNamespace::MODULE);
	m_Namespace.SetApplicationName	(strAddOnApp);
	m_Namespace.SetObjectName		(CTBNamespace::MODULE, strModName);
	m_sModulePath = AfxGetPathFinder()->GetModulePath(strAddOnApp, strModName, CPathFinder::STANDARD);
}

//-----------------------------------------------------------------------------
void AddOnModule::SetValid (const BOOL& bValue)
{
	m_bIsValid = bValue;
}

//-----------------------------------------------------------------------------
const CString	AddOnModule::GetAppModTitle() const 
{ return m_pApplication ? m_pApplication->GetTitle() + '.' + GetModuleTitle() : GetModuleTitle(); }

//-----------------------------------------------------------------------------
AddOnLibrary* AddOnModule::GetLibraryFromHinstance (HINSTANCE hDllInstance)
{
	for (int nLib = 0; nLib <= m_pAddOnLibs->GetUpperBound(); nLib++)
	{
		AddOnLibrary* pAddOnLib = m_pAddOnLibs->GetAt(nLib);
		if (pAddOnLib->m_pAddOnDll->GetInstance() == hDllInstance)
			return pAddOnLib;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
AddOnLibrary* AddOnModule::GetLibraryFromName (const CString& sName)
{
	for (int nLib = 0; nLib <= m_pAddOnLibs->GetUpperBound(); nLib++)
	{
		AddOnLibrary* pAddOnLib = m_pAddOnLibs->GetAt(nLib);
		if (_tcsicmp(pAddOnLib->GetLibraryName(), sName) == 0)
			return pAddOnLib;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
AddOnLibrary* AddOnModule::GetOwnerLibrary (CRuntimeClass* pObjectClass)
{
	if (!pObjectClass)
		return NULL;

	for (int nLib = 0; nLib <= m_pAddOnLibs->GetUpperBound(); nLib++)
	{
		AddOnLibrary* pAddOnLib = m_pAddOnLibs->GetAt(nLib);
		if (pAddOnLib->GetRegisteredObjectInfo(pObjectClass))
			return pAddOnLib;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
AddOnLibrary* AddOnModule::GetOwnerLibrary (const CTBNamespace* pNamespace)
{
	if (!pNamespace)
		return NULL;

	for (int nLib = 0; nLib <= m_pAddOnLibs->GetUpperBound(); nLib++)
	{
		AddOnLibrary* pAddOnLib = m_pAddOnLibs->GetAt(nLib);
		if (pAddOnLib->GetRegisteredObjectInfo(*pNamespace))
			return pAddOnLib;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
AddOnLibrary* AddOnModule::GetOwnerLibrary (const CTBNamespace::NSObjectType aType, const CString& sClassName)
{
	if (sClassName.IsEmpty())
		return NULL;

	for (int nLib = 0; nLib <= m_pAddOnLibs->GetUpperBound(); nLib++)
	{
		AddOnLibrary* pAddOnLib = m_pAddOnLibs->GetAt(nLib);
		if (pAddOnLib->GetRegisteredObjectInfo(aType, sClassName))
			return pAddOnLib;
	}
	
	return NULL;
}

//-----------------------------------------------------------------------------
CBaseDescription* AddOnModule::GetRegisteredObjectInfo (const CTBNamespace& aNamespace)
{
	if (!aNamespace.IsValid())
		return NULL;

	AddOnLibrary* pAddOnLib = GetOwnerLibrary(&aNamespace);
	if (pAddOnLib)
		return pAddOnLib->GetRegisteredObjectInfo(aNamespace);

	return NULL;
}

//-----------------------------------------------------------------------------
const BOOL AddOnModule::HasHotlinks()
{
	return m_XmlDescription.GetReferencesInfo().GetHotLinks().GetSize();
}

//-----------------------------------------------------------------------------
const BOOL AddOnModule::HasHotlinks(DataType dt)
{
	const CBaseDescriptionArray& ar = m_XmlDescription.GetReferencesInfo().GetHotLinks();
	int n = ar.GetSize();

	if (dt == DataType::Variant)
		return n > 0;

	for (int i = 0; i < n; i++)
	{
		CHotlinkDescription* pHkl = dynamic_cast<CHotlinkDescription*>(ar.GetAt(i));
		ASSERT_VALID(pHkl);
		if (pHkl->GetReturnValueDataType() == dt)
			return TRUE;
	}
	return FALSE;
}

///////////////////////////////////////////////////////////////////////////////
// 						AddOnApplication:
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(AddOnApplication, CObject)

//-----------------------------------------------------------------------------
AddOnApplication::AddOnApplication(AddOnModsArray* pAddOnMod)
	:
	m_pAddOnModules	(pAddOnMod)
{
	if (!pAddOnMod)
		m_pAddOnModules = new AddOnModsArray;
	m_bIsCustom = FALSE;
}

//-----------------------------------------------------------------------------
AddOnApplication::~AddOnApplication()
{
	SAFE_DELETE(m_pAddOnModules);
	m_AddOnDlls.RemoveAll();
}

//-----------------------------------------------------------------------------
void AddOnApplication::AddAlias (const CString& sDllName, const CString& sAlias)
{
	TB_OBJECT_LOCK(&m_DllAliases);
	CString sKey(sDllName), sValue;
	sKey = sKey.MakeLower();

	m_DllAliases.Lookup (sKey, sValue);

	if (sValue.IsEmpty())
		sValue = szAliasSep;
	sValue += sAlias + szAliasSep;
	
	m_DllAliases[sKey] = sValue;
}
void AddOnApplication::AdAddonDll(AddOnDll* pDll)
{
	TB_OBJECT_LOCK(&m_AddOnDlls); 
	m_AddOnDlls.Add(pDll);
}
//-----------------------------------------------------------------------------
AddOnDll* AddOnApplication::GetAddOnDll (HINSTANCE hDllInstance)
{
	TB_OBJECT_LOCK_FOR_READ(&m_AddOnDlls); 
	for (int i = 0; i <= m_AddOnDlls.GetUpperBound(); i++)
	{
		AddOnDll* pAddOnDll = m_AddOnDlls.GetAt(i);
		ASSERT(pAddOnDll);

		if (pAddOnDll->GetInstance() == hDllInstance) 
			return pAddOnDll;
	}
	return NULL;
}
//-----------------------------------------------------------------------------
AddOnDll* AddOnApplication::GetAddOnDll (const CTBNamespace& aDllNamespace)
{
	TB_OBJECT_LOCK_FOR_READ(&m_AddOnDlls); 
	AddOnDll* pAddOnDll;
	for (int i=0; i <= m_AddOnDlls.GetUpperBound(); i++)
	{
		pAddOnDll = m_AddOnDlls.GetAt(i);
		if (_tcsicmp(pAddOnDll->GetDllName(), aDllNamespace.GetObjectName()) == 0)
			return pAddOnDll;
	}
	return NULL;
}

//-----------------------------------------------------------------------------
CStringArray* AddOnApplication::GetAliasesOf(const CString& sDllName)
{
	CString sKey(sDllName), sValue;
	sKey = sKey.MakeLower();
	
	TB_OBJECT_LOCK_FOR_READ(&m_DllAliases);
	m_DllAliases.Lookup (sKey, sValue);
	
	CStringArray* pAliases = new CStringArray;
	if (sValue.IsEmpty())
		return pAliases;

	int nCurrPos = 0;
	CString sAlias;

	while (nCurrPos < sValue.GetLength())
	{
		sAlias = sValue.Tokenize(szAliasSep, nCurrPos);
		if (!sAlias.IsEmpty())
			pAliases->Add(sAlias);
	}

	return pAliases;
}

//-----------------------------------------------------------------------------
void AddOnApplication::SetValid (const BOOL& bValue)
{ 
	for (int i=0; i <= m_pAddOnModules->GetUpperBound(); i++)
	{
		AddOnModule* pAddOnMod = m_pAddOnModules->GetAt(i);
		ASSERT(pAddOnMod);
		pAddOnMod->SetValid(bValue);
	}
}

//-----------------------------------------------------------------------------
void AddOnApplication::SetAddOnModArray(AddOnModsArray* pAddOnMod)
{
	if (m_pAddOnModules)
	{
		if (m_pAddOnModules->GetSize() != 0)
			return;
		
		delete m_pAddOnModules;
	}

	m_pAddOnModules = pAddOnMod;
}


// viene chiamato da TaskBuilder dopo la creazione dell'AddOnApplication
// per poter dare la possibilità agli sviluppatori di inizializzare le proprie DLL
// dopo la creazione dell'AddOnApplication a cui sono legate e dopo il caricamento 
// dei propri componenti
//-----------------------------------------------------------------------------
void AddOnApplication::AOI_SetAddOnMod()
{ 
	for (int i = 0; i <= m_pAddOnModules->GetUpperBound(); i++) 
	{
		AddOnModule* pAddOnMod = m_pAddOnModules->GetAt(i);
		ASSERT(pAddOnMod);

		for (int n = 0; n <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); n++) 
		{
			AddOnLibrary* pAddOnLib = pAddOnMod->m_pAddOnLibs->GetAt(n);
			ASSERT(pAddOnLib);
			ASSERT(pAddOnLib->m_pAddOnDll);
			ASSERT(pAddOnLib->m_pAddOn);
			pAddOnLib->m_pAddOn->AOI_SetAddOnMod();
		}
	}
}

//-----------------------------------------------------------------------------
void AddOnApplication::AOI_AttachClientDocs
	(
		CAbstractFormDoc*	pServerDoc, 
		CClientDocArray*	pArray
	)
{
	for (int i = 0; i <= m_pAddOnModules->GetUpperBound(); i++)
	{
		AddOnModule* pAddOnMod = m_pAddOnModules->GetAt(i);
		ASSERT(pAddOnMod);

		for (int n = 0; n <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); n++) 
		{
			AddOnLibrary* pAddOnLib = pAddOnMod->m_pAddOnLibs->GetAt(n);
			ASSERT(pAddOnLib);
			ASSERT(pAddOnLib->m_pAddOnDll);


			if (pAddOnLib->m_pAddOn)
				pAddOnLib->m_pAddOn->AOI_AttachClientDocs
					(
						pServerDoc, 
						pArray,
						pAddOnLib->m_Namespace
					);
		}
	}
}

//-----------------------------------------------------------------------------
void AddOnApplication::AOI_AttachFamilyClientDocs
	(
		CAbstractFormDoc*	pServerDoc, 
		CClientDocArray*	pArray
	)
{
	for (int i = 0; i <= m_pAddOnModules->GetUpperBound(); i++)
	{
		AddOnModule* pAddOnMod = m_pAddOnModules->GetAt(i);
		ASSERT(pAddOnMod);

		for (int n = 0; n <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); n++) 
		{
			AddOnLibrary* pAddOnLib = pAddOnMod->m_pAddOnLibs->GetAt(n);
			ASSERT(pAddOnLib);
			ASSERT(pAddOnLib->m_pAddOnDll);

			if (pAddOnLib->m_pAddOn)
				pAddOnLib->m_pAddOn->AOI_AttachFamilyClientDocs
				(
					pServerDoc, 
					pArray, 
					pAddOnLib->m_Namespace
				);
		}
	}
}

//-----------------------------------------------------------------------------
BOOL AddOnApplication::AOI_EndRegistration ()
{
	BOOL bOk = TRUE;
	
	for (int i = 0; i <= m_pAddOnModules->GetUpperBound(); i++)
	{
		AddOnModule* pAddOnMod = m_pAddOnModules->GetAt(i);
		ASSERT(pAddOnMod);

		for (int n = 0; n <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); n++) 
		{
			AddOnLibrary* pAddOnLib = pAddOnMod->m_pAddOnLibs->GetAt(n);
			ASSERT(pAddOnLib);
			ASSERT(pAddOnLib->m_pAddOnDll);
			ASSERT(pAddOnLib->m_pAddOn);
			
			bOk = bOk && pAddOnLib->m_pAddOn->AOI_EndRegistration(pAddOnLib->m_Namespace);
		}
	}

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL AddOnApplication::AOI_DeclareAddOnApp (const CString& sAppName)
{ 
	m_strAddOnAppName = sAppName;
	
	return TRUE;
}

//-----------------------------------------------------------------------------
const CStringArray* AddOnApplication::GetAvailableDocEnvClasses (const CTBNamespace& aDocNS)
{
	AddOnModule* pAddOnMod;
	for (int i=0; i <= m_pAddOnModules->GetUpperBound(); i++)
	{
		pAddOnMod = m_pAddOnModules->GetAt(i);
		if (pAddOnMod)
			return pAddOnMod->GetAvailableDocEnvClasses(aDocNS);
	}

	return NULL;
}
//-----------------------------------------------------------------------------
void AddOnApplication::GetDllHandles(CArray<HINSTANCE>& ar)
{
	TB_OBJECT_LOCK_FOR_READ(&m_AddOnDlls);
	for (int i = 0; i <= m_AddOnDlls.GetUpperBound(); i++)
	{
		AddOnDll* pAddOnDll = m_AddOnDlls.GetAt(i);
		ASSERT(pAddOnDll);

		ar.Add(pAddOnDll->GetInstance());
	}
}

//-----------------------------------------------------------------------------
const CString AddOnApplication::GetTitle () const 
{ 
	CLoginManagerInterface* pIntfce = AfxGetLoginManager();

	if (pIntfce && !AfxGetAuthenticationToken().IsEmpty())
	{
		CString sTitle (pIntfce->GetBrandedApplicationTitle(m_strAddOnAppName));
		if (!sTitle.IsEmpty())
			return sTitle;
	}

	return m_XmlDescription.m_LocalizableInfo.GetTitle(); 
}
/*
///////////////////////////////////////////////////////////////////////////////
// 						TBDataBaseVersion:
///////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
TBDataBaseVersion::TBDataBaseVersion()
	:	 
	m_nMajor (-1),
	m_nMinor (-1)	
{
}

//-----------------------------------------------------------------------------
TBDataBaseVersion::TBDataBaseVersion(short nVersion)
{
	SetVersion(nVersion);
};

//-----------------------------------------------------------------------------
TBDataBaseVersion::TBDataBaseVersion(DataInt aVersion)
{
	SetVersion(aVersion);
};

//-----------------------------------------------------------------------------
TBDataBaseVersion::TBDataBaseVersion(BYTE nMajor, BYTE nMinor)
{
	SetVersion(nMajor, nMinor);
};

//-----------------------------------------------------------------------------
void TBDataBaseVersion::SetVersion(short nVersion)
{
	m_nMajor = nVersion >> 8 ;
	m_nMinor = nVersion & 0xFF;
}

//-----------------------------------------------------------------------------
void TBDataBaseVersion::SetVersion(DataInt aVersion)
{
	SetVersion((short)aVersion);
}

//-----------------------------------------------------------------------------
void TBDataBaseVersion::SetVersion(BYTE nMajor, BYTE nMinor)
{
	m_nMajor = nMajor;
	m_nMinor = nMinor;
}

//-----------------------------------------------------------------------------
short TBDataBaseVersion::GetVersion() const
{
	return m_nMajor << 8 | m_nMinor;
}

//-----------------------------------------------------------------------------
DataInt TBDataBaseVersion::GetDataIntVersion() const
{
	return DataInt(m_nMajor << 8 | m_nMinor);
}
//----------------------------------------------------------------------------------------------
BOOL TBDataBaseVersion::IsEqual(const TBDataBaseVersion& aVersion) const
{
	if (this == &aVersion)
		return TRUE;
	
	return
		(
			m_nMajor == aVersion.m_nMajor &&
			m_nMinor == aVersion.m_nMinor
		);
}

//------------------------------------------------------------------------------
BOOL TBDataBaseVersion::operator == (const TBDataBaseVersion& aVersion) const
{
	return IsEqual(aVersion);
}

//------------------------------------------------------------------------------
BOOL TBDataBaseVersion::operator != (const TBDataBaseVersion& aVersion) const
{
	return !IsEqual(aVersion);
}

//------------------------------------------------------------------------------
TBDataBaseVersion& TBDataBaseVersion::operator = (const TBDataBaseVersion& aVersion)
{
	if (this == &aVersion)
		return *this;
	
	m_nMajor = aVersion.m_nMajor;
	m_nMinor = aVersion.m_nMinor;
	
	return *this;
}
*/

// la master è la prima applicazione non sistemistica caricata
//-----------------------------------------------------------------------------
int AddOnAppsArray::Add(AddOnApplication* pAddOnApp)
{
	int nIdx = -1;
	
	if (
		_tcsicmp((LPCTSTR)pAddOnApp->m_strAddOnAppName, szExtensionsApp) == 0 ||
		_tcsicmp((LPCTSTR)pAddOnApp->m_strAddOnAppName, szTaskBuilderApp) == 0
		)
		return Array::Add(pAddOnApp);

	
	//non ho ancora trovata la MasterApp
	if (!m_pMasterAddOnApp)
	{
		CString strMasterBrandFile = AfxGetPathFinder()->GetMasterBrandFile(pAddOnApp->m_strAddOnAppName);
		if (ExistFile(strMasterBrandFile))
		{
			m_pMasterAddOnApp = pAddOnApp;
			//se è una masterApp allora la metto in terza posizione dopo Framework ed Extensions
			if (GetSize() >= 3)
			{
				Array::InsertAt(2, pAddOnApp);
				return 2;
			}			
		}
	}
	return Array::Add(pAddOnApp);
}

//-----------------------------------------------------------------------------
AddOnApplication* AddOnAppsArray::GetMasterAddOnApp () const
{
	if (m_pMasterAddOnApp)
		return m_pMasterAddOnApp;

	AddOnApplication* pAddOnApp;
	for (int i=0; i <= GetUpperBound (); i++)
	{
		pAddOnApp = GetAt(i);
		if (
				_tcsicmp((LPCTSTR) pAddOnApp->m_strAddOnAppName,szExtensionsApp) == 0 ||
				_tcsicmp((LPCTSTR) pAddOnApp->m_strAddOnAppName,szTaskBuilderApp) == 0
			)
			continue;

		return pAddOnApp;
	}
	
	return NULL;
}

// il terzo parametro mi dice se il documento a cui agganciare i ClientDoc è un ADM
// in questo caso non devo fare i controlli per nome ma per derivazione
//-----------------------------------------------------------------------------
void AddOnAppsArray::AttachClientDocs(CAbstractFormDoc* pServerDoc, CClientDocArray* pArray) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = GetAt(i);

		ASSERT(pAddOnApp);
		pAddOnApp->AOI_AttachClientDocs(pServerDoc, pArray);
	}
}

//------------------------------------------------------------------------------
void AddOnAppsArray::AttachScriptClientDocs (CAbstractFormDoc* pAServerDoc) const
{
	CBaseDocument* pServerDoc = (CBaseDocument*) pAServerDoc;

	CTBNamespace aServerNs = pServerDoc->GetNamespace();
	AddOnModule* pAddOnMod = AfxGetAddOnModule(aServerNs);
	if (!pAddOnMod)
		return;

	CObArray arClientDocs;
	AfxGetClientDocs(aServerNs, arClientDocs);
	
	for (int n=0; n <= arClientDocs.GetUpperBound(); n++)
	{
		CClientDocDescription* pDescri = (CClientDocDescription*) arClientDocs.GetAt(n);
		if (pDescri->m_Type == CClientDocDescription::SCRIPT || pDescri->m_Type == CClientDocDescription::FAMILYSCRIPT)
		{
			if (AfxIsActivated (pDescri->GetNamespace().GetApplicationName(), pDescri->GetNamespace().GetModuleName()))
				pServerDoc->AttachScriptClientDoc(pDescri);
		}
	}	
}

//-----------------------------------------------------------------------------
void AddOnAppsArray::AttachFamilyClientDocs(CAbstractFormDoc* pServerDoc, CClientDocArray* pArray) const
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = GetAt(i);
		ASSERT(pAddOnApp);
		pAddOnApp->AOI_AttachFamilyClientDocs(pServerDoc, pArray);
	}
}
