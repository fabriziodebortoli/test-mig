#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\FunctionCall.h>

//includere alla fine degli include del .H
#include "beginh.dex"

TB_EXPORT extern const TCHAR szDefaultFamilyParents[];

// not lockable as intanced into document thread
//=============================================================================
class TB_EXPORT CDocUIObjectsCache : public CMapStringToOb
{
public:
	CDocUIObjectsCache ();

public:
	CObject*	GetCachedObject (const CTBNamespace& aNamespace);
	void		AddCachedObject (const CTBNamespace& aNamespace, CObject* pObject);
};

class CViewModeDescription;

//----------------------------------------------------------------
class TB_EXPORT CDocumentPartDescription : public CBaseDescription
{
	DECLARE_DYNAMIC(CDocumentPartDescription)
public:
	enum LoadingMode { Automatic, Manual };

private:
	CString			m_sClass;
	CBaseDescriptionArray	m_arViewModes;
	LoadingMode		m_LoadingMode;

public:
	CDocumentPartDescription();

		// metodi di lettura
	CString		GetClass		() const { return m_sClass; }
	LoadingMode	GetLoadingMode	() const { return m_LoadingMode; }
	
	// metodi di settaggio
	void	SetClass		(const CString& sClass);
	void	SetLoadingMode	(const LoadingMode aLoadingMode);
	
	// Gestione ViewModes
	void					AddViewMode			(CViewModeDescription* pDescri);
	CViewModeDescription*	GetViewMode			(const CTBNamespace& aNamespace)	const;
	CViewModeDescription*	GetViewMode			(const CString& sViewMode)			const;
	CBaseDescriptionArray&	GetViewModes		();
	CViewModeDescription*	GetFirstViewMode	()									const;
};

//==============================================================================
enum ViewModeType { VMT_BATCH, VMT_FINDER, VMT_DATAENTRY };
//==============================================================================

// la classe è destinata ad ospitare anche l'array dei templates
//----------------------------------------------------------------
class TB_EXPORT CViewModeDescription : public CBaseDescription
{
	DECLARE_DYNCREATE(CViewModeDescription)
	
	friend class CXMLDocumentObjectsParser;
private:
	ViewModeType	m_Type;
	BOOL			m_bSchedulable;
	BOOL			m_bNoWeb;
	CString			m_sFrameID;
public:
	CViewModeDescription ();

public:
	// metodi di lettura
	ViewModeType	GetType			() const { return m_Type; }
	BOOL			GetSchedulable	() const { return m_bSchedulable; }
	BOOL			GetNoWeb		() const { return m_bNoWeb; }
	CString			GetFrameID		() const { return m_sFrameID; }

	// metodi di settaggio
	void	SetType				(const ViewModeType aType);
	void	SetSchedulable		(const BOOL& bSchedulable);
	void	SetNoWeb			(const BOOL bNoWeb);
	void	SetFrameID			(CString sFrameID);

	virtual CViewModeDescription*	Clone	();
	void	Assign				(const CViewModeDescription& vd);
};

// Document
//----------------------------------------------------------------
class TB_EXPORT CDocumentDescription : public CBaseDescription
{
	DECLARE_DYNCREATE(CDocumentDescription)

	friend class CXMLDocumentDescriptionParser;
	friend class CXMLDocumentObjectsParser;

private:
	CString					m_sClassHierarchy;
	CString					m_sInterfaceClass;
	CBaseDescriptionArray	m_arViewModes;
	BOOL					m_bExcludeFromFamily;
	BOOL					m_bTransferDisabled;
	BOOL					m_bExcludeFromExtRef; //default == FALSE; TRUE  i.e. LotsBug Fix #17614) when the document is not able to export as External Reference
	BOOL					m_bDynamic;
	BOOL					m_bLiveInStandard;
	BOOL					m_bRunnableAlone;
	BOOL					m_bDesignable;
	CTBNamespace*			m_pTemplateNamespace;
	CString					m_sManagedType;
	CStringArray*			m_arDescriptionKeys;
	CBaseDescriptionArray	m_DocumentParts;
	CString					m_sNotLocalizedCaption;
	BOOL					m_bPublished = TRUE;
	BOOL					m_bActivated = TRUE;

public:
	CDocumentDescription ();
	CDocumentDescription (const CTBNamespace& aNamespace, const CString& sTitle);
	~CDocumentDescription ();
public:
	static CString	GetClassHierarchy	(const CString& sClassName);

	virtual const CString GetTitle	() const;
	virtual const CString GetCaption() const;

	CString			GetClassHierarchy	() const;
	CString			GetManagedType		() const { return m_sManagedType; }
	CStringArray*	GetDescriptionKeys	() const { return m_arDescriptionKeys; }  //sono gli elementi descrittivi trovati nella DocumentObjects che descrivono come deve essere formattata la caption
	BOOL			IsManaged			() const { return !m_sManagedType.IsEmpty(); }
	const CString&	GetInterfaceClass	() const { return m_sInterfaceClass; }
	BOOL			IsExcludedFromFamily() const { return m_bExcludeFromFamily; }
	BOOL			IsTransferDisabled	() const { return m_bTransferDisabled; } //to disable import/export operations
	BOOL			IsExcludeFromExtRef	() const { return m_bExcludeFromExtRef; }
	BOOL			IsDynamic			() const { return m_bDynamic; }
	BOOL			LiveInStandard		() const { return m_bLiveInStandard; }
	BOOL			IsRunnableAlone		() const { return m_bRunnableAlone; }
	BOOL			IsDesignable		() const { return m_bDesignable; }
	BOOL			IsMyClass			(const CString& sClassName, BOOL bExactMatch = FALSE) const;
	CTBNamespace*	GetTemplateNamespace() const { return m_pTemplateNamespace; }			
	const CBaseDescriptionArray&	GetDocumentParts	() const { return m_DocumentParts; }
	void			SetTemplateNamespace(CTBNamespace* pNs) { m_pTemplateNamespace = pNs;	}
	
	CViewModeDescription*	GetViewMode 		(const CString& sViewMode) const;
	CViewModeDescription*	GetViewMode 		(const CTBNamespace& aNamespace) const;
	CBaseDescriptionArray&	GetViewModes		();
	CViewModeDescription*	GetFirstViewMode	() const;

	// metodi di settaggio
	void SetClassHierarchy		(const CString&	sClass);
	void SetInterfaceClass		(const CString&	sClass);
	void SetExcludedFromFamily	(BOOL bValue)	{ m_bExcludeFromFamily = bValue; }
	void SetTransferDisabled	(BOOL bSet)		{ m_bTransferDisabled = bSet; }
	void SetExcludeFromExtRef	(BOOL bSet)		{ m_bExcludeFromExtRef = bSet; }
	void SetDynamic				(BOOL bSet)		{ m_bDynamic = bSet; }
	void SetLiveInStandard		(BOOL bSet)		{ m_bLiveInStandard = bSet; }
	void SetRunnableAlone		(BOOL bSet)		{ m_bRunnableAlone = bSet; }
	void AddViewMode			(CViewModeDescription*);

	void	Assign				(const CDocumentDescription& dd);
	
	virtual CDocumentDescription*	Clone	();

	const CString&		GetNotLocalizedCaption() const { return m_sNotLocalizedCaption; }
	void SetNotLocalizedCaption(const CString& sCaption) { m_sNotLocalizedCaption = sCaption; }

	void SetPublished	(const BOOL bValue) { m_bPublished = bValue; }
	BOOL IsPublished	() const			{ return m_bPublished; }
	void SetActivated(const BOOL bValue) { m_bActivated = bValue; }
	BOOL IsActivated() const { return m_bActivated; }
};

// ClientDocument
//----------------------------------------------------------------
class TB_EXPORT CClientDocDescription : public CBaseDescription
{
	DECLARE_DYNCREATE(CClientDocDescription)

private:
	CBaseDescriptionArray	m_ViewModeDescriptions;

public:
	enum ClientDocType { NORMAL, FAMILY, SCRIPT, FAMILYSCRIPT };
	enum MsgRoutingMode { CD_MSG_BEFORE, CD_MSG_AFTER, CD_MSG_BOTH };

	ClientDocType	m_Type;
	MsgRoutingMode	m_MsgRouting;
	CString m_sManagedDocType;

	CClientDocDescription ();
	CClientDocDescription (const CTBNamespace& aNamespace, const CString& sTitle);

	const CBaseDescriptionArray&	GetViewModeDescriptions	() const { return m_ViewModeDescriptions; }
	void AddViewModeDescription(CViewModeDescription* pViewModeDescription) { m_ViewModeDescriptions.Add(pViewModeDescription); }
	CViewModeDescription* GetViewModeDescription(CString sName);
	bool HasViewModes() { return m_ViewModeDescriptions.GetSize() > 0; }
};

// ServerDocument
//----------------------------------------------------------------
class TB_EXPORT CServerDocDescription : public CObject
{
	DECLARE_DYNCREATE(CServerDocDescription)

public:
	CTBNamespace			m_Namespace;
	BOOL					m_bIsFamily;
	CString					m_sClass;
	CBaseDescriptionArray	m_arClientDocs;
	CTBNamespaceArray		m_arLibraries;

public:
	CServerDocDescription () {}
	CServerDocDescription (const CString& sClass, const BOOL& bIsFamily, const CTBNamespace& aNamespace);

public:
	const CString&		GetClass		() const { return m_sClass; }
	const BOOL&			GetIsFamily		() const { return m_bIsFamily; }
	const CTBNamespace&	GetNamespace	() const { return m_Namespace; }
	CString				GetName			() const { return m_Namespace.GetObjectName(CTBNamespace::DOCUMENT); }
	BOOL				IsHierarchyOf	(const CString& sClass) const;
public:
	CClientDocDescription*	GetClientDocAt	(int nIdx)				  const { return (CClientDocDescription*) m_arClientDocs.GetAt(nIdx); }
	CClientDocDescription*	GetClientDoc	(const CTBNamespace& aNS) const { return (CClientDocDescription*) m_arClientDocs.GetInfo(aNS); }
	CBaseDescriptionArray&	GetClientDocs	();
	CTBNamespaceArray&		GetLibraries	();

	// metodi di settaggio
	void SetClass		(const CString& sClass);
	void SetIsFamily	(const BOOL& bValue);
	void SetNamespace	(const CTBNamespace& aNS);
	void AddClientDoc	(CClientDocDescription*);
};
//----------------------------------------------------------------
class TB_EXPORT CClientFormDescription : public CObject
{
	friend class CServerFormDescription;
	friend class CEasyStudioDesignerDialog;
	CServerFormDescription* m_pServer = NULL;
public:

	CString m_sName;
	CTBNamespace m_Module;
	bool m_bExclude = false;

private:
	BOOL PersistOnFileSystem();
	BOOL RemoveFromFileSystem();
};
//----------------------------------------------------------------
class TB_EXPORT CServerFormDescription : public CObject
{
public:
	CString m_sName;
	TArray<CClientFormDescription> m_arClientForms;

	CClientFormDescription* AddClient(const CString& sName, bool bExclude, const CTBNamespace& nsModule);
	BOOL RemoveClient(const CString& sName, BOOL bPersistOnFileSystem);
};
typedef CArray<CClientFormDescription*> ClientFormDescriptionArray;
//----------------------------------------------------------------
class TB_EXPORT CServerFormDescriArray : public CMap <CString, LPCTSTR, CServerFormDescription*, CServerFormDescription*>
{
	~CServerFormDescriArray();
public:
	CServerFormDescription* Get(const CString &sName, BOOL bCreate = FALSE);
};

// ServerDocuments
//----------------------------------------------------------------
class TB_EXPORT CServerDocDescriArray : public Array
{
public:
	int	Add	(CServerDocDescription* pEl) { return Array::Add (pEl); }
	
	CServerDocDescription* GetAt					(int nIdx) const { return (CServerDocDescription*) Array::GetAt (nIdx); }
	CClientDocDescription* GetClientDocInfo			(const CTBNamespace& aDocNS) const;
	void				   AddClientDocsOnServer	(CServerDocDescription* pServerInfo);
};

//=============================================================================        
const TB_EXPORT CServerDocDescriArray*	AFXAPI AfxGetClientDocsTable	();
	  TB_EXPORT void					AFXAPI AfxAddClientDocsOnServer	(CServerDocDescription*);


//=============================================================================        
//			DocumentObjectsTable needed structures
//=============================================================================        
class TB_EXPORT DocumentObjectsTable : public CObject, public CTBLockable
{
	friend class CXMLDocumentObjectsParser;
	friend TB_EXPORT void AFXAPI AfxAddDocumentDescription (const CDocumentDescription* pDescription);

	DECLARE_DYNAMIC(DocumentObjectsTable)

private:
	CMapStringToOb	m_Documents;

public:
	DocumentObjectsTable ();
	~DocumentObjectsTable();

public:
	CDocumentDescription*	GetDescription		(const CTBNamespace& nsDoc) const;
	void					GetDescriptionsOf	(const CTBNamespace& nsModule, CObArray& arDescri) const;
	void					GetDescriptions		(CObArray& arDescri) const;

public:
	virtual LPCSTR  GetObjectName() const { return "DocumentObjectsTable"; }

private:
	int		AddObject	(CDocumentDescription* pDescri);
	int		Merge		(	
							CDocumentDescription* pExistingDescri, 
							CDocumentDescription* pNewDescri
						);
};


DECLARE_SMART_LOCK_PTR(DocumentObjectsTable)
DECLARE_CONST_SMART_LOCK_PTR(DocumentObjectsTable)

// General Functions
//-----------------------------------------------------------------------------
TB_EXPORT DocumentObjectsTablePtr		AFXAPI AfxGetWritableDocumentObjectsTable	();
TB_EXPORT const CDocumentDescription*	AFXAPI AfxGetDocumentDescription			(const CTBNamespace& aNamespace);
TB_EXPORT void							AFXAPI AfxAddDocumentDescription			(const CDocumentDescription* pDescription);
TB_EXPORT CBaseDescriptionArray*		AFXAPI AfxGetDocumentDescriptionsOf			(const CTBNamespace& aNsModule);
TB_EXPORT CBaseDescriptionArray*		AFXAPI AfxGetDocumentsDescriptions			();

#include "endh.dex"
