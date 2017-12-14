using System;
using System.Globalization;

namespace Microarea.TaskBuilderNet.Core.CoreTypes
{
	/// <summary>
	/// DateTimeFunctions.
	/// </summary>
	//================================================================================
	public sealed class DateTimeFunctions
	{
		//Dal file 'TbGeneric\DatesFunction.h'
		public const int MinDay = 31;
		public const int MinMonth = 12;
		public const int MinYear = 1799;
		public const int MinTimeDay = 30;			// vedere HELP di Microsoft
		public const int MinTimeMonth = 12;			// vedere HELP di Microsoft
		public const int MinTimeYear = 1899;		// vedere HELP di Microsoft
		public const int MinWeekday = 2;
		public const int MinHour = 0;
		public const int MinMinute = 0;
		public const int MinSecond = 0;
		public const int MaxDay = 31;				// dipende comunque dal mese
		public const int MaxMonth = 12;
		public const int MaxYear = 2199;
		public const long MinGiulianDate = 1L;		// corrispondente a 01/01/1800

		public static readonly long MaxGiulianDate = GiulianDate(new DateTime(MaxYear, MaxMonth, MaxDay));
		public static readonly DateTime MaxValue = new DateTime(MaxYear, MaxMonth, MaxDay);
		public static readonly DateTime MinValue = new DateTime(MinYear, MinMonth, MinDay);

		//----------------------------------------------------------------------------
		private DateTimeFunctions()
		{}

		// ritorna il nome del mese dato il numero del mese
		//----------------------------------------------------------------------------
		public static string MonthName(int month)
		{
			return MonthName(month, CultureInfo.InvariantCulture);
		}

		// ritorna il nome del mese dato il numero del mese
		//----------------------------------------------------------------------------
		public static string MonthName(int month, IFormatProvider provider)
		{
			if ((month < 1) || (month > 12)) return "";
			DateTime dt = new DateTime(2000, month, 1);

			if (provider == null)
				provider = CultureInfo.InvariantCulture;

			return dt.ToString("MMMM", provider);
		}

		// ritorna il nome del mese data una data
		//----------------------------------------------------------------------------
		public static string MonthName (DateTime dt)
		{
			return MonthName(dt, CultureInfo.InvariantCulture);
		}

		// ritorna il nome del mese data una data
		//----------------------------------------------------------------------------
		public static string MonthName(DateTime dt, IFormatProvider provider)
		{
			if (provider == null)
				provider = CultureInfo.InvariantCulture;

			return dt.ToString("MMMM", provider);
		}

		// ritorna il nome del mese data una data
		//----------------------------------------------------------------------------
		public static string WeekdayName(DateTime dt)
		{
			return WeekdayName(dt, CultureInfo.InvariantCulture);
		}

		// ritorna il nome del mese data una data
		//----------------------------------------------------------------------------
		public static string WeekdayName(DateTime dt, IFormatProvider provider)
		{
			if (provider == null)
				provider = CultureInfo.InvariantCulture;

			return dt.ToString("dddd", provider);
		}

		// ritorna il giorno della settimana. 0 = Monday; ... 6 = Sunday;
		//----------------------------------------------------------------------------
		public static int DayOfWeek(DateTime dt)
		{
			int day = (int) dt.DayOfWeek;
			if (day == 0) day = 6; else day--; 

			return day;
		}

		//----------------------------------------------------------------------------
		// differente da quella C++ in TbGeneric/DatesFunctions
		// di seguito ci sono quelle tradotte
		public static int WeekOfMonth2(DateTime dt)
		{
			int wom = 0;
			DateTime start = dt;
			TimeSpan today = new TimeSpan((int)dt.DayOfWeek, 0, 0, 0);
			TimeSpan week = new TimeSpan(7, 0, 0, 0);

			start = dt - today;
			while (!((start.Year < dt.Year) || (start.Month < dt.Month)))
			{
				wom++;
				start = start - week;
			}
			return wom;
		}

		// return the week day from a Giulian date:
		// 0 = Monday; ... 6 = Sunday;
		//----------------------------------------------------------------------------
		private const int MIN_WEEK_DAY = 2;

		public static int WeekDay(long nDate)
		{
			return (int)((nDate + MIN_WEEK_DAY - 1) % 7);
		}
		
		//-----------------------------------------------------------------------------

		public static int WeekOfMonth(DateTime dt)
		{
			DateTime dtFirst = new DateTime(dt.Year, dt.Month, 1);	//primo del mese
			
			long nFirst = GiulianDate(dtFirst);
			long nToday = GiulianDate(dt);

			int n = DayOfWeek(dtFirst);
			if( dt.Day <= (7 - DayOfWeek(dtFirst))  && DayOfWeek(dtFirst) > 0)
			{
				if (dtFirst.Month > 1)
				{
					dtFirst = new DateTime(dtFirst.Year, dtFirst.Month - 1, dtFirst.Day);	//settimana iniziata nel mese precedente
				}
				else
				{
					//settimana iniziata nel mese precedente
					//dell'anno precedente
					dtFirst = new DateTime(dtFirst.Year - 1, 12, dtFirst.Day);
				}

				nFirst = GiulianDate(dtFirst);
			}
			if (DayOfWeek(dtFirst) > 0)
				nFirst += (7 - DayOfWeek(dtFirst)); //giorni della settimana precedente

			return (int) (nToday - nFirst) / 7 + 1;
		}

		//-----------------------------------------------------------------------------
		public static int WeekOfMonthISO(DateTime nDate)
		{
			int monthDays = DateTime.DaysInMonth(nDate.Year, nDate.Month); //MonthDays
			DateTime nFoM = new DateTime(nDate.Year, nDate.Month, 1);	// First day of Month
			long loMGD = GiulianDate(nDate);	// Date in Giulian notation
			long foMGD = GiulianDate(nFoM);	// First day of Month in Giulian notation
			int weekDay;

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
			if (weekDay <= 3)
				foMGD -= weekDay;
			else
				foMGD += (7 - weekDay);

			long WoM = (loMGD - foMGD)/7 + 1;

			return (int)WoM;
		}

		//----------------------------------------------------------------------------
		// differente da quella C++ in TbGeneric/DatesFunctions
		// di seguito quella tradotta
		public static int WeekOfYear2(DateTime dt)
		{
			int woy = 0;
			DateTime start = dt;
			TimeSpan today = new TimeSpan((int)dt.DayOfWeek, 0, 0, 0);
			TimeSpan week = new TimeSpan(7, 0, 0, 0);

			start = dt - today;
			while (!((start.Year < dt.Year)))
			{
				woy++;
				start = start - week;
			}
			return woy;
		}

		//----------------------------------------------------------------------------
		public static int WeekOfYear(DateTime dt)
		{
			DateTime dtFirst = new DateTime(dt.Year, 1, 1);	//primo dell'anno

			long nFirst = GiulianDate(dtFirst);
			long nToday = GiulianDate(dt);

			int n = DayOfWeek(dtFirst);
			if (n > 3)
				n = n - 7;

			long nDays = nToday - nFirst + n;
			if (nDays < 0)
			{
				return WeekOfYear(new DateTime(dtFirst.Year - 1, 12, 31));
			}
			return (int) nDays / 7 + 1;
		}

		//----------------------------------------------------------------------------
		public static long GiulianDate(DateTime dt)
		{
			DateTime start = new DateTime(MinYear, MinMonth, MinDay); // compatibile con TaskBuilder
			TimeSpan span = dt.Subtract(start);

			return span.Days;
		}
	}
}
