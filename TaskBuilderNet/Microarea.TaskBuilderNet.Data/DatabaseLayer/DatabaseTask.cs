using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	#region Parametri creazione database SQL
	//========================================================================
	public class SQLCreateDBParameters
	{
		public string DatabaseName				= string.Empty;

		// parametri per il file di dati
		public string DataFileName				= string.Empty;
		public string DataPathFileName			= string.Empty;
		public bool DataFileGrowthByPercent		= true;
		public int DataFileGrowth				= 10;
		public bool DataUnrestrictedFileGrowth	= true;
		public int DataRestrictFileGrowthMB		= 2;
		public string DataFileMaxSize			= string.Empty;
		public int DataFileInitialSize			= 0;

		// parametri per il file di log
		public string LogFileName				= string.Empty;
		public string LogPathFileName			= string.Empty;
		public bool LogFileGrowthByPercent		= true;
		public int LogFileGrowth				= 10;
		public bool LogUnrestrictedFileGrowth	= true;
		public int LogRestrictFileGrowthMB		= 2;
		public string LogFileMaxSize			= string.Empty;
		public int LogFileInitialSize			= 0;

		// altri parametri
		public bool TruncateLogFile				= true;
		public bool AutoShrink					= false;
	}
	#endregion

	#region Parametri creazione database Azure
	//========================================================================
	public class AzureCreateDBParameters
	{
		public string DatabaseName		= string.Empty;
		public string Edition			= AzureEdition.Standard.ToString();
		public string ServiceObjective	= AzureServerLevelObjective.S2.ToString();
		public string MaxSize			= AzureMaxSize.GB250.ToString();
		public bool AutoShrink			= false;
	}
	#endregion

	#region Parametri backup database SQL
	//========================================================================
	public class SQLBackupDBParameters
	{
		public string DatabaseName	= string.Empty;
		public string BackupFilePath= string.Empty;
		public string Description	= string.Empty;
		public bool Overwrite		= false;
	}
	#endregion

	#region Parametri restore database SQL
	//========================================================================
	public class SQLRestoreDBParameters
	{
		public string DatabaseName		= string.Empty;
		public string RestoreFilePath	= string.Empty;
		public string DataLogicalName	= string.Empty;
		public string DataPhysicalName	= string.Empty;
		public string LogLogicalName	= string.Empty;
		public string LogPhysicalName	= string.Empty;
		public bool ForceRestore		= false;
	}
	#endregion

	//=========================================================================
	public class DatabaseTaskConsts
	{
		#region Constants for Transact-SQL syntax
		// comando per la creazione del database
		public const string CreateSQLDb =
			@"CREATE DATABASE [{0}] ON PRIMARY
			(NAME = N'{1}' , 
			FILENAME = N'{2}' , 
			FILEGROWTH = {3} {4} {5})
			LOG ON
			(NAME = N'{6}' , 
			FILENAME = N'{7}' , 
			FILEGROWTH = {8} {9} {10})
			COLLATE {11}";
		
		public const string CustomMaxSizeDb = ", MAXSIZE = {0}"; // se richiesto va al posto di {4} e/o {8}
		public const string CustomSizeDb	= ", SIZE = {0}"; // se richiesto va al posto di {5} e/o {10}

		// creazione database Azure SQL Database
		public const string CreateAzureDb = "CREATE DATABASE [{0}] COLLATE {1}	(EDITION = N'{2}', SERVICE_OBJECTIVE = N'{3}', MAXSIZE = {4})";

		// impostazione truncate log file
		public const string SetTruncateLogFileSimple = "ALTER DATABASE [{0}] SET RECOVERY SIMPLE";
		// impostazione AUTO_SHRINK
		public const string SetAutoShrinkON = "ALTER DATABASE [{0}] SET AUTO_SHRINK ON";
		// impostazione AUTO_CLOSE (nelle versioni Express Edition viene messo d'ufficio a true)
		public const string SetAutoCloseOFF = "ALTER DATABASE [{0}] SET AUTO_CLOSE OFF";

		public const string DataFileSuffix	= "_Data.MDF";
		public const string LogFileSuffix	= "_Log.LDF";

		// comando per il backup di un db
		public const string BackupDB		= "BACKUP DATABASE [{0}] TO DISK = '{1}' WITH DESCRIPTION = '{2}' {3}";
		public const string OverwriteBackup = ", INIT";

		// comandi per il restore di un db
		public const string RestoreDB		= "RESTORE DATABASE [{0}] FROM DISK = '{1}'";
		public const string WithMoveOption	= " WITH MOVE '{2}' TO '{3}', MOVE '{4}' TO '{5}'";
		public const string ReplaceOption	= "REPLACE";

		// per leggere i nomi dei file fisici e logici da un backup
		public const string RestoreFileListOnly = "RESTORE FILELISTONLY FROM DISK = '{0}'";
		// per effettuare la verifica del file di backup appena effettuato
		public const string RestoreVerifyOnly = "RESTORE VERIFYONLY FROM DISK = '{0}' WITH FILE = 1, NOUNLOAD";

		public const string LogicalName = "LogicalName";
		public const string PhysicalName = "PhysicalName";
		#endregion

		#region Constants Registry Key
		// comandi per leggere le chiavi di registro
		public const string XPRegRead = "xp_regread N'{0}', N'{1}', '{2}'"; // xp_regread @hive, @keyname, @valuename

		// chiavi da leggere per il path di default
		public const string KeyLocalMachine = "HKEY_LOCAL_MACHINE";
		public const string KeySQLDataRoot = "SQLDataRoot";
		public const string KeyDefaultData = "DefaultData";
		public const string KeyDefaultLog = "DefaultLog";

		public const string DataFolder = "Data";
		public const string DefaultInstance = "MSSQLServer";

		//---------------------------------------------------------------------
		public const string SqlInstancesNames = "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL";

		public const string PrimaryInstanceSQLDataRoot = "SOFTWARE\\Microsoft\\MSSQLServer\\Setup";
		public const string OtherInstanceSQLDataRoot = "SOFTWARE\\Microsoft\\Microsoft SQL Server\\{0}\\Setup";

		public const string PrimaryInstanceDefaultDataAndLog = "SOFTWARE\\Microsoft\\MSSQLServer\\MSSQLServer";
		public const string OtherInstanceDefaultDataAndLog = "SOFTWARE\\Microsoft\\Microsoft SQL Server\\{0}\\MSSQLServer";
		#endregion

		#region Postgre constants
		// comando per la creazione del database
		public const string CreateDbPostgre		= @"CREATE DATABASE {0};";
		public const string CreateSchemaPostgre = "DROP SCHEMA public; CREATE SCHEMA " + DatabaseLayerConsts.postgreDefaultSchema;
		#endregion
	}

	///<summary>
	/// Args utilizzato per gli eventi della classe DatabaseTask
	///</summary>
	//================================================================================
	public class DatabaseTaskEventArgs : EventArgs
	{
		public bool Result { get; set; }
		public Diagnostic DatabaseTaskDiagnostic { get; set; }
	}

	///<summary>
	/// DatabaseTask
	/// Classe che consente di accedere alle funzionalità di manipolazione dei database (di tipo SQL Server)
	/// senza l'utilizzo di componenti COM (quali SQLDMO)
	/// Operazioni disponibili:
	/// - creazione contenitore, 
	/// - backup, 
	/// - restore, 
	/// - verifica file di backup,
	/// - RESTORE FILELISTONLY per caricare i nomi dei file logici e fisici del backup,
	/// - lettura dal Registry dei path ove creare i file di dati e di log di SQL Server
	///</summary>
	//========================================================================
	public class DatabaseTask
	{
		#region Variables
		private Diagnostic diagnostic = new Diagnostic("DatabaseLayer.DatabaseTask");

		private string connectionString = string.Empty;

		// evento per passare il risultato e il diagnostico all'esterno, in caso di esecuzione su thread separato
		public event EventHandler<DatabaseTaskEventArgs> OperationCompleted;
		#endregion

		#region Properties
		//--------------------------------------------------------------------------------
		public Diagnostic Diagnostic { get { return diagnostic; } }
		//--------------------------------------------------------------------------------
		public string CurrentStringConnection { get { return connectionString; } set { connectionString = value; } }

		// elenco stringhe di errore visualizzate nello Scheduler
		//---------------------------------------------------------------------
		public List<string> ErrorsList
		{
			get
			{
				List<string> errors = new List<string>();
				IDiagnosticItems items = diagnostic.AllMessages();
				if (items != null)
				{
					foreach (IDiagnosticItem item in items)
					{
						if (item.Type != DiagnosticType.Error)
							continue;
						errors.Add(item.FullExplain);
					}
				}

				return errors;
			}
		}
		#endregion

		#region Constructor
		//---------------------------------------------------------------------
		public DatabaseTask()
		{
		}
		#endregion

		#region Create database SQL Server
		/// <summary>
		/// Impostando solo il nome del database, questo viene creato con i parametri di default
		/// </summary>
		//---------------------------------------------------------------------
		// Esempio sintassi Transact-SQL:
		// CREATE DATABASE [MioDb] ON PRIMARY 
		// (NAME = N'MioDb_Data.MDF', 
		//	FILENAME = N'C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL\DATA\MioDb_Data.MDF' , 
		//  FILEGROWTH = 10%, 
		//  MAXSIZE = 100,
		//  SIZE = 12)
		// LOG ON 
		// (NAME = N'MioDb_Log.LDF', 
		//  FILENAME = N'C:\PROGRAM FILES\MICROSOFT SQL SERVER\MSSQL\DATA\MioDb_Log.LDF' , 
		//  FILEGROWTH = 10%,
		//  MAXSIZE = 100,
		//  SIZE = 12) 
		// COLLATE Latin1_General_CI_AS
		//---------------------------------------------------------------------
		public bool CreateSQLDatabase(SQLCreateDBParameters createParameters)
		{
			bool success = false;

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return success;
			}

			// compongo il nome dei file di dati e di log aggiungendo i suffissi mdf e ldf
			string dataFileName = string.Concat(createParameters.DatabaseName, DatabaseTaskConsts.DataFileSuffix);
			string logFileName = string.Concat(createParameters.DatabaseName, DatabaseTaskConsts.LogFileSuffix);

			try
			{
				using (TBConnection myConnection = new TBConnection(CurrentStringConnection, DBMSType.SQLSERVER))
				{
					myConnection.Open();

					// se i path dei file di dati e/o di log sono vuoti vado a calcolarli
					if (string.IsNullOrWhiteSpace(createParameters.DataPathFileName) || string.IsNullOrWhiteSpace(createParameters.LogPathFileName))
					{
						string dataFileFolderPath = string.Empty;
						string logFileFolderPath = string.Empty;

						GetSQLDataRootPath(myConnection, out dataFileFolderPath, out logFileFolderPath);

						if (string.IsNullOrWhiteSpace(createParameters.DataPathFileName))
							createParameters.DataPathFileName = dataFileFolderPath;
						if (string.IsNullOrWhiteSpace(createParameters.LogPathFileName))
							createParameters.LogPathFileName = logFileFolderPath;
					}

					TBCommand createDbCommand = new TBCommand(myConnection);
					createDbCommand.CommandTimeout = 500;

					createDbCommand.CommandText = string.Format
						(
						DatabaseTaskConsts.CreateSQLDb,     // sintassi base della CREATE DATABASE
						createParameters.DatabaseName,      // nome del database
						dataFileName,                       // nome del file di dati (.mdf)
						Path.Combine(createParameters.DataPathFileName, dataFileName), // path completo del file di dati
						createParameters.DataFileGrowthByPercent // modalita' di crescita del file di dati 
						? string.Concat(createParameters.DataFileGrowth, "%") // (FILEGROWTH in %)
						: createParameters.DataFileGrowth.ToString(), // (FILEGROWTH in MB)
						createParameters.DataUnrestrictedFileGrowth // massima dimensione del file di dati
						? string.Empty // illimitata
						: string.Format(DatabaseTaskConsts.CustomMaxSizeDb, createParameters.DataRestrictFileGrowthMB.ToString()), // imposto la MAXSIZE per il file di dati (in MB)
						(createParameters.DataFileInitialSize > 0 ? string.Format(DatabaseTaskConsts.CustomSizeDb, createParameters.DataFileInitialSize.ToString()) : string.Empty), // imposto la SIZE iniziale per il file di dati (in MB)
						logFileName,    // nome del file di log (.ldf)
						Path.Combine(createParameters.LogPathFileName, logFileName),   // path completo del file di log
						createParameters.LogFileGrowthByPercent // modalita' di crescita del file di log
						? string.Concat(createParameters.LogFileGrowth, "%") // (FILEGROWTH in %)
						: createParameters.LogFileGrowth.ToString(), // (FILEGROWTH in MB)
						createParameters.LogUnrestrictedFileGrowth // massima dimensione del file di log
						? string.Empty // illimitata
						: string.Format(DatabaseTaskConsts.CustomMaxSizeDb, createParameters.LogRestrictFileGrowthMB.ToString()), // imposto la MAXSIZE per il file di log (in MB)
						(createParameters.LogFileInitialSize > 0 ? string.Format(DatabaseTaskConsts.CustomSizeDb, createParameters.LogFileInitialSize.ToString()) : string.Empty), // imposto la SIZE iniziale per il file di log (in MB)
						NameSolverDatabaseStrings.SQLLatinCollation // imposto la COLLATION di database con Latin1...
						);

					createDbCommand.ExecuteNonQuery();
					createDbCommand.Dispose();

					// se si è scelta l'opzione "Truncate file log at checkpoints" devo impostare il RecoveryModel = SIMPLE
					// il default di SQL è FULL. uso la sintassi di ALTER DATABASE perchè la sp_dboption é deprecata
					if (createParameters.TruncateLogFile)
					{
						createDbCommand.CommandText = string.Format(DatabaseTaskConsts.SetTruncateLogFileSimple, createParameters.DatabaseName);
						createDbCommand.ExecuteNonQuery();
						createDbCommand.Dispose();
					}

					// se si è scelta l'opzione "Auto shrink" devo impostare il parametro AUTO_SHRINK a ON
					// il default di SQL è OFF. uso la sintassi di ALTER DATABASE perchè la sp_dboption é deprecata
					if (createParameters.AutoShrink)
					{
						createDbCommand.CommandText = string.Format(DatabaseTaskConsts.SetAutoShrinkON, createParameters.DatabaseName);
						createDbCommand.ExecuteNonQuery();
						createDbCommand.Dispose();
					}

					// imposto d'ufficio il parametro AUTO_CLOSE a OFF (il default per la Express Edition e' ON)
					createDbCommand.CommandText = string.Format(DatabaseTaskConsts.SetAutoCloseOFF, createParameters.DatabaseName);
					createDbCommand.ExecuteNonQuery();
					createDbCommand.Dispose();

					success = true;
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Function, "DatabaseTask.Create");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseLayerStrings.ErrorDuringDBCreation, createParameters.DatabaseName), extendedInfo);
				success = false;
			}

			DatabaseTaskEventArgs args = new DatabaseTaskEventArgs();
			args.Result = success;
			args.DatabaseTaskDiagnostic = diagnostic;
			OperationCompleted?.Invoke(this, args);

			return success;
		}
		#endregion

		#region Creazione database Azure
		/// <summary>
		/// Create database template for Azure SQL Database and Azure SQL Data Warehouse Database
		/// </summary>
		//---------------------------------------------------------------------
		// This script will only run in the context of the master database. To manage this database in 
		// SQL Server Management Studio, either connect to the created database, or connect to master.
		// SQL Database is available in the following service tiers: Basic, Standard, Premium , DataWarehouse, Web(Retired) and Business(Retired).
		// Standard is the go-to option for getting started with cloud-designed business applications and offers mid-level performance 
		// and business continuity features.Performance objectives for Standard deliver predictable per minute transaction rates.
		// See http://go.microsoft.com/fwlink/p/?LinkId=402063 for more information about CREATE DATABASE for Azure SQL Database.
		// 
		// CREATE DATABASE <Database_Name, sysname, Database_Name> COLLATE <collation_Name, sysname, SQL_Latin1_General_CP1_CI_AS> 
		// (
		//	EDITION = '<EDITION, , Standard>',
		//	SERVICE_OBJECTIVE='<SERVICE_OBJECTIVE,,S0>',
		//	MAXSIZE = <MAX_SIZE,,1024 GB>
		// )
		//---------------------------------------------------------------------
		public bool CreateAzureDatabase(AzureCreateDBParameters azureParams)
		{
			bool result = false;

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return result;
			}

			try
			{
				using (TBConnection myConnection = new TBConnection(CurrentStringConnection, DBMSType.SQLSERVER))
				{
					myConnection.Open();

					TBCommand createDbCommand = new TBCommand(myConnection);
					createDbCommand.CommandTimeout = 500;

					createDbCommand.CommandText = string.Format
						(
						DatabaseTaskConsts.CreateAzureDb,           // sintassi base della CREATE DATABASE per Azure
						azureParams.DatabaseName,                   // nome del database
						NameSolverDatabaseStrings.SQLLatinCollation,// imposto la COLLATION di database con Latin1...
						azureParams.Edition,                        // edition (Basic, Standard, Premium)
						azureParams.ServiceObjective,               // server level objective
						azureParams.MaxSize                         // max size
						);

					createDbCommand.ExecuteNonQuery();
					createDbCommand.Dispose();

					// se si è scelta l'opzione "Auto shrink" devo impostare il parametro AUTO_SHRINK a ON
					// il default di SQL è OFF. uso la sintassi di ALTER DATABASE perchè la sp_dboption é deprecata
					if (azureParams.AutoShrink)
					{
						createDbCommand.CommandText = string.Format(DatabaseTaskConsts.SetAutoShrinkON, azureParams.DatabaseName);
						createDbCommand.ExecuteNonQuery();
						createDbCommand.Dispose();
					}

					result = true;
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Function, "DatabaseTask.CreateAzureDatabase");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseLayerStrings.ErrorDuringDBCreation, azureParams.DatabaseName), extendedInfo);
				result = false;
			}

			DatabaseTaskEventArgs args = new DatabaseTaskEventArgs();
			args.Result = result;
			args.DatabaseTaskDiagnostic = diagnostic;
			OperationCompleted?.Invoke(this, args);

			return result;
		}
		#endregion

		#region Creazione database Postgre
		/// <summary>
		///@@Anastasia Create database per Postgre
		///@@Anastasia il metodo semplice, non gestisce i log file etc..
		/// </summary>
		/// <param name="dbName"></param>
		//---------------------------------------------------------------------
		public bool CreatePostgre(string dbName)
		{
			bool success = false;

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return success;
			}

			try
			{
				using (TBConnection myConnection = new TBConnection(CurrentStringConnection, DBMSType.POSTGRE))
				{
					myConnection.Open();
					using (TBCommand createDbCommand = new TBCommand(myConnection))
					{
						createDbCommand.CommandTimeout = 500;
						createDbCommand.CommandText = string.Format(DatabaseTaskConsts.CreateDbPostgre, dbName);
						createDbCommand.ExecuteNonQuery();
						myConnection.ChangeDatabase(dbName.ToLower());
					}

					using (TBCommand changeSchemaCommand = new TBCommand(myConnection))
					{
						changeSchemaCommand.CommandText = DatabaseTaskConsts.CreateSchemaPostgre;
						changeSchemaCommand.ExecuteNonQuery();
					}

					success = true;
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Function, "DatabaseTask.CreatePostgre");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseLayerStrings.ErrorDuringDBCreation, dbName), extendedInfo);
				success = false;
			}

			return success;
		}
		#endregion

		#region Backup
		/// <summary>
		/// Backup completo database su file (il file viene sempre sovrascritto)
		/// </summary>
		/// <param name="dbName">nome db</param>
		/// <param name="filePath">path file backup, comprensivo del nome file ed estensione</param>
		/// <param name="overwrite">overwrite del file di backup</param>
		//---------------------------------------------------------------------
		// Esempio sintassi Transact-SQL:
		// BACKUP DATABASE AdventureWorks
		// TO DISK = 'E:\TestBackups\Prova bkp\TestBkpAdv.bak'
		// WITH DESCRIPTION = 'Full backup of database AdventureWorks', INIT
		//---------------------------------------------------------------------
		public bool Backup(SQLBackupDBParameters bakParams)
		{
			bool result = false;

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return result;
			}

			try
			{
				using (TBConnection myConnection = new TBConnection(CurrentStringConnection, DBMSType.SQLSERVER))
				{
					myConnection.Open();
					using (TBCommand myCommand = new TBCommand(myConnection))
					{
						myCommand.CommandTimeout = 500;
						myCommand.CommandText = string.Format
							(
							DatabaseTaskConsts.BackupDB,
							bakParams.DatabaseName,
							bakParams.BackupFilePath,
							string.Format(DatabaseLayerStrings.CompleteBackup, bakParams.DatabaseName),
							(bakParams.Overwrite) ? DatabaseTaskConsts.OverwriteBackup : string.Empty
							);

						myCommand.ExecuteNonQuery();
						result = true;
					}
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Function, "DatabaseTask.Backup");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorDatabaseBackup, extendedInfo);
			}

			DatabaseTaskEventArgs args = new DatabaseTaskEventArgs();
			args.Result = result;
			args.DatabaseTaskDiagnostic = diagnostic;
			OperationCompleted?.Invoke(this, args);

			return result;
		}
		#endregion

		#region RestoreWithMove
		/// <summary>
		/// Restore di un database su file con il parametro WITH MOVE
		/// specificando i nomi fisici e logici del file di dati e del file di log
		/// </summary>
		//---------------------------------------------------------------------
		// Esempio sintassi Transact-SQL:
		// RESTORE DATABASE Az1_M
		// FROM DISK = 'E:\Databases\TestBackups\ERP30.bak'
		// WITH 
		// MOVE 'Az1_M_Data' TO 'E:\Databases\Az1_M_Data.mdf', 
		// MOVE 'Az1_M_Log' TO 'E:\Databases\Az1_M_Log.ldf',
		// REPLACE
		//---------------------------------------------------------------------
		public bool RestoreWithMove(SQLRestoreDBParameters restoreParams)
		{
			bool result = false;

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return result;
			}

			string restoreQuery = DatabaseTaskConsts.RestoreDB + DatabaseTaskConsts.WithMoveOption;
			if (restoreParams.ForceRestore)
				restoreQuery += string.Concat(", ", DatabaseTaskConsts.ReplaceOption);

			try
			{
				using (TBConnection myConnection = new TBConnection(CurrentStringConnection, DBMSType.SQLSERVER))
				{
					myConnection.Open();
					// se il database su cui è stata aperta la connessione é diverso da master effettuo la ChangeDatabase
					myConnection.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);

					using (TBCommand myCommand = new TBCommand(myConnection))
					{
						myCommand.CommandTimeout = 500;
						myCommand.CommandText = string.Format
							(
							restoreQuery,
							restoreParams.DatabaseName,
							restoreParams.RestoreFilePath,
							restoreParams.DataLogicalName,
							restoreParams.DataPhysicalName,
							restoreParams.LogLogicalName,
							restoreParams.LogPhysicalName
							);

						myCommand.ExecuteNonQuery();
						result = true;
					}
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Function, "DatabaseTask.RestoreWithMove");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseLayerStrings.ErrorDatabaseRestore, restoreParams.DatabaseName), extendedInfo);
			}

			return result;
		}
		#endregion

		#region Restore
		/// <summary>
		/// Restore di un database su file con replace
		/// Utilizzato dallo Scheduler per effettuare un restore senza la WITH MOVE
		/// </summary>
		//---------------------------------------------------------------------
		// Esempio sintassi Transact-SQL:
		// RESTORE DATABASE Az1_M
		// FROM DISK = 'E:\Databases\TestBackups\ERP30.bak'
		// WITH REPLACE
		//---------------------------------------------------------------------
		public bool Restore(SQLRestoreDBParameters restoreParams)
		{
			bool success = false;
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return success;
			}

			string restoreQuery = string.Concat(DatabaseTaskConsts.RestoreDB, "WITH ", DatabaseTaskConsts.ReplaceOption);

			try
			{
				using (TBConnection myConnection = new TBConnection(CurrentStringConnection, DBMSType.SQLSERVER))
				{
					myConnection.Open();
					myConnection.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);

					using (TBCommand myCommand = new TBCommand(myConnection))
					{
						myCommand.CommandTimeout = 500;
						myCommand.CommandText = string.Format(restoreQuery, restoreParams.DatabaseName, restoreParams.RestoreFilePath);
						myCommand.ExecuteNonQuery();
						myCommand.Dispose();
						success = true;
					}
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Function, "DatabaseTask.Restore");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseLayerStrings.ErrorDatabaseRestore, restoreParams.DatabaseName), extendedInfo);
				diagnostic.Set(DiagnosticType.Error, e.Message);
			}

			return success;
		}
		#endregion

		#region LoadFileListOnly - carica i nomi dei file logici e fisici da un file di backup
		/// <summary>
		/// Esegue il comando RESTORE FILELISTONLY per caricare i nomi dei file logici e fisici del backup
		/// <param name="backupFile">path del file di backup da considerare</param>
		/// </summary>
		//---------------------------------------------------------------------
		public DataTable LoadFileListOnly(string backupFile)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection) || string.IsNullOrWhiteSpace(backupFile))
			{
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return null;
			}
			
			DataTable filesDataTable = new DataTable();
			filesDataTable.Columns.Add(new DataColumn(DatabaseTaskConsts.LogicalName, Type.GetType("System.String")));
			filesDataTable.Columns.Add(new DataColumn(DatabaseTaskConsts.PhysicalName, Type.GetType("System.String")));

			try
			{
				using (TBConnection myConnection = new TBConnection(CurrentStringConnection, DBMSType.SQLSERVER))
				{
					myConnection.Open();
					using (TBCommand myCommand = new TBCommand(myConnection))
					{
						myCommand.CommandText = string.Format(DatabaseTaskConsts.RestoreFileListOnly, backupFile);
						using (IDataReader myReader = myCommand.ExecuteReader())
						{

							while (myReader.Read())
							{
								DataRow myRow = filesDataTable.NewRow();
								myRow[DatabaseTaskConsts.LogicalName] = myReader[DatabaseTaskConsts.LogicalName].ToString();
								myRow[DatabaseTaskConsts.PhysicalName] = myReader[DatabaseTaskConsts.PhysicalName].ToString();
								filesDataTable.Rows.Add(myRow);
							}
							filesDataTable.AcceptChanges();
						}
					}
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Function, "DatabaseTask.LoadFileListOnly");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				if (e.Number == 3169)
					diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.BackupFileVersionIsRecent, extendedInfo);
				else
					diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorLoadingFileListOnly, extendedInfo);
				return null;
			}

			return filesDataTable;
		}
		#endregion

		#region VerifyBackupFile
		/// <summary>
		/// Esecuzione dell'istruzione RESTORE VERIFYONLY sulla device di backup per effettuarne 
		/// la verifica senza eseguirne il ripristino.
		/// Controlla che il set di backup sia completo e che tutti i volumi siano leggibili. 
		/// Non verifica tuttavia la struttura dei dati nei volumi di backup.
		/// <param name="physicalDeviceName">path del file di backup da considerare</param>
		/// </summary>
		//---------------------------------------------------------------------
		public bool VerifyBackupFile(string physicalDeviceName)
		{
			bool result = false;

			if (string.IsNullOrWhiteSpace(CurrentStringConnection) || string.IsNullOrWhiteSpace(physicalDeviceName))
			{
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrConnectStringEmpty);
				return result;
			}

			try
			{
				using (TBConnection myConnection = new TBConnection(CurrentStringConnection, DBMSType.SQLSERVER))
				{
					myConnection.Open();
					using (TBCommand myCommand = new TBCommand(myConnection))
					{
						myCommand.CommandText = string.Format(DatabaseTaskConsts.RestoreVerifyOnly, physicalDeviceName);
						myCommand.ExecuteNonQuery();
						diagnostic.SetInformation(DatabaseLayerStrings.BackupSetIsValid);
					}
					result = true;
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseLayerStrings.Function, "DatabaseTask.VerifyBackupFile");
				extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, DatabaseLayerStrings.ErrorDuringVerifyDatabase, extendedInfo);
			}

			return result;
		}
		#endregion

		#region Lettura dal registry del path di salvataggio dei file mdf e ldf
		///<summary>
		/// Lettura dal registry del path di salvataggio dei file mdf e ldf
		/// - se le chiavi DefaultData e DefaultLog sono impostate considero queste
		/// - altrimenti considero il parametro di default di SQL nella chiave SQLDataRoot.
		/// Eseguo il seg. comando Transact-SQL: xp_regread @hive, @keyname, @valuename, @param OUT
		/// (per es: xp_regread N'HKEY_LOCAL_MACHINE', N'SOFTWARE\Microsoft\MSSQLServer\Setup', 'SQLDataRoot')
		/// con una connessione istanziata al volo al database master.
		/// <param name="serverName">nome server</param>
		/// <param name="instanceName">nome istanza</param>
		/// <param name="loginName">login</param>
		/// <param name="loginPwd">password</param>
		/// <param name="isWinAuth">se è windows auth</param>
		/// <param name="defaultFolderDataPath">path di default per il file di dati</param>
		/// <param name="defaultFolderLogPath">path di default per il file di log</param>
		///</summary>
		//---------------------------------------------------------------------
		public void GetSQLDataRootPath
		(
			string serverName,
			string instanceName,
			string loginName,
			string loginPwd,
			bool isWinAuth,
			out string defaultFolderDataPath,
			out string defaultFolderLogPath
		)
		{
			defaultFolderDataPath = defaultFolderLogPath = string.Empty;

			if (!string.IsNullOrEmpty(instanceName))
				serverName = Path.Combine(serverName, instanceName);

			// apro una connessione al database master
			string connectionString = (isWinAuth)
			? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, DatabaseLayerConsts.MasterDatabase)
			: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, DatabaseLayerConsts.MasterDatabase, loginName, loginPwd);

			try
			{
				using (TBConnection myConnection = new TBConnection(connectionString, DBMSType.SQLSERVER))
				{
					myConnection.Open();
					GetSQLDataRootPath(myConnection, out defaultFolderDataPath, out defaultFolderLogPath);
				}
			}
			catch
			{ }
		}

		///<summary>
		/// Lettura dal registry del path di salvataggio dei file mdf e ldf
		/// - se le chiavi DefaultData e DefaultLog sono impostate considero queste
		/// - altrimenti considero il parametro di default di SQL nella chiave SQLDataRoot.
		/// Eseguo il seg. comando Transact-SQL: xp_regread @hive, @keyname, @valuename, @param OUT
		/// (per es: xp_regread N'HKEY_LOCAL_MACHINE', N'SOFTWARE\Microsoft\MSSQLServer\Setup', 'SQLDataRoot')
		/// con una connessione istanziata al volo al database master.
		/// <param name="connection">connessione aperta</param>
		/// <param name="defaultFolderDataPath">path di default per il file di dati</param>
		/// <param name="defaultFolderLogPath">path di default per il file di log</param>
		///</summary>
		//---------------------------------------------------------------------
		private void GetSQLDataRootPath(TBConnection connection, out string defaultFolderDataPath, out string defaultFolderLogPath)
		{
			defaultFolderDataPath = defaultFolderLogPath = string.Empty;

			if (connection == null)
				return;

			TBCommand myCommand = null;
			IDataReader myReader = null;

			string valueInstanceName = string.Empty;
			string valueDataPath = string.Empty;

			try
			{
				bool isPrimaryInstance = true;

				string serverName = string.Empty;
				string instanceName = string.Empty;

				// estrapolo il nome dell'istanza dal nome del server
				int pos = connection.DataSource.IndexOf(Path.DirectorySeparatorChar);
				if (pos > 0)
				{
					serverName = connection.DataSource.Substring(0, pos);
					instanceName = connection.DataSource.Substring(pos + 1);
					isPrimaryInstance = false;
				}
				else
				{
					serverName = connection.DataSource;
					instanceName = string.Empty;
				}

				if (connection.State == ConnectionState.Closed)
					connection.Open();

				myCommand = new TBCommand(connection);

				try
				{
					// leggo il valore nel registry associato alla mia istanza
					myCommand.CommandText =
						string.Format
						(
						DatabaseTaskConsts.XPRegRead,
						DatabaseTaskConsts.KeyLocalMachine,
						DatabaseTaskConsts.SqlInstancesNames,
						isPrimaryInstance ? DatabaseTaskConsts.DefaultInstance : instanceName
						);

					myReader = myCommand.ExecuteReader();

					while (myReader.Read())
						valueInstanceName = myReader[1].ToString();
				}
				catch (Exception ex)
				{
					Debug.WriteLine("DatabaseTask:GetSQLDataRootPath: " + ex.Message);
				}
				finally
				{
					if (myReader != null && !myReader.IsClosed)
					{
						myReader.Close();
						myReader.Dispose();
					}
				}

				// leggo il valore della chiave SQLDataRoot
				myCommand.CommandText =
					(isPrimaryInstance && string.IsNullOrEmpty(valueInstanceName))
					? string.Format
						(
						DatabaseTaskConsts.XPRegRead,
						DatabaseTaskConsts.KeyLocalMachine,
						DatabaseTaskConsts.PrimaryInstanceSQLDataRoot,
						DatabaseTaskConsts.KeySQLDataRoot
						)
					: string.Format
						(
						DatabaseTaskConsts.XPRegRead,
						DatabaseTaskConsts.KeyLocalMachine,
						string.Format(DatabaseTaskConsts.OtherInstanceSQLDataRoot, string.IsNullOrEmpty(valueInstanceName) ? instanceName : valueInstanceName),
						DatabaseTaskConsts.KeySQLDataRoot
						);

				myReader = myCommand.ExecuteReader();

				while (myReader.Read())
					valueDataPath = myReader[1].ToString();

				if (myReader != null)
					myReader.Close();

				// concateno al path letto dal registry il folder Data
				valueDataPath = Path.Combine(valueDataPath, DatabaseTaskConsts.DataFolder);

				// cerco di capire se ci sono state personalizzazioni dei path per il file di dati e di log
				FindDefaultDataAndDefaultLogPath(connection, isPrimaryInstance, instanceName, valueInstanceName, out defaultFolderDataPath, out defaultFolderLogPath);

				if (string.IsNullOrEmpty(defaultFolderDataPath))
					defaultFolderDataPath = valueDataPath;
				if (string.IsNullOrEmpty(defaultFolderLogPath))
					defaultFolderLogPath = valueDataPath;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("DatabaseTask:GetSQLDataRootPath: " + ex.Message);
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
				{
					myReader.Close();
					myReader.Dispose();
				}
			}
		}
		///<summary>
		/// Lettura dal registry del path di salvataggio dei file mdf e ldf se personalizzate
		/// ovvero considero le chiavi DefaultData e DefaultLog
		///</summary>
		//---------------------------------------------------------------------
		private void FindDefaultDataAndDefaultLogPath
			(
			TBConnection myConnection,
			bool isPrimaryInstance,
			string instanceName,
			string valueInstanceName,
			out string defaultFolderDataPath,
			out string defaultFolderLogPath
			)
		{
			defaultFolderDataPath = defaultFolderLogPath = string.Empty;

			TBCommand myCommand = new TBCommand(myConnection);
			IDataReader myReader = null;

			// leggo il valore della chiave DefaultData o DefaultLog (se esistono)
			if (isPrimaryInstance && string.IsNullOrEmpty(valueInstanceName))
			{
				try
				{
					myCommand.CommandText = string.Format(DatabaseTaskConsts.XPRegRead, DatabaseTaskConsts.KeyLocalMachine, DatabaseTaskConsts.PrimaryInstanceDefaultDataAndLog, DatabaseTaskConsts.KeyDefaultData);
					myReader = myCommand.ExecuteReader();
					while (myReader.Read())
						defaultFolderDataPath = myReader[1].ToString();

					if (myReader != null && !myReader.IsClosed)
					{
						myReader.Close();
						myReader.Dispose();
					}

					myCommand.CommandText = string.Format(DatabaseTaskConsts.XPRegRead, DatabaseTaskConsts.KeyLocalMachine, DatabaseTaskConsts.PrimaryInstanceDefaultDataAndLog, DatabaseTaskConsts.KeyDefaultLog);
					myReader = myCommand.ExecuteReader();
					while (myReader.Read())
						defaultFolderLogPath = myReader[1].ToString();
				}
				catch (Exception ex)
				{
					Debug.WriteLine("DatabaseTask:FindDefaultDataAndDefaultLogPath: " + ex.Message);
				}
				finally
				{
					if (myReader != null && !myReader.IsClosed)
					{
						myReader.Close();
						myReader.Dispose();
					}
				}
			}
			else
			{
				try
				{
					myCommand.CommandText = string.Format
						(
						DatabaseTaskConsts.XPRegRead,
						DatabaseTaskConsts.KeyLocalMachine,
						string.Format(DatabaseTaskConsts.OtherInstanceDefaultDataAndLog, string.IsNullOrEmpty(valueInstanceName) ? instanceName : valueInstanceName),
						DatabaseTaskConsts.KeyDefaultData
						);

					myReader = myCommand.ExecuteReader();
					while (myReader.Read())
						defaultFolderDataPath = myReader[1].ToString();

					if (myReader != null && !myReader.IsClosed)
					{
						myReader.Close();
						myReader.Dispose();
					}

					myCommand.CommandText = string.Format
						(
						DatabaseTaskConsts.XPRegRead,
						DatabaseTaskConsts.KeyLocalMachine,
						string.Format(DatabaseTaskConsts.OtherInstanceDefaultDataAndLog, string.IsNullOrEmpty(valueInstanceName) ? instanceName : valueInstanceName),
						DatabaseTaskConsts.KeyDefaultLog
						);
					myReader = myCommand.ExecuteReader();
					while (myReader.Read())
						defaultFolderLogPath = myReader[1].ToString();
				}
				catch (Exception ex)
				{
					Debug.WriteLine("DatabaseTask:FindDefaultDataAndDefaultLogPath: " + ex.Message);
				}
				finally
				{
					if (myReader != null && !myReader.IsClosed)
					{
						myReader.Close();
						myReader.Dispose();
					}
				}
			}
		}
		#endregion
	}
}
