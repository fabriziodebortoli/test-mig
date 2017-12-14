#pragma once


#include <TBGeneric\FunctionCall.h>
#include <TbGenlibManaged\Main.h>
#include <TbClientCore\ClientObjects.h>
#include <TbGenlib\\baseapp.h>

class CFunctionDescription;
//----------------------------------------------------------------------------
class TB_EXPORT TbSenderInterface
{
private:
	CString m_strService;
	CString m_strServiceNamespace;
	CString m_strServer;
	DataStr m_strCompany;
	int m_nWebServicesPort;

public:				
	TbSenderInterface::TbSenderInterface();
	TbSenderInterface(const CString& strServer, const CString& strService, int nWebServicesPort);
	~TbSenderInterface(void);

	CString GetCadastralCode(DataStr& strCity, DataStr& strErrorMessage);

	CString Subscribe( 
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
			);

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
		);

	DataMon GetCreditState
		(
		DataStr& login,
		DataStr& token,
		DataInt& codeState,
		DataDate& expiryDate,
		DataStr& errorMessage
		);

	DataInt Charge
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
	);

	DataBool GetEstimateCharge
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
		DataStr& strErrorMessage
	);

	DataBool GetLegalInfos
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
		DataStr& strErrorMessage
	);

	DataBool	AllotMessages(DataStr& errorMessage);
	DataBool	CreateSingleMessageLot(DataLng& aMsgId, DataBool& aSendImmediately,	DataStr& errorMessage);
	DataBool	UploadSingleLot(DataLng& lotID,	DataStr& errorMessage);
	void		RefreshSettings();
	
	DataBool	CloseLot(DataLng& lotId, DataStr& errorMessage);
	DataBool	RemoveFromLot(DataLng& lotId, DataStr& errorMessage);
	DataBool	ReopenClosedLot(DataLng& lotId, DataStr& errorMessage);
	DataBool	DeleteMessage(DataLng& aMsgId,	DataStr& errorMessage);
	DataBool	ChangeMessageDeliveryType(DataLng& aMsgId, DataLng& deliveryType,	DataStr& errorMessage);
	DataBool	ChangeMessagePrintType(DataLng& aMsgId, DataLng& printType,	DataStr& errorMessage);
	DataBool	UpdateSentLotsStatus(DataBool& bAsync, DataStr& errorMessage);
	DataBool	GetLotCostEstimate(DataLng& lotID, DataStr& errorMessage);
};

