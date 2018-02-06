#include "stdafx.h"

#include <TbGeneric\ParametersSections.h>

#include "DBarcodeSettings.h"
#include "UIBarcodeSettings.hjson"

/////////////////////////////////////////////////////////////////////////////
//							DBarcodeSettings 
/////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNCREATE(DBarcodeSettings, DCompanyUserSettingsDoc)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DBarcodeSettings, DCompanyUserSettingsDoc)
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
DBarcodeSettings::DBarcodeSettings()
	:
	DCompanyUserSettingsDoc()
{
}

//-----------------------------------------------------------------------------
BOOL DBarcodeSettings::OnAttachData()
{
	if (!__super::OnAttachData())
		return FALSE;

	// json
	DeclareRegisterJson();

	return TRUE;
}

//-----------------------------------------------------------------------------
void DBarcodeSettings::DeclareRegisterJson()
{
	//DefaultBarcode
	DECLARE_VAR_JSON(BarcodeType);
	//DefaultBarcode2DVersion
	DECLARE_VAR_JSON(DataMatrixVersion);
	DECLARE_VAR_JSON(PDF417Version);
	//DefaultBarcode2DEncoding
	DECLARE_VAR_JSON(DataMatrixEncoding);
	DECLARE_VAR_JSON(MicroQREncoding);
	DECLARE_VAR_JSON(QREncoding);
	DECLARE_VAR_JSON(PDF417Encoding);
	//DefaultBarcode2DErrCorrLevel
	DECLARE_VAR_JSON(MicroQRCorrLevel);
	DECLARE_VAR_JSON(QRCorrLevel);
	DECLARE_VAR_JSON(PDF417CorrLevel);
}

//-----------------------------------------------------------------------------
BOOL DBarcodeSettings::GetSaveSettings(BOOL bSave)
{
	if (bSave)
	{
		//DefaultBarcode
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcodeSection, szBarCodeType, m_BarcodeType, szBarcodeConnectorConfigFile);
		//DefaultBarcode2DVersion
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcode2DVersion, szDataMatrix, m_DataMatrixVersion, szBarcodeConnectorConfigFile);
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcode2DVersion, szPDF417, m_PDF417Version, szBarcodeConnectorConfigFile);
		//DefaultBarcode2DEncoding
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcode2DEncoding, szDataMatrix, m_DataMatrixEncoding, szBarcodeConnectorConfigFile);
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcode2DEncoding, szMicroQR, m_MicroQREncoding, szBarcodeConnectorConfigFile);
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcode2DEncoding, szQR, m_QREncoding, szBarcodeConnectorConfigFile);
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcode2DEncoding, szPDF417, m_PDF417Encoding, szBarcodeConnectorConfigFile);
		//DefaultBarcode2DErrCorrLevel	
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcode2DErrCorrLevel, szMicroQR, m_MicroQRCorrLevel, szBarcodeConnectorConfigFile);
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcode2DErrCorrLevel, szQR, m_QRCorrLevel, szBarcodeConnectorConfigFile);
		AfxSetSettingValue(snsTbWoormViewer, szDefaultBarcode2DErrCorrLevel, szPDF417, m_PDF417CorrLevel, szBarcodeConnectorConfigFile);		
		
		CCustomSaveInterface aCustomSaveInterface;
		aCustomSaveInterface.m_bSaveAllFile = TRUE;
		aCustomSaveInterface.m_bSaveAllUsers = TRUE;
		aCustomSaveInterface.m_eSaveMode = CCustomSaveInterface::COMPANY_USERS;
		return AfxSaveSettingsFile(snsTbWoormViewer, szBarcodeConnectorConfigFile, TRUE, &aCustomSaveInterface);
	}
	else	
	{
		//DefaultBarcode
		m_BarcodeType = *(DataStr*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcodeSection, szBarCodeType, DataStr(), szBarcodeConnectorConfigFile);
		//DefaultBarcode2DVersion
		m_DataMatrixVersion = *(DataStr*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DVersion, szDataMatrix, DataStr(_T("Auto")), szBarcodeConnectorConfigFile);
		m_PDF417Version = *(DataStr*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DVersion, szPDF417, DataStr(_T("0x0")), szBarcodeConnectorConfigFile);
		//DefaultBarcode2DEncoding
		m_DataMatrixEncoding = *(DataInt*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DEncoding, szDataMatrix, DataInt(0), szBarcodeConnectorConfigFile);
		m_MicroQREncoding = *(DataInt*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DEncoding, szMicroQR, DataInt(-1), szBarcodeConnectorConfigFile);
		m_QREncoding = *(DataInt*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DEncoding, szQR, DataInt(-1), szBarcodeConnectorConfigFile);
		m_PDF417Encoding = *(DataInt*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DEncoding, szPDF417, DataInt(-1), szBarcodeConnectorConfigFile);
		//DefaultBarcode2DErrCorrLevel
		m_MicroQRCorrLevel = *(DataStr*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DErrCorrLevel, szMicroQR, DataStr(_T("M")), szBarcodeConnectorConfigFile);
		m_QRCorrLevel = *(DataStr*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DErrCorrLevel, szQR, DataStr(_T("M")), szBarcodeConnectorConfigFile);
		m_PDF417CorrLevel = *(DataInt*)AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DErrCorrLevel, szPDF417, DataInt(-1), szBarcodeConnectorConfigFile);
	}
	return TRUE;
}

