using System;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.UI.Organizer;

namespace RESTGate.OrganizerCore
{
    //=========================================================================
    public static class ExtendDateTime
    {
        /// <summary>
        /// Return the date 31/12/1799 - Empty date for Taskbuilder
        /// </summary>
        /// <param name="date"></param>
        //---------------------------------------------------------------------------------
        public static DateTime EmptyDate(this DateTime date)
        {
            return new DateTime(1799, 12, 31);
        }

        /// <summary>
        /// Check if date is empty => Date = 31/12/1799 for Taskbuilder
        /// </summary>
        /// <param name="date"></param>
        //---------------------------------------------------------------------------------
        public static bool IsEmpty(this DateTime date)
        {
            return date == EmptyDate(date);
        }

        /// <summary>
        /// Return Number of month from 1/1/0000
        /// </summary>
        /// <param name="date"></param>
        //---------------------------------------------------------------------------------
        public static int InMonth(this DateTime date)
        {
            return date.Year * 12 + date.Month;
        }

        /// <summary>
        /// Return a Number result from Mount * 31 + day
        /// </summary>
        /// <remarks>
        /// In this way I can see in easy mode if a day is in a range without worry of year 
        /// I can't use the method DaysOfYear because the 29/2/2000 is same of 01/3/2001
        /// </remarks>
        //---------------------------------------------------------------------------------
        public static int InDayOfYear(this DateTime date)
        {
            return date.Month * 31 + date.Day;
        }
    }

    //=========================================================================
    public class TaskTodo
    {
        private string token;
        private List<OrganizerAppointmentExposer> appointments;
        private List<OrganizerWorker> workers;
        private NonWorkingDays NonWorkindDays;

        public List<OrganizerAppointmentExposer> Appointments { get { return this.appointments; } }
        public List<OrganizerWorker> Workers { get { return this.workers; } }
        public string Token { get { return this.token; } set { this.token = value; } }

        public TaskTodo()
        {
            this.token = String.Empty;
            this.appointments = new List<OrganizerAppointmentExposer>();
            this.workers = new List<OrganizerWorker>();
            NonWorkindDays = new NonWorkingDays();
        }

        /// <summary>
        /// Load task list
        /// </summary>
        //---------------------------------------------------------------------------------
        public bool LoadTasks(
            long fromWorkerID, long toWorkerID, DateTime fromDate, DateTime toDate, TaskKind taskKind)
        {
            if (String.IsNullOrEmpty(this.token))
            {
                return false;
            }

            this.appointments.Clear();

            List<OM_Commitments> taskrecord = GetTasksFromDB(fromWorkerID, toWorkerID, fromDate, toDate, taskKind);

            for (int t = 0; t < taskrecord.Count; t++)
            {
                if (taskrecord[t].IsRecurrence)
                    AddRecurrenceAppointment(taskrecord[t], fromDate, toDate);
                else
                    AddAppointment(taskrecord[t]);
            }

            return true;
        }

        /// <summary>
        /// Load task list
        /// </summary>
        //---------------------------------------------------------------------------------
        public void LoadMockTasks(
            long fromWorkerID, long toWorkerID, DateTime fromDate, DateTime toDate, TaskKind taskKind)
        {
            this.appointments.Clear();

            List<OM_Commitments> taskrecord = GetMockTasks(fromWorkerID, toWorkerID, fromDate, toDate, taskKind);

            for (int t = 0; t < taskrecord.Count; t++)
            {
                if (taskrecord[t].IsRecurrence)
                    AddRecurrenceAppointment(taskrecord[t], fromDate, toDate);
                else
                    AddAppointment(taskrecord[t]);
            }
        }

        /// <summary>
        /// Return task list
        /// </summary>
        //---------------------------------------------------------------------------------
        public void LoadWorkers()
        {
            this.workers.Clear();
            List<OM_Workers> listWorkers = GetWorkersFromDB();
            listWorkers.ForEach(p => { AddWorker(p); });
        }

        //---------------------------------------------------------------------------------
        private void AddWorker(OM_Workers omWorker)
        {
            OrganizerWorker resource = new OrganizerWorker();

            resource.Id = omWorker.WorkerId;
            resource.Name = String.Concat(omWorker.Name, " ", omWorker.LastName);
            this.workers.Add(resource);
        }

        //---------------------------------------------------------------------------------
        private List<OM_Workers> GetWorkersFromDB()
        {
            return OrganizerCache.Instance.GetCompany(LivingTokens.Instance.GetCompanyFromToken(this.token)).Workers;
        }

        /// <summary>
        /// Add a recurrence appointment to List of appointment
        /// </summary>
        //---------------------------------------------------------------------------------
        private bool AddRecurrenceAppointment(OM_Commitments record, DateTime fromDate, DateTime toDate)
        {
            // Check if my range date is into range date of recurrences
            DateTime lastAppointmentDate = new DateTime(
                Math.Min(
                    toDate.Ticks, 
                    CalendarCalculator.GetDateOfLastRecurrence(
                        record.StartDate, record.RecurrenceEndDate, (TimeType)record.RecurrenceType, record.RecurrenceOccurrences, record.RecurrenceEvery, (WeekDayAppointment)record.RecurrenceWeekDays).Ticks));
            if (fromDate > lastAppointmentDate)
                return true;

            DateTime curDate = CalendarCalculator.GetFirstAppointment(fromDate, record.StartDate, (TimeType)record.RecurrenceType, record.RecurrenceEvery, (WeekDayAppointment)record.RecurrenceWeekDays);
            while (curDate <= lastAppointmentDate)
            {
                AddAppointment(record, curDate);
                curDate = CalendarCalculator.GetNextAppointment(curDate, record.StartDate, (TimeType)record.RecurrenceType, record.RecurrenceEvery, (WeekDayAppointment)record.RecurrenceWeekDays);
            }
            return true;
        }

        /// <summary>
        /// Add a appointment to List of appointment
        /// </summary>
        //---------------------------------------------------------------------------------
        private bool AddAppointment(OM_Commitments record, DateTime startDate)
        {
            OrganizerAppointmentExposer item = new OrganizerAppointmentExposer();
            DateTime newDate = NonWorkindDays.GetFirstWorkingDate(startDate);

            item.CommitmentId = record.CommitmentID;
            //item.Category = record.TaskKind.ToString(); // to change
            item.Subject = record.Subject;
            item.Description = record.Description;
            item.Start = newDate;
            item.End = record.EndDate >= newDate ? record.EndDate : newDate;
            item.ResourceId = record.WorkerID;
            Appointments.Add(item);
            return true;
        }

        /// <summary>
        /// Add a appointment to List of appointment
        /// </summary>
        //---------------------------------------------------------------------------------
        private bool AddAppointment(OM_Commitments record)
        {
            OrganizerAppointmentExposer item = new OrganizerAppointmentExposer();
            DateTime newDate = NonWorkindDays.GetFirstWorkingDate(record.StartDate);

            item.CommitmentId = record.CommitmentID;
            //item.Category = record.TaskKind.ToString(); // to change
            item.Subject = record.Subject;
            item.Description = record.Description;
            item.Start = record.StartDate;
            item.End = record.EndDate >= newDate ? record.EndDate : newDate;
            item.ResourceId = record.WorkerID;
            Appointments.Add(item);
            return true;
        }

        /// <summary>
        /// Read from DB an array of tasks
        /// </summary>
        //---------------------------------------------------------------------------------
        private List<OM_Commitments> GetTasksFromDB(long fromWorkerID, long toWorkerID, DateTime fromDate, DateTime toDate, TaskKind taskKind)
        {
            return OrganizerCache.Instance.GetCompany(LivingTokens.Instance.GetCompanyFromToken(this.token)).Commitments;
        }

        //---------------------------------------------------------------------------------
        private List<OM_Commitments> GetMockTasks(
            long fromWorkerID, long toWorkerID, DateTime fromDate, DateTime toDate, TaskKind taskKind)
        {
            List<OM_Commitments> mockList = new List<OM_Commitments>();

            OM_Commitments task = null;

            // From 8/1 every 2 day for 6 times
            if (fromWorkerID == 0)
            {
                task = new OM_Commitments();
                task.IsRecurrence = true;
                task.StartDate = new DateTime(2013, 1, 8);
                task.RecurrenceEndDate = new DateTime().EmptyDate();
                task.RecurrenceType = (int)TimeType.Day;
                task.RecurrenceOccurrences = 6;
                task.RecurrenceEvery = 2;
            }

            // From 8/1 every Tuesday, Thursday, Friday for 9 times
            if (fromWorkerID == 1)
            {
                task = new OM_Commitments();
                task.IsRecurrence = true;
                task.StartDate = new DateTime(2013, 1, 8);
                task.RecurrenceEndDate = new DateTime().EmptyDate();
                task.RecurrenceType = (int)TimeType.Week;
                task.RecurrenceOccurrences = 9;
                task.RecurrenceWeekDays = (int) (WeekDayAppointment.Tuesday | WeekDayAppointment.Thursday | WeekDayAppointment.Friday);
                task.RecurrenceEvery = 1;
            }

            // From 8/1 every Tuesday, Thursday, Friday for 6 times every 2 week
            if (fromWorkerID == 2)
            {
                task = new OM_Commitments();
                task.IsRecurrence = true;
                task.StartDate = new DateTime(2013, 1, 8);
                task.RecurrenceEndDate = new DateTime().EmptyDate();
                task.RecurrenceType = (int)TimeType.Week;
                task.RecurrenceOccurrences = 6;
                task.RecurrenceWeekDays = (int) (WeekDayAppointment.Tuesday | WeekDayAppointment.Thursday | WeekDayAppointment.Friday);
                task.RecurrenceEvery = 2;
            }

            mockList.Add(task);
            return mockList;
        }

    }
}
