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
	/// WorkFlowUser.
	/// </summary>
	// ========================================================================
	public class WorkFlowUser
	{
		
		public const string CompanyUserTableName                = "MSD_CompanyUser";
		public const string WorkFlowUserTableName				= "MSD_WorkFlowUser";
		public const string CompanyIdColumnName					= "CompanyId";
		public const string UserIdColumnName					= "LoginId";
		public const string WorkFlowIdColumnName				= "WorkFlowId";
		public const string LoginNameColumnName                 = "LoginName";
		public const string LoginEMailColumnName                = "EMail";
		public const string WorkFlowMemberColumnName            = "WorkFlowMember";
		
		
		private int		companyId			= -1;
		private int		workFlowId			= -1;
		private int     loginId				= -1;
		private bool    isMember            = true; //default : tutti gli utenti dell'azienda partecipano al WF
		
		//dati letti da MSD_Logins
		private string  loginName			= string.Empty;
		private string  loginEMail			= string.Empty;

		//---------------------------------------------------------------------
		public int		CompanyId			{ get { return companyId; } }
		//---------------------------------------------------------------------
		public int		WorkFlowId			{ get { return workFlowId; } }  
		//---------------------------------------------------------------------
		public int		LoginId				{ get { return loginId; } }  
		//---------------------------------------------------------------------
		public string	LoginName			{ get { return loginName; } }  
		//---------------------------------------------------------------------
		public string	LoginEMail			{ get { return loginEMail; } }  
		//---------------------------------------------------------------------
		public bool     LoginIsMember       { get { return isMember; }}
		
		
		//---------------------------------------------------------------------
		public WorkFlowUser(): this(-1, -1, -1)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowUser(int companyId, int workFlowId): this(companyId, workFlowId, -1)
		{
		}

		//---------------------------------------------------------------------
		public WorkFlowUser(int companyId, int workFlowId, int loginId)
		{
			this.companyId	= companyId;
			this.workFlowId = workFlowId;
			this.loginId	= loginId;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public WorkFlowUser(DataRow aWorkFlowUserTableRow, string connectionString)
		{
			if (aWorkFlowUserTableRow == null || string.Compare(aWorkFlowUserTableRow.Table.TableName, WorkFlowUserTableName, true) != 0)
				return;

			if (connectionString == null || connectionString == string.Empty)
			{
				Debug.Fail("WorkFlowUser Constructor Error: null or empty connection string.");
				throw new WorkFlowException(WorkFlowObjectStrings.EmptyConnectionStringMsg);
			}
			SqlConnection connection = null;
			try
			{
				connection = new SqlConnection(connectionString);
				connection.Open();
				//leggo le info dalla row del datagrid
				workFlowId			= (int)		aWorkFlowUserTableRow[WorkFlowIdColumnName];
				loginId				= (int)		aWorkFlowUserTableRow[UserIdColumnName];
				loginName           = (string)	aWorkFlowUserTableRow[LoginNameColumnName];
				isMember			= (bool)	aWorkFlowUserTableRow[WorkFlowMemberColumnName];
			}
			catch (Exception exception)
			{
				Debug.Fail("Exception raised in WorkFlowUser constructor: " + exception.Message);
				throw new WorkFlowException(WorkFlowObjectStrings.InvalidWorkFlowUserConstruction, exception);
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
		public static string GetAllCompanyUsers(int aCompanyId)
		{
			string queryText =  @"SELECT MSD_CompanyLogins.LoginId, 
                                         MSD_CompanyLogins.DBUser as LoginName, 
                                         MSD_Logins.Email as EMail
                                  FROM	 MSD_CompanyLogins, MSD_Logins
                                  WHERE	 MSD_CompanyLogins.CompanyId = " + aCompanyId.ToString() + @" AND
                                         MSD_CompanyLogins.LoginId = MSD_Logins.LoginId";
				

			return queryText;
		}

		//---------------------------------------------------------------------
		public static string GetAllWorkFlowUser(int aCompanyId)
		{
			string queryText =  @"SELECT MSD_WorkFlowUser.LoginId, 
                                         MSD_CompanyLogins.DBUser as LoginName, 
                                         MSD_Logins.Email as EMail
                                  FROM	 MSD_CompanyLogins, MSD_Logins, MSD_WorkFlowUser
                                  WHERE	 MSD_CompanyLogins.CompanyId = " + aCompanyId.ToString() + @" AND
                                         MSD_CompanyLogins.LoginId = MSD_Logins.LoginId AND
                                         MSD_WorkFlowUser.LoginId = MSD_COmpanyLogins.LoginId";
				

			return queryText;
		}

		//---------------------------------------------------------------------
		public bool Insert(SqlConnection connection)
		{
			SqlCommand insertSqlCommand = null;
			SqlTransaction  insertSqlTransaction = null;

			string insertQueryText = @"INSERT INTO MSD_WorkFlowUser
                                          ( 
											  WorkFlowId,
                                              LoginId
                                           ) 
                                           VALUES 
                                           (
											 @WorkFlowId,
											 @LoginId
                                            )"; 

			try
			{
				
				insertSqlCommand = new SqlCommand(insertQueryText , connection);
				insertSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId",		workFlowId));
				insertSqlCommand.Parameters.Add(new SqlParameter("@LoginId",		loginId));
				
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
				
				Debug.Fail("Exception raised in WorkFlowUser.Insert: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowUserFailedMsgFmt, loginName), exception);
			}

			return true;
		}

		//---------------------------------------------------------------------
		public bool Delete(SqlConnection connection)
		{
			SqlTransaction  deleteSqlTransaction = null;
			SqlCommand deleteSqlCommand = null;

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowUser  
										WHERE LoginId = @LoginId AND
                                              WorkFlowId = @WorkFlowId";
			try
			{
				
				deleteSqlCommand				= new SqlCommand(deleteQueryText, connection);

				deleteSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId", workFlowId));
				deleteSqlCommand.Parameters.Add(new SqlParameter("@LoginId",	loginId));
				

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

				Debug.Fail("Exception raised in WorkFlowUser.Delete: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowUserDeleteFailedMsgFmt, loginName), exception);
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

			string deleteQueryText = @"DELETE FROM MSD_WorkFlowUser  
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

				Debug.Fail("Exception raised in WorkFlowUser.DeleteAll: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowUserDeleteFailedMsgFmt, loginName), exception);
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
			SqlCommand updateSqlCommand = null;

			string updateQueryText = @"SELECT COUNT(*) FROM MSD_WorkFlowUser
                                       WHERE 
                                           LoginId		= @LoginId AND
                                           WorkFlowId	= @WorkFlowId ";

			try
			{
				updateSqlCommand = new SqlCommand(updateQueryText, connection);

				updateSqlCommand.Parameters.Add(new SqlParameter("@WorkFlowId",				this.workFlowId));
				updateSqlCommand.Parameters.Add(new SqlParameter("@LoginId",				this.loginId));

				int exist = (int)updateSqlCommand.ExecuteScalar();
				if (exist == 0) //l'utente non c'è
				{
					if (isMember)
						Insert(connection);
				}
				else //l'utente esiste
				{
					if (!isMember)
						Delete(connection);
				}
			}
			catch (Exception exception)
			{
				if (updateSqlCommand != null)
					updateSqlCommand.Dispose();

				Debug.Fail("Exception raised in WorkFlowActivity.Update: " + exception.Message);
				throw new WorkFlowException(String.Format(WorkFlowObjectStrings.WorkFlowUserUpdateFailedMsgFmt, loginName), exception);
			}
			finally
			{
				if (updateSqlCommand != null)
					updateSqlCommand.Dispose();
			}

			return true;
		}
	}
}
