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
    public partial class OM_CommitmentsWorkers
    {
        //-------------------------------------------------------------------------------
        public bool BoolIsOwner
        {
            get { return this.IsOwner == "1"; }
            set { this.IsOwner = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsImminent
        {
            get { return this.IsImminent == "1"; }
            set { this.IsImminent = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolHasReminder
        {
            get { return this.HasReminder == "1"; }
            set { this.HasReminder = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public static OM_CommitmentsWorkers CreateItem(int commID, int workerID, int calendarSubId, bool isOwner)
        {
            OM_CommitmentsWorkers commwor = new OM_CommitmentsWorkers();

            commwor.CommitmentId = commID;
            commwor.WorkerId = workerID;
            commwor.CalendarSubId = calendarSubId;
            commwor.BoolIsOwner = isOwner;
            commwor.BoolIsImminent = false;
            commwor.BoolHasReminder = false;

            DateTime now = DateTime.Now;
            commwor.TBCreated = now;
            commwor.TBModified = now;
            commwor.TBCreatedID = workerID;
            commwor.TBModifiedID = workerID;

            return commwor;
        }

        //-------------------------------------------------------------------------------
        public static OM_CommitmentsWorkers UpdateItem(MZP_CompanyEntities db, int workerID, int commitmentID, OM_WorkersCalendars wCal, OM_GoogleEvents gEv,
                                                Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_CommitmentsWorkers commwor = null;
            DateTime updateTime = gEv.Updated;

            try
            {
                var query = from cw in db.OM_CommitmentsWorkers
                            where cw.CommitmentId == commitmentID
                            && cw.WorkerId == workerID
                            select cw;

                foreach (OM_CommitmentsWorkers comw in query)
                {
                    comw.CalendarSubId = wCal.SubId;

                    //ev.TBCreated = now;
                    comw.TBModified = updateTime;
                    //ev.TBCreatedID = workerID;
                    comw.TBModifiedID = workerID;

                    commwor = comw;
                }

            }
            catch (Exception ex) // così non blocco l'account successivo
            {
                if (excLogger != null)
                    excLogger(ex);
                //throw; // TODO commentare una volta che trace/log/etc.. è a posto
            }

            return commwor;
        }

        //-------------------------------------------------------------------------------
        public static OM_CommitmentsWorkers GetCommitmentsWorkers(MZP_CompanyEntities db, int commitmentID, int workerId)
        {
            List<OM_CommitmentsWorkers> list = new List<OM_CommitmentsWorkers>();
            var items = from c in db.OM_CommitmentsWorkers
                        where c.CommitmentId == commitmentID
                        && c.WorkerId == workerId
                        select c;
            list.AddRange(items);

            OM_CommitmentsWorkers comm = null;
            if (list.Count > 0)
                comm = list[0];

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_CommitmentsWorkers> GetGuestListWorkers(TbSenderDatabaseInfo cmp, int commitmentID)
        {
            List<OM_CommitmentsWorkers> list = new List<OM_CommitmentsWorkers>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            where cw.CommitmentId == commitmentID
                            && (cw.IsOwner == "0")
                            select cw;
                list.AddRange(items);
            }

            return list;
        }

        //-------------------------------------------------------------------------------
        public static OM_CommitmentsWorkers GetCommitmentsWorkers(TbSenderDatabaseInfo cmp, int commitmentID, int workerId)
        {
            List<OM_CommitmentsWorkers> list = new List<OM_CommitmentsWorkers>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            where cw.CommitmentId == commitmentID
                            && cw.WorkerId == workerId
                            select cw;
                list.AddRange(items);
            }
            OM_CommitmentsWorkers comm = null;
            if (list.Count > 0)
                comm = list[0];

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static void RemoveCommitmentWorker(MZP_CompanyEntities db, int commitmentID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from com in db.OM_CommitmentsWorkers
                            where (com.CommitmentId == commitmentID)
                            select com;

                foreach (OM_CommitmentsWorkers currComW in query)
                {
                    //Delete All Events??
                    db.OM_CommitmentsWorkers.Remove(currComW);
                }
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }
        }

        //-------------------------------------------------------------------------------
        public static void RemoveCommitmentWorker(MZP_CompanyEntities db, int commitmentID, int workerID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from com in db.OM_CommitmentsWorkers
                            where (com.CommitmentId == commitmentID)
                               && (com.WorkerId == workerID)
                            select com;

                foreach (OM_CommitmentsWorkers currComW in query)
                {
                    //Delete All Events??
                    db.OM_CommitmentsWorkers.Remove(currComW);
                }
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }
        }

        //-------------------------------------------------------------------------------
        public static void RemoveAllCommitmentWorker(MZP_CompanyEntities db, int commitmentID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from com in db.OM_CommitmentsWorkers
                            where (com.CommitmentId == commitmentID)
                            select com;

                foreach (OM_CommitmentsWorkers currComW in query)
                {
                    //Delete All Events??
                    db.OM_CommitmentsWorkers.Remove(currComW);
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
