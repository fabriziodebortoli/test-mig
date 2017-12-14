using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// UserDb
	/// Classe che gestisce i records della tabella MSD_Logins
	/// </summary>
	//========================================================================
	public class UserDb : DataBaseItem
	{
		#region Costruttori
		/// <summary>
		/// Costruttore 1 (vuoto)
		/// </summary>
		//---------------------------------------------------------------------
		public UserDb()	
		{}
		
		/// <summary>
		/// Costruttore 2
		/// Inizializza la stringa di connessione
		/// </summary>
		//---------------------------------------------------------------------
		public UserDb (string connectionString)
		{
			ConnectionString = connectionString;
		}
		#endregion
		
		#region Add - Inserimento di un nuovo Utente Applicativo (Login)
		/// <summary>
		/// Add
		/// Aggiunge un nuovo record alla tabella MSD_Logins
		/// </summary>
		//---------------------------------------------------------------------
		public bool Add
			(
				bool	windowsAuthentication, 
				string	login,
				string	password, 
				string	description,
				string	expirationDate, 
				bool	disabled,
				bool	userMustChangePassword,
				bool	userCannotChangePassword,
				bool	expiredDateCannotChange,
				bool	passwordNeverExpired,
				string  preferredLanguage,
				string  applicationLanguage,
			    string  eMailAddress,
				bool    webAccess,
				bool    smartClientAccess,
				bool	concurrentAccess,
				bool    locked,
				bool	privateAreaWebSiteAdmin,  //area privata sul sito web del produttore
				string  nrLoginFailedCount/*,
            int  balloonBlockedType*/
			)
		{
			bool result = false;
			SqlTransaction  myTransUtenteSql;
			SqlCommand myCommand  = new SqlCommand();
			myTransUtenteSql      = CurrentSqlConnection.BeginTransaction();
			myCommand.Connection  = CurrentSqlConnection;
			myCommand.Transaction = myTransUtenteSql;

			try
			{
				string strQuery =
                @"INSERT INTO MSD_Logins
				(Login, Password, Description, ExpiredDatePassword, LastModifyGrants, Disabled, WindowsAuthentication,
				 UserMustChangePassword, UserCannotChangePassword, ExpiredDateCannotChange, PasswordNeverExpired,
				 PreferredLanguage, ApplicationLanguage, Email, SmartClientAccess, WebAccess, ConcurrentAccess, 
				 Locked, PrivateAreaWebSiteAccess, LoginFailedCount) 
				 VALUES 
                 (@Login, @Password, @Description, @ExpiredDatePassword, @LastModifyGrants, @Disabled, @WindowsAuthentication,
				  @UserMustChangePassword, @UserCannotChangePassword, @ExpiredDateCannotChange, @PasswordNeverExpired,
                  @PreferredLanguage, @ApplicationLanguage, @Email, @SmartClientAccess, @WebAccess, @ConcurrentAccess, 
				  @Locked, @PrivateAreaWebSiteAccess, @LoginFailedCount)"; 
				
				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@Login",						login));
				myCommand.Parameters.Add(new SqlParameter("@Password",					Crypto.Encrypt(password)));
				myCommand.Parameters.Add(new SqlParameter("@Description",				description));
				if (expirationDate.Length > 0)
					myCommand.Parameters.Add(new SqlParameter("@ExpiredDatePassword",	Convert.ToDateTime(expirationDate).Date));
				else
					myCommand.Parameters.Add(new SqlParameter("@ExpiredDatePassword",	expirationDate));
				myCommand.Parameters.Add(new SqlParameter("@Disabled",					disabled));
				myCommand.Parameters.Add(new SqlParameter("@LastModifyGrants",			DateTime.Now.Date));
				myCommand.Parameters.Add(new SqlParameter("@WindowsAuthentication",		windowsAuthentication));
				myCommand.Parameters.Add(new SqlParameter("@UserMustChangePassword",	userMustChangePassword));
				myCommand.Parameters.Add(new SqlParameter("@UserCannotChangePassword",	userCannotChangePassword));
				myCommand.Parameters.Add(new SqlParameter("@ExpiredDateCannotChange",	expiredDateCannotChange));
				myCommand.Parameters.Add(new SqlParameter("@PasswordNeverExpired",		passwordNeverExpired));
				myCommand.Parameters.Add(new SqlParameter("@PreferredLanguage",			preferredLanguage));
				myCommand.Parameters.Add(new SqlParameter("@Email",						eMailAddress));
				myCommand.Parameters.Add(new SqlParameter("@ApplicationLanguage",		applicationLanguage));
				myCommand.Parameters.Add(new SqlParameter("@SmartClientAccess",			smartClientAccess));
				myCommand.Parameters.Add(new SqlParameter("@ConcurrentAccess",			concurrentAccess));
				myCommand.Parameters.Add(new SqlParameter("@WebAccess",					webAccess));
				myCommand.Parameters.Add(new SqlParameter("@Locked",					locked));
				myCommand.Parameters.Add(new SqlParameter("@PrivateAreaWebSiteAccess",  privateAreaWebSiteAdmin));
				myCommand.Parameters.Add(new SqlParameter("@LoginFailedCount",			Convert.ToInt32(nrLoginFailedCount)));
                //myCommand.Parameters.Add(new SqlParameter("@BalloonBlockedType", balloonBlockedType)); 

				myCommand.ExecuteNonQuery();
				myTransUtenteSql.Commit();
				result = true;
			}
			catch (SqlException sqlException)
			{
				myTransUtenteSql.Rollback();
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UserInserting, login), extendedInfo);
			}
			return result;
		}
		#endregion

		#region Modify - Modifica di un Utente Applicativo

        //---------------------------------------------------------------------
        public void RemoveReserverdCal(string loginId)
        {

            SqlCommand sqlCommand = new SqlCommand();
            try
            {
                sqlCommand.CommandText = "DELETE FROM MSD_LoginsArticles WHERE LoginId = @LoginId";
                sqlCommand.Parameters.AddWithValue("@LoginId", loginId);
                sqlCommand.Connection = CurrentSqlConnection;
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Dispose();
            }
            catch (SqlException)
            {
                if (sqlCommand != null) sqlCommand.Dispose();
            }
        }



		/// <summary>
		/// Modify
		/// Modifica un record nella tabella MSD_Logins
		/// </summary>
		//---------------------------------------------------------------------
		public bool Modify
			(
				string	LoginId,
				bool	windowsAuthentication, 
				string	login,
				string	password, 
				string	description,
				string	expirationDate, 
				bool	disabled,
				bool	userMustChangePassword,
				bool	userCannotChangePassword,
				bool	expiredDateCannotChange,
				bool	passwordNeverExpired,
				string  preferredLanguage,
			    string  applicationLanguage,
			    string  eMailAddress,
				bool    webAccess,
				bool    smartClientAccess,
				bool	concurrentAccess,
			    bool    locked,
				bool	privateAreaWebSiteAdmin,  //area privata sul sito web del produttore
			    string  nrLoginFailedCount/*,
                int balloonBlockedType*/
			)
		{
			bool result = false;
			SqlTransaction myTransSql;
			SqlCommand myCommand = new SqlCommand();
			myTransSql = CurrentSqlConnection.BeginTransaction();
			myCommand.Connection = CurrentSqlConnection;
			myCommand.Transaction= myTransSql;
			
			try
			{
				string strQuery =
                @"UPDATE MSD_Logins SET 
				 Login = @Login, Password = @Password, Description = @Description, LastModifyGrants = @LastModifyGrants,
				 ExpiredDatePassword = @ExpiredDatePassword, Disabled = @Disabled, WindowsAuthentication = @WindowsAuthentication,
				 UserMustChangePassword = @UserMustChangePassword, UserCannotChangePassword = @UserCannotChangePassword,
				 ExpiredDateCannotChange = @ExpiredDateCannotChange, PasswordNeverExpired = @PasswordNeverExpired,
				 PreferredLanguage = @PreferredLanguage, ApplicationLanguage = @ApplicationLanguage, Email = @Email,
				 WebAccess = @WebAccess, SmartClientAccess = @SmartClientAccess, ConcurrentAccess=@ConcurrentAccess, Locked = @Locked,
				 LoginFailedCount = @LoginFailedCount, PrivateAreaWebSiteAccess = @PrivateAreaWebSiteAccess
				 WHERE LoginId = @LoginId";

				myCommand.CommandText = strQuery;
				myCommand.Parameters.Add(new SqlParameter("@LoginId",					LoginId));
				myCommand.Parameters.Add(new SqlParameter("@Login",						login));
				myCommand.Parameters.Add(new SqlParameter("@Password",					Crypto.Encrypt(password)));
				myCommand.Parameters.Add(new SqlParameter("@Description",				description));
				if (expirationDate.Length > 0)
					myCommand.Parameters.Add(new SqlParameter("@ExpiredDatePassword",	Convert.ToDateTime(expirationDate).Date));
				else
					myCommand.Parameters.Add(new SqlParameter("@ExpiredDatePassword",	expirationDate));
				myCommand.Parameters.Add(new SqlParameter("@Disabled",					disabled));
				myCommand.Parameters.Add(new SqlParameter("@WindowsAuthentication",		windowsAuthentication));
				myCommand.Parameters.Add(new SqlParameter("@LastModifyGrants",			DateTime.Today.Date));
				myCommand.Parameters.Add(new SqlParameter("@UserMustChangePassword",	userMustChangePassword));
				myCommand.Parameters.Add(new SqlParameter("@UserCannotChangePassword",	userCannotChangePassword));
				myCommand.Parameters.Add(new SqlParameter("@ExpiredDateCannotChange",	expiredDateCannotChange));
				myCommand.Parameters.Add(new SqlParameter("@PasswordNeverExpired",		passwordNeverExpired));
				myCommand.Parameters.Add(new SqlParameter("@PreferredLanguage",			preferredLanguage));
				myCommand.Parameters.Add(new SqlParameter("@ApplicationLanguage",		applicationLanguage));
				myCommand.Parameters.Add(new SqlParameter("@Email",						eMailAddress));
				myCommand.Parameters.Add(new SqlParameter("@WebAccess",					webAccess));
				myCommand.Parameters.Add(new SqlParameter("@SmartClientAccess",			smartClientAccess));
				myCommand.Parameters.Add(new SqlParameter("@ConcurrentAccess",			concurrentAccess));
				myCommand.Parameters.Add(new SqlParameter("@Locked",					locked));
				myCommand.Parameters.Add(new SqlParameter("@LoginFailedCount",			Convert.ToInt32(nrLoginFailedCount)));
                //myCommand.Parameters.Add(new SqlParameter("@BalloonBlockedType",balloonBlockedType));
				myCommand.Parameters.Add(new SqlParameter("@PrivateAreaWebSiteAccess",	privateAreaWebSiteAdmin));		
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.Modify");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UserModify, login), extendedInfo);
			}
			return result;
		}
		#endregion

		#region Delete - Cancellazione di un Utente Applicativo
		/// <summary>
		/// Delete
		/// Cancella dalla tabella MSD_Logins la LoginId specificata
		/// Viene utilizzata la storedProcedure MSD_DeleteLogin
		/// </summary>
		/// <param name="LoginId">identificativo dell'utente da cancellare</param>
		//---------------------------------------------------------------------
		public bool Delete(string loginId)
		{
			bool result = false;

			if (!UserIsDbowner(loginId))
			{
				SqlCommand myCommand	= new SqlCommand();
				myCommand.Connection	= CurrentSqlConnection;
				myCommand.CommandText	= "MSD_DeleteLogin";
				myCommand.CommandType	= CommandType.StoredProcedure;
				myCommand.Parameters.AddWithValue("@par_loginid", Int32.Parse(loginId));
				
				try
				{
					myCommand.ExecuteNonQuery();
					result = true;
				}
				catch (SqlException sqlException)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
					extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
					extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
					extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
					extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
					extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.Delete");
					extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
					Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CannotDeleteLoginDbo, extendedInfo);
				}
			}
			else
				Diagnostic.Set(DiagnosticType.Warning, DatabaseItemsStrings.CannotDeleteLoginDbo);

			return result;
		}
		#endregion

		#region Funzioni di Ricerca e Selezione

		#region UserIsNotDbo - True se la LoginId è un dbo per qualche company
		/// <summary>
		/// UserIsNotDbo
		/// True se l'utente applicativo è un dbowner (di una qualche azienda), false altrimenti
		/// </summary>
		//---------------------------------------------------------------------
		public bool UserIsDbowner(string loginId)
		{
			int result = 0;
			string myQuery = @"SELECT COUNT(*) FROM MSD_Companies WHERE CompanyDBOwner = @LoginId AND Disabled = @Disabled";
			
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginId",	loginId);
				myCommand.Parameters.AddWithValue("@Disabled",	false);
				result = (int) myCommand.ExecuteScalar();
			}
			catch(SqlException)
			{}
			
			return (result > 0);
		}
		#endregion

		#region IsAssociated - True se l'utente è associato a una qualche azienda, false altrimenti
		/// <summary>
		/// IsAssociated
		/// True se l'utente è associato a una qualche azienda
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsAssociated(string loginId)
		{
			int result = 0;
			string myQuery = @"SELECT COUNT(*) FROM MSD_CompanyLogins WHERE LoginId = @LoginId AND Disabled = @Disabled";
			
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginId",	loginId);
				myCommand.Parameters.AddWithValue("@Disabled",	false);
				result = (int) myCommand.ExecuteScalar();
			}
			catch(SqlException)
			{}

			return (result > 0);
		}

		#endregion

		#region IsDisabled - True se la login utente è disabilitata, false altrimenti
		//---------------------------------------------------------------------
		public bool IsDisabled(string loginId)
		{
			bool isDisabled = false;
			string myQuery = @"SELECT MSD_Logins.Disabled FROM MSD_Logins WHERE LoginId = @LoginId";
                              
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginId", loginId);
				
				SqlDataReader myDataReader  = myCommand.ExecuteReader();
				while(myDataReader.Read())
					isDisabled =  Convert.ToBoolean( myDataReader["Disabled"].ToString() );

				myDataReader.Close();
			}
			catch(SqlException)
			{}

			return isDisabled;
		}
		#endregion

		#region LoadFromLogin - Trova l'Utente Applicativo specificato dal suo LoginName
		/// <summary>
		/// LoadFromLogin
		/// Dato il loginName, cerca in MSD_Logins il record corrispondente e lo carica in un array
		/// </summary>
		/// <param name="userData">array out in cui vengono caricati i dati</param>
		/// <param name="loginName">nome utente da ricercare</param>
		//---------------------------------------------------------------------
		public bool LoadFromLogin(out ArrayList userData, string loginName)
		{
			bool result = false;
			ArrayList localUser = new ArrayList();
			
			try
			{
				SqlDataReader myDataReader;
				if (GeDataFromLogin(out myDataReader, loginName))
				{
					while(myDataReader.Read())
					{
						UserItem userItem                 = new UserItem();
						userItem.LoginId                  = myDataReader["LoginId"].ToString();
						userItem.Login                    = myDataReader["Login"].ToString();
						userItem.Password                 = Crypto.Decrypt(myDataReader["Password"].ToString());
						userItem.WindowsAuthentication    = bool.Parse(myDataReader["WindowsAuthentication"].ToString());
						localUser.Add(userItem);
					}
	
					myDataReader.Close();
					result = true;
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.LoadFromLogin");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.UsersReading, loginName), extendedInfo);
			}

			userData = localUser;
			return result;
		}

		/// <summary>
		/// GetDataFromLogin
		/// Dato il loginName, riempie il dataReader con i dati dell'utente
		/// prelevati dalla tabella MSD_Logins
		/// </summary>
		/// <param name="myDataReader">datareader riempito con i dati utente</param>
		/// <param name="loginName">nome utente da ricercare</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		private bool GeDataFromLogin(out SqlDataReader myDataReader, string loginName)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery = @"SELECT LoginId, Login, Password, WindowsAuthentication FROM MSD_Logins 
								WHERE Login = @LoginName AND MSD_Logins.Disabled = @Disabled AND MSD_Logins.Locked = @Locked";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginName", loginName);
				myCommand.Parameters.AddWithValue("@Disabled",	false);
				myCommand.Parameters.AddWithValue("@Locked",	false);
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GeDataFromLogin");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectAllUsers - Seleziona tutti gli Utenti Applicativi
		/// <summary>
		/// SelectAllUsers
		/// Seleziona tutti gli utenti presenti in MSD_Logins
		/// Se selectedUsersDisabled= true seleziona anche gli utenti disabilitati,
		/// in caso contrario, li esclude
		/// </summary>
		/// <param name="users">Arraylist con i dati degli utenti</param>
		/// <param name="selectedUsersDisabled">true seleziona anche gli utenti
		/// disabilitati, false no</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllUsers(out ArrayList users, bool selectedUsersDisabled)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag = false;

			try
			{
				SqlDataReader myDataReader;
				if (GetAllUsers(out myDataReader, selectedUsersDisabled))
				{

					while(myDataReader.Read())
					{
						localUsers.Add(ReadUser(myDataReader));
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.SelectAllUsers");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
				mySuccessFlag = false;
			}

			users = localUsers;
			return mySuccessFlag;
		}

		//---------------------------------------------------------------------
		private UserItem ReadUser(SqlDataReader myDataReader)
		{
			UserItem userItem = new UserItem();
			if (myDataReader == null) return userItem;
			
				userItem.LoginId					= myDataReader["LoginId"].ToString();
				userItem.Login						= myDataReader["Login"].ToString();
				userItem.Password					= Crypto.Decrypt(myDataReader["Password"].ToString());
				userItem.Description				= myDataReader["Description"].ToString();
				userItem.LastModifyGrants			= myDataReader["LastModifyGrants"].ToString();
				userItem.ExpiredDatePassword		= myDataReader["ExpiredDatePassword"].ToString();
				userItem.PreferredLanguage			= myDataReader["PreferredLanguage"].ToString();
				userItem.ApplicationLanguage		= myDataReader["ApplicationLanguage"].ToString();
				userItem.EMailAddress				= myDataReader["Email"].ToString();
				userItem.NrLoginFailedCount			= int.Parse(myDataReader["LoginFailedCount"].ToString());
				userItem.Disabled					= bool.Parse(myDataReader["Disabled"].ToString());
				userItem.Locked						= bool.Parse(myDataReader["Locked"].ToString());
				userItem.WindowsAuthentication		= bool.Parse(myDataReader["WindowsAuthentication"].ToString());
				userItem.UserCannotChangePassword	= bool.Parse(myDataReader["UserCannotChangePassword"].ToString());
				userItem.UserMustChangePassword		= bool.Parse(myDataReader["UserMustChangePassword"].ToString());
				userItem.ExpiredDateCannotChange	= bool.Parse(myDataReader["ExpiredDateCannotChange"].ToString());
				userItem.PasswordNeverExpired		= bool.Parse(myDataReader["PasswordNeverExpired"].ToString());
				userItem.PrivateAreaAdmin			= bool.Parse(myDataReader["PrivateAreaWebSiteAccess"].ToString());
               // userItem.BalloonBlockedType =int.Parse(myDataReader["BalloonBlockedType"].ToString());
				////prima gli attributi erano indipendenti.
				//ora sono esclusivi, imposto i  flag con questo ordine 
				//in modo da amantenere una gerarchia in caso ci fossero attributi multipli a true
				userItem.SmartClientAccess			= bool.Parse(myDataReader["SmartClientAccess"].ToString());
				if (!userItem.SmartClientAccess)
				{
					userItem.ConcurrentAccess = bool.Parse(myDataReader["ConcurrentAccess"].ToString());
					if (!userItem.ConcurrentAccess)
						userItem.WebAccess = bool.Parse(myDataReader["WebAccess"].ToString());
				}
			
			return userItem;
		}

		/// <summary>
		/// GetAllUsers
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati degli utenti</param>
		/// <param name="selectedUsersDisabled">se true, comprende anche gli utenti
		/// disabilitati, se false li esclude</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllUsers(out SqlDataReader myDataReader, bool selectedUsersDisabled)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery = 
				(!selectedUsersDisabled)
				? "SELECT * FROM MSD_Logins WHERE MSD_Logins.Disabled = @Disabled AND MSD_Logins.Locked = @Locked ORDER BY MSD_Logins.Login"
				: "SELECT * FROM MSD_Logins ORDER BY MSD_Logins.Login";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				if (!selectedUsersDisabled)
				{
					myCommand.Parameters.AddWithValue("@Disabled",	false);
					myCommand.Parameters.AddWithValue("@Locked",	false);
				}
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetAllUsers");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectAllUsersExceptSa - Seleziona tutti gli Utenti Applicativi eccetto, se esiste, l'utente sa
		/// <summary>
		/// SelectAllUsersExceptSa
		/// Seleziona tutti gli utenti in MSD_Logins, eccetto l'utente
		/// sa (se esiste); se selectedUsersDisabled=TRUE considera
		/// anche gli utenti disabilitati, altrimenti li esclude
		/// </summary>
		/// <param name="users">Arraylist con i dati degli utenti</param>
		/// <param name="selectedUsersDisabled">true seleziona anche gli utenti
		/// disabilitati, false no</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllUsersExceptSa(out ArrayList users, bool selectedUsersDisabled)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag = false;

			try
			{
				SqlDataReader myDataReader;
				if (GetAllUsersExceptSa(out myDataReader, selectedUsersDisabled))
				{
					while(myDataReader.Read())
					{
						localUsers.Add(ReadUser(myDataReader));
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.SelectAllUsersExceptSa");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
				mySuccessFlag = false;
			}

			users = localUsers;
			return mySuccessFlag;
		}

		/// <summary>
		/// GetAllUsersExceptSa
		/// Riempie il dataReader con tutti gli utenti che possono essere associati
		/// a una azienda, con l'esclusione (se esiste) dell'utente sa (in quanto può
		/// solo essere il dbowner, e pertanto non può essere associato - Vincolo di 
		/// Microsoft SQL Server)
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati degli utenti</param>
		/// <param name="selectedUsersDisabled">true, seleziono anche gli utenti 
		/// disabilitati, false li escludo </param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllUsersExceptSa(out SqlDataReader myDataReader, bool selectedUsersDisabled)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery =
				(!selectedUsersDisabled)
				? @"SELECT * FROM MSD_Logins WHERE MSD_Logins.Disabled = @Disabled AND 
					MSD_Logins.Locked = @Locked AND MSD_Logins.Login NOT LIKE @LoginSa ORDER BY MSD_Logins.Login"
				: @"SELECT * FROM MSD_Logins WHERE MSD_Logins.Login NOT LIKE @LoginSa ORDER BY MSD_Logins.Login";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				if (!selectedUsersDisabled)
				{
					myCommand.Parameters.AddWithValue("@Disabled",	false);
					myCommand.Parameters.AddWithValue("@Locked",	false);
				}

				myCommand.Parameters.AddWithValue("@LoginSa", DatabaseLayerConsts.LoginSa);
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetAllUsersExceptSa");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectAllUsersExceptLocal - Seleziona tutti gli Utenti Applicativi escludendo gli utenti locali (<computer name>\nome utente)
		/// <summary>
		/// SelectAllUsersExceptLocal
		/// Seleziono tutti gli utenti che non sono login del tipo local computer\loginName 
		/// </summary>
		/// <param name="users">Arraylist con i dati degli utenti</param>
		/// <param name="serverName"></param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllUsersExceptLocal(out ArrayList users, string serverName)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag = false;

			try
			{
				SqlDataReader myDataReader;
				if (GetAllUsersExceptLocals(out myDataReader, serverName))
				{
					while (myDataReader.Read())
					{
						localUsers.Add(ReadUser(myDataReader));
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.SelectAllUsersExceptLocal");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
				mySuccessFlag = false;
			}

			users = localUsers;
			return mySuccessFlag;
		}

		/// <summary>
		/// GetAllUsersExceptLocals
		/// Riempie un datareader con i dati degli utenti prelevati dalla tabella 
		/// MSD_Logins il cui nome non sia localcomputer/loginane e che non sia disabilitato
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati degli utenti </param>
		/// <param name="serverName">Nome del server locale</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllUsersExceptLocals (out SqlDataReader myDataReader, string serverName)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery= @"SELECT * FROM MSD_Logins WHERE SUBSTRING(MSD_Logins.Login, 1, @ServerLen) <> @ServerName AND
                           MSD_Logins.Disabled = @Disabled AND MSD_Logins.Locked = @Locked ORDER BY MSD_Logins.Login";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@ServerLen", serverName.Length);
				myCommand.Parameters.AddWithValue("@ServerName", serverName);
				myCommand.Parameters.AddWithValue("@Disabled", false);
				myCommand.Parameters.AddWithValue("@Locked", false);
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
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetAllUsersExceptLocals");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectAllNTUsers - Seleziona tutti gli Utenti Applicativi con autenticazione NT
		/// <summary>
		/// SelectAllNTUsers
		/// Seleziona tutti gli utenti con login di tipo NT
		/// </summary>
		/// <param name="users">ArrayList con i dati degli utenti</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllNTUsers(out ArrayList users)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag = false;

			try
			{
				SqlDataReader myDataReader;
				if (GetAllNTUsers(out myDataReader))
				{
					while (myDataReader.Read())
					{
						localUsers.Add(ReadUser(myDataReader));
					}	
					myDataReader.Close();
					mySuccessFlag = true;
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.SelectAllNTUsers");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
				mySuccessFlag = false;
			}
			users = localUsers;
			return mySuccessFlag;
		}

		/// <summary>
		/// GetAllNTUsers
		/// Riempie un dataReader con i dati degli utenti non disabilitati
		/// con login di tipo NT, presenti in MSD_Logins
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati degli utenti</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllNTUsers (out SqlDataReader myDataReader)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery = @"SELECT * FROM MSD_Logins WHERE WindowsAuthentication = 1 AND 
								MSD_Logins.Disabled = @Disabled AND MSD_Logins.Locked = @Locked ORDER BY MSD_Logins.Login";
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@Disabled", false);
				myCommand.Parameters.AddWithValue("@Locked", false);
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				string message = string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins");
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetAllNTUsers");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectAllNTUsersExceptLocal - Seleziona tutti gli Utenti Applicativi con autenticazione NT, escludendo gli utenti locali (<computer name>\nome utente)

		/// <summary>
		/// SelectAllNTUsersExceptLocal
		/// Seleziona tutti gli utenti NT il cui nome non sia del tipo
		/// serverName\loginname (ovvero prende in considerazione gli utenti
		/// di DOMINIO e non quelli locali al pc)
		/// </summary>
		/// <param name="users">ArrayList con i dati degli utenti</param>
		/// <param name="serverName">Nome del server</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllNTUsersExceptLocal(out ArrayList users, string serverName)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag = false;

			try
			{
				SqlDataReader myDataReader;
				if (GetAllNTUsersExceptLocal(out myDataReader, serverName))
				{
					while (myDataReader.Read())
					{
						localUsers.Add(ReadUser(myDataReader));
					}	
					myDataReader.Close();
					mySuccessFlag = true;
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.SelectAllNTUsersExceptLocal");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
				mySuccessFlag = false;
			}

			users = localUsers;
			return mySuccessFlag;
		}

		/// <summary>
		/// GetAllNTUsersExceptLocal
		/// Riempi un DataReader con i dati degli utenti il cui nome non sia del tipo
		/// servername\loginame, e che non risulti disabilitato, presenti nella tabella MSD_Logins
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati degli utenti</param>
		/// <param name="serverName">Nome del server</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllNTUsersExceptLocal (out SqlDataReader myDataReader, string serverName)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery = @"SELECT * FROM MSD_Logins WHERE WindowsAuthentication = 1 AND
                               SUBSTRING(MSD_Logins.Login, 1, @ServerLen) <> @ServerName AND
                               MSD_Logins.Disabled = @Disabled AND MSD_Logins.Locked = @Locked ORDER BY MSD_Logins.Login";
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@ServerName", serverName);
				myCommand.Parameters.AddWithValue("@Disabled", false);
				myCommand.Parameters.AddWithValue("@Locked", false);
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetAllNTUsersExceptLocal");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
			}
	
			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region SelectAllUsersForCompany - Seleziona tutti gli Utenti Applicativi associati a una Azienda
		/// <summary>
		/// SelectAllUsersForCompany
		/// Seleziona tutti gli utenti che possono essere associati a una azienda, con
		/// l'esclusione dell'utente sa (può soltanto essere il dbowner, e pertanto
		/// non può essere associato - Vincolo di Microsoft SQL Server)
		/// </summary>
		/// <param name="users">ArrayList con i dati degli utenti</param>
		/// <returns>successFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllUsersForCompany(out ArrayList users)
		{
			ArrayList localUsers = new ArrayList();
			bool mySuccessFlag   = false;
			SqlDataReader myAllUsersDataReader;

			try
			{
				if (GetAllUsersForCompany(out myAllUsersDataReader))
				{
					while (myAllUsersDataReader.Read())
					{
						localUsers.Add(ReadUser(myAllUsersDataReader));
					}	
					
					myAllUsersDataReader.Close();
					mySuccessFlag = true;
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.SelectAllUsersForCompany");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
				mySuccessFlag = false;
			}
			
			users = localUsers;
			return mySuccessFlag;
		}
		
		/// <summary>
		/// GetAllUsersForCompany
		/// Trova tutti gli utenti che possono essere associati a una azienda (ovvero che risultano non disabilitati)
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati degli utenti</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllUsersForCompany(out SqlDataReader myDataReader)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery = @"SELECT * FROM MSD_Logins WHERE MSD_Logins.Disabled = @Disabled AND 
								MSD_Logins.Locked = @Locked ORDER BY MSD_Logins.Login";
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.Add(new SqlParameter("@Disabled",	false));
				myCommand.Parameters.Add(new SqlParameter("@Locked",	false));
				mylocalDataReader = myCommand.ExecuteReader();
				mySuccessFlag = true;
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetAllUsersForCompany");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
			}

			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}
		#endregion

		#region GetAllUserFieldsById - Seleziona un Utente Applicativo specificato da LoginId
		/// <summary>
		/// GetAllUserFieldsById
		/// Trovo tutti i dati di un utente specificato dalla loginId e riempio un ArrayList
		/// </summary>
		/// <param name="User">ArrayList con i dati dell'utente</param>
		/// <param name="LoginId">LoginId da trovare</param>
		/// <returns>successFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetAllUserFieldsById (out ArrayList User, string LoginId)
		{
			ArrayList localUser       = new ArrayList();
			UserItem userItem          = new UserItem();
			bool mySuccessFlag         = false;
			SqlDataReader myDataReader = null;

			try
			{
				if (GetUserId(out myDataReader, LoginId))
				{
					while(myDataReader.Read())
					{
						localUser.Add(ReadUser(myDataReader));
					}
					myDataReader.Close();
					mySuccessFlag = true;
				}
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetAllUserFieldsById");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
				mySuccessFlag = false;
			}
			User = localUser;

			if (User.Count == 0)
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.UnableToFindUser);
			return mySuccessFlag && User.Count > 0;
		}
		
		/// <summary>
		/// GetUserId
		/// Riempio un dataReader con i dati anagrafici dell'utente identificato dalla LoginId e prelevati da MSD_Logins
		/// </summary>
		/// <param name="myDataReader">DataReader con i dati dell'utente</param>
		/// <param name="LoginId">LoginId che identifica l'utente</param>
		/// <returns>mySuccessFlag, true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool GetUserId(out SqlDataReader myDataReader, string LoginId)
		{
			SqlDataReader mylocalDataReader = null;
			bool mySuccessFlag = true;

			string myQuery= "SELECT * FROM MSD_Logins WHERE LoginId = @LoginId";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(LoginId));
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.GetUserId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins"), extendedInfo);
				mySuccessFlag = false;
			}
			
			myDataReader = mylocalDataReader;
			return mySuccessFlag;
		}

		/// <summary>
		/// Cerca in MSD_Logins il loginId identificato dal LoginName
		/// </summary>
		/// <param name="loginName">login name da ricercare</param>
		/// <returns>loginId della login (se 0 significa che non lo ha trovato)</returns>
		//---------------------------------------------------------------------
		public int GetIdFromLoginName(string loginName)
		{
			int loginId = 0;
			string myQuery = "SELECT LoginId FROM MSD_Logins WHERE Login = @LoginName";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginName", loginName);

				using (SqlDataReader myReader = myCommand.ExecuteReader())
					while (myReader.Read())
						loginId = Convert.ToInt32(myReader["LoginId"]);
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "UserDb.GetIdFromLoginName");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.ReadingTable, "MSD_Logins"), extendedInfo);
			}

			return loginId;
		}
		#endregion

		#endregion

		#region Altre Funzioni

		#region LastLoginId - Trova la LoginId dell'ultimo Utente Applicativo inserito
		/// <summary>
		/// LastLoginId
		/// Legge la tabella MSD_Logins e ritorna la LoginId di valore
		/// più alto (LoginId è un campo autoincrementato, il max corrisponde all'ultimo record inserito)
		/// </summary>
		/// <returns>result = 0 se non ha trovato nulla o si è verificato
		/// un errore, il valore di loginId altrimenti</returns>
		//---------------------------------------------------------------------
		public int LastLoginId()
		{
			int result = 0;
			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;

			try
			{
				myCommand.CommandText = "SELECT MAX(LoginId) FROM MSD_Logins";
				result = (int)myCommand.ExecuteScalar();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "UserDb.LastLoginId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
			}
			
			return result;
		}
		#endregion
		
		#region ExistLogin - True se l'Utente Applicativo identificato da LoginName esiste nella tabella
		/// <summary>
		/// ExistLogin
		/// Cerca in MSD_Logins la login identificata da LoginName, a patto che non sia disabilitata
		/// </summary>
		/// <param name="loginName">login name da ricercare</param>
		/// <returns>true se ha trovato la login specificata, false altrimenti</returns>
		//---------------------------------------------------------------------
		public bool ExistLogin(string loginName)
		{
			int result = 0;
			string myQuery = "SELECT COUNT(*) FROM MSD_Logins WHERE Login = @LoginName AND Disabled = @Disabled AND Locked = @Locked";

			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginName", loginName);
				myCommand.Parameters.AddWithValue("@Disabled", false);
				myCommand.Parameters.AddWithValue("@Locked", false);
				result = (int) myCommand.ExecuteScalar();
			}
			catch(SqlException)
			{}

			return (result > 0);
		}
		#endregion

		#region ExistLoginAlsoDisabled - True se l'Utente Applicativo identificato da LoginName esiste nella tabella (anche disabilitato)
		/// <summary>
		/// ExistLoginAlsoDisabled
		/// Cerca in MSD_Logins la login identificata da LoginName, la trova anche se disabilitata
		/// </summary>
		/// <param name="loginName">login name da ricercare</param>
		/// <returns>true se ha trovato la login specificata, false altrimenti</returns>
		//---------------------------------------------------------------------
		public bool ExistLoginAlsoDisabled(string loginName)
		{
			int result = 0;
			string myQuery = "SELECT COUNT(*) FROM MSD_Logins WHERE Login = @LoginName";
			
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginName", loginName);
				result = (int)myCommand.ExecuteScalar();
			}
			catch(SqlException)
			{}
			
			return (result > 0);
		}
		#endregion

		#region IsNTLogin - True se l'Utente Applicativo specificato da LoginId ha una autenticazione NT
		/// <summary>
		/// IsNTLogin
		/// Trova l'utente identificato dalla loginId, e ne ritorna la tipologia (login Windows oppure no)
		/// </summary>
		/// <param name="loginId">LoginId che identifica l'utente</param>
		/// <returns>isNT, true se è una login di tipo NT, false altrimenti</returns>
		//---------------------------------------------------------------------
		public bool IsNTLogin (string loginId)
		{
			SqlDataReader mylocalDataReader = null;
			bool isNT = false;
			string myQuery = "SELECT WindowsAuthentication FROM MSD_Logins WHERE LoginId = @LoginId";
			
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "IsNTLogin");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
			}

			while (mylocalDataReader.Read())
				isNT = bool.Parse(mylocalDataReader["WindowsAuthentication"].ToString());

			mylocalDataReader.Close();
			return isNT;
		}
		#endregion

		#region IsGuestUser - True se l'utente selezionato è l'utente Guest (accesso Web), false altrimenti
		/// <summary>
		/// IsGuestUser
		/// True se l'utente selezionato è l'utente Guest (accesso Web), false altrimenti
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsGuestUser(string loginId)
		{
			int result = 0;
			string myQuery = "SELECT COUNT(*) FROM MSD_Logins WHERE LoginId = @LoginId AND Login=@Login";
			
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));
				myCommand.Parameters.AddWithValue("@Login", NameSolverStrings.GuestLogin);
				result = (int) myCommand.ExecuteScalar();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "IsGuestUser");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
			}
			
			return (result > 0);
		}
		#endregion

		#region IsEasyLookSystemUser - True se l'utente selezionato è l'utente EasyLookSystem, false altrimenti
		/// <summary>
		/// IsEasyLookSystemUser
		/// True se l'utente selezionato è l'utente EasyLookSystem, false altrimenti
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsEasyLookSystemUser(string loginId)
		{
			int result = 0;
			string myQuery = "SELECT COUNT(*) FROM MSD_Logins WHERE LoginId = @LoginId AND Login=@Login";
			
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));
				myCommand.Parameters.AddWithValue("@Login", NameSolverStrings.EasyLookSystemLogin);
				result = (int) myCommand.ExecuteScalar();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "IsEasyLookSystemUser");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
			}
			
			return (result > 0);
		}
		#endregion

		#region GetPassword - Restituisce la password dell' Utente Applicativo specificato da LoginId
		/// <summary>
		/// GetPassword
		/// Data la loginId, ritorna la password dell'utente
		/// </summary>
		/// <param name="loginId">LoginId che identifica l'utente</param>
		/// <returns>password, stringa con la password trovata</returns>
		//---------------------------------------------------------------------
		public string GetPassword(string loginId)
		{
			SqlDataReader mylocalDataReader = null;
			string password = null;
			string myQuery = "SELECT Password FROM MSD_Logins WHERE LoginId = @LoginId";
			
			try
			{
				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));
				mylocalDataReader = myCommand.ExecuteReader();
			}
			catch(SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description,	sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure,	sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server,		sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number,		sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber,	sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function,     "GetPassword");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto,  "SysAdminPlugIn");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.ReadingUsers, extendedInfo);
			}

			while (mylocalDataReader.Read())
				password = Crypto.Decrypt(mylocalDataReader["Password"].ToString());

			mylocalDataReader.Close();
			return password;
		}
		#endregion

		#endregion
	}

	/// <summary>
	/// UserListItem
	/// Classe che gestisce le info relative a un utente per visualizzarlo in una listView
	/// </summary>
	//========================================================================
	public class UserListItem : ListViewItem
	{
		#region Variabili membro (private)
		private string companyId					= string.Empty;
		private string loginId						= string.Empty;
		private string login						= string.Empty;
		private string description					= string.Empty;
		private bool   isAdmin						= false;
		private bool   easyBuilderDeveloper			= false;
		private string dbUser						= string.Empty;
		private string dbPassword					= string.Empty;
		private bool   dbWindowsAuthentication		= false;
		private bool   loginWindowsAuthentication	= false; 
		private bool   disabled						= false;
		private bool   isChecked					= false;
		private bool   isModified					= false;
		#endregion

		#region Proprietà
		//---------------------------------------------------------------------
		public  bool  IsChecked					{ get { return  isChecked;				} set { isChecked			   = value; } }
		public  bool  IsModified				{ get { return  isModified;				} set { isModified			   = value; } }
        public string CompanyId					{ get { return  companyId;				} set { companyId			   = value; } }
		public string LoginId					{ get { return  loginId;				} set { loginId				   = value; } }
		public string Login						{ get { return  login;					} set { login				   = value; } }
		public string Description				{ get { return  description;			} set { description			   = value; } }
		public bool   IsAdmin					{ get { return isAdmin;					} set { isAdmin = value; } }
		public bool   EasyBuilderDeveloper		{ get { return easyBuilderDeveloper;	} set { easyBuilderDeveloper = value; } }
		public string DbUser					{ get { return dbUser;					} set { dbUser = value; } }
		public string DbPassword				{ get { return  dbPassword;             } set { dbPassword			   = value; } }
		public bool   DbWindowsAuthentication	{ get { return  dbWindowsAuthentication;} set { dbWindowsAuthentication = value; } }
		public bool   LoginWindowsAuthentication {get { return  loginWindowsAuthentication; } set { loginWindowsAuthentication = value; } }
		public bool   Disabled					{ get { return  disabled;				} set { disabled			   = value; } }
		#endregion

		//---------------------------------------------------------------------
		public UserListItem() 
		{}

		///<summary>
		/// Reimplementazione del metodo Clone
		///</summary>
		//---------------------------------------------------------------------
		public override object Clone()
		{
			UserListItem newUser = (UserListItem)base.Clone();

			newUser.isChecked = this.isChecked;
			newUser.isModified = this.isModified;				
			newUser.companyId = this.companyId;				
			newUser.loginId = this.loginId;				
			newUser.login = this.login;					
			newUser.description = this.description;
			newUser.isAdmin = this.isAdmin;
			newUser.easyBuilderDeveloper = this.easyBuilderDeveloper;
			newUser.dbUser = this.dbUser;					
			newUser.dbPassword = this.dbPassword;             
			newUser.dbWindowsAuthentication = this.dbWindowsAuthentication;
			newUser.loginWindowsAuthentication = this.loginWindowsAuthentication;
			newUser.disabled = this.disabled;				

			return newUser;
		}
	}
	
	/// <summary>
	/// UserItem
	/// Classe per il mantenimento delle informazioni dell'utente
	/// </summary>
	//=========================================================================
	public class UserItem
	{
		#region Variabili membro (private)
		private string	loginId						= string.Empty;
		private string	login						= string.Empty;
		private string	password					= string.Empty;
		private string	description					= string.Empty;
		private string	lastModifyGrants			= string.Empty;
		private string	expiredDatePassword			= string.Empty;
		private string	dbUser						= string.Empty;
		private string	dbPassword					= string.Empty;
		private string	eMailAddress				= string.Empty;
		private bool	disabled					= false;
		private bool	locked						= false;
		private int		nrLoginFailedCount			= 0;
		private bool	windowsAuthentication		= false;
		private bool	userMustChangePassword		= false;
		private bool	userCannotChangePassword	= false;
		private bool	passwordNeverExpired		= false;
		private bool	expiredDateCannotChange		= false;
		private string	preferredLanguage			= string.Empty;
		private string  applicationLanguage			= string.Empty;
		private bool	webAccess					= false;
		private bool	smartClientAccess			= false;
		private bool	concurrentAccess			= false;
		private bool	privateAreaAdmin			= false;
        //private int balloonBlockedType = 0;
		#endregion

		#region Proprietà
		//---------------------------------------------------------------------
		public string	LoginId						{ get { return loginId;					} set { loginId					 = value; } }
		public string	Login						{ get { return login;			        } set { login					 = value; } }
		public string	Password					{ get { return password;				} set { password				 = value; } }
		public string	Description					{ get { return description;				} set { description				 = value; } }
		public string	LastModifyGrants			{ get { return lastModifyGrants;		} set { lastModifyGrants		 = value; } }
		public string	ExpiredDatePassword			{ get { return expiredDatePassword;		} set { expiredDatePassword		 = value; } }
		public string	EMailAddress				{ get { return eMailAddress;			} set { eMailAddress			 = value; } }
		public bool		Disabled					{ get { return disabled;				} set { disabled				 = value; } }
		public bool		Locked						{ get { return locked;					} set { locked					 = value; } }
		public int		NrLoginFailedCount			{ get { return nrLoginFailedCount;		} set { nrLoginFailedCount		 = value; }}
		public bool		WindowsAuthentication		{ get { return windowsAuthentication;	} set { windowsAuthentication	 = value; } }
		public string	DbUser						{ get { return dbUser;					} set { dbUser					 = value; } }
		public string	DbPassword					{ get { return dbPassword;				} set { dbPassword				 = value; } }
		public bool		UserMustChangePassword		{ get { return userMustChangePassword;  } set { userMustChangePassword   = value; } }
		public bool		UserCannotChangePassword	{ get { return userCannotChangePassword;} set { userCannotChangePassword = value; } }
		public bool		PasswordNeverExpired		{ get { return passwordNeverExpired;    } set { passwordNeverExpired	 = value; }	}
		public bool		ExpiredDateCannotChange		{ get { return expiredDateCannotChange; } set { expiredDateCannotChange  = value; }	}
		public string	PreferredLanguage			{ get { return preferredLanguage;		} set { preferredLanguage        = value; } }
		public string	ApplicationLanguage			{ get { return applicationLanguage;	    } set { applicationLanguage      = value; } }
		public bool		WebAccess					{ get { return webAccess;				} set { webAccess				 = value; }	}
		public bool		SmartClientAccess			{ get { return smartClientAccess;		} set { smartClientAccess		 = value; }	}
		public bool		ConcurrentAccess			{ get { return concurrentAccess;		} set { concurrentAccess		 = value; } }
		public bool		PrivateAreaAdmin			{ get { return privateAreaAdmin;		} set { privateAreaAdmin		 = value; } }
        //public int BalloonBlockedType { get { return balloonBlockedType; } set { balloonBlockedType = value; } }
		#endregion

		//---------------------------------------------------------------------
		public UserItem() {}
	}

	/// <summary>
	/// CompanyUser
	/// Classe che gestisce le informazioni per un utente di una company
	/// Deriva da UserItem e aggiunge alcune informazioni
	/// </summary>
	//=========================================================================
	public class CompanyUser : UserItem
	{
		private string	dbDefaultUser			= string.Empty;
		private string	dbDefaultPassword		= string.Empty;
		private string	companyId				= string.Empty;
		private bool	dbWindowsAuthentication	= false;
		private bool	admin					= false;
		private bool	easyBuilderDeveloper	= false;
		
		//---------------------------------------------------------------------
		public string	CompanyId					{ get { return companyId;				} set { companyId			= value; } }
		public string	DBDefaultUser				{ get { return dbDefaultUser;			} set { dbDefaultUser		= value; } }
		public string	DBDefaultPassword			{ get { return dbDefaultPassword;		} set { dbDefaultPassword	= value; } }
		public bool		DBWindowsAuthentication		{ get { return dbWindowsAuthentication; } set { dbWindowsAuthentication = value; } }
		public bool		Admin						{ get { return admin;					} set { admin				= value; } }
		public bool		EasyBuilderDeveloper		{ get { return easyBuilderDeveloper;	} set { easyBuilderDeveloper= value; } }

		//---------------------------------------------------------------------
		public CompanyUser() {}
	}

	/// <summary>
	/// CompanyRoleLogin
	/// Gestisce le informazioni di un utente associato a una role di una company
	/// Deriva da companyUser
	/// </summary>
	//=========================================================================
	public class CompanyRoleLogin : CompanyUser
	{
		private string roleId = string.Empty;
		private string role   = string.Empty;

		//---------------------------------------------------------------------
		public string RoleId { get { return roleId; } set { roleId = value; } }
		public string Role	 { get { return role; } set { role = value; } }

		//---------------------------------------------------------------------
		public CompanyRoleLogin() {}
	}

	/// <summary>
	/// CompanyRoleLoginItem
	/// Utente associato a un ruolo di una company, mostrato in una listview
	/// </summary>
	//========================================================================
	public class CompanyRoleLoginItem : UserListItem
	{
		private string roleId = string.Empty;

		//---------------------------------------------------------------------
		public string RoleId { get { return roleId; } set { roleId = value; } }

		//---------------------------------------------------------------------
		public CompanyRoleLoginItem() {}
	}
}