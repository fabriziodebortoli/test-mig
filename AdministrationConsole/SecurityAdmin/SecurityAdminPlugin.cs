using System;
using System.Collections;
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
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Console.Plugin.SecurityAdmin
{
	//=========================================================================
	public class SecurityAdmin : PlugIn
	{
		#region Dichiarazioni Eventi

		#region Eventi dei Bottoni della Security

		/// <summary>
		/// Abilita il bottone degli OtherObject Reference e DataBase
		/// </summary>
		public event System.EventHandler OnEnableOtherObjectsToolBarButton;	
		/// <summary>
		/// Disbilita il bottone degli OtherObject Reference e DataBase
		/// </summary>
		public event System.EventHandler OnDisableOtherObjectsToolBarButton;
		/// <summary>
		/// Abilito il pulsante xhe mostra il Tree con le icone di OSL
		/// </summary>
		public event System.EventHandler OnEnableShowSecurityIconsToolBarButton;
        /// <summary>
        /// Abilito il pulsante x fa il find sugli oggetti di security
        /// </summary>
        public event System.EventHandler OnEnableFindSecurityObjectsToolBarButton;
        /// <summary>
        /// disabilito il pulsante x fa il find sugli oggetti di security
        /// </summary>
        public event System.EventHandler OnDisableFindSecurityObjectsToolBarButton;	

        /// <summary>
		/// Disabilito il pulsante xhe mostra il Tree con le icone di OSL
		/// </summary>
		public event System.EventHandler OnDisableShowSecurityIconsToolBarButton;
		/// <summary>
		/// Abilito il pulsante che filtra il Tree
		/// </summary>
		public event System.EventHandler OnEnableApplySecurityFilterToolBarButton;
		/// <summary>
		/// Disabilito il pulsante che filtra il Tree
		/// </summary>
		public event System.EventHandler OnDisableApplySecurityFilterToolBarButton;
		/// <summary>
		/// Refresh dei ruoli 
		/// </summary>
		public delegate void RefreshRolesFromImportHandler(object sender, int companyId);
		public event RefreshRolesFromImportHandler OnRefreshRolesFromImport;
		/// <summary>
		/// Pulsante chiavette premuto 
		/// </summary>
		public delegate void SetShowSecurityIconsToolBarButtonPushedHandler(object sender, bool push);
		public event SetShowSecurityIconsToolBarButtonPushedHandler OnSetShowSecurityIconsToolBarButtonPushed;

        public delegate bool IsActivated(string application, string functionality);
        public event IsActivated OnIsActivated;

        public event System.EventHandler OnEnabledShowAllGrantsToolBarButtonPushed;
        public event System.EventHandler OnDisableShowAllGrantsToolBarButtonPushed;

		#endregion

		#region Eventi per i pulsanti del SysAdmin

		/// <summary>
		/// Disabilito il pulsante New 
		/// </summary>
		public event System.EventHandler OnDisableNewToolBarButton;
		/// <summary>
		/// Disabilito il pulsante Open
		/// </summary>
		public event System.EventHandler OnDisableOpenToolBarButton;
		/// <summary>
		/// Disabilito il pulsante Delete
		/// </summary>
		public event System.EventHandler OnDisableDeleteToolBarButton;	
		/// <summary>
		/// Abilita il pulsante di Salva
		/// </summary>
		public event System.EventHandler OnEnableSaveToolBarButton;
		/// <summary>
		/// Disabilita il pulsane di Salva
		/// </summary>
		public event System.EventHandler OnDisableSaveToolBarButton;

		#endregion

		#region Eventi per i bottoni dell'Auditing

		/// <summary>
		/// Disabilita il pulsante x le Query
		/// </summary>
		public event System.EventHandler OnDisableQueryToolBarButton;

		#endregion

		#region Eventi Console

		/// <summary>
		/// evento di change sulla StatusBar
		/// </summary>
		public event ChangeStatusBar OnChangeStatusBarHandle; 
		/// <summary>
		/// Evento che mi dice in che stato è la Console
		/// </summary>
		public delegate StatusType GetConsoleStatus();
		public event GetConsoleStatus OnGetConsoleStatus;

		#endregion

		#endregion

		#region Data Member pubblici
		/// <summary>
		/// SqlConnection verrà chiusa con il PlugIn
		/// </summary>
		public SqlConnection sqlConnection = null;
		/// <summary>
		/// Stringa di connessione passata dalla console
		/// </summary>
		public string connectionString = String.Empty;

        private SearchSecurityObjectsForm searchSecurityObjectsForm = null;

		#endregion
		
		#region Data Member privati
		/// <summary>
		/// Menù  della console (File....)
		/// </summary>
        private System.Windows.Forms.MenuStrip consoleMenu = null;
		private Form consoleMainForm = null;
		/// <summary>
		/// Tree della console
		/// </summary>
        private PlugInsTreeView consoleTree = null;
		/// <summary>
		/// workingAreaConsole
		/// </summary>
		private Panel workingAreaConsole = null;
		/// <summary>
		/// workingAreaConsoleBottom
		/// </summary>
		private Panel workingAreaConsoleBottom = null;
		/// <summary>
		/// showObjectsTree presente nella workingareabuttom
		/// </summary>
		private ShowObjectsTree showObjectsTree = null;
		/// <summary>
		/// Id dell'azienda selezionata nel tree della console
		/// </summary>
		private int companyId = -1;
		/// <summary>
		/// Id del Ruolo o dell'Utente selezionato nel tree della console
		/// </summary>
		private int roleOrUserId = -1;
		/// <summary>
		/// Nome dell'oggetto selezionato nel tree della console
		/// </summary>
		private string loginName = String.Empty;
		/// <summary>
		/// Indica se si tratta di un Ruolo o di un Utente
		/// </summary>
		private string loginType = String.Empty;
		/// <summary>
		/// Invcremento della progress
		/// </summary>
		private int progress = 0;
		/// <summary>
		/// Menù di contesto della company
		/// </summary>
		ContextMenu companyNodeContextMenu = null;
		/// <summary>
		/// Menù di contesto dei Ruoli e degli Utenti
		/// </summary>
		ContextMenu roleOrUserNodeContextMenu = null;
		/// <summary>
		/// info di ambiente di console
		/// </summary>
		private ConsoleEnvironmentInfo consoleEnvironmentInfo;

		/// <summary>
		/// PathFInder passato dalla Console
		/// </summary>
		private PathFinder consolePathFinder = null;
		private BrandLoader brandLoader = null;
        private ArrayList companies;
		/// <summary>
		/// per la gestione delle sole 2 aziende per la Standard Edition
		/// </summary>
		private bool isStandardEdition = false;
		private StringCollection companiesIdAdmitted = null;
        private ShowAllGrantsDialog showAllGrantsDialog = null;

		#endregion

		#region Public Properties

		//-----------------------------------------------------------------
		/// <summary>
		/// Get dell'id dell'azienda selezionata nel tree della Console
		/// </summary>
		public int CompanyId { get { return companyId; } }
		//-----------------------------------------------------------------
		/// <summary>
		/// Get dell'id del Ruolo o dell'Utente selezionato nel tree della Console
		/// </summary>
		public int RoleOrUserId { get { return roleOrUserId; } }
		//-----------------------------------------------------------------
		/// <summary>
		/// Get del nome del Ruolo o dell'Utente selezionato nel tree della Console
		/// </summary>
		public string LoginName { get { return loginName; } }
		//-----------------------------------------------------------------
		/// <summary>
		/// Get del booleano che indica se si sta lavorando con un Utente o con
		/// un Ruolo
		/// </summary>
		public bool IsRoleLogin { get { return (String.Compare(loginType, Strings.Role, true, CultureInfo.InvariantCulture) == 0); } }
		//-----------------------------------------------------------------
		/// <summary>
		/// Controlla se dtiamo lavorando con un ShowObjectsTree vaalido per 
		/// le nostre impostazioni
		/// </summary>
		public bool IsValidShowObjectsTreeLoaded
		{ 
			get 
			{ 
				PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
				if 
					(
						selectedNode == null || 
						showObjectsTree == null ||
						String.Compare(selectedNode.AssemblyName, SecurityConstString.SecurityAdminPlugIn) != 0 ||
						(
							String.Compare(selectedNode.Type, Strings.User) != 0 && 
							String.Compare(selectedNode.Type, Strings.Role) != 0
						)
					)
					return false;
			
				return (companyId == Convert.ToInt32(selectedNode.CompanyId) &&
						roleOrUserId == Convert.ToInt32(selectedNode.Id) &&
						String.Compare(loginType, selectedNode.Type) == 0 &&
						String.Compare(loginName, selectedNode.Text) == 0);
			}
		}
			

		#endregion

		#region Costruttori
		/// <summary>
		/// Costruttore vuoto
		/// </summary>
		public SecurityAdmin()
		{
	
		}


        #endregion


        [AssemblyEvent("Microarea.Console.Plugin.ApplicationDBAdmin.ApplicationDBAdmin", "OnAfterUpgradeCompanyDB")]
        public void OnAfterUpgradeCompanyDB(string connnection)
        {
            connectionString = connnection;

            if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
                sqlConnection = new SqlConnection(connnection);

            bool isOFM = OnIsActivated(SecurityConstString.OFMApplicationName, SecurityConstString.OFMCoreModuleName);
            if (!isOFM)
                return;

            IBaseModuleInfo module = this.consolePathFinder.GetModuleInfoByName(SecurityConstString.OFMApplicationName, SecurityConstString.OFMCoreModuleName);

            if (module == null)
                return;

            string fileName = Path.Combine(module.Path, SecurityConstString.XmlFolder, SecurityConstString.DefaultRolesXmlFile);

            ImportExportRole import = new ImportExportRole(fileName, module);
            import.Parse();

            ImportExportFunction.ImportDBObjects(import.Objects, sqlConnection);

            if (sqlConnection == null)
                return;

            if (sqlConnection.State != ConnectionState.Open)
                sqlConnection.Open();

            SqlConnection conn = new SqlConnection(connnection);
            conn.Open();

            // se sono nella Standard Edition allora devo caricare nel tree solo le aziende censite dal SysAdmin (le prime 2)
            string sSelect = "SELECT CompanyId FROM MSD_Companies WHERE disabled = 0";

            SqlCommand mySqlCommand = null;
            SqlDataReader myReader = null;

            try
            {
                ArrayList companies = new ArrayList();

                mySqlCommand = new SqlCommand(sSelect, sqlConnection);
                myReader = mySqlCommand.ExecuteReader();
                while (myReader.Read())
                {
                    companies.Add(myReader["CompanyId"]);
                }

                myReader.Close();

                if (mySqlCommand != null)
                    mySqlCommand.Dispose();

                foreach (int company in companies)
                {
                    foreach (Role role in import.Roles)
                    {
                        int roleid = ImportExportFunction.GetRoleId(role.RoleName, company, sqlConnection);
                        ImportExportFunction.DeleteAllGrantsForRole(company, roleid, connectionString);
                    }
                    //Cancello cadaveri nella protectedObjects
                    ImportExportFunction.DeleteFromProtectedObjects(company, connectionString);
                }



                foreach (int company in companies)
                {
                    foreach (Role role in import.Roles)
                    {
                        ImportExportFunction.ImportRolesFromXML(role, conn, company, connectionString, true);
                    }
                }
            }
            catch (SqlException e)
            {
                Debug.Fail("SqlException raised in SecurityAdminPlugIn.LoadCompanies: " + e.Message);
                if (mySqlCommand != null)
                    mySqlCommand.Dispose();
                if (myReader != null && !myReader.IsClosed)
                    myReader.Close();
            }
        }


        #region Funzioni d'inizializzazione

        #region Load del PlugIn

        //---------------------------------------------------------------------
        /// <summary>
        /// Load ci deve essere sempre perchè la usa la console quando la tira su
        /// può anche essere vuota l'importante è che valorizzi dentro a 3 variabili
        /// locali il menù il Tree e la Working Area in modo da poterle utilizzare
        /// anche in un secondo tempo
        /// </summary>
        public override void Load
			(
				ConsoleGUIObjects consoleGUIObjects,
				ConsoleEnvironmentInfo consoleEnvironmentInfo,
				LicenceInfo licenceInfo
			)
		{
			consoleMenu = consoleGUIObjects.MenuConsole;
			if (consoleMenu != null)
				consoleMainForm = consoleMenu.FindForm();

			consoleTree = consoleGUIObjects.TreeConsole;
			workingAreaConsole = consoleGUIObjects.WkgAreaConsole;
			workingAreaConsoleBottom = consoleGUIObjects.BottomWkgAreaConsole;
			this.consoleEnvironmentInfo = consoleEnvironmentInfo;
			isStandardEdition = (string.Compare(licenceInfo.Edition, NameSolverStrings.StandardEdition, true) == 0);
		}

		#endregion

		#region Inizializzazione PathFinder
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Prende il PatFinder passatogli dalla Console
		/// </summary>
		/// <param name="aPathFinder"></param>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitPathFinder")]
		public void OnInitPathFinder(PathFinder aPathFinder)
		{
			consolePathFinder = aPathFinder;
		}


		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnInitBrandLoader")]
		public void OnInitBrandLoader(BrandLoader aBrandLoader)
		{
			brandLoader = aBrandLoader;
		}

		#endregion


		#endregion

		#region Funzioni Eventi Console

		#region ShutDown Console

		//---------------------------------------------------------------------
		/// <summary>
		/// Evento di chiusura della console
		/// </summary>
		/// <returns></returns>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShutDownConsole")]
		public override bool ShutDownFromPlugIn()
		{
			//Aggiungere qui tutto ciò che il plugIn deve fare 
			//prima che la console venga chiusa
			return base.ShutDownFromPlugIn();
		}
		#endregion

		#region Connections Events

		#region LogOn
		//---------------------------------------------------------------------
		/// <summary>
		/// Evento della console sparato dopo la LogOn
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterLogOn")]
		public void OnAfterLogOn(object sender, DynamicEventsArgs e)
		{

            companies = new ArrayList();
			//utilizzo la classe di info ConsoleEnvironment
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName = e.Get("DbDefaultUser").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd = e.Get("DbDefaultPassword").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth = Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity"));
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = e.Get("DbServer").ToString();
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName = e.Get("DbDataSource").ToString();
			//prendo lo stato di console
			if (OnGetConsoleStatus != null)
				this.consoleEnvironmentInfo.ConsoleStatus = OnGetConsoleStatus();

			connectionString = String.Empty;
			string databaseName = e.Get("DbDataSource").ToString();
			if (databaseName != null && databaseName != String.Empty)
			{
				string serverName = e.Get("DbServer").ToString();
				if (e.Get("DbServerIstance") != null && e.Get("DbServerIstance").ToString() != String.Empty)
					serverName += Path.DirectorySeparatorChar + e.Get("DbServerIstance").ToString();

				if (e.Get("IsWindowsIntegratedSecurity") != null && Convert.ToBoolean(e.Get("IsWindowsIntegratedSecurity")))
					connectionString = String.Format("Data Source={0};Integrated Security=SSPI;Initial Catalog={1};Pooling=false;", 
						serverName, databaseName);
				else
					connectionString = String.Format("Data Source={0};User ID={1};Password={2};Initial Catalog={3};Pooling=false;", 
						serverName, e.Get("DbDefaultUser").ToString(), e.Get("DbDefaultPassword").ToString(), databaseName);
			}
			
			if (connectionString != String.Empty)
			{
				sqlConnection = new SqlConnection(connectionString);
				sqlConnection.Open();
			}

			// se l'Edition è Standard, allora controllo quali aziende ha caricato il SysAdmin nel suo tree
			// ed carico nel tree dell'Auditing solo quelle...
			if (isStandardEdition)
				companiesIdAdmitted = ((StringCollection)(e.Get("CompaniesIdAdmitted")));

            UpdateConsoleTree(consoleTree);

            string fileName = string.Empty;
            ImportExportRole import;
            
            if (OnIsActivated != null)
            {
                IBaseModuleInfo module = this.consolePathFinder.GetModuleInfoByName(SecurityConstString.AdministrationConsole, DatabaseLayerConsts.SecurityAdmin);
                if (module == null)
                    return;

                if (OnIsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner))
                {
                    fileName = Path.Combine(this.consolePathFinder.GetModuleInfoByName(SecurityConstString.AdministrationConsole, DatabaseLayerConsts.SecurityAdmin).Path, "Xml");
                    fileName = Path.Combine(fileName, "easystudioRole.xml");
                    import = new ImportExportRole(fileName, module);
                    import.Parse();

                    for (int i = 0; i < companies.Count; i++)
                    {
                        foreach (Role role in import.Roles)
                        {
                            if (!ExistRoleWithTheSameName(role.RoleName, Convert.ToInt32(companies[i])))
                                ImportExportFunction.ImportRolesFromXML(role, sqlConnection, Convert.ToInt32(companies[i]), connectionString, true);
                        }
                    }
                }

                if (OnIsActivated("Framework", "ReportEditor"))
                {
                    fileName = Path.Combine(this.consolePathFinder.GetModuleInfoByName(SecurityConstString.AdministrationConsole, DatabaseLayerConsts.SecurityAdmin).Path, "Xml");
                    fileName = Path.Combine(fileName, "reportstudiodeveloper.xml");
                    import = new ImportExportRole(fileName, module);
                    import.Parse();

                    for (int i = 0; i < companies.Count; i++)
                    {
                        foreach (Role role in import.Roles)
                        {
                            if (!ExistRoleWithTheSameName(role.RoleName, Convert.ToInt32(companies[i])))
                                ImportExportFunction.ImportRolesFromXML(role, sqlConnection, Convert.ToInt32(companies[i]), connectionString, true);
                        }
                    }

                }
            }
		}

        //---------------------------------------------------------------------
        public bool ExistRoleWithTheSameName(string roleName, int company)
        {
            if (connectionString == null || connectionString == String.Empty)
                return true;

            SqlConnection tmpConnection = new SqlConnection(connectionString);
            tmpConnection.Open();

            SqlCommand mySqlCommand = null;
            try
            {
                String sSelect = @"SELECT COUNT(*) FROM MSD_CompanyRoles WHERE
								CompanyId = " + company + " AND Role = @Role";

                mySqlCommand = new SqlCommand(sSelect, tmpConnection);
                mySqlCommand.Parameters.AddWithValue("@Role", roleName);
                int recordsCount = (int)mySqlCommand.ExecuteScalar();

                mySqlCommand.Dispose();
                tmpConnection.Close();
                return (recordsCount == 1);
            }
            catch (SqlException)
            {
                if (mySqlCommand != null)
                    mySqlCommand.Dispose();
                tmpConnection.Close();
                return false;
            }
        }
		#endregion

		#region LogOff

		//---------------------------------------------------------------------
		/// <summary>
		/// Evento della console sparato dopo la LogOff
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterLogOff")]
		public void OnAfterLogOff(object sender, System.EventArgs e)
		{
			workingAreaConsole.Controls.Clear();
			TreeNodeCollection nodeCollection = consoleTree.Nodes[0].Nodes;

			for (int i = 0; i < nodeCollection.Count; i++)
			{
                //PlugInTreeNode aNode = nodeCollection[i] as PlugInTreeNode;
                //if (aNode == null)
                //	continue;
                if(((PlugInTreeNode)nodeCollection[i]).AssemblyName == SecurityConstString.SecurityAdminPlugIn)
				{
                    nodeCollection[i].Remove();
                    i = i - 1;
                }
			}
			
			workingAreaConsoleBottom.Visible = false;
			DisableShowConfigurationToolbarButtons();
			DestroyShowObjectsTree();

			if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
			{
				sqlConnection.Close();
				sqlConnection.Dispose();
				sqlConnection = null;
			}

			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth = false;
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName = string.Empty;
		}
		#endregion
		
		#endregion

		#region Funzioni che cambiano la label infondo

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che modifica la label infondo alla Consol
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ModifyConsole(object sender, DynamicEventsArgs e)
		{
			if (OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.StartImportDB);
				OnChangeStatusBarHandle(sender, statusBar);
			}
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che riporta al Default la label infondo alla Consol
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DefaultConsole(object sender, DynamicEventsArgs e)
		{
			if (OnChangeStatusBarHandle != null)
			{
				DynamicEventsArgs statusBar = new DynamicEventsArgs();
				statusBar.Add(Strings.StopImportDB);
				OnChangeStatusBarHandle(sender, statusBar);
			}
		}

		#endregion

		#endregion
	
		#region Funzioni x Pulsanti ToolBar

		#region Funzioni Pulsanti TOOLBAR SECURITY

		//---------------------------------------------------------------------
		/// <summary>
		/// Viene richiamata quando clicco sul pulsante altri oggetti della Console 
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnOtherObjects")]
		public void OnAfterClickOtherObjects(object sender, System.EventArgs e)
		{
            if (sender == null || !(sender is System.Windows.Forms.ToolStripButton))
                return;

            ToolStripButton toolBarButton = sender as ToolStripButton;

			if (GetShowObjectsTreeForCurrentlySelectedConsoleTreeNode())
				showObjectsTree.CreateObjectsParser(toolBarButton.Checked);
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Viene richiamata quando clicco sul pulsante Mostra icone della Console 
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShowIconSecurity")]
		public void OnShowTreeIconSecurity(object sender, System.EventArgs e)
		{
            if (sender == null || !(sender is System.Windows.Forms.ToolStripButton))
                return;

            ToolStripButton toolBarButton = sender as ToolStripButton;

			if (GetShowObjectsTreeForCurrentlySelectedConsoleTreeNode())
				showObjectsTree.ShowSecurityIcons = toolBarButton.Checked;
		}

        //---------------------------------------------------------------------
		/// <summary>
		/// Viene richiamata quando clicco sul pulsante filtra menù della Console
		/// </summary>
        [AssemblyEvent("Microarea.Console.MicroareaConsole", "OnSearchSecurityObjects")]
        public void OnSearchSecurityObjects(object sender, System.EventArgs e)
        {
            if (sender == null || !(sender is System.Windows.Forms.ToolStripButton))
                return;

            ToolStripButton toolBarButton = sender as ToolStripButton;

            if (showObjectsTree == null || showObjectsTree.pathFinder == null || showObjectsTree.CurrentParser == null)
                return;

            if (searchSecurityObjectsForm == null)
            {
                searchSecurityObjectsForm = new SearchSecurityObjectsForm(showObjectsTree.pathFinder, showObjectsTree.CurrentParser, this.sqlConnection);
                searchSecurityObjectsForm.SelectFoundSecurityObject += new SearchSecurityObjectsForm.SearchSecurityObjectsEventHandler(searchSecurityObjectsForm_SelectFoundSecurityObject);
                searchSecurityObjectsForm.Closed += new System.EventHandler(this.SearchSecurityObjectsForm_Closed);

                searchSecurityObjectsForm.Show();
            }

            if (searchSecurityObjectsForm.WindowState == FormWindowState.Minimized)
                searchSecurityObjectsForm.WindowState = FormWindowState.Normal;

            searchSecurityObjectsForm.Activate();

        }

        //----------------------------------------------------------------------------
        private void searchSecurityObjectsForm_SelectFoundSecurityObject(object sender, MenuXmlNode node)
        {
            if (showObjectsTree == null || node == null)
                return;

            showObjectsTree.MenuManagerWinControl.SelectCommandNode(node);
        }

        //----------------------------------------------------------------------------
        private void SearchSecurityObjectsForm_Closed(object sender, System.EventArgs e)
        {
            searchSecurityObjectsForm = null;
        }

        //---------------------------------------------------------------------
        [AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShowAllGrants")]
        public void OnShowAllGrants(object sender, System.EventArgs e)
        {
            if (showObjectsTree == null || showObjectsTree.pathFinder == null || showObjectsTree.CurrentParser == null)
                return;

            if (showAllGrantsDialog == null)
            {
                showAllGrantsDialog = new ShowAllGrantsDialog(showObjectsTree, sqlConnection, CompanyId);
                //showAllGrantsDialog.SelectFoundSecurityObject += new SearchSecurityObjectsForm.SearchSecurityObjectsEventHandler(searchSecurityObjectsForm_SelectFoundSecurityObject);
                showAllGrantsDialog.Closed += new System.EventHandler(this.ShowAllGrantsDialog_Closed);
                showAllGrantsDialog.Show();
            }

            if (showAllGrantsDialog.WindowState == FormWindowState.Minimized)
                showAllGrantsDialog.WindowState = FormWindowState.Normal;

            showAllGrantsDialog.Activate();
        }

        //----------------------------------------------------------------------------
        private void ShowAllGrantsDialog_Closed(object sender, System.EventArgs e)
        {
            showAllGrantsDialog = null;
        }

		//---------------------------------------------------------------------
		/// <summary>
		/// Viene richiamata quando clicco sul pulsante filtra menù della Console
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShowFilterSecurity")]
		public void OnShowTreeObjFilterSecurity(object sender, System.EventArgs e)
		{
            if (sender == null || !(sender is System.Windows.Forms.ToolStripButton))
				return;

            ToolStripButton toolBarButton = sender as ToolStripButton;

			if (GetShowObjectsTreeForCurrentlySelectedConsoleTreeNode())
				showObjectsTree.ApplyFilterToObjectsParser(toolBarButton.Checked);
		}

		//---------------------------------------------------------------------

		#endregion

		#region Funzioni Pulsanti ToolBar SysAdmin

		#region Click sul pulsante Salva della Console
	
		//---------------------------------------------------------------------
		/// <summary>
		/// Viene richiamata quando clicco sul pulsante salva della Console 
		/// </summary>
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnSaveItem")]
		public void OnAfterClickSaveButton(object sender, System.EventArgs e)
		{
			Save();
		}

		//---------------------------------------------------------------------
		public void SaveChanges(object sender, System.EventArgs e)
		{
			Save();
		}

		//---------------------------------------------------------------------
		public void Save()
		{
			if (workingAreaConsole.Controls == null || workingAreaConsole.Controls.Count == 0)
				return;

			Type controlsType = workingAreaConsole.Controls[0].GetType();
			
			//Imposta permessi avanzata
			if (controlsType.Name == SecurityConstString.SetGrants && workingAreaConsole.Controls[0] is SetGrants)
			{
				SetGrants aSetGrantsForm = (SetGrants)workingAreaConsole.Controls[0];
				aSetGrantsForm.SaveGrants();
			}
		}

		#endregion

		#endregion

		#endregion

		#region Eventi del SysAdminPlugin 
		
		#region Eventi sulla Company

		#region Cambio del Flag della SecurityAdmin

		//---------------------------------------------------------------------
		private bool NewSecurityCompany(string companyId, bool disabled, bool isSecurity)
		{
			InsertTreeCompany();

			if (disabled || !isSecurity)
				return false;

			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return false;

			if (string.IsNullOrWhiteSpace(companyId))
				return false;

			//Controllo se ci sono oggetti nella tabella e se è vuota li inserisco
			if (IsObjectsTableEmpty())
				ImportObjectInDataBase(Convert.ToInt32(companyId), true);

			return true;
		}

		/// <summary>
		/// Evento che viene sparato quando viene modificato il flag relativo 
		/// alla secrity di un azienda già presente nel DataBase
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterChangedCompanyOSLSecurity")]
		public void OnAfterChangedCompanyOSLSecurityFlag(object sender, string companyId, bool isSecurity)
		{
			if (!NewSecurityCompany(companyId, false, isSecurity))
				return;

			// se l'azienda non deve migrare i permessi non procedo
			if (!SecurityLightManager.IsCompanyWithSecurityLightGrants(sqlConnection, Convert.ToInt32(companyId)))
				return;

			// controllo che le tabelle di SL esistano e contengano degli oggetti
			if (!SecurityLightManager.ExistAccessRights(sqlConnection) ||
				!SecurityLightManager.ExistDeniedAccessesToMigrated(sqlConnection, Convert.ToInt32(companyId))) 
				return;

			if (MessageBox.Show(Strings.SecurityLightMigrationText, 
								Strings.MigrationFromSecurityLight, 
								MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
								MessageBoxDefaultButton.Button1) == DialogResult.No)
				return;

            if (!OnIsActivated(SecurityConstString.OFMApplicationName, SecurityConstString.OFMCoreModuleName))
            {
                SecurityLightMigrationForm slmf = new SecurityLightMigrationForm(connectionString, Convert.ToInt32(companyId), Convert.ToInt32(companyId));
                slmf.ShowDialog(consoleMainForm);
            }
		}

		/// <summary>
		/// Evento che viene sparato quando viene creata una nuova company 
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterSaveNewCompany")]
		public void OnAfterSaveNewCompany(string companyId, bool disabled)
		{
			if (!NewSecurityCompany(companyId, disabled, true))
				return;

			// se l'azienda non deve migrare i permessi non procedo
			if (!SecurityLightManager.IsCompanyWithSecurityLightGrants(sqlConnection, Convert.ToInt32(companyId)))
				return;
			
			// controllo che le tabelle di SL esistano e contengano degli oggetti
			if (!SecurityLightManager.ExistAccessRights(sqlConnection) ||
				!SecurityLightManager.ExistDeniedAccessesToMigrated(sqlConnection, Convert.ToInt32(companyId)))
				return;

			if (MessageBox.Show(Strings.SecurityLightMigrationText, 
				Strings.MigrationFromSecurityLight, 
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
				MessageBoxDefaultButton.Button1) == DialogResult.No)
				return;

            if (!OnIsActivated(SecurityConstString.OFMApplicationName, SecurityConstString.OFMCoreModuleName))
            {
                SecurityLightMigrationForm slmf = new SecurityLightMigrationForm(connectionString, -1, Convert.ToInt32(companyId));
                slmf.ShowDialog(consoleMainForm);
            }
		}

		/// <summary>
		/// Controlla se la tabella è vuota
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsObjectsTableEmpty()
		{
			if (sqlConnection == null)
				return false;
			
			bool connectionToClose = false;
			SqlCommand myCommand = null;
			
			try
			{
				if (sqlConnection.State != ConnectionState.Open)
				{
					sqlConnection.Open();
					connectionToClose = true;
				}

				string sSelect = "SELECT COUNT(*) FROM MSD_Objects";

				myCommand = new SqlCommand(sSelect, sqlConnection);
				
				int result = (int)Convert.ToInt32(myCommand.ExecuteScalar());

				return (result == 0);
			}
			catch (SqlException e)
			{
				Debug.Fail("Exception raised in SecurityAdminPlugIn.IsObjectsTableEmpty: " + e.Message);
				return false;
			}
			finally
			{
				if (myCommand != null)
					myCommand.Dispose();

				if (connectionToClose && sqlConnection.State == ConnectionState.Open)
					sqlConnection.Close();
			}
		}

		#endregion

		#region Salva Nuova o Modifica Evistente
		//---------------------------------------------------------------------
		/// <summary>
		/// Evento sparato quando si salva un'azienda
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterSavedCompany")]
		public void OnAfterConsoleSaveNewCompany(object sender, string e)
		{
			InsertTreeCompany();
		}
		

		#endregion

		#region Cancella
		//---------------------------------------------------------------------
		/// <summary>
		/// Evento sparato quando viene cancellata un'azienda
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnDeleteCompanyFromSysAdmin")]
		public void OnAfterConsoleDeleteCompany(object sender, string e)
		{
            PlugInTreeNode node = FindNodeOfType(consoleTree.Nodes, Strings.Companies);
			bool isExpanded = node.IsExpanded;
			if (node != null)
			{
                PlugInTreeNode selNode;
                int position = -1;
                for (int i = 0; i < node.Nodes.Count; i++)
                {
                    selNode = node.Nodes[i] as PlugInTreeNode;
                    if (selNode != null && e == selNode.Id)
                    {
                        position = i;
                        break;
                    }
                }
                
                if (position != -1)
					node.Nodes.RemoveAt(position);
				if (isExpanded)
					node.Expand();
				this.consoleTree.Focus();
				if (consoleTree.SelectedNode != null)
					OnAfterSelectConsoleTree(sender, 
						new TreeViewEventArgs(consoleTree.SelectedNode));
			}
		}

		#endregion

		#region Clona

		//---------------------------------------------------------------------
		/// <summary>
		/// Evento sparato quando viene clonata un'azienda
		/// </summary>
		/// <param name="companyId"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterClonedCompany")]
		public void OnAfterConsoleClonedCompany(string companyId)
		{
			InsertTreeCompany();
		}

		#endregion

		#region Disabilita 
		//---------------------------------------------------------------------
		/// <summary>
		/// Evento sparato quando viene modificato il flag disabilita di un azienda
		/// già presente nel DataBase
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		/// <param name="disable"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterChangedCompanyDisable")]
		public void OnAfterChangedCompanyDisable(object sender, string companyId, bool disable)
		{
            PlugInTreeNode node = FindNodeOfType(consoleTree.Nodes, Strings.Companies);
			for (int i = 0; i < node.Nodes.Count; i++)
			{
                PlugInTreeNode aNode = node.Nodes[i] as PlugInTreeNode;
                if (aNode == null || aNode.Id != companyId) 
                    continue;
				
				if (disable)
				{
                    aNode.ForeColor = Color.Red;
                    aNode.StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
				}
				else
				{
                    aNode.ForeColor = Color.Black;
                    aNode.StateImageIndex = 0;
				}
				break;
			}

		}
		#endregion

		#endregion

		#region Eventi sui Ruoli 
		
		#region Clona

		//---------------------------------------------------------------------
		/// <summary>
		/// Evento sparato quando viene clonato un Ruolo
		/// </summary>
		/// <param name="aCompanyIdString"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterClonedRole")]
		public void OnAfterConsoleCloneRole(string aCompanyIdString)
		{
			int aCompanyId = Convert.ToInt32(aCompanyIdString);
			if (aCompanyId != -1 && IsCompanySecurity(aCompanyId))
				InsertRoleinTree(aCompanyId);
		}
		
		#endregion

		#region Salva Nuova o Modifica Esistente
		//---------------------------------------------------------------------
		/// <summary>
		/// Evento sparato quando viene inserito un nuovo Ruolo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="aCompanyIdString"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnSaveNewRole")]
		public void OnAfterConsoleSaveNewRole(object sender, string id, string aCompanyIdString)
		{
			int aCompanyId = Convert.ToInt32(aCompanyIdString);
			if (aCompanyId != -1 && IsCompanySecurity(aCompanyId))
				InsertRoleinTree(aCompanyId);
		}
		
		
		#endregion
			
		#region Cancella
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Evento sparato quando viene cancellato un Ruolo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="aCompanyIdString"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnDeleteRole")]
		public void OnAfterConsoleDeleteRole(object sender, string id, string aCompanyIdString)
		{
			int aCompanyId = Convert.ToInt32(aCompanyIdString);
			//Cerco la Company
			if (aCompanyId == -1 || !IsCompanySecurity(aCompanyId))
				return;
			if (workingAreaConsole.Controls.Count > 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
                PlugInTreeNode node = FindNodeOfType(consoleTree.Nodes, Strings.Companies);
                PlugInTreeNode selNode;
				int position = -1;
				int positionRole = -1;

                if (node != null)
                {

					for (int i = 0; i < node.Nodes.Count; i++)
				    {
                        selNode = node.Nodes[i] as PlugInTreeNode;
                        if (selNode != null && aCompanyId == Convert.ToInt32(selNode.Id))
					    {
						    position = i;
						    break;
					    }
				    }

				    //Ho Trovato la company
                    node = FindNodeOfType(((TreeNode)node).Nodes[position].Nodes, Strings.Roles);
					for (int y = 0; y < node.Nodes.Count; y++)
				    {
                        selNode = node.Nodes[y] as PlugInTreeNode;
                        if (selNode != null && id == selNode.Id)
					    {
						    positionRole = y;
						    break;
					    }
				    }
				    bool isExpanded = node.IsExpanded;
					
                    if (positionRole != -1)
						node.Nodes.RemoveAt(positionRole);
					
                    if (isExpanded)
						node.Expand();
					consoleTree.Focus();
					if (consoleTree.SelectedNode != null)
						OnAfterSelectConsoleTree(sender, 
							new TreeViewEventArgs(consoleTree.SelectedNode));
				}
			}
		}
		
		#endregion

		#endregion

		#region Eventi sugli Utenti ASSOCIATI ALLA COMPANY
		
		#region Salva Nuovo o Modifica esistente

		//---------------------------------------------------------------------
		/// <summary>
		/// Evento sparato quando viene salvato 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="id"></param>
		/// <param name="aCompanyIdString"></param>
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnSaveCompanyUser")]
		public void OnAfterConsoleSaveCompanyUser(object sender, string id, string aCompanyIdString)
		{ 
			int aCompanyId = Convert.ToInt32(aCompanyIdString);
			if (aCompanyId != -1 && IsCompanySecurity(aCompanyId))
				InsertUserinTree(aCompanyId);
		}
		
		#endregion

		#region Cancella

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterDeleteCompanyUser")]
		public void OnAfterConsoleDeleteCompanyUser(object sender, string id, string aCompanyIdString)
		{
			int aCompanyId = Convert.ToInt32(aCompanyIdString);
			//Cerco la Company 
			if (aCompanyId == -1 || !IsCompanySecurity(aCompanyId)) return;
			if (workingAreaConsole.Controls.Count > 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
                PlugInTreeNode node = FindNodeOfType(consoleTree.Nodes, Strings.Companies);
                PlugInTreeNode selNode;
				int position = -1;
				int positionRole = -1;
				for (int i = 0; i < node.Nodes.Count; i++)
				{
					selNode = node.Nodes[i] as PlugInTreeNode;
					if (aCompanyId == Convert.ToInt32(selNode.Id))
					{
						position = i;
						break;
					}
				}
				//Ho Trovato la company
				node = FindNodeOfType(((TreeNode)node).Nodes[position].Nodes, Strings.Users);
				for (int y = 0; y < node.Nodes.Count; y++)
				{
					selNode = node.Nodes[y] as PlugInTreeNode;
                    if (selNode != null && id == selNode.Id)
					{
						positionRole = y;
						break;
					}
				}
				bool isExpanded = node.IsExpanded;
				
				if (node != null)
				{
					if (positionRole != -1)
						node.Nodes.RemoveAt(positionRole);
					if (isExpanded)
						node.Expand();
					this.consoleTree.Focus();
					if (this.consoleTree.SelectedNode != null)
						OnAfterSelectConsoleTree(sender, 
							new TreeViewEventArgs(this.consoleTree.SelectedNode));
				}
			}
		}

		#endregion	

		#region Clona

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterClonedUserCompany")]
		public void OnAfterConsoleCloneUser(string aCompanyIdString)
		{
			int aCompanyId = Convert.ToInt32(aCompanyIdString);
			
			if (aCompanyId != -1 && IsCompanySecurity(aCompanyId))
				InsertUserinTree(aCompanyId);
		}
		
		#endregion

		#region Abilito / disabilito Ruolo
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterClickDisabledRole")]
		public void OnAfterClickDisabledRole(object sender, string roleId, bool disabled)
		{
				
			if (workingAreaConsole.Controls.Count > 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
                PlugInTreeNode node = FindNodeOfType(consoleTree.Nodes, Strings.Companies);
                PlugInTreeNode selNode;
                PlugInTreeNode node2;
				for (int i = 0; i < node.Nodes.Count; i++)
				{
                    node2 = FindNodeOfType(((TreeNode)node).Nodes[i].Nodes, Strings.Roles);
					if (node2 == null)
						continue;

					for (int y = 0; y < node2.Nodes.Count; y++)
					{
						selNode = node2.Nodes[y] as PlugInTreeNode;
                        if (selNode != null && roleId == selNode.Id)
						{
							if (disabled)
							{
								node2.Nodes[y].ForeColor = Color.Red;
                                node2.Nodes[y].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
							}
							else
							{
								node2.Nodes[y].ForeColor = Color.Black;
								node2.Nodes[y].StateImageIndex = 0;
							}

						}
					}
				}
			}
		}
		#endregion

		#region Abilito / Disabilito utente
		
		/// <summary>
		/// OnAfterChangedDisabledCompanyLogin
		/// Se l'associazione Utente-Aziende viene modificata (SysAdmin), anche il Security
		/// tiene traccia di questo stato
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="companyId"></param>
		/// <param name="loginId"></param>
		/// <param name="disabled"></param>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterChangedDisabledCompanyLogin")]
		public void OnAfterChangedDisabledCompanyLogin(object sender, string companyId, string loginId, bool disabled)
		{
            PlugInTreeNode node = FindNodeOfType(consoleTree.Nodes, Strings.Companies);
            PlugInTreeNode selNode;
            PlugInTreeNode node2;
			for (int i = 0; i < node.Nodes.Count; i++)
			{
                node2 = FindNodeOfType(((TreeNode)node).Nodes[i].Nodes, Strings.Users);
				for (int y = 0; y < node2.Nodes.Count; y++)
				{
					if (node2.CompanyId != companyId) continue;
					selNode = node2.Nodes[y] as PlugInTreeNode;
                    if (selNode != null && loginId == selNode.Id)
					{
						if (disabled)
						{
							node2.Nodes[y].ForeColor = Color.Red;
                            node2.Nodes[y].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
						}
						else
						{
							node2.Nodes[y].ForeColor = Color.Black;
							node2.Nodes[y].StateImageIndex = 0;
						}

					}
				}
			}
		}
		#endregion

		#endregion

		#region Eventi sulle Login
        			
		#region Abilito / disabilito Login
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterChangedDisabledLogin")]
		public void OnAfterChangeDisabledLoginFlag(object sender, string loginId, bool disabled)
		{
				
			if (workingAreaConsole.Controls.Count > 0 && workingAreaConsole.Controls[0] != null)
			{
				object o = workingAreaConsole.Controls[0];
                PlugInTreeNode node = FindNodeOfType(consoleTree.Nodes, Strings.Companies);
                PlugInTreeNode selNode;
                PlugInTreeNode node2;
				for (int i = 0; i < node.Nodes.Count; i++)
				{
                    node2 = FindNodeOfType(((TreeNode)node).Nodes[i].Nodes, Strings.Users);
					for (int y = 0; y < node2.Nodes.Count; y++)
					{
						selNode = node2.Nodes[y] as PlugInTreeNode;
                        if (selNode != null && loginId == selNode.Id)
						{
							if (disabled)
							{
								node2.Nodes[y].ForeColor = Color.Red;
                                node2.Nodes[y].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
							}
							else
							{
								node2.Nodes[y].ForeColor = Color.Black;
								node2.Nodes[y].StateImageIndex = 0;
							}
						}
					}
				}
			}
		}
	
		#endregion

		#region Cancella Login
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnDeleteUserToPlugIns")]
		public void OnAfterConsoleCompanyUser(object sender, string id)
		{
			//Cerco la Company 
		//	if(!IsCompanyUser(id)) return;
			//Nadia 30/1/04: a che serve o?
			//if (workingAreaConsole.Controls.Count > 0 && workingAreaConsole.Controls[0] != null)
			//{
			//	object o = workingAreaConsole.Controls[0];
            PlugInTreeNode node = FindNodeOfType(consoleTree.Nodes, Strings.Companies);
                PlugInTreeNode selNode;
                PlugInTreeNode node2;
			for (int i = 0; i < node.Nodes.Count; i++)
				{
                    node2 = FindNodeOfType(((TreeNode)node).Nodes[i].Nodes, Strings.Users);
				for (int y = 0; y < node2.Nodes.Count; y++)
					{
						selNode = node2.Nodes[y] as PlugInTreeNode;
                        if (selNode != null && id == selNode.Id)
						{
							node2.Nodes.RemoveAt(y);
						}
					}
				}
			//}
		}
		//---------------------------------------------------------------------
		#endregion

		#endregion

		#region Eventi sull'utente Guest

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterAddGuestUser")]
		public void AfterAddGuestUser(string guestUserName, string guestUserPwd)
		{
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.Exist = true;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = guestUserName;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd = guestUserPwd;
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterDeleteGuestUser")]
		public void AfterDeleteGuestUser(object sender, System.EventArgs e)
		{
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.Exist = false;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = string.Empty;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd = string.Empty;
		}

		#endregion

		#endregion

		#region Eventi sul Tree della Console

		#region Selezione di un ramo del Tree della Console


		//---------------------------------------------------------------------
		private void DestroySetGrants()
		{
			if (workingAreaConsole.Controls == null || workingAreaConsole.Controls.Count == 0)
				return;

			Type controlsType = workingAreaConsole.Controls[0].GetType();
			
			//Imposta permessi avanzata
			if (controlsType.Name == SecurityConstString.SetGrants && workingAreaConsole.Controls[0] is SetGrants)
				workingAreaConsole.Controls[0].Dispose();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che viene eseguita quando seleziono un nodo sel Tree della
		/// Console se è un Utente o un Ruolo  metto i miei bottoni enabled
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnAfterSelectConsoleTree(object sender, TreeViewEventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)e.Node;
			if (selectedNode == null || String.Compare(selectedNode.AssemblyName, SecurityConstString.SecurityAdminPlugIn) != 0)
				return;

			DestroySetGrants();
			workingAreaConsole.Controls.Clear();

			if (selectedNode.Type == Strings.User || selectedNode.Type == Strings.Role)
			{
				if 
					(
					IsValidShowObjectsTreeLoaded && 
					GetShowObjectsTreeForCurrentlySelectedConsoleTreeNode()
					)
				{
					workingAreaConsoleBottom.Visible = true;
					showObjectsTree.SetCurrentCommandNodeSelection();
				}
				else
					workingAreaConsoleBottom.Visible = false;

				EnableShowConfigurationToolbarButtons();
			}
			else
			{
				workingAreaConsoleBottom.Visible = false;
				
				DisableShowConfigurationToolbarButtons();
			}
			
			if (selectedNode.Type == SecurityConstString.OSLXPSecurity)
			{
				CreateViewSecurityPlugInInfo();
			}		
			DisableGenericToolbarButtons();
			if (workingAreaConsole.Controls == null || workingAreaConsole.Controls.Count == 0)
			{
				if (OnDisableSaveToolBarButton != null)
					OnDisableSaveToolBarButton(this, null);
			}
		}
		#endregion

		#region Doppio Click sull Tree della Console

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che viene richiamata quando faccio un doppio click sul Tree
		/// della Consol se è un Ruolo o un Utente faccio apparire il Tree della
		/// Carloz nella workingAreaConsoleBottom
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnAfterDoubleClickConsoleTree(object sender, System.EventArgs e)
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
			if (selectedNode == null || String.Compare(selectedNode.AssemblyName, SecurityConstString.SecurityAdminPlugIn) != 0  || selectedNode.ReadOnly)
				return;

			// se il nodo selezionato è di tipo azienda
			// e la company selezionata non è valida non procedo con le altre operazioni e dò un messaggio
			if (selectedNode.Type == Strings.Company && !selectedNode.IsValid)
			{
				DiagnosticViewer.ShowInformation(Strings.NoValidCompany, Strings.Attention);
				return;
			}

			if (!GetShowObjectsTreeForCurrentlySelectedConsoleTreeNode())
				workingAreaConsoleBottom.Visible = false;

			DisableGenericToolbarButtons();

			if (selectedNode.Type == SecurityConstString.OSLXPSecurity)
			    CreateViewSecurityPlugInInfo();


            if (selectedNode.Type == Strings.User || selectedNode.Type == Strings.Role)
            {
                if (OnEnableFindSecurityObjectsToolBarButton != null)
                    OnEnableFindSecurityObjectsToolBarButton(this, null);

                if (OnEnabledShowAllGrantsToolBarButtonPushed != null)
                    OnEnabledShowAllGrantsToolBarButtonPushed(this, null);
            }
            else
            {
                if (OnDisableFindSecurityObjectsToolBarButton != null)
                    OnDisableFindSecurityObjectsToolBarButton(this, null);

                if (OnDisableShowAllGrantsToolBarButtonPushed != null)
                    OnDisableShowAllGrantsToolBarButtonPushed(this, null);
            }

		}
		//---------------------------------------------------------------------
		#endregion

		#endregion

		#region Creazioni menù di contesto

		#region Menù di contesto sul nodo di tipo  Azienda

		private ContextMenu GetCompanyNodeContextMenu()
		{
			if (companyNodeContextMenu != null)
				return companyNodeContextMenu;

			companyNodeContextMenu = new ContextMenu();

			companyNodeContextMenu.MenuItems.Add(Strings.ImportObjectsFromXML, new System.EventHandler(OnClickImportObjectFromXML));

            if (!OnIsActivated(SecurityConstString.OFMApplicationName, SecurityConstString.OFMCoreModuleName))
                companyNodeContextMenu.MenuItems.Add(Strings.CreateDefaultRoles, new System.EventHandler(OnClickCreateDefaultRoles));

            companyNodeContextMenu.MenuItems.Add(Strings.ExportRolesInXML,     new System.EventHandler(OnClickExportRolesInXML));
			companyNodeContextMenu.MenuItems.Add(Strings.ImportRolesFromXML,   new System.EventHandler(OnClickImportRolesFromXML));

			return companyNodeContextMenu;
		}


		#endregion

		#region Menù di contesto sul nodo di tipo Ruolo / Utente

		private ContextMenu GetRoleOrUserNodeContextMenu()
		{
			if (roleOrUserNodeContextMenu != null)
				return roleOrUserNodeContextMenu;

			roleOrUserNodeContextMenu = new ContextMenu();

			roleOrUserNodeContextMenu.MenuItems.Add(Strings.ViewObjects, new System.EventHandler(OnRoleOrUserViewObjectsMenuItemClicked));

			System.Windows.Forms.MenuItem viewObjsMenuItem = roleOrUserNodeContextMenu.MenuItems[0];
			viewObjsMenuItem.Checked = false;
			
			roleOrUserNodeContextMenu.Popup += new System.EventHandler(OnRoleOrUserNodeContextMenuPopup);
			
			return roleOrUserNodeContextMenu;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OnRoleOrUserNodeContextMenuPopup(object sender, System.EventArgs e)
		{
			if (roleOrUserNodeContextMenu == null || roleOrUserNodeContextMenu.MenuItems == null || roleOrUserNodeContextMenu.MenuItems.Count == 0)
				return;

			System.Windows.Forms.MenuItem viewObjsMenuItem = roleOrUserNodeContextMenu.MenuItems[0];

			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
			if 
				(
				selectedNode == null ||
				String.Compare(selectedNode.AssemblyName, SecurityConstString.SecurityAdminPlugIn) != 0 ||
				(selectedNode.Type != Strings.User && selectedNode.Type != Strings.Role)
				)
			{
				viewObjsMenuItem.Enabled = false;
				return;
			}
			viewObjsMenuItem.Enabled = true;

			viewObjsMenuItem.Checked = IsValidShowObjectsTreeLoaded && workingAreaConsoleBottom.Visible;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OnRoleOrUserViewObjectsMenuItemClicked(object sender, System.EventArgs e)
		{
			if (!((System.Windows.Forms.MenuItem)sender).Checked)
				OnAfterDoubleClickConsoleTree(sender, e);
			else
				workingAreaConsoleBottom.Visible = false;
		}

		#endregion

		#endregion

		#region Eventi delle voci del ContextMenu sulle Aziende della Security

		/// <summary>
		/// Evento di click sulla voce "Esporta ruoli" sul menù di contesto del 
		/// ramo Azienda
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnClickExportRolesInXML(object sender, System.EventArgs e)
		{
			//nodo selezionato nel tree
            PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
			if 
				(
				selectedNode == null ||
				String.Compare(selectedNode.AssemblyName, SecurityConstString.SecurityAdminPlugIn) != 0 ||
				String.Compare(selectedNode.Type, Strings.Company) != 0
				)
				return;

			int currentCompanyId = Convert.ToInt32(selectedNode.Id);
			
			if (workingAreaConsole != null && currentCompanyId != -1)
			{
				workingAreaConsole.Controls.Clear();
				//Devo controllare se esistono dei ruoli attivi 
				bool exist = ExistActiveRoles(currentCompanyId);
				if (!exist)
				{
					DiagnosticViewer.ShowWarning(Strings.NoActivedRoles, Strings.Attention);
					return;
				}
                ImportExportRolesForm exportRolesForm = new ImportExportRolesForm(currentCompanyId, ImportExportRolesFormState.Export, sqlConnection, connectionString);
                exportRolesForm.TopLevel = false;
				
				exportRolesForm.Size = new Size(workingAreaConsole.Width, workingAreaConsole.Height);
				
				exportRolesForm.Dock = DockStyle.Fill;
				
				exportRolesForm.OnAfterExportRole += new ImportExportRolesForm.AfterExportRoleEventHandler(OnAfterExportRole);
				
				workingAreaConsole.Controls.Add(exportRolesForm);
				exportRolesForm.Visible = true;

				workingAreaConsole.Visible = true;
				
				workingAreaConsole.BringToFront();
			}
		}
		//---------------------------------------------------------------------
		private bool ExistActiveRoles(int companyId)
		{
			string sSelect = "SELECT COUNT (*) FROM MSD_CompanyRoles WHERE CompanyId =" + companyId.ToString() + " AND Disabled = 0";
			SqlCommand sqlCommand = null;

			try
			{
				sqlCommand = new SqlCommand(sSelect, sqlConnection);
				int numRow = (int)sqlCommand.ExecuteScalar();
				sqlCommand.Dispose();
				if (numRow > 0) 
					return true;
				else
					return false;
			}
			catch (SqlException)
			{
				if (sqlCommand != null)
					sqlCommand.Dispose();

				return false;
			}
		}


       

        //---------------------------------------------------------------------
        /// <summary>
        /// Evento di click sulla voce "Importa ruoli" sul menù di contesto del 
        /// ramo Azienda
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //---------------------------------------------------------------------
        private void OnClickImportRolesFromXML(object sender, System.EventArgs e)
		{
            PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
			if 
				(
				selectedNode == null ||
				String.Compare(selectedNode.AssemblyName, SecurityConstString.SecurityAdminPlugIn) != 0 ||
				String.Compare(selectedNode.Type, Strings.Company) != 0
				)
				return;
			
			int currentCompanyId = Convert.ToInt32(selectedNode.Id);
			
			if (workingAreaConsole != null && currentCompanyId != -1)
			{
				workingAreaConsole.Controls.Clear();

                ImportExportRolesForm importRolesForm = new ImportExportRolesForm(currentCompanyId, ImportExportRolesFormState.Import, sqlConnection, connectionString, this.consolePathFinder);
                importRolesForm.TopLevel = false;		
				importRolesForm.Size = new Size(workingAreaConsole.Width, workingAreaConsole.Height);
				importRolesForm.Dock = DockStyle.Fill;
				
				importRolesForm.OnAfterImportRole += new ImportExportRolesForm.AfterImportRoleEventHandler(OnAfterImportRole);
                importRolesForm.OnIsActivatedApp += ImportRolesForm_OnIsActivatedApp;

                workingAreaConsole.Controls.Add(importRolesForm);
				
				importRolesForm.Visible = true;

				workingAreaConsole.Visible = true;
				workingAreaConsole.BringToFront();
			}
		}

        private bool ImportRolesForm_OnIsActivatedApp(string application, string functionality)
        {
            return OnIsActivated(SecurityConstString.OFMApplicationName, SecurityConstString.OFMCoreModuleName);
        }




        //---------------------------------------------------------------------
        private void OnClickCreateDefaultRoles(object sender, System.EventArgs e)
        {
            PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;

            if(selectedNode == null || String.Compare(selectedNode.AssemblyName, SecurityConstString.SecurityAdminPlugIn) != 0 || String.Compare(selectedNode.Type, Strings.Company) != 0 )
                return;

            int currentCompanyId = Convert.ToInt32(selectedNode.Id);

            if (workingAreaConsole != null && currentCompanyId != -1)
            {
                workingAreaConsole.Controls.Clear();
                ImportExportRolesForm importRolesForm = new ImportExportRolesForm(currentCompanyId, ImportExportRolesFormState.Create, sqlConnection, connectionString, this.consolePathFinder);
                importRolesForm.TopLevel = false;
                importRolesForm.Size = new Size(workingAreaConsole.Width, workingAreaConsole.Height);
                importRolesForm.Dock = DockStyle.Fill;

                importRolesForm.OnAfterImportRole += new ImportExportRolesForm.AfterImportRoleEventHandler(OnAfterImportRole);

                workingAreaConsole.Controls.Add(importRolesForm);

                importRolesForm.Visible = true;

                workingAreaConsole.Visible = true;
                workingAreaConsole.BringToFront();
            }
        }

		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che viene richiamata quando clicco sulla voce del menù 
		/// di contesto "Memorizza Oggetti"
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnClickImportObjectFromXML(object sender, System.EventArgs e)
		{
            PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
			
			if 
				(
				selectedNode == null ||
				String.Compare(selectedNode.AssemblyName, SecurityConstString.SecurityAdminPlugIn) != 0 ||
				String.Compare(selectedNode.Type, Strings.Company) != 0
				)
				return;
			
			int currentCompanyId = Convert.ToInt32(selectedNode.Id);

            if (workingAreaConsole != null && currentCompanyId != -1)
			{
				ArrayList newObjectsArrayList = ImportObjectInDataBase(currentCompanyId, false);

				if (newObjectsArrayList != null)
				{
					NewObjectsForm newObjectsForm = new NewObjectsForm(newObjectsArrayList, currentCompanyId, sqlConnection, consolePathFinder);
					newObjectsForm.ShowDialog();
				}
				else
					MessageBox.Show(Strings.NoNewObjects, Strings.ImportObjects, MessageBoxButtons.OK, MessageBoxIcon.Information);

			}

		}

		//---------------------------------------------------------------------
		private ArrayList ImportObjectInDataBase(int companyId, bool firstInsert)
		{
			ArrayList newObjects = null;

			PathFinder pathFinder = new PathFinder(CommonObjectTreeFunction.GetCompanyName(companyId, sqlConnection), NameSolverStrings.AllUsers);

			SecurityMenuLoader aSecurityMenuLoader = new SecurityMenuLoader(pathFinder);
				
			aSecurityMenuLoader.ScanStandardMenuComponentsStarted += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuStarted);
			aSecurityMenuLoader.ScanStandardMenuComponentsEnded += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuEnded);
																												
			aSecurityMenuLoader.LoadAllMenuFilesStarted += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuStarted);
			aSecurityMenuLoader.LoadAllMenuFilesModuleIndexChanged += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuIndexChanged);
			aSecurityMenuLoader.LoadAllMenuFilesEnded += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuEnded);
				
			aSecurityMenuLoader.LoadMenuOtherObjectsStarted += new MenuParserEventHandler(SecurityMenuLoader_ProcessingStarted);
			aSecurityMenuLoader.LoadMenuOtherObjectsModuleIndexChanged += new MenuParserEventHandler(SecurityMenuLoader_ProcessingIndexChanged);
			aSecurityMenuLoader.LoadMenuOtherObjectsEnded += new MenuParserEventHandler(SecurityMenuLoader_ProcessingEnded);

			aSecurityMenuLoader.WriteObjectsToDBStarted += new MenuParserEventHandler(SecurityMenuLoader_WriteToDBStarted);
			aSecurityMenuLoader.WriteObjectsToDBGroupIndexChanged += new MenuParserEventHandler(SecurityMenuLoader_WriteToDBIndexChanged);
			aSecurityMenuLoader.WriteObjectsToDBEnded += new MenuParserEventHandler(SecurityMenuLoader_WriteToDBEnded);

			IMessageFilter aFilter = null;
			try
			{
				aFilter = SecurityAdmin.DisableUserInteraction();
				newObjects = aSecurityMenuLoader.ImportObjXml(sqlConnection, firstInsert);

                //CREO RUOLI DI DEFAULT
                if (OnIsActivated != null)
                {
                    if (OnIsActivated(NameSolverStrings.Extensions, NameSolverStrings.EasyStudioDesigner))
                    {
                        IBaseModuleInfo module = this.consolePathFinder.GetModuleInfoByName(SecurityConstString.AdministrationConsole, DatabaseLayerConsts.SecurityAdmin);
                        if (module == null)
                            return newObjects;
                        string fileName = Path.Combine(this.consolePathFinder.GetModuleInfoByName(SecurityConstString.AdministrationConsole, DatabaseLayerConsts.SecurityAdmin).Path, "Xml");
                        fileName = Path.Combine(fileName, "easystudioRole.xml");
                        ImportExportRole import = new ImportExportRole(fileName, module);
                        import.Parse();

                        foreach (Role role in import.Roles)
                            ImportExportFunction.ImportRolesFromXML(role, sqlConnection, companyId, connectionString, true);
                    }
                }
				SecurityAdmin.RestoreUserInteraction(aFilter);
			}
			catch (Exception exception)
			{
				// ATTENZIONE !!!
				// Qui occorre necessariamente catturare possibili eccezioni, predisporre
				// cioè un blocco try-catch- finally, altrimenti continuerebbe a restare
				// disabilitata qualunque interazione da parte del'utente...
				// Infatti, se viene sollevata un'eccezione che non viene gestita, la 
				// console visualizza una dialog box di avvertimento. Nel sottostante 
				// blocco di finally viene ripristinato l'uso di mouse e tastiera e l'utente
				// può interagire con tale dialog box. Se così non fosse, l'applicazione 
				// resterebbe "bloccata". 
				throw exception;
			}
			finally
			{
				SecurityAdmin.RestoreUserInteraction(aFilter);
			}

			aSecurityMenuLoader.Dispose();
			return newObjects;
		}
		//---------------------------------------------------------------------

		#endregion

		#region Funzioni x la Creazione /Modifica del Tree della Console
		
		#region Creazione

		public void UpdateConsoleTree(TreeView treeConsole)
		{
			//Icona lucchetto
			Stream myStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + ".img.SECURITYICON.bmp");
			StreamReader myreader = new StreamReader(myStream);
            int indexIcon = consoleTree.ImageList.Images.Add(Image.FromStream(myStream, true), Color.Magenta);

            PlugInTreeNode lastNodeTree = (PlugInTreeNode)treeConsole.Nodes[treeConsole.Nodes.Count - 1];

			PlugInTreeNode rootPlugInNode = new PlugInTreeNode(Strings.SecurityTitle);
			rootPlugInNode.AssemblyName = SecurityConstString.SecurityAdminPlugIn;
			rootPlugInNode.AssemblyType = typeof(SecurityAdmin);
			rootPlugInNode.ImageIndex = indexIcon;
			rootPlugInNode.SelectedImageIndex = indexIcon; 
			rootPlugInNode.Type = SecurityConstString.OSLXPSecurity;
			rootPlugInNode.ToolTipText = Strings.ToolTipString;

			//Azienda
			PlugInTreeNode companiesPlugInNode = new PlugInTreeNode(Strings.Companies);
			companiesPlugInNode.AssemblyName = SecurityConstString.SecurityAdminPlugIn;
			companiesPlugInNode.AssemblyType = typeof(SecurityAdmin);
			companiesPlugInNode.Type = Strings.Companies;
			companiesPlugInNode.Checked = false;
            companiesPlugInNode.ImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;
            companiesPlugInNode.SelectedImageIndex = PlugInTreeNode.GetCompaniesDefaultImageIndex;
			
			lastNodeTree.Nodes.Add(rootPlugInNode);
			rootPlugInNode.Nodes.Add(companiesPlugInNode);
			LoadCompanies(companiesPlugInNode);
			
			lastNodeTree.Expand();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Leggo le Company e aggiungo al Tree Aziende 
		/// </summary>
		/// <param name="rootPlugInNode"></param>
		public void LoadCompanies(PlugInTreeNode rootPlugInNode)
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return;

			// se sono nella Standard Edition allora devo caricare nel tree solo le aziende censite dal SysAdmin (le prime 2)
			string sSelect = 
				(isStandardEdition) 
				? "SELECT CompanyId, Company, Disabled, IsValid FROM MSD_Companies WHERE UseSecurity = 1 AND CompanyId = @companyId"
				: "SELECT CompanyId, Company, Disabled, IsValid FROM MSD_Companies WHERE UseSecurity = 1";

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;
			
			try
			{
				mySqlCommand = new SqlCommand(sSelect, sqlConnection);

				// Standard Edition
				if (isStandardEdition)
				{
					mySqlCommand.Parameters.Add("@companyId", SqlDbType.NVarChar);

					foreach (string coId in companiesIdAdmitted)
					{
						mySqlCommand.Parameters["@companyId"].Value = coId;
						myReader = mySqlCommand.ExecuteReader();
						AddCompanyNode(rootPlugInNode, myReader);
						myReader.Close();
					}
				}
				else
				{
					// Professional Edition
					myReader = mySqlCommand.ExecuteReader();
					AddCompanyNode(rootPlugInNode, myReader);
					myReader.Close();
				}

				if (mySqlCommand != null)
					mySqlCommand.Dispose();
			}
			catch (SqlException e)
			{
				Debug.Fail("SqlException raised in SecurityAdminPlugIn.LoadCompanies: " + e.Message);
				if (mySqlCommand != null)
					mySqlCommand.Dispose();
				if (myReader != null && !myReader.IsClosed)
					myReader.Close();
			}
		}

		/// <summary>
		/// Iterando sul DataReader passato come parametro aggiungo i nodi al tree delle aziende del PlugIn
		/// </summary>
		//---------------------------------------------------------------------
		private void AddCompanyNode(PlugInTreeNode rootPlugInNode, SqlDataReader myReader)
		{
			while (myReader.Read())
			{
				PlugInTreeNode currentNode = new PlugInTreeNode();
				currentNode.AssemblyName = SecurityConstString.SecurityAdminPlugIn;
				currentNode.AssemblyType = typeof(SecurityAdmin);
				currentNode.Type = Strings.Company;
				currentNode.Text = myReader["Company"].ToString();
				currentNode.Id = myReader["CompanyId"].ToString();
                currentNode.SelectedImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
                currentNode.ImageIndex = PlugInTreeNode.GetCompanyDefaultImageIndex;
				currentNode.IsValid = Convert.ToBoolean(myReader["IsValid"].ToString());
				currentNode.ContextMenu = GetCompanyNodeContextMenu();
                companies.Add(Convert.ToUInt32(myReader["CompanyId"]));
             	int ind = rootPlugInNode.Nodes.Add(currentNode);
				if (Convert.ToInt32(myReader["Disabled"]) == 1)
				{
					currentNode.ForeColor = Color.Red;
                    rootPlugInNode.Nodes[ind].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
				}

				// se l'azienda non è valida, aggiungo l'icona di stato dell'azienda da migrare, non aggancio alcun
				// menu di contesto e non procedo nell'aggancio dei nodi di tipo ruolo/utente
				if (!currentNode.IsValid)
				{
					currentNode.ForeColor = Color.Red;
                    rootPlugInNode.Nodes[ind].StateImageIndex = PlugInTreeNode.GetCompaniesToMigrateImageIndex;
					currentNode.ContextMenu = null;
					continue;
				}

				//ora devo caricare i ruoli (se esistono)
				// il contenitore Ruoli ha come Id l'id della company
				PlugInTreeNode roleNode = new PlugInTreeNode();
				roleNode.AssemblyName = SecurityConstString.SecurityAdminPlugIn;
				roleNode.AssemblyType = typeof(SecurityAdmin);
				roleNode.Type = Strings.Roles;
				roleNode.Text = Strings.Roles;
                roleNode.ImageIndex = PlugInTreeNode.GetRolesDefaultImageIndex;
                roleNode.SelectedImageIndex = PlugInTreeNode.GetRolesDefaultImageIndex;
				roleNode.CompanyId = myReader["CompanyId"].ToString();
		
				//ora leggo per ogni company i ruoli ad essa associati
				currentNode.Nodes.Add(roleNode);

				LoadAllRoles(Convert.ToInt32(myReader["CompanyId"]), roleNode);
					
				//ora devo caricare gli utenti associati alla company
				// il contenitore Utenti ha come Id l'id della company
				PlugInTreeNode usersNode = new PlugInTreeNode();
				usersNode.AssemblyName = SecurityConstString.SecurityAdminPlugIn;
				usersNode.AssemblyType = typeof(SecurityAdmin);
				usersNode.Type = Strings.Users;
				usersNode.Text = Strings.Users;
                usersNode.ImageIndex = PlugInTreeNode.GetUsersDefaultImageIndex;
                usersNode.SelectedImageIndex = PlugInTreeNode.GetUsersDefaultImageIndex;
				usersNode.CompanyId = myReader["CompanyId"].ToString();

				currentNode.Nodes.Add(usersNode);
				//ora leggo per ogni compani tutti gli utenti associati
				LoadAllUsersOfCompany(Convert.ToInt32(myReader["CompanyId"]), usersNode);
			}
		}

		
		//----------------------------------------------------------------------
		private void LoadAllUsersOfCompany(int idCompany, PlugInTreeNode roleNode)
		{
			if (string.IsNullOrEmpty(connectionString))
				return;
			
			SqlConnection connection = new SqlConnection(connectionString);

			int EasyLookLoginId = CommonObjectTreeFunction.GetApplicationUserID(NameSolverStrings.EasyLookSystemLogin, connection);
			connection.Close();

			SqlConnection tmpConnection = new SqlConnection(connectionString);
			tmpConnection.Open();

			string sSelect = @"SELECT MSD_CompanyLogins.LoginId, MSD_Logins.Login, MSD_CompanyLogins.Disabled,
								MSD_CompanyLogins.Admin FROM MSD_CompanyLogins INNER JOIN
								MSD_Logins ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId where
								CompanyId=" + idCompany + " AND MSD_CompanyLogins.LoginId <> " + EasyLookLoginId + " ORDER BY MSD_Logins.Login";

			SqlCommand mySqlCommand = new SqlCommand(sSelect, tmpConnection);
			SqlDataReader myReader = mySqlCommand.ExecuteReader();

			while (myReader.Read())
			{
				PlugInTreeNode currentNode = new PlugInTreeNode();
				currentNode.AssemblyName = SecurityConstString.SecurityAdminPlugIn;
				currentNode.AssemblyType = typeof(SecurityAdmin);
				currentNode.Type = Strings.User;
				currentNode.Text = myReader["Login"].ToString();
				currentNode.Id = myReader["LoginId"].ToString();
				currentNode.CompanyId = idCompany.ToString();
				currentNode.ImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;
                currentNode.SelectedImageIndex = PlugInTreeNode.GetUserDefaultImageIndex;

				currentNode.ContextMenu = GetRoleOrUserNodeContextMenu();
				currentNode.ForeColor = (Convert.ToInt32(myReader["Disabled"]) == 1) ? Color.Red : Color.Black;
				
				int indexRoleNode = roleNode.Nodes.Add(currentNode);

				if (Convert.ToInt32(myReader["Disabled"]) == 1)
                    roleNode.Nodes[indexRoleNode].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
				if (Convert.ToInt32(myReader["Admin"]) == 1)
                    roleNode.Nodes[indexRoleNode].StateImageIndex = PlugInTreeNode.GetKeyStateImageIndex;
			}
			myReader.Close();
			mySqlCommand.Dispose();

			tmpConnection.Close();
		}

		//---------------------------------------------------------------------
		private void LoadAllRoles(int idCompany, PlugInTreeNode roleNode)
		{
			if (connectionString == null || connectionString == String.Empty)
				return;

			SqlConnection tmpConnection = new SqlConnection(connectionString);
			tmpConnection.Open();

			string sSelect = "SELECT RoleId, Role, Disabled FROM MSD_CompanyRoles WHERE CompanyId=" + idCompany + " and readonly = 0 ORDER BY Role";

			SqlCommand mySqlCommand = new SqlCommand(sSelect, tmpConnection);
			SqlDataReader myReader = mySqlCommand.ExecuteReader();

			while (myReader.Read())
			{
				PlugInTreeNode currentNode = new PlugInTreeNode();
				currentNode.AssemblyName = SecurityConstString.SecurityAdminPlugIn;
				currentNode.AssemblyType = typeof(SecurityAdmin);
				currentNode.Type = Strings.Role;
				currentNode.Text = myReader["Role"].ToString();
				currentNode.Id = myReader["RoleId"].ToString();
				currentNode.CompanyId = idCompany.ToString();
                currentNode.ImageIndex = PlugInTreeNode.GetRoleDefaultImageIndex;
                currentNode.SelectedImageIndex = PlugInTreeNode.GetRoleDefaultImageIndex;

				currentNode.ContextMenu = GetRoleOrUserNodeContextMenu();

				if (Convert.ToInt32(myReader["Disabled"]) == 1)
					currentNode.ForeColor = Color.Red;
				else
					currentNode.ForeColor = Color.Black;
				
				int indexRoleNode = roleNode.Nodes.Add(currentNode);

				if (Convert.ToInt32(myReader["Disabled"]) == 1)
                    roleNode.Nodes[indexRoleNode].StateImageIndex = PlugInTreeNode.GetUncheckStateImageIndex;
			}
			myReader.Close();
			mySqlCommand.Dispose();

			tmpConnection.Close();
		}

		#endregion

		#region Modifica 
		public void InsertTreeCompany()
		{
            PlugInTreeNode node;
			for (int i = 0; i < consoleTree.Nodes[0].Nodes.Count; i++)
			{
				node = (PlugInTreeNode)consoleTree.Nodes[0].Nodes[i];
				if (node.Type == SecurityConstString.OSLXPSecurity)
					node.Remove();
			}
			UpdateConsoleTree(consoleTree);
		}
		//---------------------------------------------------------------------
		public void InsertRoleinTree(int aCompanyId)
		{
            PlugInTreeNode node = FindNodeOfType(this.consoleTree.Nodes, Strings.Companies);
            PlugInTreeNode selNode;
			int position = -1;
			for (int i = 0; i < node.Nodes.Count; i++)
			{
				selNode = node.Nodes[i] as PlugInTreeNode;
                if (selNode != null && aCompanyId == Convert.ToInt32(selNode.Id))
				{
					position = i;
					break;
				}
			}
			//Ho Trovato la company
            node = FindNodeOfType(((TreeNode)node).Nodes[position].Nodes, Strings.Roles);
			node.Nodes.Clear();	
			LoadAllRoles(aCompanyId, node);
		}

		//---------------------------------------------------------------------
		public void InsertUserinTree(int aCompanyId)
		{
			PlugInTreeNode node = FindNodeOfType(consoleTree.Nodes, Strings.Companies);
			PlugInTreeNode selNode;
			int position = -1;
			for (int i = 0; i < node.Nodes.Count; i++)
			{
				selNode = node.Nodes[i] as PlugInTreeNode;
                if (selNode != null && aCompanyId == Convert.ToInt32(selNode.Id))
				{
					position = i;
					break;
				}
			}
			//Ho Trovato la company
			node = FindNodeOfType(((TreeNode)node).Nodes[position].Nodes, Strings.Users);
			node.Nodes.Clear();	
			LoadAllUsersOfCompany(aCompanyId, node); 
		}

        //---------------------------------------------------------------------
        #endregion

        #endregion

        #region Funzioni di Refresh

        private void OnAfterImportRole(object sender, int aCompanyId, bool refreshList)
		{
			if (aCompanyId == -1)
				return;

			InsertRoleinTree(aCompanyId);

			this.workingAreaConsole.Controls.Clear();
			this.workingAreaConsoleBottom.Visible = false;
            DestroyShowObjectsTree();
			if (!refreshList) return;
			if (OnRefreshRolesFromImport != null)
				OnRefreshRolesFromImport(sender, aCompanyId);

		}
		
		//---------------------------------------------------------------------
		private void OnAfterExportRole(object sender)
		{
			this.workingAreaConsole.Controls.Clear();
			this.workingAreaConsoleBottom.Visible = false;
		}
		
		//---------------------------------------------------------------------

		#endregion

		#region Abilitazione/Disabiltazione delle interazioni dell'utente

		//--------------------------------------------------------------------------------------------------------------------------------
		// In certi casi si rende assolutamente necessario inibire qualsiasi possibile interazione 
		// con l'applicazione da parte dell'utente.
		// Ad esempio, il caricamento delle informazioni relative al menù scatena degli eventi 
		// che qui vengono gestiti con la visualizzazione della ProgressBar della console.
		// Se, mentre si stanno caricando le informazioni relative al menù, l'utente
		// preme un tasto o clicca su un control presente nella finestra della console, viene 
		// lanciato il comando corrispondente e vengono di conseguenza scatenati gli eventi relativi.
		// Purtroppo, dalle prove fatte la disabiltazione della form principale della console non
		// risulta essere sufficiente: i comandi vengono inviati comunque!
		// Pertanto, si rende necessario l'utilizzo di un filtro sui messaggi che impedisce
		// l'arrivo di messaggi all'applicazione causati da una qualunque interazione con essa
		// da parte dell'utente.
		//============================================================================
		private class UserInputMessageFilter : IMessageFilter 
		{
			//--------------------------------------------------------------------------------------------------------------------------------
			public bool PreFilterMessage(ref Message message) 
			{
				return	
					message.Msg == 273 || // Blocks all the WM_COMMAND messages.
					message.Msg == 256 || // Blocks all the WM_KEYDOWN messages.
					message.Msg == 257 || // Blocks all the WM_KEYUP messages.
					(message.Msg >= 513 && message.Msg <= 521);// Blocks all the messages relating to the mouse buttons.
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static IMessageFilter DisableUserInteraction()
		{
			UserInputMessageFilter aFilter = new UserInputMessageFilter();
			
			Cursor.Current = Cursors.WaitCursor;

			Application.AddMessageFilter(aFilter); // Adds a message filter to monitor Windows messages as they are routed to their destinations.

			return aFilter;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static void RestoreUserInteraction(IMessageFilter aFilter)
		{
			if (aFilter != null)
				Application.RemoveMessageFilter(aFilter); // Removes a message filter from the message pump of the application.

			// Se non si "ripulisce" la coda dai messaggi arrivati in fase di esecuzione di caricamento 
			// delle informazioni relative al menù, si possono riscontrare degli effetto indesiderati. 
			// Ad esempio, se durante la fase di caricamento del menù l'utente clicca sulla working
			// area della console e poi subito dopo si visualizza una dialog box, quest'ultima 
			// non è modale.
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			// Chiamando Application.DoEvents prima di reimpostare il cursore a Cursors.Default, l'applicazione
			// si rimetterà all'ascolto degli eventi del mouse e visualizzerà il cursore appropriato per ciascun
			// controllo.
			Application.DoEvents();

			// Set Cursor.Current to Cursors.Default to display the appropriate cursor for each control
			Cursor.Current = Cursors.Default;
		}

		#endregion

		#region Eventi del ShowObjectsTree
		
		//---------------------------------------------------------------------
		private void ShowObjectsTree_SelectCommandNode(object sender, System.EventArgs e)
		{
			if 
				(
				workingAreaConsole != null && 
				sender != null && 
				sender is ShowObjectsTree && 
				((ShowObjectsTree)sender).MenuManagerWinControl != null &&
				((ShowObjectsTree)sender).MenuManagerWinControl.CurrentCommandNode != null
				)
			{
				if (
					((ShowObjectsTree)sender).MenuManagerWinControl.CurrentCommandNode.IsRunText || 
					((ShowObjectsTree)sender).MenuManagerWinControl.CurrentCommandNode.IsRunExecutable
					)
				{
					DiagnosticViewer.ShowWarning(Strings.NotSecurityObject, Strings.Warning);
					return;
				}
			
				DestroySetGrants();
				workingAreaConsole.Controls.Clear();

                if (((ShowObjectsTree)sender).MenuManagerWinControl.CurrentCommandNode.ExternalItemType == SecurityType.Toolbar.ToString()) //||
                //    ((ShowObjectsTree)sender).MenuManagerWinControl.CurrentCommandNode.ExternalItemType == SecurityType.TileManager.ToString())
                    return;

				SetGrants setGrantsForm = new SetGrants((ShowObjectsTree)sender);
				setGrantsForm.OnAfterModifyFormStateHandler += new SetGrants.AfterModifyFormStateHandler(SetWorkingAreaButton);
				setGrantsForm.TopLevel = false;
			
				setGrantsForm.Dock = DockStyle.Fill;
				
				OnBeforeAddFormFromPlugIn(this, setGrantsForm.ClientSize.Width, setGrantsForm.ClientSize.Height);
			
				
				workingAreaConsole.Controls.Add(setGrantsForm);

				workingAreaConsole.BringToFront();
				workingAreaConsole.Visible = true;
				
				setGrantsForm.Visible = true;
				
				// Abilito nella toolbar della console il pulsante per il salvataggio
				if (OnEnableSaveToolBarButton != null)
					OnEnableSaveToolBarButton(sender, new System.EventArgs());
			}
		}

		//---------------------------------------------------------------------
		private void ShowObjectsTree_RefreshGrants(object sender, System.EventArgs e)
		{
			if 
				(
				workingAreaConsole != null && 
				sender != null && 
				sender is ShowObjectsTree
				)
			{
				DestroySetGrants();
				workingAreaConsole.Controls.Clear();
				SetGrants setGrantsForm = new SetGrants((ShowObjectsTree)sender);
				setGrantsForm.OnAfterModifyFormStateHandler += new SetGrants.AfterModifyFormStateHandler(SetWorkingAreaButton);
				
				setGrantsForm.TopLevel = false;
				setGrantsForm.Dock = DockStyle.Fill;
				setGrantsForm.Size = new Size(workingAreaConsole.Width, workingAreaConsole.Height);

				OnBeforeAddFormFromPlugIn(sender, setGrantsForm.Width, setGrantsForm.Height);

				this.workingAreaConsole.Controls.Add(setGrantsForm);
				this.workingAreaConsole.BringToFront();
				
				setGrantsForm.Visible = true;
				this.workingAreaConsole.Visible = true;
			}
		}

		//---------------------------------------------------------------------
		public void ShowObjectsTree_ClearWorkingArea(object sender, System.EventArgs e)
		{
			DestroySetGrants();
			workingAreaConsole.Controls.Clear();
		}
		
		#endregion
	
		#region metodi privati
		
		//---------------------------------------------------------------------
		private void SetWorkingAreaButton(object sender, bool isEditingState)
		{
			if (workingAreaConsole.Controls == null || workingAreaConsole.Controls.Count == 0)
				return;

			Type controlsType = workingAreaConsole.Controls[0].GetType();
			if (controlsType.Name == "SetGrants")
			{
				((ShowObjectsTree)this.workingAreaConsoleBottom.Controls[0]).IsEditingState = isEditingState;
				//	((ShowObjectsTree)this.workingAreaConsoleBottom.Controls[0]).Enabled = false;
			}
		}

		//---------------------------------------------------------------------
		private void ShowWizardGrants(object sender, DynamicEventsArgs e)
		{
			if (workingAreaConsole != null && sender is ShowObjectsTree)
			{
				DestroySetGrants();
				workingAreaConsole.Controls.Clear();

				WizardForms.StartingWizardForm startingWizardForm = new WizardForms.StartingWizardForm((ShowObjectsTree)sender);
				startingWizardForm.OnRefreshWorkingAreaEventHandler += new WizardForms.StartingWizardForm.RefreshWorkingAreaEventHandler(RefreshWorkingArea);
				startingWizardForm.TopLevel = false;
				startingWizardForm.Dock = DockStyle.Fill;
				//	OnBeforeAddFormFromPlugIn(sender, startingWizardForm.ClientSize.Width, startingWizardForm.ClientSize.Height);
			
				workingAreaConsole.Controls.Add(startingWizardForm);
				startingWizardForm.Show();

				workingAreaConsole.Visible = true;
				
				startingWizardForm.Visible = true;
				

				workingAreaConsole.BringToFront();
			}
		}

		private void SummaryGrants(object sender, DynamicEventsArgs e)
		{
			if (workingAreaConsole != null && sender is ShowObjectsTree)
			{
				DestroySetGrants();
				workingAreaConsole.Controls.Clear();

				ViewObjectGrants viewObjectGrants = new ViewObjectGrants((ShowObjectsTree)sender);

				viewObjectGrants.TopLevel = false;
				viewObjectGrants.Dock = DockStyle.Fill;
				
				workingAreaConsole.Controls.Add(viewObjectGrants);
				viewObjectGrants.Show();

				workingAreaConsole.Visible = true;
				
				viewObjectGrants.Visible = true;
				
				workingAreaConsole.BringToFront();
			}
		}

		//---------------------------------------------------------------------
		private void RefreshWorkingArea(object sender, EventArgs e)
		{
			DestroySetGrants();
			workingAreaConsole.Controls.Clear();
		}

		//---------------------------------------------------------------------
		private void DestroyShowObjectsTree()
		{
			if (showObjectsTree == null)
				return;

			if (workingAreaConsoleBottom.Controls.Contains(showObjectsTree))
				workingAreaConsoleBottom.Controls.Remove(showObjectsTree);
			
			showObjectsTree.Dispose();
			showObjectsTree = null;
		}
		
		//---------------------------------------------------------------------
		private void SetProgresForGlobalOperations(object sender, int count)
		{
			EnableProgressBarFromPlugIn(sender);
						
			SetProgressBarStepFromPlugIn(this, 1);

			SetProgressBarMinValueFromPlugIn(this, 0);
			SetProgressBarMaxValueFromPlugIn(this, count);

			SetProgressBarTextFromPlugIn(this, Strings.PleaseWait);

			SetProgressBarValueFromPlugIn(this, 0);

			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		#endregion

		//----------------------------------------------------------------------------
		public void SetProgresBarIncrementForGlobalOperations(object sender, int count)
		{
			progress++;
			SetProgressBarValueFromPlugIn(this, progress);
		}

		//----------------------------------------------------------------------------
		public void SetEndedProgresBar(object sender, EventArgs e)
		{
			progress = 0;
			SetProgressBarTextFromPlugIn(this, String.Empty);

			DisableProgressBarFromPlugIn(this);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//-----------------------------------------------------------------------------
		#region Eventi del SecurityMenuLoader
		
		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_LoadMenuStarted(object sender, MenuParserEventArgs e)
		{ 
			EnableProgressBarFromPlugIn(sender);
						
			SetProgressBarStepFromPlugIn(this, 1);

			SetProgressBarMinValueFromPlugIn(this, 0);
			SetProgressBarMaxValueFromPlugIn(this, e.Counter);

			SetProgressBarTextFromPlugIn(this, Strings.SecurityMenuLoading);

			SetProgressBarValueFromPlugIn(this, 0);

			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_LoadMenuIndexChanged(object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarValueFromPlugIn(this, e.Counter);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_LoadMenuEnded(object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarTextFromPlugIn(this, String.Empty);

			DisableProgressBarFromPlugIn(this);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}
		
		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_ProcessingStarted(object sender, MenuParserEventArgs e)
		{ 
			EnableProgressBarFromPlugIn(this);
						
			SetProgressBarStepFromPlugIn(this, 1);

			SetProgressBarMinValueFromPlugIn(this, 0);
			SetProgressBarMaxValueFromPlugIn(this, e.Counter);

			SetProgressBarTextFromPlugIn(this, Strings.SecurityMenuProcessing);

			SetProgressBarValueFromPlugIn(this, 0);

			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_ProcessingIndexChanged(object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarValueFromPlugIn(this, e.Counter);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_ProcessingEnded(object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarTextFromPlugIn(this, String.Empty);

			DisableProgressBarFromPlugIn(this);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}
		
		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_WriteToDBStarted(object sender, MenuParserEventArgs e)
		{ 
			EnableProgressBarFromPlugIn(this);
					
			SetProgressBarStepFromPlugIn(this, 1);

			SetProgressBarMinValueFromPlugIn(this, 0);
			SetProgressBarMaxValueFromPlugIn(this, e.Counter);

			SetProgressBarTextFromPlugIn(this, Strings.SecurityMenuObjsWriteToDB);

			SetProgressBarValueFromPlugIn(this, 0);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_WriteToDBIndexChanged(object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarValueFromPlugIn(this, e.Counter);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		public void SecurityMenuLoader_WriteToDBEnded(object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarTextFromPlugIn(this, String.Empty);

			DisableProgressBarFromPlugIn(this);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		#endregion

		#region SecurityAdminPlugin Common function
		
		//---------------------------------------------------------------------
		private void CreateViewSecurityPlugInInfo()
		{
			DestroySetGrants();
			workingAreaConsole.Controls.Clear();
			workingAreaConsoleBottom.Visible = false;
			
			ViewSecurityPlugInInfo viewSecurityPlugInInfo = new ViewSecurityPlugInInfo(brandLoader);
			viewSecurityPlugInInfo.TopLevel = false;
			viewSecurityPlugInInfo.Dock = DockStyle.Fill;
			
			workingAreaConsole.Controls.Add(viewSecurityPlugInInfo);
			
			viewSecurityPlugInInfo.Show();
			
			workingAreaConsole.Visible = true;
		}
		//---------------------------------------------------------------------
		private void CreateShowObjectsTree()
		{
			int applicationsPanelWidth = -1;
			int menuTreeWidth = -1;
			string applicationName = String.Empty;
			string groupName = String.Empty;
			string menuPath = String.Empty;
			string commandPath = String.Empty;

			ShowObjectsTree.ShowConfigurationState currentShowConfigurationState = ShowObjectsTree.ShowConfigurationState.Normal;
			
			workingAreaConsoleBottom.Visible = false;

			if (showObjectsTree != null)
			{
				// Salvo le dimensioni dei pannelli e l'ultima posizione selezionata del menù 
				// visualizzato dentro showObjectsTree per tentare poi di ripristinarli
				// nella nuova istanza del controllo
				applicationsPanelWidth = showObjectsTree.MenuManagerWinControl.ApplicationsPanelWidth;
				menuTreeWidth = showObjectsTree.MenuManagerWinControl.MenuTreeWidth;
				
				showObjectsTree.GetCurrentSelection(ref applicationName, ref groupName, ref menuPath, ref commandPath);
				
				currentShowConfigurationState = showObjectsTree.CurrentShowConfigurationState;

				DestroyShowObjectsTree();
			}
		
			DestroySetGrants();
			workingAreaConsole.Controls.Clear();

			string pathFinderUser = IsRoleLogin ? NameSolverStrings.AllUsers : loginName;
			PathFinder pathFinder = new PathFinder(CommonObjectTreeFunction.GetCompanyName(companyId, sqlConnection), pathFinderUser);

			showObjectsTree = new ShowObjectsTree
				(
				companyId,
				roleOrUserId,
				loginName,
				IsRoleLogin,
				pathFinder, 
				sqlConnection,
				currentShowConfigurationState, 
				connectionString
				);
		
			showObjectsTree.OnSelectCommandNode += new System.EventHandler(ShowObjectsTree_SelectCommandNode);
			showObjectsTree.OnAfterModifyGrants += new System.EventHandler(ShowObjectsTree_RefreshGrants);
			showObjectsTree.OnAfterModifyAllGrants += new System.EventHandler(ShowObjectsTree_ClearWorkingArea);
			showObjectsTree.OnAfterSelectObjectTreeNode += new System.EventHandler(ShowObjectsTree_ClearWorkingArea);
			showObjectsTree.OnAfterClickWizardGrants += new ShowObjectsTree.AfterClickWizardGrants(ShowWizardGrants);
			showObjectsTree.OnAfterClickSummaryGrants += new ShowObjectsTree.AfterClickSummaryGrants(SummaryGrants);
			
			showObjectsTree.ScanStandardMenuComponentsStarted += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuStarted);
			showObjectsTree.ScanStandardMenuComponentsEnded += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuEnded);
			
			showObjectsTree.LoadAllMenuFilesStarted += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuStarted);
			showObjectsTree.LoadAllMenuFilesModuleIndexChanged += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuIndexChanged);
			showObjectsTree.LoadAllMenuFilesEnded += new MenuParserEventHandler(SecurityMenuLoader_LoadMenuEnded);
			
			showObjectsTree.LoadMenuOtherObjectsStarted += new MenuParserEventHandler(SecurityMenuLoader_ProcessingStarted);
			showObjectsTree.LoadMenuOtherObjectsModuleIndexChanged += new MenuParserEventHandler(SecurityMenuLoader_ProcessingIndexChanged);
			showObjectsTree.LoadMenuOtherObjectsEnded += new MenuParserEventHandler(SecurityMenuLoader_ProcessingEnded);

			showObjectsTree.OnGlobalOperationsStarted += new ShowObjectsTree.GlobalOperationsStarted(SetProgresForGlobalOperations);
			showObjectsTree.OnGlobalOperationsIncrement += new ShowObjectsTree.GlobalOperationsIncrement(SetProgresBarIncrementForGlobalOperations);
			showObjectsTree.OnProgressBarForGlobalOperationsEnded += new ShowObjectsTree.ProgressBarForGlobalOperationsEnded(SetEndedProgresBar);

			showObjectsTree.OnSaveChanges += new ShowObjectsTree.SaveChanges(SaveChanges);

			showObjectsTree.TopLevel = false;
			showObjectsTree.Dock = DockStyle.Fill;

			if (applicationsPanelWidth > 0)
				showObjectsTree.MenuManagerWinControl.ApplicationsPanelWidth = applicationsPanelWidth;
			if (menuTreeWidth > 0)
				showObjectsTree.MenuManagerWinControl.MenuTreeWidth = menuTreeWidth;

			workingAreaConsoleBottom.Controls.Add(showObjectsTree);

			showObjectsTree.PerformLayout();

			showObjectsTree.Visible = true;
			workingAreaConsoleBottom.Visible = true;
			
			// Devo ripristinare l'ultima selezione del menù solo dopo aver impostato la property Visible
			// della workingAreaConsoleBottom a true, perchè altrimenti non è ancora stato processato
			// da showObjectsTree l'evento di MenuMngWinCtrl_Load nel quale viene caricato il menù
			showObjectsTree.MenuManagerWinControl.Select(applicationName, groupName, menuPath, commandPath);

			workingAreaConsoleBottom.Enabled = true;

			//Sparo l'evento x far vedere il pulsante delle chiavette premuto
			if (OnSetShowSecurityIconsToolBarButtonPushed != null)
			{
				OnSetShowSecurityIconsToolBarButtonPushed(this, true);
				showObjectsTree.ShowSecurityIcons = true;
			}
			return;
		}

		//---------------------------------------------------------------------
		private bool GetShowObjectsTreeForCurrentlySelectedConsoleTreeNode()
		{
			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTree.SelectedNode;
			if 
				(
				selectedNode == null ||
				String.Compare(selectedNode.AssemblyName, SecurityConstString.SecurityAdminPlugIn) != 0 ||
				(selectedNode.Type != Strings.User && selectedNode.Type != Strings.Role)
				)
				return false;

			companyId = Convert.ToInt32(selectedNode.CompanyId);
			loginType = selectedNode.Type;
			roleOrUserId = Convert.ToInt32(selectedNode.Id);
			loginName = selectedNode.Text;

			if 
				(
				showObjectsTree != null &&
				showObjectsTree.CompanyId == companyId &&
				showObjectsTree.RoleOrUserId == roleOrUserId &&
				String.Compare(showObjectsTree.LoginName, loginName) == 0
				)
			{
				if (!workingAreaConsoleBottom.Controls.Contains(showObjectsTree))
					workingAreaConsoleBottom.Controls.Add(showObjectsTree);
			}
			else
			{
				CreateShowObjectsTree();
			}

			if (showObjectsTree == null)
				return false;

			workingAreaConsoleBottom.Visible = true;
			workingAreaConsoleBottom.Enabled = true;

			return true;
		}
		
		//---------------------------------------------------------------------
		private PlugInTreeNode FindNodeOfType(TreeNodeCollection nodes, string nodeType)
		{			
			if (nodes == null || nodes.Count == 0)
				return null;

			foreach (TreeNode aPlugInTreeNode in nodes)
			{
				if (
					(((PlugInTreeNode)aPlugInTreeNode).AssemblyName == SecurityConstString.SecurityAdminPlugIn) &&
                    (((PlugInTreeNode)aPlugInTreeNode).Type == nodeType)
					)
				{
                    return (PlugInTreeNode)aPlugInTreeNode;
				}

				PlugInTreeNode aDescendantFound = FindNodeOfType(aPlugInTreeNode.Nodes, nodeType);
				if (aDescendantFound != null)
					return aDescendantFound;
			}

			return null;
		}
		
		//---------------------------------------------------------------------
		private bool IsCompanySecurity(int aCompanyId)
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return false;

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;

			try
			{
				string sSelect = "SELECT * FROM MSD_Companies WHERE CompanyId = @CompanyId and UseSecurity=1";
				mySqlCommand = new SqlCommand(sSelect, sqlConnection);
                mySqlCommand.Parameters.AddWithValue("@CompanyId", aCompanyId);
				myReader = mySqlCommand.ExecuteReader();
				bool existCompany = myReader.Read();
				myReader.Close();
				mySqlCommand.Dispose();
				return existCompany;
			}
			catch (SqlException)
			{
				myReader.Close();
				mySqlCommand.Dispose();

				return false;
			}
		}

		//---------------------------------------------------------------------
		private bool IsCompanyUser(string userId)
		{
			if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
				return false;

			SqlCommand mySqlCommand = null;
			SqlDataReader myReader = null;

			try
			{
				string sSelect = "SELECT * FROM MSD_CompanyLogins WHERE LoginId = @LoginId";
				mySqlCommand = new SqlCommand(sSelect, sqlConnection);
                mySqlCommand.Parameters.AddWithValue("@LoginId", Convert.ToInt32(userId));
				myReader = mySqlCommand.ExecuteReader();
				bool existUser = myReader.Read();
				myReader.Close();
				mySqlCommand.Dispose();
				return existUser;

			}
			catch (SqlException)
			{
				myReader.Close();
				mySqlCommand.Dispose();
				return false;
			}
		}
		
		//---------------------------------------------------------------------
		public void DisableGenericToolbarButtons()
		{
			if (OnDisableNewToolBarButton != null)
				OnDisableNewToolBarButton(this, null);
			
			if (OnDisableOpenToolBarButton != null)
				OnDisableOpenToolBarButton(this, null);
			
			if (OnDisableDeleteToolBarButton != null)
				OnDisableDeleteToolBarButton(this, null);
			
			if (OnDisableQueryToolBarButton != null)
				OnDisableQueryToolBarButton(this, null);
		}

		//---------------------------------------------------------------------
		public void EnableShowConfigurationToolbarButtons()
		{
			if (OnEnableOtherObjectsToolBarButton != null)
				OnEnableOtherObjectsToolBarButton(this, null);
			
			if (OnEnableShowSecurityIconsToolBarButton != null)
				OnEnableShowSecurityIconsToolBarButton(this, null);

            if (OnEnableApplySecurityFilterToolBarButton != null)
                OnEnableApplySecurityFilterToolBarButton(this, null);
		}

		//---------------------------------------------------------------------
		public void DisableShowConfigurationToolbarButtons()
		{
			if (OnDisableOtherObjectsToolBarButton != null)
				OnDisableOtherObjectsToolBarButton(this, null);

			if (OnDisableShowSecurityIconsToolBarButton != null)
				OnDisableShowSecurityIconsToolBarButton(this, null);

			if (OnDisableApplySecurityFilterToolBarButton != null)
				OnDisableApplySecurityFilterToolBarButton(this, null);
		}

		#endregion
	}
}