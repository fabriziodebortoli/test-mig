#pragma once

class CStatusBarMsg;
class CParsersForModule;
class TBMetadataArray;
class TBFile;

//==============================================================================
class  CApplicationsLoader : public CObject
{   
private:
	CString	m_sLoadedModuleLibraries;
	CString	m_sLoadedModules;

public:
	// load applications
	BOOL LoadApplications				(CStatusBarMsg* pStatusBar);
	BOOL LoadApplicationsStep2			(CStatusBarMsg* pStatusBar);
	BOOL IntergrateLoginDefinitions		(CStatusBarMsg* pStatusBar);
	BOOL ReloadApplication				(const CString& sAppName);
	
private:
	// check operations
	BOOL ThereDuplicatesApplications	(CStringArray* pApps);
	BOOL IsDuplicateModule				(AddOnModule* pAddOnMod, AddOnModsArray* pModules);

	// partial actions
	BOOL			LoadSingleApplication		(const CString& strAppName,  CParsersForModule&);
	AddOnModule*	LoadSingleModule			(const CString& strAddOnApp, const	CString& strAddOnName, CParsersForModule&, AddOnModsArray*, AddOnApplication*);
	void			SetAddOnModules				();	
	void			DestroyModules				(AddOnModsArray* pAddOnMods);
	void			InitializeTaskBuilderAddOn	(CStatusBarMsg &statusBar);
	BOOL			IntegrateModuleDefinitions	(AddOnModule* pAddOnMod, CParsersForModule& aParsers);
	
	// file loading
	BOOL LoadLocalizableApplicationInfo	(AddOnApplication*, AddOnModule*,		CParsersForModule&);
	BOOL LoadApplicationInfo			(AddOnApplication*,	CParsersForModule&);
	BOOL LoadStandardComponents			(AddOnModule*,		CParsersForModule&);
	BOOL LoadModuleInfo					(AddOnModule*,	CParsersForModule&);
	BOOL LoadEnums						(AddOnModule*,		CParsersForModule&, CPathFinder::PosType aPos, EnumsTable* pTable);
	BOOL LoadAddOnComponents			(AddOnModule*,		CParsersForModule&);
};
