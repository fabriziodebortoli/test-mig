using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTGate.OrganizerCore
{
    //=========================================================================
    internal class NonWorkingPeriodItem
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public WeekDayAction Action { get; set; }
    }

    //=========================================================================
    internal class NonWorkingPeriod : System.Collections.SortedList
    {
        //---------------------------------------------------------------------------------
        public bool ContainsKey(DateTime startdate, DateTime enddate)
        {
            return ContainsKey(GetKey(startdate, enddate));
        }

        //---------------------------------------------------------------------------------
        public void AddPeriod(DateTime startdate, DateTime enddate, WeekDayAction action)
        {
            string key = GetKey(startdate, enddate);
            if (!ContainsKey(key))
                Add(key, new NonWorkingPeriodItem() { StartDate = startdate, EndDate = enddate, Action = action });
        }

        //---------------------------------------------------------------------------------
        private string GetKey(DateTime startdate, DateTime enddate)
        {
            return string.Concat(startdate.InDayOfYear().ToString("0000"), "-", enddate.InDayOfYear().ToString("0000"));
        }
    }

    //=========================================================================
    internal class NonWorkingWeekDay : Dictionary<string, WeekDayAction>
    {
        //---------------------------------------------------------------------------------
        public void AddDay(DayOfWeek day, WeekDayAction action)
        {
            Add(day.ToString(), action);
        }

        //---------------------------------------------------------------------------------
        public void AddDay(string day, WeekDayAction action)
        {
            if (!ContainsKey(day))
                Add(day, action);
        }

        public WeekDayAction this[DayOfWeek day]
        {
            get {return this[day.ToString()]; }
        }

        //---------------------------------------------------------------------------------
        public bool ContainsKey(DayOfWeek day)
        {
            return ContainsKey(day.ToString());
        }
    }

    //=========================================================================
    public class AntiLoopPeriod : List<string>
    {
        public bool InLoop { get; private set; }

        public new void Clear()
        {
            base.Clear();
            InLoop = false;
        }

        //---------------------------------------------------------------------------------
        public bool AddNewDay(DayOfWeek dayOfWeek)
        {
            string key = dayOfWeek.ToString();
            if (Contains(key))
            {
                InLoop = true;
                return true;
            }

            Add(key);
            return false;
        }

        //---------------------------------------------------------------------------------
        public bool AddNewPeriod(DateTime startdate, DateTime enddate)
        {
            string key = string.Concat(startdate.InDayOfYear().ToString("0000"), "-", enddate.InDayOfYear().ToString("0000"));

            if (Contains(key))
            {
                InLoop = true;
                return true;
            }

            Add(key);
            return false;
        }
    }

    //=========================================================================
    public class NonWorkingDays
    {
        private NonWorkingWeekDay NonWorkingWeekDay = new NonWorkingWeekDay();
        private NonWorkingPeriod NonWorkingPeriod = new NonWorkingPeriod();
        private AntiLoopPeriod PeriodAlredyAnalized = new AntiLoopPeriod();
        private AntiLoopPeriod WeekDayAlredyAnalized = new AntiLoopPeriod();
        private Dictionary<int, DateTime> EasterDays = new Dictionary<int, DateTime>();

        private const string EasterSunday = "EasterSunday";
        private const string EasterMonday = "EasterMonday";

        /// <summary>
        /// Constructor
        /// </summary>
        //---------------------------------------------------------------------------------
        public NonWorkingDays()
        {
            LoadCalendar();
        }

        public bool IsInLoop
        {
            get { return PeriodAlredyAnalized.InLoop || WeekDayAlredyAnalized.InLoop; }
        }

        /// <summary>
        /// Get First Working Date in according to calendar
        /// </summary>
        /// <returns></returns>
        //---------------------------------------------------------------------------------
        public DateTime GetFirstWorkingDate(DateTime curdate)
        {
            PeriodAlredyAnalized.Clear();
            WeekDayAlredyAnalized.Clear();

            DateTime lastDate = curdate;
            DateTime oldDate = DateTime.MaxValue;

            while (lastDate != oldDate)
            {
                oldDate = lastDate;
                lastDate = ExcludeFromNonWorkingPeriod(lastDate);
                if (PeriodAlredyAnalized.InLoop)
                    return curdate;

                WeekDayAlredyAnalized.Clear();
                lastDate = ExcludeFromNonWorkingWeekDay(lastDate);
                if (WeekDayAlredyAnalized.InLoop)
                    return curdate;
            }
            return lastDate;
        }

        /// <summary>
        /// Return a valid data in according with NonWorking Calendar
        /// </summary>
        //---------------------------------------------------------------------------------
        private DateTime ExcludeFromNonWorkingPeriod(DateTime curdate)
        {
            bool hasChanged = false;
            DateTime lastDate = curdate;
            do
            {
                hasChanged = false;
                for (int t = 0; t < NonWorkingPeriod.Count; t++)
                {
                    // Get the NonWoring Period
                    NonWorkingPeriodItem nwP = NonWorkingPeriod.GetByIndex(0) as NonWorkingPeriodItem;

                    // If my date is in period
                    if (lastDate.InDayOfYear() >= nwP.StartDate.InDayOfYear() && lastDate.InDayOfYear() <= nwP.EndDate.InDayOfYear())
                    {
                        // If period alredy analized => I'm in loop and exit with original date
                        if (PeriodAlredyAnalized.AddNewPeriod(nwP.StartDate, nwP.EndDate))
                            return curdate;

                        // Calculate new date
                        lastDate = nwP.Action == WeekDayAction.Postpone
                                    ? new DateTime(lastDate.Year, nwP.EndDate.Month, nwP.EndDate.Day).AddDays(1)
                                    : new DateTime(lastDate.Year, nwP.StartDate.Month, nwP.StartDate.Day).AddDays(-1);
                        hasChanged = true;
                        break;
                    }
                    // If start date of period is greather then my date break the loop
                    if (nwP.StartDate.InDayOfYear() > lastDate.InDayOfYear())
                        break;
                }
            }
            // Continue until i have changed the date, because I could enter in another period
            while (hasChanged);

            return lastDate;
        }

        /// <summary>
        /// Return a valid data in according with NonWorking Calendar
        /// </summary>
        //---------------------------------------------------------------------------------
        private DateTime ExcludeFromNonWorkingWeekDay(DateTime curdate)
        {
            DateTime lastDate = curdate;
            bool hasChanged = false;
            do
            {
                hasChanged = false;
                if (NonWorkingWeekDay.ContainsKey(EasterSunday) && GetEasterSunday(lastDate.Year) == lastDate)
                {
                    // Calculate new for easter sunday
                    lastDate = NonWorkingWeekDay[EasterSunday] == WeekDayAction.Postpone
                                ? lastDate.AddDays(1)
                                : lastDate.AddDays(-1);
                    hasChanged = true;
                }
                if (NonWorkingWeekDay.ContainsKey(EasterMonday) && GetEasterMonday(lastDate.Year) == lastDate)
                {
                    // Calculate new for easter Monday
                    lastDate = NonWorkingWeekDay[EasterMonday] == WeekDayAction.Postpone
                                ? lastDate.AddDays(1)
                                : lastDate.AddDays(-1);
                    hasChanged = true;
                }
                // If my date is a non workin day
                if (NonWorkingWeekDay.ContainsKey(lastDate.DayOfWeek))
                {
                    // If weekday alredy analized => I'm in loop and exit with original date
                    if (WeekDayAlredyAnalized.AddNewDay(lastDate.DayOfWeek))
                        return curdate;

                    // Calculate new date
                    lastDate = NonWorkingWeekDay[lastDate.DayOfWeek] == WeekDayAction.Postpone
                                ? lastDate.AddDays(1)
                                : lastDate.AddDays(-1);
                    hasChanged = true;
                    continue;
                }

            }
            // Continue until i have changed the date, because I could find another day
            while (hasChanged);

            return lastDate;
        }

        /// <summary>
        /// Load NonWorking calendar from DB
        /// </summary>
        //---------------------------------------------------------------------------------
        private void LoadCalendar()
        {
            //TODO
        }

        //---------------------------------------------------------------------------------
        public void FillCalenderForUnitTest(string weekday, WeekDayAction weekDayAction)
        {
            if (!NonWorkingWeekDay.ContainsKey(weekday))
                NonWorkingWeekDay.Add(weekday, weekDayAction);
        }

        //---------------------------------------------------------------------------------
        public void FillCalenderForUnitTest(DateTime startdate, DateTime enddate, WeekDayAction action)
        {
            if (!NonWorkingPeriod.ContainsKey(startdate, enddate))
                NonWorkingPeriod.AddPeriod(startdate, enddate, action);
        }

        /// <summary>
        /// Return date of easter sunday for year
        /// </summary>
        //---------------------------------------------------------------------------------
        private DateTime GetEasterSunday(int year)
        {
            // To avoid calculate every times
            if (EasterDays.ContainsKey(year))
                return EasterDays[year];

            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }
            EasterDays.Add(year, new DateTime(year, month, day));

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Return date of easter monday for year
        /// </summary>
        //---------------------------------------------------------------------------------
        private DateTime GetEasterMonday(int year)
        {
            return GetEasterSunday(year).AddDays(1);
        }

        //[TestMethod]
        //public void EasterSunday()
        //{
        //    Assert.AreEqual<DateTime>(new DateTime(2013, 3, 31), CalendarCalculator.EasterSunday(2013), "Pasqua 2013");
        //    Assert.AreEqual<DateTime>(new DateTime(2014, 4, 20), CalendarCalculator.EasterSunday(2014), "Pasqua 2014");
        //}
    }
}