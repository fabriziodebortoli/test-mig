using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbHermesBL;
using Google.Apis.Calendar.v3.Data;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_GoogleCalendarsEvents
    {
        public bool BoolIsDeleted
        {
            get { return this.IsDeleted == "1"; }
            set { this.IsDeleted = value ? "1" : "0"; }
        }
        public bool BoolIsOwner
        {
            get { return this.IsOwner == "1"; }
            set { this.IsOwner = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendarsEvents CreateItem(int workerID, int commitmentId, int recurrenceId, Event ev, string gooIdCal, bool owner = true)
        {
            OM_GoogleCalendarsEvents calev = new OM_GoogleCalendarsEvents();
            calev.WorkerId = workerID;
            calev.IdCalendar = gooIdCal;
            calev.IdEvent = ev.Id;
            calev.CommitmentId = commitmentId;
            calev.ParentCommitmentId = recurrenceId;
            calev.BoolIsDeleted = false; //ev.Status // stato evento 
            calev.BoolIsOwner = owner; // Proprietario

            DateTime now = DateTime.Now;
            calev.TBCreated = now;
            calev.TBModified = now;
            calev.TBCreatedID = workerID;
            calev.TBModifiedID = workerID;
            return calev;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendarsEvents CreateItem(int workerID, OM_Commitments comm, Event ev, string gooIdCal, bool owner = true)
        {
            OM_GoogleCalendarsEvents calev = new OM_GoogleCalendarsEvents();
            calev.WorkerId = workerID;
            calev.IdCalendar = gooIdCal;
            calev.IdEvent = ev.Id;
            calev.CommitmentId = comm.CommitmentId;
            calev.ParentCommitmentId = comm.RecurrenceId;
            calev.BoolIsDeleted = false; //ev.Status // stato evento 
            calev.BoolIsOwner = owner; // Proprietario

            DateTime now = DateTime.Now;
            calev.TBCreated = now;
            calev.TBModified = now;
            calev.TBCreatedID = workerID;
            calev.TBModifiedID = workerID;
            return calev;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendarsEvents GetGoogleCalendarEvent(TbSenderDatabaseInfo cmp, int workerID, string EventID, string CalendarID)
        {
            if (EventID.IsNullOrEmpty() ||
                CalendarID.IsNullOrEmpty() ||
                workerID.Equals(0))
                return null;
            OM_GoogleCalendarsEvents calev = null;

            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                calev = GetGoogleCalendarEvent(db, workerID, EventID, CalendarID);              
            }
            
            return calev;
        }
        public static OM_GoogleCalendarsEvents GetGoogleCalendarEvent(MZP_CompanyEntities db, int workerID, string EventID, string CalendarID)
        {
            if (EventID.IsNullOrEmpty() ||
                CalendarID.IsNullOrEmpty() ||
                workerID.Equals(0))
                return null;

            List<OM_GoogleCalendarsEvents> list = new List<OM_GoogleCalendarsEvents>();
            var items = from p in db.OM_GoogleCalendarsEvents
                        where (p.IdEvent == EventID) &&
                              (p.IdCalendar == CalendarID) &&
                              (p.WorkerId == workerID)
                        select p;
            list.AddRange(items);

            OM_GoogleCalendarsEvents calev = null;
            if (list.Count > 0)
                calev = list[0];

            return calev;
        }
        
        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendarsEvents GetGoogleCalendarEvent(TbSenderDatabaseInfo cmp, int workerID, int commitmentId, string EventID, string CalendarID)
        {
            if (EventID.IsNullOrEmpty() ||
                CalendarID.IsNullOrEmpty() ||
                workerID.Equals(0))
                return null;
            OM_GoogleCalendarsEvents calev = null;

            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                calev = GetGoogleCalendarEvent(db, workerID, commitmentId, EventID, CalendarID);              
            }

            return calev;
        }
        public static OM_GoogleCalendarsEvents GetGoogleCalendarEvent(MZP_CompanyEntities db, int workerID, int commitmentId, string EventID, string CalendarID)
        {
            if (EventID.IsNullOrEmpty() ||
                CalendarID.IsNullOrEmpty() ||
                workerID.Equals(0))
                return null;

            List<OM_GoogleCalendarsEvents> list = new List<OM_GoogleCalendarsEvents>();
            var items = from p in db.OM_GoogleCalendarsEvents
                        where (p.IdEvent == EventID) &&
                              (p.CommitmentId == commitmentId) &&
                              (p.IdCalendar == CalendarID) &&
                              (p.WorkerId == workerID)
                        select p;
            list.AddRange(items);

            OM_GoogleCalendarsEvents calev = null;
            if (list.Count > 0)
                calev = list[0];

            return calev;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendarsEvents GetGoogleCalendarEvent(TbSenderDatabaseInfo cmp, string EventID)
        {
            if (EventID.IsNullOrEmpty())
                return null;

            OM_GoogleCalendarsEvents calev = null;
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                calev = GetGoogleCalendarEvent(db, EventID);
            }

            return calev;
        }
        public static OM_GoogleCalendarsEvents GetGoogleCalendarEvent(MZP_CompanyEntities db, string EventID)
        {
            if (EventID.IsNullOrEmpty())
                return null;

            List<OM_GoogleCalendarsEvents> list = new List<OM_GoogleCalendarsEvents>();
            var items = from p in db.OM_GoogleCalendarsEvents
                        where (p.IdEvent == EventID)
                        select p;
            list.AddRange(items);
            OM_GoogleCalendarsEvents calev = null;
            if (list.Count > 0)
                calev = list[0];

            return calev;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendarsEvents GetOtherGoogleCalendarEvent(MZP_CompanyEntities db, string EventID, string CalendarID, int workerID, int commID)
        {
            if (EventID.IsNullOrEmpty())
                return null;

            List<OM_GoogleCalendarsEvents> list = new List<OM_GoogleCalendarsEvents>();
            var items = from p in db.OM_GoogleCalendarsEvents
                        where (p.IdEvent == EventID)        &&
                              ((p.IdCalendar != CalendarID) ||
                              (p.CommitmentId != commID)    ||
                              (p.WorkerId != workerID))
                        select p;
            list.AddRange(items);
            OM_GoogleCalendarsEvents calev = null;
            if (list.Count > 0)
                calev = list[0];

            return calev;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendarsEvents GetGoogleCalendarEvent(TbSenderDatabaseInfo cmp, int workerID, int commitmentID)
        {
            if (commitmentID.Equals(0) ||
                workerID.Equals(0))
                return null;

            List<OM_GoogleCalendarsEvents> list = new List<OM_GoogleCalendarsEvents>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_GoogleCalendarsEvents
                            where (p.CommitmentId == commitmentID) &&
                                  (p.WorkerId == workerID)
                            select p;
                list.AddRange(items);
            }
            OM_GoogleCalendarsEvents calev = null;
            if (list.Count > 0)
                calev = list[0];

            return calev;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendarsEvents GetGoogleCalendarEvent(TbSenderDatabaseInfo cmp, int workerID, int commitmentID, string CalendarID)
        {
            if (commitmentID.Equals(0) ||
                CalendarID.IsNullOrEmpty()  ||
                workerID.Equals(0))
                return null;

            OM_GoogleCalendarsEvents calev = null;
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                calev = GetGoogleCalendarEvent(db, workerID, commitmentID, CalendarID);
            }

            return calev;
        }
        public static OM_GoogleCalendarsEvents GetGoogleCalendarEvent(MZP_CompanyEntities db, int workerID, int commitmentID, string CalendarID)
        {
            if (commitmentID.Equals(0) ||
                CalendarID.IsNullOrEmpty() ||
                workerID.Equals(0))
                return null;

            List<OM_GoogleCalendarsEvents> list = new List<OM_GoogleCalendarsEvents>();
            var items = from p in db.OM_GoogleCalendarsEvents
                        where (p.IdCalendar == CalendarID) &&
                                (p.CommitmentId == commitmentID) &&
                                (p.WorkerId == workerID)
                        select p;
            list.AddRange(items);

            OM_GoogleCalendarsEvents calev = null;
            if (list.Count > 0)
                calev = list[0];

            return calev;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendarsEvents UpdateGoogleCalendarEventParentId(MZP_CompanyEntities db, int workerID, int commitmentID, string IdEvent, string CalendarID, int parentId,
                                                Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_GoogleCalendarsEvents retGCE = null;

            try
            {
                var query = from p in db.OM_GoogleCalendarsEvents
                            where (p.IdCalendar == CalendarID) &&
                                  (p.CommitmentId == commitmentID) &&
                                  (p.IdEvent == IdEvent) &&
                                  (p.WorkerId == workerID)
                            select p;

                foreach (OM_GoogleCalendarsEvents gce in query)
                {
                    gce.ParentCommitmentId = parentId;

                    retGCE = gce;
                }

            }
            catch (Exception ex) // così non blocco l'account successivo
            {
                if (excLogger != null)
                    excLogger(ex);
                //throw; // TODO commentare una volta che trace/log/etc.. è a posto
            }

            return retGCE;
        }


        //-------------------------------------------------------------------------------
        public static List<OM_GoogleCalendarsEvents> GetGoogleCalendarsEvents(TbSenderDatabaseInfo cmp, int workerID, string CalendarID)
        {
            List<OM_GoogleCalendarsEvents> list = new List<OM_GoogleCalendarsEvents>();
            if (CalendarID.IsNullOrEmpty() ||
                workerID.Equals(0))
                return list;
                
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_GoogleCalendarsEvents
                            where (p.IdCalendar == CalendarID) &&
                                  (p.WorkerId == workerID)
                            select p;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_GoogleCalendarsEvents> GetGoogleCalendarsEvents(TbSenderDatabaseInfo cmp, int workerID, int  CalendarSubId)
        {
            List<OM_GoogleCalendarsEvents> list = new List<OM_GoogleCalendarsEvents>();
            if (CalendarSubId.Equals(0) ||
                workerID.Equals(0))
                return list;

            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from gce in db.OM_GoogleCalendarsEvents
                            join cw in db.OM_CommitmentsWorkers on gce.CommitmentId equals cw.CommitmentId
                            where gce.WorkerId == workerID
                            && cw.WorkerId == workerID
                            && cw.CalendarSubId == CalendarSubId
                            select gce;

                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static void RemoveGoogleCalendarEvent(TbSenderDatabaseInfo company, int workerID, int commitmentID, string calendarID, string eventID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        RemoveGoogleCalendarEvent(db, workerID, commitmentID, calendarID, eventID, excLogger);

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
        }
        public static void RemoveGoogleCalendarEvent(MZP_CompanyEntities db, int workerID, int commitmentID, string calendarID, string eventID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from cal in db.OM_GoogleCalendarsEvents
                            where ((cal.WorkerId == workerID) &&
                                    (cal.CommitmentId == commitmentID) &&
                                    (cal.IdCalendar == calendarID) &&
                                    (cal.IdEvent == eventID))
                            select cal;

                foreach (OM_GoogleCalendarsEvents currCalEv in query)
                {
                    //Delete All Events??

                    db.OM_GoogleCalendarsEvents.Remove(currCalEv);
                }
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }
        }

        //-------------------------------------------------------------------------------
        public static void RemoveGoogleCalendarEvent(MZP_CompanyEntities db, int workerID, string calendarID, string eventID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from cal in db.OM_GoogleCalendarsEvents
                            where ((cal.WorkerId == workerID) &&
                                    (cal.IdCalendar == calendarID) &&
                                    (cal.IdEvent == eventID))
                            select cal;

                foreach (OM_GoogleCalendarsEvents currCalEv in query)
                {
                    //Delete All Events??

                    db.OM_GoogleCalendarsEvents.Remove(currCalEv);
                }
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }
        }

        //-------------------------------------------------------------------------------
        public static void RemoveGoogleCalendarEvent(MZP_CompanyEntities db, string eventID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from cal in db.OM_GoogleCalendarsEvents
                            where (cal.IdEvent == eventID)
                            select cal;

                foreach (OM_GoogleCalendarsEvents currCalEv in query)
                {
                    //Delete All Events??

                    db.OM_GoogleCalendarsEvents.Remove(currCalEv);
                }
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }
        }

        //-------------------------------------------------------------------------------
        public static List<OM_GoogleCalendarsEvents> GetRecurrenceGoogleCalendarsEvents(TbSenderDatabaseInfo cmp, int workerID, string calendarId, string recurringId)
        {
            List<OM_GoogleCalendarsEvents> list = new List<OM_GoogleCalendarsEvents>();
            if (calendarId.IsNullOrEmpty() ||
                workerID.Equals(0))
                return list;

            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = (from gce in db.OM_GoogleCalendarsEvents
                            join ge in db.OM_GoogleEvents on gce.IdEvent equals ge.Id
                            where gce.WorkerId == workerID
                            && gce.IdCalendar == calendarId
                            && ge.RecurringEventId == recurringId
                             select gce).OrderBy(x => x.CommitmentId);

                list.AddRange(items);
            }
            return list;
        }

    }
}
