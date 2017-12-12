using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RESTGate.OrganizerCore
{
    [FlagsAttribute] 
    public enum WeekDayAppointment : byte
    {
        Sunday    = 1,
        Monday    = 2,
        Tuesday   = 4,
        Wednesday = 8,
        Thursday  = 16,
        Friday    = 32,
        Saturday  = 64
    }
    public enum TaskKind : int
    {
        ToDo	    = 33357824,
        Appointment	= 33357825,
        Recurrence	= 33357826,
        Deadline	= 33357827,
        Memo		= 33357828,
        Task		= 33357829,
        All		    = 33357830
    }
    public enum TimeType : int
    {
        Hour  = 33423360,
        Day   = 33423361,
        Week  = 33423362,
        Month = 33423363,
        Year  = 33423364
    };

    public enum WeekDayAction
    {
        Anticipate,
        Postpone,
        Working
    }

    public class OM_Calendar
    {
        public bool[]           NotWorking              { get; set; }
        public WeekDayAction[]  Action                  { get; set; }

        public OM_Calendar()
        {
            NotWorking = new bool[7];
            Action     = new WeekDayAction[9];
        }
    }

    public class OM_CalendarNonWorkingDays
    {
        public DateTime      StartDate  { get; set; }
        public DateTime      EndDate    { get; set; }
        public bool          NonWorking { get; set; }
        public WeekDayAction Action     { get; set; }
    }

    public class OM_WorkersWorkingTime
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
