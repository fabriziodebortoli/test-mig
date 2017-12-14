
#include "StdAfx.h"

#include "TbSenderInterface.h"

//----------------------------------------------------------------------------
TbSenderInterface::TbSenderInterface(const CString& strServer, const CString& strService, int nWebServicesPort)
	:   
		m_strService (strService),
		m_strServiceNamespace (_T("http://microarea.it/TBSender/")),	
		m_strServer (strServer),
		m_nWebServicesPort (nWebServicesPort)
{
	m_strCompany = DataStr(AfxGetLoginInfos()->m_strCompanyName);
}

//----------------------------------------------------------------------------
TbSenderInterface::TbSenderInterface()
	:   
		m_strServiceNamespace (_T("http://microarea.it/TBSender/"))
{

	CString strFileServer, strWebServer, strInstallation, strMasterSolutionName;
	GetServerInstallationInfo(strFileServer, strWebServer, strInstallation, strMasterSolutionName);

	m_strCompany = DataStr(AfxGetLoginInfos()->m_strCompanyName);
	m_strService = strInstallation + _T("/TBSender/PLProxy.asmx");	
	m_strServer = strWebServer;
	m_nWebServicesPort = AfxGetCommonClientObjects()->GetServerConnectionInfo()->m_nWebServicesPort;
}

//----------------------------------------------------------------------------
TbSenderInterface::~TbSenderInterface(void)
{
}

//----------------------------------------------------------------------------
CString TbSenderInterface::GetCadastralCode(DataStr& strCity, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("GetCadastralCode"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddStrParam(_T("city"), strCity);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);		

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return _T("");
	}
	DataStr* result = (DataStr*)aFunctionDescription.GetReturnValue();
	return result->Str();
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::GetLegalInfos
	(	
		DataStr& strlanguage,
		DataStr& strRestrictiveClauses,
		DataStr& strIban,
		DataStr& strBankName,
		DataStr& strBeneficiary,
		DataStr& strPrivacyPolicy,
		DataStr& strPriceListUrl,
		DataStr& strTermsOfUse,
		DataStr& strGeneralConditionsCharge,
		DataStr& strTransparancyObligations,
		DataStr& strCadastralPageUrl,
		DataMon& minimumRecharge,
		DataStr& errorMessage
	)
{
	CFunctionDescription aFunctionDescription(_T("GetLegalInfos"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddStrParam(_T("language"), strlanguage);
	aFunctionDescription.AddOutParam(_T("restrictiveClauses"), &strRestrictiveClauses);
	aFunctionDescription.AddOutParam(_T("iban"), &strIban);
	aFunctionDescription.AddOutParam(_T("bankName"), &strBankName);
	aFunctionDescription.AddOutParam(_T("beneficiary"), &strBeneficiary);
	aFunctionDescription.AddOutParam(_T("privacyPolicy"), &strPrivacyPolicy);		
	aFunctionDescription.AddOutParam(_T("priceListUrl"), &strPriceListUrl);		
	aFunctionDescription.AddOutParam(_T("termsOfUse"), &strTermsOfUse);		
	aFunctionDescription.AddOutParam(_T("generalConditionsCharge"), &strGeneralConditionsCharge);		
	aFunctionDescription.AddOutParam(_T("transparancyObligations"), &strTransparancyObligations);		
	aFunctionDescription.AddOutParam(_T("cadastralPageUrl"), &strCadastralPageUrl);		
	aFunctionDescription.AddOutParam(_T("minimumRecharge"), &minimumRecharge);		
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);	

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}
	
	return *((DataBool*)aFunctionDescription.GetReturnValue());
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::GetEstimateCharge
	(
		DataStr& strLogin, 
		DataStr& strToken,
		DataMon& amountCharge,
		DataMon& vat,
		DataMon& amountActivation,
		DataMon& totalAmount,
		DataStr& strIban,
		DataStr& strNameBank,
		DataStr& strBeneficiary,
		DataStr& errorMessage
	)
{
	CFunctionDescription aFunctionDescription(_T("GetEstimateCharge"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddStrParam(_T("login"), strLogin);
	aFunctionDescription.AddStrParam(_T("token"), strToken);
	aFunctionDescription.AddOutParam(_T("amountCharge"), &amountCharge);
	aFunctionDescription.AddOutParam(_T("vat"), &vat);
	aFunctionDescription.AddOutParam(_T("amountActivation"), &amountActivation);
	aFunctionDescription.AddOutParam(_T("totalAmount"), &totalAmount);
	aFunctionDescription.AddOutParam(_T("iban"), &strIban);				
	aFunctionDescription.AddOutParam(_T("nameBank"), &strNameBank);			
	aFunctionDescription.AddOutParam(_T("beneficiary"), &strBeneficiary);		
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);		

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}
	
	return *((DataBool*)aFunctionDescription.GetReturnValue());
}

//----------------------------------------------------------------------------
CString TbSenderInterface::Subscribe
	( 
		DataStr& strCompanyName,
		DataStr& strCity,
		DataStr& strAddress,
		DataStr& strZipCode,
		DataStr& strCounty,
		DataStr& strCountry,
		DataStr& strPrefixTelephoneNumber,
		DataStr& strTelephoneNumber,
		DataStr& strVatNumber,
		DataStr& strFiscalCode,
		DataStr& strEMail,	
		DataStr& strLegalCode,
		DataStr& strActivityCode,
		DataStr& strCadastralCode,
		DataBool& bPrivateEntity,
		DataStr& strPrivateEntityOption,
		DataStr& strSenderCompanyName,
		DataStr& strSenderCity,
		DataStr& strSenderAddress,
		DataStr& strSenderZipCode,
		DataStr& strSenderCounty,
		DataStr& strSenderCountry,
		DataStr& strSenderVatNumber,
		DataStr& strSenderFiscalCode,
		DataStr& strSenderEMail,	
		DataStr& strSenderLegalCode,
		DataStr& strSenderActivityCode,
		DataStr& strLoginId,
		DataStr& errorMessage
	)
{
	CFunctionDescription aFunctionDescription(_T("Subscribe"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddStrParam(_T("companyName"), strCompanyName);
	aFunctionDescription.AddStrParam(_T("city"), strCity);
	aFunctionDescription.AddStrParam(_T("address"), strAddress);
	aFunctionDescription.AddStrParam(_T("zipCode"), strZipCode);
	aFunctionDescription.AddStrParam(_T("county"), strCounty);
	aFunctionDescription.AddStrParam(_T("country"), strCountry);
	aFunctionDescription.AddStrParam(_T("prefixTelephoneNumber"), strPrefixTelephoneNumber);
	aFunctionDescription.AddStrParam(_T("telephoneNumber"), strTelephoneNumber);
	aFunctionDescription.AddStrParam(_T("vatNumber"), strVatNumber);
	aFunctionDescription.AddStrParam(_T("fiscalCode"), strFiscalCode);
	aFunctionDescription.AddStrParam(_T("eMail"), strEMail);
	aFunctionDescription.AddStrParam(_T("legalCode"), strLegalCode);
	aFunctionDescription.AddStrParam(_T("activityCode"), strActivityCode);
	aFunctionDescription.AddStrParam(_T("areaCode"), strCadastralCode);
	aFunctionDescription.AddParam(_T("privateEntity"), &bPrivateEntity);
	aFunctionDescription.AddStrParam(_T("privateEntityOption"), strPrivateEntityOption);
	aFunctionDescription.AddStrParam(_T("senderCompanyName"), strSenderCompanyName);
	aFunctionDescription.AddStrParam(_T("senderCity"), strSenderCity);
	aFunctionDescription.AddStrParam(_T("senderAddress"), strSenderAddress);
	aFunctionDescription.AddStrParam(_T("senderZipCode"), strSenderZipCode);
	aFunctionDescription.AddStrParam(_T("senderCounty"), strSenderCounty);
	aFunctionDescription.AddStrParam(_T("senderCountry"), strSenderCountry);
	aFunctionDescription.AddStrParam(_T("senderVatNumber"), strSenderVatNumber);
	aFunctionDescription.AddStrParam(_T("senderFiscalCode"), strSenderFiscalCode);
	aFunctionDescription.AddStrParam(_T("senderEMail"), strSenderEMail);
	aFunctionDescription.AddStrParam(_T("senderLegalCode"), strSenderLegalCode);
	aFunctionDescription.AddStrParam(_T("senderActivityCode"), strSenderActivityCode);
	aFunctionDescription.AddOutParam(_T("loginId"), &strLoginId);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return _T("");
	}
	DataStr* result = (DataStr*)aFunctionDescription.GetReturnValue();
	return result->Str();
}


//----------------------------------------------------------------------------
CString TbSenderInterface::Login
	(
		DataStr& login,
		DataStr& password,
		DataStr& surNameCompanyName,
		DataStr& city,
		DataStr& address,
		DataStr& zipCode,
		DataStr& county,
		DataStr& state,
		DataStr& prefixTelephoneNumber,
		DataStr& telephoneNumber,
		DataStr& vatNumber,
		DataStr& fiscalCode,
		DataStr& eMail,
		DataStr& legalCode,
		DataStr& activityCode,
		DataStr& areaCode,
		DataBool& privateEntity,
		DataStr& privateEntityOption,
		DataStr& senderCompanyName,
		DataStr& senderCity,
		DataStr& senderAddress,
		DataStr& senderZipCode,
		DataStr& senderCounty,
		DataStr& senderCountry,
		DataStr& senderVatNumber,
		DataStr& senderFiscalCode,
		DataStr& senderEMail,
		DataStr& senderLegalCode,
		DataStr& senderActivityCode,
		DataStr& errorMessage
	)
{
	CFunctionDescription aFunctionDescription(_T("Login"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::String, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("loginID"), &login);
	aFunctionDescription.AddStrParam(_T("password"), password);
	aFunctionDescription.AddOutParam(_T("surNameCompanyName"), &surNameCompanyName);
	aFunctionDescription.AddOutParam(_T("city"), &city);
	aFunctionDescription.AddOutParam(_T("address"), &address);
	aFunctionDescription.AddOutParam(_T("zipCode"), &zipCode);
	aFunctionDescription.AddOutParam(_T("county"), &county);
	aFunctionDescription.AddOutParam(_T("state"), &state);
	aFunctionDescription.AddOutParam(_T("prefixTelephoneNumber"), &prefixTelephoneNumber);
	aFunctionDescription.AddOutParam(_T("telephoneNumber"), &telephoneNumber);
	aFunctionDescription.AddOutParam(_T("vatNumber"), &vatNumber);
	aFunctionDescription.AddOutParam(_T("fiscalCode"), &fiscalCode);
	aFunctionDescription.AddOutParam(_T("eMail"), &eMail);
	aFunctionDescription.AddOutParam(_T("legalCode"), &legalCode);
	aFunctionDescription.AddOutParam(_T("activityCode"), &activityCode);
	aFunctionDescription.AddOutParam(_T("areaCode"), &areaCode);
	aFunctionDescription.AddOutParam(_T("privateEntity"), &privateEntity);
	aFunctionDescription.AddOutParam(_T("privateEntityOption"), &privateEntityOption);
	aFunctionDescription.AddOutParam(_T("senderCompanyName"), &senderCompanyName);
	aFunctionDescription.AddOutParam(_T("senderCity"), &senderCity);
	aFunctionDescription.AddOutParam(_T("senderAddress"), &senderAddress);
	aFunctionDescription.AddOutParam(_T("senderZipCode"), &senderZipCode);
	aFunctionDescription.AddOutParam(_T("senderCounty"), &senderCounty);
	aFunctionDescription.AddOutParam(_T("senderCountry"), &senderCountry);
	aFunctionDescription.AddOutParam(_T("senderVatNumber"), &senderVatNumber);
	aFunctionDescription.AddOutParam(_T("senderFiscalCode"), &senderFiscalCode);
	aFunctionDescription.AddOutParam(_T("senderEMail"), &senderEMail);
	aFunctionDescription.AddOutParam(_T("senderLegalCode"), &senderLegalCode);
	aFunctionDescription.AddOutParam(_T("senderActivityCode"), &senderActivityCode);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
		
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return _T("");
	}
	DataStr* result = (DataStr*)aFunctionDescription.GetReturnValue();
	return result->Str();
}

//----------------------------------------------------------------------------
DataMon TbSenderInterface::GetCreditState(DataStr& login, DataStr& token, DataInt& codeState, DataDate& expiryDate, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("GetCreditState"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Money, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("login"), &login);
	aFunctionDescription.AddStrParam(_T("token"), token);
	aFunctionDescription.AddOutParam(_T("codeState"), &codeState);
	aFunctionDescription.AddOutParam(_T("expiryDate"), &expiryDate);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return NULL;
	}

	return *((DataMon*)aFunctionDescription.GetReturnValue());
}

//----------------------------------------------------------------------------
DataInt TbSenderInterface::Charge
	(		
	DataStr& loginId,				
	DataStr& token, 
	DataStr& fileContentBase64, 
	DataMon& amountCharge,
	DataMon& vat,
	DataMon& amountActivation,
	DataMon& totalAmount,
	DataMon& credit, 
	DataDate& expiryDate, 
	DataStr& errorMessage
	)
{
	CFunctionDescription aFunctionDescription(_T("Charge"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Integer, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("loginId"), &loginId);
	aFunctionDescription.AddParam(_T("token"), &token);
	aFunctionDescription.AddStrParam(_T("fileContentBase64"), fileContentBase64);
	aFunctionDescription.AddParam(_T("amountCharge"), &amountCharge);
	aFunctionDescription.AddParam(_T("vat"), &vat);
	aFunctionDescription.AddParam(_T("amountActivation"), &amountActivation);
	aFunctionDescription.AddParam(_T("totalAmount"), &totalAmount);
	aFunctionDescription.AddParam(_T("credit"), &credit);
	aFunctionDescription.AddOutParam(_T("expiryDate"), &expiryDate);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
		return NULL;
	}

	return *((DataInt*)aFunctionDescription.GetReturnValue());
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::AllotMessages(DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("AllotMessages"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}

	return *((DataBool*)aFunctionDescription.GetReturnValue());
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::CreateSingleMessageLot(DataLng& aMsgId, DataBool& aSendImmediately, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("CreateSingleMessageLot"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("msgId"), &aMsgId);
	aFunctionDescription.AddParam(_T("sendImmediately"), &aSendImmediately);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}
	
	return *((DataBool*)aFunctionDescription.GetReturnValue());
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::UploadSingleLot(DataLng& lotID, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("UploadSingleLot"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("lotID"), &lotID);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}

	return *((DataBool*)aFunctionDescription.GetReturnValue());
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::ReopenClosedLot(DataLng& lotId, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("ReopenClosedLot"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("lotId"), &lotId);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::DeleteMessage(DataLng& aMsgId, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("DeleteMessage"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("msgId"), &aMsgId);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::ChangeMessageDeliveryType(DataLng& aMsgId, DataLng& deliveryType, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("ChangeMessageDeliveryType"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("msgId"), &aMsgId);
	aFunctionDescription.AddParam(_T("deliveryType"), &deliveryType);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}
	
	return TRUE;
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::ChangeMessagePrintType(DataLng& aMsgId, DataLng& printType, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("ChangeMessagePrintType"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("msgId"), &aMsgId);
	aFunctionDescription.AddParam(_T("printType"), &printType);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);

	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
void TbSenderInterface::RefreshSettings()
{
	CFunctionDescription aFunctionDescription(_T("RefreshSettings"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Void, CDataObjDescription::_OUT));
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		return;
	}
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::UpdateSentLotsStatus(DataBool& bAsync, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("UpdateSentLotsStatus"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("async"), &bAsync);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::RemoveFromLot(DataLng& msgId, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("RemoveFromLot"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("msgId"), &msgId);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::CloseLot(DataLng& lotId, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("CloseLot"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("lotId"), &lotId);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}

	return TRUE;
}

//----------------------------------------------------------------------------
DataBool TbSenderInterface::GetLotCostEstimate(DataLng& lotID, DataStr& errorMessage)
{
	CFunctionDescription aFunctionDescription(_T("GetLotCostEstimate"));

	aFunctionDescription.SetServer				(m_strServer);
	aFunctionDescription.SetService				(m_strService);
	aFunctionDescription.SetServiceNamespace	(m_strServiceNamespace);
	aFunctionDescription.SetPort				(m_nWebServicesPort);
	
	aFunctionDescription.SetReturnValueDescription(CDataObjDescription(_T(""), DataType::Bool, CDataObjDescription::_OUT));
	aFunctionDescription.AddStrParam(_T("company"), m_strCompany);
	aFunctionDescription.AddParam(_T("lotID"), &lotID);
	aFunctionDescription.AddOutParam(_T("errorMessage"), &errorMessage);
	
	if (!InvokeWCFFunction(&aFunctionDescription, FALSE))
	{
		ASSERT(FALSE);
		if (errorMessage.IsEmpty())
			errorMessage = aFunctionDescription.GetError();
	
		return FALSE;
	}

	return TRUE;
}




	
						

