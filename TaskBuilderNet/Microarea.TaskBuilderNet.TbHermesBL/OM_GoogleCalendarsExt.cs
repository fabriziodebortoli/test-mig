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
    public partial class OM_GoogleCalendars
    {
        //-------------------------------------------------------------------------------
        public bool BoolIsDeleted
        {
            get { return this.IsDeleted == "1"; }
            set { this.IsDeleted = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsPrimary
        {
            get { return this.IsPrimary == "1"; }
            set { this.IsPrimary = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendars CreateItem(TbSenderDatabaseInfo cmp, int workerID, CalendarListEntry gooCal)
        {
            OM_GoogleCalendars cal = new OM_GoogleCalendars();
            cal.WorkerId = workerID;
            cal.Id = gooCal.Id;
            cal.TimeZone = gooCal.TimeZone;
            cal.BoolIsDeleted = gooCal.Deleted.HasValue ? gooCal.Deleted.Value : false;
            cal.Summary = gooCal.Summary.IsNullOrEmpty() ? (gooCal.SummaryOverride.IsNullOrEmpty() ? gooCal.Id : gooCal.SummaryOverride) : gooCal.Summary;
            cal.Description = gooCal.Description.IsNullOrEmpty() ? "" : gooCal.Description;
            cal.BackgroundColor = gooCal.BackgroundColor.IsNullOrEmpty() ? "" : gooCal.BackgroundColor;
            cal.BkgColorR = gooCal.BackgroundColor.IsNullOrEmpty() ? 0 : System.Drawing.ColorTranslator.FromHtml(gooCal.BackgroundColor).R;
            cal.BkgColorG = gooCal.BackgroundColor.IsNullOrEmpty() ? 0 : System.Drawing.ColorTranslator.FromHtml(gooCal.BackgroundColor).G;
            cal.BkgColorB = gooCal.BackgroundColor.IsNullOrEmpty() ? 0 : System.Drawing.ColorTranslator.FromHtml(gooCal.BackgroundColor).B;
            cal.BoolIsPrimary = gooCal.Primary.HasValue ? gooCal.Primary.Value : false;

            DateTime now = DateTime.Now;
            cal.TBCreated = now;
            cal.TBModified = now;
            cal.TBCreatedID = workerID;
            cal.TBModifiedID = workerID;
            return cal;
        }

        //-------------------------------------------------------------------------------
        public static bool CheckUpdateCalendar(TbSenderDatabaseInfo cmp, CalendarListEntry gooCal, OM_GoogleCalendars cal,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;

            string TimeZone = gooCal.TimeZone;
            string Summary = gooCal.Summary.IsNullOrEmpty() ? (gooCal.SummaryOverride.IsNullOrEmpty() ? gooCal.Id : gooCal.SummaryOverride) : gooCal.Summary;
            string Description = gooCal.Description.IsNullOrEmpty() ? "" : gooCal.Description;
            string BackgroundColor = gooCal.BackgroundColor.IsNullOrEmpty() ? "" : gooCal.BackgroundColor;

            if ((TimeZone != cal.TimeZone) ||
                (Summary != cal.Summary) ||
                (Description != cal.Description) ||
                (BackgroundColor != cal.BackgroundColor))
            {
                using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
                {
                    try
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            var query = from gc in db.OM_GoogleCalendars
                                        where ((gc.WorkerId == cal.WorkerId) &&
                                               (gc.Id == cal.Id))
                                        select gc;

                            foreach (OM_GoogleCalendars currCal in query)
                            {
                                currCal.TimeZone = TimeZone;
                                currCal.Summary = Summary;
                                currCal.Description = Description;
                                currCal.BackgroundColor = BackgroundColor;
                                currCal.BkgColorR = BackgroundColor.IsNullOrEmpty() ? 0 : System.Drawing.ColorTranslator.FromHtml(BackgroundColor).R;
                                currCal.BkgColorG = BackgroundColor.IsNullOrEmpty() ? 0 : System.Drawing.ColorTranslator.FromHtml(BackgroundColor).G;
                                currCal.BkgColorB = BackgroundColor.IsNullOrEmpty() ? 0 : System.Drawing.ColorTranslator.FromHtml(BackgroundColor).B;
                                DateTime now = DateTime.Now;
                                currCal.TBModified = now;
                                currCal.TBModifiedID = cal.WorkerId;

                            }

                            db.SaveChanges();
                            ts.Complete();
                            ret = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        ret = false;
                        string a = ex.Message;
                        if (excLogger != null)
                            excLogger(ex);
                    }
                }
            }        
            return ret;
        }

        //-------------------------------------------------------------------------------
        public static bool SaveGoogleCalendar(TbSenderDatabaseInfo company, int aWorkerID, CalendarListEntry gooCal,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        OM_GoogleCalendars cal = OM_GoogleCalendars.CreateItem(company, aWorkerID, gooCal);
                        db.OM_GoogleCalendars.Add(cal);

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }

            return ret;
        }

        //-------------------------------------------------------------------------------
        public static OM_GoogleCalendars GetGoogleCalendar(TbSenderDatabaseInfo cmp, int workerID, string ID)
        {
            if (ID.IsNullOrEmpty())
                return null;

            List<OM_GoogleCalendars> list = new List<OM_GoogleCalendars>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_GoogleCalendars
                             where ((p.WorkerId == workerID) &&
                                    (p.Id == ID))
                             select p;
                list.AddRange(items);
            }
            OM_GoogleCalendars cal = null;
            if (list.Count > 0)
                cal = list[0];

            return cal;
        }

        //-------------------------------------------------------------------------------
        public static void RemoveGoogleCalendar(TbSenderDatabaseInfo company, int aWorkerID, string IdCalendar,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        RemoveGoogleCalendar(db, aWorkerID, IdCalendar, excLogger);

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
        public static void RemoveGoogleCalendar(MZP_CompanyEntities db, int aWorkerID, string IdCalendar,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from cal in db.OM_GoogleCalendars
                            where ((cal.WorkerId == aWorkerID) &&
                                    (cal.Id == IdCalendar))
                            select cal;

                foreach (OM_GoogleCalendars currCal in query)
                {
                    db.OM_GoogleCalendars.Remove(currCal);
                }
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }
        }

        //-------------------------------------------------------------------------------
        public static string GetGoogleCalendarPrimaryAddress(TbSenderDatabaseInfo cmp, int workerID)
        {
            string address = "";
            List<OM_GoogleCalendars> list = new List<OM_GoogleCalendars>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_GoogleCalendars
                            where ((p.WorkerId == workerID) &&
                                   (p.IsPrimary != "0"))
                            select p;
                list.AddRange(items);
            }
         
            OM_GoogleCalendars cal = null;
            if (list.Count > 0)
                cal = list[0];
            if (cal != null)
                address = cal.Id;

            return address;
        }
    }
}
