using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Microarea.Library.WorkFlowObjects
{
	/// <summary>
	/// Summary description for WorkFlow.
	/// </summary>
	public class WorkFlow
	{
		public const string WorkFlowTableName				= "MSD_WorkFlow";
		public const string CompanyIdColumnName				= "CompanyId";
		public const string TemplateIdColumnName			= "TemplateId";
		public const string WorkFlowIdColumnName			= "WorkFlowId";
		public const string WorkFlowNameColumnName			= "WorkFlowName";
		public const string WorkFlowDescColumnName			= "WorkFlowDescription";
		public const string EnabledColumnName				= "Enabled";

		private int		companyId			= -1;
		private int     workFlowId			= -1;
		private int     templateId          = -1;
		private string  workFlowName		= string.Empty;
		private string  workFlowDescription = string.Empty;

		//---------------------------------------------------------------------
		public int		CompanyId			{ get { return companyId; } }
		//---------------------------------------------------------------------
		public int		WorkFlowId			{ get { return workFlowId; } }  
		//---------------------------------------------------------------------
		public int		TemplateId			{ get { return templateId; } }  
		//---------------------------------------------------------------------
		public string	WorkFlowName		{ get { return workFlowName; } }  
		//---------------------------------------------------------------------
		public string	WorkFlowDescription	{ get { return workFlowDescription; } }  

		//---------------------------------------------------------------------
		public WorkFlow(int companyId): this(companyId, -1, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlow() : this(-1, -1, -1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlow(int companyId, int workFlowId, int templateId, string workFlowName, string workFlowDescription)
		{
			this.companyId				= companyId;
			this.workFlowId				= workFlowId;
			this.templateId             = templateId;
			this.workFlowName			= workFlowName;
			this.workFlowDescription	= workFlowDescription;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WorkFlow(DataRow aWorkFlowTableRow, string connectionString)
		{
			if (aWorkFlowTableRow == null || string.Compare(aWorkFlowTableRow.Table.TableName, WorkFlowTableName, true) != 0)
				return;

			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WorkFlow Constructor Error: null or empty connection string.");
				throw new WorkFlowException(WorkFlowObjectStrings.EmptyConnectionStringMsg);
			}
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();
				//leggo le info dalla row del datagrid
				workFlowId			= (int)		aWorkFlowTableRow[WorkFlowIdColumnName];
				templateId          = (int)     aWorkFlowTableRow[TemplateIdColumnName];
				companyId			= (int)		aWorkFlowTableRow[CompanyIdColumnName];
				workFlowName		= (string)	aWorkFlowTableRow[WorkFlowNameColumnName];
				workFlowDescription = (string)	aWorkFlowTableRow[WorkFlowDescColumnName];
				
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WorkFlow constructor: " + exception.Message);
				throw new WorkFlowException(WorkFlowObjectStrings.InvalidWorkFlowConstruction, exception);
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
		public static string GetSelectAllCompanyWorkFlowsOrderedByNameQuery(int aCompanyId)
		{
			string queryText =  @"SELECT * FROM " + WorkFlowTableName + " WHERE ";

			queryText += CompanyIdColumnName + " = " + aCompanyId.ToString();
			queryText += " ORDER BY " + WorkFlowNameColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectAllWorkFlowsOrderedByNameQuery()
		{
			string queryText =  @"SELECT * FROM " + WorkFlowTableName + " ORDER BY " + WorkFlowNameColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectWorkFlowOrderedByNameQuery(int aCompanyId, int aWorkflowId)
		{
			string queryText =  @"SELECT * FROM " + WorkFlowTableName + " WHERE ";

			queryText += CompanyIdColumnName + " = " + aCompanyId.ToString() + " AND ";
			queryText += WorkFlowIdColumnName + " = " + aWorkflowId.ToString();

			queryText += " ORDER BY " + WorkFlowNameColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static bool IsValidWorkFlowName(string candidateName, string connectionString)
		{
			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WorkFlow.IsValidWorkFlowName Error: empty connection string.");
				throw new WorkFlowException(WorkFlowObjectStrings.EmptyConnectionStringMsg);
			}

			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();

				return IsValidWorkFlowName(candidateName, connection);
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WorkFlow.IsValidWorkFlowName: " + exception.Message);
				throw new WorkFlowException(WorkFlowObjectStrings.WorkFlowGenericExceptionMsg, exception);
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
		public static bool IsValidWorkFlowName(string aName, SqlConnection connection)
		{
			if (aName == null || aName == string.Empty)
				return false;

			if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
			{
				Debug.Fail("WorkFlow.IsValidWorkFlowName Error: invalid connection.");
				throw new WorkFlowException(WorkFlowObjectStrings.InvalidSqlConnectionErrMsg);
			}

			string query = String.Empty;
			SqlCommand selectCommand = null;
			int recordsCount = 0;

			try
			{
				query = "SELECT COUNT(*) FROM " + WorkFlowTableName + " WHERE ";
				query += WorkFlowNameColumnName + " = '" + aName + "'" ;
				selectCommand = new SqlCommand(query, connection);
					
				recordsCount = (int)selectCommand.ExecuteScalar();
			}
			catch(SqlException exception)
			{
				throw new WorkFlowException(string.Format(WorkFlowObjectStrings.WorkFlowGenericExceptionMsg, aName), exception);
			}
			finally
			{
				if (selectCommand != null)
					selectCommand.Dispose();						
			}
			return (recordsCount == 0);
		}
	


		//---------------------------------------------------------------------
		public bool Insert(SqlConnection connection)
		{
			SqlCommand insertSqlCommand = null;
			SqlCommand getIdSqlCommand = null;
			SqlTransaction  insertSqlTransaction = null;

			string insertQueryText = @"INSERT INTO MSD_WorkFlow
                                          ( 
											  CompanyId,
                                              TemplateId,
                                              WorkFlowName, 
                                              WorkFlowDescription
                                           ) 
                                           VALUES 
                                           (
											 @CompanyId,
                                             @TemplateId,
											 @WorkFlowName,
                                             @WorkFlowDescription
                                            )"; 

			string getIdQueryText = @" SELECT MAX(WorkFlowId) FROM MSD_WorkFlow WHERE CompanyId = @CompanyId";
                                       

			try
			{
				
				insertSqlCommand = new SqlCommand(insertQueryText , connection);
				insertSqlCommand.Parameters.Add(new SqlParameter("@CompanyId",				this.companyId));
				insertSqlCommand.Parameters.Add(new SqlParameter("@TemplateId",				this.templateId));
				insertSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowName",			this.workFlowName));
				insertSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowDescription",	this.workFlowDescription));

				insertSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
				insertSqlCommand.Transaction = insertSqlTransaction;

				insertSqlCommand.ExecuteNonQuery();

				insertSqlTransaction.Commit();

				insertSqlCommand.Dispose();
				insertSqlTransaction.Dispose();
				
				getIdSqlCommand = new SqlCommand(getIdQueryText, connection);
				getIdSqlCommand.Parameters.Add(new SqlParameter("@CompanyId",				this.companyId));
				workFlowId = (int) getIdSqlCommand.ExecuteScalar();

				getIdSqlCommand.Dispose();
			}
			catch (Exception exception)
			{
				if (insertSqlCommand != null)
					insertSqlCommand.Dispose();
				if (getIdSqlCommand != null)
					getIdSqlCommand.Dispose();
				if (insertSqlTransaction != null)
				{
					insertSqlTransaction.Rollback();
					insertSqlTransaction.Dispose();
				}
				
				Debug.Fail("Exception raised in WorkFlow.Insert: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowFailedMsgFmt, workFlowName), exception);
			}
			return true;
		}

		

		//---------------------------------------------------------------------
		public bool Modify(SqlConnection connection)
		{
			SqlCommand updateSqlCommand = null;
			SqlTransaction  updateSqlTransaction = null;

			string updateQueryText = @"UPDATE MSD_WorkFlow 
                                       SET WorkFlowName			= @WorkFlowName,
										   WorkFlowDescription	= @WorkFlowDescription,
                                           TemplateId			= @TemplateId 
										WHERE 
										WorkFlowId = @WorkFlowId AND
                                        CompanyId  = @CompanyId ";

			try
			{
				updateSqlCommand = new SqlCommand(updateQueryText, connection);

				updateSqlCommand.Parameters.Add(new SqlParameter("@CompanyId",				this.companyId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId",				this.workFlowId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@TemplateId",				this.templateId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowName",			this.workFlowName));
				updateSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowDescription",	this.workFlowDescription));

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

				Debug.Fail("Exception raised in WorkFlow.Modify: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowUpdateFailedMsgFmt, workFlowName), exception);
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
		public bool Delete(SqlConnection connection)
		{
			
			SqlTransaction  deleteSqlTransaction = null;
			SqlCommand deleteSqlCommand = null;

			string deleteQueryText		= @"DELETE FROM MSD_WorkFlow
                                            WHERE WorkFlowId = @WorkFlowId AND
												  CompanyId  = @CompanyId ";

			try
			{
				

				//Cancello WorkFlow
				deleteSqlCommand				= new SqlCommand(deleteQueryText, connection);

				deleteSqlCommand.Parameters.Add(new SqlParameter("@CompanyId",				this.companyId));
				deleteSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId",				this.workFlowId));

				deleteSqlTransaction			= connection.BeginTransaction(IsolationLevel.Serializable);
				deleteSqlCommand.Transaction	= deleteSqlTransaction;

				deleteSqlCommand.ExecuteNonQuery();

				deleteSqlTransaction.Commit();

				
			}
			catch (Exception exception)
			{
				if (deleteSqlCommand != null)
					deleteSqlCommand.Dispose();

				if (deleteSqlTransaction != null)
					deleteSqlTransaction.Rollback();

				Debug.Fail("Exception raised in WorkFlow.Delete: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowDeleteFailedMsgFmt, workFlowName), exception);
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
		public bool Clone(SqlConnection connection)
		{
			return true;
		}

		//---------------------------------------------------------------------
		public bool Update(SqlConnection connection)
		{
			if (this.workFlowId == -1)
				return Insert(connection);
			else
				return Modify(connection);
		}
	}

	
}
