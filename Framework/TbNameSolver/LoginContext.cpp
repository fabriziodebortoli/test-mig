#include "StdAfx.h"
#include <math.h>
#include "ThreadContext.h"
#include "LoginContext.h"
#include "Pathfinder.h"
#include "Chars.h"
#include "FileSystemFunctions.h"
#include "JsonSerializer.h"

#define STATUS_BAR_TIMER 202

static const TCHAR szDocumentHistory[] = _T("DocumentHistory.bin");

///////////////////////////////////////////////////////////////////////////////
//	class CWorkersInterface:
//		classe base di interfaccia per la gestione dei workers nel gestionale
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CWorkersTableObj, CObject)

CWorkersTableObj::CWorkersTableObj()
:
m_bLoaded(FALSE)
{
}

//----------------------------------------------------------------------------
void CWorkersTableObj::RemoveAll()
{
	CWorker* pWorker = NULL;
	for (int i = 0; i <= m_arWorkers.GetUpperBound(); i++)
		if (pWorker = (CWorker*)m_arWorkers.GetAt(i))
			delete pWorker;
	m_arWorkers.RemoveAll();
}
//----------------------------------------------------------------------------
CWorker*CWorkersTableObj::GetWorker(long lWorkerID)
{
	CWorker* pWorker = NULL;
	for (int i = 0; i <= m_arWorkers.GetUpperBound(); i++)
	{
		pWorker = (CWorker*)m_arWorkers.GetAt(i);
		if (pWorker && pWorker->GetWorkerID() == lWorkerID)
			return pWorker;
	}
	return NULL;
}

//----------------------------------------------------------------------------
void CWorkersTableObj::Load()
{
	if (m_bLoaded) return;

	TB_LOCK_FOR_WRITE();
	LoadWorkers();
	m_bLoaded = TRUE;
}

///////////////////////////////////////////////////////////////////////////////
//	class CLoginContext:
///
///////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
CLoginContext::CLoginContext()
	:
	m_pCultureInfo(NULL),
	m_pWebServiceStateObjects(NULL),
	m_nOperationsDay(0),
	m_nOperationsMonth(0),
	m_nOperationsYear(0),
	m_nElapsedTimePrecision(1),
	m_pSecurityInterface(NULL),
	m_pOleDbMng(NULL),
	m_pSettingsTable(NULL),
	m_pNTLLookUpTableManager(NULL),
	m_pDataCachingUpdatesListener(NULL),
	m_pDataCachingSettings(NULL),
	m_pFormatsTable(NULL),
	m_pFontsTable(NULL),
	m_pDocumentObjectsTable(NULL),
	m_bIsDocked(FALSE),
	m_nOpenDialogs(0),
	m_nMenuWindowHandle(NULL),
	m_pSqlRecoveryManager(NULL),
	m_bValid(true),
	m_hTbDevMode(NULL),
	m_hTbDevNames(NULL),
	m_pLockManager(NULL),
	m_nWorkerId(0),
	m_pWorkersTable(NULL),
	m_bMluValid(TRUE),
	m_nMluValidCounter(0),
	m_pThemeManager(NULL),
	m_pDMSRepositoryManager(NULL),
	m_bAllowSetOpDateWithOpenDocs(FALSE)
{
}

//----------------------------------------------------------------------------
void CLoginContext::SetElapsedTimePrecision(int nPrec)
{
	TB_LOCK_FOR_WRITE();
	ASSERT(nPrec != 0);
	if (nPrec != 0)
		m_nElapsedTimePrecision = nPrec;

}

//----------------------------------------------------------------------------
CLoginContext::~CLoginContext(void)
{
	FreeObjects();

	SaveDocumentHistory();
}

//----------------------------------------------------------------------------
void CLoginContext::InitDocumentHistory(int nSize)
{
	TB_OBJECT_LOCK(&m_arDocumentLinks);
	m_nHistorySize = nSize;
	CString sPath = AfxGetPathFinder()->GetCustomUserApplicationDataPath(FALSE) + SLASH_CHAR + szDocumentHistory;
	if (!ExistFile(sPath))
		return;
	CStdioFile file(sPath, CFile::modeRead);
	CString s;
	while (file.ReadString(s))
	{
		m_arDocumentLinks.Add(s);
		if (m_arDocumentLinks.GetCount() == m_nHistorySize)
			break;
	}
}
//----------------------------------------------------------------------------
DataSourceGetterFunction CLoginContext::GetDataSourceGetter(const CString& sDataSourceName)
{
	DataSourceGetterFunction p = NULL;
	m_DataSourceGetters.Lookup(sDataSourceName, p);
	return p;
}
//----------------------------------------------------------------------------
void CLoginContext::RegisterDataSourceGetter(const CString& sDataSourceName, DataSourceGetterFunction pGetter)
{
	TB_OBJECT_LOCK(&m_DataSourceGetters);
#ifdef DEBUG
	if (GetDataSourceGetter(sDataSourceName))
	{
		ASSERT(FALSE);
		TRACE1("Datasourece already registered: %s", sDataSourceName);
		return;
	}
#endif
	m_DataSourceGetters[sDataSourceName] = pGetter;
}
//----------------------------------------------------------------------------
void CLoginContext::SaveDocumentHistory()
{
	TB_OBJECT_LOCK(&m_arDocumentLinks);
	if (m_arDocumentLinks.GetCount() == 0)
		return;

	CString sFolder = AfxGetPathFinder()->GetCustomUserApplicationDataPath();
	CreateDirectoryRecursive(sFolder);

	CString sPath = sFolder + SLASH_CHAR + szDocumentHistory;
	CStdioFile file(sPath, CFile::modeCreate | CFile::modeWrite);
	for (int i = 0; i < m_arDocumentLinks.GetCount(); i++)
		file.WriteString(m_arDocumentLinks.GetAt(i) + _T("\n"));
}

//----------------------------------------------------------------------------
void CLoginContext::CreateDirectoryRecursive(const CString& sFolder)
{
	if (!CreateDirectory(sFolder, NULL))//NULL: eredita la security dal folder padre
	{
		if (GetLastError() == ERROR_PATH_NOT_FOUND)
		{
			int idxOfSlash = sFolder.ReverseFind(SLASH_CHAR);
			if (idxOfSlash < 0)
			{
				AfxThrowFileException(ERROR_PATH_NOT_FOUND);
			}
			CreateDirectoryRecursive(sFolder.Left(idxOfSlash));
			CreateDirectory(sFolder, NULL);
		}
	}
}

//----------------------------------------------------------------------------
void CLoginContext::IncreaseOpenDialogs()
{
	TB_LOCK_FOR_WRITE();
	VERIFY(++m_nOpenDialogs > 0);
}
//----------------------------------------------------------------------------
void CLoginContext::DecreaseOpenDialogs()
{
	TB_LOCK_FOR_WRITE();
	VERIFY(--(m_nOpenDialogs) >= 0);
}

//----------------------------------------------------------------------------
void CLoginContext::AddDocumentThread(CTBWinThread* pThread)
{
	TB_OBJECT_LOCK(&m_DocumentThreads);
	m_DocumentThreads[pThread->m_nThreadID] = pThread;
}

//----------------------------------------------------------------------------
void CLoginContext::AddToAllHwndArray(HWND hWnd)
{
	TB_LOCK_FOR_WRITE();
	m_arAllHwndArray.Add(hWnd);
}

//----------------------------------------------------------------------------
void CLoginContext::RemoveFromAllHwndArray(HWND hWnd)
{
	TB_LOCK_FOR_WRITE();
	for (int i = m_arAllHwndArray.GetUpperBound(); i >= 0; i--)
	{
		if (m_arAllHwndArray.GetAt(i) == hWnd)
		{
			m_arAllHwndArray.RemoveAt(i);
			break;
		}
	}
}

//----------------------------------------------------------------------------
void CLoginContext::GetDocumentThreads(CArray<long, long>& arThreadIds)
{
	arThreadIds.RemoveAll();
	TB_OBJECT_LOCK(&m_DocumentThreads);
	POSITION pos = m_DocumentThreads.GetStartPosition();
	DWORD key = 0;
	CTBWinThread* pValue = NULL;
	while (pos)
	{
		m_DocumentThreads.GetNextAssoc(pos, key, pValue);
		arThreadIds.Add((long)pValue->m_nThreadID);
	}
}

//----------------------------------------------------------------------------
void CLoginContext::RemoveDocumentThread(CTBWinThread* pThread)
{
	TB_OBJECT_LOCK(&m_DocumentThreads);
	m_DocumentThreads.RemoveKey(pThread->m_nThreadID);
}

//----------------------------------------------------------------------------
void CLoginContext::SetOperationsDate(UWORD nDay, UWORD nMonth, UWORD nYear)
{
	if (GetOpenDocuments() > 0 && !m_bAllowSetOpDateWithOpenDocs)
		return;

	TB_LOCK_FOR_WRITE();
	m_nOperationsDay = nDay;
	m_nOperationsMonth = nMonth;
	m_nOperationsYear = nYear;
	m_bMluValid = TRUE;
}

//----------------------------------------------------------------------------
void CLoginContext::FreeObjects()
{
	//WARNING: order of deletion may be important, because some object
	//uses some of the other ones
	__super::FreeObjects();

	delete m_pSecurityInterface;
	delete m_pOleDbMng;

	delete m_pCultureInfo;
	delete m_pWebServiceStateObjects;

	delete m_pFormatsTable;
	delete m_pFontsTable;
	delete m_pDocumentObjectsTable;
	delete m_pSettingsTable;
	delete m_pNTLLookUpTableManager;
	delete m_pDataCachingUpdatesListener;
	delete m_pDataCachingSettings;
	delete m_pThemeManager;
	delete m_pSqlRecoveryManager;
	if (m_pLockManager)
		delete m_pLockManager;
	delete m_pWorkersTable;
	if (m_pDMSRepositoryManager)
		delete m_pDMSRepositoryManager;

	FreePrinterDevParams();
}

//-----------------------------------------------------------------------------                      
void CLoginContext::FreePrinterDevParams()
{
	TB_LOCK_FOR_WRITE();
	if (m_hTbDevMode)
	{
		GlobalFree(m_hTbDevMode);
		m_hTbDevMode = NULL;
	}
	if (m_hTbDevNames)
	{
		GlobalFree(m_hTbDevNames);
		m_hTbDevNames = NULL;
	}
}

//----------------------------------------------------------------------------
BOOL CLoginContext::IsValidToken(const CString &strToken)
{
	return strToken == m_strName;
}

//------------------------------------------------------------------------------
void CLoginContext::Lock()
{
	m_bValid = false;
}

//------------------------------------------------------------------------------
void CLoginContext::UnLock()
{
	m_bValid = true;
}

//------------------------------------------------------------------------------
BOOL CLoginContext::IsLocked()
{
	return !m_bValid;
}


//-----------------------------------------------------------------------------
void CLoginContext::SetEpsilonPrecision(const int& nTypePos, const int& nrOfDecimals)
{
	m_arDataDblEpsilonPrecisions[nTypePos] = pow(10.0, -nrOfDecimals);
}

//-----------------------------------------------------------------------------
const double CLoginContext::GetDataDblEpsilon() const
{
	return m_arDataDblEpsilonPrecisions[DATADBL_EPSILON_PRECISION_POS];
}

//-----------------------------------------------------------------------------
const double CLoginContext::GetDataMonEpsilon() const
{
	return m_arDataDblEpsilonPrecisions[DATAMON_EPSILON_PRECISION_POS];
}

//-----------------------------------------------------------------------------
const double CLoginContext::GetDataQtyEpsilon() const
{
	return m_arDataDblEpsilonPrecisions[DATAQTY_EPSILON_PRECISION_POS];
}

//-----------------------------------------------------------------------------
const double CLoginContext::GetDataPercEpsilon() const
{
	return m_arDataDblEpsilonPrecisions[DATAPERC_EPSILON_PRECISION_POS];
}
//----------------------------------------------------------------------------
CString CLoginContext::GetThreadInfos()
{
	m_arThreadInfo.Clear();

	AddThreadInfos(m_arThreadInfo);

	return m_arThreadInfo.ToXmlString();
}

//----------------------------------------------------------------------------
CString CLoginContext::GetThreadInfosJSON()
{
	m_arThreadInfo.Clear();

	AddThreadInfos(m_arThreadInfo);

	CJsonSerializer serializer;
	return m_arThreadInfo.ToJSON(serializer);
}

//----------------------------------------------------------------------------
CObject* CLoginContext::GetWorkersTable()
{
	if (m_pWorkersTable)
		((CWorkersTableObj*)m_pWorkersTable)->Load();

	return m_pWorkersTable;
}

//----------------------------------------------------------------------------
CString CLoginContext::GetCompanyInfo(const CString& sTagInfo) const
{
	for (int i = 0; i < m_arCompanyTagInfo.GetSize(); i++)
	{
		if (sTagInfo.CompareNoCase(m_arCompanyTagInfo[i]) == 0)
			return m_arCompanyInfo[i];
	}
	return L"";
}

void CLoginContext::AddCompanyInfo(const CString& sTagInfo, const CString& sInfo)
{
	m_arCompanyTagInfo.Add(sTagInfo);
	m_arCompanyInfo.Add(sInfo);
}

void CLoginContext::ResetCompanyInfo()
{
	m_arCompanyTagInfo.RemoveAll();
	m_arCompanyInfo.RemoveAll();
}

//----------------------------------------------------------------------------
int CLoginContext::GetCompanyInfoCount() const
{
	ASSERT(m_arCompanyInfo.GetCount() == m_arCompanyTagInfo.GetCount());
	return m_arCompanyInfo.GetCount();
}

CString	CLoginContext::GetCompanyTagInfo(int idx) const
{
	if (idx < 0 || idx > m_arCompanyTagInfo.GetUpperBound())
	{
		ASSERT(FALSE);
		return L"";
	}
	return m_arCompanyTagInfo[idx];
}


CString	CLoginContext::GetCompanyInfo(int idx) const
{
	if (idx < 0 || idx > m_arCompanyInfo.GetUpperBound())
	{
		ASSERT(FALSE);
		return L"";
	}
	return m_arCompanyInfo[idx];
}

///////////////////////////////////////////////////////////////////////////////

CLoginContext* AfxGetLoginContext(const CString& strName)
{
	return AfxGetApplicationContext()->GetLoginContext(strName);
}

//----------------------------------------------------------------------------
inline CLoginContext* AfxGetLoginContext()
{
	return AfxGetThreadContext()->GetLoginContext();
}
//----------------------------------------------------------------------------
CObject* AfxGetLoginContextObject(CRuntimeClass* pClass, BOOL bCreate /*= TRUE*/)
{
	return AfxGetLoginContext()->GetObject(pClass, bCreate);
}
//----------------------------------------------------------------------------
const CLoginInfos* AfxGetLoginInfos()
{
	CLoginContext* pContext = AfxGetLoginContext();
	return pContext ? pContext->GetLoginInfos() : NULL;
}
//-----------------------------------------------------------------------------
BOOL AFXAPI AfxIsRemoteInterface()
{
	return AfxGetApplicationContext()->IsRemoteInterface();
}
//-----------------------------------------------------------------------------
HWND AfxGetMenuWindowHandle()
{
	CLoginContext* pContext = AfxGetLoginContext();
	HWND hwnd = pContext ? pContext->GetMenuWindowHandle() : NULL;
	if (::IsWindow(hwnd))
		return hwnd;

	hwnd = AfxGetApplicationContext()->GetMenuWindowHandle();
	return ::IsWindow(hwnd) ? hwnd : NULL;
}
//-----------------------------------------------------------------------------
CWnd* AfxGetMenuWindow()
{
	return AfxGetThreadContext()->GetMenuWindow();
}
//-----------------------------------------------------------------------------
void CALLBACK OnTimer(
	HWND hWnd,      // handle of CWnd that called SetTimer
	UINT nMsg,      // WM_TIMER
	UINT nIDEvent,   // timer identification
	DWORD dwTime    // system time
	)
{
	if (nIDEvent == STATUS_BAR_TIMER)
	{
		PostMessage(hWnd, UM_CLEAR_STATUS_BAR, NULL, NULL);
		::KillTimer(hWnd, nIDEvent);
	}
}

struct CGarbageData
{
	TCHAR* m_pData1;
	TCHAR* m_pData2;

	CGarbageData(TCHAR* pData1, TCHAR* pData2)
		: m_pData1(pData1), m_pData2(pData2)
	{
	}
	~CGarbageData()
	{
		delete m_pData1;
		delete m_pData2;
	}
};

//-----------------------------------------------------------------------------
void CALLBACK GarbageCollect
(
HWND hwnd,
UINT uMsg,
ULONG_PTR dwData,
LRESULT lResult
)
{
	ASSERT(uMsg == UM_SET_STATUS_BAR_TEXT || uMsg == UM_SET_MENU_WINDOW_TEXT || uMsg == UM_SET_USER_PANEL_TEXT);
	delete (CGarbageData*)dwData;
	if (uMsg == UM_SET_STATUS_BAR_TEXT)
		::SetTimer(hwnd, STATUS_BAR_TIMER, 2000, &OnTimer);

}


//-----------------------------------------------------------------------------
void AfxSetStatusBarText(const CString& strText)
{
	//non visualizzo alcuni messaggi se segnalati troppo di frequente
	if (AfxGetThreadContext()->m_IntervalChecker.SkipOperations())
		return;

	HWND hwnd = AfxGetMenuWindowHandle();
	if (!hwnd) return;

	UINT n = strText.GetLength() + 1;
	TCHAR* pMsg = new TCHAR[n];
	_tcscpy_s(pMsg, n, strText);

	CGarbageData *pData = new CGarbageData(pMsg, NULL);
	if (!SendMessageCallback(hwnd, UM_SET_STATUS_BAR_TEXT, (WPARAM)pMsg, NULL, &GarbageCollect, (ULONG_PTR)pData))
		delete pData;
}

//-----------------------------------------------------------------------------
void InternalSetUserPaneText(int nRetry, CString strText, CString strTooltip)
{
	CLoginContext* pContext = AfxGetLoginContext();
	HWND hwnd = pContext ? pContext->GetMenuWindowHandle() : NULL;
	if (!hwnd)
	{
		//se non ho ancora un handle di finestra, provo a ripostarmi il messaggio per un ulteriore tentativo
		if (nRetry > 0)
		{
			Sleep(1);
			AfxInvokeAsyncThreadGlobalProcedure<int, CString, CString>(GetCurrentThreadId(), &InternalSetUserPaneText, --nRetry, strText, strTooltip);
		}
		return;
	}
	UINT n = strText.GetLength() + 1;
	TCHAR* pMsg1 = new TCHAR[n];
	_tcscpy_s(pMsg1, n, strText);

	n = strTooltip.GetLength() + 1;
	TCHAR* pMsg2 = new TCHAR[n];
	_tcscpy_s(pMsg2, n, strTooltip);

	CGarbageData *pData = new CGarbageData(pMsg1, pMsg2);
	if (!SendMessageCallback(hwnd, UM_SET_USER_PANEL_TEXT, (WPARAM)pMsg1, (LPARAM)pMsg2, &GarbageCollect, (ULONG_PTR)pData))
		delete pData;
}

//-----------------------------------------------------------------------------
void AfxSetUserPaneText(const CString& strText, const CString& strTooltip)
{
	//se non ho handle di menu nell'application context, non ce l'avro mai neanche nel login context
	if (!AfxGetApplicationContext()->GetMenuWindowHandle())
		return;

	InternalSetUserPaneText(50, strText, strTooltip);//50 tentativi prima di fallire per evitare blocchi
}

//-----------------------------------------------------------------------------
void InternalSetMenuWindowTitle(int nRetry, CString strText)
{
	CLoginContext* pContext = AfxGetLoginContext();
	HWND hwnd = pContext ? pContext->GetMenuWindowHandle() : NULL;
	if (!hwnd)
	{
		//se non ho ancora un handle di finestra, provo a ripostarmi il messaggio per un ulteriore tentativo
		if (nRetry > 0)
		{
			Sleep(1);
			AfxInvokeAsyncThreadGlobalProcedure<int, CString>(GetCurrentThreadId(), &InternalSetMenuWindowTitle, --nRetry, strText);
		}
		return;
	}

	UINT n = strText.GetLength() + 1;
	TCHAR* pMsg = new TCHAR[n];
	_tcscpy_s(pMsg, n, strText);
	CGarbageData *pData = new CGarbageData(pMsg, NULL);
	if (!SendMessageCallback(hwnd, UM_SET_MENU_WINDOW_TEXT, (WPARAM)pMsg, NULL, &GarbageCollect, (ULONG_PTR)pData))
		delete pData;
}

//-----------------------------------------------------------------------------
void AfxSetMenuWindowTitle(const CString& strText)
{
	//se non ho handle di menu nell'application context, non ce l'avro mai neanche nel login context
	if (!AfxGetApplicationContext()->GetMenuWindowHandle())
		return;

	InternalSetMenuWindowTitle(50, strText);//50 tentativi prima di fallire per evitare blocchi
}

//-----------------------------------------------------------------------------
void AfxSetStatusBarText(UINT nID)
{
	CString s;
	s.LoadString(nID);

	return AfxSetStatusBarText(s);
}

//-----------------------------------------------------------------------------
void AfxClearStatusBar()
{
	HWND hwnd = AfxGetMenuWindowHandle();
	if (!hwnd) return;

	PostMessage(hwnd, UM_CLEAR_STATUS_BAR, NULL, NULL);
}
