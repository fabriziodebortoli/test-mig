using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.TaskBuilderNet.Core.TaskschedulerEngine
{
    //=========================================================================
    public class ScheduledSequencesEngine
    {
        public const string ScheduledSequencesTableName = "MSD_ScheduledSequences";

        public const string SequenceIdColumnName = "SequenceId";
        public const string TaskIdColumnName = "TaskId";
        public const string TaskIndexColumnName = "TaskIndex";
        public const string BlockingModeColumnName = "BlockingMode";
        public static string[] ScheduledSequenceTableColumns = new string[4]{
                                                                             SequenceIdColumnName,
                                                                             TaskIdColumnName,
                                                                             TaskIndexColumnName,
                                                                             BlockingModeColumnName
                                                                         };
      

        //---------------------------------------------------------------------
        public static SqlDataReader LoadTasksInSequence(string connectionString, Guid Id)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                Debug.Fail("ScheduledTask.LoadTasksInSequence Error: null or empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.EmptyConnectionStringMsg);
            }

            SqlConnection connection = null;
       //   SqlDataReader selectDataReader = null;
            SqlCommand selectCommand = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                selectCommand = new SqlCommand("SELECT * FROM " + ScheduledSequencesEngine.ScheduledSequencesTableName + " WHERE " + ScheduledSequencesEngine.SequenceIdColumnName + " = '" + Id.ToString() + "' ORDER BY " + ScheduledSequencesEngine.TaskIndexColumnName, connection);
                return selectCommand.ExecuteReader();
            }
            catch (Exception exception)
            {

                Debug.Fail("Exception raised in ScheduledTask.LoadTasksInSequence: " + exception.Message);
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.TaskGenericExceptionMsg, exception);
            }
            //finally
            //{
            //    if (selectDataReader != null && !selectDataReader.IsClosed)
            //        selectDataReader.Close();

            //    if (selectCommand != null)
            //        selectCommand.Dispose();

            //    if (connection != null)
            //    {
            //        if ((connection.State & ConnectionState.Open) == ConnectionState.Open)
            //            connection.Close();
            //        connection.Dispose();
            //    }
            //}
        }
        //----------------------------------------------------------------------
        public static bool IsInSequenceInvolved(string connectionString, Guid Id)
        {
            if (connectionString == null || connectionString == string.Empty)
            {
                Debug.Fail("WTEScheduledTask.IsInSequenceInvolved Error: empty connection string.");
                throw new ScheduledTaskException(TaskSchedulerDBEngineStrings.EmptyConnectionStringMsg);
            }

            int recordsCount = 0;

            SqlConnection connection = null;
            SqlCommand selectCommand = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                selectCommand = new SqlCommand("SELECT COUNT(*) FROM " + ScheduledSequencesTableName + " WHERE " + TaskIdColumnName + " = '" + Id.ToString() + "'", connection);

                recordsCount = (int)selectCommand.ExecuteScalar();
            }
            catch (SqlException exception)
            {
                throw new ScheduledTaskException(String.Format(TaskSchedulerDBEngineStrings.TaskGenericExceptionMsg), exception);
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
            return recordsCount > 0;
        }
        //----------------------------------------------------------------------
        public static bool DeleteAllTaskByid(SqlConnection connection, Guid Id)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
                return false;

            SqlTransaction deleteSqlTransaction = null;
            SqlCommand deleteCommand = null;
            try
            {
                string deleteQueryText = "DELETE FROM ";
                deleteQueryText += ScheduledSequencesTableName;
                deleteQueryText += " WHERE " + SequenceIdColumnName + " = '" + Id.ToString() + "'";

                deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);

                deleteCommand = new SqlCommand(deleteQueryText, connection);
                deleteCommand.Transaction = deleteSqlTransaction;

                deleteCommand.ExecuteNonQuery();

                deleteSqlTransaction.Commit();

                deleteCommand.Dispose();
                deleteSqlTransaction.Dispose();
            }
            catch (Exception exception)
            {
                if (deleteCommand != null)
                    deleteCommand.Dispose();

                if (deleteSqlTransaction != null)
                {
                    deleteSqlTransaction.Rollback();
                    deleteSqlTransaction.Dispose();
                }
                Debug.Fail("Exception raised in ScheduledTask.SaveTasksInSequence: " + exception.Message);
                return false;
            }

            return true;
        }
        //----------------------------------------------------------------------
        public static bool InsertAllTaskInSequence(SqlConnection connection, WTETasksInScheduledSequenceCollection tasksInSequence, Guid Id)
        {
            SqlCommand insertSqlCommand = null;
            SqlTransaction insertSqlTransaction = null;
            try
            {
                string insertQueryText = "INSERT INTO ";
                insertQueryText += ScheduledSequencesTableName;
                insertQueryText += " (";
                for (int i = 0; i < ScheduledSequenceTableColumns.GetUpperBound(0); i++)
                    insertQueryText += ScheduledSequenceTableColumns[i] + ",";
                insertQueryText += ScheduledSequenceTableColumns[ScheduledSequenceTableColumns.GetUpperBound(0)] + ") VALUES (";
                for (int i = 0; i < ScheduledSequenceTableColumns.GetUpperBound(0); i++)
                    insertQueryText += "@" + ScheduledSequenceTableColumns[i] + ",";
                insertQueryText += "@" + ScheduledSequenceTableColumns[ScheduledSequenceTableColumns.GetUpperBound(0)] + ")";

                insertSqlCommand = new SqlCommand(insertQueryText, connection);
                insertSqlCommand.Connection = connection;

                insertSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                insertSqlCommand.Transaction = insertSqlTransaction;

                SqlParameter SequenceIdParam = insertSqlCommand.Parameters.Add("@" + SequenceIdColumnName, SqlDbType.UniqueIdentifier, 16, SequenceIdColumnName);
                SqlParameter TaskIdParam = insertSqlCommand.Parameters.Add("@" + TaskIdColumnName, SqlDbType.UniqueIdentifier, 16, TaskIdColumnName);
                SqlParameter TaskIndexParam = insertSqlCommand.Parameters.Add("@" + TaskIndexColumnName, SqlDbType.SmallInt, 2, TaskIndexColumnName);
                SqlParameter BlockingModeParam = insertSqlCommand.Parameters.Add("@" + BlockingModeColumnName, SqlDbType.Bit, 1, BlockingModeColumnName);

                insertSqlCommand.Prepare(); // Calling Prepare after having setup commandtext and params.

                foreach (WTETaskInScheduledSequence taskInSequence in tasksInSequence)
                {
                    SequenceIdParam.Value = Id;
                    TaskIdParam.Value = taskInSequence.TaskInSequenceId;
                    TaskIndexParam.Value = taskInSequence.TaskInSequenceIndex;
                    BlockingModeParam.Value = taskInSequence.BlockingMode;

                    // Change param values and call execute. This time the prepared command will be executed.
                    insertSqlCommand.ExecuteNonQuery();

                }
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

                Debug.Fail("ScheduledTask.SaveTasksInSequence Error.", exception.Message);

                return false;
            }

            return true;
        }

       
    }
}
