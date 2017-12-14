
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// Summary description for ViewInfoConnection.
	/// </summary>
	//=========================================================================
	public partial class ViewInfoConnection : PlugInsForm
	{

		#region Variabili private
		private Diagnostic diagnostic = new Diagnostic("SysAdminPlugIn.ViewInfoConnection");
		#endregion

		#region Eventi e Delegati

		//---------------------------------------------------------------------
		public delegate void SendDiagnostic                 (object sender, Diagnostic diagnostic);
		public event         SendDiagnostic					OnSendDiagnostic;
		

		#endregion

		#region Costruttore
		
		/// <summary>
		/// ViewInfoConnection (Costruttore)
		/// </summary>
		/// <param name="currentStatus"></param>
		//---------------------------------------------------------------------
		public ViewInfoConnection(SysAdminStatus currentStatus, BrandLoader aBrandLoader)
		{
			InitializeComponent();
			
			string serverName = string.Empty;
			if (currentStatus != null)
			{
				serverName = (currentStatus.ServerIstanceName.Length == 0) 
						? currentStatus.ServerName
						: (currentStatus.ServerName + System.IO.Path.DirectorySeparatorChar + currentStatus.ServerIstanceName);

				TbServerName.Text = serverName;
				TbLogin.Clear();
				TbPassword.Clear();
				if (currentStatus.IntegratedConnection)
				{
					TbLogin.Enabled		= false;
					TbPassword.Enabled	= false;
				}
				else
				{
					TbLogin.Enabled		= true;
					TbPassword.Enabled	= true;
					TbLogin.Text		= currentStatus.OwnerDbName;
					TbPassword.Text		= currentStatus.OwnerDbPassword;
				}
				
				TBDataBaseName.Text	= currentStatus.DataSource;
			}
			else
			{
				//diagnostic.Set(DiagnosticType.Error, Strings.ReadConfigFile);
				//DiagnosticViewer.ShowDiagnostic(diagnostic);
				//if (OnSendDiagnostic != null)
				//{
				//    OnSendDiagnostic(this, diagnostic);
				//    diagnostic.Clear();
				//}
				TbServerName.Text	= string.Empty;
				TbLogin.Text		= string.Empty;
				TbPassword.Text		= string.Empty;
				TBDataBaseName.Text = string.Empty;
			}

			TbLogin.ReadOnly		= true;
			TbPassword.ReadOnly		= true;
			TBDataBaseName.ReadOnly = true;

			if (aBrandLoader != null)
			{
				string brandedCompany = aBrandLoader.GetCompanyName();
				if (brandedCompany != null && brandedCompany.Length > 0)
					LblExplication.Text = LblExplication.Text.Replace(NameSolverStrings.Microarea, brandedCompany);
			}
		}

		#endregion

		#region Funzioni per il send della diagnostica

		/// <summary>
		/// ViewInfoConnection_Closing
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ViewInfoConnection_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// ViewInfoConnection_Deactivate
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ViewInfoConnection_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		/// <summary>
		/// ViewInfoConnection_VisibleChanged
		/// Sendo la diagnostica al SysAdmin
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void ViewInfoConnection_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		#endregion

	}

	

}
