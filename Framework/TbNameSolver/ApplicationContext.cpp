// GlobalApp.cpp : implementation file
//
#include "stdafx.h"
#include "ApplicationContext.h"
#include "PathFinder.h"
#include "IFileSystemManager.h"
#include "LoginContext.h"
#include "LockTracer.h"
#include "CompanyContext.h"

// CApplicationContext

static const TCHAR* szTaskBuilderTitle		= _T("TaskBuilder");

TPCallSetTracedFunction* s_pfCallTraced = NULL; //TBAuditing
TPCallSetProtectedFunction* s_pfCallProtected = NULL; //TBRowSecurityLayer
ExpFilterFunction* s_pfExpFilter = NULL; //exception handler

#ifdef DEBUG
BOOL g_bNO_ASSERT = FALSE;

LONG WINAPI ExpFilter(EXCEPTION_POINTERS* pExp, DWORD dwExpCode);

int MyReportHook(int reportType, char *message, int *returnValue)
{
	if (reportType != _CRT_ASSERT)
		return FALSE;

	//if an ASSERT is shown during dll initialization and you are debugging,
	//an exception of loader lock is thrown and you cannot go on debugging
	if (g_bNO_ASSERT)
	{
		USES_CONVERSION;
		AtlTrace("WARNING! AN ASSERTION HAS FAILED DURING DLL LOADING;\r\n message:%s\r\n", A2T(message));
		*returnValue = TRUE;//debug break
		return TRUE;
	}
	if (AfxGetApplicationContext()->IsRemoteInterface())
	{
		USES_CONVERSION;
		AtlTrace("WARNING! AN ASSERTION HAS FAILED RUNNING CODE WHEN APPLICATION HAS NO INTERFACE\r\n message:%s\r\n", A2T(message));
		*returnValue = FALSE;//no debug break
		return TRUE;
	}
	return FALSE;
}
#endif

//special window to identify a sort of desktow virtual window 
//the original value is defined in \standard\taskbuilder\Extensions\TbAPIHooks\TBWnd.h
#define HWND_TBMFC_SPECIAL ((HWND) -20)

BOOL TBHookedApis()
{
	static BOOL b = IsWindow(HWND_TBMFC_SPECIAL);//se è true, vuol dire che ho hookato la funzione
	return b;
}


IMPLEMENT_DYNCREATE(CApplicationContext, CObject)

//----------------------------------------------------------------------------
CApplicationContext::CApplicationContext()
: 
	m_pStringLoader(NULL),
	m_pPathFinder(NULL),
	m_pClientObjects(NULL),
	m_pMailConnector(NULL),
	m_pAddOnFieldsTable(NULL),
	m_pDeclaredDBReleasesTable(NULL),
	m_pFileSystemManager(NULL),
	m_pEnumsTable(NULL),
	m_pDataFilesManager(NULL),
	m_pClientDocsTable(NULL),
	m_pClientFormsTable(NULL),
	m_pCommandManager(NULL),
	m_pSoapServer(NULL),
	m_pFileSystemManagerWS(NULL),
	m_pAddOnApps(NULL),
	m_strAppTitle(szTaskBuilderTitle),
	m_bUnattendedMode(FALSE),
	m_bEnableReportEditor(FALSE),
	m_pGlobalSettingsTable(NULL),
	m_MacroRecorderStatus(IDLE),
	m_pLockTracer(NULL),	//auto delete object
	m_pStandardFormatTable(NULL),
	m_pStandardFontsTable(NULL),
	m_pStandardDocumentsTable(NULL),	
	m_pRadarFactory(NULL),
	m_pExplorerFactory(NULL),
	m_pHotLinkFactory(NULL),
	m_nMenuWindowHandle(NULL),
	m_pDatabaseObjectsTable	(NULL),
	m_bEnableAssertionsInRelease(FALSE), // start FALSE to avoid issuing assertions before loading settings and discover if they are to be issued or not
	m_bDumpAssertionsIfNoCrash(FALSE),
	m_bMultiThreadedDocument(TRUE), 
	m_bMultiThreadedLogin(TRUE),
	m_bIISModule(FALSE),
	m_bRemoteInterface(FALSE),
	m_bEnumsViewerThreadOpened(FALSE),
	m_bEnableActiveAccessibility(FALSE) // FALSE by default as it implies the creation of COM object by the inspecting applications
{
#ifdef DEBUG
	_CrtSetReportHook(MyReportHook);
#endif
}


//----------------------------------------------------------------------------
void CApplicationContext::FreeObjects()
{	
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
		delete m_arLoginContexts[i];
	
	POSITION pos = m_CompanyContexts.GetStartPosition();
	while (pos)
	{
		CCompanyContext *pObject;
		CString sKey;
		m_CompanyContexts.GetNextAssoc(pos, sKey, pObject);
		delete pObject;
	}

	//WARNING: order of deletion may be important, because some object
	//uses some of the other ones
	__super::FreeObjects();

	delete m_pStringLoader;
	delete m_pPathFinder;
	delete m_pClientObjects;
	delete m_pFileSystemManager;
	delete m_pMailConnector;
	delete m_pAddOnFieldsTable;
	delete m_pDeclaredDBReleasesTable;
	delete m_pEnumsTable;
	delete m_pDataFilesManager;
	delete m_pClientDocsTable;
	delete m_pClientFormsTable;
	delete m_pCommandManager;
	delete m_pSoapServer;
	delete m_pFileSystemManagerWS;
	delete m_pAddOnApps;
	delete m_pGlobalSettingsTable;
	delete m_pStandardFormatTable;
	delete m_pStandardFontsTable;
	delete m_pStandardDocumentsTable;
	delete m_pRadarFactory;
	delete m_pExplorerFactory;
	delete m_pHotLinkFactory;
	delete m_pParsedControlsRegistry;
	delete m_pBehavioursRegistry;
}

//----------------------------------------------------------------------------
CApplicationContext::~CApplicationContext()
{
	delete m_pDatabaseObjectsTable;
}

//----------------------------------------------------------------------------
void CApplicationContext::SetAppTitle(const CString& strTitle)
{
	m_strAppTitle = strTitle;
}

//----------------------------------------------------------------------------
const CString& CApplicationContext::GetAppTitle()
{
	return m_strAppTitle;
}

//----------------------------------------------------------------------------
void CApplicationContext::AttachFileSystemManager(IFileSystemManager* pObj)	
{ 
	delete m_pFileSystemManager; 
	m_pFileSystemManager = pObj; 
}

//----------------------------------------------------------------------------
void CApplicationContext::AttachMailConnector(CObject* pObj)
{
	delete m_pMailConnector;
	m_pMailConnector = pObj; 
}
//----------------------------------------------------------------------------
void CApplicationContext::AttachStandardFormatsTable (CObject* pObj)
{
	delete m_pStandardFormatTable;
	m_pStandardFormatTable = pObj; 
}
//----------------------------------------------------------------------------
void CApplicationContext::AttachStandardFontsTable (CObject* pObj)
{
	delete m_pStandardFontsTable;
	m_pStandardFontsTable = pObj; 
}

//----------------------------------------------------------------------------
void CApplicationContext::AttachStandardDocumentsTable (CObject* pObj)
{
	delete m_pStandardDocumentsTable;
	m_pStandardDocumentsTable = pObj; 
}

//----------------------------------------------------------------------------
void CApplicationContext::AttachPathFinder(CPathFinder* pObj)
{
	delete m_pPathFinder;
	m_pPathFinder = pObj; 
}

//----------------------------------------------------------------------------
CCompanyContext* CApplicationContext::GetCompanyContext(const CString& strKey)
{
	if (strKey.IsEmpty())
		return NULL;

	TB_OBJECT_LOCK(&m_CompanyContexts);
	CCompanyContext* pContext = NULL;
	if (!m_CompanyContexts.Lookup(strKey, pContext))
	{
		pContext = new CCompanyContext();
		m_CompanyContexts[strKey] = pContext;
	}
	return pContext;
}

//----------------------------------------------------------------------------
void CApplicationContext::GetLoginContextIds(CDWordArray& arIds)
{
	TB_OBJECT_LOCK_FOR_READ(&m_arLoginContexts);
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
	{
		CLoginContext* pContext = (CLoginContext*) m_arLoginContexts[i];
		arIds.Add(pContext->m_nThreadID);
	}
}
//----------------------------------------------------------------------------
CLoginContext* CApplicationContext::GetLoginContext(const CString& strName)
{
	if (strName.IsEmpty())
		return NULL;

	TB_OBJECT_LOCK_FOR_READ(&m_arLoginContexts);
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
	{
		CLoginContext* pContext = (CLoginContext*) m_arLoginContexts[i];
		if (pContext->GetName() == strName)
			return pContext;
	}

	return NULL;
}
//----------------------------------------------------------------------------
CLoginContext* CApplicationContext::GetLoginContext(DWORD id)
{
	TB_OBJECT_LOCK_FOR_READ(&m_arLoginContexts);
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
	{
		CLoginContext* pContext = (CLoginContext*) m_arLoginContexts[i];
		if (pContext->m_nThreadID == id)
			return pContext;
	}

	return NULL;
}
//----------------------------------------------------------------------------
void CApplicationContext::AddLoginContext(CLoginContext* pObj)
{
	TB_OBJECT_LOCK(&m_arLoginContexts);
	m_arLoginContexts.Add(pObj);
}

//----------------------------------------------------------------------------
UINT CApplicationContext::GetLoginContextNumber()
{
	TB_OBJECT_LOCK_FOR_READ(&m_arLoginContexts);
	return m_arLoginContexts.GetCount();
}

//----------------------------------------------------------------------------
void CApplicationContext::RemoveLoginContext(CLoginContext* pObj)
{
	TB_OBJECT_LOCK(&m_arLoginContexts);
	int index = -1;
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
		if (m_arLoginContexts[i] == pObj)
		{
			index = i;
			break;
		}

	if (index != -1)
		m_arLoginContexts.RemoveAt(index);
}

//----------------------------------------------------------------------------
CString CApplicationContext::GetThreadInfos()
{
	m_arThreadInfo.Clear();
	CDWordArray arThreads;

	BEGIN_TB_OBJECT_LOCK_FOR_READ(&m_arLoginContexts); //lock in lettura
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
	{
		CLoginContext* pContext = (CLoginContext*)m_arLoginContexts[i];
		arThreads.Add(pContext->m_nThreadID);
		//chiamata ASINCRONA per evitare deadlocks
		AfxInvokeAsyncThreadFunction<CThreadInfo*, CTBWinThread, CThreadInfoArray&>(pContext->m_nThreadID, pContext, &CTBWinThread::AddThreadInfos, m_arThreadInfo);
	}
	END_TB_OBJECT_LOCK();

	//aspetto che tutti i thread riempiano la struttura, scremando quelli che nel frattempo sono morti
	while (arThreads.GetCount() != m_arThreadInfo.GetCount())
	{
		for (int i = arThreads.GetUpperBound(); i >=0; i--)
			if (!CTBWinThread::IsThreadAlive(arThreads[i]))
				arThreads.RemoveAt(i);

		CTBWinThread::PumpThreadMessages();
	}

	return m_arThreadInfo.ToXmlString();
}

//----------------------------------------------------------------------------
UINT CApplicationContext::GetAllOpenDocumentNumber()
{
	UINT openDocumentNumber = 0;

	BEGIN_TB_OBJECT_LOCK_FOR_READ(&m_arLoginContexts); //lock in lettura
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
	{
		CLoginContext* pContext = (CLoginContext*)m_arLoginContexts[i];

		openDocumentNumber += AfxInvokeThreadFunction<int, CLoginContext>(pContext->m_nThreadID, pContext, & CLoginContext::GetOpenDocuments);
	}
	END_TB_OBJECT_LOCK();

	return openDocumentNumber;
}

//----------------------------------------------------------------------------
UINT CApplicationContext::GetAllOpenDocumentNumberEditMode()
{
	UINT openDocumentNumber = 0;

	BEGIN_TB_OBJECT_LOCK_FOR_READ(&m_arLoginContexts); //lock in lettura
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
	{
		CLoginContext* pContext = (CLoginContext*)m_arLoginContexts[i];

		openDocumentNumber += AfxInvokeThreadFunction<int, CLoginContext>(pContext->m_nThreadID, pContext, &CLoginContext::GetOpenDocumentsInDesignMode);
	}
	END_TB_OBJECT_LOCK();

	return openDocumentNumber;
}

//----------------------------------------------------------------------------
void CApplicationContext::AddWebServiceStateObject(CObject* pObject, HWND hwndThreadWindow)
{
	TB_OBJECT_LOCK(&m_WebServiceStateObjects); 
	m_WebServiceStateObjects[pObject] = hwndThreadWindow; 
}
//----------------------------------------------------------------------------
BOOL CApplicationContext::ExistWebServiceStateObject(CObject* pObject)
{
	TB_OBJECT_LOCK_FOR_READ(&m_WebServiceStateObjects); 
	HWND dummy;
	return m_WebServiceStateObjects.Lookup(pObject, dummy);
}
//----------------------------------------------------------------------------
void CApplicationContext::RemoveWebServiceStateObject(CObject* pObject)
{
	TB_OBJECT_LOCK(&m_WebServiceStateObjects); 
	m_WebServiceStateObjects.RemoveKey(pObject); 
}
//----------------------------------------------------------------------------
void CApplicationContext::RemoveWebServiceStateObjects(HWND hwndThreadWindow)
{
	TB_OBJECT_LOCK(&m_WebServiceStateObjects); 
	CObArray keysToRemove;
	POSITION pos = m_WebServiceStateObjects.GetStartPosition(); 
	while (pos)
	{
		HWND val;
		CObject* key;
		m_WebServiceStateObjects.GetNextAssoc(pos, key, val);
		if (val == hwndThreadWindow)
			keysToRemove.Add(key);
	}

	for (int i =0; i < keysToRemove.GetCount(); i++)
		m_WebServiceStateObjects.RemoveKey(keysToRemove[i]);
}
//----------------------------------------------------------------------------
HWND CApplicationContext::GetWebServiceStateObjectWnd(CObject* pObject)
{
	TB_OBJECT_LOCK_FOR_READ(&m_WebServiceStateObjects); 
	HWND hwnd = NULL;
	m_WebServiceStateObjects.Lookup(pObject, hwnd); 
	return hwnd;
}

//----------------------------------------------------------------------------
void CApplicationContext::AddThread(DWORD dwThreadId, HWND hwndMainWindow)
{
	TB_OBJECT_LOCK(&m_ThreadMap);
	m_ThreadMap[dwThreadId] = hwndMainWindow;
}
//----------------------------------------------------------------------------
void CApplicationContext::RemoveThread(DWORD dwThreadId)
{
	TB_OBJECT_LOCK(&m_ThreadMap);
	m_ThreadMap.RemoveKey(dwThreadId);
}
//----------------------------------------------------------------------------
HWND CApplicationContext::GetThreadMainWindow(DWORD dwThreadId)
{
	TB_OBJECT_LOCK_FOR_READ(&m_ThreadMap);
	return m_ThreadMap[dwThreadId];
}

//-----------------------------------------------------------------------------
CRuntimeClass* CApplicationContext::GetControlClass(UINT id)
{
	TB_OBJECT_LOCK_FOR_READ(&m_RegisteredControls);
	CRuntimeClass* pClass = NULL;
	m_RegisteredControls.Lookup(id, pClass);
	return pClass;
}
//-----------------------------------------------------------------------------
void CApplicationContext::RegisterControl(UINT nIDD, CRuntimeClass* pClass)
{
	TB_OBJECT_LOCK(&m_ThreadMap);
	CRuntimeClass* pExistingClass = NULL;
	ASSERT(!m_RegisteredControls.Lookup(nIDD, pExistingClass));
	m_RegisteredControls[nIDD] = pClass;
}
//----------------------------------------------------------------------------
void CApplicationContext::CloseLoginThread(DWORD nThreadId, HANDLE hThreadHandle)
{
	//if current thread is the login one, i cannot kill myself and wait for my death,
	//so i delegate application thrhead to kill me (I HATE SUICIDES!)
	if (nThreadId == ::GetCurrentThreadId())
	{
		
		AfxGetApp()->PostThreadMessage(UM_CLOSE_LOGIN, (WPARAM)nThreadId, (LPARAM)hThreadHandle);
		return;
	}
	PostThreadMessage(nThreadId, UM_CLOSE_LOGIN_ASYNC, 0, NULL);
	
	while (WAIT_OBJECT_0 != WaitForSingleObject(hThreadHandle, 1000))
		CTBWinThread::PumpThreadMessages();
}
//----------------------------------------------------------------------------
void CApplicationContext::CloseAllLogins(BOOL bOnlyInvalid /*= FALSE*/)
{
	CDWordArray arThreadsId;
	CPtrArray arThreadsHandles;
	BEGIN_TB_OBJECT_LOCK_FOR_READ(&m_arLoginContexts);
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
	{
		CLoginContext* pContext = (CLoginContext*) m_arLoginContexts[i];
		if (bOnlyInvalid && pContext->IsValid())
			continue;
		arThreadsId.Add(pContext->m_nThreadID);
		arThreadsHandles.Add(pContext->m_hThread);
	}
	END_TB_OBJECT_LOCK();
	for (int i = 0; i < arThreadsId.GetCount(); i++)
		CloseLoginThread(arThreadsId[i], arThreadsHandles[i]);
}
//----------------------------------------------------------------------------
BOOL CApplicationContext::CanClose()
{
	TB_OBJECT_LOCK(&m_arLoginContexts);
	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
	{
		CLoginContext* pContext = (CLoginContext*) m_arLoginContexts[i];
		if (!pContext->CanClose())
			return FALSE;
	}
	return TRUE;
}

// CApplicationContext message handlers
//----------------------------------------------------------------------------
void CApplicationContext::StartLockTracer(UINT nTBPort)
{
	ASSERT(m_pLockTracer == NULL);
	m_pLockTracer = (CLockTracer*) RUNTIME_CLASS(CLockTracer)->CreateObject();
	m_pLockTracer->SetPort(nTBPort);
	m_pLockTracer->CreateThread();
	m_pLockTracer->WaitForStartup();
	if (!m_pLockTracer->IsValid())
		StopLockTracer();
}
//----------------------------------------------------------------------------
void CApplicationContext::StopLockTracer()
{
	if (m_pLockTracer)
	{
		CLockTracer* pTracer = m_pLockTracer;
		m_pLockTracer = NULL;
		pTracer->StopAndDestroy();
	}
}

//----------------------------------------------------------------------------
void CApplicationContext::AddFormatterToLoginContext (Formatter* pFormatter)
{
	TB_OBJECT_LOCK_FOR_READ(&m_arLoginContexts);

	for (int i = 0; i < m_arLoginContexts.GetCount(); i++)
	{
		CLoginContext* pContext = (CLoginContext*) m_arLoginContexts[i];
		pContext->AddFormatter(pFormatter);
	}
}

static CApplicationContext g_ApplicationContext;
//-----------------------------------------------------------------------------
CApplicationContext* AFXAPI AfxGetApplicationContext()
{ 
	return &g_ApplicationContext;
}    


#define MAX_COLLECTED_ASSERTIONS	1000	// single line < 500 bytes => 0.5 Mb maximum used memory
//-----------------------------------------------------------------------------
BOOL CApplicationContext::_AssertAlwaysFailedLine
	(
		LPCSTR lpszFailedTest, 
		DWORD threadId, 
		LPCSTR lpszFunction, 
		LPCSTR lpszFileName, 
		int nLine, 
		const TCHAR * szAdditionalMessage /*= NULL*/
	)
{
	TB_LOCK_FOR_WRITE();
	USES_CONVERSION;
	CString strMessage;
	if (m_FailedAssertions.GetCount() < MAX_COLLECTED_ASSERTIONS - 1)
	{
		strMessage.Format	(
								_T("ASSERT(%s) failed in thread %u\nfunction %s\nfile %s\nat line %d.%s%s"), 
								A2T(lpszFailedTest), 
								threadId,
								A2T(lpszFunction), 
								A2T(lpszFileName), 
								nLine,
								szAdditionalMessage ? _T("\n") :_T(""),
								szAdditionalMessage ? szAdditionalMessage :_T("")
							);
		m_FailedAssertions.Add(strMessage);
	}
	else if (m_FailedAssertions.GetCount() == MAX_COLLECTED_ASSERTIONS - 1)
	{
		m_FailedAssertions.Add(_T("Exceeding maximum number of collected assertions (1000), stop collecting."));
		m_bEnableAssertionsInRelease = FALSE; // disable assertions from now on, as it stopped collecting them
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CApplicationContext::AssertAlwaysFailedLine(LPCSTR lpszFailedTest, DWORD threadId, LPCSTR lpszFunction, LPCSTR lpszFileName, int nLine)
{
	if (!AreReleaseAssertionsEnabled() || m_FailedAssertions.GetCount() >= MAX_COLLECTED_ASSERTIONS)
		return TRUE;

	return _AssertAlwaysFailedLine(lpszFailedTest,threadId,lpszFunction,lpszFileName,nLine,NULL);
}

CString _local_cwsprintf( const TCHAR *str, va_list marker )
{
	CString s;
	s.FormatV(str, marker);
	return s;
}

CStringA _local_csprintf( const CHAR *str, va_list marker )
{
	CStringA s;
	s.FormatV(str, marker);
	return s;
}

//-----------------------------------------------------------------------------
BOOL CApplicationContext::AssertAlwaysFailedLine(LPCSTR lpszFailedTest, DWORD threadId, LPCSTR lpszFunction, LPCSTR lpszFileName, int nLine, const TCHAR * szAdditionalMessage, ...)
{
	if (!AreReleaseAssertionsEnabled() || m_FailedAssertions.GetCount() >= MAX_COLLECTED_ASSERTIONS)
		return TRUE;

	va_list marker;

    va_start( marker, szAdditionalMessage );
	CString strFormattedMessage = _local_cwsprintf(szAdditionalMessage, marker);
    va_end( marker );

	return _AssertAlwaysFailedLine(lpszFailedTest,threadId,lpszFunction,lpszFileName,nLine,strFormattedMessage);
}

//-----------------------------------------------------------------------------
BOOL CApplicationContext::AssertAlwaysFailedLine(LPCSTR lpszFailedTest, DWORD threadId, LPCSTR lpszFunction, LPCSTR lpszFileName, int nLine, const CHAR * szAdditionalMessage, ...)
{
	if (!AreReleaseAssertionsEnabled() || m_FailedAssertions.GetCount() >= MAX_COLLECTED_ASSERTIONS)
		return TRUE;

	va_list marker;

    va_start( marker, szAdditionalMessage );
	CStringA strFormattedMessage = _local_csprintf(szAdditionalMessage, marker);
    va_end( marker );

	return _AssertAlwaysFailedLine(lpszFailedTest,threadId,lpszFunction,lpszFileName,nLine,CString(strFormattedMessage));
}

//-----------------------------------------------------------------------------
CString CApplicationContext::GetFailedAssertion(int i)
{ 
	if (i < 0 || i >= m_FailedAssertions.GetCount())
		return _T("BAD INDEX");
	
	return m_FailedAssertions.GetAt(i);
}

//---------------------------------------------------
BOOL CApplicationContext::IsEnumsViewerThreadOpened()	const
{
	TB_LOCK_FOR_READ();
	return m_bEnumsViewerThreadOpened;
}

//---------------------------------------------------
void CApplicationContext::SetEnumsViewerThreadOpened(BOOL bValue)
{
	TB_LOCK_FOR_WRITE();
	m_bEnumsViewerThreadOpened = bValue;
}
