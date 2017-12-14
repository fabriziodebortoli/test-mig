#include "stdafx.h"
#ifdef new
#undef new
#endif

#include <TbGenlibUI/SettingsTableManager.h>

#include "SmtpMail.hjson" //JSON AUTOMATIC UPDATE
#include "SmtpParametersSections.h"

#include "SmtpConfigurationDlg.h"
#include "CMailConnector.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
//-----------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(CMailConfigurationDlg, CParsedDialog)
BEGIN_MESSAGE_MAP(CMailConfigurationDlg, CParsedDialog)

	ON_BN_CLICKED(IDC_HTML, OnHtml)

	ON_BN_CLICKED(IDC_EXPLICIT_SSL, OnExplicitSSL)
	ON_BN_CLICKED(IDC_IMPLICIT_SSL, OnImplicitSSL)

	ON_BN_CLICKED(IDC_SMTP_CHOOSE_SENDER_EMAIL, 	OnChooseSenderAddress)
	ON_BN_CLICKED(IDC_SMTP_CHOOSE_REPLYTO_EMAIL,	OnChooseReplyToAddress)

	ON_BN_CLICKED(IDC_ACCOUNT_DEFAULT, OnClickAccount)
	ON_BN_CLICKED(IDC_ACCOUNT_CERTIFIED, OnClickAccount)

	ON_BN_CLICKED(IDC_APPLY, OnApply)
	ON_BN_CLICKED(IDC_TEST, OnTest)

END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CMailConfigurationDlg::CMailConfigurationDlg(CWnd* pParent /*=NULL*/)
	: CParsedDialog(IDD_CONFIGURATION, pParent)
{
	m_sAddress = _T("");
	m_sHost = _T("");
	m_sName = _T("");
	m_nTimeout = m_nPort = 0; 
	m_sPassword = _T("");
	m_sUsername = _T("");
	m_bHTML = FALSE;
	m_bExplicitSSL = FALSE;
	m_bImplicitSSL = FALSE;
	m_nPort = 25; //Use the default SMTP port
	m_nTimeout = 200000;
	m_bAccountDefault = TRUE;
	m_bAccountCertified = FALSE;
	m_sAuthenticationType = L"NTLM";
	m_nConfiguration = 0;
	//----
	LoadSettings(FALSE);
}

//-----------------------------------------------------------------------------
void CMailConfigurationDlg::LoadSettings(BOOL bUpdate)
{
	m_settings.SetCurrentSection(m_bAccountDefault ? L"" : CMapiMessage::TAG_CERTIFIED);

	m_sReplyToAddress   = m_settings.GetReplyToAddress();
	m_sReplyToName      = m_settings.GetReplyToName();
	m_sAddress          = m_settings.GetFromAddress();
	m_sName             = m_settings.GetFromName();
	m_sHost             = m_settings.GetHostName();
	m_nPort             = m_settings.GetPort();
	m_sUsername         = m_settings.GetUserName();
	m_sPassword         = m_settings.GetPassword();
	m_bMime             = m_settings.GetMimeEncoding();
	m_bHTML             = m_settings.GetHtmlEncoding();
	m_bExplicitSSL		= m_settings.GetUseExplicitSSL();
	m_bImplicitSSL		= m_settings.GetUseImplicitSSL();
	m_nTimeout          = m_settings.GetTimeout();
	m_sAuthenticationType = m_settings.GetAuthenticationType();
	if (m_bAccountDefault)
	{
		m_nConfiguration = m_settings.GetConfiguration();
		if (m_cbxConfiguration.m_hWnd)
			m_cbxConfiguration.SetCurSel(m_nConfiguration);
	}
	if (bUpdate)
		UpdateData(FALSE);
}

//-----------------------------------------------------------------------------
void CMailConfigurationDlg::SaveSettings()
{
	UpdateData(TRUE);

	m_settings.SetHostName(m_sHost);
	m_settings.SetFromAddress(m_sAddress);
	m_settings.SetFromName(m_sName);
	m_settings.SetReplyToAddress(m_sReplyToAddress);
	m_settings.SetReplyToName(m_sReplyToName);
	m_settings.SetPort(abs(m_nPort));
	m_settings.SetUserName(m_sUsername);
	m_settings.SetPassword(m_sPassword);
	m_settings.SetMimeEncoding(m_bMime);
	m_settings.SetHtmlEncoding(m_bHTML);
	m_settings.SetUseExplicitSSL(m_bExplicitSSL);
	m_settings.SetUseImplicitSSL(m_bImplicitSSL);
	m_settings.SetTimeout(abs(m_nTimeout));
	m_settings.SetAuthenticationType(m_sAuthenticationType);
	if (m_bAccountDefault)
	{
		m_nConfiguration = max(0, m_cbxConfiguration.GetCurSel());
		m_settings.SetConfiguration(m_nConfiguration);
	}

	AfxSaveSettingsFile(&m_settings, TRUE);
}

//-----------------------------------------------------------------------------
BOOL CMailConfigurationDlg::OnInitDialog		()
{
	__super::OnInitDialog();

	m_LineSep.SubclassDlgItem(IDC_SEPARATOR1, this);
		m_LineSep.SetValue(L"");
		m_LineSep.ShowSeparator(RGB(0,0,255), 2);

	//----
	m_cbxAuthenticationType.AddString(L"NTLM");
	m_cbxAuthenticationType.AddString(L"Digest");
	m_cbxAuthenticationType.AddString(L"Negotiate");
	m_cbxAuthenticationType.AddString(L"Kerberos");

	m_cbxAuthenticationType.SelectString(-1, m_sAuthenticationType);

	//----
	int idx = m_cbxConfiguration.AddString(_TB("profilo 1"));
	m_cbxConfiguration.SetItemData(idx, 0);
	idx = m_cbxConfiguration.AddString(_TB("profilo 2"));
	m_cbxConfiguration.SetItemData(idx, 1);
	idx = m_cbxConfiguration.AddString(_TB("profilo 3"));
	m_cbxConfiguration.SetItemData(idx, 2);

	m_cbxConfiguration.SetCurSel(m_nConfiguration);
	//----
	m_cbxPortNumber.AddString(L"25");
	m_cbxPortNumber.AddString(L"465");
	m_cbxPortNumber.AddString(L"587");

	return TRUE;
}

//-----------------------------------------------------------------------------
void CMailConfigurationDlg::DoDataExchange(CDataExchange* pDX)
{
	__super::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_USERNAME, m_ctrlUsername);
	DDX_Control(pDX, IDC_PASSWORD, m_ctrlPassword);
	DDX_Text(pDX, IDC_PASSWORD, m_sPassword);
	DDX_Text(pDX, IDC_USERNAME, m_sUsername);

	DDX_Text(pDX, IDC_ADDRESS, m_sAddress);
	DDX_Text(pDX, IDC_HOST, m_sHost);
	DDX_Text(pDX, IDC_NAME, m_sName);

	DDX_Text(pDX, IDC_PORT, m_nPort);
	DDX_Control(pDX, IDC_PORT, m_cbxPortNumber);

	DDX_Text(pDX, IDC_TIMEOUT, m_nTimeout);
	DDX_Check(pDX, IDC_MIME, m_bMime);
	DDX_Text(pDX, IDC_REPLYTOADDRESS, m_sReplyToAddress);
	DDX_Text(pDX, IDC_REPLYTONAME, m_sReplyToName);

	DDX_Check	(pDX, IDC_HTML, m_bHTML);
	DDX_Control	(pDX, IDC_HTML, m_btnHtml);

	DDX_Check	(pDX, IDC_EXPLICIT_SSL,			m_bExplicitSSL);
	DDX_Check	(pDX, IDC_IMPLICIT_SSL,			m_bImplicitSSL);
	DDX_Control	(pDX, IDC_EXPLICIT_SSL,			m_btnExplicitSSL);
	DDX_Control	(pDX, IDC_IMPLICIT_SSL,			m_btnImplicitSSL);

	DDX_Check	(pDX, IDC_ACCOUNT_DEFAULT,		m_bAccountDefault);
	DDX_Check	(pDX, IDC_ACCOUNT_CERTIFIED,	m_bAccountCertified);
	DDX_Control	(pDX, IDC_ACCOUNT_DEFAULT,		m_btnAccountDefault);
	DDX_Control	(pDX, IDC_ACCOUNT_CERTIFIED,	m_btnAccountCertified);

	DDX_Text	(pDX, IDC_CBX_AUTH_TYPE,		m_sAuthenticationType);
	DDX_Control	(pDX, IDC_CBX_AUTH_TYPE,		m_cbxAuthenticationType);

	DDX_Text	(pDX, IDC_CBX_CONFIGURATION,	m_sConfiguration);
	DDX_Control	(pDX, IDC_CBX_CONFIGURATION,	m_cbxConfiguration);

}

//-----------------------------------------------------------------------------
void CMailConfigurationDlg::OnOK ()
{
	SaveSettings();

	__super::OnOK();
}

//-----------------------------------------------------------------------------
void CMailConfigurationDlg::OnApply ()
{
	SaveSettings();
}

//-----------------------------------------------------------------------------
void CMailConfigurationDlg::OnHtml() 
{
  UpdateData(TRUE);

  if (m_bHTML)
  {
    m_bMime = TRUE;
    CDataExchange DX2(this, FALSE);
    DDX_Check(&DX2, IDC_MIME, m_bMime);    
  }  	
}

//-----------------------------------------------------------------------------
void CMailConfigurationDlg::OnExplicitSSL() 
{
	UpdateData(TRUE);

	if (m_bExplicitSSL)
	{
		m_bImplicitSSL = FALSE;
		//m_nPort = 465;
	}
	//else
	//	m_nPort = 25;

	CDataExchange DX2(this, FALSE);
	//DDX_Text(&DX2, IDC_PORT, m_nPort);
	DDX_Check(&DX2, IDC_IMPLICIT_SSL, m_bImplicitSSL);
}

void CMailConfigurationDlg::OnImplicitSSL() 
{
	UpdateData(TRUE);

	if (m_bImplicitSSL)
	{
		m_bExplicitSSL = FALSE;
		//m_nPort = 465;
	}
	//else
	//	m_nPort = 25;

	CDataExchange DX2(this, FALSE);
	//DDX_Text(&DX2, IDC_PORT, m_nPort);
	DDX_Check(&DX2, IDC_EXPLICIT_SSL, m_bExplicitSSL);
}

//-----------------------------------------------------------------------------
void CMailConfigurationDlg::OnClickAccount() 
{
	UpdateData(TRUE);

	m_cbxConfiguration.EnableWindow(m_bAccountDefault);

	LoadSettings(TRUE);
}

//-----------------------------------------------------------------------------
void CMailConfigurationDlg::OnChooseSenderAddress() 
{
	CString str;
	if (AfxGetIMailConnector()->MapiShowAddressBook(this->m_hWnd, str))
	{
		GetDlgItem(IDC_ADDRESS)->SetWindowText(str);
	}
}

void CMailConfigurationDlg::OnChooseReplyToAddress() 
{
	CString str;
	if (AfxGetIMailConnector()->MapiShowAddressBook(this->m_hWnd, str))
	{
		GetDlgItem(IDC_REPLYTOADDRESS)->SetWindowText(str);
	}
}
//-----------------------------------------------------------------------------
void CMailConfigurationDlg::OnTest() 
{
	SaveSettings();

	if (m_sReplyToAddress.IsEmpty())
	{
		AfxGetDiagnostic()->Add(_TB("ReplyTo address is mandatory because it will be use as addresee of test email"));
		AfxGetDiagnostic()->Show();
		return;
	}

	CMapiMessage msg;

	msg.SetBody(_TB("Test body"));
	msg.SetSubject(_TB("Test Subject"));
	msg.SetFrom(m_sAddress);
	msg.SetTo(m_sReplyToAddress);

	BOOL bOk = AfxGetIMailConnector()->SmtpSendMail(msg, FALSE, FALSE, AfxGetDiagnostic()); 

	if (AfxGetDiagnostic()->ErrorFound())
	{
		CString serr = AfxGetDiagnostic()->ToString();
		AfxGetDiagnostic()->ClearMessages(TRUE);

		AfxTBMessageBox(serr);
	}
	else if (!bOk)
	{
		AfxTBMessageBox(_TB("Generic error on send email"));
	}
}
//-----------------------------------------------------------------------------
