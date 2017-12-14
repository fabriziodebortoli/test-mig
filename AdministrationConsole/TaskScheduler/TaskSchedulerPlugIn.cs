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

using Microarea.Console.Core.TaskSchedulerWindowsControls;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Plugin.TaskScheduler
{
	/// <summary>
	/// Summary description for TaskSchedulerPlugIn.
	/// </summary>
	//============================================================================================================
	public class TaskScheduler : PlugIn
	{
		# region Varibili private
		private static readonly string taskSchedulerPlugInName = Assembly.GetExecutingAssembly().GetName().Name;
		private PathFinder			            consolePathFinder = null;
        private System.Windows.Forms.MenuStrip  consoleMenu = null;
		private Form				            consoleMainForm = null;
        private PlugInsTreeView                 consoleTreeView = null;
		private Panel				            consoleWorkingArea = null;
		private Panel				            consoleWorkingAreaBottom = null;
		private string				            buildArgument = String.Empty;
		private string				            configurationArgument = String.Empty;
		private bool				            runningFromServerArgument = false;
		private string				            consoleDBConnectionString = String.Empty;

		//info di ambiente di console
		private ConsoleEnvironmentInfo consoleEnvironmentInfo ;

		private bool				isStandardEdition	= false;
		private StringCollection	companiesIdAdmitted = null;

		private TBSchedulerControl taskBuilderSchedulerControl;
		private TBSchedulerGenericInfoControl taskSchedulerGenericInfoControl;
		#endregion

		#region Eventi per la disabilitazione dei pulsanti della ToolBar del SysAdmin

		// Disabilito il pulsante Salva
		public event System.EventHandler OnDisableSaveToolBarButton;

		// Disabilito il pulsante New
		public event System.EventHandler OnDisableNewToolBarButton;

		// Disabilito il pulsante Open
		public event System.EventHandler OnDisableOpenToolBarButton;

		// Disabilito il pulsante Delete
		public event System.EventHandler OnDisableDeleteToolBarButton;		

		#endregion

		public delegate StatusType GetConsoleStatus();
		public event GetConsoleStatus OnGetConsoleStatus;

		public event StartSchedulerAgentEventHandler  OnStartSchedulerAgent;
		public event System.EventHandler  OnStopSchedulerAgent;
		public event WTETaskExecutionEngine.TaskExecutionEndedEventHandler TaskExecutionEnded = null;

		#region Eventi per la disabilitazione dei pulsanti della ToolBar della Security
		// Disabilito il pulsante per il caricamento degli oggetti messi sotto protezione
		public event System.EventHandler OnDisableOtherObjectsToolBarButton;
		// Disabilito il pulsante che abilita la visualizzazione delle icone della sicurezza 
		public event System.EventHandler OnDisableShowSecurityIconsToolBarButton;
		// Disabilito il pulsante per l'applicazione del filtro di sicurezza
		public event System.EventHandler OnDisableApplySecurityFilterToolBarButton;

        public event System.EventHandler OnDisableFindSecurityObjectsToolBarButton;

        public event System.EventHandler OnDisableShowAllGrantsToolBarButtonPushed;

		#endregion

		# region Constructor
		//--------------------------------------------------------------------------------------------------------
		public TaskScheduler()
		{
			consoleDBConnectionString = String.Empty;
		}

		
		# endregion

		#region Load
		//--------------------------------------------------------------------------------------------------------
		public override void Load
			(
				ConsoleGUIObjects		consoleGUIObjects,
				ConsoleEnvironmentInfo	consoleEnvironmentInfo,
				LicenceInfo				licenceInfo
			)
		{
			try
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

				taskSchedulerGenericInfoControl = new TBSchedulerGenericInfoControl();
				taskSchedulerGenericInfoControl.ConnectionString = String.Empty;
				taskSchedulerGenericInfoControl.Dock = System.Windows.Forms.DockStyle.Fill;
				taskSchedulerGenericInfoControl.Location = new System.Drawing.Point(0, 33);
				taskSchedulerGenericInfoControl.Name = "TBSchedulerGenericInfoControl";
				taskSchedulerGenericInfoControl.TabIndex = 0;
				taskSchedulerGenericInfoControl.Visible = false;
				taskSchedulerGenericInfoControl.OnStartSchedulerAgent += new StartSchedulerAgentEventHandler(this.TaskBuilderScheduler_OnStartSchedulerAgent);
				taskSchedulerGenericInfoControl.OnStopSchedulerAgent += new System.EventHandler(this.TaskBuilderScheduler_OnStopSchedulerAgent);
				
				taskBuilderSchedulerControl = new TBSchedulerControl();
				taskBuilderSchedulerControl.IsLiteConsole = this.consoleEnvironmentInfo.IsLiteConsole;
				taskBuilderSchedulerControl.ConnectionString = String.Empty;
				taskBuilderSchedulerControl.Dock = System.Windows.Forms.DockStyle.Fill;
				taskBuilderSchedulerControl.Location = new System.Drawing.Point(0, 33);
				taskBuilderSchedulerControl.MenuLoader = null;
				taskBuilderSchedulerControl.Name = "TaskBuilderSchedulerControl";
				taskBuilderSchedulerControl.TabIndex = 0;
				taskBuilderSchedulerControl.Visible = false;
				
				taskBuilderSchedulerControl.GetMenuLoaderInstance += new TaskBuilderSchedulerControlEventHandler(this.TaskBuilderSchedulerControl_GetMenuLoaderInstance);
				taskBuilderSchedulerControl.OnStartSchedulerAgent += new StartSchedulerAgentEventHandler(this.TaskBuilderScheduler_OnStartSchedulerAgent);
				taskBuilderSchedulerControl.OnStopSchedulerAgent += new System.EventHandler(this.TaskBuilderScheduler_OnStopSchedulerAgent);
				taskBuilderSchedulerControl.TaskExecutionEnded += new WTETaskExecutionEngine.TaskExecutionEndedEventHandler(TaskBuilderSchedulerControl_TaskExecutionEnded);
			}
			catch (Exception exception)
			{
				MessageBox.Show("Exception thrown in TaskSchedulerPlugIn.Load:" + exception.Message);
			}
		}

		/// <summary>
		/// OnAfterConnectButton
		/// Intercetta la pressione del bottone Connect dalla Console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnConnect")]
		public void OnAfterConnectButton (object sender, System.EventArgs e)
		{
			//da linea di comando posso lanciare in automatico lo scheduler alla connessione
			CommandLineParam[] cmds = CommandLineParam.FromCommandLine();
			foreach (CommandLineParam cmd in cmds)
			{
				if (string.Compare(cmd.Name, "startScheduler", StringComparison.InvariantCultureIgnoreCase) == 0 &&
					string.Compare(cmd.Value, "true", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					if (!string.IsNullOrEmpty(taskBuilderSchedulerControl.ConnectionString))
						TaskBuilderScheduler_OnStartSchedulerAgent(sender, taskBuilderSchedulerControl.ConnectionString);
					break;
				}
			}
		}

		# endregion

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

            if (OnDisableFindSecurityObjectsToolBarButton!= null)
                OnDisableFindSecurityObjectsToolBarButton(this, null);

			if (OnDisableApplySecurityFilterToolBarButton != null)
				OnDisableApplySecurityFilterToolBarButton(this, null);

            if (OnDisableFindSecurityObjectsToolBarButton!= null)
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
            Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Plugin.TaskScheduler.Bitmaps.Clock.bmp");
			if (imageStream != null)
				rootImage = Image.FromStream(imageStream);
            int rootImageIndex = (rootImage != null) ? consoleTreeView.ImageList.Images.Add(rootImage, Color.Magenta) : -1;

			PlugInTreeNode rootPlugInNode		= new PlugInTreeNode(Strings.RootPlugInNodeText);
			rootPlugInNode.AssemblyName = taskSchedulerPlugInName;
			rootPlugInNode.AssemblyType			= typeof(TaskScheduler);
			rootPlugInNode.Type					= "TaskScheduler";
			rootPlugInNode.ImageIndex			= rootImageIndex;
			rootPlugInNode.SelectedImageIndex	= rootImageIndex;
			rootPlugInNode.ToolTipText 			= Strings.RootPlugInNodeToolTipText;

            PlugInTreeNode companiesPlugInNode = new PlugInTreeNode(PlugInTreeNode.GetCompaniesDefaultText);
			companiesPlugInNode.AssemblyName = taskSchedulerPlugInName;
			companiesPlugInNode.AssemblyType	= typeof(TaskScheduler);
			companiesPlugInNode.Type			= "TaskSchedulerPlugInCompanies";
			companiesPlugInNode.Checked			= false;
            companiesPlugInNode.ImageIndex = companiesPlugInNode.SelectedImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;

			consoleTreeView.Nodes[consoleTreeView.Nodes.Count-1].Nodes.Add(rootPlugInNode);
			rootPlugInNode.Nodes.Add(companiesPlugInNode);
			
			LoadAllCompanies(companiesPlugInNode);
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
				Debug.Fail("SqlException raised in TaskSchedulerPlugIn.LoadAllCompanies: " + e.Message);
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
				companyNode.AssemblyName = taskSchedulerPlugInName;
				companyNode.AssemblyType		= typeof(TaskScheduler);
				companyNode.Id					= companiesReader["CompanyId"].ToString();
				companyNode.Type				= "TaskSchedulerCompanyId";
				companyNode.IsValid				= Convert.ToBoolean(companiesReader["IsValid"].ToString());
				companyNode.ImageIndex			= companyNode.SelectedImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
					
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
				usersNode.AssemblyName = taskSchedulerPlugInName;
				usersNode.AssemblyType		= typeof(TaskScheduler);
				usersNode.CompanyId			= companyPlugInNode.Id;
                usersNode.ImageIndex = usersNode.SelectedImageIndex = PlugInTreeNode.GetUsersDefaultImageIndex;
				usersNode.Type				= "TaskSchedulerPlugInCompanyUsers";
		
				string sSelect =@"SELECT MSD_CompanyLogins.LoginId, MSD_Logins.Login FROM MSD_CompanyLogins INNER JOIN
								MSD_Logins ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId WHERE CompanyId = " + companyPlugInNode.Id;
				
				selectUsersSqlCommand = new SqlCommand(sSelect, consoleDBConnection);

				usersReader  = selectUsersSqlCommand.ExecuteReader();

				while (usersReader.Read())
				{
					string userName = usersReader["Login"].ToString();

					if (string.IsNullOrEmpty(userName) ||
						string.Compare(userName, NameSolverStrings.EasyLookSystemLogin, StringComparison.InvariantCultureIgnoreCase) == 0 ||
						string.Compare(userName, NameSolverStrings.GuestLogin, StringComparison.InvariantCultureIgnoreCase) == 0)
						continue;

					PlugInTreeNode userNode		= new PlugInTreeNode();
					userNode.AssemblyName = taskSchedulerPlugInName;
					userNode.AssemblyType       = typeof(TaskScheduler);
					userNode.Text               = userName;
					userNode.Id                 = usersReader["LoginId"].ToString();
					userNode.CompanyId			= companyPlugInNode.Id;
					userNode.Type				= "TaskSchedulerUserId";
                    userNode.ImageIndex = userNode.SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
					usersNode.Nodes.Add(userNode);
				}

				usersReader.Close();

				companyPlugInNode.Nodes.Add(usersNode);

				consoleDBConnection.Close();
			}
			catch(SqlException exception)
			{
				Debug.Fail("SqlException raised in TaskSchedulerPlugIn.LoadAllCompanyUsers: " + exception.Message);
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
		
		//--------------------------------------------------------------------------------------------------------
		private void RebuildTaskSchedulerPlugInConsoleSubtree()
		{
			PlugInTreeNode rootNode = GetTaskSchedulerPlugInRootNode();
			if (rootNode != null)
				rootNode.Remove();
				
			UpdateConsoleTree();
		}

		//--------------------------------------------------------------------------------------------------------
		private void ClearTaskSchedulerGenericInfoControl(bool closeConnection)
		{
			if (taskSchedulerGenericInfoControl == null)
				return;

			if (closeConnection)
				CloseTaskSchedulerGenericInfoControlConnection();

			taskSchedulerGenericInfoControl.Visible = false;
			consoleWorkingArea.Controls.Remove(taskSchedulerGenericInfoControl);
		}

		//--------------------------------------------------------------------------------------------------------
		private void CloseTaskSchedulerGenericInfoControlConnection()
		{
			if (taskSchedulerGenericInfoControl == null)
				return;

			taskSchedulerGenericInfoControl.CloseConnection();
		}

		//--------------------------------------------------------------------------------------------------------
		private void ClearTaskBuilderSchedulerControl(bool closeConnection)
		{
			if (taskBuilderSchedulerControl == null)
				return;

			taskBuilderSchedulerControl.ClearScheduledTasksGrid();

			if (taskBuilderSchedulerControl.MenuLoader != null)
				taskBuilderSchedulerControl.MenuLoader = null;

			if (closeConnection)
				CloseTaskBuilderSchedulerControlConnection();

			taskBuilderSchedulerControl.Visible = false;
			consoleWorkingArea.Controls.Remove(taskBuilderSchedulerControl);
		}

		//--------------------------------------------------------------------------------------------------------
		private void CloseTaskBuilderSchedulerControlConnection()
		{
			if (taskBuilderSchedulerControl == null)
				return;

			taskBuilderSchedulerControl.CloseConnection();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void TaskBuilderSchedulerControl_GetMenuLoaderInstance(object sender,  TaskBuilderSchedulerControlEventArgs e)
		{
			if (taskBuilderSchedulerControl.MenuLoader != null)
				return;


			PathFinder aPathFinder = new PathFinder(e.Company, e.User);

			MenuLoader menuLoader = new MenuLoader(aPathFinder, false);
					
			if (menuLoader != null)
			{
				menuLoader.ScanStandardMenuComponentsStarted			+= new MenuParserEventHandler(ScanStandardMenuComponentsStarted);
				menuLoader.ScanStandardMenuComponentsEnded				+= new MenuParserEventHandler(ScanStandardMenuComponentsEnded);
				menuLoader.LoadAllMenus(false, false);
			}
			taskBuilderSchedulerControl.MenuLoader = menuLoader;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void TaskBuilderScheduler_OnStartSchedulerAgent(object sender,  string connectionString)
		{
			if (OnStartSchedulerAgent != null)
				OnStartSchedulerAgent(this, connectionString);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void TaskBuilderScheduler_OnStopSchedulerAgent(object sender,  System.EventArgs e)
		{
			if (OnStopSchedulerAgent != null)
				OnStopSchedulerAgent(this, e);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void TaskBuilderSchedulerControl_TaskExecutionEnded(object sender, WTETaskExecutionEngine.TaskExecutionEndedEventArgs e)
		{
			if (TaskExecutionEnded != null)
				TaskExecutionEnded(this, e);
		}

		//---------------------------------------------------------------------
		private PlugInTreeNode GetTaskSchedulerPlugInRootNode()
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
				if (String.Compare(consoleNode.Type,"TaskScheduler") == 0)
					return consoleNode;
			}
			return null;
		}

		//---------------------------------------------------------------------
		private PlugInTreeNode GetTaskSchedulerPlugInCompaniesTreeNode()
		{
			PlugInTreeNode rootNode = GetTaskSchedulerPlugInRootNode();
			if (rootNode == null)
				return null;

			foreach (PlugInTreeNode node in rootNode.Nodes)
			{
				if (String.Compare(node.Type, "TaskSchedulerPlugInCompanies") == 0)
					return node;
			}
			return null;
		}

		//---------------------------------------------------------------------
		private PlugInTreeNode GetTaskSchedulerPlugInCompanyTreeNode(string companyId)
		{
			if (companyId == null || companyId == String.Empty)
				return null;

			PlugInTreeNode companiesNode = GetTaskSchedulerPlugInCompaniesTreeNode();
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
		private PlugInTreeNode GetTaskSchedulerPlugInCompanyUserTreeNode(string companyId, string loginId)
		{
			if (companyId == null || companyId == String.Empty || loginId == null || loginId == String.Empty)
				return null;

			PlugInTreeNode companyNode = GetTaskSchedulerPlugInCompanyTreeNode(companyId);
			if (companyNode == null)
				return null;

			foreach (PlugInTreeNode node in companyNode.Nodes)
			{
				if (String.Compare(node.Type, "TaskSchedulerPlugInCompanyUsers") == 0)
				{
					foreach (PlugInTreeNode userNode in node.Nodes)
					{
						if (String.Compare(userNode.Id, loginId) == 0)
							 return userNode;
					}
				}
			}

			return null;
		}

		#region Eventi del SysAdminPlugin 
		
		//--------------------------------------------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterLogOn")]
		public void OnAfterSysAdminLogOn(object sender, DynamicEventsArgs e)
		{
			CloseTaskSchedulerGenericInfoControlConnection();
			CloseTaskBuilderSchedulerControlConnection();

			if (taskSchedulerGenericInfoControl == null || taskBuilderSchedulerControl == null)
				return;

			//utilizzo la classe di info ConsoleEnvironment
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName   = e.Get("DbDefaultUser").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd    = e.Get("DbDefaultPassword").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth  = Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity"));
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = e.Get("DbServer").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName     = e.Get("DbDataSource").ToString();

			if (OnGetConsoleStatus != null)
				this.consoleEnvironmentInfo.ConsoleStatus = OnGetConsoleStatus();

			consoleDBConnectionString = WTEScheduledTask.BuildConnectionString(e.Get("DbServer").ToString(), e.Get("DbServerIstance").ToString(), e.Get("DbDataSource").ToString(), Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity")), e.Get("DbDefaultUser").ToString(), e.Get("DbDefaultPassword").ToString());
			
			if (consoleDBConnectionString != null && consoleDBConnectionString != String.Empty)
			{
				taskSchedulerGenericInfoControl.ConnectionString = consoleDBConnectionString;
				taskBuilderSchedulerControl.ConnectionString = consoleDBConnectionString;
			}

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
			ClearTaskSchedulerGenericInfoControl(true);
			ClearTaskBuilderSchedulerControl(true);

			PlugInTreeNode rootPlugInNode = GetTaskSchedulerPlugInRootNode();
			if (rootPlugInNode != null)
				rootPlugInNode.Remove();

			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName   = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd    = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth  = false;
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName     = string.Empty;
		}
		
		#region Eventi sulla Company

		//---------------------------------------------------------------------
		// Cancellazione di una azienda 
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnDeleteCompanyFromSysAdmin")]
		public void OnAfterConsoleDeleteCompany(object sender, string companyId)
		{
			RebuildTaskSchedulerPlugInConsoleSubtree();
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterSavedCompany")]
		public void OnAfterConsoleSaveNewCompany(object sender, string companyId)
		{
			RebuildTaskSchedulerPlugInConsoleSubtree();
		}

		#endregion

		#region Eventi sugli utenti 
		
		//---------------------------------------------------------------------
		// Cancellazione di un utente applicativo (utente che può essere associato 
		// all’azienda come no, se risulta associato verrà cancellato
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnDeleteUserToPlugIns")]
		public void OnAfterConsoleDeleteUser(object sender, string loginId)
		{
			PlugInTreeNode companiesNode = GetTaskSchedulerPlugInCompaniesTreeNode();
			if (companiesNode == null)
				return;

			foreach (PlugInTreeNode companyNode in companiesNode.Nodes)
			{
				foreach (PlugInTreeNode node in companyNode.Nodes)
				{
					if (String.Compare(node.Type, "TaskSchedulerPlugInCompanyUsers") == 0)
					{
						for (int i = node.Nodes.Count - 1; i >= 0; i--)
						{
							PlugInTreeNode userNode = node.Nodes[i] as PlugInTreeNode;
							if (userNode != null && String.Compare(userNode.Id, loginId) == 0)
								userNode.Remove();
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
			PlugInTreeNode companyNode = GetTaskSchedulerPlugInCompanyTreeNode(companyId);
			if (companyNode == null)
				return;
			
			companyNode.Nodes.Clear();
			LoadAllCompanyUsers(companyNode);
		}
		
		//---------------------------------------------------------------------
		// Se è stato clonato un utente in una azienda è necessario rileggere 
		// tutti gli utenti di questa azienda
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterClonedUserCompany")]
		public void OnAfterClonedUserCompany(string companyId)
		{
			PlugInTreeNode companyNode = GetTaskSchedulerPlugInCompanyTreeNode(companyId);
			if (companyNode == null)
				return;
			
			companyNode.Nodes.Clear();
			LoadAllCompanyUsers(companyNode);
		}

		//---------------------------------------------------------------------
		// Cancella un utente associato a una azienda
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterDeleteCompanyUser")]
		public void OnAfterConsoleDeleteCompanyUser(object sender, string loginId, string companyId)
		{
			PlugInTreeNode companyUserNode = GetTaskSchedulerPlugInCompanyUserTreeNode(companyId, loginId);
			if (companyUserNode != null)
				companyUserNode.Remove();
		}

		#endregion

		#region Eventi sull'utente Guest

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterAddGuestUser")]
		public void AfterAddGuestUser (string guestUserName, string guestUserPwd)
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

		#endregion

		#region Eventi della MicroareaConsole 

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnInitPathFinder")]
		public void OnInitPathFinder(PathFinder pathFinder)
		{
			consolePathFinder = pathFinder;
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnInitBrandLoader")]
		public void OnInitBrandLoader(BrandLoader aBrandLoader)
		{
			if (taskBuilderSchedulerControl != null)
				taskBuilderSchedulerControl.BrandLoader = aBrandLoader;
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnRefreshItem")]
		public void OnAfterClickRefreshConsoleButton(object sender, System.EventArgs e)
		{
			if 
				(
				consoleTreeView == null ||
				consoleTreeView.Nodes == null ||
				consoleTreeView.Nodes.Count == 0 ||
				consoleTreeView.SelectedNode == null
				)
				return;

			PlugInTreeNode plugInRoot = GetTaskSchedulerPlugInRootNode();
			if (plugInRoot == null)
				return;

			if 
				(
				consoleTreeView.SelectedNode == plugInRoot ||
				((PlugInTreeNode)consoleTreeView.SelectedNode).IsDescendantOf(plugInRoot)
				)
				taskBuilderSchedulerControl.ShowScheduledTasksEventLogEntries();
		}
			
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnSchedulerAgentStarted")]
		public void OnSchedulerAgentStarted(object sender, System.EventArgs e)
		{
			if (taskSchedulerGenericInfoControl != null)
				taskSchedulerGenericInfoControl.UpdateStartStopSchedulerAgentStatus(true);
			
			if (taskBuilderSchedulerControl != null)
				taskBuilderSchedulerControl.UpdateStartStopSchedulerAgentStatus(true);
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnSchedulerAgentStopped")]
		public void OnSchedulerAgentStopped(object sender, System.EventArgs e)
		{
			if (taskSchedulerGenericInfoControl != null)
				taskSchedulerGenericInfoControl.UpdateStartStopSchedulerAgentStatus(false);

			if (taskBuilderSchedulerControl != null)
				taskBuilderSchedulerControl.UpdateStartStopSchedulerAgentStatus(false);
		}
		
		#endregion

		#region Eventi sul Tree della Console
		//--------------------------------------------------------------------------------------------------------
		public void OnAfterSelectConsoleTree(object sender, TreeViewEventArgs e)
		{
			ClearTaskBuilderSchedulerControl(false);

			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTreeView.SelectedNode;
			if (String.Compare(selectedNode.AssemblyName, taskSchedulerPlugInName, true, CultureInfo.InvariantCulture) != 0)
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

			if(String.Compare(selectedNode.Type, "TaskScheduler") == 0)
			{
				taskSchedulerGenericInfoControl.Visible = true;
				consoleWorkingArea.Controls.Add(taskSchedulerGenericInfoControl);
			}
			else if(String.Compare(selectedNode.Type, "TaskSchedulerUserId") == 0)
			{
				SqlConnection consoleDBConnection = new SqlConnection(consoleDBConnectionString);
				consoleDBConnection.Open();

				int companyId = Convert.ToInt32(selectedNode.CompanyId);
				int loginId = Convert.ToInt32(selectedNode.Id);

				if (taskBuilderSchedulerControl.SetCurrentAuthentication(loginId, companyId))
				{
					taskBuilderSchedulerControl.Visible = true;
					consoleWorkingArea.Controls.Add(taskBuilderSchedulerControl);
				}
				consoleDBConnection.Close();
			}
		}

		// doppio click sul nodo di tipo azienda... se non è valida dò un opportuno messaggio
		//---------------------------------------------------------------------------	
		public void OnAfterDoubleClickConsoleTree(object sender, System.EventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTreeView.SelectedNode;
			
			// se il nodo selezionato è di tipo company e
			// se l'azienda non è valida non procedo con il lancio del PlugIn e dò un messaggio
			if (selectedNode.Type == "TaskSchedulerCompanyId" && !selectedNode.IsValid)
				DiagnosticViewer.ShowInformation(Strings.NoValidCompany, Strings.Attention);
		}
		#endregion

		#region Eventi in fase di caricamento dei menù
		//----------------------------------------------------------------------------
		private void ScanStandardMenuComponentsStarted(object sender, MenuParserEventArgs e)
		{
			EnableProgressBarFromPlugIn(this);
						
			SetProgressBarStepFromPlugIn(this, 1);
			
			SetProgressBarMinValueFromPlugIn(this, 0);
			SetProgressBarMaxValueFromPlugIn(this, e.Counter);

			SetProgressBarTextFromPlugIn(this, Strings.ScanStandardMenuComponentsProgressBarText);

			SetProgressBarValueFromPlugIn (this, 0);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		private void ScanStandardMenuComponentsEnded(object sender, MenuParserEventArgs e)
		{
			SetProgressBarTextFromPlugIn(this, String.Empty);

			DisableProgressBarFromPlugIn(this);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}
		
		#endregion
	}
}