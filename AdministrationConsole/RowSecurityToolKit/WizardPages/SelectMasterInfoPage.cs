using System;
using System.Collections.Generic;

using Microarea.Console.Core.DBLibrary;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	///<summary>
	/// Pagina per la selezione dei namespace correlati alla MasterTable ed alle sue colonne
	/// Specificatamente si parla di namespace del documento, dell'hotlink e del numeratore
	///</summary>
	//================================================================================
	public partial class SelectMasterInfoPage : InteriorWizardPage
	{
		private RSSelections rsSelections = null;
		private CatalogTableEntry masterTableCatalogEntry = null;
		private IBaseModuleInfo modInfo = null;

		//--------------------------------------------------------------------------------
		public SelectMasterInfoPage()
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

		/// per inizializzare i valori dei controls sulla base dei default e delle 
		/// selezioni effettuate dall'utente
		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			// mostro la MasterTable e le colonne di riferimento selezionate
			LblMasterTable.Text = string.Format(Strings.MasterTable, rsSelections.MasterTable);
			string columnsText = Strings.SelectedColumns;
			foreach (CatalogColumn cc in rsSelections.MasterTblColumns)
			{
				columnsText += string.Format(" {0} {1} ", cc.Name, cc.DataTypeName);
				columnsText += cc.HasLength() ? string.Format("({0}) ; ", cc.ColumnSize.ToString()) : ";";
			}
			LblColumns.Text = columnsText.Substring(0, columnsText.Length - 2);

			masterTableCatalogEntry = rsSelections.GetRegisteredTableEntry(rsSelections.MasterTable);
			modInfo = rsSelections.ContextInfo.PathFinder.GetModuleInfoByName(masterTableCatalogEntry.Application, masterTableCatalogEntry.Module);

			// carico nella combobox i namespace dei documenti dichiarati nel modulo
			PopulateDocNsComboBox();
		}

		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			rsSelections.DocumentNamespace = DocNsComboBox.Text;			
		}

		//---------------------------------------------------------------------
		private bool CheckData()
		{
			if (string.IsNullOrWhiteSpace(DocNsComboBox.Text))
			{
				DiagnosticViewer.ShowWarning(Strings.SpecifyDocumentNamespace, string.Empty);
				return false;
			}

			NameSpace docNs = new NameSpace(DocNsComboBox.Text, NameSpaceObjectType.Document);
			if (!docNs.IsValid())
			{
				DiagnosticViewer.ShowWarning(Strings.SpecifyDocumentNamespace, string.Empty);
				return false;
			}

			return true;
		}

		///<summary>
		/// Caricamento namespace documento dentro la combobox, con selezione del piu' prossimo
		///</summary>
		//--------------------------------------------------------------------------------
		private void PopulateDocNsComboBox()
		{
			if (modInfo == null)
				return;

			List<string> docsNs = new List<string>();
			int count = -1;
			int idx = -1;

			// leggo dal file DocumentObjects.xml tutti i documenti e li conto
			foreach (DocumentInfo docInfo in modInfo.DocumentObjectsInfo.Documents)
			{
				if (docInfo.IsDataEntry) // considero solo i documenti
				{
					docsNs.Add(docInfo.NameSpace.GetNameSpaceWithoutType().ToString());
					count++;

					// per ogni documento vado a leggere il Dbts.xml e vado a vedere se trovo
					// un dbtmaster con il nome di quella tabella
					DbtsObjects dbtsObjects = new DbtsObjects(rsSelections.ContextInfo.PathFinder.GetDbtsPath(docInfo.NameSpace));
					if (dbtsObjects.ParseMasterTable())
					{
						if (
							!string.IsNullOrWhiteSpace(dbtsObjects.DBTMasterTable) &&
							string.Compare(dbtsObjects.DBTMasterTable, masterTableCatalogEntry.TableName, StringComparison.InvariantCultureIgnoreCase) == 0
							)
							idx = count;
					}
				}
			}

			DocNsComboBox.BeginUpdate();
			DocNsComboBox.DataSource = docsNs;
			DocNsComboBox.EndUpdate();
			// se ho trovato un DBTMaster lo seleziono, altrimenti mi metto sul primo
			if (docsNs.Count > 0)
				DocNsComboBox.SelectedIndex = (idx > -1) ? idx : 0;

			if (!string.IsNullOrWhiteSpace(rsSelections.DocumentNamespace))
			{
				// mi posiziono sul namespace precedentemente selezionato e se non lo trovo lo aggiungo a mano
				int i = DocNsComboBox.FindStringExact(rsSelections.DocumentNamespace, -1);
				if (i >= 0)
					DocNsComboBox.SelectedIndex = i;
				else
				{
					docsNs.Add(rsSelections.DocumentNamespace);
					DocNsComboBox.BeginUpdate();
					DocNsComboBox.DataSource = null; // devo mettere a null il DataSource prima di assegnarlo nuovamente
					DocNsComboBox.DataSource = docsNs;
					DocNsComboBox.EndUpdate();
					DocNsComboBox.SelectedIndex = DocNsComboBox.FindStringExact(rsSelections.DocumentNamespace, -1);
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void SelectMasterInfoPage_Load(object sender, EventArgs e)
		{
			// imposto le immagini nelle picturebox, seppure non visibili
			AlertPictureBox.Image = ((RSWizard)this.WizardManager).StateImageList.Images[PlugInTreeNode.GetResultRedStateImageIndex];
		}

		//--------------------------------------------------------------------------------
		private void DocNsComboBox_Validated(object sender, EventArgs e)
		{
			// se il selectedItem e' null significa che ho scritto qualche castroneria nella combobox e visualizzo la bitmap
			AlertPictureBox.Visible = (DocNsComboBox.SelectedItem == null);
		}

		//--------------------------------------------------------------------------------
		private void DocNsComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			AlertPictureBox.Visible = (DocNsComboBox.SelectedItem == null);
		}
	}
}
