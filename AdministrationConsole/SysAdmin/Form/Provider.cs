using System.Collections;
using System.Data.SqlClient;
using System.Windows.Forms;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SysAdmin.Form
{
	/// <summary>
	/// Gestisce i dati riguardanti i Provider.
	/// </summary>
	//=========================================================================
	public partial class Provider : PlugInsForm
	{
		#region Variabili Private
		private string				connectionString	= string.Empty;
		private SqlConnection		currentConnection	= null;
		private DiagnosticViewer	diagnosticViewer	= new DiagnosticViewer();
		private Diagnostic			diagnostic			= new Diagnostic("SysAdminPlugIn.Provider");
		#endregion

		#region Delegati ed Eventi
		public delegate void ModifyTree	(object sender, string nodeType);
		public event		 ModifyTree OnModifyTree;

		public delegate void SendDiagnostic (object sender, Diagnostic diagnostic);
		public event         SendDiagnostic OnSendDiagnostic;
		#endregion
		
		#region Costruttori
		//---------------------------------------------------------------------
		public Provider(string connectionString, SqlConnection currentConnection, string id)
		{
			InitializeComponent();

			this.connectionString	= connectionString;
			this.currentConnection	= currentConnection;

			cbStripTrailingSpaces.Checked	= false;
			cbUseConstParameter.Checked		= false;
			PopolateComboProvider();
			
			ArrayList fieldsProvider		= new ArrayList();
			ProviderDb dbProvider			= new ProviderDb();
			dbProvider.ConnectionString		= connectionString;
			dbProvider.CurrentSqlConnection = currentConnection;

			bool result = dbProvider.GetAllFieldsProviderById(out fieldsProvider, id);

			if (!result)
			{
				diagnostic.Set(dbProvider.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);

				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(this, diagnostic);
					diagnostic.Clear();
				}

				fieldsProvider.Clear();
			}

			if (fieldsProvider.Count > 0)
			{
				ProviderItem providerItem = (ProviderItem)fieldsProvider[0];

				for (int i = 0; i < cbProvider.Items.Count; i++)
				{
					ProviderList providerList = (ProviderList)cbProvider.Items[i];
					if (providerList.Description == providerItem.Description)
						cbProvider.SelectedIndex = i;
				}

				tbProviderId.Text				= providerItem.ProviderId;
				cbStripTrailingSpaces.Checked	= providerItem.StripTrailingSpaces;
				cbUseConstParameter.Checked		= providerItem.UseConstParameter;
			}	

			cbProvider.Enabled = false;
			State = StateEnums.View;
		}

		//---------------------------------------------------------------------
		public Provider(string connectionString, SqlConnection currentConnection)
		{
			InitializeComponent();

			this.connectionString	= connectionString;
			this.currentConnection	= currentConnection;
			cbStripTrailingSpaces.Checked	= false;
			cbUseConstParameter.Checked		= false;
			
			PopolateComboProvider();
			cbProvider.Enabled = true;
			State = StateEnums.View;
		}
		#endregion
		
    	#region Private methods
		//---------------------------------------------------------------------
		private void Provider_Load(object sender, System.EventArgs e)
		{
			if (string.IsNullOrEmpty(cbProvider.Text))
				cbProvider.Focus();
		}

		/// <summary>
		/// Carica nella Combo i Providers disponibili.
		/// </summary>
		//---------------------------------------------------------------------
		private void PopolateComboProvider()
		{
			ArrayList listOfProviders = new ArrayList();

			listOfProviders.Add(new ProviderList(DatabaseLayerConsts.SqlOleProviderDescription, NameSolverDatabaseStrings.SQLOLEDBProvider));
            //listOfProviders.Add(new ProviderList(DatabaseLayerConsts.SqlODBCProviderDescription, NameSolverDatabaseStrings.SQLODBCProvider));
			listOfProviders.Add(new ProviderList(DatabaseLayerConsts.OracleProviderDescription, NameSolverDatabaseStrings.OraOLEDBProvider));
            //listOfProviders.Add(new ProviderList(DatabaseLayerConsts.PostgreProviderDescription, NameSolverDatabaseStrings.PostgreOdbcProvider));


			cbProvider.Items.Clear();
			cbProvider.DataSource		= listOfProviders;
			cbProvider.DisplayMember	= "Description";
			cbProvider.ValueMember		= ConstString.itemProvider;
			cbProvider.SelectedIndex	= 0;
		}

		//---------------------------------------------------------------------
		private void cbProvider_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			cbProvider.SelectedItem = ((ComboBox)sender).SelectedItem;
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void Provider_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void Provider_Deactivate(object sender, System.EventArgs e)
		{
			if (OnSendDiagnostic != null)
				OnSendDiagnostic(sender, diagnostic);
		}

		//---------------------------------------------------------------------
		private void Provider_VisibleChanged(object sender, System.EventArgs e)
		{
			if (!this.Visible)
			{
				if (OnSendDiagnostic != null)
					OnSendDiagnostic(sender, diagnostic);
			}
		}

		//---------------------------------------------------------------------
		private void cbUseConstParameter_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void cbStripTrailingSpaces_CheckedChanged(object sender, System.EventArgs e)
		{
			State = StateEnums.Editing;
		}
		#endregion

		#region Save
		//---------------------------------------------------------------------
		public void Save(object sender, System.EventArgs e)
		{
			ProviderDb dbProvider			= new ProviderDb();
			dbProvider.ConnectionString		= connectionString;
			dbProvider.CurrentSqlConnection = currentConnection;

			bool result =
				dbProvider.Modify
				(
					tbProviderId.Text,
					((ProviderList)cbProvider.SelectedItem).Provider,
					((ProviderList)cbProvider.SelectedItem).Description,
					cbUseConstParameter.Checked,
					cbStripTrailingSpaces.Checked
				);

			if (!result)
			{
				diagnostic.Set(dbProvider.Diagnostic);
				DiagnosticViewer.ShowDiagnostic(diagnostic);

				if (OnSendDiagnostic != null)
				{
					OnSendDiagnostic(sender, diagnostic);
					diagnostic.Clear();
				}

				State = StateEnums.Editing;
				return;
			}
			else
			{
				State = StateEnums.View;
				if (OnModifyTree != null)
					OnModifyTree(sender, ConstString.containerProviders);
			}
		}
		#endregion
	}
	
	/// <summary>
	/// Serve per la combo dei provider.
	/// </summary>
	//========================================================================
	public class ProviderList
	{
		#region Variabili Membro (private)
		private string description	= string.Empty;
		private string provider		= string.Empty;
		#endregion

		#region Properties
		public string Description { get { return description; }	set { description = value; } }
		public string Provider    {	get { return provider;    }	set { provider    = value; } }
		#endregion

		#region Costruttori
		//---------------------------------------------------------------------
		public ProviderList()
		{
		}

		//---------------------------------------------------------------------
		public ProviderList(string description, string provider)
		{
			Description = description;
			Provider	= provider;
		}
		#endregion
	}
}