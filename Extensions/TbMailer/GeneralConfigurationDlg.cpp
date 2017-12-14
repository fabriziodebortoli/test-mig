#include "stdafx.h"
#ifdef new
#undef new
#endif

#include <TbGeneric\ParametersSections.h>

#include "CMapiSession.h"
#include "CMailConnector.h"

#include "SmtpMail.hjson" //JSON AUTOMATIC UPDATE
#include "GeneralConfigurationDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif
IMPLEMENT_DYNAMIC(CGeneralConfigurationDlg, CParsedDialog)
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CGeneralConfigurationDlg, CParsedDialog)
	ON_BN_CLICKED(IDC_USEMAPI, OnChooseProtocol)
	ON_BN_CLICKED(IDC_USENETSMTP, OnChooseProtocol)
	ON_BN_CLICKED(IDC_CRYPTFILE, OnChooseCryptFile)
	ON_BN_CLICKED(IDC_CHOOSE_TRACKING_ADDRESS, OnChooseTrackingAddress)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
CGeneralConfigurationDlg::CGeneralConfigurationDlg(CWnd* pParent /*=NULL*/)
	: CParsedDialog(IDD_GENERAL_CONFIGURATION, pParent)
{
	m_sOutlookProfile			= _T("Outlook");
	m_bSupportOutlookExpress	= FALSE;
	m_bMailCompress				= TRUE;
	m_bCryptFile				= FALSE;
	m_sPassword					= _T("");
	m_nProtocol					= 0;
	m_bRequestDeliveryNotification	= FALSE;
	m_bRequestReadNotification		= FALSE;
}

//--------------------------------------------------------------------------------------------------
BOOL CGeneralConfigurationDlg::OnInitDialog ()
{
	CParsedDialog::OnInitDialog();
	
	GetDlgItem(IDC_REQUEST_DELIVERY_NOTIFICATION)->EnableWindow(m_nProtocol != 0);
	GetDlgItem(IDC_REQUEST_READ_NOTIFICATION)->EnableWindow(m_nProtocol != 0);

	GetDlgItem(IDC_OUTLOOKPROFILE)->EnableWindow(m_nProtocol == 0);
	GetDlgItem(IDC_SUPPORTOLEXPRESS)->EnableWindow(m_nProtocol == 0);

	//GetDlgItem(IDC_MAPIEX_REPLYTO_ADDRESS)->EnableWindow(FALSE);
	//GetDlgItem(IDC_MAPIEX_CHOOSE_REPLYTO)->EnableWindow(FALSE);

	GetDlgItem(IDC_PASSWORD)->EnableWindow(m_bCryptFile);

	m_PrintersCombo.SubclassDlgItem(IDC_PRINTERS, this);
	LoadPrinters();

	UpdateData(FALSE);
	return TRUE;
}

//--------------------------------------------------------------------------------------------------
void CGeneralConfigurationDlg::DoDataExchange(CDataExchange* pDX)
{
	CParsedDialog::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_OUTLOOKPROFILE, m_ctrlOutlookProfile);
	DDX_Text(pDX, IDC_OUTLOOKPROFILE, m_sOutlookProfile);

	DDX_Check(pDX, IDC_CRYPTFILE, m_bCryptFile);
	DDX_Control(pDX, IDC_PASSWORD, m_ctrlPassword);
	DDX_Text(pDX, IDC_PASSWORD, m_sPassword);

	DDX_Check(pDX, IDC_MAILCOMPRESS, m_bMailCompress);

	DDX_Check(pDX, IDC_SUPPORTOLEXPRESS, m_bSupportOutlookExpress);

	DDX_Radio(pDX, IDC_USEMAPI, m_nProtocol);
	//DDX_Radio(pDX, IDC_USENETSMTP, m_nProtocol);

	DDX_Check(pDX, IDC_REQUEST_DELIVERY_NOTIFICATION, m_bRequestDeliveryNotification);
	DDX_Check(pDX, IDC_REQUEST_READ_NOTIFICATION, m_bRequestReadNotification);
	
	DDX_Text(pDX, IDC_REPLYTOADDRESS, m_sReplyToAddress);
	DDX_Text(pDX, IDC_TRACKING_ADDRESS, m_sTrackingAddress);

	DDX_CBStringExact(pDX, IDC_PRINTERS, m_sPrinterTemplate);

	DDX_Text(pDX, IDC_FAX_FORMAT_TEMPLATE, m_sFaxFormatTemplate);
}

//--------------------------------------------------------------------------------------------------
void CGeneralConfigurationDlg::OnChooseProtocol()
{
	if (IsDlgButtonChecked(IDC_USENETSMTP))
	{
		m_nProtocol = 1;
	}
	else if (IsDlgButtonChecked(IDC_USEMAPI))
	{
		m_nProtocol = 0;

		CheckDlgButton(IDC_REQUEST_DELIVERY_NOTIFICATION, 0);
		CheckDlgButton(IDC_REQUEST_READ_NOTIFICATION, 0);
	}
	
	GetDlgItem(IDC_REQUEST_DELIVERY_NOTIFICATION)->EnableWindow(m_nProtocol != 0);
	GetDlgItem(IDC_REQUEST_READ_NOTIFICATION)->EnableWindow(m_nProtocol != 0);

	GetDlgItem(IDC_OUTLOOKPROFILE)->EnableWindow(m_nProtocol == 0);
	GetDlgItem(IDC_SUPPORTOLEXPRESS)->EnableWindow(m_nProtocol == 0);

	//GetDlgItem(IDC_MAPIEX_REPLYTO_ADDRESS)->EnableWindow(m_bUseMapiEx);
	//GetDlgItem(IDC_MAPIEX_CHOOSE_REPLYTO)->EnableWindow(m_bUseMapiEx);
}

//--------------------------------------------------------------------------------------------------
void CGeneralConfigurationDlg::OnChooseCryptFile()
{
	m_bCryptFile = IsDlgButtonChecked(IDC_CRYPTFILE) == 1;

	GetDlgItem(IDC_PASSWORD)->EnableWindow(m_bCryptFile);
}

//--------------------------------------------------------------------------------------------------
void CGeneralConfigurationDlg::OnChooseReplyToAddress()
{
	CString str;
	GetDlgItem(IDC_REPLYTOADDRESS)->GetWindowText(str);

	if (AfxGetIMailConnector()->MapiShowAddressBook(m_hWnd, str))
	{
		GetDlgItem(IDC_REPLYTOADDRESS)->SetWindowText(str);
	}
}

//--------------------------------------------------------------------------------------------------
void CGeneralConfigurationDlg::OnChooseTrackingAddress()
{
	CString str;
	GetDlgItem(IDC_TRACKING_ADDRESS)->GetWindowText(str);

	if (AfxGetIMailConnector()->MapiShowAddressBook(m_hWnd, str))
	{
		GetDlgItem(IDC_TRACKING_ADDRESS)->SetWindowText(str);
	}
}

//------------------------------------------------------------------------------
void CGeneralConfigurationDlg::LoadPrinters()
{
	CStringArray aPrinters;

	int nPos = GetPrinterNames(aPrinters, m_sPrinterTemplate);

	m_PrintersCombo.AddString(_T(""));
	for (int i = 0; i <= aPrinters.GetUpperBound(); i++)
		m_PrintersCombo.AddString(aPrinters[i]);

	m_PrintersCombo.SetCurSel(nPos >= 0 ? nPos + 1 : 0);
}
