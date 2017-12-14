using System.Data;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	///<summary>
	/// DataTable di appoggio per il DataGrid con l'elenco dei backups
	///</summary>
	//================================================================================
	public class BakCommonObjects : DataTable
	{
		//---------------------------------------------------------------------------------
		public BakCommonObjects()
		{
			Columns.Add(new DataColumn(DataTableConsts.BackupDBType, typeof(System.Object)));
			Columns.Add(new DataColumn(DataTableConsts.Selected, typeof(System.Boolean)));
			Columns.Add(new DataColumn(DataTableConsts.DBName, typeof(System.String)));
			Columns.Add(new DataColumn(DataTableConsts.BakPath, typeof(System.String)));
			Columns.Add(new DataColumn(DataTableConsts.Browse, typeof(System.String)));
			Columns.Add(new DataColumn(DataTableConsts.BackupConnectionInfo, typeof(System.Object)));

			Columns[DataTableConsts.Selected].DefaultValue = true;
		}
	}

	///<summary>
	/// Enumerativo per identificare il tipo di database di cui effettuare il backup
	///</summary>
	//================================================================================
	public enum BackupDBType { SYSDB, ERP, DMS }

	///<summary>
	/// Classe di appoggio per avere sottomano le informazioni del vari server e database
	/// coinvolti nella procedura di backup
	///</summary>
	//=========================================================================
	public class BackupConnectionInfo
	{
		private BackupDBType backupDBType;
		private string serverName;
		private string dbName;
		private string login;
		private string pw;
		private bool isWinAuth;
		private string connectionString;
		private string backupFilePath;

		public BackupDBType BackupDBType { get { return backupDBType; } set { backupDBType = value; } }
		public string ServerName { get { return serverName; } set { serverName = value; } }
		public string DbName { get { return dbName; } set { dbName = value; } }
		public string Login { get { return login; } set { login = value; } }
		public string Pw { get { return pw; } set { pw = value; } }
		public bool IsWinAuth { get { return isWinAuth; } set { isWinAuth = value; } }
		public string ConnectionString { get { return connectionString; } set { connectionString = value; } }
		public string BackupFilePath { get { return backupFilePath; } set { backupFilePath = value; } }

		//---------------------------------------------------------------------------
		public BackupConnectionInfo
			(
			BackupDBType backupDBType, 
			string serverName, 
			string dbName, 
			string login, 
			string pw, 
			bool isWinAuth
			)
		{
			this.backupDBType = backupDBType;

			this.serverName = serverName;
			this.dbName = dbName;
			this.login = login;
			this.pw = pw;
			this.isWinAuth = isWinAuth;

			connectionString = isWinAuth
				? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, dbName)
				: string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, dbName, login, pw);
		}
	}

	//================================================================================
	public class DataTableConsts
	{
		public const string BackupDBType = "DBType";
		public const string Selected = "Selected";
		public const string DBName = "DBName";
		public const string BakPath = "BakPath";
		public const string Browse = "Browse";
		public const string BackupConnectionInfo = "BackupConnectionInfo";
	}
}