// XMLImpExpDialog.cpp : implementation file
//

#include "stdafx.h"

#include <locale.h>

#include <TBNameSolver\ThreadContext.h>
#include <TbGeneric\CMapi.h>

#include <TbGenlib\messages.h>
#include <TbGenlib\basedoc.h>

#include "XMLImpExpDialog.h"

#include "XMLDataMng.hjson"

static const TCHAR szDefaultEmailTo[] = _T("tuning@microarea.it");
//.SmtpMailConnector......................................................
static const TCHAR szTbMailerNamespace[]	= _T("Module.Extensions.TbMailer");
static const TCHAR szSmtpMailConnector[]	= _T("MailConnector-Smtp");
static const TCHAR szSmtpSettingsFile[]		= _T("Smtp.config");
static const TCHAR szHostName[]				= _T("HostName");
static const TCHAR szFromAddress[]			= _T("FromAddress");
//=============================================================================
// CXMLImportTuningResult dialog
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLImportTuningResult, CParsedDialog)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXMLImportTuningResult, CParsedDialog)
	ON_BN_CLICKED(IDC_IMPORT_TUNING_NOTIFY,		OnNotifyClicked)
	ON_BN_CLICKED(IDC_IMPORT_TUNING_EMAILSEND,	OnSendEmail)
	ON_BN_CLICKED(IDC_IMPORT_TUNING_LOG,		OnLogFileClicked)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CXMLImportTuningResult::CXMLImportTuningResult
	(
		CWnd* pParent, 
		const CString& strLogFile, 
		Array* pChanges, 
		BOOL bSuccess
	)
	: 
	CParsedDialog(IDD_IMPORT_OUTPUT, pParent),
	m_pChanges			(pChanges),
	m_bSuccess			(bSuccess),
	m_strLogFile		(strLogFile),
	m_bWndCollapsed		(FALSE),
	m_nExpandedHeight	(0),
	m_nCollapsedHeight	(0),
	m_nWndWidth			(0)
{
}

//-----------------------------------------------------------------------------
CXMLImportTuningResult::~CXMLImportTuningResult()
{
}

//-----------------------------------------------------------------------------
BOOL CXMLImportTuningResult::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	if (!m_pChanges)
		return TRUE;

	m_lbxChanges.SubclassDlgItem (IDC_IMPORT_TUNING_LBXCHANGES, this);
	m_bnNotify.SubclassDlgItem (IDC_IMPORT_TUNING_NOTIFY, this);
	m_bnSendEmail.SubclassDlgItem (IDC_IMPORT_TUNING_EMAILSEND, this);
	
	InitializeControls ();
	
	CenterWindow();
	UpdateWindow();

	CalculateCollapse  ();
	OnNotifyClicked();

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}

//-----------------------------------------------------------------------------
void CXMLImportTuningResult::InitializeControls ()
{
	CWnd* pWnd = GetDlgItem (IDC_IMPORT_TUNING_RESULT);
	if (pWnd)
	{
		CString str;
		pWnd->GetWindowText(str);
		pWnd->SetWindowText(cwsprintf (str, m_bSuccess ? _TB("with success!") : _TB("with some errors!")));
	}

	if (!m_pChanges->GetSize())
	{
		pWnd = GetDlgItem (IDC_IMPORT_TUNING_NOTIFY);
		if (pWnd)
			pWnd->EnableWindow(FALSE);
	}

	if (AfxIsActivated(szExtensionsApp, _NS_ACT("TbMailer")))
	{
		CTBNamespace tbMailerNs (szTbMailerNamespace);
		DataObj* pDataObj;
		CWnd* pWnd = GetDlgItem (IDC_IMPORT_TUNING_NOMAILCONNECTOR);
		if (pWnd)
			pWnd->ShowWindow(SW_HIDE);

		
		pWnd = GetDlgItem (IDC_IMPORT_TUNING_EMAILSERVER);
		if (pWnd)
		{
			pDataObj = AfxGetSettingValue(tbMailerNs, szSmtpMailConnector, szHostName, DataStr(), szSmtpSettingsFile);
			if (pDataObj && pDataObj->GetDataType() == DATA_STR_TYPE)
				pWnd->SetWindowText(pDataObj->Str());
		}

		pWnd = GetDlgItem (IDC_IMPORT_TUNING_EMAILFROM);
		if (pWnd)
		{
			pDataObj = AfxGetSettingValue(tbMailerNs, szSmtpMailConnector, szFromAddress, DataStr(), szSmtpSettingsFile);
			if (pDataObj && pDataObj->GetDataType() == DATA_STR_TYPE)
				pWnd->SetWindowText(pDataObj->Str());
		}

		pWnd = GetDlgItem (IDC_IMPORT_TUNING_EMAILTO);
		if (pWnd)
			pWnd->SetWindowText(szDefaultEmailTo);
	}
	else
	{
		HideControlGroup (IDC_IMPORT_TUNING_NOTIFYBOX, TRUE);
		pWnd = GetDlgItem (IDC_IMPORT_TUNING_NOMAILCONNECTOR);
		if (pWnd)
			pWnd->ShowWindow(SW_SHOW);
	}

	LoadListBox ();
}

//-----------------------------------------------------------------------------
void CXMLImportTuningResult::CalculateCollapse	()
{
	CWnd* pWnd = GetDlgItem (IDC_IMPORT_TUNING_NOTIFY);
	if (!pWnd)
		return;
	
	CRect aBtnRect;
	pWnd->GetWindowRect(aBtnRect);
	
	CRect aDialogRect;
	GetWindowRect(aDialogRect);
	CRect aProvaRect(aBtnRect);
	ScreenToClient(aProvaRect);

	m_nExpandedHeight	= aDialogRect.Height();
	m_nCollapsedHeight	= aBtnRect.bottom -  aDialogRect.top + 10; 
	m_nWndWidth			= aDialogRect.Width();
}

//-----------------------------------------------------------------------------
void CXMLImportTuningResult::LoadListBox ()
{
	if (!m_pChanges)
		return;

	CString sStandardPath = AfxGetPathFinder()->GetStandardPath();
	sStandardPath = sStandardPath.MakeLower();
	CString sCustomPath = AfxGetPathFinder()->GetCustomPath();
	sCustomPath = sCustomPath.MakeLower();

	CChangedTables* pChange = NULL;
	for (int i=0; i <= m_pChanges->GetUpperBound(); i++)
	{
		pChange = (CChangedTables*) m_pChanges->GetAt(i);
		if (pChange)
		{
			CString sFileName = pChange->m_strFileName.MakeLower();
			sFileName.Replace (sStandardPath, _T(""));
			sFileName.Replace (sCustomPath, _T(""));
			m_lbxChanges.AddString (pChange->m_strFileName);
		}
	}
	
	if (!m_pChanges->GetSize())
		m_lbxChanges.AddString (_TB("No tuning differences detected."));
}

//-----------------------------------------------------------------------------
void CXMLImportTuningResult::OnLogFileClicked()
{
	ShellExecute (m_strLogFile);
}

//-----------------------------------------------------------------------------
void CXMLImportTuningResult::OnNotifyClicked ()
{
	m_bWndCollapsed = !m_bWndCollapsed;

	SetWindowPos (NULL, 0, 0, m_nWndWidth, m_bWndCollapsed ? m_nCollapsedHeight : m_nExpandedHeight, SWP_NOMOVE);
	UpdateWindow ();
}

//-----------------------------------------------------------------------------
BOOL CXMLImportTuningResult::CanSendEmail ()
{
	BOOL bOk = TRUE;
	CString str;
	CWnd* pWnd = GetDlgItem (IDC_IMPORT_TUNING_EMAILSERVER);
	if (pWnd)
		pWnd->GetWindowText(str);

	if (str.IsEmpty())
	{
		m_aMessages.Add(_TB("Smtp server is missing or is invalid!"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}

	pWnd = GetDlgItem (IDC_IMPORT_TUNING_EMAILFROM);
	if (pWnd)
		pWnd->GetWindowText(str);

	if (!IsValidAddress(str))
	{
		m_aMessages.Add(_TB("From email address is missing or is invalid!"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}

	pWnd = GetDlgItem (IDC_IMPORT_TUNING_EMAILTO);
	if (pWnd)
		pWnd->GetWindowText(str);

	if (str.IsEmpty())
	{
		m_aMessages.Add(_TB("To email address is missing or is invalid!"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}

	if (!bOk)
		m_aMessages.Add(_TB("Please configure mail connector settings!"), CMessages::MSG_HINT);
	
	return bOk;
}

//-----------------------------------------------------------------------------
void CXMLImportTuningResult::OnSendEmail ()
{
	if (!CanSendEmail ())
	{
		m_aMessages.Show (TRUE);
		return;
	}

	CMapiMessage email;
	email.SetFrom	(m_strEmailFrom);
	email.SetTo		(szDefaultEmailTo);
	email.SetBody	(_TB("This message is auto-generated in order to notify XEngine tuning procedure results.\nResults files are attached to this message.\n\nXEngine Development Team\nMicroarea S.p.A."));
	
	CString sDocTitle;
	// attachments
	CChangedTables* pChange = NULL;
	for (int i=0; i <= m_pChanges->GetUpperBound(); i++)
	{
		pChange = (CChangedTables*) m_pChanges->GetAt(i);
		if (!pChange)
			continue;
		email.SetAttachment (pChange->m_strFileName);
		if (sDocTitle.IsEmpty())
			sDocTitle = pChange->m_strDocTitle;
	}

	email.SetSubject(cwsprintf(_TB("XEngine Tuning Result Notification Message for the document %s"), sDocTitle));

	// sends email using mail connector configuration parameters
	// for mapi messages I manage locale
	CString strOldLocale = ::_tsetlocale (LC_ALL, NULL);

	if	(
			AfxGetIMailConnector()->SendMail (email, NULL, NULL, &m_aMessages) &&
			!m_aMessages.MessageFound()
		)
		m_aMessages.Add(_TB("Email sent successfully!"), CMessages::MSG_HINT);
	else
		m_aMessages.Add(_TB("Cannot send email due to the above errors!"), CMessages::MSG_HINT);
	
	CString strLocale = ::_tsetlocale (LC_ALL, NULL);
	
	if (strOldLocale.CompareNoCase(strLocale) != 0)
		::_tsetlocale (LC_ALL, strOldLocale);

	m_aMessages.Show (TRUE);
}

//-----------------------------------------------------------------------------
BOOL CXMLImportTuningResult::IsValidAddress	(const CString& sAddress)
{
	if (sAddress.IsEmpty())
		return FALSE;

	int nPos = sAddress.Find(_T("@"));
	if (nPos <= 0)
		return FALSE;

	nPos = sAddress.Mid(nPos+1).Find(_T("."));
	return nPos > 0;
}

//=============================================================================
// CXMLImpExpDialog dialog
//=============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CXMLImpExpDialog, CLocalizableDialog)
CXMLImpExpDialog::CXMLImpExpDialog(CWnd* pParent /*=NULL*/)
: CLocalizableDialog(IDD_IMPORT_OUTPUT, pParent),
	m_bAborted(FALSE)
{
}

//-----------------------------------------------------------------------------
CXMLImpExpDialog::~CXMLImpExpDialog()
{
}

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CXMLImpExpDialog, CLocalizableDialog)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
BOOL CXMLImpExpDialog::OnInitDialog()
{
	CLocalizableDialog::OnInitDialog();
	m_ProgressBar.SubclassDlgItem (IDC_IMPEXP_PROGRESS, this);	

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}

//-----------------------------------------------------------------------------
void CXMLImpExpDialog::PostNcDestroy()
{
	delete this;
}

//-----------------------------------------------------------------------------
void CXMLImpExpDialog::OnCancel()
{
	if (AfxMessageBox(_TB("Are you sure you want to stop the procedure?"), MB_YESNO) == IDYES)
		m_bAborted = TRUE;
}

//-----------------------------------------------------------------------------
void CXMLImpExpDialog::SetMessage(const CString& strMessage)
{
	SetDlgItemText(IDC_MESSAGE_STEP, strMessage);
	UpdateWindow();
}

//=============================================================================
// CXMLImpExpController
//=============================================================================
//-----------------------------------------------------------------------------
CXMLImpExpController::CXMLImpExpController()
:
	m_pDialog(NULL)
{
}

//-----------------------------------------------------------------------------
BOOL CXMLImpExpController::ShowDialog()
{ 
	if (IsBusy()) return FALSE;

	m_pDialog = new CXMLImpExpDialog();
	m_pDialog->Create(IDD_IMPORT_OUTPUT, AfxGetMainWnd());
	m_pDialog->ShowWindow(SW_SHOW);
	AfxGetThreadContext()->m_bSendDocumentEventsToMenu = FALSE;
	return TRUE;
}

//-----------------------------------------------------------------------------
void CXMLImpExpController::CloseDialog()
{
	if (!IsBusy()) return;

	m_pDialog->DestroyWindow();
	AfxGetThreadContext()->m_bSendDocumentEventsToMenu = TRUE;
	m_pDialog = NULL;
}

//-----------------------------------------------------------------------------
void CXMLImpExpController::SetMessage(const CString& strMessage)
{
	if (m_pDialog) 
		m_pDialog->SetMessage(strMessage);
}


DECLARE_THREAD_VARIABLE (CXMLImpExpController, t_Controller)
CXMLImpExpController* AfxGetXMLImpExpController()
{
	GET_THREAD_VARIABLE(CXMLImpExpController, t_Controller)
	return &t_Controller; 
}

