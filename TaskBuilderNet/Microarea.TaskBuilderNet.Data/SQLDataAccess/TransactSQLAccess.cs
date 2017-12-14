using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.SQLDataAccess
{
	/// <summary>
	/// TransactSQLAccess.
	/// Funzioni Transact-SQL
	/// </summary>
	// ========================================================================
	public class TransactSQLAccess
	{
		#region Private Variables
		private bool isAzureDB = false;

		private string currentStringConnection = string.Empty;
		private SqlConnection currentConnection = null;
		private string nameSpace = string.Empty;

		private Diagnostic diagnostic = new Diagnostic("TransactSQLAccess");
		#endregion

		#region Properties
		public string CurrentStringConnection { get { return currentStringConnection; } set { currentStringConnection = value; } }

		public string NameSpace { get { return nameSpace; } set { nameSpace = value; } }

		public SqlConnection CurrentConnection { get { return currentConnection; } set { currentConnection = value; } }
		public Diagnostic Diagnostic { get { return diagnostic; } }
		#endregion

		#region Delegates and Events
		public delegate bool IsUserAuthenticatedFromConsole(string login, string password, string server);
		public event IsUserAuthenticatedFromConsole OnIsUserAuthenticatedFromConsole;

		public delegate void AddUserAuthenticatedFromConsole(string login, string password, string server, DBMSType dbType);
		public event AddUserAuthenticatedFromConsole OnAddUserAuthenticatedFromConsole;

		public delegate string GetUserAuthenticatedPwdFromConsole(string login, string server);
		public event GetUserAuthenticatedPwdFromConsole OnGetUserAuthenticatedPwdFromConsole;

		public delegate void CallHelpFromPopUp(object sender, string nameSpace, string searchParameter);
		public event CallHelpFromPopUp OnCallHelpFromPopUp;

		//---------------------------------------------------------------------
		public delegate void EnableProgressBar(object sender);
		public event EnableProgressBar OnEnableProgressBar;

		public delegate void DisableProgressBar(object sender);
		public event DisableProgressBar OnDisableProgressBar;

		public delegate void SetProgressBarStep(object sender, int step);
		public event SetProgressBarStep OnSetProgressBarStep;

		public delegate void SetProgressBarValue(object sender, int currentValue);
		public event SetProgressBarValue OnSetProgressBarValue;

		public delegate void SetProgressBarText(object sender, string message);
		public event SetProgressBarText OnSetProgressBarText;

		public delegate void SetCyclicStepProgressBar();
		public event SetCyclicStepProgressBar OnSetCyclicStepProgressBar;
		#endregion

		/// <summary>
		/// Constructor (isAzureDB = false)
		/// </summary>
		//---------------------------------------------------------------------
		public TransactSQLAccess(bool isAzureDB = false)
		{
			this.isAzureDB = isAzureDB;
		}

		#region TryToConnect - Prova ad aprire una connessione
		/// <summary>
		/// TryToConnect
		/// </summary>
		//---------------------------------------------------------------------
		public bool TryToConnect()
		{
			bool success = false;

			try
			{
				//tento la connessione
				using (SqlConnection testConnection = new SqlConnection(CurrentStringConnection))
				{
					testConnection.Open();
					success = true;
				}
			}
			catch (SqlException ex)
			{
				Debug.WriteLine(ex.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, ex.Message);
				extendedInfo.Add(DatabaseLayerStrings.Server, ex.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, ex.Number);
				extendedInfo.Add(DatabaseLayerStrings.State, ex.State);
				extendedInfo.Add(DatabaseLayerStrings.Function, "TryToConnect");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, ex.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);

				// Nel caso in cui il LoginMode del server non sia impostato in MixedMode
				// Reason: Not associated with a trusted SQL Server connection.
				if (ex.Number == 18452) // [messaggio valido solo in sql2005 e sql2000]
					Diagnostic.Set(DiagnosticType.Error, ex.Message + "\r\n" + SQLDataAccessStrings.IntegratedSecurityModeOnly, extendedInfo);

				// da SQL2008 non c'e' piu' distinzione tra l'errore di trusted SQL Server connection o la semplice login failed.
				if (ex.Number == 18456)
					Diagnostic.Set(DiagnosticType.Error, ex.Message + "\r\n" + SQLDataAccessStrings.LoginFailed, extendedInfo);
				else
					Diagnostic.Set(DiagnosticType.Error, ex.Message + "\r\n" + SQLDataAccessStrings.ErrorConnection, extendedInfo);
			}
			
			return success;
		}
		#endregion

		#region Funzioni di Verifica esistenza di Database, Ruoli e Utenti

		#region ExistLogin -  Si connette al master e verifica se la loginName esiste
		/// <summary>
		/// ExistLogin
		/// Si connette al master e verifica se la login identificata da loginName esiste
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistLogin(string loginName)
		{
			bool existLogin = false;

			string query = isAzureDB ? "SELECT COUNT(*) FROM sys.sql_logins WHERE name = @User" : "SELECT COUNT(*) FROM syslogins WHERE name = @User";

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = DatabaseLayerConsts.MasterDatabase;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = new SqlCommand(query, myConnection))
					{
						myCommand.Parameters.AddWithValue("@User", loginName);

						if ((int)myCommand.ExecuteScalar() == 1)
							existLogin = true;
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "ExistLogin");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseConnection, DatabaseLayerConsts.MasterDatabase), extendedInfo);
			}

			return existLogin;
		}
		#endregion

		#region Metodi per ritornare i ruoli di una login
		/// <summary>
		/// LoginIsSystemAdminRole
		/// Ritorna true se la loginName ha il ruolo SysAdmin, identificato da roleName, false altrimenti
		/// </summary>
		//---------------------------------------------------------------------
		public bool LoginIsSystemAdminRole(string loginName, string roleName)
		{
			return SPhelpsrvrolemember(loginName, roleName);
		}

		//---------------------------------------------------------------------
		public bool LoginIsDBRoleDataReaderAndWriter(string loginName)
		{
			return
				SPHelpUserLITE(loginName, DatabaseLayerConsts.RoleDataReader) &&
				SPHelpUserLITE(loginName, DatabaseLayerConsts.RoleDataWriter);
		}

		//---------------------------------------------------------------------
		public bool LoginIsDBOwnerRole(string loginName)
		{
			return SPHelpUser(loginName, DatabaseLayerConsts.RoleDbOwner);
		}
		#endregion

		#region IsSysAdminDataBase - Verifica se il db di sistema contiene le tabelle obbligatorie
		/// <summary>
		/// Verifica se il db di SysAdmin esiste già
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsSysAdminDataBase(string dbName)
		{
			bool existDataBase = false;

			string query = @"SELECT COUNT(*) FROM sysobjects 
							WHERE name = 'MSD_Companies' OR name = 'MSD_Logins' OR name = 'MSD_Providers'";

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = dbName;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = new SqlCommand(query, myConnection))
						if ((int)myCommand.ExecuteScalar() == 3)
							existDataBase = true;
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "IsSysAdminDataBase");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorDataBaseSysAdminConnection);
			}

			return existDataBase;
		}
		#endregion

		#region ExistDataBase - Verifica se il db specificato esiste già (non di sistema)
		/// <summary>
		/// ExistDataBase
		/// Si connette al master e verifica se esiste il db specificato dal dbName
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistDataBase(string dbName)
		{
			bool existDataBase = false;
			string query = "SELECT COUNT(*) FROM sysdatabases WHERE name = @CompanyDbName";

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = DatabaseLayerConsts.MasterDatabase;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = new SqlCommand(query, myConnection))
					{
						myCommand.Parameters.AddWithValue("@CompanyDbName", dbName);
						if ((int)myCommand.ExecuteScalar() > 0)
							existDataBase = true;
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "ExistDataBase");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseConnection, dbName), extendedInfo);
			}

			return existDataBase;
		}
		#endregion

		#region ExistUserAndLoginIntoDb - True se la Login è associata al db attraverso uno specificato UserName
		/// <summary>
		/// ExistUserAndLoginIntoDb
		/// True se la Login è associata al db attraverso uno specificato UserName, false altrimenti
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistUserAndLoginIntoDb(string userName, string loginName, string dbName)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.ExistUserAndLoginIntoDb method unavailable for Azure database!!");
				return false;
			}

			bool found = false;
			
			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();

					//string currentDb = myConnection.Database;
					myConnection.ChangeDatabase(dbName);
					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandText = Consts.SPHelpUser;
						myCommand.CommandType = CommandType.StoredProcedure;

						using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
						{
							while (mySqlDataReader.Read())
							{
								if ((string.Compare(mySqlDataReader["UserName"].ToString(), userName, StringComparison.InvariantCultureIgnoreCase) == 0) &&
									(string.Compare(mySqlDataReader["LoginName"].ToString(), loginName, StringComparison.InvariantCultureIgnoreCase) == 0))
								{
									found = true;
									break;
								}
							}
						}
					}
					//myConnection.ChangeDatabase(currentDb);
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPHelpUser);
				extendedInfo.Add(DatabaseLayerStrings.Function, "ExistUserAndLoginIntoDb");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseConnection, dbName), extendedInfo);
				return false;
			}

			return found;
		}
		#endregion

		#region ExistUserIntoDb - True se l'utente UserName è associato al db dbName
		/// <summary>
		/// ExistUserIntoDb
		/// Testa l'esistenza di uno User (Login assegnata ad un db con un nome utente) a un db
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistUserIntoDb(string userName, string dbName)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.ExistUserIntoDb method unavailable for Azure database!!");
				return false;
			}

			bool found = false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();

					//string currentDb = myConnection.Database;
					myConnection.ChangeDatabase(dbName);
					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandText = Consts.SPHelpUser;
						myCommand.CommandType = CommandType.StoredProcedure;
						using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
							while (mySqlDataReader.Read())
							{
								if (string.Compare(mySqlDataReader["UserName"].ToString(), userName, StringComparison.InvariantCultureIgnoreCase) == 0)
								{
									found = true;
									break;
								}
							}
					}
					//myConnection.ChangeDatabase(currentDb);
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPHelpUser);
				extendedInfo.Add(DatabaseLayerStrings.Function, "ExistUserIntoDb");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseConnection, dbName), extendedInfo);
				return false;
			}

			return found;
		}
		#endregion

		#region ExistLoginIntoDb - True se la Login risulta assegnata a un database dbName
		/// <summary>
		/// ExistLoginIntoDb
		/// Testa l'esistenza di una Login assegnata a un db
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistLoginIntoDb(string loginName, string dbName)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.ExistLoginIntoDb method unavailable for Azure database!!");
				return false;
			}

			bool found = false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();

					//string currentDb = myConnection.Database;

					myConnection.ChangeDatabase(dbName);
					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandText = Consts.SPHelpUser;
						myCommand.CommandType = CommandType.StoredProcedure;
						using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
							while (mySqlDataReader.Read())
							{
								if (string.Compare(mySqlDataReader["LoginName"].ToString(), loginName, StringComparison.InvariantCultureIgnoreCase) == 0)
								{
									found = true;
									break;
								}
							}
					}

					//myConnection.ChangeDatabase(currentDb);
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPHelpUser);
				extendedInfo.Add(DatabaseLayerStrings.Function, "ExistLoginIntoDb");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseConnection, dbName), extendedInfo);
				return false;
			}

			return found;
		}
		#endregion

		#endregion

		#region Funzioni di Cancellazione (Database e oggetti di un Db)

		#region DeleteDatabaseObjects - Cancella tutti gli oggetti di un db (di una azienda)
		/// <summary>
		/// DeleteDatabaseObjects
		/// cancella tutte le tabelle, le view e le stored procedure in un database (esclusi gli oggetti di sistema)
		/// </summary>
		//---------------------------------------------------------------------
		public bool DeleteDatabaseObjects(string serverName, string dbName, string dbOwnerLogin, string dbOwnerPwd, bool isWinAuth)
		{
			OnEnableProgressBar?.Invoke(this);
			OnSetProgressBarStep?.Invoke(this, 3);
			OnSetProgressBarValue?.Invoke(this, 1);
			OnSetCyclicStepProgressBar?.Invoke();
			Cursor.Current = Cursors.WaitCursor;
			Application.DoEvents();

			bool companyDbObjectsDeleted = false;

			string connectionString = (isWinAuth)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, dbName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, dbName, dbOwnerLogin, dbOwnerPwd);

			SqlCommand myDeleteCommand = null;
			
			try
			{
				//tento la connessione
				using (SqlConnection deleteDBConnection = new SqlConnection(connectionString))
				{
					deleteDBConnection.Open();

					//oggetti da cancellare
					//-------------------------------------------------------------
					string myUserFKQuery = @"SELECT t1.name, t2.name as tableName FROM sysobjects t1 INNER JOIN sysobjects t2
										 ON t1.parent_obj = t2.id WHERE t1.type = 'F'";
					//-------------------------------------------------------------
					string myUserTableQuery = "SELECT name FROM sysobjects WHERE type = 'U'";
					//-------------------------------------------------------------
					string myUserSPQuery = "SELECT name FROM sysobjects WHERE type = 'P'";
					//-------------------------------------------------------------
					string myUserViewQuery = "SELECT name FROM sysobjects WHERE type = 'V'";
					//-------------------------------------------------------------
					string myUserTriggerQuery = "SELECT name FROM sysobjects WHERE type = 'TR'";

					List<string> myObjectList = new List<string>();

					myDeleteCommand = new SqlCommand();
					myDeleteCommand.Connection = deleteDBConnection;

					//-------------------------------------------------------------
					// CANCELLAZIONE TRIGGER
					//-------------------------------------------------------------
					myDeleteCommand.CommandText = myUserTriggerQuery;
					using (SqlDataReader mySqlDataReader = myDeleteCommand.ExecuteReader())
						while (mySqlDataReader.Read())
							myObjectList.Add(mySqlDataReader["name"].ToString());

					OnSetProgressBarText?.Invoke(this, SQLDataAccessStrings.DroppingTriggers);

					foreach (string trigger in myObjectList)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("DROP TRIGGER [{0}]", trigger);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
							Application.DoEvents();
						}
						catch (Exception e)
						{
							Debug.WriteLine(string.Format("An error occurring deleting {0} ({1})", trigger, e.Message));
							continue;
						}
					}

					//-------------------------------------------------------------
					// CANCELLAZIONE VIEW
					//-------------------------------------------------------------
					myDeleteCommand.CommandText = myUserViewQuery;
					myObjectList.Clear();
					using (SqlDataReader mySqlDataReader = myDeleteCommand.ExecuteReader())
						while (mySqlDataReader.Read())
						{
							if ((string.Compare(mySqlDataReader["name"].ToString(), "sysconstraints", true, CultureInfo.InvariantCulture) != 0) &&
								(string.Compare(mySqlDataReader["name"].ToString(), "syssegments", true, CultureInfo.InvariantCulture) != 0) &&
								(string.Compare(mySqlDataReader["name"].ToString(), "database_firewall_rules", true, CultureInfo.InvariantCulture) != 0)) // Azure
								myObjectList.Add(mySqlDataReader["name"].ToString());
						}

					OnSetProgressBarText?.Invoke(this, SQLDataAccessStrings.DroppingViews);

					foreach (string view in myObjectList)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("DROP VIEW [{0}]", view);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
							Application.DoEvents();
						}
						catch (Exception e)
						{
							Debug.WriteLine(string.Format("An error occurring deleting {0} ({1})", view, e.Message));
							continue;
						}
					}

					//-------------------------------------------------------------
					// CANCELLAZIONE STORED PROCEDURE
					//-------------------------------------------------------------
					myDeleteCommand.CommandText = myUserSPQuery;
					myObjectList.Clear();

					using (SqlDataReader mySqlDataReader = myDeleteCommand.ExecuteReader())
						while (mySqlDataReader.Read())
							myObjectList.Add(mySqlDataReader["name"].ToString());

					OnSetProgressBarText?.Invoke(this, SQLDataAccessStrings.DroppingProcedures);

					foreach (string procedure in myObjectList)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("DROP PROCEDURE [{0}]", procedure);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
							Application.DoEvents();
						}
						catch (Exception e)
						{
							Debug.WriteLine(string.Format("An error occurring deleting {0} ({1})", procedure, e.Message));
							continue;
						}
					}

					//-------------------------------------------------------------
					// CANCELLAZIONE FOREIGN KEY
					//-------------------------------------------------------------
					myDeleteCommand.CommandText = myUserFKQuery;
					myObjectList.Clear();
					List<string> userFKTables = new List<string>();

					using (SqlDataReader mySqlDataReader = myDeleteCommand.ExecuteReader())
						while (mySqlDataReader.Read())
						{
							myObjectList.Add(mySqlDataReader["name"].ToString());
							userFKTables.Add(mySqlDataReader["tableName"].ToString());
						}

					OnSetProgressBarText?.Invoke(this, SQLDataAccessStrings.DroppingConstraints);

					for (int i = 0; i < myObjectList.Count; i++)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("ALTER TABLE [{0}] DROP CONSTRAINT [{1}]", userFKTables[i], myObjectList[i]);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
							Application.DoEvents();
						}
						catch (Exception e)
						{
							Debug.WriteLine(string.Format("An error occurring deleting {0} ({1})", myObjectList[i], e.Message));
							continue;
						}
					}

					//-------------------------------------------------------------
					// CANCELLAZIONE TABELLE
					//-------------------------------------------------------------
					myDeleteCommand.CommandText = myUserTableQuery;
					myObjectList.Clear();

					using (SqlDataReader mySqlDataReader = myDeleteCommand.ExecuteReader())
						while (mySqlDataReader.Read())
							myObjectList.Add(mySqlDataReader["name"].ToString());

					OnSetProgressBarText?.Invoke(this, SQLDataAccessStrings.DroppingTables);

					foreach (string table in myObjectList)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("DROP TABLE [{0}]", table);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
							Application.DoEvents();
						}
						catch (Exception e)
						{
							Debug.WriteLine(string.Format("An error occurring deleting {0} ({1})", table, e.Message));
							continue;
						}
					}

					myDeleteCommand.Dispose();
					companyDbObjectsDeleted = true;
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDatabaseObjects");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseDeleting, dbName), extendedInfo);
			}
			catch (Exception aExc)
			{
				Debug.WriteLine(aExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, aExc.Message);
				extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDatabaseObjects");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, aExc.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, aExc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseDeleting, dbName), extendedInfo);
			}
			finally
			{
				if (myDeleteCommand != null)
					myDeleteCommand.Dispose();

				OnSetProgressBarText?.Invoke(this, string.Empty);
				OnDisableProgressBar?.Invoke(this);
			}
			return companyDbObjectsDeleted;
		}
		#endregion

		#region DeleteDataBase - Cancella un db
		/// <summary>
		/// DeleteDataBase
		/// </summary>
		//---------------------------------------------------------------------
		public bool DeleteDataBase
			(
				string serverName,
				string dataBaseName,
				string databaseOwnerLogin,
				string databaseOwnerPassword,
				bool integratedSecurity
			)
		{
			OnSetProgressBarText?.Invoke(this, SQLDataAccessStrings.DroppingDatabase);

			bool companyDbDeleted = false;

			string connectionString =
				(integratedSecurity)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, databaseOwnerLogin, databaseOwnerPassword);

			try
			{
				using (SqlConnection sqlConnection = new SqlConnection(connectionString))
				{
					sqlConnection.Open();

					string countDb = string.Format("SELECT COUNT(*) FROM sysdatabases WHERE name = N'{0}'", dataBaseName);

					using (SqlCommand countCmd = new SqlCommand(countDb, sqlConnection))
					{
						if ((int)countCmd.ExecuteScalar() == 1)
						{
							if (!isAzureDB)
							{
								// prima altero il db per fare in modo di rimuovere le connessioni aperte
								string alterDB = string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", dataBaseName);

								using (SqlCommand alterDbCmd = new SqlCommand(alterDB, sqlConnection))
									alterDbCmd.ExecuteNonQuery();
							}

							// poi eseguo la vera e propria cancellazione
							string dropDb = string.Format("DROP DATABASE [{0}]", dataBaseName);
							using (SqlCommand dropDbCmd = new SqlCommand(dropDb, sqlConnection))
							{
								dropDbCmd.CommandTimeout = 60;
								dropDbCmd.ExecuteNonQuery();
								companyDbDeleted = true;
							}
						}
						else
						{
							string message =
								string.Format(SQLDataAccessStrings.ErrorDataBaseDeleting, dataBaseName) + "\r\n"
								+ SQLDataAccessStrings.SeeDetailsExtendendInfo;

							ExtendedInfo extendedInfo = new ExtendedInfo();
							extendedInfo.Add(DatabaseLayerStrings.Description, SQLDataAccessStrings.DbNotExistsOnServer);
							extendedInfo.Add(DatabaseLayerStrings.Server, serverName);
							extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDataBase");
							extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
							Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
							companyDbDeleted = true;
						}
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDataBase");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseDeleting, dataBaseName), extendedInfo);
			}
			catch (ApplicationException aExc)
			{
				Debug.WriteLine(aExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, aExc.Message);
				extendedInfo.Add(DatabaseLayerStrings.Function, "DeleteDataBase");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, aExc.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, aExc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseDeleting, dataBaseName), extendedInfo);
			}

			return companyDbDeleted;
		}
		#endregion

		#endregion

		#region Funzioni per leggere le logins di un Server SQL

		#region IsNTGroup - True se la login rappresenta un Gruppo NT
		/// <summary>
		/// IsNTGroup
		/// IL dbowner del db aziendale connettendosi al master ha tutti i permessi 
		/// per eseguire questa extended stored procedure
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsNTGroup(string loginName)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.IsNTGroup method unavailable for Azure database!!");
				return false;
			}

			bool isNTGroup = false;

			try
			{
				using (SqlConnection myLoginInfoConnection = new SqlConnection(CurrentStringConnection))
				{
					myLoginInfoConnection.Open();

					//string currentDb = myLoginInfoConnection.Database;
					myLoginInfoConnection.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);
					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myLoginInfoConnection;
						myCommand.CommandText = Consts.XPLoginInfo;
						myCommand.CommandType = CommandType.StoredProcedure;
						myCommand.Parameters.AddWithValue("@acctname", loginName);
						using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
							while (mySqlDataReader.Read())
								isNTGroup = (string.Compare(mySqlDataReader["type"].ToString(), "group", true, CultureInfo.InvariantCulture) == 0);
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "IsNTGroup");
				extendedInfo.Add(DatabaseLayerStrings.Parameters, loginName);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseConnection, DatabaseLayerConsts.MasterDatabase), extendedInfo);
				return isNTGroup;
			}

			return isNTGroup;
		}
		#endregion

		#region GetLogins - legge le logins di sql dal server ove risiede il db aziendale
		/// <summary>
		/// GetLogins
		/// Si connnette al server dove risiede il dbcompany e legge le logins di sql
		/// Si setta le seguenti proprietà:
		/// la login rappresenta un utente NT
		/// la login rappresenta un utente NT locale (non di dominio)
		/// la login rappresenta un gruppo NT
		/// la login rappresenta l'utente sa
		/// </summary>
		//---------------------------------------------------------------------
		public bool GetLogins(out ArrayList loginsDatabase)
		{
			ArrayList loginsList = new ArrayList();

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = DatabaseLayerConsts.MasterDatabase;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandTimeout = 90;
						myCommand.CommandText = isAzureDB ? "SELECT name AS loginname FROM sys.sql_logins ORDER BY name" : "SELECT loginname FROM syslogins ORDER BY loginname";

						using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
						{
							while (mySqlDataReader.Read())
							{
								CompanyLogin companyLogin = new CompanyLogin();
								companyLogin.Login = mySqlDataReader["loginname"].ToString();

								if (string.Compare(companyLogin.Login, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
									companyLogin.IsSaUser = true;

								char[] separator = { '\\' };

								if (companyLogin.Login.IndexOfAny(separator) != -1)
								{
									companyLogin.IsNTGroup = IsNTGroup(companyLogin.Login);
									companyLogin.IsNTUser = !companyLogin.IsNTGroup;
									string[] elementOfLogin = companyLogin.Login.Split(separator);

									if (string.Compare(elementOfLogin[0], myConnection.DataSource, StringComparison.InvariantCultureIgnoreCase) == 0)
										companyLogin.IsLocalNTUser = true;
								}

								// nell'array delle Logins disponibili sul server SQL non considero una serie di utenti quali:
								// utenti speciali di SQL2005 (che iniziano con ##MS_)
								// l'utente NT di SQL2005 denominato NT AUTHORITY\SYSTEM
								// l'utente NT di SQL2005 che contiene la stringa ASPNET
								if (companyLogin.Login.StartsWith("##MS_"))
									continue;
								if (companyLogin.IsNTUser &&
									string.Compare(companyLogin.Login, "NT AUTHORITY\\SYSTEM", StringComparison.InvariantCultureIgnoreCase) == 0)
									continue;
								if (companyLogin.IsNTUser && companyLogin.Login.IndexOf("ASPNET") > 0)
									continue;

								// skippo la login SQL a mio uso e consumo
								if (string.Compare(companyLogin.Login, DatabaseLayerConsts.DmsOraUser, StringComparison.InvariantCultureIgnoreCase) == 0)
									continue;

								loginsList.Add(companyLogin);
							}
						}
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, isAzureDB ? "SELECT name AS loginname FROM sys.sql_logins" : "SELECT loginname FROM syslogins");
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Function, "GetLogins");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorLoadingSqlLogins, e.Server), extendedInfo);
				return false;
			}
			finally
			{
				loginsDatabase = loginsList;
			}
			
			return true;
		}
		#endregion

		#endregion

		#region Funzioni che richiamano StoredProcedures
		/*
		 * sp_grantlogin		Aggiunge una windows nt login
		 * sp_addlogin          Aggiunge una login di tipo sql
		 * sp_password          Cambia la password dell'utente
		 * sp_defaultdb         Cambia il default db per un utente
		 * sp_grantdbaccess		Aggiunge un security account (NT o SQL) nel corrente db,
		 *						e abilita le permission per eseguire operazioni sul db
		 * sp_changedbowner     Modifica il dbo del database corrente
		 * sp_revokedbaccess    Rimuove un account dal corrente dbsp
		 * sp_revokelogin       Rimuove da SQL l'account NT
		 * sp_droplogin			Rimuove da SQL un accont di tipo SQL
		 * sp_addsrvrolemember  Aggiunge una login come mebro di una fixed server role
		 * sp_helpdb            Utilizzata per sapere il dbowner di un db (funzione CurrentDbo)
		 * sp_helpuser          Utilizzata per sapere il defaultdb di una login (funzione UserDefaultDb)
		 */

		#region SPGrantLogin - Permette a una login di tipo NT (utente o gruppo) di connettersi a SQL
		/// <summary>
		/// SPGrantLogin
		/// Consente a un account utente o di gruppo di Microsoft Windows NT di connettersi a 
		/// Microsoft SQL Server tramite l'autenticazione di Windows.
		/// La login deve essere qualificata come dominio\utente oppure dominio\gruppo
		/// Solo i membri del sysadmin o securityadmin possono eseguire questa sp
		/// Per permettere l'accesso, però, è necessario prima eseguire sp_granddbaccess
		/// per creare la login in un determinato db
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPGrantLogin(string login)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.SPGrantLogin method unavailable for Azure database!!");
				return false;
			}

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();
					//string oldDb = myConnection.Database;
					myConnection.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);

					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandText = Consts.SPGrantLogin;
						myCommand.CommandType = CommandType.StoredProcedure;
						myCommand.Parameters.AddWithValue("@loginame", login);
						myCommand.ExecuteNonQuery();
					}
					//myConnection.ChangeDatabase(oldDb);
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPGrantLogin);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPGrantLogin");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPGrantLogin + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region SPChangePassword - Modifica la password di una login SQL
		//---------------------------------------------------------------------
		public bool SPChangePassword(string loginName, string oldPassword, string newPassword, string dataBaseName)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.SPChangePassword method unavailable for Azure database!!");
				return false;
			}

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			bool changePassword = false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();
					//string currentdb = myConnection.Database;
					myConnection.ChangeDatabase(dataBaseName);

					if (!TBCheckDatabase.IsOldestSqlServerVersion(myConnection))
					{
						string alterLogin = "ALTER LOGIN [{0}] WITH PASSWORD = '{1}' OLD_PASSWORD = '{2}'";
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = string.Format(alterLogin, loginName, newPassword, oldPassword);
							myCommand.ExecuteNonQuery();
						}
					}
					else
					{
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = Consts.SPPassword;
							myCommand.CommandType = CommandType.StoredProcedure;
							myCommand.Parameters.AddWithValue("@old", oldPassword);
							myCommand.Parameters.AddWithValue("@new", newPassword);
							myCommand.Parameters.AddWithValue("@loginame ", loginName);
							myCommand.ExecuteNonQuery();
						}
					}
					//myConnection.ChangeDatabase(currentdb);
					changePassword = true;
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPPassword);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, loginName);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPChangePassword");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPPassword + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}

			return changePassword;
		}
		#endregion

		#region SPAddLogin - Crea una nuova Login di tipo SQL (non NT)
		/// <summary>
		/// SPAddLogin
		/// Crea un nuovo account di accesso di Microsoft® SQL Server™ che consente a un 
		/// utente di connettersi a un computer SQL Server in base all'autenticazione di SQL Server.
		/// Se la password è specificata, viene criptata. dbDef è il default database per la login. 
		/// Non specifichiamo nè il default language (viene consideato il default impostato sulla 
		/// corrente connessione di SQL) nè sid (viene pertanto generato). 
		/// La login può essere lunga fino a 128 caratteri, non può essere nulla, o contenere '\';
		/// non può essere sa o public, che esistono già
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPAddLogin(string login, string password, string dbDef)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			if ((string.Compare(login, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0) ||
				(string.Compare(login, Consts.PublicUser, StringComparison.InvariantCultureIgnoreCase) == 0))
			{
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.DifferentCredential, login));
				return false;
			}

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = DatabaseLayerConsts.MasterDatabase;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;

						// se stiamo aggiungendo la login su un server diverso > SQL2005
						// utilizzo il comando CREATE LOGIN per ovviare ai problemi di policy
						if (!TBCheckDatabase.IsOldestSqlServerVersion(myConnection))
						{
							myCommand.CommandText = string.Format("CREATE LOGIN [{0}] WITH PASSWORD = '{1}'", login, password);
							if (!isAzureDB)
								myCommand.CommandText  += ", CHECK_POLICY = OFF"; // sintassi non supportata in Azure
						}
						else
						{
							myCommand.CommandText = Consts.SPAddLogin;
							myCommand.CommandType = CommandType.StoredProcedure;
							myCommand.Parameters.AddWithValue("@loginame", login);

							if (!string.IsNullOrEmpty(password))
								myCommand.Parameters.AddWithValue("@passwd", password);

							if (!string.IsNullOrEmpty(dbDef))
								myCommand.Parameters.AddWithValue("@defdb", dbDef);
						}

						myCommand.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPAddLogin);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPAddLogin");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);

				// se il numero dell'errore è 15025 significa che si sta aggiungendo una login che già esiste
				// perciò non devo dare un errore bloccante che invalida l'operazione, bensì procedere con le
				// successive elaborazioni
				if (e.Number == 15025)
				{
					Diagnostic.Set(DiagnosticType.Warning, string.Format(SQLDataAccessStrings.LoginAlreadyExists, login), extendedInfo);
					return true;
				}

				// se il numero dell'errore è 15247 significa che l'utente che ha aperto la connessione
				// non ha i privilegi per creare una una login. Pertanto blocco l'elaborazione.
				if (e.Number == 15247)
				{
					Diagnostic.Set
						(DiagnosticType.Error,
						string.Format(SQLDataAccessStrings.UserWithoutPermission, login) + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo,
						extendedInfo);
					return false;
				}
				// se il numero dell'errore è 15118 significa che si sta aggiungendo un account la cui password
				// non rispetta le politiche di sicurezza imposte da Windows. 
				// WinXP e Win2003 prevedono di default politiche restrittive sull'uso delle password ed il supporto
				// di SQLServer2005 utilizza le stesse dll. In caso di password per utenti non abbastanza "strong"
				// (ad es. anche la pwd vuota) viene ritornato questo specifico errore e si visualizza un apposito msg.
				if (e.Number == 15118)
				{
					Diagnostic.Set
						(DiagnosticType.Error,
						string.Format(SQLDataAccessStrings.PwdValidationFailed, login) + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo,
						extendedInfo);
					return false;
				}

				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPAddLogin + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region SPGrantDbAccess - Aggiunge un security account (utente o gruppo NT) al db corrente
		/// <summary>
		/// SPGrantDbAccess
		/// Aggiunge un account di protezione nel database corrente per un account di accesso di 
		/// Microsoft SQL Server o per un utente o gruppo di Microsoft Windows NT, quindi permette 
		/// all'account di ricevere autorizzazioni per l'esecuzione di attività all'interno del database.
		/// La login deve essere un utente e/o gruppo di NT; name_in_db è il nome dell'account nel db. 
		/// Se non è specificato, è login
		/// NOTA: al max, la login può avere lunghezza 128 caratteri, non può contenere '\', non 
		/// può essere nulla o una stringa vuota. Dopo avere eseguito questa stored procedure, è
		/// necessario grantare la login al db, prima di potersi connettere al db con queste 
		/// credenziali.
		/// La login guest può essere aggiunta a un db se e solo se non esiste già
		/// la login SA NON può essere aggiunta
		/// Solo i membri di sysadmin, i db_accessadmin e db_owner possono eseguire questa stored procedure
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPGrantDbAccess(string login, string loginDb, string dbName)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			if (string.Compare(login, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
				return false;

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = dbName;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();

					// se stiamo aggiungendo la login su un server > SQL2005
					// utilizzo il comando CREATE USER 
					if (!TBCheckDatabase.IsOldestSqlServerVersion(myConnection))
					{
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = string.Format("CREATE USER [{0}] FOR LOGIN [{1}]", loginDb, login);
							myCommand.ExecuteNonQuery();
						}
					}
					else
					{
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = Consts.SPGrantDbAccess;
							myCommand.CommandType = CommandType.StoredProcedure;
							myCommand.Parameters.AddWithValue("@loginame", login);
							myCommand.Parameters.AddWithValue("@name_in_db", loginDb);
							myCommand.ExecuteNonQuery();
						}
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPGrantDbAccess);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPGrantDbAccess");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);

				// se il numero dell'errore è 15023 significa che si sta aggiungendo un account che già esiste
				// perciò non devo dare un errore bloccante che invalida l'operazione, bensì procedere con le
				// successive elaborazioni
				// se l'utente che si sta cercando di aggiungere esiste già non è un problema
				if (e.Number == 15023)
				{
					Diagnostic.Set(DiagnosticType.Warning, string.Format(SQLDataAccessStrings.UserAlreadyExists, login), extendedInfo);
					return true;
				}

				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPGrantDbAccess + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region SPRevokeDBAccess - Rimuove un security account(utente o gruppo NT) dal db corrente
		/// <summary>
		/// SPRevokeDBAccess
		/// Rimuove un security account dal db corrente. L'account può essere un
		/// utente NT o SQL e deve esistere nel db corrente. Rimuovendo l'utente,
		/// vengono rimossi anche tutti i suoi alias e le sue permission
		/// </summary>
		//---------------------------------------------------------------------------
		public bool SPRevokeDbAccess(string login, string dbName)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			if (string.Compare(login, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
				return false;

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = dbName;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();

					// se stiamo aggiungendo la login su un server > SQL2005
					// utilizzo il comando DROP USER 
					if (!TBCheckDatabase.IsOldestSqlServerVersion(myConnection))
					{
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = string.Format("DROP USER [{0}]", login);
							myCommand.ExecuteNonQuery();
						}
					}
					else
					{
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = Consts.SPRevokeDbAccess;
							myCommand.CommandType = CommandType.StoredProcedure;
							myCommand.Parameters.AddWithValue("@name_in_db", login);
							myCommand.ExecuteNonQuery();
						}
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPRevokeDbAccess);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, dbName);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPRevokeDbAccess");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPRevokeDbAccess + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region SPChangedbowner - Cambia l'owner del database corrente
		/// <summary>
		/// SPChangedbowner
		/// Cambia l'owner del database corrente. Il database NON può essere master, model o tempdb. 
		/// La login può essere di tipo SQL oppure NT, e deve già esistere. Se l'utente ha già un 
		/// account sul db, prima è necessario eseguire un drop.
		/// Dopo aver eseguito questa sp, l'utente è visto come dbo nel db corrente.
		/// Solo i membri di sysadmin possono eseguire questa stored procedure.
		/// </summary>
		//---------------------------------------------------------------------
		private bool SPChangedbowner(string login, string dbName)
		{
			if ((string.Compare(dbName, DatabaseLayerConsts.MasterDatabase, StringComparison.InvariantCultureIgnoreCase) == 0) ||
				(string.Compare(dbName, Consts.ModelDatabase, StringComparison.InvariantCultureIgnoreCase) == 0) ||
				(string.Compare(dbName, Consts.TempDatabase, StringComparison.InvariantCultureIgnoreCase) == 0))
				return false;

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = dbName;

				using (SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString))
				{
					sqlConnection.Open();

					if (!TBCheckDatabase.IsOldestSqlServerVersion(sqlConnection))
					{
						string alterAuth = string.Format("ALTER AUTHORIZATION ON DATABASE::[{0}] TO [{1}]", dbName, login);
						using (SqlCommand myCommand = new SqlCommand(alterAuth, sqlConnection))
							myCommand.ExecuteNonQuery();
					}
					else
					{
						using (SqlCommand myCommand = new SqlCommand(Consts.SPChangeDbOwner, sqlConnection))
						{
							myCommand.CommandType = CommandType.StoredProcedure;
							myCommand.Parameters.AddWithValue("@loginame", login);
							myCommand.ExecuteNonQuery();
						}
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				//ERRORE 15110 significa che è già il dbo
				if (e.Number != 15110)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
					extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
					extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
					extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPChangeDbOwner);
					extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
					extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
					extendedInfo.Add(DatabaseLayerStrings.Function, "SPChangedbowner");
					extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
					extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
					extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
					Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPChangedbowner + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
					return false;
				}
			}
			
			return true;
		}
		#endregion

		#region SPDropUserFromMasterDb - Riumuove una login dal master db
		/// <summary>
		/// SPDropUserFromMasterDb
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPDropUserFromMasterDb(string userInDb)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.SPDropUserFromMasterDb method unavailable for Azure database!!");
				return false;
			}

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			if (string.Compare(userInDb, DatabaseLayerConsts.LoginSa, true, CultureInfo.InvariantCulture) == 0)
				return false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();
					string oldDb = myConnection.Database;
					myConnection.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);

					using (SqlCommand myCommand = new SqlCommand(Consts.SPDropUser, myConnection))
					{
						myCommand.CommandType = CommandType.StoredProcedure;
						myCommand.Parameters.AddWithValue("@name_in_db ", userInDb);
						myCommand.ExecuteNonQuery();
					}

					myConnection.ChangeDatabase(oldDb);
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPDropUser);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, userInDb);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPDropUser");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPDropUser + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region SPDropUser - Rimuove uno user da SQL
		/// <summary>
		/// SPDropUser
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPDropUser(string userInDb)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.SPDropUser method unavailable for Azure database!!");
				return false;
			}

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			if (string.Compare(userInDb, DatabaseLayerConsts.LoginSa, true, CultureInfo.InvariantCulture) == 0)
				return false;

			try
			{
				using (SqlConnection dropUserConnection = new SqlConnection(CurrentStringConnection))
				{
					dropUserConnection.Open();

					using (SqlCommand myCommand = new SqlCommand(Consts.SPDropUser, dropUserConnection))
					{
						myCommand.CommandType = CommandType.StoredProcedure;
						myCommand.Parameters.AddWithValue("@name_in_db ", userInDb);
						myCommand.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPDropUser);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, userInDb);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPDropUser");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPDropUser + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region SPDropLogin - Rimuove una login non NT da SQL
		/// <summary>
		/// SPDropLogin
		/// Rimuove una login non NT da SQL
		/// Non si possono rimuovere login mappate su un determinato database. E'
		/// necessario prima eseguie sp_revokedbaccess; inolte, non si può rimuovere sa
		/// La sp lavora cosi: valuta ogni db sul sever, e verifica che l'utente
		/// non sia legato a qualche database; se no, verifica che l'utente abbia 
		/// le permission per accedere al db e che il guest account esista nel db
		/// Se non posso accedere al db, l'utente non può essere cancellato
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPDropLogin(string login)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.SPDropLogin method unavailable for Azure database!!");
				return false;
			}

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			if (string.Compare(login, DatabaseLayerConsts.LoginSa, true, CultureInfo.InvariantCulture) == 0)
				return false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();
					using (SqlCommand myCommand = new SqlCommand(Consts.SPDropLogin, myConnection))
					{
						myCommand.CommandType = CommandType.StoredProcedure;
						myCommand.Parameters.AddWithValue("@loginame", login);
						myCommand.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPDropLogin);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPDropLogin");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPDropLogin + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region SPAddSrvRoleMember - Aggiunge una login come membro di un ruolo esistente in SQL
		/// <summary>
		/// SPAddSrvRoleMember
		/// Aggiunge una login come membro di un ruolo
		/// La login può essere di tipo NT o di SQL
		/// RoleName può essere una tra: sysadmin, securityadmin, serveradmin, setupadmin, 
		/// processadmin, diskadmin, dbcreator, bulkadmin
		/// La login sa non può essere aggiunta
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPAddSrvRoleMember(string loginName, string roleName, string dataBaseName)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.TaskBuilderNet.Data.SQLDataAccess.SPAddSrvRoleMember method unavailable for Azure database!!");
				return false;
			}

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			if (string.Compare(loginName, DatabaseLayerConsts.LoginSa, true, CultureInfo.InvariantCulture) == 0)
				return false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();
					//string oldDb = myConnection.Database;
					myConnection.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);

					if (TBCheckDatabase.IsSqlServerVersionStartingFrom2012(myConnection))
					{
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = string.Format("ALTER SERVER ROLE {0} ADD MEMBER [{1}]", roleName, loginName);
							myCommand.ExecuteNonQuery();
						}
					}
					else
					{
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = Consts.SPAddSrvRoleMember;
							myCommand.CommandType = CommandType.StoredProcedure;
							myCommand.Parameters.AddWithValue("@loginame", loginName);
							myCommand.Parameters.AddWithValue("@rolename ", roleName);
							myCommand.ExecuteNonQuery();
						}
					}
					//myConnection.ChangeDatabase(oldDb);
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPAddSrvRoleMember);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, loginName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, roleName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, dataBaseName);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPAddSrvRoleMember");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorSPAddSrvRoleMember, roleName, loginName, dataBaseName)	+ "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region SPAddRoleMember
		/// <summary>
		/// SPAddRoleMember
		/// Aggiunge o rimuove i membri a o da un ruolo del database o modifica il nome di un ruolo del database definito dall'utente.
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPAddRoleMember(string loginName, string roleName, string dataBaseName)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			if (string.Compare(loginName, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
				return false;

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = dataBaseName;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();
					if (TBCheckDatabase.IsSqlServerVersionStartingFrom2012(myConnection))
					{
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = string.Format("ALTER ROLE {0} ADD MEMBER [{1}]", roleName, loginName);
							myCommand.ExecuteNonQuery();
						}
					}
					else
					{
						using (SqlCommand myCommand = new SqlCommand())
						{
							myCommand.Connection = myConnection;
							myCommand.CommandText = Consts.SPAddRoleMember;
							myCommand.CommandType = CommandType.StoredProcedure;
							myCommand.Parameters.AddWithValue("@membername", loginName);
							myCommand.Parameters.AddWithValue("@rolename ", roleName);
							myCommand.ExecuteNonQuery();
						}
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.StoredProcedure, Consts.SPAddRoleMember);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, loginName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, roleName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, dataBaseName);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPAddRoleMember");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorSPAddRoleMember, roleName, loginName, dataBaseName) + "\r\n" + SQLDataAccessStrings.SeeDetailsExtendendInfo, extendedInfo);
				return false;
			}
			return true;
		}
		#endregion

		#region SPHelpUser e SPhelpsrvrolemember
		/// <summary>
		/// Restituisce informazioni sulle entità a livello di database nel database corrente.
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPHelpUser(string loginName, string roleName)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			//se è sa sono sicura che ha già la role quindi faccio tornare true
			if (string.Compare(loginName, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();

					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandText = Consts.SPHelpUser;
						myCommand.CommandType = CommandType.StoredProcedure;

						using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
							while (mySqlDataReader.Read())
								if (string.Compare(mySqlDataReader["LoginName"].ToString(), loginName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
								    string.Compare(mySqlDataReader["RoleName"].ToString(), roleName, StringComparison.InvariantCultureIgnoreCase) == 0)
									return true;
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, loginName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, roleName);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPHelpUser");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPHelpUser, extendedInfo);
				return false;
			}
		
			return false;
		}

		// esegue la sp_helpuser ma va a controllare la colonna UserName e non la LoginName
		// questo perche' se eseguita da un utente dbowner non ha la visibilita' dei dati di loginname
		//---------------------------------------------------------------------
		public bool SPHelpUserLITE(string userName, string roleName)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			//se è sa sono sicura che ha già la role quindi faccio tornare true
			if (string.Compare(userName, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();

					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandText = Consts.SPHelpUser;
						myCommand.CommandType = CommandType.StoredProcedure;

						using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
							while (mySqlDataReader.Read())
								if (string.Compare(mySqlDataReader["UserName"].ToString(), userName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
									string.Compare(mySqlDataReader["RoleName"].ToString(), roleName, StringComparison.InvariantCultureIgnoreCase) == 0)
									return true;
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, userName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, roleName);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPHelpUserLITE");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPHelpUser, extendedInfo);
				return false;
			}
			return false;
		}

		/// <summary>
		/// SPhelpsrvrolemember
		/// Restituisce informazioni sui membri di un ruolo predefinito del server SQL Server.
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPhelpsrvrolemember(string loginName, string roleName)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			//se è sa sono sicura che ha già la role quindi faccio tornare true
			if (string.Compare(loginName, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			bool hasRole = false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();
					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandText = Consts.SPHelpSrvRoleMember;
						myCommand.CommandType = CommandType.StoredProcedure;
						myCommand.Parameters.AddWithValue("@srvrolename", roleName);

						using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
							while (mySqlDataReader.Read())
								if (string.Compare(mySqlDataReader["MemberName"].ToString(), loginName, StringComparison.InvariantCultureIgnoreCase) == 0)
								{
									hasRole = true;
									break;
								}
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, loginName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, roleName);
				extendedInfo.Add(DatabaseLayerStrings.Function, "SPhelpsrvrolemember");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, SQLDataAccessStrings.ErrorSPHelpsrvrolemember, extendedInfo);
				return false;
			}

			return hasRole;
		}
		#endregion

		#endregion

		#region Funzioni sul Dbo

		#region CurrentDbo -  Ritorna true se il dbo del dbname selezionato è loginName
		/// <summary>
		/// CurrentDbo
		/// Ritorna true se il dbo del dbname selezionato è loginName, utilizzando la sp_helpdb
		/// </summary>
		//---------------------------------------------------------------------
		public bool CurrentDbo(string loginName, string dbName, out string dboOfDataBase)
		{
			dboOfDataBase = string.Empty;

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			bool isCurrentDbo = false;
			string owner = string.Empty;

			try
			{
				using (SqlConnection myConnectionForCurrentDbo = new SqlConnection(CurrentStringConnection))
				{
					myConnectionForCurrentDbo.Open();

					string currentDb = myConnectionForCurrentDbo.Database;
					myConnectionForCurrentDbo.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);
					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnectionForCurrentDbo;
						myCommand.CommandText = Consts.SPHelpDb;
						myCommand.CommandType = CommandType.StoredProcedure;
						myCommand.Parameters.AddWithValue("@dbname", dbName);
						using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
						{
							while (mySqlDataReader.Read())
								owner = mySqlDataReader["owner"].ToString();

							if (owner.Length > 0)
							{
								if (string.Compare(owner, loginName, true, CultureInfo.InvariantCulture) == 0)
									isCurrentDbo = true;
							}
						}
					}
					myConnectionForCurrentDbo.ChangeDatabase(currentDb);
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, loginName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, dbName);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CurrentDbo");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.AccessConnection, dbName, loginName), extendedInfo);
				dboOfDataBase = string.Empty;
				return false;
			}

			dboOfDataBase = owner;
			return isCurrentDbo;
		}
		#endregion

		#region ChangeDbo - Cambia il dbo del database dbName
		/// <summary>
		/// ChangeDbo
		/// </summary>
		//---------------------------------------------------------------------
		public bool ChangeDbo(string serverName, bool winAuth, string login, string password, string dbName)
		{
			bool success = false;

			currentStringConnection =
				(winAuth)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, login, password);

			try
			{
				//se l'utente non è grantato al db prima lo devo aggiungere
				if (!ExistLogin(login))
				{
					if (winAuth)
					{
						if (SPGrantLogin(login))
							if (!SPAddSrvRoleMember(login, DatabaseLayerConsts.RoleSysAdmin, dbName))
								return success;
					}
					else
					{
						if (SPAddLogin(login, password, dbName))
							if (!SPAddSrvRoleMember(login, DatabaseLayerConsts.RoleSysAdmin, dbName))
								return success;
					}
				}
				else
				{
					//devo cancellare la vecchia login, il change non va a buon fine se una login è già associata
					if (ExistUserIntoDb(login, dbName))
					{
						if (!SPRevokeDbAccess(login, dbName))
							return success;
					}
				}
				
				if (!SPChangedbowner(login, dbName))
					return success;

				success = true;
				Diagnostic.Set(DiagnosticType.Information, string.Format(SQLDataAccessStrings.MessageDboChanged, dbName));
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, serverName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, dbName);
				extendedInfo.Add(DatabaseLayerStrings.Parameters, login);
				extendedInfo.Add(DatabaseLayerStrings.Function, "ChangeDbo");
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.CannotChangeDbo, dbName), extendedInfo);
				return false;
			}

			return success;
		}
		#endregion

		#endregion

		#region Funzioni per Impersonificazione
		/// <summary>
		/// LoginImpersonification (silente)
		/// </summary>
		//---------------------------------------------------------------------
		public UserImpersonatedData LoginImpersonification
			(
			string login,
			string password,
			string domain,
			bool winAuth,
			bool silent
			)
		{
			string currentUser = string.Empty;
			if (winAuth)
			{
				string[] loginAndDomain = login.Split(Path.DirectorySeparatorChar);

				currentUser = (loginAndDomain.Length == 1)
					? domain + Path.DirectorySeparatorChar + login
					: login;
			}
			else
				currentUser = login;

			SQLCredential loginData = null;
			if ((string.Compare(currentUser, System.Security.Principal.WindowsIdentity.GetCurrent().Name, true, CultureInfo.InvariantCulture) != 0) && (winAuth))
			{
				loginData = new SQLCredential(login, password, domain, winAuth, silent);
				loginData.OnOpenHelpFromPopUp += new SQLCredential.OpenHelpFromPopUp(SendHelp);
				if (loginData.Success)
					return loginData.userImpersonated;
				else
					return null;
			}
			else
			{
				UserImpersonatedData impersonateUser = new UserImpersonatedData();
				impersonateUser.UserAfterImpersonate = System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();
				impersonateUser.Login = login;
				impersonateUser.Password = password;
				impersonateUser.Domain = domain;
				impersonateUser.WindowsAuthentication = winAuth;
				return impersonateUser;
			}
		}

		#region LoginImpersonification - Permetto all'utente di ri-loggarsi e di impersonificare un altro utente NT
		/// <summary>
		/// LoginImpersonification
		/// Permetto all'utente di ri-loggarsi e di impersonificare
		/// un altro utente NT
		/// </summary>
		//---------------------------------------------------------------------
		public UserImpersonatedData LoginImpersonification
			(
			string login,
			string password,
			string domain,
			bool winAuth,
			string serverName,
			string serverIstanceName,
			bool enableChangeCredential
			)
		{
			string serverToConnect =
				(serverIstanceName.Length == 0)
				? serverName
				: serverName + Path.DirectorySeparatorChar + serverIstanceName;

			string currentUser = string.Empty;

			//se l'utente correntemente loggato è diverso da quello che vuole diventare
			if (winAuth)
			{
				string[] loginAndDomain = login.Split(Path.DirectorySeparatorChar);
				currentUser = (loginAndDomain.Length == 1)
				? domain + Path.DirectorySeparatorChar + login
				: login;
			}
			else
				currentUser = login;

			SQLCredential loginData = null;
			if ((string.Compare(currentUser, System.Security.Principal.WindowsIdentity.GetCurrent().Name, true, CultureInfo.InvariantCulture) != 0) && (winAuth))
			{
				if (OnIsUserAuthenticatedFromConsole != null)
				{
					if (!OnIsUserAuthenticatedFromConsole(currentUser, password, serverToConnect))
					{
						loginData = new SQLCredential(serverToConnect, login, /*pw*/string.Empty, domain, winAuth, enableChangeCredential);
						loginData.OnOpenHelpFromPopUp += new SQLCredential.OpenHelpFromPopUp(SendHelp);
						loginData.ShowDialog();

						if (loginData.Success)
						{
							if (OnAddUserAuthenticatedFromConsole != null)
								OnAddUserAuthenticatedFromConsole
									(
									(winAuth)
									? loginData.userImpersonated.Domain + Path.DirectorySeparatorChar + loginData.userImpersonated.Login
									: loginData.userImpersonated.Login,
									loginData.userImpersonated.Password,
									serverToConnect,
									DBMSType.SQLSERVER
									);

							return loginData.userImpersonated;
						}
						else
							return null;
					}
					else
					{
						if (OnGetUserAuthenticatedPwdFromConsole != null)
						{
							string pwd = OnGetUserAuthenticatedPwdFromConsole(currentUser, serverToConnect);
							loginData = new SQLCredential(login, pwd, domain, winAuth);
							loginData.OnOpenHelpFromPopUp += new SQLCredential.OpenHelpFromPopUp(SendHelp);
						}
						else
						{
							loginData = new SQLCredential(serverToConnect, login,/*pw*/string.Empty, domain, winAuth, enableChangeCredential);
							loginData.OnOpenHelpFromPopUp += new SQLCredential.OpenHelpFromPopUp(SendHelp);
							loginData.ShowDialog();
						}
						if (loginData.Success)
							return loginData.userImpersonated;
						else
							return null;
					}
				}
				else
				{
					loginData = new SQLCredential(serverToConnect, login, /*pw*/string.Empty, domain, winAuth, enableChangeCredential);
					loginData.OnOpenHelpFromPopUp += new SQLCredential.OpenHelpFromPopUp(SendHelp);
					loginData.ShowDialog();
					if (loginData.Success)
						return loginData.userImpersonated;
					else
						return null;
				}
			}
			else
			{
				if (OnIsUserAuthenticatedFromConsole != null)
				{
					if (!OnIsUserAuthenticatedFromConsole(currentUser, password, serverToConnect))
					{
						loginData = new SQLCredential(serverToConnect, login, /*pw*/string.Empty, domain, winAuth, enableChangeCredential);
						loginData.OnOpenHelpFromPopUp += new SQLCredential.OpenHelpFromPopUp(SendHelp);
						loginData.ShowDialog();

						if (loginData.Success)
						{
							if (OnAddUserAuthenticatedFromConsole != null)
								OnAddUserAuthenticatedFromConsole
									(
									(winAuth)
									? loginData.userImpersonated.Domain + Path.DirectorySeparatorChar + loginData.userImpersonated.Login
									: loginData.userImpersonated.Login,
									loginData.userImpersonated.Password,
									serverToConnect,
									DBMSType.SQLSERVER
									);

							return loginData.userImpersonated;
						}
					}
					else
					{
						UserImpersonatedData impersonateUser = new UserImpersonatedData();
						impersonateUser.UserAfterImpersonate = System.Security.Principal.WindowsIdentity.GetCurrent().Impersonate();
						impersonateUser.Login = login;
						impersonateUser.Password = password;
						impersonateUser.Domain = domain;
						impersonateUser.WindowsAuthentication = winAuth;
						return impersonateUser;
					}
				}
				else
					return null;
			}
			return null;
		}
		#endregion

		//---------------------------------------------------------------------
		private void SendHelp(object sender, string searchParameter)
		{
			if (OnCallHelpFromPopUp != null)
				OnCallHelpFromPopUp(sender, NameSpace, searchParameter);
		}
		#endregion
	}

	//=========================================================================
	public class CompanyLogin
	{
		#region Variabili membro (privati)
		private string login = string.Empty;
		private string companyId = string.Empty;
		private bool isLocalNTUser = false;
		private bool isNTUser = false;
		private bool isSaUser = false;
		private bool isNTGroup = false;
		#endregion

		#region Proprietà
		//Properties
		//---------------------------------------------------------------------
		public string CompanyId { get { return companyId; } set { companyId = value; } }
		public string Login { get { return login; } set { login = value; } }
		public bool IsLocalNTUser { get { return isLocalNTUser; } set { isLocalNTUser = value; } }
		public bool IsNTUser { get { return isNTUser; } set { isNTUser = value; } }
		public bool IsSaUser { get { return isSaUser; } set { isSaUser = value; } }
		public bool IsNTGroup { get { return isNTGroup; } set { isNTGroup = value; } }
		#endregion

		#region Costruttore
		/// <summary>
		/// Costruttore (vuoto
		/// </summary>
		//---------------------------------------------------------------------
		public CompanyLogin() { }
		#endregion
	}
}
