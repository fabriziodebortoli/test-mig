using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace RESTGate.OrganizerCore
{
    //================================================================================
    [Table(Name = "OM_Commitments")]
    public class OM_Commitments
    {
        int commitmentID;
        int commitmentKind;
        string personCode;
        char recursive;
        string clientCode;
        int officeFileID;
        int taskTypeID;
        double quantityToDo;
        double quantityDone;
        int reminderBefore;
        int reminderKindBefore;
        DateTime reminderTime;
        char isPrivate;
        int workerID;
        DateTime startDate;
        DateTime endDate;
        string description;
        string subject;
        string place;
        char closed;
        DateTime recurrenceEndDate;
        int recurrenceOccurrences;
        int recurrenceType;
        int recurrenceEvery;
        int recurrenceWeekDays;
        int priority;
        string origin;
        string calendar;

        [Column(IsPrimaryKey = true, Name = "CommitmentID")]
        public int CommitmentID { get { return this.commitmentID; } set { this.commitmentID = value; } }

        [Column(Name = "CommitmentKind")]
        public int CommitmentKind { get { return this.commitmentKind; } set { this.commitmentKind = value; } }

        [Column(Name = "PersonCode")]
        public string PersonCode { get { return this.personCode; } set { this.personCode = value; } }

        [Column(Name = "Recursive")]
        public char Recursive { get { return this.recursive; } set { this.recursive = value; } }

        public bool IsRecurrence { get { return this.recursive == '1'; } set { this.recursive = value ? '1' : '0'; } }

        [Column(Name = "ClientCode")]
        public string ClientCode { get { return this.clientCode; } set { this.clientCode = value; } }

        [Column(Name = "OfficeFileID")]
        public int OfficeFileID { get { return this.officeFileID; } set { this.officeFileID = value; } }

        [Column(Name = "TaskTypeID")]
        public int TaskTypeID { get { return this.taskTypeID; } set { this.taskTypeID = value; } }

        public TaskKind TaskKind { get { return (TaskKind)this.taskTypeID; } set { this.taskTypeID = (int)value;  } }

        [Column(Name = "QuantityToDo")]
        public double QuantityToDo { get { return this.quantityToDo; } set { this.quantityToDo = value; } }

        [Column(Name = "QuantityDone")]
        public double QuantityDone { get { return this.quantityDone; } set { this.quantityDone = value; } }

        [Column(Name = "ReminderBefore")]
        public int ReminderBefore { get { return this.reminderBefore; } set { this.reminderBefore = value; } }

        [Column(Name = "ReminderKindBefore")]
        public int ReminderKindBefore { get { return this.reminderKindBefore; } set { this.reminderKindBefore = value; } }

        [Column(Name = "ReminderTime")]
        public DateTime ReminderTime { get { return this.reminderTime; } set { this.reminderTime = value; } }

        [Column(Name = "Private")]
        public char IsPrivate { get { return this.isPrivate; } set { this.isPrivate = value; } }

        [Column(Name = "WorkerID")]
        public int WorkerID { get { return this.workerID; } set { this.workerID = value; } }

        [Column(Name = "StartDate")]
        public DateTime StartDate { get { return this.startDate; } set { this.startDate = value; } }

        [Column(Name = "EndDate")]
        public DateTime EndDate { get { return this.endDate; } set { this.endDate = value; } }

        [Column(Name = "Description")]
        public string Description { get { return this.description; } set { this.description = value; } }

        [Column(Name = "Subject")]
        public string Subject { get { return this.subject; } set { this.subject = value; } }

        [Column(Name = "Place")]
        public string Place { get { return this.place; } set { this.place = value; } }

        [Column(Name = "Closed")]
        public char Closed { get { return this.closed; } set { this.closed = value; } }

        [Column(Name = "RecurrenceEndDate")]
        public DateTime RecurrenceEndDate { get { return this.recurrenceEndDate; } set { this.recurrenceEndDate = value; } }

        [Column(Name = "RecurrenceOccurrences")]
        public int RecurrenceOccurrences { get { return this.recurrenceOccurrences; } set { this.recurrenceOccurrences = value; } }

        [Column(Name = "RecurrenceType")]
        public int RecurrenceType { get { return this.recurrenceType; } set { this.recurrenceType = value; } }

        [Column(Name = "RecurrenceEvery")]
        public int RecurrenceEvery { get { return this.recurrenceEvery; } set { this.recurrenceEvery = value; } }

        [Column(Name = "RecurrenceWeekdays")]
        public int RecurrenceWeekDays { get { return this.recurrenceWeekDays; } set { this.recurrenceWeekDays = value; } }

        [Column(Name = "Priority")]
        public int Priority { get { return this.priority; } set { this.priority = value; } }

        [Column(Name = "Origin")]
        public string Origin { get { return this.origin; } set { this.origin = value; } }

        [Column(Name = "Calendar")]
        public string Calendar { get { return this.calendar; } set { this.calendar = value; } }
    }
}