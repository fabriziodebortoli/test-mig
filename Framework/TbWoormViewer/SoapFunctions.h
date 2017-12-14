
#pragma once

#include <TbGeneric\DataObj.h>

#include "beginh.dex"

DataBool ExecNewReport (DataLng& DocHandle);
DataBool ExecOpenReport (DataStr reportNameSpace, DataLng& docHandle);
DataBool ExecUpgradeReport ();

TB_EXPORT DataBool IsUserReportsDeveloper ();

TB_EXPORT DataStr GetCompanyInfo(DataStr/*[ciString]*/ tagProperty);

TB_EXPORT DataBool GetDocumentMethods (DataLng handleDoc, DataArray& /*string*/ arMethods);

//=============================================================================
TB_EXPORT DataBool IsPostaLiteEnabled ();

TB_EXPORT DataStr PostaLiteDecodeDeliveryType(DataInt deliveryType);

TB_EXPORT DataStr PostaLiteDecodePrintType(DataInt printType);

TB_EXPORT DataStr PostaLiteDecodeStatus(DataInt status);

TB_EXPORT DataStr PostaLiteDecodeCodeState(DataInt codeState);

TB_EXPORT DataStr PostaLiteDecodeStatusExt(DataLng status);

TB_EXPORT DataStr PostaLiteDecodeErrorExt(DataLng error);

//=============================================================================
#include "endh.dex"