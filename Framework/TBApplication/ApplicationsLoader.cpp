#include "stdafx.h"

#include <TbXmlCore\XmlSaxReader.h>

#include <TbNameSolver\TbNamespaces.h>
#include <TbNameSolver\PathFinder.h>
#include <TbGenlib\Behaviour.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbNameSolver\IFileSystemManager.h>
#include <TBClientCore\ModuleConfigInfo.h>

#include <TbGeneric\Critical.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FormatsTable.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\DataObjDescription.h>
#include <TbGeneric\DatabaseObjectsInfo.h>
#include <TbGeneric\FormatsHelpers.h>
#include <TbGeneric\NumberToLiteral.h>
#include <TbGeneric\OutDateObjectsInfo.h>
#include <TbGeneric\ParametersSections.h>

#include <TbGenlib\baseapp.h>

#include <TbOleDb\PerformanceAnalizer.h>

#include <TbParser\EnumsParser.h>
#include <TbParser\FormatsParser.h>
#include <TbParser\FontsParser.h>
#include <TbParser\XmlNumberToLiteralParser.h>
#include <TbParser\XmlAddOnDatabaseObjectsParser.h>
#include <TbParser\XmlDocumentObjectsParser.h>
#include <TbParser\XmlSettingsParser.h>
#include <TbParser\XmlOutDateObjectsParser.h>
#include <TbParser\XmlDynamicDbObjectsParser.h>
#include <TbGenlib\Behaviour.h>

#include <TbGenlib\FunProto.h>

#include <TbGenlib\messages.h>

#include <TbGes\CustomSaveDialog.h>

//................................. Main program 
#include "TbCommandManager.h"
#include "TaskBuilderApp.h"
#include "ApplicationsLoader.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const TCHAR szExtConfig[]			= _T(".config");
static const TCHAR szXmlApplicationsKey[]	= _T("/Configuration/Applications");
static const TCHAR szXmlName[]				= _T("name");
static const TCHAR szXmlApplication[]		= _T("Application");
static const TCHAR szXmlModule[]			= _T("Module");

// utility class to have only one instance of the parser objects during loading
// operations. It is removed from memory when operations are closed.
//==============================================================================
class CParsersForModule
{
public:
	// utility
	CStatusBarMsg*	m_pStatusBar;
	CPathFinder*	m_pPathFinder;

	// parsers
	CXMLDocumentObjectsParser			m_DocObjectParser;
	CXMLClientDocumentObjectsContent	m_ClientDocParser;
	CXmlEnumsContent					m_EnumsParser;
	CXMLSaxReader						m_SaxReader;
	FormatsParser						m_FormatsParser;
	FontsParser							m_FontsParser;

public:
	CParsersForModule(CPathFinder* pPathFinder, CStatusBarMsg* pStatusBar)
		:
		m_pPathFinder(pPathFinder),
		m_pStatusBar(pStatusBar)
	{
	}
};



/////////////////////////////////////////////////////////////////////////////
//					CApplicationsLoader implementation
/////////////////////////////////////////////////////////////////////////////
//

// AddOnApplications loading operations
//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::LoadApplications (CStatusBarMsg*	pStatusBar)
{
	CPerformanceCrono aCrono;

	aCrono.Start();
	CBaseApp* pBaseApp = AfxGetBaseApp();
	
	CPathFinder* pPathFinder = AfxGetPathFinder();

	CStringArray arApps;

	pPathFinder->GetCandidateApplications(&arApps);

	// check for duplicate applications
	if (ThereDuplicatesApplications(&arApps))

	AfxGetApplicationContext()->SetCustomSaveDialogClass(RUNTIME_CLASS(CCustomSaveDialog));

	CParsersForModule aParsers (pPathFinder, pStatusBar);

	// AddOnApplications create operations
	for (int i = 0; i <= arApps.GetUpperBound(); i++)
		if (!arApps.GetAt(i).IsEmpty())
			LoadSingleApplication(arApps.GetAt(i), aParsers);	
	
	LoadApplicationsStep2(pStatusBar);
	aCrono.Stop();	
	TRACE(_T("Load StartUp files total time: %s\n\r"), (LPCTSTR)aCrono.GetFormattedElapsedTime());

	// if ok set the master application
	if (AfxGetAddOnAppsTable()->GetSize() > 1 && pBaseApp->GetMasterAddOnApp())
	{
		CString strTitle = AfxGetLoginManager()->GetMasterProductBrandedName();
		if (strTitle.IsEmpty())
			strTitle = pBaseApp->GetMasterAddOnApp()->GetTitle ();
		AfxGetApplicationContext()->SetAppTitle(strTitle);
	}
	
	// developers final init operations
	SetAddOnModules();
	
	InitializeTaskBuilderAddOn (*pStatusBar);

	return TRUE;
}

// AddOnApplications loading operations
//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::LoadApplicationsStep2 (CStatusBarMsg*	pStatusBar)
{
	AddOnApplication*	pAddOnApp;
	AddOnModule*		pAddOnMod;
	// loads dynamic objects descriptions 
	CXMLDatabaseObjectsParser aDbParser;
	
	CPerformanceCrono aCrono;
	for (int a = 0; a < AfxGetAddOnAppsTable()->GetSize(); a++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(a);
		if (!pAddOnApp)
			continue;

		for (int m = 0; m < pAddOnApp->m_pAddOnModules->GetSize(); m++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(m);
			if (!pAddOnMod)
				continue;

			pAddOnMod->m_XmlDescription.LoadFunctionsObjects();
			pAddOnMod->m_XmlDescription.LoadEventHandlerObjects();
			pAddOnMod->m_XmlDescription.LoadReferenceObjects();

		}

	}
	int nFileCount = 0;
	aCrono.Start();
	for (int a = 0; a < AfxGetAddOnAppsTable()->GetSize(); a++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(a);
		if (!pAddOnApp)
			continue;

		for (int m = 0; m < pAddOnApp->m_pAddOnModules->GetSize(); m++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(m);
			if (!pAddOnMod)
				continue;
			//lock scope
			DatabaseObjectsTablePtr pTablePtr = AfxGetWritableDatabaseObjectsTable();
			aDbParser.LoadDatabaseObjects(pAddOnMod->m_Namespace, pTablePtr.GetPointer());
			nFileCount++;
		}	
	}
	aCrono.Stop();
	TRACE(_T("Load DatabaseObjects %d files total time: %s\n\r"), nFileCount, (LPCTSTR)aCrono.GetFormattedElapsedTime());


	//devo farlo DOPO avere caricato TUTTI i DatabaseObjects di TUTTE le appa!
	CXMLAddOnDatabaseObjectsParser addOnDbParser;
	CPathFinder* pPathFinder = AfxGetPathFinder();
	for (int i=0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);

		if (!pAddOnApp)
			continue;

		AddOnModule* pAddOnMod;
		for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
			addOnDbParser.LoadAdddOnDatabaseObjects(pAddOnMod->m_Namespace);
		}
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::IntergrateLoginDefinitions (CStatusBarMsg* pStatusBar)
{
	AddOnApplication*	pAddOnApp;
	AddOnModule*		pAddOnMod;
	
	CParsersForModule aParsers (AfxGetPathFinder(), pStatusBar);

	BOOL bOk = TRUE;

	for (int i=0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);

		if (!pAddOnApp)
			continue;

		for (int n = 0; n <= pAddOnApp->m_pAddOnModules->GetUpperBound(); n++)
		{
			pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(n);
			if (!pAddOnMod)
				continue;
			
			if (!IntegrateModuleDefinitions(pAddOnMod, aParsers))
				bOk = FALSE;
		}
	}

	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::ReloadApplication (const CString& sAppName)
{
	CStatusBarMsg statusBar(TRUE, TRUE, FALSE); // disabilita, hourglass
	CParsersForModule aParsers (AfxGetPathFinder(), &statusBar);
	//Ripulisco la cache dei moduli in modo che venga ricaricata l'applicazione correttamente
	AfxGetPathFinder()->ClearApplicationsModulesMap();

	return LoadSingleApplication(sAppName, aParsers);
}

//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::LoadSingleApplication (const CString& strAppName, CParsersForModule& aParsers)
{
	CStringArray aAppModules;

	AfxGetPathFinder()->GetCandidateModulesOfApp(strAppName, &aAppModules);

	if (aAppModules.GetSize() <= 0)
		return FALSE;
	// application check
	AddOnApplication* pAddOnApp = AfxGetAddOnApp(strAppName);
	if (!pAddOnApp)
	{
		pAddOnApp = new AddOnApplication();
		pAddOnApp->AOI_DeclareAddOnApp(strAppName) ;

		if	(
				LoadApplicationInfo (pAddOnApp, aParsers) &&
				pAddOnApp->m_XmlDescription.m_Info.IsTbApplication ()
			)
			AfxGetApplicationContext()->GetObject<AddOnAppsArray>(&CApplicationContext::GetAddOnApps)->Add(pAddOnApp);
		else
		{
			if (pAddOnApp->m_XmlDescription.m_Info.IsTbApplication())
				AfxGetDiagnostic()->Add (strAppName + _TB(": application not loaded due to missing or invalid XML declaration in the Application.config"), CDiagnostic::Warning);
			
			delete pAddOnApp;
			return FALSE;
		}
	}

	// application could exists 
	AddOnModsArray* pAddOnMods;
	if (pAddOnApp && pAddOnApp->m_pAddOnModules)
		pAddOnMods = pAddOnApp->m_pAddOnModules;
	else
		pAddOnMods = new AddOnModsArray;

	CBaseApp* pBaseApp = AfxGetBaseApp();
	// modules loading
	AddOnModule* pAddOnMod;
	for (int i=0; i <= aAppModules.GetUpperBound(); i++)
	{
		CTBNamespace aModuleNS(CTBNamespace::MODULE, strAppName + CTBNamespace::GetSeparator() + aAppModules.GetAt(i));
		
		// existing moodules are validated
		pAddOnMod = AfxGetAddOnModule(aModuleNS);

		if (pAddOnMod)
		{
			pAddOnMod->SetValid(TRUE);
			LoadStandardComponents(pAddOnMod, aParsers);
			pBaseApp->ValidateTemplates(pAddOnMod->m_Namespace);
			continue;
		}

		pAddOnMod = LoadSingleModule(strAppName, aAppModules.GetAt(i), aParsers, pAddOnMods, pAddOnApp);
		if (pAddOnMod)
		{
			pAddOnMods->Add(pAddOnMod);
			// i try to load localizableapplication.config file
			LoadLocalizableApplicationInfo (pAddOnApp, pAddOnMod, aParsers);

			CTBNamespace aLibNs (CTBNamespace::LIBRARY, pAddOnMod->GetApplicationName() + CTBNamespace::GetSeparator () + pAddOnMod->GetModuleName());
			const CModuleConfigLibrariesInfo& aLibrariesInfo = pAddOnMod->m_XmlDescription.GetConfigInfo().GetLibrariesInfo();

			CModuleConfigLibraryInfo* pInfo;
			CString sName, sAlias;
			for (int i=0; i <= aLibrariesInfo.GetUpperBound(); i++)
			{
				pInfo = aLibrariesInfo.GetAt (i);
		
				sName = pInfo->GetLibraryName();
				sName = sName.MakeLower();
		
				aLibNs.SetObjectName (pInfo->GetAlias());
				sAlias = aLibNs.ToString();
				sAlias.MakeLower();
		
				pAddOnApp->AddAlias (sName, sAlias);
			}
		}
	}

	// no application exists without AddOnModules
	if (pAddOnMods->GetSize() == 0)
	{
		DestroyModules(pAddOnMods);
		delete pAddOnMods;
		return FALSE;
	}

	// sets the valid modules
	pAddOnApp->SetAddOnModArray(pAddOnMods);

	return TRUE;
}

//-----------------------------------------------------------------------------
AddOnModule* CApplicationsLoader::LoadSingleModule 
		(
			const	CString&			strAddOnAppName,
			const	CString&			strAddOnName,
					CParsersForModule&	aParsers,
					AddOnModsArray*		pModules,
					AddOnApplication*	pAddOnApp
		)
{
	// first of all we need to init the module namespace
	AddOnModule* pAddOnMod = new AddOnModule (pAddOnApp);
	pAddOnMod->InitModuleIdentity(strAddOnName, strAddOnAppName);

	aParsers.m_pStatusBar->Show(cwsprintf(_TB("Loading extension: {0-%s}"), (LPCTSTR) strAddOnName));

	// loading module definition
	if	(!LoadModuleInfo(pAddOnMod, aParsers))
	{
		AfxGetDiagnostic()->Add (strAddOnAppName + _T(", ") + strAddOnName + _TB(": module not loaded due to missing or invalid XML declaration in the Module.config."), CDiagnostic::Error);
		delete pAddOnMod;
		return NULL;
	}

	// duplicate check
	if (IsDuplicateModule (pAddOnMod, pModules))
	{
		delete pAddOnMod;
		return NULL;
	}

	// module components
	if (LoadStandardComponents (pAddOnMod, aParsers))
		return pAddOnMod;

	delete pAddOnMod;
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::IntegrateModuleDefinitions (AddOnModule* pAddOnMod, CParsersForModule& aParsers)
{
	BOOL bOk = TRUE;


	// custom formatters
	{ //lock scope
		FormatStyleTablePtr formatTable = AfxGetWritableFormatStyleTable();
		aParsers.m_FormatsParser.LoadFormats 
			(
				formatTable.GetPointer(), 
				pAddOnMod->m_Namespace, 
				aParsers.m_pPathFinder, 
				aParsers.m_pStatusBar, 
				CPathFinder::CUSTOM
			);
	}

	// custom fonts
	{ //lock scope
		FontStyleTablePtr fontTable = AfxGetWritableFontStyleTable();
		aParsers.m_FontsParser.LoadFonts
			(
				fontTable.GetPointer(), 
				pAddOnMod->m_Namespace, 
				aParsers.m_pPathFinder, 
				aParsers.m_pStatusBar, 
				CPathFinder::CUSTOM
			);
		aParsers.m_FontsParser.LoadFontAlias(fontTable.GetPointer());
	}

	// Settings 
	XMLSettingsParser aSettingParser;
	aSettingParser.LoadSettings 
		(
			pAddOnMod->m_Namespace, 
			AfxGetPathFinder(), 
			AfxGetLoginInfos()->m_strUserName, 
			pAddOnMod->m_XmlDescription.GetOutDateObjectsInfo().GetSettings(), 
			aParsers.m_pStatusBar
		);
	const_cast<COutDateObjectsDescription&>(pAddOnMod->m_XmlDescription.GetOutDateObjectsInfo()).ClearSettings();
	
	return bOk;
}

//-----------------------------------------------------------------------------
void CApplicationsLoader::SetAddOnModules()
{
	for (int i = 0; i <= AfxGetAddOnAppsTable()->GetUpperBound(); i++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(i);
		ASSERT(pAddOnApp);
		pAddOnApp->AOI_SetAddOnMod();
	}
}

// Verifica che non ci siano duplicati di nomi nelle applicazioni poichè
// l'application name deve essere univoco. Potrei sistemare io l'array e
// rimuovere i duplicati, ma preferisco fermare subito l'applicativo per
// evitare eventuali problemi: rimuovo quella buona, carico xml misti ecc..
//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::ThereDuplicatesApplications (CStringArray* pApps)
{
	BOOL bDuplicates= FALSE;

	if (!pApps || pApps->GetSize() == 0)
		return bDuplicates;

	CString sName;
	for (int i=0; i <= pApps->GetUpperBound(); i++)
	{
		sName = pApps->GetAt(i);

		for (int n=i+1; n <= pApps->GetUpperBound(); n++)
		{
			// same application
			if (_tcsicmp(sName, pApps->GetAt(n)))
				continue;

			AfxGetDiagnostic()->Add(cwsprintf
						(
							_TB("There are several applications called {0-%s} to be loaded in the TaskBuilder directory and Applications\r\nApplication names must be univocal; unable to run the program.\r\n"), 
							(LPCTSTR) sName
						), 
						CDiagnostic::Error
					);
			bDuplicates	= TRUE;
			break;
		}
	}

	return bDuplicates;
}

//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::IsDuplicateModule	(AddOnModule* pAddOnMod, AddOnModsArray* pModules)
{
	if (!pAddOnMod || !pModules)
		return FALSE;

	CString sModNamespace = pAddOnMod->m_Namespace.ToString();
	CString sLibNames	  = pAddOnMod->m_XmlDescription.GetConfigInfo().GetLibrariesNames();

	sModNamespace	= sModNamespace.MakeLower ();

	// first module
	if (m_sLoadedModuleLibraries.IsEmpty())
	{
		m_sLoadedModuleLibraries = sLibNames;
		m_sLoadedModules		 = CModuleConfigInfo::GetNamesSeparator () + sModNamespace;
		return FALSE;
	}

	// same namespace
	if (m_sLoadedModules.Find (CModuleConfigInfo::GetNamesSeparator () + sModNamespace + CModuleConfigInfo::GetNamesSeparator ()) >= 0)
	{
		AfxGetDiagnostic()->Add 
			(
				cwsprintf(_TB("{0-%s}: already exist in memory a module with the same namespace"), sModNamespace),
				CDiagnostic::Error
			);
		return TRUE;
	}

	CStringArray* pLibNames = pAddOnMod->m_XmlDescription.GetConfigInfo().GetLibraries();

	CString sLibraryToFind;
	BOOL bDuplicate = FALSE;
	for (int i=0; i <= pLibNames->GetUpperBound(); i++)
	{
		sLibraryToFind = CModuleConfigInfo::GetNamesSeparator() + pLibNames->GetAt(i) + CModuleConfigInfo::GetNamesSeparator();
		sLibraryToFind = sLibraryToFind.MakeLower();

		if (m_sLoadedModuleLibraries.Find (sLibraryToFind) < 0)
			continue;

		AfxGetDiagnostic()->Add 
			(
				cwsprintf(_TB("Definition of library {0-%s} is duplicate: it already exist in a previous loaded module!"), pLibNames->GetAt(i)),
				CDiagnostic::Error
			);
		AfxGetDiagnostic()->Add 	
		(
			cwsprintf(_TB("Loading of module {0-%s}: definition duplicate!\nModule no will be loaded owing to following errors:"), 
			sModNamespace),
			CDiagnostic::Error
		);
		bDuplicate = TRUE;
		break;
	}

	SAFE_DELETE(pLibNames);
	
	if (!bDuplicate)
	{
		m_sLoadedModuleLibraries += sLibNames;
		m_sLoadedModules		 += CModuleConfigInfo::GetNamesSeparator () + sModNamespace;
	}
	return bDuplicate;
}

//-----------------------------------------------------------------------------
void CApplicationsLoader::DestroyModules (AddOnModsArray* pAddOnMods)
{
	if (!pAddOnMods)
		return;

	for (int n = 0; n <= pAddOnMods->GetUpperBound(); n++)
	{
		AddOnModule* pAddOnMod = pAddOnMods->GetAt(n);

		for (int i = 0; i <= pAddOnMod->m_pAddOnLibs->GetUpperBound(); i++)
		{
			AddOnLibrary* pAddOnLib = pAddOnMod->m_pAddOnLibs->GetAt(i);
			ASSERT(pAddOnLib && pAddOnLib->m_pAddOn);
			pAddOnLib->m_pAddOn->UnloadRegistrationInfo ();

			// devo deletare qui perche la dll che contine l'oggetto e' ancora viva
			delete pAddOnLib->m_pAddOn;
		}
	}
}

// NB: loading sequence has to be enums, formatter, fonts as into formatters 
// there could be enums depending expresions.
// I prefer consider enums loading as warnings
//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::LoadStandardComponents (AddOnModule* pAddOnMod, CParsersForModule& aParsers)
{
	if (!pAddOnMod)
		return FALSE;

	CPathFinder::PosType aPosType = AfxGetPathFinder()->GetDefaultPosTypeFor(pAddOnMod->m_pApplication->IsACustomization());
	LoadEnums	
	(
		pAddOnMod, 
		aParsers, 
		CPathFinder::STANDARD, 
		const_cast<EnumsTable*>(AfxGetStandardEnumsTable())
	);

		// custom enums
	{ //lock scope
		EnumsTablePtr enumsTable = AfxGetWritableEnumsTable();
		LoadEnums(pAddOnMod, aParsers, CPathFinder::CUSTOM, enumsTable.GetPointer());
	}

	CXMLOutDateObjectsParser m_OutDateObjectParser;
	m_OutDateObjectParser.Load(const_cast<COutDateObjectsDescription*>(&pAddOnMod->m_XmlDescription.GetOutDateObjectsInfo()), pAddOnMod->m_Namespace, AfxGetPathFinder());

	if (pAddOnMod->m_XmlDescription.GetConfigInfo().HasLibraries())
		LoadAddOnComponents (pAddOnMod, aParsers);

	// standard formatters
	aParsers.m_FormatsParser.LoadFormats 
		(
			const_cast<FormatStyleTable*>(AfxGetStandardFormatStyleTable()), 
			pAddOnMod->m_Namespace, 
			AfxGetPathFinder(), 
			aParsers.m_pStatusBar, 
			aPosType
		);
	
	// standard fonts
	aParsers.m_FontsParser.LoadFonts 
		(
			const_cast<FontStyleTable*>(AfxGetStandardFontStyleTable()), 
			pAddOnMod->m_Namespace, 
			AfxGetPathFinder(), 
			aParsers.m_pStatusBar, 
			aPosType
		);



	{ //lock scope
		DocumentObjectsTablePtr pTablePtr = AfxGetWritableDocumentObjectsTable();
		aParsers.m_DocObjectParser.LoadDocumentObjects (
			pAddOnMod->m_Namespace, 
			aPosType,
			pTablePtr.GetPointer());
	}

	// behaviours
	CString sFile = AfxGetPathFinder()->GetBehaviourObjectsFullName(pAddOnMod->m_Namespace, aPosType);
	if (ExistFile(sFile))
	{
		CBehavioursContent aContent;
		CXMLSaxReader aReader;
		aReader.AttachContent(&aContent);
		aReader.ReadFile(sFile);
	}
	return TRUE;
}


//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::LoadEnums	(AddOnModule* pAddOnMod, CParsersForModule& aParsers, CPathFinder::PosType aPos, EnumsTable* pTable)
{
	// Enums files
	CString sFileName = aParsers.m_pPathFinder->GetEnumsFullName (pAddOnMod->m_Namespace, aPos);
	if (!ExistFile(sFileName))
		return TRUE;

	aParsers.m_pStatusBar->Show(cwsprintf(_TB("Loading Enums for the Module {0-%s}..."), pAddOnMod->m_Namespace.ToString()));
	aParsers.m_EnumsParser.SetCurrentModule (pAddOnMod->m_Namespace);
	aParsers.m_EnumsParser.AttachTable (pTable);
	aParsers.m_SaxReader.AttachContent (&aParsers.m_EnumsParser);
	aParsers.m_SaxReader.ReadFile(sFileName);
	return TRUE;
}

// I manage the errors like warnings as I can skip the AddOnApplication
//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::LoadApplicationInfo (AddOnApplication* pAddOnApp, CParsersForModule& aParsers)
{
	if (!pAddOnApp || pAddOnApp->m_strAddOnAppName.IsEmpty ())
		return FALSE;

	aParsers.m_pStatusBar->Show(cwsprintf(_TB("Loading Application {0-%s}..."), pAddOnApp->m_strAddOnAppName));

	// application config must exist as it has been just checked
	CApplicationConfigContent aAppContent (&pAddOnApp->m_XmlDescription.m_Info);
	aParsers.m_SaxReader.AttachContent(&aAppContent);
	CString sFileName = aParsers.m_pPathFinder->GetApplicationConfigFullName(pAddOnApp->m_strAddOnAppName);

	return aParsers.m_SaxReader.ReadFile(sFileName);
}

//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::LoadLocalizableApplicationInfo
											(
												AddOnApplication*	pAddOnApp, 
												AddOnModule*		pAddOnMod,
												CParsersForModule&	aParsers
											)
{
	if (!pAddOnApp || !pAddOnMod)
		return FALSE;

	if (!pAddOnApp->m_XmlDescription.m_LocalizableInfo.GetOwnerModule().IsEmpty())
		return TRUE;

	// first localizable application config that is not mandatory
	CString sFileName = AfxGetPathFinder()->GetLocalizApplicationConfigFullName(pAddOnApp->m_strAddOnAppName, pAddOnMod->GetModuleName());
	if (!ExistFile(sFileName))
		return TRUE;

	CLocalizableApplicationConfigContent aLocAppContent (&pAddOnApp->m_XmlDescription.m_LocalizableInfo);
	aParsers.m_SaxReader.AttachContent(&aLocAppContent);

	return aParsers.m_SaxReader.ReadFile(sFileName);
}

//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::LoadModuleInfo (AddOnModule* pAddOnMod, CParsersForModule& aParsers)
{
	if (!pAddOnMod || pAddOnMod->GetModuleName().IsEmpty () )
		return FALSE;

	aParsers.m_pStatusBar->Show(cwsprintf(_TB("Loading Module {0-%s}..."), pAddOnMod->GetModuleName()));

	// module config must exist as it has been just checked
	CString sFileName = aParsers.m_pPathFinder->GetModuleConfigFullName(pAddOnMod->GetApplicationName(), pAddOnMod->GetModuleName());

	CModuleConfigContent aModContent (const_cast<CModuleConfigInfo*>(&pAddOnMod->m_XmlDescription.GetConfigInfo()));
	aParsers.m_SaxReader.AttachContent(&aModContent);
	if (aParsers.m_SaxReader.ReadFile(sFileName))
	{
		if (pAddOnMod->m_pApplication->IsACustomization())
			AfxGetCommonClientObjects()->AddInActivationInfo(pAddOnMod->GetApplicationName() + _T(".") + pAddOnMod->GetModuleName());
		return TRUE;
	}
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL CApplicationsLoader::LoadAddOnComponents (AddOnModule*	pAddOnMod, CParsersForModule& aParsers)
{
	aParsers.m_pStatusBar->Show(cwsprintf(_TB("Loading Addon-Definitions for the Module {0-%s}..."), pAddOnMod->GetModuleName()));

	CPathFinder::PosType aPosType = AfxGetPathFinder()->GetDefaultPosTypeFor(pAddOnMod->m_pApplication->IsACustomization());

	// client document objects
	CString sFileName = aParsers.m_pPathFinder->GetClientDocumentObjectsFullName (pAddOnMod->m_Namespace, aPosType, aPosType == CPathFinder::CUSTOM ? CPathFinder::EASYSTUDIO : CPathFinder::CURRENT);
	if (ExistFile(sFileName))
	{
		aParsers.m_ClientDocParser.SetCurrentModule(pAddOnMod->m_Namespace);

		aParsers.m_SaxReader.AttachContent(&aParsers.m_ClientDocParser);
		aParsers.m_SaxReader.ReadFile(sFileName);
	}


	return TRUE;
}

//-----------------------------------------------------------------------------
void CApplicationsLoader::InitializeTaskBuilderAddOn (CStatusBarMsg &statusBar)
{
	AddOnApplication* pTbAddOnApp = AfxGetBaseApp()->GetTaskBuilderAddOnApp();

	if (!pTbAddOnApp)
		return;

	XMLSettingsParser parser;
	for (int i=0; i <= pTbAddOnApp->m_pAddOnModules->GetUpperBound(); i++)
	{
		AddOnModule* pAddOnMod = pTbAddOnApp->m_pAddOnModules->GetAt(i);
		AfxGetTbCmdManager()->LoadNeededLibraries(pAddOnMod->m_Namespace);
		parser.LoadSettings (pAddOnMod->m_Namespace, AfxGetPathFinder(), _T(""), pAddOnMod->m_XmlDescription.GetOutDateObjectsInfo().GetSettings(), &statusBar);
		const_cast<COutDateObjectsDescription&>(pAddOnMod->m_XmlDescription.GetOutDateObjectsInfo()).ClearSettings();
	}

	// enable release-mode assertions
	DataObj* pDataObj = AfxGetSettingValue (snsTbGenlib, szEnvironment, szEnableAssertionsInRelease, DataBool(FALSE), szTbDefaultSettingFileName);
	AfxGetApplicationContext()->SetEnableAssertionsInRelease(pDataObj && pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)) ? *((DataBool*) pDataObj) : FALSE);

	// enable dumping of assertions even if no crashes occurred
	pDataObj = AfxGetSettingValue (snsTbGenlib, szEnvironment, szDumpAssertionsIfNoCrash, DataBool(FALSE), szTbDefaultSettingFileName);
	AfxGetApplicationContext()->SetDumpAssertionsIfNoCrash(pDataObj && pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)) ? *((DataBool*) pDataObj) : FALSE);


	//done here because needs setting
	AfxGetTbCmdManager()->InitOnDemandEnabled();
	
}