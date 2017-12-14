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
    public partial class OM_GoogleEvents
    {
        public bool BoolIsDeleted
        {
            get { return this.IsDeleted == "1"; }
            set { this.IsDeleted = value ? "1" : "0"; }
        }
        public bool BoolIsException
        {
            get { return this.IsException == "1"; }
            set { this.IsException = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleEvents CreateItem(MZP_CompanyEntities db, int workerID, Event gooEvent, string IdCalendar, string TimeZone)
        {
            if ((gooEvent.Start == null) || (gooEvent.Start.DateTime == null) || (!gooEvent.Start.DateTime.HasValue) || 
                (gooEvent.End == null) || (gooEvent.End.DateTime == null) || (!gooEvent.End.DateTime.HasValue))
                return null;

            DateTime aOriginalStartTime = gooEvent.Start.DateTime.Value;
            if ((gooEvent.OriginalStartTime != null) &&
                (gooEvent.OriginalStartTime.DateTime != null) &&
                (gooEvent.OriginalStartTime.DateTime.HasValue))
                aOriginalStartTime = gooEvent.OriginalStartTime.DateTime.Value;

            DateTime now = DateTime.Now;

            OM_GoogleEvents ev = new OM_GoogleEvents();
            ev.GoogleEventId = GetMaxEventId(db, workerID) + 1;
            ev.WorkerId = workerID;
            ev.Id = gooEvent.Id;
            ev.Description = gooEvent.Description.IsNullOrEmpty() ? "" : gooEvent.Description;
            ev.EventStart = gooEvent.Start.DateTime.Value;
            ev.EventEnd = gooEvent.End.DateTime.Value;
            ev.TimeZone = gooEvent.Start.TimeZone != null ? gooEvent.Start.TimeZone : TimeZone;
            ev.Summary = gooEvent.Summary.IsNullOrEmpty() ? "" : gooEvent.Summary;
            ev.Location = gooEvent.Location.IsNullOrEmpty() ? "" : gooEvent.Location;
            ev.Status = gooEvent.Status.IsNullOrEmpty() ? "" : gooEvent.Status;
            ev.Updated = (gooEvent.Updated != null) ? gooEvent.Updated.Value : now;
            ev.Visibility = gooEvent.Visibility.IsNullOrEmpty() ? "" : gooEvent.Visibility;
            ev.RecurringEventId = gooEvent.RecurringEventId.IsNullOrEmpty() ? "" : gooEvent.RecurringEventId;
            ev.Recurrence = "";
            if (ev.RecurringEventId.IsNullOrEmpty())
                ev.Recurrence = (gooEvent.Recurrence == null) ? "" : (gooEvent.Recurrence.Count > 0) ? gooEvent.Recurrence[0] : "";
            ev.HtmlLink = gooEvent.HtmlLink.IsNullOrEmpty() ? "" : gooEvent.HtmlLink;
            ev.Creator = gooEvent.Creator.Email.IsNullOrEmpty() ? "" : gooEvent.Creator.Email;
            ev.OriginalStartTime = aOriginalStartTime;
            ev.BoolIsDeleted = false;
            ev.BoolIsException = false;

            ev.TBCreated = now;
            ev.TBModified = now;
            ev.TBCreatedID = workerID;
            ev.TBModifiedID = workerID;
            return ev;
        }

        //-------------------------------------------------------------------------------
        public static int GetMaxEventId(TbSenderDatabaseInfo cmp, int workerID)
        {
            int maxID = 0;
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                maxID = GetMaxEventId(db, workerID);
            }
            return maxID;
        }
        public static int GetMaxEventId(MZP_CompanyEntities db, int workerID)
        {
            int maxID = 0;
            List<OM_GoogleEvents> list = new List<OM_GoogleEvents>();
            var items = (from p in db.OM_GoogleEvents
                         //where (p.WorkerId == workerID)
                         select p);
            list.AddRange(items);
            if ((list != null) && (list.Count > 0))
                maxID = list.Max(p => p.GoogleEventId);

            return maxID;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleEvents GetGoogleEvent(TbSenderDatabaseInfo cmp, string ID)
        {
            if (ID.IsNullOrEmpty())
                return null;

            OM_GoogleEvents ev = null;
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                ev  = GetGoogleEvent(db, ID);
            }

            return ev;
        }
        public static OM_GoogleEvents GetGoogleEvent(MZP_CompanyEntities db, string ID)
        {
            if (ID.IsNullOrEmpty())
                return null;

            List<OM_GoogleEvents> list = new List<OM_GoogleEvents>();
            var items = from p in db.OM_GoogleEvents
                        where (p.Id == ID)
                        select p;
            list.AddRange(items);
            
            OM_GoogleEvents ev = null;
            if (list.Count > 0)
                ev = list[0];

            return ev;
        }

        //-------------------------------------------------------------------------------
        public static void RemoveGoogleEvent(MZP_CompanyEntities db, string eventID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from ev in db.OM_GoogleEvents
                            where (ev.Id == eventID)
                            select ev;

                foreach (OM_GoogleEvents currEv in query)
                {
                    //Delete All Events??

                    db.OM_GoogleEvents.Remove(currEv);
                }
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleEvents UpdateItem(MZP_CompanyEntities db, int workerID, Event gooEvent, string TimeZone,
                                                 Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_GoogleEvents gEv = null;

            if ((!gooEvent.Start.DateTime.HasValue) || (!gooEvent.End.DateTime.HasValue))
                return null;

            DateTime now = DateTime.Now;
            try
            {
                var query = from ge in db.OM_GoogleEvents
                            where ge.Id == gooEvent.Id
                            select ge;

                foreach (OM_GoogleEvents ev in query)
                {
                    ev.Description = gooEvent.Description.IsNullOrEmpty() ? "" : gooEvent.Description;
                    ev.EventStart = gooEvent.Start.DateTime.Value;
                    ev.EventEnd = gooEvent.End.DateTime.Value;
                    ev.TimeZone = gooEvent.Start.TimeZone != null ? gooEvent.Start.TimeZone : TimeZone;
                    ev.Summary = gooEvent.Summary.IsNullOrEmpty() ? "" : gooEvent.Summary;
                    ev.Location = gooEvent.Location.IsNullOrEmpty() ? "" : gooEvent.Location;
                    ev.Status = gooEvent.Status.IsNullOrEmpty() ? "" : gooEvent.Status;
                    ev.Updated = gooEvent.Updated.HasValue ? gooEvent.Updated.Value : now;
                    ev.Visibility = gooEvent.Visibility.IsNullOrEmpty() ? "" : gooEvent.Visibility;
                    ev.RecurringEventId = gooEvent.RecurringEventId.IsNullOrEmpty() ? "" : gooEvent.RecurringEventId;
                    ev.Recurrence = (gooEvent.Recurrence == null) ? "" : (gooEvent.Recurrence.Count > 0) ? gooEvent.Recurrence[0] : "";
                    ev.HtmlLink = gooEvent.HtmlLink.IsNullOrEmpty() ? "" : gooEvent.HtmlLink;
                    ev.Creator = gooEvent.Creator.Email.IsNullOrEmpty() ? "" : gooEvent.Creator.Email;
                    ev.BoolIsDeleted = false;

                    //ev.TBCreated = now;
                    ev.TBModified = now;
                    //ev.TBCreatedID = workerID;
                    ev.TBModifiedID = workerID;

                    gEv = ev;
                }
    
            }
            catch (Exception ex) // così non blocco l'account successivo
            {
                if (excLogger != null)
                    excLogger(ex);
                //throw; // TODO commentare una volta che trace/log/etc.. è a posto
            }
 
            return gEv;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleEvents UpdateItemUpdatedDateTime(MZP_CompanyEntities db, string EventID, DateTime dateUpdate,
                                                 Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_GoogleEvents gEv = null;

            if (dateUpdate == null)
                return null;

            try
            {
                var query = from ge in db.OM_GoogleEvents
                            where ge.Id == EventID
                            select ge;

                foreach (OM_GoogleEvents ev in query)
                {
                    ev.Updated = dateUpdate;
                    
                    gEv = ev;
                }

            }
            catch (Exception ex) // così non blocco l'account successivo
            {
                if (excLogger != null)
                    excLogger(ex);
                //throw; // TODO commentare una volta che trace/log/etc.. è a posto
            }

            return gEv;
        }


        //-------------------------------------------------------------------------------
        public static OM_GoogleEvents UpdateItem(MZP_CompanyEntities db, OM_Commitments comm, OM_GoogleCalendarsEvents gCalEv, string  timeZone,
                                                 Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_GoogleEvents gEv = null;

            DateTime now = DateTime.Now;
            try
            {
                var query = from ge in db.OM_GoogleEvents
                            where ge.Id == gCalEv.IdEvent
                            //&& ge.WorkerId == gCalEv.WorkerId
                            select ge;

                foreach (OM_GoogleEvents ev in query)
                {
                    ev.Description = comm.Description.IsNullOrEmpty() ? "" : comm.Description;
                    ev.EventStart = comm.CommitmentDate.Add(comm.StartTime.TimeOfDay);
                    ev.EventEnd = comm.CommitmentDate.Add(comm.EndTime.TimeOfDay);
                    ev.TimeZone = timeZone;
                    ev.Summary = comm.Subject.IsNullOrEmpty() ? "" : comm.Subject;
                    ev.Location = comm.Location.IsNullOrEmpty() ? "" : comm.Location;
                    //ev.Status = comm.Status.IsNullOrEmpty() ? "" : comm.Status;
                    ev.Updated = comm.TBModified;
                    //ev.Visibility = gooEvent.Visibility.IsNullOrEmpty() ? "" : gooEvent.Visibility;
                    //ev.RecurringEventId = gooEvent.RecurringEventId.IsNullOrEmpty() ? "" : gooEvent.RecurringEventId;
                    //ev.Recurrence = (gooEvent.Recurrence == null) ? "" : (gooEvent.Recurrence.Count > 0) ? gooEvent.Recurrence[0] : "";
                    //ev.HtmlLink = gooEvent.HtmlLink.IsNullOrEmpty() ? "" : gooEvent.HtmlLink;
                    //ev.Creator = gooEvent.Creator.Email.IsNullOrEmpty() ? "" : gooEvent.Creator.Email;
                    ev.BoolIsDeleted = false;

                    //ev.TBCreated = now;
                    ev.TBModified = now;
                    //ev.TBCreatedID = workerID;
                    ev.TBModifiedID = gCalEv.WorkerId;

                    gEv = ev;
                }

            }
            catch (Exception ex) // così non blocco l'account successivo
            {
                if (excLogger != null)
                    excLogger(ex);
                //throw; // TODO commentare una volta che trace/log/etc.. è a posto
            }

            return gEv;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleEvents SetExceptionGoogleEvent(MZP_CompanyEntities db, string eventId, bool exception,
                                                 Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_GoogleEvents gEv = null;

            try
            {
                var query = from ge in db.OM_GoogleEvents
                            where ge.Id == eventId
                            select ge;

                foreach (OM_GoogleEvents ev in query)
                {
                    ev.BoolIsException = exception;

                    gEv = ev;
                }

            }
            catch (Exception ex) // così non blocco l'account successivo
            {
                if (excLogger != null)
                    excLogger(ex);
                //throw; // TODO commentare una volta che trace/log/etc.. è a posto
            }

            return gEv;
        }

    }
}
