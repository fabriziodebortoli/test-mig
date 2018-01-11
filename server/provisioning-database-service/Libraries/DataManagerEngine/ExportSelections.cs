using System;
using System.Text;
using Microarea.Common.NameSolver;
using Microarea.ProvisioningDatabase.Libraries.DatabaseManager;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.ProvisioningDatabase.Libraries.DataManagerEngine
{
	/// <summary>
	/// ExportSelections (struttura in memoria con le opzioni scelte dall'utente)
	/// </summary>
	//=========================================================================
	public class ExportSelections : DataManagerSelections
	{
		private bool allTables = true;
		public bool SelectColumns = false;
		public bool WriteQuery = false;
		public bool OneFileForTable = true;
		public bool SchemaInfo = false;

		public bool ExportTBCreated = true;	 // esportazione della colonna base TBCreated
		public bool ExportTBModified = false; // esportazione della colonna base TBModified

		// variabili per la gestione del file di configurazione da pre-caricare
		public bool LoadFromConfigurationFile = false;
		public string ConfigurationFilePathToLoad = string.Empty;

		public bool SaveInConfigurationFile = false;
		public string ConfigurationFilePathToSave = string.Empty;
		public bool ExecuteScriptTextBeforeExport = false;
		public string ScriptTextBeforeExport = string.Empty;

		// variabile di comodo per fare la clear degli items solo qualora cambino 
		// determinate selezioni nei parametri
		public bool ClearItems = false;

		//--------------------------------------------------------------------
		public bool AllTables
		{
			get { return allTables; }
			set
			{
				allTables = value;
				if (allTables)
				{
					Catalog.ClearSelectedTableEntry();
					ClearItems = true;
				}
			}
		}

		/// <summary>
		/// costruttore con assegnazione member della classe ContextInfo
		/// </summary>
		//---------------------------------------------------------------------
		public ExportSelections(ContextInfo context, BrandLoader brandLoader)
			: base(context, brandLoader)
		{
			Catalog = new CatalogInfo();
			Catalog.Load(ContextInfo.Connection, true);
		}

		/// <summary>
		/// costruttore con assegnazione member della classe CommonInfo e del Catalog
		/// viene istanziato dal wizard x la gestione dei dati di default
		/// </summary>
		//---------------------------------------------------------------------
		public ExportSelections(ContextInfo context, CatalogInfo catalog, BrandLoader brandLoader)
			: base(context, catalog, brandLoader)
		{ }

		//---------------------------------------------------------------------
		public void Clear()
		{
			Catalog.ClearSelectedTableEntry();

			allTables = true;
			SelectColumns = false;
			WriteQuery = false;
			OneFileForTable = true;
			SchemaInfo = false;
		}

		# region Costruzione delle stringhe di query, in base alle selezioni effettuate esportazione e count
		/// <summary>
		/// costruisce la stringa di count con la query di selezione. Questa serve per verificare quanti record
		/// verranno esportati
		/// </summary>
		//---------------------------------------------------------------------
		public string MakeCountExportQuery(CatalogTableEntry entry)
		{
			StringBuilder sqlText = new StringBuilder();

			sqlText.Append("SELECT ");

			if (!SelectColumns)
				sqlText.Append("COUNT(*) ");

			//aggiungo la FROM
			sqlText.Append(" FROM ");
			sqlText.Append(entry.TableName);

			// se necessario aggiungo clausola di WHERE
			if (entry.WhereClause.Length > 0)
			{
				sqlText.Append(" WHERE ");
				sqlText.Append(entry.WhereClause);
			}

			return sqlText.ToString();
		}

		/// <summary>
		/// costruisce la stringa con la query di selezione per effettuare l'esportazione dei dati
		/// </summary>
		//---------------------------------------------------------------------
		public string MakeExportQuery(CatalogTableEntry entry)
		{
			StringBuilder sqlText = new StringBuilder();
			sqlText.Append("SELECT ");

			if (!SelectColumns)
			{
				// se vuole esportare entrambe le colonne base allora faccio una SELECT * sulla tabella
				if (ExportTBCreated && ExportTBModified)
					sqlText.Append("* ");
				else
				{
					//carico le informazioni relative alle colonne
					entry.LoadColumnsInfo(ContextInfo.Connection, true);

					// se ha scelto di portare tutte le colonne devo cmq selezionarle una ad una
					// perchè devo controllare le colonne base TBCreated e TBModified
					foreach (CatalogColumn col in entry.ColumnsInfo)
					{
						// colonna TBCreated
						if (string.Compare(col.Name, DatabaseLayerConsts.TBCreatedColNameForSql, StringComparison.OrdinalIgnoreCase) == 0 ||
							string.Compare(col.Name, DatabaseLayerConsts.TBCreatedIDColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
						{
							if (!ExportTBCreated)
								continue;
						}

						// colonna TBModified
						if (string.Compare(col.Name, DatabaseLayerConsts.TBModifiedColNameForSql, StringComparison.OrdinalIgnoreCase) == 0 ||
							string.Compare(col.Name, DatabaseLayerConsts.TBModifiedIDColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
						{
							if (!ExportTBModified)
								continue;
						}

						// per ogni colonna
                        if (ContextInfo.DbType == DBMSType.SQLSERVER)
                            sqlText.Append(string.Format("[{0}]", col.Name));
                        else if (ContextInfo.DbType == DBMSType.POSTGRE)
                            sqlText.Append(string.Format("{0}", col.Name));

						sqlText.Append(", ");
					}

					if (sqlText.ToString().Length == 0)
						return string.Empty;

					sqlText.Remove(sqlText.Length - 2, 2); //tolgo l'ultima virgola e lo spazio
				}
			}
			else
			{
				//nell'array delle colonne selezionate ho giá i segmenti di chiave primaria e le colonne base
				foreach (string column in entry.SelectedColumnsList)
				{
					// TBCreated
					if (string.Compare(column, DatabaseLayerConsts.TBCreatedColNameForSql, StringComparison.OrdinalIgnoreCase) == 0 ||
						string.Compare(column, DatabaseLayerConsts.TBCreatedIDColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
					{
						if (!ExportTBCreated)
							continue;
					}

					// TBModified
					if (string.Compare(column, DatabaseLayerConsts.TBModifiedColNameForSql, StringComparison.OrdinalIgnoreCase) == 0 ||
						string.Compare(column, DatabaseLayerConsts.TBModifiedIDColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
					{
						if (!ExportTBModified)
							continue;
					}

					// per ogni colonna
                    if (ContextInfo.DbType == DBMSType.SQLSERVER)
                        sqlText.Append(string.Format("[{0}]", column));
                    else if (ContextInfo.DbType == DBMSType.ORACLE)
                        sqlText.Append(string.Format("\"{0}\"", column));
                    else if (ContextInfo.DbType == DBMSType.POSTGRE)
                        sqlText.Append(string.Format("{0}", column));


					sqlText.Append(", ");
				}

				if (sqlText.ToString().Length == 0)
					return string.Empty;

				sqlText.Remove(sqlText.Length - 2, 2); //tolgo l'ultima virgola e lo spazio
			}

			//aggiungo la FROM
			sqlText.Append(" FROM ");

            if (ContextInfo.DbType == DBMSType.SQLSERVER)
                sqlText.Append(string.Format("[{0}]", entry.TableName));
            else if (ContextInfo.DbType == DBMSType.ORACLE)
                sqlText.Append(string.Format( "\"{0}\"", entry.TableName));
            else if (ContextInfo.DbType == DBMSType.POSTGRE)
			    sqlText.Append(string.Format("{0}", entry.TableName));

			// se necessario aggiungo clausola di WHERE
			if (entry.WhereClause.Length > 0)
			{
				sqlText.Append(" WHERE ");
				sqlText.Append(entry.WhereClause);
			}

			// aggiungo la clausola di XML (solo se è una connessione SQL)
			if (ContextInfo.DbType == DBMSType.SQLSERVER)
				sqlText.Append(" FOR XML AUTO, XMLDATA");

			return sqlText.ToString();
		}
		# endregion

		//--------------------------------------------------------------------------------
		protected override string GetOperationType()
		{
			return "Export";
		}
	}
}
