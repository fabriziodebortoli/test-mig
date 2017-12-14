using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.UI.WizardDialogLib;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Console.Plugin.RowSecurityToolKit.WizardPages
{
	///<summary>
	/// Pagina per l'impostazione delle priorita' tra le entita'
	///</summary>
	//================================================================================
	public partial class SetEntityPrioritiesPage : InteriorWizardPage
	{
		private RSSelections rsSelections = null;

		//--------------------------------------------------------------------------------
		public SetEntityPrioritiesPage()
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
			GetControlsValue();

			return base.OnWizardNext();
		}
		
		//---------------------------------------------------------------------
		public override string OnWizardBack()
		{
			if (rsSelections.EntityAction == EntityAction.DELETE)
				return "ChooseEntityPage";

			return base.OnWizardBack();
		}

		//---------------------------------------------------------------------
		private void GetControlsValue()
		{
			// assegno al dictionary i nuovi valori delle priorita'
			foreach (ListViewItem lvi in EntitiesListView.Items)
				rsSelections.PrioritiesDictionary[lvi.Text] = Convert.ToInt16(lvi.SubItems[1].Text);
		}

		//---------------------------------------------------------------------
		private void SetControlsValue()
		{
			BtnMoveUp.Enabled = BtnMoveDown.Enabled = false;
			InitListView();
		}

		//--------------------------------------------------------------------------------
		private void InitListView()
		{
			EntitiesListView.Items.Clear();

			EntitiesListView.BeginUpdate();

			foreach (KeyValuePair<string, int> kvp in rsSelections.PrioritiesDictionary)
			{
				ListViewItem lvi = new ListViewItem();
				lvi.Text = kvp.Key;
				lvi.SubItems.Add(kvp.Value.ToString());
				EntitiesListView.Items.Add(lvi);
			}

			EntitiesListView.EndUpdate();
		}

		//--------------------------------------------------------------------------------
		private void EntitiesListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (EntitiesListView.SelectedIndices.Count > 0)
			{
				int idx = EntitiesListView.SelectedIndices[0];
				BtnMoveUp.Enabled = (idx > 0);
				BtnMoveDown.Enabled = (idx < EntitiesListView.Items.Count - 1);
			}
		}

		// generico metodo che si occupa di spostare un listviewitem in alto o in basso
		//--------------------------------------------------------------------------------
		private void MoveListViewItem(bool moveUp = false)
		{
			if (EntitiesListView.SelectedItems.Count <= 0)
				return;

			int idx = EntitiesListView.SelectedIndices[0];

			ListViewItem selectedItem = EntitiesListView.Items[idx];
			EntitiesListView.Items.Remove(selectedItem);
			EntitiesListView.Items.Insert(moveUp ? (idx - 1) : (idx + 1), selectedItem);

			// ri-numero tutte le priorita'
			for (int i = 0; i < EntitiesListView.Items.Count; i++)
				EntitiesListView.Items[i].SubItems[1].Text = (i + 1).ToString();
		}

		//--------------------------------------------------------------------------------
		private void BtnMoveDown_Click(object sender, EventArgs e)
		{
			MoveListViewItem();
		}

		//--------------------------------------------------------------------------------
		private void BtnMoveUp_Click(object sender, EventArgs e)
		{
			MoveListViewItem(true);
		}

		//--------------------------------------------------------------------------------
		private void BtnMoveUp_MouseHover(object sender, EventArgs e)
		{
			PrioritiesToolTip.SetToolTip((Button)sender, Strings.MoveUpStr);
		}

		//--------------------------------------------------------------------------------
		private void BtnMoveDown_MouseHover(object sender, EventArgs e)
		{
			PrioritiesToolTip.SetToolTip((Button)sender, Strings.MoveDownStr);
		}
	}
}
