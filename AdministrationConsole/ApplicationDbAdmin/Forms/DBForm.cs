using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microarea.Console.Core.DBLibrary;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.StringLoader;
using System.Diagnostics;

namespace Microarea.Console.Plugin.ApplicationDBAdmin.Forms
{
	/// <summary>
	/// Form che visualizza il tree con tutte le applicazioni+moduli che apportano oggetti di database
	/// nonche' elenco di tali oggetti suddivisi per tipo. Inoltre visualizza lo stato del database
	/// (vuoto, completo, scatti di release da effettuare, eventuali oggetti mancanti o moduli da ripristinare)
	/// </summary>
	//=========================================================================
	public partial class DBForm : PlugInsForm
	{
		private DatabaseManager databaseManager = null;
		private StatusType consoleStatusType = StatusType.None;
		private bool canMigrate = false;

		public delegate void AfterUpdateConfirm(object sender, System.EventArgs e, bool showDiagnostic=true);
		public event AfterUpdateConfirm OnAfterUpdateConfirm;

		// mi serve per inserire nell'output la struttura delle dipendenze (a solo scopo debug)
		//----------------------------------------------------
		public bool WriteLogInfo = false; // di default e' a false
		//----------------------------------------------------

		/// <summary>
		/// costruttore DBForm
		/// </summary>s
		//---------------------------------------------------------------------------
		public DBForm(DatabaseManager databaseManager, StatusType consoleStatusType, bool canMigrate)
		{
			this.databaseManager = databaseManager;
			this.consoleStatusType = consoleStatusType;
			this.canMigrate = canMigrate;
			InitializeComponent();
			InitializeImageList();
			InitDefaultConfigCombo();
			SetControlsAndText();
			LoadTree();
			LoadDiagnostic(databaseManager.DBManagerDiagnostic, DiagnosticType.All);
		}

		# region Caricamento TreeView
		/// <summary>
		/// funzione che riempie i vari nodi del tree e il menu di contesto
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadTree()
		{		
			DirTreeView.BeginUpdate();
			DirTreeView.Nodes.Clear();

			if (databaseManager == null)
				return;

			bool erpLoadingError = false, slaveLoadingError = false;

			// carico le applicazioni di ERP
			erpLoadingError = !LoadApplicationsInTreeView(databaseManager.AddOnAppList, NameSolverStrings.Apps);

			// se c'e' uno slave carico anche l'elenco delle applicazioni slave (ovvero il DMS)
			if (databaseManager.ContextInfo.HasSlaves && databaseManager.DmsStructureInfo != null)
				slaveLoadingError = !LoadApplicationsInTreeView(databaseManager.DmsStructureInfo.DmsAddOnAppList, DatabaseLayerConsts.DMSSignature);

			// se c'è un errore disabilito i controls
			UpdateButton.Enabled = (erpLoadingError || slaveLoadingError) ? false : true;
			ImportDefaultDataCheckBox.Enabled = ((erpLoadingError || slaveLoadingError) || !ImportDefaultDataCheckBox.Enabled) ? false : true;

			InfosLabel.Text = (erpLoadingError || slaveLoadingError) ? Strings.FormInfosWithErrors : Strings.FormStartElaboration;
			DefaultConfigComboBox.Enabled = (erpLoadingError || slaveLoadingError) ? false : true;
		}

		//---------------------------------------------------------------------------
		private bool LoadApplicationsInTreeView(List<AddOnApplicationDBInfo> applicationsList, string ancestor)
		{
			PlugInTreeNode appNode, modNode, entryNode = null;
			ArrayList list = null;

			bool result = true;

			// creo il nodo con l'applicazione "padre" (metto una bitmap diversa a seconda che si tratti di ERP o di DMS)
			PlugInTreeNode ancestorNode = new PlugInTreeNode(ancestor);
			ancestorNode.ImageIndex = (ancestor == NameSolverStrings.Apps) ? ImagesListManager.GetMagoNet16BitmapIndex() : ImagesListManager.GetEasyAttachment16BitmapIndex();
			ancestorNode.SelectedImageIndex = (ancestor == NameSolverStrings.Apps) ? ImagesListManager.GetMagoNet16BitmapIndex() : ImagesListManager.GetEasyAttachment16BitmapIndex();
			DirTreeView.Nodes.Add(ancestorNode);

			foreach (AddOnApplicationDBInfo addOnAppDBInfo in applicationsList)
			{
				appNode = new PlugInTreeNode(addOnAppDBInfo.BrandTitle);
				list = new ArrayList();

				appNode.ImageIndex = ImagesListManager.GetApplicationBitmapIndex();
				appNode.SelectedImageIndex = ImagesListManager.GetApplicationBitmapIndex();
				appNode.Type = DBConstStrings.AddOn;

				ancestorNode.Nodes.Add(appNode);

				foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
				{
					modNode = new PlugInTreeNode(moduleDBInfo.Title);

					// se un modulo contiene un errore allora me lo segno
					if (result)
						result = moduleDBInfo.Valid;

					if (moduleDBInfo.StatusOk)
					{
						modNode.Tag = moduleDBInfo.ModuleName;
						modNode.ImageIndex = moduleDBInfo.Valid ? ImagesListManager.GetModuleBitmapIndex() : ImagesListManager.GetUncheckedBitmapIndex();
						modNode.SelectedImageIndex = moduleDBInfo.Valid ? ImagesListManager.GetModuleBitmapIndex() : ImagesListManager.GetUncheckedBitmapIndex();
						modNode.ForeColor = moduleDBInfo.Valid ? Color.Black : Color.Red;
						modNode.Type = DBConstStrings.ModuleStatusOK;
					}
					else
					{
						modNode.ImageIndex = moduleDBInfo.Valid ? ImagesListManager.GetRedFlagBitmapIndex() : ImagesListManager.GetUncheckedBitmapIndex();
						modNode.SelectedImageIndex = moduleDBInfo.Valid ? ImagesListManager.GetRedFlagBitmapIndex() : ImagesListManager.GetUncheckedBitmapIndex();
						modNode.ForeColor = moduleDBInfo.Valid ? Color.Black : Color.Red;
						modNode.Type = DBConstStrings.ModuleStatusKO;
					}

					ArrayList myArray = new ArrayList();

					foreach (EntryDBInfo tabEntry in moduleDBInfo.TablesList)
					{
						entryNode = new PlugInTreeNode(tabEntry.Name);
						entryNode.ImageIndex = tabEntry.Exist ? ImagesListManager.GetTableBitmapIndex() : ImagesListManager.GetTableUncheckedBitmapIndex();
						entryNode.SelectedImageIndex = tabEntry.Exist ? ImagesListManager.GetTableBitmapIndex() : ImagesListManager.GetTableUncheckedBitmapIndex();
						myArray.Add(entryNode);
					}

					foreach (EntryDBInfo viewEntry in moduleDBInfo.ViewsList)
					{
						entryNode = new PlugInTreeNode(viewEntry.Name);
						entryNode.ImageIndex = viewEntry.Exist ? ImagesListManager.GetViewBitmapIndex() : ImagesListManager.GetViewUncheckedBitmapIndex();
						entryNode.SelectedImageIndex = viewEntry.Exist ? ImagesListManager.GetViewBitmapIndex() : ImagesListManager.GetViewUncheckedBitmapIndex();
						myArray.Add(entryNode);
					}

					foreach (EntryDBInfo procEntry in moduleDBInfo.ProceduresList)
					{
						entryNode = new PlugInTreeNode(procEntry.Name);
						entryNode.ImageIndex = procEntry.Exist ? ImagesListManager.GetProcedureBitmapIndex() : ImagesListManager.GetProcedureUncheckedBitmapIndex();
						entryNode.SelectedImageIndex = procEntry.Exist ? ImagesListManager.GetProcedureBitmapIndex() : ImagesListManager.GetProcedureUncheckedBitmapIndex();
						myArray.Add(entryNode);
					}

					// aggiungo il nodo del modulo
					list.Add(modNode);

					// prima di aggiungere i nodi delle tabelle faccio il sort alfabetico
					IComparer comp = new SortTreeNodeList();
					myArray.Sort(comp);
					foreach (PlugInTreeNode myNode in myArray)
						modNode.Nodes.Add(myNode);
				}

				// sort alfabetico dei moduli
				IComparer comparer = new SortTreeNodeList();
				list.Sort(comparer);

				foreach (PlugInTreeNode node in list)
					ancestorNode.Nodes[applicationsList.IndexOf(addOnAppDBInfo)].Nodes.Add(node);
			}

			DirTreeView.EndUpdate();

			return result;
		}
		#endregion

		# region Inizializzazione ImagesList
		/// <summary>
		/// funzione per inizializzare le bitmap dei nodi del tree
		/// </summary>
		//---------------------------------------------------------------------------
		private void InitializeImageList()
		{
			myImages = new ImagesListManager().ImageList;

			// inizializzo imagelist del tree
			DirTreeView.ImageList = myImages;

			// inizializzo imagelist della listview
			TablesListView.SmallImageList = myImages;
			TablesListView.LargeImageList = myImages;

			ApplicationPictureBox.Image = myImages.Images[ImagesListManager.GetApplicationBitmapIndex()];
			ModulePictureBox.Image		= myImages.Images[ImagesListManager.GetModuleBitmapIndex()];
			TablePictureBox.Image		= myImages.Images[ImagesListManager.GetTableBitmapIndex()];
			ViewPictureBox.Image		= myImages.Images[ImagesListManager.GetViewBitmapIndex()];
			StoredProcPictureBox.Image	= myImages.Images[ImagesListManager.GetProcedureBitmapIndex()];
		}
		#endregion

		# region Valorizzazione label e controlli vari
		/// <summary>
		/// funzione per valorizzare le label static
		/// </summary>
		//---------------------------------------------------------------------
		private void SetControlsAndText()
		{
			CompanyLabel.Text = string.Format(CompanyLabel.Text, databaseManager.ContextInfo.CompanyName);

			if (databaseManager.StatusDB == DatabaseStatus.EMPTY)
			{
				UpdateButton.Text = string.Format(Strings.DBFormUpdateButtonText, Strings.LblCreate);
				// se si tratta di database Oracle mostro il flag per la creazione delle viste materializzate
				CreateOracleMViewCheckBox.Visible = (databaseManager.ContextInfo.DbType == DBMSType.ORACLE);
				CreateOracleMViewCheckBox.Checked = (databaseManager.ContextInfo.DbType == DBMSType.ORACLE);
				return;
			}

            if ((databaseManager.StatusDB & DatabaseStatus.PRE_40) == DatabaseStatus.PRE_40)
			{
				if (!this.canMigrate)
				{
					UpdateButton.Visible = false;
					InfosLabel.Visible = false;
					ImportDefaultDataCheckBox.Visible = false;
					DefaultConfigComboBox.Visible = false;
					return;
				}
			}

			if (
				((databaseManager.StatusDB == DatabaseStatus.UNRECOVERABLE || databaseManager.StatusDB == DatabaseStatus.NOT_EMPTY) &&
				!databaseManager.ContextInfo.HasSlaves)
				||
				(databaseManager.StatusDB == DatabaseStatus.UNRECOVERABLE || databaseManager.StatusDB == DatabaseStatus.NOT_EMPTY) &&
				(databaseManager.DmsStructureInfo.DmsCheckDbStructInfo.DBStatus == DatabaseStatus.UNRECOVERABLE ||
				databaseManager.DmsStructureInfo.DmsCheckDbStructInfo.DBStatus == DatabaseStatus.NOT_EMPTY)
				)
			{
				UpdateButton.Visible = false;
				InfosLabel.Visible = false;
				ImportDefaultDataCheckBox.Visible = false;
				DefaultConfigComboBox.Visible = false;
				return;
			}

			// se si tratta di database Oracle mostro il flag per la creazione delle viste materializzate
			CreateOracleMViewCheckBox.Visible = (databaseManager.ContextInfo.DbType == DBMSType.ORACLE);
			CreateOracleMViewCheckBox.Checked = (databaseManager.ContextInfo.DbType == DBMSType.ORACLE);

			UpdateButton.Text = string.Format(Strings.DBFormUpdateButtonText, Strings.LblUpdate);
		}
		# endregion

		#region Inizializzazione combo configurazione dati di default
		/// <summary>
		/// inizializzazione combo con la configurazione per i dati di default
		/// </summary>
		//---------------------------------------------------------------------
		private void InitDefaultConfigCombo()
		{
			// se lo stato della console è in HTTP error non consento di importare i dati di default
			// e disabilito il tutto
			if ((this.consoleStatusType & StatusType.RemoteServerError) != StatusType.RemoteServerError)
			{
				ImportDefaultDataCheckBox.Checked = databaseManager.ImportDefaultData;

				StringCollection defaultConf = new StringCollection();
				databaseManager.ImpExpManager.GetDefaultConfigurationList(ref defaultConf);

				foreach (string config in defaultConf)
					DefaultConfigComboBox.Items.Add(config);
	
				if (DefaultConfigComboBox.Items.Count >= 1)
					DefaultConfigComboBox.SelectedIndex = 0;
				else
				{
					DefaultConfigComboBox.Enabled		= false;
					ImportDefaultDataCheckBox.Checked	= false;
					ImportDefaultDataCheckBox.Enabled	= false;
				}
			}
			else
			{
				ImportDefaultDataCheckBox.Checked	= false;
				ImportDefaultDataCheckBox.Enabled	= false;
				DefaultConfigComboBox.Enabled		= false;
				DefaultConfigComboBox.Items.Clear();
			}
		}

		/// <summary>
		/// evento sul checkbox dei dati di default
		/// </summary>
		//-------------------------------------------------------------------------------------------------
		private void ImportDefaultDataCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			DefaultConfigComboBox.Enabled = ((CheckBox)sender).Checked;
		}

		#endregion

		#region Evento sul click del bottone Aggiorna
		/// <summary>
		/// evento agganciato al click sul bottone "Aggiorna". Il plug-in lo intercetta e 
		/// richiama le funzioni di aggiornamento database.
		/// </summary>
		//---------------------------------------------------------------------------
		private void UpdateButton_Click(object sender, System.EventArgs e)
		{
			if (databaseManager == null)
				return;		

			databaseManager.ImportDefaultData = ImportDefaultDataCheckBox.Checked;
			if (databaseManager.ImportDefaultData && DefaultConfigComboBox.Items.Count > 0)
				databaseManager.ImpExpManager.SetDefaultDataConfiguration(DefaultConfigComboBox.SelectedItem.ToString());
			
			// se si tratta del db Oracle valorizzo il flag di creazione delle viste materializzate
			if (databaseManager.ContextInfo.DbType == DBMSType.ORACLE)
				databaseManager.CreateMViewForOracle = CreateOracleMViewCheckBox.Checked;

			if (OnAfterUpdateConfirm != null)
				OnAfterUpdateConfirm(sender, e);
		}
		#endregion

		# region Eventi intercettati sul TreeView
		/// <summary>
		/// localizzazione del nome tabella (o view o stored procedure) tramite tooltip
		/// </summary>
		//---------------------------------------------------------------------------
		private void DirTreeView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			PlugInTreeNode treeNode = DirTreeView.GetNodeAt(e.X, e.Y) as PlugInTreeNode;

			if (treeNode == null ||
				treeNode.Type == DBConstStrings.AddOn ||
				treeNode.Type == DBConstStrings.ModuleStatusOK ||
				treeNode.Type == DBConstStrings.ModuleStatusKO)
			{
				TableToolTip.RemoveAll();
				return;
			}

			string objLocalized = DatabaseLocalizer.GetLocalizedDescription(treeNode.Text, treeNode.Text);

			// se il testo del nodo è diverso dal testo localizzato e tradotto allora
			// faccio apparire il tooltip
			if (string.Compare(treeNode.Text, objLocalized, StringComparison.InvariantCultureIgnoreCase) != 0)
			{
				if (TableToolTip.GetToolTip(DirTreeView) != objLocalized)
					TableToolTip.SetToolTip(DirTreeView, objLocalized);
			}
			else
				TableToolTip.RemoveAll();
		}
		#endregion

		# region LoadDiagnostic
		/// <summary>
		/// Visualizzo nella listview la diagnostica del plugin
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadDiagnostic(Diagnostic diagnostic, DiagnosticType filterType)
		{
			// inizializzazione listview
			TablesListView.Columns.Add(Strings.DBFormInformation, 700, HorizontalAlignment.Left);
			TablesListView.Items.Clear();

			DiagnosticItems items = diagnostic.AllMessages() as DiagnosticItems;
			if (items != null)
			{
				items.Reverse(); // xchè le info nel file sono in ordinamento decrescente
			
				foreach (DiagnosticItem item in items)
				{
					if (item.Type == DiagnosticType.LogOnFile)
						continue;
					ListViewItem itemListView = new ListViewItem();
					itemListView.ImageIndex = ImagesListManager.GetDummyStateBitmapIndex();
					
					// se si tratta di un errore imposto il carattere rosso
					if (item.IsError)
						itemListView.ForeColor = Color.Red;
					
					itemListView.Text = item.FullExplain;
                    TablesListView.Items.Add(itemListView);

					if (WriteLogInfo)
						Debug.WriteLine(item.FullExplain);
				}
			}
        }
		#endregion

		private void DBForm_Load(object sender, EventArgs e)
		{
		}
	}

	#region Sorting TreeView
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
