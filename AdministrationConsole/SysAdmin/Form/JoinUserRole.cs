using System;
using System.Collections;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// JoinUserRole
	/// Associazione di un Utente al Ruolo di una Azienda
	/// </summary>
	//=========================================================================
	public partial class JoinUserRole : PlugInsForm
	{
		private string connectionString;
		private SqlConnection currentConnection;
		private string companyId;
		private string roleId;
		private Diagnostic diagnostic = new Diagnostic("SysAdminPlugIn.JoinUserRole");

		//---------------------------------------------------------------------
		public delegate void ModifyTreeOfCompanies(object sender, string nodeType, string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;
		//---------------------------------------------------------------------
		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;
		
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="action"></param>
		//---------------------------------------------------------------------
		public JoinUserRole(string connectionString, SqlConnection connection, string companyId, string roleId, ImageList treeImages)
		{
			InitializeComponent();
			this.connectionString = connectionString;
			this.currentConnection = connection;
			this.companyId = companyId;
			this.roleId = roleId;

			groupBox1.Visible = true;
			rbAllUsers.Checked = true;

			listViewUsersCompany.View = View.Details;
			listViewUsersCompany.CheckBoxes = true;
			listViewUsersCompany.AllowColumnReorder = true;
			listViewUsersCompany.Sorting = System.Windows.Forms.SortOrder.Ascending;
			listViewUsersCompany.SmallImageList = listViewUsersCompany.LargeImageList = treeImages;
			listViewUsersCompany.Columns.Clear();
			listViewUsersCompany.Columns.Add(Strings.User, -2, HorizontalAlignment.Left);
			listViewUsersCompany.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
			listViewUsersCompany.Items.Clear();

			//default carico tutti gli utenti
			LoadAllUsers();
		}

		#region LoadAllUsers - Mostra tutti gli Utenti associati all'Azienda (tra cui scegliere quelli da associare al Ruolo)
		/// <summary>
		/// LoadAllUsers
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadAllUsers()
		{
			listViewUsersCompany.Items.Clear();
			CompanyUserDb companyUserDb = new CompanyUserDb();
			companyUserDb.ConnectionString = this.connectionString;
			companyUserDb.CurrentSqlConnection = this.currentConnection;

			ArrayList usersCompany = new ArrayList();
			bool result = companyUserDb.SelectAll(out usersCompany, companyId);

			if (!result)
			{
				diagnostic.Set(companyUserDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				usersCompany.Clear();
			}

			foreach (CompanyUser itemUserCompany in usersCompany)
			{
				if (string.Compare(itemUserCompany.Login, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0 ||
					string.Compare(itemUserCompany.Login, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
					continue;

				CompanyRoleLoginItem item = new CompanyRoleLoginItem();
				item.Text = itemUserCompany.Login;
				item.SubItems.Add(itemUserCompany.Description.Replace("\r\n", " "));
				item.LoginId = itemUserCompany.LoginId;
				item.CompanyId = itemUserCompany.CompanyId;
				item.RoleId = roleId;
				item.Checked = ExistJoin(itemUserCompany.LoginId, itemUserCompany.CompanyId, this.roleId);

				if (item.Checked)
					item.ForeColor = Color.Blue;
				if (itemUserCompany.Disabled)
					item.ForeColor = Color.Red;

				item.ImageIndex = (itemUserCompany.WindowsAuthentication) ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;
				listViewUsersCompany.Items.Add(item);
			}
		}
		#endregion

		#region ExistJoin - Per l'utente specificato, verifica se è già stato associato al Ruolo
		/// <summary>
		/// ExistJoin
		/// </summary>
		//---------------------------------------------------------------------
		private bool ExistJoin(string loginId, string companyId, string roleId)
		{
			bool checkedBox = false;
			CompanyRoleLoginDb companyRoleLoginDb = new CompanyRoleLoginDb();
			companyRoleLoginDb.ConnectionString = connectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentConnection;
			if (companyRoleLoginDb.ExistUserCompany(loginId, roleId, companyId) > 0)
				checkedBox = true;
			return checkedBox;
		}

		#endregion

		#region UsersOfRole - Mostra solo gli Utenti già associati al Ruolo
		/// <summary>
		/// UsersOfRole
		/// </summary>
		//---------------------------------------------------------------------
		private void UsersOfRole()
		{
			listViewUsersCompany.Items.Clear();

			CompanyRoleLoginDb companyRoleLoginDb = new CompanyRoleLoginDb();
			companyRoleLoginDb.ConnectionString = connectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentConnection;

			ArrayList usersCompanyRoles = new ArrayList();
			bool result = companyRoleLoginDb.SelectAll(out usersCompanyRoles, companyId, roleId);

			if (!result)
			{
				diagnostic.Set(companyRoleLoginDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				usersCompanyRoles.Clear();
			}

			foreach (CompanyRoleLogin itemUser in usersCompanyRoles)
			{
				CompanyRoleLoginItem item = new CompanyRoleLoginItem();
				item.Text = itemUser.Login;
				item.SubItems.Add(itemUser.Description.Replace("\r\n", " "));
				item.Checked = true;
				item.LoginId = itemUser.LoginId;
				item.CompanyId = itemUser.CompanyId;
				item.RoleId = itemUser.RoleId;
				item.ForeColor = (itemUser.Disabled) ? Color.Red : Color.Blue;
				item.ImageIndex = (itemUser.WindowsAuthentication) ? PlugInTreeNode.GetLoginsDefaultImageIndex : PlugInTreeNode.GetUserDefaultImageIndex;
				listViewUsersCompany.Items.Add(item);
			}
		}
		#endregion

		#region Save - Effettua il salvataggio degli utenti associati al Ruolo
		/// <summary>
		/// Save
		/// </summary>
		//---------------------------------------------------------------------
		public void Save(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			
			CompanyRoleLoginDb companyRoleLoginDb = new CompanyRoleLoginDb();
			companyRoleLoginDb.ConnectionString = connectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentConnection;
			
			for (int i = 0; i < listViewUsersCompany.Items.Count; i++)
			{
				CompanyRoleLoginItem companyRoleLoginItem = (CompanyRoleLoginItem)listViewUsersCompany.Items[i];
				if (companyRoleLoginItem.Checked)
				{
					//se l'ho selezionato e l'utente non c'è nel DB significa che lo devo inserire
					if (companyRoleLoginDb.ExistUserCompany(companyRoleLoginItem.LoginId, this.roleId, companyRoleLoginItem.CompanyId) == 0)
					{
						bool result = companyRoleLoginDb.Add(companyRoleLoginItem.LoginId, companyRoleLoginItem.CompanyId, companyRoleLoginItem.RoleId);
						if (!result)
						{
							diagnostic.Set(companyRoleLoginDb.Diagnostic);
							DiagnosticViewer.ShowDiagnostic(diagnostic);
							if (OnSendDiagnostic != null)
							{
								OnSendDiagnostic(sender, diagnostic);
								diagnostic.Clear();
							}
						}
						else
						{
							if (OnModifyTreeOfCompanies != null)
								OnModifyTreeOfCompanies(sender, ConstString.containerCompanyRoles, this.companyId);
						}
					}
				}
				else
				{
					//se l'utente esiste già nel db e non è selezionato significa che lo devo togliere
					if (companyRoleLoginDb.ExistUserCompany(companyRoleLoginItem.LoginId, companyRoleLoginItem.RoleId, companyRoleLoginItem.CompanyId) > 0)
					{
						companyRoleLoginDb.Delete(companyRoleLoginItem.LoginId, companyRoleLoginItem.CompanyId, companyRoleLoginItem.RoleId);
						if (OnModifyTreeOfCompanies != null)
							OnModifyTreeOfCompanies(sender, ConstString.containerCompanyRoles, this.companyId);
					}
				}
			}
			companyRoleLoginDb = null;
			Cursor.Current = Cursors.Default;
		}
		#endregion

		#region rbAllUsers_Click - Selezionato il Radio Button per visualizzare Tutti gli Utenti
		/// <summary>
		/// rbAllUsers_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void rbAllUsers_Click(object sender, System.EventArgs e)
		{
			LoadAllUsers();
		}
		#endregion

		#region rbUsersRole_Click - Selezionato il Radio Button per visualizzare gli Utenti già associati al Ruolo
		/// <summary>
		/// rbUsersRole_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void rbUsersRole_Click(object sender, System.EventArgs e)
		{
			UsersOfRole();
		}
		#endregion

		#region listViewUsersCompany_ItemActivate - Ho selezionato un elemento della lista
		/// <summary>
		/// listViewUsersCompany_ItemActivate
		/// Quando attivo la voce della listView, setto anche la checkbox a true
		/// </summary>
		//---------------------------------------------------------------------
		private void listViewUsersCompany_ItemActivate(object sender, System.EventArgs e)
		{
			ListViewItem selected = ((ListView)sender).SelectedItems[0];
			selected.Checked = true;
		}
		#endregion

		/// <summary>
		/// JoinUserRole_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void JoinUserRole_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// JoinUserRole_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void JoinUserRole_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// JoinUserRole_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void JoinUserRole_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}
	}
}