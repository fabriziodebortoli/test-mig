using System.Collections;
using System.Data.SqlClient;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// DetailUserRole.
	/// </summary>
	//=========================================================================
	public partial class DetailUserRole : PlugInsForm
	{
		#region Variabili Private

		//---------------------------------------------------------------------
		private string			connectionString;
		private SqlConnection	currentConnection;
		private string			companyId;
		private string			loginId;
		private string			roleId;
		
		Diagnostic diagnostic = new Diagnostic("SystemAdminPluIn.DetailUserRole");
		#endregion

		#region Eventi e Delegati

		public delegate void ModifyTreeOfCompanies	(object sender, string nodeType,string companyId);
		public event		 ModifyTreeOfCompanies	OnModifyTreeOfCompanies;
		//---------------------------------------------------------------------
		public delegate void SendDiagnostic                 (object sender, Diagnostic diagnostic);
		public event         SendDiagnostic					OnSendDiagnostic;
		

		#endregion

		#region Costruttore
		
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="companyId"></param>
		/// <param name="loginId"></param>
		/// <param name="roleId"></param>
		//---------------------------------------------------------------------
		public DetailUserRole
			(
				string			connectionString, 
				SqlConnection	currentConnection,
			    string			companyId,
				string			loginId,
				string			roleId
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
			this.roleId								= roleId;
			this.loginId							= loginId;
		
			CompanyRoleLoginDb companyRoleLoginDb	= new CompanyRoleLoginDb();
			ArrayList userCompanyRole				= new ArrayList();
			companyRoleLoginDb.ConnectionString		= connectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentConnection;
			bool result = companyRoleLoginDb.SelectLoginCompanyRole
				(
					out userCompanyRole,
					companyId,
					roleId,
					loginId
				);
			if (!result)
			{
				diagnostic.Set(companyRoleLoginDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				userCompanyRole.Clear();
			}
			if (userCompanyRole.Count > 0)
			{
				CompanyRoleLogin companyRoleLogin = ((CompanyRoleLogin)userCompanyRole[0]);
				tbLogin.Text		= companyRoleLogin.Login;
				tbDescription.Text	= companyRoleLogin.Description;
				
				LabelTitle.Text = string.Format(LabelTitle.Text, companyRoleLogin.Login, companyRoleLogin.Role);
			}
		}

		#endregion

		#region Save 
		/// <summary>
		/// Save
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		public void Save(object sender, System.EventArgs e)
		{	
			//non fa nulla, perchè non c'è niente da salvare
			//almeno per ora
			if (OnModifyTreeOfCompanies != null)
				OnModifyTreeOfCompanies
					(
						sender, 
						ConstString.containerCompanyRoles, 
						companyId
					);
		}
	
		#endregion
		
		#region Delete - Cancella l'associazione dell'Utente al Ruolo dell'Azienda

		/// <summary>
		/// Delete
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		public void Delete(object sender, System.EventArgs e)
		{
			CompanyRoleLoginDb companyRoleLoginDb	= new CompanyRoleLoginDb();
			ArrayList userCompanyRole				= new ArrayList();
			companyRoleLoginDb.ConnectionString		= connectionString;
			companyRoleLoginDb.CurrentSqlConnection = currentConnection;
			bool result = companyRoleLoginDb.Delete(loginId, companyId, roleId);
			if (!result)
			{
				diagnostic.Set(companyRoleLoginDb.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (this.OnSendDiagnostic != null)
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

		#endregion

		/// <summary>
		/// DetailUserRole_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void DetailUserRole_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// DetailUserRole_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void DetailUserRole_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// DetailUserRole_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void DetailUserRole_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}
	}
}
