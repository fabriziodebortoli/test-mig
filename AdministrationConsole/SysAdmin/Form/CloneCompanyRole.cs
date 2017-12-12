using System.Collections;
using System.Data.SqlClient;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// CloneCompanyRole
	/// Form utilizzata per la clonazione di un ruolo di una company
	/// </summary>
	//=========================================================================
	public partial class CloneCompanyRole : PlugInsForm
	{
		#region Private variables
		private SqlConnection		currentConnection	= null;
		private DiagnosticViewer	diagnosticViewer	= new DiagnosticViewer();
		private Diagnostic			diagnostic			= new Diagnostic("SysAdminPlugIn.CloneCompanyRole");

		private string connectionString	= string.Empty;
		private string companyId		= string.Empty;
		private string roleId			= string.Empty;
		#endregion

		#region Events and delegates
		//---------------------------------------------------------------------
		public delegate void ModifyTreeOfCompanies(object sender, string nodeType,string companyId);
		public event ModifyTreeOfCompanies OnModifyTreeOfCompanies;

		public delegate void AfterClonedRole(string companyId);
		public event AfterClonedRole OnAfterClonedRole;

		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event SendDiagnostic OnSendDiagnostic;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		//---------------------------------------------------------------------
		public CloneCompanyRole(string connectionString, SqlConnection connection, string companyId, string roleId)
		{
			InitializeComponent();
			this.connectionString	   = connectionString;
			this.currentConnection	   = connection;
			this.companyId			   = companyId;
			this.roleId				   = roleId;

			LblNoCompanies.Visible	   = false;
			
			PopolateComboCompanies();
			PopolateTextRole();
		}
		#endregion

		#region PopolateComboCompanies - Popola la combobox delle Aziende
		/// <summary>
		/// PopolateComboCompanies
		/// </summary>
		//---------------------------------------------------------------------
		private void PopolateComboCompanies()
		{
			comboCompanies.Items.Clear();
			CompanyDb companyDb			   = new CompanyDb();
			companyDb.ConnectionString	   = connectionString;
			companyDb.CurrentSqlConnection = currentConnection;
			ArrayList companyList		   = new ArrayList();

			if (!companyDb.SelectAllCompanies(out companyList))
			{
				if (companyDb.Diagnostic.Error || companyDb.Diagnostic.Warning || companyDb.Diagnostic.Information)
				{
					diagnostic.Set(companyDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
				}
				companyList.Clear();
			}
			
			ArrayList companies = new ArrayList();
			foreach (CompanyItem cItem in companyList)
			{
    			// escludo le aziende non sottoposte a Security
				if (!cItem.UseSecurity)
					continue;

				companies.Add(cItem);
			}

			if (companies.Count > 0)
			{
				btnCloneRole.Enabled = true;
				comboCompanies.Enabled = true;
				comboCompanies.DataSource    = companies;
				comboCompanies.DisplayMember = "Company";
				comboCompanies.ValueMember   = "CompanyId";
			}
			else
			{
				LblNoCompanies.Visible	= true;
				tbRoleDest.Enabled		= false;
				btnCloneRole.Enabled	= false;
				comboCompanies.Enabled	= false;
			}
		}
		#endregion

		#region PopolateTextRole - Imposta il nome del Ruolo sorgente
		/// <summary>
		/// PopolateTextRole
		/// </summary>
		//---------------------------------------------------------------------
		private void PopolateTextRole()
		{
			RoleDb roleDb				= new RoleDb();
			roleDb.ConnectionString		= connectionString;
			roleDb.CurrentSqlConnection = currentConnection;
			ArrayList roleList			= new ArrayList();
			if (!roleDb.GetAllRoleFieldsById(out roleList, roleId, companyId))
			{
				if (roleDb.Diagnostic.Error || roleDb.Diagnostic.Warning || roleDb.Diagnostic.Information)
				{
					diagnostic.Set(roleDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(this, diagnostic);
						diagnostic.Clear();
					}
				}
				roleList.Clear();
			}

			if (roleList.Count > 0)
			{
				RoleItem roleItem = (RoleItem)roleList[0];
				tbRoleSource.Text = roleItem.Role;
				tbRoleDest.Text   = roleItem.Role;
			}
		}
		#endregion

		#region btnCloneRole_Click - Premuto il bottone di Clona Ruolo
		/// <summary>
		/// btnCloneRole_Click
		/// </summary>
		//---------------------------------------------------------------------
		private void btnCloneRole_Click(object sender, System.EventArgs e)
		{
			State = StateEnums.Processing;
			string companyDestId	= ((CompanyItem)comboCompanies.SelectedItem).CompanyId;
			string roleTextDest		= tbRoleDest.Text;

            if (companyId == companyDestId && string.Compare(tbRoleSource.Text, roleTextDest) == 0)
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.CannotCloneRoleIntoSameCompany);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(sender, diagnostic);
					diagnostic.Clear();
				}
                tbRoleDest.Focus();
				return;
			}

            RoleDb roleDb = new RoleDb();
            roleDb.ConnectionString = connectionString;
            roleDb.CurrentSqlConnection = currentConnection;

            if (roleDb.ExistRole(companyDestId, roleTextDest))
            {
                diagnostic.Set(DiagnosticType.Warning, Strings.RoleExist);
                DiagnosticViewer.ShowDiagnostic(diagnostic);
                if (OnSendDiagnostic != null)
                {
                    OnSendDiagnostic(sender, diagnostic);
                    diagnostic.Clear();
                }
                tbRoleDest.Focus();
                return;
            }

			if (roleTextDest.Length == 0)
			{
				diagnostic.Set(DiagnosticType.Warning, Strings.NoDestinationRole);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(sender, diagnostic);
					diagnostic.Clear();
				}
				return;
			}

			if (roleDb.ExistRole(companyDestId, roleTextDest))
			{
				diagnostic.SetError(string.Format(Strings.RoleClonedAlreadyExists, roleTextDest, ((CompanyItem)comboCompanies.SelectedItem).Company));
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(sender, diagnostic);
					diagnostic.Clear();
				}
				State = StateEnums.Editing;
				return;
			}

			if (!roleDb.Clone(companyId, roleId, companyDestId, roleTextDest))
			{
				if (roleDb.Diagnostic.Error || roleDb.Diagnostic.Warning || roleDb.Diagnostic.Information)
				{
					diagnostic.Set(roleDb.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(sender, diagnostic);
						diagnostic.Clear();
					}
				}
				State = StateEnums.Editing;
			}
			else
			{
				if (OnModifyTreeOfCompanies != null)
					OnModifyTreeOfCompanies(sender, ConstString.containerCompanyRoles, companyDestId);
				if (OnAfterClonedRole != null)
					OnAfterClonedRole(companyDestId);
				State = StateEnums.View;
			}
		}
		#endregion

		# region Eventi sui controls
		/// <summary>
		/// CloneCompanyRole_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void CloneCompanyRole_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}
		
		/// <summary>
		/// CloneCompanyRole_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void CloneCompanyRole_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// CloneCompanyRole_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void CloneCompanyRole_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}
		# endregion

	}
}