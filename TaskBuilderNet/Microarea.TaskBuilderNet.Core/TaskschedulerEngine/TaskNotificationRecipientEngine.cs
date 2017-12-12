using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.TaskBuilderNet.Core.TaskschedulerEngine
{
    //=========================================================================
    public class TaskNotificationRecipientEngine
    {
        public const string SchedulerMailNotificationsTableName = "MSD_SchedulerMailNotifications";
        public const string TaskIdColumnName = "TaskId";
        public const string RecipientNameColumnName = "RecipientName";
        public const string SendConditionColumnName = "SendCondition";

        public static string[] SchedulerMailNotificationsTableColumns = new string[3]{
                                                                                         TaskIdColumnName,
                                                                                         RecipientNameColumnName,
                                                                                         SendConditionColumnName
                                                                                     };
        //--------------------------------------------------------------------------------------------------------------------------------
        private static bool DeleteMailNotificationsByTaskId(SqlConnection connection, Guid id)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
                return false;

            SqlTransaction deleteSqlTransaction = null;
            SqlCommand deleteCommand = null;
            try
            {
                string deleteCommandQuery = "DELETE FROM ";
                deleteCommandQuery += TaskNotificationRecipientEngine.SchedulerMailNotificationsTableName;
                deleteCommandQuery += " WHERE " + TaskNotificationRecipientEngine.TaskIdColumnName + " = '" + id + "'";

                deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                deleteCommand = new SqlCommand(deleteCommandQuery, connection);
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
                Debug.Fail("Exception raised in ScheduledTask.SaveMailNotificationsSettings: " + exception.Message);
                return false;
            }

            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private static bool InsertMailNotificationsByTaskId(SqlConnection connection, Guid id, TaskSchedulerObjects.WTETaskNotificationRecipient notificationRecipient)
        {
            SqlTransaction insertSqlTransaction = null;
            SqlCommand insertSqlCommand = null;
            try
            {
                string insertQueryText = "INSERT INTO ";
                insertQueryText += TaskNotificationRecipientEngine.SchedulerMailNotificationsTableName;
                insertQueryText += " (";
                for (int i = 0; i < TaskNotificationRecipientEngine.SchedulerMailNotificationsTableColumns.GetUpperBound(0); i++)
                    insertQueryText += TaskNotificationRecipientEngine.SchedulerMailNotificationsTableColumns[i] + ",";
                insertQueryText += TaskNotificationRecipientEngine.SchedulerMailNotificationsTableColumns[TaskNotificationRecipientEngine.SchedulerMailNotificationsTableColumns.GetUpperBound(0)] + ") VALUES (";
                for (int i = 0; i < TaskNotificationRecipientEngine.SchedulerMailNotificationsTableColumns.GetUpperBound(0); i++)
                    insertQueryText += "@" + TaskNotificationRecipientEngine.SchedulerMailNotificationsTableColumns[i] + ",";
                insertQueryText += "@" + TaskNotificationRecipientEngine.SchedulerMailNotificationsTableColumns[TaskNotificationRecipientEngine.SchedulerMailNotificationsTableColumns.GetUpperBound(0)] + ")";

                insertSqlCommand = new SqlCommand(insertQueryText, connection);

                insertSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
                insertSqlCommand.Transaction = insertSqlTransaction;

                SqlParameter TaskIdParam = insertSqlCommand.Parameters.Add("@" + TaskNotificationRecipientEngine.TaskIdColumnName, SqlDbType.UniqueIdentifier, 16, TaskNotificationRecipientEngine.TaskIdColumnName);
                SqlParameter RecipientNameParam = insertSqlCommand.Parameters.Add("@" + TaskNotificationRecipientEngine.RecipientNameColumnName, SqlDbType.NVarChar, 254, TaskNotificationRecipientEngine.RecipientNameColumnName);
                SqlParameter SendConditionParam = insertSqlCommand.Parameters.Add("@" + TaskNotificationRecipientEngine.SendConditionColumnName, SqlDbType.SmallInt, 2, TaskNotificationRecipientEngine.SendConditionColumnName);

                insertSqlCommand.Prepare(); // Calling Prepare after having setup commandtext and params.

                //foreach (TaskNotificationRecipient notificationRecipient in notificationRecipients)
                //{
                    TaskIdParam.Value = id;
                    RecipientNameParam.Value = notificationRecipient.Recipient;
                    int sendConditionParam = 0;
                    if (notificationRecipient.IsToNotifyOnSuccess)
                        sendConditionParam += 0x0001;
                    if (notificationRecipient.IsToNotifyOnFailure)
                        sendConditionParam += 0x0002;
                    SendConditionParam.Value = sendConditionParam;
                    // Change param values and call execute. This time the prepared command will be executed.
                    insertSqlCommand.ExecuteNonQuery();

                //}
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

                Debug.Fail("ScheduledTask.SaveMailNotificationsSettings Error.", exception.Message);

                return false;
            }

            return true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static void SaveMailNotificationsSettings(SqlConnection connection, Guid id, TaskSchedulerObjects.WTETaskNotificationRecipientsCollection notificationRecipients)
        {
            TaskNotificationRecipientEngine.DeleteMailNotificationsByTaskId(connection, id);
            if (notificationRecipients == null || notificationRecipients.Count <= 0)
                return;

            foreach (TaskSchedulerObjects.WTETaskNotificationRecipient notificationRecipient in notificationRecipients)
                TaskNotificationRecipientEngine.InsertMailNotificationsByTaskId(connection, id, notificationRecipient);
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public static SqlDataReader LoadMailNotificationsSettings(SqlConnection connection, Guid Id)
        {
            if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
                return null;

            SqlCommand selectCommand = null;
            SqlDataReader selectDataReader = null;
            try
            {
                selectCommand = new SqlCommand("SELECT * FROM " + SchedulerMailNotificationsTableName + " WHERE " + TaskIdColumnName + " = '" + Id + "' ORDER BY " + RecipientNameColumnName, connection);
                return selectCommand.ExecuteReader();
            }
            catch (Exception exception)
            {
                Debug.Fail("Exception raised in ScheduledTask.LoadMailNotificationsSettings: " + exception.Message);
                throw new ScheduledTaskException(TaskSchedulerObjectsStrings.TaskGenericExceptionMsg, exception);
            }
            finally
            {
                if (selectDataReader != null && !selectDataReader.IsClosed)
                    selectDataReader.Close();

                if (selectCommand != null)
                    selectCommand.Dispose();
            }
        }


    }
}
