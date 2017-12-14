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
	/// Summary description for WorkFlowTemplate.
	/// </summary>
	public class WorkFlowTemplate
	{
		public const string WorkFlowTemplateTableName			= "MSD_WorkFlowTemplate";
		public const string TemplateIdColumnName				= "TemplateId";
		public const string TemplateNameColumnName				= "TemplateName";
		public const string TemplateDescriptionColumnName       = "TemplateDescription";
		
		private int		templateId			= 0;
		private string	templateName		= string.Empty;
		private string	templateDescription = string.Empty;
		private string	connectionString	= string.Empty;

		
		

		//---------------------------------------------------------------------
		public int		TemplateId			{ get { return templateId; } }  
		//---------------------------------------------------------------------
		public string	TemplateName		{ get { return templateName; } }  
		//---------------------------------------------------------------------
		public string	TemplateDescription	{ get { return templateDescription; } }  

		//---------------------------------------------------------------------
		public WorkFlowTemplate() : this(-1, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowTemplate(int templateId) : this(templateId, string.Empty, string.Empty)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowTemplate(int templateId, string templateName, string templateDescription)
		{
			this.templateId				= templateId;
			this.templateName			= templateName;
			this.templateDescription	= templateDescription;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WorkFlowTemplate(DataRow aWorkFlowTemplateTableRow, string connectionString)
		{
			if (aWorkFlowTemplateTableRow == null || string.Compare(aWorkFlowTemplateTableRow.Table.TableName, WorkFlowTemplateTableName, true) != 0)
				return;

			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WorkFlowTemplate Constructor Error: null or empty connection string.");
				throw new WorkFlowException(WorkFlowObjectStrings.EmptyConnectionStringMsg);
			}
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();
				//leggo le info dalla row del datagrid
				templateId			= (int)		aWorkFlowTemplateTableRow[TemplateIdColumnName];
				templateName		= (string)	aWorkFlowTemplateTableRow[TemplateNameColumnName];
				templateDescription = (string)	aWorkFlowTemplateTableRow[TemplateDescriptionColumnName];
				
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WorkFlowTemplate constructor: " + exception.Message);
				throw new WorkFlowException(WorkFlowObjectStrings.InvalidWorkFlowTemplateConstruction, exception);
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
		public static string GetSelectAllWorkFlowTemplateOrderedByNameQuery()
		{
			string queryText =  @"SELECT * FROM " + WorkFlowTemplateTableName + " ORDER BY " + TemplateNameColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetSelectWorkFlowTemplateOrderedByNameQuery(int templateId)
		{
			string queryText =  @"SELECT * FROM " + WorkFlowTemplateTableName + " WHERE templateId = " + templateId.ToString() + " ORDER BY " + TemplateNameColumnName;

			return queryText;
		}

		//---------------------------------------------------------------------
		public static bool IsValidTemplateName(string candidateName, string connectionString)
		{
			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WorkFlowTemplate.IsValidTemplateName Error: empty connection string.");
				throw new WorkFlowException(WorkFlowObjectStrings.EmptyConnectionStringMsg);
			}

			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();

				return IsValidTemplateName(candidateName, connection);
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WorkFlowTemplate.IsValidTemplateName: " + exception.Message);
				throw new WorkFlowException(WorkFlowObjectStrings.WorkFlowTemplateGenericExceptionMsg, exception);
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
		public static bool IsValidTemplateName(string aName, SqlConnection connection)
		{
			if (aName == null || aName == string.Empty)
				return false;

			if (connection == null || (connection.State & ConnectionState.Open) != ConnectionState.Open)
			{
				Debug.Fail("WorkFlowTemplate.IsValidTemplateName Error: invalid connection.");
				throw new WorkFlowException(WorkFlowObjectStrings.InvalidSqlConnectionErrMsg);
			}

			string query = String.Empty;
			SqlCommand selectCommand = null;
			int recordsCount = 0;

			try
			{
				query = "SELECT COUNT(*) FROM " + WorkFlowTemplateTableName + " WHERE ";
				query += TemplateNameColumnName + " = '" + aName + "'" ;
				selectCommand = new SqlCommand(query, connection);
					
				recordsCount = (int)selectCommand.ExecuteScalar();
			}
			catch(SqlException exception)
			{
				throw new WorkFlowException(string.Format(WorkFlowObjectStrings.WorkFlowTemplateGenericExceptionMsg, aName), exception);
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

			string insertQueryText = @"INSERT INTO MSD_WorkFlowTemplate
                                          ( 
                                              TemplateName, 
                                              TemplateDescription
                                           ) 
                                           VALUES 
                                           (
											 @TemplateName,
                                             @TemplateDescription
                                            )"; 

			string getIdQueryText = @" SELECT MAX(TemplateId) FROM MSD_WorkFlowTemplate";
                                       

			try
			{
				
				insertSqlCommand = new SqlCommand(insertQueryText , connection);
				insertSqlCommand.Parameters.Add(new SqlParameter("@TemplateName",		 templateName));
				insertSqlCommand.Parameters.Add(new SqlParameter("@TemplateDescription", templateDescription));

				insertSqlTransaction = connection.BeginTransaction(IsolationLevel.Serializable);
				insertSqlCommand.Transaction = insertSqlTransaction;

				insertSqlCommand.ExecuteNonQuery();

				insertSqlTransaction.Commit();

				insertSqlCommand.Dispose();
				insertSqlTransaction.Dispose();
				
				getIdSqlCommand = new SqlCommand(getIdQueryText, connection);
				templateId = (int) getIdSqlCommand.ExecuteScalar();

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
				
				Debug.Fail("Exception raised in WorkFlowTemplate.Insert: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateFailedMsgFmt, templateName), exception);
			}
			return true;
		}

		//---------------------------------------------------------------------
		public bool Delete(SqlConnection connection)
		{
			SqlCommand deleteSqlCommand = null;
			SqlTransaction  deleteSqlTransaction = null;

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowTemplate  
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

				Debug.Fail("Exception raised in WorkFlowTemplate.Delete: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateDeleteFailedMsgFmt, templateName), exception);
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

			string updateQueryText = @"UPDATE MSD_WorkFlowTemplate 
                                       SET TemplateName	= @TemplateName,
										   TemplateDescription = @TemplateDescription 
										WHERE 
										TemplateId = @TemplateId";

			try
			{
				updateSqlCommand = new SqlCommand(updateQueryText, connection);

				updateSqlCommand.Parameters.Add(new SqlParameter("@TemplateId", this.templateId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@TemplateName", this.templateName));
				updateSqlCommand.Parameters.Add(new SqlParameter("@TemplateDescription", this.templateDescription));

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

				Debug.Fail("Exception raised in WorkFlowTemplate.Update: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowTemplateUpdateFailedMsgFmt, templateName), exception);
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
		public bool Update(SqlConnection connection)
		{
			if (this.templateId == -1)
				return Insert(connection);
			else
				return Modify(connection);
		}

		//---------------------------------------------------------------------
		public bool Clone(SqlConnection connection)
		{
			return true;
		}

		
	}

	
}
