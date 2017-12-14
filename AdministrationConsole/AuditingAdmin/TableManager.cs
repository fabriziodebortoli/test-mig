using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using Microarea.Console.Core.DBLibrary;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.AuditingAdmin
{
	/// <summary>
	/// l'array di oggetti istanza di TracedColumn viene utilizzato per effettuare 
	/// l'ALTER della tabella di tracciatura quando vengono inserite o tolte 
	/// le colonna in/dalla tracciatura
	/// </summary>
	//=========================================================================
	public class TracedColumn
	{
		public string Name = string.Empty; 
		public string Type = string.Empty;	// tipo + eventuale lunghezza della colonna
		public bool	  ToAdd = false;		// true = colonna é da aggiungere 
											// false = da togliere dalla tracciatura
		//--------------------------------------------------------------------------
		public TracedColumn(string name, string type, bool toAdd)
		{
			Name	= name;
			ToAdd	= toAdd;
			Type	= type;
		}
	}

	/// <summary>
	/// Summary description for TableManager.
	/// </summary>
	//=========================================================================
	public class TableManager
	{
		private ContextInfo contextInfo		= null;
		private CatalogInfo catalogInfo		= null;
		private ArrayList	fixedColumns	= null;

		private DataTable	usersTable	= null;

		//--------------------------------------------------------------------------
		public TableManager(ContextInfo context, CatalogInfo catalog, ArrayList columns)
		{
			contextInfo = context;
			catalogInfo = catalog;
			fixedColumns = columns;
		}

		/// <summary>
		/// Legge tutti gli utenti applicativi legati all'azienda sotto tracciatura. Serve per la gestione dei sinonimi
		/// in ORACLE
		/// </summary>
		//--------------------------------------------------------------------------
		private void LoadAllUser()
		{
			// serve solo per Oracle
			if (contextInfo.DbType != DBMSType.ORACLE || usersTable != null)
				return;

			usersTable = new DataTable();

			// effettuo una connessione al db di sistema per leggere tutti gli utenti associati diversi dal dbowner
			SqlConnection sysConnect = new SqlConnection(contextInfo.ConnectSysDB);
			SqlDataAdapter dataAdapter = null;
			SqlCommand	   command = null;
			string query = string.Empty;
			
			try
			{
				sysConnect.Open();
				
				command = new SqlCommand
					(
						@"SELECT DBUser, DBPassword, DBWindowsAuthentication 
						FROM MSD_CompanyLogins X, dbo.MSD_Companies Y 
						WHERE Y.CompanyId = @CompanyId AND X.CompanyId = Y.CompanyId 
						AND X.disabled = 0 AND Y.CompanyDBOwner != X.LoginId", 
						sysConnect
					);
				
				command.Parameters.Add(new SqlParameter("@CompanyId", Int32.Parse(contextInfo.CompanyId)));
				dataAdapter = new SqlDataAdapter(command);

				dataAdapter.Fill(usersTable);
			}
			catch (SqlException sqlExc)
			{
					DiagnosticViewer.ShowError
					(
					string.Format(Strings.ErrCreatingSynonym),
					sqlExc.Message, 
					sqlExc.Procedure,
					sqlExc.Number.ToString(), 
					Strings.Error
					);
			}
			finally
			{
				command.Dispose();
				dataAdapter.Dispose();
			}
		}
				
		//--------------------------------------------------------------------------
		private string GetCreateText(string tableName)
		{
			string auditTableName = AuditTableConsts.AuditPrefix + tableName;
		
			// Compongo lo script di creazione della tabella AU_tableName 
			// Devo differenziare il comando di creazione a seconda del tipo di database e se è il db è unicode o meno
			string sqlCreate = string.Empty;
			string sqlPKConst = string.Empty;

			switch (contextInfo.DbType)
			{
				case DBMSType.SQLSERVER:
				{
					sqlCreate = (contextInfo.UseUnicode) 
								? string.Format(AuditTableConsts.SqlCreateTableTextUnicode, auditTableName)
								: string.Format(AuditTableConsts.SqlCreateTableText, auditTableName);
					sqlPKConst = string.Format(AuditTableConsts.SqlPKConstraintText, auditTableName);
					break;
				}

				case DBMSType.ORACLE:
				{
					// devo controllare che il nome associato all'AuditTable, composto da AU_ + nometabella
					// non diventi superiore a 30 caratteri, altrimenti tronco la stringa
					if (auditTableName.Length > 30)
						auditTableName = auditTableName.Substring(0, 30);

					sqlCreate = (contextInfo.UseUnicode) 
								? string.Format(AuditTableConsts.OracleCreateTableTextUnicode, auditTableName)
								: string.Format(AuditTableConsts.OracleCreateTableText, auditTableName);

					// devo controllare che il nome della PK , composto da PK_AU_ + nometabella
					// non diventi superiore a 30 caratteri, altrimenti tronco la stringa
					if (string.Concat("PK_", auditTableName).Length > 30)
						auditTableName = auditTableName.Substring(0, 27);

					sqlPKConst = string.Format(AuditTableConsts.OraclePKConstraintText, auditTableName);
					break;
				}
			}

			// aggiungo le colonne che fanno parte della chiave
			CatalogTableEntry catalogEntry = catalogInfo.GetTableEntry(tableName);
			if (catalogEntry.ColumnsInfo == null)
				catalogEntry.LoadColumnsInfo(contextInfo.Connection, true);

			sqlCreate += catalogEntry.CreateColumnsScript(true, false); // solo le colonne segmenti di chiave primaria. Di queste non devo considerare la proprietà di identity

			CatalogColumn colInfo = null;
			
			//aggiungo le colonne di tipo FixedKey
			foreach (FixedColumnsObject fixedCol in fixedColumns)
			{ 
				if (string.Compare(fixedCol.TableName, auditTableName) == 0)
				{
					foreach (string colName in fixedCol.TracedColumns)
					{
						colInfo = catalogEntry.GetColumnInfo(colName);

						if (!colInfo.IsKey)
							sqlCreate += ", " + colInfo.ToSql(true, false); //non devo considerare la proprietà di identity
					}
					break;
				}
			}
			
			// inserisco le colonne di chiave primaria della tabella tablename
			foreach (CatalogColumn col in catalogEntry.ColumnsInfo)
				if (col.IsKey)
					sqlPKConst += "," + col.Name;
			 		
			sqlCreate += sqlPKConst + "))";

			return sqlCreate;
		}

		//--------------------------------------------------------------------------
		public void CreateSynonymOnTable(string auditTableName)
		{
			//scorro il DataTable degli utenti
			if (this.usersTable == null)
				LoadAllUser();

			contextInfo.CreateSynonymOnTable(auditTableName, this.usersTable);
		}

		/// <summary>
		/// permette di creare la tabella di auditing della tabella tableName
		/// </summary>
		//--------------------------------------------------------------------------
		public bool CreateAuditTable(string tableName)
		{
			string auditTableName = AuditTableConsts.AuditPrefix + tableName;
			
			// se il db è Oracle devo controllare che il nome associato all'AuditTable, composto da AU_ + nometabella
			// non diventi superiore a 30 caratteri, altrimenti tronco la stringa
			if (contextInfo.Connection.IsOracleConnection() && auditTableName.Length > 30)
				auditTableName = auditTableName.Substring(0, 30);

			TBCommand command = null;
			string sqlCreate = string.Empty;
			
			try
			{
				sqlCreate = GetCreateText(tableName);
				command = new TBCommand(sqlCreate, contextInfo.Connection);
				command.ExecuteNonQuery();
				command.Dispose();

				//la inserisco nella tabella AUDIT_Tables
				if (InsertIntoAuditTables(tableName))
				{
					//inserisco nel catalog
					CatalogTableEntry auditEntry = new CatalogTableEntry(auditTableName, contextInfo.DbType);
					auditEntry.LoadColumnsInfo(contextInfo.Connection, true);
					
					catalogInfo.TblDBList.Add(auditEntry);

					//devo creare il sinonimo alla tabella per tutti gli utenti associati allo schema
					if (contextInfo.DbType == DBMSType.ORACLE) 
						CreateSynonymOnTable(auditTableName);
					
					return true;
				}			
			}
			catch(TBException err)
			{
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlCreate), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);
				command.Dispose();
				return false;
			}

			return false;
		}

		//--------------------------------------------------------------------------
		public void DropSynonymOnTable(string auditTableName)
		{
			//scorro il DataTable degli utenti
			if (this.usersTable == null)
				LoadAllUser();

			contextInfo.DropSynonymOnTable(auditTableName, this.usersTable); 
		}
		
		/// <summary>
		/// permette di cancellare la tabella di auditing della tabella tableName
		/// </summary>
		//--------------------------------------------------------------------------
		public bool	DropAuditTable(string tableName) 
		{
			string auditTableName = AuditTableConsts.AuditPrefix + tableName;

			// se il db è Oracle devo controllare che il nome associato all'AuditTable, composto da AU_ + nometabella
			// non diventi superiore a 30 caratteri, altrimenti tronco la stringa
			if (contextInfo.Connection.IsOracleConnection() && auditTableName.Length > 30)
				auditTableName = auditTableName.Substring(0, 30);

			string sqlDrop = string.Empty;

			switch (contextInfo.DbType)
			{ 
				case DBMSType.SQLSERVER:
					sqlDrop = string.Format(AuditTableConsts.SqlDropTableText, auditTableName);
					break;
				case DBMSType.ORACLE:
					sqlDrop = string.Format(AuditTableConsts.OracleDropTableText, auditTableName);
					break;
			}

			TBCommand command = new TBCommand(sqlDrop, contextInfo.Connection);
			
			try
			{
				command.ExecuteNonQuery();
				command.Dispose();
				
				if (RemoveFromAuditTables(tableName))
				{
					//cancello la tabella anche dal catalog
					catalogInfo.TblDBList.Remove(catalogInfo.GetTableEntry(auditTableName));
					if (contextInfo.DbType == DBMSType.ORACLE) //devo creare il sinonimo alla tabella per tutti gli utenti associati allo schema
						DropSynonymOnTable(auditTableName);
					return true;
				}	
			}
			catch (TBException err)
			{
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlDrop) ,err.Message, err.Procedure , err.Number.ToString(), Strings.Error);				
				command.Dispose();
				return false;
			}
			return false;
		}

		/// <summary>
		/// effettua l'ALTER della tabella di tracciatura tableAuditName considerando
		/// l'array delle colonne passato come argomento
		/// </summary>
		//--------------------------------------------------------------------------
		public bool	AlterAuditTable(string tableAuditName, ArrayList columns) 
		{
			string sqlAddColumns =  string.Empty;
			string sqlDropColumns = string.Empty;
			
			foreach(TracedColumn tracedCol in columns)
			{
				if (tracedCol.ToAdd)
				{
					if (sqlAddColumns.Length > 0)
						sqlAddColumns += ',';

					switch (contextInfo.DbType)
					{
						case DBMSType.SQLSERVER:
							sqlAddColumns += string.Format("[{0}] {1}", tracedCol.Name, tracedCol.Type);
							break;
						case DBMSType.ORACLE:
							sqlAddColumns += string.Format("\"{0}\" {1}", tracedCol.Name, tracedCol.Type);
							break;
					}
				}
				else
				{
					if (sqlDropColumns.Length > 0)
						sqlDropColumns += ',';
					sqlDropColumns += tracedCol.Name;
				}
			}
		
			if (DropColumns(tableAuditName, sqlDropColumns) && AddColumns(tableAuditName, sqlAddColumns))
			{
				catalogInfo.GetTableEntry(tableAuditName).RefreshColumns(contextInfo.Connection, true);
				return true;
			}
			return false;
		}
		
		//--------------------------------------------------------------------------
		private bool DropColumns(string tableAuditName, string dropColumns)
		{
			// non ho colonne da cancellare
			if (dropColumns.Length == 0)
				return true;

			string sqlDrop = string.Empty;

			switch (contextInfo.DbType)
			{
				case DBMSType.SQLSERVER:
					sqlDrop = string.Format(AuditTableConsts.SqlDropColumnText, tableAuditName, dropColumns);
					break;
				case DBMSType.ORACLE:
					sqlDrop = string.Format(AuditTableConsts.OracleDropColumnText, tableAuditName, dropColumns);
					break;
			}

			TBCommand command = new TBCommand(sqlDrop, contextInfo.Connection);
			
			try
			{
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch (TBException err)
			{
				command.Dispose();
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlDrop), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);
				return false;
			}
			
			return true;
		}

		//--------------------------------------------------------------------------
		private bool AddColumns(string tableAuditName, string addColumns)
		{
			// non ho colonne da cancellare
			if (addColumns.Length == 0)
				return true;
			
			string sqlAdd = string.Empty;

			switch (contextInfo.DbType)
			{
				case DBMSType.SQLSERVER:
					sqlAdd = string.Format(AuditTableConsts.SqlAddColumnText, tableAuditName, addColumns);
					break;
				case DBMSType.ORACLE:
					sqlAdd = string.Format(AuditTableConsts.OraclelAddColumnText, tableAuditName, addColumns);
					break;
			}

			TBCommand command = new TBCommand(sqlAdd, contextInfo.Connection);

			try
			{
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch (TBException err)
			{
				command.Dispose();
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlAdd), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);				
				return false;
			}
			
			return true;
		}

		/// <summary>
		/// inserisce nella tabella AUDIT_Tables la riga relativa a tableName
		/// </summary>
		//--------------------------------------------------------------------------
		private bool InsertIntoAuditTables(string tableName)
		{
			string sqlInsert = string.Format
			(
				"INSERT INTO {0} ({1}, {2}, {3}) VALUES (@TableName, @StartTrace, @Suspended)",
				AuditTableConsts.AuditTablesTableName,
				AuditTableConsts.TableNameCol,
				AuditTableConsts.StartDateCol,
				AuditTableConsts.StopCol
			);

			TBCommand command = new TBCommand(sqlInsert, contextInfo.Connection);
			try
			{			
				command.Parameters.Add("@TableName",	tableName);
				command.Parameters.Add("@StartTrace",	DateTime.Today);
				command.Parameters.Add("@Suspended",	"0");
				command.ExecuteNonQuery();
				command.Dispose();	
			}
			catch (TBException err)
			{
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlInsert), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);				
				command.Dispose();
				return false;
			}
			
			return true;
		}

		/// <summary>
		/// elimina dalla tabella AUDIT_Tables la riga relativa a tableName
		/// </summary>
		//--------------------------------------------------------------------------
		private bool RemoveFromAuditTables(string tableName)
		{
			string delete = string.Format
				(
					"DELETE FROM {0} WHERE {1} = @tablename",
					AuditTableConsts.AuditTablesTableName,
					AuditTableConsts.TableNameCol
				);

			TBCommand command = new TBCommand(delete, contextInfo.Connection);
			command.Parameters.Add("@tablename", tableName);

			try
			{
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch (TBException err)
			{
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, delete), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);				
				command.Dispose();
				return false;
			}				
			
			return true;
		}
		
		/// <summary>
		/// sospende la tracciatura della tabella tableName
		/// </summary>
		//--------------------------------------------------------------------------
		public bool	StopAuditing(string tableName)
		{
			string sqlUpdate = string.Format
				(
					"UPDATE {0} SET {1} = '1' WHERE {2} = @tablename",
					AuditTableConsts.AuditTablesTableName, 
					AuditTableConsts.StopCol, 
					AuditTableConsts.TableNameCol
				);
			
			TBCommand command = new TBCommand(sqlUpdate, contextInfo.Connection);
			command.Parameters.Add("@tablename", tableName);
			
			try
			{
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch (TBException err)
			{
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlUpdate), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);				
				command.Dispose();
				return false;
			}

			return true;
		}
		
		/// <summary>
		/// riprende la tracciatura della tabella tableName
		/// </summary>
		//--------------------------------------------------------------------------
		public bool	RestartAuditing(string tableName)
		{
			string sqlUpdate = string.Format
				(
					"UPDATE {0} SET {1} = '0' WHERE {2} = @tablename",
					AuditTableConsts.AuditTablesTableName, 
					AuditTableConsts.StopCol, 
					AuditTableConsts.TableNameCol
				);

			TBCommand command = new TBCommand(sqlUpdate, contextInfo.Connection);
			command.Parameters.Add("@tablename", tableName);

			try
			{
				command.ExecuteNonQuery();
				command.Dispose();
			}
			catch (TBException err)
			{
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlUpdate), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);
				command.Dispose();
				return false;
			}

			return true;
		}
	}
}
