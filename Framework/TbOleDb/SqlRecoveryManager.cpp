#include "stdafx.h"

#include <afxtempl.h>
#include <atldbcli.h>
#include <atldbsch.h>
#include <atlconv.h>
#include <Iphlpapi.h>

#include <TbNameSolver\LoginContext.h>

#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>

#include <TbGenlib\TbCommandInterface.h>
#include <TbGenlib\Parsobj.h>
#include <TbGenlib\BaseApp.h>

#include "SqlRecoveryManager.h"
#include "SqlRecoveryManager.hjson" //JSON AUTOMATIC UPDATE
#include "SqlObject.h"
#include "SqlConnect.h"
#include "oledbmng.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

// default values
static const BOOL nDefaultEnabled			= TRUE;
static const int  nDefaultRetries			= 3;
static const long nDefaultRetriesIntervalSec= 5;

// utility members
static const TCHAR	szLogFileName[]		= _T("LostDbConnection-%d-%d-%d.xml");
static const TCHAR	szXSLLogFileName[]	= _T("XSLLostDbConnection.xsl");

///////////////////////////////////////////////////////////////////////////////
// 							Global Function
//////////////////////////////////////////////////////////////////////////////
TB_EXPORT SqlRecoveryManager* AFXAPI AfxGetSqlRecoveryManager () 
{
	return (SqlRecoveryManager*) AfxGetLoginContext()->GetSqlRecoveryManager();
}

///////////////////////////////////////////////////////////////////////////////
// 						class to manage user messages
//////////////////////////////////////////////////////////////////////////////

//=============================================================================
class SqlRecoveryManagerUM
{
public:
	static CString ParamsHeader					();
	static CString ParamsRetriesError			(const int& nValue);
	static CString ParamsRetriesIntervalError	(const int& nValue);
	static CString CannotCreateLog				();
	static CString ConnectionLostError			();
	static CString ConnectionsRecoveryStart		(const int& nPhase);
	static CString ConnectionsRecoveryRetry		(const int& nPhase, const CString& sConnName, const int& nRetry);
	static CString ConnectionsRecoverySuccess	(const int& nPhase);
	static CString ConnectionsRecoveryFailed	(const int& nPhase);
	static CString ConnectionNoCloseObjects		(const int& nPhase, const CString& sConnName);
	static CString ConnectionNoReconnect		(const int& nPhase, const CString& sConnName);
	static CString ConnectionNoInitConnection	(const int& nPhase, const CString& sConnName);
	static CString ConnectionNoRecoverySession	(const int& nPhase, const CString& sConnName);
	static CString ResultsDetailsSuccess		();
	static CString ResultsDetailsSuccessNoSS	();
	static CString ResultsDetailsSuccessNoSP	();
	static CString ResultsDetailsFailed			();
	static CString ResultsDetailsAborted		();
	static CString ResultsSuccess				();
	static CString ResultsWarnings				();
	static CString ResultsFailed				();
	static CString ResultsAborted				();
};

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ParamsHeader ()
{
	return _TB("Recovery System Parameters:");
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ParamsRetriesError (const int& nValue)
{
	return cwsprintf(
						_TB("- Recovery Retries parameter is less than zero: %d. \n  Recovery System will set the to default value %d. "), 
						nValue, 
						nDefaultRetries
					);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ParamsRetriesIntervalError (const int& nValue)
{
	return cwsprintf(
						_TB("- Recovery System Retries Interval parameter is less than zero seconds: %d.\n  Recovery System Retries Interval will be not managed."), 
						nValue
					);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::CannotCreateLog ()
{
	return _TB("Recovery System cannot create log file due to an error writing into Custom../Log directories.\n Log file will not be managed and all Recovery System activity will be displayed on monitor.\n Please read carefully all Recovery System diagnostic messages.");
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ConnectionLostError ()
{
	return cwsprintf
			(
				_TB("Application %s loose the database connection. Recovery System has started and catched the error. Please, see details on original database event."), 
				AfxGetLoginManager()->GetMasterProductBrandedName()
			);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ConnectionsRecoveryStart (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System will try to recovery all database connections! Please wait..."), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ConnectionsRecoveryRetry (const int& nPhase, const CString& sConnName, const int& nRetry)
{
	return cwsprintf(_TB("Phase %d: Recovering lost database connection %s, (retry #%d)... please wait..."), nPhase, sConnName, nRetry);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ConnectionsRecoverySuccess (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System recoved database connections successfully!"), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ConnectionsRecoveryFailed (const int& nPhase)
{
	return cwsprintf(_TB("Phase %d: Recovery System was not able to recovery database connections! Please Logoff."), nPhase);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ConnectionNoCloseObjects (const int& nPhase, const CString& sConnName)
{
	return cwsprintf(_TB("Phase %d: Recovery System was not able to close OleDb sessions for connection %s! Connection Recovery Failed."), nPhase, sConnName);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ConnectionNoReconnect (const int& nPhase, const CString& sConnName)
{
	return cwsprintf(_TB("Phase %d: Recovery System was not able to reconnect database for connection %s! Connection Recovery Failed."), nPhase, sConnName);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ConnectionNoInitConnection (const int& nPhase, const CString& sConnName)
{
	return cwsprintf(_TB("Phase %d: Recovery System was not able to initialize connection for connection %s! Connection Recovery Failed."), nPhase, sConnName);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ConnectionNoRecoverySession(const int& nPhase, const CString& sConnName)
{
	return cwsprintf(_TB("Phase %d: Recovery System was not able to recovery some sessions for connection %s! Connection Recovery Failed."), nPhase, sConnName);
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ResultsDetailsSuccess()
{
	return _TB("Recovery System has completed Recovery Activities with success."); 
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ResultsDetailsSuccessNoSS()
{
	return _TB("Recovery System has completed Recovery Activities with success but NO snapshot was saved. You have to open manually all your previous activities."); 
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ResultsDetailsSuccessNoSP()
{
	return _TB("Recovery System has completed Recovery Activities with success but some snapshots cannot be played. You have to open manually your previous activities."); 
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ResultsDetailsFailed()
{
	return _TB("Recovery System has encountered fatal errors, please see Today Log. Application cannot perform any requests, please logoff and check your server database connection."); 
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ResultsDetailsAborted()
{
	return cwsprintf(_TB("Recovery Activity has been aborted by the User! No further activity can be performed. Application cannot be used, please logoff !!")); 
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ResultsSuccess()
{
	return cwsprintf(_TB("Recovery Activity successfully performed!\nApplication is ready for your requests."));
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ResultsWarnings()
{
	return cwsprintf(_TB("Recovery Activity performed with some warnings!\nPlease view details."));
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ResultsFailed()
{
	return cwsprintf(_TB("Recovery Activity has failed with errors!\nPlease view details and logoff application"));
}

//-----------------------------------------------------------------------------
/*static*/ CString SqlRecoveryManagerUM::ResultsAborted()
{
	return cwsprintf(_TB("Recovery Activity has been aborted by the user!"));
}


///////////////////////////////////////////////////////////////////////////////
// 			CRecoveryManagerSettings implementation
//////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CRecoveryManagerSettings, CObject)

//------------------------------------------------------------------------------
CRecoveryManagerSettings::CRecoveryManagerSettings ()
	:
	m_bEnabled				(nDefaultEnabled),
	m_nRetries				(nDefaultRetries),
	m_lRetriesIntervalMms	(nDefaultRetriesIntervalSec)
{
	InitDefaultSettings ();
}

//-----------------------------------------------------------------------------
const int& CRecoveryManagerSettings::GetRetries () const
{
	return m_nRetries;
}

//-----------------------------------------------------------------------------
const long&	CRecoveryManagerSettings::GetRetriesIntervalMms() const
{
	return m_lRetriesIntervalMms;
}

//-----------------------------------------------------------------------------
const long CRecoveryManagerSettings::IsEnabled () const
{
	return m_bEnabled;
}

//------------------------------------------------------------------------------
void CRecoveryManagerSettings::InitDefaultSettings ()
{
	CDiagnostic* pDiagnostic = AfxGetDiagnostic();
	DataObj* pDataObj = NULL;
	pDataObj = AfxGetSettingValue 
			(
				snsTbOleDb, 
				szRecoverySystem, 
				szRecoverySystemEnable, 
				DataBool(nDefaultEnabled),
				szTbDefaultSettingFileName
			);
	m_bEnabled = pDataObj ? *((DataBool*) pDataObj) : nDefaultEnabled;

	// if cache is not enabled I skip all other settings read.
	if (!m_bEnabled)
		return;

	BOOL bHeaderMessage = FALSE;

	pDataObj = AfxGetSettingValue 
			(
				snsTbOleDb, 
				szRecoverySystem, 
				szRecoverySystemRetries, 
				DataInt(nDefaultRetries),
				szTbDefaultSettingFileName
			);

	int nRetries = pDataObj ? *((DataInt*) pDataObj) : nDefaultRetries;
	if (nRetries < 0)
	{
		ASSERT (FALSE);
		if (pDiagnostic)
		{
			if (!bHeaderMessage)
			{
				pDiagnostic->Add (SqlRecoveryManagerUM::ParamsHeader(), CDiagnostic::Info);
				bHeaderMessage = TRUE;
			}

			pDiagnostic->Add (SqlRecoveryManagerUM::ParamsRetriesError (nRetries), CDiagnostic::Info);
		}

		TRACE	(SqlRecoveryManagerUM::ParamsRetriesError (nRetries));
		nRetries = nDefaultRetries;
	}

	m_nRetries = nRetries;

	// interval in not meaningful if no retries.
	if (m_nRetries > 0)
	{
		pDataObj = AfxGetSettingValue 
				(
					snsTbOleDb, 
					szRecoverySystem, 
					szRecoverySystemRetriesInterval, 
					DataInt(nDefaultRetriesIntervalSec),
					szTbDefaultSettingFileName
				);
		
		int lRetriesElapsedIntervalSec = pDataObj ? *((DataInt*) pDataObj) : nDefaultRetriesIntervalSec;
		if (lRetriesElapsedIntervalSec < 0)
		{
			ASSERT (FALSE);
			if (pDiagnostic)
			{
				if (!bHeaderMessage)
				{
					pDiagnostic->Add (SqlRecoveryManagerUM::ParamsHeader(), CDiagnostic::Info);
					bHeaderMessage = TRUE;
				}

				pDiagnostic->Add (SqlRecoveryManagerUM::ParamsRetriesIntervalError (lRetriesElapsedIntervalSec), CDiagnostic::Info);

			}
			TRACE	(SqlRecoveryManagerUM::ParamsRetriesIntervalError (lRetriesElapsedIntervalSec));
			lRetriesElapsedIntervalSec = 0;
		}

		// Parameter is managed in seconds
		m_lRetriesIntervalMms = lRetriesElapsedIntervalSec * 1000;
	}
}

/////////////////////////////////////////////////////////////////////////////
//						class SqlRecoveryManagerDiagnostic
/////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNAMIC (SqlRecoveryManagerDiagnostic, CDiagnosticManagerWriter)

//-----------------------------------------------------------------------------
SqlRecoveryManagerDiagnostic::SqlRecoveryManagerDiagnostic ()
	:
	CDiagnosticManagerWriter(),
	m_bSessionStarted		(FALSE),
	m_bSnapShotSaved		(FALSE),
	m_bSnapShotPlayed		(FALSE),
	m_nCurrPhase			(0)
{	
}

//-----------------------------------------------------------------------------
CString SqlRecoveryManagerDiagnostic::GetNetworkAdapters()
{
	// Allocate information for up to 16 NICs
	IP_ADAPTER_INFO adapterInfo[16];       

	DWORD dwBufLen = sizeof(adapterInfo);

	DWORD dwStatus = GetAdaptersInfo(adapterInfo, &dwBufLen);
	if (dwStatus != ERROR_SUCCESS)
	{
		ASSERT_TRACE1(FALSE,"SqlRecoveryManagerDiagnostic::GetNetworkAdapters failed with status %d", dwStatus); 
		return _T("");
	}

	PIP_ADAPTER_INFO pAdapterInfo = adapterInfo;
	CString sAdapters;
	CString sName;
	CString sIpAddress;
	do 
	{
		sName = CString(pAdapterInfo->AdapterName);
		if (sName.IsEmpty())
		{
			pAdapterInfo = pAdapterInfo->Next;
			continue;
		}

		sAdapters += sName  + _T(": ");
		sIpAddress = CString(pAdapterInfo->IpAddressList.IpAddress.String);
		sAdapters += sIpAddress + _T(", ");
		pAdapterInfo = pAdapterInfo->Next;
	}
	while (pAdapterInfo);	
	
	return sAdapters;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDiagnostic::AddError (const CString& sMessage, Array* pExtInfos /*NULL*/)
{
	AddMessage (sMessage, CDiagnosticManagerWriter::Error, pExtInfos);
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDiagnostic::AddWarning (const CString& sMessage, Array* pExtInfos /*NULL*/)
{
	AddMessage (sMessage, CDiagnosticManagerWriter::Warning, pExtInfos);
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDiagnostic::AddInfo (const CString& sMessage, Array* pExtInfos /*NULL*/)
{
	AddMessage (sMessage, CDiagnosticManagerWriter::Information, pExtInfos);
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDiagnostic::AddMessage (const CString& sMessage, const MessageType& eType, Array* pExtInfos /*NULL*/)
{
	__super::AddMessage(eType, sMessage, pExtInfos);

	// log file is saved immediatly. If the log cannot be saved on server, 
	// it is saved on client workstation
	if (!Save ())
		Save (GetLogFileName(TRUE));

}
//-----------------------------------------------------------------------------
CString SqlRecoveryManagerDiagnostic::GetLogFileName(BOOL bOnClient)
{
	DataDate today (TodayDate());

	CString sUser = AfxGetLoginInfos() ? AfxGetLoginInfos()->m_strUserName : _T("");
	CString sCompany = AfxGetLoginInfos() ? AfxGetLoginInfos()->m_strCompanyName: _T("");

	return  AfxGetPathFinder()->GetUserLogPath(sUser, sCompany, TRUE, bOnClient) 
			+ SLASH_CHAR 
			+ cwsprintf(szLogFileName, today.Year(), today.Month(), today.Day());
}

#pragma warning (disable : 4996)	
//-----------------------------------------------------------------------------
BOOL SqlRecoveryManagerDiagnostic::StartSession()
{
	CString sXSLFile = AfxGetPathFinder()->GetModuleXmlPath(snsTbGes, CPathFinder::STANDARD) 
							+ SLASH_CHAR + 
							szXSLLogFileName;
	
	if (!ExistFile(sXSLFile))
		sXSLFile.Empty();

	m_bSnapShotSaved	= FALSE;
	m_bSnapShotPlayed	= FALSE;

	// I try first on server and if not possible on client
	m_bSessionStarted = Open (GetLogFileName(), sXSLFile) || Open (GetLogFileName(TRUE), sXSLFile);

	if (m_bSessionStarted)
	{
		// operating system version
		CString sWinOS;

		OSVERSIONINFO vi;
		vi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
		
		if (GetVersionEx (&vi))
			sWinOS = cwsprintf (_TB("Major %d Minor %d Build %d PlatformID %d Additional Info %s"), vi.dwMajorVersion, vi.dwMinorVersion, vi.dwBuildNumber, vi.dwPlatformId, vi.szCSDVersion );

		Array extInfo;
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("dbDisconnectTitle"),			_TB("A Database Disconnection has been detected!")));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("sourceTitle"),				_TB("Disconnection Source Information and Diagnostic Detail")));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("Client Name"),				GetComputerName()));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("Client Network Adapters"),	GetNetworkAdapters()));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("Windows Version"),			sWinOS));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("Logged User"),				AfxGetLoginInfos()->m_strUserName));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("Logged Company"),				AfxGetLoginInfos()->m_strCompanyName));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("Database Type"),				AfxGetLoginInfos()->m_strProviderDescription));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("Database Server"),			AfxGetLoginInfos()->m_strDBServer));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("Database User"),				AfxGetLoginInfos()->m_strDBUser));
		extInfo.Add	(new CDiagnosticManagerWriterExtInfo(_TB("Original Error"),				m_sOriginalError));
		AddInfo(SqlRecoveryManagerUM::ConnectionLostError(), &extInfo);
	}
	else
		AfxMessageBox (SqlRecoveryManagerUM::CannotCreateLog(), MB_ICONINFORMATION | MB_OK);

	return m_bSessionStarted;
}
#pragma warning(default:4996)

//-----------------------------------------------------------------------------
CString SqlRecoveryManagerDiagnostic::GetOriginalError () const
{
	return m_sOriginalError;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDiagnostic::SetOriginalError (const CString& sError)
{
	m_sOriginalError = sError;
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManagerDiagnostic::IsSnapshotsSaved	()
{
	return m_bSnapShotSaved;
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManagerDiagnostic::IsSnapshotsPlayed ()
{
	return m_bSnapShotPlayed;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDiagnostic::StartPhase (const int& nPhase)
{
	m_nCurrPhase = nPhase;
}

//-----------------------------------------------------------------------------
int SqlRecoveryManagerDiagnostic::GetPhase () const
{
	return m_nCurrPhase;
}


/////////////////////////////////////////////////////////////////////////////
//						class SqlRecoveryManagerState
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
SqlRecoveryManagerState::SqlRecoveryManagerState ()
	:
	m_eState				(SqlRecoveryManagerState::OnIdle),
	m_bApplicationFreezed	(FALSE)
{	
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerState::Init ()
{	
	m_sInitConn.Empty();
	m_sInitConnDisplayable.Empty();
	m_Diagnostic.StartSession();
	m_Diagnostic.StartPhase(0);
}

//-----------------------------------------------------------------------------
CString SqlRecoveryManagerState::GetOriginalError () const
{
	return m_Diagnostic.GetOriginalError();
}

//-----------------------------------------------------------------------------
SqlRecoveryManagerState::State SqlRecoveryManagerState::GetState () const
{
	return m_eState;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerState::SetState (State eState)
{
	m_eState = eState;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerState::StartPhase (const int& nPhase)
{
	m_Diagnostic.StartPhase(nPhase);
}

//-----------------------------------------------------------------------------
int SqlRecoveryManagerState::GetPhase () const
{
	return m_Diagnostic.GetPhase();
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerState::FreezeApplication	()
{
	m_bApplicationFreezed = TRUE;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerState::UnFreezeApplication()
{
	m_bApplicationFreezed = FALSE;
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManagerState::IsApplicationFrozen() const
{
	return  m_bApplicationFreezed;
}

///////////////////////////////////////////////////////////////////////////////
//						class SqlRecoveryManagerDialog
//				feedback to the user about working recovery system
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SqlRecoveryManagerDialog : public CParsedDialog
{
	DECLARE_DYNAMIC(SqlRecoveryManagerDialog)

private:
	CProgressCtrl	m_ProgressBar;
	Scheduler		m_Scheduler;
	BOOL			m_bAborted;

public:
	SqlRecoveryManagerDialog ();

public:
	void SetUserMessage			(LPCTSTR szMessage);
	void SetMaxProgressBar		(const int& nRetries);
	void SetProgressBarStep		(const int& nrRetry);
	int GetProgressBarCurrStep	();

	BOOL IsAborted			();

private:
	virtual void PostNcDestroy	();

protected:
	virtual BOOL OnInitDialog	();
	virtual void OnCancel		();

	DECLARE_MESSAGE_MAP ();
};

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (SqlRecoveryManagerDialog, CParsedDialog)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SqlRecoveryManagerDialog, CParsedDialog)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
SqlRecoveryManagerDialog::SqlRecoveryManagerDialog ()
	:
	CParsedDialog(IDD_RCVRYMNG, AfxGetLoginContext()->m_pMainWnd),
	m_bAborted   (FALSE)
{
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDialog::PostNcDestroy() 
{
	delete this;
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManagerDialog::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_ProgressBar.SubclassDlgItem (IDC_RCVRYMNG_PROGRESS, this);
	m_Scheduler.Attach	(m_hWnd);

	// Get this window's area
    CRect rect;
    GetWindowRect(&rect);

	// I add 20% in order to move window under the others that could be displayed
    int yTop = rect.top + (rect.top * 20 / 100);

    // Move the window to the correct coordinates with SetWindowPos()
	SetWindowPos(NULL, rect.left, yTop, -1, -1, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManagerDialog::IsAborted()
{
	CFrameWnd* pFrame = GetParentFrame();
	if (pFrame)
	{
		//pFrame->SetTitle (_TB("Recovery System: Recovering Database Connection..."));
		pFrame->Invalidate();
		pFrame->UpdateWindow();
	}
	UpdateWindow();
	m_Scheduler.CheckMessage();
	return m_bAborted;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDialog::OnCancel ()
{
	if (AfxMessageBox(_TB("Do you want to cancel database recovery operation?"), MB_YESNO) == IDNO)
		return;

	m_Scheduler.Terminate	();
	m_Scheduler.Detach		();

	m_bAborted = TRUE;

	ShowWindow (SW_HIDE);
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDialog::SetMaxProgressBar (const int& nRetries)
{
	m_ProgressBar.SetRange(0, nRetries);
	UpdateWindow();
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDialog::SetProgressBarStep (const int& nrRetry)
{
	m_ProgressBar.SetPos(nrRetry);
	UpdateWindow();
}

//-----------------------------------------------------------------------------
int SqlRecoveryManagerDialog::GetProgressBarCurrStep ()
{
	return m_ProgressBar.GetPos();
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerDialog::SetUserMessage (LPCTSTR szMessage)
{
	CWnd* pWnd = GetDlgItem (IDC_RCVRYMNG_RETRIES);
	if (pWnd)
	{
		pWnd->SetWindowText (szMessage);
		pWnd->UpdateWindow	();
		UpdateWindow ();
	}
}

///////////////////////////////////////////////////////////////////////////////
//						class SqlRecoveryManagerAskDialog
//				request made to the user about recovery system behaviour
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SqlRecoveryManagerAskDialog : public CParsedDialog
{
	friend class SqlRecoveryManager;
	DECLARE_DYNAMIC(SqlRecoveryManagerAskDialog)

public:
	SqlRecoveryManagerAskDialog ();

private:
	CString		m_sExplanation;
	CString		m_sQuestion;
	CString		m_sBtnOkText;
	CString		m_sBtnCancelText;

public:
	void SetExplanation			(LPCTSTR szExplanation);
	void SetQuestion			(LPCTSTR szQuestion);

protected:
	virtual BOOL OnInitDialog	();

	DECLARE_MESSAGE_MAP ();
};

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (SqlRecoveryManagerAskDialog, CParsedDialog)
//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SqlRecoveryManagerAskDialog, CParsedDialog)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
SqlRecoveryManagerAskDialog::SqlRecoveryManagerAskDialog ()
	:
	CParsedDialog	(IDD_RCVRYMNGASK, AfxGetThread()->m_pMainWnd)
{
	m_sBtnOkText		= _TB("Yes");
	m_sBtnCancelText	= _TB("No");
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManagerAskDialog::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	CenterWindow();

	CWnd* pWnd = GetDlgItem (IDC_RCVRYMNG_EXPLANATION);
	if (pWnd)
	{
		pWnd->SetWindowText (m_sExplanation);
		pWnd->UpdateWindow	();
	}

	pWnd = GetDlgItem (IDC_RCVRYMNG_QUESTION);
	if (pWnd)
	{
		pWnd->SetWindowText (m_sQuestion);
		pWnd->UpdateWindow	();
	}

	Invalidate	();
	UpdateWindow();

	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerAskDialog::SetExplanation(LPCTSTR szExplanation)
{
	m_sExplanation = szExplanation;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerAskDialog::SetQuestion(LPCTSTR szQuestion)
{
	m_sQuestion = szQuestion;
}

///////////////////////////////////////////////////////////////////////////////
//					class SqlRecoveryManagerResultsDialog
//							shows results to the user
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT SqlRecoveryManagerResultsDialog : public CParsedDialog
{
	friend class SqlRecoveryManager;
	DECLARE_DYNAMIC(SqlRecoveryManagerResultsDialog)

public:
	SqlRecoveryManagerResultsDialog ();

private:
	CString		m_sLogFile;
	CString		m_sResults;
	CString		m_sDetails;
	CString		m_sOriginalError;
	BOOL		m_bShowDetails;
	DWORD		m_wOriginalLength;
	DWORD		m_wOriginalHeight;
	DWORD		m_wCompactHeight;

public:
	CString GetResults		() const;
	CString GetDetails		() const;

	void SetResults		(LPCTSTR szResults);
	void SetDetails		(LPCTSTR szDetails);
	void SetOriginalError(LPCTSTR szError);
	void SetLogFile		(LPCTSTR szLogFile);

protected:
	virtual BOOL OnInitDialog	();

	afx_msg	void OnViewLog		();
	afx_msg	void OnViewDetails	();
	DECLARE_MESSAGE_MAP ();
};

//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC (SqlRecoveryManagerResultsDialog, CParsedDialog)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(SqlRecoveryManagerResultsDialog, CParsedDialog)
	ON_BN_CLICKED	(IDC_RCVRYMNGRESULT_VIEWLOG, OnViewLog)
	ON_BN_CLICKED	(IDC_RCVRYMNGRESULT_DETAILS, OnViewDetails)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
SqlRecoveryManagerResultsDialog::SqlRecoveryManagerResultsDialog ()
	:
	m_bShowDetails	(TRUE),
	m_wOriginalLength(0),
	m_wOriginalHeight(0),
	m_wCompactHeight (0),
	CParsedDialog (IDD_RCVRYMNGRESULT, AfxGetThread()->m_pMainWnd)
{
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManagerResultsDialog::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	CenterWindow();

	CWnd* pWnd = GetDlgItem (IDC_RCVRYMNGRESULT_EXPLANATION);
	if (pWnd)
	{
		pWnd->SetWindowText (m_sResults);
		pWnd->UpdateWindow	();
	}

	pWnd = GetDlgItem (IDC_RCVRYMNGRESULT_RECDETAILS);
	if (pWnd)
	{
		pWnd->SetWindowText(m_sDetails);
		pWnd->UpdateWindow	();
	}

	pWnd = GetDlgItem (IDC_RCVRYMNGRESULT_ORIGINALERROR);
	if (pWnd)
	{
		pWnd->SetWindowText(m_sOriginalError);
		pWnd->UpdateWindow	();
	}

	pWnd = GetDlgItem (IDC_RCVRYMNGRESULT_VIEWLOG);
	if (pWnd)
	{
		CString sCaption;
		pWnd->EnableWindow(!m_sLogFile.IsEmpty());
		if (m_sLogFile.IsEmpty())
			sCaption = _TB("No Log");
		else
			sCaption = _TB("Today Log");;

		pWnd->SetWindowText(sCaption);
		pWnd->UpdateWindow	();
	}

	Invalidate	();
	UpdateWindow();

	// I save the original size
	CRect rect;
	GetWindowRect(rect);
	
	m_wOriginalLength = rect.right - rect.left;
	m_wOriginalHeight = rect.bottom - rect.top;

	OnViewDetails();
	
	return TRUE;
}

//-----------------------------------------------------------------------------
CString SqlRecoveryManagerResultsDialog::GetResults() const
{
	return m_sResults;
}

//-----------------------------------------------------------------------------
CString SqlRecoveryManagerResultsDialog::GetDetails() const
{
	return m_sDetails;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerResultsDialog::SetResults(LPCTSTR szResults)
{
	m_sResults = szResults;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerResultsDialog::SetDetails(LPCTSTR szDetails)
{
	m_sDetails = szDetails;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerResultsDialog::SetOriginalError(LPCTSTR szError)
{
	m_sOriginalError = szError;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerResultsDialog::SetLogFile(LPCTSTR szLogFile)
{
	m_sLogFile = szLogFile;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerResultsDialog::OnViewLog ()
{
	if (m_sLogFile.IsEmpty())
		return;

	SetForegroundWindow();
	
	::TBShellExecute (m_sLogFile);
}

//-----------------------------------------------------------------------------
void SqlRecoveryManagerResultsDialog::OnViewDetails ()
{
	m_bShowDetails = !m_bShowDetails;

	CWnd* pWnd = GetDlgItem (IDC_RCVRYMNGRESULT_DETAILS);
	if (!pWnd)
		return;

	pWnd->EnableWindow(!m_sDetails.IsEmpty());
	if (m_bShowDetails)
		pWnd->SetWindowText(_TB("Hide Details >>"));
	else
		pWnd->SetWindowText(_TB("Show Details >>"));
	
	pWnd->UpdateWindow	();

	if (m_wCompactHeight == 0)
	{
		// I calculate the compact size
		CRect rect;
		pWnd->GetWindowRect(rect);
		m_wCompactHeight = (m_wOriginalHeight - rect.Height()) / 2;
	}
	this->SetWindowPos(NULL, 0, 0, m_wOriginalLength, m_bShowDetails ? m_wOriginalHeight : m_wCompactHeight, SWP_NOMOVE|SWP_NOZORDER);
}

//////////////////////////////////////////////////////////////////////////////
//					SqlRecoveryManager
//////////////////////////////////////////////////////////////////////////////

//-----------------------------------------------------------------------------
SqlRecoveryManager::SqlRecoveryManager (CBaseDocument* pDocument /*NULL*/)
	:
	m_pUI (NULL)
{
}

//-----------------------------------------------------------------------------
void SqlRecoveryManager::InitializeRecoveryState ()
{
	m_CurrState.Init();
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManager::IsApplicationFrozen () const
{
	TB_LOCK_FOR_READ ();

	return m_CurrState.IsApplicationFrozen();
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManager::ON_CONNECTION_LOST (CString sOriginalError)
{
	// read lock context
	{
		TB_LOCK_FOR_READ();
		if	(
				m_CurrState.GetState() == SqlRecoveryManagerState::Alerted ||
				m_CurrState.GetState() == SqlRecoveryManagerState::Recovering
			)
		return TRUE;
	}

	TB_LOCK_FOR_WRITE();
	m_CurrState.m_Diagnostic.m_sOriginalError = sOriginalError;

	return OnPerformRecoveryActivity ();
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManager::RecoveryConnections ()
{
	TB_OBJECT_LOCK (AfxGetOleDbMng());

	// no connections
	if (!AfxGetOleDbMng()->m_aConnectionPool.GetSize())
		return TRUE;

	UpdateRecoveryUI(SqlRecoveryManagerUM::ConnectionsRecoveryStart(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Information);

	SqlConnection* pConnection;
	BOOL bAllConnected = TRUE;
	for (int i=0; i <= AfxGetOleDbMng()->m_aConnectionPool.GetUpperBound(); i++)
	{
		pConnection = AfxGetOleDbMng()->m_aConnectionPool.GetAt(i);
		// if it is alive, I skip it
		if (!pConnection || pConnection->IsAlive())
			continue;
	
		BOOL bConnected = FALSE;
		// sets the connection to recovery
		TB_OBJECT_LOCK (pConnection);

		CString sConnName(_T("\\\\") + pConnection->m_strServerName + _T(":") + pConnection->m_strDBName);
		for (int r=0; r < m_Settings.GetRetries(); r++)
		{
			UpdateRecoveryUI (SqlRecoveryManagerUM::ConnectionsRecoveryRetry(m_CurrState.GetPhase(), sConnName ,r+1), CDiagnosticManagerWriter::Information, nProgressBarRetriesMult);

			// recovery operation
			if (!RecoveryConnection (pConnection))
			{
				Sleep (m_Settings.GetRetriesIntervalMms());
				continue;
			}

			UpdateRecoveryUI (SqlRecoveryManagerUM::ConnectionsRecoverySuccess(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Information);
			bConnected = TRUE;
			break;
		}
		bAllConnected = bAllConnected && bConnected;
	}

	if (!bAllConnected)
		UpdateRecoveryUI (SqlRecoveryManagerUM::ConnectionsRecoveryFailed(m_CurrState.GetPhase()), CDiagnosticManagerWriter::Error);
	return bAllConnected;
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManager::CloseOldObjects (SqlConnection* pConnection)
{
	CDataSource* pDataSource = (CDataSource*) pConnection->m_pDataSource;
	if (m_CurrState.m_sInitConn.IsEmpty())
	{
		TRY
		{
			BSTR bstrConnString;
			pDataSource->GetInitializationString(&bstrConnString, true);
			CString strConn (bstrConnString);
			m_CurrState.m_sInitConn = strConn;
			
			// to blank password
			m_CurrState.m_sInitConnDisplayable = m_CurrState.m_sInitConn;
			int nPwdPos = m_CurrState.m_sInitConnDisplayable.Find(_T(";Password="));
			nPwdPos += 10;
			for (int i=nPwdPos; i <= m_CurrState.m_sInitConnDisplayable.GetLength(); i++)
			{
				if (m_CurrState.m_sInitConnDisplayable.GetAt(i) == _T(';'))
					break;
				m_CurrState.m_sInitConnDisplayable.SetAt(i, _T('*'));
			}
			pDataSource->Close();
		}
		CATCH (CException, e)
		{
			ASSERT (FALSE);
			return FALSE;
		}
		END_CATCH
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManager::RecoveryConnection (SqlConnection* pConnection)
{
	if (m_pUI->IsAborted ())
	{
		m_CurrState.SetState(SqlRecoveryManagerState::Aborted);
		return FALSE;
	}

	//  closes related objects
	if (!CloseOldObjects (pConnection))
	{
		AddError(SqlRecoveryManagerUM::ConnectionNoCloseObjects(m_CurrState.GetPhase(), m_CurrState.m_sInitConnDisplayable));
		return FALSE;
	}

	ASSERT (pConnection);

	if (m_pUI->IsAborted ())
	{
		m_CurrState.SetState(SqlRecoveryManagerState::Aborted);
		return FALSE;
	}

	CDataSource* pDataSource = (CDataSource*) pConnection->m_pDataSource;
	HRESULT hr = pDataSource->OpenFromInitializationString ((LPCWSTR) m_CurrState.m_sInitConn);

	if (hr == E_FAIL)
	{
		AddError(SqlRecoveryManagerUM::ConnectionNoReconnect(m_CurrState.GetPhase(), m_CurrState.m_sInitConnDisplayable));
		return FALSE;
	}
		
	// properties	
	if (!pConnection->InitConnectionInfo())
	{
		AddError(SqlRecoveryManagerUM::ConnectionNoInitConnection(m_CurrState.GetPhase(), m_CurrState.m_sInitConnDisplayable));
		return FALSE;
	}

	if (m_pUI->IsAborted ())
	{
		m_CurrState.SetState(SqlRecoveryManagerState::Aborted);
		return FALSE;
	}

	return RecoverySessions (pConnection);
}

//-----------------------------------------------------------------------------
BOOL SqlRecoveryManager::RecoverySessions (SqlConnection* pConnection)
{
	ASSERT (pConnection);

	SqlSession* pSession;
	for (int i=0; i <= pConnection->m_arSessionPool.GetUpperBound(); i++)
	{
		if (m_pUI->IsAborted ())
		{
			m_CurrState.SetState(SqlRecoveryManagerState::Aborted);
			return FALSE;
		}

		pSession = pConnection->m_arSessionPool.GetAt(i);
		
		if (!pSession)
			continue;

		TRY
		{
			pSession->ReleaseCommands();
			pSession->Open();
		}
		CATCH (SqlException, e)
		{
			AddError(SqlRecoveryManagerUM::ConnectionNoRecoverySession(m_CurrState.GetPhase(), m_CurrState.m_sInitConnDisplayable));
			return FALSE;
		}
		END_CATCH
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void SqlRecoveryManager::ShowRecoveryUI ()
{
	// recoverying message dialog
	m_pUI = new SqlRecoveryManagerDialog ();

	if (!m_pUI->Create(IDD_RCVRYMNG, AfxGetLoginContext()->m_pMainWnd))
	{
		DestroyRecoveryUI ();
		return;
	}

	m_pUI->ShowWindow (SW_SHOW);
	m_pUI->IsAborted();
}

//-----------------------------------------------------------------------------
void SqlRecoveryManager::UpdateRecoveryUI (LPCTSTR szMessage, CDiagnosticManagerWriter::MessageType eType, const int nSteps /*1*/)
{
	if (!m_pUI)
		return;

	m_pUI->SetUserMessage (szMessage);
	m_pUI->SetProgressBarStep (m_pUI->GetProgressBarCurrStep() + nSteps);
	m_pUI->IsAborted();
	
	m_CurrState.m_Diagnostic.AddMessage(szMessage, eType);

	m_pUI->IsAborted();
}

//-----------------------------------------------------------------------------
void SqlRecoveryManager::SetMaxProgressBarUI(const int& nSteps)
{
	if (m_pUI)
	{
		m_pUI->SetMaxProgressBar(nSteps);
		m_pUI->IsAborted();
	}
}

//-----------------------------------------------------------------------------
void SqlRecoveryManager::DestroyRecoveryUI ()
{
	m_pUI->DestroyWindow();
	m_pUI= NULL;
}

//-----------------------------------------------------------------------------
int SqlRecoveryManager::AskToUser (LPCTSTR szExplanation, LPCTSTR szQuestion)
{
	SqlRecoveryManagerAskDialog aUI;
	aUI.SetExplanation	(szExplanation);
	aUI.SetQuestion		(szQuestion);
	
	return aUI.DoModal();
}


//-----------------------------------------------------------------------------
int SqlRecoveryManager::SayToUser (LPCTSTR szExplanation, LPCTSTR szQuestion)
{
	SqlRecoveryManagerAskDialog aUI;

	aUI.SetExplanation	(szExplanation);
	aUI.SetQuestion		(szQuestion);

	return aUI.DoModal();
}

//-----------------------------------------------------------------------------
void SqlRecoveryManager::ShowResultsToUser ()
{
	SqlRecoveryManagerResultsDialog aUI;
	aUI.SetOriginalError(m_CurrState.GetOriginalError());

	switch (m_CurrState.GetState())
	{
		case SqlRecoveryManagerState::NotRecovered:
			aUI.SetDetails	(SqlRecoveryManagerUM::ResultsDetailsFailed());
			aUI.SetResults	(SqlRecoveryManagerUM::ResultsFailed());
			break;
		case SqlRecoveryManagerState::Aborted:
			aUI.SetDetails	(SqlRecoveryManagerUM::ResultsDetailsAborted());
			aUI.SetResults	(SqlRecoveryManagerUM::ResultsAborted());
			break;
		default:
			if (m_CurrState.m_Diagnostic.IsSnapshotsSaved() && m_CurrState.m_Diagnostic.IsSnapshotsPlayed())
			{
				aUI.SetDetails	(SqlRecoveryManagerUM::ResultsDetailsSuccess());
				aUI.SetResults		(SqlRecoveryManagerUM::ResultsSuccess());
			}
			else
			{
				aUI.SetResults	(SqlRecoveryManagerUM::ResultsWarnings());

				if (!m_CurrState.m_Diagnostic.IsSnapshotsSaved())
					aUI.SetDetails	(SqlRecoveryManagerUM::ResultsDetailsSuccessNoSS());
				else if (!m_CurrState.m_Diagnostic.IsSnapshotsPlayed())
					aUI.SetDetails	(SqlRecoveryManagerUM::ResultsDetailsSuccessNoSP());
			}

			break;
	}
	
	m_CurrState.m_Diagnostic.AddInformation(aUI.GetResults() + _T(" - ") + aUI.GetDetails());
	m_CurrState.m_Diagnostic.Save();
	aUI.SetLogFile (m_CurrState.m_Diagnostic.GetLogFileName());
	aUI.DoModal();
}

//-----------------------------------------------------------------------------
void SqlRecoveryManager::AddError (const CString& sMessage, Array* pExtInfos /*NULL*/)
{
	m_CurrState.SetState(SqlRecoveryManagerState::NotRecovered);
	m_CurrState.m_Diagnostic.AddError(sMessage, pExtInfos);
}

//-----------------------------------------------------------------------------
void SqlRecoveryManager::AddWarning (const CString& sMessage, Array* pExtInfos /*NULL*/)
{
	m_CurrState.m_Diagnostic.AddWarning(sMessage, pExtInfos);
}

//-----------------------------------------------------------------------------
void SqlRecoveryManager::AddInfo (const CString& sMessage, Array* pExtInfos /*NULL*/)
{
	m_CurrState.m_Diagnostic.AddInfo(sMessage, pExtInfos);
}
