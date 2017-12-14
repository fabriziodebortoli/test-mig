
#pragma once

#include "array.h"

//includere alla fine degli include del .H
#include "beginh.dex"

class Formatter;
class CNumberToLiteralLookUpTableManager;

//============================================================================
class TB_EXPORT CDateFormatHelper
{
public:
	enum FormatTag 
	{
		DATE_DMY	=		0x0000,
		DATE_MDY	=		0x0001,
		DATE_YMD	=		0x0002
	};

	enum WeekdayFormatTag 
	{
		NOWEEKDAY =			0x0000,
		PREFIXWEEKDAY =		0x0001,
		POSTFIXWEEKDAY =	0x0002
	};

	enum DayFormatTag 
	{
		DAY99 =			0x0000,
		DAYB9 =			0x0001,
		DAY9  =			0x0002
	};

	enum MonthFormatTag 
	{
		MONTH99 =			0x0000,
		MONTHB9 =			0x0001,
		MONTH9	 =			0x0002,
		MONTHS3 =			0x0003,
		MONTHSX =			0x0004
	};

	enum YearFormatTag 
	{
		YEAR99 =			0x0000,
		YEAR999 =			0x0001,
		YEAR9999 =			0x0002
	};

	enum TimeFormatTag 
	{
		TIME_NONE		= 0x0000,
		TIME_HF99		= 0x0001,
		TIME_HFB9		= 0x0002,
		TIME_HF9		= 0x0003,
		TIME_AMPM		= 0x0010,
		TIME_ONLY		= 0x0020,
		TIME_NOSEC		= 0x0040,
		HHMMTT			= TIME_HF99 | TIME_NOSEC | TIME_AMPM,	
		BHMMTT			= TIME_HFB9 | TIME_NOSEC | TIME_AMPM,	
		HMMTT			= TIME_HF9  | TIME_NOSEC | TIME_AMPM,	
		HHMMSSTT		= TIME_HF99 | TIME_AMPM,
		BHMMSSTT		= TIME_HFB9 | TIME_AMPM,
		HMMSSTT			= TIME_HF9  | TIME_AMPM,					
		HHMM			= TIME_HF99 | TIME_NOSEC,		
		BHMM			= TIME_HFB9 | TIME_NOSEC,		
		HMM				= TIME_HF9  | TIME_NOSEC,
		HHMMSS			= TIME_HF99,
		BHMMSS			= TIME_HFB9,	
		HMMSS			= TIME_HF9,
		HHMMTT_NODATE	= HHMMTT | TIME_ONLY,	
		BHMMTT_NODATE	= BHMMTT | TIME_ONLY,	
		HMMTT_NODATE	= HMMTT | TIME_ONLY,	
		HHMMSSTT_NODATE	= HHMMSSTT | TIME_ONLY,
		BHMMSSTT_NODATE	= BHMMSSTT | TIME_ONLY,
		HMMSSTT_NODATE	= HMMSSTT | TIME_ONLY,					
		HHMM_NODATE		= HHMM | TIME_ONLY,
		BHMM_NODATE		= BHMM | TIME_ONLY,		
		HMM_NODATE		= HMM | TIME_ONLY,
		HHMMSS_NODATE	= HHMMSS | TIME_ONLY,
		BHMMSS_NODATE	= BHMMSS | TIME_ONLY,	
		HMMSS_NODATE	= HMMSS | TIME_ONLY 			
	};
};

//============================================================================
class TB_EXPORT FormatStyleLocale : public CObject
{
friend class CCultureInfo;

private:
	BOOL			m_bLoaded;
	BOOL			m_bEnabled;
	Array			m_SupportedLocales;

public:
	// numbers
	CString					m_sDecSeparator;
	int						m_nDecimals;
	CString					m_s1000LongSeparator;
	CString					m_s1000DoubleSeparator;
	BOOL					m_bZeroPadded;

	// dates and times
	CString					m_sTimeSeparator;
	CString					m_sDateSeparator;
	CString					m_sAMSymbol;
	CString					m_sPMSymbol;
	CString					m_sShortDateFormat;
	CString					m_sLongTimeFormat;

	CDateFormatHelper::TimeFormatTag		m_TimeFormat;
	CDateFormatHelper::FormatTag			m_ShortDateFormat;
	CDateFormatHelper::DayFormatTag			m_ShortDateDayFormat;
	CDateFormatHelper::MonthFormatTag		m_ShortDateMonthFormat;
	CDateFormatHelper::YearFormatTag		m_ShortDateYearFormat;
	
	BOOL IsLoaded() const					{ return m_bLoaded; }

private:
	FormatStyleLocale	 ();

	BOOL ReadSettings		(const CString& sCulture);
	void RefreshFormatters	();

	LCID GetCultureLCID		(const CString& sCulture);
	void SetToProgramDefault();
	
	void ReadDateFormat		(LCID id);
	void ReadNumericFormat	(LCID id);
	void ReadTimeFormat		(LCID id);
};

// Funzioni statiche generali
//============================================================================
class TB_EXPORT FormatHelpers
{
public:

public:

//	Integer Support
	static void	InsertThousandSeparator	(CString& strIOBuffer, LPCTSTR Separator);
	static void	InsertThousandSeparator	(TCHAR* pszIOBuffer, LPCTSTR Separator);
	
	static void NumberToWords					(double aValue, CString& result, BOOL bConvertDecimal = FALSE);
	static void DecimalToWords					(double aValue, CString& result);
	static BOOL NeedDecimalConversionInLetter	();
	static void NumberToEncoded					(long l, LPCTSTR pszXTable, TCHAR* buffer);

//	Floating point Support	
	static double RoundSigned	(double d, double quantum);
	static double RoundAbsolute	(double d, double quantum);
	static double RoundZero		(double d, double quantum);
	static double RoundInfinite	(double d, double quantum);

	enum RoundMode { Signed, Absolute, Zero, Infinite };
	
	static double Round			(double d, double quantum, RoundMode Rm);
//	LiteralToNumber Lookup Table

private:
	static CString ConcatenaGruppoSingolare(long i, int len, BOOL& bWriteMilliards);
	static CString ConcatenaGruppo(long i, int nValue, int lastDigit, BOOL& bWriteMilliards);
	static CString AddSeparator(int len, long valTripla, long valDoppia, BOOL& bUsedJunction);
};


// elementi statici della classe utilizzabili da tutte le funzioni
//============================================================================
class TB_EXPORT CElapsedTimeFormatHelper
{
public:
	enum FormatTag
	{
			TIME_D		= 0X0001, 
			TIME_H		= 0X0002, 
			TIME_M		= 0x0004, 
			TIME_S		= 0x0008, 
			TIME_CH		= 0x0010,
			TIME_C		= 0x1000, 
			TIME_F		= 0x2000,
			TIME_DEC	= TIME_C	| TIME_F	| TIME_CH,
			TIME_DHMS	= TIME_D	| TIME_H	| TIME_M	| TIME_S,	
			TIME_DHMSF	= TIME_DHMS | TIME_F,
			TIME_DHMCM	= TIME_DHMS | TIME_C,		
			TIME_DHM	= TIME_D	| TIME_H	| TIME_M,
			TIME_DHCH	= TIME_DHM	| TIME_C,			
			TIME_DH		= TIME_D	| TIME_H,				
			TIME_DCD	= TIME_DH	| TIME_C,					
			TIME_HMS	= TIME_H	| TIME_M	| TIME_S,
			TIME_HMSF	= TIME_HMS  | TIME_F,			
			TIME_HMCM	= TIME_HMS  | TIME_C,		
			TIME_HM		= TIME_H	| TIME_M,				
			TIME_HCH	= TIME_HM	| TIME_C,
			TIME_MSEC	= TIME_M	| TIME_S,				
			TIME_MSF	= TIME_MSEC	| TIME_F,					
			TIME_MCM	= TIME_MSEC	| TIME_C,					
			TIME_SF		= TIME_S	| TIME_F
	};
};

//----------------------------------------------------------------------------
TB_EXPORT BOOL CALLBACK FormatLocaleEnumProc(LPTSTR lpLocaleString);
//----------------------------------------------------------------------------
TB_EXPORT const CNumberToLiteralLookUpTableManager* AfxGetNumberToLiteralLookUpTableManager();

#include "endh.dex"
