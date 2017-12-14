#pragma once

#include <TbGenlib\OslInfo.h>
#include <TbGes\ControlBehaviour.h>
#include "beginh.dex"

class CBaseApp;
class CBaseDocument;
class CAbstractFormDoc;
class CPadDoc;
class CWoormDoc;
class CWoormInfo;
class ADMObj;
class CFunctionDescription;
class FunctionDataInterface;
class CTBNamespace;
class HotKeyLinkObj;
class CTBNamespaceArray;
class CExternalControllerInfo;
class CDataFileInfo;
class CTBMultiAutoLock;
class DataObj;
class CDocumentDescription;
class CViewModeDescription;
class CTBContext;
class CDiagnostic;
class TBScript;
class SymTable;
class CBehavioursRegistryService;
class CItemSource;
class CValidator;
class CDataAdapter;

// Alcuni namespace di default utilizzati da TaskBuilder
//-----------------------------------------------------------------------------
TB_EXPORT extern const TCHAR szWoormNamespace[];
TB_EXPORT extern const TCHAR szNewReport[];
TB_EXPORT extern const TCHAR szNewReport[];
TB_EXPORT extern const TCHAR szOpenReport[];

// ViewModes
//-----------------------------------------------------------------------------
#define szDefaultViewMode		_T("Default")
#define szBackgroundViewMode	_T("BackGround")	// TODOBRUNA eliminare
#define szUnattendedViewMode	_T("Unattended")
#define szNoInterface			_T("NoInterface")

#define DEPRECATED_EXIST_POINTER "\nWARNING! This function is deprecated. If you want to check the existence of the object referenced by your pointer, you have to declare it using the TDisposablePtr smart pointer\nFor Example:\n\nWRONG CODE:\nCMyDoc* pDoc = AfxGetTbCmdManager()->RunDocument(...);\n...\nif (AfxGetTbCmdManager()->ExistDocument(pDoc))\n{\n...\n}\n\nRIGHT CODE:\nTDisposablePtr<CMyDoc> pDoc = AfxGetTbCmdManager()->RunDocument(...);\n...\nif (pDoc)\n{\n...\n}\n"
#define DEPRECATED_DESTROY "\nWARNING! This function is deprecated. \nPlease use the overload with the const pointer argument; if you want your pointer to be set to NULL, declare it using the TDisposablePtr smart pointer\nFor Example:\nCMyDoc* pDoc\nbecomes:\nTDisposablePtr<CMyDoc> pDoc \n\n"

// Events dell'applicativo
//-----------------------------------------------------------------------------
TB_EXPORT extern const TCHAR szApplicationStarted[];
TB_EXPORT extern const TCHAR szApplicationDateChanged[];
TB_EXPORT extern const TCHAR szOnDSNChanged[];
TB_EXPORT extern const TCHAR szOnQueryCanCloseApp[];
TB_EXPORT extern const TCHAR szOnBeforeCanCloseTB[];

// Espressioni non note a TB ed ottenute come events
//-----------------------------------------------------------------------------
TB_EXPORT extern const TCHAR szAccPeriodBeginDate[];
TB_EXPORT extern const TCHAR szAccPeriodEndDate[];
TB_EXPORT extern const TCHAR szPrevAccPeriodBeginDate[];
TB_EXPORT extern const TCHAR szPrevAccPeriodEndDate[];

//-----------------------------------------------------------------------------

enum TB_EXPORT FailedInvokeCode 
		{ 
			InvkNoError,
			InvkOslForbidden,
			InvkTooManyWindows,
			InvkApmCmdIdUnknown,
			InvkApmCmdType,
			InvkApmCmdInterface,
			InvkKindOfTemplate,
			InvkKindOfAdm,
			InvkUnknownTemplate,
			InvkUnknownConstraint,
			InvkCfgForbidden,
			InvkUnknownError,
			InvkLoginLocked,
			InvkOperationDateError
		};

CString FailedInvokeCodeDescription(FailedInvokeCode err);

typedef void* LPAUXINFO;
typedef LPCTSTR ADMClass;

// Macros per estrazione pulita con safe cast degli oggetti
//-----------------------------------------------------------------------------
#define GET_FILENAME(a)		(a ? ((DocInvocationInfo*)a)->m_lpszFileName : NULL)
#define GET_AUXINFO(a)		(a ? ((DocInvocationInfo*)a)->m_pAuxInfo : NULL)
#define GET_ANCESTOR(a)		(a ? ((DocInvocationInfo*)a)->m_pAncestorDoc : NULL)
#define GET_CONTEXT(a)		(a ? ((DocInvocationInfo*)a)->m_pTBContext : NULL)
#define GET_CONTEXTBAG(a)	(a ? ((DocInvocationInfo*)a)->m_pContextBag : NULL)




// interfaccia di comunicazione dei documenti
//==============================================================================
class TB_EXPORT DocInvocationInfo
{
	friend class CTbCommandManager;
	friend class CSingleExtDocTemplate;

public:
	LPCTSTR						m_lpszFileName;		// parametro che si sarebbe dovuto passare alla OpenDocumentFile
	CBaseDocument*				m_pAncestorDoc;		// documento padre di tutte le eventuali chiamate in cascata
	CExternalControllerInfo*	m_pControllerInfo;	// informazioni relative all'eventuale controller esterno (es. scheduler)
	LPAUXINFO					m_pAuxInfo;			// puntatore a dati specifici di comunicazione
	CTBContext*					m_pTBContext;		// passaggio di contesto nel caso in cui m_pAncestorDoc = NULL
	CContextBag*				m_pContextBag;		// passaggio del contextBag per condividere oggetti ma non contesto

public:
	DocInvocationInfo
		(
			CBaseDocument*				pAncestorDoc		= NULL,
			LPCTSTR						lpszFileName		= NULL,
			LPAUXINFO					pAuxInfo			= NULL,
			CExternalControllerInfo*	pControllerInfo		= NULL,
			CTBContext*					pTBContext			= NULL,
			CContextBag*				pContextBag			= NULL
		)
	{
		m_pAncestorDoc		= pAncestorDoc;
		m_lpszFileName		= lpszFileName;
		m_pAuxInfo			= pAuxInfo;
		m_pControllerInfo	= pControllerInfo;
		m_pTBContext		= pTBContext;
		m_pContextBag		= pContextBag;
	}
};

class CAbstractFormView;
class CTabDialog;
//==============================================================================
class TB_EXPORT CDesignModeManipulatorObj : public CObject
{
};


//==============================================================================
class TB_EXPORT CManagedDocComponentObj : public CObject
{
	DECLARE_DYNAMIC(CManagedDocComponentObj)

public:
	CManagedDocComponentObj () {}

public:
	virtual void CreateNewDocumentOf(CBaseDocument* pDoc) {};
	virtual CDesignModeManipulatorObj*	GetDesignModeManipulatorObj() { return NULL; }
};

//==============================================================================
class TB_EXPORT CImportExportParams : public CManagedDocComponentObj
{
	BOOL	m_bOnlyBusinessObject;

public:
	CImportExportParams(BOOL bOnlyBusinessObject)
		:
		m_bOnlyBusinessObject(FALSE)
	{
		m_bOnlyBusinessObject = bOnlyBusinessObject;
	}

	BOOL IsOnlyBusinessObject() const { return m_bOnlyBusinessObject; }
};

// It stores parameters needed by Tb to open a document. It stores DocInvocationInfo
// as first parameter in order to make the correct cast type if used by applications
//==============================================================================
class TB_EXPORT DocInvocationParams
{
	friend class CSingleExtDocTemplate;
	friend class CTbCommandManager;
	friend class CEbCommandManager;
	friend class CAbstractFormDoc;
	friend class CBaseDocument;
	friend class CWoormDoc;

private:
	DocInvocationInfo*				m_pDocInfo;		// DON'T MOVE IT FROM THE FIRST POSITION!!!!
	const CDocumentDescription*		m_pDocDescri;	// XML document description allows dynamic document to kwnow himself
	BOOL							m_bIsRunningAsADM;
	CManagedDocComponentObj*		m_pManagedParameters;

public:
	DocInvocationParams () 
		: 
		m_pDocInfo			(NULL),
		m_pDocDescri		(NULL),
		m_bIsRunningAsADM	(FALSE),
		m_pManagedParameters(NULL)
		{
		}
		~DocInvocationParams (){}
public:
	DocInvocationInfo* GetDocInvocationInfo() const { return m_pDocInfo; }
	const CDocumentDescription* GetDocumentDescription() const { return m_pDocDescri; }
};

//-----------------------------------------------------------------------------
enum TB_EXPORT LoadLibrariesMode { LoadNeeded, LoadAll, LoadBaseOnly };

//estensione comandi di EasyBuilder
//==============================================================================
class TB_EXPORT CEbCommandManagerObj
{
};

// interfaccia ai comandi esposti di TaskBuilder. I metodi sono tutti pure 
// virtual e vengono reimplementati solo dal componente che è in grado di
// raggrupparli e gestirli tutti. 
//==============================================================================
class TB_EXPORT CTbCommandInterface : public CObject
{
	friend class CApplicationsLoader;

public:
	// documents
	virtual FailedInvokeCode	CanDoRunDocument
									(
										LPCTSTR		pszDocNamespace, 
										LPCTSTR		pszViewMode		= szDefaultViewMode, 
										BOOL		bFromFunction	= FALSE,
										BOOL		bUseDiagnostic	= TRUE
									) = 0;
	virtual CBaseDocument*		RunDocument	
									(
										LPCTSTR					pszNamespace, 
										LPCTSTR					pszViewMode	 = szDefaultViewMode, 
										BOOL					bFromFunction= FALSE,
										CBaseDocument*			pAncestor	 = NULL, 
										LPAUXINFO				lpAuxInfo	 = NULL, 
										CBaseDocument**			ppExistingDoc = NULL, 
										FailedInvokeCode*		pFailedCode	 = NULL,
										CExternalControllerInfo*pControllerInfo = NULL,
										BOOL					IsRunningAsADM = FALSE,
										CTBContext*				pTBContext   = NULL,
										CManagedDocComponentObj* pMangedParams = NULL,
										CContextBag*			 pContextBag = NULL
									) = 0;

	virtual ADMObj*				RunDocument
									(
										ADMClass*			pszADMClass, 
										LPCTSTR				pszViewMode	 = szDefaultViewMode, 
										LPCTSTR				pszNamespace = NULL,
										CBaseDocument*		pAncestor	 = NULL, 
										LPAUXINFO			lpAuxInfo	 = NULL, 
										ADMObj**			ppExistingADM = NULL, 
										FailedInvokeCode*	pFailedCode	 = NULL,
										CTBContext*			pTBContext   = NULL,
										CContextBag*		pContextBag = NULL
									) = 0;

	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL ExistDocument		(CBaseDocument* pDocument, HWND& hwndFrame) = 0;
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL ExistDocument		(CBaseDocument* pDocument) = 0;
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL ExistDocument		(ADMObj* pAdm, HWND& hwndFrame) = 0;
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL ExistDocument		(ADMObj* pAdm) = 0;
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL ReportRunning		(CWoormDoc*) = 0;
	__declspec(deprecated(DEPRECATED_EXIST_POINTER))
	virtual BOOL EditorRunning		(CPadDoc* pPadDoc) = 0;
	
	__declspec(deprecated(DEPRECATED_DESTROY))
	virtual BOOL CloseDocument		(CBaseDocument*& pDocument, BOOL bAsync = FALSE) = 0;
	__declspec(deprecated(DEPRECATED_DESTROY))
	virtual BOOL DestroyDocument	(CBaseDocument*& pDocument) = 0;
	__declspec(deprecated(DEPRECATED_DESTROY))
	virtual BOOL DestroyDocument	(ADMObj*&) = 0;

	virtual BOOL CloseDocument		(const CBaseDocument* pDocument, BOOL bAsync = FALSE) = 0;
	virtual BOOL DestroyDocument	(const CBaseDocument* pDocument) = 0;
	virtual BOOL DestroyDocument	(const ADMObj*) = 0;

	
	virtual BOOL CanCloseDocument	(CBaseDocument* pDocument) = 0;
	virtual void WaitDocumentEnd	(CBaseDocument* pDocument) = 0;
	virtual BOOL CanUseNamespace	(CTBNamespace, OSLTypeObject, DWORD, CDiagnostic*, BOOL* = NULL) = 0;

	// functions
	virtual BOOL RunFunction	(CFunctionDescription*, FailedInvokeCode* pFailedCode = NULL) = 0;
	virtual BOOL RunFunction	(LPCTSTR sNamespace, FailedInvokeCode* pFailedCode = NULL) = 0;

	virtual BOOL FireEvent		(const CString& sEvent, FunctionDataInterface*, FailedInvokeCode* pFailedCode = NULL) = 0;
	virtual	BOOL ExistFunction	(const CTBNamespace& aNs) = 0;
	virtual BOOL GetFunctionDescription(const CTBNamespace& aNs, CFunctionDescription &aFunctionDescription, BOOL bEmitDiagnostic = TRUE) = 0;
	virtual BOOL GetFunctionDescription(LPCTSTR lpcszNamespace, CFunctionDescription &aFunctionDescription) = 0;
	
	virtual TBScript* CreateTbScript (CFunctionDescription* , SymTable* pSymTable) = 0;

	
	// editor
	virtual CPadDoc*		RunEditor		(CString sName = _T("")) = 0;

	// reports
	virtual CWoormDoc*		RunWoormReport		(LPCTSTR sReportNamespace, CBaseDocument* pCallerDoc = NULL, BOOL bUseRadarFrame = FALSE, BOOL bRunReport = TRUE) = 0;
	virtual CWoormDoc*		RunWoormReport		(CWoormInfo* pInfo, CBaseDocument* pCallerDoc = NULL, CExternalControllerInfo*pControllerInfo = NULL, BOOL bUseRadarFrame = FALSE, BOOL bRunReport = TRUE)= 0; 
	virtual void			WaitReportEnd		(CWoormDoc*) = 0;
	virtual void			WaitReportRunning	(CWoormDoc*) = 0;
	virtual BOOL			CloseWoormReport	(CWoormDoc*) = 0;
	virtual	BOOL			CloseReportReady	(CWoormDoc*) = 0;
	virtual BOOL			GetSchemaReportVariables(CTBNamespace& nsReport, DataTypeNamedArray& arReportColumns, DataTypeNamedArray& arReportAskFields) =0;

	// Reference Objects
	virtual FunctionDataInterface* GetHotlinkDescription	(const CTBNamespace& aHKLNs, BOOL& bIsHKLDynamic, BOOL& bIsHKLXml) = 0;
	virtual HotKeyLinkObj*		RunHotlink					(const CTBNamespace& aHKLNs, FailedInvokeCode* pFailedCode = NULL, CRuntimeClass** ppControlClass = NULL) = 0;
	virtual CItemSource*		CreateItemSource			(const CTBNamespace& aNs) = 0;
	virtual CValidator*			CreateValidator				(const CTBNamespace& aNs) = 0;
	virtual CDataAdapter*		CreateDataAdapters			(const CTBNamespace& aNs) = 0;
	virtual CControlBehaviour*  CreateControlBehaviour		(const CTBNamespace& aNs) = 0;
	
	virtual CString		   GetHotlinkQuery		(const CString& sNamespace, const CString& sParams, const int& nAction, const CString sFilter = _T(""), HotKeyLink* pHotlink = NULL) = 0;

	// DataFile
	virtual CDataFileInfo*  GetDataFileInfo				(LPCTSTR lpcszNamespace, BOOL bAllowChanges = FALSE, BOOL bUseProductLanguage = FALSE) = 0;
	virtual BOOL	  	    SaveDataFileInfo			(CDataFileInfo* pDFI) = 0; //impr 5177: BAUZI

	// DLL on demand
	virtual BOOL AutoRegisterLibrary (const CString& sLibraryNamespace, HINSTANCE hLib) = 0;
	virtual BOOL LoadNeededLibraries (const CTBNamespace& aComponentNamespace, CTBNamespaceArray* pLibDependencies = NULL, LoadLibrariesMode aMode = LoadNeeded) = 0;
	virtual BOOL IsDLLLoading		 () = 0;

	// messaggistica posticipata durante l'autoregistrazione delle DLL 
	_declspec(deprecated) virtual void		BeginDLLLoading		() = 0;
	_declspec(deprecated) virtual void		EndDLLLoading		() = 0;
	
	// Metodi per la Login e chiusura
	virtual BOOL	Login						(const CString&	sAuthenticationToken) = 0;
	virtual BOOL	GetCurrentUser				(CString&	strUser, CString&	strCompany) = 0;
	
	virtual DWORD	GetProcessID			() = 0;
	virtual BOOL	CanCloseTB				(BOOL = FALSE) = 0;
	virtual void	CloseTB					(BOOL = FALSE) = 0;
	virtual BOOL	CanChangeLogin			(BOOL bLock = FALSE) = 0;
	virtual int		ChangeLogin				(const CString&	sOldAuthenticationToken, const CString&	sNewAuthenticationToken, BOOL bLock = FALSE) = 0;
	virtual BOOL	IsTBLocked				() = 0;
	virtual BOOL	LockTB					(const CString&	sAuthenticationToken) = 0;
	virtual BOOL	UnLockTB				(const CString&	sAuthenticationToken) = 0;
	virtual BOOL	TableExists				(LPCTSTR sSqlTableName, LPCTSTR sSqlColumnName = NULL) = 0;
	virtual int		DisconnectCompany		(const CString&	sAuthenticationToken)= 0;
	virtual int		ReconnectCompany		(const CString&	sAuthenticationToken)= 0;
	
	// Native convert
	virtual CString	NativeConvert				(const DataObj*) = 0;
	virtual CBehavioursRegistryService*	GetBehaviourService	(const CString& sEntity) = 0;

private:
	virtual void	InitOnDemandEnabled		() = 0;

public:
	virtual CWinThread*				CreateDocumentThread(bool bManagedMessagePump = false, ThreadHooksState hookingState = ThreadHooksState::DEFAULT) = 0;
	virtual CEbCommandManagerObj*	GetEBCommands() = 0;
	virtual void					SetEBCommands(CEbCommandManagerObj* pCommands) = 0;

};

//-----------------------------------------------------------------------------
TB_EXPORT CTbCommandInterface*	AFXAPI AfxGetTbCmdManager	();


#include "endh.dex"