#pragma once
#include <TbGes\DParametersDoc.h>
#include <TbGenlibUI\SettingsTableManager.h>
#include "beginh.dex"


/////////////////////////////////////////////////////////////////////////////
//							DBarcodeSettings 
/////////////////////////////////////////////////////////////////////////////
class DBarcodeSettings : public DCompanyUserSettingsDoc
{
	DECLARE_DYNCREATE(DBarcodeSettings)

	// Construction
public:
	DBarcodeSettings();

public:
	//Setting BARCODE
	//DefaultBarcode
	DECLARE_SETTING(Str, BarcodeType);
	//DefaultBarcode2DVersion
	DECLARE_SETTING(Str, DataMatrixVersion);
	DECLARE_SETTING(Str, PDF417Version);
	//DefaultBarcode2DEncoding
	DECLARE_SETTING(Int, DataMatrixEncoding);
	DECLARE_SETTING(Int, MicroQREncoding);
	DECLARE_SETTING(Int, QREncoding);
	DECLARE_SETTING(Int, PDF417Encoding);
	//DefaultBarcode2DErrCorrLevel
	DECLARE_SETTING(Str, MicroQRCorrLevel);
	DECLARE_SETTING(Str, QRCorrLevel);
	DECLARE_SETTING(Int, PDF417CorrLevel);

protected:
	virtual BOOL	OnAttachData();
	virtual BOOL	GetSaveSettings(BOOL bSave = FALSE);
	// json
	void	DeclareRegisterJson();


protected:
	//{{AFX_MSG(DBarcodeSettings)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

#include "endh.dex"
