using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace RESTGate.OrganizerCore
{
    /// <summary>
    /// A collection of method to calculate deadline of appointment
    /// </summary>
    public static class CalendarCalculator
    {
        private static GregorianCalendar gc = new GregorianCalendar();


                /// <summary>
        /// Get first valid appointment from current date
        /// </summary>
        //---------------------------------------------------------------------------------
        public static DateTime[] CalculateReminderDate(DateTime curDate, TimeType typeBefore, int before, TimeType typeAfter, int after)
        {
            if (before == 0 && after == 0)
                return null;

            int arraySize = (before > 0 ? 1 : 0) +
                            (after  > 0 ? 1 : 0);

            DateTime[] reminderdate = new DateTime[arraySize];

            return reminderdate;
        }

        /// <summary>
        /// Get first valid appointment from current date
        /// </summary>
        //---------------------------------------------------------------------------------
        public static DateTime GetFirstAppointment(DateTime curDate, DateTime startDate, TimeType timeType, int every, WeekDayAppointment weekdayappointment)
        {
            if (curDate <= startDate)
                return startDate;

            switch(timeType)
            {
                case TimeType.Day:
                    var diffday = (int)((curDate - startDate).TotalDays);
                    var mod = diffday % every;
                    var corrDay = mod == 0 
                                    ? 0
                                    : every - mod;
                    curDate = curDate.AddDays(corrDay);
                    break;
                case TimeType.Week:
                    if (weekdayappointment != 0)
                    {
                        // If next appointment is in same week
                        DateTime nextApp = GetFirstDateFromWeekSelection(curDate.AddDays(1), weekdayappointment);
                        if (gc.GetWeekOfYear(curDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday) == gc.GetWeekOfYear(nextApp, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                        {
                            curDate = nextApp;
                            break;
                        }
                        else
                            curDate = nextApp.AddDays((every - 1) * 7);
                    }
                    else
                    {
                        var diffWeek = (int)((curDate - startDate).TotalDays);
                        var modWeek = diffWeek % (every * 7);
                        var corrWeek = modWeek == 0
                                    ? 0
                                    : every - modWeek;
                        curDate = curDate.AddDays(corrWeek);
                    }
                    break;
                case TimeType.Month:
                    var diffMonth = curDate.InMonth() - startDate.InMonth();
                    var modMonth = diffMonth % every;
                    var corrMonth = modMonth == 0 && curDate.Day <= startDate.Day
                                    ? 0
                                    : every - modMonth;
                    curDate = curDate.AddMonths(corrMonth);
                    if (startDate.Day <= gc.GetDaysInMonth(curDate.Year, curDate.Month))
                        curDate = new DateTime(curDate.Year, curDate.Month, startDate.Day);
                    else
                        curDate = new DateTime(curDate.Year, curDate.Month, gc.GetDayOfMonth(curDate));
                    break;

                case TimeType.Year:
                    var diffYear = curDate.Year - startDate.Year;
                    var modYear = diffYear % every;
                    var corrYear = modYear == 0
                                    ? 0
                                    : every - modYear;
                    curDate = curDate.AddYears(corrYear);
                    if (startDate.Day <= gc.GetDaysInMonth(curDate.Year, curDate.Month))
                        curDate = new DateTime(curDate.Year, startDate.Month, startDate.Day);
                    else
                        curDate = new DateTime(curDate.Year, startDate.Month, gc.GetDayOfMonth(curDate));
                    break;

            }
            return curDate;
        }

        /// <summary>
        /// Get first valid appointment from current date
        /// </summary>
        //---------------------------------------------------------------------------------
        public static DateTime GetNextAppointment(DateTime curDate, DateTime startDate, TimeType timeType, int every, WeekDayAppointment weekdayappointment)
        {
            switch (timeType)
            {
                case TimeType.Day:
                    var diff = (int)((curDate - startDate).TotalDays);
                    var mod = diff % every;
                    var correction = every - mod;
                    curDate = curDate.AddDays(correction);
                    break;
                case TimeType.Week:
                    if (weekdayappointment != 0)
                    {
                        // If next appointment is in same week
                        DateTime nextApp = GetFirstDateFromWeekSelection(curDate.AddDays(1), weekdayappointment);
                        if (gc.GetWeekOfYear(curDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday) == gc.GetWeekOfYear(nextApp, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                        {
                            curDate = nextApp;
                            break;
                        }
                        else
                            curDate = nextApp.AddDays((every - 1) * 7);
                    }
                    else 
                    {
                        var diffWeek = (int)((curDate - startDate).TotalDays);
                        var modWeek = diffWeek % (every * 7);
                        var corrWeek = every - modWeek;
                        curDate = curDate.AddDays(corrWeek);                     
                    }
                    break;
                case TimeType.Month:
                    var diffMonth = curDate.InMonth() - startDate.InMonth();
                    var modMonth = diffMonth % every;
                    var corrMonth = every - modMonth;
                    curDate = curDate.AddMonths(corrMonth);
                    if (curDate.Day < startDate.Day)
                    {
                        int nday = Math.Min(DateTime.DaysInMonth(startDate.Year, startDate.Month), DateTime.DaysInMonth(curDate.Year, curDate.Month));
                        curDate = new DateTime(curDate.Year, curDate.Month, nday);
                    }
                    break;
                case TimeType.Year:
                    var diffYear = curDate.Year - startDate.Year;
                    var modYear = diffYear % every;
                    var corrYear = every - modYear;
                    curDate = curDate.AddYears(corrYear);
                    if (curDate.Day < startDate.Day)
                    {
                        int nday = Math.Min(DateTime.DaysInMonth(startDate.Year, startDate.Month), DateTime.DaysInMonth(curDate.Year, curDate.Month));
                        curDate = new DateTime(curDate.Year, curDate.Month, nday);
                    }
                    break;
            }
            return curDate;
        }

        /// <summary>
        /// Get date of last recurrence
        /// </summary>
        //---------------------------------------------------------------------------------
        public static DateTime GetDateOfLastRecurrence(DateTime startDate, DateTime endDate, TimeType timeType, int occurence, int every, WeekDayAppointment weekdayappointment)
        {
            DateTime lastdate = DateTime.MaxValue;
            // If has a end date return this
            if (!endDate.IsEmpty())
                return endDate;

            if (occurence == 0)
                return startDate;

            switch (timeType)
            {
                case TimeType.Day:
                    lastdate = startDate.AddDays(every * (occurence - 1));
                    break;
                case TimeType.Week:
                    DateTime startdate = CalendarCalculator.GetFirstDateFromWeekSelection(startDate, weekdayappointment);
                    lastdate = CalendarCalculator.GetLastDateFromWeekSelection(startdate, occurence, every, weekdayappointment);
                    break;
                case TimeType.Month:
                    lastdate = startDate.AddMonths(occurence-1);
                    break;
                case TimeType.Year:
                    lastdate = startDate.AddYears(occurence-1);
                    break;
            }

            return lastdate;
        }

        /// <summary>
        /// Calculate date of last appointment in accordion with week day
        /// </summary>
        //---------------------------------------------------------------------------------
        public static DateTime GetLastDateFromWeekSelection(DateTime startdate, int occurence, int every, WeekDayAppointment weekdayappointment)
        {
            if (occurence == 0)
                return startdate;

            occurence--;
            // Number of appointment in week
            int appointmentInWeek = GetOccurenceAppointmentInSingleWeek(weekdayappointment);

            // If I don't select any day
            if (appointmentInWeek == 0)
                return startdate.AddDays(7 * every * occurence);

            int numOfWeek = (int)Math.Floor((double)occurence / appointmentInWeek);
            int other = occurence % appointmentInWeek + 1;

            DateTime lastdate = startdate.AddDays(7 * every * numOfWeek);
            lastdate = GetDateAfterSomeOccurenceWithWeekDays(lastdate, other, every, weekdayappointment);

            return lastdate;
        }

        /// <summary>
        /// Calculate number of appointment set in a single week
        /// </summary>
        //---------------------------------------------------------------------------------
        public static int GetOccurenceAppointmentInSingleWeek(WeekDayAppointment weekdayappointment)
        {
            int appointmentInnWeek = 0;
            for (int i = 0; i < 7; i++)
            {
                int weekday = (int)Math.Pow(2, i);
                if ((weekday & (int)weekdayappointment) == weekday)
                    appointmentInnWeek++;
            }

            return appointmentInnWeek;
        }

        /// <summary>
        /// Calculate the first date valid in accord to days of week selected
        /// </summary>
        /// <remarks>
        /// if the start date is Thursday 14/02/2013 and the weekday of appointment is Tuesday
        /// the first appointment will be Tuesday 19/02/2013
        /// </remarks>
        //---------------------------------------------------------------------------------
        public static DateTime GetFirstDateFromWeekSelection(DateTime startDate, WeekDayAppointment weekdayappointment)
        {
            if (weekdayappointment == 0)
                return startDate;

            while (((int)Math.Pow(2, (int)startDate.DayOfWeek) & (int)weekdayappointment) == 0)
                startDate = startDate.AddDays(1);

            return startDate;
        }

        /// <summary>
        /// Return of last appointment in accord to days of weekly appointment
        /// </summary>
        //---------------------------------------------------------------------------------
        public static DateTime GetDateAfterSomeOccurenceWithWeekDays(DateTime startDate, int occurrence, int every, WeekDayAppointment weekdayappointment)
        {
            occurrence--;
            while (occurrence > 0)
            {
                startDate = startDate.AddDays(1);
                if (startDate.DayOfWeek == DayOfWeek.Monday && every > 1)
                    startDate = startDate.AddDays(7*(every-1));
                if (((int)Math.Pow(2, (int)startDate.DayOfWeek) & (int)weekdayappointment) != 0)
                    occurrence--;
            }

            return startDate;
        }
    }
}
