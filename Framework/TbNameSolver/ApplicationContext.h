#pragma once

#include "GenericContext.h"
#include "TbResourcesMap.h"
#include "ThreadInfo.h"

#include "beginh.dex"

// CApplicationContext
class IFileSystemManager;
class CLoginContext;
class CLockTracer;
class CDiagnostic;
class CPathFinder;
class CCompanyContext;
class CThreadInfoArray;
class Formatter;

struct structColorDocument 
{
	CString strKey;
	COLORREF color;
	int nRefCounter;
};
BOOL TB_EXPORT TBHookedApis();

#ifdef DEBUG
TB_EXPORT extern BOOL g_bNO_ASSERT;
BOOL TB_EXPORT AFXAPI AfxTbAssertFailedLine(LPCSTR lpszFileName, int nLine);
#endif

typedef CMap<CObject*, CObject*, HWND, HWND>						MapCObjectToHWND;
typedef CMap<DWORD, DWORD, HWND, HWND>								MapDWORDToHWND;
typedef CMap <UINT, UINT, CRuntimeClass*, CRuntimeClass* >			MapUINTToRTC;
typedef CMap<CString,LPCTSTR, CCompanyContext*, CCompanyContext*>	MapCStringToCCompanyContext;
typedef CArray<structColorDocument, structColorDocument>			ColorDocumentArray;

class CAuditingMngObj;
class CRowSecurityMngObj;

//per creare l'AuditManager associato alla tabella sotto procedura
typedef  bool (TPCallSetTracedFunction) (CString, CAuditingMngObj**);
extern TB_EXPORT TPCallSetTracedFunction* s_pfCallTraced;

//per creare il RowSecurityManager associato alla tabella
typedef  bool (TPCallSetProtectedFunction) (CString, CRowSecurityMngObj**);
extern TB_EXPORT TPCallSetProtectedFunction* s_pfCallProtected;



//====================================================================
class TB_EXPORT IHostApplication
{
public:
	virtual BOOL LoadSoapNeededLibraries(LPCTSTR lpszModuleNamespace, CString& strError) = 0;
	virtual BOOL CanExecuteSoapMethod(CLoginContext* pContext, LPCTSTR lpszActionNamespace, CString& strError) = 0;
	virtual BOOL EnableSoapExecutionControl(BOOL bSet) = 0;
	virtual ~IHostApplication(){};
};

//*****************************************************************************
class TB_EXPORT CBaseCodeLocker : public CObject, public CTBLockable
{
	CStringA m_sName;
public:
	CBaseCodeLocker() {}
	CBaseCodeLocker(const CString& sName) { m_sName = sName; }

	virtual LPCSTR  GetObjectName() const { return m_sName; }
};

//*****************************************************************************
class TB_EXPORT CApplicationContext : public CObject, public CGenericContext<CApplicationContext>, public CTBLockable
{
		friend class CThreadContext;
		friend class CLoginContext;
		friend class CLoginThread;
		friend class CApplicationsLoader;
		friend class CXMLAddOnDatabaseObjectsParser;
		friend class CXMLClientDocumentObjectscContent;
		friend class CTBSoapServer;
		friend class CTaskBuilderApp;
		friend class CMailConnector;
		friend class CIMagoMailConnector;
		friend class CTBApplicationProxy;

public:
	enum MacroRecorderStatus{IDLE, PLAYING, RECORDING};

	MacroRecorderStatus			m_MacroRecorderStatus;

private:
	HWND						m_hwndAppMainWnd;
	CString						m_strAppTitle;
	BOOL						m_bUnattendedMode;
	BOOL						m_bIISModule;
	BOOL						m_bEnableReportEditor;
	BOOL						m_bMultiThreadedDocument;
	BOOL						m_bMultiThreadedLogin;
	BOOL						m_bRemoteInterface;

	CObject*					m_pStringLoader;
	CObject*					m_pEnumsTable;		
	CLockTracer*				m_pLockTracer;
	CObject*					m_pMailConnector;
	CObject*					m_pAddOnFieldsTable;
	CObject*					m_pDeclaredDBReleasesTable;
	CObject*					m_pDataFilesManager;
	CObject*					m_pClientDocsTable;
	CObject*					m_pClientFormsTable;
	CObject*					m_pCommandManager;
	CObject*					m_pSoapServer;
	CObject*					m_pFileSystemManagerWS;
	CObject*					m_pAddOnApps;
	CObject*					m_pGlobalSettingsTable;	
	CObject*					m_pStandardFormatTable;	
	CObject*					m_pStandardFontsTable;	
	CObject*					m_pStandardDocumentsTable;	
	CObject*					m_pRadarFactory;	
	CObject*					m_pExplorerFactory;	
	CObject*					m_pHotLinkFactory;	
	CObject*					m_pParsedControlsRegistry;	
	CObject*					m_pDatabaseObjectsTable;	
	CPathFinder*				m_pPathFinder;
	CObject*					m_pClientObjects;
	CObject*					m_pBehavioursRegistry;	
	IFileSystemManager* 		m_pFileSystemManager;
	HWND						m_nMenuWindowHandle;
	CThreadInfoArray			m_arThreadInfo;	
	CTBResourcesMap				m_TbResourcesMap;//la mappa delle risorse deve essere globale, e non per thread, perché la message map dei documenti è statica
	CRuntimeClass*				m_pCustomSaveDialogClass = NULL;
	BOOL						m_bEnumsViewerThreadOpened;

	DECLARE_LOCKABLE(CObArray,						m_arLoginContexts);
	DECLARE_LOCKABLE(MapCStringToCCompanyContext,	m_CompanyContexts);
	DECLARE_LOCKABLE(MapCObjectToHWND,				m_WebServiceStateObjects);
	DECLARE_LOCKABLE(MapDWORDToHWND,				m_ThreadMap);
	DECLARE_LOCKABLE(MapUINTToRTC,					m_RegisteredControls);
	DECLARE_DYNCREATE(CApplicationContext)
	CBaseCodeLocker			m_FrameListCodeLocker;

public:
	CApplicationContext();           
	virtual ~CApplicationContext();

private:
	void FreeObjects();

	void				SetAppTitle(const CString& strTitle);
	void				SetCanUseReportEditor(BOOL bSet)		{ m_bEnableReportEditor = bSet; } 
public:
	void				SetInUnattendedMode(BOOL bSet)			{ m_bUnattendedMode = bSet; } 
	
	virtual	LPCSTR		GetObjectName() const					{ return "CApplicationContext"; }
	HWND				GetAppMainWnd()							{ return m_hwndAppMainWnd; }
	void				SetAppMainWnd(HWND hwnd)				{ m_hwndAppMainWnd = hwnd; }
	const CString&		GetAppTitle();
	BOOL				CanUseReportEditor()					{ return m_bEnableReportEditor; } 
	BOOL				IsInUnattendedMode()					{ return m_bUnattendedMode; } 
	void				SetIISModule(BOOL bSet)					{ m_bIISModule = bSet; } 
	BOOL				IsIISModule()							{ return m_bIISModule; } 

	CObject*			GetStandardFormatsTable()				{ return m_pStandardFormatTable; }
	void				AddFormatterToLoginContext				(Formatter*);
	CObject*			GetStandardFontsTable()					{ return m_pStandardFontsTable; }
	CObject*			GetStandardDocumentsTable()				{ return m_pStandardDocumentsTable; }

	CObject*			GetStringLoader()						{ return m_pStringLoader; }
	CObject*			GetEnumsTable()							{ return m_pEnumsTable; }
	CObject*			GetDataFilesManager()					{ return m_pDataFilesManager; }

	CObject*			GetMailConnector()						{ return m_pMailConnector; }
	CObject*			GetAddOnFieldsTable()					{ return m_pAddOnFieldsTable; }
	CObject*			GetDeclaredDBReleasesTable()			{ return m_pDeclaredDBReleasesTable; }
	CObject*			GetClientDocsTable()					{ return m_pClientDocsTable; }
	CObject*			GetClientFormsTable()					{ return m_pClientFormsTable; }

	CObject*			GetCommandManager()						{ return m_pCommandManager; }
	CObject*			GetGlobalSettingsTable()				{ return m_pGlobalSettingsTable; }

	CObject*			GetSoapServer()							{ return m_pSoapServer; }
	CObject*			GetFileSystemManagerWS()				{ return m_pFileSystemManagerWS; }
	CObject*			GetAddOnApps()							{ return m_pAddOnApps; }

	CObject*			GetRadarFactory()						{ return m_pRadarFactory; }
	CObject*			GetExplorerFactory()					{ return m_pExplorerFactory; }
	CObject*			GetHotLinkFactory()						{ return m_pHotLinkFactory; }
	CObject*			GetDatabaseObjectsTable()				{ return m_pDatabaseObjectsTable; }

	CLockTracer*		GetLockTracer()							{ return m_pLockTracer; }

	CTBResourcesMap*	GetTbResourcesMap()						{ return &m_TbResourcesMap; }

	CPathFinder*		GetPathFinder()							{ return m_pPathFinder; }
	CObject*			GetClientObjects()						{ return m_pClientObjects; }
	IFileSystemManager* GetFileSystemManager()					{ return m_pFileSystemManager; }
	CLoginContext*		GetLoginContext(const CString& strName);
	CLoginContext*		GetLoginContext(DWORD id);
	void				GetLoginContextIds(CDWordArray& arIds);
	CCompanyContext*	GetCompanyContext(const CString& strKey);
	void				CloseLoginThread(DWORD nThreadId, HANDLE hThreadHandle);

	void AddLoginContext(CLoginContext* pObj);
	void RemoveLoginContext(CLoginContext* pObj);
	UINT GetLoginContextNumber();
	BOOL IsMultiThreadedDocument() { return m_bMultiThreadedDocument; }
	BOOL IsMultiThreadedLogin() { return m_bMultiThreadedLogin; }
	void SetMultiThreadedDocument(BOOL bSet = TRUE) { m_bMultiThreadedDocument = bSet; }
	void SetMultiThreadedLogin(BOOL bSet = TRUE) { m_bMultiThreadedLogin = bSet; }
	void AddWebServiceStateObject(CObject* pObject, HWND hwndThreadWindow);
	BOOL ExistWebServiceStateObject(CObject* pObject);
	void RemoveWebServiceStateObject(CObject* pObject);
	void RemoveWebServiceStateObjects(HWND hwndThreadWindow);
	HWND GetWebServiceStateObjectWnd(CObject* pObject);

	void AddThread(DWORD dwThreadId, HWND hwndMainWindow);
	void RemoveThread(DWORD dwThreadId);
	HWND GetThreadMainWindow(DWORD dwThreadId);

	CRuntimeClass* GetControlClass(UINT id);
	void RegisterControl(UINT nIDD, CRuntimeClass* pClass);

	void CloseAllLogins(BOOL bOnlyInvalid = FALSE);
	BOOL CanClose();
	CString		GetThreadInfos();

	HWND GetMenuWindowHandle()						{ TB_LOCK_FOR_READ(); return m_nMenuWindowHandle; }
	virtual void SetMenuWindowHandle(HWND handle)	{ TB_LOCK_FOR_WRITE(); m_nMenuWindowHandle = handle; }
	
	CObject*	GetParsedControlsRegistry	() { return m_pParsedControlsRegistry; }
	void AttachDatabaseObjectsTable(CObject* pObj)			{ delete m_pDatabaseObjectsTable; m_pDatabaseObjectsTable = pObj; }

	CObject*	GetBehavioursRegistry	() { return m_pBehavioursRegistry; }

	UINT GetAllOpenDocumentNumber();
	UINT GetAllOpenDocumentNumberEditMode();

	void			SetCustomSaveDialogClass(CRuntimeClass* pClass)			{ m_pCustomSaveDialogClass = pClass; }
	CRuntimeClass*	GetCustomSaveDialogClass()								{ return m_pCustomSaveDialogClass; }

	BOOL	IsEnumsViewerThreadOpened()	const;
	void	SetEnumsViewerThreadOpened(BOOL bValue);

	void	SetRemoteInterface(BOOL bSet) { m_bRemoteInterface = bSet; }
	BOOL	IsRemoteInterface() { return m_bRemoteInterface; }

private:
	void AttachRadarFactory(CObject* pObj)					{ delete m_pRadarFactory; m_pRadarFactory = pObj; }
	void AttachExplorerFactory(CObject* pObj)				{ delete m_pExplorerFactory; m_pExplorerFactory = pObj; }
	void AttachHotLinkFactory(CObject* pObj)				{ delete m_pHotLinkFactory; m_pHotLinkFactory = pObj; }
	void AttachMailConnector(CObject* pObj);
	void AttachAddOnFieldsTable(CObject* pObj)				{ delete m_pAddOnFieldsTable; m_pAddOnFieldsTable = pObj; }
	void AttachStringLoader(CObject* pObj)					{ delete m_pStringLoader; m_pStringLoader = pObj; }
	void AttachClientObjects(CObject* pObj)					{ delete m_pClientObjects; m_pClientObjects = pObj; }
	void AttachStandardFormatsTable(CObject* pObj);
	void AttachStandardFontsTable(CObject* pObj);
	void AttachStandardDocumentsTable(CObject* pObj);
	void AttachPathFinder(CPathFinder* pObj);
	void AttachFileSystemManager(IFileSystemManager* pObj);
	void AttachFileSystemManagerWS(CObject* pObj)			{ delete m_pFileSystemManagerWS; m_pFileSystemManagerWS = pObj;}
	void AttachEnumsTable(CObject* pObj)					{ delete m_pEnumsTable; m_pEnumsTable = pObj;}
	void AttachDataFilesManager(CObject* pObj)				{ delete m_pDataFilesManager; m_pDataFilesManager = pObj;}
	void AttachClientDocsTable(CObject* pObj)				{ delete m_pClientDocsTable; m_pClientDocsTable = pObj;}
	void AttachClientFormsTable(CObject* pObj)				{ delete m_pClientFormsTable; m_pClientFormsTable = pObj;}
	void AttachAddOnApps(CObject* pObj)						{ delete m_pAddOnApps; m_pAddOnApps = pObj;}
	void AttachCommandManager(CObject* pObj)				{ delete m_pCommandManager; m_pCommandManager = pObj;}
	void AttachSoapServer(CObject* pObj)					{ delete m_pSoapServer; m_pSoapServer = pObj;}
	void AttachParsedControlsRegistry(CObject* pObj)		{ delete m_pParsedControlsRegistry; m_pParsedControlsRegistry = pObj;}
	void AttachGlobalSettingsTable(CObject* pObj)			{ delete m_pGlobalSettingsTable; m_pGlobalSettingsTable = pObj;}
	void AttachBehavioursRegistry (CObject* pObj)			{ delete m_pBehavioursRegistry; m_pBehavioursRegistry = pObj; }

	void StartLockTracer(UINT nTBPort);
	void StopLockTracer();


	void		LoadBkgColor(COLORREF color);

public:
	void AttachDatabaseReleaesesTable(CObject* pObj)		{ delete m_pDeclaredDBReleasesTable; m_pDeclaredDBReleasesTable = pObj; }

private:
	virtual CObject* InternalGetObject(GetMethod getMethod) { return ((this)->*getMethod)(); }
	virtual BOOL IsValid() { return this != NULL; }

private:
	CStringArray	m_FailedAssertions;
	BOOL m_bEnableAssertionsInRelease;
	BOOL m_bDumpAssertionsIfNoCrash;
	BOOL m_bEnableActiveAccessibility; // useful for Ranorex Spy
	BOOL _AssertAlwaysFailedLine(LPCSTR lpszFailedTest, DWORD threadId, LPCSTR lpszFunction, LPCSTR lpszFileName, int nLine, const TCHAR * szAdditionalMessage);

protected:
	BOOL SetEnableAssertionsInRelease(BOOL bEnable) { BOOL bOld = m_bEnableAssertionsInRelease; m_bEnableAssertionsInRelease = bEnable; return bOld;}
	BOOL SetDumpAssertionsIfNoCrash(BOOL bEnable) { BOOL bOld = m_bDumpAssertionsIfNoCrash; m_bDumpAssertionsIfNoCrash = bEnable; return bOld;}

public:
	BOOL AssertAlwaysFailedLine(LPCSTR lpszFailedTest, DWORD threadId, LPCSTR lpszFunction, LPCSTR lpszFileName, int nLine);
	BOOL AssertAlwaysFailedLine(LPCSTR lpszFailedTest, DWORD threadId, LPCSTR lpszFunction, LPCSTR lpszFileName, int nLine, const TCHAR * szAdditionalMessage, ...);
	BOOL AssertAlwaysFailedLine(LPCSTR lpszFailedTest, DWORD threadId, LPCSTR lpszFunction, LPCSTR lpszFileName, int nLine, const CHAR * szAdditionalMessage, ...);
	BOOL AreReleaseAssertionsEnabled() { return m_bEnableAssertionsInRelease; }
	BOOL DumpAssertionsIfNoCrash() { return m_bDumpAssertionsIfNoCrash; }

	int GetFailedAssertionsCount() { return m_FailedAssertions.GetCount(); }
	CString GetFailedAssertion(int i);
	void ClearFailedAssertions() { m_FailedAssertions.RemoveAll(); }

	void SetEnableActiveAccessibility(BOOL bEnable) { m_bEnableActiveAccessibility = bEnable; }
	BOOL IsActiveAccessibilityEnabled() { return m_bEnableActiveAccessibility; }

};


//-----------------------------------------------------------------------------
TB_EXPORT CApplicationContext*				AFXAPI AfxGetApplicationContext		();
TB_EXPORT inline BOOL						AFXAPI AfxMultiThreadedDocument		() { return AfxGetApplicationContext()->IsMultiThreadedDocument(); }
TB_EXPORT inline BOOL						AFXAPI AfxMultiThreadedLogin		() { return AfxGetApplicationContext()->IsMultiThreadedLogin(); }


#include "endh.dex"
