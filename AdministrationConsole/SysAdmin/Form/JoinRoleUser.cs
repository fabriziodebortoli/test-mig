using System.Collections;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	
	/// <summary>
	/// JoinRoleUser
	/// Associa un Ruolo di una Azienda ad uno o più utenti
	/// </summary>
	//=========================================================================
	public partial class JoinRoleUser : PlugInsForm
	{
	
		#region Variabili private

		private SqlConnection	currentConnection;
		private string			connectionString;
		private string			companyId;
		private string			loginId;
		private Diagnostic      diagnostic = new Diagnostic("SysAdminPlugIn.JoinRoleUser");

        #endregion

		#region Eventi e Delegati

		public delegate void ModifyTreeOfCompanies	(object sender, string nodeType, string companyId);
		public event		 ModifyTreeOfCompanies	OnModifyTreeOfCompanies;
		//---------------------------------------------------------------------
		public delegate void SendDiagnostic                 (object sender, Diagnostic diagnostic);
		public event         SendDiagnostic					OnSendDiagnostic;

		#endregion

		#region Costruttore - Per default carico tutti i Ruoli nella ListView

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="currentConnection"></param>
		/// <param name="companyId"></param>
		/// <param name="loginId"></param>
		//---------------------------------------------------------------------
		public JoinRoleUser
			(
				string			connectionString, 
				SqlConnection	currentConnection, 
				string			companyId, 
				string			loginId, 
				ImageList		treeImages
			)
		{
			
			InitializeComponent();
			//PlugInsForm plugInsForm = new PlugInsForm();
			//this.Size = plugInsForm.Size;
			//this.Font = plugInsForm.Font;
			//plugInsForm.Dispose();
			this.connectionString					= connectionString;
			this.currentConnection					= currentConnection;
			this.companyId							= companyId;
			this.loginId							= loginId;
			rbAllRoles.Checked						= true;
			rbRolesUser.Checked						= false;
			listViewUsersCompany.View				= View.Details;
			listViewUsersCompany.CheckBoxes			= true;
			listViewUsersCompany.AllowColumnReorder = true;
            listViewUsersCompany.Sorting = System.Windows.Forms.SortOrder.Ascending;
			listViewUsersCompany.Items.Clear();
			listViewUsersCompany.Columns.Clear();
			listViewUsersCompany.Columns.Add(Strings.Role,		-2, HorizontalAlignment.Left);
			listViewUsersCompany.Columns.Add(Strings.Description, -2, HorizontalAlignment.Left);
			listViewUsersCompany.SmallImageList		= listViewUsersCompany.LargeImageList = treeImages;
			//default carico tutti i Ruoli
			AllRoles();
		}

		#endregion

		#region AllRoles - Visualizzo nella ListView tutti i Ruoli di una Azienda

		/// <summary>
		/// AllRoles
		/// </summary>
		//---------------------------------------------------------------------
		private void AllRoles()
		{
			listViewUsersCompany.Items.Clear();
			RoleDb roleDb				= new RoleDb();
			ArrayList rolesCompany		= new ArrayList();
			roleDb.ConnectionString		= connectionString;
			roleDb.CurrentSqlConnection = currentConnection;
			bool result = roleDb.SelectAllRolesOfCompany(out rolesCompany, companyId);
			if (!result)
			{
				diagnostic.Set(roleDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				rolesCompany.Clear();
			}
			for (int i=0; i < rolesCompany.Count; i++)
			{
				RoleItem itemRole			= (RoleItem) rolesCompany[i];
				CompanyRoleLoginItem item	= new CompanyRoleLoginItem();
				item.Text					= itemRole.Role;
                item.ImageIndex = PlugInTreeNode.GetRoleDefaultImageIndex;
				item.SubItems.Add(itemRole.Description.Replace("\r\n", " "));
				item.LoginId				= loginId;
				item.RoleId					= itemRole.RoleId;
				item.CompanyId				= companyId;
				item.Checked				= ExistJoin
												(
													loginId, 
													companyId, 
													itemRole.RoleId
												);
				if (item.Checked) 
					item.ForeColor			= Color.Blue;
				listViewUsersCompany.Items.Add(item);
			}
		}

		#endregion
		
		#region ExistJoin - Segnala se un Utente è già associata a un Ruolo

		/// <summary>
		/// ExistJoin
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="roleId"></param>
		/// <param name="profileId"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool ExistJoin(string loginId, string companyId, string roleId)
		{
			bool checkedBox							= false;
			CompanyRoleLoginDb companyRoleLoginDb	= new CompanyRoleLoginDb();
			companyRoleLoginDb.ConnectionString		= connectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentConnection;
			if (companyRoleLoginDb.ExistUserCompany(loginId,roleId,companyId) > 0)
				checkedBox = true;
			return checkedBox;
		}

		#endregion

		#region RolesOfUser - Carica nella ListView solo i Ruoli associati a un Utente
		
		/// <summary>
		/// RolesOfUser
		/// </summary>
		//---------------------------------------------------------------------
		private void RolesOfUser()
		{
			listViewUsersCompany.Items.Clear();
			CompanyRoleLoginDb companyRoleLoginDb	= new CompanyRoleLoginDb();
			ArrayList usersCompanyRoles				= new ArrayList();
			companyRoleLoginDb.ConnectionString		= connectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentConnection;
			bool result = companyRoleLoginDb.SelectAllRoles(out usersCompanyRoles, companyId, loginId);
			if (!result)
			{
				diagnostic.Set(companyRoleLoginDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (this.OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				usersCompanyRoles.Clear();
			}
			for (int i=0; i < usersCompanyRoles.Count; i++)
			{
				RoleItem itemRole			= (RoleItem) usersCompanyRoles[i];
				CompanyRoleLoginItem item	= new CompanyRoleLoginItem();
				item.Text					= itemRole.Role;
                item.ImageIndex = PlugInTreeNode.GetRoleDefaultImageIndex;
				item.SubItems.Add(itemRole.Description.Replace("\r\n", " "));
				item.Checked				= true;
				item.LoginId				= loginId;
				item.CompanyId				= itemRole.CompanyId;
				item.RoleId					= itemRole.RoleId;
				item.ForeColor				= Color.Blue;
				listViewUsersCompany.Items.Add(item);
			}
			
		}

		#endregion

		#region Save - Salvataggio dei dati 

		/// <summary>
		/// Save
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		public void Save(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			CompanyRoleLoginDb companyRoleLoginDb	= new CompanyRoleLoginDb();
			companyRoleLoginDb.ConnectionString		= connectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentConnection;
			for (int i=0; i < listViewUsersCompany.Items.Count; i++)
			{
				CompanyRoleLoginItem companyRoleLoginItem = (CompanyRoleLoginItem)listViewUsersCompany.Items[i];
				if (companyRoleLoginItem.Checked)
				{
					//se l'ho selezionato e l'utente non c'è nel DB significa che lo devo inserire
					if (companyRoleLoginDb.ExistUserCompany(loginId, companyRoleLoginItem.RoleId, companyId) == 0)
					{
						bool result = companyRoleLoginDb.Add(loginId, companyId, companyRoleLoginItem.RoleId);
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
								OnModifyTreeOfCompanies(sender, ConstString.containerCompanyRoles, companyId);
						}
					}
				}
				else
				{
					//se l'utente esiste già nel db e non è selezionato significa che lo devo togliere
					if (companyRoleLoginDb.ExistUserCompany(loginId, companyRoleLoginItem.RoleId, companyId) > 0)
					{
						companyRoleLoginDb.Delete(loginId, companyId, companyRoleLoginItem.RoleId);
						if (OnModifyTreeOfCompanies != null)
							OnModifyTreeOfCompanies(sender, ConstString.containerCompanyRoles, companyId);
					}
				}
			}
			companyRoleLoginDb = null;
			Cursor.Current = Cursors.Default;
		}

		#endregion

        #region rbAllRoles_Click - Selezionato il Radio Button per visualizzare tutti i Ruoli

		/// <summary>
		/// rbAllRoles_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void rbAllRoles_Click(object sender, System.EventArgs e)
		{
			AllRoles();
		}

		#endregion

		#region rbRolesUser_Click - Selezionato il Radio Button per visualizzare solo i Ruoli associati a uno o più Utenti dell'Azienda

		/// <summary>
		/// rbRolesUser_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void rbRolesUser_Click(object sender, System.EventArgs e)
		{
			RolesOfUser();
		}

		#endregion


		/// <summary>
		/// JoinRoleUser_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void JoinRoleUser_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}


		/// <summary>
		/// JoinRoleUser_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void JoinRoleUser_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}


		/// <summary>
		/// JoinRoleUser_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void JoinRoleUser_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		

	}
}
