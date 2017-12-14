using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Microarea.Library.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Console.Core.TaskSchedulerObjects
{
	//=========================================================================

	public class TaskExecutionEngine : IDisposable
	{
		#region TaskExecutionEngine private fields

		private SearchTasksToExecuteTimerState searchTasksToExecuteTimerState = null;

		protected internal string systemDBConnectionString = String.Empty;
		private string publicConnectionString = String.Empty;
		private string systemDBName = String.Empty;
		private string systemDBWorkStation = String.Empty;

		private static System.Diagnostics.EventLog runTBScheduledTaskEventLog = ScheduledTask.GetSchedulerAgentEventLog();

		private const short GenericLogEntryCategory = 1;
		private const int StartSchedulerAgentLogEntryEventID = ((int)(1 | ((int)GenericLogEntryCategory) << 8));
		private const int StopSchedulerAgentLogEntryEventID = ((int)(2 | ((int)GenericLogEntryCategory) << 8));
		private const int TimerStateConstructionFailedLogEntryEventID = ((int)(3 | ((int)GenericLogEntryCategory) << 8));
		private const int NextRunDateAdjustmentFailedLogEntryEventID = ((int)(4 | ((int)GenericLogEntryCategory) << 8));
		private const int InvalidDBConnectionLogEntryEventID = ((int)(5 | ((int)GenericLogEntryCategory) << 8));
		private const int InvalidLoginManagerLogEntryEventID = ((int)(6 | ((int)GenericLogEntryCategory) << 8));

		private const short TBLoaderConnectionLogEntryCategory = 2;
		private const int TryTBLoaderConnectionLogEntryEventID = ((int)(1 | ((int)TBLoaderConnectionLogEntryCategory) << 8));
		private const int TBLoaderSuccessfullConnectionLogEntryEventID = ((int)(2 | ((int)TBLoaderConnectionLogEntryCategory) << 8));
		private const int TBLoaderConnectionFailedLogEntryEventID = ((int)(3 | ((int)TBLoaderConnectionLogEntryCategory) << 8));

		private const short RunTasksLogEntryCategory = 3;
		private const int ScheduledTaskFoundLogEntryEventID = ((int)(1 | ((int)RunTasksLogEntryCategory) << 8));
		private const int ScheduledTaskExecutionFailedLogEntryEventID = ((int)(2 | ((int)RunTasksLogEntryCategory) << 8));
		private const int ExceptionRaisedDuringTaskExecutionLogEntryEventID = ((int)(3 | ((int)RunTasksLogEntryCategory) << 8));
		private const int ScheduledTaskSuccessfullyExecutedLogEntryEventID = ((int)(4 | ((int)RunTasksLogEntryCategory) << 8));

		private const short WindowsUserImpersonationLogEntryCategory = 4;
		private const int WindowsUserSuccessfullyImpersonatedLogEntryEventID = ((int)(1 | ((int)WindowsUserImpersonationLogEntryCategory) << 8));
		private const int WindowsUserImpersonationFailedLogEntryEventID = ((int)(2 | ((int)WindowsUserImpersonationLogEntryCategory) << 8));

		private const string SearchTasksToExecuteTimerStateKey = "SearchTasksToExecuteTimerStateKey";

		#endregion

		
		#region TaskExecutionEngine public properties

		//---------------------------------------------------------------------------
		internal static bool TaskIsolation
		{
			get
			{
				PathFinder pf = new PathFinder("", "");
				SettingItem si = pf.GetSettingItem("Framework", "TBGenlib", "Scheduler", "TaskIsolation");
				return si != null && si.Values[0].Equals("1");
			}
		}

		//---------------------------------------------------------------------------
		public string DatabaseName { get { return systemDBName; } }

		//---------------------------------------------------------------------------
		public string DatabaseWorkStation { get { return systemDBWorkStation; } }

		//---------------------------------------------------------------------------
		public string ConnectionString { get { return publicConnectionString; } }

		//---------------------------------------------------------------------------	
		public bool Started { get { return (searchTasksToExecuteTimerState != null); } }

		#endregion

		//@@TODO (Impersonation)[	DllImport("advapi32")]
		//@@TODO (Impersonation)	public static extern bool RevertToSelf();

		//---------------------------------------------------------------------------
		public TaskExecutionEngine(string aConsoleDBConnectionString)
		{
			if (aConsoleDBConnectionString == null || aConsoleDBConnectionString == String.Empty)
			{
				throw new ScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
			}

			systemDBConnectionString = aConsoleDBConnectionString;

			if (!CheckSystemDBConnection())
			{
				throw new ScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSqlConnectionErrMsg);
			}

			publicConnectionString = RemovePasswordFromConnectionString(aConsoleDBConnectionString);

			WriteDBConnectionStringToEventLog();
		}

		//------------------------------------------------------------------------------
		~TaskExecutionEngine()
		{
			Dispose(false);
		}

		//------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Started)
					Stop();
			}
		}

		#region TaskExecutionEngine public methods
		//---------------------------------------------------------------------------
		public void Start()
		{
			if (Started)
				return;

			try
			{
				ScheduledTask.DeleteAllTemporaryTasks(systemDBConnectionString);
				ScheduledTask.AdjustTasksNextRunDateIfNecessary(systemDBConnectionString);
			}
			catch (ScheduledTaskException exception)
			{
				WriteLogEntry(exception.ExtendedMessage, EventLogEntryType.Error, NextRunDateAdjustmentFailedLogEntryEventID, GenericLogEntryCategory);

				throw exception;
			}

			StartSearchTasksToExecuteTimer();

			WriteLogEntry(TaskSchedulerObjectsStrings.StartSchedulerAgentMsg, EventLogEntryType.Information, StartSchedulerAgentLogEntryEventID, GenericLogEntryCategory);
		}

		//---------------------------------------------------------------------------
		public void Stop()
		{
			// When a timer is no longer needed, use the Dispose method to free the resources held by the timer.
			if (searchTasksToExecuteTimerState != null)
			{
				searchTasksToExecuteTimerState.Dispose();
				searchTasksToExecuteTimerState = null;
			}

			WriteLogEntry(TaskSchedulerObjectsStrings.StopSchedulerAgentMsg, EventLogEntryType.Information, StopSchedulerAgentLogEntryEventID, GenericLogEntryCategory);

			try
			{
				ScheduledTask.DeleteAllTemporaryTasks(systemDBConnectionString);
				ScheduledTask.RemoveAllRunningFlags(systemDBConnectionString);
			}
			catch (ScheduledTaskException exception)
			{
				throw exception;
			}
		}

		#endregion

		#region TaskExecutionEngine private methods

		//---------------------------------------------------------------------------
		private bool CheckSystemDBConnection()
		{
			SqlConnection connection = null;

			try
			{
				connection = new SqlConnection(systemDBConnectionString);
				connection.Open();

				systemDBName = connection.Database;
				systemDBWorkStation = connection.WorkstationId;

				return true;
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in TaskExecutionEngine.CheckDatabase: " + exception.Message);

				return false;
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

		//---------------------------------------------------------------------------
		private void WriteDBConnectionStringToEventLog()
		{
			if
				(
				publicConnectionString == null ||
				publicConnectionString == String.Empty ||
				runTBScheduledTaskEventLog == null
				)
				return;

			WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ConnectionStringMsg, publicConnectionString), EventLogEntryType.Information);
		}

		//---------------------------------------------------------------------------
		private void StartSearchTasksToExecuteTimer()
		{
			searchTasksToExecuteTimerState = new SearchTasksToExecuteTimerState(systemDBConnectionString);

			// Create the delegate searchTasksToExecuteTimerDelegate that invokes methods for the timer.
			// The methods do not execute in the thread that created the timer; they execute in a separate thread that 
			// is automatically allocated by the system. 
			TimerCallback searchTasksToExecuteTimerDelegate = new TimerCallback(SearchTasksToExecute);
			// When creating a timer, the application specifies an amount of time to wait before the first invocation
			// of the delegate methods (due time), and an amount of time to wait between subsequent invocations (period).
			// A timer invokes its methods when its due time elapses, and invokes its methods once per period thereafter.
			// Create a timer that waits one second, then invokes every 20 seconds:
			// The timer delegate is specified when the timer is constructed and cannot be changed.
			System.Threading.Timer timer = new System.Threading.Timer(searchTasksToExecuteTimerDelegate, searchTasksToExecuteTimerState, 1000, 20000);
			// Keep a handle to the timer, so it can be disposed.
			searchTasksToExecuteTimerState.timer = timer;
		}

		#endregion

		#region TaskExecutionEngine private static methods

		//-----------------------------------------------------------------------------------
		private static bool WriteLogEntry(string aMessageToWrite, EventLogEntryType type, int eventId, short categoryId, byte[] rawData)
		{
			if (runTBScheduledTaskEventLog == null || aMessageToWrite == null || aMessageToWrite == String.Empty)
				return false;

			try
			{
				if (rawData != null && rawData.Length > 0)
					runTBScheduledTaskEventLog.WriteEntry(aMessageToWrite, type, eventId, categoryId, rawData);
				else
					runTBScheduledTaskEventLog.WriteEntry(aMessageToWrite, type, eventId, categoryId);
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
				return false;
			}

			return true;
		}

		//-----------------------------------------------------------------------------------
		private static bool WriteLogEntry(string aMessageToWrite, EventLogEntryType type)
		{
			return WriteLogEntry(aMessageToWrite, type, 0, 0, null);
		}

		//-----------------------------------------------------------------------------------
		private static bool WriteLogEntry(string aMessageToWrite, EventLogEntryType type, int eventId, short categoryId)
		{
			return WriteLogEntry(aMessageToWrite, type, eventId, categoryId, null);
		}

		//-----------------------------------------------------------------------------------
		private static string RemovePasswordFromConnectionString(string aConnectionString)
		{
			if (aConnectionString == null || aConnectionString == String.Empty)
				return aConnectionString;

			// Tolgo la password dalla stringa di connessione che vado a scrivere nell'event log
			string connectionStringToDisplay = String.Copy(aConnectionString);
			string lowerConnectionString = connectionStringToDisplay.ToLower(CultureInfo.InvariantCulture);
			int pwdIdx = lowerConnectionString.IndexOf("password");
			if (pwdIdx == -1)
				pwdIdx = lowerConnectionString.IndexOf("pwd");
			if (pwdIdx >= 0)
			{
				connectionStringToDisplay = aConnectionString.Substring(0, pwdIdx);
				int semicolonIdx = lowerConnectionString.IndexOf(';', pwdIdx);
				if (semicolonIdx >= 0 && semicolonIdx < (aConnectionString.Length - 1))
					connectionStringToDisplay += aConnectionString.Substring(semicolonIdx + 1, aConnectionString.Length - semicolonIdx - 1);
			}
			return connectionStringToDisplay;
		}

		public const int DACL_SECURITY_INFORMATION = 4;
		[StructLayout(LayoutKind.Sequential)]
		public struct SECURITY_DESCRIPTOR
		{
			public byte revision;
			public byte size;
			public short control;
			public IntPtr owner;
			public IntPtr group;
			public IntPtr sacl;
			public IntPtr dacl;
		}
		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool SetKernelObjectSecurity(int Handle, int SecurityInformation, ref SECURITY_DESCRIPTOR SecurityDescriptor);
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool SetNoSecurityOnMutex(Mutex mutex)
		{
			if (mutex == null)
				return false;

			//Create the security descriptor.
			SECURITY_DESCRIPTOR securityDescriptor = new SECURITY_DESCRIPTOR();
			//The only thing required is to set the revision to one and the DACL to IntPtr.Zero
			securityDescriptor.revision = 1;
			securityDescriptor.dacl = IntPtr.Zero;
			//Apply the DACL to the mutex

			return SetKernelObjectSecurity((int)mutex.SafeWaitHandle.DangerousGetHandle(), DACL_SECURITY_INFORMATION, ref securityDescriptor);

		}
		//-----------------------------------------------------------------------------------
		// SearchTasksToExecute is the timer delegate for searchTasksToExecuteTimerState.timer.
		// It invokes methods that do not execute in the thread that created the timer; they   
		// execute in a separate thread that is automatically allocated by the system.
		//-----------------------------------------------------------------------------------
		private void SearchTasksToExecute(Object state)
		{
			if (!Monitor.TryEnter(typeof(TaskExecutionEngine)))
				return;

			try
			{

				if (state == null || !(state is SearchTasksToExecuteTimerState))
				{
					WriteLogEntry(TaskSchedulerObjectsStrings.SearchTasksToExecuteTimerInvalidStateMsg, EventLogEntryType.Information, ScheduledTaskFoundLogEntryEventID, RunTasksLogEntryCategory);
					return;
				}

				SearchTasksToExecuteTimerState timerState = (SearchTasksToExecuteTimerState)state;

				if (timerState.Connection != null && (timerState.Connection.State & ConnectionState.Broken) == ConnectionState.Broken)
					timerState.Connection.Open();

				if (timerState.Connection == null || (timerState.Connection.State & ConnectionState.Open) != ConnectionState.Open)
				{
					// Qui dentro ci posso passare se la procedura di controllo sull'esistenza di
					// task da eseguire è disabilitata e, quindi non devo dare necessariamente errore!!! 
					// WriteLogEntry(TaskSchedulerAgentAgentString.SearchTasksToExecuteTimerInvalidConnectionMsg, EventLogEntryType.Information, ScheduledTaskFoundLogEntryEventID, RunTasksLogEntryCategory);
					return;
				}

				timerState.SetRunDateParameterValueToNow();

				SqlDataAdapter tasksToRunDataAdapter = new SqlDataAdapter(timerState.SelectTasksToRunSqlCommand);
				DataSet ds = new DataSet();
				tasksToRunDataAdapter.Fill(ds);
				ds.Tables[0].TableName = ScheduledTask.ScheduledTasksTableName;

				foreach (DataRow taskToRunRow in ds.Tables[0].Rows)
				{
					ScheduledTask taskToRun = new ScheduledTask(taskToRunRow, timerState.ConnectionString);

					string user;
					string company;
					bool isCompanyDisabled;
					taskToRun.GetLoginData(timerState.Connection, out company, out user, out isCompanyDisabled);
					if (isCompanyDisabled)
					{
						WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.TaskCompanyDisabledMsg, taskToRun.Code, company, user), EventLogEntryType.Information, ScheduledTaskFoundLogEntryEventID, RunTasksLogEntryCategory);
						continue;
					}

					if (taskToRun.RetryAttemptsActualCount == 0)
						WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ScheduledTaskFoundMsg, taskToRun.Code, company, user), EventLogEntryType.Information, ScheduledTaskFoundLogEntryEventID, RunTasksLogEntryCategory);
					else
						WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ScheduledTaskRetryMsg, taskToRun.RetryAttemptsActualCount + 1, taskToRun.Code, company, user), EventLogEntryType.Information, ScheduledTaskFoundLogEntryEventID, RunTasksLogEntryCategory);

					// Se il task risulta già in esecuzione non viene rilanciato
					if (taskToRun.IsRunning)
					{
						WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.TaskAlreadyRunningMsg, taskToRun.Code, company, user), EventLogEntryType.Information, ScheduledTaskFoundLogEntryEventID, RunTasksLogEntryCategory);
						continue;
					}

					RunTaskThread runTaskThread = StartRunTaskThread(taskToRun, timerState.Connection, timerState.ConnectionString);
					runTaskThread.OnTaskExecutionEnded += new EventHandler(RunTaskThread_OnTaskExecutionEnded);
				}
			}
			catch (Exception exception)
			{
				WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ExceptionRaisedMsg, exception.Message), EventLogEntryType.Error, ExceptionRaisedDuringTaskExecutionLogEntryEventID, RunTasksLogEntryCategory);
			}
			finally
			{
				Monitor.Exit(typeof(TaskExecutionEngine));
			}
		}

		//-----------------------------------------------------------------------------------
		public class TaskExecutionEndedEventArgs : System.EventArgs
		{
			private ScheduledTask task = null;

			//---------------------------------------------------------------------------
			public TaskExecutionEndedEventArgs(RunTaskThread aRunTaskThread)
			{
				task = (aRunTaskThread != null) ? aRunTaskThread.Task : null;
			}

			//------------------------------------------------------------------------------------------------
			public ScheduledTask Task { get { return task; } }
		}
		public delegate void TaskExecutionEndedEventHandler(object sender, TaskExecutionEndedEventArgs e);
		public event TaskExecutionEndedEventHandler TaskExecutionEnded = null;

		//-----------------------------------------------------------------------------------
		private void RunTaskThread_OnTaskExecutionEnded(object sender, System.EventArgs e)
		{
			if (sender == null || !(sender is RunTaskThread))
				return;

			if (TaskExecutionEnded != null)
				TaskExecutionEnded(this, new TaskExecutionEndedEventArgs((RunTaskThread)sender));
		}

		#endregion

		#region TaskExecutionEngine public static methods

		//-----------------------------------------------------------------------------------
		public static TaskExecutionEngine.RunTaskThread StartRunTaskThread(ScheduledTask aTaskToRun, SqlConnection connection, string aConnectionString)
		{
			if
				(
				aTaskToRun == null ||
				!aTaskToRun.Enabled ||
				aTaskToRun.IsRunning ||
				aConnectionString == null ||
				aConnectionString == String.Empty ||
				connection == null ||
				(connection.State & ConnectionState.Open) != ConnectionState.Open
				)
				return null;

			string user = String.Empty;
			string company = String.Empty;
			bool isCompanyDisabled = false;

			try
			{
				if (!aTaskToRun.GetLoginData(connection, out company, out user, out isCompanyDisabled))
				{
					throw new ScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskLoginDataNotFoundMsg, aTaskToRun.Code, aTaskToRun.CompanyId, aTaskToRun.LoginId));
				}

				if (isCompanyDisabled)
				{
					throw new ScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskCompanyDisabledMsg, aTaskToRun.Code, company, user));
				}

				//@@TODO (Impersonation)			// Hang on to the current (impersonated) Identity.
				//@@TODO (Impersonation)			System.Security.Principal.WindowsIdentity impersonatedIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
				//@@TODO (Impersonation)			// This thread is currently impersonating so any thread you start will not 
				//@@TODO (Impersonation)			// have permission to impersonate. You must drop the current impersonation 
				//@@TODO (Impersonation)			// token so that your new thread can impersonate.
				//@@TODO (Impersonation)			RevertToSelf();
				//@@TODO

				RunTaskThread runTaskThread = new RunTaskThread(aTaskToRun, aConnectionString, user, company);

				//@@TODO (Impersonation)			// Return to the original (impersonated) identity.
				//@@TODO (Impersonation)			impersonatedIdentity.Impersonate();

				return runTaskThread;
			}
			catch (Exception exception)
			{
				string newExceptionMessage = String.Format(TaskSchedulerObjectsStrings.TaskExecutionException, aTaskToRun.Code, company, user, exception.Message);
				throw new ScheduledTaskException(newExceptionMessage, exception);
			}
		}

		#endregion

		#region SearchTasksToExecuteTimerState Class
		//============================================================================
		internal class SearchTasksToExecuteTimerState : IDisposable
		{
			private string connectionString = String.Empty;
			private SqlConnection connection = null;
			private SqlCommand selectTasksToRunSqlCommand = null;
			private SqlParameter runDateParam = null;
			public System.Threading.Timer timer = null;

			//------------------------------------------------------------------------------
			public SearchTasksToExecuteTimerState(string aConnectionString)
			{
				try
				{
					connectionString = aConnectionString;
					if (aConnectionString != null && aConnectionString != String.Empty)
					{
						connection = new SqlConnection(aConnectionString);
						connection.Open();

						selectTasksToRunSqlCommand = ScheduledTask.GetAllTasksToRunNowSqlCommand(connection, out runDateParam);
					}
				}
				catch (SqlException exception)
				{
					WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ExceptionRaisedMsg, exception.Message), EventLogEntryType.Error, TimerStateConstructionFailedLogEntryEventID, GenericLogEntryCategory);
				}
			}

			//------------------------------------------------------------------------------
			~SearchTasksToExecuteTimerState()
			{
				Dispose(false);
			}

			//------------------------------------------------------------------------------
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			//------------------------------------------------------------------------------
			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (selectTasksToRunSqlCommand != null)
						selectTasksToRunSqlCommand.Dispose();

					if (connection != null)
					{
						if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
							connection.Close();
						connection.Dispose();
					}
				}
			}

			//------------------------------------------------------------------------------
			public void SetRunDateParameterValueToNow()
			{
				if (runDateParam == null)
					return;
				runDateParam.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
			}

			//------------------------------------------------------------------------------
			public string ConnectionString { get { return connectionString; } }

			//------------------------------------------------------------------------------
			public SqlConnection Connection { get { return connection; } }

			//------------------------------------------------------------------------------
			public SqlCommand SelectTasksToRunSqlCommand { get { return selectTasksToRunSqlCommand; } }
		}
		#endregion

		#region RunTaskThread Class

		/// <summary>
		/// The RunTaskThread class contains the information needed for the execution of
		/// a task, and the method that executes it.
		/// </summary>
		//============================================================================
		public class RunTaskThread
		{
			private ScheduledTask task = null;
			private string connectionString = String.Empty;
			private string user = String.Empty;
			private string company = String.Empty;
			private System.Threading.Thread workerThread = null;
			private ManualResetEvent runningStartedEvent = null;

			private TaskImpersonation taskImpersonation = null;

			public event System.EventHandler OnTaskExecutionEnded;

			#region RunTaskThread public properties

			//------------------------------------------------------------------------------------------------
			public ScheduledTask Task { get { return task; } }

			//------------------------------------------------------------------------------------------------
			public ManualResetEvent RunningStartedEvent { get { return runningStartedEvent; } }

			//------------------------------------------------------------------------------------------------
			public bool IsAlive { get { return (workerThread != null) ? workerThread.IsAlive : false; } }

			#endregion
			// Impersonation happens on a thread by thread basis to allow for concurrency, 
			// which is important for multithreaded servers, as each thread might be servicing a
			// different client. 
			//-----------------------------------------------------------------------------------
			public RunTaskThread(ScheduledTask aTask, string aConnectionString, string aUser, string aCompany)
			{
				task = aTask;
				connectionString = aConnectionString;
				user = aUser;
				company = aCompany;

				ResetRunningStartedEvent();

				// Create a thread to execute the task, and then start the thread.
				workerThread = new Thread(new ThreadStart(this.ThreadProc));
				workerThread.SetApartmentState(ApartmentState.STA);
				workerThread.Start();

				WaitForRunningStarted();

				MemoryManagement.Flush();
			}

			#region RunTaskThread public methods

			//-----------------------------------------------------------------------------------
			[STAThread]
			public void ThreadProc()
			{
				if (task == null)
					return;

				SqlConnection connection = null;

				try
				{
					//@@TODO (Impersonation)	if (!ImpersonateWindowsUser())
					//@@TODO (Impersonation)	{
					//@@TODO (Impersonation)		task.SaveFailureStatus(connection);
					//@@TODO (Impersonation)		return;
					//@@TODO (Impersonation)	}

					connection = new SqlConnection(connectionString);
					connection.Open();

					RunTask(connection);

					//@@TODO (Impersonation)	UndoImpersonation();
				}
				catch (Exception exception)
				{
					task.SaveFailureStatus(connection);

					WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ExceptionRaisedMsg, exception.Message), EventLogEntryType.Error, ExceptionRaisedDuringTaskExecutionLogEntryEventID, RunTasksLogEntryCategory);
				}
				finally
				{
					if (connection != null && (connection.State & ConnectionState.Open) == ConnectionState.Open)
						connection.Close();
				}
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			public void WaitForRunningStarted()
			{
				if (runningStartedEvent == null)
					return;
				// The caller blocks indefinitely until runningStartedEvent receives a signal
				runningStartedEvent.WaitOne();
			}

			#endregion

			#region RunTaskThread private methods

			//-----------------------------------------------------------------------------------
			private bool ImpersonateWindowsUser()
			{
				if (task == null)
					return false;

				if (!task.IsToImpersonateWindowsUser)
					return true;

				taskImpersonation = task.ImpersonateWindowsUser();

				if (taskImpersonation == null || !taskImpersonation.Done)
				{
					WriteLogEntry(TaskSchedulerObjectsStrings.WindowsUserImpersonationFailedMsg, EventLogEntryType.Error, WindowsUserImpersonationFailedLogEntryEventID, WindowsUserImpersonationLogEntryCategory);

					return false;
				}

				WriteLogEntry(TaskSchedulerObjectsStrings.WindowsUserSuccessfullyImpersonatedMsg, EventLogEntryType.Information, WindowsUserSuccessfullyImpersonatedLogEntryEventID, WindowsUserImpersonationLogEntryCategory);

				return true;
			}

			//-----------------------------------------------------------------------------------
			private void UndoImpersonation()
			{
				if (taskImpersonation != null && taskImpersonation.Done)
				{
					taskImpersonation.Undo();
					//@@TODO (Impersonation)	RevertToSelf();
				}
				taskImpersonation = null;
			}

			//--------------------------------------------------------------------------------------------------------------------------------
			private void ResetRunningStartedEvent()
			{
				if (runningStartedEvent == null)
					runningStartedEvent = new ManualResetEvent(false);//the initial state is set to nonsignaled
				else
					runningStartedEvent.Reset();
			}

			//-----------------------------------------------------------------------------------
			private void RunTask(SqlConnection connection)
			{
				if (task == null)
					return;

				if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
				{
					WriteLogEntry(TaskSchedulerObjectsStrings.InvalidDBConnectionMsg, EventLogEntryType.Error, InvalidDBConnectionLogEntryEventID, GenericLogEntryCategory);
					return;
				}

				// Il task va subito aggiornato allo stato di "running", prima della sua vera
				// e propria esecuzione di modo che esso non venga lanciato più volte. Infatti,
				// se il portare a termine tutte le operazioni preliminari necessarie al suo 
				// lancio (v.l'istanziazione del TB Loader) necessita di un tempo superiore 
				// all'intervallo del timer, la procedura timerizzata SearchTasksToExecute
				// potrebbe ritrovare al prossimo giro ancora il task tra quelli da eseguire.
				// Pertanto, in questo caso, occorre chiamare prima la SaveStartRunStatus, poi
				// effettuare le varie operazioni preliminari al lancio e, infine, chiamare la Run.
				task.SaveStartRunStatus(connection);

				if (runningStartedEvent != null)
					runningStartedEvent.Set();

				LoginManager loginManager = null;
				TbLoaderClientInterface tbInterface = null;

				try
				{
					string taskAuthenticationToken = string.Empty;
					int taskLoginId = -1;

					if (task.TBLoaderConnectionNecessaryForRun)
					{
						taskLoginId = task.LoginId;
						loginManager = task.Login(null, connection, true);

						if (loginManager == null)
						{
							task.SaveFailureStatus(connection);

							WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.InvalidLoginManagerMsg, task.Code, company, user), EventLogEntryType.Error, InvalidLoginManagerLogEntryEventID, GenericLogEntryCategory);

							if (OnTaskExecutionEnded != null)
								OnTaskExecutionEnded(this, null);

							return;
						}

						tbInterface = EstablishTBConnection
							(
							task,
							loginManager
							);

						if (tbInterface == null)
						{
							task.SaveFailureStatus(connection);

							loginManager.LogOff();

							WriteLogEntry(TaskSchedulerObjectsStrings.TBLoaderConnectionFailedMsg, EventLogEntryType.Error, TBLoaderConnectionFailedLogEntryEventID, TBLoaderConnectionLogEntryCategory);

							if (OnTaskExecutionEnded != null)
								OnTaskExecutionEnded(this, null);

							return;
						}
					}
					ManualResetEvent executionEndedEvent = new ManualResetEvent(false);//the initial state is set to nonsignaled

					if (task.Run(tbInterface, loginManager, connectionString, executionEndedEvent, runTBScheduledTaskEventLog))
					{
						// Attendo un tempo indefinito sinché executionEndedEvent non riceve alcun segnale
						executionEndedEvent.WaitOne();

						// Ricarico dal database i dati relativi al task in modo che risultino aggiornati
						task.RefreshData(connectionString);

						if (task.SequenceLastRunPartiallySuccessfull)
							WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ScheduledTaskPartiallySuccessfullyExecutedMsg, task.Code, company, user), EventLogEntryType.Information, ScheduledTaskSuccessfullyExecutedLogEntryEventID, RunTasksLogEntryCategory);
						else if (task.LastRunSuccessfull)
							WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ScheduledTaskSuccessfullyExecutedMsg, task.Code, company, user), EventLogEntryType.Information, ScheduledTaskSuccessfullyExecutedLogEntryEventID, RunTasksLogEntryCategory);
						else if (task.LastRunFailed)
							WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ScheduledTaskExecutionFailedMsg, task.Code, company, user), EventLogEntryType.Error, ScheduledTaskExecutionFailedLogEntryEventID, RunTasksLogEntryCategory);
						else
							WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.GenericScheduledTaskExecutionEndedMsg, task.Code, company, user, task.LastRunCompletitionLevel), EventLogEntryType.Information, ScheduledTaskExecutionFailedLogEntryEventID, RunTasksLogEntryCategory);
					}
					else
					{
						// Ricarico dal database i dati relativi al task in modo che risultino aggiornati
						task.RefreshData(connectionString);

						if (task.SequenceLastRunInterrupted)
							WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ScheduledTaskInterruptedExecutionMsg, task.Code, company, user), EventLogEntryType.Error, ScheduledTaskSuccessfullyExecutedLogEntryEventID, RunTasksLogEntryCategory);
						else
							WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ScheduledTaskExecutionFailedMsg, task.Code, company, user), EventLogEntryType.Error, ScheduledTaskExecutionFailedLogEntryEventID, RunTasksLogEntryCategory);
					}

					if (!task.SendMailNotifications(connection, runTBScheduledTaskEventLog))
						WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.SendMailNotificationErrorMsg, task.Code, task.CompanyId, task.LoginId), EventLogEntryType.Error);

					if (task.Temporary)
						task.Delete(connectionString);
				}
				catch (Exception exception)
				{
					if (task.Temporary)
						task.Delete(connectionString);
					else
						task.SaveFailureStatus(connection);

					WriteLogEntry(String.Format(TaskSchedulerObjectsStrings.ExceptionRaisedMsg, exception.Message), EventLogEntryType.Error, ExceptionRaisedDuringTaskExecutionLogEntryEventID, RunTasksLogEntryCategory);
				}
				finally
				{
					TaskCleaner tc = new TaskCleaner(tbInterface, loginManager, runTBScheduledTaskEventLog);
					tc.Clean();

					if (OnTaskExecutionEnded != null)
						OnTaskExecutionEnded(this, null);
				}
			}

			
			//-----------------------------------------------------------------------------------
			private TbLoaderClientInterface EstablishTBConnection(ScheduledTask task, LoginManager loginManager)
			{
				TbLoaderClientInterface aTBLoaderClientInterface = null;
				bool logged = false;

				if (task == null)
					return null;

				try
				{
					aTBLoaderClientInterface = task.EstablishTBConnection(loginManager, true, !TaskExecutionEngine.TaskIsolation);

					if (aTBLoaderClientInterface != null)
					{
						logged = aTBLoaderClientInterface.Logged;
						
						Diagnostic d = aTBLoaderClientInterface.GetGlobalDiagnostic(true);
						if (d != null)
						{
							foreach (DiagnosticItem di in d.AllMessages())
								WriteLogEntry(di.FullExplain, EventLogEntryType.Information, TBLoaderSuccessfullConnectionLogEntryEventID, TBLoaderConnectionLogEntryCategory);
						}
					}
				}
				catch (ScheduledTaskException exception)
				{
					WriteLogEntry(exception.ExtendedMessage, EventLogEntryType.Error, TBLoaderConnectionFailedLogEntryEventID, TBLoaderConnectionLogEntryCategory);
					return null;
				}

				return logged ? aTBLoaderClientInterface : null;
			}

			#endregion
		}

		#endregion

	}

	class TaskCleaner
	{
		private TbLoaderClientInterface tbInterface;
		private LoginManager loginManager;
		private bool clean = false;
		private EventLog eventLog;
		//-----------------------------------------------------------------------------------
		public TaskCleaner(TbLoaderClientInterface tbInterface, LoginManager loginManager, EventLog eventLog)
		{
			this.tbInterface = tbInterface;
			this.loginManager = loginManager;
			this.eventLog = eventLog;
		}

		//-----------------------------------------------------------------------------------
		internal void Clean()
		{
			Application.ApplicationExit += (o, args) =>
			{
				FinalClean();
			};
			//faccio partire un thread che aspetta la chiudura del documento (nel caso fosse rimasto aperta) 
			//per poi effettuare il logoff
			System.Threading.Tasks.Task.Factory.StartNew((Action)delegate
			{
				try
				{
					if (tbInterface != null)
					{
						//ogni secondo controllo se posso chiudere la login
						while (!tbInterface.CanCloseLogin())
							Thread.Sleep(1000);
						tbInterface.CloseLoginAndExternalProcess();
					}
					if (loginManager != null)
						loginManager.LogOff();

					clean = true;
				}
				catch (Exception ex)
				{
					eventLog.WriteEntry(ex.Message, EventLogEntryType.Error);
				}
			});
		}

		//-----------------------------------------------------------------------------------
		private void FinalClean()
		{
			if (clean)
				return;

			if (tbInterface != null)
			{
				//ogni secondo controllo se posso chiudere la login
				tbInterface.CloseAllDocuments();
				tbInterface.CloseLoginAndExternalProcess();
			}
			if (loginManager != null)
				loginManager.LogOff();

			clean = true;
		}
	}
}
