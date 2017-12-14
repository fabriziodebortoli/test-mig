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
	/// WorkFlowTemplateState.
	/// </summary>
	// ========================================================================
	public class WorkFlowTemplateState
	{
		public const string WorkFlowTemplateStateTableName		= "MSD_WorkFlowTemplateState";
		public const string TemplateIdColumnName				= "TemplateId";
		public const string StateIdColumnName					= "StateId";
		public const string StateNameColumnName					= "StateName";
		public const string StateDescriptionColumnName			= "StateDescription";

		public const int    TemplateBaseId = 1;
		public string		TemplateName   = string.Empty;

		private int		templateId			= 0;
		private int		stateId				= 0;
		
		private string	stateName			= string.Empty;
		private string	stateDescription	= string.Empty;
		private string	connectionString	= string.Empty;

		

		//---------------------------------------------------------------------
		public int		TemplateId			{ get { return templateId; } }  
		//---------------------------------------------------------------------
		public int		StateId			{ get { return stateId; } }  
		//---------------------------------------------------------------------
		public string	StateName		{ get { return stateName; } }  
		//---------------------------------------------------------------------
		public string	StateDescription	{ get { return stateDescription; } }  

		//---------------------------------------------------------------------
		public WorkFlowTemplateState() : this(-1, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowTemplateState(int templateId) : this(templateId, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowTemplateState(int templateId, int stateId) : this(templateId, stateId, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowTemplateState(int templateId, int stateId, string stateName, string stateDescription) 
		{
			this.templateId			= templateId;
			this.stateId			= stateId;
			this.stateName			= stateName;
			this.stateDescription	= stateDescription;
			
		}
		//---------------------------------------------------------------------
		public WorkFlowTemplateState(DataRow aWorkFlowTemplateStateTableRow, string connectionString)
		{
			if (aWorkFlowTemplateStateTableRow == null || string.Compare(aWorkFlowTemplateStateTableRow.Table.TableName, WorkFlowTemplateStateTableName, true) != 0)
				return;

			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WorkFlowTemplateState Constructor Error: null or empty connection string.");
				throw new WorkFlowException(WorkFlowObjectStrings.EmptyConnectionStringMsg);
			}

			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();
				//leggo le info dalla row del datagrid
				templateId			= (int)		aWorkFlowTemplateStateTableRow[TemplateIdColumnName];
				stateId				= (int)		aWorkFlowTemplateStateTableRow[StateIdColumnName];
				stateName			= (string)	aWorkFlowTemplateStateTableRow[StateNameColumnName];
				stateDescription	= (string)	aWorkFlowTemplateStateTableRow[StateDescriptionColumnName];
				
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WorkFlowTemplateState constructor: " + exception.Message);
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
		public static string GetSelectAllTemplateStatesOrderedByNameQuery(int templateId)
		{
			
			string queryText =  @"SELECT * FROM " + WorkFlowTemplateStateTableName + " WHERE ";
				
			queryText += TemplateIdColumnName + " = " + templateId.ToString();
			queryText += " ORDER BY " + StateIdColumnName;

			return queryText;
		}
		
		//---------------------------------------------------------------------
		public static string GetSelectAllBaseTemplateStatesOrderedByNameQuery()
		{
			
			string queryText =  @"SELECT * FROM " + WorkFlowTemplateStateTableName + " WHERE ";
				
			queryText += TemplateIdColumnName + " = " + TemplateBaseId.ToString();
			queryText += " ORDER BY " + StateIdColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectTemplateState(int templateId, int stateId)
		{
			string queryText =  @"SELECT COUNT(*) FROM " + WorkFlowTemplateStateTableName + " WHERE ";
				
			queryText += TemplateIdColumnName + " = " + templateId.ToString();
			queryText += " AND " + StateIdColumnName + " = " + stateId.ToString();

			return queryText;
		}

		//---------------------------------------------------------------------
		public bool Insert(SqlConnection connection)
		{

			SqlCommand insertSqlCommand = null;
			SqlTransaction  insertSqlTransaction = null;

			string insertQueryText = @"INSERT INTO MSD_WorkFlowTemplateState
                                          ( 
											  TemplateId,
                                              StateName, 
                                              StateDescription
                                           ) 
                                           VALUES 
                                           (
											 @TemplateId,
											 @StateName,
                                             @StateDescription
                                            )";
 
			try
			{
				
				insertSqlCommand = new SqlCommand(insertQueryText , connection);
				insertSqlCommand.Parameters.Add(new SqlParameter("@TemplateId",			 templateId));
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
				
				Debug.Fail("Exception raised in WorkFlowTemplateState.Insert: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateStateFailedMsgFmt, stateName), exception);
			}
			return true;
		}

		//---------------------------------------------------------------------
		public bool Modify(SqlConnection connection)
		{
			SqlCommand updateSqlCommand = null;
			SqlTransaction  updateSqlTransaction = null;

			string updateQueryText = @"UPDATE MSD_WorkFlowTemplateState
                                       SET StateName	= @StateName,
										   StateDescription = @StateDescription 
										WHERE 
                                           StateId = @StateId AND
										   TemplateId = @TemplateId";

			try
			{
				updateSqlCommand = new SqlCommand(updateQueryText, connection);

				updateSqlCommand.Parameters.Add(new SqlParameter("@TemplateId",			this.templateId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@StateId",			this.stateId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@StateName",			this.stateName));
				updateSqlCommand.Parameters.Add(new SqlParameter("@StateDescription",	this.stateDescription));


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

				Debug.Fail("Exception raised in WorkFlowTemplateState.Update: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateStateUpdateFailedMsgFmt, stateName), exception);
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

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowTemplateState  
										WHERE TemplateId = @TemplateId AND
                                              StateId    = @StateId";

			try
			{
				deleteSqlCommand = new SqlCommand(deleteQueryText, connection);

				deleteSqlCommand.Parameters.Add(new SqlParameter("@TemplateId", this.templateId));
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

				Debug.Fail("Exception raised in WorkFlowTemplateState.Delete: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateStateDeleteFailedMsgFmt, TemplateName), exception);
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

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowTemplateState  
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

				Debug.Fail("Exception raised in WorkFlowTemplateState.Delete: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateStateDeleteFailedMsgFmt, TemplateName), exception);
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
			if (IsNewState(connection))
				return Insert(connection);
			else
				return Modify(connection);
		}

		//---------------------------------------------------------------------
		private bool IsNewState(SqlConnection connection)
		{
			bool isNew = false;

			SqlCommand updateSqlCommand = null;

			string updateQueryText = GetSelectTemplateState(templateId, stateId);

			try
			{
				updateSqlCommand = new SqlCommand(updateQueryText, connection);

				isNew = ( ((int)updateSqlCommand.ExecuteScalar()) <= 0) ;

				
			}
			catch (Exception exception)
			{
				if (updateSqlCommand != null)
					updateSqlCommand.Dispose();


				Debug.Fail("Exception raised in WorkFlowTemplateState.IsNewState: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateStateFailedMsgFmt, stateName), exception);
			}
			finally
			{
				if (updateSqlCommand != null)
					updateSqlCommand.Dispose();

			}


			return isNew;
		}

		//---------------------------------------------------------------------
		public bool Clone(SqlConnection connection)
		{
			return true;
		}
	}
}
