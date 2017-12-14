using System;
using System.Collections;
using System.Collections.Generic;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	///<summary>
	/// Pagina per la selezione della MasterTable e relative colonne
	///</summary>
	//================================================================================
	public partial class SelectMasterTablePage : InteriorWizardPage
	{
		private RSSelections rsSelections = null;
		private CatalogTableEntry selectedCatTblEntry;

		//--------------------------------------------------------------------------------
		public SelectMasterTablePage()
		{
			InitializeComponent();
		}

		// viene richiamata tutte le volte che visualizzo la pagina
		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			rsSelections = ((RSWizard)this.WizardManager).Selections;

			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);

			return true;
		}

		//---------------------------------------------------------------------
		public override string OnWizardNext()
		{
			if (!CheckData())
				return WizardForm.NoPageChange;

			GetControlsValue();

			return base.OnWizardNext();
		}

		//---------------------------------------------------------------------
		private bool CheckData()
		{
			if (MasterTableComboBox.SelectedItem == null)
			{
				DiagnosticViewer.ShowWarning(Strings.SpecifyExistingTableName, string.Empty);
				return false;
			}

			if (ColumnsListBox.CheckedItems != null && ColumnsListBox.CheckedItems.Count == 0)
			{
				DiagnosticViewer.ShowWarning(Strings.SelectAtLeastOneColumn, string.Empty);
				return false;
			}

			if (
				selectedCatTblEntry == null || 
				string.IsNullOrWhiteSpace(selectedCatTblEntry.Application) || string.IsNullOrWhiteSpace(selectedCatTblEntry.Module) ||
				string.IsNullOrWhiteSpace(selectedCatTblEntry.Namespace)
				)
			{
				DiagnosticViewer.ShowWarning(Strings.TableNotValid, string.Empty);
				return false;
			}

			return true;
		}

		/// per inizializzare i valori dei controls sulla base dei default e delle 
		/// selezioni effettuate dall'utente
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			if (MasterTableComboBox.DataSource != null)
				MasterTableComboBox.DataSource = null;
			MasterTableComboBox.BeginUpdate();
			MasterTableComboBox.Items.Clear();
			MasterTableComboBox.EndUpdate();

			// carico il Catalog del database aziendale
			if (!rsSelections.LoadCatalogInfo())
				return;
			
			MasterTableComboBox.BeginUpdate();
			// riempio la combo con le tabelle registrate ordinate alfabeticamente
			MasterTableComboBox.DataSource = rsSelections.RegisteredTablesList;
			MasterTableComboBox.DisplayMember = ConstStrings.TableName;
			MasterTableComboBox.EndUpdate();

			if (!string.IsNullOrWhiteSpace(rsSelections.MasterTable))
			{
				// mi posiziono sulla tabella precedentemente selezionata
				int i = MasterTableComboBox.FindStringExact(rsSelections.MasterTable, -1);
				MasterTableComboBox.SelectedIndex = (i >= 0) ? i : 0;

				// vado a mettere il Checked = true alle colonne precedentemente selezionate
				// scorro tutte i nomi delle colonne che dovrebbero essere state caricate con il SelectedIndexChanged 
				// delle combo delle tabelle (se presenti)
				if (i >= 0 && ColumnsListBox.Items.Count > 0)
				{
					for (int x = 0; x < ColumnsListBox.Items.Count; x++)
					{
						CatalogColumn cc = (CatalogColumn)ColumnsListBox.Items[x];

						if (rsSelections.EntityAction == EntityAction.NEW)
						{
							// se sono in new devo semplicemente cercare la colonna corrispondente
							if (rsSelections.MasterTblColumns.Contains(cc))
								ColumnsListBox.SetItemChecked(x, true);
						}
						else
						{
							for (int z = 0; z < rsSelections.MasterTblColumns.Count; z++)
							{
								CatalogColumn col = rsSelections.MasterTblColumns[z];
								// se sono in edit devo fare questo trucco perche' ho una lista di colonne fittizie
								// che devo sostituire con le effettive CatalogColumn
								if (string.Compare(col.Name, cc.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
								{
									rsSelections.MasterTblColumns[z] = cc;
									ColumnsListBox.SetItemChecked(x, true);
									ColumnsListBox.SelectedIndex = x;
								}
							}
						}
					}
				}
			}

			// la combo della master table e' editabile se sto creando una nuova entita'
			MasterTableComboBox.Enabled = (rsSelections.EntityAction == EntityAction.NEW);
		}

		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			if (selectedCatTblEntry == null)
				return;

			rsSelections.MasterTable = selectedCatTblEntry.TableName;
			rsSelections.MasterTableNamespace = selectedCatTblEntry.Namespace;
			rsSelections.MasterTblColumns = new List<CatalogColumn>();

			List<string> masterCols = new List<string>(); // mi tengo da parte le colonne selezionate dall'utente
			foreach (CatalogColumn item in ColumnsListBox.CheckedItems)
			{
				rsSelections.MasterTblColumns.Add(item);
				masterCols.Add(item.Name);
			}

			// pulisco la lista delle tabelle+colonne correlate, solo se sono in NEW
			if (rsSelections.EntityAction == EntityAction.NEW && rsSelections.RelatedTablesList != null)
			{
				rsSelections.RelatedTablesList.Clear();
				return;
			}

			// se sono in EDIT devo riproporre le colonne memorizzate nei file piu' quelle eventualmente
			// aggiunte dall'utente
			if (rsSelections.EntityAction == EntityAction.EDIT)
			{
				if (rsSelections.RelatedTablesList == null)
					rsSelections.RelatedTablesList = new List<RSRelatedTable>();
				else
					rsSelections.RelatedTablesList.Clear();

				// leggo le info dell'entity e delle sue relazioni nei file di configurazione
				RSEntityInfo rsei;
				if (rsSelections.EntitiesDictionary.TryGetValue(rsSelections.Entity, out rsei))
				{
					foreach (RSTableInfo ti in rsei.RsTablesInfo)
					{
						// se non esiste la tabella nelle selezioni la aggiungo al dictionary
						// (con successiva scrematura delle colonne, che potrebbero essere variate)
						if (!rsSelections.RelatedTablesList.Exists(ns => string.Compare(ns.TableNamespace, ti.NameSpace, StringComparison.InvariantCultureIgnoreCase) == 0))
						{
							RSRelatedTable relatedTbl = new RSRelatedTable(ti.NameSpace);
							// un nome tabella potrebbe essere presente piu' volte, quindi devo aggiungere le colonne a blocchi
							foreach (RSColumns columns in ti.RsColumns)
							{
								List<string> selCols = new List<string>();
								for (int i = 0; i < masterCols.Count; i++)
								{
									string col = masterCols[i];
									RSColumn rc = columns.RSColumnList.Find(rsc => string.Compare(rsc.EntityColumn, col, StringComparison.InvariantCultureIgnoreCase) == 0);
									selCols.Add((rc == null) ? col : rc.Name);
								}
								relatedTbl.ColumnsList.Add(selCols);
							}

							rsSelections.RelatedTablesList.Add(relatedTbl);
						}
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void SelectMasterTablePage_Load(object sender, EventArgs e)
		{
			// imposto l'immagine nella picturebox, seppure non visibile
			AlertPictureBox.Image = ((RSWizard)this.WizardManager).StateImageList.Images[PlugInTreeNode.GetResultRedStateImageIndex];
		}

		//--------------------------------------------------------------------------------
		private void MasterTableComboBox_Validated(object sender, EventArgs e)
		{
			// se il selectedItem e' null significa che ho scritto qualche castroneria nella combobox
			// quindi visualizzo una bitmap e metto readonly l'array delle colonne
			AlertPictureBox.Visible = (MasterTableComboBox.SelectedItem == null);
			ColumnsListBox.Enabled = (MasterTableComboBox.SelectedItem != null);
		}

		///<summary>
		/// Sulla selezione della tabella dall'apposita combobox precarico le sue colonne
		/// e i namespace dei documenti dichiarati dal modulo di appartenenza
		///</summary>
		//--------------------------------------------------------------------------------
		private void MasterTableComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			AlertPictureBox.Visible = (MasterTableComboBox.SelectedItem == null);
			ColumnsListBox.Enabled = (MasterTableComboBox.SelectedItem != null);

			if (MasterTableComboBox.SelectedItem == null)
				return;

			selectedCatTblEntry = (CatalogTableEntry)MasterTableComboBox.SelectedItem;
			if (selectedCatTblEntry == null)
				return;

			try
			{
				// carico le colonne della tabella ordinate alfabeticamente nella listbox
				IComparer colComparer = new CatalogColumnComparer();
				selectedCatTblEntry.ColumnsInfo.Sort(colComparer);

				ColumnsListBox.BeginUpdate();
				// talvolta l'assegnazione del DataSource puo' generare nell'output questo msg:
				// A first chance exception of type 'System.NullReferenceException' occurred in System.Windows.Forms.dll
				// ma e' un'eccezione a basso livello della CheckedListBox.RefreshItems. quindi NO Fear!
				ColumnsListBox.DataSource = selectedCatTblEntry.ColumnsInfo;
				ColumnsListBox.DisplayMember = ConstStrings.Name;
				ColumnsListBox.EndUpdate();

				// rimuovo tutte le selezioni precedenti
				while (ColumnsListBox.CheckedIndices.Count > 0)
					ColumnsListBox.SetItemChecked(ColumnsListBox.CheckedIndices[0], false);
			}
			catch (NullReferenceException)
			{ }
			catch (Exception ex)
			{
				DiagnosticViewer.ShowCustomizeIconMessage(string.Format(Strings.ErrLoadingColsForTable, selectedCatTblEntry.TableName) + " " + ex.Message, string.Empty);
			}
		}
	}
}