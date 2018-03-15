#pragma once

#include "TBWinThread.h"
#include "ApplicationContext.h"


#include "beginh.dex"

// defines for double epsilon precision
#define DATADBL_EPSILON_PRECISION_MAX 4
#define DATADBL_EPSILON_PRECISION_POS 0 
#define DATAMON_EPSILON_PRECISION_POS 1 
#define DATAQTY_EPSILON_PRECISION_POS 2 
#define DATAPERC_EPSILON_PRECISION_POS 3 

class CLoginContext;
class Formatter;
class IJsonModelProvider;
class CXMLVariable;
class DataObj;
class CAbstractFormDoc;

///////////////////////////////////////////////////////////////////////////////
//	class CSingleWorker:
//		rappresenta il singolo worker gestito dal gestionale
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CWorker : public CObject
{   
private:
	long m_lWorkerID;
	CString m_strName;
	CString m_strLastName;
	CString m_strCompanyLogin;
	bool m_bDisabled;
	CString m_strImagePath;
public:
	long			GetWorkerID()		const { return m_lWorkerID; }
	const CString&	GetName()			const { return m_strName; }
	const CString&	GetLastName()		const { return m_strLastName; }	
	const CString&	GetCompanyLogin()	const { return m_strCompanyLogin; }
	bool			GetDisabled()		const { return m_bDisabled; }
	const CString&	GetImagePath()		const { return m_strImagePath; }
	

public:
	CWorker(long lWorkerID, CString strName, CString strLastName, CString strCompanyLogin, bool bDisabled, 	CString strImagePath = _T("")) 
		:
	m_lWorkerID(lWorkerID),
	m_strName(strName),
	m_strLastName(strLastName),
	m_strCompanyLogin(strCompanyLogin),
	m_bDisabled(bDisabled),
	m_strImagePath(strImagePath)
	{}
};


///////////////////////////////////////////////////////////////////////////////
//	class CWorkersInterface:
//		classe base di interfaccia per la gestione dei workers nel gestionale
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CWorkersTableObj : public CObject, public CTBLockable
{   
	friend class CLoginContext;
	DECLARE_DYNAMIC(CWorkersTableObj)

public:
	CWorkersTableObj();
	~CWorkersTableObj() { RemoveAll(); }

private:
	CObArray m_arWorkers;
	BOOL	m_bLoaded;

public:
	int AddWorker(long lWorkerID, CString strName, CString strLastName, CString strCompanyLogin, BOOL bDisabled, CString strImagePath = _T("")) { return m_arWorkers.Add(new CWorker(lWorkerID, strName, strLastName, strCompanyLogin, bDisabled==TRUE, strImagePath)); }
	int AddWorker(CWorker* pWorker) { return m_arWorkers.Add(pWorker); }
	
	int GetWorkersCount() { return m_arWorkers.GetUpperBound(); }
	CWorker* GetWorkerAt(int nIdx) { return (CWorker*)m_arWorkers.GetAt(nIdx); }

	CWorker* GetWorker(long lWorkerID);
	void RemoveAll();

	void Load();

public:
	virtual LPCSTR  GetObjectName() const { return "CWorkersTableObj"; }
	virtual void LoadWorkers() = 0;
};



///////////////////////////////////////////////////////////////////////////////
//	class CLoginInfos
///////////////////////////////////////////////////////////////////////////////
//
struct TB_EXPORT CLoginInfos
{
public:
	CString	m_strUserName;
	CString	m_strUserDescription;
	int		m_nLoginId;
	CString	m_strCompanyName;
	int		m_nCompanyId;
	BOOL	m_bAdmin; 
	BOOL	m_bEasyBuilderDeveloper;
	CString	m_strDBName;
	CString	m_strDBServer;
	int		m_nProviderId;
	BOOL	m_bSecurity; 
	BOOL	m_bAuditing;
	BOOL	m_bRowSecurity;
	BOOL	m_bTransactionUse;
	BOOL	m_bUseUnicode;
	BOOL	m_bDataSynchro;
	CString	m_strPreferredLanguage; 
	CString m_strApplicationLanguage;
	CString	m_strProviderName; 
	CString	m_strProviderDescription; 
	BOOL	m_bUseConstParameter; 
	BOOL	m_bStripTrailingSpaces;
	CString	m_strProviderCompanyConnectionString;
	CString	m_strNonProviderCompanyConnectionString;
	CString	m_strDBUser;
	CString	m_strProcessName;
	CString	m_strCompanyLanguage;
	CString m_strCompanyApplicationLanguage;
	LCID	m_wDataBaseCultureLCID;
	CString	m_strDatabaseType; 
	BOOL	m_bCanAccessWebSitePrivateArea; 
	CString	m_strUserEmail;

	CStringArray m_CompanyUsers;
	CStringArray m_CompanyRoles;		
	CStringArray m_UserRoles;		//ruoli dell'utente corrente

	CLoginInfos() :
		m_nLoginId (-1),
		m_nCompanyId(-1),
		m_bAdmin(FALSE),
		m_bEasyBuilderDeveloper(TRUE),
		m_nProviderId(-1),
		m_bSecurity(FALSE),
		m_bAuditing(FALSE),
		m_bRowSecurity(FALSE),
		m_bTransactionUse(FALSE),
		m_bUseUnicode(FALSE),
		m_wDataBaseCultureLCID(0),
		m_bUseConstParameter(FALSE), 
		m_bStripTrailingSpaces(FALSE),
		m_bCanAccessWebSitePrivateArea(FALSE) {}
};

class TB_EXPORT CDocumentSessionObj
{
public:
	virtual ~CDocumentSessionObj() {}
	virtual void OnAddThreadWindow(HWND hwnd) = 0;
	virtual void OnRemoveThreadWindow(HWND hwnd) = 0;
	virtual void SuspendPushToClient() = 0;
	virtual void ResumePushToClient() = 0;
	
};
class SqlRecord;
typedef const SqlRecord*(*DataSourceGetterFunction)();
typedef CMap<CString, LPCTSTR, DataSourceGetterFunction, DataSourceGetterFunction> DataSourceGetterFunctionMap;
typedef CMap<DWORD, DWORD, CTBWinThread*, CTBWinThread*> MapDWordToTBThread;

#define REGISTER_DATASOURCE_GETTER(datasource_name, datasource_getter)\
 AfxGetLoginContext()->RegisterDataSourceGetter(_T(#datasource_name), (DataSourceGetterFunction)datasource_getter);


//*****************************************************************************
class TB_EXPORT CLoginContext : public CTBWinThread, public CGenericContext<CLoginContext>, public CTBLockable
{
	friend class CLoginManagerInterface;
	friend class CXmlNumberToLiteralParser;
	friend class CTbCommandManager;
	friend class CBaseSecurityInterface;
	friend class CLoginThread;

protected:
	CString							m_strName;
	bool							m_bValid; 

	DECLARE_LOCKABLE(MapDWordToTBThread,	m_DocumentThreads);
	DECLARE_LOCKABLE(DataSourceGetterFunctionMap, m_DataSourceGetters);//associa il nome di un datasource al puntatore a funzione che lo restituisce (usato per accedere alle info via json)

	CObject*						m_pCultureInfo;
	CObject*						m_pWebServiceStateObjects;
	CObject*						m_pSecurityInterface;
	CObject*						m_pFormatsTable;	
	CObject*						m_pFontsTable;		
	CObject*						m_pSettingsTable;	
	CObject*						m_pDocumentObjectsTable;
	CObject*						m_pOleDbMng;
	CObject*						m_pNTLLookUpTableManager;
	CObject*						m_pDataCachingUpdatesListener;
	CObject*						m_pDataCachingSettings;
	CObject*						m_pSqlRecoveryManager;
	CObject*						m_pLockManager;
	CObject*						m_pWorkersTable;
	CObject*						m_pThemeManager;

	CObject*						m_pDMSRepositoryManager;

	UWORD							m_nOperationsDay;
	UWORD							m_nOperationsMonth;
	UWORD							m_nOperationsYear;
	//@@ElapsedTime
	int								m_nElapsedTimePrecision;
	int								m_nOpenDialogs;

	CLoginInfos						m_LoginInfos;

	HWND							m_nMenuWindowHandle;
	BOOL							m_bIsDocked;

	HANDLE							m_hTbDevMode;
	HANDLE							m_hTbDevNames;
	long							m_nWorkerId;

	CString							m_strDBCompanyName;
	DECLARE_LOCKABLE(CStringArray,	m_arDocumentLinks);
	int								m_nHistorySize;

	CArray<HWND>					m_arAllHwndArray;  //la prima finestra di ogni documentThread finisce in questo array

	BOOL							m_bAllowSetOpDateWithOpenDocs;

private:
	double	m_arDataDblEpsilonPrecisions[DATADBL_EPSILON_PRECISION_MAX];
	CThreadInfoArray				m_arThreadInfo;
	int m_nMluValidCounter;

	CStringArray			m_arCompanyTagInfo;
	CStringArray			m_arCompanyInfo;
public:
	BOOL			m_bMluValid;

public:
	CLoginContext();
	virtual ~CLoginContext(void);

	virtual LPCSTR  GetObjectName() const { return "CLoginContext"; }
	void Lock				();
	void UnLock				();
	BOOL IsLocked			();
	BOOL IsValid() { return (this != NULL) && m_bValid; }
	BOOL IsValidToken(const CString &strToken);
	void SetValid			(bool bValid = true) { m_bValid = bValid; }

	const CStringArray& GetDocumentLinks() { return m_arDocumentLinks; }
	const CString& GetName() const { return m_strName; }

	void FreeObjects();

	virtual void Close() = 0;

	UWORD							GetOperationsDay()	{ TB_LOCK_FOR_READ(); return m_nOperationsDay; }
	UWORD							GetOperationsMonth(){ TB_LOCK_FOR_READ(); return m_nOperationsMonth; }
	UWORD							GetOperationsYear()	{ TB_LOCK_FOR_READ(); return m_nOperationsYear; }
	
	virtual void AddDiagnostic(CDiagnostic *pDiagnostic) = 0;
	virtual int GetOpenDocuments() = 0;
	virtual int GetOpenDocumentsInDesignMode() = 0;
	int GetOpenDialogs()			{ TB_LOCK_FOR_READ(); return m_nOpenDialogs; }
	void IncreaseOpenDialogs();	
	void DecreaseOpenDialogs();

	void AddDocumentThread(CTBWinThread* pThread);	
	void RemoveDocumentThread(CTBWinThread* pThread);

	void AddToAllHwndArray(HWND hWnd);
	void RemoveFromAllHwndArray(HWND hWnd);
	CArray<HWND>& GetAllHwndArray() { return m_arAllHwndArray; }

	void GetDocumentThreads(CArray<long, long>& arThreadIds);	
	HWND GetMenuWindowHandle()						{ TB_LOCK_FOR_READ(); return m_nMenuWindowHandle; }
	virtual void SetMenuWindowHandle(HWND handle)	{ TB_LOCK_FOR_WRITE(); m_nMenuWindowHandle = handle; }

	BOOL GetDocked()								{ TB_LOCK_FOR_READ(); return m_bIsDocked; }
	void SetDocked(BOOL bIsDocked)					{ TB_LOCK_FOR_WRITE(); m_bIsDocked = bIsDocked; }

	long GetWorkerId()								{ TB_LOCK_FOR_READ(); return m_nWorkerId; }
	void SetWorkerId(long nWorkerId)				{ TB_LOCK_FOR_WRITE(); m_nWorkerId = nWorkerId; }

	
	CObject*					    GetCultureInfo()				{ return m_pCultureInfo; }
	CObject*						GetWebServiceStateObjects()		{ return m_pWebServiceStateObjects; }
	CObject*						GetSecurityInterface()			{ return m_pSecurityInterface; }
	CObject*						GetOleDbMng()					{ return m_pOleDbMng; }
	
	CObject*						GetFormatsTable()				{ return m_pFormatsTable; }
	CObject*						GetFontsTable()					{ return m_pFontsTable; }
	CObject*						GetDocumentObjectsTable()		{ return m_pDocumentObjectsTable; }
	CObject*						GetSettingsTable()				{ return m_pSettingsTable; }
	CObject*						GetNTLLookUpTableManager()		{ return m_pNTLLookUpTableManager; }
	CObject*						GetDataCachingUpdatesListener()	{ return m_pDataCachingUpdatesListener; }
	CObject*						GetDataCachingSettings()		{ return m_pDataCachingSettings; }
	CObject*						GetSqlRecoveryManager()			{ return m_pSqlRecoveryManager; }
	CObject*						GetLockManager()				{ return m_pLockManager; }
	CObject*						GetWorkersTable();				//{ return m_pWorkersTable;}
	CObject*						GetThemeManager()				{ return m_pThemeManager; }
	void							AttachWorkersTable(CObject* pObj) { delete m_pWorkersTable; m_pWorkersTable = pObj; }


	CObject*						GetDMSRepositoryManager()					{ return m_pDMSRepositoryManager; }
	void							AttachDMSRepositoryManager(CObject* pObj)	{ delete m_pDMSRepositoryManager; m_pDMSRepositoryManager = pObj; }

	int								GetElapsedTimePrecision()		{ TB_LOCK_FOR_READ(); return m_nElapsedTimePrecision; }			
	DWORD							GetThreadId()					{ return m_nThreadID; }			
	const CLoginInfos*				GetLoginInfos()	const			{ return &m_LoginInfos; } 
	CString							GetThreadInfos();
	CString							GetThreadInfosJSON();

public:
	void SetAllowSetOpDateWithOpenDocs(BOOL bSet)	{ m_bAllowSetOpDateWithOpenDocs = bSet; }
	BOOL GetAllowSetOpDateWithOpenDocs()			{ return m_bAllowSetOpDateWithOpenDocs; }
	void SetOperationsDate(UWORD nDay, UWORD nMonth, UWORD nYear);
	void SetElapsedTimePrecision(int nPrec);

	void AttachDocumentObjectsTable(CObject* pObj)				{ delete m_pDocumentObjectsTable; m_pDocumentObjectsTable = pObj; }
	void RegisterDataSourceGetter(const CString& sDataSourceName, DataSourceGetterFunction pGetter);
	DataSourceGetterFunction GetDataSourceGetter(const CString& sDataSourceName);
protected:
	void AttachCultureInfo(CObject* pObj)						{ delete m_pCultureInfo; m_pCultureInfo = pObj;}
	void AttachWebServiceStateObjects(CObject* pObj)			{ delete m_pWebServiceStateObjects; m_pWebServiceStateObjects = pObj;}
	void AttachSecurityInterface(CObject* pObj)					{ delete m_pSecurityInterface; m_pSecurityInterface = pObj;}
	void AttachOleDbMng(CObject* pObj)							{ delete m_pOleDbMng; m_pOleDbMng = pObj;}
	//per il vecchio LockManager @@BAUZI da togliere
	void AttachLockManager(CObject* pObj)						{ delete m_pLockManager; m_pLockManager = pObj;}
	void AttachFormatsTable(CObject* pObj)						{ delete m_pFormatsTable; m_pFormatsTable = pObj; }
	void AttachFontsTable(CObject* pObj)						{ delete m_pFontsTable; m_pFontsTable = pObj; }
	void AttachSettingsTable(CObject* pObj)						{ delete m_pSettingsTable; m_pSettingsTable = pObj;}
	void AttachThemeManager(CObject* pObj)						{ delete m_pThemeManager; m_pThemeManager = pObj;}
	
	void AttachNTLLookUpTableManager(CObject* pObj)				{ delete m_pNTLLookUpTableManager; m_pNTLLookUpTableManager = pObj;}
	void AttachDataCachingUpdatesListener(CObject* pObj)		{ delete m_pDataCachingUpdatesListener; m_pDataCachingUpdatesListener = pObj;}
	void AttachDataCachingSettings(CObject* pObj)				{ delete m_pDataCachingSettings; m_pDataCachingSettings = pObj; }
	void AttachSqlRecoveryManager(CObject* pObj)				{ delete m_pSqlRecoveryManager; m_pSqlRecoveryManager = pObj; }
	void InitDocumentHistory(int nSize);
	void SaveDocumentHistory();

public:
	virtual BOOL CanClose() = 0;
	virtual BOOL CloseAllDocuments() = 0;
	virtual BOOL SilentCloseLoginDocuments() = 0;
	virtual void UpdateMenuTitle() = 0;
	virtual void DestroyAllDocuments() = 0;	
	virtual void AddFormatter (Formatter*) = 0;
	virtual BOOL CanUseNamespace(const CString&	nameSpace, int grantType) = 0;

	// precision on double management (I cannot use DataType structure)
	const double	GetDataDblEpsilon	() const;
	const double	GetDataMonEpsilon	() const;
	const double	GetDataQtyEpsilon	() const;
	const double	GetDataPercEpsilon	() const;

	virtual void	GetTbDevParams			(HANDLE& hDevMode, HANDLE& hDevNames) = 0;
	virtual void	SetTbDevParams			(HANDLE hDevMode, HANDLE hDevNames) = 0;

	virtual CXMLVariable* GetVariable(const CString& sName) = 0;
	virtual void DeclareVariable(const CString& sName, DataObj* pDataObj, BOOL bOwnsDataObj, BOOL bReplaceExisting = FALSE) = 0;
	virtual void CopyVariables(CAbstractFormDoc* pDoc) = 0;
			void	FreePrinterDevParams	();

	CString GetDBCompanyName	()									{ TB_LOCK_FOR_READ(); return m_strDBCompanyName; }
	void	SetDBCompanyName	(const CString&  strDBCompanyName)	{ TB_LOCK_FOR_WRITE(); m_strDBCompanyName = strDBCompanyName; }

	CString	GetCompanyInfo		(const CString& sTagInfo) const;
	void	AddCompanyInfo		(const CString& sTagInfo, const CString& sInfo);
	//void	UpdateCompanyInfo	(const CString& sTagInfo, const CString& sInfo);
	void	ResetCompanyInfo	();
	int		GetCompanyInfoCount	() const;
	CString	GetCompanyTagInfo	(int idx) const;
	CString	GetCompanyInfo		(int idx) const;

private:
	// used by login thread to calculate epsilon. Objects are not locked!
	void SetEpsilonPrecision (const int& nTypePos, const int& nrOfDecimals);

private:
	virtual CObject* InternalGetObject(GetMethod getMethod) { return ((this)->*getMethod)(); }
	void CreateDirectoryRecursive(const CString& sFolder);
};

inline TB_EXPORT CLoginContext* AfxGetLoginContext();
TB_EXPORT CObject* AfxGetLoginContextObject(CRuntimeClass* pClass, BOOL bCreate = TRUE);
TB_EXPORT CLoginContext* AfxGetLoginContext(const CString& strName);
TB_EXPORT const CLoginInfos* AfxGetLoginInfos();
TB_EXPORT BOOL AFXAPI AfxIsRemoteInterface();

TB_EXPORT HWND AfxGetMenuWindowHandle();
TB_EXPORT CWnd* AfxGetMenuWindow();
// primary status bar set text and window title functions
TB_EXPORT void	AfxSetStatusBarText	(const CString& strText);
TB_EXPORT void 	AfxSetStatusBarText	(UINT nID);
TB_EXPORT void	AfxSetUserPaneText		(const CString& strText, const CString& stsTooltip);
TB_EXPORT void 	AfxClearStatusBar		();
TB_EXPORT void	AfxSetMenuWindowTitle	(const CString& strText);

#include "endh.dex"
