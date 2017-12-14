using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SecurityLight
{
	/// <summary>
	/// Summary description for SecurityLightPlugIn.
	/// </summary>
	public class SecurityLight : PlugIn
	{
		# region private data member
		private static readonly string securityLightPlugInName = Assembly.GetExecutingAssembly().GetName().Name;
		private PathFinder			consolePathFinder = null;
		private Form				consoleMainForm = null;
		private TreeView	    	consoleTreeView = null;
		private Panel				consoleWorkingArea = null;
		private Panel				consoleWorkingAreaBottom = null;
		private string				buildArgument = String.Empty;
		private string				configurationArgument = String.Empty;
		private bool				runningFromServerArgument = false;

		// console environment information
		private ConsoleEnvironmentInfo consoleEnvironmentInfo = null;

		private bool				isStandardEdition	= false;
		private StringCollection	companiesIdAdmitted = null;

		private Microarea.Console.Plugin.SecurityLight.SecurityLightForm securityLightForm = null;

        private ContextMenuStrip rootNodeContextMenu = null;
        private ToolStripMenuItem refreshAllSLDenyFilesMenuItem = null;
        
        # endregion

		#region Eventi per la disabilitazione dei pulsanti della ToolBar della console
	
		// Disabilita il pulsante Save
		public event System.EventHandler OnDisableSaveToolBarButton;

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
		
		// Disabilito il pulsante per l'applicazione del filtro di sicurezza
		public event System.EventHandler OnDisableApplySecurityFilterToolBarButton;

        public event System.EventHandler OnDisableFindSecurityObjectsToolBarButton;

        public event System.EventHandler OnDisableShowAllGrantsToolBarButtonPushed;

		#endregion

		#region Eventi dalla Microarea Console

		//ritorna lo stato di console
		public delegate StatusType GetConsoleStatus();
		public event GetConsoleStatus OnGetConsoleStatus;

		#endregion

		//--------------------------------------------------------------------------------------------------------
		public SecurityLight()
		{
		}

		//--------------------------------------------------------------------------------------------------------
		public override void Load
			(
				ConsoleGUIObjects		consoleGUIObjects,
				ConsoleEnvironmentInfo	consoleEnvironmentInfo,
				LicenceInfo				licenceInfo
			)
		{
            MenuStrip consoleMenu = consoleGUIObjects.MenuConsole; 
			if (consoleMenu != null)
				consoleMainForm = consoleMenu.FindForm();

			consoleTreeView				= consoleGUIObjects.TreeConsole; 
			consoleWorkingArea			= consoleGUIObjects.WkgAreaConsole; 
			consoleWorkingAreaBottom	= consoleGUIObjects.BottomWkgAreaConsole; 
			runningFromServerArgument	= consoleEnvironmentInfo.RunningFromServer; 

			this.consoleEnvironmentInfo = consoleEnvironmentInfo;
			isStandardEdition			= (String.Compare(licenceInfo.Edition, NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0);
		
			securityLightForm = new Microarea.Console.Plugin.SecurityLight.SecurityLightForm(consoleEnvironmentInfo);
			securityLightForm.EnableMenuLoad = false;
			securityLightForm.Dock = System.Windows.Forms.DockStyle.Fill;
			securityLightForm.Location = new System.Drawing.Point(0, 33);
			securityLightForm.Name = "SecurityLightForm";
			securityLightForm.TabIndex = 0;
			securityLightForm.TopLevel = false;
			securityLightForm.Visible = false;
		
			securityLightForm.BeforeMenuLoad								+= new EventHandler(SecurityLightForm_BeforeMenuLoad);
			securityLightForm.AfterMenuLoad									+= new EventHandler(SecurityLightForm_AfterMenuLoad);
			securityLightForm.ScanStandardMenuComponentsStarted				+= new MenuParserEventHandler(SecurityLightForm_ScanStandardMenuStarted);
			securityLightForm.ScanStandardMenuComponentsModuleIndexChanged	+= new MenuParserEventHandler(SecurityLightForm_ScanStandardMenuIndexChanged);
			securityLightForm.ScanStandardMenuComponentsEnded				+= new MenuParserEventHandler(SecurityLightForm_ScanStandardMenuEnded);																												  
			securityLightForm.ScanCustomMenuComponentsStarted				+= new MenuParserEventHandler(SecurityLightForm_ScanCustomMenuStarted);
			securityLightForm.ScanCustomMenuComponentsModuleIndexChanged	+= new MenuParserEventHandler(SecurityLightForm_ScanCustomMenuIndexChanged);
			securityLightForm.ScanCustomMenuComponentsEnded					+= new MenuParserEventHandler(SecurityLightForm_ScanCustomMenuEnded);																												  
			securityLightForm.LoadAllMenuFilesStarted						+= new MenuParserEventHandler(SecurityLightForm_LoadMenuStarted);
			securityLightForm.LoadAllMenuFilesModuleIndexChanged			+= new MenuParserEventHandler(SecurityLightForm_LoadMenuIndexChanged);
			securityLightForm.LoadAllMenuFilesEnded							+= new MenuParserEventHandler(SecurityLightForm_LoadMenuEnded);
		}

		#region Eventi della MicroareaConsole 

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole","OnInitPathFinder")]
		public void OnInitPathFinder(PathFinder pathFinder)
		{
			consolePathFinder = pathFinder;
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnSaveCompanyUser")]
		public void OnSaveCompanyUser(object sender, string id, string companyId)
		{
			if (securityLightForm != null)
				securityLightForm.RefreshUsersAndCompanies();
		}
		
		//---------------------------------------------------------------------
		// Se è stato clonato un utente in una azienda è necessario rileggere 
		// tutti gli utenti di questa azienda
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterClonedUserCompany")]
		public void OnAfterClonedUserCompany(string companyId)
		{
			if (securityLightForm != null)
				securityLightForm.RefreshUsersAndCompanies();
		}

		//---------------------------------------------------------------------
		// Cancella un utente associato a una azienda
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterDeleteCompanyUser")]
		public void OnAfterDeleteCompanyUser(object sender, string userId, string companyId)
		{
			if (securityLightForm != null)
			{
				securityLightForm.DeleteCompanyUserAccessRights(Convert.ToInt32(companyId), Convert.ToInt32(userId));
				securityLightForm.RefreshUsersAndCompanies();
			}
		}
		
		//---------------------------------------------------------------------
		// Cancellazione di una azienda 
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnDeleteCompanyFromSysAdmin")]
		public void OnDeleteCompanyFromSysAdmin(object sender, string companyId)
		{
			if (securityLightForm != null)
			{
				securityLightForm.DeleteCompanyAccessRights(Convert.ToInt32(companyId));
				securityLightForm.RefreshUsersAndCompanies();
			}
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterSavedCompany")]
		public void OnAfterSavedCompany(object sender, string companyId)
		{
			if (securityLightForm != null)
				securityLightForm.RefreshUsersAndCompanies();
		}
		
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnDeleteUserToPlugIns")]
		public void OnDeleteUserToPlugIns(object sender, string userId)
		{
			if (securityLightForm != null)
			{
				securityLightForm.DeleteUserAccessRights(Convert.ToInt32(userId));
				securityLightForm.RefreshUsersAndCompanies();
			}
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
			
			if (isStandardEdition)
				companiesIdAdmitted = ((StringCollection)(e.Get("CompaniesIdAdmitted")));

			UpdateConsoleTree();

            if (securityLightForm != null)
            {
                securityLightForm.UpdateConnectionData(e.Get("DbServerIstance").ToString());
                if (refreshAllSLDenyFilesMenuItem != null)
                    refreshAllSLDenyFilesMenuItem.Enabled = (consolePathFinder != null);
            }
		}

		//--------------------------------------------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin","OnAfterLogOff")]
		public void OnAfterLogOff(object sender, System.EventArgs e)
		{		
			ClearSecurityLightForm();

			PlugInTreeNode rootPlugInNode = GetPlugInRootNode();
			if (rootPlugInNode != null)
				rootPlugInNode.Remove();

			this.consoleEnvironmentInfo.ConsoleUserInfo.UserName   = String.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.UserPwd    = String.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.IsWinAuth  = false;
			this.consoleEnvironmentInfo.ConsoleUserInfo.ServerName = String.Empty;
			this.consoleEnvironmentInfo.ConsoleUserInfo.DbName     = String.Empty;

			// commentato per evitare il ri-caricamento del plugin quando esco dalla Console
			/*if (securityLightForm != null)
            {
                securityLightForm.UpdateConnectionData();
                if (refreshAllSLDenyFilesMenuItem != null)
                    refreshAllSLDenyFilesMenuItem.Enabled = (consolePathFinder != null);
            }*/
        }

		/// <summary>
		/// ShutDownFromPlugIn
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.MicroareaConsole", "OnShutDownConsole")]
		public override bool ShutDownFromPlugIn()
		{
			return (base.ShutDownFromPlugIn() && (securityLightForm == null || !securityLightForm.IsLoadingMenu));
		}
		
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
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserName = String.Empty;
			this.consoleEnvironmentInfo.ConsoleUserGuestInfo.UserPwd  = String.Empty;
		}
		#endregion
		
		#region Eventi sul Tree della Console
		//--------------------------------------------------------------------------------------------------------
		public void OnAfterSelectConsoleTree(object sender, TreeViewEventArgs e)
		{
			RemoveSecurityLightForm();

			PlugInTreeNode selectedNode = (PlugInTreeNode)consoleTreeView.SelectedNode;
			if (String.Compare(selectedNode.AssemblyName, securityLightPlugInName, true, CultureInfo.InvariantCulture) != 0)
				return;

			DisableConsoleToolBarButtons();

			// Ho visto che comunque è meglio pulire completamente la working area della
			// console visto che non tutti i PlugIn si preoccupano di eliminare da essa
			// i loro control quando non sono più "protagonisti". Se ciascuna PlugIn si
			// impegnasse, invece, ad eliminare dalla working area della console tutti e
			// soli gli oggetti aggiunti da lei, non dovrebbe essercene bisogno... 
			consoleWorkingArea.Controls.Clear();

			//// Ci sono PlugIn che impostano lo stato di Visible della working area a false
			// e poi non lo ripristinano più a true...
			consoleWorkingArea.Visible = true;

			consoleWorkingAreaBottom.Enabled = false;
			consoleWorkingAreaBottom.Visible = false;

			if(String.Compare(selectedNode.Type, "SecurityLight") == 0)
			{
				if (securityLightForm != null)
				{
					securityLightForm.Visible = true;
					if (!consoleWorkingArea.Controls.Contains(securityLightForm))
						consoleWorkingArea.Controls.Add(securityLightForm);

					securityLightForm.EnableMenuLoad = true;
				}
			}
		}
		
		#endregion

		#region SecurityLightForm event handlers

		//----------------------------------------------------------------------------
		public void SecurityLightForm_BeforeMenuLoad(object sender, System.EventArgs e)
		{
			DisableConsoleToolBars();
		
			if (securityLightForm != null)
				securityLightForm.Enabled = false;

			if (consoleTreeView != null)
				consoleTreeView.Enabled = false;
		}
		
		//----------------------------------------------------------------------------
		public void SecurityLightForm_AfterMenuLoad(object sender, System.EventArgs e)
		{
			EnableConsoleToolBars();

			if (securityLightForm != null)
				securityLightForm.Enabled = true;
	
			if (consoleTreeView != null)
				consoleTreeView.Enabled = true;
		}

		//----------------------------------------------------------------------------
		public void SecurityLightForm_ScanStandardMenuStarted(object sender, MenuParserEventArgs e)
		{ 
			if (this.consoleMainForm != null)
				this.consoleMainForm.Cursor = Cursors.WaitCursor;

			EnableProgressBarFromPlugIn(sender);
						
			SetProgressBarStepFromPlugIn(this, 1);

			SetProgressBarMinValueFromPlugIn(this, 0);
			SetProgressBarMaxValueFromPlugIn(this, e.Counter);

			SetProgressBarTextFromPlugIn(this, Strings.ScanStandardMenuComponentsText);

			SetProgressBarValueFromPlugIn (this, 0);

			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		public void SecurityLightForm_ScanStandardMenuIndexChanged (object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarValueFromPlugIn (this, e.Counter);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}
		
		//----------------------------------------------------------------------------
		public void SecurityLightForm_ScanStandardMenuEnded (object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarTextFromPlugIn(this, String.Empty);

			DisableProgressBarFromPlugIn(this);
		
			if (this.consoleMainForm != null)
				this.consoleMainForm.Cursor = Cursors.Default;

			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}
		
		//----------------------------------------------------------------------------
		public void SecurityLightForm_ScanCustomMenuStarted(object sender, MenuParserEventArgs e)
		{ 
			if (this.consoleMainForm != null)
				this.consoleMainForm.Cursor = Cursors.WaitCursor;
			
			EnableProgressBarFromPlugIn(sender);
						
			SetProgressBarStepFromPlugIn(this, 1);

			SetProgressBarMinValueFromPlugIn(this, 0);
			SetProgressBarMaxValueFromPlugIn(this, e.Counter);

			SetProgressBarTextFromPlugIn(this, Strings.ScanCustomMenuComponentsText);

			SetProgressBarValueFromPlugIn (this, 0);

			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		public void SecurityLightForm_ScanCustomMenuIndexChanged (object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarValueFromPlugIn (this, e.Counter);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}
		
		//----------------------------------------------------------------------------
		public void SecurityLightForm_ScanCustomMenuEnded (object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarTextFromPlugIn(this, String.Empty);

			DisableProgressBarFromPlugIn(this);
		
			if (this.consoleMainForm != null)
				this.consoleMainForm.Cursor = Cursors.Default;
			
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}
		
		//----------------------------------------------------------------------------
		public void SecurityLightForm_LoadMenuStarted(object sender, MenuParserEventArgs e)
		{ 
			if (this.consoleMainForm != null)
				this.consoleMainForm.Cursor = Cursors.WaitCursor;

			EnableProgressBarFromPlugIn(sender);
						
			SetProgressBarStepFromPlugIn(this, 1);

			SetProgressBarMinValueFromPlugIn(this, 0);
			SetProgressBarMaxValueFromPlugIn(this, e.Counter);

			SetProgressBarTextFromPlugIn(this, Strings.LoadAllMenuFilesStartedText);

			SetProgressBarValueFromPlugIn (this, 0);

			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

		//----------------------------------------------------------------------------
		public void SecurityLightForm_LoadMenuIndexChanged (object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarValueFromPlugIn (this, e.Counter);
		
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}
		
		//----------------------------------------------------------------------------
		public void SecurityLightForm_LoadMenuEnded (object sender, MenuParserEventArgs e)
		{ 
			SetProgressBarTextFromPlugIn(this, String.Empty);

			DisableProgressBarFromPlugIn(this);
		
			if (this.consoleMainForm != null)
				this.consoleMainForm.Cursor = Cursors.Default;
			
			// Il metodo Application.DoEvents processa tutti i messaggi correntemente in coda.
			Application.DoEvents();
		}

        //----------------------------------------------------------------------------
        public void RefreshAllSLDenyFiles(object sender, EventArgs e)
        {
            if (securityLightForm == null || securityLightForm.IsDisposed)
                return;

            if (this.consoleMainForm != null)
                this.consoleMainForm.Cursor = Cursors.WaitCursor;

            SecurityLightManager.RebuildAllSLDenyFiles(consolePathFinder, securityLightForm.CurrentConnectionString, securityLightForm.GetUserNamesToSkip());

            if (this.consoleMainForm != null)
                this.consoleMainForm.Cursor = Cursors.Default;
        }

        #endregion // SecurityLightForm event handlers

		#region SecurityLightPlugIn private methods
		
		//---------------------------------------------------------------------
		private void UpdateConsoleTree()
		{
			if (consoleTreeView == null)
				return;

			Image rootImage = null;
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.SecurityLightRootNode.gif");
			if (imageStream != null)
				rootImage = Image.FromStream(imageStream);
			int rootImageIndex = (rootImage != null) ? consoleTreeView.ImageList.Images.Add(rootImage, Color.Transparent) : -1;

			PlugInTreeNode rootPlugInNode		= new PlugInTreeNode(Strings.RootPlugInNodeText);
			rootPlugInNode.AssemblyName = securityLightPlugInName;
			rootPlugInNode.AssemblyType			= typeof(SecurityLight);
			rootPlugInNode.Type					= "SecurityLight";
			rootPlugInNode.ImageIndex			= rootImageIndex;
			rootPlugInNode.SelectedImageIndex	= rootImageIndex;
			rootPlugInNode.ToolTipText  		= Strings.RootPlugInNodeToolTipText;
            rootPlugInNode.ContextMenuStrip     = GetPlugInRootNodeContextMenuStrip();
	
			consoleTreeView.Nodes[consoleTreeView.Nodes.Count-1].Nodes.Add(rootPlugInNode);
		}

		/// <summary>
		/// Iterando sul DataReader passato come parametro aggiungo i nodi al tree delle aziende del PlugIn
		/// </summary>
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
				if (String.Compare(consoleNode.Type,"SecurityLight") == 0)
					return consoleNode;
			}
			return null;
		}

        //--------------------------------------------------------------------------------------------------------
        private ContextMenuStrip GetPlugInRootNodeContextMenuStrip()
        {
            if (rootNodeContextMenu != null)
                return rootNodeContextMenu;

            rootNodeContextMenu = new ContextMenuStrip();

			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GenericStrings.SecurityLightNamespace + ".Images.RefreshAllSLDenyFilesMenuItem.gif");
            Image itemImage = (imageStream != null) ? Image.FromStream(imageStream) : null;
            refreshAllSLDenyFilesMenuItem = new ToolStripMenuItem(Strings.RefreshAllSLDenyFilesMenuItemText, itemImage, new System.EventHandler(RefreshAllSLDenyFiles));
            refreshAllSLDenyFilesMenuItem.Enabled = (securityLightForm != null && consolePathFinder != null);

            rootNodeContextMenu.Items.Add(refreshAllSLDenyFilesMenuItem);

            return rootNodeContextMenu;
        }

        //--------------------------------------------------------------------------------------------------------
        private System.Windows.Forms.ToolStrip[] GetConsoleToolBars()
        {
            if (consoleMainForm == null || consoleMainForm.Controls.Count == 0)
                return null;

			ArrayList toolBars = new ArrayList();
            foreach (Control aControl in consoleMainForm.Controls)
            {
                if (aControl != null && (aControl is System.Windows.Forms.ToolStrip))
                    toolBars.Add(aControl);

                if (aControl != null && aControl is System.Windows.Forms.ContainerControl)
                {
                    System.Windows.Forms.ToolStrip[] controlToolBars = GetContainerControlToolBars((System.Windows.Forms.ContainerControl)aControl);
                    if (controlToolBars != null && controlToolBars.Length > 0)
                        toolBars.AddRange(controlToolBars);
                }
            }

            return (toolBars.Count > 0) ? (System.Windows.Forms.ToolStrip[])toolBars.ToArray(typeof(System.Windows.Forms.ToolStrip)) : null;
        }
        
        //--------------------------------------------------------------------------------------------------------
        private System.Windows.Forms.ToolStrip[] GetContainerControlToolBars(System.Windows.Forms.ContainerControl aContainerControl)
		{
            if (aContainerControl == null || aContainerControl.Controls.Count == 0)
				return null;

			ArrayList toolBars = new ArrayList();

            foreach (Control aControl in aContainerControl.Controls)
			{
                if (aControl != null && (aControl is System.Windows.Forms.ToolStrip))
					toolBars.Add(aControl);

                if (aControl != null && aControl is System.Windows.Forms.ContainerControl)
                {
                    System.Windows.Forms.ToolStrip[] controlToolBars = GetContainerControlToolBars((System.Windows.Forms.ContainerControl)aControl);
                    if (controlToolBars != null && controlToolBars.Length > 0)
                        toolBars.AddRange(controlToolBars);
                }
			}

            return (toolBars.Count > 0) ? (System.Windows.Forms.ToolStrip[])toolBars.ToArray(typeof(System.Windows.Forms.ToolStrip)) : null;
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void EnableConsoleToolBars(bool enable)
		{
            ToolStrip[] toolBars = GetConsoleToolBars();
			if (toolBars == null || toolBars.Length == 0)
				return;

            foreach (ToolStrip aToolBar in toolBars)
				aToolBar.Enabled = enable;
		}

		//--------------------------------------------------------------------------------------------------------
		private void EnableConsoleToolBars()
		{
			EnableConsoleToolBars(true);
		}

		//--------------------------------------------------------------------------------------------------------
		private void DisableConsoleToolBars()
		{
			EnableConsoleToolBars(false);
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void DisableConsoleToolBarButtons()
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

            if (OnDisableFindSecurityObjectsToolBarButton!= null)
                OnDisableFindSecurityObjectsToolBarButton(this, null);

            if (OnDisableShowAllGrantsToolBarButtonPushed != null)
                OnDisableShowAllGrantsToolBarButtonPushed(this, null);
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void RemoveSecurityLightForm()
		{
			if (securityLightForm == null)
				return;

			securityLightForm.Visible = false;

			if (consoleWorkingArea.Controls.Contains(securityLightForm))
				consoleWorkingArea.Controls.Remove(securityLightForm);
		}
		
		//--------------------------------------------------------------------------------------------------------
		private void ClearSecurityLightForm()
		{
			if (securityLightForm == null)
				return;

			securityLightForm.ClearInfo();

			RemoveSecurityLightForm();
		}
		
		#endregion

	}
}
