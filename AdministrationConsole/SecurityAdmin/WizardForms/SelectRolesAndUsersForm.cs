using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.UI.WizardDialogLib;

namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{
	public partial class SelectRolesAndUsersForm : InteriorWizardPage
	{

		#region Data Member
		private WizardParameters	wizardParameters	= null;
		private bool allRoles = false;
		private bool allUsers = false;

		#endregion 

		#region Costruttore
		//---------------------------------------------------------------------
		public SelectRolesAndUsersForm()
		{
			InitializeComponent();
		}
		//---------------------------------------------------------------------
		#endregion

		#region form attiva quindi alla visualizzazione
        public override bool OnSetActive()
		{
			if (!base.OnSetActive())
				return false;
 
			wizardParameters = ((SecurityWizardManager)this.WizardManager).GetImportSelections();

			if (wizardParameters == null)
				return false;
			
			if ((wizardParameters.UsersArrayList == null || wizardParameters.UsersArrayList.Count == 0) &&
				(wizardParameters.RolesArrayList == null || wizardParameters.RolesArrayList.Count == 0)
				)
			{
				InitializeMyControls();
				this.WizardForm.SetWizardButtons(WizardButton.Back |WizardButton.Next);
			}
			else
				LoadOldSelections();
			
			return true;
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzioni d'inizializzazione

		private void InitializeMyControls()
		{
            this.m_titleLabel.Text = WizardStringMaker.GetSelectRoleAndUsersTitle(wizardParameters.OperationType);
            this.m_subtitleLabel.Text = WizardStringMaker.GetSelectRoleAndUsersDescription(wizardParameters.OperationType);
			
			this.AllUsersButton.Text = Strings.UnselectAll;
			this.AllRolesButton.Text = Strings.UnselectAll;

			FillRolesListView();
			FillUsersListView();

			if (this.RolesListView.Items.Count == 0)
				this.AllRolesButton.Enabled = false;

			if (this.UsersListView.Items.Count == 0)
				this.AllUsersButton.Enabled = false;

			ApplyAllListViewItems(UsersListView, true);
			ApplyAllListViewItems(RolesListView, true);
		}
		//---------------------------------------------------------------------
		public void FillRolesListView()
		{
			RolesListView.Items.Clear();
			RolesListView.Columns.Add(Strings.Roles, -1 ,HorizontalAlignment.Left);
			if (wizardParameters.ShowObjectsTreeForm == null || 
				wizardParameters.ShowObjectsTreeForm.Connection == null || 
				wizardParameters.ShowObjectsTreeForm.Connection.State != ConnectionState.Open)
				return;

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;
			
			try
			{
				string sSelect ="SELECT RoleId, Role, Disabled FROM MSD_CompanyRoles WHERE CompanyId = @CompanyId and Readonly = 0 order by role";
			
				mySqlCommand = new SqlCommand(sSelect, wizardParameters.ShowObjectsTreeForm.Connection);

                mySqlCommand.Parameters.AddWithValue("@CompanyId", wizardParameters.ShowObjectsTreeForm.CompanyId);
				myReader = mySqlCommand.ExecuteReader();
				while (myReader.Read())
				{
					ListViewItem roleItem = RolesListView.Items.Add(new ListViewItem(myReader["Role"].ToString()));
					roleItem.Tag = Convert.ToInt32(Convert.ToInt32(myReader["RoleId"]));
					roleItem.Checked = false;
					if (Convert.ToInt32(myReader["Disabled"]) == 1)
						roleItem.ForeColor = Color.Red;
				}
			} 
			catch (SqlException err)
			{
				DiagnosticViewer.ShowError(Strings.Error, err.Message,  err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
			}
		}
		//---------------------------------------------------------------------
		public void FillUsersListView()
		{
			UsersListView.Items.Clear();
			UsersListView.Columns.Add(Strings.Users, -1 ,HorizontalAlignment.Left);
			if (wizardParameters.ShowObjectsTreeForm == null || 
				wizardParameters.ShowObjectsTreeForm.Connection == null || 
				wizardParameters.ShowObjectsTreeForm.Connection.State != ConnectionState.Open)
				return;

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;
			try
			{
				string sSelect = @"SELECT MSD_CompanyLogins.LoginId, MSD_Logins.Login, MSD_CompanyLogins.Disabled
									FROM MSD_CompanyLogins INNER JOIN MSD_Logins ON 
							MSD_Logins.LoginId = MSD_CompanyLogins.LoginId
								WHERE MSD_CompanyLogins.CompanyId = " +  wizardParameters.ShowObjectsTreeForm.CompanyId.ToString() +
					" ORDER BY MSD_Logins.Login";
			
				mySqlCommand = new SqlCommand(sSelect,wizardParameters.ShowObjectsTreeForm.Connection);

				myReader = mySqlCommand.ExecuteReader();
				while (myReader.Read())
				{
					ListViewItem userItem = UsersListView.Items.Add(new ListViewItem(myReader["Login"].ToString()));
					userItem.Tag = Convert.ToInt32(myReader["LoginId"]);
					userItem.Checked = false;
					if (Convert.ToInt32(myReader["Disabled"]) == 1)
						userItem.ForeColor = Color.Red;
				}
			} 
			catch (SqlException err)
			{
				DiagnosticViewer.ShowError(Strings.Error, err.Message,  err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);
			}
			finally
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
			}
		}
		//---------------------------------------------------------------------
		private void LoadOldSelections()
		{
			//x prima cosa guardo che tipo di selezione era
			if (wizardParameters.UsersOrRolesSelectionType == WizardParametersType.AllUsersAndRoles)
			{
				this.AllUsersButton.Text = Strings.UnselectAll;
				this.AllRolesButton.Text = Strings.UnselectAll;

				allRoles = true;
				allUsers = true;

				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
				return;
			}
			else if (wizardParameters.UsersOrRolesSelectionType == WizardParametersType.SelectedUserOrRoleByTree)
				SelectedRoleByTreecheck.Checked = true;
			else
			{
				if (wizardParameters.UsersOrRolesSelectionType == WizardParametersType.AllRoles)
				{
					this.AllRolesButton.Text = Strings.UnselectAll;
					this.AllUsersButton.Text = Strings.SelectAll;

					allRoles = true;

					ApplyAllListViewItems(RolesListView, true);
					CheckItem(wizardParameters.UsersArrayList, UsersListView);
				}
				if (wizardParameters.UsersOrRolesSelectionType == WizardParametersType.AllUsers)
				{
					this.AllRolesButton.Text = Strings.SelectAll;
					this.AllUsersButton.Text = Strings.UnselectAll;

					allUsers = true;

					ApplyAllListViewItems(UsersListView, true);
					CheckItem(wizardParameters.RolesArrayList, RolesListView);
				}
			
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
			}
		}
		//---------------------------------------------------------------------
		private void CheckItem(ArrayList selectionArrayList, ListView listView)
		{
			foreach (ListViewItem arrayItem in selectionArrayList)
			{
				foreach (ListViewItem item in listView.Items)
				{
					if (Convert.ToInt32(arrayItem.Tag) == Convert.ToInt32(item.Tag))
						item.Checked = true;
				}
			}
		}
		//---------------------------------------------------------------------
		#endregion

		#region funzioni sui ceckBox della form
		//---------------------------------------------------------------------
		private void ApplyAllListViewItems (ListView list, bool check)
		{
			foreach(ListViewItem item in list.Items)
				item.Checked = check;
		}
		//---------------------------------------------------------------------
		private void RolesListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			SetFlag((ListView)sender, e, UsersListView);
		}
		//---------------------------------------------------------------------
		private void UsersListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			SetFlag((ListView)sender, e, RolesListView);
		}
		//---------------------------------------------------------------------
		private void SetFlag(ListView list, System.Windows.Forms.ItemCheckEventArgs e, ListView oderList)
		{
			CheckBox checkBox = new CheckBox();

			if (list == this.UsersListView)
				checkBox.Checked = allUsers;
			else
				checkBox.Checked = allRoles;

			//Disabilito il flag
			if (e.NewValue == CheckState.Unchecked) 
				checkBox.Checked = false;

			//Seleziono il flag
			int tot = list.CheckedItems.Count;

			if (e.NewValue == CheckState.Unchecked)
				tot = tot - 1;
			else
				tot = tot + 1;

			if (tot == list.Items.Count)
				checkBox.Checked = true;

			if (list == this.UsersListView)
			{
				allUsers = checkBox.Checked;
				if (allUsers)
					this.AllUsersButton.Text = Strings.UnselectAll;
				else
					this.AllUsersButton.Text = Strings.SelectAll;
			}
			else
			{
				allRoles = checkBox.Checked;
				if (allRoles)
					this.AllRolesButton.Text = Strings.UnselectAll;
				else
					this.AllRolesButton.Text = Strings.SelectAll;
			}
			if (list.CheckedItems.Count <= 1 && e.NewValue == CheckState.Unchecked && oderList.CheckedItems.Count == 0)
				this.WizardForm.SetWizardButtons(WizardButton.Back);
			else
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}
		//---------------------------------------------------------------------
		#endregion

		#region form disattiva quindi quando clicco su avanti
		//---------------------------------------------------------------------
        public override bool OnKillActive()
		{
			SaveWizardParameters();
			
			return base.OnKillActive();
		}
		//---------------------------------------------------------------------
		private void SaveWizardParameters()
		{
			if (wizardParameters == null)
				wizardParameters = new WizardParameters();

			//Utente e/o Ruolo Corrente
			if (SelectedRoleByTreecheck.Checked)
			{
				wizardParameters.UsersOrRolesSelectionType = WizardParametersType.SelectedUserOrRoleByTree;
				SaveCurrentUserOrRole(wizardParameters.ShowObjectsTreeForm.IsRoleLogin);
				return;
			}

			//Tutti i Ruoli e Tutti gli oggetti
			if (allUsers && allRoles)
				wizardParameters.UsersOrRolesSelectionType = WizardParametersType.AllUsersAndRoles;
			
			//Tuttti gli Utenti
			if (allUsers)
				wizardParameters.UsersOrRolesSelectionType = WizardParametersType.AllUsers;

			//Tuttti i Ruoli
			if (allRoles)
				wizardParameters.UsersOrRolesSelectionType = WizardParametersType.AllRoles;

			if (!SelectedRoleByTreecheck.Checked && !allUsers && !allRoles)
				wizardParameters.UsersOrRolesSelectionType = WizardParametersType.SelectedUsersAndRoles;

			SaveSelections(RolesListView, wizardParameters.RolesArrayList);
			
			SaveSelections(UsersListView, wizardParameters.UsersArrayList);
		}
		//---------------------------------------------------------------------
		private void SaveCurrentUserOrRole(bool IsRoleLogin)
		{
			ListViewItem selectedItem = new ListViewItem();
			selectedItem.Tag = wizardParameters.ShowObjectsTreeForm.RoleOrUserId;
			if (IsRoleLogin)
				wizardParameters.RolesArrayList.Add(selectedItem);
			else
				wizardParameters.UsersArrayList.Add(selectedItem);
			
		}
		//---------------------------------------------------------------------
		private void SaveSelections(ListView list, ArrayList array)
		{
			//Ora mi memorizzo le selezioni dell'utente nei due array
			if (array == null)
				array = new ArrayList();
			else
				array.Clear();

			foreach (ListViewItem item in list.CheckedItems)
				array.Add(item);
		}


		private void SelectRolesAndUsersForm_Load(object sender, System.EventArgs e)
		{
		
		}

		#endregion

		private void AllRolesButton_Click(object sender, System.EventArgs e)
		{
			if (!allRoles)
			{
				ApplyAllListViewItems(RolesListView, true);
				AllRolesButton.Text = Strings.UnselectAll;
				allRoles = true;
			}
			else
			{
				AllRolesButton.Text = Strings.SelectAll;
				ApplyAllListViewItems(RolesListView, false);
				allRoles = false;
			}

			if (UsersListView.CheckedItems.Count >0 || RolesListView.CheckedItems.Count >0)
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}

		//---------------------------------------------------------------------
		private void AllUsersButton_Click(object sender, System.EventArgs e)
		{
			if (!allUsers)
			{
				AllUsersButton.Text = Strings.UnselectAll;
				ApplyAllListViewItems(UsersListView, true);
				allUsers = true;
			}
			else
			{
				AllUsersButton.Text = Strings.SelectAll;
				ApplyAllListViewItems(UsersListView, false);
				allUsers = false;
			}

			if (UsersListView.CheckedItems.Count >0 || RolesListView.CheckedItems.Count >0)
				this.WizardForm.SetWizardButtons(WizardButton.Back | WizardButton.Next);
		}


		//---------------------------------------------------------------------
		private void SelectedRoleByTreecheck_CheckedChanged(object sender, System.EventArgs e)
		{
			if(((RadioButton)sender).Checked )
			{
				RolesListView.Enabled = false;
				UsersListView.Enabled = false;
				this.AllRolesButton.Enabled = false;
				this.AllUsersButton.Enabled = false;
			}
			else
			{
				RolesListView.Enabled = true;
				UsersListView.Enabled = true;
				this.AllRolesButton.Enabled = true;
				this.AllUsersButton.Enabled = true;
			}

		}

		//---------------------------------------------------------------------
		private void SelectRolesAndUsersCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			if(((RadioButton)sender).Checked)
			{
				RolesListView.Enabled = true;
				UsersListView.Enabled = true;
			}
			else
			{
				RolesListView.Enabled = false;
				UsersListView.Enabled = false;
			}
			
		}
		
	}
}

