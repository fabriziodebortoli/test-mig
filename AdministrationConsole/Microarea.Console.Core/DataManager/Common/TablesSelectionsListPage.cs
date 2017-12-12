using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;
using Microarea.TaskBuilderNet.Core.StringLoader;

namespace Microarea.Console.Core.DataManager.Common
{
	//=========================================================================
	public partial class TablesSelectionsListPage : InteriorWizardPage
	{
		private Images myImages = null;		
		private ExportSelections exportSel = null;

		private bool fromDefault = false;
		private bool fromSample	= false;

		//---------------------------------------------------------------------
		public TablesSelectionsListPage()
		{
			InitializeComponent();
			InitializeImageList();
		}

		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
			
			SetImageInHeaderPicture();

			exportSel = ((Common.DataManagerWizard)this.WizardManager).GetExportSelections();
			exportSel.LoadModuleTableInfo(true);

			LoadAvailableTables();
		
			if (SelectedTblListView.Items.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			
			return true;
		}

		# region Inizializzazione ImageList e SetHeaderPicture
		//---------------------------------------------------------------------
		private void InitializeImageList()
		{
			myImages = new Images();

			// inizializzo imagelist del tree
			SourceTblTreeView.ImageList = myImages.ImageList;

			// inizializzo imagelist della listview
			SelectedTblListView.LargeImageList = myImages.ImageList;
			SelectedTblListView.SmallImageList = myImages.ImageList;

			SelectedTblListView.Columns.Add(string.Empty, 250, HorizontalAlignment.Left);
		}
		
		//---------------------------------------------------------------------
		private void SetImageInHeaderPicture()
		{
			// di default metto l'image dell'export
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetExportBmpSmallIndex()];

			fromDefault = (((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections() != null) ? true : false;
			if (fromDefault)
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetDefaultBmpSmallIndex()];

			fromSample = (((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false;
			if (fromSample)
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetSampleBmpSmallIndex()];
		}
		# endregion

		# region Caricamento delle informazioni suddivise per applicazione+modulo+tabella nel TreeView
		/// <summary>
		/// per caricare i nomi delle tabelle e delle colonne (letti dal catalog) nel tree della form
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadAvailableTables()
		{
			if (exportSel.AppDBStructInfo != null && (SourceTblTreeView.Nodes.Count == 0 || exportSel.ClearItems))
			{
				SelectedTblListView.Items.Clear();
				SourceTblTreeView.BeginUpdate();
				SourceTblTreeView.Nodes.Clear();

				TreeNode appNode = null; 
				TreeNode modNode = null; 
				TreeNode tableNode = null; 

				foreach (AddOnApplicationDBInfo appDBInfo in exportSel.AppDBStructInfo.ApplicationDBInfoList)
				{
					appNode = new PlugInTreeNode(appDBInfo.BrandTitle);
					appNode.ImageIndex			= Images.GetApplicationBitmapIndex();
					appNode.SelectedImageIndex	= Images.GetApplicationBitmapIndex();
					appNode.Tag				= DataManagerConsts.ApplicationNode;
					SourceTblTreeView.Nodes.Add(appNode);

					foreach (ModuleDBInfo modInfo in appDBInfo.ModuleList)
					{
						modNode = new PlugInTreeNode(modInfo.Title);
						modNode.ImageIndex = Images.GetModuleBitmapIndex();
						modNode.SelectedImageIndex	= Images.GetModuleBitmapIndex();
						modNode.Tag = DataManagerConsts.ModuleNode;
                        appNode.Nodes.Add(modNode);

						foreach (EntryDBInfo tabEntry in modInfo.TablesList)
						{
							tableNode = new PlugInTreeNode(tabEntry.Name);

							if (tabEntry.Exist)
							{
								tableNode.ImageIndex			= Images.GetTableBitmapIndex();
								tableNode.SelectedImageIndex	= Images.GetTableBitmapIndex();
								tableNode.Tag					= DataManagerConsts.TableNode;
							}
							else
							{
								tableNode.ImageIndex			= Images.GetUncheckedBitmapIndex();
								tableNode.SelectedImageIndex	= Images.GetUncheckedBitmapIndex();
								tableNode.ForeColor				= Color.Red;
								tableNode.Tag					= DataManagerConsts.NoExistTableNode; 						
							}

                            modNode.Nodes.Add(tableNode);	

							// se l'utente ha scelto di caricare le selezioni da file sposto le tabelle a mano...
							if (exportSel.LoadFromConfigurationFile)
								if (exportSel.ConfigInfo.ExistTableInConfigInfo(appDBInfo.ApplicationName, modInfo.ModuleName, tabEntry.Name))
								{
									InsertTableInListView(tableNode);
									tableNode.Remove();
								}
						}					
					}
				}
				SourceTblTreeView.EndUpdate();
				exportSel.ClearItems = false;
			}
		}
		# endregion

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			if (fromSample)
				return (exportSel.WriteQuery) ? "AddWhereClausePage" : "BaseColumnsPage";
			else
			{
				if (exportSel.SelectColumns && !exportSel.WriteQuery)
					return "ColumnsSelectionsListPage";

				if (!exportSel.SelectColumns && exportSel.WriteQuery)
					return "AddWhereClausePage";

				if (!exportSel.SelectColumns && !exportSel.WriteQuery)
					return (fromDefault) ? "BaseColumnsPage" : "BaseColumnsParamPage";

				return base.OnWizardNext();
			}
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			return "TablesParamPage";
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerCommon + "TablesSelectionsListPage");
			return true;
		}
		#endregion

		# region Funzioni per spostare oggetti di tipo Tabella dal TreeView alla ListView e viceversa
		/// <summary>
		/// per ogni applicazione scorro tutti i moduli ed inserisco tutte le tabelle
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertApplicationInfo(TreeNode selectedNode)
		{
			foreach (PlugInTreeNode moduleNode in selectedNode.Nodes)
				InsertModuleInfo(moduleNode);
		}

		/// <summary>
		/// da un nodo di tipo Modulo vado ad inserire tutte le sue tabelle nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertModuleInfo(TreeNode selectedNode)
		{
			foreach (TreeNode tableNode in selectedNode.Nodes)
				InsertTableInListView(tableNode);
		}

		/// <summary>
		/// inserisce l'elemento tabella nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InsertTableInListView(TreeNode tableNode)
		{
            if (tableNode == null)
                return;

            string tableNodeType = tableNode.Tag as string;

            if (tableNodeType == DataManagerConsts.NoExistTableNode)
				return;
			
			SelectedTblListView.BeginUpdate();
			
			CatalogTableEntry entry = exportSel.Catalog.GetTableEntry(tableNode.Text);
			entry.Selected = true;

			ListViewItem item; 
			item = new ListViewItem();
			item.ImageIndex = Images.GetTableBitmapIndex();
			item.Text = tableNode.Text;
			item.Tag  = tableNode.Parent;
			SelectedTblListView.Items.Add(item);

			item.EnsureVisible();
			SelectedTblListView.EndUpdate();
		}

		/// <summary>
		/// rimuove il singolo elemento tabella dalla listview e lo inserisce nell'albero
		/// </summary>
		//---------------------------------------------------------------------------
		private void RemoveSingleItem(ListViewItem item)
		{
			// costruisco il nodo da aggiungere la tabella nel tree nel nodo di appartenenza 
			TreeNode nodeTable = new TreeNode(item.Text);
			nodeTable.ImageIndex			= Images.GetTableBitmapIndex();
			nodeTable.SelectedImageIndex	= Images.GetTableBitmapIndex();
			nodeTable.Tag					= DataManagerConsts.TableNode;
			((TreeNode)item.Tag).Nodes.Add(nodeTable);
					
			CatalogTableEntry entry = exportSel.Catalog.GetTableEntry(item.Text);
			entry.Selected = false;

			item.Remove();
		}
		# endregion
		
		# region Eventi sui bottoni >, >>, <<
		/// <summary>
		/// evento sul pulsante > (aggiungo l'entry selezionato nel tree alla list view)
		/// </summary>
		//---------------------------------------------------------------------------
		private void AddButton_Click(object sender, System.EventArgs e)
		{
            TreeNode selectedNode = SourceTblTreeView.SelectedNode;

            if (selectedNode != null && selectedNode.Index != -1)
			{
                string selectedNodeType = selectedNode.Tag as string;
                switch (selectedNodeType)
				{
					case DataManagerConsts.ApplicationNode: // sposto tutte le tabelle di tutti i moduli dell'appl.
						InsertApplicationInfo(selectedNode);
						foreach (TreeNode modNode in selectedNode.Nodes)
							modNode.Nodes.Clear(); // tolgo solo i nodi di tipo tabella
						break;

					case DataManagerConsts.ModuleNode:
						InsertModuleInfo(selectedNode); // sposto tutte le tabelle del modulo
						selectedNode.Nodes.Clear();
						break;

					case DataManagerConsts.TableNode:
						InsertTableInListView(selectedNode);// sposto solo la singola tabella
						selectedNode.Remove(); 
						break;				
				}
			}

			if (SelectedTblListView.Items.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Next | WizardButton.Back);
		}

		/// <summary>
		/// evento sul pulsante Minore rimuovo le tabelle selezionati sulla list view
		/// e le sposto nell'albero associandoli al modulo di appartenenza
		/// </summary>
		//---------------------------------------------------------------------------
		private void RemoveButton_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem item in SelectedTblListView.SelectedItems)
				RemoveSingleItem(item);

			if (SelectedTblListView.Items.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
		}

		/// <summary>
		/// evento sul pulsante minore minore (rimuovo tutti gli item presenti nella list view)
		/// </summary>
		//---------------------------------------------------------------------
		private void RemoveAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem item in SelectedTblListView.Items)
				RemoveSingleItem(item);
			
			if (SelectedTblListView.Items.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
		}
		# endregion

		# region Eventi sul TreeView
		/// <summary>
		/// evento sul doppio click su un nodo del tree
		/// </summary>
		//---------------------------------------------------------------------
		private void SourceTblTreeView_DoubleClick(object sender, System.EventArgs e)
		{
			TreeNode selectedNode = SourceTblTreeView.SelectedNode;

            if (selectedNode != null && selectedNode.Index != -1)
            {
                string selectedNodeType = selectedNode.Tag as string;
                switch (selectedNodeType)
				{
					case DataManagerConsts.ApplicationNode:
					case DataManagerConsts.ModuleNode:
						break;

					case DataManagerConsts.TableNode:
						InsertTableInListView(selectedNode);// sposto solo la singola tabella
						selectedNode.Remove(); 
						break;				
				}
			}

			if (SelectedTblListView.Items.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Next | WizardButton.Back);
		}		

		/// <summary>
		/// evento sul "move" del mouse sul treeview faccio comparire un tooltip 
		/// con la traduzione in lingua (se esiste) del nome della tabella)
		/// </summary>
		//---------------------------------------------------------------------
		private void SourceTblTreeView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			TreeNode item = SourceTblTreeView.GetNodeAt(e.X, e.Y);
			string tableName = (item != null) ? item.Text : string.Empty;

			string tableLocalized = 
				DatabaseLocalizer.GetLocalizedDescription
				(
				tableName, 
				tableName
				);

			// se il testo del nodo è diverso dal testo localizzato e tradotto allora
			// faccio apparire il tooltip
			if (string.Compare(tableName, tableLocalized, true, CultureInfo.InvariantCulture) != 0)
			{
				if (TableToolTip.GetToolTip(SourceTblTreeView) != tableLocalized)
					TableToolTip.SetToolTip(SourceTblTreeView, tableLocalized);
			}
			else
				TableToolTip.RemoveAll();
		}
		# endregion

		# region Eventi sul ContextMenu agganciato agli item della ListView
		/// <summary>
		/// evento sul menu di contesto associato all'item tabella presente e 
		/// relativo al menu item Facoltativo che permette di rendere un file
		/// di dati di default facoltativo o meno (ovvero caricabile poi su scelta
		/// dell'utente e non durante i processo di aggiornamento del database aziendale)
		/// </summary>
		//---------------------------------------------------------------------
		private void Optional_Click(object sender, System.EventArgs e)
		{
			// questo evento è intercettato solo se è una pagina del wizard di Default
			if (fromDefault)
			{
				CatalogTableEntry entry = null;
				foreach (ListViewItem item in SelectedTblListView.SelectedItems)
				{
					entry = exportSel.Catalog.GetTableEntry(item.Text);
					((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
					entry.Optional = ((MenuItem)sender).Checked;				
				}
			}
		}

		/// <summary>
		/// evento sul menu di contesto associato all'item tabella presente e 
		/// relativo al menu item In Aggiunta che permette di creare un nuovo file denominato
		/// nometabellaAppend.xml che aggiunge dei dati di default a quelli esistenti
		/// </summary>
		//---------------------------------------------------------------------
		private void Append_Click(object sender, System.EventArgs e)
		{
			// questo evento è intercettato solo se è una pagina del wizard di Default
			if (fromDefault)
			{
				CatalogTableEntry entry = null;
				foreach (ListViewItem item in SelectedTblListView.SelectedItems)
				{
					entry = exportSel.Catalog.GetTableEntry(item.Text);
					((MenuItem)sender).Checked = !((MenuItem)sender).Checked;
					entry.Append = ((MenuItem)sender).Checked;	
				}
			}
		}
		# endregion

		# region Eventi sulla ListView
		//---------------------------------------------------------------------
		private void SelectedTblListView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// questo evento è intercettato solo se è una pagina del wizard di Default
			if (fromDefault)
			{
				// aggancio il context menu solo se ho selezionato un solo item ed ho cliccato
				// sul pulsante destro del mouse.
				if (SelectedTblListView.SelectedItems.Count != 1)
					SelectedTblListView.ContextMenu = null;
				else
				{
					if (e.Button == MouseButtons.Right)
					{
						SelectedTblListView.ContextMenu = SelTableContextMenu;
						ListViewItem item = SelectedTblListView.GetItemAt(e.X,e.Y);
						CatalogTableEntry entry = exportSel.Catalog.GetTableEntry(item.Text);

						// valorizzo i check del context menu
						Optional.Checked	= entry.Optional;
						Append.Checked		= entry.Append;
					}
				}
			}
		}

		/// <summary>
		/// evento sul "move" del mouse sulla listview faccio comparire un tooltip
		/// </summary>
		//---------------------------------------------------------------------
		private void SelectedTblListView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ListViewItem item = SelectedTblListView.GetItemAt(e.X, e.Y);
			string tableName = (item != null) ? item.Text : string.Empty;

			string tableLocalized = DatabaseLocalizer.GetLocalizedDescription(tableName, tableName);

			// se il testo del nodo è diverso dal testo localizzato e tradotto allora
			// faccio apparire il tooltip
			if (string.Compare(tableName, tableLocalized, true, CultureInfo.InvariantCulture) != 0)
			{
				if (TableToolTip.GetToolTip(SelectedTblListView) != tableLocalized)
					TableToolTip.SetToolTip(SelectedTblListView, tableLocalized);
			}
			else
				TableToolTip.RemoveAll();
		}
		# endregion
	}
}
