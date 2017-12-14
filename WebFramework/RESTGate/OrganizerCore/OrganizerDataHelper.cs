using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Web;

namespace RESTGate.OrganizerCore
{
    //================================================================================
    public class OrganizerDataHelper
    {
        //--------------------------------------------------------------------------------
        public static void SaveEntity<T>(T entity, string connectionString) where T : OM_Commitments
        {
            DataContext db = new DataContext(connectionString);
            db.GetTable<T>().InsertOnSubmit(entity);
            db.SubmitChanges();
        }

        //--------------------------------------------------------------------------------
        public static List<OM_Workers> ReadWorkers(string connectionString)
        {
            DataContext db = new DataContext(connectionString);
            Table<OM_Workers> workers = db.GetTable<OM_Workers>();

            return workers.ToList();
        }

        //--------------------------------------------------------------------------------
        public static List<OM_Commitments> ReadCommitments(string connectionString)
        {
            DataContext db = new DataContext(connectionString);
            Table<OM_Commitments> commitments = db.GetTable<OM_Commitments>();

            if (commitments == null || commitments.Count() == 0)
                return new List<OM_Commitments>();

            List<OM_Commitments> listCommitments = new List<OM_Commitments>();

            if (commitments.Count() == 0)
            {
                return listCommitments;
            }

            OM_Commitments aCommitment;

            foreach (OM_Commitments commitment in commitments)
            { 
                aCommitment = new OM_Commitments();
                aCommitment.CommitmentID = commitment.CommitmentID;
                aCommitment.Calendar = commitment.Calendar;
                aCommitment.ClientCode = commitment.ClientCode;
                aCommitment.Closed = commitment.Closed;
                aCommitment.CommitmentKind = commitment.CommitmentKind;
                aCommitment.Description = commitment.Description;
                aCommitment.EndDate = commitment.EndDate;
                aCommitment.IsPrivate = commitment.IsPrivate;
                aCommitment.IsRecurrence = commitment.IsRecurrence;
                aCommitment.OfficeFileID = commitment.OfficeFileID;
                aCommitment.Origin = commitment.Origin;
                aCommitment.PersonCode = commitment.PersonCode;
                aCommitment.Place = commitment.Place;
                aCommitment.Priority = commitment.Priority;
                aCommitment.QuantityDone = commitment.QuantityDone;
                aCommitment.QuantityToDo = commitment.QuantityToDo;
                aCommitment.RecurrenceEndDate = commitment.RecurrenceEndDate;
                aCommitment.RecurrenceEvery = commitment.RecurrenceEvery;
                aCommitment.RecurrenceOccurrences = commitment.RecurrenceOccurrences;
                aCommitment.RecurrenceType = commitment.RecurrenceType;
                aCommitment.RecurrenceWeekDays = commitment.RecurrenceWeekDays;
                aCommitment.Recursive = commitment.Recursive;
                aCommitment.ReminderBefore = commitment.ReminderBefore;
                aCommitment.ReminderKindBefore = commitment.ReminderKindBefore;
                aCommitment.ReminderTime = commitment.ReminderTime;
                aCommitment.StartDate = commitment.StartDate;
                aCommitment.Subject = commitment.Subject;
                aCommitment.TaskKind = commitment.TaskKind;
                aCommitment.TaskTypeID = commitment.TaskTypeID;
                aCommitment.WorkerID = commitment.WorkerID;

                listCommitments.Add(aCommitment);
            }

            return listCommitments;
        }
    }

}