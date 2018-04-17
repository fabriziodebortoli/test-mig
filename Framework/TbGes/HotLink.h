#pragma once

#include <TbGeneric\generalobjects.h>

#include <TbGenlib\hlinkobj.h>
#include <TbGenlib\oslinfo.h>
#include <TbGenlib\PARSOBJ.H>
#include <TbGenlib\DataObjDescriptionEx.h>
#include <TbGenlib\TbCommandInterface.h>

#include <TbNameSolver\TBNamespaces.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// usefull class
//
class SqlTable;
class SqlCachedTable;
class SqlRecord;
class SqlSession;
class DataObj;
class ITBRadar;
class CParsedCtrl;
class CAbstractFormDoc;
class CAbstractFormView;
class SymTable;
class SqlParamArray;
class FunctionDataInterface;
class SqlConnection;

#define	RADAR_MODE				0x0001
#define	CALL_LINK_MODE			0x0002
#define	RADAR_FROM_CTRL			0x0010
#define	CALL_LINK_FROM_CTRL		0x0020
#define	CALL_LINK_FROM_CTRL_WEB	0x0040

typedef  void (__stdcall *GOOD_REC_FUNC) ();

class SqlCatalogEntry;


#define ENABLE_DBT_HKL_JOIN()\
       if (!GetDocument()->IsInUnattendedMode())\
       {\
             m_pTable->GetRecord()->SetQualifier();\
             m_pTable->FromTable(m_pTable->GetRecord());\
       }

#define ADD_SINGLE_HKLTABLE(hklName, hklRuntime, recClass) if (!GetDocument()->IsInUnattendedMode()) {\
		HotKeyLink* pHKL = GetDocument()->GetHotLink<hklRuntime>(hklName);\
        recClass* pHKLRec = (recClass*)(pHKL->GetAttachedRecord());\
		pHKLRec->SetQualifier();\
		DataObjArray* pDataObjToSelect = new DataObjArray();\
		pDataObjToSelect->SetOwns(FALSE);\
		pHKL->GetFieldsForDBTJoinQuery(pDataObjToSelect);\
		for (int i = 0; i < pDataObjToSelect->GetSize(); i++)\
			m_pTable->Select(pHKLRec, pDataObjToSelect->GetAt(i));\
		delete pDataObjToSelect;

#define ADD_COMPARE_COL(a, b) \
		m_pTable->LeftOuterJoin(pHKLRec, a, b);

#define SELECT_HKL_FIELD(a) \
       m_pTable->Select( pHKLRec, a);      

#define ADD_FILTER_COL(param, col) \
       m_pTable->AddFilterColumn(pHKLRec, col);\
       m_pTable->AddParam(param, col);

#define SET_FILTER_COL(param, b) \
       m_pTable->SetParamValue(param, b);

#define END_ADD_SINGLE_HKLTABLE()\
       pHKLRec->DisableQualifier();}

#define DISABLE_DBT_HKL_JOIN()\
       if (!GetDocument()->IsInUnattendedMode()) m_pTable->GetRecord()->DisableQualifier();

/////////////////////////////////////////////////////////////////////////////
class TB_EXPORT HotKeyLink : public HotKeyLinkObj, public IOSLObjectManager
{
	DECLARE_DYNAMIC(HotKeyLink)

	friend class CAbstractFormDoc;
	friend class ComposedHotLink;
	friend class CJsonContext;
public:
	enum FindResult		{ NONE, FOUND, NOT_FOUND, NOT_VALID, EMPTY, PROTECTED };
	
protected:
	BOOL				m_bRecordAvailable;
	ITBRadar*			m_pRadarDoc;
	CAbstractFormDoc*	m_pCallLinkDoc;
	SqlRecord*			m_pRecord;
	SqlSession*			m_pSqlSession; //per lavorare in modalità disconnessa
	SqlTable*			m_pTable;
	SqlTable*			m_pSearchTable;
	CString				m_strError;
	FindResult 			m_PrevResult;
	BOOL				m_bDisableDoCallLink;	//per il browse
	CTBNamespace		m_AddOnFlyNamespace;
	CString				m_sAddOnFlyNamespace;
	CHotlinkDescription m_XmlDescription; 
	SymTable*			m_pSymTable;
	DataObjArray		m_arCustomizeParameters; //Per RadarWoorm da AskDialog
	BOOL	 			m_bForceQuery;
	BOOL	 			m_bSetParamValueByName;
	CString				m_strWarning;
	CString				m_sName;
	GOOD_REC_FUNC		m_pOnGoodRecord;
	ComposedHotLink*	m_pOwner = NULL;
public:
	CStringArray		m_arContextMenuSearches;

private:
	CString				m_strWebCallLinkRequestId;
	HWND				m_strWebCallLinkDocumentId;
	BOOL				m_bSkipEmptyDataObj;
	int					m_nCustomSearch;
	//TBROWSECURITY
	BOOL				m_bSkipRowSecurity;
	DataLng* 			m_pRSWorkerID;
	const SqlCatalogEntry* m_pCatalogEntry;
	int					m_nDbFieldRecIndex;
public:
	HotKeyLink(CRuntimeClass*, CString sAddOnFlyNamespace = _T(""), SqlConnection* pSqlConnection = NULL);
	HotKeyLink(const CString& sTableName, CString sAddOnFlyNamespace = _T(""),  SqlConnection* pSqlConnection = NULL);
	virtual ~HotKeyLink();

protected:
	//CREATO PER LA CLASSE DERIVATA SimulateHotKeyLink da non usare
	HotKeyLink();

public:
	// specific functions
	SqlRecord*			GetAttachedRecord	() const	{ return m_pRecord; }
	CTBNamespace&		GetNamespace		()			{ return GetInfoOSL()->m_Namespace; }
	void				SetNamespace		(const	CTBNamespace&);

	BOOL				IsDoCallLinkDisable	() const				{ return m_bDisableDoCallLink; }
	void				SetDoCallLinkDisable(const BOOL& bDisable)	{ m_bDisableDoCallLink = bDisable; }
	void				SetForceQuery		(BOOL bForce = TRUE )	{ m_bForceQuery = bForce;	}
	void				SetWebCallinkInfo	(CString sRequestID, HWND sDocumentId);

	const CString&		GetName				() const	{ return m_sName;	}
	void				SetName				(const CString& sName);
	const int			GetCustomSearch		() const	{ return m_nCustomSearch; }

public:
	BOOL				SetErrorString		(const CString& strError);
	CString				GetErrorString		(BOOL bClear);
	BOOL				SetWarningString	(const CString& strWarning);
	CString				GetWarningString	(BOOL bClear);
	
	virtual CString		GetDescriptionField () const;	//It returns description field NAME
	virtual DataObj*	GetKeyData			(SqlRecord* pRec, int idxDbField);

	void SetOnGoodRecordFunPtr (GOOD_REC_FUNC funPtr);
	GOOD_REC_FUNC GetOnGoodRecordFunPtr();

	void SetSkipEmptyDataObj(BOOL bValue = TRUE);
public:
	// public function called by radar	
	void			OnRadarDied				(ITBRadar*);
	virtual void	OnRadarRecordAvailable	();
	void			OnFormRecordAvailable	();
	void			OnFormDied				();
	void			GetJson					(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound);
	void			GetJsonPatch			(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound);
public:
	// non overloadabili dal programmatore
	virtual	void	AttachDocument	(CBaseDocument* pDocument);
	virtual void	Parameterize(HotLinkInfo* pInfo, int buttonId);
	virtual bool	FindNeeded(DataObj* pDataObj, SqlRecord* pMasterRec);
	virtual	BOOL	ExistData		(DataObj* pData);
	virtual	void	StopRunning		();

public:
	// non overloadabili dal programmatore

	// chiude (se e` aperta) la tabella collegata. (utile per fare query diverse)
	virtual void	CloseTable();

	BOOL			IsHotLinkFromControl ();
	
	// puo` essere usata dal programmatore per effettuare ricerche specifiche
	virtual FindResult	FindRecord		(DataObj*, BOOL bCallLink = FALSE, BOOL bFromControl = FALSE, BOOL bAllowRunningModeForInternalUse = FALSE);
	virtual BOOL		DoFindRecord	(DataObj* pKey) { return FindRecord(pKey) == FOUND; }

	// puo` essere usata dal programmatore per lanciare il radar esplicitamente
	virtual BOOL 		SearchOnLink	(DataObj* pData = NULL, SelectionType nQuerySelection = NO_SEL);

	// puo` essere usata dal programmatore per lanciare l'inserimento al volo esplicitamente
	void				CallLink		(DataObj* pData = NULL, BOOL bAskForCallLink = FALSE);
	
	void					ClearAddOnFlyNamespace		();
	void					SetAddOnFlyNamespace		(const CString& sNamespace);
	const CTBNamespace&		GetAddOnFly				()	{ return m_AddOnFlyNamespace; }

	virtual SqlParamArray*	GetQuery (SelectionType nQuerySelection, CString& sQuery, CString sFilter = _T(""));

	// ritorna i dati selezionati dalla Combo Query
	virtual int		SearchComboQueryData	(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
	virtual	int		DoSearchComboQueryData	(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);

	//virtual	BOOL	DispatchCustomize	(const DataObjArray& a);
	virtual void	PrepareCustomize		(DataObjArray& params);
	virtual void	OnPrepareAuxData();
	//TBRowSecurityLayer
	void SetProtectedState();
	BOOL IsProtected	();
	// allow the developer to disable query restrictions based on RowSecurityLayer
	void SetSkipRowSecurity(BOOL bSet) ;
	BOOL IsSkipRowSecurity	();
	//serve per effettuare il filtraggio dei dati in base ad uno specifico worker e non al worker collegato
	void		SetRowSecurityWorker(DataLng* pRSWorkerID);  // worker used from RowSecurity filter. By default is the logged worker.
	DataLng*	GetRowSecurityWorker();

protected:
	// DEVONO essere implementate dal programmatore
	virtual void		OnDefineQuery		(SelectionType nQuerySelection = DIRECT_ACCESS) = 0;
	virtual void		OnPrepareQuery		(DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS) = 0;
public:
	virtual	void		DoCallLink			(BOOL bAskForCallLink = FALSE);
	virtual void		DoDefineQuery		(SelectionType nQuerySelection = DIRECT_ACCESS);
	virtual void		DoPrepareQuery		(DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS);

protected:
	// overloadabili dal programmatore
	virtual	BOOL	 	SearchOnLinkUpper	()	{ return SearchOnLink (GetUpdatedDataObj(), UPPER_BUTTON); }
	virtual	BOOL 		SearchOnLinkLower	()	{ return SearchOnLink (GetUpdatedDataObj(), LOWER_BUTTON); }
	virtual void		OnRecordAvailable	()	{ /* do nothing */ }
	virtual void		OnCallLink			()	{ /* do nothing */ }
	virtual BOOL		IsValid				();
	virtual FindResult	OnRecordNotFound	();

	// Permette di intervennire sul DataObj associato al control un attimo prima
	// di fare la FindRecord, permettendo cosi' eventuali accessi indiretti o altre
	// possibilita di modifica del DataObj
	//
	virtual void		OnPreprocessDataObj (DataObj*, BOOL)	{ /* do nothing */  }

	virtual void		LoadSymbolTable			();
	virtual void		UpdateSymbolTable			(SqlRecord* pRec);

	virtual void		OnExtendContextMenu	(CMenu& menu);
	virtual void		DoContextMenuAction	(UINT nCode);

public:
	// costruiscono i valori per la ComboQuery
	virtual DataObj*	GetDbFieldValue	(SqlRecord* pRec) const;
	        int			GetDbFieldRecIndex (SqlRecord* pRec) const;
	virtual CString		FormatComboItem	(SqlRecord* pRec);
			CString		SlowFormatComboItem	(SqlRecord* pRec);	//per retrocompatibilità

	virtual CDocument*	BrowserLink			
			(
						DataObj*, 
						CDocument*		= NULL, 
				const	CRuntimeClass*	= NULL,
						BOOL			= TRUE
			);

	virtual void			InitNamespace		();
	virtual const CString	GetAddOnFlyNamespace()	{ return m_AddOnFlyNamespace.ToString(); }
	virtual DataObj*		GetDataObj	() const;

	virtual BOOL OnValidateRadarSelection (SqlRecord*);
	//impr 6459: hotlink e query di join con il DBT
	//fornisce l'elenco dei dataobj da inserire nella select della query di join del dbt che utilizza l'hotlink per estrapolare le informazioni di descrizione da mostrare nell'interfaccia
	//(questa query di join serve ad evitare le varie FindData nella OnPrepareAuxData)
	// di default se esiste restituisce il DataObj legato al campo Description 
	virtual void GetFieldsForDBTJoinQuery(DataObjArray*); 

private:
	virtual BOOL FindDataRecord	(DataObj* pData) { return FOUND == FindRecord(pData); }

private:
	DataObj*	GetUpdatedDataObj	();
	void		RecordAvailable		();
	void		ActivateWindow		(BOOL bPosted);
	
	// gestione del COMBO_ACCESS
	void		SplitFieldName		(const CString& sName, CString& sTblName, CString& sColName);
	BOOL		IsColumnToDisplay	(const CString& sWhenExpr);
	void		DefineXmlDescription(const CTBNamespace& aNamespace);

	void		SetErrorID			(CParsedCtrl::MessageID nErrID);

protected:
	virtual void		RemovePrimaryKeyLike(SqlTable* pTable, SqlRecord* pRec);
	virtual BOOL		RemovePrimaryKeyLike(CString& sFilter, CString sKeyField, CString sTable,SqlTable* pTable = NULL);

protected:
	void		CheckXmlDescription	();
	FindResult	OnFindRecord		(FindResult, DataObj*, BOOL bCallLink, BOOL bFromControl);
	void		EnableCtrl			(BOOL bEnable);
	void		Free				();

public:
	void		GetInfoOsl			();
	SymTable*	GetSymTable			() { return m_pSymTable; }

	CHotlinkDescription*	GetHotlinkDescription	() { return &m_XmlDescription; }
	DataObjArray*			GetCustomizeParameters	() { return &m_arCustomizeParameters; }

	virtual void SetParamValue (DataStr sName, DataObj* value);
	
	virtual void GetAuxInfoForHklBrowse(LPAUXINFO&)			{}
	virtual void SelectColumns(SqlTable* pTable, SelectionType nQuerySelection = NO_SEL);

	void SetParamValueByName(BOOL bSetParamValueByName) { m_bSetParamValueByName = bSetParamValueByName; } //It forces call Customize before OnPrepareQuery

	virtual DataObj* GetField (LPCTSTR sName) const ;

protected:
	virtual void OnInit (FunctionDataInterface*) {}

public:
	virtual SqlTable*	GetSqlTable		() { return m_pTable; }
	virtual void		OnAssignSelectedValue (DataObj* pCtrlData, DataObj* pHKLData);

	virtual CString		GetHKLDescription () const ;
	virtual DataObj*	GetDescriptionDataObj		();
	virtual void		OnAfterLoadRadarColumns(ITBRadar*) {}

	//Ottimizzazioni
protected:
	virtual BOOL		PrepareFormatComboItem (SqlRecord* pRec);



private:
	class ColumnAuxDescr: public CObject
	{
	public:
		Formatter*		m_pFormatter;
		int				m_nRecIndexDbField;

		ColumnAuxDescr() 
			: m_pFormatter(NULL), m_nRecIndexDbField(-1) {}
	};

	//per ottimizzazione FormatComboItem
	Array					m_arColumnAuxDescr;
	//Per ottimizzare UpdateSymbolTable: solo SymField referenziati
	CArray<SymField*, int>	m_arSymFieldsMappedToRecFieldIndex; 

public:
// Diagnostics
#ifdef _DEBUG
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG
};

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// è un hotlink simulato ovvero non accedo a nessuna tabella, permetto solo la gestione del datasource e attraverso il 
// metodo virtuale SearchComboQueryData ottengo la lista dei dati da visualizzare
class TB_EXPORT SimulatedHotKeyLink : public HotKeyLink
{
	DECLARE_DYNAMIC(SimulatedHotKeyLink)

public:
	SimulatedHotKeyLink();
	virtual ~SimulatedHotKeyLink() {};

public:
	virtual void	OnDefineQuery		(SelectionType nQuerySelection = DIRECT_ACCESS)				{}
	virtual void	OnPrepareQuery		(DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS)	{}
	virtual int		SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions) = 0;
	virtual	void	AttachDocument		(CBaseDocument* pDocument);

	virtual BOOL	IsHKLSimulated		() const { return TRUE; }

public:
// Diagnostics
#ifdef _DEBUG
	void Dump		(CDumpContext&)	const;
	void AssertValid()				const;
#endif // _DEBUG}
};

//////////////////////////////////////////////////////////////////////////////
//             					SimulateHKLUser
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SimHKLUser : public SimulatedHotKeyLink
{
DECLARE_DYNCREATE (SimHKLUser)

public:
	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
};

//////////////////////////////////////////////////////////////////////////////
//             					SimulateHKLRole
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SimHKLRole : public SimulatedHotKeyLink
{
DECLARE_DYNCREATE (SimHKLRole)

public:
	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
};

//////////////////////////////////////////////////////////////////////////////
//             					SimulateHKLRole
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SimHKLCurrentUserRoles : public SimulatedHotKeyLink
{
DECLARE_DYNCREATE (SimHKLCurrentUserRoles)

public:
	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
};


//////////////////////////////////////////////////////////////////////////////
//             					SimulateHKLCultureUI
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SimHKLCultureUI : public SimulatedHotKeyLink
{
DECLARE_DYNCREATE (SimHKLCultureUI)

public:
	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
};

//////////////////////////////////////////////////////////////////////////////
//             					SimulateHKLBarCodes
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SimHKLBarCode : public SimulatedHotKeyLink
{
	DECLARE_DYNCREATE (SimHKLBarCode)
public:
	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
};

//////////////////////////////////////////////////////////////////////////////
//             					HKLBehaviours
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT HKLBehaviours : public SimulatedHotKeyLink
{
	DECLARE_DYNCREATE (HKLBehaviours)

public:
	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
	virtual BOOL SearchComboKeyDescription(const DataObj* pKey, CString& sDescription);
};

//////////////////////////////////////////////////////////////////////////////
//             					HKLEntities
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT HKLEntities : public SimulatedHotKeyLink
{
	DECLARE_DYNCREATE (HKLEntities)

public:
	HKLEntities();
	~HKLEntities();

private:
	CTBNamespaceArray* m_pBehaviourArray;

public:
	const CTBNamespaceArray*	GetFilterByBehaviour() const { return m_pBehaviourArray; }
	void						AddFilterByBehaviour(const CString& aValue);

	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
	virtual BOOL SearchComboKeyDescription(const DataObj* pKey, CString& sDescription);

	void BindParam (DataObj* pObj, int n = -1);
	BOOL Customize (const DataObjArray& params);
};

//////////////////////////////////////////////////////////////////////////////
//             					HKLTables
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT HKLTables : public SimulatedHotKeyLink
{
	DECLARE_DYNCREATE (HKLTables)

public:
	HKLTables();

private:
	CString	m_sPrefixName;

public:
	void SetPrefixName(const CString& sPrefix);

	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);

	void BindParam (DataObj* pObj, int n = -1);
	BOOL Customize (const DataObjArray& params);
};

//////////////////////////////////////////////////////////////////////////////
//             					HKLTableColumns
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT HKLTableColumns : public SimulatedHotKeyLink
{
	DECLARE_DYNCREATE (HKLTableColumns)

public:
	HKLTableColumns();

private:
	CString	m_sTableName;

public:
	void SetTableName(const CString& sTable);
	BOOL SetTableNameFromDocNs(const CString& sDocNs);

	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);

	void BindParam (DataObj* pObj, int n = -1);
	BOOL Customize (const DataObjArray& params);
};

//////////////////////////////////////////////////////////////////////////////
//             					HKLApplications
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT HKLApplications : public SimulatedHotKeyLink
{
	DECLARE_DYNCREATE(HKLApplications)

public:
	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
	virtual BOOL SearchComboKeyDescription(const DataObj* pKey, CString& sDescription);
};

//////////////////////////////////////////////////////////////////////////////
//             					HKLModules
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT HKLModules : public SimulatedHotKeyLink
{
	DECLARE_DYNCREATE(HKLModules)
private:
	CString	m_sApplication;

public:
	void SetApplication(const CString& sApplication);

public:
	virtual int	SearchComboQueryData(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
	virtual BOOL SearchComboKeyDescription(const DataObj* pKey, CString& sDescription);

	void BindParam(DataObj* pObj, int n = -1);
	BOOL Customize(const DataObjArray& params);
};

//////////////////////////////////////////////////////////////////////////////
//             					XmlHotKeyLink
//////////////////////////////////////////////////////////////////////////////
class TB_EXPORT XmlHotKeyLink : public SimulatedHotKeyLink
{
	DECLARE_DYNCREATE(XmlHotKeyLink)
protected:
	BOOL			m_bInitialized;
	CTBNamespace	m_nsDatafile;
	CDataFileInfo*	m_pDfi;
	BOOL			m_bUseProductLanguage;
public:
	XmlHotKeyLink			();
	XmlHotKeyLink			(LPCTSTR ns);

	virtual ~XmlHotKeyLink	() {}

	void	SetDFNamespace			(LPCTSTR lpcszNamespace);

	BOOL	SetHotKeyLinkNamespace  (LPCTSTR sNs); //sostituisce registrazione nell'interface di library, effettua bind con ReferenceObject xml

	virtual	void	AttachDocument			(CBaseDocument* pDocument);
	virtual void	InitNamespace			();

	virtual int		SearchComboQueryData	(const int& /*ignored nMaxItems*/, DataObjArray& pKeyData, CStringArray& arDescriptions);
};

//////////////////////////////////////////////////////////////////////////////
//             					DynamicHotKeyLink
//////////////////////////////////////////////////////////////////////////////
class QueryObject;
class WoormTable;
class AskRuleData;
class InputMng;

class TB_EXPORT DynamicHotKeyLink : public HotKeyLink
{
	DECLARE_DYNCREATE(DynamicHotKeyLink)


protected:
	BOOL							m_bInitialized;
	DataStr							m_dsSelectionType;
	QueryObject*					m_pQuery;

protected:
	class CAsk
	{
	public:
		WoormTable*			m_pSymTable;
		AskRuleData*		m_pAskRule;

		CAsk		()	: m_pSymTable(NULL), m_pAskRule(NULL)	{}
		~CAsk		();
	};

	CAsk*	m_pAsk;

public:
	DynamicHotKeyLink();
	DynamicHotKeyLink(LPCTSTR ns);
	virtual ~DynamicHotKeyLink();

	BOOL SetHotKeyLinkNamespace(LPCTSTR ns); //sostituisce registrazione nell'interface di library, effettua bind con ReferenceObject xml

	void	LoadCustomSelectionTypes();

public:
	virtual void		SelectColumns		(SqlTable* pTable, SelectionType nQuerySelection = NO_SEL);
	virtual void		OnDefineQuery		(SelectionType nQuerySelection = DIRECT_ACCESS);
	virtual void		OnPrepareQuery		(DataObj*, SelectionType nQuerySelection = DIRECT_ACCESS);

	virtual	void		AttachDocument		(CBaseDocument* pDocument);
	virtual void		InitNamespace		();

	virtual void		LoadSymbolTable		();
	virtual void		UpdateSymbolTable	(SqlRecord* pRec);

	virtual	DataObj*	GetDataObj				(CString sDBField) const;
	virtual DataObj*	GetDataObj				() const;
	virtual DataObj*	GetDescriptionDataObj	() ;

	virtual SqlRecord*	GetRecord		() const;

	virtual	BOOL		Customize		(const DataObjArray&);	

	virtual BOOL		IsValid				();
	virtual BOOL		OnValidateRadarSelection (SqlRecord*);
	virtual void		OnRecordAvailable	();
	virtual void		OnCallLink			();

	virtual FindResult	FindRecord				(DataObj*, BOOL bCallLink = FALSE, BOOL bFromControl = FALSE, BOOL bAllowRunningModeForInternalUse = FALSE);
	virtual BOOL 		SearchOnLink			(DataObj* pData = NULL, SelectionType nQuerySelection = NO_SEL);
	virtual int			SearchComboQueryData	(const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
	virtual SqlParamArray* GetQuery				(SelectionType nQuerySelection, CString& sQuery, CString sFilter = _T(""));
	
	virtual void		CloseTable				();
	virtual void		OnRadarRecordAvailable	();
	virtual void		SetRecord				(const SqlRecord* pRec);

	virtual SqlTable*	GetSqlTable		();
	virtual	BOOL		ExistData		(DataObj* pData);
protected:
	void		InternalOnVoidEvent		(LPCTSTR pszEventName);
	void		ValorizeSelectedRecord	(SqlRecord*);
	BOOL		ExecuteEventIsValid		();

	virtual void OnAssignSelectedValue (DataObj* pCtrlData, DataObj* pHKLData);
};

//////////////////////////////////////////////////////////////////////////////

#define GET_RECORD(hkl, trec) ((trec*)(hkl->GetRecord()))
#define ATTACH_HOTLINK_DEFAULT(hotlink) Attach(hotlink); hotlink->SetName(CString(#hotlink));
#define ATTACH_HOTLINK(hotlink,name) Attach(hotlink); hotlink->SetName(CString(name));

//////////////////////////////////////////////////////////////////////////////
#include "endh.dex"
