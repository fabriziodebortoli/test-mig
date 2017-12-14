#pragma once

#include <afxtempl.h>

#include <TBNameSolver\TBNamespaces.h>

#include <TbGenlib\XmlModuleObjectsInfo.h>
#include <TbGenlib\funproto.h>

#include "parsobj.h"
#include "oslinfo.h"

//includere alla fine degli include del .H
#include "beginh.dex"

// useful class
class CDiagnostic;
class CAbstractFormDoc;
class CClientDocArray;
class AddOnApplication;
class SqlConnection;
class CRTAddOnNewFieldsArray;
class AddOnDll;
class SqlCatalogEntry;

//=============================================================================
//							Interfaces Management
//=============================================================================
class TB_EXPORT AddOnInterfaceObj : public CObject
{     
	DECLARE_DYNAMIC(AddOnInterfaceObj);

public:
	enum RegisterState {Registered, Registering, NotRegistered};

	CObArray				m_arDocTemplates;
	Array					m_arHotLinks;
	Array					m_arFunctions;
	CMap<CString, LPCTSTR, CRuntimeClass*, CRuntimeClass*>	m_arItemSources;
	CMap<CString, LPCTSTR, CRuntimeClass*, CRuntimeClass*>	m_arValidators;
	CMap<CString, LPCTSTR, CRuntimeClass*, CRuntimeClass*>	m_arDataAdapters;
	CMap<CString, LPCTSTR, CRuntimeClass*, CRuntimeClass*>	m_arControlBehaviours;
	

private:
	CString					m_SourceFolder;
	RegisterState			m_State;

public:
	AddOnInterfaceObj ();
	~AddOnInterfaceObj();

public:
	virtual void	AOI_InitDLL					()							{}  //viene chiamato subito dopo il caricamento della DLL
	virtual	void	AOI_SetAddOnMod				()							{}  // viene chiamato dopo il caricamento di tutte le dll e di 
																							// tutti i componenti di tutti gli AddOnModule di tutte le AddOnApplication 
	virtual void	AOI_RegisterGlobalObjects	()	{}
	virtual void	AOI_RegisterTemplates		(UINT, CTBNamespace* = NULL)	{}
	virtual void	AOI_RegisterFunctions		(CTBNamespace* = NULL) {}
	virtual void	AOI_RegisterParsedControls	(const CTBNamespace& aLibNamespace) {}
	virtual BOOL	AOI_RegisterTables			(SqlConnection*, LPCTSTR, CTBNamespace* = NULL)	{ return TRUE; }

	virtual void	AOI_AttachClientDocs			(CAbstractFormDoc*, CClientDocArray*, const CTBNamespace&)	{}
	virtual void	AOI_AttachFamilyClientDocs		(CAbstractFormDoc*, CClientDocArray*, const CTBNamespace&) {}
	virtual void	AOI_RegisterHotLinks			(const CTBNamespace& aNamespace)	{}
	virtual void	AOI_RegisterItemSources			(const CTBNamespace& aNamespace)	{}
	virtual void	AOI_RegisterValidators			(const CTBNamespace& aNamespace)	{}
	virtual void	AOI_RegisterDataAdapters		(const CTBNamespace& aNamespace)	{}
	virtual void	AOI_RegisterContextMenus		(const CTBNamespace& aNamespace)	{}
	virtual void	AOI_RegisterFormatters			(const CTBNamespace& aNamespace)	{}
	virtual void	AOI_RegisterControlBehaviours	(const CTBNamespace& aNamespace)	{}
	

	virtual void	AOI_AddOnNewColumns			(const SqlCatalogEntry*, CRTAddOnNewFieldsArray*, const CString& sSignature, const CTBNamespace& aNamespace)	{}

	virtual BOOL	AOI_RegisterOSLObjects() 
									{
										return TRUE;
									}

	virtual BOOL	AOI_BeginRegistration	(const CTBNamespace& aNamespace) { return TRUE; }
	virtual BOOL	AOI_EndRegistration		(const CTBNamespace& aNamespace) { return TRUE; }
	
	// deve essere reimplementato da un'unica library del modulo
	// restituisce la release di database del modulo
	virtual int					AOI_DatabaseRelease() { return -1; }

	// metodo per identificare l'avvenuta registrazione
	RegisterState	GetRegisterState	();
	void			SetRegisterState	(RegisterState);
	CString			GetSourceFolder		() { return m_SourceFolder; }

	void	UnloadRegistrationInfo	();
	void	DeclareFunction			(FunctionDataInterface* pFn);
	void	SetSourceFolder			(char* cFolder);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " AddOnInterfaceObj\n"); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

// array of AddOnInterfaces
//=============================================================================
class TB_EXPORT AddOnInterfaceArray : public Array
{
public:
	AddOnInterfaceObj* 	GetAt		(int nIndex)const	{ return (AddOnInterfaceObj*) Array::GetAt(nIndex);	}
	AddOnInterfaceObj*&	ElementAt	(int nIndex)		{ return (AddOnInterfaceObj*&) Array::ElementAt(nIndex); }
	
	AddOnInterfaceObj* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	AddOnInterfaceObj*& 	operator[]	(int nIndex)	{ return ElementAt(nIndex);	}
};

// array of AddOnDlls
//=============================================================================
class TB_EXPORT AddOnDllArray : public Array
{
	friend class AddOnModule;
	friend class CApplicationsLoader;
	friend class CLibrariesLoader;
public:
	AddOnDll* 	GetAt		(int nIndex)const	{ return (AddOnDll*) Array::GetAt(nIndex);	}
	AddOnDll*&	ElementAt	(int nIndex)		{ return (AddOnDll*&) Array::ElementAt(nIndex); }
	
	AddOnDll* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	AddOnDll*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}
};

//=============================================================================
class TB_EXPORT AddOnLibrary : public CObject
{    
	DECLARE_DYNAMIC(AddOnLibrary)

public:
	AddOnDll*			m_pAddOnDll;
	AddOnInterfaceObj*	m_pAddOn;
	AddOnModule*		m_pAddOnModule;
	CTBNamespace		m_Namespace;

public:
	AddOnLibrary(AddOnInterfaceObj* pAddOn, AddOnDll* pAddOnDll, AddOnModule* pAddOnModule);
	virtual ~AddOnLibrary();

public:
	void	InitLibraryIdentity		(const CTBNamespace& aLibNamespace);	
	
public:
	const CString GetApplicationName() const { return m_Namespace.GetApplicationName();   } 
	const CString GetModuleName		() const { return m_Namespace.GetObjectName(CTBNamespace::MODULE); } 
	const CString GetLibraryName	() const { return m_Namespace.GetObjectName(); } 
	
	CBaseDescription*	GetRegisteredObjectInfo (const CTBNamespace& aNamespace);
	CBaseDescription*	GetRegisteredObjectInfo (CRuntimeClass* pObjectClass);
	CBaseDescription*	GetRegisteredObjectInfo (CTBNamespace::NSObjectType aType, const CString& sClassName);
	
	void				SetItemSource (const CTBNamespace& aNamespace, CRuntimeClass* pClass);
	CRuntimeClass*		GetItemSource (const CTBNamespace& aNamespace);
	void				GetItemSources(CTBNamespaceArray& ar);

	CRuntimeClass*		GetControlBehaviour(const CTBNamespace& aNamespace);
	void				SetControlBehaviour(const CTBNamespace& aNamespace, CRuntimeClass* pClass);
	void				GetControlBehaviours(CTBNamespaceArray& ar);

	void				SetValidator(const CTBNamespace& aNamespace, CRuntimeClass* pClass);
	CRuntimeClass*		GetValidator(const CTBNamespace& aNamespace);
	void				GetValidators(CTBNamespaceArray& ar);
	
	void				SetDataAdapter(const CTBNamespace& aNamespace, CRuntimeClass* pClass);
	CRuntimeClass*		GetDataAdapter(const CTBNamespace& aNamespace);

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP1(dc, " AddOnLibrary:%s \n", (LPCTSTR) m_Namespace.ToString()); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

//=============================================================================
class TB_EXPORT AddOnLibsArray : public Array
{
public:
	AddOnLibrary* 	GetAt		(int nIndex)const	{ return (AddOnLibrary*) Array::GetAt(nIndex);	}
	AddOnLibrary*&	ElementAt	(int nIndex)		{ return (AddOnLibrary*&) Array::ElementAt(nIndex); }
	
	AddOnLibrary* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	AddOnLibrary*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}
};

//=============================================================================
//							Dlls Management
//=============================================================================
class TB_EXPORT AddOnDll : public CObject
{    
	friend class CLibrariesLoader;

	DECLARE_DYNAMIC(AddOnDll)

private:
	HINSTANCE		m_hInstance;
	CTBNamespace	m_Namespace;
	bool			m_bIsRegistered;
	AddOnLibsArray	m_arAddOnLibs;

public:
	AddOnDll	(HINSTANCE	hInstance, const CTBNamespace& aDllNamespace);

public:
	const HINSTANCE		GetInstance			() const { return m_hInstance; } 
	const CString		GetApplicationName	() const { return m_Namespace.GetApplicationName();   } 
	const CString		GetModuleName		() const { return m_Namespace.GetObjectName(CTBNamespace::MODULE); } 
	const CString		GetDllName			() const { return m_Namespace.GetObjectName(); } 
	CTBNamespace*		GetNamespace		() { return &m_Namespace; } 
	
	void			AddAddOnLibrary		(AddOnLibrary* pLibrary) { m_arAddOnLibs.Add(pLibrary); }
	const AddOnLibsArray& GetAddOnLibraries() const { return m_arAddOnLibs; }

private:
	const bool&	IsRegistered	() const { return m_bIsRegistered; } 
	void		SetRegistered	()		 { m_bIsRegistered = true; } 
	
	// diagnostics

#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP1(dc, " AddOnDll:%s \n", (LPCTSTR) m_Namespace.ToString()); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};
//=============================================================================
//							Module Management
//=============================================================================
class TB_EXPORT AddOnModule : public CObject
{    
	friend class AddOnApplication;

	DECLARE_DYNAMIC(AddOnModule)

public:
	CTBNamespace			m_Namespace;		// informazioni di identità del modulo
	AddOnLibsArray*			m_pAddOnLibs;		// elenco di librerie che compongono il modulo
	int						m_nDatabaseRel;		// database release del modulo per compatibilitá
	CModuleDescription		m_XmlDescription;	// descrizione XML del modulo e dei suoi oggetti
	CString					m_sModulePath;		// il path di file system del modulo
	BOOL					m_bIsValid;			// indica che il modulo è valido e operativo
	BOOL					m_bIsCustom;		// indica che il modulo è una customizzazione
	AddOnApplication*		m_pApplication;
public:
	AddOnModule(AddOnApplication*);
	~AddOnModule();

public:
	void			SetValid			(const BOOL& bValue);
		  CString	GetApplicationName	()			{ return m_Namespace.GetApplicationName();   } 
	const CString	GetModuleName		() const	{ return m_Namespace.GetObjectName(CTBNamespace::MODULE);   } 
	const CString	GetModuleTitle		() const	{ return m_XmlDescription.GetConfigInfo().GetTitle();   } 
	const CString	GetAppModTitle		() const; //	{ return m_pApplication ? m_pApplication->GetTitle() + '.' + GetModuleTitle() : GetModuleTitle(); }

	const CTBNamespace& GetNamespace() const { return m_Namespace; }

		  int		GetDatabaseRelease	() const	{ return m_nDatabaseRel; }
	
	const CString	GetModuleSignature	() { return AfxGetDatabaseReleasesTable()->GetSignatureOf(m_Namespace.ToString());  } 
	const int		GetModuleDbRelease	() { return AfxGetDatabaseReleasesTable()->GetReleaseOf(m_Namespace.ToString());  } 
	
	void InitModuleIdentity		(const CString& strModFolder, const CString& strAddOnApp);	

	AddOnLibrary* GetLibraryFromHinstance (HINSTANCE hDllInstance);
	AddOnLibrary* GetLibraryFromName	  (const CString& sName);
	AddOnLibrary* GetOwnerLibrary		  (CRuntimeClass* pObjectClass);
	AddOnLibrary* GetOwnerLibrary		  (const CTBNamespace*);
	AddOnLibrary* GetOwnerLibrary		  (const CTBNamespace::NSObjectType aType, const CString& sClassName);
	const BOOL	  HasHotlinks			  ();
	const BOOL	  HasHotlinks			  (DataType);
	
	CBaseDescription*	GetRegisteredObjectInfo	(const CTBNamespace& aNamespace);
	const CString		ResolveLibrary (const CTBNamespace& aLibNamespace) const { return m_XmlDescription.GetConfigInfo().ResolveLibrary(aLibNamespace); }
	// funzioni Xtech
	const CStringArray*	GetAvailableDocEnvClasses	( const CTBNamespace& aDocNS) { return m_XmlDescription.GetAvailableDocEnvClasses(aDocNS); }

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP1(dc, " AddOnModule:%s \n", (LPCTSTR) GetModuleName()); }
	void AssertValid() const{ CObject::AssertValid(); }
#endif //_DEBUG
};

//=============================================================================
class TB_EXPORT AddOnModsArray : public Array
{
public:
	AddOnModule* 	GetAt		(int nIndex)const	{ return (AddOnModule*) Array::GetAt(nIndex);	}
	AddOnModule*&	ElementAt	(int nIndex)		{ return (AddOnModule*&) Array::ElementAt(nIndex); }
	
	AddOnModule* 	operator[]	(int nIndex)const	{ return GetAt(nIndex);	}
	AddOnModule*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}
};

//=============================================================================
//							Application Management
//=============================================================================
class TB_EXPORT AddOnApplication : public CObject
{ 

	DECLARE_DYNAMIC(AddOnApplication)

public:
	AddOnModsArray*			m_pAddOnModules;
	CString					m_strAddOnAppName;
	CApplicationDescription	m_XmlDescription;
	BOOL					m_bIsCustom;		// indica che il modulo è una customizzazione

private:	
	DECLARE_LOCKABLE		(AddOnDllArray,				m_AddOnDlls);
	DECLARE_LOCKABLE		(CMapStringToString,		m_DllAliases);//TODOPERASSO va locckata?
public:
	AddOnApplication	(AddOnModsArray* pAddOnMod = NULL);
	~AddOnApplication	();
	
public:
	void				AdAddonDll(AddOnDll* pDll);
	CStringArray*		GetAliasesOf(const CString& sDllName);
	void				AddAlias			(const CString& sDllName, const CString& sAlias);
	const CString&		GetSignature		()	const { return m_XmlDescription.m_Info.GetDbSignature(); }
	const CString&		GetAppVersion		()	const { return m_XmlDescription.m_Info.GetVersion(); }
	AddOnDll*GetAddOnDll (HINSTANCE hDllInstance);
	AddOnDll*GetAddOnDll (const CTBNamespace& aDllNamespace);
	const CString		GetTitle			()	const;

	void		SetAddOnModArray	(AddOnModsArray*);
	void		SetValid			(const BOOL& bValue);

	//per la gestione degli AddOn-Module
public:
	void	AOI_SetAddOnMod			();		// per eventuali inizializzazioni degli AddOnModule dopo la creazione dell'AddOnApplication
											// viene chiamato dopo il caricamento di tutte le dll 
											// e dei componenti di tutte le AddOnApplication
	void	AOI_AttachClientDocs		(CAbstractFormDoc*, CClientDocArray*);
	void	AOI_AttachFamilyClientDocs	(CAbstractFormDoc*, CClientDocArray*);

	BOOL	AOI_EndRegistration			();
	BOOL	AOI_DeclareAddOnApp			(const CString& sAppName);

public:
	// funzioni Xtech
	const CStringArray*	GetAvailableDocEnvClasses	(const CTBNamespace& aDocNS);
	void GetDllHandles(CArray<HINSTANCE>& ar);
// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext& dc) const {	ASSERT_VALID(this); AFX_DUMP0(dc, " AddOnApplication\n"); }
#endif //_DEBUG
};

//===========================================================================
class TB_EXPORT AddOnAppsArray : public Array
{
protected:
	CMapFunctionDescription	m_mapWebClass2Namespace;

public:
	CMapFunctionDescription&		GetMapWebClass()		{ return m_mapWebClass2Namespace; }
	const CMapFunctionDescription*	GetMapWebClass() const	{ return &m_mapWebClass2Namespace; }

	AddOnApplication* 	GetAt		(int nIndex) const 	{ return (AddOnApplication*) Array::GetAt(nIndex);	} 
	AddOnApplication*&	ElementAt	(int nIndex)		{ return (AddOnApplication*&) Array::ElementAt(nIndex); } 

	AddOnApplication* 	operator[]	(int nIndex) const 	{ return GetAt(nIndex);	}
	AddOnApplication*& 	operator[]	(int nIndex)		{ return ElementAt(nIndex);	}
	AddOnApplication*	GetMasterAddOnApp () const;
	void				AttachClientDocs		(CAbstractFormDoc* pServerDoc, CClientDocArray* pArray) const;
	void				AttachScriptClientDocs	(CAbstractFormDoc* pServerDoc) const;
	void				AttachFamilyClientDocs	(CAbstractFormDoc* pServerDoc, CClientDocArray* pArray) const;
};


#include "endh.dex"
