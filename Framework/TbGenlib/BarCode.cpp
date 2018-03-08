#include "stdafx.h"

// Extern library declarations
#include <bclw/bclw.h>
#include <cmath>

#include <TbGeneric\ParametersSections.h>
#include <TbGeneric\SettingsTable.h>

#include <TbParser\SymTable.h>
#include <TbParser\Parser.h>

#include <TbGenLibManaged\BarCodeCreator.h>


#include "BaseDoc.h"

#include "BarCode.h"
#include "BarCode.hjson" //JSON AUTOMATIC UPDATE


//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif
//------------------------------------------------------------------------------
int CBarCodeTypes::s_bcTypes[CBarCodeTypes::BARCODE_TYPES_NUM] = 
					{ 
						BC_DEFAULT,
						BC_UPCA,
						BC_UPCE,
						BC_EAN13,
						BC_EAN8,
						BC_CODE39,
						BC_EXT39,
						BC_INT25,
						BC_CODE128,
						BC_CODABAR,
						BC_ZIP,
						BC_MSIPLESSEY,
						BC_CODE93,
						BC_EXT93,
						BC_UCC128,
						BC_HIBC,
						BC_PDF417,
						BC_UPCE0,
						BC_UPCE1,
						BC_CODE128A,
						BC_CODE128B,
						BC_CODE128C,
						BC_EAN128,
						BC_DATAMATRIX,
						BC_MicroQR,
						BC_QR
					};

//------------------------------------------------------------------------------
CBarCodeTypes::SBarCodeTypes CBarCodeTypes::s_arBarCodeTypes [CBarCodeTypes::BARCODE_TYPES_NUM] = 
{
	{ BC_DEFAULT,	 _T("Default"),		E_BARCODE_TYPE_DEFAULT,					-1 }, 
	{ BC_UPCA,		 _T("UPCA"),		E_BARCODE_TYPE_UPC_A,					11 }, 
	{ BC_UPCE,		 _T("UPCE"),		E_BARCODE_TYPE_UPC_E,					 7 }, //'1' o '0' + 6 cifre + checkdigit calcolato da upca corrispondente
	{ BC_EAN13,		 _T("EAN13"),		E_BARCODE_TYPE_EAN13,					12 },
	{ BC_EAN8,		 _T("EAN8"),		E_BARCODE_TYPE_EAN8,					 7 },
	{ BC_CODE39,	 _T("CODE39"),		E_BARCODE_TYPE_ALFA39,					-1 }, //lettere maiusc + num, Checkdigit mod 43
	{ BC_EXT39,		 _T("EXT39"),		E_BARCODE_TYPE_EXTENDED_CODE_39,		-1 }, //tutte le lettere e num
	{ BC_INT25,		 _T("INT25"),		E_BARCODE_TYPE_INTERLEAVED_2_OF_5,		-1 }, //solo numeri, cifre in numero pari
	{ BC_CODE128,	 _T("CODE128"),		E_BARCODE_TYPE_CODE_128_AUTO,			-1 },
	{ BC_CODABAR,	 _T("CODABAR"),		E_BARCODE_TYPE_CODABAR,					-1 },
	{ BC_ZIP,		 _T("ZIP"),			E_BARCODE_TYPE_ZIP_CODE,				-1 },
	{ BC_MSIPLESSEY, _T("MSIPLESSEY"),	E_BARCODE_TYPE_MSI_PLESSEY,				-1 },
	{ BC_CODE93,	 _T("CODE93"),		E_BARCODE_TYPE_CODE_93,					-1 },
	{ BC_EXT93,		 _T("EXT93"),		E_BARCODE_TYPE_EXTENDED_CODE_93,		-1 }, //non supportato da GDPicture.NET
	{ BC_UCC128,	 _T("UCC128"),		E_BARCODE_TYPE_UCC128,					-1 }, //AI codificati pi� una serie di combinazioni diverse
	{ BC_HIBC,		 _T("HIBC"),		E_BARCODE_TYPE_HIBC,					-1 }, //alfanumerico non supportato da GDPicture.NET
	{ BC_PDF417,	 _T("PDF417"),		E_BARCODE_TYPE_PDF417,					-1 },
	{ BC_UPCE0,		 _T("UPCE0"),		E_BARCODE_TYPE_UPCE_E0,					 6 },
	{ BC_UPCE1,		 _T("UPCE1"),		E_BARCODE_TYPE_UPCE_E1,					 6 },
	{ BC_CODE128A,	 _T("CODE128A"),	E_BARCODE_TYPE_CODE_128_A,				-1 },
	{ BC_CODE128B,	 _T("CODE128B"),	E_BARCODE_TYPE_CODE_128_B,				-1 },
	{ BC_CODE128C,	 _T("CODE128C"),	E_BARCODE_TYPE_CODE_128_C,				-1 },
	{ BC_EAN128,	 _T("EAN128"),		E_BARCODE_TYPE_EAN128,					-1 },
	{ BC_DATAMATRIX, _T("DataMatrix"),	E_BARCODE_TYPE_DATAMATRIX,				-1 },
	{ BC_MicroQR,	 _T("MicroQR"),		E_BARCODE_TYPE_MICROQR,					-1 },
	{ BC_QR,		 _T("QR"),			E_BARCODE_TYPE_QR,						-1 }
};
	
//------------------------------------------------------------------------------
CBarCodeTypes::SBarCodeDataMatrixVersions CBarCodeTypes::s_arBarCodeDataMatrixVersions[CBarCodeTypes::BARCODE_DM_VERSIONS_NUM] =
{
	{ 0	,	_T("Auto") },
	{ 1	,	_T("10x10") } ,
	{ 2	,	_T("12x12") },
	{ 3	,	_T("14x14") },
	{ 4	,	_T("16x16") },
	{ 5	,	_T("18x18") },
	{ 6	,	_T("20x20") },
	{ 7	,	_T("22x22") },
	{ 8	,	_T("24x24") },
	{ 9	,	_T("26x26") },
	{ 10,	_T("32x32") },
	{ 11,	_T("36x36") },
	{ 12,	_T("40x40") },
	{ 13,	_T("44x44") },
	{ 14,	_T("48x48") },
	{ 15,	_T("52x52") },
	{ 16,	_T("64x64") },
	{ 17,	_T("72x72") },
	{ 18,	_T("80x80") },
	{ 19,	_T("88x88") },
	{ 20,	_T("96x96") } ,
	{ 21,	_T("104x104") },
	{ 22,	_T("120x120") },
	{ 23,	_T("132x132") },
	{ 24,	_T("144x144") },
	{ 25,	_T("8x18") },
	{ 26,	_T("8x32") },
	{ 27,	_T("12x26") },
	{ 28,	_T("12x36") },
	{ 29,	_T("16x36") },
	{ 30,	_T("16x48") }
};

//------------------------------------------------------------------------------
CString CBarCodeTypes::BarCodeDescription (int nBarcCodeType)
{
	for (int i = 0; i < BARCODE_TYPES_NUM; i++)
		if (s_arBarCodeTypes[i].m_nType == nBarcCodeType)
			return s_arBarCodeTypes[i].m_sName;
	ASSERT(FALSE);
	return _T("");
}

int	CBarCodeTypes::BarCodeStandardLength(int nBarcCodeType)
{
	for (int i = 0; i < BARCODE_TYPES_NUM; i++)
		if (s_arBarCodeTypes[i].m_nType == nBarcCodeType)
			return s_arBarCodeTypes[i].m_nStdLength;
	ASSERT(FALSE);
	return -1;
}

//------------------------------------------------------------------------------
CString CBarCodeTypes::BarCodeDMVersionDescription(int nBarcodeVersion)
{
	for (int i = 0; i < BARCODE_DM_VERSIONS_NUM; i++)
		if (s_arBarCodeDataMatrixVersions[i].m_nType == nBarcodeVersion)
			return s_arBarCodeDataMatrixVersions[i].m_sName;
	ASSERT(FALSE);
	return _T("");
}

//------------------------------------------------------------------------------
int CBarCodeTypes::BarCodeDMVersion(const CString& sName, BOOL bDefaultERR)
{
	for (int i = 0; i < BARCODE_DM_VERSIONS_NUM; i++)
		if (sName.CompareNoCase(s_arBarCodeDataMatrixVersions[i].m_sName) == 0)
			return s_arBarCodeDataMatrixVersions[i].m_nType;
	return bDefaultERR ? 0 : -1;
}

//------------------------------------------------------------------------------
int CBarCodeTypes::BarCodeType (const CString& sName, BOOL bDefaultERR)
{
	for (int i = 0; i < BARCODE_TYPES_NUM; i++)
		if(sName.CompareNoCase(s_arBarCodeTypes[i].m_sName) == 0)
			return s_arBarCodeTypes[i].m_nType;
	return bDefaultERR ? BC_DEFAULT : -1;
}

//------------------------------------------------------------------------------
int CBarCodeTypes::BarCodeType (DWORD eBc, BOOL bDefaultERR)
{
	for (int i = 0; i < BARCODE_TYPES_NUM; i++)
		if (eBc == s_arBarCodeTypes[i].m_eBc)
			return s_arBarCodeTypes[i].m_nType;
	return bDefaultERR ? BC_DEFAULT : -1;
}

//------------------------------------------------------------------------------
DWORD CBarCodeTypes::BarCodeEnum (int nType, BOOL bDefaultERR)
{
	for (int i = 0; i < BARCODE_TYPES_NUM; i++)
		if (nType == s_arBarCodeTypes[i].m_nType)
			return s_arBarCodeTypes[i].m_eBc;
	return bDefaultERR ? E_BARCODE_TYPE_DEFAULT : -1;
}

//------------------------------------------------------------------------------
DWORD CBarCodeTypes::BarCodeEnum (const CString& sName, BOOL bDefaultERR)
{
	for (int i = 0; i < BARCODE_TYPES_NUM; i++)
		if (sName.CompareNoCase(s_arBarCodeTypes[i].m_sName) == 0)
			return s_arBarCodeTypes[i].m_eBc;
	return bDefaultERR ? E_BARCODE_TYPE_DEFAULT : -1;
}

//------------------------------------------------------------------------------
CString	CBarCodeTypes::TypedBarCode (const CString& strCode, int nBarCodeType, int nChkSum/* = 0*/, const CString& sHumanReadable /*= _T("")*/, int nNarrowBar/* = -1*/, int nBarHeight/* = -1*/, const CString& sVersion /*= _T("")*/, int nErrCorrLevel/* = -2*/)
{
	CString s (strCode + CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nBarCodeType));
	if (nErrCorrLevel > -2)
		s += CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nChkSum) + CBarCodeTypes::BC_SEP + sHumanReadable + CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nNarrowBar) + CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nBarHeight) + CBarCodeTypes::BC_SEP + sVersion + CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nErrCorrLevel);
	else if (!sVersion.IsEmpty())
		s += CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nChkSum) + CBarCodeTypes::BC_SEP + sHumanReadable + CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nNarrowBar) + CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nBarHeight) + CBarCodeTypes::BC_SEP + sVersion;
	else if (nBarHeight > -1)
		s += CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nChkSum) + CBarCodeTypes::BC_SEP + sHumanReadable + CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nNarrowBar) + CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nBarHeight);
	else if (nNarrowBar > -1)
		s += CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nChkSum) + CBarCodeTypes::BC_SEP + sHumanReadable + CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nNarrowBar);
	else if (!sHumanReadable.IsEmpty())
		s += CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nChkSum) + CBarCodeTypes::BC_SEP + sHumanReadable;
	else if (nChkSum > 0)
		s += CBarCodeTypes::BC_SEP + cwsprintf(_T("%d"), nChkSum);

	return s;
}

//------------------------------------------------------------------------------
BOOL CBarCodeTypes::Is2DBarcodeType(DWORD nBarCodeType)
{
	return (nBarCodeType == BC_DATAMATRIX
		|| nBarCodeType == BC_MicroQR
		|| nBarCodeType == BC_QR
		|| nBarCodeType == BC_PDF417);
}

//------------------------------------------------------------------------------
BOOL CBarCodeTypes::ReadPDF417Version(CString& sVersion, int& nRows, int& nColumns)
{
	nRows = 0;
	nColumns = 0;
	CString strVersion = sVersion.MakeLower();

	int index = strVersion.Find(_T("x"));
	if (index < 0 || strVersion.Find(_T("x"), index + 1) > 0)
		return FALSE; //correct format nxm

	CString strRowsNo = sVersion.Left(index);
	nRows = _ttoi(strRowsNo);
	CString strColumnssNo = sVersion.Right((sVersion.GetLength() - (index + 1)));
	nColumns = _ttoi(strColumnssNo);
	if (nRows == 0 || nColumns == 0)
		return FALSE;
	return TRUE;

}

//------------------------------------------------------------------------------
BOOL CBarCodeTypes::CheckAndCompleteBCString(CString& barcode, int nBarCodeType, CString& errMsg)
{
	if (barcode.GetLength() < 1)
	{ 
		errMsg = _T("Invalid format: empty barcode");
		return FALSE;
	}
		
	//da setting
	BOOL bCheckUCC128Format = false;

	// barcode con lunghezza fissata (UPCA, UPCE, EAN8, EAN13, ZIP)
	int nMinLength = BarCodeStandardLength(nBarCodeType);
	int nMaxLength = nMinLength;

	if (nBarCodeType == BC_INT25)
	{
		// numero pari di cifre
		if (barcode.GetLength() % 2 != 0)
			barcode = _T("0") + barcode;
		return TRUE;
	}
	else if (nBarCodeType == BC_CODABAR)
	{
		// formato (abcd) + numeri + (abcd)
		if (barcode.GetLength() < 2)
		{
			errMsg = _T("Invalid format: too much short content");
			return FALSE;
		}
			
		TCHAR cFirst = barcode.GetAt(0);
		TCHAR cLast = barcode.GetAt(barcode.GetLength() - 1);

		if (cFirst < 65
			|| (cFirst > 69 && cFirst< 97)
			|| cFirst > 100)
		{
			barcode = _T("A") + barcode;
		}

		if (cLast < 65
			|| (cLast > 69 && cLast< 97)
			|| cLast > 100)
		{
			barcode = barcode + _T("B");
		}
	}
	else if (bCheckUCC128Format
		&& (nBarCodeType == BC_UCC128 || nBarCodeType == BC_EAN128))
	{
		if (barcode.GetLength() < 3)
		{			
			errMsg = _T("Invalid format: too much short content");
			return FALSE;
		}
			
		CString AI_2 = barcode.Left(2);
		CString AI_3 = barcode.Left(3);


		// primi due/tre caratteri: AI - Codice tipo barcode, comanda la struttura dati successiva
		if(AI_2 == _T("00"))
		{
			// '00' Serial Shipping Container Code (SSCC-18) 18 digits
			nMinLength = 20;
			nMaxLength = nMinLength;
		}
		else if (AI_2 == _T("01")
			|| AI_2 == _T("02"))
		{
			// '01' Shipping Contained Code (SSCC-14) 14 Digits
			// '02' Number of containers contained in another unit (used with AI 37) 14 Digits
			nMinLength = 16;
			nMaxLength = nMinLength;
		}
		else if (AI_2 == _T("10")
			|| AI_2 == _T("21"))
		{
			// '10' Batch Numbers 1-20 Alphanumeric
			// '21' Serial Number 1-20 Alphanumeric
			nMinLength = 3;
			nMaxLength = 20;
		}
		else if ( AI_2 == _T("11")
			|| AI_2 == _T("13")
			|| AI_2 == _T("15")
			|| AI_2 == _T("17")
			)
		{
			//	'11' Production Date 6 Digits: YYMMDD
			//	'13' Packaging Date 6 Digits : YYMMDD
			//	'15' Sell By Date(Quality Control) 6 Digits : YYMMDD
			//	'17' Expiration Date(Safety Control) 6 Digits : YYMMDD
			CString strDate = barcode.Right(barcode.GetLength() - 2);
			CString dateErrMsg = _T("Invalid format: for AI = 11,13,15,17 content must be a date YYMMDD");
			if (strDate.GetLength() < 6)
			{
				errMsg = dateErrMsg;
				return FALSE;
			}
			else if (strDate.GetLength() > 6)
				strDate = strDate.Left(6);	

			// check year
			char* ptr = 0;
			CString currDigit = strDate.Left(2);
			ptr = (char*)currDigit.GetBuffer(currDigit.GetLength());
			int readDigits = atoi(ptr);
			currDigit.ReleaseBuffer();

			if (readDigits < 0 || readDigits > 99)
			{
				errMsg = dateErrMsg;
				return FALSE;
			}
			strDate = strDate.Right(4);

			// month
			currDigit = strDate.Left(2);
			ptr = (char*)currDigit.GetBuffer(currDigit.GetLength());
			int month = atoi(ptr);
			currDigit.ReleaseBuffer();
			if (month < 1 || month > 12)
			{
				errMsg = dateErrMsg;
				return FALSE;
			}

			// day
			currDigit = strDate.Right(2);
			ptr = (char*)currDigit.GetBuffer(currDigit.GetLength());
			readDigits = atoi(ptr);
			currDigit.ReleaseBuffer();
			if (readDigits < 1 
				|| (month == 2 && readDigits > 29 )
				|| ((month == 11 || month == 4 || month == 6 || month == 9 ) && month > 30 ))
			{
				errMsg = dateErrMsg;
				return FALSE;
			}
			nMaxLength = nMinLength = 8;			
		}
		else if(AI_2 == _T("20"))
		{ 
			// '20' Product Variant 2 Digits
			nMinLength = 4;
			nMaxLength = nMaxLength;
		}
		else if (AI_2 == _T("22"))
		{
			// '22' HIBCC Quantity, Date, Batch and Link 1-29 Alphanumeric
			nMinLength = 3;
			nMaxLength = 29;
		}
		else if (AI_2 == _T("23"))
		{
			// '23x' Lot Number 1-19 Alphanumeric
			nMinLength = 4;
			nMaxLength = 20;
		}
		else if (AI_3 == _T("240")
			|| AI_3 == _T("250"))
		{
			// '240' Additional Product Identification 1 - 30 Alphanumeric
			nMinLength = 4;
			nMaxLength = 30;
		}
		else if (AI_3[0] == '3')
		{
			// various quantity
			char* buffer = (char*) AI_3.GetBuffer(AI_3.GetLength());
			int nAI = atoi(buffer);
			if ( (nAI >= 310 && nAI <=316 )
				||( nAI >= 320 && nAI <= 336)
				|| (nAI >= 340 && nAI <= 356)
				|| (nAI >= 360 && nAI <= 369))
			{ 
				nMinLength = 10;
				nMaxLength = 10;
			}
			AI_3.ReleaseBuffer();
		}
		else if (AI_2 == _T("37"))
		{
			// '37' Number of Units Contained (Used with AI 02) 1-8 Digits
			nMinLength = 3;
			nMaxLength = 10;
		}
		else if (AI_3 == _T("400"))
		{
			// '400' Customer Purchase Order Number 1-29 Alphanumeric 
			nMinLength = 3;
			nMaxLength = 10;
		}
		else if (AI_3 == _T("410")
			|| AI_3 == _T("411")
			|| AI_3 == _T("412"))
		{
			// '400' Customer Purchase Order Number 1-29 Alphanumeric
			// '410' Ship To / Deliver To Location Code(EAN13 or DUNS code) 13 Digits
			// '411' Bill To / Invoice Location Code(EAN13 or DUNS code) 13 Digits
			// '412' Purchase From Location Code(EAN13 or DUNS code) 13 Digits
			nMinLength = 4;
			nMaxLength = 16;
		}
		else if (AI_3 == _T("420"))
		{
			// '420' Ship To/Deliver To Postal Code (Single Postal Authority) 1-9 Alphanumeric
			nMinLength = 4;
			nMaxLength = 12;
		}
		else if (AI_3 == _T("421"))
		{
			// '421' Ship To/Deliver To Postal Code (Multiple Postal Authority) 4-12 Alphanumeric
			nMinLength = 7;
			nMaxLength = 15;
		}
		else if (barcode.GetLength() >= 4 && barcode.Left(4) == _T("8001"))
		{
			// '8001' Roll Products - Width/Length/Core Diameter 14 Digits
			nMinLength = 18;
			nMaxLength = 18;
		}
		else if (barcode.GetLength() >= 4 && barcode.Left(4) == _T("8002"))
		{
			// '8002' Electronic Serial Number (ESN) for Cellular Phone 1-20 Alphanumeric 
			nMinLength = 5;
			nMaxLength = 25;
		}
		else if (barcode.GetLength() >= 4 && barcode.Left(4) == _T("8003"))
		{
			// '8003' UPC/EAN Number and Serial Number of Returnable Asset 14 Digit UPC + 1-16 Alphanumeric Serial Number 
			nMinLength = 19;
			nMaxLength = 34;
		}
		else if (barcode.GetLength() >= 4 && barcode.Left(4) == _T("8004"))
		{
			// '8003' UPC/EAN Serial Identification 1-30 Alphanumeric 
			nMinLength = 5;
			nMaxLength = 34;
		}
		else if (barcode.GetLength() >= 4 && (barcode.Left(4) == _T("8005") || barcode.Left(4) == _T("8100")))
		{
			// '8005' Price per Unit of Measure 6 Digits 
			// '8100' Coupon Extended Code: Number System and Offer 6 Digits 
			nMinLength = 10;
			nMaxLength = 10;
		}
		else if (barcode.GetLength() >= 4 && barcode.Left(4) == _T("8101"))
		{
			// '8101' Coupon Extended Code: Number System, Offer, End of Offer 10 Digits
			nMinLength = 14;
			nMaxLength = 14;
		}
		else if (barcode.GetLength() >= 4 && barcode.Left(4) == _T("8102"))
		{
			// '8102' Coupon Extended Code: Number System preceeded by 0 2 Digits
			nMinLength = 6;
			nMaxLength = 6;
		}
		else if (AI_2[0]  == '9' 
			&& AI_2[1] - '0' >=0 
			&& AI_2[1] - '0' <= 9)
		{
			// '90' Mutually Agreed Between Trading Partners 1-30 Alphanumeric
			//  from '91' to '94'  and from '97' to '99' Internal Company Codes 1-30 Alphanumeric 
			//  '95' and '96' Internal Company Carrieres 1-30 Alphanumeric  
			nMinLength = 3;
			nMaxLength = 32;
		}
	}
	else if (nBarCodeType == BC_ZIP)
	{
		// 5 digits POSTNET barcode: 5 digit long zip code
		// ZIP + 4 POSTNET barcodes: 9 digit long zip code
		// DPBC POSTNET barcode (Delivery Point barcode): 9 digit long zip code + 2 DPBC digits
		if (barcode.GetLength() <= 4)
			nMinLength = 4;
		else if (barcode.GetLength() <= 5)
			nMinLength = 5;
		else if ( barcode.GetLength() <= 8)
			nMinLength = 8;
		else 
			nMinLength = 10;

		nMaxLength = nMinLength;
	}

	if (nMinLength < 0 && nMaxLength < 0)
	{
		// non ci sono vincoli di dimensione
		return TRUE;
	}
		
	// se � troppo lungo lo taglio
	if (barcode.GetLength() > nMaxLength)
		barcode = barcode.Left(nMaxLength);

	//se � troppo corto lo allungo
	while (barcode.GetLength() < nMinLength)
		barcode.AppendChar('0');

	if (nBarCodeType == BC_UPCE0)
	{	
		barcode = _T("0") + barcode;
		return TRUE;
	}

	if (nBarCodeType == BC_UPCE1)
	{
		barcode = _T("1") + barcode;
		return TRUE;
	}

	if (nBarCodeType == BC_UPCE && barcode[0] != '0' && barcode[0] != '1')
	{
		// UPCE DEVE iniziare con 0 o con 1... altrimenti aggiungo uno zero per correggere
		barcode = _T("0") + barcode.Left(6);
		return TRUE;
	}

	return TRUE;
}

//------------------------------------------------------------------------------
 BOOL CBarCodeTypes::AddCheckDigit(CString& barcode, int nBarCodeType)
{	
	 switch (nBarCodeType)
	 {
		case BC_EAN13:
		case BC_EAN8: 
		case BC_UPCA:
		 {
			 barcode.AppendChar(Get_UPC_EAN_CheckDigit(barcode));
			 break; 
		 };

		case BC_UPCE: 
		case BC_UPCE0:
		case BC_UPCE1:
		{
			//converto in UPCA originario
			CString bcUPCA = UPCE_to_UPCA(barcode);
			char checkDigit = Get_UPC_EAN_CheckDigit(bcUPCA);
			if(checkDigit != 0)
				barcode.AppendChar(checkDigit);
			break;
		}
		case BC_ZIP:
		{
			char checkDigit = Get_Mod10_CheckDigit(barcode);
			if (checkDigit != 0)
				barcode.AppendChar(checkDigit);
			break;
		}
		/*
		// facoltativi, attivare con gestione parametro 'encoding mode'
		case BC_CODABAR:
		{
			int pos = barcode.GetLength() - 1 >= 0 ? barcode.GetLength() - 1 : 0;
			char checkDigit = Get_CODABAR_CheckDigit(barcode);
			if(checkDigit != 0)
				barcode.Insert(pos, checkDigit);
			break;
		}
		case BC_CODE39:
		{
			char checkDigit = Get_CODE39_CheckDigit(barcode);
			if (checkDigit != 0)
				barcode.AppendChar(checkDigit);
			break;
		}*/
	 }
	 return TRUE;
}

 //------------------------------------------------------------------------------
 char CBarCodeTypes::Get_UPC_EAN_CheckDigit(CString barcode)
 {
	 //modulo 10
	 int currNum = 0;
	 int totSum = 0;
	 BOOL odd = true;
	 for (int i = barcode.GetLength() - 1; i >= 0; i--)
	 {
		 TCHAR currChar = barcode.GetAt(i);
		 currNum = (int)(barcode.GetAt(i) - '0');
		 totSum += currNum * (odd ? 3 : 1);
		 odd = !odd;
	 }

	 int nCheckDigit = (totSum / 10 + 1) * 10 - totSum; 
	 return '0' + (nCheckDigit < 10 ? nCheckDigit : 0);
 }

 //------------------------------------------------------------------------------
 char CBarCodeTypes::Get_CODE39_CheckDigit(CString barcode)
 {
	 //modulo 43 
	 int totSum = 0;
	 CString characterSet = _T("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%");
	 for (int i = 0; i < barcode.GetLength(); i++)
	 {
		 int currNum = -1;
		 TCHAR currChar = barcode.GetAt(i);
		 for (int j = 0; j < characterSet.GetLength(); j++)
		 {
			 if (currChar == characterSet.GetAt(j))
			 {
				 currNum = j;
				 break;
			 }
		 }
		 if (currNum < 0) return (char)0;

		 totSum += currNum;
	 }

	 int nCheckDigit = totSum % 43;
	 char cCheckdigit = (char)characterSet.GetAt(nCheckDigit);
	 return cCheckdigit;
 }

 //------------------------------------------------------------------------------
 char CBarCodeTypes::Get_Mod10_CheckDigit(CString barcode)
 {
	 //modulo 10
	 int currNum = 0;
	 int totSum = 0;
	 for (int i = barcode.GetLength() - 1; i >= 0; i--)
	 {
		 TCHAR currChar = barcode.GetAt(i);
		 currNum = (int)(barcode.GetAt(i) - '0');
		 totSum += currNum;
	 }

	 int nCheckDigit = 10 - totSum % 10;
	 return '0' + nCheckDigit;
 }

 //------------------------------------------------------------------------------
 char CBarCodeTypes::Get_CODABAR_CheckDigit(CString barcode)
 {
	 //modulo 16	 
	 int totSum = 0;
	 CString characterSet = _T("0123456789-$:/.+ABCD");
	 CString characterSet2 = _T("abcd");
	 for (int i = 0;  i < barcode.GetLength(); i++)
	 {
		 int currNum = -1;
		 TCHAR currChar = barcode.GetAt(i);
		 for (int j = 0; j < characterSet.GetLength(); j++)
		 {		 
			 if (currChar == characterSet.GetAt(j))
			 { 
				 currNum = j;
				 break;
			 }			 
		 }
		 if(currNum < 0)
		 { 
			 for (int j = 0; j < characterSet2.GetLength(); j++)
			 {
				 if (currChar == characterSet2.GetAt(j))
				 {
					 currNum = j + 16;
					 break;
				 }
			 }
		 }
		 
		 if (currNum < 0) return (char)0;
		 totSum += currNum;
	 }

	 int nCheckDigit = 16 - (totSum % 16);
	 char cCheckdigit = (char)characterSet.GetAt(nCheckDigit);
	 return cCheckdigit;
 }

 //------------------------------------------------------------------------------
 CString CBarCodeTypes::UPCE_to_UPCA(CString strUPCE)
 {
	 CString strUPCABacode = _T("");
	 
	 if (strUPCE.GetLength() != 7)
		 return strUPCABacode;

	 //X: 0 or 1; Y: last digit
	 if (strUPCE[6] == '0' || strUPCE[6] == '1' || strUPCE[6] == '2')
	 {
		 //XabcdeY	==> XabY0000cde  (0<=Y<=2): Manufacturer code must have 3 leading digits ending with "0"/"1"/"2" with 2 trailing zeros and the item number is limited to 3 digits (000 to 999).
		 strUPCABacode = strUPCE.Left(3) + strUPCE[6] +_T("0000") + strUPCE.Left(6).Right(3);
	 } 
	 else if (strUPCE[6] == '3')
	 {
		 //Xabcde3	==> Xabc00000de : Manufacturer code must have 3 leading digits and 2 trailing zeros. The item number is limited to 2 digits (00 to 99).
		 strUPCABacode = strUPCE.Left(4) + _T("00000") + strUPCE.Left(6).Right(2);

	 }
	 else if (strUPCE[6] == '4')
	 {
		 //Xabcde4	==> Xabcd00000e : Manufacturer code must have 4 leading digits with 1 trailing zero and the item number is limited to 1 digit (0 to 9).
		 strUPCABacode = strUPCE.Left(5) + _T("00000") + strUPCE.Left(6).Right(1);

	 }
	 else if (strUPCE[6] >= '5' && strUPCE[6] <= '9')
	 {
		 //XabcdeY	==> Xabcde0000Y (Y>=5): Manufacturer code must have 4 leading digits with 1 trailing zero and the item number is limited to 1 digit (0 to 9).
		 strUPCABacode = strUPCE.Left(6) + _T("0000") + strUPCE.Right(1);
	 }
		 
	 return strUPCABacode;
 }

//=============================================================================

//==============================================================================
//			Class CBarCode implementation
//==============================================================================
IMPLEMENT_DYNAMIC(CBarCode, CObject)

//------------------------------------------------------------------------------
CBarCode::CBarCode()
	:
	m_nBarCodeType			(CBarCodeTypes::BC_DEFAULT),
	m_nBCDefaultType		(BC_EAN13),
	m_nNarrowBar			(-1),
	m_bVertical				(FALSE),
	m_bShowLabel			(TRUE),
	m_nCheckSumType			(-2),
	m_nCustomBarHeight		(-1),
	m_nHumanTextAlias		(0),
	m_nBarCodeTypeAlias		(0),
	m_nNarrowBarAlias		(0),//TODO NarrowBar thickness is contained into the associated alias
	m_bCheckSize			(TRUE),
	m_sCheckEncodeFieldName(_T("")),
	m_n2DVersion(-1),
	m_s2DVersionFieldName(_T("")),
	m_nErrCorrLevel (-1),
	m_sErrCorrLevelFieldName(_T("")),
	m_nRowsNo(-1),
	m_nColumnsNo(-1)
{
	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szReportSection, szBarCodeType, DataStr(), szTbDefaultSettingFileName);
	CString sDefaultBarcode = pSetting ? pSetting->Str() : _T("");	
	CString strTemp;
	
	CTBNamespace ns(_NS_MOD("Module.Framework.TbWoormViewer"));
	pSetting = AfxGetSettingValue(ns, szWoormGeneralOptions, szCheckBarcodeSize, DataBool(TRUE));
	m_bCheckSize = pSetting ? *((DataBool*) pSetting) : TRUE;

	if (sDefaultBarcode.IsEmpty()) 
		return;
	
	for (int nBCTypeIdx = 0 ; nBCTypeIdx < CBarCodeTypes::BARCODE_TYPES_NUM; nBCTypeIdx++)
	{
		strTemp = CBarCodeTypes::BarCodeDescription(CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
		if (!sDefaultBarcode.CompareNoCase(strTemp))
		{
			m_nBCDefaultType = CBarCodeTypes::s_bcTypes[nBCTypeIdx];
			break;
		}
	}	
}
//------------------------------------------------------------------------------
const BOOL CBarCode::Is2DBarcode()
{
	return CBarCodeTypes::Is2DBarcodeType(m_nBarCodeType) || m_nBarCodeType == 0
		&& CBarCodeTypes::Is2DBarcodeType(m_nBCDefaultType);
}

//------------------------------------------------------------------------------
const BOOL CBarCode::IsCheckEncodigEnabled()
{
	return (Is2DBarcode()
		|| m_nBarCodeType == CBarCodeTypes::BC_EAN128
		|| m_nBarCodeType == 0 && m_nBCDefaultType == CBarCodeTypes::BC_EAN128);
}


//------------------------------------------------------------------------------
CBarCode::CBarCode(const CBarCode& source)
	:
	m_nBarCodeType			(source.m_nBarCodeType),
	m_nBCDefaultType		(source.m_nBCDefaultType),
	m_nNarrowBar			(source.m_nNarrowBar),
	m_bVertical				(source.m_bVertical),
	m_bShowLabel			(source.m_bShowLabel),
	m_nCheckSumType			(source.m_nCheckSumType),
	m_nCustomBarHeight		(source.m_nCustomBarHeight),
	m_nHumanTextAlias		(source.m_nHumanTextAlias),
	m_nBarCodeTypeAlias		(source.m_nBarCodeTypeAlias),
	m_nNarrowBarAlias		(source.m_nNarrowBarAlias),//TODO NarrowBar thickness is contained into the associated alias
	m_bCheckSize			(source.m_bCheckSize),
	m_sCheckEncodeFieldName(source.m_sCheckEncodeFieldName),
	m_n2DVersion			(source.m_n2DVersion),
	m_s2DVersionFieldName	(source.m_s2DVersionFieldName),
	m_nErrCorrLevel			(source.m_nErrCorrLevel),
	m_sErrCorrLevelFieldName (source.m_sErrCorrLevelFieldName),
	m_nRowsNo				(source.m_nRowsNo),
	m_nColumnsNo			(source.m_nColumnsNo)
{
}

//------------------------------------------------------------------------------
BOOL CBarCode::DrawBarCode
(
	CDC& 		DC,
	CRect 		inside,
	const LOGFONT&	lfFont,
	const	CString& 	strOriginalValue,
	COLORREF 	clrText,
	COLORREF 	clrBkgnd,
	CString&	sErr,
	CBaseDocument*	pDoc,
	CString 	strHumanReadeable,
	BOOL		bPreview,
	int			align /*=DT_CENTER*/,
	int			vAlign /*=DT_CENTER*/
)
{
	int nBarCodeType;
	int nEncodingMode = m_nCheckSumType;
	int nVersion = m_n2DVersion;
	int nErrCorrLevel = m_nErrCorrLevel;
	int nNarrowBar = m_nNarrowBar;
	int nHeight = -1;
	int nRows = m_nRowsNo;
	int nColumns = m_nColumnsNo;

	CString	strValue = strOriginalValue;

	PrepareBCParameters(DC, strValue, nBarCodeType, strHumanReadeable, nEncodingMode, nVersion,
		nErrCorrLevel, nNarrowBar, nHeight, nRows, nColumns, bPreview);

	// da setting
	BOOL bAutocompleteBC = true;
	if(bAutocompleteBC)
	{ 
		// completamento stringa se richiesto da tipo di barcode
		if (!CBarCodeTypes::CheckAndCompleteBCString(strValue, nBarCodeType, sErr))
			return false;
	}
	// if(nEncodingMode == ?) attivare con check del parametro
	CBarCodeTypes::AddCheckDigit(strValue, nBarCodeType);

	USES_CONVERSION;

	LOGFONT	logFont = lfFont;

	if (DC.IsPrinting() || DC.GetDeviceCaps(LOGPIXELSY) != SCALING_FACTOR)
	{
		::ScaleLogFont(&logFont, DC);
	}

	BarCodeCreator*	barCodeCreator = new BarCodeCreator();
	CString str_barcodeType = CBarCodeTypes::BarCodeDescription(nBarCodeType);

	BarCodeCreator::TBPictureStatus status = BarCodeCreator::TBPictureStatus::OK;

	try {

		status = barCodeCreator->WriteBarcodeToHDC
		(DC, strValue, strHumanReadeable,
			str_barcodeType, nNarrowBar, nHeight, inside,
			clrText, clrBkgnd, m_bShowLabel, bPreview, logFont, m_bVertical, align, vAlign,
			nEncodingMode, nVersion, nErrCorrLevel, nRows, nColumns);
	}
	catch (CException* ex)
	{
		TRACE(L"WriteBarcodeToHDC fails\n");
		ex->Delete();
	}

	if (status != BarCodeCreator::TBPictureStatus::OK)
		sErr = barCodeCreator->GetErrorMessage(status) + cwsprintf(_T(" (error: %d)"), status);

	delete barCodeCreator;
	return status == BarCodeCreator::TBPictureStatus::OK;
}

//-----------------------------------------------------------------------------
BOOL CBarCode::PrepareBCParameters(	CDC& DC, 
									CString &strValue,
									int &nBarCodeType,
									CString& strHumanReadeable,
										int &nEncodingMode,
										int &nVersion,
										int &nErrCorrLevel,
										int &nNarrowBar,
										int &nHeight,
										int &nRows,
										int &nColumns,
										BOOL bPreview)
{
	nBarCodeType = m_nBarCodeType;
	// --- lettura TypedBarcode
	// se mi arriva un bar code composto (esiste il tag 0x01) allora gli ultimi byte
	// sono il numero in ascii del tipo di barcode si vuole stampare
	TCHAR* pBuff = strValue.GetBuffer(strValue.GetLength());
	TCHAR* pszType = _tcschr(pBuff, CBarCodeTypes::BC_SEP);
	if (pszType)
	{
		if (m_nBarCodeTypeAlias <= 0)
		{
			// parametri da Alias --> vincono sulla typedbarcode
			nBarCodeType = _tstol(&pszType[1]);
		}
			
		TCHAR* pChkSum = _tcschr(&pszType[2], CBarCodeTypes::BC_SEP);

		if (pChkSum)
		{
			// parametri da Alias --> vincono sulla typedbarcode
			if(m_sCheckEncodeFieldName.IsEmpty())
				nEncodingMode = _tstol(&pChkSum[1]);
			TCHAR* pHR = _tcschr(&pChkSum[2], CBarCodeTypes::BC_SEP);
			if (pHR)
			{
				CString strHR = CString(&pHR[1]);
				// parametri da Alias --> vincono sulla typedbarcode
				if(m_nHumanTextAlias <= 0)
					strHumanReadeable = strHR.Left(strHR.Find(CBarCodeTypes::BC_SEP));

				TCHAR* pNarrowBar = _tcschr(&pHR[1], CBarCodeTypes::BC_SEP);
				if (pNarrowBar)
				{
					nNarrowBar = _tstol(&pNarrowBar[1]);
					TCHAR* pBarHeight = _tcschr(&pNarrowBar[2], CBarCodeTypes::BC_SEP);

					if (pBarHeight)
					{
						nHeight = _tstol(&pBarHeight[1]);

						TCHAR* pVersion = _tcschr(&pBarHeight[2], CBarCodeTypes::BC_SEP);

						if (pVersion && m_s2DVersionFieldName.IsEmpty())
						{
							//version da 'interpretare'
							if (nBarCodeType == BC_DATAMATRIX || nBarCodeType == BC_PDF417)
							{
								CString sVersion = CString(&pVersion[1]);
								int index = sVersion.Find(CBarCodeTypes::BC_SEP);
								if (index > 0)
									sVersion = sVersion.Left(index);
								if (nBarCodeType == BC_DATAMATRIX)
									nVersion = CBarCodeTypes::BarCodeDMVersion(sVersion);
								else
									CBarCodeTypes::ReadPDF417Version(sVersion, nRows, nColumns);

							}
							else
								nVersion = _tstol(&pVersion[1]);

							TCHAR* pErrCorrLevel = _tcschr(&pVersion[2], CBarCodeTypes::BC_SEP);
							if (pErrCorrLevel && m_sErrCorrLevelFieldName.IsEmpty())
							{
								int nextIndex = pErrCorrLevel[1] == CBarCodeTypes::BC_SEP ? 0 : 1;
								nErrCorrLevel = _tstol(&pErrCorrLevel[1]);
							}
								
							if (nErrCorrLevel == -2 && nBarCodeType != BC_PDF417)
								nErrCorrLevel = -1;
						}
					}
				}
			}
		}

		//pulisco la stringa dalla parte relativa ai parametri
		strValue = strValue.Left(strValue.Find(CBarCodeTypes::BC_SEP));

		// si "tappa" la stringa e la successiva ReleaseBuffer rimettera` a posto le cose
		*pszType = NULL_CHAR;
	}
	strValue.ReleaseBuffer();

	// a questo punto se � richiesto il type di default lo applico
	if (nBarCodeType == CBarCodeTypes::BC_DEFAULT)
		nBarCodeType = m_nBCDefaultType;

	CString sBarcodeType = CBarCodeTypes::BarCodeDescription(nBarCodeType);
	BarCodeCreator*	bCreator = new BarCodeCreator();
	BOOL bBarcode2D = bCreator->If2DBarCode(sBarcodeType);
	SAFE_DELETE(bCreator);


	if (!bBarcode2D && nNarrowBar < 0)
	{ 
		nNarrowBar = 1; //auto bar size � 1 per barcode 1D
		if (nBarCodeType == BC_ZIP)
			nNarrowBar = 3;
	}

	if (nNarrowBar > -1 && (DC.IsPrinting() || DC.GetDeviceCaps(LOGPIXELSY) != SCALING_FACTOR))
	{
		CPoint nb(m_bVertical ? 0 : nNarrowBar, m_bVertical ? nNarrowBar : 0);
		ScalePoint(nb, DC);
		nNarrowBar = m_bVertical ? nb.y : nb.x;
	}

	//
	if (!bBarcode2D || nBarCodeType == BC_PDF417)
	{
		if (nBarCodeType == BC_PDF417)
		{
			//in questo caso � l'altezza della riga
			nHeight = m_nCustomBarHeight;
		}
		else if (m_nCustomBarHeight > 0)
		{
			nHeight = (int)MUtoLP(m_nCustomBarHeight, CM, 10., 3);
		}

		if (DC.IsPrinting() && m_nCustomBarHeight > 0)
		{
			if (m_bVertical)
			{
				CPoint nb(0, nHeight);
				ScalePoint(nb, DC);
				nHeight = nb.y;
			}
			else
			{
				CPoint nb(nHeight, 0);
				ScalePoint(nb, DC);
				nHeight = nb.x;
			}
		}
	}
	
	// Recupero i parametri dal setting se il
	// valore impostato al parametro equivale a 'prendi dal setting'

	// - encoding mode
	if (nEncodingMode < -1 && bBarcode2D || nEncodingMode < 0)
	{
		DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DEncoding, szBarcodeTypes[nBarCodeType - 1], DataInt(-1), szBarcode2DFileName);
		nEncodingMode = pSetting ? *((DataInt*)pSetting) : -1;
	}

	// - BC version
	if (bBarcode2D && nBarCodeType != BC_PDF417 && nVersion < 0)
	{
		if (nBarCodeType == BC_DATAMATRIX)
		{
			DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DVersion, szBarcodeTypes[nBarCodeType - 1], DataStr(_T("")), szBarcode2DFileName);
			CString str_Version = pSetting ? pSetting->Str() : _T("");
			nVersion = 0;
			CString strTemp;
			if (!str_Version.IsEmpty())
				nVersion = CBarCodeTypes::BarCodeDMVersion(str_Version);
		}
		else
		{
			DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DVersion, szBarcodeTypes[nBarCodeType - 1], DataInt(-1), szBarcode2DFileName);
			nVersion = pSetting ? *((DataInt*)pSetting) : 0;
		}
	}
	else if (nBarCodeType == BC_PDF417 && (nRows < 0 || nColumns < 0))
	{
		DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DVersion, szBarcodeTypes[nBarCodeType - 1], DataStr(_T("")), szBarcode2DFileName);
		CString str_Version = pSetting ? pSetting->Str() : _T("");
		CBarCodeTypes::ReadPDF417Version(str_Version, nRows, nColumns);
		// ripristino il valore precedente se uno dei due  non era da prendere dal setting
		if (m_nRowsNo > 0) nRows = m_nRowsNo;
		if (m_nColumnsNo > 0) nColumns = m_nColumnsNo;
	}

	// - BC Error Correction Level
	if (bBarcode2D && nBarCodeType != BC_DATAMATRIX && (nBarCodeType == BC_PDF417 ? nErrCorrLevel < -1 : nErrCorrLevel < 0))
	{
		//recupero il valore dal setting
		if (nBarCodeType != BC_PDF417)
		{
			//sono stringhe
			DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DErrCorrLevel, szBarcodeTypes[nBarCodeType - 1], DataStr(_T("")), szBarcode2DFileName);
			CString str_ErrCorrLevel = pSetting ? pSetting->Str() : _T("");
			nErrCorrLevel = 0;
			if (str_ErrCorrLevel == _T("M"))
				nErrCorrLevel = 1;
			else if (str_ErrCorrLevel == _T("Q"))
				nErrCorrLevel = 2;
			else if (nBarCodeType == BC_QR && str_ErrCorrLevel == _T("H"))
				nErrCorrLevel = 3;
		}
		else
		{
			//sono numeri
			DataObj* pSetting = AfxGetSettingValue(snsTbWoormViewer, szDefaultBarcode2DErrCorrLevel, szBarcodeTypes[nBarCodeType - 1], DataInt(-1), szBarcode2DFileName);
			nErrCorrLevel = pSetting ? *((DataInt*)pSetting) : 0;
		}
	}
	return TRUE;
}
//-----------------------------------------------------------------------------
BOOL CBarCode::Parse (Parser& lex)
{
	BOOL ok = lex.SkipToken();	// salto il token BarCode...
	Token	token = lex.LookAhead();// leggo la parentesi aperta
	if (token == T_ROUNDOPEN)
	{
		UINT	nParamNum = 1;  

		do
		{   
			lex.SkipToken();	// salto la token appena letto...
			CString strTemp;
			switch (nParamNum)
			{ 
				case 1:
					m_nBarCodeType = CBarCodeTypes::BC_DEFAULT;
					if (lex.LookAhead(T_STR))
					{
						CString strBarCodeType;
						ok = lex.ParseString(strBarCodeType); strBarCodeType.Trim();
						if (!strBarCodeType.IsEmpty())
						{
							m_nBarCodeType = CBarCodeTypes::BarCodeType(strBarCodeType);
						}
					}	
					else
					{
						ok = lex.ParseInt (m_nBarCodeTypeAlias);
					}
					break;
				case 2:
					{
						ok = lex.ParseSignedInt	(m_nNarrowBar);
					}
					break;
				case 3:
					ok = lex.ParseBool	(m_bVertical);
					break;
				case 4:
					ok = lex.ParseBool	(m_bShowLabel);
					if (lex.LookAhead(T_ROUNDCLOSE))
						return lex.ParseClose();
					break;
				case 5:
					if (lex.LookAhead(T_ID))
					{
						CString strBCEncodingMode;
						ok = lex.ParseID(strBCEncodingMode);
						strBCEncodingMode.Trim();
						m_sCheckEncodeFieldName = strBCEncodingMode;
					}
					else				
						ok = lex.ParseSignedInt	(m_nCheckSumType);
					if (lex.LookAhead(T_ROUNDCLOSE))
						return lex.ParseClose();
					break;
				case 6:
					ok = lex.ParseSignedInt (m_nCustomBarHeight);
					if (lex.LookAhead(T_ROUNDCLOSE))
						return lex.ParseClose();
					break;				
				case 7:
					ok = lex.ParseInt (m_nHumanTextAlias);
					if (lex.LookAhead(T_ROUNDCLOSE))
						return lex.ParseClose();
					break;
				case 8:
					if (lex.LookAhead(T_ID))
					{
						CString strBCVersion;
						ok = lex.ParseID(strBCVersion);
						strBCVersion.Trim();
						m_s2DVersionFieldName = strBCVersion;
					}
					else if (lex.LookAhead(T_STR))
					{
						CString strBCPDFVersion;
						ok = lex.ParseString(strBCPDFVersion); 
						strBCPDFVersion.Trim();
						CBarCodeTypes::ReadPDF417Version(strBCPDFVersion, m_nRowsNo, m_nColumnsNo);
					}
					else				
						ok = lex.ParseSignedInt(m_n2DVersion);
					if (lex.LookAhead(T_ROUNDCLOSE))
						return lex.ParseClose();
					break;
				case 9:
					if (lex.LookAhead(T_ID))
					{
						CString strBCErrCorrLevel;
						ok = lex.ParseID(strBCErrCorrLevel);
						strBCErrCorrLevel.Trim();
						m_sErrCorrLevelFieldName = strBCErrCorrLevel;
					}
					else
						ok = lex.ParseSignedInt(m_nErrCorrLevel);

					return lex.ParseClose();
				default:
					return FALSE;
			}
			nParamNum++;
			token = lex.LookAhead();
		}while (token == T_COMMA || !lex.ParseClose());
		if (lex.LookAhead() == T_SEP) lex.SkipToken();
	}       
	return ok;
}

//------------------------------------------------------------------------------
void CBarCode::Unparse (Unparser& ofile, BOOL bNewline)
{
	ofile.UnparseTag  (T_BARCODE_STRIP,     FALSE);
	ofile.UnparseOpen ();
	
	CString strTemp;
	if (m_nBarCodeTypeAlias <= 0)
	{
		for (int nBCTypeIdx = 0 ; nBCTypeIdx < CBarCodeTypes::BARCODE_TYPES_NUM; nBCTypeIdx++)
		{
			if (m_nBarCodeType == CBarCodeTypes::s_bcTypes[nBCTypeIdx])
			{
				strTemp = CBarCodeTypes::BarCodeDescription(CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
				break;
			}
		}	
		ofile.UnparseString (strTemp,	FALSE);
	}
	else
		ofile.UnparseInt (m_nBarCodeTypeAlias,	FALSE);

	ofile.UnparseComma();
	ofile.UnparseInt  (m_nNarrowBar,FALSE);
	ofile.UnparseComma();
	ofile.UnparseBool (m_bVertical, FALSE);
	ofile.UnparseComma();
	ofile.UnparseBool (m_bShowLabel,FALSE);

	// default barcode parameter
	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szReportSection, szBarCodeType, DataStr(), szTbDefaultSettingFileName);
	CString sDefaultBarcode = pSetting ? pSetting->Str() : _T("");	
	int nBCDefaultType = 0;
	if (!sDefaultBarcode.IsEmpty()) 
	{
		for (int nBCTypeIdx = 0 ; nBCTypeIdx < CBarCodeTypes::BARCODE_TYPES_NUM; nBCTypeIdx++)
		{
			strTemp = CBarCodeTypes::BarCodeDescription(CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
			if (!sDefaultBarcode.CompareNoCase(strTemp))
			{
				nBCDefaultType = CBarCodeTypes::s_bcTypes[nBCTypeIdx];
				break;
			}
		}	
	}
	BOOL b2DBarcode = Is2DBarcode();

	int bcType = m_nBarCodeType == CBarCodeTypes::BC_DEFAULT ? m_nBCDefaultType : m_nBarCodeType;
	BOOL bEAN128 = bcType == CBarCodeTypes::BC_EAN128;


	// EAN128 barcode and barcode 2D have other addon parameters
	if (b2DBarcode || bEAN128 || m_nBarCodeTypeAlias || m_nCustomBarHeight > 0 || m_nHumanTextAlias > 0)
	{
		ofile.UnparseComma();
		if (m_sCheckEncodeFieldName.IsEmpty())
		{
			ofile.UnparseInt(m_nCheckSumType, FALSE);
		}
		else
		{
			ofile.UnparseID(m_sCheckEncodeFieldName, FALSE);
		}

		if (m_nCustomBarHeight > 0 || m_nHumanTextAlias > 0 || b2DBarcode || m_nBarCodeTypeAlias)
		{
			ofile.UnparseComma();
			ofile.UnparseInt(m_nCustomBarHeight, FALSE);

			if (m_nHumanTextAlias || b2DBarcode || m_nBarCodeTypeAlias)
			{
				ofile.UnparseComma();
				ofile.UnparseInt(m_nHumanTextAlias, FALSE);

				if (b2DBarcode || m_nBarCodeTypeAlias)
				{
					ofile.UnparseComma();
					if (m_s2DVersionFieldName.IsEmpty())
					{
						if (bcType == BC_PDF417)
						{
							CString rowsXColumns = DataInt(m_nRowsNo).ToString() + _T("x") + DataInt(m_nColumnsNo).ToString();
							ofile.UnparseString(rowsXColumns, FALSE);
						}
						else
						ofile.UnparseInt(m_n2DVersion, FALSE);
					}
					else
						ofile.UnparseID(m_s2DVersionFieldName, FALSE);

					ofile.UnparseComma();
					if (m_sErrCorrLevelFieldName.IsEmpty())
					{
						ofile.UnparseInt(m_nErrCorrLevel, FALSE);
					}
					else
						ofile.UnparseID(m_sErrCorrLevelFieldName, FALSE);
				}
			}
		}
	}
	ofile.UnparseClose(bNewline);
}

/////////////////////////////////////////////////////////////////////////////
// CBarCodeAttrsDlg dialog
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBarCodeAttrsDlg, CParsedDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBarCodeAttrsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CBarCodeAttrsDlg)
	ON_BN_CLICKED       (IDC_BC_CHK_CUSTOM_TYPE,			OnCustomTypeChanged)
	ON_CBN_SELCHANGE	(IDC_BC_TYPE_COMBO,	 				OnTypeBarCodeChanged)

	ON_BN_CLICKED       (IDC_BC_CUSTOM_BARS_HEIGHT,			OnCustomBarsHeightChanged)
	ON_BN_CLICKED       (IDC_BC_SHOWLAB_CHECKBOX,			OnShowTextChanged)
	ON_BN_CLICKED       (IDC_BC_CUSTOM_TEXT_ALIAS_NUMBER,	OnCustomAliasNumberChanged)
	
	ON_EN_KILLFOCUS		(IDC_BC_VALUE_EDIT,		 			OnBCValueChanged)
	ON_EN_KILLFOCUS		(IDC_BC_BARWIDTH_EDIT,		 		OnBCValueChanged)

	ON_WM_PAINT			() 
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//----------------------------------------------------------------------------
CBarCodeAttrsDlg::CBarCodeAttrsDlg
			(   
				CBarCode* 	pBarCode,
				SymTable*	pSymTable,
				CWnd* 		pParent
			)
	:
	CParsedDialog		(IDD_BARCODE_ATTRS, pParent),
	m_pBarCode			(pBarCode),
	m_NarrowBarEdit		(BTN_SPIN_ID),
	m_pSymTable			(pSymTable),
	m_nDefaultBarCode	(0)
{
}

//----------------------------------------------------------------------------
BOOL CBarCodeAttrsDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();
	
	m_NarrowBarEdit.	SubclassEdit(IDC_BC_BARWIDTH_EDIT, 			this);
	m_NarrowBarEdit.	SetRange(1, 50);
	m_NarrowBarEdit.	SetValue(m_pBarCode->m_nNarrowBar);
	
	m_VertRadioBtn.		SubclassDlgItem(IDC_BC_VERT_RADIOBTN, 		this);
	m_HorzRadioBtn.		SubclassDlgItem(IDC_BC_HORZ_RADIOBTN, 		this);
	m_ShowLabCheckBtn.	SubclassDlgItem(IDC_BC_SHOWLAB_CHECKBOX, 	this);

	m_TypeCombo.		SubclassDlgItem(IDC_BC_TYPE_COMBO, 			this);
	m_CheckSumType.		SubclassDlgItem(IDC_BC_CHECKSUM_COMBO, 		this);
	m_Height.			SubclassDlgItem(IDC_BC_HEIGHT_EDIT, 		this);
	m_CustomBarHeight.	SubclassDlgItem(IDC_BC_CUSTOM_BARS_HEIGHT, 	this);

	m_Height.SetValue(m_pBarCode->m_nCustomBarHeight);
	m_CustomBarHeight.SetCheck(m_pBarCode->m_nCustomBarHeight > 0);

	m_HT_AliasNumber.		SubclassDlgItem(IDC_BC_TEXT_ALIAS_NUMBER,		this);
	m_HT_CustomAliasNumber.SubclassDlgItem(IDC_BC_CUSTOM_TEXT_ALIAS_NUMBER, this);
	
	m_cbxCustomType.	SubclassDlgItem(IDC_BC_CUSTOM_TYPE,		this);
	m_chkCustomType.	SubclassDlgItem(IDC_BC_CHK_CUSTOM_TYPE, this);

	m_ValueEdit.SubclassEdit (IDC_BC_VALUE_EDIT,  this);
	m_ValueEdit.SetValue (m_strBCValue);

	m_TestStatic.SubclassDlgItem	(IDC_BC_TEST_STATIC, this);

	LoadDefaultBarCode	();
	LoadAliasTable		();

	if (m_pBarCode->m_nHumanTextAlias > 0)
	{
		for (int i=0; i < m_HT_AliasNumber.GetCount(); i++)
		{
			DWORD nAlias = m_HT_AliasNumber.GetItemData(i);
			if (nAlias == m_pBarCode->m_nHumanTextAlias)
			{
				m_HT_AliasNumber.SetCurSel(i);
				break;
			}
		}
	}
	m_HT_CustomAliasNumber.SetCheck(m_pBarCode->m_nHumanTextAlias > 0);

	if (m_pBarCode->m_nBarCodeTypeAlias > 0)
	{
		for (int i=0; i < m_cbxCustomType.GetCount(); i++)
		{
			DWORD nAlias = m_cbxCustomType.GetItemData(i);
			if (nAlias == m_pBarCode->m_nBarCodeTypeAlias)
			{
				m_cbxCustomType.SetCurSel(i);
				break;
			}
		}
	}
	m_chkCustomType.SetCheck(m_pBarCode->m_nBarCodeTypeAlias > 0);

	LoadBarcodeTypesCombo		();

	m_VertRadioBtn.SetCheck(m_pBarCode->m_bVertical); 
	m_HorzRadioBtn.SetCheck(!m_pBarCode->m_bVertical); 

	m_ShowLabCheckBtn.SetCheck(m_pBarCode->m_bShowLabel); 

	OnCustomTypeChanged();

	m_CheckSumType.SetCurSel(m_pBarCode->m_nCheckSumType + 1);
	return TRUE;
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::SetCustomBarHeightControls ()
{
	m_Height.EnableWindow(m_CustomBarHeight.GetCheck() > 0);
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::LoadDefaultBarCode ()
{
	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szReportSection, szBarCodeType, DataStr(), szTbDefaultSettingFileName);
	CString sDefaultBarcode = pSetting ? pSetting->Str() : _T("");	
	CString strTemp;
	
	if (sDefaultBarcode.IsEmpty()) 
		return;
	
	for (int nBCTypeIdx = 0 ; nBCTypeIdx < CBarCodeTypes::BARCODE_TYPES_NUM; nBCTypeIdx++)
	{
		strTemp = CBarCodeTypes::BarCodeDescription(CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
		if (!sDefaultBarcode.CompareNoCase(strTemp))
		{
			m_nDefaultBarCode = CBarCodeTypes::s_bcTypes[nBCTypeIdx];
			break;
		}
	}	
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::LoadBarcodeTypesCombo ()
{
	CString strTemp;
	int		nCboIdx;

	CString strCurrentBarCodeType;
		
	// Caricamento della combo dei tipi di codice a barre supportati
	for (int nBCTypeIdx = 0 ; nBCTypeIdx < CBarCodeTypes::BARCODE_TYPES_NUM; nBCTypeIdx++)
	{
		strTemp = CBarCodeTypes::BarCodeDescription(CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
		nCboIdx = m_TypeCombo.AddString(strTemp);
		if (nCboIdx != CB_ERR)
		{
			m_TypeCombo.SetItemData(nCboIdx, (DWORD)CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
			if (m_pBarCode->m_nBarCodeType == CBarCodeTypes::s_bcTypes[nBCTypeIdx])
				strCurrentBarCodeType = strTemp;
		}
	}	

	// Selezione del tipo corrente
	m_TypeCombo.SelectString(-1, strCurrentBarCodeType);
}

//----------------------------------------------------------------------------
BOOL CBarCodeAttrsDlg::UseDynamicText	()
{
	if (m_chkCustomType.GetCheck())
		return TRUE;
	
	int bBarCodeType = 0;

	// selected barcode type 
	int nCboIdx = m_TypeCombo.GetCurSel();
	if (nCboIdx != CB_ERR)
		 bBarCodeType = (int)m_TypeCombo.GetItemData(nCboIdx);

	return bBarCodeType == CBarCodeTypes::BC_EAN128 ||
		(bBarCodeType == CBarCodeTypes::BC_DEFAULT &&
		m_nDefaultBarCode == CBarCodeTypes::BC_EAN128);
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::SetCheckSumModulesCombo	()
{
	m_CheckSumType.ResetContent();

	LoadCheckSumModulesCombo	();
		
	m_CheckSumType.EnableWindow(TRUE);
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::LoadCheckSumModulesCombo	()
{
	int idx = m_CheckSumType.AddString(_T(""));
	m_CheckSumType.SetItemData(idx, -1);

	idx = m_CheckSumType.AddString(szChecksumModule10103_Default);
	m_CheckSumType.SetItemData(idx, 0);

	idx = m_CheckSumType.AddString(szChecksumModule103_Optional);
	m_CheckSumType.SetItemData(idx, 1);

	m_CheckSumType.SetCurSel(0);
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnOK()
{
	int nCboIdx = m_TypeCombo.GetCurSel();
	if (nCboIdx != CB_ERR)
		m_pBarCode->m_nBarCodeType = (int)m_TypeCombo.GetItemData(nCboIdx);

	m_pBarCode->m_nNarrowBar = m_NarrowBarEdit.GetValue();

	m_pBarCode->m_bVertical = m_VertRadioBtn.GetCheck();
	
	m_pBarCode->m_bShowLabel = m_ShowLabCheckBtn.GetCheck();

	nCboIdx = m_CheckSumType.GetCurSel();
	m_pBarCode->m_nCheckSumType = nCboIdx >= 0 ? m_CheckSumType.GetItemData(nCboIdx) : -1;
	
	m_pBarCode->m_nCustomBarHeight = m_Height.GetValue();
	if (m_CustomBarHeight.GetCheck() > 0)
		m_pBarCode->m_nCustomBarHeight= m_Height.GetValue();
	else
		m_pBarCode->m_nCustomBarHeight = -1;

	if (m_HT_CustomAliasNumber.GetCheck() > 0)
	{
		nCboIdx = m_HT_AliasNumber.GetCurSel();
		if (nCboIdx != CB_ERR)
			m_pBarCode->m_nHumanTextAlias = (int) ((DWORD) m_HT_AliasNumber.GetItemData(nCboIdx));
	}
	else
		m_pBarCode->m_nHumanTextAlias = 0;

	if (m_chkCustomType.GetCheck() > 0)
	{
		nCboIdx = m_cbxCustomType.GetCurSel();
		if (nCboIdx != CB_ERR)
			m_pBarCode->m_nBarCodeTypeAlias = (int) ((DWORD) m_cbxCustomType.GetItemData(nCboIdx));
	}
	else
		m_pBarCode->m_nBarCodeTypeAlias = 0;

	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnTypeBarCodeChanged ()
{
	SetCheckSumModulesCombo		();
	SetCustomBarHeightControls	();
	SetCustomAliasNumberControls();

	DrawStaticBarCode();
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnCustomBarsHeightChanged ()
{
	SetCustomBarHeightControls	();
	DrawStaticBarCode();
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::SetCustomAliasNumberControls ()
{
	m_HT_CustomAliasNumber.EnableWindow(m_ShowLabCheckBtn.GetCheck() > 0);
	m_HT_AliasNumber.EnableWindow(m_HT_CustomAliasNumber.GetCheck() > 0 && m_ShowLabCheckBtn.GetCheck() > 0);
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnCustomAliasNumberChanged ()
{
	SetCustomAliasNumberControls ();
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnShowTextChanged()
{
	SetCustomAliasNumberControls ();
	DrawStaticBarCode();
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnCustomTypeChanged()
{
	BOOL bCustomType = m_chkCustomType.GetCheck() == 1;

	m_TypeCombo			.EnableWindow(!bCustomType);
	m_cbxCustomType		.EnableWindow(bCustomType);
	
	m_TestStatic		.EnableWindow(!bCustomType);
	m_ValueEdit			.EnableWindow(!bCustomType);
	if (bCustomType)	m_ValueEdit.Clear();

	OnTypeBarCodeChanged ();
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::LoadAliasTable()
{
	if (!m_pSymTable)
		return;

	m_HT_AliasNumber.ResetContent();
	m_cbxCustomType.ResetContent();
	
	for (int i = 0; i <= m_pSymTable->GetUpperBound(); i++)
	{
		SymField* pField = m_pSymTable->GetAt(i);

		if (!pField)
			continue;

		if (
			pField->GetDataType() != DataType::String &&
			pField->GetDataType() != DataType::Integer &&
			pField->GetDataType().m_wTag != E_BARCODE_TYPE
			)
			continue;

		CString sName = pField->GetName();
		int nAlias = pField->GetId();

		if (pField->GetDataType() == DataType::String)
		{
			int idx = m_HT_AliasNumber.AddString (sName);
			m_HT_AliasNumber.SetItemData(idx, nAlias);
		}

		int idx = m_cbxCustomType.AddString (sName);
		m_cbxCustomType.SetItemData(idx, nAlias);
	}
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnPaint()
{
	__super::OnPaint();

	DrawStaticBarCode();
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnBCValueChanged()
{
	m_ValueEdit.GetValue(m_strBCValue);
	DrawStaticBarCode();
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::OnDrawStaticBarCode()
{
	CRect	rcStaticRect;
	m_TestStatic.GetClientRect(rcStaticRect); 
	InvalidateRect(&rcStaticRect);
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::DrawStaticBarCode()
{
	if(m_strBCValue.IsEmpty())
		return;
	if(m_cbxCustomType.IsWindowEnabled())
		return;
	
	CDC*	pStaticDC = m_TestStatic.GetDC();
	CRect	rcStaticRect;
	LOGFONT lfFont;
	AfxGetThemeManager()->GetFormFont()->GetObject(sizeof(LOGFONT),&lfFont);

	// I change font to make barcode text readable (an 11.922)
	TB_TCSCPY(lfFont.lfFaceName, _T("MS Sans Serif"));
	if (lfFont.lfHeight < -11)
		lfFont.lfHeight = -11;
	if (lfFont.lfWeight < 400)
		lfFont.lfWeight = 400;
	
	m_TestStatic.GetClientRect(rcStaticRect);
   
	int nCboIdx = m_TypeCombo.GetCurSel();
	if (nCboIdx != CB_ERR)
		m_pBarCode->m_nBarCodeType = (int)m_TypeCombo.GetItemData(nCboIdx);
	
	m_pBarCode->m_nNarrowBar = m_NarrowBarEdit.GetValue();

	m_pBarCode->m_bVertical = m_VertRadioBtn.GetCheck();
	
	m_pBarCode->m_bShowLabel = m_ShowLabCheckBtn.GetCheck();

	nCboIdx = m_CheckSumType.GetCurSel();
	m_pBarCode->m_nCheckSumType = nCboIdx >= 0 ? m_CheckSumType.GetItemData(nCboIdx) : -1;
	
	m_pBarCode->m_nCustomBarHeight = m_Height.GetValue();
	if (m_CustomBarHeight.GetCheck() > 0)
		m_pBarCode->m_nCustomBarHeight= m_Height.GetValue();
	else
		m_pBarCode->m_nCustomBarHeight = -1;

	DrawBarCode(*pStaticDC, rcStaticRect, lfFont);
	
	ReleaseDC(pStaticDC); 
}

//----------------------------------------------------------------------------
void CBarCodeAttrsDlg::DrawBarCode(CDC& dc, CRect& rcRect, LOGFONT lfFont)
{
	CString sErr;
	BOOL bRet = m_pBarCode->DrawBarCode
		(
			dc, rcRect, lfFont,	m_strBCValue,
			CLR_BLACK,
			CLR_WHITE,
			sErr
		);
}

/////////////////////////////////////////////////////////////////////////////
// CBarCodeSettingsDlg dialog
/////////////////////////////////////////////////////////////////////////////
//
//----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CBarCodeSettingsDlg, CParsedDialog)
//----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CBarCodeSettingsDlg, CParsedDialog)
	//{{AFX_MSG_MAP(CBarCodeSettingsDlg)
	ON_EN_KILLFOCUS		(IDC_BC_VALUE_EDIT,		 	OnBCValueChanged)
	ON_CONTROL			(EN_SPIN_RELEASED,			IDC_BC_BARWIDTH_EDIT, OnDrawStaticBarCode)
	ON_BN_CLICKED       (IDC_BC_HORZ_RADIOBTN,		OnDrawStaticBarCode)
	ON_BN_CLICKED       (IDC_BC_VERT_RADIOBTN,		OnDrawStaticBarCode)
	ON_BN_CLICKED       (IDC_BC_SHOWLAB_CHECKBOX,	OnDrawStaticBarCode)
	ON_WM_PAINT			()       
	ON_CBN_SELCHANGE	(IDC_BC_TYPE_COMBO,	 				OnTypeBarCodeChanged)
	ON_CBN_SELCHANGE    (IDC_BC_CHECKSUM_COMBO,				OnDrawStaticBarCode)
	ON_BN_CLICKED       (IDC_BC_CUSTOM_BARS_HEIGHT,			OnCustomBarsHeightChanged)
	ON_EN_KILLFOCUS		(IDC_BC_HEIGHT_EDIT,				OnDrawStaticBarCode)
	ON_BN_CLICKED       (IDC_BC_SHOWLAB_CHECKBOX,			OnShowTextChanged)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//----------------------------------------------------------------------------
CBarCodeSettingsDlg::CBarCodeSettingsDlg
			(   
				CString& 	strBarCodeValue,
				CBarCode& 	bcBarCode,
				BOOL		bEnableValueEdit,
				BOOL		bEnableTypeCbo,
				CWnd* 		pParent
			)
	:
	CParsedDialog		(IDD_BARCODE_SETTINGS, pParent),
	m_strBCValue		(strBarCodeValue),
	m_BarCode			(bcBarCode),
	m_NarrowBarEdit		(BTN_SPIN_ID),
	m_bEnableValueEdit	(bEnableValueEdit),
	m_bEnableTypeCbo	(bEnableTypeCbo),
	m_nDefaultBarCode	(0)
{
}

//----------------------------------------------------------------------------
BOOL CBarCodeSettingsDlg::OnInitDialog()
{
	CParsedDialog::OnInitDialog();

	m_ValueEdit.SubclassEdit (IDC_BC_VALUE_EDIT,  this);
	m_ValueEdit.SetValue (m_strBCValue);
	m_ValueEdit.EnableWindow (m_bEnableValueEdit);
   
	m_TestStatic.	SubclassDlgItem	(IDC_BC_TEST_STATIC, this);
	m_TestStatic.	SetOwnerDialog(this);

	m_NarrowBarEdit.	SubclassEdit(IDC_BC_BARWIDTH_EDIT, this);
	m_NarrowBarEdit.	SetRange(1, 50);
	m_NarrowBarEdit.	SetValue(m_BarCode.m_nNarrowBar);
	
	m_VertRadioBtn.		SubclassDlgItem(IDC_BC_VERT_RADIOBTN, 		this);
	m_HorzRadioBtn.		SubclassDlgItem(IDC_BC_HORZ_RADIOBTN, 		this);
	m_ShowLabCheckBtn.	SubclassDlgItem(IDC_BC_SHOWLAB_CHECKBOX, 	this);

	m_TypeCombo.		SubclassDlgItem(IDC_BC_TYPE_COMBO, 			this);
	
	CString strTemp;
	int		nCboIdx;
	CString strCurrentBarCodeType;
		
	// Caricamento della combo dei tipi di codice a barre supportati
	for (int nBCTypeIdx = 0 ; nBCTypeIdx < CBarCodeTypes::BARCODE_TYPES_NUM; nBCTypeIdx++)
	{
		strTemp = CBarCodeTypes::BarCodeDescription(CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
		nCboIdx = m_TypeCombo.AddString(strTemp);
		if (nCboIdx != CB_ERR)
		{
			m_TypeCombo.SetItemData(nCboIdx, (DWORD)CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
			if ( m_BarCode.m_nBarCodeType == CBarCodeTypes::s_bcTypes[nBCTypeIdx])
				strCurrentBarCodeType = strTemp;
		}
	}	

	m_CheckSumType.		SubclassDlgItem(IDC_BC_CHECKSUM_COMBO, 			this);
	m_Height.			SubclassDlgItem(IDC_BC_HEIGHT_EDIT, 			this);
	m_CustomBarHeight.	SubclassDlgItem(IDC_BC_CUSTOM_BARS_HEIGHT,		this);

	LoadDefaultBarCode	();

	// Selezione del tipo corrente
	m_TypeCombo.SelectString(-1, strCurrentBarCodeType);
	m_TypeCombo.EnableWindow(m_bEnableTypeCbo);

	m_Height.SetValue(m_BarCode.m_nCustomBarHeight);
	m_CustomBarHeight.SetCheck(m_BarCode.m_nCustomBarHeight > 0);

	SetCheckSumModulesCombo		();
	SetCustomBarHeightControls	();

	m_CheckSumType.SetCurSel(m_BarCode.m_nCheckSumType + 1);

	BOOL bEan128 = m_BarCode.m_nBarCodeType == CBarCodeTypes::BC_EAN128 ||
		(m_BarCode.m_nBarCodeType == CBarCodeTypes::BC_DEFAULT &&
		m_nDefaultBarCode == CBarCodeTypes::BC_EAN128);

	m_VertRadioBtn.SetCheck(m_BarCode.m_bVertical); 
	m_HorzRadioBtn.SetCheck(!m_BarCode.m_bVertical); 

	m_ShowLabCheckBtn.SetCheck(m_BarCode.m_bShowLabel); 

	/*if (pAliasNr)
		SetCustomAliasNumberControls();*/

	DrawStaticBarCode();

	m_CheckSumType.SetCurSel(m_BarCode.m_nCheckSumType + 1);
	
	/*No refactoring*/
	/*Resize */
	CRect rectCtrl;
	this->GetClientRect(rectCtrl);
	this->SetWindowPos(NULL, 0, 0, rectCtrl.Width()+ ScalePix(200), rectCtrl.Height()+ ScalePix(50), SWP_NOMOVE);
	CWnd* pChild = this->GetWindow(GW_CHILD);
	pChild->GetClientRect(rectCtrl);
	pChild->SetWindowPos(NULL, 0, 0, rectCtrl.Width() + ScalePix(180), rectCtrl.Height(), SWP_NOMOVE);
	m_TestStatic.GetClientRect(rectCtrl);
	m_TestStatic.SetWindowPos(NULL, 0, 0, rectCtrl.Width() + ScalePix(160), rectCtrl.Height(), SWP_NOMOVE);
	/*End Resize*/

	return TRUE;
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::OnPaint()
{
	__super::OnPaint();

	DrawStaticBarCode();
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::OnBCValueChanged()
{
	m_ValueEdit.GetValue(m_strBCValue);
	DrawStaticBarCode();
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::OnDrawStaticBarCode()
{
	CRect	rcStaticRect;
	m_TestStatic.GetClientRect(rcStaticRect); 
	InvalidateRect(&rcStaticRect);
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::OnOK()
{
	EndDialog(IDOK);
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::OnCancel()
{
	EndDialog(IDCANCEL);
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::DrawStaticBarCode()
{
	CRect	rcStaticRect;
	m_TestStatic.GetClientRect(rcStaticRect);
	CDC*	pStaticDC = m_TestStatic.GetDC();

	FillRect(*pStaticDC, rcStaticRect, (HBRUSH)(COLOR_WINDOW + 1));

	if (m_strBCValue.IsEmpty())
	{
		ReleaseDC(pStaticDC);
		return;
	}

	LOGFONT lfFont;
	AfxGetThemeManager()->GetFormFont()->GetObject(sizeof(LOGFONT),&lfFont);

	// I change font to make barcode text readable (an 11.922)
	TB_TCSCPY(lfFont.lfFaceName, _T("Arial"));
	if (lfFont.lfHeight < -11)
		lfFont.lfHeight = -11;
	if (lfFont.lfWeight < 400)
		lfFont.lfWeight = 400;
	
	int nCboIdx = m_TypeCombo.GetCurSel();
	if (nCboIdx != CB_ERR)
		m_BarCode.m_nBarCodeType = (int)m_TypeCombo.GetItemData(nCboIdx);
	
	m_BarCode.m_nNarrowBar = m_NarrowBarEdit.GetValue();

	m_BarCode.m_bVertical = m_VertRadioBtn.GetCheck();
	
	m_BarCode.m_bShowLabel = m_ShowLabCheckBtn.GetCheck();

	nCboIdx = m_CheckSumType.GetCurSel();
	m_BarCode.m_nCheckSumType = nCboIdx >= 0 ? m_CheckSumType.GetItemData(nCboIdx) : -1;
	
	m_BarCode.m_nCustomBarHeight = m_Height.GetValue();
	if (m_CustomBarHeight.GetCheck() > 0)
		m_BarCode.m_nCustomBarHeight= m_Height.GetValue();
	else
		m_BarCode.m_nCustomBarHeight = -1;

	DrawBarCode(*pStaticDC, rcStaticRect, lfFont);
	
	ReleaseDC(pStaticDC); 
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::DrawBarCode(CDC& dc, CRect& rcRect, LOGFONT lfFont)
{
	CString sErr;
	
	BOOL bRet = m_BarCode.DrawBarCode
		(
			dc, rcRect, lfFont,	m_strBCValue,
			CLR_BLACK,
			CLR_WHITE,
			sErr
		);

	if (!bRet && !sErr.IsEmpty())
	{
		SetTextColor(dc, 0x00000000);
		SetBkMode(dc, TRANSPARENT);
		if (IsScale())
		{
			ScaleLogFont(&lfFont, dc);
			CFont newFont;
			newFont.CreateFontIndirect(&lfFont);
			CFont* pPrevFont = dc.SelectObject(&newFont);
			DrawText(dc, sErr, -1, rcRect, DT_CENTER);
			dc.SelectObject(pPrevFont);
		}
		else
			DrawText(dc, sErr, -1, rcRect, DT_CENTER);
	}
		
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::OnTypeBarCodeChanged ()
{
	SetCheckSumModulesCombo		();
	SetCustomBarHeightControls	();
	//SetCustomAliasNumberControls();
	DrawStaticBarCode			();
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::OnCustomBarsHeightChanged ()
{
	SetCustomBarHeightControls	();
	DrawStaticBarCode			();
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::SetCheckSumModulesCombo	()
{
	int bBarCodeType = 0;

	// selected barcode type 
	int nCboIdx = m_TypeCombo.GetCurSel();
	if (nCboIdx != CB_ERR)
		 bBarCodeType = (int)m_TypeCombo.GetItemData(nCboIdx);

	m_CheckSumType.ResetContent();

	LoadCheckSumModulesCombo	();

	m_CheckSumType.EnableWindow(TRUE);
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::LoadDefaultBarCode ()
{
	DataObj* pSetting = AfxGetSettingValue(snsTbGenlib, szReportSection, szBarCodeType, DataStr(), szTbDefaultSettingFileName);
	CString sDefaultBarcode = pSetting ? pSetting->Str() : _T("");	
	CString strTemp;
	
	if (sDefaultBarcode.IsEmpty()) 
		return;
	
	for (int nBCTypeIdx = 0 ; nBCTypeIdx < CBarCodeTypes::BARCODE_TYPES_NUM; nBCTypeIdx++)
	{
		strTemp = CBarCodeTypes::BarCodeDescription(CBarCodeTypes::s_bcTypes[nBCTypeIdx]);
		if (!sDefaultBarcode.CompareNoCase(strTemp))
		{
			m_nDefaultBarCode = CBarCodeTypes::s_bcTypes[nBCTypeIdx];
			break;
		}
	}	
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::LoadCheckSumModulesCombo	()
{
	int idx = m_CheckSumType.AddString(_T(""));
	m_CheckSumType.SetItemData(idx, -1);

	idx = m_CheckSumType.AddString(szChecksumModule10103_Default);
	m_CheckSumType.SetItemData(idx, 0);

	idx = m_CheckSumType.AddString(szChecksumModule103_Optional);
	m_CheckSumType.SetItemData(idx, 1);

	m_CheckSumType.SetCurSel(0);
}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::SetCustomBarHeightControls ()
{
	int nBarCodeType = 0;
	// selected barcode type 
	int nCboIdx = m_TypeCombo.GetCurSel();
	if (nCboIdx != CB_ERR)
		 nBarCodeType = (int)m_TypeCombo.GetItemData(nCboIdx);

	BOOL bEan128 = nBarCodeType == CBarCodeTypes::BC_EAN128 ||
		(nBarCodeType == CBarCodeTypes::BC_DEFAULT &&
		m_nDefaultBarCode == CBarCodeTypes::BC_EAN128);

	if (!bEan128)
	{
		m_CustomBarHeight.SetCheck(FALSE);
		m_Height.SetValue(-1);
	}

	m_CustomBarHeight.EnableWindow(bEan128);
	m_Height.EnableWindow(bEan128 && m_CustomBarHeight.GetCheck() > 0);
}

//----------------------------------------------------------------------------
//void CBarCodeSettingsDlg::SetCustomAliasNumberControls ()
//{
//	int nBarCodeType = 0;
//	// selected barcode type 
//	int nCboIdx = m_TypeCombo.GetCurSel();
//	if (nCboIdx != CB_ERR)
//		 nBarCodeType = (int)m_TypeCombo.GetItemData(nCboIdx);
//}

//----------------------------------------------------------------------------
void CBarCodeSettingsDlg::OnShowTextChanged()
{
	//SetCustomAliasNumberControls ();
	DrawStaticBarCode			();
}

//=============================================================================
//======================= CStaticImage Class  =================================
//=============================================================================


//=============================================================================


BEGIN_MESSAGE_MAP(CStaticImage, CStatic)
	//{{AFX_MSG_MAP(CStaticImage)
	ON_MESSAGE			(UM_GET_CONTROL_DESCRIPTION,		OnGetControlDescription)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


//-----------------------------------------------------------------------------
LRESULT CStaticImage::OnGetControlDescription(WPARAM wParam, LPARAM lParam)
{
	CWndObjDescriptionContainer* pContainer = (CWndObjDescriptionContainer*) wParam;
	
	/*Il metodo GetWindowDescription crea da zero una descrizione(del tipo della runtimeclass passata) se non esisteva gia,
	o ripesca quella gia creata nei round trip precedenti se esisteva.
	Nella creazione assegna un id alla descrizione (m_strId), che servira' da chiave per recuperarle.
	Questo id viene creato in modo standard sulla base dell'handle di finestra.
	In alcuni casi pero finestre "differenti" hanno lo stesso id (es. parsedbitmap del bodyedit). 
	In questi casi si puo' creare un ID disambiguo e passarlo al metodo GetWindowDescription.
	*/
	CString strId = (LPCTSTR)lParam;
	CWndImageDescription* pDesc = (CWndImageDescription*)pContainer->GetWindowDescription(this, RUNTIME_CLASS(CWndImageDescription), strId);
	pDesc->UpdateAttributes(this);

	//do un nome diverso ogni volta perche il barcode potrebbe cambiare, quindi l'immagine va aggiornata
	DWORD tick = GetTickCount();
	CString sName = cwsprintf(_T("%upic%uBarCode.png"), tick, m_hWnd);

	CDC* pStaticDC = GetDC();
	CDC memDC;
	if (!memDC.CreateCompatibleDC(pStaticDC))
		return NULL;
	
	CRect	rcStaticRect;
	LOGFONT lfFont;
	AfxGetThemeManager()->GetFormFont()->GetObject(sizeof(LOGFONT),&lfFont);

	// I change font to make barcode text readable (an 11.922)
	TB_TCSCPY(lfFont.lfFaceName, _T("MS Sans Serif"));
	if (lfFont.lfHeight < -11)
		lfFont.lfHeight = -11;
	if (lfFont.lfWeight < 400)
		lfFont.lfWeight = 400;
	
	GetClientRect(rcStaticRect);
	
	CBitmap bmp;
	bmp.CreateCompatibleBitmap(pStaticDC, rcStaticRect.Width(), rcStaticRect.Height());
	memDC.SelectObject(bmp);
	OnEraseBkgnd(&memDC);
	m_pBarCodeDlg->DrawBarCode(memDC, rcStaticRect, lfFont);
	memDC.SelectObject((CBitmap*)NULL);

	if (pDesc->m_ImageBuffer.Assign((HBITMAP)bmp, sName, this))
		pDesc->SetUpdated(&pDesc->m_ImageBuffer);

	ReleaseDC(pStaticDC);
	return (LRESULT) pDesc;
}
