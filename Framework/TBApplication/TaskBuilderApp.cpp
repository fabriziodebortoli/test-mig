#include "stdafx.h"

#include <initguid.h>

#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\PathFinder.h>
#include <TbNameSolver\Templates.h>
#include <tbnamesolver\TBResourcesMap.h>

#include <tbgenlib\CEFClasses.h>
#include <TbClientCore\ClientObjects.h>

#include <TbWebServicesWrappers\LoginManagerInterface.h>
#include <TbWebServicesWrappers\TbServicesWrapper.h>
#include <TbWebServicesWrappers\LockManagerInterface.h>

#include <TbGeneric\ProcessWrapper.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\WebServiceStateObjects.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\StatusBarMessages.h>
#include <TbGeneric\pictures.h>
#include <TbGeneric\EnumsTable.h>
#include <TbGeneric\DocumentObjectsInfo.h>

#include <TbParser\XmlSettingsParser.h>
#include <TbParser\TokensTable.h>
#include <TbParser\XmlDataFileParser.h>

#include <TbGenlib\expr.h>
#include <TbGenlib\oslbaseinterface.h>
#include <TbGenlib\TBStrings.h>
#include <TbGenlib\diagnosticmanager.h>
#include <TbGenlibManaged\HelpManager.h>
#include <TbGenlibUI\TBExplorer.h>

#include <TbOledb\sqltable.h>

#include <TbWoormEngine\ASKDLG.hjson> //JSON AUTOMATIC UPDATE
#include <TbWoormViewer\Woormdoc.h>

#include <TbGes\extdoc.h>
#include <TbGes\hotlink.h>
#include <TbGes\TBRadarInterface.h>
#include <TbGes\FormMngDlg.h>
#include <TbGes\XMLReferenceObjectsParserEx.h>

#include <TbGenlib\BaseFrm.h>
#include <TbGenlib\Generic.hjson> //JSON AUTOMATIC UPDATE
#include <TbGenlibManaged\main.h>

#include <TbRadar\TBRadarFactoryUI.h>

#include "TaskBuilderApp.h"
#include "LibrariesLoader.h"
#include "TbCommandManager.h"
#include "FileSystemManager.h"
#include "FileSystemDriver.h"
#include "DocumentThread.h"
#include "ApplicationsLoader.h"
#include "ThreadMainWindow.h"
#include "LoginThread.h"


#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

class CTaskBuilderApp;
class CTaskBuilderFrame;

//............................................................................................
static const TCHAR* szMsgFailed = _T(" *** Pre-Initialization Messages ***\r\n") 
							_T("Task Builder initialize process failed!\r\n") 
							_T("Login Manager or Lock Manager Web Services are not available.\r\n") 
							_T("The connection socket port supplied into Custom and\r\n") 
							_T("ServerConnection.Config file could be wrong or already busy.");

static const TCHAR* szTBLoaderPortParam			= _T("TBSOAPPort");
static const TCHAR* szTBLoaderTcpPortParam		= _T("TBTCPPort");
static const TCHAR* szUnattendedModeParam		= _T("UnattendedMode");
static const TCHAR* szLauncherParam				= _T("Launcher");
static const TCHAR* szRegisterWCFNamespaces		= _T("RegisterWCFNamespaces");
static const TCHAR* szTrue						= _T("true");
static const TCHAR* szMenuHandleParam			= _T("MenuHandle");

//=============================================================================================
class CStartLog : public CObject
{
public:
	CStartLog () { }
	~CStartLog() 
	{
		CString sUser = AfxGetLoginInfos() ? AfxGetLoginInfos()->m_strUserName : _T("");
		if (AfxGetDiagnostic()->MessageFound())
			CDiagnosticManager::LogToXml
				(
					AfxGetDiagnostic(), 
					AfxGetPathFinder()->GetStartupLogFullName(sUser, TRUE)
				); 
	}
};
//-----------------------------------------------------------------------------
//used to advise the caller than the initinstance method has completed
class CAutoSignalStartupEvent
{
	HANDLE m_hEvent;
public:
	CAutoSignalStartupEvent() : m_hEvent(NULL) {}
	~CAutoSignalStartupEvent() 
	{
		if (m_hEvent)
		{ 
			VERIFY(::SetEvent(m_hEvent));
			VERIFY(::CloseHandle(m_hEvent));
		}
	}
	void AssociateEvent(HANDLE hEvent) 
	{
		m_hEvent = hEvent; 
	}

};

/////////////////////////////////////////////////////////////////////////////
//							CTaskBuilderApp implementation
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTaskBuilderApp, CBaseApp)
	//{{AFX_MSG_MAP(CMyApp)
	ON_THREAD_MESSAGE		(UM_CLOSE_LOGIN,		OnCloseLogin)
	ON_THREAD_MESSAGE		(WM_TIMER,				OnThreadTimer)
//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CTaskBuilderApp::CTaskBuilderApp()
	:
	m_bControlSoapExecution (true)
{
#ifdef _DEBUG
	afxDump.SetDepth(1);
#endif
	
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.GetLogins")] = 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.GetProcessID")] = 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.IsLoginValid")]= 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.UseRemoteInterface")] = 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.SetMenuHandle")] = 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.GetAllMessages")] = 0;
	m_loginFunctionMap[_NS_WEB("Extensions.TbAuditing.TbAuditing.SetAuditingManager")] = 0;
	m_loginFunctionMap[_NS_WEB("Extensions.TbAuditing.TbAuditing.CloseAuditing")] = 0; 
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.Login")] = 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.ClearCache")] = 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.CanCloseTB")] = 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.CloseTB")] = 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.DestroyTB")]= 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.GetActiveThreads")] = 0;
	m_loginFunctionMap[_NS_WEB("Framework.TbGes.TbGes.EnableSoapFunctionExecutionControl")] = 0;

}
//--------------------------------------------------------------------------------
void CTaskBuilderApp::OnThreadTimer(WPARAM wParam, LPARAM lParam)
{
	//function called when multithreading is disabled
	//usually timers are used by login thread, 

	try
	{
		CLoginContext* pContext = AfxGetLoginContext();
		if (pContext && pContext->IsKindOf(RUNTIME_CLASS(CLoginThread)))
		{
			((CLoginThread*)pContext)->OnThreadTimer(wParam, lParam);
		}
	}
	catch (...)
	{
		TRACE("Failed to check authentication token");
		ASSERT(FALSE);

	}
}
//-----------------------------------------------------------------------------
void CTaskBuilderApp::OnCloseLogin(WPARAM wParam, LPARAM lParam)
{
	AfxGetApplicationContext()->CloseLoginThread((DWORD) wParam, (HANDLE)lParam);
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderApp::LoadSoapNeededLibraries(LPCTSTR lpcszLibraryNamespace, CString& strError)
{
	BOOL bResult = FALSE;

	bResult = AfxGetTbCmdManager()->LoadNeededLibraries(CTBNamespace(lpcszLibraryNamespace), NULL, lpcszLibraryNamespace == NULL ? LoadAll : LoadNeeded);

	if (!bResult)
		strError = AfxGetDiagnostic()->ToString();

	return bResult;
}


//-----------------------------------------------------------------------------
BOOL CTaskBuilderApp::CanExecuteSoapMethod(CLoginContext* pContext, LPCTSTR lpszActionNamespace, CString& strError)
{
	if (!pContext) return TRUE;

	if (!m_bControlSoapExecution)
		return TRUE; 

	// Application is not ready because:
	// - Login has to be performed
	if (pContext->GetName().IsEmpty())
	{
		LPCTSTR strDummy;
		if (!m_loginFunctionMap.LookupKey(lpszActionNamespace, strDummy))
		{
			strError = cwsprintf(_TB("Database connection is required in order to invoke SOAP method '{0-%s}'."), lpszActionNamespace);
			return FALSE;
		}
	}

	return TRUE;
}

//-----------------------------------------------------------------------------------------------------
BOOL CTaskBuilderApp::EnableSoapExecutionControl(BOOL bSet)
{ 
	BOOL bRetVal = m_bControlSoapExecution;
	m_bControlSoapExecution = bSet == TRUE;
	return bRetVal;
}

//-----------------------------------------------------------------------------
BOOL CTaskBuilderApp::HasArgument(const CString& strArgumentName)
{
	CString str = m_lpCmdLine;
	str.MakeLower();

	CString strArg = strArgumentName;
	strArg.MakeLower();

	return str.Find(strArg) != -1;
}

//-----------------------------------------------------------------------------
CString CTaskBuilderApp::GetArgumentValue(const CString& strArgumentName)
{
	CString str = m_lpCmdLine;
	str.MakeLower();

	CString strArg = strArgumentName;
	strArg.MakeLower();
	
	CString strValue;
	int index = 0, endOfParam = -1;
	
	while (	(index = str.Find(strArg, index)) != -1)
	{
		if ((index == 0) || (str[index - 1] == _T(' ')) )		//is the first work or il precedeed by a space
		{
			index = str.Find(_T("="), index);
			if (index != -1)
			{
				int i = index + 1;
				while (i < str.GetLength() && str[i] == _T(' '))
					i++;
				
				BOOL bApex = (i != str.GetLength()) && (str[i] == _T('"'));
				if (bApex)
					i++;

				endOfParam = str.Find((bApex ? _T('"') : _T(' ')), i);
				
				if (endOfParam == -1)
					endOfParam = str.GetLength();

				strValue = str.Mid(i, endOfParam - i);
			}
		}
		else
			index ++;
	}

	return strValue.Trim();
}


//-----------------------------------------------------------------------------
//funzione per la gestione dei parametri invalidi
void TBInvalidParameterHandler(const wchar_t* expression,
   const wchar_t* function, 
   const wchar_t* file, 
   unsigned int line, 
   uintptr_t pReserved)
{
	//non uso la traduzione per non generare un loop infinito in caso di traduzione errata
   CString sError;
#ifdef DEBUG
   sError.Format(_T("Invalid parameter detected in function %s. File: %s Line: %d\nExpression: %s"),
	   function, file, line, expression);
#else
   sError = _T("Invalid parameter detected");
#endif

   throw new CParameterException(sError);
}

//--------------------------------------------------------------------------------
int CTaskBuilderApp::Run()
{
	__try
	{
		return __super::Run();
	}
	__except (ExpFilter(GetExceptionInformation(), GetExceptionCode()))
	{
		AfxGetThreadContext()->SetInErrorState(); //to prevent another exception when destroying a document after an exception (search this boolean in code)
	}
	return -1;
}
// Application Start
//-----------------------------------------------------------------------------
BOOL CTaskBuilderApp::InitInstance()
{

	class CFreeWait
	{
		CString m_strSemaphore;
	public :
		CFreeWait(const CString& strSemaphore) : m_strSemaphore(strSemaphore)
		{
		}
		~CFreeWait()
		{
			if (!m_strSemaphore.IsEmpty())
				RemoveDirectory(m_strSemaphore);
		}
	} wait(GetArgumentValue(_T("Semaphore")));

	//imposto la funzione per la gestione degli errori sui parametri
	_set_invalid_parameter_handler(TBInvalidParameterHandler);
	
	SetThreadName (-1, "CTaskBuilderApp-MainThread");
	CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

	// Set the debug-heap flag (_CRTDBG_DELAY_FREE_MEM_DF) so that freed blocks are kept on the
	// linked list, to catch any inadvertent use of freed memory
	// Turning the _CRTDBG_CHECK_ALWAYS_DF bit field ON results in _CrtCheckMemory being called every time a 
	// memory allocation operation is requested (great slows down execution)
	
	// e' stata commentata perchè in operazioni con continue istanziazione e cancellazioni di oggetti (es. procedure
	// di chiusura magazzino, ricostruzione saldi...) la memoria saliva a dismisura arrivando ad un out of memory exception
	// da scommentare se si desidera fare un check memory sul programma
	//SET_CRT_DEBUG_FIELD( _CRTDBG_DELAY_FREE_MEM_DF | _CRTDBG_ALLOC_MEM_DF);

	CModuleDescription::s_pReferenceObjectsParser = new CXMLReferenceObjectsParser ();

	// to be done before init thread context, because thread initialization needs diagnostic manager
	AfxGetDiagnostic()->AttachViewer(new CMsgBoxViewer());

	InitThreadContext();

	CString strFileServer, strWebServer, strInstallation, strMasterSolutionName;
	GetServerInstallationInfo(strFileServer, strWebServer, strInstallation, strMasterSolutionName);
	
	// if the application context cannot be prepared, I avoid to go on
	if (!InitApplicationContext(strFileServer, strInstallation, strMasterSolutionName))	return FALSE;
	
	BOOL bUnattended = IsInUnattendedMode() || UnattendedStart();
	CDiagnosticLevelMng __diagnosticMng(bUnattended);
	
	//to enable dll loading from TBApps even if process directory is elsewhere (i.e. Menumanager one)
	AddEnvironmentVariable(_T("PATH"), AfxGetPathFinder()->GetTBDllPath());
	
	InitAssembly();
	InitServerObjects (strFileServer, strWebServer, strInstallation, strMasterSolutionName);

	if (!ApplicationLock())
		return FALSE;

	// it creates log file at the end of InitInstance
	CStartLog aLog;

	// if there are errors, I cannot go on. I avoid to try to load applications.
	if (AfxGetDiagnostic()->ErrorFound())	
		return FALSE;

	InitApplicationsAndUI ();

	//if I'm running under TBServices, no interface is needed
	//if settings are configured, load dll that intercepts user32 calls and avoid window creation
	//this code is here because only here settings are available
	/*CString sLauncher = GetArgumentValue(szLauncherParam);
	if ((sLauncher.CompareNoCase(_T("TbLoaderService")) == 0 || sLauncher.CompareNoCase(_T("TBIISMODULE")) == 0) && AfxAPIHookingEnabled())
	{
		ActivateApiHooking();
	}*/
	// dummy main window (MFC needs a thread main window)
	// moved here because must be done after api hooking (if any)
	m_pMainWnd = new CThreadMainWindow(TRUE); 
	AfxGetApplicationContext()->SetAppMainWnd(m_pMainWnd->m_hWnd);


	// I can trace only after settings load
	AfxGetDiagnostic()->EnableTraceInEventViewer(IsEventViewerTraceEnabled());

	// this is the last fatal error check. Extensions are intended optionals
	if (AfxGetDiagnostic()->ErrorFound())	
		return FALSE;

	if (!AfxGetBaseApp()->IsInUnattendedMode())
		InitExtensions ();
	
	CTBPicture::InitializeGdiPlus();
	
	CTBLockable::EnableLocking(TRUE);
	
	DataBool* aParam = (DataBool*) AfxGetSettingValue(CTBNamespace(szTbGenlibNamespace), szEnvironment, szSingleThread, DataBool(FALSE));
	BOOL bMultiThread = !aParam || !*aParam;
	AfxGetApplicationContext()->SetMultiThreadedDocument(bMultiThread);
	AfxGetApplicationContext()->SetMultiThreadedLogin(bMultiThread);

	if (bMultiThread)
		SetThreadPriority(THREAD_PRIORITY_LOWEST); //se sono multithread, questo thread non fa praticamente nulla
	else
		AfxGetThreadContext()->AddWindowRef(m_pMainWnd->m_hWnd, FALSE);//tengo a uno il numero minimo di finestre per impedire la chiusura del thread 
	
	//se siamo in unattendedmode, inizializzo il timer per il suicidio automatico dopo tot minuti
	if (AfxGetBaseApp()->IsInUnattendedMode())
		InitTimer();
	
	BOOL expect100Continue = *(DataBool*)AfxGetSettingValue (snsTbGenlib, szEnvironment, szExpect100Continue, DataBool(TRUE), szTbDefaultSettingFileName);
	SetUseExpect100ContinueInWCFCalls(expect100Continue == TRUE);
	
	//inizializza il runtime del CEF browser
	if (!AfxGetBaseApp()->IsInUnattendedMode())
		CEFInitialize();
	
	if (__super::InitInstance() && !AfxGetDiagnostic()->ErrorFound())
	{
		CTbCommandInterface* pCommand = AfxGetTbCmdManager();
		if (pCommand) 
			pCommand->FireEvent (szApplicationStarted, NULL);
		return TRUE;
	}
	return FALSE; 
}
//-----------------------------------------------------------------------------
BOOL CTaskBuilderApp::InitApplicationContext(const CString& strFileServer, const CString& strInstallation, const CString& strMasterSolutionName)
{
	CApplicationContext *pContext = AfxGetApplicationContext();
	pContext->SetInUnattendedMode(GetArgumentValue(szUnattendedModeParam).CompareNoCase(szTrue) == 0);

	int handle = _ttoi(GetArgumentValue(szMenuHandleParam));
	if (handle > 0)
		pContext->SetMenuWindowHandle((HWND)handle);

	CPathFinder* pPathfinder = new CPathFinder();

	pPathfinder->Init 
		(
			strFileServer,
			strInstallation,
			strMasterSolutionName
		);

	pPathfinder->AttachDictionaryPathFinder(new CDictionaryPathFinder());
	pContext->AttachPathFinder(pPathfinder);

	pContext->AttachClientObjects(new CClientObjects());

	pContext->AttachStringLoader(new CStringLoader(GetInstallationDate().Str(), pPathfinder->GetAppDataPath(TRUE)));
	pContext->AttachParsedControlsRegistry (new CParsedCtrlRegistry());
	pContext->AttachBehavioursRegistry (new CBehavioursRegistry());
	pContext->AttachMailConnector(new IMailConnector());
	pContext->AttachAddOnFieldsTable(new CAlterTableDescriptionArray());
	pContext->AttachEnumsTable(new EnumsTable);
	pContext->AttachDataFilesManager(new CDataFilesManager());
	pContext->AttachClientDocsTable(new CServerDocDescriArray());
	pContext->AttachClientFormsTable(new CServerFormDescriArray());
	pContext->AttachStandardFormatsTable(new FormatStyleTable());
	pContext->AttachStandardFontsTable(new FontStyleTable());
	pContext->AttachStandardDocumentsTable(new DocumentObjectsTable());

	pContext->AttachCommandManager(new CTbCommandManager());
	
	// to reanable Web Service Management (pContext->AttachFileSystemManagerWS(new CFileSystemManagerWebService());)
	CFileSystemManager* pFSManager = new CFileSystemManager(new CFileSystemDriver());
	pContext->AttachFileSystemManager (pFSManager);

	CTBExplorerFactoryUI *pObject = new CTBExplorerFactoryUI();
	pObject->m_rtcDocumentExplorer = RUNTIME_CLASS(CDocumentExplorerDlg);
	pContext->AttachExplorerFactory(pObject);

	pContext->AttachRadarFactory(new CTBRadarFactoryUI());	

	pContext->AttachAddOnApps(new AddOnAppsArray());	
	pContext->AttachGlobalSettingsTable(new SettingsTable);

	return TRUE;
}

//-----------------------------------------------------------------------------
void CTaskBuilderApp::StartWCFServices()
{
	int soapPort = 0, tcpPort = 0;

	//se non mi viene passato alcun argomento relativo alle porte, uso i default dei settings
	BOOL useDefault = !HasArgument(szTBLoaderPortParam) && !HasArgument(szTBLoaderTcpPortParam);
	CTBNamespace aNs (szTbGenlibNamespace);
	DataInt* pSetting = NULL;
	
	pSetting = (DataInt*) AfxGetSettingValue(aNs, szPreferenceSection,  _T("TBLoaderDefaultSOAPPort"));
	int defaultSoapPort = pSetting ? *pSetting : 0;
	
	pSetting = (DataInt*) AfxGetSettingValue(aNs, szPreferenceSection,  _T("TBLoaderDefaultTCPPort"));
	int defaultTcpPort = pSetting ? *pSetting : 0;
	
	//se mi viene passata una porta valida uso quella (anche zero va bene, significa 'NON ASCOLTARE')
	//altrimenti pesco dai settings
	soapPort = useDefault 
		? defaultSoapPort 
		: _ttoi(GetArgumentValue(szTBLoaderPortParam));
	
	tcpPort = useDefault
		? defaultTcpPort
		: _ttoi(GetArgumentValue(szTBLoaderTcpPortParam));

#ifdef DEBUG

	//since in debug mode the installation update information is not modified,
	//when webmethods.xml is modified I have no means to update wcf dll, so
	//I check all dates of WebMethods.xml and, if some date has changed, I recreate WCF dll
	CString sToken;
	for (int nApp = 0; nApp <= AfxGetAddOnAppsTable()->GetUpperBound(); nApp++)
	{
		AddOnApplication* pAddOnApp = AfxGetAddOnAppsTable()->GetAt(nApp);
		ASSERT(pAddOnApp);

		for (int nMod = 0; nMod <= pAddOnApp->m_pAddOnModules->GetUpperBound(); nMod++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(nMod);
			ASSERT(pAddOnMod);
			
			sToken.Append(pAddOnMod->m_XmlDescription.GetWebMethodsModifyDate());
		}
	}

	CString sFile = AfxGetPathFinder()->GetTBDllPath() + SLASH_CHAR + "WCFToken.txt";
	UINT nOpenFlags = CFile::modeReadWrite | CFile::typeText;
	if (!ExistFile(sFile))
		nOpenFlags |= CFile::modeCreate;
	try
	{
		CStdioFile file(sFile, nOpenFlags);
		CString sFileToken;
		file.ReadString(sFileToken);

		if (sFileToken != sToken)
		{
			::DeleteWCFServicesAssembly();
			file.SetLength(0);
			file.WriteString(sToken);
		}

		file.Close();
	}
	catch(...)
	{
	}
#endif

	BOOL bRegistrationNeeded = HasArgument(szRegisterWCFNamespaces);
	if (!bRegistrationNeeded && !WCFServicesCreationNeeded())
	{
		::StartWCFServices(soapPort, tcpPort, this);
		return;
	}

	CBaseDescriptionArray totalFunctions;
	totalFunctions.SetOwns(FALSE);
	AddOnApplication* pAddOnApp;
	for (int nApp = 0; nApp <= AfxGetAddOnAppsTable()->GetUpperBound(); nApp++)
	{
		pAddOnApp = AfxGetAddOnAppsTable()->GetAt(nApp);
		ASSERT(pAddOnApp);

		for (int nMod = 0; nMod <= pAddOnApp->m_pAddOnModules->GetUpperBound(); nMod++)
		{
			AddOnModule* pAddOnMod = pAddOnApp->m_pAddOnModules->GetAt(nMod);
			ASSERT(pAddOnMod);
			
			const CBaseDescriptionArray &functions = pAddOnMod->m_XmlDescription.GetFunctionsInfo().m_arFunctions;
			for (int i = 0; i < functions.GetCount(); i++)
			{
				CFunctionDescription *pFunction = (CFunctionDescription*) functions.GetAt(i);
				if (!pFunction->IsExternalFunction())
					totalFunctions.Add(pFunction);
			}
		}

	}
	CString strUser;
	//se mi viene chiesto da linea di comando, registro i namespaces per l'utente di aspnet che non ha i permessi per WCF
	if (bRegistrationNeeded)
		strUser = AfxGetLoginManager()->GetAspNetUser();

	::CreateWCFServices
		(
		totalFunctions,	//funzioni da buttare nella dll di wcf
		soapPort,		//porta per ascoltare con protocollo soap
		tcpPort,		//porta per ascoltare con protocollo net tcp
		strUser,		//utente col quale gira aspnet
		min(defaultSoapPort, defaultTcpPort), //porta a partire dalla quale andranno registrati i namespaces (siccome e' dinamica =, mi tengo un range di 20 porte a partire da questa)
		this			//per chiamarci sopra delle funzioni virtuali che fanno da ponte
		);

}

// applications and graphical objects
//-----------------------------------------------------------------------------
void CTaskBuilderApp::InitApplicationsAndUI	()
{
	// hourglass disabled
	CStatusBarMsg statusBar(TRUE, TRUE, FALSE); 

	// loads applications
	CApplicationsLoader loader;
	if (!loader.LoadApplications(&statusBar))
		return;
	
	StartWCFServices();
	if (!AfxGetBaseApp()->IsInUnattendedMode())
		::GenerateEasyBuilderEnumsDllAsync();

	// title is updated with brand informations
	CString strAppTitle = AfxGetLoginManager()->GetMasterProductBrandedName();
	if (strAppTitle.IsEmpty())
		strAppTitle = GetMasterAddOnApp()->GetTitle ();
	
	AfxGetApplicationContext()->SetAppTitle(strAppTitle);

	if (AfxThreadLockTraceEnabled())
		AfxGetApplicationContext()->StartLockTracer(AfxGetTbLoaderSOAPPort());
}

// Initialize extensions. Extensions failure are only warnings
//-----------------------------------------------------------------------------
void CTaskBuilderApp::InitExtensions ()
{
	// I can do it only here because AfxIsActivated needs LoginManager
	AfxGetApplicationContext()->SetCanUseReportEditor(AfxIsActivated(TBNET_APP, _NS_ACT("ReportEditor")));

	// I disable the soap server check on the RunFunction (CanExecute checks application validity)
	BOOL bOldValue = EnableSoapExecutionControl(FALSE);

	//BOOL bLoadMailerFailed = FALSE;
	//if (AfxIsActivated(szExtensionsApp, _NS_ACT("TbMailer")))
	//	bLoadMailerFailed = (AfxGetTbCmdManager()->RunFunction(_NS_WEB("Extensions.TbMailer.TbMailer.StartMailConnector"), NULL) == NULL);

	// I restore soap server check 
	EnableSoapExecutionControl(bOldValue);
}

// Initialize the server components needed
//-----------------------------------------------------------------------------
void CTaskBuilderApp::InitServerObjects (const CString& strFileServer, const CString& strWebServer, const CString& strInstallation, const CString& strMasterSolutionName)
{

	// diagnostic is added by Init method
	//attualmente non supportato
	//if (!AfxGetFileSystemManager()->Init (strFileServer, strInstallation))
	//	return;

	// RichEdit control
	AfxInitRichEdit	();

	// ole and soap server components
	if (AfxOleInit())
		AfxEnableControlContainer();
	else
		AfxGetDiagnostic()->Add (_T("Inizializzazione interfaccia OLE fallita."), CDiagnostic::Error);

	CPathFinder* pPathFinder = AfxGetPathFinder();
	pPathFinder->Init 
		(
			strFileServer,
			strInstallation,
			strMasterSolutionName
		);

	// server web services connections
	if	(AfxGetCommonClientObjects()->InitWebServicesConnections(strWebServer, strInstallation))
		AfxGetThreadContext()->SetUICulture(AfxGetCommonClientObjects()->GetServerConnectionInfo()->m_sPreferredLanguage);
	else
	{
		AfxGetDiagnostic()->Add (szMsgFailed, CDiagnostic::Error);	
		return;
	}

	AfxGetFileSystemManager()->Start ();

	// activation state data init
	AfxGetLoginManager()->InitActivationStateInfo ();
	
	if (IsDevelopment())
		AfxGetStringLoader()->FreeModules();
}

//-----------------------------------------------------------------------------
void CTaskBuilderApp::AdjustClosingMessageViewer()
{
	// exit log can be enabled
	BOOL bExitLog = *(DataBool*) AfxGetSettingValue(CTBNamespace(szTbGenlibNamespace), szEnvironment, szLogExitInstance, DataBool(FALSE));
	CString sLogFile;
	
	if (bExitLog)
	{
		CString sUser = AfxGetLoginInfos() ? AfxGetLoginInfos()->m_strUserName : _T("");
		sLogFile  = AfxGetPathFinder()->GetExitLogFullName(sUser, TRUE);
		AfxGetDiagnostic()->AttachViewer(new CLogFileViewer(sLogFile));
		AfxGetDiagnostic()->EnableTraceInEventViewer(IsEventViewerTraceEnabled());
	}
	else
		AfxGetDiagnostic()->AttachViewer(NULL);
}

// application exit
//-----------------------------------------------------------------------------
int CTaskBuilderApp::ExitInstance()
{
	try
	{
		{
			USES_DIAGNOSTIC();

			FreeAssemblyObjects();
	
			DumpAssertions();
			// disable release assertions for the final operations, as they was already dumped
			AfxGetApplicationContext()->SetEnableAssertionsInRelease(FALSE);
	
			AdjustClosingMessageViewer();

			AfxGetApplicationContext()->CloseAllLogins();
			AfxGetDiagnostic()->Add (_T("CloseAllLogins executed successfully."), CDiagnostic::Info);

			::StopWCFServices();

			AfxGetDiagnostic()->Add (_T("AfxStopSoapServer executed successfully."), CDiagnostic::Info);

		} // after free object log file cannot be shown

		delete CModuleDescription::s_pReferenceObjectsParser;

		AfxGetApplicationContext()->FreeObjects();
	
		AfxGetApplicationContext()->StopLockTracer();
	
		if (m_pMainWnd)
			m_pMainWnd->DestroyWindow();

		//if (CTBPicture::GetUseGdiPlus())
			CTBPicture::TerminateGdiPlus();

		if (!AfxGetBaseApp()->IsInUnattendedMode())
			CEFUninitialize();

		AfxDeleteRSIconArray();
	}
	catch (...)
	{
	}
	
	int nRet = __super::ExitInstance();
	CoUninitialize();
	return nRet;

}

//------------------------------------------------------------------------------
BOOL CTaskBuilderApp::IsMasterFrame(CRuntimeClass* pRTFrame)
{
	CRuntimeClass* pRTMaster = RUNTIME_CLASS(CMasterFrame);
	
	return	(pRTFrame && pRTMaster && 
				(
					pRTFrame->m_lpszClassName == pRTMaster->m_lpszClassName ||	
					pRTFrame->IsDerivedFrom(pRTMaster)
				)
			);	
}

//------------------------------------------------------------------------------
BOOL AfxReloadApplication(const CString& sAppName)
{
	AddOnAppsArray *pArray = (AddOnAppsArray*) AfxGetAddOnAppsTable();
	for (int i = pArray->GetUpperBound(); i >= 0; i--)
	{
		AddOnApplication* pAddOnApp = pArray->GetAt(i);
		ASSERT(pAddOnApp);

		if (sAppName.CollateNoCase(pAddOnApp->m_strAddOnAppName) == 0)
		{
			pArray->RemoveAt(i);
			break;
		}
	}

	CApplicationsLoader loader;
	return loader.ReloadApplication(sAppName);
}
