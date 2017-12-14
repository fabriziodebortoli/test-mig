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
	/// Summary description for WorkFlowState.
	/// </summary>
	public class WorkFlowState
	{
		public static string WorkFlowStateTableName				= "MSD_WorkFlowState";
		public const string TemplateIdColumnName				= "TemplateId";
		public const string CompanyIdColumnName					= "CompanyId";
		public const string StateIdColumnName					= "StateId";
		public const string WorkFlowIdColumnName				= "WorkFlowId";
		public const string StateNameColumnName					= "StateName";
		public const string StateDescriptionColumnName			= "StateDescription";

		private int		companyId			= -1;
		private int		workFlowId			= -1;

		private int		stateId				= -1;
		private string	stateName			= string.Empty;
		private string	stateDescription	= string.Empty;
		private string  workFlowName        = string.Empty;

		//---------------------------------------------------------------------
		public int		CompanyId			{ get { return companyId; } }
		//---------------------------------------------------------------------
		public int		WorkFlowId			{ get { return workFlowId; } }  
		//---------------------------------------------------------------------
		public int		StateId				{ get { return stateId; } }  
		//---------------------------------------------------------------------
		public string	StateName			{ get { return stateName; } }  
		//---------------------------------------------------------------------
		public string	StateDescription	{ get { return stateDescription; } } 

		//---------------------------------------------------------------------
		public string	WorkFlowName	{ get { return workFlowName; } set { workFlowName = value; }} 

		//---------------------------------------------------------------------
		public WorkFlowState() : this(-1, -1, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowState(int aWorkFlowId) : this(-1, aWorkFlowId, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowState(int aCompanyId, int aWorkFlowId) : this(aCompanyId, aWorkFlowId, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowState(int aCompanyId, int aWorkFlowId, int aStateId, string aStateName, string aStateDescription)
		{
			this.companyId				= aCompanyId;
			this.workFlowId				= aWorkFlowId;
			this.stateId				= aStateId;
			this.stateName				= aStateName;
			this.stateDescription		= aStateDescription;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WorkFlowState(DataRow aWorkFlowStateTableRow, int aCompanyId, string connectionString)
		{
			if (aWorkFlowStateTableRow == null || string.Compare(aWorkFlowStateTableRow.Table.TableName, WorkFlowStateTableName, true) != 0)
				return;

			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WorkFlowState Constructor Error: null or empty connection string.");
				throw new WorkFlowException(WorkFlowObjectStrings.EmptyConnectionStringMsg);
			}
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();
				//leggo le info dalla row del datagrid
				workFlowId			= (int)		aWorkFlowStateTableRow[WorkFlowIdColumnName];
				companyId			= aCompanyId;
				stateId				= (int)		aWorkFlowStateTableRow[StateIdColumnName];
				stateName			= (string)	aWorkFlowStateTableRow[StateNameColumnName];
				stateDescription	= (string)	aWorkFlowStateTableRow[StateDescriptionColumnName];
				
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WorkFlowState constructor: " + exception.Message);
				throw new WorkFlowException(WorkFlowObjectStrings.InvalidWorkFlowStateConstruction, exception);
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
		public static string GetSelectTemplateStateOrderedByNameQuery(int templateId)
		{
			string queryText =  @"SELECT StateName, StateDescription FROM " + WorkFlowTemplateState.WorkFlowTemplateStateTableName + " WHERE ";
				
			queryText += WorkFlowTemplateState.WorkFlowTemplateStateTableName + "."  + TemplateIdColumnName + " = " + templateId.ToString();
			queryText += " ORDER BY " + WorkFlowTemplateState.StateNameColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectAllStatesOrderedByNameQuery(int aCompanyId, int aWorkFlowId)
		{
			string queryText =  "SELECT * FROM " + WorkFlowStateTableName + " INNER JOIN MSD_WorkFlow ON MSD_WorkFlow.WorkFlowId = MSD_WorkFlowState.WorkFlowId WHERE "; 
			queryText += CompanyIdColumnName + " = " + aCompanyId.ToString() + " AND ";
			queryText += WorkFlowStateTableName + "." + WorkFlowIdColumnName + " = " + aWorkFlowId.ToString();

			queryText +=  " ORDER BY " + StateIdColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public bool Update(SqlConnection connection)
		{
			if (this.stateId == -1)
				return Insert(connection);
			else
				return Modify(connection);
		}

		//---------------------------------------------------------------------
		public bool Insert(SqlConnection connection)
		{
			SqlCommand insertSqlCommand = null;
			SqlTransaction  insertSqlTransaction = null;

			string insertQueryText = @"INSERT INTO MSD_WorkFlowState
                                          ( 
											  WorkFlowId,
                                              StateName, 
                                              StateDescription
                                           ) 
                                           VALUES 
                                           (
											 @WorkFlowId,
											 @StateName,
                                             @StateDescription
                                            )"; 

			try
			{
				
				insertSqlCommand = new SqlCommand(insertQueryText , connection);
				insertSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId",			 workFlowId));
				insertSqlCommand.Parameters.Add(new SqlParameter("@StateName",			 stateName));
				insertSqlCommand.Parameters.Add(new SqlParameter("@StateDescription",	 stateDescription));

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
				
				Debug.Fail("Exception raised in WorkFlowState.Insert: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowStateFailedMsgFmt, stateName), exception);
			}

			return true;
		}

		//---------------------------------------------------------------------
		public bool Modify(SqlConnection connection)
		{
			SqlCommand updateSqlCommand = null;
			SqlTransaction  updateSqlTransaction = null;

			string updateQueryText = @"UPDATE MSD_WorkFlowState
                                       SET StateName			= @StateName,
										   StateDescription		= @StateDescription 
										WHERE 
                                           StateId		= @StateId AND
										   WorkFlowId	= @WorkFlowId ";

			try
			{
				updateSqlCommand = new SqlCommand(updateQueryText, connection);

				updateSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId",				this.workFlowId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@StateId",				this.stateId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@StateName",				this.stateName));
				updateSqlCommand.Parameters.Add(new SqlParameter("@StateDescription",		this.stateDescription));

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

				Debug.Fail("Exception raised in WorkFlowState.Modify: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowStateUpdateFailedMsgFmt, stateName), exception);
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

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowState  
										WHERE WorkFlowId = @WorkFlowId AND
                                              StateId    = @StateId";

			try
			{
				deleteSqlCommand = new SqlCommand(deleteQueryText, connection);

				deleteSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId", this.workFlowId));
				deleteSqlCommand.Parameters.Add(new SqlParameter("@StateId", this.stateId));
				

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

				Debug.Fail("Exception raised in WorkFlowState.Delete: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowStateDeleteFailedMsgFmt, StateName), exception);
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

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowState  
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

				Debug.Fail("Exception raised in WorkFlowState.DeleteAll: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowStateDeleteFailedMsgFmt, StateName), exception);
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

	}
}
