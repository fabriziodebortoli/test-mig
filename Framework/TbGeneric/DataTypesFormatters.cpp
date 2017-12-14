#include "stdafx.h"

#include <math.h>
#include <memory.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <float.h>

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\JsonSerializer.h>
#include "EnumsTable.h"
#include "FontsTable.h"
#include "GeneralFunctions.h"
#include "FormatsHelpers.h"
#include "TBThemeManager.h"

#include "DataObj.h"

#include "DataTypesFormatters.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif


static const TCHAR szPlus[]			=_T("+");
static const TCHAR szMinus[]		=_T("-");
static const TCHAR szOpenedRound[]	=_T("(");
static const TCHAR szClosedRound[]	=_T(")");
static const TCHAR szE[]			=_T("E");

static const TCHAR szXlat_413_91	[]=_T("ZAEGHMPSTK");
static const TCHAR szXlat_413_91_x	[]=_T("ZAEGHMPSTK,");

static const TCHAR szYearLocalizationChars	[]=_T("@@@");

//============================================================================
//					General Functions
//============================================================================

//----------------------------------------------------------------------------
Formatter* PolyNewFormatter	(				
								const DataType&			aDataType, 
								const Formatter::FormatStyleSource& aSource /*FROM_STANDARD*/,
								const CTBNamespace&		aOwner /*CTBNamespace()*/,
								LPCTSTR					aName /*NULL*/
							)
{
	CString sName (aName);
	if (aName == NULL)
		sName = FromDataTypeToFormatName(aDataType);

	Formatter* pFormatter = NULL;

	switch (aDataType.m_wType)
    {
		case DATA_STR_TYPE	:	
				pFormatter = new CStringFormatter	(sName, aSource, aOwner);
				break;
		case DATA_INT_TYPE	: 
				if (sName == _NS_FMT("MonthName"))
					pFormatter = new CMonthNameFormatter(aName, aSource, aOwner);
				else if (sName == _NS_FMT("WeekDayName"))
					pFormatter = new CWeekDayNameFormatter(aName, aSource, aOwner);
				else
					pFormatter = new CIntFormatter	(sName, aSource, aOwner);
				break;
		case DATA_LNG_TYPE	: 
			if (aDataType.IsATime())		//@@ElapsedTime
				pFormatter = new CElapsedTimeFormatter(sName, aSource, aOwner);
			else
				pFormatter = new CLongFormatter(sName, aSource, aOwner);
			break;
		case DATA_DBL_TYPE	: 
			pFormatter = new CDblFormatter	(sName, aSource, aOwner);
			break;
		case DATA_MON_TYPE	: 
			pFormatter = new CMonFormatter	(sName, aSource, aOwner);
			break;
		case DATA_QTA_TYPE	:
			pFormatter = new CQtaFormatter	(sName, aSource, aOwner);
			break;
		case DATA_PERC_TYPE	: 
			pFormatter = new CPercFormatter	(sName, aSource, aOwner);
			break;
		case DATA_DATE_TYPE	: 
			if (aDataType.IsFullDate())
				if (aDataType.IsATime())
					pFormatter = new CTimeFormatter(sName, aSource, aOwner);
				else
					pFormatter = new CDateTimeFormatter(sName, aSource, aOwner);
			else
				pFormatter = new CDateFormatter(sName, aSource, aOwner);
			break;
		case DATA_BOOL_TYPE	: 
			pFormatter = new CBoolFormatter	(sName, aSource, aOwner);
			break;
		case DATA_ENUM_TYPE	: 
			pFormatter = new CEnumFormatter	(sName, aSource, aOwner);	
			break;
		case DATA_GUID_TYPE	: 
			pFormatter = new CGuidFormatter	(sName, aSource, aOwner);
			break;
		case DATA_TXT_TYPE	: 
			pFormatter = new CTextFormatter(sName, aSource, aOwner);
			break;
	}

	if (pFormatter)
		pFormatter->RecalcWidths();

	return pFormatter;
}

//============================================================================
//		Class CFormatMask Implementation
//============================================================================
//----------------------------------------------------------------------------
CFormatMask::CFormatMask()
	:
	m_bEnabled	(TRUE)
{
	InitMaskInfo();
}

//----------------------------------------------------------------------------
void CFormatMask::InitMaskInfo ()
{
	m_bIsIrrilevant		= TRUE;
	m_nEditableZoneStart= -1;
	m_nEditableZoneEnd	= -1;
	m_HowManyOfDecimal	= -1;
	m_nSuffixStart		= -1;
	m_HowManyOfYear		= 0;
	m_HowManyOfNumber	= 0;
	m_HowManyOfSiteCode = 0;
	m_nNumberStartAt	= 0;
}

//----------------------------------------------------------------------------
void CFormatMask::SetSiteCode (const CString& strCode)
{
	m_sSiteCode = strCode;
}

//----------------------------------------------------------------------------
void CFormatMask::SetEnabled (const BOOL& bValue)
{
	m_bEnabled = bValue;
}

//----------------------------------------------------------------------------
bool CFormatMask::IsToApply	(const CString& sValue) const
{
	return m_bEnabled && !m_bIsIrrilevant && !sValue.IsEmpty() && isdigit(sValue.GetAt(0));
}

//----------------------------------------------------------------------------
CString CFormatMask::ApplyMask (const CString str, BOOL bZeroPadded, int nYear /*0*/) const
{
	CString strResult;
	if (!IsToApply(str))
	{
		strResult.Append(str);
		return strResult;
	}

	CString sNumber = str.GetLength() > m_HowManyOfNumber ? str.Mid(m_nNumberStartAt, m_HowManyOfNumber) : str;

	if (m_HowManyOfDecimal > 0)
	{
		double lNr = _ttof(sNumber);
		if (lNr >= 0)
			sNumber =  cwsprintf(_T("%0") + cwsprintf(_T("%d.%d"), m_HowManyOfNumber + 1, m_HowManyOfDecimal) + _T("f"), lNr);
	}
	else
	{

		int lNr = _ttol(sNumber);
		if (lNr >= 0)
			sNumber = bZeroPadded ? cwsprintf(_T("%0") + cwsprintf(_T("%d"), m_HowManyOfNumber) + _T("d"), lNr) : cwsprintf(_T("%d"), lNr);
	}

	if (sNumber.IsEmpty())
	{
		strResult = str;
		return strResult;
	}

	CString sYear = cwsprintf(_T("%d"),  nYear > 0 ? nYear : AfxGetApplicationYear());
	// mi salvo il suffisso esistente
	CString sExistingSuffix;
	if (m_nEditableZoneStart > 0)
	{
		int s=-1;
		for (s=str.GetLength()-1; s >=0; s--)
			if (isdigit(str.GetAt(s)))
				break;

		if (s >= 0)
			sExistingSuffix = str.Mid(s+1);
	}

	int nAppliedYear = sYear.GetLength() - m_HowManyOfYear;
	int nAppliedSite = 0;
	int nAppliedNumbers = 0;
	int nAppliedSuffix = 0;
	
	for (int i=0; i < m_strMask.GetLength(); i++)
	{
		// gestione dell'anno
		if (m_strMask.GetAt(i) == szYearChar)
		{
			strResult += sYear[nAppliedYear];
			nAppliedYear++;
		}
		// gestione del numero
		else if (m_strMask.GetAt(i) == szNumberChar)
		{
			// gestione del decimale lo metterà la maschera
			if (sNumber[nAppliedNumbers] == szCommaChar || sNumber[nAppliedNumbers] == DOT_CHAR)
				nAppliedNumbers++;
			
			strResult += sNumber[nAppliedNumbers];
			nAppliedNumbers++;
		}

		// gestione del site di xtech, cercando di preservare l'esistente
		else if (m_strMask.GetAt(i) == szSiteCodeChar && !m_sSiteCode.IsEmpty())
		{
			CString stringToUse = sExistingSuffix.IsEmpty() ? m_sSiteCode : sExistingSuffix;
			if (!stringToUse.IsEmpty() && stringToUse.GetLength() > nAppliedSite)
			{
				strResult += stringToUse.GetAt(nAppliedSite);
				nAppliedSite++;
			}
		}
		// gestione del suffisso cercando di preservare l'esistente
		else if (m_strMask.GetAt(i) == szSuffixChar)
		{
			if (!sExistingSuffix.IsEmpty() && sExistingSuffix.GetLength() > nAppliedSuffix)
			{
				strResult += sExistingSuffix.GetAt(nAppliedSuffix);
				nAppliedSuffix++;
			}
		}
		// il suffisso non editabile va eliminato
		else if (m_strMask.GetAt(i) == szNonEditableSuffix)
			continue;
		// gestione N
		else if (m_strMask.GetAt(i) == szNoPaddingNo)
		{
			strResult += sNumber;
		}
		else if (m_strMask.GetAt(i) != szSuffixChar)
		{
			strResult += m_strMask.GetAt(i);
		}
	}

	return strResult;
}

//----------------------------------------------------------------------------
void CFormatMask::SetMask (const CString& strMask)
{
	m_strMask = strMask;
	m_strMask.MakeUpper();

	ParseMaskInfo();
}

// si occupa di calcolare le caratteristiche di formattazione della maschera
//----------------------------------------------------------------------------
void CFormatMask::ParseMaskInfo ()
{
	InitMaskInfo();

	int nHowManyOfSuffix = 0;
	for (int i=0; i < m_strMask.GetLength(); i++)
	{
		if (m_strMask.GetAt(i) == szYearChar)
			m_HowManyOfYear++;
		else if (m_strMask.GetAt(i) == szNumberChar)
		{
			if (m_HowManyOfDecimal >= 0)
				m_HowManyOfDecimal++;
			
			if (m_HowManyOfNumber == 0)
				m_nNumberStartAt = i;
			
			m_HowManyOfNumber++;
		}
		// abilitazione dell'uso dei decimali
		else if (m_strMask.GetAt(i) == szCommaChar)
			m_HowManyOfDecimal++;
		else if (m_strMask[i] == szSuffixChar || m_strMask[i] == szNonEditableSuffix || m_strMask[i] == szSiteCodeChar || _istalpha(m_strMask[i]))
		{
			if (m_nSuffixStart < 0)
				m_nSuffixStart = i;

			if (m_nEditableZoneStart < 0 && m_strMask[i] != szNonEditableSuffix)
				m_nEditableZoneStart = i;

			if (m_strMask[i] == szSiteCodeChar)
				m_HowManyOfSiteCode++;
			else if (m_strMask[i] == szSuffixChar || m_strMask[i] == szNonEditableSuffix)
				nHowManyOfSuffix++;
		}
	}

	m_bIsIrrilevant = m_strMask.IsEmpty() || (m_HowManyOfYear <= 0 && m_HowManyOfNumber <= 0 && nHowManyOfSuffix <= 0 && m_HowManyOfSiteCode <= 0);

	// calcolo la partenza della parte editabile
	if (m_bIsIrrilevant || (!m_HowManyOfNumber && !m_HowManyOfYear)) 
		m_nEditableZoneStart = 0;

	// Quindi la parte finale dell'editing: se start è 0 allora vuol dire che è tutto editabile
	if (m_nEditableZoneStart == 0)
		m_nEditableZoneEnd = m_strMask.GetLength() - 1;
	else
		for (int e=m_nEditableZoneStart + 1; e < m_strMask.GetLength(); e++)
		{
			if (m_strMask[e] == szSuffixChar || m_strMask[e] == szSiteCodeChar || _istalpha(m_strMask[e]))
				m_nEditableZoneEnd = e;
			else
				break;
		}
}

// si occupa di recuperare solo la parte del numero in caso ci sia una
// maschera applicata, skippando parte annuale e suffisso
//----------------------------------------------------------------------------
long CFormatMask::GetNumberFromMask (const CString& sText) const
{
	if (sText.IsEmpty())
		return 0;

	if (m_strMask.IsEmpty() || m_bIsIrrilevant || (!m_HowManyOfYear && sText.GetLength() < m_strMask.GetLength()))
		return _ttol(sText);

	CString strResult;
	for (int i=0; i < m_strMask.GetLength(); i++)
	{
		if (m_strMask[i] == szNumberChar && _istdigit(sText[i]))
			strResult += sText[i];
	}

	return _ttol(strResult);
}

//----------------------------------------------------------------------------
CString	CFormatMask::GetSuffixFromMask (const CString& sText) const
{
	if (m_nEditableZoneStart > 0)
		return sText.Mid(m_nEditableZoneStart, m_nEditableZoneEnd - m_nEditableZoneStart + 1);
	
	if (m_nSuffixStart > 0)
		return sText.Mid(m_nSuffixStart);

	return _T("");
}

//----------------------------------------------------------------------------
void CFormatMask::Assign(const CFormatMask& mask)
{
	m_strMask			= mask.m_strMask;
	m_bIsIrrilevant		= mask.m_bIsIrrilevant;
	m_sSiteCode			= mask.m_sSiteCode;
	m_bEnabled			= mask.m_bEnabled;
	m_nEditableZoneStart= mask.m_nEditableZoneStart;
	m_nEditableZoneEnd	= mask.m_nEditableZoneEnd;
	m_nSuffixStart		= mask.m_nSuffixStart;
	m_HowManyOfYear		= mask.m_HowManyOfYear;
	m_HowManyOfNumber	= mask.m_HowManyOfNumber;
	m_HowManyOfDecimal	= mask.m_HowManyOfDecimal;
	m_HowManyOfSiteCode	= mask.m_HowManyOfSiteCode;
	m_nNumberStartAt	= mask.m_nNumberStartAt;
}

//----------------------------------------------------------------------------
int CFormatMask::Compare(const CFormatMask& mask) const
{
	if (m_strMask		!= mask.m_strMask		)	return 1;
	if (m_bIsIrrilevant	!= mask.m_bIsIrrilevant	)	return 1;
	if (m_sSiteCode		!= mask.m_sSiteCode		)	return 1;
	if (m_bEnabled		!= mask.m_bEnabled		)	return 1;

	if (m_nEditableZoneStart!= mask.m_nEditableZoneStart)	return 1;
	if (m_nEditableZoneEnd	!= mask.m_nEditableZoneEnd)		return 1;
	if (m_nSuffixStart		!= mask.m_nSuffixStart)			return 1;
	if (m_HowManyOfYear			!= mask.m_HowManyOfYear)	return 1;
	if (m_HowManyOfNumber		!= mask.m_HowManyOfNumber)	return 1;
	if (m_HowManyOfDecimal		!= mask.m_HowManyOfDecimal)	return 1;
	if (m_HowManyOfSiteCode		!= mask.m_HowManyOfSiteCode)return 1;
	if (m_nNumberStartAt		!= mask.m_nNumberStartAt)	return 1;

	return 0;
}

//------------------------------------------------------------------------------
const CFormatMask& CFormatMask::operator= (const CFormatMask& mask)
{
	Assign(mask);
	return *this;
}

//============================================================================
//		Class CLongFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CLongFormatter, Formatter)

//----------------------------------------------------------------------------
CLongFormatter::CLongFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	Formatter	(name, aSource, aOwner)
{
	m_OwnType			= DATA_LNG_TYPE;
	m_bEditable			= TRUE;
	m_FormatType		= NUMERIC;
	m_Sign				= MINUSPREFIX;
	m_bZeroPadded		= FALSE;                  
	m_Align				= GetDefaultAlign();
	m_strXTable			= szXlat_413_91;
	m_str1000Separator	= szDefLng1000Sep;
	m_strAsZeroValue	= szZeroAsDash;
	m_bIs1000SeparatorDefault = TRUE;
}

//----------------------------------------------------------------------------
Formatter::AlignType CLongFormatter::GetDefaultAlign	()	const
{
	return RIGHT;
}

//----------------------------------------------------------------------------
void CLongFormatter::Assign(const Formatter& Fmt)
{
	Formatter::Assign(Fmt);

	const CLongFormatter& IF = (const CLongFormatter&)Fmt;

	m_Sign		  				= IF.m_Sign;
	m_bZeroPadded 				= IF.m_bZeroPadded;
	m_strXTable					= IF.m_strXTable;
	m_str1000Separator			= IF.m_str1000Separator;
	m_strAsZeroValue			= IF.m_strAsZeroValue;
	m_FormatType				= IF.m_FormatType;
	m_bIs1000SeparatorDefault	= IF.m_bIs1000SeparatorDefault;
}

//----------------------------------------------------------------------------
BOOL CLongFormatter::IsSameAsDefault() const
{
	CLongFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
int CLongFormatter::Compare(const Formatter& F) const
{
	if (Formatter::Compare(F)) return 1;

	const CLongFormatter& IF = (const CLongFormatter&)F;

	if (m_Sign		  			!= IF.m_Sign			 	)	return 1;
	if (m_bZeroPadded  			!= IF.m_bZeroPadded	 		)	return 1;
	if (m_strXTable 			!= IF.m_strXTable			)	return 1;
	if (m_str1000Separator		!= IF.m_str1000Separator	)	return 1;
	if (m_strAsZeroValue		!= IF.m_strAsZeroValue		)	return 1;
	if (m_FormatType			!= IF.m_FormatType			)	return 1;
	if (m_bIs1000SeparatorDefault	!= IF.m_bIs1000SeparatorDefault)	return 1;

	return 0;
}

//----------------------------------------------------------------------------
void CLongFormatter::Format(const void* p, CString& result, BOOL bPaddingEnabled /*= TRUE*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	result.Empty();
	if(!p)
	{
		ASSERT_TRACE(p != NULL,"Parameter p must not be null");
		return;
	}

	long aValue = *(long*)p;
	BOOL sign	= (aValue >= 0);					//se positivo :TRUE
	BOOL asZero = (m_FormatType == ZERO_AS_DASH && aValue == 0); //se sostituisco null:TRUE
	
	aValue		= (labs(aValue));					//tolgo il segno,lo poi metto a seconda delle richiesta

	TCHAR buff[256];

	switch (m_Sign)									//sign prefix
	{
		case MINUSPREFIX:
		case SIGNPREFIX:
			if (!sign)
			{
				result.Append(szMinus);
			}
			else if(!asZero && m_Sign == SIGNPREFIX)
			{
				result.Append(szPlus); 
			}
			break;
		case ROUNDS:
			if (!sign) result.Append(szOpenedRound); 
			break;		
	}
	switch(m_FormatType)
	{
		case NUMERIC:
		case ZERO_AS_DASH:
			if(asZero)
			{
				result = m_strAsZeroValue;	
			}
			else 
			{	
				result.AppendFormat(_T("%d"), aValue);
				
				if (!m_str1000Separator.IsEmpty())
					FormatHelpers::InsertThousandSeparator(result, (LPCTSTR)m_str1000Separator);					
			}										
			break; 
			
		case LETTER:
			{
				CString strNum;
				FormatHelpers::NumberToWords(aValue, strNum);
				result.Append(strNum);
				break;
			}
		case ENCODED:
			FormatHelpers::NumberToEncoded(aValue, m_strXTable, buff);
			result.Append(buff);
			break;
	}
	switch (m_Sign)								//sign postfix
	{
		case SIGNPOSTFIX:
		case MINUSPOSTFIX:
			if (!sign) 
				result.Append(szMinus); 
			else if(!asZero && m_Sign == SIGNPOSTFIX) 
				result.Append(szPlus);
			break;
		case ROUNDS:
			if (!sign)	
				result.Append(szClosedRound); break;
	}
		
	if(bPaddingEnabled)
	{
		if (!m_bZeroPadded || asZero)
		{
			result.Insert(0, m_strHead);
			result.Append (m_strTail);
			Padder(result, m_Align!=RIGHT);
			return;
		}

		// test di overflow
		if (result.GetLength() + m_strHead.GetLength() + m_strTail.GetLength()> m_nPaddedLen)
		{
			result = TextOverflow(m_nPaddedLen);
			return;
		}

		//paddo con zeri
		int padLen = m_nPaddedLen - result.GetLength() - m_strHead.GetLength() - m_strTail.GetLength();
		if (padLen > 0) 
			result.Insert (0, CString(ZERO_CHAR, padLen));

		result.Insert(0, m_strHead);
		result.Append (m_strTail);
	}
}

//----------------------------------------------------------------------------
CString CLongFormatter::UnFormat(const CString& str) const
{
	CString strValue(str);
	strValue.TrimLeft();
	strValue.TrimRight();

	if (strValue.IsEmpty())
		return strValue;

	switch (m_FormatType)
	{
		case LETTER: 
		case ENCODED: return strValue;
	}

	if (!m_strHead.IsEmpty())
	{
		int nHIdx = strValue.Find(m_strHead) + m_strHead.GetLength();
		if (nHIdx >= m_strHead.GetLength() && nHIdx <= strValue.GetLength())
			strValue = strValue.Mid(nHIdx);
	}

	if (!m_strTail.IsEmpty())
	{
		int	nTIdx = strValue.Find(m_strTail);
		if (nTIdx >= 0)
			strValue = strValue.Left(nTIdx);
	}

	if (m_FormatType == ZERO_AS_DASH && strValue == m_strAsZeroValue)
		return CString(ZERO_CHAR);
	
	if (m_Sign == ROUNDS)
	{
		int i = strValue.Find(szOpenedRound);
		if (i >= 0)
			strValue.SetAt(i, MINUS_CHAR);
	}

	int nLen = strValue.GetLength();

	if	(
			m_Sign == MINUSPOSTFIX	||
			m_Sign == SIGNPOSTFIX	||
			m_Sign == ROUNDS
		)
	{
		int i = nLen - 1;
		if (strValue[i] == MINUS_CHAR)
			strValue = CString(MINUS_CHAR) + strValue.Left(i);
		else
			if (strValue[i] == PLUS_CHAR || strValue[i] == CLOSE_ROUND_CHAR)
				strValue = strValue.Left(i);
	}

	if (m_str1000Separator.IsEmpty())
		return strValue;

	TCHAR* pszValue = strValue.GetBuffer(nLen);

	for (int i = 0; i < nLen; i++)
	{
		while(pszValue[i] == m_str1000Separator[0])
		{
			for (int j = i; j < nLen; j++)
				pszValue[j] = pszValue[j + 1];

			nLen--;
		}
	}

	strValue.ReleaseBuffer(nLen);
	return strValue;
}

//----------------------------------------------------------------------------
void CLongFormatter::RecalcWidths()
{
	DataLng data(LONG_MIN);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
}

//----------------------------------------------------------------------------
const CSize CLongFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/)
{
	return GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), GetDefaultInputString(), TRUE);
}

//----------------------------------------------------------------------------
CString	CLongFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	DataLng data(LONG_MIN);
	CString str;

	FormatDataObj(data, str, FALSE);
	return str;
}

// if (!m_bZeroPadded) m_bZeroPadded = AfxGetCultureInfo()->GetFormatStyleLocale().m_bZeroPadded;
//----------------------------------------------------------------------------
void CLongFormatter::SetToLocale ()
{
	// Attenzione! Il separatore di default del long è "" che sta per No separatore.
	// La sostituzione del default la faccio solo in caso di separatore valorizzato
	// a . (che è il default del double)
	if (_tcsicmp(m_str1000Separator, szDefDbl1000Sep) == 0 && m_bIs1000SeparatorDefault)
		m_str1000Separator = AfxGetCultureInfo()->GetFormatStyleLocale().m_s1000LongSeparator;
}
//------------------------------------------------------------------------------
void CLongFormatter::SerializeJson(CJsonSerializer& strJson)  const
{
	__super::SerializeJson(strJson);
	
	strJson.WriteInt(		szJsonFormatType,			m_FormatType);
	strJson.WriteInt(		szJsonSign,					m_Sign);
	strJson.WriteString(	szJsonThousandSeparator,	m_str1000Separator);
	strJson.WriteString(	szJsonAsZeroValue,			m_strAsZeroValue);
}
//============================================================================
//		Class CIntFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CIntFormatter, CLongFormatter)

//----------------------------------------------------------------------------
CIntFormatter::CIntFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	CLongFormatter (name, aSource, aOwner)
{
	m_OwnType = DATA_INT_TYPE;
}

//----------------------------------------------------------------------------
void CIntFormatter::Format(const void* p, CString& result, BOOL bPaddingEnabled /*= TRUE*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	if(!p)
	{
		ASSERT_TRACE(p != NULL,"Parameter p must not be null");
		return;
	}

	long l = *(short*)p;
	CLongFormatter::Format(&l, result, bPaddingEnabled, bCollateCultureSensitive);
}

//----------------------------------------------------------------------------
void CIntFormatter::RecalcWidths()
{
	DataInt data(SHRT_MIN);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
}

//----------------------------------------------------------------------------
const CSize CIntFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/)
{
	DataInt data(SHRT_MIN);
	CString str;
	FormatDataObj(data, str, FALSE);

	return GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), str, TRUE);
}

//----------------------------------------------------------------------------
CString	CIntFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	DataInt data(SHRT_MIN);
	CString str;

	FormatDataObj(data, str, FALSE);
	return str;
}


//----------------------------------------------------------------------------
BOOL CIntFormatter::IsSameAsDefault() const
{
	CIntFormatter templ;
	return *this == templ;
}

//============================================================================
//		Class CStringFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CStringFormatter, Formatter)

//----------------------------------------------------------------------------
CStringFormatter::CStringFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	Formatter			(name, aSource, aOwner)
{
	m_OwnType		= DATA_STR_TYPE;
	m_bEditable		= TRUE;
	m_FormatType	= ASIS;
	m_Align			= GetDefaultAlign();
	m_strInterChars	= BLANK_CHAR;
}

//----------------------------------------------------------------------------
Formatter::AlignType CStringFormatter::GetDefaultAlign	()	const
{
	return LEFT;
}

//----------------------------------------------------------------------------
void CStringFormatter::Assign(const Formatter& Fmt)
{
	Formatter::Assign(Fmt);

	const CStringFormatter& SF = (const CStringFormatter&)Fmt;
	m_strInterChars = SF.m_strInterChars;
	m_FormatType	= SF.m_FormatType;
	m_FormatMask	= SF.m_FormatMask;
}

//----------------------------------------------------------------------------
BOOL CStringFormatter::IsSameAsDefault() const
{
	CStringFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
int CStringFormatter::Compare(const Formatter& F) const
{
	if (Formatter::Compare(F)) return 1;

	const CStringFormatter& SF = (const CStringFormatter&)F;
	if (m_strInterChars		!= SF.m_strInterChars		)	return 1;
	if (m_FormatType		!= SF.m_FormatType			)	return 1;
	if (m_FormatMask		!= SF.m_FormatMask			)	return 1;

	return 0;
}

//----------------------------------------------------------------------------
void CStringFormatter::Format(const void* p, CString& result, BOOL bPaddingEnabled /*= TRUE*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	result.Empty();
	if(!p)
	{
		ASSERT_TRACE(p != NULL,"Parameter p must not be null");
		return;
	}

	int nLen = _tcslen((TCHAR*)p);
	TCHAR* aValue = new TCHAR [nLen + 1];
	TB_TCSCPY (aValue, (TCHAR*)p);
	CString str(aValue);

	switch (m_FormatType)
	{
		case ASIS:
			result.Append(aValue);
			break;
		case UPPERCASE:
			if (bCollateCultureSensitive)
				result.Append(AfxGetCultureInfo()->GetUpperCase(str));
			else
			{
				_tcsupr_s(aValue, nLen + 1);
				result.Append(aValue);
			}
			break;
		case LOWERCASE:
			if (bCollateCultureSensitive)
				result.Append(AfxGetCultureInfo()->GetLowerCase(str));
			else
			{
				_tcslwr_s(aValue,nLen + 1);
				result.Append(aValue);
			}
			break;
		case CAPITALIZED:
			{
				TCHAR* pCh = aValue;
				if (pCh && *pCh)
				{
 					*pCh = _T_TOUPPER(*pCh);	
					while (pCh[1])
					{
						if (!_istalpha(pCh[0]) && _istalpha(pCh[1]))
							pCh[1] = _T_TOUPPER(pCh[1]);
						pCh++;
					}
				}
				result.Append(aValue);
				break;
			}
		case MASKED:
			result = m_FormatMask.ApplyMask(str, m_bZeroPadded);
			break;
		case EXPANDED:
			int nLen = _tcslen(aValue);
			for(int i = 0; i < nLen; i++)
			{
				result += aValue[i];
				if (i < (nLen - 1)) 
					result += m_strInterChars;
			}
			break;
	}

	delete [] aValue;

	if (bPaddingEnabled)
	{
		result.Insert(0, m_strHead);
		result.Append (m_strTail);
		Padder(result, m_Align!=RIGHT);		
	}
}
//----------------------------------------------------------------------------
const CSize CStringFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/)
{
	if (nCols > 15)
		return ::GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), nCols, TRUE).cx;

	// Tapullo per gestire i control che contengono delle stringhe corte
	DataStr data(CString(nCols <= 3 ? _T('W') : _T('8'), nCols));
	
	CString str;
	FormatDataObj(data, str, FALSE);

	return ::GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), str, TRUE);
}

//----------------------------------------------------------------------------
CString	CStringFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	if (pDataObj == NULL)
		return _T("");
	int nCols = pDataObj->GetColumnLen();

	DataStr data(CString(nCols <= 3 ? _T('W') : _T('8'), nCols));
	
	CString str;
	FormatDataObj(data, str, FALSE);

	return str;
}

//----------------------------------------------------------------------------
void CStringFormatter::SetMask (const CString& strMask)
{
	m_FormatMask.SetMask(strMask);
}

//----------------------------------------------------------------------------
BOOL CStringFormatter::ZeroPaddedHasUI () const
{
	return CanAttachToData() && m_FormatType == MASKED;
}


//============================================================================
//		Class CTextFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTextFormatter, CStringFormatter)

//----------------------------------------------------------------------------
CTextFormatter::CTextFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	CStringFormatter	(name, aSource, aOwner)
{
	m_OwnType		= DATA_TXT_TYPE;
	m_bEditable		= TRUE;
	m_Align			= GetDefaultAlign();
	m_strInterChars	= BLANK_CHAR;
}

//----------------------------------------------------------------------------
void CTextFormatter::Assign(const Formatter& Fmt)
{
	CStringFormatter::Assign(Fmt);
}

//----------------------------------------------------------------------------
BOOL CTextFormatter::IsSameAsDefault() const
{
	CTextFormatter templ;
	return *this == templ;
}



//============================================================================
//		Class CDblFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDblFormatter, Formatter)

//----------------------------------------------------------------------------
CDblFormatter::CDblFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	Formatter (name, aSource, aOwner)
{
	m_OwnType	= DATA_DBL_TYPE;
	m_bEditable	= TRUE;
	m_FormatType = FIXED;
	m_Rounding  = ROUND_NONE;
	m_Sign		= MINUSPREFIX;
	m_nQuantum	= 0.;
	m_Align		= GetDefaultAlign();

	m_str1000Separator	= szDefDbl1000Sep;
	m_strDecSeparator	= szDefDecSeparator;
	m_bShowMSZero		= m_bShowLSZero = TRUE;
	m_strXTable			= szXlat_413_91_x;
	m_nDecNumber		= nDefDecimals;
	m_strAsZeroValue	= szZeroAsDash;
	
	m_bIs1000SeparatorDefault	= TRUE;
	m_bIsDecSeparatorDefault	= TRUE;
}

//----------------------------------------------------------------------------
Formatter::AlignType CDblFormatter::GetDefaultAlign	()	const
{
	return RIGHT;
}

//----------------------------------------------------------------------------
void CDblFormatter::Assign(const Formatter& Fmt)
{
	Formatter::Assign(Fmt);

	const CDblFormatter& RF = (const CDblFormatter&)Fmt;

	m_Rounding					= RF.m_Rounding;
	m_Sign						= RF.m_Sign;
	m_nQuantum					= RF.m_nQuantum;
	m_str1000Separator			= RF.m_str1000Separator;
	m_strDecSeparator			= RF.m_strDecSeparator;
	m_bShowMSZero				= RF.m_bShowMSZero;
	m_bShowLSZero				= RF.m_bShowLSZero;
	m_strXTable					= RF.m_strXTable;
	m_nDecNumber				= RF.m_nDecNumber;
	m_strAsZeroValue			= RF.m_strAsZeroValue;	
	m_FormatType				= RF.m_FormatType;
	m_bIs1000SeparatorDefault	= RF.m_bIs1000SeparatorDefault;
	m_bIsDecSeparatorDefault	= RF.m_bIsDecSeparatorDefault;
}

//----------------------------------------------------------------------------
BOOL CDblFormatter::IsSameAsDefault() const
{
	CDblFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
int CDblFormatter::Compare(const Formatter& F) const
{
	if (Formatter::Compare(F)) return 1;

	const CDblFormatter& DF = (const CDblFormatter&)F;

	if (m_Rounding			!= DF.m_Rounding		)	return 1;
	if (m_Sign			  	!= DF.m_Sign			)	return 1;
	if (m_nQuantum		  	!= DF.m_nQuantum		)	return 1;
	if (m_str1000Separator	!= DF.m_str1000Separator)	return 1;
	if (m_strDecSeparator  	!= DF.m_strDecSeparator	)	return 1;
	if (m_bShowMSZero	  	!= DF.m_bShowMSZero		)	return 1;
	if (m_bShowLSZero	  	!= DF.m_bShowLSZero		)	return 1;
	if (m_strXTable			!= DF.m_strXTable		)	return 1;
	if (m_nDecNumber	  	!= DF.m_nDecNumber		)	return 1;
	if (m_strAsZeroValue	!= DF.m_strAsZeroValue	)	return 1;
	if (m_FormatType		!= DF.m_FormatType		)	return 1;

	if (m_bIs1000SeparatorDefault != DF.m_bIs1000SeparatorDefault)	return 1;
	if (m_bIsDecSeparatorDefault != DF.m_bIsDecSeparatorDefault )	return 1;

	return 0;
}

//----------------------------------------------------------------------------
double CDblFormatter::GetRoundedValue(double dValue, double dQuantum) const
{
	if (dQuantum)        
		switch (m_Rounding)
		{
			case ROUND_ABS:
				return FormatHelpers::Round(dValue, dQuantum, FormatHelpers::Absolute);
			case ROUND_SIGNED:
				return FormatHelpers::Round(dValue, dQuantum, FormatHelpers::Signed);
			case ROUND_ZERO:              
				return FormatHelpers::Round(dValue, dQuantum, FormatHelpers::Zero);
			case ROUND_INF:
				return FormatHelpers::Round(dValue, dQuantum, FormatHelpers::Infinite);
		}

	return dValue;
}

//----------------------------------------------------------------------------
double CDblFormatter::GetRoundedValue(double d) const
{
	return GetRoundedValue(d, m_nQuantum);
}

// It calculates the quantum value for a specified number of decimal
//----------------------------------------------------------------------------
double CDblFormatter::GetQuantumFromDecNumber(const int& nDec) const
{
	if (nDec <= 0)
		return 1.0;

	double dQuantum = 0.5;

	if (m_Rounding == FormatHelpers::Absolute || m_Rounding == FormatHelpers::Infinite)
		dQuantum = 0.1;
	else if (m_Rounding == FormatHelpers::Zero)
		dQuantum = 0.0;

	for (int i=0; i < nDec; i++) 
		dQuantum = dQuantum / 10.0;

	return dQuantum;
}

//----------------------------------------------------------------------------
void CDblFormatter::Format(const void* p, CString& result, BOOL bPaddingEnabled /*= TRUE*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	result.Empty();
	if(!p)
	{
		ASSERT_TRACE(p != NULL,"Parameter p must not be null");
		return;
	}
	
	double aValue = *(double*)p;
	double aValueCopy = *(double*)p;
	
	long double ldDec;
	long double ldValue = aValue;
	long double ldInt = modf(aValue, &ldDec);
	long double exp	= 1000000000000.0;
	ldDec = floor(ldDec * exp)/exp;
	aValue = (double) (ldDec + ldInt);

	// corretta an. #11.585. Quando arrivano alla Format dei valori esponenziali
	// la funzione _fcvt, che serve a formattare in stringa il risultato, ritorna
	// una formattazione differente da quella impostata nel formattatore (0.00000). 
	// In questo caso, è preferibile ripulire il double di origine n modo che la 
	// ValueToAppend() sia in grado poi di formattare secondo le corrette regole 
	// definite dal formattatore per i campi 0.0.
	int posDec		= 0;
	int mantSign	= 0;
	CString str;

	{
		char szBuffer [128];
		_fcvt_s(szBuffer, 128, aValue, m_nDecNumber, &posDec, &mantSign);
		str = szBuffer;
	}

	if (str.IsEmpty())
		aValue = 0.0;

	//tronco il value che mi arriva a seconda dei numeri decimali richiesti senza nessun arrotondamento
	if (fabs(aValue) < ::GetEpsilonForDataType(m_OwnType))
		aValue=0.0;    

	BOOL asZero = (m_FormatType == ZERO_AS_DASH && aValue == 0);	//se sostituisco null:TRUE
	BOOL sign	= (aValue >= 0);									//se positivo :true
	aValue		= fabs(aValue);										//tolgo il segno,lo poi metto a seconda della richiesta

	aValue = GetRoundedValue (aValue);								//arrotondamento richiesto

	switch (m_Sign)								//sign prefix
	{
		case SIGNPREFIX:
		case MINUSPREFIX:
			if (!sign)
				result.Append(szMinus); 
			else if(!asZero && m_Sign == SIGNPREFIX)
				result.Append(szPlus); 
			break;
		case ROUNDS:
			if (!sign) 
				result.Append(szOpenedRound); 
			break;		
	}

	CString valueResult;
	const rsize_t nBuf = 2;
	switch(m_FormatType)
	{
		case FIXED:
		case ZERO_AS_DASH:
			if (asZero && bPaddingEnabled)
				result.Append(m_strAsZeroValue);
			else		
				result.Append(ValueToAppend(aValue,FIXED));
			break;
		case LETTER:
			{
				CString strNum;
				//Per problemi di arrotondamento, es. 2.9999999998 veniva convertito in due/00
				DataDbl aValueDbl(aValueCopy);
				aValueDbl.Round(m_nDecNumber);
				if (!FormatHelpers::NeedDecimalConversionInLetter())
				{
					FormatHelpers::NumberToWords(floor(aValueDbl), strNum);
					result.Append(strNum);
					result.Append(ValueToAppend(aValue,LETTER));
				}
				else //conversione della parte decimale in lettere
				{
					if (floor(aValueDbl) != 0)
					{
						FormatHelpers::NumberToWords(floor(aValueDbl), strNum);
						result.Append(strNum);
					}
					if (m_nDecNumber > 0)
					{
						CString strDec;
						FormatHelpers::DecimalToWords(aValueDbl, strDec);
						result.Append(strDec);
					}
				}
				break;
			}
		case ENCODED:
				result.Append(ValueToAppend(aValue,ENCODED));
				//replace dei numeri e del decseparator
				
				TCHAR ch[nBuf];
				for(int x = 0; x < m_strXTable.GetLength()-1; x++)
				{					
					_itot_s(x, ch, nBuf, 10);
					result.Replace(ch[0], m_strXTable[x]);
				}
				result.Replace(m_strDecSeparator[0], m_strXTable[m_strXTable.GetLength()-1]);
				break; 
		case EXPONENTIAL:
			valueResult = ValueToAppend(aValue,EXPONENTIAL);
			//lo zero iniziale lo mette di default, se richiesto viene eliminato
			if (valueResult[0] == ZERO_CHAR && !m_bShowMSZero) 
				result.Append(valueResult.Mid(1));
			else 
				result.Append(valueResult);
			break;
		case ENGINEER:
			result.Append(ValueToAppend(aValue,ENGINEER));
			break;
	} 

	switch (m_Sign)								//sign postfix
	{
		case SIGNPOSTFIX:
		case MINUSPOSTFIX:
			if (!sign)
				result.Append(szMinus); 
			else if(!asZero && m_Sign == SIGNPOSTFIX)
				result.Append(szPlus);
			break;
		case ROUNDS:
			if (!sign)	
				result.Append(szClosedRound);
			break;
	} 
	
	if(bPaddingEnabled)
	{
		result.Insert(0, m_strHead);
		result.Append (m_strTail);
		Padder(result, m_Align!=RIGHT);
	}
}
/*
//----------------------------------------------------------------------------
CString CDblFormatter::ValueToAppend(double aValue, FormatTag format) const
{
	//uso la mia personalizzazione di formattazione

	CString strNumberDecimalSeparator, strNumberGroupSeparator;

	if (!m_strDecSeparator.IsEmpty() && m_strDecSeparator != BLANK_CHAR)  
		strNumberDecimalSeparator = m_strDecSeparator;
	
	//se i due separatori sono uguali tolgo quello delle migliaia
	if (!m_str1000Separator.IsEmpty() && m_str1000Separator != m_strDecSeparator)	
		strNumberGroupSeparator = m_str1000Separator;

	double intPart;
	double decPart = modf(aValue, &intPart); 
	CString strInteger, strDecimal;

	CString strFormatter;
	strFormatter.Format(_T("%%.%df"), m_nDecNumber);
	strDecimal.Format(strFormatter, decPart);
	if(m_nDecNumber)
	{
		if (strDecimal.Mid(decPart > 0 ? 0 : 1, 1) == '1') // recupero overflow sulle unita
			intPart += (decPart > 0 ? 1 : -1);
		strDecimal = strDecimal.Mid(decPart >= 0 ? 2 : 3); //tolgo '0.' oppure -0. dalla stringa
	}
	else
		strDecimal.Empty();

	strInteger.Format(_T("%.0f"), intPart);
	....

*/
//----------------------------------------------------------------------------
CString CDblFormatter::ValueToAppend(double aValue, FormatTag format) const
{
	CString strNumberDecimalSeparator, strNumberGroupSeparator;

	if (!m_strDecSeparator.IsEmpty())  
		strNumberDecimalSeparator = m_strDecSeparator;
	
	//se i due separatori sono uguali tolgo quello delle migliaia
	if (!m_str1000Separator.IsEmpty() && m_str1000Separator != m_strDecSeparator)	
		strNumberGroupSeparator = m_str1000Separator;

	CString strInteger, strDecimal, strValue;
	int posDec = 0;
	int mantSign = 0;

	{
		char szBuffer [128];
		_fcvt_s(szBuffer, 128, aValue, m_nDecNumber, &posDec, &mantSign);
		strValue = szBuffer;
	}

	if (posDec == 0)
	{
		strDecimal = strValue;
		strInteger =  _T("0");
	}
	else if (posDec < 0)
	{
		strDecimal.Append(CString('0', -posDec));
		strDecimal.Append(strValue);
		strInteger =  _T("0");
	}
	else //if (posDec > 0)
	{
		strDecimal = strValue.Mid(posDec);
		strInteger = strValue.Left(posDec);
	}

	if (mantSign)
		strInteger =  _T("-") + strInteger;

	if (m_nDecNumber == 0)
		strDecimal.Empty();

	// non tolgo gli zeri finali, li sostituisco
	// perchè devo tenere conto degli spazi in allineamento
	if (!m_bShowLSZero)
		for (int i = strDecimal.GetLength()-1; i >= 0; i--)
		{
			if (strDecimal[i] != ZERO_CHAR)
				break;
			strDecimal.SetAt(i, ' ');
		}
	
	CString strResult;
	switch(format)
	{	
		case FIXED:
		case ENCODED:
			FormatHelpers::InsertThousandSeparator (strInteger, strNumberGroupSeparator);		
			if (m_bShowMSZero || _tstoi(strInteger))
				strResult += strInteger;
			
			// devo usare gli spazi per allineare bene
			if (m_nDecNumber)
				strResult += m_bShowLSZero || !strDecimal.Trim().IsEmpty() ? strNumberDecimalSeparator : _T(" ");

			strResult += strDecimal;
			return strResult;

		case LETTER:						//considero solo la parte decimale
			strResult += m_bShowLSZero || !strDecimal.Trim().IsEmpty() ? strNumberDecimalSeparator : _T(" ");
			strResult += strDecimal;
			return strResult;
			
		case EXPONENTIAL:					//non considero thSep
			{
				CString strFormatter;
				strFormatter.Format(_T("%%.%dE"), m_nDecNumber);
				strResult.Format(strFormatter, aValue);
				
				// tolgo zeri iniziale e finale (se devo farlo)
				if(!m_bShowMSZero)
					strResult.TrimLeft(ZERO_CHAR);

				if(!m_bShowLSZero)
				{
					int nPos = strResult.Find(szE);
					CString subStr = strResult.Left(nPos);
					subStr.TrimRight (ZERO_CHAR);
					strResult = subStr + strResult.Mid (nPos);
				}

				// sostituisco il corretto decimal separator
				int nPos = 0, nLength = strResult.GetLength();
				while (_istdigit(strResult[nPos]) && nPos<nLength)
					nPos++;
				
				// se potenzialmente sto producendo '.E', rimuovo il carattere '.'; altrimenti imposto 
				// il corretto decimal separator
				if(m_nDecNumber)
				{
					if(strResult[nPos+1] == EXP_CHAR)
						strResult.Delete(nPos);
					else
						strResult.SetAt(nPos, m_strDecSeparator[0]);
				}

				// la formattazione produce 'E[sign]ddd', non devono essere visualizzati gli zeri iniziali
				// (coerenza con la versione precedente), eventualmente ne deve rimanere solo uno
				int nPosZ = strResult.Find(szE) + 1;
				for (int i = 0; i < 2; i++)
				{
					if (!_istdigit(strResult[nPosZ]))	
					{
						nPosZ++;
						i--;
					}
					else if(strResult[nPosZ] == ZERO_CHAR)
						strResult.Delete(nPosZ);
					else 
						break;
				}

				return strResult;
			}

		case ENGINEER:		//non considero thSep
			{
				int		cont	= 0;
			
				//per valori >1 e =0, l'esponente e' positivo
				if (aValue >= 1.0 || aValue == 0.0)	
				{
					while (aValue >= 1000)				
					{
						aValue /= 1000;
						cont++;
					}
				}
				//per valori compresi tra zero e uno, l'esponente e'negativo
				else 
					while(aValue < 1.0)
						{aValue *= 1000; cont--;}

				double expE = pow((double)10, (double)m_nDecNumber);
				//tronco il numero alle cifre decimali richieste
				aValue = floor(aValue * expE) / expE;
				
				CString strFormatter;
				strFormatter.Format(_T("%%.%df"), m_nDecNumber);
				strResult.Format(strFormatter, aValue);

				if(!m_bShowLSZero)
					strResult.TrimLeft(ZERO_CHAR);
				
				if(!m_bShowLSZero)
					strResult.TrimRight(ZERO_CHAR);
				
				// sostituisco il corretto decimal separator
				int nPos = 0, nLength = strResult.GetLength();
				while (_istdigit(strResult[nPos]) && nPos<nLength)
					nPos++;
				
				// se potenzialmente sto producendo '.E', rimuovo il carattere '.'; altrimenti imposto 
				// il corretto decimal separator
				if(m_nDecNumber)
				{
					if(nPos+1 == nLength)
						strResult.Delete(nPos);
					else
						strResult.SetAt(nPos, m_strDecSeparator[0]);
				}

				strResult += szE;
				
				CString strPot;
				strPot.Format (_T("%+d"), cont*3);
				strResult += strPot;

				return strResult;
			}
		}
		
		return _T("");
}

//----------------------------------------------------------------------------
CString CDblFormatter::UnFormat(const CString& str) const
{
	CString strValue(str);
	strValue.TrimLeft();
	strValue.TrimRight();

	if (strValue.IsEmpty())
		return strValue;

	switch (m_FormatType)
	{
		case LETTER: 
		case ENCODED: return strValue;
	}

	if (!m_strHead.IsEmpty())
	{
		int nHIdx = strValue.Find(m_strHead) + m_strHead.GetLength();
		if (nHIdx >= m_strHead.GetLength() && nHIdx <= strValue.GetLength())
			strValue = strValue.Mid(nHIdx);
	}

	if (!m_strTail.IsEmpty())
	{
		int	nTIdx = strValue.Find(m_strTail);
		if (nTIdx >= 0)
			strValue = strValue.Left(nTIdx);
	}

	if (m_FormatType == ZERO_AS_DASH && strValue == m_strAsZeroValue)
		return CString(ZERO_CHAR);
	
	if (m_Sign == ROUNDS)
	{
		int i = strValue.Find(szOpenedRound);
		if (i >= 0)
			strValue.SetAt(i, MINUS_CHAR);
	}

	int nLen = strValue.GetLength();

	if	(
			m_Sign == MINUSPOSTFIX	||
			m_Sign == SIGNPOSTFIX	||
			m_Sign == ROUNDS
		)
	{
		int i = nLen - 1;
		if (strValue[i] == MINUS_CHAR)
			strValue = MINUS_CHAR + strValue.Left(i);
		else
			if (strValue[i] == PLUS_CHAR || strValue[i] == CLOSE_ROUND_CHAR)
				strValue = strValue.Left(i);
	}

	TCHAR* pszValue = strValue.GetBuffer(nLen);

	for (int i = 0; i < nLen; i++)
	{
		if (!m_str1000Separator.IsEmpty())
			while(pszValue[i] == m_str1000Separator[0])
			{
				for (int j = i; j < nLen; j++)
					pszValue[j] = pszValue[j + 1];

				nLen--;
			}

		// sostituisce con il punto il separatore dei decimali	
		if (!m_strDecSeparator.IsEmpty() && pszValue[i] == m_strDecSeparator[0])
			pszValue[i] = DOT_CHAR;
	}

	strValue.ReleaseBuffer(nLen);
	return strValue;
}

//----------------------------------------------------------------------------
void CDblFormatter::RecalcWidths()
{
	DataDbl data(-999999999999999.0);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
}

//----------------------------------------------------------------------------
const CSize CDblFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/)
{
	return GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), GetDefaultInputString(), TRUE);
}

//----------------------------------------------------------------------------
CString CDblFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	DataDbl data(-999999999999999.0);
	CString str;
	FormatDataObj(data, str, FALSE);
	return str;
}

/*	if (!m_bZeroPadded) m_bZeroPadded	= AfxGetCultureInfo()->GetFormatStyleLocale().m_bZeroPadded;
	if (m_nDecNumber)	m_nDecNumber	= AfxGetCultureInfo()->GetFormatStyleLocale().m_nDecimals; */
//----------------------------------------------------------------------------
void CDblFormatter::SetToLocale ()
{
	if (m_bIsDecSeparatorDefault)
		m_strDecSeparator = AfxGetCultureInfo()->GetFormatStyleLocale().m_sDecSeparator;

	if (m_bIs1000SeparatorDefault)
		m_str1000Separator = AfxGetCultureInfo()->GetFormatStyleLocale().m_s1000DoubleSeparator;
}

void CDblFormatter::SerializeJson(CJsonSerializer& strJson)  const
{
	__super::SerializeJson(strJson);
	
	strJson.WriteInt(		szJsonFormatType, m_FormatType);
	strJson.WriteInt(		szJsonSign, m_Sign);
	strJson.WriteString(	szJsonThousandSeparator, m_str1000Separator);
	strJson.WriteString(	szJsonAsZeroValue, m_strAsZeroValue);
	strJson.WriteString(	szJsonDecimalSeparator, m_strDecSeparator);
	strJson.WriteInt(		szJsonRoundingMode, m_Rounding);
	strJson.WriteDouble(	szJsonRoundingQuantum , m_nQuantum);
	strJson.WriteInt(		szJsonDecimals , m_nDecNumber);
}
//============================================================================
//		Class CMonFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CMonFormatter, CDblFormatter)

//----------------------------------------------------------------------------
CMonFormatter::CMonFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	CDblFormatter (name,aSource, aOwner)
{
	m_OwnType		= DATA_MON_TYPE;
	m_nDecNumber	= 2;
}

//----------------------------------------------------------------------------
BOOL CMonFormatter::IsSameAsDefault() const
{
	CMonFormatter templ;
	return *this == templ;
}


//============================================================================
//		Class CQtaFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CQtaFormatter, CDblFormatter)

//----------------------------------------------------------------------------
CQtaFormatter::CQtaFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	CDblFormatter (name, aSource, aOwner)
{
	m_OwnType = DATA_QTA_TYPE;
}

//----------------------------------------------------------------------------
BOOL CQtaFormatter::IsSameAsDefault() const
{
	CQtaFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
void CQtaFormatter::RecalcWidths()
{
	DataQty data(-9999999.0);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
}

//----------------------------------------------------------------------------
const CSize CQtaFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/) 
{
	return GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), GetDefaultInputString(), TRUE);
}

//----------------------------------------------------------------------------
CString CQtaFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	DataQty data(-9999999.0);
	CString str;
	FormatDataObj(data, str, FALSE);
	return str;
}

//		Class CPercFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CPercFormatter, CDblFormatter)

//----------------------------------------------------------------------------
CPercFormatter::CPercFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	CDblFormatter (name, aSource, aOwner)
{
	m_OwnType	 = DATA_PERC_TYPE;
}

//----------------------------------------------------------------------------
BOOL CPercFormatter::IsSameAsDefault() const
{
	CPercFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
void CPercFormatter::RecalcWidths()
{
	DataPerc data(-99999.0);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
}

//----------------------------------------------------------------------------
const CSize CPercFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/) 
{
	return GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), GetDefaultInputString(), TRUE);
}

//----------------------------------------------------------------------------
CString	CPercFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	DataPerc data(-99999.0);
	CString str;
	FormatDataObj(data, str, FALSE);
	return str;
}

//============================================================================
//		Class CDateFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDateFormatter, Formatter)

//----------------------------------------------------------------------------
CDateFormatter::CDateFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
    Formatter	(name, aSource, aOwner)
{
	m_OwnType			= DATA_DATE_TYPE;
	m_bEditable			= TRUE;
	m_Align				= GetDefaultAlign();
	m_FormatType		= CDateFormatHelper::DATE_DMY;
	m_WeekdayFormat		= CDateFormatHelper::NOWEEKDAY;
	m_DayFormat			= CDateFormatHelper::DAY99;
	m_MonthFormat		= CDateFormatHelper::MONTH99;
	m_YearFormat		= CDateFormatHelper::YEAR99;
	m_strFirstSeparator	= szDefDateSep;
	m_strSecondSeparator= szDefDateSep;
	m_nInputDateLen		= 0;

	m_TimeFormat		= CDateFormatHelper::TIME_NONE;
	m_strTimeSeparator	= szDefTimeSep;
	m_strTimeAM			= szDefTimeAM;
	m_strTimePM			= szDefTimePM;

	m_bIsFormatTypeDefault = TRUE;
	m_bIsFormatYearDefault = TRUE;
	m_bIsTimeFormatDefault = TRUE;
}

//----------------------------------------------------------------------------
Formatter::AlignType CDateFormatter::GetDefaultAlign	()	const
{
	return RIGHT;
}

//----------------------------------------------------------------------------
void CDateFormatter::Assign(const Formatter& Fmt)
{
	Formatter::Assign(Fmt);

	const CDateFormatter& DF = (const CDateFormatter&)Fmt;

    m_WeekdayFormat		= DF.m_WeekdayFormat;
	m_DayFormat			= DF.m_DayFormat;
	m_MonthFormat		= DF.m_MonthFormat;
	m_YearFormat		= DF.m_YearFormat;
	m_strFirstSeparator	= DF.m_strFirstSeparator;
	m_strSecondSeparator= DF.m_strSecondSeparator;

	m_nInputDateLen		= DF.m_nInputDateLen;
	m_TimeFormat		= DF.m_TimeFormat;
	m_strTimeSeparator	= DF.m_strTimeSeparator;
	m_strTimeAM			= DF.m_strTimeAM;
	m_strTimePM			= DF.m_strTimePM;
	m_FormatType		= DF.m_FormatType;
	
	m_bIsFormatTypeDefault	= DF.m_bIsFormatTypeDefault;
	m_bIsFormatYearDefault	= DF.m_bIsFormatYearDefault;
	m_bIsTimeFormatDefault	= DF.m_bIsTimeFormatDefault;
}

//----------------------------------------------------------------------------
BOOL CDateFormatter::IsSameAsDefault() const
{
	CDateFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
int CDateFormatter::Compare(const Formatter& F) const
{
	if (Formatter::Compare(F)) return 1;

	const CDateFormatter& DF = (const CDateFormatter&)F;

	if (m_WeekdayFormat		!= DF.m_WeekdayFormat		)	return 1;
	if (m_DayFormat			!= DF.m_DayFormat			)	return 1;
	if (m_MonthFormat		!= DF.m_MonthFormat			)	return 1;
	if (m_YearFormat		!= DF.m_YearFormat			)	return 1;
	if (m_strFirstSeparator	!= DF.m_strFirstSeparator	)	return 1;
	if (m_strSecondSeparator!= DF.m_strSecondSeparator	)	return 1;

	if (m_TimeFormat		!= DF.m_TimeFormat			)	return 1;
	if (m_strTimeSeparator	!= DF.m_strTimeSeparator	)	return 1;
	if (m_strTimeAM			!= DF.m_strTimeAM			)	return 1;
	if (m_strTimePM			!= DF.m_strTimePM			)	return 1;
	if (m_FormatType		!= DF.m_FormatType			)	return 1;

	if (m_bIsFormatTypeDefault != DF.m_bIsFormatTypeDefault)	return 1;
	if (m_bIsFormatYearDefault != DF.m_bIsFormatYearDefault)	return 1;
	if (m_bIsTimeFormatDefault != DF.m_bIsTimeFormatDefault)	return 1;
	
	return 0;
}

//----------------------------------------------------------------------------
void CDateFormatter::AppendFormatString (CDateFormatHelper::DayFormatTag format, CString& strWork, BOOL bTypeShort) const
{
	switch(format)
	{
		case CDateFormatHelper::DAY9:
		
			strWork.Append(_T("%#d"));
			return;
		

		case CDateFormatHelper::DAYB9:
			if(bTypeShort)
			{
				strWork.Append(_T(" %#d"));
				return;
			}	
	}
	strWork.Append (_T("%d"));
}

//----------------------------------------------------------------------------
void CDateFormatter::AppendFormatString (CDateFormatHelper::MonthFormatTag format, CString& strWork, BOOL bTypeShort, const DataDate &aDate) const
{
	switch (format)
	{
		case CDateFormatHelper::MONTH9: 
			strWork.Append (_T("%#m"));
			return;
		case CDateFormatHelper::MONTH99:
			strWork.Append (_T("%m"));
			return;
		case CDateFormatHelper::MONTHB9: 
			if (bTypeShort)	
				strWork.Append (_T(" %#m"));
			else			
				strWork.Append (_T("%m"));
			return;
		case CDateFormatHelper::MONTHS3:
			strWork.Append (aDate.ShortMonthName());
			return;
		case CDateFormatHelper::MONTHSX:
			strWork.Append (aDate.MonthName());
			return;
	}
	return;
}

//----------------------------------------------------------------------------
void CDateFormatter::AppendFormatString (CDateFormatHelper::YearFormatTag format, CString &strWork, const int& nCurrentYear) const
{
	switch (format)
	{
			case CDateFormatHelper::YEAR99:
				if	(
						((nCurrentYear / 100) == 19 && (nCurrentYear % 100) >= 30) ||
						((nCurrentYear / 100) == 20 && (nCurrentYear % 100) < 30)
					)
				{
					strWork.Append(_T("%y"));
					return;
				}
			case CDateFormatHelper::YEAR999:
				strWork.Append(szYearLocalizationChars);
				strWork.Append(_T("%Y"));
				return;
			case CDateFormatHelper::YEAR9999:
				strWork.Append(_T("%Y"));
				return;
	}

	return;
}
//----------------------------------------------------------------------------
void CDateFormatter::Format(const void* p, CString& result, BOOL bPaddingEnabled /*= TRUE*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	result.Empty();
	if(!p)
	{
		ASSERT_TRACE(p != NULL,"Parameter p must not be null");
		return;
	}

	DataDate aValue(*((DBTIMESTAMP*) p));
	if (aValue.IsEmpty())
        return;

	CString strFormatter;

	//formattazione data se ricevo la richiesta di un Date o di un DateTime
	if ((m_TimeFormat & CDateFormatHelper::TIME_ONLY) != CDateFormatHelper::TIME_ONLY)
	{	
		CString strFirstSeparator = m_strFirstSeparator;
		if(strFirstSeparator == _T('%'))
			strFirstSeparator += _T('%');
		
		CString strSecondSeparator = m_strSecondSeparator;
		if(strSecondSeparator == _T('%'))
			strSecondSeparator += _T('%');

		BOOL dayShort   = (aValue.Day()   < 10);
		BOOL monthShort = (aValue.Month() < 10);
		if (m_WeekdayFormat == CDateFormatHelper::PREFIXWEEKDAY)
		{
			strFormatter.Append(aValue.WeekDayName());
			strFormatter.Append(_T(" "));
		}
		switch (m_FormatType)
		{
			case CDateFormatHelper::DATE_DMY:
				AppendFormatString(m_DayFormat, strFormatter, dayShort);
				strFormatter.Append(strFirstSeparator);
				AppendFormatString(m_MonthFormat, strFormatter, monthShort, aValue);
				strFormatter.Append(strSecondSeparator);
				AppendFormatString(m_YearFormat, strFormatter, aValue.Year());
				break;
			case CDateFormatHelper::DATE_MDY:
				AppendFormatString(m_MonthFormat, strFormatter, monthShort, aValue);	
				strFormatter.Append(strFirstSeparator);
				AppendFormatString(m_DayFormat, strFormatter, dayShort);
				strFormatter.Append(strSecondSeparator);
				AppendFormatString(m_YearFormat, strFormatter, aValue.Year());
				break;
			case CDateFormatHelper::DATE_YMD:
				AppendFormatString(m_YearFormat, strFormatter, aValue.Year());
				strFormatter.Append(strFirstSeparator);
				AppendFormatString(m_MonthFormat, strFormatter, monthShort, aValue);
				strFormatter.Append(strSecondSeparator);
				AppendFormatString(m_DayFormat, strFormatter, dayShort);
				break;
		}

		if (m_WeekdayFormat == CDateFormatHelper::POSTFIXWEEKDAY)
		{
			strFormatter.Append(_T(" "));
			strFormatter.Append(aValue.WeekDayName());
		}
	
		if (m_TimeFormat != CDateFormatHelper::TIME_NONE)
			strFormatter += BLANK_CHAR;
	}
	
	//formattazione orario se ricevo richiesta di un Time o di un DateTime
	if (m_TimeFormat != CDateFormatHelper::TIME_NONE)
	{	
		BOOL	setTT;
		CString	hourRange;	//H range 0-23, h range 1-12
		//se il risultato dello shift é dispari si deve visualizzare am-pm
		if ((((int)m_TimeFormat >> 4) % 2) == 1)
		{
			setTT = TRUE; 
			hourRange = _T("I");
		}
		else 
		{
			setTT = FALSE;
			hourRange = _T("H");
		}

		switch (m_TimeFormat & ~CDateFormatHelper::TIME_ONLY)
		{
			case CDateFormatHelper::HHMMSS:
			case CDateFormatHelper::HHMMSSTT:
			case CDateFormatHelper::HHMMTT:
			case CDateFormatHelper::HHMM:
				strFormatter += _T("%");; 
				strFormatter += hourRange; 
				break;
			case CDateFormatHelper::BHMMSS:
			case CDateFormatHelper::BHMMSSTT:
			case CDateFormatHelper::BHMMTT:
			case CDateFormatHelper::BHMM:
				{
					int nHour = setTT ? aValue.Hour() % 12 : aValue.Hour();
					if (nHour<10 && (!setTT || nHour>0))
					{
						strFormatter += _T(" ");
						strFormatter += _T("%#"); 
					}
					//se l'ora e'nel range 1-12, l'ora 0 viene scritta come 12 e non devo allineare a dx
					else 
					{
						strFormatter += _T("%");
					}
					strFormatter += hourRange; 
					break;
				}
			case CDateFormatHelper::HMMSS:
			case CDateFormatHelper::HMMSSTT:
			case CDateFormatHelper::HMMTT:
			case CDateFormatHelper::HMM:
				strFormatter += _T("%#");
				strFormatter += hourRange; 
				break;
		}
		
		CString strTimeSeparator = m_strTimeSeparator;
		if(strTimeSeparator == _T('%'))
			strTimeSeparator += _T('%');

		strFormatter.Append(strTimeSeparator);
		strFormatter.Append(_T("%M"));

		//se il risultato dello shift é zero vanno visualizzati i secondi
		if (((int)m_TimeFormat >> 6) == 0) 
		{
			strFormatter.Append(strTimeSeparator);
			strFormatter.Append(_T("%S"));
		}
		
		if (setTT)
		{
			strFormatter.Append(_T(" "));
			strFormatter.Append((aValue.Hour()== 0 || aValue.Hour()>12) ? m_strTimePM : m_strTimeAM );
		}
	}
	
	TCHAR buff[256];
	_tcsftime(buff, 256, strFormatter, &aValue.GetTMDateTime());

	result.Append(buff);

	// tolgo il carattere iniziale dell'anno
	// individuato dalla stringa spia 'szYearLocalizationChars'
	int nPos = result.Find (szYearLocalizationChars);
	if(nPos != -1)
	{
		int nCharsToRemove = _tcslen(szYearLocalizationChars) + 1;
		result.Delete (nPos, nCharsToRemove);
	}

	if(bPaddingEnabled)
	{
		result.Append(m_strTail);
		result.Insert(0, m_strHead);
		Padder(result, m_Align != RIGHT);
	}
}

//----------------------------------------------------------------------------
void CDateFormatter::RecalcWidths()
{
	// si formatta una data "lunga": il 24/12/1997 e` "mercoledi`" se si visualizza il giorno e
	// "dicembre" e` il mese piu` lungo (nel caso si visualizzi in lettere) di due cifre)
	DataDate data(24, 12, 1997);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	// per poter scrivere l'anno in forma estesa si forza temporaneamete
	// il formato a 4 cifre
	CDateFormatHelper::YearFormatTag nOldYearFormat = m_YearFormat;
	m_YearFormat = CDateFormatHelper::YEAR9999;
	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
	m_nInputDateLen = m_nInputCharLen;

	m_YearFormat = nOldYearFormat;
}

//----------------------------------------------------------------------------
const CSize CDateFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/)
{
	return GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), GetDefaultInputString(), TRUE);
}

//----------------------------------------------------------------------------
CString CDateFormatter::GetDefaultInputString  (DataObj* pDataObj /*NULL*/)
{
	DataDate data(DataDate::MAXVALUE);
	CString str;

	// per poter scrivere l'anno in forma estesa si forza temporaneamete
	// il formato a 4 cifre
	CDateFormatHelper::YearFormatTag nOldYearFormat = m_YearFormat;
	m_YearFormat = CDateFormatHelper::YEAR9999;
	FormatDataObj(data, str, FALSE);
	m_YearFormat = nOldYearFormat;

	return str;
}

// sets to locale only default values
//----------------------------------------------------------------------------
void CDateFormatter::SetToLocale ()
{
	if (m_bIsFormatTypeDefault)
		m_FormatType = AfxGetCultureInfo()->GetFormatStyleLocale().m_ShortDateFormat;

	if (m_bIsFormatYearDefault)
		m_YearFormat = AfxGetCultureInfo()->GetFormatStyleLocale().m_ShortDateYearFormat;

	/*	non impostati 
	if (m_TimeFormat == CDateFormatHelper::TIME_NONE)
		m_TimeFormat = AfxGetCultureInfo()->GetFormatStyleLocale().m_TimeFormat;
	
	if (m_strFirstSeparator.CompareNoCase(szDefDateSep) == 0)
		m_strFirstSeparator = AfxGetCultureInfo()->GetFormatStyleLocale().m_sDateSeparator;

	if (m_strSecondSeparator.CompareNoCase(szDefDateSep) == 0)
		m_strSecondSeparator = AfxGetCultureInfo()->GetFormatStyleLocale().m_sDateSeparator;
	
	if (m_strTimeSeparator.CompareNoCase(szDefTimeSep) == 0)
		m_strTimeSeparator = AfxGetCultureInfo()->GetFormatStyleLocale().m_sTimeSeparator;

	if (m_DayFormat == CDateFormatHelper::DAY99)
		m_DayFormat	= AfxGetCultureInfo()->GetFormatStyleLocale().m_ShortDateDayFormat;

	if (m_MonthFormat == CDateFormatHelper::MONTH99)
		m_MonthFormat = AfxGetCultureInfo()->GetFormatStyleLocale().m_ShortDateMonthFormat;

	if (m_strTimeAM.CompareNoCase(szDefTimeAM) == 0)
		m_strTimeAM	= AfxGetCultureInfo()->GetFormatStyleLocale().m_sAMSymbol;

	if (m_strTimePM.CompareNoCase(szDefTimePM) == 0)
		m_strTimePM	= AfxGetCultureInfo()->GetFormatStyleLocale().m_sPMSymbol;
*/
}

void CDateFormatter::SerializeJson(CJsonSerializer& strJson)  const
{
	__super::SerializeJson(strJson);
	
	// DateFormatter(sDateOrder, prologue, epilogue, sYearMode, sFirstSep, sMonthMode, sSecSep, sDayMode) 
	strJson.WriteInt(		szJsonFormatType, m_FormatType);
	strJson.WriteString(	szJsonFirstSep, m_strFirstSeparator);
	strJson.WriteString(	szJsonSecondSep, m_strSecondSeparator);
	strJson.WriteInt(		szJsonDayFormat, m_DayFormat);
	strJson.WriteInt(		szJsonMonthFormat, m_MonthFormat);
	strJson.WriteInt(		szJsonYearFormat, m_YearFormat);
	// time related fields.
	strJson.WriteString(	szJsonTimeSeparator, m_strTimeSeparator);
	strJson.WriteInt(		szJsonTimeFormat, m_TimeFormat);
	strJson.WriteString(	szJsonTimeAM , m_strTimeAM);
	strJson.WriteString(	szJsonTimePM, m_strTimePM);
	
	
}

//============================================================================
//		Class CDateTimeFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDateTimeFormatter, CDateFormatter)

//----------------------------------------------------------------------------
CDateTimeFormatter::CDateTimeFormatter (const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	CDateFormatter(name, aSource, aOwner)
{
	m_OwnType.SetFullDate();

	m_TimeFormat = CDateFormatHelper::HHMM;
}

//----------------------------------------------------------------------------
BOOL CDateTimeFormatter::IsSameAsDefault() const
{
	CDateTimeFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
void CDateTimeFormatter::SetToLocale ()
{
	CDateFormatter::SetToLocale();
	if (m_bIsTimeFormatDefault)
		m_TimeFormat = AfxGetCultureInfo()->GetFormatStyleLocale().m_TimeFormat;
}

//----------------------------------------------------------------------------
void CDateTimeFormatter::RecalcWidths()
{
	// si formatta una data "lunga": il 24/12/1997 e` "mercoledi`" se si visualizza il giorno e
	// "dicembre" e` il mese piu` lungo (nel caso si visualizzi in lettere) di due cifre)
	DataDate data;
	data.SetFullDate();
	data.SetDate(24, 12, 1997);
	data.SetTime(23, 59, 59);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	// per poter scrivere l'anno in forma estesa si forza temporaneamete
	// il formato a 4 cifre
	CDateFormatHelper::YearFormatTag nOldYearFormat	= m_YearFormat;
	m_YearFormat		= CDateFormatHelper::YEAR9999;
	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();

	// per sapere la lunghessa della sola parte data si forza temporaneamete
	// il formato senza ora
	CDateFormatHelper::TimeFormatTag nOldTimeFormat	= m_TimeFormat;
	m_TimeFormat		= CDateFormatHelper::TIME_NONE;
	FormatDataObj(data, str, FALSE);
	m_nInputDateLen = str.GetLength();

	m_YearFormat = nOldYearFormat;
	m_TimeFormat = nOldTimeFormat;
}

//----------------------------------------------------------------------------
const CSize CDateTimeFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/)
{
	return GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), GetDefaultInputString(), TRUE);
}

//----------------------------------------------------------------------------
CString	CDateTimeFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	DataDate data;
	data.SetFullDate();
	data.SetDate(DataDate::MAXVALUE);
	data.SetTime(23, 59, 59);

	// per poter scrivere l'anno in forma estesa si forza temporaneamete
	// il formato a 4 cifre
	CDateFormatHelper::YearFormatTag nOldYearFormat	= m_YearFormat;
	CDateFormatHelper::TimeFormatTag nOldTimeFormat	= m_TimeFormat;
	m_TimeFormat		= CDateFormatHelper::HHMMSS;
	m_YearFormat		= CDateFormatHelper::YEAR9999;
	CString str;
	FormatDataObj(data, str, TRUE);

	m_YearFormat = nOldYearFormat;
	m_TimeFormat = nOldTimeFormat;
	return str;
}

//============================================================================
//		Class CTimeFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CTimeFormatter, CDateFormatter)

//----------------------------------------------------------------------------
CTimeFormatter::CTimeFormatter (const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	CDateFormatter(name, aSource, aOwner)
{
	m_OwnType.SetAsTime();

	m_TimeFormat	= CDateFormatHelper::HHMM_NODATE;
}

//----------------------------------------------------------------------------
BOOL CTimeFormatter::IsSameAsDefault() const
{
	CTimeFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
void CTimeFormatter::RecalcWidths()
{
	DataDate data;
	data.SetAsTime();
	data.SetTime(23, 59, 59);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
}

//----------------------------------------------------------------------------
const CSize CTimeFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/)
{
	return GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), GetDefaultInputString(), TRUE);
}

//----------------------------------------------------------------------------
CString	CTimeFormatter::GetDefaultInputString (DataObj* pDataObj /*NULL*/)
{
	DataDate data;
	data.SetAsTime();
	data.SetTime(23, 59, 59);
	CString str;
	FormatDataObj(data, str, FALSE);
	return str;
}

//============================================================================
//@@ElapsedTime		Class CElapsedTimeFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CElapsedTimeFormatter, Formatter)

//----------------------------------------------------------------------------
CElapsedTimeFormatter::CElapsedTimeFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
    Formatter	(name, aSource, aOwner)
{
	m_OwnType	= DATA_LNG_TYPE;
	m_bEditable	= TRUE;
	m_OwnType	.SetAsTime();

	m_FormatType		= CElapsedTimeFormatHelper::TIME_HM;
	m_strTimeSeparator	= szDefTimeSep;
	m_strDecSeparator	= szDefDecSeparator;
	m_nDecNumber		= nDefDecimals;
	m_nCaptionPos		= 0;
	m_Align				= GetDefaultAlign();

	m_bIsDecSeparatorDefault = TRUE;
}

//----------------------------------------------------------------------------
Formatter::AlignType CElapsedTimeFormatter::GetDefaultAlign	()	const
{
	return RIGHT;
}

//----------------------------------------------------------------------------
CString CElapsedTimeFormatter::GetShortDescription() const
{
	switch (m_FormatType & (CElapsedTimeFormatHelper::TIME_DHMS | CElapsedTimeFormatHelper::TIME_DEC))
	{
		case CElapsedTimeFormatHelper::TIME_DHMSF	:		return _T("(d:h:m:s.f)");
		case CElapsedTimeFormatHelper::TIME_DHMS	:		return _T("(d:h:m:s)");
		case CElapsedTimeFormatHelper::TIME_DHM		:		return _T("(d:h:m)");
    	case CElapsedTimeFormatHelper::TIME_DH		:		return _T("(d:h)");
    	case CElapsedTimeFormatHelper::TIME_D		:		return _T("(g)");
		case CElapsedTimeFormatHelper::TIME_HMSF	:		return _T("(h:m:s.f)");
    	case CElapsedTimeFormatHelper::TIME_HMS		:		return _T("(h:m:s)");
    	case CElapsedTimeFormatHelper::TIME_HM		:		return _T("(h:m)");
    	case CElapsedTimeFormatHelper::TIME_H		:		return _T("(h)");
		case CElapsedTimeFormatHelper::TIME_MSF		:		return _T("(m:s.f)");
    	case CElapsedTimeFormatHelper::TIME_MSEC	:		return _T("(m:s)");
    	case CElapsedTimeFormatHelper::TIME_M		:		return _T("(m)");
		case CElapsedTimeFormatHelper::TIME_SF		:		return _T("(s.f)");
    	case CElapsedTimeFormatHelper::TIME_S		:		return _T("(s)");
    	case CElapsedTimeFormatHelper::TIME_DHMCM	:		return _T("(d:h:m.f)");
    	case CElapsedTimeFormatHelper::TIME_DHCH	:		return _T("(d:h.f)");
    	case CElapsedTimeFormatHelper::TIME_DCD		:		return _T("(d.f)");
    	case CElapsedTimeFormatHelper::TIME_HMCM	:		return _T("(h:m.f)");
    	case CElapsedTimeFormatHelper::TIME_HCH		:		return _T("(h.f)");
    	case CElapsedTimeFormatHelper::TIME_MCM		:		return _T("(m.f)");
	   	case CElapsedTimeFormatHelper::TIME_CH		:		return _T("(hc.f)");  //ora centesimale
	}

	return _T("");
}


//----------------------------------------------------------------------------
CString CElapsedTimeFormatter::GetNumberFormatString() const
{
	switch (m_FormatType & (CElapsedTimeFormatHelper::TIME_DHMS | CElapsedTimeFormatHelper::TIME_DEC))
	{
		case CElapsedTimeFormatHelper::TIME_DHMSF	:		return _T("d:h:m:s.f"); //vedere f
		case CElapsedTimeFormatHelper::TIME_DHMS	:		return _T("d:h:m:s");
		case CElapsedTimeFormatHelper::TIME_DHM		:		return _T("d:h:m");
    	case CElapsedTimeFormatHelper::TIME_DH		:		return _T("d:h");//
    	case CElapsedTimeFormatHelper::TIME_D		:		return _T("n"); //
		case CElapsedTimeFormatHelper::TIME_HMSF	:		return _T("hh:m:s.f");
    	case CElapsedTimeFormatHelper::TIME_HMS		:		return _T("hh:m:s");
    	case CElapsedTimeFormatHelper::TIME_HM		:		return _T("hh:m");   //_T("(h:m)");
    	case CElapsedTimeFormatHelper::TIME_H		:		return _T("n"); //
		case CElapsedTimeFormatHelper::TIME_MSF		:		return _T("mm:s.f");
    	case CElapsedTimeFormatHelper::TIME_MSEC	:		return _T("mm:s");
    	case CElapsedTimeFormatHelper::TIME_M		:		return _T("n"); //
		case CElapsedTimeFormatHelper::TIME_SF		:		return _T("ss.f");
    	case CElapsedTimeFormatHelper::TIME_S		:		return _T("n"); //
    	case CElapsedTimeFormatHelper::TIME_DHMCM	:		return _T("d:h:m.f");
    	case CElapsedTimeFormatHelper::TIME_DHCH	:		return _T("d:h.f");
    	case CElapsedTimeFormatHelper::TIME_DCD		:		return _T("d.f");
    	case CElapsedTimeFormatHelper::TIME_HMCM	:		return _T("hh:m.f");
    	case CElapsedTimeFormatHelper::TIME_HCH		:		return _T("hh.f");
    	case CElapsedTimeFormatHelper::TIME_MCM		:		return _T("mm.f");
	   	case CElapsedTimeFormatHelper::TIME_CH		:		return _T("(hc.f)"); //TODO
	}

	return _T("");
}



//----------------------------------------------------------------------------
void CElapsedTimeFormatter::Assign(const Formatter& Fmt)
{
	Formatter::Assign(Fmt);

	const CElapsedTimeFormatter& DF = (const CElapsedTimeFormatter&)Fmt;
	m_strTimeSeparator	= DF.m_strTimeSeparator;
	m_strDecSeparator 	= DF.m_strDecSeparator;
	m_nDecNumber		= DF.m_nDecNumber;
	m_nCaptionPos		= DF.m_nCaptionPos;
	m_FormatType		= DF.m_FormatType;
	m_bIsDecSeparatorDefault = DF.m_bIsDecSeparatorDefault;
}

//----------------------------------------------------------------------------
BOOL CElapsedTimeFormatter::IsSameAsDefault() const
{
	CElapsedTimeFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
int CElapsedTimeFormatter::Compare(const Formatter& F) const
{
	if (Formatter::Compare(F)) return 1;

	const CElapsedTimeFormatter& IF = (const CElapsedTimeFormatter&)F;
	if (m_strTimeSeparator	!= IF.m_strTimeSeparator	)	return 1;
	if (m_strDecSeparator	!= IF.m_strDecSeparator		)	return 1;
	if (m_nDecNumber		!= IF.m_nDecNumber			)	return 1;
	if (m_nCaptionPos		!= IF.m_nCaptionPos			)	return 1;
	if (m_FormatType		!= IF.m_FormatType			)	return 1;
	
	if (m_bIsDecSeparatorDefault != IF.m_bIsDecSeparatorDefault	)	return 1;
	
	return 0;
}

//----------------------------------------------------------------------------
void CElapsedTimeFormatter::Format(const void* p, CString& result, BOOL bPaddingEnabled /*= TRUE*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{

	result.Empty();
	if(!p)
	{
		ASSERT_TRACE(p != NULL,"Parameter p must not be null");
		return;
	}
	
	long lTime = *(long*)p;
	DataLng aValue(lTime > 0 ? lTime : -lTime);
	aValue.SetAsTime();

	CString strFormatter;
	strFormatter.Format(_T("%%.%df"), m_nDecNumber);
	
	CString strDecFormatter;
	strDecFormatter.Format(_T("%%0%d.%df"), m_nDecNumber? m_nDecNumber+3 : 2, m_nDecNumber);

	BOOL bReplaceDecSeparator = FALSE;

	switch (m_FormatType)
	{
		case CElapsedTimeFormatHelper::TIME_CH: 
		{
			double	cent = aValue.GetCentHours();
			result.Format(strFormatter, cent); 
			bReplaceDecSeparator = TRUE;
			break;	
		}
		case CElapsedTimeFormatHelper::TIME_D:	
			result.Format(_T("%d"), aValue.GetDays());
			break;	
		case CElapsedTimeFormatHelper::TIME_DCD:
			result.Format(strFormatter, aValue.GetDecDays()); 
			bReplaceDecSeparator = TRUE;				
			break;	
		case CElapsedTimeFormatHelper::TIME_H:	
			result.Format(_T("%d"), aValue.GetTotalHours()); 
			break;
		case CElapsedTimeFormatHelper::TIME_HCH:
			result.Format(strFormatter, aValue.GetDecHours()); 
			bReplaceDecSeparator = TRUE;			
			break;
		case CElapsedTimeFormatHelper::TIME_M:
			result.Format(_T("%d"), aValue.GetTotalMinutes()); 
			break;
		case CElapsedTimeFormatHelper::TIME_MCM:
			result.Format(strFormatter, aValue.GetDecMinutes()); 
			bReplaceDecSeparator = TRUE;				
			break;
		case CElapsedTimeFormatHelper::TIME_S:
			result.Format(_T("%d"), (long)floor(aValue.GetTotalSeconds())); 
			break;		
		case CElapsedTimeFormatHelper::TIME_SF:	
			result.Format(strFormatter, aValue.GetTotalSeconds()); 
			bReplaceDecSeparator = TRUE;					
			break;
		case CElapsedTimeFormatHelper::TIME_DH:
			result.Format(_T("%d%s%02d"), aValue.GetDays(), m_strTimeSeparator, aValue.GetHours());			
			break;
		case CElapsedTimeFormatHelper::TIME_DHCH:
			result.Format(_T("%d%s")+strDecFormatter, aValue.GetDays(), m_strTimeSeparator, aValue.GetDecHours()- aValue.GetDays()*24);	 
			bReplaceDecSeparator = TRUE;
			break;			
		case CElapsedTimeFormatHelper::TIME_DHM:
			result.Format(_T("%d%s%02d%s%02d"), aValue.GetDays(), 
							m_strTimeSeparator, aValue.GetHours(), 
							m_strTimeSeparator, aValue.GetMinutes());
			break;
		case CElapsedTimeFormatHelper::TIME_DHMCM:
			result.Format(_T("%d%s%02d%s")+strDecFormatter, aValue.GetDays(), 
							m_strTimeSeparator, aValue.GetHours(), 
							m_strTimeSeparator, aValue.GetDecMinutes() - aValue.GetTotalHours()*60); 
			bReplaceDecSeparator = TRUE;
			break;
		case CElapsedTimeFormatHelper::TIME_DHMS:
			result.Format(_T("%d%s%02d%s%02d%s%02d"), aValue.GetDays(),
							m_strTimeSeparator, aValue.GetHours(),
							m_strTimeSeparator, aValue.GetMinutes(), 
							m_strTimeSeparator, (long)floor(aValue.GetSeconds())); 
			break;			  
		case CElapsedTimeFormatHelper::TIME_DHMSF: 
			result.Format(_T("%d%s%02d%s%02d%s")+strDecFormatter, aValue.GetDays(),
							m_strTimeSeparator, aValue.GetHours(),
							m_strTimeSeparator, aValue.GetMinutes(), 
							m_strTimeSeparator, aValue.GetSeconds());  
			bReplaceDecSeparator = TRUE;
			break;
		case CElapsedTimeFormatHelper::TIME_HM:
			result.Format(_T("%d%s%02d"), aValue.GetTotalHours(),
							m_strTimeSeparator, aValue.GetMinutes());
			break;
		case CElapsedTimeFormatHelper::TIME_HMCM:
			result.Format(_T("%d%s")+strDecFormatter, aValue.GetTotalHours(),
							m_strTimeSeparator, aValue.GetDecMinutes() - aValue.GetTotalHours()*60); 
			bReplaceDecSeparator = TRUE;
			break;		
		case CElapsedTimeFormatHelper::TIME_HMS:
			result.Format(_T("%d%s%02d%s%02d"), aValue.GetTotalHours(),
							m_strTimeSeparator, aValue.GetMinutes(),
							m_strTimeSeparator, (long)floor(aValue.GetSeconds()));
			break;			
		case CElapsedTimeFormatHelper::TIME_HMSF:
			result.Format(_T("%d%s%02d%s")+strDecFormatter, aValue.GetTotalHours(),
							m_strTimeSeparator, aValue.GetMinutes(),
							m_strTimeSeparator, aValue.GetSeconds()); 
			bReplaceDecSeparator = TRUE;
			break;		
		case CElapsedTimeFormatHelper::TIME_MSEC:
			result.Format(_T("%d%s%02d"), aValue.GetTotalMinutes(),
							m_strTimeSeparator, (long)floor(aValue.GetSeconds()));
			break;
		case CElapsedTimeFormatHelper::TIME_MSF:
			result.Format(_T("%d%s")+strDecFormatter, aValue.GetTotalMinutes(),
							m_strTimeSeparator, aValue.GetSeconds()); 
			bReplaceDecSeparator = TRUE;
			break;
	}

	// se ho un numero con virgola, sostituisco il decimal separator corretto
	if(bReplaceDecSeparator)
	{
		int nPos, nLength;
		nPos = nLength = result.GetLength()-1;
		while (_istdigit(result[nPos]) && nPos>=0)
			nPos--;
		result.SetAt(nPos, m_strDecSeparator[0]); 
	}
				
	if (lTime < 0)
	{
		result.Insert(0,szMinus);
		lTime = -lTime;
	}

	if (bPaddingEnabled)
	{
		result.Append(m_strTail);
		result.Insert(0, m_strHead);
		Padder(result, m_Align != RIGHT);
	}
}

//----------------------------------------------------------------------------
void CElapsedTimeFormatter::RecalcWidths()
{
	DataLng data(LONG_MAX);	data.SetAsTime();
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
}

//----------------------------------------------------------------------------
const CSize CElapsedTimeFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/)
{
	DataLng data(LONG_MAX);	
	data.SetAsTime();
	CString str;
	FormatDataObj(data, str, FALSE);

	return GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), str, TRUE);
}

//----------------------------------------------------------------------------
CString CElapsedTimeFormatter::GetDefaultInputString  (DataObj* pDataObj /*NULL*/)
{
	DataLng data(LONG_MAX);	
	data.SetAsTime();
	CString str;
	FormatDataObj(data, str, FALSE);
	return str;
}

//	if (m_nDecNumber) 	m_nDecNumber = AfxGetCultureInfo()->GetFormatStyleLocale().m_nDecimals;
//----------------------------------------------------------------------------
void CElapsedTimeFormatter::SetToLocale ()
{
	if (m_bIsDecSeparatorDefault)
		m_strDecSeparator = AfxGetCultureInfo()->GetFormatStyleLocale().m_sDecSeparator;
}

//------------------------------------------------------------------------------
void CElapsedTimeFormatter::SerializeJson(CJsonSerializer& strJson)  const
{
	__super::SerializeJson(strJson);
	
	strJson.WriteInt(		szJsonFormatType, m_FormatType);
	strJson.WriteString(	szJsonTimeSeparator, m_strTimeSeparator);
	strJson.WriteString(	szJsonDecimalSeparator, m_strDecSeparator);
	strJson.WriteInt(		szJsonDecNumber, m_nDecNumber);
}
//============================================================================
//		Class CBoolFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CBoolFormatter, Formatter)

//----------------------------------------------------------------------------
CBoolFormatter::CBoolFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	Formatter	(name, aSource, aOwner)
{
	m_FormatType	= AS_ZERO;
	m_OwnType		= DATA_BOOL_TYPE;                                     
	m_bEditable		= TRUE;
	m_strFalseTag	= szDefaultFalseTag;
	m_strTrueTag	= szDefaultTrueTag;
}

//----------------------------------------------------------------------------
void CBoolFormatter::Assign(const Formatter& Fmt)
{
	Formatter::Assign(Fmt);

	const CBoolFormatter& BF = (const CBoolFormatter&)Fmt;

	m_strFalseTag	= BF.m_strFalseTag;
	m_strTrueTag	= BF.m_strTrueTag;
	m_FormatType	= BF.m_FormatType;
}

//----------------------------------------------------------------------------
BOOL CBoolFormatter::IsSameAsDefault() const
{
	CBoolFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
int CBoolFormatter::Compare(const Formatter& F) const
{
	if (Formatter::Compare(F)) return 1;

	const CBoolFormatter& BF = (const CBoolFormatter&)F;

	return	(
				m_strFalseTag	!= BF.m_strFalseTag	||
				m_strTrueTag	!= BF.m_strTrueTag
			)
			? 1	: 0;
}

//----------------------------------------------------------------------------
void CBoolFormatter::Format(const void* p, CString& result, BOOL bPaddingEnabled /*= TRUE*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	result.Empty();
	if(!p)
	{
		ASSERT_TRACE(p != NULL,"Parameter p must not be null");
		return;
	}

	result = (*(const int*)p) 
		? GetTrueTag()
		: GetFalseTag();

	if (bPaddingEnabled)
	{
		result.Append(m_strTail);
		result.Insert(0, m_strHead);
		Padder(result, m_Align != RIGHT);
	}		
}

//----------------------------------------------------------------------------
void CBoolFormatter::RecalcWidths()
{
	DataBool data;
	CString str;

	// si formatta TRUE
	data.Assign(TRUE);
	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();

	// si formatta FALSE
	data.Assign(FALSE);
	FormatDataObj(data, str);
	m_nOutputCharLen = max(m_nOutputCharLen, str.GetLength());

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = max(m_nInputCharLen, str.GetLength());
}

//----------------------------------------------------------------------------
const CSize CBoolFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/) 
{
	DataBool data;
	CString strTrue, strFalse;

	// si formatta TRUE
	data.Assign(TRUE);
	FormatDataObj(data, strTrue);

	// si formatta FALSE
	data.Assign(FALSE);
	FormatDataObj(data, strFalse);

	CSize trueLength	= GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), strTrue, TRUE);
	CSize falseLength	= GetEditSize(pDC, pFont ? pFont : AfxGetThemeManager()->GetFormFont(), strFalse, TRUE);
	return (trueLength.cx > falseLength.cx) ? trueLength : falseLength;
}

//----------------------------------------------------------------------------
CString	CBoolFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	DataBool data;
	CString strTrue, strFalse;

	// si formatta TRUE
	data.Assign(TRUE);
	FormatDataObj(data, strTrue);

	// si formatta FALSE
	data.Assign(FALSE);
	FormatDataObj(data, strFalse);

	return (strTrue.GetLength() > strFalse.GetLength()) ? strTrue : strFalse;
}

//============================================================================
//		Class CEnumFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CEnumFormatter, CStringFormatter)

//----------------------------------------------------------------------------
CEnumFormatter::CEnumFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
    CStringFormatter	(name, aSource, aOwner)
{
	m_OwnType	= DATA_ENUM_TYPE;                                     
	m_bEditable	= TRUE;
	SetFormat(ASIS);
}
	
//----------------------------------------------------------------------------
void CEnumFormatter::SetFormat (FormatTag format)
{
	m_FormatType = format;
	switch(format)
	{
		case ASIS:
			CStringFormatter::SetFormat(CStringFormatter::ASIS);
			break; 	
		case UPPERCASE:
			CStringFormatter::SetFormat(CStringFormatter::UPPERCASE);
			break; 
		case LOWERCASE:	
			CStringFormatter::SetFormat(CStringFormatter::LOWERCASE);
			break;
		case CAPITALIZED:	
			CStringFormatter::SetFormat(CStringFormatter::CAPITALIZED);
			break;	
	}
}

//----------------------------------------------------------------------------
void CEnumFormatter::Assign(const Formatter& Fmt)
{
	CStringFormatter::Assign(Fmt);
	
	const CEnumFormatter& EF = (const CEnumFormatter&)Fmt;
	SetFormat(EF.m_FormatType);
}

//----------------------------------------------------------------------------
BOOL CEnumFormatter::IsSameAsDefault() const
{
	CEnumFormatter templ;
	return *this == templ;
}

//----------------------------------------------------------------------------
int CEnumFormatter::Compare(const Formatter& Fmt) const
{
	const CEnumFormatter& EF = (const CEnumFormatter&)Fmt;
	
	if (m_FormatType != EF.m_FormatType)
		return 1;

	return CStringFormatter::Compare(Fmt);
}

//----------------------------------------------------------------------------
void CEnumFormatter::Format(const void* p, CString& result, BOOL bPaddingEnabled /*= TRUE*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	result.Empty();
	if(!p)
	{
		ASSERT_TRACE(p != NULL,"Parameter p must not be null");
		return;
	}

	DWORD dwValue = *((DWORD*)p);
	const EnumItemArray* pItemArray = AfxGetEnumsTable()->GetEnumItems(GET_TAG_VALUE(dwValue));
	if (pItemArray == NULL || pItemArray->GetSize() == 0)
	{
		ASSERT_TRACE1(pItemArray != NULL && pItemArray->GetSize() != 0,"Failed to retreive the list of formatted items of enum %d",dwValue);
		return;
	}

	CString strTmp = pItemArray->GetTitle(GET_ITEM_VALUE(dwValue));

	if (strTmp.IsEmpty())
		return;

	CStringFormatter aStrFormatter;
	switch (m_FormatType)
	{
		case ASIS:
		case UPPERCASE:
		case LOWERCASE:	
		case CAPITALIZED:
			// enums format tag is different from CStringFormatter format tag!!!
			aStrFormatter.Assign(*this);
			switch (m_FormatType)
			{
				case ASIS:			aStrFormatter.SetFormat(CStringFormatter::ASIS);			break;
				case CAPITALIZED:	aStrFormatter.SetFormat(CStringFormatter::CAPITALIZED);		break;
				case UPPERCASE:		aStrFormatter.SetFormat(CStringFormatter::UPPERCASE);		break;
				case LOWERCASE:		aStrFormatter.SetFormat(CStringFormatter::LOWERCASE);		break;
			}
			aStrFormatter.Format ((void*)(LPCTSTR)strTmp, result, bPaddingEnabled, bCollateCultureSensitive);
			return;	
		case FIRSTLETTER:	
			result = strTmp.Left(1);		
			break;
	}
	
	if (bPaddingEnabled)
	{
		result.Append(m_strTail);
		result.Insert(0, m_strHead);
		Padder(result, m_Align != RIGHT);
	}		

}

//----------------------------------------------------------------------------
const CSize CEnumFormatter::GetInputWidth  (CDC* pDC, int nCols /*-1*/, CFont* pFont /*= NULL*/)
{
	ASSERT (FALSE);
	TRACE ("Enums formatter cannot calculate the width because has not tag value");
	return CSize(0,0);
}

//----------------------------------------------------------------------------
CString	CEnumFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	if (pDataObj == NULL)
	{
		ASSERT (FALSE);
		TRACE ("Enums formatter cannot calculate the width because has not tag value");
		return _T("");
	}
	
	DataEnum longerEnumValue (pDataObj->GetDataType().m_wTag, AfxGetEnumsTable()->GetEnumLongerItemValue(pDataObj->GetDataType().m_wTag));
	CString str;
	FormatDataObj(longerEnumValue, str);
	return str;
}

//============================================================================
//		Class CGuidFormatter Implementation
//============================================================================
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CGuidFormatter, CStringFormatter)

//----------------------------------------------------------------------------
CGuidFormatter::CGuidFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
    CStringFormatter	(name, aSource, aOwner)
{
	m_OwnType		= DATA_GUID_TYPE;
	m_bEditable		= FALSE;
}

//----------------------------------------------------------------------------
void CGuidFormatter::Format(const void* p, CString& result, BOOL bPaddingEnabled /*= TRUE*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	result.Empty();
	if(!p)
	{
		ASSERT_TRACE(p != NULL,"Parameter p must not be null");
		return;
	}

	GUID* pG = (GUID*)p;
	CString strTmp(GuidToString(*pG));
	CStringFormatter::Format((LPCTSTR)strTmp, result, bPaddingEnabled, bCollateCultureSensitive);
}

//============================================================================
//		Class CTickTimeFormatter Implementation
//============================================================================

//----------------------------------------------------------------------------
CTickTimeFormatter::CTickTimeFormatter()
{
	SetFormat (CElapsedTimeFormatHelper::TIME_HMS);
}

//----------------------------------------------------------------------------
CString CTickTimeFormatter::FormatTime(DWORD aTick)
{
	if (aTick <= 0)
		return _T("0");
	
	CString strTime;
	// è al di sotto del secondo
	if (aTick >= 1000)
	{
		long	nSecond		= long(aTick/1000);
		double	dMilliSec	= double((double(aTick)/1000) - nSecond);
		Format(&nSecond, strTime);
		if (dMilliSec > 0.0009)
			strTime += _T(",") + cwsprintf(_T("%d"), int(dMilliSec*1000));
	}
	else
		strTime = cwsprintf(_T("0,%d"),  aTick);
	
	return strTime;
}

//============================================================================
//		Class CMonthNameFormatter Implementation
//============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (CMonthNameFormatter, CIntFormatter)

CMonthNameFormatter::CMonthNameFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	CIntFormatter (name, aSource, aOwner)
{
	m_OwnType = DATA_INT_TYPE;
	m_nOutputCharLen	= 9;	
	m_nInputCharLen		= 4;
}

//-----------------------------------------------------------------------------
void CMonthNameFormatter::Format	(const void* Address, CString& Str, BOOL /*bPaddingEnabled*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	WORD l = *(short*)Address;
	Str = MonthName(l);
}

//----------------------------------------------------------------------------
void CMonthNameFormatter::RecalcWidths()
{
	DataInt data(12);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
}
//============================================================================
//		Class CWeekDayNameFormatter Implementation
//============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (CWeekDayNameFormatter, CIntFormatter)

CWeekDayNameFormatter::CWeekDayNameFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
		:
		CIntFormatter (name, aSource, aOwner)
{
	m_OwnType			= DATA_INT_TYPE;
	m_nOutputCharLen	= 9;	
	m_nInputCharLen		= 4;
}
	
//-----------------------------------------------------------------------------
void CWeekDayNameFormatter::Format	(const void* Address, CString& Str, BOOL /*bPaddingEnabled*/, BOOL bCollateCultureSensitive /*FALSE*/) const
{
	int l = *(short*)Address;
	Str = WeekDayName(l-1);
}

//----------------------------------------------------------------------------
void CWeekDayNameFormatter::RecalcWidths()
{
	DataInt data(12);
	CString str;

	FormatDataObj(data, str);
	m_nOutputCharLen = str.GetLength();

	FormatDataObj(data, str, FALSE);
	m_nInputCharLen = str.GetLength();
}


//============================================================================
//		Class CPrivacyFormatter Implementation
//============================================================================
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE (CPrivacyFormatter, Formatter)

//-----------------------------------------------------------------------------
CPrivacyFormatter::CPrivacyFormatter(const CString& name, const FormatStyleSource aSource, const CTBNamespace& aOwner)
	:
	Formatter (name, aSource, aOwner)
{
}

//-----------------------------------------------------------------------------
void CPrivacyFormatter::Format(const void* /*data*/, CString& Str, BOOL /*bPaddingEnabled*/, BOOL /*bCollateCultureSensitive*/) const
{
	//int nLen = (data && data->IsKindOf(RUNTIME_CLASS(DataObj)) ? data->GetLengh() : 5;
	Str = _T("***");
}

//-----------------------------------------------------------------------------
CString	CPrivacyFormatter::GetDefaultInputString(DataObj* pDataObj /*NULL*/)
{
	return _T("***");
}
