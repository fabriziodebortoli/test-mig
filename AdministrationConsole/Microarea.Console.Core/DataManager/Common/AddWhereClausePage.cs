using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Core.DataManager.Common
{
	//=========================================================================
	public partial class AddWhereClausePage : InteriorWizardPage
	{
		private string	    tableName	= string.Empty;
		private TreeNode	oldNode		= null;
		private Images		myImages	= null;		

		private ExportSelections exportSel = null;
		private bool fromDefaultOrSample = false;

		//---------------------------------------------------------------------
		public AddWhereClausePage()
		{
			InitializeComponent();
			InitializeImageList();
		}

		# region Inizializzazione e set ImageList
		/// <summary>
		/// funzione per inizializzare le bitmap
		/// </summary>
		//---------------------------------------------------------------------------
		private void InitializeImageList()
		{
			myImages = new Images();

			// inizializzo imagelist del tree
			SourceTblTreeView.ImageList = myImages.ImageList;
		}

		// imposta l'header picture corretta a seconda del tipo di dato trattato nel wizard
		//---------------------------------------------------------------------
		private void SetImageInHeaderPicture()
		{
			// di default metto l'image dell'export
			this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetExportBmpSmallIndex()];

			fromDefaultOrSample = (((Common.DataManagerWizard)this.WizardManager).GetDefaultSelections() != null) ? true : false;
			if (fromDefaultOrSample)
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetDefaultBmpSmallIndex()];

			fromDefaultOrSample = fromDefaultOrSample || ((((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false);
			if (fromDefaultOrSample && ((((Common.DataManagerWizard)this.WizardManager).GetSampleSelections() != null) ? true : false))
				this.m_headerPicture.Image = myImages.SmallPictureImageList.Images[Images.GetSampleBmpSmallIndex()];
		
			this.m_headerPicture.Refresh();
		}
		# endregion

		# region OnSetActive
		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			SetImageInHeaderPicture();

			exportSel = ((Common.DataManagerWizard)this.WizardManager).GetExportSelections();

			LoadAvailableTables();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			return true;
		}
		# endregion

		# region Riempimento del TreeView
		/// <summary>
		/// per caricare i nomi delle tabelle nel tree della pagina
		/// </summary>
		//---------------------------------------------------------------------
		protected void LoadAvailableTables()
		{
			SourceTblTreeView.BeginUpdate();
			SourceTblTreeView.Nodes.Clear();

			PlugInTreeNode tableNode = null; 
			PlugInTreeNode columnNode = null;

			foreach (CatalogTableEntry catEntry in exportSel.Catalog.TblDBList)
			{
				tableName = catEntry.TableName;	
				
				if (exportSel.AllTables || catEntry.Selected)
					tableNode = new PlugInTreeNode(tableName);
				else
					continue;

				tableNode.ImageIndex			= Images.GetTableBitmapIndex();
				tableNode.SelectedImageIndex	= Images.GetTableBitmapIndex();
				tableNode.Tag					= DataManagerConsts.TableNode;

				// se l'utente ha scelto di caricare le selezioni da file imposto la where clause sulla tabella...
				if (exportSel.LoadFromConfigurationFile)
				{
					TableNode tbl = exportSel.ConfigInfo.GetTableInConfigInfo(catEntry.Application, catEntry.Module, catEntry.TableName);
					if (tbl != null)
						catEntry.WhereClause = tbl.WhereClause;
				}

				// carico cmq le informazioni per il caricamento delle colonne, in modo da visualizzarle per
				// facilitare l'utente nella scrittura della clausola di WHERE
				if (catEntry.ColumnsInfo == null)
					catEntry.LoadColumnsInfo(exportSel.ContextInfo.Connection, true);
				
				foreach (CatalogColumn col in catEntry.ColumnsInfo)
				{
					// le colonne obbligatorie non le faccio vedere
					if (string.Compare(col.Name, DatabaseLayerConsts.TBCreatedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
						string.Compare(col.Name, DatabaseLayerConsts.TBModifiedColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
						string.Compare(col.Name, DatabaseLayerConsts.TBCreatedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0 ||
						string.Compare(col.Name, DatabaseLayerConsts.TBModifiedIDColNameForSql, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;

					columnNode = new PlugInTreeNode(col.Name);

					// visualizzo nel tree tutte le colonne (sia quelle selezionate che le altre)
					if (!catEntry.SelectedColumnsList.Contains(col.Name))
					{
						columnNode.ImageIndex			= (col.IsKey) ? Images.GetKeyBitmapIndex() : Images.GetColumnBitmapIndex();
						columnNode.SelectedImageIndex	= (col.IsKey) ? Images.GetKeyBitmapIndex() : Images.GetColumnBitmapIndex();
						columnNode.Tag					= DataManagerConsts.ColumnNode;
						tableNode.Nodes.Add(columnNode);
					}
					else
					{
						if (col.IsKey)
						{
							columnNode.ImageIndex			= Images.GetKeyBitmapIndex();
							columnNode.SelectedImageIndex	= Images.GetKeyBitmapIndex();
							columnNode.Tag					= DataManagerConsts.ColumnNode;
                            tableNode.Nodes.Add(columnNode);
						}
						else
						{
							// se la colonna analizzata non è presente nelle selezionate la inserisco con un'icona diversa
							columnNode.ImageIndex			= Images.GetSelectedColumnBitmapIndex();
							columnNode.SelectedImageIndex	= Images.GetSelectedColumnBitmapIndex();
							columnNode.Tag					= DataManagerConsts.ColumnNode;
                            tableNode.Nodes.Add(columnNode);
						}
					}
				}

				SourceTblTreeView.Nodes.Add(tableNode);
			}

			SourceTblTreeView.EndUpdate();
		}
		# endregion

		# region OnWizardNext e OnWizardBack
		//---------------------------------------------------------------------
        public override string OnWizardNext()
		{
			if (fromDefaultOrSample)
				return "BaseColumnsPage";

			return base.OnWizardNext();
		}

		//---------------------------------------------------------------------
        public override string OnWizardBack()
		{
			if (exportSel.AllTables && !exportSel.SelectColumns)
				return "TablesParamPage";

			if (!exportSel.AllTables && !exportSel.SelectColumns)
				return "TablesSelectionsListPage";

			if (exportSel.SelectColumns)
				return "ColumnsSelectionsListPage";

			return base.OnWizardBack();
		}
		# endregion

		#region OnWizardHelp
		/// <summary>
		/// OnWizardHelp
		/// </summary>
		//---------------------------------------------------------------------
        public override bool OnWizardHelp()
		{
			this.WizardManager.HelpFromWizardPage(this, DataManagerConsts.NamespaceDBAdminPlugIn, DataManagerConsts.NamespaceDataManagerCommon + "AddWhereClausePage");
			return true;
		}
		#endregion

		# region Funzioni Get e Set della WHERE clause
		//---------------------------------------------------------------------
		private void SetWhereClauseText(string tableName)
		{
			CatalogTableEntry entry = exportSel.Catalog.GetTableEntry(tableName);
			if (entry != null)
				entry.WhereClause = WhereClauseTextBox.Text;
		}

		//---------------------------------------------------------------------
		private string GetWhereClauseText(string tableName)
		{
			CatalogTableEntry entry = exportSel.Catalog.GetTableEntry(tableName);
			return 
				(entry != null) ? entry.WhereClause : null;
		}
		# endregion

		# region Eventi sul TreeView
		//---------------------------------------------------------------------
		private void SourceTblTreeView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)e.Node;

            if (selectedNode != null && selectedNode.Tag != null)
            {
                string selectedNodeType = selectedNode.Tag as string;
                if (selectedNodeType != null && selectedNodeType.Length > 0)
                {
                    WhereClauseTextBox.Text =
                        GetWhereClauseText
                        (
                        (selectedNode.Type == DataManagerConsts.ColumnNode)
                        ? selectedNode.Parent.Text
                        : selectedNode.Text
                        );
                }
            }

			SyntaxCheckButton.Enabled = (WhereClauseTextBox.Text.Length == 0) ? false : true;
			PreviewButton.Enabled = SyntaxCheckButton.Enabled;
		}

		//---------------------------------------------------------------------
		private void SourceTblTreeView_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)e.Node;
			
			if (selectedNode == null || oldNode == null)
				return;

            string selectedNodeType = selectedNode.Tag as string;
            TreeNode myNode =
                (selectedNodeType == DataManagerConsts.ColumnNode) 
				? selectedNode.Parent : selectedNode;

			SetWhereClauseText(oldNode.Text);
			oldNode = myNode;
		}

		//---------------------------------------------------------------------
		private void SourceTblTreeView_Click(object sender, System.EventArgs e)
		{
            TreeNode selectedNode = SourceTblTreeView.SelectedNode;

            if (selectedNode != null)
            {
                string selectedNodeType = selectedNode.Tag as string;
                oldNode = (selectedNodeType == DataManagerConsts.ColumnNode)
                    ? selectedNode.Parent : selectedNode;
            }
			// cliccando su un nodo del tree vengono chiamati i seg. eventi:
			// - Click
			// - BeforeSelect
			// - AfterSelect
		}

		//---------------------------------------------------------------------
		private void SourceTblTreeView_DoubleClick(object sender, System.EventArgs e)
		{
            TreeNode selectedNode = SourceTblTreeView.SelectedNode;

            if (selectedNode != null && selectedNode.Index != -1)
			{
                string selectedNodeType = selectedNode.Tag as string;
                switch (selectedNodeType)
                {
					case DataManagerConsts.TableNode:
						break;

					case DataManagerConsts.ColumnNode:
						WhereClauseTextBox.AppendText(selectedNode.Text);
						WhereClauseTextBox.Focus();
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
			TreeNode item = SourceTblTreeView.GetNodeAt(e.X, e.Y);
			string tableName = (item != null) ? item.Text : string.Empty;

			string tableLocalized = DatabaseLocalizer.GetLocalizedDescription(tableName, tableName);

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

		# region Eventi sul click dei bottoni Check Sintassi e Anteprima
		//---------------------------------------------------------------------
		private void SyntaxCheckButton_Click(object sender, System.EventArgs e)
		{
			if (oldNode == null)
				return;

			string checkQuery = 
				string.Format("SELECT * FROM {0} WHERE {1}", oldNode.Text, GetWhereClauseText(oldNode.Text));

			try
			{
				TBCommand command = new TBCommand(checkQuery, exportSel.ContextInfo.Connection);
				command.ExecuteNonQuery();
			}
			catch(TBException exp)
			{
				DiagnosticViewer.ShowErrorTrace
					(
					string.Format(DataManagerStrings.WrongSyntax + "\n" + "({0})", exp.Message),
					string.Empty, 
					string.Empty
					);
				return;
			}

			DiagnosticViewer.ShowCustomizeIconMessage
				(
				string.Format("{0}" + "\n" + DataManagerStrings.RightSyntax, checkQuery), 
				string.Empty, 
				MessageBoxIcon.Information
				);
		}

		//@@TODO BAUZI
		//---------------------------------------------------------------------
		private void PreviewButton_Click(object sender, System.EventArgs e)
		{
			string query = exportSel.MakeExportQuery(exportSel.Catalog.GetTableEntry(oldNode.Text));

			if (query.Length == 0)
				return; //ERRORE
			
			// in questo caso l'ultima parte di stringa xchè non mi serve x creare il datatable
			if (exportSel.ContextInfo.DbType == DBMSType.SQLSERVER)
				query = query.Replace(" FOR XML AUTO, XMLDATA", string.Empty);

			TBDataAdapter adapt = new TBDataAdapter(query, exportSel.ContextInfo.Connection);

			DataTable columns = null;
			
			try
			{
				columns = new DataTable(oldNode.Text);
				adapt.Fill(columns);
				adapt.Dispose();
			}			
			catch(TBException exp)
			{
				if (adapt != null) adapt.Dispose();
				DiagnosticViewer.ShowErrorTrace
					(
					string.Format(DataManagerStrings.WrongSyntax + "\n" + "({0})", exp.Message),
					string.Empty, 
					string.Empty
					);
				return;
			}

			// apro la form che visualizza un DataGrid contenente i dati del DataSet
			ShowDataSetForm form = new ShowDataSetForm(columns);
			if (form.SetDataGridBinding(columns, string.Empty))
				form.ShowDialog(); // faccio la show come se fosse modale
		}
		# endregion

		# region Eventi su altri controls della pagina
		//---------------------------------------------------------------------
		private void WhereClauseTextBox_Leave(object sender, System.EventArgs e)
		{
			if ((PlugInTreeNode)SourceTblTreeView.SelectedNode != null)
			{
				TreeNode selectedNode = SourceTblTreeView.SelectedNode;
                string selectedNodeType = (selectedNode != null) ? selectedNode.Tag as string : String.Empty;
                oldNode = (selectedNodeType == DataManagerConsts.ColumnNode) ? selectedNode.Parent : selectedNode;
				SetWhereClauseText(oldNode.Text);

				SyntaxCheckButton.Enabled = (WhereClauseTextBox.Text.Length == 0) ? false : true;
				PreviewButton.Enabled = SyntaxCheckButton.Enabled;
			}
		}

		//---------------------------------------------------------------------
		private void WhereClauseTextBox_TextChanged(object sender, System.EventArgs e)
		{
			SyntaxCheckButton.Enabled = (WhereClauseTextBox.Text.Length == 0) ? false : true;
			PreviewButton.Enabled = SyntaxCheckButton.Enabled;
		}
		# endregion
	}
}