#pragma once

#include <TbGenlib\TbCommandInterface.h>

class CTBNamespaceArray;
class AddOnLibrary;
class AddOnModule;
class AddOnInterfaceObj;
class AddOnInterfaceArray;
class AddOnDll;
class CServerDocDescription;

// Class that manages AddOnLibraries structure and DLL loading 
//==============================================================================
class CLibrariesLoader : public CObject
{
	friend class CTbCommandManager;

private:
	DECLARE_LOCKABLE	(CMapStringToOb,		m_LoadedLibNamespaces);
	DECLARE_LOCKABLE	(CMapStringToOb,		m_LoadedDlls);
	volatile bool		m_bAllLoaded;
	long				m_nCounterDLLLoading;
	BOOL				m_bIsOnDemandEnabled;

public:
	CLibrariesLoader ();
	~CLibrariesLoader();

public:
	virtual LPCSTR  GetObjectName() const { return "CLibrariesLoader"; }

private:
	void InitOnDemandEnabled();
	void BeginDLLLoading	();
	void EndDLLLoading		();
	BOOL IsDLLLoading		();

	// method to optimize loading
	BOOL IsLibraryLoaded	(const CString& aLibName);
	BOOL IsNamespaceLoaded	(const CTBNamespace& aNamespace);

	// checks dll policies to define if it can be loaded
	BOOL CanBeLoaded	(const CTBNamespace& aLibNamespace, const CModuleConfigInfo& aModuleInfo, const LoadLibrariesMode aMode);

	// method that calculates which are the dlls to load
	void GetLibrariesToLoad				(const LoadLibrariesMode aMode, const CTBNamespace& aComponentNamespace,CTBNamespaceArray* pLibraries);
	void AddNewFieldsLibraries			(const LoadLibrariesMode aMode, const CTBNamespace& aTableNamespace,	CTBNamespaceArray* pLibraries);
	void AddClientDocsLibraries			(const LoadLibrariesMode aMode, const CTBNamespace& aDocumentNamespace,CTBNamespaceArray* pLibraries, AddOnModule* pAddOnMod);
	void AddLibrariesToLoad				(const LoadLibrariesMode aMode, const CTBNamespace& aLibraryNamespace,	CTBNamespaceArray* pLibraries, AddOnModule* pAddOnMod);

	// methods that manages available loading types and application requests
	BOOL LoadOnDemand					(const LoadLibrariesMode aMode, const CTBNamespace& aComponentNamespace, CTBNamespaceArray* pLibraries = NULL);
	BOOL LoadAllLibraries				(const LoadLibrariesMode aMode, CTBNamespaceArray* pLibraries = NULL);
	BOOL LoadNeededLibraries			(	
											const	CTBNamespace&		aComponentNamespace, 
													CTBNamespaceArray*	pLibraries = NULL,
													LoadLibrariesMode	aMode = LoadNeeded
										);
	BOOL RegisterAddOnTable (AddOnLibrary* pAddOnLib);
	// method for the library auto-registration: to invoke in dllinit
	BOOL AutoRegisterLibrary (const	CString& sLibNamespace, HINSTANCE hLib);

	// loading and registration operations of a single library
	BOOL				LoadLibrary				(const CTBNamespace& aLibNamespace, AddOnModule* pAddOnMod);
	BOOL				RegisterAddOnInterface	(AddOnLibrary* pAddOnLib, AddOnModule* pAddOnMod);
	AddOnInterfaceObj*	BindAddOnInterface		(AddOnLibrary* pAddOnLib, AddOnDll* pAddOnDll);


	// calculates the physical name of the dll
	CString	GetDllPhysicalName	(const CString& sLibName);
};
