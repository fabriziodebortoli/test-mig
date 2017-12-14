using System;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Microarea.Console.Core.DBLibrary;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.AuditingAdmin
{
	/// <summary>
	/// Summary description for AuditingQuery
	/// </summary>
	//=========================================================================
	public partial class AuditingQuery : System.Windows.Forms.Form
	{
		#region DataMember Privati
		private ContextInfo		contextInfo	= null;
		private CatalogInfo		catalogInfo = null;
		private string			tableName		= string.Empty;
		private string			auditTableName	= string.Empty;
		private enum			operationType { Unknown, Insert, Update, Delete, ChangeKey }; 

		//gestione DataGrid di visualizzazione
		QueryResultDataGridStyle dataGridStyle;
		#endregion

		#region Costruttore
		/// <summary>
		/// constructor
		/// </summary>
		//---------------------------------------------------------------------
		public AuditingQuery(PlugInTreeNode treeNode, ContextInfo context, CatalogInfo catalog)
		{
			InitializeComponent();
			dataGridStyle = new QueryResultDataGridStyle(this.dtgQueryResult);
		
			//Assegnazione ai DataMember
			contextInfo = context;
			catalogInfo	= catalog;			
	
			//Carico le tabelle e gli utenti nelle combo
			LoadTablesCombo(treeNode);
			LoadUsersCombo();	
			SetDate();
			btnRunQuery.Text = Strings.RunQuery;	
		}
		#endregion

		#region Funzione di inizializzazione dei controls		
		//----------------------------------------------------------------------------------------
		private void SetDate()
		{		
			dtpFrom.MaxDate	= DateTime.Today;
			dtpTo.MinDate	= DateTime.Today;
			dtpFrom.Value	= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
			dtpTo.Value		= DateTime.Today;
			dtpFrom.Text	= dtpFrom.Value.ToShortDateString();
			dtpTo.Text		= dtpTo.Value.ToShortDateString();
		}

		#region Funzioni di inizializzazione combo tabelle		
		/// <summary>
		/// considero le sole tabelle tracciate dell'applicazione (lo faccio navigando 
		/// i nodi dell'applicazione
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadApplicationTracedTable(PlugInTreeNode appNode)
		{
			if (appNode.NodesCount <= 0 || appNode.Type != AuditConstStrings.Application)
				return;			

			foreach (PlugInTreeNode modNode in appNode.Nodes)
				LoadModuleTracedTable(modNode);
		}

		//---------------------------------------------------------------------
		private void LoadModuleTracedTable(PlugInTreeNode modNode)
		{
			if (modNode.NodesCount <= 0 || modNode.Type != AuditConstStrings.Module)
				return;

			//concetto di albero ondemand:se necessario si procede al caricamento dei nodi tabella
			((ApplicationsTree)modNode.TreeView.Parent).LoadTableNodes(modNode);

			foreach (PlugInTreeNode tableNode in modNode.Nodes)
			{
				if (tableNode.Type == AuditConstStrings.TracedTable ||
					tableNode.Type == AuditConstStrings.PauseTraceTable)
					comboBoxTables.Items.Add(tableNode.Text);			
			}			
		}

		/// <summary>
		/// considero tutte le tabelle sotto tracciatura anche quelle sospese
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadAllTracedTable(PlugInTreeNode treeNode)
		{
			string sqlText = string.Format
				(
					"SELECT {0} FROM {1}", 
					AuditTableConsts.TableNameCol,
					AuditTableConsts.AuditTablesTableName
				);
					
			comboBoxTables.Items.Clear();

			IDataReader reader  = null;
			TBCommand command	= null;

			try
			{
				command = new TBCommand(sqlText, contextInfo.Connection);
				reader = command.ExecuteReader();
			
				while (reader.Read())
					comboBoxTables.Items.Add((string)reader[AuditTableConsts.TableNameCol]);
				
				reader.Close();
				command.Dispose();
				comboBoxTables.SelectedItem = treeNode.Text;
			}
			catch (TBException err)
			{
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlText), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);
				reader.Close();
				command.Dispose();
			}
		}
		
		//---------------------------------------------------------------------
		private void LoadTablesCombo(PlugInTreeNode treeNode)
		{
			if (treeNode == null)
				return;

			if (treeNode.Type == AuditConstStrings.Application)
			{
				LoadApplicationTracedTable(treeNode);
				tableLabel.Text = string.Format(Strings.ApplicationTablesLabel, treeNode.Text);
			}
				
			if (treeNode.Type == AuditConstStrings.Module)
			{
				LoadModuleTracedTable(treeNode);			
				tableLabel.Text = string.Format(Strings.ModuleTablesLabel, treeNode.Text);
			}
				
			if (
				(comboBoxTables.Items.Count <= 0 && 
				(treeNode.Type == AuditConstStrings.Application || treeNode.Type == AuditConstStrings.Module)) ||
					treeNode.Type == AuditConstStrings.TracedTable		||
					treeNode.Type == AuditConstStrings.PauseTraceTable	||
					treeNode.Type == AuditConstStrings.NoTracedTable
				)
			{
				LoadAllTracedTable(treeNode);
				tableLabel.Text = Strings.AllTablesLabel;
			}


			if (comboBoxTables.Items.Count > 0)
			{
				if (comboBoxTables.SelectedIndex == -1)
					comboBoxTables.SelectedIndex = 0; 
			}
			else 
				DisableAllControls();
		}
		#endregion				
		
		//---------------------------------------------------------------------
		private void LoadUsersCombo()
		{
			cmbUsers.Items.Clear();

			string sqlText = @"SELECT MSD_Logins.Login FROM MSD_Logins INNER JOIN MSD_CompanyLogins ON 
								MSD_CompanyLogins.LoginId = MSD_Logins.LoginId
								WHERE (MSD_CompanyLogins.CompanyId = @CompanyId)";

			//mi apro la connessione sul db di sistema
			TBConnection sysConnect = new TBConnection(contextInfo.ConnectSysDB, DBMSType.SQLSERVER);
			sysConnect.Open();

			TBCommand command = new TBCommand(sqlText, sysConnect);
			command.Parameters.Add ("@CompanyId", Convert.ToInt32(contextInfo.CompanyId));
			IDataReader reader  = null;
			
			try
			{
				reader = command.ExecuteReader();
				
				while (reader.Read())
					cmbUsers.Items.Add(reader[AuditTableConsts.Login].ToString());
			
				if (cmbUsers.Items.Count > 0)
					cmbUsers.SelectedIndex = 0;

				reader.Close();
				command.Dispose();

			}
			catch (TBException err)
			{
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlText), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);
				if (reader != null)
					reader.Close();
				command.Dispose();
			}
		}

		//---------------------------------------------------------------------
		private void DisableAllControls()
		{
			dateGroupBox.Enabled = false;
			userGroupBox.Enabled = false;
			rowsGroupBox.Enabled = false;
			btnRunQuery.Enabled = false;
			btnDelete.Enabled = false;
		}
	
		//---------------------------------------------------------------------
		private void ChangeTable()
		{
			if (comboBoxTables.SelectedItem == null)
				return;

			try
			{
				tableName = comboBoxTables.SelectedItem.ToString();
				
				auditTableName = AuditTableConsts.AuditPrefix + tableName;	

				// se il db è Oracle devo controllare che il nome associato all'AuditTable, composto da AU_ + nometabella
				// non diventi superiore a 30 caratteri, altrimenti tronco la stringa
				if (contextInfo.Connection.IsOracleConnection() && auditTableName.Length > 30)
					auditTableName = auditTableName.Substring(0, 30);
			}	
			catch(Exception err)
			{
				DiagnosticViewer.ShowError(Strings.Error, err.Message, string.Empty, string.Empty, Strings.Error);
			}
		}
		#endregion
		
		#region Funzioni in ausilio per la composizione delle query di select/delete
		//---------------------------------------------------------------------
		private string PrepareAuditQuery(bool bMakeDelete)
		{
			string cmdText = (bMakeDelete) ? GetDeleteAudit() : GetSelectAudit();
			
			cmdText	+= string.Concat
				(
				(bMakeDelete) ? " WHERE" : " AND", 
				string.Format(" {0} >= @InitData AND {0} < @FinalData ", AuditTableConsts.DateCol)
				);

			if (!cbAllUsers.Checked && cmbUsers.SelectedItem.ToString() != null && cmbUsers.SelectedItem.ToString().Length > 0)
				cmdText += string.Format(" AND {0} = @LoginName", AuditTableConsts.LoginName);

			if (chkChangeKey.Checked && chkDelete.Checked && chkInsert.Checked && chkUpdate.Checked)
				return cmdText;
			else
				cmdText	+= " AND (";

			string sOperFilter = string.Empty;			
			
			if (chkInsert.Checked)
				sOperFilter += string.Format("{0} = ", AuditTableConsts.OperationCol) + operationType.Insert.ToString("d");

			if (chkUpdate.Checked)
			{
				if (sOperFilter.Length > 0)
					sOperFilter += " OR ";
				sOperFilter += string.Format("{0} = ", AuditTableConsts.OperationCol) + operationType.Update.ToString("d");
			}

			if (chkDelete.Checked)
			{
				if (sOperFilter.Length > 0)
					sOperFilter += " OR ";
				sOperFilter += string.Format("{0} = ", AuditTableConsts.OperationCol) + operationType.Delete.ToString("d");
			}

			if (chkChangeKey.Checked)
			{
				if (sOperFilter.Length > 0)
					sOperFilter += " OR ";
				sOperFilter += string.Format("{0} = ", AuditTableConsts.OperationCol) + operationType.ChangeKey.ToString("d");
			}

			if (sOperFilter !=  string.Empty)
				sOperFilter += ")";

			cmdText += sOperFilter;

			return cmdText;
		}

		/// <summary>
		/// compongo la query di select: seleziono tutte le colonne della tabella sottoposta
		/// sotto tracciatura (AU_) e considero il solo Namespace sulla relativa tabella AUDIT_Namespaces
		/// </summary>
		//---------------------------------------------------------------------
		private string GetSelectAudit()
		{
			return string.Format(@"SELECT {0}.*, AUDIT_Namespaces.Namespace FROM {0} 
								JOIN AUDIT_Namespaces ON AUDIT_Namespaces.ID = {0}.AU_NameSpaceID", auditTableName);		
		}

		//---------------------------------------------------------------------
		private string GetDeleteAudit()
		{
			return string.Format("DELETE FROM {0}", auditTableName);	
		}
		#endregion

		#region Funzioni di formattazione dei dati nel datagrid
		//---------------------------------------------------------------------
		private string FormatOperationType(object value)
		{
			int	type = (value is decimal) ? decimal.ToInt32((decimal)value) : Convert.ToInt32(value);

			switch((operationType)type)
			{
				case operationType.Insert:
					return Strings.Insert; 
				case operationType.Update:
					return Strings.Update;
				case operationType.Delete:
					return Strings.Delete;
				case operationType.ChangeKey:
					return Strings.ChangeKey;
			}
			return Strings.UnknownOper;		
		}			
		#endregion

		#region Esecuzione della query di cancellazione
		//---------------------------------------------------------------------
		private void MakeDeleteQuery()
		{
			this.dtgQueryResult.DataSource = null;

			string queryText = PrepareAuditQuery(true);			
		
			TBCommand command = new TBCommand(queryText, contextInfo.Connection);

			SetDateParameters(command);	
			
			try
			{
				command.ExecuteNonQuery();
			}
			catch (TBException err)
			{
				DiagnosticViewer.ShowError(Strings.SqlQueryError + " ", err.Message, err.Procedure, err.Number.ToString(), Strings.Error);
			}
			finally
			{
				command.Dispose();	
				btnDelete.Enabled = false;
			}
		}

		//----------------------------------------------------------------------
		private void SetDateParameters(TBCommand command)
		{
			DateTime toDate = dtpTo.Value;
			
			if (toDate.Hour == 0 && toDate.Minute == 0 && toDate.Second == 0)
			{
				toDate = toDate.AddHours(23);
				toDate = toDate.AddMinutes(59);
				toDate = toDate.AddSeconds(59);
			}
			
			command.Parameters.Add("@InitData",		dtpFrom.Value);
			command.Parameters.Add("@FinalData",	toDate);
		
			if (!cbAllUsers.Checked && cmbUsers.SelectedItem.ToString() != null && cmbUsers.SelectedItem.ToString().Length > 0)
				command.Parameters.Add("@LoginName", cmbUsers.SelectedItem.ToString()); 
		}
		#endregion 
		
		#region Esecuzione della query di selezione
		/// <summary>
		/// effettuo la relazione tra la tabella di auditing e quella applicativa
		/// considerando i segmenti della chiave primaria della prima
		/// </summary>
		/// <param name="dsGrid">dataset in cui inserire la relazione</param>
		//----------------------------------------------------------------------
		private void MakeRelation(DataSet dsGrid)
		{
			CatalogTableEntry catalogEntry = catalogInfo.GetTableEntry(tableName);

			if (catalogEntry == null)
				return; 
			
			dsGrid.Relations.Clear();
						
			StringCollection primaryKeys = new StringCollection();
			catalogEntry.GetPrimaryKeys(ref primaryKeys);

			// mi costruisco gli array necessari per creare la relazione	
			DataColumn[] primaryKeysApp = new DataColumn[primaryKeys.Count];
			DataColumn[] primaryKeysAudit = new DataColumn[primaryKeys.Count];
			
			for (int i = 0; i < primaryKeys.Count; i++)
			{
				primaryKeysApp[i]  = dsGrid.Tables[AuditConstStrings.AppTable].Columns[primaryKeys[i]];
				primaryKeysAudit[i]= dsGrid.Tables[AuditConstStrings.AuditTable].Columns[primaryKeys[i]];
			}				
							
			dsGrid.Relations.Add(auditTableName, primaryKeysAudit, primaryKeysApp);					
		}	
		
		//---------------------------------------------------------------------
		private void MakeSelectQuery()
		{
			this.dtgQueryResult.DataSource = null;
		
			if (!(chkChangeKey.Checked || chkDelete.Checked || chkInsert.Checked || chkUpdate.Checked))
			{
				DiagnosticViewer.ShowInformation(Strings.NoOperationFilter, Strings.FilterTitle);
				return;
			}
		
			string cmdAuditText = PrepareAuditQuery(false);			
			
			TBDataAdapter sqlAppAdp = null;
			TBDataAdapter sqlAuditAdp = null;

			try
			{
				sqlAuditAdp = new TBDataAdapter(cmdAuditText , contextInfo.Connection);
				
				SetDateParameters(sqlAuditAdp.SelectCommand);

				DataTable auditTable = new DataTable();
				sqlAuditAdp.Fill(auditTable);
				sqlAuditAdp.Dispose();

				DataTable dataToDisplay = new DataTable(AuditConstStrings.AuditTable);
				CatalogTableEntry tableEntry = catalogInfo.GetTableEntry(tableName);
				if (tableEntry != null && tableEntry.ColumnsInfo == null)
					tableEntry.LoadColumnsInfo(contextInfo.Connection, true);

				//per prima cosa inserisco i dati relativi alla data, utente, namespace, operazione decodificata
				// ovvero i campi non dipendenti dalla tabella posta sotto tracciatura
				dataToDisplay.Columns.Add(AuditTableConsts.DateCol,			Type.GetType("System.DateTime"));
				dataToDisplay.Columns.Add(AuditTableConsts.Login,			Type.GetType("System.String"));
				dataToDisplay.Columns.Add(AuditTableConsts.NamespaceCol,	Type.GetType("System.String"));
				dataToDisplay.Columns.Add(AuditTableConsts.OperationDecode, Type.GetType("System.String"));	
				
				int nStartCol = 4; //mi permette di individuare l'indice della colonna da cui iniziano
				
				// i campi dipendenti dalla tabella messa sotto tracciatura
				dataGridStyle.RemoveNoFixedColumn();
			
				string colTitle = string.Empty;
				int colLength = 0;
				
				//devo aggiungere i campi che dipendono dalla tabella messa sotto tracciatura
				foreach (DataColumn column in auditTable.Columns)
				{
					if (
						string.Compare(column.ColumnName, AuditTableConsts.LoginName, true, CultureInfo.InvariantCulture) == 0		||
						string.Compare(column.ColumnName, AuditTableConsts.NamespaceIDCol, true, CultureInfo.InvariantCulture) == 0 ||
						string.Compare(column.ColumnName, AuditTableConsts.IDCol, true, CultureInfo.InvariantCulture) == 0			||
						string.Compare(column.ColumnName, AuditTableConsts.OperationCol, true, CultureInfo.InvariantCulture) == 0	||
						string.Compare(column.ColumnName, AuditTableConsts.DateCol, true, CultureInfo.InvariantCulture) == 0		||
						string.Compare(column.ColumnName, AuditTableConsts.NamespaceCol, true, CultureInfo.InvariantCulture) == 0 	||
						string.Compare(column.ColumnName, AuditTableConsts.AuIdCol, true, CultureInfo.InvariantCulture) == 0 
						)
						continue;
					
					//aggiungo la colonna al datatable e creo lo stile da associarli nel datagrid
					dataToDisplay.Columns.Add(column.ColumnName, column.DataType);
					colLength = (tableEntry != null) ? tableEntry.GetColumnLength(column.ColumnName) : 0;
					colTitle = 	DatabaseLocalizer.GetLocalizedDescription(column.ColumnName, tableName);
					colLength = (colLength < colTitle.Length) ? colTitle.Length : 64;
					dataGridStyle.AddColumnStyle(column.ColumnName, colTitle, colLength);
				}
			
				//inserisco i dati in ogni riga
				foreach(DataRow row in auditTable.Rows)
				{
					DataRow data = dataToDisplay.NewRow();
					data[AuditTableConsts.DateCol]			= (DateTime)row[AuditTableConsts.DateCol];
					data[AuditTableConsts.Login]			= (string)row[AuditTableConsts.LoginName];
					data[AuditTableConsts.NamespaceCol]		= (string)((row[AuditTableConsts.NamespaceCol] == System.DBNull.Value) ? "" : row[AuditTableConsts.NamespaceCol]);
					data[AuditTableConsts.OperationDecode]	= FormatOperationType(row[AuditTableConsts.OperationCol]);

					for (int nCol = nStartCol; nCol < dataToDisplay.Columns.Count; nCol++)
						data[nCol] = row[dataToDisplay.Columns[nCol].ColumnName];

					dataToDisplay.Rows.Add(data);
				}

				DataView aDataView		= new DataView(dataToDisplay);	
				aDataView.AllowNew		= false;
				aDataView.AllowDelete	= false;
				aDataView.AllowEdit		= false;
				
				dtgQueryResult.DataSource = aDataView;
				this.dtgQueryResult.TableStyles.Add(dataGridStyle);
							
				/*sqlAppAdp = new SqlDataAdapter( "SELECT  * FROM " + tableName , contextInfo.Connection );
				sqlAppAdp.Fill(dsGrid, AuditConstStrings.AppTable);
				// effettuo la relazione tra la tabella di auditing e quella applicativa
				// considerando i segmenti della chiave primaria della prima
				MakeRelation(dsGrid);*/
				btnDelete.Enabled = (aDataView.Table.Rows.Count > 0);
			}
			catch (TBException err)
			{
				if (sqlAuditAdp != null) sqlAuditAdp.Dispose();
				if (sqlAppAdp != null) sqlAppAdp.Dispose() ;
				DiagnosticViewer.ShowError(Strings.SqlQueryError + " ", err.Message, err.Procedure, err.Number.ToString(), Strings.Error);
			}		
		}		
		#endregion

		#region Funzioni relative al change dei controls
		//---------------------------------------------------------------------
		private void cbAllUsers_CheckedChanged(object sender, System.EventArgs e)
		{
			bool bCheck = ((CheckBox)sender).Checked;
			cmbUsers.Enabled = !bCheck;
		}	
		
		//---------------------------------------------------------------------
		private void comboBoxTables_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ChangeTable();
		}

		//---------------------------------------------------------------------
		private void dtpFrom_ValueChanged(object sender, System.EventArgs e)
		{
			dtpTo.MinDate = ((DateTimePicker)sender).Value;
		}

		//---------------------------------------------------------------------
		private void dtpTo_ValueChanged(object sender, System.EventArgs e)
		{
			dtpFrom.MaxDate = ((DateTimePicker)sender).Value;		
		}
		#endregion
		
		#region Eventi sul bottone di estrazione dati		
		//---------------------------------------------------------------------
		private void btnRunQuery_Click(object sender, System.EventArgs e)
		{
			MakeSelectQuery();
		}

		//---------------------------------------------------------------------
		private void btnRunQuery_MouseHover(object sender, EventArgs e)
		{
			LanguageToolTip.SetToolTip((Button)sender, Strings.QueryBtnToolTip);
		}
		#endregion

		#region Eventi sul bottone di cancellazione dati
		//---------------------------------------------------------------------
		private void btnDelete_Click(object sender, System.EventArgs e)
		{
			if (DiagnosticViewer.ShowQuestion(Strings.DeleteConfirm, Strings.ConfirmationTitle) == DialogResult.No)
				return;
			
			MakeDeleteQuery();
		}

		//---------------------------------------------------------------------
		private void btnDelete_MouseHover(object sender, EventArgs e)
		{
			LanguageToolTip.SetToolTip((Button)sender, Strings.DeleteBtnToolTip);
		}

		#endregion				
	}

	#region class QueryResultDataGridStyle
	/// <summary>
	/// gestione dello stile del datagrid di visualizzazione dati
	/// </summary>
	//=========================================================================
	public class QueryResultDataGridStyle : System.Windows.Forms.DataGridTableStyle
	{
		//data member
		int intAvgCharWidth = 0;
		const int nFixedCol = 4;

		//-------------------------------------------------------------------------
		public QueryResultDataGridStyle(DataGrid datagrid)
		{
			AllowSorting = true;
			MappingName = AuditConstStrings.AuditTable;
			intAvgCharWidth =(int)(System.Drawing.Graphics.FromHwnd(datagrid.Handle).MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZ", datagrid.Font).Width/26);
			this.ForeColor = datagrid.ForeColor;
			this.GridLineColor = datagrid.GridLineColor;
			this.GridLineStyle = datagrid.GridLineStyle;
			this.HeaderFont = datagrid.HeaderFont;
			this.HeaderBackColor = datagrid.HeaderBackColor;
			this.HeaderForeColor = datagrid.HeaderForeColor;
			this.LinkColor = datagrid.LinkColor;
			this.SelectionBackColor = datagrid.SelectionBackColor;
			this.SelectionForeColor  = datagrid.SelectionForeColor;

			AddFixedColumn();
		}
			
		//-------------------------------------------------------------------------
		public void AddColumnStyle(string mapping, string title, int width)
		{
			DataGridTextBoxColumn textColumn = new DataGridTextBoxColumn();
			textColumn.MappingName = mapping;
			textColumn.HeaderText = title;
			textColumn.Alignment = HorizontalAlignment.Left;
			textColumn.Width = intAvgCharWidth * width;
			textColumn.NullText = string.Empty;
			textColumn.ReadOnly = true;
			GridColumnStyles.Add(textColumn);
		}

		/// <summary>
		/// inserisco le colonne fisse solo una volta. Esse non variano a seconda
		/// della tabella di auditing su cui si effettua la query
		/// </summary>
		//-------------------------------------------------------------------------
		public void AddFixedColumn()
		{
			AddColumnStyle(AuditTableConsts.DateCol,			Strings.OperDataTitle, Strings.OperDataTitle.Length);
			AddColumnStyle(AuditTableConsts.Login,				Strings.UserTitle, 30);
			AddColumnStyle(AuditTableConsts.NamespaceCol,		Strings.NamespaceTitle, 30);
			AddColumnStyle(AuditTableConsts.OperationDecode,	Strings.OperTypeTitle, Strings.OperTypeTitle.Length);
		}

		/// <summary>
		/// rimuovo solo le colonne non fisse che dipendono dalla tabella di auditing
		/// su cui si effettua la query
		/// </summary>
		//-------------------------------------------------------------------------
		public void RemoveNoFixedColumn()
		{
			for (int nCount = GridColumnStyles.Count-1; nCount >= nFixedCol; nCount--)
				GridColumnStyles.RemoveAt(nCount);
		}
	}
	#endregion
}