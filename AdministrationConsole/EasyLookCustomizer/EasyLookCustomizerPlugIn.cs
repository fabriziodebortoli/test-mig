using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Console.Plugin.EasyLookCustomizer
{
	/// <summary>
	/// Summary description for EasyLookCustomizerPlugIn.
	/// </summary>
	//=========================================================================
	public class EasyLookCustomizer : PlugIn
	{
		# region Variabili private
		private static readonly string		easyLookCustomizerPlugInName = Assembly.GetExecutingAssembly().GetName().Name;
		private PathFinder			consolePathFinder = null;
        private Form                consoleMainForm = null;
        private MenuStrip			consoleMenu = null;
        private PlugInsTreeView     consoleTreeView = null;
		private Panel				consoleWorkingArea = null;
		private Panel				consoleWorkingAreaBottom = null;
		private string				buildArgument = String.Empty;
		private string				configurationArgument = String.Empty;
		private bool			    runningFromServerArgument = false;
		private string			    consoleDBConnectionString = String.Empty;

		//info di ambiente di console
		private ConsoleEnvironmentInfo consoleEnvironmentInfo;

		private bool				isStandardEdition	= false;
		private StringCollection	companiesIdAdmitted = null;

		private EasyLookCustomizerControl EasyLookCustomizerControl;
		# endregion

		#region Eventi per la disabilitazione dei pulsanti della ToolBar della console
		//Abilita il pulsante di Save ----------------------------------------
		public event System.EventHandler OnEnableSaveToolBarButton;
		// Disabilita il pulsante Save
		public event System.EventHandler OnDisableSaveToolBarButton;
		// Abilita il pulsante di Delete
		public event System.EventHandler OnEnableDeleteToolBarButton;
		// Disabilita il pulsante Delete
		public event System.EventHandler OnDisableDeleteToolBarButton;
		// Disabilita il pulsante New
		public event System.EventHandler OnDisableNewToolBarButton;
		// Disabilita il pulsante Open
		public event System.EventHandler OnDisableOpenToolBarButton;
		#endregion

		#region Eventi per la disabilitazione dei pulsanti della ToolBar della Security
		// Disabilito il pulsante per il caricamento degli oggetti messi sotto protezione
		public event System.EventHandler OnDisableOtherObjectsToolBarButton;
		// Disabilito il pulsante che abilita la visualizzazione delle icone della sicurezza 
		public event System.EventHandler OnDisableShowSecurityIconsToolBarButton;
        public event System.EventHandler OnDisableFindSecurityObjectsToolBarButton;
		// Disabilito il pulsante per l'applicazione del filtro di sicurezza
		public event System.EventHandler OnDisableApplySecurityFilterToolBarButton;
        public event System.EventHandler OnDisableShowAllGrantsToolBarButtonPushed;
		#endregion

		#region Eventi dalla Microarea Console
		//ritorna lo stato di console
		public delegate StatusType GetConsoleStatus();
		public event GetConsoleStatus OnGetConsoleStatus;
		#endregion
		
		//--------------------------------------------------------------------------------------------------------
		public EasyLookCustomizer()
		{
			consoleDBConnectionString = String.Empty;
		}

		//--------------------------------------------------------------------------------------------------------
		public override void Load
			(
				ConsoleGUIObjects		consoleGUIObjects,
				ConsoleEnvironmentInfo	consoleEnvironmentInfo,
				LicenceInfo				licenceInfo
			)
		{
			consoleMenu	= consoleGUIObjects.MenuConsole; 
			if (consoleMenu != null)
				consoleMainForm = consoleMenu.FindForm();

			consoleTreeView				= consoleGUIObjects.TreeConsole; 
			consoleWorkingArea			= consoleGUIObjects.WkgAreaConsole; 
			consoleWorkingAreaBottom	= consoleGUIObjects.BottomWkgAreaConsole; 
			runningFromServerArgument	= consoleEnvironmentInfo.RunningFromServer; 

			this.consoleEnvironmentInfo = consoleEnvironmentInfo;
			isStandardEdition			= (String.Compare(licenceInfo.Edition, NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0);

			EasyLookCustomizerControl = new EasyLookCustomizerControl();
			EasyLookCustomizerControl.ConnectionString = String.Empty;
			EasyLookCustomizerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			EasyLookCustomizerControl.Location = new System.Drawing.Point(0, 0);
			EasyLookCustomizerControl.Name = "EasyLookCustomizerControl";
			EasyLookCustomizerControl.TabIndex = 0;
			EasyLookCustomizerControl.Visible = false;
			EasyLookCustomizerControl.SettingsExistenceChanged += new EasyLookCustomizerControl.SettingsExistenceChangedEventHandler(this.EasyLookCustomizerControl_SettingsExistenceChanged);
		}

		#region Eventi della MicroareaConsole 
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitPathFinder")]
		public void OnInitPathFinder(PathFinder pathFinder)
		{
			consolePathFinder = pathFinder;
		}
		/// <summary>
		/// Intercetta la pressione del bottone Save dalla Console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnSaveItem")]
		public void OnAfterClickSaveButton(object sender, System.EventArgs e)
		{
			if (consoleWorkingArea.Controls == null || consoleWorkingArea.Controls.Count == 0)
				return;
			
			if (EasyLookCustomizerControl != null)
				EasyLookCustomizerControl.SaveCurrentSettings();
		}
		
		/// <summary>
		/// Intercetta la pressione del bottone Delete dalla Console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnDeleteItem")]
		public void OnAfterClickDeleteButton(object sender, System.EventArgs e)
		{
			if (consoleWorkingArea.Controls == null || consoleWorkingArea.Controls.Count == 0)
				return;
			
			if (EasyLookCustomizerControl != null)
				EasyLookCustomizerControl.DeleteCurrentSettings();
		}
		#endregion
		
		#region Eventi del SysAdminPlugin 

		//--------------------------------------------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterLogOn")]
		public void OnAfterSysAdminLogOn(object sender, DynamicEventsArgs e)
		{
			//utilizzo la classe di info ConsoleEnvironment
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName   = e.Get("DbDefaultUser").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd    = e.Get("DbDefaultPassword").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth  = Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity"));
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = e.Get("DbServer").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName     = e.Get("DbDataSource").ToString();

			if (OnGetConsoleStatus != null)
				this.consoleEnvironmentInfo.ConsoleStatus = OnGetConsoleStatus();
		
			consoleDBConnectionString = BuildConnectionString
				(
				e.Get("DbServer").ToString(), 
				e.Get("DbServerIstance").ToString(), 
				e.Get("DbDataSource").ToString(), 
				Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity")), 
				e.Get("DbDefaultUser").ToString(), 
				e.Get("DbDefaultPassword").ToString()
				);
			
			if (EasyLookCustomizerControl != null)
				EasyLookCustomizerControl.ConnectionString = consoleDBConnectionString;
			
			// se l'Edition è Standard, allora controllo quali aziende ha caricato il SysAdmin nel suo tree
			// ed carico nel tree dell'Auditing solo quelle...
			if (isStandardEdition)
				companiesIdAdmitted = ((StringCollection)(e.Get("CompaniesIdAdmitted")));

			UpdateConsoleTree();
		}

		//--------------------------------------------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterLogOff")]
		public void OnAfterSysAdminLogOff(object sender, System.EventArgs e)
		{
			ClearEasyLookCustomizerControl(true);
			
			PlugInTreeNode rootPlugInNode = GetPlugInRootNode();
			if (rootPlugInNode != null)
				rootPlugInNode.Remove();

			consoleDBConnectionString = String.Empty;

			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName   = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd    = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth  = false;
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName     = string.Empty;
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterAddGuestUser")]
		public void AfterAddGuestUser(string guestUserName, string guestUserPwd)
		{
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.Exist    = true;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = guestUserName;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd  = guestUserPwd;
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterDeleteGuestUser")]
		public void AfterDeleteGuestUser (object sender, System.EventArgs e)
		{
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.Exist    = false;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd  = string.Empty;
		}
		#endregion
		
		#region Eventi sulla Company
		// Cancellazione di una azienda 
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnDeleteCompanyFromSysAdmin")]
		public void OnAfterConsoleDeleteCompany(object sender, string companyId)
		{
			RebuildEasyLookCustomizerPlugInConsoleSubtree();

			EasyLookCustomSettings.DeleteAllCompanySettings(consoleDBConnectionString, Convert.ToInt32(companyId));
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterSavedCompany")]
		public void OnAfterConsoleSaveNewCompany(object sender, string companyId)
		{
			RebuildEasyLookCustomizerPlugInConsoleSubtree();
		}
		#endregion

		#region Eventi sugli utenti 
		// Cancellazione di un utente applicativo (utente che può essere associato 
		// all’azienda come no, se risulta associato verrà cancellato
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnDeleteUserToPlugIns")]
		public void OnAfterConsoleDeleteUser(object sender, string loginId)
		{
			PlugInTreeNode companiesNode = GetCompaniesTreeNode();
			if (companiesNode == null)
				return;

			foreach (PlugInTreeNode companyNode in companiesNode.Nodes)
			{
				foreach (PlugInTreeNode node in companyNode.Nodes)
				{
					if (String.Compare(node.Type, "EasyLookCustomizerCompanyUsers") == 0)
					{
						for (int i = node.Nodes.Count - 1; i >= 0; i--)
						{
							PlugInTreeNode userNode = node.Nodes[i] as PlugInTreeNode;
							if (userNode != null && String.Compare(userNode.Id, loginId) == 0)
							{
								userNode.Remove();
								EasyLookCustomSettings.DeleteAllCompanyUserSettings(consoleDBConnectionString, Convert.ToInt32(loginId), Convert.ToInt32(companyNode.Id));
							}
						}
					}
				}
			}
		}
		#endregion

		#region Eventi sugli utenti associati alla company
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnSaveCompanyUser")]
		public void OnAfterConsoleSaveCompanyUser(object sender, string id, string companyId)
		{ 
			PlugInTreeNode companyNode = GetCompanyTreeNode(companyId);
			if (companyNode == null)
				return;
			
			companyNode.Nodes.Clear();
			LoadAllCompanyUsers(companyNode);
		}
		
		// Se è stato clonato un utente in una azienda è necessario rileggere 
		// tutti gli utenti di questa azienda
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterClonedUserCompany")]
		public void OnAfterClonedUserCompany(string companyId)
		{
			PlugInTreeNode companyNode = GetCompanyTreeNode(companyId);
			if (companyNode == null)
				return;
			
			companyNode.Nodes.Clear();
			LoadAllCompanyUsers(companyNode);
		}

		// Cancella un utente associato a una azienda
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterDeleteCompanyUser")]
		public void OnAfterConsoleDeleteCompanyUser(object sender, string loginId, string companyId)
		{
			PlugInTreeNode companyUserNode = GetCompanyUserTreeNode(companyId, loginId);
			if (companyUserNode != null)
				companyUserNode.Remove();
	
			EasyLookCustomSettings.DeleteAllCompanyUserSettings(consoleDBConnectionString, Convert.ToInt32(loginId), Convert.ToInt32(companyId));
		}
		#endregion

		#region Eventi sul Tree della Console
		//--------------------------------------------------------------------------------------------------------
		public void OnAfterSelectConsoleTree(object sender, TreeViewEventArgs e)
		{
			ClearEasyLookCustomizerControl(false);

			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTreeView.SelectedNode;
			if (String.Compare(selectedNode.AssemblyName, easyLookCustomizerPlugInName, true, CultureInfo.InvariantCulture) != 0)
				return;

			DisableConsoleToolBarButtons();

			// Ho visto che comunque è meglio pulire completamente la working area della
			// console visto che non tutti i PlugIn si preoccupano di eliminare da essa
			// i loro control quando non sono più "protagonisti". Se ciascuna PlugIn si
			// impegnasse, invece, ad eliminare dalla working area della console tutti e
			// soli gli oggetti aggiunti da lei, non dovrebbe essercene bisogno... 
			consoleWorkingArea.Controls.Clear();

			// Ci sono PlugIn che impostano lo stato di Visible della working area a false
			// e poi non lo ripristinano più a true...
			consoleWorkingArea.Visible = true;

			consoleWorkingAreaBottom.Enabled = false;
			consoleWorkingAreaBottom.Visible = false;

			if  (String.Compare(selectedNode.Type, "EasyLookCustomizerCompanyUsers") == 0)
			{
				int companyId = Convert.ToInt32(selectedNode.CompanyId);
				if (companyId != -1)
					ShowEasyLookCustomizerControl(companyId);
			}
			else if (String.Compare(selectedNode.Type, "EasyLookCustomizerUserId") == 0)
			{
				int companyId = Convert.ToInt32(selectedNode.CompanyId);
				if (companyId != -1)
				{
					int loginId = Convert.ToInt32(selectedNode.Id);

					if (loginId != -1)
						ShowEasyLookCustomizerControl(companyId, loginId);
				}
			}
		}
	
		// doppio click sul nodo di tipo azienda... se non è valida dò un opportuno messaggio
		//---------------------------------------------------------------------------	
		public void OnAfterDoubleClickConsoleTree(object sender, System.EventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode) consoleTreeView.SelectedNode;
			
			// se il nodo selezionato è di tipo company e
			// se l'azienda non è valida non procedo con il lancio del PlugIn e dò un messaggio
			if (selectedNode.Type == "EasyLookCustomizerCompanyId" && !selectedNode.IsValid)
				DiagnosticViewer.ShowInformation(Strings.NoValidCompany, Strings.Attention);
		}
		#endregion

		#region EasyLookCustomizerPlugIn private methods

		private const string SQL_CONNECTION_STRING_SERVER_KEYWORD						= "Server";
		private const string SQL_CONNECTION_STRING_DATABASE_KEYWORD						= "Database";
		private const string SQL_CONNECTION_STRING_LOGIN_ACCOUNT_KEYWORD				= "User Id";
		private const string SQL_CONNECTION_STRING_LOGIN_PASSWORD_KEYWORD				= "Password";
		private const string SQL_CONNECTION_STRING_INTEGRATED_SECURITY_TOKEN			= "Integrated Security";
		private const string SQL_CONNECTION_STRING_SECURITY_SUPPORT_PROVIDER_INTERFACE	= "SSPI";
		
		//--------------------------------------------------------------------------------------------------------
		private string BuildConnectionString(string server, string serverInstance, string database, bool useWindowsAuthentication, string loginAccount, string password)
		{
			if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
			{
				Debug.Fail("EasyLookCustomizerPlugIn.BuildConnectionString Error: The passed information is not sufficient for building a connection string.");
				return String.Empty;
			}
			
			if (!string.IsNullOrEmpty(serverInstance))
				server += Path.DirectorySeparatorChar + serverInstance;
			
			string connectionString = SQL_CONNECTION_STRING_SERVER_KEYWORD + "=" + server + ";";
			connectionString +=  SQL_CONNECTION_STRING_DATABASE_KEYWORD + "=" + database + ";";

			// If we are using Windows authentication when connecting to SQL Server, we avoid embedding user
			// names and passwords in the connection string.
			// We use the Integrated Security keyword, set to a value of SSPI, to specify Windows Authentication:
			if (useWindowsAuthentication)
			{
				connectionString +=  SQL_CONNECTION_STRING_INTEGRATED_SECURITY_TOKEN;// the connection should use Windows integrated security (NT authentication)
				connectionString +=  "=";
				connectionString +=  SQL_CONNECTION_STRING_SECURITY_SUPPORT_PROVIDER_INTERFACE;
				connectionString +=  ";";
			}
			else
			{
				if (string.IsNullOrEmpty(loginAccount))
				{
					Debug.Fail("EasyLookCustomizerPlugIn.BuildConnectionString Error: the login account is void.");
					return String.Empty;
				}
				connectionString +=  SQL_CONNECTION_STRING_LOGIN_ACCOUNT_KEYWORD + "=" + loginAccount;
				if (!string.IsNullOrEmpty(password))
					connectionString +=  ";" + SQL_CONNECTION_STRING_LOGIN_PASSWORD_KEYWORD + "=" + password;
				connectionString +=  ";";
			}

			return connectionString;
		}			

		//--------------------------------------------------------------------------------------------------------
		private void  DisableConsoleToolBarButtons()
		{
			if (OnDisableSaveToolBarButton != null)
				OnDisableSaveToolBarButton(this, null);

			if (OnDisableNewToolBarButton != null)
				OnDisableNewToolBarButton(this, null);
		
			if (OnDisableOpenToolBarButton != null)
				OnDisableOpenToolBarButton(this, null);
		
			if (OnDisableDeleteToolBarButton != null)
				OnDisableDeleteToolBarButton(this, null);
		
			if (OnDisableOtherObjectsToolBarButton != null)
				OnDisableOtherObjectsToolBarButton(this, null);
		
			if (OnDisableShowSecurityIconsToolBarButton != null)
				OnDisableShowSecurityIconsToolBarButton(this, null);

			if (OnDisableApplySecurityFilterToolBarButton != null)
				OnDisableApplySecurityFilterToolBarButton(this, null);

            if (OnDisableFindSecurityObjectsToolBarButton != null)
                OnDisableFindSecurityObjectsToolBarButton(this, null);

            if (OnDisableShowAllGrantsToolBarButtonPushed != null)
                OnDisableShowAllGrantsToolBarButtonPushed(this, null);
		}
		
		//---------------------------------------------------------------------
		private void UpdateConsoleTree()
		{
			if (consoleTreeView == null)
				return;

			Image rootImage = null;
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Plugin.EasyLookCustomizer.Bitmaps.EasyLookCustomizerRootNode.bmp");
			if (imageStream != null)
				rootImage = Image.FromStream(imageStream);
			int rootImageIndex = (rootImage != null) ? consoleTreeView.ImageList.Images.Add(rootImage, Color.Magenta) : -1;

			PlugInTreeNode rootPlugInNode		= new PlugInTreeNode(Strings.RootPlugInNodeText);
			rootPlugInNode.AssemblyName			= easyLookCustomizerPlugInName;
			rootPlugInNode.AssemblyType			= typeof(EasyLookCustomizer);
			rootPlugInNode.Type					= "EasyLookCustomizer";
			rootPlugInNode.ImageIndex			= rootImageIndex;
			rootPlugInNode.SelectedImageIndex	= rootImageIndex;
			rootPlugInNode.ToolTipText 			= Strings.RootPlugInNodeToolTipText;

            PlugInTreeNode companiesPlugInNode = new PlugInTreeNode(PlugInTreeNode.GetCompaniesDefaultText);
			companiesPlugInNode.AssemblyName = easyLookCustomizerPlugInName;
			companiesPlugInNode.AssemblyType	= typeof(EasyLookCustomizer);
			companiesPlugInNode.Type			= "EasyLookCustomizerCompanies";
			companiesPlugInNode.Checked			= false;
            companiesPlugInNode.ImageIndex = companiesPlugInNode.SelectedImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;

			consoleTreeView.Nodes[consoleTreeView.Nodes.Count-1].Nodes.Add(rootPlugInNode);
			
			rootPlugInNode.Nodes.Add(companiesPlugInNode);

			LoadAllCompanies(companiesPlugInNode);
		}

		//--------------------------------------------------------------------------------------------------------
		private void RebuildEasyLookCustomizerPlugInConsoleSubtree()
		{
			PlugInTreeNode rootNode = GetPlugInRootNode();
			if (rootNode != null)
				rootNode.Remove();
				
			UpdateConsoleTree();
		}

		//---------------------------------------------------------------------
		private void LoadAllCompanies(PlugInTreeNode companiesPlugInNode)
		{
			SqlConnection consoleDBConnection = null;
			SqlCommand selectCompaniesSqlCommand = null;
			SqlDataReader companiesReader = null;

			try
			{
				consoleDBConnection = new SqlConnection(consoleDBConnectionString);
				consoleDBConnection.Open();

				// se sono nella Standard Edition allora devo caricare nel tree solo le aziende censite dal SysAdmin (le prime 2)
				string sqlSelect = 
					(isStandardEdition) 
					? "SELECT CompanyId, Company, Disabled, IsValid FROM MSD_Companies WHERE CompanyId = @companyId"
					: "SELECT CompanyId, Company, Disabled, IsValid FROM MSD_Companies";

				selectCompaniesSqlCommand = new SqlCommand(sqlSelect, consoleDBConnection);

				// Standard Edition
				if (isStandardEdition)
				{
					selectCompaniesSqlCommand.Parameters.Add("@companyId", SqlDbType.NVarChar);

					foreach (string coId in companiesIdAdmitted)
					{
						selectCompaniesSqlCommand.Parameters["@companyId"].Value = coId;
						companiesReader = selectCompaniesSqlCommand.ExecuteReader();
						AddCompanyNode(companiesPlugInNode, companiesReader);
						companiesReader.Close();
					}
				}
				else
				{
					// Professional Edition
					companiesReader = selectCompaniesSqlCommand.ExecuteReader();
					AddCompanyNode(companiesPlugInNode, companiesReader);
					companiesReader.Close();
				}

				consoleDBConnection.Close();
			}
			catch(SqlException e)
			{
				Debug.Fail("SqlException raised in EasyLookCustomizerPlugIn.LoadAllCompanies: " + e.Message);
			}
			finally
			{
				if (selectCompaniesSqlCommand != null)
					selectCompaniesSqlCommand.Dispose();

				if (consoleDBConnection != null)
				{
					if(consoleDBConnection.State == ConnectionState.Open)
						consoleDBConnection.Close();
					consoleDBConnection.Dispose();
				}
			}
		}
		
		/// <summary>
		/// Iterando sul DataReader passato come parametro aggiungo i nodi al tree delle aziende del PlugIn
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyNode(PlugInTreeNode companiesPlugInNode, SqlDataReader companiesReader)
		{
			while (companiesReader.Read())
			{
				PlugInTreeNode companyNode = new PlugInTreeNode();
				companyNode.Text				= companiesReader["Company"].ToString();
				companyNode.AssemblyName = easyLookCustomizerPlugInName;
				companyNode.AssemblyType		= typeof(EasyLookCustomizer);
				companyNode.Id					= companiesReader["CompanyId"].ToString();
				companyNode.Type				= "EasyLookCustomizerCompanyId";
				companyNode.IsValid				= Convert.ToBoolean(companiesReader["IsValid"].ToString());
                companyNode.ImageIndex = companyNode.SelectedImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
					
				int ind = companiesPlugInNode.Nodes.Add(companyNode);

				if (Convert.ToInt32(companiesReader["Disabled"]) == 1)
				{
					companyNode.ForeColor = Color.Red;
                    companiesPlugInNode.Nodes[ind].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
				}

				// se l'azienda non è valida, aggiungo l'icona di stato dell'azienda da migrare
				// e non procedo a caricare i nodi degli utenti
				if (!companyNode.IsValid)
				{
					companyNode.ForeColor = Color.Red;
                    companiesPlugInNode.Nodes[ind].StateImageIndex = PlugInTreeNode.GetCompaniesToMigrateImageIndex;
					continue;
				}

				LoadAllCompanyUsers(companyNode);
			}
		}

		//---------------------------------------------------------------------
		private void LoadAllCompanyUsers(PlugInTreeNode companyPlugInNode)
		{
			SqlConnection consoleDBConnection = null;
			SqlCommand selectUsersSqlCommand = null;
			SqlDataReader usersReader = null;

			try
			{
				consoleDBConnection = new SqlConnection(consoleDBConnectionString);
				consoleDBConnection.Open();

				//carico gli utenti associati a ciascuna azienda
                PlugInTreeNode usersNode = new PlugInTreeNode(PlugInTreeNode.GetUsersDefaultText);
				usersNode.AssemblyName = easyLookCustomizerPlugInName;
				usersNode.AssemblyType	= typeof(EasyLookCustomizer);
				usersNode.CompanyId		= companyPlugInNode.Id;
                usersNode.ImageIndex	= usersNode.SelectedImageIndex = PlugInTreeNode.GetUsersDefaultImageIndex;
				usersNode.Type			= "EasyLookCustomizerCompanyUsers";
		
				string sSelect =@"SELECT MSD_CompanyLogins.LoginId, MSD_Logins.Login FROM MSD_CompanyLogins INNER JOIN
								MSD_Logins ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId WHERE CompanyId = " + companyPlugInNode.Id;
				
				selectUsersSqlCommand = new SqlCommand(sSelect, consoleDBConnection);

				usersReader  = selectUsersSqlCommand.ExecuteReader();

				while (usersReader.Read())
				{
					string userName = usersReader["Login"].ToString();
					if (string.IsNullOrEmpty(userName) ||
						string.Compare(userName, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;
					
					PlugInTreeNode userNode = new PlugInTreeNode();
					userNode.AssemblyName = easyLookCustomizerPlugInName;
					userNode.AssemblyType       = typeof(EasyLookCustomizer);
					userNode.Text               = userName;
					userNode.Id                 = usersReader["LoginId"].ToString();
					userNode.CompanyId			= companyPlugInNode.Id;
					userNode.Type				= "EasyLookCustomizerUserId";
					userNode.ImageIndex         = userNode.SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
					usersNode.Nodes.Add(userNode);
				}

				usersReader.Close();
				companyPlugInNode.Nodes.Add(usersNode);
				consoleDBConnection.Close();
			}
			catch(SqlException exception)
			{
				Debug.Fail("SqlException raised in EasyLookCustomizerPlugIn.LoadAllCompanyUsers: " + exception.Message);
			}
			finally
			{
				if (usersReader != null && !usersReader.IsClosed)
					usersReader.Close();

				if (selectUsersSqlCommand != null)
					selectUsersSqlCommand.Dispose();

				if (consoleDBConnection != null)
				{
					if(consoleDBConnection.State == ConnectionState.Open)
						consoleDBConnection.Close();
					consoleDBConnection.Dispose();
				}
			}
		}
		
		//---------------------------------------------------------------------
		private PlugInTreeNode GetPlugInRootNode()
		{
			if 
				(
				consoleTreeView == null ||
				consoleTreeView.Nodes == null ||
				consoleTreeView.Nodes.Count == 0 ||
				consoleTreeView.Nodes[0] == null ||
				consoleTreeView.Nodes[0].Nodes == null ||
				consoleTreeView.Nodes[0].Nodes.Count == 0
				)
				return null;

			foreach (PlugInTreeNode consoleNode in consoleTreeView.Nodes[0].Nodes)
			{
				if (String.Compare(consoleNode.Type,"EasyLookCustomizer") == 0)
					return consoleNode;
			}
			return null;
		}

		//---------------------------------------------------------------------
		private PlugInTreeNode GetCompaniesTreeNode()
		{
			PlugInTreeNode rootNode = GetPlugInRootNode();
			if (rootNode == null)
				return null;

			foreach (PlugInTreeNode node in rootNode.Nodes)
			{
				if (String.Compare(node.Type, "EasyLookCustomizerCompanies") == 0)
					return node;
			}
			return null;
		}

		//---------------------------------------------------------------------
		private PlugInTreeNode GetCompanyTreeNode(string companyId)
		{
			if (companyId == null || companyId == String.Empty)
				return null;

			PlugInTreeNode companiesNode = GetCompaniesTreeNode();
			if (companiesNode == null)
				return null;

			foreach (PlugInTreeNode companyNode in companiesNode.Nodes)
			{
				if (String.Compare(companyNode.Id, companyId) == 0)
					return companyNode;
			}

			return null;
		}

		//---------------------------------------------------------------------
		private PlugInTreeNode GetCompanyUserTreeNode(string companyId, string loginId)
		{
			if (string.IsNullOrEmpty(companyId) || string.IsNullOrEmpty(loginId))
				return null;

			PlugInTreeNode companyNode = GetCompanyTreeNode(companyId);
			if (companyNode == null)
				return null;

			foreach (PlugInTreeNode node in companyNode.Nodes)
			{
				if (String.Compare(node.Type, "EasyLookCustomizerCompanyUsers") == 0)
				{
					foreach (PlugInTreeNode userNode in node.Nodes)
					{
						if (String.Compare(userNode.Id, loginId, StringComparison.InvariantCultureIgnoreCase) == 0)
							return userNode;
					}
				}
			}

			return null;
		}		
		
		//--------------------------------------------------------------------------------------------------------
		private bool ShowEasyLookCustomizerControl(int aCompanyId, int aLoginId)
		{
			SqlConnection consoleDBConnection = null;
			try
			{
				consoleDBConnection = new SqlConnection(consoleDBConnectionString);
				consoleDBConnection.Open();

				if (!EasyLookCustomizerControl.SetCurrentAuthentication(aCompanyId, aLoginId, consolePathFinder))
					return false;

				EasyLookCustomizerControl.Visible = true;
				consoleWorkingArea.Controls.Add(EasyLookCustomizerControl);

				// Abilito nella toolbar della console il pulsante per il salvataggio
				if (OnEnableSaveToolBarButton != null)
					OnEnableSaveToolBarButton(this, System.EventArgs.Empty);

				return true;
			}
			catch(Exception exception)
			{
				Debug.Fail("SqlException raised in EasyLookCustomizerPlugIn.ShowEasyLookCustomizerControl: " + exception.Message);
				return false;
			}
			finally
			{
				if (consoleDBConnection != null)
				{
					if (consoleDBConnection.State == ConnectionState.Open)
						consoleDBConnection.Close();
					consoleDBConnection.Dispose();
				}
			}
		}
		
		//--------------------------------------------------------------------------------------------------------
		private bool ShowEasyLookCustomizerControl(int aCompanyId)
		{
			return ShowEasyLookCustomizerControl(aCompanyId, -1);
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void ClearEasyLookCustomizerControl(bool closeConnection)
		{
			if (EasyLookCustomizerControl == null)
				return;

			EasyLookCustomizerControl.Clear();

			if (closeConnection)
				CloseEasyLookCustomizerControlConnection();

			EasyLookCustomizerControl.Visible = false;
			consoleWorkingArea.Controls.Remove(EasyLookCustomizerControl);
		}

		//--------------------------------------------------------------------------------------------------------
		private void CloseEasyLookCustomizerControlConnection()
		{
			if (EasyLookCustomizerControl == null)
				return;

			EasyLookCustomizerControl.CloseConnection();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EasyLookCustomizerControl_SettingsExistenceChanged(object sender, bool exist)
		{
			if (EasyLookCustomizerControl == null || sender != EasyLookCustomizerControl)
				return;

			if (exist)
			{
				if (OnEnableDeleteToolBarButton != null) 
					OnEnableDeleteToolBarButton(this, System.EventArgs.Empty);

			}
			else
			{
				if (OnDisableDeleteToolBarButton != null) 
					OnDisableDeleteToolBarButton(this, System.EventArgs.Empty);
			}
		}
		#endregion
	}
}