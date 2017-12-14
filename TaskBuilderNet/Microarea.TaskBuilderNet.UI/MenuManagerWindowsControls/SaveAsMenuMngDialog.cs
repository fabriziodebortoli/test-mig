using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WinControls;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	/// <summary>
	/// Summary description for SaveAsMenuMngDialog.
	/// </summary>
	public partial class SaveAsMenuMngDialog : System.Windows.Forms.Form
	{
		private	    string officeFileName = String .Empty;
		private	    string menuFileName = String.Empty;
		private	    string menuItemXmlText = String.Empty;
		
		protected   MenuLoader.CommandsTypeToLoad commandsTypeToLoad = MenuLoader.CommandsTypeToLoad.All;			
		private		IPathFinder		pathFinder = null;
		private		LoginManager	loginManager = null;
		private		bool			firstactivation = true;

		protected	MenuLoader		menuLoader = null;
		protected	MenuSelections	lastSelections = null;
		protected	MenuXmlNode		selectedCommandNode = null;

		private Thread				loadMenuInfoProgressThread = null;
		private ManualResetEvent	menuLoadStartEvent = null;
		private ManualResetEvent	menuLoadedEvent = null;
		private int					loadProgressBarMaximum = 0;
		private int					loadProgressBarValue = 0;
		private string				loadingMenuInfoText = String.Empty;		

		protected MenuSearchForm	commandsSearchForm = null;
		private   OfficeType	    officeType;


		#region SaveAsMenuMngDialog constructors

		//----------------------------------------------------------------------------
		public SaveAsMenuMngDialog(PathFinder aPathFinder, LoginManager aLoginManager, bool initResources, OfficeType officeType)
		{
			// l'impostazione della culture va effettuata prima di chiamare la InitializeComponent
			// (altrimenti la form non viene tradotta)
			if (aLoginManager != null && aLoginManager.LoginManagerState == LoginManagerState.Logged)
				DictionaryFunctions.SetCultureInfo(aLoginManager.PreferredLanguage, aLoginManager.ApplicationLanguage);
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			pathFinder = aPathFinder;
			MenuManagerWinCtrl.PathFinder = pathFinder;
			loginManager = aLoginManager;
			this.officeType = officeType;	
			if (initResources)
			{
				this.Text = MenuManagerWindowsControlsStrings.MenuManagerDialogCaption;

				System.Drawing.Icon ico = InstallationData.BrandLoader.GetTbAppManagerApplicationIcon();
				if (ico != null)
				{
					this.Icon = ico;
				}
				else
				{
					Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Icons.MenuManagerDialog.ico");
					if (iconStream != null)
					{
						this.Icon = new System.Drawing.Icon(iconStream);
					}
				}
			}
			
			if(officeType == OfficeType.Word)
			{
				cbxFileType.Items.Add(".dot");
				cbxFileType.Items.Add(".doc");
				cbxFileType.Items.Add(".dotx");
				cbxFileType.Items.Add(".docx");
                cbxFileType.SelectedItem = ".dotx";
			}
			if(officeType == OfficeType.Excel)
			{
				cbxFileType.Items.Add(".xlt");
				cbxFileType.Items.Add(".xls");
				cbxFileType.Items.Add(".xlsx");
				cbxFileType.Items.Add(".xltx");
				cbxFileType.SelectedItem = ".xltx";
			}
		}

		//----------------------------------------------------------------------------
		public SaveAsMenuMngDialog(IPathFinder aPathFinder, LoginManager aLoginManager, bool initResources)
		{
			if (aLoginManager != null && aLoginManager.LoginManagerState == LoginManagerState.Logged)
				DictionaryFunctions.SetCultureInfo(aLoginManager.PreferredLanguage, aLoginManager.ApplicationLanguage);
			InitializeComponent();

			pathFinder = aPathFinder;
			MenuManagerWinCtrl.PathFinder = pathFinder;

			loginManager = aLoginManager;
		
			if (initResources)
			{
				this.Text = MenuManagerWindowsControlsStrings.MenuManagerDialogCaption;

				System.Drawing.Icon ico = InstallationData.BrandLoader.GetTbAppManagerApplicationIcon();
				if (ico != null)
				{
					this.Icon = ico;
				}
				else
				{
					Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Icons.MenuManagerDialog.ico");
					if (iconStream != null)
					{
						this.Icon = new System.Drawing.Icon(iconStream);
					}
				}
			}
		}

		//----------------------------------------------------------------------------
		public SaveAsMenuMngDialog(PathFinder aPathFinder, LoginManager aLoginManager)
			: this(aPathFinder, aLoginManager, true)
		{
		}

		//----------------------------------------------------------------------------
		public SaveAsMenuMngDialog(MenuLoader aMenuLoader, MenuSelections lastSelectionsToRestore, bool initResources)
			: this
			(
			(aMenuLoader != null) ? aMenuLoader.PathFinder : null,
			(aMenuLoader != null) ? aMenuLoader.LoginManager : null,
			initResources
			)
		{
			menuLoader = aMenuLoader;
			lastSelections = lastSelectionsToRestore;
		}

		//----------------------------------------------------------------------------
		public SaveAsMenuMngDialog(MenuLoader aMenuLoader, MenuSelections lastSelectionsToRestore)
			: this(aMenuLoader, lastSelectionsToRestore, true)
		{
		}

		//----------------------------------------------------------------------------
		public SaveAsMenuMngDialog(MenuLoader aMenuLoader)
			: this(aMenuLoader, null)
		{
		}

		//----------------------------------------------------------------------------
		public SaveAsMenuMngDialog(MenuLoader aMenuLoader, bool initResources)
			: this(aMenuLoader, null, initResources)
		{
		}
		#endregion

		#region SaveAsMenuMngDialog public properties
		//----------------------------------------------------------------------------
		[Browsable(false)]
		public string OfficeFileName { get { return officeFileName; } }

		//----------------------------------------------------------------------------
		[Browsable(false)]
		public string MenuFileName { get { return menuFileName; } }

		//----------------------------------------------------------------------------
		[Browsable(false)]
		public string MenuItemXmlText { get { return menuItemXmlText; } }

		//----------------------------------------------------------------------------
		[Browsable(false)]
		public MenuLoader MenuLoader { get { return menuLoader; } }

		//----------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlNode CurrentMenuNode { get { return (this.MenuManagerWinCtrl != null) ? this.MenuManagerWinCtrl.CurrentMenuNode : null; } }

		//----------------------------------------------------------------------------
		[Browsable(false)]
		public MenuSelections LastSelections { get { return lastSelections; } }

		//----------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlNode SelectedCommandNode { get { return selectedCommandNode; } }

		//----------------------------------------------------------------------------
		public bool ShowEnhancedCommandsView
		{
			get { return (MenuManagerWinCtrl != null) ? MenuManagerWinCtrl.ShowEnhancedCommandsView : false; } 
			set
			{
				if (MenuManagerWinCtrl == null) 
					return;

				MenuManagerWinCtrl.ShowEnhancedCommandsView = value; 

				MenuManagerWinCtrl.EnableShowDocumentsOption(LoadDocumentCommands);
				MenuManagerWinCtrl.EnableShowReportsOption(LoadReportCommands);
				MenuManagerWinCtrl.EnableShowBatchesOption(LoadBatchCommands);
				MenuManagerWinCtrl.EnableShowFunctionsOption(LoadFunctionCommands);
				MenuManagerWinCtrl.EnableShowExecutablesOption(LoadExecutableCommands);
				MenuManagerWinCtrl.EnableShowTextsOption(LoadTextCommands);
				MenuManagerWinCtrl.EnableShowOfficeItemsOption(LoadOfficeItemCommands);
			} 
		}
	
		//-------------------------------------------------------------------------------------
		public bool LoadDocumentCommands
		{
			get
			{
				return ((commandsTypeToLoad & MenuLoader.CommandsTypeToLoad.Form) == MenuLoader.CommandsTypeToLoad.Form);
			}
		}

		//-------------------------------------------------------------------------------------
		public bool LoadReportCommands
		{
			get
			{
				return ((commandsTypeToLoad & MenuLoader.CommandsTypeToLoad.Report) == MenuLoader.CommandsTypeToLoad.Report);
			}
		}

		//-------------------------------------------------------------------------------------
		public bool LoadBatchCommands
		{
			get
			{
				return ((commandsTypeToLoad & MenuLoader.CommandsTypeToLoad.Batch) == MenuLoader.CommandsTypeToLoad.Batch);
			}
		}

		//-------------------------------------------------------------------------------------
		public bool LoadFunctionCommands
		{
			get
			{
				return ((commandsTypeToLoad & MenuLoader.CommandsTypeToLoad.Function) == MenuLoader.CommandsTypeToLoad.Function);
			}
		}

		//-------------------------------------------------------------------------------------
		public bool LoadExecutableCommands
		{
			get
			{
				return ((commandsTypeToLoad & MenuLoader.CommandsTypeToLoad.Exe) == MenuLoader.CommandsTypeToLoad.Exe);
			}
		}

		//-------------------------------------------------------------------------------------
		public bool LoadTextCommands
		{
			get
			{
				return ((commandsTypeToLoad & MenuLoader.CommandsTypeToLoad.Text) == MenuLoader.CommandsTypeToLoad.Text);
			}
		}

		//-------------------------------------------------------------------------------------
		public bool LoadOfficeItemCommands
		{
			get
			{
				return ((commandsTypeToLoad & MenuLoader.CommandsTypeToLoad.OfficeItem) == MenuLoader.CommandsTypeToLoad.OfficeItem);
			}
		}

		#endregion

		#region SaveAsMenuMngDialog private methods

        //----------------------------------------------------------------------------
        private void LoadMenu()
        {
            if (pathFinder == null || loginManager == null || loginManager.LoginManagerState != LoginManagerState.Logged)
                return;

            Cursor.Current = Cursors.WaitCursor;

            menuLoader = new MenuLoader(pathFinder, loginManager, true);

            // Creo un thread asincrono (loadMenuInfoProgressThread) che resta in vita durante
            // il processo di caricamento dei file di menù.
            // Tale thread visualizza un finestrella che, grazie ad una ProgressBar, indica lo
            // via via lo stato del caricamento,
            menuLoadStartEvent = new ManualResetEvent(false);
            menuLoadStartEvent.Reset();

            menuLoadedEvent = new ManualResetEvent(false);
            menuLoadedEvent.Reset();

            loadMenuInfoProgressThread = new Thread(new ThreadStart(LoadMenuInfoProgress));

            loadMenuInfoProgressThread.Start();

            CreateStatusBarInfoProgressControls();

            menuLoader.ScanStandardMenuComponentsStarted += new MenuParserEventHandler(ScanStandardMenuComponentsStarted);
            menuLoader.ScanStandardMenuComponentsEnded += new MenuParserEventHandler(ScanStandardMenuComponentsEnded);

            menuLoader.LoadAllMenus(commandsTypeToLoad);

            menuLoadedEvent.Set();

            DestroyStatusBarInfoProgressControls();

            Cursor.Current = Cursors.Default;

            MenuManagerWinCtrl.DestroyLoadingPanel();

            ShowMenu();
        }

        //----------------------------------------------------------------------------
        private void ShowMenu()
        {
            this.MenuFormStatusStrip.Visible = false;
            this.OKSaveButton.Visible = true;
            this.CancelSaveButton.Visible = true;
            this.lblFileName.Visible = true;
            this.lblFileType.Visible = true;
            this.FileNameTextBox.Visible = true;
            this.cbxFileType.Visible = true;
            
            if (IsAdmin)
            {
                this.StandardRadioButton.Visible = IsInDevelopment;
                this.AllUsersRadioButton.Visible = true;
                this.UserRadioButton.Visible = true;
                this.MenuMngDlgToolStripContainer.Padding = new Padding(0, 0, 0, this.MenuFormStatusStrip.Height);
                this.PerformLayout();

                string[] availableUsers = loginManager.EnumAllCompanyUsers(loginManager.CompanyId, true);
                if (availableUsers == null || availableUsers.Length == 0)
                    this.UsersComboBox.Enabled = false;
                
				foreach (string user in availableUsers)
                {
                    int addedItemIndex = UsersComboBox.Items.Add(user);
                    if (String.Compare(user, loginManager.UserName) == 0)
                        this.UsersComboBox.SelectedIndex = addedItemIndex;
                }
                if (this.UsersComboBox.Items.Count > 0 && this.UsersComboBox.SelectedIndex == -1)
                    this.UsersComboBox.SelectedIndex = 0;

                this.UsersComboBox.Visible = true;
            }
            else
            {
                this.StandardRadioButton.Visible = this.AllUsersRadioButton.Visible = false;
                this.UsersComboBox.Visible = this.UsersComboBox.Enabled = false;
                this.UserRadioButton.Visible = false;

                this.MenuMngDlgToolStripContainer.Padding = new Padding(0, 0, 0, this.MenuFormStatusStrip.Height);
                this.PerformLayout();
            }

            EnableCommandsSearch();
            EnableRefreshMenuLoader();

            if (menuLoader == null)
                return;

            Cursor.Current = Cursors.WaitCursor;

            SetNormalViewMenu(true);

            if (menuLoader.MenuInfo == null)            
                this.OKSaveButton.Enabled = false;

            Cursor.Current = Cursors.Default;

            OnMenuShowed();
        }

        //----------------------------------------------------------------------------
        private void HideMenu()
        {
            this.MenuFormStatusStrip.Visible = true;
            this.OKSaveButton.Visible = false;
            this.CancelSaveButton.Visible = false;
            this.lblFileName.Visible = false;
            this.lblFileType.Visible = false;
            this.FileNameTextBox.Visible = false;
            this.cbxFileType.Visible = false;
            this.StandardRadioButton.Visible = false;
            this.AllUsersRadioButton.Visible = false;
            this.UserRadioButton.Visible = false;
            this.UsersComboBox.Visible = false;

            this.MenuMngDlgToolStripContainer.Padding = new Padding(0, 0, 0, 0);
            this.PerformLayout();

            EnableCommandsSearch(false);
            EnableRefreshMenuLoader(false);

            this.MenuMngDlgToolStrip.Refresh();
        }

        //----------------------------------------------------------------------------
        private void SearchCommand()
        {
            if (menuLoader == null || menuLoader.MenuInfo == null)
                return;

            if (commandsSearchForm == null)
            {
                commandsSearchForm = new MenuSearchForm(menuLoader, this);
                commandsSearchForm.RunFoundCommand += new MenuSearchFormEventHandler(this.CommandsSearchForm_RunCommand);
                commandsSearchForm.Closed += new System.EventHandler(this.CommandsSearchForm_Closed);

                OnInitMenuSearch();

                commandsSearchForm.Show();
            }

            if (commandsSearchForm.WindowState == FormWindowState.Minimized)
                commandsSearchForm.WindowState = FormWindowState.Normal;

            commandsSearchForm.Activate();
        }

        //----------------------------------------------------------------------------
        private void RefreshMenu()
        {
            SaveCurrentSelection();
            HideMenu();

            MenuManagerWinCtrl.MenuXmlParser = null;

            LoadMenu();
        }

        //----------------------------------------------------------------------------
        private void CreateStatusBarInfoProgressControls(bool createProgressBar)
        {
            MenuFormStatusStrip.CreateInfoProgressControls(createProgressBar);
        }

        //----------------------------------------------------------------------------
        private void CreateStatusBarInfoProgressControls()
        {
            CreateStatusBarInfoProgressControls(true);
        }

        //----------------------------------------------------------------------------
        private void DestroyStatusBarInfoProgressControls()
        {
            this.MenuFormStatusStrip.DestroyInfoProgressControls();
        }

        //----------------------------------------------------------------------------
        private void LoadMenuInfoProgress()
        {
            if (menuLoadStartEvent != null)
                menuLoadStartEvent.WaitOne();

            do
            {
                UpdateMenuFormStatusBar();
            } while (menuLoadedEvent != null && !menuLoadedEvent.WaitOne(10, false));
        }

        //----------------------------------------------------------------------------
        private bool SetNormalViewMenu(bool firstShow)
        {
            if (!firstShow)
                return true;

            if (menuLoader == null || menuLoader.AppsMenuXmlParser == null)
                return false;

            if (!firstShow)
                SaveCurrentSelection();

            ControlFreezer tmpControlFreezer = new ControlFreezer(MenuManagerWinCtrl);
            tmpControlFreezer.Freeze();

            MenuManagerWinCtrl.MenuXmlParser = menuLoader.AppsMenuXmlParser;

            RestoreLastSavedSelection();

            tmpControlFreezer.Defreeze();

            return true;
        }

        //----------------------------------------------------------------------------
        private bool SetNormalViewMenu()
        {
            return SetNormalViewMenu(false);
        }

        //----------------------------------------------------------------------------
        private void SaveCurrentSelection()
        {
            if (lastSelections == null)
                lastSelections = new MenuSelections();

            lastSelections.SetApplicationsSelection
                    (
                    MenuManagerWinCtrl.CurrentApplicationName,
                    MenuManagerWinCtrl.CurrentGroupName,
                    MenuManagerWinCtrl.CurrentMenuPath,
                    MenuManagerWinCtrl.CurrentCommandPath
                    );
        }

        //----------------------------------------------------------------------------
        private void RestoreLastSavedSelection()
        {
            if (lastSelections == null)
            {
                MenuManagerWinCtrl.Select(null);
                return;
            }

            MenuParserSelection selectionToRestore = null;

            selectionToRestore = lastSelections.ApplicationsSelection;

            Cursor.Current = Cursors.WaitCursor;

            MenuManagerWinCtrl.Select(selectionToRestore);

            // Set Cursor.Current to Cursors.Default to display the appropriate cursor for each control
            Cursor.Current = Cursors.Default;
        }

		//----------------------------------------------------------------------------
		protected virtual void OnMenuShowed()
		{
		}

		//----------------------------------------------------------------------------
		protected virtual void OnInitMenuSearch()
		{
			if (commandsSearchForm == null)
				return;
				
			commandsSearchForm.EnableShowDocumentsOption(LoadDocumentCommands);
			commandsSearchForm.EnableShowReportsOption(LoadReportCommands);
			commandsSearchForm.EnableShowBatchesOption(LoadBatchCommands);
			commandsSearchForm.EnableShowFunctionsOption(LoadFunctionCommands);
			commandsSearchForm.EnableShowExecutablesOption(LoadExecutableCommands);
			commandsSearchForm.EnableShowTextsOption(LoadTextCommands);
			commandsSearchForm.EnableShowOfficeItemsOption(LoadOfficeItemCommands);
			commandsSearchForm.EnableApplicationsFilter = IsXmlParserPopulatedWithCommands(menuLoader.AppsMenuXmlParser);
		}

		//----------------------------------------------------------------------------
		protected virtual bool IsXmlParserPopulatedWithCommands(MenuXmlParser aMenuXmlParser)
		{  
			return (aMenuXmlParser != null && aMenuXmlParser.HasCommandDescendantsNodes);
		}

		//----------------------------------------------------------------------------
		protected virtual void OnSelectedCommandChanging(MenuMngCtrlCancelEventArgs e)
		{
		}

		//----------------------------------------------------------------------------
		protected virtual void OnSelectedCommandChanged(MenuMngCtrlEventArgs e)
		{
		}

		//----------------------------------------------------------------------------
		protected virtual bool OnOk()
		{
			SaveCurrentSelection();

			selectedCommandNode = MenuManagerWinCtrl.CurrentCommandNode;

			return (selectedCommandNode != null);
		}

        //----------------------------------------------------------------------------
        private void EnableCommandsSearch(bool enable)
        {
            this.SearchToolStripButton.Enabled = this.SearchToolStripMenuItem.Enabled = enable;
        }

        //----------------------------------------------------------------------------
        private void EnableCommandsSearch()
        {
            EnableCommandsSearch(menuLoader != null && menuLoader.MenuInfo != null);

            this.SearchToolStripButton.Visible = this.SearchToolStripMenuItem.Visible = (loginManager != null && loginManager.LoginManagerState == LoginManagerState.Logged);
        }

        //----------------------------------------------------------------------------
        private void EnableRefreshMenuLoader(bool enable)
        {
            this.RefreshMenuLoaderToolStripButton.Enabled = this.RefreshMenuLoaderToolStripMenuItem.Enabled = enable;
        }

        //----------------------------------------------------------------------------
        private void EnableRefreshMenuLoader()
        {
            EnableRefreshMenuLoader(menuLoader != null);

            this.RefreshMenuLoaderToolStripButton.Visible = this.RefreshMenuLoaderToolStripMenuItem.Visible = (loginManager != null && loginManager.LoginManagerState == LoginManagerState.Logged);
        }

        delegate void UpdateMenuFormStatusBarCallback();
		//----------------------------------------------------------------------------
		private void UpdateMenuFormStatusBar()
        {
            // InvokeRequired required compares the thread ID of the calling thread to the 
            // thread ID of the creating thread. If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                UpdateMenuFormStatusBarCallback d = new UpdateMenuFormStatusBarCallback(UpdateMenuFormStatusBar);
                this.Invoke(d, null);
            }
            else
            {
                if (this.MenuFormStatusStrip != null && !this.MenuFormStatusStrip.IsDisposed)
                {
                    this.MenuFormStatusStrip.InfoText = loadingMenuInfoText;
                    this.MenuFormStatusStrip.InfoProgressStep = 1;
                    this.MenuFormStatusStrip.InfoProgressMaximum = loadProgressBarMaximum;
                    this.MenuFormStatusStrip.InfoProgressValue = loadProgressBarValue;

                    this.MenuFormStatusStrip.Refresh();
                }
            }
        }
        
        #endregion

		#region SaveAsMenuMngDialog overridden protected methods

		//----------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);
			
			if (menuLoader == null)
				MenuManagerWinCtrl.DisplayLoadingPanel();		
		}

		//----------------------------------------------------------------------------
		protected override void OnActivated(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnActivated(e);

			if (!firstactivation)
				return;

			firstactivation = false;
			if (menuLoader == null)
			{
				LoadMenu();
			}
			else
			{
				if (menuLoader.MenuInfo == null || !IsXmlParserPopulatedWithCommands(menuLoader.AppsMenuXmlParser))
				{
					MessageBox.Show
						(
						MenuManagerWindowsControlsStrings.NoCommandsAvailableWarningMessage,
						MenuManagerWindowsControlsStrings.NoCommandsAvailableWarningCaption,
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning
						);
					this.DialogResult = DialogResult.Cancel;
					this.Close();
					return;
				}
				ShowMenu();
			}
		}

		//----------------------------------------------------------------------------
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			// Invoke base class implementation
			base.OnClosing(e);
		
			if (commandsSearchForm != null)
				commandsSearchForm.Close();
		}
		
		#endregion

		#region SaveAsMenuMngDialog event handlers


        //----------------------------------------------------------------------------
        private void ScanStandardMenuComponentsStarted(object sender, MenuParserEventArgs e)
        {
            loadProgressBarMaximum = e.Counter;

            if (menuLoadStartEvent != null)
                menuLoadStartEvent.Set();

            loadProgressBarValue = 0;

			loadingMenuInfoText = MenuManagerWindowsControlsStrings.ScanStandardMenuComponentsText;
        }

        //----------------------------------------------------------------------------
        private void ScanStandardMenuComponentsModuleIndexChanged(object sender, MenuParserEventArgs e)
        {
            this.MenuFormStatusStrip.IsProgressBarVisible = true;

            loadProgressBarValue = e.Counter;

			loadingMenuInfoText = MenuManagerWindowsControlsStrings.ScanStandardMenuComponentsText;
            loadingMenuInfoText += " (" + e.ModuleTitle + ")";
        }

        //----------------------------------------------------------------------------
        private void ScanStandardMenuComponentsEnded(object sender, MenuParserEventArgs e)
        {
            loadProgressBarValue = loadProgressBarMaximum;
        }

        //----------------------------------------------------------------------------
        private void ScanCustomMenuComponentsStarted(object sender, MenuParserEventArgs e)
        {
            this.MenuFormStatusStrip.IsProgressBarVisible = true;
            loadProgressBarValue = 0;
			loadingMenuInfoText = MenuManagerWindowsControlsStrings.ScanCustomMenuComponentsText;
        }

        //----------------------------------------------------------------------------
        private void ScanCustomMenuComponentsModuleIndexChanged(object sender, MenuParserEventArgs e)
        {
            loadProgressBarValue = e.Counter;

			loadingMenuInfoText = MenuManagerWindowsControlsStrings.ScanCustomMenuComponentsText;
            loadingMenuInfoText += " (" + e.ModuleTitle + ")";
        }

        //----------------------------------------------------------------------------
        private void ScanCustomMenuComponentsEnded(object sender, MenuParserEventArgs e)
        {
            loadProgressBarValue = loadProgressBarMaximum;
        }

        //----------------------------------------------------------------------------
        private void LoadAllMenuFilesStarted(object sender, MenuParserEventArgs e)
        {
            this.MenuFormStatusStrip.IsProgressBarVisible = true;
            loadProgressBarMaximum = e.Counter;
            loadProgressBarValue = 0;

			loadingMenuInfoText = MenuManagerWindowsControlsStrings.LoadAllMenuFilesStartedText;
        }

        //----------------------------------------------------------------------------
        private void LoadAllMenuFilesModuleIndexChanged(object sender, MenuParserEventArgs e)
        {
            loadProgressBarValue = e.Counter;

			loadingMenuInfoText = MenuManagerWindowsControlsStrings.LoadAllMenuFilesStartedText;

            loadingMenuInfoText += " (" + e.ModuleTitle + ")";
        }

        //----------------------------------------------------------------------------
        private void LoadAllMenuFilesEnded(object sender, MenuParserEventArgs e)
        {
            loadProgressBarValue = loadProgressBarMaximum;

			if (menuLoader.MenuInfo == null)
				return;

            ArrayList appsMenuErrorMsgs = menuLoader.MenuInfo.AppsMenuLoadErrorMessages;
            if (appsMenuErrorMsgs != null && appsMenuErrorMsgs.Count > 0)
            {
                foreach (string errorMsg in appsMenuErrorMsgs)
                    MessageBox.Show
                        (
                        errorMsg,
						MenuManagerWindowsControlsStrings.MenuLoadErrorCaption,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
            }
        }

        //----------------------------------------------------------------------------
        private void LoadCachedStandardMenuStarted(object sender, MenuParserEventArgs e)
        {
            this.MenuFormStatusStrip.IsProgressBarVisible = false;
			MenuFormStatusStrip.InfoText = MenuManagerWindowsControlsStrings.LoadCachedStandardMenuText;
        }

        //----------------------------------------------------------------------------
        private void LoadCachedStandardMenuEnded(object sender, MenuParserEventArgs e)
        {
            MenuFormStatusStrip.InfoText = String.Empty;
            this.MenuFormStatusStrip.IsProgressBarVisible = true;
        }

        //----------------------------------------------------------------------------
        private void LoadAllMenusEnded(object sender, MenuInfo aMenuInfo)
        {
            if (sender == null || !(sender is MenuLoader))
                return;

            if
                (
                aMenuInfo == null ||
                (
                !IsXmlParserPopulatedWithCommands(((MenuLoader)sender).AppsMenuXmlParser) &&
                !IsXmlParserPopulatedWithCommands(((MenuLoader)sender).EnvironmentXmlParser)
                )
                )
            {
                MenuManagerWinCtrl.StopLoadingPanelAnimation();

                MessageBox.Show
                    (
					MenuManagerWindowsControlsStrings.NoCommandsAvailableWarningMessage,
					MenuManagerWindowsControlsStrings.NoCommandsAvailableWarningCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                    );

                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        //----------------------------------------------------------------------------
        private void SearchToolStripButton_Click(object sender, EventArgs e)
        {
            SearchCommand();
        }

        //----------------------------------------------------------------------------
        private void SearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchCommand();
        }

        //----------------------------------------------------------------------------
        private void RefreshMenuLoaderToolStripButton_Click(object sender, EventArgs e)
        {
            RefreshMenu();
        }

        //----------------------------------------------------------------------------
        private void RefreshMenuLoaderToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            RefreshMenu();
        }

        //----------------------------------------------------------------------------
		private void MenuManagerWinCtrl_SelectedCommandChanging(object sender, MenuMngCtrlCancelEventArgs e)
		{
			OnSelectedCommandChanging(e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void MenuManagerWinCtrl_SelectedMenuChanged(object sender, MenuMngCtrlTreeViewEventArgs e)
		{
			if (e != null && e.ItemObject != null && e.ItemType.IsMenu)
		       this.OKSaveButton.Enabled = true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuManagerWinCtrl_SelectedCommandChanged(object sender, MenuMngCtrlEventArgs e)
		{
			//In questo caso non va disabilitato il pulsante perchè si tratta della selezione di una voce di
			//un sottomenù, che in salvataggio non ci interessa
		    //this.OKSaveButton.Enabled = (e.ItemObject != null && e.ItemObject.Length > 0);
			
			//inserisco nel campo "nome file" il nome del nodo purgato dagli spazi
			FileNameTextBox.Text = e.Title.Replace(" ", "");
			OnSelectedCommandChanged(e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuManagerWinCtrl_RunCommand(object sender, MenuMngCtrlEventArgs e)
		{
			if (e == null || e.ItemType.IsUndefined)
				return;
			
			this.DialogResult = OnOk() ? DialogResult.OK : DialogResult.None;
		}

		//----------------------------------------------------------------------------
		private void CommandsSearchForm_Closed(object sender, System.EventArgs e)
		{
			commandsSearchForm = null;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CommandsSearchForm_RunCommand(object sender, MenuSearchFormEventArgs e)
		{
			if 
				(
				menuLoader == null || 
				e == null || 
				e.Node == null || 
				!e.Node.IsCommand ||
				e.Parser == null
				)
				return;

			if (e.Parser == menuLoader.AppsMenuXmlParser)
				SetNormalViewMenu();

			MenuManagerWinCtrl.SelectCommandNode(e.Node);

			if (sender == commandsSearchForm && commandsSearchForm != null)
				commandsSearchForm.WindowState = FormWindowState.Minimized;

			this.Activate();
		}
		
		#endregion

		#region SaveAsMenuMngDialog SaveMenu

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool IsLogged
		{
			get
			{
				return (loginManager != null && loginManager.LoginManagerState == LoginManagerState.Logged);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsAdmin
		{
			get
			{
				return (IsLogged && loginManager.Admin);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsInDevelopment
		{
			get
			{
				return (loginManager != null && loginManager.IsDeveloperActivation());
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string BuildXmlItem(string FileName, string ModuleName, bool isExcel, string title)
		{
			string saveType = String.Empty;
			string xml = String.Empty;

			if(cbxFileType.SelectedItem.Equals(".xlt") || cbxFileType.SelectedItem.Equals(".dot"))
				saveType = "AppTemplate";

			if (cbxFileType.SelectedItem.Equals(".xltx") || cbxFileType.SelectedItem.Equals(".dotx"))
				saveType = "AppTemplate2007";

			if(cbxFileType.SelectedItem.Equals(".xls") || cbxFileType.SelectedItem.Equals(".doc"))
				saveType = "AppDocument";

			if (cbxFileType.SelectedItem.Equals(".xlsx") || cbxFileType.SelectedItem.Equals(".docx"))
				saveType = "AppDocument2007";

			xml = "<Data><OfficeItem application=\""+( isExcel ? "Excel":"Word") + "\"" + " magicdocuments_installed=\"true\" sub_type=\""+saveType+"\">";
			xml += "<Title localizable=\"True\" original_title=\""+title+"\">"+title+"</Title>";
			xml += "<Object>ERP."+ ModuleName +"."+FileName+"</Object>";
			xml += "</OfficeItem></Data>";

			return xml;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SaveOptionRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			UsersComboBox.Enabled = UserRadioButton.Checked;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OKSaveButton_Click(object sender, System.EventArgs e)
		{
			if (FileNameTextBox.Text == null || FileNameTextBox.Text == String.Empty)
			{
				MessageBox.Show(MenuManagerWindowsControlsStrings.EmptyFileName, MenuManagerWindowsControlsStrings.WarningFormCaption, MessageBoxButtons.OK);
				FileNameTextBox.Focus(); 
				this.DialogResult = DialogResult.None;
				return;
			}

			if (!StandardRadioButton.Checked && !AllUsersRadioButton.Checked && !UserRadioButton.Checked)
			{
				MessageBox.Show(MenuManagerWindowsControlsStrings.NoCheckSelected, MenuManagerWindowsControlsStrings.WarningFormCaption, MessageBoxButtons.OK);
				StandardRadioButton.Focus(); 
				this.DialogResult = DialogResult.None;
				return;
			}

			string fileName = FileNameTextBox.Text.Trim();
			if (officeType == OfficeType.Excel && !BaseApplicationInfo.IsValidExcelFileName(fileName + cbxFileType.SelectedItem.ToString()) ||
				officeType == OfficeType.Word && !BaseApplicationInfo.IsValidWordFileName(fileName + cbxFileType.SelectedItem.ToString()))
			{
				MessageBox.Show(MenuManagerWindowsControlsStrings.InvalidExtensionOrName, MenuManagerWindowsControlsStrings.WarningFormCaption, MessageBoxButtons.OK);
				FileNameTextBox.Focus(); 
				this.DialogResult = DialogResult.None;
				return;
			}

			MenuXmlNode currentNode = this.CurrentMenuNode;
			if (currentNode == null)
			{
				MessageBox.Show(MenuManagerWindowsControlsStrings.InvalidPathSelected, MenuManagerWindowsControlsStrings.WarningFormCaption, MessageBoxButtons.OK);
				MenuManagerWinCtrl.Focus();
				this.DialogResult = DialogResult.None;
				return;
			}

			string groupName = currentNode.GetGroupName();
			string saveModuleName = String.Empty;

			groupName = groupName.Substring(groupName.IndexOf(".") + 1);

			string applicationName = currentNode.GetApplicationName();
			string appStandardPath = this.pathFinder.GetStandardApplicationPath(applicationName);
			if (appStandardPath[appStandardPath.Length - 1] != Path.DirectorySeparatorChar)
				appStandardPath += Path.DirectorySeparatorChar;
			string appCustomPath = this.pathFinder.GetCustomApplicationPath(loginManager.CompanyName, applicationName);
			if (appCustomPath[appCustomPath.Length - 1] != Path.DirectorySeparatorChar)
				appCustomPath += Path.DirectorySeparatorChar;

			if (Directory.Exists(appStandardPath + groupName))
				saveModuleName = groupName;
			else
			{
				string menuName = currentNode.GetMenuName();
				if (Directory.Exists(appStandardPath + menuName))
					saveModuleName = menuName;
				else
				{
					saveModuleName = "Company";
				}
			}				
           
			if (IsAdmin)
			{
				if (StandardRadioButton.Checked && IsInDevelopment)
				{
                    menuFileName = appStandardPath + saveModuleName + Path.DirectorySeparatorChar;
					officeFileName = menuFileName;

					menuFileName += NameSolverStrings.Menu;

					if(officeType == OfficeType.Word)
						officeFileName += NameSolverStrings.Word;
					if(officeType == OfficeType.Excel)
						officeFileName += NameSolverStrings.Excel;
				}
				else 
				{                   
					menuFileName  = appCustomPath + saveModuleName + Path.DirectorySeparatorChar;
					officeFileName = menuFileName;

					menuFileName += NameSolverStrings.Menu + Path.DirectorySeparatorChar;

					if(officeType == OfficeType.Word)
						officeFileName += NameSolverStrings.Word + Path.DirectorySeparatorChar;
					if(officeType == OfficeType.Excel)
						officeFileName += NameSolverStrings.Excel + Path.DirectorySeparatorChar;

					if (AllUsersRadioButton.Checked)
					{
						menuFileName += NameSolverStrings.AllUsers;
						officeFileName += NameSolverStrings.AllUsers;
					}
					else if (UserRadioButton.Checked && UsersComboBox.SelectedItem != null)
					{
						string userPath = PathFinder.GetUserPath(UsersComboBox.Text);
						menuFileName += userPath;
						officeFileName += userPath;
					}
				}
			}
			else
			{
				menuFileName  = appCustomPath + saveModuleName + Path.DirectorySeparatorChar;
				officeFileName = menuFileName;

				menuFileName += NameSolverStrings.Menu + Path.DirectorySeparatorChar;

				if(officeType == OfficeType.Word)
					officeFileName += NameSolverStrings.Word + Path.DirectorySeparatorChar;
				if(officeType == OfficeType.Excel)
					officeFileName += NameSolverStrings.Excel + Path.DirectorySeparatorChar;

				menuFileName += loginManager.UserName;
				officeFileName += loginManager.UserName;
			}

			if(officeType == OfficeType.Excel)
				officeFileName += Path.DirectorySeparatorChar + fileName + cbxFileType.Text.Trim();
			if(officeType == OfficeType.Word)
				officeFileName += Path.DirectorySeparatorChar + fileName + cbxFileType.Text.Trim();

			menuFileName += Path.DirectorySeparatorChar + "MagicDocument.menu";
		
			menuItemXmlText = BuildXmlItem(fileName,saveModuleName,officeType == OfficeType.Excel? true : false,fileName);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UserRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            UsersComboBox.Enabled = UserRadioButton.Checked;
		}
		#endregion

	}
}
