#include "stdafx.h"


#include <strsafe.h>
#include <tlhelp32.h>
#include <Psapi.h>

#include <TbNameSolver\Diagnostic.h>
#include <TbNameSolver\ApplicationContext.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbGenlib\generic.h>
#include <TbGenlib\TBCommandInterface.h>
#include <TbGenLibManaged\Main.h>
#include <TbClientCore\ClientObjects.h>
#include <TbGeneric\FontsTable.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\TBThemeManager.h>
#include "ThreadMainWindow.h"
#include "StackWalker.h"

int GetProcessThreadCount() 
{ 
	HANDLE hThreadSnap = INVALID_HANDLE_VALUE; 
	THREADENTRY32 te32; 
	int count = 0;
	// Take a snapshot of all running threads  
	hThreadSnap = CreateToolhelp32Snapshot( TH32CS_SNAPTHREAD, 0 ); 
	if( hThreadSnap == INVALID_HANDLE_VALUE ) 
		return( FALSE ); 
 
	// Fill in the size of the structure before using it. 
	te32.dwSize = sizeof(THREADENTRY32 ); 
 
	// Retrieve information about the first thread,
	// and exit if unsuccessful
	if( !Thread32First( hThreadSnap, &te32 ) ) 
	{
		CloseHandle( hThreadSnap );     // Must clean up the snapshot object!
		return( 0 );
	}
	DWORD dwOwnerPID = GetCurrentProcessId();
	// Now walk the thread list of the system,
	// and display information about each thread
	// associated with the specified process
	do 
	{ 
		if( te32.th32OwnerProcessID == dwOwnerPID )
			count++;
	} while( Thread32Next(hThreadSnap, &te32 ) );

	//  Don't forget to clean up the snapshot object.
	CloseHandle( hThreadSnap );
	return( count );
}

	BEGIN_MESSAGE_MAP(CThreadMainWindow, CTBLockedFrame)

	ON_MESSAGE	(UM_GET_SOAP_PORT,		OnGetSoapPort)
	ON_MESSAGE	(UM_EXECUTE_FUNCTION,	OnExecuteFunction)
	ON_MESSAGE (UM_IS_UNATTENDED_WINDOW, OnIsUnattendedWindow)
END_MESSAGE_MAP()

IMPLEMENT_DYNAMIC(CThreadMainWindow, CTBLockedFrame)

//-----------------------------------------------------------------------------
CThreadMainWindow::CThreadMainWindow(BOOL bRegisterInMap)		
{
	CWnd* pWnd = AfxGetThreadContext() ? AfxGetThreadContext()->GetMenuWindow() : NULL;
	VERIFY(Create(NULL, NULL, WS_OVERLAPPEDWINDOW, rectDefault, pWnd));
	m_bRegisterInMap = bRegisterInMap;
	if (m_bRegisterInMap)
		AfxGetApplicationContext()->AddThread(::GetCurrentThreadId(), m_hWnd);

}

//-----------------------------------------------------------------------------
CThreadMainWindow::~CThreadMainWindow()
{
	if (m_bRegisterInMap)
		AfxGetApplicationContext()->RemoveThread(::GetCurrentThreadId());
}
//-----------------------------------------------------------------------------
LRESULT CThreadMainWindow::OnGetSoapPort(WPARAM wParam, LPARAM lParam)
{
	return AfxGetTbLoaderSOAPPort();
}
//----------------------------------------------------------------------------
LRESULT CThreadMainWindow::OnExecuteFunction(WPARAM wParam, LPARAM lParam)
{
	CBaseFunctionWrapper* pWrapper = (CBaseFunctionWrapper*)wParam; 
	return pWrapper->OnExecuteFunction();	
}

//----------------------------------------------------------------------------
LRESULT CThreadMainWindow::OnIsUnattendedWindow(WPARAM wParam, LPARAM lParam)
{
	return TRUE;
}

//----------------------------------------------------------------------------
BOOL CThreadMainWindow::DestroyWindow()
{
	AfxGetApplicationContext()->RemoveWebServiceStateObjects(m_hWnd);

	return CFrameWnd::DestroyWindow();
}
#define DIAGNOSTIC_TOOL_TITLE _T("TaskBuilder.Net Diagnostic System\r\n\r\n")
//----------------------------------------------------------------------------
///salva le informazioni dello stack su un file di log (se ci sono i pdb scrive anche i nomi e le righe di file)
//inoltre produce un file .dat contenente le strutture di debug per ricreare lo stesso file in altra sede (dove magari ci
//sono i pdb)
class StackWalkerToFile : public StackWalker
{
protected:
	CStdioFile	m_LogFile;
	CString		m_sFileName;
	CString		m_sTbApplicationPath;
	CString		m_sSystemPath;
public:
	//----------------------------------------------------------------------------
	StackWalkerToFile(const CString& strLogFile = _T(""), const CString& strDataFile = _T(""), BOOL bWriteDataFile = TRUE)
	{
		USES_CONVERSION;
		
		CStringA m_sSymbolPath = T2A(AfxGetPathFinder()->GetCustomDebugSymbolsPath());
		
		int nSize = m_sSymbolPath.GetLength() + 1;
		m_szSymPath = (LPSTR) malloc(nSize);
		memcpy(m_szSymPath, m_sSymbolPath, nSize);
		
		if (strLogFile.IsEmpty())
		{
			CString sPath = AfxGetPathFinder()->GetLogDataIOPath(TRUE, CPathFinder::ALL_COMPANIES);
			SYSTEMTIME	systime;
			::GetLocalTime (&systime);
			m_sFileName.Format(_T("%s\\%s-%u-%.2u-%.2u-%.2u-%.2u-%.2u.log"),
						sPath,
						::GetComputerName(FALSE),
						systime.wYear, systime.wMonth, systime.wDay,
						systime.wHour, systime.wMinute, systime.wSecond
					);
		}
		else
		{
			m_sFileName = strLogFile;
		}

		m_LogFile.Open(m_sFileName, CFile::modeWrite | CFile::modeCreate);
		m_LogFile.WriteString(DIAGNOSTIC_TOOL_TITLE);
		m_LogFile.WriteString(::GetComputerName(FALSE) + _T("\r\n"));
		
		CString os;
		if (GetOSDisplayString(os))
		{
			m_LogFile.WriteString(_T("Operating System:"));
			m_LogFile.WriteString(os);
			m_LogFile.WriteString(_T("\r\n"));
		}

		CTBWinThread* pThread = AfxGetTBThread();
		if (pThread)
			m_LogFile.WriteString(CString(_T("Thread name: ")) + A2T(pThread->m_strName) + _T("\r\n"));
		
		if (AfxIsInUnattendedMode())
			m_LogFile.WriteString(_T("Unattended\r\n"));
		else
			m_LogFile.WriteString(_T("Attended\r\n"));
		
		m_LogFile.WriteString(_T("SingleThreaded:"));
		if (AfxGetApplicationContext()->IsMultiThreadedDocument())
			m_LogFile.WriteString(_T("No\r\n"));
		else
			m_LogFile.WriteString(_T("Yes\r\n"));

		m_LogFile.WriteString(_T("TabbedDocuments:"));
		DataObj* pDataObj = AfxGetSettingValue (snsTbGenlib, szFormsSection, _T("TabbedDocuments"), DataBool(TRUE), szTbDefaultSettingFileName);
		if (pDataObj != NULL && pDataObj->IsKindOf(RUNTIME_CLASS(DataBool)) && (*(DataBool*)pDataObj) == TRUE)
			m_LogFile.WriteString(_T("Yes\r\n"));
		else
			m_LogFile.WriteString(_T("No\r\n"));

		m_LogFile.WriteString(_T("EnableAssertionsInRelease:"));
		if (AfxGetApplicationContext()->AreReleaseAssertionsEnabled())
			m_LogFile.WriteString(_T("Yes\r\n"));
		else
			m_LogFile.WriteString(_T("No\r\n"));

		DWORD gdi = GetGuiResources(GetCurrentProcess(), GR_GDIOBJECTS);
		m_LogFile.WriteString(cwsprintf(_T("GDI objects: %d\r\n"), gdi));

		int logins = AfxGetApplicationContext()->GetLoginContextNumber();
		m_LogFile.WriteString(cwsprintf(_T("Logins: %d\r\n"), logins));

		int threads = GetProcessThreadCount();
		m_LogFile.WriteString(cwsprintf(_T("Threads: %d\r\n"), threads));

		_PROCESS_MEMORY_COUNTERS  psmemCounters;
		ZeroMemory(&psmemCounters, sizeof(_PROCESS_MEMORY_COUNTERS));
		if (GetProcessMemoryInfo(GetCurrentProcess(), &psmemCounters, sizeof(_PROCESS_MEMORY_COUNTERS)))
		{
			m_LogFile.WriteString(cwsprintf(_T("PageFaultCount: %d\r\n"), psmemCounters.PageFaultCount));
			m_LogFile.WriteString(cwsprintf(_T("PagefileUsage: %lu\r\n"), psmemCounters.PagefileUsage));
			m_LogFile.WriteString(cwsprintf(_T("PeakWorkingSetSize: %lu\r\n"), psmemCounters.PeakWorkingSetSize));
			m_LogFile.WriteString(cwsprintf(_T("QuotaNonPagedPoolUsage: %lu\r\n"), psmemCounters.QuotaNonPagedPoolUsage));
			m_LogFile.WriteString(cwsprintf(_T("QuotaPagedPoolUsage: %lu\r\n"), psmemCounters.QuotaPagedPoolUsage));
			m_LogFile.WriteString(cwsprintf(_T("QuotaPeakNonPagedPoolUsage: %lu\r\n"), psmemCounters.QuotaPeakNonPagedPoolUsage));
			m_LogFile.WriteString(cwsprintf(_T("QuotaPeakPagedPoolUsage: %lu\r\n"), psmemCounters.QuotaPeakPagedPoolUsage));
			m_LogFile.WriteString(cwsprintf(_T("WorkingSetSize: %lu\r\n"), psmemCounters.WorkingSetSize));
		}
		m_LogFile.WriteString(cwsprintf(_T("Current thread documents: %d\r\n"), AfxGetThreadContext()->GetDocuments().GetCount()));
		m_LogFile.WriteString(cwsprintf(_T("Current thread windows: %d\r\n"), AfxGetThreadContext()->GetThreadWindows().GetCount()));
			
		TCHAR buff[_MAX_DIR];
		GetModuleFileName(GetModuleHandle(_T("tbapplication")), buff, _MAX_DIR);
		m_sTbApplicationPath = GetPath(buff, false);

		GetSystemDirectory(buff, _MAX_DIR);
		m_sSystemPath = buff;

	}

	const CString& GetLogFile() { return m_sFileName; }
	void AddExceptionDetails(DWORD dwExpCode)
	{
		CStringA FaultTx;
		FaultTx.Format("Exception code: %d - ", dwExpCode);
		switch(dwExpCode)
		{
			case EXCEPTION_ACCESS_VIOLATION          : FaultTx += "ACCESS VIOLATION"         ; break;
			case EXCEPTION_DATATYPE_MISALIGNMENT     : FaultTx += "DATATYPE MISALIGNMENT"    ; break;
			case EXCEPTION_BREAKPOINT                : FaultTx += "BREAKPOINT"               ; break;
			case EXCEPTION_SINGLE_STEP               : FaultTx += "SINGLE STEP"              ; break;
			case EXCEPTION_ARRAY_BOUNDS_EXCEEDED     : FaultTx += "ARRAY BOUNDS EXCEEDED"    ; break;
			case EXCEPTION_FLT_DENORMAL_OPERAND      : FaultTx += "FLT DENORMAL OPERAND"     ; break;
			case EXCEPTION_FLT_DIVIDE_BY_ZERO        : FaultTx += "FLT DIVIDE BY ZERO"       ; break;
			case EXCEPTION_FLT_INEXACT_RESULT        : FaultTx += "FLT INEXACT RESULT"       ; break;
			case EXCEPTION_FLT_INVALID_OPERATION     : FaultTx += "FLT INVALID OPERATION"    ; break;
			case EXCEPTION_FLT_OVERFLOW              : FaultTx += "FLT OVERFLOW"             ; break;
			case EXCEPTION_FLT_STACK_CHECK           : FaultTx += "FLT STACK CHECK"          ; break;
			case EXCEPTION_FLT_UNDERFLOW             : FaultTx += "FLT UNDERFLOW"            ; break;
			case EXCEPTION_INT_DIVIDE_BY_ZERO        : FaultTx += "INT DIVIDE BY ZERO"       ; break;
			case EXCEPTION_INT_OVERFLOW              : FaultTx += "INT OVERFLOW"             ; break;
			case EXCEPTION_PRIV_INSTRUCTION          : FaultTx += "PRIV INSTRUCTION"         ; break;
			case EXCEPTION_IN_PAGE_ERROR             : FaultTx += "IN PAGE ERROR"            ; break;
			case EXCEPTION_ILLEGAL_INSTRUCTION       : FaultTx += "ILLEGAL INSTRUCTION"      ; break;
			case EXCEPTION_NONCONTINUABLE_EXCEPTION  : FaultTx += "NONCONTINUABLE EXCEPTION" ; break;
			case EXCEPTION_STACK_OVERFLOW            : FaultTx += "STACK OVERFLOW"           ; break;
			case EXCEPTION_INVALID_DISPOSITION       : FaultTx += "INVALID DISPOSITION"      ; break;
			case EXCEPTION_GUARD_PAGE                : FaultTx += "GUARD PAGE"               ; break;
			default: FaultTx += "(unknown)";           break;
		}

		FaultTx += "\r\n";
		
		OnOutput(FaultTx);
	}

	void ShowAssertions()
	{
		if (AfxGetApplicationContext()->GetFailedAssertionsCount() <= 0)
			return;

		USES_CONVERSION;
		
		CStringA AssertionTx = "\r\nFailed Assertions:\r\n\r\n";
		OnOutput(AssertionTx);
		for (int i = 0; i < AfxGetApplicationContext()->GetFailedAssertionsCount(); i++)
		{
			AssertionTx = T2A(AfxGetApplicationContext()->GetFailedAssertion(i));
			AssertionTx += "\r\n\r\n";
			OnOutput(AssertionTx);
		}
		AfxGetApplicationContext()->ClearFailedAssertions();
	}

protected:
	//----------------------------------------------------------------------------
	virtual BOOL IsModuleToLoad(LPCSTR lpszModule) 
	{ 
		USES_CONVERSION;
		CString modulePath = GetPath(A2T(lpszModule), false);
		return _tcsicmp(m_sTbApplicationPath, modulePath) == 0 ||  _tcsicmp(m_sSystemPath, modulePath) == 0;
	}
	//----------------------------------------------------------------------------
	virtual void OnOutput(LPCSTR szText)
	{
		USES_CONVERSION;
		m_LogFile.WriteString(A2T(szText));
	}

	 virtual void OnDbgHelpErr(LPCSTR szFuncName, DWORD gle, DWORD64 addr)
	 {
		//non fa niente
	 }
 
};

class StackWalkerToMemory : public StackWalker
{
	CString			m_sTbApplicationPath;
	CString			m_sSystemPath;
	CStringArray	m_arNeededPdbs;
public:
	//----------------------------------------------------------------------------
	StackWalkerToMemory()
	{
		USES_CONVERSION;
		CStringA m_sSymbolPath = T2A(AfxGetPathFinder()->GetCustomPath() + "\\DebugSymbols");
		int nSize = m_sSymbolPath.GetLength() + 1;
		m_szSymPath = (LPSTR) malloc(nSize);
		memcpy(m_szSymPath, m_sSymbolPath, nSize);

		TCHAR buff[_MAX_DIR];
		GetModuleFileName(GetModuleHandle(_T("tbapplication")), buff, _MAX_DIR);
		m_sTbApplicationPath = GetPath(buff, false);
		GetSystemDirectory(buff, _MAX_DIR);
		m_sSystemPath = buff;
	}
	//----------------------------------------------------------------------------
	virtual void OnCallstackEntry(CallstackEntryType eType, CallstackEntry &entry)
	{
		USES_CONVERSION;
		if (IsModuleToLoad(entry.loadedImageName))
		{
			CString sPath = GetName(A2T(entry.loadedImageName)) + _T(".pdb");
			for (int i = 0; i < m_arNeededPdbs.GetCount(); i++)
				if (m_arNeededPdbs[i] == sPath)
					return;
			m_arNeededPdbs.Add(sPath);
		}
	}
	//----------------------------------------------------------------------------
	virtual BOOL IsModuleToLoad(LPCSTR lpszModule) 
	{ 
		USES_CONVERSION;
		CString modulePath = GetPath(A2T(lpszModule), false);
		return _tcsicmp(m_sTbApplicationPath, modulePath) == 0 ||  _tcsicmp(m_sSystemPath, modulePath) == 0;
	}
	const CStringArray& GetNeededPdbs() { return m_arNeededPdbs; }
};

//----------------------------------------------------------------------------
LONG WINAPI ExpFilter(EXCEPTION_POINTERS* pExp, DWORD dwExpCode)
{
#ifdef DEBUG
	return EXCEPTION_CONTINUE_SEARCH;
#else
	try
	{
		AfxGetThread()->BeginWaitCursor();
		CString strMessage;
			
		CFrameWnd* pWnd = NULL;
		CStatic* pLabel = NULL;
		if (!AfxIsInUnattendedMode())
		{
			pWnd = new CFrameWnd(); 
			pWnd->Create(NULL, NULL, WS_DLGFRAME);
			pLabel = new CStatic();
			CString strInfo = _TB("An unexpected error has occurred.\r\nDiagnostic information is being collected, please wait...");

			VERIFY(pLabel->Create(strInfo, WS_VISIBLE|WS_CHILD, CRect(0, 0, 400, 200), pWnd)); 
			pLabel->SetFont(AfxGetThemeManager()->GetFormFont());
			pWnd->SetWindowPos(&CWnd::wndTopMost, 0, 0, 400, 100, SWP_SHOWWINDOW | SWP_NOMOVE); 
			pWnd->SetWindowTextW(DIAGNOSTIC_TOOL_TITLE); 
			pWnd->CenterWindow();
			pWnd->UpdateWindow();
		}
		StackWalkerToMemory swm;
		swm.ShowCallstack(GetCurrentThread(), pExp->ContextRecord, NULL, NULL);
		
		for (int i = 0; i < swm.GetNeededPdbs().GetCount(); i++)
			AfxGetLoginManager()->DownloadPdb(swm.GetNeededPdbs()[i], strMessage);

		CString sLogFile;
		{//necessario per far uscire di scopo l'oggetto, che deve chiudere il file
			StackWalkerToFile sw;  
			sw.AddExceptionDetails(dwExpCode);
			sw.ShowCallstack(GetCurrentThread(), pExp->ContextRecord, NULL, NULL);

			sw.ShowAssertions();

			sLogFile = sw.GetLogFile();
		}
		
		if (pWnd)
			pWnd->DestroyWindow();
		if (pLabel)
			SAFE_DELETE(pLabel);

		if (
			AfxIsInUnattendedMode() || 
			IDYES == AfxMessageBox(
				cwsprintf(_TB("Diagnostic information has been saved to file:\r\n{0-%s}.\r\nDo you want to send it to the Microarea support?"), sLogFile),
				MB_YESNO | MB_ICONSTOP))
		{
			CString strSendError;
			UINT nIcon = 0;
			if (AfxGetLoginManager()->SendErrorFile(sLogFile, strSendError))
			{
				strMessage = _TB("Diagnostic information has been successfully sent to Microarea.\r\nThank you for your cooperation."); 
				nIcon = MB_ICONINFORMATION;
			}
			else
			{
				strMessage = _TB("Error sending diagnostic information to Microarea."); 
				strMessage.Append(_T("\r\n"));
				strMessage.Append(strSendError);
				nIcon = MB_ICONSTOP;
			}
			
			if (!AfxIsInUnattendedMode())
				AfxMessageBox(strMessage, MB_OK | nIcon);


		}
	}
	catch(...)
	{
		//almeno qui non si deve schiantare 
	}

	AfxGetThread()->EndWaitCursor();
		
	return EXCEPTION_EXECUTE_HANDLER;
	
#endif
}

//----------------------------------------------------------------------------
void DumpAssertions()
{
#ifdef DEBUG
	return;
#else
	if (!AfxGetApplicationContext()->DumpAssertionsIfNoCrash() || AfxGetApplicationContext()->GetFailedAssertionsCount() <= 0)
		return;

	// as this function is called inside a try ... catch block, any exception will be managed outside

	USES_CONVERSION;

	CString		sFileName;
	SYSTEMTIME	systime;
	::GetLocalTime (&systime);
	sFileName.Format(_T("%s\\%s-%u-%.2u-%.2u-%.2u-%.2u-%.2u.ASSERTIONS.log"),
				AfxGetPathFinder()->GetLogDataIOPath(TRUE, CPathFinder::ALL_COMPANIES),
				::GetComputerName(FALSE),
				systime.wYear, systime.wMonth, systime.wDay,
				systime.wHour, systime.wMinute, systime.wSecond
			);

	{ // declaring logFile variable inside the scope cause its closing at the scope end
		CStdioFile	logFile;
		logFile.Open(sFileName, CFile::modeWrite | CFile::modeCreate);
		logFile.WriteString(DIAGNOSTIC_TOOL_TITLE);
		logFile.WriteString(::GetComputerName(FALSE) + _T("\r\n"));
		
		if (AfxIsInUnattendedMode())
			logFile.WriteString(_T("Unattended\r\n"));
		else
			logFile.WriteString(_T("Attended\r\n"));
			
		CStringA AssertionTx = "\r\nFailed Assertions:\r\n\r\n";
		logFile.WriteString(A2T(AssertionTx));
		for (int i = 0; i < AfxGetApplicationContext()->GetFailedAssertionsCount(); i++)
		{
			AssertionTx = T2A(AfxGetApplicationContext()->GetFailedAssertion(i));
			AssertionTx += "\r\n\r\n";
			logFile.WriteString(A2T(AssertionTx));
		}
	}

	if (
		AfxIsInUnattendedMode() || 
		IDYES == AfxMessageBox(
			cwsprintf(_TB("Some errors have occurred during program execution.\r\nThese errors have been fixed and all the related activities have been performed without any problems.\r\nError messages have been saved to file:\r\n{0-%s}.\r\nDo you want to send it to the Microarea technical support? This will help us to improve the product."), sFileName),
			MB_YESNO | MB_ICONSTOP))
	{
		CString strMessage;
		CString strSendError;
		UINT nIcon = 0;
		if (AfxGetLoginManager()->SendErrorFile(sFileName, strSendError))
		{
			strMessage = _TB("Diagnostic information has been successfully sent to Microarea.\r\nThank you for your cooperation."); 
			nIcon = MB_ICONINFORMATION;
		}
		else
		{
			strMessage = _TB("Error sending diagnostic information to Microarea."); 
			strMessage.Append(_T("\r\n"));
			strMessage.Append(strSendError);
			nIcon = MB_ICONSTOP;
		}
			
		if (!AfxIsInUnattendedMode())
			AfxMessageBox(strMessage, MB_OK | nIcon);
	}

	AfxGetApplicationContext()->ClearFailedAssertions();

#endif
}