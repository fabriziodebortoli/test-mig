#pragma once

#include <TbNameSolver\TbNamespaces.h>
#include <TbGeneric\ParametersSections.h>

#include "TbSenderInterface.h"


//includere alla fine degli include del .H
#include "beginh.dex"

// questo insieme di parametri viene sempre letto in blocco, quindi è
// comodo avere la sezione di lettura vecchio stile.
//=============================================================================        
class TB_EXPORT PostaLiteSettings : public TbBaseSettings
{
public:

	DataBool	m_Enabled;
	
	DataStr 	m_LoginId;
	DataStr		m_TokenAuth;
	
	DataStr		m_PrivateEntity;
	DataStr		m_PrivateEntityOption;
	DataInt		m_MarginLeft;
	DataInt		m_MarginRight;
	DataInt		m_MarginTop;
	DataInt		m_MarginBottom;
	DataBool	m_RotateLandscape;

	DataDate	m_SendTime;
	DataInt		m_RecurHourInterval;

	DataStr		m_CompanyName;
	DataStr		m_City;
	DataStr		m_Address;
	DataStr		m_ZipCode;
	DataStr		m_CadastralCode;
	DataStr		m_County;
	DataStr		m_Country;
	DataStr		m_AreaCodeTelephone;
	DataStr		m_Telephone;
	DataStr		m_TaxIdNumber;
	DataStr		m_FiscalCode;
	DataStr		m_EMail;
	DataStr		m_LegalStatusCode;
	DataStr		m_ActivityCode;

	DataStr		m_SenderCompanyName;
	DataStr		m_SenderCity;
	DataStr		m_SenderAddress;
	DataStr		m_SenderZipCode;
	DataStr		m_SenderCounty;
	DataStr		m_SenderCountry;
	DataStr		m_SenderTaxIdNumber;
	DataStr		m_SenderFiscalCode;
	DataStr		m_SenderEMail;
	DataStr		m_SenderLegalStatusCode;
	DataStr		m_SenderActivityCode;
	
	DataStr		m_Notes;
	DataMon		m_CreditLimit;
	DataEnum	m_DeliveryType;
	DataStr		m_DefaultCountry;
	DataEnum	m_PrintType;
	DataStr		m_AdviceOfDeliveryEmail;

	DataBool	m_bFirstActivation;
	DataMon		m_ChargeAmount;
	DataMon		m_ActivationAmount;
	DataMon		m_VatAmount;
	DataMon		m_TotalAmount;
	DataStr		m_Beneficiary;
	DataStr		m_Bank;
	DataStr		m_Iban;
	DataStr		m_WireTransferReason;

	DataEnum	m_AddresserType;

	DataStr		m_AddresserCompanyName;
	DataStr		m_AddresserAddress;
	DataStr		m_AddresserZipCode;
	DataStr		m_AddresserCity;
	DataStr		m_AddresserCounty;
	DataStr		m_AddresserCountry;
	
public:
	PostaLiteSettings ();
	void LoadSettings();
	void SaveSettings();
	void ClearSubscription();
};

#include "endh.dex"
