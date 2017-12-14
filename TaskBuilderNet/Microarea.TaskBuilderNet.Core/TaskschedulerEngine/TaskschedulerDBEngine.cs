using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.TaskBuilderNet.Core.TaskschedulerEngine
{
	//=================================================================================
	public enum TaskTypeEnum 
	{
		Undefined			= 0x00000000,		
		Batch				= 0x00000001,				
		Report				= 0x00000002,
		Function			= 0x00000003,
		Executable			= 0x00000004,
		Message				= 0x00000005,						
		Mail				= 0x00000006,
		DataExport			= 0x00000007,
		DataImport			= 0x00000008,
		WebPage				= 0x00000009,
		Sequence			= 0x0000000A,
		DelRunnedReports	= 0x0000000B,
		BackupCompanyDB		= 0x0000000C,
		RestoreCompanyDB	= 0x0000000D
	};

    public enum CompletitionLevelEnum
    {
        Undefined = 0x0000,
        Success = 0x0001,
        Failure = 0x0002,
        Running = 0x0003,
        WaitForNextRetryAttempt = 0x0004,
        Aborted = 0x0005,
        SequenceInterrupted = 0x0006,
        SequencePartialSuccess = 0x0007,
        ClosedProcess = 0x0008
    };

    /// <summary>
    /// Classe per la gestione dei task schedulati
    /// </summary>
    //====================================================================================
    public class WTEScheduledTask
    {
        public const string SchedulerAgentEventLogSourceName = "SchedulerAgent";
        public const string SchedulerAgentEventLogName = Diagnostic.EventLogName;

        public const string ScheduledTasksTableName = "MSD_ScheduledTasks";

        public const string IdColumnName = "Id";
        public const string CodeColumnName = "Code";
        public const string CompanyIdColumnName = "CompanyId";
        public const string LoginIdColumnName = "LoginId";
        public const string ConfigurationColumnName = "AppConfig";
        public const string TypeColumnName = "Type";
        public const string RunningOptionsColumnName = "RunningOptions";
        public const string EnabledColumnName = "Enabled";
        public const string CommandColumnName = "Command";
        public const string DescriptionColumnName = "Description";
        public const string FrequencyTypeColumnName = "FrequencyType";
        public const string FrequencySubtypeColumnName = "FrequencySubtype";
        public const string FrequencyIntervalColumnName = "FrequencyInterval";
        public const string FrequencySubintervalColumnName = "FrequencySubinterval";
        public const string FrequencyRelativeIntervalColumnName = "FrequencyRelativeInterval";
        public const string FrequencyRecurringFactorColumnName = "FrequencyRecurringFactor";
        public const string ActiveStartDateColumnName = "ActiveStartDate";
        public const string ActiveEndDateColumnName = "ActiveEndDate";
        public const string LastRunDateColumnName = "LastRunDate";
        public const string LastRunRetriesColumnName = "LastRunRetries";
        public const string NextRunDateColumnName = "NextRunDate";
        public const string RetryDelayColumnName = "RetryDelay";
        public const string RetryAttemptsColumnName = "RetryAttempts";
        public const string RetryAttemptsActualCountColumnName = "RetryAttemptsActualCount";
        public const string LastRunCompletitionLevelColumnName = "LastRunCompletitionLevel";
        public const string SendMailUsingSMTPColumnName = "SendMailUsingSMTP";
        public const string CyclicRepeatColumnName = "CyclicRepeat";
        public const string CyclicDelayColumnName = "CyclicDelay";
        public const string CyclicTaskCodeColumnName = "CyclicTaskCode";
        public const string ImpersonationDomainColumnName = "ImpersonationDomain";
        public const string ImpersonationUserColumnName = "ImpersonationUser";
        public const string ImpersonationPasswordColumnName = "ImpersonationPassword";
        public const string MessageContentColumnName = "MessageContent";

        public static string[] ScheduledTasksTableColumns = new string[33]{
                                                                              IdColumnName,
                                                                              CodeColumnName,
                                                                              CompanyIdColumnName,
                                                                              LoginIdColumnName,
                                                                              ConfigurationColumnName,
                                                                              TypeColumnName,
                                                                              RunningOptionsColumnName,
                                                                              EnabledColumnName,
                                                                              CommandColumnName,
                                                                              DescriptionColumnName,
                                                                              FrequencyTypeColumnName,
                                                                              FrequencySubtypeColumnName,
                                                                              FrequencyIntervalColumnName,
                                                                              FrequencySubintervalColumnName,
                                                                              FrequencyRelativeIntervalColumnName,
                                                                              FrequencyRecurringFactorColumnName,
                                                                              ActiveStartDateColumnName,
                                                                              ActiveEndDateColumnName,
                                                                              LastRunDateColumnName,
                                                                              LastRunRetriesColumnName,
                                                                              NextRunDateColumnName,
                                                                              RetryDelayColumnName,
                                                                              RetryAttemptsColumnName,
                                                                              RetryAttemptsActualCountColumnName,
                                                                              LastRunCompletitionLevelColumnName,
                                                                              SendMailUsingSMTPColumnName,
                                                                              CyclicRepeatColumnName,
                                                                              CyclicDelayColumnName,
                                                                              CyclicTaskCodeColumnName,
                                                                              ImpersonationDomainColumnName,
                                                                              ImpersonationUserColumnName,
                                                                              ImpersonationPasswordColumnName,
                                                                              MessageContentColumnName
                                                                          };

        public const string SQL_CONNECTION_STRING_SERVER_KEYWORD = "Server";
        public const string SQL_CONNECTION_STRING_DATABASE_KEYWORD = "Database";
        public const string SQL_CONNECTION_STRING_LOGIN_ACCOUNT_KEYWORD = "User Id";
        public const string SQL_CONNECTION_STRING_LOGIN_PASSWORD_KEYWORD = "Password";
        public const string SQL_CONNECTION_STRING_INTEGRATED_SECURITY_TOKEN = "Integrated Security";
        public const string SQL_CONNECTION_STRING_SECURITY_SUPPORT_PROVIDER_INTERFACE = "SSPI";

        public const int RetryAttemptsMaxNumber = 100;
        public const int RetryDelayMaximum = 60;

        public const int TaskCodeUniquePrefixLength = 7;
        private const int taskCodeMaximumLength = 10;
        private const char temporaryCodeChar = '$';
        private const int cyclicRepeatMax = 99;
        private const int impersonationPasswordMaximumLength = 255;

        //=================================================================================

    

        private Guid id = Guid.Empty;
        private string code = String.Empty;


        private string command = String.Empty;
        private string description = String.Empty;

  
        private string cyclicTaskCode = String.Empty;
        private string impersonationDomain = String.Empty;
        private string impersonationUser = String.Empty;
        private string impersonationPassword = String.Empty;
        private string messageContent = String.Empty;
        private IntPtr taskWindowHandle = IntPtr.Zero;


        private string xmlParameters = String.Empty;



        #region DB FUNCTION
        //---------------------------------------------------------------------
        public static bool DeleteAllTemporaryTasks(string connectionString)
        {
            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.DeleteAllTemporaryTasks Error: empty connection string.");
                throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            SqlTransaction deleteSqlTransaction = null;
            SqlCommand deleteCommand = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);

                string deleteTaskNotificationsQueryText = "DELETE FROM ";
                deleteTaskNotificationsQueryText += TaskNotificationRecipientEngine.SchedulerMailNotificationsTableName;
                deleteTaskNotificationsQueryText += " WHERE " + TaskNotificationRecipientEngine.TaskIdColumnName;
                deleteTaskNotificationsQueryText += " IN ( SELECT " + WTEScheduledTask.IdColumnName + " FROM " + WTEScheduledTask.ScheduledTasksTableName;
                deleteTaskNotificationsQueryText += " WHERE " + WTEScheduledTask.FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.Temporary).ToString() + " = " + ((int)FrequencyTypeEnum.Temporary).ToString() + ")";

                deleteCommand = new SqlCommand(deleteTaskNotificationsQueryText, connection, deleteSqlTransaction);

                //deleteCommand.Connection = connection;
                //deleteCommand.Transaction = deleteSqlTransaction;

                deleteCommand.ExecuteNonQuery();

                string deleteTasksInSequenceQueryText = "DELETE FROM ";
                deleteTasksInSequenceQueryText += ScheduledSequencesEngine.ScheduledSequencesTableName;
                deleteTasksInSequenceQueryText += " WHERE " + ScheduledSequencesEngine.SequenceIdColumnName;
                deleteTasksInSequenceQueryText += " IN ( SELECT " + WTEScheduledTask.IdColumnName + " FROM " + WTEScheduledTask.ScheduledTasksTableName;
                deleteTasksInSequenceQueryText += " WHERE " + WTEScheduledTask.FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.Temporary).ToString() + " = " + ((int)FrequencyTypeEnum.Temporary).ToString() + ")";

                deleteCommand.CommandText = deleteTasksInSequenceQueryText;
                deleteCommand.ExecuteNonQuery();

                string deleteQueryText = "DELETE FROM ";
                deleteQueryText += WTEScheduledTask.ScheduledTasksTableName;
                deleteQueryText += " WHERE " + WTEScheduledTask.FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.Temporary).ToString() + " = " + ((int)FrequencyTypeEnum.Temporary).ToString();

                deleteCommand.CommandText = deleteQueryText;
                deleteCommand.ExecuteNonQuery();

                deleteSqlTransaction.Commit();
            }
            catch (Exception exception)
            {
                if (deleteSqlTransaction != null)
                    deleteSqlTransaction.Rollback();

                Debug.Fail("Exception raised in WTEScheduledTask.DeleteAllTemporaryTasks: " + exception.Message);
                throw new WTEScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
            }
            finally
            {
                if (deleteCommand != null)
                    deleteCommand.Dispose();

                if (deleteSqlTransaction != null)
                    deleteSqlTransaction.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
            return true;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool DeleteAllCompanyUserTasks(string connectionString, int aCompanyId, int aLoginId)
        {
            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.DeleteAllTemporaryTasks Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            SqlTransaction deleteSqlTransaction = null;
            SqlCommand deleteCommand = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                string deleteQueryText = "DELETE FROM ";
                deleteQueryText += ScheduledTasksTableName;
                deleteQueryText += " WHERE " + CompanyIdColumnName + " = " + aCompanyId.ToString();
                deleteQueryText += " AND " + LoginIdColumnName + " = " + aLoginId.ToString();

                deleteCommand = new SqlCommand(deleteQueryText, connection);

                deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                deleteCommand.Connection = connection;
                deleteCommand.Transaction = deleteSqlTransaction;

                deleteCommand.ExecuteNonQuery();

                deleteSqlTransaction.Commit();
            }
            catch (Exception exception)
            {
                if (deleteSqlTransaction != null)
                    deleteSqlTransaction.Rollback();

                Debug.Fail("Exception raised in WTEScheduledTask.DeleteAllTemporaryTasks: " + exception.Message);
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.TaskGenericExceptionMsg, exception);
            }
            finally
            {
                if (deleteCommand != null)
                    deleteCommand.Dispose();

                if (deleteSqlTransaction != null)
                    deleteSqlTransaction.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
            return true;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool DeleteAllCompanyTasks(string connectionString, int aCompanyId)
        {
            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.DeleteAllTemporaryTasks Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            SqlTransaction deleteSqlTransaction = null;
            SqlCommand deleteCommand = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                string deleteQueryText = "DELETE FROM ";
                deleteQueryText += ScheduledTasksTableName;
                deleteQueryText += " WHERE " + CompanyIdColumnName + " = " + aCompanyId.ToString();

                deleteCommand = new SqlCommand(deleteQueryText, connection);

                deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                deleteCommand.Connection = connection;
                deleteCommand.Transaction = deleteSqlTransaction;

                deleteCommand.ExecuteNonQuery();

                deleteSqlTransaction.Commit();
            }
            catch (Exception exception)
            {
                if (deleteSqlTransaction != null)
                    deleteSqlTransaction.Rollback();

                Debug.Fail("Exception raised in WTEScheduledTask.DeleteAllTemporaryTasks: " + exception.Message);
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.TaskGenericExceptionMsg, exception);
            }
            finally
            {
                if (deleteCommand != null)
                    deleteCommand.Dispose();

                if (deleteSqlTransaction != null)
                    deleteSqlTransaction.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
            return true;
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool RemoveAllRunningFlags(string connectionString)
        {
            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.DeleteAllTemporaryTasks Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            SqlTransaction updateSqlTransaction = null;
            SqlCommand updateSqlCommand = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                string updateQueryText = "UPDATE ";
                updateQueryText += ScheduledTasksTableName;
                updateQueryText += " SET ";
                updateQueryText += LastRunCompletitionLevelColumnName + " = " + ((int)CompletitionLevelEnum.Aborted).ToString();
                updateQueryText += " WHERE " + LastRunCompletitionLevelColumnName + " = " + ((int)CompletitionLevelEnum.Running).ToString();

                updateSqlCommand = new SqlCommand(updateQueryText, connection);

                updateSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                updateSqlCommand.Transaction = updateSqlTransaction;

                updateSqlCommand.ExecuteNonQuery();

                updateSqlTransaction.Commit();

                updateSqlCommand.Dispose();
                updateSqlTransaction.Dispose();
            }
            catch (Exception exception)
            {
                if (updateSqlTransaction != null)
                    updateSqlTransaction.Rollback();

                Debug.Fail("Exception raised in WTEScheduledTask.DeleteAllTemporaryTasks: " + exception.Message);
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
            }
            finally
            {
                if (updateSqlCommand != null)
                    updateSqlCommand.Dispose();

                if (updateSqlTransaction != null)
                    updateSqlTransaction.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
            return true;
        }
        //-------------------------------------------------------------------------------------------
        public static SqlDataReader GetLoginDataReaderFromIds(SqlConnection connection, int aCompanyId, int aLoginId)
        {
            if (aCompanyId == 0 || aLoginId == 0)
                return null;

            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.GetLoginDataReaderFromIds Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.InvalidSqlConnectionErrMsg);
            }

            SqlDataReader loginDataReader = null;
            SqlCommand selectLoginDataSqlCommand = null;

            try
            {
                string selectLoginData = @"SELECT MSD_Companies.Company, MSD_Companies.Disabled, MSD_Logins.Login, MSD_Logins.Password, MSD_CompanyLogins.DBUser, MSD_CompanyLogins.DBPassword, MSD_CompanyLogins.DBWindowsAuthentication FROM MSD_Companies, MSD_Logins, MSD_CompanyLogins
											WHERE MSD_Companies.CompanyId = " + aCompanyId.ToString() + " AND MSD_Logins.LoginId = " + aLoginId.ToString() + " AND MSD_CompanyLogins.CompanyId = " + aCompanyId.ToString() + " AND MSD_CompanyLogins.LoginId = " + aLoginId.ToString();

                selectLoginDataSqlCommand = new SqlCommand(selectLoginData, connection);
                loginDataReader = selectLoginDataSqlCommand.ExecuteReader();

                if (loginDataReader == null || !loginDataReader.Read())
                {
                    loginDataReader.Close();
                    return null;
                }
                return loginDataReader;
            }
            catch (SqlException exception)
            {
                if (loginDataReader != null && !loginDataReader.IsClosed)
                    loginDataReader.Close();

               throw new ScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
            }
            finally
            {
                if (selectLoginDataSqlCommand != null)
                    selectLoginDataSqlCommand.Dispose();
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool IsValidTaskCode(string aCode, int aCompanyId, int aLoginId, string connectionString)
        {
            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.IsValidTaskCode Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                return IsValidTaskCode(aCode, aCompanyId, aLoginId, connection);
            }
            catch (Exception exception)
            {
                Debug.Fail("Exception raised in WTEScheduledTask.IsValidTaskCode: " + exception.Message);
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.TaskGenericExceptionMsg, exception);
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
        public static bool IsValidTaskCode(string aCode, int aCompanyId, int aLoginId, SqlConnection connection)
        {
            if (aCode == null || aCode.Length == 0)
                return false;

            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.IsValidTaskCode Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSqlConnectionErrMsg);
            }

            string query = String.Empty;
            SqlCommand selectCommand = null;
            int recordsCount = 0;

            try
            {
                if (aCode.Length < TaskCodeUniquePrefixLength)
                {
                    // Per specificare nella query il valore del codice del task, trattandosi di 
                    // una stringa, utilizzo un parametro (v. problemi Unicode)
                    query = "SELECT COUNT(*) FROM " + ScheduledTasksTableName + " WHERE ";
                    query += CodeColumnName + " = @Code AND ";
                    query += CompanyIdColumnName + " = " + aCompanyId.ToString() + " AND ";
                    query += LoginIdColumnName + " = " + aLoginId.ToString() + " AND ";
                    query += FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.Temporary).ToString() + " = 0";

                    selectCommand = new SqlCommand(query, connection);

                    SqlParameter param = selectCommand.Parameters.Add("@Code", SqlDbType.NVarChar, taskCodeMaximumLength, CodeColumnName);
                    param.Value = aCode;

                    recordsCount = (int)selectCommand.ExecuteScalar();
                }
                else
                {

                    string codePatternMatching = aCode.Substring(0, TaskCodeUniquePrefixLength);
                    codePatternMatching += '%';

                    // Per specificare nella query il valore del codice del task, trattandosi di 
                    // una stringa, utilizzo un parametro (v. problemi Unicode)
                    query = "SELECT COUNT(*) FROM " + ScheduledTasksTableName + " WHERE ";
                    query += CodeColumnName + " <> @Code AND ";
                    query += CodeColumnName + " LIKE @CodePatternMatching AND ";
                    query += CompanyIdColumnName + " = " + aCompanyId.ToString() + " AND ";
                    query += LoginIdColumnName + " = " + aLoginId.ToString() + " AND ";
                    query += FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.Temporary).ToString() + " = 0";

                    selectCommand = new SqlCommand(query, connection);

                    SqlParameter param = selectCommand.Parameters.Add("@Code", SqlDbType.NVarChar, taskCodeMaximumLength, CodeColumnName);
                    param.Value = aCode;

                    selectCommand.Parameters.AddWithValue("@CodePatternMatching", codePatternMatching);

                    recordsCount = (int)selectCommand.ExecuteScalar();
                }
            }
            catch (SqlException exception)
            {
                throw new ScheduledTaskException(String.Format(TaskSchedulerDBEngineStrings.TaskGenericExceptionMsg, aCode), exception);
            }
            finally
            {
                if (selectCommand != null)
                    selectCommand.Dispose();
            }
            return (recordsCount == 0);
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool Insert(SqlConnection connection, WTEScheduledTaskObj obj)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.Insert Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSqlConnectionErrMsg);
            }

            SqlCommand insertSqlCommand = null;
            SqlTransaction insertSqlTransaction = null;

            try
            {
                string insertQueryText = "INSERT INTO ";
                insertQueryText += ScheduledTasksTableName;
                insertQueryText += " (";
                for (int i = 0; i < ScheduledTasksTableColumns.GetUpperBound(0); i++)
                    insertQueryText += ScheduledTasksTableColumns[i] + ",";
                insertQueryText += ScheduledTasksTableColumns[ScheduledTasksTableColumns.GetUpperBound(0)] + ") VALUES (";
                for (int i = 0; i < ScheduledTasksTableColumns.GetUpperBound(0); i++)
                    insertQueryText += "@" + ScheduledTasksTableColumns[i] + ",";
                insertQueryText += "@" + ScheduledTasksTableColumns[ScheduledTasksTableColumns.GetUpperBound(0)] + ")";

                insertSqlCommand = new SqlCommand(insertQueryText, connection);

                insertSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                insertSqlCommand.Transaction = insertSqlTransaction;

                SetAllTaskSqlCommandParameters(ref insertSqlCommand, obj);

                insertSqlCommand.ExecuteNonQuery();

                insertSqlTransaction.Commit();

                insertSqlCommand.Dispose();
                insertSqlTransaction.Dispose();
            }
            catch (Exception exception)
            {
                if (insertSqlCommand != null)
                    insertSqlCommand.Dispose();
                if (insertSqlTransaction != null)
                {
                    insertSqlTransaction.Rollback();
                    insertSqlTransaction.Dispose();
                }

                Debug.Fail("Exception raised in WTEScheduledTask.Insert: " + exception.Message);
                throw new ScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskInsertionFailedMsgFmt, obj.Code), exception);
            
            }

            if (obj.IsSequence && obj.tasksInSequence != null && !ScheduledSequencesEngine.InsertAllTaskInSequence(connection, obj.tasksInSequence, obj.Id))
                return false;
            return true;
        }
           

        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool Update(SqlConnection connection, WTEScheduledTaskObj obj)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.Update Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.InvalidSqlConnectionErrMsg);
            }



            SqlCommand updateSqlCommand = null;
            SqlTransaction updateSqlTransaction = null;
            try
            {
                string updateQueryText = "UPDATE ";
                updateQueryText += ScheduledTasksTableName;
                updateQueryText += " SET ";
                for (int i = 0; i < ScheduledTasksTableColumns.GetUpperBound(0); i++)
                    updateQueryText += ScheduledTasksTableColumns[i] + " = @" + ScheduledTasksTableColumns[i] + ",";
                updateQueryText += ScheduledTasksTableColumns[ScheduledTasksTableColumns.GetUpperBound(0)] + " = @" + ScheduledTasksTableColumns[ScheduledTasksTableColumns.GetUpperBound(0)];
                updateQueryText += " WHERE " + IdColumnName + " = @" + IdColumnName;

                updateSqlCommand = new SqlCommand(updateQueryText, connection);

                updateSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                updateSqlCommand.Transaction = updateSqlTransaction;

                SetAllTaskSqlCommandParameters(ref updateSqlCommand, obj);

                updateSqlCommand.ExecuteNonQuery();

                updateSqlTransaction.Commit();
            }
            catch (Exception exception)
            {
                if (updateSqlCommand != null)
                    updateSqlCommand.Dispose();

                if (updateSqlTransaction != null)
                    updateSqlTransaction.Rollback();

                Debug.Fail("Exception raised in WTEScheduledTask.Update: " + exception.Message);
               throw new ScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskUpdateFailedMsgFmt, obj.Code), exception);
           
            }
            finally
            {
                if (updateSqlCommand != null)
                    updateSqlCommand.Dispose();

                if (updateSqlTransaction != null)
                    updateSqlTransaction.Dispose();
            }
            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static  bool Delete(string connectionString, Guid id,bool isSequence)
        {
            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.Delete Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            SqlTransaction deleteSqlTransaction = null;
            SqlCommand deleteCommand = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                if (isSequence)
                    ScheduledSequencesEngine.DeleteAllTaskByid(connection, id);

                deleteCommand = connection.CreateCommand();
                deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                deleteCommand.Transaction = deleteSqlTransaction;

                string deleteQueryText = String.Empty;



                deleteQueryText = "DELETE FROM ";
                deleteQueryText += ScheduledTasksTableName;
                deleteQueryText += " WHERE " + IdColumnName + " = '" + id.ToString() + "'";

                deleteCommand.CommandText = deleteQueryText;

                deleteCommand.ExecuteNonQuery();

                deleteSqlTransaction.Commit();
            }
            catch (Exception exception)
            {
                if (deleteSqlTransaction != null)
                    deleteSqlTransaction.Rollback();

                Debug.Fail("Exception raised in WTEScheduledTask.Delete: " + exception.Message);
                throw new ScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskDeleteFailedMsgFmt, id, exception));
            }
            finally
            {
                if (deleteCommand != null)
                    deleteCommand.Dispose();

                if (deleteSqlTransaction != null)
                    deleteSqlTransaction.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static SqlCommand GetSelectAllTasksOrderedByCodeQuery(SqlConnection connection, int aCompanyId, int aLoginId)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.GetSelectAllTasksSequencesOrderedByCodeQuery Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.InvalidSqlConnectionErrMsg);
            }

            string queryText = @"SELECT * FROM " + ScheduledTasksTableName + " WHERE ";

            queryText += FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.Temporary).ToString() + " = 0 AND ";
            queryText += CompanyIdColumnName + " = " + aCompanyId.ToString() + " AND ";
            queryText += LoginIdColumnName + " = " + aLoginId.ToString();
            queryText += " ORDER BY " + CodeColumnName;

            SqlCommand selectCommand = new SqlCommand(queryText, connection);

            return selectCommand;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static SqlCommand GetSelectAllTasksSequencesOrderedByCodeQuery(SqlConnection connection, int aCompanyId, int aLoginId)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.GetSelectAllTasksSequencesOrderedByCodeQuery Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSqlConnectionErrMsg);
            }

            string queryText = @"SELECT * FROM " + ScheduledTasksTableName + " WHERE ";

            queryText += FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.Temporary).ToString() + " = 0 AND ";
            queryText += CompanyIdColumnName + " = " + aCompanyId.ToString() + " AND ";
            queryText += LoginIdColumnName + " = " + aLoginId.ToString() + " AND ";
            queryText += TypeColumnName + " = " + ((int)TaskTypeEnum.Sequence).ToString();
            queryText += " ORDER BY " + CodeColumnName;

            SqlCommand selectCommand = new SqlCommand(queryText, connection);

            return selectCommand;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static SqlCommand GetSelectAllTasksOnDemandOrderedByCodeQuery(SqlConnection connection, int aCompanyId, int aLoginId, string aConfiguration)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.GetSelectAllTasksOnDemandOrderedByCodeQuery Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.InvalidSqlConnectionErrMsg);
            }

            string queryText = @"SELECT * FROM " + ScheduledTasksTableName + " WHERE ";

            if (aConfiguration != null && aConfiguration.Length > 0)
                queryText += ConfigurationColumnName + " = @Configuration AND ";

            queryText += CompanyIdColumnName + " = " + aCompanyId + " AND " + LoginIdColumnName + " = " + aLoginId;
            queryText += " AND " + FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.OnDemand).ToString() + " = " + ((int)FrequencyTypeEnum.OnDemand).ToString();
            queryText += " AND " + TypeColumnName + " <> " + (int)TaskTypeEnum.Sequence + " ORDER BY " + CodeColumnName;

            SqlCommand selectCommand = new SqlCommand(queryText, connection);

            if (aConfiguration != null && aConfiguration.Length > 0)
                selectCommand.Parameters.AddWithValue("@Configuration", aConfiguration);

            return selectCommand;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static SqlCommand GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(SqlConnection connection, TaskTypeEnum typeToSearch, int aCompanyId, int aLoginId, string aConfiguration)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.InvalidSqlConnectionErrMsg);
            }

            string queryText = @"SELECT * FROM " + ScheduledTasksTableName + " WHERE ";

            if (aConfiguration != null && aConfiguration.Length > 0)
                queryText += ConfigurationColumnName + " = @Configuration AND ";

            queryText += CompanyIdColumnName + " = " + aCompanyId + " AND " + LoginIdColumnName + " = " + aLoginId;
            queryText += " AND " + FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.OnDemand).ToString() + " = " + ((int)FrequencyTypeEnum.OnDemand).ToString();
            queryText += " AND " + TypeColumnName + " = " + (int)typeToSearch + " ORDER BY " + CodeColumnName;

            SqlCommand selectCommand = new SqlCommand(queryText, connection);

            if (aConfiguration != null && aConfiguration.Length > 0)
                selectCommand.Parameters.AddWithValue("@Configuration", aConfiguration);

            return selectCommand;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static SqlCommand GetAllTasksToRunNowSqlCommand(SqlConnection connection, out SqlParameter runDateParam)
        {
            runDateParam = null;

            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.GetAllTasksToRunNowSqlCommand Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.InvalidSqlConnectionErrMsg);
            }

            string query = "SELECT * FROM " + ScheduledTasksTableName + " WHERE ";
            query += EnabledColumnName + " = 1 AND ";
            query += LastRunCompletitionLevelColumnName + " <> " + ((int)CompletitionLevelEnum.Running).ToString() + " AND ";
            query += NextRunDateColumnName + " <= @NextRunDate";

            SqlCommand selectTasksToRunSqlCommand = new SqlCommand(query, connection);

            runDateParam = selectTasksToRunSqlCommand.Parameters.Add("@NextRunDate", SqlDbType.DateTime, 8, NextRunDateColumnName);

            return selectTasksToRunSqlCommand;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static SqlDataReader GetTaskData(int aLoginId, int aCompanyId, string aTaskCode, string connectionString)
        {
            SqlConnection connection = null;
            SqlCommand selectTasksSqlCommand = null;
            SqlDataReader taskDataReader = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                // Per specificare nella query il valore del codice del task, trattandosi di 
                // una stringa, utilizzo un parametro (v. problemi Unicode)
                string query = "SELECT * FROM " + ScheduledTasksTableName + " WHERE ";
                query += CodeColumnName + " = @Code AND ";
                query += CompanyIdColumnName + " = " + aCompanyId.ToString() + " AND ";
                query += LoginIdColumnName + " = " + aLoginId.ToString();

                selectTasksSqlCommand = new SqlCommand(query, connection);

                SqlParameter param = selectTasksSqlCommand.Parameters.Add("@Code", SqlDbType.NVarChar, taskCodeMaximumLength, CodeColumnName);
                param.Value = aTaskCode;

                taskDataReader = selectTasksSqlCommand.ExecuteReader();

                if (taskDataReader == null || !taskDataReader.Read())
                {
                    if (taskDataReader != null && !taskDataReader.IsClosed)
                        taskDataReader.Close();

                    return null;

                    throw new ScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskNotFound2MsgFmt, aTaskCode, aCompanyId, aLoginId));
                }

                return taskDataReader;
            }
            catch (SqlException exception)
            {
                Debug.Fail("Exception raised in WTEScheduledTask constructor: " + exception.Message);
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.InvalidTaskConstruction, exception);
            }
            finally
            {
                if (taskDataReader != null && !taskDataReader.IsClosed)
                    taskDataReader.Close();

                if (selectTasksSqlCommand != null)
                    selectTasksSqlCommand.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static SqlDataReader GetTaskDataById(Guid aId, SqlConnection connection)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
            {
                Debug.Fail("WTEScheduledTask.GetTaskDataById Error: invalid connection.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.InvalidSqlConnectionErrMsg);
            }

            SqlCommand selectTasksSqlCommand = null;
            SqlDataReader taskDataReader = null;
            try
            {
                string query = "SELECT * FROM " + ScheduledTasksTableName + " WHERE " + IdColumnName + " = '" + aId.ToString() + "'";

                selectTasksSqlCommand = new SqlCommand(query, connection);

                taskDataReader = selectTasksSqlCommand.ExecuteReader();

                if (taskDataReader != null && taskDataReader.Read())
                    return taskDataReader;

                taskDataReader.Close();
                return null;
            }
            catch (SqlException exception)
            {
                if (taskDataReader != null && !taskDataReader.IsClosed)
                    taskDataReader.Close();

                throw new ScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.ExistenceCheckFailedMsgFmt, aId.ToString()), exception);
            }
            finally
            {
                if (selectTasksSqlCommand != null)
                    selectTasksSqlCommand.Dispose();
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static WTEScheduledTaskObj GetTaskById(Guid aTaskId, string connectionString)
        {
            if (aTaskId == Guid.Empty)
                return null;

            if (connectionString == null || connectionString.Length == 0)
            {
                Debug.Fail("WTEScheduledTask.GetTaskById Error: null or empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.EmptyConnectionStringMsg);
            }

            WTEScheduledTaskObj task = null;
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                task = new WTEScheduledTaskObj(aTaskId, connectionString, false);
            }
            catch (Exception exception)
            {
                throw new ScheduledTaskException(String.Format(TaskSchedulerDBEngineStrings.TaskNotFoundMsgFmt, aTaskId.ToString()), exception);
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
            return task;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool Exists(string aCode,
            int aCompanyId,
            int aLoginId,
            string connectionString)
        {
            if (aCode == null || aCode.Length == 0)
                return false;

            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.Exists Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            SqlCommand selectCommand = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                // Per specificare nella query il valore del codice del task, trattandosi di 
                // una stringa, utilizzo un parametro (v. problemi Unicode)
                string query = "SELECT COUNT(*) FROM " + ScheduledTasksTableName;
                query += " WHERE " + CodeColumnName + " = @Code";
                query += " AND " + CompanyIdColumnName + " = " + aCompanyId.ToString();
                query += " AND " + LoginIdColumnName + " = " + aLoginId.ToString();

                selectCommand = new SqlCommand(query, connection);

                SqlParameter param = selectCommand.Parameters.Add("@Code", SqlDbType.NVarChar, taskCodeMaximumLength, CodeColumnName);
                param.Value = aCode;

                int recordsCount = (int)selectCommand.ExecuteScalar();

                return recordsCount > 0;
            }
            catch (SqlException exception)
            {
                throw new ScheduledTaskException(String.Format(TaskSchedulerDBEngineStrings.ExistenceCheckFailedMsgFmt, aCode), exception);
            }
            finally
            {
                if (selectCommand != null)
                    selectCommand.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static bool Exists(Guid aId, string connectionString)
        {
            if (aId == Guid.Empty)
                return false;

            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.Exists Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            SqlCommand selectCommand = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                selectCommand = new SqlCommand("SELECT COUNT(*) FROM " + ScheduledTasksTableName + " WHERE " + IdColumnName + " = '" + aId + "'", connection);

                int recordsCount = (int)selectCommand.ExecuteScalar();

                return recordsCount > 0;
            }
            catch (SqlException exception)
            {
               throw new ScheduledTaskException(String.Format(TaskSchedulerDBEngineStrings.ExistenceCheckFailedMsgFmt, aId.ToString()), exception);
            }
            finally
            {
                if (selectCommand != null)
                    selectCommand.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static int CompanyTasksCount(string connectionString, int aCompanyId)
        {
            if (aCompanyId == -1)
                return 0;

            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.CompanyHasTasks Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            SqlCommand selectCommand = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                string query = "SELECT COUNT(*) FROM " + ScheduledTasksTableName;
                query += " WHERE " + CompanyIdColumnName + " = " + aCompanyId.ToString();

                selectCommand = new SqlCommand(query, connection);

                return (int)selectCommand.ExecuteScalar();
            }
            catch (SqlException exception)
            {
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.TaskGenericExceptionMsg, exception);
            }
            finally
            {
                if (selectCommand != null)
                    selectCommand.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }
        #endregion





        #region WTEScheduledTask private methods

        ////--------------------------------------------------------------------------------------------------------------------------------
        //private void FillFromTaskDataReader(SqlDataReader taskDataReader, string connectionString, bool onlyUI)
        //{
        //	if (taskDataReader == null || taskDataReader.IsClosed)
        //	{
        //		Debug.Fail("WTEScheduledTask.FillFromTaskDataReader Error: invalid SqlDataReader.");
        //		throw new ScheduledTaskException(TaskSchedulerObjectsStrings.InvalidSqlDataReaderErrMsg);
        //	}

        //	if (connectionString == null || connectionString.Length == 0)
        //	{
        //		Debug.Fail("WTEScheduledTask.FillFromTaskDataReader Error: null or empty connection string.");
        //		throw new ScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
        //	}

        //	SqlConnection connection = null;

        //	try
        //	{
        //		connection = new SqlConnection(connectionString);
        //		connection.Open();

        //		id = new Guid(taskDataReader[IdColumnName].ToString()); 
        //		code = (string)taskDataReader[CodeColumnName];
        //		companyId = (int)taskDataReader[CompanyIdColumnName];
        //		loginId = (int)taskDataReader[LoginIdColumnName];

        //		type = (TaskTypeEnum)taskDataReader[TypeColumnName];
        //		if (IsSequence && !onlyUI)
        //			LoadTasksInSequence(connectionString);

        //		runningOptions = (RunningOptionsEnum)taskDataReader[RunningOptionsColumnName];

        //		enabled = (taskDataReader[EnabledColumnName] != System.DBNull.Value) ? (bool)taskDataReader[EnabledColumnName] : false;
        //		Command	= (taskDataReader[CommandColumnName] != System.DBNull.Value) ? (string)taskDataReader[CommandColumnName] : String.Empty;
        //		description	= (taskDataReader[DescriptionColumnName] != System.DBNull.Value) ? (string)taskDataReader[DescriptionColumnName] : String.Empty;

        //		frequencyType = (FrequencyTypeEnum)taskDataReader[FrequencyTypeColumnName];
        //		frequencySubtype = (FrequencySubtypeEnum)taskDataReader[FrequencySubtypeColumnName];

        //		frequencyInterval			= (taskDataReader[FrequencyIntervalColumnName] != System.DBNull.Value) ? (int)taskDataReader[FrequencyIntervalColumnName] : 1;
        //		frequencySubinterval		= (taskDataReader[FrequencySubintervalColumnName] != System.DBNull.Value) ? (int)taskDataReader[FrequencySubintervalColumnName] : 0;
        //		frequencyRelativeInterval	= (taskDataReader[FrequencyRelativeIntervalColumnName] != System.DBNull.Value) ? (FrequencyRelativeIntervalTypeEnum)taskDataReader[FrequencyRelativeIntervalColumnName] : FrequencyRelativeIntervalTypeEnum.Undefined;
        //		frequencyRecurringFactor	= (taskDataReader[FrequencyRecurringFactorColumnName] != System.DBNull.Value) ? (int)taskDataReader[FrequencyRecurringFactorColumnName] : 1;

        //		activeStartDate	= (taskDataReader[ActiveStartDateColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[ActiveStartDateColumnName] : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
        //		activeEndDate	= (taskDataReader[ActiveEndDateColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[ActiveEndDateColumnName] : DateTime.MaxValue;

        //		lastRunDate = (taskDataReader[LastRunDateColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[LastRunDateColumnName] : MinimumDate; 
        //		lastRunRetries = (taskDataReader[LastRunRetriesColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[LastRunRetriesColumnName] : MinimumDate; 
        //		nextRunDate = (taskDataReader[NextRunDateColumnName] != System.DBNull.Value) ? (DateTime)taskDataReader[NextRunDateColumnName] : DateTime.MaxValue; 

        //		retryAttempts = (taskDataReader[RetryAttemptsColumnName] != System.DBNull.Value) ? (int)taskDataReader[RetryAttemptsColumnName] : 0;
        //		retryDelay = (taskDataReader[RetryDelayColumnName] != System.DBNull.Value) ? (int)taskDataReader[RetryDelayColumnName] : 0;
        //		retryAttemptsActualCount = (taskDataReader[RetryAttemptsActualCountColumnName] != System.DBNull.Value) ? (int)taskDataReader[RetryAttemptsActualCountColumnName] : 0;

        //		lastRunCompletitionLevel = (CompletitionLevelEnum)(Int16)taskDataReader[LastRunCompletitionLevelColumnName];

        //		sendMailUsingSMTP = (taskDataReader[SendMailUsingSMTPColumnName] != System.DBNull.Value) ? (bool)taskDataReader[SendMailUsingSMTPColumnName] : true;

        //		cyclicRepeat = (taskDataReader[CyclicRepeatColumnName] != System.DBNull.Value)? (int)taskDataReader[CyclicRepeatColumnName] : 0;
        //		cyclicDelay = (taskDataReader[CyclicDelayColumnName] != System.DBNull.Value) ? (int)taskDataReader[CyclicDelayColumnName] : 0;
        //		cyclicTaskCode = (taskDataReader[CyclicTaskCodeColumnName] != System.DBNull.Value) ? (string)taskDataReader[CyclicTaskCodeColumnName]: String.Empty;

        //		impersonationDomain = (taskDataReader[ImpersonationDomainColumnName] != System.DBNull.Value) ? (string)taskDataReader[ImpersonationDomainColumnName] : String.Empty;
        //		impersonationUser = (taskDataReader[ImpersonationUserColumnName] != System.DBNull.Value) ? (string)taskDataReader[ImpersonationUserColumnName] : String.Empty;
        //		impersonationPassword = (taskDataReader[ImpersonationPasswordColumnName] != System.DBNull.Value) ? DecryptPassword((string)taskDataReader[ImpersonationPasswordColumnName]) : String.Empty;

        //		messageContent = (taskDataReader[MessageContentColumnName] != System.DBNull.Value) ? (string)taskDataReader[MessageContentColumnName] : String.Empty;
        //	}
        //	catch(Exception exception)
        //	{
        //		Debug.Fail("Exception raised in WTEScheduledTask.FillFromTaskDataReader: " + exception.Message);
        //		throw new ScheduledTaskException(exception.Message);
        //	}
        //	finally
        //	{
        //		if (connection != null)
        //		{
        //			if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
        //				connection.Close();
        //			connection.Dispose();
        //		}
        //	}
        //}

        ////--------------------------------------------------------------------------------------------------------------------------------
        //private void FillFromTaskDataReader(SqlDataReader taskDataReader, string connectionString)
        //{
        //	FillFromTaskDataReader(taskDataReader, connectionString, false);
        //}

        //--------------------------------------------------------------------------------------------------------------------------------
        private static  void SetAllTaskSqlCommandParameters(ref SqlCommand aTaskSqlCommand, WTEScheduledTaskObj obj)
        {
            SqlParameter param = aTaskSqlCommand.Parameters.Add("@Id", SqlDbType.UniqueIdentifier, 16, IdColumnName);
            param.Value = obj.Id;
            param = aTaskSqlCommand.Parameters.Add("@Code", SqlDbType.NVarChar, taskCodeMaximumLength, CodeColumnName);
            param.Value = obj.Code;
            param = aTaskSqlCommand.Parameters.Add("@CompanyId", SqlDbType.Int, 4, CompanyIdColumnName);
            param.Value = obj.CompanyId;
            param = aTaskSqlCommand.Parameters.Add("@LoginId", SqlDbType.Int, 4, LoginIdColumnName);
            param.Value = obj.LoginId;
            param = aTaskSqlCommand.Parameters.Add("@AppConfig", SqlDbType.NVarChar, 30, ConfigurationColumnName);
            param.Value = "";
            param = aTaskSqlCommand.Parameters.Add("@Type", SqlDbType.Int, 4, TypeColumnName);
            param.Value = obj.Type;
            param = aTaskSqlCommand.Parameters.Add("@RunningOptions", SqlDbType.Int, 4, RunningOptionsColumnName);
            param.Value = obj.RunningOptions;
            param = aTaskSqlCommand.Parameters.Add("@Enabled", SqlDbType.Bit, 1, EnabledColumnName);
            param.Value = obj.Enabled;
            param = aTaskSqlCommand.Parameters.Add("@Command", SqlDbType.NVarChar, 255, CommandColumnName);
            param.Value = obj.Command;
            param = aTaskSqlCommand.Parameters.Add("@Description", SqlDbType.NVarChar, 128, DescriptionColumnName);
            param.Value = obj.Description;
            param = aTaskSqlCommand.Parameters.Add("@FrequencyType", SqlDbType.Int, 4, FrequencyTypeColumnName);
            param.Value = obj.FrequencyType;
            param = aTaskSqlCommand.Parameters.Add("@FrequencySubtype", SqlDbType.Int, 4, FrequencySubtypeColumnName);
            param.Value = obj.FrequencySubtype;
            param = aTaskSqlCommand.Parameters.Add("@FrequencyInterval", SqlDbType.Int, 4, FrequencyIntervalColumnName);
            param.Value = obj.FrequencyInterval;
            param = aTaskSqlCommand.Parameters.Add("@FrequencySubinterval", SqlDbType.Int, 4, FrequencySubintervalColumnName);
            param.Value = obj.FrequencySubinterval;
            param = aTaskSqlCommand.Parameters.Add("@FrequencyRelativeInterval", SqlDbType.Int, 4, FrequencyRelativeIntervalColumnName);
            param.Value = obj.FrequencyRelativeInterval;
            param = aTaskSqlCommand.Parameters.Add("@FrequencyRecurringFactor", SqlDbType.Int, 4, FrequencyRecurringFactorColumnName);
            param.Value = obj.FrequencyRecurringFactor;
            param = aTaskSqlCommand.Parameters.Add("@ActiveStartDate", SqlDbType.DateTime, 8, ActiveStartDateColumnName);
            param.Value = obj.ActiveStartDate;
            param = aTaskSqlCommand.Parameters.Add("@ActiveEndDate", SqlDbType.DateTime, 8, ActiveEndDateColumnName);
            param.Value = obj.ActiveEndDate;
            param = aTaskSqlCommand.Parameters.Add("@LastRunDate", SqlDbType.DateTime, 8, LastRunDateColumnName);
            param.Value = obj.LastRunDate;
            param = aTaskSqlCommand.Parameters.Add("@LastRunRetries", SqlDbType.DateTime, 8, LastRunRetriesColumnName);
            param.Value = obj.LastRunRetries;
            param = aTaskSqlCommand.Parameters.Add("@NextRunDate", SqlDbType.DateTime, 8, NextRunDateColumnName);
            param.Value = obj.NextRunDate;
            param = aTaskSqlCommand.Parameters.Add("@RetryDelay", SqlDbType.Int, 4, RetryDelayColumnName);
            param.Value = obj.RetryDelay;
            param = aTaskSqlCommand.Parameters.Add("@RetryAttempts", SqlDbType.Int, 4, RetryAttemptsColumnName);
            param.Value = obj.RetryAttempts;
            param = aTaskSqlCommand.Parameters.Add("@RetryAttemptsActualCount", SqlDbType.Int, 4, RetryAttemptsActualCountColumnName);
            param.Value = obj.RetryAttemptsActualCount;
            param = aTaskSqlCommand.Parameters.Add("@LastRunCompletitionLevel", SqlDbType.SmallInt, 2, LastRunCompletitionLevelColumnName);
            param.Value = obj.LastRunCompletitionLevel;
            param = aTaskSqlCommand.Parameters.Add("@SendMailUsingSMTP", SqlDbType.Bit, 1, SendMailUsingSMTPColumnName);
            param.Value = obj.SendMailUsingSMTP;
            param = aTaskSqlCommand.Parameters.Add("@CyclicRepeat", SqlDbType.Int, 4, CyclicRepeatColumnName);
            param.Value = obj.CyclicRepeat;
            param = aTaskSqlCommand.Parameters.Add("@CyclicDelay", SqlDbType.Int, 4, CyclicDelayColumnName);
            param.Value = obj.CyclicDelay;
            param = aTaskSqlCommand.Parameters.Add("@CyclicTaskCode", SqlDbType.NVarChar, taskCodeMaximumLength, CyclicTaskCodeColumnName);
            param.Value = obj.CyclicTaskCode;
            param = aTaskSqlCommand.Parameters.Add("@ImpersonationDomain", SqlDbType.NVarChar, 255, ImpersonationDomainColumnName);
            param.Value = obj.ImpersonationDomain;
            param = aTaskSqlCommand.Parameters.Add("@ImpersonationUser", SqlDbType.NVarChar, 255, ImpersonationUserColumnName);
            param.Value = obj.ImpersonationUser;
            param = aTaskSqlCommand.Parameters.Add("@ImpersonationPassword", SqlDbType.NVarChar, 255, ImpersonationPasswordColumnName);
            param.Value = obj.EncryptImpersonationPassword();
            param = aTaskSqlCommand.Parameters.Add("@MessageContent", SqlDbType.NVarChar, 255, MessageContentColumnName);
            if (obj.SendReportAsMailAttachment)
                param.Value = obj.ReportSendingRecipients;
            else if (obj.SaveReportAsFile)
                param.Value = obj.ReportSavingFileName;
            else
                param.Value = obj.MessageContent;
        }

       
     

       
        //--------------------------------------------------------------------------------------------------------------------------------   

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
        public bool SetCode(string aCode, string connectionString)
        {
            if (aCode == null || aCode.Length == 0)
                return false;

            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.SetCode Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
            }
            bool ok = false;

            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                ok = SetCode(aCode, connectionString);
            }
            catch (SqlException exception)
            {
                return false;
                throw new ScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, aCode), exception);
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
        public static  SqlCommand GetSelectAllTasksOnDemandOrderedByCodeQuery(SqlConnection connection, int companyId, int loginId)
        {
            return GetSelectAllTasksOnDemandOrderedByCodeQuery(connection, companyId, loginId, "");
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public  static SqlCommand GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(SqlConnection connection, TaskTypeEnum typeToSearch, int companyId, int loginId)
        {
            return GetSelectAllTasksOnDemandOfTypeOrderedByCodeQuery(connection, typeToSearch, companyId, loginId, "");
        }

        #endregion

        #region WTEScheduledTask static functions



        //--------------------------------------------------------------------------------------------------------
        public static string BuildConnectionString(string server, string serverInstance, string database, bool useWindowsAuthentication, string loginAccount, string password)
        {
            if (server == null || server.Length == 0 || database == null || database.Length == 0)
            {
                Debug.Fail("WTEScheduledTask.BuildConnectionString Error: The passed information is not sufficient for building a connection string.");
                return String.Empty;
            }

            if (serverInstance != null && serverInstance.Length > 0)
                server += Path.DirectorySeparatorChar + serverInstance;

            string connectionString = SQL_CONNECTION_STRING_SERVER_KEYWORD + "=" + server + ";";
            connectionString += SQL_CONNECTION_STRING_DATABASE_KEYWORD + "=" + database + ";";

            // If we are using Windows authentication when connecting to SQL Server, we avoid embedding user
            // names and passwords in the connection string.
            // We use the Integrated Security keyword, set to a value of SSPI, to specify Windows Authentication:
            if (useWindowsAuthentication)
            {
                connectionString += SQL_CONNECTION_STRING_INTEGRATED_SECURITY_TOKEN;// the connection should use Windows integrated security (NT authentication)
                connectionString += "=";
                connectionString += SQL_CONNECTION_STRING_SECURITY_SUPPORT_PROVIDER_INTERFACE;
                connectionString += ";";
            }
            else
            {
                if (loginAccount == null || loginAccount.Length == 0)
                {
                    Debug.Fail("WTEScheduledTask.BuildConnectionString Error: the login account is void.");
                    return String.Empty;
                }
                connectionString += SQL_CONNECTION_STRING_LOGIN_ACCOUNT_KEYWORD + "=" + loginAccount;
                if (password != null && password.Length > 0)
                    connectionString += ";" + SQL_CONNECTION_STRING_LOGIN_PASSWORD_KEYWORD + "=" + password;
                connectionString += ";";
            }

            return connectionString;
        }




        //-------------------------------------------------------------------------------------------
        public static void AdjustTasksNextRunDateIfNecessary(string connectionString)
        {
            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.AdjustTasksNextRunDateIfNecessary Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
            SqlCommand selectCommand = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                SqlDateTime now = new SqlDateTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0));
                // Cerco tutti i task con esecuzione schedulata periodicamente la cui data di 
                // prossima esecuzione è trascorsa
                string query = "SELECT * FROM " + ScheduledTasksTableName + " WHERE " + FrequencyTypeColumnName + " & " + ((int)FrequencyTypeEnum.FrequencyTypeFlagsMask).ToString() + " IN (";
                query += ((int)FrequencyTypeEnum.RecurringDaily).ToString() + ",";
                query += ((int)FrequencyTypeEnum.RecurringMonthly1).ToString() + ",";
                query += ((int)FrequencyTypeEnum.RecurringMonthly2).ToString() + ",";
                query += ((int)FrequencyTypeEnum.RecurringWeekly).ToString() + ") AND ";
                query += NextRunDateColumnName + " < '" + DateTime.Now.ToString(@"yyyy-MM-ddTHH\:mm\:00") + "'";

                selectCommand = new SqlCommand(query, connection);

                SqlDataAdapter tasksNotExecutedDataAdapter = new SqlDataAdapter(selectCommand);
                DataSet ds = new DataSet();
                tasksNotExecutedDataAdapter.Fill(ds);
                ds.Tables[0].TableName = WTEScheduledTask.ScheduledTasksTableName;

                foreach (DataRow taskNotExecutedDataRow in ds.Tables[0].Rows)
                {
                    WTEScheduledTaskObj taskNotExecuted = new WTEScheduledTaskObj(taskNotExecutedDataRow, connectionString);
                    if (taskNotExecuted.IsRunning)
                        continue;
                    taskNotExecuted.InitNextRunDate();

                    Update(connection, taskNotExecuted);
                }
            }
            catch (Exception exception)
            {
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.NextRunDateAdjustmentFailedMsg, exception);
            }
            finally
            {
                if (selectCommand != null)
                    selectCommand.Dispose();

                if (connection != null)
                {
                    if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }



        #endregion
    }
    //=================================================================================
    public class ScheduledTaskException : ApplicationException
    {
        public ScheduledTaskException()
        {
        }
        public ScheduledTaskException(string message) : base(message)
        {
        }
        public ScheduledTaskException(string message, Exception inner) : base(message, inner)
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

    //====================================================================================
    [SecurityPermissionAttribute(SecurityAction.Demand, UnmanagedCode = true)]
    [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
    public class TaskImpersonation
    {
        private string domain = String.Empty;
        private string user = String.Empty;
        private IntPtr userToken = IntPtr.Zero;
        private IntPtr duplicatedUserToken = IntPtr.Zero;
        private WindowsIdentity identity = null;
        private WindowsImpersonationContext context = null;

        //---------------------------------------------------------------------
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_LOGON_BATCH = 4;
        const int LOGON32_LOGON_SERVICE = 5;

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetLastError();

        [DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern int FormatMessage(int dwFlags, ref IntPtr lpSource, int dwMessageId, int dwLanguageId, ref String lpBuffer, int nSize, ref IntPtr Arguments);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DuplicateToken(IntPtr hExistingToken, int dwImpersonationLevel, ref IntPtr hDuplicatedToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        //-----------------------------------------------------------------------
        public string Domain { get { return domain; } }
        public string User { get { return user; } }
        public bool Done { get { return (context != null); } }
        public IntPtr Token { get { return duplicatedUserToken; } }

        //-----------------------------------------------------------------------
        public TaskImpersonation(string aDomain, string aUser, string aPassword)
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
            catch (Exception exception)
            {
                Undo();

                // The caller does not have the correct permissions.
                Debug.Fail("Exception thrown in TaskImpersonation.Do: " + exception.Message);
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.ImpersonationFailedMsg, exception);
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
            catch (Exception exception)
            {
                CloseHandle(userToken);
                userToken = IntPtr.Zero;

                Debug.Fail("Exception thrown in ScheduledTask.ImpersonateWindowsUser: " + exception.Message);
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.ImpersonationFailedMsg, exception);
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

                throw new ScheduledTaskException(String.Format(TaskSchedulerObjectsStrings.ImpersonationFailedDetailedMsg, errorMessage));
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

                Debug.Fail("Error in TaskImpersonation.Do: user token duplication failed.");
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.ImpersonationFailedMsg);
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
  }
