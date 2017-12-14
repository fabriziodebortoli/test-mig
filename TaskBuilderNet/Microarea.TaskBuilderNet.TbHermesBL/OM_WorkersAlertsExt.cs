using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_WorkersAlerts
    {
        //-------------------------------------------------------------------------------
        public bool BoolIsMailMessagesSent
        {
            get { return this.IsMailMessagesSent == "1"; }
            set { this.IsMailMessagesSent = value ? "1" : "0"; }
        }
        public bool BoolIsMailMessagesReceived
        {
            get { return this.IsMailMessagesReceived == "1"; }
            set { this.IsMailMessagesReceived = value ? "1" : "0"; }
        }
        public bool BoolIsCalendarChanged
        {
            get { return this.IsCalendarChanged == "1"; }
            set { this.IsCalendarChanged = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public static OM_WorkersAlerts CreateItem(Int32 workerID)
        {
            OM_WorkersAlerts wrkAlerts = new OM_WorkersAlerts();
            wrkAlerts.WorkerId = workerID;
            wrkAlerts.BoolIsMailMessagesReceived = false;
            wrkAlerts.BoolIsMailMessagesSent = false;
            wrkAlerts.BoolIsCalendarChanged = false;
            DateTime now = DateTime.Now;
            wrkAlerts.TBCreated = now;
            wrkAlerts.TBModified = now;
            wrkAlerts.TBCreatedID = workerID;
            wrkAlerts.TBModifiedID = workerID;
            return wrkAlerts;
        }

        //-------------------------------------------------------------------------------
        public static void UpdateMailWorkersAlerts(TbSenderDatabaseInfo company, Int32 workerID, bool IsSent, bool IsReceived,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var query = from wrkAlert in db.OM_WorkersAlerts
                                    where wrkAlert.WorkerId == workerID
                                    select wrkAlert;

                        bool bFound = false;
                        // Execute the query, and change the column values 
                        // you want to change. 
                        foreach (OM_WorkersAlerts currWrk in query)
                        {
                            bFound = true;
                            if (IsSent)
                                currWrk.BoolIsMailMessagesSent = IsSent;
                            if (IsReceived)
                                currWrk.BoolIsMailMessagesReceived = IsReceived;
                        }

                        if (!bFound)
                        {
                            OM_WorkersAlerts record = OM_WorkersAlerts.CreateItem(
                                workerID);
                            if (IsSent)
                                record.BoolIsMailMessagesSent = IsSent;
                            if (IsReceived)
                                record.BoolIsMailMessagesReceived = IsReceived;
                            db.OM_WorkersAlerts.Add(record);
                        }
                            
                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex) // così non blocco l'account successivo
                {
                    if (excLogger != null)
                        excLogger(ex);
                    //throw; // TODO commentare una volta che trace/log/etc.. è a posto
                }
            }
        }

        //-------------------------------------------------------------------------------
        public static void UpdateCalendarWorkersAlerts(TbSenderDatabaseInfo company, Int32 workerID, bool IsCalendarChanged,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger, MailLogListener logTrace)
        {
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        /*
                        var query = from wrkAlert in db.OM_WorkersAlerts
                                    where wrkAlert.WorkerId == workerID
                                    select wrkAlert;

                        bool bFound = false;

                        foreach (OM_WorkersAlerts currWrk in query)
                        {
                            bFound = true;
                            if (IsCalendarChanged)
                                currWrk.BoolIsCalendarChanged = IsCalendarChanged;
                        }

                        */

                        if (IsCalendarChanged)
                        {
                            bool bFound = false;

                            try
                            {
                                OM_WorkersAlerts currWrk = db.OM_WorkersAlerts.Single(e => e.WorkerId == workerID);
                                if (currWrk != null)
                                {
                                    bFound = true;
                                    currWrk.BoolIsCalendarChanged = IsCalendarChanged;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (excLogger != null)
                                    excLogger(ex);
                            }

                            if (!bFound)
                            {
                                OM_WorkersAlerts record = OM_WorkersAlerts.CreateItem(
                                    workerID);
                                if (IsCalendarChanged)
                                    record.BoolIsCalendarChanged = IsCalendarChanged;
                                db.OM_WorkersAlerts.Add(record);
                            }
                        }

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


    }
}
