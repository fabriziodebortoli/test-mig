using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Microarea.Console.Core.DBLibrary;
using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.AuditingAdmin
{
	/// <summary>
	/// Visualizzazione delle tabelle sottoposte a tracciatura e relative colonne
	/// </summary>
	//=========================================================================
	public partial class ApplicationsTree : PlugInsForm
	{
		#region DataMember
		static public int TracedFilter = 0x0001;
		static public int StopTraceFilter = 0x0010;
		static public int NoTracedFilter = 0x0100;

		public enum EnumOperations { Insert, Stop, Pause, Restart }

		private ApplicationDBStructureInfo	appDBStructInfo	= null;
		private ContextInfo					contextInfo		= null;
		private CatalogInfo					catalogInfo		= null;
		private BrandLoader					brandLoader		= null;
		
		private int tracedImageIndex		= -1;
		private int stopTracedImageIndex	= -1;
		private int tableImageIndex			= -1;
		private int applicationImageIndex	= -1;
		private int moduleImageIndex		= -1;

		private DataTable		operationDT		= new DataTable(AuditConstStrings.Operation);
		private ArrayList		fixedColumns	= new ArrayList();
		private	TableManager	tableManager	= null;
		private DataTable		tracedDataTable = new DataTable();			

		private int	filterMap = TracedFilter | StopTraceFilter | NoTracedFilter;

		private string currentTableName = string.Empty;
		private bool columnsChecked = false;

		// mi serve per sapere se la struttura delle tabelle/colonne e' potenzialmente cambiata
		private bool auditStructureIsChanged = false;

		public delegate void UpdateMatViewsDelegate();
		public event UpdateMatViewsDelegate UpdateMatViews;

		// Eventi per la gestione della progressbar in fase di caricamento della struttura di database
		public event LoadDatabaseInfoEventHandler LoadDatabaseInfoStarted;
		public event LoadDatabaseInfoEventHandler LoadDatabaseInfoModChanged;
		public event LoadDatabaseInfoEventHandler LoadDatabaseInfoEnded;
		#endregion

		public bool AuditStructureIsChanged { get { return auditStructureIsChanged; } }

		#region Costruttore
		//---------------------------------------------------------------------
		public ApplicationsTree(ContextInfo context, CatalogInfo catalog, BrandLoader brandLoader)
		{
			InitializeComponent();
			State = StateEnums.Waiting;

			contextInfo		= context;
			catalogInfo		= catalog;
			this.brandLoader= brandLoader;
			
			//inizializzazione dei controls 
			lblTitle.Text = string.Format(lblTitle.Text, context.CompanyName);
			btnPauseTrace.Tag = EnumOperations.Pause;
			btnStopTrace.Tag  = EnumOperations.Stop; 
			btnStartTrace.Tag = EnumOperations.Insert;	

			LoadImages();	 //carica le immagini per la imagelist, la leggenda e il bottone			
			CreateListTableColumns();			
		}
		#endregion

		#region Inizializzazione di tutte le informazioni legate al database
		//---------------------------------------------------------------------
		public void InitDatabaseInfo()
		{
			StringCollection supportList = new StringCollection();
			StringCollection AppList	= new StringCollection();

			// prima guardo le AddOn di TaskBuilder
			contextInfo.PathFinder.GetApplicationsList(ApplicationType.TaskBuilder, out supportList);
			AppList = supportList;
			// poi guardo le AddOn di TaskBuilderApplications
			contextInfo.PathFinder.GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);

			for (int i = 0; i < supportList.Count; i++)
				AppList.Add(supportList[i]);

			appDBStructInfo = new ApplicationDBStructureInfo(contextInfo.PathFinder, brandLoader);
			appDBStructInfo.LoadDatabaseInfoStarted		+= new LoadDatabaseInfoEventHandler(OnLoadDatabaseInfoStarted);
			appDBStructInfo.LoadDatabaseInfoModChanged	+= new LoadDatabaseInfoEventHandler(OnLoadDatabaseInfoModChanged);
			appDBStructInfo.LoadDatabaseInfoEnded		+= new LoadDatabaseInfoEventHandler(OnLoadDatabaseInfoEnded);

			//leggo solo le informazioni relative all'application\module. Le tabelle le leggo ondemand
			appDBStructInfo.InitApplicationList(AppList);			
			LoadTracedKeys(AppList);

			//é il gestore dello schema delle tabelle di tracciatura
			tableManager = new TableManager(contextInfo, catalogInfo, fixedColumns);
			
			//caricamento delle tabelle nell' albero
			LoadAvailableTables();
		}
		#endregion

		#region Gestione fixedkey da spostare nella classe FixedColumnsObject
		/// <summary>
		/// Funzione che mi legge le UniversalKeys
		/// cercando il dbts.xml x ogni applicazione/module/ModuleObjects/Description
		/// (stessa folder dove cerco i report.xml x il security)
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadTracedKeys(StringCollection appList)
		{
			ApplicationInfo applicationInfo = null;
			DbtsObjects dbtsObjects = null;
			
			foreach (string appName in appList)
			{
				applicationInfo = (ApplicationInfo)contextInfo.PathFinder.GetApplicationInfoByName(appName);
				
				foreach (ModuleInfo moduleInfo in applicationInfo.Modules)
				{
					if (moduleInfo.Documents == null) 
						continue;
					
					foreach(DocumentInfo documentInfo in moduleInfo.Documents)
					{
						if (documentInfo == null) 
							continue;
						
						dbtsObjects = new DbtsObjects(contextInfo.PathFinder.GetDbtsPath(documentInfo.NameSpace));
						if (dbtsObjects.ParseFixedColumns())
						{
							if (dbtsObjects.FixedColumns != null && !ExistTracedColumn(dbtsObjects.FixedColumns))
								fixedColumns.Add(dbtsObjects.FixedColumns);
						}
					}
				}
			}
		}

		//---------------------------------------------------------------------
		public bool ExistTracedColumn(FixedColumnsObject aFixedColumnsObject)
		{
			foreach (FixedColumnsObject fixedCol in fixedColumns)
			{
				if (string.Compare(fixedCol.TableName, aFixedColumnsObject.TableName) == 0)
				{
					foreach (string col in aFixedColumnsObject.TracedColumns)
					{
						if (fixedCol.ExistTracedColumn(col))
							return true;
					}
				}
			}
			return false;
		}
		#endregion

		#region Metodi per la gestione degli eventi legati al caricamento delle informazioni di database
		//---------------------------------------------------------------------
		private void OnLoadDatabaseInfoStarted(object sender, int nCount)
		{
			if (LoadDatabaseInfoStarted != null)
				LoadDatabaseInfoStarted(sender, nCount);
		}
			
		//---------------------------------------------------------------------
		private void OnLoadDatabaseInfoModChanged(object sender, int nCount)	
		{
			if (LoadDatabaseInfoModChanged != null)
				LoadDatabaseInfoModChanged(sender, nCount);
		}	
		
		//---------------------------------------------------------------------
		private void OnLoadDatabaseInfoEnded(object sender, int nCount)
		{
			if (LoadDatabaseInfoEnded != null)
				LoadDatabaseInfoEnded(sender, nCount);
		}
		#endregion

		#region Funzione di inizializzazione dei control e di caricamento albero
		//-------------------------------------------------------------------------
		private void CreateListTableColumns()
		{
			//preparo la list-view
			lstColumns.Clear();
			lstColumns.View				  = View.Details;
			lstColumns.CheckBoxes		  = true;
			lstColumns.AllowColumnReorder = true;
			lstColumns.Activation		  = ItemActivation.OneClick;

			lstColumns.Columns.Add(Strings.ColumnName,	230, HorizontalAlignment.Left);
			lstColumns.Columns.Add(Strings.Type,		130, HorizontalAlignment.Left);			
		}

		//-------------------------------------------------------------------------
		private void SetTypeAndBitmap(PlugInTreeNode tableNode, string type)
		{
			tableNode.Type = type;
			if (type == AuditConstStrings.PauseTraceTable) 
			{
				tableNode.ImageIndex			= stopTracedImageIndex;
				tableNode.SelectedImageIndex	= stopTracedImageIndex;
				return;
			}
			if (type == AuditConstStrings.TracedTable)
			{
				tableNode.ImageIndex			= tracedImageIndex;
				tableNode.SelectedImageIndex	= tracedImageIndex;
				return;
			}
			if (type == AuditConstStrings.NoTracedTable)
			{
				tableNode.ImageIndex			= tableImageIndex;
				tableNode.SelectedImageIndex	= tableImageIndex;
				return;
			}			

			tableNode.ImageIndex			= tableImageIndex;
			tableNode.SelectedImageIndex	= tableImageIndex;
			tableNode.ForeColor				= Color.Red;
		}

		//-------------------------------------------------------------------------
		private bool ApplyFilterRules(PlugInTreeNode tableNode)
		{
			if (tableNode.Type == AuditConstStrings.TracedTable)
				return (filterMap & TracedFilter) == TracedFilter;

			if (tableNode.Type == AuditConstStrings.NoTracedTable || tableNode.Type == AuditConstStrings.NoExistTable)
				return (filterMap & NoTracedFilter) == NoTracedFilter;

			if (tableNode.Type == AuditConstStrings.PauseTraceTable)
				return (filterMap & StopTraceFilter) == StopTraceFilter;
			return false;
		}

		//-------------------------------------------------------------------------
		private bool AddTableNode(PlugInTreeNode tableNode)
		{
			CatalogEntry entry = ((CatalogEntry)tableNode.Tag);
			
			if (entry != null)
				foreach (DataRow row in tracedDataTable.Rows)
					if (string.Compare(row[AuditTableConsts.TableNameCol].ToString(), entry.TableName, true, CultureInfo.InvariantCulture) == 0)
					{
						SetTypeAndBitmap(tableNode, ((string)row[AuditTableConsts.StopCol] == "1") ? AuditConstStrings.PauseTraceTable : AuditConstStrings.TracedTable);
						tracedDataTable.Rows.Remove(row);
						return ApplyFilterRules(tableNode);
					}
			
			SetTypeAndBitmap(tableNode,	(entry == null) ? AuditConstStrings.NoExistTable : AuditConstStrings.NoTracedTable);
			return ApplyFilterRules(tableNode);
		}

		//-------------------------------------------------------------------------
		private void AddModuleTables(PlugInTreeNode modNode, ref ArrayList expandedNodes, PlugInTreeNode selectedNode)
		{
			CatalogEntry catEntry = null;
			PlugInTreeNode tableNode = null;	
			ModuleDBInfo modInfo = (ModuleDBInfo)modNode.Tag;

			if (modInfo == null)
				return;

			foreach (EntryDBInfo tabEntry in modInfo.TablesList)
			{
				catEntry = catalogInfo.GetTableEntry(tabEntry.Name);
				tableNode = new PlugInTreeNode(catEntry.TableName);
				tableNode.Tag  = catEntry;
				if (AddTableNode(tableNode))
				{
					modNode.Nodes.Add(tableNode);	
					if (expandedNodes != null)
						SetOldProperties(ref expandedNodes, selectedNode, ref tableNode); 
				}
			}
		}

		//-------------------------------------------------------------------------
		private void LoadImages()
		{
			Assembly executingAssembly	= Assembly.GetExecutingAssembly();
			Assembly plugInsAssembly	= typeof(PlugIn).Assembly;

			Stream stream = executingAssembly.GetManifestResourceStream(AuditConstStrings.NamespaceAuditingAdminImage + ".TracedTable.bmp");
			tracedImageIndex = imageList.Images.Add(Image.FromStream(stream, true), Color.Magenta);
			stream = executingAssembly.GetManifestResourceStream(AuditConstStrings.NamespaceAuditingAdminImage + ".PausedTracedTable.bmp");
			stopTracedImageIndex = imageList.Images.Add(Image.FromStream(stream, true), Color.Magenta);
			stream = plugInsAssembly.GetManifestResourceStream(DatabaseLayerConsts.NamespacePlugInsImg + ".Table.bmp");
			tableImageIndex	= imageList.Images.Add(Image.FromStream(stream, true), Color.Magenta);
			stream = plugInsAssembly.GetManifestResourceStream(DatabaseLayerConsts.NamespacePlugInsImg + ".Application.bmp");
			applicationImageIndex = imageList.Images.Add(Image.FromStream(stream, true), Color.Magenta);
			stream = plugInsAssembly.GetManifestResourceStream(DatabaseLayerConsts.NamespacePlugInsImg + ".Module.bmp");
			moduleImageIndex = imageList.Images.Add(Image.FromStream(stream, true), Color.Magenta);

			trwApplications.ImageList = this.imageList;

			// legenda
			ptbTracedTables.Image = imageList.Images[tracedImageIndex];
			ptbStopTraceTables.Image = imageList.Images[stopTracedImageIndex];
			ptbTables.Image = imageList.Images[tableImageIndex];

			//immagine del bottone di query
			stream = executingAssembly.GetManifestResourceStream(AuditConstStrings.NamespaceAuditingAdminImage + ".Query.bmp");
			if (stream != null)
				btnQuery.Image = Image.FromStream(stream, true);
		}

		//---------------------------------------------------------------------
		public void LoadTracedTable()
		{
			string sqlText = string.Format("SELECT * FROM {0}", AuditTableConsts.AuditTablesTableName);
					
			TBDataAdapter adapter = null;
			tracedDataTable.Clear();
			
			try
			{
				adapter = new TBDataAdapter(sqlText, contextInfo.Connection);		
				adapter.Fill(tracedDataTable);	
			}
			catch (TBException err)
			{
				DiagnosticViewer.ShowError(string.Format(Strings.ExecuteQueryError, sqlText), err.Message, err.Procedure, err.Number.ToString(), Strings.Error);
				tracedDataTable.Clear();
			}
			finally
			{
				if (adapter != null)
					adapter.Dispose();
			}
		}

		//-------------------------------------------------------------------------
		private void GetExpandedNodes(PlugInTreeNode nodeToExpand, ref ArrayList expandedNodes)
		{
			PlugInTreeNode expandNode = null;
			//considero tutti i nodi espansi
			foreach (PlugInTreeNode node in nodeToExpand.Nodes)
			{
				if (node.IsExpanded)
				{
					expandNode = new PlugInTreeNode();
					expandNode.Type = node.Type;
					expandNode.Text = node.Text;
					expandedNodes.Add(expandNode);
				}	 
			}
		}

		//-------------------------------------------------------------------------
		private void SetOldProperties(ref ArrayList expandedNodes, PlugInTreeNode selectedNode, ref PlugInTreeNode currentNode)
		{
			if (currentNode == null )
				return;			
			
			if (trwApplications.SelectedNode == null &&
				selectedNode != null && 
				selectedNode.Type == currentNode.Type && 
				selectedNode.Text == currentNode.Text)
				trwApplications.SelectedNode = currentNode; 

			//verifico se è da espandere
			if (expandedNodes == null || 
				(currentNode.Type != AuditConstStrings.Application && currentNode.Type != AuditConstStrings.Module))
				return;

			foreach (PlugInTreeNode node in expandedNodes)
				if (node.Type == currentNode.Type && node.Text == currentNode.Text)
				{
					currentNode.Expand();
					expandedNodes.Remove(node);
					return;
				}
		}		

		//-------------------------------------------------------------------------
		private void LoadAvailableTables()
		{
			PlugInTreeNode selectedNode = null;
			ArrayList expandedNodes = null;
			PlugInTreeNode expandNode = null;
			string	selectedModule = string.Empty;
			
			//considero il nodo selezionato
			if (trwApplications.Nodes.Count >0)
			{
				selectedNode = (PlugInTreeNode)trwApplications.SelectedNode;
				//se il nodo selezionato è di tipo NoTracedTable, TracedTable, StopTraceTable mi memorizzo
				// anche il nome del modulo di appartenenza. Questo mi serve nel caso in cui la tabella non 
				// non verifiche più il criterio di filtraggio: il fuoco deve passare al modulo
				if (selectedNode != null && selectedNode.Type != AuditConstStrings.Application &&
					selectedNode.Type != AuditConstStrings.Module)
					selectedModule = selectedNode.Parent.Text;

				expandedNodes = new ArrayList();
				foreach (PlugInTreeNode node in trwApplications.Nodes)
				{
					if (node.IsExpanded)
					{
						expandNode = new PlugInTreeNode();
						expandNode.Type = node.Type;
						expandNode.Text = node.Text;
						expandedNodes.Add(expandNode);
					}	 
					GetExpandedNodes(node, ref expandedNodes);		
				}
			}				
				
			trwApplications.BeginUpdate();
			trwApplications.Nodes.Clear();

			PlugInTreeNode appNode, modNode, tableNode = null; 
			CatalogEntry catEntry = null;
			ArrayList list = null;

			LoadTracedTable();	
			
			foreach (AddOnApplicationDBInfo appDBInfo in appDBStructInfo.ApplicationDBInfoList)
			{
				appNode = new PlugInTreeNode(appDBInfo.BrandTitle);
				list	= new ArrayList();

				appNode.ImageIndex			= applicationImageIndex;
				appNode.SelectedImageIndex	= applicationImageIndex;
				appNode.Type				= AuditConstStrings.Application;

				trwApplications.Nodes.Add(appNode);
				SetOldProperties(ref expandedNodes, selectedNode, ref appNode); 

				foreach (ModuleDBInfo modInfo in appDBInfo.ModuleList)
				{
					modNode = new PlugInTreeNode(modInfo.Title);
					modNode.Tag					= modInfo;
					modNode.ImageIndex			= moduleImageIndex;
					modNode.SelectedImageIndex	= moduleImageIndex;
					modNode.Type				= AuditConstStrings.Module; 
				
					//se il nodo non è mai stato espanso (il caricamento delle tabelle viene fatto ondemand)
					if (modInfo.TablesList.Count == 0)
					{
						tableNode = new PlugInTreeNode();
						tableNode.Type = AuditConstStrings.Dummy;
						modNode.Nodes.Add(tableNode);
					}
					else						
						foreach (EntryDBInfo tabEntry in modInfo.TablesList)
						{
							catEntry = catalogInfo.GetTableEntry(tabEntry.Name);
							tableNode = new PlugInTreeNode(catEntry.TableName);
							tableNode.Tag  = catEntry;
							if (AddTableNode(tableNode))
							{
                                modNode.Nodes.Add(tableNode);	
								SetOldProperties(ref expandedNodes, selectedNode, ref tableNode); 
							}
						}

					list.Add(modNode);
					SetOldProperties(ref expandedNodes, selectedNode, ref modNode); 
				}

				IComparer comparer = new SortTreeNodeList();
				list.Sort(comparer);

				foreach (PlugInTreeNode node in list)
					trwApplications.Nodes[appDBStructInfo.ApplicationDBInfoList.IndexOf(appDBInfo)].Nodes.Add(node);
			}
			
			// se il nodo precedentemente selezionato era una tabella e non verifica i criteri di filtraggio, 
			// allora come scelgo il suo parent come nodo selezionato (è un modulo ed esiste di sicuro)
			if (trwApplications.SelectedNode == null && selectedModule.Length > 0)
			{
				int nNode = 0;
				while (nNode < trwApplications.Nodes.Count && trwApplications.SelectedNode == null)
				{
					foreach (PlugInTreeNode node in trwApplications.Nodes[nNode].Nodes)
						if (node.Text == selectedModule)
						{
							trwApplications.SelectedNode = node;
							break;
						}
					nNode++;
				}					
			}
			trwApplications.EndUpdate();
			trwApplications.Focus();
		}
		#endregion

		#region Gestione radiobutton per la gestione dell'operazione di cambio stato
		//--------------------------------------------------------------------
		private string GetOperationDescri(EnumOperations operation)
		{
			switch (operation)
			{
				case  EnumOperations.Insert:	return Strings.InsertInAuditing;
				case  EnumOperations.Stop:		return Strings.StopAuditing;
				case  EnumOperations.Restart:	return Strings.RestartAuditing;
				case  EnumOperations.Pause:		return Strings.PauseAuditing;
			}
			return Strings.InsertInAuditing;
		}

		//--------------------------------------------------------------------
		private string GetOperationToolTip(EnumOperations operation)
		{
			switch (operation)
			{
				case  EnumOperations.Insert:	return Strings.InsertDescription;
				case  EnumOperations.Stop:		return Strings.StopDescription;
				case  EnumOperations.Restart:	return Strings.RestartDescription;
				case  EnumOperations.Pause:		return Strings.PauseDescription;
			}
			return Strings.InsertDescription;
		}

		/// <summary>
		/// modifica la combo delle operazioni a seconda dello stato della tabella scelta 
		/// </summary>
		//-----------------------------------------------------------------------
		private void UpdateStatusControls()
		{
			PlugInTreeNode tableNode = (PlugInTreeNode)trwApplications.SelectedNode;			
			if (tableNode.Type == AuditConstStrings.NoTracedTable)
			{
				this.btnStopTrace.Enabled = false;
				this.btnPauseTrace.Enabled = false;
				this.btnStartTrace.Enabled = true;
				this.btnStartTrace.Tag = EnumOperations.Insert;
				lstColumns.Items.Clear();
				btnAddColumns.Enabled = false;
                SelDeselAllButton.Enabled = false;
			}

			if (tableNode.Type == AuditConstStrings.TracedTable)
			{
				btnStopTrace.Enabled = true;
				btnStartTrace.Enabled = false; 
				btnPauseTrace.Enabled = true;
				LoadTableColumns();
				lstColumns.Enabled = true;
				btnAddColumns.Enabled = true;
				SelDeselAllButton.Enabled = true;
			}

			if (tableNode.Type == AuditConstStrings.PauseTraceTable)
			{
				btnPauseTrace.Enabled = false;
				btnStopTrace.Enabled = true; 
				btnStartTrace.Enabled = true;
				btnStartTrace.Tag = EnumOperations.Restart;	
				lstColumns.Enabled = false;
				btnAddColumns.Enabled = false;
				SelDeselAllButton.Enabled = false;
			}

			bool allSel = lstColumns.Items.Count > 0;
			foreach (ListViewItem item in lstColumns.Items)
			{
				if (item.ForeColor != Color.Orange)
					allSel = allSel && item.Checked;
			}

			SelDeselAllButton.Text = (allSel) ? Strings.DeselectAll : Strings.SelectAll;
		}
		#endregion		
		
		#region Layout Form
		//---------------------------------------------------------------------
		private void SetVisibleControls(bool visible)
		{
			gbTable.Visible				= visible;
			lstColumns.Visible			= visible;
			btnAddColumns.Visible		= visible;
			pnlInscription.Visible		= visible;
			SelDeselAllButton.Visible	= visible;
		}
			
		//---------------------------------------------------------------------
		new private void ActiveForm(object sender, DynamicEventsArgs e )
		{
			this.Enabled = true;			
		}
		#endregion

		#region Funzioni di inserimento/cancellazione sospensione/ripresa tracciatura
		//---------------------------------------------------------------------
		private bool InsertInTrace(PlugInTreeNode tableNode)
		{
			if (tableNode == null || tableNode.Text == null)
				return false;
			
			// questa funzione crea la tabella di auditing 
			// la inserisce in AUDIT_Tables e nel catalog
			if (tableManager.CreateAuditTable(tableNode.Text))
			{
				SetTypeAndBitmap(tableNode, AuditConstStrings.TracedTable);
				return true;
			}

			return false;
		}

		//---------------------------------------------------------------------
		private bool StopTrace(PlugInTreeNode tableNode)
		{
			if (tableNode == null || tableNode.Text == null)
				return false;

			//questa funzione sospende la tracciatura
			if (tableManager.StopAuditing(tableNode.Text))
			{
				SetTypeAndBitmap(tableNode, AuditConstStrings.PauseTraceTable);
				return true;
			}
			
			return false;
		}

		//---------------------------------------------------------------------
		private bool RestartTrace(PlugInTreeNode tableNode) 
		{
			if (tableNode == null || tableNode.Text == null)
				return false;

			//questa funzione riprende la tracciatura
			if (tableManager.RestartAuditing(tableNode.Text))
			{
				SetTypeAndBitmap(tableNode, AuditConstStrings.TracedTable);	
				return true;	
			}
			
			return false;
		}

		//---------------------------------------------------------------------
		private bool DeleteFromTrace(PlugInTreeNode tableNode) 
		{
			if (tableNode == null || tableNode.Text == null)
				return false;

			//questa funzione cancella la tabella di auditing 
			// la toglie dalla AUDIT_Tables e dal catalog			
			if (tableManager.DropAuditTable(tableNode.Text))
			{
				SetTypeAndBitmap(tableNode, AuditConstStrings.NoTracedTable);
				return true;	
			}
			return false;						
		}
		#endregion
		
		#region Gestione colonne della listview
		//controllo se è stato cambiato qualcosa nello stato delle colonne
		//---------------------------------------------------------------------
		private void CheckColumns()
		{
			if (columnsChecked || currentTableName.Length == 0)
				return;

			ArrayList columns  = new ArrayList();	
			GetColumnChanged(ref columns);
			
			if (columns.Count> 0 &&
				DiagnosticViewer.ShowQuestion(string.Format(Strings.ChangeConfirmation, currentTableName), Strings.ConfirmationTitle) == DialogResult.Yes)
				tableManager.AlterAuditTable(AuditTableConsts.AuditPrefix + currentTableName, columns);
			
			columnsChecked = true;						
		}
		
		//---------------------------------------------------------------------
		private void GetColumnChanged(ref ArrayList columns)
		{	
			if (columnsChecked || currentTableName == string.Empty)
				return;

			string auditTableName = AuditTableConsts.AuditPrefix + currentTableName;

			// se il db è Oracle devo controllare che il nome associato all'AuditTable, composto da AU_ + nometabella
			// non diventi superiore a 30 caratteri, altrimenti tronco la stringa
			if (contextInfo.Connection.IsOracleConnection() && auditTableName.Length > 30)
				auditTableName = auditTableName.Substring(0, 30);

			CatalogColumn col = null;			
			CatalogTableEntry auditEntry = catalogInfo.GetTableEntry(auditTableName);

			foreach (ListViewItem item in lstColumns.Items)
			{
				col = auditEntry.GetColumnInfo(item.Text);
				if (col == null && item.Checked == true)
					columns.Add(new TracedColumn(item.Text, item.SubItems[1].Text, true));
				else 
					if (col != null && item.Checked == false)
						columns.Add(new TracedColumn(item.Text, item.SubItems[1].Text, false));
			}
			
			columnsChecked = true;
		}
		
		//da inserire nella classe di fixedkey
		//---------------------------------------------------------------------
		private bool IsFixedKey(string columnName, string tableName)
		{
			foreach (FixedColumnsObject fixedColumnsObject in fixedColumns)
			{
				if (tableName != fixedColumnsObject.TableName) 
					continue;

				foreach (string col in fixedColumnsObject.TracedColumns)
					if (string.Compare(col, columnName, true, CultureInfo.InvariantCulture) == 0)
						return true;
			}
			return false;					
		}
		
		//---------------------------------------------------------------------
		private void AddColumInListView(CatalogColumn column, CatalogTableEntry tableEntry, CatalogTableEntry auditEntry)
		{
			if (currentTableName.Length == 0)
				return;

			// se il nome della colonna è TBCreated, TBModified, TBCreatedID, TBModifiedID, TBGuid non procedo (non vanno visualizzate!)
			if (string.Compare(column.Name, DatabaseLayerConsts.TBModifiedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
				string.Compare(column.Name, DatabaseLayerConsts.TBCreatedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
				string.Compare(column.Name, DatabaseLayerConsts.TBModifiedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
				string.Compare(column.Name, DatabaseLayerConsts.TBCreatedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(column.Name, DatabaseLayerConsts.TBGuidColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0)
				return; 

			string columnName = column.Name;
	
			ListViewItem listViewItem = new ListViewItem();
			listViewItem.Text = columnName; //nome colonna
			listViewItem.SubItems.Add(column.GetCompleteDBType()); //tipo con eventuale dimensione						
			
			// se é una chiave primaria allora inserisco la colonna obbligatoria (colore rosso)
			if (column.IsKey || IsFixedKey(columnName, currentTableName))
			{
				listViewItem.Checked = true;
				listViewItem.ForeColor = Color.Orange;	
			}
			else //verifico se é sotto tracciatura
			{
				bool tracedColumn = (auditEntry.GetColumnInfo(columnName) != null);
				listViewItem.Checked = tracedColumn;
				listViewItem.ForeColor = (tracedColumn) ? Color.Green : Color.Black;				
			}

			// inserisce l'Item in modo ordinato
			OrderColumnsList(listViewItem);			
		}
		
		//---------------------------------------------------------------------
		private void LoadTableColumns()
		{
			lstColumns.Items.Clear();
			if (currentTableName.Length == 0)
				return;
		
			string auditTableName = AuditTableConsts.AuditPrefix + currentTableName;

			// se il db è Oracle devo controllare che il nome associato all'AuditTable, composto da AU_ + nometabella
			// non diventi superiore a 30 caratteri, altrimenti tronco la stringa
			if (contextInfo.Connection.IsOracleConnection() && auditTableName.Length > 30)
				auditTableName = auditTableName.Substring(0, 30);

			CatalogTableEntry auditEntry = catalogInfo.GetTableEntry(auditTableName);
			if (auditEntry == null)
				return;
			if (auditEntry.ColumnsInfo == null)
				auditEntry.LoadColumnsInfo(contextInfo.Connection, true);
			
			CatalogTableEntry tableEntry = catalogInfo.GetTableEntry(currentTableName);
			if (tableEntry == null)
				return;
			if (tableEntry.ColumnsInfo == null)
				tableEntry.LoadColumnsInfo(contextInfo.Connection, true);
		
			//X ogni colonna del Catalog della tabella originale
			if (tableEntry.ColumnsInfo == null)
				return;

			foreach (CatalogColumn row in tableEntry.ColumnsInfo)
				AddColumInListView(row, tableEntry, auditEntry);
		}

		//---------------------------------------------------------------------
		private void OrderColumnsList(ListViewItem newItem)
		{
			if (newItem.ForeColor == Color.Orange)
			{
				foreach(ListViewItem item in lstColumns.Items)
				{
					if (item.ForeColor != Color.Orange)
					{
						lstColumns.Items.Insert(item.Index, newItem);
						return;
					}
				}
			}

			if (newItem.ForeColor == Color.Green)
			{
				foreach(ListViewItem item in lstColumns.Items)
				{
					if (item.ForeColor != Color.Orange)
					{
						lstColumns.Items.Insert(item.Index, newItem);
						return;
					}
				}			
			}
			lstColumns.Items.Add(newItem);
		}
		#endregion

		#region Eventi delle Voci di Menu di un gruppo di tabelle (tramite toolstripmenu)
		///<summary>
		/// click sul toolstripmenu per avviare la tracciatura
		///</summary>
		//---------------------------------------------------------------------
		private void StartToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OnClickInsertInAuditingAllTables(sender, e);
		}

		///<summary>
		/// click sul toolstripmenu per ripristinare la tracciatura
		///</summary>
		//---------------------------------------------------------------------
		private void ResumeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OnClickRestartAuditingAllTables(sender, e);
		}

		///<summary>
		/// click sul toolstripmenu per mettere in pausa la tracciatura
		///</summary>
		//---------------------------------------------------------------------
		private void PauseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OnClickStopAuditingAllTables(sender, e);
		}

		///<summary>
		/// click sul toolstripmenu per stoppare la tracciatura
		///</summary>
		//---------------------------------------------------------------------
		private void StopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OnClickDeleteAuditingAllTables(sender, e);
		}

		///<summary>
		/// Dato il nodo selezionato ritorna un PlugInTreeNode arricchito delle informazioni
		/// di tabella dei suoi sottonodi (perche' sono caricati on-demand)
		///</summary>
		//---------------------------------------------------------------------
		private PlugInTreeNode GetGroupInfoForSelectedNode()
		{
			if (trwApplications.SelectedNode == null)
				return null;

			PlugInTreeNode pluginTreeNode = (PlugInTreeNode)trwApplications.SelectedNode;

			if (pluginTreeNode == null)
				return null;

			// nel caso in cui non avessi espanso il nodo non riuscirei a visualizzare nessuna tabella
			// (perche' il caricamento avviene on-demand), pertanto forzo tale operazione
			if (pluginTreeNode.Type == AuditConstStrings.Module)
				LoadTableNodes((PlugInTreeNode)trwApplications.SelectedNode);

			if (pluginTreeNode.Type == AuditConstStrings.Application)
			{
				// per ogni modulo carico le sue tabelle
				foreach (PlugInTreeNode moduleNode in pluginTreeNode.Nodes)
					LoadTableNodes(moduleNode);
			}

			return pluginTreeNode;
		}

		/// <summary>
		/// inserisco sotto tracciatura un gruppo di tabelle
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickInsertInAuditingAllTables(object sender, System.EventArgs e)
		{
			PlugInTreeNode pluginTreeNode = GetGroupInfoForSelectedNode();
			if (pluginTreeNode == null)
				return;

			// mostro la form con la tabella per le selezioni multiple
			SelectedTables selectedTables = new SelectedTables(this);
			selectedTables.OnAfterClickOkButton += new SelectedTables.AfterClickOkButton(SwitchOperation);
			selectedTables.OnAfterClickCloseButton += new SelectedTables.AfterClickCloseButton(ActiveForm);
			this.Enabled = false;
			selectedTables.ShowTables(pluginTreeNode, EnumOperations.Insert);
			enumOperations = EnumOperations.Insert;
		}

		/// <summary>
		/// sospendo dalla tracciatura un gruppo di tabelle
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickStopAuditingAllTables(object sender, System.EventArgs e)
		{
			PlugInTreeNode pluginTreeNode = GetGroupInfoForSelectedNode();
			if (pluginTreeNode == null)
				return;

			SelectedTables selectedTables = new SelectedTables(this);
			selectedTables.OnAfterClickOkButton += new SelectedTables.AfterClickOkButton(SwitchOperation);
			selectedTables.OnAfterClickCloseButton += new SelectedTables.AfterClickCloseButton(ActiveForm);
			this.Enabled = false;
			selectedTables.ShowTables(pluginTreeNode, EnumOperations.Pause);
			enumOperations = EnumOperations.Pause;
		}

		/// <summary>
		/// effettuo il restart della  tracciatura un gruppo di tabelle
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickRestartAuditingAllTables(object sender, System.EventArgs e)
		{
			PlugInTreeNode pluginTreeNode = GetGroupInfoForSelectedNode();
			if (pluginTreeNode == null)
				return;

			SelectedTables selectedTables = new SelectedTables(this);
			selectedTables.OnAfterClickOkButton += new SelectedTables.AfterClickOkButton(SwitchOperation);
			selectedTables.OnAfterClickCloseButton += new SelectedTables.AfterClickCloseButton(ActiveForm);
			this.Enabled = false;
			selectedTables.ShowTables(pluginTreeNode, EnumOperations.Restart);
			enumOperations = EnumOperations.Restart;
		}
		
		/// <summary>
		/// elimino dalla tracciatura un gruppo di tabelle
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickDeleteAuditingAllTables(object sender, System.EventArgs e)
		{
			PlugInTreeNode pluginTreeNode = GetGroupInfoForSelectedNode();
			if (pluginTreeNode == null)
				return;

			SelectedTables selectedTables = new SelectedTables(this);
			selectedTables.OnAfterClickOkButton += new SelectedTables.AfterClickOkButton(SwitchOperation);
			selectedTables.OnAfterClickCloseButton += new SelectedTables.AfterClickCloseButton(ActiveForm);
			this.Enabled = false;
			selectedTables.ShowTables(pluginTreeNode, EnumOperations.Stop);
			enumOperations = EnumOperations.Stop;
		}

		//---------------------------------------------------------------------
		private void SwitchOperation(object sender, DynamicEventsArgs e )
		{
			Enabled = true;
			
			//mi viene restituito un array contenente le sole tabelle selezionate
			ArrayList checkedTableNodes =(ArrayList)e.DataArgument;

			foreach(PlugInTreeNode tableNode in checkedTableNodes)
			{
				switch (enumOperations)
				{
					case EnumOperations.Insert:
						InsertInTrace(tableNode); 				
						break;
					case EnumOperations.Pause:
						StopTrace(tableNode); 
						break;
					case EnumOperations.Restart:
						RestartTrace(tableNode);
						break;
					case EnumOperations.Stop:
						DeleteFromTrace(tableNode); 
						break;
				}
			}
		}
		#endregion

		#region Eventi sul tree
		//-------------------------------------------------------------------------------
		private void trwApplications_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			PlugInTreeNode plugInTreeNode = (PlugInTreeNode)(((TreeView)sender).SelectedNode);
			
			CheckColumns();

			EventArgs arg = new EventArgs();
			if (btnQuery.Enabled == false)
				btnQuery.Enabled = true; //abilito il pulsante di query che nasce disabilitato
	
			if (plugInTreeNode.Type == AuditConstStrings.NoExistTable	||
				plugInTreeNode.Type == AuditConstStrings.Module			||
				plugInTreeNode.Type == AuditConstStrings.Application)
			{
				SetVisibleControls(false);
				currentTableName = string.Empty;
			}
			else
			{
				currentTableName = plugInTreeNode.Text;
				SetVisibleControls(true);
				if (plugInTreeNode.Type == AuditConstStrings.PauseTraceTable)
					LoadTableColumns();
				UpdateStatusControls();
				gbTable.Text = string.Format(Strings.TableStatusDescri, plugInTreeNode.Text);				
			}

			btnQuery.Enabled = (plugInTreeNode.Type != AuditConstStrings.NoExistTable);	

			
		}

		/// <summary>
		/// albero ondemand: dato un nodo di tipo modulo per cui non sono state ancora caricate le tabelle, 
		/// di procedere al caricamento
		/// </summary>
		//----------------------------------------------------------------------------------------------------
		public void LoadTableNodes(PlugInTreeNode node)
		{
			//il nodo non è stato ancora espanso 
			if (
				node != null &&
				node.Type == AuditConstStrings.Module && 
				node.Nodes.Count > 0 &&
                node.Nodes[0] != null &&
                node.Nodes[0] is PlugInTreeNode &&
                ((PlugInTreeNode)node.Nodes[0]).Type == AuditConstStrings.Dummy &&
				node.Parent != null
				)
			{
				node.Nodes.Clear();

				ModuleDBInfo modInfo =  (ModuleDBInfo)node.Tag;	
				if (modInfo == null)
					return;

				appDBStructInfo.LoadTablesInfo(modInfo.ApplicationMember, modInfo);

				foreach (EntryDBInfo tabEntry in modInfo.TablesList)
				{
					CatalogEntry catEntry = catalogInfo.GetTableEntry(tabEntry.Name);
					if (catEntry == null)
						continue;

					PlugInTreeNode tableNode = new PlugInTreeNode(catEntry.TableName);
					tableNode.Tag = catEntry;
					if (AddTableNode(tableNode))
						node.Nodes.Add(tableNode);						
				}
			}
		}
		
		//----------------------------------------------------------------------------------------------------
		private void trwApplications_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			PlugInTreeNode node = (PlugInTreeNode)e.Node;
			LoadTableNodes((PlugInTreeNode)e.Node);						
		}

		//---------------------------------------------------------------------
		private void trwApplications_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			PlugInTreeNode tableNode = trwApplications.GetNodeAt(e.X, e.Y) as PlugInTreeNode;

			if (tableNode == null || 
				tableNode.Type == AuditConstStrings.Application || 
				tableNode.Type == AuditConstStrings.Module)
			{
				LanguageToolTip.RemoveAll();
				return;
			}

			string tableLocalized = DatabaseLocalizer.GetLocalizedDescription(tableNode.Text, tableNode.Text);

			// se il testo del nodo è diverso dal testo localizzato e tradotto allora
			// faccio apparire il tooltip
			if (string.Compare(tableNode.Text, tableLocalized, true, CultureInfo.InvariantCulture) != 0)
			{
				if (LanguageToolTip.GetToolTip(trwApplications) != tableLocalized)
					LanguageToolTip.SetToolTip(trwApplications, tableLocalized);
			}
			else
				LanguageToolTip.RemoveAll();
		}

		//---------------------------------------------------------------------------
		private void trwApplications_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Right)
				return;

			TreeView localTree = sender as TreeView;

			if (localTree == null || localTree.Nodes == null || localTree.Nodes.Count == 0)
				return;

			localTree.SelectedNode = localTree.GetNodeAt(e.X, e.Y);

			if (localTree.SelectedNode == null)
				return;

			localTree.SelectedNode.ContextMenuStrip = TreeContextMenuStrip;

			PlugInTreeNode plugInTreeNode = (PlugInTreeNode)localTree.SelectedNode;

			// il menu lo devo visualizzare solo per le applicazioni/moduli
			if (plugInTreeNode.Type != AuditConstStrings.Module &&
				plugInTreeNode.Type != AuditConstStrings.Application)
				localTree.SelectedNode.ContextMenuStrip.Enabled = false;
			else
				localTree.SelectedNode.ContextMenuStrip.Enabled = true;
		}
		#endregion
		
		#region Eventi sulla listview delle colonne
		//---------------------------------------------------------------------
		private void btnAddColumns_Click(object sender, System.EventArgs e)
		{
			if (currentTableName.Length == 0)
				return;

			string auditTableName = AuditTableConsts.AuditPrefix + currentTableName;

			// se il db è Oracle devo controllare che il nome associato all'AuditTable, composto da AU_ + nometabella
			// non diventi superiore a 30 caratteri, altrimenti tronco la stringa
			if (contextInfo.Connection.IsOracleConnection() && auditTableName.Length > 30)
				auditTableName = auditTableName.Substring(0, 30);

			ArrayList columns = new ArrayList();	
			GetColumnChanged(ref columns);

			if (columns != null && columns.Count > 0 && tableManager.AlterAuditTable(auditTableName, columns))
				LoadTableColumns();

			columnsChecked = false;

			// mi segno che la struttura sul database potrebbe essere cambiata
			auditStructureIsChanged = true;
		}

		/// <summary>
		/// le colonne obbligatorie non possono essere unchecked
		/// </summary>
		//-----------------------------------------------------------------------------
		private void lstColumns_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			ListViewItem columnItem = this.lstColumns.Items[e.Index];
			if (columnItem == null)
				return;
			//è una colonna obbligatoria: deve essere sempre checked
			if (columnItem.ForeColor == Color.Orange && e.NewValue == CheckState.Unchecked)
				e.NewValue = CheckState.Checked;
			columnsChecked = false;
		}

		//---------------------------------------------------------------------
		private void lstColumns_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (currentTableName.Length == 0)
				return;
			
			ListViewItem item = lstColumns.GetItemAt(e.X, e.Y);
			string colName = (item != null) ? item.Text : string.Empty;

			string colLocalized = DatabaseLocalizer.GetLocalizedDescription(colName, currentTableName);

			// se il testo del nodo è diverso dal testo localizzato e tradotto allora
			// faccio apparire il tooltip
			if (string.Compare(colName, colLocalized, true, CultureInfo.InvariantCulture) != 0)
			{
				if (LanguageToolTip.GetToolTip(lstColumns) != colLocalized)
					LanguageToolTip.SetToolTip(lstColumns, colLocalized);
			}
			else
				LanguageToolTip.RemoveAll();
		}

		/// <summary>
		/// evento sul pulsante Seleziona/Deseleziona tutto
		/// </summary>
		//---------------------------------------------------------------------
		private void SelDeselAllButton_Click(object sender, System.EventArgs e)
		{
			if (SelDeselAllButton.Text == Strings.SelectAll)
			{
				foreach (ListViewItem item in lstColumns.Items)
					item.Checked = true;

				SelDeselAllButton.Text = Strings.DeselectAll;
			}
			else
			{
				if (SelDeselAllButton.Text == Strings.DeselectAll)
				{
					foreach (ListViewItem item in lstColumns.Items)
					{
						if (item.ForeColor == Color.Orange) // skippo le chiavi primarie
							continue;
						item.Checked = false;
					}

					SelDeselAllButton.Text = Strings.SelectAll;
				}
			}
			
			columnsChecked = false;
		}
		#endregion		
	
		#region Eventi sul click sui checkbox di filtraggio
		//---------------------------------------------------------------------------------
		private void FilterChanged(int filterType, bool bFilter)
		{
			CheckColumns();
			if (bFilter)
				filterMap |= filterType;
			else
				filterMap &= ~filterType;
			LoadAvailableTables();
		}

		//---------------------------------------------------------------------------------
		private void cbNoTracedFilter_CheckedChanged(object sender, System.EventArgs e)
		{
			FilterChanged(NoTracedFilter, ((CheckBox)sender).Checked);
		}

		//---------------------------------------------------------------------------------
		private void cbTracedFilter_CheckedChanged(object sender, System.EventArgs e)
		{
			FilterChanged(TracedFilter, ((CheckBox)sender).Checked);
		}
		
		//---------------------------------------------------------------------------------
		private void cbStopTraceFilter_CheckedChanged(object sender, System.EventArgs e)
		{
			FilterChanged(StopTraceFilter, ((CheckBox)sender).Checked);
		}
		#endregion
		
		#region Eventi sul cambio di stato
		//---------------------------------------------------------------------------------
		private void ChangeState(EnumOperations operation)
		{
			CheckColumns();
			PlugInTreeNode tableNode = (PlugInTreeNode)trwApplications.SelectedNode;
			
			switch (operation)
			{
				case EnumOperations.Insert:
				{
					if (InsertInTrace(tableNode)) 
						UpdateStatusControls();
					SelDeselAllButton.Text = Strings.SelectAll;
					break;
				}
				case EnumOperations.Pause:
				{
					if (StopTrace(tableNode))
						UpdateStatusControls();
					break;
				}
				case EnumOperations.Restart:
				{
					if (RestartTrace(tableNode))
						UpdateStatusControls();
					break;
				}
				case EnumOperations.Stop:
				{
					if (DeleteFromTrace(tableNode))
						UpdateStatusControls();
					break;
				}
			}

			// mi segno che la struttura sul database potrebbe essere cambiata
			auditStructureIsChanged = true; 
		}		
				
		//---------------------------------------------------------------------------------
		private void btnStartTrace_Click(object sender, System.EventArgs e)
		{
			ChangeState((EnumOperations)((Button)sender).Tag);
		}

		//---------------------------------------------------------------------------------
		private void btnPauseTrace_Click(object sender, System.EventArgs e)
		{
			ChangeState((EnumOperations)((Button)sender).Tag);
		}

		//---------------------------------------------------------------------------------
		private void btnStopTrace_Click(object sender, System.EventArgs e)
		{
			ChangeState((EnumOperations)((Button)sender).Tag);
		}

		//---------------------------------------------------------------------
		private void btState_MouseHover(object sender, EventArgs e)
		{
			LanguageToolTip.SetToolTip((Button)sender, GetOperationToolTip((EnumOperations)((Button)sender).Tag));
		}
		#endregion

		#region Gestione form di query e bottone di apertura della form di query
		//---------------------------------------------------------------------
		public void btnQuery_Click(object sender, System.EventArgs e)
		{
			CheckColumns();
			AuditingQuery auditingQuery = 
				new AuditingQuery((PlugInTreeNode)trwApplications.SelectedNode, contextInfo, catalogInfo);
			auditingQuery.ShowDialog();
		}	
		#endregion				

		#region Evento di chiusura della form	
		//---------------------------------------------------------------------
		private void ApplicationsTree_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!Visible)
			{
				// se la struttura delle tabelle/colonne e' potenzialmente cambiata, sparo un evento
				// all'ApplicationDBAdmin, in modo da dare la possibilita' di aggiornare
				// le viste materializzate (solo per ORACLE)
				if (UpdateMatViews != null)
					UpdateMatViews();

				contextInfo.CloseConnection();	
				contextInfo.UndoImpersonification();
			}
		}
		#endregion
	}

	# region Sorting TreeView
	// per ordinare alfabeticamente i nodi di tipo modulo nel treeview
	//============================================================================
	public class SortTreeNodeList : IComparer
	{
		//---------------------------------------------------------------------------
		int IComparer.Compare(Object node1, Object node2)
		{
			return (new CaseInsensitiveComparer(CultureInfo.InvariantCulture)).Compare
				(
				((PlugInTreeNode)node1).Text, 
				((PlugInTreeNode)node2).Text
				);
		}
	}
	# endregion
}