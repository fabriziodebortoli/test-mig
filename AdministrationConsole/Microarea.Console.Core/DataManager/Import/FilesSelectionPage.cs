using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.DataManager.Common;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Import
{
	//=========================================================================
	public partial class FilesSelectionPage : InteriorWizardPage
	{
		# region Variabili private
		private ImportSelections importSel = null;
		private Images myImages = null;		

		// path della company che ho selezionato nella pagina precedente
		private string			path		= string.Empty;
		private	ListViewItem	selItemXML	= null;
		# endregion

		# region Costruttore
		//---------------------------------------------------------------------
		public FilesSelectionPage()
		{
			InitializeComponent();
			InitializeListView();
		}
		# endregion

		# region OnSetActive e OnKillActive
		//---------------------------------------------------------------------
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			importSel = ((Common.DataManagerWizard)this.WizardManager).GetImportSelections();
			
			LoadAvailableFiles();

			if (importSel.ImportList.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);

			return true;
		}

		//---------------------------------------------------------------------
        public override bool OnKillActive()
		{
			importSel.OldCompany = importSel.Company;
			return base.OnKillActive();
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
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerImport + "FilesSelectionPage");
			return true;
		}
		#endregion

		# region Caricamento delle informazioni suddivise per folder+files nel TreeView
		/// <summary>
		/// per caricare i nomi dei folder e dei files (letti dal file system) nel tree
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadAvailableFiles()
		{
			// se non ci sono elementi nel tree oppure ho forzato la clear degli items...
			if	(SourceFilesTreeView.Nodes.Count == 0 || importSel.ClearItems)
			{
				FilesListView.Items.Clear();
				SourceFilesTreeView.Nodes.Clear();

				// se carico i file da filesystem allora considero il path specificato, altrimenti guardo
				// nella Custom dell'azienda selezionata
				path = (importSel.LoadXmlToFileSystem)
					? importSel.PathFolderXml
					: importSel.ContextInfo.PathFinder.GetCustomCompanyDataManagerPath(importSel.Company);

				if (!Directory.Exists(path))
					return;
				
				DirectoryInfo dir = new DirectoryInfo(path);

				if (!importSel.LoadXmlToFileSystem)
				{
					foreach (DirectoryInfo d in dir.GetDirectories())
					{
						// se la directory non contiene files allora non la faccio neppure vedere
						// in modo che l'utente non si trovi neppure a selezionarla per errore
						if (d.GetFiles().Length == 0)
							continue;
						ExploreDirectory(d);
					}
				}
				else
				{ 
					if (dir.GetFiles("*.xml").Length > 0)
						ExploreDirectory(dir);
				}

				importSel.ClearItems = false;
			}
		}

		/// <summary>
		/// esplora il contenuto della directory
		/// </summary>
		//---------------------------------------------------------------------
		private void ExploreDirectory(DirectoryInfo dir)
		{
			bool xmlExist = false;
			foreach (FileInfo fi in dir.GetFiles())
			{
				// se almeno un file della directory ha un'estensione xml allora procedo
				if (string.Compare(fi.Extension, NameSolverStrings.XmlExtension, true, CultureInfo.InvariantCulture) == 0)
				{
					xmlExist = true;
					break;
				}
			}

			if (!xmlExist)
				return;

            TreeNode dirNode = new TreeNode(dir.Name);
			dirNode.ImageIndex			= Images.GetFolderBitmapIndex();
			dirNode.SelectedImageIndex	= Images.GetFolderBitmapIndex();
            dirNode.Tag = DataManagerConsts.DirectoryNode; 
			SourceFilesTreeView.Nodes.Add(dirNode);

			// carico i singoli file xml nelle directory
			foreach (FileInfo f in dir.GetFiles())
				ExploreFile(f, dirNode);
		}

		/// <summary>
		/// carica i nomi dei file (solo quelli con estensione xml) contenuti nei folder
		/// </summary>
		//---------------------------------------------------------------------
        private void ExploreFile(FileInfo file, TreeNode dirNode)
		{
			// se il file esiste già nella import list non lo inserisco più.
			if (importSel.ExistenceFileInImportList(file.DirectoryName, file.Name))
				return;

			// se il file ha un'estensione diversa da xml non lo visualizzo
			if (string.Compare(file.Extension, NameSolverStrings.XmlExtension, true, CultureInfo.InvariantCulture) != 0)
				return;

            TreeNode fileNode = new TreeNode(file.Name);
			fileNode.ImageIndex			= Images.GetXmlFileBitmapIndex();
			fileNode.SelectedImageIndex	= Images.GetXmlFileBitmapIndex();
			fileNode.Tag				= DataManagerConsts.FileNode; 

			dirNode.Nodes.Add(fileNode);
		}
		# endregion

		# region Inizializzazione ImageList
		/// <summary>
		/// inizializzo le colonne da visualizzare nella list view
		/// </summary>
		//---------------------------------------------------------------------------
		private void InitializeListView()
		{
			myImages = new Images();

			// inizializzo imagelist del tree
			SourceFilesTreeView.ImageList = myImages.ImageList;

			// inizializzo imagelist della listview
			FilesListView.LargeImageList = myImages.ImageList;
			FilesListView.SmallImageList = myImages.ImageList;
			FilesListView.Columns.Add(string.Empty, 250, HorizontalAlignment.Left);

			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetImportBmpSmallIndex()];
		}
		# endregion

		# region Eventi sui bottoni >, >>, <<
		//---------------------------------------------------------------------
		private void AddButton_Click(object sender, System.EventArgs e)
		{
            TreeNode selectedNode = (TreeNode)SourceFilesTreeView.SelectedNode;

			if (selectedNode != null && selectedNode.Index != -1)
			{
                string selectedNodeType = selectedNode.Tag as string;
                switch (selectedNodeType)
				{
					case DataManagerConsts.DirectoryNode:
					{
						FilesListView.BeginUpdate();

						importSel.AddItemInImportList
							(
							(importSel.LoadXmlToFileSystem 
							? path 
							: string.Concat(path, Path.DirectorySeparatorChar, selectedNode.Text)),
							string.Empty
							);

						ListViewItem item = null;

                        foreach (TreeNode file in selectedNode.Nodes)
						{
							item = new ListViewItem();
							item.ImageIndex	= Images.GetXmlFileBitmapIndex();
							item.Text	= file.Text;
							item.Tag	= selectedNode;
							FilesListView.Items.Add(item);
						}
						
						selectedNode.Nodes.Clear();

						FilesListView.EndUpdate();
						break;
					}
					
					case DataManagerConsts.FileNode:
					{
						FilesListView.BeginUpdate();

						if (importSel.LoadXmlToFileSystem)
							importSel.AddItemInImportList(path, selectedNode.Text);
						else
							importSel.AddItemInImportList(path + Path.DirectorySeparatorChar + selectedNode.Parent.Text, selectedNode.Text);

						ListViewItem item = new ListViewItem();
						item.ImageIndex	= Images.GetXmlFileBitmapIndex();
						item.Text	= selectedNode.Text;
						item.Tag	= selectedNode.Parent;
						FilesListView.Items.Add(item);

						selectedNode.Remove(); // elimino solo il nodo di tipo file

						FilesListView.EndUpdate();
						break;
					}
				}
			}

			if (importSel.ImportList.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}

		/// <summary>
		/// evento sul pulsante Minore (rimuovo il singolo item selezionato sulla list view)
		/// </summary>
		//---------------------------------------------------------------------------
		private void RemoveButton_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem item in FilesListView.SelectedItems)
				RemoveSingleItem(item);
		}

		/// <summary>
		/// evento sul pulsante DoppioMinore (rimuovo tutti gli item della list view)
		/// </summary>
		//---------------------------------------------------------------------
		private void RemoveAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem item in FilesListView.Items)
				RemoveSingleItem(item);
		}
		# endregion

		# region Funzione per spostare oggetti dal TreeView alla ListView e viceversa
		//---------------------------------------------------------------------
		private void RemoveSingleItem(ListViewItem item)
		{
			// per ogni nodo presente nella list view rintraccio a quale tabella appartiene
			// e prima di rimuovere l'item reinserisco il nodo nel tree
			TreeNode node	= new TreeNode(item.Text);
			node.ImageIndex	= Images.GetXmlFileBitmapIndex();
			node.SelectedImageIndex	= Images.GetXmlFileBitmapIndex();
            node.Tag = DataManagerConsts.FileNode;
            ((TreeNode)item.Tag).Nodes.Add(node);

			if (importSel.LoadXmlToFileSystem)
				importSel.RemoveItemFromImportList(path, item.Text);
			else
				importSel.RemoveItemFromImportList(path + Path.DirectorySeparatorChar + ((TreeNode)item.Tag).Text, item.Text);

			item.Remove();

			if (importSel.ImportList.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
		}
		# endregion

		# region Eventi sul TreeView
		/// <summary>
		/// evento sul click di un nodo del tree
		/// cliccando su un nodo del tree visualizzo nella list view tutte i file xml che ho già selezionato
		/// </summary>
		//---------------------------------------------------------------------
		private void SourceFilesTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			// gestisco il refresh delle selezioni dei file sul cambio delle directory
            TreeNode selectedNode = (TreeNode)e.Node;
			string fullPath = string.Empty;

            if (selectedNode != null && selectedNode.Tag != null)
            {
                string selectedNodeType = selectedNode.Tag as string;
                if (selectedNodeType != null && selectedNodeType.Length > 0)
                {
                    switch (selectedNodeType)
                    {
                        case DataManagerConsts.DirectoryNode:
							fullPath = (importSel.LoadXmlToFileSystem) 
								? path 
								: path + Path.DirectorySeparatorChar + selectedNode.Text;
                            break;

                        case DataManagerConsts.FileNode:
							if (importSel.LoadXmlToFileSystem)
								fullPath = path;
							else
								fullPath = path + Path.DirectorySeparatorChar + selectedNode.Parent.Text;
                            break;
                    }

                    FilesListView.Items.Clear();
                    ImportItemInfo item = importSel.GetImportItemInfo(fullPath);
                    if (item == null) 
						return;

                    if (item.SelectedFiles != null)
                    {
                        foreach (string file in item.SelectedFiles)
                        {
                            ListViewItem i = new ListViewItem();
                            i.ImageIndex = Images.GetXmlFileBitmapIndex();
                            i.Text = file;
                            i.Tag = (selectedNodeType == DataManagerConsts.FileNode) ? selectedNode.Parent : selectedNode;
                            FilesListView.Items.Add(i);
                        }
                    }
                }
            }
		}
		
		/// <summary>
		/// evento sul doppio click nel treeview
		/// </summary>
		//---------------------------------------------------------------------
		private void SourceFilesTreeView_DoubleClick(object sender, System.EventArgs e)
		{
            TreeNode selectedNode = (TreeNode)SourceFilesTreeView.SelectedNode;

			if (selectedNode != null && selectedNode.Index != -1)
			{
                string selectedNodeType = selectedNode.Tag as string;
                switch (selectedNodeType)
                {
					case DataManagerConsts.DirectoryNode:
						break;
					
					case DataManagerConsts.FileNode:
					{
						FilesListView.BeginUpdate();

						if (importSel.LoadXmlToFileSystem)
							importSel.AddItemInImportList(path, selectedNode.Text);
						else
							importSel.AddItemInImportList
							(
							string.Concat(path, Path.DirectorySeparatorChar, selectedNode.Parent.Text),
							selectedNode.Text
							);

						ListViewItem item = new ListViewItem();
						item.ImageIndex	= Images.GetXmlFileBitmapIndex();
						item.Text	= selectedNode.Text;
						item.Tag	= selectedNode.Parent;
						FilesListView.Items.Add(item);

						selectedNode.Remove(); // elimino solo il nodo di tipo file

						FilesListView.EndUpdate();

						break;
					}
				}
			}

			if (importSel.ImportList.Count <= 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}
		# endregion

		# region Eventi sulla ListView
		/// <summary>
		/// evento agganciato al click del mouse su un singolo item della list view
		/// per visualizzare il menu di contesto.
		/// </summary>
		//---------------------------------------------------------------------
		private void FilesListView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ShowFileContextMenu.MenuItems.Clear();

			// visualizzo il ContextMenu solo se ho selezionato UN SOLO item nella list view
			if (FilesListView.SelectedItems.Count == 1)
			{
				selItemXML = FilesListView.GetItemAt(e.X,e.Y);

				if (selItemXML != null)
				{
					switch (e.Button)
					{
						// solo col tasto dx faccio vedere il context menu
						case MouseButtons.Right:
						{	
							MenuItem menuItem = new MenuItem
								(
								DataManagerStrings.ContextMenuShow,  
								new System.EventHandler(OnShowFileXml)
								);

							ShowFileContextMenu.MenuItems.Clear();
							ShowFileContextMenu.MenuItems.Add(menuItem);
							break;
						}

						case MouseButtons.Left:
						case MouseButtons.Middle:
						case MouseButtons.None:
						default: 
							break;
					}
				}
			}
		}

		/// <summary>
		/// evento sul "move" del mouse sulla listview faccio comparire un tooltip
		/// </summary>
		//---------------------------------------------------------------------------
		private void FilesListView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ListViewItem item = FilesListView.GetItemAt(e.X, e.Y);

			if (item != null)
			{
				if (FileToolTip.GetToolTip(FilesListView) != item.Text)
					FileToolTip.SetToolTip(FilesListView, item.Text);
			}
			else
				FileToolTip.RemoveAll();
		}
		# endregion
        
		# region Funzione per visualizzare un datagrid con i dati contenuti nell'xml (dal menu di contesto)
		/// <summary>
		/// per visualizzare il datagrid associato ai dati contenuti nel file xml selezionato
		/// </summary>
		//---------------------------------------------------------------------------
		private void OnShowFileXml(object sender, System.EventArgs e)
		{
			// costruisco il path completo del file da passare al dataset
			string fullPath = (importSel.LoadXmlToFileSystem) 
				? string.Concat(path, Path.DirectorySeparatorChar, selItemXML.Text)
				: string.Concat(path, Path.DirectorySeparatorChar, ((TreeNode)selItemXML.Tag).Text,
								Path.DirectorySeparatorChar, selItemXML.Text);

			DataSet dataSet = null;
			
			try
			{
				dataSet = new DataSet();
				dataSet.ReadXml(fullPath);
			}
			catch
			{
				DiagnosticViewer.ShowInformation(DataManagerStrings.ErrFileEmpty, DataManagerStrings.LblAttention);
				return;
			}

			// controllo che la tabella analizzata contenga delle righe di dati da 
			// visualizzare. se è vuota non procedo
			int rows = -1;
			foreach (DataTable myTable in dataSet.Tables)
				rows = myTable.Rows.Count;

			if (rows > 0)
			{
				// se il nome del file è ExportData.xml significa che si tratta
				// un file che riporta i dati di più tabelle, quindi il datagrid non
				// deve fare il binding con il nome della singola tabella
				string table = string.Empty;
				FileInfo fi = new FileInfo(selItemXML.Text);
				if (string.Compare(fi.Name, DataManagerConsts.ExportDataFileName, true, CultureInfo.InvariantCulture) != 0)
					table = Path.GetFileNameWithoutExtension(fi.Name);
			
				// richiamo la form per mostrare il datagrid
				ShowDataSetForm form = new ShowDataSetForm(dataSet, table);
				if (form.SetDataGridBinding(dataSet, table))
					form.ShowDialog(); // faccio la show come se fosse modale
			}
			else
				DiagnosticViewer.ShowInformation(DataManagerStrings.ErrFileEmpty, DataManagerStrings.LblAttention);
		}
		# endregion
	}
}