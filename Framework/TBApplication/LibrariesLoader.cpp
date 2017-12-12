#include "stdafx.h"

#include <TbNameSolver\Templates.h>
#include <TbNameSolver\TBNamespaces.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\LoginContext.h>

#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\GeneralObjects.h>

#include <TbGenlib\SettingsTableManager.h>
#include <TbGenlib\Messages.h>

#include <TbGes\extdoc.hjson>

#include <TbOleDb\OleDbMng.h>

#include "LibrariesLoader.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

//-----------------------------------------------------------------------------
typedef AddOnInterfaceObj*	(CALLBACK* TB_ADDON)(void);
static const char	 szTaskBuilderAddOnHead	[]	= "_TaskBuilderAddOn_";
static const char	 szTaskBuilderAddOnTail	[]	= "@0";

static const TCHAR szTbLoader[]             = _T("TbLoader");
static const TCHAR szDLLExt[]				= _T(".dll");
//-----------------------------------------------------------------------------
CLibrariesLoader::CLibrariesLoader ()
	:
		m_bAllLoaded(false),
		m_bIsOnDemandEnabled(TRUE),
		m_nCounterDLLLoading (0)
{

}

//-----------------------------------------------------------------------------
CLibrariesLoader::~CLibrariesLoader ()
{
}

//-----------------------------------------------------------------------------
void CLibrariesLoader::InitOnDemandEnabled() 
{
	DataObj* pOnDemandEnabled = AfxGetSettingValue(snsTbGenlib, szEnvironment, szLoadLibrariesOnDemand, DataBool(m_bIsOnDemandEnabled), szTbDefaultSettingFileName);
	m_bIsOnDemandEnabled = pOnDemandEnabled ?  *(DataBool*) pOnDemandEnabled : TRUE;
}

//-----------------------------------------------------------------------------
BOOL CLibrariesLoader::IsDLLLoading	()
{
	long l = InterlockedCompareExchange(&m_nCounterDLLLoading, 0, 0);
	return l > 0;
}

//-----------------------------------------------------------------------------
void CLibrariesLoader::BeginDLLLoading()
{ 
	InterlockedIncrement(&m_nCounterDLLLoading); 
}

//-----------------------------------------------------------------------------
void CLibrariesLoader::EndDLLLoading()
{ 
	InterlockedDecrement(&m_nCounterDLLLoading); 

#ifdef DEBUG
	g_bNO_ASSERT = FALSE; //avoids loader lock exception during dll initialization
#endif 
}

// checks it the DLL has been already loaded previously
//-----------------------------------------------------------------------------
inline BOOL CLibrariesLoader::IsLibraryLoaded (const CString& aLibName)
{
	TB_OBJECT_LOCK_FOR_READ(&m_LoadedDlls);
	CString sKey (aLibName);	
	LPCTSTR pDummy;
	return m_LoadedDlls.LookupKey(sKey.MakeLower(), pDummy);
}

// checks if the loading library of this namespace has been already invoked
//-----------------------------------------------------------------------------
inline BOOL CLibrariesLoader::IsNamespaceLoaded (const CTBNamespace& aNamespace)
{
	TB_OBJECT_LOCK_FOR_READ(&m_LoadedLibNamespaces);
	LPCTSTR pDummy = NULL;

	return m_LoadedLibNamespaces.LookupKey(aNamespace.ToString(), pDummy);

}

// calculates the physical name of the dll starting from a namespace
//-----------------------------------------------------------------------------
CString	CLibrariesLoader::GetDllPhysicalName (const CString& sLibName)
{
	CString sName = AfxGetPathFinder()->GetTBDllPath() + SLASH_CHAR + sLibName;

	// it adds extension if it not exists
	if	(_tcsicmp(sName.Right(4), szDLLExt))
		sName += szDLLExt;
	
	return sName;
}

// Checks if the DLL has the policies to be loaded
//-----------------------------------------------------------------------------
BOOL CLibrariesLoader::CanBeLoaded (const CTBNamespace& aLibNamespace, const CModuleConfigInfo& aModuleInfo, const LoadLibrariesMode aMode)
{
	CString sLibAlias = aLibNamespace.GetObjectName();
	// the module does not declare the library
	if (!aModuleInfo.HasLibrary(sLibAlias))
	{
		ASSERT(FALSE);
		TRACE(_TB("Attempt to load a library not declared in module.config") + aLibNamespace.ToString());
		return FALSE;
	}

	// check activation of the module
	BOOL bModuleActive = AfxIsActivated(aLibNamespace.GetApplicationName(), aLibNamespace.GetModuleName());

	// addon activation checking must be with sourceFolder attribute.
	if (aModuleInfo.IsAddOnDeployPolicy(sLibAlias))
		return bModuleActive && AfxIsActivated (aLibNamespace);

	// activation rules
	return  (
				!aModuleInfo.IsFullDeployPolicy(sLibAlias) || 
				(bModuleActive && aMode != LoadBaseOnly)
			);
}

// loads the dlls needed by a particular component identified by its namespace
//-----------------------------------------------------------------------------
BOOL CLibrariesLoader::LoadNeededLibraries	(
												const	CTBNamespace&		aComponentNamespace, 
														CTBNamespaceArray*	pLibraries /*=NULL*/,
														LoadLibrariesMode	aMode /*LoadNeeded*/
											)
{
	// checks and optimize repetitive calls
	if (m_bAllLoaded || IsNamespaceLoaded (aComponentNamespace.ToString()))
		return TRUE;

	CTBNamespace::NSObjectType aType = aComponentNamespace.GetType();

	// I have all loaded or I don't have dependencies
	if	(
		aType == CTBNamespace::IMAGE || aType == CTBNamespace::TEXT || 
		aType == CTBNamespace::PDF || aType == CTBNamespace::RTF /*|| aType == CTBNamespace::ODF*/
		)
		return TRUE;

	// I avoid to pass this parameter into all methods
	LoadLibrariesMode mode = aMode;
	
	// if on-demand is disabled by settings, it loads all dlls
	if (!m_bIsOnDemandEnabled && mode == LoadNeeded)
		mode = LoadAll;

	// all libraries or only with base policies
	if (mode == LoadAll || mode == LoadBaseOnly)
		return LoadAllLibraries(mode, pLibraries);

	// an invalid namespace cannot load anything
	if (!aComponentNamespace.IsValid() || !LoadOnDemand(mode, aComponentNamespace, pLibraries))
		return FALSE;
	TB_OBJECT_LOCK(&m_LoadedLibNamespaces);
	m_LoadedLibNamespaces.SetAt(aComponentNamespace.ToString(), NULL);
	return TRUE;
}

// loads all the all the libraries declared into a module depending on the policy
//-----------------------------------------------------------------------------
BOOL CLibrariesLoader::LoadAllLibraries (const LoadLibrariesMode aMode, CTBNamespaceArray* pLibraries /*NULL*/)
{
	ASSERT(AfxGetDiagnostic());

	bool bOk = TRUE;

	AddOnApplication*	pAddOnApp;
	AddOnModule*		pAddOnMod;

	// loop for all applications and modules
	for (int a=0; a <= AfxGetAddOnAppsTable()->GetUpperBound(); a++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(a);
		
		if (pAddOnApp)
			for (int m=0; m <= pAddOnApp->m_pAddOnModules->GetUpperBound(); m++)
			{
				pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(m);

				// loads all the libraries of a single module
				if (pAddOnMod && !LoadOnDemand (aMode, pAddOnMod->m_Namespace))
					return FALSE;
			}
	}
	
	// for all policies I update the global flag
	if (aMode == LoadAll)
		m_bAllLoaded = bOk;
	
	return bOk;
}

// it manages the loading on demand of the dlls
//-----------------------------------------------------------------------------
BOOL CLibrariesLoader::LoadOnDemand (
										const LoadLibrariesMode aMode, 
										const CTBNamespace& aComponentNamespace, 
										CTBNamespaceArray*	pLibraries /*=NULL*/
									)
{
	AfxGetThread()->BeginWaitCursor();

	// I calculate which are the libraries to load 
	CTBNamespaceArray aLibraries;
	GetLibrariesToLoad (aMode, aComponentNamespace, &aLibraries);

	// no libraries to load
	if (!aLibraries.GetSize())
	{
		AfxGetThread()->EndWaitCursor();
		return TRUE;
	}

	CTBNamespace* pLibNs;
	AddOnModule* pAddOnMod;

	// loop on all the libraries to load
	for (int i=0; i <= aLibraries.GetUpperBound(); i++)
	{
		pLibNs = aLibraries.GetAt(i);

		pAddOnMod = AfxGetAddOnModule(*pLibNs);
		if (!pAddOnMod)
		{
			AfxGetDiagnostic()->Add(cwsprintf(_TB("Loading {0-%s} failed, module ownership not found."), pLibNs->ToString()));
			continue;
		}


		// if I cannot load dll I stop any other operation
		if (!LoadLibrary(*pLibNs, pAddOnMod))
		{
			AfxGetDiagnostic()->Add ( cwsprintf(_TB("Loading {0-%s} library failed"), pLibNs->ToString()));
			AfxGetThread()->EndWaitCursor();
			return FALSE;
		}
	}

	// if requested return the array loaded to the caller
	if (pLibraries)
		*pLibraries = aLibraries;

	AfxGetThread()->EndWaitCursor();

	return !AfxGetDiagnostic()->ErrorFound();
}

// loads the DLL in memory
//-----------------------------------------------------------------------------
BOOL CLibrariesLoader::LoadLibrary (const CTBNamespace& aLibNamespace, AddOnModule* pAddOnMod)
{
	ASSERT(pAddOnMod);

	// display the add-on loading message
	AfxSetStatusBarText(cwsprintf(_TB("Loading library {0-%s}... Please wait."), (LPCTSTR) aLibNamespace.ToString()));

	// I have to resolve the source folder used for library name into the real library name
	CTBNamespace aDllNs (aLibNamespace);
	aDllNs.SetObjectName(pAddOnMod->ResolveLibrary(aLibNamespace));

	// checks if the object is the executable object
	if (_tcsicmp(aDllNs.GetObjectName(), szTbLoader) == 0 || IsLibraryLoaded(aDllNs.GetObjectName()))
		return TRUE;

	// I can consider it loaded only if the library has
	// loaded and if the library has failed. If library
	// has failed I mark it to avoid to retry further loads
	// that will fail again.
	CString sKey(aDllNs.GetObjectName());
	sKey.MakeLower();
	CObject* pDummy;

	CString sPhysicalName = GetDllPhysicalName(aDllNs.GetObjectName());

	//////////////////////////////////////////////////////////////////
	// They could be are already loaded in memory. When they are already 
	// loaded and it only executes interface register operations.
	HINSTANCE hAddOn = GetModuleHandle(sPhysicalName);
	if (hAddOn)
	{
		BOOL bOk = AutoRegisterLibrary (aDllNs.ToString(), hAddOn);
		{
			TB_OBJECT_LOCK(&m_LoadedDlls)
			if (!m_LoadedDlls.Lookup(sKey, pDummy))
				m_LoadedDlls.SetAt(sKey, NULL);
		}
		return bOk;
	}
	//////////////////////////////////////////////////////////////////

	// checks if it exists
	if (!ExistFile(sPhysicalName))
	{
		AfxGetDiagnostic()->Add(cwsprintf (_TB("The library {0-%s} does not exist."), (LPCTSTR) sPhysicalName));
		return FALSE;
	}

	// dll loading operation
	try 
	{
#ifdef DEBUG
		CLocalChange<BOOL> _l (g_bNO_ASSERT, TRUE); //avoids loader lock exception during dll initialization
#endif 
		hAddOn = AfxLoadLibrary(GetName(sPhysicalName));

		{
			TB_OBJECT_LOCK(&m_LoadedDlls)
			if (!m_LoadedDlls.Lookup(sKey, pDummy))
				m_LoadedDlls.SetAt(sKey, NULL);
		}
		if (hAddOn == (HINSTANCE)NULL)
		{
			LPVOID lpMsgBuf;
			FormatMessage
			( 
				FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
				NULL,
				GetLastError(),
				MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
				(LPTSTR) &lpMsgBuf,
				0,
				NULL
			);

			AfxGetDiagnostic()->Add(cwsprintf(_TB("Error in function AfxLoadLibrary: {0-%s} loading dll {1-%s}."), (LPCTSTR) lpMsgBuf, (LPCTSTR) sPhysicalName));
			AfxGetDiagnostic()->Add(_TB("Is possible that a library (with dependencies) is absent on disk."), CDiagnostic::Info);
			return FALSE;
		}
	}
	catch (CException* e)
	{
		TCHAR szError[1024];
		e->GetErrorMessage(szError, 1024);
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Error in function AfxLoadLibrary: {0-%s} loading dll {1-%s}"), szError, (LPCTSTR) sPhysicalName));
		AfxGetDiagnostic()->Add(_TB("Is possible that a library (with dependencies) is absent on disk."), CDiagnostic::Info);
		return FALSE;
	}
	
	BOOL bOk = AutoRegisterLibrary (aDllNs.ToString(), hAddOn);
	return bOk;
}

// esegue il codice di creazione e registrazione dell'interface della libreria
//-----------------------------------------------------------------------------
BOOL CLibrariesLoader::AutoRegisterLibrary (const CString& sLibraryNamespace, HINSTANCE hAddOn)
{
#ifdef DEBUG
	CLocalChange<BOOL> _l (g_bNO_ASSERT, TRUE); //avoids loader lock exception during dll initialization
#endif 
	BeginDLLLoading();
	
	AfxSetStatusBarText(cwsprintf(_TB("Registrating library {0-%s}..."), sLibraryNamespace));
	// namespace may contain a dll name or an AddOnLib namespace (in old format)
	// first of all I have to transform all in a dll name
	CTBNamespace aDllNs(CTBNamespace::LIBRARY, sLibraryNamespace);
	ASSERT(aDllNs.IsValid());
	AddOnModule* pMod = AfxGetAddOnModule(aDllNs);
	if (pMod)
		aDllNs.SetObjectName(pMod->ResolveLibrary(aDllNs));

	// if it has been already processed I can skip it
	AddOnDll* pAddOnDll = AfxGetAddOnDll(aDllNs);
	if (pAddOnDll && pAddOnDll->IsRegistered())
	{
		EndDLLLoading();
		return TRUE;
	}
	
	// display the loading message
	AfxSetStatusBarText(cwsprintf(_TB("Registrating library {0-%s}... Please wait."), (LPCTSTR) aDllNs.ToString()));
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(aDllNs.GetApplicationName());
	if (!pAddOnApp)
	{
		AfxGetDiagnostic()->Add(cwsprintf (_TB("Library application {0-%s} not loaded!"), (LPCTSTR) aDllNs.ToString()));
		EndDLLLoading();
		return FALSE;
	}

	// I create the dll with its namespace
	if (!pAddOnDll)
	{
		pAddOnDll = new AddOnDll(hAddOn, aDllNs);
		pAddOnApp->AdAddonDll(pAddOnDll);
	}

	// I have to create all libraries that are referred with this dll
	AddOnModule*	pAddOnMod;
	AddOnLibrary*	pAddOnLib;
	CString			sLibToken;
	BOOL bOk = TRUE;
	
	CStringArray* pLibNs = pAddOnApp->GetAliasesOf(aDllNs.GetObjectName());

	for (int l=0; l <= pLibNs->GetUpperBound(); l++)
	{
		CTBNamespace aAddOnLibNs (pLibNs->GetAt(l));
		sLibToken = aAddOnLibNs.GetObjectName();

		// AddOnLibrary instance
		pAddOnMod = AfxGetAddOnModule(aAddOnLibNs);
		ASSERT (pAddOnMod);
		pAddOnLib = pAddOnMod->GetLibraryFromName(sLibToken);
		if (!pAddOnLib)
		{
			pAddOnLib = new AddOnLibrary(NULL, pAddOnDll, pAddOnMod);
			pAddOnMod->m_pAddOnLibs->Add(pAddOnLib);
			pAddOnLib->	InitLibraryIdentity(aAddOnLibNs);
		}
		
		// addon interface management. Interfaces must be registered only when active.	
		if (pAddOnMod->m_XmlDescription.GetConfigInfo().IsBaseDeployPolicy(sLibToken) ||	// base dll
				(
					pAddOnMod->m_XmlDescription.GetConfigInfo().IsFullDeployPolicy(sLibToken) && AfxIsActivated(pAddOnLib->GetApplicationName(), pAddOnLib->GetModuleName()) || // full dll
					(pAddOnMod->m_XmlDescription.GetConfigInfo().IsAddOnDeployPolicy(sLibToken) && AfxIsActivated(aAddOnLibNs)) // addon dll
				)
			)
		{
			pAddOnLib->m_pAddOn = BindAddOnInterface (pAddOnLib, pAddOnDll);
			if (pAddOnLib->m_pAddOn->GetRegisterState() == AddOnInterfaceObj::NotRegistered)
				bOk = RegisterAddOnInterface(pAddOnLib, pAddOnMod) && bOk;
		}
	}
	delete pLibNs;
	if (bOk)
		pAddOnDll->SetRegistered();

	EndDLLLoading();

	return bOk;
}

//-----------------------------------------------------------------------------
AddOnInterfaceObj* CLibrariesLoader::BindAddOnInterface (AddOnLibrary* pAddOnLib, AddOnDll* pAddOnDll)
{
	// an existing interface
	if (pAddOnLib && pAddOnLib->m_pAddOn)
		return pAddOnLib->m_pAddOn;

	CString sName (pAddOnLib->GetModuleName() + pAddOnLib->GetLibraryName());
	sName = sName.MakeLower() ;
	CStringA addOnFunctName (szTaskBuilderAddOnHead);
	addOnFunctName += (CStringA) sName;
	addOnFunctName += szTaskBuilderAddOnTail;

	// multiple interface management
	TB_ADDON lpInitDLL = (TB_ADDON)::GetProcAddress (pAddOnDll->m_hInstance, addOnFunctName);

	AddOnInterfaceObj* pInterface;
	if (lpInitDLL == NULL) 
	{
		// old single interface management
		addOnFunctName = szTaskBuilderAddOnHead;
		addOnFunctName += szTaskBuilderAddOnTail;
		
		lpInitDLL = (TB_ADDON)::GetProcAddress (pAddOnDll->m_hInstance, addOnFunctName);
		if (!lpInitDLL)
			return new AddOnInterfaceObj();
	}

	// AddOnInterfaceObj instance
	pInterface = (*lpInitDLL)();
	
	return pInterface;
}

//-----------------------------------------------------------------------------
BOOL CLibrariesLoader::RegisterAddOnTable(AddOnLibrary* pAddOnLib)
{
	COleDbManager *pOleDb = AfxGetOleDbMng();
	// Table registration. This is performed for all the connections that manages register table
	if (pOleDb && !pOleDb->RegisterAddOnTable	(pAddOnLib))
	{
		AfxGetDiagnostic()->Add
				(
					cwsprintf(
								_TB("Missing tables in {0-%s} library.\r\nPlease contact your system administrator."),	
								pAddOnLib->GetModuleName() + _T(": ") + pAddOnLib->GetLibraryName()
								)
				);
		pAddOnLib->m_pAddOn->SetRegisterState(AddOnInterfaceObj::NotRegistered);
		AfxGetLoginContext()->Lock ();
		return FALSE;
	}
	
	return TRUE;
}
// Register of AddOnInterfaceObj objects
//-----------------------------------------------------------------------------
BOOL CLibrariesLoader::RegisterAddOnInterface (AddOnLibrary* pAddOnLib, AddOnModule* pAddOnMod)
{
	// DLL initialization code
	pAddOnLib->m_pAddOn->AOI_InitDLL();

	// database release
	pAddOnLib->m_pAddOn->AOI_RegisterOSLObjects();
	int nDBRel = pAddOnLib->m_pAddOn->AOI_DatabaseRelease();
	BOOL bCheckDB = FALSE;
	if (nDBRel > -1)
	{
		if (pAddOnMod->m_nDatabaseRel > -1 && pAddOnMod->m_nDatabaseRel != nDBRel)
			AfxGetDiagnostic()->Add(cwsprintf
				(
					_TB("Database release for the {0-%s} module already assigned.\r\nValue {1-%d}"), 
					(LPCTSTR) pAddOnLib->m_Namespace.ToString(), 
					nDBRel
				)
			);
		else
		{
			pAddOnMod->m_nDatabaseRel = nDBRel;		
			bCheckDB = TRUE;
		}
	}

	if (pAddOnLib->m_pAddOn->GetRegisterState() == AddOnInterfaceObj::Registered)
		return TRUE;

	pAddOnLib->m_pAddOn->SetRegisterState(AddOnInterfaceObj::Registering);

	pAddOnLib->m_pAddOn->AOI_BeginRegistration	(pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterGlobalObjects();

	//correzione al problema segnalato da artware sugli addonfields: se faccio due login, poi carico un documento
	//che scatena il caricamento di una dll, la registrazione delle tabelle veniva effettuata solo per 
	//il login context corrente e non per tutti
	
	//mi faccio dare l'elenco degli id dei login thread attivi
	CDWordArray arIds;
	AfxGetApplicationContext()->GetLoginContextIds(arIds);
	BOOL bOk = TRUE;
	for (int i = 0; i < arIds.GetCount(); i++)
	{
		try
		{
			//ATTENZIONE: NON POSSO FARE UNA CHIAMATA SU AD ALTRO THREAD SINCRONA PERCHE SE SONO
			//IN CARICAMENTO DLL SI VERIFICA UN PROBLEMA DI LOADER LOCK CAUSATO DALLA PUMPTHREADMESSAGES
			//QUINDI SE IL LOGIN CONTEXT E' IL MIO, LO FACCIO NEL THREAD CORRENTE COME PRIMA, ALTRIMENTI FACCIO
			//UNA CHIAMATA ASINCRONA
			DWORD dwThreadId = arIds[i];
			CLoginContext* pLoginContext = AfxGetLoginContext();
			if (pLoginContext && dwThreadId == pLoginContext->m_nThreadID)
				bOk = RegisterAddOnTable(pAddOnLib);
			//registro le tabelle per ognuno dei login thread (faccio una dispatch della chiamata su quel thread per 
			//evitare problemi di accesso concorrente)
			else
				AfxInvokeAsyncThreadFunction<BOOL, CLibrariesLoader, AddOnLibrary*>(dwThreadId, this, &CLibrariesLoader::RegisterAddOnTable, pAddOnLib);
		}
		catch (CThreadCallFailedException* e)
		{
			//il thread potrebbe essere morto, quindi è normale che fallisca la chiamata
			e->Delete();
		}
	}
	if (!bOk) 
		return FALSE;

	pAddOnLib->m_pAddOn->AOI_RegisterTemplates			(IDR_EXTDOC, &pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterFunctions			(&pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterHotLinks			(pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterItemSources		(pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterValidators			(pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterDataAdapters		(pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterContextMenus		(pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterFormatters			(pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterControlBehaviours	(pAddOnLib->m_Namespace);
	pAddOnLib->m_pAddOn->AOI_RegisterParsedControls	(pAddOnLib->m_Namespace);

	bOk = pAddOnLib->m_pAddOn->AOI_EndRegistration(pAddOnLib->m_Namespace);

	pAddOnLib->m_pAddOn->SetRegisterState(AddOnInterfaceObj::Registered);
	
	// bugfix #17185
	// checks database release
	
	if (bCheckDB)
	{
		COleDbManager *pOleDb = AfxGetOleDbMng();
		if (pOleDb)
			bOk = bOk && pOleDb->CheckAddOnModuleRelease(pAddOnMod, AfxGetDiagnostic());
	}

	return bOk;
}

// It calculates which are the dlls needed to execute the component namespace
// The dependent dlls will be automatically loaded by the linker and they will
// performs the autoregister procedure called into their DllMain code
//-----------------------------------------------------------------------------
void CLibrariesLoader::GetLibrariesToLoad (const LoadLibrariesMode aMode, const CTBNamespace& aComponentNS, CTBNamespaceArray* pLibraries)
{
	AddOnModule* pAddOnMod = AfxGetAddOnModule(aComponentNS);
	
	if (!pAddOnMod)
		return;

	switch (aComponentNS.GetType())
	{
		// AddNewFields dependencies managed for the tables
		case CTBNamespace::TABLE:
			// I start the sequence with the dll of the component requested
			AddLibrariesToLoad		(aMode, aComponentNS, pLibraries, pAddOnMod);
			AddNewFieldsLibraries	(aMode, aComponentNS, pLibraries);
			break;

		// Client Documents dependencies managed for the documents
		case CTBNamespace::DOCUMENT:
			// I start the sequence with the dll of the component requested
			AddLibrariesToLoad		(aMode, aComponentNS, pLibraries, pAddOnMod);
			AddClientDocsLibraries	(aMode, aComponentNS, pLibraries, pAddOnMod);
			break;
		
		// for the following types the request is to load all the libraries of the module. 
		case CTBNamespace::REPORT:
		case CTBNamespace::MODULE:
		{
			CStringArray* pModLibs = pAddOnMod->m_XmlDescription.GetConfigInfo().GetLibrariesAliases ();
			if (!pModLibs)
				break;

			CTBNamespace aLibNs (pAddOnMod->m_Namespace);
			aLibNs.SetType (CTBNamespace::LIBRARY);
			for (int i=0; i <= pModLibs->GetUpperBound(); i++)
			{
				aLibNs.SetObjectName (pModLibs->GetAt(i));
				AddLibrariesToLoad (aMode, aLibNs, pLibraries, pAddOnMod);
			}

			delete pModLibs;
			break;
		}
		default:
			// I add only the dll of the component requested 
			AddLibrariesToLoad (aMode, aComponentNS, pLibraries, pAddOnMod);
	}
}

// It calculates and add the libraries of the client documents for the specified server
// Client Documents dependencies managed for the documents are declared into Xml 
// Description. I start from it I have to check and load all the client documents 
// attached to the documents. Family client documents can have more that one server
//-----------------------------------------------------------------------------
void CLibrariesLoader::AddClientDocsLibraries
									(
										const LoadLibrariesMode aMode,
										const CTBNamespace&	aDocumentNamespace, 
										CTBNamespaceArray*	pLibraries,
										AddOnModule*		pAddOnMod
									)
{
	// xml description of the document
	const CDocumentDescription* pDocInfo = AfxGetDocumentDescription(aDocumentNamespace);

	if (!pDocInfo)
		return;

	CServerDocDescription*	pServerInfo;
	CTBNamespace* pNamespace;
	AddOnModule* pCDAddOnMod;

	// server document loop 
	for (int i=0; i <= AfxGetClientDocsTable()->GetUpperBound(); i++)
	{
		pServerInfo = AfxGetClientDocsTable()->GetAt(i); 

		if (pServerInfo &&  
			 (
				// is a non family clientdoc 
				pServerInfo->GetNamespace() == aDocumentNamespace ||
				// is a family clientdoc 
				(!pDocInfo->IsExcludedFromFamily() && pServerInfo->IsHierarchyOf(pDocInfo->GetClassHierarchy()))
			  )
			)
		{
			// client document libraries list loop 
			for (int i=0; i <= pServerInfo->GetLibraries().GetUpperBound(); i++)
			{
				pNamespace = pServerInfo->GetLibraries().GetAt(i);
				if (!pNamespace)
					continue;

				// i get the addon-module of the client doc
				pCDAddOnMod = AfxGetAddOnModule(*pNamespace);
				if (pCDAddOnMod)
					AddLibrariesToLoad (aMode, *pNamespace, pLibraries, pCDAddOnMod);
			}
		}
	}
}

// It calculates and add the libraries of the sqlAddOnFieldsColumns for the specified table
//-----------------------------------------------------------------------------
void CLibrariesLoader::AddNewFieldsLibraries
									(
										const LoadLibrariesMode aMode, 
										const CTBNamespace&		aTableNS,
										CTBNamespaceArray*	pLibraries
									)
{
	const CAddColsTableDescription* pNewColsInfo = AfxGetAddOnFieldsOnTable(aTableNS);

	if (!pNewColsInfo)
		return;
	
	// all alter table sequences
	for (int i=0; i < pNewColsInfo->m_arAlterTables.GetSize(); i++)
	{
		CAlterTableDescription* pNewField = (CAlterTableDescription*) pNewColsInfo->m_arAlterTables.GetAt(i);
		if (pNewField)
			AddLibrariesToLoad (aMode, pNewField->GetNamespace(), pLibraries, AfxGetAddOnModule(pNewField->GetNamespace()));
	}
}

// if adds the library requested into the dependency array checking the 
// correct policies and activation state
//-----------------------------------------------------------------------------
void CLibrariesLoader::AddLibrariesToLoad	(
												const LoadLibrariesMode aMode,
												const CTBNamespace& aComponentNS,
												CTBNamespaceArray*	pLibraries,
												AddOnModule*		pAddOnMod
											)
{
	if (aComponentNS.HasAFakeLibrary())
		return;

	CTBNamespace aLibNs (CTBNamespace::LIBRARY, aComponentNS.GetApplicationName() + CTBNamespace::GetSeparator() + aComponentNS.GetModuleName());
	aLibNs.SetObjectName (aComponentNS.GetObjectName(CTBNamespace::LIBRARY));

	// already loaded
	if (!pAddOnMod)
		return;

	// I have to resolve the source folder used for library name into the real library name
	CTBNamespace aDllNs (aLibNs);
	aDllNs.SetObjectName(pAddOnMod->ResolveLibrary(aLibNs));

	// if it is loaded I dont't check policies
	if (IsLibraryLoaded(aDllNs.GetObjectName()))
		return;

	if (CanBeLoaded(aLibNs, pAddOnMod->m_XmlDescription.GetConfigInfo(), aMode))
		pLibraries->AddIfNotExists(aLibNs);
}
