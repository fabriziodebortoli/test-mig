using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// ConfirmPassword.
	/// Form per la richiesta di conferma password quando si inserisce e/o 
	/// modifica un utente di tipo SQL
	/// </summary>
	//=========================================================================
	public partial class ConfirmPassword  : System.Windows.Forms.Form
	{

        #region Variabili private

		private string			  setNewPassword	= string.Empty;
		private string			  oldPassword		= string.Empty;
		private Diagnostic        diagnostic        = new Diagnostic("SysAdminPlugIn");
		//private DiagnosticViewer  diagnosticViewer  = new DiagnosticViewer();
		#endregion

		#region Proprietà

		//Properties
		//---------------------------------------------------------------------
		public string OldPassword	 { get { return oldPassword;    } set { oldPassword    = value; }}
		public string SetNewPassword { get { return setNewPassword; } set { setNewPassword = value; }}
		public Diagnostic Diagnostic { get { return diagnostic;}}

		#endregion

		#region Costruttore 

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="typeOfUser"></param>
		//---------------------------------------------------------------------
		public ConfirmPassword()
		{
			InitializeComponent();
			btnCancel.DialogResult = DialogResult.Cancel;
		}

		#endregion

		#region btnCancel_Click - Premuto il bottone di Cancel

		/// <summary>
		/// btnCancel_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		#endregion

		#region btnOk_Click - Premuto il bottone di Ok

		/// <summary>
		/// btnOk_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void btnOk_Click(object sender, System.EventArgs e)
		{
			if (CheckPassword())
			{
				this.DialogResult = DialogResult.OK;
				diagnostic.Clear();
				this.Close();
			}
			if (diagnostic.Error | diagnostic.Warning | diagnostic.Information)
				DiagnosticViewer.ShowDiagnostic(diagnostic);
		}

		#endregion

		#region CheckPassword - Verifica la correttezza della pwd

		/// <summary>
		/// CheckPassword()
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		private bool CheckPassword()
		{
			SetNewPassword = txtNewPassword.Text;
			if (string.Compare(OldPassword, SetNewPassword, false) != 0)
			{
				//le due password non sono uguali
				diagnostic.Set(DiagnosticType.Warning, Strings.NotEqualPassword);
				/*diagnosticViewer.Message		= Strings.NotEqualPassword;
				diagnosticViewer.Title			= Strings.Error;
				diagnosticViewer.ShowButtons	= MessageBoxButtons.OK;
				diagnosticViewer.ShowIcon		= MessageBoxIcon.Error;
				diagnosticViewer.Show();*/
				return false;
			}
			else
				return true;
		}

		#endregion

		#region Load della pagina

		/// <summary>
		/// ConfirmPassword_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);

			txtNewPassword.Enabled = true;
			txtNewPassword.Visible = true;
			txtNewPassword.Focus();
			Assembly myAss = Assembly.GetExecutingAssembly();
			//Nuovo
			Stream myStream = myAss.GetManifestResourceStream(ConstString.nameSpaceSysAdmin + ".Images.Keys.gif");
			this.ImageKey.Image = Image.FromStream(myStream, true);		
		}

		#endregion
	}
}
