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
	/// Role
	/// Form che visualizza i dati relativi ai Ruoli di una Azienda
	/// </summary>
	//=========================================================================
	public partial class Role : PlugInsForm
	{
		#region Variabili private
		private string				connectionString	= string.Empty;
		private	SqlConnection		currentConnection	= null;
		private string				companyId			= string.Empty;
		private DiagnosticViewer	diagnosticViewer	= new DiagnosticViewer();
		private Diagnostic			diagnostic			= new Diagnostic("SysAdminPlugIn.Role");
		#endregion

		#region Delegati ed Eventi
		//---------------------------------------------------------------------
		public delegate void ModifyTreeOfCompanies	(object sender, string nodeType,string companyId);
		public event		 ModifyTreeOfCompanies OnModifyTreeOfCompanies;

		public delegate void AfterChangedDisabled( object sender, string roleId, bool disabled);
		public event		 AfterChangedDisabled OnAfterChangedDisabled;

		public delegate void SendDiagnostic(object sender, Diagnostic diagnostic);
		public event         SendDiagnostic OnSendDiagnostic;
		#endregion

		#region Costruttori
		/// <summary>
		/// Costruttore 
		/// </summary>
		//---------------------------------------------------------------------
		public Role(string connectionString, SqlConnection connection, string roleId, string companyId)
		{
			InitializeComponent();
			this.connectionString		= connectionString;
			this.currentConnection		= connection;
			tbRoleId.Text				= roleId;
			this.companyId				= companyId;
			ArrayList fieldsRole		= new ArrayList();

			RoleDb dbRole				= new RoleDb();
			dbRole.ConnectionString		= this.connectionString;
			dbRole.CurrentSqlConnection = this.currentConnection;
			
			if (!dbRole.GetAllRoleFieldsById(out fieldsRole, roleId, this.companyId))
			{
				diagnostic.Set(dbRole.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
				fieldsRole.Clear();
			}

			if (fieldsRole.Count > 0)
			{
				RoleItem roleItem		= (RoleItem)fieldsRole[0];
				tbRoleId.Text			= roleItem.RoleId;
				tbRole.Text				= roleItem.Role;
				tbDescrizione.Text		= roleItem.Description;
				tbCompanyId.Text		= roleItem.CompanyId;
				CbDisableRole.Checked	= roleItem.Disabled;
				LabelTitle.Text = string.Format(Strings.TitleModifyingRole, roleItem.Role);
			}
			
			SettingToolTips();
			State = StateEnums.View;
		}
		
		/// <summary>
		/// Costruttore 
		/// </summary>
		//---------------------------------------------------------------------
		public Role(string connectionString, SqlConnection	currentConnection, string companyId)
		{
			InitializeComponent();
			this.connectionString	= connectionString;
			this.currentConnection	= currentConnection;
			this.companyId			= companyId;
			LabelTitle.Text			= Strings.TitleInsertingNewRole;
			State = StateEnums.View;
		}
		#endregion

		#region Role_Load - Caricamento della Form
		//---------------------------------------------------------------------
		private void Role_Load(object sender, System.EventArgs e)
		{
			if (tbRole.Text.Length == 0)
				tbRole.Focus();
		}
		#endregion
		
		# region CheckValidator
		/// <summary>
		/// Verifica la validità dei dati inputati
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool CheckValidator(string role)
		{
			bool result = true;
			if (role.Length == 0)
			{
				string message = string.Format(Strings.NoEmptyValue, ConstString.itemRole);
				diagnostic.Set(DiagnosticType.Error, message);
				result = false;
			}

			if (!result)
			{
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}
			}

			return result;
		}
		# endregion

		# region Salvataggio nuovo ruolo o modifica di ruolo già esistente
		/// <summary>
		/// Save a modified role
		/// </summary>
		//---------------------------------------------------------------------
		public void Save(object sender, System.EventArgs e)
		{
			tbRole.Text = tbRole.Text.Trim();	// ensures robustness against bad coding practices
			string role = tbRole.Text;
			
			if (!CheckValidator(role))
				return;

			RoleDb dbRole				= new RoleDb();
			dbRole.ConnectionString		= this.connectionString;
			dbRole.CurrentSqlConnection = this.currentConnection;
			
			if (!dbRole.Modify(tbRoleId.Text, role, tbDescrizione.Text, tbCompanyId.Text, CbDisableRole.Checked))
			{
				diagnostic.Set(dbRole.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);
				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(sender, diagnostic);
					diagnostic.Clear();
				}
				State = StateEnums.Editing;
			}
			else
			{
				State = StateEnums.View;
				if (OnModifyTreeOfCompanies != null)
					OnModifyTreeOfCompanies(sender, ConstString.containerCompanyRoles, this.companyId);
				if (OnAfterChangedDisabled != null)
					OnAfterChangedDisabled(sender, tbRoleId.Text, CbDisableRole.Checked);
			}
		}

		/// <summary>
		/// Saves a new role
		/// </summary>
		//---------------------------------------------------------------------
		public void SaveNew(object sender, System.EventArgs e)
		{
			tbRole.Text = tbRole.Text.Trim();	// ensures robustness against bad coding practices
			string role = tbRole.Text;
			
			if (CheckValidator(role))
			{
				RoleDb dbRole				= new RoleDb();
				dbRole.ConnectionString		= connectionString;
				dbRole.CurrentSqlConnection = currentConnection;
				
				// se il ruolo esiste già non procedo
				if (dbRole.ExistRole(companyId, role))
				{
					diagnostic.SetError(string.Format(Strings.RoleAddedAlreadyExists, role));
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{

						OnSendDiagnostic(sender, diagnostic);
						diagnostic.Clear();
					}
					State = StateEnums.Editing;
					return;
				}

				if (!dbRole.Add(role, tbDescrizione.Text, companyId, CbDisableRole.Checked))
				{
					diagnostic.Set(dbRole.Diagnostic);
					DiagnosticViewer.ShowDiagnostic(diagnostic);
					if (OnSendDiagnostic != null)
					{
						OnSendDiagnostic(sender, diagnostic);
						diagnostic.Clear();
					}
					State = StateEnums.Editing;
				}
				else
				{
					if (OnModifyTreeOfCompanies != null)
						OnModifyTreeOfCompanies(sender, ConstString.containerCompanyRoles, companyId);
					
					if (OnAfterChangedDisabled != null)
					{
						tbRoleId.Text = dbRole.LastRoleIdInserted();
						OnAfterChangedDisabled(sender, tbRoleId.Text, CbDisableRole.Checked);
					}

					State = StateEnums.View;
				}
			}
		}
		# endregion

		#region Delete - Cancella il Ruolo di una Azienda
		/// <summary>
		/// Delete
		/// </summary>
		//---------------------------------------------------------------------
		public void Delete(object sender, System.EventArgs e)
		{
			RoleDb dbRole				= new RoleDb();
			dbRole.ConnectionString		= connectionString;
			dbRole.CurrentSqlConnection = currentConnection;

			if (!dbRole.Delete(tbRoleId.Text, tbCompanyId.Text))
			{
				diagnostic.Set(dbRole.Diagnostic);
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
		#endregion

		#region SettingToolTips
		/// <summary>
		/// SettingToolTips
		/// </summary>
		//---------------------------------------------------------------------
		private void SettingToolTips()
		{
            toolTip.SetToolTip(tbRole, Strings.RoleNameToolTip);
            toolTip.SetToolTip(tbDescrizione, Strings.RoleDescriptionToolTip);
            toolTip.SetToolTip(CbDisableRole, Strings.RoleDisabledToolTip);
		}
		#endregion
	
		#region Funzioni per il Send della diagnostica
		/// <summary>
		/// Role_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void Role_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// Role_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void Role_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// Role_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		//---------------------------------------------------------------------
		private void Role_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}
		#endregion

		#region Funzioni per la modalità di Editing
		//---------------------------------------------------------------------
		private void tbRole_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void tbDescrizione_TextChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void CbDisableRole_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}
		#endregion
	}
}