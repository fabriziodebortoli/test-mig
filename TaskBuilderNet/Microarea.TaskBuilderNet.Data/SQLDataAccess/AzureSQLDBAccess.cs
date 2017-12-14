using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.SQLDataAccess
{
	/// <summary>
	/// AzureSQLDBAccess
	/// Metodi per accesso / interrogazione SQL Database Azure
	/// </summary>
	// ========================================================================
	public class AzureSQLDBAccess
	{
		//---------------------------------------------------------------------
		private Diagnostic diagnostic = new Diagnostic("AzureSQLDBAccess");

		//---------------------------------------------------------------------
		public Diagnostic Diagnostic { get { return diagnostic; } }

		public string ConnectionString { get; set; }

		public TBConnection CurrentTBConnection { get; set; }

		//---------------------------------------------------------------------
		public delegate void SetProgressBarText(object sender, string message);
		public event SetProgressBarText OnSetProgressBarText;

		//---------------------------------------------------------------------
		public AzureSQLDBAccess()
		{
		}

		#region TryToConnect 
		/// <summary>
		/// Prova ad aprire una connessione e la chiude immediatamente
		/// </summary>
		//---------------------------------------------------------------------
		public bool TryToConnect()
		{
			bool result = false;

			if (string.IsNullOrWhiteSpace(ConnectionString))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return result;
			}

			try
			{
				using (TBConnection tbConnection = new TBConnection(ConnectionString, DBMSType.SQLSERVER))
				{
					tbConnection.Open();
					result = true;
				}
			}
			catch (TBException ex)
			{
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, ex.Message);
				ei.Add(DatabaseLayerStrings.Server, ex.Server);
				ei.Add(DatabaseLayerStrings.Number, ex.Number);
				ei.Add(DatabaseLayerStrings.Function, "TryToConnect");
				ei.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess.AzureSQLDBAccess");
				ei.Add(DatabaseLayerStrings.Source, ex.Source);
				ei.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, ex.Message, ei);
			}

			return result;
		}
		#endregion

		#region OpenConnection
		/// <summary>
		/// Apre la connessione impostata dalla stringa
		/// </summary>
		//---------------------------------------------------------------------
		public bool OpenConnection()
		{
			if (string.IsNullOrWhiteSpace(ConnectionString))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return false;
			}

			try
			{
				CurrentTBConnection = new TBConnection(ConnectionString, DBMSType.SQLSERVER);
				CurrentTBConnection.Open();
			}
			catch (TBException ex)
			{
				Debug.WriteLine(ex.Message);
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, ex.Message);
				ei.Add(DatabaseLayerStrings.Procedure, ex.Procedure);
				ei.Add(DatabaseLayerStrings.Server, ex.Server);
				ei.Add(DatabaseLayerStrings.Number, ex.Number);
				ei.Add(DatabaseLayerStrings.Function, "OpenConnection");
				ei.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess.AzureSQLDBAccess");
				ei.Add(DatabaseLayerStrings.Source, ex.Source);
				ei.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, ex.Message, ei);

				if (CurrentTBConnection != null)
				{
					CurrentTBConnection.Close();
					CurrentTBConnection.Dispose();
				}
				else
					CurrentTBConnection = null;
				return false;
			}

			return true;
		}
		#endregion

		//---------------------------------------------------------------------
		public void CloseConnection()
		{
			try
			{
				if (CurrentTBConnection != null && CurrentTBConnection.State != ConnectionState.Closed)
				{
					CurrentTBConnection.Close();
					CurrentTBConnection.Dispose();
				}
			}
			catch (TBException ex)
			{
				Debug.WriteLine(ex.Message);
				ExtendedInfo ei = new ExtendedInfo();
				ei.Add(DatabaseLayerStrings.Description, ex.Message);
				ei.Add(DatabaseLayerStrings.Procedure, ex.Procedure);
				ei.Add(DatabaseLayerStrings.Server, ex.Server);
				ei.Add(DatabaseLayerStrings.Number, ex.Number);
				ei.Add(DatabaseLayerStrings.Function, "CloseConnection");
				ei.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess.AzureSQLDBAccess");
				ei.Add(DatabaseLayerStrings.Source, ex.Source);
				ei.Add(DatabaseLayerStrings.StackTrace, ex.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, ex.Message, ei);
			}
		}

		#region ExistDataBase - Verifica se il db specificato esiste già
		/// <summary>
		/// Si connette al master e verifica se esiste il db specificato dal dbName
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistDataBase(string dbName)
		{
			if (string.IsNullOrWhiteSpace(ConnectionString))
			{
				Debug.Fail("AzureSQLDBAccess::ExistDataBase: the connection string is empty!");
				return false;
			}

			bool existDataBase = false;

			try
			{
				// non posso utilizzare il metodo ChangeDatabase
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
				builder.InitialCatalog = DatabaseLayerConsts.MasterDatabase;

				using (TBConnection myConnection = new TBConnection(builder.ConnectionString, DBMSType.SQLSERVER))
				{
					myConnection.Open();

					using (IDbCommand command = myConnection.CreateCommand())
					{
						command.CommandText = string.Format("SELECT COUNT(*) FROM sysdatabases WHERE name = '{0}'", dbName);
						if ((int)command.ExecuteScalar() > 0)
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
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess.AzureSQLDBAccess");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(SQLDataAccessStrings.ErrorDataBaseConnection, dbName), extendedInfo);
			}

			return existDataBase;
		}
		#endregion

		#region DeleteDatabase - Cancella un db su Azure
		/// <summary>
		/// DeleteDatabase // DA FARE
		/// </summary>
		//---------------------------------------------------------------------
		public bool DeleteDatabase
			(
				string serverName,
				string dataBaseName,
				string databaseOwnerLogin,
				string databaseOwnerPassword,
				bool integratedSecurity
			)
		{
			if (string.IsNullOrWhiteSpace(ConnectionString))
			{
				Debug.Fail("AzureSQLDBAccess::DeleteDatabase: the connection string is empty!");
				return false;
			}

			OnSetProgressBarText?.Invoke(this, SQLDataAccessStrings.DroppingDatabase);

			bool companyDbDeleted = false;
			SqlConnection sqlConnection = null;

			string connectionString =
				(integratedSecurity)
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, databaseOwnerLogin, databaseOwnerPassword);

			try
			{
				sqlConnection = new SqlConnection(connectionString);
				sqlConnection.Open();

				string countDb = string.Format("SELECT COUNT(*) FROM sysdatabases WHERE name = N'{0}'", dataBaseName);
				SqlCommand countCmd = new SqlCommand(countDb, sqlConnection);

				if ((int)countCmd.ExecuteScalar() == 1)
				{
					// prima altero il db per fare in modo di rimuovere le connessioni aperte
					string alterDB = string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", dataBaseName);

					SqlCommand dropDbCmd = new SqlCommand(alterDB, sqlConnection);
					dropDbCmd.ExecuteNonQuery();

					// poi eseguo la vera e propria cancellazione
					string dropDb = string.Format("DROP DATABASE [{0}]", dataBaseName);
					dropDbCmd.CommandText = dropDb;
					dropDbCmd.CommandTimeout = 60;
					dropDbCmd.ExecuteNonQuery();

					companyDbDeleted = true;
					dropDbCmd.Dispose();
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

				countCmd.Dispose();
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
			catch (System.ApplicationException aExc)
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

			if (sqlConnection != null)
			{
				sqlConnection.Close();
				sqlConnection.Dispose();
				sqlConnection = null;
			}

			return companyDbDeleted;
		}
		#endregion
	}
}