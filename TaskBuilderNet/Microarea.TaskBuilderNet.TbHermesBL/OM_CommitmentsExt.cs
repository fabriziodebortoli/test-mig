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
    public partial class OM_Commitments
    {
        //-------------------------------------------------------------------------------
        public enum CommitmentType
        {
            Commitment  = 33357824,
            Deadline    = 33357825
        }
        public OM_Commitments.CommitmentType EnumCommitmentType
        {
            get { return (OM_Commitments.CommitmentType)Enum.Parse(typeof(OM_Commitments.CommitmentType), this.CommitmentKind.ToString(), true); }
            set { this.CommitmentKind = (Int32)value; }
        }

        //-------------------------------------------------------------------------------
        public enum CommitmentRecurrenceType
        {
            None = 33423360,
            Day = 33423361,
            Week = 33423362,
            Month = 33423363,
            Year = 33423364
        }
        public OM_Commitments.CommitmentRecurrenceType EnumCommitmentRecurrenceType
        {
            get { return (OM_Commitments.CommitmentRecurrenceType)Enum.Parse(typeof(OM_Commitments.CommitmentRecurrenceType), this.RecurrenceType.ToString(), true); }
            set { this.RecurrenceType = (Int32)value; }
        }

        //-------------------------------------------------------------------------------
        public enum CommitmentRecurrenceEnd
        {
            Never   =   35258368,
            Days    =   35258369,
            Date    =   35258370
        }
        public OM_Commitments.CommitmentRecurrenceEnd EnumCommitmentRecurrenceEnd
        {
            get { return (OM_Commitments.CommitmentRecurrenceEnd)Enum.Parse(typeof(OM_Commitments.CommitmentRecurrenceEnd), this.RecurrenceEndType.ToString(), true); }
            set { this.RecurrenceEndType = (Int32)value; }
        }

        //-------------------------------------------------------------------------------
        public bool BoolIsPrivate
        {
            get { return this.IsPrivate == "1"; }
            set { this.IsPrivate = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public bool BoolIsRecurring
        {
            get { return this.IsRecurring == "1"; }
            set { this.IsRecurring = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolRecurrenceException
        {
            get { return this.RecurrenceException == "1"; }
            set { this.RecurrenceException = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsRecurrenceMonthlyDay
        {
            get { return this.IsRecurrenceMonthlyDay == "1"; }
            set { this.IsRecurrenceMonthlyDay = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsRecurrenceMonthlyWeek
        {
            get { return this.IsRecurrenceMonthlyWeek == "1"; }
            set { this.IsRecurrenceMonthlyWeek = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsNotEditable
        {
            get { return this.IsNotEditable == "1"; }
            set { this.IsNotEditable = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public static OM_Commitments CreateItem(int commID, int workerID, OM_WorkersCalendars cal, OM_GoogleAccounts gAcc, OM_GoogleEvents ev)
        {
            OM_Commitments comm = new OM_Commitments();

            comm.CommitmentId = commID;
            comm.EnumCommitmentType = CommitmentType.Commitment;    //CommitmentKind

            comm.WorkerId = workerID;
            comm.CalendarSubId = cal.SubId;
            comm.Description = ev.Description;
            comm.Subject = ev.Summary;
            comm.CommitmentDate = ev.EventStart.Date;
            comm.StartTime = new DateTime(1899, 12, 30) + ev.EventStart.TimeOfDay;
            comm.EndTime = new DateTime(1899, 12, 30) + ev.EventEnd.TimeOfDay;
            TimeSpan length = ev.EventEnd - ev.EventStart;
            comm.Length = (60 * 60 * length.Hours) + (60 * length.Minutes) + length.Seconds;
            comm.Location = ev.Location;
            comm.BoolIsPrivate = cal.BoolIsPrivate; //(ev.Visibility == "");
            comm.OriginalStartDate = ev.OriginalStartTime;
            comm.OriginalParentStartDate = new DateTime(1799, 12, 31);
            comm.BoolIsNotEditable = (gAcc.PrimaryAddress != ev.Creator);
            DateTime now = DateTime.Now;
            comm.TBCreated = now;
            comm.TBModified = ev.Updated;
            comm.TBCreatedID = workerID;
            comm.TBModifiedID = workerID;
            
            //Setto i valori di Default
            comm.BoolIsRecurring = false;
            //comm.IsRecurring = "0";
            comm.MasterCode = "";
            comm.ReferenceCode = "";
	        comm.OfficeFileId = 0;
	        comm.TaskTypeId = 0;
            comm.TTDescription = "";
          	comm.STDeadlineType = 0;
	        comm.QuantityToDo = 0;
        	comm.QuantityDone = 0;
	        comm.ReminderBefore = 0;
            comm.HasReminder = "0";
	        comm.ReminderKindBefore = 33488896;
            comm.ReminderTime = new DateTime(1899, 12, 30);
	        comm.ImminentDays = 0;
	        comm.IsImminent = "0";
	        comm.IsGeneric = "0";
	        comm.Telephone = "";
	        comm.InternalNote = "";
	        comm.MasterNote = "";
	        comm.IsClosed = "0";
	        comm.DeadlineStatus = 34209792;
	        comm.AutomaticGenerated = "0";
            comm.RecurrenceId = 0;
            comm.RecurrenceSubId = 0;
            comm.RecurrenceEndDate = new DateTime(1799,12,31);
	        comm.RecurrenceOccurrences = 0;
	        comm.EnumCommitmentRecurrenceType = CommitmentRecurrenceType.None;
            comm.BoolRecurrenceException = false;
            //comm.RecurrenceException = "0";
	        comm.RecurrenceEvery = 0;
	        comm.RecurrenceWeekdays = 0;
            comm.EnumCommitmentRecurrenceEnd = CommitmentRecurrenceEnd.Never;
            comm.BoolIsRecurrenceMonthlyDay = true;
            comm.BoolIsRecurrenceMonthlyWeek = false;
            //comm.IsRecurrenceMonthlyDay = "0";
            //comm.IsRecurrenceMonthlyWeek = "0";
            comm.RecurrenceMonthlyWeeks = 0;
            comm.RecurrenceOldRecurrenceId = 0;
            comm.Priority = 35520513;
	        comm.Origin = "";
	        comm.RecurrenceId = 0;
	        comm.LinkedDlGroupId = 0;
	        comm.LinkedDlId = 0;
	        comm.LinkedDlSubId = 0;
	        comm.LinkedDlStep = 0;
	        comm.LinkedDlParentStep = 0;
	        comm.LinkedDlMain = "0";
	        comm.LinkedDlFixed = "0";
	        comm.LinkedDlSign = 32833537;
	        comm.LinkedDlDays = 0;
	        comm.MinReminderDate = new DateTime(1799,12,31);
	        comm.MaxReminderDate = new DateTime(1799,12,31);
            comm.MaxCommitmendDate = ev.EventStart.Date;
	        comm.FromOrgToTaskStatus = 35586048;
            comm.TBGuid = Guid.NewGuid();

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_Commitments> GetSingleOwnerCommitments(TbSenderDatabaseInfo cmp, int aWorkerId, int calendarSubID, DateTime minLimit, DateTime maxLimit)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == aWorkerId
                            && cw.IsOwner != "0"
                            && cw.CalendarSubId == calendarSubID
                            && ((c.IsRecurring == "0") && (c.RecurrenceException == "0"))
                            && c.RecurrenceId == 0
                            && c.CommitmentKind == 33357824
                            && c.CommitmentDate > minLimit
                            && c.CommitmentDate < maxLimit
                            select c;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_Commitments> GetRecurrenceOwnerMainCommitments(TbSenderDatabaseInfo cmp, int aWorkerId, int calendarSubID, DateTime minLimit, DateTime maxLimit)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == aWorkerId
                            && cw.IsOwner != "0"
                            && cw.CalendarSubId == calendarSubID
                            && ((c.IsRecurring != "0") || (c.RecurrenceException != "0"))
                            && c.RecurrenceSubId == 1
                            && c.CommitmentKind == 33357824
                            && c.CommitmentDate > minLimit
                            && c.CommitmentDate < maxLimit
                            select c;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_Commitments> GetRecurrenceGuestMainCommitments(TbSenderDatabaseInfo cmp, int aWorkerId, int calendarSubID, DateTime minLimit, DateTime maxLimit)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == aWorkerId
                            && cw.IsOwner == "0"
                            && cw.CalendarSubId == calendarSubID
                            && c.IsRecurring != "0"
                            && c.RecurrenceSubId == 1
                            && c.CommitmentKind == 33357824
                            && c.CommitmentDate > minLimit
                            && c.CommitmentDate < maxLimit
                            select c;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_Commitments> GetRecurrenceOwnerCommitments(TbSenderDatabaseInfo cmp, int aWorkerId, int calendarSubID, DateTime minLimit, DateTime maxLimit)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == aWorkerId
                            && cw.IsOwner != "0"
                            && cw.CalendarSubId == calendarSubID
                            && ((c.IsRecurring != "0") || (c.RecurrenceException != "0"))
                            && c.RecurrenceId != 0
                            && c.CommitmentKind == 33357824
                            && c.CommitmentDate > minLimit
                            && c.CommitmentDate < maxLimit
                            select c;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_Commitments> GetRecurrenceGuestCommitments(TbSenderDatabaseInfo cmp, int aWorkerId, int calendarSubID, DateTime minLimit, DateTime maxLimit)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == aWorkerId
                            && cw.IsOwner == "0"
                            && cw.CalendarSubId == calendarSubID
                            && ((c.IsRecurring != "0") || (c.RecurrenceException != "0"))
                            && c.RecurrenceId != 0
                            && c.CommitmentKind == 33357824
                            && c.CommitmentDate > minLimit
                            && c.CommitmentDate < maxLimit
                            select c;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_Commitments> GetGuestCommitments(TbSenderDatabaseInfo cmp, int aWorkerId, int calendarSubID, DateTime minLimit, DateTime maxLimit)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == aWorkerId
                            && cw.IsOwner == "0"
                            && cw.CalendarSubId == calendarSubID
                            && c.IsRecurring == "0"
                            && c.RecurrenceId == 0
                            && c.CommitmentKind == 33357824
                            && c.CommitmentDate > minLimit
                            && c.CommitmentDate < maxLimit
                            select c;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_Commitments> GetRecurrenceCommitments(TbSenderDatabaseInfo cmp, int aWorkerId, int calendarSubID, int RecurrenceId)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = (from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == aWorkerId
                            //&& cw.IsOwner != "0"
                            && cw.CalendarSubId == calendarSubID
                            && c.CommitmentKind == 33357824
                            && c.RecurrenceId == RecurrenceId
                            select c).OrderBy(x=>x.RecurrenceSubId);
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static int GetRecurrenceIDFromRecurrenceEvent(TbSenderDatabaseInfo cmp, int workerId, string calendarId, string RecurrenceEventId)
        {
            int ret = 0;
            List<int> listGCE = new List<int>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from ge in db.OM_GoogleEvents
                            join gce in db.OM_GoogleCalendarsEvents on ge.Id equals gce.IdEvent
                            where gce.WorkerId == workerId
                            && gce.IdCalendar == calendarId
                            && ge.RecurringEventId == RecurrenceEventId
                            select gce.CommitmentId;

                listGCE.AddRange(items);

                if (listGCE.Count > 0)
                {
                    List<OM_Commitments> listC = new List<OM_Commitments>();
                    var c_items = from c in db.OM_Commitments
                                  where listGCE.Contains(c.CommitmentId)
                                select c;
                    listC.AddRange(c_items);

                    if (listC.Count > 0)
                    {
                        foreach (OM_Commitments c in listC)
                        {
                            ret = c.RecurrenceId;
                            if (ret != 0)
                                break;
                        }
                    }
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        public static int GetMaxRecurrenceSubIdFromRecurrenceId(TbSenderDatabaseInfo cmp, int workerId, int calendarId, int RecurrenceId)
        {
            int ret = 1;
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == workerId
                            && cw.CalendarSubId == calendarId
                            && c.CommitmentKind == 33357824
                            && c.RecurrenceId == RecurrenceId
                            select c;
                list.AddRange(items);

                if (list.Count > 0)
                {
                    foreach (OM_Commitments c in list)
                    {
                        if (c.RecurrenceSubId > ret)
                            ret = c.RecurrenceSubId;
                    }
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        public static OM_Commitments GetNonExceptionCommitmentFromRecurrenceId(TbSenderDatabaseInfo cmp, int workerId, int calendarId, int RecurrenceId)
        {
            OM_Commitments comm = null;
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = (from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == workerId
                            && cw.CalendarSubId == calendarId
                            && c.CommitmentKind == 33357824
                            && c.RecurrenceId == RecurrenceId
                            select c).OrderBy(x=>x.RecurrenceSubId);
                list.AddRange(items);

                if (list.Count > 0)
                {
                    foreach (OM_Commitments c in list)
                    {
                        if (!c.BoolRecurrenceException)
                        {
                            comm = c;
                            break;
                        }
                    }
                }
            }
            return comm;
        }

        //-------------------------------------------------------------------------------
        public static OM_Commitments GetCommitment(TbSenderDatabaseInfo cmp, int aWorkerId, int calendarSubID, int commitmentID)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == aWorkerId
                            && cw.CalendarSubId == calendarSubID
                            && c.CommitmentKind == 33357824
                            && c.CommitmentId == commitmentID
                            select c;
                list.AddRange(items);
            }
            OM_Commitments comm = null;
            if (list.Count > 0)
                comm = list[0];

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static OM_Commitments GetCommitment(MZP_CompanyEntities db, int commitmentID)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            var items = from c in db.OM_Commitments 
                        where c.CommitmentId == commitmentID
                        select c;
            list.AddRange(items);

            OM_Commitments comm = null;
            if (list.Count > 0)
                comm = list[0];

            return comm;
        }


        //-------------------------------------------------------------------------------
        public static OM_Commitments GetCommitment(MZP_CompanyEntities db, int aWorkerId, int calendarSubID, int commitmentID)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            var items = from cw in db.OM_CommitmentsWorkers
                        join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                        where cw.WorkerId == aWorkerId
                        && cw.CalendarSubId == calendarSubID
                        && c.CommitmentKind == 33357824
                        && c.CommitmentId == commitmentID
                        select c;
            list.AddRange(items);

            OM_Commitments comm = null;
            if (list.Count > 0)
                comm = list[0];

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static OM_Commitments GetCommitment(TbSenderDatabaseInfo cmp, int commitmentID)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from c in db.OM_Commitments
                            where c.CommitmentId == commitmentID
                            select c;
                list.AddRange(items);
            }
            OM_Commitments comm = null;
            if (list.Count > 0)
                comm = list[0];

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static OM_Commitments GetCommitmentRecurrenceChild(TbSenderDatabaseInfo cmp, int aWorkerId, int calendarSubID, int recurrenceId, int recurrenceSubId)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from cw in db.OM_CommitmentsWorkers
                            join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                            where cw.WorkerId == aWorkerId
                            && cw.CalendarSubId == calendarSubID
                            && c.CommitmentKind == 33357824
                            && c.RecurrenceId == recurrenceId
                            && c.RecurrenceSubId == recurrenceSubId
                            select c;
                list.AddRange(items);
            }
            OM_Commitments comm = null;
            if (list.Count > 0)
                comm = list[0];

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static OM_Commitments GetCommitmentRecurrenceChild(MZP_CompanyEntities db, int aWorkerId, int calendarSubID, int recurrenceId, int recurrenceSubId)
        {
            List<OM_Commitments> list = new List<OM_Commitments>();
            var items = from cw in db.OM_CommitmentsWorkers
                        join c in db.OM_Commitments on cw.CommitmentId equals c.CommitmentId
                        where cw.WorkerId == aWorkerId
                        && cw.CalendarSubId == calendarSubID
                        && c.CommitmentKind == 33357824
                        && c.RecurrenceId == recurrenceId
                        && c.RecurrenceSubId == recurrenceSubId
                        select c;
            list.AddRange(items);

            OM_Commitments comm = null;
            if (list.Count > 0)
                comm = list[0];

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static OM_Commitments UpdateItem(MZP_CompanyEntities db, int workerID, int commitmentID, OM_WorkersCalendars wCal, OM_GoogleEvents gEv,
                                                Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_Commitments comm = null;
            DateTime updateTime = gEv.Updated;

            try
            {
                var query = from cm in db.OM_Commitments
                            where cm.CommitmentId == commitmentID
                            && cm.WorkerId == workerID
                            select cm;

                foreach (OM_Commitments com in query)
                {
                    com.Description = gEv.Description;
                    com.Subject = gEv.Summary;
                    com.CommitmentDate = gEv.EventStart.Date;
                    com.CalendarSubId = wCal.SubId;
                    com.StartTime = new DateTime(1899, 12, 30) + gEv.EventStart.TimeOfDay;
                    com.EndTime = new DateTime(1899, 12, 30) + gEv.EventEnd.TimeOfDay;
                    TimeSpan length = com.EndTime - com.StartTime;
                    com.Length = (60 * 60 * length.Hours) + (60 * length.Minutes) + length.Seconds;
                    com.Location = gEv.Location;
                    //.BoolIsPrivate = (gEv.Visibility == "private");

                    //ev.TBCreated = now;
                    com.TBModified = updateTime;
                    //ev.TBCreatedID = workerID;
                    com.TBModifiedID = workerID;

                    comm = com;
                }

            }
            catch (Exception ex) // così non blocco l'account successivo
            {
                if (excLogger != null)
                    excLogger(ex);
                //throw; // TODO commentare una volta che trace/log/etc.. è a posto
            }

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static OM_Commitments UpdateItemUpdatedDateTime(MZP_CompanyEntities db, int workerID, int commitmentID, DateTime updateTime,
                                                Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_Commitments comm = null;
          
            try
            {
                var query = from cm in db.OM_Commitments
                            where cm.CommitmentId == commitmentID
                            //&& cm.WorkerId == workerID
                            select cm;

                foreach (OM_Commitments com in query)
                {
                    com.TBModified = updateTime;
                    com.TBModifiedID = workerID;

                    comm = com;
                }

            }
            catch (Exception ex) // così non blocco l'account successivo
            {
                if (excLogger != null)
                    excLogger(ex);
                //throw; // TODO commentare una volta che trace/log/etc.. è a posto
            }

            return comm;
        }

        //-------------------------------------------------------------------------------
        public static void RemoveCommitment(MZP_CompanyEntities db, int commitmentID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from com in db.OM_Commitments
                            where (com.CommitmentId == commitmentID)
                            select com;

                foreach (OM_Commitments currCom in query)
                {
                    //Delete All Events??
                    db.OM_Commitments.Remove(currCom);
                }
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }
        }

        //-------------------------------------------------------------------------------
        public static string GetRecurrenceString(IList<String> listRecurrence)
        {
            string strRule = "RRULE:";

            string strRecurrence = "";
            foreach (string s in listRecurrence)
            {
                if (s.IndexOf(strRule) >= 0)
                    strRecurrence = s;
            }

            return strRecurrence;
        }

        //-------------------------------------------------------------------------------
        public static string RecurrenceWeekDays(int iValue)
        {
            string sDays = "";
            if (iValue > 0)
            {
                bool comma = false;
                sDays = "";
                if (iValue - 64 >= 0) //Sunday
                {
                    if (comma)
                        sDays = "," + sDays;
                    sDays = "SU" + sDays;
                    iValue -= 64;
                    comma = true;
                }
                if (iValue - 32 >= 0) //Saturday
                {
                    if (comma)
                        sDays = "," + sDays;
                    sDays = "SA" + sDays;
                    iValue -= 32;
                    comma = true;
                }
                if (iValue - 16 >= 0) //Friday
                {
                    if (comma)
                        sDays = "," + sDays;
                    sDays = "FR" + sDays;
                    iValue -= 16;
                    comma = true;
                }
                if (iValue - 8 >= 0) //Thursday
                {
                    if (comma)
                        sDays = "," + sDays;
                    sDays = "TH" + sDays;
                    iValue -= 8;
                    comma = true;
                }
                if (iValue - 4 >= 0) //Wednesday
                {
                    if (comma)
                        sDays = "," + sDays;
                    sDays = "WE" + sDays;
                    iValue -= 4;
                    comma = true;
                }
                if (iValue - 2 >= 0) //Tuesday
                {
                    if (comma)
                        sDays = "," + sDays;
                    sDays = "TU" + sDays;
                    iValue -= 2;
                    comma = true;
                }
                if (iValue - 1 >= 0) //Monday
                {
                    if (comma)
                        sDays = "," + sDays;
                    sDays = "MO" + sDays;
                    iValue -= 1;
                    comma = true;
                }
            }
            return sDays;
        }

        //-------------------------------------------------------------------------------
        public static string RecurrenceString(OM_Commitments comm)
        {
            string sRecurrence = "";

            if (comm.BoolIsRecurring)
            {
                sRecurrence = "RRULE:";
                string sFreq = "FREQ=";
                string sByDay = "";
                string sByMonthDay = "";
                string sEnd = "";
                string sInterval = "";
                int weekDays = comm.RecurrenceWeekdays.Value;

                switch (comm.EnumCommitmentRecurrenceType)
                {
                    case CommitmentRecurrenceType.Day:
                        sFreq += "DAILY;";
                        break;

                    case CommitmentRecurrenceType.Month:
                        sFreq += "MONTHLY;";
                        if (comm.BoolIsRecurrenceMonthlyDay)
                        {
                            sByMonthDay += "BYMONTHDAY=" + comm.CommitmentDate.Date.Day.ToString() + ";";
                        }
                        else if (comm.BoolIsRecurrenceMonthlyWeek)
                        {
                            sByDay += "BYDAY=" + comm.RecurrenceMonthlyWeeks.ToString() + RecurrenceWeekDays(weekDays) + ";";
                        }
                        break;

                    case CommitmentRecurrenceType.Week:
                        sFreq += "WEEKLY;";
                        sByDay += "BYDAY=" + RecurrenceWeekDays(weekDays) + ";";
                        break;

                    case CommitmentRecurrenceType.Year:
                        sFreq += "YEARLY;";
                        break;
                }
                switch (comm.EnumCommitmentRecurrenceEnd)
                {
                    case CommitmentRecurrenceEnd.Date:
                        sEnd += "UNTIL=" + comm.RecurrenceEndDate.ToString("yyyyMMddTHHmmssZ") + ";";
                        break;

                    case CommitmentRecurrenceEnd.Days:
                        sEnd += "COUNT=" + comm.RecurrenceOccurrences.ToString() + ";";
                        break;

                    case CommitmentRecurrenceEnd.Never:
                        sEnd += "";
                        break;
                }
                if (comm.RecurrenceEvery > 1)
                    sInterval += "INTERVAL=" + comm.RecurrenceEvery.ToString() + ";";

                sRecurrence += sFreq + sInterval + sByDay + sByMonthDay + sEnd;
            }

            return sRecurrence;
        }

        //-------------------------------------------------------------------------------
        public static CommitmentRecurrenceEnd GetRecurrenceEndType(IList<string> listRecurrence)
        {
            CommitmentRecurrenceEnd endType = CommitmentRecurrenceEnd.Never;

            string strRule = "RRULE:";
            string strRecurrence = "";
            foreach (string s in listRecurrence)
            {
                if (s.IndexOf(strRule) >= 0)
                    strRecurrence = s;
            }

            if (strRecurrence == "")
                return endType;

            string strRR = strRecurrence.ToUpper();
            if (strRR.IndexOf(strRule) >= 0)
            {
                string str = strRR.Right(strRR.Length - strRule.Length);
                string until = "UNTIL=";
                string count = "COUNT=";
                
                int pos = -1;
                pos = str.IndexOf(until);
                if (pos >= 0)
                    endType = CommitmentRecurrenceEnd.Date;

                pos = -1;
                pos = str.IndexOf(count);
                if (pos >= 0)
                    endType = CommitmentRecurrenceEnd.Days;
            }
            return endType;
        }

        //-------------------------------------------------------------------------------
        public static DateTime GetRecurrenceEndDate(IList<String> listRecurrence)
        {
            DateTime dt = new DateTime(1799, 12, 31);
            string strRule = "RRULE:";

            string strRecurrence = "";
            foreach (string s in listRecurrence)
            {
                if (s.IndexOf(strRule) >= 0)
                    strRecurrence = s;
            }

            if (strRecurrence == "")
                return dt;

            string strRR = strRecurrence.ToUpper();
            if (strRR.IndexOf(strRule) >= 0)
            {
                string str = strRR.Right(strRR.Length - strRule.Length);
                List<string> sList = str.Split(';').ToList();
                foreach (string s in sList)
                {
                    string until = "UNTIL=";
                    string date = "1799/12/31";
                    int pos = s.IndexOf(until);
                    if (pos >= 0)
                    {
                        date = s.Right(s.Length - until.Length);
                        dt = DateTime.ParseExact(date, "yyyyMMddTHHmmssZ", System.Globalization.CultureInfo.InvariantCulture);
                    }
                }

            }
            return dt;
        }

        //-------------------------------------------------------------------------------
        public static CommitmentRecurrenceType GetRecurrenceType(IList<string> listRecurrence)
        {
            CommitmentRecurrenceType rr = CommitmentRecurrenceType.Day;
            string strRule = "RRULE:";

            string strRecurrence = "";
            foreach (string s in listRecurrence)
            {
                if (s.IndexOf(strRule) >= 0)
                    strRecurrence = s;
            }

            if (strRecurrence == "")
                return rr;

            string strRR = strRecurrence.ToUpper();
            if (strRR.IndexOf(strRule) >= 0)
            {
                string str = strRR.Right(strRR.Length - strRule.Length);
                List<string> sList = str.Split(';').ToList();
                foreach (string s in sList)
                {
                    string freq = "FREQ=";
                    string freqType = "DAILY";
                    int pos = s.IndexOf(freq);
                    if (pos >= 0)
                    {
                        freqType = s.Right(s.Length - freq.Length);
                        if (freqType == "DAILY")
                            rr = CommitmentRecurrenceType.Day;

                        if (freqType == "MONTHLY")
                            rr = CommitmentRecurrenceType.Month;

                        if (freqType == "WEEKLY")
                            rr = CommitmentRecurrenceType.Week;
                        
                        if (freqType == "YEARLY")
                            rr = CommitmentRecurrenceType.Year;
                    }
                }

            }
            return rr;
        }

        //-------------------------------------------------------------------------------
        public static int GetRecurrenceEvery(IList<string> listRecurrence)
        {
            int iInterval = 1;
            string strRule = "RRULE:";
            string strRecurrence = "";
            foreach (string s in listRecurrence)
            {
                if (s.IndexOf(strRule) >= 0)
                    strRecurrence = s;
            }

            if (strRecurrence == "")
                return iInterval;
            
            string strRR = strRecurrence.ToUpper();
            if (strRR.IndexOf(strRule) >= 0)
            {
                string str = strRR.Right(strRR.Length - strRule.Length);
                List<string> sList = str.Split(';').ToList();
                foreach (string s in sList)
                {
                    string sInterval = "INTERVAL=";
                    string strval = "0";
                    int pos = s.IndexOf(sInterval);
                    if (pos >= 0)
                    {
                        strval = s.Right(s.Length - sInterval.Length);
                        iInterval = Convert.ToInt32(strval);
                    }
                }

            }
            return iInterval;
        }

        //-------------------------------------------------------------------------------
        public static int GetRecurrenceWeekday(IList<string> listRecurrence)
        {
            int iWeekday = 0;
            string strRule = "RRULE:";
            string strRecurrence = "";
            foreach (string s in listRecurrence)
            {
                if (s.IndexOf(strRule) >= 0)
                    strRecurrence = s;
            }

            if (strRecurrence == "")
                return iWeekday;

            string strRR = strRecurrence.ToUpper();
            if (strRR.IndexOf(strRule) >= 0)
            {
                string str = strRR.Right(strRR.Length - strRule.Length);
                List<string> sList = str.Split(';').ToList();
                foreach (string s in sList)
                {
                    string sByDay = "BYDAY=";
                    string strval = "";
                    int pos = s.IndexOf(sByDay);
                    if (pos >= 0)
                    {
                        strval = s.Right(s.Length - sByDay.Length);
                        if (strval.IndexOf("SU") >= 0) //Sunday
                            iWeekday += 64;
                        if (strval.IndexOf("SA") >= 0) //Saturday
                            iWeekday += 32;
                        if (strval.IndexOf("FR") >= 0) //Friday
                            iWeekday += 16;
                        if (strval.IndexOf("TH") >= 0) //Thursday
                            iWeekday += 8;
                        if (strval.IndexOf("WE") >= 0) //Wednesday
                            iWeekday += 4;
                        if (strval.IndexOf("TU") >= 0) //Tuesday
                            iWeekday += 2;
                        if (strval.IndexOf("MO") >= 0) //Monday
                            iWeekday += 1;
                    }
                }

            }
            return iWeekday;
        }

        //-------------------------------------------------------------------------------
        public static int GetRecurrenceMonthlyWeek(IList<string> listRecurrence)
        {
            int iMonthlyWeek = 0;
            try
            {
                string strRule = "RRULE:";
                string strRecurrence = "";
                foreach (string s in listRecurrence)
                {
                    if (s.IndexOf(strRule) >= 0)
                        strRecurrence = s;
                }

                if (strRecurrence == "")
                    return iMonthlyWeek;

                string strRR = strRecurrence.ToUpper();
                if (strRR.IndexOf(strRule) >= 0)
                {
                    string str = strRR.Right(strRR.Length - strRule.Length);
                    List<string> sList = str.Split(';').ToList();
                    foreach (string s in sList)
                    {
                        string sByDay = "BYDAY=";
                        string strval = "";
                        int pos = s.IndexOf(sByDay);
                        if (pos >= 0)
                        {
                            int comma = s.IndexOf(sByDay);
                            if (comma>=0)
                                return iMonthlyWeek; 

                            strval = s.Right(s.Length - sByDay.Length);
                            if (strval.IndexOf("SU") >= 0) //Saturday
                            {
                                string sMonthlyWeek = strval.Replace("SU", "");
                                if (sMonthlyWeek.Length > 0)
                                    iMonthlyWeek = Int32.Parse(sMonthlyWeek);
                            }
                            if (strval.IndexOf("SA") >= 0) //Saturday
                            {
                                string sMonthlyWeek = strval.Replace("SA", "");
                                if (sMonthlyWeek.Length > 0)
                                    iMonthlyWeek = Int32.Parse(sMonthlyWeek);
                            }
                            if (strval.IndexOf("FR") >= 0) //Friday
                            {
                                string sMonthlyWeek = strval.Replace("FR", "");
                                if (sMonthlyWeek.Length > 0)
                                    iMonthlyWeek = Int32.Parse(sMonthlyWeek);
                            }
                            if (strval.IndexOf("TH") >= 0) //Thursday
                            {
                                string sMonthlyWeek = strval.Replace("TH", "");
                                if (sMonthlyWeek.Length > 0)
                                    iMonthlyWeek = Int32.Parse(sMonthlyWeek);
                            }
                            if (strval.IndexOf("WE") >= 0) //Wednesday
                            {
                                string sMonthlyWeek = strval.Replace("WE", "");
                                if (sMonthlyWeek.Length > 0)
                                    iMonthlyWeek = Int32.Parse(sMonthlyWeek);
                            }
                            if (strval.IndexOf("TU") >= 0) //Tuesday
                            {
                                string sMonthlyWeek = strval.Replace("TU", "");
                                if (sMonthlyWeek.Length > 0)
                                    iMonthlyWeek = Int32.Parse(sMonthlyWeek);
                            }
                            if (strval.IndexOf("MO") >= 0) //Monday
                            {
                                string sMonthlyWeek = strval.Replace("MO", "");
                                if (sMonthlyWeek.Length > 0)
                                    iMonthlyWeek = Int32.Parse(sMonthlyWeek);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            return iMonthlyWeek;
        }

        //-------------------------------------------------------------------------------
        public static bool CheckRecurrenceException(TbSenderDatabaseInfo cmp, OM_GoogleCalendarsEvents gCalEv,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;
            OM_Commitments comm = GetCommitment(cmp, gCalEv.CommitmentId);
            if (comm == null)
                return ret;

            if (comm.RecurrenceId == 0)
                return ret;

            if (comm.BoolRecurrenceException)
                return ret;

            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        OM_Commitments commUpdate = GetCommitment(db, gCalEv.CommitmentId);

                        if (!commUpdate.BoolRecurrenceException)
                        {
                            commUpdate.BoolRecurrenceException = true;
                            commUpdate.BoolIsRecurring = false;
                        }

                        OM_GoogleEvents gEv = OM_GoogleEvents.SetExceptionGoogleEvent(db, gCalEv.IdEvent, true, excLogger);
                        
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

            return ret;
        }

        //-------------------------------------------------------------------------------
        public static bool CheckRecurrenceMaxCommitmentDate(TbSenderDatabaseInfo cmp, OM_WorkersCalendars wCal, int recurrenceId, DateTime maxDate,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;
            List<OM_Commitments> list = GetRecurrenceCommitments(cmp, wCal.WorkerId, wCal.SubId, recurrenceId);
            if (list == null)
                return ret;

            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        foreach (OM_Commitments c in list)
                        {
                            var query = from com in db.OM_Commitments
                                        where (com.CommitmentId == c.CommitmentId)
                                        select com;

                            foreach (OM_Commitments currCom in query)
                            {
                                currCom.MaxCommitmendDate = maxDate.Date;
                            }
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

            return ret;
        }


    }
}
