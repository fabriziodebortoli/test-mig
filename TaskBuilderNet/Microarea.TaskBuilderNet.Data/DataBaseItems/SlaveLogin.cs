using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// SlaveLoginDb
	/// Classe che gestisce i record della tabella MSD_SlaveLogins
	/// </summary>
	//========================================================================
	public class SlaveLoginDb : DataBaseItem
	{
		//---------------------------------------------------------------------
		public SlaveLoginDb()
		{ }

		#region Add - associazione di un utente ad uno slave
		/// <summary>
		/// Add
		/// Inserisce un nuovo record nella tabella MSD_SlaveLogins, ovvero associa un utente applicativo
		/// anche agli eventuali database secondari
		/// </summary>
		//---------------------------------------------------------------------
		public bool Add(string slaveId, string loginId, string slaveDbUser, string slaveDbPassword, bool slaveDbWinAuth)
		{
			bool result = false;
			
			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;
			
			try
			{
				myCommand.CommandText =
				@"INSERT INTO MSD_SlaveLogins
				(SlaveId, LoginId, SlaveDBUser, SlaveDBPassword, SlaveDBWindowsAuthentication) 
				VALUES 
				(@SlaveId, @LoginId, @SlaveDBUser, @SlaveDBPassword, @SlaveDBWindowsAuthentication)";

				myCommand.Parameters.AddWithValue("@SlaveId", slaveId);
				myCommand.Parameters.AddWithValue("@LoginId", loginId);
				myCommand.Parameters.AddWithValue("@SlaveDBUser", slaveDbUser);
				myCommand.Parameters.AddWithValue("@SlaveDBPassword", slaveDbWinAuth ? string.Empty : Crypto.Encrypt(slaveDbPassword));
				myCommand.Parameters.AddWithValue("@SlaveDBWindowsAuthentication", slaveDbWinAuth);
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
				extendedInfo.Add(DatabaseLayerStrings.Function, "SlaveLoginDb.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.SlaveLoginInserting, slaveDbUser), extendedInfo);
			}
			
			myCommand.Dispose();
			return result;
		}

		/// <summary>
		/// Add
		/// Inserisce un nuovo record nella tabella MSD_SlaveLogins, ovvero associa un utente applicativo
		/// anche agli eventuali database secondari
		/// </summary>
		/// <param name="userOfCompany">Dati dell'utente da inserire</param>
		/// <param name="slaveId">id dello slave da aggiornare</param>
		//---------------------------------------------------------------------
		public bool Add(UserListItem userOfCompany, string slaveId)
		{
			bool result = false;

			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;

			try
			{
				myCommand.CommandText =
				@"INSERT INTO MSD_SlaveLogins
				(SlaveId, LoginId, SlaveDBUser, SlaveDBPassword, SlaveDBWindowsAuthentication) 
				VALUES 
				(@SlaveId, @LoginId, @SlaveDBUser, @SlaveDBPassword, @SlaveDBWindowsAuthentication)";

				myCommand.Parameters.AddWithValue("@SlaveId", slaveId);
				myCommand.Parameters.AddWithValue("@LoginId", userOfCompany.LoginId);
				myCommand.Parameters.AddWithValue("@SlaveDBUser", userOfCompany.DbUser);
				myCommand.Parameters.AddWithValue("@SlaveDBPassword", userOfCompany.DbWindowsAuthentication ? string.Empty : Crypto.Encrypt(userOfCompany.DbPassword));
				myCommand.Parameters.AddWithValue("@SlaveDBWindowsAuthentication", userOfCompany.DbWindowsAuthentication);
				myCommand.ExecuteNonQuery();

				result = true;
			}
			catch (SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, e.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SlaveLoginDb.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.SlaveLoginInserting, userOfCompany.DbUser), extendedInfo);
			}

			myCommand.Dispose();
			return result;
		}

		#endregion

		#region Modify - Modifica un utente associato ad uno slave
		/// <summary>
		/// Modify
		/// Modifica i dati di un utente assegnato ad uno slave
		/// </summary>
		//---------------------------------------------------------------------
		public bool Modify(string slaveId, string loginId, string slaveDbUser, string slaveDbPassword, bool slaveDbWinAuth)
		{
			bool result = true;

			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;

			try
			{
				string strQuery =
				@"UPDATE MSD_SlaveLogins
				SET SlaveDBUser = @SlaveDBUser, SlaveDBPassword = @SlaveDBPassword, SlaveDBWindowsAuthentication = @SlaveDBWindowsAuthentication
				WHERE LoginId = @LoginId AND SlaveId= @SlaveId";

				myCommand.CommandText = strQuery;
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));
				myCommand.Parameters.AddWithValue("@SlaveId", Int32.Parse(slaveId));
				myCommand.Parameters.AddWithValue("@SlaveDBUser", slaveDbUser);
				myCommand.Parameters.AddWithValue("@SlaveDBPassword", slaveDbWinAuth ? string.Empty : Crypto.Encrypt(slaveDbPassword));
				myCommand.Parameters.AddWithValue("@SlaveDBWindowsAuthentication", slaveDbWinAuth);
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
				extendedInfo.Add(DatabaseLayerStrings.Function, "SlaveLoginDb.Modify");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.SlaveLoginModify, slaveDbUser), extendedInfo);
			}
			return result;
		}

		/// <summary>
		/// Modify
		/// Modifica i dati di un utente assegnato a una azienda
		/// </summary>
		/// <param name="userOfCompany">dati dell'utente</param>
		//---------------------------------------------------------------------
		public bool Modify(UserListItem userOfCompany, string slaveId)
		{
			bool result = true;

			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;

			try
			{
				string strQuery =
				@"UPDATE MSD_SlaveLogins
				SET SlaveDBUser = @SlaveDBUser, SlaveDBPassword = @SlaveDBPassword, SlaveDBWindowsAuthentication = @SlaveDBWindowsAuthentication
				WHERE LoginId = @LoginId AND SlaveId= @SlaveId";

				myCommand.CommandText = strQuery;
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(userOfCompany.LoginId));
				myCommand.Parameters.AddWithValue("@SlaveId", Int32.Parse(slaveId));
				myCommand.Parameters.AddWithValue("@SlaveDBUser", userOfCompany.DbUser);
				myCommand.Parameters.AddWithValue("@SlaveDBPassword", userOfCompany.DbWindowsAuthentication ? string.Empty : Crypto.Encrypt(userOfCompany.DbPassword));
				myCommand.Parameters.AddWithValue("@SlaveDBWindowsAuthentication", userOfCompany.DbWindowsAuthentication);
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
				extendedInfo.Add(DatabaseLayerStrings.Function, "SlaveLoginDb.Modify");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.SlaveLoginModify, userOfCompany.DbUser), extendedInfo);
			}
			return result;
		}

		#endregion

		/// <summary>
		/// SelectDboOwnerForCompanyId
		/// Seleziona l'utente dbo di uno slave di un'azienda leggendo dalla tabella MSD_Companies, MSD_SlaveLogins, MSD_CompanyDBSlaves
		/// </summary>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <param name="loginItem">login dbowner</param>
		/// <returns>true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectDboOwnerForCompanyId(string companyId, out SlaveLoginItem loginItem)
		{
			bool result = false;
			loginItem = new SlaveLoginItem();

			if (string.IsNullOrWhiteSpace(companyId))
				return result;

			SqlDataReader myDataReader = null;

			try
			{
				string selectQuery = @"SELECT MSD_SlaveLogins.* FROM MSD_Companies 
						INNER JOIN MSD_CompanyDBSlaves ON MSD_Companies.CompanyId = @CompanyId AND
						MSD_Companies.CompanyId = MSD_CompanyDBSlaves.CompanyId 
						INNER JOIN MSD_SlaveLogins ON MSD_CompanyDBSlaves.SlaveId = MSD_SlaveLogins.SlaveId AND
						MSD_Companies.CompanyDBOwner = MSD_SlaveLogins.LoginId";

				SqlCommand myCommand = new SqlCommand(selectQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				myDataReader = myCommand.ExecuteReader();

				while (myDataReader.Read())
					loginItem = ReadLoginItem(myDataReader);

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
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.SelectDboOwnerForCompanyId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.SlaveLoginRead, extendedInfo);
				result = false;
			}
			finally
			{
				if (myDataReader != null && !myDataReader.IsClosed)
				{
					myDataReader.Close();
					myDataReader.Dispose();
				}
			}

			return result;
		}

		/// <summary>
		/// SelectAllForCompanyId
		/// Seleziona tutte le login di uno slave di un'azienda leggendo dalla tabella MSD_Companies, MSD_SlaveLogins, MSD_CompanyDBSlaves
		/// </summary>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <param name="loginItems">lista delle logins</param>
		/// <returns>true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllForCompanyId(string companyId, out List<SlaveLoginItem> loginItems)
		{
			bool result = false;
			loginItems = new List<SlaveLoginItem>();

			if (string.IsNullOrWhiteSpace(companyId))
				return result;

			SqlDataReader myDataReader = null;

			try
			{
				string selectQuery = @"SELECT MSD_SlaveLogins.* FROM MSD_Companies 
						INNER JOIN MSD_CompanyDBSlaves ON MSD_Companies.CompanyId = @CompanyId AND
						MSD_Companies.CompanyId = MSD_CompanyDBSlaves.CompanyId 
						INNER JOIN MSD_SlaveLogins ON MSD_CompanyDBSlaves.SlaveId = MSD_SlaveLogins.SlaveId";

				SqlCommand myCommand = new SqlCommand(selectQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				myDataReader = myCommand.ExecuteReader();

				while (myDataReader.Read())
					loginItems.Add(ReadLoginItem(myDataReader));

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
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.SelectAllForCompanyId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.SlaveLoginRead, extendedInfo);
				result = false;
			}
			finally
			{
				if (myDataReader != null && !myDataReader.IsClosed)
				{
					myDataReader.Close();
					myDataReader.Dispose();
				}
			}

			return result;
		}

		/// <summary>
		/// SelectAllForSlaveId
		/// Seleziona tutte le login di uno slave
		/// </summary>
		/// <param name="slaveId">Id che identifica lo slave</param>
		/// <param name="loginItems">lista delle logins</param>
		/// <returns>true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllForSlaveId(string slaveId, out List<SlaveLoginItem> loginItems)
		{
			bool result = false;
			loginItems = new List<SlaveLoginItem>();

			if (string.IsNullOrWhiteSpace(slaveId))
				return result;

			SqlDataReader myDataReader = null;

			try
			{
				string selectQuery = "SELECT * FROM MSD_SlaveLogins WHERE SlaveId = @SlaveId";

				SqlCommand myCommand = new SqlCommand(selectQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@SlaveId", Int32.Parse(slaveId));
				myDataReader = myCommand.ExecuteReader();

				while (myDataReader.Read())
					loginItems.Add(ReadLoginItem(myDataReader));

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
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.SelectAllForSlaveId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.SlaveLoginRead, extendedInfo);
				result = false;
			}
			finally
			{
				if (myDataReader != null && !myDataReader.IsClosed)
				{
					myDataReader.Close();
					myDataReader.Dispose();
				}
			}

			return result;
		}

		/// <summary>
		/// SelectAllForSlaveAndLogin
		/// Seleziona le info di una specifica login di uno slave di un'azienda
		/// leggendo dalla tabella MSD_Companies, MSD_SlaveLogins, MSD_CompanyDBSlaves
		/// </summary>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <param name="loginItems">lista delle logins</param>
		/// <returns>true se è andato tutto bene</returns>
		//---------------------------------------------------------------------
		public bool SelectAllForSlaveAndLogin(string slaveId, string loginId, out SlaveLoginItem loginItem)
		{
			bool result = false;
			loginItem = new SlaveLoginItem();

			if (string.IsNullOrWhiteSpace(slaveId) || string.IsNullOrWhiteSpace(loginId))
				return result;

			SqlDataReader myDataReader = null;

			try
			{
				string selectQuery = @"SELECT * FROM MSD_SlaveLogins
										WHERE SlaveId = @SlaveId AND LoginId = @LoginId";

				SqlCommand myCommand = new SqlCommand(selectQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@SlaveId", Int32.Parse(slaveId));
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));
				myDataReader = myCommand.ExecuteReader();

				while (myDataReader.Read())
					loginItem = ReadLoginItem(myDataReader);

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
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.SelectAllForSlaveAndLogin");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.SlaveLoginRead, extendedInfo);
				result = false;
			}
			finally
			{
				if (myDataReader != null && !myDataReader.IsClosed)
				{
					myDataReader.Close();
					myDataReader.Dispose();
				}
			}

			return result;
		}

		///<summary>
		/// Generica funzione che legge le informazioni dalle colonne della tabella MSD_SlaveLogins
		///</summary>
		//---------------------------------------------------------------------
		private SlaveLoginItem ReadLoginItem(SqlDataReader myDataReader)
		{
			SlaveLoginItem loginItem = new SlaveLoginItem();

			if (myDataReader == null)
				return loginItem;

			try
			{
				loginItem.SlaveId = myDataReader["SlaveId"].ToString();
				loginItem.LoginId = myDataReader["LoginId"].ToString();
				loginItem.SlaveDBUser = myDataReader["SlaveDBUser"].ToString();
				loginItem.SlaveDBPassword = Crypto.Decrypt(myDataReader["SlaveDBPassword"].ToString());
				loginItem.SlaveDBWinAuth = bool.Parse(myDataReader["SlaveDBWindowsAuthentication"].ToString());
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.ReadLoginItem");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.SlaveLoginRead, extendedInfo);
			}

			return loginItem;
		}

		/// <summary>
		/// GetSlaveDBConnectionString
		/// Prepara la stringa di connessione per connettersi al database slave con signature = DMS. 
		/// Per fare ciò deve leggere i dati di MSD_CompanyDBSlaves e MSD_SlaveLogins e costruirsi la stringa 
		/// di connessione utilizzando le info sul nome del database, tipologia di autenticazione e dbowner
		/// </summary>
		/// <param name="companyId">Id che identifica l'azienda</param>
		/// <returns>connection, stringa di connessione</returns>
		//---------------------------------------------------------------------
		internal string GetSlaveDBConnectionString(string companyId)
		{
			string slaveDbConnectionString = string.Empty;

			if (string.IsNullOrWhiteSpace(companyId))
				return slaveDbConnectionString;

			SqlDataReader myDataReader = null;

			try
			{
				string myQuery = @"SELECT MSD_CompanyDBSlaves.ServerName, MSD_CompanyDBSlaves.DatabaseName,
									  MSD_SlaveLogins.LoginId, MSD_SlaveLogins.SlaveDBWindowsAuthentication,
									  MSD_SlaveLogins.SlaveDBUser, MSD_SlaveLogins.SlaveDBPassword
									  FROM MSD_CompanyDBSlaves, MSD_SlaveLogins
									  WHERE MSD_CompanyDBSlaves.CompanyId = @companyId 
									  AND MSD_CompanyDBSlaves.Signature = @signature
									  AND MSD_CompanyDBSlaves.SlaveDBOwner = MSD_SlaveLogins.LoginId 
									  AND MSD_CompanyDBSlaves.SlaveId = MSD_SlaveLogins.SlaveId";

				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@companyId", Int32.Parse(companyId));
				myCommand.Parameters.AddWithValue("@signature", DatabaseLayerConsts.DMSSignature);

				myDataReader = myCommand.ExecuteReader();
				if (myDataReader == null)
					return slaveDbConnectionString;

				while (myDataReader.Read())
				{
					string slaveDbServer = myDataReader["ServerName"].ToString();
					string slaveDbName = myDataReader["DatabaseName"].ToString();

					//Windows Authentication
					if (bool.Parse(myDataReader["SlaveDBWindowsAuthentication"].ToString()))
						slaveDbConnectionString = string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, slaveDbServer, slaveDbName);
					else
					{
						//Sql Authentication
						string slaveDbUser = myDataReader["SlaveDBUser"].ToString();
						string slaveDbPassword = Crypto.Decrypt(myDataReader["SlaveDBPassword"].ToString());
						slaveDbConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, slaveDbServer, slaveDbName, slaveDbUser, slaveDbPassword);
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
				extendedInfo.Add(DatabaseLayerStrings.Function, "SlaveLoginDb.GetSlaveDBConnectionString");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.SlaveLoginRead, extendedInfo);
			}
			finally
			{
				if (myDataReader != null && !myDataReader.IsClosed)
				{
					myDataReader.Close();
					myDataReader.Dispose();
				}
			}

			return slaveDbConnectionString;
		}

		/// <summary>
		/// ExistLoginForSlaveId
		/// verifica se una login e' effettivamente associata allo slave 
		/// (e quindi esiste nella tabella MSD_SlaveLogins)
		/// </summary>
		/// <param name="loginId">Id che identifica l'utente</param>
		/// <param name="slaveId">Id che identifica lo slave</param>
		/// <returns>true: se l'utente esiste, altrimenti false</returns>
		//---------------------------------------------------------------------
		public bool ExistLoginForSlaveId(string loginId, string slaveId)
		{
			int result = 0;

			try
			{
				string myQuery = "SELECT COUNT(*) FROM MSD_SlaveLogins WHERE LoginId = @LoginId AND SlaveId = @SlaveId";

				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@SlaveId", Int32.Parse(slaveId));
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));

				result = (int)myCommand.ExecuteScalar();
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SlaveLoginDb.ExistLoginForSlaveId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.SlaveLoginRead, extendedInfo);
			}

			return result > 0;
		}

		/// <summary>
		/// ExistLoginForCompanyId
		/// verifica se una login e' effettivamente associata allo slave della company
		/// (e quindi esiste nella tabella MSD_SlaveLogins)
		/// </summary>
		/// <param name="loginId">Id che identifica l'utente</param>
		/// <param name="companyId">Id che identifica ll company</param>
		/// <returns>true: se l'utente esiste, altrimenti false</returns>
		//---------------------------------------------------------------------
		public bool ExistLoginForCompanyId(string loginId, string companyId)
		{
			int result = 0;

			try
			{
				string myQuery = @"SELECT COUNT(*)
									FROM MSD_Companies INNER JOIN
									MSD_CompanyDBSlaves ON MSD_Companies.CompanyId = MSD_CompanyDBSlaves.CompanyId INNER JOIN
									MSD_SlaveLogins ON MSD_CompanyDBSlaves.SlaveId = MSD_SlaveLogins.SlaveId
									WHERE (MSD_SlaveLogins.LoginId = @LoginId) AND (MSD_Companies.CompanyId = @CompanyId)";

				SqlCommand myCommand = new SqlCommand(myQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				myCommand.Parameters.AddWithValue("@LoginId", Int32.Parse(loginId));

				result = (int)myCommand.ExecuteScalar();
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SlaveLoginDb.ExistLoginForCompanyId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.SlaveLoginRead, extendedInfo);
			}

			return result > 0;
		}

		///<summary>
		/// Metodo che controlla se una login si puo' eliminare dall'associazione su SQL
		/// (ovvero se non e' usata da altri utenti associati alla medesima azienda)
		///</summary>
		//---------------------------------------------------------------------
		public bool CanDropLogin(string dbUser, string companyId)
		{
			bool canDrop = false;
			int numbersOfLogin = 0;

			List<SlaveLoginItem> loginItems = new List<SlaveLoginItem>();
			if (!SelectAllForCompanyId(companyId, out loginItems))
				return canDrop;

			foreach (SlaveLoginItem item in loginItems)
			{
				if (string.Compare(item.SlaveDBUser, dbUser, StringComparison.InvariantCultureIgnoreCase) == 0)
					numbersOfLogin += 1;

				if (numbersOfLogin > 1)
					break;
			}

			canDrop = (numbersOfLogin <= 1) ? true : false;
			return canDrop;
		}
	}

	/// <summary>
	/// SlaveLoginItem
	/// Classe che gestisce le informazioni per un utente di un database slave
	/// Deriva da UserItem e aggiunge alcune informazioni (il LoginId lo vede per ereditarietà)
	/// </summary>
	//=========================================================================
	public class SlaveLoginItem : UserItem
	{
		private string slaveDbUser = string.Empty;
		private string slaveDbPassword = string.Empty;
		private string slaveId = string.Empty;
		private bool slaveDbWinAuth = false;

		//---------------------------------------------------------------------
		public string SlaveId { get { return slaveId; } set { slaveId = value; } }
		public string SlaveDBUser { get { return slaveDbUser; } set { slaveDbUser = value; } }
		public string SlaveDBPassword { get { return slaveDbPassword; } set { slaveDbPassword = value; } }
		public bool SlaveDBWinAuth { get { return slaveDbWinAuth; } set { slaveDbWinAuth = value; } }

		//---------------------------------------------------------------------
		public SlaveLoginItem() { }
	}
}
