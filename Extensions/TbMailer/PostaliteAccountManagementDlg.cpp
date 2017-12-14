#include "stdafx.h" 
#include <afxlinkctrl.h>
#include "PostaliteAccountManagementDlg.h"
#include "PostaliteAccountManagementDlg.hjson" //JSON AUTOMATIC UPDATE	
#include "PostaLiteSubscribeDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static PostaLiteSettings* m_Setting;
static CMessages* m_Messages;
static TbSenderInterface* m_pTbSenderInterface;

static DataStr m_strRestrictiveClauses;
static DataStr m_strPrivacyPolicyUrl;
static DataStr m_strTermsOfUseUrl; 
static DataStr m_strPriceListUrl;
static DataStr m_strCadastralPageUrl;
static DataStr m_strAcceptRecharge;
static DataStr m_strLaw136;
static DataMon m_MinimumRecharge;
static DataStr m_strRestrictiveClause1; 
static DataStr m_strPrivacyPolicyClause; 

//---------------------------------------------------------------------------------------
static TbSenderInterface* GetTbSenderInterface()
{
	if (!m_pTbSenderInterface)
		m_pTbSenderInterface = new TbSenderInterface();

	return m_pTbSenderInterface;
}

//---------------------------------------------------------------------------------------
static PostaLiteSettings* GetPostaLiteSettings()
{
	if (!m_Setting)
		m_Setting = new PostaLiteSettings();

	return m_Setting;
}

//---------------------------------------------------------------------------------------
static void ClearStaticObjects()
{
	SAFE_DELETE(m_Setting);
	SAFE_DELETE(m_Messages);
	SAFE_DELETE(m_pTbSenderInterface);
}

//---------------------------------------------------------------------------------------
static CMessages* GetPostaLiteMessages()
{
	if (!m_Messages)
		m_Messages = new CMessages();

	return m_Messages;
}

//---------------------------------------------------------------------------------------
static void LoadLegalInfos()
{
	DataStr iban, bank, beneficiary, errorMessage;
	DataStr language (AfxGetLoginContext()->GetLoginInfos()->m_strPreferredLanguage);

	if (m_strTermsOfUseUrl.IsEmpty())
		GetTbSenderInterface()->GetLegalInfos
		(
			language,
			m_strRestrictiveClauses, 
			iban,
			bank,
			beneficiary,
			m_strPrivacyPolicyUrl,
			m_strPriceListUrl,
			m_strTermsOfUseUrl,
			m_strAcceptRecharge, 
			m_strLaw136,
			m_strCadastralPageUrl,
			m_MinimumRecharge,
			errorMessage
		);
}

//////////////////////////////////////////////////////////////////////////////////
//		CPostaliteAccountManagementWizardDoc implementation
//////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaliteAccountManagementWizardDoc, CWizardFormDoc)

//------------------------------------------------------------------
CPostaliteAccountManagementWizardDoc::CPostaliteAccountManagementWizardDoc()
{
	m_Type = VMT_BATCH;	
}

//---------------------------------------------------------------------------
BOOL CPostaliteAccountManagementWizardDoc::OnOpenDocument(LPCTSTR pParam)
{
	return CAbstractFormDoc::OnOpenDocument(pParam);
}

//---------------------------------------------------------------------------
BOOL CPostaliteAccountManagementWizardDoc::OnAttachData()
{
	SetFormTitle(_TB("PostaLite Account Management"));

	return TRUE;
}

//---------------------------------------------------------------------------
CCompanyAddressInfo* CPostaliteAccountManagementWizardDoc::GetCompanyData()
{			
	return LoadCompanyData();
	//TPostaliteCompany* pTCompany = new TPostaliteCompany();
	//SqlTable* m_pTblCompany = new SqlTable(pTCompany, AfxGetDefaultSqlSession());	

	//if (m_pTblCompany->IsOpen())
	//	m_pTblCompany->Close();

	//pTCompany->SetQualifier();

	//m_pTblCompany->Open();
	//m_pTblCompany->SelectAll();
	//m_pTblCompany->Select(pTCompany, pTCompany->f_CompanyName);
	//m_pTblCompany->Select(pTCompany, pTCompany->f_TaxIdNumber);
	//m_pTblCompany->Select(pTCompany, pTCompany->f_FiscalCode);	
	//m_pTblCompany->Select(pTCompany, pTCompany->f_Address);
	//m_pTblCompany->Select(pTCompany, pTCompany->f_ZIPCode);
	//m_pTblCompany->Select(pTCompany, pTCompany->f_City);
	//m_pTblCompany->Select(pTCompany, pTCompany->f_County);
	//m_pTblCompany->Select(pTCompany, pTCompany->f_Country);
	//m_pTblCompany->Select(pTCompany, pTCompany->f_Telephone1);
	//m_pTblCompany->Select(pTCompany, pTCompany->f_Fax);
	//m_pTblCompany->Select(pTCompany, pTCompany->f_EMail);
	//m_pTblCompany->FromTable(pTCompany); 

	//TRY
	//{
	//	m_pTblCompany->Query();
	//
	//	while (!m_pTblCompany->IsEOF())
	//	{
	//		m_pTblCompany->MoveNext();
	//	}
	//}
	//CATCH (SqlException, e)
	//{
	//	CString err = e->m_strError;
	//}
	//END_CATCH

	//m_pTblCompany->Close();
	//SAFE_DELETE (m_pTblCompany);
	//
	//return pTCompany;
}

//---------------------------------------------------------------------------------------
BOOL CPostaliteAccountManagementWizardDoc::Subscribe()
{
	DataStr errorMessage;

	DataBool bPrivateEntity =
		(GetPostaLiteSettings()->m_PrivateEntity.IsEmpty() || GetPostaLiteSettings()->m_PrivateEntity.Str().CompareNoCase(_T("false")) == 0)
		? FALSE
		: TRUE;

	GetPostaLiteSettings()->m_TokenAuth = GetTbSenderInterface()->Subscribe
		(
		GetPostaLiteSettings()->m_CompanyName,
		GetPostaLiteSettings()->m_City, 
		GetPostaLiteSettings()->m_Address,
		GetPostaLiteSettings()->m_ZipCode,
		GetPostaLiteSettings()->m_County, 
		GetPostaLiteSettings()->m_Country,
		GetPostaLiteSettings()->m_AreaCodeTelephone,
		GetPostaLiteSettings()->m_Telephone,
		GetPostaLiteSettings()->m_TaxIdNumber, 
		GetPostaLiteSettings()->m_FiscalCode, 
		GetPostaLiteSettings()->m_EMail,
		GetPostaLiteSettings()->m_LegalStatusCode,
		GetPostaLiteSettings()->m_ActivityCode,
		GetPostaLiteSettings()->m_CadastralCode,
		bPrivateEntity,
		GetPostaLiteSettings()->m_PrivateEntityOption,
		GetPostaLiteSettings()->m_SenderCompanyName,
		GetPostaLiteSettings()->m_SenderCity,
		GetPostaLiteSettings()->m_SenderAddress,
		GetPostaLiteSettings()->m_SenderZipCode,
		GetPostaLiteSettings()->m_SenderCounty,
		GetPostaLiteSettings()->m_SenderCountry,
		GetPostaLiteSettings()->m_SenderTaxIdNumber,
		GetPostaLiteSettings()->m_SenderFiscalCode, 
		GetPostaLiteSettings()->m_SenderEMail,
		GetPostaLiteSettings()->m_SenderLegalStatusCode,
		GetPostaLiteSettings()->m_SenderActivityCode,
		GetPostaLiteSettings()->m_LoginId,
		errorMessage
		);

	if (GetPostaLiteSettings()->m_TokenAuth.IsEmpty() )
	{
		CString msg = errorMessage.IsEmpty() 
			? _TB("Error during subscribe operation")
			: errorMessage;

		GetPostaLiteMessages()->Add(msg, CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return FALSE;
	}

	if (GetPostaLiteSettings()->m_DefaultCountry.IsEmpty())
	{
		CString productIso = CPostaLiteNet::GetPostaliteCountryFromIsoCode(AfxGetLoginManager()->GetProductLanguage());
		GetPostaLiteSettings()->m_DefaultCountry = DataStr(productIso);
	}

	GetPostaLiteSettings()->m_AdviceOfDeliveryEmail = 
		(!GetPostaLiteSettings()->m_PrivateEntityOption.IsEmpty() &&
		GetPostaLiteSettings()->m_PrivateEntityOption.Str().CompareNoCase(_T("B")) == 0)
		? GetPostaLiteSettings()->m_SenderEMail
		: GetPostaLiteSettings()->m_EMail;

	SetDefaultAddresser();

	GetPostaLiteSettings()->m_Enabled = TRUE;
	GetPostaLiteSettings()->m_bFirstActivation = TRUE;

	GetPostaLiteSettings()->SaveSettings();			
	GetTbSenderInterface()->RefreshSettings();
	
	AfxTBMessageBox(_TB("Subscription completed: to complete the PostaLite account activation you will now be prompted to the credit management page"), MB_ICONINFORMATION | MB_OK);
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CPostaliteAccountManagementWizardDoc::CheckSubscribeData()
{
	BOOL bOk = TRUE;
	if (GetPostaLiteSettings()->m_CompanyName.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The company name field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_City.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The city field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_Address.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The address field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_ZipCode.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The zip Code field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_CadastralCode.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The cadastral code is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_County.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The county field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_Country.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The country field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_AreaCodeTelephone.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The telephone area code is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_Telephone.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The telephone number is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_TaxIdNumber.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The vat number is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (!PostaLiteStaticMethods::IsValidTaxIdNumber(GetPostaLiteSettings()->m_TaxIdNumber))
	{
		GetPostaLiteMessages()->Add(_TB("Invalid vat number"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_FiscalCode.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The fiscal code is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (!PostaLiteStaticMethods::IsValidFiscalCode(GetPostaLiteSettings()->m_FiscalCode))
	{
		GetPostaLiteMessages()->Add(_TB("Invalid fiscal code"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_EMail.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The e-mail address is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_LegalStatusCode.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The legal status code is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_ActivityCode.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The activity code is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (PostaLiteStaticMethods::TelephoneNumberHasInvalidChars(GetPostaLiteSettings()->m_Telephone))
	{
		GetPostaLiteMessages()->Add(_TB("Invalid characters in the telephone number field"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (PostaLiteStaticMethods::TelephoneNumberHasInvalidChars(GetPostaLiteSettings()->m_AreaCodeTelephone))
	{
		GetPostaLiteMessages()->Add(_TB("Invalid characters in the area code telephone number field"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (!PostaLiteStaticMethods::IsValidEmailAddress(GetPostaLiteSettings()->m_EMail))
	{
		GetPostaLiteMessages()->Add(_TB("Invalid e-mail address"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	
	return bOk;
}

//-----------------------------------------------------------------------------
void CPostaliteAccountManagementWizardDoc::ClearSenderData()
{
	GetPostaLiteSettings()->m_SenderCompanyName.Clear();
	GetPostaLiteSettings()->m_SenderCity.Clear();
	GetPostaLiteSettings()->m_SenderAddress.Clear();
	GetPostaLiteSettings()->m_SenderZipCode.Clear();
	GetPostaLiteSettings()->m_SenderCounty.Clear();
	GetPostaLiteSettings()->m_SenderCountry.Clear();
	GetPostaLiteSettings()->m_SenderTaxIdNumber.Clear();
	GetPostaLiteSettings()->m_SenderEMail.Clear();
	GetPostaLiteSettings()->m_SenderLegalStatusCode.Clear();
	GetPostaLiteSettings()->m_SenderActivityCode.Clear();
}

//-----------------------------------------------------------------------------
void CPostaliteAccountManagementWizardDoc::SetDefaultAddresser()
{
	CCompanyAddressInfo* pCompany = GetCompanyData();

	GetPostaLiteSettings()->m_AddresserType = E_POSTALITE_ADDRESSER_DEFAULT;
	GetPostaLiteSettings()->m_AddresserCompanyName = pCompany->f_CompanyName;
	GetPostaLiteSettings()->m_AddresserAddress = pCompany->f_Address;
	GetPostaLiteSettings()->m_AddresserZipCode = pCompany->f_ZIPCode;
	GetPostaLiteSettings()->m_AddresserCity = pCompany->f_City;
	GetPostaLiteSettings()->m_AddresserCounty = pCompany->f_County;
	GetPostaLiteSettings()->m_AddresserCountry = pCompany->f_County;
	SAFE_DELETE(pCompany);
}

//-----------------------------------------------------------------------------
void CPostaliteAccountManagementWizardDoc::ClearWireTransferData()
{
	GetPostaLiteSettings()->m_ChargeAmount.Clear();
	GetPostaLiteSettings()->m_ActivationAmount.Clear();
	GetPostaLiteSettings()->m_VatAmount.Clear();
	GetPostaLiteSettings()->m_TotalAmount.Clear();
	GetPostaLiteSettings()->m_Beneficiary.Clear();
	GetPostaLiteSettings()->m_Bank.Clear();
	GetPostaLiteSettings()->m_Iban.Clear();
	GetPostaLiteSettings()->m_WireTransferReason.Clear();
}

//-----------------------------------------------------------------------------
BOOL CPostaliteAccountManagementWizardDoc::CheckSenderData()
{
	if (!GetPostaLiteSettings()->m_PrivateEntityOption.Str().CompareNoCase(_T("B")) == 0)
		return TRUE;

	BOOL bOk = TRUE;
	if (GetPostaLiteSettings()->m_SenderCompanyName.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The company name field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}

	if (GetPostaLiteSettings()->m_SenderCity.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The city field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}

	if (GetPostaLiteSettings()->m_SenderAddress.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The address field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_SenderZipCode.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The sender zip code field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_SenderCounty.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The county field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_SenderCountry.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The country field is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_SenderTaxIdNumber.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The vat number is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (!PostaLiteStaticMethods::IsValidTaxIdNumber(GetPostaLiteSettings()->m_SenderTaxIdNumber))
	{
		GetPostaLiteMessages()->Add(_TB("Invalid vat number"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_SenderFiscalCode.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The fiscal code is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (!PostaLiteStaticMethods::IsValidFiscalCode(GetPostaLiteSettings()->m_SenderFiscalCode))
	{
		GetPostaLiteMessages()->Add(_TB("Invalid fiscal code"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_SenderEMail.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The e-mail address is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_SenderLegalStatusCode.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The legal status code is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (GetPostaLiteSettings()->m_SenderActivityCode.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The activity code is mandatory"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	if (!PostaLiteStaticMethods::IsValidEmailAddress(GetPostaLiteSettings()->m_SenderEMail))
	{
		GetPostaLiteMessages()->Add(_TB("Invalid e-mail address"), CMessages::MSG_ERROR);
		bOk = FALSE;
	}
	
	return bOk;
}

//---------------------------------------------------------------------------------------
BOOL CPostaliteAccountManagementWizardDoc::Login(DataStr& strPassword)
{
	DataStr errorMessage;
	DataBool bPrivateEntity;

	GetPostaLiteSettings()->m_TokenAuth = GetTbSenderInterface()->Login
		(
			GetPostaLiteSettings()->m_LoginId,
			strPassword,
			GetPostaLiteSettings()->m_CompanyName,
			GetPostaLiteSettings()->m_City,
			GetPostaLiteSettings()->m_Address,
			GetPostaLiteSettings()->m_ZipCode,
			GetPostaLiteSettings()->m_County,
			GetPostaLiteSettings()->m_Country,
			GetPostaLiteSettings()->m_AreaCodeTelephone,
			GetPostaLiteSettings()->m_Telephone,
			GetPostaLiteSettings()->m_TaxIdNumber,
			GetPostaLiteSettings()->m_FiscalCode,
			GetPostaLiteSettings()->m_EMail,
			GetPostaLiteSettings()->m_LegalStatusCode,
			GetPostaLiteSettings()->m_ActivityCode,
			GetPostaLiteSettings()->m_CadastralCode,
			bPrivateEntity,
			GetPostaLiteSettings()->m_PrivateEntityOption,
			GetPostaLiteSettings()->m_SenderCompanyName,
			GetPostaLiteSettings()->m_SenderCity,
			GetPostaLiteSettings()->m_SenderAddress,
			GetPostaLiteSettings()->m_SenderZipCode,
			GetPostaLiteSettings()->m_SenderCounty,
			GetPostaLiteSettings()->m_SenderCountry,
			GetPostaLiteSettings()->m_SenderTaxIdNumber,
			GetPostaLiteSettings()->m_SenderFiscalCode,
			GetPostaLiteSettings()->m_SenderEMail,
			GetPostaLiteSettings()->m_SenderLegalStatusCode,
			GetPostaLiteSettings()->m_SenderActivityCode,
			errorMessage
		);
	
	if (GetPostaLiteSettings()->m_TokenAuth.IsEmpty())
	{
		CString msg = errorMessage.IsEmpty() 
			? _TB("Error during login operation")
			: errorMessage;

		GetMessages()->Add(msg, CMessages::MSG_ERROR);
		GetMessages()->Show();
		return FALSE;
	}

	GetPostaLiteSettings()->m_AdviceOfDeliveryEmail = 
		(!GetPostaLiteSettings()->m_PrivateEntityOption.IsEmpty() &&
		GetPostaLiteSettings()->m_PrivateEntityOption.Str().CompareNoCase(_T("B")) == 0)
		? GetPostaLiteSettings()->m_SenderEMail
		: GetPostaLiteSettings()->m_EMail;

	SetDefaultAddresser();

	GetPostaLiteSettings()->m_PrivateEntity = bPrivateEntity ? _T("true") : _T("false");
		
	GetPostaLiteSettings()->m_Enabled = TRUE;

	GetPostaLiteSettings()->SaveSettings();

	GetTbSenderInterface()->RefreshSettings();
	return TRUE;
}

//---------------------------------------------------------------------------------------
void CPostaliteAccountManagementWizardDoc::Logout()
{
	AfxGetApp()->BeginWaitCursor();

	GetPostaLiteSettings()->ClearSubscription();

	GetTbSenderInterface()->RefreshSettings();

	AfxGetApp()->EndWaitCursor();
}


//---------------------------------------------------------------------------------------
void CPostaliteAccountManagementWizardDoc::ViewCredit(DataInt& nCodeState, DataMon& currentCredit, DataDate& expiryDate, DataStr& errorMsg)
{
	if (GetPostaLiteSettings()->m_TokenAuth.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The PostaLite service must be enabled and you have to be logged in to check your account status"), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}
	
	AfxGetApp()->BeginWaitCursor();
	
	currentCredit = GetTbSenderInterface()->GetCreditState(GetPostaLiteSettings()->m_LoginId, GetPostaLiteSettings()->m_TokenAuth, nCodeState, expiryDate, errorMsg);

	AfxGetApp()->EndWaitCursor();
}

//==============================================================================
//	CPostaliteAccountManagementWizardFrame
//==============================================================================
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CPostaliteAccountManagementWizardFrame, CWizardStepperBatchFrame)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPostaliteAccountManagementWizardFrame, CWizardStepperBatchFrame)
	//{{AFX_MSG_MAP(CPostaliteAccountManagementWizardFrame)
	ON_WM_GETMINMAXINFO()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
CPostaliteAccountManagementWizardFrame::CPostaliteAccountManagementWizardFrame()
{
	SetDockable(FALSE);	
}

//-----------------------------------------------------------------------------
void CPostaliteAccountManagementWizardFrame::OnGetMinMaxInfo(MINMAXINFO FAR* pMinMaxInfo)
{
	pMinMaxInfo->ptMinTrackSize.x = 800;
	pMinMaxInfo->ptMinTrackSize.y = 500;
}

//////////////////////////////////////////////////////////////////////////////////
//		CPostaliteAccountManagementWizardView implementation
//////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaliteAccountManagementWizardView, CWizardFormView)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPostaliteAccountManagementWizardView, CWizardFormView)
	ON_COMMAND	(IDC_WIZARD_FINISH,	OnWizardFinish)
	ON_COMMAND	(IDCANCEL,	OnWizardCancel)
END_MESSAGE_MAP() 

//-----------------------------------------------------------------------------
CPostaliteAccountManagementWizardDoc* CPostaliteAccountManagementWizardView::GetPostaliteAccountManagementWizardDoc() const
{
	ASSERT(GetDocument() && GetDocument()->IsKindOf(RUNTIME_CLASS(CPostaliteAccountManagementWizardDoc)));
	return (CPostaliteAccountManagementWizardDoc*)GetDocument();
}

//-----------------------------------------------------------------------------
CPostaliteAccountManagementWizardView::~CPostaliteAccountManagementWizardView()
{
	ClearStaticObjects();
}

//-----------------------------------------------------------------------------
void CPostaliteAccountManagementWizardView::OnInitialUpdate()
{
	__super::OnInitialUpdate();
	CFrameWnd* pWnd = this->GetParentFrame();
	CRect rect;
	pWnd->GetClientRect(rect);
	pWnd->SetWindowPos(NULL, 0, 0, rect.Width() + 80, rect.Height() + 200, SWP_NOMOVE| SWP_NOZORDER);
}

//-----------------------------------------------------------------------------
void CPostaliteAccountManagementWizardView::CustomizeTabWizard(CTabManager* pTabWizard)
{    
	if (!pTabWizard)
	{
		ASSERT(FALSE);
		return;
	}

	SetWizardBitmap(IDB_POSTALITE_WIZARD_BITMAP);

	pTabWizard->AddDialog(RUNTIME_CLASS(CPostaLiteWizardMainPage), IDD_POSTALITE_ACCOUNT_MANAGEMENT_MAIN_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CPostaLiteWizardNotLoggedPage), IDD_POSTALITE_ACCOUNT_MANAGEMENT_NOT_LOGGED_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CPostaLiteWizardLoggedPage), IDD_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CPostaLiteWizardLoginPage), IDD_POSTALITE_ACCOUNT_MANAGEMENT_LOGIN_PAGE);
	pTabWizard->AddDialog(RUNTIME_CLASS(CPostaLiteWizardSubscribePage1), IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE1);
	pTabWizard->AddDialog(RUNTIME_CLASS(CPostaLiteWizardSubscribePage2), IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE2);
	pTabWizard->AddDialog(RUNTIME_CLASS(CPostaLiteWizardSubscribePage3), IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE3);
	pTabWizard->AddDialog(RUNTIME_CLASS(CPostaLiteWizardSubscribePage4), IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE4);
	pTabWizard->AddDialog(RUNTIME_CLASS(CPostaLiteWizardCreditPage), IDD_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_PAGE);
}

//-----------------------------------------------------------------------------
void CPostaliteAccountManagementWizardView::OnWizardFinish()
{
	CWizardFormView::OnWizardFinish();
}

//-----------------------------------------------------------------------------
void CPostaliteAccountManagementWizardView::OnWizardCancel()
{
	CWizardFormView::OnWizardCancel();
}
//-----------------------------------------------------------------------------
LRESULT CPostaliteAccountManagementWizardView::OnWizardNext(UINT nIDD)
{
	return WIZARD_DEFAULT_TAB;
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardMainPage dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteWizardMainPage, CWizardTabDialog)

	BEGIN_MESSAGE_MAP(CPostaLiteWizardMainPage, CWizardTabDialog)
	//{{AFX_MSG_MAP(CPostaLiteWizardLogin)
	ON_WM_HELPINFO()
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------------------
CPostaLiteWizardMainPage::CPostaLiteWizardMainPage()   
	: CWizardTabDialog(_T("PostaLiteMainPage"), IDD_POSTALITE_ACCOUNT_MANAGEMENT_MAIN_PAGE)
{

}

//---------------------------------------------------------------------------------------
CPostaLiteWizardMainPage::~CPostaLiteWizardMainPage()   
{

}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardMainPage::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardMainPage::UpdateControls()
{
	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardMainPage::OnWizardNext()
{
	AfxGetApp()->BeginWaitCursor();

	LoadLegalInfos();

	AfxGetApp()->EndWaitCursor();

	if (GetPostaLiteSettings()->m_TokenAuth.IsEmpty())
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_NOT_LOGGED_PAGE;

	return IDD_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_PAGE;
}

//--------------------------------------------------------------------------
void CPostaLiteWizardMainPage::BuildDataControlLinks()
{
}

//--------------------------------------------------------------------------
void CPostaLiteWizardMainPage::OnShowHelp()
{
	ShowHelp(szHelpNamespace);
}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteWizardMainPage::OnHelpInfo (HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardNotLoggedPage dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteWizardNotLoggedPage, CWizardTabDialog)

	BEGIN_MESSAGE_MAP(CPostaLiteWizardNotLoggedPage, CWizardTabDialog)
	//{{AFX_MSG_MAP(CPostaLiteWizardLogin)
	ON_WM_HELPINFO()
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------------------
CPostaLiteWizardNotLoggedPage::CPostaLiteWizardNotLoggedPage()   
	: CWizardTabDialog(_T("PostaLiteNotLogged"), IDD_POSTALITE_ACCOUNT_MANAGEMENT_NOT_LOGGED_PAGE),
	m_bLoginRadio(FALSE),
	m_bSubscribeRadio(TRUE)
{

}

//---------------------------------------------------------------------------------------
CPostaLiteWizardNotLoggedPage::~CPostaLiteWizardNotLoggedPage()   
{

}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardNotLoggedPage::UpdateControls()
{
	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardNotLoggedPage::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardNotLoggedPage::OnWizardNext()
{
	if (m_bSubscribeRadio)
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE1;
		
	if (m_bLoginRadio)
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_LOGIN_PAGE;
	
	return WIZARD_DEFAULT_TAB;
}

//--------------------------------------------------------------------------
void CPostaLiteWizardNotLoggedPage::BuildDataControlLinks()
{
	AddLink (IDC_POSTALITE_ACCOUNT_MANAGEMENT_NOT_LOGGED_SUBSCRIBE_RADIO, _T("SubscribeRadio"), NULL, &m_bSubscribeRadio, RUNTIME_CLASS(CBoolButton));
	AddLink (IDC_POSTALITE_ACCOUNT_MANAGEMENT_NOT_LOGGED_LOGIN_RADIO, _T("LoginRadio"), NULL, &m_bLoginRadio, RUNTIME_CLASS(CBoolButton));
}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteWizardNotLoggedPage::OnHelpInfo (HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardNotLoggedPage dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteWizardLoggedPage, CWizardTabDialog)

	BEGIN_MESSAGE_MAP(CPostaLiteWizardLoggedPage, CWizardTabDialog)
	//{{AFX_MSG_MAP(CPostaLiteWizardLogin)

	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_LOGOUT_BUTTON, OnLogout)
	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_GO_TO_CREDIT_MNG_BUTTON, OnGoToCreditMng)
	ON_EN_VALUE_CHANGED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_ENABLED, OnChanged)

	ON_WM_HELPINFO()
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------------------
CPostaLiteWizardLoggedPage::CPostaLiteWizardLoggedPage()   
	: CWizardTabDialog(_T("PostaLiteLogged"), IDD_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_PAGE),
	m_bIsDirty(FALSE),
	m_bLogout(TRUE),
	m_bGoToCredit(TRUE)
{

}

//---------------------------------------------------------------------------------------
CPostaLiteWizardLoggedPage::~CPostaLiteWizardLoggedPage()   
{

}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardLoggedPage::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardLoggedPage::OnLogout()
{
	if (AfxTBMessageBox(_TB("Are you really sure you want to logout?"), MB_ICONWARNING | MB_OKCANCEL) != IDOK)
		return;

	GetWizardDoc()->Logout();
	UpdateControls();

	CWizardFormView* pView = ((CWizardFormView*)m_pFormView);
	if (pView)
		((CWizardFormView*)pView)->OnWizardBack();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardLoggedPage::OnGoToCreditMng()
{
	CWizardFormView* pView = ((CWizardFormView*)m_pFormView);
	if (pView)
		((CWizardFormView*)pView)->OnWizardNext();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardLoggedPage::OnChanged()
{
	m_bIsDirty = TRUE;
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardLoggedPage::UpdateControls()
{
	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardLoggedPage::OnWizardNext()
{
	return IDD_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_PAGE;
}

//--------------------------------------------------------------------------
void CPostaLiteWizardLoggedPage::BuildDataControlLinks()
{
	AddLink (IDC_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_LOGOUT_BUTTON, _T("LogoutButton"),  NULL, &m_bLogout, RUNTIME_CLASS(CPushButton));
	AddLink (IDC_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_GO_TO_CREDIT_MNG_BUTTON, _T("GoToCreditButton"),  NULL, &m_bGoToCredit, RUNTIME_CLASS(CPushButton));
	AddLink (IDC_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_SUBSCRIBER_LOGGED, _T("SubscriberLogged"), NULL, &GetPostaLiteSettings()->m_CompanyName,	RUNTIME_CLASS(CStrStatic));
	AddLink (IDC_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_SUBSCRIBER_LOGIN_ID_STATIC, _T("LogidIdStatic"), NULL, &GetPostaLiteSettings()->m_LoginId,	RUNTIME_CLASS(CStrStatic));
}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteWizardLoggedPage::OnHelpInfo (HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardLoginPage dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteWizardLoginPage, CWizardTabDialog)

BEGIN_MESSAGE_MAP(CPostaLiteWizardLoginPage, CWizardTabDialog)
	//{{AFX_MSG_MAP(CPostaLiteWizardLogin)
	ON_WM_HELPINFO()
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------------------
CPostaLiteWizardLoginPage::CPostaLiteWizardLoginPage()   
	: CWizardTabDialog(_T("PostaLiteLoginPage"), IDD_POSTALITE_ACCOUNT_MANAGEMENT_LOGIN_PAGE)
{

}

//---------------------------------------------------------------------------------------
CPostaLiteWizardLoginPage::~CPostaLiteWizardLoginPage()   
{

}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardLoginPage::UpdateControls()
{
	BOOL bIsReadOnly = !GetPostaLiteSettings()->m_TokenAuth.IsEmpty();
	GetPostaLiteSettings()->m_LoginId.SetReadOnly(bIsReadOnly);
	m_strPassword.SetReadOnly(bIsReadOnly);

	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardLoginPage::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardLoginPage::OnWizardNext()
{
	if (GetPostaLiteSettings()->m_LoginId.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("You need to provide a LoginId to continue"), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_LOGIN_PAGE;
	}

	AfxGetApp()->BeginWaitCursor();
	
	BOOL bOk = GetWizardDoc()->Login(m_strPassword);
	
	AfxGetApp()->EndWaitCursor();

	UpdateControls();

	return bOk 
		? IDD_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_PAGE
		: IDD_POSTALITE_ACCOUNT_MANAGEMENT_LOGIN_PAGE;
}

//--------------------------------------------------------------------------
void CPostaLiteWizardLoginPage::BuildDataControlLinks()
{
	CStrEdit* pCtrl = (CStrEdit*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_LOGIN_LOGINID, _T("LoginId"), NULL, &GetPostaLiteSettings()->m_LoginId, RUNTIME_CLASS(CStrEdit));
	pCtrl->SetCtrlMaxLen(9);
	
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_LOGIN_PASSWORD, _T("Password"), NULL, &m_strPassword, RUNTIME_CLASS(CStrEdit));
}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteWizardLoginPage::OnHelpInfo (HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardSubscribePage1 dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteWizardSubscribePage1, CWizardTabDialog)

BEGIN_MESSAGE_MAP(CPostaLiteWizardSubscribePage1, CWizardTabDialog)
	//{{AFX_MSG_MAP(CPostaLiteWizardLogin)
	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_LOAD_FROM_COMPANY, OnLoadCompanyDetails)
	ON_EN_VALUE_CHANGED	(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_CITY, OnCityChanged)

	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_GENERATE_CADASTRAL_CODE, OnGetCadastralCode)

	ON_WM_HELPINFO()
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//---------------------------------------------------------------------------------------
CPostaLiteWizardSubscribePage1::CPostaLiteWizardSubscribePage1()   
	: CWizardTabDialog(_T("PostaLiteSubscribe1"), IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE1),
		m_bLoadCompanyDefaultsButton(TRUE),
		m_bGetCadastralCodeButton(TRUE)
{

}

//---------------------------------------------------------------------------------------
CPostaLiteWizardSubscribePage1::~CPostaLiteWizardSubscribePage1()   
{

}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage1::UpdateControls()
{
	BOOL bIsReadOnly = !GetPostaLiteSettings()->m_TokenAuth.IsEmpty();
	GetPostaLiteSettings()->m_AreaCodeTelephone.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_Telephone.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_EMail.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_TaxIdNumber.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_CompanyName.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_Address.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_County.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_Country.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_City.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_ZipCode.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_CadastralCode.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_ActivityCode.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_LegalStatusCode.SetReadOnly(bIsReadOnly);

	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage1::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
}

//-----------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage1::OnCityChanged()
{
	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardSubscribePage1::OnWizardNext()
{
	if (!GetWizardDoc()->CheckSubscribeData())
	{
		GetPostaLiteMessages()->Show();
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE1;
	}

	//altrimenti salto alla pagina finale della sottoscrizione
	return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE2;
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardSubscribePage1::OnWizardCancel()
{
	if (AfxTBMessageBox(_TB("Subscription in progress, are you really sure you want to exit?"), MB_ICONWARNING | MB_OKCANCEL) != IDOK)
		return WIZARD_DEFAULT_TAB;

	return CWizardTabDialog::OnWizardCancel();
}

//--------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage1::BuildDataControlLinks()
{
	CMFCLinkCtrl* m_LearnMoreLinkCtrl = (CMFCLinkCtrl*)GetDlgItem(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_LEARN_MORE);
	m_LearnMoreLinkCtrl->SetURL(m_strCadastralPageUrl.Str());

	CPushButton* pLoadCompanyDefaultsButton = (CPushButton*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_LOAD_FROM_COMPANY, _T("LoadCompanyDefaultsButton"), NULL, &m_bLoadCompanyDefaultsButton, RUNTIME_CLASS(CBoolButton));
	pLoadCompanyDefaultsButton->SetToolTipBuffer(_TB("Load defaults from company master data"));
	pLoadCompanyDefaultsButton->LoadBitmaps(IDB_IMPORT_MASTER_DATA);

	CPushButton* pGetCadastralCodeButton = (CPushButton*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_GENERATE_CADASTRAL_CODE, _T("GetCadastralCodeButton"), NULL, &m_bGetCadastralCodeButton, RUNTIME_CLASS(CPushButton));
	pGetCadastralCodeButton->LoadBitmaps(IDB_POSTALITE_CADASTRALCODE);
	pGetCadastralCodeButton->SetToolTipBuffer(_TB("Tries to retrieve the cadastral based on the \"City\" field"));

	CXmlCombo* pCombo = (CXmlCombo*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_CITY, _T("City"), NULL, &GetPostaLiteSettings()->m_City, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.City"));
	pCombo->Attach(&GetPostaLiteSettings()->m_County,	_NS_DFEL("County"));
	pCombo->Attach(&GetPostaLiteSettings()->m_ZipCode, _NS_DFEL("ZIPCode"));

	pCombo = (CXmlCombo*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_COUNTY, _T("County"), NULL, &GetPostaLiteSettings()->m_County, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.County"));
	
	pCombo = (CXmlCombo*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_COUNTRY, _T("Country"), NULL, &GetPostaLiteSettings()->m_Country, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.State"));
	
	pCombo = (CXmlCombo*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_ACTIVITY, _T("Activity"), NULL, &GetPostaLiteSettings()->m_ActivityCode, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.Activity"));
	
	pCombo = (CXmlCombo*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_LEGAL_STATUS, _T("LegalStatus"), NULL, &GetPostaLiteSettings()->m_LegalStatusCode, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.LegalStatus"));

	CParsedCtrl* pCtrl = AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_AREA_CODE_TELEPHONE, _T("AreaCodeTelephone"), NULL, &GetPostaLiteSettings()->m_AreaCodeTelephone, RUNTIME_CLASS(CStrEdit));
	pCtrl->SetCtrlStyle(STR_STYLE_NUMBERS);
	
	pCtrl = AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_TELEPHONE, _T("Telephone"), NULL, &GetPostaLiteSettings()->m_Telephone, RUNTIME_CLASS(CStrEdit));
	pCtrl->SetCtrlStyle(STR_STYLE_NUMBERS);

	pCtrl = AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_FISCAL_CODE, _T("FiscalCode"), NULL, &GetPostaLiteSettings()->m_FiscalCode, RUNTIME_CLASS(CStrEdit));
	
	pCtrl = AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_TAXIDNUMBER, _T("TaxIdNumber"), NULL, &GetPostaLiteSettings()->m_TaxIdNumber, RUNTIME_CLASS(CStrEdit));
	pCtrl->SetCtrlStyle(STR_STYLE_NUMBERS);
	
	pCtrl = AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_AREACODE, _T("CadastralCode"), NULL, &GetPostaLiteSettings()->m_CadastralCode, RUNTIME_CLASS(CStrEdit));
	pCtrl->SetCtrlMaxLen(4);

	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_EMAIL, _T("EMail"), NULL, &GetPostaLiteSettings()->m_EMail, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_COMPANYNAME, _T("CompanyName"), NULL, &GetPostaLiteSettings()->m_CompanyName, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_ADDRESS, _T("Address"), NULL, &GetPostaLiteSettings()->m_Address, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE1_ZIPCODE, _T("ZipCode"), NULL, &GetPostaLiteSettings()->m_ZipCode, RUNTIME_CLASS(CStrEdit));
}

//-----------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage1::OnGetCadastralCode()
{
	DataStr errorMessage;

	//se è vuoto non faccio niente
	if (GetPostaLiteSettings()->m_City.IsEmpty())
		return;

	AfxGetApp()->BeginWaitCursor();

	GetPostaLiteSettings()->m_CadastralCode.Clear();

	CString cadastralCode = GetTbSenderInterface()->GetCadastralCode(GetPostaLiteSettings()->m_City, errorMessage);
	if (!cadastralCode.IsEmpty())
		GetPostaLiteSettings()->m_CadastralCode = cadastralCode;

	AfxGetApp()->EndWaitCursor();

	if (!errorMessage.IsEmpty())
	{
		GetPostaLiteMessages()->Add(errorMessage, CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}

	GetWizardDoc()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage1::OnLoadCompanyDetails()
{
	CCompanyAddressInfo* pCompany = GetWizardDoc()->GetCompanyData();
	GetPostaLiteSettings()->m_CompanyName = pCompany->f_CompanyName;
	GetPostaLiteSettings()->m_City = pCompany->f_City;
	GetPostaLiteSettings()->m_Address = pCompany->f_Address;
	GetPostaLiteSettings()->m_ZipCode = pCompany->f_ZIPCode;
	GetPostaLiteSettings()->m_County = pCompany->f_County;
	GetPostaLiteSettings()->m_Country = pCompany->f_Country;
	GetPostaLiteSettings()->m_TaxIdNumber = pCompany->f_TaxIdNumber;
	GetPostaLiteSettings()->m_EMail = pCompany->f_EMail;
	GetPostaLiteSettings()->m_FiscalCode = pCompany->f_FiscalCode;
	
	UpdateControls();

	SAFE_DELETE(pCompany);
}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteWizardSubscribePage1::OnHelpInfo (HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardSubscribePage2 dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteWizardSubscribePage2, CWizardTabDialog)

BEGIN_MESSAGE_MAP(CPostaLiteWizardSubscribePage2, CWizardTabDialog)
	//{{AFX_MSG_MAP(CPostaLiteWizardSubscribePage2)
	ON_EN_VALUE_CHANGED	(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE2_PUBLIC_ENTITY_RADIO, UpdateControls)
	ON_EN_VALUE_CHANGED	(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE2_NOT_PUBLIC_ENTITY_RADIO, UpdateControls)

	ON_WM_HELPINFO()
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//---------------------------------------------------------------------------------------
CPostaLiteWizardSubscribePage2::CPostaLiteWizardSubscribePage2()   
	: CWizardTabDialog(_T("PostaLiteSubscribe2"), IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE2)
{
	m_bPublicEntity = 
		GetPostaLiteSettings()->m_PrivateEntity.IsEmpty() 
		? FALSE
		: GetPostaLiteSettings()->m_PrivateEntity.Str().CompareNoCase(_T("False")) == 0;

	m_bNotPublicEntity =
		GetPostaLiteSettings()->m_PrivateEntity.IsEmpty() 
		? FALSE
		: GetPostaLiteSettings()->m_PrivateEntity.Str().CompareNoCase(_T("True")) == 0;

	m_bSenderPublicEntity  = 
		GetPostaLiteSettings()->m_PrivateEntityOption.IsEmpty()
		? FALSE
		: GetPostaLiteSettings()->m_PrivateEntityOption.Str().CompareNoCase(_T("B")) == 0;

	m_bSenderNotPublicEntity = 
		GetPostaLiteSettings()->m_PrivateEntityOption.IsEmpty()
		? FALSE
		: GetPostaLiteSettings()->m_PrivateEntityOption.Str().CompareNoCase(_T("A")) == 0;
}

//---------------------------------------------------------------------------------------
CPostaLiteWizardSubscribePage2::~CPostaLiteWizardSubscribePage2()   
{
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage2::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage2::UpdateControls()
{
	if (m_bNotPublicEntity)
	{
		m_bSenderPublicEntity.SetReadOnly(FALSE);
		m_bSenderNotPublicEntity.SetReadOnly(FALSE);
	}
	else
	{
		m_bSenderNotPublicEntity = FALSE;
		m_bSenderPublicEntity = FALSE;
		m_bSenderPublicEntity.SetReadOnly(TRUE);
		m_bSenderNotPublicEntity.SetReadOnly(TRUE);
	}
	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardSubscribePage2::OnWizardNext()
{
	if (!m_bPublicEntity && !m_bNotPublicEntity)
	{
		GetPostaLiteMessages()->Add(_TB("You need to either specify if you are or are not a Public Entity "), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE2;
	}

	if (m_bNotPublicEntity && !m_bSenderPublicEntity && !m_bSenderNotPublicEntity)
	{
		GetPostaLiteMessages()->Add(_TB("You need to specify if the private entity refers or not to public works/services/suppliers"), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE2;
	}

	GetPostaLiteSettings()->m_PrivateEntity = m_bPublicEntity ? _T("False") : _T("True");
	
	GetPostaLiteSettings()->m_PrivateEntityOption = m_bSenderPublicEntity 
		? _T("B")
		: m_bSenderNotPublicEntity 
			? _T("A")
			: _T("");

	if (m_bNotPublicEntity &&  	GetPostaLiteSettings()->m_PrivateEntityOption.Str().CompareNoCase(_T("B")) == 0)
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE3;

	return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE4;
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardSubscribePage2::OnWizardBack()
{
	GetPostaLiteSettings()->m_PrivateEntity = m_bPublicEntity ? _T("False") : _T("True");
	
	GetPostaLiteSettings()->m_PrivateEntityOption = m_bSenderPublicEntity 
		? _T("B")
		: m_bSenderNotPublicEntity 
			? _T("A")
			: _T("");
		
	return CWizardTabDialog::OnWizardBack();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardSubscribePage2::OnWizardCancel()
{
	if (AfxTBMessageBox(_TB("Subscription in progress, are you really sure you want to exit?"), MB_ICONWARNING | MB_OKCANCEL) != IDOK)
		return WIZARD_DEFAULT_TAB;

	return CWizardTabDialog::OnWizardCancel();
}

//--------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage2::BuildDataControlLinks()
{
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE2_PUBLIC_ENTITY_RADIO, _T("PublicEntity"), NULL, &m_bPublicEntity, RUNTIME_CLASS(CBoolButton));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE2_NOT_PUBLIC_ENTITY_RADIO, _T("NotPublicEntity"), NULL, &m_bNotPublicEntity, RUNTIME_CLASS(CBoolButton));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE2_SENDER_PUBLIC_ENTITY_RADIO, _T("SenderPublicEntity"), NULL, &m_bSenderPublicEntity, RUNTIME_CLASS(CBoolButton));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE2_SENDER_NOT_PUBLIC_ENTITY_RADIO, _T("SenderNotPublicEntity"), NULL, &m_bSenderNotPublicEntity, RUNTIME_CLASS(CBoolButton));
	
	CStrEdit* pCtrl = (CStrEdit*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE2_LAW_BOX, _T("LawBoxCtrl"), NULL, &m_strLaw136, RUNTIME_CLASS(CStrEdit));
	pCtrl->SetOwnFont(FALSE, TRUE, FALSE, 8);

}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteWizardSubscribePage2::OnHelpInfo (HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardSubscribePage3 dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteWizardSubscribePage3, CWizardTabDialog)

BEGIN_MESSAGE_MAP(CPostaLiteWizardSubscribePage3, CWizardTabDialog)
	//{{AFX_MSG_MAP(CPostaLiteWizardLogin)
	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_LOAD_FROM_COMPANY, OnLoadSenderCompanyDetails)
	ON_EN_VALUE_CHANGED	(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_CITY, OnCityChanged)
	ON_WM_HELPINFO()
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------------------
CPostaLiteWizardSubscribePage3::CPostaLiteWizardSubscribePage3()   
	: CWizardTabDialog(_T("PostaLiteSubscribe3"), IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE3),
	m_bLoadSenderCompanyDefaultsButton (TRUE)
{

}

//---------------------------------------------------------------------------------------
CPostaLiteWizardSubscribePage3::~CPostaLiteWizardSubscribePage3()   
{

}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage3::UpdateControls()
{
	BOOL bIsReadOnly = !GetPostaLiteSettings()->m_TokenAuth.IsEmpty();
	
	//Sender
	GetPostaLiteSettings()->m_SenderEMail.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_SenderTaxIdNumber.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_SenderCompanyName.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_SenderAddress.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_SenderCounty.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_SenderCountry.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_SenderCity.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_SenderZipCode.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_SenderActivityCode.SetReadOnly(bIsReadOnly);
	GetPostaLiteSettings()->m_SenderLegalStatusCode.SetReadOnly(bIsReadOnly);
	
	m_bLoadSenderCompanyDefaultsButton.SetReadOnly(bIsReadOnly);

	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage3::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardSubscribePage3::OnWizardNext()
{
	if (!GetWizardDoc()->CheckSenderData())
	{
		GetPostaLiteMessages()->Show();
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE3;
	}

	return WIZARD_DEFAULT_TAB;
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardSubscribePage3::OnWizardCancel()
{
	if (AfxTBMessageBox(_TB("Subscription in progress, are you really sure you want to exit?"), MB_ICONWARNING | MB_OKCANCEL) != IDOK)
		return WIZARD_DEFAULT_TAB;

	return CWizardTabDialog::OnWizardCancel();
}

//--------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage3::BuildDataControlLinks()
{
	CPushButton* pLoadSenderCompanyDefaultsButton = (CPushButton*) AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_LOAD_FROM_COMPANY, _T("LoadSenderCompanyDefaultsButton"), NULL, &m_bLoadSenderCompanyDefaultsButton, RUNTIME_CLASS(CBoolButton));
	pLoadSenderCompanyDefaultsButton->SetToolTipBuffer(_TB("Load defaults from company master data"));
	pLoadSenderCompanyDefaultsButton->LoadBitmaps(IDB_IMPORT_MASTER_DATA); 

	CXmlCombo* pCombo = (CXmlCombo*) AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_CITY, _T("SenderCity"), NULL, &GetPostaLiteSettings()->m_SenderCity, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.City"));
	pCombo->Attach(&GetPostaLiteSettings()->m_SenderCounty,	_NS_DFEL("County"));
	pCombo->Attach(&GetPostaLiteSettings()->m_SenderZipCode, _NS_DFEL("ZIPCode"));
	
	pCombo = (CXmlCombo*) AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_COUNTY, _T("SenderCounty"), NULL, &GetPostaLiteSettings()->m_SenderCounty, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.County"));
	
	pCombo = (CXmlCombo*) AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_COUNTRY, _T("SenderCountry"), NULL, &GetPostaLiteSettings()->m_SenderCountry, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.State"));
	
	pCombo = (CXmlCombo*) AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_ACTIVITY, _T("SenderActivity"), NULL, &GetPostaLiteSettings()->m_SenderActivityCode, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.Activity"));
	
	pCombo = (CXmlCombo*) AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_LEGAL_STATUS, _T("SenderLegalStatus"), NULL, &GetPostaLiteSettings()->m_SenderLegalStatusCode, RUNTIME_CLASS(CXmlCombo));
	pCombo->UseProductLanguage(TRUE);
	pCombo->SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.LegalStatus"));
	
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_EMAIL, _T("SenderEMail"), NULL, &GetPostaLiteSettings()->m_SenderEMail, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_COMPANYNAME, _T("SenderCompanyName"), NULL, &GetPostaLiteSettings()->m_SenderCompanyName, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_ADDRESS, _T("SenderAddress"), NULL, &GetPostaLiteSettings()->m_SenderAddress, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_ZIPCODE, _T("SenderZipCode"), NULL, &GetPostaLiteSettings()->m_SenderZipCode, RUNTIME_CLASS(CStrEdit));

	CStrEdit* pEdit = (CStrEdit*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_TAXIDNUMBER, _T("SenderTaxIdNumber"), NULL, &GetPostaLiteSettings()->m_SenderTaxIdNumber, RUNTIME_CLASS(CStrEdit));
	pEdit->SetCtrlStyle(STR_STYLE_NUMBERS);
	
	pEdit = (CStrEdit*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE3_SENDER_FISCAL_CODE, _T("SenderFiscalCode"), NULL, &GetPostaLiteSettings()->m_SenderFiscalCode, RUNTIME_CLASS(CStrEdit));
}

//-----------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage3::OnCityChanged()
{
	GetDocument()->UpdateDataView();
}

//-----------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage3::OnLoadSenderCompanyDetails()
{
	CCompanyAddressInfo* pTCompany = GetWizardDoc()->GetCompanyData();
	GetPostaLiteSettings()->m_SenderCompanyName = pTCompany->f_CompanyName;
	GetPostaLiteSettings()->m_SenderCity = pTCompany->f_City; 
	GetPostaLiteSettings()->m_SenderAddress = pTCompany->f_Address; 
	GetPostaLiteSettings()->m_SenderZipCode = pTCompany->f_ZIPCode; 
	GetPostaLiteSettings()->m_SenderCounty = pTCompany->f_County; 
	GetPostaLiteSettings()->m_SenderCountry = pTCompany->f_Country; 
	GetPostaLiteSettings()->m_SenderTaxIdNumber = pTCompany->f_TaxIdNumber; 
	GetPostaLiteSettings()->m_SenderEMail = pTCompany->f_EMail; 
	GetPostaLiteSettings()->m_SenderFiscalCode = pTCompany->f_FiscalCode; 

	UpdateControls();

	SAFE_DELETE (pTCompany);
}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteWizardSubscribePage3::OnHelpInfo (HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardSubscribePage4 dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteWizardSubscribePage4, CWizardTabDialog)

BEGIN_MESSAGE_MAP(CPostaLiteWizardSubscribePage4, CWizardTabDialog)
	//{{AFX_MSG_MAP(CPostaLiteWizardLogin)

	ON_EN_VALUE_CHANGED	(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_CLAUSE1_CHECK, UpdateControls)
	ON_EN_VALUE_CHANGED	(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_CLAUSE2_CHECK, UpdateControls)
	ON_EN_VALUE_CHANGED	(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_PRIVACY_POLICY_CHECK, UpdateControls)

	ON_WM_HELPINFO()
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------------------
CPostaLiteWizardSubscribePage4::CPostaLiteWizardSubscribePage4()   
	: CWizardTabDialog(_T("PostaLiteSubscribe4"), IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE4),
	 m_bAcceptTos(FALSE),
	 m_bAcceptRestrictiveClauses(FALSE),
	 m_bAcceptPrivayPolicy(FALSE)
{

}

//---------------------------------------------------------------------------------------
CPostaLiteWizardSubscribePage4::~CPostaLiteWizardSubscribePage4()   
{
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage4::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage4::UpdateControls()
{
	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardSubscribePage4::OnWizardNext()
{
	
	//Check dei flag
	if (!m_bAcceptTos || !m_bAcceptRestrictiveClauses)
	{
		GetPostaLiteMessages()->Add(_TB("You need to read and accept the PostaLite terms and conditions to complete the subscription"), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE4;
	}

	if (!m_bAcceptPrivayPolicy)
	{
		GetPostaLiteMessages()->Add(_TB("You need to read and accept the privacy policy terms"), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE4;
	}

	//Se per qualche giro ho riempito i dati del sender, ma a fine wizard non siamo in condizione 2b, allora cancello
	//i dati del sender
	if (
		GetPostaLiteSettings()->m_PrivateEntity.Str().CompareNoCase(_T("False")) == 0 || 
		GetPostaLiteSettings()->m_PrivateEntityOption.Str().CompareNoCase(_T("A")) == 0 
		)
		GetWizardDoc()->ClearSenderData();

	AfxGetApp()->BeginWaitCursor();
	BOOL bOk = GetWizardDoc()->Subscribe();

	AfxGetApp()->EndWaitCursor();
	
	return bOk 
		? WIZARD_DEFAULT_TAB
		: IDD_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE_PAGE4;
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardSubscribePage4::OnWizardCancel()
{
	if (AfxTBMessageBox(_TB("Subscription in progress, are you really sure you want to exit?"), MB_ICONWARNING | MB_OKCANCEL) != IDOK)
		return WIZARD_DEFAULT_TAB;

	return CWizardTabDialog::OnWizardCancel();
}

//--------------------------------------------------------------------------
void CPostaLiteWizardSubscribePage4::BuildDataControlLinks()
{
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_CLAUSE1_CHECK, _T("AcceptTosCheck"), NULL, &m_bAcceptTos, RUNTIME_CLASS(CBoolButton));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_CLAUSE2_CHECK, _T("AcceptRestrictiveClausesCheck"), NULL, &m_bAcceptRestrictiveClauses, RUNTIME_CLASS(CBoolButton));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_PRIVACY_POLICY_CHECK, _T("AcceptPrivayPolicyCheck"), NULL, &m_bAcceptPrivayPolicy, RUNTIME_CLASS(CBoolButton));

	CMFCLinkCtrl* pCurrentLink = (CMFCLinkCtrl*)GetDlgItem(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_TOS_LINK);
	pCurrentLink->SetURL(m_strTermsOfUseUrl.Str());

	pCurrentLink = (CMFCLinkCtrl*)GetDlgItem(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_PRIVACY_POLICY_LINK);
	pCurrentLink->SetURL(m_strPrivacyPolicyUrl.Str());

	pCurrentLink = (CMFCLinkCtrl*)GetDlgItem(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_PRICE_LIST_LINK);
	pCurrentLink->SetURL(m_strPriceListUrl.Str());

	m_strRestrictiveClause1 = _TB("I know the present contract is exclusively regulated by Italian law and to have read and I fully accept the Terms and Conditions of the \"PostaLite\" service.");
	m_strRestrictiveClause1 += _T("(") + m_strTermsOfUseUrl + _T(")") ;
	CStrEdit* pCtrl1 = (CStrEdit*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_CLAUSE1_EDIT, _T("RestrictiveClauses1Ctrl"), NULL, &m_strRestrictiveClause1, RUNTIME_CLASS(CStrEdit));
	pCtrl1->SetOwnFont(FALSE, TRUE, FALSE, 8);

	CStrEdit* pCtrl2 = (CStrEdit*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_CLAUSE2_EDIT, _T("RestrictiveClauses2Ctrl"), NULL, &m_strRestrictiveClauses, RUNTIME_CLASS(CStrEdit));
	pCtrl2->SetOwnFont(FALSE, TRUE, FALSE, 8);

	m_strPrivacyPolicyClause = _TB("I have read and accepted the Privacy Policy terms");
	CStrEdit* pCtrl3 = (CStrEdit*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_SUBSCRIBE4_PRIVACY_POLICY_EDIT, _T("PrivacyPolicyCtrl"), NULL, &m_strPrivacyPolicyClause, RUNTIME_CLASS(CStrEdit));
	pCtrl3->SetOwnFont(FALSE, TRUE, FALSE, 8);
}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteWizardSubscribePage4::OnHelpInfo (HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteWizardSubscribePage4 dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteWizardCreditPage, CWizardTabDialog)

BEGIN_MESSAGE_MAP(CPostaLiteWizardCreditPage, CWizardTabDialog)
	//{{AFX_MSG_MAP(CPostaLiteWizardLogin)
	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_VIEWCREDIT, OnViewCredit)
	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_CHARGE, OnCharge)
	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_BROWSE_FILES, OnBrowse)
	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_WIRE_TRANSFER_DETAILS, OnGetWireTransferDetails)
	ON_BN_CLICKED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_CLEAR_WIRE_TRANSFER_DETAILS, OnClearWireTransferDetails)

	ON_EN_VALUE_CHANGED(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_CHARGE_AMOUT, OnChargeAmountChanged)

	ON_WM_HELPINFO()
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------------------
CPostaLiteWizardCreditPage::CPostaLiteWizardCreditPage()   
	: CWizardTabDialog(_T("PostaLiteCredit"), IDD_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_PAGE),
	m_nCodeState					(),
	m_CurrentCredit					(),
	m_CreditExpiryDate				(),
	m_bViewCurrentCredit			(TRUE),
	m_bBrowseFile					(TRUE),
	m_bChargeFile					(TRUE),
	m_bGetWireTransferDetails		(TRUE),
	m_bResetWireTransferDetails		(TRUE),
	m_bModify						(TRUE),
	m_bAcceptRechargeCondition		(FALSE)
{
}

//---------------------------------------------------------------------------------------
CPostaLiteWizardCreditPage::~CPostaLiteWizardCreditPage()   
{
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::UpdateControls()
{
	m_bViewCurrentCredit.SetReadOnly(GetPostaLiteSettings()->m_TokenAuth.IsEmpty());
	
	BOOL bEnable = 
		!GetPostaLiteSettings()->m_TokenAuth.IsEmpty() &&
		(m_nCodeState == CPostaLiteAddress::Enabled || GetPostaLiteSettings()->m_bFirstActivation);

	//attivo solo se prima hai calcolato i dati del bonifico
	m_bBrowseFile = bEnable && GetPostaLiteSettings()->m_TotalAmount > 0;
	m_bChargeFile = bEnable && GetPostaLiteSettings()->m_TotalAmount > 0;
	m_bGetWireTransferDetails = bEnable;
	m_bResetWireTransferDetails = bEnable && GetPostaLiteSettings()->m_TotalAmount > 0;

	m_bAcceptRechargeCondition.SetReadOnly(!bEnable || GetPostaLiteSettings()->m_TotalAmount <= 0);

	m_strFileName.SetReadOnly(!bEnable || GetPostaLiteSettings()->m_TotalAmount <= 0);
	GetPostaLiteSettings()->m_ChargeAmount.SetReadOnly(!bEnable);

	GetDocument()->UpdateDataView();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardCreditPage::OnWizardNext()
{
	return WIZARD_DEFAULT_TAB;
}

//---------------------------------------------------------------------------------------
LRESULT CPostaLiteWizardCreditPage::OnWizardBack()
{
	return IDD_POSTALITE_ACCOUNT_MANAGEMENT_LOGGED_PAGE;
}

//--------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::BuildDataControlLinks()
{
	CWnd* pWnd =  GetDlgItem(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_ACCOUNT_STATUS_GROUP);
	if (pWnd)
	{
		CString current;
		pWnd->GetWindowText(current);
		current += _TB(" Login Id: ") + GetPostaLiteSettings()->m_LoginId;
		pWnd->SetWindowText(current);
	}

	m_TempChargeAmount = GetPostaLiteSettings()->m_ChargeAmount;

	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_VIEWCREDIT, _T("LoginRadio"), NULL, &m_bViewCurrentCredit, RUNTIME_CLASS(CPushButton));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_BROWSE_FILES, _T("BrowseFileButton"), NULL, &m_bBrowseFile, RUNTIME_CLASS(CPushButton));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_CHARGE, _T("ChargeFileButton"), NULL, &m_bChargeFile, RUNTIME_CLASS(CPushButton));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_WIRE_TRANSFER_DETAILS, _T("GetWireTranferButton"), NULL, &m_bGetWireTransferDetails, RUNTIME_CLASS(CPushButton));

	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_CURRENTCREDIT, _T("CurrentCredit"), NULL, &m_CurrentCredit, RUNTIME_CLASS(CMoneyStatic));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_FILEPATH, _T("FilePath"), NULL, &m_strFileName, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_CODESTATE, _T("CodeState"), NULL, &m_strCodeStateString, RUNTIME_CLASS(CStrStatic));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_CREDIT_EXPIRY_DATE, _T("CreditExpiryDate"), NULL, &m_CreditExpiryDate, RUNTIME_CLASS(CDateStatic));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_AMOUT_CHARGE_STATIC, _T("ChargeAmountStaticCtrl"), NULL, &m_TempChargeAmount, RUNTIME_CLASS(CPostaLiteMoneyEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_ACTIVATION_AMOUNT_STATIC, _T("ActivationAmountStaticCtrl"), NULL, &GetPostaLiteSettings()->m_ActivationAmount, RUNTIME_CLASS(CPostaLiteMoneyEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_VAT_AMOUNT_STATIC, _T("VatAmountStaticCtrl"), NULL, &GetPostaLiteSettings()->m_VatAmount, RUNTIME_CLASS(CPostaLiteMoneyEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_TOTAL_AMOUT_STATIC, _T("TotalAmountStaticCtrl"), NULL, &GetPostaLiteSettings()->m_TotalAmount, RUNTIME_CLASS(CPostaLiteMoneyEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_BENEFICIARY_STATIC, _T("BeneficiaryStaticCtrl"), NULL, &GetPostaLiteSettings()->m_Beneficiary, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_BANK_STATIC, _T("BankStaticCtrl"), NULL, &GetPostaLiteSettings()->m_Bank, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_IBAN_STATIC, _T("IbanStaticCtrl"), NULL, &GetPostaLiteSettings()->m_Iban, RUNTIME_CLASS(CStrEdit));
	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_WIRE_TRANSFER_REASON_STATIC, _T("WireTransferReasonStaticCtrl"), NULL, &GetPostaLiteSettings()->m_WireTransferReason, RUNTIME_CLASS(CStrEdit));

	AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_CLEAR_WIRE_TRANSFER_DETAILS, _T("ResetDetailsCtrl"), NULL, &m_bResetWireTransferDetails, RUNTIME_CLASS(CPushButton));
	
	CMoneyEdit* pAmount = (CMoneyEdit*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_CHARGE_AMOUT, _T("ChargeAmountCtrl"), NULL, &GetPostaLiteSettings()->m_ChargeAmount, RUNTIME_CLASS(CMoneyEdit));
	pAmount->SetMaxValue(DataMon(10000));
	pAmount->SetMinValue(DataMon(0));

	CBoolButton* pButton = (CBoolButton*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_RECHARGE_CONDITIONS_CHECK, _T("AcceptRechargeConditionCheck"), NULL, &m_bAcceptRechargeCondition, RUNTIME_CLASS(CBoolButton));
	if (GetPostaLiteSettings()->m_bFirstActivation)
		pButton->ShowCtrl(SW_HIDE);

	CStrEdit* pEdit= (CStrEdit*)AddLink(IDC_POSTALITE_ACCOUNT_MANAGEMENT_CREDIT_RECHARGE_CONDITIONS, _T("AcceptRechargeCtrl"), NULL, &m_strAcceptRecharge, RUNTIME_CLASS(CStrEdit));
	pEdit->SetOwnFont(FALSE, TRUE, FALSE, 8);

	if (GetPostaLiteSettings()->m_bFirstActivation)
		pEdit->ShowCtrl(SW_HIDE);

	ViewCredit();
}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteWizardCreditPage::OnHelpInfo (HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::ViewCredit()
{
	DataStr errorMsg;
	GetWizardDoc()->ViewCredit(m_nCodeState, m_CurrentCredit, m_CreditExpiryDate, errorMsg);	
	
	m_strCodeStateString = PostaLiteDecodeCodeState(m_nCodeState);
	
	if (!errorMsg.IsEmpty())
	{
		GetPostaLiteMessages()->Add(errorMsg, CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::OnViewCredit()
{
	ViewCredit();

	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::OnCharge()
{
	if (GetPostaLiteSettings()->m_TokenAuth.IsEmpty())
	{
		GetPostaLiteMessages()->Add(_TB("The PostaLite service must be enabled and you have to be logged in to recharge your account"), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}

	if (m_strFileName.IsEmpty() || !ExistFile(m_strFileName))
	{
		GetPostaLiteMessages()->Add(_TB("The provided pdf file does not exist"), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}

	if (!GetPostaLiteSettings()->m_bFirstActivation && !m_bAcceptRechargeCondition)
	{
		GetPostaLiteMessages()->Add(_TB("You need to accept the recharge conditions"), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}

	if (GetPostaLiteSettings()->m_TotalAmount <= 0)
	{
		GetPostaLiteMessages()->Add(_TB("The total amount of the recharge must be a valid value"), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}

	CString strPreUploadMessage = _TB("You are about to upload the wire transfer pdf with the following details:\r\n");
	strPreUploadMessage += cwsprintf(_TB("Recharge amount: %s€\r\nVat amount: %s%%\r\n"), GetPostaLiteSettings()->m_ChargeAmount.FormatData(), GetPostaLiteSettings()->m_VatAmount.FormatData());
	if (GetPostaLiteSettings()->m_bFirstActivation)
		strPreUploadMessage += cwsprintf(_TB("Activation amount: %s€\r\n"), GetPostaLiteSettings()->m_ActivationAmount.FormatData());
	strPreUploadMessage +=  cwsprintf(_TB("Total amount: %s€\r\nDo you confirm the operation?"), GetPostaLiteSettings()->m_TotalAmount.FormatData());
	if (AfxTBMessageBox(strPreUploadMessage, MB_ICONWARNING | MB_OKCANCEL) != IDOK)
		return;

	CString strConvertFile = ConvertToBase64(m_strFileName.Str());
	if (strConvertFile.IsEmpty())
	{
		AfxTBMessageBox(_TB("Error converting wire transfer pdf"), MB_ICONWARNING | MB_OK);
		return;
	}

	DataStr errorMsg;
	DataMon amountCharge;
	DataMon vat;
	DataMon amountActivation;
	DataMon totalAmount;

	AfxGetApp()->BeginWaitCursor();
	m_nCodeState = GetTbSenderInterface()->Charge
		(
			GetPostaLiteSettings()->m_LoginId, 
			GetPostaLiteSettings()->m_TokenAuth,
			DataStr(strConvertFile),
			GetPostaLiteSettings()->m_ChargeAmount,
			GetPostaLiteSettings()->m_VatAmount,
			GetPostaLiteSettings()->m_ActivationAmount,
			GetPostaLiteSettings()->m_TotalAmount,
			m_CurrentCredit, 
			m_CreditExpiryDate, 
			errorMsg
		);

	AfxGetApp()->EndWaitCursor();
	
	if (!errorMsg.IsEmpty())
	{
		GetPostaLiteMessages()->Add(errorMsg, CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}

	AfxTBMessageBox(_TB("Operation successfully completed: you will receive a confirmation  e-mail that your credit is available once the wire transfer is validated."), MB_ICONINFORMATION | MB_OK);

	m_strCodeStateString = PostaLiteDecodeCodeState(m_nCodeState);

	//ad upload completato, resetto i campi di ricarica
	m_TempChargeAmount.Clear();
	m_bAcceptRechargeCondition.Clear();
	GetPostaLiteSettings()->m_ChargeAmount.Clear();
	GetPostaLiteSettings()->m_ActivationAmount.Clear();
	GetPostaLiteSettings()->m_VatAmount.Clear();
	GetPostaLiteSettings()->m_TotalAmount.Clear();
	GetPostaLiteSettings()->m_Beneficiary.Clear();
	GetPostaLiteSettings()->m_Bank.Clear();
	GetPostaLiteSettings()->m_Iban.Clear();
	GetPostaLiteSettings()->m_WireTransferReason.Clear();
	
	//al termine della prima ricarica lo stato di prima attivazione viene messo a false per non tornarci più
	GetPostaLiteSettings()->m_bFirstActivation = FALSE;
	GetPostaLiteSettings()->m_Enabled = TRUE;

	GetPostaLiteSettings()->SaveSettings();
	GetTbSenderInterface()->RefreshSettings();

	m_strFileName.Clear();
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::OnBrowse()
{
	CFileDialog dialog(FALSE, _T("*.pdf"), _T(""), OFN_FILEMUSTEXIST,  _T("Pdf files (*.pdf)|*.pdf"));
	
	if ((dialog.DoModal() != IDOK))
		return;
		 
	m_strFileName = dialog.GetPathName();

	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::OnChargeAmountChanged()
{
	m_TempChargeAmount.Clear();
	GetPostaLiteSettings()->m_ActivationAmount.Clear();
	GetPostaLiteSettings()->m_VatAmount.Clear();
	GetPostaLiteSettings()->m_TotalAmount.Clear();
	GetPostaLiteSettings()->m_Beneficiary.Clear();
	GetPostaLiteSettings()->m_Bank.Clear();
	GetPostaLiteSettings()->m_Iban.Clear();
	GetPostaLiteSettings()->m_WireTransferReason.Clear();

	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::OnClearWireTransferDetails()
{
	m_TempChargeAmount.Clear();
	GetWizardDoc()->ClearWireTransferData();

	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteWizardCreditPage::OnGetWireTransferDetails()
{
	DataStr errorMsg;

	if (GetPostaLiteSettings()->m_ChargeAmount < m_MinimumRecharge)
	{
		GetPostaLiteMessages()->Add(cwsprintf(_TB("The recharge amount must be at least %s€"), m_MinimumRecharge.FormatData()), CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}

	AfxGetApp()->BeginWaitCursor();
	DataBool bOk = GetTbSenderInterface()->GetEstimateCharge
		(
		GetPostaLiteSettings()->m_LoginId,
		GetPostaLiteSettings()->m_TokenAuth,
		GetPostaLiteSettings()->m_ChargeAmount,
		GetPostaLiteSettings()->m_VatAmount,
		GetPostaLiteSettings()->m_ActivationAmount,
		GetPostaLiteSettings()->m_TotalAmount,
		GetPostaLiteSettings()->m_Iban,
		GetPostaLiteSettings()->m_Bank,
		GetPostaLiteSettings()->m_Beneficiary,
		errorMsg
		);

	if (!bOk || !errorMsg.IsEmpty())
	{
		GetPostaLiteMessages()->Add(errorMsg, CMessages::MSG_ERROR);
		GetPostaLiteMessages()->Show();
		return;
	}
	
	GetPostaLiteSettings()->m_bFirstActivation = GetPostaLiteSettings()->m_ActivationAmount.IsEmpty() ? FALSE : TRUE;

	m_TempChargeAmount = GetPostaLiteSettings()->m_ChargeAmount;

	//TODOLUCA, causale nel caso di 136
	GetPostaLiteSettings()->m_WireTransferReason = GetPostaLiteSettings()->m_bFirstActivation
		? _TB("PostaLite first activation, account: ") + GetPostaLiteSettings()->m_LoginId
		: _TB("PostaLite recharge, account: ") + GetPostaLiteSettings()->m_LoginId;

	GetPostaLiteSettings()->SaveSettings();

	UpdateControls();
	
	AfxGetApp()->EndWaitCursor();
}


//////////////////////////////////////////////////////////////////////////////////
//		CPostaliteAccountManagementWizardDoc implementation
//////////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(CPostaLiteMoneyEdit, CMoneyEdit)

BEGIN_MESSAGE_MAP(CPostaLiteMoneyEdit, CMoneyEdit)
	//{{AFX_MSG_MAP(CPostaLiteMoneyEdit)
	ON_WM_CONTEXTMENU	()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//Questa classe serve solamente per avere un menù contestuale che non permetta la formattazione ma la sola
//copia del contenuto

//-----------------------------------------------------------------------------
CPostaLiteMoneyEdit::CPostaLiteMoneyEdit()
: CMoneyEdit()
{
}

//-----------------------------------------------------------------------------
void CPostaLiteMoneyEdit::OnContextMenu(CWnd* pWnd, CPoint mousePos)
{
	CBCGPEdit::OnContextMenu(pWnd, mousePos);
}
