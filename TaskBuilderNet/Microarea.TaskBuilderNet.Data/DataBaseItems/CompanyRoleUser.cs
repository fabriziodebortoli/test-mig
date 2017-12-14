using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// CompanyRoleUserDb
	/// Gestisce i records della tabella MSD_CompanyRolesLogins
	/// </summary>
	//=========================================================================
	public class CompanyRoleLoginDb : DataBaseItem
	{
		#region Costruttori
		/// <summary>
		/// Costruttore 1
		/// (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public CompanyRoleLoginDb()	
		{}
		
		/// <summary>
		/// Costruttore 2
		/// inizializza la connessione
		/// </summary>
		//---------------------------------------------------------------------
		public CompanyRoleLoginDb (string connectionString)
		{
			ConnectionString = connectionString;
		}
		#endregion

		#region Add - Associa un Utente a un Ruolo di una Azienda
		/// <summary>
		/// Add
		/// Aggiunge un nuovo record alla tabella MSD_CompanyRolesLogins, ovvero
		/// assegna un utente a un ruolo di una azienda
		/// </summary>
		/// <param name="loginId">Id dell'utente da assegnare al ruolo</param>
		/// <param name="companyId">Id dell'azienda a cui il ruolo è associato</param>
		/// <param name="roleId">Id del ruolo associato all'azienda</param>
		//---------------------------------------------------------------------
		public bool Add (string loginId, string companyId, string roleId)
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
				@"INSERT INTO MSD_CompanyRolesLogins(CompanyId, LoginId, RoleId) 
                  VALUES (@CompanyId, @LoginId, @RoleId)"; 

				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				myCommand.Parameters.Add(new SqlParameter("@LoginId",	Int32.Parse(loginId)));
				myCommand.Parameters.Add(new SqlParameter("@RoleId",	Int32.Parse(roleId)));
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.RoleUserInserting, extendedInfo);
			}
			
			return result;
		}
		#endregion

		#region Modify - Modifica di un Utente associato al Ruolo di una Azienda
		/// <summary>
		/// Modify
		/// Modifica i dati di un utente assegnato a un ruolo di una azienda (tabella MSD_CompanyRolesLogins)
		/// </summary>
		/// <param name="companyRoleLogin">classe con i dati da scrivere sulla tabella</param>
		//---------------------------------------------------------------------
		public bool Modify (CompanyRoleLoginItem companyRoleLogin)
		{
			bool result = false;
			SqlTransaction myTransSql;
			SqlCommand myCommand  = new SqlCommand();
			myTransSql			  = CurrentSqlConnection.BeginTransaction();
			myCommand.Connection  = CurrentSqlConnection;
			myCommand.Transaction = myTransSql;

			try
			{
				string strQuery = 
				@"UPDATE MSD_CompanyRolesLogins SET CompanyId = @CompanyId, RoleId = @RoleId, LoginId = @LoginId
                  WHERE LoginId = @LoginId AND CompanyId = @CompanyId AND RoleId = @RoleId";

				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@LoginId"  , Int32.Parse(companyRoleLogin.LoginId)));
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyRoleLogin.CompanyId)));
				myCommand.Parameters.Add(new SqlParameter("@RoleId"   ,	Int32.Parse(companyRoleLogin.RoleId)));
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.Modify");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyRoleUsersModify, extendedInfo);
			}

			return result;
		}
		#endregion

		#region DeleteCompanyLoginToRole - Cancellazione di un Utente associato al Ruolo di una Azienda
		/// <summary>
		/// DeleteCompanyLoginToRole
		/// Cancella l'assegnazione di un utente al ruolo dell'azienda
		/// Utilizzo la stored procedure MSD_DeleteCompanyRoleLogin
		/// </summary>
		/// <param name="LoginId">Id utente</param>
		/// <param name="CompanyId">Id azienda a cui il ruolo è associato</param>
		/// <param name="RoleId">Id del ruolo dell'azienda a cui l'utente è assegnato</param>
		//---------------------------------------------------------------------
		public bool Delete (string LoginId, string CompanyId, string RoleId)
		{
			bool result = false;
			SqlCommand myCommand	= new SqlCommand();
			myCommand.Connection	= CurrentSqlConnection;
			myCommand.CommandText	= "MSD_DeleteCompanyRoleLogin";
			myCommand.CommandType	= CommandType.StoredProcedure;
			myCommand.Parameters.AddWithValue("@par_companyid", Int32.Parse(CompanyId));
			myCommand.Parameters.AddWithValue("@par_loginid",   Int32.Parse(LoginId));
			myCommand.Parameters.AddWithValue("@par_roleid",    Int32.Parse(RoleId));

			try
			{
				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.Delete");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.UserRoleDeleting, extendedInfo);
			}

			return result;
		}
		#endregion

		#region Funzioni di Ricerca

		#region SelectLoginCompanyRole - Seleziona un Utente (assegnato al Ruolo di una Azienda) attraverso il suo LoginId
		/// <summary>
		/// SelectLoginCompanyRole
		/// Seleziona i dati di un utente assegnato a un ruolo di una azienda
		/// </summary>
		/// <param name="companyRolesLogins">Array con i dati dell'utente</param>
		/// <param name="companyId">Id dell'azienda</param>
		/// <param name="roleId">Id del ruolo</param>
		/// <param name="loginId">Id dell'utente</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectLoginCompanyRole(out ArrayList companyRolesLogins, string companyId, string roleId, string loginId)
		{
			ArrayList localCompanyRolesLogins = new ArrayList();
			bool mySuccessFlag = false;

			try
			{
				SqlDataReader myDataReader;
				if (GetLoginCompanyRole(out myDataReader, companyId, roleId, loginId))
				{
					while(myDataReader.Read())
					{
						CompanyRoleLogin userItem      = new CompanyRoleLogin();
						userItem.CompanyId			   = myDataReader["CompanyId"].ToString();
						userItem.LoginId			   = myDataReader["LoginId"].ToString();
						userItem.RoleId				   = myDataReader["RoleId"].ToString();
						userItem.Role                  = myDataReader["Role"].ToString();
						userItem.Login				   = myDataReader["Login"].ToString();
						userItem.Password			   = Crypto.Decrypt(myDataReader["Password"].ToString());
						userItem.Description		   = myDataReader["Description"].ToString();
						userItem.LastModifyGrants	   = myDataReader["LastModifyGrants"].ToString();
						userItem.ExpiredDatePassword   = myDataReader["ExpiredDatePassword"].ToString();
						userItem.Disabled			   = bool.Parse(myDataReader["Disabled"].ToString());
						userItem.WindowsAuthentication = bool.Parse(myDataReader["WindowsAuthentication"].ToString());
						userItem.DBDefaultUser		   = myDataReader["DBUser"].ToString();
						userItem.DBDefaultPassword     = Crypto.Decrypt(myDataReader["DBPassword"].ToString());
						userItem.Admin				   = bool.Parse(myDataReader["Admin"].ToString());
						userItem.EasyBuilderDeveloper  = bool.Parse(myDataReader["EBDeveloper"].ToString());
						userItem.Disabled			   = bool.Parse(myDataReader["Disabled"].ToString());
						localCompanyRolesLogins.Add(userItem);
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.SelectLoginCompanyRole");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyRolesUsersReading, extendedInfo);
				mySuccessFlag = false;
			}
			
			companyRolesLogins = localCompanyRolesLogins;
			return mySuccessFlag;
		}
		
		/// <summary>
		/// GetLoginCompanyRole
		/// Riempie un dataReader con i dati di un utente assegnato a un ruolo di una azienda
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati dell'utente</param>
		/// <param name="companyId">Id dell'azienda a cui l'utente è associato</param>
		/// <param name="roleId">Id del ruolo dell'azienda a cui l'utente è associato</param>
		/// <param name="loginId">Id dell'utente</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetLoginCompanyRole(out	SqlDataReader myDataReader, string companyId, string roleId, string loginId)
		{
			SqlDataReader mylocalDataReader = null;
			bool		  mySuccessFlag		= true;

			string myQuery =
			@"SELECT MSD_CompanyLogins.LoginId, MSD_CompanyLogins.DBUser, MSD_CompanyLogins.DBPassword,
                MSD_CompanyLogins.Admin, MSD_CompanyLogins.EBDeveloper, MSD_CompanyLogins.LastModifyGrants, MSD_CompanyLogins.Disabled,
                MSD_Logins.Login, MSD_Logins.Password, MSD_Logins.Description, MSD_Logins.ExpiredDatePassword,
                MSD_Logins.WindowsAuthentication, MSD_CompanyRolesLogins.CompanyId, MSD_CompanyRolesLogins.RoleId,
				MSD_CompanyRoles.Role, MSD_CompanyRolesLogins.LoginId
                FROM MSD_CompanyLogins, MSD_CompanyRoles, MSD_Logins, MSD_CompanyRolesLogins
                WHERE MSD_CompanyRolesLogins.CompanyId = @CompanyId AND MSD_CompanyRolesLogins.RoleId = @RoleId AND
                MSD_CompanyRoles.RoleId = @RoleId AND MSD_CompanyLogins.CompanyId = MSD_CompanyRolesLogins.CompanyId AND
                MSD_CompanyLogins.LoginId = MSD_CompanyRolesLogins.LoginId AND 
				MSD_CompanyLogins.LoginId = MSD_Logins.LoginId AND MSD_Logins.LoginId = @LoginId ";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				myCommand.Parameters.Add(new SqlParameter("@RoleId",    Int32.Parse(roleId)));
				myCommand.Parameters.Add(new SqlParameter("@Loginid",   Int32.Parse(loginId)));
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.GetLoginCompanyRole");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyLogins, MSD_CompanyRoles, MSD_Logins, MSD_CompanyRolesLogins"), extendedInfo);
				mySuccessFlag = false;
			}
			
			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectAll - Seleziona tutti gli Utenti assegnati al Ruolo di una Azienda
		/// <summary>
		/// SelectAll
		/// Seleziona tutti gli utenti assegnati a una certa azienda e a un certo ruolo
		/// </summary>
		/// <param name="companyRolesLogins">Array con gli utenti</param>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <param name="roleId">Id che identifica il ruolo</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//--------------------------------------------------------------------
		public bool SelectAll(out ArrayList companyRolesLogins, string companyId, string roleId)
		{
			ArrayList localCompanyRolesLogins = new ArrayList();
			bool mySuccessFlag = true;

			try
			{
				SqlDataReader myDataReader;
				if (GetAllLoginsCompanyRole(out myDataReader, companyId, roleId))
				{
					while(myDataReader.Read())
					{
						CompanyRoleLogin userItem = new CompanyRoleLogin();
						userItem.CompanyId			   = myDataReader["CompanyId"].ToString();
						userItem.LoginId			   = myDataReader["LoginId"].ToString();
						userItem.RoleId				   = myDataReader["RoleId"].ToString();
						userItem.Login				   = myDataReader["Login"].ToString();
						userItem.Password			   = Crypto.Decrypt(myDataReader["Password"].ToString());
						userItem.Description           = myDataReader["Description"].ToString();
						userItem.LastModifyGrants      = myDataReader["LastModifyGrants"].ToString();
						userItem.ExpiredDatePassword   = myDataReader["ExpiredDatePassword"].ToString();
						userItem.Disabled			   = bool.Parse(myDataReader["Disabled"].ToString());
						userItem.WindowsAuthentication = bool.Parse(myDataReader["WindowsAuthentication"].ToString());
						userItem.DBDefaultUser		   = myDataReader["DBUser"].ToString();
						userItem.DBDefaultPassword     = Crypto.Decrypt(myDataReader["DBPassword"].ToString());
						userItem.Admin				   = bool.Parse(myDataReader["Admin"].ToString());
						userItem.EasyBuilderDeveloper  = bool.Parse(myDataReader["EBDeveloper"].ToString());
						userItem.Disabled              = bool.Parse(myDataReader["Disabled"].ToString());
						localCompanyRolesLogins.Add(userItem);
					}
					myDataReader.Close();
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.SelectAll");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyRolesUsersReading, extendedInfo);
				mySuccessFlag = false;
				companyRolesLogins = null;
				return mySuccessFlag;
			}
			
			companyRolesLogins = localCompanyRolesLogins;
			return mySuccessFlag;
		}

		/// <summary>
		/// GetAllLoginsCompanyRole
		/// Riempie un dataReader con tutti gli utenti assegnati a una certa azienda e a un certo ruolo
		/// </summary>
		/// <param name="myDataReader">dataReader con gli utenti</param>
		/// <param name="companyId">Id dell'azienda</param>
		/// <param name="roleId">Id del ruolo</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllLoginsCompanyRole(out SqlDataReader myDataReader, string companyId, string roleId)
		{
			SqlDataReader mylocalDataReader = null;	
			bool mySuccessFlag = true;

			string myQuery =
			@"SELECT MSD_CompanyLogins.LoginId, MSD_CompanyLogins.DBUser, MSD_CompanyLogins.DBPassword,
              MSD_CompanyLogins.Admin, MSD_CompanyLogins.EBDeveloper, MSD_CompanyLogins.LastModifyGrants, MSD_CompanyLogins.Disabled,
              MSD_Logins.Login, MSD_Logins.Password, MSD_Logins.Description, MSD_Logins.ExpiredDatePassword,
              MSD_Logins.WindowsAuthentication, MSD_CompanyRolesLogins.CompanyId, MSD_CompanyRolesLogins.RoleId,
              MSD_CompanyRolesLogins.LoginId
              FROM MSD_CompanyLogins, MSD_Logins, MSD_CompanyRolesLogins
              WHERE MSD_CompanyRolesLogins.CompanyId = @CompanyId AND MSD_CompanyRolesLogins.RoleId = @RoleId AND
              MSD_CompanyLogins.CompanyId = MSD_CompanyRolesLogins.CompanyId AND
              MSD_CompanyLogins.LoginId = MSD_CompanyRolesLogins.LoginId AND 
			  MSD_CompanyLogins.LoginId = MSD_Logins.LoginId ORDER BY MSD_Logins.Login";
				
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery,CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				myCommand.Parameters.Add(new SqlParameter("@RoleId",    Int32.Parse(roleId)));
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.GetAllLoginsCompanyRole");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyLogins, MSD_Logins, MSD_CompanyRolesLogins"), extendedInfo);
				mySuccessFlag = false;
				myDataReader = null;
				return mySuccessFlag;
			}
			
			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectAllRoles - Seleziona tutti i Ruoli di una Azienda a cui l'Utente è associato
		/// <summary>
		/// SelectAllRoles
		/// Seleziona tutti i Ruoli di una azienda a cui l'utente è associato
		/// </summary>
		/// <param name="companyRolesLogins">Array con i ruoli trovati</param>
		/// <param name="companyId">Id dell'azienda</param>
		/// <param name="loginId">Id dell'utente</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllRoles (out ArrayList companyRolesLogins, string companyId, string loginId)
		{
			ArrayList localCompanyRolesLogins = new ArrayList();
			bool	  mySuccessFlag			  = true;
			
			try
			{
				SqlDataReader myDataReader;
				if (GetAllRolesCompanyLogin(out myDataReader, companyId, loginId))
				{
					while(myDataReader.Read())
					{
						RoleItem roleItem    = new RoleItem();
						roleItem.CompanyId	 = myDataReader["CompanyId"].ToString();
						roleItem.RoleId		 = myDataReader["RoleId"].ToString();
						roleItem.Role		 = myDataReader["Role"].ToString();
						roleItem.Description = myDataReader["Description"].ToString();
						localCompanyRolesLogins.Add(roleItem);
					}
					myDataReader.Close();
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.SelectAllRoles");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyRolesUsersReading, extendedInfo);
				mySuccessFlag = false;
				companyRolesLogins = null;
				return mySuccessFlag;
			}
			
			companyRolesLogins = localCompanyRolesLogins;
			return mySuccessFlag;
		}
		
		/// <summary>
		/// GetAllRolesCompanyLogin
		/// Riempie un dataReader con tutti gli utenti assegnati a una azienda e a un ruolo specificati
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati degli utenti</param>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <param name="loginId">Id che identifica l'utente</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllRolesCompanyLogin(out SqlDataReader myDataReader, string companyId, string loginId)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;
			
			string myQuery = 
			@"SELECT MSD_CompanyRolesLogins.RoleId, MSD_CompanyRolesLogins.CompanyId,
              MSD_CompanyRoles.Description, MSD_CompanyRoles.Role
			  FROM MSD_CompanyRoles, MSD_CompanyRolesLogins
			  WHERE MSD_CompanyRolesLogins.CompanyId = @CompanyId AND MSD_CompanyRolesLogins.LoginId = @LoginId AND
              MSD_CompanyRolesLogins.RoleId = MSD_CompanyRoles.RoleId ORDER BY MSD_CompanyRoles.Role";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery,CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(companyId)));
				myCommand.Parameters.Add(new SqlParameter("@LoginId"  ,	Int32.Parse(loginId)));
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.GetAllRolesCompanyLogin");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_CompanyRoles, MSD_CompanyRolesLogins"), extendedInfo);
				mySuccessFlag = false;
				myDataReader = null;
				return mySuccessFlag;
			}
			
			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region ExistUserCompany - True se l'Utente è associato al Ruolo specificato
		/// <summary>
		/// ExistUserCompany
		/// ritorna true se l'utente specificato da LoginId è presente nella tabella MSD_CompanyRolesLogins
		/// </summary>
		/// <param name="LoginId">Id dell'utente da ricercare</param>
		/// <param name="RoleId">Id del ruole dell'utente</param>
		/// <param name="CompanyId">Id dell'azienda a cui l'utente è associato</param>
		/// <returns>result, 1 se l'utente dell'azienda se esiste, 0 altrimenti</returns>
		//---------------------------------------------------------------------
		public int ExistUserCompany(string LoginId, string RoleId, string CompanyId)
		{
			int	result = 0;
			SqlDataReader mylocalDataReader = null;
			
			string myQuery = 
			@"SELECT COUNT(*) FROM MSD_CompanyRolesLogins 
			  WHERE LoginId = @LoginId AND CompanyId = @CompanyId AND RoleId = @RoleId";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery,CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(CompanyId));
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(LoginId));
				myCommand.Parameters.AddWithValue("@RoleId", Int32.Parse(RoleId));
				mylocalDataReader = myCommand.ExecuteReader();
				if (mylocalDataReader.Read())
					result = mylocalDataReader.GetInt32(0);
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "CompanyRoleLoginDb.ExistUserCompany");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyRolesUsersReading, extendedInfo);
			}

			mylocalDataReader.Close();
			return result;
		}
		#endregion

		#endregion
	}
}