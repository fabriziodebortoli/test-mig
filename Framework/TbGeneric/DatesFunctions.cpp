
#include "stdafx.h"

#include <TbNameSolver\Chars.h>
#include <TbNameSolver\LoginContext.h>
#include <TbNameSolver\ThreadContext.h>
#include <TbNameSolver\Diagnostic.h>

#include <TbGeneric\TbStrings.h>
#include <TbGeneric\GeneralFunctions.h>
#include <TbGeneric\DataObj.h>
#include <TbGeneric\DataTypesFormatters.h>
#include <TbGeneric\SettingsTable.h>
#include <TbGeneric\ParametersSections.h>

#include "DatesFunctions.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

static const TCHAR szAppYear[] = _T("AppYear");
//------------------------------------------------------------------------------
UWORD	AFXAPI AfxGetApplicationDay		() 
{ 
	CLoginContext* pContext = AfxGetLoginContext();
	return (pContext == AfxGetThread())
		? pContext->GetOperationsDay()
		: AfxGetThreadContext()->GetOperationsDay(); 
}
//------------------------------------------------------------------------------
UWORD	AFXAPI AfxGetApplicationMonth	() 
{ 
	CLoginContext* pContext = AfxGetLoginContext();
	return (pContext == AfxGetThread())
		? pContext->GetOperationsMonth()
		: AfxGetThreadContext()->GetOperationsMonth(); 
}
//------------------------------------------------------------------------------
SWORD	AFXAPI AfxGetApplicationYear	() 
{ 
	CLoginContext* pContext = AfxGetLoginContext();
	return (pContext == AfxGetThread())
		? pContext->GetOperationsYear()
		: AfxGetThreadContext()->GetOperationsYear(); 
}

//------------------------------------------------------------------------------
DataDate AFXAPI AfxGetApplicationDate	() 
{ 
	return DataDate
		(
			AfxGetApplicationDay(),
			AfxGetApplicationMonth(),
			AfxGetApplicationYear()
		);
}

//------------------------------------------------------------------------------
TB_EXPORT DataDate AFXAPI AfxGetFirstOfTheYear	()
{
	return DataDate(1, 1, AfxGetApplicationYear());
}

//------------------------------------------------------------------------------
TB_EXPORT DataDate AFXAPI AfxGetEndOfTheYear	()
{
	return DataDate(31, 12, AfxGetApplicationYear());
}

//------------------------------------------------------------------------------
TB_EXPORT DataDate AFXAPI AfxGetSystemDate	()
{
	return DataDate(CTime::GetTickCount());
}


/////////////////////////////////////////////////////////////////////////////
//						Dates API's
/////////////////////////////////////////////////////////////////////////////

//----------------------------------------------------------------------------
/*
This program calculates the day of the week, the week-number and the corrected year.
 ISO 2015 specifies that every week runs from Monday to Sunday.
 Week 1 is the week with the first Thursday of the year. 
 The next week has number 2, etc. 
A consequence of this numbering scheme is that the last few days of a year may belong
 to week 1 of the next year. Also, the first few days of a year may belong to week 52 
 or 53 of the preceding year. 
 *
 * weeknumber:		Compute week number from the date.
 * arguments:
 * int year:		Four digit year number.
 * int month:		Two digit month number.
 * int day:		Number of day of the month.
 * returns:		Int, the weeknumber.
 * remark:		If in month 1 the returned week number is 52 or 53,
 *			the day belonged to the last week of the previous year
 *			(the year-number should be decremented).
 *			If in month 12 the returned week number is 1, the day
 *			belonged to the first week of the next year (the year-
 *			number should be incremented).
 */
//int WeekNumber(int year, int month, int day)
//{
//    long daynumber;
//    long firstdaynumber;
//    long lastdaynumber;
//    /*
//     * This code is incorrect for dates before March 1 1900 or after
//     * February 28 2100 (or some such range)...
//     * This is not considered a bug.
//     */
//    daynumber = 4L + 365L * ((long) year) +
//	    31L * (((long) month) - 1L) +
//	    ((long) ((year - 1) / 4)) +
//	    (long) (day);
//    if((month > 2) && (year % 4 == 0))
//	daynumber++;			/* after leapday */
//    if((month > 2) && (month < 8))
//	daynumber -= (3L + ((month - 3) / 2));
//    else if(month > 7)
//	daynumber -= (3L + ((month - 4) / 2));
//    firstdaynumber = 5L + 365L * ((long) year) +
//	    ((long) ((year - 1) / 4));
//    lastdaynumber = 4L + 365L * ((long) (year + 1)) +
//	    ((long) ((year) / 4));
//
//    if(((lastdaynumber % 7L) <= 2L) &&
//	    ((lastdaynumber - daynumber) <= (lastdaynumber % 7L)))
//	return (1);/* wrap to 1st week next year */
//    else if((firstdaynumber % 7L) <= 3L)/* This year starts Mo..Th */
//	return ((daynumber - firstdaynumber / 7L * 7L) / 7L + 1L);
//    else if(daynumber < (firstdaynumber + 7L - firstdaynumber % 7L))
//    {
//	/*
//	 * Day daynumber belongs to last week of previous year.
//	 */
//	firstdaynumber = 4L + 365L * ((long) (year - 1)) +
//		((long) ((year - 1) / 4)) + 1L;
//	if((firstdaynumber % 7L) <= 3L)/* Previous year starts Mo..Th */
//	    return ((daynumber - firstdaynumber / 7L * 7L) / 7L + 1L);
//	else
//	    return ((daynumber - firstdaynumber / 7L * 7L) / 7L);
//    }
//    else
//	return ((daynumber - firstdaynumber / 7L * 7L) / 7L);
//}

//----------------------------------------------------------------------------
WORD DayOfWeek(const DBTIMESTAMP& tmsDate)
{
	return (WORD)((GetGiulianDate(tmsDate) + MIN_WEEK_DAY - 1) % 7);
}

// return the week number of month
//----------------------------------------------------------------------------
int WeekOfMonth(const DBTIMESTAMP& tmsDate)
{
	DBTIMESTAMP tms = tmsDate;
	tms.day = 1;	//primo del mese

	long nFirst = ::GetGiulianDate(tms);
	long nToday = ::GetGiulianDate(tmsDate);

	//if( tmsDate.day < 7 && DayOfWeek(tmsDate) >= DayOfWeek(tms))
	int n = DayOfWeek(tms);
	if( tmsDate.day <= (7-DayOfWeek(tms))  && DayOfWeek(tms) > 0)
	{
		if(tms.month > 1)
		{
			tms.month--;	//settimana iniziata nel mese precedente
		}
		else
		{
			tms.year--;	//settimana iniziata nel mese precedente
			tms.month = 12;	//dell'anno precedente
		}

		nFirst = ::GetGiulianDate(tms);
	}
	if(DayOfWeek(tms) > 0)
		nFirst += (7-DayOfWeek(tms)); //giorni della settimana precedente

	return (nToday - nFirst) / 7+1;
}

//-----------------------------------------------------------------------------
int WeekOfMonthISO(const DataDate& nDate)
{
	int monthDays = MonthDays(nDate.Month(), nDate.Year());
	DataDate nFoM = DataDate(1, nDate.Month(), nDate.Year());	// First day of Month
	long loMGD = nDate.GiulianDate();	// Date in Giulian notation
	long foMGD = nFoM.GiulianDate();	// First day of Month in Giulian notation
	WORD weekDay;

	// Se l'ultimo giorno del mese e' > di mercoledi allora la settimana finisce 
	// la domenica del mese successivo
	// Altrimenti l'ultima settimana appartiene al mese successivo e quindi non la considero.
	weekDay = WeekDay(loMGD);
	if (weekDay > 2)
		loMGD += (6 - weekDay);
	else
		loMGD -= (weekDay + 1);

	// Se il primo giorno del mese e' <= di giovedi, allora la settimana inizia il mese prima
	// Altrimenti la prima settimana del mese inizia il primo lunedi che trovo

	weekDay = WeekDay(foMGD);
	if (weekDay <=3)
		foMGD -= weekDay;
	else
		foMGD += (7 - weekDay);

	int WoM = (loMGD - foMGD)/7 + 1;

	return WoM;
}

// return the week number of year
//----------------------------------------------------------------------------
int WeekOfYear(const DBTIMESTAMP& tmsDate)
{
	DBTIMESTAMP tms = tmsDate;
	tms.day = 1;
	tms.month = 1;	//primo dell'anno

	long nFirst = ::GetGiulianDate(tms);
	long nToday = ::GetGiulianDate(tmsDate);

	int n = DayOfWeek(tms);
	if (n > 3)
		n = n - 7;

	int nDays = nToday - nFirst + n;
	if (nDays < 0)
	{
		tms.day = 31;
		tms.month = 12;
		tms.year -= 1;
		return WeekOfYear(tms);
	}
	return nDays / 7 + 1;
}

// return the week number of year
//----------------------------------------------------------------------------
int DayOfYear(const DBTIMESTAMP& tmsDate)
{
	DBTIMESTAMP tms = tmsDate;
	tms.day = 1;
	tms.month = 1;	//primo dell'anno
	
	long nFirst = ::GetGiulianDate(tms);
	long nToday = ::GetGiulianDate(tmsDate);

	return (nToday - nFirst + 1);
}

// return the Giulian format of a TimeStamp
//----------------------------------------------------------------------------
long GetGiulianDate(const DBTIMESTAMP& aDateTime)
{
	if (!CheckDate(aDateTime))
		return BAD_DATE;

	// NULL DATE
	if (IsNullDate(aDateTime))
		return 0L;
		
	long date =	(long) (365 * (long) ((int)aDateTime.year - (MIN_YEAR + 1)))	+
				(long) (((int)aDateTime.year - (MIN_YEAR + 2)) / 4)		-
				(long) (((int)aDateTime.year - (MIN_YEAR + 2)) / 100)		+
				(long) (((int)aDateTime.year - 1601) / 400);

	for (WORD i = 1; i < aDateTime.month; i++)
		date += MonthDays(i, aDateTime.year);

	return date + aDateTime.day;
}

// Tests if the parameters could be a date
//----------------------------------------------------------------------------
BOOL CheckDate (const DBTIMESTAMP& aDateTime)
{            
	if (IsNullDate(aDateTime))
		return TRUE;

	return
		aDateTime.hour >= 0		&& aDateTime.hour <= 23		&&
		aDateTime.minute >= 0	&& aDateTime.minute <= 59	&&
		aDateTime.second >= 0	&& aDateTime.second <= 59	&&
		(
			(
				aDateTime.day		== MIN_TIME_DAY		&&
				aDateTime.month		== MIN_TIME_MONTH	&&
				aDateTime.year		== MIN_TIME_YEAR
			) ||
			(
				aDateTime.year > MIN_YEAR && aDateTime.year <= MAX_YEAR &&
				aDateTime.month >= 1 && aDateTime.month <= 12 &&
				aDateTime.day >= 1 &&
				aDateTime.day <= MonthDays(aDateTime.month, aDateTime.year)
			)
		);
}

//----------------------------------------------------------------------------
BOOL IsNullDate(const DBTIMESTAMP& aDateTime, BOOL bIsATime /*FALSE*/)
{
	// corretta an. su controllo IsNullDate in caso di time
	// la data di min in caso di Time e' differente da quella di data piena
	if	(	bIsATime && 
			aDateTime.day	== MIN_TIME_DAY		&&
			aDateTime.month == MIN_TIME_MONTH	&&
			aDateTime.year	== MIN_TIME_YEAR		&&
			aDateTime.hour	== MIN_HOUR		&&
			aDateTime.minute == MIN_MINUTE	&&
			aDateTime.second == MIN_SECOND
		)
			return TRUE;
	return
		aDateTime.day		== MIN_DAY		&&
		aDateTime.month		== MIN_MONTH	&&
		aDateTime.year		== MIN_YEAR		&&
		aDateTime.hour		== MIN_HOUR		&& 
		aDateTime.minute	== MIN_MINUTE	&&
		aDateTime.second	== MIN_SECOND;
}

//----------------------------------------------------------------------------
BOOL IsNullDate(const SYSTEMTIME& aSystemTime, BOOL bIsATime /*FALSE*/)
{
	// corretta an. su controllo IsNullDate in caso di time
	// la data di min in caso di Time e' differente da quella di data piena
	if (bIsATime &&
		aSystemTime.wDay == MIN_TIME_DAY		&&
		aSystemTime.wMonth == MIN_TIME_MONTH	&&
		aSystemTime.wYear == MIN_TIME_YEAR	&&
		aSystemTime.wHour == MIN_HOUR		&&
		aSystemTime.wMinute == MIN_MINUTE	&&
		aSystemTime.wSecond == MIN_SECOND
		)
		return TRUE;

	return
		aSystemTime.wDay	== MIN_DAY		&&
		aSystemTime.wMonth	== MIN_MONTH	&&
		aSystemTime.wYear	== MIN_YEAR		&&
		aSystemTime.wHour	== MIN_HOUR		&& 
		aSystemTime.wMinute	== MIN_MINUTE	&&
		aSystemTime.wSecond	== MIN_SECOND;
}


// return last day of a month of a year
//----------------------------------------------------------------------------
WORD MonthDays (WORD wMonth, WORD wYear)
{
	if (wMonth == 2)
		return WORD((int)Intercalary(wYear) + 28);

	return ((((int)wMonth - 1) % 7) % 2) ? (WORD)30 : (WORD)31;
}
// E` bisestile se divisibile per 400 oppure se divisible per 4 ma non per 100
// ritorna 1 se e` bisestile altrimenti 0
//----------------------------------------------------------------------------
WORD Intercalary (WORD wYear)
{
	BOOL b1 = ((int)wYear % 4) == 0;
	BOOL b2 = ((int)wYear % 100) != 0;
	BOOL b3 = ((int)wYear % 400) == 0;

	return (b1 && b2) || b3 ? (WORD)1 : (WORD)0;

}

// return the total of seconds format of a TimeStamp
//----------------------------------------------------------------------------
long GetTotalSeconds(const DBTIMESTAMP& aDateTime)
{
	if (!CheckDate(aDateTime))
		return BAD_TIME;

	return aDateTime.hour * 3600 + aDateTime.minute * 60 + aDateTime.second;
}

//----------------------------------------------------------------------------
CString ShortMonthName (WORD wMonth)
{
	switch 	(wMonth)
	{
		case 1 : return _TB("Jan");
		case 2 : return _TB("Feb");	
		case 3 : return _TB("Mar");	
		case 4 : return _TB("Apr");	
		case 5 : return _TB("May");	
		case 6 : return _TB("Jun");	
		case 7 : return _TB("Jul");	
		case 8 : return _TB("Aug");	
		case 9 : return _TB("Sep");	
		case 10 : return _TB("Oct");	
		case 11 : return _TB("Nov");	
		case 12 : return _TB("Dec");	
	}              
	
	return _T("");
}

//----------------------------------------------------------------------------
CString WeekDayName(long nDate)
{
	CString strTmp;
                           
	switch 	(WeekDay(nDate))
	{
		case 0 : strTmp = _TB("Monday");	break;
		case 1 : strTmp = _TB("Tuesday");	break;
		case 2 : strTmp = _TB("Wednesday");	break;
		case 3 : strTmp = _TB("Thursday");	break;
		case 4 : strTmp = _TB("Friday");	break;
		case 5 : strTmp = _TB("Saturday");	break;
		case 6 : strTmp = _TB("Sunday");	break;
	}              
	
	return strTmp;
}

// return the week day from a Giulian date:
// 0 = Monday; ... 6 = Sunday;
//----------------------------------------------------------------------------
WORD WeekDay(long nDate)
{
	return (WORD)((nDate + MIN_WEEK_DAY - 1) % 7);
}
// return the Giulian format of a dd mm yyyy date
//----------------------------------------------------------------------------
long GetGiulianDate(WORD wDay, WORD wMonth, WORD wYear)
{
	DBTIMESTAMP aDateTime;
	aDateTime.day		= wDay;
	aDateTime.month		= wMonth;
	aDateTime.year		= wYear;
	aDateTime.hour		= 0;
	aDateTime.minute	= 0;
	aDateTime.second	= 0;
	aDateTime.fraction	= 0;

	return GetGiulianDate(aDateTime);
}                           


//-----------------------------------------------------------------------------
long TodayDate()
{
	SYSTEMTIME	today;
	::GetLocalTime (&today);

	return GetGiulianDate(today.wDay, today.wMonth, today.wYear);
}

//-----------------------------------------------------------------------------
WORD TodayDay()
{
	SYSTEMTIME	today;
	::GetLocalTime (&today);

	return (WORD)today.wDay;
}
//-----------------------------------------------------------------------------
WORD TodayMonth()
{
	SYSTEMTIME	today;
	::GetLocalTime (&today);

	return (WORD)today.wMonth;
}

// format 19xx
//-----------------------------------------------------------------------------
WORD TodayYear()
{
	SYSTEMTIME	today;
	::GetLocalTime (&today);

	return (WORD)today.wYear;
}

//-----------------------------------------------------------------------------
long TodayTime()
{
	SYSTEMTIME	today;
	::GetLocalTime (&today);

	return (int)today.wHour * 3600 + (int)today.wMinute * 60 + (int)today.wSecond;
}

// ritorna l'ora del time di sistema
//-----------------------------------------------------------------------------
int TodayHour()
{
	SYSTEMTIME	today;
	::GetLocalTime (&today);

	return (int)today.wHour;
}

// ritorna il minuto del time di sistema
//-----------------------------------------------------------------------------
int TodayMinute()
{
	SYSTEMTIME	today;
	::GetLocalTime (&today);

	return (int)today.wMinute;
}

// ritorna il minuto del time di sistema
//-----------------------------------------------------------------------------
int TodaySecond()
{
	SYSTEMTIME	today;
	::GetLocalTime (&today);

	return (int)today.wSecond;
}


#define	MAX_TOTAL_SECONDS 86399L		// 23L * 3600L + 59L * 60L + 59L
//-----------------------------------------------------------------------------
BOOL GetShortTime(WORD& nHour, WORD& nMinute, WORD& nSecond, long nTotalSeconds)
{
	if (nTotalSeconds < 0 || nTotalSeconds > MAX_TOTAL_SECONDS)
	{
		nHour	= 0;
		nMinute = 0;
		nSecond = 0;
		return FALSE;
	}

	nHour = WORD(nTotalSeconds / 3600L);
	long nHourRemainder = nTotalSeconds % 3600L;

	nMinute = (WORD) (nHourRemainder / 60L);
	nSecond = (WORD) (nHourRemainder % 60L);

	return TRUE;
}

// return the dd mm yyyy format of a Giulian date
//
//----------------------------------------------------------------------------
BOOL GetShortDate(WORD& wDay, WORD& wMonth, WORD& wYear, long nDate)
{
	if (nDate < MIN_GIULIAN_DATE || nDate > MAX_GIULIAN_DATE)
	{
		wDay	= MIN_DAY;
		wMonth	= MIN_MONTH;
		wYear	= MIN_YEAR;
		
		return nDate == 0;
	}
	
	long r = 0;
	wYear = (WORD) ((nDate / 365.25) + (MIN_YEAR + 1));

	do {
		wYear += WORD(r / 365.25);
		r = nDate -
			(
				(long) (365L * ((int)wYear - (MIN_YEAR + 1)))	+
				(long) (((int)wYear - (MIN_YEAR + 2)) / 4L)	-
				(long) (((int)wYear - (MIN_YEAR + 2)) / 100L)	+
				(long) (((int)wYear - 1601L) / 400L)
			);

	} while (r > (long) (365 + (int)Intercalary(wYear)));
       
    wMonth = 1;
    WORD nDays;
    
    while ((long)(nDays = MonthDays(wMonth, wYear)) < r) 
	{
		wMonth++;
		r -= nDays;
	}

	wDay = (WORD) r;
	
	return TRUE;
}

//@@ElapsedTime return the dd hh mm ss format of a string time span
//----------------------------------------------------------------------------
BOOL GetElapsedTime(DataLng& anElapsedTime, LPCTSTR pszElapsedTime, int nFormatIdx, CTBNamespace* pFormatContext)
{
	anElapsedTime.SetAsTime();

	CElapsedTimeFormatter* pFormatter = NULL;
	
	if (nFormatIdx >= 0)
		pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(nFormatIdx, pFormatContext);

	if (pFormatter == NULL)
		pFormatter = (CElapsedTimeFormatter*) AfxGetFormatStyleTable()->GetFormatter(DataType::ElapsedTime, pFormatContext);

	if (pFormatter == NULL)
		return GetElapsedTime(anElapsedTime, pszElapsedTime, CElapsedTimeFormatHelper::TIME_DHMS, _T(":"), NULL);

		// use international format
	return GetElapsedTime
	(
		anElapsedTime, pszElapsedTime, 
		pFormatter->GetFormat(), 
 		pFormatter->GetTimeSeparator(),
 		pFormatter->GetDecSeparator()
	);
}

//@@ElapsedTime return the dd hh mm ss format of a string time span 
//----------------------------------------------------------------------------
BOOL GetElapsedTime
	(
		DataLng&	anElapsedTime,
		LPCTSTR		pszElapsedTime,
		int			nTimeFormat,
		LPCTSTR		pszTimeSeparator,
		LPCTSTR		pszDecSeparator
	)
{
	BOOL bSign = FALSE;

	anElapsedTime.SetAsTime();
	anElapsedTime = 0;

	CString	strElapsedTime(pszElapsedTime);
	strElapsedTime.Trim();

	if (strElapsedTime.IsEmpty())
		return TRUE;

	if (strElapsedTime[0] == MINUS_CHAR)
	{
		strElapsedTime = strElapsedTime.Mid(1);
		bSign = TRUE;
	}
	
	LPCTSTR pszSep	= NULL;
	int nSepLen		= 0;
	int	nSepPos		= 0;
	long nDays		= 0;
	long nHours		= 0;
	long nMinutes	= 0;
	double nSeconds	= 0;

	if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_D) == CElapsedTimeFormatHelper::TIME_D)
	{
		// Giorni

		if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_H) == CElapsedTimeFormatHelper::TIME_H)
		{
			pszSep = ((nTimeFormat & (CElapsedTimeFormatHelper::TIME_DHMS | CElapsedTimeFormatHelper::TIME_C)) == CElapsedTimeFormatHelper::TIME_DCD)
				? pszDecSeparator
				: pszTimeSeparator;

			nSepLen = pszSep ? _tcslen(pszSep) : 0;

			
			if (pszSep)	// DHMS, DHM, DH, DHMCM, DHCH, DCD
			{
				nSepPos = strElapsedTime.Find(pszSep);
				if (nSepPos < 0)
				{
					nDays = _ttol(strElapsedTime);
					anElapsedTime.Assign(nDays, 0, 0, 0);
					if (bSign)
						anElapsedTime = - (long) anElapsedTime;
					return TRUE;
				}
			}
			else
				switch (anElapsedTime.GetElapsedTimePrecision())
				{
					case PRECISON_ZERO	: nSepPos = FMT_ELAPSED_TIME_D_LEN;		break;
					case PRECISON_DEC	: nSepPos = FMT_ELAPSED_TIME_D_LEN - 1;	break;
					case PRECISON_CENT	: nSepPos = FMT_ELAPSED_TIME_D_LEN - 2;	break;
					case PRECISON_MILL	: nSepPos = FMT_ELAPSED_TIME_D_LEN - 3;	break;
					default				: nSepPos = FMT_ELAPSED_TIME_D_LEN;		break;
				}
		
			nDays = _ttol(strElapsedTime.Left(nSepPos));
			strElapsedTime = strElapsedTime.Right(strElapsedTime.GetLength() - nSepPos - nSepLen);
		}
		else
			nDays = _ttol(strElapsedTime);	// D
	}

	if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_H) == CElapsedTimeFormatHelper::TIME_H)
	{
		// Ore
		if ((nTimeFormat & (CElapsedTimeFormatHelper::TIME_DHMS | CElapsedTimeFormatHelper::TIME_C)) == CElapsedTimeFormatHelper::TIME_DCD)
		{
			double nVal = _tstof(CString("0.") + strElapsedTime) * 24.0;
			nHours = (long) nVal;
			nVal -= nHours;
			nVal *= 60.0;
			nMinutes = (long) nVal;
			nVal -= nMinutes;
			nVal *= 60.0;
			
			anElapsedTime.Assign(nDays, nHours, nMinutes, nVal);
			if (bSign)
				anElapsedTime = - (long) anElapsedTime;

			return TRUE;	// DCD
		}

		if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_M) == CElapsedTimeFormatHelper::TIME_M)
		{
			pszSep = ((nTimeFormat & (CElapsedTimeFormatHelper::TIME_HMS | CElapsedTimeFormatHelper::TIME_C)) == CElapsedTimeFormatHelper::TIME_HCH)
				? pszDecSeparator
				: pszTimeSeparator;

			nSepLen = pszSep ? _tcslen(pszSep) : 0;

			if (pszSep)	// DHMS, DHM, DH, DHMCM, DHCH
			{
				nSepPos = strElapsedTime.Find(pszSep);
				if (nSepPos < 0)
				{
					nHours = _ttol(strElapsedTime);
					anElapsedTime.Assign(nDays, nHours, 0, 0);
					if (bSign)
						anElapsedTime = - (long) anElapsedTime;

					return TRUE;
				}
			}
			else
				if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_D) == CElapsedTimeFormatHelper::TIME_D)
					nSepPos = 2;	// DHMS, DHM, HMS, HM
				else
					switch (anElapsedTime.GetElapsedTimePrecision())
					{
						case PRECISON_ZERO	: nSepPos = FMT_ELAPSED_TIME_H_LEN;		break;
						case PRECISON_DEC	: nSepPos = FMT_ELAPSED_TIME_H_LEN - 1;	break;
						case PRECISON_CENT	: nSepPos = FMT_ELAPSED_TIME_H_LEN - 2;	break;
						case PRECISON_MILL	: nSepPos = FMT_ELAPSED_TIME_H_LEN - 3;	break;
						default				: nSepPos = FMT_ELAPSED_TIME_H_LEN;		break;
					}

			nHours = _ttol(strElapsedTime.Left(nSepPos));
			strElapsedTime = strElapsedTime.Right(strElapsedTime.GetLength() - nSepPos - nSepLen);
		}
		else
			nHours = _ttol(strElapsedTime);	// DH, H

		if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_D) == CElapsedTimeFormatHelper::TIME_D && nHours > 23)
			return FALSE;
	}

	if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_M) == CElapsedTimeFormatHelper::TIME_M)
	{
		// Minuti
		if ((nTimeFormat & (CElapsedTimeFormatHelper::TIME_HMS | CElapsedTimeFormatHelper::TIME_C)) == CElapsedTimeFormatHelper::TIME_HCH)
		{
			double nVal = _tstof(CString("0.") + strElapsedTime) * 60.0;
			nMinutes = (long) nVal;
			nVal -= nMinutes;
			nVal *= 60.0;
			
			anElapsedTime.Assign(nDays, nHours, nMinutes, nVal);
			if (bSign)
				anElapsedTime = - (long) anElapsedTime;

			return TRUE;	// DHCH, HCH
		}

		if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_S) == CElapsedTimeFormatHelper::TIME_S)
		{
			pszSep = ((nTimeFormat & (CElapsedTimeFormatHelper::TIME_MSEC | CElapsedTimeFormatHelper::TIME_C)) == CElapsedTimeFormatHelper::TIME_MCM)
				? pszDecSeparator
				: pszTimeSeparator;

			nSepLen = pszSep ? _tcslen(pszSep) : 0;

			if (pszSep)		// DHMS, HMS, MS, DHMCM, HMCM, MCM
			{
				nSepPos = strElapsedTime.Find(pszSep);
				if (nSepPos < 0)
				{
					nMinutes = _ttol(strElapsedTime);
					anElapsedTime.Assign(nDays, nHours, nMinutes, 0);
					if (bSign)
						anElapsedTime = - (long) anElapsedTime;

					return TRUE;
				}
			}
			else
				if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_H) == CElapsedTimeFormatHelper::TIME_H)
					nSepPos = 2;	// DHMS, HMS, MS, DHMCM, HMCM, MCM
				else
					switch (anElapsedTime.GetElapsedTimePrecision())
					{
						case PRECISON_ZERO	: nSepPos = FMT_ELAPSED_TIME_M_LEN;		break;
						case PRECISON_DEC	: nSepPos = FMT_ELAPSED_TIME_M_LEN - 1;	break;
						case PRECISON_CENT	: nSepPos = FMT_ELAPSED_TIME_M_LEN - 2;	break;
						case PRECISON_MILL	: nSepPos = FMT_ELAPSED_TIME_M_LEN - 3;	break;
						default				: nSepPos = FMT_ELAPSED_TIME_M_LEN;		break;
					}

			nMinutes = _ttol(strElapsedTime.Left(nSepPos));
			strElapsedTime = strElapsedTime.Right(strElapsedTime.GetLength() - nSepPos - nSepLen);
		}
		else
			nMinutes = _ttol(strElapsedTime);	// DHM, HM, M

		if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_H) == CElapsedTimeFormatHelper::TIME_H && nMinutes > 59)
			return FALSE;
	}

	if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_S) == CElapsedTimeFormatHelper::TIME_S)
	{
		// Secondi
		if ((nTimeFormat & (CElapsedTimeFormatHelper::TIME_MSEC | CElapsedTimeFormatHelper::TIME_C)) == CElapsedTimeFormatHelper::TIME_MCM)
		{
			double nVal = _tstof(CString("0.") + strElapsedTime) * 60.0;
			
			anElapsedTime.Assign(nDays, nHours, nMinutes, nVal);
			if (bSign)
				anElapsedTime = - (long) anElapsedTime;

			return TRUE;	// DHMCM, HMCM, MCM
		}

		// DHMS, HMS, MS, S, DHMSF, HMSF, MSF, SF

		if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_F) == CElapsedTimeFormatHelper::TIME_F)
		{
			nSepLen = pszDecSeparator ? _tcslen(pszDecSeparator) : 0;

			if (pszDecSeparator)		// DHMSF, HMSF, MSF, SF
			{
				nSepPos = strElapsedTime.Find(pszDecSeparator);
				if (nSepPos < 0)
				{
					nSeconds = _ttol(strElapsedTime);
					anElapsedTime.Assign(nDays, nHours, nMinutes, nSeconds);
					if (bSign)
						anElapsedTime = - (long) anElapsedTime;

					return TRUE;
				}
			}
			else
				if ((nTimeFormat & CElapsedTimeFormatHelper::TIME_M) == CElapsedTimeFormatHelper::TIME_M)
					nSepPos = 2;	// DHMS, HMS, MSF
				else
					switch (anElapsedTime.GetElapsedTimePrecision())
					{
						case PRECISON_ZERO	: nSepPos = FMT_ELAPSED_TIME_S_LEN;		break;
						case PRECISON_DEC	: nSepPos = FMT_ELAPSED_TIME_S_LEN - 1;	break;
						case PRECISON_CENT	: nSepPos = FMT_ELAPSED_TIME_S_LEN - 2;	break;
						case PRECISON_MILL	: nSepPos = FMT_ELAPSED_TIME_S_LEN - 3;	break;
						default				: nSepPos = FMT_ELAPSED_TIME_S_LEN;		break;
					}

				nSeconds = _ttol(strElapsedTime.Left(nSepPos));
				strElapsedTime = strElapsedTime.Right(strElapsedTime.GetLength() - nSepPos - nSepLen);
				nSeconds +=  _tstof(CString("0.") + strElapsedTime);
		}
		else
			nSeconds = _tstof(strElapsedTime);

		if	(
				(nTimeFormat & CElapsedTimeFormatHelper::TIME_M) == CElapsedTimeFormatHelper::TIME_M &&
				(nTimeFormat & CElapsedTimeFormatHelper::TIME_F) != CElapsedTimeFormatHelper::TIME_F &&
				nSeconds > 59
			)
			return FALSE;

		anElapsedTime.Assign(nDays, nHours, nMinutes, nSeconds);
		if (bSign)
			anElapsedTime = - (long) anElapsedTime;

		return TRUE;
	}

	//per l'ora centesimale. 1 secondo sessantesimale = 0.0278 secondi centesimali 
	//ovvero un secondo centesimale = 36 secondi sessantesimali
	if (nTimeFormat == CElapsedTimeFormatHelper::TIME_CH)
	{
		//devo modificare il separatore dei decimali utilizzando il "."
		nSepLen = pszDecSeparator ? _tcslen(pszDecSeparator) : 0;
		nSepPos = strElapsedTime.Find(pszDecSeparator);
		if (nSepPos >= 0) 
			strElapsedTime = strElapsedTime.Left(nSepPos) + _T(".") + strElapsedTime.Right(strElapsedTime.GetLength() - nSepPos - nSepLen);		

		double nVal = _tstof(strElapsedTime) * 36.;

		anElapsedTime.Assign(0, 0, 0, nVal);
		if (bSign)
			anElapsedTime = - (long) anElapsedTime;

		return TRUE;
	}

	if	(
			(nTimeFormat & CElapsedTimeFormatHelper::TIME_D) == CElapsedTimeFormatHelper::TIME_D ||
			(nTimeFormat & CElapsedTimeFormatHelper::TIME_H) == CElapsedTimeFormatHelper::TIME_H ||
			(nTimeFormat & CElapsedTimeFormatHelper::TIME_M) == CElapsedTimeFormatHelper::TIME_M
		)
	{
		anElapsedTime.Assign(nDays, nHours, nMinutes, 0);
		if (bSign)
			anElapsedTime = - (long) anElapsedTime;

		return TRUE;
	}

	return FALSE;
}

// return the dd mm yyyy format of a string date
//----------------------------------------------------------------------------
BOOL GetTimeStamp
	(
		DBTIMESTAMP& aDateTime, LPCTSTR pszDate,
		int			nDateFormat,
		int			/* nDayFormat */,
		int			/* nMonthFormat */,
		int			nYearFormat,
		LPCTSTR		pszFirstSeparator,
		LPCTSTR		pszSecondSeparator,
		int			nTimeFormat,
		LPCTSTR		pszTimeSeparator,
		LPCTSTR		pszAM,
		LPCTSTR		pszPM
	)
{      
	aDateTime.day		= MIN_DAY;
	aDateTime.month		= MIN_MONTH;
	aDateTime.year		= MIN_YEAR;
	aDateTime.hour		= MIN_HOUR;
	aDateTime.minute	= MIN_MINUTE;
	aDateTime.second	= MIN_SECOND;
	aDateTime.fraction	= 0;
	
	CString	strDate(pszDate);
	strDate.Trim();

	if (strDate.IsEmpty() || (IsIntNumber(strDate) && _ttol(strDate) == 0))
		return TRUE;		// NULL DATE

	int	nSepPos;

	if ((nTimeFormat & CDateFormatHelper::TIME_ONLY) == CDateFormatHelper::TIME_ONLY)
	{
		aDateTime.day		= MIN_TIME_DAY;
		aDateTime.month		= MIN_TIME_MONTH;
		aDateTime.year		= MIN_TIME_YEAR;
		goto TimeOnly;
	}

	if (strDate.GetLength() < 6)
		goto BadDateLabel;

	if (pszFirstSeparator && *pszFirstSeparator && pszSecondSeparator && *pszSecondSeparator)
	{
 		CString	s1;
		CString	s2;
		CString	s3;

		int nYLen;
		
		nSepPos = strDate.Find(pszFirstSeparator);
		if (nSepPos <= 0) goto BadDateLabel;
		
		s1 = strDate.Left(nSepPos);
	    strDate = strDate.Right(strDate.GetLength() - nSepPos - _tcslen(pszFirstSeparator));
		nSepPos = strDate.Find(pszSecondSeparator);		
		if (nSepPos <= 0) goto BadDateLabel;
		
		s2 = strDate.Left(nSepPos);
	    s3 = strDate.Right(strDate.GetLength() - nSepPos - _tcslen(pszSecondSeparator));
		nSepPos = s3.Find(_T(" "));	// separatore tra data e ora
		if (nSepPos > 0)
		{
			strDate = s3.Right(s3.GetLength() - nSepPos - 1);
			s3 = s3.Left(nSepPos);
		}
		else
			strDate.Empty();

		// default nel secolo corrente se l'anno e' di due cifre
	
		switch (nDateFormat)
		{
			case CDateFormatHelper::DATE_DMY:
				aDateTime.day	= (WORD) _ttoi(s1);
				aDateTime.month	= (WORD) _ttoi(s2);
				if (s3.CompareNoCase(szAppYear) == 0)
					aDateTime.year = AfxGetApplicationYear();
				else
				{
					nYLen			= s3.GetLength();
					aDateTime.year	= (WORD) _ttoi(s3);
					aDateTime.year	+= GET_MILLENIUM(nYLen, aDateTime.year);
				}
				break;
			case CDateFormatHelper::DATE_MDY:
				aDateTime.month	= (WORD) _ttoi(s1);
				aDateTime.day	= (WORD) _ttoi(s2);
				if (s3.CompareNoCase(szAppYear) == 0)
					aDateTime.year = AfxGetApplicationYear();
				else
				{
					nYLen			= s3.GetLength();
					aDateTime.year	= (WORD) _ttoi(s3);
					aDateTime.year	+= GET_MILLENIUM(nYLen, aDateTime.year);
				}
				break;
			case CDateFormatHelper::DATE_YMD:
				if (s1.CompareNoCase(szAppYear) == 0)
					aDateTime.year = AfxGetApplicationYear();
				else
				{
					nYLen			= s1.GetLength();
					aDateTime.year	= (WORD) _ttoi(s1);
					aDateTime.year	+= GET_MILLENIUM(nYLen, aDateTime.year);
				}
				aDateTime.month	= (WORD) _ttoi(s2);
				aDateTime.day	= (WORD) _ttoi(s3);
				break;
		}
 	}
 	else
 	{
 		int nYChr = 0;
 		
 		switch (nYearFormat)
 		{
 			case CDateFormatHelper::YEAR99   : nYChr = 2; break;
 			case CDateFormatHelper::YEAR999  : nYChr = 3; break;
 			case CDateFormatHelper::YEAR9999 : nYChr = 4; break;
 			default : ASSERT_TRACE1(FALSE,"Bad year format: %d",nYearFormat); goto BadDateLabel;
 		}

		switch (nDateFormat)
		{
			case CDateFormatHelper::DATE_DMY:
				aDateTime.day	= (WORD) _ttoi(strDate.Left(2));
				aDateTime.month	= (WORD) _ttoi(strDate.Mid(2, 2));
				aDateTime.year	= (WORD) _ttoi(strDate.Mid(4, nYChr));
				break;
			case CDateFormatHelper::DATE_MDY:
				aDateTime.month	= (WORD) _ttoi(strDate.Left(2));
				aDateTime.day	= (WORD) _ttoi(strDate.Mid(2, 2));
				aDateTime.year	= (WORD) _ttoi(strDate.Mid(4, nYChr));
				break;
			case CDateFormatHelper::DATE_YMD:
				aDateTime.year	= (WORD) _ttoi(strDate.Left(nYChr));
				aDateTime.month	= (WORD) _ttoi(strDate.Mid(nYChr, 2));
				aDateTime.day	= (WORD) _ttoi(strDate.Mid(nYChr + 2, 2));
				break;
 			default : ASSERT_TRACE1(FALSE,"Bad date format: %d",nDateFormat); goto BadDateLabel;
		}

		aDateTime.year	+= GET_MILLENIUM(nYChr, aDateTime.year);

		strDate = strDate.Mid(4 + nYChr);
 	}

TimeOnly:

	if ((nTimeFormat & 0x000F) != CDateFormatHelper::TIME_NONE && !strDate.IsEmpty())
	{
		if (pszTimeSeparator && *pszTimeSeparator)
		{
			int nSepLen = _tcslen(pszTimeSeparator);
			nSepPos = strDate.Find(pszTimeSeparator);
			if (nSepPos <= 0) goto BadDateLabel;
			
			aDateTime.hour	= _ttoi(strDate.Left(nSepPos));

			strDate = strDate.Right(strDate.GetLength() - nSepPos - nSepLen);

			if ((nTimeFormat & CDateFormatHelper::TIME_NOSEC) == CDateFormatHelper::TIME_NOSEC)
				aDateTime.second = 0;
			else
			{
				nSepPos = strDate.Find(pszTimeSeparator);		
				if (nSepPos <= 0) goto BadDateLabel;
			
				aDateTime.minute= _ttoi(strDate.Left(nSepPos));

				strDate = strDate.Right(strDate.GetLength() - nSepPos - nSepLen);
			}

			if ((nTimeFormat & CDateFormatHelper::TIME_AMPM) == CDateFormatHelper::TIME_AMPM)
			{
				nSepPos = strDate.Find(pszAM);
				if (nSepPos < 0)
				{
					nSepPos = strDate.Find(pszPM);
					if (nSepPos > 0)
						aDateTime.hour += 12;
				}

				// quello che c'e` a sinistra sono i secondi (oppure i minuti)
				if (nSepPos > 0)
					strDate = strDate.Left(nSepPos);
			}

			if ((nTimeFormat & CDateFormatHelper::TIME_NOSEC) == CDateFormatHelper::TIME_NOSEC)
				aDateTime.minute = _ttoi(strDate);
			else
				aDateTime.second = _ttoi(strDate);
		}
		else
		{
			aDateTime.hour		= (WORD) _ttoi(strDate.Left(2));
			aDateTime.minute	= (WORD) _ttoi(strDate.Mid(2, 2));
			aDateTime.second	= (WORD) _ttoi(strDate.Mid(4, 2));
		}
	}

	if (CheckDate(aDateTime))
		return TRUE;

BadDateLabel:

	// imposta i valori di non validita`
	aDateTime.day		= BAD_DAY;
	aDateTime.month		= BAD_MONTH;
	aDateTime.year		= BAD_YEAR;
	aDateTime.hour		= BAD_HOUR;
	aDateTime.minute	= BAD_MINUTE;
	aDateTime.second	= BAD_SECOND;
	aDateTime.fraction	= 0;

	return FALSE;
}

//----------------------------------------------------------------------------
BOOL GetTimeStamp(DBTIMESTAMP& aDateTime, LPCTSTR pszDate, int nFormatIdx, CTBNamespace* pFormatContext /*NULL*/)
{
	CDateFormatter* pFormatter = NULL;
	
	if (nFormatIdx >= 0)
		pFormatter = (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(nFormatIdx, pFormatContext);

	if (pFormatter == NULL)
		pFormatter = (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(DataType::Date, pFormatContext);

	if (pFormatter == NULL)
		return GetTimeStamp
		(
			aDateTime, pszDate, 
			CDateFormatHelper::DATE_DMY, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999,_T("/"), _T("/"),
			CDateFormatHelper::TIME_NONE, _T(""), _T(""), _T("")
		);

		// use international format
	return GetTimeStamp
	(
		aDateTime, pszDate, 
		pFormatter->GetFormat(), 
		pFormatter->GetDayFormat(),
		pFormatter->GetMonthFormat(),
		pFormatter->GetYearFormat(),
		pFormatter->GetFirstSeparator(),
		pFormatter->GetSecondSeparator(),
		pFormatter->GetTimeFormat(),
 		pFormatter->GetTimeSeparator(),
		pFormatter->GetTimeAMString(),
		pFormatter->GetTimePMString()
	);
}

//------------------------------------------------------------------------------
CString FormatDate(WORD wDay, WORD wMonth, WORD wYear, int nFormatIdx, CTBNamespace* pFormatContext /*NULL*/)
{
	DBTIMESTAMP aDateTime;

	aDateTime.day		= wDay;
	aDateTime.month		= wMonth;
	aDateTime.year		= wYear;
	aDateTime.hour		= MIN_HOUR;
	aDateTime.minute	= MIN_MINUTE;
	aDateTime.second	= MIN_SECOND;
    aDateTime.fraction	= 0;

	return FormatDate(aDateTime, nFormatIdx, pFormatContext);
}

//------------------------------------------------------------------------------
CString FormatDate(const DBTIMESTAMP& aDateTime, int nFormatIdx, CTBNamespace* pFormatContext /*NULL*/)
{
	if (IsNullDate(aDateTime))
		return _T("");

	// I formattatori di DataTime e Time derivano da CDateFormatter e quindi
	// verra` chiamata la format giusta
	CDateFormatter* pFormatter = (nFormatIdx < 0)
								? (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(FromDataTypeToFormatName(DataType::Date), pFormatContext)
								: (CDateFormatter*) AfxGetFormatStyleTable()->GetFormatter(nFormatIdx, pFormatContext);

	
	if (pFormatter == NULL)
		return _T("");

	CString str;
	pFormatter->Format(&aDateTime, str, FALSE);

	return str;
}

// format date as string using standard ISO 8601 date/time format 
// of YYYYMMDDTHHMMSSZ, where the T indicates the split between date 
// and time, and the optional Z indicates that the event uses the 
// Universal Coordinated Time (UTC) zone, or Greenwich Mean Time (GMT). 
//----------------------------------------------------------------------------
CString FormatDateTimeForXML (const DBTIMESTAMP& aDateTime, BOOL bSoapType /* = FALSE */)
{
	if (!bSoapType && IsNullDate(aDateTime))
		return _T("");
	
	return cwsprintf // XML Data Type: dateTime
		(
			_T("%04d-%02d-%02dT%02d:%02d:%02d"),
			aDateTime.year, aDateTime.month, aDateTime.day,
			aDateTime.hour, aDateTime.minute, aDateTime.second
		);
}

//----------------------------------------------------------------------------
CString FormatDateTimeForXML (const SYSTEMTIME& aSystemTime, BOOL bSoapType /* = FALSE */)
{
	if (!bSoapType && IsNullDate(aSystemTime))
		return _T("");
	
	return cwsprintf // XML Data Type: dateTime
		(
			_T("%04d-%02d-%02dT%02d:%02d:%02d"),
			aSystemTime.wYear, aSystemTime.wMonth, aSystemTime.wDay,
			aSystemTime.wHour, aSystemTime.wMinute, aSystemTime.wSecond
		);
}

//----------------------------------------------------------------------------
CString FormatDateForXML (const DBTIMESTAMP& aDateTime)
{
	if (IsNullDate(aDateTime))
		return _T("");
	
	return cwsprintf // XML Data Type: dateTime
		(
			_T("%04d-%02d-%02d"),
			aDateTime.year, aDateTime.month, aDateTime.day
		);
}

//----------------------------------------------------------------------------
CString FormatDateForXML (const SYSTEMTIME& aSystemTime)
{
	if (IsNullDate(aSystemTime))
		return _T("");
	
	return cwsprintf // XML Data Type: dateTime
		(
			_T("%04d-%02d-%02d"),
			aSystemTime.wYear, aSystemTime.wMonth, aSystemTime.wDay
		);
}

//----------------------------------------------------------------------------
CString FormatTimeForXML (const DBTIMESTAMP& aDateTime)
{
	if (IsNullDate(aDateTime))
		return _T("");
	
	return cwsprintf // XML Data Type: dateTime
		(
			_T("%02d:%02d:%02d"),
			aDateTime.hour, aDateTime.minute, aDateTime.second
		);
}

//----------------------------------------------------------------------------
CString FormatTimeForXML (const SYSTEMTIME& aSystemTime)
{
	if (IsNullDate(aSystemTime))
		return _T("");
	
	return cwsprintf // XML Data Type: dateTime
		(
			_T("%02d:%02d:%02d"),
			aSystemTime.wHour, aSystemTime.wMinute, aSystemTime.wSecond
		);
}

//----------------------------------------------------------------------------
BOOL GetDD_MM_YYYY(WORD& wDay, WORD& wMonth, WORD& wYear, LPCTSTR pszDate)
{      
	DBTIMESTAMP aDateTime;

	// use fixed format DD/MM/YYYY
	BOOL bOk = GetTimeStamp
	(
		aDateTime, pszDate, 
		CDateFormatHelper::DATE_DMY, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999,_T("/"), _T("/"),
		CDateFormatHelper::TIME_NONE, NULL, NULL, NULL
	);

	wDay	= aDateTime.day;
	wMonth	= aDateTime.month;
	wYear	= aDateTime.year;

	return bOk;
}

//----------------------------------------------------------------------------
BOOL GetYYYYMMDD(WORD& wDay, WORD& wMonth, WORD& wYear, LPCTSTR pszDate)
{      
	// use fixed format YYYYMMDD or YYYY-MM-DD

	DBTIMESTAMP aDateTime;

	BOOL bOk = GetTimeStamp
	(
		aDateTime, pszDate, 
		CDateFormatHelper::DATE_YMD, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999, NULL, NULL,
		CDateFormatHelper::TIME_NONE, NULL, NULL, NULL
	);

	if (!bOk)
		bOk = GetTimeStamp
		(
			aDateTime, pszDate, 
			CDateFormatHelper::DATE_YMD, CDateFormatHelper::DAY99, CDateFormatHelper::MONTH99, CDateFormatHelper::YEAR9999, _T("-"), _T("-"),
			CDateFormatHelper::TIME_NONE, NULL, NULL, NULL
		);

	wDay	= aDateTime.day;
	wMonth	= aDateTime.month;
	wYear	= aDateTime.year;

	return bOk;
}
