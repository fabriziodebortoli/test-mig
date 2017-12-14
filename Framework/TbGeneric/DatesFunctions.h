#pragma once

#include <oledb.h>
#include <TbNameSolver\TBResourceLocker.h>
#include <lm.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class DataDate;
class CTBNamespace;
//=============================================================================
#define MIN_DAY				31
#define MIN_MONTH			12
#define MIN_YEAR			1799
#define MIN_TIME_DAY		30			// vedere HELP di Microsoft
#define MIN_TIME_MONTH		12			// vedere HELP di Microsoft
#define MIN_TIME_YEAR		1899		// vedere HELP di Microsoft
#define MIN_WEEK_DAY		2
#define MIN_HOUR			0
#define MIN_MINUTE			0
#define MIN_SECOND			0
#define MAX_DAY				31			// dipende comunque dal mese
#define MAX_MONTH			12
#define MAX_YEAR			2199
#define	MIN_GIULIAN_DATE	1L			// corrispondente a 01/01/1800
#define	MAX_GIULIAN_DATE	GetGiulianDate(MAX_DAY, MAX_MONTH, MAX_YEAR)

#define GET_MILLENIUM(l,y) (l == 3 ? (y >= 799 ? 1000 : 2000) : (l == 2 ? (y >= 30 ? 1900 : 2000) : 0))

// Usati solo nei parsedctrl per gestire l'errore
#define BAD_DATE			LONG_MIN
#define BAD_TIME			LONG_MIN
#define BAD_DAY				0
#define BAD_MONTH			0
#define BAD_YEAR			0
#define BAD_HOUR			24
#define BAD_MINUTE			60
#define BAD_SECOND			60

// Define utili

//precisione dell'ElapsedTime
#define	PRECISON_ZERO			1    //espresso in secondi
#define	PRECISON_DEC			10 	 //espresso in decimi di secondo
#define	PRECISON_CENT			100  //espresso in centesimi di secondo
#define	PRECISON_MILL			1000 //espresso in millesimi di secondo 

// return current date in giulian format and year in 19xx format
//----------------------------------------------------------------------------
TB_EXPORT long	TodayDate	();
TB_EXPORT WORD	TodayDay	();
TB_EXPORT WORD	TodayMonth	();
TB_EXPORT WORD 	TodayYear	();
TB_EXPORT long	TodayTime	();
TB_EXPORT int	TodayHour	();
TB_EXPORT int	TodayMinute	();
TB_EXPORT int	TodaySecond	();

//----------------------------------------------------------------------------
TB_EXPORT WORD	DayOfWeek	(const DBTIMESTAMP&);
TB_EXPORT int	WeekOfMonth	(const DBTIMESTAMP&);
TB_EXPORT int	WeekOfMonthISO(const DataDate& nDate);
TB_EXPORT int	WeekOfYear	(const DBTIMESTAMP&);
TB_EXPORT int	DayOfYear	(const DBTIMESTAMP&);

TB_EXPORT WORD	MonthDays	(WORD aMonth, WORD aYear);

// return the week day from a date in giulian date
TB_EXPORT WORD WeekDay (long aGiulianDate);

//----------------------------------------------------------------------------
TB_EXPORT CString WeekDayName		(long aGiulianDate);
TB_EXPORT CString MonthName			(WORD nMonth);
TB_EXPORT CString ShortMonthName	(WORD nMonth);

//----------------------------------------------------------------------------
// return the Giulian format of a dd mm yyyy date
TB_EXPORT long GetGiulianDate(WORD nDay, WORD nMonth, WORD nYear);
TB_EXPORT long GetGiulianDate(const DBTIMESTAMP&);

// return the dd mm yyyy format of a Giulian date
//----------------------------------------------------------------------------
TB_EXPORT BOOL GetShortDate(WORD& nDay, WORD& nMonth, WORD& nYear, long aDate);

// return the hh min sec a partire dal numerototale di seconfi
//----------------------------------------------------------------------------
TB_EXPORT BOOL GetShortTime(WORD& nHour, WORD& nMinute, WORD& nSecond, long nTotalSeconds);

// return 1 if aYear is an intercalary year otherwise 0
//----------------------------------------------------------------------------
TB_EXPORT WORD Intercalary (WORD aYear);

// return the total of seconds format of a TimeStamp
//----------------------------------------------------------------------------
TB_EXPORT long GetTotalSeconds(const DBTIMESTAMP&);

//----------------------------------------------------------------------------
TB_EXPORT BOOL IsNullDate	(const DBTIMESTAMP&, BOOL bIsATime = FALSE);
TB_EXPORT BOOL IsNullDate	(const SYSTEMTIME&, BOOL bIsATime = FALSE);

// Tests if the parameters could be a date
TB_EXPORT BOOL CheckDate	(const DBTIMESTAMP&);

// ElapsedTime return the dd hh mm ss format of a string ElapsedTime
//----------------------------------------------------------------------------
class DataLng;
TB_EXPORT BOOL GetElapsedTime(DataLng& anElapsedTime, LPCTSTR pszElapsedTime, int nTimeFormat, LPCTSTR pszTimeSeparator, LPCTSTR pszDecSeparator);
TB_EXPORT BOOL GetElapsedTime(DataLng& anElapsedTime, LPCTSTR pszElapsedTime, int nFormatIdx, CTBNamespace* pFormatContext);

//----------------------------------------------------------------------------
// return the DBTIMESTAMP of a string date as set in Formatter
TB_EXPORT BOOL GetTimeStamp(DBTIMESTAMP& nDateTime, LPCTSTR pszDateTime, int nFormatIdx, CTBNamespace* pFormatContext = NULL);
// return the DBTIMESTAMP of a string date giving a specific format
TB_EXPORT BOOL GetTimeStamp
	(
		DBTIMESTAMP& nDateTime, LPCTSTR pszDate,
		int			nDateFormat,
		int			nDayFormat,	
		int			nMonthFormat,
		int			nYearFormat,
		LPCTSTR		pszFirstSeparator,
		LPCTSTR		pszSecondSeparator,
		int			nTimeFormat,
		LPCTSTR		pszTimeSeparator,
		LPCTSTR		pszAM,
		LPCTSTR		pszPM
	);


// format date as string using formatter settings
//----------------------------------------------------------------------------
TB_EXPORT CString FormatDate(WORD nDay, WORD nMonth, WORD nYear, int nFormatIdx = -1, CTBNamespace* pFormatContext = NULL);
TB_EXPORT CString FormatDate(const DBTIMESTAMP&, int nFormatIdx, CTBNamespace* pFormatContext = NULL);

// format date as string using standard ISO 8601 date/time format 
// of YYYYMMDDTHHMMSSZ, where the T indicates the split between date 
// and time, and the optional Z indicates that the event uses the 
// Universal Coordinated Time (UTC) zone, or Greenwich Mean Time. 
//----------------------------------------------------------------------------
TB_EXPORT CString FormatDateTimeForXML(const DBTIMESTAMP&, BOOL bSoapType = FALSE);
TB_EXPORT CString FormatDateTimeForXML(const SYSTEMTIME&, BOOL bSoapType = FALSE);
TB_EXPORT CString FormatDateForXML	(const DBTIMESTAMP&);
TB_EXPORT CString FormatDateForXML	(const SYSTEMTIME&);
TB_EXPORT CString FormatTimeForXML	(const DBTIMESTAMP&);
TB_EXPORT CString FormatTimeForXML	(const SYSTEMTIME&);

TB_EXPORT BOOL GetDD_MM_YYYY	(WORD& nDay, WORD& nMonth, WORD& nYear, LPCTSTR pszDate);
TB_EXPORT BOOL GetYYYYMMDD		(WORD& nDay, WORD& nMonth, WORD& nYear, LPCTSTR pszDate);

// General Functions
//-----------------------------------------------------------------------------
TB_EXPORT UWORD			AFXAPI AfxGetApplicationDay		();
TB_EXPORT UWORD			AFXAPI AfxGetApplicationMonth	();
TB_EXPORT SWORD			AFXAPI AfxGetApplicationYear	();
TB_EXPORT DataDate		AFXAPI AfxGetApplicationDate	();
TB_EXPORT DataDate		AFXAPI AfxGetFirstOfTheYear		();
TB_EXPORT DataDate		AFXAPI AfxGetEndOfTheYear		();
TB_EXPORT DataDate		AFXAPI AfxGetSystemDate			();
//=============================================================================        

#include "endh.dex"
