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
	/// WorkFlowTemplateActivity.
	/// </summary>
	// ========================================================================
	public class WorkFlowTemplateActivity
	{
		public const string WorkFlowTemplateActionTableName		= "MSD_WorkFlowTemplateActivity";
		public const string TemplateIdColumnName				= "TemplateId";
		public const string ActivityIdColumnName				= "ActivityId";
		public const string ActivityNameColumnName				= "ActivityName";
		public const string ActivityDescriptionColumnName       = "ActivityDescription";

		public string  TemplateName        = string.Empty;

		private int		templateId			= 0;
		private int		activityId			= 0;
		private string	activityName		= string.Empty;
		private string	activityDescription = string.Empty;
		private string	connectionString	= string.Empty;

		//---------------------------------------------------------------------
		public int		TemplateId			{ get { return templateId; } }  
		//---------------------------------------------------------------------
		public int		ActivityId			{ get { return activityId; } }  
		//---------------------------------------------------------------------
		public string	ActivityName		{ get { return activityName; } }  
		//---------------------------------------------------------------------
		public string	ActivityDescription	{ get { return activityDescription; } }  

		//---------------------------------------------------------------------
		public WorkFlowTemplateActivity() : this(-1, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowTemplateActivity(int templateId) : this(templateId, -1, string.Empty, string.Empty)
		{
		}
		//---------------------------------------------------------------------
		public WorkFlowTemplateActivity(int templateId, int activityId, string activityName, string activityDescription)
		{
			this.templateId				= templateId;
			this.activityId				= activityId;
			this.activityName			= activityName;
			this.activityDescription	= activityDescription;
			
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WorkFlowTemplateActivity(DataRow aWorkFlowTemplateActiviyTableRow, string connectionString)
		{
			if (aWorkFlowTemplateActiviyTableRow == null || string.Compare(aWorkFlowTemplateActiviyTableRow.Table.TableName, WorkFlowTemplateActionTableName, true) != 0)
				return;


			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WorkFlowTemplateActivity Constructor Error: null or empty connection string.");
				throw new WorkFlowException(WorkFlowObjectStrings.EmptyConnectionStringMsg);
			}

			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();
				//leggo le info dalla row del datagrid
				templateId			= (int)		aWorkFlowTemplateActiviyTableRow[TemplateIdColumnName];
				activityId			= (int)		aWorkFlowTemplateActiviyTableRow[ActivityIdColumnName];
				activityName		= (string)	aWorkFlowTemplateActiviyTableRow[ActivityNameColumnName];
				activityDescription = (string)	aWorkFlowTemplateActiviyTableRow[ActivityDescriptionColumnName];
				
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
		public static string GetSelectAllTemplateActivitiesOrderedByNameQuery()
		{
			int templateBaseId = 1;
			string queryText =  @"SELECT * FROM " + WorkFlowTemplateActionTableName + " WHERE ";
				
			queryText += TemplateIdColumnName + " = " + templateBaseId.ToString();
			queryText += " ORDER BY " + ActivityIdColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectAllTemplateActivitiesOrderedByNameQuery(int templateId)
		{
			string queryText =  @"SELECT * FROM " + WorkFlowTemplateActionTableName + " WHERE ";
				
			queryText += TemplateIdColumnName + " = " + templateId.ToString();
			queryText += " ORDER BY " + ActivityIdColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectTemplateActivity(int templateId, int activityId)
		{
			string queryText =  @"SELECT COUNT(*) FROM " + WorkFlowTemplateActionTableName + " WHERE ";
				
			queryText += TemplateIdColumnName + " = " + templateId.ToString();

			queryText += " AND " + ActivityIdColumnName + " = " + activityId.ToString();

			return queryText;
		}


		//---------------------------------------------------------------------
		public bool Insert(SqlConnection connection)
		{

			SqlCommand insertSqlCommand = null;
			SqlTransaction  insertSqlTransaction = null;

			string insertQueryText = @"INSERT INTO MSD_WorkFlowTemplateActivity
                                          ( 
											  TemplateId,
                                              ActivityName, 
                                              ActivityDescription
                                           ) 
                                           VALUES 
                                           (
											 @TemplateId,
											 @ActivityName,
                                             @ActivityDescription
                                            )"; 

			try
			{
				
				insertSqlCommand = new SqlCommand(insertQueryText , connection);
				insertSqlCommand.Parameters.Add(new SqlParameter("@TemplateId",			 templateId));
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
				
				Debug.Fail("Exception raised in WorkFlowTemplateActivity.Insert: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateActivityFailedMsgFmt, activityName), exception);
			}

			return true;
		}

		//---------------------------------------------------------------------
		public bool Modify(SqlConnection connection)
		{
			SqlCommand updateSqlCommand = null;
			SqlTransaction  updateSqlTransaction = null;

			string updateQueryText = @"UPDATE MSD_WorkFlowTemplateActivity
                                       SET ActivityName	= @ActivityName,
										   ActivityDescription = @ActivityDescription 
										WHERE 
                                           ActivityId = @ActivityId AND
										   TemplateId = @TemplateId";

			try
			{
				updateSqlCommand = new SqlCommand(updateQueryText, connection);

				updateSqlCommand.Parameters.Add(new SqlParameter("@TemplateId", this.templateId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@ActivityId", this.activityId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@ActivityName", this.activityName));
				updateSqlCommand.Parameters.Add(new SqlParameter("@ActivityDescription", this.activityDescription));

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

				Debug.Fail("Exception raised in WorkFlowTemplateActivity.Update: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateActivityUpdateFailedMsgFmt, activityName), exception);
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
		public bool Delete(SqlConnection connection)
		{
			SqlCommand deleteSqlCommand = null;
			SqlTransaction  deleteSqlTransaction = null;

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowTemplateActivity  
										WHERE ActivityId = @ActivityId AND
                                              TemplateId = @TemplateId";

			try
			{
				deleteSqlCommand = new SqlCommand(deleteQueryText, connection);

				deleteSqlCommand.Parameters.Add(new SqlParameter("@TemplateId", this.templateId));
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

				Debug.Fail("Exception raised in WorkFlowTemplateActivity.Delete: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateActivityDeleteFailedMsgFmt, TemplateName), exception);
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
		public bool DeleteAll(SqlConnection connection)
		{
			SqlCommand deleteSqlCommand = null;
			SqlTransaction  deleteSqlTransaction = null;

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowTemplateActivity  
										WHERE 
										TemplateId = @TemplateId";

			try
			{
				deleteSqlCommand = new SqlCommand(deleteQueryText, connection);

				deleteSqlCommand.Parameters.Add(new SqlParameter("@TemplateId", this.templateId));
				

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

				Debug.Fail("Exception raised in WorkFlowTemplateActivity.Delete: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateActivityDeleteFailedMsgFmt, TemplateName), exception);
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
		public bool Update(SqlConnection connection)
		{
			if (IsNewActivity(connection))
				return Insert(connection);
			else
				return Modify(connection);
		}

		//---------------------------------------------------------------------
		public bool Clone(SqlConnection connection)
		{
			return true;
		}

		//---------------------------------------------------------------------
		private bool IsNewActivity(SqlConnection connection)
		{
			bool isNew = false;

			SqlCommand updateSqlCommand = null;

			string updateQueryText = GetSelectTemplateActivity(templateId, activityId);

			try
			{
				updateSqlCommand = new SqlCommand(updateQueryText, connection);

				isNew =( ((int)updateSqlCommand.ExecuteScalar()) <= 0) ;

				
			}
			catch (Exception exception)
			{
				if (updateSqlCommand != null)
					updateSqlCommand.Dispose();


				Debug.Fail("Exception raised in WorkFlowTemplateActivity.IsNewActivity: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateActivityFailedMsgFmt, activityName), exception);
			}
			finally
			{
				if (updateSqlCommand != null)
					updateSqlCommand.Dispose();

			}


			return isNew;
		}
	}
}
