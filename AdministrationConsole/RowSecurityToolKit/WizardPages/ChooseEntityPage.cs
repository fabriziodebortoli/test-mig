using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	///<summary>
	/// Pagina per la scelta dell'entità da creare/da modificare/da eliminare
	///</summary>
	//================================================================================
	public partial class ChooseEntityPage : ExteriorWizardPage
	{
		private RSSelections rsSelections = null;

		//--------------------------------------------------------------------------------
		public ChooseEntityPage()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;

			rsSelections = ((RSWizard)this.WizardManager).Selections;
			
			// inizializzo i controls
			SetControlsValue();

			this.WizardForm.SetWizardButtons(WizardButton.Next);

			return true;
		}

		//---------------------------------------------------------------------
		public override string OnWizardNext()
		{
			if (!CheckValues())
				return WizardForm.NoPageChange;

			GetControlsValue();

			// se voglio eliminare l'entita' vado direttamente alla pagina delle priorita' (solo se ne ho piu' d'una)
			if (rsSelections.EntityAction == EntityAction.DELETE)
				return (rsSelections.PrioritiesDictionary.Count > 1) ? "SetEntityPrioritiesPage" :  "SummaryPage";
			
			return base.OnWizardNext();
		}

		//---------------------------------------------------------------------
		private bool CheckValues()
		{
			if (BtnNewEntity.Checked)
			{
				if (string.IsNullOrWhiteSpace(TxtNewEntity.Text))
				{
					DiagnosticViewer.ShowWarning(Strings.MissingEntityName, string.Empty);
					return false;
				}

				char[] charChars = new char[] { '.', ' ' } ;

				if (!Functions.IsValidName(TxtNewEntity.Text) || TxtNewEntity.Text.IndexOfAny(charChars) > -1)
				{
					DiagnosticViewer.ShowCustomizeIconMessage(Strings.InvalidCharsForEntityName, string.Empty);
					return false;
				}

				// se e' gia' presente un entity con il nome non procedo
				if (rsSelections.EntitiesDictionary.ContainsKey(TxtNewEntity.Text))
				{
					DiagnosticViewer.ShowCustomizeIconMessage(string.Format(Strings.DuplicateEntityName, TxtNewEntity.Text), string.Empty);
					return false;
				}
			}

			if (!BtnNewEntity.Checked && (EntitiesComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(EntitiesComboBox.SelectedItem.ToString())))
			{
				DiagnosticViewer.ShowWarning(Strings.MissingEntityName, string.Empty);
				return false;
			}

			return true;
		}

		// viene chiamata solo la prima volta che viene caricata la pagina, e cmq DOPO il metodo OnSetActive
		//--------------------------------------------------------------------------------
		private void ChooseEntityPage_Load(object sender, EventArgs e)
		{
			if (rsSelections == null)
				return;

			// leggo i file di configurazione presenti nell'installazione
			rsSelections.LoadRowSecurityObjectsInfo();

			EntitiesComboBox.DataSource = null;
			EntitiesComboBox.BeginUpdate();
			EntitiesComboBox.Items.Clear();
			if (rsSelections.EntitiesDictionary != null)
				EntitiesComboBox.DataSource = rsSelections.EntitiesDictionary.Keys.ToList();
			EntitiesComboBox.EndUpdate();

			if (EntitiesComboBox.Items.Count > 0)
				EntitiesComboBox.SelectedIndex = 0;
		}

		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			switch (rsSelections.EntityAction)
			{
				case EntityAction.NEW:
					BtnNewEntity.Checked = true;
					TxtNewEntity.Text = rsSelections.Entity;
					break;
				case EntityAction.EDIT:
					BtnEditEntity.Checked = true;
					int i = EntitiesComboBox.FindStringExact(rsSelections.Entity, -1);
					EntitiesComboBox.SelectedIndex = (i >= 0) ? i : 0;
					break;
				case EntityAction.DELETE:
					BtnDeleteEntity.Checked = true;
					int ii = EntitiesComboBox.FindStringExact(rsSelections.Entity, -1);
					EntitiesComboBox.SelectedIndex = (ii >= 0) ? ii : 0;
				break;
			}
		}

		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			if (BtnNewEntity.Checked)
				rsSelections.EntityAction = EntityAction.NEW;
			if (BtnEditEntity.Checked)
				rsSelections.EntityAction = EntityAction.EDIT;
			if (BtnDeleteEntity.Checked)
				rsSelections.EntityAction = EntityAction.DELETE;

			rsSelections.Entity = (rsSelections.EntityAction == EntityAction.NEW) ? TxtNewEntity.Text : EntitiesComboBox.SelectedItem.ToString();
			rsSelections.EntityDescription = (rsSelections.EntityAction == EntityAction.NEW) ? TxtEntityDescription.Text : TxtEditDescri.Text;

			if (rsSelections.EntityAction != EntityAction.NEW)
			{
				// in caso di edit precarico le info dai file di configurazione
				foreach (RowSecurityObjectsInfo item in rsSelections.RowSecurityObjectsList)
				{
					foreach (RSEntity rsent in item.RSEntities)
					{
						if (string.Compare(rsent.Name, rsSelections.Entity, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							rsSelections.MasterTable = rsent.MasterTableName;
							rsSelections.MasterTableNamespace = rsent.MasterTableNamespace;
							rsSelections.DocumentNamespace = rsent.DocumentNamespace;
							rsSelections.Priority = rsent.Priority;

							rsSelections.MasterTblColumns = new List<CatalogColumn>();
							foreach (RSColumn rscol in rsent.RsColumns) // creo una colonna fittizia
								rsSelections.MasterTblColumns.Add(new CatalogColumn(rscol.Name, DBMSType.SQLSERVER));
							break;
						}
					}
				}
			}

			// carico le priorita' esistenti dichiarate nei files di configurazione
			LoadPrioritiesValues();
		}

		///<summary>
		/// Riempio una struttura a parte con le priorita' ed eventuali ri-assegnazioni dei valori in base all'azione che sto effettuando
		///</summary>
		//--------------------------------------------------------------------------------
		private void LoadPrioritiesValues()
		{
			rsSelections.PrioritiesDictionary.Clear();
			
			// riempio la listview con le priorita' lette dagli appositi file (ordinate per priorita')
			foreach (KeyValuePair<string, RSEntityInfo> kvp in rsSelections.EntitiesDictionary.OrderBy(i => i.Value.Priority))
			{
				if (rsSelections.EntityAction == EntityAction.DELETE && string.Compare(kvp.Key, rsSelections.Entity, StringComparison.InvariantCultureIgnoreCase) == 0)
					continue; // skippo l'entita' che sto eliminando
				
				rsSelections.PrioritiesDictionary.Add
					(
						kvp.Key, 
						(rsSelections.EntityAction == EntityAction.DELETE) 
						? ((kvp.Value.Priority > 1) ? (kvp.Value.Priority - 1) : kvp.Value.Priority) // se sto eliminando l'entita' devo rinumerare le altre (faccio -1)
						: kvp.Value.Priority // altrimenti aggiungo semplicemente le esistenti
					);
			}

			// se invece sto aggiungendo una nuova entita' devo gestirla manualmente e imposto il valore piu' grande
			if (rsSelections.EntityAction == EntityAction.NEW)
			{
				int lastPriorityValue = rsSelections.PrioritiesDictionary.Count;
				rsSelections.PrioritiesDictionary.Add(rsSelections.Entity, lastPriorityValue + 1);
			}
		}

		//--------------------------------------------------------------------------------
		private void BtnNewEntity_CheckedChanged(object sender, EventArgs e)
		{
			LblNewEntity.Enabled = ((RadioButton)(sender)).Checked;
			TxtNewEntity.Enabled = ((RadioButton)(sender)).Checked;
			TxtEntityDescription.Enabled = ((RadioButton)(sender)).Checked;

			EntitiesComboBox.Enabled = !((RadioButton)(sender)).Checked;
			LblEditEntity.Enabled = !((RadioButton)(sender)).Checked;

			if (((RadioButton)(sender)).Checked)
				TxtEntityDescription.Clear();
			else
				EntitiesComboBox_SelectedIndexChanged(sender, e);
		}

		//--------------------------------------------------------------------------------
		private void BtnEditEntity_CheckedChanged(object sender, EventArgs e)
		{
			TxtEditDescri.Enabled = ((RadioButton)(sender)).Checked;
		}

		//--------------------------------------------------------------------------------
		private void EntitiesComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (BtnNewEntity.Checked || EntitiesComboBox.SelectedItem == null)
				return;

			string entity = EntitiesComboBox.SelectedItem.ToString();
			if (string.IsNullOrWhiteSpace(entity))
				return;

			RSEntityInfo rs;
			if (rsSelections.EntitiesDictionary.TryGetValue(entity, out rs))
				TxtEditDescri.Text = rs.Description;
		}
	}
}