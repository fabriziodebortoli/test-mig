
#include "stdafx.h"

#include <string.h>
#include <ctype.h>
#include <stdlib.h>
#include <math.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\ThreadContext.h>

#include <TbGeneric\Globals.h>
#include <TbGeneric\NumberToLiteral.h>
#include <TbGeneric\DataObjDescription.h>
#include <TbGeneric\NumberToLiteral.h>

#include "FormatsTable.h"
#include "FormatsHelpers.h"
#include "DataTypesFormatters.h"
#include "DataObj.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#endif

static const TCHAR BASED_CODE szEmpty[] = _T("");
//============================================================================
struct nameEntry
{
	int		num;
	TCHAR*	str;
};


//----------------------------------------------------------------------------
static const nameEntry BASED_CODE ne[]=
{           
	{	0,	_T("zero")},
	{	1,	_T("uno" )},      
	{	2,	_T("due" )},
	{	3,	_T("tre" )},
	{	4,	_T("quattro") },
	{	5,	_T("cinque") },
	{	6,	_T("sei") },
	{	7,	_T("sette") },
	{	8,	_T("otto") },
	{	9,	_T("nove") },
	{	10,	_T("dieci") },
	{	11, _T("undici") },
	{	12, _T("dodici") },
	{	13,	_T("tredici") },
	{	14, _T("quattordici") },
	{	15, _T("quindici") },
	{	16, _T("sedici") },
	{	17, _T("diciassette") },
	{	18, _T("diciotto") },
	{	19, _T("diciannove") },
	{	20, _T("venti") },
	{	21, _T("ventuno") },
	{	30, _T("trenta") },
	{	31, _T("trentuno") },
	{	40, _T("quaranta") },
	{	41, _T("quarantuno") },
	{	50, _T("cinquanta") },
	{	51, _T("cinquantuno") },
	{	60, _T("sessanta") },
	{	61, _T("sessantuno") },
	{	70, _T("settanta") },
	{	71, _T("settantuno") },
	{	80, _T("ottanta") },
	{	81, _T("ottantuno") },
	{	90, _T("novanta") },
	{	91, _T("novantuno") },
	{	100,_T("cento")},
	{	1000,_T("mille")}
};

static const TCHAR BASED_CODE sz1E0[]= _T("un");
static const TCHAR BASED_CODE szNE3[]= _T("mila");
static const TCHAR BASED_CODE sz1E6[]= _T("milione");
static const TCHAR BASED_CODE szNE6[]= _T("milioni");
static const TCHAR BASED_CODE sz1E9[]= _T("miliardo");
static const TCHAR BASED_CODE szNE9[]= _T("miliardi");

//----------------------------------------------------------------------------
static int cmp(const void* cpv0, const void* cpv1)
{
	int i = *(int*) cpv0;
	nameEntry* p = (nameEntry*)cpv1;

	return i - p->num;    
}


//----------------------------------------------------------------------------
static LPCTSTR lookup(int num)
{
	nameEntry* p = (nameEntry*)bsearch(&num,
									 ne,
									 sizeof(ne)/sizeof(nameEntry),
									 sizeof(nameEntry),
                                     cmp);
	return (p) ? (LPCTSTR)(p->str) : szEmpty;
}

//============================================================================
//					class FormatStyleLocale
//============================================================================

DECLARE_AND_INIT_THREAD_VARIABLE(Array*, t_pSupportedLocales, NULL)
//----------------------------------------------------------------------------
BOOL CALLBACK FormatLocaleEnumProc(LPTSTR lpLocaleString)
{
	// mi arriva senza la notazione esadecimale
	GET_THREAD_VARIABLE(Array*, t_pSupportedLocales)
	t_pSupportedLocales->Add (new DataStr(_T("0X") + CString(lpLocaleString)));
	return TRUE;
}

//----------------------------------------------------------------------------
FormatStyleLocale::FormatStyleLocale ()
{
	m_bLoaded	= FALSE;
	m_bEnabled	= FALSE;

	GET_THREAD_VARIABLE(Array*, t_pSupportedLocales)
	t_pSupportedLocales = &m_SupportedLocales;
	EnumSystemLocales ((LOCALE_ENUMPROC) FormatLocaleEnumProc, LCID_SUPPORTED);
}	

//----------------------------------------------------------------------------
void FormatStyleLocale::SetToProgramDefault ()
{
	m_sDecSeparator			= szDefDecSeparator;
	m_nDecimals				= nDefDecimals;
	m_s1000LongSeparator	= szDefLng1000Sep;
	m_s1000DoubleSeparator	= szDefDbl1000Sep;
	m_bZeroPadded			= FALSE;

	m_sTimeSeparator		= szDefTimeSep;
	m_sDateSeparator		= szDefDateSep;
	m_sAMSymbol				= szDefTimeAM;
	m_sPMSymbol				= szDefTimePM;

	m_TimeFormat			= CDateFormatHelper::TIME_NONE;
	m_ShortDateFormat		= CDateFormatHelper::DATE_DMY;
	m_ShortDateDayFormat	= CDateFormatHelper::DAY99;
	m_ShortDateMonthFormat	= CDateFormatHelper::MONTH99;
	m_ShortDateYearFormat	= CDateFormatHelper::YEAR99;
}
 
//----------------------------------------------------------------------------
LCID FormatStyleLocale::GetCultureLCID (const CString& sCulture)
{
	TCHAR sTmpBuffer1[255]; 
	TCHAR sTmpBuffer2[255]; 

	CString sCountry;
	int nPos = sCulture.FindOneOf(_T("-"));
	if (nPos > 0)
		sCountry = sCulture.Left(nPos);
	else
		sCountry = sCulture;

	sCountry.Trim();

	LCID idNeutral = 0, idDefault = 0;
	for (int i=0; i <= m_SupportedLocales.GetUpperBound(); i++)
	{
		DataStr* pDataStr = (DataStr*) m_SupportedLocales.GetAt (i);

		// trasformo il valore esadecimale in decimale
		int langid = 0;
		
		_stscanf_s((LPCTSTR) pDataStr->Str(),  _T("%x"), &langid);
		LCID lcid = MAKELCID(langid, SORT_DEFAULT);

		// chiedo le informazioni corrispondenti al numero
		int nBuffer1Size = GetLocaleInfo(lcid, LOCALE_SISO639LANGNAME, (LPTSTR) sTmpBuffer1, sizeof(sTmpBuffer1));
		int nBuffer2Size = GetLocaleInfo(lcid, LOCALE_SISO3166CTRYNAME, (LPTSTR) sTmpBuffer2, sizeof(sTmpBuffer2));

		if (nBuffer1Size <= 0  || nBuffer2Size <= 0)
			continue;

		CString sISO639	(sTmpBuffer1, nBuffer1Size-1);
		sISO639.Trim();

		if (sISO639.IsEmpty())
			continue;

		CString sISO3166 (sTmpBuffer2, nBuffer2Size-1);
		sISO3166.Trim();

		CString sLocaleName;
		if (!sISO3166.IsEmpty() && sCulture.GetLength() > sISO639.GetLength())
		{
			sLocaleName = sISO639;
			sLocaleName = sLocaleName + _T("-");
			sLocaleName = sLocaleName + sISO3166;
		}

		if (_tcsicmp(sLocaleName, sCulture) == 0)
			return lcid;

		// mi tengo anche il valore della cultura neutrale
		if (_tcsicmp(sISO639, sCountry) == 0)
		{
			WORD sublangid = SUBLANGID(langid);
			if (sublangid == SUBLANG_DEFAULT)
				idNeutral = lcid;
	
			if (sublangid == SUBLANG_DEFAULT)
				idDefault = lcid;
		}	
	}

	if (idNeutral > 0)
		return idNeutral;
	
	return idDefault;
}

//----------------------------------------------------------------------------
BOOL FormatStyleLocale::ReadSettings (const CString& sCulture)
{
	m_bEnabled = AfxGetStringLoader() != NULL;

	if (!m_bEnabled || sCulture.IsEmpty())
	{
		m_bLoaded = FALSE;
		SetToProgramDefault ();
		return FALSE;
	}

	// saranno le singole letture ad invalidare il loading
	m_bLoaded = TRUE;

	LCID lcid = GetCultureLCID(sCulture);
	
	if (lcid == 0)
	{
		m_bEnabled = FALSE;
		m_bLoaded = FALSE;
		SetToProgramDefault ();
		return FALSE;
	}

	ReadNumericFormat	(lcid);
	ReadDateFormat		(lcid);
	ReadTimeFormat		(lcid);

	RefreshFormatters	();

	return m_bLoaded;
}

//----------------------------------------------------------------------------
void FormatStyleLocale::RefreshFormatters	()
{
	for (int i=0; i <= AfxGetFormatStyleTable()->GetUpperBound(); i++)
	{
		FormatterGroup* pGroup = AfxGetFormatStyleTable()->GetAt(i);

		for (int n=0; n <= pGroup->GetFormatters().GetUpperBound(); n++)
		{
			Formatter* pFormatter = (Formatter*) pGroup->GetFormatters().GetAt(n);
			pFormatter->SetToLocale	();
			pFormatter->RecalcWidths();
		}
	}
}

//----------------------------------------------------------------------------
void FormatStyleLocale::ReadNumericFormat (LCID id)
{
	TCHAR sTmpBuffer[255]; 
	int nSize = sizeof(sTmpBuffer)* sizeof(TCHAR);

	// decimals
	if (GetLocaleInfo(id, LOCALE_IDIGITS, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}

	m_nDecimals = _ttoi(sTmpBuffer);

	// decimal separator
	if (GetLocaleInfo(id, LOCALE_SDECIMAL, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}

	m_sDecSeparator = CString (sTmpBuffer);
	
	// thousand separator
	if (GetLocaleInfo(id, LOCALE_STHOUSAND, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	m_s1000LongSeparator = CString (sTmpBuffer);
	m_s1000DoubleSeparator = m_s1000LongSeparator;

	// leading zeros
	if (GetLocaleInfo(id, LOCALE_ILZERO, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}

	CString sZeros (sTmpBuffer);
	m_bZeroPadded = _tcsicmp(sZeros, _T("0")) == 0;
}

//----------------------------------------------------------------------------
void FormatStyleLocale::ReadDateFormat (LCID id)
{
	TCHAR sTmpBuffer[255]; 
	int nSize = sizeof(sTmpBuffer)* sizeof(TCHAR);

	// date format
	if (GetLocaleInfo(id, LOCALE_IDATE, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	
	CString sDate (sTmpBuffer);
	if (_tcsicmp(sDate, _T("2")) == 0)
		m_ShortDateFormat = CDateFormatHelper::DATE_YMD;
	else if (_tcsicmp(sDate, _T("1")) == 0)
		m_ShortDateFormat = CDateFormatHelper::DATE_DMY;
	else
		m_ShortDateFormat = CDateFormatHelper::DATE_MDY;

	// year format
	if (GetLocaleInfo(id, LOCALE_ICENTURY, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	CString sYear (sTmpBuffer);
	if (_tcsicmp(sYear, _T("1")) == 0)
		m_ShortDateYearFormat = CDateFormatHelper::YEAR9999;
	else
		m_ShortDateYearFormat = CDateFormatHelper::YEAR99;

	// month format
	if (GetLocaleInfo(id, LOCALE_IMONLZERO, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	CString sMonth (sTmpBuffer);
	if (_tcsicmp(sMonth, _T("0")) == 0)
		m_ShortDateMonthFormat = CDateFormatHelper::MONTH9;
	else
		m_ShortDateMonthFormat = CDateFormatHelper::MONTH99;

	// day format
	if (GetLocaleInfo(id, LOCALE_IDAYLZERO, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	CString sDay (sTmpBuffer);
	if (_tcsicmp(sDay, _T("0")) == 0)
		m_ShortDateDayFormat = CDateFormatHelper::DAY9;
	else
		m_ShortDateDayFormat = CDateFormatHelper::DAY99;

	// date separator
	if (GetLocaleInfo(id, LOCALE_SDATE, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	m_sDateSeparator = CString(sTmpBuffer);

	if (GetLocaleInfo(id, LOCALE_SSHORTDATE , (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		//m_bLoaded = FALSE;
		return;
	}
	m_sShortDateFormat = sTmpBuffer;
}

//----------------------------------------------------------------------------
void FormatStyleLocale::ReadTimeFormat (LCID id)
{
	TCHAR sTmpBuffer[255]; 
	int nSize = sizeof(sTmpBuffer)* sizeof(TCHAR);

	// AM Symbol 
	if (GetLocaleInfo(id, LOCALE_S1159, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	m_sAMSymbol = CString(sTmpBuffer);

	// PM Symbol 
	if (GetLocaleInfo(id, LOCALE_S2359, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	m_sPMSymbol = CString(sTmpBuffer);

	// Time separator
	if (GetLocaleInfo(id, LOCALE_STIME, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	m_sTimeSeparator = CString(sTmpBuffer);

	// time format
	if (GetLocaleInfo(id, LOCALE_STIMEFORMAT, (LPTSTR) sTmpBuffer, nSize) <= 0)
	{
		m_bLoaded = FALSE;
		return;
	}
	
	m_sLongTimeFormat = sTmpBuffer;

	CString sTimeFormat (sTmpBuffer);

	// seconds & minute digits
	BOOL bSeconds	= (sTimeFormat.Find(_T("ss")) >= 0) || (sTimeFormat.Find(_T("s")) >= 0);

	// minute digits number
	int nHours = 0;
	BOOL b12Day = FALSE;
	if (sTimeFormat.Find(_T("HH")) == 0)
		nHours = 2;
	else if (sTimeFormat.Find(_T("H")) == 0)
		nHours = 1;
	else if (sTimeFormat.Find(_T("hh")) == 0)
	{
		b12Day = TRUE;
		nHours = 2;
	}
	else if (sTimeFormat.Find(_T("h")) == 0)
	{
		b12Day = TRUE;
		nHours = 1;
	}

	m_TimeFormat = (nHours > 1 ? CDateFormatHelper::TIME_HF99 : CDateFormatHelper::TIME_HF9);

	if (!bSeconds)
		m_TimeFormat = (CDateFormatHelper::TimeFormatTag) (m_TimeFormat | CDateFormatHelper::TIME_NOSEC);

	if (b12Day)
		m_TimeFormat = (CDateFormatHelper::TimeFormatTag) (m_TimeFormat | CDateFormatHelper::TIME_AMPM);
}

//============================================================================
//					class FormatHelpers
//============================================================================

//----------------------------------------------------------------------------
void FormatHelpers::InsertThousandSeparator	(TCHAR* pszIOBuffer, LPCTSTR Separator)
{
	CString str(pszIOBuffer); 
	InsertThousandSeparator(str, Separator);
	TB_TCSCPY(pszIOBuffer, str);
}

//----------------------------------------------------------------------------
void FormatHelpers::InsertThousandSeparator(CString& strIOBuffer, LPCTSTR Separator)
{
	// Si prendera` comunque solo il primo carattere del separatore
	if (Separator && *Separator)
    {
		// viene fatta una copia in place partendo dal fondo utilizzando
		// tanti extra byte oltre la fine del corrente numero quanti ne servono
		// per inserire i separatori

		int nStart = strIOBuffer.GetLength() % 3;
		
		while (nStart<strIOBuffer.GetLength())
		{
			if(nStart!=0)
			{
				strIOBuffer.Insert(nStart, Separator);
				nStart++;
			}
			nStart += 3;
		}
	}
}

//----------------------------------------------------------------------------
const CNumberToLiteralLookUpTableManager* AfxGetNumberToLiteralLookUpTableManager()
{
	CLoginContext* pContext = AfxGetLoginContext();
	if (!pContext) 
		return NULL;
	return pContext->GetObject<const CNumberToLiteralLookUpTableManager>(&CLoginContext::GetNTLLookUpTableManager);
}

//----------------------------------------------------------------------------
BOOL FormatHelpers::NeedDecimalConversionInLetter()
{	
	const CNumberToLiteralLookUpTableManager* pNTLLookUpTableManager = AfxGetNumberToLiteralLookUpTableManager();
	if (pNTLLookUpTableManager == NULL)
		return FALSE;
	
	return pNTLLookUpTableManager->m_bDecimalLiteral;
}


// Converte in lettere la parte decimale del valore passato con la precisione fissa di 2 cifre
//----------------------------------------------------------------------------
void FormatHelpers::DecimalToWords(double aValue, CString& result)
{		
	const CNumberToLiteralLookUpTableManager* pNTLLookUpTableManager = AfxGetNumberToLiteralLookUpTableManager();
	if (pNTLLookUpTableManager == NULL)
		return;
	
	double divisor = 1.0;
	double decimalPart = modf(aValue, &divisor);
	DataDbl dbl(decimalPart * 100);
	dbl.Round(0);
	if (dbl.IsEmpty())
		return;
	
	NumberToWords(dbl, result, TRUE);
	
	if (floor(aValue) > 0)
		result = pNTLLookUpTableManager->m_strUniversalSeparator + result;
	
	if (dbl > 1)
		result += pNTLLookUpTableManager->m_strCentesimalPlural;
	else
		result += pNTLLookUpTableManager->m_strCentesimalSingular;
}



//arriva fino a 9.999.999.999(MAXDIGIT = 10)
// questa è la versione nuova
//----------------------------------------------------------------------------
void FormatHelpers::NumberToWords(double aValue, CString& result, BOOL bConvertDecimal /*= FALSE*/)
{				
	const CNumberToLiteralLookUpTableManager* pNTLLookUpTableManager = AfxGetNumberToLiteralLookUpTableManager();

	double dValue = aValue >= 0 ? aValue : (aValue * -1);
	
	result.Empty();
	
	if (pNTLLookUpTableManager == NULL)
		return;

	BOOL bUsedJunction = FALSE;
	BOOL bWriteMilliards = FALSE;
	//usato del brasile come separatore per ogni cifra
	CString strUniversalSeparator = pNTLLookUpTableManager->m_strUniversalSeparator;
	
	result = pNTLLookUpTableManager->Get((long)dValue, 0);
	if (!result.IsEmpty())
	{
		if (!bConvertDecimal)
		{
			if (aValue > 1)
				result.Append(pNTLLookUpTableManager->m_strCurrencyPlural);
			else
				result.Append(pNTLLookUpTableManager->m_strCurrencySingular);
		}
		return;
	}

	const int MAXDIGIT = 15;
	int ris[MAXDIGIT];

	int len = 0;

	for (int n = MAXDIGIT - 1; n >= 0; n--)//popolo un array con le cifre che compongono aValue
	{
		ris[MAXDIGIT - n - 1] = (int)(floor(dValue / pow((double)10,(double)n)));
		
		if (ris[MAXDIGIT - n - 1] > 0 || len > 0)
			len += 1;

		dValue = (double)(dValue - (ris[MAXDIGIT - n - 1] * pow((double)10,(double)n)));
	}

	for (int n = 12; n >= 0; n-=3)
	{
		long tripla = (ris[n] * 100) + (ris[n + 1] * 10) + ris[n + 2];
		long doppia = (ris[n + 1] * 10) + ris[n + 2];
		long lastDigit = ris[n + 2];

		if (tripla == 0)
		{
			//sto interpretando i miliardi, se sono a 0 permetto che venga scritta la dicitura "miliardi"
			//in caso esistano migliaia di miliadri (gruppo successivo da interpretare)
			if (n == 3)
				bWriteMilliards = TRUE;
			continue;
		}

		if (!result.IsEmpty())
			if (!(n==6 && ((ris[9] * 100) + (ris[10] * 10) + ris[11]) == 1 && !pNTLLookUpTableManager->GetHundreds()->m_bUseJunction.IsEmpty()))
				result = AddSeparator(len, tripla, doppia, bUsedJunction) + result;

		if (tripla == 1)
		{
			if (strUniversalSeparator.IsEmpty())
				result = ConcatenaGruppoSingolare(n, len+1, bWriteMilliards) + result;
			else
			{
				if (!result.IsEmpty())
					result = strUniversalSeparator + result;
				result = ConcatenaGruppoSingolare(n, len+1, bWriteMilliards) + result;
			}
			continue;
		}

		CString tmpres;
		tmpres.Empty();

		tmpres = pNTLLookUpTableManager->Get(tripla, n);
		if (tmpres != _T(""))
		{
			if (strUniversalSeparator.IsEmpty())
				result = tmpres + ConcatenaGruppo(n, (int)tripla, lastDigit, bWriteMilliards) + result;
			else
			{
				if (!result.IsEmpty())
					result = strUniversalSeparator + result;
				result = tmpres + ConcatenaGruppo(n, (int)tripla, lastDigit, bWriteMilliards) + result;
			}

			continue;
		}

		CString hundredResult;
		hundredResult.Empty();

		if (ris[n] > 0)
		{
			tmpres = pNTLLookUpTableManager->Get(ris[n] * 100, n);
			if (tmpres == _T(""))
			{
				hundredResult.Append(pNTLLookUpTableManager->Get(ris[n], n));
				hundredResult.Append(pNTLLookUpTableManager->GetHundreds()->GetDescription((int)tripla, ris[n + 2]));
			}
			else
				hundredResult.Append(tmpres);
		}

		if (doppia == 0)
		{
			if (strUniversalSeparator.IsEmpty())
			{
				result = hundredResult + ConcatenaGruppo(n, (int)tripla, lastDigit, bWriteMilliards) + result;
			}
			else
			{
				if (!result.IsEmpty())
				{
					result = strUniversalSeparator + result;
				}
				result = hundredResult + ConcatenaGruppo(n, (int)tripla, lastDigit, bWriteMilliards) + result;
			}
			continue;
		}

		tmpres = pNTLLookUpTableManager->Get(doppia, n);
		if (tmpres != _T(""))
		{
			CString junct = pNTLLookUpTableManager->GetHundreds()->m_bUseJunction;
			bUsedJunction = TRUE;
			if (strUniversalSeparator.IsEmpty())
			{
				result = hundredResult + junct + tmpres + ConcatenaGruppo(n, (int)tripla, lastDigit, bWriteMilliards) + result;
			}
			else
			{
				if (!result.IsEmpty())
				{
					result = strUniversalSeparator + result;
				}
				result = hundredResult + junct + tmpres + ConcatenaGruppo(n, (int)tripla, lastDigit, bWriteMilliards) + result;
			}
			continue;
		}

		if (pNTLLookUpTableManager->m_bUnitInversion)
		{
			if (ris[n+2] > 0)
				result.Append(pNTLLookUpTableManager->Get(ris[n+2], n));

			result.Append(pNTLLookUpTableManager->m_Junction);
			bUsedJunction = TRUE;

			if (ris[n+1] > 0)
			{
				result.Append(pNTLLookUpTableManager->Get(ris[n+1] * 10, n));
			}
		}
		else
		{
			if (ris[n+1] > 0)
			{
				tmpres = pNTLLookUpTableManager->Get(ris[n+1] * 10, n);
			}

			tmpres.Append(pNTLLookUpTableManager->m_Junction);
			bUsedJunction = TRUE;

			if (ris[n+2] > 0)
				tmpres.Append(pNTLLookUpTableManager->Get(ris[n+2], n));
		}
		if (!strUniversalSeparator.IsEmpty())
		{
			if (!result.IsEmpty())
			{
				result = strUniversalSeparator + result;
			}
			if (!hundredResult.IsEmpty())
			{
				hundredResult = hundredResult + strUniversalSeparator;
			}

			result = hundredResult + tmpres + ConcatenaGruppo(n, (int)tripla, lastDigit, bWriteMilliards) + result;
		}
		else
			result = hundredResult + tmpres + ConcatenaGruppo(n, (int)tripla, lastDigit, bWriteMilliards) + result;
	}

	if (!bConvertDecimal)
	{
		if (aValue > 1)
			result.Append(pNTLLookUpTableManager->m_strCurrencyPlural);
		else
			result.Append(pNTLLookUpTableManager->m_strCurrencySingular);
	}
	result.Trim().Replace(_T("  "), _T(" "));
}

CString FormatHelpers::AddSeparator(int len, long valTripla, long valDoppia, BOOL& bUsedJunction)
{
	const CNumberToLiteralLookUpTableManager* pNTLLookUpTableManager = AfxGetNumberToLiteralLookUpTableManager();
	BOOL bUsedJunct = bUsedJunction && !pNTLLookUpTableManager->GetHundreds()->m_bUseJunction.IsEmpty();
	bUsedJunction = FALSE;
	
	for (int i = 0; i < pNTLLookUpTableManager->GetExceptions()->GetSize(); i++)
	{
		if (pNTLLookUpTableManager->GetExceptions()->GetAt(i)->m_Value == len)
			return _T("");
	}

	if (bUsedJunct)
		return _T("");

	return pNTLLookUpTableManager->m_Separator;
}
CString FormatHelpers::ConcatenaGruppoSingolare(long i, int len, BOOL& bWriteMilliards)
{
	const CNumberToLiteralLookUpTableManager* pNTLLookUpTableManager = AfxGetNumberToLiteralLookUpTableManager();

	CString res;
	res.Empty();

	switch(i)
	{
		case 12:
			return pNTLLookUpTableManager->Get(1, i);
		case 9:
			res = pNTLLookUpTableManager->Get(1000, i);
			if (res == _T(""))
			{
				res = pNTLLookUpTableManager->Get(1, i);
				res.Append(ConcatenaGruppo(i, 1, 1, bWriteMilliards));
			}
			break;
		case 6:
			res = pNTLLookUpTableManager->Get(1000000, i);
			if (res == _T(""))
			{
				res = pNTLLookUpTableManager->Get(1, i);
				res.Append(ConcatenaGruppo(i, 1, 1, bWriteMilliards));
			}
			break;
		case 3:
			res = pNTLLookUpTableManager->Get(1000000000, i);
			if (res == _T(""))
			{
				res = pNTLLookUpTableManager->Get(1, i);
				res.Append(ConcatenaGruppo(i, 1, 1, bWriteMilliards));
			}
			break;
		case 0:
			res = pNTLLookUpTableManager->Get(1000, i);
			if (res == _T(""))
			{
				res = pNTLLookUpTableManager->Get(1, i);
				res.Append(ConcatenaGruppo(i, 1, 1, bWriteMilliards));
			}
			else
				if (bWriteMilliards)
					res.Append(pNTLLookUpTableManager->GetMilliards()->GetDescription(1000, 0));
			break;
	}

	return res;
}

CString FormatHelpers::ConcatenaGruppo(long i, int nValue, int lastDigit, BOOL& bWriteMilliards)
{
	const CNumberToLiteralLookUpTableManager* pNTLLookUpTableManager = AfxGetNumberToLiteralLookUpTableManager();
	CString res;
	res.Empty();

	switch(i)
	{
		case 12:
			return _T("");
		case 9: 
			res = pNTLLookUpTableManager->GetThousands()->GetDescription(nValue, lastDigit);
			break;
		case 6:
			res = pNTLLookUpTableManager->GetMillions()->GetDescription(nValue, lastDigit);
			break;
		case 3:
			res = pNTLLookUpTableManager->GetMilliards()->GetDescription(nValue, lastDigit);
			break;
		case 0:
			res = pNTLLookUpTableManager->GetThousands()->GetDescription(nValue, lastDigit);
			if (bWriteMilliards)
					res.Append(pNTLLookUpTableManager->GetMilliards()->GetDescription(nValue, lastDigit));
			break;
	}

	return res;
}


//----------------------------------------------------------------------------
void FormatHelpers::NumberToEncoded(long l, LPCTSTR pszXTable, TCHAR* buffer)
{
	TCHAR* ptr = buffer;
	
	for (;l; l/=10) 
		*ptr++ = pszXTable[(int) (l % 10)];
		
	*ptr = NULL_CHAR;    
	_tcsrev(buffer);
}

//----------------------------------------------------------------------------
double FormatHelpers::RoundSigned(double nValue, double quantum)
{
	if (quantum == 0.0)
		return nValue;

	double nQuantum = quantum;
	double e = 1.;
	
	while (floor(nQuantum) < nQuantum) { nQuantum *= 10.; nValue *= 10.; e *= 10.; }

	double r = fmod(nValue, nQuantum);
	nValue -= r;
	double p = nQuantum / 2.;

	// Codice Originale Germano.  >if (r >= p)<
	// Deve essere eliminato il rumore decimale 
	// che rimane in r. (ad es. nValue=497.105 
	// e qantum=0.01). Bruna
	if ((p - r) <= DataDbl::GetEpsilon())
		nValue += nQuantum;

	return nValue / e;
}

//----------------------------------------------------------------------------
double FormatHelpers::RoundAbsolute(double nValue, double quantum)
{
	return RoundSigned(fabs(nValue), quantum) * (nValue >= 0. ? 1 : -1);
}

//----------------------------------------------------------------------------
double FormatHelpers::RoundZero(double nValue, double quantum)
{
	if (quantum == 0.0)
		return nValue;

	int sign = (nValue >= 0.) ? 1 : -1;
	nValue = fabs(nValue);

	double nQuantum = quantum;
	double e = 1.;
	
	while (floor(nQuantum) < nQuantum) { nQuantum *= 10.; nValue *= 10.; e *= 10.; }

	return sign * nQuantum * floor(nValue / nQuantum) / e;
}

//----------------------------------------------------------------------------
double FormatHelpers::RoundInfinite(double nValue, double quantum)
{
	if (quantum == 0.0)
		return nValue;

	int sign = (nValue >= 0.) ? 1 : -1;
	nValue = fabs(nValue);

	double nQuantum = quantum;
	double e = 1.;
	
	while (floor(nQuantum) < nQuantum) { nQuantum *= 10.; nValue *= 10.; e *= 10.; }

	// Prevenzione rumore (anomalia 5646)
	// Se nValue ha del rumore lo pulisco 6404.000000000001 diventa 6404
	// se nValue ha dei decimali validi   6404.300000000001 non faccio nulla, ma la ceil lo
	// arotonda all'intero superiore e diventa correttamente 6405 (nQuantum e' diventato intero)
	if ((nValue - floor(nValue)) <= DataDbl::GetEpsilon())
		nValue = floor(nValue);

	return sign * nQuantum * ceil(nValue / nQuantum) / e;
}

//----------------------------------------------------------------------------
double FormatHelpers::Round(double nValue, double quantum, RoundMode Rm)
{
	if (quantum == 0. || nValue == 0.) return nValue;

	switch (Rm)
	{
		case Signed:	return FormatHelpers::RoundSigned(nValue, quantum);
		case Absolute:	return FormatHelpers::RoundAbsolute(nValue, quantum);
		case Zero:		return FormatHelpers::RoundZero(nValue, quantum);
		case Infinite:	return FormatHelpers::RoundInfinite(nValue, quantum);
		default:		return 0;
	}
}
