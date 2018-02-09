#pragma once

#include <TbGeneric\Critical.h>
#include <TbGenlib\baseapp.h>
#include <TbGenlib\TbTreeCtrl.h>
#include <TbGenlib\ToolBarButton.h>
#include "hotlink.h"
#include "TBEDataCoDec.h"

class IAbstractFormDocObject;
class CWizardFormView;
class CTabWizard;
class HotFilterObj;
class CNumbererBinder;

//-------------------------------------------------------------------------
//includere alla fine degli include del .H
#include "beginh.dex"
//==============================================================================

//	CCmdTarget
		class CClientDoc;

// CEventManager
		class CBusinessServiceProviderObj;

//	Array 
		class CClientDocArray;
		class CBSPArray;

//	CBaseDocument
		class CAbstractDoc;
			class CAbstractFormDoc;
				class CFinderDoc;
			class CRadarDoc;
			
//	CMDIChildWnd
		class CLocalizableFrame;
			class CAbstractFrame;
				class CAbstractFormFrame;
						class CMasterFrame;
							class CBatchFrame;
							class CFinderFrame;
						class CSlaveFrame;
							class CRowFormFrame;
				class CRadarFrame;
			
//	CFormView			
		class CAbstractFormView;
				class CMasterFormView;
				class CSlaveFormView;
				class CRowFormView;

//	CScrollView			
		class CAbstractView;
			class CRadarView;
			
			
//==============================================================================
// usefuls class declaration
//==============================================================================
class DataObj;
class Array;
class CParsedCtrl;
class CMessages;
class CFormManager;
class CReportManager;
class ITBRadar;
class CQueryManager;

class TabManagers;
class ControlLinks;
class CBodyEdit; class CBodyEditRowSelected; class ColumnInfo; class CBEButton;
class HotKeyLink;
class CWoormDoc;

class DBTObject;
class DBTMaster;
class DBTSlave;
class DBTSlaveBuffered;
class SqlRecord;
class SqlAddOnFieldsColumn;
class SqlTable;
class SqlTableInfo;
class RecordArray;
class SqlDatabase;
class SqlBrowser;
class CBaseTabManager;
class CTabManager;
class CTabDialog;
class CTileDialog;
class CTileGroup;
class CBodyEdit;
class CTBGridControl;
class CTBEDataCoDecRecordToValidate;
class CTBEDataCoDecPastedRecord;

class CXMLDocInfo;
class CXMLBaseAppCriteria;

class CEventManager;
class CXSLTFunction;

//class SqlConnection;
//class SqlSession;
class CWoormInfo;
class CObjectWrapperObj;

class CToolBarButtons;
class CDataSourceAlias;
class CFieldAlias;
class CGenericAlias;
class CTBProperty;
class CBETooltipProperties;
class CTBLinearGaugeCtrl;

//////////////////////////////////////////////////////////////////////////////

enum	SelStatus { SELECTED, NOT_SELECTED };
typedef	CArray<SelStatus, SelStatus>	SelArray;

//////////////////////////////////////////////////////////////////////////////

#define DECLARE_VAR(a, n)	DeclareVariable(a, n);
#define DECLARE_VAR_JSON(a)	DECLARE_VAR(_T(#a), &m_##a)
// pDoc must be: - m_pServerDocument for ClientDoc - m_pCallerDoc or CBusinessServiceProviderDoc for BSP 
#define DECLARE_VAR_JSON_WITH_PREFIX(prefix, nameVar, pDoc)		ASSERT(pDoc); pDoc->DeclareVariable(prefix#nameVar, &m_##nameVar);


class TB_EXPORT DeprecatedSymField : public SymField
{
	DECLARE_DYNAMIC(DeprecatedSymField)
public:
	DeprecatedSymField (const CString& strName, DataType dt = DataType::Null, WORD nId = SpecialReportField::NO_INTERNAL_ID, DataObj* pValue = NULL, BOOL bCloneValue = TRUE)
		: SymField(strName, dt, nId, pValue, bCloneValue)
	{
	}
};

///////////////////////////////////////////////////////////////////////////////
//	class CDMSAttachmentManagerObj:
//		classe base di interfaccia per permettere la gestione degli allegati da parte del documento
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CDMSAttachmentManagerObj : public CObject
{
	DECLARE_DYNAMIC(CDMSAttachmentManagerObj)

private:
	CXMLVariableArray* m_pBookmarkXMLVariables;

public:
	CAbstractFormDoc* m_pDocument;

public:
	CDMSAttachmentManagerObj() 
		: 
		m_pBookmarkXMLVariables(NULL),
		m_pDocument(NULL)
	{
		m_pBookmarkXMLVariables = new CXMLVariableArray();
	}

	~CDMSAttachmentManagerObj()
	{
		SAFE_DELETE(m_pBookmarkXMLVariables);
	}

public:
	void AttachDocument(CAbstractFormDoc* pDocument) { m_pDocument  = pDocument;}
	CXMLVariableArray* GetBookmarkXMLVariables() const { return m_pBookmarkXMLVariables; }

	virtual ::ArchiveResult		AttachReport			(const CString& strPdfFileName, const CString& strReportTitle, const CString& strBarcode, int& nAttachmentID, CString& strMessage) = 0;
	virtual ::ArchiveResult		AttachFile				(const CString& strFileName, const CString& strDescription, int& nAttachmentID, CString& strMessage) = 0;
	virtual ::ArchiveResult		AttachBinaryContent		(const DataBlob& binaryContent, const CString& strFileName, const CString& strDescription, int& nAttachmentID, CString& strMessage) = 0;
	virtual ::ArchiveResult		AttachArchivedDocument	(int nArchivedDocID, int& nAttachmentID, CString& strMessage) = 0;
	virtual	::ArchiveResult		AttachFromTable			(CString& strMessage) = 0;
	virtual ::ArchiveResult		AttachPapery			(const CString& strBarcode, const CString& strDescription, const CString& strReportName, CString& strMessage) = 0;

	virtual CAttachmentInfo*	GetAttachmentInfo		(int nAttachmentID) = 0;
	virtual CAttachmentsArray*  GetAttachments			(AttachmentFilterTypeEnum filterType) { return NULL; }
	virtual void				OpenAttachmentsListForm	(CUIntArray* pSelectedAttachmentID, bool onlyForMail) = 0;
	virtual bool				CreateNewSosDocument	(int nAttachmentID, CString& strMessage) = 0;
	virtual bool				IsDocumentInSOS			() = 0;

	//permette di utilizzare un datamember del documento come bookmark (vedi bookmark delle stampe fiscali in sostitutiva)
	virtual void				AddVariableForBookmark(const CString& strVariableName, DataObj* pDataObj) {	m_pBookmarkXMLVariables->Add(strVariableName, pDataObj); }
};

//=============================================================================
class TB_EXPORT CAbstractDoc : public CBaseDocument	//CRadarDoc derived from it too
{
protected: 
	DECLARE_DYNAMIC(CAbstractDoc)
	~CAbstractDoc();
protected:
	HACCEL		m_hDocAccel = NULL;	
	CAcceleratorDescription* m_pAccelDesc = NULL;
	CArray<UINT>m_arDocAccelIDRs;	
	CStringArray m_arDocAccelNames;	
	HINSTANCE	m_hResourceModule = NULL;

public :
	HINSTANCE				GetResourceModule();
	void					SetResourceModule(HINSTANCE hInstance) { m_hResourceModule = hInstance; }
	virtual void	SetPathName(LPCTSTR lpszPathName, BOOL = TRUE);
	virtual BOOL	IsBadCmdMsg		(UINT nID);
	virtual ITBRadar* GetRadarInterface () { return NULL; }

public :
	void 	SetIconID 		(UINT nIcon);
	void 	SetDocAccel		(UINT nDocAccelIDR) { m_arDocAccelIDRs.Add(nDocAccelIDR); }
	void 	SetDocAccel		(LPCTSTR sDocAccelName) { m_arDocAccelNames.Add(sDocAccelName); }
	HACCEL	GetDocAccel		() 					{ return m_hDocAccel; }
	CString GetDocAccelText (WORD id);
protected:
	void	LoadDocAccel ();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};

class CReportMenuNode;
class HotFilterManager;
class CJsonDialog;

//=============================================================================
class TB_EXPORT CAbstractFormDoc : public CAbstractDoc, public IBehaviourContext, public IJsonModelProvider
{
	friend class ADMObj;
	friend class ADMGateObj;
	friend class HotKeyLink;
	friend class CAbstractView;
	friend class CAbstractFormView;
	friend class TableUpdater;
	friend class CounterManagerObj;
	friend class CEventManager;
	friend class CControlBehaviour;

	friend class CRadarDoc;
	friend class CWrmRadarDoc;
	friend class CWrmMaker;
	friend class CXMLExportCriteria;
	friend class AbstractDocSnapshot;
	friend class CUpdateDataViewLevel;
	template <class T> friend class TBJsonBodyEditWrapper;
	friend class CJsonContext;
	friend class CClientDoc;
	friend class CBusinessServiceProviderObj;
	friend class CJsonModelGenerator;
	friend class CMasterFrame;
	friend class DBTSlaveBuffered;

protected:
	DECLARE_DYNAMIC(CAbstractFormDoc)

protected:	// Data Member
	BOOL	m_bDisableLoadXMLDocInfo;
	BOOL	m_bFavoritesState;

public:
	CString			m_strFormName;		// Nome del file contenente la definizione della form generale di documento

	UINT			m_nMessagesID;

	SqlRecord*		m_pCurrentRec;		// contiene l'ultimo record correttamente caricato
	CFormManager*	m_pFormManager;
	CQueryManager*	m_pQueryManager;	// gestisce le query del documento
	CEventManager*	m_pEventManager;	// gestisce gli eventi relativi a importazioni da file XML
	CXSLTFunction*	m_pXSLTFunction;	// fornisce le funzioni da utilizzare in un foglio di trasformazione XSLT
	
	// dati necessari per gestire correttamente la CallLink
	HotKeyLink*	m_pHotLink;
	SqlRecord*	m_pHLRecord;

	// Dati per gestire i Templates Batch che usano Frame e View particolari
	Scheduler		m_BatchScheduler;
	CriticalArea	m_BatchRunningArea;

	//aggiunto per il documentale
	BOOL			m_bEnableDocumentale;
	//aggiunto per i report relativi al barcode WMS
	BOOL            m_bEnableReportBarcodeWMS;

	//TODO da togliere
	int		m_nLocalQueries;
	int		m_nAuxQueries;	
	int		m_nAdmQueries;	
	int		m_nUpdateDataViewLevel;
	bool	m_bNeedsUpdateDataView;
	int		m_nDisabled;
	bool	m_bWarnWhenEmptyQuery;
	Array	m_arDataSourceAliases;
	Array	m_arFieldAliases;
	Array	m_arGenericAliases;
	CArray<CJsonDialog*> m_JsonDialogs;
protected:	
	CWoormInfo*	m_pWoormInfo;
	//DMS 
	CDMSAttachmentManagerObj*			m_pDMSAttachmentManager;

public:
	//CArray<CWoormInfo*>		m_arWoormInfo;
	//void		SetWoormInfo	(CWoormInfo* pWoormInfo)	{ m_arWoormInfo.Add(pWoormInfo); }
	//CWoormInfo* GetWoormInfo	()							{ return m_arWoormInfo.GetSize() ? m_arWoormInfo.GetAt(m_arWoormInfo.GetUpperBound()) : NULL; }

	virtual void	SetWoormInfo(CWoormInfo* pWoormInfo)	{ m_pWoormInfo = pWoormInfo; }
	CWoormInfo*	GetWoormInfo() { return m_pWoormInfo; }

	Array		m_arReportDoc;		// corrente istanza del report lanciato

	BOOL		CloseReportReady();
	BOOL		CloseWoormReport();
	WoormDocPtr	RunWoormReport	(CWoormInfo* pWoormInfo, BOOL bWaitEnd = FALSE);
	BOOL		IsReportAlive   (BOOL bActivateLast);
	void		WaitReportEnd	();

public:
	enum LockStatus { ALL_LOCKED, NO_AUX_DATA, LOCK_FAILED };
	
protected:
	// modificabili dal programmatore
	BOOL m_bRepeatableNew;	// premette di rimanere in new dopo una new
	BOOL m_bOnlyOneRecord;	// disabilita new su piu' di un record
	BOOL m_bBrowseOnCreate;	// Abilita la chiamata al browser
	BOOL m_bBrowseOnFirstRecord; //Si richiede di posizionarsi sul primo record
	BOOL m_bNoFamilyClientDoc;
	BOOL m_bOnAttachClient;
	BOOL m_bUseEasyBrowsing;	// on document selection action (from radar or browser) stay in browse mode instead of editing
	BOOL m_bSingleUserMode;
	BOOL m_bLockedDocument;
	BOOL m_bUpdateDefaultReport; //save or no default report choosing report in dropdown of 'Print' toolbar button

public:	//	Data Member	
	DBTMaster*				m_pDBTMaster;
	Array*					m_pHotKeyLinks;
	Array					m_ItemSources;
	Array					m_Validators;
	Array					m_pDataAdapters;
	TArray<CControlBehaviour>m_ControlBehaviours;
	ITBRadar*				m_pRadarDoc;
	SqlBrowser*				m_pBrowser;
	CClientDocArray*		m_pClientDocs;
	CBSPArray*				m_pBSPs;
	HotFilterManager*		m_pHotFilterManager = NULL;

	DataStr*				m_pHeaderTitle;
	CXMLBaseAppCriteria*	m_pBaseExportCriteria;
	CXMLBaseAppCriteria*	m_pBaseImportCriteria;
	CXMLVariableArray*		m_pVariablesArray;

	// gestione delle sessioni di lavoro
	virtual SqlSession*		GetReadOnlySqlSession	() ;
	/*TBWebMethod*/virtual SqlSession*		GetUpdatableSqlSession  () ;
	virtual SqlConnection*	GetSqlConnection		() ;


	//per compatibilità con la versione WEB
	virtual void ConnectToDatabase(LPCTSTR = NULL) {}
	virtual void DisconnectFromDatabase() {}


	BEGIN_TB_STRING_MAP(QueryNames)
		TB_LOCALIZED(SEARCH_QUERYNAME,	"Search")
	END_TB_STRING_MAP()

private:
	CObArray			m_arRegisteredDBTs;
	BOOL				m_bCheckExistXMLDocInfo;
	BOOL				m_bExistXMLDocInfo;
	CJsonWrapper		m_JsonData;//per i delta dei dati json
	CNumbererBinder*	m_pNumbererBinder;
	CMap<CString, LPCTSTR, CString, LPCTSTR> m_ForwardHotlinks;//elenco degli hotlink dichiarati nel json ma non necessariamente ancora istanziati 
protected:
	CXMLDocInfo*		m_pXMLDocInfo;

	CToolBarButtons	m_ToolBarButtons; // contiene le info inerenti i bottoni aggiunti dal programmatore e dai client doc

public:		// constructors
	CAbstractFormDoc();
	virtual ~CAbstractFormDoc();

	void SetAbstractFormDocObjectProxy(IAbstractFormDocObject* pObject);
	
private:	
	CString GetDocumentObjectCaption();
	// Data Caching Management
	void SetDataCacheStatus		();
	BOOL TranslateDataSourceAlias(CString& sDataSource);
	BOOL TranslateFieldAlias(const CString& sActualDataSource, CString& sField);
	BOOL TranslateAlias(CString& sAlias);
protected:
	// gestione del record corrente necessario per abilitare delete/update
	// e per ripristinare i dati da eventuali escape
	void SaveCurrentRecord();
	void ClearCurrentRecord();
	void RefreshCurrentRecord();

public:	
	virtual void GetDataSource(CString sDataManager, CString sField, DBTObject*& pDbt, SqlRecord*& pRecord, DataObj*& pField, bool& isVirtual);
	BOOL ValidCurrentRecord		();
	void SetWarnWhenEmptyQuery(bool bSet){ m_bWarnWhenEmptyQuery = bSet; }

	virtual void  GetDocumentObjectCaptionDataObjArray(DataObjArray& ar);
	
	CAbstractFormDoc*	GetMyAncestor() const
	{
		return m_pDocInvocationInfo ? (CAbstractFormDoc*)m_pDocInvocationInfo->m_pAncestorDoc : NULL;
	}

	CNumbererBinder*	GetNumbererBinder();
private:
	// torna l'ancestor passato durante la OnOpenDocument se esiste
	

	void AdjustDoubleInWhereClause	(
										const CString& strParam, 
										const DataObj& aRadarDataObj, 
										SqlTable* pTable, 
										DataDbl* pDataObj,
										SqlRecord* pRec = NULL
									);
	HotKeyLink* GetHotLink(const CString& sName, CRuntimeClass* pClass);

//for DMS
public:
	//void GetDescriptionFieldsForEA(CStringArray* pDescriFields);
	//void GetFieldsForEA(CStringArray* pFieldsForEA);
	virtual CString GetKeyInXMLFormat();
	void DispatchDMSEvent(DMSEventTypeEnum eventType, int eventKey); //effettua il dispatch degli eventi del DMS	
	virtual void OnDMSEvent(DMSEventTypeEnum eventType, int eventKey) {} //permette di intervenire sugli eventi del DMS
	void DispatchCommandToDocument(UINT nID);

	CDMSAttachmentManagerObj*	GetDMSAttachmentManager()								const { return m_pDMSAttachmentManager;			}
	void						SetDMSAttachmentManager(CDMSAttachmentManagerObj *pAttachMng) { m_pDMSAttachmentManager = pAttachMng;	}
	

public:
	void ResetJsonData(CJsonParser& json);
	void Disable(bool bDisable = true);
	bool IsDisabled() { return m_nDisabled > 0; }
	template<class T> T* GetHotLink(const CString& sName) { return (T*)GetHotLink(sName, RUNTIME_CLASS(T)); }
	virtual HotKeyLink*	GetHotLink(const CString& sName, const CTBNamespace& aNameSpace = CTBNamespace(_T("")));
	virtual CItemSource* GetItemSource(const CString& sName, const CTBNamespace& aNameSpace, const CString& sParameter = NULL, bool  bUseProductLanguage = FALSE, bool  bAllowChanges = FALSE);
	virtual CValidator* GetValidator(const CString& sName, const CTBNamespace& aNameSpace);
	virtual CDataAdapter* GetDataAdapter(const CString& sName, const CTBNamespace& aNameSpace);
	virtual CControlBehaviour* GetControlBehaviour(const CString& sName, const CTBNamespace& aNameSpace);
	virtual void OnControlBehaviourCreated(CControlBehaviour* pBehaviour) {}

	void		RegisterForwardHotLink(const CString& sName, const CString& aNameSpace);
	void		RemoveHotLink(HotKeyLink* pHotKeyLink);

	/*TBWebMethod*/DataObj*		GetFieldData	(DataStr aTableName, DataStr aFieldName, DataInt nRow);
	/*TBWebMethod*/DataStr		GetHotLinkValue	(DataStr aCtrlNamespace, DataStr aHotLinkFieldName);
	/*TBWebMethod*/DataStr		GetFieldValue	(DataStr aTableName, DataStr aFieldName, DataInt nRow);
	/*TBWebMethod*/void			SetFieldValue	(DataStr aTableName, DataStr aFieldName, DataInt nRow, DataStr aValue);
	/*TBWebMethod*/DBTMaster*	GetMaster();
	
	void GetJsonRadarInfos(CJsonSerializer& resp);

	// DOC_DIAGNOSTIC
	CMessages*			GetMessages		() const { return m_pMessages; }

	CAbstractFormDoc*	GetAncestor		() const { return GetMyAncestor() ? GetMyAncestor() : (CAbstractFormDoc*)this; }
	CXMLDocInfo*		GetXMLDocInfo	() const { return m_pXMLDocInfo; }
	BOOL				SetXMLDocInfo	(CXMLDocInfo*);
	BOOL				SetXMLDocInfo	(const CXMLDocInfo&);

	Scheduler&	GetBatchScheduler ()	{ return m_BatchScheduler; }

	void	SetDefaultFocus ();
	void	SetViewFocus	(CView* pView, BOOL bActivate = TRUE);
	BOOL	SetCtrlFocus	(UINT nIDC);

	//@@TODO da togliere
	void 	SetLocalQueries	(int)	{}
	void 	SetAuxQueries	(int)	{}
	void 	SetAdmQueries	(int)	{}

	const	CString GetCurrentQueryName	();
	void 	SetCurrentQueryName	(const CString& strQueryName);

	const	CString& GetFormName()  { return m_strFormName; }
	void 	SetFormName			(const CString& strFormName);
	
	void 	SetFormTitle		(UINT nTitleID);
	void 	SetFormTitle		(const CString& strTitle);
	void	SetUseEasyBrowsing	(BOOL bUseEasyBrowsing) { m_bUseEasyBrowsing = bUseEasyBrowsing;}

	void 	SetMessagesID		(UINT nMessagesID)	{ m_nMessagesID		= nMessagesID;	}
	void 	SetOnlyOneRecord	(BOOL bSet = FALSE)	{ m_bOnlyOneRecord	= bSet;			}
	BOOL	IsOnlyOneRecord		()					{ return m_bOnlyOneRecord;			}
	
	void 	SetRepeatableNew	(BOOL bSet = FALSE)	{ m_bRepeatableNew	= bSet;			}
	void 	SetUpdateDefaultReport(BOOL bSet = TRUE) { m_bUpdateDefaultReport= bSet; }
	void 	SetBrowseOnCreate	(BOOL bSet = FALSE)	{ m_bBrowseOnCreate	= bSet;			}
	void	SetBrowseOnFirstRecord(BOOL bSet = FALSE) { m_bBrowseOnFirstRecord = bSet; }
	void	DisableFamilyClientDoc	(BOOL bSet = FALSE)	{ m_bNoFamilyClientDoc = bSet;  }

	virtual CString GetDefaultReport			();
	virtual CString GetDefaultForm				();
	virtual CString GetDefaultTitleForm			();
	virtual CString GetDefaultMenuDescription	();
	virtual BOOL	ReportIsEnabled(CString sReportName) { return TRUE; }

	CBaseTileDialog*	GetTileDialog(UINT nIDD);

	CString GetDefaultTitleReport	();
	CReportManager& GetReportManager();
	void	ChangeReports			(const CString& strDocumentNamspace);
	void	SetSpecificReports		(const CString& sForPrint, const CString& sForEmail);
	
	// da usarsi nella intercettazione del messaggio EN_VALUE_CHANGED
	void	SetBadData		(DataObj& aData, const CString& strMess)		{ aData.SetValid(FALSE); IsInUnattendedMode() ? m_pMessages->Add(strMess)						: AfxGetBaseApp()->SetError(strMess); }	
	void	SetError		(const CString& strMess)						{						 IsInUnattendedMode() ? m_pMessages->Add(strMess)						: AfxGetBaseApp()->SetError(strMess); }	
	void	SetWarning		(const CString& strMess, BOOL bNoCanc = FALSE)	{						 IsInUnattendedMode() ? m_pMessages->Add(strMess, CMessages::MSG_HINT)	: AfxGetBaseApp()->SetWarning(strMess, bNoCanc); }	
	//	gestione del tooltip
	BOOL	GetToolTipText	(UINT nId, CString& strMessage);

	//per il documentale
	void	EnableDocumentale	(BOOL bEnable = TRUE)		{ m_bEnableDocumentale = bEnable; }

	// gestione monoutenza:un documento di tipo batch può essere aperto in monoutenza, ovvero può essere eseguito solo
	// da un utente alla volta
	void	SetSingleUserMode(BOOL bSet = TRUE) { m_bSingleUserMode = bSet; }

	void	SetBatchButtonState	();

	virtual BOOL	HasClientDocToAttach	(const CString& sWhenServerName, const BOOL& bIsFamily);
	virtual BOOL	HasClientDocToAttach	(const CRuntimeClass* pServerRuntimeClass);

	void		RegisterDBT			(DBTObject* pDBT); 
	DBTObject*	GetRegisteredDBT	(const CTBNamespace& aNs) const;
	DBTObject*	GetRegisteredDBT	(const CString& aTableName) const;
	DBTObject*	GetRegisteredDBT	(const CRuntimeClass* pDBTClass) const;
	DBTObject*	GetRegisteredDBTByName(const CString& aDbtName) const;
	void		DeregisterDBT		(DBTObject* pDBT); 

	BOOL		CanLoadXMLDescription();
	void		LoadXMLDescription(); //load XML files description in InitDocument only if is a dynamic document; otherwise is on demand

	//richiama una funzione dell'event manager (lavora anche sugli eventuali event manager dei ClientDoc)
	virtual int		FireAction			(const CString& strActionName,  CString* pstrInputOutput);
	virtual int		FireAction			(const CString& strActionName,  void* pVoidInputOutput);
	virtual int		FireAction			(const CString& strActionName);
	virtual int		FireAction			(const CString& strActionName, CFunctionDescription*);
	
	virtual BOOL	ExistAction				(const CString& strActionName);

	// ottengo la lista di universal key gestite associate al documento
	// restituisco TRUE se devo gestire la lista di UK restituite nell'array
	virtual void 	GetUniversalKeyList				(CStringArray& UKList);

	// per il menu popup del control passato come parametro
	//@@BAUZI mettere poi la funzione nel cpp
	virtual BOOL	ShowingPopupMenu	(UINT nID, CMenu* pMenu) { return OnShowingPopupMenu(nID, pMenu) && DispatchOnShowingPopupMenu(nID, pMenu); }

	void	GoInBrowseMode		();

	CManagedDocComponentObj*	GetComponent	(CString& sParentNamespace, CString& sName);
	void						GetComponents	(CManagedDocComponentObj* pRequest, Array& returnedComponens);

	//per Weblook
	void  GetIDsUsedInMessageMap(CArray<int>& arIDs);
	void  PopulateIDsArrayFromMessageMap(const AFX_MSGMAP* pMsgMap, CArray<int>& arIDs);
protected:
	BOOL	ReturnDataToCallLink	(BOOL bClose = TRUE, BOOL bCopyRecord = TRUE);
	void	SetFocusOnField(DataObj* pField);
	
	void	InitData			();
public:
	HotFilterManager*	GetHotFilterManager();
	virtual void		CompleteQuery(const CString strName, SqlTable* pTable, SqlRecord* pRec, const DataObj& aColumn);
	virtual void		CompleteQuery(const CString strName, SqlTable* pTable, const DataObj& aColumn);
	virtual void		OnAfterHotLinkCreated(HotFilterObj* pHF) {}


	CGenericAlias*		RegisterGenericAlias(CString sAlias, const CString& sActual);
	CDataSourceAlias*	RegisterDataSourceAlias(CString sAlias, const CString& sActual);
	CFieldAlias*		RegisterFieldAlias(CString sAlias, const CString& sActual);

	void	EnableControls		();
protected:
	virtual BOOL	BatchEnableControls	();
			BOOL	DispatchPrepareAuxData (BOOL bOnlyDocument = FALSE);
	
			void 	PrepareFindQuery	(SqlTable* pTable, BOOL bFromXTech = FALSE);
			BOOL	PrepareFindQuery	(int& nParamIdx, SqlTable* pTable, SqlRecord* pRec, BOOL bFromXTech = FALSE);
	virtual void	OnAfterSetFormMode	(FormMode oldFormMode);
	virtual void	OnAfterCustomizeClientDocs() {}

public:
	virtual void 	PrepareRadarQuery	(SqlTable* pTable);
	virtual void 	SelectRadarColumns	(SqlTable* pTable);

	BOOL	CanFindOnSlave () const;
	virtual BOOL CanShowInOpenDocuments();

public:
	void	BatchStart				 ();
	void	UnlockAll				 ();
	
	virtual BOOL OnOpenDocument	(LPCTSTR);
protected:
	void	BatchStop				 ();
	void	SwitchBatchRunButtonState();

	// serve per impostare le personalizzazioni dei clientdoc sul documento
	void	CustomizeClientDoc	();
	
	// disabilitio il caricamento delle info XML
	void	DisableLoadXMLDocInfo()	{ m_bDisableLoadXMLDocInfo = TRUE; }

protected:
	// supporto al LOCK	
	BOOL LockDocument	(BOOL bDelete = FALSE);
	BOOL LockMaster		(BOOL bUseMessageBox = TRUE);
	
protected:	
	virtual BOOL OnNewDocument	();
	virtual BOOL OnSaveDocument	(LPCTSTR);
	virtual BOOL OnCmdMsg		(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);

	// NON REIMPLEMENTABILE dal programmatore
	virtual	FormMode	SetFormMode(FormMode aFormMode);

	virtual SqlConnection* GetDefaultSqlConnection();
	
	// serve per riconoscere se ho l'interfaccia o meno al di sotto di TbGes
	virtual IBehaviourContext*	GetBehaviourContext	() { return	this; }

protected:

	// serve per gestire il messaggio nella statusbar per indicare che il
	// documento non e' usabile (colonna Disattivo nel DBTMaster) ma editabile
	virtual BOOL OnDisactiveIndicator	() { return FALSE; }

	virtual BOOL DispatchFunctionCall		(CFunctionDescription*);	
	virtual	void DispatchDocumentCreated	();
	virtual	void DispatchFrameCreated		();
	
//---------------> 
//---------------> START delle routine da ruotare ad eventuali add-on document
//---------------> 
//
protected:
	// routines fondamentali per la gestione della transazione
	virtual	LockStatus OnLockDocumentForNew		() { return ALL_LOCKED; }
	virtual	LockStatus OnLockDocumentForEdit	() { return ALL_LOCKED; }
	virtual	LockStatus OnLockDocumentForDelete	() { return ALL_LOCKED; }
	
	virtual BOOL OnOkEdit			();
	virtual BOOL OnOkNewRecord		();
	virtual	BOOL OnOkDelete			();
	virtual	BOOL OnOkTransaction	();
	virtual BOOL OnBeforeOkTransaction();
	
	// x le transazione secondarie che devono far parte dello stesso spazio
	// transazionale del documento. Se fallisce una transazione secondaria
	// fallisce tutta la transazione del documento
	virtual	BOOL OnNewTransaction	();
	virtual	BOOL OnEditTransaction	();
	virtual	BOOL OnDeleteTransaction(); 

	// x le transazione accessorie che NON devono far parte dello stesso spazio
	// transazionale del documento. Le transazioni accessorie sono chiamate dopo
	// la commit del documento (sia transazione primaria che secondaria). Il fallimento
	// di una transazione accessoria non implica il rollback di tutto il documenot
	virtual	void OnExtraNewTransaction		();
	virtual	void OnExtraEditTransaction		();
	virtual	void OnExtraDeleteTransaction	();

 	// Metodo per permettere ai documenti derivati di fare qualche cosa
 	// poco prima di andare in Browse Mode
	virtual void OnGoInBrowseMode	();

	// Permette di ridefinire dinamicamente il contenuto di un tooltip
	// Il conenuto di strMessage rispetta quanto MFC prevede per le risorse statiche: la parte fino al primo \n
	// va nella status bar (FlyBy), mantre la restante parte va nel tooltip
	// Tornando FALSE si accetta la gestione standard di MFC (cerca staticamente nelle risorse)
	virtual BOOL OnGetToolTipText	(UINT nId, CString& strMessage);


	virtual void OnBatchCompleted() {}; //per permettere al programmatore gestionale di poter intervenire al termine dell'elaborazione batch (dopo che è stata chiamata la OnAfterBatchExecute dei ClientDocs)
	virtual BOOL OnBeforeBatchExecute() { return TRUE; } //per permettere al programmatore gestionale di poter intervenire all'inizio dell'elaborazione batch 
	virtual void OnAfterBatchExecute()  {  } //per permettere al programmatore gestionale di poter intervenire alla fine dell'elaborazione batch 

public:
	// needed to connect DBT, HotKey and Auxiliary data
	virtual BOOL OnAttachData		();
	virtual BOOL OnPostAttachData	() { return TRUE; }

	virtual	void OnDocumentCreated	();
	virtual BOOL OnInitDocument		(); // permette di istanziare cursori, tablereader, tableupdater dopo aver costruito il contesto di documento
	virtual BOOL OnExistTables		();
	virtual BOOL OnInitAuxData		();
	virtual BOOL OnPrepareAuxData	();
	virtual BOOL ImportExportPrepareAuxData();
	virtual void InitializeHotFilter(HotFilterObj* hotFilter) {}


	// connessione della catena DBT al documento
	virtual	BOOL Attach	(DBTMaster* pDBTMaster);

	virtual BOOL			OnValidateRadarSelection	(SqlRecord* pRec, HotKeyLink* pHotKeyLink)								{ return OnValidateRadarSelection(pRec); } 
	virtual BOOL			OnValidateRadarSelection	(SqlRecord* pRec, CTBNamespace nsHotLinkNamespace, HotKeyLink* pHotKeyLink)	{ return OnValidateRadarSelection(pRec, nsHotLinkNamespace); } 
	virtual BOOL			OnValidateRadarSelection	(SqlRecord*)															{ return TRUE; } 
	virtual BOOL			OnValidateRadarSelection	(SqlRecord*, CTBNamespace nsHotLinkNamespace)							{ return TRUE; } 
	virtual BOOL			OnShowStatusBarMsg			(CString&)				{ return FALSE; }
	virtual CParsedCtrl*	OnCreateParsedCtrl			(UINT /*nIDC*/, CRuntimeClass* /*pParsedCtrlClass*/)	{ return NULL; }
	virtual void			OnBeforeSave				();					
	virtual void			OnAfterSave					();
	virtual void			OnBeforeDelete				()						{ }
	virtual void			OnAfterDelete				()						{ }
	
	virtual CString			FormatRollbackLogMessage	();
	virtual void DispatchBuildingSecurityTree	(CTBTreeCtrl* pTree, Array* arInfoTreeItems);

//---------------> 
//---------------> END delle routine da ruotare ad eventuali add-on document
//---------------> 

public:		
	void	SuspendUpdateDataView	();
	void	ResumeUpdateDataView	();
	void	UpdateDataView			(BOOL bForce = FALSE);

	virtual void	AbortAllViews	();

	void	InvalidateActiveTabDialog();
	void	EnableAllControlLinks	(BOOL bEnable = TRUE);
	void	InitViewsFormFlags		();
	CWnd*	GetWndLinkedCtrl		(UINT nIDC);
	virtual CWnd*	GetWndLinkedCtrl(const CTBNamespace& aNS);
	CWnd*	GetWndCtrl				(UINT nIDC);
	CView*	GetNotValidView			(BOOL bSignalError = FALSE);
	CView*	CreateSlaveView			(const CRuntimeClass* pClass, UINT nSubTitleID, const CRuntimeClass* pClientClass = NULL, const CString& strFormName = _T(""), CWnd* pParent = NULL, BOOL bModal = FALSE);
	CView*	CreateSlaveView			(const CRuntimeClass* pClass, const CString& strSubTitle, const CRuntimeClass* pClientClass = NULL, const CString& strFormName = _T(""), CWnd* pParent = NULL, UINT nViewId = 0, UINT nFrameId = 0, BOOL bModal = FALSE);
	CView*	CreateSlaveView			(UINT nFrameId, CWnd* pParent = NULL, BOOL bModal = FALSE);
	void 	ConnectForm				(HotKeyLink*, SqlRecord* pHLRecord);
	BOOL	TabDialogActivate		(UINT nTabIDC, UINT nIDD);
	BOOL	TabDialogShow			(UINT nTabIDC, UINT nIDD, BOOL bValue);
	BOOL	TabDialogEnable			(UINT nTabIDC, UINT nIDD, BOOL bValue);
	BOOL	TileDialogEnable		(UINT nIDDTileGroup, UINT nIDD, BOOL bValue);
	BOOL	TilePanelEnable			(UINT nIDDTileGroup, UINT nIDD, BOOL bValue);
	BOOL	TileGroupActivate		(UINT nIDDTileManager, UINT nIDDTileGroup);

	/*TBWebMethod*/	DataBool	TabDialogActivate		(DataStr NsTab);
/*TBWebMethod*/	DataBool	PressToolbarButton		(DataStr NsButton);

	void	DoCustomize				(BOOL bExecEnabled = TRUE, LPCTSTR pszNewFile = NULL);
	void	DisableAllView			();
	void	EnableAllView			();
	BOOL	UseWoormRadar			();

	CMasterFrame*	GetMasterFrame			() const;
	CParsedCtrl*	GetLinkedParsedCtrl		(UINT nIDC);
	CParsedCtrl*	GetLinkedParsedCtrl		(const CTBNamespace& aNS);
	CParsedCtrl*	GetLinkedParsedCtrl		(DataObj* pDataObj);
	CBodyEdit*		GetBodyEdits			(CTBNamespace aNS);
	CStateCtrlObj*	GetLastStateCtrlChanged	(UINT nIDC);

	// per la gestione dei bottoni in una toolbar
	// permette di nascondere/visualizzare un bottone presente in una delle toolbar della MasterFrame 
	// (sia principale che secondaria).
	// se viene nasconsto l'ultimo bottone visibile di un gruppo di bottoni il metodo cancella 
	// l'eventuale separator alla sua sinistra. Una volta cancellato il separator
	// non può essere più visualizzato successivamente se decido di rendere di nuovo visibili il bottone
	BOOL			HideButton		(UINT nID, BOOL bHide  = TRUE);
	BOOL			DeleteButton	(UINT nID);
	
	//data la runtime class
	// * di un DBTObject: restituisce (se esiste nella lista dei dbt associati al dbt master o se 
	//   associati al dbt master o se  è lo stesso dbt master) il puntatore all'istanza del dbt
	// * di un SqlRecord: restituisce (se esiste come sqlrecord associato ad un dbt) il puntatore 
	//   all'istanza del sqlrecord corrente
	// * di un CClientDoc: restituisce (se esiste nella lista dei clientdoc associati al documento)
	//	 il puntatore all'istanza del clientdoc
	DBTObject*						GetDBTObject		(const CRuntimeClass*	pDBTClass)		 const;	
	DBTObject*						GetDBTObject		(const CString&			sTableName)		 const;	
	DBTObject*						GetDBTObject		(const CTBNamespace&	aNs)			 const;	
	DBTObject*						GetDBTObject		(SqlRecord*)							 const;
	DBTObject*						GetDBTByName		(const CString&			sDbtName)		 const;	
	SqlRecord*						GetCurrentRecord	(const CRuntimeClass*	pSqlRecordClass) const;
	CClientDoc*						GetClientDoc		(const CRuntimeClass*	pClientDocClass) const;
	CClientDoc*						GetClientDoc		(const CString&			pNamespace)		 const;
	CBusinessServiceProviderObj*	GetBSP				(const CString&			pNamespace)		 const;
	SqlAddOnFieldsColumn*			GetAddOnFields		(const CRuntimeClass*	pSqlClass, const CRuntimeClass* pAddOnFieldsClass) const;

	CTBNamespaceArray*		GetClientDocNamespaces () const;

	BOOL BrowseRecordByTBGuid(const CString& strDataGuid);
	BOOL BrowseRecordByTBGuid(const DataGuid& dataGuid);

	BOOL BrowseRecord		(BOOL bFocus = TRUE, BOOL bCreating = FALSE);

	void SetBatchAborted(LPCTSTR lpszErrorMessage = NULL);

	// per l'aggiunta di nuove TabDialog da parte dei ClientDoc associati al documento
	void	AddClientDocTabDlg		(CTabManager*);
	void	AddClientDocTileDialog	(CTileGroup*);
	// per l'aggiunta di nuove colonne nel BodyEdit da parte dei ClientDoc
	void	CallCustomizeBodyEdit(CBodyEdit*);
	void    CallCustomizeGridControl(CTBGridControl*);

	// aggancio al documento gli eventuali criteri di esportazione/importazione XTech cablati programmativamente
	void	AttachXMLExpCriteria(CXMLBaseAppCriteria*);
	void	AttachXMLImpCriteria(CXMLBaseAppCriteria*);

	//dispatcher verso i clientdoc dell'evento On...
	BOOL DispatchOnShowingPopupMenu (UINT, CMenu*); 
	BOOL DispatchOnGetCustomColor (const CBodyEdit*, CBodyEditRowSelected* /*CurRow*/);
	BOOL DispatchOnDblClick (const CBodyEdit*, UINT /*nFlags*/, CBodyEditRowSelected* /*CurRow*/); 
	BOOL DispatchOnEnableTabSelChanging(UINT /*nTabber*/, UINT /*nFromIDD*/, UINT /*nToIDD*/);
	void DispatchOnTabSelChanged(UINT /*nTabber*/, UINT /*nTabIDD*/);
	BOOL DispatchOnShowingBodyEditContextMenu(CBodyEdit*, CMenu*, int /*nCol*/, int /*nRow*/, CPoint /*ptClient*/); 
	
	BOOL DispatchOnPostCreateClient(CBodyEdit*);
	virtual BOOL OnPostCreateClient(CBodyEdit*)  { return TRUE; }

	void DispatchOnModifyDBTDefineQuery		(DBTObject*, SqlTable*); //devo ruotare la chiamata anche ai clientdoc
	void DispatchOnModifyDBTPrepareQuery	(DBTObject*, SqlTable*);
	void DispatchOnPrepareBrowser			(SqlTable*);
	void DispatchOnPrepareFindQuery			(SqlTable*);
	void DispatchOnAfterCreateAndInitDBT	(DBTObject*);

	void DispatchOnDuringBatchExecute (SqlRecord*); // Permette di agire sul record correttamente processato dalla procedura batch
													// è il programmatore gestionale che deve inserirlo nella sua procedura batch	
	
	void DispatchOnLoadAttachedDocument	(CFinderDoc* ); //permette al clientdoc di intervenire subito dopo la DispatchLoadAttached del CFinderDoc passato come parametro
	// chiama il metodo virtuale del documento e ruota sui clientdoc
	void DispatchOnModifyHKLDefineQuery		(HotKeyLink*, SqlTable*, HotKeyLink::SelectionType nQuerySelection = HotKeyLink::DIRECT_ACCESS);
	void DispatchOnModifyHKLPrepareQuery	(HotKeyLink*, SqlTable*, DataObj*, HotKeyLink::SelectionType nQuerySelection = HotKeyLink::DIRECT_ACCESS);
	int  DispatchOnModifyHKLSearchComboQueryData (HotKeyLink*, const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions);
	BOOL DispatchOnHKLIsValid				(HotKeyLink* pHotLink);
	virtual void OnPrepareForFind(HotKeyLinkObj* pHKL, SqlRecord* pRec);//called to customize hotlink behavior before FindRecord
	virtual void OnPrepareAuxData(HotKeyLinkObj* pHKL);//called to customize hotlink data after FindRecord
	// permette di intervenire in fase di run dell'hotlink
	void DispatchBeforeCallLink(CParsedCtrl* pCtrl);
	void DispatchOnHotLinkRun();
	void DispatchOnHotLinkStop();

	BOOL DispatchRunReport	(CWoormInfo*);
	BOOL DispatchToolbarDropDown(int nID, CMenu& menu);

	BOOL			DispatchOnValidateRadarSelection	(SqlRecord*, HotKeyLink* pHotKeyLink = NULL);
	BOOL			DispatchOnValidateRadarSelection	(SqlRecord*, CTBNamespace nsHotLinkNamespace, HotKeyLink* pHotKeyLink = NULL);
	BOOL			DispatchOnShowStatusBarMsg			(CString&);
	CParsedCtrl*	DispatchOnCreateParsedCtrl			(UINT /*nIDC*/, CRuntimeClass* /*pParsedCtrlClass*/);
	void			DispatchOnBeforeSave				();
	void			DispatchOnAfterSave					();
	void			DispatchOnBeforeDelete				();
	void			DispatchOnAfterDelete				();

	void NotifyCommand(UINT nIDC);
	
	//selezione del report di documento da dialog e non da menu, nel caso il documento abbia piu di 15(gli ID riservati per le voci di menu) report associati
	void SelReport(UINT idxReport);

	//reprepare DBTSlave/DBTSlaveBuffered primary key segments
	void ForcePreparePrimaryKey();

	void OnFormHelp();
	void OnFavorites();




	//EasyBuilder
	virtual void LoadSymbolTable();
	void ReloadSymbolTable();
	void AddToSymbolTable(SqlRecord* pRecord, const CString& strObjectName, BOOL bDeprecated = FALSE);
	//----
	virtual void PrepareSymbolTable ();	//Nuova gestione 
	virtual BOOL PrepareSymbolField (SymField*) { return TRUE; }


public:		//	Function Member
	virtual BOOL InitDocument	();
	virtual	void OnFrameCreated ();
	virtual void DeleteContents	();

	//external controller functions (scheduler)
	void ValorizeControlData(ControlLinks *pLinks, BOOL bNotifyChange);
	void RetrieveControlData(ControlLinks *pLinks);
	virtual void ExecuteBatchFromExternalController();
public:
	virtual BOOL SetControlAutomaticExpression	(UINT nID, const CString& strExp);
	virtual BOOL GetControlAutomaticExpression	(UINT& nID, CString& strExp);
	
	// needed for accelerator command skip
	virtual BOOL CanDoEditRecord	();
	virtual BOOL CanDoDeleteRecord	(); 
	virtual BOOL CanDoNewRecord		();

	virtual BOOL OnBeforeEditRecord		() { return TRUE; }
	virtual BOOL OnBeforeDeleteRecord	() { return TRUE; }
	virtual BOOL OnBeforeNewRecord		() { return TRUE; }

	virtual BOOL CanDoSaveRecord	();
	virtual BOOL CanDoFindRecord	();
	
	virtual BOOL CanDoFirstRecord	();
	virtual BOOL CanDoPrevRecord	();
	virtual BOOL CanDoNextRecord	();
	virtual BOOL CanDoLastRecord	();

	virtual	BOOL CanCloseDocument   ();		
	virtual BOOL CanDoEscape		(); //controlla solo se ci sono transazioni pending
	virtual BOOL CanDoQuery			();
	virtual BOOL CanDoReport		();
	virtual BOOL CanDoRadar			();
	virtual BOOL CanDoCustomize		();
	virtual BOOL CanDoExecQuery		();
	virtual BOOL CanDoEditQuery		();
	virtual BOOL CanDoRefreshRowset ();
    
	virtual BOOL CanDoBatchExecute	() { return TRUE; }
	virtual BOOL CanDoPauseResume	() { return m_bBatchRunning; }
	virtual BOOL CanDoRunWrmRadar	() { return TRUE; }

	// On...
	virtual	void DisableControlsForBatch	();
	virtual	void DisableControlsForAddNew	();
	virtual	void DisableControlsForEdit		();
	virtual	void EnableControlsForFind		();
	virtual	void DisableControlsAlways		();
	
	virtual BOOL CanDoImportXMLData	() { return !GetXmlDescription() || !GetXmlDescription()->IsTransferDisabled(); }
	virtual BOOL CanDoExportXMLData	() { return !GetXmlDescription() || !GetXmlDescription()->IsTransferDisabled(); }

	virtual void OnInitializeUI					(const CTBNamespace& aFormNs);
	virtual void OnInitializeUI					(CParsedForm*);
	virtual void OnDestroyTabDialog				(CTabDialog*);
	virtual void OnBuildDataControlLinks		(CTabDialog*);
	virtual void OnBuildDataControlLinks		(CAbstractFormView*);
	virtual void OnBuildDataControlLinks		(CTileDialog*);
	virtual void OnPrepareAuxData				(CTabDialog*);
	virtual void OnPrepareAuxData				(CAbstractFormView*);
	virtual void OnPrepareAuxData				(CTileGroup*);
	virtual void OnPrepareAuxData				(CTileDialog*);
	virtual void OnUpdateTitle					(CTileDialog*){}
	virtual CString OnGetCaption(CAbstractFormView*) { return _T(""); }

	virtual void DispatchDisableControlsForBatch	();
	virtual	void DispatchDisableControlsForAddNew	();
	virtual	void DispatchDisableControlsForEdit		();
	virtual	void DispatchEnableControlsForFind		();
	virtual	void DispatchDisableControlsAlways		();
	virtual BOOL DispatchOnBeforeBatchExecute		();
	virtual void DispatchOnAfterBatchExecute		();

	//da disabilitare i campi se il documento è chiamato da HotLink
	virtual void OnDisableControlsForCallLink() {};


	// dynamic variabiles
	CXMLVariable*		GetVariable				(const CString& sName);
	DataObj*			GetVariableValue		(const CString& sName);
	void				BindVariable			(const CString& sName, DataObj* pDataObj);
	void				DeclareVariable			(const CString& sName, DataObj& aDataObj);
	void				DeclareVariable			(const CString& sName, DataObj* pDataObj, BOOL bOwnsDataObj = FALSE, BOOL bReplaceExisting = FALSE);
	virtual BOOL		ValorizeVariable		(const CString& sName, const CString& sValue);
	void				RemoveVariable			(const CString& sName);

	void				GetJson					(CJsonSerializer& jsonSerializer, BOOL bOnlyWebBound);
	void				SetJson					(CJsonParser&	  jsonParser);
	virtual CString		GetComponentId			();
	
	virtual void		OnAddFormsOnDockPane	(CTaskBuilderDockPane* pPane);

public:	
	// advanced microarea overridable

	// needed to connect HotLink caller
	virtual	void OnHotLinkDied		(HotKeyLink*);
	// connessione degli hotlinks
	virtual void Attach	(HotKeyLink*, CString strName = _T(""));

	//gestore degli eventi di importazione da file XML
	virtual void Attach	(CEventManager*);

	//gestore delle funzioni esterne del file di trasformazione XSLT
	virtual void Attach	(CXSLTFunction*);

	// per gestire la disabilitazione/abilitazione del documento
	// quando un hotlink e` running
	virtual	void OnHotLinkRun		();
	virtual	void OnHotLinkStop		();

	// needed to connect Radar called
	virtual	void OnRadarDied			(ITBRadar*);
	virtual	void OnRadarRecordSelected	(BOOL bMustActive = FALSE, BOOL bForceEditing = FALSE);
	virtual	BOOL OnRadarRecordNew		();

	// needed to connect Radar called
	virtual void OnCustomizeWrmRadar	() {}

	// Gestiscono la presentazione di default in Radar ed in WrmRadar
	void	ClearVisible();
	void	SetVisible	(const DataObj* pDataObj,	BOOL bVisible = TRUE);
	
	// overridable
	virtual LRESULT	OnValueChanged	(WPARAM, LPARAM);
	
	// per gestire AbstractDocumentManager reimplementare tornando
	// la derivazione destra
	virtual	ADMObj*	GetADM () { return NULL; }

	// utile per il banner 
	virtual CString GetOpeningBanner();
    
    // Permette di customizzare l'evento di right click su una scheda del Tabber
	virtual void OnTabRButtonDown(UINT/*nIDD*/) {}

	// metodi virtuali per poter modificare o sostituire la query del DBT passato come parametro
	// agendo dal documento (vedi ADM) e solo per la modifica anche dai clientdoc
	virtual void OnModifyDBTDefineQuery	 (DBTObject*, SqlTable*) {}
	virtual void OnModifyDBTPrepareQuery (DBTObject*, SqlTable*) {}
	virtual void OnModifyPrepareFindQuery(SqlTable*) {}
	virtual void OnModifyPrepareBrowser  (SqlTable*) {}
	virtual void OnAfterCreateAndInitDBT (DBTObject*) {} //serve per poter intervenire alla fine del processo di creazione del DBT

	// chi reimplementa questi metodi deve ritornare TRUE
	virtual BOOL OnChangeDBTDefineQuery	(DBTObject*, SqlTable*)	{ return FALSE; }
	virtual BOOL OnChangeDBTPrepareQuery(DBTObject*, SqlTable*)	{ return FALSE; }

	// permettono di poter modificare la query dell'hotlink passato come parametro
	virtual void OnModifyHKLDefineQuery	(HotKeyLink*, SqlTable*, HotKeyLink::SelectionType  = HotKeyLink::DIRECT_ACCESS)		   {}
	virtual void OnModifyHKLPrepareQuery(HotKeyLink*, SqlTable*, DataObj*, HotKeyLink::SelectionType  = HotKeyLink::DIRECT_ACCESS) {}
	virtual int  OnModifyHKLSearchComboQueryData(HotKeyLink*, const int& nMaxItems, DataObjArray& pKeyData, CStringArray& arDescriptions) { return 0; }

protected:
	// permetteno di personalizzare l'inserimento al volo degli hkl nel caso in cui il record dell'hotlink non sia lo stesso di quello del DBTMaster
	virtual 	void	AssignHklRecordToMaster	();
	virtual 	void	AssignMasterRecordToHkl	();
	
public:	//permettono di personalizzare il comportamento dei controlli creati da json
	virtual 	BOOL	CanCreateControl(UINT idc);//permette di evitare la creazione del controllo o della colonna
	virtual 	void	OnParsedControlCreated(CParsedCtrl* pCtrl);//chiamato dopo la creazione del controllo, permette di modificarne lo stato
	virtual 	void	OnColumnInfoCreated(ColumnInfo* pColInfo);//chiamato dopo la creazione della colonna, permette di modificarne lo stato
	virtual 	void	OnPropertyCreated(CTBProperty* pProperty);//chiamato dopo la creazione della property, permette di modificarne lo stato
	virtual		BOOL	OnGetToolTipProperties(CBETooltipProperties* pTooltip);
	virtual 	void	EnableBodyEditButtons(CBodyEdit* pBodyEdit);
	virtual 	BOOL	CanDoDeleteRow(CBodyEdit* pBodyEdit) { return TRUE; }
	virtual		void	CustomizeGauge(CTBLinearGaugeCtrl* pGaugeCtrl);

			BOOL DoSave				(LPCTSTR pszPathName, BOOL bReplace = TRUE);
	virtual BOOL SaveModified		(); // return TRUE if ok to continue
	virtual void OnCloseDocument	();
	virtual	BOOL CloseDocument		(BOOL bAsync = TRUE);
	

	//Utilizzata da Woorm, non reimplementare
	virtual BOOL GoInBrowserMode	(CFunctionDescription*);
	virtual BOOL GoInBrowserMode	(const CString& sPK);
	virtual BOOL GoInBrowserMode	(const SqlRecord* pRec);

	virtual CSingleExtDocTemplate*	GetDocTemplateFromSqlRecordRTC	(CRuntimeClass* rtcSqlRecordMaster);

	// customize del documento prima della effettiva creazione del browser
	virtual BOOL OnCreateBrowser(LPAUXINFO /*pInfo*/) { return TRUE;/* do nothing */ }
	//
	virtual void CustomizeBodyEdit		(CBodyEdit*)		{ }
	virtual void CustomizeGridControl	(CTBGridControl*)	{ }

	virtual BOOL		OnBEBeginMultipleSel(CBodyEdit*) { return FALSE; }
	virtual BOOL		OnBEEndMultipleSel(CBodyEdit*) { return FALSE; }
	virtual	BOOL		OnBECustomizeSelections(CBodyEdit*, SelArray&) { return FALSE; }

	BOOL		DispatchOnBEBeginMultipleSel(CBodyEdit*);
	BOOL		DispatchOnBEEndMultipleSel(CBodyEdit*);
	BOOL		DispatchOnBECustomizeSelections(CBodyEdit*, SelArray&	sel);

	virtual BOOL		OnBECanDoDeleteRow(CBodyEdit*) { return TRUE; }
	BOOL		DispatchOnBECanDoDeleteRow(CBodyEdit*);

	CBEButton*		GetBEButton(UINT idBody, UINT idBtn);

	//called by EasyAttachment to allow the document to do something at the end of the attaching processing
	virtual void OnAfterAttachProcess() {}

	// può essere reimplementato. Vedi Wizard di esportazione
	virtual CXMLBaseAppCriteria* GetBaseExportCriteria	() const { return m_pBaseExportCriteria; }
	virtual CXMLBaseAppCriteria* GetBaseImportCriteria	() const { return m_pBaseImportCriteria; }

	virtual CXMLVariableArray*		GetVariableArray		() const { return m_pVariablesArray; }

	virtual void OnModifyAuxiliaryToolbar() {} //per modificare la toolbar ausiliaria
	virtual BOOL OnGetCustomColor (const CBodyEdit*, CBodyEditRowSelected* /*CurRow*/) { return FALSE;} 	
	virtual BOOL OnDblClick (const CBodyEdit*, UINT /*nFlags*/, CBodyEditRowSelected* /*CurRow*/) { return FALSE; } 
	virtual BOOL OnEnableTabSelChanging(UINT /*nTabber*/, UINT /*nFromIDD*/, UINT /*nToIDD*/) { return TRUE; } 
	virtual void OnTabSelChanged(UINT /*nTabber*/, UINT /*nTabIDD*/) {}
	virtual BOOL OnShowingPopupMenu	(UINT, CMenu*) { return TRUE; }
	virtual BOOL OnRunReport	(CWoormInfo*) { return TRUE; }
	virtual BOOL OnToolbarDropDown (UINT, CMenu& ) { return FALSE; }

	//reimplementabile per effettuare dei controlli in fase iniziale ed eventualmente abortire l'apertura del documento
	virtual BOOL CanRunDocument () { return TRUE; } 


	//Improvement #5062: SosConnector
	virtual SWORD GetFiscalYear			() { return MIN_YEAR; }
	virtual CString GetSosSuffix		() { return _T(""); }
	//Improvement #5372: creato per procedura stampa di 	
	virtual CString GetSosDocumentType	() { return _T(""); }

	//import/export function
	BOOL	SaveImportDocument		();					// per il salvataggio del documento importato
	BOOL	DeleteImportDocument	();					// per la cancellazione del documento importato
	void	EscapeImportDocument	() {GoInBrowseMode();} // per la cancellazione dell'importazione

	//impr. 5320 
	BOOL	SetDataFromXMLString(CString strXML,  const CString& strXSLTFileName);
	CString	GetDataToXMLString(const CString& strProfileName, const CString& strXSLTFileName);

protected:
	BOOL CreateBrowser	(LPCTSTR pObject = NULL);
	void ReInitBrowser	();
	BOOL Escape			(BOOL bMessage = TRUE);
	virtual void ToolBarButtonsHideGhost(int nCase);

public:
	BOOL NewRecord		();
	BOOL EditRecord		();
	BOOL FindRecord		();
	BOOL DeleteRecord	();
	BOOL SaveRecord		();

public:
	BOOL DispatchOnPasteDBTRows			(CTBEDataCoDecPastedRecord&);
	BOOL DispatchOnValidatePasteDBTRows (RecordArray& arRows,	CTBEDataCoDecRecordToValidate&);
	BOOL DispatchOnValidatePasteDBTRows (SqlRecord* pRec,		CTBEDataCoDecRecordToValidate&);

	BOOL DispatchCanDoDeleteRecord		();
	BOOL DispatchCanDoEditRecord		();
	BOOL DispatchCanDoNewRecord			();

	BOOL DispatchOnBeforeDeleteRecord	();
	BOOL DispatchOnBeforeEditRecord		();
	BOOL DispatchOnBeforeNewRecord		();

	void DispatchOnActivate				(CAbstractFormFrame* pFrame, UINT nState, CWnd* pWndOther, BOOL bMinimized);

	BOOL DispatchOnBeforeEscape			();
	virtual  BOOL OnBeforeEscape		() { return TRUE; }

	virtual BOOL OnValidatePasteDBTRows (RecordArray& arRows,	CTBEDataCoDecRecordToValidate& vr)
				{
					for (int i= 0; i < arRows.GetSize(); i++)
						if (!DispatchOnValidatePasteDBTRows(arRows.GetAt(i), vr))
							return FALSE;
					return TRUE;
				}
/*OVERRIDABLE to convert and validate bodyedit rows pasted/dropped */	
	virtual BOOL OnPasteDBTRows (CTBEDataCoDecPastedRecord& pr)
		{
			BOOL bOk = pr.PasteFieldsValue();
			return TRUE;
		}
	virtual BOOL OnValidatePasteDBTRows	(SqlRecord* pRec, CTBEDataCoDecRecordToValidate&) { return TRUE; }
	
	void DispatchOnBESelCell		(CBodyEdit*, SqlRecord* , ColumnInfo* );
	void DispatchOnBEShowCtrl		(CBodyEdit*, SqlRecord* , ColumnInfo* );
	void DispatchOnBEHideCtrl		(CBodyEdit*, SqlRecord* , ColumnInfo* );
	void DispatchOnBEEnableButton	(CBodyEdit*, CBEButton*);

	virtual  void OnBESelCell		(CBodyEdit*, SqlRecord* , ColumnInfo* )  {}
	virtual  void OnBEShowCtrl		(CBodyEdit*, SqlRecord* , ColumnInfo* )  {}
	virtual  void OnBEHideCtrl		(CBodyEdit*, SqlRecord* , ColumnInfo* )  {}
	virtual  void OnBEEnableButton	(CBodyEdit* , CBEButton* ) {}
	void DoPinUnpin(CBaseTileDialog* pTileDialog);
	virtual	 void OnPinUnpin		(CBaseTileDialog* pTileDialog) {}
	virtual WebCommandType OnGetWebCommandType(UINT commandID);

	CRuntimeClass* DispatchOnModifySqlRecordClass	(DBTObject*, const CString& sDBTName, CRuntimeClass* pSqlRecordClass);
	virtual CRuntimeClass* OnModifySqlRecordClass	(DBTObject*, const CString& /*sDBTName*/, CRuntimeClass* pSqlRecordClass) { return pSqlRecordClass; }

	void FirstRecord	();
	void PrevRecord		();
	void NextRecord		();
	void LastRecord		();
	void FormReport		();
	void Radar			();
	void AltRadar		();
	void OldRadar		();
	void Query			();
	BOOL AsQuery		();
	void ExecQuery		();
	void FormHelp		();
	void RefreshRowset	();
	void Customize		();
	void NewWrmRadar	();
	void ExecSelQuery	(CString strQuery);
	void EditQuery		();
	void OtherQuery		();	
	void UndoChanges	();

	virtual void OnAfterLoadRadarColumns(ITBRadar*, SqlTable*) {}
	int GetAttachedReports(CReportMenuNode* arReports);
	int GetAttachedQueries(CStringArray& arQueries);
	
	BOOL CanShowQueryManager();

	afx_msg	void OnBatchStartStop	();
	afx_msg	void OnBatchPauseResume	();
	afx_msg void OnRefreshRowset();

protected:	// Generated message map functions
	//{{AFX_MSG(CAbstractFormDoc)
	afx_msg void OnBackMenu		();
	afx_msg void OnSwitchTo		();
	afx_msg void OnProgBarButton();
	
	afx_msg void OnNewRecord	();
	afx_msg void OnEditRecord	();
	afx_msg void OnFindRecord	();
	afx_msg void OnSaveRecord	();
	afx_msg void OnDeleteRecord	();
	afx_msg void OnActionsCopy	();	
	afx_msg void OnFirstRecord	();
	afx_msg void OnPrevRecord	();
	afx_msg void OnNextRecord	();
	afx_msg void OnLastRecord	();

	afx_msg void OnProperties	();
	afx_msg void OnEscape		();
	afx_msg void OnRadar		();
	afx_msg void OnAltRadar		();
	afx_msg void OnOldRadar		();
	afx_msg void OnWrmRadar		();
	afx_msg void OnNewWrmRadar	();
	afx_msg void OnFileWrmRadar (UINT nID);
	afx_msg void OnCustomize	();
	afx_msg void OnGenerateJsonModel();

	afx_msg void OnReport		();
	afx_msg void OnSelReport	(UINT nID);
	afx_msg void OnSwitchTo		(UINT nID);
	
	afx_msg void OnTabSwitch	  ();
	afx_msg void OnTabSwitchRange (UINT nID);

	afx_msg void OnFormReport	();
	afx_msg void OnListReports  ();
			void DoRunReport	(const CString& strReportName);

	afx_msg void OnQuery		();
	afx_msg void OnExecQuery	();
	afx_msg void OnEditQuery	();
	afx_msg void OnExecPostQuery ();
	//@@TODO da ripristinare
	afx_msg void OnExecSelQuery	(UINT nID);
	afx_msg void OnOtherQuery	();	
	//OLEDB
	
	afx_msg void OnUpdateSelReport		(CCmdUI*);

	afx_msg void OnUpdateNewRecord		(CCmdUI*);
	afx_msg void OnUpdateEditRecord		(CCmdUI*);
	afx_msg void OnUpdateFindRecord		(CCmdUI*);
	afx_msg void OnUpdateSaveRecord		(CCmdUI*);
	afx_msg void OnUpdateDeleteRecord	(CCmdUI*);
	afx_msg void OnUpdateTabSwitch		(CCmdUI*);

	afx_msg void OnUpdateFirstRecord	(CCmdUI*);
	afx_msg void OnUpdatePrevRecord		(CCmdUI*);
	afx_msg void OnUpdateNextRecord		(CCmdUI*);
	afx_msg void OnUpdateLastRecord		(CCmdUI*);

	afx_msg void OnUpdateEscape			(CCmdUI*);
	afx_msg void OnUpdateReport			(CCmdUI*);
	afx_msg void OnUpdateQuery			(CCmdUI*);
	afx_msg void OnUpdateRadar			(CCmdUI*);
	afx_msg void OnUpdateOldRadar		(CCmdUI*);
	afx_msg void OnUpdateWrmRadar		(CCmdUI*);
	afx_msg void OnUpdateCustomize		(CCmdUI*);
	afx_msg void OnUpdateGenerateJsonModel(CCmdUI*);
	afx_msg void OnUpdateExecQuery		(CCmdUI*);
	afx_msg void OnUpdateEditQuery		(CCmdUI*);
	afx_msg void OnUpdateRefreshRowset	(CCmdUI*);	

    // status bar message management
	afx_msg void OnUpdateFormModeIndicator		(CCmdUI*);
	afx_msg void OnUpdateMessageIndicator		(CCmdUI*);
	afx_msg void OnUpdateTotalRecordsIndicator	(CCmdUI*);
	afx_msg void OnUpdateDeletedIndicator		(CCmdUI*);
	afx_msg void OnUpdateDisactiveIndicator		(CCmdUI*);
	afx_msg void OnUpdateBatchStartStop			(CCmdUI*);
	afx_msg void OnUpdateBatchPauseResume		(CCmdUI*);
	afx_msg void OnUpdateFormHelp				(CCmdUI* pCmdUI);
	afx_msg void OnBarButton					(CCmdUI* pCmdUI);
	
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

public:
	void DumpDbts (const CString& sPath);
	void DumpDbts ();

// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};



typedef TDisposablePtr<CAbstractFormDoc> AbstractFormDocPtr;
//=============================================================================
class TB_EXPORT CWizardFormDoc : public CAbstractFormDoc
{
	friend class CWizardFormView;
	DECLARE_DYNCREATE(CWizardFormDoc)
	
public:
	AbstractFormDocPtr	m_pWaitingDoc;
	CString				m_strWizardTitle;
	bool				m_bReExecutable = true;
	DataStr				m_HeaderTitle;
	DataBool			m_bHeaderTitleBold;
	DataStr				m_HeaderSubTitle;
	DataBool			m_bHeaderSubTitleBold;

public:
	CWizardFormDoc(); 

	void	WaitDocument ();

protected:
	virtual void OnCloseDocument	();
	virtual BOOL OnAttachData 		();
	virtual void SetTitle 			(LPCTSTR lpszTitle);

	virtual LRESULT OnWizardNext(UINT /*IDD*/);
	virtual LRESULT OnWizardBack(UINT /*IDD*/);
	virtual LRESULT OnWizardCancel(UINT /*IDD*/);
	virtual LRESULT	OnWizardFinish(UINT /*IDD*/);
	virtual void OnWizardRestart() {}
	virtual	void OnFrameCreated();
	CWizardFormView* GetWizardView();
	CTabWizard* GetTabWizard();

public:
	LRESULT	DispatchWizardNext	 			(UINT nDlgIDD);
	LRESULT	DispatchWizardBack	 			(UINT nDlgIDD);
	LRESULT DispatchGetBitmapID  			(UINT nDlgIDD); 
	LRESULT	DispatchWizardFinish 			(UINT nDlgIDD);
	LRESULT	DispatchWizardCancel 			(UINT nDlgIDD);
	LRESULT	DispatchOnBeforeWizardFinish	(UINT nDlgIDD);

	void	DispatchUpdateWizardButtons(UINT nDlgIDD);
	void	DispatchDeactivate	 (UINT nDlgIDD);
	void	DispatchActivate	 (UINT nDlgIDD);

public:
	
	bool IsReExecutable() const
	{
		return m_bReExecutable;
	}
	void SetReExecutable(bool bValue)
	{
		m_bReExecutable = bValue;
	}

	virtual void				OnParsedControlCreated		(CParsedCtrl* pCtrl);
	void						SetHeaderTitle(DataStr sTitle, BOOL bBold = FALSE)
	{
		m_HeaderTitle			= sTitle;
		m_bHeaderTitleBold		= bBold;
	}

	void						SetHeaderSubTitle(DataStr sSubTitle, BOOL bBold = FALSE)
	{
		m_HeaderSubTitle		= sSubTitle;
		m_bHeaderSubTitleBold	= bBold;
	}
protected:	
	// Generated message map functions for local purpose
	//{{AFX_MSG(CWizardFormDoc)
	virtual void OnEnableWizardNext(CCmdUI* pCmdUI);
	virtual void OnEnableWizardBack(CCmdUI* pCmdUI);
	virtual void OnEnableWizardFinish(CCmdUI* pCmdUI);
	virtual void OnEnableWizardCancel(CCmdUI* pCmdUI);
	virtual void OnEnableWizardRestart(CCmdUI* pCmdUI);
	virtual BOOL CanDoBatchExecute();
	virtual void OnWizardActivate(UINT nPageIDD) {  }
	virtual void OnWizardDeactivate(UINT nPageIDD) {  }


	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;
#endif // _DEBUG
};
//=============================================================================
class TB_EXPORT CAuxiliaryFormDoc : public CAbstractFormDoc
{
public:
	CAuxiliaryFormDoc()
		: CAbstractFormDoc()
	{
		DisableFamilyClientDoc(TRUE);
	}
};

//==========================================================================================
#include "endh.dex"

