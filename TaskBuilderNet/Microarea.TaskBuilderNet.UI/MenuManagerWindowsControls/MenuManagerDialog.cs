using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.WinControls;


namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	/// <summary>
	/// Summary description for MenuManagerDialog.
	/// </summary>
	public partial class MenuManagerDialog : System.Windows.Forms.Form
	{
		protected MenuLoader.CommandsTypeToLoad commandsTypeToLoad = MenuLoader.CommandsTypeToLoad.All;

		protected IPathFinder pathFinder = null;
		protected LoginManager loginManager = null;
		private bool firstactivation = true;

		protected MenuLoader menuLoader = null;
		protected MenuSelections lastSelections = null;
		protected MenuXmlNode selectedCommandNode = null;

		private Thread loadMenuInfoProgressThread = null;
		private ManualResetEvent menuLoadStartEvent = null;
		private ManualResetEvent menuLoadedEvent = null;
		private int loadProgressBarMaximum = 0;
		private int loadProgressBarValue = 0;
		private string loadingMenuInfoText = String.Empty;

		private bool showingFavorites = true;
		private bool showingEnvironment = true;

		protected MenuSearchForm commandsSearchForm = null;


		#region MenuManagerDialog constructors

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuManagerDialog(IPathFinder aPathFinder, LoginManager aLoginManager, bool initResources)
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

			if (initResources)
			{
				this.Text = MenuManagerWindowsControlsStrings.MenuManagerDialogCaption;
				if (DesignMode)
				{
					Icon ico = InstallationData.BrandLoader.GetTbAppManagerApplicationIcon();
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
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuManagerDialog(IPathFinder aPathFinder, LoginManager aLoginManager)
			: this(aPathFinder, aLoginManager, true)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuManagerDialog(MenuLoader aMenuLoader, MenuSelections lastSelectionsToRestore, bool initResources)
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

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuManagerDialog(MenuLoader aMenuLoader, MenuSelections lastSelectionsToRestore)
			: this(aMenuLoader, lastSelectionsToRestore, true)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuManagerDialog(MenuLoader aMenuLoader)
			: this(aMenuLoader, null)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuManagerDialog(MenuLoader aMenuLoader, bool initResources)
			: this(aMenuLoader, null, initResources)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuManagerDialog()
			: this(null)
		{
		}

		#endregion

		#region MenuManagerDialog public properties

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public MenuLoader MenuLoader { get { return menuLoader; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public MenuSelections LastSelections { get { return lastSelections; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlNode SelectedCommandNode { get { return selectedCommandNode; } }

		//--------------------------------------------------------------------------------------------------------------------------------
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

		#region MenuManagerDialog private methods

		//--------------------------------------------------------------------------------------------------------------------------------
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

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShowMenu()
		{
			MenuFormStatusStrip.Visible = false;
			OkButton.Visible = true;
			CancelButton.Visible = true;

			this.MenuMngDlgToolStripContainer.Padding = new Padding(0, 0, 0, this.OkButton.Height + 16);
			this.PerformLayout();

			EnableNormalView();
			EnableEnvironment();
			EnableCommandsSearch();
			EnableRefreshMenuLoader();

			if (menuLoader == null)
				return;

			Cursor.Current = Cursors.WaitCursor;

			SetNormalViewMenu(true);

			if
				(
				menuLoader.MenuInfo == null ||
				!(this.NormalViewToolStripButton.Enabled || this.FavoritesToolStripButton.Enabled || this.EnvironmentToolStripButton.Enabled)
				)
				OkButton.Enabled = false;

			Cursor.Current = Cursors.Default;

			OnMenuShowed();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void HideMenu()
		{
			MenuFormStatusStrip.Visible = true;
			OkButton.Visible = false;
			CancelButton.Visible = false;

			this.MenuMngDlgToolStripContainer.Padding = new Padding(0, 0, 0, 0);
			this.PerformLayout();

			this.NormalViewToolStripButton.Checked = this.NormalViewToolStripMenuItem.Checked = false;
			this.FavoritesToolStripButton.Checked = this.FavoritesToolStripMenuItem.Checked = false;
			this.EnvironmentToolStripButton.Checked = this.EnvironmentToolStripMenuItem.Checked = false;

			EnableNormalView(false);
			EnableFavorites(false);
			EnableEnvironment(false);
			EnableCommandsSearch(false);
			EnableRefreshMenuLoader(false);
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

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RefreshMenu()
		{
			SaveCurrentSelection();

			HideMenu();

			MenuManagerWinCtrl.MenuXmlParser = null;

			LoadMenu();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CreateStatusBarInfoProgressControls(bool createProgressBar)
		{
			MenuFormStatusStrip.CreateInfoProgressControls(createProgressBar);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CreateStatusBarInfoProgressControls()
		{
			CreateStatusBarInfoProgressControls(true);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void DestroyStatusBarInfoProgressControls()
		{
			MenuFormStatusStrip.DestroyInfoProgressControls();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void LoadMenuInfoProgress()
		{
			if (menuLoadStartEvent != null)
				menuLoadStartEvent.WaitOne();

			do
			{
				UpdateMenuFormStatusBar();
			} while (menuLoadedEvent != null && !menuLoadedEvent.WaitOne(10, false));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool SetNormalViewMenu(bool firstShow)
		{
			if (!firstShow && !showingFavorites && !showingEnvironment)
				return true;

			if (menuLoader == null || menuLoader.AppsMenuXmlParser == null)
				return false;

			if (!firstShow)
				SaveCurrentSelection();

			ControlFreezer tmpControlFreezer = new ControlFreezer(MenuManagerWinCtrl);
			tmpControlFreezer.Freeze();

			MenuManagerWinCtrl.MenuXmlParser = menuLoader.AppsMenuXmlParser;

			showingFavorites = false;
			showingEnvironment = false;

			this.NormalViewToolStripButton.Checked = this.NormalViewToolStripMenuItem.Checked = true;
			this.FavoritesToolStripButton.Checked = this.FavoritesToolStripMenuItem.Checked = false;
			this.EnvironmentToolStripButton.Checked = this.EnvironmentToolStripMenuItem.Checked = false;

			RestoreLastSavedSelection();

			tmpControlFreezer.Defreeze();

			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool SetNormalViewMenu()
		{
			return SetNormalViewMenu(false);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool SetEnvironmentMenu()
		{
			if (showingEnvironment)
				return true;

			if (menuLoader == null || menuLoader.EnvironmentXmlParser == null)
				return false;

			SaveCurrentSelection();

			ControlFreezer tmpControlFreezer = new ControlFreezer(MenuManagerWinCtrl);
			tmpControlFreezer.Freeze();

			MenuManagerWinCtrl.MenuXmlParser = menuLoader.EnvironmentXmlParser;

			showingFavorites = false;
			showingEnvironment = true;

			this.NormalViewToolStripButton.Checked = this.NormalViewToolStripMenuItem.Checked = false;
			this.FavoritesToolStripButton.Checked = this.FavoritesToolStripMenuItem.Checked = false;
			this.EnvironmentToolStripButton.Checked = this.EnvironmentToolStripMenuItem.Checked = true;

			RestoreLastSavedSelection();

			tmpControlFreezer.Defreeze();

			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SaveCurrentSelection()
		{
			if (lastSelections == null)
				lastSelections = new MenuSelections();

			if (showingFavorites)
				lastSelections.SetFavoritesSelection
					(
					MenuManagerWinCtrl.CurrentApplicationName,
					MenuManagerWinCtrl.CurrentGroupName,
					MenuManagerWinCtrl.CurrentMenuPath,
					MenuManagerWinCtrl.CurrentCommandPath
					);
			else if (showingEnvironment)
				lastSelections.SetEnvironmentSelection
					(
					MenuManagerWinCtrl.CurrentApplicationName,
					MenuManagerWinCtrl.CurrentGroupName,
					MenuManagerWinCtrl.CurrentMenuPath,
					MenuManagerWinCtrl.CurrentCommandPath
					);
			else
				lastSelections.SetApplicationsSelection
					(
					MenuManagerWinCtrl.CurrentApplicationName,
					MenuManagerWinCtrl.CurrentGroupName,
					MenuManagerWinCtrl.CurrentMenuPath,
					MenuManagerWinCtrl.CurrentCommandPath
					);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RestoreLastSavedSelection()
		{
			if (lastSelections == null)
			{
				MenuManagerWinCtrl.Select(null);
				return;
			}

			MenuParserSelection selectionToRestore = null;
			if (showingFavorites)
				selectionToRestore = lastSelections.FavoritesSelection;
			else if (showingEnvironment)
				selectionToRestore = lastSelections.EnvironmentSelection;
			else
				selectionToRestore = lastSelections.ApplicationsSelection;

			Cursor.Current = Cursors.WaitCursor;

			MenuManagerWinCtrl.Select(selectionToRestore);

			// Set Cursor.Current to Cursors.Default to display the appropriate cursor for each control
			Cursor.Current = Cursors.Default;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected virtual void OnMenuShowed()
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
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

		//--------------------------------------------------------------------------------------------------------------------------------
		protected virtual bool IsXmlParserPopulatedWithCommands(MenuXmlParser aMenuXmlParser)
		{
			return (aMenuXmlParser != null && aMenuXmlParser.HasCommandDescendantsNodes);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected virtual void OnSelectedCommandChanging(MenuMngCtrlCancelEventArgs e)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected virtual void OnSelectedCommandChanged(MenuMngCtrlEventArgs e)
		{
		}

		//--------------------------------------------------------------------
		protected virtual bool OnOk()
		{
			SaveCurrentSelection();

			selectedCommandNode = MenuManagerWinCtrl.CurrentCommandNode;

			return (selectedCommandNode != null);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnableNormalView(bool enable)
		{
			this.NormalViewToolStripButton.Enabled = this.NormalViewToolStripMenuItem.Enabled = enable;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnableNormalView()
		{
			EnableNormalView((menuLoader != null && menuLoader.MenuInfo != null && IsXmlParserPopulatedWithCommands(menuLoader.AppsMenuXmlParser)));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnableFavorites(bool enable)
		{
			this.FavoritesToolStripButton.Enabled = this.FavoritesToolStripMenuItem.Enabled = enable;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnableEnvironment(bool enable)
		{
			this.EnvironmentToolStripButton.Enabled = this.EnvironmentToolStripMenuItem.Enabled = enable;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnableEnvironment()
		{
			EnableEnvironment((menuLoader != null && menuLoader.MenuInfo != null && IsXmlParserPopulatedWithCommands(menuLoader.EnvironmentXmlParser)));

			this.EnvironmentToolStripButton.Visible = this.EnvironmentToolStripMenuItem.Visible = (menuLoader == null || menuLoader.IsEnvironmentStandAlone);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnableCommandsSearch(bool enable)
		{
			this.SearchToolStripButton.Enabled = this.SearchToolStripMenuItem.Enabled = enable;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnableCommandsSearch()
		{
			EnableCommandsSearch(menuLoader != null && menuLoader.MenuInfo != null);

			this.SearchToolStripButton.Visible = this.SearchToolStripMenuItem.Visible = (loginManager != null && loginManager.LoginManagerState == LoginManagerState.Logged);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnableRefreshMenuLoader(bool enable)
		{
			this.RefreshMenuLoaderToolStripButton.Enabled = this.RefreshMenuLoaderToolStripMenuItem.Enabled = enable;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnableRefreshMenuLoader()
		{
			EnableRefreshMenuLoader(menuLoader != null);

			this.RefreshMenuLoaderToolStripButton.Visible = this.RefreshMenuLoaderToolStripMenuItem.Visible = (loginManager != null && loginManager.LoginManagerState == LoginManagerState.Logged);
		}

		delegate void UpdateMenuFormStatusBarCallback();
		//--------------------------------------------------------------------------------------------------------------------------------
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

				}
			}
		}

		#endregion

		#region MenuManagerDialog overridden protected methods

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);

			if (menuLoader == null)
				MenuManagerWinCtrl.DisplayLoadingPanel();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
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
				if
					(
					menuLoader.MenuInfo == null ||
					(
					!IsXmlParserPopulatedWithCommands(menuLoader.AppsMenuXmlParser) &&
					!IsXmlParserPopulatedWithCommands(menuLoader.EnvironmentXmlParser)
					)
					)
				{
					MessageBox.Show
						(
						MenuManagerWindowsControlsStrings.NoCommandsAvailableWarningMessage,
						MenuManagerWindowsControlsStrings.NoCommandsAvailableWarningCaption,
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning
						);

					//LUCA Commentata la chiusura automatica: aveva come effetto collaterale che 
					//in mancanza di elementi di office il mini menu manager si chiudeva ed impediva il 
					//Refresh degli elementi (Non dovrebbe creare problemi)
					//this.DialogResult = DialogResult.Cancel;
					//this.Close();
					//return;
				}
				ShowMenu();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			// Invoke base class implementation
			base.OnClosing(e);

			if (commandsSearchForm != null)
				commandsSearchForm.Close();
		}

		#endregion

		#region MenuManagerDialog event handlers

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

			if (menuLoader.MenuInfo != null)
			{
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

				ArrayList environmentMenuErrorMsgs = menuLoader.MenuInfo.EnvironmentMenuLoadErrorMessages;
				if (environmentMenuErrorMsgs != null && environmentMenuErrorMsgs.Count > 0)
				{
					foreach (string errorMsg in environmentMenuErrorMsgs)
						MessageBox.Show
							(
							errorMsg,
							MenuManagerWindowsControlsStrings.MenuLoadErrorCaption,
							MessageBoxButtons.OK,
							MessageBoxIcon.Error
							);
				}
			}
		}

		//----------------------------------------------------------------------------
		private void LoadFavoritesMenuStarted(object sender, MenuParserEventArgs e)
		{
			this.MenuFormStatusStrip.IsProgressBarVisible = true;
			loadProgressBarMaximum = e.Counter;
			loadProgressBarValue = 0;

			loadingMenuInfoText = MenuManagerWindowsControlsStrings.LoadFavoritesMenuText;
		}

		//----------------------------------------------------------------------------
		private void LoadFavoritesMenuAppIndexChanged(object sender, MenuParserEventArgs e)
		{
			loadProgressBarValue = e.Counter;
		}

		//----------------------------------------------------------------------------
		private void LoadFavoritesMenuEnded(object sender, MenuParserEventArgs e)
		{
			loadProgressBarValue = loadProgressBarMaximum;

			if (e != null && e.Parser != null)
			{
				ArrayList favoritesMenuErrorMsgs = e.Parser.LoadErrorMessages;
				if (favoritesMenuErrorMsgs != null && favoritesMenuErrorMsgs.Count > 0)
				{
					foreach (string errorMsg in favoritesMenuErrorMsgs)
						MessageBox.Show
							(
							errorMsg,
							MenuManagerWindowsControlsStrings.MenuLoadErrorCaption,
							MessageBoxButtons.OK,
							MessageBoxIcon.Error
							);
				}
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

				//LUCA Commentata la chiusura automatica: aveva come effetto collaterale che 
				//in mancanza di elementi di office il mini menu manager si chiudeva ed impediva il 
				//Refresh degli elementi
				//this.DialogResult = DialogResult.Cancel;
				//this.Close();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void NormalViewToolStripButton_Click(object sender, EventArgs e)
		{
			SetNormalViewMenu();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void NormalViewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetNormalViewMenu();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EnvironmentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetEnvironmentMenu();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SearchToolStripButton_Click(object sender, EventArgs e)
		{
			SearchCommand();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SearchToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SearchCommand();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RefreshMenuLoaderToolStripButton_Click(object sender, EventArgs e)
		{
			RefreshMenu();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void RefreshMenuLoaderToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			RefreshMenu();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuManagerWinCtrl_SelectedCommandChanging(object sender, MenuMngCtrlCancelEventArgs e)
		{
			OnSelectedCommandChanging(e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuManagerWinCtrl_SelectedMenuChanged(object sender, MenuMngCtrlTreeViewEventArgs e)
		{
			OkButton.Enabled = MenuManagerWinCtrl.CurrentCommandNode != null;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuManagerWinCtrl_SelectedCommandChanged(object sender, MenuMngCtrlEventArgs e)
		{
			OkButton.Enabled = (e.ItemObject != null && e.ItemObject.Length > 0);

			OnSelectedCommandChanged(e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuManagerWinCtrl_RunCommand(object sender, MenuMngCtrlEventArgs e)
		{
			if (e == null || e.ItemType.IsUndefined)
				return;

			this.DialogResult = OnOk() ? DialogResult.OK : DialogResult.None;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OkButton_Click(object sender, System.EventArgs e)
		{
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
			else if (e.Parser == menuLoader.EnvironmentXmlParser)
				SetEnvironmentMenu();

			MenuManagerWinCtrl.SelectCommandNode(e.Node);

			if (sender == commandsSearchForm && commandsSearchForm != null)
				commandsSearchForm.WindowState = FormWindowState.Minimized;

			this.Activate();
		}

		#endregion

	}

	/// <summary>
	/// Summary description for MenuOfficeFilesDialog.
	/// </summary>
	public class MenuOfficeFilesDialog : MenuManagerDialog
	{
		#region MenuOfficeFilesDialog constructors

		//--------------------------------------------------------------------
		public MenuOfficeFilesDialog(IPathFinder aPathFinder, LoginManager aLoginManager, bool initResources)
			: base(aPathFinder, aLoginManager, initResources)
		{
			if (initResources)
			{
				this.Text = MenuManagerWindowsControlsStrings.MenuOfficeFilesDialogCaption;

				Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Icons.MenuOfficeFilesDialog.ico");
				if (iconStream != null)
				{
					this.Icon = new System.Drawing.Icon(iconStream);
				}
			}

			commandsTypeToLoad = MenuLoader.CommandsTypeToLoad.OfficeItem;
		}

		//--------------------------------------------------------------------
		public MenuOfficeFilesDialog(IPathFinder aPathFinder, LoginManager aLoginManager)
			: this(aPathFinder, aLoginManager, true)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuOfficeFilesDialog(MenuLoader aMenuLoader, MenuSelections lastSelectionsToRestore, bool initResources)
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

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuOfficeFilesDialog(MenuLoader aMenuLoader, MenuSelections lastSelectionsToRestore)
			: this(aMenuLoader, lastSelectionsToRestore, true)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuOfficeFilesDialog(MenuLoader aMenuLoader)
			: this(aMenuLoader, null)
		{
		}

		#endregion

		//--------------------------------------------------------------------
		public virtual string SelectedFilename
		{
			get
			{
				if (menuLoader == null || menuLoader.MenuInfo == null || selectedCommandNode == null)
					return String.Empty;

				return menuLoader.MenuInfo.GetOfficeItemFileName(selectedCommandNode, pathFinder, loginManager.PreferredLanguage);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override bool IsXmlParserPopulatedWithCommands(MenuXmlParser aMenuXmlParser)
		{
			return (aMenuXmlParser != null && aMenuXmlParser.HasOfficeItemsDescendantsNodes);
		}

		//--------------------------------------------------------------------
		protected override bool OnOk()
		{
			if (!base.OnOk())
				return false;

			if (selectedCommandNode != null && !selectedCommandNode.IsOfficeItem)
			{
				selectedCommandNode = null;
				return false;
			}

			MenuXmlNode.OfficeItemApplication application = selectedCommandNode.GetOfficeApplication();
			if (application == MenuXmlNode.OfficeItemApplication.Undefined)
				return false;

			return true;
		}
	}

	/// <summary>
	/// Summary description for MenuExcelFilesDialog.
	/// </summary>
	public class MenuExcelFilesDialog : MenuOfficeFilesDialog
	{
		#region MenuExcelFilesDialog constructors

		//--------------------------------------------------------------------
		public MenuExcelFilesDialog(IPathFinder aPathFinder, LoginManager aLoginManager)
			: base(aPathFinder, aLoginManager, false)
		{
			this.Text = MenuManagerWindowsControlsStrings.MenuExcelFilesDialogCaption;

			Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Icons.MenuExcelFilesDialog.ico");
			if (iconStream != null)
			{
				this.Icon = new System.Drawing.Icon(iconStream);
			}

			commandsTypeToLoad = MenuLoader.CommandsTypeToLoad.ExcelItem;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuExcelFilesDialog(MenuLoader aMenuLoader, MenuSelections lastSelectionsToRestore)
			: this
			(
			(aMenuLoader != null) ? aMenuLoader.PathFinder : null,
			(aMenuLoader != null) ? aMenuLoader.LoginManager : null
			)
		{
			menuLoader = aMenuLoader;
			lastSelections = lastSelectionsToRestore;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuExcelFilesDialog(MenuLoader aMenuLoader)
			: this(aMenuLoader, null)
		{
		}

		#endregion

		//--------------------------------------------------------------------
		public override string SelectedFilename
		{
			get
			{
				if (menuLoader == null || menuLoader.MenuInfo == null || selectedCommandNode == null)
					return String.Empty;

				return MenuInfo.GetOfficeItemFileName(
					selectedCommandNode.ItemObject,
					selectedCommandNode.CommandSubType,
					MenuXmlNode.OfficeItemApplication.Excel, 
					pathFinder,
					loginManager.PreferredLanguage
					);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override bool IsXmlParserPopulatedWithCommands(MenuXmlParser aMenuXmlParser)
		{
			return (aMenuXmlParser != null && aMenuXmlParser.HasExcelItemsDescendantsNodes);
		}

		//--------------------------------------------------------------------
		protected override bool OnOk()
		{
			if (!base.OnOk())
				return false;

			if (selectedCommandNode != null && !selectedCommandNode.IsExcelItem)
			{
				selectedCommandNode = null;
				return false;
			}

			return true;
		}
	}

	/// <summary>
	/// Summary description for MenuWordFilesDialog.
	/// </summary>
	public class MenuWordFilesDialog : MenuOfficeFilesDialog
	{
		#region MenuWordFilesDialog constructors

		//--------------------------------------------------------------------
		public MenuWordFilesDialog(IPathFinder aPathFinder, LoginManager aLoginManager)
			: base(aPathFinder, aLoginManager, false)
		{
			this.Text = MenuManagerWindowsControlsStrings.MenuWordFilesDialogCaption;

			Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Icons.MenuWordFilesDialog.ico");
			if (iconStream != null)
			{
				this.Icon = new System.Drawing.Icon(iconStream);
			}

			commandsTypeToLoad = MenuLoader.CommandsTypeToLoad.WordItem;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuWordFilesDialog(MenuLoader aMenuLoader, MenuSelections lastSelectionsToRestore)
			: this
			(
			(aMenuLoader != null) ? aMenuLoader.PathFinder : null,
			(aMenuLoader != null) ? aMenuLoader.LoginManager : null
			)
		{
			menuLoader = aMenuLoader;
			lastSelections = lastSelectionsToRestore;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuWordFilesDialog(MenuLoader aMenuLoader)
			: this(aMenuLoader, null)
		{
		}

		#endregion
		//--------------------------------------------------------------------
		public override string SelectedFilename
		{
			get
			{
				if (menuLoader == null || menuLoader.MenuInfo == null || selectedCommandNode == null)
					return String.Empty;

				return MenuInfo.GetOfficeItemFileName(
					selectedCommandNode.ItemObject,
					selectedCommandNode.CommandSubType,
					MenuXmlNode.OfficeItemApplication.Word,
					pathFinder, 
					loginManager.PreferredLanguage
					);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override bool IsXmlParserPopulatedWithCommands(MenuXmlParser aMenuXmlParser)
		{
			return (aMenuXmlParser != null && aMenuXmlParser.HasWordItemsDescendantsNodes);
		}

		//--------------------------------------------------------------------
		protected override bool OnOk()
		{
			if (!base.OnOk())
				return false;

			if (selectedCommandNode != null && !selectedCommandNode.IsWordItem)
			{
				selectedCommandNode = null;
				return false;
			}
			return true;
		}
	}
}
