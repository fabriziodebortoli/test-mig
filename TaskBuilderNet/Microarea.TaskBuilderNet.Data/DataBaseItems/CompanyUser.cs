using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.OracleDataAccess;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// CompanyUserDb
	/// Gestisce i records della tabella MSD_CompanyLogins (utenti associati a una azienda)
	/// </summary>
	//=========================================================================
	public class CompanyUserDb : DataBaseItem
	{
		#region Costruttori
		/// <summary>
		/// Costruttore 1 (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public CompanyUserDb() 
		{}
		
		/// <summary>
		/// Costruttore 2 - Inizializza la connessione
		/// </summary>
		//---------------------------------------------------------------------
		public CompanyUserDb (string connectionString)
		{
			ConnectionString = connectionString;
		}
		#endregion

		#region Add - associa un Utente a una Azienda
		/// <summary>
		/// Add
		/// Inserisce un nuovo utente associato all'azienda
		/// (ovvero inserisce un nuovo record nella tabella MSD_CompanyLogins)
		/// </summary>
		/// <param name="userOfCompany">Dati dell'utente da inserire</param>
		//---------------------------------------------------------------------
		public bool Add (UserListItem userOfCompany)
		{
			bool result = false;
			SqlTransaction  myTransSql;
			SqlCommand myCommand	= new SqlCommand();
			myTransSql				= CurrentSqlConnection.BeginTransaction();
			myCommand.Connection	= CurrentSqlConnection;
			myCommand.Transaction	= myTransSql;

			try
			{
				string strQuery =
				@"INSERT INTO MSD_CompanyLogins
				(CompanyId, LoginId, DBUser, DBPassword, DBWindowsAuthentication, Admin, Disabled, LastModifyGrants, EBDeveloper) 
                VALUES 
                (@CompanyId, @LoginId, @DBUser, @DBPassword, @DBWinAuth, @Admin, @Disabled, @LastModifyGrants, @EBDev)"; 

				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@CompanyId",			userOfCompany.CompanyId));
				myCommand.Parameters.Add(new SqlParameter("@LoginId",			userOfCompany.LoginId));
				myCommand.Parameters.Add(new SqlParameter("@Admin",				userOfCompany.IsAdmin));
				myCommand.Parameters.Add(new SqlParameter("@EBDev",				userOfCompany.EasyBuilderDeveloper));
				myCommand.Parameters.Add(new SqlParameter("@Disabled",			userOfCompany.Disabled));
				myCommand.Parameters.Add(new SqlParameter("@DBUser",			userOfCompany.DbUser));
				myCommand.Parameters.Add(new SqlParameter("@DBPassword",		userOfCompany.DbWindowsAuthentication 
																				? string.Empty 
																				: Crypto.Encrypt(userOfCompany.DbPassword)));
				myCommand.Parameters.Add(new SqlParameter("@LastModifyGrants",	DateTime.Now.Date));
				myCommand.Parameters.Add(new SqlParameter("@DBWinAuth",			userOfCompany.DbWindowsAuthentication));
				myCommand.ExecuteNonQuery();
				myTransSql.Commit();
				result = true;
			}
			catch (SqlException sqlException)
			{
				myTransSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UserInserting, userOfCompany.DbUser), extendedInfo);
			}
			myTransSql.Dispose();
			myCommand.Dispose();
			return result;
		}
		#endregion

		#region Add - associazione "manuale" di un Utente ad un'Azienda
		/// <summary>
		/// Add
		/// Inserisce un nuovo utente associato all'azienda
		/// (ovvero inserisce un nuovo record nella tabella MSD_CompanyLogins)
		/// </summary>
		//---------------------------------------------------------------------
		public bool Add
			(
			string	companyId,
			string	loginId,
			bool	isAdmin,
			bool	disabled,
			string	dbUser,
			string	dbPassword,
			bool	dbWinAuth
			)
		{
			bool result = false;
			SqlTransaction  myTransSql;
			SqlCommand myCommand	= new SqlCommand();
			myTransSql				= CurrentSqlConnection.BeginTransaction();
			myCommand.Connection	= CurrentSqlConnection;
			myCommand.Transaction	= myTransSql;

			try
			{
				myCommand.CommandText =
				@"INSERT INTO MSD_CompanyLogins
				(CompanyId, LoginId, DBUser, DBPassword, DBWindowsAuthentication, Admin, Disabled, LastModifyGrants) 
                VALUES 
                (@CompanyId, @LoginId, @DBUser, @DBPassword, @DBWinAuth, @Admin, @Disabled, @LastModifyGrants)"; 

				myCommand.Parameters.Add(new SqlParameter("@CompanyId",			companyId));
				myCommand.Parameters.Add(new SqlParameter("@LoginId",			loginId));
				myCommand.Parameters.Add(new SqlParameter("@Admin",				isAdmin));
				myCommand.Parameters.Add(new SqlParameter("@Disabled",			disabled));
				myCommand.Parameters.Add(new SqlParameter("@DBUser",			dbUser));
				myCommand.Parameters.Add(new SqlParameter("@DBPassword",		dbWinAuth ? string.Empty : Crypto.Encrypt(dbPassword)));
				myCommand.Parameters.Add(new SqlParameter("@LastModifyGrants",	DateTime.Now.Date));
				myCommand.Parameters.Add(new SqlParameter("@DBWinAuth",			dbWinAuth));
				myCommand.ExecuteNonQuery();
				myTransSql.Commit();
				result = true;
			}
			catch(SqlException sqlException)
			{
				myTransSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UserInserting, dbUser), extendedInfo);
			}
			myTransSql.Dispose();
			myCommand.Dispose();
			return result;
		}
		#endregion

		#region Modify - Modifica un Utente associato a una Azienda
		/// <summary>
		/// Modify
		/// Modifica i dati di un utente assegnato a una azienda
		/// </summary>
		/// <param name="userOfCompany">dati dell'utente</param>
		//---------------------------------------------------------------------
		public bool Modify (UserListItem userOfCompany)
		{
			bool result = true;
			SqlTransaction myTransSql;
			SqlCommand myCommand	= new SqlCommand();
			myTransSql				= CurrentSqlConnection.BeginTransaction();
			myCommand.Connection	= CurrentSqlConnection;
			myCommand.Transaction	= myTransSql;

			try
			{
				string strQuery =
				@"UPDATE MSD_CompanyLogins
                SET DBUser = @DBUser, DBPassword = @DBPassword, DBWindowsAuthentication = @DBWinAuth,
					Admin = @Admin, Disabled = @Disabled, LastModifyGrants = @LastModifyGrants, EBDeveloper = @EBDev
                WHERE LoginId = @LoginId AND CompanyId = @CompanyId";

				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@LoginId",			Int32.Parse(userOfCompany.LoginId)));
				myCommand.Parameters.Add(new SqlParameter("@CompanyId",			Int32.Parse(userOfCompany.CompanyId)));
				myCommand.Parameters.Add(new SqlParameter("@Admin",				userOfCompany.IsAdmin));
				myCommand.Parameters.Add(new SqlParameter("@EBDev",				userOfCompany.EasyBuilderDeveloper));
				myCommand.Parameters.Add(new SqlParameter("@Disabled",			userOfCompany.Disabled));
				myCommand.Parameters.Add(new SqlParameter("@DBUser",			userOfCompany.DbUser));
				myCommand.Parameters.Add(new SqlParameter("@DBPassword",		userOfCompany.DbWindowsAuthentication 
																				? string.Empty 
																				: Crypto.Encrypt(userOfCompany.DbPassword)));
				myCommand.Parameters.Add(new SqlParameter("@LastModifyGrants",	DateTime.Now.Date));
				myCommand.Parameters.Add(new SqlParameter("@DBWinAuth",			userOfCompany.DbWindowsAuthentication));
				myCommand.ExecuteNonQuery();                           
				myTransSql.Commit();
				result = true;
			}
			catch (SqlException sqlException)
			{
				myTransSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.Modify");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UserModify, userOfCompany.DbUser), extendedInfo);
			}
			return result;
		}
		#endregion

		#region ModifyDisableStatusCompanyUsers - Modifica, per tutte le Aziende, il campo Disable dell'Utente assegnato, specificato da LoginId
		/// <summary>
		/// ModifyDisableStatusCompanyUsers
		/// Trova l'utente assegnato alle azienda identificato da LoginId, e per
		/// ognuno di queste asssegnazioni modifica il campo Disable con il valore del 
		/// parametro isDisabled
		/// </summary>
		/// <param name="loginId">Id che identifica l'utente assegnato a una azienda</param>
		/// <param name="isDisabled">true se l'utente va disabilitato, false altrimenti</param>
		//---------------------------------------------------------------------
		public bool ModifyDisableStatusCompanyUsers (string loginId, bool isDisabled)
		{
			bool result = false;
			SqlDataReader mylocalDataReader = null;
			SqlConnection myConnection		= new SqlConnection(ConnectionString);
			string myQuery = "SELECT * FROM MSD_CompanyLogins WHERE LoginId = @LoginId ORDER BY CompanyId";
			SqlCommand myCommand = new SqlCommand(myQuery,myConnection);
			myCommand.Parameters.AddWithValue("@LoginId", loginId);
			
			try
			{
				myConnection.Open();
				mylocalDataReader = myCommand.ExecuteReader();
				result = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.ModifyDisableStatusCompanyUsers");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
				mylocalDataReader.Close();
				myConnection.Close();
				myConnection.Dispose();
				return result;
			}
			
			while (mylocalDataReader.Read())
			{
				SqlConnection myModifyConnection = new SqlConnection(this.ConnectionString);
				string modifyQuery = "UPDATE MSD_CompanyLogins SET Disabled = @Disabled WHERE LoginId = @LoginId";
				SqlCommand myModifyCommand = new SqlCommand(modifyQuery, myModifyConnection);
				myModifyCommand.Parameters.AddWithValue("@LoginId",  int.Parse(loginId));
				myModifyCommand.Parameters.AddWithValue("@Disabled", isDisabled);
				
				try
				{
					myModifyConnection.Open();
					myModifyCommand.ExecuteNonQuery();    
					result = true;   
				}
				catch (SqlException sqlException)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
					extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
					extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
					extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
					extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
					extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.ModifyDisableStatusCompanyUsers");
					extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
					Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
					myModifyConnection.Close();
					myModifyConnection.Dispose();
					result = false;
				}

				myModifyConnection.Close();
				myModifyConnection.Dispose();
			}
			mylocalDataReader.Close();
			myConnection.Close();
			myConnection.Dispose();
			return result;
		}
		#endregion

		#region ForceDelete - cancella l'associazione loginId-CompanyId, anche del dbowner
		/// <summary>
		/// ForceDelete
		/// cancella l'associazione loginId-CompanyId, anche del dbowner
		/// </summary>
		//---------------------------------------------------------------------
		public bool ForceDelete(string LoginId, string CompanyId)
		{
			bool result = false;
			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;
			myCommand.CommandText = "MSD_DeleteCompanyLogin";
			myCommand.CommandType = CommandType.StoredProcedure;
			myCommand.Parameters.AddWithValue("@par_companyid", Int32.Parse(CompanyId));
			myCommand.Parameters.AddWithValue("@par_loginid",   Int32.Parse(LoginId));
			
			try
			{
				myCommand.ExecuteNonQuery();
				myCommand.Dispose();
				result = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.Delete");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.UserDeleted, extendedInfo);
				myCommand.Dispose();
			}

			return result;
		}
		#endregion

		#region Delete - Cancella un Utente associato a una Azienda
		/// <summary>
		/// Delete
		/// Cancella un utente assegnato all'azienda, utilizzando la stored procedure MSD_DeleteCompanyLogin
		/// </summary>
		/// <param name="LoginId">Id che identifica l'utente</param>
		/// <param name="CompanyId">Id che identifica l'azienda</param>
		//---------------------------------------------------------------------
		public bool Delete(string LoginId, string CompanyId)
		{
			bool result = false;

			//l'utente può essere cancellato solo se non è il dbo della company
			if (!IsDbo(LoginId,CompanyId))
			{
				SqlCommand myCommand	= new SqlCommand();
				myCommand.Connection	= CurrentSqlConnection;
				myCommand.CommandText	= "MSD_DeleteCompanyLogin";
				myCommand.CommandType	= CommandType.StoredProcedure;
				myCommand.Parameters.AddWithValue("@par_companyid", Int32.Parse(CompanyId));
				myCommand.Parameters.AddWithValue("@par_loginid",   Int32.Parse(LoginId));
				
				try
				{
					myCommand.ExecuteNonQuery();
					myCommand.Dispose();
					result = true;
				}
				catch(SqlException sqlException)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
					extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
					extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
					extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
					extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
					extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.Delete");
					extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
					Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.UserDeleted, extendedInfo);
					myCommand.Dispose();
				}
			}
			else
				Diagnostic.Set(DiagnosticType.Warning, DatabaseItemsStrings.CannotDeleteDbo);
			
			return result;
		}
		#endregion

		#region Funzioni di Ricerca e Selezione

		#region SelectAllDistinct - Seleziona tutti gli utenti (non doppioni) assegnati ad un'azienda
		/// <summary>
		/// SelectAllDistinct
		/// Seleziona tutti gli utenti (non doppioni) assegnati ad un'azienda
		/// </summary>
		//---------------------------------------------------------------------
		public bool SelectAllDistinct (out ArrayList usersOfCompany, string companyId)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag = true;
			
			try
			{
				SqlDataReader myDataReader;
				if (GetAllDistinct(out myDataReader, companyId))
				{
					while(myDataReader.Read())
					{
						CompanyUser userItem			 = new CompanyUser();
						userItem.CompanyId				 = companyId;
						userItem.LoginId				 = myDataReader["LoginId"].ToString();
						userItem.DBDefaultUser		     = myDataReader["DBUser"].ToString();
						userItem.DBWindowsAuthentication = bool.Parse(myDataReader["DBWindowsAuthentication"].ToString());
						localUsers.Add(userItem);
					}
					myDataReader.Close();
				}
				else
					mySuccessFlag = false;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,		sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,		"CompanyUserDb.SelectAllDistinct");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,	"SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
				mySuccessFlag = false;
			}
			usersOfCompany = localUsers;
			return mySuccessFlag;
		}

		//---------------------------------------------------------------------
		public bool GetAllDistinct (out SqlDataReader myDataReader, string companyId)
		{
			SqlDataReader mylocalDataReader = null;
			string myQuery;
			bool mySuccessFlag = true;
			try
			{
				myQuery = @"SELECT DISTINCT DBUser, DBWindowsAuthentication, LoginId
							FROM MSD_CompanyLogins
                            WHERE MSD_CompanyLogins.CompanyId=@CompanyId 
                            ORDER BY DBUser";
				
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				mylocalDataReader = myCommand.ExecuteReader();
				mySuccessFlag = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,		sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,		"CompanyUserDb.GetAllDistinct");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,	"SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyLogins"), extendedInfo);
				mySuccessFlag = false;
			}
			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		# region SelectAllExceptSa - Seleziona tutti gli utenti assegnati ad un'azienda, eccetto 'sa'
		//---------------------------------------------------------------------
		public bool SelectAllExceptSa(out ArrayList usersOfCompany, string companyId)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag = true;
			
			try
			{
				SqlDataReader myDataReader;
				bool successFlag = false;
				successFlag = GetAll(out myDataReader, companyId);
				if (successFlag)
				{
					while(myDataReader.Read())
					{
						// skippo l'utente sa
						if (string.Compare(myDataReader["DBUser"].ToString(), DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
							continue;

						CompanyUser userItem			 = new CompanyUser();
						userItem.CompanyId				 = myDataReader["CompanyId"].ToString();
						userItem.LoginId				 = myDataReader["LoginId"].ToString();
						userItem.Login					 = myDataReader["Login"].ToString();
						userItem.Password				 = Crypto.Decrypt(myDataReader["Password"].ToString());
						userItem.Description			 = myDataReader["Description"].ToString();
						userItem.LastModifyGrants		 = myDataReader["LastModifyGrants"].ToString();
						userItem.ExpiredDatePassword	 = myDataReader["ExpiredDatePassword"].ToString();
						userItem.Disabled				 = bool.Parse(myDataReader["Disabled"].ToString());
						userItem.WindowsAuthentication   = bool.Parse(myDataReader["WindowsAuthentication"].ToString());
						userItem.DBDefaultUser		     = myDataReader["DBUser"].ToString();
						userItem.DBDefaultPassword	     = Crypto.Decrypt(myDataReader["DBPassword"].ToString());
						userItem.Admin					 = bool.Parse(myDataReader["Admin"].ToString());
						userItem.EasyBuilderDeveloper	 = bool.Parse(myDataReader["EBDeveloper"].ToString());
						userItem.Disabled			     = bool.Parse(myDataReader["Disabled"].ToString());
						userItem.DBWindowsAuthentication = bool.Parse(myDataReader["DBWindowsAuthentication"].ToString());
						localUsers.Add(userItem);
					}
					myDataReader.Close();
				}
				else
					mySuccessFlag = successFlag;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.SelectAll");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
				mySuccessFlag = false;
			}
			
			usersOfCompany = localUsers;
			return mySuccessFlag;
		}
		# endregion

		#region SelectAll - Seleziona tutti gli Utenti assegnati ad un'azienda
		/// <summary>
		/// SelectAll
		/// Seleziona tutti gli utenti di una azienda leggendo dalla tabella MSD_CompanyLogins e MSD_Logins
		/// </summary>
		/// <param name="usersOfCompany">Array con l'elenco degli utenti</param>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAll(out ArrayList usersOfCompany, string companyId)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag = true;
			
			try
			{
				SqlDataReader myDataReader;
				bool successFlag = GetAll(out myDataReader, companyId);
				
				if (successFlag)
				{
					while(myDataReader.Read())
					{
						CompanyUser userItem			 = new CompanyUser();
						userItem.CompanyId				 = myDataReader["CompanyId"].ToString();
						userItem.LoginId				 = myDataReader["LoginId"].ToString();
						userItem.Login					 = myDataReader["Login"].ToString();
						userItem.Password				 = Crypto.Decrypt(myDataReader["Password"].ToString());
						userItem.Description			 = myDataReader["Description"].ToString();
						userItem.LastModifyGrants		 = myDataReader["LastModifyGrants"].ToString();
						userItem.ExpiredDatePassword	 = myDataReader["ExpiredDatePassword"].ToString();
						userItem.Disabled				 = bool.Parse(myDataReader["Disabled"].ToString());
						userItem.WindowsAuthentication   = bool.Parse(myDataReader["WindowsAuthentication"].ToString());
						userItem.DBDefaultUser		     = myDataReader["DBUser"].ToString();
						userItem.DBDefaultPassword	     = Crypto.Decrypt(myDataReader["DBPassword"].ToString());
						userItem.Admin					 = bool.Parse(myDataReader["Admin"].ToString());
						userItem.EasyBuilderDeveloper	 = bool.Parse(myDataReader["EBDeveloper"].ToString());
						userItem.Disabled			     = bool.Parse(myDataReader["Disabled"].ToString());
						userItem.DBWindowsAuthentication = bool.Parse(myDataReader["DBWindowsAuthentication"].ToString());
						localUsers.Add(userItem);
					}
					myDataReader.Close();
				}
				else
					mySuccessFlag = successFlag;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.SelectAll");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
				mySuccessFlag = false;
			}
			
			usersOfCompany = localUsers;
			return mySuccessFlag;
		}
		
		/// <summary>
		/// GetAll
		/// Riempie un dataReader con gli utenti assegnati all'azienda
		/// </summary>
		/// <param name="myDataReader">datareader con gli utenti</param>
		/// <param name="companyId">Id che identifica la company</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAll (out SqlDataReader myCompanyUserDataReader, string companyId)
		{
			SqlDataReader mylocalDataReader1 = null; 
			string myQuery;
			bool mySuccessFlag = true;
			try
			{
				myQuery = @"SELECT MSD_CompanyLogins.LoginId, MSD_CompanyLogins.CompanyId,
                                   MSD_CompanyLogins.DBUser, MSD_CompanyLogins.DBPassword,
                                   MSD_CompanyLogins.DBWindowsAuthentication, MSD_CompanyLogins.Admin,
								   MSD_CompanyLogins.EBDeveloper,
                                   MSD_CompanyLogins.LastModifyGrants, MSD_CompanyLogins.Disabled,
								   MSD_Logins.Login, MSD_Logins.Password, MSD_Logins.Description,
                                   MSD_Logins.ExpiredDatePassword, MSD_Logins.WindowsAuthentication
                            FROM   MSD_CompanyLogins, MSD_Logins
                            WHERE MSD_CompanyLogins.CompanyId = @CompanyId AND MSD_Logins.LoginId = MSD_CompanyLogins.LoginId  
                            ORDER BY MSD_Logins.Login";
				
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				mylocalDataReader1 = myCommand.ExecuteReader();
				mySuccessFlag = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetAll");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyLogins, MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
				mylocalDataReader1.Close();
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
			}
			
			myCompanyUserDataReader = mylocalDataReader1;
			return mySuccessFlag;
		}
		#endregion

		#region GetUserCompanyById - Seleziona un Utente attraverso il suo Id, assegnato ad un'azienda 
		/// <summary>
		/// GetUserCompanyById
		/// Seleziona i dati di un utente assegnato a una azienda
		/// </summary>
		/// <param name="userCompany">Array con i dati dell'utente trovato</param>
		/// <param name="userId">Id che identifica l'utetne</param>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetUserCompany(out ArrayList userCompany, string userId, string companyId)
		{
			ArrayList localUserCompany  = new ArrayList();
			CompanyUser companyUserItem = new CompanyUser();
			bool mySuccessFlag = false;
			
			try
			{
				SqlDataReader myDataReader;
				if (GetUserCompanyId(out myDataReader, userId, companyId))
				{
					while(myDataReader.Read())
					{
						companyUserItem.CompanyId				= myDataReader["CompanyId"].ToString();
						companyUserItem.LoginId					= myDataReader["LoginId"].ToString();
						companyUserItem.Login					= myDataReader["Login"].ToString();
						companyUserItem.Admin					= bool.Parse(myDataReader["Admin"].ToString());
						companyUserItem.EasyBuilderDeveloper	= bool.Parse(myDataReader["EBDeveloper"].ToString());
						companyUserItem.WindowsAuthentication	= bool.Parse(myDataReader["WindowsAuthentication"].ToString());
						companyUserItem.DBDefaultUser			= myDataReader["DBUser"].ToString();
						companyUserItem.DBDefaultPassword		= Crypto.Decrypt(myDataReader["DBPassword"].ToString());
						companyUserItem.Description				= myDataReader["Description"].ToString();
						companyUserItem.Disabled				= bool.Parse(myDataReader["Disabled"].ToString());
						companyUserItem.ExpiredDatePassword		= myDataReader["ExpiredDatePassword"].ToString();
						companyUserItem.LastModifyGrants		= myDataReader["LastModifyGrants"].ToString();
						companyUserItem.Password			    = Crypto.Decrypt(myDataReader["Password"].ToString());
						companyUserItem.DBWindowsAuthentication = bool.Parse(myDataReader["DBWindowsAuthentication"].ToString());
						localUserCompany.Add(companyUserItem);
					}
					
					myDataReader.Close();
					mySuccessFlag = true;
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.GetUserCompany");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
				mySuccessFlag = false;
			}
			
			userCompany = localUserCompany;
			return mySuccessFlag;
		}
		
		/// <summary>
		/// GetUserCompanyId
		/// Riempie il dataReader con i dati dell'utente assegnato all'azienda
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati dell'utente</param>
		/// <param name="loginId">Id che identifica l'utente</param>
		/// <param name="companyId">Id che identifica l'azienda a cui l'utente è
		///                         assegnato</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------------
		public bool GetUserCompanyId(out SqlDataReader myDataReader, string loginId, string companyId)
		{
			SqlDataReader mylocalDataReader = null;
			string myQuery;
			bool mySuccessFlag = false;
			
			try
			{
				myQuery = @"SELECT MSD_CompanyLogins.LoginId, MSD_CompanyLogins.CompanyId,
                                 MSD_CompanyLogins.DBUser, MSD_CompanyLogins.DBPassword,
                                 MSD_CompanyLogins.DBWindowsAuthentication, MSD_CompanyLogins.Admin,
                                 MSD_CompanyLogins.LastModifyGrants, MSD_CompanyLogins.Disabled,
                                 MSD_CompanyLogins.EBDeveloper,
								 MSD_Logins.Login, MSD_Logins.Password, MSD_Logins.Description,
                                 MSD_Logins.ExpiredDatePassword, MSD_Logins.WindowsAuthentication
                            FROM MSD_CompanyLogins, MSD_Logins
                            WHERE MSD_CompanyLogins.CompanyId = @CompanyId AND MSD_CompanyLogins.LoginId = @LoginId AND
                                  MSD_Logins.LoginId = MSD_CompanyLogins.LoginId";
				
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				myCommand.Parameters.AddWithValue("@LoginId",   Int32.Parse(loginId));
				mylocalDataReader = myCommand.ExecuteReader();
				mySuccessFlag = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetUserCompanyId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyLogins, MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectDataForUserCompany - Trova i dati dell'Utente (con LoginID) assegnato a una Azienda
		/// <summary>
		/// SelectDataForUserCompany
		/// Dato l'Id che identifica l'utente, trova i dati leggendo dalla
		/// tabella MSD_CompanyLogins
		/// </summary>
		/// <param name="LoginId">Id dell'utente</param>
		/// <param name="CompanyId">Id dell'azienda a cui l'utente è associato</param>
		/// <param name="isAdmin">true se l'utente è un amministratore</param>
		/// <param name="dbUser">userName dell'utente, con cui si connette al db
		///                      dell'azienda</param>
		/// <param name="dbPassword">Password dell'utente, con cui si connette
		///                      al db dell'azienda</param>
		/// <param name="dbWindowsAuthentication">se true autenticazione NT, false
		///                                       autenticazione SQL</param>
		/// <param name="isDisabled">true se la login è disabilitata,
		///                          false altrimenti</param>
		//---------------------------------------------------------------------
		public bool SelectDataForUserCompany
			(
			string		LoginId, 
			string		CompanyId, 
			out bool	isAdmin, 
			out string	dbUser, 
			out string	dbPassword, 
			out bool	dbWinAuth,
			out bool	isDisabled
			)
		{
			bool result = true;
			string localdbUser			= string.Empty;
			string localdbPassword		= string.Empty;
			string myQuery				= string.Empty;
			bool   localIsAdmin			= false;
			bool   localWinAuth			= false;
			bool   localIsDisabled		= false;
			
			SqlDataReader mylocalDataReader	= null;
			
			try
			{
				myQuery = @"SELECT Admin, DBUser, DBPassword, DBWindowsAuthentication, Disabled
                          FROM MSD_CompanyLogins WHERE LoginId = @LoginId AND CompanyId = @CompanyId";
				
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(CompanyId));
				myCommand.Parameters.AddWithValue("@LoginId",   Int32.Parse(LoginId));
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.SelectDataForUserCompany");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
				result = false;
			}
			
			if (mylocalDataReader.Read())
			{
				localIsDisabled	= bool.Parse(mylocalDataReader["Disabled"].ToString());
				localIsAdmin	= bool.Parse(mylocalDataReader["Admin"].ToString());
				localWinAuth	= bool.Parse(mylocalDataReader["DBWindowsAuthentication"].ToString());
				localdbUser		= mylocalDataReader["DBUser"].ToString();
				localdbPassword	= Crypto.Decrypt(mylocalDataReader["DBPassword"].ToString());
			}
			
			isDisabled	= localIsDisabled;
			isAdmin		= localIsAdmin;
			dbUser		= localdbUser;
			dbPassword	= localdbPassword;
			dbWinAuth	= localWinAuth;
			mylocalDataReader.Close();
			return result;
		}
		#endregion

		#region IsDbo - True se l'utente identificato da LoginId è il dbOwner del database dell'Azienda (il check è a livello di anagrafica)
		/// <summary>
		/// IsDbo
		/// Torna true se l'utente identificato dalla loginId è il dbo
		/// dell'azienda, identificata dalcompanyId
		/// </summary>
		/// <param name="loginId">Id dell'utente</param>
		/// <param name="companyId">Id della company</param>
		/// <returns>isDbowner, true se la loginId è il dbo della azienda, 
		///			false altrimenti</returns>
		//---------------------------------------------------------------------
		public bool IsDbo (string loginId, string companyId)
		{
			bool isDbowner = false;
			string myQuery = 
				"SELECT MSD_Companies.CompanyDBOwner FROM MSD_Companies WHERE MSD_Companies.CompanyId = @CompanyId";
			
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				object OwnerId = myCommand.ExecuteScalar();
				if (string.Compare(loginId, OwnerId.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
					isDbowner = true;
				myCommand.Dispose();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.IsDbo");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
			}
		
			return isDbowner;
		}
		#endregion

		#region ExistLogin - True se l'utente è associato all'azienda con Id = companyId
		/// <summary>
		/// ExistLogin
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistLogin(string companyId, string dbUser, string dbPassword)
		{
			bool  loginExist = false;
			SqlDataReader mylocalDataReader = null;
			SqlConnection myConnection = new SqlConnection(ConnectionString);
			
			string myQuery = @"SELECT COUNT(*) FROM MSD_CompanyLogins
								WHERE CompanyId = @CompanyId AND DBUser = @DBUser AND DBPassword = @DBPassword";
			
			SqlCommand myCommand = new SqlCommand(myQuery, myConnection);
			myCommand.Parameters.AddWithValue("@CompanyId", companyId);
			myCommand.Parameters.AddWithValue("@DBUser",	dbUser);
			myCommand.Parameters.AddWithValue("@DBPassword",Crypto.Encrypt(dbPassword));
			
			try
			{
				myConnection.Open();
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.ExistLogin");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
				mylocalDataReader.Close();
				myConnection.Close();
				myConnection.Dispose();
				return false;
			}
			
			if (mylocalDataReader.Read())
				loginExist = (mylocalDataReader.GetInt32(0) == 0) ? false : true;

			if (mylocalDataReader != null)
				mylocalDataReader.Close();
			if (myConnection != null)
			{
				myConnection.Close();
				myConnection.Dispose();
			}
			
			return loginExist;
		}
		#endregion

		# region CanDropLogin
		//---------------------------------------------------------------------
		public bool CanDropLogin(string dbUser, string companyId)
		{
			bool canDrop = false;
			int  numbersOfLogin = 0;

			ArrayList companyUsers = new ArrayList();
			if (!SelectAll(out companyUsers, companyId))
				return canDrop;

			for (int i = 0; i < companyUsers.Count; i++)
			{
				CompanyUser currentUsers = (CompanyUser) companyUsers[i];
				if (string.Compare(currentUsers.DBDefaultUser, dbUser, StringComparison.InvariantCultureIgnoreCase) == 0) 
					numbersOfLogin +=1;

				if (numbersOfLogin > 1)
					break;
			}

			canDrop = (numbersOfLogin <= 1) ? true : false;
			return canDrop;
		}
		# endregion

		#endregion

		# region Funzioni per ORACLE

		#region SelectAllDistinctForOracle - Seleziona tutti gli utenti (non doppioni) assegnati ad un'azienda
		/// <summary>
		/// SelectAllDistinctForOracle
		/// Seleziona tutti gli utenti (non doppioni) assegnati ad un'azienda
		/// </summary>
		//---------------------------------------------------------------------
		public bool SelectAllDistinctForOracle(out ArrayList usersOfCompany, string companyId)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag = true;

			try
			{
				SqlDataReader myDataReader;
				if (GetAllDistinctForOracle(out myDataReader, companyId))
				{
					while (myDataReader.Read())
					{
						CompanyUser userItem	= new CompanyUser();
						userItem.CompanyId		= companyId;
						userItem.DBDefaultUser	= myDataReader["DBUser"].ToString();
						localUsers.Add(userItem);
					}
					myDataReader.Close();
				}
				else
					mySuccessFlag = false;
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyUserDb.SelectAllDistinctForOracle");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
				mySuccessFlag = false;
			}

			usersOfCompany = localUsers;
			return mySuccessFlag;
		}

		//---------------------------------------------------------------------
		public bool GetAllDistinctForOracle(out SqlDataReader myDataReader, string companyId)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			try
			{
				string myQuery = @"SELECT DISTINCT DBUser FROM MSD_CompanyLogins
                            WHERE MSD_CompanyLogins.CompanyId = @CompanyId 
                            ORDER BY DBUser";

				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				mylocalDataReader = myCommand.ExecuteReader();
				mySuccessFlag = true;
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyUserDb.GetAllDistinctForOracle");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyLogins"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region LoadUsersFromOracleSchema
		//---------------------------------------------------------------------
		public ArrayList LoadUsersFromOracleSchema
			(
			string oracleService, 
			string oracleCompanyDbName, 
			OracleUserImpersonatedData oracleAdmin, 
			OracleAccess oracleAccess
			)
		{
			ArrayList usersFromSchema = new ArrayList();

			try
			{
				oracleAccess.LoadUserData
					(
					oracleAdmin.OracleService, 
					oracleAdmin.Login, 
					oracleAdmin.Password, 
					oracleAdmin.WindowsAuthentication
					);
				oracleAccess.OpenConnection();
				usersFromSchema = oracleAccess.GetAllUsersToSchema(oracleCompanyDbName);
				usersFromSchema.Sort();
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
			}
			finally
			{
			}
			
			return usersFromSchema;
		}
		#endregion

		# region LoadAllOracleUsers
		/// <summary> 
		/// LoadAllOracleUsers
		/// Richiamata dal tool di Migrazione dati tramite un evento per avere l'elenco degli utenti Oracle disponibili
		/// </summary>
		//---------------------------------------------------------------------
		public ArrayList LoadAllOracleUsers(string oracleService, string oracleAdminLogin, string oracleAdminPwd, bool oracleAdminIsWinNt)
		{
			ArrayList freeUsers			= new ArrayList();
			OracleAccess oracleAccess	= new OracleAccess();

			try
			{
				oracleAccess.LoadUserData(oracleService, oracleAdminLogin, oracleAdminPwd, oracleAdminIsWinNt);
				oracleAccess.OpenConnection();
				//mostro tutte le login di oracle meno quelle già assegnate su qualche azienda
				freeUsers = oracleAccess.GetFreeDBUsersForAttach();
				//ordino
				freeUsers.Sort();
				
				//leggo tutte le aziende oracle
				ArrayList allOracleCompanies		= new ArrayList();
				CompanyDb companiesDb				= new CompanyDb();
				companiesDb.ConnectionString		= this.ConnectionString;
				companiesDb.CurrentSqlConnection	= this.CurrentSqlConnection;
				companiesDb.SelectAllOracleCompanies(out allOracleCompanies);
				
				//escludo tutti gli utenti che sono censiti in MConsole come aziende
				for (int i = 0; i < allOracleCompanies.Count; i++)
				{
					CompanyItem company = (CompanyItem)allOracleCompanies[i];
					for (int j = 0; j < freeUsers.Count; j++)
					{
						//se l'utente j-esimo ha già un'azienda lo escludo
						OracleUser currentUser = (OracleUser)freeUsers[j];
						if (currentUser.OracleUserId == company.DbName)
						{
							freeUsers.RemoveAt(j);
							break;
						}
					}
					//prendo tutti gli utenti per cui su quella azienda esistono dei sinonimi e li escludo
					ArrayList allUsersWithSynonym = oracleAccess.GetAllUsersToSchema(company.DbName);
					for (int j = 0; j < allUsersWithSynonym.Count; j++)
					{
						OracleUser currentUser = (OracleUser)allUsersWithSynonym[j];
						int pos = freeUsers.BinarySearch(currentUser);
						if (pos > 0)
							freeUsers.RemoveAt(pos);
					}
				}
			}
			catch(Exception exc)
			{
				Debug.Fail(exc.Message);
			}
			return freeUsers;
		}
		# endregion
	
		# region LoadFreeOracleUsersForAttach
		/// <summary>
		/// LoadFreeOracleUsersForAttach
		/// Utenti oracle non ancora assegnati disponibili per l'attach
		/// </summary>
		//---------------------------------------------------------------------
		public ArrayList LoadFreeOracleUsersForAttach
			(
			string oracleService, 
			string oracleCompanyDbName, 
			OracleUserImpersonatedData oracleAdmin
			)
		{
			ArrayList freeUsers = new ArrayList();
			OracleAccess oracleAccess = new OracleAccess();

			if (oracleAdmin.IsDba)
			{
				try
				{
					oracleAccess.LoadUserData
						(
						oracleAdmin.OracleService, 
						oracleAdmin.Login, 
						oracleAdmin.Password, 
						oracleAdmin.WindowsAuthentication
						);
					oracleAccess.OpenConnection();
					
					// mostro tutte le login di oracle meno quelle già assegnate su qualche azienda
					freeUsers = oracleAccess.GetFreeDBUsersForAttach();
					freeUsers.Sort();

					FindOracleUsers(freeUsers, oracleAccess, oracleCompanyDbName);
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
				}
			}

			return freeUsers;
		}
		# endregion

		# region LoadFreeOracleUsersForAssociation
		/// <summary>
		/// LoadFreeOracleUsersForAssociation
		/// Utenti oracle non ancora assegnati (da richiamare nelle operazioni di associazione degli utenti)
		/// </summary>
		//---------------------------------------------------------------------
		public ArrayList LoadFreeOracleUsersForAssociation
			(
			string oracleService, 
			string oracleCompanyDbName, 
			OracleUserImpersonatedData oracleAdmin
			)
		{
			ArrayList freeUsers = new ArrayList();
			OracleAccess oracleAccess = new OracleAccess();

			if (oracleAdmin.IsDba)
			{
				try
				{
					oracleAccess.LoadUserData
						(
						oracleAdmin.OracleService, 
						oracleAdmin.Login, 
						oracleAdmin.Password, 
						oracleAdmin.WindowsAuthentication
						);
					oracleAccess.OpenConnection();
					// mostro tutte le login di oracle meno quelle già assegnate su qualche azienda
					freeUsers = oracleAccess.GetFreeDBUsersForAssociation();
					freeUsers.Sort();
					
					FindOracleUsers(freeUsers, oracleAccess, oracleCompanyDbName);
				}
				catch(Exception exc)
				{
					Debug.Fail(exc.Message);
				}
			}

			return freeUsers;
		}
		# endregion

		# region FindOracleUsers
		//---------------------------------------------------------------------
		private void FindOracleUsers(ArrayList freeUsers, OracleAccess oracleAccess, string oracleCompanyDbName)
		{
			//leggo tutte le aziende oracle
			ArrayList allOracleCompanies = new ArrayList();
			
			CompanyDb companiesDb = new CompanyDb();
			companiesDb.ConnectionString = this.ConnectionString;
			companiesDb.CurrentSqlConnection = this.CurrentSqlConnection;
			companiesDb.SelectAllOracleCompanies(out allOracleCompanies);

			ArrayList allUsersWithSynonym = null;

			for (int i = 0; i < allOracleCompanies.Count; i++)
			{
				CompanyItem company = (CompanyItem)allOracleCompanies[i];
				if (company.DbName.Length > 0 &&
					string.Compare(oracleCompanyDbName, company.DbName, StringComparison.InvariantCultureIgnoreCase) == 0)
					continue;

				allUsersWithSynonym = oracleAccess.GetAllUsersToSchema(company.DbName);
				for (int j = 0; j < allUsersWithSynonym.Count; j++)
				{
					OracleUser currentUser = (OracleUser)allUsersWithSynonym[j];
					int pos = freeUsers.BinarySearch(currentUser);
					if (pos > 0)
						freeUsers.RemoveAt(pos);
				}
			}

			ArrayList users = new ArrayList();

			for (int i = 0; i < allOracleCompanies.Count; i++)
			{
				CompanyItem company = (CompanyItem)allOracleCompanies[i];
				if (oracleCompanyDbName != company.DbName)
				{
					if (!SelectAllDistinctForOracle(out users, company.CompanyId))
						continue;

					foreach (CompanyUser user in users)
					{
						for (int j = 0; j < freeUsers.Count; j++)
						{
							if (((OracleUser)freeUsers[j]).OracleUserId == user.DBDefaultUser)
							{
								freeUsers.RemoveAt(j);
								break;
							}
						}
					}
				}
			}
		}
		# endregion
		
		#region ExistUserInOracleCompanys - True se l'utente può essere associato a un'azienda Oracle
		/// <summary>
		/// ExistUserInOracleCompanys
		/// True se l'utente non è associato a nessun altra azienda Oracle oppure se è associato ma è disabilitato
		/// false altrimenti
		/// </summary>
		/// <param name="loginId"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public bool ExistUserInOracleCompanys(string loginId)
		{
			SqlDataReader mylocalDataReader = null;
			string myQuery;
			
			try
			{
				myQuery = @"SELECT MSD_CompanyLogins.DBUser, MSD_CompanyLogins.Disabled
					      FROM MSD_CompanyLogins 
                          INNER JOIN MSD_Companies
                          ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyID AND
                          MSD_Companies.ProviderId  = (SELECT MSD_Providers.ProviderId FROM MSD_Providers
                          WHERE MSD_Providers.Provider = @ProviderType) WHERE LoginId = @LoginId";
				
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@ProviderType", NameSolverDatabaseStrings.OraOLEDBProvider);
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.ExistUserInOracleCompanys");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
			}

			bool disabled = false;
			bool emptyUserName = false;
			
			if (!mylocalDataReader.HasRows)
			{
				if (mylocalDataReader != null)
					mylocalDataReader.Close();
				return true;
			}
			else
			{
				while (mylocalDataReader.Read())
				{
					if ( bool.Parse(mylocalDataReader["Disabled"].ToString()))
						disabled = disabled && true;
					if (mylocalDataReader["DBUser"].ToString().Length == 0)
						emptyUserName = emptyUserName && true;
				}
				if (mylocalDataReader != null)
					mylocalDataReader.Close();
				
				return (disabled && emptyUserName);
			}
		}
		#endregion

		# endregion

        #region ExistUserInPostgreCompanys - True se l'utente può essere associato a un'azienda Postgre
        /// <summary>
        /// True se l'utente non è associato a nessun altra azienda Postgre oppure se è associato ma è disabilitato
        /// false altrimenti
        /// </summary>
        /// <param name="loginId"></param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        public bool ExistUserInPostgreCompanys(string loginId)
        {
            SqlDataReader mylocalDataReader = null;
            string myQuery;

            try
            {
                myQuery = @"SELECT MSD_CompanyLogins.DBUser, MSD_CompanyLogins.Disabled
					      FROM MSD_CompanyLogins 
                          INNER JOIN MSD_Companies
                          ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyID AND
                          MSD_Companies.ProviderId  = (SELECT MSD_Providers.ProviderId FROM MSD_Providers
                          WHERE MSD_Providers.Provider = @ProviderType) WHERE LoginId = @LoginId";

                SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
                myCommand.Parameters.AddWithValue("@ProviderType", NameSolverDatabaseStrings.PostgreOdbcProvider);
                myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));
                mylocalDataReader = myCommand.ExecuteReader();
            }
            catch (SqlException sqlException)
            {
                ExtendedInfo extendedInfo = new ExtendedInfo();
                extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
                extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
                extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
                extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
                extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
                extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyUserDb.ExistUserInOracleCompanys");
                extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "SysAdminPlugIn");
                Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
            }

            bool disabled = false;
            bool emptyUserName = false;

            if (!mylocalDataReader.HasRows)
            {
                if (mylocalDataReader != null)
                    mylocalDataReader.Close();
                return true;
            }
            else
            {
                while (mylocalDataReader.Read())
                {
                    if (bool.Parse(mylocalDataReader["Disabled"].ToString()))
                        disabled = disabled && true;
                    if (mylocalDataReader["DBUser"].ToString().Length == 0)
                        emptyUserName = emptyUserName && true;
                }
                if (mylocalDataReader != null)
                    mylocalDataReader.Close();

                return (disabled && emptyUserName);
            }
        }
        #endregion

		#region Altre Funzioni

		#region ExistUser - Verifica se l'Utente è effettivamente associato all'Azienda
		/// <summary>
		/// ExistUser
		/// Verifica se per una data company identificata da companyId l'utente
		/// identificato da LoginId risulta associato ( = esiste nella tabella
		/// MSD_CompanyLogins)
		/// </summary>
		/// <param name="LoginId">Id che identifica l'utente</param>
		/// <param name="CompanyId">Id che identifica l'azienda</param>
		/// <returns>result, 0 se l'utente non è associato all'azienda,
		///          1 altrimenti</returns>
		//---------------------------------------------------------------------
		public int ExistUser (string LoginId, string CompanyId)
		{
			int result = 0;
			SqlDataReader mylocalDataReader = null;
			string myQuery;
			
			try
			{
				myQuery = "SELECT COUNT(*) FROM MSD_CompanyLogins WHERE LoginId = @LoginId AND CompanyId = @CompanyId";
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(CompanyId));
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(LoginId));
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.ExistUser");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
			}

			if (mylocalDataReader.Read())
				result = mylocalDataReader.GetInt32(0);
			
			mylocalDataReader.Close();
			return result;
		}
		#endregion

		#region CurrentDbo - true se l'Utente esiste nel database dell'azienda
		/// <summary>
		/// CurrentDbo
		/// True se loginName è il dbowern del database Dbname
		/// Per stabilire ciò viene utilizzata la stored procedure di sistema
		/// sp_helpdb
		/// </summary>
		/// <param name="loginName">Nome utente assegnato al database</param>
		/// <param name="dbName">Nome del database</param>
		/// <returns>isCurrentDbo, true se è il dbo, false altrimenti</returns>
		//---------------------------------------------------------------------
		public bool CurrentDbo(string loginName, string dbName)
		{
			bool isCurrentDbo			  = false;
			string owner				  = string.Empty;
			SqlDataReader mySqlDataReader = null;
			
			SqlConnection myConnectionForCurrentDbo = new SqlConnection(this.ConnectionString);
			if (myConnectionForCurrentDbo.State == ConnectionState.Closed)
				myConnectionForCurrentDbo.Open();

			string currentDb = myConnectionForCurrentDbo.Database;
			myConnectionForCurrentDbo.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);
			SqlCommand myCommand	= new SqlCommand();
			myCommand.Connection	= myConnectionForCurrentDbo;
			myCommand.CommandText	= "sp_helpdb";
			myCommand.CommandType	= CommandType.StoredProcedure;
			myCommand.Parameters.AddWithValue("@dbname", dbName);
			
			try
			{
				mySqlDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException)
			{
				mySqlDataReader.Close();
				myConnectionForCurrentDbo.Close();
				myConnectionForCurrentDbo.Dispose();
				myCommand.Dispose();
				return false;
			}

			while(mySqlDataReader.Read())
				owner = mySqlDataReader["owner"].ToString();

			if (owner.Length > 0) 
			{
				if (string.Compare(owner, loginName, StringComparison.InvariantCultureIgnoreCase) == 0)
					isCurrentDbo = true;
			}

			mySqlDataReader.Close();
			myConnectionForCurrentDbo.ChangeDatabase(currentDb);
			myConnectionForCurrentDbo.Close();
			myConnectionForCurrentDbo.Dispose();
			myCommand.Dispose();
			return isCurrentDbo;
		}
		#endregion
		
		#region CompanyDbLoginRevoke - Esegue la SPRevokeDbAccess invocandola a seconda se la login è NT o SQL
		/// <summary>
		/// CompanyDbLoginRevoke
		/// Invoca la SPRevokeDbAccess con le credenziali opportune a seconda se la login è NT o SQL
		/// </summary>
		/// <param name="loginId">Id che identifica la login da revocare</param>
		/// <param name="companyId">Id che identifica l'azienda</param>
		//---------------------------------------------------------------------
		public bool CompanyDbLoginRevoke (string loginId, string companyId)
		{
			bool result = false;
			ArrayList userCompany = new ArrayList();
			GetUserCompany(out userCompany, loginId, companyId);
			CompanyUser userItem  = (CompanyUser)userCompany[0];
			
			string connectionString = GetCompanyDBConnectionString(companyId);
			if (connectionString.Length == 0)
				return result;

			SqlConnection myConnection = new SqlConnection(connectionString);
			
			try
			{
				myConnection.Open();

				if (userItem.DBWindowsAuthentication)
				{
					if (!SPRevokeDbAccess(userItem.Login, myConnection))
					{
						myConnection.Close();
						myConnection.Dispose();
						return result;
					}
					else
						result = true;
				}
				else
				{
					if (userItem.DBDefaultUser.Length > 0)
					{
						if (string.Compare(userItem.DBDefaultUser, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) != 0)
						{
							if (!SPRevokeDbAccess(userItem.DBDefaultUser, myConnection))
							{
								myConnection.Close();
								myConnection.Dispose();
								return result;
							}
							else
								result = true;
						}
					}
					else
					{
						if (string.Compare(userItem.Login, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) != 0)
						{
							if (!SPRevokeDbAccess(userItem.Login, myConnection))
							{
								myConnection.Close();
								myConnection.Dispose();
								return result;
							}
							else 
								result = true;
						}
					}
				}
				result = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.CompanyDbLoginRevoke");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UserCannotRevoked, userItem.Login), extendedInfo);
				myConnection.Close();
				myConnection.Dispose();
				return result;
			}

			myConnection.Close();
			myConnection.Dispose();
			return result;
		}
		#endregion

		#region SPRevokeDbAccess - Rimuove un utente dal database SQL (login di Utente associato all'Azienda)
		/// <summary>
		/// SPRevokeDbAccess
		/// Esegue la stored procedure sp_revokedbaccess, che rimuove un security
		/// account dal db corrente.
		/// la login può essere di tipo NT o SQL, e DEVE esistere nel db corrente
		/// Non si può rimuovere: il dbo e la public role,il guest dai db master e tempdb
		/// Solo i membri del sysadmin, db_accessadmin e db_owner possono eseguire questa
		/// sp
		/// </summary>
		/// <param name="loginDb">Nome dell'utente la cui login va revocata</param>
		/// <param name="currentConnection"> connessione</param>
		//---------------------------------------------------------------------
		private bool SPRevokeDbAccess (string loginDb, SqlConnection currentConnection)
		{
			bool successRevoke = false;
			if (string.Compare(loginDb, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0) 
				return successRevoke;

			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = currentConnection;
			myCommand.CommandText = "sp_revokedbaccess";
			myCommand.CommandType = CommandType.StoredProcedure;
			myCommand.Parameters.AddWithValue("@name_in_db", loginDb);  
			
			try
			{
				myCommand.ExecuteNonQuery();
				successRevoke = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyUserDb.SPRevokeDbAccess");
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, "sp_revokedbaccess");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UserCannotRevoked, loginDb), extendedInfo);
				myCommand.Dispose();
				return successRevoke;
			}
			
			return successRevoke;
		}
		#endregion

		#region GetCompanyDBConnectionString - Prepara la stringa di connessione per il database Aziendale
		/// <summary>
		/// GetCompanyDBConnectionString
		/// Prepara la stringa di connessione per connettersi al database della company. 
		/// Per fare ciò deve leggere i dati di MSD_Companies e MSD_Logins e costruirsi la stringa 
		/// di connessione utilizzando le info sul nome del database, tipologia di autenticazione e dbowner
		/// </summary>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <returns>connection, stringa di connessione</returns>
		//---------------------------------------------------------------------
		internal string GetCompanyDBConnectionString(string companyId)
		{
			string connectionString = string.Empty;

			if (string.IsNullOrWhiteSpace(companyId))
				return connectionString;

			SqlDataReader myReader = null;

			try
			{
				string myQuery =
						@"SELECT MSD_Companies.CompanyDBServer, MSD_Companies.CompanyDBName,
                          MSD_Companies.CompanyDBOwner, MSD_CompanyLogins.DBWindowsAuthentication,
                          MSD_CompanyLogins.DBUser, MSD_CompanyLogins.DBPassword
						  FROM MSD_Companies, MSD_CompanyLogins
                          WHERE MSD_Companies.CompanyId	= @CompanyId AND
                          MSD_Companies.CompanyDBOwner = MSD_CompanyLogins.LoginId AND
                          MSD_CompanyLogins.CompanyId = MSD_Companies.CompanyId;";

				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				myReader = myCommand.ExecuteReader();

				if (myReader == null)
					return connectionString;

				while (myReader.Read())
				{
					string companyDbServer = myReader["CompanyDBServer"].ToString();
					string companyDbName = myReader["CompanyDBName"].ToString();

					//Windows Authentication
					if (bool.Parse(myReader["DBWindowsAuthentication"].ToString()))
						connectionString = string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, companyDbServer, companyDbName);
					else
					{
						//Sql Authentication
						string companyDbUser = myReader["DBUser"].ToString();
						string companyDbPassword = Crypto.Decrypt(myReader["DBPassword"].ToString());
						connectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, companyDbServer, companyDbName, companyDbUser, companyDbPassword);
					}
				}
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyUserDb.GetCompanyDBConnectionString");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyUsersReading, extendedInfo);
				myReader.Close();
				return null;
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
				{
					myReader.Close();
					myReader.Dispose();
				}
			}

			return connectionString;
		}
		#endregion
		#endregion
	}
}