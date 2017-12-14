using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SecurityAdmin
{
	//=========================================================================
	public enum ImportExportRolesFormState
	{
		Create = 0,
		Import = 1,
		Export = 2
	}

	//=========================================================================
	public partial class ImportExportRolesForm : PlugInsForm
	{
		private const int yOffSet = 24;

		#region Controlli della Form

		private bool check = false;

		#endregion

		#region DataMember Privati

		private SqlConnection sqlOSLConnection = null;
		private string connectionString = string.Empty;
		private int companyId = -1;
		private int indexList = -1;
		private PathFinder pathFinder = null;
		private ImportExportRole import = null;
		private ImportExportRolesFormState formState;

		#endregion

		#region Dichiarazione Eventi

		#region Refresh dell'Albero della Console dopo l'Importazione
		//Evento che mi fa refresh della mia parte di albero della console
		public delegate void AfterImportRoleEventHandler(object sender, int companyId, bool refreshList);
		public event AfterImportRoleEventHandler OnAfterImportRole;

        public delegate bool IsActivatedApp(string application, string functionality);
        public event IsActivatedApp OnIsActivatedApp;

        #endregion

        #region Refresh dell'Albero della Console dopo l'Esportazione
        //Evento che mi fa refresh dopo l'esportazione
        public delegate void AfterExportRoleEventHandler(object sender);
		public event AfterExportRoleEventHandler OnAfterExportRole;
		#endregion

		#endregion

		#region Costruttori
		//---------------------------------------------------------------------
		public ImportExportRolesForm(int aCompanyId, ImportExportRolesFormState aFormState, SqlConnection aSqlOSLConnection, string connectionString)
		{
			InitializeComponent();

			RolesListView.Columns.Add(Strings.Roles, -1, HorizontalAlignment.Left);
			RolesListView.Columns.Add(Strings.Description, -1, HorizontalAlignment.Left);

			formState = aFormState;
			companyId = aCompanyId;

			this.connectionString = connectionString;
			sqlOSLConnection = aSqlOSLConnection;

			ExportButton.Text = Strings.Export;

			SetRoleListFromDB();

			AdjustFormLayout();
		}

		//---------------------------------------------------------------------
		public ImportExportRolesForm(int aCompanyId, ImportExportRolesFormState aFormState, SqlConnection aSqlOSLConnection, string connectionString, PathFinder consolePathFinder)
		{
			InitializeComponent();

			RolesListView.Columns.Add(Strings.Roles, -2, HorizontalAlignment.Left);
			RolesListView.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);

			pathFinder = consolePathFinder;

			companyId = aCompanyId;
			formState = aFormState;

			this.connectionString = connectionString;
			sqlOSLConnection = aSqlOSLConnection;

			if (aFormState == ImportExportRolesFormState.Import)
			{
				ExportButton.Text = Strings.Import;
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.CheckFileExists = true;
				openFileDialog.Multiselect = false;
				openFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Path.Combine(this.pathFinder.GetModuleInfoByName(SecurityConstString.AdministrationConsole, DatabaseLayerConsts.SecurityAdmin).Path, "Xml");
				DialogResult openFileDlgResult = openFileDialog.ShowDialog();

				if (openFileDlgResult != DialogResult.OK)
				{
					RolesListView.Visible = false;
					SelectAllcheckBox.Visible = false;
					ExportButton.Visible = false;
					this.BaseRolesRadioButton.Visible = false;
					this.AdvancedRolesRadioButton.Visible = false;
					ChangeRoleGroup.Visible = false;
					return;
				}

				SetRoleListFromXML(openFileDialog.FileName);
				AdjustFormLayout();
			}
			else
			{
				ExportButton.Text = Strings.CreateDefaultRoles;
				LoadDefaultBaseRoleList();
			}

			RolesListView.Visible = true;
			SelectAllcheckBox.Visible = true;
			ExportButton.Visible = true;
		}
		#endregion

		#region Funzioni di Inizializzazione

		#region Leggo la Lista dei Ruoli già presenti nel DB

		//---------------------------------------------------------------------
		private void AdjustFormLayout()
		{
			this.BaseRolesRadioButton.Visible = false;
			this.AdvancedRolesRadioButton.Visible = false;

			this.SuspendLayout();
			int controlOffSet = RolesListView.Location.Y - yOffSet;
			this.RolesListView.Location = new Point(RolesListView.Location.X, yOffSet);

			this.SelectAllcheckBox.Location = new Point(SelectAllcheckBox.Location.X, SelectAllcheckBox.Location.Y - controlOffSet);
			this.ChangeRoleGroup.Location = new Point(ChangeRoleGroup.Location.X, ChangeRoleGroup.Location.Y - controlOffSet);
			this.ExportButton.Location = new Point(ExportButton.Location.X, ExportButton.Location.Y - controlOffSet);
			this.ResumeLayout();
		}

		//---------------------------------------------------------------------
		public void SetRoleListFromDB()
		{
			RolesListView.Clear();
			RolesListView.Columns.Add(Strings.Roles, -1, HorizontalAlignment.Left);
			RolesListView.Columns.Add(Strings.Description, -1, HorizontalAlignment.Left);

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;

			try
			{
				string sSelect = @"SELECT RoleId, Role, Description FROM MSD_CompanyRoles WHERE 
											CompanyId = @CompanyId AND
											Disabled = 0 and readonly = 0";

				mySqlCommand = new SqlCommand(sSelect, sqlOSLConnection);
				mySqlCommand.Parameters.AddWithValue("@CompanyId", companyId);

				myReader = mySqlCommand.ExecuteReader();
				while (myReader.Read())
				{
					CustomRoleListViewItem roleItem = new CustomRoleListViewItem(Convert.ToInt32(myReader["RoleId"]), companyId, myReader["Role"].ToString());
					if (myReader["Description"] != null)
					{
						roleItem.Description = myReader["Description"].ToString();
						roleItem.SubItems.Add(roleItem.Description);
					}
					RolesListView.Items.Add(roleItem);
				}
				myReader.Close();
				mySqlCommand.Dispose();
			}
			catch (SqlException err)
			{
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();

				if (mySqlCommand != null)
					mySqlCommand.Dispose();

				DiagnosticViewer.ShowError(Strings.Error, err.Message, err.Procedure, err.Number.ToString(), SecurityConstString.SecurityAdminPlugIn);
			}
		}

		#endregion

		#region Leggo la lista dei Ruoli da File XML
		//--------------------------------------------------------------------------------
		public void SetRoleListFromXML(string fileName)
		{
			RolesListView.Clear();

			RolesListView.Columns.Add(Strings.Roles, -1, HorizontalAlignment.Left);
			RolesListView.Columns.Add(Strings.Description, -1, HorizontalAlignment.Left);

			IBaseModuleInfo module = this.pathFinder.GetModuleInfoByName(SecurityConstString.AdministrationConsole, DatabaseLayerConsts.SecurityAdmin);
			if (module == null)
				return;

			import = new ImportExportRole(fileName, module);
			import.Parse();

			foreach (Role role in import.Roles)
			{
				CustomRoleListViewItem roleItem = new CustomRoleListViewItem(-1, companyId, role.RoleName, true, sqlOSLConnection);
				roleItem.Description = role.RoleDescription;
				roleItem.SubItems.Add(role.RoleDescription);
				RolesListView.Items.Add(roleItem);
			}

		}

		#endregion

		#endregion

		#region Bottone Esporta / Importa
		//---------------------------------------------------------------------
		private void ExportButton_Click(object sender, System.EventArgs e)
		{

			if (RolesListView.CheckedItems == null || RolesListView.CheckedItems.Count == 0)
			{
				DiagnosticViewer.ShowWarning(Strings.MissingRoles, SecurityConstString.SecurityAdminPlugIn);
				return;
			}

			this.Cursor = Cursors.WaitCursor;

			if (formState == ImportExportRolesFormState.Import)
				ImportRoleFromXML(sender);
			else if (formState == ImportExportRolesFormState.Export)
				ExportRoleToXML(sender);
            else
                CreateDefaultRoles(sender);

			this.Cursor = Cursors.Default;
		}

        //---------------------------------------------------------------------
        private void CreateDefaultRoles(object sender)
        {

            ArrayList roles = new ArrayList();

            foreach (ListViewItem aItem in RolesListView.CheckedItems)
                roles.Add(aItem.Tag);

            ImportExportFunction.ImportBaseRolesGrantFromXML(roles, sqlOSLConnection, companyId, pathFinder, connectionString);
            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();
            if (OnAfterImportRole != null)
                OnAfterImportRole(sender, companyId, true);
        }


		//---------------------------------------------------------------------
		private void ImportRoleFromXML(object sender)
		{
            ImportExportFunction.ImportDBObjects(this.import.Objects, sqlOSLConnection);

            ArrayList roles = new ArrayList();
            foreach (ListViewItem aItem in RolesListView.CheckedItems)
                roles.Add(aItem);

            bool isOFM = OnIsActivatedApp(SecurityConstString.OFMApplicationName, SecurityConstString.OFMCoreModuleName);
            if (isOFM)
            {
                foreach (ListViewItem aItem in roles)
                {
                    int roleid = ImportExportFunction.GetRoleId(aItem.Text, companyId, sqlOSLConnection);
                    ImportExportFunction.DeleteAllGrantsForRole(companyId, roleid, connectionString);
                }
            
               //Cancello cadaveri nella protectedObjects
                ImportExportFunction.DeleteFromProtectedObjects(companyId, connectionString);
            }


            foreach (ListViewItem aItem in RolesListView.CheckedItems)
                ImportExportFunction.ImportRolesFromXML(this.import.GetRoleByName(aItem.Text), sqlOSLConnection, companyId, connectionString, isOFM);

            BasePathFinder.BasePathFinderInstance.InstallationVer.UpdateCachedDateAndSave();

            if (OnAfterImportRole != null)
				OnAfterImportRole(sender, companyId, true);

		}

		//---------------------------------------------------------------------
		private void ExportRoleToXML(object sender)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.ValidateNames = true;
			saveFileDialog.CheckFileExists = false;
			saveFileDialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";

			DialogResult saveFileDlgResult = saveFileDialog.ShowDialog();

			if (saveFileDlgResult != DialogResult.OK)
				return;

			ArrayList roleIdsList = new ArrayList();
			foreach (CustomRoleListViewItem aItem in RolesListView.CheckedItems)
			{
				if (aItem.Id != -1)
					roleIdsList.Add(aItem.Id);
			}

			ImportExportFunction.ExportRolesInXML(companyId, saveFileDialog.FileName, roleIdsList, sqlOSLConnection, connectionString);

			if (OnAfterExportRole != null)
				OnAfterExportRole(sender);
		}

		//---------------------------------------------------------------------
		#endregion

		#region Selezione di un elemento dall lista

		//---------------------------------------------------------------------
		private void RolesListView_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if (e.NewValue != CheckState.Checked)
				return;

			if (((CustomRoleListViewItem)RolesListView.Items[e.Index]).Exists)
			{
				e.NewValue = CheckState.Unchecked;
				RolesListView.Items[e.Index].Selected = true;
			}
		}

		//---------------------------------------------------------------------
		private void RolesListView_SelectedIndexChanged(object sender, System.EventArgs e)
		{

			if
				(
				RolesListView.SelectedItems != null &&
				RolesListView.SelectedItems.Count == 1 &&
				RolesListView.SelectedItems[0] != null
				)
			{
				ChangeRoleGroup.Enabled = true;
				indexList = RolesListView.SelectedItems[0].Index;
				DescriptionTextBox.Text = ((CustomRoleListViewItem)RolesListView.SelectedItems[0]).Description;

				if (((CustomRoleListViewItem)RolesListView.SelectedItems[0]).Exists)
					NewRoleTextBox.ForeColor = RolesListView.SelectedItems[0].ForeColor;

				NewRoleTextBox.Text = RolesListView.SelectedItems[0].Text;
			}
		}

		#endregion

		#region Bottone x cambiare il nome di un ruolo da importare se nel DB ne è già presente uno con quel nome

		//---------------------------------------------------------------------
		private void ChangeRoleButton_Click(object sender, System.EventArgs e)
		{
			if (indexList == -1)
				return;

			//Controllo che abbiano inserito in nome
			if (NewRoleTextBox.Text.Trim().Length == 0)
			{
				DiagnosticViewer.ShowWarning(Strings.NewRole, SecurityConstString.SecurityAdminPlugIn);
				return;
			}

			if (formState == ImportExportRolesFormState.Import || formState == ImportExportRolesFormState.Create)
			{
				if (RolesListView.Items[indexList] == null)
					return;

				CustomRoleListViewItem item = ((CustomRoleListViewItem)RolesListView.Items[indexList]);

				if (item.Tag == null)
					return;

				Role role = ((Role)item.Tag);

				if (role != null)
				{
					role.RoleNewName = NewRoleTextBox.Text.Trim();
					role.RoleDescription = DescriptionTextBox.Text;
				}
				((CustomRoleListViewItem)RolesListView.Items[indexList]).SetRoleName(NewRoleTextBox.Text.Trim());
			}

			NewRoleTextBox.Text = String.Empty;
			DescriptionTextBox.Text = String.Empty;
			ChangeRoleGroup.Enabled = false;
		}

		#endregion

		//---------------------------------------------------------------------------
		private void SelectAllcheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			check = !check;
			for (int i = 0; i < RolesListView.Items.Count; i++)
			{
				CustomRoleListViewItem listItem = (CustomRoleListViewItem)RolesListView.Items[i];
				if (!listItem.Exists)
					listItem.Checked = check;
			}
		}

		//---------------------------------------------------------------------------
		private void BaseRolesRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (BaseRolesRadioButton.Checked)
				LoadDefaultBaseRoleList();
		}

		//---------------------------------------------------------------------------
		private void AdvancedRolesRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (AdvancedRolesRadioButton.Checked)
				LoadDefaultAdvancedRoleList();
		}

		//---------------------------------------------------------------------------
		private void LoadDefaultAdvancedRoleList()
		{
			RolesListView.Items.Clear();
			this.SuspendLayout();

			ArrayList defaultAdvancedRoles = DefaultAdvancedRoles.GetAllAdvancedRoles();
			for (int i = 0; i < defaultAdvancedRoles.Count; i++)
			{
				string[] a = (string[])defaultAdvancedRoles[i];
				Role role = new Role(a[0], a[1], false, DefaultAdvancedRoles.GetDefaultAdvancedRolesTypeFromRoleName(a[0]), false);
				AddRoleList(role);
			}

			if (RolesListView.Columns.Count > 0)
			{
				for (int i = 0; i < RolesListView.Columns.Count; i++)
					RolesListView.Columns[i].Width = -2;
			}

			this.RolesListView.Update();

			this.ResumeLayout();
		}

		//---------------------------------------------------------------------------
		private void LoadDefaultBaseRoleList()
		{
			RolesListView.Items.Clear();

			this.SuspendLayout();

			Role role = new Role(DefaultBaseRoles.Accounting.ToString(), DefaultBaseRoles.Accounting.ToString(), false, DefaultAdvancedRolesType.Base, false);
			AddRoleList(role);

            role = new Role(DefaultBaseRoles.Configuration.ToString(), DefaultBaseRoles.Configuration.ToString(), false, DefaultAdvancedRolesType.Base, false);
			AddRoleList(role);

            role = new Role(DefaultBaseRoles.Inventory.ToString(), DefaultBaseRoles.Inventory.ToString(), false, DefaultAdvancedRolesType.Base, false);
			AddRoleList(role);

            role = new Role(DefaultBaseRoles.Manufacturing.ToString(), DefaultBaseRoles.Manufacturing.ToString(), false, DefaultAdvancedRolesType.Base, false);
			AddRoleList(role);

            role = new Role(DefaultBaseRoles.Purchases.ToString(), DefaultBaseRoles.Purchases.ToString(), false, DefaultAdvancedRolesType.Base, false);
			AddRoleList(role);

            role = new Role(DefaultBaseRoles.Sales.ToString(), DefaultBaseRoles.Sales.ToString(), false, DefaultAdvancedRolesType.Base, false);
			AddRoleList(role);

			if (RolesListView.Columns.Count > 0)
			{
				for (int i = 0; i < RolesListView.Columns.Count; i++)
					RolesListView.Columns[i].Width = -2;
			}

			this.RolesListView.Update();

			this.ResumeLayout();
		}

		//---------------------------------------------------------------------------
		private void AddRoleList(Role role)
		{
			CustomRoleListViewItem roleItem = new CustomRoleListViewItem(-1, companyId, role.RoleName);

			if (role.RoleDescription != null && role.RoleDescription.Length > 0)
			{
				roleItem.Description = role.RoleDescription;
				roleItem.Tag = role;
				roleItem.SubItems.Add(role.RoleDescription);
			}

			RolesListView.Items.Add(roleItem);
		}

		//---------------------------------------------------------------------------
		private void RolesListView_SizeChanged(object sender, System.EventArgs e)
		{
			this.RolesListView.Update();
		}

		//---------------------------------------------------------------------------
		private void RolesListView_Resize(object sender, System.EventArgs e)
		{

			if (RolesListView.Columns.Count > 0)
			{
				for (int i = 0; i < RolesListView.Columns.Count; i++)
					RolesListView.Columns[i].Width = -2;
			}

			this.RolesListView.Update();
		}
	}

	//=========================================================================
	public class CustomRoleListViewItem : ListViewItem
	{
		private int id = -1;
		private int companyId = -1;
		private bool exists = false;
		private bool checkExistence = false;
		private SqlConnection sqlConnection = null;
		private string description = string.Empty;

		//---------------------------------------------------------------------
		public int Id { get { return id; } }
		//--------------------------------------------------------------------------------
		public bool Exists { get { return exists; } }
		//--------------------------------------------------------------------------------
		public string Description { get { return description; } set { description = value; } }

		//---------------------------------------------------------------------
		public CustomRoleListViewItem(int id, int companyId, string roleName, bool checkExistence, SqlConnection aSqlConnection)
		{
			this.id = id;
			this.companyId = companyId;
			this.checkExistence = checkExistence;
			this.sqlConnection = aSqlConnection;

			SetRoleName(roleName);
		}

		//---------------------------------------------------------------------
		public CustomRoleListViewItem(int aId, int aCompanyId, string aName)
			:
			this(aId, aCompanyId, aName, false, null)
		{
		}

		//---------------------------------------------------------------------
		public void SetRoleName(string aText)
		{
			Text = aText;

			UpdateExistence();
		}

		//---------------------------------------------------------------------
		public void UpdateExistence()
		{
            //exists = checkExistence ? ExistRoleWithTheSameName() : false;
            //ForeColor = exists ? Color.Red : SystemColors.WindowText;
		}

		//---------------------------------------------------------------------
		public bool ExistRoleWithTheSameName()
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return false;

			SqlCommand mySqlCommand = null;
			try
			{
				String sSelect = @"SELECT COUNT(*) FROM MSD_CompanyRoles WHERE
								CompanyId = " + companyId.ToString() + " AND Role = @Role";

				mySqlCommand = new SqlCommand(sSelect, sqlConnection);
				mySqlCommand.Parameters.AddWithValue("@Role", this.Text);
				int recordsCount = (int)mySqlCommand.ExecuteScalar();

				mySqlCommand.Dispose();

				return (recordsCount == 1);
			}
			catch (SqlException)
			{
				if (mySqlCommand != null)
					mySqlCommand.Dispose();

				return false;
			}
		}
	}
}
