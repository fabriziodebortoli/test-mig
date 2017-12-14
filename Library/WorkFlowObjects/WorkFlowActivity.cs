using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Microarea.Library.WorkFlowObjects
{
	/// <summary>
	/// Summary description for WorkFlowActivity.
	/// </summary>
	public class WorkFlowActivity
	{
		public const string WorkFlowActionTableName				= "MSD_WorkFlowActivity";
		public const string TemplateIdColumnName				= "TemplateId";
		public const string CompanyIdColumnName					= "CompanyId";
		public const string ActivityIdColumnName				= "ActivityId";
		public const string WorkFlowIdColumnName				= "WorkFlowId";
		public const string ActivityNameColumnName				= "ActivityName";
		public const string ActivityDescriptionColumnName       = "ActivityDescription";
		public const string EnabledColumnName					= "Enabled";

		private int		companyId			= -1;
		private int		workFlowId			= -1;
		private int		activityId			= -1;
		private string	activityName		= string.Empty;
		private string	activityDescription = string.Empty;
		private string  workFlowName	    = string.Empty;

		//---------------------------------------------------------------------
		public int		CompanyId			{ get { return companyId; } }
		//---------------------------------------------------------------------
		public int		WorkFlowId			{ get { return workFlowId; } }  
		//---------------------------------------------------------------------
		public int		ActivityId			{ get { return activityId; } }  
		//---------------------------------------------------------------------
		public string	ActivityName		{ get { return activityName; } }  
		//---------------------------------------------------------------------
		public string	ActivityDescription	{ get { return activityDescription; } }  
		//---------------------------------------------------------------------
		public string	WorkFlowName		{ get { return workFlowName; } set { workFlowName = value; }} 



		//---------------------------------------------------------------------
		public WorkFlowActivity() : this(-1, -1, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowActivity(int aWorkFlowId) : this(-1, aWorkFlowId, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowActivity(int aCompanyId, int aWorkFlowId) : this(aCompanyId, aWorkFlowId, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowActivity(int aCompanyId, int aWorkFlowId, int aActivityId, string aActivityName, string aActivityDescription)
		{
			this.companyId				= aCompanyId;
			this.workFlowId				= aWorkFlowId;
			this.activityId				= aActivityId;
			this.activityName			= aActivityName;
			this.activityDescription	= aActivityDescription;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WorkFlowActivity(DataRow aWorkFlowActiviyTableRow, int aCompanyId, string connectionString)
		{
			if (aWorkFlowActiviyTableRow == null || string.Compare(aWorkFlowActiviyTableRow.Table.TableName, WorkFlowActionTableName, true) != 0)
				return;

			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WorkFlowActiviy Constructor Error: null or empty connection string.");
				throw new WorkFlowException(WorkFlowObjectStrings.EmptyConnectionStringMsg);
			}
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();
				//leggo le info dalla row del datagrid
				workFlowId			= (int)		aWorkFlowActiviyTableRow[WorkFlowIdColumnName];
				companyId			= aCompanyId;
				activityId			= (int)		aWorkFlowActiviyTableRow[ActivityIdColumnName];
				activityName		= (string)	aWorkFlowActiviyTableRow[ActivityNameColumnName];
				activityDescription = (string)	aWorkFlowActiviyTableRow[ActivityDescriptionColumnName];
				
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WorkFlowActivity constructor: " + exception.Message);
				throw new WorkFlowException(WorkFlowObjectStrings.InvalidWorkFlowActivityConstruction, exception);
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


		//---------------------------------------------------------------------
		public static string GetSelectAllWorkFlowActivitiesOrderedByNameQuery(int aCompanyId, int aWorkFlowId)
		{
			string queryText =  @"SELECT * FROM " + WorkFlowActionTableName + " WHERE ";

			
			queryText += ActivityIdColumnName + " = " + aWorkFlowId.ToString() + " AND ";
			queryText += " INNER JOIN MSD_WorkFlow ON MSD_WorkFlow.WorkFlowId = MSD_WorkFlowActivity.WorkFlowId WHERE ";
			queryText += CompanyIdColumnName + " = " + aCompanyId.ToString();
			queryText += " ORDER BY " + ActivityIdColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectAllBaseActivitiesOrderedByNameQuery()
		{
			int templateBaseId = 1;
			string queryText =  @"SELECT * FROM " + WorkFlowTemplateActivity.WorkFlowTemplateActionTableName + " WHERE ";
				
			queryText += WorkFlowTemplateActivity.WorkFlowTemplateActionTableName + "."  + WorkFlowTemplateActivity.TemplateIdColumnName + " = " + templateBaseId.ToString();
			queryText += " ORDER BY " + WorkFlowTemplateActivity.ActivityNameColumnName;


			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectTemplateActivitiesOrderedByNameQuery(int templateId)
		{
			string queryText =  @"SELECT ActivityName, ActivityDescription FROM " + WorkFlowTemplateActivity.WorkFlowTemplateActionTableName + " WHERE ";
				
			queryText += WorkFlowTemplateActivity.WorkFlowTemplateActionTableName + "."  + TemplateIdColumnName + " = " + templateId.ToString();
			queryText += " ORDER BY " + WorkFlowTemplateActivity.ActivityNameColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectAllActivitiesForCompanyOrderedByNameQuery(int aCompanyId)
		{
			string queryText =  @"SELECT * FROM " + WorkFlowActionTableName + " INNER JOIN MSD_WorkFlow ON MSD_WorkFlow.WorkFlowId = MSD_WorkFlowActivity.WorkFlowId WHERE ";
				

			queryText += CompanyIdColumnName + " = " + aCompanyId.ToString();
			queryText += " ORDER BY " + ActivityIdColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectAllActivitiesOrderedByNameQuery(int aCompanyId, int aWorkFlowId)
		{
			string queryText =  "SELECT * FROM " + WorkFlowActionTableName + " INNER JOIN MSD_WorkFlow ON MSD_WorkFlow.WorkFlowId = MSD_WorkFlowActivity.WorkFlowId WHERE "; 
			queryText += CompanyIdColumnName + " = " + aCompanyId.ToString() + " AND ";
			queryText += WorkFlowActionTableName + "." + WorkFlowIdColumnName + " = " + aWorkFlowId.ToString();

            queryText +=  " ORDER BY " + ActivityIdColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public bool Insert(SqlConnection connection)
		{
			SqlCommand insertSqlCommand = null;
			SqlTransaction  insertSqlTransaction = null;

			string insertQueryText = @"INSERT INTO MSD_WorkFlowActivity
                                          ( 
											  WorkFlowId,
                                              ActivityName, 
                                              ActivityDescription
                                           ) 
                                           VALUES 
                                           (
											 @WorkFlowId,
											 @ActivityName,
                                             @ActivityDescription
                                            )"; 

			try
			{
				
				insertSqlCommand = new SqlCommand(insertQueryText , connection);
				insertSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId",			 workFlowId));
				insertSqlCommand.Parameters.Add(new SqlParameter("@ActivityName",		 activityName));
				insertSqlCommand.Parameters.Add(new SqlParameter("@ActivityDescription", activityDescription));

				insertSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
				insertSqlCommand.Transaction = insertSqlTransaction;

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
				
				Debug.Fail("Exception raised in WorkFlowActivity.Insert: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowActivityFailedMsgFmt, activityName), exception);
			}

			return true;
		}

		//---------------------------------------------------------------------
		public bool Delete(SqlConnection connection)
		{
			
			SqlTransaction  deleteSqlTransaction = null;
			SqlCommand deleteSqlCommand = null;

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowActivity  
										WHERE ActivityId = @ActivityId AND
                                              WorkFlowId = @WorkFlowId";
			try
			{
				
				deleteSqlCommand				= new SqlCommand(deleteQueryText, connection);

				deleteSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId", this.workFlowId));
				deleteSqlCommand.Parameters.Add(new SqlParameter("@ActivityId", this.activityId));
				

				deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
				deleteSqlCommand.Transaction = deleteSqlTransaction;


				deleteSqlCommand.ExecuteNonQuery();

				deleteSqlTransaction.Commit();

				
			}
			catch (Exception exception)
			{
				if (deleteSqlCommand != null)
					deleteSqlCommand.Dispose();

				if (deleteSqlTransaction != null)
					deleteSqlTransaction.Rollback();

				Debug.Fail("Exception raised in WorkFlowActivity.Delete: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowActivityDeleteFailedMsgFmt, activityName), exception);
			}
			finally
			{
				if (deleteSqlCommand != null)
					deleteSqlCommand.Dispose();

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
		//---------------------------------------------------------------------
		public bool DeleteAll(SqlConnection connection)
		{
			
			SqlCommand deleteSqlCommand = null;
			SqlTransaction  deleteSqlTransaction = null;

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowActivity  
										WHERE 
										WorkFlowId = @WorkFlowId";

			try
			{
				deleteSqlCommand = new SqlCommand(deleteQueryText, connection);

				deleteSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId", this.workFlowId));
				

				deleteSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
				deleteSqlCommand.Transaction = deleteSqlTransaction;


				deleteSqlCommand.ExecuteNonQuery();

				deleteSqlTransaction.Commit();
			}
			catch (Exception exception)
			{
				if (deleteSqlCommand != null)
					deleteSqlCommand.Dispose();

				if (deleteSqlTransaction != null)
					deleteSqlTransaction.Rollback();

				Debug.Fail("Exception raised in WorkFlowActivity.DeleteAll: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowActivityDeleteFailedMsgFmt, ActivityName), exception);
			}
			finally
			{
				if (deleteSqlCommand != null)
					deleteSqlCommand.Dispose();

				if (deleteSqlTransaction != null)
					deleteSqlTransaction.Dispose();
			}


			return true;
			
		}

		//---------------------------------------------------------------------
		public bool Modify(SqlConnection connection)
		{
			SqlCommand updateSqlCommand = null;
			SqlTransaction  updateSqlTransaction = null;

			string updateQueryText = @"UPDATE MSD_WorkFlowActivity
                                       SET ActivityName			= @ActivityName,
										   ActivityDescription	= @ActivityDescription 
										WHERE 
                                           ActivityId	= @ActivityId AND
                                           WorkFlowId	= @WorkFlowId ";

			try
			{
				updateSqlCommand = new SqlCommand(updateQueryText, connection);

				updateSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId",				this.workFlowId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@ActivityId",				this.activityId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@ActivityName",			this.activityName));
				updateSqlCommand.Parameters.Add(new SqlParameter("@ActivityDescription",	this.activityDescription));

				updateSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
				updateSqlCommand.Transaction = updateSqlTransaction;

				updateSqlCommand.ExecuteNonQuery();

				updateSqlTransaction.Commit();
			}
			catch (Exception exception)
			{
				if (updateSqlCommand != null)
					updateSqlCommand.Dispose();

				if (updateSqlTransaction != null)
					updateSqlTransaction.Rollback();

				Debug.Fail("Exception raised in WorkFlowActivity.Update: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowActivityUpdateFailedMsgFmt, activityName), exception);
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

		//---------------------------------------------------------------------
		public bool Clone(SqlConnection connection)
		{
			return true;
		}

		//---------------------------------------------------------------------
		public bool Update(SqlConnection connection)
		{
			if (this.activityId == -1)
				return Insert(connection);
			else
				return Modify(connection);
		}
	}
}
