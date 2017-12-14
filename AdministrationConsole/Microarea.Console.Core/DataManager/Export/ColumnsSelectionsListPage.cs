using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microarea.Console.Core.DataManager.Common;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;
using System;
using Microarea.TaskBuilderNet.Core.StringLoader;

namespace Microarea.Console.Core.DataManager.Export
{
	//=========================================================================
	public partial class ColumnsSelectionsListPage : InteriorWizardPage
	{
		private string tableName = string.Empty;
		public	ArrayList NodeList = null;
		private ExportSelections expSelections = null;
		private Images myImages	= null;		

		//---------------------------------------------------------------------
		public ColumnsSelectionsListPage()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			expSelections = ((Common.DataManagerWizard)this.WizardManager).GetExportSelections();
			LoadAvailableTables();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			return true;
		}

		/// <summary>
		/// inizializzo le colonne da visualizzare nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InitializeListView()
		{
			myImages = new Images();

			// inizializzo imagelist del tree
			SourceTblTreeView.ImageList = myImages.ImageList;

			// inizializzo imagelist della listview
			SelectedTblListView.LargeImageList = myImages.ImageList;
			SelectedTblListView.SmallImageList = myImages.ImageList;

			SelectedTblListView.Columns.Add(string.Empty, 250, HorizontalAlignment.Left);

			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetExportBmpSmallIndex()];
		}

		# region Caricamento delle informazioni (per applicazione+modulo+tabella+colonne) nel TreeView
		/// <summary>
		/// per caricare i nomi delle tabelle e delle colonne (letti dal catalog) nel tree
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadAvailableTables()
		{
			SourceTblTreeView.BeginUpdate();
			SourceTblTreeView.Nodes.Clear();

			StringCollection keysList; // per le primary key di ogni tabella

			// array di appoggio per il sorting dei nodi del tree
			// non usare il Sorting del control xchè fa casino!!!!
			NodeList = new ArrayList(); 

			PlugInTreeNode tableNode, columnNode = null; 

			int index = -1;

			foreach (CatalogTableEntry catEntry in expSelections.Catalog.TblDBList)
			{
				tableName = catEntry.TableName;	

				// se provengo da una selezione di un sottoinsieme di tabelle, aggiungo
				// nel tree il nodo delle sole entry che risultano selezionate
				if (expSelections.AllTables || catEntry.Selected)
					tableNode = new PlugInTreeNode(tableName);
				else
					continue;

				tableNode.ImageIndex			= Images.GetTableBitmapIndex();
				tableNode.SelectedImageIndex	= Images.GetTableBitmapIndex();
				tableNode.Type					= DataManagerConsts.TableNode;

				SourceTblTreeView.Nodes.Add(tableNode);
				index++;

				//carico le informazioni relative alle colonne
				catEntry.LoadColumnsInfo(expSelections.ContextInfo.Connection, true);

				foreach (CatalogColumn col in catEntry.ColumnsInfo)
				{
					// se la colonna analizzata è già presente nelle selezionate la skippo
					if (!catEntry.SelectedColumnsList.Contains(col.Name))
					{
						// aggiungo direttamente le colonne obbligatorie
						if (string.Compare(col.Name, DatabaseLayerConsts.TBCreatedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
							string.Compare(col.Name, DatabaseLayerConsts.TBModifiedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
							string.Compare(col.Name, DatabaseLayerConsts.TBCreatedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
							string.Compare(col.Name, DatabaseLayerConsts.TBModifiedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							catEntry.AddSelectedColumn(col.Name);
							continue;
						}

						columnNode = new PlugInTreeNode(col.Name);

						// solo se la colonna non è chiave primaria la visulizzo nel tree
						if (!col.IsKey)
						{
							columnNode.ImageIndex			= Images.GetColumnBitmapIndex();
							columnNode.SelectedImageIndex	= Images.GetColumnBitmapIndex();
							columnNode.Type					= DataManagerConsts.ColumnNode;
							SourceTblTreeView.Nodes[index].Nodes.Add(columnNode);
						}
						else
						{
							// gestione chiavi primarie (vanno sempre esportate)
							keysList = new StringCollection();
							catEntry.GetPrimaryKeys(ref keysList);

							foreach (string k in keysList)
								catEntry.AddSelectedColumn(k);
						}
					}
				}

				NodeList.Add(tableNode);
			}

			// se ho almeno 2 nodi faccio il sort alfabetico dei nomi delle tabelle
			if (NodeList.Count > 1)
			{
				IComparer comparer = new DataManagerSortTreeNodeList();
				NodeList.Sort(comparer);

				SourceTblTreeView.Nodes.Clear();

				foreach (PlugInTreeNode node in NodeList)
					SourceTblTreeView.Nodes.Add(node);
			}

			SourceTblTreeView.EndUpdate();
		}

		/// <summary>
		/// da un nodo di tipo Table vado ad inserire tutte le sue colonne nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertTableInfo(string text)
		{
			CatalogTableEntry entry = expSelections.Catalog.GetTableEntry(text);
			
			if (entry != null)
				InsertTableInListView(entry);
		}

		/// <summary>
		/// inserisco il nodo di tipo Column nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertColumnInfo(string textParent, string text)
		{
			CatalogColumn col = expSelections.Catalog.GetColumnInfo(textParent, text);

			if (col != null)
				InsertColumnInListView(textParent, text);
		}
		
		/// <summary>
		/// inserisce tutte le colonne della tabella selezionata nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertTableInListView(CatalogTableEntry entry)
		{
			foreach (CatalogColumn col in entry.ColumnsInfo)
			{
				// le colonne obbligatorie non le aggiungo
				if (string.Compare(col.Name, DatabaseLayerConsts.TBCreatedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(col.Name, DatabaseLayerConsts.TBModifiedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(col.Name, DatabaseLayerConsts.TBCreatedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(col.Name, DatabaseLayerConsts.TBModifiedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0)
					continue;

				// solo se la colonna non è una chiave primaria la aggiungo alla list view
				if (!col.IsKey)
					InsertColumnInListView(entry.TableName, col.Name);
			}
		}

		/// <summary>
		/// inserisce una riga di tipo colonna nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertColumnInListView(string textParent, string colName)
		{
			CatalogTableEntry entry = expSelections.Catalog.GetTableEntry(textParent);

			SelectedTblListView.BeginUpdate();
				
			ListViewItem item = new ListViewItem();
			item.ImageIndex = Images.GetColumnBitmapIndex();
			item.Text = colName;
			item.SubItems.Add(textParent);
			entry.AddSelectedColumn(item.Text);

			// prima di inserire l'item nella listview devo controllare che non sia già presente
			bool b = false;
			foreach (ListViewItem i in SelectedTblListView.Items)
			{
				if (string.Compare(i.Text, item.Text, true, CultureInfo.InvariantCulture) == 0)
					b = true;
			}

			if (!b)
                SelectedTblListView.Items.Add(item);

			SelectedTblListView.EndUpdate();
		}
		# endregion

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			if (expSelections.WriteQuery)
				return "AddWhereClausePage";
		
			return "BaseColumnsParamPage";
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			if (expSelections.AllTables)
				return "TablesParamPage";

			return "TablesSelectionsListPage";
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerExport + "ColumnsSelectionsListPage");
			return true;
		}
		#endregion

		# region Eventi sui controls della pagina
		//---------------------------------------------------------------------
		private void ColumnsSelectionsListPage_Load(object sender, System.EventArgs e)
		{
			InitializeListView();
		}
		# endregion

		# region Eventi sui bottoni >, >>, <<
		/// <summary>
		/// evento sul click del pulsante > (quando trasferisco le entry di tabelle e/o
		/// colonna dal tree alla list view)
		/// </summary>
		//---------------------------------------------------------------------------
		private void AddButton_Click(object sender, System.EventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)SourceTblTreeView.SelectedNode;

			if (selectedNode.Index != -1)
			{
				switch (selectedNode.Type)
				{
					case DataManagerConsts.TableNode:
						InsertTableInfo(selectedNode.Text);
						selectedNode.Nodes.Clear();
						break;

					case DataManagerConsts.ColumnNode:
						InsertColumnInfo(selectedNode.Parent.Text, selectedNode.Text);
						selectedNode.Remove(); // elimino solo il nodo di tipo colonna
						break;
				}
			}
		}

		/// <summary>
		/// evento sul click del pulsante Minore (elimino gli item selezionati dalla list view)
		/// </summary>
		//---------------------------------------------------------------------------
		private void RemoveButton_Click(object sender, System.EventArgs e)
		{
			// per ogni nodo selezionato rintraccio a quale tabella appartiene
			// e prima di rimuovere l'item reinserisco il nodo nel tree
			foreach (ListViewItem item in SelectedTblListView.SelectedItems)
				RemoveSingleItem(item);		
		}

		/// <summary>
		/// evento sul pulsante minore minore
		/// </summary>
		//---------------------------------------------------------------------
		private void RemoveAllButton_Click(object sender, System.EventArgs e)
		{
			// per ogni nodo presente nella list view rintraccio a quale tabella appartiene
			// e prima di rimuovere l'item reinserisco il nodo nel tree
			foreach (ListViewItem item in SelectedTblListView.Items)
				RemoveSingleItem(item);		
		}
		# endregion

		# region Funzioni per spostare o visualizzare oggetti dal TreeView alla ListView e viceversa
		//---------------------------------------------------------------------
		private void RemoveSingleItem(ListViewItem item)
		{
			// se all'item è stata associata l'immagine con l'indice 1 significa che 
			// è una chiave primaria, xciò non è consentito la sua rimozione dalla lista
			// delle colonne selezionate
			if (item.ImageIndex == Images.GetKeyBitmapIndex())
				return;

			PlugInTreeNode nodeC = new PlugInTreeNode(item.Text);
			string table = item.SubItems[1].Text;

			for (int i = 0; i < SourceTblTreeView.Nodes.Count; i++)
			{
				PlugInTreeNode selectedNode = 
					(PlugInTreeNode)SourceTblTreeView.Nodes[i];

				if (string.Compare(selectedNode.Text, table, true, CultureInfo.InvariantCulture) == 0)
				{
					nodeC.ImageIndex			= Images.GetColumnBitmapIndex();
					nodeC.SelectedImageIndex	= Images.GetColumnBitmapIndex();
					nodeC.Type					= DataManagerConsts.ColumnNode;
					SourceTblTreeView.Nodes[i].Nodes.Add(nodeC);

					CatalogTableEntry entry = expSelections.Catalog.GetTableEntry(table);
					entry.SelectedColumnsList.Remove(item.Text);
				}
			}
			item.Remove();
		}

		//---------------------------------------------------------------------
		private void ShowSelectedColumnsList(string tableName)
		{
			CatalogTableEntry entry = expSelections.Catalog.GetTableEntry(tableName);
			
			StringCollection keys = new StringCollection();
			expSelections.Catalog.GetPrimaryKeys(tableName, ref keys); 

			SelectedTblListView.BeginUpdate();

			ListViewItem item;

			foreach (string col in entry.SelectedColumnsList)
			{
				// non visualizzo le colonne obbligatorie (c'è una pagina apposita per la loro gestione)
				if (string.Compare(col, DatabaseLayerConsts.TBCreatedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(col, DatabaseLayerConsts.TBModifiedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(col, DatabaseLayerConsts.TBCreatedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(col, DatabaseLayerConsts.TBModifiedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0)
					continue;

				item = new ListViewItem();

				// scelgo la bitmap (se è chiave o meno)
				if (keys.Contains(col))
				{
					item.ImageIndex = Images.GetKeyBitmapIndex();
					item.ForeColor	= Color.Gray;
				}
				else
					item.ImageIndex = Images.GetColumnBitmapIndex();

				item.Text = col;
				item.SubItems.Add(tableName);
				SelectedTblListView.Items.Add(item);
			}

			SelectedTblListView.EndUpdate();
		}

		//---------------------------------------------------------------------
		private string GetSelectedTableName(PlugInTreeNode selectedNode)
		{
			if (selectedNode == null)
				return string.Empty;

			switch (selectedNode.Type)
			{
				case DataManagerConsts.TableNode:
					return selectedNode.Text;
				case DataManagerConsts.ColumnNode:
					return selectedNode.Parent.Text;
				default:
					return string.Empty;
			}
		}
		# endregion

		# region Eventi sul TreeView
		/// <summary>
		/// evento sul click di un nodo del tree
		/// cliccando su un nodo del tree visualizzo nella list view tutte le colonne
		/// delle tabella che ho già selezionato (ci saranno sicuramente le chiavi primarie)
		/// </summary>
		//---------------------------------------------------------------------
		private void SourceTblTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)e.Node;
			
			if (selectedNode != null && selectedNode.Type.Length > 0)
			{
				SelectedTblListView.Items.Clear();
				ShowSelectedColumnsList(GetSelectedTableName(selectedNode));
			}
		}

		//---------------------------------------------------------------------
		private void SourceTblTreeView_DoubleClick(object sender, System.EventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)SourceTblTreeView.SelectedNode;

			if (selectedNode.Index != -1)
			{
				switch (selectedNode.Type)
				{
					case DataManagerConsts.TableNode:
						break;

					case DataManagerConsts.ColumnNode:
						InsertColumnInfo(selectedNode.Parent.Text, selectedNode.Text);
						selectedNode.Remove(); // elimino solo il nodo di tipo colonna
						break;
				}
			}
		}

		/// <summary>
		/// evento sul "move" del mouse sul treeview faccio comparire un tooltip 
		/// con la traduzione in lingua (se esiste) del nome della tabella)
		/// </summary>
		//---------------------------------------------------------------------
		private void SourceTblTreeView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			PlugInTreeNode selectedNode = SourceTblTreeView.GetNodeAt(e.X, e.Y) as PlugInTreeNode;

			if (selectedNode != null)
			{
				switch (selectedNode.Type)
				{
					case DataManagerConsts.TableNode:
					{
						string tableName = (selectedNode != null) ? selectedNode.Text : string.Empty;
						string tableLocalized = DatabaseLocalizer.GetLocalizedDescription(tableName, tableName);

						// se il testo del nodo è diverso dal testo localizzato e tradotto allora
						// faccio apparire il tooltip
						if (string.Compare(selectedNode.Text, tableLocalized, true, CultureInfo.InvariantCulture) != 0)
						{
							if (ColumnToolTip.GetToolTip(SourceTblTreeView) != tableLocalized)
								ColumnToolTip.SetToolTip(SourceTblTreeView, tableLocalized);
						}
						else
							ColumnToolTip.RemoveAll();

						break;
					}

					case DataManagerConsts.ColumnNode:
					{
						string colName = (selectedNode != null) ? selectedNode.Text : string.Empty;

						string colLocalized = DatabaseLocalizer.GetLocalizedDescription
							(
							colName,
							GetSelectedTableName(SourceTblTreeView.SelectedNode as PlugInTreeNode)
							);

						// se il testo del nodo è diverso dal testo localizzato e tradotto allora
						// faccio apparire il tooltip
						if (string.Compare(colName, colLocalized, true, CultureInfo.InvariantCulture) != 0)
						{
							if (ColumnToolTip.GetToolTip(SourceTblTreeView) != colLocalized)
								ColumnToolTip.SetToolTip(SourceTblTreeView, colLocalized);
						}
						else
							ColumnToolTip.RemoveAll();

						break;
					}

					default:
						break;
				}
			}
		}
		# endregion

		# region Eventi sulla ListView
		/// <summary>
		/// evento sul "move" del mouse sulla listview faccio comparire un tooltip
		/// </summary>
		//---------------------------------------------------------------------
		private void SelectedTblListView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ListViewItem item = SelectedTblListView.GetItemAt(e.X, e.Y);
			string colName = (item != null) ? item.Text : string.Empty;

			string colLocalized = DatabaseLocalizer.GetLocalizedDescription
				(
				colName,
				GetSelectedTableName(SourceTblTreeView.SelectedNode as PlugInTreeNode)
				);

			// se il testo del nodo è diverso dal testo localizzato e tradotto allora
			// faccio apparire il tooltip
			if (string.Compare(colName, colLocalized, true, CultureInfo.InvariantCulture) != 0)
			{
				if (ColumnToolTip.GetToolTip(SelectedTblListView) != colLocalized)
					ColumnToolTip.SetToolTip(SelectedTblListView, colLocalized);
			}
			else
				ColumnToolTip.RemoveAll();
		}
		# endregion
	}
}
