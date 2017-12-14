using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Microarea.Library.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects
{
    [Flags]
    public enum FrequencyTypeEnum
    {
        Undefined = 0x00000000,
        OnDemand = 0x00000001,
        Once = 0x00000002,
        RecurringDaily = 0x00000004,
        RecurringWeekly = 0x00000008,
        RecurringMonthly1 = 0x00000010,
        RecurringMonthly2 = 0x00000020,
        Cyclic = 0x00010000,
        Temporary = 0x00020000,
        CyclicRepeatRunning = 0x00040000,
        FrequencyTypeFlagsMask = 0x0000FFFF,
        FrequencyTypeAttributesMask = 0x00FF0000
    };

    [Flags]
    public enum FrequencyRecurringIntervalTypeEnum
    {
        Undefined = 0x00000000,
        WeeklySunday = 0x00000001,
        WeeklyMonday = 0x00000002,
        WeeklyTuesday = 0x00000004,
        WeeklyWednesday = 0x00000008,
        WeeklyThursday = 0x00000010,
        WeeklyFriday = 0x00000020,
        WeeklySaturday = 0x00000040,
        Monthly2Sunday = 0x00000001,
        Monthly2Monday = 0x00000002,
        Monthly2Tuesday = 0x00000003,
        Monthly2Wednesday = 0x00000004,
        Monthly2Thursday = 0x00000005,
        Monthly2Friday = 0x00000006,
        Monthly2Saturday = 0x00000007,
        Monthly2Day = 0x00000008,
        Monthly2DayOfWeek = 0x00000009,
        Monthly2WeekendDay = 0x0000000A
    };

    [Flags]
    public enum FrequencyRelativeIntervalTypeEnum
    {
        Undefined = 0x00000000,
        Monthly2First = 0x00000001,
        Monthly2Second = 0x00000002,
        Monthly2Third = 0x00000004,
        Monthly2Fourth = 0x00000008,
        Monthly2Last = 0x00000010
    };

    [Flags]
    public enum RunningOptionsEnum
    {
        Undefined = 0x00000000,
        RunIconized = 0x00000001,
        CloseOnEnd = 0x00000002,
        PrintReport = 0x00000004,
        WaitForProcessHasExited = 0x00000008,
        ValidateData = 0x00000010,
        SetApplicationDateBeforeRun = 0x00000020,
        ImpersonateWindowsUser = 0x00000040,
        UseInternetExplorer = 0x00000080,
        CreateNewBrowserInstance = 0x00000100,
        SendReportAsRDEWTEMailAttachment = 0x00000200,
        SendReportAsPDFWTEMailAttachment = 0x00000400,
        SendReportAsCompressedWTEMailAttachment = 0x00000800,
        SendReportAsExcelWTEMailAttachment = 0x00001000,
        SaveReportAsRDEFile = 0x00002000,
        SaveReportAsPDFFile = 0x00004000,
        SaveReportAsExcelFile = 0x00008000,
        ConcatReportPDFFiles = 0x00010000,
        ForceRestoreOverExistingDB = 0x00020000, // elemento riutilizzabile perchè non più usato
        OverwriteCompanyDBBackup = 0x00040000,
        VerifyCompanyDBBackup = 0x00080000,
    };

    [Flags]
    public enum FrequencySubtypeEnum
    {
        Undefined = 0x00000000,
        DailyOnce = 0x00000001,
        MinutesInterval = 0x00000002,
        HoursInterval = 0x00000004
    };
    #region TaskRunAsyncState
    //=================================================================================
    public class TaskRunAsyncState
    {
        private WTEScheduledTaskObj task = null;
        private TbLoaderClientInterface tbAppClientInterface = null;
        private string connectionString = String.Empty;
        private bool closeDocumentOnEnd = false;
        private bool runIconized = false;
        private ManualResetEvent executionEndedEvent = null;
        private EventLog eventLog = null;
        private WoormInfo woormInfo = null;

        //-------------------------------------------------------------------------------------------------
        public TaskRunAsyncState(WTEScheduledTaskObj aTask, TbLoaderClientInterface aTbAppClientInterface, string aConnectionString, bool aCloseDocumentOnEndFlag, bool aRunIconized, ManualResetEvent aExecutionEndedEvent, EventLog aEventLog)
        {
            task = aTask;
            tbAppClientInterface = aTbAppClientInterface;
            connectionString = aConnectionString;
            closeDocumentOnEnd = aCloseDocumentOnEndFlag;
            runIconized = aRunIconized;
            executionEndedEvent = aExecutionEndedEvent;
            eventLog = aEventLog;
        }

        //-------------------------------------------------------------------------------------------------
        public TaskRunAsyncState(WTEScheduledTaskObj aTask, TbLoaderClientInterface aTbAppClientInterface, string aConnectionString, bool aCloseDocumentOnEndFlag, bool aRunIconized, ManualResetEvent aExecutionEndedEvent, EventLog aEventLog, WoormInfo aWoormInfo)
            : this(aTask, aTbAppClientInterface, aConnectionString, aCloseDocumentOnEndFlag, aRunIconized, aExecutionEndedEvent, aEventLog)
        {
            woormInfo = aWoormInfo;
        }

        //-----------------------------------------------------------------------------------
        public void WriteLogEntry(string aMessage, EventLogEntryType type)
        {
            if (eventLog == null || aMessage == null || aMessage.Length == 0)
                return;

            eventLog.WriteEntry(aMessage, type);
        }

        //-------------------------------------------------------------------------------------------------
        public WTEScheduledTaskObj Task { get { return task; } }
        public TbLoaderClientInterface TbApplicationClientInterface { get { return tbAppClientInterface; } }
        public string ConnectionString { get { return connectionString; } }
        public bool CloseDocumentOnEnd { get { return closeDocumentOnEnd; } }
        public bool RunIconized { get { return runIconized; } }
        public ManualResetEvent ExecutionEndedEvent { get { return executionEndedEvent; } }
        public EventLog EventLog { get { return eventLog; } }
        public WoormInfo WoormInfo { get { return woormInfo; } }
    }
    #endregion
  
    //====================================================================================
    public class WTEScheduledTaskObj
	{
		
		public const int RetryAttemptsMaxNumber = 100;
		public const int RetryDelayMaximum = 60;
	
		public const int	TaskCodeUniquePrefixLength = 7;
		private const int	taskCodeMaximumLength = 10;
		private const char	temporaryCodeChar = '$';
		private const int	cyclicRepeatMax		=	99;
		private const int	impersonationPasswordMaximumLength = 255;
	
		private Guid								id = Guid.Empty;
		private string								code = String.Empty;
		private int									companyId = 0;
		private int									loginId = 0;
		private TaskTypeEnum						type = TaskTypeEnum.Undefined;
		private RunningOptionsEnum					runningOptions;
		private	string								command = String.Empty;
		private	string								description = String.Empty;
		private	bool								enabled = false;
		private FrequencyTypeEnum					frequencyType = FrequencyTypeEnum.Undefined;
		private FrequencySubtypeEnum				frequencySubtype = FrequencySubtypeEnum.Undefined;
		private int									frequencyInterval = 0;
		private int									frequencySubinterval = 0;
		private FrequencyRelativeIntervalTypeEnum	frequencyRelativeInterval = FrequencyRelativeIntervalTypeEnum.Undefined;
		private int									frequencyRecurringFactor = 0;
		private DateTime							activeStartDate;
		private DateTime							activeEndDate;
		private DateTime							lastRunDate;
		private DateTime							lastRunRetries;
		private DateTime							nextRunDate;
		private int									retryAttempts = 0;
		private int									retryDelay = 0;
		private int									retryAttemptsActualCount = 0;
		private CompletitionLevelEnum				lastRunCompletitionLevel = CompletitionLevelEnum.Undefined;
		private bool								sendMailUsingSMTP = false;
		private int									cyclicRepeat = 0;	
		private int									cyclicDelay = 0;	
		private string								cyclicTaskCode = String.Empty;
		private string								impersonationDomain = String.Empty;
		private string								impersonationUser = String.Empty;
		private string								impersonationPassword = String.Empty;
		private string								messageContent = String.Empty;
		private IntPtr								taskWindowHandle = IntPtr.Zero;
                
        public WTETaskNotificationRecipientsCollection notificationRecipients;
		public WTETasksInScheduledSequenceCollection	 tasksInSequence = null;

		private string xmlParameters = String.Empty;

        #region WTEScheduledTaskObj public properties

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SaveReportAsPDFFile
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.SaveReportAsPDFFile) == RunningOptionsEnum.SaveReportAsPDFFile); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.SaveReportAsPDFFile;
                else
                    runningOptions &= ~RunningOptionsEnum.SaveReportAsPDFFile;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SaveReportAsExcelFile
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.SaveReportAsExcelFile) == RunningOptionsEnum.SaveReportAsExcelFile); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.SaveReportAsExcelFile;
                else
                    runningOptions &= ~RunningOptionsEnum.SaveReportAsExcelFile;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SaveReportAsFile
        {
            get { return SaveReportAsRDEFile || SaveReportAsPDFFile || SaveReportAsExcelFile; }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsRDEMailAttachment
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.SendReportAsRDEWTEMailAttachment) == RunningOptionsEnum.SendReportAsRDEWTEMailAttachment); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.SendReportAsRDEWTEMailAttachment;
                else
                    runningOptions &= ~RunningOptionsEnum.SendReportAsRDEWTEMailAttachment;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsPDFMailAttachment
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.SendReportAsPDFWTEMailAttachment) == RunningOptionsEnum.SendReportAsPDFWTEMailAttachment); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.SendReportAsPDFWTEMailAttachment;
                else
                    runningOptions &= ~RunningOptionsEnum.SendReportAsPDFWTEMailAttachment;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsExcelMailAttachment
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.SendReportAsExcelWTEMailAttachment) == RunningOptionsEnum.SendReportAsExcelWTEMailAttachment); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.SendReportAsExcelWTEMailAttachment;
                else
                    runningOptions &= ~RunningOptionsEnum.SendReportAsExcelWTEMailAttachment;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsMailAttachment
        {
            get { return SendReportAsRDEMailAttachment || SendReportAsPDFMailAttachment || SendReportAsExcelMailAttachment; }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsCompressedMailAttachment
        {
            get { return SendReportAsMailAttachment && ((runningOptions & RunningOptionsEnum.SendReportAsCompressedWTEMailAttachment) == RunningOptionsEnum.SendReportAsCompressedWTEMailAttachment); }
            set
            {
                if (SendReportAsMailAttachment && value)
                    runningOptions |= RunningOptionsEnum.SendReportAsCompressedWTEMailAttachment;
                else
                    runningOptions &= ~RunningOptionsEnum.SendReportAsCompressedWTEMailAttachment;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SaveReportAsRDEFile
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.SaveReportAsRDEFile) == RunningOptionsEnum.SaveReportAsRDEFile); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.SaveReportAsRDEFile;
                else
                    runningOptions &= ~RunningOptionsEnum.SaveReportAsRDEFile;
            }
        }



        //--------------------------------------------------------------------------------------------------------------------------------
        public Guid Id { get { return id; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string Code
        {
            get { return code; }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int CompanyId { get { return companyId; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int LoginId { get { return loginId; } }


        //--------------------------------------------------------------------------------------------------------------------------------
        public int Type { get { return (int)type; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int RunningOptions { get { return (int)runningOptions; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SetApplicationDateBeforeRun
        {
            get
            {
                if (!RunBatch && !RunReport && !RunFunction && !RunDataExport && !RunDataImport)
                    return false;

                return ((runningOptions & RunningOptionsEnum.SetApplicationDateBeforeRun) == RunningOptionsEnum.SetApplicationDateBeforeRun);
            }
            set
            {
                if (value && (RunBatch || RunReport || RunFunction || RunDataExport || RunDataImport))
                    runningOptions |= RunningOptionsEnum.SetApplicationDateBeforeRun;
                else
                    runningOptions &= ~RunningOptionsEnum.SetApplicationDateBeforeRun;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool IsToImpersonateWindowsUser
        {
            get { return (!ToRunOnDemand && (runningOptions & RunningOptionsEnum.ImpersonateWindowsUser) == RunningOptionsEnum.ImpersonateWindowsUser); }
            set
            {
                if (value)
                    runningOptions |= RunningOptionsEnum.ImpersonateWindowsUser;
                else
                    runningOptions &= ~RunningOptionsEnum.ImpersonateWindowsUser;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool RunIconized
        {
            get { return ((runningOptions & RunningOptionsEnum.RunIconized) == RunningOptionsEnum.RunIconized); }
            set
            {
                if (value)
                    runningOptions |= RunningOptionsEnum.RunIconized;
                else
                    runningOptions &= ~RunningOptionsEnum.RunIconized;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool CloseOnEnd
        {
            get { return (RunBatch || RunReport) && ((runningOptions & RunningOptionsEnum.CloseOnEnd) == RunningOptionsEnum.CloseOnEnd); }
            set
            {
                if ((RunBatch || RunReport) && value)
                    runningOptions |= RunningOptionsEnum.CloseOnEnd;
                else
                    runningOptions &= ~RunningOptionsEnum.CloseOnEnd;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool PrintReport
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.PrintReport) == RunningOptionsEnum.PrintReport); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.PrintReport;
                else
                    runningOptions &= ~RunningOptionsEnum.PrintReport;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsRDEWTEMailAttachment
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.SendReportAsRDEWTEMailAttachment) == RunningOptionsEnum.SendReportAsRDEWTEMailAttachment); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.SendReportAsRDEWTEMailAttachment;
                else
                    runningOptions &= ~RunningOptionsEnum.SendReportAsRDEWTEMailAttachment;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsPDFWTEMailAttachment
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.SendReportAsPDFWTEMailAttachment) == RunningOptionsEnum.SendReportAsPDFWTEMailAttachment); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.SendReportAsPDFWTEMailAttachment;
                else
                    runningOptions &= ~RunningOptionsEnum.SendReportAsPDFWTEMailAttachment;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsExcelWTEMailAttachment
        {
            get { return RunReport && ((runningOptions & RunningOptionsEnum.SendReportAsExcelWTEMailAttachment) == RunningOptionsEnum.SendReportAsExcelWTEMailAttachment); }
            set
            {
                if (RunReport && value)
                    runningOptions |= RunningOptionsEnum.SendReportAsExcelWTEMailAttachment;
                else
                    runningOptions &= ~RunningOptionsEnum.SendReportAsExcelWTEMailAttachment;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsWTEMailAttachment
        {
            get { return SendReportAsRDEWTEMailAttachment || SendReportAsPDFWTEMailAttachment || SendReportAsExcelWTEMailAttachment; }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendReportAsCompressedWTEMailAttachment
        {
            get { return SendReportAsWTEMailAttachment && ((runningOptions & RunningOptionsEnum.SendReportAsCompressedWTEMailAttachment) == RunningOptionsEnum.SendReportAsCompressedWTEMailAttachment); }
            set
            {
                if (SendReportAsWTEMailAttachment && value)
                    runningOptions |= RunningOptionsEnum.SendReportAsCompressedWTEMailAttachment;
                else
                    runningOptions &= ~RunningOptionsEnum.SendReportAsCompressedWTEMailAttachment;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool ConcatReportPDFFiles
        {
            get { return (SendReportAsPDFWTEMailAttachment || SaveReportAsPDFFile) && ((runningOptions & RunningOptionsEnum.ConcatReportPDFFiles) == RunningOptionsEnum.ConcatReportPDFFiles); }
            set
            {
                if ((SendReportAsPDFWTEMailAttachment || SaveReportAsPDFFile) && value)
                    runningOptions |= RunningOptionsEnum.ConcatReportPDFFiles;
                else
                    runningOptions &= ~RunningOptionsEnum.ConcatReportPDFFiles;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool OverwriteCompanyDBBackup
        {
            get { return BackupCompanyDB && ((runningOptions & RunningOptionsEnum.OverwriteCompanyDBBackup) == RunningOptionsEnum.OverwriteCompanyDBBackup); }
            set
            {
                if (BackupCompanyDB && value)
                    runningOptions |= RunningOptionsEnum.OverwriteCompanyDBBackup;
                else
                    runningOptions &= ~RunningOptionsEnum.OverwriteCompanyDBBackup;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool VerifyCompanyDBBackup
        {
            get { return BackupCompanyDB && ((runningOptions & RunningOptionsEnum.VerifyCompanyDBBackup) == RunningOptionsEnum.VerifyCompanyDBBackup); }
            set
            {
                if (BackupCompanyDB && value)
                    runningOptions |= RunningOptionsEnum.VerifyCompanyDBBackup;
                else
                    runningOptions &= ~RunningOptionsEnum.VerifyCompanyDBBackup;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool WaitForProcessHasExited
        {
            get { return RunExecutable && ((runningOptions & RunningOptionsEnum.WaitForProcessHasExited) == RunningOptionsEnum.WaitForProcessHasExited); }
            set
            {
                if (RunExecutable && value)
                    runningOptions |= RunningOptionsEnum.WaitForProcessHasExited;
                else
                    runningOptions &= ~RunningOptionsEnum.WaitForProcessHasExited;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool ValidateData
        {
            get { return (RunDataImport && (runningOptions & RunningOptionsEnum.ValidateData) == RunningOptionsEnum.ValidateData); }
            set
            {
                if (RunDataImport && value)
                    runningOptions |= RunningOptionsEnum.ValidateData;
                else
                    runningOptions &= ~RunningOptionsEnum.ValidateData;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool OpenWebPageInInternetExplorer
        {
            get
            {
                if (!OpenWebPage)
                    return false;

                return ((runningOptions & RunningOptionsEnum.UseInternetExplorer) == RunningOptionsEnum.UseInternetExplorer);
            }
            set
            {
                if (value && OpenWebPage)
                    runningOptions |= RunningOptionsEnum.UseInternetExplorer;
                else
                    runningOptions &= ~RunningOptionsEnum.UseInternetExplorer;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool CreateNewBrowserInstance
        {
            get
            {
                if (!OpenWebPage)
                    return false;

                return ((runningOptions & RunningOptionsEnum.CreateNewBrowserInstance) == RunningOptionsEnum.CreateNewBrowserInstance);
            }
            set
            {
                if (value && OpenWebPage)
                    runningOptions |= RunningOptionsEnum.CreateNewBrowserInstance;
                else
                    runningOptions &= ~RunningOptionsEnum.CreateNewBrowserInstance;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string Command
        {
            get { return command; }
            set
            {
                //@@TODO  aggiungere i controlli su tutti i possibili tipi di comandi schedulabili

                if (!OpenWebPage || IsValidURI(value)) command = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string Description { get { return description; } set { description = value; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool IsRunning { get { return lastRunCompletitionLevel == CompletitionLevelEnum.Running; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool UndefinedType
        {
            get { return (type == TaskTypeEnum.Undefined); }
            set { type = TaskTypeEnum.Undefined; }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool RunBatch
        {
            get { return (type == TaskTypeEnum.Batch); }
            set
            {
                if (value)
                    type = TaskTypeEnum.Batch;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool RunReport
        {
            get { return (type == TaskTypeEnum.Report); }
            set
            {
                if (value)
                    type = TaskTypeEnum.Report;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool RunExecutable
        {
            get { return (type == TaskTypeEnum.Executable); }
            set
            {
                if (value)
                    type = TaskTypeEnum.Executable;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool RunFunction
        {
            get { return (type == TaskTypeEnum.Function); }
            set
            {
                if (value)
                    type = TaskTypeEnum.Function;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool RunDataExport
        {
            get { return (type == TaskTypeEnum.DataExport); }
            set
            {
                if (value)
                    type = TaskTypeEnum.DataExport;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool RunDataImport
        {
            get { return (type == TaskTypeEnum.DataImport); }
            set
            {
                if (value)
                    type = TaskTypeEnum.DataImport;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendMessage
        {
            get { return (type == TaskTypeEnum.Message); }
            set
            {
                if (value)
                    type = TaskTypeEnum.Message;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendMail
        {
            get { return (type == TaskTypeEnum.Mail); }
            set
            {
                if (value)
                    type = TaskTypeEnum.Mail;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool OpenWebPage
        {
            get { return (type == TaskTypeEnum.WebPage); }
            set
            {
                if (value)
                    type = TaskTypeEnum.WebPage;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool DeleteRunnedReports
        {
            get { return (type == TaskTypeEnum.DelRunnedReports); }
            set
            {
                if (value)
                    type = TaskTypeEnum.DelRunnedReports;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool BackupCompanyDB
        {
            get { return (type == TaskTypeEnum.BackupCompanyDB); }
            set
            {
                if (value)
                    type = TaskTypeEnum.BackupCompanyDB;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool RestoreCompanyDB
        {
            get { return (type == TaskTypeEnum.RestoreCompanyDB); }
            set
            {
                if (value)
                    type = TaskTypeEnum.RestoreCompanyDB;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool IsSequence
        {
            get { return (type == TaskTypeEnum.Sequence); }
            set
            {
                if (value)
                    type = TaskTypeEnum.Sequence;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool TBLoaderConnectionNecessaryForRun
        {
            get
            {
                if (!IsSequence)
                    return (RunBatch || RunReport || RunFunction || RunDataExport || RunDataImport);

                if (tasksInSequence != null && tasksInSequence.Count > 0)
                {
                    foreach (WTETaskInScheduledSequence taskInSequence in tasksInSequence)
                    {
                        if (taskInSequence.TBLoaderConnectionNecessaryForRun)
                            return true;
                    }
                }
                return false;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool Enabled { get { return enabled; } set { enabled = value; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public int FrequencyType { get { return (int)frequencyType; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public int FrequencySubtype { get { return (int)frequencySubtype; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public int FrequencyRelativeInterval { get { return (int)frequencyRelativeInterval; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool FrequencyTypeUndefined
        {
            get { return (frequencyType == FrequencyTypeEnum.Undefined); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool ToRunOnDemand
        {
            get { return ((frequencyType & FrequencyTypeEnum.OnDemand) == FrequencyTypeEnum.OnDemand); }
            set
            {
                if (value)
                    frequencyType = FrequencyTypeEnum.OnDemand | (frequencyType & FrequencyTypeEnum.FrequencyTypeAttributesMask);
                else
                    frequencyType &= ~FrequencyTypeEnum.OnDemand;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool ToRunOnce
        {
            get { return ((frequencyType & FrequencyTypeEnum.Once) == FrequencyTypeEnum.Once); }
            set
            {
                if (value)
                    frequencyType = FrequencyTypeEnum.Once | (frequencyType & FrequencyTypeEnum.FrequencyTypeAttributesMask);
                else
                    frequencyType &= ~FrequencyTypeEnum.Once;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool Recurring
        {
            get
            {
                return ((frequencyType & (FrequencyTypeEnum.RecurringDaily | FrequencyTypeEnum.RecurringWeekly | FrequencyTypeEnum.RecurringMonthly1 | FrequencyTypeEnum.RecurringMonthly2)) != FrequencyTypeEnum.Undefined);
            }
            set
            {
                if (value && Recurring)
                    return;
                if (!value && !Recurring)
                    return;

                if (value)
                    frequencyType = FrequencyTypeEnum.RecurringWeekly | (frequencyType & FrequencyTypeEnum.FrequencyTypeAttributesMask);
                else
                    frequencyType &= ~FrequencyTypeEnum.RecurringWeekly;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool DailyRecurring
        {
            get { return ((frequencyType & FrequencyTypeEnum.RecurringDaily) == FrequencyTypeEnum.RecurringDaily); }
            set
            {
                if (value)
                    frequencyType = FrequencyTypeEnum.RecurringDaily | (frequencyType & FrequencyTypeEnum.FrequencyTypeAttributesMask);
                else
                    frequencyType &= ~FrequencyTypeEnum.RecurringDaily;

                frequencyRecurringFactor = value ? 0 : 1;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool WeeklyRecurring
        {
            get { return ((frequencyType & FrequencyTypeEnum.RecurringWeekly) == FrequencyTypeEnum.RecurringWeekly); }
            set
            {
                if (value)
                    frequencyType = FrequencyTypeEnum.RecurringWeekly | (frequencyType & FrequencyTypeEnum.FrequencyTypeAttributesMask);
                else
                    frequencyType &= ~FrequencyTypeEnum.RecurringWeekly;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool WeeklyRecurringOnSunday
        {
            get
            {
                if (!WeeklyRecurring)
                    return false;
                return ((frequencyInterval & (int)FrequencyRecurringIntervalTypeEnum.WeeklySunday) == (int)FrequencyRecurringIntervalTypeEnum.WeeklySunday);
            }
            set
            {
                if (!WeeklyRecurring)
                    return;
                if (value)
                    frequencyInterval |= (int)FrequencyRecurringIntervalTypeEnum.WeeklySunday;
                else
                    frequencyInterval &= ~(int)FrequencyRecurringIntervalTypeEnum.WeeklySunday;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool WeeklyRecurringOnMonday
        {
            get
            {
                if (!WeeklyRecurring)
                    return false;
                return ((frequencyInterval & (int)FrequencyRecurringIntervalTypeEnum.WeeklyMonday) == (int)FrequencyRecurringIntervalTypeEnum.WeeklyMonday);
            }
            set
            {
                if (!WeeklyRecurring)
                    return;
                if (value)
                    frequencyInterval |= (int)FrequencyRecurringIntervalTypeEnum.WeeklyMonday;
                else
                    frequencyInterval &= ~(int)FrequencyRecurringIntervalTypeEnum.WeeklyMonday;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool WeeklyRecurringOnTuesday
        {
            get
            {
                if (!WeeklyRecurring)
                    return false;
                return ((frequencyInterval & (int)FrequencyRecurringIntervalTypeEnum.WeeklyTuesday) == (int)FrequencyRecurringIntervalTypeEnum.WeeklyTuesday);
            }
            set
            {
                if (!WeeklyRecurring)
                    return;
                if (value)
                    frequencyInterval |= (int)FrequencyRecurringIntervalTypeEnum.WeeklyTuesday;
                else
                    frequencyInterval &= ~(int)FrequencyRecurringIntervalTypeEnum.WeeklyTuesday;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool WeeklyRecurringOnWednesday
        {
            get
            {
                if (!WeeklyRecurring)
                    return false;
                return ((frequencyInterval & (int)FrequencyRecurringIntervalTypeEnum.WeeklyWednesday) == (int)FrequencyRecurringIntervalTypeEnum.WeeklyWednesday);
            }
            set
            {
                if (!WeeklyRecurring)
                    return;
                if (value)
                    frequencyInterval |= (int)FrequencyRecurringIntervalTypeEnum.WeeklyWednesday;
                else
                    frequencyInterval &= ~(int)FrequencyRecurringIntervalTypeEnum.WeeklyWednesday;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool WeeklyRecurringOnThursday
        {
            get
            {
                if (!WeeklyRecurring)
                    return false;
                return ((frequencyInterval & (int)FrequencyRecurringIntervalTypeEnum.WeeklyThursday) == (int)FrequencyRecurringIntervalTypeEnum.WeeklyThursday);
            }
            set
            {
                if (!WeeklyRecurring)
                    return;
                if (value)
                    frequencyInterval |= (int)FrequencyRecurringIntervalTypeEnum.WeeklyThursday;
                else
                    frequencyInterval &= ~(int)FrequencyRecurringIntervalTypeEnum.WeeklyThursday;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool WeeklyRecurringOnFriday
        {
            get
            {
                if (!WeeklyRecurring)
                    return false;
                return ((frequencyInterval & (int)FrequencyRecurringIntervalTypeEnum.WeeklyFriday) == (int)FrequencyRecurringIntervalTypeEnum.WeeklyFriday);
            }
            set
            {
                if (!WeeklyRecurring)
                    return;
                if (value)
                    frequencyInterval |= (int)FrequencyRecurringIntervalTypeEnum.WeeklyFriday;
                else
                    frequencyInterval &= ~(int)FrequencyRecurringIntervalTypeEnum.WeeklyFriday;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool WeeklyRecurringOnSaturday
        {
            get
            {
                if (!WeeklyRecurring)
                    return false;
                return ((frequencyInterval & (int)FrequencyRecurringIntervalTypeEnum.WeeklySaturday) == (int)FrequencyRecurringIntervalTypeEnum.WeeklySaturday);
            }
            set
            {
                if (!WeeklyRecurring)
                    return;
                if (value)
                    frequencyInterval |= (int)FrequencyRecurringIntervalTypeEnum.WeeklySaturday;
                else
                    frequencyInterval &= ~(int)FrequencyRecurringIntervalTypeEnum.WeeklySaturday;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool MonthlyRecurring { get { return ((frequencyType & (FrequencyTypeEnum.RecurringMonthly1 | FrequencyTypeEnum.RecurringMonthly2)) != FrequencyTypeEnum.Undefined); } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool Monthly1Recurring
        {
            get { return ((frequencyType & FrequencyTypeEnum.RecurringMonthly1) == FrequencyTypeEnum.RecurringMonthly1); }
            set
            {
                if (value)
                    frequencyType = FrequencyTypeEnum.RecurringMonthly1 | (frequencyType & FrequencyTypeEnum.FrequencyTypeAttributesMask);
                else
                    frequencyType &= ~FrequencyTypeEnum.RecurringMonthly1;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool Monthly2Recurring
        {
            get { return ((frequencyType & FrequencyTypeEnum.RecurringMonthly2) == FrequencyTypeEnum.RecurringMonthly2); }
            set
            {
                if (value)
                    frequencyType = FrequencyTypeEnum.RecurringMonthly2 | (frequencyType & FrequencyTypeEnum.FrequencyTypeAttributesMask);
                else
                    frequencyType &= ~FrequencyTypeEnum.RecurringMonthly2;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool Cyclic
        {
            get { return (!ToRunOnDemand && (frequencyType & FrequencyTypeEnum.Cyclic) == FrequencyTypeEnum.Cyclic); }
            set
            {
                if (value && !ToRunOnDemand)
                    frequencyType |= FrequencyTypeEnum.Cyclic;
                else
                    frequencyType &= ~FrequencyTypeEnum.Cyclic;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool Temporary
        {
            get { return ((frequencyType & FrequencyTypeEnum.Temporary) == FrequencyTypeEnum.Temporary); }
            set
            {
                if (value)
                    frequencyType |= FrequencyTypeEnum.Temporary;
                else
                    frequencyType &= ~FrequencyTypeEnum.Temporary;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool IsCyclicToRepeat
        {
            get { return Cyclic && (cyclicRepeat < 0 || cyclicRepeat > 1); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool IsCyclicInfinite
        {
            get { return Cyclic && (cyclicRepeat < 0); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool CyclicRepeatRunning
        {
            get { return (!Temporary && IsCyclicToRepeat && (frequencyType & FrequencyTypeEnum.CyclicRepeatRunning) == FrequencyTypeEnum.CyclicRepeatRunning); }
            set
            {
                if (value && !Temporary && IsCyclicToRepeat)
                    frequencyType |= FrequencyTypeEnum.CyclicRepeatRunning;
                else
                    frequencyType &= ~FrequencyTypeEnum.CyclicRepeatRunning;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int FrequencyInterval
        {
            get { return frequencyInterval; }
            set
            {
                if (DailyRecurring || Monthly1Recurring)
                {
                    frequencyInterval = value;
                }
                else if (WeeklyRecurring)
                {
                    frequencyInterval = value;

                    frequencyInterval &= (int)(FrequencyRecurringIntervalTypeEnum.WeeklySunday |
                        FrequencyRecurringIntervalTypeEnum.WeeklyMonday |
                        FrequencyRecurringIntervalTypeEnum.WeeklyTuesday |
                        FrequencyRecurringIntervalTypeEnum.WeeklyWednesday |
                        FrequencyRecurringIntervalTypeEnum.WeeklyThursday |
                        FrequencyRecurringIntervalTypeEnum.WeeklyFriday |
                        FrequencyRecurringIntervalTypeEnum.WeeklySaturday);
                }
                else if (Monthly2Recurring)
                {
                    frequencyInterval = value;

                    frequencyInterval &= (int)(FrequencyRecurringIntervalTypeEnum.Monthly2Sunday |
                        FrequencyRecurringIntervalTypeEnum.Monthly2Monday |
                        FrequencyRecurringIntervalTypeEnum.Monthly2Tuesday |
                        FrequencyRecurringIntervalTypeEnum.Monthly2Wednesday |
                        FrequencyRecurringIntervalTypeEnum.Monthly2Thursday |
                        FrequencyRecurringIntervalTypeEnum.Monthly2Friday |
                        FrequencyRecurringIntervalTypeEnum.Monthly2Saturday |
                        FrequencyRecurringIntervalTypeEnum.Monthly2Day |
                        FrequencyRecurringIntervalTypeEnum.Monthly2DayOfWeek |
                        FrequencyRecurringIntervalTypeEnum.Monthly2WeekendDay);
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int Monthly2RecurringDayTypeIndex
        {
            get
            {
                if (!Monthly2Recurring)
                    return -1;

                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Sunday)
                    return 0;
                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Monday)
                    return 1;
                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Tuesday)
                    return 2;
                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Wednesday)
                    return 3;
                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Thursday)
                    return 4;
                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Friday)
                    return 5;
                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Saturday)
                    return 6;
                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Day)
                    return 7;
                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2DayOfWeek)
                    return 8;
                if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2WeekendDay)
                    return 9;

                return -1;
            }

            set
            {
                if (!Monthly2Recurring)
                    return;

                switch (value)
                {
                    case 0:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2Sunday;
                        break;
                    case 1:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2Monday;
                        break;
                    case 2:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2Tuesday;
                        break;
                    case 3:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2Wednesday;
                        break;
                    case 4:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2Thursday;
                        break;
                    case 5:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2Friday;
                        break;
                    case 6:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2Saturday;
                        break;
                    case 7:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2Day;
                        break;
                    case 8:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2DayOfWeek;
                        break;
                    case 9:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2WeekendDay;
                        break;
                    default:
                        frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Undefined;
                        break;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int FrequencyRecurringFactor
        {
            get
            {
                return frequencyRecurringFactor;
            }
            set
            {
                if (DailyRecurring)
                {
                    frequencyRecurringFactor = 0;
                    return;
                }
                if (value < 1)
                {
                    Debug.Fail("Set WTEScheduledTask.FrequencyRecurringFactor property error: invalid value");
                    return;
                }
                frequencyRecurringFactor = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int FrequencyRelativeIntervalTypeIndex
        {
            get
            {
                if (!Monthly2Recurring)
                    return -1;

                if (frequencyRelativeInterval == FrequencyRelativeIntervalTypeEnum.Monthly2First)
                    return 0;
                if (frequencyRelativeInterval == FrequencyRelativeIntervalTypeEnum.Monthly2Second)
                    return 1;
                if (frequencyRelativeInterval == FrequencyRelativeIntervalTypeEnum.Monthly2Third)
                    return 2;
                if (frequencyRelativeInterval == FrequencyRelativeIntervalTypeEnum.Monthly2Fourth)
                    return 3;
                if (frequencyRelativeInterval == FrequencyRelativeIntervalTypeEnum.Monthly2Last)
                    return 4;

                return -1;
            }
            set
            {
                if (!Monthly2Recurring)
                {
                    frequencyRelativeInterval = FrequencyRelativeIntervalTypeEnum.Undefined;
                    return;
                }

                switch (value)
                {
                    case 0:
                        frequencyRelativeInterval = FrequencyRelativeIntervalTypeEnum.Monthly2First;
                        break;
                    case 1:
                        frequencyRelativeInterval = FrequencyRelativeIntervalTypeEnum.Monthly2Second;
                        break;
                    case 2:
                        frequencyRelativeInterval = FrequencyRelativeIntervalTypeEnum.Monthly2Third;
                        break;
                    case 3:
                        frequencyRelativeInterval = FrequencyRelativeIntervalTypeEnum.Monthly2Fourth;
                        break;
                    case 4:
                        frequencyRelativeInterval = FrequencyRelativeIntervalTypeEnum.Monthly2Last;
                        break;
                    default:
                        Debug.Fail("Set WTEScheduledTask.FrequencyRelativeIntervalTypeIndex Error: invalid value.");
                        frequencyRelativeInterval = FrequencyRelativeIntervalTypeEnum.Undefined;
                        break;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool DailyFrequenceOnce
        {
            get { return (frequencySubtype == FrequencySubtypeEnum.DailyOnce); }
            set
            {
                if (value)
                    frequencySubtype = FrequencySubtypeEnum.DailyOnce;
                else
                    frequencySubtype &= ~FrequencySubtypeEnum.DailyOnce;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool DailyFrequenceHoursInterval
        {
            get { return (frequencySubtype == FrequencySubtypeEnum.HoursInterval); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool DailyFrequenceMinutesInterval
        {
            get { return (frequencySubtype == FrequencySubtypeEnum.MinutesInterval); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int DailyFrequenceIntervalTypeIndex
        {
            get
            {
                if (DailyFrequenceOnce)
                    return -1;

                if (frequencySubtype == FrequencySubtypeEnum.MinutesInterval)
                    return 0;
                if (frequencySubtype == FrequencySubtypeEnum.HoursInterval)
                    return 1;

                return -1;
            }
            set
            {
                if (DailyFrequenceOnce)
                {
                    frequencySubtype = FrequencySubtypeEnum.Undefined;
                    return;
                }

                switch (value)
                {
                    case 0:
                        frequencySubtype = FrequencySubtypeEnum.MinutesInterval;
                        break;
                    case 1:
                        frequencySubtype = FrequencySubtypeEnum.HoursInterval;
                        break;
                    default:
                        Debug.Fail("Set WTEScheduledTask.DailyFrequenceIntervalTypeIndex Error: invalid value.");
                        frequencySubtype = FrequencySubtypeEnum.Undefined;
                        break;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int FrequencySubinterval
        {
            get
            {
                return frequencySubinterval;
            }
            set
            {
                if (DailyFrequenceOnce)
                {
                    frequencySubinterval = 0;
                    return;
                }
                if (value < 1)
                {
                    Debug.Fail("Set WTEScheduledTask.FrequencySubinterval Error: invalid value.");
                    return;
                }
                frequencySubinterval = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public DateTime ActiveStartDate
        {
            get
            {
                return new DateTime(activeStartDate.Year, activeStartDate.Month, activeStartDate.Day, activeStartDate.Hour, activeStartDate.Minute, 0);
            }
            set
            {
                TimeSpan timeToSave = ActiveStartTime;
                activeStartDate = new DateTime(value.Year, value.Month, value.Day);
                ActiveStartTime = timeToSave;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public DateTime ActiveEndDate
        {
            get
            {
                return new DateTime(activeEndDate.Year, activeEndDate.Month, activeEndDate.Day, activeEndDate.Hour, activeEndDate.Minute, 0);
            }
            set
            {
                TimeSpan timeToSave = ActiveEndTime;
                activeEndDate = new DateTime(value.Year, value.Month, value.Day);
                ActiveEndTime = timeToSave;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public TimeSpan ActiveStartTime
        {
            get
            {
                return activeStartDate.TimeOfDay;
            }
            set
            {
                activeStartDate = new DateTime(activeStartDate.Year, activeStartDate.Month, activeStartDate.Day, value.Hours, value.Minutes, 0);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public TimeSpan ActiveEndTime
        {
            get
            {
                return activeEndDate.TimeOfDay;
            }
            set
            {
                activeEndDate = new DateTime(activeEndDate.Year, activeEndDate.Month, activeEndDate.Day, value.Hours, value.Minutes, 0);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public DateTime MinimumDate
        {
            get
            {
                return new DateTime(1753, 1, 1, 0, 0, 0);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public DateTime LastRunDate
        {
            get
            {
                return new DateTime(lastRunDate.Year, lastRunDate.Month, lastRunDate.Day, lastRunDate.Hour, lastRunDate.Minute, 0);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool LastRunDateUndefined
        {
            get
            {
                return LastRunDate <= MinimumDate;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public DateTime NextRunDate
        {
            get
            {
                return new DateTime(nextRunDate.Year, nextRunDate.Month, nextRunDate.Day, nextRunDate.Hour, nextRunDate.Minute, 0);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool NextRunDateUndefined
        {
            get
            {
                return NextRunDate >= DateTime.MaxValue;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public DateTime LastRunRetries
        {
            get
            {
                return new DateTime(lastRunRetries.Year, lastRunRetries.Month, lastRunRetries.Day, lastRunRetries.Hour, lastRunRetries.Minute, 0);
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int RetryAttempts
        {
            get { return retryAttempts; }
            set
            {
                if (value >= 0 && value <= RetryAttemptsMaxNumber)
                    retryAttempts = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int RetryDelay
        {
            get { return retryDelay; }
            set
            {
                if (value >= 0 && value <= RetryDelayMaximum)
                    retryDelay = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public int RetryAttemptsActualCount { get { return retryAttemptsActualCount; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public int LastRunCompletitionLevel { get { return (int)lastRunCompletitionLevel; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SequenceLastRunPartiallySuccessfull { get { return (IsSequence && lastRunCompletitionLevel == CompletitionLevelEnum.SequencePartialSuccess); } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SequenceLastRunInterrupted { get { return (IsSequence && lastRunCompletitionLevel == CompletitionLevelEnum.SequenceInterrupted); } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public bool LastRunSuccessfull { get { return (lastRunCompletitionLevel == CompletitionLevelEnum.Success) || SequenceLastRunPartiallySuccessfull; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public bool LastRunFailed { get { return (lastRunCompletitionLevel == CompletitionLevelEnum.Failure) || SequenceLastRunInterrupted; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SendMailUsingSMTP { get { return sendMailUsingSMTP; } set { sendMailUsingSMTP = value; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public int CyclicRepeat { get { return cyclicRepeat; } set { cyclicRepeat = value; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public int CyclicDelay { get { return cyclicDelay; } set { cyclicDelay = value; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public string CyclicTaskCode { get { return cyclicTaskCode; } set { cyclicTaskCode = value; } }
        //--------------------------------------------------------------------------------------------------------------------------------
        public string CyclicOriginalTaskCode { get { return Temporary ? cyclicTaskCode : code; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public WTETasksInScheduledSequenceCollection TasksInSequence
        {
            get
            {
                if (!IsSequence)
                {
                    Debug.Fail(String.Format("Get WTEScheduledTask.TasksInSequence Error: the task {0} is not a sequence.", Code));
                    return null;
                }
                return tasksInSequence;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public WTETaskNotificationRecipientsCollection NotificationRecipients { get { return notificationRecipients; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool HasXmlParameters { get { return RunBatch || RunReport || RunFunction || RunDataExport || RunDataImport; } }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string XmlParameters
        {
            get
            {
                return HasXmlParameters ? xmlParameters : String.Empty;
            }

            set
            {
                if (!HasXmlParameters || value == null || value.Length == 0)
                {
                    xmlParameters = String.Empty;
                    return;
                }
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(value);
                    xmlParameters = value;
                }
                catch (XmlException)
                {
                    //There is a load or parse error in the XML.
                    xmlParameters = String.Empty;
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string ImpersonationDomain
        {
            get
            {
                if (!IsToImpersonateWindowsUser)
                    return String.Empty;

                return impersonationDomain;
            }
            set
            {
                if (!IsToImpersonateWindowsUser)
                {
                    impersonationDomain = String.Empty;
                    return;
                }
                impersonationDomain = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string ImpersonationUser
        {
            get
            {
                if (!IsToImpersonateWindowsUser)
                    return String.Empty;

                return impersonationUser;
            }
            set
            {
                if (!IsToImpersonateWindowsUser)
                {
                    impersonationUser = String.Empty;
                    return;
                }
                impersonationUser = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string ImpersonationPassword
        {
            set
            {
                if (!IsToImpersonateWindowsUser)
                {
                    impersonationPassword = String.Empty;
                    return;
                }
                impersonationPassword = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string MessageContent
        {
            get
            {
                if (!SendMessage && !SendMail)
                    return String.Empty;

                return messageContent;
            }
            set
            {
                if (SendMessage || SendMail)
                    messageContent = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string ReportSendingRecipients
        {
            get
            {
                if (!SendReportAsWTEMailAttachment)
                    return String.Empty;

                return messageContent;
            }
            set
            {
                if (SendReportAsWTEMailAttachment)
                    messageContent = value;
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public string ReportSavingFileName
        {
            get
            {
                if (!SaveReportAsFile)
                    return String.Empty;

                return messageContent;
            }
            set
            {
                if (SaveReportAsFile)
                    messageContent = value;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------
        public WTEScheduledTaskObj(int aCompanyId, int aLoginId)
		{
			Clear();
			notificationRecipients = new WTETaskNotificationRecipientsCollection();
			notificationRecipients.Clear();

			id = Guid.NewGuid();
			companyId = aCompanyId;
			loginId = aLoginId;
		}

		//-----------------------------------------------------------------------------------------------------
		public WTEScheduledTaskObj(WTEScheduledTaskObj aTask) : this(aTask, true)
		{
		}
		
		//-----------------------------------------------------------------------------------------------------
		public WTEScheduledTaskObj(WTEScheduledTaskObj aTask, bool copyRunInfo) : this(0, 0)
		{
			if (aTask == null)
				return;

			id							= aTask.Id;
			code						= aTask.Code;
			companyId					= aTask.CompanyId;
			loginId						= aTask.LoginId;
			type						= (TaskTypeEnum)aTask.Type;
			runningOptions				= (RunningOptionsEnum)aTask.RunningOptions;
			command						= aTask.Command;
			description					= aTask.Description;
			enabled						= aTask.Enabled;
			frequencyType				= (FrequencyTypeEnum)aTask.FrequencyType;
			frequencySubtype			= (FrequencySubtypeEnum)aTask.FrequencySubtype;
			frequencyInterval			= aTask.FrequencyInterval;
			frequencySubinterval		= aTask.FrequencySubinterval;
			frequencyRelativeInterval	= (FrequencyRelativeIntervalTypeEnum)aTask.FrequencyRelativeInterval;
			frequencyRecurringFactor	= aTask.FrequencyRecurringFactor;

			activeStartDate	= aTask.ActiveStartDate;
			activeEndDate	= aTask.ActiveEndDate;

			if (copyRunInfo)
			{
				lastRunDate = aTask.LastRunDate; 
				lastRunRetries = aTask.LastRunRetries; 
				lastRunCompletitionLevel = (CompletitionLevelEnum)aTask.LastRunCompletitionLevel;
			}

			nextRunDate = aTask.NextRunDate; 

			retryAttempts = aTask.RetryAttempts;
			retryDelay = aTask.RetryDelay;
			
			if (copyRunInfo)
				retryAttemptsActualCount = aTask.RetryAttemptsActualCount;

			sendMailUsingSMTP = aTask.SendMailUsingSMTP;

			cyclicRepeat	= aTask.CyclicRepeat;
			cyclicDelay		= aTask.CyclicDelay;
			cyclicTaskCode	= aTask.CyclicTaskCode;

			impersonationDomain = aTask.ImpersonationDomain;
			impersonationUser = aTask.ImpersonationUser;
			impersonationPassword = aTask.impersonationPassword;

			messageContent = aTask.messageContent;

			xmlParameters = aTask.XmlParameters;
			
			if (aTask.IsSequence && aTask.TasksInSequence != null)
			{
				if (tasksInSequence == null)
					tasksInSequence = new WTETasksInScheduledSequenceCollection();
				tasksInSequence.Clear();
				
				foreach (WTETaskInScheduledSequence task in aTask.TasksInSequence)
					tasksInSequence.Add(new WTETaskInScheduledSequence(task));
			}

			foreach (WTETaskNotificationRecipient notificationRecipient in aTask.NotificationRecipients)
				notificationRecipients.Add(new WTETaskNotificationRecipient(notificationRecipient));
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public WTEScheduledTaskObj(DataRow aTaskTableRow, string connectionString) : this(0, 0)
		{
			if (aTaskTableRow == null || String.Compare(aTaskTableRow.Table.TableName, WTEScheduledTask.ScheduledTasksTableName, true, CultureInfo.InvariantCulture) != 0)
				return;

			if (connectionString == null || connectionString.Length == 0)
			{
				Debug.Fail("WTEScheduledTask Constructor Error: null or empty connection string.");
                throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.XEngineNotActivatedMsgFmt);// EmptyConnectionStringMsg);
			}
			
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();

				id = new Guid(aTaskTableRow[WTEScheduledTask.IdColumnName].ToString());
				code = (string)aTaskTableRow[WTEScheduledTask.CodeColumnName];
				companyId = (int)aTaskTableRow[WTEScheduledTask.CompanyIdColumnName];
				loginId = (int)aTaskTableRow[WTEScheduledTask.LoginIdColumnName];
				
				type = (TaskTypeEnum)aTaskTableRow[WTEScheduledTask.TypeColumnName];
				if (IsSequence)
					LoadTasksInSequence(connectionString);

				runningOptions = (RunningOptionsEnum)aTaskTableRow[WTEScheduledTask.RunningOptionsColumnName];
				
				enabled = (aTaskTableRow[WTEScheduledTask.EnabledColumnName] != System.DBNull.Value) ? (bool)aTaskTableRow[WTEScheduledTask.EnabledColumnName] : false;
				Command	= (aTaskTableRow[WTEScheduledTask.CommandColumnName] != System.DBNull.Value) ? (string)aTaskTableRow[WTEScheduledTask.CommandColumnName] : String.Empty;
				description	= (aTaskTableRow[WTEScheduledTask.DescriptionColumnName] != System.DBNull.Value) ? (string)aTaskTableRow[WTEScheduledTask.DescriptionColumnName] : String.Empty;
				
				frequencyType = (FrequencyTypeEnum)aTaskTableRow[WTEScheduledTask.FrequencyTypeColumnName];
				frequencySubtype = (FrequencySubtypeEnum)aTaskTableRow[WTEScheduledTask.FrequencySubtypeColumnName];

				frequencyInterval			= (aTaskTableRow[WTEScheduledTask.FrequencyIntervalColumnName] != System.DBNull.Value) ? (int)aTaskTableRow[WTEScheduledTask.FrequencyIntervalColumnName] : 1;
				frequencySubinterval		= (aTaskTableRow[WTEScheduledTask.FrequencySubintervalColumnName] != System.DBNull.Value) ? (int)aTaskTableRow[WTEScheduledTask.FrequencySubintervalColumnName] : 0;
				frequencyRelativeInterval	= (aTaskTableRow[WTEScheduledTask.FrequencyRelativeIntervalColumnName] != System.DBNull.Value) ? (FrequencyRelativeIntervalTypeEnum)aTaskTableRow[WTEScheduledTask.FrequencyRelativeIntervalColumnName] : FrequencyRelativeIntervalTypeEnum.Undefined;
				frequencyRecurringFactor	= (aTaskTableRow[WTEScheduledTask.FrequencyRecurringFactorColumnName] != System.DBNull.Value) ? (int)aTaskTableRow[WTEScheduledTask.FrequencyRecurringFactorColumnName] : 1;

				activeStartDate	= (aTaskTableRow[WTEScheduledTask.ActiveStartDateColumnName] != System.DBNull.Value) ? (DateTime)aTaskTableRow[WTEScheduledTask.ActiveStartDateColumnName] : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
				activeEndDate	= (aTaskTableRow[WTEScheduledTask.ActiveEndDateColumnName] != System.DBNull.Value) ? (DateTime)aTaskTableRow[WTEScheduledTask.ActiveEndDateColumnName] : DateTime.MaxValue;

				lastRunDate = (aTaskTableRow[WTEScheduledTask.LastRunDateColumnName] != System.DBNull.Value) ? (DateTime)aTaskTableRow[WTEScheduledTask.LastRunDateColumnName] : MinimumDate; 
				lastRunRetries = (aTaskTableRow[WTEScheduledTask.LastRunRetriesColumnName] != System.DBNull.Value) ? (DateTime)aTaskTableRow[WTEScheduledTask.LastRunRetriesColumnName] : MinimumDate; 
				nextRunDate = (aTaskTableRow[WTEScheduledTask.NextRunDateColumnName] != System.DBNull.Value) ? (DateTime)aTaskTableRow[WTEScheduledTask.NextRunDateColumnName] : DateTime.MaxValue; 

				retryAttempts = (aTaskTableRow[WTEScheduledTask.RetryAttemptsColumnName] != System.DBNull.Value) ? (int)aTaskTableRow[WTEScheduledTask.RetryAttemptsColumnName] : 0;
				retryDelay = (aTaskTableRow[WTEScheduledTask.RetryDelayColumnName] != System.DBNull.Value) ? (int)aTaskTableRow[WTEScheduledTask.RetryDelayColumnName] : 0;
				retryAttemptsActualCount = (aTaskTableRow[WTEScheduledTask.RetryAttemptsActualCountColumnName] != System.DBNull.Value) ? (int)aTaskTableRow[WTEScheduledTask.RetryAttemptsActualCountColumnName] : 0;
				
				lastRunCompletitionLevel = (CompletitionLevelEnum)(Int16)aTaskTableRow[WTEScheduledTask.LastRunCompletitionLevelColumnName];
				
				sendMailUsingSMTP = (aTaskTableRow[WTEScheduledTask.SendMailUsingSMTPColumnName] != System.DBNull.Value) ? (bool)aTaskTableRow[WTEScheduledTask.SendMailUsingSMTPColumnName] : true;
				
				cyclicRepeat = (aTaskTableRow[WTEScheduledTask.CyclicRepeatColumnName] != System.DBNull.Value)? (int)aTaskTableRow[WTEScheduledTask.CyclicRepeatColumnName] : 0;
				cyclicDelay = (aTaskTableRow[WTEScheduledTask.CyclicDelayColumnName] != System.DBNull.Value) ? (int)aTaskTableRow[WTEScheduledTask.CyclicDelayColumnName] : 0;
				cyclicTaskCode = (aTaskTableRow[WTEScheduledTask.CyclicTaskCodeColumnName] != System.DBNull.Value) ? (string)aTaskTableRow[WTEScheduledTask.CyclicTaskCodeColumnName]: String.Empty;
				
				LoadCommandParametersFromFile(connection);

				impersonationDomain = (aTaskTableRow[WTEScheduledTask.ImpersonationDomainColumnName] != System.DBNull.Value) ? (string)aTaskTableRow[WTEScheduledTask.ImpersonationDomainColumnName] : String.Empty;
				impersonationUser = (aTaskTableRow[WTEScheduledTask.ImpersonationUserColumnName] != System.DBNull.Value) ? (string)aTaskTableRow[WTEScheduledTask.ImpersonationUserColumnName] : String.Empty;
				impersonationPassword = (aTaskTableRow[WTEScheduledTask.ImpersonationPasswordColumnName] != System.DBNull.Value) ? DecryptPassword((string)aTaskTableRow[WTEScheduledTask.ImpersonationPasswordColumnName]) : String.Empty;
				
				messageContent = (aTaskTableRow[WTEScheduledTask.MessageContentColumnName] != System.DBNull.Value) ? (string)aTaskTableRow[WTEScheduledTask.MessageContentColumnName] : String.Empty;

				LoadMailNotificationsSettings(connection);
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WTEScheduledTask constructor: " + exception.Message);
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InvalidTaskConstruction, exception);
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}


        //--------------------------------------------------------------------------------------------------------------------------------
        public WTEScheduledTaskObj(string aTaskCode, int aCompanyId, int aLoginId, string connectionString, bool loadRelatedData)
        {
            Clear();
            notificationRecipients = new WTETaskNotificationRecipientsCollection();
            notificationRecipients.Clear();

            if (connectionString == null || connectionString.Length == 0)
            {
                Debug.Fail("WTEScheduledTask Constructor Error: null or empty connection string.");
                throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
            }

            SqlDataReader taskDataReader = WTEScheduledTask.GetTaskData(aLoginId, aCompanyId, aTaskCode, connectionString);

            FillFromTaskDataReader(taskDataReader, connectionString);

            taskDataReader.Close();

            SqlConnection connection = null;
            connection = new SqlConnection(connectionString);
            connection.Open();

             if (loadRelatedData)
            {
                LoadCommandParametersFromFile(connection);

                LoadMailNotificationsSettings(connection);
            }
        }

		//--------------------------------------------------------------------------------------------------------------------------------
		public WTEScheduledTaskObj(Guid aTaskId, string connectionString, bool loadRelatedData)
		{
			Clear();
			notificationRecipients = new WTETaskNotificationRecipientsCollection();
			notificationRecipients.Clear();
			
			if (connectionString == null || connectionString.Length == 0)
			{
				Debug.Fail("WTEScheduledTask Constructor Error: null or empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}
			
			id = aTaskId;

			SqlConnection connection = null;

			try
			{
				RefreshData(connectionString);

				connection = new SqlConnection(connectionString);
				connection.Open();
			
				if (loadRelatedData)
				{
					LoadCommandParametersFromFile(connection);

					LoadMailNotificationsSettings(connection);
				}
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in WTEScheduledTask constructor: " + exception.Message);
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InvalidTaskConstruction, exception);
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

        //--------------------------------------------------------------------------------------------------------------------------------
        private void LoadMailNotificationsSettings(SqlConnection connection)
        {
            notificationRecipients.Clear();

           
            SqlDataReader selectDataReader = null;
            selectDataReader = TaskNotificationRecipientEngine.LoadMailNotificationsSettings(connection, Id);

                while (selectDataReader.Read())
                {
                    notificationRecipients.Add(
                        new WTETaskNotificationRecipient(
                            selectDataReader[TaskNotificationRecipientEngine.RecipientNameColumnName].ToString(),
                            Convert.ToInt32(selectDataReader[TaskNotificationRecipientEngine.SendConditionColumnName].ToString())
                            )
                        );
                }
        }

        #region WTEScheduledTask private methods

       
        //--------------------------------------------------------------------------------------------------------------------------------
        private void FillFromTaskDataReader(SqlDataReader taskDataReader, string connectionString, bool onlyUI)
		{
			if (taskDataReader == null || taskDataReader.IsClosed)
			{
				Debug.Fail("WTEScheduledTask.FillFromTaskDataReader Error: invalid SqlDataReader.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSqlDataReaderErrMsg);
			}

			if (connectionString == null || connectionString.Length == 0)
			{
				Debug.Fail("WTEScheduledTask.FillFromTaskDataReader Error: null or empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}
			
			SqlConnection connection = null;

			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();
		
				id = new Guid(taskDataReader[WTEScheduledTask.IdColumnName].ToString()); 
				code = (string)taskDataReader[WTEScheduledTask.CodeColumnName];
				companyId = (int)taskDataReader[WTEScheduledTask.CompanyIdColumnName];
				loginId = (int)taskDataReader[WTEScheduledTask.LoginIdColumnName];
				
				type = (TaskTypeEnum)taskDataReader[WTEScheduledTask.TypeColumnName];
				if (IsSequence && !onlyUI)
					LoadTasksInSequence(connectionString);

				runningOptions = (RunningOptionsEnum)taskDataReader[WTEScheduledTask.RunningOptionsColumnName];
				
				enabled = (taskDataReader[WTEScheduledTask.EnabledColumnName] != System.DBNull.Value) ? (bool)taskDataReader[WTEScheduledTask.EnabledColumnName] : false;
				Command	= (taskDataReader[WTEScheduledTask.CommandColumnName] != System.DBNull.Value) ? (string)taskDataReader[WTEScheduledTask.CommandColumnName] : String.Empty;
				description	= (taskDataReader[WTEScheduledTask.DescriptionColumnName] != System.DBNull.Value) ? (string)taskDataReader[WTEScheduledTask.DescriptionColumnName] : String.Empty;

				frequencyType = (FrequencyTypeEnum)taskDataReader[WTEScheduledTask.FrequencyTypeColumnName];
				frequencySubtype = (FrequencySubtypeEnum)taskDataReader[WTEScheduledTask.FrequencySubtypeColumnName];

				frequencyInterval			= (taskDataReader[WTEScheduledTask.FrequencyIntervalColumnName] != System.DBNull.Value) ? (int)taskDataReader[WTEScheduledTask.FrequencyIntervalColumnName] : 1;
				frequencySubinterval		= (taskDataReader[WTEScheduledTask.FrequencySubintervalColumnName] != System.DBNull.Value) ? (int)taskDataReader[WTEScheduledTask.FrequencySubintervalColumnName] : 0;
				frequencyRelativeInterval	= (taskDataReader[WTEScheduledTask.FrequencyRelativeIntervalColumnName] != System.DBNull.Value) ? (FrequencyRelativeIntervalTypeEnum)taskDataReader[WTEScheduledTask.FrequencyRelativeIntervalColumnName] : FrequencyRelativeIntervalTypeEnum.Undefined;
				frequencyRecurringFactor	= (taskDataReader[WTEScheduledTask.FrequencyRecurringFactorColumnName] != System.DBNull.Value) ? (int)taskDataReader[WTEScheduledTask.FrequencyRecurringFactorColumnName] : 1;

				activeStartDate	= (taskDataReader[WTEScheduledTask.ActiveStartDateColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[WTEScheduledTask.ActiveStartDateColumnName] : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
				activeEndDate	= (taskDataReader[WTEScheduledTask.ActiveEndDateColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[WTEScheduledTask.ActiveEndDateColumnName] : DateTime.MaxValue;

				lastRunDate = (taskDataReader[WTEScheduledTask.LastRunDateColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[WTEScheduledTask.LastRunDateColumnName] : MinimumDate; 
				lastRunRetries = (taskDataReader[WTEScheduledTask.LastRunRetriesColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[WTEScheduledTask.LastRunRetriesColumnName] : MinimumDate; 
				nextRunDate = (taskDataReader[WTEScheduledTask.NextRunDateColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[WTEScheduledTask.NextRunDateColumnName] : DateTime.MaxValue; 

				retryAttempts = (taskDataReader[WTEScheduledTask.RetryAttemptsColumnName] != System.DBNull.Value) ? (int)taskDataReader[WTEScheduledTask.RetryAttemptsColumnName] : 0;
				retryDelay = (taskDataReader[WTEScheduledTask.RetryDelayColumnName] != System.DBNull.Value) ? (int)taskDataReader[WTEScheduledTask.RetryDelayColumnName] : 0;
				retryAttemptsActualCount = (taskDataReader[WTEScheduledTask.RetryAttemptsActualCountColumnName] != System.DBNull.Value) ? (int)taskDataReader[WTEScheduledTask.RetryAttemptsActualCountColumnName] : 0;
					
				lastRunCompletitionLevel = (CompletitionLevelEnum)(Int16)taskDataReader[WTEScheduledTask.LastRunCompletitionLevelColumnName];
				
				sendMailUsingSMTP = (taskDataReader[WTEScheduledTask.SendMailUsingSMTPColumnName] != System.DBNull.Value) ? (bool)taskDataReader[WTEScheduledTask.SendMailUsingSMTPColumnName] : true;
					
				cyclicRepeat = (taskDataReader[WTEScheduledTask.CyclicRepeatColumnName] != System.DBNull.Value)? (int)taskDataReader[WTEScheduledTask.CyclicRepeatColumnName] : 0;
				cyclicDelay = (taskDataReader[WTEScheduledTask.CyclicDelayColumnName] != System.DBNull.Value) ? (int)taskDataReader[WTEScheduledTask.CyclicDelayColumnName] : 0;
				cyclicTaskCode = (taskDataReader[WTEScheduledTask.CyclicTaskCodeColumnName] != System.DBNull.Value) ? (string)taskDataReader[WTEScheduledTask.CyclicTaskCodeColumnName]: String.Empty;
	
				impersonationDomain = (taskDataReader[WTEScheduledTask.ImpersonationDomainColumnName] != System.DBNull.Value) ? (string)taskDataReader[WTEScheduledTask.ImpersonationDomainColumnName] : String.Empty;
				impersonationUser = (taskDataReader[WTEScheduledTask.ImpersonationUserColumnName] != System.DBNull.Value) ? (string)taskDataReader[WTEScheduledTask.ImpersonationUserColumnName] : String.Empty;
				impersonationPassword = (taskDataReader[WTEScheduledTask.ImpersonationPasswordColumnName] != System.DBNull.Value) ? DecryptPassword((string)taskDataReader[WTEScheduledTask.ImpersonationPasswordColumnName]) : String.Empty;
				
				messageContent = (taskDataReader[WTEScheduledTask.MessageContentColumnName] != System.DBNull.Value) ? (string)taskDataReader[WTEScheduledTask.MessageContentColumnName] : String.Empty;
			}
			catch(Exception exception)
			{
				Debug.Fail("Exception raised in WTEScheduledTask.FillFromTaskDataReader: " + exception.Message);
				throw new WTEScheduledTaskException(exception.Message);
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void FillFromTaskDataReader(SqlDataReader taskDataReader, string connectionString)
		{
			FillFromTaskDataReader(taskDataReader, connectionString, false);
		}

        //--------------------------------------------------------------------------------------------------------------------------------
		private void FindNearestNextRunDate(ref DateTime aRunDate)
		{
			if (WeeklyRecurring)
			{
				if (frequencyInterval == 0)
					frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.WeeklySunday;

				// DateTime.DayOfWeek returns a DayOfWeek enumerated constant that indicates the
				// day of the week. This property value ranges from zero, indicating Sunday, to 
				// six, indicating Saturday.
				if ((frequencyInterval & (0x00000001 << (int)aRunDate.DayOfWeek)) == 0)
					IncrementRunDate(ref aRunDate);
			}
			
			if ((Monthly1Recurring && aRunDate.Day != frequencyInterval) || (Monthly2Recurring && !IsValidMonthly2Date(aRunDate)))
				IncrementRunDate(ref aRunDate);

			DateTime currentDate = DateTime.Now;
			// imposto correttamente l'ora
			if (DailyFrequenceOnce)
				aRunDate = new DateTime(aRunDate.Year, aRunDate.Month, aRunDate.Day, activeStartDate.Hour, activeStartDate.Minute, 0);
			else
			{
				int currTotalMinutes = currentDate.Hour * 60 + currentDate.Minute;
				int totalMinutes  = activeStartDate.Hour * 60 + activeStartDate.Minute;
				while (currTotalMinutes > totalMinutes)
				{
					totalMinutes += DailyFrequenceHoursInterval ? frequencySubinterval * 60 :frequencySubinterval;
				}
				aRunDate = new DateTime(aRunDate.Year, aRunDate.Month, aRunDate.Day);
				aRunDate = aRunDate.AddMinutes(totalMinutes);
			}
			
			while ((DateTime.Compare(currentDate, aRunDate) > 0) && IncrementRunDate(ref aRunDate)) ;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool IncrementRunDate(ref DateTime aRunDate)
		{
			if (!Recurring)
				return false;

			DateTime tmpNextRunDate = aRunDate <= DateTime.Now
                ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0)
                : new DateTime(aRunDate.Year, aRunDate.Month, aRunDate.Day, aRunDate.Hour, aRunDate.Minute, 0);

			// In base alla ricorrenza rispetto alla data e tempo correnti devo 
			// calcolare la prossima data di esecuzione ed aggiornare nextRunDate

			// Prima vedo l'eventuale numero di giorni da far scattare: mi chiedo,
			// cioè, quale sarebbero i giorni che intercorrono tra la data 
			// corrente ed il prossimo giorno utile
			int elapsedDays = 0;
			bool addElapsedDays = false;

			if (DailyRecurring)
				elapsedDays = Math.Max(1,frequencyInterval);
			else if (WeeklyRecurring)
			{
				if (frequencyInterval == 0)
					frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.WeeklySunday;
				
				// la funzione DayOfWeek restituisce i seguenti valori:
				// 0 per Sunday		
				// 1  "  Monday		
				// 2  "  Tuesday	
				// 3  "  Wednesday	
				// 4  "  Thursday	
				// 6  "  Friday	
				// Tali valori corrispondono all'esponente di 2 in FrequencyRecurringIntervalTypeEnum
				int dayShiftPos = (int)tmpNextRunDate.DayOfWeek;
				int nextDayShift = (frequencyInterval >> (dayShiftPos + 1) != 0) ? (dayShiftPos + 1) : 0; 

				while ((0x00000001 & (frequencyInterval >> nextDayShift)) == 0)
					nextDayShift++;

				if (nextDayShift <= dayShiftPos) // devo saltare di n settimane,...
					elapsedDays = frequencyRecurringFactor * 7;
				elapsedDays += (nextDayShift - dayShiftPos);
			}
			else if (Monthly1Recurring)
			{
				int dayOfMonth = frequencyInterval;

				if	(tmpNextRunDate.Day >= dayOfMonth || dayOfMonth > CultureInfo.CurrentUICulture.Calendar.GetDaysInMonth(tmpNextRunDate.Year, tmpNextRunDate.Month))
				{
					int newDateYear = tmpNextRunDate.Year;
					int newDateMonth = tmpNextRunDate.Month;
					do
					{	
						newDateYear += (newDateMonth + frequencyRecurringFactor) / 13;
						newDateMonth = Math.Max((newDateMonth + frequencyRecurringFactor) % 13, 1);
					}
					while (dayOfMonth > CultureInfo.CurrentUICulture.Calendar.GetDaysInMonth(newDateYear, newDateMonth));
					DateTime tmpDate = new DateTime(newDateYear, newDateMonth, dayOfMonth, tmpNextRunDate.Hour, tmpNextRunDate.Minute, tmpNextRunDate.Second);
					elapsedDays = tmpDate.Subtract(tmpNextRunDate).Days;
					
				}
				else
					elapsedDays = dayOfMonth - tmpNextRunDate.Day;

				if (dayOfMonth != tmpNextRunDate.Day)
				{
					if (DailyFrequenceMinutesInterval || DailyFrequenceHoursInterval)
						addElapsedDays = true;
					else
						tmpNextRunDate = tmpNextRunDate.AddDays(elapsedDays); // somma n giorni interi
				}
			}
			else if (Monthly2Recurring)
			{
				if (frequencyInterval == 0)
					frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Monthly2Sunday;
				
				tmpNextRunDate = GetNextValidMonthly2Date(tmpNextRunDate);
			}

			// Calcolo l'ora in base alla frequenza giornaliera:
			if (DailyFrequenceMinutesInterval || DailyFrequenceHoursInterval)
			{
				int startTimeMinutes = activeStartDate.Hour * 60 + activeStartDate.Minute;
				int endTimeMinutes = activeEndDate.Hour * 60 + activeEndDate.Minute;

				DateTime tmpEndRunDate = new DateTime(tmpNextRunDate.Year, tmpNextRunDate.Month, tmpNextRunDate.Day);

				if (startTimeMinutes >= endTimeMinutes)
					tmpEndRunDate = tmpEndRunDate.AddDays(1);

				tmpEndRunDate = tmpEndRunDate.AddMinutes(endTimeMinutes);

				int frequencyMinutes = DailyFrequenceHoursInterval ? frequencySubinterval * 60 : frequencySubinterval;
				int minutes = tmpNextRunDate.Hour * 60 + tmpNextRunDate.Minute + frequencyMinutes;

				DateTime tmpNextRunDateTry = new DateTime(tmpNextRunDate.Year, tmpNextRunDate.Month, tmpNextRunDate.Day);
				tmpNextRunDateTry = tmpNextRunDateTry.AddMinutes(minutes);

				if (addElapsedDays || tmpNextRunDateTry > tmpEndRunDate)
				{
					tmpNextRunDate = tmpNextRunDate.AddDays(elapsedDays); // somma n giorni interi
					minutes = startTimeMinutes;
				}

				tmpNextRunDate = new DateTime(tmpNextRunDate.Year, tmpNextRunDate.Month, tmpNextRunDate.Day);
				tmpNextRunDate = tmpNextRunDate.AddMinutes(minutes);
			}
			else if(DailyFrequenceOnce)
			{
				tmpNextRunDate = new DateTime(tmpNextRunDate.Year, tmpNextRunDate.Month, tmpNextRunDate.Day, activeStartDate.Hour, activeStartDate.Minute, 0);
				if (tmpNextRunDate < DateTime.Now)
					tmpNextRunDate = tmpNextRunDate.AddDays(elapsedDays); // somma n giorni interi
			}

            if (activeEndDate < DateTime.MaxValue && tmpNextRunDate > activeEndDate)
            {
                aRunDate = DateTime.MaxValue;   // BugFix 19122
                return false;
            }
            
			aRunDate = tmpNextRunDate;
			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private DateTime GetNextValidMonthly2Date(DateTime aStartingDate)
		{
			int day = 1;

			if(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Day)
			{
				if (frequencyRelativeInterval == FrequencyRelativeIntervalTypeEnum.Monthly2Last)
				{
					day = CultureInfo.CurrentUICulture.Calendar.GetDaysInMonth(aStartingDate.Year, aStartingDate.Month);
				}
				else
				{
					int orderShift = 0;
					while ((0x00000001 & ((int)frequencyRelativeInterval >> orderShift++)) == 0)
						day++;
				}
			}
			else
			{
				if (frequencyRelativeInterval == FrequencyRelativeIntervalTypeEnum.Monthly2Last)
				{
					DateTime tmpDate = new DateTime(aStartingDate.Year, aStartingDate.Month, CultureInfo.CurrentUICulture.Calendar.GetDaysInMonth(aStartingDate.Year, aStartingDate.Month), 0, 0, 0); // in tmpDate c'è l'ultimo giorno del mese
						
					// Adesso, se non va bene l'ultimo giorno del mese, vado indietro con i giorni finchè non trovo quello buono.
					while 
						(
						(frequencyInterval < (int)FrequencyRecurringIntervalTypeEnum.Monthly2Day && (frequencyInterval != ((int)tmpDate.DayOfWeek + 1))) ||
						(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2DayOfWeek && (tmpDate.DayOfWeek == DayOfWeek.Sunday || tmpDate.DayOfWeek == DayOfWeek.Saturday)) ||
						(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2WeekendDay && (tmpDate.DayOfWeek != DayOfWeek.Sunday && tmpDate.DayOfWeek != DayOfWeek.Saturday))
						)
					{
						tmpDate = tmpDate.AddDays(-1);
					}
					day = tmpDate.Day;
				}
				else
				{
					DateTime tmpDate = new DateTime(aStartingDate.Year, aStartingDate.Month, 1, 0, 0, 0);
					while 
						(
						(frequencyInterval < (int)FrequencyRecurringIntervalTypeEnum.Monthly2Day && (frequencyInterval != ((int)tmpDate.DayOfWeek + 1))) ||
						(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2DayOfWeek && (tmpDate.DayOfWeek == DayOfWeek.Sunday || tmpDate.DayOfWeek == DayOfWeek.Saturday)) ||
						(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2WeekendDay && (tmpDate.DayOfWeek != DayOfWeek.Sunday && tmpDate.DayOfWeek != DayOfWeek.Saturday))
						)
					{
						tmpDate = tmpDate.AddDays(1);
					}

					int orderShift = 0;
					while ((0x00000001 & ((int)frequencyRelativeInterval >> orderShift++)) == 0)
					{
						if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2WeekendDay)
						{
							if (tmpDate.DayOfWeek == DayOfWeek.Saturday)
								tmpDate = tmpDate.AddDays(1);
							else if (tmpDate.DayOfWeek == DayOfWeek.Sunday)
								tmpDate = tmpDate.AddDays(6);
						}
						if (frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2DayOfWeek)
						{
							if (tmpDate.DayOfWeek == DayOfWeek.Friday)
								tmpDate = tmpDate.AddDays(3);
							else 
								tmpDate = tmpDate.AddDays(1);
						}
						else	
							tmpDate = tmpDate.AddDays(7);
					}
					day = tmpDate.Day;
				}
			}
			
			DateTime nextValidMonthly2Date = new DateTime(aStartingDate.Year, aStartingDate.Month, day, aStartingDate.Hour, aStartingDate.Minute, aStartingDate.Second);
			if (aStartingDate < nextValidMonthly2Date)
				return nextValidMonthly2Date;

			// Scatto di un numero di mesi pari a frequencyRecurringFactor
			int newDateYear  = aStartingDate.Year;
			int newDateMonth = aStartingDate.Month + frequencyRecurringFactor;
			if (newDateMonth > 12)
			{
				newDateMonth = (aStartingDate.Month + frequencyRecurringFactor) % 12;
				newDateYear  += (aStartingDate.Month + frequencyRecurringFactor) / 12;
			}

			return GetNextValidMonthly2Date(new DateTime(newDateYear, newDateMonth, 1, 0, 0, 0));
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool IsValidMonthly2Date(DateTime aDate)
		{
			if(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2Day)
			{
				if (frequencyRelativeInterval == FrequencyRelativeIntervalTypeEnum.Monthly2Last)
					return (aDate.Day == CultureInfo.CurrentUICulture.Calendar.GetDaysInMonth(aDate.Year, aDate.Month));

				int day = 1;			
				int orderDayShift = 0;
				while ((0x00000001 & ((int)frequencyRelativeInterval >> orderDayShift++)) == 0)
					day++;

				return aDate.Day == day;
			}

			if (frequencyRelativeInterval == FrequencyRelativeIntervalTypeEnum.Monthly2Last)
			{
				DateTime tmpLastDate = new DateTime(aDate.Year, aDate.Month, CultureInfo.CurrentUICulture.Calendar.GetDaysInMonth(aDate.Year, aDate.Month), 0, 0, 0); // in tmpLastDate c'è l'ultimo giorno del mese
				while 
					(
					(frequencyInterval < (int)FrequencyRecurringIntervalTypeEnum.Monthly2Day && (frequencyInterval != ((int)tmpLastDate.DayOfWeek + 1))) ||
					(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2DayOfWeek && (tmpLastDate.DayOfWeek == DayOfWeek.Sunday || tmpLastDate.DayOfWeek == DayOfWeek.Saturday)) ||
					(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2WeekendDay && (tmpLastDate.DayOfWeek != DayOfWeek.Sunday && tmpLastDate.DayOfWeek != DayOfWeek.Saturday))
					)
				{
					tmpLastDate = tmpLastDate.AddDays(-1);
				}
				return (aDate.Day == tmpLastDate.Day);
			}

			DateTime tmpDate = new DateTime(aDate.Year, aDate.Month, 1, 0, 0, 0);
			while 
				(
				(frequencyInterval < (int)FrequencyRecurringIntervalTypeEnum.Monthly2Day && (frequencyInterval != ((int)tmpDate.DayOfWeek + 1))) ||
				(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2DayOfWeek && (tmpDate.DayOfWeek == DayOfWeek.Sunday || tmpDate.DayOfWeek == DayOfWeek.Saturday)) ||
				(frequencyInterval == (int)FrequencyRecurringIntervalTypeEnum.Monthly2WeekendDay && (tmpDate.DayOfWeek != DayOfWeek.Sunday && tmpDate.DayOfWeek != DayOfWeek.Saturday))
				)
			{
				tmpDate = tmpDate.AddDays(1);
			}
				
			int orderShift = 0;
			while ((0x00000001 & ((int)frequencyRelativeInterval >> orderShift++)) == 0)
				tmpDate = tmpDate.AddDays(7);

			return (aDate.Day == tmpDate.Day);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SaveCompletitionLevel(SqlConnection connection, CompletitionLevelEnum completitionLevel)
		{
			lastRunCompletitionLevel = completitionLevel;
			
			if (connection != null && (connection.State & ConnectionState.Open) == ConnectionState.Open)
                Update(connection, false);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool SendMailNotificationsUsingSMTP(SqlConnection currentConnection, EventLog schedulerEventLog)
		{
			if (notificationRecipients == null || notificationRecipients.Count == 0)
				return true;

			try 
			{
				string mailText;
				// HTML Body (remove HTML tags for plain text).
				if (LastRunSuccessfull)
					mailText = String.Format(TaskSchedulerObjectsStrings.TaskSuccessfullyExecutedNotificationMessageFormat, Code, LastRunDate.Date.ToString("dd-MM-yyyy"), LastRunDate.TimeOfDay.ToString());
				else if (LastRunFailed)
					mailText = String.Format(TaskSchedulerObjectsStrings.TaskExecutionFailureNotificationMessageFormat, Code, LastRunDate.Date.ToString("dd-MM-yyyy"), LastRunDate.TimeOfDay.ToString());
				else
					return true;

				string  aSMTPRelayServerName = String.Empty;
			    bool    aSMTPUseDefaultCredentials = true;
				bool    aSMTPUseSSL = false;
                Int32   aSMTPPort = 25; 
                string  aSMTPUserName = string.Empty;
                string  aSMTPPassword = string.Empty;
                string  aSMTPDomain = string.Empty;
				string	aSMTPFromAddress = "Scheduler@TaskBuilder.Net";
	            if (currentConnection != null && (currentConnection.State & ConnectionState.Open) == ConnectionState.Open)
				{
                    IBasePathFinder aPathFinder = CreateTaskPathFinder(currentConnection);
					IServerConnectionInfo serverConnectionInfo = (aPathFinder != null) ? InstallationData.ServerConnectionInfo : null;
                    if (serverConnectionInfo != null)
                    {
                        aSMTPRelayServerName = serverConnectionInfo.SMTPRelayServerName;
                        aSMTPUseDefaultCredentials = serverConnectionInfo.SMTPUseDefaultCredentials;
						aSMTPUseSSL = serverConnectionInfo.SMTPUseSSL;
                        aSMTPPort = serverConnectionInfo.SMTPPort;
                        aSMTPUserName = serverConnectionInfo.SMTPUserName;
                        aSMTPPassword = serverConnectionInfo.SMTPPassword;
                        aSMTPDomain = serverConnectionInfo.SMTPDomain;
						aSMTPFromAddress = serverConnectionInfo.SMTPFromAddress;
                    }
                }

				foreach (WTETaskNotificationRecipient notificationRecipient in notificationRecipients)
				{
					if
						(
						(notificationRecipient.IsToNotifyOnSuccess && LastRunSuccessfull)||
						(notificationRecipient.IsToNotifyOnFailure && LastRunFailed)
						)
					{
                        SendSmtpMail
                            (
                            notificationRecipient.Recipient,
							TaskSchedulerObjectsStrings.TaskExecutionNotificationSubject, 
                            mailText, 
                            aSMTPRelayServerName,
                            aSMTPUseDefaultCredentials,
							aSMTPUseSSL,
                            aSMTPPort,
                            aSMTPUserName,
                            aSMTPPassword,
                            aSMTPDomain,
							aSMTPFromAddress,
                            schedulerEventLog
                            );
					}
				}
				return true;
			}			
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WTEScheduledTask.SendMailNotificationsUsingSMTP: " + exception.Message);
				return false;
			}
		}	
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool SendMailNotificationsUsingMAPI()
		{
			if (notificationRecipients == null || notificationRecipients.Count == 0)
				return true;

			try 
			{
				string mailText;
				// HTML Body (remove HTML tags for plain text).
				if (LastRunSuccessfull)
					mailText = String.Format(TaskSchedulerObjectsStrings.TaskSuccessfullyExecutedNotificationMessageFormat, Code, LastRunDate.Date.ToString("dd-MM-yyyy"), LastRunDate.TimeOfDay.ToString());
				else if (LastRunFailed)
					mailText = String.Format(TaskSchedulerObjectsStrings.TaskExecutionFailureNotificationMessageFormat, Code, LastRunDate.Date.ToString("dd-MM-yyyy"), LastRunDate.TimeOfDay.ToString());
				else
					return true;

				using (WTESimpleMAPIWrapper simpleMAPI = new WTESimpleMAPIWrapper())
				{
					if (!simpleMAPI.Logon(IntPtr.Zero, false))
						return false;

					foreach (WTETaskNotificationRecipient notificationRecipient in notificationRecipients)
					{
						if
							(
							(notificationRecipient.IsToNotifyOnSuccess && LastRunSuccessfull) ||
							(notificationRecipient.IsToNotifyOnFailure && LastRunFailed)
							)
						{
							simpleMAPI.AddRecipient(notificationRecipient.Recipient);
						}
					}
					bool returnCode = simpleMAPI.Send(TaskSchedulerObjectsStrings.TaskExecutionNotificationSubject, mailText);
					simpleMAPI.Logoff();

					return returnCode;
				}
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WTEScheduledTask.SendMailNotificationsUsingMAPI: " + exception.Message);
				return false;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void LoadTasksInSequence(string connectionString)
		{
			if (connectionString == null || connectionString.Length == 0)
			{
				Debug.Fail("WTEScheduledTask.LoadTasksInSequence Error: null or empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}
			
			if (!IsSequence)
			{
				Debug.Fail("WTEScheduledTask.LoadTasksInSequence Error.");
				tasksInSequence = null;
				return;
			}
			
			if (tasksInSequence == null)
				tasksInSequence = new WTETasksInScheduledSequenceCollection();
			
			tasksInSequence.Clear();

            SqlDataReader selectDataReader = ScheduledSequencesEngine.LoadTasksInSequence(connectionString, Id);
					
			while(selectDataReader.Read())
			{
				AddTaskInSequence(
					connectionString,
					(Guid)selectDataReader[ScheduledSequencesEngine.TaskIdColumnName],
					Convert.ToInt32(selectDataReader[ScheduledSequencesEngine.TaskIndexColumnName].ToString()),
					Convert.ToBoolean(selectDataReader[ScheduledSequencesEngine.BlockingModeColumnName].ToString())
					);	
			}
			
		}
		
		
	
		//--------------------------------------------------------------------------------------------------------------------------------
		private string GetXmlParametersDirectory(PathFinder aPathFinder)
		{
			if (aPathFinder == null)
				return String.Empty;

			string xmlParametersDirectory = aPathFinder.GetCustomCompanyPath();
			if (xmlParametersDirectory == null || xmlParametersDirectory.Length == 0)
				return String.Empty;
			
			// <CustomCompanyPath>\ScheduledTasks\Parameters\<user>
			xmlParametersDirectory += Path.DirectorySeparatorChar;
			xmlParametersDirectory += "ScheduledTasks";
			xmlParametersDirectory += Path.DirectorySeparatorChar;
			xmlParametersDirectory += "Parameters";
			xmlParametersDirectory += Path.DirectorySeparatorChar;
			xmlParametersDirectory += aPathFinder.User;
			
			return xmlParametersDirectory;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private PathFinder CreateTaskPathFinder(string aUser, string aCompany)
		{
			PathFinder aPathFinder = new PathFinder(aCompany, aUser);
			return aPathFinder;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private PathFinder CreateTaskPathFinder(SqlConnection currentConnection)
		{
			if (currentConnection == null || (currentConnection.State & ConnectionState.Open) != ConnectionState.Open)
				return null;

			// Ricavo l'utente, la sua password e l'azienda dalle informazioni contenute nel record relativo al task
			string company;
			string user;
			if (!GetLoginData(currentConnection, out company, out user))
				return null;

			return CreateTaskPathFinder(user, company);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string EncryptImpersonationPassword()
		{
			if (impersonationPassword == null || impersonationPassword.Length == 0)
				return String.Empty;

			string stringToCrypt = impersonationPassword;
			string postFix = "*" + stringToCrypt.Length.ToString();
			if (stringToCrypt.Length + postFix.Length <= impersonationPasswordMaximumLength)
				stringToCrypt += postFix;
			
			ReverseString(ref stringToCrypt);
			
			return CryptPassword(stringToCrypt.ToCharArray());
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private string DecryptPassword(string passwordToDecrypt)
		{
			if (passwordToDecrypt == null || passwordToDecrypt.Length == 0)
				return String.Empty;

			string decryptedString = CryptPassword(passwordToDecrypt.ToCharArray());
			
			ReverseString(ref decryptedString);

			int lastAsteriskIndex = decryptedString.LastIndexOf('*');
			try
			{
				if (lastAsteriskIndex > 0)
				{
					int realLength = Convert.ToInt32(decryptedString.Substring(lastAsteriskIndex + 1));
				
					return decryptedString.Substring(0, realLength);
				}
			}
			catch(FormatException)
			{
			}
			catch(OverflowException)
			{
			}
			return decryptedString;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private string GetCryptKey()
		{
			string idString = Id.ToString();
			StringBuilder key = new StringBuilder(CyclicOriginalTaskCode.Length + idString.Length, CyclicOriginalTaskCode.Length + idString.Length);

			int j = 0;
			int k = 0;
			for(int i = 0; i < CyclicOriginalTaskCode.Length + idString.Length - 1; i+=2)
			{
				if (j == CyclicOriginalTaskCode.Length)
					j = 0;
				key.Insert(i, CyclicOriginalTaskCode[j++]);
				if (k == idString.Length)
					k = 0;
				key.Insert(i + 1,idString[k++]);
			}

			return key.ToString();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void ReverseString(ref string aStringToReverse)
		{
			if(aStringToReverse == null || aStringToReverse.Length == 0)
				return;

			char[] reversedArray = aStringToReverse.ToCharArray();

			for(int i = 0; i < aStringToReverse.Length; i++)
			{
				int j = aStringToReverse.Length - 1 - i;
				if (i >= j)
					break;
				reversedArray[i] = aStringToReverse[j];
				reversedArray[j] = aStringToReverse[i];
			}

			StringBuilder reversedString = new StringBuilder(aStringToReverse.Length, aStringToReverse.Length);
			reversedString.Append(reversedArray);
			aStringToReverse = reversedString.ToString();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private string CryptPassword(char[] aStringToCrypt)
		{
			if(aStringToCrypt == null || aStringToCrypt.Length == 0)
				return String.Empty;

			StringBuilder box1 = new StringBuilder(256, 256);
			int i = 0;
			for(i = 0; i < 256; i++)
				box1.Insert(i,(char)i);

			string key = GetCryptKey();
			StringBuilder box2 = new StringBuilder(256, 256);
			int j = 0;
			int k = 0;
			for(j = 0; j < 256; j++)
			{
				if (k == key.Length)
					k = 0;
				box2.Insert(j,key[k++]);
			}    

			j = 0 ;
			for(i = 0; i < 256; i++)
			{
				j = (int)((j + (uint)box1[i] + (uint)box2[i]) % 256);
				char temp = box1[i];                    
				box1[i] = box1[j];
				box1[j] = temp;
			}

			j = 0;
			StringBuilder cryptResult = new StringBuilder(aStringToCrypt.Length, aStringToCrypt.Length);
			for(k = 0; k < aStringToCrypt.Length; k++)
			{
				i = (i + 1) % 256;
				j = (int)((j + (uint)box1[i]) % 256);
				
				char temp = box1[i];
				box1[i] = box1[j] ;
				box1[j] = temp;

				int randomByte = box1[(int)(((uint) box1[i] + (uint) box1[j]) %  256)];

				//xor with the data and done
				cryptResult.Insert(k, (char)((int)aStringToCrypt[k] ^ randomByte));
			}    
			return cryptResult.ToString();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private string GetXmlParametersFilename()
		{
			return Code + NameSolverStrings.XmlExtension;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private string GetXmlParametersFullFilename(PathFinder aPathFinder)
		{
			if (aPathFinder == null)
				return String.Empty;

			string filename = GetXmlParametersDirectory(aPathFinder);
			
			if (filename != null && filename.Length > 0)
			{
				// <CustomCompanyPath>\WTEScheduledTasks\Parameters\<user>\<task_code>.xml
				filename += Path.DirectorySeparatorChar;
				filename += GetXmlParametersFilename();
			}
			
			return filename;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool LoadCommandParametersFromFile(SqlConnection currentConnection)
		{
			xmlParameters = String.Empty;

			if (!HasXmlParameters || currentConnection == null || (currentConnection.State & ConnectionState.Open) != ConnectionState.Open)
				return false;

			PathFinder aPathFinder = CreateTaskPathFinder(currentConnection);

			string xmlParametersFilename = GetXmlParametersFullFilename(aPathFinder);
			if (xmlParametersFilename == null || xmlParametersFilename.Length == 0)
				return false;

			if (!File.Exists(xmlParametersFilename))
				return true;

			try
			{
				//Create the XmlDocument.
				XmlDocument doc = new XmlDocument();

				// Save the document to a file. White space is preserved (no white space).
				doc.PreserveWhitespace = true;

				doc.Load(xmlParametersFilename);

				xmlParameters = doc.DocumentElement.OuterXml;
			}
			catch (XmlException exception)
			{
				throw new WTEScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.LoadCommandParametersFromFileFailedMsg, Code, CompanyId, LoginId, xmlParametersFilename), exception);
			}

			return true;
        }
		
        //---------------------------------------------------------------------
        public bool Insert(SqlConnection connection, WTEScheduledTaskObj obj )
        {
            if (id == Guid.Empty)
                id = Guid.NewGuid();

            if (!WTEScheduledTask.Insert(connection, obj))
                return false;

            try
            {
                SaveCommandParametersToFile(connection);
            }
            catch (ScheduledTaskException exception)
            {
                Debug.Fail("Exception raised in ScheduledTask.Insert: " + exception.Message);
                throw exception;
            }

            if (IsSequence && !SaveTasksInSequence(connection))
                return false;

            TaskNotificationRecipientEngine.SaveMailNotificationsSettings(connection, id, notificationRecipients);

            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool Update(SqlConnection connection, bool refreshAllTaskInfo)
        {
            if (refreshAllTaskInfo)
                InitNextRunDate();

            WTEScheduledTask.Update(connection, this);

            if (refreshAllTaskInfo)
            {
                try
                {
                    SaveCommandParametersToFile(connection);
                }
                catch (ScheduledTaskException exception)
                {
                    throw exception;
                }

                if (IsSequence && !SaveTasksInSequence(connection))
                    return false;

                TaskNotificationRecipientEngine.SaveMailNotificationsSettings(connection, this.id, notificationRecipients);
            }
            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SaveTasksInSequence(SqlConnection connection)
        {
            if (!IsSequence)
            {
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSequenceOperationRequestMsg);
            }

            if (!ScheduledSequencesEngine.DeleteAllTaskByid(connection, Id))
                return false;
            if (tasksInSequence == null || tasksInSequence.Count <= 0)
                return true;

            tasksInSequence.ReorderByTaskIndex();

            if (!ScheduledSequencesEngine.InsertAllTaskInSequence(connection, tasksInSequence, Id))
                return true;

            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public bool SaveCommandParametersToFile(SqlConnection currentConnection)
		{
			if (!HasXmlParameters || currentConnection == null || (currentConnection.State & ConnectionState.Open) != ConnectionState.Open)
				return false;

			PathFinder aPathFinder = CreateTaskPathFinder(currentConnection);

			try
			{
				if (xmlParameters == null || xmlParameters.Length == 0)
				{
					string xmlParametersFilename = GetXmlParametersFullFilename(aPathFinder);
					if (File.Exists(xmlParametersFilename))
						File.Delete(xmlParametersFilename);
					return true;
				}
				
				string xmlParametersDirectory = GetXmlParametersDirectory(aPathFinder);
				if (xmlParametersDirectory == null || xmlParametersDirectory.Length == 0)
					return false;

				if (!Directory.Exists(xmlParametersDirectory))
					Directory.CreateDirectory(xmlParametersDirectory);

				//Create the XmlDocument.
				XmlDocument doc = new XmlDocument();

				// Save the document to a file. White space is preserved (no white space).
				doc.PreserveWhitespace = true;

				doc.LoadXml(xmlParameters);

				doc.Save(GetXmlParametersFullFilename(aPathFinder));
				
				XmlTextWriter writer = new XmlTextWriter(GetXmlParametersFullFilename(aPathFinder), System.Text.Encoding.UTF8);
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 1;
				writer.IndentChar = '\t';
				writer.QuoteChar = '"';

				writer.WriteStartDocument();

				doc.WriteContentTo(writer);

				writer.Flush();
				writer.Close();
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WTEScheduledTask.SaveCommandParametersToFile: " + exception.Message);
				throw new WTEScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.SaveCommandParametersToFileFailedMsg, Code, CompanyId, LoginId, GetXmlParametersFullFilename(aPathFinder)), exception);
			}

			return true;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void DeleteRelatedFiles(SqlConnection connection)
		{
			PathFinder aPathFinder = CreateTaskPathFinder(connection);
			if (aPathFinder == null)
				return;
			
			DeleteXmlParametersFile(aPathFinder);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void DeleteXmlParametersFile(PathFinder aPathFinder)
		{
			if (aPathFinder == null)
				return;
			
			string xmlParametersFilename = GetXmlParametersFullFilename(aPathFinder);
			if (xmlParametersFilename != null && xmlParametersFilename.Length > 0 && File.Exists(xmlParametersFilename))
				File.Delete(xmlParametersFilename);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private System.Diagnostics.Process ExecuteCommandAsProcess()
		{
			if (command == null || command.Length == 0)
				return null;

			bool result = false;
			System.Diagnostics.Process exeProcess = new System.Diagnostics.Process();
				
			try
			{
				// If it is not to close, do not receive an event when the process exits.
				exeProcess.EnableRaisingEvents = CloseOnEnd;
				if (CloseOnEnd)
					exeProcess.Exited += new System.EventHandler(ExecutableTask_Exited);
				
				string[] recipients = command.Split(' ');
				bool bCmd = false;
				foreach (string recipient in recipients)
				{
					if (recipient == null || recipient.Length == 0)
						continue;

					if (!bCmd)
					{
						bCmd = true;
						exeProcess.StartInfo.FileName = recipient;
					}
					else
						exeProcess.StartInfo.Arguments = exeProcess.StartInfo.Arguments + ' ' + recipient;
				}

				result = exeProcess.Start();
			}
			catch (Exception exception)
			{
				throw new WTEScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.ProcessExecutionFailedMsgFmt, command, id, code, companyId, loginId), exception);
			}

			return result ? exeProcess : null;

		}

		//----------------------------------------------------------------------------
		private void ExecutableTask_Exited(object sender, System.EventArgs e)
		{
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool UpdateApplicationDate(TbLoaderClientInterface currentTBLoaderClientInterface)
		{
			if 
				(
				!enabled ||
				IsSequence ||
				!TBLoaderConnectionNecessaryForRun ||
				!SetApplicationDateBeforeRun
				)
				return true;

			try
			{
				if 
					(
					currentTBLoaderClientInterface == null || 
					!currentTBLoaderClientInterface.IsConnectionActive() ||
					currentTBLoaderClientInterface.GetNrOpenDocuments() > 0					
					)
					return false;
			
				//@@TODO Sostituire SetApplicationDate con SetApplicationDateToSystemDate
				currentTBLoaderClientInterface.SetApplicationDate(DateTime.Now);
			
				return true;
			}
			catch(Exception exception)
			{
				Debug.Fail("WTEScheduledTask.UpdateApplicationDate Error: " + exception.Message);

				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		// Devo necessariamente passare come argomento alla funzione Run sia la connessione, sia la stringa di connessione
		// originale: se si passasse solo la connessione poi, nel caso in cui sia prevista una password, avrei dei problemi
		// ad ottenere da essa la stringa di connessione corretta e, quindi, a riutilizzare tale stringa per effettuare
		// nuove connessioni. Infatti, la proprietà ConnectionString di SqlConnection "taglia" questa informazione (a meno
		// che la stringa usata per connettersi contenga anche "persist security info=True")
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool Run
						(
							TbLoaderClientInterface currentTBLoaderClientInterface, 
							LoginManager			loginManager,
							string					connectionString, 
							SqlConnection			connection, 
							bool					saveStartRunStatus, 
							ManualResetEvent		executionEndedEvent, 
							EventLog				schedulerEventLog
						)
		{
			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WTEScheduledTask.Run Error: empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}

			if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
			{
				Debug.Fail("WTEScheduledTask.Run Error: invalid connection.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSqlConnectionErrMsg);
			}

			if 
				(
				(
				TBLoaderConnectionNecessaryForRun && 
				(currentTBLoaderClientInterface == null || !currentTBLoaderClientInterface.IsConnectionActive())
				) ||
				!enabled
				)
				return false;
			
			//if (IsRunning) // the task is already running
			//	return false;
			//

			if (saveStartRunStatus)
				SaveStartRunStatus(connection);

			if (IsSequence)
			{
				if (tasksInSequence == null || tasksInSequence.Count <= 0)
					return true;

				tasksInSequence.ReorderByTaskIndex();
			
				bool sequenceSuccess = true;
				// Adesso eseguo i task che compongono la sequenza
				foreach (WTETaskInScheduledSequence taskInSequence in tasksInSequence)
				{
					WTEScheduledTaskObj taskToRun = new WTEScheduledTaskObj(taskInSequence.TaskInSequenceId, connectionString, true);
					
					// bugfix: 19723
					// in task one element not enabled, jump this
 
					if (!taskToRun.Enabled)
						continue;
					
					taskInSequence.ResetExecutionEndedEvent();
					if
						(
						!taskToRun.Run
						(
						currentTBLoaderClientInterface,
						loginManager,
						connectionString,
						connection,
						true,
						taskInSequence.ExecutionEndedEvent,
						schedulerEventLog
						)
						)
					{
						if (taskInSequence.BlockingMode)
						{
							lastRunCompletitionLevel = CompletitionLevelEnum.SequenceInterrupted;
							break;
						}
						sequenceSuccess = false;
					}

					// Se il task fa parte di una sequenza devo attendere il termine della sua esecuzione
					// in modo che possa partire il task successivo
					taskInSequence.WaitForExecutionEnd();
					
				}
				if (IsRunning)
					lastRunCompletitionLevel = sequenceSuccess ? CompletitionLevelEnum.Success : CompletitionLevelEnum.SequencePartialSuccess;
			}
			else if (RunBatch)
			{
				// La funzione SpawnBatchExecution "innesca l'esecuzione della procedura batch
				return SpawnBatchExecution(currentTBLoaderClientInterface, connectionString, connection, executionEndedEvent, schedulerEventLog);
			}
			else if (RunReport)
			{
				// La funzione SpawnReportExecution "innesca il caricamento del report
				return SpawnReportExecution(currentTBLoaderClientInterface, loginManager, connectionString, connection, executionEndedEvent, schedulerEventLog);
			}
			else if (RunFunction)
			{
				UpdateApplicationDate(currentTBLoaderClientInterface);

				currentTBLoaderClientInterface.RunFunction(Command, this.xmlParameters);
				
				lastRunCompletitionLevel = CompletitionLevelEnum.Success;
			}
			else if (RunDataExport)
			{
				try
				{
					// Se il modulo di XEngine non risulta attivato il task non deve partire
					if (loginManager != null && loginManager.IsActivated(NameSolverStrings.Extensions, "XEngine"))
					{
						UpdateApplicationDate(currentTBLoaderClientInterface);
					
						AsyncCallback beginCallback = new AsyncCallback(WTEScheduledTaskObj .RunFromSchedulerModeCallback);
						
						currentTBLoaderClientInterface.BeginRunXMLExportInUnattendedMode
							(
							Command,
							xmlParameters,
							beginCallback,
							new TaskRunAsyncState(this, currentTBLoaderClientInterface, connectionString, true, RunIconized, executionEndedEvent, schedulerEventLog)
							);
						return true;
					}
					else
					{
						if (schedulerEventLog != null)
							schedulerEventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.XEngineNotActivatedMsgFmt, Code, CompanyId, LoginId), EventLogEntryType.Error);
						
						lastRunCompletitionLevel = CompletitionLevelEnum.Failure;
					}
				}
				catch(Exception exception)
				{
					// Se non sono previsti ulteriori tentativi, devo comunque segnalare il termine
					// dell'esecuzione del task.
					if (executionEndedEvent != null && (retryAttempts <= retryAttemptsActualCount))
						executionEndedEvent.Set();

					throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
				}
			}
			else if (RunDataImport)
			{
				try
				{
					// Se il modulo di XEngine non risulta attivato il task non deve partire
					if (loginManager != null && loginManager.IsActivated(NameSolverStrings.Extensions, "XEngine"))
					{
						UpdateApplicationDate(currentTBLoaderClientInterface);
						
						AsyncCallback beginCallback = new AsyncCallback(WTEScheduledTaskObj.RunFromSchedulerModeCallback);
						
						currentTBLoaderClientInterface.BeginRunXMLImportInUnattendedMode
							(
							Command,
							false,
                            ValidateData,
                            xmlParameters,
							beginCallback,
							new TaskRunAsyncState(this, currentTBLoaderClientInterface, connectionString, true, RunIconized, executionEndedEvent, schedulerEventLog)
							);
						return true;
					}
					else
					{
						if (schedulerEventLog != null)
							schedulerEventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.XEngineNotActivatedMsgFmt, Code, CompanyId, LoginId), EventLogEntryType.Error);
						
						lastRunCompletitionLevel = CompletitionLevelEnum.Failure;
					}
				}
				catch(Exception exception)
				{
					// Se non sono previsti ulteriori tentativi, devo comunque segnalare il termine
					// dell'esecuzione del task.
					if (executionEndedEvent != null && (retryAttempts <= retryAttemptsActualCount))
						executionEndedEvent.Set();

					throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
				}
			}
			else if (RunExecutable)
			{
				System.Diagnostics.Process process = ExecuteCommandAsProcess();
				if (process != null && process.Responding)
				{
					if (WaitForProcessHasExited)
						process.WaitForExit();

					lastRunCompletitionLevel = CompletitionLevelEnum.Success;
				}
				else
					lastRunCompletitionLevel = CompletitionLevelEnum.Failure;
				
			}
			else if (OpenWebPage)
			{
				if (OpenWebPageInBrowser(command, OpenWebPageInInternetExplorer, CreateNewBrowserInstance))
					lastRunCompletitionLevel = CompletitionLevelEnum.Success;
				else
					lastRunCompletitionLevel = CompletitionLevelEnum.Failure;
			}
			else if (SendMessage)
			{
				RunSendMessage(schedulerEventLog);
			}
			else if (SendMail)
			{
				RunSendMail(schedulerEventLog, connection);
			}		
			else if (DeleteRunnedReports)
			{
				RunReportHistoryDeletion(connectionString, schedulerEventLog);
			}			
			//else if (BackupCompanyDB)
			//{
			//	RunBackupCompanyDB(connectionString, schedulerEventLog);
			//}			
			//else if (RestoreCompanyDB)
			//{
			//	RunRestoreCompanyDB(connectionString, schedulerEventLog);
			//}			
			//else 
				lastRunCompletitionLevel = CompletitionLevelEnum.Failure;

			EndRunningState(connectionString, executionEndedEvent, null);

			return LastRunSuccessfull;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		// La funzione SpawnBatchExecution "innesca l'esecuzione della procedura batch in
		// modo asincrono; quando termina l'esecuzione della batch la proxy-class di TbLoader 
		// invoca la funzione di call-back WTEScheduledTask.RunFromSchedulerModeCallback. Questa
		// funzione di call-back riconosce se la batch è stata eseguita con successo o meno e
		// imposta di conseguenza il livello di completamento del task in oggetto.
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool SpawnBatchExecution
			(
			TbLoaderClientInterface currentTBLoaderClientInterface, 
			string					connectionString, 
			SqlConnection			connection, 
			ManualResetEvent		executionEndedEvent, 
			EventLog				schedulerEventLog
			)
		{
			if (!RunBatch)
				return false;

			try
			{
				UpdateApplicationDate(currentTBLoaderClientInterface);

				AsyncCallback beginCallback = new AsyncCallback(WTEScheduledTaskObj.RunFromSchedulerModeCallback);
				
				currentTBLoaderClientInterface.BeginRunBatchInUnattendedMode
					(
					Command,
					xmlParameters,
					beginCallback,
					new TaskRunAsyncState(this, currentTBLoaderClientInterface, connectionString, CloseOnEnd, RunIconized, executionEndedEvent, schedulerEventLog)
					);
				return true;
			}
			catch(Exception exception)
			{
				// Se non sono previsti ulteriori tentativi, devo comunque segnalare il termine
				// dell'esecuzione del task.
				if (executionEndedEvent != null && (retryAttempts <= retryAttemptsActualCount))
					executionEndedEvent.Set();

				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		// La funzione SpawnReportExecution "innesca il caricamento di un report in modo
		// asincrono; quando termina l'esecuzione del report la proxy-class di TbLoader 
		// invoca la funzione di call-back WTEScheduledTask.RunFromSchedulerModeCallback. Questa
		// funzione di call-back riconosce se il report è stata eseguito con successo o meno e
		// imposta di conseguenza il livello di completamento del task in oggetto.
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool SpawnReportExecution
			(
			TbLoaderClientInterface currentTBLoaderClientInterface, 
			LoginManager			loginManager,
			string					connectionString, 
			SqlConnection			connection, 
			ManualResetEvent		executionEndedEvent, 
			EventLog				schedulerEventLog
			)
		{
			if (!RunReport || currentTBLoaderClientInterface == null || !currentTBLoaderClientInterface.IsConnectionActive())
				return false;

			try
			{
				UpdateApplicationDate(currentTBLoaderClientInterface);
					
				WoormInfo woormInfo = currentTBLoaderClientInterface.CreateWoormInfo(Command);
				woormInfo.SetAutoPrint(PrintReport);

				if (loginManager != null && loginManager.IsActivated(NameSolverStrings.Extensions, "MailConnector"))
				{
					if (SendReportAsWTEMailAttachment)
					{
						// Se il campo messageContent è vuoto significa che non è stato specificato
						// alcun destinatario di mail con il report in allegato e quindi non serve
						// specificare le opzioni relative al MailConnector
						if (ReportSendingRecipients != null && ReportSendingRecipients.Length > 0)
						{
                            woormInfo.SetAttachPDF(SendReportAsPDFWTEMailAttachment);
                            woormInfo.SetConcatPDF(ConcatReportPDFFiles);

                            woormInfo.SetAttachRDE(SendReportAsRDEWTEMailAttachment);
                            woormInfo.SetExcelOutput(SendReportAsExcelWTEMailAttachment);

                            woormInfo.SetCompressAttach(SendReportAsCompressedWTEMailAttachment);

                            woormInfo.SetMailTo(ReportSendingRecipients);

                            woormInfo.SetMailSubject(description);

                            woormInfo.SetSendEmail(true);

                            woormInfo.SetCloseOnEndPrint(true);
                        }
					}
					else if (SaveReportAsFile)
					{
						// Se il campo messageContent è vuoto significa che non è stato specificato
						// il nome del file sul quale va salvato il report e quindi non serve
						// specificare le opzioni di salvataggio
						if (ReportSavingFileName != null && ReportSavingFileName.Length > 0)
						{
                            woormInfo.SetAttachPDF(SendReportAsPDFWTEMailAttachment);
                            woormInfo.SetConcatPDF(ConcatReportPDFFiles);

                            woormInfo.SetPDFOutput(SaveReportAsPDFFile);
                            woormInfo.SetRDEOutput(SaveReportAsRDEFile);
                            woormInfo.SetExcelOutput(SaveReportAsExcelFile);
                            woormInfo.AddOutputFileName(ReportSavingFileName);

                            woormInfo.SetCompressAttach(SendReportAsCompressedWTEMailAttachment);

                            woormInfo.SetCloseOnEndPrint(true);
                        }
					}
				}

				AsyncCallback beginCallback = new AsyncCallback(WTEScheduledTaskObj.RunFromSchedulerModeCallback);
					
				currentTBLoaderClientInterface.BeginRunReportInUnattendedMode
					(
					woormInfo,
					xmlParameters,
					beginCallback,
					new TaskRunAsyncState(this, currentTBLoaderClientInterface, connectionString, CloseOnEnd, RunIconized ,executionEndedEvent, schedulerEventLog, woormInfo)
					);
				
				return true;
			}
			catch(Exception exception)
			{
				// Se non sono previsti ulteriori tentativi, devo comunque segnalare il termine
				// dell'esecuzione del task.
				if (executionEndedEvent != null && (retryAttempts <= retryAttemptsActualCount))
					executionEndedEvent.Set();

				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool RunSendMessage(EventLog schedulerEventLog)
		{
			if (!SendMessage)
				return false;
			
			bool sentToAllrecipients = true;
			
			// Se il campo command è vuoto significa che non è stato specificato
			// alcun destinatario del messaggio e quindi il task non deve far nulla 
			if (command != null && command.Length > 0)
			{
				string[] recipients = command.Split(';');
				if (recipients == null || recipients.Length == 0)
				{
					lastRunCompletitionLevel = CompletitionLevelEnum.Success;

					return true;
				}

				if (recipients != null && recipients.Length > 0)
				{
					try
					{
						foreach(string recipient in recipients)
						{
							if (recipient == null || recipient.Length == 0)
								continue;

							WTENetSendMessage msgToSend = new WTENetSendMessage(recipient, messageContent);

							if (!msgToSend.Send())
								sentToAllrecipients = false;
						}
					}
					catch(WTEScheduledTaskException exception)
					{
						if (schedulerEventLog != null)
							schedulerEventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.SendMessageFailedMsgFmt, Code, exception.ExtendedMessage), EventLogEntryType.Error);
					
						lastRunCompletitionLevel = CompletitionLevelEnum.Failure;

						return false;
					}
				}

			}

			lastRunCompletitionLevel = sentToAllrecipients ? CompletitionLevelEnum.Success :CompletitionLevelEnum.Failure;

			return sentToAllrecipients;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool RunSendMail(EventLog schedulerEventLog, SqlConnection currentConnection)
		{
			if (!SendMail)
				return false;

			// Se il campo command è vuoto significa che non è stato specificato
			// alcun destinatario della mail e quindi il task non deve far nulla 
			if (command == null || command.Length == 0)
			{
				lastRunCompletitionLevel = CompletitionLevelEnum.Success;
				return true;
			}
				
			if (!SendMailUsingSMTP)
			{
				try
				{
					using (WTESimpleMAPIWrapper simpleMAPI = new WTESimpleMAPIWrapper())
					{
						if (!simpleMAPI.Logon(IntPtr.Zero, false))
						{
							lastRunCompletitionLevel = CompletitionLevelEnum.Failure;

							if (schedulerEventLog != null)
								schedulerEventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.SendMailUsingSMTPFailureMsg, code, companyId, loginId), EventLogEntryType.Error);

							return false;
						}

						string[] recipientsArray = command.Split(';');
						if (recipientsArray != null && recipientsArray.Length > 0)
						{
							foreach (string recipient in recipientsArray)
							{
								if (recipient == null || recipient.Length == 0)
									continue;

								simpleMAPI.AddRecipient(recipient);
							}
						}
						bool mailSent = simpleMAPI.Send(description, messageContent);

						simpleMAPI.Logoff();

						lastRunCompletitionLevel = mailSent ? CompletitionLevelEnum.Success : CompletitionLevelEnum.Failure;

						return true;
					}
				}
				catch (Exception exception)
				{				
					lastRunCompletitionLevel = CompletitionLevelEnum.Failure;

					if (schedulerEventLog != null)
						schedulerEventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.SendMailFailureMsg, code, companyId, loginId, exception.Message), EventLogEntryType.Error);

					return false;
				}
			}

			string[] recipients = command.Split(';');
			if (recipients == null || recipients.Length == 0)
			{
				lastRunCompletitionLevel = CompletitionLevelEnum.Success;

				return true;
			}
				
			try
			{
                string aSMTPRelayServerName = String.Empty;
                bool aSMTPUseDefaultCredentials = true;
				bool aSMTPUseSSL = false;
                Int32 aSMTPPort = 25;
                string aSMTPUserName = string.Empty;
                string aSMTPPassword = string.Empty;
                string aSMTPDomain = string.Empty;
				string aSMTPFromAddress = "Scheduler@TaskBuilder.Net";
				if (currentConnection != null && (currentConnection.State & ConnectionState.Open) == ConnectionState.Open)
                {
                    IBasePathFinder aPathFinder = CreateTaskPathFinder(currentConnection);
					IServerConnectionInfo serverConnectionInfo = (aPathFinder != null) ? InstallationData.ServerConnectionInfo : null;
                    if (serverConnectionInfo != null)
                    {
                        aSMTPRelayServerName = serverConnectionInfo.SMTPRelayServerName;
                        aSMTPUseDefaultCredentials = serverConnectionInfo.SMTPUseDefaultCredentials;
						aSMTPUseSSL = serverConnectionInfo.SMTPUseSSL;
                        aSMTPPort = serverConnectionInfo.SMTPPort;
                        aSMTPUserName = serverConnectionInfo.SMTPUserName;
                        aSMTPPassword = serverConnectionInfo.SMTPPassword;
                        aSMTPDomain = serverConnectionInfo.SMTPDomain;
						aSMTPFromAddress = serverConnectionInfo.SMTPFromAddress;
					}
                }

                bool allSent = true;
                foreach (string recipient in recipients)
				{
                    if (String.IsNullOrEmpty(recipient))
                        continue;

                    if (!SendSmtpMail
                        (
                            recipient,
                            description,
                            messageContent,
                            aSMTPRelayServerName,
                            aSMTPUseDefaultCredentials,
							aSMTPUseSSL,
                            aSMTPPort,
                            aSMTPUserName,
                            aSMTPPassword,
                            aSMTPDomain,
							aSMTPFromAddress,
                            schedulerEventLog
                        )
                        )
                    {
                        allSent = false;
                        break;
                    }
                }

                lastRunCompletitionLevel = allSent ? CompletitionLevelEnum.Success : CompletitionLevelEnum.Failure;

				return true;
			}
			catch (Exception exception)
			{
				lastRunCompletitionLevel = CompletitionLevelEnum.Failure;

				if (schedulerEventLog != null)
					schedulerEventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.SendMailFailureMsg, code, companyId, loginId, exception.Message), EventLogEntryType.Error);

				return false;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool RunReportHistoryDeletion(string connectionString, EventLog schedulerEventLog)
		{
			if (!DeleteRunnedReports)
				return false;
			
			if (connectionString == null || connectionString.Length == 0)
			{
				Debug.Fail("WTEScheduledTask.RunReportHistoryDeletion Error: null or empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}
			
			try
			{
				string company;
				string user;
				
				if (!GetLoginData(connectionString, out company, out user))
				{
					lastRunCompletitionLevel = CompletitionLevelEnum.Failure;

					if (schedulerEventLog != null)
						schedulerEventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.TaskLoginDataNotFoundMsg, code, companyId, loginId), EventLogEntryType.Error);

					return false;
				}
				
				RunnedReportMng runnedReportManager = new RunnedReportMng();
				runnedReportManager.DeleteOldRunnedReports(company, user);
	
				lastRunCompletitionLevel = CompletitionLevelEnum.Success;

				return true;
			}
			catch(Exception exception)
			{
				lastRunCompletitionLevel = CompletitionLevelEnum.Failure;

				if (schedulerEventLog != null)
					schedulerEventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.ReportHistoryDeletionFailedMsg, code, companyId, loginId, exception.Message), EventLogEntryType.Error);
						
				return false;
			}
		}


		//--------------------------------------------------------------------------------------------------------------------------------
		private bool EndRunningStateWithSuccess(string connectionString, ManualResetEvent executionEndedEvent, EventLog eventLog)
		{	
			lastRunCompletitionLevel = CompletitionLevelEnum.Success;

			return EndRunningState(connectionString, executionEndedEvent, eventLog);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool EndRunningStateWithFailure(string connectionString, ManualResetEvent executionEndedEvent, EventLog eventLog)
		{	
			lastRunCompletitionLevel = CompletitionLevelEnum.Failure;

			return EndRunningState(connectionString, executionEndedEvent, eventLog);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool EndRunningState(string connectionString, ManualResetEvent executionEndedEvent, EventLog eventLog)
		{	
			if (connectionString == null || connectionString.Length == 0)
			{
				Debug.Fail("WTEScheduledTask.EndRunningState Error: null or empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}
			
			SqlConnection connection = null;

			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();

				// Creazione degli eventuali task temporanei, "clonati" dal task corrente, 
				// qualora questi sia da ripetere ciclicamente
				if (IsCyclicToRepeat)
				{
					if (BuildCurrentCyclicRepeatTask(connection))
					{
						if (Temporary)
						{
							// Devo aggiornare il task originale (quello di partenza, "clonato" ciclicamente)
							WTEScheduledTaskObj originalTask = new WTEScheduledTaskObj(CyclicOriginalTaskCode, CompanyId, LoginId, connectionString, false);
							if (!originalTask.CyclicRepeatRunning)
							{
								originalTask.CyclicRepeatRunning = true;
                                Update(connection, false);

                            }
						}
						else
							CyclicRepeatRunning = true;
					}
					else if (eventLog != null)
						eventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.CyclicRepeatFailedMsgFmt, Code, CompanyId, LoginId), EventLogEntryType.Error);
				}

				RefreshNextRunDate(LastRunSuccessfull);
					
				if (connection != null && (connection.State & ConnectionState.Open) == ConnectionState.Open)
                    WTEScheduledTask.Update(connection, this);

                 if (!this.SendMailNotifications(connection, eventLog) && eventLog != null)
                      eventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.SendMailNotificationErrorMsg, Code, CompanyId, LoginId), EventLogEntryType.Error);

                return true;
			}
			catch(Exception exception)
			{
				Debug.Fail("Error in WTEScheduledTask.EndRunningState: " + exception.Message);
				if (eventLog != null)
					eventLog.WriteEntry(exception.Message, EventLogEntryType.Error);

				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}

				// Devo segnalare il termine dell'esecuzione del task.
				// Se chi lancia il task deve attenderne il completamento, ad es. se esso fa parte
				// di una sequenza, devo segnalare il termine della sua esecuzione.
				// Nel caso di una sequenza di task, così so che può partire il task successivo
				if (executionEndedEvent != null && (retryAttempts <= retryAttemptsActualCount))
					executionEndedEvent.Set();
			}
		}

		#endregion

		#region WTEScheduledTask public methods

		/// <summary>
		/// Imposta l'handle della finestra attuale come handle da utilizzare qualora venga 
		/// istanziato un tbloader privo di menu (ad esempio la finestra parametri della schedulazione
		/// dell'esportazione)
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetWindowHandle(IntPtr handle)
		{
			taskWindowHandle = handle;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			id							= Guid.Empty;
			code						= String.Empty;
			companyId					= 0;
			loginId						= 0;	
			command						= String.Empty;
			description					= String.Empty;
			type						= TaskTypeEnum.Batch;
			enabled						= true;
			frequencyType				= FrequencyTypeEnum.RecurringWeekly;
			frequencySubtype			= FrequencySubtypeEnum.DailyOnce;
			frequencyInterval			= 1;
			frequencySubinterval		= 0;
			frequencyRelativeInterval	= FrequencyRelativeIntervalTypeEnum.Undefined;
			frequencyRecurringFactor	= 1;

			activeStartDate	= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
			activeEndDate	= DateTime.MaxValue;

			lastRunDate = MinimumDate; 
			lastRunRetries = MinimumDate; 
			nextRunDate = DateTime.MaxValue; 
			retryAttempts = 0;
			retryDelay = 0;
			retryAttemptsActualCount = 0;
			lastRunCompletitionLevel = CompletitionLevelEnum.Undefined;
			sendMailUsingSMTP = true;
			cyclicRepeat	= 0;
			cyclicDelay		= 0;
			cyclicTaskCode	= String.Empty;
			impersonationDomain = String.Empty;
			impersonationUser = String.Empty;
			impersonationPassword = String.Empty;
			messageContent = String.Empty;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetNewId()
		{
			id = Guid.NewGuid();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool SetCode(string aCode, SqlConnection connection)
		{
			if (aCode == null || aCode.Length == 0)
				return false;

			if (!Temporary && !WTEScheduledTask.IsValidTaskCode(aCode, this.LoginId, this.CompanyId, connection))
				return false;
			
			code = aCode;
			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool SetCode(string aCode, string connectionString)
		{
			if (aCode == null || aCode.Length == 0)
				return false;

			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WTEScheduledTask.SetCode Error: empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}
			bool ok = false;
			
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();

				ok = SetCode(aCode, connection);
			}
			catch(SqlException exception)
			{
				throw new WTEScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, Code), exception);
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
			return ok;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetDataRowFields(ref System.Data.DataRow aTaskTableRow)
		{
			if (aTaskTableRow == null || String.Compare(aTaskTableRow.Table.TableName, WTEScheduledTask.ScheduledTasksTableName, true, CultureInfo.InvariantCulture) != 0)
				return;
			
			aTaskTableRow[WTEScheduledTask.IdColumnName] = Id;
			aTaskTableRow[WTEScheduledTask.CodeColumnName] = Code;
			aTaskTableRow[WTEScheduledTask.CompanyIdColumnName] = CompanyId;
			aTaskTableRow[WTEScheduledTask.LoginIdColumnName] = LoginId;
			aTaskTableRow[WTEScheduledTask.ConfigurationColumnName] = "";
			aTaskTableRow[WTEScheduledTask.TypeColumnName] = Type;
			aTaskTableRow[WTEScheduledTask.RunningOptionsColumnName] = RunningOptions;
			aTaskTableRow[WTEScheduledTask.EnabledColumnName] = Enabled;
			aTaskTableRow[WTEScheduledTask.CommandColumnName] = Command;
			aTaskTableRow[WTEScheduledTask.DescriptionColumnName] = Description;
			aTaskTableRow[WTEScheduledTask.FrequencyTypeColumnName] = FrequencyType;
			aTaskTableRow[WTEScheduledTask.FrequencySubtypeColumnName] = FrequencySubtype;

			aTaskTableRow[WTEScheduledTask.FrequencyIntervalColumnName] = FrequencyInterval;
			aTaskTableRow[WTEScheduledTask.FrequencySubintervalColumnName] = FrequencySubinterval;
			aTaskTableRow[WTEScheduledTask.FrequencyRelativeIntervalColumnName] = FrequencyRelativeInterval;
			aTaskTableRow[WTEScheduledTask.FrequencyRecurringFactorColumnName] = FrequencyRecurringFactor;

			aTaskTableRow[WTEScheduledTask.ActiveStartDateColumnName] = ActiveStartDate;
			aTaskTableRow[WTEScheduledTask.ActiveEndDateColumnName] = ActiveEndDate;

			aTaskTableRow[WTEScheduledTask.LastRunDateColumnName] = LastRunDate; 
			aTaskTableRow[WTEScheduledTask.LastRunRetriesColumnName] = LastRunRetries; 
			aTaskTableRow[WTEScheduledTask.NextRunDateColumnName] = NextRunDate; 

			aTaskTableRow[WTEScheduledTask.RetryAttemptsColumnName] = RetryAttempts;
			aTaskTableRow[WTEScheduledTask.RetryDelayColumnName] = RetryDelay;
			aTaskTableRow[WTEScheduledTask.RetryAttemptsActualCountColumnName] = RetryAttemptsActualCount;
			aTaskTableRow[WTEScheduledTask.LastRunCompletitionLevelColumnName] = LastRunCompletitionLevel;
			aTaskTableRow[WTEScheduledTask.SendMailUsingSMTPColumnName] = SendMailUsingSMTP;
			aTaskTableRow[WTEScheduledTask.CyclicRepeatColumnName] = CyclicRepeat;
			aTaskTableRow[WTEScheduledTask.CyclicDelayColumnName] = CyclicDelay;
			aTaskTableRow[WTEScheduledTask.CyclicTaskCodeColumnName] = CyclicTaskCode;

			aTaskTableRow[WTEScheduledTask.ImpersonationDomainColumnName] = ImpersonationDomain;
			aTaskTableRow[WTEScheduledTask.ImpersonationUserColumnName] = ImpersonationUser;
			aTaskTableRow[WTEScheduledTask.ImpersonationPasswordColumnName] = EncryptImpersonationPassword();
		
			aTaskTableRow[WTEScheduledTask.MessageContentColumnName] = messageContent;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void RefreshData(string connectionString, bool onlyUI)
		{
			if (connectionString == null || connectionString.Length == 0)
			{
				Debug.Fail("WTEScheduledTask.RefreshData Error: null or empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}
			
			if (!WTEScheduledTaskObj.IsValidTaskId(id))
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InvalidTaskIdErrMsg);

			SqlConnection connection = null;
			SqlDataReader taskDataReader = null;

			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();

				taskDataReader = WTEScheduledTask.GetTaskDataById(id, connection);

				if (taskDataReader == null)
					throw new WTEScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskNotFoundMsgFmt, id));

				FillFromTaskDataReader(taskDataReader, connectionString, onlyUI);
			}
			catch(Exception exception)
			{
				throw new WTEScheduledTaskException(exception.Message);
			}
			finally
			{
				if (taskDataReader != null && !taskDataReader.IsClosed)
					taskDataReader.Close();

				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RefreshData(string connectionString)
		{
			RefreshData(connectionString, false);
		}
		
		//-----------------------------------------------------------------------------
		public void InitNextRunDate()
		{
			nextRunDate = DateTime.MaxValue;
			if (ToRunOnDemand)
				return;

			if (ToRunOnce)
			{
				nextRunDate = activeStartDate;
				return;
			}
			// imposto correttamente la data con il primo giorno di esecuzione a partire dall'inizio di durata
			DateTime tmpNextRunDate = ActiveStartDate;

			FindNearestNextRunDate(ref tmpNextRunDate);

			nextRunDate = tmpNextRunDate;
		}

		//-----------------------------------------------------------------------------
		public void RefreshNextRunDate(bool lastRunSuccessfully)
		{
			if (nextRunDate > DateTime.Now)
				return;
				
			if (!lastRunSuccessfully && retryAttempts > retryAttemptsActualCount)
			{
				if (retryAttemptsActualCount == 0 && (ToRunOnce || ToRunOnDemand))
					nextRunDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
				nextRunDate = nextRunDate.AddMinutes(retryDelay);
				retryAttemptsActualCount ++;
				return;
			}
			retryAttemptsActualCount = 0;

			if (ToRunOnce || ToRunOnDemand)
			{
				nextRunDate = DateTime.MaxValue;
				return;
			}

			// Ora come ora nel campo nextRunDate si dovrebbe trovare la data di esecuzione
			// corrente (RefreshNextRunDate va appunto chiamata appena dopo il lancio 
			// del task...)
			IncrementRunDate(ref nextRunDate);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void CopyRecurringModeSettings(WTEScheduledTaskObj aTask)
		{
			if (aTask == null)
				return;

			frequencyType				= (FrequencyTypeEnum)aTask.FrequencyType;
			frequencySubtype			= (FrequencySubtypeEnum)aTask.FrequencySubtype;
			frequencyInterval			= aTask.FrequencyInterval;
			frequencySubinterval		= aTask.FrequencySubinterval;
			frequencyRelativeInterval	= (FrequencyRelativeIntervalTypeEnum)aTask.FrequencyRelativeInterval;
			frequencyRecurringFactor	= aTask.FrequencyRecurringFactor;
			activeStartDate				= aTask.ActiveStartDate;
			activeEndDate				= aTask.ActiveEndDate;
		}
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetWeeklyRecurringDays(bool onSunday, bool onMonday, bool onTuesday, bool onWednesday, bool onThursday, bool onFriday, bool onSaturday)
		{
			frequencyInterval = (int)FrequencyRecurringIntervalTypeEnum.Undefined;

			if (onSunday)
				WeeklyRecurringOnSunday = true;
			if (onMonday)
				WeeklyRecurringOnMonday = true;
			if (onTuesday)
				WeeklyRecurringOnTuesday = true;
			if (onWednesday)
				WeeklyRecurringOnWednesday = true;
			if (onThursday)
				WeeklyRecurringOnThursday = true;
			if (onFriday)
				WeeklyRecurringOnFriday = true;
			if (onSaturday)
				WeeklyRecurringOnSaturday = true;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		// L'esecuzione di un task può avvenire in seguito a due diverse situazioni: o è lanciata
		// dal servizio RunWTEScheduledTasksService quando "scatta la sua ora" oppure interattivamente
		// dalla MicroareaConsole qualora si tratti di un task su richiesta.
		// Nel primo caso il task va subito aggiornato allo stato di "running", prima della sua vera
		// e propria esecuzione (che può essere preceduta dall'eventuale istanziazione dell'interfaccia
		// con TB), di modo che il task non venga lanciato più volte dal servizio stesso. Infatti, se 
		// il portare a termine tutte le operazioni di lancio (v.l'istanziazione del TB Loader) necessita
		// di un tempo superiore all'intervallo del timer, la procedura timerizzata del servizio
		// RunWTEScheduledTasksService potrebbe ritrovare ancora il task tra quelli da eseguire.
		// Pertanto, in questo caso, occorre chiamare prima la SaveStartRunStatus, poi effettuare le
		// varie operazioni preliminari necessarie al lancio e, infine, chiamare la Run.
		// Se, invece, il task viene schedulato "su richiesta" ed eseguito da MicroareaConsole, ciò non
		// è necessario.
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SaveStartRunStatus(SqlConnection connection)
		{
			// Aggiorno la data di ultima esecuzione con la data corrente
			lastRunDate = DateTime.Now;

			RefreshNextRunDate(true);

			SaveCompletitionLevel(connection, CompletitionLevelEnum.Running);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SaveFailureStatus(SqlConnection connection)
		{
			RefreshNextRunDate(false);

			SaveCompletitionLevel(connection, (retryAttempts <= retryAttemptsActualCount) ? CompletitionLevelEnum.Failure : CompletitionLevelEnum.WaitForNextRetryAttempt);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SaveSuccesStatus(SqlConnection connection)
		{
			RefreshNextRunDate(false);

			SaveCompletitionLevel(connection, CompletitionLevelEnum.Success);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Run
					(
						TbLoaderClientInterface currentTBLoaderClientInterface,
						LoginManager			loginManager,
						string					connectionString,
						ManualResetEvent		executionEndedEvent, 
						EventLog				schedulerEventLog
			)
		{
			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WTEScheduledTask.Run Error: empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}
	
			SqlConnection connection = null;
	
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();
	
				return Run
					(
					currentTBLoaderClientInterface, 
					loginManager,
					connectionString, 
					connection, 
					false, 
					executionEndedEvent, 
					schedulerEventLog
					);
			}
			catch(SqlException exception)
			{
				throw new WTEScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, Code), exception);
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool SendMailNotifications(SqlConnection currentConnection, EventLog schedulerEventLog)
		{
			if (notificationRecipients == null || notificationRecipients.Count == 0)
				return true;
			if (SendMailUsingSMTP)
                return SendMailNotificationsUsingSMTP(currentConnection, schedulerEventLog);
			
			return SendMailNotificationsUsingMAPI();
		}

		//ritorna la pwd per la login degli utenti scheduler
		//---------------------------------------------------------------------------
		private string GetLoginMngSession(IBasePathFinder aPathFinder)
		{
			string pwd = string.Empty;
			string loginSessionFile = aPathFinder.GetLoginMngSessionFile();
			if (File.Exists(loginSessionFile))
			{
				try
				{
					StreamReader sr = new StreamReader(loginSessionFile);
					pwd = sr.ReadToEnd();
					sr.Close();
				}
				catch
				{
					pwd = string.Empty;
				}
			}

			if (pwd == null || pwd.Length == 0)
				return string.Empty;

			char[] c = pwd.ToCharArray();
			Array.Reverse(c);
			return new string(c);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public LoginManager Login(IBasePathFinder aPathFinder, SqlConnection currentConnection, bool unattended)
		{
			if (currentConnection == null || (currentConnection.State & ConnectionState.Open) != ConnectionState.Open)
				return null;
	
			LoginManager loginManager = null;
			
			try
			{	
				// Ricavo l'utente, la sua password e l'azienda dalle informazioni contenute nel record relativo al task
				string taskCompany;
				string taskUser;
				string taskPassword;
				bool   windowsAuthentication;

				if (!GetLoginData(currentConnection, out taskCompany, out taskUser, out taskPassword, out windowsAuthentication))
					return null;

				if (aPathFinder == null)
					aPathFinder = CreateTaskPathFinder(taskUser, taskCompany);

                if (windowsAuthentication)
                {
                    taskPassword = string.Empty;
                    string loginSessionFile = aPathFinder.GetLoginMngSessionFile();
                    if (File.Exists(loginSessionFile))
                    {
                        try
                        {
                            StreamReader sr = new StreamReader(loginSessionFile);
                            taskPassword = sr.ReadToEnd();
                            sr.Close();
                        }
                        catch
                        {
                            taskPassword = string.Empty;
                        }
                    }
                }
                else
                {
                    taskPassword = Crypto.Decrypt(taskPassword);
                }

				loginManager = new LoginManager(aPathFinder.LoginManagerUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut);

				// Il metodo loginManager.ValidateUser(...) deve essere chiamato per ogni utente prima di 
				// effettuare la login vera e propria.
				if (loginManager.ValidateUser(taskUser, taskPassword, windowsAuthentication) != (int)LoginReturnCodes.NoError)
					return null;

				loginManager.Password = GetLoginMngSession(aPathFinder);

				if
					(
					loginManager.Login
					(
					taskCompany, 
					unattended ? ProcessType.SchedulerAgent : ProcessType.SchedulerManager, 
					true
					)!= 0
					)
					return null;
			}
			catch(Exception exception)
			{
				Debug.Fail("WTEScheduledTask.Login Error.", exception.Message);

				if (loginManager != null)
					loginManager.LogOff();

				return null;
			}
			return loginManager;
		}

		//-----------------------------------------------------------------------------------
		public TbLoaderClientInterface EstablishTBConnection(LoginManager loginManager, bool unattendedMode, bool inProc)
		{
			try
			{
				string launcher = Process.GetCurrentProcess().ProcessName;
				if (loginManager == null || loginManager.UserName == null || loginManager.UserName.Length == 0)
					throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InsufficientLoginDataMsg);

				TbLoaderClientInterface aTBLoaderClientInterface = inProc?
					new TbApplicationClientInterface
					(
					launcher,
					new PathFinder(loginManager.CompanyName, loginManager.UserName),
					loginManager.AuthenticationToken
					)
					: new TbLoaderClientInterface
					(
					new PathFinder(loginManager.CompanyName, loginManager.UserName),
					launcher,
					loginManager.AuthenticationToken
					);
				if (inProc)
					aTBLoaderClientInterface.MenuHandle = taskWindowHandle;

				aTBLoaderClientInterface.TbServer = Environment.MachineName;
				aTBLoaderClientInterface.StartTbLoader(launcher, unattendedMode);
				
				aTBLoaderClientInterface.InitTbLogin();

				return aTBLoaderClientInterface;
			}
			catch(TbLoaderClientInterfaceException e)
			{
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TBLoaderConnectionFailedMsg, e);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool GetCommandParameters(TbLoaderClientInterface aTBLoaderClientInterface)
		{
			string[] resultMessages = null;
			bool result = false;
			Thread t = new Thread(() =>
			{
				try
				{
					// Apertura del documento batch o il report in modalità di
					// impostazione parametri (o regole di richiesta nel caso di report)
					if (RunBatch)
						result = aTBLoaderClientInterface.GetDocumentParameters(Command, ref xmlParameters, code);
					else if (RunReport)
						result = aTBLoaderClientInterface.GetReportParameters(Command, ref xmlParameters, Code);
					else if (RunDataExport)
						result = aTBLoaderClientInterface.GetXMLExportParameters(Command, ref xmlParameters, ref resultMessages, Code);
					else if (RunDataImport)
						result = aTBLoaderClientInterface.GetXMLImportParameters(Command, ref xmlParameters, ref resultMessages, Code);
                }
				catch(Exception ex)
				{
					resultMessages = new string[1];
					resultMessages[0] = ex.ToString();
				}
			}
			);
			
			t.Start();
			

			while (!t.Join(1))
				Application.DoEvents();

			if (resultMessages != null && resultMessages.Length > 0)
			{
				StringBuilder excMessage = new StringBuilder();
				foreach (String msg in resultMessages)
					excMessage.AppendFormat(msg + Environment.NewLine);
				throw new WTEScheduledTaskException(excMessage.ToString());
			}

			return result;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool GetCommandParameters(IBasePathFinder aPathFinder, string connectionString)
		{
			if (!HasXmlParameters)
				return true;

			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WTEScheduledTask.GetCommandParameters Error: empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}

			if (Command == null || Command == string.Empty)
			{
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyCommandMsg);
			}

			SqlConnection currentConnection = null;
			LoginManager loginManager = null;
			TbLoaderClientInterface aTBLoaderClientInterface = null;
            bool ok = false;

			try
			{
				currentConnection = new SqlConnection(connectionString);
				currentConnection.Open();

				loginManager = Login(aPathFinder, currentConnection, false);
				
				currentConnection.Close();

				if (loginManager == null)
					return false;

				aTBLoaderClientInterface = EstablishTBConnection(loginManager, false, true);

				if (!aTBLoaderClientInterface.Logged)
				{
					throw new WTEScheduledTaskException(aTBLoaderClientInterface.GetGlobalDiagnostic(true).ToString());
				}

				ok = GetCommandParameters(aTBLoaderClientInterface);
				
			}
			catch(Exception exception)
			{
				throw new WTEScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.GetCommandParametersFailedMsg, Command, Code, CompanyId, LoginId), exception);
			}
			finally
			{
				if (loginManager != null)
					loginManager.LogOff();

				if (currentConnection != null)
				{
					if ((currentConnection.State & ConnectionState.Open) == ConnectionState.Open)
						currentConnection.Close();
					currentConnection.Dispose();
				}

				if (aTBLoaderClientInterface != null 
					&& aTBLoaderClientInterface.Logged
					&& aTBLoaderClientInterface.CanCloseLogin())
				{
					aTBLoaderClientInterface.CloseLoginAndExternalProcess();
				}			
			}
			
			return ok;
		}

		//-------------------------------------------------------------------------------------------
		public void AddNotificationRecipient(WTETaskNotificationRecipient aRecipient)
		{
			notificationRecipients.Add(aRecipient);
		}

		//-------------------------------------------------------------------------------------------
		public WTETaskNotificationRecipient GetNotificationRecipientAt(int index)
		{
			return notificationRecipients.Item(index);
		}

		//-------------------------------------------------------------------------------------------
		public void RemoveNotificationRecipient(int index)
		{
			notificationRecipients.Remove(index);
		}

		//-------------------------------------------------------------------------------------------
		public void ClearNotificationRecipientsList()
		{
			notificationRecipients.Clear();
		}

		//-------------------------------------------------------------------------------------------
		public bool AddTaskInSequence(string connectionString, Guid aTaskId, int aTaskInSequenceIndex, bool aBlockingMode)
		{
			if (connectionString == null || connectionString.Length == 0)
			{
				Debug.Fail("WTEScheduledTask.AddTaskInSequence Error: null or empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}
			
			
			if (!IsSequence)
			{
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSequenceOperationRequestMsg);
			}

			if (aTaskId == Guid.Empty)
			{
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.UndefinedTaskIdMsg);
			}
			
			if (tasksInSequence == null)
				tasksInSequence = new WTETasksInScheduledSequenceCollection();

			WTEScheduledTaskObj task = WTEScheduledTask.GetTaskById(aTaskId, connectionString);
			if (task == null)
				return false;

			tasksInSequence.Add(
				new WTETaskInScheduledSequence(
				this,
				aTaskId,
				aTaskInSequenceIndex,
				aBlockingMode,
				task.TBLoaderConnectionNecessaryForRun,
				task.CloseOnEnd
				)
				);	
			return true;
		}

		//-------------------------------------------------------------------------------------------
		public WTETaskInScheduledSequence GetTaskInSequenceAt(int index)
		{
			if (!IsSequence)
			{
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSequenceOperationRequestMsg);
			}

			if (tasksInSequence == null)
				return null;

			return tasksInSequence[index];
		}

		//-------------------------------------------------------------------------------------------
		public void RemoveTaskInSequence(int index)
		{
			if (!IsSequence)
			{
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSequenceOperationRequestMsg);
			}

			if (tasksInSequence == null)
				return;

			tasksInSequence.Remove(index);
		}

		//-------------------------------------------------------------------------------------------
		public void ClearTasksInSequenceList()
		{
			if (tasksInSequence == null)
				return;

			tasksInSequence.Clear();

			if (!IsSequence)
				tasksInSequence = null;
		}

		//-------------------------------------------------------------------------------------------
		public bool GetLoginData(string connectionString, out string company, out string user, out bool isCompanyDisabled)
		{
			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WTEScheduledTask.GetLoginData Error: empty connection string.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}

			SqlConnection connection = null;

			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();

				return GetLoginData(connection, out company, out user, out isCompanyDisabled);
			}
			catch(SqlException exception)
			{
				throw new WTEScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, Code), exception);
			}
			finally
			{
				if (connection != null)
				{
					if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
					connection.Dispose();
				}
			}
		}

		//-------------------------------------------------------------------------------------------
		public bool GetLoginData(string connectionString, out string company, out string user)
		{
			bool isCompanyDisabled;
			return GetLoginData(connectionString, out company, out user, out isCompanyDisabled);
		}
		
		//-------------------------------------------------------------------------------------------
		public bool GetLoginData(SqlConnection connection, out string company, out string user, out bool isCompanyDisabled)
		{
			return GetLoginDataFromIds(connection, companyId, loginId, out company, out user, out isCompanyDisabled);
		}

		//-------------------------------------------------------------------------------------------
		public bool GetLoginData(SqlConnection connection, out string company, out string user)
		{
			bool isCompanyDisabled;
			return GetLoginData(connection, out company, out user, out isCompanyDisabled);
		}
		
		//-------------------------------------------------------------------------------------------
		public bool GetLoginData(SqlConnection connection, out string company, out string user, out string password, out bool windowsAuthentication)
		{
			return GetLoginDataFromIds(connection, companyId, loginId, out company, out user, out password, out windowsAuthentication);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AppendText (StringBuilder sb, string text)
		{
			//funziona per le grammatiche occidentali, tapullo! In realta' andava evitato di concatenare stringhe
			//in questo modo, ma ormai... an. #10716
			if (sb[sb.Length - 1] != ' ' && text[0] != ',') 
				sb.Append(' ');

			sb.Append(text);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string BuildRecurringModeDescription()
		{
			if (!Recurring)
				return String.Empty;

			StringBuilder recurringModeDescription = new StringBuilder();
			recurringModeDescription.Append(TaskSchedulerObjectsStrings.FrequencyOccursEveryDescription);

			if (DailyRecurring)
			{
				if (FrequencyInterval > 1)
					AppendText(recurringModeDescription, FrequencyInterval.ToString());
				AppendText(recurringModeDescription, (FrequencyInterval > 1) ? TaskSchedulerObjectsStrings.FrequencyDailyPeriodDescription : TaskSchedulerObjectsStrings.FrequencyDayDescription);
			}
			else
			{
				if (FrequencyRecurringFactor > 1)
					AppendText(recurringModeDescription, FrequencyRecurringFactor.ToString());

				if (WeeklyRecurring)
				{
					AppendText(recurringModeDescription, (FrequencyRecurringFactor > 1) ? TaskSchedulerObjectsStrings.FrequencyWeeklyPeriodDescription : TaskSchedulerObjectsStrings.FrequencyWeekDescription);
					
					string daysDescription = String.Empty;
					if (FrequencyInterval == 0)
						daysDescription = TaskSchedulerObjectsStrings.FrequencySundayDescription;
					if (WeeklyRecurringOnSunday)
						daysDescription = TaskSchedulerObjectsStrings.FrequencySundayDescription;
					if (WeeklyRecurringOnMonday)
					{
						if (daysDescription.Length > 0)
							daysDescription += ", ";
						daysDescription += TaskSchedulerObjectsStrings.FrequencyMondayDescription;
					}
					if (WeeklyRecurringOnTuesday)
					{
						if (daysDescription.Length > 0)
							daysDescription += ", ";
						daysDescription += TaskSchedulerObjectsStrings.FrequencyTuesdayDescription;
					}
					if (WeeklyRecurringOnWednesday)
					{
						if (daysDescription.Length > 0)
							daysDescription += ", ";
						daysDescription += TaskSchedulerObjectsStrings.FrequencyWednesdayDescription;
					}
					if (WeeklyRecurringOnThursday)
					{
						if (daysDescription.Length > 0)
							daysDescription += ", ";
						daysDescription += TaskSchedulerObjectsStrings.FrequencyThursdayDescription;
					}
					if (WeeklyRecurringOnFriday)
					{
						if (daysDescription.Length > 0)
							daysDescription += ", ";
						daysDescription += TaskSchedulerObjectsStrings.FrequencyFridayDescription;
					}
					if (WeeklyRecurringOnSaturday)
					{
						if (daysDescription.Length > 0)
							daysDescription += ", ";
						daysDescription += TaskSchedulerObjectsStrings.FrequencySaturdayDescription;
					}
					
					AppendText(recurringModeDescription, daysDescription);
				}
				else if (MonthlyRecurring)
				{
					AppendText(recurringModeDescription, (FrequencyRecurringFactor > 1) ? TaskSchedulerObjectsStrings.FrequencyMonthlyPeriodDescription : TaskSchedulerObjectsStrings.FrequencyMonthDescription);
	
					if (Monthly1Recurring)
					{
						AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyDayOfMonthDescription);
						AppendText(recurringModeDescription, FrequencyInterval.ToString());
					}
					else if (Monthly2Recurring)
					{
						switch (FrequencyRelativeInterval)
						{
							case  (int)FrequencyRelativeIntervalTypeEnum.Monthly2First:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyFirstDescription);
								break;
							case  (int)FrequencyRelativeIntervalTypeEnum.Monthly2Second:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencySecondDescription);
								break;
							case  (int)FrequencyRelativeIntervalTypeEnum.Monthly2Third:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyThirdDescription);
								break;
							case  (int)FrequencyRelativeIntervalTypeEnum.Monthly2Fourth:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyFourthDescription);
								break;
							case  (int)FrequencyRelativeIntervalTypeEnum.Monthly2Last:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyLastDescription);
								break;
							default:
								break;
						}
						switch (FrequencyInterval)
						{
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2Sunday:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencySundayDescription);
								break;
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2Monday:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyMondayDescription);
								break;
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2Tuesday:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyTuesdayDescription);
								break;
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2Wednesday:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyWednesdayDescription);
								break;
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2Thursday:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyThursdayDescription);
								break;
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2Friday:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyFridayDescription);
								break;
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2Saturday:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencySaturdayDescription);
								break;
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2Day:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyDayDescription);
								break;
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2DayOfWeek:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyDayOfWeekDescription);
								break;
							case  (int)FrequencyRecurringIntervalTypeEnum.Monthly2WeekendDay:
								AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyWeekendDayDescription);
								break;
							default:
								break;
						}
					}
				}
			}
			if (DailyFrequenceOnce)
			{
				AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyAtHourDescription);
				AppendText(recurringModeDescription, ActiveStartDate.ToShortTimeString());
			}
			else
			{
				if (DailyFrequenceMinutesInterval)
				{
					if (FrequencySubinterval > 1)
						AppendText(recurringModeDescription, String.Format(TaskSchedulerObjectsStrings.FrequencyMinutesOfDayDescription, FrequencySubinterval));
					else
						AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyEveryMinuteDescription);
				}
				else if (DailyFrequenceHoursInterval)
				{
					if (FrequencySubinterval > 1)
						AppendText(recurringModeDescription, String.Format(TaskSchedulerObjectsStrings.FrequencyHoursOfDayDescription, FrequencySubinterval));
					else
						AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyEveryHourDescription);
				}

				AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyStartAtHourDescription);
				AppendText(recurringModeDescription, ActiveStartDate.ToShortTimeString());

				AppendText(recurringModeDescription, TaskSchedulerObjectsStrings.FrequencyEndAtHourDescription);
				AppendText(recurringModeDescription, ActiveEndDate.ToShortTimeString());
			}

			return recurringModeDescription.ToString();
		}
		
		////--------------------------------------------------------------------------------------------------------------------------------
		//public SqlCommand GetSelectAllTasksOnDemandOrderedByCodeQuery(SqlConnection connection)
		//{
		//	return GetSelectAllTasksOnDemandOrderedByCodeQuery(connection, companyId, loginId, "");
		//}
		
		////--------------------------------------------------------------------------------------------------------------------------------
		//public SqlCommand GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(SqlConnection connection, ScheduledTaskObj typeToSearch)
		//{
		//	return GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(connection, typeToSearch, companyId, loginId, "");
		//}
		
	
		
		//-----------------------------------------------------------------------------
		public bool BuildCurrentCyclicRepeatTask(SqlConnection connection)
		{
			if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
			{
				Debug.Fail("WTEScheduledTask.BuildCurrentCyclicRepeatTask Error: invalid connection.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSqlConnectionErrMsg);
			}

			if (!IsCyclicToRepeat)
				return false;

			// Se si innesca nuovamente l'esecuzione ciclica occorre inizializzare
			// tale esecuzione con la data corrente (infatti BuildCurrentCyclicRepeatTask 
			// va chiamata al termine dell'esecuzione del task) sommando ovviamente
			// ad essa i minuti impostati come intervallo d'attesa tra le esecuzioni
			DateTime nextCyclicRunDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
			nextCyclicRunDate = nextCyclicRunDate.AddMinutes((double)CyclicDelay);
	
			WTEScheduledTaskObj aCyclicRepeatTask = new WTEScheduledTaskObj(this);
			// Cambio l'id del nuovo task clonato in modo che risulti differente dall'originale
			aCyclicRepeatTask.SetNewId(); 
			// "pulisco" lo stato di running
			aCyclicRepeatTask.lastRunCompletitionLevel = CompletitionLevelEnum.Undefined;

			string currentCode = code;
			int repeatCount = 0;
			if (Temporary)
			{
				// Ricavo dal codice corrente del task temporaneo il valore del
				// suo contatore che va incrementato e concatenato al codice del
				// task originale per formare il codice del nuovo task temporaneo
				// che vado a creare
				int temporaryCodeCharIndex = currentCode.LastIndexOf(temporaryCodeChar);
				if (temporaryCodeCharIndex >= 0)
				{
					repeatCount = Convert.ToInt32(currentCode.Substring(temporaryCodeCharIndex + 1));
					currentCode = currentCode.Substring(0, temporaryCodeCharIndex);
				}
				if (repeatCount >= cyclicRepeatMax)
					repeatCount = 0;
			}
			repeatCount++;
			string codePostfix = temporaryCodeChar + repeatCount.ToString();

			// Per comporre correttamente il codice del nuovo task temporaneo vado a
			// prendere il codice originale del task "clonato" ciclicamente
			aCyclicRepeatTask.CyclicTaskCode = CyclicOriginalTaskCode;
		
			aCyclicRepeatTask.ToRunOnce = true;
			aCyclicRepeatTask.Cyclic = true;
			aCyclicRepeatTask.Temporary = true;
            // Il codice va impostato DOPO aver specificato che si tratta di un task temporaneo!!!!
            aCyclicRepeatTask.SetCode(currentCode.Substring(0, Math.Min(taskCodeMaximumLength - codePostfix.Length, currentCode.Length)) + codePostfix, connection.ConnectionString);

			aCyclicRepeatTask.ActiveStartDate = nextCyclicRunDate;
			aCyclicRepeatTask.ActiveStartTime = nextCyclicRunDate.TimeOfDay;

			aCyclicRepeatTask.CyclicRepeat = CyclicRepeat - 1;

			return WTEScheduledTask.Insert(connection, aCyclicRepeatTask);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WTETaskImpersonation ImpersonateWindowsUser()
		{
			if (!IsToImpersonateWindowsUser)
				return null;

			if (
				impersonationDomain == null ||
				impersonationDomain.Length == 0 ||
				impersonationUser == null ||
				impersonationUser.Length == 0
				)
			{
				Debug.Fail("Error in WTEScheduledTask.ImpersonateWindowsUser: insufficient logon data.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.InsufficientImpersonationLogonDataMsg);
			}

			return new WTETaskImpersonation(impersonationDomain, impersonationUser, impersonationPassword);
		}
		
		#endregion
		


		#region WTEScheduledTask static functions

		//-------------------------------------------------------------------------------------------
		private static bool CheckSchedulerAgentEventLog()
		{
			try
			{
				// The Source can be any random string, but the name must be distinct from other
				// sources on the computer. It is common for the source to be the name of the 
				// application or another identifying string. An attempt to create a duplicated 
				// Source value throws an exception. However, a single event log can be associated
				// with multiple sources.
				if (EventLog.SourceExists(WTEScheduledTask.SchedulerAgentEventLogSourceName)) 
				{
					string existingLogName = EventLog.LogNameFromSourceName(WTEScheduledTask.SchedulerAgentEventLogSourceName, ".");
					if (String.Compare(WTEScheduledTask.SchedulerAgentEventLogName, existingLogName, true, CultureInfo.InvariantCulture) == 0)
						return true;

					EventLog.DeleteEventSource(WTEScheduledTask.SchedulerAgentEventLogSourceName);
				}
				EventLog.CreateEventSource(WTEScheduledTask.SchedulerAgentEventLogSourceName, WTEScheduledTask.SchedulerAgentEventLogName);
			
				return true; 
			}
			catch(Exception exception)
			{
				Debug.Fail("EventSource creation failed in WTEScheduledTask.CheckSchedulerAgentEventLog: " + exception.Message);
				return false;
			}
		}
		
	
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool IsValidTaskId(Guid aId)
		{
			//@@TODO
			return (aId != Guid.Empty);
		}
		
		
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool IsValidBackupFilePath(string aFilePath)
		{
			if (aFilePath == null || aFilePath.Length == 0)
				return false;

			try
			{
				// se la directory specificata non esiste non valìdo il path
				FileInfo fi = new FileInfo(aFilePath);
				if (!Directory.Exists(fi.DirectoryName))
					return false;

				return Path.IsPathRooted(aFilePath);
			}
			catch(ArgumentException)
			{
				return false;
			}
			catch(PathTooLongException)
			{
				return false;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public static bool IsValidURI(string aURI)
		{
			try
			{
				// A URI (Uniform Resource Identifier) is an address string referring to an object, typically 
				// on the Internet. The most common type of URI is the URL, in which the address maps onto an 
				// access algorithm using network protocols. Sometimes URI and URL are used interchangeably.
				// A Uniform Resource Name (URN) is also a URI and identifies a property or resource using a 
				// globally unique name. 


				// The Uri class constructor creates a Uri instance from a URI string. 
				// It parses the URI, puts it in canonical format, and makes any required escape encodings.
				Uri uri = new Uri(aURI);
			}
			catch(UriFormatException)
			{
				return false;
			}
			return true;
		}

        //============================================================================
        private class SchedulerSmtpClient : SmtpClient
        {
            private ManualResetEvent sendCompletedEvent = null;
            private Exception sendCompletedLastError = null;

            //----------------------------------------------------------------------------
            public SchedulerSmtpClient(string aSmtpMailSmtpServer)
                :
                base(aSmtpMailSmtpServer)
            {
                sendCompletedEvent = new ManualResetEvent(false);//the initial state is set to nonsignaled
            }
            public ManualResetEvent SendCompletedEvent { get { return sendCompletedEvent; } }
            public Exception SendCompletedLastError { get { return sendCompletedLastError; } set { sendCompletedLastError = value; } }
        }

        //----------------------------------------------------------------------------
        private static void SmtpSendCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (sender != null && sender is SchedulerSmtpClient)
            {
                if (((SchedulerSmtpClient)sender).SendCompletedEvent != null)
                    ((SchedulerSmtpClient)sender).SendCompletedEvent.Set();
                ((SchedulerSmtpClient)sender).SendCompletedLastError = e.Error;
            }
        }
        
        //----------------------------------------------------------------------------
		public static bool SendSmtpMail
            (
            string  recipient, 
            string  messageSubject, 
            string  messageContent, 
            string  aSMTPRelayServerName,
            bool    useDefaultCredentials,
			bool	useSSL,
            Int32   port, // the port number on the SMTP host. The default value is 25
            string  userName,
            string  password,
            string  domain,
			string	fromAddress,	// "Scheduler@TaskBuilder.Net"
            EventLog schedulerEventLog
            )
		{
			if (String.IsNullOrEmpty(recipient))
				return false;

            MailMessage mailTask = new MailMessage(fromAddress, recipient);
            mailTask.Subject = messageSubject;
            mailTask.IsBodyHtml = true;
            mailTask.BodyEncoding = Encoding.UTF8;
			
            if (messageContent != null && messageContent.Length > 0)
            {
                messageContent = messageContent.Replace("\r\n", "<br>");
                messageContent = messageContent.Replace("\n", "<br>");
                mailTask.Body = "<html><body><p>" + messageContent + "</p></body></html>";
            }
            else
                mailTask.Body = String.Empty;

            mailTask.Priority = MailPriority.Normal;

            string smtpMailSmtpServer = "localhost";

            if (aSMTPRelayServerName != null && aSMTPRelayServerName.Trim().Length > 0)
                smtpMailSmtpServer = aSMTPRelayServerName.Trim();

            try
            {
                SchedulerSmtpClient aSmtpClient = new SchedulerSmtpClient(smtpMailSmtpServer);
                aSmtpClient.UseDefaultCredentials = useDefaultCredentials;
				aSmtpClient.EnableSsl = useSSL;
                if (!useDefaultCredentials)
                {
                    aSmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    //se il server è GMail, domain deve essere vuoto
					//probabilmente domain va usato solo se il server di posta è in dominio...
					NetworkCredential netCredential =
                        new NetworkCredential
                        (
                        userName,
                        password,
                        domain  // The domain parameter specifies the domain or realm to which the user name belongs. 
                                // Typically, this is the host computer name where the application runs or the user domain for the
                                // currently logged in user.
                        );
                    // Il metodo CredentialCache.Add(string host, int port, string authenticationType, NetworkCredential credential)
                    // aggiunge un'istanza NetworkCredential da utilizzare con SMTP alla cache delle credenziali e la associa ad un 
                    // computer host, a una porta e a un protocollo di autenticazione. Le credenziali aggiunte con questo metodo 
                    // sono valide solo per SMTP. Questo metodo non funziona per le richieste HTTP o FTP.
                    //
                    CredentialCache netCredentialCache = new CredentialCache();
                    netCredentialCache.Add(smtpMailSmtpServer, port, "NTLM", netCredential);
					aSmtpClient.Credentials = netCredentialCache.GetCredential(smtpMailSmtpServer, port, "NTLM");

                    //aSmtpClient.Credentials = netCredential;
                }
                else
                {
                    aSmtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                }


                aSmtpClient.SendCompleted += new SendCompletedEventHandler(SmtpSendCompletedCallback);
                // The userState can be any object that allows your callback 
                // method to identify this send operation.
                string userState = "Task Builder Scheduler e-mail SMTP sending";
                
                aSmtpClient.SendAsync(mailTask, userState);

                if (aSmtpClient.SendCompletedEvent != null)
                {
                    aSmtpClient.SendCompletedEvent.WaitOne();

                    if (aSmtpClient.SendCompletedLastError != null)
                    {
                        if (schedulerEventLog != null)
							schedulerEventLog.WriteEntry(String.Format(TaskSchedulerObjectsStrings.SmtpSendMailErrorMsg, recipient, aSmtpClient.SendCompletedLastError.Message));
                        
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
		}
        
		//----------------------------------------------------------------------------
		// General utility function to launch a web browser on a specified page
		// via either a URL or a full file path/name.
		// The easiest way to launch the default Web browser from an application is 
		// to simply call the ShellExecute API and pass it a URL. If the default Web
		// browser is currently running, ShellExecute will tell the running instance
		// to navigate to the URL. If it is not running, ShellExecute will start the
		// application and navigate to the URL. 
		//----------------------------------------------------------------------------
		public static bool OpenWebPageInBrowser(string webPageToOpen, bool useIE, bool newBrowserInstance)
		{
			if (webPageToOpen == null || webPageToOpen.Length == 0 || !IsValidURI(webPageToOpen))
				return false;

			try
			{
				System.Diagnostics.Process browserProcess = new System.Diagnostics.Process();

				// Do not receive an event when the process exits.
				browserProcess.EnableRaisingEvents = false;

				if (useIE)
				{
					browserProcess.StartInfo.FileName = GetInternetExplorerFullPath();					
					browserProcess.StartInfo.Arguments = "-new -nohome " + webPageToOpen;

					browserProcess.Start();

					return true;
				}

				if (newBrowserInstance)
				{
					string arguments = String.Empty;
					string browserExe = GetDefaultBrowserCommand(true, webPageToOpen, ref arguments);
					if (browserExe != null && browserExe.Length > 0)
					{
						browserProcess.StartInfo.FileName = browserExe;
						browserProcess.StartInfo.Arguments = arguments;
						browserProcess.StartInfo.UseShellExecute = true;

						browserProcess.Start();

						return true;
					}
				}
				
				browserProcess.StartInfo.FileName = webPageToOpen;
				browserProcess.StartInfo.UseShellExecute = true;
				browserProcess.StartInfo.Verb = "open";

				browserProcess.Start();

				return true;
			}
			catch(Exception)
			{
				return false;
			}

		}

		//----------------------------------------------------------------------------
		public static string GetBrowserCommand(bool useIE, bool newBrowserInstance, string webPageToOpen, ref string arguments)
		{
			if (useIE)
			{
				string internetExplorerFullPath = GetInternetExplorerFullPath();

				arguments = webPageToOpen;

				return internetExplorerFullPath;
			}

			return GetDefaultBrowserCommand(newBrowserInstance, webPageToOpen, ref arguments);
		}
		
		//----------------------------------------------------------------------------
		public static string GetInternetExplorerFullPath()
		{
			string fullPath = String.Empty;
			Microsoft.Win32.RegistryKey internetExplorerRegKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\IEXPLORE.EXE");
			if (internetExplorerRegKey != null)
			{
				fullPath = internetExplorerRegKey.GetValue("").ToString();
				if (fullPath != null && fullPath.Length > 0)
					return fullPath;
			}
			fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			
			if (fullPath != null && fullPath.Length > 0 && !fullPath.EndsWith("\\"))
				fullPath += "\\";
			
			fullPath += "IEXPLORE.EXE";

			return fullPath;
		}

		//----------------------------------------------------------------------------
		public static string GetDefaultBrowserCommand(bool newBrowserInstance, string webPageToOpen, ref string arguments)
		{
			arguments = String.Empty;

			string registryKeyPath = "htmlfile\\shell";
			registryKeyPath += newBrowserInstance ? "\\opennew" : "\\open";
			registryKeyPath += "\\command";
			Microsoft.Win32.RegistryKey defaultBrowserRegKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(registryKeyPath);
			if (defaultBrowserRegKey == null)
				return null;
			
			string browserCommandLine = defaultBrowserRegKey.GetValue("").ToString();
	
			if (browserCommandLine == null || browserCommandLine.Length == 0)
				return null;

			browserCommandLine = browserCommandLine.Replace("\"", "");
			browserCommandLine = browserCommandLine.ToLower(CultureInfo.InvariantCulture);

			int exeExtensionIndex = browserCommandLine.IndexOf(".exe ");
			
			arguments = browserCommandLine.Substring(exeExtensionIndex + 4);

			if (arguments.IndexOf("%1") != -1)
				arguments = arguments.Replace("%1", webPageToOpen);
			else
				arguments += " " + webPageToOpen;

			return browserCommandLine.Substring(0, exeExtensionIndex + 4);
		}
	

		//-------------------------------------------------------------------------------------------
		public static bool GetLoginDataFromIds(SqlConnection connection, int aCompanyId, int aLoginId, out string company, out string user, out bool isCompanyDisabled)
		{
			company				= String.Empty;
			user				= String.Empty;
			isCompanyDisabled	= false;

			SqlDataReader loginDataReader = null;
			try
			{
				loginDataReader  = WTEScheduledTask.GetLoginDataReaderFromIds(connection, aCompanyId, aLoginId);

				if (loginDataReader == null)
					return false;

				company = loginDataReader["Company"].ToString();
				isCompanyDisabled = (bool)loginDataReader["Disabled"];

				bool windowsAuthentication = (bool)loginDataReader["DBWindowsAuthentication"];
				if (!windowsAuthentication)
					user = loginDataReader["Login"].ToString();
				else
					user = loginDataReader["DBUser"].ToString();

				loginDataReader.Close();

				return true;
			}
			catch(SqlException exception)
			{
				if (loginDataReader != null && !loginDataReader.IsClosed)
					loginDataReader.Close();

				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
			}
		}

		//-------------------------------------------------------------------------------------------
		public static bool GetLoginDataFromIds(SqlConnection connection, int aCompanyId, int aLoginId, out string company, out string user)
		{
			bool isCompanyDisabled;
			return GetLoginDataFromIds(connection, aCompanyId, aLoginId, out company, out user, out isCompanyDisabled);
		}
		
		//-------------------------------------------------------------------------------------------
		public static bool GetLoginDataFromIds(SqlConnection connection, int aCompanyId, int aLoginId, out string company, out string user, out string password, out bool windowsAuthentication)
		{
			company		= String.Empty;
			user		= String.Empty;
			password	= String.Empty;
			windowsAuthentication = false;

			SqlDataReader loginDataReader = null;
			try
			{
				loginDataReader  = WTEScheduledTask.GetLoginDataReaderFromIds(connection, aCompanyId, aLoginId);

				if (loginDataReader == null)
					return false;

				company = loginDataReader["Company"].ToString();

				windowsAuthentication = (bool)loginDataReader["DBWindowsAuthentication"];
				if (!windowsAuthentication)
				{
					user = loginDataReader["Login"].ToString();
					password = loginDataReader["Password"].ToString();
				}
				else
				{
					user = loginDataReader["DBUser"].ToString();
					password = loginDataReader["DBPassword"].ToString();
				}
				loginDataReader.Close();
			
				return true;
			}
			catch(SqlException exception)
			{
				if (loginDataReader != null && !loginDataReader.IsClosed)
					loginDataReader.Close();

				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
			}
		}

	
		//-------------------------------------------------------------------------------------------
		public static EventLog GetSchedulerAgentEventLog()
		{
			if (!CheckSchedulerAgentEventLog()) 
				return null;

			EventLog eventLog = new EventLog();
			eventLog.Log = WTEScheduledTask.SchedulerAgentEventLogName;
			eventLog.Source = WTEScheduledTask.SchedulerAgentEventLogSourceName;

			return eventLog;
		}
		
		//-------------------------------------------------------------------------------------------
		public static bool SchedulerAgentEventLogExists()
		{
			try
			{
				return EventLog.Exists(WTEScheduledTask.SchedulerAgentEventLogName);
			}
			catch (SecurityException)
			{
				return false;
			}
		}

		//-------------------------------------------------------------------------------------------
		// Call-back function that is invoked by the proxy class
		// when the asynchronous operation completes.
		//-------------------------------------------------------------------------------------------
		public static void RunFromSchedulerModeCallback(IAsyncResult asyncResult)
		{
			string[] resultMessages = null;
			TaskRunAsyncState taskRunAsyncState = (TaskRunAsyncState)asyncResult.AsyncState;

			bool success = false;
			
			int documentHandle = 0;

			try
			{
				if (taskRunAsyncState.WoormInfo != null)
					taskRunAsyncState.WoormInfo.Dispose();

				if (taskRunAsyncState.Task.RunBatch)
					success = taskRunAsyncState.TbApplicationClientInterface.EndRunBatchInUnattendedMode(asyncResult, out documentHandle, out resultMessages);
				else if (taskRunAsyncState.Task.RunReport)
					success = taskRunAsyncState.TbApplicationClientInterface.EndRunReportInUnattendedMode(asyncResult, out documentHandle, out resultMessages);
				else if (taskRunAsyncState.Task.RunDataExport)
					success = taskRunAsyncState.TbApplicationClientInterface.EndRunXMLExportInUnattendedMode(asyncResult, out documentHandle, out resultMessages);
				else if (taskRunAsyncState.Task.RunDataImport)
					success = taskRunAsyncState.TbApplicationClientInterface.EndRunXMLImportInUnattendedMode(asyncResult, out documentHandle, out resultMessages);
				else
					return;
	            
				if (taskRunAsyncState.EventLog != null && resultMessages != null && resultMessages.Length > 0)
				{
                    StringBuilder taskLog = new StringBuilder();
					String nl = "\n";
					String strCont = "(cont. ...)\n";
					String strSeguito = "(... cont.)\n";
					int maxLen = 16000;
					foreach (String msg in resultMessages)
					{
						// La dimensione massima dell'entry deve essere 32K quindi se del caso spezzo in chuncks.
						if (taskLog.Length + msg.Length + nl.Length + strCont.Length > maxLen)
						{
							taskLog.Append(strCont);
							taskRunAsyncState.WriteLogEntry(taskLog.ToString(), EventLogEntryType.Warning);
							taskLog.Clear();
							taskLog.Append(strSeguito); // do per scontato che msg.Length + nl.Length + strSeguito.Length > 32768
						}

						taskLog.Append(msg);
						taskLog.Append(nl);
					}

                    taskRunAsyncState.WriteLogEntry(taskLog.ToString(), EventLogEntryType.Warning);
                }

                bool runIconizedDocument = false;
                if (documentHandle != 0 && taskRunAsyncState.RunIconized)
                    runIconizedDocument = taskRunAsyncState.TbApplicationClientInterface.RunIconizedDocument(documentHandle);

				//Chisura della finestra della batch o del report
				bool closedDocument = false;
				if (documentHandle != 0 && taskRunAsyncState.CloseDocumentOnEnd)
					closedDocument = taskRunAsyncState.TbApplicationClientInterface.CloseDocument(documentHandle);
			}
			catch(Exception exception)
			{
				Debug.Fail("Error in WTEScheduledTask.RunFromSchedulerModeCallback: " + exception.Message);
				taskRunAsyncState.WriteLogEntry(exception.Message, EventLogEntryType.Error);

				success = false;
			}
			finally
			{
				if (success)
					taskRunAsyncState.Task.EndRunningStateWithSuccess(taskRunAsyncState.ConnectionString, taskRunAsyncState.ExecutionEndedEvent, taskRunAsyncState.EventLog);
				else
					taskRunAsyncState.Task.EndRunningStateWithFailure(taskRunAsyncState.ConnectionString, taskRunAsyncState.ExecutionEndedEvent, taskRunAsyncState.EventLog);
				
				// Devo segnalare il termine dell'esecuzione del task
				if 
					(
					taskRunAsyncState != null && 
					taskRunAsyncState.ExecutionEndedEvent != null &&
					(success || taskRunAsyncState.Task.RetryAttempts <= taskRunAsyncState.Task.RetryAttemptsActualCount)
					)
					taskRunAsyncState.ExecutionEndedEvent.Set();				
			}
		}
		
		#endregion
	}

	//====================================================================================
	[SecurityPermissionAttribute(SecurityAction.Demand, UnmanagedCode=true)]
	[PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
	public class WTETaskImpersonation
	{
		private string domain = String.Empty;
		private string user = String.Empty;
		private IntPtr userToken = IntPtr.Zero;
		private IntPtr duplicatedUserToken = IntPtr.Zero;
		private WindowsIdentity identity = null;
		private WindowsImpersonationContext context = null;

		//---------------------------------------------------------------------
		const int LOGON32_PROVIDER_DEFAULT	= 0;
		const int LOGON32_LOGON_NETWORK		= 3;
		const int LOGON32_LOGON_BATCH		= 4;
		const int LOGON32_LOGON_SERVICE		= 5;

		[DllImport("advapi32.dll", CharSet=CharSet.Auto)]
		public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int GetLastError();

		[DllImport("kernel32.dll", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		private static extern int FormatMessage(int dwFlags, ref IntPtr lpSource, int dwMessageId, int dwLanguageId, ref String lpBuffer, int nSize, ref IntPtr Arguments);

		[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern bool DuplicateToken(IntPtr hExistingToken, int dwImpersonationLevel, ref IntPtr hDuplicatedToken);

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		public static extern bool CloseHandle(IntPtr handle);

		//-----------------------------------------------------------------------
		public string	Domain	{ get { return domain; } }
		public string	User	{ get { return user; } }
		public bool		Done	{ get { return (context != null); } }
		public IntPtr	Token	{ get { return duplicatedUserToken; } }

		//-----------------------------------------------------------------------
		public WTETaskImpersonation(string aDomain, string aUser, string aPassword)
		{
			domain = aDomain;
			user = aUser;

			Do(aPassword);
		}

		//-----------------------------------------------------------------------
		public void Do(string aPassword)
		{
			try
			{
				if (Done)
					Undo();

				IntPtr userToken = LogonUser(aPassword);
				if (userToken == IntPtr.Zero)
					return;

				// Create a new instance of the WindowsIdentity class, passing the account token returned by LogonUser
				// The token that is passed to the following constructor must be a primary token in order to use it for impersonation.
				identity = new WindowsIdentity(userToken);

				// Begin impersonation by creating a new instance of the WindowsImpersonationContext
				// class and initializing it with the WindowsIdentity.Impersonate method of the initialized class
				context = identity.Impersonate();
			}
			catch(Exception exception)
			{
				Undo();

				// The caller does not have the correct permissions.
				Debug.Fail("Exception thrown in WTETaskImpersonation.Do: " + exception.Message);
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.ImpersonationFailedMsg, exception);
			}
		}

		//-----------------------------------------------------------------------
		public IntPtr LogonUser(string aPassword)
		{
			if (
				domain == null ||
				domain.Length == 0 ||
				user == null ||
				user.Length == 0
				)
				return IntPtr.Zero;

			bool ok = false;
			
			userToken = new IntPtr(0);

			try
			{
				ok = LogonUser
					(
					user, 
					domain, 
					aPassword, 
					LOGON32_LOGON_BATCH, 
					LOGON32_PROVIDER_DEFAULT,
					ref userToken
					);
			}
			catch(Exception exception)
			{
				CloseHandle(userToken);
				userToken = IntPtr.Zero;

				Debug.Fail("Exception thrown in WTEScheduledTask.ImpersonateWindowsUser: " + exception.Message);
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.ImpersonationFailedMsg, exception);
			}

			if (!ok)
			{		
				Undo();

				int lastError = GetLastError();
				IntPtr ptrSource = IntPtr.Zero;
				IntPtr ptrArguments = IntPtr.Zero;
				string errorMessage = String.Empty;

				FormatMessage
					(
					0x00001100, // FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM
					ref ptrSource,
					lastError,
					0x0400, // MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT)
					ref errorMessage,
					0,
					ref ptrArguments
					);

				throw new WTEScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.ImpersonationFailedDetailedMsg, errorMessage));
			}

			duplicatedUserToken = new IntPtr(0);
			ok = DuplicateToken
				(
				userToken, 
				2, // The server process can impersonate the client's security context on its local system. 
				ref duplicatedUserToken
				);
			
			if (!ok)
			{
				CloseHandle(userToken);
				userToken = IntPtr.Zero;

				duplicatedUserToken = IntPtr.Zero;

				Debug.Fail("Error in WTETaskImpersonation.Do: user token duplication failed.");
				throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.ImpersonationFailedMsg);
			}
        
			return duplicatedUserToken;
		}
		
		//-----------------------------------------------------------------------
		public void Undo()
		{
			if (context != null)
			{
				context.Undo();
				context = null;
			}

			if (identity != null)
			{
				if (identity.Token != IntPtr.Zero)
					CloseHandle(identity.Token);
				identity = null;
			}

			if (context != null)
			{
				context.Undo();
				context = null;
			}

			if (duplicatedUserToken != IntPtr.Zero)
			{
				CloseHandle(duplicatedUserToken);
				duplicatedUserToken = IntPtr.Zero;
			}
		}		
	}
	
	//=================================================================================
	public class WTEScheduledTaskException : ApplicationException 
	{
		public WTEScheduledTaskException()
		{
		}
		public WTEScheduledTaskException(string message) : base(message)
		{
		}
		public WTEScheduledTaskException(string message, Exception inner): base(message, inner)
		{
		}
		//-----------------------------------------------------------------------
		public string ExtendedMessage
		{
			get
			{
				if (InnerException == null || InnerException.Message == null || InnerException.Message.Length == 0)
					return Message;
				return Message + "\n(" + InnerException.Message + ")";
			}
		}
	}
}
