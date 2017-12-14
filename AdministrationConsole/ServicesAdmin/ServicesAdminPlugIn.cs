using System;
using System.Collections;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.ServicesAdmin
{
	//=========================================================================
	public class ServicesAdmin : PlugIn
	{
		#region Events and delegates
		// Il SysAdmin mi ritorna l'elenco delle aziende in MSD_Companies
		public delegate SqlDataReader GetCompanies();
		public event GetCompanies OnGetCompanies;

		// Il SysAdmin mi ritorna l'elenco degli utenti presenti nella MSD_Logins
		public delegate SqlDataReader GetApplicationUsers(bool localServer, string serverName);
		public event GetApplicationUsers OnGetApplicationUsers;

		// Il SysAdmin mi ritorna l'elenco degli utenti associati a una azienda
		public delegate SqlDataReader GetCompanyUsers(string companyId);
		public event GetCompanyUsers OnGetCompanyUsers;

		// Il SysAdmin mi ritorna i record tracciati
		public delegate SqlDataReader GetRecordsTraced(string allString, string company, string user, TraceActionType operationType, DateTime fromDate, DateTime toDate);
		public event GetRecordsTraced OnGetRecordsTraced;

		// Il SysAdmin cancella i record tracciati
		public delegate bool DeleteRecordsTraced(DateTime toDate);
		public event DeleteRecordsTraced OnDeleteRecordsTraced;

		// Il SysAdmin cancella i record tracciati
		public delegate bool AddRootPlugInTreeNode(PlugInTreeNode rootNode);
		public event AddRootPlugInTreeNode OnAddRootPlugInTreeNode;

		// evento per chiedere alla Console l'authentication token
		public delegate string GetAuthenticationToken();
		public event GetAuthenticationToken OnGetAuthenticationToken;

		#region Eventi per pulsanti SysAdmin
		//Disabilito il pulsante Salva ----------------------------------------
		public event System.EventHandler OnDisableSaveToolBarButton;
		//Disabilito il pulsante New ------------------------------------------
		public event System.EventHandler OnDisableNewToolBarButton;
		//Disabilito il pulsante Open -----------------------------------------
		public event System.EventHandler OnDisableOpenToolBarButton;
		//Disabilito il pulsante Delete ---------------------------------------
		public event System.EventHandler OnDisableDeleteToolBarButton;		
		//Faccio vedere il pulsante Salva -------------------------------------
		public event System.EventHandler OnEnableSaveToolBarButton;
		//Abilita pulsante Cancella della Console------------------------------
		public event System.EventHandler OnEnableDeleteToolBarButton;
		#endregion

		#region Eventi per i pulsanti della security
		//Disabilita il bottone degli OtherObject Reference e DataBase ---------
		public event System.EventHandler OnDisableOtherObjectsToolBarButton;
		//Disabilito il pulsante xhe mostra il Tree con le icone di OSL -------
		public event System.EventHandler OnDisableShowSecurityIconsToolBarButton;
		//Abilito il pulsante che mostra il Tree con le icone di OSL ----------
		public event System.EventHandler OnDisableApplySecurityFilterToolBarButton;

        public event System.EventHandler OnDisableFindSecurityObjectsToolBarButton;

        public event System.EventHandler OnDisableShowAllGrantsToolBarButtonPushed;
		#endregion

		#region Eventi per i pulsanti dell'Auditing 
		//Disabilita il pulsante x le Query -----------------------------------
		public event System.EventHandler OnDisableQueryToolBarButton;
		#endregion

		#endregion

		#region Forms Controls
        private System.Windows.Forms.MenuStrip  consoleMenu;
        private PlugInsTreeView                 consoleTree;
		private Panel				            workingAreaConsole;
		private Panel				            workingAreaConsoleBottom;
		#endregion

		#region Public variables
		//---------------------------------------------------------------------
		public bool					isRunningFormServer			= true;
		public ConnectionParameters	connectionParameters;
		public SqlConnection		sqlConnection;
		public ArrayList			settingsConfigInfoArray		= null;		
		public bool					allSectionConfiguration		= false;
		public ContextMenu			applicationServecontextMenu = null;
		#endregion

		#region Private Variables
		//---------------------------------------------------------------------
		private int				indexIconSettings			= -1;
		private int				indexIcon					= -1;
		private int				indexIconArticols			= -1;
		private int				indexIconSessionContainer	= -1;

		private PathFinder pathFinder = null;
		private	LockManager lockManager = null;
		private	LoginManager loginManager = null;
		private TbServices tbService = null;
		private EasyAttachmentSync eaSync = null;
        private TbSenderWrapper tbsender = null;
		private TbHermesWrapper tbHermes;
		private DataSynchronizer dataSynch = null;

		private Diagnostic diagnostic = new Diagnostic("ServicesAdminPlugIn");

		private ConsoleEnvironmentInfo consoleEnvironmentInfo;
		private LicenceInfo			   licenceInfo;
		#endregion

		#region ConnectionParameters structure
		//---------------------------------------------------------------------
		public struct ConnectionParameters
		{
			public string   DbName;
			public string   ServerName;
			public string   Password;
			public string   UserId;
			public string   Instance;
			public int      CodCompany;
			public string   ObjectNameSpace;
			public string   Type;
			public int      CodSelTree;
			public string   NameSpace;
			public string   ObjectName;
			public string   TreeSelectString;
			public bool     WindowsAuthentication;

			//---------------------------------------------------------------------
			public ConnectionParameters(int o)
			{
				DbName                = string.Empty; 
				ServerName            = string.Empty; 
				Password              = string.Empty; 
				UserId                = string.Empty; 
				Instance              = string.Empty; 
				CodCompany            = -1;
				ObjectNameSpace       = string.Empty;
				Type                  = string.Empty;
				CodSelTree            =  0;
				NameSpace             = string.Empty;
				ObjectName            = string.Empty;
				TreeSelectString      = string.Empty;
				WindowsAuthentication = false;
			}
		}
		#endregion

		#region Constructor
		//---------------------------------------------------------------------
		public ServicesAdmin()
		{
		}
		#endregion
		
		#region SysAdminPlugin Events

		#region Abilito / Disabilito Login (OnAfterChangeDisabledLoginFlag)
		//---------------------------------------------------------------------		
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterChangedDisabledLogin")]
		public void OnAfterChangeDisabledLoginFlag(object sender, string loginId, bool disabled)
		{
			if (!disabled) 
				return;
			
			SetArticles.DeleteUserProducts(Convert.ToInt32(loginId), sqlConnection);
		}
		#endregion

		#region Cancello Utente (OnAfterChangedDisabledCompanyLogin)
		//---------------------------------------------------------------------		
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterChangedDisabledCompanyLogin")]
		public void OnAfterChangedDisabledCompanyLogin(object sender, string companyId, string loginId, bool disabled)
		{
			if (!disabled) 
				return;
			
			SetArticles.DeleteUserProducts(Convert.ToInt32(loginId), sqlConnection);
		}

		# region OnDeleteUserToPlugIns
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnDeleteUserToPlugIns")]
		public void OnAfterConsoleCompanyUser (object sender, string id)
		{
			SetArticles.DeleteUserProducts(Convert.ToInt32(id), sqlConnection);
		}
		#endregion

		#endregion

		#region Utente Guest (OnAfterAddGuestUser + OnAfterDeleteGuestUser)
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

		# region OnAfterLogOn
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterLogOn")]
		public void OnAfterLogOn(object sender, DynamicEventsArgs e)
		{
			//utilizzo la classe di info ConsoleEnvironment
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName   = e.Get("DbDefaultUser").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd    = e.Get("DbDefaultPassword").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth  = Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity"));
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = e.Get("DbServer").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName     = e.Get("DbDataSource").ToString();

			//Valorizzo la struttura che conterrà i parametri che mi ha passato la 
			//login di Nadia per la connessione.
			connectionParameters.DbName		= e.Get("DbDataSource").ToString();
			connectionParameters.Password	= e.Get("DbDefaultPassword").ToString();
			connectionParameters.UserId     = e.Get("DbDefaultUser").ToString();
			connectionParameters.ServerName = e.Get("DbServer").ToString();
			connectionParameters.Instance   = e.Get("DbServerIstance").ToString();
			connectionParameters.WindowsAuthentication = Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity"));

            string serverName = connectionParameters.ServerName;
            if (connectionParameters.Instance.Length > 0)
                serverName += Path.DirectorySeparatorChar + connectionParameters.Instance;

            sqlConnection = new SqlConnection();
            sqlConnection.ConnectionString =
                (connectionParameters.WindowsAuthentication)
                ? string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, serverName, connectionParameters.DbName)
                : string.Format(NameSolverDatabaseStrings.SQLConnection, serverName, connectionParameters.DbName, connectionParameters.UserId, connectionParameters.Password);

            try
            {
                sqlConnection.Open();
            }
            catch (SqlException sqlExc)
            {
                diagnostic.Set(DiagnosticType.Error, sqlExc.Message);
            }

			//Aggiungo i miei rami
			UpdateConsoleTree(consoleTree, connectionParameters);
		}
		# endregion

		# region OnAfterLogOff
		//-----------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterLogOff")]
		public void OnAfterLogOff(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName   = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd    = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth  = false;
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName     = string.Empty;

			TreeNodeCollection nodeCollection = consoleTree.Nodes[0].Nodes;
			for (int i=0; i< nodeCollection.Count; i++)
			{
                PlugInTreeNode aNode = nodeCollection[i] as PlugInTreeNode;
                if (aNode != null && aNode.AssemblyName == ConstString.servicePlugIn)
				{
                    aNode.Remove();	
					i = i-1;
				}
			}
			
			workingAreaConsoleBottom.Visible = false;

			if (OnDisableSaveToolBarButton != null)
				OnDisableSaveToolBarButton(sender, e);
		}
		# endregion

		# region "Fire" di eventi alla console
		/// <summary>
		/// per avere l'elenco degli utenti applicativi (presenti nella MSD_Logins)
		/// </summary>
		//---------------------------------------------------------------------------	
		public SqlDataReader loginsDetail_OnGetApplicationUsers(bool localServer, string serverName)
		{
			if (OnGetApplicationUsers != null)
				return OnGetApplicationUsers(localServer, serverName);

			return null;
		}

		/// <summary>
		/// get elenco delle aziende in MSD_Companies
		/// </summary>
		//---------------------------------------------------------------------------	
		public SqlDataReader CallOnGetCompanies()
		{
			if (OnGetCompanies != null)
				return OnGetCompanies();

			return null;
		}

		/// <summary>
		/// get elenco degli utenti associati a un'azienda
		/// </summary>
		//---------------------------------------------------------------------------	
		public SqlDataReader loginsDetail_OnGetCompanyUsers(string companyId)
		{
			if (OnGetCompanyUsers != null)
				return OnGetCompanyUsers(companyId);
			
			return null;
		}

		/// <summary>
		/// Get dei record relativi al trace
		/// </summary>
		//---------------------------------------------------------------------------	
		public SqlDataReader loginsDetail_OnGetRecordsTraced
			(
			string company, 
			string user, 
			TraceActionType operationType, 
			DateTime fromDate, 
			DateTime toDate
			)
		{
			if (OnGetRecordsTraced != null)
				return OnGetRecordsTraced(Strings.AllElements, company, user, operationType, fromDate, toDate);
			
			return null;
		}

		/// <summary>
		/// delete dei record relativi al trace
		/// </summary>
		//---------------------------------------------------------------------------	
		public bool loginsDetail_OnDeleteRecordsTraced(DateTime toDate)
		{
			if (OnDeleteRecordsTraced != null)
				return OnDeleteRecordsTraced(toDate);
			else
				return false;
		}

		/// <summary>
		/// get dell'authentication token
		/// </summary>
		//---------------------------------------------------------------------------	
		public string webServicesDetail_OnGetAuthenticationToken()
		{
			if (OnGetAuthenticationToken != null)
				return OnGetAuthenticationToken();

			return string.Empty;
		}
		# endregion

		#endregion

		#region ServiceAdmin Console Events
		
		# region OnShutDownConsole
		/// <summary>
		/// ShutDownFromPlugIn
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShutDownConsole")]
		public override bool ShutDownFromPlugIn()
		{
			// se c'è almeno un control nella workingArea allora procedo a mettere a posto le cose.
			if (workingAreaConsole.Controls.Count == 0 || workingAreaConsole.Controls[0] == null)
				return base.ShutDownFromPlugIn();

			if (workingAreaConsole.Controls[0] is SetArticles)
			{
				SetArticles setArticols = (SetArticles)workingAreaConsole.Controls[0];

				// prima di chiudere la Console rimetto il flag Updating a false sulla MSD_Companies
				if (setArticols != null)
				    setArticols.SetUpdatingFlagOnCompanies(false/*, companiesReader*/);
			}
		
			//Aggiungere qui tutto ciò che il plugIn deve fare prima che la console venga chiusa
			return base.ShutDownFromPlugIn();
		}
		# endregion

		#region ToolBar Event - OnSaveItem
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnSaveItem")]
		public void OnAfterClickSaveButton(object sender, System.EventArgs e)
		{
			if (workingAreaConsole.Controls.Count == 0 || workingAreaConsole.Controls[0] == null)
				return;

			Type controlsType = workingAreaConsole.Controls[0].GetType();
				
			if (string.Compare(controlsType.Name, ConstString.setParameters)==0)
			{
				SetParameters  aSetParameters = null;
				aSetParameters = (SetParameters)workingAreaConsole.Controls[0];
				aSetParameters.SaveCustomSettings();
				return;
			}

			if (string.Compare(controlsType.Name, ConstString.setArticols)==0)
			{
				SetArticles setArticols = null;
				setArticols = (SetArticles)workingAreaConsole.Controls[0];
				setArticols.SaveArticles();

				// Dopo il salvataggio delle associazioni articoli/utente devo
				// richiamare il metodo ReloadUserArticleBindings del LoginManager
				try
				{
					loginManager.ReloadUserArticleBindings(InstallationData.ServerConnectionInfo.SysDBConnectionString);
				}
				catch(WebException webExc)
				{
					if (webExc.Response != null ) 
					{
						HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
						Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
						webResponse.Close();
					}
					else
						Debug.Fail(webExc.Status.ToString());
				
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description,	webExc.Message);
					extendedInfo.Add(Strings.Description,	webExc.Response);
					extendedInfo.Add(Strings.Function,		"ReloadUserArticleBindings");
					extendedInfo.Add(Strings.DefinedInto, NameSolverStrings.LoginManager);
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReloadUserArticleBindings, extendedInfo);
				}
				catch(SoapException soapExc)
				{
					Debug.Fail(soapExc.Message);
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description,	soapExc.Message);
					extendedInfo.Add(Strings.Function,		"ReloadUserArticleBindings");
					extendedInfo.Add(Strings.DefinedInto, NameSolverStrings.LoginManager);
					diagnostic.Set(DiagnosticType.Error, Strings.CannotReloadUserArticleBindings, extendedInfo);
				}
				catch(System.Exception exc)
				{
					diagnostic.Set(DiagnosticType.Error, exc.Message);
				}
			}
		}
		# endregion

		#region OnGetPlugInMessages - Risponde all'evento lanciato dalla Console
		/// <summary>
		/// OnAfterClickMessagesButtons
		/// Risponde all'evento lanciato dalla console OnGetPlugInMessages
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnGetPlugInMessages")]
		public void OnAfterClickMessagesButton(object sender, Diagnostic diagnosticFromConsole)
		{
			if (diagnostic.Warning || diagnostic.Error || diagnostic.Information)
				diagnosticFromConsole.Set(diagnostic);
		}
		#endregion

		# region OnInitPathFinder
		/// <summary>
		/// OnInitPathFinder
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnInitPathFinder")]
		public void OnAfterInitPathFinder(PathFinder pathFinder)
		{
			this.pathFinder = pathFinder;

			lockManager = new LockManager(pathFinder.LockManagerUrl);
			tbService = new TbServices(pathFinder.TbServicesUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut);
			eaSync = new EasyAttachmentSync(pathFinder.EasyAttachmentSyncUrl);
            tbsender = new TbSenderWrapper(pathFinder.TbSenderUrl);
			this.tbHermes = new TbHermesWrapper();
			dataSynch = new DataSynchronizer(pathFinder.DataSynchronizerUrl);
		}
		# endregion

		# region OnInitLoginManager
		/// <summary>
		/// OnInitLoginManager
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnInitLoginManager")]
		public void OnAfterInitLoginManager(LoginManager loginManager)
		{
			this.loginManager = loginManager;
		}
		# endregion

		#endregion

		#region Creation Tree and Menù Functions

		#region UpdateConsoleTree
		/// <summary>
		/// UpdateConsoleTree
		/// Esegue l'aggiunta dei nodi del Settings al Tree creato dalla MicroareaConsole
		/// rootPlugInNode
		/// webServicesStateNode(Nadia)
		/// </summary>
		//---------------------------------------------------------------------
		public void UpdateConsoleTree(TreeView treeConsole, ConnectionParameters connectionParameters)
		{
			ContextMenu contextMenu = null;
			//ROOT DI TUTTO
            PlugInTreeNode lastNodeTree = (PlugInTreeNode)treeConsole.Nodes[treeConsole.Nodes.Count - 1];

			//ROOT (Settings Admin)
			PlugInTreeNode rootPlugInNode = CreateNode
				(
				Strings.SystemInformations,
                PlugInTreeNode.GetConfigSettingsDefaultImageIndex, 
				ConstString.settingsAdminlbl, 
				contextMenu
				);
			rootPlugInNode.ToolTipText  = Strings.ToolTipString;

			//Contenitore delle application dei parametri
			PlugInTreeNode parametersNode = CreateNode
				(
				Strings.SystemParameters,
                PlugInTreeNode.GetConfigSettingsDefaultImageIndex, 
				ConstString.applicationsContainer, 
				null
				);
			rootPlugInNode.Nodes.Add(parametersNode);
			
			LoadApplicationSettingsFile(parametersNode);
			//Nodo per la visualizzazione dello stato dei WebServices
			PlugInTreeNode webServicesStateNode = CreateNode
				(
				Strings.WebServices,
                PlugInTreeNode.GetConfigSettingsDefaultImageIndex,
				ConstString.webServicesContainer,
				null
				);
			webServicesStateNode.ToolTipText  = Strings.WebServices;
			rootPlugInNode.Nodes.Add(webServicesStateNode);

			//Aggiungo il ramo delle licenze solo se l'edizione non è Enterprise
            //if (string.Compare(this.licenceInfo.Edition, NameSolverStrings.EnterpriseEdition, true, CultureInfo.InvariantCulture) != 0)
            //{31/05/2013 ORA ANCHE IN ENT PER ASSEGNAZIONE CAL FLOATING
				PlugInTreeNode articlePlugInNode = CreateNode
					(
					Strings.ArticleUsers,
					indexIconArticols,
					ConstString.articleNode,
					null
					);

				articlePlugInNode.ToolTipText = Strings.ArticlesLicensesToolTip;
				rootPlugInNode.Nodes.Add(articlePlugInNode);
            //}

			// Nodo per aprire il LogViewer
			PlugInTreeNode logViewer = CreateNode
				(
				Strings.LogViewer,
                PlugInTreeNode.GetConfigSettingsDefaultImageIndex,
				ConstString.logContainer,
				null
				);
			logViewer.ToolTipText = Strings.ToolTipLogViewer;
			rootPlugInNode.Nodes.Add(logViewer);

			lastNodeTree.Nodes.Add(rootPlugInNode);

			if (OnAddRootPlugInTreeNode != null)
				OnAddRootPlugInTreeNode(rootPlugInNode);
		}
		# endregion

		# region LoadApplicationSettingsFile
		/// <summary>
		/// LoadApplicationSettingsFile
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadApplicationSettingsFile(PlugInTreeNode groupPlugInNode)
		{
			if (pathFinder == null)
				return;
			
			if (pathFinder.ApplicationInfos != null)
			{
				foreach ( ApplicationInfo apInfo in pathFinder.ApplicationInfos)
				{
					if (apInfo.Modules== null || 
						(apInfo.ApplicationType != ApplicationType.TaskBuilder ) || 
						!apInfo.ApplicationConfigInfo.Visible)
						continue;
					
					AddApplication(apInfo, groupPlugInNode);
				}
			}
		}
		# endregion

		# region AddApplication + AddSection
		/// <summary>
		/// AddApplication
		/// </summary>
		//---------------------------------------------------------------------
		private void AddApplication(ApplicationInfo apInfo, PlugInTreeNode groupPlugInNode)
		{
			// Prima di inserire il nodo relativo all'application controllo se ha almeno un file .config
			SystemParametersTreeNode  newApplicationNode = new SystemParametersTreeNode();
			if (!ExistConfigFile(apInfo)) 
				return;

			//Applicazione (Nome dell'applicazione)
			newApplicationNode.SelectedImageIndex	= indexIcon;
			newApplicationNode.ImageIndex			= indexIcon;
			newApplicationNode.Text					= apInfo.Name;
			newApplicationNode.AssemblyName			= Assembly.GetExecutingAssembly().GetName().Name;
			newApplicationNode.AssemblyType			= typeof(ServicesAdmin);
			newApplicationNode.Type					= ConstString.application;
			newApplicationNode.ApplicationName		= apInfo.Name;
			
			if (File.Exists(pathFinder.GetShsFilePath()))
				newApplicationNode.ContextMenu     = ContextMenuSessionContainer();
							
			groupPlugInNode.Nodes.Add(newApplicationNode);

			//Aggiungo il Ramo fittizio che mi serve per far apparire il +
			//così poi vado ad intercettare l'evento Expand sul tree
			SystemParametersTreeNode fitNode = new SystemParametersTreeNode();
			fitNode.SelectedImageIndex = indexIconSessionContainer;
			fitNode.ImageIndex         = indexIconSessionContainer;
			fitNode.Text				= "";
			fitNode.AssemblyName		= Assembly.GetExecutingAssembly().GetName().Name;
			fitNode.AssemblyType		= typeof(ServicesAdmin);
			fitNode.Type                = "fittizio";
			newApplicationNode.Nodes.Add(fitNode);		
		}

		/// <summary>
		/// AddSection
		/// </summary>
		//---------------------------------------------------------------------
		private void AddSection
			(
			SystemParametersTreeNode plugInTreeNode, 
			SectionInfo aSections, 
			ModuleInfo	moduleInfo, 
			string		fileName, 
			int			index
			)
		{
			SystemParametersTreeNode currentNode = new SystemParametersTreeNode();

			currentNode.Text = (aSections.Localize.Length == 0) ? aSections.Name : aSections.Localize;
			currentNode.ImageIndex			= indexIconSettings;
			currentNode.SelectedImageIndex	= indexIconSettings;
			currentNode.AssemblyName		= Assembly.GetExecutingAssembly().GetName().Name;
			currentNode.AssemblyType		= typeof(ServicesAdmin);
			currentNode.Type				= ConstString.session;
			currentNode.ApplicationName		= plugInTreeNode.ApplicationName;
			currentNode.Module				= moduleInfo;
			currentNode.SectionInfoNode		= aSections;
			currentNode.FileName			= fileName;

			plugInTreeNode.Nodes.Insert(index, currentNode);
		}
		# endregion

		# region CreateNode
		/// <summary>
		/// CreateNode
		/// </summary>
		//---------------------------------------------------------------------
		private PlugInTreeNode CreateNode
			(
			string	nodeCaption, 
			int		imageIndex, 
			string	nodeType, 
			ContextMenu context
			)
		{
			PlugInTreeNode rootPlugInNode = new PlugInTreeNode(nodeCaption);
			//Aggancio l'eventuale contexMenù
            if (context != null)
				rootPlugInNode.ContextMenu  = context;

			rootPlugInNode.ImageIndex			= imageIndex;
			rootPlugInNode.SelectedImageIndex	= imageIndex;
			rootPlugInNode.Type					= nodeType;
			rootPlugInNode.AssemblyName			= Assembly.GetExecutingAssembly().GetName().Name;
			rootPlugInNode.AssemblyType			= typeof(ServicesAdmin);
			return rootPlugInNode;
		}
		# endregion

		# region ExistConfigFile
		/// <summary>
		/// ExistConfigFile
		/// </summary>
		//---------------------------------------------------------------------		
		private bool ExistConfigFile(ApplicationInfo apInfo)
		{
			string [] settingsFiles;
			string ext = string.Empty;

			foreach (ModuleInfo aModInfo in apInfo.Modules)
			{
				string path = aModInfo.GetStandardSettingsPath();
				if (!Directory.Exists(path)) 
					continue;
				settingsFiles = aModInfo.GetSettingsFiles(path);
				if (settingsFiles != null && settingsFiles.Length >0 )
					return true;
			}
			return false;
		}
		# endregion

		# region LoadSections
		/// <summary>
		/// LoadSections
		/// </summary>
		//----------------------------------------------------------------------
		private void LoadSections(SystemParametersTreeNode selectedSettingNode, bool allSession)
		{
			ArrayList settingsFiles;
			
			if (settingsConfigInfoArray == null)
				settingsConfigInfoArray = new ArrayList();
			
			for(int i = 0; i < settingsConfigInfoArray.Count; i++)
			{
				SettingsConfigInfo setting = (SettingsConfigInfo)settingsConfigInfoArray[i];
				if (string.Compare(setting.ParentModuleInfo.ParentApplicationInfo.Name, selectedSettingNode.ApplicationName) ==0)
				{
					settingsConfigInfoArray.Remove(setting);
					i = i + 1;
				}
			}

			SettingsConfigInfo aSettingsConfigInfo = null;

			ApplicationInfo applicationInfo = (ApplicationInfo)pathFinder.GetApplicationInfoByName(selectedSettingNode.ApplicationName);

			foreach (ModuleInfo aModInfo in applicationInfo.Modules)
			{
				settingsFiles = aModInfo.GetConfigFileArray();
				
				foreach (string settingFile in settingsFiles)
				{
					aSettingsConfigInfo = new SettingsConfigInfo(settingFile, aModInfo);
					aSettingsConfigInfo.Parse(); // lo parso
					if (aSettingsConfigInfo.Sections == null) continue;

					int i = 0;
					foreach(SectionInfo aSections in aSettingsConfigInfo.Sections)
					{
						//Controllo che non sia una session Idden = true
						if ((aSections.Hidden  && !allSession ))continue;
						//	fileName = settingFile.Substring(settingFile.LastIndexOf("\\") +1);
						AddSection(selectedSettingNode, aSections, aModInfo, settingFile, i);
						i = i+1;
					}

					if (aSettingsConfigInfo.Sections != null && aSettingsConfigInfo.Sections.Count > 0)
						settingsConfigInfoArray.Add(aSettingsConfigInfo);
				}
			}
		}
		#endregion

		#region ContextMenu del CONTENITORE SESSIONI
		//---------------------------------------------------------------------
		private ContextMenu ContextMenuSessionContainer()
		{
			applicationServecontextMenu = new ContextMenu();
			applicationServecontextMenu.MenuItems.Add(Strings.AllSession, new System.EventHandler(OnClickAllSection));
			System.Windows.Forms.MenuItem viewObjsMenuItem = applicationServecontextMenu.MenuItems[0];
			applicationServecontextMenu.Popup += new System.EventHandler(OnContextMenuPopup);
			return applicationServecontextMenu;
		}

		//---------------------------------------------------------------------
		private void OnContextMenuPopup(object sender, System.EventArgs e)
		{
			applicationServecontextMenu.MenuItems[0].Text = 
				(!allSectionConfiguration) ? Strings.AllSession : Strings.PublicSection;
		}
		#endregion

		#endregion

		#region FindNodeTipology
		/// <summary>
		/// FindNodeTipology
		/// </summary>
		//----------------------------------------------------------------------
		private PlugInTreeNode FindNodeTipology(TreeNodeCollection nodes, string plugInName, string tipology)
		{
			PlugInTreeNode nodeOfTypology = null;						
			for (int i = 0; nodeOfTypology == null; i++)
			{
				if (i >= nodes.Count)
					return nodeOfTypology;

                PlugInTreeNode aNode = nodes[i] as PlugInTreeNode;
                if (aNode == null)
                    continue;
				
                if (aNode.AssemblyName == plugInName)
				{
                    nodeOfTypology = aNode;
                    if (aNode.Type == tipology)
						return nodeOfTypology;
					else
					{
                        if (aNode.Parent.NextNode.Nodes.Count > 0)
						{
                            PlugInTreeNode nextNode = aNode.Parent.NextNode;
							nodeOfTypology    = FindNodeTipology(nextNode.Nodes, plugInName, tipology);
						}
					}
				}
				else
				{
                    if (aNode.Nodes.Count > 0)
                        nodeOfTypology = FindNodeTipology(aNode.Nodes, plugInName, tipology);	
				}
			}

			return nodeOfTypology;
		}
		#endregion

		#region Initial Function (Load)
 		/// <summary>
		/// Load
		/// Funzione di caricamento del PlugIns (Load) richiamata dalla MicroareaConsole
		/// </summary>
		//---------------------------------------------------------------------
		public override void Load
			(
				ConsoleGUIObjects		consoleGUIObjects,
				ConsoleEnvironmentInfo	consoleEnvironmentInfo,
				LicenceInfo				licenceInfo
			)
		{
			consoleMenu							= consoleGUIObjects.MenuConsole; 
			consoleTree							= consoleGUIObjects.TreeConsole; 
			workingAreaConsole					= consoleGUIObjects.WkgAreaConsole; 
			workingAreaConsoleBottom			= consoleGUIObjects.BottomWkgAreaConsole; 
			connectionParameters				= new ConnectionParameters();
			isRunningFormServer					= consoleEnvironmentInfo.RunningFromServer; 
			workingAreaConsoleBottom.Visible	= false;
			this.consoleEnvironmentInfo         = consoleEnvironmentInfo;
			this.licenceInfo                    = licenceInfo;

			//Aggiungo l'icona Service.bmp
			Assembly myAss			= Assembly.GetExecutingAssembly();
			Stream myStream			= myAss.GetManifestResourceStream(myAss.GetName().Name + ".img."  + "Service.bmp");
			StreamReader myreader	= new StreamReader(myStream);
			consoleTree.ImageList.Images.Add(System.Drawing.Image.FromStream(myStream,true));
						
			//Icona Applicazioni
			Assembly assemblyPlugIns = typeof(PlugIn).Assembly;
			myStream	= assemblyPlugIns.GetManifestResourceStream(ConstString.NamespacePlugInsImg + ".Application.bmp");
			myreader	= new StreamReader(myStream);
            indexIcon = consoleTree.ImageList.Images.Add(Image.FromStream(myStream, true), Color.Magenta);
			
			//ICONS SETTING
			myStream            = myAss.GetManifestResourceStream(myAss.GetName().Name  + ".img." + "SETTINGS.BMP");
			myreader			= new StreamReader(myStream);
            indexIconSettings = consoleTree.ImageList.Images.Add(Image.FromStream(myStream, true), Color.Magenta);
			
			//ICONS ARTICOLS
			myStream            = myAss.GetManifestResourceStream(myAss.GetName().Name  + ".img." + "ARTICOLS.bmp");
			myreader			= new StreamReader(myStream);
            indexIconArticols = consoleTree.ImageList.Images.Add(Image.FromStream(myStream, true), Color.Magenta);
	
			myStream	= myAss.GetManifestResourceStream(myAss.GetName().Name  + ".img." + "SESSIONCONTAINER.BMP");
			myreader	= new StreamReader(myStream);
            indexIconSessionContainer = consoleTree.ImageList.Images.Add(Image.FromStream(myStream, true), Color.Magenta);
		}
		#endregion 
	
		#region Selezione Ramo della Console (OnAfterSelectConsoleTree)
		/// <summary>
		/// OnAfterSelectConsoleTree
		/// </summary>
		//---------------------------------------------------------------------
		public void OnAfterSelectConsoleTree(object sender, TreeViewEventArgs e)
		{
			workingAreaConsoleBottom.Enabled  = false;
			workingAreaConsoleBottom.Visible  = false;
			workingAreaConsole.Controls.Clear();

			if (OnDisableOtherObjectsToolBarButton != null)
				OnDisableOtherObjectsToolBarButton(sender, e);
			if (OnDisableSaveToolBarButton != null)
				OnDisableSaveToolBarButton(sender, e);
			if (OnDisableShowSecurityIconsToolBarButton != null)
				OnDisableShowSecurityIconsToolBarButton(sender, e);
            if (OnDisableFindSecurityObjectsToolBarButton != null)
                OnDisableFindSecurityObjectsToolBarButton(sender, e);

			if (OnDisableApplySecurityFilterToolBarButton != null)
				OnDisableApplySecurityFilterToolBarButton(sender, e);
			if (OnDisableQueryToolBarButton != null)
				OnDisableQueryToolBarButton(sender, e);
			if (OnDisableDeleteToolBarButton != null)
				OnDisableDeleteToolBarButton(sender, e);
			if (OnDisableNewToolBarButton != null)
				OnDisableNewToolBarButton(sender, e);
			if (OnDisableOpenToolBarButton != null)
				OnDisableOpenToolBarButton(sender, e);
			if (OnDisableSaveToolBarButton != null)
				OnDisableSaveToolBarButton(sender, e);

            if (OnDisableShowAllGrantsToolBarButtonPushed != null)
                OnDisableShowAllGrantsToolBarButtonPushed(sender, e);

			//Setting
			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;

			switch (selectedNode.Type)
			{
				case ConstString.webServicesContainer : 
					ViewWebServicesState(); 
					break;
				case ConstString.session : 
					SelectSectionNode(); 
					break;
				case ConstString.articleNode : 
					SelectArticleNode(); 
					break;
				case ConstString.logContainer:
					OpenLogViewer();
					break;
				default: 
					if (OnDisableSaveToolBarButton != null)
						 OnDisableSaveToolBarButton(sender, e); 
					break;
			}
		}
		#endregion

		#region Doppio Click sul Tree della Console (OnAfterDoubleClickConsoleTree)
		//---------------------------------------------------------------------
		public void OnAfterDoubleClickConsoleTree(object sender, System.EventArgs e)
		{
			workingAreaConsoleBottom.Enabled  = false;
			workingAreaConsoleBottom.Visible = false;
			workingAreaConsole.Controls.Clear();

			if (OnDisableQueryToolBarButton != null)
				OnDisableQueryToolBarButton(sender, e);

			if (OnDisableDeleteToolBarButton != null)
				OnDisableDeleteToolBarButton(sender, e);
			if (OnDisableNewToolBarButton != null)
				OnDisableNewToolBarButton(sender, e);
			if (OnDisableOpenToolBarButton != null)
				OnDisableOpenToolBarButton(sender, e);
			if (OnDisableSaveToolBarButton != null)
				OnDisableSaveToolBarButton(sender, e);

			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
			switch (selectedNode.Type)
			{
				case ConstString.webServicesContainer :
					ViewWebServicesState(); 
					break;
				case ConstString.session : 
					SelectSectionNode(); 
					break;
				case ConstString.articleNode : 
					SelectArticleNode(); 
					break;
				case ConstString.logContainer:
					OpenLogViewer();
					break;
				default: 
					if (OnDisableSaveToolBarButton != null) 
						OnDisableSaveToolBarButton(sender, e); 
					break;
			}
		}
		#endregion

		#region Apro un Nodo del Tree della Console (OnAfterExpandConsoleTree)
		//---------------------------------------------------------------------
		public void OnAfterExpandConsoleTree(object sender, System.EventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
			if (selectedNode.Type == ConstString.application)
			{	
				selectedNode.Nodes.Clear();
				SystemParametersTreeNode selectedSettingNode = (SystemParametersTreeNode)consoleTree.SelectedNode;
				LoadSections(selectedSettingNode, false);
			}
		}
		#endregion

		# region ViewWebServicesState (visualizza lo stato dei WebServices)
		/// <summary>
		/// Visualizza lo stato dei WebServices
		/// </summary>
		//---------------------------------------------------------------------
		private void ViewWebServicesState()
		{
			//Pulisco la working area
			workingAreaConsole.Controls.Clear();
			//disabilito la working area bottom
			workingAreaConsoleBottom.Enabled		= false;
			workingAreaConsoleBottom.Visible		= false;

			WebServicesDetail webServicesDetail		= new WebServicesDetail(loginManager, lockManager, tbService, eaSync, tbsender, tbHermes, dataSynch, consoleTree.StateImageList);
			webServicesDetail.OnAskAllCompanies     += new WebServicesDetail.AskAllCompanies(CallOnGetCompanies);
			webServicesDetail.OnAskAllLogins        += new WebServicesDetail.AskAllLogins(loginsDetail_OnGetApplicationUsers);
			webServicesDetail.OnAskAllCompanyUsers  += new WebServicesDetail.AskAllCompanyUsers(loginsDetail_OnGetCompanyUsers);
			webServicesDetail.OnAskRecordsTraced    += new WebServicesDetail.AskRecordsTraced(loginsDetail_OnGetRecordsTraced);
			webServicesDetail.OnDeleteRecordsTraced += new WebServicesDetail.DeleteRecordsTraced(loginsDetail_OnDeleteRecordsTraced);
			webServicesDetail.OnSendDiagnostic		+= new WebServicesDetail.SendDiagnostic(ReceiveDiagnostic);
			webServicesDetail.OnDisableProgressBar	+= new WebServicesDetail.DisableProgressBar(DisableProgressBarFromPlugIn);
			webServicesDetail.OnEnableProgressBar	+= new WebServicesDetail.EnableProgressBar(EnableProgressBarFromPlugIn);
			webServicesDetail.OnSetProgressBarStep	+= new WebServicesDetail.SetProgressBarStep(SetProgressBarStepFromPlugIn);
			webServicesDetail.OnSetProgressBarText	+= new WebServicesDetail.SetProgressBarText(SetProgressBarTextFromPlugIn);
			webServicesDetail.OnSetProgressBarValue	+= new WebServicesDetail.SetProgressBarValue(SetProgressBarValueFromPlugIn);
			webServicesDetail.OnGetAuthenticationToken += new WebServicesDetail.GetAuthenticationToken(webServicesDetail_OnGetAuthenticationToken);
			webServicesDetail.SettingListView();

			webServicesDetail.TopLevel = false;
			webServicesDetail.Dock = DockStyle.Fill;
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere
			OnBeforeAddFormFromPlugIn(this, webServicesDetail.Width, webServicesDetail.Height);
			workingAreaConsole.Controls.Add(webServicesDetail);
			webServicesDetail.Show();
		}
		# endregion

		#region ReceiveDiagnostic - Le info delle form vengono aggiunte alla diagnostica del SysAdmin
		/// <summary>
		/// ReceiveDiagnostic
		/// </summary>
		//---------------------------------------------------------------------
		private void ReceiveDiagnostic(object sender, Diagnostic diagnostic)
		{
			if (diagnostic.Error | diagnostic.Warning | diagnostic.Information)
				this.diagnostic.Set(diagnostic);
		}
		#endregion
		
		# region FindSection
		/// <summary>
		/// FindSection
		/// </summary>
		//---------------------------------------------------------------------
		private SectionInfo FindSection(SystemParametersTreeNode selectedSettingNode)
		{
			SectionInfo section = null;
			foreach (SettingsConfigInfo settingsConfigInfo in settingsConfigInfoArray)
			{
				if (string.Compare(selectedSettingNode.ApplicationName, settingsConfigInfo.ParentModuleInfo.ParentApplicationInfo.Name) ==0 &&
					string.Compare(selectedSettingNode.Module.Name,		settingsConfigInfo.ParentModuleInfo.Name) ==0 &&
					string.Compare(selectedSettingNode.FileName,		settingsConfigInfo.FileName) == 0)
				{
					section = settingsConfigInfo.GetSectionInfoByName(selectedSettingNode.SectionInfoNode.Name);
					if (section != null)
						return section;
				}
			}
			return null;
		}
		# endregion

		# region SelectSectionNode
		//---------------------------------------------------------------------
		private void SelectSectionNode()
		{
			SystemParametersTreeNode selectedSettingNode = (SystemParametersTreeNode)consoleTree.SelectedNode;

			SectionInfo section = FindSection(selectedSettingNode);
			if (section == null) 
				return;

			Cursor.Current = Cursors.WaitCursor;
			workingAreaConsole.Controls.Clear();
						
			SetParameters setParameters	= new  SetParameters(selectedSettingNode, section, pathFinder, allSectionConfiguration);
			setParameters.OnDisableDeleteToolBarButton += new System.EventHandler(DisableDeleteButton);
			setParameters.OnRefreshSectionsArray += new SetParameters.RefreshSectionsArrayEventHandler(RefreshSectionsArray);
			setParameters.TopLevel		= false;
			Object sender = new Object();
			//eventualmente adatta il form di console per le dimensioni della form che si vuole aggiungere

			setParameters.Dock = DockStyle.Fill;
			OnBeforeAddFormFromPlugIn(sender, setParameters.ClientSize.Width, setParameters.ClientSize.Height);
			workingAreaConsole.Controls.Add(setParameters);				
			workingAreaConsole.Visible	= true;
			setParameters.Show();

			workingAreaConsoleBottom.Visible = false;
			workingAreaConsoleBottom.Enabled = false;
			Cursor.Current = Cursors.Default;

			//Abilito il pulsante salva della console
			if (OnEnableSaveToolBarButton!=null)
				OnEnableSaveToolBarButton(sender, new System.EventArgs());
		}
		# endregion

		# region SelectArticleNode
		/// <summary>
		/// SelectArticleNode
		/// </summary>
		//---------------------------------------------------------------------
		private void SelectArticleNode()
		{
			Cursor.Current = Cursors.WaitCursor;
            bool ent =  (string.Compare(this.licenceInfo.Edition, NameSolverStrings.EnterpriseEdition, true, CultureInfo.InvariantCulture) == 0);
			SetArticles setArticols = 
				new SetArticles(connectionParameters, pathFinder, sqlConnection, loginManager, ent);
			
			setArticols.OnGetCompanies += new SetArticles.GetCompanies(CallOnGetCompanies);

			if (setArticols.UnVisible()) 
				return;

			setArticols.Dock = DockStyle.Fill;
			setArticols.TopLevel = false;
			setArticols.Show();

			Object sender = new object();
			EventArgs e = new EventArgs();
			if (OnEnableSaveToolBarButton != null)
				OnEnableSaveToolBarButton(sender, e);
			
			workingAreaConsole.Controls.Clear();
			OnBeforeAddFormFromPlugIn(sender, setArticols.Width, setArticols.Height);
			workingAreaConsole.Controls.Add(setArticols);
			workingAreaConsoleBottom.Visible= false;
			workingAreaConsole.Visible		= true;
		}
		# endregion

		# region OnClickAllSection
		/// <summary>
		/// OnClickAllSection
		/// </summary>
		//---------------------------------------------------------------------
		private void OnClickAllSection(object sender, System.EventArgs e)
		{
			MenuItem menuItem = (MenuItem)sender;
			ContextMenu menu = menuItem.Parent.GetContextMenu();
			TreeView  plugInsTreeView = (TreeView)menu.SourceControl;
			SystemParametersTreeNode plugInTreeNode = (SystemParametersTreeNode)plugInsTreeView.SelectedNode;
			
			//Controllo che sia un nodo di tipo Application 
			if (((PlugInTreeNode)this.consoleTree.SelectedNode).Type != ConstString.application) 
				return;
			
			plugInTreeNode.Nodes.Clear();
			if (!allSectionConfiguration)
			{
				LoadSections(plugInTreeNode, true);
				allSectionConfiguration = true;
			}
			else
			{
				LoadSections(plugInTreeNode, false);
				allSectionConfiguration = false;
			}
		}
		# endregion

		# region DisableDeleteButton + EnabledDeleteButton
		//---------------------------------------------------------------------
		private void DisableDeleteButton(object sender, EventArgs e)
		{
			if (OnDisableDeleteToolBarButton != null)
				OnDisableDeleteToolBarButton(sender, e);
		}

		//---------------------------------------------------------------------
		private void EnabledDeleteButton(object sender, EventArgs e)
		{
			if (OnEnableDeleteToolBarButton != null)
				OnEnableDeleteToolBarButton(sender, e);
		}
		# endregion

		#region RefreshSectionsArray
		/// <summary>
		/// RefreshSectionsArray
		/// </summary>
		//---------------------------------------------------------------------
		private void RefreshSectionsArray(object sender, SystemParametersTreeNode node)
		{
			//Mi posiziono sul nodo dell'Applicazione
			PlugInTreeNode parentNode = ((PlugInTreeNode)this.consoleTree.SelectedNode).Parent;
			int positionIndex = this.consoleTree.SelectedNode.Index;
			int totalNodes =  parentNode.Nodes.Count;
			for(int i = 0; i < totalNodes; i++)
			{
				if (string.Compare(node.Module.Name, ((SystemParametersTreeNode)parentNode.Nodes[i]).Module.Name)== 0)
				{
					parentNode.Nodes.Remove(parentNode.Nodes[i]);
					i = i - 1;
					totalNodes = totalNodes -1;
				}
			}
			
			for (int i = 0; i < settingsConfigInfoArray.Count; i++)
			{
				if (string.Compare(((SettingsConfigInfo)settingsConfigInfoArray[i]).ParentModuleInfo.Name, node.Module.Name)==0 )
				{
					settingsConfigInfoArray.Remove(settingsConfigInfoArray[i]);
					i = i - 1;
				}
			}

			bool allSession = false;
			ArrayList fileNameArray =  node.Module.GetConfigFileArray();
			foreach (string fileName in fileNameArray)
			{
				SettingsConfigInfo aSettingsConfigInfo = new SettingsConfigInfo(fileName,  node.Module);
				aSettingsConfigInfo.Parse(); // lo parso
				if (aSettingsConfigInfo.Sections == null) continue;

				foreach(SectionInfo aSections in aSettingsConfigInfo.Sections)
				{
					//Controllo che non sia una session Idden = true
					if (aSections.Hidden  && !allSession  )continue;
					//	fileName = settingFile.Substring(settingFile.LastIndexOf("\\") +1);
					AddSection((SystemParametersTreeNode)parentNode, aSections, node.Module, fileName, positionIndex);
					positionIndex = positionIndex +1;
				}
				settingsConfigInfoArray.Add(aSettingsConfigInfo);
			}
		}
		#endregion

		# region OpenLogViewer
		/// <summary>
		/// OpenLogViewer
		/// </summary>
		//---------------------------------------------------------------------
		private void OpenLogViewer()
		{
			Cursor.Current = Cursors.WaitCursor;

			workingAreaConsole.Controls.Clear();

			LogViewer logViewer = new LogViewer(pathFinder);
			logViewer.Dock = DockStyle.Fill;
			logViewer.TopLevel = false;
			logViewer.Show();

			workingAreaConsole.Controls.Clear();
			OnBeforeAddFormFromPlugIn(this, logViewer.Width, logViewer.Height);
			workingAreaConsole.Controls.Add(logViewer);
			workingAreaConsoleBottom.Visible= false;
			workingAreaConsole.Visible		= true;
		}
		# endregion
	}

	# region SystemParametersTreeNode
	//=========================================================================
	public class SystemParametersTreeNode : PlugInTreeNode
	{
		private string		applicationName	= string.Empty;
		private ModuleInfo	moduleInfo		= null;
		private string		fileName		= string.Empty;
		private SectionInfo sectionInfo		= null;

		//Proprietà
		public string		ApplicationName	{ get { return applicationName; }	set { applicationName = value; }}
		public ModuleInfo	Module			{ get { return moduleInfo; }		set { moduleInfo = value; }}
		public string		FileName		{ get { return fileName; }			set { fileName = value; }}
		public SectionInfo	SectionInfoNode	{ get { return sectionInfo; }		set { sectionInfo = value; }}

		//---------------------------------------------------------------------
		public SystemParametersTreeNode()
		{
		}
	}
	# endregion

	# region PreRenderEventArgs
	//=========================================================================
	public class PreRenderEventArgs
	{
		private string nodeType;
		private ContextMenu newContext;
		
		public string NodeType { get { return this.nodeType; } set { this.nodeType = value; } }
		public ContextMenu NewContext { get { return this.newContext; } set { this.newContext = value; } }
		
		//---------------------------------------------------------------------
		public PreRenderEventArgs()
		{
			this.nodeType = null;
			this.newContext = null;
		}
		
		//---------------------------------------------------------------------
		public PreRenderEventArgs(string nodeType, ContextMenu context)
		{
			this.nodeType = nodeType;
			this.newContext = context;
		}
	}
	# endregion
}