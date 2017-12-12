#include "stdafx.h"

#include <TbGeneric/SettingsTable.h>
#include <TbGeneric/DataObj.h>
#include <TbGeneric/EnumsConst.h>
#include <TbGeneric/GeneralFunctions.h>

#include <TbGenLibManaged/GlobalFunctions.h>

#include <TbGenlibUI/SettingsTableManager.h>

#include "PostaLiteSettings.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//=============================================================================        
//PostaLiteSettings 
static const TCHAR szPostaLiteSettingsFile[]	= _T("PostaLite.config");
static const TCHAR szPostaLiteSubscribe[]		= _T("Subscribe");
static const TCHAR szPostaLiteParameters[]		= _T("Parameters");
static const TCHAR szPostaLiteCreditManagement[]= _T("CreditManagement");
static const TCHAR szAddresser[]				= _T("Addresser");

static const TCHAR szEnabled[]					= _T("Enabled");

static const TCHAR szMarginLeft[]				= _T("MarginLeft");
static const TCHAR szMarginRight[]				= _T("MarginRight");
static const TCHAR szMarginTop[]				= _T("MarginTop");
static const TCHAR szMarginBottom[]				= _T("MarginBottom");

static const TCHAR szRotateLandscape[]			= _T("RotateLandscape");

static const TCHAR szDeliveryType[]				= _T("DeliveryType");
static const TCHAR szPrintType[]				= _T("PrintType");
static const TCHAR szDefaultCountry[]			= _T("DefaultCountry");

static const TCHAR szLoginId[]					= _T("LoginId");
static const TCHAR szTokenAuth[]				= _T("TokenAuth");

static const TCHAR szSendTime[]					= _T("SendTime");
static const TCHAR szRecurHourInterval[]		= _T("RecurHourInterval");
static const TCHAR szGroupingInterval[]			= _T("GroupingInterval");
static const TCHAR szCreditLimit[]				= _T("CreditLimit");

static const TCHAR szBehalfThirdParty[]			= _T("BehalfThirdParty");
static const TCHAR szPrivateEntity[]			= _T("PrivateEntity");
static const TCHAR szPrivateEntityOption[]		= _T("PrivateEntityOption");
static const TCHAR szAdviceOfDeliveryEmail[]	= _T("AdviceOfDeliveryEmail");

static const TCHAR szCompanyName[]				= _T("CompanyName");
static const TCHAR szCity[]						= _T("City");
static const TCHAR szAddress[]					= _T("Address");
static const TCHAR szZIPCode[]					= _T("ZIPCode");
static const TCHAR szCadastralCode[]			= _T("CadastralCode");
static const TCHAR szCounty[]					= _T("County");
static const TCHAR szCountry[]					= _T("Country");
static const TCHAR szAreaCodeTelephone[]		= _T("AreaCodeTelephone");
static const TCHAR szTelephone[]				= _T("Telephone");
static const TCHAR szTaxIdNumber[]				= _T("TaxIdNumber");
static const TCHAR szFiscalCode[]				= _T("FiscalCode");
static const TCHAR szEMail[]					= _T("EMail");
static const TCHAR szNotes[]					= _T("Notes");
static const TCHAR szActivityCode[]				= _T("ActivityCode");
static const TCHAR szLegalStatusCode[]			= _T("LegalStatusCode");

static const TCHAR szSenderCompanyName[]		= _T("SenderCompanyName");
static const TCHAR szSenderCity[]				= _T("SenderCity");
static const TCHAR szSenderAddress[]			= _T("SenderAddress");
static const TCHAR szSenderZIPCode[]			= _T("SenderZIPCode");
static const TCHAR szSenderCounty[]				= _T("SenderCounty");
static const TCHAR szSenderCountry[]			= _T("SenderCountry");
static const TCHAR szSenderTaxIdNumber[]		= _T("SenderTaxIdNumber");
static const TCHAR szSenderFiscalCode[]			= _T("SenderFiscalCode");
static const TCHAR szSenderEMail[]				= _T("SenderEMail");
static const TCHAR szSenderLegalStatusCode[]	= _T("SenderLegalStatusCode");
static const TCHAR szSenderActivityCode[]		= _T("SenderActivityCode");

static const TCHAR szChargeAmount[]				= _T("ChargeAmount");
static const TCHAR szActivationAmount[]			= _T("ActivationAmount");
static const TCHAR szVatAmount[]				= _T("VatAmount");
static const TCHAR szTotalAmount[]				= _T("TotalAmount");
static const TCHAR szBeneficiary[]				= _T("Beneficiary");
static const TCHAR szBank[]						= _T("Bank");
static const TCHAR szIban[]						= _T("Iban");
static const TCHAR szWireTransferReason[]		= _T("WireTransferReason");
static const TCHAR szFirstActivation[]			= _T("FirstActivation");

static const TCHAR szAddresserType[]			= _T("AddresserType");
static const TCHAR szAddresserCompanyName[]		= _T("AddresserCompanyName");
static const TCHAR szAddresserAddress[]			= _T("AddresserAddress");
static const TCHAR szAddresserZipCode[]			= _T("AddresserZipCode");
static const TCHAR szAddresserCity[]			= _T("AddresserCity");
static const TCHAR szAddresserCounty[]			= _T("AddresserCounty");
static const TCHAR szAddresserCountry[]			= _T("AddresserCountry");

//=============================================================================        
//						PostaLiteSettings
//=============================================================================        
//-----------------------------------------------------------------------------
PostaLiteSettings::PostaLiteSettings ()
	: 
	TbBaseSettings(_T("Module.Extensions.TbMailer"), szPostaLiteSettingsFile)
{
	LoadSettings();
}

//-----------------------------------------------------------------------------
void PostaLiteSettings::LoadSettings()
{
	m_Enabled				= *((DataBool*) AfxGetSettingValue(m_Owner, szPostaLiteParameters, szEnabled, DataBool(FALSE), szPostaLiteSettingsFile));
	m_MarginLeft			= *((DataInt*)	AfxGetSettingValue(m_Owner, szPostaLiteParameters, szMarginLeft, DataInt(11), szPostaLiteSettingsFile));
	m_MarginTop				= *((DataInt*)	AfxGetSettingValue(m_Owner, szPostaLiteParameters, szMarginTop, DataInt(14), szPostaLiteSettingsFile));
	m_MarginBottom			= *((DataInt*)	AfxGetSettingValue(m_Owner, szPostaLiteParameters, szMarginBottom, DataInt(14), szPostaLiteSettingsFile));
	m_MarginRight			= *((DataInt*)	AfxGetSettingValue(m_Owner, szPostaLiteParameters, szMarginRight, DataInt(11), szPostaLiteSettingsFile));
	m_RotateLandscape		= *((DataBool*) AfxGetSettingValue(m_Owner, szPostaLiteParameters, szRotateLandscape, DataBool(TRUE), szPostaLiteSettingsFile));
	m_DeliveryType			= *((DataEnum*)	AfxGetSettingValue(m_Owner, szPostaLiteParameters, szDeliveryType, DataEnum(E_POSTALITE_DELIVERY_TYPE_DEFAULT), szPostaLiteSettingsFile));
	m_PrintType				= *((DataEnum*)	AfxGetSettingValue(m_Owner, szPostaLiteParameters, szPrintType, DataEnum(E_POSTALITE_PRINT_TYPE_DEFAULT), szPostaLiteSettingsFile));
	m_CreditLimit			= *((DataMon*)	AfxGetSettingValue(m_Owner, szPostaLiteParameters, szCreditLimit, DataMon(20), szPostaLiteSettingsFile));
	m_AdviceOfDeliveryEmail = *((DataStr*)	AfxGetSettingValue(m_Owner, szPostaLiteParameters, szAdviceOfDeliveryEmail, DataStr(), szPostaLiteSettingsFile));
	m_DefaultCountry		= *((DataStr*)	AfxGetSettingValue(m_Owner, szPostaLiteParameters, szDefaultCountry, DataStr(), szPostaLiteSettingsFile));

	DataDate defaultDate = DataDate(1, 1, 2000, 20, 0, 0);
	DataDate tempSend		= *((DataDate*)AfxGetSettingValue(m_Owner, szPostaLiteParameters, szSendTime, defaultDate, szPostaLiteSettingsFile));
	m_SendTime.SetFullDate();
	m_SendTime = tempSend;
	m_RecurHourInterval		= *((DataInt*)AfxGetSettingValue(m_Owner, szPostaLiteParameters, szRecurHourInterval, DataInt(12), szPostaLiteSettingsFile));
	
	m_PrivateEntity			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szPrivateEntity, DataStr(), szPostaLiteSettingsFile));
	m_PrivateEntityOption	= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szPrivateEntityOption, DataStr(_T("A")), szPostaLiteSettingsFile));
	
	m_LoginId				= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szLoginId, DataStr(), szPostaLiteSettingsFile));
	m_TokenAuth				= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szTokenAuth, DataStr(), szPostaLiteSettingsFile));
	m_Notes					= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szNotes, DataStr(), szPostaLiteSettingsFile));
	
	m_CompanyName			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szCompanyName, DataStr(), szPostaLiteSettingsFile));
	m_City					= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szCity, DataStr(), szPostaLiteSettingsFile));
	m_Address				= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szAddress, DataStr(), szPostaLiteSettingsFile));
	m_ZipCode				= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szZIPCode, DataStr(), szPostaLiteSettingsFile));
	m_CadastralCode			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szCadastralCode, DataStr(), szPostaLiteSettingsFile));
	m_County				= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szCounty, DataStr(), szPostaLiteSettingsFile));
	m_Country				= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szCountry, DataStr(), szPostaLiteSettingsFile));
	m_Telephone				= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szTelephone, DataStr(), szPostaLiteSettingsFile));
	m_AreaCodeTelephone		= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szAreaCodeTelephone, DataStr(), szPostaLiteSettingsFile));
	m_TaxIdNumber			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szTaxIdNumber, DataStr(), szPostaLiteSettingsFile));
	m_FiscalCode			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szFiscalCode, DataStr(), szPostaLiteSettingsFile));
	m_EMail					= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szEMail, DataStr(), szPostaLiteSettingsFile));
	m_ActivityCode			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szActivityCode, DataStr(), szPostaLiteSettingsFile));
	m_LegalStatusCode		= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szLegalStatusCode, DataStr(), szPostaLiteSettingsFile));

	//Sender data
	m_SenderCompanyName		= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderCompanyName, DataStr(), szPostaLiteSettingsFile));
	m_SenderCity			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderCity, DataStr(), szPostaLiteSettingsFile));
	m_SenderAddress			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderAddress, DataStr(), szPostaLiteSettingsFile));
	m_SenderZipCode			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderZIPCode, DataStr(), szPostaLiteSettingsFile));
	m_SenderCounty			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderCounty, DataStr(), szPostaLiteSettingsFile));
	m_SenderCountry			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderCountry, DataStr(), szPostaLiteSettingsFile));
	m_SenderTaxIdNumber		= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderTaxIdNumber, DataStr(), szPostaLiteSettingsFile));
	m_SenderFiscalCode		= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderFiscalCode, DataStr(), szPostaLiteSettingsFile));
	m_SenderEMail			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderEMail, DataStr(), szPostaLiteSettingsFile));
	m_SenderActivityCode	= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderActivityCode, DataStr(), szPostaLiteSettingsFile));
	m_SenderLegalStatusCode = *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderLegalStatusCode, DataStr(), szPostaLiteSettingsFile));

	m_bFirstActivation		= *((DataBool*)AfxGetSettingValue(m_Owner, szPostaLiteCreditManagement, szFirstActivation, DataBool(), szPostaLiteSettingsFile));
	m_ChargeAmount			= *((DataMon*)AfxGetSettingValue(m_Owner, szPostaLiteCreditManagement, szChargeAmount, DataMon(), szPostaLiteSettingsFile));
	m_ActivationAmount		= *((DataMon*)AfxGetSettingValue(m_Owner, szPostaLiteCreditManagement, szActivationAmount, DataMon(), szPostaLiteSettingsFile));
	m_VatAmount				= *((DataMon*)AfxGetSettingValue(m_Owner, szPostaLiteCreditManagement, szVatAmount, DataMon(), szPostaLiteSettingsFile));
	m_TotalAmount			= *((DataMon*)AfxGetSettingValue(m_Owner, szPostaLiteCreditManagement, szTotalAmount, DataMon(), szPostaLiteSettingsFile));
	m_Beneficiary			= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteCreditManagement, szBeneficiary, DataStr(), szPostaLiteSettingsFile));
	m_Bank					= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteCreditManagement, szBank, DataStr(), szPostaLiteSettingsFile));
	m_Iban					= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteCreditManagement, szIban, DataStr(), szPostaLiteSettingsFile));
	m_WireTransferReason	= *((DataStr*)AfxGetSettingValue(m_Owner, szPostaLiteCreditManagement, szWireTransferReason, DataStr(), szPostaLiteSettingsFile));	

	m_AddresserType			= *((DataEnum*)	AfxGetSettingValue(m_Owner, szAddresser, szAddresserType, DataEnum(E_POSTALITE_ADDRESSER_COMPANY_DATA), szPostaLiteSettingsFile));
	m_AddresserCompanyName	= *((DataStr*)	AfxGetSettingValue(m_Owner, szAddresser, szAddresserCompanyName, DataStr(), szPostaLiteSettingsFile));	
	m_AddresserAddress		= *((DataStr*)	AfxGetSettingValue(m_Owner, szAddresser, szAddresserAddress, DataStr(), szPostaLiteSettingsFile));	
	m_AddresserZipCode		= *((DataStr*)	AfxGetSettingValue(m_Owner, szAddresser, szAddresserZipCode, DataStr(), szPostaLiteSettingsFile));	
	m_AddresserCity			= *((DataStr*)	AfxGetSettingValue(m_Owner, szAddresser, szAddresserCity, DataStr(), szPostaLiteSettingsFile));	
	m_AddresserCounty		= *((DataStr*)	AfxGetSettingValue(m_Owner, szAddresser, szAddresserCounty, DataStr(), szPostaLiteSettingsFile));	
	m_AddresserCountry		= *((DataStr*)	AfxGetSettingValue(m_Owner, szAddresser, szAddresserCountry, DataStr(), szPostaLiteSettingsFile));	
}

//-----------------------------------------------------------------------------
void PostaLiteSettings::SaveSettings()
{
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szEnabled, m_Enabled, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szMarginLeft, m_MarginLeft, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szMarginRight, m_MarginRight, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szMarginTop, m_MarginTop, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szMarginBottom, m_MarginBottom, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szRotateLandscape, m_RotateLandscape, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szSendTime, m_SendTime, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szRecurHourInterval, m_RecurHourInterval, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szDeliveryType, m_DeliveryType, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szPrintType, m_PrintType, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szCreditLimit, m_CreditLimit, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szAdviceOfDeliveryEmail, m_AdviceOfDeliveryEmail, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteParameters, szDefaultCountry, m_DefaultCountry, szPostaLiteSettingsFile);
	
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szPrivateEntity, m_PrivateEntity, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szPrivateEntityOption, m_PrivateEntityOption, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szLoginId, m_LoginId, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szTokenAuth, m_TokenAuth, szPostaLiteSettingsFile);

	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szCompanyName, m_CompanyName, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szCity, m_City, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szAddress, m_Address, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szZIPCode, m_ZipCode, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szCadastralCode, m_CadastralCode, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szCounty, m_County, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szCountry, m_Country, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szAreaCodeTelephone, m_AreaCodeTelephone, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szTelephone, m_Telephone, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szTaxIdNumber, m_TaxIdNumber, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szFiscalCode, m_FiscalCode, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szEMail, m_EMail, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szActivityCode, m_ActivityCode, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szLegalStatusCode, m_LegalStatusCode, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szNotes, m_Notes, szPostaLiteSettingsFile);
	
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderCompanyName, m_SenderCompanyName, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderCity, m_SenderCity, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderAddress, m_SenderAddress, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderZIPCode, m_SenderZipCode, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderCounty, m_SenderCounty, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderCountry, m_SenderCountry, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderTaxIdNumber, m_SenderTaxIdNumber, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderFiscalCode, m_SenderFiscalCode, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderEMail, m_SenderEMail, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderActivityCode, m_SenderActivityCode, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteSubscribe, szSenderLegalStatusCode, m_SenderLegalStatusCode, szPostaLiteSettingsFile);

	AfxSetSettingValue(m_Owner, szPostaLiteCreditManagement, szFirstActivation, m_bFirstActivation, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteCreditManagement, szChargeAmount, m_ChargeAmount, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteCreditManagement, szActivationAmount, m_ActivationAmount, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteCreditManagement, szVatAmount, m_VatAmount, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteCreditManagement, szTotalAmount, m_TotalAmount, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteCreditManagement, szBeneficiary, m_Beneficiary, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteCreditManagement, szBank, m_Bank, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteCreditManagement, szIban, m_Iban, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szPostaLiteCreditManagement, szWireTransferReason, m_WireTransferReason, szPostaLiteSettingsFile);
	
	AfxSetSettingValue(m_Owner, szAddresser, szAddresserType, m_AddresserType, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szAddresser, szAddresserCompanyName, m_AddresserCompanyName, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szAddresser, szAddresserAddress, m_AddresserAddress, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szAddresser, szAddresserZipCode, m_AddresserZipCode, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szAddresser, szAddresserCity, m_AddresserCity, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szAddresser, szAddresserCounty, m_AddresserCounty, szPostaLiteSettingsFile);
	AfxSetSettingValue(m_Owner, szAddresser, szAddresserCountry, m_AddresserCountry, szPostaLiteSettingsFile);
	 
	CCustomSaveInterface* pIntf = new CCustomSaveInterface();
	pIntf->m_bSaveAllUsers = TRUE;
	pIntf->m_eSaveMode = CCustomSaveInterface::COMPANY_USERS;

	AfxSaveSettingsFile(this->m_Owner, this->m_sFileName, FALSE, pIntf);
	delete pIntf;
}

//---------------------------------------------------------------------------------------
void PostaLiteSettings::ClearSubscription()
{
	m_LoginId.Clear();
	m_TokenAuth.Clear();

	m_CompanyName.Clear();
	m_City.Clear(); 
	m_Address.Clear();
	m_ZipCode.Clear();
	m_CadastralCode.Clear();
	m_County.Clear();
	m_Country.Clear();
	m_AreaCodeTelephone.Clear();
	m_Telephone.Clear();
	m_TaxIdNumber.Clear();
	m_FiscalCode.Clear();
	m_EMail.Clear();
	m_ActivityCode.Clear();
	m_LegalStatusCode.Clear();

	m_PrivateEntity.Clear();
	m_PrivateEntityOption.Clear();

	m_SenderCompanyName.Clear();
	m_SenderCity.Clear(); 
	m_SenderAddress.Clear();
	m_SenderZipCode.Clear();
	m_SenderCounty.Clear();
	m_SenderCountry.Clear();
	m_SenderTaxIdNumber.Clear();
	m_SenderFiscalCode.Clear();
	m_SenderEMail.Clear();
	m_SenderActivityCode.Clear();
	m_SenderLegalStatusCode.Clear();
	
	m_Notes.Clear();
	m_bFirstActivation.Clear();
	m_ChargeAmount.Clear();
	m_ActivationAmount.Clear();
	m_VatAmount.Clear();
	m_TotalAmount.Clear();
	m_Beneficiary.Clear();
	m_Bank.Clear();
	m_Iban.Clear();
	m_WireTransferReason.Clear();
	
	m_AdviceOfDeliveryEmail.Clear();

	m_DefaultCountry.Clear();

	m_AddresserType.Clear();			
	m_AddresserCompanyName.Clear();
	m_AddresserAddress.Clear();
	m_AddresserZipCode.Clear();
	m_AddresserCity.Clear();
	m_AddresserCounty.Clear();
	m_AddresserCountry.Clear();

	SaveSettings();
}
