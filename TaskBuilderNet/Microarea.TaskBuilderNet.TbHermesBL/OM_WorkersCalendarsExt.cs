using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbHermesBL;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_WorkersCalendars
    {
        //-------------------------------------------------------------------------------
        public bool BoolIsPrivate
        {
            get { return this.IsPrivate == "1"; }
            set { this.IsPrivate = value ? "1" : "0"; }
        }
        public bool BoolIsDefault
        {
            get { return this.IsDefault == "1"; }
            set { this.IsDefault = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public static List<OM_WorkersCalendars> GetWorkersCalendars(TbSenderDatabaseInfo cmp, int aWorkerId)
        {
            List<OM_WorkersCalendars> list = new List<OM_WorkersCalendars>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_WorkersCalendars
                            where p.WorkerId == aWorkerId 
                            select p;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static OM_WorkersCalendars GetWorkersCalendar(TbSenderDatabaseInfo cmp, int aWorkerId, string calendarID)
        {
            OM_WorkersCalendars ret = null;
            List<OM_WorkersCalendars> list = new List<OM_WorkersCalendars>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_WorkersCalendars
                            where (p.WorkerId == aWorkerId) &&
                                  (p.IdGoogleCalendar == calendarID)
                            select p;
                list.AddRange(items);
                if (list.Count > 0)
                    ret = list[0];
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        public static OM_WorkersCalendars RemoveSynchronizationToWorkersCalendar(MZP_CompanyEntities db, int aWorkerId, string calendarID)
        {
            OM_WorkersCalendars ret = null;
            List<OM_WorkersCalendars> list = new List<OM_WorkersCalendars>();

            var query = from p in db.OM_WorkersCalendars
                        where (p.WorkerId == aWorkerId) &&
                              (p.IdGoogleCalendar == calendarID)
                        select p;
            foreach(OM_WorkersCalendars wc in query)
            {
                wc.IdGoogleCalendar = "";
                ret = wc;
            }

            return ret;
        }

    }
}
