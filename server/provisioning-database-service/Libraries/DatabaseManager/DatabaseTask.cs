using Microarea.Common.DiagnosticManager;
using Microarea.ProvisioningDatabase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.ProvisioningDatabase.Libraries.DatabaseManager
{
	#region Parametri creazione database SQL
	//========================================================================
	public class SQLCreateDBParameters
	{
		public string DatabaseName = string.Empty;

		// parametri per il file di dati
		public string DataFileName = string.Empty;
		public string DataPathFileName = string.Empty;
		public bool DataFileGrowthByPercent = true;
		public int DataFileGrowth = 10;
		public bool DataUnrestrictedFileGrowth = true;
		public int DataRestrictFileGrowthMB = 2;
		public string DataFileMaxSize = string.Empty;
		public int DataFileInitialSize = 0;

		// parametri per il file di log
		public string LogFileName = string.Empty;
		public string LogPathFileName = string.Empty;
		public bool LogFileGrowthByPercent = true;
		public int LogFileGrowth = 10;
		public bool LogUnrestrictedFileGrowth = true;
		public int LogRestrictFileGrowthMB = 2;
		public string LogFileMaxSize = string.Empty;
		public int LogFileInitialSize = 0;

		// altri parametri
		public bool TruncateLogFile = true;
		public bool AutoShrink = false;
	}
	#endregion

	#region Parametri creazione database Azure
	//========================================================================
	public class AzureCreateDBParameters
	{
		public string DatabaseName = string.Empty;
		public string Edition = AzureEdition.Standard.ToString();
		public string ServiceObjective = AzureServerLevelObjective.S2.ToString();
		public string MaxSize = AzureMaxSize.GB250.ToString();
		public bool AutoShrink = false;
	}
	#endregion

	#region Parametri backup database SQL
	//========================================================================
	public class SQLBackupDBParameters
	{
		public string DatabaseName = string.Empty;
		public string BackupFilePath = string.Empty;
		public string Description = string.Empty;
		public bool Overwrite = false;
	}
	#endregion

	#region Parametri restore database SQL
	//========================================================================
	public class SQLRestoreDBParameters
	{
		public string DatabaseName = string.Empty;
		public string RestoreFilePath = string.Empty;
		public string DataLogicalName = string.Empty;
		public string DataPhysicalName = string.Empty;
		public string LogLogicalName = string.Empty;
		public string LogPhysicalName = string.Empty;
		public bool ForceRestore = false;
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
		public const string CustomSizeDb = ", SIZE = {0}"; // se richiesto va al posto di {5} e/o {10}

		// creazione database Azure SQL Database
		public const string CreateAzureDb = "CREATE DATABASE [{0}] COLLATE {1}	(EDITION = N'{2}', SERVICE_OBJECTIVE = N'{3}', MAXSIZE = {4})";

		// impostazione truncate log file
		public const string SetTruncateLogFileSimple = "ALTER DATABASE [{0}] SET RECOVERY SIMPLE";
		// impostazione AUTO_SHRINK
		public const string SetAutoShrinkON = "ALTER DATABASE [{0}] SET AUTO_SHRINK ON";
		// impostazione AUTO_CLOSE (nelle versioni Express Edition viene messo d'ufficio a true)
		public const string SetAutoCloseOFF = "ALTER DATABASE [{0}] SET AUTO_CLOSE OFF";

		public const string DataFileSuffix = "_Data.MDF";
		public const string LogFileSuffix = "_Log.LDF";

		// comando per il backup di un db
		public const string BackupDB = "BACKUP DATABASE [{0}] TO DISK = '{1}' WITH DESCRIPTION = '{2}' {3}";
		public const string OverwriteBackup = ", INIT";

		// comandi per il restore di un db
		public const string RestoreDB = "RESTORE DATABASE [{0}] FROM DISK = '{1}'";
		public const string WithMoveOption = " WITH MOVE '{2}' TO '{3}', MOVE '{4}' TO '{5}'";
		public const string ReplaceOption = "REPLACE";

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
		private bool isAzureDB = false;

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
		//--------------------------------------------------------------------------------
		public bool IsAzureDB { get { return isAzureDB; } set { isAzureDB = value; } }

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
		public DatabaseTask(bool isAzureDB = false)
		{
			this.isAzureDB = isAzureDB;
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
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
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

					using (TBCommand createDbCommand = new TBCommand(myConnection))
					{
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

						DateTime start = DateTime.Now;
						Debug.WriteLine(string.Format("Start SQL Server database {0} creation: ", createParameters.DatabaseName) + start.ToString("hh:mm:ss.fff"));
						createDbCommand.ExecuteNonQuery();
						DateTime end = DateTime.Now;
						Debug.WriteLine("End SQL Server database creation: " + end.ToString("hh:mm:ss.fff"));
						TimeSpan ts = end - start;
						Debug.WriteLine("SQL Server database creation - total seconds: " + ts.TotalSeconds.ToString());
					}

					// se si è scelta l'opzione "Truncate file log at checkpoints" devo impostare il RecoveryModel = SIMPLE
					// il default di SQL è FULL. uso la sintassi di ALTER DATABASE perchè la sp_dboption é deprecata
					if (createParameters.TruncateLogFile)
					{
						using (TBCommand createDbCommand = new TBCommand(myConnection))
						{
							createDbCommand.CommandText = string.Format(DatabaseTaskConsts.SetTruncateLogFileSimple, createParameters.DatabaseName);
							createDbCommand.ExecuteNonQuery();
						}
					}

					// se si è scelta l'opzione "Auto shrink" devo impostare il parametro AUTO_SHRINK a ON
					// il default di SQL è OFF. uso la sintassi di ALTER DATABASE perchè la sp_dboption é deprecata
					if (createParameters.AutoShrink)
					{
						using (TBCommand createDbCommand = new TBCommand(myConnection))
						{
							createDbCommand.CommandText = string.Format(DatabaseTaskConsts.SetAutoShrinkON, createParameters.DatabaseName);
							createDbCommand.ExecuteNonQuery();
						}
					}

					// imposto d'ufficio il parametro AUTO_CLOSE a OFF (il default per la Express Edition e' ON)
					using (TBCommand createDbCommand = new TBCommand(myConnection))
					{
						createDbCommand.CommandText = string.Format(DatabaseTaskConsts.SetAutoCloseOFF, createParameters.DatabaseName);
						createDbCommand.ExecuteNonQuery();
					}

					success = true;
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Function, "DatabaseTask.Create");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, e.Message, extendedInfo);
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
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
				return result;
			}

			try
			{
				using (TBConnection myConnection = new TBConnection(CurrentStringConnection, DBMSType.SQLSERVER))
				{
					myConnection.Open();

					using (TBCommand createDbCommand = new TBCommand(myConnection))
					{
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

						DateTime start = DateTime.Now;
						Debug.WriteLine(string.Format("Start Azure database {0} creation: ", azureParams.DatabaseName) + start.ToString("hh:mm:ss.fff"));
						createDbCommand.ExecuteNonQuery();
						DateTime end = DateTime.Now;
						Debug.WriteLine("End Azure database creation: " + end.ToString("hh:mm:ss.fff"));
						TimeSpan ts = end - start;
						Debug.WriteLine("Azure database creation - total seconds: " + ts.TotalSeconds.ToString());
					}

					// se si è scelta l'opzione "Auto shrink" devo impostare il parametro AUTO_SHRINK a ON
					// il default di SQL è OFF. uso la sintassi di ALTER DATABASE perchè la sp_dboption é deprecata
					if (azureParams.AutoShrink)
					{
						using (TBCommand createDbCommand = new TBCommand(myConnection))
						{
							createDbCommand.CommandText = string.Format(DatabaseTaskConsts.SetAutoShrinkON, azureParams.DatabaseName);
							createDbCommand.ExecuteNonQuery();
						}
					}

					result = true;
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Function, "DatabaseTask.CreateAzureDatabase");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, e.Message, extendedInfo);
				result = false;
			}

			DatabaseTaskEventArgs args = new DatabaseTaskEventArgs();
			args.Result = result;
			args.DatabaseTaskDiagnostic = diagnostic;
			OperationCompleted?.Invoke(this, args);

			return result;
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
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
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
							string.Format(DatabaseManagerStrings.CompleteBackup, bakParams.DatabaseName),
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
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Function, "DatabaseTask.Backup");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorDatabaseBackup, extendedInfo);
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
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
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
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Function, "DatabaseTask.RestoreWithMove");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.ErrorDatabaseRestore, restoreParams.DatabaseName), extendedInfo);
			}

			return result;
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
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
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
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Function, "DatabaseTask.LoadFileListOnly");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, (e.Number == 3169) ? DatabaseManagerStrings.BackupFileVersionIsRecent : DatabaseManagerStrings.ErrorLoadingFileListOnly, extendedInfo);
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
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
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
						diagnostic.SetInformation(DatabaseManagerStrings.BackupSetIsValid);
					}
					result = true;
				}
			}
			catch (TBException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Function, "DatabaseTask.VerifyBackupFile");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorDuringVerifyDatabase, extendedInfo);
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

		#region Create login
		//---------------------------------------------------------------------
		public bool CreateLogin(string login, string password)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
				return false;
			}

			if ((string.Compare(login, DatabaseLayerConsts.LoginSa, StringComparison.InvariantCultureIgnoreCase) == 0) ||
				(string.Compare(login, DatabaseLayerConsts.PublicUser, StringComparison.InvariantCultureIgnoreCase) == 0))
			{
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.LoginNameNoAdmitted, login));
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
						myCommand.CommandText = string.Format("CREATE LOGIN [{0}] WITH PASSWORD = '{1}'", login, password);

						if (!isAzureDB)
							myCommand.CommandText += ", CHECK_POLICY = OFF"; // sintassi non supportata in Azure

						myCommand.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Parameters, login);
				extendedInfo.Add(DatabaseManagerStrings.Function, "CreateLogin");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);

				// se il numero dell'errore è 15025 significa che si sta aggiungendo una login che già esiste
				// perciò non devo dare un errore bloccante che invalida l'operazione, bensì procedere con le
				// successive elaborazioni
				if (e.Number == 15025)
				{
					Diagnostic.Set(DiagnosticType.Warning, string.Format(DatabaseManagerStrings.LoginAlreadyExists, login), extendedInfo);
					return true;
				}

				// se il numero dell'errore è 15247 significa che l'utente che ha aperto la connessione
				// non ha i privilegi per creare una una login. Pertanto blocco l'elaborazione.
				if (e.Number == 15247)
				{
					Diagnostic.Set
						(DiagnosticType.Error,
						string.Format(DatabaseManagerStrings.UserWithoutPermission, login) + e.Message,
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
						string.Format(DatabaseManagerStrings.PwdValidationFailed, login) + e.Message,
						extendedInfo);
					return false;
				}

				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorCreateLogin, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region CreateUser
		//---------------------------------------------------------------------
		public bool CreateUser(string login, string dbName)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
				return false;
			}

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = dbName;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;

						try
						{
							myCommand.CommandText = string.Format("CREATE USER [{0}] FOR LOGIN [{0}] WITH DEFAULT_SCHEMA = [dbo]", login);
							myCommand.ExecuteNonQuery();
						}
						catch (SqlException e)
						{
							// se il numero dell'errore è 15023 significa che si sta aggiungendo un utente che già esiste
							// perciò non devo dare un errore bloccante che invalida l'operazione, bensì procedere con le successive elaborazioni
							if (e.Number == 15023)
								Diagnostic.Set(DiagnosticType.Warning, string.Format(DatabaseManagerStrings.UserAlreadyExists, login) + e.Message);
							else
							{
								// se il numero dell'errore è 15247 significa che l'utente che ha aperto la connessione
								// non ha i privilegi per creare una una login. Pertanto blocco l'elaborazione.
								if (e.Number == 15247)
								{
									Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.UserWithoutPermission, login) + e.Message);
									return false;
								}
								// la login ha già un account sotto un differente username
								if (e.Number == 15063)
								{
									Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.LoginAlreadyExists, login) + e.Message);
									return false;
								}
								// se il numero dell'errore è 15118 significa che si sta aggiungendo un utente associata ad una login la cui password
								// non rispetta le politiche di sicurezza imposte da Windows. 
								if (e.Number == 15118)
								{
									Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.PwdValidationFailed, login) + e.Message);
									return false;
								}

								Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.ErrorCreatingUser, login, e.Message));
								return false;
							}
						}

						try
						{
							myCommand.CommandText = string.Format("ALTER ROLE {0} ADD MEMBER [{1}]", DatabaseLayerConsts.RoleDbOwner, login);
							myCommand.ExecuteNonQuery();
						}
						catch (SqlException e)
						{
							// se il numero dell'errore è 15151 significa che si sta modificando il ruolo di un utente che non e' stato ancora 
							// aggiunto al database (Cannot add the principal 'Test1', because it does not exist or you do not have permission.)
							if (e.Number == 15151)
							{
								Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.UserWithoutPermission, login) + e.Message);
								return false;
							}

							Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.ErrorAddingLoginToRole, login, DatabaseLayerConsts.RoleDbOwner, e.Message));
							return false;
						}
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Parameters, login);
				extendedInfo.Add(DatabaseManagerStrings.Function, "CreateUser");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorCreateLogin, extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region ChangeDbo
		//---------------------------------------------------------------------
		public bool ChangeDbo(string login, string dbName)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
				return false;
			}

			try
			{
				// non posso utilizzare il metodo ChangeDatabase in caso di Azure
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
				builder.InitialCatalog = dbName;

				using (SqlConnection myConnection = new SqlConnection(builder.ConnectionString))
				{
					myConnection.Open();

					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandText = string.Format("ALTER AUTHORIZATION ON DATABASE::{0} TO {1}", dbName, login);
						myCommand.ExecuteNonQuery();
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Parameters, login);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ChangeDbo");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.ErrorChangingDbo, login, dbName), extendedInfo);
				return false;
			}

			return true;
		}
		#endregion

		#region TryToConnect
		/// <summary>
		/// TryToConnect
		/// </summary>
		//---------------------------------------------------------------------
		public bool TryToConnect()
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
				return false;
			}

			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);

			bool result = false;

			try
			{
				//tento la connessione
				using (SqlConnection testConnection = new SqlConnection(CurrentStringConnection))
				{
					testConnection.Open();
					result = true;
				}
			}
			catch (SqlException ex)
			{
				Debug.WriteLine(ex.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, ex.Message);
				extendedInfo.Add(DatabaseManagerStrings.Server, ex.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, ex.Number);
				extendedInfo.Add(DatabaseManagerStrings.State, ex.State);
				extendedInfo.Add(DatabaseManagerStrings.Function, "TryToConnect");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseManagerStrings.Source, ex.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, ex.StackTrace);

				// la login non esiste o la password e' sbagliata
				if (ex.Number == 18456)
				{
					Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorConnectionNotValid + " " + ex.Message, extendedInfo);
					return false;
				}

				// la login non ha i permessi per connettersi al database specificato, ma la password e' corretta (altrimenti ci saremmo fermati prima)
				// The server principal "Login name" is not able to access the database "db name" under the current security context.
				if (ex.Number == 916)
				{
					Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.UserWithoutPermission, builder.UserID) + ex.Message, extendedInfo);
					return false;
				}

				Diagnostic.Set(DiagnosticType.Error, ex.Message + "\r\n" + DatabaseManagerStrings.ErrorConnectionNotValid, extendedInfo);
			}

			return result;
		}

		/// <summary>
		/// TryToConnect
		/// Ritorna il numero di errore in modo da poter gestire a monte il msg
		/// </summary>
		//---------------------------------------------------------------------
		public bool TryToConnect(out int errorNumber)
		{
			errorNumber = -1;

			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
				return false;
			}

			// per estrapolare il nome utente 
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);

			bool result = false;

			try
			{
				//tento la connessione
				using (SqlConnection testConnection = new SqlConnection(CurrentStringConnection))
				{
					testConnection.Open();
					result = true;
				}
			}
			catch (SqlException ex)
			{
				Debug.WriteLine(ex.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, ex.Message);
				extendedInfo.Add(DatabaseManagerStrings.Server, ex.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, ex.Number);
				extendedInfo.Add(DatabaseManagerStrings.State, ex.State);
				extendedInfo.Add(DatabaseManagerStrings.Function, "TryToConnect");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseManagerStrings.Source, ex.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, ex.StackTrace);
				errorNumber = ex.Number;

				// la login non esiste o la password e' sbagliata
				if (ex.Number == 18456)
				{
					Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorConnectionNotValid + " " + ex.Message, extendedInfo);
					return false;
				}

				// la login non ha i permessi per connettersi al database specificato, ma la password e' corretta (altrimenti ci saremmo fermati prima)
				// The server principal "Login name" is not able to access the database "db name" under the current security context.
				if (ex.Number == 916)
				{
					Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.UserWithoutPermission, builder.UserID) + ex.Message, extendedInfo);
					return false;
				}

				Diagnostic.Set(DiagnosticType.Error, ex.Message + "\r\n" + DatabaseManagerStrings.ErrorConnectionNotValid, extendedInfo);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Function, "TryToConnect");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.SQLDataAccess");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.InnerException);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, e.Message + "\r\n" + DatabaseManagerStrings.ErrorConnectionNotValid, extendedInfo);
			}

			return result;
		}
		#endregion

		#region ExistDataBase - Verifica se il db specificato esiste già (non di sistema)
		/// <summary>
		/// Si connette al master e verifica se esiste il db specificato dal dbName
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistDataBase(string dbName)
		{
			bool existDataBase = false;
			string query = "SELECT COUNT(*) FROM sysdatabases WHERE name = @dbName";

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
						myCommand.Parameters.AddWithValue("@dbName", dbName);
						if ((int)myCommand.ExecuteScalar() > 0)
							existDataBase = true;
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ExistDataBase");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorConnectionNotValid, extendedInfo);
			}

			return existDataBase;
		}
		#endregion

		#region ExistLogin - Si connette al master e verifica se la loginName esiste
		/// <summary>
		/// Si connette al master e verifica se la login identificata da loginName esiste
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistLogin(string loginName)
		{
			bool existLogin = false;

			string query = isAzureDB
				? "SELECT COUNT(*) FROM sys.sql_logins WHERE name = @User"
				: "SELECT COUNT(*) FROM syslogins WHERE name = @User";

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
						existLogin = (int)myCommand.ExecuteScalar() == 1;
					}
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ExistLogin");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorConnectionNotValid, extendedInfo);
			}

			return existLogin;
		}
		#endregion

		#region ExistLoginIntoDb - True se la Login risulta assegnata a un database dbName
		/// <summary>
		/// Testa l'esistenza di una Login assegnata a un db
		/// </summary>
		//---------------------------------------------------------------------
		public bool ExistLoginIntoDb(string loginName, string dbName)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.ProvisioningDatabase.Libraries.DatabaseManager.ExistLoginIntoDb method unavailable for Azure database!!");
				return false;
			}

			bool found = false;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(CurrentStringConnection))
				{
					myConnection.Open();
					myConnection.ChangeDatabase(dbName);

					using (SqlCommand myCommand = new SqlCommand())
					{
						myCommand.Connection = myConnection;
						myCommand.CommandText = DatabaseLayerConsts.SPHelpUser;
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
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.StoredProcedure, DatabaseLayerConsts.SPHelpUser);
				extendedInfo.Add(DatabaseManagerStrings.Function, "ExistLoginIntoDb");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorConnectionNotValid, extendedInfo);
				return false;
			}

			return found;
		}
		#endregion

		//---------------------------------------------------------------------
		public bool LoginIsDBOwnerRole(string loginName)
		{
			if (isAzureDB)
			{
				Debug.Fail("Microarea.ProvisioningDatabase.Libraries.DatabaseManager.LoginIsDBOwnerRole method unavailable for Azure database!!");
				return false;
			}

			return SPHelpUser(loginName, DatabaseLayerConsts.RoleDbOwner);
		}

		#region SPHelpUser e SPhelpsrvrolemember
		/// <summary>
		/// Restituisce informazioni sulle entità a livello di database nel database corrente.
		/// </summary>
		//---------------------------------------------------------------------
		public bool SPHelpUser(string loginName, string roleName)
		{
			if (string.IsNullOrWhiteSpace(CurrentStringConnection))
			{
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
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
						myCommand.CommandText = DatabaseLayerConsts.SPHelpUser;
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
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Parameters, loginName);
				extendedInfo.Add(DatabaseManagerStrings.Parameters, roleName);
				extendedInfo.Add(DatabaseManagerStrings.Function, DatabaseLayerConsts.SPHelpUser);
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorSPHelpUser, extendedInfo);
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
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
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
						myCommand.CommandText = DatabaseLayerConsts.SPHelpUser;
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
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Parameters, userName);
				extendedInfo.Add(DatabaseManagerStrings.Parameters, roleName);
				extendedInfo.Add(DatabaseManagerStrings.Function, "SPHelpUserLITE");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorSPHelpUser, extendedInfo);
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
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrConnectStringEmpty);
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
						myCommand.CommandText = DatabaseLayerConsts.SPHelpSrvRoleMember;
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
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Parameters, loginName);
				extendedInfo.Add(DatabaseManagerStrings.Parameters, roleName);
				extendedInfo.Add(DatabaseManagerStrings.Function, DatabaseLayerConsts.SPHelpSrvRoleMember);
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, DatabaseManagerStrings.ErrorSPHelpsrvrolemember, extendedInfo);
				return false;
			}

			return hasRole;
		}
		#endregion

		#region DeleteDatabaseObjects - Cancella tutti gli oggetti di un db
		/// <summary>
		/// DeleteDatabaseObjects
		/// cancella tutte le tabelle, le view e le stored procedure in un database (esclusi gli oggetti di sistema)
		/// </summary>
		//---------------------------------------------------------------------
		public bool DeleteDatabaseObjects()
		{
			bool result = false;

			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CurrentStringConnection);
			string dbName = builder.InitialCatalog;

			SqlCommand myDeleteCommand = null;

			try
			{
				using (SqlConnection deleteDBConnection = new SqlConnection(CurrentStringConnection))
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

					foreach (string trigger in myObjectList)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("DROP TRIGGER [{0}]", trigger);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
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
							if ((string.Compare(mySqlDataReader["name"].ToString(), "sysconstraints", StringComparison.InvariantCultureIgnoreCase) != 0) &&
								(string.Compare(mySqlDataReader["name"].ToString(), "syssegments", StringComparison.InvariantCultureIgnoreCase) != 0) &&
								(string.Compare(mySqlDataReader["name"].ToString(), "database_firewall_rules", StringComparison.InvariantCultureIgnoreCase) != 0)) // Azure
								myObjectList.Add(mySqlDataReader["name"].ToString());
						}

					foreach (string view in myObjectList)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("DROP VIEW [{0}]", view);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
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

					foreach (string procedure in myObjectList)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("DROP PROCEDURE [{0}]", procedure);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
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


					for (int i = 0; i < myObjectList.Count; i++)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("ALTER TABLE [{0}] DROP CONSTRAINT [{1}]", userFKTables[i], myObjectList[i]);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
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

					foreach (string table in myObjectList)
					{
						try
						{
							myDeleteCommand.CommandText = string.Format("DROP TABLE [{0}]", table);
							myDeleteCommand.ExecuteNonQuery();
							myDeleteCommand.Dispose();
						}
						catch (Exception e)
						{
							Debug.WriteLine(string.Format("An error occurring deleting {0} ({1})", table, e.Message));
							continue;
						}
					}

					myDeleteCommand.Dispose();
					result = true;
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "DeleteDatabaseObjects");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.ErrorDeletingObjectsFromDB, dbName), extendedInfo);
			}
			catch (Exception aExc)
			{
				Debug.WriteLine(aExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, aExc.Message);
				extendedInfo.Add(DatabaseManagerStrings.Function, "DeleteDatabaseObjects");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.TaskBuilderNet.Data.DatabaseLayer");
				extendedInfo.Add(DatabaseManagerStrings.Source, aExc.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, aExc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseManagerStrings.ErrorDeletingObjectsFromDB, dbName), extendedInfo);
			}
			finally
			{
				if (myDeleteCommand != null)
					myDeleteCommand.Dispose();
			}
			return result;
		}
		#endregion

		#region DeleteDatabase - Cancella un database
		/// <summary>
		/// DeleteDatabase
		/// </summary>
		//---------------------------------------------------------------------
		public bool DeleteDatabase(DeleteDatabaseBodyContent deleteContent)
		{
			bool result = false;

			string erpConnectionString = string.Empty;
			string dmsConnectionString = string.Empty;

			// le stringhe di connessione devono andare sul master, altrimenti il database risulterebbe in uso
			if (isAzureDB)
			{
				// se il db e' su Azure e' necessario connettersi al master con specifiche credenziali di amministrazione, perche' il dbowner del db non potrebbe avere i permessi
				erpConnectionString = string.Format(NameSolverDatabaseStrings.SQLAzureConnection, deleteContent.AdminCredentials.Server, DatabaseLayerConsts.MasterDatabase, deleteContent.AdminCredentials.Login, deleteContent.AdminCredentials.Password);
				dmsConnectionString = string.Format(NameSolverDatabaseStrings.SQLAzureConnection, deleteContent.AdminCredentials.Server, DatabaseLayerConsts.MasterDatabase, deleteContent.AdminCredentials.Login, deleteContent.AdminCredentials.Password);
			}
			else
			{
				// su un server SQL e' possibile connettersi con le credenziali del dbo
				erpConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, deleteContent.Database.DBServer, DatabaseLayerConsts.MasterDatabase, deleteContent.Database.DBOwner, deleteContent.Database.DBPassword);
				dmsConnectionString = string.Format(NameSolverDatabaseStrings.SQLConnection, deleteContent.Database.DMSDBServer, DatabaseLayerConsts.MasterDatabase, deleteContent.Database.DMSDBOwner, deleteContent.Database.DMSDBPassword);
			}

			if (deleteContent.DeleteParameters.DeleteERPDatabase)
				result = (isAzureDB) ? DeleteAzureDatabase(erpConnectionString, deleteContent.Database.DBName) : DeleteSqlDatabase(erpConnectionString, deleteContent.Database.DBName);

			if (deleteContent.DeleteParameters.DeleteDMSDatabase)
				result = result &&
					(isAzureDB) ? DeleteAzureDatabase(dmsConnectionString, deleteContent.Database.DMSDBName) : DeleteSqlDatabase(dmsConnectionString, deleteContent.Database.DMSDBName);

			return result;
		}

		/// <summary>
		/// Elimina il database passato come parametro (SQL Server)
		/// </summary>
		/// <param name="connectionString">la stringa di connessione deve essere sul master, altrimenti il db risulta in uso</param>
		/// <param name="databaseName">nome del database da eliminare</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool DeleteSqlDatabase(string connectionString, string databaseName)
		{
			bool result = false;

			try
			{
				using (SqlConnection sqlConnection = new SqlConnection(connectionString))
				{
					sqlConnection.Open();

					string countDb = string.Format("SELECT COUNT(*) FROM sysdatabases WHERE name = N'{0}'", databaseName);

					using (SqlCommand countCmd = new SqlCommand(countDb, sqlConnection))
					{
						if ((int)countCmd.ExecuteScalar() == 1)
						{
							// prima altero il db per fare in modo di rimuovere le connessioni aperte
							string alterDB = string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", databaseName);
							using (SqlCommand alterDbCmd = new SqlCommand(alterDB, sqlConnection))
								alterDbCmd.ExecuteNonQuery();

							// poi eseguo la vera e propria cancellazione
							string dropDb = string.Format("DROP DATABASE [{0}]", databaseName);
							using (SqlCommand dropDbCmd = new SqlCommand(dropDb, sqlConnection))
							{
								dropDbCmd.CommandTimeout = 60;
								dropDbCmd.ExecuteNonQuery();
							}
						}
						else
						{
							ExtendedInfo extendedInfo = new ExtendedInfo();
							extendedInfo.Add(DatabaseManagerStrings.Description, "The database does not exist on specified server");
							extendedInfo.Add(DatabaseManagerStrings.Function, "DeleteSqlDatabase");
							extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
							Diagnostic.Set(DiagnosticType.Warning, string.Format("Unable to delete the database {0}", databaseName), extendedInfo);
						}
					}

					result = true;
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "DeleteSqlDatabase");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format("Unable to delete the database {0}", databaseName), extendedInfo);
			}
			catch (ApplicationException aExc)
			{
				Debug.WriteLine(aExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, aExc.Message);
				extendedInfo.Add(DatabaseManagerStrings.Function, "DeleteSqlDatabase");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, aExc.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, aExc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format("Unable to delete the database {0}", databaseName), extendedInfo);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, ex.Message);
				extendedInfo.Add(DatabaseManagerStrings.Function, "DeleteSqlDatabase");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, ex.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, ex.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format("Unable to delete the database {0}", databaseName), extendedInfo);
			}

			return result;
		}

		/// <summary>
		/// Elimina il database passato come parametro (SQL Azure)
		/// </summary>
		/// <param name="connectionString">la stringa di connessione deve essere sul master, altrimenti il db risulta in uso</param>
		/// <param name="databaseName">nome del database da eliminare</param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool DeleteAzureDatabase(string connectionString, string databaseName)
		{
			bool result = false;

			try
			{
				using (SqlConnection sqlConnection = new SqlConnection(connectionString))
				{
					sqlConnection.Open();

					// poi eseguo la vera e propria cancellazione
					string dropDb = string.Format("DROP DATABASE IF EXISTS [{0}]", databaseName);
					using (SqlCommand dropDbCmd = new SqlCommand(dropDb, sqlConnection))
					{
						dropDbCmd.CommandTimeout = 60; // timeout su Azure ???
						dropDbCmd.ExecuteNonQuery();
					}

					result = true;
				}
			}
			catch (SqlException e)
			{
				Debug.WriteLine(e.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseManagerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseManagerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseManagerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseManagerStrings.Function, "DeleteAzureDatabase");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, e.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, e.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format("Unable to delete the database {0}", databaseName), extendedInfo);
			}
			catch (ApplicationException aExc)
			{
				Debug.WriteLine(aExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, aExc.Message);
				extendedInfo.Add(DatabaseManagerStrings.Function, "DeleteAzureDatabase");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, aExc.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, aExc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format("Unable to delete the database {0}", databaseName), extendedInfo);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseManagerStrings.Description, ex.Message);
				extendedInfo.Add(DatabaseManagerStrings.Function, "DeleteAzureDatabase");
				extendedInfo.Add(DatabaseManagerStrings.Library, "Microarea.ProvisioningDatabase.Libraries.DatabaseManager");
				extendedInfo.Add(DatabaseManagerStrings.Source, ex.Source);
				extendedInfo.Add(DatabaseManagerStrings.StackTrace, ex.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, string.Format("Unable to delete the database {0}", databaseName), extendedInfo);
			}

			return result;
		}
		#endregion
	}
}
