#pragma once

#include <TbGeneric/CMapi.h>

#include "beginh.dex"

class TB_EXPORT CPostaLiteNet
{
public:
	//CPostaLiteNet(void) {}
	//virtual ~CPostaLiteNet(void) {}

	static BOOL SavePdfBlob 
	(
		CString sConnectionString,
		CString sFileName,
		int nMsgID,
		CDiagnostic* pDiagnostic = NULL
	);

	static BOOL CPostaLiteNet::ReadPdfBlob 
	(
		CString sConnectionString,
		CString sFileName,
		int nMsgID,
		CDiagnostic* pDiagnostic = NULL
	);

	static CString GetPostaliteCountryFromIsoCode (const CString& isoCode);

};

#include "endh.dex"