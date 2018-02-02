#pragma once 

#include <TbXmlCore\XMLTags.h>
#include <TBNameSolver\TBNamespaces.h>
#include <TBNameSolver\CallbackHandler.h>
#include <TbNameSolver\MacroToRedifine.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DocumentObjectsInfo.h>

#include "Messages.h"
#include "ExternalControllerInfo.h"
#include "oslinfo.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class Formatter;
class CBaseDocument;
class AddOnModule;
class CSingleExtDocTemplate;
class CXMLDataManagerObj;
class CAutoExpressionMng;
class CXMLBaseAppCriteria;
class CXMLVariableArray;
class CExternalControllerInfo;
class DocInvocationInfo;
class DocInvocationParams;
class SqlConnection;
class CTBContext;
class CContextBag;
class SqlSession;
class CDiagnostic;
class SymTable;
class CLocalizableFrame;
class CManagedDocComponentObj;
class CWoormDoc;
class CAbstractFormDoc;
class ADMObj;
class ITBRadar;
class CDataSynchroNotifierObj;

#define NO_MSG_BOX_SHOWN	-1

typedef void (*PCALLBACK) ();
enum WebCommandType
{
	WEB_UNDEFINED	= -1,
	WEB_UNSUPPORTED = 0,
	WEB_NORMAL		= 1,
	WEB_LINK		= 2
};

//=============================================================================
class TB_EXPORT CDocFieldNames
{
public:
	static const TCHAR DocumentDot[];

	static const TCHAR Document[];
	static const TCHAR Document_Name[];
	static const TCHAR Document_Namespace[];
	//enum CBaseDocument::FormMode { NONE, BROWSE, NEW, EDIT, FIND };
	static const TCHAR Document_FormMode[];
		static const TCHAR FormMode_New[]; 
		static const TCHAR FormMode_Edit[];
		static const TCHAR FormMode_Browse[];
		static const TCHAR FormMode_Find[];
	//enum CAbstractFormDoc::LockStatus { ALL_LOCKED, NO_AUX_DATA, LOCK_FAILED };
	static const TCHAR LockStatus_AllLocked[]; 
	static const TCHAR LockStatus_NoAuxData[]; 
	static const TCHAR LockStatus_LockFailed[]; 

	//enum CClientDoc::MsgState { ON_BEFORE_MSG, ON_AFTER_MSG };
	static const TCHAR AddOn[];
	static const TCHAR AddOn_MessageRoutingState[];
		static const TCHAR AddOn_MessageRoutingState_Before[];
		static const TCHAR AddOn_MessageRoutingState_After[];
};

class CTaskBuilderDockPane;
class CDesignModeManipulatorObj;
class CImportExportParams;

//=============================================================================
class TB_EXPORT CBaseDocument : public CDocument, public IDisposingSource, public IOSLObjectManager 
{
	friend class SqlLockTable;
	friend class SqlLockMng;
	friend class CEasyTraceMng;
	friend class CSingleExtDocTemplate;
	friend class SqlObject;
	friend class CTbCommandManager;
	friend class CAbstractFormDoc;
	friend class CDEasyBuilder;//Client document di EasyBuilder, serve per permettere ad EasyBuilder di impostare il booleano m_bIsInDesignMode
	friend class CEbCommandManager;//serve per impostare m_hFrameHandle
	friend class CRSHiddenProp;

protected: // create from serialization only
	DECLARE_DYNCREATE(CBaseDocument)

public:
	enum FormMode { NONE = 0, BROWSE = 1, NEW = 2, EDIT = 3, FIND = 4 };
	enum DesignMode { DM_NONE /*non sono in design mode*/, DM_RUNTIME /*Easy Studio*/, DM_STATIC /*JSON designer*/};
private:
	DesignMode					m_DesignMode = DM_NONE;
	CCallbackHandler			m_Handler;
	time_t						m_nCreationTime;  //last interaction Time in seconds since midnight (00:00:00), January 1, 1970, coordinated universal time (UTC).
	CMap <UINT, UINT, CRuntimeClass*, CRuntimeClass*> m_RegisteredControls;
protected:
	BOOL						m_bAborted;			// indica se il documento ha completato la fase di costruzione
	BOOL						m_bClosing;			// indica se il documento è in fase di chiusura
	BOOL						m_bRetryingLock;	// indica se il documento ha dei tentativi di lock in corso

	CXMLDataManagerObj*			m_pXMLDataManager;
	BOOL						m_bUnattendedMode;	// se il documento è istanziato senza interfaccia visibile (vedi importo\export o scheduler)
	
	DataObj*					m_pAssignedCounter;	// tiene traccia del fatto che al documento è stato assegnato un identificatore per evitare di assegnarlo più volte
	BOOL						m_bCacheCounter;	// pilota la gestione o meno dell'assigned counter
	CBaseDocument*				m_pCallerDocument;
	DocInvocationParams*		m_pDocInvocationParams; // Allocato da fuori ma distrutto qui
	DocInvocationInfo*			m_pDocInvocationInfo;	// Allocato da fuori ma distrutto qui

	ViewModeType				m_Type;	// tipo di documento/view mode istanziato 
	HWND						m_hFrameHandle;

	DWORD						m_ThreadId;			//id del thread su cui nasce ii documento

	SymTable*					m_pSymTable;

public :
	CObserverContext		m_ObserverContext;
	// Dati per gestire i Templates Batch che usano Frame e View particolari
	BOOL					m_bBatch;				// TODOBRUNA eliminare
	BOOL					m_bBatchRunning;
	BOOL					m_bBatchCloseAfterExecution;
	BOOL					m_bCloseChildReport;
	
	CSingleExtDocTemplate*	m_pTemplate; //Template da cui è stato istanziato

	CAutoExpressionMng*		m_pAutoExpressionMng;

	// posso indicare solo i datamember
	// la loro gestione viene effettuata a livello CAbstractFormDoc e a WoormDoc
	// onde evitare la dipendenza incrociata con TBOleDb
	SqlConnection*	m_pSqlConnection;	// connessione di lavoro del documeno
	CTBContext*		m_pTbContext;		// contesto di lavoro x le transazioni e la messaggistica
	CMessages*		m_pMessages;		// per la gestione della messaggistica DOC_DIAGNOSTIC
	CContextBag*	m_pContextBag;		// per la gestione della condivisione di informazioni tra documenti

	CExternalControllerInfo*	m_pExternalControllerInfo;	// informazioni relative al controllore esterno del documento (§es. scheduler)

	// per gestire l'inoltro del wm_syskeydown da parte delle PreTranslateMessage per evitare il loop
	BOOL	m_bForwardingSysKeydownToParent;
	BOOL	m_bForwardingSysKeydownToChild;

	//DataSyncro
	CDataSynchroNotifierObj*	m_pDataSynchroNotifier;

public:
	virtual CView*	GetNotValidView	(BOOL bSignalError = FALSE) { return NULL; }

private:
	DataInt				m_FormMode;

protected:
	// name of the current dynamic view that is going to create
	CTBNamespace		m_nsCurrentViewParent;
	CTBNamespace		m_nsCurrentRowView;

public :
	CBaseDocument();
	virtual ~CBaseDocument();

public:
	CRuntimeClass*		GetControlClass			(UINT id);
	void RegisterControl(UINT nIDD, CRuntimeClass* pClass);

	CTBNamespace&		GetNamespace			() { return GetInfoOSL()->m_Namespace; }
	void				SetNamespace			(const CTBNamespace& aNamespace);
	
	DWORD				GetThreadId				() const { return m_ThreadId; }
	CDiagnostic*		GetDiagnostic			() { return (CDiagnostic*) m_pMessages; }
	const CTBNamespace&	GetNsCurrentViewParent	() { return m_nsCurrentViewParent; }
	const CTBNamespace&	GetNsCurrentRowView		() { return m_nsCurrentRowView; }
	void				SetNsCurrentViewParent	(const CTBNamespace& aNs);
	void				SetNsCurrentRowView		(const CTBNamespace& aNs);
	time_t				GetCreationTime			() { return m_nCreationTime; }
	const CDocumentDescription*	GetXmlDescription ();

	virtual void	OnCloseDocument	();
	virtual BOOL	OnOpenDocument	(LPCTSTR);
			void	ClearMessageQueue();
	virtual	BOOL	CloseDocument	(BOOL bAsync = TRUE);

	// metodo che dice se il documento puo essere chiuso senza peredere informazioni o integrita,
	// i figli lo reimplementano se aggiungono logica per impedire la chiusura.
	// nelle implementazioni BISOGNA impostare il thread in unattended mode all'inizio del metodo e reimpostarlo in attended
	// mode in uscita per evitare visualizzazione di messagebox
	virtual	BOOL	CanCloseDocument (); 
	
	void	DestroyDocument	();
	void    DestroyFrameHandle();
	virtual void	UpdateFrameCounts();
	
	//CDocUIObjectsCache&	GetUIObjectsCache();

	//aggiunge una callback da chiamare alla distruzione del documento
	void AddDisposingHandler (CObject* pListener, ON_DISPOSING_METHOD pHandler) { m_Handler.AddDisposingHandler(pListener, pHandler); }
	void RemoveDisposingHandlers (CObject* pListener) { m_Handler.RemoveDisposingHandlers(pListener); }

	BOOL CanRunDocumentInStandAloneMode(); //restituisce TRUE se non c'è un unico documento aperto e se non ci sono altri utenti connessi all'azienda, FALSE altrimenti
										   //---------------------------------------------------------------------------
	void AssignParameters(const DataStr& arguments);

public:
	virtual void SetPathName(LPCTSTR lpszPathName, BOOL = TRUE)
		{ CDocument::SetPathName(lpszPathName, FALSE); }

	BOOL			ResourceAvailable	();
	BOOL			IsAborted			()	const { return m_bAborted; }
	//BOOL			IsLocked			()	const { return m_bLocked; }
	DesignMode		GetDesignMode()		const { return m_DesignMode; }
	bool			IsInDesignMode()	const { return m_DesignMode != DM_NONE; }
	bool			IsInStaticDesignMode()	const { return m_DesignMode == DM_STATIC; }
	void			SetDesignMode(DesignMode nDesignMode)	{ m_DesignMode = nDesignMode; }

public:
	const ViewModeType	GetType () const;
	void				SetType (const ViewModeType aMode);

	BOOL			IsBackgroundMode	()	const;	// TODOBRUNA eliminare
	BOOL			IsABatchDocument	()	const;
	BOOL			IsRunningAsADM		()	const;
	BOOL			IsABackgroundADM	()	const;
	BOOL			IsADataEntry		()	const { return GetType() == VMT_DATAENTRY; }
	
	CSingleExtDocTemplate * GetDocTemplate () const						{ return m_pTemplate; }
	CBaseDocument*	GetCallerDocument	() const						{ return m_pCallerDocument; }
	virtual void	AttachCallerDocument(CBaseDocument* pCallerDoc)		{ m_pCallerDocument = pCallerDoc; }
	inline BOOL		PostMessage(UINT Msg, WPARAM wParam, LPARAM lParam) { return ::PostMessage(m_hFrameHandle, Msg, wParam, lParam); }
	inline LRESULT	SendMessage(UINT Msg, WPARAM wParam, LPARAM lParam) { return ::SendMessage(m_hFrameHandle, Msg, wParam, lParam); }
	LRESULT			SendCommand(WPARAM wParam)							{ return SendMessage (WM_COMMAND, wParam, 0); }
	HWND			GetFrameHandle()									{ return m_hFrameHandle; } 
	BOOL			SetInForeground()									{ return ::SetForegroundWindow(m_hFrameHandle); }
	BOOL			InvokeRequired();

	virtual SymTable* GetSymTable() { return m_pSymTable; }

	CManagedDocComponentObj*	 GetManagedParameters ();
	CDesignModeManipulatorObj*	GetDesignModeManipulatorObj();
	CImportExportParams*		GetImportExportParams();

public:
	// interfaccia alla gestione dello schedulatore di task

	// usata da tutti i documenti batch per eseguire il vero e proprio lavoro
	// (in interattivo lo si fa con il bottone "inizio"
	//
	virtual void OnBatchExecute	() {/* default do nothig*/}

	// nID = 0 identifica l'inizio (nel get) o la fine (set) delle operazioni di
	// riempimento o lettura dei controls di una form schedulata
	//
	virtual BOOL SetControlAutomaticExpression	(UINT, const CString&)	{ return TRUE; }
	virtual BOOL GetControlAutomaticExpression	(UINT&, CString&)		{ return FALSE; }

	void AddFormsOnDockPane(CTaskBuilderDockPane* pPane);
	virtual void OnAddFormsOnDockPane(CTaskBuilderDockPane* pPane) {  };

public:
	virtual FormMode	SetFormMode		(FormMode aFormMode);			// return previous mode

	virtual BOOL		DispatchFunctionCall	(CFunctionDescription*)	{ return FALSE; }
	
	virtual	Formatter*	GetFormatter	(const CString&)  const	{ return NULL; }
	virtual	Formatter*	GetFormatter	(const DataType&) const	{ return NULL; }
	
			FormMode	GetFormMode		()	const		{ return (FormMode) (int) m_FormMode; }
			BOOL		UseEasyReading	()	const;
	
	virtual	void		OnFrameCreated			()	{ } //default does nothing
	virtual	void		OnDocumentCreated		()	{ } //default does nothing
	virtual	void		DispatchDocumentCreated	()	{ } //default does nothing
	virtual	void		DispatchFrameCreated	()  { } //default does nothing
	virtual	void		OnOpenCompleted			()  { } //default does nothing

	virtual CString		FormatRollbackLogMessage ();
	virtual void		SuspendUpdateDataView() {}
	virtual void		ResumeUpdateDataView()	{}
public:
	CXMLDataManagerObj* GetXMLDataManager	() const						{ return m_pXMLDataManager; }
	void				SetXMLDataManager	(CXMLDataManagerObj *pDataMng)	{ m_pXMLDataManager = pDataMng; }

	BOOL	IsExporting	() const;
	BOOL	IsImporting	() const;
	//I'm importing with optimizated algorithm. This method is used in ERP classes
	BOOL	IsImportingFastMode() const;

	//DataSynchro
	CDataSynchroNotifierObj*	GetDataSynchroManager()										const { return m_pDataSynchroNotifier; }
	void						SetDataSynchroManager(CDataSynchroNotifierObj* pDataSynchroNotifier) { m_pDataSynchroNotifier = pDataSynchroNotifier; }
	void						NotifiyToDataSynchronizer();
	void						RemoveNotifications();

	void	Activate	(CView* pView = NULL, BOOL bPostMessage = FALSE);
	
	CView*	GetFirstView() const;

	DocInvocationInfo*	GetDocInvocationInfo() { return	m_pDocInvocationInfo; }
	virtual IBehaviourContext*	GetBehaviourContext	() { return	NULL; }

public:
	virtual void 	OnBeforeXMLExport				() {}
	virtual void 	OnAfterXMLExport				() {}
	virtual BOOL 	OnOkXMLExport					() { return TRUE; }
	
	// che faccio se devo esportare un record con tutti i campi di universal key vuoti?
	// di default fallisce l'esportazione
	virtual BOOL 	OnOkXMLExportUniversalKeyEmpty	() { return FALSE; }

	// che faccio se devo importare un record con tutti i campi di universal key vuoti?
	// di default utilizzo la chiave primaria originaria
	virtual BOOL 	OnOkXMLImportUniversalKeyEmpty	() { return TRUE; }

	virtual void 	OnBeforeXMLImport				() {}
	virtual void 	OnAfterXMLImport				() {}
	virtual BOOL	OnPreparePKForXMLImport			() { return FALSE; }
	virtual BOOL 	OnOkXMLImport					() { return TRUE; }
	virtual BOOL 	OnOkXMLDeleteImport				() { return TRUE; }

	// determina se devo o meno aggiornare un record già esistente
	// in fase di importazione da XML (di default importa aggiornando)
	// se restituisce FALSE, fallisce l'importazione
	virtual BOOL 	OnOkXMLUpdateImport				() { return TRUE; }
	_declspec(deprecated)
	virtual void 	OnDeclareVariables				() {}

	virtual BOOL 	IsAWoormRunningMultithread		() { return FALSE; }

	// m_pAssignedCounter effettua un caching dei counters assegnati al documento:
	// se ad un documento viene assegnato un counter, ulteriori eventuali assegnazioni vengono ignorate
	// fino a che il documento non entra in stato di NEW
	// questo comportamento può essere abilitato invocando EnableCounterCaching(TRUE);
	// di default il comportamento è disattivo (per compatibilità col passato, onde evitare
	// la generazione di bachi non previsti)
	DataObj*		GetAssignedCounter				() {return m_bCacheCounter ? m_pAssignedCounter : NULL;}
	void			SetAssignedCounter				(DataObj* pDataObj);
	void			EnableCounterCaching			(BOOL bCacheCounter) {m_bCacheCounter = bCacheCounter;}

	// Dice se il documento è in modalità unattended (perchè gestisce solo logiche 
	// di business senza interfaccia (ad es. in fase import/export) o perchè lanciato 
	// dallo scheduler
	BOOL	IsInUnattendedMode () const;

	//Imposta m_bUnattendedMode e torna il suo old value
	BOOL   SetInUnattendedMode (BOOL bSet = TRUE);

	// per la gestione dei Task controllati esternamente
	virtual BOOL IsEditingParamsFromExternalController	() const;
	virtual BOOL IsRunningFromExternalController		() const;
	virtual BOOL IsExternalControlled					() const;
			void SetRunningTaskStatus					(CExternalControllerInfo::RunningTaskStatus status) const;

	//reimplementata da CAbstractFormDoc
	virtual BOOL	GoInBrowserMode					(CFunctionDescription*) { return FALSE; }
	virtual BOOL	GoInBrowserMode					(const CString&)		{ return FALSE; }
	//virtual BOOL	GoInBrowserMode					(LPCTSTR /*pszStreamSpecials*/) { return FALSE; };

	virtual BOOL	ShowingPopupMenu				(UINT, CMenu*) { return TRUE; }
	virtual CSingleExtDocTemplate*	GetDocTemplateFromSqlRecordRTC	(CRuntimeClass*) { return NULL; }

public:
	void UseAutoExpression							(BOOL);
	BOOL IsUsingAutoExpression						(){return (m_pAutoExpressionMng != NULL);}

	virtual CXMLBaseAppCriteria*	GetBaseExportCriteria	() const { return NULL; }
	virtual CXMLBaseAppCriteria*	GetBaseImportCriteria	() const { return NULL; }
	virtual CXMLVariableArray*		GetVariableArray		() const { return NULL; }
	
	virtual BOOL					ValorizeVariable		(const CString& sName, const CString& sValue) { return FALSE; }

	// restituisce il path in cui sono presenti gli eventuali dizionari associati al documento
	// se bStandard, restituisce il dictionary.bin della standard che contiene traduzioni relative ad oggetti NON
	// contenuti nella dll, altrimenti il dictionary.bin locale all'applicazione
	CString GetDictionaryPath(BOOL bStandard);

	// gestione delle sessioni di lavoro
	// questi metodi sono reimplementati in CAbstractFormDoce ed in WoormDocument
	virtual SqlSession*		GetReadOnlySqlSession	()  { return NULL; }
	virtual SqlSession*		GetUpdatableSqlSession  ()  { return NULL; }
	virtual SqlConnection*	GetSqlConnection		()  { return NULL; }

	virtual	int Message (LPCTSTR lpszText, UINT nType = MB_OK, UINT nIDHelp = 0, LPCTSTR lpszAdditionalText = NULL, CMessages::MessageType MsgType = CMessages::MSG_ERROR);

	virtual WebCommandType OnGetWebCommandType(UINT commandID) { return WEB_UNDEFINED; }

	virtual void AttachScriptClientDoc (CClientDocDescription* /*pDescri*/) {}
	virtual BOOL SaveModified (); // return TRUE if ok to continue

	virtual CString GetUICulture();
	virtual CString SetUICulture(const CString&);

	virtual void	UpdateDataView			(BOOL bForce = FALSE) {}

	virtual CFont*	SetCurrentFont(LOGFONT* /*pLogFont*/, CDC& /*DC*/) { return NULL; } //overridate by WoormDoc

	//its used to have the key description of the document primary key ovveridate in CAbstractFormDoc
	virtual CString GetKeyInXMLFormat() { return _T(""); }
	virtual CLocalizableFrame* GetFrame() const;  //{ return NULL; } //overridate by CAbstractFormDoc

	virtual CString GetDefaultMenuDescription();

	virtual void OnCustomizeRadarToolbar(CTBTabbedToolbar* pTabbedToolbar)  {}
	virtual BOOL CanShowInOpenDocuments() { return FALSE; }

	CView*	ViewAlreadyPresent(const CRuntimeClass* pClass, UINT nViewID = 0, BOOL bExact = FALSE) const;
	CView*	ViewAlreadyPresent(UINT nFrameId) const;
	virtual BOOL CanPushToClient();
	virtual void OnPrepareForFind(HotKeyLinkObj* pHKL, SqlRecord* pRec) {}//called to customize hotlink behavior before FindRecord
	virtual void OnPrepareAuxData(HotKeyLinkObj* pHKL) {}//called to customize hotlink data after FindRecord

protected:	
	
	void		OnBackToMenu();
	void		OnSwitchTo();
	void		OnSwitchTo(UINT nID);

	//{{AFX_MSG(CBaseDocument)
	afx_msg void OnClearMessageQueue();
	afx_msg void OnUpdateSwitchTo	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateBackToMenu	(CCmdUI* pCmdUI);
	afx_msg void OnUpdateActionsCopy(CCmdUI* pCmdUI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
public:
	/*TBWebMethod*/ DataBool GetMethods (DataArray& /*string*/ arMethods);

	// context related methods 
public:
	BOOL		CheckContextObject	(const CString& strName);
	DataStr*	LookupContextString	(const DataStr& aName);
	DataInt*	LookupContextInt	(const DataStr& aName);
	DataDate*	LookupContextDate	(const DataStr& aName);
	DataStr*	AddContextString	(const DataStr& aName);
	DataInt*	AddContextInt		(const DataStr& aName);
	DataDate*	AddContextDate		(const DataStr& aName);
	DataStr		ReadContextString	(const DataStr& aName);
	DataInt		ReadContextInt		(const DataStr& aName);
	DataDate	ReadContextDate		(const DataStr& aName);

	// Diagnostics
#ifdef _DEBUG
public:
	void Dump (CDumpContext&) const;
	void AssertValid() const;

	virtual void SetModifiedFlag(BOOL bModified = TRUE) 
		{ 
			//if (bModified)
			//{
			//	ASSERT(m_FormMode != BROWSE || m_bBatch);
			//}
			__super::SetModifiedFlag(bModified);
		}
#endif // _DEBUG
private:
	using CDocument::m_bModified;	//forzo l'utilizzo della SetModifiedFlag
};

//=============================================================================
class TB_EXPORT CUpdateDataViewLevel
{
private:
	CBaseDocument * m_pDoc;
public:
	CUpdateDataViewLevel(CBaseDocument* pDoc)
	{
		m_pDoc = pDoc;
		if (m_pDoc)
			m_pDoc->SuspendUpdateDataView();
	}
	~CUpdateDataViewLevel()
	{
		if (m_pDoc)
			m_pDoc->ResumeUpdateDataView();
	}
};

#define SUSPEND_UPDATE_DATA_VIEW() CUpdateDataViewLevel __upd(this);

typedef TDisposablePtr<CBaseDocument> BaseDocumentPtr;
typedef TDisposablePtr<CWoormDoc> WoormDocPtr;
typedef TDisposablePtr<ADMObj> ADMObjPtr;
///////////////////////////////////////////////////////////////////////////////
//	class CXMLDataManagerDocObj:
//		classe base di interfaccia per la gestione dell'import/export via XML
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CXMLDataManagerObj : public CObject
{   
	DECLARE_DYNAMIC(CXMLDataManagerObj)

public:
	enum XMLDataMngStatus 
		{ 
			XML_MNG_EXPORTING_DATA, XML_MNG_IMPORTING_DATA, XML_MNG_IDLE			
		};

protected:
	XMLDataMngStatus	m_nStatus;


public:
	virtual ~CXMLDataManagerObj();

	virtual	BOOL			IsExtRef					() const = 0;

	virtual void			SetUnattendedExportParams	(LPCTSTR, LPCTSTR, LPCTSTR, int, int, BOOL) = 0;
	virtual	BOOL			Export						() = 0;
	virtual int				GetExportCmdMsg				() const = 0;
	
	virtual void			SetUnattendedImportParams	(BOOL, BOOL, LPCTSTR) = 0;
	virtual	BOOL			Import						() = 0;
	virtual int				GetImportCmdMsg				() const = 0;
	
	virtual	CPropertyPage*	CreateProfilesWizardPropPage(const CTBNamespace&)	const = 0;
	virtual BOOL			GetUKCommonFunctionList		(CStringArray*) const = 0;
	virtual CString			GetProfilesPath				()				const = 0;
	virtual CString			GetProfileName				()				const = 0;
	virtual BOOL			UseOldXTechMode				() const = 0;
	virtual void			SetCachedDocumentBusy		(bool bBusy = true) = 0;
	virtual bool			GetCachedDocumentBusy		() = 0;

	XMLDataMngStatus		GetStatus					() { return m_nStatus;}
	void					SetStatus(XMLDataMngStatus eStatus) { m_nStatus = eStatus; }

	 //impr. 5320 
	virtual BOOL	SetDataFromXMLString(CString strXML,  const CString& strXSLTFileName) = 0;
	virtual CString	GetDataToXMLString(const CString& strProfileName, const CString& strXSLTFileName) = 0;		
};


///////////////////////////////////////////////////////////////////////////////
//	class CDataSynchroNotifierObj:
//		classe base di interfaccia per permettere la gestione della sincronizzazione dati
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CDataSynchroNotifierObj : public CObject
{
	DECLARE_DYNAMIC(CDataSynchroNotifierObj)

public:
	virtual ~CDataSynchroNotifierObj() {};

public:
	virtual void NotifiyToDataSynchronizer() = 0;
	virtual void RemoveNotifications() = 0;
};


#include "endh.dex"
