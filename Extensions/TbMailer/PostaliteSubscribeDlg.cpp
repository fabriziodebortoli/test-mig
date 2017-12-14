#include "stdafx.h"

#include "PostaLiteSubscribeDlg.h"
#include "PostaLite.hjson" //JSON AUTOMATIC UPDATE
#include "TbSenderInterface.h"
#include <TbWoormViewer\SoapFunctions.h>
#include <TbGenLibManaged\HelpManager.h>
#include <TbGenLibManaged\PostaLiteNet.h>
	
#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szHelpNamespace[] = _T("RefGuide-Extensions-TbMailer-TbMailer-PostaLiteSettings");

static PostaLiteSettings* m_Setting;
static CMessages* m_Messages;
static TbSenderInterface* m_pTbSenderInterface;

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
static BOOL IsCountryValidForPostaMassiva()
{
	return 
		GetPostaLiteSettings()->m_DefaultCountry.Str().CompareNoCase(_T("Italia")) == 0 || 
		GetPostaLiteSettings()->m_DefaultCountry.Str().CompareNoCase(_T("Italy")) == 0; 
}

//---------------------------------------------------------------------------
CCompanyAddressInfo* LoadCompanyData()
{				
	CCompanyAddressInfo* pCompany = new CCompanyAddressInfo();

	if (PostaLiteStaticMethods::s_pfGetCompanyInfos)
		PostaLiteStaticMethods::s_pfGetCompanyInfos(pCompany);

/*
	SqlTable* m_pTblCompany = new SqlTable(pTCompany, AfxGetDefaultSqlSession());	

	if (m_pTblCompany->IsOpen())
		m_pTblCompany->Close();

	pTCompany->SetQualifier();

	m_pTblCompany->Open();
	m_pTblCompany->SelectAll();
	m_pTblCompany->Select(pTCompany, pTCompany->f_CompanyName);
	m_pTblCompany->Select(pTCompany, pTCompany->f_TaxIdNumber);
	m_pTblCompany->Select(pTCompany, pTCompany->f_FiscalCode);	
	m_pTblCompany->Select(pTCompany, pTCompany->f_Address);
	m_pTblCompany->Select(pTCompany, pTCompany->f_ZIPCode);
	m_pTblCompany->Select(pTCompany, pTCompany->f_City);
	m_pTblCompany->Select(pTCompany, pTCompany->f_County);
	m_pTblCompany->Select(pTCompany, pTCompany->f_Country);
	m_pTblCompany->Select(pTCompany, pTCompany->f_Telephone1);
	m_pTblCompany->Select(pTCompany, pTCompany->f_Fax);
	m_pTblCompany->Select(pTCompany, pTCompany->f_EMail);
	m_pTblCompany->FromTable(pTCompany); 

	TRY
	{
		m_pTblCompany->Query();
	
		while (!m_pTblCompany->IsEOF())
		{
			m_pTblCompany->MoveNext();
		}
	}
	CATCH (SqlException, e)
	{
		CString err = e->m_strError;
	}
	END_CATCH

	m_pTblCompany->Close();
	SAFE_DELETE (m_pTblCompany);
	*/
	return pCompany;
}

//---------------------------------------------------------------------------------------
CString AssociateAddresserAndGetPreview(DataStr& companyName, DataStr& address, DataStr& county, DataStr& zipCode, DataStr& city, DataStr& country)
{
	CString strPreviewAddresserData = companyName + _T("\r\n");
	strPreviewAddresserData += address + _T("\r\n");
	strPreviewAddresserData += cwsprintf(_T("%s %s ( %s )\r\n"), zipCode.Str(), city.Str(), county.Str());
	strPreviewAddresserData += country;

	GetPostaLiteSettings()->m_AddresserCompanyName = companyName;
	GetPostaLiteSettings()->m_AddresserAddress = address;
	GetPostaLiteSettings()->m_AddresserZipCode = zipCode;
	GetPostaLiteSettings()->m_AddresserCity = city;
	GetPostaLiteSettings()->m_AddresserCounty = county;
	GetPostaLiteSettings()->m_AddresserCountry = country;

	return strPreviewAddresserData;		
}

/////////////////////////////////////////////////////////////////////////////
// CPostaLiteSettingsDialog dialog
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CPostaLiteSettingsDialog, CParsedDialog)
//---------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CPostaLiteSettingsDialog, CParsedDialog)
	//{{AFX_MSG_MAP(CPostaLiteSettingsDialog)
		ON_BN_CLICKED(IDC_POSTALITE_SETTINGS_UPDATE, OnUpdate)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_DEFAULT_DELIVERY_TYPE, OnDeliveryTypeChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_DEFAULT_COUNTRY, OnCountryChanged)

		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_DEFAULT_DELIVERY_TYPE, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_DEFAULT_PRINT_TYPE, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_DEFAULT_COUNTRY, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_ADVICE_OF_RETURN_EMAIL, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_DELIVERYTIME, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_DELIVERYINTERVAL, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_NOTIFIY_CREDITLIMIT, OnValueChanged)

		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_MARGINTOP, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_MARGINLEFT, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_MARGINRIGHT, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_MARGINBOTTOM, OnValueChanged)

		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_NO_ADDRESSER, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_ADDRESSER_COMPANY, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_ADDRESSER_SUBSCRIBER, OnValueChanged)
		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_ADDRESSER_LAW136, OnValueChanged)

		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_ENABLED, OnValueChanged)

		ON_EN_VALUE_CHANGED	(IDC_POSTALITE_SETTINGS_ADDRESSER_GROUP, OnAddresserChanged)
		ON_WM_ENABLE()
		ON_WM_SHOWWINDOW()
		ON_WM_HELPINFO()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//---------------------------------------------------------------------------------------
CPostaLiteSettingsDialog::CPostaLiteSettingsDialog(CWnd* pParent /*=NULL*/)
	: CParsedDialog(IDD_POSTALITE_SETTINGS, pParent),
		m_nMarginTopCtrl			(NULL, &GetPostaLiteSettings()->m_MarginTop),
		m_nMarginLeftCtrl			(NULL, &GetPostaLiteSettings()->m_MarginLeft),
		m_nMarginRightCtrl			(NULL, &GetPostaLiteSettings()->m_MarginRight),
		m_nMarginBottomCtrl			(NULL, &GetPostaLiteSettings()->m_MarginBottom),
		m_nDeliveryIntervalCtrl		(NULL, &GetPostaLiteSettings()->m_RecurHourInterval),
		m_DeliveryTimeCtrl			(NULL, &GetPostaLiteSettings()->m_SendTime),
        m_cbxDeliveryType			(NULL, &GetPostaLiteSettings()->m_DeliveryType),
        m_cbxPrintType				(NULL, &GetPostaLiteSettings()->m_PrintType),
		m_AdviceOfReturnEmailCtrl	(NULL, &GetPostaLiteSettings()->m_AdviceOfDeliveryEmail),
		m_NotifyOnLowCreditCtrl		(NULL, &GetPostaLiteSettings()->m_CreditLimit),
		m_PreviewAddresserDataCtrl	(NULL, &m_strPreviewAddresserData),
		m_EnabledCtrl				(&GetPostaLiteSettings()->m_Enabled),
		m_AddresserTypeCtrl			(&GetPostaLiteSettings()->m_AddresserType),
		m_bIsFax					(GetPostaLiteSettings()->m_DeliveryType == E_POSTALITE_DELIVERY_TYPE_FAX),
		m_bIsDirty					(FALSE)
{
	DataEnum add =	GetPostaLiteSettings()->m_AddresserType;
	m_bInOpenDlgCounter = FALSE;
}

//---------------------------------------------------------------------------------------
CPostaLiteSettingsDialog::~CPostaLiteSettingsDialog()
{
	ClearStaticObjects();
}

//---------------------------------------------------------------------------------------
BOOL CPostaLiteSettingsDialog::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	OnSubclassEdit();
	
	AddresserChanged();

	UpdateControls();

	return TRUE;
}

//---------------------------------------------------------------------------------------
void CPostaLiteSettingsDialog::OnCancel()
{
	if (m_bIsDirty)
	{
		if (AfxTBMessageBox(_TB("There are unsaved changes, do you want to exit anyway?"), MB_ICONWARNING | MB_YESNO) == IDYES)
		{
			__super::OnCancel();
		}
	}
	else
		__super::OnCancel();
}


//-----------------------------------------------------------------------------
void CPostaLiteSettingsDialog::OnShowWindow(BOOL bShow, UINT nStatus)
{
	UpdateControls();
	
	m_EnabledCtrl.SetFocus();
}

//-----------------------------------------------------------------------------
BOOL CPostaLiteSettingsDialog::OnHelpInfo(HELPINFO* pHelpInfo)
{
	return ShowHelp(szHelpNamespace);
}

//---------------------------------------------------------------------------------------
void CPostaLiteSettingsDialog::OnValueChanged()
{
	m_bIsDirty = TRUE;
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteSettingsDialog::AddresserChanged()
{
	m_strPreviewAddresserData.Clear();
	switch(GetPostaLiteSettings()->m_AddresserType.GetValue())
	{
		case E_POSTALITE_ADDRESSER_COMPANY_DATA:
		{
			CCompanyAddressInfo* pCompanyData = LoadCompanyData();
			m_strPreviewAddresserData = AssociateAddresserAndGetPreview
				(
				pCompanyData->f_CompanyName,
				pCompanyData->f_Address, 
				pCompanyData->f_County, 
				pCompanyData->f_ZIPCode, 
				pCompanyData->f_City,
				pCompanyData->f_Country
				);
		
			SAFE_DELETE(pCompanyData);
			break;
		}
		case E_POSTALITE_ADDRESSER_SUBSCRIBER_DATA:
		{
			m_strPreviewAddresserData = AssociateAddresserAndGetPreview
				(
				GetPostaLiteSettings()->m_CompanyName,
				GetPostaLiteSettings()->m_Address,
				GetPostaLiteSettings()->m_County,
				GetPostaLiteSettings()->m_ZipCode,
				GetPostaLiteSettings()->m_City,
				GetPostaLiteSettings()->m_Country
				);
			break;
		}
		case E_POSTALITE_ADDRESSER_SENDER_DATA:
		{
			m_strPreviewAddresserData = AssociateAddresserAndGetPreview
				(
				GetPostaLiteSettings()->m_SenderCompanyName,
				GetPostaLiteSettings()->m_SenderAddress, 
				GetPostaLiteSettings()->m_SenderCounty, 
				GetPostaLiteSettings()->m_SenderZipCode,
				GetPostaLiteSettings()->m_SenderCity,
				GetPostaLiteSettings()->m_SenderCountry
				);
			break;
		}
		default:
			m_strPreviewAddresserData.Clear();
	}
}

//---------------------------------------------------------------------------------------
void CPostaLiteSettingsDialog::OnAddresserChanged()
{
	AddresserChanged();
		 	
	m_bIsDirty = TRUE;
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteSettingsDialog::OnUpdate()
{
	AfxGetApp()->BeginWaitCursor();

	if (GetPostaLiteSettings()->m_Enabled && !GetPostaLiteSettings()->m_TokenAuth.IsEmpty())
	{
		if (GetPostaLiteSettings()->m_AdviceOfDeliveryEmail.IsEmpty())
		{
			AfxTBMessageBox(_TB("The e-mail address used for advice of delivery is mandatory"), MB_ICONWARNING | MB_OK);
			return;
		}

		if (!PostaLiteStaticMethods::IsValidEmailAddress(GetPostaLiteSettings()->m_AdviceOfDeliveryEmail))
		{
			AfxTBMessageBox(_TB("Invalid advice of delivery e-mail address"), MB_ICONWARNING | MB_OK);
			return;
		}

		if (GetPostaLiteSettings()->m_CreditLimit < 0)
		{
			AfxTBMessageBox(_TB("The credit limit cannot be a negative number"), MB_ICONINFORMATION | MB_OK);
			return;
		}
	}

	GetPostaLiteSettings()->SaveSettings();

	GetTbSenderInterface()->RefreshSettings();

	AfxGetApp()->EndWaitCursor();

	m_bIsDirty = FALSE;

	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteSettingsDialog::OnDeliveryTypeChanged()
{
	m_bIsFax = GetPostaLiteSettings()->m_DeliveryType == E_POSTALITE_DELIVERY_TYPE_FAX;
	if (m_bIsFax)
		GetPostaLiteSettings()->m_PrintType = E_POSTALITE_PRINT_TYPE_FRONT_BW;

	m_bIsDirty = TRUE;
	
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteSettingsDialog::OnCountryChanged()
{
	if (!IsCountryValidForPostaMassiva())
		GetPostaLiteSettings()->m_DeliveryType = E_POSTALITE_DELIVERY_TYPE_POSTA_PRIORITARIA;

	m_bIsDirty = TRUE;
	UpdateControls();
}

//---------------------------------------------------------------------------------------
void CPostaLiteSettingsDialog::UpdateControls()
{
	BOOL bIsReadonly = !GetPostaLiteSettings()->m_Enabled || GetPostaLiteSettings()->m_TokenAuth.IsEmpty();
	GetPostaLiteSettings()->m_MarginTop.SetReadOnly(bIsReadonly);
	GetPostaLiteSettings()->m_MarginLeft.SetReadOnly(bIsReadonly);
	GetPostaLiteSettings()->m_MarginRight.SetReadOnly(bIsReadonly);
	GetPostaLiteSettings()->m_MarginBottom.SetReadOnly(bIsReadonly);
	GetPostaLiteSettings()->m_SendTime.SetReadOnly(bIsReadonly);
	GetPostaLiteSettings()->m_RecurHourInterval.SetReadOnly(bIsReadonly);
	GetPostaLiteSettings()->m_DeliveryType.SetReadOnly(bIsReadonly);
	GetPostaLiteSettings()->m_PrintType.SetReadOnly(bIsReadonly || m_bIsFax);
	GetPostaLiteSettings()->m_DefaultCountry.SetReadOnly(bIsReadonly);
	GetPostaLiteSettings()->m_AdviceOfDeliveryEmail.SetReadOnly(bIsReadonly);
	GetPostaLiteSettings()->m_CreditLimit.SetReadOnly(bIsReadonly);

	m_UpdateSettingsButton.EnableWindow(m_bIsDirty);

	m_nMarginTopCtrl.UpdateCtrlStatus();			m_nMarginTopCtrl.UpdateCtrlView();
	m_nMarginLeftCtrl.UpdateCtrlStatus();			m_nMarginLeftCtrl.UpdateCtrlView();
	m_nMarginRightCtrl.UpdateCtrlStatus();			m_nMarginRightCtrl.UpdateCtrlView();
	m_nMarginBottomCtrl.UpdateCtrlStatus();			m_nMarginBottomCtrl.UpdateCtrlView();
	m_DeliveryTimeCtrl.UpdateCtrlStatus();			m_DeliveryTimeCtrl.UpdateCtrlView();
	m_nDeliveryIntervalCtrl.UpdateCtrlStatus();		m_nDeliveryIntervalCtrl.UpdateCtrlView();
	m_cbxDeliveryType.UpdateCtrlStatus();			m_cbxDeliveryType.UpdateCtrlView();
	m_cbxDefaultCountry.UpdateCtrlStatus();			m_cbxDefaultCountry.UpdateCtrlView();
	m_cbxPrintType.UpdateCtrlStatus();				m_cbxPrintType.UpdateCtrlView();
	m_AdviceOfReturnEmailCtrl.UpdateCtrlStatus();	m_AdviceOfReturnEmailCtrl.UpdateCtrlView();
	m_NotifyOnLowCreditCtrl.UpdateCtrlStatus();		m_NotifyOnLowCreditCtrl.UpdateCtrlView();
	m_AddresserTypeCtrl.UpdateCtrlStatus();			m_AddresserTypeCtrl.UpdateCtrlView();
	m_PreviewAddresserDataCtrl.UpdateCtrlStatus();	m_PreviewAddresserDataCtrl.UpdateCtrlView();
	m_EnabledCtrl.UpdateCtrlStatus();				m_EnabledCtrl.UpdateCtrlView();
}

//---------------------------------------------------------------------------------------
void CPostaLiteSettingsDialog::OnSubclassEdit()
{
	m_nMarginTopCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_MARGINTOP,	this, _NS_CTRL("MarginTop"));	
	m_nMarginLeftCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_MARGINLEFT, this, _NS_CTRL("MarginLeft"));	
	m_nMarginRightCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_MARGINRIGHT,	this, _NS_CTRL("MarginRight"));	
	m_nMarginBottomCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_MARGINBOTTOM, this, _NS_CTRL("MarginBottom"));	
	m_nDeliveryIntervalCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_DELIVERYINTERVAL, this, _NS_CTRL("DeliveryInterval"));	
	m_DeliveryTimeCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_DELIVERYTIME, this, _NS_CTRL("DeliveryTime"));	
	m_cbxDeliveryType.SubclassEdit(IDC_POSTALITE_SETTINGS_DEFAULT_DELIVERY_TYPE, this, _NS_CTRL("DeliveryType"));	
	m_cbxPrintType.SubclassEdit(IDC_POSTALITE_SETTINGS_DEFAULT_PRINT_TYPE, this, _NS_CTRL("PrintType"));		
	m_NotifyOnLowCreditCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_NOTIFIY_CREDITLIMIT, this, _NS_CTRL("NotifyOnLowCreditCtrl"));		
	m_EnabledCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_ENABLED, this, _NS_CTRL("EnabledCtrl"));		

	m_PreviewAddresserDataCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_ADDRESSER_PREVIEW, this, _NS_CTRL("PreviewAddresserDataCtrl"));		

	m_AddresserTypeCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_ADDRESSER_GROUP, this, _NS_CTRL("AddresserTypeCtrl"));		
	m_AddresserTypeCtrl.AddAssociation(IDC_POSTALITE_SETTINGS_NO_ADDRESSER, E_POSTALITE_ADDRESSER_NO_ADDRESSER);
	m_AddresserTypeCtrl.AddAssociation(IDC_POSTALITE_SETTINGS_ADDRESSER_COMPANY,	E_POSTALITE_ADDRESSER_COMPANY_DATA);
	m_AddresserTypeCtrl.AddAssociation(IDC_POSTALITE_SETTINGS_ADDRESSER_SUBSCRIBER,	E_POSTALITE_ADDRESSER_SUBSCRIBER_DATA);
	m_AddresserTypeCtrl.AddAssociation(IDC_POSTALITE_SETTINGS_ADDRESSER_LAW136,	E_POSTALITE_ADDRESSER_SENDER_DATA);

	//il radio button del mittente legge 136 è visibile solamente se siamo nel caso 2b
	DataBool isAddresserLaw136Visible =
		GetPostaLiteSettings()->m_PrivateEntity.Str().CompareNoCase(_T("True")) == 0 && 
		!GetPostaLiteSettings()->m_PrivateEntityOption.IsEmpty() && 
		GetPostaLiteSettings()->m_PrivateEntityOption.Str().CompareNoCase(_T("B")) == 0;

	CWnd* pWnd =  GetDlgItem(IDC_POSTALITE_SETTINGS_ADDRESSER_LAW136);
	if (pWnd)
		pWnd->ShowWindow(isAddresserLaw136Visible ? SW_SHOW : SW_HIDE);

	if (GetPostaLiteSettings()->m_DefaultCountry.IsEmpty())
	{
		CString productIso = CPostaLiteNet::GetPostaliteCountryFromIsoCode(AfxGetLoginManager()->GetProductLanguage());
		GetPostaLiteSettings()->m_DefaultCountry = DataStr(productIso);
	}

	m_cbxDefaultCountry.SubclassEdit(IDC_POSTALITE_SETTINGS_DEFAULT_COUNTRY, this, _NS_CTRL("DefaultCountry"));	
	m_cbxDefaultCountry.UseProductLanguage(TRUE);
	m_cbxDefaultCountry.SetNameSpace(_NS_DF("DataFile.Extensions.TbMailer.State"));
	m_cbxDefaultCountry.Attach(&GetPostaLiteSettings()->m_DefaultCountry);
	
	m_UpdateSettingsButton.SubclassEdit(IDC_POSTALITE_SETTINGS_UPDATE, this, _NS_CTRL("UpdateSettingsButton"));	
	m_AdviceOfReturnEmailCtrl.SubclassEdit(IDC_POSTALITE_SETTINGS_ADVICE_OF_RETURN_EMAIL, this, _NS_CTRL("AdviceOfReturnEmail"));	

	m_nMarginTopCtrl.SetMinValue(DataInt(14));
	m_nMarginLeftCtrl.SetMinValue(DataInt(11));
	m_nMarginRightCtrl.SetMinValue(DataInt(11));
	m_nMarginBottomCtrl.SetMinValue(DataInt(14));

	m_nDeliveryIntervalCtrl.SetMinValue(DataInt(0));
	m_nDeliveryIntervalCtrl.SetMaxValue(DataInt(12));
}

/////////////////////////////////////////////////////////////////////////////
// CDeliveryTypeCombo dialog
/////////////////////////////////////////////////////////////////////////////

//---------------------------------------------------------------------------------------
CDeliveryTypeCombo::CDeliveryTypeCombo()
{

}

//---------------------------------------------------------------------------------------
CDeliveryTypeCombo::CDeliveryTypeCombo(UINT nBtnIDBmp, DataEnum* pEnum /*NULL*/)
	: CEnumCombo(nBtnIDBmp, pEnum)
{

}


//-----------------------------------------------------------------------------
BOOL CDeliveryTypeCombo::IsValidItemListBox (const DataObj& aDataObj)
{      
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)))
		return FALSE;

	if (((DataEnum&) aDataObj != E_POSTALITE_DELIVERY_TYPE_POSTA_MASSIVA))
		return TRUE;
		
	return IsCountryValidForPostaMassiva();
}

